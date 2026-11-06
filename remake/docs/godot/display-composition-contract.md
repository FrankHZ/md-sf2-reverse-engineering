# Display composition responsibilities

## Decision and scope

**Standard rigor:** #1621 audits several cooperating presenters, but changes documentation only.
The outcome is a reviewable ownership and ordering contract, concrete findings, and bounded repair
options. This is the canonical agreement for this investigation. It does not approve implementation
or human visual acceptance. The current milestone ends at independent review of the design PR.

**Decision:** keep `PresentationCanvas` for viewport composition and the existing field and battle
compositors for their local rules. The available evidence does not justify a global Z registry,
general scene graph, new rendering DSL, or another input/navigation authority. The historical
world-over-UI failure already has a shared boundary and finite acceptance. Remaining reported
bed/weapon problems do not yet establish that those local mechanisms are insufficient. One current
resize refresh gap is statically identifiable; it needs local geometry wiring, not another layer.

In scope: world versus UI, field plane/tile/entity occlusion, battle board versus close-up,
character/weapon/effect composition, and visible-group lifetime, hints, focus and resize seams.
Out of scope: product code, source-content corrections, a full campaign/weapon census, new captures
or comparators, engine rules, new battle inspection, save formats and UI style implementation.
The [UI language](ui-design-language.md) and [scene review](ui-scene-review.md) are accepted design
recommendations, not proof that their proposed layouts run. #1609 and #1620 retain their source
investigations and fixes; #1613 retains dialogue/portrait close behavior.

Acceptance for this design is direct source/reference/scope review, valid document routes, a clean
committed verification plan and actual PR CI. No SDK, Godot, emulator, normal/full research suite,
screenshots or private-input processing is needed. No runtime was started for this audit.

## Evidence boundary and source map

The audit baseline is accepted commit `11a8fded61bc675217179082b8649d42506caa96`.
File links below identify current owners; named methods locate the reviewed responsibility in that
Git object. **Confirmed** means directly checked implementation structure, or the explicitly named
retained observation boundary. It does not turn static code into original-game or rendered-pixel
proof. **Inferred** identifies an impact not reproduced here. **Unknown** stays with its named
owner and minimum observation below. Issues are coordination routes, not original-game evidence.

