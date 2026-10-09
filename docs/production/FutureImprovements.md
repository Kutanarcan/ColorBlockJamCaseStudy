<div align="center">

# 🔭 Future Improvements

**What a live puzzle title of this kind would add next, measured against industry standards, and where each piece plugs into the current architecture**

![Status](https://img.shields.io/badge/Status-Not_started-6e7681)
![Scope](https://img.shields.io/badge/Scope-Beyond_the_7--day_brief-d29922)

<sub>[README](../../README.md) · [Technology Used](TechnologyUsed.md) · [Extending §7](Extending.md#7-deliberate-trade-offs--future-services-v2) · [REVIEW](../REVIEW.md) · [ProductionV2](ProductionV2.md)</sub>

</div>

> [!NOTE]
> The brief gave 7 days and asked for "a small and complete result". The slice took ~47 hours, and scope was cut on purpose ([ProductionV2 §12](ProductionV2.md#-12-time-budget)). The architecture was built so that each item below is an **addition**, not a rewrite: a new start service, a new save section, a new modifier, a new scope. Where a seam already exists, its row names it.

| # | Section |
|---|---|
| 1 | [Open items from the review](#1-open-items-from-the-review) |
| 2 | [Simulation & determinism](#2-simulation--determinism) |
| 3 | [Time, save & lifecycle](#3-time-save--lifecycle) |
| 4 | [Live-ops & content](#4-live-ops--content) |
| 5 | [Analytics & observability](#5-analytics--observability) |
| 6 | [Monetization & compliance](#6-monetization--compliance) |
| 7 | [Rendering, memory & build](#7-rendering-memory--build) |
| 8 | [Input, audio, feel & accessibility](#8-input-audio-feel--accessibility) |
| 9 | [Architecture growth](#9-architecture-growth) |
| 10 | [Level Editor & content pipeline](#10-level-editor--content-pipeline) |
| 11 | [Engineering pipeline](#11-engineering-pipeline) |
| 12 | [Priority](#12-priority) |

---

## 1. Open items from the review
Found by the lead review ([REVIEW](../REVIEW.md)), left for after submission.

| Item | Today | Next |
|---|---|---|
| Frame check in Core | Only the Level Editor checks that edge cells are walls or doors | `LevelValidator` rejects an open frame, so remote or hand-edited levels cannot break Core (#2) |
| Pause on press | The clock stops only after the popup prefab loads | Pause on the button press (#4) |
| Atomic save | `File.Copy` over the target | `File.Replace` / `File.Move`, verified on an IL2CPP device (#5) |
| Listener-driven view changes | The director ignores entity moves and modifier adds raised by commands | Views follow them, before a push, conveyor or paint mechanic ships (#6) |
| `GameplayEntry` size | ~200 lines, 17 dependencies | Split into a level builder and UI wiring (#7) |
| Small logic edges | Move flag across a fail; a level with no blocks never wins | #9, #10 |

## 2. Simulation & determinism

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **Fixed-step session tick** | The timer runs on `Time.deltaTime` today. A fixed step (accumulate `dt`, advance in whole ticks, count frames as ticks) makes every run reproducible regardless of frame rate or spikes | `GameplayLoop.Tick` accumulates; `LevelSession.Tick` receives whole steps. Core does not change |
| **Move log & deterministic replay** | Record `(tick, block, direction)` per move. A bug report carries its replay; QA reproduces it exactly; the same log drives automated playtests | The session is already the only door for moves (`TryMove`), so a recorder wraps it |
| **Server-side result check** | A competitive event or leaderboard replays the move log on the server with the same Core | Core is pure C#, so the same assembly runs on a .NET server |
| **Level solver** | The brief's optional "unsolvable level" warning; also the minimum move count, used as a difficulty signal | A search (BFS / A*) over board states in an Editor or Core-side assembly, using `BlockMover.Preview` |
| **Seedable randomness behind an interface** | Any future random content (booster drops, generated levels) stays reproducible | `IRandomSource` in Meta / Core, like `IClock` |

## 3. Time, save & lifecycle

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **App lifecycle adapter** | Backgrounding the app opens Pause; audio pauses; pending writes flush | `IAppLifecycle` + one forwarding MonoBehaviour at the root ([Extending T3](Extending.md#71-trade-offs-taken)) |
| **UTC + monotonic clock (`IClock`)** | Lives refill, daily rewards and events must not be cheated by turning the device clock. Store the last known time, compare against monotonic elapsed time, correct by server time | [Extending §7.3](Extending.md#73-future-live-service-utc--monotonic-time); a movable test clock in `Game.LevelTest` |
| **Save versioning & migration** | Each section already has a `version`; a migration step per version keeps old saves valid after an update | `JsonSaveStore.Load` runs migrations by section |
| **Save integrity** | A checksum (or signing) per section stops trivial coin edits; corrupted files are detected, not silently reset | `ISaveStore` decorator, no feature change |
| **Cloud save & account** | Progress survives a reinstall or a new device (Google Play Games / Apple Game Center / own backend) | A new `ISaveStore` behind the start installers; conflict rule by `version` + timestamp |
| **Write batching** | Many small writes on one frame (win = wallet + progression) become one flush | `ISaveStore` decorator that flushes at the end of the frame and on pause |

## 4. Live-ops & content

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **Remote config** (Unity Remote Config, Firebase Remote Config) | Economy (continue price, reward, start coins), timers and level order change without a client update | `GameConfig` values overlaid by a remote source at start-up (an `IStartupStep`, [Extending T5](Extending.md#71-trade-offs-taken)) |
| **Feature flags & kill switches** | Any mechanic, booster or popup can be turned off live if it breaks | Flags in remote config, read by the installers and presenters |
| **A/B testing** | Price, reward and difficulty are tuned by cohort, not by opinion | Remote config cohorts + analytics events (§5) |
| **Remote levels over a CDN** | New levels ship weekly without a store release | Already prepared: `Levels` group with Remote paths and the `remote` label, content update flow (D65, D115). Needs a hosted bucket and a Remote profile load path |
| **Remote level order** | Level order and difficulty curve change live | `GameConfig.levelKeys` moved to the remote catalog or remote config |
| **Lives system** | Brief placeholder today; standard in the genre, with a refill timer | A Meta feature + save section; needs `IClock` (§3) and service → UI notifications ([Extending T4](Extending.md#71-trade-offs-taken)) |
| **Functional boosters** | FreezeTime and Hammer (ProductionV2 Stage G) | FreezeTime pauses ticks; Hammer needs a Core removal operation (Q6) |
| **Live events & daily rewards** | Retention loops: daily login, streaks, limited-time events | Live service on `IClock` (§3), one save section per feature |

## 5. Analytics & observability

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **Gameplay analytics** (Firebase Analytics, GameAnalytics, Unity Analytics) | Level start / complete / fail with fail kind, moves, time left, continues bought, coins earned and spent: the data a level designer and live-ops balance with | `IAnalytics` start service: real in `LiveServicesInstaller`, null in the level test ([Extending §6.2](Extending.md#62-a-cross-cutting-service-eg-analytics)) |
| **Level funnel & difficulty dashboard** | Find the level where players churn | The events above, keyed by level key |
| **Crash & ANR reporting** (Firebase Crashlytics, Backtrace, Sentry) | Crash-free users is a store-ranking metric | Root service; also catches the `UniTask` unobserved exceptions |
| **Release log policy** | No `Debug.Log` cost or noise in release; warnings and errors kept | A logger behind an interface with `[Conditional]` calls |
| **Performance telemetry** | Frame time and memory by device tier from real players | `ProfilerRecorder` sampling, sent as analytics |

## 6. Monetization & compliance

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **Rewarded ads** (AppLovin MAX, Unity LevelPlay mediation) | "Watch an ad to continue" next to "pay coins": the genre's main revenue loop | A second continue action on LevelFail, behind an `IAds` start service |
| **Interstitials with frequency caps** | Between levels, never inside one | On LevelComplete → Continue, rules in remote config |
| **In-app purchases** (Unity IAP) | Coin packs, no-ads, booster bundles; receipt validation on a server | Wallet already has a single `Add` door; purchases become one more source |
| **Consent & privacy** (Google UMP, iOS ATT, GDPR / COPPA) | Required before ads and analytics start | A start-up step before those services initialize |

## 7. Rendering, memory & build

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **URP for mobile** | SRP Batcher, per-tier URP assets (shadows, MSAA, render scale) and better tooling. The brief named no pipeline, so Built-in was kept | Port the three custom shaders; instancing becomes SRP Batcher compatibility |
| **Quality tiers by device** | Low-end Android gets lower render scale and effects | Device profiling at start-up, a tier in config |
| **Texture & audio import standards** | ASTC compression, sprite atlases, compressed / streamed audio by clip length | Import presets per folder |
| **Memory budgets per scene** | Addressables bundles measured per scope; a budget fails CI when exceeded | Asset scopes already count handles (D113) |
| **Profiler markers on hot paths** | `ProfilerMarker` around drag, tick and exit steps makes device captures readable without Deep Profile | Runtime hot paths |
| **IL2CPP stripping policy** | Higher managed stripping for a smaller APK, with `link.xml` / `[Preserve]` for VContainer's constructor injection, or its source generator | Build settings + one `link.xml` |
| **Android App Bundle + Play Asset Delivery** | Google Play requires AAB; large content can be delivered as asset packs | Build pipeline; Addressables groups map to packs |

## 8. Input, audio, feel & accessibility

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **New Input System** (EnhancedTouch) | Explicit multi-touch handling (a second finger never steals a drag), better device support | A new `IPointerInput`; nothing else changes (D106 already uses its vocabulary) |
| **Haptics** | The vibration toggle acts: light taps on select, drop and exit | A haptics start service, read by the SFX points |
| **Audio service & music** | Music across scenes, ducking, background pause | Root `AudioService` ([Extending §7.2](Extending.md#72-future-audioservice)) |
| **Color-blind support** | A color-matching game must be playable without color: a symbol or pattern per palette color on blocks and doors | Palette asset gains a symbol per color; block and door views show it |
| **Localization** (Unity Localization package) | Every UI string from tables; RTL and font fallback | `FailContent` and popup texts become localized references |
| **Juice pass** | Combo feedback, win celebration, idle hints for stuck players | New steps in the director; a hint uses the solver (§2) |

## 9. Architecture growth

| Improvement | Why | Where |
|---|---|---|
| **Start-up step list** | Remote config, server time, consent and ads all add async start-up steps | `IStartupStep` ordered by the installer ([Extending T5](Extending.md#71-trade-offs-taken)) |
| **Service → UI change notification** | Lives refilling or an event starting change the UI without a player action | Decide once per Runtime ([Extending T4](Extending.md#71-trade-offs-taken)) |
| **One modifier view registry** | Today a look needs a line in `ModifierPresenters` and a field in `ModifierViews` | One list of (presenter, prefab) entries in a ScriptableObject |
| **Generic modifier field kinds** | A new field type (color, cell) touches six files today | A small field-kind registry shared by JSON and the editor |
| **Content shared across starts** | The level test still runs the content update | Move `IContentDelivery` into the start installers ([Extending T6](Extending.md#71-trade-offs-taken)) |

## 10. Level Editor & content pipeline

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **Solvability & difficulty in the editor** | Designers see "unsolvable" and "min moves: 14" before saving | Solver (§2) as an `ILevelRule` |
| **Batch validation** | Every level re-checked after a rule or format change, in CI | A menu item + an EditMode test over `Assets/Levels` |
| **Level browser with thumbnails & tags** | Find levels by difficulty, mechanic, status | Editor window over the level files |
| **Play from any level number** | Test a level inside the real progression, not only alone | A level test option for a progression index |
| **Publish to remote** | Designers ship a level to the CDN without a programmer | Addressables content build + upload from the editor |
| **Level format migrations** | `schemaVersion` upgrades old files automatically instead of by hand (D105) | A migration per version in `LevelIO` |

## 11. Engineering pipeline

| Improvement | Why it is standard | Where it plugs in |
|---|---|---|
| **CI on every push** (GameCI on GitHub Actions) | EditMode + PlayMode tests and an Android build on every change | Tests already run headless |
| **Performance tests in CI** (Unity Performance Testing package) | Allocation and timing budgets checked automatically, not only by hand | `HotPathAllocationTests` become `[Performance]` tests with stored baselines |
| **Code coverage** | Shows which rules have no test | Code Coverage package (already in the development feature set) |
| **Debug menu** | QA forces win, fail, coins, a level number, and timer speed on a device | A Development-only overlay, a start service |
| **Automated playtests** | The move log (§2) replays real sessions after each change | PlayMode test over recorded logs |
| **Commit & release conventions** | Conventional commits, tagged releases, changelog | Repository settings |

## 12. Priority
What a studio would do first if this slice moved toward a soft launch.

| Stage | Items |
|---|---|
| **Before soft launch** | §1 open items · lifecycle pause · atomic + versioned save · analytics events · crash reporting · remote config with kill switches · CI · debug menu |
| **Soft launch** | Remote levels on a CDN · lives with `IClock` · rewarded continue · consent · A/B tests on economy · solver in the editor · color-blind symbols |
| **Scale** | Fixed-step + replay · URP and quality tiers · cloud save · live events · IAP · localization · AAB + asset packs · performance tests in CI |
