<div align="center">

# ⚡ Performance

**What was measured in the presentation stage (P8), what was fixed, and which small costs were accepted on purpose**

![Draw calls](https://img.shields.io/badge/Draw_calls-183_→_15-2ea043)
![GC per frame](https://img.shields.io/badge/GC_per_frame-0_B-2ea043)
![GC per exit](https://img.shields.io/badge/GC_per_exit-~0.5_KB_accepted-d29922)

<sub>[README](../../README.md) · [ProductionV2 §8](ProductionV2.md#-8-performance) · [Decision Log](ProductionV2.md#-14-decision-log)</sub>

</div>

> [!IMPORTANT]
> **Verdict:** nothing allocates per frame, and the board draws in 15 calls. What is left is a few hundred bytes of garbage **per player action** (an exit, a win), not per frame. It was measured, understood, and **left in on purpose**: the fixes exist and are listed below, but they add code and risk for a cost the incremental GC absorbs.

| # | Section |
|---|---|
| 1 | [Setup](#1-setup) |
| 2 | [Draw calls](#2-draw-calls) |
| 3 | [Garbage](#3-garbage) |
| 4 | [Accepted costs](#4-accepted-costs) |
| 5 | [When to revisit](#5-when-to-revisit) |

---

## 1. Setup

| Where | Build | Used for |
|---|---|---|
| Editor | Mono, Game view 1080×1920, `Level.json` | Draw calls (Stats, Frame Debugger), first GC look |
| Android phone | IL2CPP, Development build, Script Debugging off, **Deep Profiling Support** on, profiler over USB (adb) | GC per action, down to the call that allocates |

- GC is read under `FrameTicker.Update` only, so the Editor's and the profiler's own allocations are not counted.
- Logic hot paths (session tick, moves, the drag's previews) are also guarded by `HotPathAllocationTests`: they must allocate nothing.
- Development and Deep Profile builds are slower and instrumented; times are only compared with each other.

## 2. Draw calls

| Full board (`Level.json`) | Before | Instancing (D108, step 1) | One block material, per-instance color (D108) |
|---|---|---|---|
| Draw calls (Frame Debugger) | 183 | 34 | **15** |
| Batches (Stats) | 181 | 32 | **14** |
| SetPass calls | 14 | 14 | **13** |
| Saved by batching | 0 | 14 | **165** |
| No blocks left: batches / SetPass | 135 / 8 | — | **9 / 8** |

**What was done**
- GPU instancing on every material that repeats: ground, walls, door arrows, arrows, ice, blocks. Walls and door arrows use instanced copies of the kit's FBX-embedded materials (D108).
- Blocks share **one** material; their color is a per-instance property, so pieces of one mesh draw together whatever their color (D108).

**The 15 calls that are left, and why**

| Calls | What | Why it stays separate |
|---|---|---|
| 1 | Ground | — |
| 2 | Wall, Corner | Different meshes; instancing joins only same mesh + material |
| 4 | Doors | One material per color, one door per color in this level |
| 1 | Door arrows | — |
| 3 | Blocks | One call per piece mesh in use (Edge, OuterCorner, InnerCorner), all colors together |
| 2 | Ice | One call per piece mesh in use (Edge, OuterCorner) |
| 1 | Arrow | Its own material |
| 1 | Ice count text | TextMeshPro material |

## 3. Garbage

### Per frame
**0 B** while idle, dragging and releasing (Editor and `HotPathAllocationTests`).

### Per action, step by step

| Action | Before | After DOTween init at load (P8) | After cached tween delegates + recycling (D109), Android |
|---|---|---|---|
| Release (snap) | — | 0.6 KB (Android) | 0 B on the tween path (same path as the exit tweens below) |
| First exit | 10.1 KB (Editor) | 6.8 KB (Editor, Deep Profile) | 3.9 KB, **one time** |
| Later exits | — | 0.7 KB (Android) | **~0.5 KB** |
| Win | — | — | 2.3 KB, **once per level** |

**First exit (3.9 KB): one time only.** DOTween fills its tween pool and UniTask its source pools on first use. From the next exit on, these paths show **0 B**.

![First exit](DeepProfile_ExitMove_1.png)

**Later exits (~0.5 KB): what is left.**

![Later exit](DeepProfile_ExitMove_2.png)
![Later exit, expanded](DeepProfile_ExitMove_3.png)

| Item | Size | Source |
|---|---|---|
| `ExitSteps.For` | 264 B per exit | The exit's own objects: `ExitPath` with its cell array, `BlockExitView`, `ExitStep` |
| `ToUniTask(…, token)` | 48 B per tween; a block of N rows has N + 2 tweens | Each awaited tween registers on the cancel token |
| `ExitStep.Play`, `Sequencer.RunAsync` | 96 B + 80 B | Async state machines built as classes in this build (`<Play>d__…ctor` shows); expected to vanish without Deep Profiling Support, where they are pooled structs |

**Win (2.3 KB): once per level.** The win sequence is built (steps, `StepSequence`), plus UniTask's first-use setup.

![Win](DeepProfile_ExitMove_4.png)

**Earlier captures** (before D109): [first exit](DeepProfile_Android.png), [first exit, tween creation](DeepProfile_Android_2.png).

**Scale.** A level with ~10 exits produces roughly 10 × 0.5 KB + 2.3 KB ≈ **7–8 KB** of garbage (an estimate from the numbers above). Unity 2022 runs the incremental GC, so this does not show as a frame spike.

## 4. Accepted costs

Measured, understood, and **not fixed on purpose**. Each has a known fix.

| Cost | Size · how often | Fix | Not fixing: pros | Not fixing: cons |
|---|---|---|---|---|
| Exit objects (`ExitPath`, `BlockExitView`, `ExitStep`) | 264 B · per exit | Pool them in `ExitSteps`; `ExitPath.Reset(block, direction)`, cell buffer sized once for the largest block | Objects stay immutable and short-lived; no reset bugs, no stale state between exits | ~264 B per exit |
| Cancel-token registration per tween | 48 B · per tween (N + 2 per exit) | Await tweens without the token; `ExitStep` checks the token between steps. Restart already stops the tween through `Release → DOKill` | Cancel is immediate and explicit: a cancelled sequencer is idle at once (`Restart_LeavesNoViewOrStepFromTheLastAttempt` relies on it) | ~150–250 B per exit |
| Async state machines | 96 + 80 B · per exit, **only if** the build compiles them as classes | Verify first in a build without Deep Profiling Support. If they stay: an `ExitStep` without `async`, finishing through a pooled completion source | `async` / `await` reads like the steps it plays; UniTask pools it in release builds | Possibly 176 B per exit in instrumented builds |
| Win / fail sequence | 2.3 KB · once per level | Build both sequences once at load and reuse them | Built where it is used; simple | One small allocation per level end |
| First-use warm-up (tween pool, UniTask pools, static constructors) | 3.9 KB · once per run | Prewarm while loading: `DOTween.SetTweensCapacity`, one dummy tween and await | Nothing extra at load | A one-time 3.9 KB on the first exit |
| Doors: one material per color | +3 draw calls, +3 SetPass | Per-instance color for doors, as for blocks | Doors keep the kit's material as is | A few calls more |
| Wall and Corner meshes | +1 draw call | Static batching (needs Read/Write on the kit's FBX meshes: a CPU copy) | No mesh memory doubled | One call more |

**Why not now**
- **The rule is met.** Production rules forbid allocation on hot paths, meaning per frame. Every remaining cost is per action, small, and bounded.
- **Budget and scope (D81).** Each fix adds code and state, and pooling brings its own bugs (stale state, double release). The time goes to the brief's required items first.
- **Evidence first.** Nothing here shows as a frame spike on the test phone. Optimizing without a measured problem is against the project's rules.

## 5. When to revisit

- A GC collection shows up as a spike during play on a target device (Profiler: `GC.Collect` in the frame).
- Lower-end devices or a stricter memory budget come into scope.
- Levels grow far beyond ~10 exits, or exits happen many times per second.
- Draw calls or SetPass become the bottleneck in a device GPU or CPU capture.

The order to apply the fixes: token registration (smallest change) → exit object pool → win sequence reuse → warm-up.
