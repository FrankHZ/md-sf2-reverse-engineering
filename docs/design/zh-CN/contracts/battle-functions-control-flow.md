# 战斗函数控制流合同

- **已确认原版行为：** fixture 受限的单回合路线、Kiwi 转换门、EGRESS 与 Angel Wing 请求顺序、战斗加载与移动命令选择顺序、光标与目标列表控制、战斗与战场菜单分支，以及所列局部动作/结果写入。
- **推断原版角色：** 源码名与注释暗示网格动画、AI 目标显示、玩家面向菜单用途与战术意图，但那些含义不是可执行证据。
- **未知或独立：** 自然 Map 3 或 Battle 01 可达性、输入获取与时序、目标列表构建、被调方成功与持久效果、完整取消嵌套、战斗解决、渲染或可听输出、畸形状态行为与平台 ABI。

## 目的

本合同为共享战斗函数定义实现无关控制边界，其已接受静态证据由 `sf2-battle-functions-static-v1` 拥有。它闭合[Map 3 至 Battle 01 就绪](../../synthesis/map3-battle01-readiness.md)识别的缺口：玩家与单回合控制事实有已接受证据，但此前没有证据绑定设计合同。

合同保留抽象路线决定、fixture 显式局部结果与有序源请求轨迹。它不把源调用变成被调方成功、玩家看到或听到效果、或分支在 Battle 01 中自然到达的证明。它不是场景合同，也不启动 Phase 4。

## 判断边界

**已确认静态：** 所选测试夹具与固定源建立十五个具名入口或表身份、下文字段闭合的控制事实，以及已接受 ROM 溯源下 H1 解析的符号地址。源顺序只在 fixture 显式表示的范围内确认分支决定、局部写入、命令操作数与调用或转移请求。

**推断：** `CreatePulsatingBlocksForGrid`、`PerformAiTargetingVisualAct`、菜单标签、动作名与其他源码词汇暗示开发者或玩家面向角色。它们不证明动画、可见性、战术意图、可访问性或普通运行时使用。

**未知或排除：** 精确输入节奏、重复、同时性与平台映射；完整调用方状态；目标列表合法性与构建；AI 打分与选择；外部服务的成功、原子性或回滚；持久物品、金币、Deals、存档或故事变更；随机分布；寄存器、CCR 与栈效果；完整 H1/ROM 指令 parity；VInt、VDP、DMA、淡入、音乐、文本、窗口、光标或声音行为；畸形列表或状态；以及自然 Map 3 至 Battle 01 可达性。

## 证据合同

本合同消费的唯一可执行所有者是 `sf2-battle-functions-static-v1`，位于 [`battle-functions-static-v1.json`](../../../../tests/fixtures/h2/battle-functions-static-v1.json)，由 [`battle_functions.py`](../../../../src/sf2tool/h2/battle_functions.py) 验证并由[Shared Battle Functions](../../../research/battle-functions.md)解释。提取 manifest 是 [`battle-functions-static.json`](../../../../manifests/extractions/battle-functions-static.json)。

此处不消费 battle-control、battlefield、battle-action、输入、音频、存档、摄像机、聚合战斗或 H3 测试夹具作为可执行所有者。

### 字段闭合的 fixture 使用

| Fixture 字段 | 本合同中的处理 |
| --- | --- |
| `romSha256`、`upstreamCommit` | 仅溯源身份 |
| `function` | 十五个受限符号/地址身份 |
| `expected.representativeSymbols` | 仅七文件溯源见证 |
| `expected.functionFacts` | fixture 显式路线、局部结果、操作数与请求顺序事实 |
| `expected.playerControlFacts` | fixture 显式光标、目标、菜单、物品与宝箱分支事实 |
| `expected.playerControlInputBits` | 仅抽象消费者观察名 |
| `expected.playerControlBattleActions` | 仅源常量身份 |
| `expected.playerControlMenus` | 仅源菜单常量身份 |
| `expected.playerControlSelectedCallEdges` | 仅受限静态调用边清单 |
| `expected.playerControlSummary` | 仅分母与溯源元数据；不是行为保真或 H4 |
| `expected.indexedRecordIds`、`indexedSourcePaths`、`indexedRecordsBySourcePath` | 仅关联/源成员审计；不是行为保真或 H4 |

