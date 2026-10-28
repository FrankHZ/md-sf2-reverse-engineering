# New Granseal 出发至 Battle09 准入

> 本文是[规范英文原文](../../synthesis/new-granseal-departure-through-battle09.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Peter 城堡觐见之后的选定接续始于 New Granseal 南出口。其大地图连接向东进入 Bedoe 区域，再向南进入 South Parmecia。**推断：**这是通往名为 `BATTLE_TO_RIBBLE` 遭遇的候选接续。**已确认静态内容：**跨越这些滚动边界时，探索继续运行；Battle09 准入则依赖普通入场或存续的重载请求，以及战斗解锁状态。两个开场脚本为空，但共享包装器仍改变介绍标志。**未知：**自然连续步行，以及此边界处完成的战斗准备。

本文承接[Peter 城堡觐见与返回城镇](peter-castle-audience-through-town-return.md)。已接受的[研究所有者](../../../research/new-granseal-departure-through-battle09.md)提供选定连接、使用方、zone 谓词与开场顺序。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有 [#755](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/755) 的 Standard 严谨度协议。本指南支持内容/设计规划，止于 Battle09 start 分发器返回、紧邻 `ActivateEnemies` 之前；交战、结果、Ribble 内容和重制就绪不在本分段内。Fnnn 表示标志 nnn，战斗 ID 使用源码从零开始的表行。

## 1. 离开城镇携带历史、出战队伍状态与随从

**已确认条件连接：**Map6 南出口匹配 Y31、X 通配，请求 Map73 `(29,53)`、Down，不滚动。移动调用方仍要求传送布局属性 `$1000`；编码行不证明出口可通行。独立 `(31,14)` 行有相同目标，注释为 `unusable warp`；它不支持另一条可用路径主张。若请求通过轮询和分发前清除而被消费，无滚动使用方将元组返回 `MainLoop`。Map73 没有标志驱动替换或战斗选择行，因此进入普通探索入场，仍受服务及待处理状态约束。

完成的航行/建设历史携带 F700/F508/F705 已设。Peter 觐见携带 F7 成员身份、他的 AI 控制位、F702 和持久地图随从标志 F67。F39 出战队伍成员身份仍取决于容量和先前状态。已加入、战斗中出战、AI 控制以及作为随从可见是四个独立事实。其他提供的故事标志、随从和请求仍是输入；前一城镇控制读取不建立出口可达性。

普通 Map73 入场清除待处理，设置当前地图/非战斗状态，重建实体和精灵，清除临时标志 256–383，设置 F80，并在 init 前加载地图。它还治疗符合条件的存活/不死己方角色并调整步数计数器；这不是全军团重置。Map73 使用默认配置与空固定实体列表，但仍可分配按标志选定的随从。其 init 从提供历史选择：

| 普通 Map73 init 时的状态 | 选定内容 |
| --- | --- |
| F700 未设 | 运行较早航行场景，然后设置 F700，不测试建设即返回。 |
| F700 已设，F508 未设 | 不运行场景，直接返回。 |
| F700/F508 已设，F705 未设 | 运行建设，然后设置 F705 并返回。 |
| F700/F508/F705 已设 | 不运行场景，直接返回，如本文使用的完成历史。 |

[航行](voyage-through-damaged-ship-control.md)与[建设](battle08-outcomes-through-new-granseal-handoff.md)指南拥有较早场景。选择器不测试 F701/F702 或抵达坐标。Map73 默认 zone 处理器直接返回，不解锁 Battle09。init 后安装控制仍要求待处理为零、携带玩家类型 0；init 返回不保证这些条件或可用输入。

## 2. 东、南边界在探索内滚动

**已确认静态连接：**选定 Map73 东边界通向 Map72，Map72 南边界通向 Map69。Map73 还有通往 Southwest Parmecia 的南出口，Map72 也有其他出口。本指南选择一条候选分支，不使其成为唯一或自然可穿越路线。

| 选定连接 | 触发 | 目标与使用方行为 |
| --- | --- | --- |
| Map73 向东 → Map72 | X63，Y 通配 | 向 Right 滚动；设置 X 为 1，保留 Y。 |
| Map72 南边缘 → Map69 | X 通配，Y63 | 向 Down 滚动；保留 X，设置 Y 为 1。 |
| 独立 Map72 南连接 → Map69 | `(40,62)` | 普通无滚动入场，位置 `(41,16)`、Down。 |

滚动使用方改变当前地图，加载地图瓦片/图块/布局并执行地图过渡。负目标坐标字节保留该轴。它读取主实体实际朝向，而不应用行中编码的朝向字节；后者保留为请求参数状态。然后返回既有探索循环。这些是使用方规则，不主张移动、滚动、行动者或木筏服务已完成。

滚动不经 `MainLoop` 或 `CheckBattle` 返回，不运行普通地图 init，不重建实体群、不治疗出战队伍或清除临时标志。因此滚动 Map73 → Map72 → Map69 保留先前实体群，尽管两个目标都有空固定实体列表。Map72 普通 init 的 Luke 分支仅在 F716 已设、F10 未设时选定，本次边界滚动不调用它。Map69 普通 init 直接返回。这些事实不消除持续探索中的 zone 中断。

独立固定南连接有实质区别。其存续的无滚动请求将 Map69 `(41,16)`、Down 返回 `MainLoop`，在那里战斗选择先于普通探索入场和 init。因此携带的 F409 可不经 Map69 zone 访问而立即准入 Battle09。若没有战斗行符合条件，就执行普通 Map69 入场。没有 Map69/72 地图替换改变这些选定目标。反向固定/滚动行是连接的对应项，不是任一方向可用步行路线的证明。

## 3. 大地图 zone 可中断候选步行

**已确认静态内容：**移动需要 zone 布局属性 `$1400` 才产生 zone 事件。zone 表默认项不意味着每个普通瓦片都分发它。在 Map72，选择先检查 X32/33/34、Y 通配的随机 Battle26 辅助，再检查 X25–30,Y37 的后期大炮处理器。F750 已设或 F805 未设时，大炮处理器立即返回；其较后函数体不在本分段内。默认项在 F523 未设时调用 Battle08 辅助，F523 已设时调用 Battle24 辅助。

携带的 F508 已设使 Battle08 辅助进入已完成/随机分支，因此不保证选定 Map72 步行期间没有事件。[随机战斗所有者](../../../research/map-event-random-battle-state.md)拥有步数计数器、RNG 与请求产生。若请求到达普通战斗选择，Map72 行按 8,24,26 排序，触发点均通配：首个已解锁行胜出，而非最近战斗区域。这些可能中断阻止连续无中断旅程主张；其遭遇和较后场景函数体不在此审计。

在 Map69，特定与默认 zone 后果不同：

| 选定 Map69 zone | 处理器效果 |
| --- | --- |
| X23,24,25，Y 通配 | 调用 Battle17 随机辅助；其 F517/步数/RNG 规则可设置 F417 并请求重载。不设置 F409。 |
| 默认，F509 未设 | 设置 F409，并写入部分同图重载请求。 |
| 默认，F509 已设 | 不写入，直接返回；此处不清除已有 F409。 |

特定行先于默认。特定处理器即使不产生请求，也不顺序进入默认。默认仅测试 F509：Peter 成员身份/AI、F702、F700/F705、朝向、步数和 RNG 都不是额外 Battle09 解锁门槛。前面故事提供上下文，不向该处理器增加谓词。南向滚动保留 X，因此不建立随后分发 zone 会选定特定行还是默认。

## 4. 重载请求保留产生方的部分状态

**已确认静态请求边界：**普通类型 6 产生方将 zone X 与 Y 作为字写入参数 1/2 和 3/4。其使用方应用主实体 init 动作脚本并分发选定 zone。该处理器之后，分发器关闭立绘/文本，等待一次 VInt，再等待物理实体 0 停止。它既不等待每个随从，也不清除新写入请求。只有后续探索轮询可消费重载；服务完成与请求存续仍是条件。

默认项的 `MAP_EVENT_RELOADMAP=$100FF` 长字写入产生类型 1、参数 1=`00`（无滚动）及参数 2=`FF` (`MAP_CURRENT`)。参数 3/4/5 不变。这是部分普通传送请求，不是特殊重载事件或新提供的位置/朝向元组。

无滚动使用方采样实体 0 地图位置，以非负参数 3/4 字节覆盖各坐标，并始终将参数 5 用作朝向。在普通类型 6 历史完整且 zone Y 为 0–63 时，这两个坐标字节为 `00,Y`：因此返回元组为 `MAP_CURRENT,(0,Y),retained parameter 5`，而不是保证恢复主实体采样位置。其他历史保留一般带符号覆盖规则。选定前序南向传送将 Down 写入参数 5，但类型 6 与此默认项都不写它，中间写入方可改变它。可变待处理槽位不是队列。覆盖、中断、当前地图改变或服务未完成可破坏整个拟议组合。

## 5. 有序战斗选择决定准入

**已确认静态内容：**`MainLoop` 先应用 `SwitchMap`，再应用 `CheckBattle`。`MAP_CURRENT` 从当前地图状态解析。Map69 行中，Battle09 (`BATTLE_TO_RIBBLE`) 先于 Battle17 (`BATTLE_TO_TAROS_SHRINE`)，触发 X/Y 均为通配。各自解锁/完成对为 F409/F509 与 F417/F517。辅助在完成检查前写入战斗区域输出；区域边界不限制准入。

| 选择时提供的 Map69 状态 | 本次调用结果 |
| --- | --- |
| F409 已设，F509 未设 | 选择 9，即使 F417 已设或坐标位于 Battle09 区域外。 |
| F409 与 F509 已设 | 清除 F409，但同次调用仍返回 9；不继续到 17。 |
| F409 未设，F417 已设 | 选择 17，使用其自身完成处理。 |
| F409 与 F417 未设 | 返回无战斗哨兵值 `-1`。 |

存续的 F509 未设默认请求在选择前设置 F409，因此优先于携带的 F417。反之，即使特定 zone 的 Battle17 辅助请求，只要 F409 已设，也可选择 9：辅助的遭遇 ID 不是直接 `BattleLoop` 参数。完成状态抑制默认请求产生，与已完成但仍解锁时的同次调用规则分开。这些条件支持候选 Battle09 交接，不支持必然按故事顺序发生的遭遇。

## 6. 空开场内容仍有包装器效果

**已确认静态新战斗顺序：**选定索引 9 进入 `BattleLoop`，清除 `PLAYER_TYPE`，设置 F399 并测试 F88。F88 未设时，清除已用秒数，建立当前地图/战斗和基础 VInt，然后调用 before-battle 分发器。它恢复/重新确立地图/战斗，清除区域标志 90–105，治疗符合条件的己方角色，先初始化己方位置再初始化敌方位置，清除 AI 内存，为战斗员 0 调用 `LoadBattle`，并调用 battle-start 分发器。本指南止于该分发器返回、`ActivateEnemies` 之前。

源码行 9 的两个开场目标均为 `ms_Empty`，即地图脚本终止符。它们没有故事命令对话、招募、传送或故事标志序列。不应虚构缺失的 Battle09 before 脚本依赖。不过包装器仍计算介绍标志 450+9，即 F459：

| 介绍状态 | before/start 行为 |
| --- | --- |
| 各包装器处 F459 未设 | before 分发但不设置它；start 在分发空目标前设置 F459。 |
| F459 已设 | 跳过对应脚本分发；新战斗初始化仍运行。 |

F509 完成与 F459 介绍状态是独立门槛。存在对话窗口时，空解释器返回仍可等待视野滚动，清除滚动速度并恢复寄存器。因此空故事内容不证明服务效果为零或瞬时完成。`LoadBattle` 还在 start 返回前调用地图/精灵/地形、音乐和淡入淡出服务；此组合中它们的可见完成未被观察。

战斗准备使用出战队伍列表，放置存活出战己方角色，并可移除超出容量的成员；不从可见随从派生该列表。普通 Map73 随从初始化为 Peter 测试 F67，提供的 F65 在 Maps69/72/73 选择大地图随从前缀。其他随从标志仍为输入，滚动不重建实体群。空 Battle09 脚本不让 Peter 加入，也不安装随从。48 条目上次目标/内存重置与 Peter 战斗员 AI 控制位分开。此处不主张具体名册、Peter 行动或就绪回合（单次行动）。

F88 已设时，独立恢复分支恢复已存秒数，清除 F88 和 AI 内存，加载战斗状态并跳到单次行动循环。它绕过新战斗 before/start 序列及其 F459 写入。仅提供 F88 不建立有效存档状态组合；恢复是区分项，不是本指南中自然证明的替代旅程。

## 证据审阅与剩余限制

已接受研究提供场景特定连接及其组合静态时序，锚定 USA 零售版与固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`。已跟踪测试夹具选择及更严格合同与本说明一致。尤其是辅助局部入场合同不证明自然入场，介绍路由不保证立即空返回。无需研究/合同修正。以下已审阅边界不是已执行的连续出发至 Battle09 比较：

| 关系 | 已审阅可执行所有者 | 此处保留限制 |
| --- | --- | --- |
| 普通入场与战斗选择 | [sf2-common-maps-static-v1](../../../../tests/fixtures/h2/common-maps-static-v1.json)，`expected.mapFacts.battleTrigger`；[入场路由合同](../contracts/map-entry-routing-state.md)与[探索合同](../contracts/exploration-control-flow.md)。 | 通配符、区域先于完成、辅助局部变更；选定连接元组和滚动组合来自已接受研究，不是自然步行测试夹具。 |
| 目标 init | [sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json)，`expected.primarySourceBodies[76]`、`[78]`、`[79]`。 | Map69 返回、Map72 F716/F10 与 Map73 F700/F508/F705 选择器；不保证滚动运行它们或服务完成。 |
| zone 处理器选择/效果 | [sf2-map-events-static-v1](../../../../tests/fixtures/h2/map-events-static-v1.json) 与 [zone 程序分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)，Map69/72/73 的 `expected.zoneTargetPrograms`。 | 具名谓词和请求写入；不是自然首次 zone 或可通行布局。 |
| 待处理请求与中断 | [sf2-map-event-request-consumption-static-v1](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`eventRequestConsumption.consumerContexts.waitForEvent` / `processMapEvent`；[请求所有者](../../../research/map-event-request-consumption.md)与[随机战斗所有者](../../../research/map-event-random-battle-state.md)。 | 轮询/清除/访问与共享辅助边界；行动者状态、剩余参数字节和请求存续需要上文保留的调用方历史。 |
| 开场包装器与准备 | [sf2-battle-cutscenes-static-v1](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`expected.cutsceneFacts.intro`；[sf2-battle-control-static-v1](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`expected.controlFacts.mainLoop`；[sf2-battle-loop-static-v1](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`expected.lifecycleFacts`；[介绍合同](../contracts/battle-cutscene-routing.md)。 | 共享标志与恢复/初始化边界；行 9 空目标与激活前组合是已接受研究，没有就绪回合（单次行动）或服务完成证明。 |
| Peter 与脚本返回 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，force-state/control 命令事实；[出战队伍/名册合同](../contracts/party-roster-state.md)与前篇指南。 | 已加入/出战/AI/随从区别及终止符语义；不主张已加载行动者群、Peter AI 行动或自然控制。 |

[map-init 验证器](../../../../src/sf2tool/h2/map_init.py)与[测试夹具 schema](../../../../schemas/h2-map-init-static-fixture.schema.json)、[请求消费验证器](../../../../src/sf2tool/h2/map_event_request_consumption.py)与[schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)，以及[过场验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)与[schema](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)比较受限结构、溯源与测试夹具事实，不证明本自然路线。其 CLI 所有者是 `uv run sf2 h2` 后接 `map-init`、`map-events`、`map-event-request-consumption`、`map-event-random-battle-state`、`common-maps`、`battle-control`、`battle-loop`、`battle-cutscenes` 或 `map-script-engine`。**本次设计切片未重跑这些提取/运行时验证轨道。**[研究复现边界](../../../research/new-granseal-departure-through-battle09.md#coverage-controls-and-reproduction)拥有源码锚点与保留比较；可选派生载荷既不要求，也未生成。

**未知：**自然出口/边界/zone 可达性、碰撞及首次 zone 坐标；可达输入标志、随从与出战队伍；穿过全部服务后的完整行动者/请求状态；完成的滚动、加载、音乐/淡入淡出与解释器效果；实际战斗/输入就绪；以及存档持久性。这些缺口不是自动获取队列。Battle09 结果与 Ribble 需要另行已接受的后继证据。本指南既不完成战役时序，也不完成 epic #514，不选择引擎/产品变更；英文原文将中文同步留待路线图另行安排。

接续[Battle09 结果至 Ribble 欢迎（中文）](battle09-outcomes-through-ribble-welcome.md)，了解可变战斗返回、独立城镇入场、指控与 Peter 调解，止于首次条件性恢复控制边界。其标志、行动者、重载和屋顶恢复限制仍与本准入分段分离。
