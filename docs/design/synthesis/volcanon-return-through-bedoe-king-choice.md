# Volcanon Return through the Bedoe King Choice

Back in Bedoe, the king asks whether Bowie will take up Volcanon's demand to defeat Zeon.
Accepting leads to advice about Creed and a requested departure by Luke to arrange a raft.
Refusing leads to an offer to remain in Bedoe; a later interaction after acceptance gives only
an encouragement. The speech, Luke's local departure and the caller's flag write are separate
outcomes. None establishes that a raft is prepared or available.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/volcanon-return-through-bedoe-king-choice.md) supports the selected
return requests, prompt and actor/service boundaries below. **Inferred:** this is an optional
content continuation of [the shrine audience](bedoe-to-volcanon-audience.md). **Unknown:** natural
floor travel, actual supplied state, input/service completion, presentation and playable continuity.
This guide adds no remake capability or product decision. Fnnn denotes flag nnn; coordinates are
map tiles unless specified.

Stop after the first selected king response, completed caller and conditional control handoff.
Bedoe exit, Polca arrival or raft service, hatchling/Skreech/rescue, Luke recruitment and Battle13
remain outside this guide. The proposed journey is attributed advice, not an established route.

## Returning to Bedoe and Selecting Its Changed Setup

**Confirmed selected requests:** two Map11 warp rows describe return discriminators:

| Matching point on Map11 | No-scroll request |
| --- | --- |
| Any X, Y19 | Current map (`255`), destination `(8,13)`, Down. |
| X8, Y34 | Explicit Bedoe/Map10, destination `(45,50)`, Down. |

Neither row tests F722, F799, membership or facing. Down is the arrival request, not a prerequisite.
The current-map destination does not prove a walk to `(8,34)` or produce the next request itself.
The movement producer needs layout attribute masked to `$1000`; ordered row matching uses tile
coordinates derived from 384 internal units per tile and negative-coordinate wildcards. Pending
consumption clears the event type before dispatch. The no-scroll consumer requests sound, idles
main and returns map/position/facing to the main loop. Map10/11 have no selected flag-switched
replacement or battle-map row, so these requests select exploration independently of battle unlocks,
with the current-map row assumed to remain Map11.

Exploration entry heals living/immortal allies and fades. A current-map request updates main and
raft/follower placement, loads the map and runs init while retaining fixed population and mappings;
Map11 init simply returns. Explicit Map10 entry loads the selected setup and rebuilds mappings and
population, then proceeds through map/VInt/init services. Entry healing proves neither an actor's
presence nor HP at the later interaction.

F722 alone selects Bedoe's alternate setup, including entities, entity events, zone events and init;
descriptions and item events are shared. Each setup lookup resolves the current flag again. The
shrine caller writes F722 only after its script returns: merely hearing part of the audience does
not establish this selection. F722-clear supplied state retains the default setup. Entry and an
F722 write do not automatically invoke the king's interaction; F799 does not select the setup.

## Luke, Peter and the King Depend on Current Mappings

**Confirmed population boundary:** the alternate fixed rows declare Luke as ally lookup 10 at
`(38,51)`, Down, and KING_BEDOE as regular NPC lookup 147 at `(36,49)`, Down, both with `eas_Init`.
These lookups are not physical slot numbers. Followers are allocated first; Map10 admits them and
is not an overworld map, so F65 does not prepend the caravan.

| Supplied flags at a fresh Map10 rebuild | Population consequence |
| --- | --- |
| F75 clear | The fixed Luke row creates lookup 10 at its declared coordinate. |
| F75 set | Luke is declared as a follower first; the already mapped ally is not redeclared by the fixed row, whose action-pointer operand is still consumed. The scene therefore starts from the follower's actual placement/action. |
| F67 set | Peter lookup 7 is supplied by the follower declaration. |
| F67 clear | Peter has no fixed row; absent a later operation, his cleared lookup resolves to physical 0. |

Follower declaration tests neither force/active membership, word HP nor force AI. The preceding
shrine joining/AI services did not set F67; they cannot substitute for a Peter mapping here.

The F722-selected init independently tests F799 and F724. F799 conditionally moves lookup 10 out
of map; F724 conditionally moves lookup 152 out of map. It writes neither flag. F724's producer and
scene are outside scope; it neither selects this choice nor controls Luke's removal. Default init
is a separate owner, so supplied F799 does not invoke this alternate init while F722 is clear.

