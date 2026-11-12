# Battle02: meet the Rat on a supported tile

[中文](../../zh-CN/synthesis/analyses/battle02-first-contact.md) ·
[Seven-battle coverage audit](pre-voyage-battle-model-coverage.md)

**Inferred design response:** when the southern Huge Rat reaches `(10,13)`, Jaha can
step from `(12,13)` to the adjacent **plains** tile `(11,13)` and attack. Sarah can
then take the vacated `(12,13)` and heal him from one cell away. Moving Jaha around
to the **path** tile `(10,12)` reaches the same target, but removes his defensive
land effect. Two ordinary Rat hits leave the plains Jaha at **1/9 HP**; on the path
they defeat him. This matters when an allied attack misses and recovery falls
after the second hostile hit. Neither placement forces the Rat to target Jaha.

## Scope and reference party

**Standard rigor:** [Issue #1725](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1725)
joins accepted original data into one conditional first-contact choice and one
position comparison. Acceptance covers cells, movement costs, range, integer
damage, healing and action windows in English and Chinese. This is a paper
calculation, without a new collection tool, balance change or natural-play claim.
It extends the [Battle02 calibration](pre-voyage-battle-model-coverage.md#one-paper-calibration-for-battle02).

Use `private-battle02-controlled-party` in
[`battle01-to-battle02-party.json`](../../../../remake/reference/inputs/battle01-to-battle02-party.json),
Normal/difficulty 0, first fresh entry, all four members living, active and free of
statuses. Its values are already refreshed and equipped; do not add weapon ATT
again. This is a named controlled Level 1 loadout, **not** an estimate of what
players naturally carry out of Battle01. Fresh entry restores living allies'
HP/MP, but does not reset their growth or inventory to these rows.

| Combatant | Start `(x,y)` | HP / MP | ATT / DEF / AGI | MOV and movement type | Relevant action |
| --- | --- | --- | --- | --- | --- |
| Bowie | `(13,12)` | 12 / 8 | 9 / 4 / 4 | 6, REGULAR | Held in place in this comparison |
| Sarah | `(12,12)` | 11 / 10 | 9 / 5 / 5 | 5, HEALER | HEAL1: range 0–1, radius 0, 3 MP, power 15 |
| Chester | `(13,13)` | 11 / 0 | 8 / 5 / 7 | 7, CENTAUR | Held in place in this comparison |
| Jaha | `(12,13)` | 9 / 0 | 14 / 7 / 4 | 5, REGULAR | Equipped Short Axe: range 1–1 |
| Huge Rat, slot 131 | `(5,14)` | 10 / 0 | 12 / 8 / 7 | 6, STEALTH | Unarmed range 1–1; ATTACKER1/6, primary order NONE |

**Confirmed inputs:** the [initialization owner](../../../research/chapters/grans-opening/battle02-initialization-and-first-control.md)
supplies placement, Normal Rat refresh and AI routing. The selected Rat is **131**,
not the LEADER-command Rat 128 at `(3,5)`. There are no activation polygons;
region operands `15/15` take the no-region route. Set 6 tries ATTACK1, HEAL1,
SUPPORT, MOVE1, STAY. Its no-item/no-spell Rat can move toward opponents when no
attack is reachable. No source rule promises it will select this exact route or
keep attacking Jaha. Original baseline and provenance remain at that owner:
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, pinned USA ROM.

## Legal approach and support

All coordinates are **battle-local**, zero based, `offset = y*48+x` in the decoded
48×48 array. Battle02's area is 22×18; its Map66 origin `(16,19)` is not added when
reading terrain or measuring range. For a map-coordinate cross-check, local
`(11,13)` corresponds to `(27,32)`. Here terrain IDs 1/2/3 are plains/path/grass.
Costs below are **internal half-MOV units**: budget `2*MOV`. Count destination
cells, excluding the origin. REGULAR and HEALER pay 2/2/3 on these types;
STEALTH pays 2/2/2. Plains select land-effect setting 1 (`230/256`); path selects
0 (`256/256`); grass selects 2 (`205/256`). Displayed 15%/30% are not the damage
multipliers. See [navigation](../../contracts/battle/battlefield-navigation.md).

| Branch / mover | Explicit path after its origin | Cells / cost / budget | Endpoint and action |
| --- | --- | --- | --- |
| Rat approach | `(6,14)→(7,14)→(8,14)→(9,14)→(10,14)→(10,13)` | 6 / 12 / 12 | Plains; cannot attack the unchanged Jaha at `(12,13)` from distance 2 |
| A: Jaha | `(11,13)` | 1 / 2 / 10 | Plains; distance 1 to Rat, attack legal |
| A: Sarah, after Jaha leaves | `(12,13)` | 1 / 2 / 10 | Plains; distance 1 to Jaha, HEAL1 legal |
| B: Jaha, alternate from the same start | `(11,13)→(11,12)→(10,12)` | 3 / 6 / 10 | Path; distance 1 to the same Rat, attack legal |
| B: Sarah, after Jaha passes | `(11,12)` | 1 / 2 / 10 | Path; distance 1 to Jaha, HEAL1 legal |

The Rat's first destination cell `(6,14)` is grass; its other five cells are
plains. Both Jaha paths and both Sarah endpoints are otherwise the plains/path
cells shown above. No listed cell is water/obstructed or occupied by the held
Bowie/Chester. Jaha vacates Sarah's A endpoint and passes through, then leaves,
her B endpoint. These are alternate branches, not consecutive Jaha moves.
Sarah staying at `(12,12)` is distance **2** from either Jaha endpoint: HEAL1
does not reach diagonally. Moving and casting share her action; moving alone
does not heal anyone.

**Inferred conditional contact:** the Rat path fits one turn and both replies
fit the next allied actions. From `(5,14)` the Rat cannot attack any unchanged
ally on its first turn: even the nearest, Jaha, is Manhattan distance 8, needing
at least 7 movement cells before a range-1 attack, beyond MOV 6. It can advance
before Jaha. The table proves a legal approach/reply, **not** that MOVE1's target
ranking and backtracking naturally choose `(10,13)`. Check the actual Rat cell
before using this response; another cell requires a fresh range check.

## Damage and the one discriminating comparison

Apply the [physical pipeline](../../contracts/battle/combat-resolution.md#2-calculate-base-physical-damage-with-integer-arithmetic):
`max(ATT−DEF,1)`, target land-effect truncation, then two downward-spread draws.
For these noncritical values `floor(damage/8)+1 = 1`, so both draws return zero.
Assume ordinary landed single hits, no counter, critical, status or outside
damage, and unchanged stats. **Confirmed rules; Inferred conditional results:**

| Exchange | Integer stages | HP result |
| --- | --- | --- |
| Jaha → Rat on plains, either branch | `14−8=6; floor(6*230/256)=5; 5−0−0=5` | Rat `10→5→0`: two landed Jaha hits |
| Rat → A Jaha on plains | `12−7=5; floor(5*230/256)=4; 4−0−0=4` | Jaha `9→5→1→0`: third landed hit defeats |
| Rat → B Jaha on path | `12−7=5; floor(5*256/256)=5; 5−0−0=5` | Jaha `9→4→0`: second landed hit defeats |

With two successful Jaha attacks, the Rat's opening movement is followed by one
Rat attack and Jaha's finishing blow. A leaves Jaha at 5 HP, B at 4; neither
requires immediate healing against this Rat alone. The useful contrast is
**one missed Jaha attack**: a dodge skips damage, so the Rat can still have 5 HP
after Jaha's second attempt. If it attacks again before recovery, A survives to
receive HEAL1 and B does not. This is a contingency, not a guaranteed miss or
a claim about its probability. No additional special hit is assumed.

## When does Sarah actually get a window?

**Confirmed rule:** [round generation](../../../research/chapters/grans-opening/battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
uses `AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3)−1`, sorted descending; equal
scores retain ally-before-enemy insertion order. All three AGI values here are
below 8, so the first two draws return 0 (but still advance RNG). Rat scores
6–8, Sarah 4–6, Jaha 3–5. Rat always precedes Jaha within a fresh round. Sarah
precedes Rat only at the 6/6 tie; she precedes or follows Jaha depending on scores.
None has a second-turn bit. Do not treat a round boundary as a recovery action.

| Conditional window | What can happen | Consequence |
| --- | --- | --- |
| Opening: Rat 8, Jaha 5, Sarah 4 | Rat moves to `(10,13)`; Jaha moves/attacks; Sarah moves into support | Legal score order for the placement table; Sarah need not cast on full HP |
| Next round: Sarah 6, Rat 6, Jaha 3 | Sarah acts before the Rat's first hit; Rat then hits; Jaha attacks | Healing before damage cannot bank extra HP. If both Jaha attacks land, the Rat now dies |
| Following round: Rat 8, Sarah 4, Jaha 3 | After one missed Jaha attempt, a surviving Rat can hit again before Sarah | A Jaha reaches 1 HP; Sarah can restore 8 HP for 3 MP, `10→7`, then Jaha can finish. B Jaha dies before that cast |

These are explicit **order assumptions** within the score ranges, not a reproduced
seed/queue or a promised three-round script. The HP comparison additionally
requires the Rat to select Jaha, both incoming hits to land, and no other damage
or interruption. If Sarah instead acts between the two hits, she can recover
the first loss (A: 4 HP; B: 5 HP), making either Jaha able to take the next hit.
HEAL1 restores `min(15, missing HP)` for this unpromoted Sarah: it never adds
15 on top of a 9-HP maximum. Ten MP permits three casts with 1 MP left, each
spending a Sarah action. See the [healing contract](../../contracts/battle/spell-resolution.md#confirmed-heal-1-subset).

## Choice, cost and remaining threats

**Inferred:** prefer A when this contact is offered: one plains step has the
same outgoing damage and attack opportunity as the three-step path detour,
while preserving an extra ordinary incoming-hit margin. Move Sarah into
range before the damage window, then spend HEAL1 after damage if Jaha cannot
finish safely. A cast costs 3 MP and a support action; finishing the 5-HP Rat
with Jaha instead removes its future attacks. This case does not assume another
Level 1 ally can finish it: their listed ATT 8/9 is much lower than Jaha's 14.

This is not a safe pocket or a forced tank formation. From `(10,13)` the Rat
can reach Sarah too. For example, after A it can move through `(10,12),(11,12),
(12,12)` for cost 6 and attack her at `(12,13)`; after B it can move to `(11,13)`
for cost 2 and attack her at `(11,12)`. Her DEF 5 means a normal hit is 6 damage
on A's plains or 7 on B's path, so two such hits defeat her 11 HP. Jaha does
not block every approach or compel the target choice.

The other five enemies cannot be ignored. From their declared starts, their
Manhattan distances to A Jaha are: Rat128 `(3,5)` **16**; Ooze129 `(5,6)` **13**;
Ooze130 `(4,10)` **10**; Ooze132 `(7,16)` **7**; Ooze133 `(13,6)` **9**. None can
attack A in one move from those starts (MOV 6/4, range 1). That bound expires
when they move. In particular, Ooze132 could cover `(8,16),(9,16),(10,16),(11,16)`
for cost 8 on one action, then `(11,15),(11,14)` for cost 6 on another and
attack `(11,13)` for `floor((11−7)*230/256)=3` ordinary damage. Its FOLLOW_ENEMY3
order tracks this Rat; the route is a threat possibility, not an asserted AI
choice. Three hostile landed hits (two Rat plus one Ooze) exceed A's 9 HP.
Inspect actual other-enemy positions and available actions before relying on
the isolated two-hit margin.

**Unknown:** natural first-entry roster/growth, incoming seeds, selected AI paths
and targets, other enemies' reached positions, dodge/follow-up outcomes and a
continuous original playthrough producing this contact. Neither the named
controlled party nor source-local route legality establishes those observations.
These gaps do not prevent the conditional choice; they prevent calling it a
guaranteed opening or a complete Battle02 solution.

## Reproduce and review the operands

**Confirmed review surface:** initialization research, reference party, navigation,
physical and healing contracts agree at the bounded quantities used here. Their
fixtures `sf2-battlefield-static-v1`, `sf2-battle-terrain-decode-v1` and
`sf2-heal1-self-recovery-v1` cover shared rules/decoding or controlled healing,
not a composed Battle02 natural encounter. Read their payloads at
[`battlefield-static-v1.json`](../../../../tests/fixtures/h2/battlefield-static-v1.json),
[`battle-terrain-decode-v1.json`](../../../../tests/fixtures/h2/battle-terrain-decode-v1.json)
and [`spell-healing-v1.json`](../../../../tests/fixtures/h3/spell-healing-v1.json).

At the pinned source, inspect `data/battles/spritesets/spriteset02.asm`,
`data/battles/global/landeffectsettingsandmovecosts.asm`,
`data/stats/allies/classes/classdefs.asm`, Short Axe in
`data/stats/items/itemdefs.asm`, HEAL in `data/stats/spells/spelldefs.asm`,
`battlefieldengine.asm:PopulateMoveCostsTable/GetLandEffectSetting` and
`battleloop/turnorderfunctions.asm` under `code/gameflow/battle/` as applicable.
These select movement types, table nibbles, equipped range, healing range and
score arithmetic; the original initialization owner supplies their consumers.

The following bounded read-only check uses the existing decoder and ROM pointer
table. It reads 2 MiB, decodes one 2,304-byte grid and prints only selected cells;
no emulator, extraction publication, new parser or generated tracked file is needed.
Run from the checkout root, with registered inputs selected:

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
@'
import json, os
from pathlib import Path
from sf2tool.compression import decode_stack_compressed
f = json.loads(Path('tests/fixtures/h2/battle-terrain-decode-v1.json').read_text())
r = (Path(os.environ['SF2_SHARED_INPUT_ROOT']) / 'roms/sf2-us.bin').read_bytes()
p = f['table']['terrainTableAddress'] + 2*4
a = int.from_bytes(r[p:p+4], 'big')
b = int.from_bytes(r[p+4:p+8], 'big')
g = decode_stack_compressed(r[a:b], expected_output_bytes=2304).output
paths = {
    'rat': [(6,14),(7,14),(8,14),(9,14),(10,14),(10,13)],
    'jahaA': [(11,13)], 'sarahA': [(12,13)],
    'jahaB': [(11,13),(11,12),(10,12)], 'sarahB': [(11,12)],
    'ooze1': [(8,16),(9,16),(10,16),(11,16)],
    'ooze2': [(11,15),(11,14)],
}
for name, path in paths.items():
    ts = [g[y*48+x] for x,y in path]
    costs = {1:2, 2:2, 3:2 if name == 'rat' else 3}
    print(name, ts, 'cost', sum(costs[t] for t in ts))
print('terrain ROM span', hex(a), hex(b), 'bytes', b-a)
'@ | uv run python -X utf8 -
```

Expected: Rat types `[3,1,1,1,1,1]` cost 12; Jaha A `[1]` cost 2;
Sarah A `[1]` cost 2; Jaha B `[1,2,2]` cost 6; Sarah B `[2]` cost 2;
Ooze paths `[1,1,1,1]` cost 8 and `[3,3]` cost 6. ROM span is
`0x1ad460..0x1ad5ac` (332 compressed bytes). Recheck each listed endpoint's
occupancy and Manhattan action range against the declared branch before using it.
