# 文档索引

这是当前中文阅读入口。[英文索引](./README.md)是规范索引；证据、合同与决策以各自的英文所有者为准。
设计翻译遵循[术语表](./design/glossary.md)。下面区分已有中文镜像及其审阅状态与英文回退；
缺少中文译文不表示缺少研究或英文说明。

先检查 Git 提交、工作树、改动和进程，再沿[任务恢复入口（英文）](./operations/agent-resume.md)定位最近的所有者。
常规任务无需通读全局索引或覆盖率台账。仓库文档和精确 Git 对象是持久记录；旧聊天与历史启动脚本不是新指令。

## 玩法与设计阅读路线

从[玩法总览（中文）](./design/zh-CN/synthesis/gameplay-overview.md)开始，再选主题，沿章节/示例链接进入合同与研究证据。
下列五主题英文源及中文镜像已经独立验收并合并；它们支持限定的规则与章节说明，不证明自然全流程、全部内容或生产支持。

| 读者问题 | 已接受的中文主题 | 英文规范源 |
| --- | --- | --- |
| 探索、遭遇、战斗、奖励与返回怎样连接？原版、观察和重制的边界是什么？ | [玩法总览](./design/zh-CN/synthesis/gameplay-overview.md) | [Gameplay Overview](./design/synthesis/gameplay-overview.md) |
| 单次行动有哪些选择，如何结算，代表遭遇有什么例外？ | [战术战斗循环](./design/zh-CN/synthesis/tactical-battle-loop.md) | [Tactical Battle Loop](./design/synthesis/tactical-battle-loop.md) |
| 加入、出战、职业、成长、装备与资源容量/消费如何相互约束？ | [成长与经济](./design/zh-CN/synthesis/progression-and-economy.md) | [Progression and Economy](./design/synthesis/progression-and-economy.md) |
| 交互、对话、交通、返回与地图重建怎样组合，哪些状态能保留？ | [地图设计原则](./design/zh-CN/synthesis/map-design-principles.md) | [Map Design Principles](./design/synthesis/map-design-principles.md) |
| 六组章节如何推进，必需条件与可选分支在哪里？ | [剧情推进](./design/zh-CN/synthesis/story-progression.md) | [Story Progression](./design/synthesis/story-progression.md) |

