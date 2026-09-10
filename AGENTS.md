# AGENTS.md

## Project

Unity Match-3 technical challenge.

- Unity version: 6000.3.16f1
- Language: C#
- Original repository: https://github.com/gazeus-tests/desafio-match-3

Before changing gameplay code, read:

- `Docs/Baseline.md`

The project is being evolved incrementally from the provided implementation.
Do not treat it as a greenfield rewrite.

## Working principles

### Keep changes narrow

Implement only what the current task requires.

Do not:

- refactor unrelated systems;
- rename unrelated classes or files;
- reorganize folders without a concrete need;
- add abstractions only for architectural cleanliness;
- add packages without explicit justification;
- implement future roadmap features early.

If a larger change seems necessary, explain why before expanding the scope.

### Preserve existing behavior

Unless the task explicitly changes a behavior, assume the existing behavior
must remain intact.

Prefer small, reviewable changes over broad rewrites.

### Understand before changing

Before modifying an existing system:

1. inspect the relevant code path;
2. identify the current behavior;
3. identify the specific problem the task is addressing;
4. make the smallest change that solves it.

Do not replace working legacy code simply because another design would be
cleaner.

### Do not optimize from assumptions

Performance changes should be motivated by either:

- profiler evidence;
- a clearly identified unnecessary operation;
- a requirement that makes the current approach unsuitable.

Do not introduce complexity for hypothetical performance gains.

The initial profiling baseline is documented in `Docs/Baseline.md`.

## Unity and C#

Follow normal Unity and C# conventions already present in the project.

Prefer:

- clear ownership of responsibilities;
- explicit names;
- simple data flow;
- deterministic gameplay logic where practical;
- plain C# for gameplay rules when Unity-specific behavior is unnecessary.

Avoid:

- unnecessary singletons;
- excessive interfaces;
- pattern-driven architecture without a concrete use case;
- hidden side effects;
- unnecessary allocations in frequently executed gameplay paths.

Comments should explain non-obvious decisions or constraints, not restate the
code.

## Gameplay architecture

Keep gameplay rules separate from presentation where practical.

The View should represent gameplay results rather than decide gameplay rules.

Input methods such as click or swipe should eventually produce the same logical
move intent instead of duplicating gameplay behavior.

Do not introduce new gameplay architecture ahead of the current task.

## Testing

The automated test infrastructure will be introduced incrementally.

Until the test suite is available:

- the project must compile without new errors;
- do not claim automated validation that was not actually run;
- report manual validation performed;
- report anything that could not be validated.

Once automated tests are introduced:

- all relevant existing tests must pass before a task is considered complete;
- bug fixes should include regression coverage when practical;
- gameplay-rule changes should include deterministic tests;
- do not modify tests simply to make a failing implementation pass unless the
  expected behavior itself has intentionally changed.

## Validation and reporting

At the end of each task, report:

1. what changed;
2. which files changed;
3. how it was validated;
4. any assumptions made;
5. any known limitation or follow-up that remains.

Do not declare a task complete if the project does not compile or required
validation failed.

## Scope discipline

This project has a staged roadmap.

Do not implement later phases while working on an earlier one unless explicitly
requested.

Current roadmap:

1. Baseline
2. Safety Net / Tests
3. Domain Cleanup
4. Match Semantics
5. Resolution Engine
6. Special Tiles
7. Player Experience / Game Feel
8. Board Robustness
9. Level Definition
10. Final Proof / PR

The current task always takes precedence over the broader roadmap.