# Battle23 结果与返回 Willard Burrow

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

**已确认：**Battle23 可通过首领清理或敌人数为空结束。胜利治疗符合条件的己方角色、标记战斗完成，并返回从当前出战队伍派生的位置。新 Map27 返回重建没有居民的完整洞穴。败北和获准 Egress/Angel Wing 则使用存档点选择器，费用与恢复效果不同。**推断：**改变后的居民选择移除了 Willard 对峙实体群。**未知：**自然交战/穿行、传入状态、服务完成、表现、可用控制及持久性。

本指南从 [Battle23 初始编队与挂起入场](battle23-initialization-and-roster.md)继续，使用[已修正接受的结果研究](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md)，依据 [#942](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/942) 及 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。它从行动后结束检查开始，不模拟初始化与该接缝之间的行动。它止于有条件的洞穴探索或下一地图准入，早于目的地初始化或任何 Creed 场景。后续 [Creed 恢复与主房间离开](creed-restoration-and-basement-departure.md)从独立获准的 Map33 加载开始，保留本指南退出限制。Fnnn 表示标志 nnn；槽位从零计。下例在传入前提下区分源码分支，不是已执行场景。

## 什么结束战斗

**已确认：**单次行动与可选调试 kill-all 分支之后，首次结束检查运行 defeated 钩子、处理死亡列表、计数剩余战斗员，再**先测试零己方角色，后测试零敌人**。若两数均非零，则运行行动后效果，再清理一次并按相同顺序计数。第二次检查不调用 defeated 钩子。调试分支不确立自然结果。

计数战斗员要求非负 X 和非零 HP。扫描全部30 个己方和32 个敌方槽位，不测试已加入/出战成员资格。Bowie HP0 即使其他人存活也强制己方计数为零。因此双方计数同时为空意味着败北；Bowie 存活仍需非空计数己方集合才可胜利。

### Willard 死亡可移除剩余敌人

defeated 包装器要求 Bowie 存活且敌人128 HP0。已接受新名册中的身份为：

| 类型 | 逻辑战斗员 |
| --- | --- |
| Willard | 128 |
| 五只 Rat | 129, 130, 132, 134, 137 |
| 六只 Bubbling Ooze | 131, 133, 135, 136, 138, 139 |

这些交错身份来自前篇完整十二行[编队表](battle23-initialization-and-roster.md#willard-与完整敌方编队)。传入/恢复名册必须建立自身身份前提。

Battle23 的 defeated 表选择 `ms_Empty`。F523 未设置则分发，已设置则跳过脚本。**两条路径仍到达公共首领尾部**：选定首领字节为-1，启用敌人128–159 扫描。每个非零 HP 条目追加到现有死亡列表，后接一个-1 字节，并递增长度。这不测试布置、不清 HP、不重置列表，也不去重。Willard 已零 HP 的槽位不由此尾部追加；他的任何普通死亡列表条目来自早先工作。

清理处理长度限定列表，先包括旋转/死亡声音请求，再记账：列出的死亡己方角色记败北，或向 BATTLESCENE_FIRST_ALLY 记敌人击杀；X/Y=-1、清状态、重算属性值，并将映射实体放到原始 `(0x7000,0x7000)`。它既不将列出 HP 清零，也不清列表长度。追加的存活敌人因负 X 而不再计数。空列表即时返回。陈旧/重复/无效列表条目与行动者映射是传入问题，不是该包装器执行的修复。计数前全部必要表现/VInt/睡眠服务都必须返回。

因此，在**首次钩子接缝**中 Bowie 存活、Willard HP0，并且已布置出战队伍一致、列表/映射有效且服务完成时，可不逐个杀死每只 Rat/Ooze 而胜利。若 Willard 仅在行动后效果中死亡，第二次检查不运行该首领尾部：剩余计数敌人仍阻止胜利。反之，只要己方仍存在，零计数敌人允许不使用首领尾部而胜利。

没有需要增加的局部死亡对话或布置覆盖：defeated 与 after 表选择仅终止符脚本，完整 leader-position 表只有 Battle05，region-script 表只有战斗32/40。[研究表/包装器关联](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md#post-action-checks-and-leader-cleanup)负责精确选定槽位和完整排除检查。

## 胜利从当前出战队伍状态返回

**已确认：**胜利主体执行以下序列：

1. 独立于成员资格，治疗三十槽位中全部符合条件的己方角色。存活者，以及即使死亡的 Peter (7)/Lemon (28)，获得最大 HP/MP；其他死亡者保持死亡。保留既有 STUN/POISON/CURSE 位，再重建属性值/装备效果。Battle44 计时器例外不适用。
2. 建立返回种子：PARAM2=CURRENT_MAP；重建军团列表；取首个 BATTLE_PARTY_MEMBERS 字节；以字节算术将该战斗员 X/Y 加 BATTLE_AREA_X/Y 存入 PARAM3/4。从实际映射实体读取朝向到 PARAM5，并设置 PARAM1=0。这不写 MAP_EVENT_TYPE。
3. 运行 after 包装器。F523 未设置则分发 `ms_Empty`；已设置则跳过。两者均到达 Bowie 的公共 `JoinForce(0)` 尾部。
4. 清 F423、设置 F523，将 PARAM2–5 字节读入已清零 d0–d3 字，并返回 d4=1。

### 选择成员与坐标

UpdateForce 按 ID0–29 升序扫描，战斗队伍列表要求已加入**且**出战标志。不选择首个存活成员。普通已加入并出战的 Bowie 因 ID0 而排首位，并非胜利硬编码选择他。传入死亡/地图外首成员可建立返回种子；零长度列表使首字节未由此次重建初始化。后面的 Bowie 加入不修复已从该字节取得的种子。

当前 Map27 和已接受区域 `(23,14)` 下，选定当前局部位置 `(x,y)` 产生模256 的 `(x+23,y+14)`，朝向经 ENTITY_INDEX_LIST 从该成员物理实体读取。例如选定成员当前在 `(6,4)`、朝 Left，则得 Map27 `(29,18)`、Left。改变成员资格、区域、位置、地图或映射会改变结果。准入 `(0,0,Right)` 请求和初始编队均非胜利位置。

JoinForce(0) 设置 Bowie 已加入标志 F0 并重建军团列表。出战数量小于12 时，JoinBattleParty 还设置 F32；此写入不即时再次重建计数。不会为了腾位驱逐满员出战队伍。该公共尾部不增加 Creed 招募者，也不保证任意空队伍状态安全。

空脚本仍初始化工作状态/文本跳过处理，有对话窗口时，清滚动速度并返回前等待视野滚动。脚本未完成会阻止加入/标志；加入未完成会阻止清/设。已完成前缀保留。空脚本不提供替换元组；最终读取前的竞争参数写入可改变返回字节，无需等待传送消费。

Battle23 三种结果均恢复到 MainLoop 的**替代入口**：SwitchMap 后 ExplorationLoop，绕过 CheckBattle。该调用方不按 d4 符号分支。没有标志切换 Map27 替换改变此显式返回。[胜利消费者关联](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md#victory-order-and-the-actual-return-seed)保留精确种子、映射、包装器和调用方溯源。

## 败北与撤退使用存档点选择器

**已确认：**Battle23 没有特殊败北/Egress 目的地分支。已获准处理器区别如下；仅凭处理器不确立菜单可用性或实际法术/物品持有。

| 结果 | 返回前恢复或费用 | d4 |
| --- | --- | --- |
| 败北 | 更新解锁；清对话名字；请求悲伤音乐/疲惫文本363 并关闭文本；恢复 Bowie 最大 HP；无符号金币向下取整减半；取得 Egress 位置 | -1 |
| Egress | 按法术费用减少行动战斗员 MP；关闭战场窗口；请求施法文本及共享声音/闪光/关闭；更新解锁、退出行动栈帧并取得 Egress 位置 | 0 |
| Angel Wing | 移除选定物品槽代替 MP 费用，然后走共享撤退尾部 | 0 |

撤退不执行败北的金币损失或 Bowie HP 恢复。败北主体不复活其他死亡己方角色；后续探索独立治疗存活/不死己方角色。均不运行胜利 after/加入，也不设置 F523。UpdateBattleUnlockedFlag 在 F523 未设置时保留 F423，已设置时清 F423。更早准入可能已为传入完成遭遇清 F423。文本/音频请求要求相应服务完成，不是表现观察。

GetEgressPositionForBattle 将 **EGRESS_MAP 作为地图选择器**读取，再查存档点行。不取回出战队伍先前 XYZ/朝向元组。

| 传入状态 | 选定返回或木筏效果 |
| --- | --- |
| F399 已设置，选择器26 | Floor World26 `(4,36)`、Up1 |
| F399 已设置，选择器22 | Desktop22 `(5,8)`、Up1 |
| F399 已设置，选择器27 | 无存档点行：Map27 默认 `(1,1)`、Up1 |
| F399 已设置，其他选择器 | 匹配四字节存档点行，否则该地图默认 `(1,1)`、Up |
| F399 未设置，直接不一致调用 | 初始 Map3 `(56,3)`、Down3；在木筏重置前提前返回 |
| F399 已设置，F64 未设置 | 保留已保存木筏字段 |
| F399 已设置，F64 已设置，选择器26/22/27 | 默认 RAFT_MAP69、RAFT_X/Y `(42,15)`；不登筏 |

BattleLoop 在新或挂起入场前设置 F399，因此未设置示例是传入调用反例。具名选择器没有特殊木筏行；其他 Hassan/Polca/Bedoe 行不改变这些示例。

Floor World 已完成首次抵达 init 先设置 F779，再 EGRESS_MAP26；F779 已设置的重复跳过该赋值。洞穴 init、对峙和 Battle23 钩子/结果不替换它。**若保留该更早选择器**，败北/撤退返回 Map26。进入洞穴或仅传入 F779 已设置，不足以保证 Floor World。见已接受 [Floor World 抵达](floor-world-first-arrival.md)及[存档点消费者关联](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md#defeat-egress-and-the-savepoint-selector)。

三种结果的替代 MainLoop 返回均绕过 CheckBattle。因此保留的 F423 不在那里即时重新准入 Battle23；后续普通 MainLoop 遍处理可能使用它。实际返回地图/状态控制加载器。Map26/22 时本指南止于准入，早于目的地 init。

## 新洞穴返回替换战场

**已确认：**显式 Map27 胜利返回执行新 ExplorationLoop 配置：清事件类型、调整步数计数器、治疗符合条件己方角色、请求淡出并禁用摄像机跟随；赋 CURRENT_MAP=27/CURRENT_BATTLE=255；加载瓦片集、等待淡入淡出/VInt 工作并重建实体/地图精灵。清临时标志256–383 并设置 F80。F423/473/523/781 不受该重置范围影响。公共加载初始化精灵、以 physical0 为目标、加载地图、安装基础 VInts 并调用地图 init。

LoadMap 接收 d0=-1 用于视野选择，但**完整加载使用 d1=27**。CURRENT_BATTLE=255 时跳过战斗裁切并恢复完整洞穴，而非保留27x16 战场。Map27 没有标志事件布局复制或宝箱覆盖：两个完整文件仅含 endWord。F523 改变居民选择，不是已证明的墙/碰撞开口。

| 新选择时 F523 | 居民列表 |
| --- | --- |
| 未设置 | Rat79 `(43,20)`、Willard154 `(43,21)`、Rat79 `(43,22)`，均为 Left2/eas_Init |
| 已设置 | 仅终止符：零居民 |

配置指针表共享回调、区域（事件）、描述、Section5 和 init；仅居民列表指针不同。每个访问器重新选择配置。胜利在选择前设置 F523。行动者记录/查找清除；随从从 physical1 分配，然后居民，Bowie 则以返回元组/eas_Idle 声明于 physical0。主角坐标乘384，不使用居民行的六位坐标掩码。空居民列表仍允许主角和传入随从。

普通随从示例中，仅 F67/F68 分配 Peter (7) 于 physical1、B (logical31)/Oddler 于 physical2；仅成员资格不会。其他标志移动分配/别名，模式/木筏规则仍适用。BattleLoop 将 PLAYER_TYPE 清为0；这些结果/init 不改变它。前篇[完整实体群所有者](../../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md#admission-setup-and-the-complete-population)保留其他分配及逻辑/物理/木筏别名。

战斗敌人128 是 Willard，而基线探索 logical129 是 Willard，128/130 是 Rats。F523 已设置时，清除后居民查找32–34 保持零。缺席居民脚本查找解析为 physical0，而非安全无操作。战斗行动者映射不能复用于该新探索列表。

Map27 init **仅为 rts**。不增加对话、布置、奖励、传送、EGRESS 赋值或 F781 修复。条件音乐/淡入淡出请求后，WaitForEvent 先处理待处理事件。只有无待处理类型才允许 physical0 当前模式控制行动；mode0 选择 eas_ControlledCharacter。仅 init 返回不证明服务完成或可用输入。

### CURRENT255 保留不同状态

传入地图255 重新入场保留当前地图/实体群，跳过新瓦片集、分配、临时重置和 CURRENT_BATTLE 赋值，更新主角及木筏/随从，再复用工作布局并调用 init。仅 F523 不能在此路径移除已分配居民或规范保留战斗状态。这不同于普通显式 Map27 胜利加载。[新/保留返回关联](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md#fresh-map27-return-and-conditional-exploration)负责两个完整加载分支。

## 既有出口仍是条件请求

**已确认：**四个独立标志有不同消费者：

| 标志 | 消费者与结果效果 |
| --- | --- |
| F423 | 战斗准入；胜利清除；败北/撤退仅在 F523 已设置时清除 |
| F473 | before/start 开场门槛；start 设置，结果/init 不设置 |
| F523 | 完成与新居民选择；胜利设置；没有直接退出谓词 |
| F781 | Y19 对峙门槛；回调在场景返回后设置；结果/init 不设置 |

F523 已设置/F781 未设置不被修复。后续 Y19 区域（事件）调用可能经 physical0 寻址缺失居民，无法正常完成。若服务确实完成，既有场景可设置 F423 并再次请求 Battle23；之后 F523 在 CheckBattle 中清 F423，但不取消选定遭遇。F781 已设置则即使 F473 未设置也抑制对峙。调用方标志写入和待处理请求先于立绘/文本/VInt/physical0 停止等待；均不证明包装器返回或请求消费。完整对峙仍在[准入指南](willard-burrow-through-battle23-entry.md)。

未改变表包含八个不滚动/type0 请求：

| 行 | 触发 X/Y | 目的地地图 / X/Y | 朝向 |
| --- | --- | --- | --- |
| 0 | `(2,8)` | Floor World26 `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 `(3,3)` | Right0 |
| 6 | `(255,0)` | Creed Mansion33 `(15,9)` | Right0 |
| 7 | `(21,255)` | Creed Mansion33 `(15,9)` | Right0 |

每条记录八字节，包括朝向后零填充，整表后接 endWord。负触发字节为通配符：行6 是 Y0 任意 X，行7 是 X21 任意 Y，首匹配胜出。无反向 `(3,3)` 行。朝向是输出。匹配器/表不测试 F423/F473/F523/F781 中任何一个、不消费物品，也不授予招募。

普通 esc02 要求候选布局在掩码0x3C00 下为类别0x1000，之后传送匹配器才有符号除原始 X/Y 以384 并扫描。仅坐标相等不证明可行走路线。独立区域（事件）分支使用类别0x1400 和低字节选择器。匹配请求 event type1、PARAM1–6 和传送声音；待处理消费和服务完成仍必需。

ProcessMapEvent 在分发前清采样事件类型。type0 传送消费者提交/清声音、让 physical0 空闲，并以 d4=0 返回**普通 MainLoop**。它从 physical0 派生有符号位置字，仅以非负 PARAM3/4 覆盖低字节。显式目的地字节不修复继承的异常高字节；上述一致元组假设该限制。

SwitchMap 不替换26/27/33/255。显式26/33 无战斗行并准入探索；止于其实体群/init 之前。CURRENT255 为 CheckBattle 解析当前 Map27：F423 未设置保留255 请求以保留式探索；已设置选择 Battle23 并写区域 `(23,14)`、27x16。F523 已设置在选择中清 F423，但仍准入战斗。这些是条件消费者，不是迷宫解法、完成撤退或 Creed 场景。

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `6498fd588d3ed12b9d38392d97778e9e4b90af90` 包含已修正 #937/PR939 和 #938/PR940。研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../../../research/chapters/southern-travel-creed/battle23-outcomes-and-burrow-return.md#reproduction-and-remaining-boundaries)保留精确源码符号/地址、表槽位和完整消费者关联。不使用新观察、提取或 #941 发现。

原英文切片的对抗审阅覆盖完整修正研究、前篇编队/准入和选定控制合同、公共 fixture 载荷、schema 及验证器。逐个比较精确十二身份、1/5/6 数量、八条传送和结果顺序，不从总数推断。选定表关联为 defeated48、leader45、after48、join52 的 slot23；cutscenes 的 join 表负责 Bowie0，而非名称相似的全局表。

| 可执行所有者 | 声明与限制 |
| --- | --- |
| `sf2-battle-loop-static-v1`，tests/fixtures/h2/battle-loop-static-v1.json | expected.lifecycleFacts：30/32 槽、X/HP 计数、Bowie 覆盖、治疗与清理。匹配 h2-battle-loop-static-fixture schema；battle_loop.py `_build_lifecycle_facts`/验证器。调试 kill-all 的列表重置/HP 清除不是首领尾部。 |
| `sf2-battle-control-static-v1`，tests/fixtures/h2/battle-control-static-v1.json | expected.controlFacts：两次结束检查、结果顺序、d4 和金币操作。匹配 control fixture schema；battle_control.py `_build_control_facts`/验证器。有序源码片段不证明自然执行或种子正确。 |
| `sf2-battle-cutscenes-static-v1`，tests/fixtures/h2/battle-cutscenes-static-v1.json | expected.cutsceneFacts：HP 谓词、完成脚本跳过、首领追加及共享加入。匹配 cutscenes fixture schema；battle_cutscenes.py `_cutscene_facts`/验证器。研究独立负责精确槽位23 表/主体选择。 |
| `sf2-map-init-static-v1`，tests/fixtures/h2/map-init-static-v1.json | 配置/init 路由与内嵌程序比较；匹配 map-init fixture schema 和 map_init.py 验证器。仅返回 init 不确立可用控制。 |
| `sf2-map-script-engine-static-v1`，tests/fixtures/h2/map-script-engine-static-v1.json | 共享脚本/请求消费者；匹配 map_script_schema.py fixture schema 和 map_script_engine.py `_verify_map_script_engine_fixture`。不执行此组合结果，也不证明待处理交付。 |

schema 位于 `schemas/`；验证器位于 `src/sf2tool/h2/`。既有窄命令为 `uv run sf2 h2` 后接 `battle-loop`、`battle-control`、`battle-cutscenes`、`map-init` 或 `map-script-engine`。保留研究比较在具名边界通过；原英文设计切片检出缺少固定 upstream/保留对，因此不算新重跑或再生成数据。汇总相等不能守护新正文语义。

[#937 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252)、[P2 审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395)、[修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382)保留原始/修正结果及已取代的连续 ID 错误。[#938 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938#issuecomment-6177356652)、[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938#issuecomment-6177400955)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/940#issuecomment-6177419250)保留初始化结果、缺输入诊断和前篇失败。这些记录对各自精确对象、CI 与收据仍具权威。

原英文切片执行直接完整正文/diff、表/顺序/改变状态、链接/锚点、英文/私有检查及 git diff --check，然后检查已配置干净已提交 planner 与实际最终 head CI。不单因正文选择局部一揽子套件、获取或新机制。

**未知：**自然首领/全敌结果和退出可达性；传入死亡列表、出战队伍/HP/布置/映射/标志/当前或保存几何、EGRESS_MAP、模式/木筏；脚本/加载/表现/输入服务完成；可见/可听时序、可用控制及持久性。空/满队伍与中断前缀示例保留这些限制。[ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)不创建自动运行时队列。目的地 init、地下室/招募、Creed 场景和后续战斗仍在本指南及重制验收之外。
