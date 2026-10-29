# New Granseal 城堡：一年后讲话与首次控制交接

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

建设分段请求 New Granseal 城堡。其首次访问场景呈现一年后的集会：大臣肯定重建工作，国王谢绝讲话，Astral 提议建立贸易联盟，大臣遣散众人。**推断的叙事含义：**新定居点已经建成，注意力从建设转向未来。**已确认静态行为：**城堡初始化通过 F707 选择场景，以普通实体群替换集会实体群，然后返回首次待处理事件/控制读取。**未知：**自然抵达、移动及呈现服务完成，以及实际可用控制。

本文承接[Battle08 结果至 New Granseal 城堡交接](battle08-outcomes-through-new-granseal-handoff.md)。已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md)提供固定源码、对话、行动者列表及调用方审阅。本指南止于首次 init 后待处理/控制读取。后续 Peter 内容、城镇探索和战斗仍是独立工作；本文不增加重制能力。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#743](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/743) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。源码身份使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。命令索引是从零开始的脚本记录，不是帧数或实测时长。

## 1. 抵达先选择实体群，再选择讲话

**已确认条件入场：**前篇提供城堡 Map7 `(11,10)`、Up。Map7 没有标志替换或战斗坐标行，因此该已接受准入选择探索。预期完成历史携带 F408 未设、F508/F700/F705 已设和玩家类型 0；F458 来自普通 Battle08 start。这些事实不重置无关故事或随从历史，不建立自然抵达，也不提供 F707 值。

探索清除旧待处理字，治疗存活/不死己方角色，并执行入场加载。它设置当前地图 7 和非战斗状态，选择/重建实体并加载精灵，清除临时标志 256–383，设置 F80，加载地图/基础 VInt，然后在安装控制前调用 init。临时清除不包括 F411/F707 和下方配置标志。此入场治疗保留[共享 HP/状态限制](../../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)；不重复建设的全军团重置。

