# New Granseal return, Astral, Dynamite and Janet

The next [Battle24 chapter](battle24-north-parmecia.md) consumes the ordinary exit and deployed Janet saves.

**Confirmed in the remake:** unchanged bilingual [mine-return saves](dwarven-mine-cannon.md)
continue through the town and castle audience, Astral following, Dynamite, Janet recruitment and an
ordinary Map73 save. This is the bounded [Issue1896](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1896)
outcome under the [High-rigor campaign agreement](remaining-campaign-risk-baseline.md).
Battle24, North Cliff/Rohde and later towns are outside this slice. Original natural reach/timing and
human visual/audio quality remain **Unknown**; remake input observations are not original evidence.

## Source and implementation boundaries

The original owners are [audience/Astral](../../../docs/research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md),
[inventor/Dynamite](../../../docs/research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md)
and [Janet/departure](../../../docs/research/chapters/northern-travel-mitula/new-granseal-janet-recruitment-and-departure.md).
They pin SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA ROM.
The relevant source is `disasm/data/maps/entries/map06/mapsetups/` and `map07/mapsetups/`,
`code/common/stats/iteminventory.asm`, and `code/gameflow/battle/battlefunctions/battlefunctions_0.asm`.

- Map7's zone at X9–13/Y8 gives F802 priority and runs `cs_55BEE` while805 is clear.
  It needs neither Cannon nor F761. Completion sets70/805; the local Astral remains resident139 and
  follows. Fresh805 selects its actual eleven-resident population, without139, plus follower B31.
- Map6's inventor141 at (24,7) uses the shared `s2_entityevents_701.asm:Map6_EntityEvent13` even in
  setup702. F802 has priority;804 precedes805. Eligible805/!804 uses the actual joined-force Cannon
  lookup, including reserve/dead members and the source's count-limited reverse inventory scan.
  An item beyond a gap can therefore be missed. No invented F70, F761 or adviser-presence guard exists.
- `ReceiveMandatoryItem` option0 picks the first joined member with an empty slot, in member order.
  `TryAddPartyItem` branches on all-full; success retains the actual recipient/item for213's NAME/ITEM
  operands, plays MUSIC_ITEM, waits for music/previous-music/input, then the caller's second input and
  close. Only afterward does the source caller set804. Cannon114 remains; Dynamite116 is added once
  per successful caller, with the caller's804 repeat guard. Full refusal1729 leaves804 clear.
- The shared music-return lowering also admits the audience's `j_FadeOut_WaitForP1Input`. Its name
  does not mean palette fade. MUSIC_ITEM reuses the accepted finite PCM, with modernEndStep168 from
  the retained original-driver dispatch243/end411 observation. This is a bounded modern completion
  binding, not a claim of natural audio phase or original scene timing.
- Janet's Map6 zones (19,29)/(20,29) test805/!806. No Dynamite, Elric, HP or capacity predicate is
  added. `cs_55288` allocates ally18 at (16,22); its sprite operand18 resolves through actual ally
  class/death identity, yielding base field sprite45, rather than displaying sprite18. Joining18
  at12 active members leaves her reserve. Local following is not installed into the fresh town cast.

The implementation reuses story operations, joined-party queries, source programs and field-scene
restore mechanics. `Sf2NewGransealRestore` owns the finite map/setup context and the admitted Janet
allocation; generic restoration validates actors, aliases, slots and follower graphs. Historical
opening roster revision7 adds18 while retaining old records. Scene-bearing town saves use the existing
format3 protection. No new save envelope, general script interpreter or production reference dependency
is introduced. Private producers/readers and the private selection own added content; no research
fixtures or original evidence are changed.

## Ordinary route and observed branches

The unmodified mine endpoints start at Map68 (57,25), Down. Ordinary directional inputs take the
Caravan north through the dry river, crossing Map68 (55,0) to Map73 (55,62), landing at (55,60),
then entering town at the (29,52) warp. Town entry is Map6 (20,30); castle entry is (11,0),
audience approach (11,8), and castle exit Y23 returns Map6 (11,1). The inventor is approached from
(24,8), facing Up. Janet's zone is reached at (20,29); the south exit produces Map73 (29,53), Down.
These waypoints describe the observed route, not new admission predicates.

Both language routes finish1249–1289,1119/1120/213 and Janet's scene/join446, with no display fallback
or reached production failure. English and Chinese retain their different inherited Creed choices
and growth results. The actual gift recipient is Chester; Bowie keeps Cannon. Janet is ACHR24 with
equipped Steel Arrow47, 36 HP in English and35 in Chinese. Both formations stay at12 active; Janet
is reserve. Ordinary Caravan relief deploys her in place of Chester; Chinese Party details display
her name, HP, class and pack through the existing consumers.

The ordinary full-pack witness buys33 Medical Herbs at the F802 item counter, filling all13 joined
packs. Inventor1729 then leaves804 clear and adds no Dynamite. Selling one purchased herb through the
same counter permits retry213 and804. This uses disposable copies and actual menu inputs, without
manufactured saves. Separate ordinary witnesses cover pre-audience1114/1115, repeat gift1122 and
fresh-castle king1237/minister1238–1239. Controlled actual-engine variants distinguish802 priority,
804-before805, actual Cannon absence/presence, count-limited gaps, equipment bits, dead reserve capacity
and full12 recruitment. They do not certify natural original branch reach.

