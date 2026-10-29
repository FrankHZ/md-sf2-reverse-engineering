# 损坏船只木板选择至 Battle08 准入

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Bowie 被要求把木板带到甲板上，一名工人随后请他帮助寻找在 North Cliff 失踪的朋友。**推断的叙事连接：**搬运并交付木材引出救援旅程与一次遭遇。**已确认静态内容：**第一次选择可以不产生请求而返回；甲板上的选择则重复到回答 Yes 为止。该场景设置 F408 并请求 Map72，其首个符合条件的遭遇是 Battle08。**未知：**自然完成的路线、实际行动者/随从、请求存续，以及移动、文本、摄像机、淡入淡出或音频服务的完成。

本文承接[航行登陆至损坏船只控制](../grans-opening/voyage-through-damaged-ship-control.md)。已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md)提供固定源码、文本、内联动作及调用方审阅。本指南支持内容/设计规划，止于 Battle08 start 分发器返回，紧邻 `ActivateEnemies` 之前。交战、结果与城镇建设属于后续分段。本文不增加重制能力。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#735](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/735) 的 Standard 严谨度协议。

接续[Battle08 结果至 New Granseal 城堡交接](battle08-outcomes-through-new-granseal-handoff.md)，了解有条件的工人后续、建设场景与发出的城堡请求。

`Fnnn` 表示标志 nnn。源码引用使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。命令索引是从零开始的脚本记录，不是帧数或实测时长。对话摘要和叙事解释不提供额外玩法谓词。

## 1. 回应木板请求

**已确认起始边界：**前篇从 Map73 init 返回时，当前地图为 Map14，实体群来自脚本 `ce_5079E`。它没有运行 Map14 init。Bowie 的创作位置为 `(40,8)`、Left；物体 131–133 位于 `(44,6)`、`(45,6)`、`(46,6)`，Jaha 已被隐藏。实际抵达位置、F6、随从映射、玩家类型，以及首次待处理事件/控制读取仍受前篇边界限制。这是起始场景上下文，而非普通 Map14 配置实体群。

若移动目标的工作地图事件位经 `$3C00` 掩码后等于 `$1400`，就产生区域（zone）事件类型 6 与目标瓦片坐标。该产生方既不检查 F257，也不证明 Bowie 能合法到达 `(45,5)`：碰撞、占位、变化的图块与受控实体更新仍是输入。`ProcessMapEvent` 在分发前清除待处理字；zone 选择器选取首个坐标匹配项，允许 255 通配符，否则选择默认项。

F458 未设时，Map14 默认 zone 表在 `(45,5)` 选择 `Map14_ZoneEvent0`。它运行 `cs_58E5C` (0–15)，然后设置 F257。产生方、选择器、处理器和脚本均不以 F257 防止重复。F458 则选择另一张 zone 表，其中非默认的 `(255,15)` 行是独立内容；在该配置下，同一 `(45,5)` 查询不提供本次木板选择。

文本询问 Bowie 是否愿意把木板带上去。每次调用 `yesNo` 都覆盖 F89：Yes/返回 0 设置它；No 或 B 取消清除它。菜单初始选中 Yes，Left/Right 选择 Yes/No，A/C 确认，B 返回 No。预先提供的 F89 不能回答新提示。

| 回应 | 已确认有序结果 |
| --- | --- |
| No 或 B 取消 | F89 未设，跳至结尾。随后没有脚本移动或传送。调用方仍设置 F257。其 zone 尾部关闭立绘/文本，等待一次 VInt，再等待实体 0 停止；之后待处理值为零可重新安装控制，实际就绪未知。 |
| Yes | 隐藏文本；等待 Bowie Down 1；等待 40；分别等待在 131、132、133 上禁用自动朝向。不等待完成地启动 Bowie 与物体 131/132 Down 3；等待 133 Down 3，然后请求显式 Map14 `(13,11)`、Down。 |

**推断的布置：**物体组表现木板被搬动。这些移动不产生物品获取或物品栏携带状态。等待物体 133 不能独立证明其他三个行动者已完成。禁用自动朝向会跳到 idle；后续不可达的动作指令不增加加速效果。

