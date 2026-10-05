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
![Phase](https://img.shields.io/badge/Phase-🧪_Prototyping-d29922)

</div>

> [!NOTE]
> **Fan-made study, not affiliated with the original game.** Color Block Jam is a mobile puzzle game by Rollic / Gybe Games (2024).
> This repository only rebuilds its core mechanic for learning and portfolio purposes.

---

# 🧪 Prototype

> [!IMPORTANT]
> **Question:** Can the existing model kit build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?
> **Status:** Phases 1–10 done · Next: Phase 11 · Win

The prototype's output is **knowledge, not code**. No architecture, no tests; every feel value is exposed in the Inspector. The code lives in `Assets/Prototype/`, is Editor-only and will be deleted after the harvest.

## 🎬 Videos

**Gameplay**

https://github.com/user-attachments/assets/b0947644-7580-4f5f-8c1e-7055974298b3

**Marketing · Vol. 1**

https://github.com/user-attachments/assets/790ba8f5-b63e-40f3-bb16-6daa3c05b080

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
| 11 | Win | ⏳ Next |

**Feel on top**
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

⏳ **Not started.** It begins after the prototype's last phase, from a harvest of `FINDINGS.md`. The prototype code is read as a spec and never migrated.

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