## Current, fresh and cold state

Loaded audience retains Astral139; fresh Map7/F805 reconstructs B31 and removes139. Loaded Janet
retains the source-allocated18/following graph; ordinary fresh Map6 does not recreate that local
actor. Both states can be saved and continued. Cold observations at audience, gift and Janet preserve
all player-owned payload fields, aliases, follower identity/offsets and layout. Elapsed ordinary town
work changes ambient resident position/facing and main RNG. The finite restore restarts resident
ambient initialization: castle minister128/king129 idle speed returns from the saved scene’s40/32
to the source idle16. This is an explicit restore limit, not exact preservation of every NPC work
register. Thinking RNG, party statistics,
packs/equipment, flags, names, gold, depot/deals, counter, raft and return town remain unchanged.
Both Map73 exit payloads round-trip completely equal. Fresh805 castle also cold-continues successfully.
The predecessor's unchanged mine-return slots upgrade16 records to17: every retained ally record and
both RNG words remain equal; only the compatibility revision and newly initialized Janet record differ.

## Optional callers and next consumer

Static inventory retains Map6 default/701/702, fourteen resident callbacks in the selected702 cast,
two chests/two loose-item placements and ten selected descriptions. Map7 retains default/701 and
admits805's eleven residents, two loose-item placements and two descriptions. The shared zone/init,
F802 talk branches and F802 stock are source-derived; Search placements retain their source flags.
The current observations cover inventor, the F802 item shop, town route conversation1123 and fresh
royal callers. Existing Church, weapon shop and Search engines are reused. Other resident talks,
weapon-counter transactions, church services and every optional Search are statically admitted but
not all naturally observed here. Earlier mine optional1705–1710 limits remain unchanged. The castle headquarters warp at (19,2)
still stops at an unavailable destination. The [Tactical Base visit design](tactical-base-design.md)
owns the source route, missing bindings and proposed optional visit and return.

Janet's actual ACHR sprite12 uses palette0; Elric's existing archer binding is not a substitute.
Maintained ROM/source preparation generates her weapon45/46/47 bodies and field sprite45, mounted
into all23 existing encounter scene packs. Their referenced rasters and member identity are checked
directly; missing bindings were repaired before handoff. The [Battle24 chapter](battle24-north-parmecia.md) observes Janet's natural ACHR attack.
Her promoted SNIP and BRGN bindings are admitted. [Promotion coverage](church-promotion-design.md#brass-gunner-promotion). North Cliff must consume actual Cannon/Dynamite custody and retain its own
current/fresh terrain distinction.

## Reproduction, gates and retained limits

Load the checkout's ignored private-input configuration. The stable private Issue1896 handoff supplies
unchanged return and ordinary audience/gift/Janet/exit slots, `Play.ps1`, `Prepare.py`, `Rebuild.ps1`,
native input/state/projection/error receipts and exact completed failure records. Use a fresh sample
and save namespace: `Play.ps1 -Language en -Sample review-route -Mode route`, then gift/departure or
`-StartAt exit` for the player. `zh-CN` selects Chinese. Existing namespaces are not overwritten.
Launchers resolve their bundle from their own path; rebuild takes an explicit checkout and fresh output.

`Prepare.py` consumes the registered ROM, pinned source, canonical map input, immutable predecessor
encounter/media inputs and sole translation corpus. It calls maintained exploration, icon, battle-scene
and catalog producers; only Janet's referenced actor rasters are mounted. Reproduction from another
cwd rebuilds the full package; final native observations use that rebuilt package. New work is under
the operator-selected task output root; earlier inputs/evidence remain read-only. Initial budgets are
1.5 GiB new storage, 1.5 GiB preparation memory and180s per serial native sample. The measured retained
package/build/receipts footprint is below1 GiB before final review artifacts, within the storage budget.

Run the dedicated engine unit project with filter
`NewGransealReturnTests|MineCannonTests|OpeningRestoreTests|LocalizationDisplayTests|MapEntityLifecycleTests`.
The controlled private caller test additionally selects `SF2_NEW_GRANSEAL_SAVE` to an ordinary audience
slot and the package's normal private inputs. Adapter compilation and direct package/catalog/reader
observations are required; new output paths use SDK artifacts/output overrides on the selected disk.
On the committed head run `uv run sf2 verify plan --base origin/main --head HEAD`, interpret shared
producer changes as engine-only preparation, and run the selected public checks/CI. No unrelated full
research or legacy aggregate rerun is required. Independent main-gate integration remains separate.

Completed discovery failures include reader sprite admission, Janet field bindings, audience helper,
item-jingle completion, missing Chinese selection and fresh805 population selection. Their retained
corrections have narrow witnesses. Route/probe mistakes and the corrected synthetic resident-slot test
are recorded separately from product defects. Native successful samples exit through existing Quit
controls with code0; ObjectDB warnings (including4 at Caravan exit) and the prior native exit failure
remain under [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716). No screenshots,
human quality claim or clean-teardown claim is made.
