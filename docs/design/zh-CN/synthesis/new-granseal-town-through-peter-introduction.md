# New Granseal 城镇：Peter 首次登场

> 本文是[规范英文原文](../../synthesis/new-granseal-town-through-peter-introduction.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

城堡一年后讲话之后，接下来的城镇场景将注意力引向一只大鸟与围观人群。孩子们追逐、威胁 Peter，发现他会说话，并受到大臣训斥。大臣道歉，欢迎 Peter 来到新定居点，并邀请他见国王。**推断的叙事含义：**陌生访客从被围观转为作为宾客受到接待。**已确认静态行为：**两个独立 zone 调用方布置这些场景，并在脚本返回后各自标记完成。**未知：**自然旅行、行动者和呈现工作完成，以及实际可用控制。

本文承接[New Granseal 城堡：一年后讲话与首次控制交接](new-granseal-castle-year-later-control.md)。已接受的[研究所有者](../../../research/new-granseal-town-through-peter-introduction.md)提供源码、对话、实体群和处理器审阅。本指南止于 Peter 城镇事件及调用方 F701 之后的首次待处理/控制读取。城堡觐见、后续城镇服务、大地图和战斗需要独立已接受所有者。本文不增加重制能力。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有 [#747](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/747) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。源码身份使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。脚本命令索引指从零开始的源码记录，不是帧数或实测时长。

## 1. 离开城堡请求独立城镇访问

**已确认条件连接：**城堡 Map7 南出口行匹配 Y=23、X 通配，使用无滚动传送请求城镇 Map6 `(11,1)`、Down。移动使用方在较早移动/碰撞处理之后，仅对选定传送布局属性测试此行。X 通配不使该行每个瓦片均可步行。该行和前篇控制边界都不证明自然步行到出口。

前篇正常完成的首次讲话留下 F707/F411 已设及 `EGRESS_MAP=6`；重复城堡入场保留提供的 egress/F411。南出口不测试这些值。仅设置 egress 从不将玩家移到城镇。匹配移动请求必须在单个可变待处理槽位中存续，直到探索消费它。使用方在分发前清除待处理字，使主实体 idle，并将显式城镇元组返回 `MainLoop`。

Map6 没有标志替换或战斗坐标行，因此该准入保留 Map6 并选择探索，独立于携带的 F411。探索清除待处理，按[共享 HP/状态限制](../../../research/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)治疗存活/不死己方角色，设置地图 6 和非战斗状态，选择/重建实体并加载精灵，清除临时标志 256–383，设置 F80，加载地图，并在首次控制读取前运行 init。持久故事与军团/随从历史仍为提供的输入；这不是全军团重置。

## 2. 城镇历史选择角色群；init 单独调整

**已确认配置优先级：**Map6 依次扫描默认、F701 和 F702。最后适用行胜出，因此 F702 优先于 F701。三种配置共享相同 zone 表和 init 函数，但实体群不同。

| 提供的历史 | 选定实体群 |
| --- | --- |
| F701/F702 未设 | 默认 `ms_map6_Entities`：Peter 加 19 个 NPC 声明，包括孩子、水手/卫兵和大臣一行。 |
| F702 未设，F701 已设 | `ms_map6_flag701_Entities`：14 个 NPC 声明，没有固定 Peter 或大臣/护送者记录。 |
| F702 已设，F701 任意 | `ms_map6_flag702_Entities`：14 个 NPC 声明，同样没有那些固定记录；仅属后期历史选择。 |

默认列表将 `ALLY_PETER`（己方 ID 7）放在 `(18,25)`、Up，带水平循环动作。男孩 136/137/140 围绕该区域，位于 `(16,25)`、`(21,25)` 和 `(19,28)`。引用 138 使用工人精灵，位于 `(10,12)`、Right；139 使用卫兵精灵，位于 `(11,12)`、Left。水手/士兵描述来自调用方注释与叙事，而非独立物理槽位身份。大臣 144、卫兵 145 和工人 146 起始于 `(63,63)`、Down，准备接受显式场景放置。附近其他市民组成围观人群。[实体群所有者](../../../research/new-granseal-town-through-peter-introduction.md#setup-ordinary-population-and-init)保留完整声明列表。

这些引用经当前角色到实体映射解析。初始化先创建按标志选定的随从；已映射己方角色跳过固定声明。因此提供的 Peter 随从可改变本场景使用的实体与起始位置。NPC 引用与物理分配独立映射。固定 Peter 实体不是军团成员身份的证据。

**已确认 init 优先级：**`ms_map6_InitFunction` 先测试 F802，再测试 F706，不写任何标志。它使用所选实体群提供的映射：

| init 历史 | 控制前调整 |
| --- | --- |
| F802 已设 | 将祭司引用 132 放到 `(11,18)`、Down，然后不测试 F706 即返回。 |
| F802 未设，F706 已设 | 将 138 放到 `(63,63)`、Down，将 139 放到 `(15,23)`、Down。 |
| F802/F706 未设 | 不运行脚本，直接返回。 |

此 init 不启动两个长场景中的任何一个，不改变 egress、不招募任何人，也不重置 F701/F702。将行动者放到 `(63,63)` 保留其映射；与后续场景中显式隐藏操作不同。卫兵重入放置也不同于其首次事件移动结果。

## 3. 水手与卫兵引出骚动

**已确认独立分发：**zone 坐标 `(10,9)` 或 `(11,9)` 选择 `Map6_ZoneEvent0`。F706 未设时运行 `cs_54E86`，仅在正常返回后设置 F706，然后直接返回。F706 已设时跳过该介绍。重复路径上，F802/F803 可选择独立后期问候；其函数体不在本指南内。首次执行不顺序进入该问候。

卫兵与水手讨论一只大鸟和聚集的人群。卫兵决定调查，要求另一人报告国王。他逐次等待的移动让他靠近人群，中间穿插跳跃、朝向变化与等待。脚本还不等待完成地启动水平动作。较后，它显式将存活 Bowie 放到 `(10,9)`、Down，让 138 向上一步，并报告该动物。然后等待 138 离开路径并隐藏他，再返回调用方。

在默认映射与服务完成条件下，卫兵从 `(11,12)` 出发的移动操作数总和得到 `(14,25)`、Right。这是**已确认条件性源码结果**，不是观察终点。它不是重入 init 元组 `(15,23)`、Down。Bowie 位置命令有己方 HP 守卫：当前 HP 为零的 Bowie 跳过它。等待动作只汇合该具名实体的 idle 脚本；定时停顿不是对较早未等待动作的独立汇合。

返回并由调用方设置 F706 后，zone 包装器关闭立绘/文本，等待 VInt 与主实体停止移动，再返回探索待处理/控制读取。程序不调用 Peter 场景。到达其独立 zone 需要另一次分发；两个 zone 之间的自然移动仍为**未知**。

## 4. Peter 的接待有自己的触发

**已确认谓词：**zone `(19,23)` 选择 `Map6_ZoneEvent2`。F701 未设运行 `cs_54F26`；F701 已设跳过。**没有 F706 前提**，也不测试 F707、F411、F702、F802、成员身份或 HP。坐标是分发参数，不证明 Bowie 实际所在瓦片。

该分离影响内容规划。F701 未设/F706 未设可在未运行介绍时选择 Peter 事件。F702 已设/F701 未设可在缺少其固定行动者的后期实体群下选择它；F701 已设/F706 未设仍可在变体实体群下选择第一场景。这些是针对假定强制故事顺序或行动者可用性的输入状态反例，不建立自然可达存档历史。

### 孩子们发现 Peter 会说话

开场给 Peter 与男孩 136/137/140 内联速度操作数 `(48,48)`，并禁用 Peter 自动朝向。男孩们轮流靠近；Peter 踏步躲开。他们的移动不等待完成地启动，Peter 的回应则逐一等待。一个男孩评论他的速度。Peter 转身抗议，男孩们发现他会说话且反对他们的对待方式，作出反应。

交流升级为叙述中的殴打威胁。男孩们继续不等待完成地靠近；Peter 接受被等待的 init 和向左跳跃动作，然后大臣开口。**已确认区别：**该威胁没有伴随伤害或 HP 指令。速度操作数不是实测速度。Peter 较早 init 后恢复速度 48；跳跃前的 init 后没有恢复，因此开场速度不是 Peter 或整个场景的永久属性。

### 大臣改变城镇对他的接待方式

程序请求震动，让人群转向打断者，停止震动，并显式将大臣 144 和护送者 145/146 放在集会上方。大臣与卫兵不等待完成地开始步伐，只等待工人 146 路径。大臣训斥孩子们；他们退走时，男孩 136/137 也不等待完成地启动，仅等待 140。摄像机转向交流，大臣上前，女子 130 接受单独等待的移动。

对话随后包含道歉、欢迎及大臣自我介绍，解释新定居点，包含 Peter 回应，并邀请他见国王。**推断：**他的说话能力与大臣干预将敌意围观转为正式欢迎。有序源码命令确认了布置的交流，但不建立摄像机、震动、文本、声音或运动服务完成及其时序。

### 离开布置不建立城堡抵达

大臣不等待完成地启动 Up-eight 路径，Peter 对应路径被等待。护送者转向内侧，摄像机恢复中性，最后离开时大臣、Peter 和卫兵不等待完成地启动，仅等待工人 146 的不同路径。脚本随后隐藏 144、145、146 和 Peter，等待女子 130 返回步伐，使她朝 Down，然后结束。

**推断：**邀请和移动布置 Peter 跟随大臣走向城堡。**未知：**完成城堡旅行。两个早期场景都没有传送、地图加载、实体群重载、招募、军团重置或持久随从选择写入。城堡觐见是本终点之后的独立结果。

## 5. 隐藏结束布置，然后调用方返回控制

**已确认处理器区别：**动作/init 处理器使用当前映射，不跳过死亡己方角色。它们等待具名行动者 idle 脚本，而非全部较早行动者。位置与朝向处理器则跳过当前 HP 为零的己方引用；NPC 引用绕过该守卫。因此 Peter 的提供映射与 HP 可改变姿态，但不抑制 zone 调用方。

`HideEntity` 将当前位置及目标 X/Y 移至 `$7000`，移除匹配索引映射，并从当前探索随从列表移除匹配条目。它不改变 HP、不清除军团成员身份或持久随从选择标志，也不汇合隐藏行动者的动作。提供的 Peter 随从可经隐藏离开**当前**链；这不是持久遣散、死亡或招募。最后被等待的护送者也不证明三次未等待离开在隐藏前已完成。

脚本正常返回时，当前地图仍为 6。Peter 程序不给 Bowie 位置命令；其显式朝向命令依次是 Up、Left，受 HP/映射守卫约束。不主张自然领队终点。调用方仅在此时设置 F701。**设置 F701 不立即替换实体群：**当前行动者/动作与显式隐藏继续保留。未来入场在 F702 不胜出时选择 F701 列表，再运行 init。该列表卫兵声明 `(13,24)`、Up，及 F706 已设 init 覆盖 `(15,23)`、Down，是重入操作数，不是追溯性场景结果。

调用方 F701 后，`RunMapSetupZoneEvent` 关闭立绘/文本，等待 VInt 与主实体停止，并经类型 6 处理器返回。探索设置移动声音并调用 `WaitForEvent`。**首次事件后待处理读取**是本指南停止边界：

| 该次读取状态 | 直接静态后果 |
| --- | --- |
| `MAP_EVENT_TYPE=0`，携带 `PLAYER_TYPE=0` | 将视野目标设为物理实体 0，并安装 `eas_ControlledCharacter`；后续循环再次读取待处理并测试 A/C。 |
| 待处理为零，提供的玩家类型非零 | 选择共享载具控制分支。场景不将玩家类型规范化。 |
| 待处理非零 | 跳过控制安装；若第二次读取仍非零，返回事件供分发。不将特定产生方或下一事件归于这些场景。 |

zone 分发前已清除待处理，两个早期程序与调用方都不产生请求。没有中间写入时，零选择普通控制。这是条件静态结果，不是可用输入观察。调用方 F701 与该读取之间没有第二次探索 init、治疗或待处理清除。F707/F411/egress 贯穿保留；F706 仅经独立事件改变。脚本/服务停滞可令其调用方标志尚未到达。

## 证据审阅与剩余问题

**已确认审阅结论：**已接受研究、前篇、共享合同与已跟踪测试夹具选择在条件链上相符。init 函数体 10 保留 F802 先于 F706 的优先级；zone 程序分片保留独立 F706/F701 测试和返回后写入。脚本见证区分具名等待、未等待动作、放置守卫和隐藏。没有冲突要求所属研究或合同修正。城镇特定角色群、对话、内联动作、精确配置行与 `HideEntity` 辅助效果依赖已接受研究的固定源码审阅；精简测试夹具不复现整个场景或建立自然游玩。

已审阅测试夹具有下方精确 ID。窄命令使用前缀 `uv run sf2 h2`，指明既有可执行所有者；**未为本次文档切片重跑**。场景特定说明使用已接受研究；不主张新源码调查或运行时观察。

| 关系 | 已跟踪选择 / 窄命令 | 限制 |
| --- | --- | --- |
| 配置、行动者与 init | [sf2-map-setup-static-v1](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`expected.sourceFacts` / `selectionCases`；[sf2-map-entities-static-v1](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`expected.consumerFacts`；[sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json)，`expected.primarySourceBodies[10]`；`map-setup`、`map-entities`、`map-init`。 | 共享选择/映射事实与八操作 Map6 init。选择用例不提供专门 Map6 历史见证。 |
| 独立 zone 调用方 | [sf2-map-events-static-v1](../../../../tests/fixtures/h2/map-events-static-v1.json)、[zone 程序分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)，`expected.zoneTargetPrograms` `Map6_ZoneEvent0` / `Map6_ZoneEvent2`；`map-events`。 | 精确调用方操作，不是碰撞/布局或自然 zone 可达性。 |
| 动作、姿态与隐藏 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`expected.entityActionBridgeCommandFacts`、`entityPlacementCommandFacts`、`entityLifecyclePresentationCommandFacts`；`map-script-engine`。 | 源码位置键包括 `cs_54E86:21:setActscript`、`cs_54E86:33:hide`、`cs_54F26:138:entityActions` 至 `141:entityActionsWait`，以及隐藏 142–145。处理器守卫不证明离开已完成。 |
| 待处理与控制 | [sf2-map-event-request-consumption-static-v1](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`eventRequestConsumption.consumerContexts.waitForEvent` (`wait-for-event`)；[sf2-common-maps-static-v1](../../../../tests/fixtures/h2/common-maps-static-v1.json)、[sf2-common-scripting-static-v1](../../../../tests/fixtures/h2/common-scripting-static-v1.json)；相同 basename 命令。 | 共享使用方，没有从城镇准入到实际可用控制的可执行组合。 |

[map-init 验证器](../../../../src/sf2tool/h2/map_init.py)与[测试夹具 schema](../../../../schemas/h2-map-init-static-fixture.schema.json)、[脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)与[schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)，以及[请求消费验证器](../../../../src/sf2tool/h2/map_event_request_consumption.py)保留比较边界。[配置合同](../contracts/map-setup-data.md#合同边界)与[地图探索合同](../contracts/map-exploration.md#加载与选择顺序)约束共享入场语义。[研究复现所有者](../../../research/new-granseal-town-through-peter-introduction.md#coverage-counterexamples-and-reproduction)保留源码锚点和缺失组合；本指南不需要新比较器。

**未知，保留：**自然出口/zone 旅行与可达配置/随从历史；实际映射、HP、阻挡和动作完成；显示/摄像机/文本/音频时序；待处理存续或覆盖、玩家类型历史、实际可用控制和存档持久性。没有 F706 的独立第二次分发、F702 选定而缺失的行动者、死亡或已映射己方角色、隐藏时未完成动作、服务停滞，以及非零待处理/玩家类型状态都约束解释。它们不建立自动运行时获取队列。

本指南既不完成战役时序，也不完成 epic #514。英文原文将中文同步留待路线图另行安排；后续觐见与路线需要各自已接受证据。

接续[Peter 城堡觐见与返回城镇](peter-castle-audience-through-town-return.md)，了解独立觐见谓词、Peter 加入/跟随、叙述礼物，以及条件性城镇重入待处理/控制边界。
