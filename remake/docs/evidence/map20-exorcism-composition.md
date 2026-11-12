# Map20 exorcism and narration 2212

**Confirmed:** the ordinary castle scene cuts from the king and Astral to narration over black.
Astral remains allocated outside the new view. The reported grey roof came from drawing the
castle's original layout after the cut. The feedback comment describes the player's image and
has no attached image. [Human observation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456#issuecomment-6817736688)
and [scope](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1708).

## Original

Use the [registered US retail input and BizHawk](../../../docs/operations/local-private-inputs.md#bizhawk-direct-launch-state)
and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. The ordinary observation
uses a copy of the retained Map57 post-Battle01 controllable state (F501, Bowie at 5,12,
SHA256 `C3200B3D288210C48D0CB7C8981510E08D796469B35028BE30B97CC4DDEFE5D2`).
Take the tower and passage exits, then Map21 (3,16), Map20 (6,37), Map19 (23,3), and Map20
(25,35) before walking to (27,52). The live `Map20_1F5_ZoneEvent0` callback at $5376E confirms
ordinary dispatch. This caller restores allies 1 and 2 and calls `cs_53B66`, which later falls
through to `cs_53C42`. See the [caller contract](../../../docs/research/chapters/grans-opening/battle01-to-battle02-content-route.md).

With a new local emulator configuration and isolated desktop, observe `DisplayText` ($6260, D0.W),
the W2 input loop ($6472) and W1 input loop ($659C). At the three settled waits, read the four
plane coordinates at $FFA810, view target at $FFA82C, dialogue window count at $FFAF6C, layout
words at $FF0000, and block zero at $FF2000. Resolve entity IDs via $FFB140 into the 32-byte
records at $FFA902. One private 256×224 frame per ordinary wait corroborated the visible result.

| Ordinary wait | View and layers | Actors and windows |
| --- | --- | --- |
| 2211 W1, king calls to Astral | Both planes at cell (19,48), target FF. The held frame shows the interior. | Bowie (27,51) facing down, king (22,53) facing right, Astral (23,53) facing left with sprite 209. The king and Astral are in frame. Text and portrait are open. |
| 2212 W2, first narration page | Both planes at (0,0). Sampled layout words at (1,1) and (21,53) are zero. Block zero begins with $C0F8. | Same positions, sprites and facings. Text only. Outside the dialogue rectangle every pixel is black. |
| 2212 W1, after one confirm | Both planes remain at (0,0). The nonblack pixels on each narration page lie within x=16…247, y=153…214. | Same retained actors outside the frame. The second page waits for a separate confirm, followed by the Map19 handoff. |

The other sampled F501 actors stayed in the adjoining room throughout: Elis (21,40), the minister (25,39), and guard 132 (17,37). None entered the narration frame.

**Confirmed:** the entity-130 event calls the suffix directly. To inspect this caller, repeat the
ordinary approach through (25,35), then on the disposable emulator move only the player's live X/Y
and destination words to (23,54), face up and interact. The first observed text is 2200. At 2211
the king and Astral both occupy (19,53), and Bowie is at (23,53). The two 2212 waits again have
zero plane origins, zero sampled layout words, blank block zero and one dialogue window. This
controlled interaction accounts for the missing prefix. **Unknown:** natural access to that
interaction before zone dispatch. The bounded walk avoiding (27,52) found no route from (25,50).

`data/maps/entries/map20/mapsetups/scripts_1.asm` orders fade-out, reset, fade-in and reload at
(0,0) before the two-page text, then sets F609 and warps to Map19 (29,8). `csc36_resetMap`
calls `ResetCurrentMap`, which clears 2,048 longs and writes nine priority blank tile words
for block zero. Its negative `LoadMap` and the later `csc46_reloadMap` skip block/layout decoding.
The latter detaches the camera. A positive map load can populate blocks and layout again.
See the [working-layout contract](../../../docs/research/world/map-content.md).

## Remake reproduction and acceptance

Build Debug under the [locked environment](../development-and-verification.md#locked-net-workflow).
Use the prepared private pre-voyage world with gameplay revision 4 and its matched battle/entry
inputs. Copy a compatible post-victory `battle01-return` test slot to a fresh external save
namespace. Use ordinary title Continue in a separate headless Godot instance. Follow the same
warps and enter the zone by movement. The observation used a slot-2 copy at Map57 (2,11) and a
1920×1080 logical viewport. The route and ordinary-input pathfinding are in
[the field observer](../../game/probes/engine_battle01_to_battle02_observation.gd).

At 2211 and at both 2212 input waits, read `ExplorationSessionView.ReadObservationJson()`
after a draw, plus `ReadLayoutRegion(0,0,3,2)`. Confirm once per page. Inspect the first Map19
commission dialogue as the next positive-load control. Godot evidence uses actual draw submissions,
window and actor projections. The direct behavior check from the repository root is:

```powershell
. ./local/private-inputs.ps1
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --configuration Release --no-restore --filter FullyQualifiedName~CurrentMapReloadUsesTheWorkingLayoutIncludingAScriptedReset
```

| Stop | Baseline 34244d1a | Corrected result |
| --- | --- | --- |
| 2211 | Interior, 1,887 low and 153 high background tile submissions. | Same counts, two windows, Bowie, king and Astral projected in view. |
| 2212 W2 and W1 | Correct detached camera and offscreen actors, but original castle words in the working layout. 1,485 background tile submissions on each page. | Six sampled words zero, zero terrain submissions, one window, all three actors outside the view. The blank block exposes the black backdrop. |
| Map19 dialogue 721 | 1,820 low and 220 high background submissions. | Same counts and actors in view, with Bowie and the king visible. |

**Confirmed:** reset changes the working layout and block zero. Current-map reload preserves these
and other live edits, including copied blocks. Freshly generated programs distinguish the commands.
For existing pre-voyage exports the content reader recovers the first reset in the two pinned source
bodies that contained one, preserving later reloads. The camera and actor rules are independent
of the dialogue ID. The tests exercise reset and ordinary reload with a mutated layout.

The baseline and corrected native observers exited with no scene or presentation failure.
Godot reported a pre-existing ObjectDB instance warning at shutdown. These observations cover the
specified waits and handoff. Player visual acceptance remains with #1456.
