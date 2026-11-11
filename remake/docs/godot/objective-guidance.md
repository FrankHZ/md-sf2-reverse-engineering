# Optional field objective and destination

Implementation owner for [#1705](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1705),
the authorized A→B→C pilot of [#1578](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578).
The [design state matrix](modernization-design.md#optional-objective-and-destination-guidance-1578)
and [visual language](ui-design-language.md) remain the acceptance boundary.

**Standard rigor:** a finite content-disclosure rule crosses Application queries, preferences and two
field surfaces. Scope is the ordinary pre-voyage Kazin offer in Yeel through Hawel's first house scene
and revisit. This intentional modernization establishes no original-game mission system.
Other chapters, quest histories, new story facts, automatic movement, route search, battle guidance,
new save locations and the separate remote-services proposal are outside this slice.

## State and ownership

`GameSession.QueryFieldGuidance` is a read-only projection of the current session/revision/map,
ordinary field phase, flags, selected setup and live follower. It returns at most one objective,
known destination and current-map next leg/anchor. It submits no command and changes no clock, RNG,
resources, story or save. No objective identifier is persisted. Ambiguous matches produce no guidance.
Ordinary player motion retains the aid; automatic entry, dialogue, services, battle and stopped scenes
cannot supply ordinary field guidance.

`PrivateFieldGuidance` owns the finite profile-specific predicates and source bindings. The common
query and adapter contain no map numbers or story-specific flags. The admitted world must contain
the Yeel/overworld/house maps, compatible default setups and the source follower binding. Partial or
unrelated worlds have no pilot. Missing/ambiguous warp bindings or invalid local approaches reject
the declared binding at admission. A changed setup, live obstruction or missing follower can still
remove precision or coverage at query time.

| Reached state | Display |
| --- | --- |
| Before completed Kazin caller; F72 clear | No pilot objective or destination. |
| Following in default Yeel; F72 set, F4/F612 clear | Visit Hawel with Kazin; known house outside Yeel; recommended north exit. |
| Same following state on Map66, F503 clear | Same objective and known house; Route not available; no marker or bearing. |
| Map66 after F503, F72 still set, F4/F612 clear | Same objective; validated local house entrance when visible. |
| House scene, completion or revisit; joined/completed flags set | No pilot; no next-chapter spoiler or completion toast. |
| Other profile/map/setup, inconsistent flags, absent follower or ambiguity | No pilot. |

The town anchor binds Map5 `(7,0)` from `(7,1)` to Map66 `(18,21)`; the house anchor binds
Map66 `(6,5)` from `(6,6)` to Map43 `(6,11)`. These are internal content facts, never player-facing
coordinates. The [source chapter](../../../docs/design/synthesis/chapters/grans-opening/battle02-outcome-through-battle03.md)
and [ordinary campaign observations](../application/pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment)
own provenance and earlier traversal. The query checks the current public warp, marker, active area,
approach collision and live obstructing entities; it makes no shortest-path or all-map reach claim.
An invalid anchor retains the known destination with Route not available. The unverified pre-battle
leg deliberately stays coarse. Neither the unrelated villager's northwest line nor future enemies,
triggers, consequences or rewards are disclosed.

## UI and preference

Modern guidance / 现代引导 is one general Settings row. New and old version-1 files lacking the member
default On. The existing draft/Apply transaction, atomic storage, conflict/error retention, defaults,
read-only overrides and current mappings remain the sole preference owner. Explicit Off survives New
Game, Continue and restart, hides both surfaces including their fallback copy, and never writes a
gameplay slot. The graphics choice introduced by #1703 retains its index, storage and behavior.

The root Member/Magic/Item/Search menu adds Current objective in its bounded reading region, including
the known destination and next leg. It preserves command focus, names, Search availability and Back.
Right transfers directional ownership to overflowing reading; Up/Down then scroll, Back/Left restores
the same command. Party/Prepare covers the root menu and returns to that command. The whole command
and reading group moves to the opposite edge when its right-hand placement covers the player.

With menus closed, a capped, quiet 24px destination label uses the existing blue reading frame. A
validated, fully visible current-map entrance may have one hollow named marker; an off-camera house
has no compass arrow. Both labels ignore pointer input. They never move the player, reveal terrain,
retarget the camera or replace collision/warp legality. World guidance hides before host pause,
transfer, dialogue, Party, Search, services, battle and window recovery; current state is requeried on
return. The root menu alone shows the objective. English/zh-CN text and companion display names use
the existing catalog/name projection, 30px reading text and current mapped hints. Wider windows keep
capped reading widths and 96px cells; undersized gameplay uses existing recovery.

## Verification and replay

Engine behavior tests: `FieldGuidanceTests` exercises disclosure/completion, changed content,
unsupported setup/follower, ambiguous predicates, invalid local anchors and read-only invariants.
Use `uv run sf2 verify engine`, `uv run sf2 verify adapter`, the committed dependency planner, and the
actual selected CI under the [verification owner](../development-and-verification.md#scope).
Preserve completed failures and rerun only their owning nodes/files plus affected gates.

The native observer is `res://probes/engine_objective_guidance_observation.gd`, using actual host
input, preferences, field state and projected geometry; no screenshots or active-state setters.
Load this checkout's ignored environment and select its accepted ordinary pre-voyage content, then
use an explicit fresh `SF2_HOST_UI_OUTPUT`, disposable external `SF2_SAVE_ROOT` and `issue1705-*`
`SF2_SAVE_NAMESPACE`. `SF2_HOST_UI_CASE` selects town, house, revisit, save, continue or override. `SF2_HOST_UI_START`
is a worktree-local pre-session checkpoint admitted through the existing controlled startup consumer.
Town begins before the real invitation; house begins at the retained post-Battle03 approach. The
observer does not claim to replay a whole battle victory. House stops after the real scene completes;
leaving starts Battle04, outside this pilot. Revisit uses the compatible completed F612 initial
checkpoint, not a new battle campaign. Save uses an already legal Map3 recipe; continue relaunches
that real slot while retaining the later Off preference, then creates an ordinary New Game in its
empty second slot without changing the old slot or preferences. The ordinary unmodified
world remains the playable consumer, including HD herb coverage.

The checkpoint file contains a `start` object with `map`, `player: "entity-0"`, `position: {x,y}`,
`facing: 1`, `speed: 32` and these controlled initial flags:

| Case | Map / position | Flags |
| --- | --- | --- |
| town / override | `map-5` / `(7,7)` | `0,1,2,3,32,33,34,35,502` |
| house | `map-66` / `(6,6)` | `0,1,2,3,32,33,34,35,72,502,503` |
| revisit | `map-43` / `(6,11)` | `0,1,2,3,4,32,33,34,35,36,80,404,502,503,612,650` |
| save | `map-3` / `(42,9)` | `0,32,80,601,602` |

Town additionally supports `names: {"ally-4":"WWWWWWW"}` through the existing pre-session observer,
validated against admitted allies and `NewGameName`. This tests the existing name projection; it
adds no player rename operation. Native remap coverage uses ordinary preference binding
`confirm: {keys:["K","Enter"],buttons:["South"],axes:[]}`. The UI displays the current localized
input hint, not these configuration tokens. Start from old version-1 preferences missing
`modernGuidance`; select installed HD and the desired language. Override selects a separate
`--input-settings` file with `modernGuidance:false` and verifies no preference publication.

After selecting the existing environment, content and fresh namespace, invoke the installed editor
with the owning `remake/game` path and `--script res://probes/engine_objective_guidance_observation.gd`.
Use one fresh save→continue pair; continue deliberately requires slot2 to be empty before its New
Game check. A fresh replay must not reuse a namespace after that second-slot check has completed.
The observer records actual text, visibility, revision/flags/party/RNG invariance while menus own
input, layout, remapped keyboard and controller input, pointer non-travel, storage failure, locale
switch and the actual source caller/scene effects. Human judgments below remain separate.

Bound observation to at most twelve short cases, 30s per live checkpoint and 100KiB per terminal
report, 2MiB total; run serially. Reuse the existing approximately 149MB local world/dependencies
read-only rather than copying assets or regenerating seven battles. Query work is O(finite rules +
local entities), one result; no full-map acquisition. A native window/import uses the existing owned
Godot project and installation. No earlier debug instance was live at entry. Any missing ordinary
admission, unsupported save or longer checkpoint is an explicit failure, not permission to expand
the campaign or create another guidance authority.

For manual replay, launch this checkout's ordinary entry with a separate preferences/save namespace:
Continue an existing eligible checkpoint or play New Game through Battle02, enter Yeel and talk to
Kazin. Compare the gameplay menu before/after; Apply Off/On in Settings and revisit both surfaces.
Leave north; inspect the coarse overworld label, encounter interruption and usable return. After the
ordinary Battle03 victory, approach the visible house entrance, enter, finish the scene, leave and
revisit. Repeat in English/zh-CN and supported window widths. A local handoff supplies exact fresh
launch commands and retained checkpoint/report names for independent main-gate reproduction.

Engineering acceptance and human acceptance remain separate. Default On, writing quality,
unobtrusiveness, navigation usefulness and physical-device readability remain **Unknown** pending
human review in #1578/#1456. This implementation does not close those Issues.
