# 文档路线图与治理边界

- 状态：**已确认的仓库治理指南**；本文档不是关于原版游戏的证据，也不选择重制引擎、产品、平台或商业方向。
- 记录日期：2026-08-01
- 范围：将带来源的合同组织成简洁的面向玩家的说明，而不改变其证据标签；未来的重制选择仍需显式决策与 H4 验收边界。

> 本文件是 [`documentation-roadmap.md`](../documentation-roadmap.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 编写语言政策

**已确认的仓库政策：** 在当前设计综合阶段，英文是新增或实质性修订的设计综合文档的规范编写与审阅语言。保留源码忠实的标识符、fixture ID、证据标签与代码词汇，而非发明翻译等价物。

**已确认的仓库决定（2026-08-04）：** 项目术语表已在 [`glossary.md`](../glossary.md) 接受。它是设计综合文档中英文→中文术语的单一绑定来源，并约束 `docs/design/zh-CN/` 下的镜像约定。不得创造临时的双语术语；应使用术语表的固定术语与规则（证据标签译法、保留源码标识符、一词一译、专有名词保留英文并附建议注记，以及条目修订流程）。

非英文本地化从英文规范源出发，按术语表规则以专用批次进行，并满足术语一致性、链接完整性、证据标签保留与 fixture 追踪 QA。`docs/design/zh-CN/` 下的中文镜像是派生文档；除非某个本地化批次另行定义显式政策，英文源始终是审阅基线。

## 三层边界

| 层 | 归属与允许内容 | 禁止的捷径 |
| --- | --- | --- |
| A. 原版行为/数据合同 | `docs/research/`、`schemas/`、manifests、H2/H3 测试夹具以及带来源的设计合同记录 **已确认**、**推断** 与 **未知** 事实。 | 源码宏、字段或符号名并不能自动确立面向玩家的含义。保留源码标签，只解释有证据支持的解读。 |
| B. 重构的设计解释 | `docs/design/` 可以解释受 A 层支持、面向玩家的后果，链接本地研究/fixture 所有者，并把缺口标为 **未知**。 | 不得把静态调用、地址或看似合理的解读提升为原版行为、战役结论或玩家体验事实。 |
| C. 未来的重制决定 | 现代化、实现意图与产品选择属于显式决策以及独立的 expected-deviation/H4 验收边界。 | 不得把现代化改写为原版行为，也不得用综合文档选择引擎或产品方向。 |

**已确认的仓库规则：** B 层是已接受 A 层证据的可追溯解读，不是第二套证据系统。**推断** 的解读保留该标签，**未知** 的行为仍是问题而非作为叙事被补全。

## 综合前证据审查

**已确认的仓库规则：** 每个 B 层综合切片必须对它将解释的 A 层证据做对抗性审查。链接到已接受文档是必要但不充分。审查必须在其存在处检查：所属研究文章、证据绑定设计合同、可执行 fixture 载荷与精确 fixture ID、schema/验证器或聚焦测试，以及拥有所声称的量、单位、顺序或状态转换的窄 H2/H3 命令。

审查必须专门测试：过期的问题队列、比其 fixture 更宽泛的摘要文章、在不同生命周期阶段复用的单位、被描述为运行时结果的源码静态调用顺序，以及被呈现为自然战役行为的受控验证接缝。在综合文档或其审查记录中记录已检查的表面与处置。

当所有者意见不一致时，B 层不得选择方便的答案或静默修复 A 层。排除有争议的结论或将其保留为 **未知**，向所属研究车道报告精确的不一致，并在扩展综合之前等待已接受的所有者修正。过期的队列或过宽的摘要仅在可执行所有者与更严格的主张边界一致时才是非阻塞的；综合必须使用更严格的边界并把差异对审阅者保持可见。

## 当前基线与近期综合

**已确认的仓库基线：** 现有合同覆盖战斗、地图、升级、法术、服务、存档/输入/窗口、对话、队伍/名册状态与随机性。它们列于[设计索引](../../README.md#design)，并追溯回研究与 fixture 所有者。本路线图不合并也不替换这些合同。

以下顺序是一种 **推断** 的规划优先级，并非声称所列设计结论已经存在。每篇综合文档在证据所有者稳定之前必须保持增量。

| 顺序 | 候选文档 | 范围与前置条件 | 现有合同/证据链接 | 非目标/停止条件 |
| ---: | --- | --- | --- | --- |
| 1 | gameplay overview | 解释当前受支持玩家动作、状态边界与主要子系统交接，仅从已接受的地图、对话、名册、服务与输入事实开始。 | [地图探索](../map-exploration.md)、[对话](../dialogue-system.md)、[队伍/名册](../party-roster-state.md)、[服务](../service-interactions.md)、[map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | 不得承诺完整战役流程、界面感受或叙事体验；这些保持 **未知**。 |
| 2 | tactical battle loop | 解释从玩家输入/控制经战斗行动、解决、状态回放到已知结果的受限顺序，同时保留每个未解决分支；需要已接受的 battle-loop/action/AI 研究与战斗/法术合同。 | [battle-loop 研究](../../research/battle-loop.md)、[battle-actions 研究](../../research/battle-actions.md)、[战斗](../combat-resolution.md)、[法术解决](../spell-resolution.md)、[physical-damage fixture](../../../tests/fixtures/h3/physical-damage-v1.json) | 不得从孤立用例发明战术、平衡意图、目标选择含义或一般模拟。 |
| 3 | progression and economy | 仅在输入、输出、顺序与持久性有证据之处，把成长、EXP/金币/物品与服务边界连接成资源流说明。 | [ally-growth 研究](../../research/ally-growth.md)、[common-stats 研究](../../research/common-stats.md)、[升级](../level-up.md)、[服务](../service-interactions.md)、[level-up fixture](../../../tests/fixtures/h3/level-up-boundaries-v1.json) | 不得声称预期的难度曲线、预期价格、最优构筑或长期经济。 |
| 4 | story progression | 解释已确认的状态/路线/对话/名册边界为可追溯的推进地图，同时保留正常剧情可达性与呈现标签；这些最不稳定的依赖使它排在前面文档之后。 | [gameflow 研究](../../research/gameflow-core.md)、[common-scripting 研究](../../research/common-scripting.md)、[对话](../dialogue-system.md)、[队伍/名册](../party-roster-state.md)、[dialogue runtime fixture](../../../tests/fixtures/h3/map-script-dialogue-v1.json) | 不得从源码标签或孤立程序引用重构剧情节拍、玩家选择后果或完整故事路线。 |

该顺序先建立读者导航，然后覆盖最受限的战术循环、连接起来的资源流，最后是依赖可达性的故事说明。当活动切片正在修订其所有者合同，或大多数答案保持 **未知** 时，文档等待。

## 长期方向

以下为 **未知** 的未来方向，不是当前承诺。只有当入场标准引用已接受的本地证据时才可开始工作；这些方向都不授权新的引擎设计。

| 方向 | 入场标准与证据依赖 | 非目标 |
| --- | --- | --- |
| map-design principles | 有文档记载的地图语料、路线/事件/区域证据，以及足以区分布局事实与玩家路线解读的可达性和交互结果观察。 | 不得仅从 64x64 布局数据推断作者意图或重新设计关卡。 |
| player roster choice space | 已接受的名单、职业/转职、成长、装备、出战队伍与持久性/容量边界；未解决的生命周期限制保持可见。 | 不得发布强度排行、“最优队伍”建议或假定的玩家偏好。 |
| player/enemy numerical curves | 完整的带来源数值表，加上运行时应证、上限与等级/遭遇上下文，足以命名单位与边界。 | 不得设定重制平衡目标，也不得把数学曲线描述为预期难度。 |
| battle simulation | 完整且互相兼容的 battle-loop/action/AI/寻路/状态合同，加上受限的 H4 适配器验收面。 | 不得选择模拟架构、声称一般预测精度，也不得用模型填补未解决分支。 |

## 可复用的编写结构

未来的 `docs/design/` 综合文档可以有选择地使用以下结构。这描述文档形状，而非并行工作区或强制性的完整 GDD 模板。

1. **读者与判断边界。** 识别读者——研究者、保真实现者或面向玩家的说明者——以及支持与不支持的判断。原版游戏主张在来源所有者层保留 **已确认**、**推断** 或 **未知**。
2. **玩家动作与动作-目标对齐。** 从有证据的输入、状态变更与结果开始。把原始源码标签与中性的玩家动作短语分开。没有本地证据的玩家目标或含义是 **推断** 或 **未知**。
3. **循环、状态流与系统动态。** 只对带证据所有者的有序转换、资源与反馈关系画图。保留未观察的分支，不要把控制流图呈现为引擎架构。
4. **证据矩阵。** 每个实质性条目包含其标签、受限主张、来源/研究所有者、合同、适用的 fixture ID/路径与剩余问题。本地链接如[运行时 RNG 与战斗数学](../../research/runtime-rng-and-battle-math.md)、[战斗 fixture](../../../tests/fixtures/h3/physical-damage-v1.json) 与[战斗合同](../combat-resolution.md) 是规范溯源；不要复制另一套证据台账。
5. **原版保真与现代化。** 先陈述原版保真规则，然后把刻意偏差标为带独立 expected-deviation fixture 的未来决定。在没有决定时，不得暗示现代化。
6. **H4 验收、扩展与停止条件。** 列出适配器可见的 parity 事实、fixture 消费者与扩展所需证据。当缺口是运行时、可达性、呈现或产品问题时停止，而非静默扩展合同。

## 外部参考溯源与选择性采用

**已确认的外部参考溯源：**
[DY-2026/GameDesignOS README](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/README.md)、
[contract catalog](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/README.md) 与
[player-promise contract schema](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/player-promise-contract.schema.json)
于 2026-08-01 在固定的 `main` 提交 `d01dfebc6eac7a619b9a18f3cbafa51270d1edba` 被访问；该仓库使用
[MIT license](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/LICENSE)。
复现命令 `git ls-remote https://github.com/DY-2026/GameDesignOS.git` 观察到该提交位于 `refs/heads/main`，并且对每个所列固定 raw 文档/模板的请求返回 HTTP 200。

以下结构性提示被选择性采用：
[player-verb inventory](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/player-verb-inventory.md)、
[system-dynamics map](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/system-dynamics-map.md)、
[game-dissection report](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-experience-analyzer/templates/game-dissection-report.md)、
[full design brief](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/full-design-brief.md) 与
[reference-game boundary](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/reference-game-boundary.md)：
读者/动作范围、可见不确定性、循环映射、证据链接、范围门禁与验证条件。它们在不复制模板文本的情况下，被适配到本仓库的证据标签与 H4 边界。

本项目明确拒绝外部项目的九目录工作区、商业 pitch/市场假设与第二套证据/决定系统。本仓库已拥有 `docs/research/`、`docs/design/`、`docs/decisions/`、`schemas/`、`manifests/research-index`、H2/H3 测试夹具与 H4 验收边界。外部参考只贡献有选择的编写视角；它不是项目依赖或新的真相来源。

## 协作与持续维护

**已确认的协作规则：** 综合文档可在逆向工程继续时加在已接受证据之上，但它们不得与活动的 worker 并行重写子系统合同。当未来发现改变结论时，同时更新所属研究笔记、fixture/合同与设计说明，使溯源保持双向。

**已确认的仓库维护收尾：** [`party-roster-state.md`](../party-roster-state.md) 现已注册于 `src/sf2tool/design_contracts.py`，附带其 H2 map-script 与 H3 active-party fixtures。公开的受追踪输入门禁在两个方向验证文档路径、fixture 路径与 fixture ID 的可追溯性。本收尾不改变任何原版游戏发现。
