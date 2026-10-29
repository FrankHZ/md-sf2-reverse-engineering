# Ketto: Conversations, Shops and the Merchant's Move

**Confirmed:** Ketto's residents describe devil-imposed isolation, while separate
callbacks offer a real Yes/No conversation, stage a merchant's move, invoke
shops/church, and handle inspections and two collectible items. Their words,
flags, movement and service results have distinct consequences. **Inferred:**
these are local activities following the accepted
[Ketto entry](battle25-outcomes-through-ketto-entry.md), rather than a mandatory
sequence. **Unknown:** natural access, supplied state, completed services,
presentation/timing, usable control and save/load persistence.

This English guide consumes the complete accepted
[Ketto interaction research](../../../../research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md),
whose pinned source, ROM identity, symbols, addresses, literals and reproduction
recipe own the original evidence. It follows the
[Standard scope agreement](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
for [#1002](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1002).
It adds no original finding or remake behavior and stops at local returns and
conditional Map72 admission, before Battle26's scene, roster or combat.

Fnnn denotes flag nnn; dialogue numbers identify selected text, not sequence gates.

## Addressing the Residents

**Confirmed:** F800 does not select a different population. Fresh Ketto declares
logical actors 128–135 after the actual ordinary followers; with n followers,
they normally occupy physical n+1 through n+8. Bowie/main is physical 0;
merchant 133 and woman 135 normally use physical n+6/n+8 and lookup offsets 37/39.
The [entry guide](battle25-outcomes-through-ketto-entry.md#a-separate-entrance-builds-ketto)
owns origins, walking bounds, follower aliases and finite storage premises.

Ordinary player action first idles physical 0 and waits for destination/view
scroll. A opens the field menu; C reaches activation after debug and overlapping
Caravan alternatives. With view target 0, activation uses current position and
facing, can look through desk/counter layout kind `$2800`, then selects the first
eligible physical actor within Manhattan distance 384 (exclusive). It skips the
viewed actor and current followers. A negative view target returns; another
camera state is not automatically ordinary interaction. The layout read is not
a layer-equality test. Reverse lookup searches the actor list from its beginning,
switches to logical 128 after the 32 ally entries and falls back to logical 0 on no
match. An earlier alias can therefore change which event is selected.

The entity wrapper stores player facing in EVENT_RELATIVE_POSITION and takes
the first matching logical row or default. Portrait/speech context comes from
the actual mapped sprite, not a narrative assumption about the intended NPC;
it prefills CURRENT_PORTRAIT/CURRENT_SPEECH_SFX and conditionally opens the
portrait. `UP1` on rows 128–134 means turn after a VInt to
`(playerFacing+2)&3`, without restoring saved facing. `DOWN3` on 135 also restores
saved actor facing after return. These are turn/restore action bits, not approach
gates. Default 0 does neither. The wrapper suppresses VInt_UpdateEntities before
the callback, then applies selected facing restoration, closes portrait/text
and activates entity updates on return.

| Actor / callback | Dialogue and local consequence |
| --- | --- |
|128 young man / Event0|1956 locates Ketto on the southern edge of North Parmecia; return.|
|129 old man / Event1|1957 asks where the boy came from; 1958 expresses surprise at South Parmecia; 1959 says the southern tunnel is blocked; return.|
|130 man / Event2|1960 questions coming from South Parmecia; 1961 says Pacalon has been isolated since opposing the devils; 1962 advises leaving; return.|
|131 young man / Event3|1963 complains of unusual heat; 1964 asks whether Mitula is as sick as rumored; return.|
|132 man / Event4|1965 says the devils forbade residents to go out; return.|
|133 merchant / Event5|Clear 800: 1966–1969, speed restoration, set 800 and run the relocation program. Set 800: select shop 9 and call ShopMenu; return if it returns.|
|134 priest / Event6|Clear 256: 1971 warning, then set 256. Both clear/set paths invoke ChurchMenu; return if it returns.|
|135 woman / Event7|Set 800: shop 27. Otherwise set 257: 1978 dismissal. Otherwise 1972 and actual YesNoPrompt, then 1973–1975 or 1976–1977 and set 257; return.|

The default entity callback is RTS within that wrapper. A separate
`nullsub_5D2E8` is not a ninth resident. No local callback or merchant program
sets 426, recruits or activates members, charges gold or grants an item.
Shared services have their own selected mutation paths.

These resident literals contain no name/item substitution. Successive `txt`
calls create/reuse the text window and reset its substitution pointer; they do
not implicitly close it, sleep or advance a cutscene cursor. W1/W2 text controls
are acknowledgements with distinct cursor, RNG, VInt/input and cleanup work;
neither creates a choice menu. The [dialogue contract](../../../contracts/interface/dialogue-system.md)
owns those consumers. Explicit text closure and wrapper cleanup remain separate.
Narrative prohibitions do not create travel, prayer or sales gates.

## The Woman's Choice and the Priest's Warning

**Confirmed:** with 800 clear and 257 clear, the woman says “Hey, listen to me.”
(1972), then actually calls YesNoPrompt. Yes starts selected. Held input at
entry must first release. Left selects Yes/0; Right selects No/$FF; those
selection actions loop. The loop tests Left, Right, B, C, A in that order;
B commits No, while C/A confirm the current selection. After its window cleanup,
the helper returns sign-extended Yes=0 or No/B=-1. The callback closes text
and branches on that word.

Yes requests 1973–1975: Pacalon was a good customer and bought a lot, but the
devils now prevent weapon sales; she calls them annoying. No or B requests
1976–1977: she apologizes and does not want to bother the player. Both paths
set temporary 257 only after the final text returns. Interrupting before that
write can leave the prompt eligible again. Held 257 instead gives 1978,
“You again? Bye now.” F800 is checked first: once set it overrides both the
prompt and repeat dismissal with the item shop.

The priest's 1971 says he cannot open the church and asks for understanding.
Temporary 256 suppresses only this first warning: its set follows the text,
and ChurchMenu is unconditional on both paths. Text 1970, “I'm a priest.”,
exists but none of these callbacks calls it. Neither the warning nor the
woman's account of forbidden sales disables the invoked service.

## The Merchant's Move and Shop Branch

**Confirmed:** with 800 clear, the merchant curses the devils (1966), recognizes
a customer (1967), offers secret high-quality weapons (1968), then asks whether
the player wants one and immediately assumes agreement, inviting them to his
shop (1969). There is no YesNoPrompt or result branch in this exchange.
Acknowledging 1969 is not consent, payment or an item grant.

Event5 saves MESSAGE_SPEED's byte, writes 0/1/2/3 before 1966/1967/1968/1969,
restores the original byte, then sets persistent 800 **before** starting
`cs_5D3B8`. The typewriting consumer derives ordinary glyph-delay operands
4/2/1/0 from those values; speech and W1/W2 work remain separate, so these are
not elapsed-time promises. A text interruption may leave a temporary speed;
a later interruption may leave 800 set and only a prefix of the actor changes.
There is no rollback. The set 800 shop branch repeats none of this scene.

The complete program proceeds in this order:

| Stage | Command and consequence |
| --- | --- |
|Merchant speed|`customActscriptWait 133` installs paired X/Y speed 64 and a jump to eas_Idle; it waits for that idle pointer before continuing.|
|Bowie destination|`setDest ALLY_BOWIE,12,2` uses actor word 0 and waits until both positions equal their destinations.|
|Bowie facing|Only after that wait, `setFacing ALLY_BOWIE,DOWN` requests Down.|
|Five merchant segments|Five separate `entityActionsWait 133` streams move Right5, Down1, Right2, Up1, Right2, each returning to idle before the next.|
|Bowie gesture|`headshake ALLY_BOWIE` performs its shake work.|
|Final placements|`setPos 133,25,18,RIGHT`, then `setPos 135,25,17,RIGHT`, then script end.|

The accepted correction is essential: **actor 0 setDest waits**. Its consumer
retains the full actor word, writes destination/travel/±32 velocity and tests
bit 15. Only a set high bit skips WaitForEntityToStopMoving. An altered operand
`$8000` would still resolve logical 0 but skip waiting. The actual scene uses 0.
The following facing command has an ally-HP guard and can skip dead Bowie;
setDest and headshake have no such guard. This is conditional command behavior,
not proof that the actor settles or that the complete scene returns.

Each merchant segment generates 14 action bytes: signed relative tile motion,
a destination wait and jump-to-idle. Five streams total 70 generated bytes.
Installation clears FLAGS_A bits 5/6; bit 5 would gate destination obstruction.
Motion adds scaled deltas to the actor's actual current position and uses
stored speed. The net relative displacement is (+9,0): from untouched (11,3),
the conditional endpoint is (20,3), before final shop placement. A changed origin
changes that endpoint. These commands prove neither pathfinding nor access.
The speed 64 install itself waits on the idle pointer; it does not prove a
particular displayed animation or elapsed duration.

Headshake runs seven iterations with six explicit VInt waits each, plus sprite
transforms/DMA requests. Its 42 named waits are not total-duration or completed-DMA
proof. Final setPos writes both position and destination at tile×384 and Right,
then requests sprite update. It does not walk, replace action scripts or clear
velocity/travel. Merchant/woman NPC selectors bypass the ally-HP guard. A missing
zero lookup aliases physical 0; hidden `$FF` is not a safe no-op. Current mappings
must actually identify the intended actors. Interruption can leave only the first
placement. Script end can wait for scrolling with open text and clears scroll
speed. Text-skip does not skip these motion commands; service progress remains
a premise throughout.

## Services and What Survives Re-entry

**Confirmed:** after 800 is set, the merchant selects CURRENT_SHOP_INDEX 9; the
woman selects 27. These are zero-based length-prefixed inventory indexes:
9 is the tenth physical row (comment “Weapon shop 10”), not comment “Weapon
shop 9”; 27 is physical row 27 (comment “Item shop 13”, alias SHOP_ITEM_MOUN).
There is no separate local type byte selecting another service.

| Index / category | Ordered stock, with ITEM_ prefix omitted |
| --- | --- |
|9 / weapon|BROAD_SWORD, BUSTER_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ROBIN_ARROW, ASSAULT_SHELL, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|27 / item|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

ShopMenu conditionally opens the current portrait, requests 158, closes
text/portrait and enters the common selector. Stock and invocation establish
no successful purchase, affordability, free item or completed transaction.
The [service contract](../../../contracts/party/service-interactions.md#shop-source-boundary)
owns buy/deals payment-before-add, deals removal, sell/drop/rare-deals,
repair, cancellation and looping, with inventory/equipment/capacity premises.

ChurchMenu conditionally opens the current portrait, requests 110, closes
text/portrait and enters raise/cure/promote/save. Merely entering it neither
changes EGRESS_MAP nor heals or saves everyone. In the selected Save branch,
zero at the first prompt copies CURRENT_MAP to EGRESS_MAP (here 25), sets 399,
then calls SaveGame with CURRENT_SAVE_SLOT. Declining that prompt does not
commit the path; canceling before Save retains the old selector. The later
continue/suspend prompt is separate: zero exits, nonzero reaches fade and
WitchSuspend instead of ordinary callback return.

The accepted [Church boundary](../../../contracts/party/service-interactions.md#church-source-boundary)
and [save contract](../../../contracts/system/save-system.md) retain shared service observations;
they are not new Ketto runtime evidence. Ketto's savepoint datum is
Map25 (12,11),Up. GetSavepointForMap uses this table path when 399 is set;
clear 399 uses the initial Granseal fallback, with later raft-reset handling
retained by the entry owner. The row is a possible return datum, not an entry
teleport, completed save/load or persistence guarantee.

| Entry/state | Local consequence |
| --- | --- |
|Fresh explicit 25,800 clear|Rebuild NPC origins/actions and clear temporary 256–383; init does not relocate.|
|Fresh explicit 25,800 set|Rebuild, then run cs_5D3AA's two setPos placements only. No speed 64 install, route, text or headshake; fresh eas_Init supplies speed 32 policy.|
|CURRENT255 on current 25|Retain actors/actions/temp flags and run init. Set 800 reapplies placements to current mappings; clear 800 does not restore origins.|
|Scroll|Skip reset/init; do not infer fresh actors or temporary-flag reset.|
|Manually clear 800 after movement|Can replay relative motion from changed positions; no automatic restoration or repair.|

Persistent 800 differs from temporary 256/257: fresh entry resets the warning
and woman's repeat state, but 800 still overrides her prompt with shop 27.
CURRENT on another map is not Ketto. Supplied modes, mappings and interrupted
prefixes retain their own effects.

## Books, Signs and Two Search Items

**Confirmed:** ordinary CheckArea examines the tile ahead with view target 0,
clears dialogue-name index 1 and speech SFX to 0, and prioritizes eligible
chest/vase/barrel/bookshelf/area-item layout kinds before the local description
handler. Normal no-entity C supplies d6=1. Coordinates alone do not establish
layout eligibility, accessibility or an actual inspection.

All 11 local rows have condition 0/kind 0: none requires d6=0. First admitted
packed X/Y match displays investigation 423+offset followed by description
4132+offset, then closes portrait/text and returns -1; no match returns 0 for
caller fallback. The investigation uses member 0's actual name through NAME,
not hardcoded “Bowie”. Offset 4/text 427 says the name investigated the book
shelves; offset 2/text 425 says the name investigated the sign. Its W2/CLEAR
continuation differs from the description's final W1.

| X,Y / investigation | Description offset / ID | English content |
| --- | --- | --- |
|2,10 / book shelves|0 /4132|A book: TRAVELS.|
|3,10 / book shelves|1 /4133|A book: VILLAGES AND TOWNS.|
|2,16 / sign|2 /4134|Do not go out or be punished; signed Devils.|
|6,26 / sign|3 /4135|“You doubt the punishment?”; signed Devils.|
|15,26 / sign|4 /4136|Church is closed.|
|15,15 / sign|5 /4137|No praying today; signed Devils.|
|14,18 / sign|6 /4138|Pacalon will never come to save you.|
|16,19 / sign|7 /4139|Do not work.|
|22,29 / sign|8 /4140|Mitula ignores you.|
|27,24 / sign|9 /4141|No weapon or item sales; signed Devils.|
|16,28 / sign|11 /4143|Village of Ketto.|

No local row selects offset 10/text 4142. The signs contribute the town's
oppressive context, without imposing executable service prohibitions.
The [description contract](../../../contracts/world/map-area-description-routing.md)
retains the shared first-match and caller boundary; no map was extracted here.

The chest table is empty. The other-item table contains BRIGHT_HONEY at (4,22)
with collection flag 170 and HEALING_WATER at (29,18) with flag 171. These are
collection flags, distinct from 256/257. Shared nonchest search requires an
eligible masked layout kind (`$2C00/$3000/$3400/$1C00`) and the actual coordinate
match, using low bytes and applying battle crop only outside CURRENT_BATTLE=255.
No match returns -1. Empty chest data does not disable shared search.

For a matching row, a held collection flag yields ITEM_NOTHING and is set again;
a clear flag returns the item and sets the flag **before inventory success**.
Text 413 uses name index 1=0 and the selected item in index 2. Bowie with fewer
than four items receives AddItem. Otherwise UpdateForce and the ordered other
joined-force list select the first member with room, regardless of HP or active
battle membership. That list starts one byte after TARGETS_LIST and assumes
ordinary joined Bowie occupies its first entry. With Bowie unjoined, the initial
slot 0 attempt still happens, but fallback omits the actual first joined member;
it does not repair that supplied state.

Success retains the flag, requests 415 or 416 (third name index identifies the
recipient), item music and a separate music/plain-input helper. Those messages
do not acquire invented W1/W2 tokens. Acquisition is not consumable use.
All-full instead requests 417, then CloseChest and RefillNonChestItem re-find
the row at saved coordinates and clear its flag for retry. Already collected
ITEM_NOTHING takes empty-object text rather than granting again. Interruption
after flag setting can leave no item and no refill; interruption after AddItem
can leave an item without completed presentation. Full-force recovery is not
a transaction guarantee.

Default zone and item-event callbacks are RTS, but their wrappers still clean
up and wait for a VInt/physical 0 destination. Item-event setup also clears
speech, masks the used item, stores supplied Y in EVENT_RELATIVE_POSITION and
sets d6=0. Item-use dispatch is separate from other-item collection. These
no-ops do not disable shared handlers or unlock 426.

## Leaving Ketto Is a Separate Request

**Confirmed:** after callbacks/services actually return and cleanup completes,
the caller can reach WaitForEvent. Pending 0 selects camera physical 0 and requests
control: mode 0 character, mode 1 raft, other nonzero mode Caravan. Pending
nonzero returns that event instead. These are conditional requests, not an
observation of playable control or completed input/text/menu/action services.

The sole outgoing row matches wildcard X255/Y31 and requests no-scroll
Map72 (31,27),Down. It has no 800/256/257/426, item or party predicate. Ordinary
character control still needs qualifying layout kind `$1000` and the warp
producer; Y31 alone does not instantly leave. The producer writes mutable
type 1/parameters and sound. Consumption clears type, submits/clears sound,
idles 0, unwinds exploration, samples coordinates, replaces nonnegative
coordinate low bytes and applies facing. Nominal (31,27) assumes ordinary
sampled high bytes. Replacement or interruption can change the request.

MainLoop subsequently runs SwitchMap/CheckBattle. Map72 candidates 8/24/26
are ordered, all with wildcard triggers and independent unlocks 408/424/426.
First unlocked wins; a matching held completion 508/524/526 clears that unlock
**after selecting the same battle**. Crops are output, not containment tests.
No local Ketto callback writes 426; shopping and 800 do not select Battle26.
Without admission, Map72 setup/init still applies, including F65 Caravan
eligibility and the independent 716-set/Luke10-clear scene.

Later zones X32–34 can call helper 26, which tests 526, not 525. Clear 526 requests
immediately. Set 526/nonzero STEP_COUNTER declines; zero counter draws range 8:
zero requests, otherwise range 4+2 supplies counter 2–5 and declines. A request
sets 426 and CURRENT255 before later services; the next scan can still select 8/24
first. Other cave/default zones retain their
[accepted behavior](battle25-outcomes-through-ketto-entry.md#separate-cave-departures-and-map72-admission).
This boundary establishes no necessary, navigable or unopposed route and stops
before Battle26's selected scene, formation, combat or aftermath.

[Battle26 admission and the Geshp confrontation](battle26-admission-and-geshp-confrontation.md)
continues through the independently admitted fresh before-battle scene: actual
field cast, both Pacalon soldiers, Geshp's possession threat and the Granseal
interruption. Its endpoint remains before fresh healing/formation/load/start;
the connection does not establish natural travel, combat deaths or Higins recruitment.

## Evidence Review and Retained Results

The complete accepted research and its changed-state matrix were reviewed,
including actual activation/aliases/approach bits, all 800/256/257 paths,
Yes/No/B and interrupted commits, actor 0 versus `$8000`, dead/missing/hidden
actors, relative origins, reload modes, stock indexes, service cancellation,
Save/suspend, coordinate/layout priority, collection/refill and ordered exit
admission. Selected public surfaces were inspected directly:

| Surface | Reproducible owner and limit |
| --- | --- |
|Entity/default callbacks and wrappers|Composed map-events fixture, including entity-programs and routing/default siblings; selected public operations do not replace private actor/script rows.|
|Interaction/dialogue state|`sf2-map-event-interaction-state-static-v1` and `sf2-map-event-dialogue-state-static-v1`; actual producer/consumer roles and text state, not natural activation or delivered text.|
|Choice/services|`sf2-common-menus-static-v1`, menuFacts.yesNoPrompt and serviceStateMachines; confirmed return values and selected mutations, not completed Ketto transactions.|
|Motion/init|`sf2-map-script-engine-static-v1` placement/action consumers and `sf2-map-init-static-v1`; actual high-bit wait and initialization order, not animation completion.|
|Inspection|`sf2-map-descriptions-static-v1`; first-match/arithmetic/layout boundary, not all local literals or reachable tiles.|
|Departure/savepoint|`sf2-map-event-request-consumption-static-v1` and `sf2-common-maps-static-v1`; mutable request/admission/return rules, not a natural journey.|

The maintained [map-event loader](../../../../../src/sf2tool/h2/map_events_fixture.py),
[menu verifier](../../../../../src/sf2tool/h2/menus.py),
[script verifier](../../../../../src/sf2tool/h2/map_script_engine.py),
[description verifier](../../../../../src/sf2tool/h2/map_descriptions.py) and
[request consumer](../../../../../src/sf2tool/h2/map_event_request_consumption.py)
retain those schemas and focused boundaries. No in-scope contradiction was
found; source contradictions return to research. Complete local tables,
text literals and private program rows remain owned by the accepted research,
not proven anew by aggregate schema validation.

Owning commands remain `uv run sf2 h2` with `map-events`,
`map-event-interaction-state`, `map-event-dialogue-state`, `common-menus`,
`map-script-engine`, `map-init`, `map-descriptions`,
`map-event-request-consumption` and `common-maps`; the research owner links the
retained-pair recipe. Source/pair absence in this design environment is accepted.
Public checks and prose review are **not a fresh comparison**. No acquisition,
regeneration, runtime, environment change or new maintained machinery is needed.

[#997 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/997#issuecomment-6181515501)
and [PR1000 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1000#issuecomment-6181536109)
retain `cb3d3f0b`/tree `42adda37`/base `10e1ec16`, CI 38659895203
(scope 15s/research 47s), the 0.081s unchanged-pair comparison, the waiting-Bowie
correction, zero-based stock 9 and the opcode-case assertion failure/correction
and diagnostics. The accepted actor 0 wait is preserved here.

[#998 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/998#issuecomment-6181422397)
and [PR999 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/999#issuecomment-6181437287)
retain `d086715e`/tree `2dac7cbc`, CI 38659282728 (scope 12s), eleven schemas plus
map-events/531 links, the failed readability-link check/correction and unavailable
source/pair. Its [retained review](battle25-outcomes-through-ketto-entry.md#evidence-review-and-retained-results)
keeps predecessor corrections, failures and Unknowns. Completed results are not reset.

The #1002 Issue/PR handoff records full prose/diff/navigation/link/anchor,
UTF-8/private/exact-five-path checks, git diff --check, configured clean
committed planning and actual exact-head CI. Documentation scope launches no
blanket normal/full/Python/.NET/Godot/H3/H4/translation suite. Supplied state,
natural reach, service progress, rendered timing/control and save persistence
remain **Unknown**, without creating an automatic acquisition queue. Independent
acceptance and integration remain with main-gate.
