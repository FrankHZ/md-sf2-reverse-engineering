# Creed Return, Fairy Following and Recruitment Choices

After the consultation, the shrine has an exit back to Creed Mansion. In the
usual supplied post-consultation room state, Goliath says his master wants to
help and asks the party to choose a companion. Creed suggests looking for
storytellers in Tristan. The Fairy offers to travel with the party, and Eric,
Randolf, Tyrin and Karna each have a recruitment conversation.

**Inferred:** these conversations form the room's help and companion choices.
**Confirmed:** the route, dialogue branches and ordered operations below are
accepted static evidence. They do not establish a naturally walked return,
completed following, visible shrinking, usable input or saved choice.
**Unknown:** actual supplied state and those runtime outcomes.

This guide continues [the consultation and conditional zone return](evil-spirit-consultation-and-zone-return.md)
and consumes [accepted #953 research](../../research/creed-return-fairy-and-recruitment-choices.md)
on accepted main `17718cd3ece7dc087737f3a6bea4eb463fd4553f`.
It stops at the immediate room interaction returns. The later Oddler departure
scene continues in [Oddler stays and the Fairy handoff](oddler-stays-and-fairy-handoff.md),
which explains its current actors, ordered caller flags and later fresh population.
This Layer B explanation selects no remake rule or new observation work.

## Returning to the Room

**Confirmed:** Map34's exit at `(26,28)` requests an explicit, no-scroll entry
to Map33 at `(9,5)`, facing Right. The table tests no consultation flag. Its
other two warps use CURRENT255 for internal shrine transfers; they preserve
population rather than reentering the mansion. The [route owner](../../research/evil-spirit-shrine-entry-and-scene-admission.md#return-only-local-surfaces-and-the-three-warps)
retains the working-layout warp block, admitted movement, request-consumption
and inherited coordinate-high-byte premises. F784 after consultation is
compatible with returning; it does not prove that the player walked this exit.

Explicit mansion entry rebuilds actors and loads the selected layout and
mapsprites. It clears temporary flags 256–383 and sets F80, preserving the
room's F22/F67–69/F759/F762/F774/F783–787 and joined/active flags. CURRENT255
reuse instead keeps current actors and layout. Each setup accessor reads
current flags again, so population, init and interaction can disagree when
flags change between calls.

The [population and init owner](../../research/creed-return-fairy-and-recruitment-choices.md#return-population-and-init)
uses last-set-wins selection: default, F523, F783, F784, F786, then F22.
F783 selects baseline again; F22 overrides even F786. All selections share
the room's interaction table, but do not share the same residents or init.

| Supplied room state | Fresh population and selected init |
| --- | --- |
| F784 set, F786/F22 clear | Goliath128 at `(17,14)` Up, Creed129 at `(17,5)` Down, Fairy130 at `(17,10)` Up, then four initially parked recruits. Normal init has the placement tail below. |
| F786 set, F22 clear | Walking worker128 at `(22,4)` Up, Creed129 at `(17,5)` Down, Oddler130 at `(17,7)` Up. No recruit rows; alternate init only returns. Its source label mentioning312 does not select F312. |
| F22 set | Six baseline rows, initially `(63,63)`: worker128, Creed129 and the four recruits, with no resident130. Normal init places128 at `(14,14)` Up and independently tests F784 for recruit placement. |
| F784 clear, no later override | Default/F783 gives baseline; F523 alone gives the restoration population. The independent F523-set/F783-clear restoration gate still applies. |

Normal init first checks F523 set/F783 clear for restoration; only its return
sets F783 and EGRESS_MAP=Hassan9. F783 already set skips that scene and the
Egress write. After the Goliath placement alternatives, F784 independently
admits four joined-flag checks. F787 is not an init gate.

| Recruit | Joined flag tested | Requested position when clear |
| --- | --- | --- |
| Eric14 | F14 | `(15,14)` Down |
| Randolf16 | F16 | `(16,15)` Down |
| Tyrin17 | F17 | `(18,15)` Down |
| Karna15 | F15 | `(19,14)` Down |

These placement commands skip zero-HP allies. Fixed ally-row allocation instead
skips an already nonzero lookup, without testing joined or active membership.
Thus an unjoined flag alone does not prove a visible, reachable recruit. Actual
class/HP selects the ally sprite, potentially a blue flame at HP 0; exploration
healing covers living/immortal allies rather than every dead recruit.

Logical identity is also separate from physical allocation. With only F67/F68
ordinary followers, Peter7 and B31/Oddler occupy physical 1/2 before the seven
residents occupy 3–9. Other follower selectors change this example. Repeated B31
rows can overwrite its lookup without deduplication. Missing ordinary mappings
can alias physical 0; hidden mappings are -1 and are not repaired locally.
F69's Fairy is FOLLOWER_C158, lookup offset 62, distinct from resident130.
The [restoration owner](../../research/creed-restoration-and-basement-departure.md#admission-setup-selection-and-population)
retains mode, raft, excessive-population and HP/alias limits. F784 alone proves
neither completed restoration nor consultation.

Entry is not automatically a usable return. Init supplies no zone-style
physical 0 coordinate wait. Palette/music and WaitForEvent must return, and a
pending map event takes priority over controller installation. The local
interactions change neither EGRESS_MAP nor PLAYER_TYPE.

## Goliath, Creed and the Changed Room

**Confirmed:** the seven specific callbacks comprise Goliath128, Creed129,
resident130 and the four recruits. The default callback only returns. Their
[complete decision and speaker owner](../../research/creed-return-fairy-and-recruitment-choices.md#complete-interaction-table-and-callbacks)
keeps the exact IDs and source branches; this guide paraphrases their content.

Goliath checks F22 first. In that branch he sobs, claims Oddler regained memory
and went berserk, says Creed understood the boy's fearful face, and reports a
fall into the sea while asking whether Creed lives. Without F22, F786 selects
questions about recovered memory and a severe headache. Otherwise F784 selects
his master's offer of help and request to choose one companion; F783 selects
his response to Zeon's awakening; the fallback rejects the visit.

Creed checks F786, without a separate F22 test. Set selects his claim that
devils cannot help and people must seal Zeon. Clear selects his suspicion that
the jewel has a secret, advice to ask Tristan's storytellers, and directions to
Tristan. These are attributed reports and advice, not implemented travel or
proof that later disasters happened.

Resident130 checks F786 before anything else. Set selects a greeting to the
leader and a headache report, then returns. Fresh F786 population makes that
actor Oddler; a retained Fairy with newly changed flags can speak the same
branch. A fresh F22 baseline has no resident130. Callback identity alone is
therefore insufficient to label the actual speaker Fairy.

## The Fairy's Decision and Current Following

**Confirmed:** outside the F786 branch, F759 chooses the Fairy's preceding
speech. Clear announces that she is going east with the party. Set asks about
a sick dwarf, offers help and asks to be taken there. Both paths close the text
and call the same scene, `cs_5A828`. There is no yes/no menu or force join.
The [Fairy operation owner](../../research/creed-return-fairy-and-recruitment-choices.md#fairy-live-block-copy-and-current-following)
keeps narration, layout, HP and following separate.

The scene first copies the live layout word at `(17,13)` into `(9,5)` and
requests view-plane updates. This copies current tile/collision bits, not a
fixed tile or warp-table change. Earlier mutations, including F22's separate
block copy, can change the source word. Neither the actual image nor completed
redraw follows from the command. Fresh Map33's flag-copy table has no F762/F785
replay of this one-cell copy.

Narration then states the leader's decision to take the Fairy, followed by
`followEntity130,FOLLOWER_B,2`. Its encoded follower word is `$0082`, but the
HP guard reads its high byte 0: it tests Bowie HP, not Fairy HP. Bowie HP 0 skips
the follow operation after the copy and narration. Normal scene return can
still reach caller F762 then F785. The displayed LEADER name may belong to the
first active member rather than Bowie; that does not change the HP guard.

When admitted, following resolves resident130 and B31 and installs an action
with offset `(-24,0)`. It appends the physical follower only if absent from the
exploration list, but a repeated call still allocates/replaces its action.
The action uses the target's actual coordinates, destination and facing;
installation does not instantly place Fairy behind Oddler. Missing B can
resolve to physical 0, aliases can self-follow, and no local operation creates
missing actors or repairs invalid lookups. There is no follower-settle wait.

Only after scene return does the callback set F762, then F785. These writes
are non-atomic and have no repeat guard. They do not set F69, clear F68 or grant
force membership. Ordinary activation excludes listed exploration followers;
a direct callback may repeat, and a skipped follow has not established that
exclusion. Fresh entry discards this current action/list allocation and
reselects residents: F762/F785 do not suppress the F784 Fairy row or create
FOLLOWER_C158. Static flag survival is not a verified save or permanent follow.
The healing offer likewise does not heal the dwarf in this interaction.

## Four Companions, Two Forms of Each Offer

**Confirmed:** each recruit first checks F22. Set selects the later offer even
when F787 is already set. Otherwise F787 clear selects the initial offer;
F787 set gives that actor's initial refusal text without a prompt. No callback
requires F784, completed restoration/consultation, unjoined membership, living
HP or available active-party capacity. The [eight-offer owner](../../research/creed-return-fairy-and-recruitment-choices.md#eight-offers-and-ordered-successful-branches)
retains each program, prompt/refusal ID and success target.

| Companion | Initial offer and No response | F22 offer and No response |
| --- | --- | --- |
| Eric | A knight searching for a sword asks to be chosen; on refusal he wishes the party happiness. | He wants a legendary sword and asks to join; on refusal he proposes searching alone. |
| Randolf | He names himself, claims exceptional warrior strength and offers to go; on refusal he calls life here comfortable. | He says his original size returned and considers going with the party; on refusal he will stay. |
| Tyrin | A mage trained in Ponpei offers help; on refusal he says his magic could help. | He says he is free and asks to join; on refusal he warns of regret. |
| Karna | A young priest asks to leave with the party; she insults a refusal. | She celebrates freedom and asks to come along; on refusal she calls the choice rude. |

Each offer displays its two speech entries, then a real yes/no choice. Default
and Left select Yes 0; Right selects No -1. A/C accept the current choice; B
chooses No. Initially held input requires release before the menu loop.
Returning Yes sets F89; nonzero No clears it, overwriting a stale F89.
Both paths include the source sleep 10. A set-F89 branch reaches success;
No displays the refusal and ends without join, F787, shrinking or following.
Dialogue W1/W2 waits are separate from this menu.

**Inferred:** the initial conversation presents choosing one helper, while
F22 offers express later freedom to recruit others. **Confirmed:** the local
rule is F22/F787 branching, not a universal one-recruit limit. A fresh F784
entry can place unjoined recruits even with F787 set, but their initial
callbacks then give refusal text. F22 bypasses that suppression. Already joined
actors can receive another offer if supplied/reachable under these branches;
fresh init parking and ordinary follower exclusion can prevent natural talk.
Sword searches, restored size, strength, freedom and destinations are speech;
these callbacks grant no sword/equipment, class change, healing or AI.

## What a Yes Changes, and When

**Confirmed:** both forms join the selected ally first. `JoinForce` sets its
joined flag, rebuilds joined/active/reserve lists and counts through UpdateForce
without HP filtering, then sets its active flag only if the rebuilt active
count is below 12. At 12 or above it skips that active write; it does not reject
joining, displace another ally or clear an existing active flag.

| Ally | Joined flag set | Active flag conditionally set |
| --- | --- | --- |
| Eric14 | F14 | F46 |
| Randolf16 | F16 | F48 |
| Tyrin17 | F17 | F49 |
| Karna15 | F15 | F47 |

UpdateForce runs before the active write, with no second rebuild in this join
tail. A newly active ally can remain in the prior reserve/count snapshot until
another consumer rebuilds it. An already active ally is counted once and may
have its flag set again. Full party or HP 0 does not prevent the remaining
acknowledgement, F787, initial shrink operations or selected following.

The join handler requests join music, reads the ally's current name/class,
and requests generic text 446. Its acknowledgement schedules previous music
and waits for nonzero standard direction/button input, without requiring a new
input edge; the function name does not prove a screen fade. Text closes and
sleep 10 precedes handler return. Only then does the scene set F787. Membership
can therefore change while F787 is still clear if acknowledgement stalls or
is interrupted. The [join/capacity owner](../../research/creed-return-fairy-and-recruitment-choices.md#join-and-capacity-are-separate-from-following)
and [party contract](../contracts/party-roster-state.md#handler-boundary)
retain the call, snapshot and input boundaries.

For an initial Yes, F787 is followed by the three other actors' shrink action
installations, in the exact order below. The first two are asynchronous; only
the third is waited. The selected recruit's following request comes afterward.
These named targets are not filtered by joined/active membership or HP.

| Selected recruit | First asynchronous | Second asynchronous | Third, waited |
| --- | --- | --- | --- |
| Eric | Randolf | Tyrin | Karna |
| Randolf | Eric | Tyrin | Karna |
| Tyrin | Eric | Randolf | Karna |
| Karna | Eric | Randolf | Tyrin |

Every F22 Yes instead sets F787 after joining, requests selected following and
ends, with no other-actor shrink operations. Both forms use the current last
listed exploration follower as the target, or physical 0 if the list is empty.
They do not expressly choose Fairy, Peter or Oddler. Fairy-first and
recruit-first interactions can therefore give different targets. This command
has no HP guard, creates no absent actor, and sets no permanent follower
selector. If the selected actor is already last, it can follow itself;
deduplication avoids a second list entry but still allocates a new action.
Following remains separate from force membership and active-party capacity.

## Shrinking Parks Actors; It Does Not Remove Membership

**Confirmed:** the [complete shrink/action owner](../../research/creed-return-fairy-and-recruitment-choices.md#complete-shrink-action-and-completion-boundary)
starts with wait 5 and resizing enabled. Nine facing/size updates turn
Down/Right/Up/Left repeatedly, ending Down, with size operands 22,20,18,16,14,
12,10,8,6 and wait 5 after each update. Its full tail sets position `(63,63)`,
disables resizing, writes size 18 and jumps to Idle. These are source operands
and sprite-service requests, not measured pixel sizes or frame durations.
The final size 18 write has no following updateSprite in this body.

Positioning writes current and destination coordinates, scaled by384 to
`$5E80`, and clears travel, without clearing velocity. It neither invalidates
lookups, removes exploration-list entries, clears membership nor frees actor
slots. `$5E80` is below the entity-loop skip threshold `$7000`, allowing the
remaining tail to run. HideEntity instead parks at `$7000`, invalidates the
lookup and removes the list entry; its retained action pointer is dormant.
Shrink and hide therefore have different state consequences.

The waited installation polls only its own actor's pointer for exact Idle.
It does not join all three shrink actions. Actor indices, prior state, sprite
load pressure and update availability can alter progression. Aliased targets
can replace earlier installations on the same physical slot; hidden/invalid
mappings are not repaired. A stalled wait does not undo joining or F787, and
one actor reaching Idle proves neither all three completion nor rendered absence.

## Text, Speakers and the Interaction Return

**Confirmed:** Goliath's wrapper turns him toward the player and restores saved
facing on return; the other six specific rows turn without that restoration.
These flags are action behavior, not admission-facing requirements. Actual
activation still needs the view/facing, layout/counter reach, nearby actor and
inverse-lookup conditions. Listed followers are excluded. Calling a callback
directly is a different premise from ordinary talk.

The wrapper selects portrait/speech from the mapped actor's actual sprite.
Baseline worker and Fairy have no portrait, Creed and Oddler have theirs;
recruit class/HP/sprite differences can change the portrait and speech sound.
Recruit scripts use their named actor with modifier0; Fairy's decision uses
`$FFFF` narration, bypassing actor lookup and selecting speech SFX0.
Requested portraits are not rendered evidence. The [text/control owner](../../research/creed-return-fairy-and-recruitment-choices.md#exact-text-joins-and-presentation-limits)
retains all 49 room text sites, 45 distinct room IDs and generic join 446 without
a duplicated transcript here.

Explicit name controls read stored ally names; they do not rename recruits.
LEADER rebuilds the force and uses its first active member's name. Join446
reads current NAME/CLASS and has no W1/W2 token; its separate acknowledgement
provides the input wait. N is newline, W1/W2 wait for input, and W2 adds
continuation-arrow/validation-sound behavior. Continuing text and single text
retain different close/sleep behavior even when a literal ends W1.

The nominal Fairy cursor ends 1904. Initial No ends 1910/1913/1916/1919;
F22 No ends 1923/1926/1929/1932. Yes skips the refusal with the cursor at its ID.
Debug/P2 Start may latch text skipping after interpreter entry clears it;
display and cursor advancement can then be omitted while yesNo, joining and
other commands still run. Nominal text/cursor order is not proof that every
line appeared or asynchronous actors completed. The [dialogue contract](../contracts/dialogue-system.md#confirmed-static-contract)
retains the handler and bounded service scope.

The wrapper disables entity updates around the callback; the script bridge
reactivates them for action execution. On normal callback return, windows close
and updates reactivate. There is no all-follower settle, verified save or
observation of usable controller input at this return.

## Interrupted State and the Next Boundary

**Confirmed:** these operations are ordered, not atomic. Fairy copy, narration
and following can be partial before caller flags; interruption between writes
can leave F762 set with F785 unchanged. A join acknowledgement can leave joined
or active flags changed with F787 clear. After F787, shrinking or following
can remain unfinished even though subsequent initial offers are suppressed.
Retained actors with changed flags and fresh reconstructed actors are distinct
premises throughout.

At `(15,21)`, the later zone first tests F774. Clear selects the earlier Goliath
scene and returns without falling through. F774 set, F785 set and F786 clear
admit `cs_5B466`. Only after that separate scene returns does its caller set
F786, set F69 and clear F68, in order. It tests neither F787 nor recruit joined
flags, so recruitment is not a local prerequisite. The [next-boundary owner](../../research/creed-return-fairy-and-recruitment-choices.md#changed-premises-return-and-next-boundary)
identifies this admission only; no unaccepted Oddler-scene internals or natural
zone reach are inferred. Current resident130 following is not already that
later Fairy/Oddler handoff.

**Unknown:** naturally reached return/talk/zone requests; actual flags, HP,
class, mode, layout, population, mappings and membership/list/count snapshots;
unshimmed service/action completion, presentation/audio, usable control and
save/load persistence. These limits do not erase confirmed ordered source
behavior, and do not create an automatic emulator or acquisition queue.

## Evidence Review and Retained Results

The pre-synthesis review checked accepted Layer A against selected public
payloads and consumers, keeping source structure separate from runtime reach:

| Reviewed surface | Disposition and claim boundary |
| --- | --- |
| [#953 research](../../research/creed-return-fairy-and-recruitment-choices.md), accepted shrine/restoration predecessors | Complete room route, seven callbacks/eight offers, text and actual HP/alias premises; no universal recruitment rule or later-scene inference. |
| `sf2-map-events-static-v1`: [routing-setup](../../../tests/fixtures/h2/map-events/routing-setup.json) and [entity-programs](../../../tests/fixtures/h2/map-events/entity-programs.json) | Eight room rows/six entity route joins and all 89 callback operations; F22/F787 priority, F786/F759 speech and ordered762/785 agree. |
| `sf2-map-init-static-v1`: [map-init fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Normal 58-operation body and F786 alternate return; F523/F783 and F22/F784 branches, joined tests and exact placements agree. |
| `sf2-map-script-engine-static-v1`: [map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | Selected join, addNewFollower, yesNo, followEntity, action bridge, hide and live-block-copy handler facts; this public fixture is not the full private programCorpus. |
| [Event shard/schema consumer](../../../src/sf2tool/h2/map_events_fixture.py), [event verifier](../../../src/sf2tool/h2/map_events.py), [script verifier](../../../src/sf2tool/h2/map_script_engine.py), [map-script schema](../../../schemas/h2/map-script-engine-fixture.schema.json) | Selected shards/sections and map-script fixture schema-validated; provenance and complete expected equality are stronger than legal shape. No fresh source/output comparison is claimed. |
| [Party/roster](../contracts/party-roster-state.md) and [dialogue](../contracts/dialogue-system.md) contracts | Handler-local snapshots, follow allocation and text services retain their controlled scopes; broad capacity/persistence questions do not override the accepted local source/count order. |

No blocking contradiction was found. The stricter research boundaries govern
current follow versus persistent selectors, source waits versus completed
presentation, and local force capacity versus broader lifecycle Unknowns.
Contradictions would return to the research owner without Layer B research edits.

The research pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [reproduction owner](../../research/creed-return-fairy-and-recruitment-choices.md#reproduction-and-review-boundary)
selects the complete 17-program/105-record body, 49 text joins, shrink consumer
and init/population evidence. This checkout lacks that pinned upstream and
retained pair; preserve accepted evidence without acquisition, regeneration
or a fresh comparison claim. Narrow evidence commands remain
`uv run sf2 h2 map-init`, `uv run sf2 h2 map-events` and `uv run sf2 h2 map-script-engine`.

[#953's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178749558),
[rebase receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178813915)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/956#issuecomment-6178823238)
retain the unchanged 18,285,979-byte pair comparison 0.081s, original
CI 38639160994 and rebased CI 38639720868, complete joins, corrected
FOLLOWER_C158/shrink facts and inspection diagnostics. Both original and
rebased results remain retained, including the acceptance inspector's corrected
whitespace regex; none was an interrupted suite.

[#954's corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)
retains its original and corrected CI and the distinction between callback
invocation and a skipped consultation scene/caller writes. The linked
[#949 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949#issuecomment-6178414274),
[#945 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849),
[#941 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)
and [#937 corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
retain earlier comparisons, corrected text/high-byte/dormant-slot facts,
completed failures and Unknowns. No completed-failure restart, blanket local
suite, runtime or translation check is introduced by this prose.
