# Harpy Pond 接近至 Battle20 入场

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

在 Harpy Pond，Elric 感谢抵达的一行为他提供帮助。一个隐藏怪物打断对话，声称池塘用于捕获食物，并威胁吃掉 精灵。Peter 作出反应，摄像机转向怪物，对峙以解锁 Battle20 及请求转换结束。对峙标志尚未设置时，独立警告可使 Bowie 从南侧两个格子后退。随后的全新战斗以战斗员替换探索行动者；Elric 独立的中立池塘行动者不确立军团成员资格。

**已确认：**选定静态程序、对话及条件使用方确立下述顺序。**推断：**遭遇将战斗呈现为围绕受困 精灵 展开。**未知：**自然访问、实际调用方状态、服务完成、渲染表现、可用输入及持久性。角色陈述仍为归属于角色的台词。对峙和战斗入场均不救援或招募 Elric。

本指南接续 [Elven Village 离开至 Harpy Pond 入场](elven-village-departure-through-harpy-pond-entry.md)，使用已接受的[接近与 Battle20 入场研究](../../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md)。全新入场止于 BattleLoop 的 `@Start`，即 **ActivateEnemies 之前**。挂起入场止于 `@IndividualTurns_Loop` 之前。无关请求止于自己的继续边界。激活、回合（单次行动）、战术、结果、救援／招募、F751 通道、Fairy Cave/Creed、Battle21 及地形重建均不在范围内。Fnnn 表示标志 nnn；行和记录从零开始。下文等待次数与位置是命令输入，不是实测时间或观察移动。

[Battle20 结果与 Elric 招募](battle20-outcomes-and-elric-recruitment.md)另行接续选定后续及条件探索返回。

## 选择哪个池塘事件？

**已确认：**Map51 默认与 F520 替代配置共享相同区域回调及 init。F520 改变固定实体生成，不改变区域可用性。步行获准区域事件路径以目的地推导的瓦片坐标发布 event 6；传送检查路径是独立生产方。布局及更早移动检查决定运行哪条路径。仅区域表不证明自然路线或不可避免的屏障。

| 传入区域坐标 | 门槛与选定后果 |
| --- | --- |
| `(9,6)`、`(9,7)`、`(9,8)`、`(9,9)` | F470 已设置则返回。F470 未设置则运行对峙 `cs_5C312`，脚本返回后设置 F470。 |
| `(9,24)`、`(10,24)` | F470 已设置则返回。F470 未设置则运行独立警告 `cs_5C3AA`；不改变 F470。 |
| 其他坐标 | 仅返回的默认处理器。实体事件回调同样仅返回。 |

两个选定区域均不检查 F420、F520、成员资格、F13 或 HP。ProcessMapEvent 在分发前清除待处理请求并初始化 physical actor 0。区域包装器随后选择当前配置及首个匹配行。返回时关闭立绘／文本、等待一次 VInt，并等待 physical 0 到达目的地。最后等待不汇合每个行动者的动作。

前一指南负责池塘初始实体生成及提示。全新默认实体生成将 logical Elric 13 放在 `(6,6)`，使用 `eas_WhirlsInWater`；一名 Harpy 与两名 Soulsower 精灵以 logical 128–130 占据停放 `(63,63)` 位置。物理索引取决于传入随从。F520 已设置的全新配置省去这四个固定行动者，但可保留按标志选定的随从。己方行的查找已填入时可被跳过；后续随从标志可改变绑定。实体生成后改变标志本身不创建或移除行动者。

对峙使用这些**当前逻辑查找映射**，不重建它们。B 表示 FOLLOWER_B 31，其身份由当前实体生成提供；不会独立验证其为 Oddler。Peter 存在及 Elric 军团成员资格同样不是场景检查的前提。

## 对峙顺序

