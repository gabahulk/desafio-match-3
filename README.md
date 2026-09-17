# Match-3 Challenge

This is my solution for the Gazeus Match-3 developer challenge.

The starting project already had the basic board and Match-3 loop working. My goal was to build on top of that without turning a small challenge into a giant framework.

I focused first on making the game rules predictable and testable, then implemented the special pieces and scoring, and only after that moved into presentation and game feel.

The result is a Match-3 with cascades, Candy Crush-style special pieces, special combinations, chain reactions, scoring, responsive UI and a small polish pass.

Built with **Unity 6000.3.16f1**.

## Quick look

<!--
Replace this section with the final 15-30 second gameplay video.

Suggested demo:
normal match
-> special creation
-> special activation
-> chain/cascade
-> score feedback
-->

**Gameplay video coming here**

---

## What I added

The challenge asked for scoring and additional mechanics for matches larger than three pieces.

I implemented:

- score based on pieces actually destroyed;
- cascade multiplier;
- horizontal and vertical Striped pieces;
- Wrapped pieces for intersecting matches;
- Color Bombs for 5+ matches;
- direct combinations between special pieces;
- indirect special chain reactions;
- cascades, gravity and refill;
- visual feedback for tiles and scoring;
- responsive layouts for different board and screen shapes;
- Edit Mode and Play Mode test coverage.

### Match rules

| Match | Result |
| --- | --- |
| 3 in a row | normal clear |
| 4 in a row | Striped |
| 5+ in a row | Color Bomb |
| L / T / Cross | Wrapped |

Striped pieces clear a row or column.

Wrapped pieces explode around themselves and have a second activation after the board settles.

Color Bombs clear a color and also support combinations with the other specials.

Specials can trigger other specials, so large chain reactions are possible.

## Scoring

I kept scoring intentionally simple:

```text
destroyed tiles x 10 x cascade multiplier
```

The initial resolution uses `x1`.

If gravity/refill creates another match, the next cascade uses `x2`, then `x3`, and so on.

Special activations and chain reactions that belong to the same resolution stay on the same multiplier. The multiplier only increases when the board settles and produces a new match.

This made the score directly reflect what actually happened on the board instead of relying on arbitrary bonuses.

## A bit about the architecture

The biggest change wasn't one particular special piece. It was making the resolution flow explicit enough that adding those pieces didn't turn `GameService` into a collection of special cases.

The main flow ended up roughly like this:

```text
TrySwap
  -> MatchFinder
  -> MatchPattern / SpecialMatch
  -> SpecialCreator
  -> SpecialEffectResolver
  -> destruction
  -> gravity
  -> refill
  -> cascade
```

There are a few separations here that I think are important.

### `MatchFinder` describes what happened

A match is represented as a `MatchPattern`.

It knows things such as:

- which cells belong to the match;
- its size;
- its shape;
- whether the pattern is eligible to create a special.

It does not create the special itself.

### `SpecialCreator` decides what to create

Special creation is a separate step.

This is also where the spawn position is chosen, including the swapped tile when appropriate.

That kept match detection independent from the rules for creating pieces.

### `SpecialEffectResolver` owns special behavior

Activating a Striped, Wrapped or Color Bomb goes through the special-effect resolution layer.

Chain reactions use the same path instead of having a second set of rules for a special triggered by another special.

### The board resolves before the View plays it

Gameplay produces a sequence of board changes through `BoardSequence`.

The presentation layer then reproduces those steps:

```text
special activation
-> destruction
-> movement
-> refill
```

This made it possible to add tweens and effects without putting animation timing into the game rules.

## Tests

A significant part of the work on the challenge was creating a safety net before expanding the original implementation.

There are both **Edit Mode** and **Play Mode** tests.

Edit Mode covers things such as:

- board generation;
- movement validation;
- match detection;
- special creation;
- special effects;
- combinations;
- chain reactions;
- scoring and cascade multipliers;
- edge cases and board bounds.

Play Mode tests exercise the gameplay flow through the scene, controller and UI, using deterministic boards where necessary.

The repository also includes helper scripts for running both suites from the command line:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/run-tests.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File Tools/run-playmode-tests.ps1
```

## What I deliberately didn't build

The challenge also suggests bonus features such as level progression, lives, missions, hints and alternate board mechanics.

I chose not to add those before the core resolution system was solid.

Given more time, I would rather continue polishing feedback, audio and game feel than add progression systems to what is ultimately a technical Match-3 exercise.

There are also places where I would continue cleaning up the code if this became a production project. `GameService`, in particular, still carries more orchestration responsibility than I would want long-term.

For this scope, I preferred leaving a few visible seams over introducing abstractions before they were actually needed.

## Original project analysis

I kept notes from the initial characterization/refactoring pass in [`Docs/baseline.md`](Docs/baseline.md).

It documents the state of the original project and some of the reasoning behind the incremental changes.
