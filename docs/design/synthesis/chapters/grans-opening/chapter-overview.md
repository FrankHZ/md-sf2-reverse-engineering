# Grans Opening and Departure — Chapter Overview

- **Story span:** Ordinary New Game (`Map3` Granseal home) through Captain's committed boarding and
  departure (`Map44` Docks → `Map73` voyage request). The opening storm/temple acts
  (`--opening-act 1–4`) provide background only; the sea voyage, landing, and `Battle08` continue in
  [Voyage and landing](voyage-through-damaged-ship-control.md) and
  [Damaged-ship plank through Battle08](../eastern-parmecia/damaged-ship-plank-through-battle08.md).
- **Chapter arc & world changes:** From a peaceful town morning and first Ancient Tower fight
  (`Battle01`) → royal commission and western Grans search (`Battle02`–`03`, Yeel, Hawel) → Galam
  ambush, captivity, prison escape, and Jewel of Evil (`Battle04`–`06`) → invaded Granseal, tower
  rescue (`Battle07`), town-shattering earthquake, and evacuation by ship.
- **Evidence & boundary rules:**
  - **Original:** Only the Map3–Battle01 opening has bounded original runtime observations
    ([original frontier](../../milestones/map3-battle01-readiness.md#accepted-original-frontier)).
    Later Grans events (`Battle02`–`07`, earthquake, docks) are **Confirmed static** in pinned
    `SF2DISASM` (`c834c652b6862bc5679fd7f69a38a7093206efc6`) and the USA ROM; natural original
    traversal, hardware timing, and original SRAM persistence remain **Unknown**.
  - **Remake:** Ordinary continuous play and v6 settled saves cover New Game through `Map14` first
    control ([pre-voyage campaign](../../../../../remake/docs/application/pre-voyage-campaign.md)).
    Named controlled Debug starts (such as `slade-join` and `castle-quake`) verify specific scene
    transitions and **do not** prove a continuous natural playthrough or carry earned gold/EXP.
  - **Flags are not the whole session:** Story flags (`Fnnn`) select setups and gates, but live
    actors, follower links, working layout copies, and party resources also depend on how the map
    was entered or restored.
  - **Publication note:** Maintained in the canonical repository first; live Wiki synchronization
    happens separately after acceptance.

## Chronological story events (player view)

*Optional does **not** automatically mean missable:* optional stops and pickups below remain
revisitable until a stated state change or committed departure (`G20`) closes them.

### Phase 1 · Granseal morning, castle audience, and Battle01

| ID | Place & player goal | Trigger / choice | What changes & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **G1** | **Granseal home & school** (`Map3`) · Start the day and visit Astral's class | Ordinary New Game starts Bowie at home; walk out and enter school (`Map3` interior). | Home departure (`F601`) and school talk (`F602`) advance town state; Church and school `Search` (`F128`) are already usable. | Mandatory |
| **G2** | **Granseal messenger** (`Map3`) · Answer the summons to the castle | Choose **Yes** or **No** when the messenger appears. | **Yes** recruits **Sarah** and **Chester** (joined, active, and following via `F66`) and unlocks the castle gate (`F600`, `F603`). **No** ends the prompt without joining or unlocking—talk again to accept. | Mandatory (Yes retryable) |
| **G3** | **Granseal Castle** (`Map19`–`21`) · Meet the King, Astral, and the tower guard | Enter castle gate (`F604`), visit the King's bedroom (`F605`), answer Astral (`F607`/`F608`), then speak to the west-tower guard (`Map21`). | Refusing Astral leaves him in place for retry; accepting moves him and allows the guard to open the tower route and arm `Battle01` (`F401`). | Mandatory (refusal retryable) |
| **G4** | **Ancient Tower** (`Map40`/`57`) · Clear the Gizmo encounter (`Battle01`) | Enter the tower antechamber while `F401` is armed. | **Victory** sets `F501`, clears `F401`, and returns to `Map57`, unlocking the castle exorcism setup. **Defeat / EGRESS** keeps `F401` armed for retry. | Mandatory |
| **G5** | **Exorcism, commission & Jaha** (`Map20`/`19`/`3`) · Report back and depart west | Return to the King's room (`F609`), hear the throne commission (`F610`, re-promptable), then exit Granseal west. | `F609` opens Granseal's shops and clears the dock blocker (`Map44` becomes visitable). Leaving west recruits **Jaha** (joined/active, clears Sarah/Chester `F66`) and arms `Battle02` (`F402`). | Mandatory |

### Phase 2 · Western Grans, Yeel, Hawel, and Battle02–04

| ID | Place & player goal | Trigger / choice | What changes & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **G6** | **West road & southern detours** (`Map66`/`18`/`41`/`42`) · Cross toward Yeel (`Battle02`) and optional side areas | Walk west onto `Map66` for `Battle02`; optionally visit the closed Galam drawbridge south shore (`Map18`) or the **Southern Devil's Head & Jewel Cave** (`Map41`/`42`). | `Battle02` victory (`F502`) opens travel toward Yeel (note: victory can land on an enclosed tile recovered via field `EGRESS`). The southern shrine's lower chamber offers `Power Water` (`F133`) with no story-flag gate. | `Battle02` mandatory; detours optional |
| **G7** | **Yeel village** (`Map5`) · Hear about Hawel and gain a guide | Enter Yeel and speak with **Kazin** (optional Church and `Healing Seed` `F134` also available). | Sets **temporary follower** `F72` (Kazin guides Bowie on the field; **not** a combat party member yet). Skipping Yeel does **not** block `Battle03` or Hawel's house. | Optional (not required for `Battle03`/Hawel) |
| **G8** | **Woods to Hawel's house** (`Map66`) · Fight `Battle03` | Cross the north-Yeel overworld zone (`Map66` default zone arms `F403` when `F503` is clear, with or without Yeel). | **Victory** (`F503`) clears passage to Hawel's house. **Defeat / EGRESS** returns to the active savepoint for retry. | Mandatory |
| **G9** | **Hawel's house** (`Map43`) · Learn of Galam's attack and recruit Kazin | Enter `Map43` (default init plays even if `F503` or `F72` was clear); inspect diary/chests (`F135`/`F136`) as desired. | **Kazin formally joins** the force (`F4`/`F36`), clearing temporary `F72`, setting quiet-house `F612` and changed-Yeel `F650`, and arming `Battle04` (`F404`). Revisiting `Map43` skips the first scene (`F612`). | Mandatory |
| **G10** | **Galam ambush** (`Map66`) · Resolve `Battle04` | Leave Hawel's area while `F404` is armed. | **Special outcome rule:** **First victory** triggers Lemon's capture scene and sends the party to Galam Prison (`Map17`); **ordinary defeat** halves gold, restores Bowie, and **also** clears `F404`, sets `F504`, and enters `Map17`. Only **paid EGRESS / Angel Wing** leaves `F404` armed for retry. | Mandatory (victory & defeat both reach prison) |

### Phase 3 · Galam captivity, Slade's escape, and Battle05–06

| ID | Place & player goal | Trigger / choice | What changes & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **G11** | **Galam Prison & tunnel** (`Map17`) · Escape the cells with Slade | Wake up (`F660`), converse/open doors (`F661`), trigger stairs (`F662`; repeat keeps displacement without recopying shortcut), and take the tunnel. | Always writes `F611` (opens Galam drawbridge `Map18` later). The tunnel installs **Slade as a temporary follower** (`F73`, **not** joined yet). Optional prison pickups: `Chirrup Sandals` (`F205`) and `Mithril` (`F206`). | Mandatory (pickups optional) |
| **G12** | **Galam town rally & west guard** (`Map16`) · Cross town and confront the guard | Watch the army rally (`F663`, opening Galam shops/services), then step into the west guard zone. | First discovery arms `Battle05` (`F405`) and requests the warp (`F664` hides the guard on fresh entry). Town shops, Church, and `F207`/`F208` chests are usable; the treasury guard blocks `F137`/`F138` stairs throughout early Galam. | Mandatory |
| **G13** | **Galam Castle battle** (`Battle05`) · Defeat the Knight and recruit Slade | Win `Battle05` (killing the Knight in slot 128 with Bowie alive triggers the special post-scene cleanup). | **First victory** sets the first jewel display flag (`F384`, **not** an inventory item), **formally joins Slade** (`F5`/`F37`, clearing `F73`), sets `F505`, arms `Battle06` (`F406`), and returns to `Map16`. **Defeat / EGRESS** keeps `F405` armed and Slade unjoined. Post-victory castle throne/east-tower rooms are optionally visitable. | Mandatory |
| **G14** | **Galam drawbridge & south road** (`Map18`/`66`) · Fight `Battle06` back toward Granseal | Cross the lowered drawbridge (`F611`; optional upper guard/trader/sign) south to `Map66` while `F406` is armed. | **Entry/victory split:** `Battle06`'s before-program writes `F407` (arming `Battle07`) at **entry**, while **victory** sets `F506` (invaded Granseal) and returns to `Map66`. Defeat/EGRESS keeps `F406` armed. | Mandatory |

### Phase 4 · Invaded Granseal, Battle07, earthquake, and docks departure

| ID | Place & player goal | Trigger / choice | What changes & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **G15** | **Invaded Granseal & optional Kiwi** (`Map3` `F506`) · Check town and decide whether to recruit Kiwi | Enter invaded Granseal; talk to the frightened creature in the open cage (accept & name vs. decline/bypass). | **Accept + confirm name** joins **Kiwi** (`F6`/`F38`) and adds current following; **decline or bypass** leaves `F6` clear so the castle route still proceeds, and you may return later before departure (`G20`) to recruit him. | Optional (revisitable before `G20`) |
| **G16** | **Invaded Castle** (`Map19` `F506`) · Witness Astral/Lemon depart and escort the King & Minister | Walk north through the two independent `Map19` zones (`F620`, then `F621`). | `F620` stages Astral and Lemon heading to the tower (no Lemon join). `F621` installs **King & Minister as temporary followers** (`F83`, **not** combat members). | Mandatory |
| **G17** | **Ancient Tower & Battle07** (`Map40`/`57`/`60`/`58`) · Descend the tower and defeat Dark Smokes | Pass wounded Lemon outside (`Map40`, optional `Mithril` `F132`), descend `Map57`/`60`, and enter `Map58` while `F407` is armed. | `Battle07` intro clears `F83`. **First victory** writes `F385` (second jewel narration; member screen needs `F384`+`F385`), installs the **three-actor royal follower train** (`F74`: Astral → Bowie, King → Astral, Minister → King), sets `F640` (quake town switch), sets `F507`, and returns to `Map58`. Defeat/EGRESS retries without these writes. | Mandatory |
| **G18** | **Castle warnings & earthquake town** (`Map19` `F507` → `Map4`) · Leave the castle through the falling guards | Ascend back to `Map19` (two independent warnings `F622`/`F623`), then exit south to town. | `F640` substitutes **`Map4` (earthquake town)** for `Map3`. **First entry** plays the tremor, chasm block copies, and **both guards falling offscreen**, then sets `F624` (note: castle/tower themselves have no persistent fissure layout, `#2288`). **Revisit** rebuilds cracks and parks guards without replaying the scene. Optional `Mithril` `F217` remains reachable. | Mandatory |
| **G19** | **Evacuation docks & Captain choice** (`Map44` `F507`) · Decide whether to sail now | Exit `Map4` east to `Map44`; speak to **Captain (entity 138)** at `(10,14)` and choose **No/cancel** or **Yes**. | Fixed passengers occupy all three gangplank warps (closing the below-deck `F140` barrel; deck `F139` remains reachable). West exit returns to `Map4` (`F640` set). **No / cancel** returns control cleanly without writing `F89` or clearing `F74`/`F640`. | Mandatory decision (No retryable) |
| **G20** | **Committed departure** (`Map44` → `Map66` → `Map73`) · Watch Granseal fall and sail for Parmecia | Confirm **Yes** with Captain 138. | Stages boarding, **clears `F74`** (ending the royal field train), loads `Map66` with the ship actor, plays the destruction sequence, queues **`Map73`** (`(26,13)`, Right), and **clears `F640`**. Closes Grans exploration and hands off to the voyage. | Mandatory chapter finale |

## Technical & scenario trace (lookup by Event ID)

*This table links key F-IDs, authoritative owners, and existing named scenarios so readers can jump
directly from any event without duplicating a second complete flag registry or scenario JSON.*

| Events | Key flags (lookup only) | Authoritative design / flag / location owners | Named remake starts & observation boundary |
| --- | --- | --- | --- |
| **G1–G4** | `F0`/`F32`, `F78–80`, `F600–605`, `F607–608`, `F66`, `F1–2`/`F33–34`, `F401`/`F451`/`F501`, visit `F256–263` | [Opening through Battle01](opening-through-battle01.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#opening-granseal-and-the-tower) · [Locations](../../../../../remake/docs/content/campaign-locations.md#grans-and-the-voyage) | **New Game** + continuous **Battle01** & `battle01` snapshot (`battle01-movement/slot1.json`); **bounded original runtime** also covers Map3–Battle01 ([opening acceptance](../../../../../remake/docs/evidence/opening-loop-acceptance.md#reproduce-the-composed-route), [showcase starts](../../../../../remake/docs/application/pre-voyage-campaign.md#showcase-starts-and-continuity)). |
| **G5–G8** | `F609–610`, `F3`/`F35`, clear `F66`, `F402`/`F502`, `F72`, `F403`/`F503`, `F133` | [Battle01→02](battle01-return-through-battle02.md) · [Battle02→03](battle02-outcome-through-battle03.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) · [Optional locations](../../../../../remake/docs/application/pre-voyage-optional-locations.md#southern-shrine-and-excluded-connections) | Ordinary continuous remake play covers New Game through Battle03 return & southern shrine ([through Battle03](../../../../../remake/docs/application/pre-voyage-campaign.md#ordinary-new-game-through-the-battle03-return)); **no dedicated standalone scenario JSON** (owner link only). |
| **G9–G10** | `F4`/`F36`, clear `F72`, `F612`, `F650`, `F404`/`F504` | [Battle03→04](battle03-outcome-through-battle04.md) · [Battle04→05](battle04-outcomes-through-battle05.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) | Ordinary remake play covers Hawel recruitment, quiet revisit, Yeel `F650`, and Battle04 victory/defeat/EGRESS ([Hawel](../../../../../remake/docs/application/pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment), [Battle04](../../../../../remake/docs/application/pre-voyage-campaign.md#battle04-and-the-galam-prison-handoff)); **no standalone scenario JSON**. |
| **G11–G14** | `F660–664`, `F611`, `F73`, `F405`/`F505`, `F384`, `F5`/`F37`, clear `F73`, `F406`/`F456`/`F506`, `F407` | [Battle04→05](battle04-outcomes-through-battle05.md) · [Battle05→06](battle05-outcomes-through-battle06.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) · [Galam audit](../../../../../remake/docs/application/pre-voyage-optional-locations.md#galam-optional-exploration) | Ordinary play covers prison/Galam/Battle06 ([Galam route](../../../../../remake/docs/application/pre-voyage-campaign.md#galam-escape-through-battle06-first-control)). Named controlled start: **`slade-join`** ([`slade-join.json`](../../../../../remake/tools/showcase_starts/slade-join.json)) uses Debug Auto Victory (no combat rewards; verifies the Battle05 outcome → jewel → Slade join handoff only, **not** natural combat). Post-Battle05 saved visit: **`map16`** (`galam-learned-magic/slot2.json`). |
| **G15–G17** | `F506`, `F6`/`F38`, `F620–621`, `F83`, `F407`/`F457`/`F507`, `F385`, `F74`, `F640` | [Battle06→07](battle06-outcomes-through-battle07.md) · [Battle07 aftermath](battle07-outcomes-and-aftermath.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#occupied-granseal-and-the-voyage) | Ordinary remake play covers Kiwi refusal/bypass/revisit/join, royal following, and Battle07 return ([Battle06→tower](../../../../../remake/docs/application/pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration)); proposed Debug stop: **Tower return** ([debug starts](../../../../../remake/docs/application/pre-voyage-campaign.md#proposed-non-battle-debug-starts)). |
| **G18–G20** | `F622–624`, `F640`, `F74`, `F507`, clear `F74`/`F640`, onward `F700` | [Post-tower→quake](post-tower-return-through-earthquake-entry.md) · [Quake→docks](earthquake-town-through-docks-departure.md) · [Grans flags](../../../../../remake/docs/content/campaign-flags-grans.md#occupied-granseal-and-the-voyage) · [Debug port](../../../../../remake/docs/verification/debug-port.md#preparing-a-progression-start) | Ordinary carried route covers warnings, first/repeat quake, Captain No/cancel/Yes, and Map14 handoff ([tower→ship](../../../../../remake/docs/application/pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control)). Named controlled start: **`castle-quake`** ([`castle-quake.json`](../../../../../remake/tools/showcase_starts/castle-quake.json), verifies falling guards & Map4 handoff only); independent field record: **Boarding** (`post-battle07-return/slot1.json`). *Export-specific natural `Map44` royal portraits remain **Unknown** (`#2330`).* |

## Grans characters quick reference (joined vs. following vs. NPC)

*Full-game recruitment (Parmecia onward) and later vehicle chains belong to later slices of `#2366`.
Detailed remake promotion and combat records live in
[campaign-characters.md](../../../../../remake/docs/content/campaign-characters.md#grans-and-the-voyage).*

| Character | Role in Grans | Temporary following (non-combat) | Formal join (force membership) | Optional / boundary notes |
| --- | --- | --- | --- | --- |
| **Bowie** (0) | Starting leader | — | New Game (`F0`/`F32`) | Always in active party; 0 HP forces battle defeat. |
| **Sarah** (1), **Chester** (2) | Opening companions | `F66` (from `G2` Yes until `G5` Jaha clears `F66`) | `G2` messenger Yes (`F1–2`/`F33–34`) | Declining `G2` skips join; retryable. |
| **Jaha** (3) | Granseal recruit | Local follow behind Chester on join | `G5` west-exit zone (`F3`/`F35`) | Mandatory exit trigger after commission (`F609`). |
| **Kazin** (4) | Yeel guide → recruit | **`F72`** in Yeel (`G7`, optional — not a combatant) | **`G9`** Hawel's house (`F4`/`F36`, clears `F72`) | Joining in `G9` works even if Yeel (`G7`) was skipped. |
| **Slade** (5) | Galam guide → recruit | **`F73`** in prison tunnel (`G11`, not joined yet) | **`G13`** `Battle05` first victory (`F5`/`F37`, clears `F73`) | Losing or retreating from `Battle05` leaves him unjoined. |
| **Kiwi** (6) | Optional Granseal recruit | Current follow after naming (`G15`) | **`G15`** invaded Granseal (`F6`/`F38`) after name confirm | **Optional:** declining/bypassing does not block the castle; revisitable until `G20`. |
| **Astral** | Adviser / escort | **`F74`** royal train (`G17` victory → cleared at `G20`) | Never joins combat roster | Opening castle offer (`G3`) can be declined and retried; later Parmecia adviser role uses a separate writer (`F70`). |
| **King Granseal & Minister** | Royal escorts | **`F83`** (`G16` → cleared at `Battle07` intro), then **`F74`** (`G17` → cleared at `G20`) | Never join combat roster | Scene followers are independent of the 12-member active combat limit. |
| **Lemon** | Staged story NPC | None in Grans | None in Grans (late ally 28 belongs to Phase C) | Leads `Battle04` capture (`G10`), departs castle (`G16`), and appears wounded outside tower (`G17`); none of these joins him. |

## Grans key items & notable pickups quick reference

*Grans has **no mandatory inventory key-item gate** (unlike Parmecia's Wooden Panel, Achilles
Sword, Cannon, and Dynamite). Story jewels are flags, while physical pickups use ordinary field
`Search` (`campaign-items.md`, `pre-voyage-optional-locations.md`).*

| Item / state | Category | Where & flag | Timing / availability boundary |
| --- | --- | --- | --- |
| **Jewel of Evil / Jewel of Light** (`F384` / `F385`) | **Story state (NOT an inventory item)** | `Battle05` first victory (`F384`) and `Battle07` first victory (`F385`) | Drives Bowie's member-screen jewel display (`F385` display requires `F384` also set); occupies zero pack slots. |
| **Power Water** | Optional permanent growth pickup | Southern Jewel Cave lower chamber (`Map42`, `F133`) | Reachable via `Map66` `(33,43)` → `Map41` door → `Map42` stairs after `G5` with no story-flag gate; closes once committed departure (`G20`) leaves Grans. (The separate central Devil's Head entrance opens only after `Battle08`.) |
| **Quick Chicken** | Optional permanent growth pickup | Granseal Docks deck barrel (`Map44` `(27,6)`, `F139`) | Reachable once `F609` opens the docks (`G5`) and still reachable on `F507` docks (`G19`) before `G20`; `Map14` copies carry no active marker. |
| **Chirrup Sandals** | Optional ring-slot novelty gear | Galam Prison (`Map17` `(56,4)`, `F205`) | Collectible during prison escape (`G11`) or later Galam revisits before `G20`; plays alternate footstep sound when equipped in ring slot. |
| **Mithril** (4 early pieces) | Optional future smithing material | Tower exterior (`Map40` `F132`), Galam Prison (`Map17` `F206`), Grans field (`Map66` `F213`), Quake Granseal (`Map4` `F217`) | No Equip/Use or early smithing on Grans (forging is late Dwarven Village); `F217` survives the `Map4` chasm block copies (`F624`). |
| **Routine supplies & gated/late rows** | Optional pickups vs. excluded late rows | Granseal (`F128`), Castle (`F129–131`), Yeel (`F134`), Hawel (`F135–136`), Galam (`F207–208`), Docks hold (`F140`) | `F140` (below-deck `Medical Herb`) is open on `F609`/`F506` docks but blocked by fixed passengers at `F507` (`G19`). Granseal `F220` is an obstructed empty chest; Galam treasury `F137–138` stays blocked by its guard; Yeel underground `F196–197` (`F940`) and Galam north `F200` (`F999`) belong to the late Grans return. |

## Onward reading & remaining scope

- **Immediate continuation (outside Phase A):** [Voyage and landing through damaged-ship control](voyage-through-damaged-ship-control.md) → [Damaged-ship plank choices through Battle08](../eastern-parmecia/damaged-ship-plank-through-battle08.md).
- **Chapter hub & detailed segment guides:** [Story Progression](../../themes/story-progression.md#grans-opening-and-departure) lists all 11 Grans segment guides in order alongside the remaining five campaign groups (`#2366` Phases B–C).
- **Developer indexes (English technical owners):** [Grans campaign flags](../../../../../remake/docs/content/campaign-flags-grans.md) · [Campaign characters](../../../../../remake/docs/content/campaign-characters.md) · [Campaign items](../../../../../remake/docs/content/campaign-items.md) · [Campaign locations](../../../../../remake/docs/content/campaign-locations.md) · [Debug port](../../../../../remake/docs/verification/debug-port.md#preparing-a-progression-start).