忽略的生成报告包含更深的逐函数目录，包括完整源范围与 hash、分支目标、直接调用图、全局状态访问行、文本 ID 与其他源码清单。那些目录是私有验证材料，不导入本公开合同。

### 无全文 parity 的溯源

fixture 在固定上游 commit 与已接受 ROM SHA 溯源下通过 H1 解析每个所选符号身份。它不解析并逐字节比较每个函数的完整指令体与 H1 与 ROM。完整源体、宏展开指令字节与任何未来 body-parity 比较保持私有可选验证输入，不是 **已确认** 事实或 H4 要求。

## 精确关联分母

fixture 的源根成员连接包含跨七个源路径的十六条记录。直接 `sf2-battle-functions-static-v1` 证据集包含恰好以下十五条当前未关联记录：

| Research-index 记录 | fixture 身份 | ROM 入口/地址 |
| --- | --- | ---: |
| `battle.functions.pulsating-grid` | `CreatePulsatingBlocksForGrid` | `0x22C84` |
| `battle.functions.control-cursor` | `ControlCursorEntity` | `0x22D90` |
| `battle.functions.choose-target` | `ControlCursorEntity_ChooseTarget` | `0x230E2` |
| `battle.functions.set-cursor-target` | `SetCursorDestinationToNextBattleEntity` | `0x232BC` |
| `battle.functions.angel-wing` | `ExecuteBattleaction_AngelWing` | `0x23D98` |
| `battle.functions.execute-turn` | `ExecuteIndividualTurn` | `0x23EB0` |
| `battle.functions.update-targets` | `UpdateTargetsListForCombatant` | `0x24642` |
| `battle.functions.player-input` | `ProcessBattleEntityControlPlayerInput` | `0x24662` |
| `battle.functions.equip-in-battle` | `EquipNewItemInBattle` | `0x24C94` |
| `battle.functions.check-gold-chest` | `CheckGoldChest` | `0x250FC` |
| `battle.functions.battlefield-menu` | `BattlefieldMenu` | `0x2519E` |
| `battle.functions.ai-target-visual` | `PerformAiTargetingVisualAct` | `0x2548E` |
| `battle.functions.load-battle` | `LoadBattle` | `0x25610` |
| `battle.functions.relative-move-table` | `table_RelativeTileMoveX` | `0x256A2` |
| `battle.functions.move-sfx` | `SetMoveSfx` | `0x25790` |

第十六条成员行是 `map.camera-control.destination-service`。它共享 `battlefunctions_0.asm`，但不直接绑定本 fixture。它仍只与[地图探索](../../contracts/map-exploration.md)关联，其摄像机行为受[Map Camera Update Control Flow](../../contracts/map-camera-update-control-flow.md)约束。它不得获得本合同。

未来语义关联集恰好是表中的十五条 `battle.functions.*` 行。没有其他记录获得或失去关联。

## 单回合控制

### 行动者与控制器路线

源码静态 `ExecuteIndividualTurn` 路线保留以下决定：

1. 死亡行动者跳过回合；
2. 非零 MUDDLE 状态、AI 控制位、己方自动战斗或普通不受控敌人选择 AI 控制路线；
3. 对手控制开关是把敌人路由到玩家控制的受限例外；
4. SLEEP、STUN 与已提交 STAY 结果消耗动作而不构建战斗演出；
5. EGRESS 与 Angel Wing 在战斗演出构建之前退出；以及
6. 普通已提交动作按该源顺序请求 `WriteBattlesceneScript`、`ExecuteBattlesceneScript`、`EndBattlescene` 与 `LoadBattle`。

这是路由与请求顺序合同。它不拥有状态字段如何产生、AI 行为、玩家输入时序、演出构建、演出执行、战场重载效果或任何路线的自然调用方状态。

### Kiwi 转换门

