# Private media production and publication

**Accepted design; implementation pending.** The first milestone is an incremental image and sound pilot. Ordinary
launch and release still use the [registered selections](asset-inventory.md#pre-voyage-package-locations).
A proposed path or example below becomes operational only after its migration step passes.
[#2370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2370) has High rigor because
a producer, two kinds of launch, multiple checkouts and irreversible source retirement share these inputs.

## What an asset edit should cost

An artist changing one Witch layer should prepare that layer and its required variants. Unchanged
gameplay, opening and campaign text should cost zero publication bytes. A new sound for an existing
command and timer can also change its world binding. A spell frame embedded in 26 scene documents
may require rewriting all 26. The edit unit is a resource or atomic variant group. The smallest
write unit is the file its real reader opens. Budget both before generating anything.

| Approach | Consequence | Decision |
| --- | --- | --- |
| Produce a complete campaign successor and ZIP after each edit | Today's closed inputs work, but a six-frame change can publish hundreds of MiB of unrelated data. | Use full assembly for actual delivery or an explicitly costed recovery. |
| Maintain a single directory edited in place | Cheap individual writes, but a stopped writer can leave a mixture of world, scene and audio revisions visible to every checkout. | Keep accepted inputs immutable. |
| Publish a new revision of each changed reader root, reusing unchanged immutable files | Preserves today's sibling names, relative dependencies, Original/HD grouping and rollback. Fixed-layout JSON still costs its full rewritten size. | Default. Generate changed records in scratch, then publish the smallest changed roots together as one selection. |

A selection names gameplay, opening, entry and display catalog independently. Keep a
reader-compatible tree in each revision of an affected role. Within the canonical entity only,
reuse regular **files** from the preceding immutable revision with same-volume NTFS file hard
links. This is the optimization already used for some mount paths. An unchanged role keeps its
existing revision, with no new directory. A delivery is a regular self-contained copy. A separate
runtime overlay, a general asset service and filesystem-wide fingerprints are unnecessary here.

## What exists today

| Owner | Reuse | Change required before incremental publication |
| --- | --- | --- |
| [Prepare-PrivateAssets](../../../tools/Prepare-PrivateAssets.ps1) and [retail preparation](presentation-assets.md#prepare-and-mount-retail-resources) | Prepare selects families, and SFX/music accept command IDs. The audio producer verifies existing command, cue, timer and WAV identities and can resume by command. Coverage joins an inventory to prepared results. | Pass the same explicit selection to discovery, Prepare, Coverage and Mount. The wrapper currently builds the inventory path under its fixed issue root. File existence or a valid *previous* result alone cannot prove an unchanged recipe. |
| [Private asset preflight](../../../src/sf2tool/remake_assets.py) and [local product assets](presentation-assets.md#local-asset-repository) | Exact private Git identities, versioned manifests, checked source/master/runtime relationships. Keep the existing checks on real Git roots. | Import the permitted identity and derivation fields into main Git once. Retain detailed operand, byte identity and custody records with their private source. |
| [Private package mount](../../../src/sf2tool/remake_private_package.py) and [Kiwi producer](../../../src/sf2tool/remake_kiwi_mount.py) | Existing family-specific transforms, fresh destinations, relative dependency closure and selected-file comparison. Kiwi derives six frame pairs, then writes each selected encounter's raster banks. Some mount modes already link unaffected files and copy the rest. | Separate calculation of changed outputs from role assembly. The current mount prepares a complete destination and can silently fall back to copying on link failure. Do not run a writer on a candidate tree sharing writable files with an accepted revision. |
| [Windows input resolver](../../../src/sf2tool/remake_windows_package.py), [ordinary entry](../evidence/opening-loop-acceptance.md) | Resolve the actual world, encounter inputs, opening documents, named Witch PNG siblings and catalogs. Packaged startup reads adjacent content. Content readers check semantic admission. | Drive resolver and Debug environment from the same selection. Derive metadata from resolved inputs **and** the reader bindings. A directory listing includes files the build never selects. |
| Historical full successors and one-off wrappers | Existing evidence identifies a source or past observation. Atlas and the indexed opening subtree are still producer inputs. | Replace active fixed paths, including the Aqua reproduction/rollback defaults. Once supported consumers and unique sources are accounted for, retire surplus successors and scripts through the cleanup owner. Git preserves tracked recipes. |

## Tracked metadata and private records

Main Git is the lookup for a resource at an accepted code revision, even on a machine without
private media. Implement the following small files in the pilot. Keep existing definitions in
their owning tables or reader code and reference their keys.

| Information | Who writes it | How it is used |
| --- | --- | --- |
| Family identity, intended IDs/groups, Original/HD policy, source/rights class, recipe with relevant tools/parameters and intended consumers | Family owner in the existing [inventory](asset-inventory.md), [music command table](music-command-inventory.md) or maintained binding/producer. Add missing machine-readable expectations only for the family under implementation. | Supplies intent, including expected but missing assets. A private manifest can supply a pinned source key. Changes to source meaning or permission need review. |
| Accepted gameplay, opening, entry and catalog shared-relative roots | Release owner in one proposed tracked `remake/docs/content/metadata/selection.json`. Change only affected role/version values. | Debug setup, resolver, projection and Windows build read the same four values once per launch/build. Machine configuration supplies the shared root and tool locations. A checkout on an older code commit can select its own retained revision. |
| Current semantic bindings, variants, consumer and selected relative resource | Generate a sorted `remake/docs/content/metadata/current-media.tsv` from that selection, supported Content structures and declared expectations. Commit the generated diff with the selection. | Lookup key is family + semantic ID + binding context where coverage differs. Record Original and HD availability separately, mode-independent audio, missing and inapplicable states distinctly. Join the family key to its source/rights and production rule. |
| Private payload records | Existing private manifests, prepared indexes and candidate receipts | Exact input identity, tool revision/parameters, palette/frame operands, captures, full local paths and byte checks stay private. Use a reviewed private revision or existing input identity to reference them. |

For example, the public key `aqua-2/frame-0` identifies a spell raster. Its row selects a scene
and a member of its Original/HD banks. Audio keys distinguish command **and** timer/context when
the selected versions differ. A missing expected HD member records the actual reader fallback for
its group. A prepared cue with no selected binding remains prepared-only. Deleting a binding must
remove its stale selected row or expose the missing expectation. Do not infer HD coverage from a
directory name, infer licensed distribution from a successful export, or publish embedded rasters.
The [battle gauge owner](../../game/assets/ui/battle-status/README.md) already records the public
project assets it ships.

Generate rows from the *complete logical selection* in stable order even when generation touched a
single family. Resolve a scene or bank once and join expected keys to the admitted set. Compare a
fresh projection directly with the staged TSV. Unknown IDs, duplicate/conflicting bindings, paths
outside the selected roots or a changed row with no selected input/intent change must be resolved
before publication. A change to payload bytes with the same ID and coverage still advances the
affected role revision. CI without private media can check shape, keys and portable paths. The private
projection and Content admission establish correspondence to actual material. The earlier bounded
projection exercised present bindings. It still needs expected/missing rows, recipe joins and the
shared selection input before it can own this file.

## Incremental run

1. **Identify the change.** Start from the accepted selection and producer revision. Compare pins
   for the relevant source paths in private Git, the registered ROM/upstream, the prepared family
   index, derivation parameters and versions of the modules/tools actually invoked. Record a newly
   edited master under its source owner's version control or existing reviewed identity before
   using it for publication. An unversioned or ambiguous input invalidates its consuming family.
2. **Compute the work.** A changed master invalidates its dependent variants and mounts. A palette
   or shared donor also invalidates all keys that borrow it. A change to a command's capture
   context invalidates the affected command/timer bindings and the worlds that select them. A
   scale/filter change invalidates the derived bank using it and leaves original extraction alone.
   A pure consumer remapping reruns binding/projection and rewrites only affected documents. Follow
   both old and new edges so removal and rename reach former consumers. A common extractor change
   reaches all its consuming families. If inputs cannot be narrowed, rebuild the smallest full
   dependent family and report the expansion. Compare only recorded dependencies between the old and proposed pins, for example with
   a path-scoped Git diff on the generator, imported helpers or private source subtree. A changed
   timing rule for SFX must also reach any cue sharing that context. Reuse a reviewed unchanged
   upstream object or existing private manifest identity without hashing the asset library.
3. **Size and assign the batch.** List selected families, output files/banks, dependency fan-out,
   expected bytes read/written and required tool slots. One checkout owns a family's candidate.
   Serialize families touching the same world/scene or private Git writer, and all use of the
   registered emulator. Start with one image group and one audio command serially. Increase the
   batch only after the measurements below fit. Batching multiple edits to one scene should
   rewrite it once.
4. **Generate into checkout scratch.** Use a fresh candidate under the owning checkout's `local/`.
   Retain the existing family/command receipt with the exact inputs, recipe revision/arguments,
   output list and successful admission result. Resume completed units only when these still
   match and the output passes its existing checks. Retry an incomplete unit in fresh scratch.
   A missing source stays missing until its owner supplies it. Preserve the last working selection.
5. **Assemble the candidate.** A single publication owner reserves the affected roots and checks
   that the base selection is still current. Stage unpublished role revisions in fresh directories
   on the canonical volume. Write independent new files for the entire changed set, including
   rewritten JSON containers. Link only explicitly unchanged, final regular files from the same
   canonical entity after all transforms. Never patch, truncate or change attributes through a
   linked name. The producing worktree never receives a link to an accepted writable file. If
   linking cannot be used, budget physical copy bytes and stage only after the batch fits. Do
   not silently copy a full campaign or add directory links. Abort and replan a stale base.
6. **Check, publish and switch.** Resolve the four candidate roots, compare selected relative
   paths and bytes for unchanged dependencies, check changed groups with the Content reader and
   regenerate the public projection. Move each verified role into its fresh final version by
   same-volume rename. Keep it unselected while other roles are being staged. Then integrate one
   tracked change selecting the entire set and its generated metadata after the private owner
   confirms availability. Each consumer reads one Git selection. Mixing roots from two reads of
   a changing checkout is unsupported. A crash before integration leaves the prior selection
   usable. Restart by verifying any unselected final directory against the same candidate before
   reusing it. A conflicting existing version needs a fresh name.
7. **Rollback and reclaim.** Revert the selection **and** its generated rows together and restart
   the affected reader with the previous retained roots. Release the previous revision only
   after actual consumers, registered checkouts and the agreed rollback window have released it.
   Retire exact directory entries under the cleanup owner. Never edit or erase a shared file's
   contents to free space. Physical space returns after the last hard-linked name is removed.

## Directory responsibilities

| Location | Writer, contents and lifetime |
| --- | --- |
| Main Git, `remake/docs/content/` and existing source/binding owners | Reviewable rules, current selection and generated public lookup. Main Git also retains existing licensed/authored public resources with their provenance. |
| Canonical checkout, ignored `private-media/source/<family>/` and `private-media/masters/<family>/` | Custodian owns unique raw material and editable masters that are currently outside private Git. Use a family/version below these roots. For already versioned private Git sources, keep history authoritative and materialize only pinned inputs needed by current production here. Edit their originals in the owning private repo, never edit both copies. |
| Canonical `private-media/{gameplay,opening,entry,display-catalog}/<version>/` | Publication owner writes fresh immutable reader roots. The [location table](asset-inventory.md#pre-voyage-package-locations) names the initial versions and required seed locations. Prepared inputs, including the retail preparation, stay available while a supported producer reads them. One complete gameplay base can be a necessary seed. |
| Configured shared root, `media` | One NTFS **directory junction**, pointing **to** canonical `private-media/`. Worktrees resolve the shared root and select a role through this entry. Verify both the link path and its resolved final target. This is the only directory link in the layout. |
| Shared `sources/{product-assets,graphics,audio}/` | Three real independent Git roots. Product assets retain binary source/master history and private byte-level manifests. Graphics owns pinned transforms and their development inputs. Audio owns MIDI/source projects and tools. Preserve refs, uncommitted work, required ignored sources and local-only history. Private production records can reference main's semantic ID; the main selection and its public lookup have one writer in main Git. |
| Checkout `local/`, shared `packages/windows-demo/<version>/`, operator data | Staging, cache and failed scratch belong to their worktree. The packager assembles a complete delivery only for a requested export. SDKs, archives selected for a rollback, player data and backups have separate [storage owners](../../../docs/operations/local-private-inputs.md#single-volume-local-layout). |

A normal scratch path must resolve under its owning checkout's `local/` with no reparse ancestor
beyond that authorized root. Skip link entries when enumerating a parent for cleanup. To unload
the shared entrance verify the exact junction and target, then remove only that link entry under
its own authorization. Deleting the canonical entity is a separate operation. Do not use a reader
path resolved through the shared alias as a cleanup target.

## Migration and acceptance

**1. Image and audio pilot.** Use disposable roots and existing reviewed private inputs. Exercise
one Witch image pair (or one existing six-frame spell group when measuring scene fan-out) and one
prepared SFX command with its timer. Measure elapsed time, peak process memory, input, transient,
output and *physical* additional bytes for a clean targeted run. Compare bytes and admitted
bindings with a fresh run of those same units. Check that unchanged files linked within two disposable
role revisions share a file identity and that replacing a changed file and retiring the trial
revision leave the original bytes readable. Repeat unchanged and expect zero generated media.
Change one image parameter and then one audio input/context. Verify only the expected groups and
containers rebuild. Interrupt between units and again before selection switch, then check the old
selection and resume. Verify metadata additions, removal/missing, deterministic order and selector
parity. Stop with the initial implementation and measurements for review.

Budget peak disk as changed containers + candidate scratch + physically copied unchanged files +
retained rollback + comparison workspace. Measure free space first. As a scale reference, the
verified runtime closure is 196 files / 408,278,969 bytes, independent of the much smaller public
binding table. Start the pilot serially with a one-family cap, at most 1 GiB additional disk and
2 GiB process working set; if the estimate or observations exceed either, narrow the unit or
revise the batch before continuing. Include failed scratch in the next estimate. Derive later
batch sizes from available headroom, observed per-unit cost and shared tool slots. Optional
emulator recapture has its own time and serial installation budget. Reuse prepared PCM for a
mount/edit experiment; measure capture separately before scheduling capture at scale.

**2. Move the supported roots and switch real consumers.** Depends on the pilot and its checked
selection/projection. Inventory only the exact source selections, owners and necessary production
inputs. Measure free space for the largest concurrent transfer, validation and rollback. The first
four runtime roles are approximately 0.39 GiB together. Stage and byte-compare selected members.
Move independent repositories one at a time, checking HEAD, refs, status and local-only material.
Update retail wrapper inventory/Prepare defaults, pinned inputs and private preparation records.
Verify one required preparation from retained sources. Keep Atlas, the indexed opening subtree and
the registered ROM while their producers need them. Do not replay every predecessor.

First exercise junction resolution, contained paths and unlink/cleanup with disposable data.
Then verify the authorized real junction and read the same selection from a second worktree,
including from outside its root. Start normal Debug on an isolated save namespace. Observe ordinary
opening, Witch and gameplay and an affected Original/HD binding and audio selection. Package
Windows from only the new roots, use the [exporter's disk preflight](../windows-local-candidate.md#resource-budget-and-verification),
and launch the relocated executable with adjacent content in a clean environment. Resolve every
input into its declared role, with legacy paths absent from the test configuration. A previous
isolated export established closure on ordinary directories. The junction-backed Release check is
outstanding. Respect the existing execution refusal until an authorized review resolves it.
Keep old selection and rollback resources until Debug **and** Release pass.

**3. Retire exact obsolete consumers and data.** Depends on accepted launch/build/producer checks
and a retained rollback. Main-gate reviews the selected script and directory list. Resolve active
callers, ownership, sole sources and pending work. Retire obsolete fixed-path scripts and historical
collection copies when supported production no longer needs them. Compare the size of any
remaining unique material with destination and rollback headroom before including it in a batch.
Review any remaining worktree with Git before retirement and preserve unique sources first.
Keep the canonical checkout and three to four reusable worktrees including the manual-play
maintenance checkout. Check recovered space and closure again. Hold the old root through the
agreed rollback window. After deletion, recovery uses the retained source/recipe or approved
backup for that item. Keep any unique item without such a recovery path out of the retirement set.
Perform any remaining project-volume migration in measured batches under the
[cutover owner](../../../docs/operations/local-private-inputs.md#cut-over-a-local-selection).
Player saves/backups, both designated video projects, independent history, unique inputs and the
previously excluded handoff stay protected.
