# Baseline

Before changing the gameplay code, I wanted to understand what was already
working, how the current implementation was structured, and where changes were
actually justified.

This document is a snapshot of the original project. Its purpose is not to
criticize the starting implementation or define the final architecture. It is
simply the baseline I will use to make and evaluate the changes that follow.

## Environment

- Unity 6000.3.16f1
- Profiling performed in the Unity Editor
- CPU Profiler
- Main Thread
- Deep Profile disabled
- GC allocation call stacks enabled

The Editor profiling below is diagnostic only. It is useful for comparing the
project with itself as it evolves, but it should not be treated as
target-device performance data.

---

## Original architecture

The project is small and has a straightforward separation between input,
gameplay logic and presentation.

```text
TileSpotView
    ↓
BoardView
    ↓
GameController
    ↓
GameService
    ↓
BoardSequence[]
    ↓
GameController
    ↓
BoardView
```
TileSpotView

Represents a cell in the board and emits its coordinates when clicked.

BoardView

Owns the visual representation of the board.

It is responsible for creating tile GameObjects and playing the animations for
swaps, removed tiles, falling tiles and new tiles.

GameController

Connects player input to the game logic and coordinates the visual playback.

It keeps track of the selected tile, requests move validation/resolution from
the GameService, then asks the BoardView to reproduce the result.

GameService

Owns the logical board state and most of the current gameplay rules.

Its responsibilities currently include:

board generation;
move validation;
tile swapping;
match detection;
tile removal;
gravity;
refill;
cascade resolution.
BoardSequence

Represents one step of a resolution from the presentation point of view.

It contains the positions that should disappear, which existing tiles should
move and which new tiles should be created.

The GameService resolves the logical result first and returns one or more
sequences for the GameController to play afterwards.

Current gameplay flow

Player input currently uses two clicks rather than swipe.

First tile click
↓
Store selection

Second adjacent tile click
↓
Animate swap

Tween completes
↓
Validate movement

Invalid
    → animate the tiles back

Valid
    → resolve the move
    → remove matches
    → apply gravity
    → refill
    → resolve cascades
    → play each BoardSequence

An important detail is that move validation happens after the first visual
swap has completed.

The model therefore decides whether the move is valid after the View has
already started representing it.

The invalid swap still looks correct to the player because the pieces move
back, but this coupling is worth revisiting when the move flow is refactored.

What already works

The starting project already provides a functional Match-3 foundation:

horizontal and vertical Match-3 detection;
invalid swap rollback;
gravity;
refill;
cascades;
initial board generation without immediate matches.

The default scene currently uses a 10x10 board and four tile types.

Cascades are resolved completely by the gameplay logic before their
BoardSequences are played visually.

Initial findings

These are observations from reading and playing the original implementation.
They are not all bugs, and not all of them necessarily need to be changed.

Match information is reduced too early

The current match finder ultimately describes matched cells as a boolean mask.

That is enough to remove tiles, but information such as:

match size;
orientation;
shape;
color;
intersection;
originating move;

is no longer represented explicitly.

This becomes important for the requested Match-4/5 and special-piece rules, so
match semantics will need to become richer before those features are added.

Move validation duplicates work

A move is first evaluated through IsValidMovement, which copies the board,
performs the swap and scans for matches.

A valid move is then processed again by SwapTile, which performs another
copy/swap/match pass before resolving it.

This is currently inexpensive on a 10x10 board, but it duplicates work and
makes validation and execution two separate representations of the same move.

This is a candidate for simplification after existing behavior is covered by
tests.

Rectangular boards expose a width/height assumption

One loop inside the current match-detection code uses the board row count when
building its horizontal structure.

The default 10x10 board hides this because width and height are equal.

A rectangular-board regression test will be added before correcting it.

Initial generation does not guarantee a playable move

The generator avoids immediate matches, but it does not currently verify that
the generated board contains at least one valid move.

This becomes relevant for both initial board generation and future dead-board
handling.

Same-cell input can reach the swap flow

Adjacency currently accepts the same position as well as an adjacent one,
because a distance of zero is not rejected.

This does not appear to cause a major gameplay failure, but it is another
small behavior worth covering when the move flow is tested.

Performance baseline

The challenge explicitly mentions performance as an evaluation criterion, so I
captured a small baseline before modifying production code.

The goal at this point is not to optimize a very small board or claim
target-device numbers. The goal is simply to establish evidence that can be
compared against later changes.

Invalid move

A representative invalid-swap resolution frame showed approximately:

6 KB managed allocation
allocation concentrated in the frame where the DOTween completion callback
executes

The current profiler capture does not isolate how much of that allocation
comes from move validation itself versus surrounding tween/callback work, so I
am not attributing the full amount to GameService.

Valid move with cascade

A valid move that produced a cascade generated repeated allocation spikes
during its resolution/refill steps.

Two representative frames showed approximately:

41.4 KB managed allocation
41.8 KB managed allocation

In one captured frame, roughly 30 KB was associated with Instantiate
calls.

The repeated spikes make sense with the current flow: each cascade produces
another resolution step and another visual refill.

This is worth revisiting later, especially if the presentation layer continues
to create and destroy tile objects during normal gameplay.

CPU

No obvious CPU bottleneck appeared in the initial Editor capture.

Representative total frames were comfortably below the frame budget, and a
large portion of the measured frame time came from the Unity Editor itself.

For that reason, I am not treating the Editor CPU numbers as a meaningful
optimization target at this stage.

Baseline conclusions

The current project already has the core loop needed to build on.

The main risks I want to address before adding features are not raw
performance problems. They are mostly about making the gameplay rules easier
to represent, test and extend without breaking existing behavior.

The next step is therefore not to start implementing special pieces.

It is to first create a small automated safety net around the existing Match-3
behavior.

That will give us a controlled starting point for the structural changes that
follow.