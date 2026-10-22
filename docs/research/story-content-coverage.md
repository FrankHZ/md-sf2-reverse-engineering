# Story Content Coverage

Evidence date: 2026-10-22. **Confirmed:** accepted research owners describe a
conditional story spine from the Map3 opening through both continents, the return
to Grans, Zeon, the epilogue, credits and Battle0's reset caller. That spine has
explicit content gaps. The [Fairy Woods44 account](./battle44-fairy-woods-trial-and-records.md)
connects Map23/24 invitation, hidden/respawning formation, timer/record and returns.
The [Battle14 desert account](./battle14-desert-encounter-and-outcomes.md)
connects Map70 random requests, complete base formation, repeat upgrades, control, drops and returns.
The [Battle15 shrine account](./battle15-shrine-approach-and-outcomes.md)
connects exterior61, the complete scenes/formation/control boundary and conditional outcomes/exits.
The [Battle28 bridge account](./battle28-bridge-and-outcomes.md)
covers its conditional entry, complete scenes/formation/control boundary and outcomes/returns.
It is not a complete campaign
walkthrough, an all-content denominator or proof of naturally continuous play.
**Inferred:** the chapter groupings below are a useful reading order over those
connections. **Unknown:** complete original natural chronology, all optional
branches, actual service completion/timing, hardware behavior and durable saves.

## Scope and reading rules

This owner maintains the current story-content coverage relationship for
[Issue1196 / Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1196).
**Standard** rigor fits a cross-owner documentation assessment: chapters need
traceable entry/exit boundaries, conditional connections and explicit omissions.
The outcome supports selecting the next coherent research slice and prevents
static research progress from being mistaken for design or engine completion.

The denominator is the named chapter connections, the Battle0–43 omission check,
and selected optional branches below. It is **not** every NPC, text, chest,
service visit, map-state combination, combat action or return itinerary. The
[global coverage ledger](./source-coverage.md) continues to own registered
source/fixture denominators. A later unregistered prose owner does not add a
fixture, an indexed finding or a runtime observation to that ledger.

Use these distinctions throughout:

- **Confirmed static** means the linked accepted owner supports the stated
  source condition, operation or interface. The chapter summaries inherit that
  limited evidence, not every possible interpretation of the scene.
- A posted warp/reload, an admitted destination, completed initialization and
  usable control are different endpoints. Ordinary entry, retained CURRENT
  reload, scrolling and in-script scene loads can have different populations and
  caller lifetimes. A connection in the tables does not prove a walk through it.
- Joined force membership, active party, AI control, temporary following and
  rebuilt entity identity are separate. Spoken rewards are not inventory writes;
  scene actors are not the subsequent combat roster. The linked owners retain
  interruptions, supplied-state branches, repeat gates and service failures.
- Battle numbers are identifiers. They do not prove order, mandatory visits,
  unavoidable losses, geometric eligibility or accepted content coverage.
  Opening/roster/first-control/outcome research does not establish every AI action,
  boss move, legal strategy or a played battle.