`MoveEntityOutOfMap` writes current and destination X/Y to internal `$7000,$7000` through
`SetEntityPosition`. It leaves mappings and the follower list intact, including for F75-supplied
Luke. A fresh rebuild creates new mapping state before this coordinate-only removal; it does not
reuse the acceptance scene's invalidated lookup. F799-set is therefore not a complete scene receipt.

## Addressing the King Does Not Require Down Facing

**Confirmed ordinary activation:** C takes the entity-activation route after debug/caravan
alternatives; A opens the menu. Main is idled and physical 0's current/destination coordinates and
view scrolling are waited first. Activation uses the current view target and facing to address a
tile, extends across counter tiles, excludes the view target/listed followers and takes the first
entity within Manhattan distance less than 384 internal units of the addressed point. Negative view
target/no match does not enter the successful path. Normal view target is physical 0; natural access
to the king is still **Unknown**.

The activated physical actor must reverse-map to lookup 147 for the ordinary king case. Ordered
reverse lookup, fallback to lookup 0, mismatched or aliased mappings can select another event.
The alternate event row encodes lookup 147 and `DOWN=3`. The consumer matches the lookup, then
interprets the second byte as bits; it does not test king or player facing against Down:

- Bit 0 waits one VInt and faces the mapped actor opposite the supplied player-facing byte,
  `(playerFacing+2)&3`.
- Bit 1 restores the saved physical actor's pre-interaction facing after caller return.

The wrapper stores player facing as `EVENT_RELATIVE_POSITION` and selects speech properties from
the current mapped sprite. Normal KING_BEDOE supplies portrait NONE and `DIALOG_BLEEP_7`; changed
sprites/mappings can change that selection. These are service inputs, not observed presentation.
It disables entity updates before the caller. The script's trap separately enables updates before
executing its waited actions; refusal/repeat do not invoke that trap.

For contrast, the default F722-clear row encodes 147, `UP=1`, and requests text1678 without an F799
test or prompt. It faces the actor toward the player but does not restore saved facing. The alternate
choice itself has no independent F720/F721 prerequisite.

## The Question, Refusal and Repeat Response

**Confirmed caller content:** with F799 clear, the king says Volcanon has abandoned the
earth-dwellers (1603), then asks whether Bowie will kill Zeon as demanded (1604). The caller
invokes the actual input-aware `j_YesNoPrompt` / `YesNoPrompt`, rather than the alternate entry or
the script `yesNo`/F89 service.

| State or prompt result | Selected response |
| --- | --- |
| F799 clear; confirmed Yes, D0 word 0 | Run the acceptance scene `cs_5797A`; after successful script return, set F799 and return. |
| F799 clear; confirmed No or B cancel, D0 word `-1` (`$FFFF`) | King offers a stay in Bedoe (1605) and claims it is safe (1606); return without scene, Luke action or F799 write. |
| F799 clear; no confirming input | Prompt remains pending; no returned result or automatic acceptance. |
| F799 set | King wishes the leader luck (1614); no prompt, scene or F799 write. |

The caller branches on any nonzero D0 word, but the prompt normally returns only 0 or `-1`.
No and cancellation cannot be distinguished through that return. Refusal leaves this caller's F799
clear, so a later eligible interaction can offer the question again. The king's assurance of safety
is attributed speech, not a protection rule.

The prompt starts with Yes selected. It snapshots `PLAYER_1_INPUT`, opens/loads the prompt, moves
existing dialogue/gold windows and waits for their movement. A nonzero entry snapshot requires
`PLAYER_1_INPUT` to become zero before the input loop; a zero snapshot bypasses that release loop.
The loop checks `CURRENT_PLAYER_INPUT` in order Left, Right, B, C, A. Left selects Yes and Right
selects No; neither direction alone exits. B selects No and exits; C/A confirm the selected byte.
Directions take priority over buttons in the same sample. Initial Yes still needs confirmation.
No input keeps animation/VInt/loop work pending without timeout or a third returned choice.
After closing/deleting the prompt, the byte is sign-extended into the returned word.

**Unknown:** actual release/edge timing, simultaneous sampled inputs, window completion and visible
selection. Static input order does not measure timing or establish a naturally supplied outcome.

