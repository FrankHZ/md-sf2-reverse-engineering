# Battles 08–13, the Polca raft and save compatibility

This audit covers the #1734 merge at `52f6a2c3` and compares `8ca3a914..52f6a2c3`. It used retained ordinary-input observations and separately ran the candidate Content, Application and Domain assemblies. The observations of the six battles and first water step remain bounded to the delivered routes. The unchanged v2 Battle01 slot had no upgrade path from the seven-member world to the eleven-member world. The original audit is preserved in [its Git revision](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/e7d8c86d3250aaa748effe0f2332537f8f80bc00/remake/docs/evidence/polca-raft-engine-save-audit.zh-CN.md).

## Confirmed findings and disposition

| Finding | Observation | Follow-up |
| --- | --- | --- |
| Old v2 slot refused | Both worlds decoded the same #1688 Chinese Battle01 slot. First round, three deployed and four retained. The seven-member #1735 world returned `SessionStarted`; the eleven-member #1738 world returned `save-battle-retained-roster`. Battle recipe 1 and gameplay revision 4 were unchanged. The retained plus deployed roster had to equal the entire current package. The original bytes remained intact. | [Roster compatibility #1749](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1749), [PR #1755](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1755) |
| Wrong berth appeared continuable | In `polca-raft`, keep Map72 (43,47), F64 and F10. A berth on Map13 or at (43,47) restored, recaptured and restored again. Slot 31 was missing or overlapped the player. Moving south requested event 3 in all three cases; only the original aligned. Missing berth or Y64 returned `save-raft-berth`. The historical generic restore checked the chapter recipe name and broad bounds. | [Berth policy #1750](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1750), [save agreement](../application/polca-first-raft.md#save-and-continue) |
| New ally growth depended on upgrade order | Same Map58 slot, 2,806 gold and primary RNG 1507850002. Direct 1→4 and staged 1→2→3→4 without walking differed only in Allies. Direct/staged Gerhalt base ATT 35/36 and DEF 22/21; Luke max HP 28/29, DEF 18/19 and AGI 22/23. Each step used a copy of the saved RNG to initialize just the newly missing records. | [Roster compatibility #1749](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1749) |
| Birdman AI priority absent | BDMN source class 6 was absent from the four ranking tables. Comparing equal-priority 15 candidates of class 0 and 6 returned `ai-target-class` for each table. Controlled Battle13 with Luke admitted early failed before first input. Luke joined after Battle13 in the ordinary route. | [Target priority #1751](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1751), [PR #1753](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1753) |
| Post-raft v2 lost the berth | In a controlled Battle13 first-turn snapshot, give story F64 and a Map72 (43,48) berth. Capture, serialization and restore all succeeded, leaving F64 set and `Story.Raft` null. v2 had no berth field. The ordinary six battles preceded raft acquisition. | [Berth handling #1750](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1750) |

Battle10 Power Ring (source ID 19) added a carry-only exception at both initialization and AI
equipment checks. Generic return validation and SF2 healing also both knew immortal IDs 7/28.
The former was a confirmed duplicated restriction. The effect of the latter on hypothetical
alternative recovery policies was **Inferred**. See [carried items and return treatment #1752](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1752)
and [PR #1754](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1754).

## Ordinary route evidence consulted

The historical content producer selected triggers, entry/after programs, placements and egress.
The world reader validated encounter identity and shared ally definitions. Entry consumed the live
roster and conditions; the outcome used the actual resources and carried state. The individual
application owners retain observations, refusals, and the original and remake boundaries:

| Encounter | Bounded ordinary observations |
| --- | --- |
| 08 | Decline repeats on deck; Yes enters Map72. Defeat or paid EGRESS returns to Map14 (45,7). Victory clears F408, sets F508, and leads through construction and recovery. The reached route included Kiwi. [Ship and town](../application/battle08-new-granseal.md) |
| 09 | Victory at Map69 (1,1), 3,693 gold and Jaha poisoned. Defeat uses carried New Granseal egress. Peter follows and uses AI. [New Granseal and Ribble](../application/new-granseal-east-shrine.md) |
| 10 | The two reached paths cover departure without May and victory with her. Victory keeps Power Ring, fallen allies and 5,033 gold. Map70 (17,19) can reach the caves; East Shrine is a separate route and F77 is not a hard cave prerequisite. [Two routes](../application/new-granseal-east-shrine.md) |
| 11 | Defeat/retreat returns to New Granseal with 2,976 gold. Victory returns to (15,11), with 6,233 gold and five fallen allies. Polca church supplies paid Raise. [Caves and church](../application/dark-caves-bedoe.md) |
| 12 | Fairy rescue is separate. Victory returns to (23,8) facing left; F714/F712 set, Chester and Slade remain fallen. A defeat case returns with F512/F712/F714 clear. Peter's control change occurs at Volcanon. [Mountain and Bedoe](../application/dark-caves-bedoe.md) |
| 13 | East approach admits the confrontation and Gerhalt. Victory returns to (16,12), facing down, with F513/F75 and 9,753 gold, before acquiring the raft. Defeat uses fixed Bedoe (27,35), Up. A natural return to town after that defeat was not established. [Polca](../application/polca-first-raft.md) |

The mayor's first three contacts set temporary F260–262; the fourth returns before F716/F64 are
written. The raft record creates a physical berth on Map72. Luke's membership and deployment caches
are separate: retained projections place him in reserve before boarding; Continue reconstruction in
the same progression includes him in the active roster. The #1738 `win04/{berth,boarded,first-step}.json`
observations were (43,47)→(43,48)→(43,49), mode 0→2→2, with berth (43,48), 9,753 gold, step register
26807 and input at each checkpoint. (44,47) failed alignment. Landing still returned `raft-landing`
and riding was ineligible for Save in this audit. Cross-map water travel, the far bank, original
timing and player experience remained **Unknown**. [Raft rules and evidence](../application/polca-first-raft.md)

The long wait and Map72 admission identified in the [earlier audit](pre-voyage-readiness.md) were
addressed by #1733 and #1735. The [script boundary review](engine-script-boundary-review.md)
retains the resource, follower-order and music-context owners.

## Save and restore observations

Ordinary Save freezes input, commits music observations and queries eligibility before revision-bound
capture. Continue selects the configured world, checks the envelope and dispatches by version and kind
before mounting an accepted session. This audit exercised serialization and restore in memory and
queried `SlotRead.CanRecover`. It did not reproduce disk replacement, recovery-button behavior or
window shutdown. [Save agreement](../application/battle-save-and-continue.md)

| Field input | Measured restore, capture, encode/decode and second restore |
| --- | --- |
| r1, #1735 Map58 `battle07-return`, slot 1 | 7→11 with 2,806 gold. Existing records and other progress equal. Remove seventh ally or add Luke's F10 to obtain `save-roster`. |
| r2, #1736 `after-year` (slot 2), `after09`, `before-may`, `after10`, `shrine-complete` (slot 1) | 9→11. Castle, Map69, Ribble, Map70 and shrine. Existing ally records match. Shrine clears entry-temporary F256/F257, preserving F77/F729 and resources. |
| r3, #1737 `after11`, `after12`, `before13`, slot 1 | 10→11. Cave 6,233 gold, volcano and east Polca 7,413 gold. Luke is initially unjoined. |
| r4, #1738 `after13`, `before-raft`, slot 1 | 11→11, all captured progress matches. First has no berth; second has slot 31 at Map72 (43,48), two fallen members and Luke's equipped sword. |

Field format v1, gameplay 4, used recipe prefixes of 7/9/10/11. Comparisons included full existing ally
records, equipment, HP, activation, both RNGs, gold, transactions, flags, steps, music, egress and
berth. Adding F260–262 to `after13` then restoring cleared all three. No dialogue was run from that
variant. On `after10`, altered (63,63) and (0,0) returned `map-entry-position`; (43,41) restored.
An admissible standing position does not prove a walking route between the banks.

Six controlled v2 `battle-player-input` cases used first-turn movement and ally prefixes
7/8/9/9/9/10, constructed using `after-year`, actual route music and egress. All six passed
`BattleSave.Capture → WriteRecord → ReadRecord → Restore → CaptureBattleSave` and yielded the
same bytes at fixed slot, sequence and time, including queue, cursor, both RNGs and retained
members. The next turn and ordinary historical slots for those six were outside this comparison.
An earlier construction of 09–13 completed with `poison-death-context` failures before capture;
adding `BattleDeathCredit(null)` as in `BattleEntry.Continue` exposed the early-Luke
`ai-target-class` failure for 13. The corrected compositions use the actual join frontier.
Dialogue, moving, riding and arbitrary battle frames were outside the two save agreements.

### Reproduction at the audited revision

Use the reviewed revision's assemblies and private read-only sample copies, loading the chapter's
world and selected scene data with `OpeningRestoreReader.Read()`. Pass the accepted definition
to the same restore entry. The original helper below records the field round trip and rejection code.

```csharp
var record = OpeningSaveJson.ReadRecord(File.ReadAllBytes(sample), slot);
var result = SaveRecordRestore.Restore(definition, record, RuleCompositions.ForGame());
if (result is SessionStartFailed failed) { Console.WriteLine(failed.Failure.Code); return; }
var session = ((SessionStarted)result).Session;
var here = session.Current;
if (record is OpeningSaveRecord field) {
    var captured = session.CaptureSave(here.SessionId, here.Revision);
    var bytes = OpeningSaveJson.WriteRecord(field with { Data = captured.Data! });
    var next = OpeningSaveJson.ReadRecord(bytes, slot);
    Console.WriteLine(SaveRecordRestore.Restore(definition, next, RuleCompositions.ForGame()).GetType().Name);
}
```

- Compare the #1688 slot first with #1735 `release04/world.json`, then #1738 `release/world.json` and
  `scene-raft.json`. For staged Map58 migration, use #1736 `release/world-next.json`, then #1737 and
  #1738 `release/world.json`; the first two use the corresponding `scene.json`.
- Use the revision's `BattleSaveProfileTests` construction from `BattleAdvancer.Start` through
  `WithStory`, route ordinal 0, maps 72/69/70/48/52/13 for encounters 8–13. Supply the recorded
  carried field state and entry death context, then run the v2 round trip described above.
- In disposable copies of `before-raft`, change one decoded field, then recalculate the checksum
  for structural changes. Wrong checksum reports `save-checksum`, format 99 `save-version`,
  unknown v2 kind `save-kind`, field recipe 5 `save-incompatible`, and removed first active member
  `save-membership`. With a valid backup, only the checksum failure among the tested checksum,
  unknown version/kind, `save-battle-retained-roster` and `save-raft-berth` cases allows recovery.
