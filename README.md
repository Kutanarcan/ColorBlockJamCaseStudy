<div align="center">

# 🧱 Color Block Jam — Case Study

**Slide the blocks. Match the doors. Clear the board.**

A vertical slice of **Color Block Jam** in Unity: Home, five levels, win / fail flow, coins that persist,
and a level editor a designer can use without code.

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2_LTS-black?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Android-Portrait_1080×1920-lightgrey)
![DI](https://img.shields.io/badge/DI-VContainer-6e7681)
![Content](https://img.shields.io/badge/Content-Addressables-6e7681)
![Time](https://img.shields.io/badge/Work_time-~47_h-d29922)

</div>

> [!NOTE]
> A case study assigned by Rollic. Color Block Jam is a mobile puzzle game by Rollic / Gybe Games (2024).

<details>
<summary><b>🎬 Videos and how the project was built: prototype → production</b></summary>
<br>

1. **Prototype (10 h):** answered one question as fast as possible: can the supplied kit build the board and move blocks out through matching doors? Free 2D drag, block autotiling from 4 quadrant meshes, clip-plane exit shader, a short marketing video. The output was **knowledge, not code**: rules, tuning values and rejected ideas went into [`FINDINGS.md`](docs/prototype/FINDINGS.md).
2. **Production V1 (12 h):** the whole game verified in the logic layer alone, with tests, before any visuals: movement, exit, timer, win / fail, Ice and Arrow, events and commands, the JSON format and the Level Editor.
3. **Production V2 (25 h):** the vertical slice on top of the unchanged V1 core: presentation, infrastructure, meta, gameplay UI, Home and navigation.

Each stage ran in small phases: a plan first, one phase per step, a review and a playtest after each, and the next phase only on an explicit go.

**Prototype**

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

**Level Editor**

<div align="center">
  <video src="https://github.com/user-attachments/assets/8a9dca3f-37b8-452a-8c32-e50bf642923f" width="100%" controls muted></video>
</div>

**Level Editor -2 - Ice&Arrow**

<div align="center">
  <video src="https://github.com/user-attachments/assets/c91c7077-19a0-4f6f-92d4-ff029eff044f" width="100%" controls muted></video>
</div>


**Production: full flow**

<!-- Production full-flow video (portrait): replace PRODUCTION_VIDEO_URL with the uploaded asset link. The APK link goes under it. -->
<div align="center">
  <video src="https://github.com/user-attachments/assets/d848b45c-44fa-4f59-ae43-f8e75e23fbae" width="300" controls muted></video>
</div>

</details>

| Section | |
|---|---|
| [▶️ Open and run](#️-open-and-run) | Unity version, first Play, Android build |
| [🛠️ Make a level](#️-make-a-level) | Level Editor, step by step |
| [🏛️ Architecture decisions](#️-architecture-decisions) | What was decided and why |
| [📌 Decisions on unclear requirements](#-decisions-on-unclear-requirements) | Brief §8 |
| [⚠️ Known problems and incomplete parts](#️-known-problems-and-incomplete-parts) | What is missing or accepted on purpose |
| [🤖 LLM tools](#-llm-tools) | Which tool, for what |
| [⏱️ Work time](#️-work-time) | Per stage |
| [📚 Docs](#-docs) | Plans, decision logs, deep dives |

---

## ▶️ Open and run

1. Open the project with **Unity 2022.3.62f2**. Packages (VContainer, Addressables, UniTask, DOTween) restore on first open.
2. Press **Play** with any scene open. Every Editor Play starts from `Assets/Scenes/Bootstrap.unity` (the only scene in Build Settings), which opens **Home**.
3. Addressables: in `Window → Asset Management → Addressables → Groups`, keep **Play Mode Script = Use Asset Database** for daily work. No content build is needed for it.
4. Tests: `Window → General → Test Runner` → **EditMode** (300+ tests) and **PlayMode** (2 smoke tests of the real scene flow).

**Android build**
1. `File → Build Settings` → **Android**, Switch Platform. Only `Bootstrap` is in the scene list.
2. Addressables Groups window → `Build → New Build → Default Build Script` (the content is built per platform).
3. `Build` in Build Settings.

**The flow**
Home → level button → Play popup → level → win → LevelComplete → next level · fail (time out) → LevelFail → Continue for coins, or X → Play popup (Retry / Home) · HUD Pause → Settings · HUD Restart → LoseLife (Retry).

## 🛠️ Make a level

Open **`Tools → Color Block Jam → Level Editor`**.

1. **New level:** set **W**, **H** and **Time** in the toolbar and press **New**. The edge cells start as walls.
2. **Paint:** pick a brush (**Wall**, **Door**, **Block**) and a color from the palette. Left-drag paints, right-drag erases. A door painted on the edge points outward.
3. **Select and edit:** Ctrl/Cmd + click an entity to select it; add or remove modifiers (**Ice**, **Arrow**) and their fields in the inspector.
4. **Resize and timer:** grow or shrink each side with `+` / `−`; set the time limit in seconds. Undo / Redo cover every step.
5. **Save:** the design rules run first (edge cells, edge doors, straight doors, connected blocks, door width, time limit, palette colors) and broken cells are outlined. A new level asks for a name; the file goes to `Assets/Levels/<name>.json` and becomes addressable under that name.
6. **▶ Play:** saves and plays the level in the game at once, as a **level test**: a separate start with its own coins and an in-memory save, so the player's save is never touched. A red **LEVEL TEST** badge is on screen.
7. **Put it in the game:** add the level's name to `levelKeys` in `Assets/ScriptableObjects/Config/GameConfig.asset`, in play order.

The five game levels were made this way. One JSON format is saved by the editor and read by the game ([`LevelFormat.md`](docs/production/LevelFormat.md)).

## 🏛️ Architecture decisions

| Decision | Why |
|---|---|
| **Logic in pure C# assemblies** (`Game.Core`, `Game.Meta`) with no `UnityEngine` reference | Data, logic and view stay apart by the compiler: a Unity call in the logic is a build error. The whole game rule set is tested without Unity. |
| **One logic → view seam** (`ISessionObserver`), called by the session | The view only hears what happened (moved, exited, modifier changed, state changed). Presentation was added later without rewriting the verified logic. |
| **The logic finishes first, a director plays it later** as awaitable, cancellable steps | A win is decided at once; the exit animation and the panel follow. Restart cancels every step with one token, so nothing of the last attempt survives on screen. |
| **Dumb views, pure C# presenters (MVP)** | Views have setters and report presses; presenters read the logic and the meta. They are tested with fakes, and a popup variant (Retry / Play, Home / Gameplay Settings) is data plus actions, not a subclass. |
| **Additive mechanics:** modifiers only declare (suspend, constrain, listen); central rules decide | A new mechanic touches no existing file except one catalog line. Proven by adding one end to end from a separate test assembly. |
| **VContainer scopes:** a root scope for the run, one child scope per content scene | One composition root; a service lives exactly as long as its scope. Closing a scene disposes everything it made. |
| **Addressables by key, an asset scope per `LifetimeScope`** | Callers never release: a scene's assets go with its scope, checked with the profiler (nothing left after a round trip). A content server is a profile change; the download flow already runs. |
| **Two start scenes: the game and the level test**, each with its own start services | A level test runs the real game code on an in-memory save and test values. Live code never checks a mode, and test types cannot reach a build (Editor-only assembly). |
| **Screens, panels and popups** share one modal layer per scene | One dim, one open / close animation and one pause hook for everything. Popups are a catalog loaded by key on first open; panels belong to the Gameplay flow. |
| **Save: one JSON section per feature** | Wallet, progression and settings each own their file. A new feature adds a section and touches no other data; one bad file cannot cost the others. |
| **Performance by measurement** | GPU instancing (draw calls 183 → 15), 0 B garbage per frame (guarded by a test), measured on an Android phone ([`Performance.md`](docs/production/Performance.md)). |
| **Small units** | A class over ~150 lines or a folder over 6 files is split; one public type per file. |

## 📌 Decisions on unclear requirements

| Topic | Decision |
|---|---|
| Fail popup | **LevelFail** panel: **Continue** buys +20 s for coins (900, then +1000 per continue in the same attempt; red when the wallet cannot pay). Its **X** opens the **Play** popup: **Retry** restarts, **X** goes Home. |
| Pause / restart / home | HUD **Pause** opens Settings and stops the timer; Settings' **Home** asks to leave (LoseLife). HUD **Restart** asks first (LoseLife, Retry). |
| Coins | 1000 to start, +50 per win. The win is saved the moment it happens, before the panel. |
| After level 5 | The game wraps to level 1; the level number keeps counting. |
| Lives | Placeholder: a fixed count and timer; Retry and Leave take no life. |
| Boosters | Visual only, with button feedback. |
| Settings | All three toggles are saved and show on / off. Only sound acts (mutes the effects); there is no music asset, and vibration has no effect. |
| Tabs | Every tab gives button feedback; none opens a section. |
| Extra | Holding the bottom of the fail panel fades it and the dim out to show the board; releasing brings them back. |

## ⚠️ Known problems and incomplete parts

- **Optional scope not done:** no unsolvable-level warning (it needs a solver), boosters are not functional. Ice and Arrow are done.
- **Placeholders by the brief:** lives, boosters, tabs, the music and vibration toggles, Home's level track (numbers only).
- **2×2 inner walls:** the kit has no fill piece, so the middle of a 2×2 or larger inner wall block stays empty.
- **Small garbage per action:** ~0.5 KB per exit and ~2.3 KB per win, accepted on purpose (none per frame). An exiting block draws in its own batch while it is cut. Details and fixes: [`Performance.md`](docs/production/Performance.md).
- **`GameplayEntry` is large:** it builds the level, the views and the UI wiring (~190 lines, many dependencies). The next step is to move the HUD, popup and panel wiring into its own class.
- **Deliberate trade-offs** (sound player made inside Gameplay, no app-lifecycle hook, no change events from services…): each with its trigger and fix in [`Extending.md` §7](docs/production/Extending.md#7-deliberate-trade-offs--future-services-v2).
- **Tests:** Home → level → Home is a PlayMode smoke test; the full win → next → fail → Home chain is checked by hand:
  Home → Play → win → Continue (next level, coins +50) → let the time run out → LevelFail → Continue (coins −900, +20 s) → fail again → X → Play popup → X → Home (coins and level kept).
- **No server:** the Remote profile and the download flow are in place, but nothing is hosted.

## 🤖 LLM tools

**Claude Code (Anthropic Claude)** was the only LLM tool, used as a pair programmer for the whole project: prototype, production code, tests, editor tooling and documentation.

- **Me:** analysis of the game and the asset kit, the phase plans, every architecture and design decision, scene and prefab building, playtesting and tuning, reviewing every change before moving on.
- **Claude Code:** implementing one agreed phase at a time, proposing options with trade-offs, writing tests and docs, keeping the plan and decision log up to date.
- **Rules:** written by me and loaded per folder ([`CLAUDE.md`](CLAUDE.md), [`.claude/`](.claude/)): small phases, an explicit go before the next one, no performance claim without a measurement, no prototype code carried into production.

## ⏱️ Work time

About **47 hours** of hands-on work.

| Stage | Sessions | Time |
|---|---|:---:|
| 🧪 Prototype: prove the mechanic with the supplied kit | 3 | 10 h |
| 🏗️ Production V1: logic, level format, Level Editor, additivity | planning + 3 | 12 h |
| 🎨 V2: presentation (board, blocks, drag, exit, camera, performance) | 2 | 8 h |
| 🏛️ V2: infrastructure (DI, Addressables, scenes, config) | 1 | 4 h |
| 🖼️ V2: meta, gameplay UI, Home, navigation | 3 | 13 h |
| **Total** | | **~47 h** |

## 📚 Docs

| Doc | |
|---|---|
| [`ProductionV2.md`](docs/production/ProductionV2.md) | The vertical slice plan: phases, acceptance criteria, decision log (D61+) |
| [`ProductionV1.md`](docs/production/ProductionV1.md) | Logic, data model, level pipeline, Level Editor; decision log D1–D60 |
| [`Addressables.md`](docs/production/Addressables.md) | Groups, keys, loading and release, download flow, editor setup, release check |
| [`Performance.md`](docs/production/Performance.md) | Profiler captures, draw calls, allocations, accepted costs |
| [`Extending.md`](docs/production/Extending.md) | How to add a mechanic; deliberate trade-offs |
| [`LevelFormat.md`](docs/production/LevelFormat.md) | Level JSON format and change checklists |
| [`FINDINGS.md`](docs/prototype/FINDINGS.md) · [`AlgorithmExplanation.md`](docs/prototype/AlgorithmExplanation.md) | What the prototype proved, and how its algorithms work |

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
