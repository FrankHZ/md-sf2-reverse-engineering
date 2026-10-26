# Polca 出发与 Luke 招募

> 本文是[规范英文原文](../../synthesis/polca-departure-through-luke-recruitment.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Luke 离开场景呈现他自愿与 Bowie 同行和战斗。Polca 选定南出口请求大地图，招募场景可在地图初始化期间运行。它创建和调度一个 Luke 行动者，让 Luke 加入军团，随后移动并隐藏该行动者，再写入后续战斗解锁。这些是独立有序效果：陪同、场景出现、加入军团与在战斗中出战是不同状态。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/polca-departure-through-luke-recruitment.md)支持下文准入、完整场景和直接消费者。**推断：**归属交流描绘 Luke 自愿参加旅程。**未知：**自然离开、实际传入状态、服务完成、可见表现、响应输入及存档/读档持久性。Fnnn 表示标志 nnn；除另指单位外，坐标为地图瓦片。

两次请求及当前/未来跟随见 [Polca 的 Oddler 选择与跟随](polca-oddler-choice-and-following.md)；市长交易见[木筏授予](battle13-outcomes-through-polca-raft-grant.md)。本指南解释选定离开至条件探索控制。登乘、河流旅行和随后战斗准入仍在边界外。不增加重制能力或产品决策。接续 [Polca 木筏登乘与 South Parmecia 入场（英文）](../../synthesis/polca-raft-boarding-through-south-parmecia-entry.md)，了解独立玩家请求、分阶段 mode2 提交及两个选定 Map69 入场生命周期。该指南保留本场景接触与自然路线限制。

## 离开 Polca 并到达场景

**已确认选定请求：**Polca 南向传送匹配任意 X、Y31，请求 Map72 `(43,47)`、Down。更早两扇同地图铁匠门不离开城镇；之后东出口请求 Mount Volcano。在 `(31,31)`，南行胜过东行。普通移动必须从相应布局标记产生传送请求，其消费者必须经地图选择返回。单凭元组既不证明合法步行，也不证明旅行完成。

