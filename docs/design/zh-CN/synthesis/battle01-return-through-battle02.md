# Battle 01 返回至 Battle 02 准入

第一次遭遇将出战队伍留在塔入口。下一段有条件、由源码相连的内容返回城堡，
通过驱魔场景与委任发展 King 的处境，将 Jaha 加入军团，并开启 Granseal 外通往 Yeel 途中的遭遇。
**推断的剧情连接：**下列编写内容构成[开篇至 Battle 01](opening-through-battle01.md)之后的下一段。
**已确认静态：**已接受[路线调查](../../../research/battle01-to-battle02-content-route.md)
识别下述调用方、程序、标志和表连接。**未知：**自然执行、连续合法地走完这段扩展的旅程。
本指南解释有条件内容，不把表连接呈现为流程攻略。

- 读者：跟随地点、人物、对话与遭遇阅读的人；链接支持研究审查与还原实现。
- 范围：Battle 01 可用返回至源码 Battle 02 准入。Battle 02 游玩、结算与后续场景不在本指南范围内。
- 原版基线：`ShiningForceCentral/SF2DISASM` `master` 的
  `c834c652b6862bc5679fd7f69a38a7093206efc6`、固定 US ROM，以及已接受
  [原版证据所有者](../../../research/battle01-to-battle02-content-route.md#scope-and-evidence-boundary)。
- 重制边界：已接受私有里程碑止于 Battle 01 可用返回。本静态扩展不授予额外可玩支持或验收。

这里的**已确认**表示源码事实或具名的有界原版观察；**推断**表示叙事连接；
**未知**保留尚未观察的关系。地图编号、源码楼层名和战斗槽编号是不同身份。
特别是“Battle 02”指从零编号的源码槽 2、`BATTLE_TO_YEEL`，不是已验证原版 UI 编号。

## 1. 离开塔并返回城堡

**已确认有界原版终点：**选定 Battle 01 获胜谱系稳定在 Map 57 `(5,12)`、Down，
F401 未设置、F501 已设置，名册为 `[0,1,2]`。另一次已接受普通 Down 输入到达 `(5,13)`。
两项观察都位于终点且不可恢复执行；都不是下面旅程的捕获。
[当前原版前沿](map3-battle01-readiness.md#已接受原版前沿)和
[终点合同](../contracts/map3-battle01-continuous-scenario.md#精确已观测端点)拥有完整状态与溯源。

此前的战后程序 `abcs_battle01` 提供叙事交接。**已确认编写内容：**源码注释描述灵体离开以及返回 King 处的指示。
它加载场景的 mist-demon 实体 128 和 Astral 实体 129；游标 2305 与选择 129 的对话命令属于该已加载场景。
这些身份来自源码行动者绑定，不是对数字发言者操作数的猜测。
[返回调查](../../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)
将这些编写内容关联到已有胜利证据。完整静态程序与获胜采集到达的操作仍是不同证据集。

**已确认静态连接：**从 Map 57 的通配 `y=19` 传送离开，目标为 Map 40 `(15,13)`、Down。
Map 40 的通配 `y=31` 传送目标为 Map 21 `(9,2)`、Down。
Map 21 `(3,16)` 目标为 Map 20 `(3,36)`、Right。
这些返回连接重访开篇的通道与城堡地图，但选定内容已经改变：
Map 21 的 F501 配置将实体 128 初始化于 `(6,16)`。
不能仅因地图相同，就把返回配置当作此前塔守卫状态。

一条表连接的城堡接近路径从 Map 20 `(6,37)` 到 Map 19 `(6,2)`、Left；
再从 Map 19 `(23,3)` 到 Map 20 `(23,37)`、Down；
并穿过 Map 20 局部传送 `(25,35) → (25,50)`、Right。
这识别了可调查王室房间事件的位置。**未知：**在选定布局、事件标记、碰撞和实体占位条件下，接缝之间的移动。
通配传送记录不会让每个匹配的可行走单元都成为传送终点。
[条件连接所有者](../../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table)
保留精确记录；[地图探索](../contracts/map-exploration.md)拥有其控制器边界。

### 选择哪种城堡内容？

**已确认选择规则：**配置按源码顺序采用最后设置的标志；局部事件表采用首条匹配记录。
城堡返回最初在 Map 19/20/21 使用 F501 配置，在 Map 40 使用默认配置。Map 57 选择 void 配置。
除已观察到的 F501 已设置外，选定早期状态解读假设 F609 起初未设置、F3 未设置，
且 F402/F502/F452/F88 未设置。这些额外假设不是由终点观察重建的完整标志位集。

后期标志不得覆盖预期配置。501 之后，Map 19 按序有 609/506/507/543/982 替代项；
Map 20/21 有 609/506/543。Map 40 默认项之后有 506/507；Map 3 的 609 之后有 506/543。
F609 会在本段后面有意改变选定内容；其他后期覆盖必须不存在，才适用这种解读。
返回 Map 3 时 F640 也必须未设置，因为它在配置选择前把地图切换为 Map 4。
Map 66 只有默认配置。[配置所有者](../../../research/map-data-inventory.md#setup-selection-and-rom-parity)
和[路线假设](../../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table)
将这些局部规则与自然战役状态分开。

初始化入口也有影响。Map 19 的 F501 配置进入内部直接返回函数，不执行默认初始化前缀。
Map 20 的 F501 内部初始化仍测试 F507。配置指针不允许重放其文件中所有前置代码。

## 2. 城堡驱魔有两个不同入口

**推断的内容解读：**城堡场景在出战队伍接到下一委任前发展 King 的状况与 Astral 的干预。
源码将 Map 20 选定 F501 实体 128 绑定到 King Granseal 精灵，将实体 130 绑定到 Astral。
这是**已确认编写身份**；该扩展自然显示的发言者、立绘、声音和时序仍为**未知**。

完整入口是 `(27,52)` 的区域事件 `Map20_1F5_ZoneEvent0`。
**已确认调用方顺序：**取得己方角色 1 和 2 的最大 HP、MP，写入它们的当前 HP、MP，然后调用 `cs_53B66`。
该准备属于区域调用方，发生在场景之前。
脚本以游标 2197 开始，经过内部后缀 `cs_53C42`，在此把游标重置为 2200。
区域表记录标识控制器分发该事件时的选定目标，不确立普通移动能够到达并分发它。

另有实体入口。同一 F501 配置下，实体 130 配合 Down 朝向条件选择 `Map20_1F5_EntityEvent2`，
它直接调用 `cs_53C42`。**已确认静态替代项：**该调用方进入共享后缀，
同时省略区域的 HP/MP 准备和完整脚本的前缀。
这是额外的局部内容入口，不是已观察可选捷径，也不具有等同的初始行动者状态。
将区域当作唯一源码入口会遗漏这一关系；将后缀当作完整执行会虚构效果。

共享继续路径设置 F609，并发出到 Map 19 `(29,8)`、Up 的脚本传送。
该脚本过渡不同于接近场景所用的物理传送记录。
**推断的叙事连接：**它把城堡剧情交给委任配置。
**未知：**扩展中的实际调用方状态、过渡消费与可见交接。
[已接受调用方/脚本所有者](../../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table)、
[对话合同](../contracts/dialogue-system.md)和[脚本过渡合同](../contracts/map-exploration.md)提供相应边界。

F501 城堡的其他对话仍是局部回应，不是必需解锁步骤。
Map 19 实体事件引用包括文本 ID 583–586；Map 20 包括 588–596/2198，带朝向条件与临时标志 256/259–262。
上面的实体 130 后缀调用才是相关替代推进入口。
仅有静态可用性不确立玩家层面的可选性、可错过性，也不证明每项回应都属于同一次流程。

## 3. 委任：请求接来 Hawel

F609 选择 Map 19 的委任内容 `ms_map19_flag609`。
**已确认初始化行为：**初始化测试 F610。未设置时调用 `cs_53176`，仅在脚本返回后设置 F610；
已设置时跳过脚本。F609 选择配置，F610 控制此初始化调用。
两者都不应改称同一个“任务已接受”状态。

**已确认编写内容：**脚本安排一组士兵离开，并由 King/Minister 请求前往 Yeel 附近接来 Hawel。
游标 721 开启程序；实体操作数 138/140 与选定 Minister/King 精灵条目关联，
`newEntity 144` 提供发言士兵。Sarah 和 Chester 使用显式己方角色操作数。
[对话与行动者所有者](../../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)
保留这些绑定及简短的源码依据概要，不公开逐字记录。

选择在源码中是循环，不是独立永久路线的证据。
在 `cs_53482`，F89 未设置会重复提示分支，拒绝游标为 757。
F89 已设置则进入 `cs_534AC`，接受游标为 758。
共享剧情提示处理器在结果 0 时设置 F89，否则清除它。
**已确认静态：**接受后的继续路径以 Sarah 跟随 Bowie、Chester 跟随 Sarah 结束。
**未知：**自然呈现的回答、这些行动的时序与可见完成，或持久玩家选择后果。

**推断的剧情连接：**委任为向西离开提供叙事背景。
它不是后续 Jaha 事件或 Battle02 选择器的额外谓词。
F610 已设置可跳过委任；后续 Jaha 调用方不读取 F610。
所以本指南解释选定条件序列，不把每个场景都变成共享准入要求。

## 4. 返回 Granseal：Jaha 加入，调用方解锁遭遇

**已确认静态返回：**Map 19 的通配 `y=31` 传送目标是 Granseal，即 Map 3 `(29,2)`、Down。
选择 F609 且不存在覆盖标志时，`ms_map3_flag609` 在 `(1,29)` 提供 Jaha，并有两条西侧区域记录。
其初始化在 F3 已设置时隐藏 `ALLY_JAHA`。
实体存在与已加入成员资格是相关源码状态，不证明一次已观察返回中他的可见位置。

区域顺序影响该内容：

| 分发坐标 / 入口状态 | 已确认选定行为 |
| --- | --- |
| `(2,23)` | 第一条精确记录选择 `Map3_261_ZoneEvent0`，直接返回。它排除后面的通配事件。 |
| `x=2`、另一个匹配 Y、F3 已设置 | 通配记录选择 `Map3_261_ZoneEvent1`，其 F3 分支到达同一个直接返回。 |
| `x=2`、另一个匹配 Y、F3 未设置 | 通配调用方运行 `cs_516A8`；脚本返回后设置 F402、清除 F66。 |

**已确认编写加入序列：**`cs_516A8` 使用游标 662 和三条面向 `ALLY_JAHA` 的 `nextSingleText` 命令。
`join` 位于第二条与第三条之间，脚本安装 Jaha 跟随在 Chester 后面。
加入服务自己的消息路径不会给该脚本游标再增加一步。
`csc08_joinForce` 调用 `JoinForce`，设置 Jaha 已加入标志 F3 并更新军团成员资格；
进入出战队伍仍以容量为条件。四条 Battle02 放置行不确立所有调用方状态下都准入四名活跃己方角色。
[出战队伍/名册合同](../contracts/party-roster-state.md)和
[已接受加入分析](../../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)保留这些区别。

加入与解锁是不同效果。**已确认调用方所有权：**F402/F66 由 `Map3_261_ZoneEvent1`
在 `cs_516A8` 返回后写入，不由 `join` 命令写入。
若 F3 已设置而 F402 未设置，该已检查事件跳过解锁写入方；它不会修复该状态。
精确 `(2,23)` 例外同样不提供解锁。
**未知：**本段自然到达的事件坐标、调用方标志、可见跟随与持久存档状态。

## 5. Granseal 以西：什么选择 Battle 02？

**已确认静态出口：**Map 3 的通配 `x=0` 传送目标为 Map 66 `(29,32)`、Down，
即源码所称 Granseal Kingdom 大地图。**推断的剧情连接：**这是朝委任目的地 Yeel 的出发。
**未知：**从 Jaha 事件合法走到实际西出口标记；仅凭传送元组不提供这条路径。

准入使用当前地图与解锁状态。`MainLoop` 应用 `SwitchMap`，调用 `CheckBattle`，
将不是 `-1` 的返回索引交给 `BattleLoop`。
**已确认源码选择：**Map 66 上 F402 已设置时，第 2 行是首条合格行；
第 0/1 行地图不同，第 2 行 X/Y 触发均为通配。
它写入战斗区域 `(16,19,22,18)` 并返回索引 2。
矩形是输出原点/大小，不是玩家边界检查。
到达该矩形、完成 Battle01、F610 或接近敌人都不能替代 F402 门禁。

完成状态不会拒绝同一次已匹配调用。**已确认静态：**若 F502 已设置，
`CheckBattle` 在该次调用清除 F402，仍返回索引 2。后续调用才看到改变后的解锁状态。
选定全新进入假设 F502 未设置是场景前提，不是另一条共享资格规则。
F402 未设置时，第 2 行不合格；若自己的解锁已设置，后面的 Map66 行仍可能被选中。
[选择器分析](../../../research/battle01-to-battle02-content-route.md#battle-02-data-and-admission-semantics)
和[通用地图所有者](../../../research/common-maps.md#confirmed-map-routing)保留扫描与完成语义。

### 该准入所指的遭遇数据

**已确认源码声明：**槽 2 选择 `BattleSpriteset02` 和 `BattleTerrain02`；地形槽不是别名。
精灵集声明四条己方角色放置行、六名敌人、无 AI 区域和一个 AI 点。
敌人类型为两个 `HUGE_RAT` 和四个 `OOZE`；全部十条行为记录使用 `STARTING`。
这些事实描述编写的遭遇内容，不是完整活跃名册、激活、生成状态、能否被选为目标或首位行动者。
[遭遇定义](../contracts/battle-encounter-definition.md)、
[精灵集清单](../../../research/battle-spriteset-data.md)和
[地形/路由所有者](../../../research/battle-routing-data.md)定位数据，不复制放置表或地形网格。
Map 66 的普通空实体列表与 RTS 初始化不描述该战斗名册；准入先于普通探索分支。

源码槽 2 的战前程序与开始程序均指向 `ms_Empty`。
**已确认静态包装器效果：**两者共享 F452；战前测试该标志而不设置，
战斗开始在未设置时先设置它再分发。
所以空目标不等于没有包装器效果，也不等于因没有 Battle02 脚本目录而缺失提取。
[过场路由合同](../contracts/battle-cutscene-routing.md#共享-intro-门)拥有该共享顺序。

在新战斗的 F88 未设置分支，控制器安装当前地图/战斗，运行战前程序，清除 F90–F105，
治疗/初始化战斗员，清除 AI 内存，加载战斗内容，然后运行开始程序。保存的战斗 F88 分支是另一条路径。
**已确认共享源码顺序：**已有 [Battle01 准入所有者](../../../research/map3-battle01-admission.md)
在其选定数据范围闭合该控制器序列；它不会变成 Battle02 初始化或玩家就绪观察。
本指南止于准入。接着读
[Battle 02 结果至 Battle 03 准入](battle02-outcome-through-battle03.md)，
了解有条件返回、Yeel/Kazin 内容与大地图请求/消费者连接。
后续指南保留自然路线的未知，不授予后期重制支持。

## 证据审查与限制

审查检查了下列已接受载荷与验证器语义。
[路线调查](../../../research/battle01-to-battle02-content-route.md)拥有各项固定源码发现及其已复现静态清单。
本指南不新增测试夹具或观察，也不为概述而重跑证据命令。

| 关键连接 | 已接受载荷及验证器 / 复现所有者 | 审查结论 |
| --- | --- | --- |
| 可用返回与编写的交接 | [胜利测试夹具](../../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json)，`sf2-map3-battle01-victory-return-static-v1`；[验证器](../../../../src/sf2tool/h2/map3_battle01_victory_return.py)，`sf2 h2 map3-battle01-victory-return`；[原版前沿](map3-battle01-readiness.md#已接受原版前沿) | 静态返回主干与后续终点/输入观察保持不同；不推断已恢复执行的扩展。 |
| 配置、传送与实体存在 | [配置](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`；[内容](../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[配置](../../../../src/sf2tool/h2/map_setup.py)、[实体](../../../../src/sf2tool/h2/map_entities.py)与[内容验证器](../../../../src/sf2tool/h2/map_content.py)，`sf2 h2 map-setup / map-entities / map-content` | 有序选择与数据不闭合考虑标记/碰撞/占位的行走。各条路线记录/绑定可追溯至已接受源码调查，不是虚构聚合测试夹具行。 |
| 驱魔调用方与 Jaha 解锁产生方 | [区域程序](../../../../tests/fixtures/h2/map-events/zone-programs.json)和[实体程序](../../../../tests/fixtures/h2/map-events/entity-programs.json)，为 `sf2-map-events-static-v1` 的组成部分；[路由配置](../../../../tests/fixtures/h2/map-events/routing-setup.json)与[脚本调用](../../../../tests/fixtures/h2/map-events/script-invocation.json)；[验证器](../../../../src/sf2tool/h2/map_events.py)，`sf2 h2 map-events` | 已审查 `Map20_1F5_ZoneEvent0:341870`、`Map20_1F5_EntityEvent2:342108`、`Map3_261_ZoneEvent1:331510` 及有序表。HP/MP 准备、后缀入口和返回后的 F402/F66 保持不同。 |
| 委任初始化 | [初始化测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`：`routeJoins`、`sourceFiles`、`scriptCallSites`；[验证器](../../../../src/sf2tool/h2/map_init.py)，`sf2 h2 map-init` | F609 路由连接 F610 测试/脚本/返回后写入序列；内部 F501 初始化入口不继承默认前缀。 |
| 场景标志、文本、加入与过渡 | [脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`：`storyStateCommandFacts`、`dialogueCommandFacts`、`forceStateCommandFacts`、`transitionCommandFacts`；[验证器](../../../../src/sf2tool/h2/map_script_engine.py)，`sf2 h2 map-script-engine` | 源码位置/处理器连接保留 F609、F89、`ALLY_JAHA` 和脚本传送操作数。各游标/行动者绑定使用已接受源码所有者。处理器局部矩阵不证明自然显示、选择或时间顺序。 |
| Battle02 行与完成选择 | [通用地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[验证器](../../../../src/sf2tool/h2/maps.py)，`sf2 h2 common-maps`；[战斗全局数据](../../../../tests/fixtures/h2/battle-global-data-static-v1.json)，`sf2-battle-global-data-static-v1`；[验证器](../../../../src/sf2tool/h2/battle_global_data.py)，`sf2 h2 battle-global-data` | 共享有序片段与聚合维度不是选定第 2 行的可执行连接。已接受源码分析及其已检查生成行拥有该结论，包括已完成状态在同次调用仍返回。其拟议测试夹具扩展尚未实现。 |
| 遭遇声明、地形与空路由 | [精灵集](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`sf2-battle-spriteset-data-static-v1`；[路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1`；[精灵集](../../../../src/sf2tool/h2/battle_spriteset_data.py)、[路由](../../../../src/sf2tool/h2/battle_routing_data.py)和[过场验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)，`sf2 h2 battle-spriteset-data / battle-routing-data / battle-cutscenes` | `rowContentTracked=false` 和 `routeTargetsParsed=false` 防止将清单成功当作完整选定行为。具体第 2 行操作数/目标是已接受源码发现；F452 包装器效果是共享源码语义。 |

地图事件验证器在比较完整语义测试夹具之前，对照固定 H1 保留操作码/操作数/顺序；
初始化与脚本验证器比较源码派生字段及溯源。
这些守卫支持其声明边界内的结构主张，不会将这些调用方作为连续剧情执行。
选定 Battle01 准入测试夹具也不能仅因控制器共享，就默默补充缺失 Battle02 操作数或活跃军团。

仍为**未知**：所列接缝之间的合法移动、自然选择的配置与调用方标志、场景/提示交付和输入就绪、
初始化后的 Battle02 成员资格/激活/首次行动，以及跨进程持久性。
仍为**推断**：相连事件的预期叙事顺序。
阅读已接受条件段不需要新内容平台、比较器、路线模型、运行时捕获或重制能力。
[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有后续范围；
[剧情推进](story-progression.md)解释更广状态关系。
