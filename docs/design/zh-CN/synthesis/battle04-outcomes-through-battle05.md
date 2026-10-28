# Battle 04 结果至 Battle 05 准入

> 本文是[规范英文原文](../../synthesis/battle04-outcomes-through-battle05.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle04 有两条不同源码路径通向 Galam 俘虏情节：首次完成的胜利运行 Lemon 抓捕场景，特殊败北则在承担普通败北代价之后返回 Galam。逃脱遵循选定撤离状态。在 Galam 室内，监狱对话、打开牢房和 Slade 的捷径提供下一段编写内容；城堡守卫才提供 Battle05 的实际解锁与请求。
**推断的剧情连接：**这些场景承接[Battle 03 结果至 Battle 04 准入](battle03-outcome-through-battle04.md)的遭遇。
**已确认静态：**已接受的 [Battle04 至 Battle05 调查](../../../research/battle04-to-battle05-content-route.md)连接其条件调用方与消费者。
**未知：**自然交战结果、连续合法行走、传入实体状态和完成的呈现。源码不确立强制败北或成功攻略。

本指南解释地点、行动者、对话内容和状态交接，直至 Battle05 before/start。Battle04 和 Battle05 是从零计数的源码槽位 4 和 5（`BATTLE_AMBUSHED_BY_GALAM_SOLDIERS` 与 `BATTLE_GALAM_CASTLE`），并非已验证的原版 UI 编号。Galam Castle 是 Map16；Galam 室内为 Map17；Hawel 住宅为 Map43。`Fnnn` 表示标志。原版证据使用 USA 零售 ROM 及 `ShiningForceCentral/SF2DISASM` `master` 的 `c834c652b6862bc5679fd7f69a38a7093206efc6`，身份见[研究溯源](../../../research/battle04-to-battle05-content-route.md#agreement-and-provenance)。
**已确认**表示具名静态事实或明确受限的观察；**推断**表示叙事连接；**未知**指出缺少的执行边界。

[已接受的私有重制里程碑](map3-battle01-readiness.md#当前已接受里程碑)止于 Battle01 可用返回。本解释不增加后续可玩支持，也不使用重制实现作为原版证据。

## 1. Battle04 结果通向不同返回路径

**已确认共享结果规则：**控制器在行动死亡处理后，以及单次行动结束效果/死亡处理后统计战斗员。要求 X 非负且 HP 为正；Bowie HP 为零会强制己方为零。己方为零优先选择败北，然后才检查敌人为零的胜利。Battle04 的敌方首领存在行为零，敌人败北程序是 `ms_Empty`。因此，仅杀死槽位 128 既不提供其胜利规则，也不提供脚本强制败北。[战斗控制合同](../contracts/battle-control-lifecycle.md)拥有共享生命周期；[选定结果分析](../../../research/battle04-to-battle05-content-route.md#battle04-outcome-branches)拥有 Battle04 组合，不声称可获胜性。

| 结果与谓词 | 有序状态效果 | 返回边界 |
| --- | --- | --- |
| 胜利，F504 未设 | 治疗存活/不死己方角色，准备当前地图、首名出战成员最终战斗员 X/Y 加区域原点及实体朝向。after 程序用抓捕传送替换这些返回字段。共享尾部调用 `JoinForce(0)`；胜利清除 F404、设置 F504。 | Map17 `(0,0)`、Right，结果 `d4=1`。MainLoop 切换地图并进入探索，不重新检查遭遇。 |
| 胜利，F504 已设 | 跳过抓捕程序，仍调用 `JoinForce(0)`、清除 F404/设置 F504。 | 准备的最终状态操作数保留：对前述遭遇调用方是 Map66。此重复分支不确立自然重复战斗。 |
| Battle04 败北 | 调用解锁辅助函数、显示败北消息、恢复 Bowie HP、按无符号向下取整除法将金币减半，并取得撤离元组。特殊尾部随后清除 F404、设置 F504、调用 `j_UpgradeBattle`，将地图覆盖为 17，返回 `d4=0`。不执行 after 程序或共享战后加入。 | 仅地图/结果被覆盖。使用前篇的 `EGRESS_MAP=43` 时，普通 F399 已设查找给出 `(7,5)`、Down；这些操作数在 Map17 返回中保留。 |
| Egress / Angel Wing | Egress 消耗法术 MP；Angel Wing 移除选定物品槽。两者都使用解锁辅助函数与撤离查找，返回 `d4=0`。都不执行特殊败北尾部、抓捕程序或 F504 写入。 | `EGRESS_MAP=43` 未变时，为 Map43 `(7,5)`、Down。F504 未设则 F404 保持启用；F504 已设则清除 F404。逃脱不提供俘虏交接。 |

**已确认辅助函数限制：**只有当前完成标志已设，`UpdateBattleUnlockedFlag` 才清除当前解锁。`GetEgressPositionForBattle` 没有 Battle04 目的地特例；普通 BattleLoop 入场设置 F399，选择地图存档点查找。提供 F399 未设的辅助调用则使用游戏开始元组；未列出的撤离地图有自身默认坐标。这些替代分支使前篇 Map43 调用方不能被当作通用目的地。

特殊败北的 `j_UpgradeBattle` 解析到 `UpgradeBattle`，保留 d0–a6，并扫描随机战斗列表。
**已确认选定源码结果：**列表没有 Battle04；调用不写完成标志、不改变敌人、不推进战斗 ID，也不解锁 Battle05。F504 来自显式败北尾部。较旧测试夹具名 `controlFacts.outcomes.battle4DefeatUpgradesBattle=true` 记录这一具名调用，不是效果。

### 首次胜利运行 Lemon 抓捕内容

**已确认编写场景：**`0x498EE` 处的 `abcs_battle04` 将普通游标 ID 2319–2322 绑定到 Lemon。他赞扬战斗、拒绝让出战队伍离开、辨认首领并下令将他们带到 Galam。脚本移动、撞击音效与姿态布置抓捕；它们不把战斗员 HP 写为零。场景实体表含同伴和士兵，不证明出战成员资格。

终端命令 57 请求 Map17 `(0,0)`、Right。传送处理器写入事件类型 1 及参数 1–5；胜利将被覆盖的参数读为自身返回。探索入场清除待处理事件类型，因此这不是之后第二次类型 1 分发。消费的零加入表是 `data/battles/cutscenes/` 下的表，不是 `global/` 下未使用的同名表；零到达 `JoinForce(0)`，不是 Slade 招募。[过场合同](../contracts/battle-cutscene-routing.md#战后路由与加入尾)和[队伍成员资格合同](../contracts/party-membership-state.md)保留包装器与已加入/出战域的区别。

## 2. 在 Galam 醒来恢复战斗属性，不恢复资源

**已确认配置边界：**Map17 的有序选择器是 default、F505、F530；最后匹配选择器胜出。下述早期内容假设 F505/F530 未设。Map16 与 Map17 都没有标志切换地图覆盖。后期配置是替代端点，不属于所声称的早期剧情路线。[地图配置合同](../contracts/map-setup-data.md)拥有配置结构；[俘虏分析](../../../research/battle04-to-battle05-content-route.md#conditional-captivity-and-escape-content)拥有这些选定调用方。

默认 `ms_map17_InitFunction`（`0x52450`）仅在 F660 未设时调用 `cs_5249E`，然后设置 F660。之后，F663 已设会移除实体 131，并调用 `cs_5247C` 中四次门/图块复制。调用方总会设置 F611。F660 已设跳过醒来场景及其重置和放置：不能为此重复替代分支抹除胜利/败北不同的返回坐标。

**已确认重置顺序：**`resetForceBattleStats` → `resetMap` → `loadEntitiesFromMapSetup 0,0,RIGHT`。第一步调用 `ResetAlliesBattleStats`：全部 30 个己方角色槽位恢复到最大 HP/MP；既有状态位与 `0x0007` 做掩码，仅保留原有 STUN/POISON/CURSE 位，随后刷新派生属性。不跳过死亡己方角色。这不同于战斗前后使用的存活/不死治疗。不恢复金币、不招募任何人，也不没收物品栏。因此特殊败北此前的金币减半仍保留。地图重置/加载有自身消费者，不意味着新游戏。[队伍/名册合同](../contracts/party-roster-state.md#处理器边界)拥有重置调用；[地图合同](../contracts/map-exploration.md#加载与选择顺序)拥有加载区别。

普通醒来对话使用 ID 880–891，再显式重置到 892–893。Sarah、Kazin、Chester 和两名囚犯谈论 Galam 战争及 Ground Seal；Bowie 醒来加入谈话。程序将 Bowie 布置在 `(59,4)`、Up，再设置目的地 `(59,5)`。默认配置的 132 是老人精灵，133 是士兵精灵；对话描述学者和军队领袖。精灵身份与对话角色应区分。具名配置条目包括 `(48,4)` 的 Slade；仅声明不使他加入。传入随从标志先于配置实体初始化，可能影响实际人口/放置。

## 3. 牢门、Slade 与通道

以下**已确认静态**阶段是区域（事件）选定的场上内容，取决于传入坐标与标志。区域（事件）分发器不证明阶段之间有合法行走。

| 阶段与调用方 | 有依据的内容 | 状态交接与重复替代分支 |
| --- | --- | --- |
| 监狱谈话：Map17 `(53,5)`，F661 未设 → `cs_52530` | Slade 对宝石作出反应，打开两间牢房，并面对 Sarah 关于破坏封印的指责。普通 ID 901–931、重置到 931、再至 932–946，涵盖同伴、囚犯、Slade 与叙述。铁丝/锁眼两处叙述复用 ID 931；910 的实际行动者是 133，尽管措辞可能让人以为是 Slade。 | 四次门/图块复制；Jaha 跟随 Bowie，Kazin 跟随 Jaha，Chester 跟随 Kazin，Sarah 跟随 Chester。调用方在返回后设置 F661。F661 已设则跳过场景。没有传送或 F405 写入。 |
| 楼梯警告：Map17 `(43,9)`，F662 未设 → `cs_528D4`，再到 `cs_528CA` | ID 947–951 中 Slade 劝阻走楼梯，叙述描述捷径。 | 显露 `(57,4)` 瓦片，再让 Bowie 向右移一步，然后设置 F662。F662 已设则显示固定 ID 948/951，只执行一步脚本。F661 不是显式门槛。 |
| 同行/通道：Map17 `(57,5)`，F662 已设 → `cs_52938` | ID 952–957 包含 Slade 承认责任、请求同行、叙事接受与通道/厨房方向；955 为叙述。没有 yes/no 分支或加入操作。 | 执行下述重载/隐藏/布置/跟随序列；调用方返回后设置 F73。F662 未设则直接返回，不执行它。没有 F73 一次性守卫。 |

**已确认隧道顺序：**淡出 → 以摄像机 `(24,14)` 重载当前地图 → 重置四名同伴与 Slade 的行动脚本 → 隐藏四名同伴 → 在通道布置 Bowie/Slade → 让 Slade 跟随 Bowie。脚本目的地是 Bowie `(29,18)` 和 Slade `(29,17)`。`reloadMap` 对当前地图调用 `LoadMap`，既不递归运行配置 init，也不请求地图出口。它先于隐藏/布置，而非事后重建这些实体。

`followEntity` 调用 `AddFollower`；F73 在随从初始化中另行选择 Slade。
**已确认状态区别：**两项操作都不写已加入标志 F5。叙事接受、配置在场和跟随不是军团招募或出战成员资格。隐藏同伴也不将其移出军团。所调查俘虏/守卫脚本均不授予或移除物品栏物品。宝石谈话不是物品事务。

其他监狱交互补充背景，不提供战斗解锁：同伴/囚犯台词与女佣场景 `cs_52A3C`（ID 958–961）描述附近情形。该场景取决于实体 131/F256，返回后设置 F256，并有重复台词；F256 是临时标志，在传入地图的探索入场时清除。它不是已证明的必经停留。

### 重入不会自动重建每条已开路线

**已确认选定写入：**init 仅在 F663 已设时重复四次门复制；捷径复制属于 F662 未设的警告程序。Map17 踩踏表为空，标志复制表只有 F999 控制的北门复制。因此，仅凭 F661/F662 不确立重载时自动重建这些脚本变更。工作布局、占位/随从状态及重入持久性仍为**未知**。

## 4. 城堡内容与实际 Battle05 生产者

**已确认候选连接：**Map17 无滚动传送记录包括 `(2,24)` → 同地图 `(32,9)`、Up；`(35,2)` → 同地图 `(16,9)`、Right；`(17,3)` → Map16 `(23,5)`、Right。前两条有反向记录。这些支持对话暗示的梯子/地窖/厨房连接，但不证明从通道出发或标记之间的连续行走。还有其他出口。[地图合同](../contracts/map-exploration.md#区域事件与物品)将标记/碰撞条件与传送操作数区分。

Map16 选择器为 default、F663、F530。选定早期配置时，X=17/任意 Y 区域（事件）仅在 F663 未设时调用 `cs_51F88`，之后设置 F663。
**已确认编写内容：**普通 ID 962–985 绑定 King Galam、Lemon、Slade 和人群叙述。Galam 指控 Granseal 杀死其信使；Lemon 集结军队，众人离开，Slade 谈论离去。程序隐藏 Galam/Lemon 与军队实体 129–144，不写战斗员死亡或军团移除。此处门/桥命令含音效/等待，没有显式图块复制，因此尚未确立碰撞改变。此场景不写 F405。F663 选择后续替代配置，保留守卫 145–147，共享区域（事件）/init 指针；F530 优先级更晚，不在本链范围。

### 守卫分别解锁并请求战斗

**已确认守卫调用方：**Map16 X=3/任意 Y → `Map16_ZoneEvent1`。F505 已设则立即返回。否则在测试 F664 前写 `EGRESS_MAP=16`。F664 未设时，`cs_521BA`（`0x521BA`）使用普通 ID 986–996：守卫 145 发现逃犯，Slade 与守卫 146/147 谈论 Jewel of Light，随后发出逮捕警报。此场上脚本提供实际解锁：

1. `setStoryFlag 5` 加上战斗解锁基数 400，设置 F405。
2. 顺序落入 `cs_5227C`，请求 Map16 `(0,0)`、Right、事件类型 1/参数 1 为零。
3. 脚本返回后，ASM 处理器设置 F664，先于探索分发请求。

F663 和 F73 都不是守卫处理器的前提。F505 未设/F664 已设时，只调用 `cs_5227C`，重新发布传送，不执行对话或 F405 生产者。因此传入 F664 已设/F405 未设状态不会重新启用 Battle05。Map16 init 还会在 F664 已设入场时移除守卫 145，因此任意重入标志不保证首场景的行动者人口。[对话合同](../contracts/dialogue-system.md)拥有游标/固定台词语义；[选定守卫分析](../../../research/battle04-to-battle05-content-route.md#battle05-admission-and-start-boundary)拥有行动者绑定及剧情标志算术。F405 既不是已加入 F5，也不是完成 F505。

**已确认请求消费者：**`WaitForEvent` 轮询；`ProcessMapEvent` 清除待处理类型；参数 1 为零的传送分支从探索退回 MainLoop。SwitchMap 保持 Map16 不变，随后若 F405 已设，`CheckBattle` 选择行 5。参数 1 非零会留在探索滚动分支。[请求消费所有者](../../../research/map-event-request-consumption.md)止于局部分发；已接受研究提供下游组合。

Map16 是行 5 的地图，也是唯一匹配该地图的行，因此较早解锁遭遇不能在此抢先。两个触发坐标均为通配值 255；`(0,14,24,14)` 是返回的战斗区域数据，不是会拒绝请求 `(0,0)` 的准入矩形。F405/F505 都已设会使选择器清除 F405，并仍在该次调用返回 5，尽管守卫调用方原会在 F505 已设时返回。它们是不同谓词。F663/F73 不是选择器要求。

## 5. Battle05 before 与 start 都是空程序

**已确认普通准备：**F88 未设时，BattleLoop 为 before 安装当前地图/战斗，随后恢复传入值、清除区域标志 90–105、治疗存活/不死己方角色、初始化双方名册、清除 AI 内存、加载战斗，最后调用 start。[共享开场门槛](../contracts/battle-cutscene-routing.md#共享-intro-门)在 before 检查 F455 而不设置；start 在分发前设置 F455。两个行 5 目标都是 `ms_Empty`。逮捕对话属于此前场上脚本。空终止仍有解释器等待/滚动清理，不证明瞬间完成。

F455 原已设置会跳过两个程序；F88 挂起入场绕过普通准备和这些包装器。`BattleSpriteset05`（`0x1B35A2`）声明 6 个己方布置、8 个敌人、1 个 AI 区域及 1 个 AI 点；地形行 5 选择 `BattleTerrain05`。六个布置不证明招募了六名己方角色或 Slade 参战。[遭遇合同](../contracts/battle-encounter-definition.md)和[精灵集所有者](../../../research/battle-spriteset-data.md)保留数据边界。
**未知：**自然入场、实际 Battle05 出战队伍、已完成服务/呈现及首次单次行动。Battle05 结算与后续剧情不属本指南。

## 证据审阅与剩余限制

本综合审阅了已接受研究、前篇、链接合同及下列选定公共载荷/验证器表面。选定源码结论仍归已接受研究所有者；该任务没有进行新提取或原版观察。普通对话简述使用其脚本操作数/匹配文本审阅，不是新渲染台词或独立的画面外历史。Debug/P2-Start 跳过可抑制普通文本和游标递增；固定 `txt` 调用独立。完整脚本、布局与名册仍属私有。

| 主张 | 已审阅可执行所有者与复现路线 | 处置 |
| --- | --- | --- |
| 结果与共享包装器 | [循环测试夹具](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`sf2-battle-loop-static-v1`，`lifecycleFacts.remainingCombatants`；[控制测试夹具](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`sf2-battle-control-static-v1`，`controlFacts.outcomes`；[过场测试夹具](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`sf2-battle-cutscenes-static-v1`，`cutsceneFacts`；[循环](../../../../src/sf2tool/h2/battle_loop.py)、[控制](../../../../src/sf2tool/h2/battle_control.py)、[过场](../../../../src/sf2tool/h2/battle_cutscenes.py)验证器；`uv run sf2 h2 battle-loop`、`battle-control`、`battle-cutscenes` | 共享事实受到比较，但 upgrade 布尔值不推导列表成员资格/效果。Battle04 抓捕覆盖、败北保留 d1–d3 及列表未命中使用已接受选定源码分析；没有组合运行时见证。 |
| init 与重置 | [init 测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`，路由顺序 32–37、`primarySourceBodies`、`embeddedPrograms[id=cs_5249E/cs_5247C]`；[验证器](../../../../src/sf2tool/h2/map_init.py)，`uv run sf2 h2 map-init` | 完整预期对象比较拥有条件 init 与调用方写入。F660 跳过重置/布置；F663 控制复制；F611 无条件写入。 |
| 脚本消费者/状态位置 | [脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`；[验证器](../../../../src/sf2tool/h2/map_script_engine.py)，`uv run sf2 h2 map-script-engine` | 已审阅军团状态 `$55` 处理器/源码位置 `cs_5249E:1`；转换位置 `abcs_battle04:57` 和 `cs_521BA:37`；剧情状态 `cs_521BA:36`；生命周期 `cs_52938:14:reloadMap`。调用/溯源连接不独立推导每项下游重置/随从效果。精确隧道顺序与没有加入/物品事务使用已接受源码审阅。 |
| 区域（事件）分支与配置人口 | [区域分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)，测试夹具 `sf2-map-events-static-v1`，选定 `zoneTargetPrograms` Map17 事件 0–2/Map16 事件 0–1；[验证器](../../../../src/sf2tool/h2/map_events.py)；[分片加载器](../../../../src/sf2tool/h2/map_events_fixture.py)，`uv run sf2 h2 map-events`；[配置](../../../../tests/fixtures/h2/map-setup-static-v1.json) / [实体](../../../../tests/fixtures/h2/map-entities-static-v1.json)测试夹具与[配置](../../../../src/sf2tool/h2/map_setup.py) / [实体](../../../../src/sf2tool/h2/map_entities.py)验证器；`uv run sf2 h2 map-setup`、`map-entities` | 完整区域（事件）语义保留 F662 未设时的抑制、固定重复台词、F73/F664 返回后写入及 F505 提前返回。配置/实体结构不提供自然调用或实际人口证明。 |
| 传送、请求与选择 | [内容](../../../../tests/fixtures/h2/map-content-static-v1.json)、[请求](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)、[通用地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)测试夹具；[内容](../../../../src/sf2tool/h2/map_content.py)、[请求](../../../../src/sf2tool/h2/map_event_request_consumption.py)、[地图](../../../../src/sf2tool/h2/maps.py)验证器；`uv run sf2 h2 map-content`、`map-event-request-consumption`、`common-maps` | 结构/局部分发不证明行走，也不组合守卫 → 请求 → 行 5。已接受源码审阅提供条件关系，以及 F664 已设/F405 未设、参数 1 非零和已完成行对照。 |
| 入场数据 | [路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1`；[精灵集](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`sf2-battle-spriteset-data-static-v1`；[路由](../../../../src/sf2tool/h2/battle_routing_data.py)、[精灵集](../../../../src/sf2tool/h2/battle_spriteset_data.py)验证器；`uv run sf2 h2 battle-routing-data`、`battle-spriteset-data` | `routeTargetsParsed=false` 与 `rowContentTracked=false` 保留清单限制。选定行 5 目标/头部/顺序使用研究源码分析，不是出战队伍或已完成服务证据。 |

控制器合同的 New Battle 总览把 before/start 归组过宽，不能见证其顺序位置。上方精确顺序遵循已接受选定源码分析和开场包装器所有者。旧 upgrade 标签同样收窄到调用，明确保留列表未命中因而无效果。这些差异在更严格边界下不阻塞；Layer A 不变。通用运行时队列与提议的现有所有者扩展不授权采集或新测试夹具。具名 H2 命令是复现所有者，不是本综合的新 PASS 声明；复现前在拥有该工作的进程中加载 `local/private-inputs.ps1`。

**推断：**预期的俘虏/逃脱连接。**未知：**自然结果与返回状态、考虑标记/碰撞/占位的行走、传入随从和行动者、重入重建、场景/服务完成、渲染台词/输入/音频、存档持久性及实际 Battle05 出战队伍。[研究缺口处置](../../../research/battle04-to-battle05-content-route.md#evidence-coverage-and-gap-disposition)保留缺失关系和区分性对照。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有未来范围；[剧情推进](story-progression.md)提供更广状态框架。止于 Battle05 结算之前；不消费后续未合并研究。

接着读 [Battle05 结果至 Battle06 准入](battle05-outcomes-through-battle06.md)，了解位置/HP 消费者、显式 Slade 招募、Galam 出口及条件 before/start。续篇保留结果、随从/出战队伍和自然路线限制。
