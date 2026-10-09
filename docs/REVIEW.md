# Lead Review — Color Block Jam Case Study

> Adapted from a Snake review template: "items" → block modifiers (Ice, Arrow); "tick loop" → session + drag loop. Unity 2022.3.62f2, **Built-in RP** (brief-mandated): the URP axis does not apply. Brief budget: 7 days; candidate planned ~45 h, logged ~47 h.

## 1. Architecture map
- `Game.Core` (no UnityEngine): `Grid` (int[] occupancy) → `Board` → `BlockMover` / `ExitRule` → `LevelSession` (state, timer, win).
- Mechanics = `IModifier` + part interfaces (`ISuspender`, `IMoveConstraint`, listeners); `Capabilities` / `EventDispatcher` apply them; listeners write through deferred `LevelCommands`.
- `Game.LevelIO`: Newtonsoft JObject ↔ `LevelData`; each `ModifierData` reads and writes itself (no reflection).
- `Game.Meta` (no UnityEngine): `Wallet`, `Progression`, `Settings`, `ContinuePrice`; one save section each, behind `ISaveStore`.
- `Game.Infrastructure`: VContainer root installer, `AssetScope` per `LifetimeScope`, Addressables scene loader under a cover, `Navigation`, `JsonSaveStore`.
- `Game.Runtime`: `GameplayEntry` (composition) → `GameplayDirector : ISessionObserver` → two `Sequencer`s → views. `DragController` / `DragResolver` drive input into `LevelSession.TryMove`.
- UI: one `ModalLayer` per content scene; panels (LevelComplete/Fail) live in the scene, popups load by key; MVP presenters are pure C#.
- `Game.LevelEditor` (Editor): model, rules via `TypeCache`, ▶ Play → `Game.LevelTest` (UNITY_EDITOR-only start with an in-memory save).
- Tests: 316 `[Test]` + 60 `[TestCase]` EditMode, 2 PlayMode smoke tests, plus an additivity assembly that adds a mechanic from outside.

## 2. The three questions

**Q1 — Core loop.** The grid is the single source of truth: occupancy is an `int[]` of entity ids, moves are discrete cell offsets, and views only read `Block.Position`. The timer runs on `Time.deltaTime` through `session.Tick`, and moves are driven by input, not by a fixed step. That is correct for this genre and does not depend on frame rate.
```csharp
// BlockMover.cs:18 / Board.cs:40
if (board.CanPlace(block, direction.ToOffset())) return MoveResult.Moved;
internal void MoveEntity(Entity entity, Cell offset) { Fill(entity, Grid.Empty); entity.Position += offset; Fill(entity, entity.Id); }
```

**Q2 — Third mechanic test.** A third mechanic needs 2 new Core files (`XData`, `X`) and 1 line in `ModifierCatalog.Default()`. Its look needs 2–4 new view files (presenter, view, optional look and view interface), 1 line in `ModifierPresenters.Default()` and 1 field in `ModifierViews`. That is 3 edited files, and the editor picks the new mechanic up through `Write`/`Read`. A new *field kind* (for example color) touches `IModifierWriter/Reader`, the JSON reader and writer, `ModifierFields`, `FieldKind` and the inspector. A designer retunes per-level values in the editor and global feel in serialized settings and `GameConfig`.
```csharp
// ModifierCatalog.cs:16
new ModifierCatalog().Add(() => new IceData()).Add(() => new ArrowData());
```

**Q3 — Separation.** Yes: the full rule set, including win, fail, continue, Ice and Arrow, runs with no scene. Even the Runtime loop (director, drag, sequencers) is driven headlessly with fakes in `HotPathAllocationTests`.
```csharp
session = LevelSession.TryCreate(level).Value;            // HotPathAllocationTests.cs:34
Assert.That(() => loop.Tick(0.016f), Is.Not.AllocatingGCMemory());
```

## 3. Top 5
- `[P1] LevelFailPresenter.cs:52` — Continue stays clickable while the panel closes → a double tap pays 900 + 1900 coins for one continue.
- `[P1] LevelValidator.cs:9` — the closed-frame invariant Core relies on is checked only in the Editor → a hand-edited or remote level reads the wrong row or throws.
- `[P1] GameplayEntry.cs:97` — when a level fails to load or validate, the cover lifts on an empty scene with no HUD → hard soft-lock.
- `[P2] GameplayPopups.cs:53` — pause applies only after the popup prefab loads → a timeout in that window leaves the player in Failed with no panel.
- `[P2] JsonSaveStore.cs:49` — `File.Copy` over the target is not atomic → a torn write resets that section (coins re-granted, progression back to 0).

