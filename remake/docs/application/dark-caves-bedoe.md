# Dark Caves, Polca and Bedoe

The ordinary campaign starts with the [Battle10 field return](new-granseal-east-shrine.md). This slice owns Battles11 and 12, Bedoe and the first playable turn of Battle13. High rigor applies because encounter selection, script returns, party control and save compatibility share one campaign state. Acceptance requires ordinary Continue, movement, interactions and combat, then a replayable field save before the return confrontation. Continue with [Battle13 and the first raft crossing](polca-first-raft.md) for combat, the mayor, Oddler and Luke.

## Play in the prepared checkout

Run `./local/issue1737/play.ps1 zh-CN Cave` or `./local/issue1737/play.ps1 en Cave`, then choose Continue, slot 1. The four starting points use independent player slots per language.

| Start | Continue location | Route |
| --- | --- | --- |
| Cave | Eastern Parmecia, Map70 (17,19) Down | Walk north to Map71, west to Map72 and enter the cave north of (58,50). The cave's (15,26) entrance selects Battle11. |
| Polca | Dark Caves, Map48 (15,11) Down | Leave through (3,12), cross to Polca at Map72 (43,46) and walk east through town. The church offers paid individual resurrection. |
| Bedoe | Mount Volcano, Map52 (23,8) Left | Enter Bedoe at (28,6). Follow the stairways to the king's address and audience, then go to Volcanon Shrine. Return to the king and answer his proposal. |
| Battle13 | Polca, Map13 (30,6) Left | Walk west along the road to the x=24 zone. Advance the confrontation to Gerhalt's recruitment and battle admission. |

Run `./local/issue1737/check-route.ps1 zh-CN` for an independent fresh-slot replay of the six westward steps, confrontation, recruitment and first battle turn. The proof uses the running Godot main scene, its input bindings, projected state, dialogue and audio. The prepared checkout's `local/issue1737/HANDOFF.md` identifies the immutable starting records and private receipts.

## Campaign state

**Confirmed** in the remake. The originating party held 5,033 gold with four dead allies, F510 set and F77 clear. Battle11 defeat returned to its carried New Granseal egress with 2,976 gold after battle earnings and halving. The winning continuation returned to (15,11) Down with 6,233 gold and five dead. The Polca priest charged 10, 140, 120, 100 and 110 gold for their individual resurrections. Battle12's winning return held 7,413 gold with Chester and Slade dead. Its after-script warped to (23,8) Left. The leader's dialogue set F714, and the field init delivered Peter's two lines and set F712.

Peter's activation word remained 4 through Battle12 and arrival at the shrine. The Volcanon scene's zero AI selector cleared bit 2 on the existing party record. The scene caller then set F722 and F711. The king's affirmative answer set F799 on return. Initial Bedoe entry, the king's address and the audience separately set F719, F720 and F721.

On a separate continuation, rescuing the fairy set F711 before the Oddler event. The winning main route left the fairy alone. An independent Battle12 leader defeat after the Polca resurrections returned to New Granseal (11,22) with 2,876 gold. F512, F712 and F714 remained clear. Refusing the king showed his refusal reply with F799 clear. Speaking to him again and accepting set F799. Bedoe's hatchling has a separate source condition and was not visited. F711 alone cannot distinguish fairy rescue from speaking to Volcanon. At the recorded Battle13 start, Sarah had the first player turn in round 1. Gerhalt was a player-controlled level 13 wolfman with 25 HP at (23,7), and Peter was player-controlled at (21,9). Chester and Slade remained dead. F715 and F413 were set, while F513, F716 and F718 were clear. A different movement or dialogue sequence can advance RNG and change the first actor. The field presentation and battle resources come from the local private package. Exact original presentation timing and hardware parity remain **Unknown**.

## Source and reproduction

Use the [cave admission](../../../docs/research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md), [cave outcome and Polca approach](../../../docs/research/chapters/eastern-parmecia/battle11-outcomes-through-polca-arrival.md), [fairy and mountain admission](../../../docs/research/chapters/eastern-parmecia/polca-fairy-through-battle12-admission.md), [mountain combat](../../../docs/research/chapters/eastern-parmecia/battle12-initialization-and-first-control.md) and [mountain outcomes](../../../docs/research/chapters/eastern-parmecia/battle12-outcomes-through-bedoe-entry.md) for original predicates, source symbols and table operands. The [audience](../../../docs/research/chapters/eastern-parmecia/bedoe-king-address-through-first-audience.md), [Volcanon](../../../docs/research/chapters/eastern-parmecia/bedoe-to-volcanon-audience.md), [king choice](../../../docs/research/chapters/eastern-parmecia/volcanon-return-through-bedoe-king-choice.md) and [return to Polca](../../../docs/research/chapters/eastern-parmecia/bedoe-return-through-battle13-admission.md) own the subsequent contracts.

Extend the preceding chapter's encounter and scene preparation with ordered battle rows 11–13. Regenerate the world, identity icons and both localization catalogs. The private publication has 31 maps, 903 programs and 1,277 selected world messages, 77 names and six entry aliases. The 126,378,910-byte world fits the 128 MiB reader limit. External command 33 uses the reviewed original-driver loop at timer 192. The dark cave follows the target with the secondary plane's aperture, and the mountain scrolls its secondary plane at half speed.

Field recipe 3 on gameplay revision 4 admits ten actor records. Older seven- and nine-member field saves initialize only missing, unjoined members. Existing inventory, names, HP, MP, activation, gold and the two saved RNG words are carried forward. Each prepared replay entry uses an ordinary game save.
