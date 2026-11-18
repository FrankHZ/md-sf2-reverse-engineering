# Dwarven mine: Fairy recovery and Cannon

## Outcome and agreement

High rigor under [the campaign agreement](remaining-campaign-risk-baseline.md) and
[Issue1891](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1891): two real consumers
(Creed and the mine) retain loaded scene state across flags that select a different fresh population.
This slice continues the unchanged bilingual [Creed mine-approach slots](creed-recruitment.md),
entering Map30 from Map68 (57,25), completing recovery, handling Bowie's full pack, giving Cannon,
digging, returning control, saving/cold Continuing, leaving and reentering. It ends at an ordinary
controllable mine return. Astral, Dynamite, Battle24, North Cliff and later towns are excluded.

The original [mine owner](../../../docs/research/chapters/southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md)
pins SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA ROM.
Remake observations do not prove original natural reach, hardware timing or human presentation quality.

## Implementation boundary

The private selection admits Map30's base, F785 and F761 populations and their shared source callbacks.
The existing compiler lowers the source scenes, item helper and caller independently. A typed pack-space
branch and actor item addition reuse the four-slot inventory rule. They address the named combatant,
without party-wide fallback, membership/HP checks or an implicit one-time reward guard. Full addition
is a no-op; the source caller still owns narration, music, flags and subsequent actions.

`Sf2FieldSceneRestore` extracts the settled scene mechanics used by both actual consumers.
`Sf2CreedRestore` and `Sf2MineRestore` own their separate context/population admission. Existing
`SavedFieldScene` carries loaded population, actors/aliases/current list, following and layout deltas;
no executable continuation or new wire field is saved. The accepted save-format protection writes
populated scenes as field-v3 while continuing to accept prior Creed scene-v1 data.

A current F785 room keeps five dwarves and the digging 2×2 copy after F761. Fresh F761 entry has no
residents and also applies the 4×4 copy. Every Map30 save requires its actual scene; flags alone cannot
recover which room was loaded. In the F68-clear history B31 resolves to physical0 during recovery.
The later Bowie inline action replaces that temporary following. The distinct F68-set B can keep
its source follower relation. Current versus fresh layout/population must be checked separately.

## Acceptance and resources

**Confirmed** in the remake through native Godot state/input/projection/error observations:

| Boundary | Result and limits |
| --- | --- |
| Unchanged ordinary entrances | English and Chinese Creed mine-approach slots enter Map30 (8,20), Up. English Bowie already has three items after its earlier battle; Chinese Bowie has four. Neither seed is edited. |
| Recovery and helper ordering | Both consume1711–1727. English receives Cannon114 and completes1728/1730–1732. Chinese consumes1729, retains all four items and no F761, but the caller still sets F760 and clears F69. Fairy is hidden and Bowie has no final follower relation. |
| Full pack and real retry | Chinese gives Healing Seed from Bowie to Chester through Item/Give, then talks to128. Cannon fills Bowie's vacant slot and digging completes. A separate English branch first gives Sarah's Healing Drop to Bowie, observes the same refusal, then legally moves it to Chester and retries. These are ordinary inventory variations, separate from the unchanged English baseline. |
| Live versus fresh | Gifted live F785 retains five dwarves, hidden Fairy and the2×2 copy. Fresh F761 has only Bowie/Peter and the additional4×4 copy. Three versus eighteen changed cells are this fixture's deltas; unchanged copied cells are omitted. Five full decoded-data cold comparisons cover both live/fresh languages and the full-pack state. |
| Search and services | Dry Stone115 at(8,10), flag156, is acquired through ordinary Search in both languages; full Bowie sends it to Chester. Chinese repeat Search after reentry adds nothing. Map30 has no shop/church binding. |
| Exit and next consumer | Both leave, reenter the empty fresh room without recovery/reward replay, leave again and save under the existing southwest-Parmecia recipe. Final `return` slots are ordinary Map68 (57,25), Down, with Cannon custody and F760/F761, without F69. They are the next slice's inputs; no New Granseal audience is claimed. |
| Previous saves | Old Creed scene-v1 slots Continue and save as field-v3 with complete data equality in both languages. Old Battle23 upgrades recipe5→6 and initializes the accepted extra carried definitions; its subsequent cold save is completely equal. Old Desktop loads/saves normally; elapsed ambient motion changes its RNG seed, so that observation is compatibility, not whole-data equality. |
| Changed conditions | `MineCannonTests` executes F68-clear alias0 and F68-set distinct B follow/action replacement, round-trips actual scene policy, and checks named dead/unjoined actor packs with0/1/3/4 items, full no-op and source-allowed repeated addition despite F761. These are controlled engine checks, not ordinary F68-set mine history. |

