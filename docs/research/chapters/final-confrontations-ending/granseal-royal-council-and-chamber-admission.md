# Granseal royal council and chamber admission

Continue with [chamber interactions and ending kiss](./granseal-chamber-interactions-and-ending-kiss.md)
for Map20's later interactions and the kiss-to-credits admission boundary.

Evidence date: 2026-10-22. **Confirmed (static):** Castle2F's royal council
selects Bowie in dialogue, sends Sarah and Kazin offstage, then returns to an
init suffix that sets F982 and terminates the temporary follower list. Current
actors remain the scene's actors; subsequent interactions reselect the982 event
table. A future ordinary rebuild selects its different population. The shared
stairs request Map20,23,37/Down; its543 init optionally repositions the Minister,
then requests town music and fade. None of this establishes a playable ending.

## Agreement, provenance and method

Standard rigor for [Issue #1188](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1188)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one complete council → companions' departure → post-council interactions →
ordinary Map20 entry account. Accepted base
`fa50cf56988ac1fa02e4c38205d5b37a30fcb8d2`, tree
`9f1c1ccb7248125efafe7194c9383d7975fbf933`; no stacked dependency. Own this
document and predecessor navigation only. Acceptance covers the complete royal
script including its internal branch and inline actions, dialogue/cursor/speaker
changes, both Map19 populations and33 event declarations, defaults/search,
seven warps, Map20 selection/population/short init and actual consumer limits.
Map20 later interactions, kiss, credits, WitchEnd, implementation and runtime
collection remain outside scope. No shared schema/index/fixture/code change.

The [entry owner](./granseal-epilogue-and-castle-admission.md#castle2f-entry-and-the-next-scene-boundary)
retains the invitation, five sequential **waiting setDest** town requests,
queued warp and optional25/8/27 checks before cs_62D06. Reuse
[common scripting](../../world/common-scripting.md), [common maps](../../world/common-maps.md),
[interaction state](../../world/map-event-interaction-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[follow semantics](../moun-nazca-grans/roft-town-interactions-and-zynk-recruitment.md#actual-join-and-field-follow-effects)
and [search/award semantics](../moun-nazca-grans/dojo-exploration-and-sheela-recruitment.md).
All source paths below are relative to pinned read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`castle/` means `data/maps/entries/map19/mapsetups/`, `chamber/` the Map20
equivalent, `map/` means `code/common/scripting/map/`. Addresses and packed words
are hexadecimal, intervals half-open; IDs, coordinates and waits are decimal.
The registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this source inspection makes no fresh ROM/source-equivalence claim.

Linear reads of selected source and the existing18,285,979B init/script pair
are sufficient. Pair provenance remains `local/issue1005/pair.txt`; no recreation
or world preparation. One selected lowering addresses concrete roof-service,
internal-label and post-return publication gaps; its refusal is retained below.
Targets are<60s/<512MiB per direct check, peak **unmeasured**, each small output
<2MiB and all new ignored output<4MiB under `local/issue1188/`. These are replan
signals, not reasons to omit necessary design or facts. Time/memory are linear
in existing pair plus bounded source/consumer reads, without acquisition or
model/API batches. Full local recipes and fresh receipts remain review aids;
this document, named source and exact Git objects own the durable findings.

## Admission and actor continuity

**Confirmed:** Map19 selector order in `data/maps/mapsetups.asm` is
default→501→609→506→507→543→982, last held flag wins. F982 alone can therefore
select982 even with clear543. The982 pointer table `$629B2..$629CA` changes only
population and entity events, reusing543 zone/descriptions/item/init. Population
and init are separately selected reads, not a flag snapshot.

The common init `castle/s6_initfunction_543.asm:$62C9E..$62D06` checks25/Sheela,
8/May,27/Chaz in order; clear moves the resolved actor to `$7000,$7000` through
MoveEntityOutOfMap, preserving its alias. Every combination next calls
cs_62D06 `$62D06..$62D0E`: Rohde's NPC sprite request then end. No recruitment
write is made. Live981/982 then choose:

| F981 | F982 | Remaining init requests |
| --- | --- | --- |
|clear|clear or held|MUSIC_TOWN → FadeInFromBlack → rts; no royal scene.|
|held|clear|Full cs_62D0E → set982 → first EXPLORATION_ENTITIES byteFF → rts.|
|held|held|MUSIC_TOWN → FadeInFromBlack → rts; no royal scene.|

An interruption before script return leaves982 unwritten by this suffix.
Neither this init nor the royal script clears981. No saved one-time or marriage
state follows. Supplied982/clear981 selects982 population but skips council;
clear982/clear981 can leave the543 hidden actors without council positioning.
These are valid input distinctions, not observed natural histories.

Ally logical IDs in sf2enums.asm are Bowie0, Sarah1, Chester2, Jaha3,
Kazin4, Slade5, Peter7, May8, Luke10, Rohde11, Higins19, Sheela25, Zynk26
and Chaz27. These logical values do not change with allocation.
Bowie is physical0. Allies and nonallies resolve through ENTITY_INDEX_LIST;
logical129 here is **Astral**, not the town's138. Persistent followers allocate
first, and already aliased ally rows are consumed without reposition/script
replacement. With **no persistent followers**,543 allocates Slade1, Peter2,
May3, Rohde4, Luke5, Higins6, Sheela7, Chaz8, Zynk9, Sarah10, Chester11,
Jaha12, Kazin13, King128→14, Astral129→15, Old Woman130→16, Guards131–134→17–20.
The982 rebuild gives Chester10, Jaha11, King12, Astral13, Old Woman14,
Guards15–18; the first nine allies keep slots1–9 in that example. Additional
followers shift slots or reuse an ally alias; no logical-ID-as-slot assumption.

`csc2C_followEntity` reads the **high byte of each follower word** before
resolving that word. All five royal followers have high byte0, so the precheck
is live Bowie HP, including Sarah/Kazin/Jaha/Chester calls. Bowie HP0 skips
each six-byte operand group; positive HP permits AddFollower. Signed pixel
offsets2=(−24,0),5=(−24,−24),6=(−24,+24) are rotated/conditioned by actual
leader motion and layout. No constant HP is supplied by the town's prior heal.
setPos instead has a byte actor HP guard, so Sarah/Kazin/Jaha/Chester placement
can independently skip for that ally's live HP; nonally129 bypasses that check.
startEntity uses a word actor HP check and sets ANIMCOUNTER0, not a new actor.

## Complete council script and command boundaries

**Confirmed:** `castle/s6_initfunction_543.asm:cs_62D0E` occupies
`$62D0E..$631EA`:1,244 physical bytes. The retained corpus attributes245 map
commands and1,028 encoded command bytes to this one script; inline entity
actions account for the216-byte difference. `cs_62FB0:$62FB0` is an internal
label within the same physical script, not another corpus body or independent
scene. The complete init/source file is16,601 UTF-8 bytes and includes cs_62D06; its byte length must
not be called the royal script's encoded size.

The ledger below retains every map request and every inline action in order.
Ranges combine only consecutive facing requests and preserve actor order.
`W` means install and wait for the **named actor's script pointer** to reach
eas_Idle; `A` installs without that wait. Inline U/D/L/R moves are relative
**destination moves with waits**; faceU/D/L/R includes its operand wait.
`speed(x,y) → idle` is the complete custom action body, not a relocation.
`T`=nextText, `S`=nextSingleText; speaker modifier is00 or80. Text IDs below
assume ordinary non-skipped text completion. Phase labels organize reading,
not extra game state or barriers.

setPos writes current/destination X/Y in tile units×384 and facing after its
HP guard; it does not wait for travel. No setDest occurs in this royal script.
entityActions clears flag bits5/6 with AND9F; customActscript and setActscript
do not perform that clear. csc14/csc15/csc2D install the resolved **physical slot** in
ACTSCRIPTWAITTIMER. Astral's example timer is15, King's14, Sarah's10, Bowie's0;
these change with allocation. A later ac_jump executes timer clearing separately.
No installation timer is silently assumed zero, and no frame duration is derived.
csc14 captures the inline stream, waits when controlFF, then scans through
the8080 terminator; csc15 installs an external stream pointer. Their Wait
spellings are encoded controls, not a universal actor-completion barrier.

The five waited eas_Init replacements end following scripts in Bowie, Sarah,
Jaha, Chester, Kazin order; they do not remove the list entries themselves.
eas_Init sets acceleration factors0, speed32/32, disables acceleration and
deceleration, sets obstruction7, clears collision5/6, enables autofacing,
orients up, clears immersion, sets size24, refreshes sprite then jumps idle.
This explains later collision/speed state without interpreting names containing
“Uncollidable” as proof of unimpeded movement.

setCameraEntity resolves an alias into VIEW_TARGET_ENTITY. setCamDest writes−1
to that target, sets a tile destination and waits for view-scroll completion.
The two early waited Astral routes request26,29→29,23→29,9; his following U1
requests29,8. These are conditional route arithmetic if services finish,
not observed geometry or follower endpoints. Only named waited actors finish;
Sarah's first D1 and Kazin's D2 are asynchronous installations later replaced
by further scripts. Sleeps and dialogue may allow progress but are not global
actor joins. Facing requests do not establish successful previous movement.

roofEvent29,23 calls csc43_RoofEvent: coordinates×MAP_TILE_SIZE then
PerformMapBlockCopyScript (`code/gameflow/exploration/exploration.asm`,
symbol is authoritative). It returns immediately when the
live roof-active word is nonzero. Otherwise its coordinate hash scans current
roof records; the second Map19 row is29,23, source255/255, size38×23,
destination2,32 (`data/maps/entries/map19/5-roof-events.asm:$A4614..$A464E`).
Signed source Y−1 selects save-old-blocks-and-clear, not reading a255,255 tile.
It records the active row, saves rectangle state, clears working layout words,
terminates saved data, requests MUSIC_NOTHING and sets plane-update bit0.
No match causes no copy. This is a conditional working-layout/service request;
return is not a VDP display, collision, restoration or player-ready observation.

### Arrival and royal announcement

| Order | Source line | Ordered request |
| --- | --- | --- |
|1|57|Cursor 3967|
|2|58|Camera actor Astral129|
|3|59|Place Astral129 26,29/UP|
|4|60|Place Kazin 26,30/UP|
|5|61|Place Sarah 26,30/UP|
|6|62|Place Jaha 26,30/UP|
|7|63|Place Chester 26,30/UP|
|8|64|Follow Bowie → Astral129 offset2|
|9|65|Follow Kazin → Astral129 offset5|
|10|66|Follow Sarah → Astral129 offset6|
|11|67|Follow Jaha → Kazin offset2|
|12|68|Follow Chester → Sarah offset2|
|13|69|Sleep 10|
|14|70|Fade in from black|
|15|71|Motion W Astral129: U(4) → R(3) → U(2) → idle|
|16|76|Roof request 29,23|
|17|77|Motion W Astral129: U(14) → idle|
|18|80|Script W Bowie: eas_Init|
|19|81|Script W Sarah: eas_Init|
|20|82|Script W Jaha: eas_Init|
|21|83|Script W Chester: eas_Init|
|22|84|Script W Kazin: eas_Init|
|23|85|Camera destination 24,5|
|24|86|Sleep 40|
|25|87|Motion W Astral129: speed(16,16) → jump(eas_Idle)|
|26|91|Motion W Astral129: U(1) → idle|
|27|94|Sleep 30|
|28|95|S3967 Astral129/00|
|29|96|nod King128|
|30|97|S3968 King128/80|
|31|98|Sleep 10|
|32|99|nod Astral129|
|33|100|S3969 Astral129/00|
|34|101|Motion W Astral129: R(1) → faceD(1) → idle|
|35|105|Sleep 30|
|36|106|nod Astral129|
|37–50|107|Face Bowie, Sarah, Jaha, Chester, Kazin, Slade, Peter, Luke, Sheela, May, Higins, Rohde, Zynk, Chaz UP|
|51|121|Sleep 10|
|52|122|T3970 Astral129/00|
|53|123|T3971 Astral129/00|
|54|124|S3972 Astral129/00|
|55|125|Script W Peter: eas_Jump|
|56|126|Script W Peter: eas_Jump|
|57|127|S3973 Peter/80|
|58|128|Sleep 10|
|59|129|shiver Sarah|
|60|130|Sleep 10|
|61|131|Face Astral129 UP|
|62|132|Sleep 40|
|63|133|nod King128|
|64|134|Sleep 30|
|65|135|S3974 King128/00|
|66|136|Sleep 30|
|67|137|Face Astral129 DOWN|
|68|138|S3975 Astral129/00|
|69|139|Face Peter LEFT|
|70|140|S3976 Peter/80|
|71|141|Face Peter DOWN|
|72|142|Sleep 20|
|73|143|shiver Luke|
|74|144|Sleep 20|
|75|145|T3977 Luke/80|
|76|146|Sleep 10|
|77|147|Face Luke LEFT|
|78|148|S3978 Luke/80|
|79|149|Motion W Astral129: R(2) → faceD(1) → idle|
|80–81|153|Face Peter, Luke UP|
|82|155|S3979 Astral129/00|

### Objections and May's optional line

| Order | Source line | Ordered request |
| --- | --- | --- |
|83|156|Face Peter DOWN|
|84|157|Sleep 60|
|85|158|Motion W Astral129: L(2) → faceD(1) → idle|
|86|162|Face Peter UP|
|87|163|S3980 Astral129/00|
|88|164|Sleep 30|
|89|165|Motion W Astral129: L(2) → faceD(1) → idle|
|90|169|Face Peter DOWN|
|91|170|Face Kazin RIGHT|
|92|171|Face Luke UP|
|93–95|172|Face Sheela, May, Higins DOWN|
|96|175|Face Rohde UP|
|97|176|Face Bowie DOWN|
|98|177|Sleep 40|
|99|178|Camera destination 24,7|
|100|179|Sleep 5|
|101|180|Script A Jaha: eas_2xRightLeft|
|102|181|Sleep 100|
|103|182|Motion W Jaha: D(1) → idle|
|104|185|Face Jaha RIGHT|
|105|186|shiver Jaha|
|106–108|187|Face Sarah, Chester, Kazin DOWN|
|109|190|Face Slade RIGHT|
|110|191|Face Luke LEFT|
|111–112|192|Face Sheela, May RIGHT|
|113|194|Face Higins LEFT|
|114–115|195|Face Rohde, Zynk RIGHT|
|116|197|Face Chaz LEFT|
|117|198|Face Bowie DOWN|
|118|199|T3981 Jaha/00|
|119|200|S3982 Jaha/00|
|120|201|Sleep 5|
|121|202|Script A Jaha: eas_2xRightLeft|
|122|203|shiver Sarah|
|123|204|Sleep 20|
|124|205|Face Peter DOWN|
|125|206|Face Luke UP|
|126|207|Sleep 20|
|127|208|headshake Luke|
|128|209|Sleep 20|
|129|210|Motion W Peter: L(1) → D(2) → faceL(1) → idle|
|130|215|Face Luke LEFT|
|131|216|Face Jaha RIGHT|
|132|217|S3983 Peter/80|
|133|218|shiver Jaha|
|134|219|Motion W Slade: R(1) → D(1) → faceR(1) → idle|
|135|224|Face Jaha LEFT|
|136|225|T3984 Slade/00|
|137|226|S3985 Slade/00|
|138|227|shiver Jaha|
|139|228|If clear F8 jump cs_62FB0|
|140|229|Motion W May: D(1) → faceR(1) → idle|
|141|233|S3986 May/00|
|142|234|shiver Jaha|

### Internal continuation and companion departures

| Order | Source line | Ordered request |
| --- | --- | --- |
|143|235|Cursor 3987 (internal cs_62FB0)|
|144|236|Motion W Peter: U(1) → faceD(1) → idle|
|145|240|Motion W Higins: L(1) → idle|
|146|243|Face Jaha RIGHT|
|147|244|T3987 Higins/80|
|148|245|S3988 Higins/80|
|149|246|shiver Jaha|
|150|247|Sleep 30|
|151|248|Motion W Astral129: R(2) → D(1) → idle|
|152|252|S3989 Astral129/00|
|153|253|shiver Sarah|
|154–160|254|Face Sarah, Jaha, Chester, Kazin, Peter, Higins, Bowie UP|
|161|261|T3990 Sarah/80|
|162|262|shiver Sarah|
|163|263|T3991 Sarah/80|
|164|264|Face Sarah LEFT|
|165|265|Sleep 30|
|166|266|Face Sarah UP|
|167|267|Sleep 10|
|168|268|S3992 Sarah/80|
|169|269|Motion W Sarah: speed(48,48) → jump(eas_Idle)|
|170|273|Motion W Chester: speed(48,48) → jump(eas_Idle)|
|171|277|Motion A Sarah: D(1) → idle|
|172|280|Motion W Chester: L(1) → idle|
|173|283|Script A Chester: eas_RotateRight|
|174–178|284|Face Bowie, Kazin, Jaha, Peter, Higins DOWN|
|179|289|Motion W Sarah: D(8) → idle|
|180|292|hide Sarah|
|181|293|Script W Chester: eas_Init|
|182|294|startEntity Chester|
|183|295|Face Chester DOWN|
|184|296|headshake Chester|
|185|297|Sleep 30|
|186|298|Camera destination 24,9|
|187|299|Motion W Chester: D(2) → idle|
|188|302|Sleep 50|
|189|303|S3993 Chester/00|
|190|304|Script W Kazin: eas_Jump|
|191|305|Script W Kazin: eas_Jump|
|192|306|Motion W Kazin: D(1) → R(2) → D(2) → faceL(1) → idle|
|193|312|Face Chester RIGHT|
|194|313|S3994 Kazin/80|
|195|314|Face Chester DOWN|
|196|315|Motion A Kazin: D(2) → idle|
|197|318|Sleep 8|
|198|319|Script W Chester: eas_Jump|
|199|320|Script W Chester: eas_Jump|
|200|321|S3995 Chester/00|
|201|322|Face Kazin UP|
|202|323|S3996 Kazin/80|
|203|324|Motion W Kazin: D(5) → idle|
|204|327|hide Kazin|
|205|328|Sleep 30|
|206|329|S3997 Chester/00|

### Final recommendation and royal release

| Order | Source line | Ordered request |
| --- | --- | --- |
|207|330|Motion W Rohde: R(2) → D(1) → R(2) → faceD(1) → idle|
|208|336|S3998 Rohde/80|
|209|337|Motion W Zynk: R(3) → D(1) → faceR(1) → idle|
|210|342|S3999 Zynk/00|
|211|343|shiver Zynk|
|212|344|Face Rohde LEFT|
|213|345|Script W Rohde: eas_Jump|
|214|346|Script W Rohde: eas_Jump|
|215|347|S4000 Rohde/80|
|216|348|Camera destination 24,5|
|217|349|Motion W Luke: U(1) → L(2) → faceU(1) → idle|
|218–225|354|Face Jaha, Peter, Chester, Luke, Rohde, Zynk, Higins, Bowie UP|
|226|362|T4001 Luke/80|
|227|363|Face Luke DOWN|
|228|364|T4002 Luke/80|
|229|365|nod Astral129|
|230|366|Face Luke UP|
|231|367|S4003 Luke/80|
|232|368|shiver Astral129|
|233|369|T4004 Astral129/00|
|234|370|T4005 Astral129/00|
|235|371|S4006 Astral129/00|
|236|372|Motion W King128: speed(16,16) → jump(eas_Idle)|
|237|376|Motion W King128: D(1) → R(1) → faceD(1) → idle|
|238|381|Face Astral129 UP|
|239|382|S4007 King128/80|
|240|383|Motion W King128: L(1) → D(1) → idle|
|241|387|S4008 King128/80|
|242|388|Motion W King128: U(2) → faceD(1) → idle|
|243|392|Motion W Astral129: L(1) → faceD(1) → idle|
|244|396|S4009 Astral129/00|
|245|397|End map script|

The full Jump body disables autofacing; relative0,−1; wait5; relative0,+1;
wait5; relative0,0; autofacing on; jump idle. It has no waitDest between those
relative requests. Jaha's eas_2xRightLeft faces R,L,R,L, refreshing and waiting30
after each, then jumps idle; the two A installations can be replaced before
finishing. Chester's eas_RotateRight disables animation/autofacing, faces
L,U,R,D with refresh/wait5 each, then **word_45FEC ac_branch loops back**.
His later waited eas_Init replaces that loop; startEntity then enables animation.
No exact number of rotations or jump height follows from the source requests.

## Dialogue, speakers and cursor

`data/scripting/text/gamescript.txt` owns the IDs and tokens. Meanings below
are author paraphrases; claims about feelings, two years, choice or the future
are attributed speech. They do not write party membership, HP, marriage or
ending progress. There is no yesNo, join, joinForce, clear joined flag or
marriage write in cs_62D0E. May's F8 test controls only her motion/text/shiver
suffix; Sheela/Chaz's absence does not suppress all their facing requests.

Ordinary text begins cursor3967; T/S advance it after DisplayText returns.
Single closes portrait/text and sleeps10; continued keeps the text sequence.
csc00/csc02's SKIP_CUTSCENE_TEXT branch consumes its speaker operand without the
normal display/increment; thus the ledger's IDs are not an assertion under
that debug state. Clear8 jumps over May's3986 to cs_62FB0; held8 executes it
and falls through. **Both reset cursor3987 at that internal label**. No cursor
increment is invented for the skipped line. Cursor after normal final4009 is4010.
Modifier80 retains its source portrait request; it is not a new speaker identity.

| ID | Speaker / modifier / mode | Author meaning | Ordered control tokens |
| --- | --- | --- | --- |
|3967|Astral129/00/S|Astral announces Bowie to the King.|`{LEADER}{N}{W1}`|
|3968|King128/80/S|King asks for an explanation.|`{N}{W1}`|
|3969|Astral129/00/S|Astral agrees.|`{W1}`|
|3970|Astral129/00/T|Astral calls for attention.|`{W2}`|
|3971|Astral129/00/T|Astral says the poison recovery took two years.|`{N}{N}{W2}`|
|3972|Astral129/00/S|Astral says Elis sleeps and proposes Mitula advice.|`{N}{N}{W1}`|
|3973|Peter/80/S|Peter asks whether someone will kiss her.|`{N}{W1}`|
|3974|King128/00/S|King says the time is right.|`{N}{N}{W1}`|
|3975|Astral129/00/S|Astral asks who should kiss Elis.|`{N}{N}{W1}`|
|3976|Peter/80/S|Peter nominates Bowie and asks Luke.|`{N}{LEADER}{N}{NAME;10}{W1}`|
|3977|Luke/80/T|Luke agrees with Peter.|`{NAME;7}{W2}`|
|3978|Luke/80/S|Luke says Bowie is the best choice.|`{LEADER}{N}{W1}`|
|3979|Astral129/00/S|Astral asks for disagreement.|`{N}{W1}`|
|3980|Astral129/00/S|Astral again asks whether anyone objects.|`{W1}`|
|3981|Jaha/00/T|Jaha declares his love despite the others.|`{N}{N}{W2}`|
|3982|Jaha/00/S|Jaha asks to kiss her and requests support.|`{N}{W1}`|
|3983|Peter/80/S|Peter rejects Jaha as the choice.|`{N}{NAME;3}{N}{W1}`|
|3984|Slade/00/T|Slade understands Jaha but notes Elis is human.|`{NAME;3}{N}{W2}`|
|3985|Slade/00/S|Slade recommends a human.|`{W1}`|
|3986|May/00/S|May supports Bowie as a human partner.|`{LEADER}{N}{W1}`|
|3987|Higins/80/T|Higins praises Bowie as a lad with knightly manners.|`{LEADER}{N}{N}{W2}`|
|3988|Higins/80/S|Higins endorses him.|`{N}{W1}`|
|3989|Astral129/00/S|Astral asks Sarah.|`{NAME;1}{N}{W1}`|
|3990|Sarah/80/T|Sarah hesitates.|`{W2}`|
|3991|Sarah/80/T|Sarah expresses concern and hesitation.|`{N}{W2}`|
|3992|Sarah/80/S|Sarah accepts Bowie while distressed.|`{N}{LEADER}{N}{W1}`|
|3993|Chester/00/S|Chester asks whether Sarah loves Bowie.|`{LEADER}{N}{W1}`|
|3994|Kazin/80/S|Kazin offers to comfort Sarah.|`{N}{W1}`|
|3995|Chester/00/S|Chester asks to accompany Kazin.|`{NAME;4}{N}{W1}`|
|3996|Kazin/80/S|Kazin refuses his company.|`{N}{W1}`|
|3997|Chester/00/S|Chester is confused.|`{N}{W1}`|
|3998|Rohde/80/S|Rohde recalls youth.|`{N}{W1}`|
|3999|Zynk/00/S|Zynk questions Rohde having been young.|`{NAME;11}{N}{W1}`|
|4000|Rohde/80/S|Rohde rebukes Zynk.|`{W1}`|
|4001|Luke/80/T|Luke reiterates the group recommendation.|`{N}{LEADER}{N}{W2}`|
|4002|Luke/80/T|Luke says Kazin will care for Sarah.|`{NAME;4}{N}{NAME;1}{W2}`|
|4003|Luke/80/S|Luke suggests Astral expected this.|`{N}{W1}`|
|4004|Astral129/00/T|Astral praises Luke for noticing.|`{N}{NAME;10}{W2}`|
|4005|Astral129/00/T|Astral says Sarah and Jaha reactions were uncertain and he wished not to hurt them.|`{N}{NAME;1}{NAME;3}{W2}{N}{W1}`|
|4006|Astral129/00/S|Astral explicitly chooses Bowie.|`{LEADER}{W1}`|
|4007|King128/80/S|King asks Bowie to wake Elis.|`{LEADER}{N}{W1}`|
|4008|King128/80/S|King says the Minister is waiting.|`{N}{W1}`|
|4009|Astral129/00/S|Astral expresses respect for Bowie.|`{LEADER}{N}{W2}`|

## Returned world and repeat interactions

**Confirmed:** `csc2E_hideEntity` resolves the actor and calls HideEntity
(`code/common/scripting/entity/entityfunctions_2.asm`). Unlike optional
MoveEntityOutOfMap, HideEntity writes current/destination `$7000,$7000`,
clears **every matching alias** toFF, and removes the first matching physical
follower-list entry by shifting through its terminator. Sarah/Kazin therefore
lose those current aliases when their hides execute. Their ally party/join
flags, stats and equipment are not cleared. The actor slots are not a rebuilt
population. Chester/Jaha remain current actors with the actions above.

After csc_end returns, set982 runs, then a **single byte** FF is written to
EXPLORATION_ENTITIES. This makes sentinel-scanning users (IsFollowerEntity,
AddFollower, raft positioning) see an empty list; it does not clear all bytes,
ENTITY_INDEX_LIST, current actor slots or persistent follower flags66–84.
It does not call InitializeMapEntities, rerun eas_Init on every actor, clear
other scripts, or reposition the current population to the982 declarations.
GetActivatedEntity normally excludes listed physical followers. The empty
sentinel removes that exclusion for current actors; reach and alias validity
still apply. Most royal follows were already replaced, but this list reset is not a general
script cancellation mechanism. A future ordinary explicit-map rebuild selects
population afresh and initializes persistent followers again.

Each later interaction calls RunMapSetupEntityEvent, reselecting setup from
live flags. Held982 immediately picks its table **against the current aliases**;
no repeat interaction calls cs_62D0E or rewrites981/982. If982 is externally
cleared with981 still held, ordinary init can request council again on next
entry; no rollback or complete-scene resumption is provided. If982 changes
between population and callback selection, mismatched declarations and current
actors must not be silently reconciled by this static account.

Both tables' UP1/DOWN3 event bytes turn toward the player (bit0); DOWN3 also
takes the post-callback property restore branch (bit1). RIGHT0 does neither.
These bytes are not required player facings. The wrapper first resolves and
stores portrait/speech properties and displays the portrait when nonnegative.
It then resolves the physical alias; bit0 conditionally waits for VInt and
turns the actor. Only then does it clear VInt_UpdateEntities and call the
callback. On return, bit1 conditionally restores properties, then it closes
portrait/text and reactivates updates. Actual service completion is unobserved.

### Complete populations and33 event declarations

`castle/s1_entities_543.asm:$629CA..$62A6C` has20 rows;
`s1_entities_982.asm:$62A6C..$62AFE` has18. All fixed rows use eas_Init except
Guards133/134 eas_InitFixedSprite; W denotes walking origin/radius2.
Tables below show all declarations, not a promise of physical presence after
optional removal, aliases, live HP or the scene. Event names are relative to
`ms_map19_flag543_EntityEvents` or982 equivalent.

| Logical actor | F543 x,y/facing; action | F982 rebuild x,y/facing; action |
| --- | --- | --- |
|Slade|26,11/RIGHT; eas_Init|27,12/RIGHT; eas_Init|
|Peter|32,10/LEFT; eas_Init|31,11/LEFT; eas_Init|
|May|26,10/RIGHT; eas_Init|26,11/RIGHT; eas_Init|
|Rohde|26,13/RIGHT; eas_Init|30,14/UP; eas_Init|
|Luke|32,11/LEFT; eas_Init|30,10/LEFT; eas_Init|
|Higins|32,12/LEFT; eas_Init|32,11/LEFT; eas_Init|
|Sheela|26,12/RIGHT; eas_Init|26,12/RIGHT; eas_Init|
|Chaz|32,13/LEFT; eas_Init|32,13/LEFT; eas_Init|
|Zynk|25,13/RIGHT; eas_Init|28,14/UP; eas_Init|
|Sarah|63,63/RIGHT; eas_Init|absent|
|Chester|63,63/LEFT; eas_Init|29,13/UP; eas_Init|
|Jaha|63,63/UP; eas_Init|28,12/DOWN; eas_Init|
|Kazin|63,63/DOWN; eas_Init|absent|
|128 King|29,6/DOWN; eas_Init|29,6/DOWN; eas_Init|
|129 Astral|63,63/DOWN; eas_Init|29,9/DOWN; eas_Init|
|130 Old Woman|24,16/RIGHT; eas_Init|24,16/RIGHT; eas_Init|
|131 Guard|23,26/DOWN; W(23,26,2)|23,26/DOWN; W(23,26,2)|
|132 Guard|34,26/DOWN; W(34,26,2)|34,26/DOWN; W(34,26,2)|
|133 Guard|14,25/DOWN; eas_InitFixedSprite|14,25/DOWN; eas_InitFixedSprite|
|134 Guard|17,25/DOWN; eas_InitFixedSprite|17,25/DOWN; eas_InitFixedSprite|

F543 event source `$62AFE..$62BCA` has15 declarations and a default;982
`$62BCA..$62C6C` has18 and a default. No Sarah/Kazin event is added to either.
F543 also lacks Chester/Jaha/Astral rows, so they take default if interactable
under that table. F982's last five explicitly reuse543 callbacks via16-bit
offsets; it also reuses the same rts-only default. Aliases being present and
being reachable are separate conditions. Chaz's982 callback intentionally
reuses4002–4003 (Luke's scene lines); preserve actual event speaker Chaz.

| Setup | Actor | Event byte | Exact callback | Text IDs in order |
| --- | --- | --- | --- | --- |
|543|Slade|UP|Map19_21F_EntityEvent0|3945,3946,3947|
|543|Peter|UP|Map19_21F_EntityEvent1|3948,3949|
|543|May|UP|Map19_21F_EntityEvent2|3950,3951|
|543|Rohde|UP|Map19_21F_EntityEvent3|3952,3953|
|543|Luke|UP|Map19_21F_EntityEvent4|3954,3955|
|543|Higins|UP|Map19_21F_EntityEvent5|3956,3957|
|543|Sheela|UP|Map19_21F_EntityEvent6|3958,3959|
|543|Chaz|UP|Map19_21F_EntityEvent7|3960,3961,3962|
|543|Zynk|UP|Map19_21F_EntityEvent8|3963,3964|
|543|128|DOWN|Map19_21F_EntityEvent9|3965,3966|
|543|130|DOWN|Map19_21F_EntityEvent10|4017|
|543|131|UP|Map19_21F_EntityEvent11|4018|
|543|132|UP|Map19_21F_EntityEvent12|4019|
|543|133|RIGHT|Map19_21F_EntityEvent13|4020|
|543|134|RIGHT|Map19_21F_EntityEvent14|4021|
|982|Jaha|UP|Map19_3D6_EntityEvent0|4029|
|982|Chester|UP|Map19_3D6_EntityEvent1|3997|
|982|Slade|UP|Map19_3D6_EntityEvent2|4030|
|982|Peter|UP|Map19_3D6_EntityEvent3|4026,4027|
|982|May|UP|Map19_3D6_EntityEvent4|3986|
|982|Rohde|UP|Map19_3D6_EntityEvent5|3998|
|982|Luke|UP|Map19_3D6_EntityEvent6|4028|
|982|Higins|UP|Map19_3D6_EntityEvent7|3988|
|982|Sheela|UP|Map19_3D6_EntityEvent8|4031|
|982|Chaz|UP|Map19_3D6_EntityEvent9|4002,4003|
|982|Zynk|UP|Map19_3D6_EntityEvent10|4032|
|982|128|DOWN|Map19_3D6_EntityEvent11|4008|
|982|129|UP|Map19_3D6_EntityEvent12|4009|
|982|130|DOWN|Map19_21F_EntityEvent10|4017|
|982|131|UP|Map19_21F_EntityEvent11|4018|
|982|132|UP|Map19_21F_EntityEvent12|4019|
|982|133|RIGHT|Map19_21F_EntityEvent13|4020|
|982|134|RIGHT|Map19_21F_EntityEvent14|4021|

All these callback bodies contain only the listed txt requests then rts.
They contain no join, flag write, scene call, choice or one-time guard. Repeat
interaction repeats its currently selected dialogue, conditional on services
returning and the actor still being available. Inviting Bowie in Zynk's3964
is speech, not a recruitment mechanism.

### Resident dialogue and search

| ID | Author meaning | Ordered control tokens |
| --- | --- | --- |
|3945|Slade greets Bowie.|`{LEADER}{W2}`|
|3946|Slade mentions his invitation.|`{N}{W2}`|
|3947|Slade is pleased everyone looks well.|`{N}{W1}`|
|3948|Peter again serves Volcanon.|`{LEADER}{N}{W2}`|
|3949|Peter asks who is marrying.|`{N}{N}{W1}`|
|3950|May admires Granseal.|`{W2}`|
|3951|May says it exceeds Chester description.|`{NAME;2}{N}{N}{W1}`|
|3952|Rohde greets Bowie.|`{LEADER}{N}{W2}`|
|3953|Rohde reports Caravan travel and more ruins.|`{N}{N}{W1}`|
|3954|Luke says Astral invited him and Peter.|`{N}{NAME;7}{W2}`|
|3955|Luke finds the former threat hard to believe.|`{N}{W1}`|
|3956|Higins relays Pacalon king greetings.|`{N}{W2}`|
|3957|Higins praises the castle.|`{N}{W1}`|
|3958|Sheela recalls two years since the battle.|`{N}{W2}`|
|3959|Sheela hopes Lemon ({NAME;28}) is well.|`{NAME;28}{W1}`|
|3960|Chaz admires the occasion.|`{W2}`|
|3961|Chaz compares Bowie and Elis ages.|`{LEADER}{N}{N}{W1}`|
|3962|Chaz dismisses age as important.|`{W1}`|
|3963|Zynk calls Rohde eccentric but great.|`{NAME;11}{N}{W2}`|
|3964|Zynk describes more adventure and invites Bowie.|`{N}{N}{LEADER}{W1}`|
|3965|King asks why Elis sleeps.|`{W2}`|
|3966|King says Astral knows a candidate.|`{W1}`|
|4017|Old Woman plans a meal.|`{N}{W1}`|
|4018|Guard131 notes many guests.|`{N}{W1}`|
|4019|Guard132 promises protection.|`{N}{W1}`|
|4020|Guard133 asks to sleep.|`{N}{LEADER}{N}{W1}`|
|4021|Guard134 dreams of earthquake and evacuation.|`{N}{N}{W1}`|
|4026|Peter urges Bowie upstairs.|`{LEADER}{N}{W2}`|
|4027|Peter recalls his adventure performance.|`{N}{W1}`|
|4028|Luke urges the kiss.|`{LEADER}{W1}`|
|4029|Jaha says he will find someone someday.|`{W2}{N}{W1}`|
|4030|Slade says Jaha is tough.|`{N}{W1}`|
|4031|Sheela tells Bowie to stay calm.|`{N}{W1}`|
|4032|Zynk again questions Rohde youth.|`{W1}`|
|4052|Book about Granseal kings.|`{N}{W1}`|
|4053|Book about treasures.|`{N}{W1}`|
|4054|A cool, deep, dark well.|`{N}{W1}`|

`castle/s3_zoneevents_543.asm:$62C6C..$62C72` has only the default rts:
no castle-specific zone declaration or automatic council trigger exists there.
The common init, not a zone, requests the scene. Item section
`$62C96..$62C9E` also has only a default rts. Neither disables common Search.

Description source `$62C72..$62C96` sets base `$FD4`=4052 and jumps
DisplayAreaDescription. Complete rows are19,3 type4 offset0→4052;
20,3 type4 offset1→4053;13,20 type5 offset2→4054. DisplayAreaDescription
(`map/mapsetupsfunctions_1.asm`) reads row byte4 and adds423: type4's
introduction427 investigates shelves, type5's428 checks a well; each has
`{NAME}{N}{W2}{CLEAR}`. Facing/adjacent-cell selection and the common
open/chest/item precedence remain with the search owner. Coordinates are lookup
keys, not proof Bowie can stand at the necessary adjacent tile.

Shared Map19 chest table `$A4688..$A4696` declares8,10/F129 MEDICAL_HERB,
9,10/F130 ANGEL_WING,10,10/F131 ANTIDOTE; other-items `$A4696..$A4698`
is terminator-only. Flags129–131 here are item-consumed flags, not actor IDs.
Common search checks current consumed state, awards through its real capacity/
item service and uses generic fallback; no event default grants an item or
resets those flags. Shared tables are unchanged by543/982, and current layout,
geometry, inventory and prior item consumption remain inputs.

## Shared exits and ordinary chamber admission

`data/maps/entries/map19/6-warp-events.asm:$A464E..$A4688` has seven
no-scroll rows, shared across setups, with no table-level981/982/543 test:

| Trigger x,y | Map / destination / facing | Source purpose |
| --- | --- | --- |
|6,2|20 /6,37 /R|west tower stairs up|
|23,3|20 /23,37 /D|royal chambers stairs up|
|35,7|20 /36,40 /R|east tower stairs up|
|38,8|20 /38,10 /L|east tower stairs down|
|19,15|20 /19,14 /U|west room door|
|35,15|20 /35,14 /U|east room door|
|255,31|3 /29,2 /D|south exit;255 wildcard X|

WarpIfSetAtPoint needs a matching reached warp marker/coordinate before posting
MAP_EVENT_TYPE1 and parameters. These declarations do not prove route geometry,
layout markers, collision, player input or natural staircase access. No-scroll
ProcessMapEvent clears the event then unwinds exploration to ordinary MainLoop;
SwitchMap/CheckBattle precede rebuild. Map20 has no flag-switched-map row or
battle trigger. Town3 can be replaced by earthquake4 when640 is held, as the
[entry owner](./granseal-epilogue-and-castle-admission.md#ordinary-population-and-interaction-admission)
retains. Nothing in this warp table requires completion of council.

Despite its source constant `MAP_GRANSEAL_CASTLE_1F`, Map20 contains the royal
chamber coordinates; the name alone does not prove floor geometry. Selection
order is default→501→609→506→543, last held wins. F982 is **not** a Map20 setup
selector. Clear543 can select an older setup whose population/init this account
does not claim; held543 wins regardless of the lower flags. Stable543's six
pointers are `$631EA..$63202`, and population `$63202..$63234` is:

| Logical ID | Declaration x,y/facing | Sprite / action |
| --- | --- | --- |
|128|27,51/D|Minister /Init|
|129|20,52/D|Elis /InitFixedSprite|
|130|16,9/U|Young Man /Init|
|131|35,11/R|Guard /Init|
|132|39,56/R|Guard /Init|
|133|22,7/U|Maid /Init|

The ordinary player entry from the requested stairs is23,37/Down, distinct
from each NPC declaration and from the later Minister reposition. With no
persistent followers the NPC slots are1–6; with followers they shift. Fresh
logical128 here is Minister, not Map19's King. F982 changes only this543 init:
`chamber/s6_initfunction_543.asm:$633A2..$633C4` checks982; clear jumps directly
to byte_633BA, held calls cs_633B2 `$633B2..$633BA` then branches there.
The helper is precisely setPos128,27,50/Left then csc_end (two commands/8 bytes).
Its nonally byte bypasses ally-HP suppression. Both paths request MUSIC_TOWN,
call FadeInFromBlack and rts. No F983/F984 write, ending call or kiss occurs
in this init. Map19's current follower/actor slots are not carried as the new
Map20 population. This ordinary entry completion is the stopping boundary.

Only navigation for the next owner: `chamber/s3_zoneevents_543.asm`
`$6329A..$63364` declares19,52→Map20_21F_ZoneEvent0 (later raw YesNo/
cs_EndingKiss→PlayEndingCredits→PlayIntroOrEndCutscene→WitchEnd route),
26,255→ZoneEvent1,27,52→ZoneEvent2, plus default. These three zone bodies,
all indoor entity/description/item events, the kiss and ending implementations
are deliberately unanalysed here; their names are not natural-reach evidence.

## Actual producer and consumer boundary

**Confirmed:** the sole selected producer observation returned without exception,
compiled14 programs and four cached action streams; no compile_maps, final
preparation, selected map package, world publication or runtime execution ran.
The existing receipt retains every instruction/action/native and partial state.

| Actual owner | Support, refusal and publication boundary |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py)|cs_62D0E is230 typed instructions with native roofEvent at typed15/source19. First15 include cursor/camera/positions/follows/wait/fade and Astral motion; they are available prefixes, not observed execution. Later native rejection cannot establish council dialogue reach.|
|Same producer, internal continuation|Clear8 branches to cs_62FB0; held8 falls through via generated jump. Internal program199 instructions begins cursor3987 and has no native; it includes the two hides and animation restart. Availability as a standalone compiled suffix does not bypass the earlier roof stop.|
|Same producer, action streams|eas_Init14, Jump9,2xRightLeft14 and RotateRight14 operations. RotateRight stops source parsing before internal word_45FEC, omitting the original back-branch; it emits one finite L/U/R/D sequence. This is a semantic gap even though no native is emitted. No repair/recompile is performed.|
|Same producer, init wrappers|Optional25/8/27 become branch/hide(removeAliases:false). byte_62CCE has7 instructions: helper call,981/982 branches, royal call, set982, native move.b list reset at5, jump. Other music/fade suffixes retain native sndCom/jsr. Map20 init is3 instructions (branch/call/jump); cs_633B2 is2(position/end-map-script); byte_633BA retains native sndCom/jsr then end. No post-return list reset is silently treated as supported.|
|[Map preparation](../../../../src/sf2tool/remake_exploration_maps.py), [final preparation](../../../../src/sf2tool/remake_exploration_content.py)|Can bind selected setups/entities/events and map roof tables; admitted ordinary warps and unsupported frontiers remain explicit. This observation supplied no maps/layout/materials/final package and did not prove selector or event content publication. Generic resource conversion does not implement script roofEvent.|
|[Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [story types](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Reads typed flag branches/cursor, positions, camera, motion, hides, animation and follows. native-call becomes UnsupportedInstruction. There is no implicit native roof-service or arbitrary RAM-write execution. Text availability and parsing are separate from an admitted543/982 package.|
|[ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|UnsupportedInstruction throws program-opcode before Next/commit; preceding commits survive. BranchFlag uses live flags; cursor and WriteFlag commit individually. Position writes current/destination and facing; motion records entity.Slot timer and optional AND9F, replacing Follower with null; named waits distinguish ScriptIdle/MotionStopped. This is per-instruction publication, not whole-scene rollback. Original byte-actor HP guards on setPos and word-actor guard on startEntity are not reproduced by their typed cases.|
|[FollowerMotion](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FollowerMotion.cs), [EntityMotion](../../../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs)|Typed follow installs physical leader/offsets with source-derived movement flags; no csc2C live Bowie high-byte precheck is emitted. Motion/collision/passability consumers exist, but selected natural scheduling, timing and positions remain Unknown.|
|[ExplorationState](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [allocation](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Hide with removeAliases:true differs from optional out-of-map false. Typed Hide also clears Actions/Follower, unlike the original HideEntity body; Sarah/Kazin have already had named waits before hiding on the normal source path. Current actor mutations and later Rebuild are distinct; generic transfer/allocation cannot make this unprepared content executable or replace the original list-reset write.|
|[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [block-copy reducers](../../../../remake/src/Sf2.Remake.Domain/Maps/MapBlockCopyAction.cs)|Roof/RoofOnLoad apply admitted roof tables using player position and lifecycle state. They do not execute csc43's explicit29,23 request in the story runner or prove visual publication. Similar underlying data operations do not close that seam.|
|[Adapter projection](../../../../remake/game/src/Exploration/ExplorationSessionView.cs), [presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [capabilities](../../../../remake/docs/capability-status.md)|Projects current entity/camera/text/wait state and supports existing input/presentation paths. No royal package, actual service completion, physical player input or Map20 player-ready observation was made. Earlier playable capabilities do not admit this ending segment.|

**Inferred:** existing primitives are reusable after explicit roof/list-reset,
HP-guard, rotation-loop and content-admission work. That is a scoped future
integration decision, not authorization here or evidence of reached gameplay.
No support table, importer, engine, allowlist or native handling was changed.

## Reproduction, retained failures and open boundary

Ignored `local/issue1188/observe.py` was the sole selected lowering. Do not rerun
it in ordinary review. `review.py` reuses observe-01, checks pinned source and
corpus attribution, complete ledger/actions/text/populations/events, source
service order, changed-state cases, actual consumers, links/UTF8/LF, exact
predecessor navigation and two-file diff. It performs no lowering/publication.
Its original PASS missed the description
introduction-ID prose; use the focused correction check below for that chain.
Independent reproduction uses a **fresh nonexistent receipt**, for example:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1188/review.py <configured-source-disasm-root> --receipt local/issue1188/reproduction-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Every uv invocation loads the worktree's configuration in the same process.
Exact command paths, source byte lengths, durations/receipt sizes, committed
head/tree/planner, actual exact-head scoped CI and process state belong in the
frozen Issue/PR and ignored handoff. No fresh ROM observation/blocker is claimed.
Normal/full/SDK/Godot/emulator/H3/H4, screenshots, pair regeneration/full world,
old comparisons/suites and previous recipes are **SKIPPED**, not passed, under
the Issue's explicit documentation acceptance. ADR0004's relevant provenance,
control order/date/width/alias/changed-state/consumer/private checks apply;
new schema/parser tests and H3 callback closure are N/A.

Preserve the [predecessor failure record](./granseal-epilogue-and-castle-admission.md#reproduction-retained-failures-and-open-boundary)
and `local/issue1186/handoff.md`: direct01 destination-write order failure,
direct02 CRLF-vs-Git comparison failure and corrected receipts; raw choice/
setDest native stops and omitted follow HP guard. Preserve #1184's original
PASS that missed physical-slot timer and equipment-refreshed CURSE, independent
P2 findings, focused corrections and earlier corpus/spacing/snapshot errors,
native/partial/failed imports and twelve completed old comparison failures.
No previous check, lowering or failed artifact was rerun, overwritten or relabelled.
This slice's direct01 failed because its local table comparison retained source
CRLF at the end of the captured field. Normalize line endings only in the
local comparison, preserving raw byte sizes and source; direct02 passed. Both
fresh receipts remain. Orientation read failures remain in the local handoff;
the sole selected lowering had no exception. Self-review added the csc14
manual-script timer/terminator boundary and corrected3959's name selector
to Lemon28 from sf2enums rather than relying on a remembered roster order.

Independent review found a P2 after the author's direct-committed-01 and
exact-head CI39830600450 passed: this owner incorrectly called investigation
kinds4/5 text IDs425/426. Those IDs describe a sign/wash basin; the actual
row-byte4 +423 consumer yields427/428 for shelves/well. observe-01 selected
425/426, and equal control tokens did not validate that semantic relationship.
All original receipts and PASS conclusions remain, with this missed finding
explicit. The focused `local/issue1188/focused-descriptions.py` check follows
msDesc operand3 → row byte4 → addi423 → gamescript ID/meaning → corrected
prose, writing a fresh nonexistent receipt. It neither lowers again nor reruns
the full recipe. The local review's unrelated document-length gate was also
removed; the user set no arbitrary document length limit. Neither correction
changes the preceding owner or expands source/consumer scope.
The same independent review found a second P2: this owner's interaction
wrapper summary incorrectly put disabling entity updates before portrait/
speech lookup. The original direct/CI PASS missed that order as well.
The focused check also guards portrait setup and its signed display branch,
physical alias resolution, bit0's optional wait/turn, CLEAR_FLAG before callback,
bit1's optional restore, window closure and update reactivation in source order.

**Unknown:** natural entrance/zone/warp/interaction reach and geometry/bypasses,
live flags/HP/party/aliases/followers/input/inventory, interruption prefixes,
service completion, scheduler/follow motion/timing, source-ROM equivalence,
rendered materials/portraits/audio/roof restoration, saved persistence and
original/remake playability. Map20 later interactions/kiss/credits/ending are
outside this evidence boundary. These Unknowns create no runtime queue.
Freeze a clean pushed Draft PR and stop the writer for independent main-gate
acceptance; same task owns corrections. No merge, cleanup, archival, ref deletion
or next dispatch belongs to this slice.
