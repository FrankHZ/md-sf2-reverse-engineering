# Story Content Coverage

Evidence date: 2026-10-23. **Confirmed:** accepted research owners describe a
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
[Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
including the bounded [remaining-gap assessment](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1212).
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
| Battle01 return; exorcism/commission, Jaha; Battle02/Map66 | [Return through Battle02](./battle01-to-battle02-content-route.md), [Battle02 initialization/first dispatch](./battle02-initialization-and-first-control.md). | Conditional unlock and separate admission; complete selected static initialization/control account, not a naturally walked continuation or all-action audit. |
| Battle02 return; Yeel5/Kazin; Battle03/Map66 | [Return through Battle03](./battle02-to-battle03-content-route.md), [Battle03 initialization/first dispatch](./battle03-initialization-and-first-control.md). | Yeel guidance does not write the unlock and is not a proved prerequisite. Following is not Kazin recruitment. The selected static account separates region0/15 activation, masked bat item and completed-repeat upgrades; it is not a natural arrival or all-action observation. |
| Battle03 return; Hawel house43; Battle04 | [Hawel-house route](./battle03-to-battle04-content-route.md), [Battle04 opening/initialization/first dispatch](./battle04-initialization-and-first-control.md). | House scene recruits Kazin/writes404; Battle03's empty after-script does neither. House admission does not require proved Battle03 completion. Battle04 separates scene actors, five templates and actual party; fresh placement triggers no region. Capture dialogue does not prove forced defeat. |
| Battle04 outcomes; Galam17/16; Battle05 | [Captivity/guard admission](./battle04-to-battle05-content-route.md), [Battle05 initialization/first dispatch](./battle05-initialization-and-first-control.md). | Victory and special defeat have different consumers; escape differs. Battle05 separates six templates from actual membership, empty hooks from the field arrest, region0 standby from five15/15 enemies, and SENTRY commands from stored follow orders. No unavoidable defeat or Slade participation claim. |
| Battle05 outcomes; Galam exits18/66; Battle06 | [Jewel/Slade/escape](./battle05-to-battle06-content-route.md), [Battle06 opening/initialization/first dispatch](./battle06-opening-and-first-control.md). | Joining, current following and returned requests differ. Battle06 joins6/9/1/0 to the failed package scene, HEALER3/live-follow129, NONE versus special-order standby and conditional control. Before supplies407; no natural arrival or all-action claim. |
| Battle06 return; Granseal3/19, tower58; Battle07 | [Occupied town, optional Kiwi, royal following](./battle06-to-battle07-content-route.md), [complete Battle07 opening/initialization/first dispatch](./battle07-opening-and-first-control.md), then [Battle07 aftermath](./battle07-outcomes-and-aftermath-handoff.md). | Earlier before-program supplies407; victory supplies506. Seven templates do not require Kiwi. Before actors, ten enemies and five completion-gated neutrals are separate populations. After07 posts a return before later follower/flag work. |

Battles02–13 now have their own complete selected static initialization, activation and
first-dispatch accounts. Their natural-state, all-action and production limits
remain separate from that selected static completeness.

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
conditionally to Battle08/Map72. Its [initialization/upgrades/first dispatch](./battle08-initialization-and-first-control.md)
connects13/11/4/1 to actual party/capacity, F508 type selection, sloping regions
and conditional control. Thirteen templates do not recruit thirteen allies or
require Kiwi; the prone worker is a separate completion-gated HP0 neutral. No
fresh region triggers, fixed first actor or production8 support is implied.
[Outcomes/construction](./battle08-outcomes-through-new-granseal-handoff.md)
lead through Map73 to the separate [year-later castle7 init](./new-granseal-castle-year-later-control.md).
[Town6 introductions](./new-granseal-town-through-peter-introduction.md) and
[Peter's castle audience](./peter-castle-audience-through-town-return.md) cover
independent scene gates, force/AI/follower effects and town return. The spoken
gold gift is not a confirmed increase. No chapter label makes the year
transition, map handoff or first control read an observed runtime.

## Eastern Parmecia: Ribble, Polca and Bedoe

| Segment and encounters | Organized boundary | Conditional or optional relation |
| --- | --- | --- |
| New Granseal→Map73/72/69; Battle09 | [Departure/admission](./new-granseal-departure-through-battle09.md), [initialization/upgrades/first dispatch](./battle09-initialization-and-first-control.md), [outcomes/Ribble8 welcome](./battle09-outcomes-through-ribble-welcome.md). | Scroll does not scan combat or rerun init. Twelve templates and F67 follower do not establish Peter's active/AI state or a fixed party. Both fresh regions miss; F509 upgrades retain the airborne defect. Victory does not itself enter Ribble. |
| Ribble→Map70; Battle10 | [Optional May/admission](./ribble-may-through-battle10-admission.md), [initialization/first dispatch/Power Ring](./battle10-initialization-and-first-control.md), [outcomes/East Shrine62](./battle10-outcomes-through-east-shrine-visit.md). | May's invitation is linear, not Yes/No; F8/F40/HP/AI/following remain separate. Three fresh regions miss; F510 upgrades retain the airborne defect. Ring129 is masked unequipped at successful AddItem; conditional action drop uses actual item19/bit4 and RARE deals. Optional shrine/following is not a proved Battle11 prerequisite. The separate Battle14 random encounter is organized below. |
| Shrine return/Map69/72→cave48; Battle11 | [Cave admission](./east-shrine-return-through-battle11-admission.md), [initialization/regions/first control](./battle11-initialization-and-first-control.md), [outcomes/Polca13](./battle11-outcomes-through-polca-arrival.md). | Complete12/10/4/1 is separate from scene population/Peter membership. No11 repeat substitution or special drop; four fresh regions miss, three15/15 enemies bypass standby. Fixed point0, live128 and ally0 differ; set6 lacks MOVE_ORDER and set7 NONE falls through. Darkness/presentation and crossing remain Unknown; separate Polca entry builds Oddler's scene. |
| Polca→Volcano52; Battle12 | [Optional turtle/east departure](./polca-fairy-through-battle12-admission.md), [initialization/dual regions/first control](./battle12-initialization-and-first-control.md), [outcomes/Bedoe10](./battle12-outcomes-through-bedoe-entry.md). | Neither turtle nor first Oddler scene is required by the exit. Complete12/12/4/1 separates actual party from scene/templates. No repeat substitution/drop; four fresh regions miss, dual-region primary-first/sticky activation differs from NONE standby movement. Fixed point0/live128 and sets0/2/6/7/14 reach conditional control; HEALER3 is not HEAL3. |
| Bedoe10→Volcanon11→Bedoe | [King address/audience](./bedoe-king-address-through-first-audience.md), [Volcanon/Peter](./bedoe-to-volcanon-audience.md), [king choice/Luke](./volcanon-return-through-bedoe-king-choice.md), [hatchling/rescue](./bedoe-hatchling-flight-and-rescue.md). | Address/audience gates are independent; invitation is not shrine admission. Demonstration is not rescue admission; rescue is not Skreech recruitment. |
| Bedoe return→Polca; Battle13 | [Confrontation/Gerhalt](./bedoe-return-through-battle13-admission.md), [initialization/party/neutral/first control](./battle13-initialization-and-first-control.md), [outcomes/raft grant](./battle13-outcomes-through-polca-raft-grant.md), [Oddler choices](./polca-oddler-choice-and-following.md), [Luke recruitment](./polca-departure-through-luke-recruitment.md). | Complete12/16/3/1 separates actual Gerhalt participation, prior Peter AI-bit clear and F513-gated HP0 neutral159. Same-region2/2 is134/136 following live135; fresh regions miss. Outcome installs Luke following, not joining; later init joins him. Mayor, Oddler requests and departure have independent gates. |

## Southern Parmecia: transport, the sword and Taros

[Raft boarding/southern exits](./polca-raft-boarding-through-south-parmecia-entry.md)
separate movement request, alignment, boarding completion, mode write and
scroll/no-scroll. [Kraken/Battle16 entry](./south-parmecia-through-kraken-battle16-entry.md)
on49 uses416 independently of raft mode. [Initialization/SWARM/first control](./battle16-initialization-and-first-control.md)
connects12/11/0/0, exact HP-only128–138 counting, live arm orders and conditional
control; the complete intro remains at its entry owner. [Outcomes/disembarking/Hassan9](./battle16-outcomes-through-hassan-entry.md)
separate victory transport, defeat/Egress savepoints and town population.
The [Battle14 desert account](./battle14-desert-encounter-and-outcomes.md) covers Map70 requests,
ordered10-before14 admission, base formation/conditional repeat upgrades, control, drop and returns.
The separate [Battle15 shrine account](./battle15-shrine-approach-and-outcomes.md)
does not make either encounter an implicitly completed predecessor of16.

| Branch / onward encounter | Accepted owner and boundary | Remaining connection |
| --- | --- | --- |
| Hassan/Rohde; Ribble8/tree, separate Map50 chest regions | [Possession/choice caller](./hassan-ancestry-and-rohde-caravan-choice.md), [Panel/tree/Achilles Sword](./ribble-tree-opening-and-achilles-sword.md), [exterior61/Battle15](./battle15-shrine-approach-and-outcomes.md). | Recognition neither acquires nor consumes the sword; acceptance neither joins Rohde nor acquires Caravan. The separate shrine account does not prove a walk between50's regions. |
| Hassan→Map69; Battle17 | [Departure/shared outcomes](./hassan-departure-and-battle17-outcomes.md), [initialization/first control](./battle17-initialization-and-first-control.md). | Ordered9-before17,417 versus418, crop/local positions and empty hooks retain separate consumers. Complete12/14/2/0, F517 upgrades, actual live follows and two fresh region misses reach conditional control. |
| Map69→68→Taros12; Battle18 | [Entry/warning/opening](./southwest-parmecia-through-taros-battle18-entry.md), [initialization/equipment/first control](./battle18-initialization-and-first-control.md), [outcomes/Caravan](./battle18-outcomes-and-caravan-acquisition.md). | Admission precedes shrine init. Complete12/14/5/0 separates actual party, neutral159/lookup31 and scene actors; five fresh region misses, Taros's absent-point read, item-use and Bowie-equipped61 gate remain conditional. Vehicle actor, drop, flags and return seed differ; not every tactic or a winning trajectory. |
| Optional Rick; Caravan toward19 | [Rick](./hassan-rick-recruitment.md), [boarding/travel/landing](./caravan-boarding-and-southwest-parmecia-travel.md). | Rick is separate optional interaction. Caravan mode lacks walking's19 zone producer; landing and later request are separate. |

## Elven Village, Harpy Pond and Fairy Cave

[Battle19 initialization/first control](./battle19-initialization-and-first-control.md)
connects12/15/3/1, F519 repeat selection, three fresh region misses, actual live
orders and HEAL3/Shining Ball custody to conditional control.
[Battle19 outcomes/Elven Village23](./battle19-outcomes-through-elven-village-entry.md)
retain no mandatory victory-before-village predicate. [Village conversations/search](./elven-village-conversations-and-local-exploration.md)
organize items/descriptions and current-map stairs. [Departure/Harpy Pond51](./elven-village-departure-through-harpy-pond-entry.md)
connect selected exits/scrolling to help scenes without a compulsory village
itinerary. [Approach/Battle20](./harpy-pond-approach-through-battle20-entry.md)
connects to [initialization/SWARM/first control](./battle20-initialization-and-first-control.md):
complete12/12/0/0, HP-only128–138 count omitting139, three thresholds3/2/1,
live orders, actual DISPEL/HEAL filters and neutral159/lookup31 versus reader63.
The separate [outcomes/Elric](./battle20-outcomes-and-elric-recruitment.md) distinguish
admission, physical neutral, force join and lost transient following.

[Elric's passage scene](./harpy-pond-departure-through-fairy-cave-entry.md)
conditionally changes live blocks and requests Fairy Cave32. [Rewards/returns/rock](./fairy-cave-exploration-and-devils-tail-return.md)
have separate consumers; the hint does not open the passage. Void setup still
constructs ordinary actors. Battle44/Fairy Woods on24 is a different optional
encounter, outside this account.

## Creed's small worlds and the consultation

| Chapter boundary | Accepted content owners | Connection or omission |
| --- | --- | --- |
| Devil's Tail67/Battle21→Mansion33 | [Opening/initial roster](./battle21-opening-and-initial-roster.md), [regions/upgrades/items/first control](./battle21-initialization-and-first-control.md), [outcomes](./battle21-outcomes-and-devils-tail-return.md), [Goliath obstruction](./creed-mansion-entry-and-first-goliath-scene.md). | Entry does not automatically run Goliath's zone scene. |
| Magic door→Desktop22/Battle22 | [Shrinking/transfer](./creed-magic-door-and-desktop-transfer.md), [arrival](./desktop-kingdom-first-arrival.md), [King permission](./desktop-kingdom-interactions-through-battle22-entry.md), [initialization/first control](./battle22-initialization-and-first-control.md), [outcomes](./battle22-outcomes-and-desktop-return.md). | Door gate is independent of earlier Goliath flag. Reward announcement is not Balloon acquisition. Nine SWARM actors, truncated HP count, valid King point0 and dual neutrals reach conditional control; natural/all-action claims remain outside it. |
| Balloon→Floor World26→Burrow27/Battle23 | [Acquisition/use/departure](./cotton-balloon-through-floor-world-entry.md), [landing](./floor-world-first-arrival.md), [interactions/exits](./floor-world-interactions-and-exits.md), [Burrow/admission](./willard-burrow-through-battle23-entry.md), [initial roster](./battle23-initialization-and-roster.md), [activation/AQUA2/first control](./battle23-initialization-and-first-control.md), [outcomes](./battle23-outcomes-and-burrow-return.md). | Chest flag, custody, own-tile use and force-wide removal differ. Ring scene is not an exit prerequisite. |
| Restoration→Evil Spirit34→return | [Restoration](./creed-restoration-and-basement-departure.md), [shrine entry](./evil-spirit-shrine-entry-and-scene-admission.md), [consultation](./evil-spirit-consultation-and-zone-return.md), [Fairy/recruit offers](./creed-return-fairy-and-recruitment-choices.md), [Oddler stays](./oddler-stays-and-fairy-handoff.md). | Current actors and future populations differ. Eight offers cover named initial/F22 alternatives, not every natural return itinerary. Narration/lore is not independently established world fact. |

## Returning for the Cannon and opening North Cliff

[Mine30/Fairy recovery/Cannon](./creed-departure-dwarf-recovery-and-cannon-gift.md)
is an accepted **bounded part** of dwarf content. Later Village29 and smithing
have a separate [static lifecycle](./dwarven-village-and-mithril-smithing.md) via77.
[Astral's return audience](./new-granseal-return-and-astral-following.md),
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
| Roft→Galam region77 | [Petro/Zynk join](./roft-town-interactions-and-zynk-recruitment.md), [Dry Stone/route/36 admission](./roft-to-galam-and-battle36-admission.md). | Zynk's own gates do not require35 victory. [Village29/smith lifecycle](./dwarven-village-and-mithril-smithing.md) continues the declared77 entrance; natural traversal and exhaustive revisits remain unproved. |

## Grans return: Prism Flowers, Lemon and Geshp

| Encounter | Accepted owners | Limits on connection |
| --- | --- | --- |
| Battle36, Prism Flowers53 | [Opening](./battle36-initialization-and-opening.md), [activation/control](./battle36-activation-and-first-turn-dispatch.md), [laser/results](./battle36-prism-flower-laser.md), [outcomes](./battle36-outcomes-and-galam-return.md). | Laser is a deeper action slice, not every action.36 completion is not a predicate of the separate37 zone/coordinate row. |
| Battle37, Red Baron77 | [Opening](./battle37-initialization-and-opening.md), [double-turn/control](./battle37-activation-and-first-turn-dispatch.md), [outcomes/Lemon](./battle37-outcomes-and-lemon-aftermath.md). | Aftermath does not recruit Lemon. No full Red Baron action audit. |
| Map77→66; Battle38/Geshp | [Admission](./granseal-overworld-return-and-battle38-admission.md), [opening](./battle38-initialization-and-opening.md), [spawns/control](./battle38-activation-and-first-turn-dispatch.md), [Burst Rocks](./battle38-burst-rock-explosion.md), [outcomes](./battle38-outcomes-and-geshp-aftermath.md). | Earlier66 encounter rows can win. Rock construction/HP restoration is not observed playback/all Geshp actions. Early engine66 selection does not cover late content. |
| Late Yeel05, Chaz and Lemon | [Complete selected chapter](./late-yeel-chaz-and-lemon-recruitment.md) covers both66 entrances, late residents/search, piano/stairs, both930 story branches, distinct joins and return/revisit. | Static coverage only; membership, active party, actor pose and following differ. Natural terrain/service/save and late production bindings remain unproved. |

## Force Sword, Devils Head and the final tower

[Battle39 opening/control](./battle39-opening-and-first-control.md) and
[outcomes/Force Sword](./battle39-outcomes-and-force-sword-route.md) cover
Map66→shrine42/head41, independent attraction/extraction, selected sword use and
conditional Lemon recruitment. Map41 and [late Yeel05](./late-yeel-chaz-and-lemon-recruitment.md)
retain separate recruitment predicates; these owners do not establish all entrances.
[Labyrinth28/40 admission](./devils-head-labyrinth-and-battle40-admission.md)
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
| 2 | [Initialization/first dispatch](./battle02-initialization-and-first-control.md) joins the complete4/6/0/1 declaration to fresh/suspended entry, changed joined/active/dead/capacity state, enemy refresh, no-region AI and randomized control routing. Existing entrance/outcome owners retain conditional links; natural arrival/first actor, all actions and general production-party equivalence remain unproved. |
| 3 | [Initialization/first dispatch](./battle03-initialization-and-first-control.md) joins12/6/1/0 formation, conditional Yeel EGRESS, region0 versus15/15, masked bat item and503 repeat upgrades to the actual consumers. Natural state/first actor, unsafe airborne scan results, all actions and general production equivalence remain unproved. |
| 4 | [Opening/initialization/first dispatch](./battle04-initialization-and-first-control.md) joins5/8/3/1, complete before scene/text keys, non-upgradable definitions/equipment, three regions and point/follow orders to consumers. No fresh initial trigger; two15/15 enemies bypass standby. Natural state/first actor and all actions remain unproved; current conversion does not admit4. |
| 5 | [Initialization/first dispatch](./battle05-initialization-and-first-control.md) joins6/8/1/1, empty hooks, actual party, equipped enemies/BLAZE1, region0 standby and five15/15 enemies with distinct SENTRY/follow bindings to consumers. No fresh initial trigger; natural state/first actor and all actions remain unproved; current conversion does not admit5. |
| 6 | [Opening/initialization/first dispatch](./battle06-opening-and-first-control.md) joins6/9/1/0, complete guard scene/text, actual party, Dark Cleric HEAL1/HEALER3/live-follow129, equipped Galam enemies, initially untriggered region0 and distinct NONE/special-order standby to consumers. No repeat upgrade; natural state/first actor/all actions remain unproved; current conversion excludes6. |
| 7 | [Opening/initialization/first dispatch](./battle07-opening-and-first-control.md) joins7/10/4/1, complete confrontation/text, actual party, five F507-gated neutrals, Smoke extra turns/DISPEL, equipped Galam enemies, initially untriggered regions and distinct standby/live-follow routing to consumers. No repeat upgrade; natural state/first actor/all actions remain unproved; current conversion excludes7. |
| 8 | [Initialization/upgrades/first dispatch](./battle08-initialization-and-first-control.md) joins13/11/4/1, actual party versus thirteen templates, F508 category/level/RNG type selection, one HP0 neutral, four initially untriggered sloping regions and distinct point0/enemy128/ally0 orders to consumers. Existing before/outcome owners retain surrounding content. Natural state/substitutions/first actor/all actions remain unproved; current conversion excludes8. |
| 9 | [Initialization/upgrades/first dispatch](./battle09-initialization-and-first-control.md) joins12/11/2/0, actual party/Peter joined-active-AI-follower distinctions, F509 level/category/RNG upgrades with the airborne count0 defect, two initially untriggered ordered regions, NONE/follow standby and conditional control to consumers. Surrounding admission/outcome owners remain separate. Natural state/substitutions/first actor/all actions stay unproved; production excludes9. |
| 10 | [Initialization/first dispatch/Power Ring](./battle10-initialization-and-first-control.md) joins12/12/3/1, actual May/Peter state, F510 upgrades, three initially untriggered regions and point0/live137/134/ally0 bindings to conditional control. Ring129 initialization masks declared EQUIPPED; actual item19/action targetDies/bit4/capacity/RARE deals govern custody. Natural state/substitutions/actions/custody stay unproved; production excludes10. |
| 11 | [Initialization/regions/first control](./battle11-initialization-and-first-control.md) joins12/10/4/1 to actual party/scene replacement, base gear/spells/difficulty refresh, no repeat substitution/drop, four initially untriggered regions and sets6/7/9/14. Three15/15 enemies, fixed point0/live128/ally0 and NONE/follow standby remain distinct. Natural party/first actor/all actions/darkness stay unproved; production excludes11. |
| 12 | [Initialization/dual regions/first control](./battle12-initialization-and-first-control.md) joins12/12/4/1, actual party/Peter, empty hooks, five base definitions/gear/HEAL2/BLAST1/FREEZE1 and no random substitution/drop. Four fresh regions miss; primary/secondary1/3 and0/1 activation, six15/15 enemies, NONE standby movement, fixed point0/live128 and sets0/2/6/7/14 remain distinct. Natural party/first actor/all actions remain unproved; production excludes12. |
| 13 | [Initialization/actual party/neutral/first control](./battle13-initialization-and-first-control.md) joins12/16/3/1, Gerhalt F9/F41 versus capacity/HP, prior Peter bit2 clear, F513-gated neutral159 versus Luke membership, empty hooks, six complete base definitions/gear/prowess/BLAZE2/HEAL2/BLAST1 and no random substitution/drop. Three fresh regions miss; primary-first2/2 on134/136, NONE/follow standby, point0/live128/135 and sets2/6/7/9/14 remain distinct. Natural state/first actor/all tactics are unproved; production excludes13. |
| 14 | [Southeast Desert account](./battle14-desert-encounter-and-outcomes.md): Map70 request/cooldown and10-before14 scan, complete formation/empty hooks, conditional repeat upgrades, region/control predicates, Power Stick conditions and outcomes/returns/exits. Natural reach, unsafe airborne scan results, all tactics and production14 remain outside its static boundary. |
| 15 | [Shrine account](./battle15-shrine-approach-and-outcomes.md):69/61 approach, full exterior population/opening, both50 admissions, complete scenes/formation/control boundary, hidden Golem/chest/drop conditions, outcomes and three exits. Natural paths, all tactics and implementation remain outside its static boundary; [Panel/Sword custody](./ribble-tree-opening-and-achilles-sword.md) retains its separate scope. |
| 16 | [Initialization/SWARM/first control](./battle16-initialization-and-first-control.md) joins12/11/0/0, actual party/scene replacement, three complete Kraken definitions/AQUA1 and no random substitution/drop. Eight SWARM actors use thresholds5/4/2/2/3/0/0/4/1/3/0; ALLIES12−2/DBF checks exactly128–138, fresh count0. Three ordinary legs bypass it; live135/131 orders, distinct Aquatic/Kraken costs and Head double turns reach conditional control. Natural state/first actor/all actions remain unproved; production excludes16. |
| 17 | [Initialization/repeat upgrades/first control](./battle17-initialization-and-first-control.md) joins12/14/2/0, ordered Map69 selection, actual party/refresh, six bases and F517 substitution. BRASS_GUNNER uses melee bounds/fallback; FLYING count0 overread remains unsafe. Both regions initially miss; only135/136 assign0, while1 is still tested. Live ally1/0/enemy141 and exact sets0/2/6/7 differ;0/6 have no MOVE_ORDER. Natural state/first actor/all actions remain unproved; production excludes17. |
| 18 | [Initialization/Taros equipment/first control](./battle18-initialization-and-first-control.md) joins12/14/5/0, seven full definitions, no repeat upgrade, five fresh region misses, exact2/6/7/9/14 streams and live128/137/ally0 follows. LEADER reads absent point0 through the following header before MOV0; Taros has two round entries and equipped BOLT2 use. Bowie0's equipped Achilles gate, counter direction, conditional Sword drop/deals and neutral159/lookup31 remain distinct. Natural state/actions/loot are unproved; production excludes18. |
| 19 | [Initialization/Shining Ball/first control](./battle19-initialization-and-first-control.md) joins12/15/3/1, actual party/final-CURSE refresh, six bases, F519 effective-level29+ substitution and BRASS_GUNNER/unsafe FLYING boundaries. All72 triangle substitutions miss; primary1/0/2 versus15/15, NONE/follow standby, declared point0/live128/133/135/139/140 and exact2/6/7/9/14 streams differ. Learned HEAL3/BLAST2/FREEZE2, masked Ball16 and no-RNG conditional action-drop/deals do not prove actions/loot; production excludes19. |
| 20 | [Initialization/SWARM/neutral/first control](./battle20-initialization-and-first-control.md) joins12/12/0/0, actual party/final-CURSE refresh, five complete definitions/gear/poison/DISPEL/HEAL3/BLAST2/FREEZE2 and no random substitution/drop. All15/15/STARTING, zero-region cache return, exact2/6/9/15 streams and live131/129/133/136 orders differ. SWARM128/134/139 use3/2/1 and HP-only128–138 count0, omitting real139; damage bypass/re-healed wait is conditional. Neutral159 writes31/read63, separate from ally13/active45; production excludes20. |
| 21 | [Initialization/regions/upgrades/items/first control](./battle21-initialization-and-first-control.md) joins12/12/4/0, six full bases, all96 fresh triangle misses, F521 selection/unsafe airborne scan, live128/131/135 and exact0/2/6/9 streams. Masked Herb/Ring custody and independent62/20 drops/once7/8 remain conditional. Existing opening/outcome owners retain scenes/returns. Natural party/repeated roster/actions/loot remain unproved; production excludes21. |
| 22 | [Initialization/SWARM/dual neutrals/first control](./battle22-initialization-and-first-control.md) joins12/16/0/1, six full bases and actual party/final-CURSE refresh. Nine actual SWARM actors use thresholds1/3/6; HP-only128–138 omits139–143. Exact2/3/7/9/14/15 streams, valid King point0/live128/137/136/132/134, candidate-local HEAL3 and King extra entry differ. F522-gated159/158 write31/30 while ordinary readers use63/62. Natural state/actions/first actor remain unproved; production excludes22. |
| 23 | [Initialization/region/AQUA2/first control](./battle23-initialization-and-first-control.md) joins12/12/1/1, actual party/final-CURSE, one initially missed region, live ally0 versus point0 and complete6/7/9/14 streams. AQUA2 costs0 but its packed`$68` misses AQUA1's`$28` shortcut; Willard165 gives37/30 round bases. No repeat substitution/drop/neutral/SWARM. Natural state/first actor/actions remain unproved; production excludes23. |
| 24–25 | Creed/North Cliff selected fresh initialization/outcomes/map links; not every region/first-control/action path. |
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
| Creed choices/later alternatives | Accepted selected static account: [recruitment owner](./creed-return-fairy-and-recruitment-choices.md) covers the complete shared interaction table, initial/F22 offers, refusal and actual join/follow paths for all four candidates; [restoration](./creed-restoration-and-basement-departure.md) and [Oddler handoff](./oddler-stays-and-fairy-handoff.md) retain setup/init alternatives. | F22's late population and offers are already explained. Natural return, ordinary contact, every travel itinerary and recruiting everyone on a naturally reached save remain unproved; they do not imply another missing recruitment scene. See the [bounded reassessment](#remaining-labels-and-next-content-task). |
| Dwarven Village29 and smith; separate mine30 | [Village29 lifecycle](./dwarven-village-and-mithril-smithing.md) connects77 admission, seven residents/search/eighteen warps, actual smith placement/readiness/collection and exit/revisit. Earlier30 recovery/Cannon remains a separate accepted chapter via68. | Complete static account; natural access, full church/shop services, presentation, saves and production smithing remain unproved. Existing helper/transaction/caller H3 retains only its bounded seams. Source interior/exterior names do not connect29 and30. |
| Arm/Claude and Magic Tunnel63/64 | [Moun Arm award](./moun-town-painter-and-golem-arm.md) and [Hub/Claude account](./magic-tunnel-hub-and-claude-recruitment.md) connect acquisition boundary, both talk/item-use entries, actual removal, complete awakening/join/follow, bridge/pipe transitions and exits/revisit. | Complete selected static branch; no natural journey from Moun, custody by a fixed member, guaranteed active/following Claude, production binding or save durability follows. |
| Later Yeel/Chaz | [Late Yeel account](./late-yeel-chaz-and-lemon-recruitment.md) connects530/28 population selection, all late interactions/search, piano and underground scenes, Chaz talk/join/follow and return/revisit. | Complete selected static chapter; no natural route, guaranteed active/following Chaz, service presentation, save or late production acceptance. |
| Lemon | [Red Baron aftermath](./battle37-outcomes-and-lemon-aftermath.md), [Map41 sword/head entry](./battle39-outcomes-and-force-sword-route.md) and [Yeel zone recruitment](./late-yeel-chaz-and-lemon-recruitment.md#chaz-join-lemon-join-and-changed-party-states) retain their separate gates and lifetimes. | After37 does not recruit;41 and05 conditional joins are organized. F28/active60/F71/F945 differ; no all-entrances/all-recruits census follows. |
| Dojo45 and hidden Battle0 | Accepted bounded chapter owners. | Optional Dojo and special EndGame/reset caller do not close other optional encounters. |
| Battle14 | Dedicated [static encounter/repeat/outcome account](./battle14-desert-encounter-and-outcomes.md). | Natural order/state, actual airborne overread results, full tactics/runtime and production consumption remain unproved. |
| Fairy Woods44 | Dedicated [static trial/records account](./battle44-fairy-woods-trial-and-records.md). | Complete declared formation/regions and hidden/respawn/leader/timer/result/return consumers; natural play, all tactics, chest custody, exact presentation/time and persistence remain unproved. |
| Battle28 bridge branch | Dedicated [static entry/scenes/formation/control/outcome/return account](./battle28-bridge-and-outcomes.md). | All action/AI decisions, natural traversal and late production consumption remain unproved; no physical collapse/countdown inferred from staging. |
| Late town revisits, shop/church/Caravan, promotion/equipment | Selected alternatives/services/warps where linked; [Hub63/Pipe64](./magic-tunnel-hub-and-claude-recruitment.md) and [late Yeel05](./late-yeel-chaz-and-lemon-recruitment.md) own those bounded returns. Global [menus](./common-menus.md), [map content](./map-content.md), [search](./field-search-control.md), [item effects](./field-item-effects.md) retain mechanisms. | No exhaustive late-state NPC/chest/travel census was selected. Generic service state machines already have static owners; their runtime/UI/persistence limits and production support are separate. A new content task needs a named missing selector/callback relationship, not this broad label. See the [dispositions below](#remaining-labels-and-next-content-task). |

The29 smith entry and Chaz declarations use pinned
`data/maps/entries/map29/mapsetups/s2_entityevents.asm` and
`data/maps/entries/map05/mapsetups/s1_entities_530.asm` / `s1_entities_28.asm`.
The [Claude owner](./magic-tunnel-hub-and-claude-recruitment.md#talk-and-explicit-item-use-custody-and-result)
now follows both Map63 entity talk and `s5_itemevents.asm:Map63_ItemEvent0`
through physical Arm removal and `scripts.asm:cs_5CBB4`, including separate
membership/active/follower lifetimes. The [Yeel owner](./late-yeel-chaz-and-lemon-recruitment.md)
also follows actual late pointers/callbacks and all selected scene bodies;
its Chaz account extends beyond the declaration identities.

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

The dedicated Battle14/15/28/44, Village29/smith, Hub63/Claude and late
Yeel/Chaz/Lemon accounts in the tables above cover their declared static
branches. Their runtime, design, production and save limits remain explicit;
those limits do not reopen the same static content tasks.

## Remaining labels and next content task

This is an incremental review within the chapter/encounter/selected-branch
denominator above. **Confirmed** below describes the inspected owner and source
boundary. **Inferred** prioritizes a useful next account. A category left
unassessed is not an assertion that all its source is missing or already explained.

| Remaining label | Disposition and semantic evidence | Consequence for planning |
| --- | --- | --- |
| Creed late return/recruitment | **Confirmed:** `data/maps/mapsetups.asm` selects Map33's baseline last for F22, overriding F786. Baseline `s1_entities.asm` declares the four recruits; `s6_initfunction.asm:byte_5ABEE` still independently gates their placement on F784 and each joined flag. `s2_entityevents.asm:Map33_EntityEvent3–6` tests F22 before F787. The complete offers/successes, layout copy and current/fresh actor differences are already in the [recruitment](./creed-return-fairy-and-recruitment-choices.md), [restoration](./creed-restoration-and-basement-departure.md) and [handoff](./oddler-stays-and-fairy-handoff.md) owners. F22 is Frayja's joined flag: [Battle33's join consumer](./battle33-initialization-and-opening.md#keyed-texts-join-ceremony-and-actual-replacement-rule) connects `data/battles/entries/battle33/cs_beforebattle.asm:join ALLY_FRAYJA` through JoinForce. | No additional four-recruit scene investigation is justified by the old partial label. F22 is not a Battle22-completion gate. Existing Map33 exits to34/67 and the accepted return selectors do not prove a natural late journey, collision access or four successful recruitments on one save. Keep those runtime/itinerary questions at their owners. |
| Early encounter initialization | **Confirmed:** the [Battle02 account](./battle02-initialization-and-first-control.md) joins all selected formation/order operands to fresh/suspended entry, enemy refresh, activation/spawn/round and first-control consumers. The [Battle03 account](./battle03-initialization-and-first-control.md) adds12/6/1/0, conditional Yeel egress, region0/15, masked bat item and503 upgrades with the airborne defect retained. The [Battle04 account](./battle04-initialization-and-first-control.md) adds5/8/3/1, complete before/scene rebuild, live party versus templates, three initially untriggered regions and non-upgradable equipped enemies with distinct point/follow orders. The [Battle05 account](./battle05-initialization-and-first-control.md) adds6/8/1/1, empty hooks, CASTLE background, equipped knight/archer/mage/soldier, BLAZE1 and SENTRY versus region0 standby. The [Battle06 account](./battle06-opening-and-first-control.md) adds6/9/1/0, complete before scene/text and F407, Dark Cleric HEAL1/HEALER3/live-follow129 and NONE versus special-order standby. The [Battle07 account](./battle07-opening-and-first-control.md) adds7/10/4/1, complete confrontation/text, five F507-gated neutrals, Smoke extra turns/DISPEL, four initially untriggered regions and live-follow129/133/136. The [Battle08 account](./battle08-initialization-and-first-control.md) adds13/11/4/1, actual party/capacity, F508 effective-level/category/RNG upgrades, one HP0 neutral, four initially untriggered sloping regions and distinct point0/enemy128/ally0 orders. The [Battle09 account](./battle09-initialization-and-first-control.md) adds12/11/2/0, Peter's four independent states, F509 upgrades including the airborne defect, two initially untriggered regions, NONE versus follow standby, live enemy128/ally0 and no point destination. The [Battle10 account](./battle10-initialization-and-first-control.md) adds12/12/3/1, May/Peter state, F510 upgrades, three initially untriggered regions, point0/live137/134/ally0 and the selected Power Ring mask/use/drop/rare-deals relationship. The [Battle11 account](./battle11-initialization-and-first-control.md) adds12/10/4/1, separate scene/party states, base definitions without random upgrades, four initially untriggered regions and point0/live128/ally0 with sets6/7/9/14. The [Battle12 account](./battle12-initialization-and-first-control.md) adds12/12/4/1, empty hooks, five base definitions, dual-region1/3 and0/1 activation, four fresh misses, Monk HEAL2/BLAST1 and point0/live128 with sets0/2/6/7/14. The [Battle13 account](./battle13-initialization-and-first-control.md) adds12/16/3/1, actual Gerhalt/Peter participation, F513-gated HP0 Luke neutral159, six base definitions, three fresh misses, same-region2/2 on134/136 and point0/live128/135 with sets2/6/7/9/14. The [Battle16 account](./battle16-initialization-and-first-control.md) adds12/11/0/0, eight SWARM actors/three ordinary legs, exact HP-only128–138 count, live135/131 orders, full Kraken definitions/AQUA1 and Head double turns. The [Battle17 account](./battle17-initialization-and-first-control.md) adds12/14/2/0, six full bases, F517 upgrades with BRASS_GUNNER melee fallback and unsafe FLYING count0, two fresh misses including unassigned1, actual ally1/0/enemy141 orders and exact0/2/6/7 streams. The [Battle18 account](./battle18-initialization-and-first-control.md) adds12/14/5/0, seven full bases, five fresh misses, neutral159/lookup31, Taros LEADER absent-point access, equipped BOLT2, Bowie0 equipped61 gate and conditional110 drop/deals. The [Battle19 account](./battle19-initialization-and-first-control.md) adds12/15/3/1, six full bases, F519 upgrades from E29, three fresh misses, declared point0/live128/133/135/139/140 and exact2/6/7/9/14 streams. HEAL3 current-input keys/costs/promotion and failed-candidate local downgrade, Ball16 mask/getter termination and independent no-RNG action-drop/deals retain their actual conditions. The [Battle20 account](./battle20-initialization-and-first-control.md) adds12/12/0/0, five complete definitions, omitted139 in the128–138 HP count, three SWARM gates3/2/1, exact2/6/9/15 streams, live131/129/133/136 orders, DISPEL SUPPORT and candidate-local HEAL. Neutral159 writes31/read63 separately from Elric13. The [Battle21 account](./battle21-initialization-and-first-control.md) adds12/12/4/0, six full bases,96 fresh triangle misses, F521 E31/category/RNG/unsafe airborne selection, live128/131/135 and0/2/6/9 streams. Masked Herb/Ring additions and independent Broad Sword135/Ring129 once7/8 drops retain actual inventory/actor/death conditions. The [Battle22 account](./battle22-initialization-and-first-control.md) adds12/16/0/1, six full chess definitions, nine SWARM actors and HP-only128–138/omitted139–143, exact2/3/7/9/14/15 streams, valid point0/live orders, candidate-local HEAL3, King extra entry and dual-neutral31/30 versus63/62 mappings. | These finite static accounts add no natural arrival, fixed first actor, all-action or arbitrary-party production claim. The importer admits2/3, not4/5/6/7/8/9/10/11/12/13/16/17/18/19/20/21/22; controlled identities, capacity rejection, general refresh, upgrade selection, ranged/equipped support and suspended-entry limits remain explicit. Battle05 also requires currently Unsupported SENTRY/MAGE/BLAZE behavior; Battle06 adds HEALER3 and the cleric's prowess/healer-AI loadout boundary. Battle07 additionally requires Smoke silence/double/counter prowess and DISPEL execution; its hovering and extra-round-action data are expressible but do not establish encounter support. Battle08 adds STEALTH_ARCHER/Hunter prowess/range2–2 and Witch MAGE/FREEZE limits; repeated-entry admission does not implement F508 upgrades. Battle09 also requires Bat poison prowess/BLAZE2 and conditional Peter AI/class support; Flying/Hovering are already expressible, while F509 upgrades, equipment/refresh/capacity and saved-state limits remain. Battle10 additionally requires the selected Ring initialization/use, actual action-drop inventory/once-bit and rare-deals consumers; descriptive item fields are not executable support. Battle11 adds Hobgoblin/Middle Axe descriptive conversion, coordinate-trigger admission and the existing weapon/general-refresh limits; set14/point/follow/NONE and Flying already exist. It needs no original repeat-upgrade mechanism. Battle12 adds trusted Death Archer/Cloud/Monk definition conversion, ARCHER/range2–2, Monk prowess/HEALER3/sets0/2 AI and BLAST/FREEZE/equipped-refresh limits. Hovering, dual-region activation and wildcard-route structure already exist; descriptive resistance is not executable spell/attack support. Battle12 needs no original repeat-upgrade mechanism. Battle13 adds trusted Mage/Black Monk/Zombie selection, Rod/equipped and BLAZE/BLAST/healing-spell AI, Zombie poison/Monk prowess, actual Gerhalt/Peter/Luke classes/join/control and neutral159/F513 placement requirements. Existing primary-first2/2, generic definition lookup, formation/orders/round and Hovering are reusable; descriptive resistance is not execution. It also needs no original repeat-upgrade mechanism. Battle16 additionally requires trusted conversion, SWARM threshold/count/re-wait and exact15 stream, AQUATIC/Kraken move-cost profiles, Arm/Head ranges, all three prowess expressions, AQUA execution and Head special mapping/services. Formation/NONE/live-order/round primitives exist; no original repeat substitution is needed. Battle17 adds trusted conversion/selected17 admission, F517 definition selection and unsafe airborne behavior, BRASS_GUNNER/MAGE and Steel Arrow2–2, Soulsower stun/Golem/Monk prowess, exact0/2 healing AI and BLAZE/BLAST/equipped-refresh requirements. Generic definition/formation/live-order/dead-target/round and Flying mechanisms already exist; no special17 drop or new fixed-point mechanism is required. Battle18 additionally requires trusted admission, type3 region/signed projection/cache behavior, LEADER out-of-table point access, selected gear/ranges/prowess, MAGE/BRASS_GUNNER/HEALER3 and item/spell AI, Bowie gate/counter/drop/deals and neutral mapping/services. Generic definitions, formation, Flying, LEADER and extra turns exist; no original repeat upgrade is needed. Battle19 adds trusted admission/conversion,519 substitution, selected gear/ranges/prowess, MAGE/BRASS_GUNNER, HEALER3/HEAL3/spell/item AI, actual16 consumption/drop/deals and party/refresh/control/suspend limits. Its type4 rectangles, valid point0, generic LEADER/live orders, formation and Flying reuse existing mechanisms; a new enemy name is not a new mechanism. Battle20 adds trusted conversion, SWARM count/threshold/re-wait/stream15, HEALER3/candidate-local healing/DISPEL and offensive-spell decisions, poison/Orc prowess, selected gear/range/movers, neutral mapping/services and actual party/status/control/suspend limits. Existing Flying/live-order/zero-region/round mechanisms remain reusable; no fixed-point mechanism is needed. Battle21 adds selected21 conversion, original signed geometry/cache, ENHANCED Skeleton, MAGE/BRASS_GUNNER/prowess/gear, exact0/2 AI, candidate-local HEAL/DISPEL/offensive spells, Ring/Herb use/drop/deals/once state,521 substitution and actual party/refresh/control/suspend limits. Formation/live orders/Flying/round mechanisms remain reusable; no SWARM or fixed-point platform is needed. Battle22 adds selected22 trusted conversion, SWARM HP-only128–138/omitted139–143 count, thresholds/re-wait/exact15, HEALER3/4 exact2/3 fallbacks, enhanced/gear/prowess/spell/mover support, dual-neutral mapping and actual party/status/control/suspend limits. Generic LEADER/valid point0/live-order/zero-region/extra-round mechanisms are reusable; no geometry or scene platform is required. No implementation is changed. Other early-encounter limits remain qualified and have not each been traced into new tasks. |
| Town revisits | **Confirmed:** named changed populations already occur in the chapter owners, including [restored Pacalon/Frayja](./pacalon-return-frayja-and-battle31-admission.md#fresh-towncastle-lifetime-and-restored-population), the F22 Creed alternatives, Hub63 and late Yeel above. Pacalon's F530 selector changes the castle entity/event pointers while shared init tests joined22; it is a real selected return account, not just a first-visit label. | Do not reopen those selected returns. No full-map/state comparison was made to identify every other late NPC/chest/service variation. The residual census is intentionally unassessed: its state combinations and user outcome are not specified by this denominator. No concrete additional town chapter is recommended from that absence alone. |
| Generic services, promotion/equipment | **Confirmed:** [common menus](./common-menus.md#static-service-menu-state-machines) owns the complete built eight-source service state machines, including Church promotion selection/item removal/SetClass→Promote and Caravan Join/Depot/Item/Purge. Its [caller inventory](./common-menus.md#static-service-menu-state-machines) retains all69 direct transfers and selected bounded return observations. [Common stats](./common-stats.md), [field items](./field-item-effects.md) and [smith29](./dwarven-village-and-mithril-smithing.md) retain their helper and local lifecycle boundaries. | No new generic service inventory is warranted by a town's call to ShopMenu/ChurchMenu/CaravanMenu. Original UI/service completion, persistence and unobserved callers stay Unknown; actual service implementation is a consumer task. This does not assert all equipment/promotions or every location's eligibility has been semantically exhausted. A future local account must name the omitted caller/state relationship. |

The Pacalon check uses `data/maps/mapsetups.asm:msMap 36`,
`map36/mapsetups/pointertable_530.asm`, `s1_entities_530.asm`,
`s2_entityevents_530.asm` and shared `s6_initfunction.asm` under
`data/maps/entries/`. The service check uses actual ShopMenu, ChurchMenu and
CaravanMenu selectors under `code/common/menus/` and their accepted complete
statement/caller owners, not filenames as evidence of service completion.

### Battle02 initialization through first dispatch

The [dedicated static account](./battle02-initialization-and-first-control.md)
now supplies the bounded deliverable selected by the remaining-gap review. Its
users are the content/engine maintainer and independent reviewer deciding which
original rules conversion must preserve under changed valid party state.
It includes every selected formation/AI-point/behavior row and the actual
fresh/suspended, refresh/load, activation/region/spawn, round and first-control
consumers. The four ally templates neither choose four fixed heroes nor scan
until four living actors are found: a dead active member consumes one of the
four member iterations without advancing the placement cursor; excess members
leave the active party.

### Battle03 initialization through first dispatch

The Battle03 [dedicated account](./battle03-initialization-and-first-control.md)
adds all twelve formation positions and six enemy rows without treating the
templates as joined members. Its first fresh region check cannot be triggered by
the actual ally formation south of region0; two enemies use inactive standby
while four15/15 enemies can enter ordinary AI. The bat's declared AI-use/drop
bits are masked by AddItem, and completed503 can upgrade enemy definitions through
the existing random-battle algorithm, including its unsafe airborne exclusion
scan. Before-program Yeel EGRESS assignment remains conditional on fresh entry
and clear453. None of this establishes natural state or an actual first actor.

### Battle04 opening through first dispatch

The [dedicated account](./battle04-initialization-and-first-control.md) connects
all5/8/3/1 operands and the complete before program to scene loading and later
combatant-derived rebuilding. Capture dialogue does not write capture status or
force defeat. Fresh ally positions cannot trigger any of the three regions,
including region2's slanted edge; two15/15 enemies bypass standby while the other
six use its actual predicates. Point0, ally0 and enemy128 are distinct order
targets. Battle04 misses the random-upgrade selector, even with504 set. Scene
Lemon does not become an enemy deployment or a required active member.

The accounts check the existing producer, Content reader, common initialization,
Application entry/round flow and selected source-order AI. These already admit
Battles02/03, while Battle04 is not admitted or converted. Controlled ally
identity admission, capacity rejection, general refresh, repeat upgrade selection
and suspended-state limits remain distinct from original behavior. No engine
change, new fixture, natural original observation or all-action completion is
implied. Battle04 additionally exposes existing class/movement/AI-word, ranged
weapon and equipped-enemy refresh boundaries; no allowlist or engine code changes.
Existing entrance/outcome owners keep their separate conditional
connections and provenance. Runtime and production Unknowns do not reopen the
same static organization task.

### Battle05 initialization through first dispatch

The [dedicated account](./battle05-initialization-and-first-control.md) connects
all6/8/1/1 operands and empty before/start hooks to actual party initialization,
CASTLE metadata, four equipped enemy definitions and the first conditional
dispatch. Six placements do not recruit Slade. Fresh ally positions cannot
trigger region0;128/134/135 use its special-order standby stop, while129–133
bypass standby through15/15. SENTRY8 has no follow-movement command despite
two stored FOLLOW_ENEMY0 orders; its ATTACK1 can still construct an attack
path. BLAZE1 availability is separate from first-action selection. Battle05
misses the upgrade selector even with505 set.

Actual production inspection retains the2/3 producer and1/2/3 reader/program
admission boundary. Existing SENTRY, ARCHER/MAGE, BIT4, ranged weapons,
equipment refresh, BLAZE, capacity and suspended-state limits prevent treating
shared primitives as Battle05 support. No allowlist or engine change, natural
observation, all-action claim or frontier advancement is implied. The existing
captivity/admission and Jewel/Slade/outcome accounts keep their own endpoints.

### Battle06 opening through first dispatch

The [dedicated account](./battle06-opening-and-first-control.md) connects all6/9/1/0
operands and the complete before scene/text to actual party initialization,
OVERWORLD metadata, Dark Cleric HEAL1 and equipped Galam enemies. Scene Slade
is not membership; scene soldier128 is not battle cleric128. Before writes407,
while victory writes506. Fresh ally placements trigger no region0; five15/15
enemies bypass standby, while cleric128's special-order stop differs from the
three NONE/0/15 enemies' conditional local movement with STAY. HEALER3 is not
HEAL level3; its stored FOLLOW_ENEMY1 binds129's live position. Battle06 misses
the random-upgrade selector even with506 set.

Production remains at2/3 producer and1/2/3 reader/program admission. HEALER3,
SENTRY, cleric prowess, ARCHER/MAGE/BIT4, ranged/equipped enemies, attack-spell
execution and healer-AI loadout remain unsupported at their named consumers.
No natural arrival, fixed first actor, all-action audit or production support
follows. Existing admission and outcome owners keep the surrounding story;
this account changes no fixture count or runtime/design/production frontier.

### Battle07 opening through first dispatch

The [dedicated account](./battle07-opening-and-first-control.md) connects all7/10/4/1
operands and the complete28-key confrontation to Map58/TOWER_INTERIOR metadata,
actual party and combatant-derived rebuilding. Seven templates do not recruit
Kiwi. Before clears83 only if executed; scene133–135 are Galam-sprite actors,
not a Dark Smoke spawn mechanism. The actual completion-gated neutral table
loads five HP0 actors independently of the before aliases. Fresh ally positions
trigger no region; six15/15 enemies bypass standby, while two NONE/region
Smokes permit conditional local STAY movement and two special-order followers
stop until activation. Follow orders bind live129/133/136; point0 is unused.
Dark Smoke AGI13+128 supplies extra round entries, with DISPEL SUPPORT separate
from physical silence prowess. Battle07 misses the actual upgrade selector.

Production remains at2/3 producer and1/2/3 reader/program admission. Hovering,
extra-round-action and follow mechanisms are expressible; Smoke prowess and
DISPEL, HEALER3, cleric prowess, ranged/equipped and mage/healer AI remain at
explicit unsupported consumers. No natural arrival, fixed party/first actor,
all-action audit or production support follows. Existing admission and outcome
owners retain the surrounding story and long aftermath. Fixture counts and
original-runtime/design/production frontiers are unchanged.

### Battle08 initialization through first dispatch

The [dedicated account](./battle08-initialization-and-first-control.md) connects
all13/11/4/1 operands to Map72 area`(1,32,32,30)`, actual joined/active/dead
membership and capacity, refreshed enemies and one F508-gated HP0 worker.
It reuses the complete35-command before owner, preserving F6/HP-aware Kiwi
relocation and actual speaker130, and leaves all outcomes/construction with
their existing owner. Thirteen templates neither recruit thirteen members nor
replace general JoinForce's below12 active rule. F508 enables the actual random
upgrade selector at effective level18 for ordinary inputs; per-slot category,
bounds/exclusions/retry and RNG precede definition copy, not every round.

All thirteen fresh positions miss all four sloping regions under the original
integer projection/CCR branches. Six assigned-region special-order enemies
stop on inactive standby; five15/15 enemies bypass it. Point0, enemy128 and
ally0 are distinct destinations. Primary/secondary activation does not itself
swap orders; dead-follow promotion and NONE have their actual consumers.
No fixed first actor, natural party/substitution/region crossing, all-action
acceptance or production8 support follows. Actual2/3 producer and1/2/3
reader/program admission, missing repeat-upgrade logic, STEALTH_ARCHER/ranged,
Hunter prowess, MAGE/FREEZE, equipment/refresh/capacity and suspend limits
remain explicit. No fixture count or runtime/design/production frontier changes.

### Battle09 initialization through first dispatch

The [dedicated account](./battle09-initialization-and-first-control.md) connects
all12/11/2/0 operands to Map69 area`(0,0,48,32)`, actual membership/refresh,
empty hooks and no neutral population. Peter F7 joined, conditional F39 active,
combatant AI bit2 and F67 follower have separate producers/consumers; templates
and map following establish no natural party or first actor. F509 enables
per-slot upgrades from ordinary effective level19, with actual categories,
bounds/exclusions/shared retry/RNG before definition copy. Both Bat slots use
the airborne count0 scan defect; no safe substitution result is invented.

All twelve fresh positions miss both ordered regions under signed integer
projection/CCR. Region-assigned NONE permits conditional local standby movement;
non-NONE follow stops inactive standby; three15/15 enemies bypass it. Live enemy128
and ally0 remain distinct from nonexistent point0; first dispatch is conditional
player/AI/no-action with correct opponent-control polarity. Production excludes9.
Existing Flying/Hovering/standby/follow/round mechanisms are reusable; Bat poison
prowess/BLAZE2, Hunter STEALTH_ARCHER/ranged/prowess, Witch MAGE/FREEZE, equipped
loadouts/general refresh, actual Peter AI/class/party and suspend support remain
explicit limits. Admission/outcome/Ribble owners retain surrounding content;
no fixture count or runtime/design/production frontier changes.

### Battle10 initialization, first dispatch and Power Ring custody

The [dedicated account](./battle10-initialization-and-first-control.md) joins
complete12/12/3/1 to Map70 area`(0,0,32,32)`, actual party/refresh, empty hooks,
no neutral and independent410/460/510/88. May8/40/HP/AI/transient following and
Peter7/39/AI/follower67 stay separate from templates. F510 upgrades begin at
ordinary effective level20; actual category/bounds/exclusions/retry/RNG precede
definition copy. All three Bat rows retain the unsafe airborne count0 scan.

All12 fresh positions miss three ordered regions under integer projection/CCR.
Assigned point/follow orders stop inactive standby; NONE permits conditional
local movement; five15/15 enemies bypass it. Point0(17,25), live137/134 and ally0
are distinct; set6 has no MOVE_ORDER while sets7/9 attempt their actual streams.
First dispatch remains conditional player/AI/no-action, without a fixed actor.
Witch129's successful AddItem masks declared Ring EQUIPPED/drop bits and returns
unequipped19, so no fresh Ring ATT+5 or automatic support use follows. Conditional
action-drop requires actual held19/targetDies/ally actor/enemy129, once-bit4 and
recipient capacity; dead/full recipient uses Ring RARE deals, with saturation.
This is neither generic death cleanup nor an automatic victory reward.

Production excludes10. Shared formation/point/follow/standby/round mechanisms
exist; general refresh/gear/spells/movement/prowess, Peter/party/suspend, F510
selection and Ring/action-drop/rare-deals custody remain explicit consumer limits.
No natural party/substitution/action/custody, fixture count or runtime/design/
production frontier is added. Admission/outcome/shrine owners retain their scope.

### Battle11 initialization, regions and first conditional control

The [dedicated account](./battle11-initialization-and-first-control.md) joins
complete12/10/4/1 to exact Map48 trigger15,26/crop27×32, row11-before25,
independent411/461/511/88, actual party/healing/refresh and scene-to-battle
reconstruction. Existing admission owns all34 opening records/seven texts and
Plane-B; outcomes/Polca remain with the adjacent owner. Scene Peter/three enemies
and old-man following do not establish battle members or a fixed natural party.

The real random list omits11: its upgrade entry restores the original definition
before level/category/exclusion/RNG processing even with511 set. All placement
items are NOTHING, and the independent drop table has no11 entry. Hobgoblin's
Middle Axe/LEADER14 is neither special loot nor a leader-death victory rule.
All12 fresh positions miss four ordered regions under integer projection/CCR.
Three15/15 enemies bypass standby; assigned follow stops it, assigned NONE allows
conditional local movement. Fixed point0(15,4), live128 and ally0 differ. Set6
has no MOVE_ORDER; set7 NONE first fails MOVE_ORDER1 then tries its remaining
stream; sets9/14 reach their actual modes. First actor/control remains conditional.

Production excludes11. Hobgoblin/Middle Axe descriptive/adjacent-physical fields,
sets6/7/9/14, formation/point/follow/standby/Flying/round mechanisms already exist;
coordinate-trigger admission, general refresh/equipped AI, Hunter/Witch/Bat
movement/prowess/spells, arbitrary party/Peter and suspend limits remain explicit.
No natural state/action/darkness/timing/persistence, fixture count or runtime/
design/production frontier is added. Retained failures and corrections above
remain unchanged.

### Battle12 initialization, dual regions and first conditional control

The [dedicated account](./battle12-initialization-and-first-control.md) joins
complete12/12/4/1 to sole Map52 wildcard/crop32×32, independent412/462/512/88,
empty dispatch hooks, actual party/healing/refresh and rebuilt battle entities.
The four searching-scene NPCs and Peter/follower states do not establish the
battle roster. Existing admission and leader-outcome/Bedoe owners retain their
scope. The random list and special-drop table omit12; no512 repeat substitution
or Battle10 airborne upgrade processing follows.

All12 fresh positions miss all four ordered regions by actual integer projection/
CCR. Cloud129 uses1/3, Witch131/Hobgoblin137 use0/1, three others2/15 and six
use15/15. Primary hits win; secondary hits set both bits and remain sticky.
NONE standby can construct local movement. Fixed point0(22,8) and live128 differ;
sets0/2/6/7/14 reach conditional control. Monk HEALER3 is commandset2 with HEAL2
and BLAST1, not HEAL3; its actual AI selects HEAL1 while testing the unmasked
HEAL2 operand's5 MP cost. Weapons, spell getters, prowess, difficulty and no-drop
consumers are explicit, without a fixed first actor or performed action.

Production excludes12. Definition lookup, formation/point/follow/NONE/standby,
Hovering, dual-region activation and round mechanisms exist. Trusted conversion,
ARCHER/MAGE/range2–2, Monk prowess/sets0/2 AI, general gear/CURSE refresh,
BLAST/FREEZE/healing-spell AI, Peter/arbitrary party and suspend remain consumer
limits. Descriptive Cloud resistance is not executable damage/spell support.
No natural state/action, fixture count, runtime/design/production frontier or
preserved failure is changed.

### Battle13 actual party, neutral entity and first conditional control

The [dedicated account](./battle13-initialization-and-first-control.md) connects
complete12/16/3/1 and sole Map13 wildcard/crop32×32 to independent413/463/513/88,
empty hooks, actual party/healing/refresh and scene-to-battle replacement.
Gerhalt F9/conditional41 still needs active capacity/life/placement; Peter's
Volcanon join plus selector0 clears only AI bit2, conditional on that producer
being reached. Luke's F513-gated neutral159 at16,13 has max/currentHP0 and neutral
bit3, independent of ally10 membership or F75 following. Its separate physical
entity is written to mapping offset31, while fresh combatant159 mapping63 staysFF.
It is no seventeenth ordinary enemy or guaranteed deployed Luke. Admission and aftermath/later join
retain their own complete scenes and endpoints.

The random list and independent drop table omit13. Six full base definitions
retain gear, resistance/prowess and BLAZE2/HEAL2/BLAST1; Black Monk's selected
healing helper returns HEAL1 while testing the unmasked HEAL2 operand's5 MP cost.
All12 fresh positions miss three ordered regions under72 triangle substitutions.
Same-region2/2 on134/136 hits primary first and does not select a secondary order;
138 is1/15/NONE. NONE permits conditional local standby movement, assigned follow
stops inactive standby, and five15/15 enemies bypass it. Point0(9,24), live128 and
live135 differ; all sets2/6/7/9/14 reach conditional control without a fixed actor.

Production excludes13. Generic descriptive lookup, formation/point/follow/NONE/
standby, Hovering, primary-first/sticky-secondary activation and round mechanisms
exist. Trusted selection, MAGE/ARCHER/range2–2, Monk/Zombie prowess, general gear/
CURSE refresh, attack/healing-spell AI, Gerhalt/Peter/Luke actual classes/join/
control, neutral159/F513 and suspend remain consumer limits. Descriptive
resistance grants no executable attack/spell/status support. No natural state/
action, fixture count, runtime/design/production frontier or retained failure changes.

This owner selects no replacement task automatically. The other early-encounter
limits and unassessed town/state census remain as qualified above; a further
task needs a concrete missing source/caller relationship and product outcome.
Epic1053 and the four frontiers remain unchanged.

### Battle16 Kraken SWARM, live orders and first conditional control

The [dedicated account](./battle16-initialization-and-first-control.md) connects
complete12/11/0/0 and sole Map49 wildcard/crop30×30 to independent416/466/516/88,
actual party/healing/refresh and scene-to-battle replacement. The full intro,
raft/Luke producers and leader/outcome/Hassan boundary keep their own owners.
Random/substitution and independent special-drop tables omit16. Full definitions
Leg10/Arm59/Head87 retain resistance/prowess, no gear, AQUA1 and Head AGI153.
Head's unknownByte0 is not a replacement enemy ID.

Zero regions/points causes no polygon work or assigned-region activation, but
15/15 normal AI bypasses standby. Eight SWARM actors use the selected11-byte
threshold table; the three ordinary legs133/134/138 use set6 directly. ALLIES12
minus2 yields DBF10..0, exactly11 HP-only checks over128–138, fresh count0.
No undeclared phantom deaths follow from the generic wrong-subsection comment.
Damage bypasses waiting; full healing can restore it. STAY/empty movement and
normal-path tested-cache clearing are distinct. Complete15/6 streams consume
live135/131 arm orders conditionally; ordinary AQUATIC water costs and special
Kraken approach costs differ. Head's two round entries do not guarantee first
action/control. No natural state/actor/target/action is asserted.

Production excludes16. Existing descriptive definition lookup, formation,
NONE/live-follow/dead-target promotion, zero-region/15/15 and randomized/extra
turns are reusable. Trusted conversion, SWARM threshold/count/re-wait and exact15
stream, AQUATIC/Kraken costs, Arm/Head ranges, all three prowess expressions,
AQUA execution, arbitrary party/Peter/Luke/control, general refresh, saved-state
and Head special mapping/services remain explicit limits. Descriptive records
do not grant executable resistance/spell/status support. Fixture counters,
retained failures and all four frontiers remain unchanged. Other24–25 limits
remain qualified; this owner selects no next task automatically.

### Battle17 repeat upgrades, live follows and first conditional control

The [dedicated account](./battle17-initialization-and-first-control.md) connects
complete12/14/2/0 to ordered Map69 row9-before17, crop0,31/30×32 and independent
417/467/517/88. Existing Hassan/random-zone/raft/Rohde/outcome owners keep their
boundaries. Twelve templates differ from actual joined/active/dead/capacity/HP/
AI/gear refresh; intermediate status differs from final equipment-derived CURSE.
All fourteen enemy rows and six complete definitions retain gear/spells,
resistance/prowess and movement, without performed-action claims.

F517 admits actual level/category/RNG selection before definition copy.
BRASS_GUNNER Launcher60 explicitly selects melee0..35, with eligible ordinary
candidates60..185 failing signed bounds and returning60 after two draws. It is
not an archer-table upgrade. Gargoyle FLYING count0 reaches the unsafe65,536-byte
exclusion scan; no guaranteed base or successful substitution is invented.
The independent special-drop table omits17; added NOTHING preserves base gear.

All48 finite triangle substitutions miss both regions. Only135/136 assign0;
unassigned1 still participates in region testing/cache. Those two assigned
ally0 follows stop in inactive standby; the other twelve15/15 enemies bypass it.
Live ally1 on128–130 differs from template1/point1;131/132/135/136 use ally0,
and139 follows live141. Complete0/2/6/7 streams distinguish stored follows from
actual MOVE_ORDER consumers:0/6 have none. Monk's selected HEAL2 input can return
HEAL1 while checking5MP. Base round/control and repeated definitions remain
conditional; there is no fixed first actor, action or usable-input promise.

Production excludes17. Existing generic definition lookup, formation, Flying,
standby/live-order/dead-target/round primitives differ from trusted conversion,
F517 upgrades, BRASS_GUNNER/MAGE, ranged/equipped refresh, stun/Golem/Monk prowess,
exact0/2 healing/spell AI, arbitrary parties/control and suspend requirements.
No fixture metrics, original observation, design/production frontier or retained
failure status changes. Other24–25 content limits remain qualified; no next task
is selected automatically.

### Battle18 Taros equipment, regions and first conditional control

The [dedicated account](./battle18-initialization-and-first-control.md) joins
complete12/14/5/0, Map12's wildcard row and independent418/468/518/88 to actual
party/refresh, seven full base definitions and conditional first control.
Entry/warning/before, sword/Rohde producers and outcome/Caravan owners retain
their scope. There is no repeat upgrade. The fresh HP0 neutral159 declaration
writes lookup31, while logical159 reads63; a sprite is not a working mapping.

All96 triangle substitutions miss all five regions, including type3 triangles
and region2's negative projection. Primary-first activation/cache and NONE
versus live-follow standby differ from the seven15/15 actors. Complete2/6/7/9/14
streams retain live128/137/ally0 follows. Taros's LEADER prepass precedes MOV0
and reads the absent point0 through spriteset19's12/15 header. Equipped110 can
select BOLT2 item use without learned spells/MP cost or enemy breakage; that is
not a performed cast. Monk's HEAL2 input retains the HEAL1/check5MP behavior.

The initial script requires Bowie0, ATTACK, target type88 and equipped61;
F112 and the damage toggle have separate lifetimes. Double retains the toggle;
counter resets it, but actual swapped-pointer validation excludes Taros-origin
attacks, so no ordinary bypass follows. Sword loot needs actual held110,
ally-caused target death, range32 zero and clear once-bit24 before removal and
inventory/rare-deals handling. Two Taros entries do not fix actor order or loot.

Production excludes18. Reusable definitions, formation, dual activation, Flying,
LEADER and extra turns differ from trusted admission, type3 region rejection,
signed geometry/cache, out-of-table point access, selected prowess/gear/ranges,
MAGE/BRASS_GUNNER, HEALER3/spell/item AI, Bowie gate/counter/drop/deals and
neutral mapping/services. General refresh, actual parties/control and suspend
remain limited. No fixture count, original observation, design/production
frontier or completed failure is changed; other24–25 limits remain qualified.

### Battle19 repeat selection, Shining Ball and first conditional control

**Confirmed:** the [complete account](./battle19-initialization-and-first-control.md)
joins Map68's sole wildcard19/crop5,19/48×32 and independent419/469/519/88 to
actual party/final-CURSE refresh and12/15/3/1. The Caravan and outcome/village
owners retain scenes, travel/request and return boundaries. Six full definitions
include Orc12, High Priest79 and Dark Madam70 with real gear, HEAL3/BLAST2/FREEZE2,
resistance/prowess and movement/priority versus row activation. F519 selection
starts at E29 and uses original movement/category/bounds/exclusions/retry/RNG;
Launcher is melee fallback, Gargoyle count0 remains unsafe. Base stats/turns do
not establish a repeated roster.

All72 signed-projection/CCR triangle substitutions miss. Tested cache differs
from persistent flags; primary1/0/2 assignment, eight15/15 actors and NONE versus
live-follow standby differ. Declared point0(14,19) is valid; live128/133/135/139/140
and exact2/6/7/9/14 streams retain short-circuit/dead-target conditions. High
Priest HEAL3's current local input`$80` uses erroneous keys C0/A0/80, costs20/3/10
and nonself≥11MP promotion. The selected entry is written before station lookup;
failure carries downgraded`$40/$00` to later candidates, with HEAL1/check5 or3MP and
no further nonself promotion. Learned slots stay unchanged; a new HEAL command
rereads them. PR1245's original16f4233a mechanical PASS and the independent P2
that found this lifetime remain preserved. Slot129's successful AddItem masks
6010→0010; earlier equipped Power Stick also stops the attack-item getter before
Ball. Conditional use consumes a reached Ball slot; independent battle19/row1/
item16/bit6 requires actual target death/held item, no drop RNG, removal and
living-actor capacity or RARE-deals fallback. Leader sweep is not action loot.
No first actor/action, natural Ball use/drop or victory trajectory is established.

Production excludes19. Generic formation/rectangular regions/Flying/LEADER/valid
point/live-order/round mechanisms exist, separately from trusted admission,
selected519 conversion, gear/range/prowess, MAGE/BRASS_GUNNER, HEALER3/healing/
spell/item decisions,16 use/drop/deals and actual party/refresh/control/suspend
requirements. No code, fixture count, original runtime/design/production frontier
or retained failure changes; remaining24–25 limits stay qualified.

### Battle20 Harpy SWARM, neutral Elric and first conditional control

**Confirmed:** the [complete account](./battle20-initialization-and-first-control.md)
joins sole Map51 row20/crop18×26 and independent420/470/520/88 to actual party,
final-CURSE refresh and12/12/0/0. The approach/request, outcome/recruitment and
Elric passage owners retain their boundaries. All24 records and five complete
Harpy44/Madam70/Priest79/Orc12/Launcher60 definitions retain gear/spells,
resistance/prowess and movement. Random-upgrade and special-drop tables omit20.

Zero regions return before geometry/cache/assignment work; all15/15 enemies
bypass normal standby and STARTING declares no wave. SWARM128/134/139 select
thresholds3/2/1; ordinary Harpies137/138 use6. ALLIES12−2/DBF10..0 tests only
HP128–138, omitting actual139 and neutral159; fresh count0. Damage bypasses the
gate and full healing may restore waiting. STAY/empty movement precedes normal
cache clearing/order work; temporary obstructions clear on every exit. Exact
2/6/9/15 streams and live131/129/133/136 orders retain dead-target promotion and
earlier-success short-circuits. No fixed point is selected. DISPEL's actual
SUPPORT/5MP/area-score≥2/station path and Priest's corrected candidate-local
HEAL3 downgrade differ from learned-slot descriptions. No actor/action is fixed.

F520-clear neutral159 hasHP0, X/Y6,6 and physical ENTNUM, but writes lookup31
while159 reads63/FF on fresh state. Its immersion/double-speed four-facing loop
does not establish rendered timing or Elric13 joined/active45/recruitment.
Production excludes20. Flying/live-order/dead-target/zero-region/round support
is reusable, separately from trusted conversion, SWARM thresholds/count/re-wait/
stream15, HEALER3/healing/DISPEL/spell decisions, poison/Orc prowess, gear/ranges,
MAGE/BRASS_GUNNER, neutral mapping/services and party/status/control/suspend.
No fixture metric, four-frontier or retained failure changes; remaining24–25
limits stay qualified. This does not select another task automatically.

### Battle21 regions, repeat upgrades, item custody and first control

**Confirmed:** the [complete account](./battle21-initialization-and-first-control.md)
joins sole Map67 row21/crop21,0/32×42 and independent421/471/521/88 to actual
party/final-CURSE refresh and12/12/4/0. Opening/outcome owners retain scenes,
leader sweep and return. All24 records and six full Skeleton17/Orc12/Harpy44/
Launcher60/Madam70/Priest79 bases retain gear/spells/resistance/prowess/movement
and Skeleton's ENHANCED field. All96 fresh signed-projection triangle cases
miss four exact regions; cache/tested flags, primary0/1/2/3 versus15/15, NONE
standby versus live128/131/135 and exact0/2/6/9 streams remain distinct.
Harpy names do not install SWARM. Learned DISPEL SUPPORT and candidate-local
HEAL3 require their actual filters, earlier success and station consumers.

F521 enables E31/category/bounds/exclusions/two-attempt upgrades. BRASS_GUNNER
uses melee and falls back; FLYING count0 remains unsafe. No repeated roster
is asserted. AddItem807F strips Herb2000 AI-use and Ring4094 equipped/drop
metadata; copied Sword/Stick remain equipped, added Ring is not and gives no
DEF+5. Actual item/HEAL/SUPPORT/attack filters and earlier-slot termination do
not establish automatic use. Separate target135 held62/once7 and129 held20/
once8 drops require an ally actor and action death; neither selected item
uses drop RNG. Removal, capacity/actorHP, Ring RARE deals, once-state and
conditional use/break/consumption/MP consumers do not guarantee loot.

Production excludes21. Existing formation/live-order/Flying/region/round
mechanisms are reusable, separately from trusted conversion, signed geometry/
cache, ENHANCED/gear/movers/prowess, exact0/2/healing/DISPEL/spell/item/drop/
repeat and actual party/refresh/control/suspend gaps. No fixture metric, four-
frontier or completed failure changes; natural state/actions/loot remain
Unknown and24–25 content limits remain qualified. No next task is selected.

### Battle22 chess SWARM, dual neutrals and first conditional control

**Confirmed:** the [complete account](./battle22-initialization-and-first-control.md)
joins sole Map22 row22/crop9,0/30×30, independent422/472/522/88, empty hooks,
actual party/final-CURSE refresh and12/16/0/1. All28 records, point0(15,5) and
six full King89/Queen71/Bishop80/Knight14/Rook61/Pawn13 definitions retain raw
unknownByte (King0), spellPower, gear/spells/resistance/prowess/movement/AI and
refresh effects. No random upgrade/special drop or hidden/region wave selects22.

Zero regions returns before geometry/cache mutation;15/15 permits ordinary AI.
Actual SWARM132/133/135/136/137/138/139/141/142 use1/3/1/3/6/6/1/3/3. Bishop131's
table6 does not gate HEALER4. ALLIES12−2/DBF counts onlyHP0 on128–138, omitting
five actual enemies139–143. Fresh count0, damage bypass and re-healed wait remain
conditional; wait precedes normal cache clear/order promotion. Exact2/3/7/9/14/15
streams distinguish HEALER3 MOVE2 from HEALER4 MOVE1. Valid King point0 and live
128/137/136/132/134 differ; Bishop131 can promote secondary128. Learned HEAL3
retains candidate-local80→40/00 behavior; BLAZE/FREEZE/BLAST and copied equipped
gear require actual usability/MP/effect consumers. KingAGI161 gives ordinary33
plus extra score, independently of leadership/SWARM; no first actor is fixed.

F522-clear neutral159 Ponpei/158 Maid receiveHP0 and distinct physical cursors;
predecrement writes31/30 differ from ordinary readers63/62. They are not King128
or party members; both round and SWARM exclusion reasons remain explicit.
Production excludes22. Existing LEADER/valid-point/live-order/zero-region/round
mechanisms are reusable, separately from trusted conversion, SWARM count/threshold/
re-wait/exact15, exact2/3 AI, gear/prowess/enhanced/spell/mover, neutral and actual
party/status/control/suspend gaps. No fixture metric, four-frontier or completed
failure changes; natural state/actions remain Unknown and24–25 limits qualified.

### Battle23 region activation, AQUA2 and first conditional control

**Confirmed:** the [complete account](./battle23-initialization-and-first-control.md)
joins sole Map27 row23/crop23,14/27×16 and independent423/473/523/88 to empty
hooks, actual party/final-CURSE and12/12/1/1. All24 records/306bytes, point0(22,3)
and full ordinal Willard90(raw0)/Rat15/Ooze16 definitions retain stats, resistance,
prowess, spellPower, four items/spells and actual difficulty refresh. All copied/
row items are NOTHING. Random-upgrade, special-drop and neutral tables omit23;
halfEXP selects1 and after-leader positioning5, separately from timer44.

All24 signed-projection/CCR triangle substitutions miss region0's exact
(9,0),(9,15),(26,15),(26,0). Tested misses, persistentF90, normal AI cache reset
and later/saved positions remain distinct; extra160 is the conditional deals
alias, not another enemy. Assigned0/15 actors131–136 differ from six15/15;
NONE136 can build standby movement, while assigned live follows131–135 cannot.
Raw0 follows live ally0, not point0/enemy128;129/130 follow128 and138 follows137.
Complete6/7/9/14 streams retain prepasses, dead-target promotion, no MOVE_ORDER
on6 and earlier-success conditions; no SWARM/hidden wave/region script is selected.

Ooze learns AQUA2/`$68`, MP0/range1–1/radius0/power15/BUBBLE_BREATH variation2,
TYPE_ATTACK without AFFECTEDBYSILENCE. Ordinary enemy usability retains level2
atMP0; confused/ally whitelist excludes AQUA. Full`$68` stored in ATTACK_COMMAND_SPELL
misses exact`$28` AQUA1 shortcut: with spell/physical candidates RNG6 selects spell
on2/4, otherwise physical; spell-only still draws. Priorities/stations/paths and
earlier order success remain necessary. Willard enhanced has no learned spell.
Willard165 gives ordinary37/extra30 bases, not a fixed first actor or usable input.

Production excludes23. Existing STEALTH, LEADER, valid point/live-follow,
standby and round mechanisms are reusable; precise gaps are trusted selection,
signed geometry/cache lifetime, ENHANCED/prowess/AQUA decisions/effects, actual
party/refresh/status/control/suspend and actor/services composition. No new
fixture metric, natural observation, runtime/design/production frontier or
completed failure change follows. Remaining24–25 limits stay qualified.

## Verification boundary

Read complete root README, documentation index and source-coverage, then the
complete related owner and relevant complete sections of adjacent chapter owners,
actual table/producer records and production owners for asserted relationships.
Check negative claims against declared scope as well as positive references.
Filenames/keywords alone cannot prove coverage.

Direct checks cover links/anchors, UTF-8/LF, exact declared path scope, unchanged
fixture-counter table and four-frontier section, finite source selectors/callers,
current production relationships and private boundary. The ignored recipe and semantic claim-to-owner review are
provided in the Issue handoff with full dependencies; use a fresh receipt name.
On clean committed head load existing private config in the same process, then
run `uv run sf2 verify plan --base origin/main --head HEAD` and inspect actual
exact-head scoped CI. Normal/full, SDK/Godot/emulator/H3/H4, screenshots, old
recipes/comparisons/lowering and acquisition are outside this documentation
acceptance. Mechanical PASS is not whole-game coverage or independent acceptance.