Resident128's F760 retry is observed. The producer admits all resident callbacks and the bilingual
catalog includes1705–1710: pre-recovery128, earthquake129, sick130, absent-Fairy131, digging132 and
F761-set131. Those optional lines are not reached in the tested ordinary histories. The Fairy route
crosses the healing zone before reaching the residents; current full-pack128 blocks the corridor
to the eastern group. Native attempts retain their navigation failures. Do not reinterpret admitted
source callbacks as naturally reached dialogue, or add a global F761 guard to suppress them.

No subsequent campaign slice is started by this work. Original natural timing, human visual/audio
acceptance and #1716 remain **Unknown** or outside this observation boundary. Menu Quit completes;
some runs retain the known two ObjectDB leak warnings, so this is not a clean-shutdown claim.

Initial free space: G about32 MiB; H about1.49 GiB. Existing toolchains/caches are reused. Candidate
world/media budget250 MiB; task build/observations/publication increment150 MiB; minimum H headroom
512 MiB. The first full candidate is about235 MiB, world100,634,495 bytes, below128 MiB reader bound.
Preparation is linear in selected content/media with estimated peak below1.5 GiB and no network/model
cost. External output uses an explicit task-owned scratch root; ordinary default stays checkout local/.
Localization remains a small checkout-local result copied into the task publication.

The reached music37/MITHRIL_DIGGERS uses the existing original-driver whole-channel recurrence and
unchanged-PCM workflow. MUSIC_ITEM27 uses source channel exhaustion and an unchanged finite PCM cut.
Controlled original-driver captures restore their core state and verify ROM/installation identity;
they do not establish natural mine timing. Finite effects reuse accepted same-command defaults;
no effect×Timer matrix is collected.
Old media, inputs and failures stay read-only. No complete predecessor bundle or tool installation is
copied. The final budget is750 MiB including a fresh complete rebuild, retained failures and a36 MiB
standalone player runtime, with512 MiB minimum H headroom. The full rebuild reproduces every world
field of the observed candidate; only its regenerated provenance list differs. Published live/fresh
cold observations from a different checkout/cwd retain complete data equality.

The affected74 engine checks and adapter compilation pass. Clean-head planner and actual CI results
are retained with the frozen PR. Shared Python edits prepare remake content;
they do not change research extraction or evidence contracts. Do not rerun the old completed
aggregate merely to replace its retained obsolete-assertion failures. #1716 remains open.

## Reproduction and private handoff

The stable private handoff `issue1891/HANDOFF.md` names source dependencies, exact head, native samples,
all completed failures and rebuild arguments. Its `Prepare.py` invokes maintained producers with an
explicit output root, source/ROM identity checks, selected audio and icon bindings. `Report.py` compares
whole decoded save data. `Play.ps1` uses the bundled runtime, stable shared inputs and separate player
namespaces; it can start at `mine-approach`, `full`, `live`, `searched`, `fresh` or `return` in either language.
No historical task checkout is needed for playback. The unchanged predecessor slots remain separate.

```powershell
# From the stable private handoff; normal interactive player:
./Play.ps1 -Language zh-CN -StartAt return
# Fresh automated native sample; other modes and exact seeds are in HANDOFF.md:
./Play.ps1 -Language en -Sample mine-review01 -Mode route
# Configured source checkout, after loading its private-input configuration:
uv run sf2 verify adapter
# Selected actual engine behavior:
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj -c Release --no-restore --filter 'FullyQualifiedName~MineCannonTests|FullyQualifiedName~CreedRecruitmentTests|FullyQualifiedName~CreedRestorationTests|FullyQualifiedName~OpeningRestoreTests'
```
