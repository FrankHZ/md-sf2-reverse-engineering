# Church promotion

**Ordinary church promotion.** Bowie can become HERO from SDMN at stored level 20 or higher. The current class travels with his earned progress through field play, combat, level-up, saves and return. The campaign retains High rigor because this change crosses growth, equipment, presentation and both save recipes. The ordinary class pairs and Eric’s Pegasus Wing route are playable through first Pacalon. The other special families remain open. [Campaign agreement](remaining-campaign-risk-baseline.md) · [Design assignment](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1945)

## Playable HERO and reproduction

**Confirmed** in the remake. Each independent English and Simplified Chinese Pacalon Castle party selected level 27 Bowie at the ordinary priest, declined, selected him again and accepted HERO. Member selection includes the whole joined force and scrolls. The field member card, class text and walking sprite update. Each party kept its equipped Achilles Sword, EGRESS 1, BOLT 1, live vitals, earned stats and accounting. The only changed Bowie save values were class, level and EXP. The existing historical roster upgrade also seeds unjoined Rick when loading these older castle slots. The rite preserves party membership, other actors, flags, possessions and gold. Compare seeds immediately across the final Yes because walking to the priest advances field simulation.

Both cold field saves continued through Map02, Map71, Map72 and Ketto to the Battle26 repeat. HERO moved with MOV 6 and normal terrain costs, attacked with sprite 24/palette 0 and the equipped sword, received damage and earned a level. English reached HERO 2/20 EXP, Chinese HERO 2/0. Both spell books retained one BOLT 1 through level-up and cold Continue. Both cold battle saves continued at player input and returned through ordinary paid EGRESS to a playable Map36 with HERO 2. In English the complete battle save data after level-up matched across uninterrupted play and a separate cold Continue from the same pre-action save. Field formats 1/3 carry the existing Class. A changed party writes battle format 4 with class terms for deployed and retained members. An actual Battle26 format-2 save with a retained ally restored and re-saved in format 2. The engine cases in `FieldServiceTests`, `OpeningSaveTests` and `BattleSaveProgressTests` cover level 19/20/late, living/fallen reserve, equipment variation, cancel, stale/repeated answers, future growth, invalid pair/book and changed-class reserve restore.

