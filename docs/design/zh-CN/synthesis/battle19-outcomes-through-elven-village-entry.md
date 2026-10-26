# Battle19 结果与独立 Elven Village 入场

> 本文是[规范英文原文](../../synthesis/battle19-outcomes-through-elven-village-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle19 即使没有战斗专属场景，结束时仍可改变出战队伍、战斗和返回状态。胜利使队伍返回 Southwest Parmecia，位置取自当前出战名册。败北或获准逃脱使用传入存档点。Elven Village 有独立入口：选定的 Southwest Parmecia 传送不要求 Battle19 胜利。

**已确认：**已接受的[研究所有者](../../../research/battle19-outcomes-through-elven-village-entry.md)确立选定静态路径、共享使用方及村庄初始实体生成。**推断：**它们解释战斗结果与村庄抵达这一段。**未知：**自然行程、实际调用方状态、服务完成、响应控制和持久性。未确立组合运行时结果或必须先胜利再入村的顺序。

此前独立的步行请求／准入见 [Caravan 旅行](caravan-boarding-and-southwest-parmecia-travel.md)，F65/F84 见 [Battle18 获取](battle18-outcomes-and-caravan-acquisition.md)，仅当前跟随见[可选 Rick 招募](hassan-rick-recruitment.md)。本指南止于紧接的结果返回或条件 Map23 控制。村庄对话、搜索、宝物、楼梯、Map24/Battle20、后续旅行及 F762 的生产方均不在范围内。Fnnn 表示标志 nnn；表索引从零开始。坐标是源码操作数，不是观察到的位置。下文源码符号沿用已接受研究所有者的固定溯源。

## 场景目标为空的战斗入场

**已确认：**Battle19 是 Southwest Parmecia68 上的 BATTLE_OUTSIDE_ELVEN_VILLAGE19。唯一的 Map68 战斗坐标行提供原点 `(5,19)`、大小 48x32 和通配触发 `(255,255)`。CheckBattle 检查 F419 及这些触发字节；矩形不是额外的包含门槛。若 F519 已设置，选择会清除 F419，但不取消已选入场。Caravan 所有权、Battle18 完成、F762、Sword 持有、成员资格和运输模式在此均不增加谓词。步行的独立区域生产方及传入 F419 准入仍由前一指南负责。

BattleLoop 首先设置 PLAYER_TYPE=0 和 F399，包括传入 F88 恢复。全新入场清除秒数，临时选择 map68/battle19，安装基础 VInts 并调用 before 包装器。它恢复传入地图／战斗，清除区域标志90–105，治疗符合条件的己方角色，初始化战斗员，清除 AI 内存，以 d0=0 调用 LoadBattle 并调用 start 包装器。之后才是初始激活／区域／生成／行动顺序工作；可用的首次回合（单次行动）仍有条件。

`data/battles/cutscenes/` 中的四个 slot19 指针均选择 `ms_Empty`，位于 `0x47A4E`，仅含 `$FFFF`。它们不提供 Battle19 对话、自定义行动者移动、传送或剧情招募。各包装器仍有独立效果：

| 钩子 | 选定门槛与共享工作 |
| --- | --- |
| 战前 | F469 已设置则跳过脚本；未设置则执行空目标，不设置 F469。 |
| 战斗开始 | F469 已设置则跳过；未设置则先设置 F469，再执行空目标。 |
| 敌人被击败 | Bowie 必须存活且 enemy128 死亡。F519 未设置则分发空目标；已设置则跳过。两个符合条件的分支均到达首领尾部。 |
| 战后 | F519 未设置则分发空目标；已设置则跳过。两个分支均到达 EndAfterBattleCutscene 及其加入表。 |

脚本读取器还保留共享进入／终止工作，包括跳过状态初始化和条件滚动等待／摄像机速度清除。内容为空不会移除这些服务。F469 独立于 F519，败北或重复不会重置它。

F88 已设置时则恢复已保存秒数，清除 F88/AI 内存和视野目标，调用 LoadBattle 并直接跳入各自回合（单次行动）循环。它绕过传入地图／战斗赋值、全新治疗／放置、before/start 及初始回合生成，也没有全新入场显式的 d0=0。已保存地图、战斗、区域、名册和行动状态必须一致；仅传入19参数不能确立一致的恢复遭遇。

### 初始名册与重复遭遇

**已确认：**`0x1B46BE` 处的 BattleSpriteset19 含 12 个己方放置、15 个敌人行、三个 AI 区域及一个 AI 点。首个己方放置是战斗局部 `(40,12)`，加上区域原点为 `(45,31)`。它既不是村庄入口，也不是固定胜利返回。

己方初始化清除全部 30 个己方 X 值并重建当前出战成员。死亡成员消耗一次列表迭代，但不推进放置行；超额成员可能离开出战队伍。因此十二个放置不能证明有十二名存活参与者。治疗将存活己方角色与不死的 Peter/Lemon 恢复至 HP/MP 上限，仅保留已有 STUN/POISON/CURSE 位。它不会复活所有死亡己方角色，也不授予成员资格／跟随。

| 敌人槽位 | 按槽位顺序的基础行 |
| --- | --- |
| 128–130 | ORC, HIGH_PRIEST, SOULSOWER |
| 131–133 | GARGOYLE, HIGH_PRIEST, ORC |
| 134–136 | DARK_MADAM, SOULSOWER, GARGOYLE |
| 137–139 | ORC, ARROW_LAUNCHER, SOULSOWER |
| 140–142 | SOULSOWER, ARROW_LAUNCHER, GARGOYLE |

全部 15 行均为 STARTING。Enemy128 具有 LEADER AI 和 MOVE_TO point0 `(14,19)`；enemy129 携带 SHINING_BALL|USABLE_BY_AI|UNUSED_ITEM_DROP。这些是初始操作数，不是战术或观察到的掉落／使用。32 个敌人槽位中的其余槽位走隐藏／清除初始化路径。

Battle19 列于 `randombattles.asm`。F519 已设置时，DetermineBattleUpgrade 在定义及行复制前接纳现有 movetype/level/class/random 算法。它可以保留或改变基础身份；仅有 F519 不能确定升级后名册。F469 不选择该升级。此处不计算名册，也不模拟交战。

LoadBattle 清除行动者／查找表并声明已放置战斗员。中立表没有 Battle19 条目：Taros 场景中的 Caravan 不会带入本次遭遇。探索随从与 Rick 的当前列表扩展同样不会保留为战斗行动者。地图／精灵／地形、VInt、音乐／淡入淡出及视野服务仍先于条件战斗控制。

## 结果判定前的首领清理

**已确认：**各自行动之后，BattleLoop 运行 defeated 钩子、处理被击杀战斗员并计数双方。己方为零优先于敌方为零。若两者均未结束战斗，回合（单次行动）后效果还会经过一次击杀列表／计数检查，仍先判败北，但不再次调用 defeated 钩子。计数要求 X 非负且 HP 非零；Bowie 的 HP 为零则强制己方计数为零。

对于符合条件的 Bowie 存活／enemy128 死亡钩子，`enemyleaderpresence.asm` 的 row19 为 -1。两个 F519 分支都将 32 个槽位中所有 HP 非零的敌人追加到 DEAD_COMBATANTS_LIST，带 -1 尾部。追加不检查放置，也不直接将其 HP 归零。ProcessKilledCombatants 执行死亡表现／记账，设置所列 X/Y=-1，清除状态，刷新属性值，并将映射行动者移至原始 `(0x7000,0x7000)`。它们因此离开已放置存活计数。Enemy128 死亡是有条件的清理触发；Bowie 同时死亡时不保证胜利。AI 名称 LEADER 不是此清理的权威；首领存在表才是。

ApplyPositionsAfterEnemyLeaderDies 仅选择 Battle05。Battle19 不接收该表的位置／HP 写入，也没有分阶段结束位置来替代其返回种子。

## 各结果保留或改变什么

**已确认：**以下为有序选定使用方，其服务仍有条件：

| 结果 | 共享结果与紧接的返回 |
| --- | --- |
| 首次胜利，F519 未设置 | 治疗符合条件的己方角色，预置返回，执行空 after 目标，加入 Bowie，清除 F419／设置 F519；d4=1。全新 Map68 返回。 |
| 重复胜利，F519 已设置 | 相同返回种子、加入及完成写入；跳过 after 脚本；d4=1。全新 Map68 返回。 |
| 败北 | 更新解锁、疲惫音乐／文本、恢复 Bowie 的 HP、以无符号向下取整除法将金币减半、选择存档点；d4=-1。Battle4 的特殊败北不适用。 |
| 获准的 Egress / Angel Wing | 消耗 MP／移除所选物品槽位，文本／闪光／关闭、更新解锁、展开动作栈帧、选择存档点；d4=0。不执行胜利加入／完成，也无败北金币惩罚。 |

逃脱使用方检查行动种类及掩码后的法术／物品身份。这不证明菜单可用、MP 充足或持有物品。UpdateBattleUnlockedFlag 在 F519 未设置时保留 F419，在 F519 已设置时清除它。败北／逃脱既不设置 F519，也不重置 F469。这些选定 Battle19 路径均不授予新招募、不设置 F65、不清除 F84，也不解锁 Battle20。

### 胜利在加入前按当前名册返回

**已确认：**BattleLoop_Victory 重建出战列表，以当前首名成员预置返回：CURRENT_MAP、该战斗员的 X/Y 加战斗区域原点（字节运算），以及其映射实体的朝向。Parameter1 为 0。此成员不必是 Bowie 或首名存活己方角色，位置也不必仍是初始放置。不一致成员资格／查找映射没有已确立的修复。一致的 Battle19 状态给出 Map68、局部 X+5/Y+19 及采样朝向。

空 after 目标保留该元组。首次／重复路径均到达 EndAfterBattleCutscene；实际使用的 `data/battles/cutscenes/afterbattlejoins.asm` 的 row19 为 0。s07 include 使用此表，而非名称相近的旧全局文件。其 `j_JoinForce`/JoinForce(0) 调用设置 Bowie 的加入 F0，重建军团列表，并在出战数量小于 12 时激活他。这发生在返回成员选择之后。它不是村庄招募，未使用功能的注释也不能抹去此调用。随后胜利清除 F419、设置 F519 并读取元组；F469 保持已设置／传入状态。

直接结果返回使用 SwitchMap → ExplorationLoop，不经过 CheckBattle。没有 source68 的 switched-map 行替换地图。全新探索清除待处理状态，从 STEP_COUNTER 字减去 20000 并对负结果钳位，治疗符合条件的己方角色，淡出／加载，设置 CURRENT_MAP=68 和 CURRENT_BATTLE=255，重建行动者，清除临时标志 256–383，设置 F80 并运行 init。它既不保留战斗实体群，也不立即重新扫描重复战斗。

Map68 有一个配置、空固定实体列表及仅返回的 init。在这张大地图上，F65 已设置则选择以 physical 1 的 Caravan/FOLLOWER_A 开头、随后是获准普通随从的表；F65 未设置则仅选择普通随从。Peter/F67、Oddler/F68、Rohde/F84 及其他标志仍为传入值；胜利不修复它们。后续冲突 FOLLOWER_A 行可覆盖其查找映射。普通战斗入场已经选择步行模式 0，因此 F65 不会自动登乘 Caravan。F64 及已保存木筏坐标保留；已保存地图匹配68时可另行声明木筏。

init／音乐／淡入淡出之后，待处理为零才允许跟随 main 0 并安装当前模式控制，通常是步行 0。新的区域请求或村庄传送需要独立生产方／准入。胜利返回本身不进入村庄。止于条件 Map68 控制。

### 败北或逃脱使用传入存档点

**已确认：**Battle19 没有特殊 egress 返回。普通 BattleLoop 设置 F399 后，GetSavepointForMap 使用 EGRESS_MAP 及其所选存档点 X/Y／朝向。缺失行保留该地图并使用 `(1,1)`、Up。异常 F399 未设置的直接辅助调用会在扫描存档点／木筏前返回初始 Bowie 房间元组；它不验证任意继续。

F64 未设置则保留已保存 RAFT_MAP/X/Y。F64 已设置则按所选存档点地图重置它们：Hassan9 → Map69 `(28,55)`；Polca13 或 Bedoe10 → Map72 `(43,48)`；其他所有选择均使用哨兵默认 Map69 `(42,15)`。这些已保存写入既不登乘木筏，也不改变控制器。Battle19 结果使用方不会再次规范化模式；普通入场已设置 0。

传入 EGRESS_MAP=23 时，返回 Elven Village `(61,4)`、Up。它不同于独立入口 `(22,29)`、Right。F64 已设置会把已保存木筏重置在69上，因此不会将其放入23。直接结果入场仍绕过 CheckBattle，以存档点元组使用下述全新 Map23 实体生成／init。这既不证明玩家上次在哪里存档，也不证明任何教堂／NPC 激活。

其他存档点保留自己的 SwitchMap／配置／init 继续，此处为**未知**。它们不必共享 Map23 的仅返回 init，也不必产生可用暂停。传入 map255 则走既有实体群分支：更新主实体属性／木筏并加载／init，不生成全新行动者，也不清除临时标志。此处不提供自然败北行程或全部地图继续。

## 村庄入口独立于胜利

**已确认：**Map68 选定传送 row2 匹配 `(48,30)`，请求无滚动 Map23 `(22,29)`、Right。此前的 `(55,43)`、`(57,24)` 及后续边界行不在该点竞争。WarpIfSetAtPoint 将传入坐标除以 384，按顺序扫描带负字节通配的行，并写入 event1、参数及 WARP_SFX。它不检查 F519/F419/F65/F762 或成员资格。步行／载具使用方在移动检查后通过其 `$1000` 布局分支到达此辅助程序；匹配不证明地形可达、运输资格或从胜利种子步行抵达。

ProcessMapEvent 在分发前清除待处理状态。无滚动使用方提交／清除 WARP_SFX，使 main 0 空闲，移除探索返回栈帧，采样位置并应用非负的请求坐标／朝向。它返回 `(23,22,29,Right,d4=0)`，不治疗、不重建行动者、不改变 PLAYER_TYPE，也不保存木筏位置。随后 MainLoop 使用普通 SwitchMap → CheckBattle 准入。

完整 switched-map 表无 source23 行，完整 45 行战斗地图表无 Map23 行。Battle19 在68；row44 在24。F419/F519 或其他解锁不能在未改变的23元组上选择战斗。F762 改变配置选择，不改变此地图／战斗扫描。因此入场没有 victory19 前提，但自然的胜利前步行仍为**未知**。替换的待处理／地图请求是独立传入条件；这不是原子式保证转换。

## 全新 Elven Village 抵达初始化什么

**已确认：**全新探索写入 CURRENT_MAP=23/CURRENT_BATTLE=255 并使用上述共享顺序。Map23 默认为 ms_map23，且恰有一个替代配置 F762 → ms_map23_flag762。F519 不选择实体生成。两份六条目指针表共享区域／描述／物品及 init 指针，但选择不同的实体／实体事件指针。抵达绑定这些事件，不执行它们。F762 是传入的后续剧情选择器，不构成解释其生产方的授权。

### 居民、随从与行动者身份

`0x59AF0` 处的默认实体列表与 `0x59B22` 处的替代列表以 msEntitiesEnd 终止：

| 配置／逻辑 NPC | 初始瓦片、朝向 | 地图精灵／初始动作 |
| --- | --- | --- |
| 默认128 | `(18,9)`、Down | MAN194；行走原点 `(18,9)`、范围1。 |
| 默认129 | `(17,11)`、Right | DWARF226；eas_Init。 |
| 默认130 | `(18,11)`、Left | OLD_MAN198；eas_Init。 |
| 默认131 | `(54,3)`、Down | WORKER202；eas_Init。 |
| 默认132 | `(8,12)`、Right | ELF_FEMALE224；行走原点 `(8,12)`、范围1。 |
| 默认133 | `(61,3)`、Down | PRIEST192；eas_Init。 |
| 替代128 | `(18,9)`、Down | MAN194；行走原点 `(18,9)`、范围1。 |
| 替代129 | `(56,6)`、Down | WORKER202；行走原点 `(56,6)`、范围1。 |
| 替代130 | `(18,11)`、Up | OLD_MAN198；eas_Init。 |
| 替代131 | `(61,3)`、Down | PRIEST192；eas_Init。 |

msWalkingEntity 编码 `$FF` 及原点／范围操作数。DeclareNewEntity 以这三个补丁将该字段解析到 50 字节的 eas_Walking 模板；固定行安装 eas_Init。坐标、目的地及零速度／行程是初始值，不是冻结姿势或已完成移动。此处不评估 NPC 交互或随机行走轨迹。

InitializeMapEntities 清除物理行动者／查找表，从 physical 1 分配随从，再分配六／四名 NPC，并在传入元组处单独声明 Bowie/main 0，带 eas_Idle 及其职业／HP 精灵。仅随从进入全新 EXPLORATION_ENTITIES；main 0/NPC 不进入。有 n 名随从时，NPC 从 n+1 开始：logical128 起并非物理槽位号。Rick 没有全新随从行。

Map23 既非排除随从地图，也非大地图，因此无论 F65 如何均使用普通随从表。其标志／行顺序为 F66 Sarah；F67 Peter；F72 Kazin；F73 Slade；F74 Astral；F66 Chester；F68 Oddler；F75 Luke；F69 仙女；F70 Astral；F74 国王；F71 Lemon；F74 大臣；F76 Zynk；F77 老人；F82 Frayja；F83 国王；F83 大臣；F84 Rohde。准入逐个检查标志，没有 HP 或军团成员资格门槛。精灵选择另行考虑职业／HP 及 Rohde 成员资格；战斗外死亡己方角色选择蓝色火焰。

仅 F65 不会在此添加 Caravan 随从，也不被清除。其他随从标志保持传入；冲突逻辑行可以覆盖查找映射，而此前物理行动者保留。过多传入行的容量修复未确立。仅有 F67/F68 时，Peter 为 physical 1，Oddler 的 FOLLOWER_B 为 physical 2，列表 `[1,2,-1]`；默认 NPC 占 3–8，替代 NPC 占 3–6。这是传入状态示例，不是自然名册。传入 F69 不能表明它由 F762 产生。

### 运输、仅返回的 init 与条件控制

入口及配置／init 均不清除 PLAYER_TYPE。全新实体生成对 mode 2 的特殊处理使 main 0 使用木筏精灵，将当前所列随从移至原始 `(0x6000,0x6000)` 并安装站立动作，同时在 `(64,64)` 声明独立 physical 31/lookup 63 木筏，不改写已保存 RAFT_MAP/X/Y。NPC 不在此隐藏列表内。其他模式下，F64 加已保存 map23 会在已保存坐标声明独立木筏；否则该尾部不添加木筏。仅 mode1 不会重放 Caravan 登乘或恢复其精灵。

实体生成后，清除临时标志 256–383 并设置 F80。F762/F519/F65/F64 及随从标志保留。LoadMap／基础 VInts 先于为 init 重新解析配置。两个选择均使用 `0x59C04` 处仅含 `rts` 的 ms_map23_InitFunction：那里不提供场景、请求、移动、重定位、模式改变或招募。

条件音乐／淡入淡出之后，WaitForEvent 在安装控制前检查待处理状态。待处理为零则跟随 physical 0 并选择数字模式使用方：0 → eas_ControlledCharacter/opcode2；1 → eas_Raft/opcode7，实际为 Caravan；2 → eas_Caravan/opcode8，实际为木筏。其他非零且非1的值落入最后脚本，没有已确立的有效性修复。源码载具标签不能覆盖此映射。

普通战斗结果带着 BattleLoop 的 mode 0 抵达。独立入口保留传入模式。因此实体生成本身不能确立步行、已登乘 Caravan、有效木筏状态或可用输入。待处理请求优先于控制器安装；服务及异步初始动作仍有条件。在村庄交互或离开之前止于此边界。

获准抵达后的可选局部内容见 [Elven Village 对话与局部探索](elven-village-conversations-and-local-exploration.md)。该指南负责对话、搜索及内部楼梯；本指南保留入场／控制停止边界。

## 证据追踪与审阅限制

**已确认溯源：**已接受 #856/PR857 绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[研究复现所有者](../../../research/battle19-outcomes-through-elven-village-entry.md#reproduction-and-remaining-limits)保留源码／表／include 连接及未改变的 map-init/script/cutscene 比较。本指南使用已接受证据；不重新调查源码或开展新采集。

| 解释边界 | 合同与精确测试夹具所有者 | 结论限制 |
| --- | --- | --- |
| 空目标、包装器门槛、首领／加入尾部 | [过场路由](../contracts/battle-cutscene-routing.md)；[包装器](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)中的 `sf2-battle-cutscenes-static-v1`、expected.cutsceneFacts；[路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)中的 `sf2-battle-routing-data-static-v1` | routeTargetsParsed=false；聚合加入标签／占用不闭合实际 s07 include 或选定 row19。已接受研究提供四个指针、首领 -1 及实际使用的 join0。 |
| 全新／恢复、计数、结果及成员资格 | [战斗生命周期](../contracts/battle-control-lifecycle.md)、[军团成员资格](../contracts/party-membership-state.md)；[循环](../../../../tests/fixtures/h2/battle-loop-static-v1.json)中的 `sf2-battle-loop-static-v1`、expected.lifecycleFacts；[控制](../../../../tests/fixtures/h2/battle-control-static-v1.json)中的 `sf2-battle-control-static-v1`、expected.controlFacts；[属性值](../../../../tests/fixtures/h2/common-stats-static-v1.json)中的 `sf2-common-stats-static-v1`、expected.statsFacts.party | 共享算法不是游玩过的 Battle19、一致恢复、实际种子／名册或菜单准入。选定的返回先于加入及逃脱／存档点分支使用已接受研究。 |
| 选定初始战斗行／重复 | [遭遇定义](../contracts/battle-encounter-definition.md)；[精灵集](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)中的 `sf2-battle-spriteset-data-static-v1`、expected.facts | rowContentTracked=false。计数不序列化完整选定 spriteset19，也不计算升级敌人；这些静态选择归研究所有者。 |
| 入口、竞争地图／战斗行、存档点及控制 | [入场路由](../contracts/map-entry-routing-state.md)、[探索流程](../contracts/exploration-control-flow.md)、[地图探索](../contracts/map-exploration.md)；[地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)中的 `sf2-common-maps-static-v1`、expected.mapFacts；[流程](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)中的 `sf2-gameflow-core-static-v1`；[请求消费](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-event-request-consumption-static-v1`、eventRequestConsumption | 共享扫描／轮询／清除结构不序列化选定私有表，也不证明生产方到使用方的运行时路径。不存在 Map23 战斗行及两个不同抵达元组使用已接受研究。 |
| 默认／F762 实体生成及 init | [配置](../contracts/map-setup-data.md)、[实体](../contracts/map-entity-data.md)；[选择](../../../../tests/fixtures/h2/map-setup-static-v1.json)中的 `sf2-map-setup-static-v1`、[行动者](../../../../tests/fixtures/h2/map-entities-static-v1.json)中的 `sf2-map-entities-static-v1`、[init](../../../../tests/fixtures/h2/map-init-static-v1.json)中的 `sf2-map-init-static-v1` | Init expected.primarySourceBodies sourceOrder38 是一个 rts；expected.routeJoins sourceOrder59/60 及 expected.pointerTableJoins sourceOrder62/63 共享它。共享宏／实体生成事实不证明实际标志、初始动作完成或村庄事件。十个选定 NPC 行及紧接的随从／模式使用方采用已接受研究。 |

schema／使用方配对为 [过场 schema](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../../src/sf2tool/h2/battle_cutscenes.py)、
[路由 schema](../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../../src/sf2tool/h2/battle_routing_data.py)、
[循环 schema](../../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py)、
[控制 schema](../../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../../src/sf2tool/h2/battle_control.py)、
[属性值 schema](../../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../../src/sf2tool/h2/stats.py)、
[精灵集 schema](../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)/[battle_spriteset_data.py](../../../../src/sf2tool/h2/battle_spriteset_data.py)、
[地图 schema](../../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../../src/sf2tool/h2/maps.py)、
[流程 schema](../../../../schemas/h2-gameflow-core-static-fixture.schema.json)/[gameflow.py](../../../../src/sf2tool/h2/gameflow.py)、
[请求 schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)、
[配置 schema](../../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../../src/sf2tool/h2/map_setup.py)、
[实体 schema](../../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../../src/sf2tool/h2/map_entities.py)，以及
[init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../src/sf2tool/h2/map_init.py)。现有复现命令为 `uv run sf2 h2` 搭配 `battle-cutscenes`、`battle-routing-data`、`battle-loop`、`battle-control`、`common-stats`、`battle-spriteset-data`、`common-maps`、`gameflow-core`、`map-event-request-consumption`、`map-setup`、`map-entities` 和 `map-init`。受控复现前须在同一进程加载被忽略的 worktree 配置。这些标识所有者，不选择为纯文字开展新运行；保留的 map-script-engine 比较仍由研究复现配方负责。

综合前审阅连接了已接受研究、更严格合同、选定测试夹具字段及 schema／使用方边界。它区分公开聚合结构、选定内容与运行时完成。通用旧未知队列不使后续已接受静态连接失效，也不授权采集。反例包括 F519 未设置的入口、已完成19但传入 F419、Bowie/enemy128 均死亡、F88 恢复、加入前改变成员资格、F65／模式不匹配、F762 替代配置、仅当前 Rick、冲突随从查找、F64／已保存地图不匹配及被替换的待处理状态。它们约束静态解释，并非已执行场景。不需要 Layer A 修正。

保留 [PR857 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/856#issuecomment-6162129601)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/857#issuecomment-6162154889)，包括保留比较、已修正检查及精确 CI。保留 [PR858 更新交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/858#issuecomment-6162196284)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/858#issuecomment-6162210098)，以及所链接的原始／前序失败。已接受比较无需为文字仪式性重放；测试夹具／schema／黄金夹具保持不变，也不会成为组合运行时观察。

**未知：**自然可达入口／地形／路线或获胜轨迹；实际标志、HP、名册、映射、随机种子、存档点及运输；未选定存档点 init；服务／初始动作完成、渲染表现、响应输入和持久性。F762 的生产方及后续内容仍被排除。这些缺口继续按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)延后。本指南不选择重制规则，也不使用未接受的 Issue860 结论。
