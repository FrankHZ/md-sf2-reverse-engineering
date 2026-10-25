# 文档路线图与治理边界

- 状态：**已确认的仓库治理指南**；本文档不是关于原版游戏的证据，也不选择重制引擎、产品、平台或商业方向。
- 范围：将带来源的合同组织成简洁的面向玩家的说明，而不改变其证据标签；在各自的决策与验收边界内，区分原版证据、已接受的重制选择与未来工作。

> 本文件是 [`documentation-roadmap.md`](../documentation-roadmap.md) 的完整中文镜像。英文原文始终是审阅基线；镜像遵循 [`glossary.md`](../glossary.md) 的 R1–R7。源码标识符、fixture ID 与路径保持原样；详细证据链接指向英文规范所有者，中文阅读路线见[中文入口](../../README.zh-CN.md)。

## 编写语言政策

**已确认的仓库政策：** 在当前设计综合阶段，英文是新增或实质性修订的设计综合文档的规范编写与审阅语言。保留忠实于源码的标识符、fixture ID、证据标签与代码词汇，不另造翻译等价物。

**已确认的仓库决定（2026-08-04）：** 项目术语表已在 [`glossary.md`](../glossary.md) 接受。它是设计综合文档中英文到中文术语的唯一约束来源，并约束 `docs/design/zh-CN/` 下的镜像约定。不得创造临时双语术语；应采用术语表的固定术语与规则：证据标签译法、保留源码标识符、一词一译、专有名词保留英文并可加建议注记，以及条目修订流程。

非英文本地化从英文规范源出发，按术语表规则以独立批次进行，检查术语一致性、链接完整性、证据标签保留与 fixture 可追溯性。`docs/design/zh-CN/` 下的中文镜像是派生文档；除非某个本地化批次另有明确政策，英文源始终是审阅基线。

