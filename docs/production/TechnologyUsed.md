<div align="center">

# 🧰 Technology Used

**Every engine feature, package, technique and tool in the project, what it does here, and why it was chosen**

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2_LTS-black?logo=unity&logoColor=white)
![Render](https://img.shields.io/badge/Render-Built--in_RP-6e7681)
![Backend](https://img.shields.io/badge/Android-IL2CPP-6e7681)
![DI](https://img.shields.io/badge/DI-VContainer-6e7681)
![Content](https://img.shields.io/badge/Content-Addressables-6e7681)

<sub>[README](../../README.md) · [ProductionV2](ProductionV2.md) · [ProductionV1](ProductionV1.md) · [Addressables](Addressables.md) · [Performance](Performance.md) · [Extending](Extending.md)</sub>

</div>

> [!NOTE]
> Each row names the decision behind it (`Dn`, see the decision logs in [ProductionV1](ProductionV1.md#-16-decision-log) and [ProductionV2](ProductionV2.md#-14-decision-log)). The rule throughout: the simplest thing that solves a concrete problem, and nothing added "for later" without a reason.

| # | Section |
|---|---|
| 1 | [Engine & platform](#1-engine--platform) |
| 2 | [Packages](#2-packages) |
| 3 | [Architecture techniques](#3-architecture-techniques) |
| 4 | [Rendering & visuals](#4-rendering--visuals) |
| 5 | [Input & feel](#5-input--feel) |
| 6 | [Content & data](#6-content--data) |
| 7 | [Level Editor](#7-level-editor) |
| 8 | [Testing & measurement](#8-testing--measurement) |
| 9 | [Tools & workflow](#9-tools--workflow) |
| 10 | [Left out on purpose](#10-left-out-on-purpose) |

---

## 1. Engine & platform

| Technology | Used for | Why |
|---|---|---|
| **Unity 2022.3.62f2 LTS** | The whole project | Required by the brief |
| **Built-in Render Pipeline** | Rendering | The brief names no render pipeline, so the project stays on Unity's default. Custom shaders are hand-written for it (§4) |
| **Android, IL2CPP** | The APK | Brief deliverable. IL2CPP is the shipping backend, so performance was measured on it (D109) |
| **Portrait 1080×1920 reference, 16:9 to 20:9** | Layout, camera fit | Brief constraint. The camera fit and the canvas scaler cover tall phones (D73, D137) |
| **`Application.targetFrameRate = 60`** | Frame pacing on mobile | Set once by the root scope, so both starts get it |
| **Assembly definitions** | Layer walls | A Unity API call in the logic is a compile error, not a review comment (§3) |

## 2. Packages

| Package | Version | Used for | Why this one |
|---|---|---|---|
| **VContainer** | 1.19.0 | Dependency injection: a root scope for the run, one child scope per content scene | Light and fast, with explicit registration and scope lifetimes that match scene lifetimes. Replaces V1's manual installer (D64, D112) |
| **UniTask** | 2.5.10 | Every async flow: loading, the sequencer, popups, scene changes | No allocation per await, and `CancellationToken` support everywhere. Core stays on `Task` and takes no package (D67) |
| **Addressables** | 1.29.0 | Every scene, prefab, config and level, loaded by key | Lifetimes are explicit (asset scopes), and remote content later is a profile change, not a code change (D65, D115). Last 1.x line for 2022.3 |
| **DOTween** (free) | in `Assets/Plugins` | Block snap, exit slide, popup and button animation | Mature and fast; awaited through UniTask's DOTween support (D84). Closure garbage removed by hand (D109) |
| **Newtonsoft Json** (`com.unity.nuget.newtonsoft-json`) | 3.2.1 | Level files (read and write) | `JObject` access, so each modifier reads its own fields without reflection: IL2CPP safe (V1) |
| **JsonUtility** (built in) | — | Save sections | Flat classes, no stripping risk, no extra dependency (D128) |
| **TextMeshPro** | 3.0.7 | Every UI text, LilitaOne font | Its number `SetText` overloads update the HUD without garbage |
| **uGUI** | 1.0.0 | All screens, panels, popups | Canvas scaler + safe area, built from the supplied UI art (D76) |
| **Unity Test Framework** | 1.1.33 | EditMode and PlayMode tests | Includes `Is.Not.AllocatingGCMemory()` for allocation guards (§8) |
| **Recorder** | 4.0.3 | The flow videos, with Android screen recording | Editor captures; the device recording shows the real build |
| **Device Simulator devices** | 1.0.1 | 20:9 and notch checks | The brief's 20:9 criterion, checked without a phone |
| **2D Sprite** | 1.0.0 | Sprite atlas for the UI | Fewer UI draw calls |

## 3. Architecture techniques

| Technique | Where | What it solves |
|---|---|---|
| **Pure C# layers behind asmdefs** | `Game.Core`, `Game.Meta` (no `UnityEngine` reference) | Logic and meta are tested without Unity, and the compiler keeps the view out |
| **One grid of entity ids** | `Grid` (`int[]`), `Board` | The board's one source of truth: moves, exits and win read it; views only follow |
| **Composition over inheritance for mechanics** | `IModifier` + part interfaces (`ISuspender`, `IMoveConstraint`, listeners); `Capabilities`, `EventDispatcher` | A mechanic only declares what it is, and central rules decide. Ice and Arrow stack without knowing each other (V1) |
| **Deferred commands** | `LevelCommands` | Listeners never change the board mid-event. Their changes apply in order after the event, so the order of a single move is fixed |
| **Explicit catalogs, no reflection** | `ModifierCatalog`, `ModifierPresenters` | Nothing is looked up by name or stripped under IL2CPP. A new mechanic is one line |
| **`Result<T>` instead of exceptions** | Level loading and validation | An invalid level is a value listing every error, not a crash |
| **Observer seam** | `ISessionObserver` | The one way logic talks to presentation; no C# events, no message bus (D68, D82, D85) |
| **"Logic first, view later"** | `LevelSession` → `GameplayDirector` | A win is decided at once; the animation and the panel follow, and restart cancels them (D69) |
| **Director + sequencer of steps** | `Sequencer`, `StepSequence`, `StepGroup`, `IStep` | Exits play side by side and the win waits for the last one. One token cancels everything (D69, D71) |
| **MVP for UI and modifier looks** | Dumb views, pure C# presenters, actions interfaces | Presenters are tested with fakes; a variant is data plus actions, never a subclass (D104, D121) |
| **DI scopes = scene lifetimes** | `RootScope`, `GameplayLifetimeScope`, `MainLifetimeScope` | Closing a scene disposes everything it made (D64, D114) |
| **Two start scenes** | `Bootstrap.unity` (game), `LevelTest.unity` (Editor only) | A level test runs the real game on an in-memory save; live code never checks a mode (D130, D131) |
| **Start services contract** | `LiveServicesInstaller` / `LevelTestServicesInstaller` | Both starts must register the same contracts, guarded by a test (M1.5) |
| **One modal layer** | `ModalLayer` + `ModalLayerView` | One dim, one slot, one pause hook for every panel and popup (D134, D135) |
| **Navigation and loading cover** | `Navigation`, `CoveredSceneLoader` | Every scene change happens under one cover, at most once per scene (D119, D142) |
| **Per-feature save sections** | `ISaveStore`, `Wallet`, `Progression`, `Settings` | A new feature adds a file and touches no other data; one bad file costs only itself (D128) |

## 4. Rendering & visuals

| Technique | Where | Why |
|---|---|---|
| **Custom shader `Game/Block`** | Block parts, exit particles | GPU instancing with a per-instance color, and a world clip plane that cuts the block at the door (D108) |
| **Custom shader `Game/IceBlock`** | Frozen blocks | Lit ice with a triplanar texture in world space, so blocks of any shape look continuous (D102) |
| **Custom shader `Game/SelectionOutline`** | The held block | Screen-space outline drawn through a camera command buffer, with no extra meshes (D111) |
| **GPU instancing + `MaterialPropertyBlock`** | All blocks share one material | Draw calls 183 → 15 ([Performance](Performance.md)) |
| **Runtime palette materials** | `PaletteMaterials` | One door material per color made from the palette at load, destroyed with its owner (D95) |
| **Mesh swap on kit prefabs** | `PieceView`, `DoorPieceView` | Only the kit's five prefabs; variety comes from meshes and run length (D99) |
| **Quadrant autotiling** | `WallDrawRule` | One rule draws the frame, inner walls and notches from cell data (D93) |
| **Block draw rule** | `BlockDrawRule` | Any block shape becomes corner, edge and center pieces; tested on L, T, U, ring and plus shapes |
| **Object pools** | `PiecePool`, exit particles | Restart returns pieces instead of destroying and recreating them |
| **Camera fit function** | `CameraFit` | Board bounds, HUD margins and safe area → camera placement; tested with numbers (D73) |
| **Safe area** | `SafeArea`, `SafeAreaFit` | Notches on 20:9 phones; the dim still covers the full screen (D76) |

## 5. Input & feel

| Technique | Where | Why |
|---|---|---|
| **Ray onto the ground plane, no colliders** | `BoardRaycast` | Pointer → cell is one division; nothing to keep in sync with the logic |
| **Free 2D drag resolved cell by cell** | `DragResolver` | The logic moves first and the view follows, leaning at most half a cell, only where Core says the move goes (D107) |
| **Early exit threshold** | `DragSettings.ExitThreshold` | A block next to its door leaves at 0.3 of a cell pull, which feels right (D111) |
| **UI raycast filter on press** | `MousePointerInput` | A touch on the UI never starts a block drag |
| **Old Input Manager with touch emulation** | `MousePointerInput` | One code path for mouse and touch; enough for a single-finger game |
| **`PressButton`** | Every button | One feedback component everywhere, on unscaled time, with no garbage per press (D76) |
| **Pause without `Time.timeScale`** | `GameplayLoop` | Ticks stop while UI animations keep running (D70) |

## 6. Content & data

| Technique | Where | Why |
|---|---|---|
| **Versioned level JSON** | `Assets/Levels`, [LevelFormat](LevelFormat.md) | One format written by the editor and read by the game (brief 4.4); `schemaVersion` guards changes |
| **Addressable groups by lifetime** | `Boot`, `Main`, `Gameplay`, `Levels`, `Popups` | Groups load and release together with the scope that uses them (D115) |
| **Asset scope per `LifetimeScope`** | `AssetScope` | Callers never release; a scope releases every handle it opened (D66, D113) |
| **Local / Remote profiles + content update flow** | `ContentUpdate` | Moving to a server is a profile change; the flow already runs and finds nothing to download (D65, D115) |
| **ScriptableObject config** | `GameConfig`, `PresentationAssets`, `Palette`, `ModifierViews` | Tunable values are data, not literals (D72) |
| **One JSON file per save section** | `JsonSaveStore` | Written through a temporary file (D128); an atomic replace is still open ([REVIEW](../REVIEW.md) #5) |

## 7. Level Editor

| Technique | Where | Why |
|---|---|---|
| **`EditorWindow` with a visual grid** | `LevelEditorWindow`, `GridView` | Brief 4.4: a designer paints the level instead of typing numbers |
| **Pure model behind the window** | `LevelModel`, `LevelEditorSession` | Painting, resize and undo are tested without the Editor UI |
| **Own undo history** | `EditHistory` | Every step undoes, independent of Unity's undo stack |
| **Design rules discovered by `TypeCache`** | `LevelRules` | A new rule is a new class; reflection is fine in an Editor-only assembly (D51) |
| **Modifier fields drawn from `Write` / `Read`** | `ModifierFields` | Any modifier gets an inspector without a custom drawer |
| **Save makes the level addressable** | `LevelAddressables` | One load path for the editor and the build (D118) |
| **▶ Play via `SessionState` + `playModeStartScene`** | `LevelTestLauncher`, `BootstrapPlayMode` | Save and play in one click, on a test save that never touches the player's (D131) |

## 8. Testing & measurement

| Technique | Where | Why |
|---|---|---|
| **EditMode tests by default** | `Game.Tests.*` | 300+ tests run in seconds without a scene |
| **Hand-written fakes** | `Fake*` classes | A fake is also documentation of the contract |
| **ASCII level maps in tests** | `AsciiLevel`, `LevelMap` | A test's board is readable at a glance |
| **Additivity test assembly** | `Game.Tests.Additivity` | Proves a new mechanic can be added from outside, with no edit to Core |
| **Allocation guards** | `HotPathAllocationTests` | 0 B per frame is a test, not a promise |
| **PlayMode smoke tests** | `SceneFlowTests` | Only the real scene flow needs the Unity runtime |
| **Profiler (Deep Profile on device), Frame Debugger, Addressables Event Viewer** | P8, I6 | Every performance claim has a capture behind it ([Performance](Performance.md)) |

## 9. Tools & workflow

| Tool | Used for |
|---|---|
| **Claude Code** | Pair programming under written rules: one phase at a time, an explicit go before the next, no performance claim without a measurement ([README § LLM tools](../../README.md#-llm-tools)) |
| **`CLAUDE.md` + `.claude/rules/`** | Code rules loaded per folder: layers, allocation, code shape, tests |
| **Prototype → Production** | A throwaway prototype answered the risky questions first; its knowledge moved to production through [FINDINGS](../prototype/FINDINGS.md), never its code |
| **Phase plans + decision log** | [ProductionV1](ProductionV1.md) and [ProductionV2](ProductionV2.md): every choice numbered with its reason |
| **Git / GitHub** | Version control and video hosting for the README |

## 10. Left out on purpose

| Not used | Why |
|---|---|
| **`Resources` folder** | Everything goes by key through Addressables. The only exception is DOTween's settings asset, which DOTween requires (D65, D91) |
| **Message bus / C# events in the logic** | One explicit observer seam is enough (D68, D82) |
| **Singletons, service locators, `DontDestroyOnLoad`** | Scopes own lifetimes; Bootstrap stays loaded instead (D64, D114) |
| **Static batching** | It needs Read/Write meshes at runtime; instancing already covers the repeats (D108) |
| **`Time.timeScale` for pause** | It would also freeze the UI (D70) |
| **Physics colliders for input** | A plane raycast and a division are enough |
| **URP** | The brief names no render pipeline, so the project stays on Unity's default Built-in pipeline |
| **New Input System** | A single-finger game; the old manager with touch emulation keeps one code path |