**已确认：**`cs_5C312` 含 29 个顶层记录及一个完整内联移动。在普通映射角色配置下，源码按下列顺序组织内容与动作。对话转述自 keys 2555–2564；精确键、修饰符和控制保留于[完整研究程序](../../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#complete-approach-program)。

| 记录 | 内容与动作顺序 |
| --- | --- |
| 0–5 | 设置文本游标 2555。以 Init 替换 Peter 动作并等待空闲，再对 B 同样处理。将 128 放在 `(7,16)`、129 放在 `(6,17)`、130 放在 `(8,17)`，均为 Up。这些写入显露／重定位既有行动者，不分配行动者。 |
| 6–10 | Elric 问一行是否来帮他，表示感谢（2555），并说否则怪物也许已经吃了他（2556）。怪物 128 抗议晚餐要逃跑（2557）。开始 B 的 Jump，再开始 Peter 的 Jump 并等待 Peter。 |
| 11–15 | 等待 5；开始 Peter 的 Right/Left/Right/Left 动作；等待 120。Peter 问谁在说话（2558）。请求摄像机目的地 `(3,12)` 并等待滚动。 |
| 16–20 | 怪物 128 解释食物陷阱（2559），并声称任何被困者都归它（2560）。Elric 问失踪的人（2561）；怪物答他们曾在其池塘中（2562）。以内联目的地／空闲等待将 128 向 Up 移动两个瓦片。 |
| 21–25 | 怪物 128 声称 Elric 是其晚餐（2563），面朝 Down，并问其他人是否有 番茄酱（2564）。开始 129 的 Jump，再开始 130 的 Jump 并等待 130。 |
| 26–28 | `setStoryFlag 20` 写入 F420。发布无滚动 Map51 `(0,0)`、Right 传送；结束脚本。仅在脚本返回后，调用方才设置 F470。 |

在映射一致且移动完成时，128 从 `(7,16)` 到 `(7,14)`，再面朝 Down。没有命令将 Elric 移出池塘、加入他、授予物品或写 F13、F520、F751。怪物关于受害者的声称与 Elric 关于逃脱的话，不独立证明事件已经发生。没有选择提示或同意分支。

文本 2555 和 2559 使用继续式 nextText，带 `{N}`/`{W2}`。其余行使用带 `{W1}` 的 single text；2556、2560、2561、2563 和 2564 还包含换行。怪物 128 的 `$C0` 修饰符选择镜像／右侧立绘位，Elric 与 Peter 使用 `$0`。立绘／台词属性来自当前映射地图精灵。继续文本留待后续关闭；single text 显示后关闭立绘／文本并请求 Sleep 10。这些控制不是额外选择，也不是对话时长测量。

### 等待与行动者写入确立什么

**已确认：**Init 替换选定动作，设置速度 32/32 和零加速度，启用阻挡／碰撞及自动朝向、转向 Up、禁用浸没、设置大小 24、刷新并空闲。因此接近会替换前一提示中 Peter 和 B 的跟随动作。Jump 暂时禁用自动朝向，请求相对竖直移动及两次等待 5，恢复自动朝向并空闲。Peter 的四朝向动作在每次朝向后刷新并等待 30。

等待修饰符轮询具名行动者精确的 `eas_Idle` 指针。等待 Peter 的 Jump 不独立等待 B；等待 130 不汇合 129。独立等待 120 不汇合 Peter 的朝向动作。后续写入可替换未完成动作。内联 Up 移动构造相对目的地移动，等待目的地相等，最后空闲。它绕过普通步行区域／传送生产方；穿过所列格子本身不提交另一事件。

位置和朝向命令有存活己方 HP 辅助程序，但 IDs 128–130 绕过它。因此这些选定 NPC 写入无 HP 门槛。动作与对话命令不会因 Peter 或 Elric HP 0 而抑制工作。它们不验证成员资格，也不安全拒绝缺失行动者：未分配逻辑查找可别名到 physical 0，255 对此使用方不是安全缺失哨兵值。因此传入 F520 已设置／F470 未设置状态可在行动者缺失或独立提供时选择同一脚本。源码不修复该状态，也不将它重新解释为战后场景。这些是静态反例，不是观察到的游玩失败。

摄像机命令禁用实体跟随并等待瓦片目的地；此场景在发布传送前不恢复摄像机实体。debug 模式配合 player-2 Start 可启用过场文本跳过，绕过显示／游标推进及 csWait 的 Sleep。End 在对话窗口索引非零时有条件等待滚动，清除摄像机速度并返回。动作、文本、摄像机及包装器服务仍须返回，后续工作才发生。此前写入在部分执行后保留；无回滚或所有行动者完成的保证。

## 南侧警告与独立出口

**已确认：**五记录的 `cs_5C3AA` 设置游标 2565，显示 Peter 催促 `{LEADER}` 救 精灵 的 single 行（`{N}`/`{W1}`），使 Bowie 点头，以内联空闲等待将 Bowie 向 Up 移动一个瓦片，再结束。不写标志、传送、模式或成员资格状态。F470 是唯一调用方门槛；未设置时，后续独立获准调用可重复它。

Bowie 点头无 HP 或同意检查。它停止动画，请求 Sleep 10、变换／上传地图精灵、请求 Sleep 20、刷新、请求 Sleep 10 并恢复动画。内联移动同样无 HP 门槛。在普通 Bowie 到 physical-0 映射及服务完成时，它使用更早事件 Init 速度，从 Bowie 当时位置向 Up 移动。传入 Y24 区域坐标不证明实际起始 Y24 或最终绝对 Y23。`{LEADER}` 在 UpdateForce 后解析自当前出战队伍首名成员，不是无条件字面 Bowie 姓名。

独立外部传送表选择 **Y25 处任意 X**，请求无滚动 Map67 `(57,26)`、Down。WarpIfSetAtPoint 写 WARP_SFX，不检查 F470。此返回可扫描 Map67 无关的 row 21/F421/F521 准入；本指南止于该继续。两个警告格不确立通用出口锁、全部可通行路线或到外部传送的自然移动。

直接 Map67 抵达请求 Map51 `(10,24)`、Up；更早求救请求 `(11,22)`、Up。初始配置／init 与随后获准步行区域是不同操作。抵达直接元组本身不触发 Y24 警告。F470 未设置的初始提示可在后续步行前移动角色。两个入场元组均不证明警告已运行。

## 从对峙请求到 Battle20

**已确认：**顺序保留三个独立改变：

1. 脚本的 `setStoryFlag 20` 加解锁基址 400 并设置 **F420**。
2. 其传送发布 pending event 1、no-scroll parameter 0、Map51/X0/Y0/Right。不加载地图、不放置战斗员、不立即离开脚本，也不写 WARP_SFX。
3. 脚本返回后区域回调设置 **F470**，随后包装器服务完成。

请求保持待处理时，探索绕过普通控制器安装。ProcessMapEvent 清除待处理状态；无滚动路径消费／清除当前传入 WARP_SFX，使 physical 0 空闲，展开至 MainLoop 并返回元组。MainLoop 在全新 ExplorationLoop 前先执行 SwitchMap，再执行 CheckBattle。Map51 无标志切换地图替换。

Map51 唯一战斗坐标行是 **row 20**，原点 `(0,0)`、大小 18x26、触发 255/255。CheckBattle 要求 F420，两个触发字节均按通配处理；不检查矩形包含、F470、成员资格或 HP。写入战斗区域后检查 F520。若 F520 已设置，则在**选择后清除 F420，但不取消此次准入**。MainLoop 随后传递 Map51/Battle20 至 BattleLoop。传送元组不是己方放置或战斗朝向指令。

| 传入状态或中断 | 条件后果 |
| --- | --- |
| 完成 F470 未设置的对峙、请求保留、F520 未设置 | 先设置 F420，再设置 F470。普通消费在全新池塘配置／init 前接纳 Battle20。未确立中间可用探索暂停。 |
| 相同完成但 F520 已设置 | 仍成功准入并清除 F420。全新战斗省去中立 Elric，但仍选择普通敌人行；不修复此前缺失的探索行动者。 |
| F470 已设置、F420 未设置 | 两个池塘区域均返回且无写入。不修复解锁。后续普通扫描可按传入 F520 实体生成及 F470 init 门槛进入全新探索。 |
| F420 独立设置、F470 任意值 | 普通 Map51 扫描可在任一初始元组或其他坐标直接接纳 Battle20，无需对峙。 |
| 扫描前 F420 被清除 | 仅保留传送不接纳 Battle20。若选择全新探索，治疗／配置／临时清除／init／控制器顺序归前一指南；保留的 F470 跳过提示。 |
| 待处理被替换，或脚本／包装器未返回 | 替换使用方或部分执行控制继续。此前标志写入不回滚。F420 及 pending 可已在 F470 前写入。止于无关准入／请求工作。 |

F470、F420 和 F520 是独立传入标志；源码不将其规范化为单一剧情阶段。成功战斗准入绕过普通全新探索，不在无滚动返回与 BattleLoop 之间插入它。

## 空 before/start 目标及全新或挂起入场

**已确认：**实际 before-battle 与 battle-start slot 20 均选择 `ms_Empty`，即 `$FFFF` 终止符，位于 `0x47A4E`。包装器共享介绍标志 **F470**，对峙也使用它。Before 检查但不设置；start 检查，未设置则在分发前设置 F470。

因此已完成接近会跳过两个选定脚本。传入直接 F420／F470 未设置的准入会执行空 before 目标，随后设置 F470，再执行空 start 目标。不重放对峙、不显露旧 NPC，也不运行池塘 init。空目标仍使用脚本引擎进入／结束服务；无场景记录不证明即时表现或响应输入。

BattleLoop 先将 PLAYER_TYPE 规范化为 0 并设置 F399，再检查挂起 F88。全新 F88 未设置入场清除经过秒数，保存传入地图／战斗，赋值当前 Map51/Battle20，安装基础 VInts 并调用 before。再恢复传入地图／战斗，清除区域标志 90–105，治疗符合条件的己方角色，初始化己方／敌人位置，清除 AI 内存，以全新 d0=0 选择调用 LoadBattle，再调用 start。止于其返回 `@Start`，即 **ActivateEnemies 之前**。

治疗将存活己方角色与不死 Peter 7 / Lemon 28 恢复至 HP/MP 上限，仅保留已有 STUN/POISON/CURSE 位并更新属性值。不复活其他死亡己方角色，也不加入任何人。之后的战斗治疗不能确立此前池塘场景具有有效行动者或 HP。模式清除发生在 BattleLoop，不发生在传送；不证明已保存木筏坐标写入。

F88 已设置入场则恢复已保存秒数、清除 F88 与 AI 内存、清除视野目标，调用 LoadBattle 并直接分支至 `@IndividualTurns_Loop`。它跳过传入地图／战斗赋值、before/start、全新治疗／名册初始化及新行动顺序生成；不在 LoadBattle 前显式清除 d0。已保存地图、战斗、区域、战斗员及行动状态必须已一致。传入 Battle20 不修复它们。止于各自回合（单次行动）之前；后续源码至 `@Start` 分支在此挂起路径上不可达。

## 战斗角色配置被重建

**已确认：**BattleSpriteset20 含十二个己方放置行、十二个敌人行，AI 区域／点为零。完整位置、行值和使用方归[选定名册研究](../../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#complete-selected-spriteset-and-roster-consumers)。本指南不维护第二份底层名册表。

己方行是当前已加入／出战列表的位置，不是十二名具名己方角色或新招募列表。初始化清除全部 30 名己方角色的 X，调用 UpdateForce 并消费该队伍及行 X/Y。死亡成员消耗队伍／循环槽位，但不推进放置行；超过容量的其余成员可离开出战队伍。不由此保证存活十二人军团、Peter、Oddler 或 Elric 成员资格。首个队伍条目在检查数量耗尽前读取，因此空／不一致列表不保证安全。行 HEALER1/NOTHING 与 NONE/15 操作数不安装新己方 AI 或物品。

十二个敌人行在战斗员槽位 128–139 选择**五名 Harpies、两名 Dark Madams、两名 High Priests、两名 Orcs 及一名 Arrow Launcher**。命令不同：Harpy rows 0/6/11 为 SWARM，rows 3/5/8/9/10 为 ATTACKER1，rows 1/4 为 ATTACKER3，rows 2/7 为 HEALER3。Rows 1/2/4/7 的主 FOLLOW_ENEMY 操作数编码敌人 rows 3/1/5/8；其他主命令为 NONE。全部为主区域 15、次 NONE/15、filler 0、STARTING 及行物品 NOTHING。这些是安装输入，不是观察到的跟随、攻击、治疗或激活；区域记录为零不闭合被排除的激活算法。

敌人初始化扫描全部 32 个槽位 128–159。十二个 STARTING 行复制所选敌人定义、填充 HP/MP、写行位置／类型／命令／触发、组合移动类型与指令集、处理行物品／激活位、应用共享难度并更新属性值。其余槽位初始走缺失／隐藏路径（HP 0、X=-1）。NOTHING 省去额外行物品，不一定省去定义物品栏、装备或法术。本指南不推出派生属性或有效性。实际随机升级列表排除 Battle20；F520 已设置准入不经该机制提升敌人类型。

LoadBattle 请求淡入淡出／瓦片集／VInt 工作，再清除实体数据／查找表，以重建战斗员中 X/Y 非负且符合条件者创建行动者，普通处理跳过标记为中立的敌人。普通行动者使用站立动作及 Down 朝向。探索物理索引和动作不带入这些交战槽位：旧探索 logical 128–130 映射不会变成战斗敌人类型。特别是战斗 129/130 为 Dark Madam/High Priest。

### Elric 的独立中立行动者

**已确认：**普通放置后，中立尾部检查 F500+CURRENT_BATTLE。**F520 未设置**时，在 `(6,6)` 声明新 ELRIC_BASE 行动者，初始 Down，使用 `eas_WhirlsInWater`。它写入战斗 lookup **159**、零最大／当前 HP 和状态、中立激活位及位置。这不同于探索 ally lookup 13 及军团成员资格。其动作启用浸没／双倍动画速度，以每次等待 10 循环刷新 Right/Up/Left/Down 朝向。初始 Down 不是固定最终朝向或观察到的溺水动画。

F520 已设置则跳过此尾部，包括 CheckBattle 清除 F420 但保留准入之后。不移除十二个敌人行。中立 HP 0 不采用普通死亡己方省略规则。该声明与接近均不加入 Elric。独立传入 Elric 成员资格可与普通己方放置及独立中立者共存；源码在此不协调两者。

LoadBattle 随后选择 d0 实体（全新入场提供 combatant 0）、初始化精灵、加载地图／地图精灵、安装基础 VInts、加载地形、请求音乐／淡入并返回 start 包装器。这些是具名服务依赖，不是地形重建或观察到的可玩回合（单次行动）。此激活前停止边界不确立后续名册快照、可玩回合（单次行动）或救援。

## 证据追踪与审阅边界

**已确认溯源：**已接受研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。其[复现所有者](../../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#reproduction-and-remaining-limits)保留精确源码路径、地址、完整程序、字面文本及紧接使用方。本综合不增加原始源码调查、载荷重新生成或运行时观察。

| 解释 | 合同与可执行所有者 | 本指南保留的边界 |
| --- | --- | --- |
| 选定区域及实体生成 | [地图探索](../../../contracts/world/map-exploration.md)、[配置](../../../contracts/world/map-setup-data.md)、[实体](../../../contracts/world/map-entity-data.md)；[区域分片](../../../../../../tests/fixtures/h2/map-events/zone-programs.json) sf2-map-events-static-v1:zone-programs、expected.zoneTargetPrograms orders 94–96；[init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json) sf2-map-init-static-v1 | 公开区域体绑定 5/4/1 操作。初始实体生成／init 仍归前一指南及已接受研究。两者均不证明自然可达。 |
| 完整对峙／警告及紧接命令语义 | [对话](../../../contracts/interface/dialogue-system.md)、[出战队伍／名册](../../../contracts/party/party-roster-state.md)；[地图脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) sf2-map-script-engine-static-v1、expected.dialogueCommandFacts、storyStateCommandFacts、transitionCommandFacts、entityPlacementCommandFacts、entityActionBridgeCommandFacts、entityGestureRelationshipMotionCommandFacts、mapCameraControlCommandFacts 和 scriptControlCommandFacts | 公开摘要不含完整场景程序。已接受研究保留的 programCorpus.programs cs_5C312/cs_5C3AA 提供全部 29/5 记录及两个内联移动；选定字面文本／HP／查找／动作连接归研究所有者。 |
| 待处理请求、普通准入及无全新探索 | [探索流程](../../../contracts/world/exploration-control-flow.md)、[入场路由](../../../contracts/world/map-entry-routing-state.md)；[请求写入](../../../../../../tests/fixtures/h2/map-event-request-state-static-v1.json) sf2-map-event-request-state-static-v1/eventRequestState、[消费](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) sf2-map-event-request-consumption-static-v1/eventRequestConsumption；[通用地图](../../../../../../tests/fixtures/h2/common-maps-static-v1.json) sf2-common-maps-static-v1/expected.mapFacts | 共享测试夹具守护辅助程序及上下文；已接受研究负责选定 Map51 row 20、无切换替换及实际有序生产方／使用方连接。 |
| Before/start 及全新／已保存生命周期 | [过场路由](../../../contracts/battle/battle-cutscene-routing.md)、[战斗生命周期](../../../contracts/battle/battle-control-lifecycle.md)；[过场测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) sf2-battle-cutscenes-static-v1/expected.cutsceneFacts.intro、[循环测试夹具](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json) sf2-battle-loop-static-v1/expected.lifecycleFacts | 精确 slot 20 空目标及分支特定生命周期是选定研究连接。[路由测试夹具](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json) sf2-battle-routing-data-static-v1 为 routeTargetsParsed=false，不是完整公开槽位至程序证明。 |
| 名册、升级排除及中立 159 | [遭遇定义](../../../contracts/battle/battle-encounter-definition.md)、[战斗生命周期](../../../contracts/battle/battle-control-lifecycle.md)；[精灵集测试夹具](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json) sf2-battle-spriteset-data-static-v1/expected.facts | rowContentTracked=false：聚合不守护 Battle20 完整行。已接受研究负责选定精灵集、中立／升级列表及实际使用方；不引入战术／控制结果。 |

schema／验证器路线为 [区域 schema](../../../../../../schemas/h2/map-events-zone-programs.schema.json)/[map_events_fixture.py](../../../../../../src/sf2tool/h2/map_events_fixture.py)、
[引擎 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)、
[init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../../../src/sf2tool/h2/map_init.py)、
[请求写入 schema](../../../../../../schemas/h2/map-event-request-state-static-fixture.schema.json)/[map_event_request_state.py](../../../../../../src/sf2tool/h2/map_event_request_state.py)、
[请求消费 schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)、
[地图 schema](../../../../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../../../../src/sf2tool/h2/maps.py)、
[过场 schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、
[循环 schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、
[路由 schema](../../../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../../../../src/sf2tool/h2/battle_routing_data.py)，以及
[精灵集 schema](../../../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)/[battle_spriteset_data.py](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)。现有 `uv run sf2 h2` 路线包括 map-events、map-script-engine、map-init、map-event-request-state、map-event-request-consumption、common-maps、battle-cutscenes、battle-loop、battle-routing-data 和 battle-spriteset-data。它们定位证据，不选择纯文字重新生成或一揽子套件。受控命令须在同一进程加载被忽略的 worktree 私有输入配置。

审阅使用上述已接受研究、共享合同、有边界公开测试夹具分区及 schema／使用方边界。完整程序、选定表行及字面文本经已接受研究及其回执使用，不在此独立复现。保留 [PR870 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868#issuecomment-6170807905)、[变基后交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868#issuecomment-6170840743)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/870#issuecomment-6170954167)，包括未改变比较、原始／变基后 CI、检查修正及链接的前序失败。内容专属静态连接不全局闭合共享合同未知项。

审阅状态包括独立 F470/F420/F520、直接准入、缺失／死亡／错误映射行动者、改变的随从、空／超容量队伍、F88 已保存状态、debug 文本跳过、部分服务及被覆盖待处理请求。它们约束此说明，不是已执行场景。**未知：**自然地形／访问／生产方选择、实际标志／HP／职业／成员资格／查找／模式、动作／文本／摄像机／淡入淡出服务完成、渲染场景、可用控制及持久性。它们按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md) 继续延后，不是自动运行时队列。本指南既不确立组合运行时路线，也不完成战役／设计 epic。
