# Battle 02 结果至 Battle 03 准入

Granseal 外的遭遇之后，胜利使出战队伍返回大地图。
Yeel 提供关于 Hawel 的局部信息，以及 Kazin 提议带出战队伍去见他的内容。
一个独立大地图区域事件调用方请求下一遭遇。
**推断的剧情连接：**这些编写内容延续
[Battle 01 返回至 Battle 02 准入](battle01-return-through-battle02.md)中的委任。
**已确认静态：**已接受 [Battle02 至 Battle03 调查](../../../research/battle02-to-battle03-content-route.md)
把 Map66 上有条件的胜利返回连接到源码 Battle03 准入。
**未知：**自然执行的行走、遭遇前是否访问 Yeel 或咨询 Kazin，以及实际返回和进入状态。

本指南面向沿地点、人物、交互与遭遇阅读的人，提供审查与还原工作所需证据链接。
它止于 Battle03 准入及选定入口依赖；Battle03 结算与 Hawel 住宅场景需要独立范围。
原版基线是固定 US ROM 及 `ShiningForceCentral/SF2DISASM` `master` 的
`c834c652b6862bc5679fd7f69a38a7093206efc6`，身份见
[研究溯源](../../../research/battle02-to-battle03-content-route.md#agreement-and-provenance)。
这里没有重制实现提供证据。其[已接受私有里程碑](map3-battle01-readiness.md#当前已接受里程碑)
止于 Battle01 可用返回；本指南不授予后期可玩支持。

这里的**已确认**指具名静态事实或显式限定的原版观察；**推断**描述叙事关系；
**未知**保留缺失的执行证据。“Battle02”和“Battle03”是从零编号的源码槽 2 和 3，
即 `BATTLE_TO_YEEL` 和 `BATTLE_TO_HAWEL_HOUSE`，不是已验证原版 UI 编号。
Map66 是源码所称 Granseal Kingdom 大地图；Yeel 是 Map5。名称解释内容身份，不解释时间顺序。

## 1. 结果将控制交回场地

### 什么结束 Battle02？

**已确认共享结果规则：**行动死亡处理之后，以及回合（单次行动）后效果及其死亡处理之后，
控制器都会统计剩余战斗员。战斗员必须具有已放置 X 坐标且 HP 为正才计数。
零己方角色选择败北，优先于零敌人选择胜利；Bowie 的 HP 为零会强制己方角色计数为零。
因此同时计数为零不选择胜利。Battle02 的敌方首领表值为零，
所以击败处理包装器不使用其首领存在尾部杀死剩余敌人。
有依据的获胜条件涉及所有剩余计入的敌人，不只敌人槽 128。
[战斗控制合同](../contracts/battle-control-lifecycle.md)和
[选定结果分析](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
拥有该关系；它们不提供本遭遇的获胜行动序列。

### 胜利有空剧情程序，也有实际共享加入尾部

**已确认选定分发：**F502 未设置时，Battle02 战后槽选择 `ms_Empty`。
若 F502 已设置，包装器跳过程序。两条路径都到达 `EndAfterBattleCutscene`，
将加入表第 2 行读为零，并调用 `JoinForce(0)`。
零是 Bowie 的己方角色索引；消费者没有跳过零索引的逻辑。
`JoinForce` 设置已加入成员资格、重建军团，并在低于容量时有条件调用 `JoinBattleParty`。
该行没有指定额外剧情招募者。

空程序包含地图脚本终止符，不提供剧情命令对话、传送、标志变更或招募，
但解释器终止尾部可能等待滚动，并清除滚动速度。
所以空编写内容不会消除包装器、解释器或加入消费者。
[过场路由合同](../contracts/battle-cutscene-routing.md)、
[出战队伍成员资格合同](../contracts/party-membership-state.md)和
[已接受选定源码分析](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
将这些效果留给各自所有者。参与构建的加入表是
`data/battles/cutscenes/afterbattlejoins.asm`；未使用的同名全局表不是该消费者的输入。

### 返回位置来自战斗最终状态

**已确认源码方法：**战后程序包装器之前，胜利治疗存活/不死己方角色，
并调用 `UpdateForceAndGetFirstBattlePartyMemberIndex`。
它将 Map66、该成员最终战斗员 X/Y 加战斗区域原点，以及成员实体朝向，保存到地图事件返回参数。
参数 1 为零。这些是依赖最终调用方的操作数，不是起始放置，也不是固定“返回 Yeel”点。

包装器返回后，胜利清除 F402、设置 F502，将已保存参数恢复到 `d0`–`d3`，返回 `d4=1`。
`MainLoop` 在 `alt_MainLoopEntry` 恢复，应用 `SwitchMap` 后调用 `ExplorationLoop`，
中间不调用 `CheckBattle`。该胜利序列不写 F403。
**已确认静态后果：**胜利将控制交给场地探索，不立即启动 Battle03。
**未知：**稳定后位置/朝向、自然可用输入与后续行走。
[已接受返回所有者](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
将 `battleloop_2.asm` (`BattleLoop_Victory`、`0x23CBA`) 关联到 `mainloop.asm`。

Map66 只有一个配置 `ms_map66`，普通实体列表为空、初始化为 RTS。
探索清除战斗模式，初始化地图实体/随从，加载地图并运行初始化。
**已确认结构：**普通列表为空不表示场地为空：领队与随从使用各自初始化。
它也不描述遭遇的战斗名册。Map66 和 Yeel 都没有由标志切换地图的覆盖；
其配置选择器仍有不同局部内容规则。

败北与逃脱是替代返回。**已确认静态：**普通 Battle02 败北调用 `UpdateBattleUnlockedFlag`，
恢复 Bowie 的 HP、金币减半、取得 Egress 位置并返回 `d4=-1`；Battle02 不是特殊可败战斗例外。
Egress/Angel Wing 使用解锁辅助并返回 `d4=0`。
该辅助仅在战斗已完成时清除解锁；普通失败的 Battle02 不被标为完成。
这些返回的精确目的地/状态和后续移动在此仍为**未知**。
它们不能继承所调查胜利的 F402 未设置/F502 已设置状态。

## 2. Yeel 提供信息与向导

**推断的内容解读：**村庄为委任提供更具体的当地目的地和向导。
这是有条件内容分支，不是已证明的 Battle03 前提。
下一节实际准入调用方既不读取 Yeel 访问条件，也不读取 Kazin 的 F72。

**已确认传送记录：**Map66 `(19,23)` 目标为 Yeel `(21,42)`、Up；
`(18,22)` 目标为 Yeel `(7,1)`、Down。
Yeel 在 Y=43 和 Y=0 的 X 通配出口返回 Map66 `(19,24)` 和 `(18,21)`，均为 Down。
这些记录在表中连接地点，不是从胜利终点出发的合法行走。
Map66 的 F508/F611 图块复制、Yeel 持久 F940 教堂图块复制及四条门踩踏记录，是额外工作布局依赖。
标记、碰撞与实体占位也有影响。
[地图内容所有者](../../../research/map-content.md)和
[地图探索合同](../contracts/map-exploration.md)保留该边界，不公开布局。

### 选择哪种村庄内容？

**已确认早期配置：**默认 `ms_map5` 在 `(7,6)`、Down 声明 `ALLY_KAZIN`，
并提供村民、动物与牧师。其初始化与默认区域程序直接返回。
早期解读假设后期选择器标志 650、530 和 28 未设置。
已设置时，这些替代项按该源码顺序覆盖：最后设置者胜出。
F650 使用不同实体列表/事件表，同时复用默认初始化、区域、描述和物品处理器；
它省略显式 Kazin 放置与默认 Kazin 事件绑定。
F530/F28 选择本分支之外的后期共享处理器。

默认实体事件表将 `ALLY_KAZIN` 配合 Down 朝向条件绑定到 `Map5_EntityEvent0`。
程序物理上位于 `s2_entityevents_650.asm`，但默认表跨文件边界调用它。
**已确认路由：**文件名后缀不是 F650 要求。
[Yeel 内容调查](../../../research/battle02-to-battle03-content-route.md#yeel-branch-content-without-an-invented-prerequisite)
和[地图配置合同](../contracts/map-setup-data.md)拥有选定绑定与共享选择器。

### Kazin 的提议改变跟随状态

**已确认编写内容：**固定 `txt` ID 784–787 提议带路去找 Kazin 的师父 Hawel，并指出住宅在村外。
文本 786 是中间的叙事消息；不能仅凭行动者精灵就把四个 ID 都归给同一发言者。
这些是固定操作数，不是 `textCursor` 序列。
调用方调用 `cs_51B14`，然后在脚本返回后设置 F72。

脚本执行 `followEntity ALLY_KAZIN,ALLY_BOWIE,2` 后结束。
`ALLY_KAZIN=4`，但此序列不调用 `JoinForce`，也不设置已加入标志 F4。
F72 在 `data/scripting/entity/followers.asm` 中与 Kazin 关联，并由 `InitializeFollowerEntities` 消费。
**已确认静态关系：**这确立跟随/F72，不是招募，也不是战斗解锁 F403。
[出战队伍/名册合同](../contracts/party-roster-state.md)将成员资格命令保持独立，
[通用脚本](../../../research/common-scripting.md)及已接受 Yeel 分析则拥有随从消费者。

随从初始化先于配置实体初始化；已建立索引的己方角色条目被跳过。
因此默认 Kazin 放置不证明重新进入后会出现第二个活跃 Kazin。
F650 省略该放置，同样不会移除由随从状态提供的随从。
**未知：**自然显示的跟随、实际重新进入实体集合与存档/读档持久性。

### 局部交互提供背景，但不解锁遭遇

| 早期 Yeel 交互 | 已确认内容或分支 | 限制 |
| --- | --- | --- |
| 实体事件 5，固定 ID 767–769 | 描述 Hawel 并指向西北。 | 局部指引不确立通往住宅的可行走路线。 |
| 实体事件 10，固定 ID 775–777 | 提及 Galam 士兵询问 Hawel。 | 这提供编写剧情背景，不是已验证的士兵移动时间顺序。 |
| 实体事件 13 | 读取 F72，选择 782–783 或 788–789。 | 跟随状态改变局部回应；本分支不产生 F403 写入方。 |
| 实体事件 14 | 调用教堂服务。 | 服务结果、呈现与持久性不在本内容观察范围内。 |

默认表及 `s2_entityevents_650.asm` 中的主体提供这些**已确认静态**关系。
[对话合同](../contracts/dialogue-system.md)和
[服务合同](../contracts/service-interactions.md)拥有共享服务。
**未知：**自然旅程中发生哪些交互、渲染的台词及其完成时序。
向导对话与村庄出口支持朝 Hawel 前进的**推断**路线，不是遭遇前必需访问。

## 3. 大地图区域调用方请求 Battle03

返回或停留在 Map66 不足以调用其默认事件。
**已确认控制器条件：**受控移动必须检查一个掩码后区域标记为 `$1400` 的目标。
事件消费者把请求的 X/Y 传给 `RunMapSetupZoneEvent`，后者使用首条匹配行。
Map66 在 Y=9、Y=10 和 Y=28 的 X 通配行先于默认项：

- Y=9/10 选择 `Map66_ZoneEvent0`；F537 已设置且 F538 未设置时写 F438 并请求重载。
- Y=28 选择 `Map66_ZoneEvent2`；F538 已设置且 F539 未设置时写 F439 并请求重载。

若其中一行匹配，但标志使其直接返回，仍不会继续落入默认项。
普通瓦片不是默认区域触发，坐标匹配本身也不证明到达标记。
[区域分发调查](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
连接移动、`ProcessMapEventType6_ZoneEvent` 与 `RunMapSetupZoneEvent` (`0x4751A`)。

**已确认精确默认调用方：**位于 `0x4FAD4` 的 `Map66_DefaultZoneEvent`
加载 `d0=3`，在 `0x4FAD8` 调用 `CheckRandomBattle`。
辅助区分首次与已完成遭遇：

| 入口条件 | 已确认静态请求行为 |
| --- | --- |
| F503 未设置 | 不读 `STEP_COUNTER`，不抽取 RNG，直接选择请求路径。 |
| F503 已设置，`STEP_COUNTER` 非零 | 不发出请求。 |
| F503 已设置，`STEP_COUNTER` 为零 | 以范围 8 抽取。首次结果为零则请求；首次非零则以范围 4 再抽取，将结果 +2 存为步数计数器。 |

请求路径设置 F403，在 `MAP_EVENT_TYPE` 写入长字 `$000100FF`，
设置 `STEP_COUNTER=30000`，等待滚动，请求 `SFX_BOOST` 并调用闪光服务。
**已确认静态顺序：**尽管辅助名为随机战斗，在该选定调用方，未完成遭遇的请求是确定性的。
F502、F72 和 Yeel 访问都不是其门禁。
辅助保留调用方的 `d1/d6/d7`；内部 `d1` 选择不是返回的成功标志。
[精确随机调用方所有者](../../../research/map-event-random-battle-state.md)和
[已接受路线调查](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
保留测试夹具及源码/H1/ROM 连接。
**未知：**自然到达默认区域、等待完成、声音/闪光交付与输入可用性。

## 4. 待处理请求成为遭遇准入

**已确认有条件消费者顺序：**区域处理器返回后，`WaitForEvent` 轮询待处理的 type-1 请求，
`ProcessMapEvent` 在分发前清除其类型。参数 1 为零；参数 2 为 `MAP_CURRENT`。
因此 `ProcessMapEventType1_Warp` 退出探索到 `MainLoop`，
后者应用地图选择与 `CheckBattle`，再针对返回索引进入 `BattleLoop`。
这把解锁/请求产生方与后续遭遇选择器分开。

长字写入仅覆盖类型字与参数字节 1/2 (`0xFFA84A..0xFFA84D`)。
参数 3/4 字节仍为保留的有符号坐标覆盖，参数 5 仍为保留朝向。
**已确认源码边界：**该请求不是新鲜的位置/朝向快照。
`CheckBattle` 从 `CURRENT_MAP` 解析 `MAP_CURRENT`；第 3 行 X/Y 通配触发
使未知保留坐标不影响该行选择，却仍影响实际场地终点的主张。
[请求消费所有者](../../../research/map-event-request-consumption.md)拥有轮询/分发；
[已接受消费者组合](../../../research/battle02-to-battle03-content-route.md#conditional-transition-and-content-table)
拥有下游 type-1 与选择器关系。

当前为 Map66、所调查胜利已清除 F402、选定请求已设置 F403 时，
**已确认有条件准入：**第 3 行选择 Battle03，写入区域 `(0,0,22,30)` 并返回索引 3。
矩形是输出原点/大小，不是玩家坐标资格测试。
假设选定服务成功返回，这就是主要停止边界。

较早合格行可以胜出。若替代调用方的 F402 已设置，即使 F502 已设置，Map66 上第 2 行也胜出：
`CheckBattle` 清除 F402，但同次调用仍返回索引 2。
类似地，若选定第 3 行的 F403/F503 均已设置，它清除 F403，同次调用仍返回索引 3。
后续调用才看到改变后的标志。F403 未设置时，选择可继续到更晚的已解锁 Map66 行；
不意味着通用“无遭遇”结果。
[通用地图所有者](../../../research/common-maps.md#confirmed-map-routing)提供共享扫描，
已接受路线提供选定第 3 行操作数。任何拟议组合测试夹具都不被当作已经实现。

## 5. 选定 Battle03 入口声明了什么

**已确认源码声明：**`battlemapcoords.asm` 第 3 行是 `[66,0,0,22,30,255,255]`。
精灵集槽 3 选择 `BattleSpriteset03` (`0x1B33F4`)，其头部声明 12 个己方角色放置、
6 名敌人、1 个 AI 区域且无 AI 点。十二条放置行不确立十二名已加入或出战己方角色。
地形槽 3 选择 `BattleTerrain03`；其后续别名不确立剧情顺序。
[遭遇合同](../contracts/battle-encounter-definition.md)、
[精灵集所有者](../../../research/battle-spriteset-data.md)和
[地形/路由所有者](../../../research/battle-routing-data.md)保留数据与载荷边界。
解释该准入不需要名册表或地形网格。

战前槽选择 `bbcs_03` (`0x4980E`)，它执行 `csub_49816` 写入 `EGRESS_MAP=MAP_YEEL`，随后结束。
**已确认入口侧赋值：**这在选定分支选择返回目的地值；不表明此前访问过 Yeel。
程序不提供对话游标、招募或 F403 写入。

该赋值有条件。`ExecuteBeforeBattleCutscene` 测试 F453 而不设置。
开始包装器也测试 F453，并在将槽 3 分发到 `ms_Empty` 前设置它。
若 F453 已设置，两次分发都跳过；F88 挂起战斗路径也绕过这套普通新战斗准备。
**已确认共享顺序：**F88 未设置时，`BattleLoop` 安装当前地图/战斗，运行战前程序，
清除区域标志、治疗/初始化战斗员，加载战斗内容并运行开始程序。
[过场路由合同](../contracts/battle-cutscene-routing.md#共享-intro-门)和
[选定 Battle03 依赖](../../../research/battle02-to-battle03-content-route.md#source-battle03-entry-dependencies)
拥有 F453 与 Egress 条件。共享控制器的 Battle01 证据不证明已初始化的 Battle03 出战队伍、激活或首次行动。

接着读 [Battle 03 结果至 Battle 04 准入（中文）](battle03-outcome-through-battle04.md)，
了解有条件返回、Hawel 住宅场景、Kazin 招募及独立出口/准入路径。
它保留自然路线与随从状态的未知，不扩展重制支持。

## 证据审查与剩余限制

本综合审查已接受调查的固定源码绑定、前一指南和当前就绪边界，
以及下列选定公共载荷/验证器表面。选定源码发现仍归于其研究所有者；不默默将聚合测试夹具特化。
命令是复现路线，不是本综合中的新运行。本工作不产生新测试夹具或原版观察。

| 关键主张 | 已审查可执行所有者与窄复现路线 | 结论 |
| --- | --- | --- |
| 结果与普通返回 | [战斗循环测试夹具](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`sf2-battle-loop-static-v1`、`remainingCombatants`；[控制测试夹具](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`sf2-battle-control-static-v1`；[循环](../../../../src/sf2tool/h2/battle_loop.py) / [控制](../../../../src/sf2tool/h2/battle_control.py)验证器；`uv run sf2 h2 battle-loop`、`uv run sf2 h2 battle-control` | 计数/结果事实和有序源码片段覆盖共享结构。选定最终状态返回方法与 Map66 交接是已接受源码发现；已审查测试夹具不含组合的 Battle02 胜利见证。 |
| 空战后程序、加入与开场门禁 | [过场测试夹具](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`sf2-battle-cutscenes-static-v1`；[验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)，`uv run sf2 h2 battle-cutscenes`；[脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`、[验证器](../../../../src/sf2tool/h2/map_script_engine.py)，`uv run sf2 h2 map-script-engine` | 共享完成/加入及开场顺序一致。选定第 2 行、零索引 JoinForce 与终止符消费者使用调查中具名源码；不把历史 Battle01 测试夹具改写成 Battle02 证明。 |
| 地图选择与局部内容结构 | [配置](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`；[内容](../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[配置](../../../../src/sf2tool/h2/map_setup.py)、[实体](../../../../src/sf2tool/h2/map_entities.py)、[内容](../../../../src/sf2tool/h2/map_content.py)验证器；`uv run sf2 h2 map-setup`、`uv run sf2 h2 map-entities`、`uv run sf2 h2 map-content` | 共享选择器/消费者事实不证明合法行走。各项 Map05/Map66 绑定与传送记录仍是选定源码发现，不是虚构聚合测试夹具用例。 |
| Kazin 调用方与区域调用方 | [实体程序](../../../../tests/fixtures/h2/map-events/entity-programs.json)、[区域程序](../../../../tests/fixtures/h2/map-events/zone-programs.json)和[脚本调用](../../../../tests/fixtures/h2/map-events/script-invocation.json)，为 `sf2-map-events-static-v1` 的组成部分；[验证器](../../../../src/sf2tool/h2/map_events.py)，`uv run sf2 h2 map-events` | 选定 `Map5_EntityEvent0:334530` 保留固定文本操作数、先脚本后 F72；`Map66_DefaultZoneEvent:326356` 保留先战斗 3 再辅助调用。脚本测试夹具中的 `followEntity` 处理器/调用结构不同于军团成员资格；随从初始化仍属于已接受源码消费者。 |
| 首次与重复请求 | [随机战斗测试夹具](../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)，`sf2-map-event-random-battle-state-static-v1`；[验证器](../../../../src/sf2tool/h2/map_event_random_battle_state.py)，`uv run sf2 h2 map-event-random-battle-state` | 已审查 `callerSites.map66-default-battle3`、`completionFlagGate`、`stepCounterGate`、`randomCadence`、`requestWriteSequence`、`returnContract`。完整语义比较约束该调用方/主体结构；下游准入与呈现被排除。 |
| 请求消费与有序战斗选择 | [请求测试夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`；[请求验证器](../../../../src/sf2tool/h2/map_event_request_consumption.py)，`uv run sf2 h2 map-event-request-consumption`；[通用地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`、[验证器](../../../../src/sf2tool/h2/maps.py)，`uv run sf2 h2 common-maps` | 请求测试夹具止于轮询/清除/分发，通用地图片段不组合选定行。保留参数与第 2/3 行同次调用返回使用已接受源码分析。其拟议有条件准入扩展仍未实现。 |
| 入口声明与选定过场 | [精灵集](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`sf2-battle-spriteset-data-static-v1`；[路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1`；[精灵集](../../../../src/sf2tool/h2/battle_spriteset_data.py) / [路由](../../../../src/sf2tool/h2/battle_routing_data.py)验证器；`uv run sf2 h2 battle-spriteset-data`、`uv run sf2 h2 battle-routing-data` | `rowContentTracked=false` 和 `routeTargetsParsed=false` 保留清单限制。Battle03 的选定头部、战前/开始目标及有条件 EGRESS_MAP 写入来自调查，不是活跃入口证明。 |

严格边界彼此一致：不存在需要所有者修正的证据分歧。
[研究缺口处置](../../../research/battle02-to-battle03-content-route.md#evidence-gaps-and-fixture-disposition)
为缺失组合见证标识已有所有者。拟议扩展不是执行授权，通用运行时问题队列也不是自动采集工作。

仍为**未知**：自然达到的 Battle02 结果/调用方状态与稳定返回终点；考虑标记/碰撞/占位的行走；
选择或访问哪些 Yeel 内容及其时间；实际跟随、对话和服务交付；初始化后的 Battle03 成员资格/激活/首次行动；
以及存档/读档持久性。前往 Hawel 的预期旅程仍为**推断**。
准入、可选内容和入口侧目的地赋值不能合并成已证明的连续流程攻略。
[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有后续范围，
[剧情推进](story-progression.md)解释更广状态关系。
