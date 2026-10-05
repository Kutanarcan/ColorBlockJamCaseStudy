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

> [!IMPORTANT]
> **📍 Now:** Prototyping — Phases 1–10 done, plus exit, selection and audio feel
> **Next:** Phase 11 · Win

## 🎬 Videos

### Gameplay
<!-- Gameplay_1.mp4: drag the file into the GitHub editor and paste the generated user-attachments URL on the line below -->

<sub>File: [`docs/prototype/Gameplay_1.mp4`](docs/prototype/Gameplay_1.mp4)</sub>

### Marketing · Vol. 1
<!-- Marketing_Vol1.mp4: drag the file into the GitHub editor and paste the generated user-attachments URL on the line below -->

<sub>File: [`docs/prototype/Marketing_Vol1.mp4`](docs/prototype/Marketing_Vol1.mp4)</sub>

## 🗺️ Roadmap

```mermaid
flowchart LR
    A["📋 Planning"] --> B["🧪 Prototype"] --> H["🌾 Harvest"] --> P["🏗️ Production"]

    classDef done fill:#2ea043,color:#fff,stroke:#2ea043
    classDef active fill:#d29922,color:#fff,stroke:#d29922
    classDef todo fill:#6e7681,color:#fff,stroke:#6e7681
    class A done
    class B active
    class H,P todo
```

| Stage | Question it answers | Status | Progress |
|---|---|---|---|
| 📋 Planning | What is the game, and what can the model kit do? | ✅ Done | `██████████` 100% |
| 🧪 Prototype | Can the kit build a playable board with blocks, doors and exits? | 🚧 In Progress | `█████████░` 91% (10/11) |
| 🌾 Harvest | What did we learn? | ⏳ Planned | `░░░░░░░░░░` 0% |
| 🏗️ Production | Can it be rebuilt clean, tested and extensible? | ⏳ Planned | `░░░░░░░░░░` 0% |

---

## 🎮 Core Loop

<sub>Click a tab to open it. Opening one closes the others in this group.</sub>

<details name="core">
<summary><b>🎮 The Game</b></summary>
<br>

A rectangular board is enclosed by walls. Some wall segments are **colored doors**. Blocks are polyominoes (1×1, bars, L, T…), one color each.

- **Move:** drag a block. It slides through empty cells and stops at walls and other blocks. Blocks never overlap.
- **Axis lock:** a block with an arrow on top moves along one axis only.
- **Exit:** a block pushed into a door of **its own color**, where the door covers the block's full width, leaves the board.
- **Win:** every block has exited.
- **The puzzle:** move order. Blocks block each other; the player has to clear paths in the right sequence.

</details>

<details name="core" open>
<summary><b>🧪 Prototype</b> — 10/11</summary>
<br>

**Goal:** answer one question as fast as possible — *can the existing model kit build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?*
The output is **knowledge, not code**: no architecture, no tests, every feel value exposed in the Inspector.

- [x] Grid renders
- [x] 1×1 block
- [x] Block autotile: convex shapes
- [x] Block autotile: concave shapes (L, T, S/Z, +, U, ring)
- [x] Board walls
- [x] Doors
- [x] Drag (free 2D)
- [x] Grid collision
- [x] Axis lock
- [x] Exit through door
- [ ] Win

**Feel added on top**
- Exit: the block snaps to the door, slides in and is **cut by a clip-plane shader** at the door line, with per-row particles.
- Selection: **screen-space outline** around the held block (one clean silhouette for a multi-piece block).
- SFX: select, drop and exit crunch.
- Recording: a hand cursor that follows the mouse from the fingertip, toggled in the Inspector.

**Questions it had to answer**

| # | Question | Answer |
|---|---|---|
| 1 | Are L/T blocks in scope? | ✅ Yes — any polyomino, via a quadrant autotile rule |
| 2 | How are levels authored? | ✅ Inspector field list on `Board` |
| 3 | Which corner piece goes on the board corners? | ✅ One cleaned `Corner` mesh |
| 4 | Where does the door arrow sit? | ✅ Centered on the door top |
| 5 | Free 2D drag or one axis per step? | ✅ Free 2D, clamped to reachable space, snaps on release |
| 6 | Which quadrant does a block piece cover at rotation 0? | ✅ Top-right; InnerCorner pivots on the vertex |
| 7 | Does a block need a separate Visuals level for effects? | ✅ No — tweens move the root, the exit cut is a shader |

