# Creed restoration and the shrine entrance

**Confirmed** in the private remake candidate: both ordinary [Willard victory](willard-burrow-battle23.md)
saves continue through Map27's exit, all 58 restoration messages, usable Map33 control and the first
Map34 entrance. Both languages save in each map and cold-continue with every decoded save-data field
unchanged. This is not yet complete chapter acceptance: the shrine-to-mansion warp fails audio
selection, and restoration is outside the existing battle-entry skip predicate.

## Agreement and source boundary

Rigor remains **High** under the [campaign agreement](remaining-campaign-risk-baseline.md) and
[Issue1874](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1874). The outcome is the
complete restoration followed by an ordinary, movable and saveable shrine entrance, preserving each
inherited party and independent F519 history. Reentry, body/follower/camera state and full cold-save
equality are acceptance boundaries. Consultation, recruit choices, Fairy handoff and Oddler's departure
belong to subsequent chapters. No original-game capture or new comparison platform is introduced.

The [restoration source owner](../../../docs/research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md)
and [shrine entry owner](../../../docs/research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md)
bind the USA input and SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`.
Their selectors, command operands and population are **Confirmed** static evidence. Native observations
below establish remake behavior only; original hardware timing and human visual acceptance remain
**Unknown**. The [selection](../../content/private/pre-voyage-opening.json) consumes these owners through
the existing map/program compiler. Story flags and map branches are not added to generic execution.

## Ordinary restoration

From the unchanged Map27 (20,15), Right victory save, Right exits to Map33 (15,9), Right. The source
selector admits the F523 seven-resident population. F783 selects the default population on a later
entry; F784/F786 and the last F22 override retain their distinct original ordering. The latter story
stages are not admitted save recipes by this chapter.

The ordinary init calls `cs_5B016` only with F523 set and F783 clear. Both language runs consume texts
1840–1897 in order before the caller sets F783 and the Hassan return map. The compiler now follows
`ac_jump eas_Init` by appending Init's actual actions and rebasing its local jumps. TwirlGrow's three
concurrent actors therefore run the speed/flag/size/sprite-refresh tail, including its completion wait,
before idling. Compact action-cursor and actual draw observations retain the intermediate sizes and
final 24-pixel presentation; no screenshots or per-frame capture archive is used.

At returned input Bowie is (16,9), Up, Goliath is (17,10), Down, and Creed/Fairy are hidden by the
scene. Peter follows Bowie; actor31 follows Peter. Eric14, Randolf16, Tyrin17 and Karna15 remain
parked candidates, not joined allies. The camera follows slot0, finishes scrolling and releases input.
F783 is the only persistent story flag added along this route. Both parties retain their own names,
joined/active lists, HP/MP, packs/equipment, gold, once receipts, Deals and raft state. Ordinary play
advances RNG; the seeds stored at each actual save are preserved by cold Continue.

The existing [skip boundary](desktop-arrival.md) requires an entering battle and a before/load/start
continuation. Restoration does not qualify. No skipped restoration is claimed, and no new exploration
skip policy is silently introduced. Normal-versus-skipped restoration acceptance remains unresolved.

## Shrine entrance and persistence

Map33's (9,5) warp requests Map34 (26,28), Left. Ordinary input reaches that entrance in both
languages. Map34 has 21 source residents: Creed and Fairy, parked special Evil Spirit252, the parked
beast/boy and sixteen parked effect actors. Special sprites at or above240 use physical slot47 and
shared sprite slot32 without consuming an ordinary allocation slot. The two ordinary follower
identities therefore survive before the resident allocation. The asset reader and adapter admit
Evil Spirit's existing two-frame, 48-pixel special-sprite representation; its visible consultation
animation is outside this observation.

The default Map34 init is return-only. No consultation flags are set at arrival. The original
wildcard Y13 zone still requires a working-layout zone marker and calls the consultation before
writing F784 and F802. This chapter neither invokes its unsupported native helpers nor fakes those
completion flags. F784's later empty population remains separately selected.

The `creed-mansion` save recipe now admits completed restoration while retaining first-visit
compatibility. Cold reconstruction places Goliath at (17,10), Down with F783, otherwise (17,12),
Down with F774. It does not replay restoration, rewards or transient door copies. The new
`evil-spirit-shrine` recipe admits settled active-area positions before F784, with the existing
Hassan return register. It does not invent a F523/F783 entrance prerequisite. Both names appear in
the ordinary localized Save/Continue UI.

All four new cold Continue → immediate Save comparisons match their complete decoded `data` objects.
Temporary flags256–383 are absent at the fresh Map34 entrance. Old Battle23 and Desktop secondary-plane
slots also load and save; their only recorded-data addition is the already adopted empty Caravan
depot (`battle.custody.depot` or `depot`). No old slot is rewritten in place. English retains F519
clear and Chinese retains F519 set; these are distinct journeys, not alternate translations of one save.

## Reachable interactions and remaining boundary

| Consumer | Current support and observation |
| --- | --- |
| Goliath after restoration | Original text1904 and return to movement observed from an ordinary restored save. His (17,10) placement still obstructs the closed southern doorway. |
| Upper-room books and locked door | Existing generic Search and source Map33 descriptions are admitted. A book Search was issued from (14,3), Up; its transient reading was not separately retained, so no exact text claim is made. The locked-door branch was not exercised. |
| Map33 lower rooms, magic-door zone and item placements | Existing source inventory remains admitted: F164 Healing Rain, F165 Antidote, F166 Medical Herb and F167 Running Ring. No new pickup is claimed. The restored Goliath/door layout prevented the attempted ordinary route to the magic-door zone; its F783 no-op guard has static evidence only here. |
| Map33 north warp | Ordinary first Map34 entrance and Save observed in both languages. |
| Map34 return and internal passage | Original (26,28) return and (22,29)↔(24,2) passage remain content-driven. The attempted return fails before transfer: SFX89 has multiple admitted timer variants but none for current MUSIC_WITCH TimerB193. Internal passage consumption remains unobserved. |
| Map34 local interactions | Entity and item callbacks are return-only; chest/hidden-item tables are empty. Generic Search/item behavior is not globally disabled. No consultation, recruit or healing-service result is claimed. |
| Map34 Y13 consultation | Preserved, unsupported next-chapter boundary; F784/F802 remain clear. |

MUSIC_WITCH reuses the accepted title-entry PCM, losslessly wrapped as WAV through the existing
runtime-WAV mechanism. This avoids exceeding the 128-MiB world-document bound and does not acquire
or synthesize new audio. The warp failure is `audio-command-ambiguous`, not missing music. It needs a
properly sourced TimerB193 SFX89 input; selecting an arbitrary variant, deleting other candidates or
retiming the source would hide the failure. Reentry acceptance remains blocked pending that input.

## Reproduction and verification

The registered private `handoffs/issue1874` bundle contains `Play-en.cmd`, `Play-zh-CN.cmd`, unchanged
victory inputs, ordinary restored/shrine saves and a stable runtime. Title → Continue → Slot1 starts
at the shrine entrance; `-StartAt victory` selects the earlier seed for a fresh player namespace.
`Play.ps1 -Language en -Sample <fresh-name> -Mode route` replays the full route;
`cold`, `old`, `interactions` and `revisit` take an explicit `-Seed`. Use disposable namespaces.
`Compare.py` checks retained route and complete cold-save data. `HANDOFF.md` retains exact attempts,
native exits, source/build commands and the unresolved acceptance boundaries. Private content stays local.

Affected actual-engine tests cover cold mansion reconstruction, source-independent shrine admission
and special-resident/follower allocation with varied legal flags and follower counts. Existing
restore/lifecycle/Willard cases, adapter and private consumer builds supply their scoped checks.
Configured source/index checks and the clean committed planner select further public checks; the
engine scope override applies only to remake compilation/localization in shared Python files.
Research extraction and original evidence are unchanged. No legacy aggregate is a prerequisite.

Completed failures are retained. In particular, the successful English shrine cold-save comparison
ended with native `0xC0000005` during Godot finalization. ObjectDB warnings and other finalizer exits
remain separate from gameplay results under [Issue1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).
The frozen candidate requires independent review and resolution of reentry/skip acceptance before
this chapter can be reported complete.
