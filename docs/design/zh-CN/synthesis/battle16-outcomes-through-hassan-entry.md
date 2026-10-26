# Battle16 结果、下筏与条件 Hassan 入场

> 本文是[规范英文原文](../../synthesis/battle16-outcomes-through-hassan-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

击败 Kraken 可让出战队伍以木筏模式返回 South Parmecia。败北或逃脱改选 Polca 存档点。离筏是独立事件：保存离开位置、安排向前一步、恢复跟随，最后返回步行模式。随后选定 South Parmecia 出口请求 Hassan，其实体群与 init 取决于当前标志。出口与城镇入场均不确立所有人已上岸或步行控制可用。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/battle16-outcomes-through-hassan-entry.md)支持下文结果、下筏与入场机制。**推断：**一致的行动者与已完成移动可以组合为遭遇后的成功登陆。**未知：**自然交战/旅行、实际调用方状态、服务完成、原版表现、可用输入与持久性。Fnnn 表示标志 nnn；除另指外，坐标为地图瓦片操作数，记录索引从零开始。

前段准入/start 边界见 [Kraken 现身与 Battle16 进入](south-parmecia-through-kraken-battle16-entry.md)，独立登乘机制见[木筏登乘](polca-raft-boarding-through-south-parmecia-entry.md)。本指南止于重建 Hassan 的 init 与首次条件控制交接，早于城镇行动、zone、实体、物品、服务对话、祭司事件、Rick/Rohde 互动或后续剧情。不增加连续游玩或重制能力。

接续 [Hassan 祖先与 Rohde 的 Caravan 选择](hassan-ancestry-and-rohde-caravan-choice.md)，了解独立祭司 zone 与 Rohde 交谈/选定物品路径。该指南区分初次/重复选择、当前跟随与条件菜单/控制返回，不假定本入场已有可用步行，也不推进至剑的获取、Rick、离开、Taros 或 Caravan 使用。

## 遭遇如何结束

**已确认调用方顺序：**单个行动后，BattleLoop 调用败北钩子、处理死亡战斗员列表并统计剩余己方/敌人。零己方优先于零敌人。若均未结束交战，则运行行动后效果，重复列表/计数/败北优先检查，但该检查点不再调用败北钩子。

Battle16 钩子要求 Bowie 存活且 enemy128 为零 HP。第一个初始化敌人行是 Kraken 头；谓词测试该槽位 HP，不测试名称或精灵。F516 清除选择空败北脚本；F516 已设则跳过。两者都到达首领尾部。Battle16 首领行值为 -1，因此尾部将全部剩余非零 HP 敌人加入死亡处理队列。

