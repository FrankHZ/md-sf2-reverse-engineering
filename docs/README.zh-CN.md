# 文档索引

这是当前中文阅读入口。[英文索引](./README.md)是规范索引；证据、合同与决策以各自的英文所有者为准。
设计翻译遵循[术语表](./design/glossary.md)。已索引设计文档均有经独立全文双语验收、合并的完整中文镜像；
英文研究、ADR 与引擎文档保留各自所有者，不属于这份设计翻译语料。

## 按阅读目的进入

| 你想了解什么 | 从哪里开始 |
| --- | --- |
| 游戏规则如何连成一个整体 | [玩法总览](./design/zh-CN/synthesis/gameplay-overview.md) → 下方[主题路线](#玩法与设计阅读路线) |
| 某个地点、章节或遭遇 | [章节与遭遇](#章节与遭遇) → 中文说明 → 各自链接的英文证据 |
| 精确的条件、状态和结算规则 | [合同参考](#按主题查合同)；合同描述规则，不能直接当作已实现能力 |
| 现在能玩什么、怎样启动和验证 | [当前重制与开发](#当前重制与开发)；使用英文工程文档 |
| 翻译是否同步、怎样继续维护 | [翻译状态与持续同步](#翻译状态与持续同步) |

目录按职责保留原有层级：`research/` 是英文原版证据，`design/synthesis/` 是玩法与章节解释，
`design/contracts/` 是规则合同，`design/zh-CN/` 按原相对路径保存中文镜像。
本页按主题和章节组织阅读，不把文件机械迁入新目录。完整英文文档清单见[英文索引](./README.md)。
原版事实、设计解释和重制支持有不同边界，不能凭某章已有中文说明就认为该章已经可玩。

执行仓库任务时，先检查实时 Git、工作树和进程，再沿[任务恢复入口（英文）](./operations/agent-resume.md)定位所有者。
常规任务无需通读全局索引；仓库文档和精确 Git 对象是持久记录。

## 玩法与设计阅读路线

从[玩法总览（中文）](./design/zh-CN/synthesis/gameplay-overview.md)开始，再选主题，沿章节/示例链接进入合同与研究证据。
下列主题连接已接受的中文合同与现有详细指南；它们支持限定的规则与章节说明，不证明自然全流程、全部内容或生产支持。

| 读者问题 | 已接受的中文主题 | 英文规范源 |
| --- | --- | --- |
| 探索、遭遇、战斗、奖励与返回怎样连接？原版、观察和重制的边界是什么？ | [玩法总览](./design/zh-CN/synthesis/gameplay-overview.md) | [Gameplay Overview](./design/synthesis/gameplay-overview.md) |
| 单次行动有哪些选择，如何结算，代表遭遇有什么例外？ | [战术战斗循环](./design/zh-CN/synthesis/tactical-battle-loop.md) | [Tactical Battle Loop](./design/synthesis/tactical-battle-loop.md) |
| 加入、出战、职业、成长、装备与资源容量/消费如何相互约束？ | [成长与经济](./design/zh-CN/synthesis/progression-and-economy.md) | [Progression and Economy](./design/synthesis/progression-and-economy.md) |
| 交互、对话、交通、返回与地图重建怎样组合，哪些状态能保留？ | [地图设计原则](./design/zh-CN/synthesis/map-design-principles.md) | [Map Design Principles](./design/synthesis/map-design-principles.md) |
| 六组章节如何推进，必需条件与可选分支在哪里？ | [剧情推进](./design/zh-CN/synthesis/story-progression.md) | [Story Progression](./design/synthesis/story-progression.md) |

## 章节与遭遇

按章节选择详细阅读入口；完整指南清单、可选分支与精确条件集中在[剧情推进（中文）](./design/zh-CN/synthesis/story-progression.md#分段阅读故事内容)。
这些是条件连接的阅读分组，不是自然必经攻略，也不扩展各重制切片的可玩范围；当前边界见下方工程入口。

| 章节 | 有用的入口与接续 |
| --- | --- |
| [Grans 开篇与离开（中文）](./design/zh-CN/synthesis/story-progression.md#grans-开篇与离开) | 从[开篇至 Battle01（中文）](./design/zh-CN/synthesis/opening-through-battle01.md)，经 Hawel、Galam、塔楼和地震撤离，至[航行登陆与损坏船只控制（中文）](./design/zh-CN/synthesis/voyage-through-damaged-ship-control.md)。 |
| [New Granseal 与东部 Parmecia（中文）](./design/zh-CN/synthesis/story-progression.md#new-granseal-与东部-parmecia) | 从[木板选择至 Battle08 准入（中文）](./design/zh-CN/synthesis/damaged-ship-plank-through-battle08.md)，经 Peter、Ribble、East Shrine、Polca 与 Bedoe，至[Polca 出发与 Luke 招募（中文）](./design/zh-CN/synthesis/polca-departure-through-luke-recruitment.md)。 |
| [南方旅行、Caravan 与 Creed（中文）](./design/zh-CN/synthesis/story-progression.md#南方旅行caravan-与-creed) | 从[木筏登乘与 South Parmecia 入场（中文）](./design/zh-CN/synthesis/polca-raft-boarding-through-south-parmecia-entry.md)，经 Kraken、Hassan、Taros/Caravan、Creed Mansion、Desktop 与 Floor World，接续 Evil Spirit 咨询、招募及 Oddler 告别，至 [Creed 离开、矮人恢复与 Cannon 赠礼（中文）](./design/zh-CN/synthesis/creed-departure-dwarf-recovery-and-cannon-gift.md)。完整指南见本章节导读；之后接续返回 New Granseal 的北方章节。 |
| [北方旅行、Pacalon、Tristan 与 Mitula（中文）](./design/zh-CN/synthesis/story-progression.md#北方旅行pacalontristan-与-mitula) | 从[返回 New Granseal 与 Astral 跟随（中文）](./design/zh-CN/synthesis/new-granseal-return-and-astral-following.md)，经 North Cliff、Ketto、Higins 招募与 Pacalon，至 [Battle27 激活与首次单次行动交接（中文）](./design/zh-CN/synthesis/battle27-activation-and-first-turn-dispatch.md)。之后的 Tristan 与 Mitula 已有同一篇中文章节说明；精确证据链接到英文研究所有者。 |
| [Moun、Nazca 与返回 Grans（中文）](./design/zh-CN/synthesis/story-progression.md#mounnazca-与返回-grans) | 章节导读连接 Moun 幸存者、Nazca 航行与返回 Grans，并保留可选招募/锻造分支；中文章节已有连贯说明，精确证据沿其链接阅读英文研究所有者。 |
| [最终对决与结局（中文）](./design/zh-CN/synthesis/story-progression.md#最终对决与结局) | 章节导读连接 Sword、塔路线、尾声、亲吻选择及字幕/隐藏战斗重置；中文章节已有连贯说明，精确证据沿其链接阅读英文研究所有者。 |

### 常用详细入口

不必先记英文文件名；以下链接按实际阅读问题选择。地点与人名保留术语表规定的英文形式。

| 主题 | 中文详细指南 |
| --- | --- |
| 开篇、城堡委托与下一场遭遇 | [开篇地点、人物与 Battle01 返回](./design/zh-CN/synthesis/opening-through-battle01.md)；[城堡返回、委托与 Battle02 准入](./design/zh-CN/synthesis/battle01-return-through-battle02.md)；[Battle02 返回、Yeel 与 Battle03 准入](./design/zh-CN/synthesis/battle02-outcome-through-battle03.md) |
| 地震撤离与新的定居点 | [地震城镇与码头离开](./design/zh-CN/synthesis/earthquake-town-through-docks-departure.md)；[木板选择与 Battle08 准入](./design/zh-CN/synthesis/damaged-ship-plank-through-battle08.md)；[一年后的城堡讲话](./design/zh-CN/synthesis/new-granseal-castle-year-later-control.md) |
| Peter、Bedoe 与 Polca | [Peter 的城堡觐见与回城](./design/zh-CN/synthesis/peter-castle-audience-through-town-return.md)；[Volcanon 觐见](./design/zh-CN/synthesis/bedoe-to-volcanon-audience.md)；[木筏赠予](./design/zh-CN/synthesis/battle13-outcomes-through-polca-raft-grant.md) |
| Taros、剑与车队 | [树洞与 Achilles Sword](./design/zh-CN/synthesis/ribble-tree-opening-and-achilles-sword.md)；[Taros 警告与 Battle18 准入](./design/zh-CN/synthesis/southwest-parmecia-through-taros-battle18-entry.md)；[战斗结果与 Caravan 获取](./design/zh-CN/synthesis/battle18-outcomes-and-caravan-acquisition.md)；[另行登乘与旅行](./design/zh-CN/synthesis/caravan-boarding-and-southwest-parmecia-travel.md) |
| Creed、棋盘与缩小世界 | [魔法门与桌面转移](./design/zh-CN/synthesis/creed-magic-door-and-desktop-transfer.md)；[桌面王国交谈与棋盘挑战](./design/zh-CN/synthesis/desktop-kingdom-interactions-through-battle22-entry.md)；[棋盘初始阵容](./design/zh-CN/synthesis/battle22-initialization-and-roster.md)；[棋盘结果与返回](./design/zh-CN/synthesis/battle22-outcomes-and-desktop-return.md)；[Floor World 居民与出口](./design/zh-CN/synthesis/floor-world-interactions-and-exits.md) |
| 北方旅行与 Pacalon | [North Cliff 开通与 Rohde 招募](./design/zh-CN/synthesis/north-cliff-opening-and-rohde-recruitment.md)；[Ketto 交谈与商人迁移](./design/zh-CN/synthesis/ketto-interactions-and-merchant-relocation.md)；[Higins 招募](./design/zh-CN/synthesis/battle26-outcomes-and-higins-recruitment.md)；[Pacalon 城堡初访](./design/zh-CN/synthesis/pacalon-castle-entry-and-first-visit.md) |
| 后期章节与结局 | [Tristan 居民与 Mitula 洞穴路线](./design/zh-CN/synthesis/story-progression.md#tristan-居民与通向-mitula-的洞穴路线)；[Jaro、Frayja 与 Moun 幸存者](./design/zh-CN/synthesis/story-progression.md#jaro-叛离frayja-加入与-moun-幸存者)；[后期 Yeel：钢琴、Chaz 与 Lemon](./design/zh-CN/synthesis/story-progression.md#后期-yeel钢琴chaz-与-lemon)；[亲吻选择](./design/zh-CN/synthesis/story-progression.md#minister-提示与玩家真正的亲吻选择)；[字幕与隐藏战斗](./design/zh-CN/synthesis/story-progression.md#字幕终场完成存储与隐藏战斗) |

这些文档已经解释规则、条件流程和部分遭遇例外；轻量战斗设计分析
[#1402（英文任务）](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1402)仍是后续独立工作，
尚未交付战术意图、难度或平衡分析。

也可沿总览 → [Taros/Caravan 完整示例（中文）](./design/zh-CN/synthesis/story-progression.md#完整示例taros-与-caravan)
→ 物品容量/拒绝、剑的持有与装备、遭遇准入/结果、获取与登乘的各自证据，查看跨系统依赖。
四篇详细中文指南及 Ribble、Hassan/Rohde、可选 Rick 的相邻入口均可从该章节找到。
Sword 持有、装备和伤害规则保持各自边界；静态连接、实际观察、表现/输入与持久性限制仍由各指南保留。
[设计索引（英文）](./README.md#design)保留完整规范文档导航；后期细节若直接链接英文研究，表示其证据职责，并非尚待翻译的设计指南。中文镜像不把静态连接写成自然连续游玩。

## 按主题查合同

先读主题解释，再用合同核对精确条件；各中文合同保留英文规范源、fixture 与英文研究链接。
[英文合同全目录](./README.md#contracts-combat-turns-and-encounter-decisions)按同样的职责分组，
中文镜像始终位于对应的 `design/zh-CN/contracts/` 路径。

| 规则或接口 | 中文合同入口 |
| --- | --- |
| 战斗控制、移动与 AI | [战斗控制及生命周期](./design/zh-CN/contracts/battle-control-lifecycle.md)、[战场寻路及目标空间](./design/zh-CN/contracts/battlefield-navigation.md)、[AI 决策](./design/zh-CN/contracts/battle-ai-decision.md)、[遭遇定义](./design/zh-CN/contracts/battle-encounter-definition.md) |
| 行动构建与效果结算 | [行动构建](./design/zh-CN/contracts/battle-action-construction.md)、[物理交战](./design/zh-CN/contracts/combat-resolution.md)、[法术伤害](./design/zh-CN/contracts/spell-resolution.md)、[战斗演出](./design/zh-CN/contracts/battle-scene-presentation.md) |
| 成员、职业、成长与资源 | [队伍与名册](./design/zh-CN/contracts/party-roster-state.md)、[伙伴定义与成长数据](./design/zh-CN/contracts/ally-definition-data.md)、[升级](./design/zh-CN/contracts/level-up.md)、[物品定义](./design/zh-CN/contracts/item-definition-data.md)、[法术表状态](./design/zh-CN/contracts/spellbook-state.md) |
| 地图、交互与程序 | [地图探索](./design/zh-CN/contracts/map-exploration.md)、[探索控制流](./design/zh-CN/contracts/exploration-control-flow.md)、[入图路由](./design/zh-CN/contracts/map-entry-routing-state.md)、[地图实体](./design/zh-CN/contracts/map-entity-data.md)、[全局标志](./design/zh-CN/contracts/global-flag-state.md) |
| 服务、交易与持久化 | [服务交互](./design/zh-CN/contracts/service-interactions.md)、[车队与特卖](./design/zh-CN/contracts/caravan-and-deals-state.md)、[存档](./design/zh-CN/contracts/save-system.md)；描述原版服务不等于重制已有完整服务 UI 或持久保存 |
| 输入、对话与声音 | [输入](./design/zh-CN/contracts/input-system.md)、[对话与已达文本等待](./design/zh-CN/contracts/dialogue-system.md)、[窗口](./design/zh-CN/contracts/window-system.md)、[音频命令与播放状态](./design/zh-CN/contracts/audio-system.md)、[音乐等待](./design/zh-CN/contracts/music-wait-service.md) |
| 图形与界面数据 | [地图布局](./design/zh-CN/contracts/map-layout-data.md)、[地图瓦片集](./design/zh-CN/contracts/map-tileset-data.md)、[UI 布局](./design/zh-CN/contracts/ui-layout-data.md)、[立绘窗口](./design/zh-CN/contracts/portrait-window-state.md) |

私有里程碑另见[连续 Map3–Battle01 场景](./design/zh-CN/contracts/map3-battle01-continuous-scenario.md)与
[就绪台账](./design/zh-CN/synthesis/map3-battle01-readiness.md#当前已接受里程碑)；下方保留英文规范验收入口。
[Map3 受控准入合同](./design/zh-CN/contracts/map3-controlled-admission.md)仍为**提议（Proposed）**，
[Phase4 引导计划](./design/zh-CN/synthesis/phase4-bootstrap-plan.md)仍为**历史（Historical）**且已被现行实现指导取代；译文接受不提升其状态。

## 翻译状态与持续同步

[路线图（中文）](./design/zh-CN/documentation-roadmap.md)完整说明当前范围与[持续同步协议](./design/zh-CN/documentation-roadmap.md#有边界的翻译修复协议)；[英文规范协议](./design/documentation-roadmap.md#bounded-translation-repair-agreement)是审阅基线。已索引设计语料均已有独立全文双语验收并合并的完整译文；当前版本锚和漂移状态以现有命令复现。内容与导航变更须经独立语义及读者路线验收；main-gate 负责集成与 Epic 完成判断。

[翻译索引](../manifests/zh-translation-index.json)记录逐文档状态与审阅版本锚。使用 `uv run --locked sf2 zh-meta list` 查看记录、`uv run --locked sf2 zh-meta test` 复现全局检查。元数据 PASS 只证明检查器覆盖的同步、链接、结构与 fixture 约束，不能替代全文语义与读者路线验收。

未来英文实质变更或新增文档须先确认 accepted main 来源与稳定性，再限定范围、完整翻译和独立审阅。只更新已审阅的受影响记录，其他记录与术语表元数据保持不变。尚未同步时保留旧锚并明确报告漂移，不能只刷新 hash 或日期。`translated` 表示有已锚定译文，不保证来源仍最新；`pending` 表示无镜像，应回退英文。它们都不是原版研究或采集队列。持续同步不包括游戏脚本汉化，也不扩大重制能力。

历史原版/现代失败及 [#1047 的十二项已完成未解决比较](./research/story-content-coverage.md#unknowns-retained-failures-and-next-decision)仍由证据所有者保留，当前组合验收不改写其结果。

## 当前重制与开发

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

当前状态/内容驱动引擎运行通用 authored（项目自编）包及有边界的私有本地内容。
已接受的连续 Map3 → Battle01 胜利与可用返回之外，已有 Battle02 胜败、返回与再次入战，以及选定的
Yeel 指引、Kazin 跟随和 Battle03 可操作战斗；这不包括 Battle03 结果、Hawel 招募或后续全战役。
完整服务、存读档、全游戏保真和原版素材公开分发仍不在这些切片的范围内。

当前呈现已有连续探索/情境 Talk、1920×1080 共享世界和 96px 格子、蓝金剧情对话框、可达移动格、
HP/MP 图片状态卡、战斗消息、返回反馈与只读队伍状态页。各项呈现的限定验收和现代化偏离见
[呈现所有者（英文）](../remake/docs/godot/presentation.md)；更广的美术/UI/UX 和最终人类审美验收仍单独管理。
启动用法见 [Remake README（英文）](../remake/README.md#run-locally)，构建与验证范围见上表的开发入口。

M5 已退役过渡期 reference 实现、host 与旧测试项目；外部受控比较输入保留原有边界。
原版观察前沿、历史失败与硬件精确一致性仍由各证据所有者负责，当前组合验收不改写旧报告结果。

用户确定的测试政策已生效：只新增实际引擎行为的单元测试；验证、probe、fixture driver、planner、门禁、报告和助手直接使用，
不另为这些程序加测试。旧测试按行为迁移或退役，不要求数量保留、每删必补或旧全套全绿，原版证据继续保留。
Godot 默认复用已有安装、项目及运行中的调试实例；行为验收读取实际状态与输入结果，禁止截图。
只有具体故障或必要的启动、导入、导出、清理验证才另行启动或隔离实例。
