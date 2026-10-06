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
  Tests/          one test assembly per tested assembly; PlayMode only where Unity is required
```

Calling a Unity API from Core or Meta is a **compile error**. The full table is in the active plan (ProductionV2 §9).

## MonoBehaviour = dumb adapter
A MonoBehaviour may only:
- Carry serialized data from the Editor (`[SerializeField]`)
- Forward lifecycle events into Core
- Call Unity APIs (Instantiate, Transform, Animator, Audio…)

Forbidden: business-rule `if`s, calculations, state machines, caching logic, data transformation.

- Never do work inside `Update()` — call `ITickable.Tick(float dt)`.
- No singletons, no service locator, no static state.
- Composition root (D64):
  - **Until Infrastructure (P0–P9):** one manual `GameplayInstaller : MonoBehaviour` per scene.
  - **From I0:** VContainer `LifetimeScope`s: a root scope in Bootstrap, `MainLifetimeScope` and `GameplayLifetimeScope` as its children. Only a `LifetimeScope` registers or resolves; nothing else touches the container.
- Plain C# classes take dependencies through their constructor. MonoBehaviours have no constructors — they receive dependencies through `Initialize(...)` (or a VContainer `[Inject]` method once scopes exist), never by `Find` or `GetComponent` on other objects.

## Design patterns
- Simplest working solution first. Propose a pattern only for a concrete problem: an axis of change, a testing barrier, the third repetition.
- When proposing a pattern, state in one sentence **what it solves**. If you cannot, do not propose it.
- Banned reflexes: singleton for everything, factory with a single implementation, unnecessary observer layers, premature abstract factory.