## 4. Findings
| # | Sev | File:line | Title |
|---|---|---|---|
| 1 | P1 | LevelFailPresenter.cs:52 | Continue double-charge |
| 2 | P1 | LevelValidator.cs:9 | Frame invariant not enforced at runtime |
| 3 | P1 | GameplayEntry.cs:97 | Load failure soft-locks |
| 4 | P2 | GameplayPopups.cs:53 | Pause applied after the async load |
| 5 | P2 | JsonSaveStore.cs:49 | Non-atomic save |
| 6 | P2 | GameplayDirector.cs:44 | Listener-driven moves and adds are dropped by the view |
| 7 | P2 | GameplayEntry.cs:19 | 196-line composition root, 16 dependencies |
| 8 | P2 | — | No lifecycle pause, remote config, kill switch or analytics |
| 9 | P3 | LevelSession.cs:82 | `moveInProgress` survives a fail |
| 10 | P3 | LevelSession.cs:115 | A level with no blocks never wins |
| 11 | P3 | GameplayLifetimeScope.cs:38 | Dead dev hooks |

## 5. Detail

**#1 [P1] LevelFailPresenter.cs:52 — Continue double-charge**
```csharp
if (!price.TryPay(wallet)) return;
actions.Continue(seconds);   // ModalLayer.CloseAsync then animates for 0.2 s; the panel still takes raycasts
```
Problem: closing a modal never turns its raycasts off (`ModalLayerView.Pop`, line 54). A second tap pays again: the price rises and another 20 s is added.
Impact: a player loses 1900 extra coins. On a live title that means refunds and support tickets.
Fix:
```csharp
private bool offered;
public void Show(FailContent content) { offered = true; view.ShowContent(...); ShowWallet(); }
private void Continue()
{
    if (!offered || !price.TryPay(wallet)) return;
    offered = false;
    actions.Continue(seconds);
}
// and for every modal: ModalLayerView.Pop(..., open:false) → GroupOf(modal).blocksRaycasts = false; Show() sets it true
```
Repro: fail with ≥ 2800 coins and double-tap Continue → coins drop by 2800.

**#2 [P1] LevelValidator.cs:9 — Frame invariant not enforced at runtime**
```csharp
public int IndexOf(Cell cell) => cell.Y * Width + cell.X;          // Grid.cs:23, no bounds
while (occupant == Grid.Empty) { cell += offset; occupant = grid.Get(cell); }  // ExitRule.cs:26
```
Problem: `EdgeCellsRule` ("lets Core skip bounds checks") lives in the Editor assembly. `LevelSession.TryCreate`, described as "the only way in", does not check it, and levels carry the `remote` label (D115).
Impact: x = −1 silently reads the previous row's last cell, and y = −1 throws `IndexOutOfRangeException` mid-drag.
Fix:
```csharp
// LevelValidator.Validate, after the entity loops (add LevelErrorKind.OpenFrame)
for (int y = 0; y < level.Height; y++)
for (int x = 0; x < level.Width; x++)
{
    bool edge = x == 0 || y == 0 || x == level.Width - 1 || y == level.Height - 1;
    string owner = owners[y * level.Width + x];
    if (edge && (owner == null || owner.StartsWith("Block")))
        errors.Add(new LevelError(LevelErrorKind.OpenFrame, $"Edge cell ({x}, {y})"));
}
```
Repro: delete one edge wall cell in a level JSON, then drag a block into that gap.

**#3 [P1] GameplayEntry.cs:97 — Load failure soft-locks**
```csharp
Debug.LogError($"Level '{key}' could not be loaded: {level.Error}");
return;            // StartAsync then calls cover.Hide(): empty board, HUD unwired, no popup
```
Problem: an unknown key or an invalid level ends in a dead scene. `Bootstrapper.StartAsync` has no failure path either: the run stays on the camera background.
Impact: the player can only force-quit. In release builds the error log is the only signal.
Fix:
```csharp
if (!await BuildAsync(cancellation)) { navigation.GoHome(); return; } // Home lifts the cover
cover.Hide();
```
Repro: put a missing key in `GameConfig.levelKeys` and press Play on Home.

**#4 [P2] GameplayPopups.cs:53 — Pause applied after the async load**
```csharp
settingsView = await popups.GetAsync<SettingsView>(PopupKeys.Settings, cancellation); // ticks still running
await modals.Open(...)   // ModalLayer.cs:43 pauses only here
```
Problem: on the first open the clock and input run during the Addressables load. If time runs out, the fail panel opens, then Settings replaces it, and Settings' X resumes into `Failed` with the HUD off.
Impact: a rare soft-lock that QA reports as "can't reproduce".
Fix:
```csharp
public void OpenSettings() { play?.Pause(); OpenSettingsAsync(life.Token).Forget(); } // same for OpenRetry
```