Findings → [`FINDINGS.md`](docs/prototype/FINDINGS.md) · Plan → [`PrototypeV1.md`](docs/prototype/PrototypeV1.md) · Algorithms → [`AlgorithmExplanation.md`](docs/prototype/AlgorithmExplanation.md)

</details>

<details name="core">
<summary><b>🧩 How It Works</b></summary>
<br>

| Topic | Approach |
|---|---|
| **Grid** | Cell = 2 units. Two layers: a cell layer (`int[,]`, block id) and an edge layer (`hEdges` / `vEdges`: Open, Wall or Door color). |
| **Block visuals** | Each cell is split into 4 quadrants; each quadrant picks `OuterCorner`, `Edge`, `Center` or `InnerCorner` from its neighbors. A block is dressed once and only its root moves. |
| **Movement** | Cell-by-cell steps toward the pointer, larger axis first. The visual leans up to half a cell, never into a refused cell. No physics colliders. |
| **Exit rule** | A blocked step checks the border: every column / row the block covers must face a door of its color. |
| **Exit visual** | Per-block world clip plane via `MaterialPropertyBlock`; back faces drawn flat so the cut looks solid. |
| **Outline** | Camera `CommandBuffer`: draw the held block into a mask, blit the outline over the frame. |

Full walkthroughs with diagrams and worked examples → [`AlgorithmExplanation.md`](docs/prototype/AlgorithmExplanation.md)

</details>

<details name="core">
<summary><b>🌾 Harvest</b></summary>
<br>

**Goal:** turn prototype knowledge into a production spec.

- [ ] Settled rules & tuning values from `FINDINGS.md`
- [ ] Data shapes & rejected ideas
- [ ] Retire prototype code — read as a spec, **never migrated**

</details>

<details name="core">
<summary><b>🏗️ Production</b></summary>
<br>

**Goal:** rebuild the rules from scratch as a pure C# core with a thin Unity layer.

- **No `UnityEngine` in the core** — enforced by assembly definitions, so rules are tested without opening Unity.
- **Views are dumb** — they display state and forward input.
- **Every phase ships with tests.**

</details>

---

## 🛠️ How I Work

<details name="work">
<summary><b>🧪 Prototype vs 🏗️ Production</b></summary>
<br>

| | 🧪 Prototype | 🏗️ Production |
|---|---|---|
| **Purpose** | Answer one question | Build it to last |
| **Output** | Knowledge | Shippable code |
| **Folder** | `Assets/Prototype/` (Editor-only, excluded from builds) | `Assets/Scripts/` |
| **Architecture** | None, speed first | Pure C# core + thin Unity layer |
| **Tests** | None | Every phase |
| **Progress** | One playable phase at a time | One tested phase at a time |

</details>

<details name="work">
<summary><b>🤖 AI-Assisted Workflow</b></summary>
<br>

AI is used as a pair programmer under rules I designed — not as an autopilot.

| Me | AI |
|---|---|
| Game analysis, phase plan, decisions | Implements one phase at a time |
| Rules & constraints per mode | Follows the active mode's rules |
| Playtesting in the Editor, tuning | Explains the approach before coding |
| Deciding when a phase is done | Stops after every phase |

- **Modes:** `PROTOTYPE` and `PRODUCTION` rule sets; only I switch between them.
- **Folder-scoped rules:** prototype rules can't leak into production code.
- **Findings first:** every settled rule, tuning value and rejected idea goes into `FINDINGS.md`.

Rules → [`CLAUDE.md`](CLAUDE.md) · [`.claude/`](.claude/)

</details>

<details name="work">
<summary><b>📁 Project Layout</b></summary>
<br>

```
Assets/
  Art/            models, textures, audio, UI sprites
  Prefabs/        GroundTile, BlockPiece, ArrowPiece, WallPiece, DoorPiece, BlockExitParticles
  Prototype/      disposable prototype code + shaders (Game.Prototype.asmdef, Editor-only)
  Scenes/         Prototype.unity
  Plugins/        DOTween
docs/
  prototype/      PrototypeV1.md, FINDINGS.md, AlgorithmExplanation.md, videos
```

</details>

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a></sub>
