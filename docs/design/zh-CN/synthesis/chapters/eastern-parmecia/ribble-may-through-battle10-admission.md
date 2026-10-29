# Ribble 可选 May 招募至 Battle10 准入

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/ribble-may-through-battle10-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Ribble 欢迎场景之后，出战队伍可以遇见 May、听取旅行建议并离开城镇。May 的邀请直接进入加入与当前跟随；选定出发和遭遇规则并不要求这段对话。随后向东的连接跨越两种不同地图生命周期，之后 zone 请求才可能使 Battle10 准入。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/ribble-may-through-battle10-admission.md)支持下文互动、状态变化及选定 Map8→69→70 连接。**推断：**建议和东向遭遇构成欢迎之后的叙事接续。**未知：**合法步行、实际行动者与出战队伍状态、请求存续、服务完成，以及连续可玩的路线。本指南支持内容/设计规划，不确立重制就绪或实际游玩的攻略。

始于[欢迎指南的条件性控制交接](battle09-outcomes-through-ribble-welcome.md)，止于 Battle10 start 分发器返回、紧邻 `ActivateEnemies` 之前。继续阅读 [Battle10 结果至可选 East Shrine 访问](battle10-outcomes-through-east-shrine-visit.md)，了解独立的结果/访问边界。交战模拟、洞窟和后续内容仍在本准入指南之外。Fnnn 表示标志 nnn；地图与战斗 ID 保留源码编号。

## 1. 遇见 May 是可选的；她的脚本没有拒绝分支

**已确认静态互动：**F708 选择 Ribble 扩展配置及其 May 事件行。编写的游走原点在 `(18,27)` 附近、Down，半径操作数为 2；它不是观察到的位置。仅设置 F708 不重建当前实体群体。正如前序指南所述，欢迎场景的显式实体重载与之后普通入场是取得实体群体的不同途径。

普通互动要求已激活的当前实体、有效映射和事件分发路径。当前随从与受控实体被排除在激活扫描之外。May 的 character 身份 8 不是物理槽位 8。事件行的 `UP=1` 字节控制朝向：在处理器前让行动者转向与传入玩家朝向相反的方向，之后不恢复她原先朝向。它不要求从北面靠近；行选择匹配事件身份，而非此朝向字节。

F8 已设时，`Map8_2C4_EntityEvent12` 不运行场景即返回。F8 未设时调用 `cs_56146`。此处理器没有局部 Battle09 完成、Peter、出战队伍容量或旅行前提。场景等待 May 初始化和停止移动行动，让她面向 Bowie 目标，然后请求邀请文本框。它调用普通 `join ALLY_MAY`，包括加入公告/音乐/输入服务，请求她鼓励出发的文本，并在终止前安装 `addNewFollower ALLY_MAY`。

邀请与鼓励是文本请求 1347/1348（键 `0543`/`0544`），不是选择菜单。已接受脚本没有是/否、拒绝、分支或地图请求。**已确认静态后果：**招募是可选的，因为选定旅行和准入谓词不测试此互动；但这个 F8 未设处理器一旦运行，源码顺序便进入加入。文本确认等待不构成同意分支。文本跳过路径不递增游标，因此这两条解析后的消息描述普通未跳过路径。

事件分发器暂时禁用实体更新，脚本 trap 则在执行时启用更新。其尾部关闭窗口并恢复更新。探索可在待处理为零的 `WaitForEvent` 读取时重新安装普通控制，条件是服务返回。这些等待不证明 May 存在或其 HP、自然场景完成或响应输入。缺失 character 映射可能解析到物理槽位 0，并非安全地跳过互动。强制缺失/死亡调用不是自然招募证据。

## 2. 加入、出战与跟随是不同结果

**已确认源码组合：**普通 `JoinForce` 使用方按以下顺序处理 May 成员身份：

1. 设置加入标志 F8，然后 `UpdateForce` 构建已加入/出战/后备列表及计数。
2. 若得到的出战人数少于 12，`JoinBattleParty` 设置出战标志 F40。人数达 12 或以上时跳过此调用；加入仍成功，不强制挤出其他成员。
3. 不再调用 `UpdateForce` 即返回。对于初始未设的 F40，新出战位尚未反映到刚构建的列表/计数中。之后更新（包括战斗放置）才重建它们。

满员出战队伍不取消后续鼓励或随从命令。容量分支不清除既有 F40；提供 F8 未设/F40 已设状态不一定表示未出战。加入既不测试 HP，也不复活 May。此脚本不调用 `joinForceAI`，不写入新的激活/AI 位域，因此既不确立也不清除既有 AI 控制。仅 F8 不能证明她是出战、存活、由玩家控制的战斗参与者。