**#5 [P2] JsonSaveStore.cs:49 — Non-atomic save**
```csharp
File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));
File.Copy(temporaryPath, path, true);   // rewrites the target in place
```
Problem: the comment promises that a crash never leaves half a section, but `Copy` truncates and rewrites the target file. A torn file falls to `Load`'s fresh section.
Impact: coins reset to the start grant and progression drops to 0 after a crash or power loss.
Fix:
```csharp
if (File.Exists(path)) File.Replace(temporaryPath, path, null); else File.Move(temporaryPath, path);
```
(needs verification on an IL2CPP Android device; `File.Replace` maps to `rename`)

**#6 [P2] GameplayDirector.cs:44 — Listener-driven moves and adds are dropped by the view**
```csharp
public void OnEntityMoved(Entity entity, Cell offset) { }
public void OnModifierAdded(Entity entity, IModifier modifier) { }
```
Problem: Core reports `LevelCommands.MoveEntity` and `AddModifier`, and the view ignores both. `BlocksView` also reads `Colors.Of` only once, at build (line 75).
Impact: the first push, conveyor or paint mechanic breaks visually while every Core test passes.
Fix: route both calls to `blocks.ViewOf(block).SnapTo(...)` and `presenters.Find(m)?.Show(...)`, and add a director test with a fake that moves an entity.

