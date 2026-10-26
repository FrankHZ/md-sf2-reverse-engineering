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

一条具体路线是总览 → 剧情推进中的 [Taros/Caravan 示例（中文）](./design/zh-CN/synthesis/story-progression.md#完整示例taros-与-caravan)
→ 剑的持有/装备、遭遇准入、结局、获取与登乘的各自证据。

该示例的四篇详细中文指南是：

1. [Ribble 树洞开启与 Achilles Sword](./design/zh-CN/synthesis/ribble-tree-opening-and-achilles-sword.md)：两个宝箱的容量/拒绝/重试、精确位置与朝向的 Panel 使用、条件 Sword 持有与返回。
2. [Southwest Parmecia、Taros 警告与 Battle18 进入](./design/zh-CN/synthesis/southwest-parmecia-through-taros-battle18-entry.md)：独立的警告/撤退和战斗准入，首次/重复战前程序及激活前交接。
3. [Battle18 结果与 Caravan 获取](./design/zh-CN/synthesis/battle18-outcomes-and-caravan-acquisition.md)：首次/重复胜利、败北/逃脱、Rohde 演示、F65/F84 变化及紧接的条件返回。
4. [Caravan 登乘与 Southwest Parmecia 旅行](./design/zh-CN/synthesis/caravan-boarding-and-southwest-parmecia-travel.md)：获取/随从与 mode 1 登乘的区别、登陆、独立步行 Battle19 请求；C 菜单路径止于调用，不证明服务事务。

这是条件依赖的阅读顺序，不是自然必经攻略；Sword 持有、装备与伤害规则保持各自边界。
[Hassan/Rohde 的提议（英文）](./design/synthesis/hassan-ancestry-and-rohde-caravan-choice.md)、
[Rick 可选招募（英文）](./design/synthesis/hassan-rick-recruitment.md)及其他未译相邻指南仍回退英文；
四篇指南各自的证据追踪和未知项保留静态连接、实际观察、表现/输入与持久性的限制。

Grans 开场经 Hawel、Galam、塔楼后续和地震撤离，至航行登陆/损坏船只条件控制的详细中文阅读路线是：

1. [开篇至 Battle 01：地点、人物与返回](./design/zh-CN/synthesis/opening-through-battle01.md)：选定住宅/学校、信使、城堡/塔、Battle01 和可用返回；区分原版观察与已接受现代组合。
2. [Battle 01 返回至 Battle 02 准入](./design/zh-CN/synthesis/battle01-return-through-battle02.md)：有条件的城堡驱魔/委任、Jaha 加入和调用方解锁，以及西出口遭遇选择。
3. [Battle 02 结果至 Battle 03 准入](./design/zh-CN/synthesis/battle02-outcome-through-battle03.md)：结果与替代返回、Yeel/Kazin 内容及独立的大地图请求/准入；跟随不同于招募，Yeel 不是已证明的必需前提。
4. [Battle 03 结果至 Battle 04 准入](./design/zh-CN/synthesis/battle03-outcome-through-battle04.md)：Hawel 住宅默认/重复内容、Kazin 正式加入，以及独立住宅出口与遭遇选择。
5. [Battle 04 结果至 Battle 05 准入](./design/zh-CN/synthesis/battle04-outcomes-through-battle05.md)：胜利/特殊败北/逃脱、Galam 俘虏与通道、Slade 跟随及守卫条件解锁；不证明剧情必败。
6. [Battle 05 结果至 Battle 06 准入](./design/zh-CN/synthesis/battle05-outcomes-through-battle06.md)：独立位置/HP 调用、首次完成宝石场景与 Slade 招募、Galam 出口及入场时 F407 写入。
7. [Battle06 结果、Granseal 内容与 Battle07 准入](./design/zh-CN/synthesis/battle06-outcomes-through-battle07.md)：F506 内容、可选 Kiwi 招募、王室跟随和条件塔请求；保留 F401 拦截、F999 替换及跳过分支。
8. [Battle07 结果与探索交接](./design/zh-CN/synthesis/battle07-outcomes-and-aftermath.md)：四类结果、分阶段塔楼场景、宝石/当前跟随/持久标志，以及脚本和调用方完成后空 Map58 配置重建与条件控制。
9. [塔后返回至地震城镇入场](./design/zh-CN/synthesis/post-tower-return-through-earthquake-entry.md)：条件塔楼/城堡请求、独立 Astral 对话、F640 地图替换和首次/重复地震场景；止于初始化后控制/事件读取。
10. [地震城镇出口至码头出航](./design/zh-CN/synthesis/earthquake-town-through-docks-departure.md)：变化后的港口、同地图登船、老人 Yes/No 选择、F74 清除与实体重建，以及脚本内 Map66 加载后的条件 Map73 请求。
11. [航行登陆至损坏船只控制](./design/zh-CN/synthesis/voyage-through-damaged-ship-control.md)：首次/重复选择、可选 Kiwi、人群登陆及 Jaha 木板场景；两次脚本内 Map14 加载绕过 init，F700 先于条件控制读取写入。

后十篇解释源码条件连接，不证明自然连续行程，也不扩展止于 Battle01 可用返回的重制里程碑。
从此处接续 New Granseal 与 Peter 的六篇详细中文指南：

1. [木板选择至 Battle08 准入](./design/zh-CN/synthesis/damaged-ship-plank-through-battle08.md)：可选木板回应与重复救援请求、请求先于调用方写入，以及激活前开场边界。
2. [Battle08 结果至 New Granseal 城堡交接](./design/zh-CN/synthesis/battle08-outcomes-through-new-granseal-handoff.md)：首次/重复与非胜利返回、工人后续、条件建设和脚本后 F705；止于城堡 init 前。
3. [New Granseal 城堡：一年后讲话与首次控制交接](./design/zh-CN/synthesis/new-granseal-castle-year-later-control.md)：配置/场景选择、实体群重建及 F707/F411 后的条件控制读取。
4. [New Granseal 城镇：Peter 首次登场](./design/zh-CN/synthesis/new-granseal-town-through-peter-introduction.md)：独立 F706/F701 触发、接待与隐藏；不把场景动作当作招募或持久遣离。
5. [Peter 城堡觐见与返回城镇](./design/zh-CN/synthesis/peter-castle-audience-through-town-return.md)：加入/出战/AI、临时/持久跟随、礼物叙事与静态效果差异及城镇条件控制。
6. [New Granseal 出发至 Battle09 准入](./design/zh-CN/synthesis/new-granseal-departure-through-battle09.md)：滚动与普通入场、部分重载和有序选择；止于激活前空 start 返回。

这仍是条件连接的阅读路线，不证明自然必经行程、可用输入或扩展重制里程碑；服务完成与持久性未知保留在各指南。
下一段的 [Ribble 欢迎（中文）](./design/zh-CN/synthesis/battle09-outcomes-through-ribble-welcome.md)接续 Battle09 结果，经可选 May 招募、East Shrine 与洞窟返回，
到达 [Polca 仙女互动至 Battle12 准入（中文）](./design/zh-CN/synthesis/polca-fairy-through-battle12-admission.md)。六篇详细指南及可选/条件边界集中在
[New Granseal 与东部 Parmecia 章节（中文）](./design/zh-CN/synthesis/story-progression.md#new-granseal-与东部-parmecia)，不构成必经链或自然连续游玩证明。
继续沿 [Battle12 结果至 Bedoe 入场（中文）](./design/zh-CN/synthesis/battle12-outcomes-through-bedoe-entry.md)，经国王讲话/首次觐见、Volcanon、可选国王选择与雏鸟救援；
五篇详细指南及其独立准入、状态和条件控制边界仍集中在上述剧情章节。后续 [Bedoe 返回至 Battle13 准入（英文）](./design/synthesis/bedoe-return-through-battle13-admission.md)及其他未译细节仍回退英文。
[详细章节指南（英文）](./README.md#design)仍提供精确条件和所有者；未译的详细指南沿英文链接阅读，不把静态连接写成自然连续行程。

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
