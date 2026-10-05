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
- `docs/production/` — production docs: plan (`ProductionV1.md`).
- `docs/` sits next to `Assets/`, outside the Unity project content. Never put `.md` files under `Assets/`.
- Code rules live in `.claude/rules/` and load by folder. Before creating the first file in a folder, read the rule files whose `paths` match it.

## Plan
Active production plan (scope, game rules, grid & entity model, modifiers, level data, editor, assemblies, phase list, decision log):
@docs/production/ProductionV1.md

- Required input: the prototype's knowledge in `docs/prototype/FINDINGS.md`. Prototype code is a reference only, never migrated.
- Work the phases in order, one per answer, following the active mode's process.
- When a phase is done, update its **Status** in the plan. Do not edit anything else in the plan unless the user asks.
- When a new decision is settled with the user, add it to the plan's **Decision Log**.
- This import belongs to production mode. If the mode changes, ask the user which plan to import; do not keep loading this one.
