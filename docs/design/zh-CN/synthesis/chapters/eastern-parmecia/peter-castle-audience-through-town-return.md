# Peter 城堡觐见与返回城镇

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Peter 在 New Granseal 受到接待后，接续独立城堡觐见。Astral 与国王听取有关 phoenix 和 Volcanon 的讲述；Astral 提议陪 Peter 向东，但大臣与国王坚持让他留下，改派出战队伍。Peter 自我介绍，国王下令赠礼，Astral 在队伍出发前告别。**推断的叙事含义：**Peter 成为旅伴，而 Astral 留下。**已确认静态行为：**觐见使 Peter 以 AI 控制加入，布置临时随从并请求返回城镇；调用方随后设置独立完成与随从标志。**未知：**自然旅行、行动者/服务完成、观察到的金币余额和实际可用目标控制。

本文承接[New Granseal 城镇：Peter 首次登场](new-granseal-town-through-peter-introduction.md)。接续[New Granseal 出发至 Battle09 准入](new-granseal-departure-through-battle09.md)，了解选定大地图连接、条件战斗选择与激活前空开场。已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md)提供场景、文本、处理器及调用方审阅。本指南止于城镇首次 init 后待处理/控制读取。大地图出发、Battle09 和后续觐见不在本分段内。本文不增加重制能力，也不决定如何处理礼物。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#751](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/751) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。源码身份使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。下方命令索引是从零开始的源码记录，不是帧数或实测时长。

## 1. 返回城堡是独立准入

**已确认条件连接：**前一个 Peter 事件以调用方 F701 结束。它既不让 Peter 加入、不设置 F702/F67，也不重载实体群或请求此次城堡旅行。城镇北出口行匹配 Y=0、X 通配，经无滚动传送请求城堡 Map7 `(11,22)`、Up。普通移动在较早移动/碰撞工作后，对选定传送布局属性测试此行。匹配行不证明瓦片可通行，也不证明从 Peter 接待处自然步行而来。

若请求存续，探索在分发前消费并清除它，将显式元组返回 `MainLoop`。Map7 没有替换或战斗坐标行，因此进入普通探索入场。它清除待处理，应用已接受[存活/不死角色治疗与状态限制](../../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)，设置地图/非战斗状态，重建实体/精灵，清除临时标志 256–383，设置 F80，加载地图并在控制前运行 init。这不是全军团重置。

正常完成的前篇历史携带 F707/F411、`EGRESS_MAP=6` 与 `PLAYER_TYPE=0`。F706 属于独立水手/卫兵场景，不必随 F701 一起设置。其他出战队伍、随从和后期历史状态仍是提供的输入。

## 2. 觐见资格与可用角色群不同

**已确认配置优先级：**Map7 依次扫描默认、F701、F702 和 F805；最后适用行胜出。所有变体共享 zone 表和 init 函数。

| 提供的历史 | init 前选定实体群 |
| --- | --- |
| F805 已设 | F805 实体群：11 个 NPC 声明，没有固定 Astral。 |
| F805 未设，F702 已设 | 默认实体群：12 个 NPC 声明，即使 F701 已设。 |
| F805/F702 未设，F701 已设 | 觐见实体群：五个己方角色和 12 个 NPC 声明。 |
| 三者均未设 | 默认实体群。 |

觐见实体群将 Peter 放在 `(11,6)`、Left，与 Astral 引用 139 `(10,6)`、Right 相对。国王 129 位于 `(11,4)`、Down，大臣 128 位于 `(12,5)`、Down。Sarah、Chester、Jaha 和 Kazin 分别在 `(12,8)`、`(10,8)`、`(13,8)` 和 `(9,8)` 朝 Up。这些是声明操作数，不是保证的物理槽位或观察位置：初始化先分配按标志选定的随从，若己方角色已映射，就跳过其固定声明。

共享 init 只测试 F707。已设时返回，不改变实体群或 egress/F411。未设时先写 `EGRESS_MAP=6`，再运行[一年后讲话](new-granseal-castle-year-later-control.md) (`cs_55832`)，仅在正常返回后设置 F707/F411；该场景显式恢复默认实体群。因此，提供的 F701 已设/F707 未设历史不保留普通五己方觐见布置。觐见本身使用行动者前不重载他们。

**已确认觐见谓词：**五个 zone 点 `(9..13,8)` 全部选择 `Map7_ZoneEvent0`。移动必须另行在 zone 布局属性上产生 zone 请求；这些行与城堡入场元组都不证明自然 zone 可达性。使用方在分发前清除待处理并重新解析配置。