**#7 [P2] GameplayEntry.cs:19 — 196-line composition root, 16 dependencies**
Problem: it loads, builds the board, blocks and audio, and wires the director, drag, HUD, popups and panels. It breaks the project's own 150-line rule, and the README admits it.
Impact: each new screen widens this constructor, and setup order bugs (#4) cluster here.
Fix: extract `GameplayUiWiring.Bind(loop, session, continuePrice, levelNumber)` (HUD, popups, panels) and a `LevelBuilder` (board, blocks).

**#8 [P2] No lifecycle pause, remote config, kill switch or analytics**
Problem: nothing opens Pause when the app is backgrounded, so the player returns to a running clock (D132 T3). `GameConfig` sits in the local `Boot` group, so economy values (`continueBasePrice`, `winReward`) and mechanics cannot be retuned or turned off without a build. There are no events for level start, win, fail kind, continue bought or session length.
Impact: live-ops cannot balance a continue price it cannot measure.
Fix: add `IAppLifecycle` → `GameplayPopups.OpenSettings` on pause, an `IAnalytics` stand-in already shaped by Extending §6.2, and move `GameConfig` to the `remote` label.

**#9 [P3] LevelSession.cs:82 — `moveInProgress` survives a fail**
```csharp
if (State != GameState.Playing || !moveInProgress) return;
```
Problem: when time runs out mid-drag the flag stays true, so after Continue the next commit also counts the earlier partial move, and `IMoveListener` fires late.
Fix: `if (!moveInProgress) return; if (State != GameState.Playing) { moveInProgress = false; return; }`

**#10 [P3] LevelSession.cs:115 — A level with no blocks never wins**
Problem: `CheckWin` runs only after an exit or a flush, and no editor rule rejects a level without blocks, so ▶ Play waits for the timeout.
Fix: `if (level.Blocks.Length == 0) errors.Add(new LevelError(LevelErrorKind.NoBlocks, "Nothing to clear."));`

**#11 [P3] GameplayLifetimeScope.cs:38 — Dead dev hooks**
```csharp
// Development hooks, kept until the popups own pause and restart (U2, U3)
```
Problem: U2, U3 and M3 are done. The same kind of hooks remain in `RootScope.cs:43`, and `PlayLauncher` is a one-line passthrough. The brief scores "no dead code".
Fix: delete them.

## 6. Edge cases
| Case | Status | Proof |
|---|---|---|
| Block exits mid-drag | HANDLED | DragController.cs:91 |
| Drag during pause / win / fail | HANDLED | DragController.cs:57, InputLock |
| Win and timeout on the same frame | HANDLED | LevelSession.cs:99 (`State == Playing` guard) |
| Timeout mid-drag, then Continue | BROKEN | #9 |
| Two exits wear the same Ice in one flush | HANDLED | LevelCommands.cs:73 |
| Ice + Arrow on one block | HANDLED | Capabilities.cs:5, StackingTests |
| Frozen block picked | HANDLED (feedback only) | BlockMover.cs:15 |
| Wrong-color or wrong-direction door | HANDLED | ExitRule.cs:35 |
| Open frame in level data | BROKEN | #2 |
| Restart during exit animation / popup tween | HANDLED | GameplayLoop.cs:57, BlockView.cs:135 |
| Continue double-tap | BROKEN | #1 |
| Pause tap during the first popup load near 0 s | BROKEN | #4 |
| Level load / validation failure | BROKEN | #3 |
| Coins after leave and return | HANDLED | Wallet.cs:33, `Coins_StayCorrect_AfterLeavingAndReturning` |
| Wrap after the last level | HANDLED | Progression.cs:30 |
| Level with no blocks | BROKEN | #10 |
| App backgrounded mid-level | UNVERIFIED (no auto-pause; `deltaTime` capped by `maximumDeltaTime`) | #8 |
| Multi-touch while dragging | UNVERIFIED (mouse emulation) | MousePointerInput.cs:22 |
| Torn save file | BROKEN | #5 |

## 7. Scaling verdict
- 10 mechanics: Core scales (part interfaces, one catalog line each). Runtime keeps two registries (`ModifierPresenters` + a `ModifierViews` field each), so merge them into one SO list.
- Mechanics that move, recolor or add at runtime break the view until #6 is fixed.
- New field kinds touch 6 files. The closed `FieldKind` set is the editor's real extension cost.
- Rarity, weighting and live-ops tuning have no remote path (#8). Level order is a local `string[]`.
- `Capabilities` / `EventDispatcher` are O(entities × modifiers) per preview or event: fine up to ~20×20 boards, so no action needed.

## 8. Case study evaluation

| Requirement | Status |
|---|---|
| 4.1 Home (tabs, level button, settings, 20:9) | Met |
| 4.2 Settings toggles + close | Met |
| 4.3 Five levels, timer, coins, fail, pause / restart / home | Met (fail → home is two taps: X → Play popup) |
| 4.4 Visual editor, one format, make → save → play | Met, strong |
| Optional: Ice, Arrow | Met |
| Optional: boosters, solver | Missing (declared) |
| Deliverables: README | Met |
| Deliverables: APK / video | Partial: `PRODUCTION_VIDEO_URL` placeholder at README.md:62 |

**Over-engineering** (against 7 days)
- Remote profile, catalog update and cache cleaning with no server and no remote content to ship.
- A separate Editor-only start (`Game.LevelTest`, second root scope, memory save): correct, but a large surface for a case.
- ~140 decisions across two plans; the paperwork outweighs the code it governs.

**Under-engineering**
- Failure paths: #3, #4 and #5 all end in soft-locks or data loss.
- Runtime trust of level data (#2).
- No lifecycle hook (#8).

**Repo hygiene**
- 154 commits, terse messages ("add draw rule"), no conventional prefixes, one merge commit on `main`.
- `docs/production/Production_Starting .txt`: personal Turkish notes and a space in the filename; remove it.
- Docs drift: ProductionV2 / D116 say `Level_1`–`Level_5`, while `GameConfig.asset` lists 15 keys (skipping 6–7).
- `Library/`, `Temp/` and `*.csproj` are correctly ignored; the Prototype assembly is `UNITY_EDITOR`-only, so it never reaches the build.
- Folder layout is by concept with depth ≤ 2, as the project's rules require.

| Axis | Score |
|---|---|
| Simulation correctness /25 | 21 |
| Mechanic system design /20 | 17 |
| Architecture & testability /20 | 18 |
| Performance & Unity idiom /15 | 12 |
| Code quality /10 | 8 |
| Presentation & hygiene /10 | 7 |
| **Total** | **83** |

**Hire:** Yes, at Senior (mid-senior on live-ops).
- The compiler-enforced logic/view split, a single observer seam and headless tests of the whole loop are what we ask seniors to build.
- Measured performance work: Android draw calls 183 → 15, 0 B/frame guarded by a test, DOTween closure garbage found on an IL2CPP device.
- Gaps are in failure modes and live-ops readiness (#1–#5, #8), not in design. They are teachable, but they are exactly what bites in production.

**Interview questions**
1. Why does Core skip bounds checks, and who owns that invariant once levels ship remotely?
2. Walk me through what happens on a double tap of Continue.
3. A conveyor mechanic moves blocks from a listener. What breaks first?
4. Prove `JsonSaveStore` is crash-safe on Android.
5. Why do panels run on the director's sequencer while popups are fire-and-forget on `life.Token`?
6. What does the player see if `Level_8` fails to load in release?
7. How would you A/B test the continue price without a client update?
8. `GameplayEntry` has 16 dependencies. Split it live: what goes where, and why?

## 9. Action plan
**Must fix before submitting**
- #1 Continue guard + `blocksRaycasts` off on close: 30 min
- #3 failure → GoHome, Bootstrapper try/catch: 45 min
- #11 delete dev hooks, `Production_Starting .txt`, fill the README video/APK: 20 min

**If time permits**
- #2 frame check in `LevelValidator` + test: 40 min
- #4 pause on press: 15 min
- #5 atomic save + device check: 30 min
- #9, #10: 20 min

**Backlog**
- #7 split `GameplayEntry`: 2 h
- #6 view handling of command moves and adds: 3 h
- #8 lifecycle adapter, analytics interface, remote `GameConfig`: 1 day
- Merge `ModifierViews` into the presenter registry: 1 h
