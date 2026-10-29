# Battle23 初始编队与挂起入场

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/battle23-initialization-and-roster.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

**已确认：**新 Battle23 入场治疗符合条件的己方角色，并从传入军团建立编队，敌方为 Willard、五只 Rat 和六只 Bubbling Ooze。十二个己方布置槽位不保证十二名存活英雄或具名出战队伍。挂起入场则加载当前状态，不执行该新配置。**推断：**具名敌方编队布置了洞穴遭遇。**未知：**自然准入、实际传入状态、服务完成、可见/可听表现、可用控制和持久性。

本指南从 [Willard Burrow 准入](willard-burrow-through-battle23-entry.md)的 BattleLoop 调用继续，使用[已接受 Battle23 研究](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md)，依据 [#938](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938) 及 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。它解释原版配置与条件消费者。新入场止于 `BattleLoop:@Start`，**早于 ActivateEnemies**、区域激活与初始回合创建。挂起入场止于 `@IndividualTurns_Loop`，早于读取或执行单次行动。Fnnn 表示标志 nnn；下文坐标、属性值和 AI 标签为源码数据或初始化器结果，不是观察到的游玩。

## 准入与新配置

**已确认：**Map27 仅在战斗解锁 F423 已设置时选择 Battle23。唯一记录使用通配触发 X/Y，不检查裁切包含关系，并设置区域原点 `(23,14)`、大小27x16。完成战斗 F523 已设置时清 F423，但仍准入此次已选调用。探索场景 F781 与战斗开场 F473 独立。传入 F423 可完全绕过探索对峙。前篇的低字节坐标覆盖限制仍适用：通配准入不修复异常高字节。

BattleLoop 先清 PLAYER_TYPE 并设置 F399，再检查挂起 F88。清 PLAYER_TYPE 不确立所有保留木筏字段均已擦除。F88 未设置时，条件序列为：

1. 清秒计数器，保存传入字，赋当前地图27/战斗23。安装基础 VInts 并执行 before 包装器。
2. 恢复传入字，重新赋当前地图/战斗。清区域标志90–105，包括唯一声明区域对应的 F90。
3. 按顺序治疗存活/不死己方角色、初始化己方位置、初始化敌方位置并清 AI 内存。
4. 将战斗员0 传给 LoadBattle。返回后执行 start 包装器。
5. 到达 `@Start`；本指南止于其 ActivateEnemies 调用前。

before 包装器先于治疗及双方名册。更早的探索实体群或当前地图值不在这些赋值后选择新名册。每个后续步骤都要求之前服务返回；后续服务未完成时，已完成前缀效果不回滚。

### 空钩子仍有效果

两个完整48 字表均在 Battle23 槽位23、字节偏移46 选择 `ms_Empty`。其完整主体是 `$FFFF` 脚本终止符。两个选定钩子均无局部文本、移动、选择、招募或加载命令。

| 传入 F473 | before 包装器 | start 包装器 |
| --- | --- | --- |
| 未设置 | 分发空脚本而不写 F473 | 在分发空脚本**之前**设置 F473 |
| 已设置 | 跳过分发 | 跳过标志写入和分发 |

跳过两个钩子不跳过新重置、治疗、编队或加载。F473 已设置不证明开场曾被游玩。即使空分发也初始化脚本状态、保存寄存器并清文本跳过字节；共享调试/输入处理可再次设置它。尾部在有对话窗口时条件等待滚动、清滚动速度、恢复寄存器并返回。等待可阻止完成，包括 start 包装器已设置 F473 之后。[研究钩子关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#empty-hooks-still-have-wrapper-effects)负责表、主体及完整包装器溯源。

## 哪些己方角色接收十二个槽位

**已确认：**精灵集头声明12 名己方角色、12 名敌人、一个区域及一个点。己方条目提供以下战斗局部坐标：

| 布置索引 | X/Y | 布置索引 | X/Y |
| --- | --- | --- | --- |
| 0 | `(4,5)` | 6 | `(2,6)` |
| 1 | `(4,4)` | 7 | `(4,6)` |
| 2 | `(3,5)` | 8 | `(4,3)` |
| 3 | `(3,4)` | 9 | `(2,7)` |
| 4 | `(3,6)` | 10 | `(3,8)` |
| 5 | `(3,7)` | 11 | `(3,3)` |

每条完整己方记录还声明 HEALER1=0、物品 NOTHING=`$007F`、主/次指令 NONE=`$FF` 且区域15、填充0 及 STARTING=0。己方初始化器**仅复制 X/Y**，不复制行索引、物品或 AI 字段。因此布置索引0 自身不表示 Bowie，也不招募任何角色。

布置前，治疗独立于出战成员资格扫描全部三十名己方角色。存活己方角色及始终包括的 Peter (7)/Lemon (28) 获得最大当前 HP/MP；其他死亡角色跳过。辅助程序将状态掩码为既有 STUN/POISON/CURSE 位（`$0007`），再重建当前属性值、装备效果与诅咒。该掩码是中间步骤，不保证所有最终状态位不变。职业、物品栏和已加入标志是传入输入；治疗既不自动征召后备成员，也不使其出战。

位置初始化器先将全部三十名己方角色 X=-1。UpdateForce 按己方 ID 升序重建已加入/出战列表，替换陈旧列表顺序。列表中存活成员取得下一对未消费坐标。死亡成员消费一个已处理成员槽位，**不推进布置指针**。因此后续存活成员取得更早未用坐标对。上限是十二名已处理成员，存活行动者可能更少。多余成员离开出战队伍并保持 X=-1；LeaveBattleParty 不即时重建列表/计数。符合条件成员更少时布置更少。传入零长度列表在首次条目读取前没有守卫，因此任意不一致空状态并非已证明安全的部署。

[完整编队与队伍关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#complete-spriteset-and-ally-formation)保留306 字节精灵集、记录解码器及实际成员资格/治疗消费者。它支持状态相关出战队伍，而非固定角色序列。

## Willard 与完整敌方编队

**已确认：**十二条敌方记录成为逻辑战斗员128–139。全部共享物品 NOTHING=`$007F`、次指令 NONE=`$FF` /区域15、填充0 和 STARTING=0。下表给出每个不同记录字段；共享字段适用于每行。

| 行 / 战斗员 | 类型 (ID) | 局部 X/Y | 指令集（值） | 主指令（字节） | 区域 |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | WILLARD (90) | `(18,7)` | LEADER (14) | MOVE_TO\|0 (`$40`) | 15 |
| 1 /129 | RAT (15) | `(20,6)` | ATTACKER3 (9) | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 2 /130 | RAT (15) | `(20,8)` | ATTACKER3 (9) | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 3 /131 | BUBBLING_OOZE (16) | `(17,1)` | ATTACKER2 (7) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 4 /132 | RAT (15) | `(16,2)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 5 /133 | BUBBLING_OOZE (16) | `(15,2)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 6 /134 | RAT (15) | `(21,12)` | ATTACKER2 (7) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 7 /135 | BUBBLING_OOZE (16) | `(22,13)` | ATTACKER1 (6) | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 8 /136 | BUBBLING_OOZE (16) | `(20,14)` | ATTACKER2 (7) | NONE (`$FF`) | 0 |
| 9 /137 | RAT (15) | `(10,11)` | ATTACKER1 (6) | NONE (`$FF`) | 15 |
| 10 /138 | BUBBLING_OOZE (16) | `(9,10)` | ATTACKER3 (9) | FOLLOW_ENEMY\|9 (`$89`) | 15 |
| 11 /139 | BUBBLING_OOZE (16) | `(5,12)` | ATTACKER1 (6) | NONE (`$FF`) | 15 |

初始化器扫描全部32 个敌人槽位。十二条 STARTING 行从选定定义初始化；缺席槽位140–159 获得激活0、最大/当前 HP0 和 X=-1。缺席路径不清所有字段，仍读取触发字节；不确立额外有效行。Battle32 的 Jaro 例外不适用。

对每条存在行，初始化复制56 字节定义、从最大值恢复当前 HP/MP、赋位置/类型、在指令集之上打包 movetype，并写触发区域、指令及物品效果。触发字节在行3–8 为 `$0F`，其余为 `$FF`。指令字为行0 `$40FF`，行1/2 `$80FF`，行3–7 `$00FF`，行8/9/11 `$FFFF`，行10 `$89FF`。Willard 激活字成为 `$0000`；Rats/Oozes 为 `$2000`，保留优先级位但丢弃复制来的 BIT5。在 ActivateEnemies 之前，主/次激活位和中立位未设置，初始化模式为0。区域15 或指令集标签均不表示敌人已激活。

### 定义、装备与初始化属性值

三个定义均以状态 NONE 和四个 NOTHING 物品槽开始，因此没有已装备武器。所有记录物品字均为精确 NOTHING 哨兵值；InitializeEnemyItems 在 AddItem 前返回，保留复制的物品栏。NOTHING 不是通用物品栏擦除操作。

| 定义 | HP/MP 最大值与新当前值 | 基础 ATT / DEF / 原始 AGI / MOV | Movetype | 四个法术槽 |
| --- | --- | --- | --- | --- |
| WILLARD (90) | 50/0 | 50 /36 /165 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| RAT (15) | 41/0 | 45 /32 /30 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| BUBBLING_OOZE (16) | 32/0 | 38 /35 /26 /4 | REGULAR=1 | AQUA\|LV2 (`$68`), NOTHING, NOTHING, NOTHING |

法术 NOTHING 为 `$3F`；AQUA=`$28` 与 LV2=`$40` 组合为 `$68`。Ooze 在零 MP 下声明法术，不证明后续 AI 选择或费用。Willard 原始 AGI165 是复制数据，不是显示敏捷或行动次数结果。定义还提供等级、抗性、特殊攻击概率（会心/二连/反击）与法术威力字段；后续行动/伤害解释在本入场边界外。

激活写入后，先调整难度，再重建派生属性。已接受连续分支分析表示**所有难度**先应用 floor(baseATT*5/4)；SUPER (2) 再应用一次。因此 Willard/Rat/Ooze 在 NORMAL (0)、HARD (1) 或 OUCH (3) 的初始化基础 ATT 为62/56/47，在 SUPER (2) 为77/70/58。这些是初始化攻击值，不是伤害。UpdateCombatantStats 从基础/状态/装备重建当前属性值；选定定义不引入初始状态或已装备物品效果。完整选定定义及消费者见[定义/难度关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#definitions-items-spells-and-difficulty)。

### 一个区域与一个点不确立战术

唯一 type 4 区域顶点为 `(9,0),(9,15),(26,15),(26,0)`。第二字节与尾部字节为零；选定几何消费者读取类型和打包顶点，使用两个三角形，不读取其他字节。Point 0 为 `(22,3)`。全部坐标均为战斗局部坐标。

新入场清 F90。后续区域消费者将区域0 关联 F90，并独立于存储触发区域字节写激活位。区域15 是 NONE 哨兵值，不是十五个多边形。清标志90–105 自身不声称每个 AI 临时字段均已重置。MOVE_TO|0 通过低半字节点索引选择 point 0；FOLLOW_ENEMY|9 编码战斗员137（FOLLOW_ENEMY|0 编码128）。FOLLOW_TARGET|0 保留目标操作数0。这些是消费者关系，不是选定路线、到达的点、目标或攻击。在本指南终点没有已证明触发的声明区域。

完整随机战斗表排除 Battle23，因此完成 F523 不升级这些类型。完整中立表同样没有 Battle23 组：F523 已设置则跳过扫描，未设置则扫描至末而不匹配。均不增加新中立角色，也不改用158/159。[记录/激活关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#twelve-enemy-records-and-initialization)和[区域/点关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#region-0-and-point-0-are-inputs-not-fired-triggers)保留完整字节与排除证据。不从 LEADER、ATTACKER、STEALTH 或编队布置推断战术意图。

## 加载战场重建行动者身份

**已确认：**LoadBattle 淡出、禁用视野跟踪、加载瓦片集、等待淡入淡出/VInt 工作并重建实体。该遍处理清49 个物理记录和64 个查找字节，再扫描全部30 个己方与32 个敌方槽位。非负 X/Y 准入记录；带中立标志的敌人排除。此处**没有额外 HP 检查**。新初始化提供位置；任意保存位置不一定与 HP 有相同关系。

普通行动者使用掩码局部坐标 `(X & $3F)*384`、`(Y & $3F)*384`，Down (3) 和 eas_Standing。己方地图精灵使用当前随职业变化的映射，包括 Rohde 成员资格例外；战斗模式跳过仅探索使用的死亡己方 BLUE_FLAME 分支。敌人类型选择 WILLARD (154)、RAT (79)、BUBBLING_OOZE (80)。这些是声明精灵 ID，不是已验证渲染精灵。

声明 N 名己方角色时，新敌人行 r 获得物理槽 N+r。十二名己方会让敌人占 physical12–23；N 未必为十二。逻辑战斗员128–139 使用查找32–43；跳过的普通槽位查找为-1。Willard 类型90、地图精灵154、逻辑128 和物理 N 是不同身份。探索的 logical128 Rat /129 Willard /130 Rat 及随从/木筏映射被替换，不延续为这些战斗身份。

通用脚本别名 A30/B31、C158/D159 不是额外角色。此处查找30/31 清除后保持零，缺席敌人查找62/63 为-1。继承 D/raft63 别名规则不保留木筏行动者，也不使无效查找安全。Chessboard 的 Ponpei/Maid 或探索居民也不导入为 Battle23 中立角色。传入恢复状态中，中立标记槽仍被普通遍处理排除；中立表不匹配不修复该战斗员。带有效位置的额外非中立槽即使在新十二行外也可声明行动者。

### 编队坐标已属于裁切区域

新 LoadBattle 接收战斗员0 用于视野选择。它解析重建查找并读取该实体坐标；负查找回退到 physical0 读取，不放置 Bowie，也不修复成员资格。之后向 LoadMap 传 d0=-1 以按目标放置视野。独立 d1 地图操作数选择完整加载或重载：保留的当前 Map27 非负，走完整加载，而非更早负 d1 reloadMap 路径的工作布局复用。

完整加载应用地图标志复制/宝箱覆盖，将源码原点 `(23,14)`、大小27x16 裁切到工作原点 `(0,0)`。战斗 layer1 范围终点为 `(26,15)*384`；其他图层操作数仍是地图区域数据。编队、敌人、区域及点已是局部坐标；辅助程序不再次减23/14。布置0 `(4,5)` 对应源码 Map27 `(27,19)`，Willard `(18,7)` 对应 `(41,21)`，point0 `(22,3)` 对应 `(45,17)`。这是裁切对应，不是观察到的从传入 `(0,0)` 传送位置移动，也不证明地形合法性。LoadBattle 不重跑 CheckBattle；传入区域字节可改变裁切。

地图精灵、基础 VInts 和地形后，LoadBattle 提交地图音乐并淡入。Battle23 不走 Battle44 计时窗口分支。只有返回才允许 start 包装器及新入场终点。[加载/实体/裁切关联](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#battle-load-entities-and-coordinate-domains)区分物理声明、视野参数和完整加载几何。仅调用返回既不证明精确可见时序，也不证明可用控制。

## 挂起入场使用传入当前状态

**已确认：**F88 已设置时，清 PLAYER_TYPE/设 F399 后，分支恢复保存秒数、清 F88、清 AI 内存、清视野目标、加载战斗，并直接到 `@IndividualTurns_Loop`。之后到 `@Start` 的分支不可达。新入场与已保存入场的 AI 清除均将48 个最后目标字节重置为 `$FF`，48 个内存字节重置为0；这不创建新行动顺序。

| 配置或前提 | 新入场 F88 未设置 | 挂起入场 F88 已设置 |
| --- | --- | --- |
| 当前地图/战斗 | before 包装器前后赋传入27/23 | 保留实际当前值；传入27/23 不替换它们 |
| 秒数 | 清除 | 恢复保存秒数 |
| 区域标志、治疗与编队 | 清标志90–105；治疗；初始化双方 | 绕过这些新操作 |
| 钩子 | 通过 F473 条件分发 before/start | 两个包装器均不运行 |
| 视野/加载 | 显式战斗员0；重建实体查找 | 无显式 d0 清零；传入参数与当前状态加载 |
| 下一边界 | ActivateEnemies 和初始回合前 | 读取/执行已保存单次行动前 |

HP/MP/状态/物品/职业、成员资格、位置/激活、区域、地图/战斗与保存的行动/顺序仍是传入前提。改变 CURRENT_BATTLE 可选择另一战斗数据/中立实体群，即使传入27/23。LoadBattle 重建物理行动者并加载实际当前地图/地形；视野参数不选择该地图。不由此保证全新健康、固定行动者数量、自动修复或跨进程持久性。即使保存顺序哨兵值分支也在本指南终点之后。[保存入场研究](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#suspendedcurrent-state-entry-and-changed-premises)负责这些改变状态示例，不是已游玩的恢复演示。

## 证据审阅与保留限制

**已确认溯源：**已接受 main `424b8eaeb2f467f3f714d39b73d255f2a3c9dd27` 包含 #932/PR935 和 #934/PR936。研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../../../research/chapters/southern-travel-creed/battle23-initialization-and-roster.md#reproduction-retained-results-and-limits)保留精确符号/地址、完整选定记录与源码关联。本指南不增加原版观察或提取。

原英文切片的对抗审阅阅读完整已接受研究和前篇，比较全部布置/敌人行、共享字段、数量和生命周期区别。选定公共 fixture/schema/验证器边界如下：

| 可执行所有者 | 选定声明与限制 |
| --- | --- |
| `sf2-battle-loop-static-v1`，tests/fixtures/h2/battle-loop-static-v1.json | 名册30/32、治疗/掩码/派生属性与 AI 清除48；匹配 h2-battle-loop-static-fixture schema 和 battle_loop.py `_build_lifecycle_facts`。不识别实际传入出战队伍。 |
| `sf2-battle-control-static-v1`，tests/fixtures/h2/battle-control-static-v1.json | F88/秒数、标志90–105、12 字节战斗员/区域步长与有序源码片段；匹配 control fixture schema 和 battle_control.py `_build_control_facts`。不是自然新入场/恢复观察。 |
| `sf2-battle-cutscenes-static-v1`，tests/fixtures/h2/battle-cutscenes-static-v1.json | 共享开场门槛与 start 的先标志、后分发；匹配 cutscenes fixture schema 和 battle_cutscenes.py `_cutscene_facts`。研究负责完整槽位23/终止符/包装器关联。 |
| `sf2-battle-routing-data-static-v1`，tests/fixtures/h2/battle-routing-data-static-v1.json | 共享表拓扑/溯源；匹配 routing fixture schema 和 battle_routing_data.py。routeTargetsParsed=false 不验证槽位23 语义。 |
| `sf2-battle-spriteset-data-static-v1`，tests/fixtures/h2/battle-spriteset-data-static-v1.json | H1 地址/头汇总溯源；匹配 spriteset fixture schema 和 battle_spriteset_data.py。rowContentTracked=false 不验证这24 条记录主体或含义。 |

schema 在 `schemas/`；验证器在 `src/sf2tool/h2/`。既有窄路线为 `uv run sf2 h2 battle-loop`，以及相同命令前缀的 `battle-control`、`battle-cutscenes`、`battle-routing-data` 和 `battle-spriteset-data`。汇总相等不是正文或运行时证明。保留研究直接关联完整队伍/治疗/初始化器/属性值/难度/加载/实体/裁切消费者、全部选定记录、一个区域/点与完整随机/中立表。原英文设计切片的检出缺少固定 upstream/保留载荷对；已接受源码/比较结果予以保留，不声称新重跑或再生成。

保留 [#932 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/932#issuecomment-6176988463)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/935#issuecomment-6177066791)，包括对比较0.081s、CI38626848120、已完成诊断与前篇修正；[#934 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/934#issuecomment-6177126852)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/936#issuecomment-6177145045)保留 CI38627848438、局部输入缺席、#928 结果及其失败。原英文切片执行直接文档/diff、数值/顺序/表/链接/英文/私有检查与 `git diff --check`，然后检查已配置干净已提交 planner 及实际最终 head CI。不单因正文选择局部一揽子套件。

**未知：**自然准入；实际标志、出战队伍/HP/职业/物品、模式/别名、位置/激活、当前/保存区域/地图/战斗和行动/顺序；脚本/加载/淡入淡出/VInt 服务完成、可见/可听名册/表现、可用控制和持久性。本指南及其配置均不证明已执行战术、显示敏捷、结果、首领死亡、战后或 Creed 进展。独立的[结果与洞穴返回指南](battle23-outcomes-and-burrow-return.md)使用已修正接受的 #937 从行动后检查继续，不填补两边界间未观察的行动。这些缺口仍受 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)约束，不创建自动运行时队列、新重制验收或完整战役完成声明。
