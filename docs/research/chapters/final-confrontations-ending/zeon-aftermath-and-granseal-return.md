# Zeon aftermath, sealing and the Granseal return

Evidence date: 2026-10-22. **Confirmed (static):** ordinary Map59/F543 init
admits one continuous scene, `cs_615E6` through the physical `cs_62658`
tail. Galam's recovery becomes Zeon's hostage confrontation; Lemon takes
him away, Mitula explains Elis's condition, and Bowie carries Elis during
the evacuation. The tail loads Granseal scenery, requests the two-year
transition text, clears384/385, resets ally battle stats and **posts**
Granseal3, 56, 3/Down. Ordinary Granseal/F543 init is only `rts`.
None of these source facts proves completed sealing, elapsed game time,
restored input, a save checkpoint or the whole ending.

## Agreement, provenance and method

Standard rigor for [Issue #1184](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1184)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
The outcome is a connected original-game story/state/entity/text account
from the [accepted entrance](./battle43-outcomes-and-ending-admission.md#return-parameters-and-ordinary-map59-reconstruction)
to the ordinary Granseal return interface. Distinct scene loads, retained
actors, posted events and setup reads justify Standard. Accepted base
`fd25779584b399f28e3954874fd9aa209b11a418`, tree
`f4e5353e053bb178fae1d3197915cd3971ad0b48`; no stacked dependency.
Own this document, predecessor navigation and its focused healing-status
correction authorized in PR1185 review. No index, schema,
contract, producer, engine or maintained parser changes.

Reuse [common scripting](../../world/common-scripting.md), [common maps](../../world/common-maps.md),
[request consumption](../../world/map-event-request-consumption.md), the predecessor's
ordinary543 allocation and [corrected follow/action consumers](../moun-nazca-grans/roft-town-interactions-and-zynk-recruitment.md#actual-join-and-field-follow-effects).
Source paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`scene` means `data/maps/entries/map59/mapsetups/s6_initfunction_543.asm`;
`map/` means `code/common/scripting/map/`; Fnnn denotes a flag.
Addresses and packed words are hexadecimal, intervals half-open, text
IDs decimal. The registered USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This slice directly verifies pinned source, not new source/ROM equivalence.

Linear selected source/consumer reads suffice: source48,254 bytes,
1,138 physical lines, 150 dialogue requests, 32 inline action blocks and
82 motion blocks. The retained script corpus has **one** `cs_615E6`
program containing both labels, 797 top-level records, with tail starting
at record780. Its top-level `encodedBytes` sum3,454 excludes inline
payload bytes; do not call it total scene machine size. Address boundaries
give prefix`$615E6..$62658`=4,210 bytes, shared tail`$62658..$6269C`=68,
helper`$6269C..$626A6`=10. The initial guard is`$615D8..$615E6`.

Reuse the unchanged18,285,979-byte init/script pair and
`local/issue1005/pair.txt` provenance. One selected lowering is necessary
to establish the actual fallthrough/helper/action output and retained
unsupported instructions; it is not final preparation or world publication.
No pair regeneration, full-world processing, frames, seeds or renderer
simulation. Each direct recipe budget<60s/<512MiB, peak **unmeasured**;
small evidence<2MiB, all new outputs<4MiB in ignored `local/issue1184/`.
Reassess growth or cross-system machinery. Granseal epilogue execution,
credits, kiss, extra battles, optional returns, translation and runtime
acquisition remain excluded; Mitula's dialogue *mentions* later recovery
without authorizing execution of it.

## Entrances, actor continuity and command semantics

**Confirmed:** Map59/F543 init checks live543 and branches clear→`rts`,
held→`script cs_615E6`. Scene execution reaches `csWait60` at source1109,
then the **adjacent** `cs_62658` label at1110: no call, return, jump or new
guard intervenes. Script end follows its warp. Separately,
`s3_zoneevents_543.asm` has wildcardX255/Y36→`Map59_21F_ZoneEvent0`;
that callback checks543 and calls **only** `cs_62658` when held. It does
not replay the prefix, carry Elis, hide Mitula, establish follower chains
or assert any other prefix state. Both entrances depend on their callers
and services actually returning. Movement/event admission is not proved.

The predecessor's eleven ordinary fixed rows remain the scene's actors;
there is **no** `loadMapEntities`, create, clone, join or population reset
in this file. Their logical IDs are not combat aliases or fixed physical
slots. Existing other followers can affect physical allocation. Changes
below are script requests conditional on live aliases, HP guards and progress.

| Logical identity | Initial declaration and selected scene changes |
| --- | --- |
|B=ally0 Bowie|12,11/U return; prefix explicitly repositions there, priority0, eas_Init. Later prone motion, recovery, carry pose174, then priority1. No Elis inventory or passenger object is created.|
|P=ally7 Peter|14,11/U, current ally sprite. Relative paths, custom speeds, prone/recovery scripts, finally transient follower of B.|
|L=ally28 Lemon|11,10/R, current ally sprite. Temporary Effect5=186 then `setSprite L,L` resolves his live ally sprite; fire/prone/recovery and paired disappearance scripts. No HP-death/removal-from-force transaction.|
|E=132 Elis|11,15/U sprite204. Pose4=177→Elis204, prone/frozen animation, later hidden as B changes to Pose2=174.|
|A=133 Astral|15,10/L sprite209. Knockback/prone/recovery, examination, then transient follower of P.|
|G=134 Galam/Zeon|13,9/L sprite214. Motion OFF/right orientation, later Init/Jump and sprite163 Zeon-Galam; priority1 before disappearance. Narrative change does not replace the logical actor.|
|M=135 Mitula|63,63/U sprite215, eas_InitSlow. Dialogue3860 precedes explicit13,7/D placement; later slow movements and hide. No new entity allocation.|
|W=136 Woman|63,63/U sprite195, eas_Init. Tail3909 uses this **retained** actor before ordinary Granseal population; it is not Granseal's logical136 Man. No explicit reposition/visibility change here.|
|128 Effect3=178|63,63/D; priority1, autofacing off. Two placements13,14/D and paths to flame, mosaic in/out; no Zeon enemy identity.|
|129 Object1=173|63,63/D; eas_Transparent; later14,8/D and moveRight2, finally hide during Mitula's second jewel acquisition. Narrative jewel identity is author interpretation, not an item transaction.|
|130 Flame1=171|63,63/L, autofacing off.12,8/L then10,8/L with SLIDE_IN; each later63,63/L. setPos63,63 is not HideEntity deletion.|
|131 Object1=173|63,63/L, autofacing off;14,8/L over E, custom speed10/down1, then hide.|

`csc53_setPriority` resolves actor→physical slot, writes byte0 for word0,
byte1 for any nonzero word to `byte_FFAFB0`; FFFF is not a depth coordinate.
`setSprite` resolves ally-valued sprite operands through GetAllyMapsprite;
L,L is not a literal sprite28. `stopEntity`/`startEntity` write animation
counter−1/0 after the live-ally HP gate; they do not stop destination motion.
`ac_motion OFF` writes animation counterFF, not velocity or script stop.
`ac_orient*` writes orientation bits and `ac_updateSprite` refreshes the
sprite; it does not mean the same thing as `setFacing`.

`customActscriptWait` installs the inline address and writes the resolved
**physical slot byte** to its timer. In csc14, GetEntityAddressFromCharacter
returns that slot in D0 before `move.b d0,ACTSCRIPTWAITTIMER(a5)`;
csc15/setActscript and csc2D/entityActions use the same installation write.
For ordinary543 with no additional followers and newly allocated P/L,
G134 resolves to physical9: installation writes9, not0. Its wait tests
script-address equality with `eas_Idle`. A **later executed** inline
`ac_jump eas_Idle` redirects the action cursor through
esc_clearTimerGoToNextCommand, which clears the timer; that is separate
from installation and does not execute the later delimiter as an action.
No frame duration is derived without the actual scheduler.
`ac_setSpeed x,y` packs two bytes
written together into XSPEED/YSPEED by esc10; values5/10/16/48/64 are
source speed units, not tiles per tick. `eas_StopMoving` executes
`ac_moveRel0,0` then idle; esc04 replaces destination relative to **current**
position, so a timed partial move can stop between tile endpoints.

`entityActions[Wait]` builds a scratch script, clears collision-check bits5/6
with AND9F and terminates by jumping to idle. Moves set relative destinations
and wait for them; face commands include their duration. Its waiting form
waits for that actor's script pointer to idle, not all previously launched
actors. `waitIdle` has that same narrow pointer predicate. Other actors'
nonwaiting work can overlap, be replaced or fail to complete. Plain waits
call Sleep with low-byte duration; debug skip affects them.

`eas_Jump` disables autofacing, requests0,−1/wait5/0,+1/wait5/0,0,
reenables autofacing then idles; it is not a new jump physics primitive.
`jumpUp1` and `jumpUp10` in P's final inline block both copy the same
34-byte eas_Jump template via csc2D_D/loc_46914: their second source byte
does **not** change that copied jump's waits. `eas_Bump*` has the analogous
directional out/back pair. `eas_DeactivateAutoFacing` immediately jumps to
idle after OFF; bytes physically following that jump are unreachable from
that entry. Reuse the accepted collision/follow corrections, not macro names
as behavioral evidence. No tile-exact final actor positions are inferred
from timed movement prefixes.

## Galam appears to recover, then takes Elis hostage

**Confirmed:** prefix detaches the camera, reloads current Map59 at9,6,
repositions B, assigns priorities and copies7×4 blocks12,39→10,3 from
current working RAM. It prepares128/130/131 autofacing, B Init and129
transparency. G's motion-off/right-facing inline block precedes a partial
downward step, StopMoving and black fade-in. Text3760–3773 accompanies
G's apparent confusion, L's response and E's approach. E uses Pose4,
then G Init/Jump, animation restarts and E sprite restore precede the
speed64 partial movements that accompany the hostage narration.

Speed10 G/E blocks precede their parallel up/left paths. B/P/A jumps are
launched before each L waiting jump; this does not wait for all four.
L steps up; three flash4 requests lead to G Init→sprite163, shiver and
MUSIC_ZEON_ATTACK. E/P/B/A/L reaction scripts and E AnimSpeedx2 follow.
L's speed10 left step, later Init/speed48 right step and camera changes
accompany Zeon's threats. B/P turn toward each other, B nods and both
face up before the next approach. The dialogue is story testimony; no
new enemy HP, boss revival or battle admission follows from it.

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3760/T|80/G|Galam asks about his surroundings.|`{N}{W2}`|
|3761/S|80/G|He cannot raise himself.|`{W1}`|
|3762/S|80/G|He recognizes Lemon and seeks an explanation.|`{NAME;28}{N}{NAME;28}{W1}`|
|3763/S|0/L|Lemon addresses his king hesitantly.|`{W1}`|
|3764/S|0/L|Lemon struggles to answer.|`{W1}`|
|3765/T|80/G|Galam fears his failing sight means death.|`{N}{W2}`|
|3766/S|80/G|He appeals to Lemon against dying.|`{N}{NAME;28}{W1}`|
|3767/S|0/E|Elis explains the possession.|`{N}{N}{W1}`|
|3768/T|80/G|Galam questions the devil account.|`{W2}`|
|3769/S|80/G|He identifies Elis.|`{N}{W1}`|
|3770/S|0/E|Elis confirms her identity.|`{W1}`|
|3771/S|80/G|Galam asks Elis to approach.|`{N}{W2}`|
|3772/S|80/E|Elis checks whether he can see her.|`{N}{W1}`|
|3773/S|FFFF/N|Narration identifies the seizure of Elis.|`{N}{W1}`|
|3774/T|0/L|Lemon challenges the action.|`{W2}`|
|3775/S|0/L|Lemon reacts to Galam standing.|`{N}{W1}`|
|3776/S|80/G|The captor mocks the Galam identity.|`{N}{W1}`|
|3777/S|80/G|The captor denies being Galam.|`{W1}`|
|3778/S|80/P|Peter identifies Zeon.|`{W1}`|
|3779/S|0/L|Lemon challenges the apparent return from death.|`{N}{W1}`|
|3780/T|80/G|Zeon dismisses their understanding.|`{N}{N}{W2}`|
|3781/T|80/G|Zeon denies the sword can kill him.|`{N}{W2}`|
|3782/S|80/G|Zeon describes their achievement as weakening him.|`{W1}`|
|3783/T|80/A|Astral recalls Mitula's instruction.|`{N}{W2}`|
|3784/S|80/A|Astral regrets remembering too late to protect Elis.|`{N}{N}{W1}`|
|3785/S|0/L|Lemon condemns Zeon.|`{W1}`|
|3786/S|80/G|Zeon threatens Elis if Lemon advances.|`{N}{W1}`|
|3787/S|0/L|Lemon calls for her rescue.|`{N}{W1}`|
|3788/S|0/G|Zeon summons Bowie and the phoenix.|`{LEADER}{N}{W1}`|
|3789/S|80/P|Peter asks Bowie how to respond.|`{LEADER}{N}{W1}`|
|3790/T|0/G|Zeon stresses urgency and the hostage threat.|`{N}{W2}`|
|3791/S|0/G|Zeon repeats his demand to approach.|`{W1}`|
|3792/S|80/P|Peter accepts the coercion.|`{W1}`|
|3793/S|FFFF/N|Narration records Bowie's reluctant agreement.|`{LEADER}{W1}`|

## The jewel, retaliation and Lemon's intervention

**Confirmed:** B/P speed16 upward approach precedes the jewel demand.
P's turning/waits, B's right step and P's speed10 timed left/right steps
stage removal; there is no RemoveItem/AddItem call. P advances up2,
sets facing, then right2; G/E move right1. Three SFX_SPELL_CAST plus
entityFlashWhite G20 requests accompany the claimed power recovery.
131 moves down with E, E's motion-off/down orientation follows, then131
is hidden. L moves right and G threatens him; three blast/flash4 pairs
precede Flame130 at12,8/L and SLIDE_IN. L and130 speed10/down2 run
in parallel, waiting only L; L becomes motion-off/down-oriented.

P left1 and A up1 precede speed48 knockback: G BumpRight/white40,
camera P, P right4 and A downRight4, falling sound, waitIdle A, lightning,
quake4 and G destination13,8. P/A motion-off orientation blocks and a
quake0 reset follow. B speed64/up1/BumpUp precedes narration3822;
129 is placed14,8/D and sent right2. B speed48/down5 after G BumpDown
and white flashes ends at waitIdle B, lightning/quake4, prone orientation,
quake0 and narration3825. This is no STUN battle-status write.

G's stop/start/2xRightLeft and slow right step precede Effect128's first
13,14/D MOSAIC_IN, speed10/up3/upLeft1, laser sound, Flame130 relocation
to63,63 and MOSAIC_OUT. MUSIC_FINAL_BATTLE accompanies L's recovery:
Init/start/down, speed10 and two upward steps. L challenges G and is
driven left3 amid five blast/flash4 requests. After waitIdle L, a second
flame at10,8 and Effect128 up3/upLeft3 sequence repeats the removal
staging. L advances/retreats/advances around three further blast/flash4
requests before closing on G. These are selected entity/effect requests,
not proof that Volcanon or a battle damage service executed.

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3794/T|0/G|Zeon stops the approach and addresses Peter.|`{N}{W2}`|
|3795/S|0/G|Zeon demands Bowie's evil jewel.|`{N}{LEADER}{W1}`|
|3796/S|80/P|Peter claims it cannot be detached.|`{W1}`|
|3797/S|0/G|Zeon insists on the attempt.|`{W1}`|
|3798/S|80/P|Peter hesitates.|`{W1}`|
|3799/S|FFFF/N|Narration describes Peter pulling the jewel.|`{NAME;7}{N}{W1}`|
|3800/S|80/P|Peter proposes another attempt.|`{W1}`|
|3801/S|FFFF/N|Narration describes Peter freeing Bowie's jewel.|`{NAME;7}{N}{LEADER}{W1}`|
|3802/S|80/P|Peter reacts with surprise.|`{W1}`|
|3803/S|80/P|Peter asks where to place it.|`{W1}`|
|3804/S|0/G|Zeon specifies a nearby placement.|`{W1}`|
|3805/S|80/P|Peter agrees.|`{W1}`|
|3806/S|FFFF/N|Narration describes the jewel's placement.|`{NAME;7}{N}{W1}`|
|3807/S|0/G|Zeon welcomes its recovery.|`{N}{W1}`|
|3808/S|FFFF/N|Narration describes Zeon collecting it.|`{W1}`|
|3809/S|0/G|Zeon reports returning strength.|`{N}{W1}`|
|3810/T|80/A|Astral warns of Zeon's recovery.|`{N}{W2}`|
|3811/S|80/A|Astral appeals to Mitula and Volcanon.|`{N}{W1}`|
|3812/S|0/G|Zeon predicts his restoration soon.|`{N}{N}{W1}`|
|3813/S|0/G|Zeon dismisses Elis after using her.|`{N}{W1}`|
|3814/S|80/E|Elis reacts to the dismissal.|`{W1}`|
|3815/S|FFFF/N|Narration describes a dark cloud over Elis.|`{N}{W1}`|
|3816/S|80/E|Elis gives a distressed reaction.|`{W1}`|
|3817/S|0/L|Lemon calls to Elis.|`{W1}`|
|3818/S|0/G|Zeon threatens Lemon next.|`{NAME;28}{N}{W1}`|
|3819/S|0/L|Lemon reports the burning attack.|`{N}{W1}`|
|3820/S|0/G|Zeon claims the fire harms vampires too.|`{N}{W1}`|
|3821/S|80/P|Peter protests.|`{W1}`|
|3822/S|FFFF/N|Narration describes Bowie dislodging the jewel.|`{LEADER}{N}{W1}`|
|3823/S|0/G|Zeon retaliates verbally against Bowie.|`{LEADER}{W1}`|
|3824/S|0/B|Bowie reacts to the blow.|`{W1}`|
|3825/S|FFFF/N|Narration describes Bowie's dazed condition.|`{LEADER}{W1}`|
|3826/S|80/G|Zeon gloats at the retaliation.|`{W1}`|
|3827/S|0/L|Lemon reacts after recovering.|`{W1}`|
|3828/S|80/A|Astral recognizes Lemon's return.|`{NAME;28}{W1}`|
|3829/S|80/G|Zeon reacts with disbelief.|`{W1}`|
|3830/S|80/G|Zeon orders Lemon away.|`{NAME;28}{W1}`|
|3831/T|0/L|Lemon appeals to his king.|`{W2}`|
|3832/S|0/L|Lemon asks him to stop.|`{W1}`|
|3833/S|80/G|Zeon rejects Lemon's approach.|`{W1}`|
|3834/S|0/L|Lemon declares he will take the king away with him.|`{N}{W1}`|
|3835/S|80/G|Zeon attacks again.|`{W1}`|
|3836/S|0/L|Lemon persists in addressing Galam.|`{W1}`|
|3837/T|80/G|Zeon questions Lemon's survival.|`{W2}`|
|3838/T|80/G|Zeon insists the survival is impossible.|`{W2}`|
|3839/S|80/G|Zeon reports weakening and seeks the jewel.|`{N}{W1}`|
|3840/S|0/L|Lemon invites the king to leave with him.|`{W1}`|
|3841/S|80/G|Zeon resists returning to the other place.|`{N}{W1}`|
|3842/S|80/G|Zeon promises another return.|`{N}{W1}`|
|3843/S|80/G|Zeon cries out during departure.|`{W1}`|

G/L speed5 left1 scripts, repeated headshakes and waitIdle G precede
G priority1/L priority0. L's timed upRight1 is stopped after8; G/L up3
run together while only G is waited. Falling sound then launches
G eas_TwirlShrinkDisappear and waits for L's same script. That action
waits5 between facing/size steps22,20,18,16,14,12,10,8,6, then sets
position63,63, disables resizing, size18 and jumps idle. It does not call
HideEntity, clear force flags or kill/revive a combatant. G's final text
still resolves the retained G identity. Five blast/flash20 requests surround
quake2 and block copy20,39 size7×4→10,3; quake0/wait60/music fade follow.
The block copies each write28 words/56 bytes, not decoded geometry or
proof of a visible sealed tower.

## Checking Bowie and Elis; Mitula's explanation

**Confirmed:** camera P; P Init/start/down and headshakes precede the
report about L. Camera A; A Init/start/down, right1/up-facing precede
the lament. P's two waiting jumps and staged downward/left paths bring
P/A toward B. B Init/start/down/headshake precede the nod narration.
They next approach E: A up2, P left1/up1, B up5/right-facing,
A up5/right2/up1/left-facing and P up6/right1/up-facing. Priorities
P1/E0 precede two P BumpUp scripts and the shaking narration.

At source813–815, `showPortrait $80,132`→`executeSubroutine csub_6269C`
→single3853. Helper`$6269C..$626A6` executes `moveq #1,d0`,
`jsr j_ClosePortraitEyes`, `rts`. Alias resolves to portraitfunctions.asm
ClosePortraitEyes`$154F6`: clears BLINK_CONTROL_TOGGLE, waits VInt,
uses bit0=1 for blink update and bit1=0 for mouth update, through
UpdatePortrait with BLINK_DATA/MOUTH_DATA and their tile counts. This
is a portrait service, not an Elis status mutation. The following single
request finds an already open portrait; showPortrait's open-window guard
can preserve that helper-modified portrait rather than reopening it.

A's speed10 timed left/right examination steps each stop after16; the
text claims breathing rather than death. Mitula's3860 request occurs
while M is still the previously declared offscreen actor. Only after
staggered B/A/P reaction scripts and MUSIC_MITULA does setPos M13,7/D
and entityFlashWhite70 stage her appearance. Reaction jumps and A up1,
camera8,4, P up1, later speed10/down1, then Init/left3/up3 accompany
their conversation. A Init/left1/up1 accompanies the Jewel of Light
question; no past-story truth is newly proven by a character's account.

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3844/S|80/P|Peter reports Lemon taking Zeon away.|`{NAME;28}{N}{W1}`|
|3845/T|80/A|Astral declares the confrontation ended.|`{W2}`|
|3846/S|80/A|Astral mourns the result.|`{W1}`|
|3847/S|80/P|Peter asks about Bowie's condition.|`{LEADER}{W1}`|
|3848/S|80/P|Peter tells Bowie what Lemon did.|`{LEADER}{N}{NAME;28}{W1}`|
|3849/S|FFFF/N|Narration records Bowie's nod.|`{LEADER}{W1}`|
|3850/S|80/A|Astral remembers to check Elis.|`{N}{W1}`|
|3851/S|80/P|Peter calls to Elis.|`{W1}`|
|3852/S|FFFF/N|Narration describes Peter shaking her.|`{NAME;7}{N}{W1}`|
|3853/S|80/E|Elis's speaker slot describes her unresponsiveness.|`{W1}`|
|3854/S|80/P|Peter describes her cold and pallid appearance.|`{N}{W2}`|
|3855/S|80/P|Peter asks Astral whether she died.|`{W1}`|
|3856/S|80/A|Astral reserves judgement and approaches.|`{N}{W1}`|
|3857/S|FFFF/N|Narration describes Astral's examination.|`{W1}`|
|3858/T|80/A|Astral reports breathing despite the cold.|`{N}{W2}`|
|3859/S|80/A|Astral admits uncertainty.|`{W1}`|
|3860/S|0/M|Mitula identifies a coma.|`{W1}`|
|3861/S|80/A|Astral recognizes Mitula.|`{W1}`|
|3862/S|0/M|Mitula greets and commends the group.|`{N}{LEADER}{NAME;7}{N}{W1}`|
|3863/S|0/P|Peter welcomes Mitula's arrival.|`{N}{W1}`|
|3864/S|0/M|Mitula describes unseen observers.|`{W1}`|
|3865/S|80/P|Peter asks about the plural observers.|`{N}{W1}`|
|3866/S|0/M|Mitula prompts Peter to infer her meaning.|`{NAME;7}{N}{W1}`|
|3867/S|80/P|Peter suggests Volcanon.|`{W1}`|
|3868/S|0/M|Mitula credits Volcanon's past help.|`{N}{W1}`|
|3869/S|80/P|Peter asks when that help occurred.|`{W1}`|
|3870/T|80/M|Mitula questions Peter's doubt.|`{W2}`|
|3871/S|80/M|Mitula credits Volcanon for protecting Lemon.|`{NAME;28}{N}{N}{W1}`|
|3872/S|0/P|Peter recalls Volcanon's declared withdrawal.|`{N}{N}{W1}`|
|3873/T|80/M|Mitula describes that withdrawal as deception.|`{N}{W2}`|
|3874/T|80/M|Mitula describes a risk from an earlier divine intervention.|`{N}{N}{W2}`|
|3875/T|80/M|Mitula predicts continental destruction in that alternative.|`{N}{W2}`|
|3876/S|80/M|Mitula warns of dependence on divine aid.|`{N}{N}{W1}`|
|3877/T|80/A|Astral connects Volcanon to the light jewel.|`{N}{W2}`|
|3878/S|80/A|Astral connects that jewel to Lemon's protection.|`{NAME;28}{W1}`|
|3879/S|0/M|Mitula acknowledges Astral's deduction.|`{W1}`|

## Mitula takes the jewels and the party evacuates

**Confirmed:** rumble/quake1/wait90/quake0 precede Mitula's departure
request. M down1, speed10 timed down/up steps stopped after16 and
white40 accompany the first jewel narration. M InitSlow/right3 precedes
nod→hide129→second jewel narration. No inventory, equipment or separate
jewel-object ownership transfer is called. M faces up/white60; A calls
her back and down1/right-facing precedes recovery questions. A's nod,
turns and right1 step accompany the advice; M headshakes, then up/
white100→hide135, wait50 and music fade conclude the conversation.

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3880/T|0/M|Mitula announces departure.|`{W2}`|
|3881/S|0/M|Mitula requests the light jewel.|`{W1}`|
|3882/S|FFFF/N|Narration describes taking it from Bowie.|`{N}{LEADER}{W1}`|
|3883/S|0/M|Mitula requests the evil jewel too.|`{W1}`|
|3884/S|FFFF/N|Narration describes its acquisition.|`{N}{W1}`|
|3885/T|80/M|Mitula states the urgency of resealing Zeon.|`{N}{W2}`|
|3886/S|80/M|Mitula orders evacuation and predicts destruction.|`{N}{W1}`|
|3887/S|0/A|Astral calls Mitula back.|`{W1}`|
|3888/S|80/M|Mitula responds.|`{W1}`|
|3889/S|0/A|Astral asks how Elis can recover.|`{W1}`|
|3890/T|80/M|Mitula identifies poisoning.|`{W2}`|
|3891/S|80/M|Mitula gives a visible sign of poison leaving.|`{N}{W1}`|
|3892/S|0/A|Astral asks whether that guarantees awakening.|`{W1}`|
|3893/T|80/M|Mitula introduces a later kiss condition.|`{N}{N}{W2}`|
|3894/S|80/M|Mitula links that kiss to awakening.|`{W1}`|
|3895/S|0/A|Astral asks how long poison clearance takes.|`{W1}`|
|3896/T|80/M|Mitula makes duration depend on Elis's constitution.|`{N}{W2}`|
|3897/T|80/M|Mitula gives a broad months-to-years estimate.|`{N}{W2}`|
|3898/S|80/M|Mitula ends the conversation.|`{N}{W1}`|

P right1, A speed16/left1/down-facing and P right1/down-facing/two
inline jump copies lead to the evacuation reminder. A Init/left1/down
precedes the carry instruction. Priorities B0/E1, B nod, timed up1
stopped after2, right1/down20, nod→hide132→B facingDown/sprite174 stage
the carry. After staggered P/A look scripts, quake8003 and
MUSIC_BOSS_ATTACK accompany the sealing warning. B's timed down1
stops after2; P/A down1 and the final calls precede B priority1,
follow P→B offset2 and A→P offset2, B autofacing OFF, camera B and
nonwaiting down1/left1/down10. Wait60 then **falls through** to the tail;
there is no waitIdle B or proof all party movement finished.

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3899/S|0/P|Peter observes Mitula's departure.|`{W1}`|
|3900/S|80/A|Astral dwells on the possible long wait.|`{W1}`|
|3901/T|0/P|Peter urges Astral to move.|`{W2}`|
|3902/S|0/P|Peter recalls the evacuation instruction.|`{W1}`|
|3903/S|80/A|Astral agrees to evacuate.|`{N}{W1}`|
|3904/S|80/A|Astral tells Bowie to carry Elis.|`{LEADER}{N}{W1}`|
|3905/S|0/A|Astral interprets the disturbance as tower sealing.|`{N}{W1}`|
|3906/S|0/P|Peter urges haste.|`{W1}`|
|3907/S|0/A|Astral urges Bowie to run.|`{LEADER}{W1}`|

`followEntity` encodes words0007/0000/0002 and0085/0007/0002.
csc2C's alive precheck reads the **first byte** of follower word (00 in
both), so it tests **Bowie's HP**, not P's or A's. Dead B skips each
six-byte operand set; live B does not independently require live P/A or
leader. Offset2 doubles to table bytes4/5=−24,0 **pixels**, not0,24 or
tiles. AddFollower patches physical leader/offsets into a42-byte follower
script, replaces an already listed follower's script and appends only if
absent. It does not set persistent follower flags67/70/71/76. No natural
follow completion, capacity repair or new party member is established.

## Shared tail: scene load, reset, text and posted return

**Confirmed:** these are all17 top-level tail records, in physical order:

| Corpus record / tail offset | Operation and boundary |
| --- | --- |
|780 /0|setQuake5.|
|781 /4|csWait50.|
|782 /6|playSound SOUND_COMMAND_FADE_OUT.|
|783 /10|fadeOutB.|
|784 /12|setQuake0.|
|785 /16|mapLoad MAP_GRANSEAL,0,0.|
|786 /24|resetMap.|
|787 /26|fadeInB.|
|788 /28|reloadMap0,0.|
|789 /34|textCursor3908.|
|790 /38|single3908, FFFF narration.|
|791 /42|single3909, packed0088: modifier0/entity136.|
|792 /46|clearF384 (word flag ID, zero-valued toggle).|
|793 /52|clearF385.|
|794 /58|resetForceBattleStats.|
|795 /60|warp MAP_GRANSEAL,56,3,DOWN.|
|796 /66|csc_end (FFFF); tail ends at offset68.|

| ID / mode | Modifier / actor | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3908/S|FFFF/N|Narration states a two-year interval after resealing.|`{N}{W1}`|
|3909/S|0/W|Woman136 asks about a reported change in Elis's complexion.|`{N}{W1}`|

`csc48_loadMap` detaches view, probes map word3 for LoadMapTilesets,
waits VInt/fade-setting-clear, deactivates VInt, consumes map/X/Y words
and calls LoadMap with D1=3, D0=((0<<8)|0)*3=0; then enables display/
interrupts, reactivates services and waits VInt. LoadMap tests signed
**D1.b**, not map equality. Nonnegative3 writes CURRENT_MAP3 and loads
palette/blocks/layout even if already3. It **does not** call
GetMapSetupEntities, InitializeMapEntities or RunMapSetupInitFunction.
Map59 actors/aliases/action streams remain while scenery becomes Granseal;
no ordinary Granseal init or conversations are invoked by this load.

`resetMap` csc36 calls ResetCurrentMap (LoadMap tail at`$3E3C`): clears2048 longs/
8,192 bytes at FF0000, seeds display scratch at FF2000 and tails to
LoadMap with D0=0/D1=FFFF. Negative D1 reloads current map's properties
without the new-map blocks/layout path. It neither restores the cleared
working layout from source nor rebuilds entities. `reloadMap0,0` csc46
likewise passes D1=FFFF/D0=0; prefix reload9,6 passes D0=1B12. It
retains current layout, does not clear it, and wraps LoadMap with VInt/
display work. Fade-in between reset/reload is a service request with
return dependencies, not proof of observed visuals. Ordinary outer-loop
reentry later uses nonnegative3 and reloads fresh layout separately.

The two-year text is a narrative time jump; no clock/calendar increment
appears here.3909's portrait/speech lookup uses retained Woman136,
not the uncreated Granseal fixed Man136. Tail clears384 and385 **after**
both texts return. It never clears543,542,640,443 or joined/follower flags.
Narrated jewel removal and flag writes are distinct; no inventory removal
or atomic checkpoint is supplied. Interruptions can leave either flag
still held, a partially reset roster or a posted event awaiting consumption.

`resetForceBattleStats` opcode55 calls ResetAlliesBattleStats
`$478C6..$4790E`. D0=0, DBF counter29 visits **all30 ally records0–29**,
including unjoined, reserve and HP0; no Peter/Lemon immortality or living
filter. For each: GetCurrentHp (unused for a branch)→GetMaxHp→SetCurrentHp
**word**→GetMaxMp→SetCurrentMp **byte**→GetStatusEffects→AND.w7
(STUN1|POISON2|CURSE4)→SetStatusEffects **word**→UpdateCombatantStats.
It does not zero level/EXP, kills/defeats, gold, items, X/Y, membership or
story flags. It can revive an HP0 ally to its current max; narrative
disappearance does not stop L's slot being healed.

The refresh at`code/common/stats/updatecombatantstats.asm:$89CE`
reinitializes current stats, reapplies status/equipped-item effects and
rewrites status. Its starting mask excludes CURSE; actual equipped cursed
items can re-add it. Thus AND7 is an **intermediate** write, not a guarantee
that final status preserves every original curse bit. STUN/POISON survive;
SLEEP/MUDDLE/boost/slow/attack/silence do not survive this reset prefix.
Current attack/defence/agility/move and prowess bytes, plus resistance
word, are recalculated from base fields and applicable effects, not
permanently reset base progression. Supplied malformed equipment can
prevent service completion; this is no atomic all-roster transaction.

`csc07_warp` posts MAP_EVENT_TYPE.w=1 then bytes PARAM1..5=
0,3,56,3,3 (Down), and returns. No immediate map transfer. csc_end can
wait for scrolling if dialogue is open and clears view speed before
restoring its saved registers. It is no global action/fade/audio idle
barrier. On return from init **or** zone callback, WaitForEvent sees the
pending type before installing controlled-player action. ProcessMapEvent
clears type before dispatch. Type1/PARAM1=0 idles B, unwinds exploration,
returns D0=3/D1=56/D2=3/D3=3/D4=0 to MainLoop. UpdatePlayerPosFromMapEvent
is a separate service in that handler. Do not confuse this flow with
after43's earlier battle-return parameter reuse.

## Ordinary Granseal entry and the stopping boundary

**Confirmed:** ordinary return goes through MainLoop's Start:
SwitchMap→CheckBattle→ExplorationLoop (unlike the post-battle alternate
entry). SwitchMap's three-row table still maps3+held640→4. This tail
does not clear640: with it held, scene `mapLoad3` still used3 directly,
but the later **ordinary** request can select earthquake Map4. Stable
clear640 gives Map3. BattleMapCoordinates has no Map3 row; CheckBattle
there cannot select a battle even with unrelated unlock flags held.
No later epilogue or extra-battle route is followed here.

ExplorationLoop clears event type on entry, heals living/immortal allies,
sets CURRENT_MAP/CURRENT_BATTLEFF, loads tilesets, selects setup/entities,
initializes ordinary population/mapsprites, clears temporary256–383,
sets80, loads fresh map/layout, installs services and reselects init.
Scene carry sprite, hides, physical follower scripts and temporary working
layout are not durable ordinary population. Existing persistent follower
flags can still allocate actors. Bowie's new seed is56,3/Down with his
live ally sprite and idle script; no persistent carry pose is declared.

Map3 setup source order is default→609→506→543. GetCurrentMapSetup
overwrites its candidate on **each held** flag, so held543 wins over
609/506, not first-match609. Population and init select separately.
With stable543, pointertable543`$626A6..$626BE` selects15 rows at
`$626BE..$62738`: nonallies128–138 are Woman, Worker, Young Man,
Worker, Girl, Girl, Centaur Female, Woman, **Man136**, Guard137 and
Astral138; fixed allies Sarah1, Chester2, Jaha3, Kazin4 follow.
Walking Young Man130 is19,14/radius3; Girl132 is20,21/radius1;
Astral138 is42,9/Down. Those are fresh setup identities, not M/G/A/W
continuity. Ally rows can reuse existing follower aliases and skip their
fixed declarations, as with the accepted ordinary allocator.

Selected init`$628C6..$628C8` is **rts only**: it does not automatically
launch a Granseal epilogue. If543 changes between selection reads, another
init can pair with543 population or vice versa; no transactional setup
snapshot exists. After init, palette/music/fade checks precede WaitForEvent.
With no pending event it installs controlled-player script and waits for
events/A/C. That is a source interface, **not observed usable input**.

The next543-specific zone admission is X2/Y255→Map3_21F_ZoneEvent0
in `s3_zoneevents_543.asm:$62820..$62846`, not the return coordinate56,3.
It has no extra flag guard: selected setup and actual zone admission are
its guards; it calls cs_6283E then requests3912–3914. This identifies the
next boundary only; that script, residents, castle epilogue, kiss and credits
are excluded. No save routine/checkpoint/ending-complete flag is supplied
by the selected init or return tail.

| Supplied-state change | Boundary if services return |
| --- | --- |
|Stable543, prefix admission|Full scene falls through to tail; actor narrative prerequisites come from prefix only.|
|Zone36 with543 held|Tail only; no assumed Elis carry, hidden M, follows or prior scene progress.|
|543 clear at init/zone body guard|No selected script; no claim of control restoration.|
|BowieHP0 at follow prechecks|Both follows skip; tail can still be reached and later all-ally reset can heal Bowie.|
|640 clear / held on ordinary request|Map3 / switched Map4; scene-map load earlier was3 in both.|
|Stable543 with609/506 held|543 ordinary population and rts init win; clearing543 reopens the earlier selectors.|
|384 cleared, interruption before385/reset/warp|Different memory prefix; no rollback or saved completion established.|
|All services return but source text skip enabled|Dialogue commands skip without cursor increment; linear IDs below describe unskipped progression, not actual debug-mode text.|

## Actual producer and current consumers

**Confirmed about accepted repository code:** the one selected diagnostic
registered only this scene file, scene_maps=[3,59], reused the pair and
called OriginalPrograms.compile(cs_615E6) once. No exception; no world
publication. The compiler splits physical labels differently from the
corpus:1451 instructions/29 native-call in main, 29/1 in tail, 2/1 in
helper; main's final jump targets cs-62658. Thirteen cached action
programs remain, including unsupported action operations.

| Actual owner | Expression, refusal and publication limits |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py)|First main native is typed7, source-operation6, setBlocks; earlier0–6 include text-cursor, camera detach, reload/wait1, position and two priorities. Main29 natives comprise19 flashScreenWhite, five waitIdle, two setBlocks, one setDest, one setSprite L,L and quake8003. Tail typed5 is native mapLoad; reset/reload both lower as reload-scene-map+wait1. Only loadMapFadeIn currently has scene-map lowering. Helper's jsr j_ClosePortraitEyes is native. Program dictionaries insert before compile; this run returned, but no admitted execution follows.|
|Same action compiler; [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|212 main motion installations include fixed and inline scripts. SlotTimer/SlotTimerClearCollision installs WaitTimer from entity.Slot, matching the source's physical-slot seed; later action execution can clear it. Transparency/AnimSpeedx2 retain native ac_transparency/ac_animSpeedX2. Twirl retains native ac_resizable/ac_setPos/ac_resizable. Inline jumpUp currently lowers through generic action fallback to native rather than an admitted jump template. Later typed flags/reset/queue-warp do not bypass these earlier refusals.|
|[Map preparation](../../../../src/sf2tool/remake_exploration_maps.py), [final preparation](../../../../src/sf2tool/remake_exploration_content.py)|Existing543 ordinary rows are input declarations, not future scene-created actors. Default59/543 share a map-59-setup key; retained variant-overwrite risk stays **Inferred**. No543 setup/material world or final prepare was requested/published. Final preparation validates identity/pin and serializes after building maps/programs; this is not a disk rollback proof.|
|[Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [story types](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Generic text/portrait/motion/follow/scene-map/reload/reset-party/queued warp types exist; native-call becomes UnsupportedInstruction and native actions become UnsupportedEntityAction. Reader availability does not establish scene content, aliases, portrait bindings or accepted runtime. Battle43 leader/defeated-program refusals remain with predecessor.|
|[ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|UnsupportedInstruction throws before Next/commit; earlier committed prefixes remain. Each instruction can publish separately. QueueMapWarp stores ScriptWarp; null-cursor processing services it before VictoryProgramFinished. This retained after43 outcome-order difference is not fixed by this scene research.|
|[MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|LoadScene preserves physical actors/party while changing map/layout; BindScene requires closed windows, black display and valid palette. ReloadScene instead resets layout to Definition.Layout for both reset/reload, unlike native negative-D1 retention/explicit RAM clearing distinction. Ordinary Apply reselects setup from current flags and rebuilds. None was run for this scene.|
|[OutcomeReturnRules](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), ProgramRunner above|ResetPartyBattleStats calls Heal(all:true) on represented party actors, not a native unconditional30-record array. Nonzero status throws return-status-refresh; it does not implement native AND7 and equipment-derived curse recomputation. Dead represented allies heal with all:true, without proving all original slots or progress fields.|
|[Domain allocator](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [adapter presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../../../remake/docs/capability-status.md)|Allocation, movement/follow and presentation projections are reusable primitives. They supply no reached scene, exact flash20 launches, retained native portrait helper result, actual voices/palettes, service completion or player-ready Granseal evidence.|

**Inferred:** accepted mechanisms could support later integration after
selected content and missing native semantics are supplied. Allowlisting
map3/59, skipping native instructions or treating generic types as scene
support would not close those gaps. This finding authorizes no implementation.

## Dialogue notation and complete inline inventory

The phase tables contain every150 request exactly once. S=`nextSingleText`,
T=`nextText`; each speaker cell retains raw modifier and logical actor.
B/P/L/E/A/G/M/W abbreviations use the identity table above; N is full
FFFF narration (FF modifier/ENTITY_NONE), not actor255. Modifier80
requests the right-side portrait,00 the other side; portrait lookup uses
the actor's **current sprite** and table, so pose/effect/sprite changes
can change eligibility. Dialogue author paraphrases do not reproduce game
sentences. Tokens are the complete ordered meaningful control sequence,
including repeated `{N}`, `{W1}`/`{W2}`, `{LEADER}` and literal
`{NAME;7}`/`{NAME;28}` name substitutions. No variable-text form occurs;
these substitutions are not new recruit/inventory/actor operations.

textCursor3760 initializes the cursor; successful unskipped displays
increment through3907, then explicit textCursor3908 precedes the last two
requests. T displays continue the dialogue; S closes portrait/text and
sleeps10 after display. W tokens belong to text content and do not replace
that command distinction:3771 is **S with W2**, as is3854. csc00/csc02
test SKIP_CUTSCENE_TEXT; held skip advances the operand but does not
display or increment cursor. Portrait-helper ordering/early-open guard
and native return dependencies remain distinct from visible completion.

The following source-order inventory covers **all114 inline blocks**.
`C!`=customActscriptWait; `M`=entityActions, `M!`=entityActionsWait.
M direction distances and face durations are literal source operands.
C action operands retain OFF/speed/orientation/update/jump in order.
Every C block ends with physical ac_end after its idle jump; every M
block ends endActions. Source line is a locator, not a machine address.
Fixed eas_* installations and their intervening sound/camera/text/flag
joins are explained in the phases and accepted consumers above.

| Source line / block | Actor | Exact ordered inline operations |
| --- | --- | --- |
|32/C!|G|`ac_motion OFF; ac_orientRight; ac_updateSprite; ac_jump eas_Idle`|
|39/M|G|`moveDown 1`|
|52/M!|L|`moveUp 1; faceRight 40`|
|66/M!|E|`moveRight 2; moveUp 4`|
|78/M!|E|`moveUp 1; moveRight 1; moveUp 1; faceLeft 10`|
|96/C!|G|`ac_setSpeed 64,64; ac_jump eas_Idle`|
|100/M|G|`moveRight 1`|
|105/M|G|`moveUp 1`|
|112/C!|G|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|116/C!|E|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|123/M|E|`moveUp 1; moveLeft 1`|
|127/M!|G|`moveUp 1; moveLeft 1`|
|140/M!|L|`moveUp 1; faceRight 1`|
|173/C!|L|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|177/M!|L|`moveLeft 1`|
|192/C!|L|`ac_setSpeed 48,48; ac_jump eas_Idle`|
|196/M!|L|`moveRight 1`|
|221/C!|B|`ac_setSpeed 16,16; ac_jump eas_Idle`|
|225/C!|P|`ac_setSpeed 16,16; ac_jump eas_Idle`|
|229/M|B|`moveUp 1`|
|232/M!|P|`moveUp 1`|
|251/M!|B|`moveRight 1`|
|255/C!|P|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|259/M|P|`moveLeft 1`|
|270/M|P|`moveRight 1`|
|286/M!|P|`moveUp 2; faceLeft 1`|
|295/M!|P|`moveRight 2`|
|300/C!|G|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|304/M|G|`moveRight 1`|
|307/M!|E|`moveRight 1`|
|340/C!|131|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|344/M|131|`moveDown 1`|
|347/M!|E|`moveDown 1`|
|351/C!|E|`ac_motion OFF; ac_orientDown; ac_updateSprite; ac_jump eas_Idle`|
|359/M!|L|`moveRight 1`|
|379/C!|L|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|383/C!|130|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|387/M|130|`moveDown 2`|
|390/M!|L|`moveDown 2`|
|394/C!|L|`ac_motion OFF; ac_orientDown; ac_updateSprite; ac_jump eas_Idle`|
|410/M|P|`moveLeft 1`|
|413/M!|A|`moveUp 1`|
|418/C!|P|`ac_setSpeed 48,48; ac_jump eas_Idle`|
|422/C!|A|`ac_setSpeed 48,48; ac_jump eas_Idle`|
|432/M|P|`moveRight 4`|
|435/M|A|`moveDownRight 4`|
|444/C!|P|`ac_motion OFF; ac_orientRight; ac_updateSprite; ac_jump eas_Idle`|
|451/C!|A|`ac_motion OFF; ac_orientDown; ac_updateSprite; ac_jump eas_Idle`|
|459/C!|B|`ac_setSpeed 64,64; ac_jump eas_Idle`|
|464/M!|B|`moveUp 1`|
|472/M|129|`moveRight 2`|
|478/C!|B|`ac_setSpeed 48,48; ac_jump eas_Idle`|
|486/M|B|`moveDown 5`|
|495/C!|B|`ac_motion OFF; ac_orientDown; ac_updateSprite; ac_jump eas_Idle`|
|518/C!|G|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|522/M!|G|`moveRight 1`|
|529/C!|128|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|533/M!|128|`moveUp 3; moveUpLeft 1`|
|551/C!|L|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|555/M!|L|`moveUp 1`|
|560/M!|L|`moveUp 1; faceRight 1`|
|574/M!|L|`moveRight 1`|
|585/M|L|`moveLeft 3`|
|606/C!|128|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|610/M!|128|`moveUp 3; moveUpLeft 3`|
|621/M!|L|`moveRight 1`|
|633/M!|L|`moveLeft 1`|
|637/M!|L|`moveRight 2`|
|650/M!|L|`moveRight 1`|
|653/M|L|`moveRight 1`|
|661/C!|G|`ac_setSpeed 5,5; ac_jump eas_Idle`|
|665/C!|L|`ac_setSpeed 5,5; ac_jump eas_Idle`|
|669/M|G|`moveLeft 1`|
|672/M|L|`moveLeft 1`|
|685/M|L|`moveUpRight 1`|
|690/M|G|`moveUp 3`|
|693/M|L|`moveUp 3`|
|742/M!|A|`moveRight 1; faceUp 1`|
|755/M!|P|`moveRight 1; moveDown 4`|
|759/M|A|`moveDown 4; moveLeft 7`|
|763/M!|P|`moveDown 5; moveLeft 6`|
|781/M|A|`moveUp 2`|
|784/M!|P|`moveLeft 1; moveUp 1`|
|790/M|B|`moveUp 5; faceRight 1`|
|794/M|A|`moveUp 5; moveRight 2; moveUp 1; faceLeft 1`|
|800/M!|P|`moveUp 6; moveRight 1; faceUp 1`|
|818/M!|P|`moveRight 1; faceUp 1`|
|828/C!|A|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|833/M|A|`moveLeft 1`|
|841/M|A|`moveRight 1`|
|873/M!|A|`moveUp 1`|
|881/M!|P|`moveUp 1`|
|897/C!|P|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|901/M!|P|`moveDown 1`|
|908/M!|P|`moveLeft 3; moveUp 3; faceRight 1`|
|930/M!|A|`moveLeft 1; moveUp 1; faceLeft 1`|
|949/M!|M|`moveDown 1`|
|956/C!|M|`ac_setSpeed 10,10; ac_jump eas_Idle`|
|960/M|M|`moveDown 1`|
|966/M|M|`moveUp 1`|
|976/M!|M|`moveRight 3`|
|997/M!|A|`moveDown 1; faceRight 1`|
|1013/M!|A|`moveRight 1`|
|1028/M!|P|`moveRight 1`|
|1032/C!|A|`ac_setSpeed 16,16; ac_jump eas_Idle`|
|1036/M!|A|`moveLeft 1; faceDown 10`|
|1042/M!|P|`moveRight 1; faceDown 1; jumpUp 1; jumpUp 10`|
|1054/M!|A|`moveLeft 1; faceDown 1`|
|1064/M|B|`moveUp 1`|
|1069/M!|B|`moveRight 1; faceDown 20`|
|1085/M|B|`moveDown 1`|
|1090/M!|P|`moveDown 1`|
|1094/M!|A|`moveDown 1; faceRight 1`|
|1104/M|B|`moveDown 1; moveLeft 1; moveDown 10`|

## Reproduction, failures and open boundary

Ignored `local/issue1184/analyze.py` inventories selected pinned source
and retained pair without lowering; `observe.py` made the sole selected
lowering retained in observe-01.json. **Do not rerun observation** during
ordinary review. `review.py` reuses both, checks source/corpus order and
inline payloads, dialogue IDs/modes/speakers/tokens, selected joins,
source/consumer state examples, links/UTF8/LF/private ownership and
retained unsupported output. These local review aids are not maintained
parsers, tool tests, schemas or durable evidence owners; this tracked
account plus exact Git objects and named source own the findings.

The original author/main direct checks and CI39821008118 passed while
missing two P2 prose defects identified in
[independent review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1185#issuecomment-6346897412):
installation was incorrectly described as clearing the timer, and the
predecessor described CURSE preservation without the equipment refresh.
Both findings and original results remain preserved. The old review.py
navigation-only predecessor predicate no longer applies to the authorized
factual correction; do not rerun analysis, observation or that full recipe.
The correction recipe directly checks the three installation use sites,
slot-resolution/jump helpers, healing masks/equipment refresh, finite
supplied states, corrected prose/links and the exact expanded diff.

For the corrected candidate, from the assigned checkout with a fresh
nonexistent receipt path:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1184/correction-review.py <configured-source-disasm-root> --receipt local/issue1184/correction-reproduction-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Source argument ends in disasm. Initial analysis and observation commands,
fresh receipt identities, actual durations/bytes, base/head/tree, planner,
exact-head scoped CI and process state belong in the frozen Issue/PR and
ignored handoff. No new ROM verifier was required for a blocker claim;
there was no missing-input claim or new ROM observation. Normal/full,
SDK/Godot/emulator/H3/H4, old suites and screenshots are **SKIPPED** by
documentation scope, not passed. ADR0004's applicable provenance/date,
width/order/polarity, caller/alias, changed-state, actual-consumer/private
checks apply; new maintained schemas/parsers and H3 callbacks are N/A.

Preserve [predecessor failures](./battle43-outcomes-and-ending-admission.md#reproduction-retained-failures-and-open-boundary):
its LF/link failures and corrections, after43's68 instructions/23 native
operations, reader leader/defeated-program refusal and queued-warp ordering;
#1180 KeyError98/correction; #1176 entity/position corrections; #1172
status/publication corrections; after42 special251/five-op partial,
Map54 overwrite, Map53 failed import/zero partial and twelve completed
old comparison failures. No rerun, green replacement or repair queue.
This slice's initial analysis incorrectly expected cs_62658 to be a second
corpus program and failed; corrected to the actual embedded-label shape,
preserving the failed command. Orientation quoting/glob/missing-filename
read failures remain in local handoff; they made no source changes.
Direct review01 failed because its inline-jump guard compared normalized
spacing to raw source; normalize that selected block, retain direct-01.json
and use fresh review02. This correction changes no source claim or producer.
Review03 then rejected the edited research prose as source/consumer drift;
restrict that immutable comparison to source/code consumers while checking
the owned prose afresh. Retain direct-03.json; no snapshot is overwritten.

**Unknown:** natural entrance/zone reach, actual party/flags/HP/status/
inventory/debug mode, intermediate services and interruption states,
source-ROM equality, layout contents/geometry, rendered/audio timing,
portrait/helper completion, actual player input, saving/persistence,
clock advancement, full sealing/ending/credits/kiss completion and
original/remake playability. Static completeness is not natural execution.
Freeze clean pushed Draft PR for independent main-gate acceptance; same
task owns corrections. No merge, next dispatch, cleanup, ref deletion or
archival is authorized.

Continue with [Granseal epilogue interactions and Castle2F admission](./granseal-epilogue-and-castle-admission.md) for the town events, Astral choice and escort, and the next castle-entry boundary.