死亡处理执行表现/属性记账，并把列出的战斗员 X 标为 -1。计数要求 X 非负且 HP 非零；Bowie HP0 强制零己方。因此击败头部可将存活肢体排除计数，而不把其 HP 变为零。两类计数同时为零选择败北。头部若仅在行动后效果中死亡，该处不会额外调用钩子；死亡服务停滞不能确立胜利完成。[结果所有者](../../../research/battle16-outcomes-through-hassan-entry.md#outcome-selection-and-return-state)保留精确调用方、首领与计数关联。

| 选定结果 | 状态与返回后果 |
| --- | --- |
| 胜利 | 治疗符合条件己方；以当前地图及首个选定出战队伍成员的位置加战斗区域原点和映射朝向初始化返回元组。该成员不必是 Bowie。将 parameter1 置零，运行 after 包装器，清除 F416、设置 F516，随后读取返回 parameters2–5 并返回 result1。 |
| 败北 | 更新解锁标志，请求力竭 text363/音乐，恢复 Bowie HP，以无符号右移将金币减半并选择逃脱元组；返回 result-1。Battle4 特殊失败不适用。没有 after 场景或新 F516。 |
| Egress / Angel Wing | 消费选定法术 MP 或已用物品槽位，请求文本/闪光服务，更新解锁标志并退出行动调用帧至逃脱元组；返回 result0。没有败北金币惩罚或胜利完成。 |

仅当 F516 原已设置时，解锁更新才清除 F416；首次失败/逃脱保留解锁。**F416 解锁、F516 完成与 F466 登场/start 保持独立。**此处任何结果均不改变 F466。共享治疗把存活己方与不死 Peter/Lemon 恢复至 HP/MP 最大值，保留既有 STUN/POISON/CURSE 位，不复活其他死亡己方或缺失成员。

败北/Egress 显式选择 **Polca13**，忽略携带的 EGRESS_MAP。正常 BattleLoop 入场已设 F399，因此存档点为 `(18,25)`、Up。F64 已设时，Polca 重置行把已保存木筏元组写为 **Map72 `(43,48)`**；F64 清除则保留传入元组。畸形 F399 清除的直接调用在重置扫描前走更早房间回退，而非正常返回。正常战斗入场也已把 PLAYER_TYPE 清为0。败北/Egress 不写木筏模式，因此 mode0 仍取决于该调用方且无中途变更，与保存的木筏位置独立。此分支止于返回选择，早于 Polca 内容。

## 首次胜利与返回河流

**已确认完整场景：**after 包装器入口 F516 清除时，`abcs_battle16`（位于 `0x4B1BC`） 恰有三条记录。F516 已设则全部跳过。

| 记录 | 直接效果 |
| --- | --- |
| 0: `executeSubroutine csub_4B1D0` | 写 PLAYER_TYPE=2，即木筏模式。此精确主体不同于清除模式的相邻 `csub_4B1CA`。 |
| 1: `warp MAP_OVERWORLD_SOUTH_PARMECIA,29,51,LEFT` | 将地图解析为69；写入待处理 type1、parameter1=0 及地图/X/Y/朝向元组。不加载地图、移动行动者、保存木筏元组或请求传送声音。 |
| 2: `csc_end` | 经普通终止符的条件视野滚动等待与滚动速度清除返回。此场景没有对话或行动者行动程序。 |

此处 **BattleLoop 在包装器后直接读取元组**，不运行探索的普通 type1 传送消费者。首次胜利返回 `(69,29,51,Left,result1)`，经过 `alt_MainLoopEntry`，请求 SwitchMap，**不经 CheckBattle** 进入 ExplorationLoop。69 没有地图切换替换，不能仅因胜利返回该处便立即扫描其 battles9/17。待处理与参数字节是可变槽位，不是原子队列。

首次和重复胜利均到达共享加入尾部。After-join row16 调用 `JoinForce(0)`：设置 Bowie 的 F0、更新军团，并在低于 capacity12 时有条件地把他加入出战队伍。这不是空尾部或新同伴招募。返回种子已在此前选定。

| 包装器处传入 F516 | 返回与交通 |
| --- | --- |
| 清除 | 运行三条记录、设置 mode2，以 Map69 `(29,51)`、Left 替换种子；随后清除 F416/设置 F516。 |
| 已设 | 保留种子 Map49、可变坐标/朝向和传入模式，通常0。仍运行加入尾部和胜利标志写入。没有重复固定河流返回或木筏模式写入。 |

重复 Map49 使用前段 `ms_Void` 探索回退，不是新地图场景。模式与元组写入之间中断，可留下 mode2 与旧目的地；传送后中断，可留下 F516 清除。完成标志不证明场景刚运行过或服务已结束。after 场景没有指令写 RAFT_MAP/X/Y，已完成表现也不证明存档持久性。

重新返回 Map69 时，探索清除待处理状态、减少/钳位步数计数器、治疗符合条件己方、加载地图/瓦片并重建实体。选择当前 map69/battle255，清除临时 flags256–383，设置 F80。Map69 无固定配置行动者，init 直接返回。战斗行动者/查找表被替换。Mode2 给 main0 木筏精灵，将探索列表行动者移至 `(64,64)` 并安装站立行动，在该处以 Left 声明独立物理木筏31，lookup63→31。此分支既不需要 F64，也不需要 RAFT_MAP=69，不保存元组。

当前随从标志提供新实体群；F65 与 overworld69 共同选择大地图表。实体群与治疗均不证明特定出战名册。随后为条件音乐/淡入和 `WaitForEvent`；待处理为零时，物理0 接收 mode2 控制器。[返回所有者](../../../research/battle16-outcomes-through-hassan-entry.md#complete-after-scene-and-fresh-south-parmecia-return)保留完整场景与实体群边界；实际更新/输入完成仍未知。

## 离开木筏

**已确认生产者：**尽管源码名称如此，mode2 选择 `eas_Caravan` 和实际实体 opcode8。输入、边界与当前行动者阻挡检查后，门/传送/移动标记分支优先。否则原始工作布局字按无符号解释小于 `$C000` 时请求**数值5**，不执行移动尾部；至少 `$C000` 则不请求。尝试朝向可能已经改变，但 type5 分支不推进目的地。F64、木筏坐标、成员资格和战斗标志不是附加生产测试。

固定源码中 `MAP_EVENT_GETOUTOFCARAVAN=5`，`MAP_EVENT_GETOUTOFRAFT=4`。实际 numeric5 路径为 `ProcessMapEventType5_GetOutOfRaft → j_MapEventType5 → MapEventType5`。探索先清除待处理状态。与登乘不同，此消费者没有对齐/准入拒绝。[完整顺序所有者](../../../research/battle16-outcomes-through-hassan-entry.md#numeric-type5-tuple-save-disembark-and-restoration)支持如下序列：

1. 把当前地图与**物理0 的目的地**保存至 RAFT_MAP/X/Y。每个目的地字先符号扩展，再除以每瓦片384 内部单位，存入商的低字节。此为向前移动之前的离筏位置，不是登陆点或物理31 位置。它是 RAM 更新，不是磁盘/SRAM 持久性回执。
2. 令物理0 空闲，设置精灵链接排序 byte_FFAFB0=1，运行 `cs_45470` 全部四条记录。
3. 向当前 EXPLORATION_ENTITIES 列表安装随从离筏行动，调用其停止等待。
4. 恢复按标志选择的随从行动指针，最后写 PLAYER_TYPE=0 并返回。

| `cs_45470` 记录 | 行动者、移动与等待 |
| --- | --- |
| 0 | 被等待的逻辑 FOLLOWER_D159 行动复制**物理0**的位置、目的地、行程和朝向，刷新自身精灵并空闲。正常 lookup63→31 将独立木筏放在出发位置。 |
| 1 | 解析逻辑 Bowie0，选择其当前己方地图精灵并写入，等待一次 VInt 后刷新。PLAYER_TYPE 仍为2。 |
| 2 | 被等待的 Bowie 行动请求根据当前位置/朝向向前一瓦片、传送声音、自身目的地等待和空闲。不复制木筏。 |
| 3 | 普通脚本终止符，带前述条件滚动等待/速度清除。 |

随从行动先复制**物理木筏31**，再使用相同向前移动/声音/自身目的地等待/空闲主体。这些脚本移动不重复布局/占用检查，也没有登乘式额外 wait6。在映射一致、出发坐标已稳定、朝向正常且服务完成时，Bowie/随从踏离而木筏留下属于**推断的成功登陆**，不是已证明的自然登陆。

逻辑 FOLLOWER_D159、物理木筏31 与逻辑 FOLLOWER_B31 身份不同。地图命令等待解析目标查找映射，轮询该行动者的空闲行动指针；没有己方 HP 守卫。Bowie 精灵替换也不跳过 HP0。查找缺失或别名可把目标变成 main0 或其他行动者，而随从复制仍读物理31。复制/精灵替换不修复映射、成员资格或 HP，也不重置所有行动/速度字段。下筏不治疗或复活任何人。

探索列表含物理索引，以 `$FF` 结束。重建实体群单独声明 main0，因此辅助名不证明主角在列表中。空列表立即返回；否则**停止等待只检查最后列出行动者**，不检查所有人。恢复随后扫描当前随从标志，从物理1 起向连续槽位写跟随指针。不重建列表或解析每个逻辑查找映射。稳定标志/实体群可使这些状态权威一致；变化标志/列表可使其分歧，更早行动者尚在运行的离筏行动可能被覆盖。最终 mode0 写入既不证明全部行动者登陆，也不证明控制可用。

部分执行可已保存元组、复制木筏、恢复 Bowie 精灵或移动随从，而模式仍为2。后续 mode0 实体群重建只在 F64 已设且已保存木筏地图匹配当前地图时声明独立木筏。再次登乘仍需独立已接受的布局/F64 生产者与物理目的地对齐。下筏与已保存元组均不自动再次登乘；本指南不推广至其他交通工具消费者。

## 选定 Hassan 出口

**已确认请求：**Map69 row2 匹配 `(26,55)`，把 `MAP_HASSAN` 解析为 **Map9**，请求无滚动 `(14,1)`、Down。其他七行均不在该点竞争；匹配器使用首匹配优先级和负坐标通配符。

普通步行 opcode2 与实际木筏 opcode8 都可在各自移动检查后调用工作布局 `$1000` 传送生产者。Opcode8 在测试 type5 前处理它，并继续移动尾部。该行/生产者既不测试完成/F518、F64、成员资格，也不测试交通模式。**下筏不是本请求的显式前提。**自然水陆几何与可达性仍未知；Map69 返回 `(29,51)` 不是到 `(26,55)` 的旅行回执。

本请求包含传送声音，且**确实使用普通探索 type1 消费**：清除待处理，请求/清除声音，令 main0 空闲，退出探索调用层，采样玩家坐标，替换非负 X/Y 并使用传入朝向。返回 `(9,14,1,Down,result0)` 至 `MainLoop/@Start`。保留 PLAYER_TYPE 与 RAFT_MAP/X/Y。单独传入非零 parameter1 会走滚动，而非本行无滚动路线。

Map9 没有切换表替换，且**没有战斗地图行**。因此不论传入解锁标志如何，CheckBattle 此处均不准入战斗。Hassan 入场本身不启动 Battle17/18。[请求所有者](../../../research/battle16-outcomes-through-hassan-entry.md#selected-map69-to-hassan-request)保留完整行比较与符号到编号关联。

## Hassan 实体群与条件控制

**已确认重建生命周期：**探索现在使用 Map9 与 `(14,1)`，治疗符合条件己方、重建物理实体/全部64 查找字节、清除临时标志并设置 F80，保留交通模式。F518 选择默认 `ms_map9` 或替代 `ms_map9_flag518`；实体列表不同，但均使用 `ms_map9_InitFunction`（位于 `0x568C4`）。选择与 init 重新读取标志，因此变化的传入状态不必构成一致快照。

即使有 F65，Hassan 仍用普通随从表，因为它不是大地图或排除地图。标志/地图精灵选择随从；这不是通用出战队伍成员资格测试。实体群依次声明随从、配置行动者、main0，复用既有非零己方查找映射，避免重复。入场操作均不让 Rick 或 Rohde 加入。

两套配置列表都有十条非己方行，连续映射至逻辑128–137，以及位于 `(16,59)`、Down 的 Rohde11 行。默认有11 条配置行；F518 替代有12 条，增加 `(12,3)`、Right 的 Rick12。Merchant137 在两者中均最后，位于 `(16,3)`、Down。替代配置还改变一名战士的行走位置、祭司朝向/行动和一名年轻人的行走原点。此实体群说明用于识别 init 目标，不提供对话/服务清单。

完整十三操作 init 含三组有序条件移动和返回：

| 操作 | 条件与效果 | 行动者/成员资格限制 |
| --- | --- | --- |
| 0–3 | F518 清除：将逻辑137 移至地图外。 | 目标是商人，不是 Rick。F518 已设则不动。 |
| 4–7 | F65 已设：将映射 Rohde11 移至地图外。 | F65 是 Caravan 解锁，不是其成员标志。F84 跟随可提供被复用的随从查找映射，此时移动的是该随从，而非固定配置行动者。 |
| 8–11 | F12 已设：将映射 Rick12 移至地图外。 | F12 是其加入标志。Rick 不在普通随从表中，仅在 F518 替代配置声明。重建时 F518 清除/F12 已设会留下 lookup12 为零，因此调用作用于**物理 main0**，而非安全跳过 Rick。不声称此不一致状态自然发生。 |
| 12 | 返回。 | 无对话、传送、存档点选择、战斗标志或交通写入。 |

`MoveEntityOutOfMap` 解析查找映射，把位置**和目的地**写为 `$7000`。不删除/隐藏行动者、移除查找/列表条目、改变 HP/成员资格/标志、停止行动或清除行程/速度。辅助既不验证存在，也不验证 HP。此内部地图外放置不是观察到的永久消失。

交通实体群先于 init。Mode2 再次给 main0 木筏精灵，把列表随从放在 `(64,64)` 站立，并在该处声明物理31，不论 F64/已保存地图。Mode0 用 Bowie 精灵，且仅当 F64 与 RAFT_MAP=9 时声明独立木筏；保存在69 的元组不合格。两条路径均不改变已保存元组或规范化模式。Init 随后可移动映射行动者，包括传入 Rick 用例中的 main0。

init 后为条件音乐/淡入与 `WaitForEvent`。无待处理事件时，物理0 在 mode0 接收步行控制，在 mode2 接收 opcode8 控制器。其他传入模式保留共享选择器行为。init 已返回、主角在地图外或服务未完成，都不能确立可用步行。[实体群/控制所有者](../../../research/battle16-outcomes-through-hassan-entry.md#fresh-hassan-population-init-and-conditional-control)保留精确操作与行动者规则。本指南止于任何城镇输入或交互之前。

## 证据追踪与未解问题

**已确认溯源：**已接受 #824/PR826 研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与已注册 USA 零售 ROM。[复现所有者](../../../research/battle16-outcomes-through-hassan-entry.md#reproduction-and-evidence-limits)列出源码主体、选定记录与未改变的保留比较配方。本指南使用该已接受审阅，不重新调查源码、重建载荷或采集运行时。

| 边界 | 现有合同与精确夹具追踪 | 覆盖限制 |
| --- | --- | --- |
| 结果、首领与 after/join 包装器 | [战斗生命周期](../contracts/battle-control-lifecycle.md)、[过场路由](../contracts/battle-cutscene-routing.md)；`sf2-battle-loop-static-v1`、`sf2-battle-control-static-v1`、`sf2-battle-cutscenes-static-v1`、`sf2-battle-global-data-static-v1`、`sf2-battle-routing-data-static-v1` | 共享生命周期/数据事实；选定 row16、头部身份、Polca 存档点/重置与直接返回组合是研究所有者的关联，不是组合结果夹具。 |
| after 场景与下筏 | [地图探索](../contracts/map-exploration.md)；`sf2-map-script-engine-static-v1` | 跟踪命令事实选择 `abcs_battle16`/`cs_45470`；不包含完整 programCorpus。研究检查全部3/4 条保留记录和独立行动/子程序消费者。这不是自然登陆或全部行动者回执。 |
| 传送、实体群/init 与跟随 | [探索流程](../contracts/exploration-control-flow.md)、[入场路由](../contracts/map-entry-routing-state.md)、[名册](../contracts/party-roster-state.md)；`sf2-map-event-request-consumption-static-v1`、`sf2-map-setup-static-v1`、`sf2-map-entities-static-v1`、`sf2-map-init-static-v1`、`sf2-common-stats-static-v1` | 跟踪 init 含全部13 操作和两组路由/指针关联。Hassan 行/枚举、配置行动者列表与 Rick 缺失映射后果仍是已接受研究关联，不是逐用例自然输入覆盖。 |

这些 ID 对应 `tests/fixtures/h2/` 下去掉 `sf2-` 前缀的同名文件。所有者命令包括 `uv run sf2 h2 battle-loop`、`battle-cutscenes`、`map-script-engine`、`map-init`、`map-setup` 与 `common-stats`。[地图脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)及其 [schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)比较命令事实投影；[map-init 验证器](../../../../src/sf2tool/h2/map_init.py)与[夹具 schema](../../../../schemas/h2-map-init-static-fixture.schema.json)保留完整操作与关联。[过场验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)拥有共享包装器事实。本说明审阅检查有界跟踪命令位置、全部13 init 操作、两组关联和共享结果/请求事实，以及其 schema/验证器边界。没有把汇总夹具提升为组合自然旅程。任何受控复现均于同一进程加载所属工作树忽略的私有输入配置。

[PR826 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160352020)通过其[完整交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160316137)保留已完成比较、最终 CI，以及早前已修正的敌人宏断言/检查失败。[PR822](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072)保留其检查、检查/说明修正与被拒命令历史；[PR818 最初目的地失败](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159788942)和[修正最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)保留实际状态。本文件切片不重放或替换它们。

**未知：**自然结果/旅行/登陆以及调用方模式、标志、HP、成员资格、映射、列表、布局/占用/输入；全部行动者/服务完成、原版渲染/音频、可用 Hassan 输入与存档/恢复/持久性。这些是 [ADR 0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md)下延期限制，不是自动采集队列。
