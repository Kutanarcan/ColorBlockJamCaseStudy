# ColorBlockJamCaseStudy — Unity / C#

## Communication
- Short, precise, bulleted. No intro sentence, no closing summary.
- Do not dump depth unprompted. If there is more, leave one line: "I can expand on X if you want."
- Never guess — ask.
- Match the user's language; keep technical terms in English.

## Mode
MODE: PROTOTYPE
@.claude/modes/prototype.md

- Only the user changes the two lines above. Never change them, never assume they changed, never infer the mode from the code.
- If the mode line and the import disagree, or either is missing, stop and ask.
- Available modes: `prototype`, `production`.

## Layout
- `Assets/Prototype/` (`Game.Prototype.asmdef`) — disposable, excluded from player builds.
- `Assets/Scripts/` — production.
- The two assemblies never reference each other.
- `docs/prototype/` — prototype docs: plan (`PrototypeV1.md`) and `FINDINGS.md`.
- `docs/production/` — production docs.
- `docs/` sits next to `Assets/`, outside the Unity project content. Never put `.md` files under `Assets/`.
- Code rules live in `.claude/rules/` and load by folder. Before creating the first file in a folder, read the rule files whose `paths` match it.

## Plan
Active prototype plan (game summary, asset analysis, phase list, open questions):
@docs/prototype/PrototypeV1.md

- Work the phases in order, one per answer, following the active mode's process.
- When a phase is done, update its **Status** in the plan. Do not edit anything else in the plan unless the user asks.
- When an open question is answered, move the answer into `docs/prototype/FINDINGS.md` and mark the question answered in the plan.
- This import belongs to prototype mode. If the mode changes, ask the user which plan to import; do not keep loading this one.