传送写入共享的待处理类型 1 槽位，不立即加载 Map14。脚本返回、调用方 F257 写入及 zone 尾部服务都先于后续待处理读取/使用方。其他写入方可在这些服务期间覆盖请求。若它存续，无滚动使用方就将其元组返回 MainLoop。[木板事件审阅](../../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#starting-population-and-plank-event)拥有此顺序，以及可选回应与观察到的控制恢复之间的区别。

## 2. 甲板入场取决于配置与 init 状态

**已确认入场区别：**接受回应请求显式地图 14，尽管它已经是当前地图。Map14 没有替换行或遭遇行。显式探索入场重建选定实体、加载精灵、清除配置标志 256–383（包括 F257），并在加载地图和运行配置 init 前设置 F80。默认固定列表为空，但随从初始化仍会发生。init 前没有玩家控制交接。

同图楼梯则使用 `MAP_CURRENT`：甲板触发点 `(12,11)`–`(14,11)` 通向舱内 `(44,8)`–`(46,8)`、Up；舱内触发点 `(44,9)`–`(46,9)` 通向甲板 `(12,12)`–`(14,12)`、Down。CheckBattle 扫描当前 Map14，但无战斗匹配时保留哨兵值。该入场分支更新主实体/木筏，不重建配置实体或清除临时标志。**两个入场分支都会运行配置 init。**出生坐标也出现在传送表中，并不会自行触发第二次传送。

| init 选择时的状态 | 已确认后果 |
| --- | --- |
| F458 未设，F703 未设 | 默认 init 运行 `cs_58FA4`，然后设置 F703 并写入 `EGRESS_MAP=14`。没有 F257、携带物品或入场坐标测试。 |
| F458 未设，F703 已设 | 在场景及两次调用方写入前返回。显式入场保留空固定配置与获准随从；不会恢复木板物体。 |
| F458 已设，F703 任意 | 替代 init 仅返回。其显式入场固定实体群为一名祭司和一名工人，加上获准随从；它替代首次交接内容。 |

因此，在 F458/F703 未设时，同图楼梯或直接入场可不经木板接受测试而选择甲板场景。这是强制物品栏任务的条件反例，不是合法步行捷径或任意标志可自然达到的证据。F703 的源码注释还提到 Ribble：设置它不是本次交付的唯一证明。[入场/init 审阅](../../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#explicit-map14-entry-versus-stairs-and-repeat-entry)拥有这些替代情况。

## 3. 木材交付转为救援请求

**已确认场景内容：**Map14 `mapsetups/scripts_2.asm` 中的 `cs_58FA4` 以视野 `(8,7)` 重载地图，然后加载 `ce_59270`。Bowie 的创作位置为 `(13,11)`、Down。商人 128、青年 129、男子 130 和工人 131 现在与物体 **132–134** 同场；旧舱内物体范围是 131–133。逻辑引用使用重建后的实体索引映射，而非分配顺序。

加载列表会清除实体、初始化随从并声明固定记录/主实体。已作为随从映射的己方角色会在固定列表中被跳过。Sarah、Chester、Jaha、Kazin、Slade 和 Kiwi 均有场景声明，但标志/玩家类型仍影响实际准入。此脚本没有 F6 测试：即使 F6 未设，也声明并指挥 Kiwi。这些命令既不招募他，也不建立战斗资格。

| 面向玩家的阶段 | 已确认源码顺序及解释边界 |
| --- | --- |
| 准备甲板 (0–9) | 等待 Bowie init；向 Bowie、男子 130 和中间物体 133 赋予优先级；淡入。不等待完成地启动青年 129 的独立自定义行走/等待动作。 |
| 递交木材 (10–43) | 商人求助；男子 130 回应并靠近。不等待完成地移动物体 132/133 Down 1，并等待 134；显示交付叙述。不等待完成地移动男子及 132/133 Down 7，并等待 134；隐藏男子和全部三个物体，然后重新定位商人。 |
| 听取救援请求 (44–68) | 商人哀叹 Granseal；工人 131 求助。等待其速度 48 配置和 Up-6 移动，然后初始化回速度 32。工人做手势并描述在 North Cliff 失踪的朋友；商人作出反应并向 Bowie 说话。 |
| 接受任务 (69–75) | 重置游标 2275，请 Bowie 调查，提示 Yes/No。Yes 分支继续；No/取消显示两句回应并回到此提示。每次重试都重置游标并覆盖 F89。没有拒绝后返回控制的路径。 |

**推断的叙事含义：**木材交付后，工人要求 Bowie 陪同救援朋友。与第一次木板选择不同，第二次拒绝是对话循环。实际操作没有给予/移除物品、军团加入/离开或 HP 赋值。隐藏男子/物体会移除场景实体及映射，不转移物品栏或战斗员。动画停止/启动不改变健康状态，优先级写入存储布尔 0/1，而非带符号高度或已观察到的重叠。

程序没有图块复制命令。重载通过 LoadMap 更新地图/视野，不重新进入准入或运行另一次配置 init。先前舱内布局单元不是物品栏。文本、淡入淡出、摄像机运动与未等待的青年动作各自保留完成依赖。[完整甲板审阅](../../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#above-deck-script-and-actor-boundary)拥有命令、游标重置、内联动作与引用变化。

## 4. 出发先产生遭遇请求，再进行调用方写入

**已确认有序出发：**回答 Yes 后，商人赞扬 Bowie 并指派工人当向导。商人/工人移动并说话；工人和 Bowie 不等待完成地启动 Down 4，同时等待 Jaha Up 5。Jaha 呼喊，Sarah 评论他湿透的样子 (76–94)。外观和对话都不提供 HP 变化，也不证明本场景之外他遭遇了什么。

脚本随后将 Kazin、Chester、Slade、Sarah 和 Kiwi 沿 X13 放在 Y10/9/8/7/6，朝 Down。它启动各自路径，等待 Kiwi Down-7 动作，然后赋予朝向 (95–112)。Sarah、Kazin 和工人讨论陪伴 Bowie。一次等待和中性摄像机后，工人、Bowie、Sarah、Chester、Jaha、Kazin 和 Slade 不等待完成地启动 Down 7；Kiwi 的 Down 7 被等待 (113–126)。最后等待 Kiwi 不是证明每个行动者或摄像机服务都已完成的屏障。

向外传递的状态顺序精确如下：

1. 命令 127 `setStoryFlag 8` 加上 400，设置 **F408**。它不设置 F8、F458 或 F508。
2. 命令 128 写入待处理 Map72 `(1,1)`、Down；命令 129 结束脚本。
3. 仍在运行的 Map14 init 调用方设置 **F703**，然后写入 **EGRESS_MAP=14**。
4. 后续非零待处理读取跳过控制安装。若此请求存续，其类型 1 使用方将元组返回 MainLoop 以供准入。

F703 和 EGRESS_MAP 在请求产生后、待处理读取/消费前写入。EGRESS_MAP 存储目标输入，不建立已执行逃脱或未来战斗结果。待处理槽位被清除/覆盖会中断此条件连接。独立的 Map14 出口 `(255,20)` 请求 Map72 `(1,1)`、Up，不写故事标志；若 F408 已设，它也可准入 Battle08。两条路线都不证明自然步行或特定标志历史。

## 5. Map72 选择首个符合条件的战斗

**已确认准入：**Map72 没有替换行。`CheckBattle` 按战斗索引升序扫描。行 8 指定 Map72、通配触发点 `(255,255)` 和输出战斗区域 `(1,32,32,30)`。较早的行 0–7 指定其他地图，因此 F400–F407 不拦截此请求。较后的 Map72 行 24/26 不能先于符合条件的行 8。F408 已设时，`(1,1)` 即使在区域矩形外也选择 Battle08：矩形是输出几何范围，不是包含性测试。朝向不是谓词。

| 相关输入 | 本次调用的已确认结果 |
| --- | --- |
| F408 已设，F508 未设 | 选择 Battle08。 |
| F408 已设，F508 已设 | 选择 Battle08，并在同一次调用中清除 F408；完成状态不拒绝已经选中的遭遇。 |
| F408 未设 | 跳过行 8；较后已解锁的匹配行可胜出，否则返回无战斗。 |

F257、F703 和 F458 不是 CheckBattle 谓词。仅有产生标志的脚本不能证明 MainLoop 已消费其请求。[准入/before-start 审阅](../../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#map72-admission-and-battle08-beforestart)拥有精确行与调用方边界。

## 6. 开场场景与交战名册分离

MainLoop 将战斗索引 8 传给 BattleLoop，后者清除玩家类型并设置 F399。**已确认普通路径，F88 未设：**清除秒数，保存传入地图/战斗，写入当前 Map72/Battle08，安装基础 VInt 并运行 before 分发器。F458 未设选择 `bbcs_08`；F458 已设则跳过。before 分发器返回时**不**设置 F458。

before 场景加载 Map72/视野 `(1,46)` 后从 `ce_4AA14` 重建。主实体位置为 `(4,50)`、Right，固定声明 Sarah/Jaha/Kazin/Chester/Kiwi。Slade 没有固定记录；任何随从映射都是独立输入。工人 128 是向导；工人 129 是朋友。女巫 130 与生物精灵 131–133 构成另一场景组。此列表描述布置，不是交战名册或保证实际存在的角色群。

| 开场阶段 | 已确认内容与限制 |
| --- | --- |
| 布置朋友 (0–9) | 等待 Bowie init；停止工人 129 的动画，并等待 motion-off/Left 朝向/精灵更新。F6 未设将 Kiwi 移到 `(63,63)`、Down；F6 已设跳过该命令。两者都请求 Main Theme/淡入。重新定位不是隐藏或军团离开操作。 |
| 发现麻烦 (10–17) | 向导靠近并指出朋友；摄像机瞄准 129，对话描述其位置，随后请求 Enemy Attack 音乐和另一个摄像机目标。 |
| 布置袭击者 (18–30) | 依次放置 goblin/green ooze/hunter goblin 131–133，再放置女巫 130，中间有等待。文本操作数 25–28 全部指定 `$C0,130`，包括询问伤害工人的那句；叙事直觉不能重新分配该说话者。将女巫朝 Up，并发出攻击对话。 |
| 结束介绍 (31–34) | 不等待完成地启动 131/132 跳跃动作，并等待 133 跳跃，随后返回。每个动作禁用自动朝向，带等待地请求 Y -1/+1，恢复自动朝向并回到 idle。 |

**推断的布置：**朋友看起来俯卧，敌对生物与队伍对峙。抑制运动和动画不赋予工人 HP，也不证明死亡。场景内 Map72 加载既不调用 MainLoop 准入，也不调用 Map72 配置 init；具名等待/音乐/淡入淡出/摄像机请求不等于已独立观察到呈现完成。

before 分发器返回后，普通 BattleLoop 恢复/写入保存的地图/战斗，清除区域标志 90–105，治疗存活/不死己方角色，初始化己方/敌方战斗位置，清除 AI 内存并调用 LoadBattle。交战放置使用更新后的出战队伍、Battle08 精灵集及己方 HP 检查。它不将场景 NPC 或每个声明的己方角色提升为交战成员。LoadBattle 定位实体、加载地图/精灵/地形、安装 VInt、请求地图音乐并淡入；此处运行时完成仍未知。

start 分发器随后检查同一个 F458。若未设，它在调用 Battle08 start 表目标 `ms_Empty` **之前设置 F458**；若已设，跳过该目标。因此首次普通入场有 before 场景和空 start 脚本，而 F458 已设的重入跳过两脚本，仍执行普通战斗初始化/加载。两个分发器都不招募任何人。后续查询时，F458 还选择 Map14 替代配置。

**已确认替代调用方：**F88 已设走挂起战斗路径：恢复秒数、清除 F88、清除 AI 内存、重置摄像机目标并 LoadBattle，然后直接跳到单次行动循环。它绕过 before/start 分发与普通位置初始化。它既不证明所提供挂起状态有效，也不建立每次 BattleLoop 入场都会设置 F458。此处没有执行交战。普通路径的停止边界是 start 分发器返回，**在 ActivateEnemies 之前**。

## 证据审阅与剩余限制

本综合审阅了已接受 #732 及其航行前篇、相关合同、选定已跟踪测试夹具及其验证器/schema 边界。研究拥有完整固定源码、文本、内联列表/动作审阅和保留比较。本文不增加源码调查、提取、比较重跑或运行时观察。下方窄命令指明复现所有者，而非本指南新执行的检查。

| 关系 | 已审阅可执行所有者与复现路线 | 边界 |
| --- | --- | --- |
| 木板选择器与 F257 | [zone 分片](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，`sf2-map-events-static-v1`，程序 `Map14_ZoneEvent0:364112`；[分片/schema 所有者](../../../../../../src/sf2tool/h2/map_events_fixture.py)；[配置测试夹具](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`。命令 `uv run sf2 h2 map-events`、`map-setup`。 | 处理器包含脚本 → F257 → 返回，没有局部守卫。已接受研究拥有所需产生方/菜单/选择器审阅和 F458 反例；不产生自然可达瓦片/重复观察。 |
| 入场与待处理使用方 | [common-maps](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[map-content](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[请求消费](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`。命令为 `uv run sf2 h2` 下相同 basename。 | 独立源码表、写入方和使用方与显式/哨兵值入场一致；请求存续、步行和控制读取值为零仍未知。 |
| init 与脚本效果 | [init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`，主函数体 19/20；[验证器](../../../../../../src/sf2tool/h2/map_init.py)、[schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)；[脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`；[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[schema 所有者](../../../../../../src/sf2tool/h2/map_script_schema.py)。命令为 `uv run sf2 h2` 下 `map-init`、`map-script-engine`。 | 函数体 19 保留 F703 测试 → cs_58FA4 → F703 → EGRESS_MAP；函数体 20 仅返回。公共命令事实保留 cs_58E5C 传送 14、cs_58FA4 重载/列表 1/2 和传送 128、bbcs_08 加载/列表 1/2。完整分支、文本和内联动作使用已接受研究，不是公共完整转录。 |
| 场景行动者与交战 | [map-entities](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`；[battle-loop](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`sf2-battle-loop-static-v1`；[spritesets](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`sf2-battle-spriteset-data-static-v1`。命令为 `uv run sf2 h2` 下 `map-entities`、`battle-loop`、`battle-spriteset-data`。 | 通用实体/名册事实保留独立权威；精确列表与随从/F6 反例使用研究审阅。创作角色群不证明实际映射、加入、健康或交战资格。 |
| Battle08 before/start | [全局数据](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)，`sf2-battle-global-data-static-v1`；[过场](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`sf2-battle-cutscenes-static-v1`；[过场数据](../../../../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json)，`sf2-battle-cutscene-data-static-v1`；[路由](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1`。命令为 `uv run sf2 h2` 下相同 basename。 | 共享 F458 before/start 规则一致。聚合声明 `storyContentParsed=false` / `routeTargetsParsed=false`；精确行 8、bbcs_08 和 ms_Empty 目标属于已接受研究的直接源码审阅，不是已解析路由摘要的主张。 |

[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)保留依路径而异的加载；其概念性探索 init 步骤不等于每次 LoadMap 服务都会进行的 init 调用。[配置数据合同](../../../contracts/world/map-setup-data.md#合同边界)保留静态选择身份，[出战队伍/脚本合同](../../../contracts/party/party-roster-state.md#处理器边界)使行动者操作与招募、战斗员状态分离。此处不产生合同改写。

**未知，保留：**自然事件/抵达历史与合法步行；实际出战队伍、行动者/随从映射与位置；待处理存续/覆盖和控制就绪；独立运动、滚动、文本、淡入淡出、精灵重叠、音频与时序完成；以及存档/读档持久性。未来组合观察需要起始实体群/标志/玩家类型、两次选择结果、显式或哨兵值入场、init 选择、有序请求/调用方写入，以及实际准入/start 使用方。F257 已设、No/取消、F703/F458 替代状态、F6 未设、冲突随从、待处理被覆盖、F408 未设、F508 已设和 F88 恢复可区分相关边界。这些是对照，不是自动运行时队列。本受限指南既不完成战役，也不完成设计 epic。
