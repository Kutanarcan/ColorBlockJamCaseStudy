<div align="center">

# 📦 Addressables

**How content is packed, loaded, released and (one day) downloaded — and what is built so far**

![Packages](https://img.shields.io/badge/Addressables-1.29.0-1f6feb)
![DI](https://img.shields.io/badge/VContainer-1.19.0-8250df)
![Status](https://img.shields.io/badge/Built-I0_–_I3-d29922)

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
| 8 | [Editor setup](#8-editor-setup) |
| 9 | [How to add content](#9-how-to-add-content) |
| 10 | [Status](#10-status) |

---

## 1. Rules
- `IAssetLoader` is the **only** door to assets. Nobody calls `Addressables.*` outside `Game.Infrastructure`.
- Scenes go through `ISceneLoader` only (I2), never `SceneManager` or `Addressables.LoadSceneAsync` directly.
- No `Resources` for game content (the one exception is DOTween's settings, D91).
- No direct asset references in scenes for data-driven content: the board, blocks and popups are built from code with assets loaded by key.
- Callers never release: an asset lives as long as the scope that loaded it.

## 2. Groups
A group is a **packing** unit: which assets share a bundle. Code never names a group.

| Group | Holds | Lives as long as | Paths | Bundle mode | Created |
|---|---|---|---|---|---|
| `Boot` | `GameConfig` | the whole run (root scope) | Local | Pack Together | ✅ I4 |
| `Main` | Home and settings content, the Main scene | the Main scope | Local | Pack Together | M2 |
| `Gameplay` | The Gameplay scene; `PresentationAssets` and, through it, prefabs, meshes, materials and the palette | the Gameplay scope | Local | Pack Together | ✅ I3 (scene) · I4 (presentation) |
| `Levels` | Level JSONs (`TextAsset`), `Level_1`–`Level_6` | the scope that loaded the level | **Remote** | **Pack Separately** | ✅ I3 |
| `Popups` | Popup prefabs | the scope that opened the popup | Local | Pack Together | U2 |

**Why by lifetime:** loading one asset brings its whole bundle into memory, and the bundle stays until **every** asset loaded from it is released. Mixing Main and Gameplay assets in one bundle would keep Gameplay's bundle alive on Home. Grouped by lifetime, closing a scope really empties its bundles; I6 checks this with the Event Viewer.

**A group is created when its first asset exists** (D115): no empty groups, same rule as assemblies and scenes.

**Bundle mode (D115):** "Pack Together" (one bundle per group), except `Levels`: "Pack Separately", one bundle per level, so a remote update of one level re-downloads only that level and loading a level does not bring the others into memory.

## 3. Keys
- Every addressable asset has a stable string key; code holds keys, never asset references.
- **Level key = the JSON's file name** (`Level_1`), the same key the Level Editor saves (V1 D52).
- The ordered level list is in `GameConfig` (I4), so no label is needed to find "all levels".
- Key names for prefabs, palette and popups are fixed when each phase adds them (§10).

| Key | Asset | Group | Constant |
|---|---|---|---|
| `Gameplay` | `Assets/Scenes/Gameplay.unity` | `Gameplay` | `SceneKeys.Gameplay` |
| `Level_1` … `Level_6` | `Assets/Levels/Level_N.json` | `Levels` | `GameConfig.LevelKeys` (1–5 in play order; 6 only through the Level Editor) |
| `GameConfig` | `Assets/ScriptableObjects/Config/GameConfig.asset` | `Boot` | `GameConfig.Key` |
| `PresentationAssets` | `Assets/ScriptableObjects/Presentation/PresentationAssets.asset` (pulls in its prefabs, meshes, materials, palette, modifier views) | `Gameplay` | `PresentationAssets.Key` |

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

| Label | On | Constant |
|---|---|---|
| `remote` | everything that would come from a server: the `Levels` group | `ContentLabels.Remote` |

The bootstrapper runs the server path even without a server (D65). `ContentUpdate` (pure, tested with a fake) drives `IContentDelivery` (`AddressablesContentDelivery`):

| # | Step | Locally | Phase |
|---|---|---|---|
| 1 | `Addressables.InitializeAsync` (`IContentInitializer`) | loads the local catalog | ✅ I0 |
| 2 | `CheckForCatalogUpdates` | no changed catalog | ✅ I3 |
| 3 | `UpdateCatalogs` — only when step 2 found some; `autoCleanBundleCache: true` | skipped | ✅ I3 |
| 4 | `GetDownloadSizeAsync(remote)` | 0 | ✅ I3 |
| 5 | `DownloadDependenciesAsync(remote)` — always runs | nothing to fetch | ✅ I3 |

- Every handle is released once its answer is read; the catalog list is copied first, because it belongs to the operation.
- A download failure is an exception today; a retry / offline popup is out of V2.

### How a remote asset reaches the game
- **The catalog** maps every key to a bundle name and that bundle's **hash**. The hash changes whenever the bundle's content changes.
- **Loading** (`LoadAssetAsync`) looks up the bundle and its hash in the catalog, then:
  - the bundle with that hash is in the device's AssetBundle cache (`Use Asset Bundle Cache` ✓ on our groups) → **loaded from disk**, no download;
  - it is not in the cache, or only an older hash is → **downloaded, written to the cache, then loaded.**
- So a load downloads on its own when it must. **`DownloadDependenciesAsync` is only a pre-download**: the wait happens at start-up with progress, not in the middle of a level.
- **`GetDownloadSizeAsync` counts only what is not cached yet**: once everything is downloaded it returns 0.
- **New content arrives through the catalog.** A content update publishes a new catalog with new hashes; until the app takes that catalog, it keeps asking for the old hashes and sees the old content. With `Only update catalogs manually` the bootstrapper's steps 2–3 are what take it.
- **Old versions are cleaned** when the catalog is updated (`autoCleanBundleCache: true`): cached bundles the new catalog no longer uses are deleted, so every update does not leave the previous levels on the device.
- **Local bundles** (`Built-In` location, inside the player's StreamingAssets) are never downloaded or cached; they are read straight from the APK. Our `Default` profile points Remote at `Built-In`, so today the `Levels` bundles are local: size 0, download empty.

## 7. Profiles
Groups never name a URL: they use the profile's **Local** or **Remote** bundle location, and the **profile** decides where Remote points (D115). The `Default` profile cannot be renamed; it is the local one.

| Profile | Local location | Remote location | Used |
|---|---|---|---|
| **Default** | `Built-In` | `Built-In` (or `Custom` with the built-in paths below) — remote bundles are built into the player | ✅ active |
| **Remote** | `Built-In` | `Custom`: build `ServerData/[BuildTarget]`, load path empty until a server exists | defined only |

Built-in paths, if `Custom` is needed for Default's Remote: build `[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]`, load `{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]`.

Not used: `Editor Hosted` (Unity's local hosting service, for testing real downloads over the LAN) and `Cloud Content Delivery` (Unity's CDN); the slice has no server.

Moving to a server: fill the Remote profile's load path, make it active, build. No code and no group change.

## 8. Editor setup
Done by hand once (D115); the result is committed under `Assets/AddressableAssetsData/`.

**Profiles** (`Window → Asset Management → Addressables → Profiles`)
1. `Default`: Bundle Locations → Local = `Built-In`, Remote = `Built-In` (or `Custom` with the built-in paths in §7).
2. `Create → Profile`, name it `Remote`: Local = `Built-In`, Remote = `Custom`, build path `ServerData/[BuildTarget]`, load path empty.
3. Keep `Default` active.

**Labels** (`Groups → Tools → Labels`)
4. Add `remote`.

**Groups** (`Window → Asset Management → Addressables → Groups`)
5. Rename `Default Local Group` to `Gameplay`. It holds the Gameplay scene, key `Gameplay`. Paths: Local. Bundle mode: Pack Together.
6. Create `Levels` (`Create → Group → Packed Assets`). Drag `Assets/Levels/Level_1.json` … `Level_6.json` into it.
7. Select the six entries → right click → `Simplify Addressable Names`: keys become `Level_1` … `Level_6`.
8. Give the six entries the `remote` label.
9. `Levels` group inspector → `Content Packing & Loading`: Build & Load Paths = `Remote`, Bundle Mode = `Pack Separately`.

**Settings** (`Assets/AddressableAssetsData/AddressableAssetSettings`)
10. Catalog → `Only update catalogs manually` ✓ (the bootstrapper does it, step 2–3).
11. Catalog → `Build Remote Catalog` ✓, Build & Load Paths = `Remote` (locally it points to the local folder, §7).
12. Play Mode Script: `Use Asset Database` while developing; `Use Existing Build` to test real bundles (needs a content build).

**Boot group and presentation** (I4)
13. Create `Boot` (`Create → Group → Packed Assets`). Paths: Local. Bundle mode: Pack Together.
14. Drag `Assets/ScriptableObjects/Config/GameConfig.asset` into it and set its key to `GameConfig` (or `Simplify Addressable Names`).
15. Drag `Assets/ScriptableObjects/Presentation/PresentationAssets.asset` into `Gameplay`, key `PresentationAssets`. Its references come along as dependencies; do not add them one by one.

**Play Mode Script** (Groups window → `Play Mode Script`) decides where Addressables takes assets from when you press Play in the Editor. Game code is the same in every mode; only the loading path underneath changes.

| Script | Takes assets from | Use it for |
|---|---|---|
| **Use Asset Database (fastest)** | The project files (`AssetDatabase`); no bundles, no build | Daily work: keys, labels and code flow |
| **Simulate Groups (advanced)** | Still `AssetDatabase`, but imitates the group / bundle layout: dependencies and bundle load / release show in the Event Viewer | Looking at bundle dependencies without a build |
| **Use Existing Build** | The last content build's real bundles and catalog — the same path as on a device | Real behaviour in the Editor: bundle size, memory, catalog. **Needs a content build first** |

- `Use Asset Database` can hide mistakes: an asset left out of a group, a wrong bundle mode, a bundle never released. Everything is in the project anyway.
- The I6 "zero bundles left" check is done with **Use Existing Build**, where the Event Viewer shows real reference counts.
- The content update flow (§6) always answers "nothing new" under `Use Asset Database`; a real catalog exists only with an existing build or on a device.
- The choice is stored in EditorPrefs: per machine, not in git. Player builds always use real bundles.

## 9. How to add content
1. Put the asset in the group matching its lifetime (§2). Create the group if it is the first asset of that lifetime.
2. Give it a key; add the key where code reads it (config, a key constant set by its phase).
3. Load it through the `IAssetLoader` injected in the scope that should own it. Never release it by hand.
4. If it should be downloadable later, give it the `remote` label and put it in a group with `Remote` paths.
5. Building a player: build Addressables content first (`Build → New Build → Default Build Script`), then the player.

## 10. Status
**What runs today** (Bootstrap → Play):
```
Bootstrap scene ── RootLifetimeScope (RootInstaller)
   └── Bootstrapper (entry point)
         1. IContentInitializer   InitializeAsync
         2. ContentUpdate         catalogs → (update + clean) → size(remote) → download(remote)
         3. IAssetLoader          GameConfig → LoadedConfig   (root AssetScope, lives for the run)
         4. ISceneLoader          ReplaceContentSceneAsync("Gameplay")  additive, active
                                     └── GameplayLifetimeScope (child of root, own AssetScope)
                                           GameplayEntry: PresentationAssets + level (LevelKeys[0]) by key → session, views, play; ticks through VContainer
```

| Phase | What | State |
|---|---|---|
| I0 | Packages, root scope, `Bootstrapper` → `InitializeAsync` | ✅ |
| I1 | `IAssetLoader`, `AssetScope`, `AddressablesAssetSource`, leak report | ✅ |
| I2 | `ISceneLoader`, Gameplay child scope (Main in M2), scene unload disposes scope | ✅ |
| I3 | `Gameplay` and `Levels` groups, `remote` label, Local / Remote profiles, content update flow | ✅ |
| I4 | a: `GameConfig`, `LoadedConfig`, `Boot` group, `AssetLevelSource` · b: `GameplayLifetimeScope`, `PresentationAssets` from Addressables | a: ✅ · b: ✅ code · setup & tests pending |
| I6 | Event Viewer round trip, first APK | ⏳ |
