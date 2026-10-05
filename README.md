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
![Prototype](https://img.shields.io/badge/Prototype-✅_Done-2ea043)
![Marketing](https://img.shields.io/badge/Marketing_Video-✅_Done-2ea043)
![Production](https://img.shields.io/badge/Production-⏳_Next-6e7681)

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
    classDef todo fill:#6e7681,color:#fff,stroke:#6e7681
    class P,L,M done
    class R todo
```

| Stage | Purpose | Status |
|---|---|---|
| 🧪 **Prototype** | Prove the core loop with the existing model kit, as fast as possible. Code is written to be read and studied, not shipped. | ✅ Done |
| ✨ **Light Polish** | A small, fast pass so the mechanic reads well on video: exit animation, particles, sounds, selection outline. | ✅ Done |
| 🎬 **Marketing Video** | Check that the mechanic sells itself in a short clip before investing in production. | ✅ Done |
| 🏗️ **Production** | Rebuild the game cleanly on top of what the prototype proved. | ⏳ Next |

**From prototype to production**
- Production carries over **knowledge, not code**: settled rules, tuning values, data shapes and rejected ideas, all recorded in [`FINDINGS.md`](docs/prototype/FINDINGS.md).
- Prototype code is a reference only. If a piece is worth keeping, it is refactored and refined to production standards first; nothing is copied over as is.

---

# 🧪 Prototype

> [!IMPORTANT]
> **Question:** Can the existing model kit build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?
> **Status:** Phases 1–10 done · Phase 11 (Win) skipped

The prototype's output is **knowledge, not code**. No architecture, no tests; every feel value is exposed in the Inspector. The code lives in `Assets/Prototype/`, is Editor-only and is not carried into production as is.

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

⏳ **Next.** Starts from the prototype's findings in `FINDINGS.md`, not from its code (see **Workflow** at the top).

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
