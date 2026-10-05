# Color Block Jam — Mechanics Reference

## Purpose
- Knowledge about the **original game's** obstacles and special blocks.
implement anything from this file unless the user asks for it.
- Use it to:
  - check that new structures don't block these mechanics later;
  - answer the user's questions about the original game.

## Source
- Rollic's official Color Block Jam Help Center, "Obstacles in Color Block Jam": https://rollic.helpshift.com/hc/en/24-color-block-jam/faq/1234-obstacles-in-color-block-jam/
- Retrieved October 2026; the page had last been updated about 7 months earlier. Newer mechanics may exist.
- Descriptions below are paraphrased summaries, not the official text.

## User's names → official names

| User's name | Official name(s) |
|---|---|
| Key-Lock | Chained Blocks (key blocks), Locked Door |
| Rope-Scissors | Ropes |
| Ice | Ice, Iced Door |

## Trigger taxonomy
Most mechanics react to **one event: a block exits through a door**. That event drives counters, toggles, jumps and color changes.

| Trigger | Meaning | Mechanics |
|---|---|---|
| Exit count | Counter drops on every exit (any color) | Ice, Iced Door, Crate, Moveable Crate, Ivy, Turn Based Arrow, Combined Locked, Size-Changing Door |
| Exit toggle | Flips open/closed or between two colors on every exit | Door, Barrier, Curtain, Color-Switcher |
| Exit move | Moves or jumps to a new spot on every exit | Jumping Single Door, Colorful Jumping Door, Moving Door Lock |
| Exit by specific block | Reacts only when a particular block or block type exits | Chained (keys), Locked Door (keys), Ropes (scissors, same color), Hidden, Layer, Combined, Color Swapping Block, Time Capsule |
| Exit color | Reacts to the color of the exiting block | Color-Switching Block, Colorful Door (color sequence), Colorful Crate (color exhausted) |
| Position | Reacts to where blocks are on the board | Button Door, Laser Door, Magnet, Elevator, Colorful Moving Path, Flower Crate |
| Move count | Counts player moves, not exits | Dynamite |
| Time | Level timer | Time Capsule (adds time), Bomb |
| Static | Fixed rule, no trigger | Arrow, Star, Colorful Path, Moving Blocks |

## A. Block properties

| Mechanic | Behavior | Trigger | State it holds |
|---|---|---|---|
| Arrow | Moves only in the direction its arrow shows (V1: axis lock) | Static | direction/axis |
| Turn Based Arrow | Starts locked to one direction; after a shown number of exits it moves in both directions | Exit count | counter, direction |
| Layer | Has several layers; one comes off at a time, outermost first | Exit by specific block | layer stack |
| Star | Exits only through star doors; normal blocks may also use star doors | Static | flag |
| Combined | Two blocks stuck together; taking one part out separates them | Exit by specific block | partner link |
| Combined Locked | Two linked blocks can't pass a door even at the right color until other blocks have exited | Exit count | link, counter |
| Curtain | Can pass a door only while its curtain is open; the curtain flips on every exit. Can still move while closed | Exit toggle | open flag |
| Color-Switching Block | Has an outer and an inner color. If a block of the outer color exits, the outer layer clears; if another color exits, inner and outer swap | Exit color | two colors |
| Color Swapping Block | Moves like a normal block; its own exit swaps colors of blocks on the board | Exit by specific block | — |
| Magnet | Same-color blocks link when lined up correctly, opening a path | Position | color |
| Dynamite | Shows a move limit; if not cleared within it, it explodes and the level is lost | Move count | counter |
| Bomb | Must be removed before it explodes | Time / counter | counter |
| Time Capsule | Carries a time bonus (e.g. +10, +15) added when it exits through its color door | Exit by specific block | time value |
| Hidden | Invisible until a specific visible block exits, then must be cleared too | Exit by specific block | trigger block |
| Moving Blocks | Move freely unless locked, roped or screwed, but can never exit | Static (obstacle) | — |

## B. Locks and containers on blocks

| Mechanic | Behavior | Trigger | State it holds |
|---|---|---|---|
| Ice | Frozen block can't move; thaws after a set number of other blocks exit | Exit count | counter |
| Chained (Key) | Can't move until a required number of key blocks exit | Exit by specific block | key count |
| Ropes | Stuck until scissors of the same color are removed | Exit by specific block (color) | rope colors |
| Crate | Hides something (possibly a bomb); opens after required exits | Exit count | counter, content |
| Moveable Crate | Number drops on every exit; at zero it vanishes and reveals a block | Exit count | counter, content |
| Flower Crate | Moves like a block; opens when its linked glowing leaves are cleared, revealing blocks/bombs/etc. | Position (linked targets) | linked leaves, content |
| Colorful Crate | Opens once every block of its color has left the board | Exit color | color |
| Elevator | Stack of hidden blocks, one visible on top; moving the top one away brings up the next | Position | block queue |

## C. Door properties

| Mechanic | Behavior | Trigger | State it holds |
|---|---|---|---|
| Door | Opens and closes on every exit | Exit toggle | open flag |
| Iced Door | Locked by ice; opens after a required number of exits | Exit count | counter |
| Locked Door | Chained gate tied to a color; opens when key blocks pass through their matching doors | Exit by specific block | key count |
| Moving Door Lock | The lock moves to the next door in a set direction | Exit move | position, direction |
| Colorful Door | Accepts one color at a time; after it passes, shows the next required color | Exit color | color sequence |
| Color-Switcher | Alternates between two colors whenever any block exits | Exit toggle | two colors |
| Size-Changing Door | Width changes as blocks exit (wider or narrower) | Exit count | width schedule |
| Jumping Single Door | A blocker covering one spot of the door jumps to a new spot on every exit | Exit move | blocked index |
| Colorful Jumping Door | Jumps to a new spot after every exit and recolors the door it lands on | Exit move | position, color |
| Button Door | Opens when a block of the button's color is placed on its button | Position | color |
| Laser Door | Open only while its beam reaches the receiver on the other side; blocked beam = closed | Position | beam path |

## D. Board properties

| Mechanic | Behavior | Trigger | State it holds |
|---|---|---|---|
| Colorful Path | Only blocks of its color may use these cells | Static | color |
| Colorful Moving Path | Moves when a block is slid onto it, reshaping the board | Position | path layout |
| Barrier | Opens and closes after every exit | Exit toggle | open flag |
| Ivy | Covers board cells; shrinks by one tile on every exit | Exit count | cell list |

## Notes
- **"Screwed":** the Moving Blocks description mentions a screw lock, but it has no entry of its own in the official list.
- **Ice discrepancy:** some third-party guides say ice melts when blocks pass over it. The official description is an exit counter; this file follows the official one.
- **Timer and failure states** (Bomb, Dynamite, Time Capsule) imply a lose condition. V1 has no lose condition.
- **Design hint for later:**
  - Phase 10's "block exited" event is the natural hook for most mechanics here.
  - Several mechanics need per-block state beyond V1's `Block { id, color, axisLock, cells }`: counters, flags, links, color pairs.
  - Several need per-door state beyond `Door { side, index, length, color }`: open flag, color sequence, width, blocked index.