南行没有 Oddler、木筏、招募或更早场景前提。Polca 独立 Y30 zone 可在 F716 已设/F718 未设时选择 Oddler 请求，但选定南向传送不证明到达、回答或绕过过该 zone。相邻坐标不推出自然场景顺序。[出口/准入所有者](../../../research/polca-departure-through-luke-recruitment.md#selected-exit-and-interruption-before-entry)保留请求生产、待处理槽消费及源码优先级。

探索 init 前，普通准入按顺序检查 Map72 战斗行：

| 首个符合条件行 | 解锁 / 完成标志 | 对本请求的后果 |
| --- | --- | --- |
| Battle08 | F408 / F508 | 可在招募前打断。 |
| Battle24 | F424 / F524 | 仅在更早行未选择战斗时考虑。 |
| Battle26 | F426 / F526 | 仅在两条更早行均未选择战斗时考虑。 |

三者均有通配触发。区域矩形是战斗输出，不是包含检查，因此 `(43,47)` 不排除它们。已设完成标志清除选定行解锁，但本次仍准入该战斗。三个解锁均未设时选择普通 Map72 探索。后续战斗返回使用独立入场路径；此处不假定返回元组或立即重复准入检查。

**已确认 init 准入：**Map72 普通 init 仅检查 F716 和 Luke 成员资格 F10。

| 传入状态 | 招募场景 |
| --- | --- |
| F716 未设 | 跳过，无论 F10；不修复缺失后续解锁。 |
| F716 已设、F10 已设 | 跳过；不检查行动者、木筏、跟随或 F416 一致性。 |
| F716 已设、F10 未设 | 运行 `cs_4FF5A`，即使 F416 已设。 |

F718、F68、F75、F64、HP、出战队伍容量和坐标不是额外 init 条件。因此对话完成标志 F716 可在没有 Oddler 跟随或木筏时准入 Luke。传入该标志不证明市长对话或有效木筏状态。未使用的挂起场景字节不在此普通路径范围。

## 谁在大地图出现

**已确认全新入场：**显式南向请求在 init 前重建 Map72 行动者，中间没有玩家控制交接。入场包含普通存活/不死同伴治疗，但之后创建和加入命令不治疗。Map72 固定实体列表为空；标志随从和 main physical 0 仍填充它。Polca NPC140 及其安装的跟随指针不会作为相同行动者带过来。

F67 声明 Peter7，F68 在 **FOLLOWER_B 查找 31** 下声明 Oddler 精灵，F75 声明 Luke10。成员资格和 HP 不决定这些随从行准入。F65 前置车队并使物理槽位偏移；其他传入标志也可改变实体群。F74/F70 可在查找31下提供 Astral，冲突标志并存时有序覆盖。场景不修复这些冲突或缺失随从。

木筏独立：普通非木筏玩家状态加 F64 及匹配 `RAFT_MAP`，在 init 前声明它。已接受授予元组将它放在 Map72 `(43,48)`。其 **physical 31、mapping 63** 不同于 FOLLOWER_B 的**查找 31、mapping 31**。F716 本身不声明或修复它。传入木筏玩家状态使用独立 main 精灵/站立随从/离图木筏分支。

传入 `MAP_CURRENT` 请求走不重建替代路径：更新 main 并调用独立木筏消费者，不重构行动者或清除临时标志。init 仍可运行，但当前映射不同。选定南行是显式72。[实体群与入场所有者](../../../research/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)保留两个入场分支和木筏限制。

**已确认新行动者：**场景在 `(43,47)`、Down 创建 Luke10，选择映射0–62中最大有符号非负物理索引加一。排除木筏映射63、覆盖查找10，不复用已有 Luke 或显式检查容量。`NOTHING_BYTE` 精灵操作数由 Luke HP/职业精灵选择替代：战斗外零字 HP 选择蓝焰；否则用职业/表精灵。创建既不加入、治疗，也不追加到探索随从列表。之后一帧 sleep 不是初始化完成的汇合等待。

全新入场、普通玩家且仅 F67/F68 时，Peter 和 FOLLOWER_B 占 physical 1/2，新 Luke 占3。若还传入 F75，旧 Luke3 留在随从列表，新 Luke4 替换查找10。后续命令和 hide 解析新 Luke，不移除所有 Luke 精灵。仅加 F65 会使这些随从/新索引加一。这些示例要求所述映射，不定义永久角色槽位。

放置和朝向可因零字 HP 跳过 Bowie/Peter/Luke；FOLLOWER_B31 绕过保护。映射查找不增加安全存在检查。HP 非零但缺失的 Peter 可解析为 main0；未声明31可将 main 移到 `(45,48)`、Left，而 Astral 映射则移动该行动者。脚本既不创建 Peter，也不创建31。精确查找与精灵消费者见[分配和调度所有者](../../../research/polca-departure-through-luke-recruitment.md#new-luke-actor-and-explicit-staging)。

## Luke 自愿加入

**已确认请求序列：**完整场景有六条归属游标文本1556–1561（`0614..0619`），加加入消息446（`01BE`）。没有 Yes/No 或命名提示。下文改述对话；台词中的方向不证明可达路线。

| 阶段 | 请求行动与归属内容 |
| --- | --- |
| 集合 | 创建 Luke、sleep1，随后放置 Bowie `(45,47)`、Left，Peter `(44,47)`、Down，Luke `(43,47)`、Down，FOLLOWER_B `(45,48)`、Left，受上述 HP/映射限制。 |
| 提议旅程 | 请求主旋律音乐、`fadeInB` 和 sleep40。Luke 1556 提议去 Devil's Tail 见 Creed。请求两次分别等待的 Luke 跳跃，随后 sleep30。 |
| 自愿同行 | Peter 转 Left、Luke 转 Right。Peter 1557 指出 Luke 要同行；Luke 1558 在连续文本中说想与领袖并肩战斗，Luke 1559 表示兴奋。请求 `join ALLY_LUKE`。 |
| 加入通知 | 请求加入音乐、提交成员资格及条件出战标志，随后用 Luke 职业/名字请求消息446，再执行淡入淡出/输入/文本服务。 |
| 出发交流 | Peter 转 Right 说1560、评论 Luke 爱冒险，再转 Left。Luke 1561 催促出发；sleep30。 |
| 离开当前场景 | 等待 Luke 内联 Down1 行动、隐藏其映射行动者、设置剧情操作数16（F416），随后终止并返回 init。 |

每次跳跃等待所选行动者行动指针到达 `eas_Idle`；最后 Down1 同样等待该行动者。这些等待不继承放置 HP 保护，也不证明所有随从已停。初始化恢复移动/精灵参数，不恢复健康。四次独立 sleep 共请求101帧，不含对话和其他服务；不是实测经过时长。单条文本关闭文本/立绘并请求 Sleep10；连续文本有独立生命周期。跳跃、文本、音乐和淡入淡出的渲染仍未知。

## 成员资格、隐藏与后续解锁

**已确认成员资格时序：**`JoinForce(10)` 设置 **F10**，以 `UpdateForce` 重建已加入/出战/后备缓存，随后检查重建出战数与12。低于12时设置 **F42（`32+10`）**；否则保留传入出战标志。它既不驱逐其他成员，也不保证满员时新增出战槽。已设 F42 在传入超员状态仍保持已设。

重建先于条件 F42 写入，辅助函数没有第二次重建。因此之前未出战 Luke 可留在缓存后备/计数快照中，直到另一次 `UpdateForce`。成员资格不确立即时缓存同步、HP、复活或玩家单次行动。此场景没有清除 F75、追加随从、跟随命令或军团 AI 写入。[成员资格时序合同](../contracts/party-membership-state.md#joinforce-时间线)与[场景所有者](../../../research/polca-departure-through-luke-recruitment.md#membership-current-hiding-and-the-later-unlock)保持这些边界独立。

等待 Down1 后，hide 把当前映射 Luke 移出地图、使指向该物理行动者的每项映射失效，并在其存在于探索列表时移除。普通新行动者从未追加；不同物理索引上的较早 F75 随从可保留。Hide 不清 F10、F42 或 F75。之后全新入场时，F75 可独立于 F10 重建 Luke；F75 未设则空固定列表不提供他。F10 仍抑制招募。因此当前隐藏不是永久视觉移除，不重建入场也不是全新重构。

之后剧情写入设置 **F416**，解锁 Battle16。既不设置 F414，也不启动交战或调用战斗准入。Battle14/16 编号不证明自然路线顺序。序列为行动者/调度 → F10/重建/条件 F42 → 加入服务 → 更多对话 → 等待 Down1 → hide → F416 → 调用方返回。不是原子事务或自动恢复机制。

加入前停滞可留下 F10 未设和已调度行动者；另一次符合条件 init 创建 Luke，而不复用。F10 设置后停滞可留下 F416 未设、移动/hide 未完成，但之后 init 跳过场景。反过来，F416 已设/F10 未设仍运行。这些是静态部分状态后果，不是已观察存档历史或承诺修复。

## 条件探索控制

**已确认边界：**Luke 的 Down1 仅在预期调度和移动完成时指向已配置木筏瓦片。这是 Luke 行动随后 hide，不是玩家登乘或保证木筏接触。场景不写玩家类型或木筏坐标，不排入登乘或传送事件。碰撞、HP 跳过放置和替代映射仍重要。

终止符在对话窗口仍存在时条件等待滚动、清除滚动速度并返回 init。探索仅在基础/当前调色板比较不同时请求地图音乐与淡入；不保证第二次音乐请求。在 `WaitForEvent`，待处理为零会将摄像机重新指向 physical 0 并设置受控实体行动；普通玩家 type0 选择角色控制。非零待处理或另一玩家类型走各自处理。**止于该条件控制交接。**脚本返回不是响应输入回执、登乘完成、后续旅行或 Battle14/16 准入。

## 证据审阅与剩余限制

本指南消费 main 上 [PR814 接受的研究](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/814)及已接受前序所有者。研究保留 USA 零售身份、固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、完整场景/文本/内联行动体比较与直接消费者审阅。其[复现边界](../../../research/polca-departure-through-luke-recruitment.md#reproduction-and-acceptance-limits)拥有原始源码检查。本综合审阅正文、合同和限定 tracked 测试夹具选择，不作新源码调查、载荷生成或运行时采集。

| 审阅边界 | 既有证据表面 | 结论 |
| --- | --- | --- |
| 请求/准入 | [sf2-map-event-request-state-static-v1](../../../../tests/fixtures/h2/map-event-request-state-static-v1.json)、[sf2-map-event-request-consumption-static-v1](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)。 | 写入方/消费者及路由事实不证明自然步行。研究拥有四条选定传送行及三条实际 Map72 准入行。 |
| 全新实体群/init | [sf2-map-setup-static-v1](../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-entities-static-v1](../../../../tests/fixtures/h2/map-entities-static-v1.json)、[sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json)，body 78。 | 六个操作及两个返回目标确认 F716/F10 选择；汇总实体群事实不是实时映射或木筏回执。 |
| 场景/生命周期 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，实体群、放置、行动、生命周期、军团和剧情族；[地图脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)。 | 命令族覆盖不是完整逐场景运行时覆盖。研究拥有全部29条记录、六条游标文本、join446、分配/HP及直接消费者。 |
| 已加入/出战/缓存顺序 | [成员资格合同](../contracts/party-membership-state.md#joinforce-时间线)、[出战队伍/名册](../contracts/party-roster-state.md)。 | 条件激活前重建一致；Luke 数值标志/容量是场景所有者事实，不是玩家控制或持久性的新保证。 |

[Init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)、[init 验证器](../../../../src/sf2tool/h2/map_init.py)及[地图脚本 schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)限定测试夹具形状。既有窄路线包括 `uv run sf2 h2 map-init`、`map-script-engine`、`map-setup`、`map-entities`、`battle-routing-data` 与 `map-event-request-consumption`；聚焦检查包括 `tests/python/test_map_init.py` 和 `test_map_script_engine.py`。这是证据导航，不是新执行的文档门禁。未发现需要所有者纠正的证据冲突。

**未知：**自然 Y30/出口选择与更早选择；实际战斗、随从、木筏、成员资格、出战、HP、AI 与映射；并发位置、行动/服务完成、文本/精灵/摄像机/音频/淡入淡出渲染、响应输入及持久存档/读档行为。这些限制不阻止静态指南，也不授权自动采集。[已接受研究交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/814#issuecomment-6159534926)保留最终成功 CI 以及已解决的检查、引号、路径检查与传播失败，并链接更早结果。不隐藏或仪式性重放未解决比较失败。本指南既不声称连续可玩，也不声称战役/epic 完成。
