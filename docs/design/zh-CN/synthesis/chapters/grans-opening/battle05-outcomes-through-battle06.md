# Battle 05 结果至 Battle 06 准入

> 本文是[规范英文原文](../../../../synthesis/chapters/grans-opening/battle05-outcomes-through-battle06.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

- **已确认原版结构：**Battle05 选定的位置/HP 和结果消费者、首次完成的宝石/招募/返回程序、条件 Galam 出口及 Battle06 before/start。
- **推断的原版连接：**这些场景描述从 Galam 逃往 Granseal。
- **未知原版行为：**自然结果、合法行走、实际调用方/行动者/出战队伍状态、任意重入、服务完成、呈现及持久性。
- 范围：[#711](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/711) 的英文 Layer B 内容指南之中文镜像，隶属 [#514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)；不准入重制能力。
- 证据基线：已接受的 [Battle05 至 Battle06 研究](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md)，证据日期 2026-10-12，固定 `ShiningForceCentral/SF2DISASM` 修订 `c834c652b6862bc5679fd7f69a38a7093206efc6`，以及该所有者中的 USA 零售 ROM 身份。

承接 [Battle04 结果至 Battle05 准入](battle04-outcomes-through-battle05.md)。该指南解释 Galam 俘虏内容、Slade 跟随和守卫 F405 解锁/请求。此处 Battle05 首次完成的胜利提供显式 Slade 加入和 F406；之后选定出口请求可在 Map66 准入 Battle06。败北、逃脱及已完成的替代分支各有不同。仅数字顺序不确立连续或不可避免的路线。

`Fnnn` 表示标志。源码 Battle05 是 `BATTLE_GALAM_CASTLE`；Battle06 是 `BATTLE_TO_GRANSEAL`。Map16 是 Galam Castle，Map17 是其室内，Map18 是吊桥，Map66 是 Granseal Kingdom 大地图（`MAP_OVERWORLD_GRANSEAL_KINGDOM`）。Slade 的己方角色选择器是 5；已加入 F5、出战 F37、随从 F73、解锁 F405/F406 和完成 F505/F506 是独立状态域。下方所有源码路径/符号均指研究所有者固定的 `disasm/` 基线。

## 1. 骑士败北有独立位置与 HP 消费者

**已确认静态：**普通精灵集首个敌人为 Galam Knight。其 `LEADER` AI 标签独立于敌方首领存在布尔值：`data/battles/global/enemyleaderpresence.asm` 的 Battle05 行是零。该零抑制败北包装器向共享死亡列表追加存活敌人，不禁用较早的 Battle05 位置辅助函数。

[过场合同](../../../contracts/battle/battle-cutscene-routing.md#队长死亡位置准备)与[选定消费者分析](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#battle05s-defeated-consumer-and-outcomes)区分以下步骤：

| 消费者与条件 | 效果与限制 |
| --- | --- |
| `executeindividualturn.asm`，在 `j_EndBattlescene` 之后 | 先调用 `j_ApplyPositionsAfterEnemyLeaderDies`，再调用 `LoadBattle`，早于顶层败北脚本检查。这是战斗演出返回调用方；较晚单次行动结束检查的 HP 为零事件不确立此前调用。 |
| `ApplyPositionsAfterEnemyLeaderDies`（`0x47D9E`），Bowie HP 非零且槽位 128 HP 为零 | 扫描战斗位置表，唯一战斗是 5。不测试 F505。己方角色 0–29 与敌人 128–157 获得 X=-1；敌人 128–157 另获 HP=0。槽位 158/159 仅获 X=0，不写 HP。X 写入不杀死己方角色。 |
| 选定 `abp_Battle5` 记录 | 恢复战斗相对 Bowie `(16,4)`、Slade `(17,4)` 和槽位 128 `(17,2)`。己方角色要求 HP 非零；槽位 128 允许 HP 为零。四字节步长跳过第四字节。区域原点 `(0,14)` 给出地图 Y 18/18/16。Slade 的放置不使他加入，也不证明他参加 Battle05。 |
| `ExecuteBattleCutscene_Defeated`（`0x47B92`） | Bowie 存活、槽位 128 死亡且 F505 未设时，选择行 5 的 `edcs_battle5`（`0x49AB2`）。F505 已设则跳过此脚本。其零首领存在行不将其他存活敌人追加到死亡列表；较早 HP 写入属于独立辅助函数。 |

**已确认编写对话：**普通游标 ID 2323–2331 绑定骑士、Slade 与叙述。骑士提及 Slade 被囚；Slade 找到 Jewel of Light，并得知 King Galam 拿走另一颗宝石。此败北脚本没有加入、物品栏奖励或后续解锁。简述消费已接受操作数/文本审阅，不是渲染台词。

`CountRemainingCombatants`（`0x23C58`）统计 X 非负且 HP 为正的战斗员，Bowie 死亡时强制己方为零。己方为零优先于敌人为零。位置辅助函数解释在其调用方/输入条件下通向剩余敌人为零的源码路径；不证明自然胜利、可获胜性或通用首领死亡规则。[战斗控制合同](../../../contracts/battle/battle-control-lifecycle.md)拥有阵营检查生命周期。

## 2. 胜利、败北与逃脱的返回不同

**已确认源码分支：**结果按下述方式消费传入状态。[结果所有者](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#battle05s-defeated-consumer-and-outcomes)保留精确调用方与存档点条件。

| 结果 | 脚本/状态工作 | 返回与边界 |
| --- | --- | --- |
| 胜利，F505 未设 | 治疗存活/不死己方角色；准备当前地图/最终位置返回字段；运行 `abcs_battle05`；到达共享 `JoinForce(0)` 尾部；清除 F405、设置 F505。after 程序替换返回字段。 | Map16 `(16,18)`、Down，d4=1。宝石/招募/F406 效果见下文。 |
| 胜利，F505 原已设置 | 跳过整个 after 程序；仍调用共享 `JoinForce(0)`；清除 F405、设置 F505。 | 准备的当前地图/首名出战成员最终位置加区域原点/朝向保留。没有新 F384、Slade 加入、F73 清除、F406 或固定城堡传送。这是源码替代分支，不是已证明的自然重复。 |
| 败北 | 更新解锁；固定 ID 363；恢复 Bowie 最大 HP；通过无符号移位将金币减半；取得撤离位置，d4=-1。 | 前篇选定 `EGRESS_MAP=16` 且 F399 已设时：Map16 `(27,7)`、Up。没有 after/加入程序或 F505 写入；不运行 Battle04 特殊抓捕/upgrade 尾部。之后普通存活/不死治疗不复活每名倒下的己方角色。 |
| Egress / Angel Wing | 分别消耗法术 MP 或移除所选物品槽，随后文本/闪光清理、解锁更新及撤离查找，d4=0。 | 相同的条件城堡存档点；没有败北金币惩罚、after 程序、Slade 加入或 F406 生产者。 |

只有 F505 原已设置，解锁辅助函数才清除 F405；否则败北/逃脱使其保持启用。BattleLoop 设置 F399，Battle05 没有撤离特例。不同传入 `EGRESS_MAP` 会改变目的地；F399 未设的辅助调用使用游戏开始元组，未列出地图则默认为 `(1,1)`、Up。因此城堡存档点是有条件的。

前篇较旧的 `battle4DefeatUpgradesBattle=true` 测试夹具标签记录一次 `UpgradeBattle` 调用。其选定列表不含 Battle04/05/06：不证明敌人升级、战斗 ID 递增或下一解锁。此处显式 F406 生产者是首次完成的 after 程序。

### 胜利传送作为返回被消费

**已确认顺序：**`abcs_battle05` 终端传送写入事件类型 1 与参数 1–5，替换准备的返回字段。胜利读取这些字段。`MainLoop` 的 `alt_MainLoopEntry`（`0x75E0`）随后调用 `SwitchMap` 与探索，不重新调用 `CheckBattle`；探索在入场时清除事件类型。此传送提供胜利返回，而非第二次待处理出口分发。后续城堡/吊桥出口使用自身请求与下述遭遇检查。[请求消费所有者](../../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology)闭合局部轮询/清除接缝；已接受分段研究提供此条件组合。

## 3. 宝石场景正式招募 Slade

**已确认编写内容：**`abcs_battle05`（`0x49B48`，`data/battles/entries/battle05/cs_afterbattle.asm`）使用普通 ID 2332–2341。Slade 交出宝石；叙述描述它与 Bowie 颈部融合。他建议之后去教堂，并陪 Bowie 前往 Granseal 寻找另一颗宝石。程序不调用必需教堂事件。状态效果必须与这些台词分别读取：

| 保留的命令索引 | 实际消费者与后果 |
| --- | --- |
| 11：`setF 384` | 宝石对话中写入 F384。此处及败北程序都不授予物品栏槽位，也不移除宝石物品；叙述不是物品事务。 |
| 18：`join ALLY_SLADE` | `csc08_joinForce` 选择普通加入音乐，调用 `j_JoinForce(5)`，取得职业，以名字/职业替换显示固定 ID 446，并等待/关闭文本。固定加入消息不推进脚本游标。 |
| `JoinForce` 辅助函数 | 设置已加入 F5，调用 `UpdateForce`，再检查重建出战数量是否达到 12。低于容量时，`JoinBattleParty` 设置 F37；满队不强制他出战。加入不恢复 HP。 |
| 21/22：`clearF 73`；`setStoryFlag 6` | 清除持久随从选择器，再将剧情操作数加 400，设置 F406。不改变 `CURRENT_BATTLE`，也不同步开始 Battle06。 |
| 23/24：`followEntity ALLY_SLADE,ALLY_BOWIE,2`；`warp ...` | 对当前实体使用角色存活门槛与 `AddFollower`，然后提供固定城堡返回。当前跟随关系独立于 F73 和军团成员资格。 |

[队伍成员资格合同](../../../contracts/party/party-membership-state.md#joinforce-时间线)要求区分重建先于激活。`JoinForce` 没有第二次 `UpdateForce`：新设 F37 不确立辅助函数返回时出战前缀/数量已同步。上述选定源码数字标志/容量来自分段研究；通用合同有意保留符号范围/容量。出战列表同步与实际下一战军团仍为**未知**。

包装器消费的加入表行 5（`0x47D6F`，位于 `data/battles/cutscenes/`）包含零。即使 F505 跳过脚本，它仍到达普通 `JoinForce(0)`；零不是不调用的哨兵值，也不是 Slade 招募。`global/` 下未使用的同名表是独立表。[队伍/名册合同](../../../contracts/party/party-roster-state.md#处理器边界)拥有脚本处理器时序，而非场景叙事角色。

Slade 此前的配置在场、跟随及 F73 不确立 F5。显式 after 程序加入不追溯证明他参加过 Battle05。重建探索实体使用配置与随从标志，因此清除 F73 再添加当前随从，不证明返回后他仍可见地跟在 Bowie 后面。

## 4. 离开 Galam 取决于配置、布局和出口选择

**已确认选择器：**没有标志切换地图行重映射 Map16/18/66。配置选择使用最后匹配标志：Map16 default → F663 → F530；Map18 default → F530；Map66 只有一个配置。下方早期内容假设 F530 未设。Battle05 胜利不设置 F663；前篇军队离开区域（事件）在被调用时提供它。后期配置是替代端点，不属于此处声称的路线。[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)拥有布局/加载顺序。

| 地点或请求 | 条件交接与限制 |
| --- | --- |
| Map16 城堡返回 | F664 已设时，default/F663 init 隐藏守卫 145。X=3 守卫区域（事件）在 F505 已设时返回，先于写 `EGRESS_MAP` 或重新发布守卫程序。X=17 的 F663 测试保持独立。完成不确立每条城堡路线已开。 |
| Map16 南记录 `(255,31)` | 无滚动请求 → Map18 `(9,10)`、Down。独立北记录 `(255,0)` 则提供 Map66 `(32,4)`、Down。两条记录都不证明其标记物理可达。 |
| Map18 标志复制 | F611 控制两次复制：`(19,0)`、尺寸 `(18,30)` → `(0,0)`，以及 `(38,30)`、尺寸 `(18,18)` → `(38,0)`。源码标签描述吊桥放下/新屋顶工作。Map17 init 在 F660/F663 分支之后无条件写 F611，尽管数据注释暗示依赖 F663。F611 未设布局是独立替代分支；仅复制不确立可通行性。 |
| 默认 Map18 内容 | init 与默认区域（事件）直接返回；踩踏事件为空。四名 NPC 提供可选对话。此处没有必需脚本产生 F406。 |
| Map18 南记录 `(255,29)` | 无滚动请求 → Map66 `(31,9)`、Down。北 `(255,9)` 返回 Map16 `(26,30)`、Up；塔楼梯记录仍在 Map18 内。两个南标记之间的行走仍未知。 |

**已确认可选对话：**商人（128，固定 ID 870–872）赞叹宝石，尝试取下但失败；处理器准入不测试 F384，也不进行物品转移。男子（129，873–874）担心 Granseal 的母亲与战争。守卫（130，875–876；131，877–878）谈论禁止离开的命令、生锈桥梁与受限塔楼。对话不确立碰撞、桥梁变更或必需交互。[选定行动者/文本所有者](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#dialogue-and-actors)将这些绑定与实际玩家可达或渲染台词区分。

可选 Map17 重入也发生变化：F505 已设且 F530 未设时，`ms_map17_flag505` 保留 init/实体事件指针，但其区域（事件）表只有默认返回目标。仅重访地图，不会重播早先监狱/捷径/隧道区域（事件）。这不解决任意门/捷径重建、传入随从或工作布局持久性。

## 5. 出口请求可选择 Battle06，较早行优先

**已确认请求顺序：**选定无滚动出口到达 `WaitForEvent` → `ProcessMapEvent`，后者清除待处理类型。类型 1 参数 1 为零，携带地图/X/Y/朝向从探索退回 MainLoop；参数 1 非零则留在滚动探索分支。`SwitchMap` 保持 Map66 不变，随后 `CheckBattle` 先于 Map66 探索配置/init/区域（事件）运行。不需要新 Map66 区域（事件）来启用 F406。

`CheckBattle`（`0x799C`）按升序扫描遭遇行。Map66 上行 2/3/4 先于行 6，且都使用通配触发。
**已确认行 6 条件：**F402/F403/F404 未设、F406 已设。行 6 提供区域 `(21,2,20,32)` 和触发 `(255,255)`；矩形是输出数据，不测试出口元组是否被包含。两个选定 Map66 抵达元组都能匹配，不证明任一路行走。

完成 F506 不拒绝该行：选择会清除 F406，但仍在该次调用返回战斗 6。同样，F402 未设、F403 与 F406 已设时，即使 F503 原已设置，仍选择行 3；其完成状态在选定后清除自身解锁。F505 已设/F406 未设时，重复 Battle05 胜利跳过 after 程序生产者，仅到达 Map66 不启用 Battle06。这些对照防止把源码编号写成自动顺序。[准入所有者](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#direct-exit-content-and-conditional-battle06-selection)保留选定源码关系；独立现有验证轨道没有以可执行方式闭合其组合。

## 6. Battle06 战前场景在入场时写入 F407

**已确认编写场景：**行 6 before 目标 `bbcs_06`（`0x49BCA`）使用普通 ID 2342–2356。守卫 129/130 拦住众人；Slade 声称携带 King Galam 遗留的东西。军官 128 提议代送，认出逃犯，称他们为 Granseal 间谍。借口随后转入战斗布置。Bowie 被布置，但在此场景没有文本操作数。

`loadMapFadeIn Map66,25,5` 提供地图/摄像机布置，不是将 Bowie 放到 `(25,5)` 的传送。`ce_49CBA` 布置 Bowie `(30,9)`、Slade `(29,8)` 及三名守卫行动者。它们是场景实体，不是交战名册。最后 `setStoryFlag 7`（保留命令 45）在 before 阶段设置 **F407**，既不是 Battle06 完成，也不证明 Battle07 准入。

**已确认普通入场顺序：**F88 未设时，BattleLoop 为 before 安装地图/战斗，随后恢复传入地图/战斗、清除区域标志 90–105、治疗存活/不死己方角色、初始化己方角色/敌人、清除 AI 内存、加载战斗，最后调用 start。before 检查 F456 而不设置；start 在分发前设置 F456，其行 6 目标为 `ms_Empty`。F456 原已设置会跳过两个脚本，因此不重复此 F407 写入。F88 已设走挂起入场：清除 F88、恢复保存秒数、清除 AI 内存、重载战斗状态，绕过这些新入场/开场步骤。空终止仍拥有解释器等待/滚动清理，不证明瞬间完成。

[开场合同](../../../contracts/battle/battle-cutscene-routing.md#共享-intro-门)和[选定入场所有者](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#battle06-beforestart-stopping-boundary)拥有这些门槛/顺序。`BattleSpriteset06`（`0x1B365C`）声明 6 个己方布置、9 个敌人、1 个 AI 区域且无 AI 点。己方初始化消费实际已加入/出战队伍，跳过死亡成员。六个布置与 Slade 战前场景行动者不证明六人军团或其传入 HP/出战成员资格。[遭遇合同](../../../contracts/battle/battle-encounter-definition.md)保留数据与出战队伍的区别。止于 Battle06 结算之前。

## 证据审阅与剩余限制

该任务将已接受研究、前篇及相关合同与选定公共载荷和验证器比较相互审阅。没有新提取、源码采集、运行时观察或 H2 重跑。选定原版源码组合、行动者/文本含义和行操作数归已接受分段研究，其固定源码路径/地址与复现凭据仍是溯源所有者。普通对话简述假设未跳过分支；debug/P2-Start 可抑制普通文本/游标递增，固定 `txt`/加入消息则独立。

| 主张 | 已审阅可执行表面与复现命令 | 处置 |
| --- | --- | --- |
| 位置/HP、阵营检查与结果 | [过场测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，ID `sf2-battle-cutscenes-static-v1`，`cutsceneFacts.leaderDeathPositions`、`enemyDefeated`、`afterBattle`、`intro`；[循环](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)，ID `sf2-battle-loop-static-v1`，`lifecycleFacts.remainingCombatants`；[控制](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)，ID `sf2-battle-control-static-v1`，`controlFacts.outcomes`；[过场](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、[循环](../../../../../../src/sf2tool/h2/battle_loop.py)、[控制](../../../../../../src/sf2tool/h2/battle_control.py)验证器。`uv run sf2 h2 battle-cutscenes`、`battle-loop`、`battle-control`。 | HP 为零范围与仅 X 尾部受测试夹具约束，不是杀死全部敌人。Battle05 表/调用方及返回字段组合使用已接受选定源码审阅。旧 upgrade 布尔值仅确立调用。 |
| 宝石/加入/跟随/解锁/返回与 F407 | [脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，ID `sf2-map-script-engine-static-v1`，`forceStateCommandFacts`、`storyStateCommandFacts`、`transitionCommandFacts` 源码位置 `abcs_battle05:11/18/21/22/24` 和 `bbcs_06:1/45`；[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)。[属性测试夹具](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)，ID `sf2-common-stats-static-v1`，`statsFacts.party`；[验证器](../../../../../../src/sf2tool/h2/stats.py)。`uv run sf2 h2 map-script-engine`、`common-stats`。 | 精确位置/守卫将加入、标志运算和传送区分。命令 23 的当前跟随使用已接受消费者审阅。四项属性事实不证明激活后列表同步或此自然场景。 |
| 城堡/吊桥调用方与 F611 | [init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，ID `sf2-map-init-static-v1`，Map17 `routeJoins` 顺序 35–37 及 `primarySourceBodies`；[init 验证器](../../../../../../src/sf2tool/h2/map_init.py)。`sf2-map-events-static-v1` 的[实体](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)与[区域](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)分片，Map18 实体事件 0–3/Map16 区域（事件）0–1；[事件验证器](../../../../../../src/sf2tool/h2/map_events.py)。`uv run sf2 h2 map-init`、`map-events`。 | 完整语义比较保留分支与操作顺序；F611 在 F663 分支之外。实体目标结构不证明可达性；场景简述保留研究文本归属。 |
| 出口数据、请求消费与遭遇选择 | [内容测试夹具](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，ID `sf2-map-content-static-v1`；[请求测试夹具](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，ID `sf2-map-event-request-consumption-static-v1`；[通用地图测试夹具](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)，ID `sf2-common-maps-static-v1`；[内容](../../../../../../src/sf2tool/h2/map_content.py)、[请求](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)、[地图](../../../../../../src/sf2tool/h2/maps.py)验证器。`uv run sf2 h2 map-content`、`map-event-request-consumption`、`common-maps`。 | 清单/局部消费者没有以可执行方式组合 after → 胜利返回 → 选定出口 → 行 6，也不确立行走。已接受条件源码审阅提供此关系；较早行/完成/参数对照仍明确。 |
| 开场与遭遇数据 | [路由测试夹具](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，ID `sf2-battle-routing-data-static-v1`，`facts.routeTargetsParsed=false`；[精灵集测试夹具](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，ID `sf2-battle-spriteset-data-static-v1`，`facts.rowContentTracked=false`；[路由](../../../../../../src/sf2tool/h2/battle_routing_data.py)、[精灵集](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)验证器。`uv run sf2 h2 battle-routing-data`、`battle-spriteset-data`。 | 选定目标/头部/布置仍归研究源码审阅；清单既不证明目标语义，也不证明实际出战队伍。F456/F88 跳过保留入场侧 F407 边界。 |

控制器合同 New Battle 总览把 before/start 归组过宽，不能见证其顺序位置；使用上方已接受选定顺序及精确包装器合同。旧 upgrade 名称限制与 Map18 数据注释/F611 消费者差异同样在更严格边界下保留可见。符号成员资格合同有意省略选定数字值，不证明出战列表同步。这些都不要求悄然改变 Layer A。请求所有者的历史 H3 队列与研究建议的现有所有者扩展是未知项处置，不是采集或新测试夹具指派。

**推断：**预期 Galam 至 Granseal 逃脱连接。**未知：**自然结果与实际返回/调用方/行动者/HP/出战队伍状态；考虑标记/碰撞/占位的行走和依赖 F611 的路径；随从/重入重建；服务完成、渲染对话/音频/输入时序；持久状态。[研究缺口所有者](../../../../../research/chapters/grans-opening/battle05-to-battle06-content-route.md#existing-coverage-and-gap-disposition)保留区分性对照及缺失关系。上述具名 H2 命令是复现路线，不是新 PASS 声明；运行前在同一进程加载 `local/private-inputs.ps1`。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有当前协议，[剧情推进](../../themes/story-progression.md)拥有更广状态框架。Battle06 结果及后续战役内容不属本指南；不消费未合并下游结论。

接着读 [Battle06 结果至 Battle07 准入](battle06-outcomes-through-battle07.md)，了解各自独立的结果、Granseal 内容、可选 Kiwi 加入、王室跟随和条件塔请求/入场。其静态说明保留自然行走、重入及出战队伍未知项。
