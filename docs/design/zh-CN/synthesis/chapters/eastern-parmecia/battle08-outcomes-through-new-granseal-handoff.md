# Battle08 结果至 New Granseal 城堡交接

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

救援遭遇引向工人恢复意识、阻塞隧道的对话与返回船只。较后场景呈现船只拆解和 New Granseal 建设，然后请求其城堡。**推断的叙事含义：**这些场景将救援与定居点建立连接起来。**已确认静态行为：**首次胜利提供 Map73 返回；只有适当的 Map73 init 分支运行建设场景并请求 Map7。**未知：**自然完成的旅程、隧道通行、实际行动者/随从状态、服务完成与发出请求的存续。

本文承接[损坏船只木板选择至 Battle08 准入](damaged-ship-plank-through-battle08.md)。已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md)提供固定源码、对话、动作及调用方审阅。本指南支持内容/设计规划，止于条件性发出 Map7 准入之后、**目标初始化之前**。接续[New Granseal 城堡：一年后讲话与首次控制交接](new-granseal-castle-year-later-control.md)，了解目标初始化；后续城镇/战役内容另行限定范围。本文不增加可玩重制能力。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#739](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/739) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。源码身份使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。命令索引指从零开始的脚本记录，不是帧数或实测时长。叙事摘要不增加玩法谓词。

## 1. 胜利、败北与逃脱产生不同返回

**已确认结果规则：**Battle08 使用共享战斗循环。行动与行动结束效果后，零己方角色优先于零敌人；Bowie 零 HP 提供己方失败覆盖。单独击败敌方槽位 128 不产生特殊的仅首领胜利。选定 defeated 路由为 `ms_Empty`；其包装器要求 Bowie 存活、槽位 128 死亡且 F508 未设。其首领存在行是零，首领位置表没有 Battle08 覆盖。因此该 defeated 钩子不提供工人救援，也不移除全部剩余敌人。

独立的战后行仅在完成标志 F508 未设时选择 `abcs_battle08`。共享 after 包装器在执行与跳过路径上仍为 Bowie 调用 `JoinForce(0)`。这是实际使用的过场加入表行 8，而非名字相近的旧表。它既不招募工人，也不招募 Kiwi。[选定行审阅](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields)拥有这些精确源码选择；共享[过场合同](../../../contracts/battle/battle-cutscene-routing.md)拥有分发与加入边界。

| 结果与提供状态 | 已确认效果 | 提供给调用方的返回 |
| --- | --- | --- |
| F508 未设时胜利 | 治疗存活/不死己方角色，生成返回字段种子，运行 after 场景与共享加入，清除 F408 并设置 F508；返回 d4=1。 | 场景以 Map73 `(62,49)`、Left 替换种子。 |
| F508 已设时胜利 | 执行相同治疗、种子、共享加入与完成写入，但跳过 after 场景。 | 当前 Map72、首名出战队伍成员最终位置加战斗区域原点 `(1,32)`，以及该实体朝向。不提供固定 Map73 返回。 |
| 败北 | 更新解锁标志，显示固定文本 363，恢复 Bowie 最大 HP，并以无符号移位将金币减半；返回 d4=-1。 | 按提供的 `EGRESS_MAP` 选定存档点；没有 after 场景、共享 after 加入或新 F508/F705 写入。 |
| Egress / Angel Wing | 扣除法术 MP / 移除选定物品槽位；使用固定文本 274 / 275，更新解锁标志并返回 d4=0。 | 相同的已存地图选择，没有败北的金币减半或 after/加入路径。 |

败北或逃脱时，`UpdateBattleUnlockedFlag` 在 F508 未设时保留 F408，在 F508 已设时清除 F408。两种结果都不产生完成。若前篇 `EGRESS_MAP=14` 未变，且 BattleLoop 设置了 F399，则损坏船只存档点为 Map14 `(45,7)`、Down。其他输入地图选择各自行或 `(1,1)`、Up 后备。共享 F64 木筏重置与 F399 未设的新游戏后备规则仍归研究所有者。这些结果返回使用 `SwitchMap` 并直接进入探索，不执行另一次战斗准入扫描。自然逃脱/败北重入为**未知**。

胜利治疗及随后探索治疗会跳过死亡且非不死的己方角色。其精确 HP/MP 与状态掩码限制仍由[共享结果/治疗所有者](../../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)负责。较后建设重置范围更广。F458 独立于 F508 控制 before/start 场景；跳过介绍不建立完成。F88 挂起入场恢复秒数、清除 F88/AI 内存、加载战斗并恢复单次行动；它本身不抑制较后的 F508 未设 after 场景。有效挂起历史与自然交战仍为**未知**。

## 2. 工人恢复意识，但隧道仍止于对话边界

