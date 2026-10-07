<div align="center">

# 📦 Addressables

**How content is packed, loaded, released and (one day) downloaded — and what is built so far**

![Packages](https://img.shields.io/badge/Addressables-1.29.0-1f6feb)
![DI](https://img.shields.io/badge/VContainer-1.19.0-8250df)
![Status](https://img.shields.io/badge/Built-I0_·_I1-d29922)

<sub>[README](../../README.md) · [ProductionV2 §4](ProductionV2.md#-4-infrastructure) · [Decision Log](ProductionV2.md#-14-decision-log)</sub>

</div>

> [!IMPORTANT]
> **The model in one line:** groups decide **packing**, keys decide **loading**, labels decide **downloading**. Code loads one asset at a time by key, every handle belongs to the scope that loaded it, and closing the scope releases them all (D65, D66).

| # | Section |
|---|---|
| 1 | [Rules](#1-rules) |
| 2 | [Groups](#2-groups) |
| 3 | [Keys](#3-keys) |
| 4 | [Loading & lifetime](#4-loading--lifetime) |
| 5 | [Leaks](#5-leaks) |
| 6 | [Labels & download flow](#6-labels--download-flow) |
| 7 | [Profiles](#7-profiles) |
| 8 | [How to add content](#8-how-to-add-content) |
| 9 | [Status](#9-status) |

---

## 1. Rules
- `IAssetLoader` is the **only** door to assets. Nobody calls `Addressables.*` outside `Game.Infrastructure`.
- Scenes go through `ISceneLoader` only (I2), never `SceneManager` or `Addressables.LoadSceneAsync` directly.
- No `Resources` for game content (the one exception is DOTween's settings, D91).
- No direct asset references in scenes for data-driven content: the board, blocks and popups are built from code with assets loaded by key.
- Callers never release: an asset lives as long as the scope that loaded it.

## 2. Groups
A group is a **packing** unit: which assets share a bundle. Code never names a group.

| Group | Holds | Lives as long as |
|---|---|---|
| `Boot` | `GameConfig`, palette | the whole run (root scope) |
| `Main` | Home and settings content, the Main scene | the Main scope |
| `Gameplay` | Presentation assets, prefabs, materials, the Gameplay scene | the Gameplay scope |
| `Levels` | Level JSONs (`TextAsset`) | the scope that loaded the level |
| `Popups` | Popup prefabs | the scope that opened the popup |

**Why by lifetime:** loading one asset brings its whole bundle into memory, and the bundle stays until **every** asset loaded from it is released. Mixing Main and Gameplay assets in one bundle would keep Gameplay's bundle alive on Home. Grouped by lifetime, closing a scope really empties its bundles; I6 checks this with the Event Viewer.

**Bundle mode:** "Pack Together" (one bundle per group) by default. `Levels` may use "Pack Separately" so a remote update of one level re-downloads only that level; settled in I3.

## 3. Keys
- Every addressable asset has a stable string key; code holds keys, never asset references.
- **Level key = the JSON's file name** (`Level_1`), the same key the Level Editor saves (V1 D52).
- The ordered level list is in `GameConfig` (I4), so no label is needed to find "all levels".
- Key names for prefabs, palette and popups are fixed when each phase adds them (§9).

## 4. Loading & lifetime
```
LifetimeScope (root / Main / Gameplay)
   └── AssetScope : IAssetLoader      one per scope (Lifetime.Scoped)
          └── IAssetSource            AddressablesAssetSource: Addressables.LoadAssetAsync<T>(key)
```

| Type | Role |
|---|---|
| `IAssetLoader` | What consumers inject: `LoadAsync<T>(key, cancellation)`, `T : UnityEngine.Object`. Unknown key throws |
| `AssetScope` | Keeps every handle it loads; `Dispose` releases all once. `OpenHandles` counts them |
| `IAssetSource` | The raw door. A failed or cancelled load releases its own handle, so only a loaded asset reaches the scope |
| `IAssetHandle` | One loaded asset plus the right to release it |

- `AssetScope` is registered `Lifetime.Scoped` in the root installer: every `LifetimeScope` that resolves `IAssetLoader` gets **its own** scope, and VContainer disposes it when that `LifetimeScope` closes.
- Scene unload → its `LifetimeScope` is destroyed → its `AssetScope` is disposed → its handles are released.
- One asset is loaded one at a time; there is no batch or label load in game code (a label load returns an unordered list and makes release tracking harder).

### Scenes (D114)
- `ISceneLoader` (`ReplaceContentSceneAsync(key)`, `UnloadContentSceneAsync()`) is the only way to change scenes; keys live in `SceneKeys`.
- **Bootstrap stays loaded** for the whole run and is the only scene in Build Settings; the root scope lives with it (no `DontDestroyOnLoad`).
- One **content scene** at a time is loaded **additively** next to it and made the active scene, so objects created from code land in it and are destroyed with it. Replacing it unloads the current one first.
- The loaded scene's `LifetimeScope` gets the root scope as parent through `LifetimeScope.EnqueueParent`; a scene played alone in the Editor has no parent and still runs.
- Unloading: `Addressables.UnloadSceneAsync` → the scene's scope is destroyed → its `AssetScope` releases its handles.
- Content scenes are addressable under their key (`Gameplay`; `Main` from M2) and **not** in Build Settings.

## 5. Leaks
- **A leak is a handle that would outlive its scope.** Normal handles are released by `Dispose`, so a scope closing with handles is expected, not a leak.
- A load that finishes **after** its scope closed is released at once, logged as a warning in the Editor and development builds, and the caller sees a cancel.
- Tests assert `OpenHandles == 0` after `Dispose` (fake source).
- Bundle-level check: Addressables Event Viewer after a full Gameplay → Bootstrap round trip, zero bundles left (I6).

## 6. Labels & download flow
Labels are used **only** to download, never to load in game code.

- Everything that would live on a server gets one label (e.g. `remote`); one call downloads it all.
- The bootstrapper runs the server path even without a server (D65):
  1. `Addressables.InitializeAsync` — ✅ I0 (`IContentInitializer`)
  2. `CheckForCatalogUpdates` → update catalogs — I3
  3. `GetDownloadSizeAsync(label)` — 0 locally — I3
  4. `DownloadDependenciesAsync(label)` — nothing to fetch locally — I3

## 7. Profiles
| Profile | Build / load path | Used |
|---|---|---|
| **Local** | local build path, local load path | ✅ always |
| **Remote** | remote build path, load path empty until a server exists | defined only |

Moving to a server is a profile change plus marking groups remote, not a code change. Set up in I3.

## 8. How to add content
1. Put the asset in the group matching its lifetime (§2).
2. Give it a key; add the key where code reads it (config, a key constant set by its phase).
3. Load it through the `IAssetLoader` injected in the scope that should own it. Never release it by hand.
4. If it should be downloadable later, give it the `remote` label.
5. Building a player: build Addressables content first (`Build → New Build → Default Build Script`), then the player.

## 9. Status
| Phase | What | State |
|---|---|---|
| I0 | Packages, root scope, `Bootstrapper` → `InitializeAsync` | ✅ |
| I1 | `IAssetLoader`, `AssetScope`, `AddressablesAssetSource`, leak report | ✅ |
| I2 | `ISceneLoader`, Gameplay child scope (Main in M2), scene unload disposes scope | ✅ |
| I3 | Groups and keys, Local / Remote profiles, content update flow | ⏳ |
| I4 | `GameConfig`, Addressables level source, `PresentationAssets` from Addressables | ⏳ |
| I6 | Event Viewer round trip, first APK | ⏳ |
