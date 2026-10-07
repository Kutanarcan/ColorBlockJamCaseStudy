# ColorBlockJamCaseStudy — Unity / C#

## Communication
- Short, precise, bulleted. No intro sentence, no closing summary.
- Do not dump depth unprompted. If there is more, leave one line: "I can expand on X if you want."
- Never guess — ask.
- Match the user's language; keep technical terms in English.

## Mode
MODE: PRODUCTION
@.claude/modes/production.md

- Only the user changes the two lines above. Never change them, never assume they changed, never infer the mode from the code.
- If the mode line and the import disagree, or either is missing, stop and ask.
- Available modes: `prototype`, `production`.

## Layout
- `Assets/Prototype/` (`Game.Prototype.asmdef`) — disposable, excluded from player builds.
- `Assets/Scripts/` — production.
- The two assemblies never reference each other.
- `docs/prototype/` — prototype docs: plan (`PrototypeV1.md`) and `FINDINGS.md`.
- `docs/production/` — production docs: active plan (`ProductionV2.md`), finished plan (`ProductionV1.md`), level format rules (`LevelFormat.md`), mechanic recipe (`Extending.md`), mechanics reference (`ColorBlockJamMechanics.md`), performance record (`Performance.md`), Addressables guide (`Addressables.md`).
- `Addressables.md` follows the code: at the end of every phase that changes loading, groups, keys, labels, profiles or the download flow, update its sections and its **Status** table.
- `docs/Game Developer Case 2026.pdf` — the case brief: required scope, acceptance criteria, evaluation order, deliverables.
- `docs/` sits next to `Assets/`, outside the Unity project content. Never put `.md` files under `Assets/`.
- Code rules live in `.claude/rules/` and load by folder. Before creating the first file in a folder, read the rule files whose `paths` match it.

## Plan
Active production plan (vertical slice: presentation, infrastructure, gameplay UI, main menu & meta, delivery; phase list, open questions, decision log D61+):
@docs/production/ProductionV2.md

- `ProductionV1.md` is finished (logic, level data, editor, additivity). Its rules and decisions D1–D60 still hold; read it when a phase touches Core, LevelIO or the Level Editor. Do not edit it except to fix a fact.
- Required input: the prototype's knowledge in `docs/prototype/FINDINGS.md`. Prototype code is a reference only, never migrated.
- Every phase is checked against the brief's acceptance criteria (`ProductionV2.md` §1).
- Work the phases in order, one per answer, following the active mode's process.
- When a phase is done, update its **Status** in the plan. Do not edit anything else in the plan unless the user asks.
- When a new decision is settled with the user, add it to the plan's **Decision Log**.
- This import belongs to production mode. If the mode changes, ask the user which plan to import; do not keep loading this one.