**已确认场景内容：**`abcs_battle08` 加载 Map72/视野 `(8,32)`，以 `ce_4ABBE` 替换场景实体群，等待 Bowie 初始化，并将工人 129 布置为动画停止、运动关闭。工人 128 是向导，129 是朋友。列表声明 Bowie、Sarah、Jaha、Kazin、Chester 和 Kiwi，没有固定 Slade 记录。这些声明是场景布置，不是交战 HP 或名册。加载列表重建实体/随从；既有己方映射可优先于固定放置，NPC 引用经 `ENTITY_INDEX_LIST` 解析，而非按物理分配顺序。

F6 未设时，场景选择将 Kiwi 移到 `(63,63)`、Down；F6 已设则跳过。这是位置命令，不是隐藏或离开军团。受 `AdjustScriptPointerByCharacterAliveStatus` 守卫的己方处理器可跳过索引 30 以下零 HP 己方角色的载荷，因此选中不证明已为死亡 Kiwi 写入位置。工人 128/129 绕过该己方检查。即使在 F6 未设分支上，较后 Kiwi 朝向命令也存在。实际映射与有效己方变更仍取决于输入。

向导靠近并对朋友作出反应。朋友先获得姿势精灵，再恢复工人精灵；随后动画与朝向动作伴随对话。**推断：**场景描绘落石后恢复意识。没有治疗、死亡、军团加入或物品获取操作实现该叙事。

朋友描述石头和阻塞的隧道。摄像机转向隧道区域再转回；向导跳跃，感叹无法取得宝藏，并提议返回船只。**已确认源码内容，且有严格限制：**场景没有移除石头、图块复制或穿越隧道操作。对话不证明碰撞、可步行路径、通行或宝藏内容。[after 场景所有者](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle08-after-scene)绑定命令 0–60 与对话 ID `09A4..09B2`，不将对话转为路线证据。

具名等待也保留各自范围。朋友的 Right/Left 动作与向导的碰撞动作不等待完成地启动；等待 120 不是显式汇合。其他工人移动、init 动作和跳跃逐一等待。被等待的行动者或解释器条件性滚动等待，都不证明全部运动、摄像机、文本、淡入淡出或音频服务已完成。

## 3. 战斗调用方先返回船只，再由 Map73 选择场景

**已确认首次胜利顺序：**after 场景最后的 `warp` 写入事件类型 1、参数 1 为零，以及 Map73 `(62,49)`、Left 字段。它不加载 Map73，也不结束脚本。在 `csc_end` 与解释器返回后，共享加入运行；BattleLoop 清除 F408、设置 F508，将目标字段直接读入 d0–d3，并返回 d4=1。

首次返回**不**使用 `WaitForEvent` 或 `ProcessMapEventType1_Warp`。`alt_MainLoopEntry` 执行 `SwitchMap`，绕过 `CheckBattle` 直接进入探索。Map73 没有替换行。工人场景与该返回之间不暗示存在玩家控制的探索。直接读取已确认；字段能否穿过中间服务而存续为**未知**。

入场时，探索清除旧待处理字、治疗并淡出。显式 Map73 入场设置当前地图与非战斗状态，取得配置实体，重建它们，加载精灵，清除临时标志 **256–383**，设置 F80，加载地图/基础 VInt，并在安装控制前调用 init。F408/F458/F508/F700/F705 位于清除范围外。Map73 只有默认配置；固定实体列表为空，但按标志选定的随从仍可构建。

**已确认选择：**`ms_map73_InitFunction` 检查进度标志，不检查抵达坐标、工人健康、F408、F458 或 EGRESS_MAP。

| 提供的 init 状态 | 选定内容与调用方写入 |
| --- | --- |
| F700 未设，F508/F705 任意 | 运行航行 `cs_503A6`，然后设置 F700，不测试较后分支即返回。 |
| F700 已设，F508 未设 | 不运行任何一个场景，直接返回。 |
| F700 已设，F508 已设，F705 已设 | 不运行任何一个场景，直接返回。 |
| F700 已设，F508 已设，F705 未设 | 运行建设 `cs_50806`，然后设置 F705 并返回。 |

较早[航行](../grans-opening/voyage-through-damaged-ship-control.md)在预期历史中提供 F700；本次胜利提供 F508。F705 未设是必要输入。F508 已设时直接进行 Map73 init，可以不经过本次工人 after 场景而选择建设；F700 未设则改选航行。重复胜利本身不提供 Map73 返回，因此不能保证每次胜利后都会建设。[战斗至 init 审阅](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle-caller-to-map73-init)拥有此条件连接。

## 4. 返回船只呈现新城建设

**已确认有序内容：**`cs_50806` 没有脚本级分支。一旦选定，它以 `ce_5094A` 替换实体群：Bowie、两个工人引用和一个船只精灵，没有固定己方角色声明。新工人是新声明引用；与前一场景身份的持续性未被证明。随从初始化仍适用，包括 Map73 依 F65 而定的大地图选择，以及其他提供的随从标志。

