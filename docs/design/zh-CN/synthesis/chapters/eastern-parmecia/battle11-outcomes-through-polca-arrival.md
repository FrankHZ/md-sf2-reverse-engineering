# Battle11 结果至 Polca 首次 Oddler 抵达

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/battle11-outcomes-through-polca-arrival.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

洞窟遭遇之后，胜利让出战队伍返回洞窟，而非送往 Polca。独立洞窟出口通向 Bedoe Kingdom 大地图，另一个选定入口通向 Polca。之后城镇 zone 编排受伤男孩抵达：一名 beastman 检查他、呼叫村长并请求治疗。场景末尾在调用方完成城镇状态变化和条件性恢复探索控制之前，先重建居民。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/battle11-outcomes-through-polca-arrival.md)支持下文结果、连接、行动者及首个事件交接。**推断：**它们构成洞窟开场之后的候选接续。**未知：**自然洞窟/城镇穿行、提供的故事和行动者状态、完成的服务与可见呈现。表连接不确立强制或可玩序列；本指南不增加重制就绪主张。

本文承接 [East Shrine 返回至 Battle11 洞窟准入](east-shrine-return-through-battle11-admission.md)，止于首个 Oddler 场景调用方之后及首个条件性控制恢复边界。继续阅读 [Polca 仙女互动至 Battle12 准入](polca-fairy-through-battle12-admission.md)，了解可选乌龟遭遇、独立东向出发及搜寻 devils 交接。乌龟互动 `cs_58512`、F710 已设后续场景、村长/木筏/招募内容、Bedoe 和 Battle12/13 在本指南之外。Fnnn 表示标志 nnn；地图和战斗保留源码编号。

## 从洞窟战斗返回

**已确认结果：**Battle11 没有首领胜利捷径。循环先统计存活己方，再统计敌方，使用非负战斗 X 和非零**字** HP；Bowie HP 为零强制己方计数为零。仅击败 enemy 128 不击败剩余敌人。敌人零且仍有己方时获胜；同时耗尽则败北。

战斗结束与战后场景行均选择 `ms_Empty`。end 包装器要求 Bowie 存活、enemy128 死亡且 F511 未设；after 包装器仅在 F511 未设时运行脚本。空脚本仍执行终止符的条件性滚动等待和速度重置。这里没有新故事场景将出战队伍带过洞窟或进入城镇。

| 结果 | 返回与持久后果 |
| --- | --- |
| 胜利 | 治疗存活/不死己方角色、更新军团并选择首名出战队伍成员。以该成员战斗位置加区域原点 `(0,0)` 及映射朝向初始化 Map48 返回；再运行 after 包装器、清除 F411 并设置 F511。 |
| 败北 | 更新解锁、请求耗尽文本、恢复 Bowie HP，并通过无符号右移将金币减半。通过携带 egress 地图的存档点查找返回，不新设 F511。 |
| Egress 或 Angel Wing | 消耗法术 MP 或物品、执行文本/闪光服务并更新解锁。通过携带 egress 元组返回，不施加败北金币惩罚或胜利完成写入。 |

胜利不规定准入元组 `(15,26)` 或下文选定出口。参考成员未必是 Bowie。治疗不复活死亡且非不死的己方角色。即使 F511 跳过 after 脚本，共享战后加入尾部仍运行：其零/Bowie 行设置 F0、更新军团，并在容量允许时通过 F32 条件性添加 Bowie。它不招募 Oddler、Gerhalt 或 Peter，也不改变已选定返回成员。

败北/逃脱时，`UpdateBattleUnlockedFlag` 仅在 F511 原已设时清除 F411；首次战斗败北/egress 保留提供的解锁。Battle11 没有特殊 egress 覆盖。携带的 `EGRESS_MAP` 选择存档点，包括未匹配地图时 `(1,1)`、Up 回退；本文不确立固定败北目标或最近存档点。