**已确认当前跟随：**`addNewFollower` 解析 May 物理实体，目标为实际最后一个物理随从；列表为空时目标为物理实体 0。因此目标未必是 Peter 或 Bowie。`AddFollower` 复制一个 42-byte 循环随从行动块，修补其目标和 `(-24,0)` 偏移、推进 walking 存储，再在物理索引尚未存在时追加它。重复项仍会在重复检查之前分配并替换行动块。42 bytes 是行动块大小，不是列表容量；此使用方没有容量守卫。

跟随行动通过距离和布局条件使用目标的位置、朝向及目的位置。它不是即时传送，也不保证可见间距。若提供的重复项已是列表尾部，它可能选中自身；自然发生与呈现仍为**未知**。

| 后续边界 | 已接受源码所确立的内容 |
| --- | --- |
| 招募在当前实体群体中返回 | May 可保留新安装的当前跟随行动。招募自身既不重载实体，也不运行 Ribble init。 |
| 不含地图 init 的显式实体重载 | 重建实体、映射、walking 分配和按标志选定随从。持久随从表没有 May 行或 F8 谓词；它不重建此临时关系。此重载不调用依赖 F8 的 init 隐藏。 |
| 之后普通 Ribble 入场 | 重建选定实体群体，再运行 init。F8 不会在群体选择期间移除 May 编写行；init 检查 F8，并通过她当前映射调用 `MoveEntityOutOfMap(MAY)`，写入 `(0x7000,0x7000)`。默认/F708 未设配置和缺失映射限制仍适用。 |
| 普通大地图入场 | 重建新实体群体和按标志选定随从，而不是携带 May 临时行动/列表项。 |

招募不写入持久 May 随从标志。F8/F40 位于逻辑存档域内，探索列表、walking 指针与动态行动存储位于其外。这一存储区别及既有有界加入存档/读档用例，不证明 May 组合的跨存档呈现、随从容量或自然重新入场行为。这些仍为**未知**。

## 3. 旅行建议提供背景而非路线门控

**已确认静态对话：**工人与商人描述向东的旅程、向北的河道，以及前往 Mt. Volcano 途中的黑暗洞窟。他们的跳跃与朝向变化伴随从 1332（`0534..0538`）起的五个文本请求，说话者依次为 135,134,134,135,135。编写的工人在 `(7,11)`，商人在 `(5,11)`；两者事件行携带控制字节 7。

两个说话者不等于两个调用方。工人 event135 调用 `Map8_2C4_EntityEvent7` 与 `cs_56172`；商人 event134 调用 `Map8_2C4_EntityEvent6` 并打开武器商店。商店内部在本指南之外。指路脚本不设置招募或路线标志，也不请求地图切换。下文门控既不测试听过建议，也不测试招募 May。

**推断的叙事含义：**这些对话给出伙伴机会和后续旅行背景。措辞不证明地图相邻、可导航路径、河道可达或洞窟准入。选定地图连接来自已接受的传送/使用方证据，而非对话。

## 4. 离开城镇重建出战队伍的探索实体

**已确认条件连接：**选定 Ribble 出口是 Map8 首条传送行：X 通配、Y31，不滚动，通往 Map69 `(44,8)`、Down。普通移动必须先遇到传送布局属性；仅坐标匹配不确立合法路径。产生方写入完整 type-1 请求元组和传送 SFX。若请求存续且服务返回，无滚动使用方使 main 空闲、退出探索并将元组返回 `MainLoop`。

该行及其使用方均不测试 F8、F708、F709 或指路对话。Map8/69/70 没有按标志切换的替换。main 在普通 Map69 入场前检查战斗：携带的 F409 可选择 Battle09；F409 未设时，F417 可选择 Battle17。[Map69 准入所有者](../../../../../research/chapters/eastern-parmecia/new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)保留其顺序与完成行为。

若两场战斗均不满足，普通提供地图入场设置当前地图/非战斗状态、重建实体和标志随从、清除临时标志 256–383，并在地图加载/init 前设置 F80。F8/F40/F67/F708/F709 及战斗标志在该临时重置后保留。Map69 有空固定列表和 `rts` init。此重建不保留 May 当前跟随；她的已加入成员身份是另一项保留事实。

Map69 内，特定 X23/24/25 zone 可请求 Battle17；F509 未设时，默认 zone 可请求 Battle09。F509 已设不清除原已设置的 F409。这些继承的中断使连接保持有条件：离开 Ribble 不确立不受打断的东向步行。

## 5. 东边缘滚动进入 Map70 而不进行战斗准入

