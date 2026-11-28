# Southern Travel, Caravan, and Creed — Chapter Overview

- **Story span:** Picking up directly from Polca's parked raft and first water move
  ([Eastern Parmecia Chapter Overview](../eastern-parmecia/chapter-overview.md)), this chapter
  covers the southern river voyage and **Kraken** (`Battle16`), **Hassan**, the **Southeast Desert**
  (`Battle14`) and **East Shrine / Ancient Hall** (`Battle15`) puzzle for the **Wooden Panel**,
  **Ribble's** hollow tree and the **Achilles Sword**, **Rohde**, **Taros Shrine** (`Battle18`),
  acquiring and driving the **Caravan**, **Elven Village** (`Battle19`), rescuing **Elric** at
  **Harpy Pond** (`Battle20`), **Fairy Cave** and **Devil's Tail** (`Battle21`), **Creed's Mansion**,
  **Desktop Kingdom / Chessboard** (`Battle22`), **Floor World / Willard Burrow** (`Battle23`),
  body restoration, the **Evil Spirit consultation**, Creed's four-way recruitment and
  **Oddler / Fairy** handoff, and ends at the **Dwarven Mine** after Fairy recovery and receiving
  (or deferring on a full pack) the **Cannon**.
- **Chapter arc & world changes:** Sailing south defeats the Kraken to reach Hassan → recovering
  the **Wooden Panel** and **Achilles Sword** wins Rohde's trust to face **Taros** and earn the
  **Caravan** → crossing the dry riverbed opens Southwest Parmecia, where rescuing **Elric** opens
  Fairy Cave toward **Creed's Mansion** → trapped and shrunk into **Desktop Kingdom** and **Floor
  World**, the force defeats the chess army and Willard, earns restoration, learns **Zeon's**
  demand at the Evil Spirit Shrine, recruits one of Creed's four companions as Oddler stays behind,
  and escorts the **Fairy** to heal the dwarves and claim the **Cannon**.
