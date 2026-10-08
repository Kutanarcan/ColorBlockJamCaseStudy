---
paths:
  - "Assets/Scripts/**"
---

# Architecture

## C# Core, thin Unity shell
Game logic lives in pure C# layers that do not reference `UnityEngine`, enforced by assembly definitions:

```
Assets/Scripts/
  Core/           (Game.Core.asmdef)           -> NO UnityEngine reference
  Meta/           (Game.Meta.asmdef)           -> NO UnityEngine reference
  LevelIO/        (Game.LevelIO.asmdef)        -> Core + Newtonsoft
  Infrastructure/ (Game.Infrastructure.asmdef) -> Core, LevelIO, Meta + Unity packages
  Runtime/        (Game.Runtime.asmdef)        -> Core, LevelIO, Meta, Infrastructure + UnityEngine
  LevelTest/      (Game.LevelTest.asmdef)      -> Meta, Infrastructure, Runtime; UNITY_EDITOR only, never in a player
  Tests/          one test assembly per tested assembly; PlayMode only where Unity is required
```

Calling a Unity API from Core or Meta is a **compile error**. The full table is in the active plan (ProductionV2 §9).

## MonoBehaviour = dumb adapter
A MonoBehaviour may only:
- Carry serialized data from the Editor (`[SerializeField]`)
- Forward lifecycle events into Core
- Call Unity APIs (Instantiate, Transform, Animator, Audio…)

Forbidden: business-rule `if`s, calculations, state machines, caching logic, data transformation.

## View / Presenter
- **A view never references `Game.Core`.** It exposes dumb setters in Unity terms (`SetCount(int)`, `Place(Vector3)`, `SetMaterial(Material)`) and nothing else.
- **A presenter is pure C#** (Runtime): it reads the logic — casts, modifier parts, rules such as `BlockAnchor` — and calls the view's setters. Type checks ("which modifier is this?") live in the presenter, never in a view.
- The presenter, not the view, is what the director talks to; views are reached only through presenters.

- Never do work inside `Update()` — call `ITickable.Tick(float dt)`.
- No singletons, no service locator, no static state.
- Composition root (D64):
  - **Until Infrastructure (P0–P9):** one manual `GameplayInstaller : MonoBehaviour` per scene.
  - **From I0:** VContainer `LifetimeScope`s: a root scope in Bootstrap, `MainLifetimeScope` and `GameplayLifetimeScope` as its children. Only a `LifetimeScope` registers or resolves; nothing else touches the container.
  - **From M0b:** two root scopes, one per start scene, both deriving from `RootScope`: `RootLifetimeScope` (Bootstrap, the game) and `LevelTestLifetimeScope` (LevelTest, Editor only). See § Start services.
- Plain C# classes take dependencies through their constructor. MonoBehaviours have no constructors — they receive dependencies through `Initialize(...)` (or a VContainer `[Inject]` method once scopes exist), never by `Find` or `GetComponent` on other objects.

## Start services: game and level test (D131)
The game and the Level Editor's test are two starts. The test reuses the game's code; the game never knows the test.

- **Live code never names a test type.** No `if (levelTest)`, no `#if` choosing a test type, no test stand-in in Core, Meta, Infrastructure or Runtime. Test stand-ins live in `Game.LevelTest`; the assembly references make the reverse a compile error.
- **Where a registration goes:**
  - `RootInstaller` (shared): only what must behave the same in the game and in a test (content, assets, scenes, config, meta *logic*, the bootstrapper).
  - `LiveServicesInstaller` / `LevelTestServicesInstaller` (start services): anything a test must not run for real or must run differently: storage, level choice, starting values, and every cross-cutting service with an outside effect (analytics, live events, ads, purchases, notifications, cloud save).
  - When in doubt, it is a start service. A service registered in `RootInstaller` runs for real in every level test.
- **Both start installers register the same contracts.** A contract added to one is added to the other in the same change; the live side gets the real implementation, the test side a null / in-memory stand-in or a test value.
- **Cross-cutting services come from the root.** Child scopes (`GameplayLifetimeScope`, `MainLifetimeScope`) and their entry points get them by injection from the parent; they never `new` one (e.g. an analytics or haptics service built inside `GameplayEntry`), because a start scene could not replace it.
- A level test may **read** live data once while installing (the player's settings); it never holds a live store, so it cannot write one.
- Recipe: `docs/production/Extending.md` § 6.

## Saved data (D128)
- No shared save model. Each feature owns its section: a `[Serializable]` data class with its own `version`, and a `SaveKey` constant on the feature.
- Features reach storage only through `ISaveStore`; they read their section on first use and write only their section on change.
- A new feature adds a section; it never adds fields to another feature's data.

## Design patterns
- Simplest working solution first. Propose a pattern only for a concrete problem: an axis of change, a testing barrier, the third repetition.
- When proposing a pattern, state in one sentence **what it solves**. If you cannot, do not propose it.
- Banned reflexes: singleton for everything, factory with a single implementation, unnecessary observer layers, premature abstract factory.