即时战斗返回切换地图并进入探索，不再 `CheckBattle`。因此胜利不立即重新准入 Battle11 或 Battle25。普通返回加载清除待处理、治疗存活/不死己方角色、重建实体/随从，并清除临时标志 256–383。Map48 [缺失配置/init](east-shrine-return-through-battle11-admission.md#洞窟抵达与战斗准入相互独立)，不提供固定居民。加载服务后待处理为零才允许控制；这不确立通向出口的可达路径。

## 离开洞窟并进入 Polca

**已确认选定连接：**这两个固定、不滚动请求独立于战斗返回，也相互独立：

| 选定源点 | 请求目标 |
| --- | --- |
| Map48 `(3,12)` | Map72 `(56,46)`、Down |
| Map72 `(43,46)` | Map13 `(15,30)`、Up — Polca |

Map13 是 Polca；`(15,30)` 是抵达坐标，不是 Map15。返回方向 Map72 点 `(56,45)` 则请求 Map48 `(3,11)`，后者不匹配任一洞窟战斗触发。其他洞窟出口不能替代此选定连接。两个选定行均不测试 F511、神殿完成、Peter 成员身份或 Oddler 标志。

产生传送要求布局 `$1000` 属性与匹配行。探索使用方使 main 空闲、处理声音/返回服务，并将可变地图/X/Y/朝向请求提供给地图切换和战斗选择。仅有行既不证明无碰撞步行，也不证明参数能存续至消费。

Map72 保留已接受的[中断限制](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request)：有序 Battle8/24/26 解锁可截获普通入场；完成可清除选定解锁，但不拒绝同次调用。其 init 可在提供 F716 已设/F10 未设时调用后续内容，zone 也可请求遭遇或后续故事内容。抵达 `(56,46)` 不证明这些分支不存在。本指南复用该审阅，不扩展场景范围。

Polca 有独立战斗区分条件。提供 F413 已设会在任意请求点选择 Battle13，包括 `(15,30)`。若 F513 同时已设，选择器清除 F413，仍在本次调用中选择 Battle13。F413 未设时，无论 F513 如何均允许普通城镇入场。Battle13 区域是输出，不是包含性谓词；F511 完成既不确立 F413，也不确立 F513。下文城镇内容描述普通入场分支，不解释 Battle13。

## 事件之前的城镇实体群体

**已确认配置选择：**普通 Polca 入场重建实体、执行共享地图加载服务并运行 init。配置变体按 default、F710、F799、F513 顺序扫描。最后设置标志胜出，优先级为 F513 > F799 > F710 > default。全部共享 zone 与 init 处理器；F513 还替换实体事件表。以 `_709` 结尾的文件名包含 F710 符号，不增加 F709 条件。

默认与 F710 列表提供己方角色 9 Gerhalt，加十三条普通 NPC 行。Gerhalt 使用己方角色查找，不占普通 NPC 事件编号。普通 NPC ID 从 128 开始，独立于物理槽位和此前随从：

| 查找身份 | 本有界内容中的角色 |
| --- | --- |
| 己方角色 9，Gerhalt | 若尚未映射，声明在 `(23,13)`、Down。他存在不是招募事件。 |
| 128 与 129 | 男性和女性 centaur，前两条普通 NPC 行。 |
| 130 | Kappa/乌龟，在 `(1,11)`、Down，使用 `eas_InitSlow`；不是女性 centaur。 |
| 133 | Beastman，在 `(23,6)`、Right，检查男孩并呼救。 |
| 137 | 男子，在 `(15,3)`、Down，场景中被称为村长。 |
| 140 | Oddler 精灵，初始在 `(63,63)`、Down；场景为其抵达显式定位。 |

Polca 没有固定 Peter 行。F67 可独立于已加入/出战标志、字 HP 或军团 AI 状态重建 Peter7。战斗外死亡己方角色可取得共享蓝色火焰精灵替代；行动者或精灵既不证明存活，也不证明成员身份。若没有 Peter 映射，具名脚本操作可解析到物理 main 槽位 0，而非安全跳过。

即使 F710 未设，提供的替代状态仍重要。F799 选择较短实体群体：此时 130 是 minotaur，且尽管共享处理器，133/137/140 没有固定行。F513 提供另一实体群体及实体事件表。缺失映射可解析到槽位 0。首个事件测试不排除这些状态，因此不能证明每个提供元组都有合理阵容。

init 依序作三个独立选择。F711 未设时在 130 上运行 `cs_58310`；F716 已设让 137 面向 Down；F718 已设将 140 移出地图。简短 130 脚本请求 Down 朝向，安装并等待关闭运动、写入 Right 朝向并更新精灵的行动，然后重置动画。**推断：**这编排乌龟姿态。**未知：**可见外观。仅 F711 已设 init 不隐藏 130。移出地图写入离图坐标，不删除行动者；即使提供 F718，选定事件之后仍可重新定位 140。这些都不是被排除的乌龟互动。

## 受伤男孩与村民反应

**已确认事件选择：**在 `(15,30)` 城镇入场不会自动运行事件。之后 zone 请求需要 `$1400` 并选择首个匹配行。X24/Y 通配先于 Y11、Y30 与默认行，因此即使在 `(24,11)` 或 `(24,30)` 也胜出；返回的处理器不落入后续行。尚未确立连续步行到合格 X24 瓦片。

`Map13_ZoneEvent0` 首个分支仅测试 F710。未设时运行 `cs_58330`；提供的 F711/F716/F718 不跳过它。F710 已设到达本指南之外的独立后续场景区分条件。首个场景有十一条文本请求，ID 1452–1462（`05AC..05B6`）：五条 beastman、五条村长、一条旁白。其有序内容与调度如下：

1. 隆隆声、地震幅度 6、白色闪光请求 90 和地震重置引入抵达。Bowie、beastman 与 Peter 接收往复行动，其间有 sleep。Oddler140 被放在 `(29,7)`、Left，以变化速度向左移动，然后收到短暂向下运动请求，随即被停止行动替换。随后关闭运动/Left 朝向调度；beastman 转向他。
2. Beastman 作出反应、移向男孩并面向他。旁白描述检查，beastman 报告受伤。更多朝向/移动与两次跳跃请求引向呼叫村长。这是前四条文本 1452–1455。
3. 村长被放在 `(17,7)`、Right。摄像机目标位置请求为 `(13,3)`；Bowie 和 Peter 均被请求位于 `(25,9)`、Up，跟踪改为村长。村长向右/上移动、询问发生何事，听 beastman 报告来自山中的受伤男孩并作出反应（1456–1458）。他随后向下/右移动靠近男孩。
4. 村长谈到男孩眼伤，并将伤归因于喷发（1459–1460）。Beastman 靠近、请求治疗，村长命令将男孩带进去（1461–1462）。Beastman 收到点头请求。文本传达村民说法，不是独立观察到的医疗或喷发事件。
5. 村长、beastman 和男孩接收短移动请求，被停止脚本中断。跟踪转为中性；村长与男孩开始 Left9 移动，beastman 的 Left9 行动被等待。男孩和 Bowie 收到初始化行动，脚本请求最终朝向、指示 Peter 跟随 Bowie，并显式重载 F710 实体群体。

这些是概述，不是全文记录。场景没有选择、加入、传送、战斗完成测试或故事标志写入。村长首条眼伤文本使用 `nextText`；其他十条请求使用 `nextSingleText`。完整有序记录、内联行动和文本溯源仍归[场景研究](../../../../../research/chapters/eastern-parmecia/battle11-outcomes-through-polca-arrival.md#first-x24-event-and-scene-records)所有。

## 请求、跟随与重建城镇

**已确认服务边界：**地震命令写入幅度；闪光参数 90 导致十二次淡入淡出服务启动和调色板复制，不证明 90 个可见帧。十四次显式 sleep 共请求 392 帧，不是场景经过时长。文本、滚动、淡入淡出和行动等待增加服务，调试跳过则可省略文本/sleep。往复安装及短 sleep 后用停止脚本替换，不保证完成瓦片移动。

位置/朝向命令仅在己方角色**字** HP 为零时跳过；HP256 非零。因此 Bowie/Peter `(25,9)` 请求各自有条件。NPC 绕过该守卫，行动安装没有同等 HP 测试。摄像机目标位置会等待滚动，而仅指定实体目标不会。文本服务等待滚动；终止符条件性等待滚动并清除速度，而非等待每个行动者。实际映射、运动、摄像机到达、可听/可见输出和服务完成仍为**未知**。

Peter→Bowie `followEntity` 请求编码字 `$0007,$0000,$0002`。处理器检查首字高字节，因此测试 **Bowie0 的**字 HP，而非 Peter7。Bowie 存活且映射有效时，选择器 2 安装 Peter 以偏移 `(-24,0)` 跟随 Bowie，独立于 Peter HP。缺失 Peter 可使两个操作数都解析到 main。这既不写入 F67，也不写入已加入/出战状态，且不等待可见跟随。

下一命令在调用方 F710 仍未设时显式加载 `ms_map13_flag710_Entities`。它采样 main 当前坐标/朝向、清除实体/行动状态和映射、重置 walking 分配，并重建标志随从及该列表。它不查询配置标志、不调用地图 init、不治疗/重置临时标志，也不运行独立地图精灵加载服务。main 在采样位置以空闲状态重建，未必在请求的 `(25,9)`。

当前临时跟随因此被重建替换。随从标志只有 F67 时，Peter 在物理槽位 1 以 `eas_Follower1` 重建，Bowie 目标及 `(-24,0)` 偏移与成功的前序请求相同。重建不一定意味着可见跟随不同。其他启用随从改变物理顺序/行动选择；F67 未设时，没有固定 Peter 行重建他。提供的 F68 可独立在 31 下构建 Oddler 精灵随从，与居民 140 分开；本场景不设置它。

显式居民列表将 beastman133 恢复到 `(16,4)`、mayor137 到 `(15,3)`、Oddler140 到 `(14,3)`，不受先前移动终点影响。其他 default/F710 声明不变。男孩现在被放入城镇恢复后的实体群体，没有招募。

**已确认调用方顺序：**只有场景和重载返回后，调用方才处理 F711。未设则在重建的 130 上重跑 `cs_58310` 以编排姿态；已设则将该重建行动者移出地图。然后设置 F710 并返回。它不写入 F711/F716/F718/F68，也不增加加入或待处理传送。之后配置查找仍遵循更高优先级 F799/F513；F710 写入不再次重载，当前显式实体群体也不必与之后选择一致。

zone 分发器关闭立绘/文本、等待一个 VInt，并等待物理 main 停止。仅待处理为零才让 `WaitForEvent` 恢复摄像机目标 0 和受控实体行动。这是首个条件性控制恢复边界，也是本指南终点。它不保证所有步行/随从行动、已渲染城镇状态或可用物理输入。

## 证据审阅与剩余限制

本指南使用 [PR #774](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/774) 接受的研究及 PR #773 的前序英文指南。研究拥有固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、USA retail 身份、精确行/文本/实体群体组合和复现。对已接受文字、受跟踪夹具、合同与 schema/验证器边界的有界审阅，未发现需要修正的 Layer A。未生成或复制可选载荷、未重复源码调查，也未执行运行时观察。

| 审阅关系 | 既有证据与区分边界 |
| --- | --- |
| 结果与返回 | [sf2-battle-global-data-static-v1](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)及[战斗生命周期合同](../../../contracts/battle/battle-control-lifecycle.md)。汇总 routing 保留 `routeTargetsParsed=false`；精确 row11 首领/加入/空目标和循环/egress 组合来自已接受研究。 |
| 请求、配置与阵容 | [sf2-map-data-static-v1](../../../../../../tests/fixtures/h2/map-data-static-v1.json)、[sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-entities-static-v1](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)及[地图探索合同](../../../contracts/world/map-exploration.md)。清单/使用方事实自身不证明精确选定传送行、居民身份或自然旅行；研究提供这些关联。 |
| init 与首个调用方 | [sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，body18/嵌入 `cs_58310`，以及 [sf2-map-events-static-v1:zone-programs](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，program9 `Map13_ZoneEvent0`。有序操作保留独立 init 标志与脚本→F711 处理→F710 写入；后续分支保持排除。 |
| 场景命令与实体群体替换 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，Polca 放置/关系/实体群体位置及处理器事实；[出战队伍/名册合同](../../../contracts/party/party-roster-state.md)。族位置索引从零开始，不同于研究从一开始的场景记录；它们不是完整公开场景记录。 |
| 跟随守卫 | [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json)及[关系研究](../../../../../research/world/common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)。受控处理器证据支持高字节 HP 接缝，而非此组合 Polca 事件或可见跟随。 |

既有[地图 init 验证器](../../../../../../src/sf2tool/h2/map_init.py)与 [schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)保留主体/操作顺序。[事件夹具加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)和[分片 schema](../../../../../../schemas/h2/map-events-fixture-shard.schema.json)保留选定调用方。[脚本 schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)和[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)保留操作数、处理器及族位置限制。受跟踪 H2 夹具已在内存中对其所属 schema 验证。窄所有者包括 `uv run sf2 h2 map-data`、`map-setup`、`map-entities`、`map-init`、`map-events`、`map-script-engine`、`battle-global-data`、`battle-routing-data` 和 `battle-cutscenes`；这些名称是可追溯性，不主张新提取。研究的[复现边界](../../../../../research/chapters/eastern-parmecia/battle11-outcomes-through-polca-arrival.md#caller-handoff-and-evidence-boundary)拥有保留比较。纯文字工作选择直接检查及已提交头的 planner/CI。

**未知：**自然无碰撞洞窟/城镇穿行和无中断旅行；实际标志、行动者、HP、出战队伍/AI 与映射；请求存续、待处理和步行/文本/摄像机/淡入淡出完成；地震/闪光/精灵外观及时序；连续可玩性。这些限制既不授权采集，也不授权新机制。本指南不作引擎/产品选择，也未完成战役或更广设计 epic。