**已确认的当前顺序决定：** [设计文档 Epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514) 下的中文同步，以已独立验收、合并且在所选轮次中没有计划实质重写的英文源为基线。五个选定主题源与其中文镜像已在下述边界内接受。[#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434) 的[地图探索合同](../contracts/map-exploration.md)及中文镜像另经独立验收，其来源就绪性独立于五主题判定。这些验收不表示所有合同或详细指南均已稳定或有译文。

当前协议先修复已有镜像与读者导航，再评估缺失的译文。保留术语表、未经复核的译文锚、已知漂移和已完成的诊断失败。必须先翻译并审阅实质内容，再显式重新锚定对应记录；新增 pending 记录不代表完成翻译。限定范围验收或今后的全局元数据 PASS 均不等于双语 Epic 完成。

## 设计信息架构

**已确认的仓库政策：** `docs/design/` 的一级信息架构按归属和证据角色组织，而非仅按游戏功能组织：

- `contracts/` 包含证据绑定的子系统合同。已接受发现改变时，仍由相应研究切片负责。
- `synthesis/` 包含跨子系统或面向玩家的说明；消费 `main` 上已接受的证据，由设计综合车道负责。
- 包括本文与 `glossary.md` 在内的仓库治理文档保留在 design 根目录，因为它们同时约束两个类别。
- `zh-CN/` 保留英文规范源的相对层级，例如 `contracts/save-system.md` 对应 `zh-CN/contracts/save-system.md`。

此划分是归属边界，不是关于原版游戏架构的主张，也不是未来引擎的模块布局。只有具体切片建立了持久的导航或归属需求，才增加更深层的子目录；不得创建空的类别脚手架。

## 三层边界

| 层 | 归属与允许内容 | 禁止的捷径 |
| --- | --- | --- |
| A. 原版行为/数据合同 | `docs/research/`、`schemas/`、manifests、H2/H3 测试夹具与带来源的设计合同记录 **已确认**、**推断** 和 **未知** 事实。 | 源码宏、字段或符号名不能自动确立面向玩家的含义。保留源码标签，只解释有证据支持的解读。 |
| B. 重构的设计解释 | `docs/design/` 可以解释 A 层支持的玩家层面后果，链接本地研究/fixture 所有者，并将缺口标为 **未知**。 | 不得把静态调用、地址或看似合理的解读提升为原版行为、战役结论或玩家体验事实。 |
| C. 未来的重制决定 | 现代化、实现意图与产品选择属于显式决策及独立的 expected-deviation/H4 验收边界。 | 不得把现代化改写为原版行为，也不得通过综合文档选择引擎或产品方向。 |

**已确认的仓库规则：** B 层是已接受 A 层证据的可追溯解读，不是第二套证据系统。**推断** 的解读保留该标签，**未知** 的行为仍是问题，不得补写成叙事。

## 综合前证据审查

**已确认的仓库规则：** 每个 B 层综合切片必须对它将解释的 A 层证据做对抗性审查。链接到已接受文档是必要条件，但不充分。相关材料存在时，审查必须检查所属研究文章、证据绑定设计合同、可执行 fixture 载荷与精确 fixture ID、schema/验证器或聚焦测试，以及负责所声称的量、单位、顺序或状态转换的窄 H2/H3 命令。

审查必须专门检查：过期问题队列、比其 fixture 更宽泛的摘要、跨生命周期阶段复用的单位、被描述成运行时结果的源码静态调用顺序，以及被呈现为自然战役行为的受控验证接缝。在综合文档或审查记录中记下已检查的材料与处置。

所有者不一致时，B 层不得选择方便的答案或静默修复 A 层。排除争议结论或保留为 **未知**，向所属研究车道报告精确差异，并在扩展综合前等待所有者修正获接受。过期队列或过宽摘要，只有在可执行所有者与更严格的主张边界一致时才可不阻塞；综合必须采用更严格边界，并让审阅者看见差异。

## 当前基线与近期综合

**当前范围协议 — Standard。** 已接受的英文设计解释与当前有边界的中文同步，消费 [Epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514) 下已接受的数据。目标是以可读方式说明游戏中相连的规则、世界推进与玩家选择，为后续设计和实现决策提供支持。证据所有者已知，但存在跨系统依赖，因此采用 Standard：声明各主题的主张与验收，独立审阅；新主张或归属冲突改变范围时重新评估。本协议不选择新引擎、不重新平衡游戏，也不扩大可玩里程碑。

### 已接受的起点

[玩法总览](../synthesis/gameplay-overview.md)仍是读者入口。已有的[战术战斗循环](../synthesis/tactical-battle-loop.md)、[成长与经济](../synthesis/progression-and-economy.md)、[剧情推进](../synthesis/story-progression.md)和[地图设计原则](../synthesis/map-design-principles.md)仍是主题所有者。它们已接受的修订提供下述相连解释。既有详细剧情指南仍可经剧情推进与[设计索引](../../README.md#design)访问，包括 [Battle27 激活指南](../synthesis/battle27-activation-and-first-turn-dispatch.md)。

[剧情内容覆盖所有者](../../research/story-content-coverage.md#four-different-frontiers)区分原版静态研究、原版观察、英文综合和重制生产。已接受的静态章节连接延伸至结尾、字幕和 Battle0 重置，并包含选定的可选遭遇与分支。这支持有限定的战役层面说明，不是自然游玩攻略、每个 NPC/宝箱/服务访问、每种合法交战行动或穷尽的内容普查。后续研究不会自动成为已接受综合。覆盖所有者的[剩余处置](../../research/story-content-coverage.md#remaining-labels-and-next-content-task)没有选择自动采集队列。

### 选定英文源集合与协调

本轮英文源集合严格限于以下五个已有综合文件。各文件现已具备实质说明；**本轮均无进一步实质重写计划**。该处置仅涉及所选主题，不覆盖全部设计合同或详细章节指南。

| 选定来源 | 当前读者责任 | 剩余实质重写 |
| --- | --- | --- |
| [gameplay-overview.md](../synthesis/gameplay-overview.md) | 相连的循环、主题入口，以及原版/观察/重制/生产边界。 | 无计划。 |
| [story-progression.md](../synthesis/story-progression.md) | 延伸至结尾/reset 的六组章节、选定可选分支和 Taros/Caravan 依赖示例。 | 无计划。 |
| [tactical-battle-loop.md](../synthesis/tactical-battle-loop.md) | 通用选择、结算与返回，包含五个遭遇对比及其例外。 | 无计划。 |
| [progression-and-economy.md](../synthesis/progression-and-economy.md) | 将成员、职业、装备与成长连接到接收、容量、支出和恢复。 | 无计划。 |
| [map-design-principles.md](../synthesis/map-design-principles.md) | 交互尝试、对话继续、交通、当前/重建状态与持久化边界。 | 无计划。 |

**已接受的整体来源处置：** 这五份英文源已通过 main-gate 独立读者验收并合并，本轮无计划实质重写。其中文主题镜像也已独立验收并合并。审阅路线为总览 → 对应主题 → 章节/示例 → 具名证据，主题及章节链接返回有用的阅读入口。相连示例沿 [Ribble/Achilles → Taros → Caravan](../synthesis/story-progression.md#worked-example-taros-and-the-caravan) 展开：容量/拒绝、持有与装备、准入与效果、首次/重复及非胜利返回、获取与登乘，以及独立的服务访问。补充的 Creed/Jaro、奖励/smith 与 Polca no-scroll/scroll 分支检查成员/控制、资源及生命周期交接。这些是选定的静态连接与有限观察，不是一次自然游玩的战役或全内容审计。详细审阅路线、精确接受源对象及实际检查结果由 [Issue #1280](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1280) 及其 PR 保存。[中文读者入口](../../README.zh-CN.md)通往已接受的主题镜像与另行接受的地图探索镜像；翻译索引记录它们经审阅的版本锚。

验收严格覆盖五个选定主题与 #434 的独立合同，不表示全部设计文档、全局翻译 PASS 或双语 Epic #514 完成。未复核镜像保留锚与失败。未翻译的细节转到英文所有者；没有中文镜像不代表原版研究或英文解释缺失。

保留问题是对解释的明确限制，不是计划重写或自动研究任务：

| 保留问题 | 所属边界 |
| --- | --- |
| 自然战役可达性、每种合法行动/交互及未观察的调用者/呈现完成。 | [剧情覆盖的四个前沿](../../research/story-content-coverage.md#four-different-frontiers)及选定章节/遭遇所有者。静态连接不延伸原版已观察路线。 |
| 完整转职变更、全名册选择与战役平衡/稀缺性。 | [队伍/资源判断边界](../synthesis/progression-and-economy.md#judgment-boundary)、[转职](../synthesis/progression-and-economy.md#definitions-current-class-and-promotion)及所列合同。不主张最优配装或通用曲线。 |
| 完整交通/返回生命周期，以及存档/挂起相关状态能否持久保留。 | [地图返回/持久化](../synthesis/map-design-principles.md#return-and-persistence)、[资源持久化](../synthesis/progression-and-economy.md#persistence-boundary)及[存档合同](../contracts/save-system.md)。进入存档域、存档返回与持久存储仍相互独立。 |
| 更广可玩内容、硬件保真与未来 UI/UX 或存档选择。 | [能力所有者](../../../remake/docs/capability-status.md)、[已接受里程碑](../synthesis/map3-battle01-readiness.md#accepted-current-milestone)及[已接受/未来选择](../synthesis/gameplay-overview.md#made-remake-choices-and-remaining-product-choices)。英文覆盖不授予生产准入。 |

历史原版/现代版失败、#1047 的十二个已完成但未解决的比较，以及先前实际诊断、修正与 CI，仍由链接的证据及 Issue/PR 所有者保存。不通过重跑历史套件或改标签来建立此来源处置。

[Map3–Battle01 就绪性所有者](../synthesis/map3-battle01-readiness.md#accepted-current-milestone)与[保留比较所有者](../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)保存已接受组合及已完成失败。原版观察保留[独立边界](../synthesis/map3-battle01-readiness.md#accepted-original-frontier)。当前生产支持由[能力台账](../../../remake/docs/capability-status.md)、所链接的消费者及[覆盖区分](../../research/story-content-coverage.md#four-different-frontiers)负责。重组文字不会推进这些边界。历史报告、失败比较与未知问题仍由原所有者负责；本计划既不以 PASS 替换它们，也不重启它们。[Phase 4 引导计划](../synthesis/phase4-bootstrap-plan.md)保留为历史；[ADR 0019](../../decisions/0019-state-and-content-driven-remake-engine.md)负责当前引擎方向。

### 阅读结构与归属

在已有架构内采用三个阅读深度：

1. **总览：** 解释探索、遭遇、战斗、奖励/恢复和返回世界的循环，指向相关主题说明。区分原版知识、可玩重制支持与已接受偏差。
2. **主题与章节组：** 用普通玩法语言解释规则与依赖。读者无需读汇编或原始 fixture 行，就应理解可尝试的动作、条件、变化、失败/拒绝、返回行为与剩余不确定性。
3. **详细参考：** 链接现有片段指南、合同与研究，以获取精确标志、源码顺序、参数和证明。每项事实保留其规范所有者；不复制完整源码表、不在每章重复技术审计，也不建立第二套证据清单。

章节按有意义的世界变化与内容依赖组织，而非每个战斗编号或调查任务一页。初始编辑分组是 **推断**：Grans 开场与离开；New Granseal 与东部 Parmecia；南方旅程、Caravan 与 Creed；北方旅程、Pacalon、Tristan 与 Mitula；Moun/Nazca 与返回 Grans；最终交锋与结尾。可选招募、交通、试炼与重访放在实际门槛旁，不写成必经的编号步骤。这些是阅读分组，不主张自然时间顺序或原作者预定章节。

优先复用现有页面。只有现有所有者不足以承担责任时，才增加主题页；对应 Issue 必须列出文件及从原页面接管的边界。重组时保留稳定链接，或修复实际调用方。不得创建空目录、新目录框架或整体搬移文件。长度由内容和可读性决定，不设上限。

### 交付顺序与验收

以下五项已接受的读者边界约束所选源集合，不是一行一个任务或新的写作队列；它们仍是未来实质修改的标准。每个可执行 Issue 负责一个连贯的读者结果和精确路径；独立审阅同时判断读者价值与证据解读。

| 里程碑 | 读者问题与交付物 | 所有者与依赖 | 验收边界 |
| --- | --- | --- | --- |
| 1. 相连总览与战役结构 | 系统如何衔接、世界发生哪些变化、选择在哪分支？更新总览，将剧情推进组织成章节路线并链接既有细节；从已接受研究扩展后期章节。 | 已有总览/剧情所有者；[章节覆盖](../../research/story-content-coverage.md)、[游戏流程](../../research/gameflow-core.md)、[对话](../contracts/dialogue-system.md)、[名册](../contracts/party-roster-state.md)。 | 读者能沿选定章节主线，通过有限定的入口、前提、遭遇/结局及返回连接到达结尾，找到可选分支，区分静态连接与已观察游玩。每个选定组均解释或明确限定；编号顺序不是证明。 |
| 2. 战术规则与遭遇构成 | 战斗员能做什么、单次行动如何结算、敌人如何激活、遭遇有何变化？修订战术战斗循环，以代表例子组织有证据的差异。 | [战斗控制](../contracts/battle-control-lifecycle.md)、[行动](../contracts/battle-action-construction.md)、[导航](../contracts/battlefield-navigation.md)、[AI](../contracts/battle-ai-decision.md)、[交战](../contracts/combat-resolution.md)、[法术](../contracts/spell-resolution.md)及覆盖所链接的选定遭遇研究。 | 解释合法行动、移动/目标条件、结算、敌人激活/控制、结局与重试/返回边界。区分共通规则与 boss、中立、重复、特殊遭遇例外；初始编队或首次调度不证明已游玩战斗或每项 AI 决策。不做模拟器、难度评级或平衡方案。 |
| 3. 队伍发展与资源 | 招募、出战队伍、职业/转职、装备与成长如何交互，物品、魔法、金钱、服务如何约束选择？让队伍发展有清楚所有者，按这些关系修订成长与经济。 | [名册](../contracts/party-roster-state.md)、[成长](../contracts/level-up.md)、[同伴定义](../contracts/ally-definition-data.md)、[物品定义](../contracts/item-definition-data.md)、[法术表](../contracts/spellbook-state.md)、[服务](../contracts/service-interactions.md)、[Caravan/deals](../contracts/caravan-and-deals-state.md)及已接受招募/服务研究。 | 区分加入、跟随、出战成员与 AI；将成长/装备连接到实际规则应用。解释资源来源、转移、消耗、容量、拒绝/失败和恢复，保留实际服务边界。参数引用其所有者；不发明最优配装、战役平衡曲线或完整服务/运行时覆盖。 |
| 4. 探索、交互与连续性 | 移动、交互、地图转换、对话、交通和世界变化如何组合，哪些状态在各类返回后保留？更新地图设计原则及相关剧情/总览章节，区分存档/挂起与普通地图返回。 | [地图探索](../contracts/map-exploration.md)、[对话](../contracts/dialogue-system.md)、[地图进入状态](../contracts/map-entry-routing-state.md)、[存档](../contracts/save-system.md)、[输入](../contracts/input-system.md)及已接受场景/服务/交通所有者。 | 解释条件交互、当前实体与重建实体、剧情门槛、返回/重新进入和持久化限制。通过链接所有者概述已接受的反馈/输入/音频选择；不增加 UI 布局、美术/无障碍选择、存档设计或硬件一致性目标。 |
| 5. 整体读者验收 | 读者能否无矛盾地从总览到主题，经章节示例回到证据？协调交叉链接与重复说明，记录所选英文主题的稳定状态及保留问题。 | 全部已接受主题修订、路线图与现有导航；main-gate 独立审阅。 | 审阅至少一条相连路线，以及跨主题的可选/拒绝或失败分支。检查术语、状态区分和原版/重制边界一致，而非仅检查链接。未解释的已主张转换交回所有者。英文完成不表示双语 Epic 完成。 |

整体审阅采用已有阅读结构与跨系统实例。已接受细节仍在[剑的取得指南](../synthesis/ribble-tree-opening-and-achilles-sword.md)、[Taros 准入指南](../synthesis/southwest-parmecia-through-taros-battle18-entry.md)、[Battle18 初始化/首次控制研究](../../research/battle18-initialization-and-first-control.md)、[Caravan 获取指南](../synthesis/battle18-outcomes-and-caravan-acquisition.md)与[Caravan 旅行指南](../synthesis/caravan-boarding-and-southwest-parmecia-travel.md)。示例连接物品/遭遇条件、战斗与结局边界、获取和后续通行选择，保留拒绝/失败及缺少前提的分支。未建立的结果保持为精确缺口；示例不主张自然连续行程。详细参考所有者保持只读，具体修正另定范围。main-gate 串行安排 roadmap/index 及其他共享编辑。整体验收检查主题间连接与重复术语，不是另一轮通用重写或研究活动。新矛盾须先返回所有者，再扩展相关主张。

### 证据、投入与停止规则

综合前证据审查仅针对实际选定主张。复用已接受的精确对象、详细审阅和公开事实；数量、状态转换或解读需要检查时，查看有关源码/fixture 边界。不因主题指南引用过往提取、几何计算或实验，就机械重跑每项工作。所有者冲突必须明确修正或排除争议主张；独立说明可以继续，未解决事实仍保留 **未知**。

每项实质规则都须保留条件、可观察或源码定义的后果、重要例外和所有者链接。解读标为 **推断**；静态证明保持静态。新的重制选择需要独立产品/决策所有者。综合缺口只有在阻断具名解释或实现决策、且已接受所有者无法回答时才成为研究 Issue。该 Issue 列出最小缺失条件与所需证据，不是采集全部剩余数据的指令。已有详细失败及 #1047 已完成的未解决比较继续保留，不改称中断，也不为清理文字而重跑。

这是使用现有文本与公开事实的文档工作。采用一个活动主题 writer、独立审阅和串行集成，产物为小规模 Markdown 差异与本地审阅笔记。不需要新原始采集、私有载荷复制、生成数据语料库、数据库、比较框架或受维护检查器。不确定成本是证据协调与可读综合，不是处理量。后续扩展以实际编写、审阅和修正投入估算。若主题需要复制同一事实、任务无法定义读者问题，或证据修复开始主导解释，应在扩大工作前重新评估归属与范围。不为任意成本或文档长度目标牺牲必要设计和审阅。

每个切片须直接检查文字/范围、相对链接/锚、UTF-8/私有边界与 `git diff --check`，并按仅文档政策运行配置中的已提交 planner，记录实际 scoped CI。翻译切片使用现有术语表、索引及源到镜像的直接标签、结构、fixture 和锚检查；在限定交接处运行既有全局 `zh-meta test`，保留任何范围外 FAIL。本协议不启动普通/完整 Python、.NET、Godot 或 H3/H4 全套。独立审阅判断证据解读和读者价值；机械 PASS 不充分。交付干净、已提交并推送的 Draft PR，等待独立验收。本计划不删除历史报告、私有输入或保留失败。

本路线图负责当前协议与来源处置；Issue 负责工作协调。后续英文扩展须另定 Issue。更多中文翻译需要自己的来源稳定性评估及限定文件/验收范围；五主题和 #434 的接受不使其他来源自动就绪。[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) 及其 UI/UX 任务保留产品归属；[#638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/638) 继续暂停。本轮不授权生产准入、新美术、存档/读档实现、全游戏一致性或通用比较器。

### 有边界的翻译修复协议

先按三个连贯批次修复已有镜像，依序集成：

1. **索引、路线图与导航：** 登记全部英文规范源，将缺少镜像的来源列为 pending；修正本协议、完整翻译最终路线图，并恢复当前双语阅读路线。其他每条译文记录原样保留。范围外漂移保持 FAIL。
2. **七份通用合同：** 对已有音频、战斗 AI、战斗控制、对话、音乐等待、队伍名册和随机性镜像，分别复核已接受来源后同步。保留行为与证据边界，不修改英文规范合同。
3. **连续场景与就绪性：** 同步已有连续 Map3–Battle01 场景和就绪性镜像，保留已接受的当前组合、原版前沿限制及历史失败；随后复核全局索引和翻译诊断。

完成这些修复后，按读者需求及已接受来源的稳定性，独立评估剩余 90 份 pending 文档。它们是翻译缺口，不是自动启动 2.25 MB 翻译、新研究队列或宣布 #514 完成的授权。不创建逐文件微任务或新的目录/同步框架。持续同步复用当前索引与直接检查；英文实质变更须有经审阅的中文更新，或明确的待同步缺口。

索引/导航切片检查约 34 KB 的 roadmap 来源、两份导航页与 88 条缺失记录；工作量与文档字节数线性相关，输出是小型文本/JSON。后续合同和里程碑批次在各自限定范围内复用相同检查。来源分歧、需要新研究，或证据修复主导翻译时，先重新评估再扩展。本协议不授予新采集、引擎能力、UI/UX 选择、游戏脚本汉化或清理权限。精确派发、结果与剩余失败由链接的 [Epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514) 及其限定 Issue/PR 负责。

## 长期方向

除上述有边界的主题解释外，以下扩展仍是 **未知的未来方向**，不是当前承诺。工作必须以引用已接受本地证据的进入标准为前提；这些方向均不授权新引擎设计，也不把重制功能当作原版游戏证据。

| 方向 | 进入标准与证据依赖 | 非目标 |
| --- | --- | --- |
| 地图设计原则的体验层面扩展 | 已有结构综合是基线；新增可达性与交互结果观察必须区分布局事实和玩家路线解读。 | 不得仅凭 64x64 布局数据推断作者意图或重新设计关卡。 |
| 跨战役状态的穷尽名册选择空间 | 本轮解释已接受的招募/发展规则。更广比较需要超出所选说明且互相兼容的容量、生命周期与遭遇上下文。 | 不做强度排行、“最优队伍”建议、偏好假设或完整战役优化。 |
| 玩家/敌人数值曲线 | 完整的源码支持数值表，加上运行时确认的应用、上限和等级/遭遇上下文，足以明确单位与边界。 | 不设重制平衡目标，也不把数学曲线描述成预期难度。 |
| 战斗模拟 | 完整且互相兼容的战斗循环/行动/AI/寻路/状态合同，加上有边界的 H4 适配器验收面。 | 不选择模拟架构、不主张一般预测精度，也不用模型填补未解决分支。 |

## 可复用的编写结构

未来 `docs/design/` 综合文档可选择性采用下列结构。它描述文档形态，不是并行工作区或强制性的完整 GDD 模板。

**已确认的编写指南：** 按连贯主题、读者流程和可维护归属组织设计文档。提供理解限定主题所需的解释；不限制行数、字数或页数，不仅因长度拆文档，也不为简短目标删除必要说明。手写源码行数限制不适用于设计 Markdown。投入预算约束工作扩展，不约束文档长度。

1. **读者与判断边界。** 说明读者是研究者、保真实现者还是面向玩家的说明者，以及支持和不支持的判断。原版游戏主张在来源所有者层保留 **已确认**、**推断** 或 **未知**。
2. **玩家动作与动作/目标对齐。** 从有证据的输入、状态变化和结果出发。区分原始源码标签与中性玩家动作短语。无本地证据的玩家目标或含义标为 **推断** 或 **未知**。
3. **循环、状态流与系统动态。** 只为具备证据所有者的有序转换、资源与反馈关系制图。保留未观察分支，不把控制流图当作引擎架构。
4. **证据矩阵。** 每个实质条目包含标签、限定主张、源码/研究所有者、合同、适用的 fixture ID/路径和剩余问题。[运行时 RNG 与战斗数学](../../research/runtime-rng-and-battle-math.md)、[交战 fixture](../../../tests/fixtures/h3/physical-damage-v1.json)及[交战合同](../contracts/combat-resolution.md)等本地链接是规范追溯；不另抄证据台账。
5. **原版保真与现代化。** 先陈述原版保真规则，再将有意偏差标为带独立 expected-deviation fixture 的未来决定。没有决定时不暗示现代化。
6. **H4 验收、扩展与停止条件。** 列出适配器可见的一致性事实、fixture 消费者与扩展所需证据。缺口属于运行时、可达性、呈现或产品问题时停止，不静默扩展合同。

## 外部参考溯源与选择性采用

**已确认的外部参考溯源：** [DY-2026/GameDesignOS README](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/README.md)、[合同目录](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/README.md)及[玩家承诺合同 schema](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/player-promise-contract.schema.json)，于 2026-08-01 在固定 `main` 提交 `d01dfebc6eac7a619b9a18f3cbafa51270d1edba` 访问；该仓库使用 [MIT 许可证](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/LICENSE)。复现命令 `git ls-remote https://github.com/DY-2026/GameDesignOS.git` 观察到该提交位于 `refs/heads/main`；对每个列明的固定 raw 文档/模板的请求均返回 HTTP 200。

选择性采用以下结构提示：[玩家动作清单](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/player-verb-inventory.md)、[系统动态地图](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/system-dynamics-map.md)、[游戏拆解报告](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-experience-analyzer/templates/game-dissection-report.md)、[完整设计简报](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/full-design-brief.md)及[参考游戏边界](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/reference-game-boundary.md)：读者/动作范围、显式不确定性、循环映射、证据链接、范围门禁与验证条件。它们被适配到本仓库证据标签与 H4 边界，未复制模板文本。

本项目明确拒绝外部项目的九目录工作区、商业推介/市场假设和第二套证据/决策系统。本仓库已拥有 `docs/research/`、`docs/design/`、`docs/decisions/`、`schemas/`、`manifests/research-index`、H2/H3 测试夹具与 H4 验收边界。外部参考只贡献选择性的编写视角，不是项目依赖或新的事实来源。

## 协作与持续维护

**已确认的协作规则：** 逆向工程继续时，可在已接受证据上增加综合文档，但不得与活动 worker 并行重写子系统合同。未来发现改变结论时，一并更新所属研究笔记、fixture/合同和设计解释，保持双向可追溯性。

**已确认的仓库维护收尾：** [`party-roster-state.md`](../contracts/party-roster-state.md) 已在 `src/sf2tool/design_contracts.py` 注册其 H2 map-script 和 H3 active-party fixtures。公开受追踪输入门禁双向验证文档路径、fixture 路径与 fixture ID 的可追溯性；这项收尾不改变任何原版游戏发现。