| 阶段 | 脚本指令 | 含义与限制 |
| --- | --- | --- |
| 建立队列 | 等待 Bowie init、向导 128 init 两次，然后再等待 Bowie init；安装工人 129 跟随 Bowie、Bowie 跟随 128，两者选择器均为 2。从 `(42,0)` 向 `(24,48)` 复制 `11x10` 图块区域，以摄像机跟随向导并淡入。 | 重复 init 调用确实存在。跟随是暂态动作脚本关系，不是军团成员身份或持久随从标志写入。 |
| 靠近船只 | 等待向导上的四个动作列表，然后对船只 130 使用 `setPosFlash` 到 `(0,0)`、Left。工人评论船已拆解；将朝向和摄像机指向建设区域。 | 等待向导不建立随从碰撞或最终位置。闪烁/重新定位不是删除、隐藏或交战死亡。 |
| 呈现建设 | 按顺序用源图块 `(42,0)`、`(53,0)`、`(42,10)` 和 `(53,10)` 替换 `(24,48)` 处 `11x10` 区域，中间有等待。对话讨论新城与旧 Granseal。 | **推断：**布置船只拆解和城镇建设。工作地图复制和视野更新不测量建设耗时，也不建立持久世界事务。 |
| 离开场景 | 设置中性摄像机；等待 128、129 和 Bowie init。按此顺序，对每个实体设置目标 `(29,52)`、等待抵达，再设置位置/目标 `(0,0)`、Down。等待 60，并显示建城叙述。 | init 替换其当前动作脚本。每个目标命令是具名等待，随后重新定位；既不是三次不等待的离开，也不是全局完成屏障。 |

`followEntity` 选择器 2 解析偏移 `(-24,0)`，并经 `AddFollower` 安装随从动作脚本；较后 `eas_Init` 替换会终止此脚本关系。尚未建立它跨新地图加载存续。三个 `setDest` 调用各自未改动的实体字，其位 15 均未设，因此 `csc29_setEntityDest` 在位置等于目标前等待，之后才返回。实际成功移动及其他服务仍为**未知**。