All original-source paths here are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The root [baseline](../../README.md#pinned-original-baseline) identifies the USA
ROM. This assessment reuses accepted owners and bounded read-only source/code
inspection; it makes no fresh ROM equivalence, extraction or runtime claim.
No dialogue transcript, source asset or private payload is reproduced here.

## Four different frontiers

| Surface | Current accepted boundary | What it does not establish |
| --- | --- | --- |
| Original static story content | Chapter owners below reach ending/credits/Battle0 reset, with named gaps and conditional links. Global map/script/battle tables provide structural coverage separately. | Natural continuity, all optional content, all combat mechanics or whole-game semantic completion. |
| Original runtime observations | The [accepted original frontier](../design/synthesis/map3-battle01-readiness.md#accepted-original-frontier) records controlled R1 through the savestate-linked messenger/castle/tower route, natural Battle01 admission, selected player/AI actions, natural victory, after-program return and stable Map57 field readiness. [PR504's native result](./map3-messenger-acceptance.md#native-victory-and-stable-field-readiness) and [PR526's ordinary-input result](./map3-messenger-acceptance.md#native-post-victory-ordinary-input-result-issue-515) establish these bounded observations and the subsequent one-step Down effect. | Natural visible New/load, uninterrupted wall-time execution, all action branches, later chapters/ending, complete presentation or hardware parity. Savestate-linked original observations remain distinct from remake acceptance. |
| English design | [Roadmap](../design/documentation-roadmap.md#current-baseline-and-near-term-synthesis), [story framework](../design/synthesis/story-progression.md) and accepted [Battle27 activation/first-call guide](../design/synthesis/battle27-activation-and-first-turn-dispatch.md); [PR1044](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1044), merge `dee915fd`. | Research after that boundary has not automatically become English synthesis; Chinese synchronization has its own deferred policy. |
| Remake production | Accepted [Map3–Battle01 composed milestone](../design/synthesis/map3-battle01-readiness.md#accepted-current-milestone), Battle02 outcomes, and selected early Yeel→Battle03 play/control. [PR1050](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1050), merge `fe8ac968`, and [PR1052](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1052), merge `13cce997`, are ancestors of accepted main. [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation), [trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter) and [battlefield rendering](../../remake/docs/godot/battle-scenes.md#battlefield-maps) own actual behavior. | Battle03 outcomes/Hawel recruitment/Battle04, later campaign, full services, save/load or full-game parity. Controlled initial remake state and modern presentation are explicit. PR692 is an independent UI topic and supplies no capability here. |

The [acquisition owner](./map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
and [research audit](./map3-battle01-audit.md) retain the selected winning lineage
and its failures; the accepted frontier distinguishes PR504's stable return from
PR526's later single Down read, movement acceptance and settled displacement.
These observations start at controlled R1 and validate compatible saved parents;
they are not a natural New/load flow or uninterrupted execution. Earlier
[opening](./map3-battle01-natural-route.md) and [seeded R2d bridge](./map3-battle01-admission.md)
keep their own narrower projections. R2d's seeded actor1 and the [frozen replay
failures](./original-reference-replay-capability.md#current-lineage-hard-stop)
do not downgrade the later accepted natural admission/winning observations.

The [capability document](../../remake/docs/capability-status.md#current-engineering-frontier)
retains candidate/pending wording in those two implementation agreements.
Their merged Git objects and current production owners establish the accepted
bounded code; the old wording is not proof that the PRs remain unmerged.
Human-play/aesthetic acceptance and original timing limits remain distinct.
The research frontier below changes neither consumer frontier.

## Grans: opening, captivity and the first tower

**Confirmed static:** the Map3/castle/tower owners connect the opening to
Battle01 and its return. Subsequent owners connect conditional outcomes through
Yeel, Hawel's house, Galam and occupied Granseal. Caller state, setup flags,
followers, recruitment and battle admission remain separate.

| Segment and locations | Organized content / handoff owner | Connection or gap to retain |
| --- | --- | --- |
| Map3; castle19/20/21, passage40, tower57; Battle01 | [Optional interactions](./map3-optional-interactions.md), [castle unlock](./map3-castle-battle-unlock.md), [admission](./map3-battle01-admission.md), [turn/action chain](./map3-battle01-turn-control.md) through [victory return](./map3-battle01-victory-return.md). | Optional records are not required observed actions. Action-effect/completion/finalization owners remain reachable from the turn owner; the static spine is not original executed victory. |
| Battle01 return; exorcism/commission, Jaha; Battle02/Map66 | [Return through Battle02](./battle01-to-battle02-content-route.md). | Conditional unlock and separate admission; no naturally walked continuation is proved. |
| Battle02 return; Yeel5/Kazin; Battle03/Map66 | [Return through Battle03](./battle02-to-battle03-content-route.md). | Yeel guidance does not write the unlock and is not a proved prerequisite. Following is not Kazin recruitment. |
| Battle03 return; Hawel house43; Battle04 | [Hawel-house route](./battle03-to-battle04-content-route.md). | House scene recruits Kazin/writes404; Battle03's empty after-script does neither. House admission does not require proved Battle03 completion. |
| Battle04 outcomes; Galam17/16; Battle05 | [Captivity/guard admission](./battle04-to-battle05-content-route.md). | Victory and special defeat have different consumers; escape differs. No unavoidable defeat claim. |
| Battle05 outcomes; Galam exits18/66; Battle06 | [Jewel/Slade/escape](./battle05-to-battle06-content-route.md). | Joining, current following and returned requests differ. |
| Battle06 return; Granseal3/19, tower58; Battle07 | [Occupied town, optional Kiwi, royal following](./battle06-to-battle07-content-route.md), then [Battle07 aftermath](./battle07-outcomes-and-aftermath-handoff.md). | Earlier before-program supplies407; victory supplies506. After07 posts a return before later follower/flag work; rebuilding does not preserve all scene actors. |

This is not a complete encounter-specific initialization, activation and
all-action audit for Battles02–07. Generic battle evidence does not close those
individual encounter details.

## Leaving Grans and establishing New Granseal

[Post-tower requests](./post-tower-return-through-earthquake-entry.md) connect
selected tower/castle exits to earthquake substitution of Map3 with4.
[Evacuation/docks44](./earthquake-town-through-docks-departure.md) separates the
boarding choice, actor reconstruction, in-script map load and Map73 request.
[Voyage/landing](./voyage-through-damaged-ship-control.md) follows it through the
damaged-ship Map14 context; in-script loads bypass ordinary Map14 init.
**Unknown:** walking, completed services and actual pending state, including the
docks owner's separately documented zone-table anomaly.

[Plank choices/rescue admission](./damaged-ship-plank-through-battle08.md) lead
conditionally to Battle08/Map72. [Outcomes/construction](./battle08-outcomes-through-new-granseal-handoff.md)
lead through Map73 to the separate [year-later castle7 init](./new-granseal-castle-year-later-control.md).
[Town6 introductions](./new-granseal-town-through-peter-introduction.md) and
[Peter's castle audience](./peter-castle-audience-through-town-return.md) cover
independent scene gates, force/AI/follower effects and town return. The spoken
gold gift is not a confirmed increase. No chapter label makes the year
transition, map handoff or first control read an observed runtime.

## Eastern Parmecia: Ribble, Polca and Bedoe

| Segment and encounters | Organized boundary | Conditional or optional relation |
| --- | --- | --- |
| New Granseal→Map73/72/69; Battle09 | [Departure/admission](./new-granseal-departure-through-battle09.md), [outcomes/Ribble8 welcome](./battle09-outcomes-through-ribble-welcome.md). | Scroll does not scan combat or rerun init. Victory does not itself enter Ribble. |
| Ribble→Map70; Battle10 | [Optional May/admission](./ribble-may-through-battle10-admission.md), [outcomes/East Shrine62](./battle10-outcomes-through-east-shrine-visit.md). | May's invitation is linear, not Yes/No. Optional shrine/following is not a proved Battle11 prerequisite. The separate Battle14 random encounter is organized below. |
| Shrine return/Map69/72→cave48; Battle11 | [Cave admission](./east-shrine-return-through-battle11-admission.md), [outcomes/Polca13](./battle11-outcomes-through-polca-arrival.md). | Darkness/presentation and crossing are Unknown; separate Polca entry builds Oddler's scene. |
| Polca→Volcano52; Battle12 | [Optional turtle/east departure](./polca-fairy-through-battle12-admission.md), [outcomes/Bedoe10](./battle12-outcomes-through-bedoe-entry.md). | Neither turtle nor first Oddler scene is required by the exit. |
| Bedoe10→Volcanon11→Bedoe | [King address/audience](./bedoe-king-address-through-first-audience.md), [Volcanon/Peter](./bedoe-to-volcanon-audience.md), [king choice/Luke](./volcanon-return-through-bedoe-king-choice.md), [hatchling/rescue](./bedoe-hatchling-flight-and-rescue.md). | Address/audience gates are independent; invitation is not shrine admission. Demonstration is not rescue admission; rescue is not Skreech recruitment. |
| Bedoe return→Polca; Battle13 | [Confrontation/Gerhalt](./bedoe-return-through-battle13-admission.md), [outcomes/raft grant](./battle13-outcomes-through-polca-raft-grant.md), [Oddler choices](./polca-oddler-choice-and-following.md), [Luke recruitment](./polca-departure-through-luke-recruitment.md). | Outcome installs Luke following, not joining; later init joins him. Mayor, Oddler requests and departure have independent gates. |

## Southern Parmecia: transport, the sword and Taros

[Raft boarding/southern exits](./polca-raft-boarding-through-south-parmecia-entry.md)
separate movement request, alignment, boarding completion, mode write and
scroll/no-scroll. [Kraken/Battle16 entry](./south-parmecia-through-kraken-battle16-entry.md)
on49 uses416 independently of raft mode. [Outcomes/disembarking/Hassan9](./battle16-outcomes-through-hassan-entry.md)
separate victory transport, defeat/Egress savepoints and town population.
The [Battle14 desert account](./battle14-desert-encounter-and-outcomes.md) covers Map70 requests,
ordered10-before14 admission, base formation/conditional repeat upgrades, control, drop and returns.
The separate [Battle15 shrine account](./battle15-shrine-approach-and-outcomes.md)
does not make either encounter an implicitly completed predecessor of16.

| Branch / onward encounter | Accepted owner and boundary | Remaining connection |
| --- | --- | --- |
| Hassan/Rohde; Ribble8/tree, separate Map50 chest regions | [Possession/choice caller](./hassan-ancestry-and-rohde-caravan-choice.md), [Panel/tree/Achilles Sword](./ribble-tree-opening-and-achilles-sword.md), [exterior61/Battle15](./battle15-shrine-approach-and-outcomes.md). | Recognition neither acquires nor consumes the sword; acceptance neither joins Rohde nor acquires Caravan. The separate shrine account does not prove a walk between50's regions. |
| Hassan→Map69; Battle17 | [Departure/shared outcomes](./hassan-departure-and-battle17-outcomes.md). | Walking zone producer differs from raft control;417 is independent of418. Empty hooks retain shared work. |
| Map69→68→Taros12; Battle18 | [Entry/warning/opening](./southwest-parmecia-through-taros-battle18-entry.md), [outcomes/Caravan](./battle18-outcomes-and-caravan-acquisition.md). | Admission precedes shrine init. Vehicle actor, flags and return seed differ. Not Taros's complete tactical repertoire. |
| Optional Rick; Caravan toward19 | [Rick](./hassan-rick-recruitment.md), [boarding/travel/landing](./caravan-boarding-and-southwest-parmecia-travel.md). | Rick is separate optional interaction. Caravan mode lacks walking's19 zone producer; landing and later request are separate. |

## Elven Village, Harpy Pond and Fairy Cave

[Battle19 outcomes/Elven Village23](./battle19-outcomes-through-elven-village-entry.md)
retain no mandatory victory-before-village predicate. [Village conversations/search](./elven-village-conversations-and-local-exploration.md)
organize items/descriptions and current-map stairs. [Departure/Harpy Pond51](./elven-village-departure-through-harpy-pond-entry.md)
connect selected exits/scrolling to help scenes without a compulsory village
itinerary. [Approach/Battle20](./harpy-pond-approach-through-battle20-entry.md)
and [outcomes/Elric](./battle20-outcomes-and-elric-recruitment.md) distinguish
admission, neutral actor, force join and lost transient following.

[Elric's passage scene](./harpy-pond-departure-through-fairy-cave-entry.md)
conditionally changes live blocks and requests Fairy Cave32. [Rewards/returns/rock](./fairy-cave-exploration-and-devils-tail-return.md)
have separate consumers; the hint does not open the passage. Void setup still
constructs ordinary actors. Battle44/Fairy Woods on24 is a different optional
encounter, outside this account.

## Creed's small worlds and the consultation

| Chapter boundary | Accepted content owners | Connection or omission |
| --- | --- | --- |
| Devil's Tail67/Battle21→Mansion33 | [Opening/initial roster](./battle21-opening-and-initial-roster.md), [outcomes](./battle21-outcomes-and-devils-tail-return.md), [Goliath obstruction](./creed-mansion-entry-and-first-goliath-scene.md). | Entry does not automatically run Goliath's zone scene. |
| Magic door→Desktop22/Battle22 | [Shrinking/transfer](./creed-magic-door-and-desktop-transfer.md), [arrival](./desktop-kingdom-first-arrival.md), [King permission](./desktop-kingdom-interactions-through-battle22-entry.md), [initialization](./battle22-initialization-and-roster.md), [outcomes](./battle22-outcomes-and-desktop-return.md). | Door gate is independent of earlier Goliath flag. Reward announcement is not Balloon acquisition. No full Chess AI audit. |
| Balloon→Floor World26→Burrow27/Battle23 | [Acquisition/use/departure](./cotton-balloon-through-floor-world-entry.md), [landing](./floor-world-first-arrival.md), [interactions/exits](./floor-world-interactions-and-exits.md), [Burrow/admission](./willard-burrow-through-battle23-entry.md), [initial roster](./battle23-initialization-and-roster.md), [outcomes](./battle23-outcomes-and-burrow-return.md). | Chest flag, custody, own-tile use and force-wide removal differ. Ring scene is not an exit prerequisite. |
| Restoration→Evil Spirit34→return | [Restoration](./creed-restoration-and-basement-departure.md), [shrine entry](./evil-spirit-shrine-entry-and-scene-admission.md), [consultation](./evil-spirit-consultation-and-zone-return.md), [Fairy/recruit offers](./creed-return-fairy-and-recruitment-choices.md), [Oddler stays](./oddler-stays-and-fairy-handoff.md). | Current actors and future populations differ. Eight offers cover named initial/F22 alternatives, not every natural return itinerary. Narration/lore is not independently established world fact. |

## Returning for the Cannon and opening North Cliff

[Mine30/Fairy recovery/Cannon](./creed-departure-dwarf-recovery-and-cannon-gift.md)
is an accepted **bounded part** of dwarf content. It does not complete later
Dwarven Village29/30 or smithing. [Astral's return audience](./new-granseal-return-and-astral-following.md),
[inventor/Dynamite](./new-granseal-inventor-and-dynamite-gift.md) and
[Janet](./new-granseal-janet-recruitment-and-departure.md) have independent flags,
capacity and following; the audience does not check Cannon, and Janet does not
require Dynamite or Elric membership.

[Battle24 admission](./new-granseal-departure-through-battle24-admission.md),
[outcomes/preparation](./battle24-outcomes-through-north-cliff-preparation.md),
[blast/Rohde join](./north-cliff-opening-and-rohde-recruitment.md) and
[Battle25 entry](./north-cliff-entry-through-battle25-admission.md) connect Map72
to North Cave48 at separate request/consumer boundaries. Removals, effects and
caller flags are ordered partial effects, not one rollback transaction.
[Battle25 outcomes/Ketto](./battle25-outcomes-through-ketto-entry.md) and
[Ketto/merchant relocation](./ketto-interactions-and-merchant-relocation.md)
reach departure. Shop/church entry does not complete every service or revisit.

## Ketto, Higins and drought-state Pacalon

| Boundary | Accepted owners | Retained limitation |
| --- | --- | --- |
| Battle26 admission→fresh battle→first call | [Geshp/Higins](./battle26-admission-and-geshp-confrontation.md), [initialization](./battle26-initialization-and-roster.md), [activation/dispatch](./battle26-activation-and-first-turn-dispatch.md). | Scene/neutral Higins, joined ally and deployed actor differ; extra-slot/deals alias is conditional. Dispatch is not an executed turn. |
| Battle26 result→Map72/71/78→Pacalon2 | [Outcomes/Higins](./battle26-outcomes-and-higins-recruitment.md), [fresh return/town admission](./battle26-return-through-pacalon-entry.md). | Return seed precedes scene staging. East scroll does no immediate battle scan;431 can select31 before town. |
| Pacalon town/castle36→Map74/Battle27 | [Town/services/exits](./pacalon-interactions-and-castle-handoff.md), [first castle visit](./pacalon-castle-entry-and-first-visit.md), [north departure](./pacalon-departure-to-battle27-admission.md). | Drought dialogue is not a quest-flag write. Optional centaur/stairs have separate lifetimes. Later Frayja is separate. |
| Battle27→Tristan15 admission | [Opening](./battle27-initialization-and-opening.md), [activation/first call](./battle27-activation-and-first-turn-dispatch.md), [outcomes/Map74/Tristan](./battle27-outcomes-through-tristan-admission.md). | Victory does not itself set428–430; a separate zone can set them together.28 is a separate bridge branch, not29. |
| Map74⇄bridge47/Battle28 | [Both entrances, complete bridge scenes/roster/control and outcome returns](./battle28-bridge-and-outcomes.md). | Clear528 victory retains the after-scene through CURRENT47, including CURRENT_BATTLE28; repeat victory rebuilds explicit47. Neutral woman/B31 and Astral's scene alias differ. Dialogue/quake do not establish physical collapse or a countdown; neither table nor outcome proves natural travel to Tristan. |

## Tristan, Mitula and restored-rain returns

[Tristan](./tristan-entry-interactions-and-exits.md) separates F530 populations,
conversations/services and cave requests. [Caves35](./tristan-caves-and-shrine-exterior-admission.md)
cover chests and CURRENT/external warps through Map0 admission. This route is
toward **Battle29**, not proof that Battle28 bridge was traversed or29 must
immediately follow27.

| Encounter / return | Accepted owners | Further boundary |
| --- | --- | --- |
| Battle29, exterior0 | [Opening](./battle29-initialization-and-opening.md), [activation](./battle29-activation-and-first-turn-dispatch.md), [outcomes/door](./battle29-outcomes-and-shrine-exterior-route.md). | Door scene and four exits differ; later admission selects30. |
| Battle30, interior1/Zalbard | [Opening](./battle30-initialization-and-opening.md), [activation/hidden enemy](./battle30-activation-and-first-turn-dispatch.md), [outcomes/Mitula](./battle30-outcomes-and-mitula-scene.md). | Ground Orb effects do not award inventory. No full Zalbard action audit. |
| Shrine1→Tristan→Pacalon | [Lore/search/Taya](./mitula-shrine-exploration-and-taya-recruitment.md), [return/Skreech](./mitula-return-and-skreech-recruitment.md), [Frayja/Battle31](./pacalon-return-frayja-and-battle31-admission.md). | Scene flag, joined/active membership and following differ. Skreech uses supplied rescue flag; Frayja follows before joining. |
| Battle31, Pacalon2→Battle32, Map74 | [31 opening](./battle31-initialization-and-opening.md), [activation](./battle31-activation-and-first-turn-dispatch.md), [outcomes/32 admission](./battle31-outcomes-and-battle32-admission.md). | Victory neither joins Frayja nor writes432. Later zone/scan is required; eligible27 can win first. |

## Moun, Jaro and the Nazca approach

[Battle32 opening](./battle32-initialization-and-opening.md), [Jaro side-change/first dispatch](./battle32-activation-jaro-and-first-turn-dispatch.md)
and [outcomes/Moun](./battle32-outcomes-and-moun-entry.md) separate the region
scene, enemy143/ally23 position transfer, later join and active-party removal.
Untouched fresh placements do not themselves reach the side-change.

[Battle33 opening/Frayja](./battle33-initialization-and-opening.md),
[activation](./battle33-activation-and-first-turn-dispatch.md) and
[outcomes/Zynk](./battle33-outcomes-and-zynk-handoff.md) lead conditionally to
Map31 exploration. [Moun/painter/Arm](./moun-town-painter-and-golem-arm.md) and
[underground39/Gyan/Zynk](./moun-underground-gyan-and-zynk.md) cover separate
interactions, awards and following. Neither rock scene nor underground following
recruits Zynk. Arm award is not Claude recruitment. [Moun→Nazca](./moun-to-nazca-and-battle34-admission.md)
retains population on west scroll; a later explicit55 request can select34.

## Nazca ship, optional Dojo and Roft

| Boundary | Accepted owners | Conditional or missing relation |
| --- | --- | --- |
| Battle34/Cameela, Map55 | [Opening](./battle34-initialization-and-opening.md), [activation](./battle34-activation-and-first-turn-dispatch.md), [outcomes/Sky Orb](./battle34-outcomes-and-sky-orb-handoff.md). | Inventory/discard differs from hidden scene Orb; return rebuilds current following. |
| Boarding37→voyage75/59→shootdown→crash37→75 | [Boarding/departure gate](./nazca-ship-boarding-and-departure-admission.md), [voyage](./nazca-voyage-and-map59-arrival.md), [shootdown](./nazca-ship-shootdown-and-crash-handoff.md), [post-crash return](./post-crash-ship-and-nazca-overworld-return.md). | Scene map changes and999/unlocks have separate boundaries. Native/special-entity limits prevent calling lowered prefixes playable voyage. |
| Optional Dojo45 | [Search/church/Sheela](./dojo-exploration-and-sheela-recruitment.md). | Optional75 branch, not proved mandatory to Roft/Grans. |
| Map75→76; Battle35 and separate Roft38 | [Route/admission/greeting](./nazca-to-roft-and-battle35-admission.md), [opening](./battle35-initialization-and-opening.md), [activation/Mud Men](./battle35-activation-and-first-turn-dispatch.md), [outcomes](./battle35-outcomes-and-roft-return.md). | Tables do not prove whether town or35 occurs first. Completed-repeat upgrades differ from first entry. |
| Roft→Galam region77 | [Petro/Zynk join](./roft-town-interactions-and-zynk-recruitment.md), [Dry Stone/route/36 admission](./roft-to-galam-and-battle36-admission.md). | Zynk's own gates do not require35 victory. Dry Stone boundary does not complete village/smithing; selected revisits are not exhaustive. |

## Grans return: Prism Flowers, Lemon and Geshp

| Encounter | Accepted owners | Limits on connection |
| --- | --- | --- |
| Battle36, Prism Flowers53 | [Opening](./battle36-initialization-and-opening.md), [activation/control](./battle36-activation-and-first-turn-dispatch.md), [laser/results](./battle36-prism-flower-laser.md), [outcomes](./battle36-outcomes-and-galam-return.md). | Laser is a deeper action slice, not every action.36 completion is not a predicate of the separate37 zone/coordinate row. |
| Battle37, Red Baron77 | [Opening](./battle37-initialization-and-opening.md), [double-turn/control](./battle37-activation-and-first-turn-dispatch.md), [outcomes/Lemon](./battle37-outcomes-and-lemon-aftermath.md). | Aftermath does not recruit Lemon. No full Red Baron action audit. |
| Map77→66; Battle38/Geshp | [Admission](./granseal-overworld-return-and-battle38-admission.md), [opening](./battle38-initialization-and-opening.md), [spawns/control](./battle38-activation-and-first-turn-dispatch.md), [Burst Rocks](./battle38-burst-rock-explosion.md), [outcomes](./battle38-outcomes-and-geshp-aftermath.md). | Earlier66 encounter rows can win. Rock construction/HP restoration is not observed playback/all Geshp actions. Early engine66 selection does not cover late content. |

## Force Sword, Devils Head and the final tower

[Battle39 opening/control](./battle39-opening-and-first-control.md) and
[outcomes/Force Sword](./battle39-outcomes-and-force-sword-route.md) cover
Map66→shrine42/head41, independent attraction/extraction, selected sword use and
conditional Lemon recruitment. The Map41 entrance is accepted; other recruitment
entrances do not become complete. [Labyrinth28/40 admission](./devils-head-labyrinth-and-battle40-admission.md)
separates facing scripts, live copies and outgoing54 request; no persistent
puzzle-completion predicate is inferred.

| Final segment | Accepted owners | Endpoint / gap |
| --- | --- | --- |
| Battle40/Odd Eye,54 | [Opening](./battle40-initialization-and-opening.md), [regions/terrain/control](./battle40-regions-terrain-and-first-control.md), [outcomes](./battle40-outcomes-and-odd-eye-aftermath.md). | Conditional drops/pre-scene seed; exits28/56. No full Odd Eye action repertoire. |
| Tower56/Battle41 | [Ascent/Creed/entry](./ancient-tower-ascent-creed-and-battle41-admission.md), [opening/activation/control](./battle41-opening-activation-and-first-control.md), [outcomes/stairs60](./battle41-outcomes-and-ancient-tower-stairway.md). | Creed talk alone does not unlock41. CURRENT retains state;999 substitutes58 request with59. |
| Battle42/Galam,59 | [Opening](./battle42-initialization-and-opening.md), [hidden activation/control](./battle42-activation-and-first-control.md), [outcomes/43 admission](./battle42-outcomes-and-zeon-admission.md). | Elis aliases differ. After42 sets443; only later scan selects43, with42 first. |
| Battle43/Zeon,59 | [Initialization](./battle43-initialization.md), [activation/control](./battle43-activation-and-first-control.md), [Guard laser](./battle43-zeon-guard-laser.md), [outcomes/ending](./battle43-outcomes-and-ending-admission.md). | No full Zeon/boss action/item trajectory, new runtime or source-ROM agreement. Ending admission is not completed ending/free walking. |

## Epilogue, credits and the hidden battle

[Zeon aftermath/sealing/return](./zeon-aftermath-and-granseal-return.md) organizes
the admitted scene through shared tail, party reset and posted two-years-later
Granseal3 request. Ordinary543 init returns; posted request and earlier scene
loads differ. [Town/Astral escort](./granseal-epilogue-and-castle-admission.md),
[council/chamber admission](./granseal-royal-council-and-chamber-admission.md) and
[chamber/kiss](./granseal-chamber-interactions-and-ending-kiss.md) cover optional
hints, Yes/No, retained actors versus future populations, departures and credits
call. Minister hints are not the kiss prompt's tested prerequisites.

[Credits/finale/WitchEnd/EndGame](./ending-credits-finale-and-bonus-admission.md)
continues through completion storage and real Battle0 admission. [Battle0](./battle00-bonus-battle-and-reset.md)
separates Map63's caller from MainLoop: actual victory/defeat/admitted Egress or
Wing returns lead to start-vector reset, not exploration. Ordinary Suspend is
rejected to the menu before prompt, SaveGame,88 or WitchSuspend. Software reset
does not prove hardware persistence, resulting menus or saved later outcomes.

These source accounts close the named **reading spine's ending boundary** with
omissions below still explicit. They do not prove naturally completed ending,
playable Godot epilogue, all boss moves or Epic1053 acceptance.

## Encounter omission check

**Confirmed static inventory:** [spritesets](./battle-spriteset-data.md),
[routing/terrain](./battle-routing-data.md), [global tables](./battle-global-data.md)
and [cutscene data](./battle-cutscene-data.md) own built table domains. Enum and
coordinate table have slots0–44; cutscene routing has a different larger domain.
Structural presence/empty hooks are not content acceptance. This check maps
**0–43** to the chapters and44 to its separate optional trial account. It does not
count documents as completion units.

| Slots | Content-account boundary |
| --- | --- |
| 0 | Hidden chapter: selected formation/control/laser/actual returns. Other boss actions remain outside it. |
| 1 | Accepted savestate-linked original route/admission, selected winning actions, victory/after-program/stable return and one subsequent Down input/effect; [current original frontier](../design/synthesis/map3-battle01-readiness.md#accepted-original-frontier). All action branches, natural New/load, uninterrupted execution and hardware/full-presentation claims remain outside that bounded acceptance. |
| 2–7 | Early content/outcome/admission; not complete encounter-specific initialization/activation/actions. |
| 8–13 | Continent/eastern selected admissions/openings/outcomes/local content; not full tactical processing per encounter. |
| 14 | [Southeast Desert account](./battle14-desert-encounter-and-outcomes.md): Map70 request/cooldown and10-before14 scan, complete formation/empty hooks, conditional repeat upgrades, region/control predicates, Power Stick conditions and outcomes/returns/exits. Natural reach, unsafe airborne scan results, all tactics and production14 remain outside its static boundary. |
| 15 | [Shrine account](./battle15-shrine-approach-and-outcomes.md):69/61 approach, full exterior population/opening, both50 admissions, complete scenes/formation/control boundary, hidden Golem/chest/drop conditions, outcomes and three exits. Natural paths, all tactics and implementation remain outside its static boundary; [Panel/Sword custody](./ribble-tree-opening-and-achilles-sword.md) retains its separate scope. |
| 16–25 | Southern/Creed/North Cliff selected fresh initialization/outcomes/map links; not every region/first-control/action path. |
| 26–27 | Separate opening/init, activation/first-call and outcome owners; first call is not an executed individual action. |
| 28 | [Bridge47 account](./battle28-bridge-and-outcomes.md): both74 entrances, full before/after programs, live formation/sixteen enemies/neutral alias, first dispatch/control predicates, conditional terminal/escape/Suspend and retained/fresh returns. All tactical choices, natural traversal and implementation remain outside its static boundary; Tristan/Caves still enters0/29 independently. |
| 29–43 | Late entry/opening, activation or first-control and outcome owners. Laser/Burst Rock exceptions do not establish all-AI coverage. |
| 44, outside0–43 | [Fairy Woods trial](./battle44-fairy-woods-trial-and-records.md):23 east exit/24 invitation, complete12/15/14/1 declaration, hidden/respawn/leader terminal, timer/snapshot/record, chest availability and conditional returns. Distinct from Fairy Cave32; natural play, all tactics, persistence and production44 remain outside its static boundary. |

### Direct evidence for encounter gaps and the bridge branch

This bounded source check supplements accepted owners. The dedicated28 owner
fills its declared static content boundary; the dedicated15/14 and optional44 owners
do likewise. It does not
classify natural order/mandatory status. Actual records/producers at
the pin, rather than comment names alone, establish the following:

- **14:** `sf2enums.asm:BATTLE_SOUTHEAST_DESERT=14` and
  `data/battles/global/battlemapcoords.asm:table_BattleMapCoordinates` row14→70.
  `data/maps/entries/map70/mapsetups/s3_zoneevents.asm` routes X16–19/wildcard-Y
  to `Map70_ZoneEvent0`, supplying14 to `CheckRandomBattle`. Default10 producer
  and ordered scan are independent; no14-before15/16 sequence is derived. The
  [dedicated desert owner](./battle14-desert-encounter-and-outcomes.md) now connects
  base/substituted formation, empty hooks, control, actual held-item drop and returns.
  The airborne count0 scan defect remains explicit without an invented safe fallback.
- **15:** row15→50; enum `BATTLE_SHRINE_SOUTH_OF_RIBBLE`.
  `data/maps/entries/map61/mapsetups/s3_zoneevents.asm:Map61_ZoneEvent0`
  admits `scripts.asm:cs_5C6CA` with clear730/held77. Its `setStoryFlag 15`
  precedes final follows/end; caller sets730 only on return. Before/after tables
  select `bbcs_15` / `abcs_battle15` in `data/battles/entries/battle15/`.
  Map50's no-scroll return to61 and selected chest/tree region are separate
  records. The [dedicated shrine owner](./battle15-shrine-approach-and-outcomes.md)
  now organizes the full approach/opening, scenes, battle data/control boundary,
  hidden143 and outcomes/exits. No naturally walked connection among Map50's
  storage regions, complete AI or original story-runtime observation is added.
- **28:** row28→47; enum `BATTLE_PANGOAT_VALLEY_BRIDGE`.
  `data/maps/entries/map74/mapsetups/s3_zoneevents.asm:Map74_ZoneEvent0`
  with527 clear sets427, posts reload, then sets428/429/430. Its warp table
  separately declares two47 entrances;47 declares two no-scroll returns to74.
  Before/after tables select `bbcs_28` / `abcs_battle28` in
  `data/battles/entries/battle28/`. These records do not say27 victory runs28,
  or that28 is required for35→0. The [dedicated bridge owner](./battle28-bridge-and-outcomes.md)
  now organizes the complete scenes, battle data/control boundary and outcome
  effects, including the actual CURRENT return caller. That is static content
  coverage, not a new runtime observation or all-AI account.

For44, `sf2enums.asm:BATTLE_FAIRY_WOODS=44`, row44→24 and Map24 init are now
joined by the [complete optional-trial owner](./battle44-fairy-woods-trial-and-records.md).
It separates invitation wording from the actual leader sweep, seconds from callback
snapshot/record, declared chests from the USA stride5 lookup, and fresh/retry/
Suspend/outcome callers. This is static coverage, not natural or playable completion.

## Optional content and branches

Status describes the **content account**, not a gameplay completion flag.
**Confirmed** accepted means a bounded owner exists; partial means a named
adjacent question is outside scope. Absence is relative to this reviewed owner
set, not proof that no source or generic helper evidence exists.

| Branch | Current account | Missing or intentionally unreviewed boundary |
| --- | --- | --- |
| Opening extras, May, East Shrine, Bedoe demonstration/rescue, Polca choices | Accepted bounded chapter owners. | Every NPC/item/itinerary and all future consequences are not exhaustively reviewed. Independent gates must not become a mandatory quest chain. |
| Panel/tree/Sword and shrine approach | Accepted bounded [custody/tree](./ribble-tree-opening-and-achilles-sword.md) and [61/15 chapter](./battle15-shrine-approach-and-outcomes.md), with Rohde's possession predicate separate. |50 storage regions are not a proven continuous route; complete tactics, natural actor/service completion and production15 remain outside the static account. |
| Rick, Elric, Taya, Skreech, Janet, Rohde, Higins, Jaro, Frayja, Gyan, Zynk, Sheela | Accepted selected joining/following/capacity scenes. | Not all-recruits/all-entries census. Skreech rescue and Jaro/Frayja multi-stage transitions retain conditions; membership/following differ. |
| Creed choices/later alternatives | Partial: [recruitment owner](./creed-return-fairy-and-recruitment-choices.md) covers initial/F22 offers, refusal and actual join/follow paths for four recruits. | Complete later journey, every setup and ability to recruit everyone on a naturally reached save are not established. |
| Dwarven Village29/30 and smith | Partial: earlier30 recovery/Cannon accepted. [Menus](./common-menus.md#static-service-menu-state-machines), [item auxiliary data](./core-stats-data-inventory.md#item-auxiliary-catalog) and service H3 own helper/order/fulfillment seams. | Later village access/setup/interactions, local smith caller and full player service experience are not an accepted connected account. Helper H3 is not village traversal. Source29's tail to `j_BlacksmithMenu` identifies an entry, not completed service. |
| Arm/Claude and later Yeel/Chaz | Partial: [Moun Arm award](./moun-town-painter-and-golem-arm.md) is accepted but stops before later Golem recruitment/item use. | Separate Map63 item-event/script identifies the Arm/Claude branch; late Yeel530/28 setups declare Chaz. Neither supplies a full accepted optional-route account here. Arm acquisition alone does not establish Claude joining. |
| Lemon | Partial across [Red Baron aftermath](./battle37-outcomes-and-lemon-aftermath.md) and [Map41 sword/head entry](./battle39-outcomes-and-force-sword-route.md). | After37 does not recruit;41 conditional join is accepted. Other entrances remain unorganized by that one owner. |
| Dojo45 and hidden Battle0 | Accepted bounded chapter owners. | Optional Dojo and special EndGame/reset caller do not close other optional encounters. |
| Battle14 | Dedicated [static encounter/repeat/outcome account](./battle14-desert-encounter-and-outcomes.md). | Natural order/state, actual airborne overread results, full tactics/runtime and production consumption remain unproved. |
| Fairy Woods44 | Dedicated [static trial/records account](./battle44-fairy-woods-trial-and-records.md). | Complete declared formation/regions and hidden/respawn/leader/timer/result/return consumers; natural play, all tactics, chest custody, exact presentation/time and persistence remain unproved. |
| Battle28 bridge branch | Dedicated [static entry/scenes/formation/control/outcome/return account](./battle28-bridge-and-outcomes.md). | All action/AI decisions, natural traversal and late production consumption remain unproved; no physical collapse/countdown inferred from staging. |
| Late town revisits, shop/church/Caravan, promotion/equipment, magic tunnels | Selected alternatives/services/warps where linked; global [menus](./common-menus.md), [map content](./map-content.md), [search](./field-search-control.md), [item effects](./field-item-effects.md) retain mechanisms. | No exhaustive late-state NPC/chest/tunnel/service-UI/persistence census. All-map parsing and helpers do not close these experiences. |

The29 smith entry and Chaz declarations use pinned
`data/maps/entries/map29/mapsetups/s2_entityevents.asm` and
`data/maps/entries/map05/mapsetups/s1_entities_530.asm` / `s1_entities_28.asm`.
The separate Claude identity uses Map63's `mapsetups/s5_itemevents.asm:Map63_ItemEvent0` →
`scripts.asm:cs_5CBB4`, which contains `join ALLY_CLAUDE`. Only these entry/
declaration/join identities are used; no complete optional-route research is claimed.

## Cross-chapter consumption gaps

**Confirmed by accepted production-code inspection:** these recurring categories
can guide a concrete consumer task; they do not authorize all later content or
import allowlist expansion now.

| Category | Existing expression / actual boundary | Representative owner |
| --- | --- | --- |
| Directly expressible primitives | Typed flag branches, text/portrait, calls/returns, position/follow, scene/reload and queued warps exist. Each needs admitted content, aliases and correct caller lifetime. | [Application](../../remake/docs/application/session-and-programs.md), [Content](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [after00 audit](./battle00-bonus-battle-and-reset.md#actual-producer-and-production-consumers). Types are not execution of later scenes. |
| Conversion/selection | Map/setup/events, formation, refreshed stats, equipment metadata and live selectors need trusted conversion. `prepare_encounter` admits2/3; scenario region-program assembly admits1/2/3. | [Producer](../../src/sf2tool/remake_exploration_battles.py), [reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [third encounter](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter). Exports are not full-game production bindings. |
| Native / Unsupported | `native-call` becomes `UnsupportedInstruction`; runner rejects before advancing/committing that instruction, preserving earlier committed prefixes. Native actions also have explicit unsupported form. | [Reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [voyage](./nazca-voyage-and-map59-arrival.md), [aftermath audit](./zeon-aftermath-and-granseal-return.md#actual-producer-and-current-consumers). Typed suffixes cannot bypass native setBlocks/setDest/mapLoad/special effects or partial compiler failure. |
| Battle rules/return caller | Current physical/heal/item/AI subsets do not supply all late spells, loadouts, SENTRY8, lasers, explosions or bosses. Generic outcomes do not implement EndGame reset. | [Actual consumer limits](./battle00-bonus-battle-and-reset.md#actual-producer-and-production-consumers), [Guard laser](./battle43-zeon-guard-laser.md), [Burst Rock](./battle38-burst-rock-explosion.md). Static mechanism is not implemented encounter. |
| Scene/service/persistence | Retained/rebuilt actors, request publication, real branch/wait completion, status refresh, service UI/save require actual consumers. Generic projection is insufficient. | [Epilogue](./granseal-epilogue-and-castle-admission.md#actual-content-importer-and-runtime-consumers), [council](./granseal-royal-council-and-chamber-admission.md), [kiss](./granseal-chamber-interactions-and-ending-kiss.md), [deferred surfaces](../../remake/docs/capability-status.md#milestone-and-deferred-surfaces). |

No source lowering, historical recipe replay, material package or engine/adapter run was used.
Linked audits retain failed/partial compilation and native boundaries, including
HP/choice/rotation/portrait/ending limits. Research to ending does not change
playable scope.

## Unknowns, retained failures and next decision

**Unknown:** complete natural original path/admission/caller state across
chapters, supplied party/HP/flags/RNG, service/wait/audio/presentation completion,
collision routes, hardware timing and cross-process/physical SRAM persistence.
Helper/runtime evidence proves only its seam. Questions stay at linked owners
under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md); no automatic
emulator/capture/comparison/per-NPC closure queue is created.

The [verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03)
retains #1047's **twelve completed unresolved comparisons**: nine old-start
full-fade-state cases, selected seed, first actor and activation rejection.
[Retained comparisons](../../remake/docs/evidence/retained-comparisons.md) and
chapter owners preserve older failures/Unknowns. They are not interrupted runs,
passes or a repair queue. Battle0's accepted Suspend correction preserves the
original author/main/CI PASS that missed that exclusion; the focused correction
does not validate unrelated source content.

Battle28's [dedicated account](./battle28-bridge-and-outcomes.md) now spans the
whole declared static branch, through conditional outcomes, full Cameela scene
and both return connections. It does not prove that the chapter spine must traverse28 or that
the later campaign is playable, and creates no all-AI/runtime closure queue.

Battle15's [dedicated account](./battle15-shrine-approach-and-outcomes.md) closes
the named external61/15 static content gap while preserving separate Panel/Sword
custody, natural/runtime, design and remake boundaries.

The [dedicated Battle14 account](./battle14-desert-encounter-and-outcomes.md) closes
the named connected static encounter gap, including conditional repeat upgrade
and held-equipment drop limits; it adds no natural/runtime or playable acceptance.

The [Fairy Woods44 account](./battle44-fairy-woods-trial-and-records.md) closes the
named connected static trial gap without adding runtime/design/playable acceptance.

**Recommendation, not dispatch:** dwarf village/smith and later revisits remain
optional-content decisions. These are alternatives for
main-gate planning, not a numbered queue or Epic close decision.

## Verification boundary

Read complete root README, documentation index and source-coverage, then relevant
complete sections of chapter owners, actual table/producer records and production
owners for asserted relationships. Check negative claims against declared scope
as well as positive references. Filenames/keywords alone cannot prove coverage.

Direct checks cover links/anchors, UTF-8/LF, exact three-path scope, unchanged
fixture-counter table, finite encounter records/producers, consumer Git ancestry
and private boundary. The ignored recipe and semantic claim-to-owner review are
provided in the Issue handoff with full dependencies; use a fresh receipt name.
On clean committed head load existing private config in the same process, then
run `uv run sf2 verify plan --base origin/main --head HEAD` and inspect actual
exact-head scoped CI. Normal/full, SDK/Godot/emulator/H3/H4, screenshots, old
recipes/comparisons/lowering and acquisition are outside this documentation
acceptance. Mechanical PASS is not whole-game coverage or independent acceptance.