## Accepting: Creed Advice and Luke's Requested Departure

**Confirmed ordered scene:** cursor 1607 supplies eight text requests, 1607–1614, with seven by king
147 and one by Luke 10. Together with the five caller sites above, this is **13 request sites and
12 distinct texts**: closing text1614 is shared by acceptance and repeat. Research retains the full
16-record ledger (15 opcode commands plus terminator) and both inline bodies; all content follows:

1. Peter is requested to face the king. The king praises the choice and suggests Creed (1607),
   describes Creed as an ex-greater devil who may provide information (1608), directs travel along
   the river toward his mansion (1609), and suggests Polca may have a raft (1610).
2. The king faces Right and orders that the mayor be told to prepare a raft (1611). Luke's first
   waited action moves Up2, then `faceLeft 1`; Luke acknowledges the order (1612).
3. Luke's second waited action moves Down3, Right1, Down4, then the scene hides Luke. The king is
   requested to face Peter, asks Peter to help the leader (1613), and wishes the leader luck (1614).
   The script ends; only a successful return reaches the caller's F799 write.

Creed's history, the proposed river route and possible raft remain the king's claims/advice.
Luke's acknowledgment and local departure do not establish delivery to the mayor, Polca arrival,
raft preparation/availability or a map transition. No prompt, story-flag write, join, follow, party/AI
change, warp, entity reload or recruitment occurs within this script. Peter's F7/F39, Luke's F10/F42,
force AI and F67/F75 remain as supplied.

Both `faceEntity` commands resolve current word lookups and compare absolute **destination** X/Y
differences. Vertical wins ties; equal destinations choose Down. The first actor's sprite/facing is
updated and one VInt is waited, with **no HP guard on either actor**. This differs from byte-actor
`setFacing`, whose ally admission tests word HP; this scene's sole `setFacing` uses NPC147 and
bypasses that ally test. Zero Peter/Luke HP therefore does not skip these face/action/hide/text
requests. The scene neither heals nor revives.

Lookup resolution masks to a byte, translates NPC128+ by subtracting `$60`, then scales the mapped
physical index by 32. A zero mapping resolves physical 0; `$FF` resolves index 255 without a validity
guard. F67-clear Peter can therefore face physical 0. Action/hide/face do not generally validate
presence, and already hidden or aliased Luke must not be described as safely skipped.

Each `entityActionsWait` installs an action pointer after masking actor flags with `$9F`, then waits
for that actor's pointer to equal `eas_Idle`. This is not a global actor barrier or proof of an
unobstructed path. Relative moves use signed tile deltas times 384 from current position;
`faceLeft 1` is facing/sprite/wait work, not a leftward tile. The scene imposes no Luke starting
coordinate. **Inferred:** with valid fixed placement, completed movement and ordinary services,
Luke's arithmetic is `(38,51)` → `(38,49)` → `(39,56)` before hide. F75 following or changed positions
invalidate that fixed-start narrative; the endpoint is not observed.

## Hiding, Returning and Re-entering Have Different Boundaries

**Confirmed hide service:** `HideEntity` writes Luke's current/destination X/Y to `$7000`, invalidates
every matching physical-index entry in the 64-byte mapping list to `$FF`, and removes the matching
follower-list entry by shifting later entries. It does not clear F75, membership/party flags or
force AI, nor erase action/travel fields. Entity updates/action dispatch skip current X at least
`$7000`. In the successful distinct-mapping case both actions have already reached their waited
idle boundary; hide itself does not complete travel. Aliased mappings widen the invalidation.
This differs from the later F799-selected coordinate-only init.

Text services select portrait/speech and wait for scrolling/text; single-text additionally closes
windows and sleeps 10. Debug skip is separate. There is no standalone sleep record, but that does
not mean zero elapsed time. The terminator conditionally waits for scrolling when a text window
remains and clears scrolling speed; it sets neither F799 nor control.

| Supplied or partial state | Limit on the conclusion |
| --- | --- |
| Script stalled before hide | F799 remains clear; Luke's installed/completed actions depend on the reached point. |
| Luke hidden, but final text/script return unfinished | Hidden Luke and invalidated mappings can coexist with F799 clear. Retrying is not a guaranteed safe replay. |
| F799 set before a fresh F722-selected init | Rebuild supplies current population, then init moves Luke out of map without invalidating mapping/list state, even if F75 declared him as a follower. |
| F799 changes after init while Luke is still present | Repeat caller only requests closing text; it does not rerun removal or repair the actor. |