The promotion observation used EGRESS and proved that the book survived the rite, level-up and Continue. The [battle spell observation](#current-battle-spells) exercises the retained BOLT 1 as an action.

The first scene pilot added eight HERO weapon bindings and 18 unique rasters, 46,956 compact JSON bytes including their actor records. The growth row takes 1,570 bytes. The rebuilt world is 101,808,849 bytes versus 101,761,388 for the Rick selection. The campaign merges the Battle26 member frames into earlier mounted scenes in memory. Only the Battle26 attack is witnessed. The source promotion fanfare (command 25) is absent from the mounted audio selection. The service uses the current church audio. Original rite timing, audiovisual parity and promotion-specific original SRAM behavior remain **Unknown**. The shared rite audio for command 25 and the remaining ordinary pairs are tracked in [#1956](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1956) and special promotion in [#1957](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957).

Use the registered private handoff `handoffs/issue1955` under the shared input root. It contains the stable rebuilt world, scene and catalogs, both original castle slots, an original battle slot, ordinary-input drivers and retained receipts. Build the current adapter in the owning checkout. A fresh journey starts from its matching read-only seed. Subsequent modes use the same external slot namespace. Pick a new journey name for a new attempt.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv run sf2 verify engine
uv run sf2 verify adapter
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj -c Debug --no-restore
$repo = (Get-Location).Path
$handoff = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1955'
foreach ($language in 'en', 'zh-CN') {
    $journey = 'review-' + [guid]::NewGuid().ToString('N').Substring(0,8)
    foreach ($mode in 'promote', 'road-fight', 'egress') {
        & "$handoff/Review.ps1" -Repo $repo -Journey $journey -Language $language -Mode $mode
        if ($LASTEXITCODE) { throw "Inspect $language $mode receipts and native exit" }
    }
}
```

For the upgraded legacy input use `-Mode legacy -Language en`. To compare continuation determinism, use a fresh journey with `-Mode fight -Seed <battle-ready-slot.json>` from the road-fight receipts and compare decoded level-slot payload `data`. To rebuild the private content from the registered sources use the accepted `handoffs/issue1939/runner/Rebuild.ps1 -Repo $repo -Output <fresh-package-directory>` on this revision. The evidence runner uses mapped input and state, scene and error projections.

**Retained failures.** Initial content admission stopped on HERO sprite 24. The first cold Battle26 fight completed a HERO attack and stopped at `ai-target-class`. The pinned `data/battles/global/aipriority.asm` gives HERO rank 12 for the archer table and 1 for regular, flying and mage. The corrected run resumed the same battle slot. In the Chinese `zh-CN-fight01` run, ordinary Save published HERO 2 and the driver completed its post-save return. After `END fight`, process shutdown exited **−1073741795 (0xC000001D)** in `godotsharp_internal_refcounted_disposed → GodotObject.Dispose(Boolean) → GodotObject.Finalize → System.GC.RunFinalizers`. A fresh process loaded that exact published slot, used EGRESS and saved on Map36, exit 0. Whether the shutdown fault shares a cause with [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) is **Unknown**.

## Current battle spells

**Standard rigor.** [#1964](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1964) connects the current retail spell records to the existing attack and healing actions. The private catalog selects source ranks from admitted ally growth and current books, enemy books and item use. A packed highest rank expands to its lower ranks. Actual action selection still checks the live caster's book, MP, status and target. Cancellation before commit preserves HP, MP, EXP and RNG.

| Source action | Terms and current boundary |
| --- | --- |
| BOLT 1 | Bowie first learns it at SDMN 22. HERO inherits his learning list and retains the book. The source rank costs 8 MP, has power 14, opponent center range 1–2 and Manhattan radius 1. The attack uses lightning resistance and HERO's integer 5/4 power modifier, giving power 17 before resistance. |
| HEAL 4 | Sarah learns it at effective level 40. It costs 20 MP, targets one living teammate or self at range 0–1 and uses power 255 to restore all missing HP. Sarah kept the learned rank after promotion and cast it as VICR. [Vicar evidence](ordinary-promotion-classes.md#vicar-ordinary-reproduction) |
| AURA 1 | Karna learns it at effective level 33. It costs 7 MP and restores up to 15 HP as PRST or 18 as VICR per eligible recipient in source-slot order within Manhattan radius 1 of a legal teammate center at range 0–3. Engine cases cover both classes and the shared healing EXP cap. Ordinary acquisition and casting in the connected campaign remain **Unknown**. [Vicar evidence](ordinary-promotion-classes.md#vicar-ordinary-reproduction) |
| AURA 2–4 | Karna learns these at effective levels 41, 55 and 60. Controlled ordinary PRST2/3 casts and VICR4 casting with cold Continue are confirmed. [Recovery rules and reproduction](../domain/gameplay-rules.md#private-learned-aura-ranks) |
| DETOX 4 | Sarah's learned threshold is effective level 45. It costs 16 MP and treats poison, stun and curse. Battle Magic uses a teammate center at range 0–3 and Manhattan radius 2. Field Magic treats one joined member. [Rules and reproduction](../domain/detox-ranks-design.md#detox-4-area-and-field-admission) |

The source operands are from the pinned USA disassembly's spell table, element table and ally growth. The [spell-resolution contract](../../../docs/design/contracts/battle/spell-resolution.md) owns the damage, promoted adjustment, full recovery and targeting evidence. [Spell terms](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm) · [Elements](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spellelements.asm) · [Bowie](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats00.asm) · [Sarah](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats01.asm) · [Karna](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats15.asm)

**Confirmed in the remake.** The ordinary Battle26 repeat from the accepted English HERO 2 level-up slot has no HEAL 4 enemy. Both language settings use that same seed. Sarah's HEAL 4 spends 20 MP (29 to 9), restores Bowie from 32 to 62 HP and earns 12 EXP. Bowie's BOLT 1 spends 8 MP (16 to 8), damages the Dark Bishop for 15 HP (39 to 24) and earns 19 EXP. Both casts were canceled at target selection and reselected before committing. Sarah's cast returns to Bowie in round 5 and Bowie's cast returns to Peter in round 5. Battle Save, fresh title Continue and a second save produce equal complete decoded battle data in each language. The Simplified Chinese choices display 治疗 4 and 雷击 1, and their battle results and EXP messages use Simplified Chinese. Bowie's saved player name remains Bowie.

Use the registered `handoffs/issue1964/Review.ps1` with the unchanged promotion handoff and Pacalon inputs. The driver copies `receipts/en-control01/level-slot.json` into a fresh external player-save namespace per language. It drives ordinary Magic, target, cancel, confirm and Save inputs, then compares the whole decoded battle record after cold Continue. The runner stores receipts under the executing checkout's ignored `local/issue1964/receipts`.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv run sf2 verify engine
uv run sf2 verify adapter
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj -c Debug --no-restore
$repo = (Get-Location).Path
$handoffs = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs'
foreach ($language in 'en', 'zh-CN') {
    $journey = 'review-' + [guid]::NewGuid().ToString('N').Substring(0,8)
    foreach ($mode in 'cast', 'cold') {
        & pwsh -NoProfile -File "$handoffs/issue1964/Review.ps1" -Repo $repo `
            -PromotionHandoff "$handoffs/issue1955" -PacalonInputs "$handoffs/issue1927/pacalon" `
            -Journey $journey -Language $language -Mode $mode
        if ($LASTEXITCODE) { throw "Inspect $language $mode receipts and native exit" }
    }
}
```

BLAZE 4, FREEZE 3–4 and BOLT 3–4 are available through learned private Magic.
BOLT 2 was already bound. BLAZE, FREEZE and BOLT high ranks use the existing elemental placeholders.
The learned player rank actions in [#2016](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2016)
are admitted through the current Battle26 boundary. Retail elemental animation variants remain with
[#2104](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2104).
[High-rank Magic](../domain/gameplay-rules.md#high-rank-elemental-magic)

Sarah used SLOW 1/2 and Karna used BOOST 1 and 2 through the ordinary Battle26 Magic command. The BOOST 2 observation starts from controlled legal PRST 30 growth. [SLOW and BOOST design](../domain/support-spell-status-design.md) · [Learned DETOX](../domain/detox-ranks-design.md) · [MUDDLE, DESOUL and SLEEP](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1967)

All four candidate processes exited 0. The Simplified Chinese cast process and both cold processes reported ObjectDB leak warnings after the driver completed Save. A connection to the native shutdown fault under [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) remains **Unknown**.

## Which parties can exercise it?

**Confirmed** in the remake. The Battle26 repeat used a level 27 Bowie. Both Pacalon journeys reached a church, searched the Wing and cold-continued. Reading the accepted castle slots gives the following starting points. Slots below count from zero. These are examples for play, never conditions in the rule. [Battle26](battle26-higins.md) · [Pacalon castle and reproduction](pacalon-first-visit.md) · [Campaign characters](../content/campaign-characters.md)

| Saved party | Ordinary candidate | Wing holder | KNTE candidate |
| --- | --- | --- | --- |
| English | Bowie, SDMN 27. Kazin, MAGE 27, is fallen. | Kazin, slot 2 | Eric, joined KNTE 27, is fallen. Also Chester, KNTE 1. |
| Simplified Chinese | Bowie, SDMN 27. Kazin, MAGE 29, is alive. | Slade, slot 3 | Chester, KNTE 1. The seeded Eric row has no joined flag. |

To check these facts, open `slots/castle-en.json` and `slots/castle-zh-CN.json` read-only in the registered Pacalon handoff. Decode the envelope’s base64 `payload` as UTF-8 JSON. Read `data.allies` by actor alongside the joined flags, stored level/class, HP and the four item words. Item 118 is the Wing and F175 records Search. For play, copy the slot into a fresh external acceptance namespace as described below. [Save input and output](battle-save-and-continue.md#manual-save-history-and-developer-files)

For PGNT acceptance, use the existing English history with joined Eric (KNTE 27) and Kazin holding the Wing. Use copies of this same save to exercise the cross-holder transaction with English and Simplified Chinese UI. Eric can be raised before deployment. On the original Chinese save, check Chester’s low-level refusal and verify that Slade still holds the same Wing in slot 3. An independent natural Chinese PGNT history remains **Unknown**. Reuse an existing legal Chinese Eric branch if one becomes available. This acceptance budget excludes training Chester from 1 to 20 or replaying Creed for another sample. Actual engine tests cover variation in legal levels, candidates and holders. [PGNT acceptance decision](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1957#issuecomment-7025579892)

## Original rules that the implementation must preserve

**Confirmed** here denotes source structure in the pinned USA disassembly unless a runtime owner is named. The baseline is the [USA retail manifest](../../../manifests/roms/sf2-us.json) and [SF2DISASM at c834c652](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6). The [progression synthesis](../../../docs/design/synthesis/themes/progression-and-economy.md#definitions-current-class-and-promotion) and [service contract](../../../docs/design/contracts/party/service-interactions.md#church-source-boundary) trace the caller. Following its callees resolves the stat writes at source level. **Unknown** remains the complete original runtime transaction across the church caller, exact audiovisual timing and a promotion-specific SRAM reload. Existing Raise, Cure and level-up observations cover their named seams.

### Selection and custody

`CountPromotableMembers` rebuilds the joined force in member-index order, copies the whole list for selection and counts entries with a regular base mapping and stored level ≥20. Zero ends the action. Otherwise the player chooses a member. An unmapped class or low level gives its own refusal and returns to selection. A fallen or reserve member is still joined. Neither the count nor the selected-member check tests HP, deployment, a chapter flag or gold. Declining the initial consent returns to selection. Cancelling the member list takes the service-exit conversation. [Force list](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/battleparty.asm) · [Count and tables lookup](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_2.asm) · [ChurchMenu](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L273-L474)

Read paired rows by their table positions. The five special items are consumed only on the final accepted special route. [Promotion tables](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/promotions.asm)

| Base | Regular | Special |
| --- | --- | --- |
| SDMN | HERO | — |
| KNTE | PLDN | PGNT with Pegasus Wing |
| WARR | GLDT | BRN with Warrior Pride |
| MAGE | WIZ | SORC with Secret Book |
| PRST | VICR | MMNK with Vigor Ball |
| ACHR | SNIP | BRGN with Silver Tank |
| BDMN / WFMN / RNGR | BDBT / WFBR / BWNT respectively | — |
| PHNK / THIF / TORT | PHNX / NINJ / MNST respectively | — |

For a mapped special class, scan the joined list, then each compact pack from held-count minus one down to zero. The caller compares the low byte of the slot word to the promotion item. The first match identifies a **holder and slot**, which may belong to the candidate, a fallen ally or a reserve. The depot is outside that scan. Absence or declining the special offer leads to a separately confirmed regular offer. Declining regular returns to selection. Accepting SORC first asks for consent to erase the book. Declining this second prompt restarts selection with the Book intact. Only after the last Yes does the special route remove one item from the selected holder, shift later slots and refresh the holder. The regular route spends nothing. Success returns to member selection. [Church caller](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L324-L474) · [Slot count and removal](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm)

### What changes on success?

| State | Static evidence and consequence |
| --- | --- |
| Class and class bases | Church calls `SetClass` then `Promote`. The latter reads the **new** class, calls `LoadAllyClassData` and then `UpdateCombatantStats`. The class row supplies base MOV, resistance word, movement type/AI-commandset byte and base prowess. For example, HERO retains MOV 6 but changes Bowie's double/counter settings. PGNT uses FLYING while PLDN uses CENTAUR. These affect later actions even when the status screen numbers look similar. [Promote and class load](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/newgame.asm#L109-L144) · [Class definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/classes/classdefs.asm) |
| Accumulated stats and live vitals | The promotion path writes neither max nor current HP/MP, nor earned base ATT/DEF/AGI. Preserve damage, spent MP and death. Initialize current ATT/DEF/AGI/MOV/resistance/prowess from their bases, apply retained status modifiers, then apply effects of equipped items in slot order and write status including recomputed curse. Do not roll growth, heal, or add a distilled promotion bonus. [InitializeCurrentStats and refresh order](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm) · [Bounded growth refresh observations](../../../docs/research/party/ally-growth.md) |
| Carried items and equipment | Ordinary gear and its flags survive. MMNK and NINJ specifically check for an equipped weapon, clear the weapon equip bit via `UnequipWeapon` and refresh again. The weapon stays in its slot, the ring remains independently equipped. Other destinations have no blanket strip or auto-equip. If holder equals candidate, use the already compacted pack for subsequent work. [Church exceptions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L432-L470) · [Unequip by type](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm#L535-L580) |
| Book, presentation, level and EXP | SORC clears four spell slots and learns DAO 1. Every other destination retains the existing packed book, including ranks learned before a late promotion. The caller requests promotion music, waits, updates the ally mapsprite where an entity is available, reports success, sets **stored level 1 and EXP 0**, and loops. No payment or separate saving step occurs in the rite. [SORC defaults and mapsprite helper](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_2.asm) · [Church completion](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/church/churchactions_1.asm#L432-L479) |

On the **next earned level**, `LevelUp` selects the ally’s block for the new class. It uses the stored level for growth, the promoted cap of 99 and the new level +20 for promoted spell thresholds. `useFirstSpellList` inherits the ally’s first list. Existing LearnSpell rules handle matching base spells and upgrades. HERO Bowie at level 1 can already know BOLT from SDMN 22. Reaching HERO 2 must neither duplicate BOLT nor discard a higher retained rank. The class-11 TORT spell comparison is a documented source exception distinct from class admission or cap. Promotion does not replay levels 1–20 or replace earned stats with the destination block’s starting column. The original save copies the combatant data containing this state. [LevelUp](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/levelup.asm) · [Bowie growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats00.asm) · [Save representation](../../../docs/design/contracts/system/save-system.md)

## Remake decision

**Store one current source class ID on the SF2 ally’s existing `BattleActorProgress`.** Seed it once from the admitted starting class, including already-promoted recruits such as Higins (PLDN) and Rohde (BRGN). The field’s carried actor, deployed battle actor and retained reserve already exchange that progress. Other profiles and actors without SF2 progression keep their existing contract. An actor/class pair selects immutable growth, class attributes and appearance data; only the ID and earned member state travel. A new game or session sharing the package must still see the original definitions. [Actor progress](../../src/Sf2.Remake.Domain/Battles/State/BattleGrowthDefinition.cs) · [Carried actor input](../../src/Sf2.Remake.Domain/Battles/State/BattleStartInput.cs) · [Starting classes](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ClassRules.cs)

Changing the shared deployment would leak a promotion into another battle or session. A story flag, inventory receipt or second party/class dictionary would require reconciling two authorities at every join, retreat and save. A promoted boolean cannot distinguish PLDN from PGNT. Select the finite content pair once at battle construction/restoration and give existing rules the resolved actor definition. Use the same resolution for field queries. Keep encounter placement, control, AI orders and actor identity from the deployment. Do not serialize the resolved definition or derive the current class from level, flags, equipment, sprite or starting occupation.

| Boundary | Starting assumption at design acceptance | Required behavior |
| --- | --- | --- |
| Content | The campaign producer already reads a named ally/class block and resolves list inheritance. The reader keys growth by actor alone and validates it against the startup class. The private reader selects class rows from the start roster. Weapon masks are built from the mounted classes. | Keep initial roster rows distinct. Mount additional legal destination pairs for supported actors, with class bases, inherited or explicit spell lists, packed-rank resources, equipment masks and admitted actions. Resolve the pair in memory from validated content. Missing or duplicate terms fail explicitly. [Producer](../../../src/sf2tool/remake_exploration_content.py) · [Growth reader](../../src/Sf2.Remake.Content/Scenarios/BattleGrowthReader.cs) · [Private definitions](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) · [Weapon masks](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs) |
| Domain | Class rule, movement, promoted flag, prowess and growth sit on the deployment definition. Growth constructs a fresh progress record. Start validation can require stored level ≥ deployment level. | Resolve the current class before start validation, movement, AI target priorities, damage/reward and magic queries. Preserve its ID on every progress copy, including level-up. A converted Eric can legally enter at PGNT 1 below his recruited level. Reuse growth, LearnSpell semantics, equipment and status calculations. Source-specific comparisons (promoted EXP, spell threshold, repeat upgrade) keep their own rules. Confirm an attack and subsequent EXP/level, weapon range, class prowess and spell access; admit casting/healing per supported spell/action. [Actor definition and state](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) · [Start validation](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) · [Growth](../../src/Sf2.Remake.Domain/Gameplay/Sf2/BattleGrowthRules.cs) · [Reward rule](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs) |
| Application | Church currently enumerates Raise/Cure, prices Raise using the deployment’s promoted flag, and commits one immutable party replacement. Party inspection and class-name text resolve the deployment, as does the leader class for repeat enemies. | Add a promotion-specific selection/offer to the existing tokenized church session. Project current class in Party, dialogue, Raise cost and battle inspection. Resolve it before repeat quotient, normal battle entry and cold battle restore. Preserve it on victory, defeat, EGRESS and reserve/active exchange. [FieldService](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldService.cs) · [Party](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs) · [Dialogue](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs) · [Battle entry](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) · [Inspection](../../src/Sf2.Remake.Application/Runtime/Battles/BattleUnitProjection.cs) |
| Godot and appearance | The church diamond already has a Promote caption/icon, bound to a null command. Map entities use static/flag-selected sprites. Caravan walking has a startup-class check. The battle-scene reader binds member/class/weapon appearances but compares against starting class. | View emits candidate/consent commands and renders Application’s list, reason, holder, item and class. Refresh bound field entities at commit, reentry, disembark and restore, preserving facing/position and current vehicle/death appearance. Resolve battle mapsprite, scene sprite/palette and equipped/unarmed variant from live member/class. Reuse unaffected portraits, church rasters, text controls and input latches. Mount required variants before offering a usable route. [Church view](../../game/src/Exploration/ChurchServiceView.cs) · [Map sprites](../../src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs) · [Caravan](../../src/Sf2.Remake.Application/Runtime/Exploration/CaravanRuntime.cs) · [Battle scene reader](../../src/Sf2.Remake.Content/Scenarios/BattleSceneContentReader.cs) |

### Commit and refresh boundary

1. The church lists joined members once the source count admits selection. Distinguish an ineligible member from a legal destination whose content is still unavailable. Keep eligibility independent of language, actor name, level-27 witness, item receipt flag and current chapter. In a partial release, show an unavailable branch explicitly; silently treating an unmounted special offer as a missing item would change the player’s choice.
2. An offer identifies candidate, current class, destination, and for special use the matched actor, slot and item word. Tie it to the session revision, wait token and prompt stage. Confirm against the live joined list, level, class, ordered inventory match, and all operands needed for the class, refresh, spellbook and visible appearance. A stale Yes, moved item or missing binding returns a reason with the authoritative state intact. A deliberate No or Back changes only the dialog branch.
3. Build the member replacements off the current party. For special use, compact the holder first with the existing four-word removal, including its refresh. Apply destination class bases to the candidate, then the SORC book or MMNK/NINJ weapon exception as applicable. Recompute the final admitted status/equipped effects from earned bases; apply each bonus once. Keep packed slots and their derived spell/range/stat views consistent. If the same person holds the item, fold both edits into one replacement. If any required status/equipment effect is unsupported, abort the whole command before spending anything. [Field inventory](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldInventory.cs) · [Equipment](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs)
4. Publish one party/field result with level 1, EXP 0 and the updated appearance. Return to member selection, then through church exit to ordinary field input. Reopening or repeating the last command cannot reapply the rite. Party order, membership, gold, RNG streams, kill/defeat counts, unrelated slots, depot, flags and save anchor carry through. A settled Save is a separate player action. Single publication and token checks are modern transaction decisions; the assembly gives an ordered sequence.

### Saving a promoted ally

**Field.** Existing field formats 1 and 3 already require `SavedAlly.Class`. Write the live ID there. Replace the current comparison against the first deployment’s `Growth.ClassId` with validation that this actor admits the saved class (starting class or supported destination). Reconstruct with the saved bases, vitals, book and loadout. Existing field records contain their actual start class, including PLDN/BRGN recruits. Keep the existing compatibility identity, roster upgrades and scene recipes for those histories. [Field payload](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSaveData.cs) · [Capture](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs) · [Restore](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs)

**Battle.** Format 2 carries progress for deployed actors and the field-player’s retained allies but has no class field. Select a new top-level battle format (next free version, **4** at this baseline) for saves carrying a changed class. Require the class on every SF2 ally progress in that version, deployed and retained. Reuse the existing battle payload/codec and keep its independent enemy-replacement recipe selector. On legacy v2 decode, materialize the historical starting class before validation and reconstruction equality. A missing class in the new format is an error. An older reader already protects unknown top-level versions. A v2 optional class with an unconditional start-class default could silently demote a new save or route an unfamiliar payload through a shape error. Keep unchanged parties eligible for the existing writer. Check version dispatch, slot listing and publication in addition to the data round trip. [Wire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) · [Dispatch](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs) · [Battle reconstruction](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleRestore.cs) · [Continuation](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationRestore.cs)

**Book validation is part of compatibility.** Before slice 1 both restore paths compared a known spell’s learn threshold to stored level. HERO 1 with BOLT and WIZ 1 with a late MAGE spell would fail. Decode the four packed slots, enforce distinct base IDs and legal ranks, resolve all lower castable ranks, and check against the actor’s reachable pre-promotion learning up to the base cap and the destination’s learning through its effective level. Handle SORC using DAO and its destination list only. Preserve the narrower original bounds for legacy starting-class histories and their initialization rules. The book records what was actually learned; do not synthesize missing spells, require a promotion-level history, or accept arbitrary spells from another class. Admission must validate level, carried gear, book and actor/class pair before publishing a slot or replacing a live session. Do not infer missing content by downgrading to the start class. Retain prior slot, backups and completed failure evidence. [Learning contract](../../../docs/design/contracts/party/level-up.md) · [Save history and recovery](battle-save-and-continue.md#manual-save-history-and-developer-files)

### Interface for rings and Black Ring

Expose current source class to the equipment **and** ordinary player item-use queries. Existing equipment identifies one weapon and treats other admitted bare items as inert. Ring equipment needs its own equipped ring selection alongside the weapon in the same four carried words. Weapon unequip on promotion must leave the ring and its contribution intact. The later equipment slice owns adding/removing the ring contribution, refreshing curse and other status/terrain/turn consumers, inventory restrictions and cold restoration. Promotion supplies the class and refresh interface. Other reachable rings (e.g. AGI from Quick Ring or MOV from Running Ring) can use that slot independent of Black Ring’s class requirement. [Equipment owner](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs) · [Campaign equipment](../content/campaign-items.md#special-equipment-and-attack-items)

**Confirmed** in the pinned player battle menu. Both the range preview and `@IsUsable` call `IsItemUsableByCombatant`. For equipment this calls `IsWeaponOrRingEquippable`, which tests the item mask against the combatant’s **current** class, then `IsItemUsableInBattle`. Black Ring (24) specifies WIZ/SORC, CURSED, BREAKABLE, ATT +10 equipped and BLAZE 2 on Use. The ordinary Use check applies even to a bare carried ring; equipped status supplies the stat/curse effects, not admission to the class mask. There is a subsequent target and action path. Enemy AI admission and the helper’s general no-spell sentinel defect do not widen this player case. [Ordinary Use and preview](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L370-L449) · [Class mask and Use helper](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/itemstats.asm) · [Black Ring definition](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/items/itemdefs.asm#L263-L271)

The earliest evidenced route available from these parties is **ordinary MAGE → WIZ for Kazin**, without a Secret Book. Use a history that actually kills the Battle26 carrier and receives the ring, give it to the promoted mage and check class refusal on the prior holder. The main Pacalon slots and the separate Chinese drop witness are distinct histories. SORC becomes a second eligible route once both the Book grant and its rite are playable. [#1926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1926) owns ring Equip, curse/Cure, BLAZE 2 targeting and ally durability through actual use, refusal, cancellation and cold Continue. Neither WIZ admission nor the earlier drop proves those operations work. [Battle26 custody](battle26-higins.md)

## Implementation order and acceptance

Each slice requires independent acceptance before its dependent work starts. Pair narrow Domain behavior tests with actual session commands and the running adapter. For shared generator, save, content, localization, test and ledger paths, reserve a single writer on the then-accepted main. The following paths name the change owners for dispatch. Paths beginning with `Domain/`, `Application/` and `Content/` below are relative to the respective `remake/src/Sf2.Remake.*` project; tests are in `remake/tests/Sf2.Remake.Engine.Tests/`.

**1. Ordinary rite, from the church back to playable HERO.** Own the progress/definition, content, field service, presentation and persistence owners linked above. Bind Bowie’s SDMN→HERO first. Use class/actor content rows for admission so that another eligible actor or legal level can use the same transaction. Keep a table-known destination with missing action/growth/appearance terms visibly unsupported before confirmation. Include the following integration points in this slice:

- **Class, growth and battle carry.** `Domain/Battles/State/BattleGrowthDefinition.cs`, `EngineBattleState.cs` in the same directory, `Domain/Gameplay/Sf2/{Sf2ClassRules,BattleGrowthRules,OpeningPartyInitialization}.cs`, `Domain/Battles/Rules/{BattleEquipment,BattleTurnFlow}.cs`. Bind against current class at `Application/Runtime/Exploration/BattleEntry.cs` and its return boundary in `Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs`. Preserve rules for existing start classes, reserve partition, actor-specific spells and promoted reward/spell-power inputs.
- **Content and visual selection.** `remake/content/private/pre-voyage-opening.json`, `src/sf2tool/{remake_exploration_content,remake_exploration_programs,remake_battle_scene_content}.py`, `Content/Scenarios/{BattleGrowthReader,PrivateBattleDefinitionReader,PrivateBattleScenarioReader,PrivateFieldPreparation,BattleSceneContentReader}.cs` and `Application/Content/Scenarios/{PrivateBattleDefinitions,BattleSceneDefinition}.cs`. Keep the existing initial member list singular. Merely appending a second Bowie row to `growthClasses` would also feed start roster/scene consumers. Reuse the parsed curves and source class definitions for destination rows. Select promoted field/map and battle sprites, variants and palettes privately; reuse existing resources where their identity matches. Inspect the mounted church art/music and retained map overlays before selecting missing assets.
- **Service, text and projections.** `Application/Runtime/Exploration/{FieldService,FieldInventory,PartyDetailsQuery,ExplorationTextRunner,SceneEntities,CaravanRuntime,MapTransfer}.cs`, `Application/Runtime/Battles/BattleUnitProjection.cs`, `remake/game/src/Exploration/ChurchServiceView.cs`. Keep promotion prompts distinct from paid treatment. Use `remake/game/src/Localization/GameText.Keys.cs`, `remake/game/localization/{en,zh_CN}.po` and the existing display catalog for class/item names, custom ally names and source text. Provide both languages for member selection, eligibility, unavailable content, confirmations, cancel and success. Reuse diamond and Yes/No input, page/scroll a long joined list, preserve focus/neutral release and return ordinary controls. Test actual text bindings and clipping on the running view.
- **Save and tests.** `Application/Runtime/Persistence/{OpeningSave,OpeningRestore,BattleSave,BattleRestore,BattleSaveStory}.cs`, `Domain/Battles/State/{BattleContinuationData,BattleContinuationRestore}.cs`, `Content/Persistence/{OpeningSaveJson,BattleSaveWire,SaveRecord}.cs`. Extend the actual behavior cases in `FieldServiceTests.cs`, `BattleGrowthTests.cs`, `OpeningSaveTests.cs` and `BattleSaveProgressTests.cs`. Include level 19 refusal, exact 20, late promotion with a learned spell, ineligible/already promoted, damaged or fallen member, reserve, stale/cancel/repeat, no gold/depot changes, attack bonus applied once, promoted growth and return. Exercise field v1/v3 and battle v2 legacy input, changed-class battle save with a retained ally, invalid pair/book rejection, and field/battle cold Continue. Keep the comparison of the complete saved party, not only a successful load message.

**Native exit criterion for slice 1.** Use both actual castle parties. At an ordinary priest, decline once, promote Bowie on a separate attempt, inspect HERO 1/0, vitals, book, weapon, class name and sprite, leave and Save. Cold Continue, visit another map or board/disembark, deploy into an admitted repeat, inspect movement/weapon and complete an ordinary attack and EXP/level-up with the HERO binding. Save/Continue at stable battle input and finish or EGRESS back to a playable field. Confirm retained book/rank access, no duplication, coherent geometry and no new errors. Vary the source level and equipment in engine cases. Broader earlier encounter appearances remain explicit wherever a private package lacks a required variant. Expand ordinary rows next where a live consumer needs them, notably KNTE→PLDN for regular fallback and MAGE→WIZ for Kazin; accept an ordinary cast with the retained book when admitting WIZ. The NINJ row additionally owns weapon unequip. Do not present all twelve destinations as playable because HERO passed.

**2. Special item and the first PGNT.** Depends on the accepted class/save/refresh path and KNTE→PLDN admission. Extend the same campaign selection, generator, class/scene readers, `FieldService.cs`, `FieldInventory.cs`, `ChurchServiceView.cs`, bilingual catalogs and `FieldServiceTests.cs`. Use `Domain/Battles/Rules/BattleInventory.cs` for slot compaction. Bind the source mapping and PGNT class, flying movement/terrain, weapon and member-specific appearances. Test empty/depot-only, first of multiple matching holders and reverse slot precedence, reserve/fallen holder, same member in both roles, stale slot after Give, special No → regular No/Yes, and unsupported destination. Distinguish Wing consumption from F175, including after a fresh castle visit. On separate branches from ordinary Search, decline and retain the Wing, or accept and remove exactly one Wing. Check candidate, actual other holder, rest of pack, F175, zero EXP, menu exit, flight and a legal battle action, Save, cold Continue and attempted repeat. Use the English Eric history for both UI languages and the original Chinese save for low-level refusal and unchanged item custody, as specified under [party selection](#which-parties-can-exercise-it). Show the full-pack acquisition refusal where the receipt itself is at issue. Preserve any failed run.

**Subsequent special rows.** Reuse slice 2’s files per actor/class pair. For BRN or BRGN, first observe ordinary pickup and holder for Warrior Pride or Silver Tank. SORC/MMNK require Map23 Search admission for the Secret Book/Vigor Ball in the current selection. SORC must exercise warning No (no spend, no book change), final Yes (DAO-only), a later destination spell, and cold book restore. MMNK and ordinary NINJ must keep the weapon carried, clear its contribution and leave an independent ring alone where supported. Accept changed valid states with and without an equipped weapon. Treat spell and equipment actions beyond the mounted cases as explicit dependencies. The item index distinguishes confirmed placement, selection and actual pickup for each item. [Promotion item lifecycle](../content/campaign-items.md)

For each delivered behavior, update its row in [campaign items](../content/campaign-items.md), [characters](../content/campaign-characters.md), [locations](../content/campaign-locations.md), the applicable [flag ledger](../content/campaign-flags.md) and the [capability trace](../capability-status.md#campaign-feature-trace). Describe the actual reach, remaining unsupported cases, owning tests and ordinary receipt. The implementation ticket should reserve those exact rows and affected files against concurrent work before editing.

### Inputs, commands and observation budget

The registered Pacalon package already contains a 101,760,092-byte world and a 577,176-byte main scene selection. Use existing inputs, install, configured project and running instance for inspection. For a pre-implementation binary, mount the changed Debug adapter for that check and restart for the cold-Continue check. For each class variant, start with one actor/class growth block (five curves of at most 29 fraction pairs), a class row and only the required sprite/palette/weapon resources. Measure the first private asset delta against existing package limits before adding another variant. Aim for a few kilobytes of numerical metadata, bounded private visual additions measured on the pilot, and a handful of compact state/slot receipts per language. No new full world per attempt. If the combined world must be regenerated for changed content, prepare one checkout-local package per accepted selection and reuse it. Keep the stable package and legal slots read-only. [Input and tool setup](../../../docs/operations/local-private-inputs.md) · [Pacalon reproduction](pacalon-first-visit.md#reproduction)

Implementation commands from the owning checkout, after loading its private configuration:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify engine
uv run sf2 verify adapter
uv run sf2 verify plan --base origin/main --head HEAD
```

For ordinary play, mount the Pacalon handoff’s `player-config.json` inputs as in its `Play.ps1`, with the implementation’s changed world/scene rows and current Debug adapter. Keep the disposable host profile, TEMP and logs under the owning checkout. Select an absolute operator-owned `$operatorSaveRoot` outside every checkout, with no linked ancestor. Ordinary Save rejected an in-checkout root in the Healing Drop pilot. Give each acceptance run a fresh namespace under the external root so that play only updates its copy. [Save storage policy](opening-session-lifecycle.md#format-compatibility-and-local-storage) · [Healing Drop pilot](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1953)

Set `$game` to the configured Godot project, `$settings` to the language settings, and `$seed` to the matching read-only Pacalon castle slot. Both castle seeds have embedded slot number 1. Run the setup once, then reuse the same environment and slot for the cold launch. This exercises the ordinary player store through title Continue, Save and Quit, and fresh-process Continue. The separate developer-file workflow uses `--load-save` and `--save-output`. [Save file modes](battle-save-and-continue.md#manual-save-history-and-developer-files)

```powershell
$env:SF2_SAVE_ROOT = $operatorSaveRoot
$env:SF2_SAVE_NAMESPACE = "promotion-$([guid]::NewGuid().ToString('N'))"
$slotDirectory = Join-Path $env:SF2_SAVE_ROOT "$env:SF2_SAVE_NAMESPACE/sf2-opening"
New-Item -ItemType Directory -Path $slotDirectory -ErrorAction Stop | Out-Null
Copy-Item -LiteralPath $seed -Destination (Join-Path $slotDirectory 'slot1.json') -ErrorAction Stop
& $env:GODOT_BIN --path $game -- --input-settings $settings
# After Save and Quit, select Continue again in the same namespace.
& $env:GODOT_BIN --path $game -- --input-settings $settings
```

Read the existing exploration/battle `ReadObservationJson`, scene projection, church wait, item words, save/entry observations and native errors. Use mapped controls; keep screenshots, frame capture, manually edited saves and public private-content attachments out of acceptance. Compare decoded progress before/after across all actors, flags, gold, depot, seeds and actual slot/class/book; check stale input after reentry and successful ordinary control return. Compare level-up outcomes against an uninterrupted run from the same state/RNG. Separate existing shutdown failures from new action/restore errors. Use the clean committed planner and record actual PR checks. [Engine/adapter verification](../development-and-verification.md) · [Native save and control](battle-save-and-continue.md) · [Known Pacalon shutdown evidence](pacalon-first-visit.md#reproduction)

## PGNT pilot ownership and compatibility

Use a finite class-ID lookup on the immutable deployment for the admitted ordinary and
special destinations. The live word stays in `BattleActorProgress.ClassId`. Growth,
equipment, movement, AI priorities, field and battle Continue, and both battle and field
appearance resolve the same ID. Map populations normalize the previously published
scalar promotion pair and the multi-destination rows into one lookup. The scene keeps its
existing member/class/equipped-item key. An encounter-specific class swap or another
persistent class field would make ordinary fallback and return depend on the entry route.

The first package selects Eric alone for PGNT and retains KNTE and PLDN in the same
selection. The existing packed Wing in a joined pack determines the offer. Verify both
loadouts, target class and its mounted appearances before committing the existing church
answer, then write the candidate and holder together. Same-holder promotion has one pack
and one actor update. Restrict the pilot to the existing five-way selection's KNTE route.
Other special offers retain their current unavailable response. The source priority order
is 11, 23, 3 and 30 in the Regular, Flying, Archer and Mage tables respectively, counting
from zero. [Source class ordering](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm)

**Confirmed in the remake.** Eric uses the Pegasus Knight class definition (14), its own
growth, Flying MOV 7, walking sprite 35 and battle body 17/palette 2. Battle26 binds the
unarmed body and all nine weapons admitted by the current PGNT mask and weapon rules
(56 and 72–79). Chester starts with Wooden Stick (56), and the mounted Map9 shop
lists Power Spear (76). Ordinary acquisition of Chrome Lance (79) by this frontier is
**Unknown**. The appearance follows the equipped item after Give and Equip as well as
at promotion. HEAL target admission uses the same actor/class/item lookup and idle ticks.
The ordinary attack is 17, weapon graphics 12–13 select throw 82, and dodge is 57.
GetBattlesceneGround skips the ally ground for both Flying and Hovering movement.
[Attack selection](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/getallyanimation.asm#L12-L35) ·
[Ground selection](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_2.asm#L11-L25)

The starting, ordinary and special
walking choices for Eric are 33, 34 and 35 in the same package. Chester and Rick retain
their ordinary paths. Their PGNT variants have no content binding in this pilot.
[Eric's source growth](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/stats/allystats14.asm) ·
[Member battle sprites](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/allies/allybattlesprites.asm)

### Reproduce Eric's special promotion

The opt-in `pgnt` mount starts from the registered `derived/issue2106/pre-voyage-spells-v2`
and `derived/issue2001/retail-v1`. The world grows by 7,900 bytes and the Battle26 scene
by 28,658. The closed package totals 314,310,638 bytes across 147 files. The 36,558-byte
increment falls within the 100 KiB estimate. Both rewritten files reproduced byte for byte
against the registered `derived/issue1957/pgnt-v3`. Shared graphics/palette selectors reuse
mounted knight weapon layers. Chrome Lance uses prepared graphic 11 and palette 23 at the
scene's 2× texture size. All 36 mounted frames match the visible colors and alpha of the
prepared operands. The producer reuses the other immutable
dependencies. The companion catalog adds exactly `name.class/14` from the configured
`names.zh-CN.jsonl` to the `names` list of the #2001 catalog. Select its
`namespace`, `index` and `translation` (as `text`), keeping the source identity and 2,221
message rows. The result has 190 names.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --no-sync python -m sf2tool.remake_retail_assets --family pgnt `
    --root (Join-Path $derived 'issue2001/retail-v1') `
    --base (Join-Path $derived 'issue2106/pre-voyage-spells-v2') `
    --upstream $pinnedSource --mount $freshAbsolutePackage
```

Build the Debug adapter in the isolated checkout. Choose a fresh external disposable
save root and an unused journey. The published runner resolves the registered content,
sets the locale and stages its input scripts under the supplied checkout's `local/`.
Each command launches a separate headless process. Use fresh sample names for a retry.

```powershell
$run = Join-Path $derived 'issue1957/review-v3/Boot.ps1'
$seed = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1927/pacalon/slots/castle-en.json'
$sample = @{Repo = (Get-Location).Path; SaveRoot = $disposableSaveRoot; Journey = 'pgnt-en'}
& $run @sample -Mode eric -Sample rite -Seed $seed
& $run @sample -Mode raise -Sample restored-field
& $run @sample -Mode road -Sample battle-ready
& $run @sample -Mode steel -Sample restored-battle
& $run -Repo $sample.Repo -SaveRoot $sample.SaveRoot -Journey pgnt-zh -Mode eric `
    -Sample rite-zh -Language zh-CN -Seed $seed
```

| Boundary | Observed result |
| --- | --- |
| Church and custody | From the real English castle history, fallen Eric changed from KNTE 27/83 to PGNT 1/0. Kazin's pack changed from `[213,5,118,123]` to `[213,5,123,127]`. F175, 5,527 gold, Eric's 36/32/31 base ATT/DEF/AGI, equipped Steel Lance word 203 and empty book survived. Separate English and Chinese runs of the same history named Kazin, the Wing, Eric and the destination in the offer and confirmed his actual new class. |
| Field Continue and Raise | A v3 legacy visit initially refused Save with `field-visit-actor-origin` before promotion. Ordinary exit and re-entry supplied the fresh castle visit. V6 Save sequence 5 and independent cold re-save 6 had equal complete decoded `data`. The runner held simulation time below one field tick for this comparison, then resumed normal time. Declining Raise retained the fallen member. Paying 210 restored 58 HP and left 5,317 gold. Eric was already deployed in this history. |
| Flight and combat | Ordinary travel to Ketto entered the Battle26 repeat. The actor had Flying MOV 7, visible walking sprite 35, body `ally17-palette2/frame0` and his equipped weapon. The first Steel Lance attack missed and earned 1 EXP. The enemy continued, including the Dark Sniper's 14 damage to Bowie. Eric's second attack dealt 15 to that sniper and earned 19 EXP, returning to Movement at 20 EXP. V4 battle Save sequence 8 and fresh-process re-save 9 had equal complete decoded `data`. |
| Wooden Stick and ground | On a separate copy of the raised English field save, use `stick-road` after `raise`, then `stick` in a fresh process. Ordinary Caravan Give and Equip moved Chester's Wooden Stick to Eric. Battle26 saved Eric with attack 39 and pack `[75,184,127,127]`; the complete decoded battle data matched after cold Continue and re-save (8→9). The production scene displayed `ally17-palette2/frame0` and `weapon56/frame0`, with weapon visible, ground hidden and no scene error. Eric dealt 1 damage to the Dark Sniper, received 1 EXP and returned to Movement. The earlier Steel Lance receipt shows the old visible ground. The old missing-stick failure was identified by static lookup. A HEAL specifically targeting stick-equipped Eric was not played in this observation. |
| Limits | Same-holder compaction, stale high-byte refusal, independent ordinary consent, destination growth and target priorities have engine coverage. Natural Chinese Eric recruitment, further special families and original rite timing remain **Unknown**. Some successful headless exits reported ObjectDB leaks under #1716. |

An early driver moved down from the revised system menu’s default selection and missed Save. The initial
localized prompt also described an available Wing as unavailable. The final checks
used the current Save selection and the corrected confirmation in both languages. A fresh local
installation from the published runner repeated the Chinese rite and Save with the producer
scratch held aside.

### Review boundary

The pinned sources, retained inputs and current code resolve the design questions without new emulator acquisition. For a static recheck in a pinned SF2DISASM checkout, use `git show c834c652:disasm/code/common/stats/newgame.asm` and follow the named symbols in the source links above. The actor/class/weapon combinations for future destinations require their own content and observations. Map23 Search remains a dependency for the Secret Book and Vigor Ball routes. The English Pacalon save already supplies the eligible knight and separate Wing holder for PGNT.
