# New Granseal Janet 招募与离开

> 本文是[规范英文原文：New Granseal Janet Recruitment and Departure](../../../../synthesis/chapters/northern-travel-mitula/new-granseal-janet-recruitment-and-departure.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

队伍准备离开 New Granseal 时，Janet 找到他们。她询问 Elric，说明自己想陪伴他，即便被警告旅途危险仍坚持同行。队伍接受她；招募宣布之后，场景请求新的局部跟随链。**推断：**这是自愿团聚与接纳，由脚本对话和手势表达。没有 yes/no 菜单或玩家拒绝分支。

**已确认（静态）：**城镇相遇需要两个选定区域（事件）之一的 F805 已设、F806 未设。Janet 的军团成员资格、出战成员资格、分配地图实体、当前跟随、调用方完成与后续可用控制是不同变化。前述[发明家交谈](new-granseal-inventor-and-dynamite-gift.md)是阅读连接：获取 Dynamite 不是 Janet 前提。**未知：**自然可达、传入状态、表现/服务完成、可用控制及存档/读档持久性。解释止于局部返回和必要的重新城镇后果；南出口仅指出下一路线。

## 证据与阅读边界

已接受 Layer A 所有者是 [New Granseal Janet 招募与离开](../../../../../research/chapters/northern-travel-mitula/new-granseal-janet-recruitment-and-departure.md)，经 [#973 / PR975](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/975)接受。它固定 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` 及 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6)。以下源码路径相对其 `disasm/`；Fnnn 表示标志 nnn。路线图拥有原切片的 [Standard 范围协议（英文）](../../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。

[城镇/Peter 指南](../eastern-parmecia/new-granseal-town-through-peter-introduction.md)与 [Astral 返回指南](new-granseal-return-and-astral-following.md)说明既有人口、随从声明顺序与觐见。[成员资格合同](../../../contracts/party/party-membership-state.md)拥有 joined/active 状态与重建时序；[名册合同](../../../contracts/party/party-roster-state.md)拥有地图脚本命令边界。Janet 已接受研究增加选定数值与实际被调用方审查，不扩大这些合同较早的符号或处理器局部范围。研究描述原版；本指南不选择重制行为。

## 在当前城镇找到队伍

**已确认（静态）：**Map6 区域（事件）行 `(19,29)` 与 `(20,29)` 选择 `Map6_ZoneEvent3`。`data/maps/entries/map06/mapsetups/s3_zoneevents.asm` 中 `$54D24..$54D3C` 的完整回调读取 F805，未设则返回；读取 F806，已设则返回；调用 `cs_55288`，设置 F806 并返回。两个拒绝路径都使用 `Map6_DefaultZoneEvent`，即共享最终 `rts`，位于 `$54D3A`，不招募。F806 在场景返回后写入，不在首段文本出现时写入。

谓词不测试 F804、Cannon、Dynamite、F70、Elric 成员资格、HP 或出战容量。剧情关于 Elric 的陈述与早期 Cannon/Dynamite 事务都不增加该测试。传入805/806 可独立于这些历史准入或抑制相遇。

坐标匹配需要真实事件产生器。步行须获准移动，到达布局属性 `$1400` 路径，位置为 `entityscriptengine_2.asm:loc_5294`，提交 event6。待处理请求是单个可变槽；能否保留以及移动/布局条件都是前提。`ProcessMapEvent` 先清除请求类型，区域（事件）处理器才在 physical0 安装 `eas_Init`、加载坐标字并调用 `RunMapSetupZoneEvent`。包装器选择当前配置/表，却不重建行动者。直接调用场景绕过区域（事件）谓词，自身不写调用方806。见已接受的[请求状态](../../../../../research/world/map-event-request-state.md)与[请求消费](../../../../../research/world/map-event-request-consumption.md)所有者。

Map6 有序 default/F701/F702 配置选择，以及独立的 F802 优先、随后 F706 init 仍适用。三个配置共用此区域（事件）表与 init。F802 已设时，init 把132 移到 `(11,18)` Down；否则 F706 把138 移到 `(63,63)`、139 移到 `(15,23)` Down。这些是当前查找写入。805/806 均不选择人口或构建 Janet。Default 在 `(18,25)` Up 有固定 Peter7，除非已有 follower7 映射抑制；701/702 居民列表没有固定 Peter。其139 是**城镇 guard**，分别声明在 `(11,12)` Left、`(13,24)` Up 或 `(18,28)` Up。它区别于城堡 Astral139 和 B31，即使叙事顾问为 Astral。

重新城镇人口在居民和 physical0 之前构建启用的普通随从。F67 提供 Peter；F74/B/Astral、F68/B/Oddler 与后期 F70/B/Astral 都可各分配随从。后期 B 声明覆盖角色查找，不删除更早物理条目。因此仅 F70 未设不证明 B 缺失；仅 F805 不建立 Astral 映射。其他启用随从影响分配/列表顺序。当前行动者可保留更早配置映射，直到另行重建。链接的人口所有者提供完整顺序，不把本文变成城镇目录。

## Janet 请求与队伍接受

**已确认（静态）：**`data/maps/entries/map06/mapsetups/scripts.asm:cs_55288` 有45 条记录、三个内联行动序列，跨 `$55288..$5537E`（246 字节），没有场景分支。以下故事说明完整布置顺序。B 表示随从槽 B（31），P 表示 Peter7，J 表示 Janet18，Bowie 表示 ally0；每个选择器作用于当前物理映射。

场景设置文本游标1126，在 `(16,22)` Down 分配 J。在 B、再 P 上安装并等待 `eas_Init`。Janet 首段文本说找到首领，询问他们是谁。Bowie 与 B 接受不等待的跳跃行动；P 接受跳跃并等待其完成。Guard139 被请求朝 Up，摄像机前往 `(14,17)`。只等待 P 不建立每次跳跃都完成。

Bowie 被送到 `(19,28)`，B 到 `(20,28)`，P 到 `(20,27)`，再请求三者朝 Up。Janet 步行 **Down2, Right3, Down1**，摄像机移到 `(14,22)`。B 转 Left，询问身份，再转 Up。Janet 说 Elric 是爱人，问他是否同行。Peter 说他自愿加入。Janet 摇头，解释自己想陪伴他而非阻止，再恳求带她同行。

Peter 转 Down，场景请求50 ticks，他步行 **Right1, Down1** 后朝 Up。B 警告 Janet 旅途危险。Janet 前进 **Down1**，仍坚持同行。Bowie 转 Right，B 转 Left，接受她，认为她足够坚强。Bowie 点头，Bowie 与 B 朝 Up，Janet 感谢队伍。随后执行 `join J`、三个 follow 调用并结束。警告和接受属于 B 对话选择器，点头属于 Bowie；实际映射仍支配物理行动者。

这些是带明确限制的源码请求。`eas_Init` 设置速度32、大小24，重置加速度，启用阻挡与自动朝向，禁用地图/实体不可碰撞，朝 Up、更新精灵并到 Idle。跳跃禁用自动朝向，请求相对 `(0,-1)`、wait5、`(0,1)`、wait5、`(0,0)`，再恢复自动朝向/Idle。内联行动产生瓦片大小相对移动（384 内部单位），等待被寻址行动指针成为 Idle。绝对 `setDest` 写缩放目的地与有符号 velocity32/travel，等待位置相等；这里没有 HP 守卫。摄像机命令等待滚动请求，选择 VIEW_TARGET_ENTITY=-1；场景没有显式最终摄像机跟踪命令。

映射有效且彼此不同、行动完成时，Janet 首段步行后到 `(19,25)`、末段后到 `(19,26)`；Peter 到 `(21,28)`。这些及更早绝对目的地是跟随前有条件的指令推导位置，不是已观察或通用最终坐标。既有移动、碰撞、别名与被替换行动可改变完成。点头请求带10/20/10 睡眠的精灵/DMA 变换；摇头请求七组配对变换/恢复，每次迭代六次 VInt 等待。两种手势均不验证身份、不治疗行动者，也不汇合所有移动。

## 对话、分配与变化行动者

十段连续场景文本是1126–1135（`0466..046F`）；前文转述已接受精确字面量。1126 的 `LEADER` 在重建军团列表后命名首个出战成员，未必是 Bowie 或 physical0；这里没有出战列表为空的守卫回退。1128 的 `NAME;13` 读取 Elric 显式己方名字记录，不测试是否已加入。重命名影响替换。Peter 关于 Elric 的陈述仍是对话。

文本通常使用 `nextSingleText`，仅1130 使用 `nextText`。前者显示后关闭立绘与对话，睡眠10；后者保持窗口供1131 接续。换行控制仍是换行。W1 请求 VInt 后等待方向/A/B/C 输入；W2 使用继续/确认音效路径。Janet 修饰符为 `$00`；B/P 用 `$C0` 选择右侧/镜像立绘。Speech/portrait 来自**实际映射精灵**，不替换已打开立绘：Janet45/46/47 选 portrait18/bleep4；Peter18/19 选 portrait7/48、bleep4/5；Astral209 选 portrait31/bleep6。Blue Flame 没有行，使用 portrait-1/bleep6。别名 B 以实际映射上下文说话；其选择器不证明 Astral 在场。

分配自身无条件。`newEntity` 调用 `InitializeNewEntity`，使用 `eas_Init`，不检查既有 Janet、joined 状态、HP 或容量。对 ally18，传入 sprite byte18 被 Janet 职业相关 mapsprite45/46/47 替换，战斗外 HP0 则为 Blue Flame190。它不保证 Peter 的 sprite18，也不初始化/治疗 Janet 战斗员记录。分配器扫描63 个查找字节，取最大正有符号物理索引加1，覆盖 Janet 查找，声明/更新新实体。它不寻找空闲物理槽，也不移除旧 Janet。再入可遗留旧物理行动者或随从列表条目。

共享查找以 `$FF` 掩码选择器，把负引用经 NPC/敌人偏移 `$60` 映射，再读取物理索引并乘32。清除的普通 lookup0 别名到 physical0；隐藏 -1 没有安全缺失检查。不拒绝缺失或别名 B/P。多个名字可寻址同一物理行动者，让后续目的地/行动替换前者。这些路径既非安全空操作，也不保证场景终点。

HP 守卫也不同。`setFacing` 读取实际行动者**字节**，跳过30 以下零 HP 命名己方角色；B31 与 NPC139 绕过。分配、行动安装、目的地、内联行动、点头与摇头没有该守卫。因此死亡 Janet 可被分配、移动、交谈并加入，却不复活。相对地，每个 `followEntity` 在随从字**之前**读取一个字节。`$0007`、`$001F`、`$0012` 都以0 开头，故三个调用均测试 **Bowie 的 HP**。Bowie HP0 跳过各六字节操作数组；存活 Bowie 则不管 Peter/Janet 自身 HP 而准入。

解释器入场清除跳过锁存；debug P2 Start 可设置。跳过普通场景文本消费操作数，不显示或递增游标，50-tick 等待也跳过。分配、行动、手势、join、跟随与调用方806 不被抑制。跳过游标推进后的变化文本归属不能写成普通可见对话。直接加入宣布另有下述路径。

## 加入、宣布与出战容量

**已确认（静态）：**`join` 重置 speech0，等待滚动后读取字 `$0012`。它清除/测试 bit15，因该位未设而选择普通 MUSIC_JOIN19。这不是特殊128 Sarah/Chester 形式。它为 Janet18 调用 `JoinForce`，读取当前职业，写名字/职业替换索引并直接显示文本446（`01BE`）：NAME、CLASS 与加入军团宣布。`DisplayText` 重置替换指针后，NAME 再 CLASS 消费这两个索引。文本446 有换行但无 W1/W2。宣布不初始化属性值、职业、物品、AI 或 HP，也不保证未来战斗槽位。

实际成员顺序为 joined flag18 → `UpdateForce` → 与12 比较无符号出战计数 → 条件 `JoinBattleParty` 设置 active flag50。`UpdateForce` 扫描已加入 allies0–29 到 targets 与带计数出战/后备列表，不管 HP。后续 active 位写入不再重建列表；标志可比缓存出战/后备前缀更新。下一次重建是独立事件。

已有12 名出战成员且 Janet 未出战时，她仍以后备加入。已加入 Janet 不被拒绝，会再获宣布；未出战且有空间则成为出战成员。已出战 Janet 即使达到或超过容量也保留该位。未加入 Janet 若 active50 预设，立即进入重建出战计数。不清除任何既有 active 位，不修复不一致传入状态。出战成员资格不支配后续跟随。

446 后，`j_FadeOut_WaitForP1Input` 请求先前音乐 `$FB`，使用 `PlayMusicAfterCurrentOne`，提交 WAIT_MUSIC_END，以 Sleep3 轮询，再调用一次 `WaitForPlayerInput`。它保留/恢复 d0.w。按住方向、A/B/C/Start 满足输入，不要求新边沿或松开。虽名含 FadeOut，没有调色板淡出。Join 随后关闭对话、睡眠10 并返回。这是**一个**持有输入服务，区别于发明家收取辅助的两个。军团成员资格先于宣布、音乐与输入完成；均不建立可见确认或持久存档完成。

## 当前跟随与离开交接

**已确认（静态）：**Bowie HP 守卫准入后，每个调用在当时解析两个名字为物理行动者。依次请求 **P→Bowie、B→P、J→B**，采用 direction2 的有符号像素偏移 `(-24,0)`。它们命名依赖，不是最后列表条目、相邻或固定坐标。

`AddFollower` 把当前步行参数缓冲区安装为行动指针，复制42 字节 `eas_Follower1`，推进缓冲区，在 `$1E/$20/$22` 修补物理首领与偏移。然后才扫描探索列表到 -1 终止符。已有物理随从不追加/重排；新随从追加并带新终止符。安装先于去重。额外随从与旧 Janet 条目保留；没有列表容量修复、自跟随拒绝或环守卫。B 别名 physical0 可在 P 跟随 Bowie 后，让 Bowie 跟随 P；B/P 别名可把首个行动替换成自跟随。后续控制重置可替换 physical0 行动，却不修复列表。

该行动设置速度32、零加速度、禁用阻挡并启用地图/实体不可碰撞，再循环跟随/等待目的地。实际 `esc03_follow` 消费者保留存储的物理首领。最大轴距≤30 像素时，它推进而不改目的地；大于该距离时使用首领目的地/朝向，旋转偏移并测试布局 `<$C000`。它不带相对 Y 重试，再回退首领目的地；此处 Y 已是0，前两个候选相同。它写目的地/速度/行程，在那里等待后循环。不把行动者瞬移成一列，也不稍后重新解析名字。完成与最终坐标仍有条件。

场景结束时，对话若仍打开则有条件等待滚动，清除 VIEW_SCROLLING_SPEED 并恢复寄存器。调用方随后设置806。区域（事件）包装器关闭立绘/对话，等待一个 VInt，再等待 **physical0** 位置等于目的地。它不等待所有随从，也不像发明家实体事件包装器那样关闭/开启 VInt_UpdateEntities。既有更新服务必须继续，行动才能完成。

探索随后调用 `WaitForEvent`。仅无待处理请求时，才恢复 camera target0，并在 physical0 安装模式选择的受控行动。待处理请求绕过该重置；场景不规范化 PLAYER_TYPE。场景返回、806、包装器返回与可用控制是独立边界，没有原子存档事务。

普通重新 Map6 入场重建居民/随从，应用相同配置/init 与已接受探索治疗限制。没有居民列表或普通随从行声明 Janet。因此 joined18、active50 与806 不重建其临时城镇行动者或链。F67/F70 及其他随从标志重建各自顺序；805/806 不修复缺失67/70。这是重新城镇事实，不声称 Janet 离开军团或在所有其他地图消失。

| 变化状态或中断前缀 | 局部后果 |
| --- | --- |
|805 未设，或805 已设/806 已设 | 无场景/新806；不修复 Janet 成员资格、实体或跟随缺失。 |
|805 已设/806 未设，804 未设，无相关物品/Elric | 场景仍获准；无物品栏搜索或消费。 |
|Janet 已分配/已加入/死亡 | 仍尝试新分配与加入宣布；旧条目、替换映射与 HP 相互独立。 |
|Bowie 死亡；或 Bowie 存活而 P/J 死亡 | 前者跳过全部 follow；后者准入。朝向仍用独立行动者字节守卫。 |
|分配后、join 前中断 | 当前 Janet 可存在而无 joined18/806；重试可再分配。 |
|joined18 后、active50/宣布返回前中断 | 成员资格、缓存与确认可不一致；806 仍未设。 |
|follow 调用之间，或全部之后、806 前中断 | 跟随可缺失/部分/已安装，不抑制后续合格重放。 |
|806 后、包装器/控制返回前 | 即使窗口、移动或可用控制未完成，也抑制重放。 |
|806 已设后重新城镇 | 不重放招募，也不单凭军团标志重建 Janet。 |

场景有意写 joined18 与条件 active50，再由调用方写806；没有物品/HP/复活、传送、战斗、Egress 或随从解锁标志命令。普通服务状态仍变化。独立南传送 `(any,31)` → Map73 `(29,53)` Down、无滚动仅指出下一路线；场景不发布它。Battle24 入场/名册、North Cave 爆破/Rohde 与后续旅行/交战属于独立成果。

**未知：**自然区域（事件）可达；传入标志/模式/军团/HP/职业；分配、映射/容量与随从列表有效性/顺序；移动/碰撞、DMA/文本/立绘/音乐/输入完成；可见招募、可用控制与存档/读档持久性。这些限制不创建运行时队列或重制要求。

接续 [New Granseal 离开至 Battle24 准入](new-granseal-departure-through-battle24-admission.md)，了解独立南出口/向东滚动路线、条件准入与重新初始遭遇。Janet 招募不是该路线前提。

## 证据审查与保留结果

本综合对抗检查完整已接受 #973 正文与前序边界，对照选定公开证据：`sf2-map-events-static-v1`（[zone shard](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)）、`sf2-map-init-static-v1`（[fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json)）、`sf2-map-script-engine-static-v1`（[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)），以及 common-stats/roster 所有者。其 schema 与实际消费者保留区域（事件）分支极性、共享返回、配置/init 顺序、编码宽度、join 音乐/名字/职业/调用顺序，以及随从字前读取字节守卫。通用名册观察是处理器局部边界，不是 Janet 场景观察。较早符号合同未知项不否定 #973 选定被调用方审查，也不授权全局合同编辑。选定边界内未发现证据矛盾。

所属复现命令为 `uv run sf2 h2 map-events`、`uv run sf2 h2 map-init`、`uv run sf2 h2 map-script-engine` 与研究所有者保留数据对/消费者配方。原综合切片工作树缺少固定上游/数据对；不声称采集、重新生成或新比较。公开 schema/fixture 检查不复现保留的45 记录/246 字节源码/语料比较或自然服务完成。

[#973 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/973#issuecomment-6180052225)与 [PR975 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/975#issuecomment-6180065642)保留 head3e907b8f/tree5bc75a39/base36b0026b、CI38648632852（scope19s、research40s）、作者0.093s 与独立0.096s 的不变数据对回执、全部45 记录/246 字节/十段场景文本加446、修正的 `(pc)` 别名检查及 HTTPS/私有 regex 诊断，以及链接的前序结果。[#974 更新交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/974#issuecomment-6180123789)与 [PR976 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/976#issuecomment-6180132098)保留原始92af6c79/CI38648796686（scope15s）、最终00791e35/treebbf88d2a/basecdbcccba/CI38649272542（scope11s）、521 链接与独立审查。其基线/独立范围措辞更新不是研究修正；修正的 KeyError 检查仍保留。前序的[保留审查](new-granseal-inventor-and-dynamite-gift.md#证据审查与保留结果)保留 #969/#970/#965 结果与失败、Battle19/32 字节步长、#954 回调与 #962 Fairy 修正及链接未知项。不重置已完成失败，也不称为中断。

原 Issue 交接记录直接完整正文/差异、导航/链接/锚点、UTF-8/私有/精确所有权检查及 `git diff --check`、同进程配置后的干净已提交 planner 与实际精确 head CI。文档政策使用直接检查，不要求全套套件或翻译。证据矛盾返回研究所有者；独立验收仍由 main-gate 负责。
