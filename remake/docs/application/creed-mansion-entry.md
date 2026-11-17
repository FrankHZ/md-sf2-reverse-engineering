# First visit to Creed's mansion

**Confirmed** in the private remake: both ordinary Battle21 victory saves enter Map33, complete Goliath's first scene, recover movement, save and cold-continue. The registered `handoffs/issue1857` package provides `Play-en.cmd` and `Play-zh-CN.cmd`. Choose **Continue → Slot 1**. Each language has its own history and player namespace. The default save stands at (12,15), beside the next magic-door zone; the [Desktop chapter](desktop-arrival.md) continues those saves through the transfer. Use `./Play.ps1 -Language en -StartAt victory` to replay the entrance or `-StartAt mansion` for the immediate post-scene save. [Predecessor](battle21-devils-tail.md) · [Original entrance and complete scene](../../../docs/research/chapters/southern-travel-creed/creed-mansion-entry-and-first-goliath-scene.md)

## Chapter agreement

Rigor remains **High** under [the campaign agreement](remaining-campaign-risk-baseline.md) and [Issue1857](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1857): cross-chapter saves carry two different party histories, persistent flags and transport. This chapter admits the separate exterior doorway, first-visit population/init, Goliath zone, ordinary movement and conversation, finite field saves and reentry. Acceptance requires the two inherited slots, bilingual text, actor obstruction and following, both live door copies, a usable exit, full cold-save record equality and continued acceptance of the old high-level Battle19 slot.

The existing map/program importer owns story conditions. The existing adapter EntityEffect channel adds the two reached wipe variants. The existing SF2 finite savepoint policy adds the bounded Map33 recipe; it neither changes the file format nor executes on-load scripts during restoration. Later mansion setups, the Desktop and Battle22 are outside this chapter. No terrain is changed to enforce that boundary. Original-game timing and presentation remain **Unknown** for these player journeys.

## Entrance, scene and return to movement

From the first-victory Map67 (26,23) save, press Up into the separate doorway (26,22). It requests Map33 (15,30), Up. No Battle21-completed predicate was added to this entrance. Init parks the six fixed residents at their source positions; it does not run the first conversation. Walking eight tiles north reaches (15,22) without F774. The next Up activates the (15,21) zone.

With F774 clear, the imported callback calls the complete first scene before setting F774. Both languages displayed IDs 1804–1833 in order. The two live `stepEvent` copies replace (17,11) from (26,0), then (15,20) from (25,0). The observed closed word at each destination was `0xC46F`; the copied words were 6 and 5 respectively. These are working-layout edits, not additional persistent flags.

The scene moves and confronts the travelling actors, performs Goliath's wipe-out, positions him at (17,12), Down, then wipes him in. The adapter uses the existing presentation-completion wait for each effect. Its modern crop projection follows the named source table: 23 iterations, two nominal ticks each, with crop row 1–23 outward and 22–0 inward. It retains the final crop until the sprite is refreshed; entry-skip delivery has the same terminal projection. Existing slide/mosaic mappings and their durations remain unchanged. This does not claim original DMA, VInt or visual parity.

The completed route leaves Bowie at (17,15), Up, Goliath at (17,12), Down, and temporary relations Peter → Bowie and follower B → Peter, each with the source place-2 offset. It grants player input and only then offers a settled save. The original static owner identifies B as **Inferred** Oddler; these player slots supply F68 and the Oddler sprite. No recruit, item, gold or transport change belongs to the scene.

Facing Goliath from (17,13), Up leaves Bowie blocked at that position. Confirm gives dialogue 1827 and returns control in both languages. From there, Down twice and Left five times reaches (12,15) without entering the separate magic-door zone. The stable `door` slots were saved there through the ordinary menu.

## Savepoint restoration and reentry

The `creed-mansion` recipe accepts settled positions in the active map area before or after F774. It does not require F521. Later setup states F22/F523/F783/F784/F786 remain outside its admitted restoration boundary. An occupied or blocked reconstructed landing remains unsaveable; for example, leave an opened door tile before saving.

Like the existing field recipes, Continue rebuilds the baseline layout and population at the recorded player position. F774 restores Goliath to (17,12), Down, without replaying the scene. The two live door edits and custom follower spacing are not part of the serialized record; ordinary population followers resume from the leader. This is a remake savepoint restoration, not an original-game save-state claim. The cold Continue → Save check compared **every field of the decoded recorded data**, including RNG, text/music state, names, party, inventory, Deals, once flags, gold, return town and vehicle berth. Both languages matched their own input record exactly.

The two journeys retain their distinct F519 histories and Shining Ball custody, Elric's F13/F45, chest flags F160–163, F751, Battle21 completion, active party and return town. Moving and reading dialogue may advance RNG through the existing rules; Continue preserves the seeds stored at the savepoint. The old high-level Battle19 battle save also cold-loaded, accepted movement confirmation/cancellation and saved its complete battle data unchanged.

For ordinary reentry, walk south through Map33's y=31 exit to Map67 (26,23), Up, then Up back into the mansion. F774 remains set, Goliath is placed at his post-scene position, and both live copies have reset to the base layout. The (15,21) zone returns without repeating dialogue while F785 is clear. Moving Up into the southern door opens it normally; its word becomes 5 again. Both parties then left the door tile and saved inside.

## Next reachable boundary

The separate zone (12,14) is naturally reachable from the `door` save by pressing Up. Its guard is F783, not F774. The [Desktop arrival chapter](desktop-arrival.md) now consumes both stable door saves, completes collection and first arrival, and leaves ordinary saves before the king. Battle22 and the later Cotton Balloon exit remain its named successors. The other Map33 exit, (9,5) to the Evil Spirit Shrine, remains a later interior-route boundary. [Magic door and destination selectors](../../../docs/research/chapters/southern-travel-creed/creed-magic-door-and-desktop-transfer.md)

## Reproduction and limits

In the private handoff, choose a fresh `-Sample` for each run. `-Mode enter` starts from that language's unchanged predecessor slot; `cold`, `revisit`, `inspect` and `door` use the named ordinary saves. `old` accepts an explicit old Battle19 `-Seed`. `compare.py` checks the retained state and save receipts. The handoff's derivation scripts use the pinned canonical import, registered USA ROM and upstream, current content selection, existing asset library and prior accepted package. They preserve the earlier resources and repeat encounter inputs. No private assets or generated packages belong in Git.

Configured checks are `uv run sf2 verify engine`, `uv run sf2 verify adapter`, the affected native input/state observations and `uv run sf2 verify plan --scope engine --base origin/main --head HEAD` on the committed candidate. The override applies only to the engine content/localization lowering edits in this slice; research evidence is unchanged. Completed engine failures and targeted corrections, native pilot failures and final CI identities stay in the Issue/PR handoff. Some successful headless runs retain the existing ObjectDB shutdown-leak warning. Screenshots and original hardware claims are excluded.
