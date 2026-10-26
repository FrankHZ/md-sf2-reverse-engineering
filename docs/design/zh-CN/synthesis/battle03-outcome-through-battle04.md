# Battle 03 结果至 Battle 04 准入

> 本文是[规范英文原文](../../synthesis/battle03-outcome-through-battle04.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle03 可以使出战队伍返回大地图。Hawel 的住宅提供下一段源码连接的场景：Kazin 找到师父，获得加入军团的理由，并解锁另一场遭遇。随后通过住宅出口离开，才提供可由其选中 Battle04 的请求。
**推断的剧情连接：**这些编写内容延续[Battle 02 结果至 Battle 03 准入](battle02-outcome-through-battle03.md)中前往 Hawel 住宅的旅程。
**已确认静态：**已接受的 [Battle03 至 Battle04 调查](../../../research/battle03-to-battle04-content-route.md)连接这些条件消费者。
**未知：**自然行走、实际 Battle03 返回状态、住宅调用方/随从状态，以及已初始化的 Battle04 出战队伍。尚未确立 Battle03 完成是住宅场景的必需谓词。

本指南解释地点、人物、场景内容和状态交接，止于 Battle04 准入与选定的 before/start 依赖。原版证据使用 USA 零售 ROM 和 `ShiningForceCentral/SF2DISASM` `master` 的 `c834c652b6862bc5679fd7f69a38a7093206efc6`，身份见[研究溯源](../../../research/battle03-to-battle04-content-route.md#agreement-and-provenance)。
**已确认**表示具名静态事实或明确受限的观察；**推断**表示叙事连接；**未知**指出缺少的执行证据。Battle03 和 Battle04 是从零计数的源码槽位 3 和 4（`BATTLE_TO_HAWEL_HOUSE` 与 `BATTLE_AMBUSHED_BY_GALAM_SOLDIERS`），并非已验证的原版 UI 编号。Hawel 的住宅是 Map43；Granseal Kingdom 大地图是 Map66。

重制实现不是原版证据。[已接受的私有重制里程碑](map3-battle01-readiness.md#当前已接受里程碑)止于 Battle01 可用返回；本指南不授予后续可玩支持。

接着读 [Battle 04 结果至 Battle 05 准入](battle04-outcomes-through-battle05.md)，了解各自独立的胜利/败北/逃脱返回、Galam 俘虏情节、Slade 跟随和条件守卫请求。它保留静态/自然及原版/重制边界，止于 Battle05 结算之前。

## 1. Battle03 将控制交回探索

**已确认共享结果规则：**控制器在行动死亡处理之后，以及单次行动结束效果及其死亡处理之后，再次统计战斗员。计入要求 X 非负且 HP 为正。己方角色为零时优先选择败北，之后才检查敌人为零的胜利；Bowie HP 为零会强制己方计数为零。Battle03 的敌方首领存在行是零，所以仅杀死敌方槽位 128 不是其胜利条件。独立的敌人败北包装器也为此遭遇选择 `ms_Empty`；它既不提供住宅传送，也不替代结果规则。[战斗控制合同](../contracts/battle-control-lifecycle.md)和[选定结果所有者](../../../research/battle03-to-battle04-content-route.md#conditional-transition-table)保留共享规则及选定源码关系，不提供获胜行动序列。

### 空 after 程序不招募 Kazin

**已确认选定分发：**F503 未设时，战后行 3 选择 `ms_Empty`；F503 已设则跳过该程序。两路都到达共享加入尾部，读取参与构建的 `data/battles/cutscenes/afterbattlejoins.asm` 行 3 的零，并调用 `JoinForce(0)`。零是 Bowie，不是不调用的哨兵值。空脚本不提供剧情对话、传送、标志或加入命令，但其解释器终止仍可能等待滚动，并清除滚动速度。包装器实际的加入调用仍会执行。Kazin 的招募属于下述住宅场景。

[过场路由合同](../contracts/battle-cutscene-routing.md)和[队伍成员资格合同](../contracts/party-membership-state.md#joinforce-时间线)区分分发、脚本效果与军团成员资格。`global/` 下未使用的同名加入表不是此包装器的输入。没有单独战斗脚本文件名，不能证明目标为空。

### 胜利返回最终状态操作数，随后场上等待另一事件

**已确认返回规则：**战后程序包装器之前，胜利治疗存活/不死己方角色，更新军团，并取得首名出战成员。它将当前地图、该成员最终战斗员 X/Y 加战斗区域原点，以及该成员实体朝向存入返回参数 2–5；参数 1 为零。因此 Map66 调用方使用依赖最终调用方状态的操作数返回 Map66，而非使用初始布置或固定住宅入口坐标。

包装器之后，胜利清除 F403、设置 F503，并在 `d0`–`d3` 中返回保存的地图/位置/朝向，`d4=1`。`MainLoop` 在 `alt_MainLoopEntry` 恢复，应用 `SwitchMap` 并进入 `ExplorationLoop`，不先重新检查战斗。此处不写入 F404。
**已确认静态后果：**下一遭遇需要自身的解锁与请求路径。**未知：**稳定后的位置/朝向、可用输入与后续行走。已接受的[返回分析](../../../research/battle03-to-battle04-content-route.md#conditional-transition-table)连接 `BattleLoop_Victory`（`0x23CBA`）和 `mainloop.asm` 的战斗返回位置（`0x75E0`）。

败北与逃脱仍是替代分支。**已确认静态：**普通 Battle03 败北调用解锁辅助函数、恢复 Bowie、将金币减半、取得撤离位置，并返回 `d4=-1`；Egress/Angel Wing 使用该辅助函数，返回 `d4=0`。只有 F503 原已设置时，辅助函数才清除 F403；普通首次失败或逃脱不把 Battle03 标为完成。前篇中的 Battle03 战前程序可选择 `EGRESS_MAP=MAP_YEEL`，但只在开场未播放、未挂起的分支。实际失败/逃脱目的地及后续行走仍为**未知**。

## 2. 进入 Hawel 住宅会选择初始化内容

**已确认传送关系：**Map66 的 `(6,5)` 记录请求 Map43 `(6,11)`、Up、无滚动。受控目标必须提供传送标记，并到达 `WarpIfSetAtPoint`。此表连接不证明能从 Battle03 最终返回位置合法走到那里。Map66 的 F508/F611 图块复制状态、住宅踩踏/屋顶记录和脚本图块变化，仍是行走依赖。[地图探索合同](../contracts/map-exploration.md)及[住宅内容所有者](../../../research/battle03-to-battle04-content-route.md#hawel-house-content-and-caller-state)将工作布局、标记、碰撞和行动者条件与传送元组区分。

### 场景在默认地图初始化时运行

**已确认配置选择：**Map43 唯一标志选择器是 F612。它未设时，选定 init `ms_map43_InitFunction`（`0x540A6`）调用嵌入的 `cs_540C0`（`0x540C0`）。F612 已设时，替代配置选择 RTS init 与空的编写实体列表。两者共享直接返回的默认实体及区域（事件）桩。场景属于初始化内容，不是与 Hawel 或局部物体交互；书本/井的描述提供背景，不写入 F404。F503 和 F72 均不是此配置/默认 init 调用方中的显式门槛。

**已确认行动者结构：**默认配置声明 Hawel（逻辑实体 128）和两名 Galam 士兵（129/130）。Hawel 初始位于地图外 `(63,63)`，程序把他放到 `(7,4)`。该流顺序落入替代列表的终止符。空替代列表不移除独立初始化的主实体与随从。Map43 和 Map66 均没有标志切换地图覆盖。[地图配置合同](../contracts/map-setup-data.md)拥有表结构；地图探索与已接受研究拥有选择器/init 行为。

### Kazin 的在场有独立随从路径

住宅默认列表不声明 Kazin。**已确认源码关系：**探索在配置实体之前初始化随从；随从表将 F72 绑定到 Kazin，其消费者在 F72 已设时声明他。此前 Yeel 对话调用跟随脚本，随后设置 F72，并未招募他。这为住宅场景提供了有源码支持的在场路径。已加入本身不能替代 F72 声明。

**未知：**自然进入住宅时取得的 F72/实体状态。没有 F72 门槛，既不证明 Kazin 缺席时编排能正常执行，也不证明 Yeel 必经。同样，没有 F503 门槛不确立从已完成 Battle03 开始的连续旅程。这些是调用方状态问题，不是应在场景中虚构的缺失谓词。

## 3. 住宅场景改变 Kazin 与出战队伍的关系

**已确认编写内容：**以下简述使用 `cs_540C0` 的命令/行动者操作数及固定 `data/scripting/text/gamescript.txt` 中匹配 ID，已经[接受的场景分析](../../../research/battle03-to-battle04-content-route.md#hawel-house-content-and-caller-state)审阅。它们描述编写内容，不是新观察到的渲染台词，也不独立证明画面外发生的事情。完整对话与编排仍属私有。

| 场景阶段 | 对话身份与行动者 | 内容关系 |
| --- | --- | --- |
| 抵达与闯入 | 普通游标 ID 811–816；Kazin，815 为士兵 129 | Kazin 认出住宅，呼喊未获回应，发现 Hawel 与士兵在内。程序重新放置 Bowie/Kazin，以摄像机 `(0,4)` 重载当前地图，并应用显式屋顶/踩踏事件。 |
| Hawel 的叙述 | ID 817–826；Hawel 128、Kazin 及 Bowie 的 822 耳语 | 士兵按脚本离开后，Hawel 讲述士兵索要他的 Ground Seal 笔记。Kazin 讲述封印开启与 Granseal 灾难。这些陈述给出场景解释，不是独立的画面外证据。 |
| 警告与失去 | ID 827–831；Hawel/Kazin | Hawel 解释被封印的危险与宝石，留下最后请求。程序使他颤抖/停止，并以 `setPosFlash` 移到地图外；Kazin 哀悼。这一呈现序列不是对 Hawel 的战斗员死亡写入。 |
| 共同目标 | ID 832–834；Kazin | Kazin 请求同行，以完成 Hawel 的遗愿，在 833 与 834 之间加入，再指向 Ancient Shrine。跟随和解锁操作在此之后。 |

普通文本路径安装游标 811，推进 24 次至 834。继续显示的 `nextText` 调用是 829 与 832；其他选定调用均关闭文本。场景没有针对 Kazin 请求的 yes/no 分支。加入处理器另行显示固定消息 446，不消耗本场景游标。[对话合同](../contracts/dialogue-system.md)拥有游标及显示处理器语义。Debug/P2-Start 跳过可抑制普通文本及其游标递增；这些范围描述普通分支，不是每次执行。

### 已加入成员资格、跟随与遭遇解锁各自独立

**已确认有序尾部：**悲伤加入 → 最终场景文本 → `followEntity` Kazin/Bowie → `clearF 72` → `setStoryFlag 4` → 终止。其状态域必须区分：

| 操作 | 已确认消费者效果 | 限制 |
| --- | --- | --- |
| Kazin 的悲伤 `join` | `csc08_joinForce` 读取 `0x8004`，清除 bit 15 以取得己方角色身份，选择悲伤加入音乐，并调用 `JoinForce(4)`，设置已加入 F4。 | 出战激活取决于容量 12。军团重建先于出战标志设置；此调用不确立出战列表已同步，也不确立实际 Battle04 名册。 |
| `followEntity` 与 `clearF 72` | 跟随消费者解析实体地址并调用 `AddFollower`；清除操作移除后续随从初始化使用的标志。 | 跟随不等于加入。清除 F72 不会将 Kazin 移出军团；自然显示的跟随/重入仍未知。 |
| `setStoryFlag 4` | `csc13_setStoryFlag` 加上战斗解锁基数 400，并设置 F404。 | 不设置 F4 或 F504、不清除较早遭遇解锁，也不请求转换。 |

[队伍/名册](../contracts/party-roster-state.md)、[队伍成员资格](../contracts/party-membership-state.md)与[通用脚本](../../../research/common-scripting.md)所有者保留这些消费者边界。

### 调用方写入发生在场景返回之后

**已确认生命周期区别：**较早的 `reloadMap` 对当前地图调用 `LoadMap`，既不递归运行配置 init，也不将住宅出口入队。`cs_540C0` 返回后，其 ASM 调用方设置 F612、设置 F650、再次清除 F72，并写入 `EGRESS_MAP=43`，随后返回。F612 选择住宅重复配置；F650 选择 Yeel 的替代配置，但仍受其后续选择器优先级约束。撤离值独立于传送请求。这些都不是行 4 的准入谓词。

场景结束后，住宅不会自动离开。提供 F612 已设/F404 未设状态，会选择 RTS init，并不补回缺失解锁。**未知：**自然完成场景、稳定后输入、重访人口，以及这些变化的存档/读档持久性。

## 4. 住宅出口提供 Battle04 请求

**已确认出口生产者：**Map43 唯一传送匹配通配 X 与 Y=14，请求 Map66 `(6,6)`、Down、无滚动。匹配调用时，`WarpIfSetAtPoint` 写入事件类型 1、参数 1 为零、目的地图/X/Y 至参数 2–4、朝向至参数 5。这些是出口操作数，不是 Battle03 返回快照，也不是此前随机辅助函数保留的坐标请求。场景后走到出口标记仍为**未知**。

`WaitForEvent` 轮询请求，`ProcessMapEvent` 在分发前清除其类型。
**已确认条件消费者：**参数 1 为零使 `ProcessMapEventType1_Warp` 退出探索到 `MainLoop`；非负目的坐标覆盖当前玩家位置。参数 1 非零则在探索内部滚动。随后 `MainLoop` 选择地图并检查遭遇。[请求消费所有者](../../../research/map-event-request-consumption.md)拥有轮询/清除/局部分发；已接受的[出口/准入分析](../../../research/battle03-to-battle04-content-route.md#returned-request-and-battle04-entry-dependencies)拥有下游组合。

提供 Map66、F402/F403 未设且 F404 已设时，**已确认条件选择：**`CheckBattle` 返回行 4 及区域 `(0,0,22,30)`。两个触发坐标均为通配值（255）；区域是输出原点/尺寸，不是准入矩形。假设中间服务成功返回，场景解锁与实际出口生产者是独立步骤。

选择按源码顺序扫描。较早已解锁的 Map66 行 2 或 3 会抢先于行 4，即使其完成标志已设：选择器清除该行解锁，并仍在同一次调用返回其索引。F404/F504 都已设时，也是在清除 F404 的同时返回 4。没有 F404 时，扫描可能到达后续已解锁行。Map66 默认区域（事件）辅助函数还可在其步数/RNG 条件下重新启用已完成 Battle03，归前篇所有。因此，场景或数字遭遇顺序不能替代真实选择器。[通用地图研究](../../../research/common-maps.md#confirmed-map-routing)保留共享扫描；提议的组合准入见证尚未实现。

## 5. 选定 Battle04 开场与 start

**已确认选定分支：**F88 未设且 F454 未设时，before 表行 4 选择 `bbcs_04`（`0x4981E`）。它安装游标 2311，以摄像机 `(1,5)` 加载 Map66，并加载独立场景实体表 `ce_4989E`。ID 2311–2318 中，Kazin 就 Hawel 一事质问士兵/Lemon，Lemon 下令抓捕。Bowie/Kazin、具名同伴、Lemon 与士兵精灵是布置的场景实体，不证明出战成员资格。此开场没有加入或 F404 写入；对话请求将来采取行动，不确立遭遇结果。

普通准备时，控制器为 before 安装地图/战斗，之后恢复其传入值、清除区域标志 90–105、治疗、初始化己方角色/敌人、清除 AI 内存、加载战斗，最后调用 start。精灵集槽位 4 选择 `BattleSpriteset04`（`0x1B34DC`），声明 5 个己方布置、8 个敌人条目、3 个 AI 区域及 1 个 AI 点。布置容量不是已加入或出战成员资格。地形槽位 4 是 `BattleTerrain03` 的别名；共享地形不使遭遇内容相同。[遭遇合同](../contracts/battle-encounter-definition.md)、[精灵集研究](../../../research/battle-spriteset-data.md)和[选定入场所有者](../../../research/battle03-to-battle04-content-route.md#returned-request-and-battle04-entry-dependencies)保留这些数据边界，不发布名册或网格。

before 包装器测试 F454，不设置它。start 包装器在将行 4 分发到 `ms_Empty` 前设置 F454；F454 原已设置会跳过两个程序。F88 挂起入场绕过普通准备和这些包装器。因此[共享开场合同](../contracts/battle-cutscene-routing.md#共享-intro-门)不承诺每次准入都重播开场。**未知：**自然初始化的 Battle04 成员资格、服务完成、呈现及首次单次行动。交战结算及所有后续剧情不属本指南范围。

## 证据审阅与剩余限制

本综合审阅了已接受研究、前篇、上方链接合同及下列选定公共载荷/验证器语义。原版选定源码发现仍归研究所有者。复现路线不是本综合的新运行；没有产生新测试夹具或原版观察。使用路线前，在拥有该工作的进程中加载 `local/private-inputs.ps1`。

| 实质主张 | 已审阅可执行所有者与复现路线 | 处置 |
| --- | --- | --- |
| 共享结果/返回 | [循环测试夹具](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`sf2-battle-loop-static-v1`，`lifecycleFacts.remainingCombatants`；[控制测试夹具](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`sf2-battle-control-static-v1`，`controlFacts`；[循环](../../../../src/sf2tool/h2/battle_loop.py) / [控制](../../../../src/sf2tool/h2/battle_control.py)验证器；`uv run sf2 h2 battle-loop`、`uv run sf2 h2 battle-control` | 共享计数与生命周期事实一致。Battle03 的最终状态返回规则、选定行和 Map66 返回是已接受源码发现，不是组合的 battle-3 运行时见证。 |
| 空程序/共享加入与开场 | [过场测试夹具](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`sf2-battle-cutscenes-static-v1`，`cutsceneFacts`；[验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)，`uv run sf2 h2 battle-cutscenes` | 包装器门槛/加入时序受到检查。精确槽位目标与零索引 JoinForce 使用研究的源码消费者。空程序不抹除包装器/解释器效果。 |
| 住宅 init 与调用方 | [init 测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`，`expected.routeJoins` 93/94、`scriptCallSites`、`primarySourceBodies`、`embeddedPrograms[id=cs_540C0]`；[验证器](../../../../src/sf2tool/h2/map_init.py)，`uv run sf2 h2 map-init` | 完整语义比较包括选定 init 调用及返回后写入。替代 RTS 路径不重播场景。嵌入操作索引与脚本测试夹具命令索引使用不同计数域。 |
| 成员资格、对话、随从与解锁 | [脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`，军团/剧情状态源码位置及处理器、地图生命周期 `cs_540C0:8:reloadMap`；[验证器](../../../../src/sf2tool/h2/map_script_engine.py)，`uv run sf2 h2 map-script-engine` | 已审阅源码位置保留 join 109、clearF 112 和 setStoryFlag 113；reload 是命令 8。位掩码、固定加入文本和基数 400 运算独立于自然台词/跟随。跟随命令 111/AddFollower、行动者/文本配对和 F72 声明使用已接受源码分析。 |
| 配置/实体/传送结构 | [配置](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`，Map43 顺序落入；[内容](../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[配置](../../../../src/sf2tool/h2/map_setup.py)、[实体](../../../../src/sf2tool/h2/map_entities.py)、[内容](../../../../src/sf2tool/h2/map_content.py)验证器；`uv run sf2 h2 map-setup`、`uv run sf2 h2 map-entities`、`uv run sf2 h2 map-content` | 结构事实不证明具备标记且考虑碰撞/占位的行走。精确传送及随从关系仍归选定源码所有者。空配置实体不确立实际人口为空。 |
| 出口请求与遭遇选择器 | [请求测试夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`；[通用地图测试夹具](../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[请求](../../../../src/sf2tool/h2/map_event_request_consumption.py) / [地图](../../../../src/sf2tool/h2/maps.py)验证器；`uv run sf2 h2 map-event-request-consumption`、`uv run sf2 h2 common-maps` | 请求证据止于局部分发；清单不组合出口 → Map66 → 行 4。该关系与较早行/同次调用选择反例使用已接受源码审阅；提议的现有所有者扩展尚未实现。 |
| 入场数据 | [精灵集](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`sf2-battle-spriteset-data-static-v1`；[路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1`；[精灵集](../../../../src/sf2tool/h2/battle_spriteset_data.py) / [路由](../../../../src/sf2tool/h2/battle_routing_data.py)验证器；`uv run sf2 h2 battle-spriteset-data`、`uv run sf2 h2 battle-routing-data` | `rowContentTracked=false` 和 `routeTargetsParsed=false` 保留清单限制。槽位 4 的目标/头部及 before/start 顺序使用选定研究源码，不是实际出战队伍证据。 |

控制器合同的 New Battle 总览把 before/start 接缝归组；不能将它用作 start 先于初始化的顺序见证。上方精确的 before → 准备 → start 顺序遵循已接受路线的源码分析和共享开场门槛所有者。通用运行时问题队列同样不覆盖这些更严格的局部边界。没有相冲突的可执行主张要求扩大本批。[研究缺口处置](../../../research/battle03-to-battle04-content-route.md#evidence-gaps-and-fixture-disposition)将组合见证提议保留在现有所有者；提议不授权新实现或自动模拟器工作。

**推断**仍是预期的下一剧情段。**未知**仍包括自然取得的结果/返回状态、考虑标记/碰撞/行动者的行走、住宅 F72/实体状态（含随从缺失时的编排）、普通场景及服务完成、渲染台词/输入/音频、重入/存档持久性，以及已初始化 Battle04。这些缺口限制连续攻略与重制准入主张，同时保留有用的条件内容解释。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有未来范围；[剧情推进](story-progression.md)提供更广状态框架。