按章节选择详细阅读入口；完整指南清单、可选分支与精确条件集中在[剧情推进（中文）](./design/zh-CN/synthesis/story-progression.md#分段阅读故事内容)。
这些是条件连接的阅读分组，不是自然必经攻略，也不扩展止于 Battle01 可用返回的重制里程碑。

| 章节 | 有用的入口与接续 |
| --- | --- |
| [Grans 开篇与离开（中文）](./design/zh-CN/synthesis/story-progression.md#grans-开篇与离开) | 从[开篇至 Battle01（中文）](./design/zh-CN/synthesis/opening-through-battle01.md)，经 Hawel、Galam、塔楼和地震撤离，至[航行登陆与损坏船只控制（中文）](./design/zh-CN/synthesis/voyage-through-damaged-ship-control.md)。 |
| [New Granseal 与东部 Parmecia（中文）](./design/zh-CN/synthesis/story-progression.md#new-granseal-与东部-parmecia) | 从[木板选择至 Battle08 准入（中文）](./design/zh-CN/synthesis/damaged-ship-plank-through-battle08.md)，经 Peter、Ribble、East Shrine、Polca 与 Bedoe，至[Polca 出发与 Luke 招募（中文）](./design/zh-CN/synthesis/polca-departure-through-luke-recruitment.md)。 |
| [南方旅行、Caravan 与 Creed（中文）](./design/zh-CN/synthesis/story-progression.md#南方旅行caravan-与-creed) | 从[木筏登乘与 South Parmecia 入场（中文）](./design/zh-CN/synthesis/polca-raft-boarding-through-south-parmecia-entry.md)，经 Kraken、Hassan、Taros/Caravan、Elven Village 和 Harpy Pond，接续 Fairy Cave、Battle21、Creed 门事件与[Desktop 对话及棋盘挑战（中文）](./design/zh-CN/synthesis/desktop-kingdom-interactions-through-battle22-entry.md)，再经 Chessboard 结果、Cotton Balloon 至 [Floor World 居民与退出（中文）](./design/zh-CN/synthesis/floor-world-interactions-and-exits.md)。再经 [Willard Burrow/Battle23（中文）](./design/zh-CN/synthesis/willard-burrow-through-battle23-entry.md)、[Creed 恢复（中文）](./design/zh-CN/synthesis/creed-restoration-and-basement-departure.md)，至 [Evil Spirit 咨询与区域（事件）返回（中文）](./design/zh-CN/synthesis/evil-spirit-consultation-and-zone-return.md)。后续宅邸返回/Fairy/招募细节回退英文。 |
| [北方旅行、Pacalon、Tristan 与 Mitula（中文）](./design/zh-CN/synthesis/story-progression.md#北方旅行pacalontristan-与-mitula) | 章节导读连接 Cannon/North Cliff、Ketto、Pacalon、Tristan 与 Mitula；详细指南回退英文。 |
| [Moun、Nazca 与返回 Grans（中文）](./design/zh-CN/synthesis/story-progression.md#mounnazca-与返回-grans) | 章节导读连接 Moun 幸存者、Nazca 航行与返回 Grans，并保留可选招募/锻造分支；细节与证据沿章节链接阅读英文研究所有者。 |
| [最终对决与结局（中文）](./design/zh-CN/synthesis/story-progression.md#最终对决与结局) | 章节导读连接 Sword、塔路线、尾声、亲吻选择及字幕/隐藏战斗重置；细节与证据沿章节链接阅读英文研究所有者。 |

也可沿总览 → [Taros/Caravan 完整示例（中文）](./design/zh-CN/synthesis/story-progression.md#完整示例taros-与-caravan)
→ 物品容量/拒绝、剑的持有与装备、遭遇准入/结果、获取与登乘的各自证据，查看跨系统依赖。
四篇详细中文指南及 Ribble、Hassan/Rohde、可选 Rick 的相邻入口均可从该章节找到。
Sword 持有、装备和伤害规则保持各自边界；静态连接、实际观察、表现/输入与持久性限制仍由各指南保留。
[详细章节指南（英文）](./README.md#design)提供其他未译细节及其规范所有者；中文镜像不把静态连接写成自然连续游玩。

[地图探索合同（中文）](./design/zh-CN/contracts/map-exploration.md)及其[英文规范源](./design/contracts/map-exploration.md)
已在 #434 中另行评估来源稳定性并独立验收。它解释受控地图/交互边界；其接受独立于上述五主题，不表示所有地图或服务运行时均已验证。
其他可用参考包括[存档](./design/zh-CN/contracts/save-system.md)与[输入](./design/zh-CN/contracts/input-system.md)的既有中文镜像；
新增结论仍须回到它们链接的英文证据所有者。

## 翻译状态与持续同步

[路线图（中文）](./design/zh-CN/documentation-roadmap.md)完整说明当前协议；[英文规范协议](./design/documentation-roadmap.md#bounded-translation-repair-agreement)
规定先修复已有镜像，再按读者需求与来源稳定性评估缺译文档。三个修复批次依次为索引/路线图/导航、七份通用合同、连续场景/就绪性。
这不是整库自动翻译或 #514 完成声明。

[翻译索引](../manifests/zh-translation-index.json)是逐文档状态与审阅版本锚的入口。`translated` 表示已有经锚定的译文，
不保证其来源仍然最新；`pending` 表示尚无中文镜像，阅读时回退到英文规范源。英文实质变化后须复核译文，不能仅刷新 hash 消除漂移。
用现有 `uv run sf2 zh-meta list` 查看记录、`uv run sf2 zh-meta test` 检查当前全局状态；范围外失败继续保留为 FAIL。

已有镜像已完成同步修复，包括七份通用合同及[连续 Map3–Battle01 场景](./design/zh-CN/contracts/map3-battle01-continuous-scenario.md)
和[就绪台账](./design/zh-CN/synthesis/map3-battle01-readiness.md#当前已接受里程碑)。当前全局索引检查 PASS：已有镜像的来源/术语表/译文锚均为当前版本，链接和结构检查通过。
这只表示已索引镜像同步，不表示全部设计文档已有译文，也不表示 #514 完成。剩余 `pending` 沿英文规范源阅读；它们是翻译缺口，不是待补原版研究的自动队列。
历史原版/现代失败及 [#1047 的十二项已完成未解决比较](./research/story-content-coverage.md#unknowns-retained-failures-and-next-decision)仍由证据所有者保留，当前组合验收不改写其结果。

## 工程、研究与验收入口

下表均为英文规范文档；中文导航不复制它们的能力表、覆盖率或历史验收清单。

| 工作 | 当前英文入口 |
| --- | --- |
| 工作规则、隔离与验证范围 | [AGENTS](../AGENTS.md) |
| 引擎实现与架构 | [Remake README](../remake/README.md)、[架构](../remake/docs/architecture.md) |
| 已采用的引擎方向与测试政策 | [ADR 0019](./decisions/0019-state-and-content-driven-remake-engine.md) |
| 当前可运行能力、Unsupported 与 Unknown | [能力矩阵](../remake/docs/capability-status.md) |
| 当前私有 Map 3 → Battle01 胜利与可用返回验收 | [就绪性](./design/synthesis/map3-battle01-readiness.md#accepted-current-milestone) |
| 历史失败与组合接受证据 | [保留比较](../remake/docs/evidence/retained-comparisons.md#accepted-composition-review) |
| 验证命令与 Godot 实例复用 | [开发与验证](../remake/docs/development-and-verification.md) |
| 原版研究与证据前沿 | [Research 索引](./README.md#research)、[源码覆盖率](./research/source-coverage.md) |
| 原版静态、原版观察、英文综合与重制生产的区别 | [剧情覆盖的四个前沿](./research/story-content-coverage.md#four-different-frontiers) |
| 原始决策与后续修订 | [Decisions 索引](./README.md#decisions) |
| 文档与 agent instruction 审计 | [审计记录](./operations/documentation-agent-audit.md) |

当前引擎包含通用 authored 战斗会话及有边界的私有 Map 3 → Battle01 世界；具体支持以能力矩阵为准。
M5 已退役过渡期 reference 实现、host 与旧测试项目，`remake/reference/inputs` 仅保留外部受控比较输入。
私有连续 Map 3 → Battle01 胜利与可用返回，已在当前组合的 8D/H4 边界验收；原版观察前沿、历史比较失败、
硬件精确一致性和更广生产能力各保留自己的边界，不能把旧报告失败改称当前里程碑未完成。

用户确定的测试政策已生效：只新增实际引擎行为的单元测试；验证、probe、fixture driver、planner、门禁、报告和助手直接使用，
不另为这些程序加测试。旧测试按行为迁移或退役，不要求数量保留、每删必补或旧全套全绿，原版证据继续保留。
Godot 默认复用已有安装、项目及运行中的调试实例；行为验收读取实际状态与输入结果，禁止截图。
只有具体故障或必要的启动、导入、导出、清理验证才另行启动或隔离实例。
