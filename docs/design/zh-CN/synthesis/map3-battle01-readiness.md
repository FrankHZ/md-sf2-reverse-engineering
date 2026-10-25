# Map 3 至 Battle 01 就绪台账

- 状态：**当前私有连续里程碑已接受**，范围见[下文组合边界](#当前已接受里程碑)。
- 当前验收所有者：[现代连续比较](../../../../remake/docs/evidence/retained-comparisons.md#current-keyboard-comparison-scope)与[能力状态](../../../../remake/docs/capability-status.md#current-engineering-frontier)。原版证据及已完成的历史失败仍由下文各自所有者维护。
- 里程碑：[ADR 0009](../../../decisions/0009-first-phase4-playable-slice.md)；画像：[ADR 0010](../../../decisions/0010-map3-battle01-product-acceptance.md)。
- 启动政策：[ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md)；引擎方向：[ADR 0019](../../../decisions/0019-state-and-content-driven-remake-engine.md)。
- 定义所有者：[连续场景合同](../contracts/map3-battle01-continuous-scenario.md)。

本文件是[英文台账](../../synthesis/map3-battle01-readiness.md)的中文镜像；英文为审阅基线，术语与证据标签遵循[术语表](../../glossary.md)，源码标识符保持原样。

## 当前验收范围

选定画像为 `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`。
8D 比较玩法与呈现语义，包括实际宿主消费、完成、确认及输入就绪。不要求像素、帧、波形、芯片以及 VInt/DMA/CRAM/VDP 相等；影响玩法、因果顺序或输入可用性的时序仍是必需项。
私有原版内容、自然连续性、5B 及单独报告的适用偏差仍是必需项。[当前键盘范围](../../../decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope)要求默认键盘 A；C 是补充，手柄 B/D 排除。已有重映射、按键交换、减闪和可调文字不产生额外整段路线要求。独立的 #517 快速文字语音选择仍是必需项。

已接受的现代确定性时钟可能改变原版整段历史的 RNG、行动顺序和资源。这些历史保留为诊断；同状态规则/RNG 效果、必经路线、手动操作自主权、实际内容使用及消费者边界仍是必需项。不能仅凭时钟决策推断历史差异的因果解释。

本台账核算证据与验收，精确比较规则由所链接的合同拥有。原版证据、定义审阅、可执行报告结论与组合里程碑验收是不同的声明。
本台账不消费未合并的 Research 或 Remake 结果。合同不注册测试夹具、schema 或索引关联。Godot 验收读取运行中的状态/输入/投影；禁止截图。本次文档变更不需要 normal/full/native 套件。

## 已接受原版前沿

**已确认的有界原版观察：**[最终采集所有者](../../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)与[审计](../../../research/map3-battle01-audit.md)建立了受控 R1 经 messenger、Map19、royal/guard、城堡/塔、自然 Battle01 准入、已到达行动及自然胜利，直至战后程序之后的稳定野外就绪边界。
已接受源码是 `9c3ea03ac5f5b467ee744f1ac624870da2408443`；PR #504 在 `5102804b` 接受该结果。选定的新 prepared-68..86 谱系验证 18 对父子关系，只使用成功获胜请求。这是通过即时存档串接的连续性，不是不中断的墙钟时间或自然 New/load 流程。

自然首个行动者为 **2**；R2d 注入行动者 1 的情况仍是显式受控桥接。最终链到达回合 **14**、单次行动 **103**，胜利帧 **58657**、`abcs_battle01` 返回 **60945**，随后是共享尾部/外层返回、F401 清除、F501 设置、BattleLoop D4=1、SwitchMap、ExplorationLoop 和 Map57 `ms_Void`。
**67** 对已到达操作不等于静态语料的 80 条记录。

两个完成的中性输入帧 **61009/61010** 确认 Map57、玩家 `(5,12)`、DOWN、battle255、F401=false/F501=true、名册 `[0,1,2]`、金币 **420**、已停稳的移动/摄像机，以及没有阻塞脚本/模态/传送/战斗/消费者。己方角色的精确 HP/MP/等级/物品/法术/状态及 RNG 读回见[合同端点](../../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint)及其原版所有者。
通用窗口字节 1 不是阻塞对话。PR #504 终端的原版输入轮询为中性；该终端保持不变且不可续跑。已接受 PR #526 单独重现该边界，观察一次普通 Down 读取及停稳后的 Map57 `(5,13)` 位移。这关闭有界 RA-12 输入/效果观察，不代表完整 5B 或里程碑。PR #526 扩展本身也已终止且不可续跑。末段 106 对音频分发/邮箱记录不证明完整 8D。

较早 R1/R2/R2a/R2d 测试夹具及 Map19/首个玩家就绪观察保留各自受限投影，不因最终链而自动扩展。精确 post-447 等待入口仍为**推断**；公开 R1 投影省略状态及完整物品槽位。使用获胜谱系的读回，不能使用另一尝试的零状态或 R2d 注入顺序。历史失败及清理未知项（含尝试 21/41/61/67）保留在采集所有者。已完成败局和传输/观察器失败不是中断运行，也不会被最终成功抹去。

## 就绪检查表

| 门禁 | 当前结果 | 所有者 / 精确保留边界 |
| --- | --- | --- |
| 里程碑、引擎与产品选择 | PASS | ADR0008/0009/0010；此处没有新产品决定 |
| 受控准入 | 有界 PASS | [准入合同](../../contracts/map3-controlled-admission.md)；选定状态/完整物品槽位/NPC/RNG 现有[离线绑定](../../contracts/map3-battle01-continuous-scenario.md#offline-reference-bindings)；完整 R1 标志及其他省略的原版字段仍未知 |
| 自然必经路线及遭遇准入 | 有界原版证据 PASS | 最终采集；实际行动者 2；不是 R2d 桥接 |
| 获胜行动与已消费结果 | 有界原版证据 PASS | 选定获胜链；已提交行动/种子/主 RNG 抽取记录已有离线绑定；完整原版野外输入归一化、逐次抽取至效果和取消/重选超出已接受接缝的部分仍未知 |
| 胜利、战后程序、标志与返回主干 | 有界原版证据 PASS | 67 对已到达操作及选定最终链；不是完整呈现声明 |
| 精确中性停稳端点 | 有界原版证据 PASS | 合同端点与原版终端；其余完整记录必须从私有证据消费 |
| 有界 RA-12 普通输入/效果 | 有界原版证据 PASS | PR #526：一次 Down 读取及 Map57 `(5,12)` 至 `(5,13)` 的停稳位移；独立终端，不可续跑，不属于 PR #504 投影器绑定 |
| 完整可控 5B | 已接受组合边界 | 普通连续获胜/返回及已交付野外控制证明；独立原版 RA-12 扩展保留自己的谱系 |
| 连续合同与十层定义 | 已接受当前要求映射 | 所链接合同定义字段、来源、实际映射及失败/不可用规则；历史投影缺口保留其命名限制 |
| 每项当前适用断言的预期证据 | 已接受源码与受控组合 | 完整当前映射在下文接受；不补出省略的历史原版字段 |
| 存档政策 6A | 已接受 | 无用户持久化界面；重启回到准入状态 |
| 7C 内容/溯源 | 已接受组合边界 | 已到达视觉/音频溯源及可变地图交付，范围按各自声明。私有内容不跟踪；自编 JoinCue 和弦或静音不能替代必需原版音频 |
| 8D 语义呈现 | 已接受组合边界 | 文字、移动、操作、音频和战斗演出绑定，加上独立检查的父级覆盖与共享依赖；原版硬件时序仍在范围外 |
| 已有设置与有界直接观察 | 有界实现 PASS；补充/历史 | [设置所有者](../../../../remake/docs/verification/host.md#native-9a-observation)；实现不授权额外必需变体 |
| 当前键盘范围 / 10A 偏差 | 已接受 | 仅 A 必需；C 为诊断，B/D 排除。范围自相等只证明范围行；显式偏差与共享证据在下文独立审阅 |
| 必需已到达行动支持 | 已接受组合边界 | 普通 Medical Herb/实时物品栏支持，以及选定获胜行动/规则/消费者比较，保留各自所有者 |
| 实际连续比较 | 已接受组合边界 | 已执行连续路线和比较，加上下文局部义务；保留 A10/矩阵及限定范围报告结论不变 |
| 所有适用 H4 层 | 已接受独立审阅的组合 | 必需父/子断言、共享证据身份和偏差均已核算；没有新的完整可执行 H4 报告 |
| 独立里程碑就绪验收 | 已接受 | Main-gate 在下文边界作出的决定；基于证据及集成检查，不基于 Issue 状态或 PASS 数量 |
| 独立实现启动授权 | PASS | [Remake README](../../../../remake/README.md) 中的用户授权；不接受本里程碑 |
| 公开发行 | 私有里程碑范围外 BLOCKED | 需要独立权利/许可替代决定；私有资产不跟踪 |

## H4 组合规则

[合同十层](../../contracts/map3-battle01-continuous-scenario.md#ten-h4-comparison-layers)是：(1) 准入/溯源，(2) 逻辑输入/路线，(3) 世界/剧情转换，(4) 遭遇准入，(5) 行动顺序/行动/RNG/已消费效果，(6) 胜利/战后程序/返回，(7) 精确端点与普通输入效果，(8) 存档排除/私有资产，(9) 呈现身份/顺序/消费/完成/确认/就绪，(10) 显式偏差。

每项断言分别保留预期来源与实际观察。缺失原版字段或私有输入时，输出 Unavailable 并列明缺失侧/字段，不能 PASS、填零或隐含排除。不匹配和已观察到不支持的必需行动为 FAIL。原版未知字段使其原版字段定义保持 OPEN。
整段报告 PASS 要求显式当前范围内每项断言与偏差都通过。补充或排除报告不能改变必需项总计。生产玩法合法性必须依赖状态/内容，不能依赖固定行动者序列、回合数或参考回执历史。已有子系统测试夹具保留其所有者。

### 初始偏差清单与 H4 映射

合同映射全部 ADR0010 偏差：受控构造（1A）、可选路线排除（2A）、固定准入种子和使用已接受现代确定性时钟的手动游玩（4A）、无存档（6A）、当前键盘范围及单独报告的设置（9A）、明确标识的域外安全行为（10A）。每个结果都在第 10 层单独可见，含 PASS。7C 私有处理是产品边界，不是保真度豁免；缺证据/内容不能改称偏差。

## 依赖与完成边界

| 所有者 | 剩余工作 / 依赖 |
| --- | --- |
| Research | PR #526 独立接受有界 RA-12 输入/效果。其他选定原版字段、输入归一化、RNG 和呈现缺口仍按各自所有者定义 |
| Design | 保持当前适用性、键盘验收边界与合同一致；原版 PR #526 扩展仍独立于 PR #504 投影器 |
| Remake/content | 在其命名依赖下复用已接受音频/私有资源、可变地图交付和战斗演出消费者证明。#517 Option A 保留受控揭示尾段机制与历史 C 限制 |
| H4 执行者 | 保留已接受组合、精确依赖、失败与 Unavailable 报告；没有具体新缺陷就不需要进一步证据工作 |
| Main-gate | 拥有独立验收及串行集成；当前私有里程碑已在下文接受 |

已接受[结果实现](../../../../remake/docs/application/session-and-programs.md#battle01-outcome-after-program-and-return)与 R4a 比较证明其有界共同会话/静态主干行为，不证明自然原版预期值或本次 H4 运行。[能力台账](../../../../remake/docs/capability-status.md)保留其他不受支持消费者。除非选定路线需要，不扩展到 EGRESS 或无关物品/菜单分支。已有重映射/按键交换与成对减闪/文字观察仍是有界证据；缺设备/导出覆盖不扩大当前键盘里程碑。本台账不引发新采集。

### 当前必需比较边界

**已确认：**保留 A10 先前缺失的十三项局部义务现已有独立接受的源码/消费者组合。各自合同定义当前范围：

| 必需子项 | 已接受所有者与边界 |
| --- | --- |
| 准入种子副本字节 | [种子组合](../../contracts/map3-battle01-continuous-scenario.md#admission-seed-copy-composition)：非恢复情况下先写后读，加上当前副本机制 |
| 开场嘴部/视野控制 | [受控开场](../../contracts/map3-battle01-continuous-scenario.md#controlled-opening-control-binding)：选定原版回调/回执谱系 |
| 行动分数/顺序 | [行动顺序组合](../../contracts/map3-battle01-continuous-scenario.md#composed-current-turn-rule-and-queue-consumption)：真实生成及完整保留队列消费 |
| 物理效果 | [物理规则](../../contracts/map3-battle01-continuous-scenario.md#selected-physical-rule-and-consumer-binding)：已到达源码操作数及实际效果归属 |
| HEAL 消耗/恢复/精灵 | [HEAL](../../contracts/map3-battle01-continuous-scenario.md#selected-heal-rule-and-consumer-binding)：选定逻辑机会及实际消费者 |
| 奖励/行动后/结果 | [奖励](../../contracts/map3-battle01-continuous-scenario.md#selected-reward-growth-and-outcome-consumer-binding)：源码奖励、成长、返回效果 |
| AI 决策/记忆/移动 | [AI](../../contracts/map3-battle01-continuous-scenario.md#selected-ai-rule-and-consumer-binding)：采用已接受种子机制的源码决策 |
| 野外服务抽取/效果门控 | [野外服务](../../contracts/map3-battle01-continuous-scenario.md#selected-field-service-rule-and-consumer-binding)：有界源码及实际服务用例 |
| 可变地图资源 | [地图交付](../../contracts/map3-battle01-continuous-scenario.md#composed-mutable-map-delivery)：普通输入谱系、工作布局与实际绘制 |
| W1 令牌/读取/服务 | [W1](../../contracts/map3-battle01-continuous-scenario.md#selected-w1-consumer-binding)：发生实例、服务及输入归属 |
| W2 接受读取/令牌返回 | [W2](../../contracts/map3-battle01-continuous-scenario.md#composed-w2-consumer-binding)：选定校验及源码续接 |
| 战斗演出命令/资源/等待/效果/结束 | [战斗演出](../../contracts/map3-battle01-continuous-scenario.md#selected-battle-scene-command-and-consumer-binding)：可见消费者、完成归属与有界终端组合 |
| 音频依赖消费者边 | [音频](../../contracts/map3-battle01-continuous-scenario.md#composed-reached-audio-consumer-binding)：已到达播放/替换/淡出/停止/恢复及释放 |

复用已接受连续获胜/返回路线及实际野外控制，加上[显示文字](../../contracts/map3-battle01-continuous-scenario.md#complete-reached-displayed-text-material-binding)、[移动](../../contracts/map3-battle01-continuous-scenario.md#complete-reached-field-motion-and-consumer-binding)、[操作](../../contracts/map3-battle01-continuous-scenario.md#complete-reached-operation-flow-binding)及[资源组](../../../../remake/docs/evidence/retained-comparisons.md#caller-and-reached-visual-resource-cohort)证明。
独立 [Option A 语音政策](../../../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy)使用其已接受受控揭示尾段证据；历史 C 未采样区间仍未知。

### 当前已接受里程碑

**已确认：**2026-10-05，main-gate 针对已接受基线 `acaaba60a49357b8842e152d5e490981fe19d0c1`，在既有默认键盘 A、现代确定性时钟、7C/8D/10A 边界下，独立接受完整当前私有 Map 3 → Battle 01 胜利 → 可用 5B 返回里程碑。产品范围不变。

验收组合了已执行连续获胜/返回路线及父级证据、上文每个已接受子项、未变化且已测试/当前的依赖、已接受 Option A 受控揭示尾段行为与显式偏差。独立紧凑检查核对共享源码、生产者、会话、发生实例、输入与效果关系，以及父/子覆盖和完整性。共同物理/奖励/行动顺序结果及选定奖励/音频战斗演出投影未发现冲突；全部八个保留父级声明及十三个先前缺失子项均已核算。
[父级完整性审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/606#issuecomment-5965838128)与[键盘范围审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/609#issuecomment-5976283697)仍是精确已接受依赖。[验证路线](../../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)标识保留支持检查与复现命令。
这是对完整当前要求映射的实质验收；仅凭 PASS 数量、源码结构、Issue 状态或自相等均不能建立该验收。没有已识别的范围内已知缺陷。

历史 A10/default/矩阵报告保留其原始 Unavailable 结论及 `milestonePass=false`；限定范围输出也保持 false。没有生成新的完整可执行现代报告或修正后的完整 A 轨迹。
已接受结果是对已有已执行比较及整段路线/父级证据的独立审阅组合。它不接受硬件 8C、全游戏一致性或公开发行，也不把任何历史产物改标 PASS。

历史种子锁存、HEAL 时序、溯源和审阅失败保持保留。终端内部完成仍为**推断**，延迟仍**未知**；历史缺失的行动顺序生成操作数及修正后完整 A 历史仍显式保留。这些限制不重开已接受组合。当前键盘范围下 C 仍为补充、B/D 排除。

### 条件式运行时问题

只有命名的缺失原版语义断言才能在 ADRs0014/0016 及 [ADR0015](../../../decisions/0015-original-reference-replay-and-h4-boundary.md) 下支持另行准入观察。
历史输入/RNG/状态及原版呈现缺口保留其科学限制；已接受当前要求映射没有已识别的缺失范围内断言。进一步证据工作需要具体新缺陷。PR #526 已接受有界 RA-12 Down/效果。
先复用已接受静态规则及有界观察。[研究呈现审计](../../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)记录 DisplayText 绕过；程序返回不证明未经垫片的交付。持久音乐要求已到达的开始/替换/停止，不虚构结束事件。本台账不授权原生启动，也不自动建立逐未知项观察队列。

### 首次实际 H4 比较及修正（PR #528 / #533）

本节属于历史，保留采用现代时钟前已完成的比较，不描述当前必需门禁，也不授权旧四变体范围。

**已确认的比较结果：**PR #528 已完成基线报告 **5,336 PASS / 6 FAIL / 40 Unavailable**；六个失败及报告保留为历史证据。PR #533 将相连选定 R1 产品输入修正为源码 NewGame 金币 60 和完整起始物品字，同时保留独立受控 R1 测试夹具的金币 0/四字节投影。
修正后的实际宿主运行完成归一化野外路线、全部 73 个已到达文字 ID 和自然首个行动者 2，然后在诊断性下个行动者差异处停止。报告 **5,340 PASS / 2 FAIL / 40 Unavailable**，`milestonePass=false`。准入金币为 60。
自然首次控制时，各己方角色实时物品数组与固定 NewGame 源码支持的起始槽位一致：Bowie `[199,0,127,127]`、Sarah `[213,0,0,127]`、Chester `[184,0,127,127]`。这些后续物品栏不能证明准入 `SourceLoadout`，后者仍为 null。
首回合顺序仍不同；首次诊断 STAY 后，原版下一行动者是 Bowie，实际为 Sarah。宿主退出码 2 与该比较互证。PR #526 战后扩展未重新绑定或到达。NPC 阶段与时序/RNG 映射仍未知。这次已完成比较不是 H4 验收。当前 Battle01/返回/端点及键盘义务按上文适用性与范围所有者验收，不抹去这些失败。

### 原版回放谱系与启动准入

旧回放路径对本里程碑保持 **DISABLED / NOT REQUIRED**。[谱系硬停止](../../../research/original-reference-replay-capability.md#current-lineage-hard-stop)保留已消费诊断身份、ordinal-2 已完成超时 FAIL、清理失败、缺失实际账本/回执，以及 ordinal-3/重试/重置启动禁令。
恢复/传输/硬件 API 不是 H4 前提。合成重建、任务改名或新运行器均不重置该历史。当前分段采集授权由 ADR0015 管理，不由被替代的累计上限管理；本文不消费或续授运行时权限。

## 证据矩阵

| 既有所有者 | 继续拥有的范围 |
| --- | --- |
| [准入](../../contracts/map3-controlled-admission.md)、[研究审计](../../../research/map3-battle01-audit.md#accepted-evidence-and-exact-field-mapping) | R1/R2/R2a/R2b/R2c/R2d/R3a–d/R4a 测试夹具身份、精确字段及有界/静态区分 |
| [Battle functions](../../contracts/battle-functions-control-flow.md)、[摄像机](../../contracts/map-camera-update-control-flow.md) | 既有 15 个直接 battle-function 关联及独立摄像机记录；连续场景工作不新增索引关联 |
| [战斗生命周期](../../contracts/battle-control-lifecycle.md)、[过场路由](../../contracts/battle-cutscene-routing.md) | 局部胜利/控制/程序规则；自然顺序由已接受观察拥有 |
| [采集](../../../research/map3-messenger-acceptance.md) | 精确源码/ROM/工具/父级身份、选定获胜记录、独立审阅、复现、既往失败及成本 |
| [连续合同](../contracts/map3-battle01-continuous-scenario.md) | 组合与比较语义；缺失原版值保持显式 |

Map3 聚合清单不是场景关联集合；不得批量关联其 26 行。研究证据不由重制输出改写。不恢复 M5 已退休消费者/聚合测试：保留 #431 已完成 `PrivateExplorationTests.PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` 失败及独立完整私有世界重跑，见[验证所有者](../../../../remake/docs/development-and-verification.md)。

## 公共与私有边界

ROM、存档、轨迹、采集、完整文字/图形/音频及生成导出保持私有、忽略、本地；不公开其绝对路径，不要求公共 CI 消费。公开报告只含有权使用/最少语义事实与安全溯源/结果。完整私有清单及资源绑定保留其已接受本地 7C 证明。纯文档验收使用范围/链接/双语语义检查；本切片不生成原版或重制输出。