对源类身份 `28`，只有物理攻击路线请求 `RNG(4)`。结果零选择 Kiwi 法术转换；非零保持物理路线不变。所选法术等级由有序等级阈值 `32`、`40` 与 `50` 决定，产生等级 `0`、`1`、`2` 与 `3`。

操作数 `4`、成功结果 `0`、阈值与等级身份是 **已确认静态**。RNG 状态、分布、调用时序、自然类/动作准入、伤害、动画与可见含义仍归[随机性](../../contracts/randomness.md)、解决合同、呈现或 **未知**。

### EGRESS 与 Angel Wing

fixture 标签被解读为源码形状请求，而非已完成服务效果：

- Angel Wing 在进入共享退出路径前请求移除物品；
- EGRESS 法术路径请求法术消耗查找与 MP 减少；
- 两条路径都请求战场窗口关闭、解锁标志更新与退出位置查找；
- 两条路径都局部产生返回码 `D4 = 0`。

此处只拥有局部返回结果与精确请求顺序。物品移除、MP 变更、窗口关闭、标志持久性、返回地图状态与调用方可视退出行为需要其自身可执行所有者或保持 **未知**。

## 战斗加载与移动命令选择

`LoadBattle` 按顺序发出这十个源请求：

1. 淡出；
2. 加载地图瓦片集；
3. 定位战斗实体；
4. 初始化精灵；
5. 加载地图；
6. 加载实体地图精灵；
7. 安装战斗 VInt 处理器；
8. 加载战斗地形；
9. 请求地图音乐；以及
10. 淡入。

Fairy Woods 分支额外到达源码具名的计时器开启请求。该列表只保留调用身份与顺序。它不确认图形、地图、实体、地形、VInt、音乐、淡入或计时器服务已完成、什么中间状态可见或任何步骤耗时。

`SetMoveSfx` 在战斗外选择源命令身份零，在战斗中选行走身份。Chirrup Sandals 条件用源身份 `BLOAB` 覆盖任一状态。选择与覆盖顺序是静态事实；声音传输、命令接受、波形、可听性与时序仍归[音频系统](../../contracts/audio-system.md)或 **未知**。

## 玩家控制面

### 清单计数器不是行为

fixture 记录跨九个源范围的六个所选条目，有 `1,039` 条语句、`231` 个分支站点、指向 `84` 个唯一目标的 `207` 个直接调用站点、`5` 条所选内部调用边、`59` 个全局状态身份、全部 `8` 个具名输入位、`4` 个战斗动作常量与 `4` 个菜单常量。这些是闭合清单与溯源计数器。重制不得复现这些计数、源范围、分支拓扑或调用站点总计。

具名输入位是原版分支消费的抽象观察。本合同不拥有控制器获取、边界发布、重复、同时性、节奏、电气行为或现代平台映射；那些仍归[输入系统](../../contracts/input-system.md)与后续产品决定。

### 光标与提供的目标列表

受限光标路线把 A、B 或 C 识别为格子确认身份、存储所选格子坐标并到达源光标隐藏请求。这些是局部输出与请求事实，不是渲染光标行为的保证。

目标选择消费提供的列表：

- 空列表返回 `-1`；
- B 是取消身份；
- A 或 C 确认并返回战斗员索引；以及
- UP、LEFT、DOWN 与 RIGHT 在候选中循环。

[战场导航](../../contracts/battlefield-navigation.md)保留提供列表的构建、合法性、排序、阵营选择与空间含义。本合同不使无效或畸形列表安全，也不定义目标高亮或移动动画。

### 战斗动作与战场菜单

源码静态战斗动作选择顺序是攻击、魔法、物品与搜索或待机。取消移动/动作路线恢复源局部位置状态并写入动作结果 `-1`。物品菜单顺序是使用、给予、装备与丢弃。fixture 列出的已提交结果身份包括攻击、施放法术、使用物品、待机与陷阱宝箱。

战场菜单选择顺序是成员、小地图、选项与挂起。战斗零拒绝挂起路线。已接受挂起接缝保留复制秒数、设置标志 `88`、请求存档并向 `WitchSuspend` 转移的源请求/局部写入顺序；调试 Start 路线在其存档请求后返回菜单。

