# Battle06 结果、Granseal 内容与 Battle07 准入

> 本文是[规范英文原文](../../../../synthesis/chapters/grans-opening/battle06-outcomes-through-battle07.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle06 胜利准备大地图返回；选定城镇与城堡内容描述 Galam 入侵后的 Granseal。Kiwi 可以被命名并招募，城堡事件描写 Astral 与 Lemon 前往塔楼，并添加国王及大臣为随从，塔中对话通向与 Galam 对峙。
**已确认：**这些是条件源码程序与状态消费者。**推断：**它们组成预期的返回 Granseal 和营救连接。
**未知：**自然连续走过这些内容。尤其是，Battle06 开场写 F407，胜利写 F506：遭遇准入与局部内容改变各自独立。

本英文指南的中文镜像承接 [Battle05 结果至 Battle06 准入](battle05-outcomes-through-battle06.md)。它消费[已接受分段研究](../../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md)，止于 Battle07 before/start，早于其结算。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#715](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/715) 的 Standard 严谨度协议。此处不接受可玩重制能力或新观察。

接着读 [Battle07 结果与探索交接（中文）](battle07-outcomes-and-aftermath.md)，了解胜利/重复/败北/逃脱、长塔楼场景及条件恢复探索。

下方原版源码路径、标签、地址、对话绑定和选定数字值归该研究所有者，固定至 `ShiningForceCentral/SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA 零售 ROM 身份。`Fnnn` 表示标志 nnn；源码路径相对 `disasm/`。Map3 是 Granseal，Maps19/20/21 是城堡 2F/1F/3F，Map66 是王国大地图，Maps40/57/60/58 是塔外/入口/楼梯/地下房间。这些是源码身份，不是从楼梯注释推断的楼层名。

## 1. Battle06 没有脚本指定的胜利目的地

**已确认源码条件：**败北包装器要求 Bowie 存活且敌方槽位 128 死亡。Battle06 的未完成 F506 分支选择 `ms_Empty`；敌方首领存在行为零，因此共享尾部不将其他存活敌人入队。独立首领死亡位置辅助函数的表只有 Battle05，不为 Battle06 执行位置/HP 工作。因此杀死槽位 128 不是已证明的全敌人死亡或自然胜利。剩余战斗员检查要求 X 非负且 HP 为正，Bowie 死亡时强制己方为零，且己方为零优先于敌人为零。[战斗控制合同](../../../contracts/battle/battle-control-lifecycle.md)拥有该生命周期。

**已确认结果分支：**不同于 Battle05 宝石场景，Battle06 after 目标为空。F506 原已设置则跳过该脚本，但两路都到达所消费过场加入表的行 6：零调用 Bowie 的 `JoinForce(0)`。它不是不调用的哨兵值，也不是新剧情招募。[过场合同](../../../contracts/battle/battle-cutscene-routing.md#战后路由与加入尾)保留共享尾部边界；同名旧 global 表不是被消费的表。

| 结果 | 状态/资源工作 | 返回与限制 |
| --- | --- | --- |
| 胜利，F506 未设或已设 | 治疗存活/不死己方角色；准备返回字段；执行空/跳过的 after 与共享 Bowie 加入；清除 F406、设置 F506；d4=1。 | 当前地图及首名出战成员最终战斗相对位置加区域原点，带实体朝向。普通 Battle06 使用 Map66 和原点 `(21,2)`。空 after 保留这些字段：没有固定 Granseal 传送、宝石事务、Slade 加入或后续解锁。 |
| 败北 | 更新解锁；固定文本 363；恢复 Bowie 最大 HP；通过无符号移位将金币减半；取得撤离位置；d4=-1。 | 没有 after/加入或新 F506。前篇选定 `EGRESS_MAP=16` 且 F399 已设时：Map16 `(27,7)`、Up。其他传入存档点状态改变目的地。不运行 Battle04 抓捕/upgrade 尾部。 |
| Egress / Angel Wing | 消耗法术 MP / 移除选定物品槽，调用逃脱呈现，更新解锁并取得撤离位置；d4=0。 | 相同的条件存档点；没有败北金币惩罚、after/加入或 F506 写入。 |

只有 F506 原已设置，`UpdateBattleUnlockedFlag` 才清除 F406；F506 未设时，败北/逃脱使其保持启用。BattleLoop 设置 F399，Battle06 没有特殊撤离例外。F399 未设的辅助调用使用游戏开始坐标；未列出撤离地图默认为 `(1,1)`、Up。这些源码替代分支不确立自然重复或逃脱路线。

胜利准备的地图事件字段作为战斗返回被消费。`MainLoop` 的 `alt_MainLoopEntry`（`0x75E0`）运行 `SwitchMap` 后进入探索，不重新调用 `CheckBattle`。探索在入场时清除事件类型，并治疗符合资格的存活/不死己方角色，不复活每名死亡己方角色。保留的眩晕/中毒/诅咒掩码保留已有位，不添加状态。后续出口产生自身请求与遭遇检查。

### F407 来自入场，F506 来自胜利

**已确认：**`bbcs_06:45` 是 `setStoryFlag 7`，其处理器加 400 并设置 F407。选定 Battle06 结果路径均不清除它。F456 原已设置会跳过 before/start；F88 选择挂起入场，也绕过开场。因此入场被跳过时，F407 是输入，不是从战斗编号作出的假设。[前篇](battle05-outcomes-through-battle06.md#6-battle06-战前场景在入场时写入-f407)拥有该入场边界。

| 传入状态 | 条件后果 |
| --- | --- |
| F407 已设、F506 未设，包括执行开场后逃脱 | 不选择所述战后城镇/城堡配置。行 7 准入有解锁输入，但物理可达仍未知。 |
| F506 已设、F407 未设 | 可以选择局部内容改变，但 Map58 不选择 Battle07。 |
| F506 与 F407 已设 | 内容改变与行 7 解锁可共存，受下方地图/配置/请求条件约束。两者都不确立合法行走。 |

较旧 `battle4DefeatUpgradesBattle=true` 测试夹具措辞记录一次 `UpgradeBattle` 调用。其随机战斗列表不含 Battle04/05/06，因此不证明对它们升级、递增战斗 ID 或后续解锁。此更窄已接受边界保持完整。

## 2. Granseal 改变后的内容含可选 Kiwi 招募

**已确认配置选择：**`GetCurrentMapSetup` 使用源码顺序最后匹配的已设标志。此处内容选择 F506，要求 F543 未设；Map19 还要求 F507 与 F982 未设。较早 F501/F609 不覆盖 F506。胜利不抹除后期标志，因此任意/重复输入可能选中其他配置。[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)拥有加载与选择顺序；文件名和注释不是配置谓词。

Map3 的 F506 选择损坏兽笼图块复制、仅默认项的区域（事件）表、变化的行动者及悲伤音乐/淡入淡出 init 脚本。已加入标志 F6 已设时，init 隐藏 Kiwi。
**已确认编写文本：**选定居民 ID 665–673 与 678 描述士兵进入城堡、暴力和破损笼子；俯卧行动者共享死亡叙述。这些台词确立游戏说了什么，不是对每项画面外事件的独立观察。

### 同意命名通向独立加入

**已确认处理器顺序：**`Map3_1FA_EntityEvent15`（`0x51256`）在 F6 原已设置时返回。否则 ID 674–677 呈现受惊动物与命名邀请。非零回答返回；零调用 `j_NameAlly(6)`，继续对话并调用 `cs_512BA`。命名本身不是成员资格事务。该程序命令 4 是 `join ALLY_KIWI`；命令 5 是 `addNewFollower ALLY_KIWI`。

| 消费者 | 状态效果与边界 |
| --- | --- |
| `csc08_joinForce` | 普通 `j_JoinForce(6)`、职业查找、带名字/职业替换的固定文本 446，之后等待/关闭。固定加入消息独立于场景对话游标。 |
| `JoinForce` / `JoinBattleParty` | 设置已加入 F6，通过 `UpdateForce` 重建，再检查重建出战数量是否达到 12。低于容量时设置出战 F38；满队不强制 Kiwi 出战。加入不恢复 HP。 |
| `csc56_addFollower` / `AddFollower` | 将当前实体追加在探索列表最后随从之后，获准时安装当前跟随行动。这不在 `table_Followers` 创建持久 Kiwi 行，也不设置军团标志。 |

[成员资格合同](../../../contracts/party/party-membership-state.md#joinforce-时间线)要求重建先于激活。`JoinForce` 中没有第二次 `UpdateForce`；设置 F38 不证明辅助函数返回时出战前缀/数量已同步。合同保留符号范围和容量；上述数字 Kiwi 标志/容量是分段研究的选定源码事实。[脚本名册合同](../../../contracts/party/party-roster-state.md#处理器边界)另行拥有加入与 `addNewFollower` 处理器时序。

Kiwi 场景在场、选定名字、已加入成员资格、出战标志、重建列表和当前跟随是独立事实。自然交互、加入后 HP/出战队伍同步、可见跟随和重入重建仍为**未知**。可选招募不是 `CheckBattle` 前提。

## 3. 城堡区分 Astral 离开与王室跟随

**已确认编写内容：**Map19 F506 NPC ID 678–689 描述 Galam 魔法、Astral 咒语及 Elis 被带走，不解锁 Battle07。其两个区域（事件）调用位于 `s3_zoneevents_507.asm` 的处理器，尽管选定表是 `s3_zoneevents_506.asm`。`507` 后缀不施加 F507 检查。

| 条件调用方 | 内容与实际有序效果 |
| --- | --- |
| Map19 `(255,15)`，`Map19_1FA_ZoneEvent0`，F620 未设 | 运行 `cs_534D2`（ID 694–712）：Astral 质问 Lemon 命令与国王身份；Elis 呼喊；Astral 说服 Lemon 前往塔楼。命令 56–59 隐藏 Astral 128、Lemon 和守卫 132/133。脚本返回后处理器设置 F620。没有 Lemon 加入。F620 已设则跳过场景。 |
| Map19 `(25,12)`，`Map19_1FA_ZoneEvent1`，F621 未设 | 运行 `cs_535FA`（ID 690–693）：大臣 129 指认 Ancient Tower，国王 130 决意救 Elis。命令 5/6 先请求国王 → Bowie，再请求大臣 → 国王跟随。脚本返回后先写 F83，再写 F621。此处理器不测试 F620。 |
| 相同大臣调用方，F621 原已设置 | 跳过整个脚本和两次写入。此处不修复传入的 F621 已设/F83 未设状态。源码局部独立于 F620，不证明玩家可绕过首个区域（事件）。 |
| 重入时 Map19 F506 init | F620 隐藏 Lemon、Astral 与两名守卫；F621 隐藏大臣/国王。这些抑制原场景行动者，本身不重建任意当前随从。 |

跟随请求对当前实体使用 `csc2C_followEntity` 的角色存活门槛与 `AddFollower`。F83 在 `data/scripting/entity/followers.asm` 有独立持久行：`FOLLOWER_A` 国王及 `FOLLOWER_D` 大臣。`InitializeFollowerEntities` 测试每行标志；`InitializeFollowerActscripts` 为获准行分配脚本。两者都不加入交战军团。因此，同行叙述、当前跟随行动、F83 准入和完成跨地图跟随必须保持区分。多个/冲突传入随从标志及实际行动者/移动状态仍为**未知**。

**已确认后续局部内容：**Map20 的 F506 init 直接指向 `ms_map20_flag501_InitFunction`，绕过旧卧室开场。Map21 同样在较早守卫位置脚本之后进入 `ms_map21_flag609_InitFunction`。这些入口仅在 F507 已设时调用共享音乐/淡入淡出脚本。其 F506 实体事件表只有默认项：仅因实现共享文件，不会选择旧 Map21 守卫谈话及 `setStoryFlag 1`。复用入口标签不恢复其前面的场景。

## 4. 塔楼进路是条件请求链

**已确认选定出口：**下表每行是 `warpNoScroll` 记录，表示若该标记产生请求会发生什么，不确立从一个抵达点走到下一标记；255 表示坐标通配值。[请求所有者](../../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology)闭合局部轮询/清除接缝，[分段请求审阅](../../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#conditional-onward-map-requests)拥有选定源码组合。

| 请求地图 / 标记 | 目的地 / 朝向 | 内容或准入条件 |
| --- | --- | --- |
| Map66 `(30,32)` | Map3 `(1,27)`、Right | F640 必须未设才保留 Map3；否则 `SwitchMap` 替换为地震地图。F506 选择城镇内容，不改变本记录。 |
| Map3 `(255,1)` | Map19 `(26,30)`、Up | 城堡 F506 配置与上方独立区域（事件）调用方。 |
| Map19 `(6,2)` | Map20 `(6,37)`、Right | 选定西塔路段；其他城堡出口为独立请求。 |
| Map20 `(3,36)` | Map21 `(3,16)`、Right | 独立的下一西塔路段。 |
| Map21 `(9,1)` | Map40 `(4,30)`、Up | 塔外；仅在后期配置标志允许时，F506 才选择 Lemon 背景内容。 |
| Map40 `(255,12)` | Map57 `(8,18)`、Up | 此选定非战斗继续需要 F401 未设；否则即使 F501 已设，行 1 也可拦截。 |
| Map57 `(255,4)` | Map60 `(53,1)`、Right | F506 入口图块复制与北进路重叠；合法通行未解决。 |
| Map60 `(255,51)` | Map58 `(13,36)`、Up | 独立楼梯标记，需要 F999 未设才保留有效 Map58。 |

`ProcessMapEvent` 清除待处理事件类型。类型 1 参数 1 为零，从探索退回并向 MainLoop 提供地图/X/Y/朝向；参数 1 非零则走滚动分支。MainLoop 在 `CheckBattle` 之前调用 `SwitchMap`，只有未选中遭遇才进入有效目的地的探索配置/init。此顺序对 Map57 和 Map58 有影响。

**已确认塔楼内容：**Map40 F506 放置俯卧的 Lemon 与两名守卫。后期 F507 改选默认配置；F506 init 仅在 F507 已设时调用音乐/淡入淡出，默认区域（事件）直接返回。Lemon 的 `cs_53FE0`（ID 997–999）催促营救，描述恶魔与黑云；守卫 ID 1000 使用死亡叙述。使场景行动者颤抖/停止，不是交战 HP/死亡写入或军团加入。此处没有 F620 检查，不能独立证明 Lemon 画面外旅程。

**已确认布局数据，后果有限：**F506 已设时，Map57 从 `(17,0)` 向 `(7,4)` 复制 `2 x 1` 图块。它与北传送进路重叠，但既不证明可通行，也不证明必经门槛。Map60 标志复制依赖 F999，其局部楼梯/传送门记录不闭合从 `(53,1)` 到 Y=51 的通行。Maps19/20/21/40 标志复制表为空；Map19 有两次局部踩踏复制。此处不重建完整工作布局、碰撞、占位或随从路线。

Map66 本身也有条件：默认区域（事件）调用 `CheckRandomBattle(3)`，可按 F503、步数计数器与 RNG 设置 F403 并请求重载。其他具名区域（事件）有依赖后期 F537/F538/F539 的消费者。因此，不证明从可变 Battle06 胜利位置走到 Granseal 不会遭遇战斗。这些消费者保留路线限制，不开启后期战役研究。

## 5. Battle07 准入测试有效地图与 F407

**已确认选择器：**`CheckBattle`（`0x799C`）按升序扫描行。行 7（`0x7A67`）使用 Map58、区域 `(0,0,28,38)` 及通配触发 `(255,255)`；没有较早行使用 Map58。有效地图为 58 且 F407 已设时，不论请求 X/Y 都选择战斗 7。矩形是返回的战斗几何，不是包含测试。F507 已设会在选择后清除 F407，仍在该次调用返回战斗 7。[遭遇合同](../../../contracts/battle/battle-encounter-definition.md#地图坐标与触发元数据)保留元数据边界。

| 反例输入 | 条件源码结果 |
| --- | --- |
| F506 已设、F407 未设 | 不选择行 7；完成/内容不替代解锁。 |
| Map58 请求，F999 已设 | `SwitchMap` 在检查前替换为 Map59；原请求地图不确立 Battle07 准入。 |
| 较早 Map57 请求，F401 已设 | 即使 F501 已设，行 1 仍选择 Battle01；其完成状态在选择后清除解锁，而非事先拒绝战斗。 |
| 有效 Map58，F407 与 F507 已设 | 仍发生一次 Battle07 选择，之后清除 F407。 |

F506、F620、F621、F83 和 Kiwi 成员资格均不在 `CheckBattle` 行 7 谓词中。其缺失不证明物理可达、可玩绕过，或传入标志组合能自然取得。已接受连接是条件源码组合；现有验证轨道没有以可执行方式将开场 → 保留的结果标志 → 选定请求 → 有效地图 → 遭遇闭合为连续旅程。

## 6. Battle07 before 清除王室跟随并布置对峙

**已确认选定 before/start：**before 行 7 指向 `bbcs_07`（`0x49CE2`）。F457 未设的新入场中，命令 1 清除 F83，命令 2 布置 Map58/摄像机 `(8,29)`，命令 3 加载 `ce_49F36`。摄像机坐标不是玩家传送。Bowie 被布置在 `(13,35)`，同场有 Galam、Astral、Elis、国王、大臣及三名敌方场景行动者。其在场不要求先前成功的 F83 随从观察，也不确立交战名册。

**已确认编写对话：**普通 ID 2357–2384 描写 Galam 威胁 Elis、Astral 与 Galam 对峙并相互定身、Elis 逃跑受阻、Astral 请 Bowie 营救她，以及 Galam 召唤攻击者。这是在为遭遇作铺垫；程序没有新军团加入或后续 `setStoryFlag`，本指南不解释其结果。

BattleLoop 在 before 后恢复传入地图/战斗、清除区域标志 90–105、治疗符合资格己方角色、初始化己方/敌方位置、清除 AI 内存并加载战斗，之后调用 start。行 7 start 目标为 `ms_Empty`；包装器在分发前设置 F457。空终止保留解释器清理/等待，不证明瞬间完成。F457 原已设置会跳过两个脚本，包括 F83 清除。F88 挂起入场恢复保存秒数、清除 F88/AI 内存并重载战斗状态，绕过新战斗初始化及 before/start。[开场合同](../../../contracts/battle/battle-cutscene-routing.md#共享-intro-门)拥有包装器门槛；[选定入场审阅](../../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#battle07-admission-and-beforestart-stopping-boundary)拥有其顺序位置和 Battle07 目标。

`BattleSpriteset07`（`0x1B3720`）声明 7 个己方布置、10 个敌人、4 个 AI 区域及 1 个 AI 点。初始化消费实际已加入/出战/存活军团：七个布置不强制 Kiwi 出战，也不证明七名己方存活。止于此选定 before/start 边界。Battle07 结算、其后长场景及之后战役为独立工作。

## 证据审阅与剩余限制

综合前审阅检查了已接受研究、前篇、相关合同、选定公共载荷及下列验证器比较。没有新提取、源码采集、运行时观察或 H2 重跑。研究所有者提供选定原版源码组合、行动者/文本含义、精确记录/目标及固定溯源/复现凭据。普通文本简述假设未跳过分支；debug/P2-Start 可抑制普通文本/游标递增，固定 `txt`/加入消息独立。此处没有见证渲染台词、音频及完成的输入等待。

| 主张 | 已审阅可执行所有者与复现路线 | 处置 |
| --- | --- | --- |
| 结果、败北/after 与成员资格尾部 | [过场测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，ID `sf2-battle-cutscenes-static-v1`，`cutsceneFacts.intro/afterBattle/enemyDefeated/leaderDeathPositions`；[控制测试夹具](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)，ID `sf2-battle-control-static-v1`，`controlFacts.mainLoop/outcomes`；[过场](../../../../../../src/sf2tool/h2/battle_cutscenes.py)与[控制](../../../../../../src/sf2tool/h2/battle_control.py)验证器。`uv run sf2 h2 battle-cutscenes`、`battle-control`。 | 通用守卫与结果事实不确立选定行 6 或自然胜利。研究提供空目标、首领/位置表缺失和可变返回组合。旧 upgrade 布尔值是调用，不是对这些战斗的效果。 |
| F407/F83、Kiwi 加入与跟随 | [脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，ID `sf2-map-script-engine-static-v1`，`storyStateCommandFacts`、`forceStateCommandFacts`、`entityGestureRelationshipMotionCommandFacts`；[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)。[属性测试夹具](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)，ID `sf2-common-stats-static-v1`，`statsFacts.party`；[属性验证器](../../../../../../src/sf2tool/h2/stats.py)。`uv run sf2 h2 map-script-engine`、`common-stats`。 | 位置 `bbcs_06:45`、`bbcs_07:1`、`cs_512BA:4` 与处理器守卫分别保留写入/加入。当前跟随及持久 F83 行使用已接受消费者审阅。四项属性事实不证明激活后同步。 |
| Kiwi 同意与城堡调用方顺序/重入 | `sf2-map-events-static-v1` 的[实体](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)与[区域](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)分片，程序顺序 467 和 31/32；[事件验证器](../../../../../../src/sf2tool/h2/map_events.py)。[init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，ID `sf2-map-init-static-v1`，`primarySourceBodies`；[init 验证器](../../../../../../src/sf2tool/h2/map_init.py)。`uv run sf2 h2 map-events`、`map-init`。 | 完整语义比较保留命名先于脚本、F620 返回后写入、F83 先于 F621 和 init 隐藏。F506 表中以 507 命名的实现不是 F507 谓词。自然调用与随从重建仍未知。 |
| 请求、替换、遭遇与图块复制 | [请求测试夹具](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，ID `sf2-map-event-request-consumption-static-v1`；[通用地图测试夹具](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)，ID `sf2-common-maps-static-v1`，`mapFacts.mapSwitch/battleTrigger/egress`；[内容测试夹具](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，ID `sf2-map-content-static-v1`；[请求](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)、[地图](../../../../../../src/sf2tool/h2/maps.py)与[内容](../../../../../../src/sf2tool/h2/map_content.py)验证器。`uv run sf2 h2 map-event-request-consumption`、`common-maps`、`map-content`。 | 局部消费者/清单不组合连续路线，也不确立合法行走。研究提供选定记录和 F401/F999 对照；图块复制存在不是碰撞/可通行性证据。 |
| before/start 与遭遇布置 | [路由测试夹具](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，ID `sf2-battle-routing-data-static-v1`，`facts.routeTargetsParsed=false`；[精灵集测试夹具](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，ID `sf2-battle-spriteset-data-static-v1`，`facts.rowContentTracked=false`；[路由](../../../../../../src/sf2tool/h2/battle_routing_data.py)与[精灵集](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)验证器。`uv run sf2 h2 battle-routing-data`、`battle-spriteset-data`。 | 精确目标/头部/布置使用研究源码审阅，而非这些清单。F457/F88 跳过与七个布置对实际出战队伍的区别限定主张。 |

控制器合同 New Battle 总览对 before/start 的归组比选定顺序更宽；使用上方精确包装器/分段顺序。成员资格合同有意保留符号值，不闭合同步列表。请求所有者历史 H3 问题队列及研究可能的现有所有者扩展是未知项处置，不是获准采集。这些更窄边界与可执行所有者一致；本指南不导出 Layer A 修复或新测试夹具主张。

**推断：**连接 Granseal 与塔楼的预期营救旅程。**未知：**自然结果及传入标志/存档点/调用方/行动者/HP/出战队伍状态；合法行走、随机遭遇、工作布局碰撞与占位、塔楼/楼梯可达或绕过；重入和冲突随从；服务完成与呈现/输入时序；持久性和可获胜性。[研究缺口所有者](../../../../../research/chapters/grans-opening/battle06-to-battle07-content-route.md#existing-coverage-and-gap-disposition)保留其区分性对照。具名 H2 命令是复现路线，不是新 PASS 声明；启动前在同一进程加载 `local/private-inputs.ps1`。不消费未合并 Battle07 结果结论，指南不增加重制、UI 或翻译支持。