**已确认静态边缘：**Map69 选定 X63、Y 通配行向 Right 滚动进入 Map70 `(1,255)`，并将 Right 写入朝向参数。非零滚动使用方设置 `CURRENT_MAP=70`、调用 `ProcessMapTransition`、以瓦片 1 替换 main X；因目标字节为负，保留 Y。它加载相关瓦片/布局并施加滚动切换，然后通过 `DeclareRaftEntity` 重新定位木筏/随从，返回同一探索循环。

此边缘既不调用普通入场、实体初始化、地图 init、`SwitchMap`，也不调用 `CheckBattle`。它不清除临时标志，也不意味着普通入场治疗。参数 5 保留，但在此不作为新朝向赋值应用；实际朝向依赖实体/控制路径。既有 F410 不会仅因本次滚动到达 Map70 就选择 Battle10。

Map70 只有默认配置、空固定实体列表和 `rts` init。没有故事标志配置选择或 init 触发的 Battle10 场景。该 init 属于之后普通入场路径；选定滚动不调用它。

## 6. zone 请求与战斗选择器决定启动内容

**已确认静态产生：**普通 zone 处理仍要求布局属性 `$1400`。Map70 表将 X16/17/18/19、Y 通配匹配到特定处理器；仅默认匹配到达默认处理器。拒绝请求的特定辅助不会落入默认处理器。

| zone 选择 | 请求行为 |
| --- | --- |
| 特定 X16–19 | 向 `CheckRandomBattle` 请求 Battle14。F514 未设时立即请求。已完成时，非零步数计数器拒绝；否则，range-8 抽取为零则请求，非零抽取则建立 range-4-plus-2 冷却。请求设置 F414、写入 `$000100FF`、将步数计数器设为 30000，并执行其等待/SFX/闪光服务。 |
| 默认 | 若 F510 已设，不写入即返回。否则设置 F410 并写入 `$000100FF`。不增加招募、介绍、步数计数器或 RNG 谓词。 |

[随机战斗所有者](../../../../../research/world/map-event-random-battle-state.md)保留辅助规则；[请求所有者](../../../../../research/world/map-event-request-consumption.md)保留可变待处理槽位。type-6 zone 产生方将字 X 放入参数 1/2、字 Y 放入 3/4。其使用方初始化 main actscript 并调用 zone 分发器，后者在处理器之后关闭窗口、等待一个 VInt 和物理实体 0 停止。它不擦除新写入的请求。这是一个可变请求槽位，不是队列。

长字重载写入覆盖 type=1、参数 1=0（不滚动）和参数 2=255（`MAP_CURRENT`）；参数 3/4/5 保留。无滚动使用方采样 main 位置，但用对应非负参数替换各坐标，并始终将参数 5 用作朝向。对于完整普通 zone 谱系且 Y 在 0–63 时，返回元组为 `MAP_CURRENT,(0,Y),retained parameter 5`，未必是 main 采样位置/朝向。前序滚动向参数 5 写入 Right；只有没有中间写入方才保留它。

该无滚动返回后，main 执行 `SwitchMap` 和有序 `CheckBattle`。Map70 合格行依次为 Battle10（F410/F510）和 Battle14（F414/F514），两者触发坐标均通配。区域矩形是输出放置/视野数据，不是包含性测试。

- F410 已设即选择 Battle10，即使 F414 同时已设或提供坐标位于其区域之外。
- F410 与 F510 均已设时，清除 F410，但仍在同一次调用中返回 Battle10。完成不会立即跳到 Battle14；默认处理器因 F510 已设而返回也不会清除携带的 F410。
- F410 未设时，若 F414 已设则允许 Battle14；两者均未解锁时不选择战斗。

因此，F410 原已设时，Battle14 辅助请求可导致选择 Battle10。反之，特定辅助拒绝时，不会仅因存在解锁而进行准入。辅助参数、存续待处理请求和选择器结果是独立决定。

## 7. Battle10 开场为空但仍有介绍门控

**已确认新战斗顺序：**选择 Battle10 后，`BattleLoop` 清除 `PLAYER_TYPE`、设置 F399 并检查 F88。F88 未设时，它清除经过秒数、建立地图/战斗/基准 VInts、调用 before 包装器、恢复地图/战斗、清除区域标志 90–105、治疗存活/不死己方角色、先初始化己方再初始化敌方位置、清除 AI 内存、为战斗员 0 加载战斗，并调用 start 包装器。本指南止于该分发器返回、`ActivateEnemies` 之前。

已接受 before/start 两表精确 row-10 目标均为 `ms_Empty`。两个包装器使用 F460（介绍基数 450 + battle 10）：before 仅在未设时分发而不设置；start 在未设时先设置再分发。F460 已设跳过两个脚本，不跳过新战斗初始化。即使空脚本也使用解释器终止符，包括条件性滚动等待与速度清除。F510 完成是不同谓词。

