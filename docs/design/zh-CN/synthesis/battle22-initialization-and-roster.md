# 棋盘：Battle22 初始化与初始名册

> 本文是[规范英文原文](../../synthesis/battle22-initialization-and-roster.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**获准的全新 Battle22 没有局部 before/start 场景，但仍治疗合格出战队伍、分配起始位置、初始化十六个棋子敌人并加载战斗地图。
F522 未设置时，King Ponpei 和 Maid 作为中立行动者加入。
**推断：**名称和阵形为 King 的棋盘挑战搭建场面。
**未知：**自然准入、传入出战队伍/当前状态、服务完成及实际可见遭遇。空场景不表示跳过初始化。

本指南从 [Desktop 对话与棋盘挑战](desktop-kingdom-interactions-through-battle22-entry.md)的条件 BattleLoop 准入接续，
使用[已接受 Battle22 研究](../../../research/battle22-initialization-and-roster.md)及
[战斗控制/生命周期合同](../contracts/battle-control-lifecycle.md)，依据
[Issue #911](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/911) 和
[当前 Standard 约定](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
它止于**全新入场的 ActivateEnemies 之前**，或**挂起入场的 IndividualTurns_Loop 之前**。
战术、AI 执行、伤害、结果、战后 Desktop、Cotton Balloon 和 Floor World 均不属本指南。
继续阅读[棋盘结果与 Desktop 返回](battle22-outcomes-and-desktop-return.md)，了解已接受结果、重建实体群、
公告及改变后的对话；本指南的激活前/当前状态停止点不变。下文坐标为源码瓦片操作数，行从零计数，
不是观察到的移动，也不是规定的出战队伍阵容。

## 四个独立门控

**已确认：**前篇存续的 Map22 传送提供 `(0,0,Right)`。CheckBattle 仅在地图为 22 且 F422 已设置时选择 row22。
两个触发字节都是 255 通配符；此源码检查不要求队伍位于战斗区域矩形内。
选择写区域原点 `(9,0)`、大小 30×30，然后检查 F522。F522 已设置会清 F422，但该次调用仍准入 Battle22。
前篇保留此特定传送之前所需的许可/区域（事件）/请求条件；传入请求不证明自然行程。

| 标志 | 选定使用方 | 本指南内后果 |
| --- | --- | --- |
| F422，解锁 | CheckBattle | row22 选择所必需；独立于介绍/挂起。 |
| F472，介绍 | Before/start 包装器 | 已设置跳过两个脚本；未设置允许分发。 |
| F522，完成 | CheckBattle 与 PositionBattleEntities | 选择可清 F422；未设置准入两个中立行动者，已设置则跳过。不升级棋子类型。 |
| F88，挂起 | BattleLoop | 选择当前状态继续，而非全新初始化。 |

F777 许可属于更早对话/区域（事件），不是这些战斗门控。
F472 已设置不证明玩家看过介绍，F522 已设置不阻止这个已选中的调用，F88 已设置也不保证传入地图/战斗对与当前状态匹配。
任一标志均不能代替其余三个。

## 空开场仍然构建遭遇

**已确认：**BattleLoop 先清 PLAYER_TYPE、设置 F399，再测试 F88。
F88 未设置且传入 Map22/Battle22 时，全新序列为：

1. 清已过秒数，保存传入地图/战斗字，将 CURRENT_MAP 和 CURRENT_BATTLE 赋为 22，安装基础 VInt 并运行 before 包装器。
2. 返回后还原传入字，再赋地图/战斗。清区域标志 90–105，尽管本名册没有区域记录。
3. 治疗存活/不死己方角色；初始化己方位置，然后敌人；清 AI 内存。
4. 向 LoadBattle 提供 combatant0，再运行 start 包装器。
5. 到达 BattleLoop 的 Start 标签。**止于其 ActivateEnemies 调用之前。**

完整 slot22 before/start 表条目均选择 `ms_Empty`，即单个 `$FFFF` 终止符。
两个钩子均没有局部对话、选择、招募、行动者部署或地图加载命令。包装器和共享脚本引擎仍然相关：

| F472 未设置时的包装器 | 标志效果 | 完成边界 |
| --- | --- | --- |
| Before | 检查 F472；不写 | 分发空脚本，还原保存的寄存器并返回。 |
| Start | 分发**之前**设置 F472 | 分发空脚本，还原保存的寄存器并返回。 |

F472 已设置时，两者跳过脚本分发，但仍执行全新治疗/名册/加载序列。
空脚本仍初始化脚本状态、清文本跳过状态并消费终止符。
其尾部在 DIALOGUE_WINDOW_INDEX 非零时有条件地等待视野滚动，再清 VIEW_SCROLLING_SPEED 并返回。
共享等待可阻止返回；start 分发未完成时 F472 可能已经设置。空钩子既不确立即时完成，也不确立可用输入。
它们不重放 Ponpei 更早的遭遇场景。

## 哪些己方角色获得起始位置

**已确认：**选定精灵集声明十二个己方放置。
[完整放置表](../../../research/battle22-initialization-and-roster.md#complete-spriteset-and-party-placement)
给出有序坐标，从 placement0 `(18,18)` 至 placement11 `(17,18)`。
这些行是当前出战队伍的位置，不是具名角色指派。

治疗先运行。存活己方角色，加上即使已死亡的 Peter7 和 Lemon28，获得最大 HP/MP，
仅保留已有 STUN/POISON/CURSE 状态位并刷新属性值。其他死亡己方角色仍死亡。
该步骤不招募、不改变职业/物品栏。

放置先将全部三十个己方 X 值标为缺失（`-1`）。UpdateForce 按己方 ID 升序，
从已加入**且出战**标志重建战斗列表，替换传入的陈旧列表顺序。
列表中每个存活成员获得下一个放置 X/Y。行的数字索引、物品和 AI 字段不复制给己方角色。

上限是**十二个已处理出战队伍槽位**，而非十二个存活放置。
列表中死亡成员消耗循环容量，却不推进放置记录；后续存活成员使用该尚未消费位置。
存活出战成员较少，放置的己方角色也较少。超出十二个已处理槽位的多余成员被清出战标志、X 设为 `-1`；
LeaveBattleParty 不立即重建列表/计数。后备成员和探索随从不会自动纳入。
首次列表读取没有空列表守卫，因此不保证任意空/不一致状态能安全地产生空部署。

## 十六个棋子

**已确认：**spriteset22 声明十二个己方记录、十六个敌人记录、**零个区域和一个 AI 点**。
其十六个敌人行成为 logical 战斗员 128–143。阵形有一个 King、三个 Queen、两个 Bishop、两个 Knight、两个 Rook 和六个 Pawn：

| 行 / logical 战斗员 | 敌人类型 | 起始 X/Y | 存储指令集 |
| --- | --- | --- | --- |
| 0 /128 | King89 | `(15,5)` | LEADER |
| 1 /129 | Queen71 | `(17,5)` | ATTACKER3 |
| 2 /130 | Bishop80 | `(13,5)` | HEALER3 |
| 3 /131 | Bishop80 | `(19,5)` | HEALER4 |
| 4 /132 | Knight14 | `(11,5)` | SWARM |
| 5 /133 | Knight14 | `(21,5)` | SWARM |
| 6 /134 | Rook61 | `(9,5)` | ATTACKER2 |
| 7 /135 | Rook61 | `(23,5)` | SWARM |
| 8 /136 | Queen71 | `(15,6)` | SWARM |
| 9 /137 | Queen71 | `(17,6)` | SWARM |
| 10 /138 | Pawn13 | `(13,6)` | SWARM |
| 11 /139 | Pawn13 | `(19,6)` | SWARM |
| 12 /140 | Pawn13 | `(11,6)` | ATTACKER2 |
| 13 /141 | Pawn13 | `(21,6)` | SWARM |
| 14 /142 | Pawn13 | `(9,6)` | SWARM |
| 15 /143 | Pawn13 | `(23,6)` | ATTACKER2 |

[已接受精确敌人表及解码器](../../../research/battle22-initialization-and-roster.md#sixteen-enemy-records-and-immediate-decoding)
负责每项主/次指令、数字指令集及压缩字节。King row0 存储 MOVE_TO point0；唯一的点为 `(15,5)`。
跟随指令指向敌人行，不是地图精灵或 physical 行动者 ID。
Bishop row3 是唯一具有非 NONE 次指令的行：主 FOLLOW_ENEMY9、次 FOLLOW_ENEMY0。
全部行在两个字段中都存触发区域 15，但没有区域记录；15 不确立第十五个多边形。

全部十六行使用 STARTING 初始化。敌人定义提供 HP/MP 最大值、复制物品栏及移动类型；
初始化填充当前 HP/MP，指派行位置/类型，并组合移动/指令集、触发、指令和物品栏效果。
随后执行共享难度/属性值调整。激活前，初始激活字为 King `$1000`、Bishop `$3000`、其他棋子 `$2000`，
主/次激活位和中立位均未设置。槽位 144–159 走缺失路径：activation0、最大/当前 HP0、X=`-1`，
不保证清零每个字段。中立加载器随后可复用 158/159。
LEADER、HEALER 和 SWARM 是存储标签，不是已执行战术、治疗或结果规则。
本指南不执行点/跟随指令或法术。

### NOTHING 保留复制的装备

**已确认：**每个敌人行存储精确物品字 NOTHING（`$007F`）。物品初始化器遇到该完整字哨兵值时在 AddItem 之前返回；
它**不会**擦除从敌人定义复制的物品栏。King 携带已装备 FLAIL；Queen/Bishop 为已装备 POWER_STICK；
Knight 为 POWER_SPEAR；Rook 为 ROBIN_ARROW；Pawn 为 BATTLE_AXE。各定义另外三个物品槽位均为 NOTHING。
[定义物品栏表](../../../research/battle22-initialization-and-roster.md#row-nothing-versus-definition-inventory)
保留 HP/MP 最大值、移动类型及非哨兵解码器的独立限制。

选择敌人类型 Bishop80，尽管其定义注释为 EVIL BISHOP；该注释不选择另一枚举。
完整随机战斗列表没有 Battle22，因此传入 F522 已设置不会升级这些类型。
Battle21 的随机升级行为及开场场景不移植至此。复制的物品/法术不确立使用、伤害、后续掉落或奖励；
传入己方装备也不被放置行替换。

## 加载棋盘替换探索行动者

**已确认：**LoadBattle 使用 CURRENT_MAP 选择瓦片集/地图，淡出、禁用视野跟踪、等待淡入淡出并清 VInt。
PositionBattleEntities 清旧行动者数据和查找，然后扫描三十个己方及三十二个敌人状态槽位。
非负 X/Y 准入普通行动者；敌人遍历还排除带中立标记的槽位。该行动者扫描没有额外 HP 测试。
全新初始化提供了资格；已有恢复状态因此可能产生不同实体群。

普通行动者以 Down 和 eas_Standing 开始，使用 `(X & $3F)*384` 及 `(Y & $3F)*384`。
己方解析受职业影响的地图精灵；敌人由存储类型解析精灵。
六种棋子精灵为 King153、Queen135、Bishop144、Knight78、Rook125、Pawn77，均使用普通顺序分配。
若成功声明 N 个己方角色，全新敌人行 r 的 physical 索引为 N+r；十二个已放置己方角色使十六个敌人位于 physical12–27。
跳过的己方查找为 `-1`。

探索的棋子 NPC137–152、Caravan128 和 ORB153–155 不会带入。
Logical combatant128 此时指 King，而 combatant137 指 Queen row9。
**敌人 type89、sprite153、logical actor128 和 physical actor N 是不同标识符。**
之前的随从别名、探索 physical 索引/行动及 Right 朝向传送不作为战斗映射保留。

### Ponpei 与 Maid 是受完成标志门控的中立角色

**已确认：**普通分配后，F522 未设置精确选择两个 CHESSBOARD 中立行：

| 行动者 | 战斗员状态槽位 | 脚本查找 | 局部 X/Y | 朝向 / 精灵 / 行动 |
| --- | --- | --- | --- | --- |
| King Ponpei | 159 | B，offset31 | `(6,2)` | Down / KING_PONPEI188 / eas_Standing |
| Maid | 158 | A，offset30 | `(5,3)` | Down / MAID208 / eas_Standing |

加载器写最大/当前 HP0、status0 和行 X/Y，并将中立掩码 `$0008`（bit3）OR 入激活值。
全新缺失槽位初始化使该激活值恰为 `$0008`。
N 个已放置己方及十六个普通敌人时，physical 分配为 N+16/N+17，十二个己方时即 28/29。
它们是两个额外中立行动者，不是敌人精灵集行或出战队伍成员。

查找写入从 ENTITY_INDEX_LIST_ENEMIES 预减，落在整体列表 offset31/30，即 **B/A**，而非敌人查找 offset63/62。
普通 combatant159/158 查找仍为 `-1`；GetEntityIndexForCombatant(159/158) 无法取得这些行动者。
状态槽位与脚本查找键必须区分。它们也不是旧探索 Ponpei134 和 Maid133 映射。
F522 已设置则跳过该组，全新槽位 158/159 仍缺失。不加入 Caravan、ORB 或其他居民。
恢复时，同一加载器可重写这些中立状态槽位；它不保证保留每个已保存字段。

### 完整地图加载、裁剪棋盘及视野目标

**已确认：**LoadBattle 还原其战斗员参数（全新为 0），为 VIEW_TARGET_ENTITY 解析该行动者，并读取选定战斗瓦片坐标。
查找缺失时，这些读取回退至 physical0，不还原成员资格，也不放置 Bowie。
然后设置 **d0=-1** 用于基于目标的视野放置，以非负 **d1=Map22** 调用 LoadMap。
后者选择完整地图加载。这不同于前篇复用工作布局的负 d1 reloadMap；负视野 d0 不是地图重载选择器。

完整加载应用基础布局、标志复制及宝箱覆盖，再将原点 `(9,0)`、大小 30×30 的区域裁剪至战斗工作原点 `(0,0)`。
Layer1 跨局部 `(0,0)` 至 `(29,29)`。
名册、中立角色及点坐标已经是**战斗局部坐标**；初始化不从其 X 减去 9。
裁剪棋盘与源码布局的对应为 `(localX+9,localY)`：

| 选定位置 | 战斗局部 | 完整 Map22 对应 |
| --- | --- | --- |
| King / point0 | `(15,5)` | `(24,5)` |
| 己方 placement0 | `(18,18)` | `(27,18)` |
| 中立 Ponpei | `(6,2)` | `(15,2)` |

此关系既不是传送，也不是从 `(0,0)` 走上棋盘的证据。
传入区域状态改变会改变裁剪；LoadBattle 不重跑 CheckBattle。
随后是实体地图精灵、基础 VInt、既有地形加载器、音乐提交及淡入；Battle22 没有 Battle44 计时器窗口分支。
必需服务须返回，才能到达 start 包装器和全新停止点。后续服务未完成不回滚早先写入。
布局几何和服务调用不证明渲染地形、可听音乐、精确时序或可用输入。

## 挂起入场使用当前状态

**已确认：**共用模式清除/F399 写入后，F88 已设置会还原已保存的已过秒数、清 F88 和 AI 内存、
清 VIEW_TARGET_ENTITY、调用 LoadBattle 并直接分支至 IndividualTurns_Loop。**止于该循环之前。**
它绕过全新地图/战斗赋值、区域重置、治疗、己方/敌人初始化和两个介绍包装器。
它不显式向 LoadBattle 提供全新 combatant0。
两种入场都将 48 个上次目标字节清为 `$FF`、48 个 AI 内存字节清为零；该重置不是执行回合（单次行动）或战术模拟。

加载器重建行动者并加载实际当前地图/区域/战斗员，施加适用的完成/中立效果。
它不重建 spriteset22 的全新名册。既有 HP、物品、状态、位置、当前回合（单次行动）/顺序及视野参数可能不同；
此分支上，传入 22/22 对不覆盖独立改变的 CURRENT_BATTLE。
此源码继续路径不证明跨进程存档还原或持久性。

| 改变的传入前提 | 条件后果 |
| --- | --- |
| CheckBattle 时 F422 未设置 | 无 row22 准入，不受 F472/F522/F88 影响。 |
| F472 已设置，F88 未设置 | 跳过两个脚本；全新出战队伍/敌人/加载工作仍运行。 |
| F522 和 F422 已设置 | 准入本次选择并清 F422；无类型升级；无选定中立角色。 |
| 成员资格改变、死亡或多余成员 | 重建升序出战/已加入列表；十二个已处理槽位不保证十二个存活行动者。 |
| 己方职业/物品改变 | 全新治疗后，受职业影响的精灵及传入物品栏仍相关。 |
| F88 已设置，当前/已保存状态不一致 | 使用该当前状态，不作全新修复，包括适用中立重写。 |
| 包装器/加载服务未完成 | 仅确立已完成前缀写入；不声称已完成表现/控制。 |

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `5ef473acf19a3c47cbccc71078e0e3bbfc65f047` 包含
[#908/PR910](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/910) 及
[#907/PR909 前篇验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499)。
研究所有者固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
和 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
本综合指南审阅已接受关联，不重新获取源码、不提取或运行运行时。

综合前对抗审阅检查完整 Battle22 所有者、前篇、生命周期合同及下列有边界可执行表面，
包括 fixture ID、溯源、schema 和验证器选择。窄命令是复现所有者，不是新运行：

| Fixture / 命令所有者 | 已审阅边界 |
| --- | --- |
| [sf2-battle-loop-static-v1](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`uv run sf2 h2 battle-loop` | 名册大小、合格治疗和 AI 内存重置；生命周期合同与已接受 Battle22 直接关联提供放置/死亡/多余成员细节。 |
| [sf2-battle-control-static-v1](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`uv run sf2 h2 battle-control` | 全新/挂起控制器事实与小节单位；已接受选定控制跟踪提供精确赋值、绕过及停止标签。 |
| [sf2-battle-cutscenes-static-v1](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`uv run sf2 h2 battle-cutscenes` | 共享介绍检查/写入顺序；完整 slot22 表、终止符及实际返回属于已接受直接关联。 |
| [sf2-battle-routing-data-static-v1](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`uv run sf2 h2 battle-routing-data` | 汇总拓扑有 routeTargetsParsed=false；不能独立证明两个 slot22 目标。 |
| [sf2-battle-spriteset-data-static-v1](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)，`uv run sf2 h2 battle-spriteset-data` | 头部/宏/地址清单有 rowContentTracked=false；已接受十二/十六记录、点、定义及加载/映射关联提供精确局部语义。 |

对应 fixture schema 为
[loop](../../../../schemas/h2-battle-loop-static-fixture.schema.json)、
[control](../../../../schemas/h2-battle-control-static-fixture.schema.json)、
[cutscenes](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)、
[routing](../../../../schemas/h2-battle-routing-data-static-fixture.schema.json) 及
[spritesets](../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)。
既有验证器为
[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py)、
[battle_control.py](../../../../src/sf2tool/h2/battle_control.py)、
[battle_cutscenes.py](../../../../src/sf2tool/h2/battle_cutscenes.py)、
[battle_routing_data.py](../../../../src/sf2tool/h2/battle_routing_data.py) 及
[battle_spriteset_data.py](../../../../src/sf2tool/h2/battle_spriteset_data.py)。
这些汇总相等不能守护每项新名册/加载操作数，也不能证明自然到达。
已接受所有者的[复现边界](../../../research/battle22-initialization-and-roster.md#reproduction-retained-evidence-and-limits)
记录完整宏/常量、初始化、物品、中立、行动者及地图加载使用方关联。
未发现冲突的已接受语义所有者。更早 battle-loop/spriteset 的“runtime queue”措辞，
依 ADR0014/0016 仍是暂缓歧义登记，不是获取指令，也不是降低 Battle22 已接受静态关联等级的理由。

保留 [#908 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174872245)：
head `0a7048f0fbfa38a69d6470a73e40445cebc55acd`、tree `eb68c08e958a868dd163f9e128edffffa5147f14`、
base `ccf4a57360fea5b62160fe959a5d2dbca6dd8736`、
[CI38610992302](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38610992302)。
其[纯变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174994479)
保留 head `9b054f217a07b1ab8926dcf90f30ac5fd90f8668`、tree `6da75413c07be7587231e10e6659f3dde40c7eca`、
base `2c13623a3720199d2e92b13f9b353bcf054cc756`、
[CI38612004333](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38612004333)。
两次 CI 的 scope/research-public 均成功，engine/adapter/H4 按范围跳过；
[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/910#issuecomment-6175006823)适用于未改变的变基后文档。
保留 map-init/map-script 对未改变：18,285,979 字节，0.084s 内完成比较。
本指南不重跑或再生成比较。

这些交接保留有界读取/路径搜索失败、中立状态与 B/A 查找修正、完整加载 d1 与视野 d0 澄清及 CRLF 检查。
其链接的 #904/#903/#900/#899 收据保留不同 head、CI 及已完成失败/修正。
[前篇证据段](desktop-kingdom-interactions-through-battle22-entry.md#证据与审阅边界)保留这些链接，
包括已修正 csc2D FLAGS_A5/6 目的地扫描绕过、reloadMap 工作布局复用及 RightLeft 朝向。
[#907 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499)
保留 CI38610983007 仅 scope 成功。这些结果不被本综合指南替换。

直接验收检查完整自有正文/diff、全部十六敌人行及放置/标志/物品/映射/坐标区别、溯源、链接/片段、范围、英文/私有边界和空白。
使用已配置的干净已提交依赖规划器及实际最终 head CI。
不单因正文选择局部 normal/full/engine/Godot/H3/H4 或翻译套件。

**未知：**自然接近/准入；传入标志、成员资格、HP/职业/物品/当前/已保存状态；实际活动名册、查找和视野；
已完成加载/淡入淡出/VInt/共享脚本返回服务；可见/可听演出、可用控制及跨进程持久性。
改变状态示例是静态反例，不是已执行场景；依
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和
[ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)，也不构成自动运行时队列。
独立结果指南使用已接受 #912；本初始化指南保留自身停止点。
本指南既未完成战役，也未完成 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