此处拥有菜单选择与局部结果身份。菜单布局、文本、输入时序、存档成功、持久字节、Witch 执行、挂起恢复与可见过渡是独立所有者或 **未知**。

### 装备、物品与宝箱

fixture 闭合以下分支与请求身份：

- 诅咒已装备状态阻止源交换路线；
- 新诅咒分支到达其具名对话请求；
- 已装备诅咒物品阻止给予与丢弃；
- 目标物品栏满选择交换路线；
- 转移在添加物品请求前清除局部已装备位；
- 完成给予路线局部写入 STAY；
- 丢弃到达确认请求，珍稀物品分支请求加入 Deals；
- 宝箱搜索区分无内容、空、陷阱、金币、物品与满物品栏分支；
- 陷阱分支写入陷阱宝箱动作并请求敌人生成；
- 金币分支到达阈值/金额与增加金币请求；
- 物品分支到达添加物品；以及
- 非陷阱已解决路线写入 STAY，而满物品栏请求宝箱关闭并返回菜单路线。

这些不是对话显示、物品栏或经济交易完成、敌人生成成功、金币持久性、物品交付、Deals 持久性或原子回滚的证明。物品定义、状态变更、经济、服务与呈现保持独立。

## 仅身份记录

五个关联刻意比行为段更窄：

- `battle.functions.pulsating-grid`；
- `battle.functions.update-targets`；
- `battle.functions.relative-move-table`；
- `battle.functions.set-cursor-target`；以及
- `battle.functions.ai-target-visual`。

它们只保留其 fixture 支持的身份、地址、源清单成员与任何显式受限调用边元数据。relative-move 表不成为数据保真所有者；其条目、坐标含义、消费者与运行时使用此处不重构。同样，“pulsating”与“visual”等名称不证明渲染行为。

## 跨系统分离

- [Battle Control 与 Combatant Lifecycle](../../contracts/battle-control-lifecycle.md) 保留战斗入口、回合、死亡、动作后、结果与调用方准入语义。
- [战场导航](../../contracts/battlefield-navigation.md) 保留移动、范围、目标列表形成与合法空间状态。
- [Battle Action Construction](../../contracts/battle-action-construction.md) 保留已提交动作到演出脚本的构建。本合同在选择与有序交接处结束。
- [Battle AI Decision](../../contracts/battle-ai-decision.md) 保留打分与动作选择。本合同只保留静态 AI 对玩家路线与 AI 视觉入口身份。
- [交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)与[随机性](../../contracts/randomness.md)保留算术、效果、回放与 RNG 行为。
- [输入系统](../../contracts/input-system.md) 保留控制器状态的获取与发布。
- [存档系统](../../contracts/save-system.md)、服务、物品与经济所有者保留菜单、装备、物品、宝箱与挂起请求背后的持久变更与完成语义。
- [Battle Scene Presentation](../../contracts/battle-scene-presentation.md)与[音频系统](../../contracts/audio-system.md)保留可见与可听执行、加载器、淡入、时序与面向硬件效果。
- [地图探索](../../contracts/map-exploration.md) 保留摄像机跨所有者记录与目标服务。
- [战术战斗循环](../../synthesis/tactical-battle-loop.md)、[游戏总览](../../synthesis/gameplay-overview.md)与[Map 3 至 Battle 01 就绪](../../synthesis/map3-battle01-readiness.md)保持综合消费者。它们不是可执行证据所有者。

## 实现无关模型

```text
BattleControlInput {
  actorState
  committedOrPendingAction
  suppliedTargetList
  abstractInputObservations
  abstractMenuResults
  boundedServiceResults
}

BattleControlOutput {
  routeIdentity
  localActionOrTargetResult
  orderedLocalWrites[]
  orderedRequests[]
}
```

原版 source/H1 身份与地址是溯源与私有往返锚点。验证后，合规重制可以使用引擎原生引用、状态机、集合、事件与服务。它不需要复现 Mega Drive 地址、寄存器、栈布局、分支计数、源范围或原版内存结构。

