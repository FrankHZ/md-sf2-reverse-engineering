# Private media production and publication

The [tracked selection](metadata/selection.json) now reads four runtime roles through the canonical
media junction. Debug, the metadata projection and the Windows exporter have consumed this selection.
The relocated Release restored a disposable save through ordinary Continue. Keep the previous version available for
rollback. The incremental publisher still operates in disposable checkout-local storage.
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

| Owner | Reuse | Remaining production work |
| --- | --- | --- |
| [Prepare-PrivateAssets](../../../tools/Prepare-PrivateAssets.ps1) and [retail preparation](presentation-assets.md#prepare-and-mount-retail-resources) | Prepare selects families, and SFX/music accept command IDs. The audio producer verifies existing command, cue, timer and WAV identities and can resume by command. Coverage joins an inventory to prepared results. | The wrapper selects a versioned preparation root for the inventory and base, and separates the writable candidate library from the published selection. File existence or a valid *previous* result alone cannot prove an unchanged recipe. |
| [Private asset preflight](../../../src/sf2tool/remake_assets.py) and [local product assets](presentation-assets.md#local-asset-repository) | Exact private Git identities, versioned manifests, checked source/master/runtime relationships. Keep the existing checks on real Git roots. | Import the permitted identity and derivation fields into main Git once. Retain detailed operand, byte identity and custody records with their private source. |
| [Private package mount](../../../src/sf2tool/remake_private_package.py) and [Kiwi producer](../../../src/sf2tool/remake_kiwi_mount.py) | Existing family-specific transforms, fresh destinations, relative dependency closure and selected-file comparison. Kiwi derives six frame pairs, then writes each selected encounter's raster banks. Some mount modes already link unaffected files and copy the rest. | Separate calculation of changed outputs from role assembly. The current mount prepares a complete destination and can silently fall back to copying on link failure. Do not run a writer on a candidate tree sharing writable files with an accepted revision. |
| [Windows input resolver](../../../src/sf2tool/remake_windows_package.py), [ordinary entry](../evidence/opening-loop-acceptance.md) | Resolve the actual world, encounter inputs, opening documents, named Witch PNG siblings and catalogs. Packaged startup reads adjacent content. Content readers check semantic admission. | The pilot uses one selection for the resolver, Debug environment and projected reader bindings. Debug and Windows export both consumed the real shared entrance. The relocated executable completed ordinary Continue. |
| Historical full successors and one-off wrappers | Existing evidence identifies a source or past observation. Atlas and the indexed opening subtree are still producer inputs. | Rebuild Aqua into checkout scratch from the selected Atlas and retail inputs. Once supported consumers and unique sources are accounted for, retire surplus successors and scripts through the cleanup owner. Git preserves tracked recipes. |

## Tracked metadata and private records

Main Git is the lookup for a resource at an accepted code revision, even on a machine without
private media. The selection and lookup are tracked. Keep existing definitions in
their owning tables or reader code and reference their keys.

| Information | Who writes it | How it is used |
| --- | --- | --- |
| Family identity, intended IDs/groups, Original/HD policy, source/rights class, recipe with relevant tools/parameters and intended consumers | Family owner in the existing [inventory](asset-inventory.md), [music command table](music-command-inventory.md) or maintained binding/producer. Add missing machine-readable expectations only for the family under implementation. | Supplies intent, including expected but missing assets. A private manifest can supply a pinned source key. Changes to source meaning or permission need review. |
| Accepted gameplay, opening, entry and catalog shared-relative roots | Release owner in [selection.json](metadata/selection.json). Change only affected role/version values. | Debug setup, resolver, projection and Windows build read the same four values once per launch/build. Machine configuration supplies the shared root and tool locations. A checkout on an older code commit can select its own retained revision. |
| Current semantic bindings, variants, consumer and selected relative resource | Generate the sorted [current-media.tsv](metadata/current-media.tsv) from that selection, supported Content structures and declared expectations. Commit the generated diff with the selection. | Lookup key is family + semantic ID + binding context where coverage differs. Record Original and HD availability separately, mode-independent audio, missing and inapplicable states distinctly. Join the family key to its source/rights and production rule. |
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
projection and Content admission establish correspondence to actual material. The projector joins the six required entry names, their optional HD siblings, the two required
entry music contexts and the music owner's prepared command/timer records to the actual bindings.
Kiwi and Aqua frame groups report missing members and whole-group HD fallback. Other banks report
the selected resources at the reader's bank or semantic key granularity. Extend expectations in
their family owner when changing those families. An Original-only image selects the base bank;
`mode-independent` marks audio, `missing` an expected absent binding and `prepared-only` a reviewed
cue without a selected consumer. The detailed pixels, palette data and PCM stay private.

From the owning checkout, use `uv run --locked python -m sf2tool.remake_media_metadata --public-check`
for offline shape/key/path checks. With the private configuration loaded, omit `--public-check`
to compare the tracked bytes with a fresh projection. Pass `--write` to replace the table after
reviewing changed inputs. An explicit `--selection`, `--shared-root` and `--table` select a private
candidate without touching the tracked table.

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

### 1. Run the incremental pilot

Run PowerShell in the owning Windows checkout. The registered graphics root selects the pinned code used
by the [HD entry producer](presentation-assets.md#switchable-entry-layers). The seed copy costs
about 390 MiB once per fresh trial and keeps all reader-relative paths. Allow 1 GiB free for the
pilot and a 2 GiB process working set. Count previous trials, interrupted candidates and tool caches
when budgeting another batch.

```powershell
. ./local/private-inputs.ps1
[void][IO.Directory]::CreateDirectory($env:TEMP)
$root = Join-Path $PWD 'local/media-pilot'
$pilot = @('run','--locked','--with','pillow==11.3.0','python','-B','-m',
           'sf2tool.remake_media_pilot','--root',$root)
& uv @pilot init
if ($LASTEXITCODE) { throw 'Seed selection failed' }
& uv @pilot publish --base base --revision v1 --gfx-root $env:SF2_GRAPHICS_ROOT
if ($LASTEXITCODE) { throw 'Initial pilot failed' }
& uv @pilot publish --base v1 --revision png9 --png-compression 9 --gfx-root $env:SF2_GRAPHICS_ROOT
if ($LASTEXITCODE) { throw 'Image pilot failed' }
& uv @pilot publish --base png9 --revision loop --png-compression 9 --loop-begin 279420 --gfx-root $env:SF2_GRAPHICS_ROOT
if ($LASTEXITCODE) { throw 'Audio pilot failed' }
```

The Original input is the 1,055-byte `numeral.png`. The HD producer uses its existing 4× filter.
The audio input is the 6,245,032-byte prepared Witch WAV, command 10 / timer C1. Its PCM must
match the admitted entry cue. The one-sample loop-point edit is an isolated invalidation witness.
The entry reader requires a loop; this trial keeps every PCM sample. No capture is needed.

The publisher holds one OS writer lock. Cache keys include each unit's reviewed source, actual
generator and helper revisions, relevant parameters and tools. The entry reader and Python version
are shared dependencies. Entry assembly has its own identity for the binding and serialization
code. A revision without this identity receives one assembly refresh. When the assembler changes,
replay both verified unit outputs against the original entry role and write an independent
descriptor. Unchanged units can be read directly from their checked caches.
The result lists the affected stages. The `generated` field identifies
work actually performed after checking completed units. A PNG-only change preserves the old
`entry.json` by file identity. An audio-binding or HD-bank membership change rewrites that document
once. The role carries immutable regular-file hard links only inside the trial. The prior selection
and table remain readable until one atomic replacement of `ACTIVE`.

Read `ACTIVE` once and use `versions/<name>/selection.json` with the sibling `current-media.tsv`.
A metadata-only revision can still select an older entry role. Retain that referenced role when
retiring a revision. Roll back both records with `activate --base <active> --revision <retained>`.
For a no-op, publish with the active base, the same parameters and a fresh proposed revision name.
It returns the same active name. Use `--omit-hd` to withdraw this layer and omit the switch to
restore it. Entry names are fixed by the production reader. A new arbitrary entry name requires
a consumer change.

**Confirmed.** The following measurements are for serial units on the pilot host. File allocation
was measured by NTFS file identity to count hard links once. Working set is the publisher process
peak; candidate disk is the change in allocated bytes for that operation.

| Operation | Time | Peak working set | Candidate disk change |
| --- | ---: | ---: | ---: |
| Copy and verify 196 inputs, then project the base | 2.28 s | 260 MiB | 409,229,664 B |
| Stop after the image unit | 0.34 s | 73 MiB | 8,848 B |
| Resume audio and stop before the selection switch | 1.11 s | 272 MiB | 15,156,672 B |
| Resume the completed pair, then separately repeat unchanged | 0.94 / 0.92 s | 272 MiB | 0 B |
| Change PNG compression only | 1.13 s | 273 MiB | 587,224 B |
| Change the loop point only, including the 10,734,157-byte entry document | 1.11 s | 272 MiB | 25,884,096 B |

Both requested interruptions returned failure with the prior selection intact. Repeating the same
base, revision and arguments with `--stop-after image` or `--stop-after assembly` removed resumes
the completed work. The generated image and audio agreed byte-for-byte with fresh targeted
derivation, including the complete cue binding. Switching Python 3.13 to 3.12 with the same
pinned Pillow invalidated both units. The production EntryPresentationReader admitted six tested
selections, including Original fallback after HD withdrawal and the restored HD binding. Rollback
and removing one disposable version preserved the prior descriptor, shared PNG and edited image.
A recipe-link change updated the public projection while generating zero media.

The resolver preserves logical source names beside each delivery key and checks containment
against the resolved owner directory. Seed initialization maps those logical names to checked
physical sources. **Confirmed.** Read-only resolution through a disposable shared junction
selected the same 196 files and 2,836 public rows as the direct roots, including the logical
`media/` prefix. A separate nested-world fixture copied 22 members through the link into
independent files with those same relative names. These checks exercised input resolution and
seeding. The runtime migration below covers full Windows export.

A disposable change to the assembler alone adjusted the Witch loop binding by one sample.
The production entry reader reported 279420, then 279421, then 279420 on restoration.
Both media keys stayed constant, all three assembly refreshes generated zero media, and the
restored descriptor matched the first byte for byte. Repeating the restored recipe was a no-op.

The peak observed checkout-local allocation including the retained preliminary run, interrupted
outputs, alternate-interpreter cache, clean comparison and assembly counterexample was
1,032,197,440 B. The regenerated
tracked lookup was about 0.55 MiB. The highest observed publisher working set across both trials
was 303,878,144 B. The first measurement wrapper used a truncated Windows process handle and
failed to report its peak after seeding. The corrected measurement is the fresh seed above.
Logical file sizes include repeated hard-linked names and can exceed physical allocation. Recheck
headroom before any additional trial. These observations cover the selected entry reader and
input closure. The runtime migration below covers the real shared junction.

### 2. Runtime migration and validation

The custodian copied only the delivery closure selected by the existing Windows input resolver.
Each file kept its source-relative name within its role. After comparing the staged files directly
with their predecessors, the custodian renamed each checked version into its immutable directory on
the same volume. The shared `media` junction points to the real canonical `private-media/`.
All four final role directories are real directories inside that target. The checkout selection
changed after the complete set was readable.

**Confirmed.** The old and relocated selections resolved the same 196 delivery keys and
408,278,969 bytes. Every pair compared equal. Of 2,836 projected rows, 2,821 changed only their
shared-relative selection and 15 remained identical. A second checkout reproduced the tracked
projection from outside Git with an explicit selection and table. The comparison and projection
took 1.82 seconds and peaked at 260 MiB in the Python process. Staging and final publication used
one payload copy. After Debug, the project volume showed roughly 0.53 GiB less free space than
before staging, including tool caches and run output. The exporter required 4,740,314,411 free bytes at preflight. After export and unpacking for
validation, the volume used about 2.73 GiB more than before staging, within the 3 GiB allowance.
The compiled Debug adapter ran on the second checkout with an isolated namespace.
[Ordinary opening, Witch and Continue](../evidence/opening-loop-acceptance.md#launch-and-checkpoints)
read the relocated inputs.

For the current selection check, load the checkout configuration and run
`uv run --locked python -m sf2tool.remake_media_metadata`. The 196-member comparison above
used both complete selections during migration. Several predecessor directories have since been
retired. For recovery use the registered installation or select a complete available version;
verify its private projection before switching a source launcher.

**Confirmed.** The first full junction-backed export attempt was refused before process creation.
With automatic approval enabled, the maintained exporter read all four roles through the real
junction. The unpacked Release used adjacent content and restored Map44 with an isolated slot
through ordinary Continue. The [Windows observation](../windows-local-candidate.md#build-and-run)
records the exit diagnostic. Retain any previous selections still used by an operator launcher.

**Production inputs.** Atlas, the indexed opening subtree and the selected retail preparation
families now live under the [versioned media roots](asset-inventory.md#pre-voyage-package-locations).
The independent source repositories moved on the same volume. Comparing Git roots, HEADs, refs,
working state and file identities before and after the move covered ignored masters as well as Git
objects. A subsequent product-repository commit updated four private manifest selections. The
previous commit remains reachable. Graphics and audio kept their original commits.

The copied preparation inputs total 4,501 files and 2,666,855,494 bytes. Initial Coverage found
music command 12 missing from the previous library. Its recorded candidate, WAV, driver boundaries
and source recording were recovered from the pinned product checkout. Coverage then reported
3,375 prepared, 32 present, 204 not applicable and zero missing. With reads of former producer
packages blocked in the Python producers, icon Prepare, Coverage and command-12 Mount succeeded.
The relocated graphics filter reproduced the entry numeral HD PNG. The prepared-audio recipe
reproduced the C4 WAV and binding. Both outputs matched the accepted bytes. The two units ran
serially in 0.83 seconds with 289 MiB peak working set. The four runtime selections stayed fixed.
[Production commands](presentation-assets.md#prepare-and-mount-retail-resources)

To reproduce a previous production result, select its retained stable inputs or restore the
matching private Git revision into an isolated checkout and run the existing strict preflight.
An old Git pin identifies the original bytes but does not restore a removed machine directory.

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

The manual-play launcher now uses the canonical selection and ordinary slot Continue was
verified after its switch. The former Kiwi bank’s thirteen entry files match the retained Atlas
bank byte for byte. The independent Witch role supplies the runtime entry. Regenerating it
from Atlas and the relocated localization sources reproduced all fourteen selected files.
The indexed producer input already has identical copies of the historical opening’s second
scene and preparation record. The HD producer writes its own report with elapsed time and output
size. None of these extra files requires keeping a previous complete runtime package.

The retirement executor records exact paths, current readers, successors and exclusions in a
private operator batch. Apply it only after independent review and a fresh writer, link and
ownership check. Remove historical launchers with their old operands. Keep the current installation
and ZIP, private source repositories, explicit player profiles and backups, named fixtures,
designated video projects and the excluded video handoff. The [lantern study](hd-art-pilot.md#one-lantern-three-treatments)
and [localization authority](../application/localization-design.md#current-full-text-chinese-corpus)
have durable selections. The [open native-exit investigation](../verification/map3-commission-save.md)
owns its evidence independently of package retention.