| 调用方输入 | 直接结果 |
| --- | --- |
| F802 未设，F701 已设，F702 未设 | 运行 `cs_55A36`；正常返回后设置 F702，再设置 F67。 |
| F802 未设，F701 未设 | 跳过觐见，与 F702 无关。 |
| F802 未设，F701/F702 已设 | 跳过；不修复缺少的 Peter 成员身份或 F67。 |
| F802 已设 | 先进入较后 F805 选择器；其另一觐见函数体不在本指南内。 |

F707、F706、F67、成员身份、AI 与行动者存在都不守卫早期觐见。F802 未设时 F805 也不是守卫：异常 F805 已设/F701 已设/F702 未设历史可在不同角色群下准入早期脚本。这些提供状态约束解释，不主张自然可达历史。

## 3. Astral 提议同行；改由队伍出发

**已确认有序内容：**`cs_55A36` 是有限、无分支的 88 命令序列，含 39 个连续对话 ID 1198–1236。已接受研究比较了源码注释与对话文本。下文说明交流，不复现转录，也不将人物说法视为已独立建立的世界事实。

摄像机移向觐见区域。Astral 与国王讨论 Peter 的 phoenix 身份和 Volcanon；Peter 描述受伤翅膀并求助。Astral 移动、向他点头，然后转向队伍，提议与 Peter 一起向东旅行。大臣上前质疑。Astral 逐个等待的向左跳跃动作、一次等待及向右移动先于其回复。大臣和国王坚持让他留下。

国王改派队伍，Astral 同意。Peter 靠近并自我介绍，文本 1230 (`04CE`) 描述他陪伴队伍。该叙述先于实际 `joinForceAI` 命令。脚本随后移动 Peter，安装五条临时随从链，并在赠礼交流前停顿。Astral 留下是**推断的叙事含义**；程序不遣散他，也不清除独立提供的 F70/F74 随从标志。

具名移动等待只汇合其具名行动者。决定期间大臣向上一格动作不被等待，而 Astral 随后的 left/up 动作被等待。定时等待不能独立证明较早动作已完成，其操作数不是实测呈现时间。HP 守卫依命令而异：下方五个跟随调用探测 Bowie，而非具名随从；加入命令没有相应存活守卫。缺失映射或异常随从历史也约束布置。脚本返回不证明每个行动者、摄像机、文本、声音或移动服务已可见地完成。

## 4. 加入、场景跟随与城镇跟随分别处理

**已确认加入语义：**命令 60 是 `joinForceAI ALLY_PETER,$FFFF`。第二个字是非零选择器，不是激活字段替换值。处理器读取 Peter 旧激活字，按位 OR 入 `AIBITFIELD_AI_CONTROLLED` (4)，调用 `JoinForce`，然后存储 OR 后的字。其他激活位存续。零选择器仅清除 AI 位，不会加入。

