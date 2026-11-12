# Demo asset inventory and the Granseal art review

The detailed inventory is `manifests/pre-voyage-asset-inventory.csv` in the local product asset repository. Each row identifies a resource by owner, family and ID. Filter by `map-3` for the map, `shared actors` for walking frames, `shared dialogue` for portraits, or `battle-1` through `battle-7` for a particular encounter. `manifests/map3-cast.csv` joins map setup, entity or party member, sprite and portrait. These tables are produced from the selected demo content. They describe available assets; a conditional follower or unused block can be present without appearing in a playthrough.

| Owner in the selected demo | Identity and grain of a row | Where to look or what the variant covers |
| --- | --- | --- |
| 18 field maps | 5,332 map-local block IDs. Each has nine source tile references, layout-use count, sample coordinates and base/overlay area tags. | Original blocks draw at 24×24. Map3 alone supplies a complete bank of 96×96 painted blocks. Door, roof and story copies use the same block IDs. |
| Shared field actors | 462 poses: 77 sprite IDs × three direction strips × two walking phases. The lateral view is also mirrored. | Map3 selects the 96×96 treatment of each pose. A field cell, foot offset, phase and facing keep their existing meaning. |
| Shared dialogue and member sheet | 38 portrait atlases, including the hidden eye and mouth replacement columns. | Original 64×64 or 256×256 on Map3 in HD. Crop, mirror, animation mapping and panel dimensions are independent of texture density. |
| Field interface | 44 item/spell identity states plus 32 states across four command-menu families. | The herb retains its earlier two field variants. Other field icons keep their existing resources. |
| Battle presentation | 1,046 encounter-scoped raster identities. The ID distinguishes actor/frame, weapon, ground, backdrop, effect and menu art as applicable. | These are installed battle resources. Repetition of a character between battles is expected. |
| Title and witch | Six named entry layers, including the numeral, arrival, face and the two expressions. | Dimensions and used tile count refer to the entry pack. |
| Audio | 78 world cues and two entry cues with command, timer and sample rate. | Listed for navigation. Audio has no graphics variant. |

The CSV has 7,040 rows for this pack. Shared art appears once per logical owner and frame. This is the pre-voyage demo inventory; the rest of the retail ROM has a different scope. The [presentation owner](presentation-assets.md) also identifies the bundled UI font, theme and public-resource policy. Code-drawn windows and text are outside the raster table.

## Map3 coverage and treatment

The outdoor area is 51×32 cells. Two interior areas each measure 11×10. The exterior's roof/facade plane has a 32-row offset and can be copied, revealed or restored. The player package includes all 447 block definitions, including copy sources and the declaration absent from the initial layout. HD supplies separate textures to the existing layer renderer. Tile priority, camera clipping and actor occlusion continue to use the layout and its nine tile words. The source atlas alpha distinguishes transparent index zero from opaque black ink.

The painted master is `masters/map3-materials/surfaces.png` in the local asset repository at `2e8524dcdd94b1568cf2e15e389d660a1f876a54`. The accompanying README records the text-only GPT Image prompt. It supplies sixteen original material studies. The local painter selects the applicable surfaces for grass, leaves, water, earth, stone, shingles, oak, timber and iron. It applies those painted shapes at display resolution using the original's color regions, guide ink and opacity. Neighboring blocks share a boundary treatment. Walking frames and portraits use local contour, shadow and cloth treatments of their source poses; their silhouettes and character designs are retained. No game pixels were supplied to the image service. The herb uses its earlier Scale2x recipe.

## Rebuild and review

Select the accepted world, opening battle scene, entry pack and material master as explicit read-only inputs. Publish into a fresh private output. This is an offline Pillow dependency only.

```powershell
. ./local/private-inputs.ps1
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_map3_hd --world $env:SF2_PRIVATE_EXPLORATION_CONTENT --scene $env:SF2_PRIVATE_BATTLE_SCENE_CONTENT --entry $env:SF2_PRIVATE_ENTRY_CONTENT --master $materialMaster --out $output
```

The final pack retains the original content IDs and gameplay revision. Select its `world.json` and `scene.json` for ordinary launch. The existing settings transaction prepares the installed HD content before enabling the choice. Original is selectable independently. A world with no optional bank retains its original graphics. The local `review/` folder contains all three map plates, two interior comparisons, a street detail, a character comparison and the two CSV files. The map plates compose source areas and the static roof copy; a moving actor or an opened roof must be reviewed in the game. These private images belong on the reviewer's machine.

The selected HD world is about 121 MB (decimal), below the unchanged 128 MiB input limit. One release adds roughly 6.5 MB to the herb world. Preparation is bounded by the admitted cells, actor poses and portraits, and makes one set of complete field plates; it does not collect gameplay frames. Derived assets and slots remain private.

For the actual host witness, give `engine_map_art_observation.gd` an isolated `issue1717-check-*` namespace containing a compatible Map3 slot near (32,13), the installed pack, writable settings initially in Original and an explicit report destination. It loads through Continue, changes Settings in both directions and reopens the member sheet. Compare the actual drawn block widths, pass geometry, portrait density and paused progress. Field time can legitimately advance the main RNG between menu visits. Human art-direction approval remains with the player.
