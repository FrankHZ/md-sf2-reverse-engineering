# 开篇至 Battle 01：地点、人物与返回

- 范围：[Issue #690](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/690) 所限定的开篇、城堡/塔路线、Battle 01 与可用场地返回的中文内容指南。
- 证据基线：已接受仓库 `bda69671355cf8d60c15136b705fb842a76f6d16`；原版源码为
  `ShiningForceCentral/SF2DISASM` 的 `c834c652b6862bc5679fd7f69a38a7093206efc6`，USA ROM SHA-256 为
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
- 读者：沿这段剧情内容阅读的人，以及需要定位其已接受证据的作者。完整对话、布局、名册、素材与运行时记录仍由各自的私有/本地所有者保管。

开篇从住宅和学校区域出发，经信使交流、城堡访问、返回顾问处、塔守卫，连接到第一次选定遭遇。
遭遇以战后程序和恢复普通场地控制结束。**推断的内容解释：**这些阶段构成一段有用的阅读路线，
因为选定程序留下了下一阶段使用的条件。下列顺序以已接受路线为依据；动机、完整剧情梗概和所有可能的流程仍为**未知**。

涉及的系统见[玩法总览](../../gameplay-overview.md)，一般状态路由规则见[剧情推进](../../themes/story-progression.md)。
[连续场景合同](../../../contracts/milestones/map3-battle01-continuous-scenario.md)拥有选定路线与比较边界；
[就绪性](../../milestones/map3-battle01-readiness.md#当前已接受里程碑)拥有已接受的私有重制里程碑。
本指南组织这些内容，不另建战役模型，也不改变它们的验收。

## 如何阅读证据

源码地图编号是身份，不是章节编号。本指南按选定内容流程阅读，包括重访 Map 19 和 Map 20。
`MAP_ANCIENT_TOWER_ENTRANCE`、`INSIDE_ANCIENT_TOWER` 等名称保留为源码标签；
措辞不同不能成为重命名地点或推断未观察过渡的理由。

| 证据种类 | 在此确立的内容 | 限制 |
| --- | --- | --- |
| **已确认静态** | 固定源码/H1/ROM 选择、程序结构、文本引用、标志谓词、实体身份和合法路线拓扑。 | 结构不确立自然执行、对话交付或可见完成。 |
| **已确认受控运行时** | R1 准入；早期处理器/路线接缝；R2d 的显式 Map 21 桥接和设定种子后的战斗就绪结果。 | 测试台返回、状态归一化和桥接状态仍是控制条件。 |
| **已确认有界原版连续性** | 已接受采集从受控 R1 经选定剧情路线、自然遭遇、获胜行动到稳定返回；另有普通输入扩展。 | 这是由存档状态衔接的原版连续性，不是不中断的墙钟时间，也不是自然标题/New/load 输入。 |
| **已确认已接受重制组合** | 当前私有默认键盘 A 里程碑，包括实际到达的内容消费者、完成与输入就绪。 | 适用现代时钟/显式偏离；不证明硬件相等、全游戏一致性或分发权。 |

| 地图身份 | 有依据的源码标签或角色 | 选定内容阶段及证据 |
| --- | --- | --- |
| 3 / `map03` | 住宅/学校开篇角色；本指南不赋予规范城镇名。 | 受控默认准入，随后是已观察的开篇与信使路线。 |
| 19 / `map19` | `MAP_GRANSEAL_CASTLE_2F` 是保留的王室返回传送目标标签。 | 静态城堡入口、王室/塔连接和顾问阶段；选定执行属于后续原版谱系。 |
| 20 / `map20` | 选定图中的王室入口与通塔角色。 | 有条件的王室初始化、返回 Map 19 与后续通塔楼梯连接；不推断规范房间名。 |
| 21 / `map21` | 塔守卫交互角色。 | 静态守卫谓词/解锁程序及后续选定原版执行。 |
| 40 / `map40` | 选定出口之间的通道角色。 | 静态选定空实体列表、受约束传送终点及后续穿行。 |
| 57 / `map57` | `MAP_ANCIENT_TOWER_ENTRANCE`；战斗标签 `INSIDE_ANCIENT_TOWER` 是另一身份。 | 静态遭遇定义、已观察战斗/战后程序与场地返回终点。 |
下文“必需”表示在已接受的选定路线或已观察前缀中必需，并注明适用范围。
它不表示每次原版流程都必须使用该交互。“额外”内容表示属于选定地点的静态交互；
除非所有者证明，其原版可选性与可达结果仍为**未知**。条件分支不一定是玩家选择，
立绘/精灵查找也不足以证明谁在发言。

## 1. Map 3：离开住宅并与学校实体相遇

**已确认受控准入：**R1 到达默认 `ms_map3` 配置及其第一个原版 `WaitForEvent`，
MainLoop 在 Map 3 `(56,3)` 交接。该受控快照中仅己方角色 0 已加入并出战。
输出的原始玩家坐标与这些交接参数是不同投影；时间被显式归一化，状态被省略，
四个存储物品字节不是四个完整物品槽。精确状态见[准入起点](../../../../../research/chapters/grans-opening/map3-admitted-start.md#admitted-snapshot-and-setup-join)，
不要用后续出战队伍或重制状态填补空白。

默认配置将实体、实体事件、区域（事件）、描述、物品事件和初始化分给不同所有者。
替代标志按源码顺序 `609 → 506 → 543` 扫描；最后设置的变体胜出。
R1 观察到受检查的标志未设置，默认初始化返回而未请求程序。
这些替代项是**已确认静态**内容，不是 R1 证明的额外开篇阶段。

**已确认有界开篇路线：**普通原版输入离开住宅，打开住宅与学校的门，与 Sarah 交互，
使用两处学校楼梯传送，到达 Zone 7 的两个阶段，与实体 142 交互并到达信使程序入口。
Map 3 内部传送连接 `(54,3) → (3,3)` 和 `(46,7) ↔ (59,12)`；它们仍是 Map 3 内的过渡。
[自然路线所有者](../../../../../research/chapters/grans-opening/map3-battle01-natural-route.md#reached-opening-route)停在 `cs_5149A` 主体之前。
后续连续性来自采集所有者，不是扩大后的 R2 测试夹具。

重要的对话与实体连接如下：

| 触发与源码身份 | 对话/程序连接 | 状态或继续路径；证据边界 |
| --- | --- | --- |
| 住宅出口路线，`cs_5145C` 中的实体操作数 128 | 游标 510 产生 510/511，然后游标 483 产生 483。 | 文本前后有移动/初始化等待。[开篇对话证据](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#opening-dialogue-return-and-subsequent-actions)拥有三条命令的结构与已到达返回；仅凭 ID 不证明无垫片的确认。 |
| `ALLY_SARAH` / 行动者 1，`Map3_EntityEvent0` | 选定的 F603/F602/F256 均未设置分支使用 512、480、481 和 `cs_513D6`。 | 移动程序之后才写入 F256。其他标志分支使用不同文本/程序；[额外交互测试夹具](../../../../../../tests/fixtures/h2/map3-optional-interactions-static-v1.json)保留它们。 |
| `Map3_ZoneEvent7`，F602 之前与之后 | 首次介绍使用 513；后续 F602 已设置/F603 未设置/F260 未设置阶段使用 514–516 和 `cs_5148C`。 | 后续定位先于 F260。[源码区域绑定](../../../contracts/interface/dialogue-system.md#源码区域调用方直至返回)区分调用方、文本、程序与包装器返回。 |
| 实体别名 142，`Map3_EntityEvent15` | 文本 500/501，带 F261 分支及 F261/F602 写入。 | 选定开篇别名解析到物理槽 17；[实体 142 证据](../../../../../research/chapters/grans-opening/map3-entity142-interactable-reference.md)拥有该活跃身份，不保证重载后永久占据同一槽。楼梯重载也到达 `cs_513A0`。 |

Sarah 的具名己方角色身份不同于数字 NPC 别名。“Astral 介绍”是已接受所有者对 Zone 7 内容的描述；
它不能仅凭精灵识别每次实体 142 的使用或发言者。
[对话合同](../../../contracts/interface/dialogue-system.md#已确认的静态合同)分别保留游标、打包修饰符/实体字、立绘辅助与实体对话属性查找。

### Map 3 的额外交互

**已确认静态：**默认表面有 19 个实体定义、包含默认项的 17 条实体事件路由、17 条区域描述和两处物品放置。
该清单仅将 `Map3_EntityEvent0` 和 `Map3_EntityEvent15` 分类为 `mandatory-observed-opening`。
所有其他路由的相关性为**未知**；“optional interactions”这一名称不能决定它。

下列例子说明该地点还有哪些内容，不把它们列为本路线的要求。
完整结构映射仍在链接的测试夹具中，精确实体/区域/物品位置仍属私有，不在这里复制。

| 额外触发 | 源码已确认内容 | 保留的问题 |
| --- | --- | --- |
| `ALLY_CHESTER`，`Map3_EntityEvent1` | F603/F602 分支引用 482、503/504 或 507。 | 单独交互时哪个分支自然可达，以及玩家看到什么。 |
| 别名 128，`Map3_EntityEvent2` | F602 选择 483/505 文本替代项。 | 此处理器不能与住宅出口脚本对 483 的引用互换。 |
| 别名 138/139，`Map3_EntityEvent10/11` | 依赖 F604 的引用包括 497 和 508/509。 | 门的移动不证明后续对话或其可选性。 |
| 别名 141，`Map3_EntityEvent13` | 直接调用 `j_ChurchMenu → ChurchMenu`。 | 静态调用不确立本段中的普通准入、选择、交易与调用方返回。 |
| 其他默认 NPC 路由、共享文本 494 目标及默认 RTS | 源码拥有的对话引用、目标共享与空回退。 | 精灵外观不赋予角色名；空回退不是凭空补出的对话。 |
| 区域描述及宝箱/其他物品 | 已有描述与物品 include 所有者；`Map03s7_ChestItems` 和 `Map03s8_OtherItems` 不同。 | 可达性、拾取和路线相关性在此仍为**未知**。 |

选择与物品规则见 [Map 3 额外交互](../../../../../research/chapters/grans-opening/map3-optional-interactions.md#confirmed-static-contract)
和[地图探索](../../../contracts/world/map-exploration.md#区域事件与物品)。选定参考路线排除额外交互与 FieldMenu；
这不从原版地点内容中排除它们，也不把它们纳入重制里程碑。

## 2. 接受信使请求：出战队伍与城堡路线相连

**已确认有界运行时：**Zone 8 在 F602 已设置、F603 未设置时到达 `cs_5149A`。
已接受的提示路径选择 F89 已设置分支 `cs_51614`。
十七条已到达地图脚本文本命令使用 517–531 和 535–536；加入处理器还发出文本 447。
原始发言者操作数包括 142、143、具名己方角色和 Sarah 的打包形式 `0xC001`。
后者组合修饰符/实体信息；不能显示为另一个角色 ID。

**推断的内容概要：**交流协调学校群体与信使，随后已接受回答引起出战队伍/随从变化。
这描述已观察命令的后果，不重建角色动机或私有脚本文字。

观察顺序为 F600、F66、`join 128`、Sarah (1) 与 Chester (2) 加入及出战队伍更新，
然后建立距离 2 的 Sarah→Bowie、Chester→Sarah 跟随连接。
`join 128` 选择器是源码命令操作数，不是第四名持久出战队伍成员。
只有整个程序返回之后，`Map3_ZoneEvent8` 才提交 F603。
随从就绪终点为 Map 3 `(43,10)`、Down，所跟踪的脚本/文本/提示/关闭消费者均已返回。
[信使所有者](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#confirmed-source-and-runtime-facts)
和 [R2a 测试夹具](../../../../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json)拥有这些事实。

**已确认静态替代项；运行时未知：**拒绝/再次提示分支引用 532–533。
它不是已观察的另一流程，不能拼入已接受的 Yes 谱系。
[出战队伍合同](../../../contracts/party/party-roster-state.md)区分成员资格、出战队伍与随从；
本路线及其加入都不证明一般容量或持久存档行为。

## 3. 城堡访问：门、王室区域与返回顾问处

下一连接从学校返回城堡门。**已确认静态且在已接受原版路线中另有观察：**
`Map3_ZoneEvent4` 调用 `cs_51652`；其六个文本引用 537–542 位于别名 138/139 的守卫行动组之间。
调用方在脚本返回**之后**设置 F604。门程序本身不拥有该标志写入，且仅等待实体 139 的行动组。
这区分了对话、运动完成与门后使用的条件。

选定 Map 3 北出口使用 X 通配/y=1 传送，选定终点 `(28,1)` 通向 Map 19 `(26,30)`、Up。
通配谓词不证明该行每个瓦片都能触发：几何、区域、碰撞与事件标记仍然适用。
已接受原版谱系也观察到随后移动至 `(26,29)`；较早的 Map 19 移动验收接缝本身停在该位移之前。

**已确认静态内容：**Map 19 的选定默认实体集合包含 13 条记录及多个固定/行走/循环占位域。
Alias140 依阶段而变：初始初始化隐藏它，王室返回阶段使其出现，已接受顾问程序将其从选定路线中移除。
Map 19 初始化守卫在 `cs_53104` 前后检查 F605 和 F608；
没有选定初始化/程序状态时，不要从实体表推断其存在。

王室访问跨越 Map 19 `(23,3) → Map 20 (23,37)`、Down。
选定王室入口的初始化运行 `cs_53996`：游标 2176 属于该程序；源码效果把 Bowie 放在 `(23,39)`，
移动别名 131、隐藏别名 130，随后调用方提交 F605。
这些是程序结构内容与选定原版王室返回阶段，不证明每次进入 Map 20 都运行该场景。
[城堡静态所有者](../../../../../research/chapters/grans-opening/map3-castle-battle-unlock.md#confirmed-static-derivation)约束入口选择器、程序与程序后占位。

返回使用 Map 20 `(23,37) → Map 19 (23,3)`。**已确认静态：**目标朝向字段为 LEFT，
来自 `warpFacing LEFT`。这是传送注记，不是目标初始化或后续运动后朝向的无条件结论。
同一 Map 19/20 会在通塔路线中重访；仅凭地图身份不能区分这些内容阶段。

顾问交互中，在选定玩家位置 `(16,6)`，Map 19 别名 140 / `Map19_EntityEvent12`
将未设置 F607 的情况路由到 `cs_52F0C`、游标 575 和 Yes/No。
F89 已设置选择 `cs_52F40`、游标 578；其源码效果移走别名 140 并设置 F608。
外层实体处理器在脚本返回后设置 F607。**已确认静态条件：**F607 已设置则改走 `cs_52F24`；
因此 F607 和 F608 表达不同效果，不能合并为“接受任务”。
已接受原版路线到达王室/守卫阶段及 F607/F608；
未观察的提示分支和重复交互仍保留**未知**运行时后果。

其他城堡对话属于各自局部事件表。例如 Map 19 event 0 在 F256 条件下引用 543/544，
Map 20 event 2 在 F256 条件下引用 570/571。
这些是[实体程序语料](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)中的**已确认静态**替代项，
不是额外必需步骤。实体 ID 和发言者局限于选定配置；Map 19 别名 140 不能等同于 Map 3 别名 140。
更广泛的[地图事件对话分析](../../../../../research/world/map-event-dialogue-state.md#runtime-boundary)
证明状态/引用结构，不证明自然显示的台词顺序或角色身份。

## 4. 塔守卫与通道：解锁遭遇

通塔路径选择相同地图的另一部分：Map 19 `(6,2) → Map 20 (6,37)`、Right，
然后 Map 20 `(3,36) → Map 21 (3,16)`、Right。
这些选定楼梯/传送连接来自 R2 谓词和 R2b 合法图；后续原版采集证明选定连续城堡/塔执行。
图的最短路径是一条可复现合法路线，不是要求玩家恰好使用其 110 次逻辑输入的规则。

**已确认静态条件：**默认 Map 21 实体表把别名 128 指向 `Map21_EntityEvent0`，
其物理定义位于 `s2_entityevents_506.asm`。文件名不要求选择标志 506。
实际 F608/F256 条件选择如下：

| 入口 F608 / F256 | 内容与继续路径，假设所调用服务返回 |
| --- | --- |
| 未设置 / 未设置 | 文本 568，设置 F256，文本 569；无解锁程序。 |
| 未设置 / 已设置 | 文本 569；无解锁程序或标志写入。 |
| 已设置 / 已设置 | 文本 579；无解锁程序或标志写入。 |
| 已设置 / 未设置 | 文本 579、`cs_53EF4`，然后在整个脚本返回后写入 F256。 |

最后一个分支中，`cs_53EF4` 先等待别名 128 向右移动，再应用 `setFacing 135,DOWN`，
然后由 `setStoryFlag 1` 设置 F401。守卫从 `(5,16)` 移到 `(6,16)`；
选定玩家终点为 `(5,15)`、Down。**已确认有条件静态：**缺少逻辑别名 135 本身不会拒绝原版命令：
在默认的查找已初始化为零的条件下，它解析到物理槽 0。
这不允许跳过朝向命令、其返回或之前的行动等待。
[查找/继续路径所有者](../../../../../research/chapters/grans-opening/map3-castle-battle-unlock.md#flag-predicates-and-return-boundaries)拥有四个分支及该限制。
F256 在局部处理器中复用，不是一个通用战役阶段名。

从 Map 21 经选定出口 `(9,1) → Map 40 (4,30)`、Up，到达选定实体列表为空的通道配置。
Map 40 的 X 通配/y=12 传送通向 Map 57 `(8,18)`、Up。
[准入所有者](../../../../../research/chapters/grans-opening/map3-battle01-admission.md#confirmed-static-findings)仅识别两处可达传送事件终点
`(14,12)` 和 `(15,12)`，并选择前者；该行其他普通可行走单元不是传送终点。
这段 46 次输入的扩展是一条选定合法路线，不证明所有有效路线，也不是 Map 40 中已观察的对话场景。

## 5. Map 57：Battle 01 准入及其行动者

**已确认静态遭遇身份：**源码战斗索引 **1** 是本指南选定的“Battle 01”。
它是从零编号的战斗地图表第 1 行，即第二条记录；不声称原版 UI 显示“01”。
地图为 57，源码 `MAP_ANCIENT_TOWER_ENTRANCE`，战斗标签则是 `INSIDE_ANCIENT_TOWER`。
选定区域为 `(0,0,16,20)`，触发坐标为通配。`CheckBattle` 要求该行的 F401 解锁门禁。
匹配后写入战斗矩形，再检查 +100 完成标志 F501。
选定全新进入状态的 F501 未设置；该场景条件不是共享函数的额外拒绝规则。
若 F501 已设置，同次调用清除 F401，仍返回索引 1；后续调用才看到已改变的解锁状态。
[通用地图路由](../../../../../research/world/common-maps.md#confirmed-map-routing)和
[已接受选择器分析](../../../../../research/chapters/grans-opening/battle01-to-battle02-content-route.md#battle-02-data-and-admission-semantics)
拥有共享源码语义。战斗身份、地图身份和源码名称保持不同。

新战斗的 F88 未设置分支中，战前程序 `bbcs_01` 先于加载与战斗开始路由。
在 `bbcs_01` 内，游标 2292 先于 `loadMapFadeIn`，随后是 `loadMapEntities ce_49694`，
再是后续文本命令，包括实体操作数 135。因此场景实体加载先于这些文本命令；
脚本 `loadMapFadeIn` 不是另一次玩家传送。
开始表第 1 行目标为 `ms_Empty`，在该空程序之前检查/设置 F451。
遭遇会清除 F90–F105。`LoadBattle` 确立 Map 57，定位并加载战斗实体/精灵，
加载 `BattleTerrain01`，选择音乐并完成加载/淡入淡出序列。
详细程序/调用方顺序仍见[准入测试夹具](../../../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json)。

**已确认静态及有界初始化参战：**`BattleSpriteset01` 提供三个己方角色槽、六名敌人、三个 AI 区域且无 AI 点。
敌人引用 `GIZMO`；全部九个条目均为 `STARTING`。
选定运行时行动者为己方角色 0/1/2 (Bowie/Sarah/Chester) 和敌人 128–133。
这些敌方战斗员 ID 不是数字相同的 Map 3 NPC 别名。
初始位置、地形、激活多边形与敌人定义从
[Battle 01 放置](../../../../../research/chapters/grans-opening/battle01-placement.md#confirmed-placement-facts)、
[战斗精灵集](../../../../../research/battle/battle-spriteset-data.md)和[遭遇定义](../../../contracts/battle/battle-encounter-definition.md)链接，
不复制成名册。

激活、区域过场与生成工作先于行动顺序生成。Battle 01 没有区域过场行。
已放置/存活过滤、随机化敏捷和降序排序决定行动顺序；
起始精灵集中存在某个条目，与激活及后续能否行动不同。

**已确认受控 R2d 接缝：**显式非自然桥接设定 Map 21 终点和 RNG `0x1234`，
穿过原版扩展，在战斗输入读取之前的行动者 **1** 处停止。
**已确认选定自然采集：**已接受、由存档状态衔接的路线则到达首位行动者 **2**。
两者都不把固定行动者或种子变成合法性规则。R2d 测试夹具从未证明被省略的城堡段或选定自然顺序。
[原版前沿](../../milestones/map3-battle01-readiness.md#已接受原版前沿)定位后续结果。

## 6. 获胜战斗与战后程序

**已确认有界原版连续性：**选定获胜谱系使用移动、STAY、物理攻击、HEAL 1 和 Medical Herb，
到达玩家/AI 行动、演出、已消费资源及 HP 效果、死亡、奖励和回合（单次行动）交接。
胜利发生在 回合 14/回合（单次行动）103；这是参考观察，不是遭遇规则。
失败尝试及其已完成失败仍由采集所有者保留，不插入这条成功输入流。
一般规则连接见[战术战斗循环](../../themes/tactical-battle-loop.md)。

**已确认静态结果规则：**Battle 01 使用普通的已放置/存活敌人计数胜利条件。
其首领存在表条目不是另一条击杀首领胜利规则。
获胜返回时，`BattleLoop_Victory` 路由出战队伍恢复，并经 `ExecuteAfterBattleCutscene` 到 `abcs_battle01`，
然后清除 F401、设置 F501，返回 D4=1。战后加入行是零；此前信使加入不能归到这一行。

战后程序必须在场地返回前完成。游标 2305、战斗属性重置、Map 57 场景替换、
`ce_497F6` 实体加载、实体 129 文本命令、精灵变化、白色调色板效果、移动与移除构成源码拥有的序列。
临时实体 128/129 是场景身份，不能凭数字推断为幸存敌方战斗员。
**推断的内容概要：**场景改变遭遇后的状况，然后在探索恢复前移除场景主体。
谁在发言、剧情确切解释是什么，不能仅由数字操作数或 `MAPSPRITE_ASTRAL`/`MAPSPRITE_MIST_DEMON` 补出。

[胜利返回静态测试夹具](../../../../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json)
包含 **80 个源码操作**，包括嵌入的行动/数据记录。
已接受原版最后一段记录 **67 对已到达操作入口/返回**，然后是共享尾部/外层返回、
标志更新、BattleLoop 返回、SwitchMap 和 ExplorationLoop。
这是不同分母，不是缺失十七条运行时命令。
[最终采集](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)拥有该观察；
[静态胜利所有者](../../../../../research/chapters/grans-opening/map3-battle01-victory-return.md#static-boundary)
停在尚未进入的 ExplorationLoop 调用处，本身不能确立可用返回。

## 7. 返回场地并结束本段

**已确认有界原版终点：**Map 57 选择 `ms_Void`；玩家 `(5,12)`、Down，在两个已完成中性帧内保持稳定，
战斗哨兵值 255，F401 未设置、F501 已设置，名册为 `[0,1,2]`。
不存在阻塞的战斗/脚本/模态/传送/消费者；仅有通用窗口字节 1 不表示阻塞对话。
[精确终点](../../../contracts/milestones/map3-battle01-continuous-scenario.md#精确已观测端点)
拥有金币、等级、完整槽位、状态、RNG/时间及该次获胜历史的其余内容。

已接受 PR #504 终点不可恢复执行，并含中性轮询。
另一次已接受的 PR #526 兼容采集观察到一次普通 Down 输入及稳定到 `(5,13)` 的位移。
它闭合具名输入/效果边界，不改变第一个终点，也不证明战役接下来的内容。
本指南止于 Battle 01 返回。接着读
[Battle 01 返回至 Battle 02 准入](battle01-return-through-battle02.md)，了解城堡驱魔/委任、Jaha 事件及有条件的下一遭遇。
该指南使用[已接受静态路线调查](../../../../../research/chapters/grans-opening/battle01-to-battle02-content-route.md)，
保留调用方/配置条件与合法行走/自然连续性的未知；不为本终点或已接受重制里程碑增加已观察扩展。

**已确认已接受重制边界：**当前组合接受连续私有 Map 3 → Battle 01 胜利 → 可用 5B 返回，
在默认键盘 A 和现代确定性时钟下到达实际内容使用、完成、确认与输入就绪。
C 是补充；B/D 被排除。原版行动者/顺序、103 次行动历史、金币 420 和原始帧值是原版观察，
不是无条件现代结果。通过[现代适用性合同](../../../contracts/milestones/map3-battle01-continuous-scenario.md#现代时钟适用性)
保留匹配状态规则及必需内容/因果效果。

历史 A10/default/matrix 报告仍为 Unavailable 且 `milestonePass=false`，各限定里程碑字段也一样。
本指南不产生新的完整可执行报告或修正后的整条 A 轨迹。
[组合所有者](../../../../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)
保留历史种子、HEAL、溯源与审查失败。终点内部完成仍为**推断**，其延迟为**未知**。
这些限制不重启已接受里程碑，也不形成自动模拟器队列。
可用返回也不确立用户存档/读档、全游戏还原度、硬件输出相等或公共内容权利。

## 源码到内容矩阵与证据审查

综合前审查检查了这些已接受载荷及限制其主张的代码。
下列命令标识已有复现所有者；没有为了刷新本说明而重跑。
本工作不新增测试夹具、提取、捕获或观察。

| 内容连接 | 精确已接受载荷 / 测试夹具 ID 与验证器 | 审查结论及剩余边界 |
| --- | --- | --- |
| Map 3 准入/默认配置 | [R1](../../../../../../tests/fixtures/h3/map3-admitted-start-v1.json)，`sf2-map3-admitted-start-runtime-v1`：`static.map3`、`harness`、`expectedObservation`；[验证器](../../../../../../src/sf2tool/h3/map3_admitted_start.py)，`sf2 h3 map3-admitted-start` | 受控菜单/服务返回与时间归一化保持显式；状态/完整槽位来自后续已接受读回，不是虚构的 R1 字段。 |
| 有序配置、区域、传送与内容 | [地图数据](../../../../../../tests/fixtures/h2/map-data-static-v1.json)，`sf2-map-data-static-v1`；[地图内容](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[地图数据验证器](../../../../../../src/sf2tool/h2/map_data.py)、[内容验证器](../../../../../../src/sf2tool/h2/map_content.py)，`sf2 h2 map-data` / `sf2 h2 map-content` | 最后设置标志胜出的配置规则，与首匹配事件/传送规则保持不同。精确私有内容表不在此公开。[清单](../../../../../research/world/map-data-inventory.md#setup-selection-and-rom-parity)与[内容](../../../../../research/world/map-content.md#source-form-sections)拥有它们。 |
| 开篇移动与程序入口 | [R2](../../../../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json)，`sf2-map3-battle01-natural-route-runtime-v1`：`static.route.warps`、`static.route.runtimeOpening`、`expectedObservation`；[验证器](../../../../../../src/sf2tool/h3/map3_battle01_natural_route.py)，`sf2 h3 map3-battle01-natural-route` | 名为“Battle 01”的验证轨道只到达信使入口；不能从名称制造主体或城堡观察。 |
| Map 3 额外默认内容 | [额外交互测试夹具](../../../../../../tests/fixtures/h2/map3-optional-interactions-static-v1.json)，`sf2-map3-optional-interactions-static-v1`：`pointerSetup`、`entityEventRoutes`、聚合项；[验证器](../../../../../../src/sf2tool/h2/map3_optional_interactions.py)，`sf2 h2 map3-optional-interactions` | 精确 schema 加上重新解析相等检查约束结构/顺序；15 条路由及全部描述/物品的路线相关性仍为未知。 |
| 信使选择、加入与随从 | [R2a](../../../../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json)，`sf2-map3-messenger-acceptance-runtime-v1`：`static.stream`、`static.text`、`expectedObservation`；[验证器](../../../../../../src/sf2tool/h3/map3_messenger_acceptance.py)，`sf2 h3 map3-messenger-acceptance` | 发言者操作数保留打包/源码兼容形式；已接受 Yes 与整体返回，不同于静态拒绝及已渲染/确认文本。 |
| 城堡/王室/守卫拓扑 | [R2b](../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json)，`sf2-map3-castle-battle-unlock-static-v1`：`static.programs`、`static.occupancy`、`static.routeGraph`、`static.warps`；[验证器](../../../../../../src/sf2tool/h2/map3_castle_battle_unlock.py)，`sf2 h2 map3-castle-battle-unlock` | 程序 `operations` 数组是受约束结构选择，不是完整逐字记录。F604/F605/F607/F256 调用方写入与 F608/F401 程序效果保持不同。自然可达性使用后续采集。 |
| 城堡分支与对话身份 | [实体程序](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)，`sf2-map-events-static-v1` 的一部分：`Map19_EntityEvent12`、`Map21_EntityEvent0` 及其他局部目标；[对话状态测试夹具](../../../../../../tests/fixtures/h2/map-event-dialogue-state-static-v1.json)，`sf2-map-event-dialogue-state-static-v1`；[对话状态验证器](../../../../../../src/sf2tool/h2/map_event_dialogue_state.py)，`sf2 h2 map-event-dialogue-state` | 精确目标/分支/操作数顺序是静态证据。到达立绘/SFX 定义不证明自然查找输入/结果或可见发言者；通用对话/名册矩阵不提供这些连接。 |
| 遭遇与首次控制接缝 | [准入](../../../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json)，`sf2-map3-battle01-admission-static-v1`：`static.admission`、`static.cutscenes`、`static.loadAndTurnOrder`；[R2d](../../../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json)，`sf2-map3-battle01-player-ready-runtime-v1`：`static.bridge`、`expectedObservation`；[H2](../../../../../../src/sf2tool/h2/map3_battle01_admission.py) / [H3](../../../../../../src/sf2tool/h3/map3_battle01_player_ready.py)，`sf2 h2 map3-battle01-admission` / `sf2 h3 map3-battle01-player-ready` | 源码/ROM 与精确测试夹具/观察比较不消除已声明桥接。设定种子后的行动者 1、调试测试顺序和自然行动者 2 各自保留溯源。 |
| 胜利程序与返回边 | [R4a](../../../../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json)，`sf2-map3-battle01-victory-return-static-v1`：`victoryReturnSpine`；[验证器](../../../../../../src/sf2tool/h2/map3_battle01_victory_return.py)，`sf2 h2 map3-battle01-victory-return` | 验证器闭合保留所有者身份和精确源码/H1/ROM 结构；自然执行及稳定/输入就绪返回使用后续原版结果。 |
| 原版连续性与可用输入 | [最终谱系](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)，采集源码 `9c3ea03ac5f5b467ee744f1ac624870da2408443`；[Down 扩展](../../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#native-post-victory-ordinary-input-result-issue-515)；[连续合同](../../../contracts/milestones/map3-battle01-continuous-scenario.md#精确已观测端点) | 已有私有记录/18 对已验证父级关系及其只读审计方法确立有界结果。早期所有者队列是保留的历史边界，不是新增的里程碑缺失主张。 |
| 重制交付 | [已接受组合](../../../../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)与[当前就绪性](../../milestones/map3-battle01-readiness.md#当前已接受里程碑) | 使用已接受组合，不重标历史报告、不重启比较，也不把重制结果当作原版证据。 |

对抗性检查保留边界差异：R1 交接与原始坐标/不完整槽位；源码图与后续原版连续性；
R2d 设定种子与自然首位行动者；脚本返回与调用方标志写入；文本 ID 与无垫片交付；
全新进入假设与共享选择器完成分支；80 个静态操作与 67 对已到达操作；
独立组合与保留的 false 里程碑字段。每项都采用上面具名的更严格边界。
更广剧情含义、未观察替代项、发言者/呈现连接、持久性与下一遭遇仍为**未知**，
不自动派发研究或实现任务。
