# Creed restoration and the shrine entrance

**Confirmed** in the private remake candidate: both ordinary [Willard victory](willard-burrow-battle23.md)
saves continue through Map27's exit, all 58 restoration messages, usable Map33 control and the first
Map34 entrance. Both languages save in each map and cold-continue with every decoded save-data field
unchanged. Both also return from the shrine to the mansion, resume ordinary interaction, reenter
the shrine and cold-save again without replaying restoration or rewards. Independent main-gate
acceptance remains separate from these candidate observations.

## Agreement and source boundary

Rigor remains **High** under the [campaign agreement](remaining-campaign-risk-baseline.md) and
[Issue1874](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1874). The outcome is the
complete restoration followed by an ordinary, movable and saveable shrine entrance, preserving each
inherited party and independent F519 history. Reentry, body/follower/camera state and full cold-save
equality are acceptance boundaries. Consultation, recruit choices, Fairy handoff and Oddler's departure
belong to subsequent chapters. A single bounded original-driver audio-only capture supplies the
ordinary return warp at its existing TimerB context; no new comparison platform is introduced.

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
continuation. Restoration does not qualify. Restoration is
accepted through its normal performance under the existing [Issue1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386)
boundary. The corrected chapter agreement does not require or claim exploration skipping.

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

All six cold Continue → immediate Save comparisons match their complete decoded `data` objects:
both languages in the restored mansion, at the first shrine entrance and after an ordinary round trip.
Temporary flags256–383 are absent at the fresh Map34 entrance. Old Battle23 and Desktop secondary-plane
slots also load and save; their only recorded-data addition is the already adopted empty Caravan
depot (`battle.custody.depot` or `depot`). No old slot is rewritten in place. English retains F519
clear and Chinese retains F519 set; these are distinct journeys, not alternate translations of one save.

## Reachable interactions and remaining boundary

| Consumer | Current support and observation |
| --- | --- |
| Goliath after restoration | Original text1904 and return to movement observed in both languages after ordinary shrine return. His (17,10) placement still obstructs the closed southern doorway. |
| Upper-room books and locked door | Existing generic Search and source Map33 descriptions are admitted. Ordinary book Search at (14,3), Up selected source texts427/4120 and returned control in both languages; no full rendered-text equality claim is made. The locked-door branch was not exercised. |
| Map33 lower rooms, magic-door zone and item placements | Existing source inventory remains admitted: F164 Healing Rain, F165 Antidote, F166 Medical Herb and F167 Running Ring. No new pickup is claimed. The restored Goliath/door layout prevented the attempted ordinary route to the magic-door zone; its F783 no-op guard has static evidence only here. |
| Map33 north warp | Ordinary first Map34 entrance and Save observed in both languages. |
| Map34 return and internal passage | Original (26,28) return and (22,29)↔(24,2) passage remain content-driven. Both languages return through (26,28), recover Map33 control with Goliath at (17,10), read1904 and reenter Map34. No1840–1897 replay, reward or consultation flag write occurs. Internal passage consumption remains unobserved. |
| Map34 local interactions | Entity and item callbacks are return-only; chest/hidden-item tables are empty. Generic Search/item behavior is not globally disabled. No consultation, recruit or healing-service result is claimed. |
| Map34 Y13 consultation | Preserved, unsupported next-chapter boundary; F784/F802 remain clear. |

MUSIC_WITCH reuses the accepted title-entry PCM, losslessly wrapped as WAV through the existing
runtime-WAV mechanism. This avoids exceeding the 128-MiB world-document bound and does not acquire
or synthesize that music. The return-warp SFX89/C1 uses one separate, bounded controlled original-driver
capture. Source Map34 area music is MUSIC_WITCH10; its pinned music-bank header supplies TimerB193.
The existing finite observer loads that context, consumes stop254 without replacing its timer, then
dispatches89 and verifies the source channel9 cursor against its finite stream. It observes dispatch
at243, all channels inactive at288 and a60-frame tail through348. The verified canonical ROM and
shared BizHawk2.11.1 installation remain unchanged; isolated launch state and all receipts stay private.
This proves a controlled driver export, not natural original map traversal or hardware-phase equality.

The1400-frame PCM16 stereo44,100-Hz WAV is4,121,348 bytes, capture SHA256
`57370D4D8AA57E83D42E6EE9FBAFC9B6633AD4D86C5645B40B67B08FA066C101`.
The existing materializer retains unchanged sample frames `[179271,207168)` including10ms on each
side of the finite audible interval; the observed quiet tail is48,351 sample frames at at most one
PCM16 unit. The27,897-frame candidate is111,632 bytes with WAV SHA256
`3D015AF9902BE233E1E2271D24E5417EEF4B4AD3BDAB785BC352C22245316DE2`.
No normalization, sample replacement or retiming occurs. Existing candidate admission and world
preparation bind `SFX_WARP_TIMER_C1`; ordinary bilingual return observes this exact cue playing at
TimerB193 and stopped without audio error at regained Map33 control. Other variants remain intact.
The earlier `audio-command-ambiguous` attempt is preserved as a completed failure.

## Reproduction and verification

The registered private `handoffs/issue1874` bundle contains `Play-en.cmd`, `Play-zh-CN.cmd`, unchanged
victory inputs, ordinary restored/shrine saves and a stable runtime. Title → Continue → Slot1 starts
at the shrine entrance; `-StartAt victory` selects the earlier seed for a fresh player namespace.
`Play.ps1 -Language en -Sample <fresh-name> -Mode route` replays the full route;
`cold`, `old`, `interactions` and `revisit` take an explicit `-Seed`. Use disposable namespaces.
`Compare.py` checks retained route and complete cold-save data. `HANDOFF.md` retains exact attempts,
native exits, the bounded audio recipe, source/build commands and the remaining chapter limits. Private content stays local.

Affected actual-engine tests cover cold mansion reconstruction, source-independent shrine admission
and special-resident/follower allocation with varied legal flags and follower counts. Existing
restore/lifecycle/Willard cases, adapter and private consumer builds supply their scoped checks.
Configured source/index checks and the clean committed planner select further public checks; the
engine scope override applies only to remake compilation/localization in shared Python files.
Research extraction and original evidence are unchanged. No legacy aggregate is a prerequisite.

Completed failures are retained. In particular, the successful English shrine cold-save comparison
ended with native `0xC0000005` during Godot finalization. ObjectDB warnings and other finalizer exits
remain separate from gameplay results under [Issue1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).
The [finalizer verification owner](../verification/map3-commission-save.md#later-finalizer-faults-and-ordinary-quit)
records the reproducible English observer failures, two successful ordinary-menu Quit controls and
remaining Unknowns. Subsequent observer acceptance uses the product menu Quit path; the original
failure driver remains available. These controls do not resolve #1716 or establish a lifecycle fix.
The frozen candidate awaits independent review. Consultation, later recruitment, original hardware
timing and human visual acceptance remain outside this chapter result.
