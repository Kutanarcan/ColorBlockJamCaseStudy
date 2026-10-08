<div align="center">

# 🧱 Color Block Jam — Case Study

**Slide the blocks. Match the doors. Clear the board.**

A Unity case study that rebuilds the core loop of **Color Block Jam**
from an existing 3D model kit — prototype first, production later.

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2_LTS-black?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Render](https://img.shields.io/badge/Render-Built--in_RP-lightgrey)
![Tween](https://img.shields.io/badge/Tween-DOTween-6e7681)
![Platform](https://img.shields.io/badge/Mobile-Portrait-lightgrey)
![Prototype](https://img.shields.io/badge/Prototype-✅_Done_in_10h-2ea043)
![Marketing](https://img.shields.io/badge/Marketing_Video-✅_Done-2ea043)
![Production V1](https://img.shields.io/badge/Production_V1-✅_Logic_·_Levels_·_Additive-2ea043)
![Production V2](https://img.shields.io/badge/Production_V2-🚧_17/28_phases-1f6feb)

</div>

> [!NOTE]
> **A case study assigned by Rollic.** Color Block Jam is a mobile puzzle game by Rollic / Gybe Games (2024).
> This repository rebuilds its core mechanic as part of that case study.

## 🛠️ Workflow

The project follows a studio-style pipeline: **validate the mechanic first, show it, then build it properly.**

```mermaid
flowchart LR
    P["🧪 Prototype"] --> L["✨ Light Polish"] --> M["🎬 Marketing Video"] --> R["🏗️ Production"]

    classDef done fill:#2ea043,color:#fff,stroke:#2ea043
    classDef active fill:#1f6feb,color:#fff,stroke:#1f6feb
    class P,L,M done
    class R active
```

| Stage | Purpose | Status |
|---|---|---|
| 🧪 **Prototype** | Prove the core loop with the existing model kit, as fast as possible. Code is written to be read and studied, not shipped. | ✅ Done |
| ✨ **Light Polish** | A small, fast pass so the mechanic reads well on video: exit animation, particles, sounds, selection outline. | ✅ Done |
| 🎬 **Marketing Video** | Check that the mechanic sells itself in a short clip before investing in production. | ✅ Done |
| 🏗️ **Production** | Rebuild the game cleanly on top of what the prototype proved. | ✅ V1 (logic, levels, additive) · 🚧 V2 vertical slice (presentation and infrastructure done, gameplay UI next) |

**From prototype to production**
- Production carries over **knowledge, not code**: settled rules, tuning values, data shapes and rejected ideas, all recorded in [`FINDINGS.md`](docs/prototype/FINDINGS.md).
- Prototype code is a reference only. If a piece is worth keeping, it is refactored and refined to production standards first; nothing is copied over as is.

<details>
<summary><b>🤖 AI-Assisted Workflow</b></summary>
<br>

AI is used as a pair programmer under rules I designed — not as an autopilot. I don't just read and approve: I split the work into small phases, go through every step in detail, correct what is off and learn the code as it grows. The AI implements, explains and documents within the limits I set.

**Who does what**

| Me | AI |
|---|---|
| Analyze the game and the model kit, write the phase plan | Implements one phase at a time |
| Write the rules and constraints for each mode | Follows the active mode's rules |
| Correct phase definitions: split them, narrow them, ask for a plan first | Explains the approach before writing code |
| Choose between approaches when there is a real trade-off | Lays out the options with a recommendation |
| Review every phase line by line until I understand how it works | Answers questions and documents the algorithms |
| Playtest and tune every phase in the Editor | Exposes every feel value in the Inspector |
| Decide when a phase is done | Stops after every phase and waits for an explicit go |

**How the rules are layered**

| Layer | File | What it controls |
|---|---|---|
| Project | [`CLAUDE.md`](CLAUDE.md) | Communication style, folder layout, which mode is active, which plan is loaded |
| Mode | [`.claude/modes/`](.claude/modes/) | The process: how phases are sliced, what "done" means, the answer format |
| Folder | [`.claude/rules/`](.claude/rules/) | Code rules that load only for matching folders: `prototype.md` for `Assets/Prototype/`, `production/*` for `Assets/Scripts/` |
| Plan | [`ProductionV2.md`](docs/production/ProductionV2.md) (active) · [`ProductionV1.md`](docs/production/ProductionV1.md) (done) · [`PrototypeV1.md`](docs/prototype/PrototypeV1.md) | Scope, rules, data model, phase list and decisions for the active mode |

The mode line in `CLAUDE.md` is changed only by me. The AI never infers the mode from the code, and stops to ask if the mode and its rules disagree.

**One phase, start to finish**
1. **Plan:** the AI states the goal, the files it will touch and what will be visible in the Editor when it is done. If the phase is too big or aimed wrong, I correct it before any code is written.
2. **Decide:** if there is more than one sensible approach, it lists them with trade-offs and a recommendation; I pick.
3. **Build:** the smallest change that makes the phase playable, with every feel value as a serialized field.
4. **Review:** I read the change, question anything unclear and make sure I understand it before moving on.
5. **Try:** I play it in the Editor, report what feels wrong and tune the values.
6. **Record:** settled rules, tuning values, costs and rejected ideas go into [`FINDINGS.md`](docs/prototype/FINDINGS.md); answered questions are marked in the plan.
7. **Stop:** the next phase starts only when I say so. A remark like "it works" is not a go.

**Review depth per mode**

| | 🧪 Prototype | 🏗️ Production |
|---|---|---|
| **Correctness & feel** | ✅ Every phase | ✅ Every phase |
| **Understanding the code** | ✅ Every phase | ✅ Every phase |
| **Readability, naming, comment cleanup** | ⏭️ Skipped on purpose — the code is a disposable reference | ✅ Every phase, by hand |
| **Tests** | ⏭️ None | ✅ Every phase |

**Keeping the rhythm**
The phase rhythm is the rule I watch most closely. Once, the AI treated a "drag works" remark as approval and continued from Phase 7 into Phase 8 in the same answer. Two phases landed at once and the step-by-step review broke. I stopped it, and "no next phase without an explicit go" became a permanent rule in its memory. It has held since.

**Examples from this prototype**
- **Drag:** started as one axis per step; switched to free 2D drag after comparing it with the original game. Before the drag code was split into `Drag`, `DragCollision` and `Placement`, I asked for the plan first.
- **Exit animation:** five options were compared (row-by-row hiding, depth mask, stencil, squash, clip plane). I chose the clip-plane shader because it handles every block shape, including `InnerCorner`. The work was then split into five small steps.
- **Selection outline:** a screen-space outline was chosen over an inverted hull, because a multi-piece block would otherwise show seams between its pieces.
- **Documentation:** the first algorithm write-up was too dense; I had it rewritten in a step-by-step style with terms, diagrams and worked examples.
- **Known gaps:** edge cases found while documenting were logged as costs instead of being fixed on sight, so the prototype stayed fast.

**What the AI does not do**
- Switch modes, start the next phase or change the plan on its own.
- Carry prototype code into production.
- Make performance claims without a measurement.

</details>

---

# 🧪 Prototype

> [!IMPORTANT]
> **Question:** Can the existing model kit build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?
> **Status:** Phases 1–10 done · Phase 11 (Win) skipped
> **Time:** ⏱️ 10 hours over 3 sessions

The prototype's output is **knowledge, not code**. No architecture, no tests; every feel value is exposed in the Inspector. The code lives in `Assets/Prototype/`, is Editor-only and is not carried into production as is.

## ⏱️ Time Spent

The whole prototype was built in **10 hours of hands-on work**, from an empty project to a playable level, split across **3 working sessions**:

| Session | Duration |
|---|:---:|
| Session 1 | 4 h |
| Session 2 | 3 h |
| Session 3 | 3 h |
| **Total** | **10 h** |

Speed is the point of a prototype: answer the question with the least time invested, before committing to production. Ten hours was enough to prove the core loop with the existing model kit.

## 🎬 Videos

<table>
  <tr>
    <th>Gameplay</th>
    <th>Marketing</th>
  </tr>
  <tr>
    <td><video src="https://github.com/user-attachments/assets/b70f40cb-7e7c-474b-8a10-92ef0715b7d5" width="300" controls muted></video></td>
    <td><video src="https://github.com/user-attachments/assets/39c8f7a1-a1f1-45dc-81fd-15d65a67241f" width="300" controls muted></video></td>
  </tr>
</table>

## 🎮 The Game

- **Board:** a grid enclosed by walls; some wall segments are **colored doors**.
- **Blocks:** polyominoes (1×1, bars, L, T…), one color each. Drag a block; it slides through empty cells and stops at walls and other blocks.
- **Axis lock:** a block with an arrow on top moves along one axis only.
- **Exit:** a block pushed into a door of **its own color**, wide enough for the block, leaves the board.
- **Win:** every block has exited. The puzzle is the move order.

## ✅ What Was Built

| # | Phase | Result |
|---|---|---|
| 1 | Grid | Ground tiles at a 2-unit cell pitch |
| 2–4 | Block autotile | Any polyomino (L, T, S/Z, +, U, ring) dressed from 4 quadrant meshes |
| 5–6 | Walls & doors | Border from an edge layer; same-color door edges merge into one door |
| 7–8 | Drag & collision | Free 2D drag, cell-by-cell logic, snap on release |
| 9 | Axis lock | Locked blocks move on one axis, with a fitted arrow on top |
| 10 | Exit | A block pushed into a matching, wide-enough door leaves the board |
| 11 | Win | ⏭️ Skipped |

**Light polish**
- **Exit:** the block snaps to the door, slides in and is cut by a clip-plane shader at the door line, with per-row particles.
- **Selection:** a gold screen-space outline around the held block.
- **SFX:** select, drop and exit crunch.
- **Recording:** a hand cursor that follows the mouse from the fingertip.

## 🧩 How It Works

Each topic has a full walkthrough with diagrams and a worked example in [`AlgorithmExplanation.md`](docs/prototype/AlgorithmExplanation.md).

| Topic | In one line |
|---|---|
| [BlockDrawRule](docs/prototype/AlgorithmExplanation.md#blockdrawrule) | Each quadrant of a cell looks at 3 neighbors and picks `OuterCorner`, `Edge`, `Center` or `InnerCorner`. |
| [Movement](docs/prototype/AlgorithmExplanation.md#movement) | One cell per step; every target cell must be inside, empty or the block's own, behind an open edge. |
| [Exit](docs/prototype/AlgorithmExplanation.md#exit) | A blocked step checks the border: every column or row the block covers must face a door of its color. |
| [Drag pipeline](docs/prototype/AlgorithmExplanation.md#drag-pipeline) | Logic walks toward the pointer cell by cell; the visual leans up to half a cell and never into a refused cell. |
| [Exit visual](docs/prototype/AlgorithmExplanation.md#exit-visual) | A per-block world clip plane hides everything past the door; back faces are drawn flat so the cut looks solid. |
| [Edge layer](docs/prototype/AlgorithmExplanation.md#edge-layer) | Walls and doors sit on the lines between cells, in two arrays: `hEdges[W, H+1]` and `vEdges[W+1, H]`. |
| [Coordinates](docs/prototype/AlgorithmExplanation.md#coordinates) | Cell = 2 units, cell center = `(2x + 1, 0, 2y + 1)`; logic is always whole cells. |
| [Border runs](docs/prototype/AlgorithmExplanation.md#border-runs) | Consecutive equal border edges become one stretched wall or door piece. |
| [Arrow placement](docs/prototype/AlgorithmExplanation.md#arrow-placement) | The arrow sits on the straight run through the block's most central cell, sized `min(run, 3)`. |

## 📚 Prototype Docs

- [`PrototypeV1.md`](docs/prototype/PrototypeV1.md) — game summary, asset analysis, phase plan, open questions
- [`FINDINGS.md`](docs/prototype/FINDINGS.md) — settled rules, tuning values, costs, rejected ideas, data shapes
- [`AlgorithmExplanation.md`](docs/prototype/AlgorithmExplanation.md) — how the prototype's algorithms work

---

# 🏗️ Production

> [!IMPORTANT]
> **V1 goal:** The whole game is playable and verified **in the logic layer alone**, with tests, before any presentation work starts.
> **Status:** ✅ V1 done (Phases 0–13, Phase 9 dropped) · 🚧 V2 in progress (see [V2: Vertical Slice](#-v2-vertical-slice))
> **Input:** the prototype's [`FINDINGS.md`](docs/prototype/FINDINGS.md), not its code (see **Workflow** at the top).

V1 is built on three pillars: **logic** that runs and is tested without Unity, a **level pipeline** that saves exactly what the logic loads, and an architecture where a new mechanic is **added, not edited in**. Presentation (visuals, drag feel, exit animation, audio) comes after, on top of a verified core.

## ⏱️ Time Spent

Production V1 took **12 hours of hands-on work**: a planning pass first, then **3 working sessions**:

| Session | Duration |
|---|:---:|
| Planning | 2 h |
| Session 1 | 4 h |
| Session 2 | 3 h |
| Session 3 | 3 h |
| **Total** | **12 h** |

Planning came first on purpose: the design, the phase plan and the decision log were settled before any production code, so each session built a phase that was already agreed.

## 🎬 Level Editor

<!-- Level Editor video (OBS, landscape): replace LEVEL_EDITOR_VIDEO_URL with the uploaded asset link. -->
<div align="center">
  <video src="https://github.com/user-attachments/assets/8a9dca3f-37b8-452a-8c32-e50bf642923f" width="100%" controls muted></video>
</div>

## 🧱 The Three Pillars

### 🧠 Logic
- **Pure C#.** `Game.Core` has no `UnityEngine` reference; calling a Unity API there is a compile error.
- **One occupancy array.** Walls, doors and blocks are all entities in one 1D grid of entity ids. No edge layer, no border ring, no bounds checks.
- **Movement:** one cell per step; every target cell must be empty or the block's own.
- **Exit:** each column the block covers scans forward; its first occupied cell must be a door of the block's color and direction. This closes the prototype's U-recess bug.
- **Game state:** timer driven by `Tick(dt)`, win, fail on timeout or by a listener, continue with added time, restart from level data. No undo in the game.
- **Modifiers:** Ice (frozen until N exits) and Arrow (one axis, both ways; one direction until V2 D105), stackable.

### 💾 Level Pipeline
- **Level data** (`LevelData`) is plain C# in Core; **JSON** lives in `Game.LevelIO` and is mapped by hand: no runtime reflection, IL2CPP safe.
- **Validation in two places:** structural checks when a level loads (`LevelSession.TryCreate`), design rules in the editor, which refuses to save a level that breaks them.
- **Ready for remote levels:** levels are reached by key through `ILevelSource` (async), every file carries a `schemaVersion`, and an unknown modifier type rejects the level cleanly.
- **Level Editor** (`Tools → Color Block Jam → Level Editor`): an Editor-only window on a data-oriented model, with all its behaviour in a testable session class.

| Level Editor | |
|---|---|
| **Painting** | Wall / Door / Block brush; left-drag paints, right-drag erases; fast drags stay connected |
| **Selection** | Ctrl/Cmd + click selects and takes up the entity's brush; a stroke next to the selection extends it |
| **Frame** | Edge cells are walls by default and hold only walls and doors; edge doors point outward |
| **Undo / Redo** | Snapshot history, one step per stroke or edit |
| **Resize** | Grow or shrink each side; the frame moves, the inside stays, a cut asks first |
| **Modifiers** | Listed from the catalog, fields edited generically: a new modifier needs no editor change |
| **Rules** | Edge cells, edge doors, straight doors, connected blocks, door width, time limit, Core validation, palette colors (V2); broken cells are outlined |
| **▶ Play** (V2) | Saves the level and plays it in the game through the Bootstrap scene, like a real start: a new level needs no list or code change |
| **Addressable on save** (V2) | Saving puts the level in the `Levels` Addressables group under its key, so the editor and the build load it the same way |
| **Colors** (V2) | Swatches and grid colors come from the game's palette asset, so the editor shows what the game shows |

### 🔌 Additive
- **Modifiers only declare** (suspends a capability, constrains a direction, listens to an event); central rules decide.
- **Three events** (exited, move committed, ticked) and a small set of **command primitives**; new effects are combinations, not new commands.
- **The criterion:** adding a mechanic changes no existing file, except one line in `ModifierCatalog.Default()` or a documented edit point.
- **Proven:** Turn Based Arrow was added end to end (data, JSON, logic) from a separate test assembly that sees only the public API, with **zero changes** to Core, LevelIO or the editor. The recipe is in [`Extending.md`](docs/production/Extending.md).

## ✅ What Was Built

| # | Phase | Result |
|---|---|---|
| 0 | Skeleton | `Game.Core` without UnityEngine, first EditMode test |
| 1 | Grid & Entities | Occupancy array, Block / Wall / Door entities, level data and builder |
| 2–3 | Movement & Exit | Step rule and the column-scan exit, U-recess bug closed |
| 4 | Game State | Win, timer, fail, continue, restart |
| 5–6 | Modifiers | Capability model, Ice and Arrow, parallel stacking |
| 7–8 | Seams | Events, listeners, command primitives, move count, effective color, win exemption |
| 9 | Dormant Entities | ➖ Dropped from V1, kept as a documented edit point |
| 10–11 | Validation & LevelIO | Load-time validation, hand-mapped JSON, explicit modifier catalog |
| 12 | Level Editor | Data-oriented model, validation rules, window, save / load |
| 12b | Editor Usability | Brush & stroke, undo / redo, resize, frame, selection, styling |
| 13 | Additivity Proof | Turn Based Arrow with zero Core changes, `Extending.md` |

**Tests:** 195 EditMode test methods (more with `TestCase` rows) across `Game.Tests.EditMode`, `Game.Tests.LevelIO`, `Game.Tests.LevelEditor` and `Game.Tests.Additivity`.

## 📦 Assemblies

| Assembly | Role |
|---|---|
| `Game.Core` | Logic and level data model, no `UnityEngine` |
| `Game.LevelIO` | JSON serialization, shared by Runtime and the Editor |
| `Game.LevelEditor` | Editor-only level editor; reads the palette through `Game.Runtime`, makes saved levels addressable and plays them through `Game.Infrastructure` (V2) |
| `Game.Infrastructure` | Infrastructure (V2): root installer, bootstrapper, Addressables content flow, asset scopes, scene loader, config, level source |
| `Game.Infrastructure.Editor` | Editor-only: every Play starts from the Bootstrap scene (V2) |
| `Game.Runtime` | Presentation (V2): board, blocks, modifier looks, drag, director and sequencer, exit, camera fit; the root and Gameplay `LifetimeScope`s |
| `Game.Tests.Runtime` | EditMode tests of Runtime's pure parts (V2) |
| `Game.Tests.Infrastructure` | EditMode tests of Infrastructure with fakes: installer, bootstrapper, asset scope, content update, config, level source (V2) |
| `Game.Tests.PlayMode` | One smoke test of the real scene flow through Addressables (V2) |

## 🎬 V2: Vertical Slice

> [!IMPORTANT]
> **V2 goal:** the case brief's vertical slice — a playable level flow (Home → Gameplay → Win / Fail), five levels made with the Level Editor, coins and progress that survive a restart, an APK and a video.
> **Status:** ✅ Presentation (P0–P9) · ✅ Infrastructure (I0–I6) · 🚧 Gameplay UI next (U0 Canvas, Safe Area & Loading Cover). Plan: [`ProductionV2.md`](docs/production/ProductionV2.md).

### ⏱️ Time Spent

V2 so far took **12 hours of hands-on work** over **3 working sessions**:

| Session | Stage | Duration |
|---|---|:---:|
| Session 1 | 🎨 Presentation | 4 h |
| Session 2 | 🎨 Presentation | 4 h |
| Session 3 | 🏛️ Infrastructure (I0–I6) | 4 h |
| **Total** | | **12 h** |

### 🎨 Presentation

| # | Phase | Result |
|---|---|---|
| P0 | Runtime Skeleton | UniTask, `Game.Runtime`; one logic → view seam (`ISessionObserver`) heard for moves, exits, modifier changes and state; Gameplay scene with a manual installer |
| P1 | Board View | Ground, frame, inner walls and doors from the kit; one quadrant rule draws every wall; door runs stretched to their length; palette materials shared per color |
| P2 | Block View | `BlockDrawRule` dresses any polyomino; pooled parts; Arrow and Ice looks; a lit triplanar ice shader |
| P3 | Camera Fit | The board fills the safe area minus the HUD margins, 16:9 to 20:9 |
| P4 | Input & Drag | Free 2D drag; Core decides every move, the view only previews what Core allows; a block beside its door leans in and exits early; the held block is outlined; Arrow became an axis lock (level format v2) |
| P5 | Director & Sequencer | Logic events become awaitable, cancellable steps; exits never block the player, win / fail wait for them |
| P6 | Exit Visual & Feedback | Snap, slide and clip-plane cut through the door, per-row particles, select / drop / crunch sounds |
| P7 | Restart & Lifecycle | Restart leaves nothing from the last attempt; pause stops the timer and locks input |
| P8 | Polish & Performance | Draw calls 183 → 15 through instancing; no allocation per frame (see **Performance** below) |
| P9 | Five Levels & Editor Play | **▶ Play** in the Level Editor saves and plays the level in the game; editor colors come from the game's palette, with a rule for ids outside it; levels 1–6 built in the editor |

**Tests:** 43 EditMode test methods in `Game.Tests.Runtime` at the end of the stage (sequencer, draw rules, camera fit, drag, exit path, restart, allocation on hot paths), plus the V1 suites extended for move preview, axis lock and editor play.

**Decisions worth knowing**
- **The V1 core is not rewritten.** Presentation sits on top through one observer seam; Core only gained that seam.
- **Kit prefabs only.** Variety is mesh / material swap and run length through a View component with serialized references; no child is found by path.
- **Views are dumb, presenters are pure C#.** A view never references the logic; a presenter reads it and drives the view. A new modifier's look is a new presenter and view plus one registration line.
- **The logic decides, the view plays.** Every move, also one the drag only previews, is answered by Core (`PreviewMove`); the logic finishes first, then the director turns what happened into steps the sequencer plays.
- **One token cancels everything.** Restart cancels every running step and rebuilds the views from the level data: nothing of the last attempt survives, on screen either.
- **Small folders.** A code folder holds at most 6 files; the change that adds the 7th splits it.

**⚡ Performance** (measured in the Editor and on an Android phone, IL2CPP)

| | Result |
|---|---|
| Draw calls | **183 → 15** (GPU instancing; one block material with per-instance color) |
| Garbage per frame | **0 B** (idle, drag, release), also guarded by a test |
| Garbage per action | ~0.5 KB per exit, 2.3 KB per win: **accepted on purpose**, small and per action, not per frame |

The captures, what each allocation is, the known fixes and why they were not applied yet are in [`Performance.md`](docs/production/Performance.md).

### 🏛️ Infrastructure

The game now starts like a shipped one: one Bootstrap scene, dependency injection through VContainer scopes, and every scene, level, config and presentation asset loaded **by key** through Addressables. Presentation did not change: it was built refactor-ready, so only the wiring and the loading moved.

```
Bootstrap scene ── RootLifetimeScope (lives for the run)
   └── Bootstrapper
         1. Addressables.InitializeAsync
         2. Content update: catalogs → clean old bundles → download size → download (all empty locally)
         3. GameConfig by key
         4. First level: the Level Editor's request, else the config's first key
         5. Gameplay scene, additive ── GameplayLifetimeScope (child scope, own asset scope)
                                          GameplayEntry: presentation assets + level by key → session, views, play
```

| # | Phase | Result |
|---|---|---|
| I0 | Packages & Root Scope | VContainer 1.19.0, Addressables 1.29.0; `Game.Infrastructure`; Bootstrap scene with a root scope whose registrations live in a pure, testable installer |
| I1 | Asset Loader & Scopes | One door to Addressables (`IAssetLoader`); every `LifetimeScope` owns an asset scope that releases all its handles when the scope closes; a load that finishes after its scope closed is released and reported |
| I2 | Scene Loader | Bootstrap stays loaded; one content scene at a time is loaded additively and becomes a child scope; unloading it releases its assets |
| I3 | Addressables Setup | Groups by lifetime (`Boot`, `Gameplay`, `Levels`), `remote` label, Default / Remote profiles; the server-side content update flow runs at start-up and finds nothing locally |
| I4 | Config & Level Source | `GameConfig` (level order, popup timing) by key; levels by key through the scope's asset loader; the manual installer became `GameplayLifetimeScope` + `GameplayEntry` |
| I5 | Editor Play via Bootstrap | Every Editor Play starts from Bootstrap; Level Editor ▶ Play hands its key to the bootstrapper; saved levels become addressable automatically |
| I6 | Release Check & Android Smoke | Profiler round trip with real bundles: after unloading Gameplay only `Boot` stays, also on a second round trip; first APK runs, wins and fails on a device |

**Tests:** 17 EditMode test methods in `Game.Tests.Infrastructure` (installer, bootstrapper order, asset scope release and leaks, content update, config, level source, play request) and 1 PlayMode smoke test of the real scene flow (Bootstrap → Gameplay → back, nothing left behind).

**Decisions worth knowing**
- **Groups pack, keys load, labels download.** Groups follow lifetime so closing a scope really empties its bundles; game code loads one asset at a time by key; the `remote` label is only for the download step.
- **Callers never release.** An asset lives as long as the scope that loaded it; the scope releases everything when its scene closes.
- **A server is a profile change.** Groups use the profile's Remote location; the Default profile points it at the build, the Remote profile at a server path. The download flow already runs on every start.
- **No empty structure.** Groups, assemblies and scenes appear when their first content does (`Main`, `Popups`, `Game.Meta` come with their phases).
- **Editor-only stays editor-only.** The Level Editor's play request is stored in `SessionState`; the root scope picks that store in the Editor and a null store in a player, so no editor type reaches a build.
- **Found on the device:** cleaning the bundle cache inside the catalog update failed on Android before the cache was ready and stopped the start-up. Cleaning now runs after it, waits for the cache and only warns on failure.

Everything about groups, keys, the download flow, profiles, the editor setup checklist and the release check is in [`Addressables.md`](docs/production/Addressables.md).

## 📚 Production Docs

- [`ProductionV2.md`](docs/production/ProductionV2.md) — **active:** vertical slice plan: presentation, infrastructure, UI, meta, delivery; decision log D61+
- [`ProductionV1.md`](docs/production/ProductionV1.md) — scope, game rules, data model, modifiers, level pipeline, phase plan, decision log
- [`Performance.md`](docs/production/Performance.md) — measurements, profiler captures, accepted costs and their known fixes
- [`Addressables.md`](docs/production/Addressables.md) — groups, keys, loading and release, download flow, profiles, editor setup, release check
- [`LevelFormat.md`](docs/production/LevelFormat.md) — level JSON format and the **mandatory** checklists for changing level data, modifiers or LevelIO
- [`Extending.md`](docs/production/Extending.md) — how to add a mechanic, the Turn Based Arrow example, edit points
- [`ColorBlockJamMechanics.md`](docs/production/ColorBlockJamMechanics.md) — the mechanics reference the architecture is built for

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
