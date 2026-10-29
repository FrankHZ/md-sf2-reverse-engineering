# Evil Spirit consultation and conditional zone return

Evidence date: 2026-10-14. **Confirmed:** the complete Map34 `cs_5B6C0`
program stages a consultation, changes the special-sprite graphics, drives
sixteen separate effect actors, hides the spirit and attendants, and requests
two follower relations. Only after script return does its zone caller set
F784, then F802; the wrapper subsequently waits for physical0's coordinates.
**Inferred:** this is the Evil Spirit consultation interrupted by Zeon and
followed by the spirit's explosion. **Unknown:** natural reach, actual supplied
state, completed services, rendered appearance, usable control and persistence.
The script's dialogue is evidence of what characters say, not of transactions
or independently established world facts.

## Agreement and provenance

Standard rigor under [Issue #949](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one complete scene outcome through its immediate zone return. The sole owned
path is this document. Accepted dispatch base is
`c6bbc1c11ea52d9a0828abf2280e9ff125bfaf91`. Consume accepted
[shrine entry and population](./evil-spirit-shrine-entry-and-scene-admission.md)
and [restoration/shared action semantics](./creed-restoration-and-basement-departure.md).
Accepted #950 synthesis is disjoint and is not an evidence dependency.
No shared index, fixture, schema, tooling, engine, translation, runtime,
acquisition or environment changes. Stop before subsequent walked return,
reentry, mansion recruitment or Oddler departure.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only source is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Scene lines mean `data/maps/entries/map34/mapsetups/scripts.asm`; the complete
program is lines4–580, `$5B6C0..$5BFD0` (half-open). Its three helpers occupy
`$5BFD0..$5BFEE`. Map-engine and entity-engine filenames below live under
`code/common/scripting/map/` and `code/common/scripting/entity/`; shared actions
are in `data/scripting/entity/eas_actions.asm`. Constants resolve through
`sf2enums.asm` and RAM symbols through `sf2const.asm`.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair with the
existing consumers. This is linear retained comparison plus bounded source,
fixture and text joins: budget under one minute/512 MiB; output only selected
rows, this prose and small receipts. No regeneration, renderer, comparator
framework, transcript publication or network acquisition. Reassess a missing
claim or material consumer expansion before enlarging the work. Aggregate
fixture equality supports, but does not replace, the direct semantic joins.

## Actor and admission premises

**Confirmed:** the accepted entry owner establishes fresh F784-clear Map34's
21 resident records and the special allocation. C=logical128 Creed, F=129 Fairy,
E=130 Evil Spirit, P=ALLY_PETER7, B=FOLLOWER_B31, and Bowie=ALLY_BOWIE0 below.
These are logical lookups, not interchangeable physical numbers.

| Actor | Baseline mapsprite | Lookup offset / allocated physical slot |
| --- | --- | --- |
| C128 | CREED212 | 32 / N+1 |
| F129 | FAIRY219 | 33 / N+2 |
| E130 | EVIL_SPIRIT252 | 34 / special47, stored ENTNUM32 |
| Dialogue131 | BEASTMAN2=222 | 35 / N+3 |
| Unused dialogue-capable132 | BOY200 | 36 / N+4 |
| Effects133–148 | EFFECT1=63 each | 37–52 / N+5 through N+20 |

N counts the ordinary follower records allocated before residents. Under only
F67/F68, N=2: P is physical1, B/Oddler physical2, C/F physical3/4,
131/132 physical5/6, effects physical7–22. E remains physical47, not physical5
or logical32. No scene command creates any of these actors, changes the resident
mapsprite IDs, or moves/speaks as132. The Zeon text selects131, still its declared
BEASTMAN2 sprite; the special-graphics helper independently updates shared
special-sprite tiles. It does not turn131 into a new physical Zeon actor.

F67/F68 are premises, not local tests. B can instead resolve to a later
flag-selected Astral row; repeated B allocations overwrite that lookup.
Absent follower/resident entries can resolve to physical0 through zero mappings.
A hide invalidates every matching lookup to -1; later resolution has no local
validity repair. Fresh F784-set entry has no residents; changing F784 on a
retained population does not rebuild it. Excessive follower/raft populations,
HP/class choices and aliasing retain the accepted entry owner's limitations.
No newly spawned or correctly populated scene may be assumed from admission.

The destination-derived type6 request must select the wildcard-X/Y-low-byte13
zone row. `Map34_ZoneEvent0` checks only F784: set skips the script and both
caller writes; clear calls it even if F783 is clear or F802 is already set.
The request consumer clears MAP_EVENT_TYPE before dispatch and applies Init
to physical0 before this gate. Those operations do not prove a natural request
or successful arrival. The following nominal choreography assumes distinct
baseline mappings and that the relevant commands/services actually return.

## Complete ordered scene

**Confirmed:** retained `programCorpus.programs[id=cs_5B6C0]` has 409 top-level
records, indexed0–408, with the exact source lines, macro arguments and csc-end
termination. The partition below covers every record once, including sleeps;
inline `entityActions` and custom-action bodies are included in their rows and
expanded in the movement/effects sections. Source lines are inclusive.

A suffix `!` means the source uses a waited action installation; an unmarked
action is asynchronous. U/D/L/R are Up/Down/Left/Right. Position tuples are
absolute tiles; movement operands are relative tiles. Text IDs refer to the
complete table below. Pn and Sn denote only the palette/special helper calls
defined later, not measured presentation events.

| Scene lines / records | Ordered operations and semantic phase |
| --- | --- |
| 4–24 / 0–18 | Set text cursor3207; camera(2,4); C3207–3208; flash C100. Three E appearances at(7,6) Down: each sleep7 then park(60,60) Down, with sleeps80,60,40 respectively. F U1!; F3209. |
| 25–50 / 19–44 | Camera(2,2). Six more E appearance/park pairs, each sleep7 then sleep60,50,40,30,20,10 respectively. Finally place E(7,6) Down without another park. |
| 51–71 / 45–63 | E Transparent asynchronously; Init! Bowie, P, B in that order; place Bowie(7,12), P(6,12), B(8,12), all Up. Camera(2,4); C U1!; text3210–3213; nod C; text3214–3218. |
| 72–105 / 64–91 | Sleep40/flash4, sleep30/flash4, sleep20/flash4; map fade out to white; sleep30; E3219. Bowie U1, P U1, B U1!; P3220. F Down, F3221, F Up; map fade in from white; sleep40; F Down, F3222, F Up. C Jump! twice; C3223, E3224; shiver C; C3225, E3226. |
| 106–125 / 92–109 | Camera(2,5); P Right, Bowie Left, B Left; P3227. C/F Down; C Jump! twice; C3228–3229. Bowie/P/B Up; P3230, C3231; C D1!; C3232. |
| 126–151 / 110–131 | C DeactivateAutoFacing!; C D1, sleep2, StopMoving!, stopEntity C, sleep20; flash Bowie40; startEntity C; C U1, sleep2, StopMoving!; headshake C; C3233–3234, P3235; shiver C; C3236–3239; nod Bowie; sleep20. |
| 152–179 / 132–154 | Camera(2,4); C Init!; C R1,U1 asynchronously; Bowie U2!; F Up; unattributed3240; flash Bowie40; E3241. P1/sleep20/P0/sleep40; P1/sleep30/P0/sleep30; P1/sleep40/P0/sleep20; P1; E3242; sleep100. |
| 180–196 / 155–171 | Text3243–3250; C Jump! twice; text3251–3257. The two contiguous C nextText segments3254–3255 form one continuing statement. |
| 197–221 / 172–196 | S1/sleep10/S0, C3258; S1/sleep10/S0, E3259; S1/sleep10/S0/sleep10/S1; flash40; S2. Two ordered rounds install Jump on Bowie,P,B,C, then Jump! on F. Only each F installation is waited. |
| 222–236 / 197–209 | Text3260–3263; shiver C; text3264–3267; P U1!; text3268–3270. |
| 237–256 / 210–229 | Sleep40/flash4/S1/sleep8/S2; sleep60/flash4/S1/sleep8/S2; sleep40/flash4/S1/sleep8/S2; sleep20/flash4/S1. Set quake5; request SFX_BIG_DOOR_RUMBLE. |
| 257–320 / 230–245 | For each logical133 through148 in ascending order, install/wait a custom action: setSpeed64,64 then jump eas_Idle; ac_end terminates the inline body. |
| 321–356 / 246–273 | Place141–144 at the four inner starting points, Right; install their four clockwise entries in order. Place145–148 at the outer starting points, Left; for each in order issue R1/D1/L1/U1 respectively, sleep2 and StopMoving!; install their four counterclockwise entries. Full operands below. |
| 357–390 / 274–291 | Death SFX; first eight-ray burst from(7,6), all Right, for133–140; only140's diagonal is waited. Then death SFX for the next burst. |
| 391–423 / 292–308 | Second identical eight-ray placement/action burst; death SFX. |
| 424–458 / 309–327 | Third identical burst; hide E; death SFX then big-door-rumble SFX. |
| 459–491 / 328–344 | Fourth identical burst; death SFX. |
| 492–540 / 345–377 | Fifth identical burst; hide133–140 in order; sleep40; hide141–148 in order. |
| 541–557 / 378–394 | Quake $4005 (descending ramp); P0; sleep60; Bowie/F/C Down; C3271–3273. F Right, C Left; F3274, C3275; C Down; C3276–3277; sleep30. |
| 558–580 / 395–408 | C D1,L1,D5! then hide C; F L1,D1!; P/B Down; F3278; F Jump! twice; F3279; F D5! then hide F. Request P→Bowie direction2, then B→P direction2. csc_end. |

The screen flashes before consultation are three calls4; the interruption uses
one call40; its aftermath uses four calls4. The entity flashes are C100 and
two separate Bowie40 calls. Literal sleeps, flashes and speed operands are not
wall-clock durations or a guarantee that concurrent actions completed.

## Text identity, speaker and continuation state

**Confirmed:** the 73 source-comment literals exactly join cursor3207–3279 to
`data/scripting/text/gamescript.txt` keys `0C87`–`0CCF`, preserving spaces and
controls. S means nextSingleText; T means nextText. C uses modifier$80, all
F/E/P/131 uses modifier0, and none uses `$FF,ENTITY_NONE` ($FFFF).
The summaries paraphrase the transcript; the ordered controls are exact.

| ID / source line | Form / speaker | Content | Ordered controls |
| --- | --- | --- | --- |
| 3207 / 6 | T/C | Invocation | `{N}{N}{W2}` |
| 3208 / 7 | S/C | Calls the spirit | `{W1}` |
| 3209 / 24 | S/F | Fairy announces its appearance | `{N}{W1}` |
| 3210 / 62 | S/C | Creed reacts | `{W1}` |
| 3211 / 63 | S/E | Spirit asks Creed's purpose | `{W1}` |
| 3212 / 64 | S/C | Creed names Zeon | `{W1}` |
| 3213 / 65 | S/E | Spirit identifies the devil king | `{N}{W1}` |
| 3214 / 67 | T/C | Creed agrees | `{W2}` |
| 3215 / 68 | T/C | Says Zeon was unsealed | `{W1}` |
| 3216 / 69 | T/C | Asks Zeon's current and intended acts | `{N}{W2}` |
| 3217 / 70 | S/C | Requests any information | `{N}{W1}` |
| 3218 / 71 | T/E | Spirit claims contact | `{W1}` |
| 3219 / 80 | S/E | Spirit concentrates | `{N}{W1}` |
| 3220 / 90 | S/P | Peter asks what it is doing | `{W1}` |
| 3221 / 92 | T/F | Fairy explains its channeling | `{N}{N}{W2}` |
| 3222 / 97 | S/F | Fairy requests silence | `{N}{W1}` |
| 3223 / 101 | S/C | Creed asks for findings | `{W1}` |
| 3224 / 102 | S/E | Spirit reports a barrier hiding Grans | `{N}{N}{W1}` |
| 3225 / 104 | S/C | Creed asks it to retry | `{W1}` |
| 3226 / 105 | S/E | Spirit requests an object associated with Zeon | `{N}{N}{W1}` |
| 3227 / 110 | S/P | Peter mentions the leader's Jewel of Evil | `{LEADER}{N}{W1}` |
| 3228 / 115 | T/C | Creed asks for confirmation | `{LEADER}{W2}` |
| 3229 / 116 | S/C | Identifies a sealing jewel and requests it | `{N}{N}{W1}` |
| 3230 / 120 | S/P | Peter says it cannot be removed | `{W1}` |
| 3231 / 121 | S/C | Creed claims he can remove it | `{W1}` |
| 3232 / 125 | S/C | Creed asks to inspect it | `{W1}` |
| 3233 / 142 | T/C | Creed exclaims | `{W2}` |
| 3234 / 143 | S/C | Sees two lights | `{N}{W1}` |
| 3235 / 144 | S/P | Peter names the Jewel of Light | `{N}{N}{W1}` |
| 3236 / 146 | T/C | Creed reacts | `{W2}` |
| 3237 / 147 | T/C | Asks whether the leader has both jewels | `{LEADER}{N}{W2}` |
| 3238 / 148 | T/C | Says the Mithril chain prevents removal | `{N}{W2}` |
| 3239 / 149 | S/C | Asks the leader to show them to the spirit | `{N}{W1}` |
| 3240 / 162 | S/none | Narrates the leader displaying the jewels | `{LEADER}{N}{W1}` |
| 3241 / 164 | T/E | Spirit claims improved contact | `{N}{N}{W1}` |
| 3242 / 178 | S/E | Spirit concentrates again | `{N}{W1}` |
| 3243 / 180 | S/E | Reports Arc Valley opened and Zeon revived | `{N}{W1}` |
| 3244 / 181 | S/C | Creed asks it to continue | `{W1}` |
| 3245 / 182 | S/E | Reports devils approaching Parmecia by air | `{N}{W1}` |
| 3246 / 183 | T/C | Creed considers the report | `{W2}` |
| 3247 / 184 | S/C | Asks their destination | `{W1}` |
| 3248 / 185 | S/E | Names North Parmecia | `{W1}` |
| 3249 / 186 | S/C | Creed suspects an event involving Mitula | `{N}{W1}` |
| 3250 / 187 | S/E | Spirit says it cannot see Zeon | `{W1}` |
| 3251 / 190 | S/C | Creed asks what that means | `{W1}` |
| 3252 / 191 | T/E | Spirit places Zeon in Arc Valley | `{W2}` |
| 3253 / 192 | S/E | Says his strength has not fully recovered | `{N}{N}{W1}` |
| 3254 / 193 | T/C | First segment of Creed's statement about mischief | `{N}` |
| 3255 / 194 | T/C | Continues that statement | `{N}{W2}` |
| 3256 / 195 | S/C | Creed asks about his power | `{W1}` |
| 3257 / 196 | S/E | Spirit begins an explanation | `{N}{W1}` |
| 3258 / 200 | S/C | Creed asks it to finish | `{W1}` |
| 3259 / 204 | S/E | Spirit's broken reply | `{W1}` |
| 3260 / 222 | S/131 | Voice identifies itself as Zeon | `{N}{W1}` |
| 3261 / 223 | S/C | Creed reacts | `{W1}` |
| 3262 / 224 | T/131 | Voice threatens the caller | `{N}{W1}` |
| 3263 / 225 | S/131 | Voice recognizes the Jewel of Evil | `{N}{W1}` |
| 3264 / 227 | T/C | Creed says Zeon sees through the spirit | `{N}{W2}` |
| 3265 / 228 | S/C | Creed calls it impossible | `{W1}` |
| 3266 / 229 | T/131 | Voice identifies the leader | `{LEADER}{N}{W2}` |
| 3267 / 230 | S/131 | Demands the jewel at Arc Valley | `{LEADER}{N}{N}{W1}` |
| 3268 / 234 | S/P | Peter objects | `{W1}` |
| 3269 / 235 | T/131 | Offers Elis in exchange | `{N}{W2}` |
| 3270 / 236 | S/131 | Repeats the destination | `{N}{W1}` |
| 3271 / 547 | T/C | Creed says the spirit exploded | `{W2}` |
| 3272 / 548 | T/C | Attributes it to Zeon's power | `{W2}` |
| 3273 / 549 | S/C | Warns about full revival | `{N}{N}{W1}` |
| 3274 / 552 | S/F | Fairy asks about the spirit's reliability | `{N}{W1}` |
| 3275 / 553 | T/C | Creed affirms it | `{W2}` |
| 3276 / 555 | T/C | Asks why Zeon wants the jewel | `{W2}` |
| 3277 / 556 | S/C | Says the spirit can no longer answer | `{N}{W1}` |
| 3278 / 570 | T/F | Fairy calls after Creed | `{W2}` |
| 3279 / 573 | S/F | Fairy asks him to wait | `{N}{W1}` |

`mapscriptengine_2.asm:csc04` writes CUTSCENE_DIALOG_INDEX. csc00/csc02
wait for view scrolling, select speech state, call DisplayText and increment
the cursor. S then closes portrait/text and sleeps10; T leaves it open.
Therefore3215,3218,3241 and3262 are T even though their text ends W1.
3254 ends only `{N}`: it has no input wait or closing marker;3255 continues it.
No omitted W1/W2 or invented close should be inserted between those segments.
With every nominal text call executed, the cursor ends at3280.

The accepted text consumer `code/common/scripting/text/textfunctions_1.asm`
interprets N as line break and W1/W2 as input waits; W2 uses the continuation
arrow/validation-sound path. They are not a choice menu. LEADER uses UpdateForce
and the first battle-party member's name. The narrated display3240 and the nod
command address different authorities: text substitutes that current leader;
the physical gesture addresses Bowie. Arbitrary supplied membership can differ.

`getentityportaitandspeechsfx.asm` resolves the mapped entity's actual mapsprite
against `data/spritedialogproperties.asm`. Baseline C gets portrait CREED and
DIALOG_BLEEP_8; F gets NONE/bleep1, E NONE/bleep8,131 NONE/bleep6,132 NONE/bleep1.
P's base/promoted sprites select the corresponding Peter portrait and bleep4/5;
HP-derived or otherwise changed sprites can select something else.132 has no
selected text. No Zeon portrait is requested by choosing131. In csc1D and
`code/common/menus/portraitwindow.asm:OpenPortraitWindow`, $80 requests the
right-side unmirrored portrait,0 sets neither modifier bit; an already open
portrait or NONE result can suppress opening. The `$FFFF` narration bypasses
actor lookup and writes speech SFX0. These are requested speaker/portrait states,
not proof that the expected portrait or speech was rendered.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry. Debug mode plus P2 Start
can latch it: csc00/csc02 then skip display and cursor advancement, and dispatcher
csWait sleeps are skipped. The latch does not skip the other action/helper
commands or their internal waits. Thus the table is the nominal cursor path,
not proof of73 displayed lines or enough elapsed time for asynchronous motion.

**Inferred:** the narration and staging depict jewel inspection, remote contact,
Zeon's demand and an explosion. **Confirmed:** there is no inventory transfer,
jewel removal, Elis return, consent/recruitment, battle/warp, healing, joined-flag
or story-flag command inside this complete program. The claims about the chain,
barrier, Mitula, Zeon's power/location and the proposed exchange are dialogue.
They do not implement those services or independently establish their truth.

## Actions, placement and completion

**Confirmed:** reuse the accepted immediate consumers, with these scene-specific
use sites. csc19 setPos writes both current/destination coordinates from unsigned
byte tiles scaled by384, facing, and a sprite-update request. csc23 setFacing
writes facing and requests an update. Their HP guard skips ally IDs below30
with HP0; residents and B31 bypass it. P/B/Bowie Init and action installations,
entity flashes, nod/headshake/shiver have no corresponding HP skip. The selected
startEntity/stopEntity word operand128 also bypasses that guard.

`stopEntity` means csc1C writes animation counter -1; `startEntity` means csc1B
writes0. Neither command is the movement-stopping action. `eas_StopMoving`
is exactly `ac_moveRel 0,0; ac_jump eas_Idle`: it retargets from the actor's
then-current coordinates via esc04, subject to destination-conflict handling.
It does not complete the replaced D1/U1 or guarantee a whole-tile endpoint.
C's short approaches and all four outer-ring offsets must retain this distinction.

The complete selected shared action bodies are:

| Action / source lines | Complete reachable body |
| --- | --- |
| eas_Jump / 4–11 | Disable autofacing; moveRel(0,-1); wait5; moveRel(0,1); wait5; moveRel(0,0); enable autofacing; jump eas_Idle. The move offsets use current coordinates at each execution. |
| eas_Transparent / 82–83 | Set transparency ON; jump eas_Idle. esc1C sets ENTITYDEF_FLAGS_B_BLINKING; no opacity interpolation or sprite replacement occurs in this action. |
| eas_Init / 168–180 | Factors0/0; speed32/32; acceleration OFF/OFF; deceleration OFF/OFF; entity obstructable ON; entity uncollidable OFF; map uncollidable OFF; autofacing ON; orientUp; immersion OFF; shared size24; sprite update; jump eas_Idle. It does not reset coordinates or generally clear every flag. |
| eas_DeactivateAutoFacing / 210–212 | Autofacing OFF; jump eas_Idle. Subsequent unrelated bytes after the jump are not part of its execution. |
| eas_StopMoving / 223–224 | moveRel(0,0); jump eas_Idle. |
| Each scene custom body / 257–320 | For one actor133–148: setSpeed64,64; jump eas_Idle. ac_end ($8080) is the map interpreter's inline-body delimiter, not a further entity instruction. No reset of motion, autofacing, collision, transparency or position is included. |
| eas_Idle / eas_main.asm | wait1; branch back. Idle-pointer equality is the waited-installation sentinel, not termination of all entity processing. |

csc14/csc15 seed the wait timer with the resolved physical index and replace
that actor's action pointer. Their waited forms poll that pointer for eas_Idle
without a timeout. csc2D also seeds the timer, clears flags A bits5/6 with `$9F`,
compiles each direction into signed relative movement plus waitDest in action
work RAM, appends an idle jump and optionally polls only that actor. For example,
B U1! does not wait for Bowie/P U1, and140's burst wait does not join133–139.
Waiting for F's Jump twice does not join the other four Jump installations.
A second installation can replace unfinished work, especially with aliases,
debug-skip sleeps, or delayed entity service.

`entityscriptengine_2.asm:esc04` multiplies signed deltas by384 and adds actual
current coordinates. HasSameDestinationAsOtherEntity can defer advancement.
esc01 waits for current X/Y to equal destination; esc10 writes the two speed
bytes. `esc00_wait` uses the per-entity byte timer. These source operations do
not establish exact velocities, service timing or completion under arbitrary
supplied actors. The fixed-sprite initialization inherited by effects disables
autofacing and motion animation; speed64 does not undo that whole initial body.
The scene issues positions/facings separately.

All inline non-effect paths are included above: F U1; C U1; Bowie/P/B U1;
C D1!, then the interrupted D1/U1; C R1,U1 concurrent with Bowie U2!; P U1!;
C D1,L1,D5!; F L1,D1!, then D5!. Under distinct living mappings, placements and
completed relative moves, and net-zero gesture displacement, Bowie reaches(7,9),
P reaches(6,10), and B reaches(8,11) before the last follower requests. These are conditional path calculations;
Jump/partial moves, omitted HP-guarded placements or other supplied state are
not silently converted to those endpoints. C's partial approach prevents an
unconditional integer-tile departure start; hiding later supersedes its placement.

Camera requests in source order are(2,4),(2,2),(2,4),(2,5),(2,4).
csc32 sets VIEW_TARGET_ENTITY=-1, calls SetCameraDestination (tile scaling384)
and WaitForViewScrollEnd. No scene command restores camera following.
A completed camera wait is not a player-control handoff.

## Sixteen effect trajectories and hides

**Confirmed:** these are sixteen ordinary EFFECT1 actors already allocated
by the fresh baseline. They are distinct from E's special47 and from131/132.
All receive speed64/64 through separate waited custom actions before placement.
`eas_actions.asm:446–473` supplies eight labels into two cyclic programs.
The labels are retained verbatim below because, for example,
`eas_ClockwiseMoveUpRight` actually starts with moveRel(-1,+1).

Each orbit instruction is moveRel followed by waitDest; labels fall through
to the next pair, and the final ac_branch loops to the first label. There is
no idle exit. Listed cyclic deltas start at the selected label, wrap at the
branch, and repeat. Do not reinterpret the operands from the English names.

| Logical actor / placement facing | Selected action entry | Ordered repeating deltas |
| --- | --- | --- |
| 141 / (8,6) Right | eas_ClockwiseMoveUpLeft | (-1,-1),(-1,+1),(+1,+1),(+1,-1) |
| 142 / (7,5) Right | eas_ClockwiseMoveUpRight | (-1,+1),(+1,+1),(+1,-1),(-1,-1) |
| 143 / (6,6) Right | eas_ClockwiseMoveDownRight | (+1,+1),(+1,-1),(-1,-1),(-1,+1) |
| 144 / (7,7) Right | eas_ClockwiseMoveDownLeft | (+1,-1),(-1,-1),(-1,+1),(+1,+1) |
| 145 / (8,6) Left; R1/sleep2/StopMoving! first | eas_CounterClockwiseMoveUpRight | (-1,+1),(-1,-1),(+1,-1),(+1,+1) |
| 146 / (7,7) Left; D1/sleep2/StopMoving! first | eas_CounterClockwiseMoveUpLeft | (-1,-1),(+1,-1),(+1,+1),(-1,+1) |
| 147 / (6,6) Left; L1/sleep2/StopMoving! first | eas_CounterClockwiseMoveDownLeft | (+1,-1),(+1,+1),(-1,+1),(-1,-1) |
| 148 / (7,5) Left; U1/sleep2/StopMoving! first | eas_CounterClockwiseMoveDownRight | (+1,+1),(-1,+1),(-1,-1),(+1,-1) |

With completed inner moves,141's successive destinations are(7,5),(6,6),(7,7),
(8,6), and the other inner actors enter at different phases of that diamond.
Outer trajectories start from each actual stopped position Q, not an assumed
one-tile displacement: add its listed deltas successively to Q. There is no
completion join between the four looping actors; neither nominal ring is
proof of a measured radius, stable synchronization or rendered circular motion.

The other eight actors are reset to(7,6), Right before **each of five** bursts.
The complete identical ray assignment is:

| Logical actor | Relative action | Nominal destination if completed | Waited? |
| --- | --- | --- | --- |
| 133 | Right4 = (+4,0) | (11,6) | No |
| 134 | Up4 = (0,-4) | (7,2) | No |
| 135 | Left4 = (-4,0) | (3,6) | No |
| 136 | Down4 = (0,+4) | (7,10) | No |
| 137 | UpRight4 = (+4,-4) | (11,2) | No |
| 138 | UpLeft4 = (-4,-4) | (3,2) | No |
| 139 | DownLeft4 = (-4,+4) | (3,10) | No |
| 140 | DownRight4 = (+4,+4) | (11,10) | Yes, this actor only |

Burst starts are lines358,391,424,459,492; the five action groups start366,
399,432,467,500 and end389,422,455,490,523. Repositioning and replacement can
interrupt the preceding unjoined rays. Death SFX occurs before each burst;
E is hidden after burst3 and before burst4, with the second rumble request.
After burst5,133–140 hide in order; sleep40;141–148 hide in order. None is a
combatant death/experience transaction, and there is no `eas_Die` here.

csc2E calls `entityfunctions_2.asm:HideEntity`: it writes current/destination
`$7000,$7000`, changes every matching lookup byte to -1, and removes the
physical actor from EXPLORATION_ENTITIES if present. It does **not** clear
its action pointer, wait for its action, erase its sprite/flags, or deallocate
its entity record. In particular141–148 retain cyclic program pointers, but
`entityscriptengine_2.asm:VInt_UpdateEntities/loc_4EF4` skips a slot whose
current X is at least $7000 before UpdateEntityData or action dispatch. Thus
ordinary subsequent service leaves these hidden slots dormant while that X
remains; the retained orbit pointers do not themselves restart motion. Hide
is not an idle-pointer completion join or a reset of the retained action.
Rendered absence and persistence across later reactivation remain unobserved.
Under aliases, one hide can invalidate multiple logical IDs and undermine subsequent commands. No local repair occurs.

## Special graphics, palettes and presentation services

**Confirmed:** the three complete local helpers each load d0, call the
`s05_jumpinterface.asm:j_AnimateSpecialSprite` alias, and return:

| Shorthand / source | d0 / immediate selection |
| --- | --- |
| S0 = csub_5BFD0, lines585–589 / $5BFD0 | 0: SpecialSprite_EvilSpirit+$20 |
| S1 = sub_5BFDA, lines597–601 / $5BFDA | 1: SpecialSprite_EvilSpiritAlt |
| S2 = sub_5BFE4, lines609–613 / $5BFE4 | 2: SpecialSprite_Zeon+$20 |

`code/common/tech/graphics/specialsprites.asm:AnimateSpecialSprite` saves
and restores d0–d2/a0–a1. It selects the source, decompresses through
LoadStackCompressedData into FF6802_LOADING_SPACE (`$FF6802`), queues
ApplyVIntVramDma from that buffer to VRAM `$AF00` with d0=$480 and increment2,
then calls EnableDmaQueueProcessing. The DMA consumer encodes a word count
and advances pointers by twice d0: $480 words = $900 bytes, destination interval
`$AF00..$B800`. EnableDmaQueueProcessing only sets DMA_REQUEST; unlike the
separate WaitForDmaQueueProcessing it does not itself wait for a VInt.
The helper is a graphics-data/decompression/DMA request, not entity creation,
positioning, mapsprite-ID replacement or guaranteed displayed completion.

The full selected S sequence is
`1,0,1,0,1,0,1,2,1,2,1,2,1,2,1`. The last request is S1 at line254;
there is no final S0 or S2 restore. E's later hide is a separate operation.
No general special-sprite renderer audit is needed to establish those requests;
actual graphics/occlusion/transparency and DMA consumption remain Unknown.

P1=`data/maps/entries/map07/mapsetups/scripts.asm:csub_55EF4` ($55EF4),
P0=`sub_55F82` ($55F82). They are calls into shared source, not a transition to
Map07. P1 traverses64 current palette words starting PALETTE_1_CURRENT ($FFD000),
writing PALETTE_1_BASE ($FFD080). For each word c it computes
`s=((c & $E00)>>9)+(c & $E)+((c & $E0)>>3)`, divides by7, and increments the
quotient when the remainder is at least4. Call this rounded quotient q.
The three eight-byte sequences at palette_55F6A are:

- T0 = `[14,12,10,8,8,6,4,2]`;
- T1 = `[14,12,12,10,8,6,4,2]`;
- T2 = `[0,2,2,4,6,8,10,10]`.

The written word is `(T2[q]<<8) | (T0[7-q]<<4) | T1[7-q]`.
This is a concrete color remap of the current palette, not an unproved named
filter. The helper clears FADING_TIMER_BYTE ($FFDFAB), then tail-calls
UpdateBasePalettesAndBackupCurrent with a0=BASE. P0 instead clears that byte
and supplies PALETTE_1_COPY ($FFDF2A) to the same consumer.

`code/common/tech/graphics/updatefadingpalette.asm` copies CRAM_SIZE128 bytes
from a0 to BASE (self-copy for P1), copies CURRENT to COPY, and writes byte32
at FADING_TIMER_WORD ($FFDFAA). P0 therefore restores its selected COPY snapshot
to BASE **before replacing COPY with the then-current palette**. It does not
promise the original room palette, reload a map palette, or synchronously finish
the transition. P1/P0 can act on different intermediate snapshots.

The immediate UpdateFadingPalette consumer returns when the timer is zero;
otherwise it decrements it, computes `t=timer>>2`, and interpolates each
component as `(COPY*t + BASE*(8-t))>>3`, writing CURRENT for64 colors and
requesting ApplyVIntCramDma. At zero it only requests a return transition if
FADING_TIMER_BYTE is nonzero; these helpers clear that byte. This establishes
the requested32-update progression and the source/copy/target roles, not32
observed frames or completed palette presentation. The scene calls P1/P0/P1/P0/
P1/P0/P1 with the sleeps listed above, then P0 only after the explosion's quake
ramp. Flash services can independently reset fading state; there is no global
transaction or fixed original-palette snapshot guaranteed by this call sequence.

csc3F/csc40 map fades call LaunchFading with palette mask1, maximum1 and
OUT_TO_WHITE/IN_FROM_WHITE. csc41 screen flashes shift their operand right3
and use DBF: each4 requests one launch,40 requests six. They use mask$F,
maximum1 and FLASH_QUICKLY_2, then DuplicatePalettes copies BASE to CURRENT.
LaunchFading clears the fading timer/pointer, starts the selected service,
waits for FADING_SETTING to clear plus another VInt, restores the previous
maximum and leaves mask$F. These are conditional service returns; operands
are not literal frame counts.

csc18 entity flash shifts its operand right2 and uses DBF:100 gives26 cycles,
40 gives11. Each cycle sets flags B bit2, requests sprite update, waits two
VInts, clears bit2, requests update and waits two VInts. It does not preserve
an originally set bit2. The accepted nod/headshake/shiver consumers synchronously
perform their sprite-update/transform and wait sequences while other actions
can continue: nod sleeps10/20/10 and resets animation counter0; headshake runs
seven alternating transform/update cycles and resets0; shiver uses shared size21
and three resize/update on/off pairs with Sleep5 per half, restoring saved size/
animation counter but leaving resize cleared. None proves all motion completed.

csc33 writes QUAKE_AMPLITUDE ($FFA80C) directly for operand5. For $4005 it
starts from5, decrements before each write, and uses DBF for five iterations:
amplitudes4,3,2,1,0, each followed by Sleep40. This internal sleep is not a
csWait subject to the debug latch. Conditional completion leaves amplitude0;
interruption can leave an earlier amplitude. Sound commands request the named
SFX through the accepted sound interface; a request does not prove playback,
completion or an audiovisual match.

## Conditional end state and zone caller return

**Confirmed:** the last phase requests C's waited D1,L1,D5 path and hide,
then F's waited L1,D1 path, text and two waited jumps, then D5 and hide.
P and B are faced Down before the final F text; Bowie was faced Down at544.
As elsewhere, ally-facing HP guards can suppress the Bowie/P writes.
C's earlier Init restored autofacing, so the last actual facing of moving
actors is not derived solely from the last explicit setFacing.

`followEntity P,Bowie,2` then `followEntity B,P,2` encode follower words7/31.
csc2C tests the **high byte** of each follower word before decoding the full
IDs; both high bytes are0, so both relations are guarded by Bowie's HP, not
Peter's or B's. If admitted, direction2 selects table_FollowerPositions
`(-24,0)`. AddFollower copies a42-byte eas_Follower1 template, patches the target/
offset and installs it, appending the physical follower to EXPLORATION_ENTITIES
only if absent. Its executed template resets speed32/32 and movement flags,
then follows through destination waits. These calls do not set F67/F68, recruit,
teleport, restore missing mappings, or wait for the relation to settle. Missing
or aliased mappings can create self-follow or replace the first relation.

| State surface | Conditional result at this boundary |
| --- | --- |
| Story flags | No scene-internal flag write. Only a returned scene reaches caller F784 then F802. |
| C/F/E and133–148 | Hide writes issued in the documented order; matching lookup entries invalidated and exploration-list entries removed. Hide leaves action pointers/flags/sprites; the ordinary entity loop skips these $7000-X slots unless later state reactivates them. |
| 131/132 | No placement/hide/action replacement by this scene. Their baseline(63,63), Up declarations and mappings remain subject to inherited/actual entity service.131 only supplies dialogue;132 is unused here. |
| Bowie/P/B | Last explicit facing is Down where HP guards permit; no hides. P/B receive following relations only under the Bowie-HP/high-byte guard and actual mappings. No all-actor settle. |
| E transparency and special graphics | Transparent requests blinking ON; no OFF. Last special-graphics request selects EvilSpiritAlt; hide does not restore the tiles. No guaranteed visual result. |
| Effects | Speed64/64 requested for all; five rays per133–140, cyclic actions for141–148. No scene-wide action cancellation. |
| Quake | Reaching the end of $4005's helper yields amplitude0. Interrupted prefixes may leave5,4,3,2 or1. |
| Palette | Final P0 requests transition toward the COPY snapshot it reads, and overwrites COPY with CURRENT. Sleep60 is not a direct completion poll; no claim of restored original bytes. |
| Camera | Last explicit destination(2,4); following disabled by csc32. csc_end clears VIEW_SCROLLING_SPEED but does not set VIEW_TARGET_ENTITY back to0. |
| Text | Nominal final S3279 closes portrait/text and advances cursor to3280. Debug skip changes that cursor/presentation outcome. |
| Map/request/service state | No scene warp, map reload, inventory/service transaction, EGRESS_MAP assignment or explicit pending-request normalization. Do not infer request absence from the original dispatch clear. |

`mapscriptengine_2.asm:loc_47234` handles csc_end at line580: if the dialogue
window index is nonzero, wait for view scrolling; clear VIEW_SCROLLING_SPEED,
restore saved d0–a6 and return. It does not poll all actors or palette/DMA queues.
The MAPSCRIPT6 trap, which activated VInt_UpdateEntities and called the
j_ExecuteMapScript alias, restores its saved registers only after return.
Those saves restore registers, not the global actor/palette/flag state changed
by the script and services.

The complete `s3_zoneevents.asm` callback then has this ordered boundary,
joined to `tests/fixtures/h2/map-events/zone-programs.json`:

| PC / source line | Operation |
| --- | --- |
| $5B69E / 14 | chkFlg784 |
| $5B6A2 / 15 | bne.s Map34_DefaultZoneEvent; set F784 skips the call and both writes |
| $5B6A4 / 16 | script cs_5B6C0 via lea/trap6; clear F784 admits it |
| $5B6AA / 17 | setFlg784, only after scene return |
| $5B6AE / 18 | setFlg802, only after the preceding write |
| $5B6B2 / 21 | rts, also the default target |

`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` closes portrait/text after the
selected callback, waits one VInt, calls WaitForEntityToStopMoving with physical
index0, restores its saved registers and returns. The helper in
`entityfunctions_2.asm` polls only current/destination X/Y equality for physical0;
it is not an idle-action or all-followers/service barrier. The same wrapper tail
applies when F784 was already set or a different request Y selected the default.
No completed observation establishes that the wrapper or its caller returned.

The sequence is not atomic: interruption during the scene can leave partial
positions/actions/graphics/hides with neither new caller flag; interruption
between setFlg784 and setFlg802 can leave F784 set/F802 unchanged. Neither
flag proves that the wrapper wait completed, control became usable, all effects
stopped, or data persisted through save/load.

## Changed premises, evidence limits and reproduction

These are source discriminators, not executed scenarios:

| Supplied condition | Consequence |
| --- | --- |
| Fresh Map34, F784 clear | Baseline population is declared; a separately selected Y13 callback can call the scene. Entry itself is not a scene trigger. |
| Fresh Map34, F784 set | Empty residents and skipped scene/caller writes. No evidence that consultation ran previously. |
| Retained baseline, F784 set after creation | Existing actors remain; the next eligible callback skips. Setup selection does not remove them. |
| Retained empty/hidden population, F784 cleared | The call is admitted without recreating missing/invalidated actors. Repetition is not a safe reset. |
| Fresh explicit34 again while F784 clear | Rebuilds baseline, unlike CURRENT255 reuse; still no automatic consultation or proof of past completion. |
| F783 or F802 changed alone | No local gate/payload selector change. Already-set F802 does not suppress this call. |
| Follower flags/HP/class/mode/raft state differ | Mappings and inherited properties differ. HP can skip placements/facings or both follower requests while actions continue. No repair is added. |
| Same physical actor reached through multiple logical IDs | Installations/hides/follow relations can overwrite or invalidate one another; waits cease to mean independent actors. |
| Debug latch becomes set | Further text/cursor advancement and csWait sleeps skip; action/helper/internal waits remain. Relative scheduling changes. |
| Entity, camera, fading or text wait never returns | Later source operations and caller flags are not completed facts. No timeout/rollback is supplied here. |
| A new request is written during the scene/wrapper | The selected program has no explicit clear of that request; usable-control/next-event state remains unresolved. |
| Callback returns but physical0 has not settled | Both flags may already be set while the wrapper still waits. |

**Unknown:** natural route/request production and actual caller state, active
population/lookup/HP/class/follower/mode/raft values, scheduling and service
completion, exact movement/graphic/text/sound presentation, final usable control
and save/load persistence. **Inferred:** the coherent staging corresponds to
the narrated consultation, Zeon interruption and explosion. These uncertainties
remain under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic
emulator/acquisition queue. This deliverable ends at conditional zone return.

Load `. ./local/private-inputs.ps1` in the same process as controlled tools.
Verify the read-only upstream HEAD/clean status and reuse the exact
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture`, and `_cutscene_facts`.
No regeneration or blanket normal/full/engine/Godot/H3/H4 suite is selected
solely for this prose. Record actual receipts and diagnostics on #949.

Direct reproduction and full-document review must also perform these joins:

1. Select `programCorpus.programs` by `id=cs_5B6C0` in the retained map-script
   output; the public `tests/fixtures/h2/map-script-engine-static-v1.json`
   contains aggregate/consumer facts, not a duplicate full program corpus.
   Compare those facts with the accepted verifier. Check all409 retained
   sourceLine/macro/arguments records directly against
   lines4–580, and each partition's exact contiguous index interval above.
   Read inline actions through every endActions/ac_end; top-level counts do
   not prove their bodies. Join all73 text comments exactly to hexadecimal
   keys0C87–0CCF; compare ID/source line/form/speaker/ordered controls with the
   table, especially3254's N-only continuation.
2. Compare all sixteen speed bodies, eight orbit entries and full cyclic tails,
   all five eight-ray placement/action groups, all19 hides and their order,
   every special/palette call and each wait/facing/position. Resolve actor
   declarations/lookup/ENTNUM through the accepted entry owner rather than
   assigning physical slots from logical labels.
3. Read each complete selected shared action and its immediate interpreter,
   csc18 flash, csc33 quake, csc3F/40/41 and LaunchFading, the three local
   special helpers and AnimateSpecialSprite plus DMA-request boundary, and
   the two Map07 palette helpers plus UpdateBasePalettesAndBackupCurrent/
   UpdateFadingPalette. Confirm actual operations without a general renderer.
4. In `tests/fixtures/h2/map-events/routing-setup.json`, select
   `expected.categories.zoneEvents.tables` symbol `ms_map34_ZoneEvents`;
   confirm wildcard255/Y13 and default records. In `zone-programs.json`,
   select canonicalSymbol `Map34_ZoneEvent0` and `Map34_DefaultZoneEvent`;
   compare every opcode/operand/sourceLine/PC with the six-row table above.
   Read trap6, csc_end, the full wrapper and physical0 coordinate wait.
5. Check the complete owned document/diff, local links, evidence labels,
   exact pinned provenance, private/generated boundary and `git diff --check`.
   On the clean committed head run configured
   `uv run sf2 verify plan --base origin/main --head HEAD`; record exact head,
   tree/base and actual CI. Independent acceptance and merge remain main-gate's.

Preserve [#945 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178108485)
and [PR948 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/948#issuecomment-6178118055),
including retained pair results and rebase newline/index diagnostics. Preserve
[#941 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)
and [PR944 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802),
including corrected1892 controls and followEntity high-byte/Bowie-HP reasoning.
Their linked [#937 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252),
[review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382)
retain completed findings/failures. None is reset, overwritten or restarted here.