- **What continues later (outside B2):** Returning north to New Granseal for **Astral**, the
  inventor's **Dynamite**, **Janet**, blasting **North Cliff** (recruiting **Rohde**), and northern
  Parmecia (`Battle24`–`26`+) belong to `#2366` Phase B3 (see
  [Story Progression · Northern travel](../../themes/story-progression.md#northern-travel-pacalon-tristan-and-mitula)).

## Dependency-ordered story events & branches (player view)

*Battle numbers (`Battle14`–`Battle23`) are **not** a single fixed itinerary:*
- On the normal raft journey south, **Kraken (`Battle16`)** comes **before** the Southeast Desert
  (`Battle14`) and East Shrine Hall (`Battle15`).
- After acquiring the Caravan, **Elven Village** can be visited **without** winning `Battle19`
  first.
- Optional stops and recruitments (**Rick**, **Elven Village** revisit, **Fairy Cave** chests, and
  later post-Frayja Creed offers) each obey their own map and story rules rather than one shared
  cutoff.

### Phase 1 · River Kraken, Hassan, East Shrine, Wooden Panel, and Achilles Sword

| ID | Place & player goal | Trigger / choice | What happens & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **S1** | **South Parmecia river (`Battle16`) & Hassan landing** · Sail south and reach Hassan | Sail south from Polca's raft berth into the river encounter (`Battle16`), then disembark at Hassan. | Defeating the Kraken **Head** ends `Battle16` even while limbs remain alive. **Defeat or `EGRESS`** returns specifically to **Polca** and resets the raft berth outside Polca. **First victory** leaves you on the raft to land beside **Hassan** (the former encounter tile becomes navigable). | Mandatory |
| **S2** | **Southeast Desert (`Battle14`) & East Shrine** · Cross the desert and recruit the grandfather as an escort | Sail east across the narrow strait from Hassan, cross the marked desert (`Battle14`), enter **East Shrine**, and speak to the grandfather twice. | `Battle14` requires clearing all enemies (defeat or `EGRESS` returns to your active return town and resets the raft; later crossings can roll random repeat encounters). Inside **East Shrine**, speaking twice has the grandfather follow as a **non-combat escort** (no decline branch). *(Note: B1's early East Shrine check used a controlled opposite-bank start; earliest original pre-raft access on foot remains **Unknown**, whereas the connected remake route reaches East Shrine here via Hassan and the desert.)* | Mandatory for Wooden Panel chain |
| **S3** | **Ancient Hall (`Battle15`) & Wooden Panel** · Open the shrine gate and claim the Wooden Panel | With the grandfather following, sail north to the shrine exterior so he opens the gate, clear `Battle15`, and search the northern Hall chest. | Defeating the enemy leader (Arrow Launcher) clears `Battle15`. **First victory** ends the grandfather's escort and returns you inside the Hall; searching the north chest grants the **Wooden Panel** (full party leaves it retryable). *(Optional trapped chest inside `Battle15` spawns a Golem carrying `Power Water`—admitting that trap from the battle menu is an intentional remake fix of an original US indexing bug.)* | Mandatory for Achilles Sword |
| **S4** | **Ribble hollow tree & Achilles Sword** · Trade the Wooden Panel for the Achilles Sword | Sail to Ribble, stand directly in front of the hollow tree facing up, **Use** the **Wooden Panel**, enter the Ribble-side Hall chamber, and search the chest. | Using the Panel on the wrong tile or facing costs nothing; valid Use opens the tree and **consumes one Wooden Panel** from joined inventories. The separate Ribble-side Hall chest grants the **Achilles Sword** unequipped (full party can make space and retry; no direct tunnel connects the Panel chest and Sword chest). | Mandatory for Taros |

### Phase 2 · Rohde, Taros Shrine, Caravan, and Optional Rick

| ID | Place & player goal | Trigger / choice | What happens & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **S5** | **Hassan & Rohde's proposal** · Show the Achilles Sword and gain Rohde as an escort | Speak to **Rohde** in Hassan (or **Use** the Achilles Sword beside him before the first proposal completes); choose **Yes** or **No**. | His first talk checks all joined packs (including reserves) without consuming or equipping the Sword: without it, he dismisses you; with it, he proposes seeking the **Caravan**. **No** lets you reconsider later on a shorter prompt (which no longer re-checks Sword custody); **Yes** adds **Rohde** as a **temporary non-combat escort** and unlocks Taros Shrine (**Rohde does NOT join the combat force or grant the Caravan here**). | Mandatory (No retryable) |
| **S6** | **Western road (`Battle17`) & Taros Shrine (`Battle18`)** · Defeat Taros and earn the Caravan | Leave Hassan west (`Battle17`), then enter **Taros Shrine** (`Battle18`) after agreeing with Rohde. | `Battle17` clears all enemies (defeat or `EGRESS` returns to Hassan; later walks can trigger level-scaled repeats). Entering Taros Shrine without Rohde's agreement turns you back. **Crucial `Battle18` rule:** **Bowie must equip the Achilles Sword** to damage **Taros** (merely carrying it bare or handing it to another ally fails the damage check; Taros casts `BOLT 2` at 0 MP from his held `Taros Sword`). **First victory** ends Rohde's escort, keeps the Achilles Sword, **grants Caravan ownership** (still on foot inside the shrine), and updates Hassan's town cast. | Mandatory |
| **S7** | **Caravan travel & optional Rick in Hassan** · Drive across dry riverbeds, use Caravan services, and optionally recruit Rick | Step onto a dry riverbed on the overworld to board the Caravan (steer onto land to disembark); interact with the Caravan for services; optionally revisit post-Taros **Hassan** to talk to **Rick**. | Driving the Caravan preserves your walking step counter and keeps the raft's shore berth separate. Caravan services open **Join/Purge** (active 12-member formation), 64-slot **Depot** storage, and supported **Item** actions. In post-Taros Hassan, speaking to **Rick** recruits him immediately (level 19 Knight with an equipped `Steel Lance`; enters **reserve** if 12 allies are active, deployable via Caravan Join/Relief; follows locally during that visit only). | Caravan travel mandatory; **Rick** optional |

### Phase 3 · Southwest Parmecia, Elven Village, Elric, Fairy Cave, and Devil's Tail

| ID | Place & player goal | Trigger / choice | What happens & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **S8** | **Southwest Parmecia fork: Elven Village & `Battle19`** · Visit the village and/or fight on the plains | Drive across the dry riverbed into Southwest Parmecia; head left into **Elven Village** or north/west into `Battle19`. | **Independent forks:** entering **Elven Village** does **not** require winning `Battle19` first (you can use its Church and collect upstairs pickups including `Secret Book`, `Vigor Ball`, `Fairy Powder`, and `Mithril`; later, after taking the Fairy at Creed, Elven Village updates to a four-resident post-Creed cast). In `Battle19`, defeating the leader Orc while Bowie lives clears the field (a qualifying kill on the High Priest drops a one-time `Shining Ball`); later crossings can trigger level-scaled repeats. | Both branches independent at entry (`Battle19` optional before village/Harpy Pond) |
| **S9** | **Harpy Pond (`Battle20`) & Elric rescue** · Free Elric from the harpies | Travel west into Devil's Tail and enter **Harpy Pond** (`Battle20`; stepping south before clearing shows an exit warning). | **Elric** starts on the battlefield as a captive neutral sprite (not yet in the force). Defeat all 12 enemies (defeat or `EGRESS` returns to your return town and resets the raft). **First victory** rescues and **formally recruits Elric** (carrying an equipped `Steel Arrow`, active if under 12 members, else reserve). **Cross-chapter cutoff:** recruiting Elric permanently closes the optional **Bedoe hatchling rescue** back in Eastern Parmecia. | Mandatory |
| **S10** | **Elric's rock passage, Fairy Cave & `Battle21`** · Open the cave and reach Creed's Mansion | Leave Harpy Pond south and approach the blocked rock; cross **Fairy Cave** (4 optional chests) and exit west for `Battle21`. | **Passage rule:** opening the rock requires **Elric to be joined AND alive** (active or reserve both work; `Battle20` victory alone is not the gate, and a fallen Elric cannot open it). Inside **Fairy Cave**, four optional chests hold `Angel Wing`, `Brave Apple`, `Medical Herb`, and `Mithril`. Exiting west triggers `Battle21` on Devil's Tail (leader defeat clears the field; Dark Madam can drop a `Protect Ring` on a qualifying kill before leader sweep), opening the path to **Creed's Mansion**. | Mandatory (cave chests optional) |

### Phase 4 · Creed's Mansion, Desktop Kingdom, Floor World, Consultation, Recruitment, and Dwarven Mine

| ID | Place & player goal | Trigger / choice | What happens & where to go next | Requirement |
| --- | --- | --- | --- | --- |
| **S11** | **Creed's Mansion, Goliath & the magic door** · Enter the mansion and shrink to Desktop Kingdom | Enter Creed's Mansion (optional `Healing Rain` chest in the southwest before Goliath), meet **Goliath**, then step into the **magic door**. | Stepping through the magic door shrinks Bowie, Peter, and Oddler and sends the party to **Desktop Kingdom** (not a permanent size penalty or force change; once Creed restores you later, the magic door stops shrinking you). | Mandatory (`Healing Rain` optional) |
| **S12** | **Desktop Kingdom & Chessboard (`Battle22`)** · Earn King Ponpei's permission and defeat the chess army | Explore Desktop Kingdom (Church, Caravan NPC, **Eric** and **Karna** as residents—not recruitable yet), talk to **King Ponpei**, and cross onto the **Chessboard** (`Battle22`). | Defeating the enemy King clears all 16 pieces (defeat or `EGRESS` returns to Desktop Kingdom). **First victory** returns to Desktop Kingdom where the King announces a reward—**the announcement alone does NOT grant the Cotton Balloon**. | Mandatory |
| **S13** | **Cotton Balloon, Floor World & Willard Burrow (`Battle23`)** · Descend to the floor and defeat Willard | Search the newly exposed Desktop chest for the **Cotton Balloon**, pass the exit guard, **Use** the Balloon on the marked ledge tile, cross **Floor World** (optional giant-ring scene; Church & Caravan NPC; **Randolf** and **Tyrin** as residents), and clear **Willard Burrow** (`Battle23`). | Full party rolls back the Balloon chest cleanly for retry; the exit guard checks chest claim rather than current pack custody, while **Use** requires a held Balloon and **consumes it** (any facing on the marked tile). Landing sets Floor World as your return anchor. Inside **Willard Burrow**, defeating **Willard** clears the rats and opens the exit back into Creed's Mansion. | Mandatory (giant ring optional) |
| **S14** | **Body restoration & Evil Spirit consultation** · Regain normal size and hear Zeon's demand | Re-enter Creed's Mansion after defeating Willard, then follow Creed and the Fairy down the basement stairs to the **Evil Spirit Shrine**. | Creed restores the party to normal size (and sets **Hassan** as your return anchor), learns of **Zeon's** return, and descends with the Fairy. In the basement shrine, the Evil Spirit inspects the jewel and **Zeon's** voice demands the **Jewel of Evil** in exchange for Elis before the shrine erupts (updating the mansion to its post-consultation hall and priming New Granseal's later Astral return in Phase B3). | Mandatory |
| **S15** | **Creed's four-way recruitment, Fairy & Oddler's farewell** · Take the Fairy, choose one companion, and leave Oddler behind | Back in Creed's Mansion: talk to the resident **Fairy**, speak with **Eric / Karna / Randolf / Tyrin** (**Yes** vs. **No**), and walk south through the hall. | **Fairy:** agrees to follow toward the Dwarven Mine (and updates Elven Village's cast), sealing the basement shrine warp behind you. **Four offers:** **No** leaves everyone available; **Yes** recruits **only that one ally** (in **reserve** if 12 allies are active—deploy via Caravan Join/Relief!), shrinks the other three, and closes the remaining initial offers *(much later, after recruiting **Frayja**, returning to Creed allows recruiting the remaining candidates without shrinking)*. **Physical exit rule:** although the southern Oddler zone has **no recruitment-flag gate**, in the settled post-consultation hall **Goliath and the four living unjoined candidates physically occupy all five crossings** across the passage, so you cannot walk past them without choosing a companion. Crossing south stages **Oddler** staying at Creed's Mansion while the **Fairy** becomes your travelling follower. | Mandatory (choose 1 of 4 here to clear the hall) |
| **S16** | **Dwarven Mine: Fairy recovery & Cannon gift** *(B2 endpoint)* | Return across Southwest Parmecia (completing any skipped `Battle19` if encountered) and enter the **Dwarven Mine** with the Fairy following. | The **Fairy** heals the sick dwarves and her travelling follow ends immediately—**even if Bowie's pack is full**. The recovered dwarf then grants the **Cannon** strictly into **Bowie's own 4-slot pack** (no party-wide fallback): if Bowie has space, he receives the Cannon and digging finishes; **if Bowie's pack is full**, recovery still completes and the Fairy still leaves, and you must free a slot on Bowie (Give/Drop) and **talk to the dwarf again in the live room** to receive the Cannon. An optional `Dry Stone` pickup also lies in the mine (for late Grans). Leaving with the Cannon ends Phase B2; New Granseal, **Astral**, **Dynamite**, and **North Cliff** belong to Phase B3. | Mandatory chapter finale (`Dry Stone` optional) |

## Southern Travel & Creed characters quick reference

*Only characters whose membership, control, or following changes in this chapter are listed here.
Earlier Grans and Eastern Parmecia allies carry forward; full promotion and combat records live in
[campaign-characters.md](../../../../../remake/docs/content/campaign-characters.md#southern-travel).*

| Character | Role in this chapter | Temporary following (non-combat) | Formal combat membership | Optional / boundary notes |
| --- | --- | --- | --- | --- |
| **East Shrine grandfather** | Shrine gate escort | Follows from East Shrine (`S2`) until `Battle15` first victory (`S3`) | Never joins combat force | Required to open the Ancient Hall gate for the Wooden Panel. |
| **Rohde** | Caravan custodian | Follows from Hassan **Yes** (`S5`) until `Battle18` first victory (`S6`) | **Does NOT join combat force in B2** (joins later at North Cliff in Phase B3) | Requires Achilles Sword in joined packs for first proposal; No is retryable without re-checking custody. |
| **Rick** | Optional Hassan knight | Local follow during his recruitment visit (`S7`) | Joins upon Talk in post-Taros Hassan (level 19 Knight, equipped `Steel Lance`; reserve if 12 active) | **Optional:** never required for main progression; available once `Battle18` updates Hassan's cast. |
| **Elric** | Harpy Pond archer | Local follow in rescue scene (`S9`) | Formally joins on `Battle20` first victory (`S9`, equipped `Steel Arrow`; reserve if 12 active) | **Must be joined AND alive** (active or reserve) to open Fairy Cave (`S10`); joining him closes Bedoe's hatchling rescue. |
| **Eric, Karna, Randolf, Tyrin** | Creed's four candidates | Chosen ally follows locally in the room (`S15`) | Choosing **Yes** on one joins only that candidate (`S15`, reserve if 12 active) | In the settled post-consultation room, Goliath + all 4 unjoined candidates physically block all 5 hall crossings. Remaining candidates can be recruited much later after **Frayja** joins (outside B2). |
| **Oddler** | Travelling companion from Polca | Carried from Eastern Parmecia until **staying behind at Creed's Mansion (`S15`)** | Never joins combat force | Stays as a Creed resident on subsequent visits. |
| **Fairy** | Creed healer | Follows locally in mansion (`S15`), becomes travelling follower on south exit (`S15`), **ends at Dwarven Mine recovery (`S16`)** | Never joins combat force | Leaves at mine recovery **even if Bowie's pack is full** and refuses the initial Cannon grant. |

## Key items, pickups & the Caravan / story-item chain

*Detailed custody, drop, and equipment rules live in
[campaign-items.md](../../../../../remake/docs/content/campaign-items.md).*

### B2 story-item & vehicle chain (each link is a separate state)

1. **Wooden Panel (`S3` → `S4`):** Claimed from the shrine-side Ancient Hall chest (`S3`), then
   **Used** at Ribble's hollow tree facing up (`S4`, **consumed** on valid use; wrong tile/facing
   costs nothing).
2. **Achilles Sword (`S4` → `S5` → `S6`):** Claimed unequipped from the Ribble-side Ancient Hall
   chest (`S4`); **held** in any joined pack to open Rohde's first Hassan proposal (`S5`, not
   consumed); **equipped on Bowie** to damage Taros in `Battle18` (`S6`, kept after victory).
3. **Caravan (`S6` → `S7`):** **Ownership** is granted on `Battle18` first victory (`S6`, still on
   foot inside Taros Shrine); **boarding** happens by stepping onto a dry riverbed on the overworld
   (`S7`, distinct from the raft berth); **services** (Join/Purge, Depot, Item) are entered by
   interacting with the Caravan on the overworld or the Caravan NPCs in Desktop Kingdom / Floor
   World.
4. **Cotton Balloon (`S12` → `S13`):** Announced after `Battle22` (`S12`, no item yet), **claimed**
   by searching the exposed Desktop chest (`S13`), and **Used** on the marked ledge tile (**consumed**
   before descent to Floor World).
5. **Cannon (`S16` → Phase B3):** Offered strictly into **Bowie's own 4-slot pack** after Fairy
   recovery in the Dwarven Mine (`S16`); if Bowie's pack is full, free a slot and speak to the dwarf
   again in the live room. Combined later with **Dynamite** at North Cliff (Phase B3).

### Notable chapter items & verified effects

| Item | Category & verified effect | Where obtained | Timing / availability boundary |
| --- | --- | --- | --- |
| **Power Water** | Permanent ATT growth item (Field/Caravan Use) | Optional `Battle15` trapped-chest Golem drop (`S3`) | Admitting the trap from the battle menu is an intentional remake fix of an original US indexing bug; requires a qualifying kill on the spawned Golem before leader sweep. |
| **Taros Sword** | Boss weapon (`BOLT 2` at 0 MP for Taros); **cannot be equipped or Used by allies** (`NONE` class mask) | Rare `Battle18` Taros drop (`S6`) | Bare carry only if dropped (natural player drop not observed on the main remake run). |
| **Running Pimento** & **Mithril (×4)** | Permanent MOV growth (`Running Pimento`); late smithing material (`Mithril`) | Hassan (`Running Pimento`, 2 `Mithril`), Elven Village (1 `Mithril`), Fairy Cave (1 `Mithril`) | Ordinary field Search; Mithril forging belongs to late Dwarven Village. |
| **Shining Ball** | Consumable battle attack item (Use casts `FBALL`, 0 MP; no Equip effect) | `Battle19` High Priest qualifying drop (`S8`) | Must kill the High Priest before leader sweep; full pack routes missed rare drop to Deals. |
| **Secret Book** & **Vigor Ball** | Special promotion items (`MAGE → SORC`; `PRST → MMNK`); no direct Equip/Use | Elven Village (`S8`) | Consumed only upon confirmed special Church promotion (held in any joined pack). |
| **Fairy Powder**, **Healing Water**, **Brave Apple**, **Healing Rain** | Recovery / growth supplies (`Fairy Powder` cures poison+paralysis in Field/Caravan, poison only in Battle; `Healing Water` & `Healing Rain` are Battle-only recovery; `Brave Apple` grants +1 level in Field/Caravan) | Elven Village (`Fairy Powder`), Taros Shrine (`Healing Water`), Fairy Cave (`Brave Apple`, `Angel Wing`, `Medical Herb`), Creed Mansion (`Healing Rain`) | Ordinary field Search; Creed Mansion's southwest `Healing Rain` chest is reachable on the first visit before Goliath (`S11`). |
| **Protect Ring** | Ring (Equip DEF +5; Battle Use casts `BOOST 1`, 0 MP, breakable) | `Battle21` Dark Madam qualifying drop (`S10`) | Requires qualifying kill before leader sweep; full pack routes to Deals. |
| **Dry Stone** | Later map-event story item (no Equip or battle Use in B2) | Dwarven Mine loose pickup (`S16`) | Collectible during `S16`; opens the late Grans / Dwarven Village crossing (outside B2). |

## Technical & scenario trace (lookup by Event ID)

*Developers and researchers can trace each player event above to exact maps, story flags (`Fnnn`),
authoritative owners, and registered remake checkpoints here without duplicating a second flag
registry.*

| Events | Maps & key flags / technical notes (lookup only) | Authoritative design / flag / location owners | Remake observation & acceptance boundary |
| --- | --- | --- | --- |
| **S1–S4** | `Map69`, `Map49`, `Map9`, `Map70`, `Map62`, `Map61`, `Map50`, `Map8` · `F416`/`F466`/`F516` (`Battle16` Head defeat clears limbs; loss/EGRESS returns to Polca `Map13` + resets raft `Map72 (43,48)`), `F414`/`F464`/`F514` (`Map70` desert; `F410` precedes `F414`), `F728`/`F77`/`F729` (East Shrine old man), `F730`/`F415`/`F515`/`F732` (`Map61`→`Map50` `Battle15`; trap chest `F151` + Golem `143` `Power Water` drop bit 29), `F152` (`Wooden Panel` item 112), `F731` (Ribble tree Use at `(15,19)`/`(16,19)` Up consumes 1 Panel), `F147` (`Achilles Sword` item 61). | [Raft→South](polca-raft-boarding-through-south-parmecia-entry.md) · [Kraken entry](south-parmecia-through-kraken-battle16-entry.md) · [Battle16→Hassan](battle16-outcomes-through-hassan-entry.md) · [Ribble tree/Sword](ribble-tree-opening-and-achilles-sword.md) · [Parmecia flags](../../../../../remake/docs/content/campaign-flags-parmecia.md#hassan-the-shrines-and-taros) | Ordinary bilingual remake play covers Kraken win/EGRESS/defeat & Hassan save ([`kraken-hassan.md`](../../../../../remake/docs/application/kraken-hassan.md)), desert win/loss/EGRESS/repeat ([`desert-battle.md`](../../../../../remake/docs/application/desert-battle.md)), post-desert East Shrine, `Battle15` + trap chest + Panel ([`east-shrine-panel.md`](../../../../../remake/docs/application/east-shrine-panel.md)), and Ribble tree → Sword ([`ribble-sword-rohde.md`](../../../../../remake/docs/application/ribble-sword-rohde.md)). |
| **S5–S7** | `Map9`, `Map69`, `Map12`, `Map68` · `F726` (ancestry), `F798` (no-Sword dismissal), `F727` (first proposal done), `F725` + `F84` (Rohde temp follow) + `F418` (`Battle18` unlock; `F11` stays clear), `F417`/`F467`/`F517` (`Map69` west road; `Battle09` precedes `Battle17`), `F468`/`F518` + `F65` (Caravan) + clear `F84` (`Battle18`; `Taros Sword` item 110 drop bit 24 / `NONE` mask), `F12`/`F44`/`F256` (optional Rick in `F518` Hassan), `PLAYER_TYPE=1` (`esc07_controlRaft` / `$3800` riverbed) vs. `PLAYER_TYPE=2` (raft `$3C00`); pickups `F153`/`F154`/`F201`/`F155`. | [Hassan/Rohde](hassan-ancestry-and-rohde-caravan-choice.md) · [Battle17](hassan-departure-and-battle17-outcomes.md) · [Taros entry](southwest-parmecia-through-taros-battle18-entry.md) · [Battle18→Caravan](battle18-outcomes-and-caravan-acquisition.md) · [Rick](hassan-rick-recruitment.md) · [Caravan travel](caravan-boarding-and-southwest-parmecia-travel.md) | Ordinary bilingual remake play covers swordless/No/Yes/Show Rohde ([`ribble-sword-rohde.md`](../../../../../remake/docs/application/ribble-sword-rohde.md)), `Battle17` + level-bounded upgrade ([`hassan-taros-road.md`](../../../../../remake/docs/application/hassan-taros-road.md)), Taros ineffective vs. equipped Sword, win/loss/EGRESS ([`taros-caravan.md`](../../../../../remake/docs/application/taros-caravan.md)), Caravan riverbed driving/landing ([`caravan-travel.md`](../../../../../remake/docs/application/caravan-travel.md)), Caravan services ([`caravan-services-design.md`](../../../../../remake/docs/application/caravan-services-design.md)), and Rick recruitment/Relief ([`hassan-rick-recruitment.md`](../../../../../remake/docs/application/hassan-rick-recruitment.md)). |
| **S8–S10** | `Map68`, `Map23`, `Map67`, `Map51`, `Map32` · `F419`/`F469`/`F519` (`Battle19` Orc leader 128; High Priest 129 `Shining Ball` bit 6; `Map23` entry independent of `F519`; `Map23` pickups `F157`/`F158`/`F202`/`F221`; later `F762` cast), `F420`/`F470`/`F520` + `F13`/`F45` (Elric rescue & join; `F13` also closes Bedoe hatchling `F724`), `F751` (Map67 rock requires `F13` + nonzero Elric HP, independent of `F45`), `Map32` chests `F160–163`, `F421`/`F471`/`F521` (`Battle21` Dark Madam 129 `Protect Ring` bit 8). | [Battle19→Village](battle19-outcomes-through-elven-village-entry.md) · [Elven Village](elven-village-conversations-and-local-exploration.md) · [Harpy Pond](harpy-pond-approach-through-battle20-entry.md) · [Battle20→Elric](battle20-outcomes-and-elric-recruitment.md) · [Fairy Cave](fairy-cave-exploration-and-devils-tail-return.md) · [Battle21](battle21-outcomes-and-devils-tail-return.md) · [Creed flags](../../../../../remake/docs/content/campaign-flags-creed.md#elven-village-and-harpy-pond) | Ordinary bilingual remake play covers independent Elven Village fork, `Battle19` win/repeat & post-Creed `F762` village ([`battle19-village.md`](../../../../../remake/docs/application/battle19-village.md), [`battle19-repeat.md`](../../../../../remake/docs/application/battle19-repeat.md)), `Battle20` & Elric ([`battle20-elric.md`](../../../../../remake/docs/application/battle20-elric.md)), Elric passage & Fairy Cave ([`fairy-cave.md`](../../../../../remake/docs/application/fairy-cave.md)), and `Battle21` ([`battle21-devils-tail.md`](../../../../../remake/docs/application/battle21-devils-tail.md)). |
| **S11–S16** | `Map33`, `Map22`, `Map26`, `Map27`, `Map34`, `Map30` · `F774` (Goliath; `Healing Rain` `F164`), `F783` (magic-door shrink guard + restoration + `EGRESS_MAP=9`), `F776`/`F777`/`F422`/`F472`/`F522`/`F778` (Desktop/Chessboard), `F212` + visit `F258` (`Cotton Balloon` item 125; Use at `(35,24)` any facing), `F779`/`EGRESS_MAP=26`/`F780` (Floor World), `F781`/`F423`/`F473`/`F523` (Willard Burrow), `F784`/`F802` (Evil Spirit consultation; `F802` feeds B3 New Granseal), `F762`/`F785` (Fairy talk + shrine-warp overwrite), `F14–17`/`F46–49`/`F787`/`F89` (4 initial offers; late `F22` override), `F786`/`F69`/clear `F68` (Oddler stays; **settled F784 room has 5 blocking hall actors**), `Map30` `F760`/clear `F69` (Fairy recovery) + `F761` (`Cannon` item 114 strictly to Bowie slot0–3) + `F156` (`Dry Stone` item 115). | [Creed entry](creed-mansion-entry-and-first-goliath-scene.md) · [Desktop](desktop-kingdom-interactions-through-battle22-entry.md) · [Balloon→Floor](cotton-balloon-through-floor-world-entry.md) · [Willard](battle23-outcomes-and-burrow-return.md) · [Restoration](creed-restoration-and-basement-departure.md) · [Consultation](evil-spirit-consultation-and-zone-return.md) · [Recruitment](creed-return-fairy-and-recruitment-choices.md) · [Oddler/Fairy](oddler-stays-and-fairy-handoff.md) · [Mine/Cannon](creed-departure-dwarf-recovery-and-cannon-gift.md) · [Creed flags](../../../../../remake/docs/content/campaign-flags-creed.md#desktop-floor-world-and-willard) | Ordinary bilingual remake play covers Mansion, Desktop/`Battle22`, Balloon/Floor World, Willard/`Battle23`, Restoration, Consultation, all 4 initial No/Yes offers + 5-actor hall obstruction proof, Oddler/Fairy handoff, and Dwarven Mine recovery/full-pack retry/Cannon/Dry Stone ([`cotton-balloon-floor-world.md`](../../../../../remake/docs/application/cotton-balloon-floor-world.md), [`creed-restoration.md`](../../../../../remake/docs/application/creed-restoration.md), [`evil-spirit-consultation.md`](../../../../../remake/docs/application/evil-spirit-consultation.md), [`creed-recruitment.md`](../../../../../remake/docs/application/creed-recruitment.md), [`creed-no-recruit-exit.md`](../../../../../remake/docs/application/creed-no-recruit-exit.md), [`dwarven-mine-cannon.md`](../../../../../remake/docs/application/dwarven-mine-cannon.md)). *Late `F22` offers and `F68`-set mine variations have controlled engine checks only; natural original timing/SRAM remain **Unknown**.* |

## Evidence & boundary rules

- **Original:** All Southern Travel, Caravan, and Creed segments (`S1`–`S16`) are **Confirmed
  static** in pinned `SF2DISASM` (`c834c652b6862bc5679fd7f69a38a7093206efc6`) and the USA retail
  ROM ([southern coverage](../../../../research/story-content-coverage.md#southern-parmecia-transport-the-sword-and-taros),
  [Creed coverage](../../../../research/story-content-coverage.md#creeds-small-worlds-and-the-consultation)).
  Original runtime observations stop earlier at `Map3`–`Battle01`; natural original traversal,
  hardware timing, rendered presentation, and original SRAM persistence remain **Unknown**.
- **Remake:** Ordinary continuous bilingual play and cold Save/Continue cover the connected route
  from Polca's raft berth through Kraken, Hassan, desert `Battle14`, East Shrine/`Battle15`/Panel,
  Ribble/Sword/Rohde, `Battle17`–`18`/Caravan/Rick, Southwest Parmecia/Elven Village/`Battle19`,
  Harpy Pond/Elric, Fairy Cave/`Battle21`, Creed/Desktop/Floor World/Willard, restoration,
  consultation, recruitment (with the 5-actor hall obstruction witness), Oddler/Fairy handoff, and
  Dwarven Mine recovery + Cannon custody ([capability status](../../../../../remake/docs/capability-status.md#campaign-feature-trace)).
  Controlled checks (such as late `F22` Creed offers, `F68`-set mine variations, and full-pack
  duplicate-Panel tests) remain distinct from ordinary continuous journeys; Battle15's playable
  trapped-chest entry is an intentional remake correction of an original US battle-menu indexing
  bug.
- **Original Unknown ≠ remake missing:** An **Unknown** in an original segment guide marks the
  limit of original-game runtime evidence, not an unimplemented remake feature, and a passing remake
  observation does not prove original-console chronology or timing.
- **Publication note:** Maintained in the canonical repository first; live Wiki synchronization
  happens separately after acceptance.

## Onward reading & remaining scope

- **Previous chapter:** [New Granseal and Eastern Parmecia — Chapter Overview](../eastern-parmecia/chapter-overview.md) (voyage/landing through Polca raft boarding and first water move).
- **Next continuation (outside B2):** [Story Progression · Northern travel, Pacalon, Tristan and Mitula](../../themes/story-progression.md#northern-travel-pacalon-tristan-and-mitula) (`#2366` Phase B3 — New Granseal return, Astral, Dynamite, Janet, North Cliff, Ketto, and Pacalon).
- **Chapter hub, worked example & 37 segment guides:** [Story Progression · Southern travel, Caravan and Creed](../../themes/story-progression.md#southern-travel-caravan-and-creed) and [Worked Example: Taros and the Caravan](../../themes/story-progression.md#worked-example-taros-and-the-caravan).
- **Developer indexes (English technical owners):** [Parmecia flags](../../../../../remake/docs/content/campaign-flags-parmecia.md#hassan-the-shrines-and-taros) · [Creed flags](../../../../../remake/docs/content/campaign-flags-creed.md) · [Campaign characters](../../../../../remake/docs/content/campaign-characters.md#southern-travel) · [Campaign items](../../../../../remake/docs/content/campaign-items.md) · [Campaign locations](../../../../../remake/docs/content/campaign-locations.md#southern-travel) · [Capability status](../../../../../remake/docs/capability-status.md#campaign-feature-trace).
