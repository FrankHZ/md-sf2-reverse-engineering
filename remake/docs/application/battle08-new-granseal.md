# Battle08 and the New Granseal handoff

The ordinary private campaign continues from the [damaged-ship deck](map14-plank-deck.md)
through the battle at North Cliff and the founding of the town. The acceptance point is the
transfer into the castle at Map7 (11,10) Up. The [year-later continuation](new-granseal-east-shrine.md) takes over at this transfer. The campaign retains
High rigor because battle outcomes and map initialization share persistent party state.

## Play and reproduce

In the prepared checkout run `./local/issue1735/play.ps1` for Chinese or
`./local/issue1735/play.ps1 en` for English. Choose Continue, slot 1. Each language has an
independent player namespace seeded from a compatible Map58 copy on first launch. For another
installation, use the ordinary entry, world, scene and catalogs described below, a compatible
slot copy and the normal Godot main scene. Graphics Settings still selects Original or HD.

1. Follow the [tower, docks and plank route](map14-plank-deck.md#play-and-review) to the man's
   deck reply. Confirm 2263 and advance the deck dialogue. At the second question, No repeats
   the question and Yes continues. The Map14 caller sets F703 and the ship egress. F408 admits
   Battle08 on Map72. Advance the shore introduction to field the current active living party.
2. Select a legal movement destination and action. The North Cliff board is 32×30 at Map72
   origin (1,32), with eleven enemies and thirteen ally placement templates. Actual deployment uses the living active party.
   Eliminate the enemies and acknowledge the combat and reward messages. Defeating the hunter
   in slot 128 while another enemy lives keeps the battle running.
3. Alternatively, on Bowie's turn choose Egress with sufficient MP. The paid retreat returns
   to Map14 (45,7) Down. A defeat returns there with the ordinary leader recovery and halved
   gold. Walk Down onto the plank, then south on deck to the man. Confirm his ready question
   (3204) to re-enter. F458 skips the first shore introduction. F408 remains set and F508
   remains clear on these ordinary returns.
4. On first victory advance the Map72 aftermath (2468–2482). Its queued Map73 return takes
   effect after the battle caller clears F408 and sets F508. Read the building sequence
   (2288–2291). After the final confirmation the force reset runs, the init caller sets F705,
   and its pending warp transfers the party to Map7 (11,10) Up. In the Battle08-only publication the default Map7 setup is the
   explicit Unsupported boundary, before year-later dialogue, F707 or castle control. There
   is no admitted field-save recipe at that point.

## Acceptance evidence

**Confirmed, remake.** The retained predecessor advanced from Chinese 2263 to 2264 with one
ordinary confirmation. Fresh Godot 4.7.2 .NET processes reproduced the deck from title Continue
on the compatible Map58 slot. The release observations below came from bound keyboard input
on the production main scene. The board drew 173 textures with its movement overlay and
fielded seven allies including Kiwi. The battle projected action messages, HP/MP cards,
opponent identity and reward. Source F6 and the joined/active lists remain authoritative for
optional Kiwi. The absent-Kiwi route was not played in this run.

| Observation | Result |
| --- | --- |
| Paid Egress | Bowie spent 8 MP, from 8 to 0. The ship return and second battle entry used F458. |
| Defeat and re-entry | The ordinary leader-death sequence returned to ship control. The winning replay began its second attempt after a defeat with 1,503 gold. |
| Ordinary first victory | At round 15 enemy-0 (slot 128) showed 0/24 HP. At round 16 enemy-3 showed 0/17 and the battle ended. Chinese combat and field dialogue used the selected catalog. |
| Founding | At 2291 Chester, Slade and Kiwi had 0 HP. On castle entry all seven had maximum HP/MP and clear status. Active/joined lists, actual equipment and packs, XP, levels, kills/defeats, item custody and 2,573 gold matched the pre-reset capture. |
| Castle handoff | The camera projection selected Map7 and drew 2,037 background tiles. Player x/y were 4,224/3,840 in 1/384-tile units, facing 1 (Up). F458/F508/F700/F703/F705 were set and F408/F707/F411 clear. The next cursor is `map-7-setup:0`, reported as `UnsupportedCapability/program-opcode`. |

The [deck and admission source](../../../docs/research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md),
[initialization](../../../docs/research/chapters/eastern-parmecia/battle08-initialization-and-first-control.md)
and [victory/return source](../../../docs/research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md)
own original-game evidence. Original natural timing and human visual assessment remain **Unknown**.

## Build and review

Use the registered retail ROM and clean pinned SF2DISASM from the [private-input owner](../../../docs/operations/local-private-inputs.md).
Run the [ordinary world recipe](pre-voyage-campaign.md#preparing-the-ordinary-selection)
with ordered encounter rows 02–08 and the tracked opening selection. Generate row 08 with
`sf2tool.remake_exploration_battles --battle 8` and `sf2tool.remake_battle_scene_content --battle 8`,
then run the same scene producer with `--field-death-base`. Retain its field-death scene as
row 08's `battleScenes` dependency. Use the [menu, HD herb and Map3 publication commands](map14-plank-deck.md#build-and-select-the-private-content)
and derive the Chinese catalogs against the finished world with the [current selector](../development-and-verification.md#pre-voyage-chinese-consumer-observation).

The measured final world is 121,599,318 bytes, an increase of 783,988 with 12,618,410 bytes
remaining below the 128 MiB reader limit. World, opening scene and 36 relative files total
156,901,380 bytes. The publication including review files is 172,355,289 bytes. The new field-death
scene is 533,401 bytes. Prior map art, audio, encounter inputs and admitted scene resources
were compared directly, and the earlier seven-battle world still passed ordinary new-game
admission. The current catalog has 862 world messages, 58 names and six separate entry aliases.
The four outputs of `--scope pre-voyage` reproduce the preserved earlier catalog and review
files byte for byte.

In the prepared checkout, `local/issue1735/HANDOFF.md` lists the publication, live successor,
exact replay and completed failures. `python -X utf8 local/issue1735/check.py` compares the
release and before/after receipts. No private package, translation corpus, save or trace is
required in the tracked repository.