[已接受配置审阅](../../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#entry-setup-priority-and-init-selection)区分**实体群选择**与**场景选择**。Map7 依次扫描默认、F701、F702 和 F805；每个已设标志替换候选，因此最后适用行胜出。

| 提供的配置历史 | 初始实体群 |
| --- | --- |
| F805 已设 | `ms_map7_flag805_Entities`：11 个 NPC 声明，没有 Astral 声明。 |
| F805 未设，F702 已设 | `ms_map7_Entities`：12 个 NPC 声明；即使 F701 已设，F702 也恢复默认。 |
| F805/F702 未设，F701 已设 | `ms_map7_flag701_Entities`：五个己方角色和 12 个 NPC 声明。 |
| 三者均未设 | `ms_map7_Entities`，预期早期历史实体群。 |

三个配置指针表全部选择**同一个** `ms_map7_InitFunction`。因此异常 F701/F702/F805 历史会改变初始实体群，却不抑制讲话。固定声明也不排除按标志选定的随从，不建立实际出战队伍名册。

## 2. 首次访问改变 Egress；重复入场跳过场景

**已确认 init 顺序：**`ms_map7_InitFunction` 测试 F707，独立于配置历史、入场坐标、Battle08 标志或提供的 F411。两条路径保留状态不同：

| 入场时 F707 | init 行为 |
| --- | --- |
| 未设 | 设置 `EGRESS_MAP=6`（New Granseal 城镇），运行 `cs_55832`，然后**在脚本正常返回后**依次设置 F707、F411。提供的 F411 已设不跳过场景。 |
| 已设 | 立即返回，保留提供的 egress、F411 和初始选定实体群。没有场景重新定位或最终显式实体群重载。 |

城堡 Map7 与城镇 Map6 仍然不同：设置 egress 不把玩家移入城镇。若场景停滞，egress 已改变，而调用方两次标志写入尚未到达。F707 已设/F411 未设入场**不**修复 F411。源码注释将 F411 称为 Battle11 已解锁，但此写入不执行战斗扫描或请求，不提供立即 Battle11 遭遇。

## 3. 集会赋予重建叙事含义

**已确认有序布置：**`cs_55832` 加载 Map7，摄像机为 `(6,2)`，安装集会列表 `ce_559AE`，等待 Bowie init，重置/重载显示并呈现一年后叙述。列表将 Bowie 放在 `(11,7)`、Up，面向前方国王与大臣。卫兵和士兵、两名女仆及 Astral 组成 NPC 角色群；Jaha、Sarah、Kazin 和 Chester 有固定己方声明。[行动者与阶段所有者](../../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#address-actors-and-ordered-phases)保留其精确引用、声明和全部 78 条命令。

这些是经 `ENTITY_INDEX_LIST` 解析的场景引用，不是物理槽位或军团成员身份。实体初始化先声明按标志选定的随从；已映射己方角色可保留该映射，而不接受固定放置。NPC 引用分配独立。因此角色群描述集会预期布置，不证明各行动者实际起始位置、招募或随从状态。加载列表不治疗或招募任何人。

大臣上前，肯定重建努力、避免事故的必要性与士兵贡献。他转向国王并呼唤两次。国王回复无话可说时启动动画，然后停止；大臣回应并邀请 Astral。Astral 与大臣依次执行逐个等待的步伐，再向集会说话。

Astral 谈到过去一年、建设完成和缺乏贸易，然后提议联盟。大臣致意并遣散众人。**推断：**场景标志着从建立居所转向联络其他定居点。其叙述与讲话不是实测游戏时钟推进、建设模拟、贸易系统或联盟选择。程序不包含物品栏、招募、联盟或时间推进操作。启动或停止动画改变动画计数器，不改变健康、死亡或成员身份。

### 显示变化不重新开始访问

**已确认处理器区别：**`mapLoad` 和 `loadMapFadeIn` 中正 Map7 操作数，即使当前地图已是 7，也走 `LoadMap` 的新地图显示分支。它们重载地图资源/布局，以 `(6,2)` 为摄像机输入，而非 Bowie 目标。它们不再次调用城堡 init、重建实体、清除故事标志或安装控制。`loadMapFadeIn` 在顺序进入地图加载处理器前设置淡出状态种子；较后的显式淡入仍独立。

`resetMap` 清除工作布局存储并以地图索引 -1 加载；`reloadMap` 也以 -1 和其摄像机操作数调用。这些影响当前地图显示/区域状态，不影响持久进度或探索初始化。[共享地图加载合同](../../../contracts/world/map-exploration.md#加载与选择顺序)与[场景处理器审阅](../../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#map-loading-and-action-semantics)使这些路径分离。调用不建立可见淡入淡出、摄像机、文本或音乐时序。

## 4. 遣散先启动离开，再恢复普通实体群

**已确认动作顺序：**六名卫兵/士兵不等待完成地启动离开序列，然后脚本启动并等待士兵 136。Astral 启动未等待步伐；Jaha 首次 Down 步伐被等待，随后 Jaha、Sarah 和 Kazin 不等待完成地启动离开。Chester 离开被启动并等待。然后在重载前，Astral 与引用 132/135 获得 Down 朝向命令。女仆没有离开命令。

每个具名动作等待测试该实体脚本指针是否 idle。等待 136、Jaha 或 Chester 不显式汇合较早行动者；实体群替换前没有全局完成屏障。动作和 init 处理器直接解析当前映射，不过滤死亡己方角色。这不同于使用存活状态守卫的位置/朝向/动画命令：零 HP 己方角色仍可接受这些动作脚本。已有随从映射可改变动作使用的实体和起始位置。成功运动、阻挡和调度仍为**未知**；未建立逐个行动者最终离开坐标。

命令 76 显式重载 `ms_map7_Entities`，保留主实体当前瓦片坐标和朝向。该普通列表恢复国王、大臣、卫兵、士兵、女仆和 Astral，混合 init、行走与水平循环声明。它没有固定己方记录，但按标志选定的随从仍适用。实体群替换丢弃/重建动作记录，不证明较早未等待离开已完成。与场景列表加载不同，此重载没有直接 `LoadEntityMapsprites` 调用，因此不能证明精灵交付已完成。

重载使用**显式默认列表**，不再次选择配置。因此在异常提供的 F701/F805 历史下，它可替换实体群，同时保留那些标志及选定配置的实体事件指针。在任意历史下，后续实体群和交互配置不一定匹配；解析这些后续交互超出本指南范围。F707 已设入场则保留初始选定列表。

## 5. 调用方返回条件控制边界

**已确认源码派生位置：**假设主实体映射成立且 init 服务正常完成，F707 未设场景将 Bowie 留在 `(11,7)`、Up。其被等待 init 返回 idle，之后没有主实体移动/朝向命令；显式重载保留该元组。F707 已设入场跳过场景声明，在相同无中间变化假设下保留 `(11,10)`、Up。两个元组都不是已观察的领队终点。

脚本结束通过解释器条件性滚动等待与正常尾部返回。只有之后 init 调用方才写 F707 和 F411。当前地图仍为城堡 7，执行路径的 egress 仍为城镇 6，提供的玩家类型、军团成员身份和无关持久标志贯穿保留。该返回没有第二次治疗、探索初始化、战斗准入扫描或待处理清除。

在条件性音乐/淡入淡出处理和移动声音配置后，探索到达 `WaitForEvent`。**首次 init 后待处理字读取**是本指南终点：

| 该次读取状态 | 直接静态后果 |
| --- | --- |
| `MAP_EVENT_TYPE=0`，携带 `PLAYER_TYPE=0` | 设置视野目标 0，并在实体 0 上安装 `eas_ControlledCharacter`。 |
| `MAP_EVENT_TYPE=0`，提供的玩家类型非零 | 共享控制设置器选择载具控制；场景不将玩家类型规范化。 |
| 非零待处理字 | 跳过控制安装。若后续读取仍非零，该事件返回探索分发器。 |

入场清除了旧待处理字，场景及 init 调用方都不显式产生事件。若无中间写入，零选择普通控制路径。该字是单个共享槽位，不是队列；服务写入、存续或覆盖未被观察。没有将特定事件归于本次讲话。安装控制脚本不建立实际可用输入或呈现完成，且在执行路径上，两个调用方标志在此读取前均已设置。

## 证据审阅与剩余问题

**已确认审阅结论：**已接受研究、前篇和下方已跟踪选择在条件链上相符。init 函数体保留场景前 egress 与调用方写入；脚本见证保留同图显示加载和显式默认实体群重载。动作处理器守卫区分具名等待与全局完成，不含其他命令族所用的存活状态检查。没有证据冲突要求合同或研究修正。场景特定对话、内联移动、角色群与共享 init 指针身份来自已接受研究的固定源码审阅；精简测试夹具不重复整个场景。

已审阅测试夹具包含下方链接的精确 ID。窄命令前缀为 `uv run sf2 h2`；它们指明既有可执行所有者，**未为本次文档切片重跑**。

| 关系 | 已跟踪测试夹具选择 / 窄命令 | 主张边界 |
| --- | --- | --- |
| 初始配置与共享 init | [sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`expected.selectionCases` `map7-alias-restores-default` / `map7-final-variant-wins`；[sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`expected.primarySourceBodies[11]`；[sf2-map-entities-static-v1](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)；`map-setup`、`map-init`、`map-entities`。 | 选择器见证、声明/使用方和七操作 init 函数体；自然配置历史未被证明。 |
| 场景加载、动作与重载 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`expected.transitionCommandFacts`、`entityPopulationCommandFacts`、`entityActionBridgeCommandFacts`、`entityLifecyclePresentationCommandFacts`；`map-script-engine`。 | `cs_55832` 加载索引 1/4/6/10、列表索引 2/76 与处理器守卫/顺序。它们不执行场景或闭合每个行动者运动。 |
| init 返回与待处理/控制边界 | [sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`eventRequestConsumption.consumerContexts.waitForEvent`；[sf2-common-maps-static-v1](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)、[sf2-common-scripting-static-v1](../../../../../../tests/fixtures/h2/common-scripting-static-v1.json)；相同 basename 命令。 | 独立共享使用方事实；没有可执行组合证明从抵达到可用控制。 |

[map-init 验证器](../../../../../../src/sf2tool/h2/map_init.py)与[测试夹具 schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)、[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)与[schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)，以及[请求消费验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)保留比较边界。[配置合同](../../../contracts/world/map-setup-data.md#合同边界)覆盖配置身份，与[地图探索合同](../../../contracts/world/map-exploration.md#加载与选择顺序)分离。地图合同的概念性配置/init 顺序描述探索入场；已接受处理器审阅区分脚本内显示加载。历史处理器观察是受限合成边界，不是本次访问的自然观察。

**未知，保留：**自然抵达与可达配置/随从历史；实际己方映射与 HP；成功具名动作和未等待离开；完成的显示/精灵/摄像机/文本/淡入淡出/音频服务及其时序；中间待处理写入、玩家类型历史和实际可用控制；存档/读档持久性。反例包括 F707 已设/F411 未设跳过、F805 已设且 F707 未设、已映射或死亡己方角色、未完成的未等待离开、非零玩家类型、待处理覆盖，或调用方返回前服务停滞。它们约束解释，不建立自动获取队列。

接续[New Granseal 城镇：Peter 首次登场](new-granseal-town-through-peter-introduction.md)，了解条件性城堡出口、城镇配置、独立 zone 场景与事件后控制边界。

本指南既不完成战役时序，也不完成 epic #514。后续城镇/Peter 内容和战斗需要各自已接受所有者。英文原文将中文同步留待路线图另行安排。