| Responsibility | Actual source owner and entry points |
| --- | --- |
| Three UI canvases | [`PresentationCanvas.cs`](../../game/src/UI/PresentationCanvas.cs): `Mount`, `SyncVisibility`, `_EnterTree`, `_ExitTree`; the only `CanvasLayer` construction in the adapter |
| Session handoff and host input | [`GameRoot.cs`](../../game/src/GameRoot.cs): `_Input`, `BeginSource`'s `EnterBattle` / `ShowExploration`, `OpenSystemMenu`; [`SessionPresentationOwner.cs`](../../game/src/SessionPresentationOwner.cs): `IsBattle` |
| Modal cover, pause, recovery | [`WindowSupportHost.cs`](../../game/src/Entry/WindowSupportHost.cs): `ShowWindowRecovery`, `RefreshWindowSupport`, `ApplyHostPause`, `RouteWindowInput`; existing Settings / OpeningPersistence host partials attach to `_hostUi` |
| Field UI | [`ExplorationSessionView.cs`](../../game/src/Exploration/ExplorationSessionView.cs): `_Ready`, `Present`, `PresentServices`, `HandleAction`; [`ExplorationSessionView.Menu.cs`](../../game/src/Exploration/ExplorationSessionView.Menu.cs) and [`ExplorationSessionView.Mouse.cs`](../../game/src/Exploration/ExplorationSessionView.Mouse.cs) own menu and pointer eligibility |
| Field world | [`ExplorationPresentation.cs`](../../game/src/Exploration/ExplorationPresentation.cs): `Draw`, `DrawLayer`, `Sprite`; [`ExplorationStoryEffects.cs`](../../game/src/Exploration/ExplorationStoryEffects.cs): `Transform`, `Draw` |
| Battle composition and input | [`BattleSessionView.cs`](../../game/src/Battles/BattleSessionView.cs): `_Ready`, `Present`, `Arrange`, `RetireWindows`, `HandleAction`, `HandlePointer` |
| Board-local composition | [`BattleMapViewport.cs`](../../game/src/Battles/BattleMapViewport.cs): `Build`, `BuildTerrain`, `Present`; [`BattlefieldTerrain.cs`](../../game/src/Battles/BattlefieldTerrain.cs); [`BattleMovementRangeView.cs`](../../game/src/Battles/BattleMovementRangeView.cs) |
| Close-up composition | [`BattleSceneView.cs`](../../game/src/Battles/BattleSceneView.cs): `_Ready`, `Present`, `Consume`, `ProjectHealing`, `Weapon`, `PresentStatus`, `SyncOverlay` |
| Text, portrait and local focus | [`ExplorationDialogueView.cs`](../../game/src/Exploration/ExplorationDialogueView.cs): `Present`; [`StoryDialoguePanel.cs`](../../game/src/UI/StoryDialoguePanel.cs): `Present`, `Arrange`, `MountPortrait`; [`DiamondCommandButton.cs`](../../game/src/UI/DiamondCommandButton.cs); [`MenuPointer.cs`](../../game/src/Input/MenuPointer.cs) |
| Weapon data boundary | [`BattleSceneDefinition.cs`](../../src/Sf2.Remake.Application/Content/Scenarios/BattleSceneDefinition.cs): `BattleSceneWeaponFrame`; [`BattleSceneContentReader.cs`](../../src/Sf2.Remake.Content/Scenarios/BattleSceneContentReader.cs): `Weapon`; [`remake_battle_scene_content.py`](../../../src/sf2tool/remake_battle_scene_content.py): sequence/weapon generation; [`battle_sprite_animations.py`](../../../src/sf2tool/h2/battle_sprite_animations.py): raw animation extraction |

To reproduce the static audit from a checkout of that object, inspect those methods and run:

```powershell
rg -n 'CanvasLayer|PresentationCanvas|ZIndex|ZAsRelative|YSortEnabled|MoveChild|ShowBehindParent|TopLevel' remake/game/src
rg -n 'CanvasLayer|z_index|z_as_relative|y_sort_enabled|show_behind_parent|top_level' remake/game -g '*.tscn' -g '*.tres'
rg -n 'SyncOverlay|PresentStatus|Weapon\(|_canvas.Scale|SizeChanged|ApplyHostPause' remake/game/src
```

**Confirmed, static:** the scene resources add no competing canvas/Z override; `Main.tscn` starts
`GameRoot`. No audited input dispatch selects its owner by comparing Z values. This is a bounded
inventory of the current adapter, not enforcement against future consumers.

## Viewport and visible-group contract

