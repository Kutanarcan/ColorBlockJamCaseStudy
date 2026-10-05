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
![Production](https://img.shields.io/badge/Production-🚧_In_Progress-1f6feb)

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
| 🏗️ **Production** | Rebuild the game cleanly on top of what the prototype proved. | 🚧 In progress |

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
| Plan | [`ProductionV1.md`](docs/production/ProductionV1.md) (active) · [`PrototypeV1.md`](docs/prototype/PrototypeV1.md) | Scope, rules, data model, phase list and decisions for the active mode |

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
> **Goal:** The whole game is playable and verified **in the logic layer alone**, with tests, before any presentation work starts.
> **Status:** 🧠 Design settled · 🪜 Phase 2 done (3/10) · ▶️ Phase 3 in progress
> **Input:** the prototype's [`FINDINGS.md`](docs/prototype/FINDINGS.md), not its code (see **Workflow** at the top).

## 🧬 Design at a Glance

| Topic | In one line |
|---|---|
| **Grid** | One 1D occupancy array of entity ids; no edge layer, no border ring. |
| **Entities** | Blocks, walls and doors share one base (shape + position + modifiers); walls and doors can sit anywhere. |
| **Movement** | A step is valid when every target cell is empty or the block's own and no gate vetoes it. |
| **Exit** | Each covered column scans forward; its first occupied cell must be a door of the block's color and direction. |
| **Modifiers** | Gate (veto), Reactor (exit → counter), Board command (narrow API). Ice, Key/Lock, Rope/Scissors, Arrow and Tangled, all stackable. |
| **Exit resolution** | Exit is the only trigger; a resolver walks the reactors in a fixed order, single pass. |
| **Game state** | Timer from level data, fail on timeout, continue adds time, restart rebuilds from level data. |
| **Level data** | Polymorphic JSON in `Game.LevelIO` (Newtonsoft), `schemaVersion`, key-based loading ready for Addressables. |
| **Level Editor** | Editor-only, data-oriented, refuses to save a level that breaks a validation rule. |

## 📦 Assemblies

| Assembly | Role |
|---|---|
| `Game.Core` | Logic and level data model, no `UnityEngine` |
| `Game.LevelIO` | JSON serialization, shared by Runtime and the Editor |
| `Game.Runtime` | Presentation, after the logic is verified |
| `Game.LevelEditor` | Editor-only level editor |

## 📚 Production Docs

- [`ProductionV1.md`](docs/production/ProductionV1.md) — scope, game rules, data model, modifiers, level pipeline, decision log

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