After successful acceptance-script return the caller sets F799. After any completed caller branch,
the event wrapper restores the **saved physical king actor's saved facing**, closes portrait/text
windows and enables entity updates. Its saved actor remains the target even if mappings changed.
Thus the final face-toward-Peter request is not a guaranteed final interaction facing: ordinary
unchanged king mapping restores Down, while a different supplied pre-interaction facing restores
that value. Unlike the zone wrapper, this entity wrapper adds no post-caller main-stop wait.

`ProcessPlayerAction` returns to exploration. `WaitForEvent` retargets camera 0 and installs physical
0's controlled action **only when pending map-event type is zero**; another pending request takes
priority. `PLAYER_TYPE=0` selects `eas_ControlledCharacter`, while other types select their own
actions. Completed caller/window/VInt services and zero pending establish this conditional handoff,
not observed responsive input. F799 alone proves none of those conditions. Stop here.

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #794](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/794) and the preceding English
guide accepted in [PR #793](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/793), without
stacking. Research retains the USA retail identity and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, caller at `0x56F0C`, init at `0x5708E`, scene at
`0x5797A` and named prompt/actor/control consumers. Its
[reproduction boundary](../../research/volcanon-return-through-bedoe-king-choice.md#return-control-and-reproduction-boundary)
owns source/retained comparisons. This slice reviewed accepted prose and bounded tracked-fixture
selections, with no source reinvestigation, optional payload generation/copy or new observation.

| Reviewed relation | Executable/contract owner | Adversarial disposition |
| --- | --- | --- |
| Choice caller and init | [sf2-map-events-static-v1:entity-programs](../../../tests/fixtures/h2/map-events/entity-programs.json), program25; [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body15. | All 15 caller operations preserve F799 test, actual prompt alias, refusal/repeat texts and script-before-write. Nine init operations preserve independent F799/F724 reads and coordinate-removal calls, without writes. |
| Return, activation and population | [map exploration](../contracts/map-exploration.md), [interaction state](../../research/map-event-interaction-state.md), [request consumption](../../research/map-event-request-consumption.md). | Accepted selected research owns row/mapping/flag-bit composition; inventory or handler seams alone prove neither natural floor reach nor a usable composed interaction. |
| Prompt, text and actor services | [common-menu input](../../research/common-menus.md#confirmed-input-contracts), [dialogue](../contracts/dialogue-system.md), [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), placement/action/lifecycle/gesture families. | The reconciled common-menu summary and accepted selected research distinguish directional selection from confirmed return, B cancellation and pending no input; timing remains Unknown. Command-family fixture identities do not contain the complete scene or observe it; research supplies its eight requests, two action bodies and handler distinctions. |

The [entity-program schema](../../../schemas/h2/map-events-entity-programs.schema.json) and
[fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) retain ordered operations, targets and
shard composition. The [init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py) retain body/provenance equality; the
[map-script verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../src/sf2tool/h2/map_script_schema.py) retain separate families/consumers.
Their focused checks live in `tests/python/test_map_events.py`, `test_map_init.py` and
`test_map_script_engine.py`. Existing narrow routes are `uv run sf2 h2 map-events`, `map-init`,
`map-script-engine`, `map-data`, `map-setup` and `map-entities`; naming them is not a fresh execution
claim. The common-menu Yes/No summary is reconciled to the same accepted prompt evidence; its
inventory and timing/presentation Unknowns remain unchanged.

**Unknown:** natural floor/collision traversal and interaction reach; actual flags, HP, mappings,
positions, membership/AI, follower/player state; release/input sampling, completed services and
pending requests; visible speaker/facing/motion/hiding, windows/audio/timing and responsive control;
later persistence, Luke's arrival and raft availability. These gaps admit no automatic runtime work.

For separate optional content, read [Bedoe's hatchling flight and rescue](bedoe-hatchling-flight-and-rescue.md).
F722 selects its callers, but rescue tests F13/F724 rather than the king's F799 or flight's F723.
That guide preserves both independent caller/control boundaries and claims no future recruitment;
this link establishes no compulsory order after the king choice.

This guide completes neither the campaign nor the design epic.