`JoinForce` 设置已加入成员身份，以 `UpdateForce` 重建军团列表，检查重建后的出战数量，仅在低于源码定义容量时设置出战成员身份。在此固定源码中，Peter 标志为已加入 F7、出战 F39，容量为 12。因此原先不存在的 Peter 加入，有空位时进入出战队伍；满员时加入，但不设置原先未设的出战标志。已出战 Peter 保持出战；已加入的后备 Peter 有空位时可转为出战。重复命令重新确立 AI 控制，再次执行重建/容量决定，不创建另一个成员。这些局部数值事实不扩展[成员身份合同的符号容量边界](../../../contracts/party/party-membership-state.md#joinforce-时间线)。

重建先于可能的出战标志写入，`JoinForce` 内没有第二次重建，因此列表可滞后于新设置的出战标志。本次觐见较后正常文本处理很关键：礼物文本的 `{LEADER}` 替换调用 `UpdateForceAndGetFirstBattlePartyMemberIndex`，先重建，再查首名出战成员名字。调试文本跳过可绕过该重建。辅助局部测试夹具和静态顺序都不建立城镇控制时观察到的列表。加入处理器不重置 HP、不重命名 Peter、不播放加入音乐、不安装随从，也不设置 F67。

**已确认临时跟随：**命令 62–66 在守卫允许时，安装 Peter 跟随 Bowie、Sarah 跟随 Peter、Chester 跟随 Sarah，以及 Jaha/Kazin 分别按不同偏移跟随 Bowie。三个操作数都是字；依该顺序，随从字为 `$0007,$0001,$0002,$0003,$0004`。HP 探测读取各首字的高字节，即零：五次调用全都检查 Bowie。Bowie HP=0 会跳过每次调用；Bowie 存活时，Peter 或其他具名随从 HP=0 本身不跳过这些命令。这些提供状态区分守卫，不证明自然可达性或行动者存在。允许时，`followEntity` 解析两个引用，调用 `AddFollower` 安装行走脚本并登记物理随从。[研究操作数/处理器审阅](../../../../../research/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md#membership-ai-and-the-two-follower-mechanisms)保留精确三元组与已有受控高字节观察。复用探索列表中已有随从仍会替换其行走脚本。这些操作不让任何人加入或出战，也不设置持久故事随从标志。其表偏移不保证观察到的最终位置。

**已确认目标跟随：**只有 zone 调用方在脚本返回后设置 F67。目标重建替换临时行动者/脚本，使用普通随从表 F67 行分配 Peter。该行测试 F67，而非 F7/F39 或 AI 控制；即使提供 Caravan F65，Map6 也使用此普通表。其他随从标志仍独立。场景不设置持久 Sarah/Chester/Jaha/Kazin 随从标志。任何状态说明都必须区分成员身份、出战成员身份、AI 控制、临时跟随和 F67。

## 5. 受审链中的礼物叙述没有有效授予

国王要求赠礼；大臣点头、走开、停顿并返回送上礼物。文本 1234 (`04D2`，命令 76) 叙述向领队赠送 100 金币。**已确认于受审静态边界：**字符串包含领队替换、普通字面数字 100、换行与 W1 等待。没有数值奖励操作数或隐藏金币指令。

完整程序没有自定义调用或金币变更命令。受审显示/token 链为 `{LEADER}` 重建列表，并执行文本/布局/输入工作；不经 `SetGold`、`IncreaseGold` 或 `DecreaseGold` 调用赠礼授予，也不直接写 `CURRENT_GOLD`。zone 调用方只设置 F702/F67。因此该受限源码链不实现所宣称的 +100 奖励。列表重建是真实文本侧状态效果，与入账分开。

**未知：**自然到达的零售版觐见前后金币余额、创作者意图，以及针对该差异的任何重制决定。本指南保留叙事/状态不符，不提供运行时余额主张或修复。[研究礼物审阅](../../../../../research/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md#the-narrated-gold-gift)拥有精确文本与服务锚点。

## 6. 出发先产生请求，再设置调用方标志

Astral 移动，告别并提出忠告；Bowie 点头。停顿后摄像机跟随 Bowie，逐个等待他 down-18 移动。该移动不建立目标：命令 86 显式以无滚动类型 1 传送请求城镇 Map6 `(11,1)`、Down；命令 87 结束程序。

**已确认顺序：**加入/AI 与临时随从先于请求产生；请求先于脚本返回，以及调用方 F702、随后 F67。传送写待处理事件字和元组，不进入城镇、不改变 `CURRENT_MAP`，也不赋值 `WARP_SFX`。存在对话窗口时，脚本结束处理可在返回前等待视野滚动。zone 包装器随后关闭立绘/文本，等待一次 VInt，再等待物理实体 0 停止；不汇合整条随从链，也不清除此新产生请求。

下一次探索 `WaitForEvent` 时，存续非零待处理绕过普通控制安装。`ProcessMapEvent` 在分发前清除它，无滚动使用方播放/清除提供的传送 SFX，使主实体 idle，并将显式元组返回 `MainLoop`。这是一个可变待处理槽位，不是队列：中断或覆盖可破坏该链。觐见中断可令已加入/AI 或临时随从已改变，而 F702/F67 尚未到达。

## 7. 城镇重入先重建实体群，再读取控制

**已确认条件准入：**Map6 没有替换或战斗坐标行。普通探索入场清除待处理，应用共享治疗限制，选择地图 6，重建实体/精灵，清除临时标志 256–383，设置 F80，加载地图并运行共享 init。上述成员身份与故事标志位于该临时清除范围外。在这些入场假设下，主实体使用 `(11,1)`、Down；Peter 通过 F67 重新准入，替代场景跟随。

F702 现在在城镇配置顺序中胜过 F701。其实体群含 14 个 NPC 声明，引用 128–141，没有固定 Peter 或大臣。Peter 是否存在取决于随从选择，而非该实体群列表。共享 init 单独调整此选定实体群：

| init 历史 | 待处理/控制读取前调整 |
| --- | --- |
| F802 已设 | 将祭司 132 放在 `(11,18)`、Down；不测试 F706。 |
| F802 未设，F706 已设 | 将工人 138 放在 `(63,63)`、Down，卫兵 139 放在 `(15,23)`、Down。 |
| F802/F706 未设 | 不运行脚本或重新定位，直接返回。 |

正常早期觐见完成携带 F802 未设，因此后两行是其替代情况；第一行是输入状态对照。这些 init 脚本既不写标志/请求、不招募任何人、不授予金币，也不重启 Peter 介绍。觐见不赋予玩家类型或 egress。

条件性调色板/音乐工作后，首次 `WaitForEvent` 读取是停止边界：

| 该次读取状态 | 直接源码结果 |
| --- | --- |
| 待处理非零 | 绕过控制安装并处理请求；未建立可用控制。 |
| 待处理为零，携带玩家类型 0 | 设置视野目标 0，调用 `SetControlledEntityActScript(0)` 安装 `eas_ControlledCharacter`；后续循环再次读取待处理并测试 A/C。 |
| 待处理为零，提供的玩家类型非零 | 保留载具控制分支。 |

init 返回与此读取之间没有无条件待处理清除。F702 已设/F67 未设重入跳过觐见，不修复 Peter 随从标志。**未知：**实际可用输入、自然旅行、可达历史/映射、行动者/服务完成、待处理存续和存档持久性。止于此处；后续城镇服务、大地图出发与 Battle09 需要独立已接受所有者，不会从本条件控制边界自动得出。

## 证据审阅与限制

已接受研究提供场景特定对话、实体群、内联动作、金币链审阅及组合请求/调用方时序。已跟踪测试夹具审阅保留下方更窄可执行边界。没有分歧要求研究/合同修正：通用合同保留调用方特定列表同步问题，而本已接受觐见审阅识别正常 `{LEADER}` 重建及其调试跳过限制。精简测试夹具不执行整个场景。

| 关系 | 已审阅可执行所有者 | 限制 |
| --- | --- | --- |
| 配置、行动者与 init | [sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`expected.sourceFacts` / `selectionCases`；[sf2-map-entities-static-v1](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)，`expected.consumerFacts`；[sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`expected.primarySourceBodies[10]` / `[11]`。 | 共享选择器、映射与 Map6/7 init；不是自然旅行或有保证的场景行动者。 |
| 觐见调用方 | [sf2-map-events-static-v1](../../../../../../tests/fixtures/h2/map-events-static-v1.json) 与 [zone 程序分片](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，`expected.zoneTargetPrograms` `Map7_ZoneEvent0`。 | F802/F701/F702 分支及脚本 → F702 → F67；不是自然 zone 准入。 |
| 加入/AI 与场景命令 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`expected.forceStateCommandFacts.activePartyCommandFacts`，包括 `cs_55A36:60:joinForceAI`；[sf2-common-stats-static-v1](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)。 | 命令/处理器与辅助顺序，不是完成的觐见服务或城镇列表状态。 |
| 处理器局部出战队伍/跟随 | [sf2-force-state-active-party-runtime-v1](../../../../../../tests/fixtures/h3/force-state-active-party-v1.json)，连同[名册合同](../../../contracts/party/party-roster-state.md)与[成员身份合同](../../../contracts/party/party-membership-state.md)。 | 受控辅助用例；不主张觐见金币差值、自然招募或存档/控制。 |
| 待处理使用方/控制 | [sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`eventRequestConsumption.consumerContexts.waitForEvent`；[sf2-common-maps-static-v1](../../../../../../tests/fixtures/h2/common-maps-static-v1.json) 与 [sf2-common-scripting-static-v1](../../../../../../tests/fixtures/h2/common-scripting-static-v1.json)。 | 共享访问/使用方事实，没有已执行的连续觐见至城镇组合。 |

[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)、[状态块 schema](../../../../../../schemas/h2/map-script-engine-state-block.schema.json)、[map-init 验证器](../../../../../../src/sf2tool/h2/map_init.py) / [测试夹具 schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)，以及[请求消费验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)保留各自受限比较。其既有窄命令是 `uv run sf2 h2` 后接 `map-script-engine`、`common-stats`、`map-setup`、`map-entities`、`map-init`、`map-events`、`map-event-request-consumption`、`common-maps` 或 `common-scripting`。
**本次文档切片未重跑这些验证轨道与 H3 测试夹具。**[研究复现所有者](../../../../../research/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md#coverage-discriminators-and-reproduction)保留源码锚点与剩余组合。不主张重新调查源码或运行时观察，保留的未知项不自动授权获取。

本指南既不完成战役时序，也不完成 epic #514。英文原文将中文同步留待路线图另行安排；原版证据与未来重制决定保留各自所有者。
