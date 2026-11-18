# North Cliff and Rohde

Both ordinary [Battle24 returns](battle24-north-parmecia.md) reach a save immediately outside the opened cave at Map72 (27,34). The English and Chinese parties retain their own progression. Cave entry and Battle25 consume this handoff.

## Original callback

**Confirmed** by static inspection of the USA baseline and pinned SF2DISASM revision. On the specific zone rows at X25–30, Y37, the callback returns for F750 set, F805 clear or temporary F256 set, in that order. It searches for Cannon first and Dynamite second. Only joined packs participate, including reserves and fallen members. Either failed search names Dynamite, leaves custody intact and sets F256 after the missing-item scene. A border scroll retains the latch. Fresh explicit map entry clears temporary flags. [Battle24 outcomes and cave preparation](../../../docs/research/chapters/northern-travel-mitula/battle24-outcomes-through-north-cliff-preparation.md)

**Confirmed** from the same source. With both items present the caller invokes the inventory removal service twice. Each call searches again and compacts the matching pack, first for Cannon, then for Dynamite. The caller continues without testing either return. The scene blasts the cliff, copies the 1×2 entrance, joins Rohde, hides the three local allocations, installs the Caravan-led follower branches and sets the Battle25 request flag F425. After scene return the caller writes F750 and then F256. Fresh layout construction applies the flag copy at (27,33). [Opening and recruitment source, callsites and limits](../../../docs/research/chapters/northern-travel-mitula/north-cliff-opening-and-rohde-recruitment.md)

## Ordinary remake play

**Confirmed** through Godot input, dialogue, state and layout observations. Both inherited parties started at (27,38) with Bowie carrying Cannon 114 and reserve Chester carrying Dynamite 116.

| Journey | Observed result |
| --- | --- |
| English, both items carried | Walking up started the scene. Both items had left the packs by the first dialogue. Native audio reported the battlefield-death cue playing at timer C2. The blast changed the two entrance words from 49316 and 173 to 53251 and 5. Rohde was joined during the closing dialogue with F750 still clear. After control returned, F425, F750 and F256 were set. The player walked to (27,34) and saved with 7,915 gold. |
| Cannon deposited, both languages | The normal Caravan Depot moved Bowie's Cannon into storage. The English and Chinese prompts both named Dynamite. Neither pack nor depot changed during the refusal. Leaving and crossing the row again on this visit produced no second prompt. |
| Chinese retry | A new process continued the missing-item save with F256 intact. Scrolling west to Map73 kept the latch. The player retrieved Cannon from the Caravan, entered New Granseal and walked back out. Fresh entry had cleared F256. Scrolling east and crossing the cliff row ran the blast and the translated BRGN join message. The party reached (27,34) with 8,665 gold. No repeat battle started on this journey. |
| Loaded and fresh terrain | The loaded scene retained hidden physical actors for Rohde, the cannon and the effect. The Caravan followed Bowie, with Peter and Astral following the Caravan. An English round trip through town and back across the completed row left the entrance open and restored the ordinary four-actor field population. |

**Confirmed** for persistence. In each language a new process selected the journey through the title's Continue menu. Saving before moving reproduced every decoded gameplay field, including flags, follower links, layout, party, depot, RNG and music. Both processes moved down and crossed the completed event row without replay. The title checks and a separate English blast ran with the producing checkout's local directory unavailable. Recipe 8 adds the previously unjoined Rohde record using the existing growth and migration rules. His source class is BRGN 22, level 4, with equipped Assault Shell 49. Both ordinary forces were full and he joined the reserve. The carried records of the other allies matched the prior cliff saves apart from the two consumed items. A previous recipe 7 Battle24 movement save also resumed, adding Rohde to the retained roster while preserving the battle state.

## Reproduce

Load the worktree's registered configuration and use the private issue 1905 package. Run `Rebuild.ps1 -Repo <checkout> -Output <fresh-external-directory>` for one selected world, a refreshed Battle24 scene and its adapter. It uses the pinned inputs, predecessor media and local language corpus. The world is about 101 MB and the catalog remains under the 256 KiB reader limit.

Run `Play.ps1 -Language en -Mode cliff -Sample <fresh-name>` from the package for the direct route. For the missing route use `-Mode depot`, followed by `-Mode retry -Seed <missing-route-output>` in Chinese. For title Continue use `-Mode continue -Seed <opened-cliff-slot>` with a fresh sample name. Omitting `-Sample` leaves control with the player. The private handoff locates both immutable endpoints and records the independent replay commands.

With the package inputs and `SF2_NORTH_CLIFF_SAVE` set to an ordinary approach slot, run the engine test project with filter `FullyQualifiedName~NorthCliffTests`. It drives the compiled callback with changed custody, including two items in one pack, duplicate items, a dead reserve, unjoined and depot copies, and each missing-item branch. It also exercises an explicit Map72 rebuild with each completion state. The existing Ribble inventory test covers removal with no match.

**Unknown** for this chapter includes Rohde's first actual battle action, the cave interior, Battle25, human presentation acceptance and original hardware timing. Some native processes emitted the existing ObjectDB shutdown warning. [Exit issue #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)