兼容性由 fixture 准入的抽象输入与可观察路线/局部结果/请求轨迹判定。它不由指令数、精确调用拓扑、帧数或平台特定实现判定。

## 公开与私有边界

公开合同可以保留：

- fixture ID、ROM/上游溯源 hash 与规范摘要；
- 十五个所选符号与 H1 解析地址；
- 受限七文件代表身份；
- 显式标注为非行为元数据的清单计数器；
- fixture 列出的路线决定、常量、局部结果与有序请求摘要；以及
- 精确 `15 + 1` 关联/成员边界。

它不得发布完整源体、完整分支/调用/全局/文本目录、指令字节、私有 RAM 或 ROM 转储、截图、音频、对话载荷或派生版权资源。私有材料可以支持本地验证，但不得成为受追踪运行时依赖。

## 保真与现代化

原版保真适配器保留：

- fixture 准入的路线区分与优先级；
- 精确 fixture 显式局部动作/目标结果；
- 顺序是已接受事实一部分的有序源请求轨迹；
- 选择/控制、服务执行与呈现之间的区分；以及
- 本文档中每个独立所有者与 **未知** 边界。

现代化可以在抽象兼容轨迹保持等价时，用引擎原生状态转换、集合、future 或事件替换源码形状循环与调用。它不得静默声称外部请求成功、规范化缺失或畸形域、赋予玩家意图，或把源码词汇提升为可见行为。

## H4 验收面

在重制适配器可以声称本合同之前，项目自有合成检查必须：

1. 覆盖死亡、AI 准入、对手控制、睡眠、眩晕、待机、特殊退出与普通动作单回合路线；
2. 覆盖 Kiwi 类/动作/RNG 门与全部四个等级带，而不把 RNG 请求当作分布保证；
3. 验证 Angel Wing 与 EGRESS 局部结果加有序请求轨迹，而不断言被调方完成；
4. 验证十个 LoadBattle 请求身份与 Fairy Woods 条件请求，而不断言淡入、VInt、图形、地图、音乐或计时器完成；
5. 验证移动命令选择与覆盖身份，而不断言可听输出；
6. 覆盖提供目标列表上的空、取消、循环与确认用例；
7. 覆盖战斗动作与战场菜单选择/取消结果身份与受限挂起请求顺序；
8. 用受控抽象服务结果输入选择每个 fixture 列出的装备、物品与宝箱分支，同时只断言 fixture 显式局部写入/结果与有序请求，而非服务结果；
9. 保留仅身份记录，而不发明表内容、视觉效果或更深行为；
10. 从通过条件中排除源计数器、原始地址、寄存器/CCR/栈状态、完整 body parity、自然 Map 3 或 Battle 01 可达性，以及所有可见/可听/时序主张。

测试断言抽象路线决定、局部输出与有序请求轨迹。它们不要求源码形状微控制流。

## 证据矩阵

| 主张 | 证据 | 判断 |
| --- | --- | --- |
| 十五个入口/表身份与地址 | `function` 加 H1 解析 | Confirmed static identity |
| 单回合、Kiwi、退出、加载与移动命令事实 | `expected.functionFacts` 与固定源检查 | Confirmed static route/local-result/request order |
| 光标、目标、菜单、物品、装备与宝箱事实 | `expected.playerControlFacts` 与固定源检查 | Confirmed static route/local-result/request order |
| 摘要计数器与源成员 | fixture 审计字段 | Confirmed inventory/provenance only; not H4 behavior |
| 五条窄记录 | 仅地址/清单/调用边面 | Confirmed static identity only |
| pulsating/visual/menu/tactical 用途 | 源码名与注释 | Inferred |
| 输入时序、被调方完成、持久性、AI/目标合法性、呈现、自然场景可达性 | 未被所选所有者建立 | Unknown or separate owner |

## 复现

```powershell
uv run sf2 h2 battle-functions
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成输出保留在忽略的 `local/derived/battle-functions-static.json` 下。