The implemented [display-layer agreement](presentation.md#display-layer-boundaries-1517) remains
authoritative. Godot documents that [CanvasLayer order](https://docs.godotengine.org/en/stable/classes/class_canvaslayer.html)
dominates child Z, while [CanvasItem](https://docs.godotengine.org/en/stable/classes/class_canvasitem.html)
uses local/tree ordering and does not propagate modulation through an intervening non-CanvasItem.
These API rules explain the implementation, not a new claim about the original console.

| Boundary | Required composition and current mapping |
| --- | --- |
| World, canvas 0 | Field draw calls, world fades/white flashes, battle board and close-up art. World Z must never be used to overtake UI. Fades intentionally exclude readable UI. |
| `BattleUi`, canvas 1 | Battle command/auxiliary HUD, scene cards/message, Give/Discard/Retreat, story/appearance, feedback. `SceneUi` is mounted here even though its producer is the world close-up. |
| `FieldUi`, canvas 2 | Dialogue/context, field diamond, Party/preparation, Church, Shop, failure. The field presenter may still own battle-entry/return delivery; a visible field canvas above battle is not automatically a leak. |
| `HostUi`, canvas 3 | Entry/name/slots, system Menu, Settings and recovery. Cover and pause are explicit host operations, not consequences of this layer number. |

There is one live canvas per role in the intended session lifecycle. `Mount` accepts an integer and
does not enforce uniqueness, but all three current call sites use its role constants. There is no
observed duplicate-role failure requiring a registry or new validation framework. Keep new viewport
UI attached through these existing owners; do not mount a fourth canvas to repair local overlap.

Canvas 0/1/2/3 is only the outer order. Within a canvas, retain these explicit relationships:

* Field mounts dialogue, field diamond then Party/preparation (via `MoveChild`), Church, Shop and
  failure feedback. Phase eligibility hides incompatible pages; later sibling order is not modality.
* Battle mounts auxiliary HUD and command menu before `SceneUi`, then transaction/story/appearance
  views. `BattleFeedback` uses local Z100. Mutually exclusive session phases, plus `Present` and
  `RetireWindows`, select visible groups. Its high local Z cannot cover HostUi.
* Host subpages cover/hide the retained Menu. Recovery saves each covered page's visibility, hides
  it, pauses processing, then restores it before gameplay resumes. Do not replace this with a large Z.
* `StoryDialoguePanel` creates frame, reading area, portrait, choices, hint in that order. It is one
  local widget, not independently ranked viewport layers. Diamond emphasis moves its own art/label;
  it does not move the hit rectangle or borrow world Z.

**Confirmed, static:** current consumers use the intended canvases; no world sprite or close-up
effect is attached directly to a UI root. `WhiteFade` at world Z100 and feedback at BattleUi Z100
are unrelated local values. Their equality is not a collision. Same-Z creation order remains real
coupling inside the finite owners above; changing child construction must preserve its documented
relationship, without turning every child into a registered layer.

Visibility and input are separate contracts:

1. `PresentationCanvas` mirrors the owning session Control's inherited visibility and resubscribes
   on tree entry. Host's owner is a `Node`; its explicit page lifecycle owns visibility instead.
   `SceneUi` mirrors `BattleSceneView.IsVisibleInTree()` and shares the battle session's lifetime.
2. Reparenting a live battle is not retirement. Keep `SceneUi` and reading state; freeing it on every
   `_ExitTree` was a reproduced historical failure. True session retirement frees the canvas subtree.
3. Full close-ups explicitly hide the map branch. A later black sibling at Z0 cannot cover its Z1/2/3
   descendants. Field death and after-turn notices keep the board; scene end restores normal control.
4. `SessionPresentationOwner.IsBattle` and the Application snapshot decide the field/battle handoff.
   UI canvas order must not decide when entry, return or a wait completes.
5. `GameRoot._Input`, `ApplyHostPause`, focus guards and each presenter own input. A visible overlay
   must block underlying gameplay through those paths, not presumed drawing precedence. Preserve
   mapped release guards and `MenuPointer`'s same-id/rectangle/boundary press-release checks.
   [Godot Control input](https://docs.godotengine.org/en/stable/classes/class_control.html) is a
   distinct routing concern; increasing `z_index` is not a modal-input implementation.

The retained [#1517 observation](../verification/host.md#display-layer-boundary-observation) confirms
the named private HEAL/UI overlap correction, field Hide/Show/process inheritance, reparenting,
EGRESS return and retirement at its headless state/input/geometry boundary. It also preserves the
failed overlay-disposal attempt and shutdown warnings. This audit does not rerun or extend it.

## Field world: priority is not one number

**Confirmed, static:** `ExplorationPresentation.Draw` has two profiles. With `Story.LogicalView`,
it uses independent A/B plane origins and the source-shaped priority/window inputs. Without it,
the fallback draws the full background, entities, then full foreground. The fallback does not
establish source tile-priority fidelity and must not be cited as the bound-path algorithm.

The bound profile's responsibility order is:

| Input or pass | Meaning and constraint |
| --- | --- |
| Area layer / plane origin | Select the main coordinate plane before the entity loop; B for area layer0, A for the admitted nonzero profile. Preserve independent A/B offset/parallax and actual draw clip. Entity `Motion.Layer` does not select coordinates. |
| Background low, foreground low, background high, foreground high | `DrawLayer` splits each 24px block into nine 8px tiles using word bit `0x8000`. Foreground block0 is skipped. Texture alpha and screen clipping remain in each pass. |
| Entity order | Visible entities sort by `entity.Priority`, then `Motion.Y`, then slot. This orders sprites relative to sprites, independently of tile/display priority. |
| Entity display priority | `High(entity)` is `(sbyte)entity.Motion.Layer > windows`, where windows counts the logical open text and portrait windows. Byte255 is signed−1, not a very high Godot Z. Modern host/field menus do not add to this source window count. |
| Low-priority entity repair | After drawing that actor, repaint high-priority B then A pixels only under its transformed opaque ink. Do not repaint its transparent rectangle: that would erase earlier high-priority actors. |
| Pose and image | Direction/animation half, `FlagsB & 3` orientation, render size, gesture/head shift, mirror and effect change the ink or destination. Ordering cannot correct the wrong sprite or anchor. |

The [bound camera contract](../../../docs/design/contracts/world/map-exploration.md#bound-field-camera-lifecycle)
owns source operands; the [actual visibility-coordinate agreement](presentation.md#scene-visibility-coordinates-1589)
owns modern clipping. `cameraProjection.legalScreen` uses actual main-plane/actor coordinates;
the clamped viewport rectangle is not interchangeable during an explicit story camera.

The [source-bound teacher observation](../verification/field-programs.md#source-bound-camera-observation)
retains the named text525 / teacher entity142, slot17, sprite209 seam, source revision, launcher,
draw passes and wall-over-ink results. The later [scene-visible-area observation](../verification/field-programs.md#scene-visible-area-observation)
checks the modern clip. **Confirmed at those retained boundaries:** wall repair and clip are
real mechanisms. Neither observation identifies the sleeping soldier's correct exposed head/body.

For [#1609](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1609), first bind the actual
map/area/slot and source initialization, then compare sprite, pose, anchor, area/main plane, tile
words and signed layer/window operands. **Unknown:** which operand, if any, is wrong. Raising every
entity, deleting foreground or adding a bed-specific engine exception would bypass the existing
contract. A source-specific pose/placement correction belongs to admitted content or its source
projection owner, after evidence. This audit does not select a replacement pose.

## Battle board and close-up composition

**Confirmed, static:** the board deliberately has a simpler admitted composition than field scenes.
`BuildTerrain` rejects unsupported plane motion. Low B/A terrain is Z0, movement range is Z0 after
terrain creation, actor markers/sprites are Z1, high B/A terrain is Z2, and target/preview marks are
Z3. Equal-Z B-before-A, range-after-ground and deployment child order are local creation-order
dependencies. They do not implement field `Motion.Layer`/window comparison or universal Y sorting.
No current reproduced board actor-order failure justifies replacing them in this slice.
The [battlefield owner](battle-scenes.md#battlefield-maps) retains its admission limits.

Within a full close-up, `BattleSceneView` owns a distinct 256×224 composition transformed to the
viewport. It must be reasoned about as a unit below BattleUi:

| Part | Current order and data owner |
| --- | --- |
| Black, background/wrap, enemy, ground | Black precedes `SceneCanvas`. The named sprites use default Z0 and their creation order; enemy art includes its weapon in the current generator's model. |
| Ally and separate weapon | Ally Z2. `Weapon` maps admitted layer1 → Z3 (in front of ally), layer2 → Z1 (behind ally); this is a semantic mapping, not copying a raw source number into Godot Z. |
| Fairy body/wings/dust | Z3; created after weapon, with body/wing pairs then dust. Equal-Z overlap depends on this local order. Exact original fairy/weapon overlap is not proved by that fact. |
| AURA / attack spell / laser | Z4 and distinct phase-selected visibility. Their creation order is not permission to display incompatible effects together. |
| Message and current actor/target cards | `SceneUi` on BattleUi; target follows the current reaction target. No part of the weapon/effect ordering ranks against these controls. |

`BattleSceneWeaponFrame` already carries the necessary tuple `(Frame, Layer, X, Y)`. The reader
admits layer1/2, signed-byte offsets and a present weapon frame. The generator preserves raw layer
and signed offsets; the H2 extractor's historical field name `zIndex` is not a Godot Z contract.
`Weapon` selects resource `Frame & 7`, H/V flips from bits16/32, and position `(136+X,64+Y)` plus
the current actor-frame/reaction offset. `Sprite` uses `Centered=false`. Action frame15 retains the
previous actor image while still applying its position and weapon tuple. Idle, action, reaction and
healing paths reuse this method. **Confirmed:** these are the actual implementation inputs and
call sites; they do not independently prove that the pinned original is interpreted correctly.

For [#1620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1620), bind the reported
knight/action and weapon tuple first. Compare front/back frames, retained actor frame, flip and
anchor together against the pinned source and accepted asset contract. **Unknown:** the offending
frame, correct original overlap and whether the fault is extraction, frame selection, sign/flip,
anchor or ordering. A spear crossing the body in one reported image cannot select a global Z flip.
No new global interface is needed to express either front or back; both already exist.

## Findings and minimum remedies

There is no newly demonstrated P0/P1 defect in this static slice. P2 below denotes a bounded
presentation/maintenance impact, not an accepted original-fidelity failure. Resolved historical
failures remain evidence, not an instruction to reopen their fixes.

| Finding / severity | Evidence, impact and smallest direction |
| --- | --- |
| F1 — historical world-over-UI / resolved | **Confirmed, retained #1517 observation:** private HEAL reproduced overlap before the fix, then retained geometry with higher UI canvas and identical semantic receipts. Keep the current canvases and visible-group lifetime. No current bypass was found in the audited constructors. |
| F2 — current resize refresh gap / P2 | **Confirmed, static:** `BattleSceneView.PresentStatus` positions the right card from viewport width; only `Present` calls it. `BattleSessionView.Arrange`, the size-change subscriber, only sizes map/menu. `_canvas` scale/position updates in `Consume` after suspended/process/completion guards. **Inferred:** resizing during a same-token message can leave the card at its old X; while host-paused, art can retain its old transform as well. **Unknown:** observed duration/visibility on the owned native instance. Add a geometry-only close-up layout method, called by the existing resize owner and normal presentation; never advance time or submit a command to refresh geometry. |
| F3 — implicit local order / P2 maintenance constraint | **Confirmed, static:** field Party uses `MoveChild`, board range relies on insertion after ground, close-up Z0/Z3 siblings rely on creation order. No general ordering failure is established. Preserve the local relationships above when editing; local named constants/comments in the affected owner are sufficient if an authorized fix touches those numbers. Do not create a central registry merely to rename them. |
| F4 — sleeping soldier and spear / unresolved reported defects | **Unknown** original relation and root cause; exact inspection owners are #1609 and #1620. Existing contracts can express tile occlusion and both weapon sides. Fix the identified operand/consumer only after diagnosis; these reports do not prove a shared infrastructure deficiency. |
| F5 — portrait/text close / separately owned | **Confirmed, static:** `ExplorationDialogueView.Present` derives portrait visibility from `PortraitWindow`, while panel lifetime also accepts `TextWindow`/waits. `StoryDialoguePanel` keeps them in one widget but does not force simultaneous close. **Unknown:** concrete source close sequence behind the user's report. #1613 must coordinate visible exit without changing legal script commands/waits or premature acknowledgement; Z cannot fix lifetime. |

F2 is distinct from #1614's optional **world-camera anchor** preservation. Both touch resize but do
not have the same acceptance. Do not silently enlarge #1614 or claim it already owns this close-up
repair. The coordinator can choose a separately bounded layout correction after review; no new
Issue or implementation is created here.

## Minimal interfaces, migration and observable acceptance

Keep composition responsibilities local. `PresentationCanvas.Mount` remains the cross-presenter
boundary; `ExplorationPresentation.Draw/DrawLayer` and `BattleSceneView.Weapon` already supply the
needed semantic seams. The only proposed new method is F2's geometry-only layout entry point.
Its concrete failure is missing resize refresh; canvas separation cannot recompute absolute card
positions, and calling `Consume` would mix layout with time/completion. An ordinary method in the
existing presenter, invoked by its current size-change owner, is sufficient. It stores no new
session state, layer table or lifecycle authority.

Future implementations must preserve these review constraints: a new visible group declares its
canvas, local order, visibility/lifetime owner and input owner; layout refresh changes geometry only;
cover/return preserves reading/selection while cancelling incomplete input; field and weapon data
retain their own source semantics. These are design/review obligations, not new executable gates.
Existing helpers do not enforce every obligation automatically.

| Candidate slice / dependency | Minimal candidate paths and stopping condition | Actual observation if later authorized |
| --- | --- | --- |
| F2 close-up layout refresh, after this design | `BattleSceneView.cs`, `BattleSessionView.cs`; layout only, no new camera policy. Stop once resize reaches art/cards at the retained phase, including pause/return. Serialize with #1616 battle composition. | Use an existing admitted close-up at a stable message token. Read actual card rectangles, canvas transform, message/portrait bounds before/after supported 1080p↔1440p resize, including Menu cover and return. Token, revision, elapsed delivery, HP/MP and RNG must not advance because of layout. Check errors and a fresh input after release. Keep world-anchor acceptance separate. |
| Bed/foreground diagnosis, #1609 | Its assigned projection/content paths only, after source map/area/slot binding. No dependency on F2 or new UI styling. | Read pose/frame/anchor, plane origins, clip, tile words, signed layer/windows and actual opaque repair intersections at the reported bed plus one nearby lawful wall/bed overlap. If source intent remains ambiguous, retain it and request only the minimum original seam under existing admission rules. |
| Weapon tuple diagnosis, #1620 | `BattleSceneView.cs` and only the demonstrated data-reader/generator owner. No dependency on F2. | Check one identified attack's front/back tuple and a neighboring legal weapon combination, including flips/offsets and retained-frame behavior. Read actual resources/transforms/local order, unchanged action delivery and UI canvas order. Do not sample every weapon/frame. |
| Visible close, #1613 | Existing dialogue/panel consumers, exact paths frozen by that owner. Coordinate before #1616 changes the same widget. | Ordinary close, no/with portrait, CLEAR/page, successive speakers and an actually related choice: visible group retirement and existing confirmation/wait progression, no early completion. |
| UI layout adoption, #1616 | Its already proposed field/battle/host owners; no new display manager. F2 may share the battle layout slice only with explicit ownership. | Long EN/zh-CN content and the current scene phase: disjoint measured cards/Menu/reading regions, actual focus/pointer ownership, Menu→subpage→recovery→return, field→battle→field and hidden descendant retirement. Target marks/hints stay local and cannot survive as stale input owners. |

For an affected code slice, reuse its existing adapter compilation and finite state/input/projection
observations. Add engine behavior tests only if engine behavior actually changes. No screenshot,
frame archive, generalized compositor test platform or full legacy run follows from this design.
Preserve the existing #1517 reparenting failure, #1398 map-visibility failure and retained shutdown
Unknowns; an unchanged historical result is not a fresh pass at a new head.

Remaining **Unknowns** are original bed/spear intent, the specific portrait-close sequence, F2's
native observed effect, unsampled equal-Z local overlaps, physical focus/device combinations,
final raster appearance and user visual acceptance. None blocks delivering this bounded design.
They do bound future repair acceptance; no subsystem-wide completion or fidelity claim is made.