`setBlocks` 调用 `CopyMapBlocks`，标记两个视图平面待更新。`setPosFlash` 在等待期间交替船只 X 坐标，然后赋予最终位置和目标。两种效果都不消耗建筑材料或物品栏物品。[建设所有者](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#map73-later-scene-and-castle-request)保留精确命令 0–52、复制顺序与对话 ID `08F0..08F3`；此叙事摘要不替代这些源码操作。

场景随后请求声音淡出并调用 `resetForceBattleStats`。**已确认重置范围：**`ResetAlliesBattleStats` 为索引 **0–29** 赋予最大 HP 和最大 MP，不按已加入、出战或存活过滤。它将已有状态掩码为 stun/poison/curse，重建属性值，随后从装备派生诅咒；保留位不是新施加的异常。与较早存活/不死角色治疗不同，此重置可恢复零 HP 己方角色。它不招募任何人，也不改变出战队伍成员身份。[已接受重置所有者](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#force-reset-and-entity-list-semantics)与[出战队伍/脚本合同](../../../contracts/party/party-roster-state.md#处理器边界)保留此区别。

## 5. 城堡请求先于调用方完成标志

**已确认发出顺序：**重置后，建设命令 51 以事件类型 1、参数 1 为零请求 Map7 `(11,10)`、Up。命令 52 结束脚本。解释器返回后，**Map73 init 调用方设置 F705**；建设脚本本身不写它。

探索在到达 `WaitForEvent` 前执行条件性音乐/淡入淡出调用。若类型 1 存续，非零读取跳过控制安装。`ProcessMapEvent` 在分发前清除待处理字。其类型 1 传送使用方走参数 1 为零分支，处理/清除 WARP_SFX，使实体 0 idle，退出探索，并在 d0–d3 返回地图/朝向及正 X/Y 字节，d4=0。此普通探索返回进入 `MainLoop/@Start`，同时执行 `SwitchMap` 与 `CheckBattle`。

Map7 是 `MAP_NEW_GRANSEAL_CASTLE`。它没有地图替换或战斗坐标行，因此此条件准入保留 `(7,11,10,UP)` 并选择探索。**本指南止于 Map7 的 ExplorationLoop 初始化之前。**它既不建立城堡实体群/init 场景，也不建立城堡玩家就绪控制。[发出使用方所有者](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#pending-consumer-and-stopping-boundary)保留精确边界。

只有一个共享待处理槽位，不是队列。即使 F705 已设，覆盖或清除也可阻止城堡消费；若控制读取时待处理为零，Map73 可改为安装控制。显式目标入场前 Map73 仍为当前地图，因为请求本身不改变当前地图。

在预期完成链上，F408 未设，F508/F700/F705 已设。F458 从普通前篇 start 携带而来，不是由挂起入场建立；`EGRESS_MAP` 保留提供值，通常为 14。`PLAYER_TYPE` 保留 BattleLoop 的 0。其他故事/随从标志保留其提供历史。不保证场景行动者、暂态跟随及渲染/音频完成成为携带状态。

## 证据审阅与剩余问题

**已确认审阅结论：**本综合将已接受研究与前篇指南同下方已跟踪测试夹具选择及其 schema/验证器边界比较。选定 Map73 函数体与 F700 优先选择器、脚本后 F705 写入一致。共享过场事实与完成门槛和 after 加入一致；处理器事实与传送产生、暂态跟随、位 15 目标等待及军团重置一致。没有证据冲突要求合同变更。场景特定文本/动作、行 8 选择与 Battle08 位置覆盖缺失来自已接受研究的直接固定源码审阅。聚合测试夹具不独立证明整个旅程，也不再分发场景转录。

下方测试夹具链接包含其精确 `sf2-…-static-v1` ID。窄命令名前缀为 `uv run sf2 h2`；它们指明既有所有者，**未为本次文档切片重跑**。

| 主张与已接受所有者 | 已跟踪测试夹具选择 / 窄命令 | 保留边界 |
| --- | --- | --- |
| [结果与返回规则](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields) | [sf2-battle-control-static-v1](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)、[sf2-battle-loop-static-v1](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)；`battle-control`、`battle-loop`。 | 共享检查、调用方标志与治疗；实际自然交战及结果历史仍未知。 |
| [选定 after/defeated/加入行](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields) | [sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) `expected.cutsceneFacts`；[sf2-battle-global-data-static-v1](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)；相同 basename 命令。 | 共享包装器事实绑定测试夹具；`routeTargetsParsed=false` 不解析行 8。实际使用的加入不同于未使用的旧加入聚合。 |
| [Map73 选择](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle-caller-to-map73-init) | [sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json) `expected.primarySourceBodies[79]`、`ms_map73_InitFunction` 与 `cs_50806` 调用位置；[sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-entities-static-v1](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)；`map-init`、`map-setup`、`map-entities`。 | 静态选择器/配置身份一致；空固定条目不排除随从，也不证明自然抵达。 |
| [after 与建设命令](../../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#map73-later-scene-and-castle-request) | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) `expected.transitionCommandFacts`、`entityPopulationCommandFacts`、`mapBlockMutationCommandFacts`、`entityPlacementCommandFacts`、`entityGestureRelationshipMotionCommandFacts`、`forceStateCommandFacts.activePartyCommandFacts`；`map-script-engine`。 | 处理器事实与精简源码顺序证据保留这些操作。研究拥有已审阅的 `abcs_battle08` / `cs_50806` 程序流、内联动作、对话与列表。 |
| [独立请求使用方](../../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology) | [sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) `eventRequestConsumption.consumerContexts`；[sf2-common-maps-static-v1](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)、[sf2-common-scripting-static-v1](../../../../../../tests/fixtures/h2/common-scripting-static-v1.json)；相同 basename 命令。 | BattleLoop 直接字段读取不同于探索待处理消费。没有主张可执行的组合自然链。 |

相应[过场验证器](../../../../../../src/sf2tool/h2/battle_cutscenes.py)与[测试夹具 schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)、[map-init 验证器](../../../../../../src/sf2tool/h2/map_init.py)与[测试夹具 schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)、[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)与[schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)，以及[请求消费验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)保留精确比较面。[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)保留依路径而异的加载；[配置合同](../../../contracts/world/map-setup-data.md#合同边界)拥有配置身份。这些是证据边界，不是新重制架构或完成主张。

**未知，保留：**自然首次/重复胜利、失败/逃脱与抵达历史；合法步行、隧道碰撞/通行与宝藏；实际 HP、出战队伍及行动者/随从映射；成功的具名移动及独立摄像机/文本/淡入淡出/精灵/音频/时序完成；字段/请求存续、控制就绪和存档/读档持久性。区分这些边界的对照包括仅槽位 128 死亡、Bowie 零 HP、F508 已设重复、其他 EGRESS_MAP、F88 恢复、死亡/未加入的 Kiwi、冲突随从、F700 未设航行、F705 已设跳过，以及待处理请求被覆盖。它们是审阅反例，不是自动采集队列。

扩展需要由已接受所有者补足缺失关系：起始标志/行动者与结果、after 返回字段和调用方写入、选定 Map73 init、重置、存续请求及实际目标使用方。本切片既不完成战役，也不完成 epic #514。英文原文将中文同步留待路线图另行安排；后续城堡内容不在本指南范围内。