Battle10 精灵集有十二条己方放置记录，不等于要求十二名己方角色或已有十二名加入者。放置重置己方 X 位置、调用 `UpdateForce` 并遍历实际出战列表。死亡成员跳过放置；列表推进和放置指针推进不同，剩余尾部分支可调用 `LeaveBattleParty`。May 当前探索跟随不决定成员身份或放置。仅 F8 既不证明 F40、HP 或槽位，也不证明玩家控制。

F88 已设则走替代恢复路径：恢复已保存秒数、清除 F88/AI 内存并加载已保存战斗状态，绕过新放置及两个介绍包装器。它分支走向单独回合（单次行动），而不经过此 start 分发器终点。这是明确替代路径，不是已通过本指南停止边界的实例。本文不纳入激活、回合（单次行动）、交战或结果。

## 证据审阅与剩余限制

本指南使用 PR #761 的已接受研究及 PR #762 的已接受欢迎前序。研究将 USA retail 绑定到固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`，拥有精确源码/文本/传送/表组合与复现。本文不主张重新调查上游、生成提取载荷或新运行时观察。直接受跟踪夹具及 schema/使用方审阅未发现必须修正的 Layer A；这些夹具仍窄于完整场景或路线。

| 审阅关系 | 保留的可执行/合同边界 |
| --- | --- |
| 互动、加入与跟随 | [sf2-map-events-static-v1](../../../../../../tests/fixtures/h2/map-events-static-v1.json)、[实体程序分片](../../../../../../tests/fixtures/h2/map-events/entity-programs.json) 顺序 664/669；[sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`forceStateCommandFacts` 及嵌套 `activePartyCommandFacts`；[出战队伍/名册合同](../../../contracts/party/party-roster-state.md)。Join/addNewFollower 源码位置使用命令索引 5/7，不同于研究从一开始的完整脚本记录 6/8。这些是选定调用方/命令事实，不是完整邀请或动作编排。 |
| 容量、列表时序与生命周期 | [sf2-common-stats-static-v1](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)、[sf2-force-state-active-party-runtime-v1](../../../../../../tests/fixtures/h3/force-state-active-party-v1.json) 和 [sf2-force-state-roster-death-runtime-v1](../../../../../../tests/fixtures/h3/force-state-roster-death-v1.json)；研究中直接 JoinForce/随从/重建/存档范围审阅。处理器矩阵不确立 May 自然行动者、容量或跨存档呈现。 |
| 普通入场与滚动 | [sf2-map-data-static-v1](../../../../../../tests/fixtures/h2/map-data-static-v1.json)、[sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json) 主体 12/76/77、[sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)；[地图/探索合同](../../../contracts/world/map-exploration.md)。精确选定边缘及无 init/重建组合仍归研究所有；未证明布局可达和完整请求谱系。 |
| zone 请求与行优先级 | [zone 程序分片](../../../../../../tests/fixtures/h2/map-events/zone-programs.json) 顺序 132/133、[sf2-map-event-random-battle-state-static-v1](../../../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)、[sf2-common-maps-static-v1](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)、[sf2-battle-global-data-static-v1](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)。产生方/辅助/选择器事实保持独立，不含连续组合。 |
| 空介绍与实际出战放置 | [sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[sf2-battle-spriteset-data-static-v1](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)；[过场合同](../../../contracts/battle/battle-cutscene-routing.md)。routing 的 `routeTargetsParsed=false` 不能证明精确目标；已接受研究拥有 row-10 `ms_Empty` 读取。没有夹具提供实际游玩的 Battle10 开场。 |

[地图脚本 schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)和[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)保留字段封闭的处理器/源码位置事实；[地图事件夹具加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)和[验证器](../../../../../../src/sf2tool/h2/map_events.py)保留有序分片与原始操作数。[地图 init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)和[验证器](../../../../../../src/sf2tool/h2/map_init.py)保留精确主体，而不证明自然调用方。研究的[覆盖与复现边界](../../../../../research/chapters/eastern-parmecia/ribble-may-through-battle10-admission.md#existing-coverage-reproduction-and-remaining-unknowns)列明各自所属窄 H2 命令及选定使用方。本篇文字不选择整套测试。

**未知：**合法路径/布局及激活行动者；当前物理映射、HP、出战/AI/随从状态；自然文本/输入/实体/滚动完成；待处理/参数存续；May 跨存档呈现和随从容量；连续可玩的 Battle10 入场。保持缺口明确，不将其变成模拟器采集队列。其他城镇内容、East Shrine/洞窟和 Battle10 结果在本指南之外；本文不选择新引擎或产品行为。
