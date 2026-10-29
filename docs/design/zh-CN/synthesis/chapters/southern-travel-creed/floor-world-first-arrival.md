# Floor World 首次抵达与条件探索

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/floor-world-first-arrival.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**存续的 Cotton Balloon 离开请求准入 Map26 探索，重建九个实体群行。
F779 未设置时，目的地 init 运行完整降落场景，然后设置 F779 并写 EGRESS_MAP=26。
F780 独立控制居民 129 的停放。**推断：**六个行动者表现 balloon 降落和爆裂。
**未知：**自然抵达、实际传入状态、行动/服务完成、视听表现、可用控制及持久性。

本指南从 [Cotton Balloon 获取与离开](cotton-balloon-through-floor-world-entry.md)的条件加载器准入接续。
仅使用已接受 [Floor World 抵达研究](../../../../../research/chapters/southern-travel-creed/floor-world-first-arrival.md)，依据
[Issue #926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/926) 和
[当前 Standard 约定](../../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
止于条件局部控制器安装或待处理事件处理。
[居民、巨环和退出后续](floor-world-interactions-and-exits.md)解释下一局部交互，直至条件目的地准入。
目的地 init、隧道内容、Battle23 和招募仍是后续边界。
Fnnn 表示标志 nnn；命令及实体群索引从零开始。坐标、速度、等待是源码操作数，不是测得移动、像素或已过时间。

## 新实体群替换 Desktop 角色

**已确认：**前篇存续请求为 Map26 `(26,8)`、Down。
SwitchMap 不替换为另一地图，CheckBattle 找不到 Map26 战斗行；Battle23 使用 Map27。
准入开始探索加载，不证明自然到达或控制。离开场景未写 EGRESS_MAP；目的地后续写入为独立效果。

显式地图分支上，ExplorationLoop 清待处理事件、调整步数计数器、治疗合格存活/不死己方，
开始淡出并禁用摄像机跟随。它写 CURRENT_MAP=26、CURRENT_BATTLE=255，加载瓦片集、等待淡入淡出、
清实体中断、初始化选定实体群并加载精灵。它清临时标志 256–383、设置 F80、初始化精灵帧状态并以 physical 0 为目标。
LoadMap 和基础中断安装先于 RunMapSetupInitFunction。这既不清 F779/F780，也不确立每个己方都存活。

全新加载替换 Desktop 行动者、查找及移动；Desktop137–139 不带入 Floor World130–132。
Map26 只有一个默认配置 `ms_map26`，没有标志选择的替代实体群。
六个配置指针选择实体、实体事件、区域（事件）、区域描述、section 5 和 init；各访问器均解析当前地图。
改变 F522/F779/F780 不选择不同的九行表。

InitializeMapEntities 清记录/查找，从 physical 1 分配选定随从，再依次消费九行。
它在 physical 0 按请求位置/朝向、eas_Idle 声明 Bowie，写共享 SPRITE_SIZE=16，
终止随从列表，并进行独立木筏刷新。

完整[实体群与映射所有者](../../../../../research/chapters/southern-travel-creed/floor-world-first-arrival.md#complete-population-and-logical-to-physical-mapping)
为 `$59D20..$59D6A` 的 `s1_entities.asm`。
下文 physical 示例假设仅 F67/Peter、F68/Oddler，没有跳过己方行，映射互异且无覆盖。
Init/Fixed 表示 eas_Init/eas_InitFixedSprite。

| 行 | Logical 行动者 | Physical 示例 | 精灵 / 初始行动 | 瓦片、朝向 |
| --- | --- | --- | --- | --- |
| 0 | Randolf16 | 3 | 己方精灵查找；Init | `(27,22)`，Right |
| 1 | Tyrin17 | 4 | 己方精灵查找；Init | `(29,22)`，Left |
| 2 | NPC128 | 5 | OLD_MAN198；步行原点 `(50,12)`，range3 | `(50,12)`，Down |
| 3 | NPC129 | 6 | YOUNG_MAN196；Init | `(5,24)`，Down |
| 4 | NPC130 | 7 | OBJECT1=173；Fixed | `(63,63)`，Up |
| 5 | NPC131 | 8 | OBJECT1=173；Fixed | `(63,63)`，Up |
| 6 | NPC132 | 9 | OBJECT1=173；Fixed | `(63,63)`，Up |
| 7 | NPC133 | 10 | PRIEST192；Init | `(4,35)`，Down |
| 8 | NPC134 | 11 | CARAVAN62；Init | `(11,35)`，Down |

Randolf 和 Tyrin 的出现不经过已加入军团或出战队伍准入测试。这不是招募。
HP/职业决定实际精灵，包括死亡探索己方的 BLUE_FLAME。
先前存在的非零己方查找会跳过该行，不消耗 physical 槽位；普通全新随从表本身不声明二人。
七个普通精灵行使用 NPC 查找游标，独立于两个己方行。
Logical129 因而指年轻男子，通常 physical 6，不是 Tyrin 或 balloon 对象。
老人的复制步行程序不证明其后续观察位置。

### 随从与交通仍为传入前提

Map26 既不是大地图，也不是 Tactical Base 或 Nazca Ship，因此 F65 不添加大地图 Caravan/A 前缀。
仅 F67/F68 时，Peter7 为 physical 1、eas_Follower1；B31 为 physical 2、eas_Follower2，初始均在请求位置/朝向。
B 是脚本身份：Oddler 示例要求 F68。已接受随从声明顺序也允许 Astral/B 及其他 A/B/D 别名。
后续声明替换 logical 查找，同时更早 physical 记录仍在；其他随从会移动居民槽位。

仅已加入成员资格不会在此分配 Peter 或 B。缺失零查找解析至 physical 0，因此名义不同命令可寻址 Bowie；
场景不创建缺失随从，也不复活/招募任何人。仅映射一致且互异时，六个 logical 对象才是六个不同 physical 行动者。
实际 HP、职业、随从及查找必须保留为前提。

全新加载木筏辅助可在 mode 2 替换主角色精灵，将随从停在原始 `$6000,$6000`，并在 `(64,64)` 声明 physical 31 为 RAFT，
不测试已保存地图。否则 F64 加 RAFT_MAP=26 准入 physical 31，使用以 `$7F` 掩码的已保存 X/Y。
查找 offset `$3F` 和 physical 记录可被覆盖。普通双随从角色占 0–11，独立于 31；不引入 Desktop 的 slot31 重叠。
加载与降落均不证明普通步行模式、不修复别名，也不清已保存木筏状态。

## 按完整阶段阅读降落

**已确认：**`$59DDC` 的 init `ms_map26_InitFunction` 先测试 F779。
未设置则运行 `$59E04` 的 `cs_59E04`；已设置则绕过。
此分组覆盖源码行 28–98 中全部 41 个顶层命令。
[精确命令与嵌套体表](../../../../../research/chapters/southern-travel-creed/floor-world-first-arrival.md#init-dispatcher-and-all-41-landing-commands)
仍为操作数权威。嵌入的 map-init fixture 还记录 71 条语句，包括十八条自定义体和十二条内联体语句；
它们不是 71 个顶层场景命令。

下文“六者顺序”为 Bowie、Peter、B、130、131、132。每个自定义体以 ac_end 结束；每个内联移动体以 endActions 结束。

| 索引 | 有序工作 |
| --- | --- |
| 0–2 | 选择文本 1739；中立摄像机；在 `(21,8)` 重载当前地图视野。 |
| 3–5 | 分别为 Bowie、Peter、B 安装/等待 eas_Init。 |
| 6–8 | 停止 Bowie、Peter、B 动画计数。 |
| 9–14 | 按六者顺序执行六个独立 customActscriptWait 程序体：speed 10/10；jump eas_Idle；end。 |
| 15–17 | 放置 Bowie `(26,7)`、Peter `(27,6)`、B `(25,6)`，均 Down。 |
| 18–20 | 放置 130 `(26,6)`、131 `(25,5)`、132 `(27,5)`，均 Up。 |
| 21 | 调用 fadeInB。 |
| 22–26 | 为 Bowie/Peter/B/130/131 安装独立 `moveDown 7; endActions`。 |
| 27 | 为 132 安装同一移动；仅等待 132 idle 指针。 |
| 28–32 | Sleep 20；按 Bowie/Peter/B 顺序重启三人动画；sleep 40。 |
| 33–36 | 依次 hide 130、131、132；sleep 30。 |
| 37 | nextSingleText `$FF,255`，使用文本 1739。 |
| 38–40 | 安装 Peter-follow-Bowie offset 2，再 B-follow-Peter offset 2；结束场景。 |

### 准备视野与行动者

Trap6 在 ExecuteMapScript 之前激活实体更新。中立摄像机写负目标 `$FF`，不解析行动者。
reloadMap 禁用跟随、停用中断，以地图索引 -1 及压缩视野操作数 21/8 乘以 3 调用 LoadMap。
它复用当前地图的区域属性及工作布局，绕过 LoadMapBlocksAndLayout；之后启用显示/中断并等待一次 VInt。
它不重建行动者、不清临时标志、不还原初始位置，也不递归调用 init。
`(21,8)` 是视野输入，不是另一次生成或传送。

三个带等待 Init 命令确立零加速度因子、speed 32/32、加速/减速关、obstructable 开、
entity/map uncollidable 关、自动朝向开、Up orientation、immersion 关、共享 SPRITE_SIZE=24，
然后更新精灵并跳至 idle。不重置位置、HP、行进/速度分量，也不开启动画。
三个对象行以 Fixed 开始，另禁用自动朝向/动画；场景不给它们带等待 Init。
自定义速度行动替换原 Init 前，不另行汇合其完成。共享精灵尺寸不是持久世界比例。

六个自定义体各将 X/Y 速度改为 10/10 并跳至 idle，不还原其他 Init 属性。
带等待行动安装以 physical 索引初始化计时器，轮询该行动者行动指针直至 eas_Idle。
没有 HP/缺失行动者守卫、超时或全行动者屏障。进展要求实际实体更新；后续指针安装可替换工作。

stopEntity 写动画计数器 -1；后续 startEntity 写 0。
它们改变动画计数，不改变速度分量、目的地或行动完成。
setPos 将当前和目的地坐标写为绝对瓦片×384，写朝向并更新精灵，不重置行动、速度分量或速度。
放置和动画命令跳过 HP0 的真正己方 0–29，包括 Bowie/Peter；B31 和 NPC130–132 绕过该守卫。
行动安装仍无守卫。因此死亡己方并非统一被跳过，请求起点也无保证。

fadeInB 调用 FadeInFromBlack，其服务等待淡入淡出状态清除及额外一次 VInt。
仅返回后才允许移动命令。这是源码服务条件，不是已观察显示或测得时长。

### 六次向下移动只有一个选定等待

csc2D 清目标 FLAGS_A bits 5/6，将各 moveDown 7 编译为从行动者实际当前位置起的相对 `(0,+7)`、
对当前目的地的等待，再跳至 idle。速度字节决定有符号行进速度分量。
bit5 未设置的目的地扫描分支绕过选定冲突分支；这不证明通用碰撞/物理规则或目的地占用门控。
安装依次发生，允许并发进展，但不是同时开始，也没有行动者附着或共用 balloon 物理对象。

若每次放置均生效，且在移动执行时仍为当前位置，请求目的地为 Bowie `(26,14)`、Peter `(27,13)`、B `(25,13)`、
130 `(26,13)`、131 `(25,12)`、132 `(27,12)`。
这些是条件目的地，不是保证最终坐标。移动等待比较行动者当前位置与当前目的地，而非预留的不变端点。

仅 132 的 idle 指针等待在地图脚本层汇合移动。即使未改变的 132 到达目的地，也不能证明其他五者。
Sleeps20/40/30 不添加完成检查。动画重启、隐藏和跟随因此可发生在其他行动者完成尚未确立之前。
指针、目的地或别名改变以及更新停滞均可改变/阻止进展。
实体更新跳过原始 X 至少 `$7000` 的行动者；`(63,63)` 低于该界限，但传入已隐藏行动者可能无法执行新行动。
精灵加载容量也可延迟行动进展。

### 移除对象与命名爆裂

hide 130/131/132 独立调用 HideEntity，没有 HP 守卫或其他行动者等待。
它写当前/目的地原始 `$7000,$7000`，将**全部匹配** ENTITY_INDEX_LIST 条目重写为 `$FF`，
并通过移动列表从 EXPLORATION_ENTITIES 移除匹配条目。
这使查找及随从列表成员资格失效；不招募、不释放物品栏、不改变精灵，也不请求爆裂声音。
缺失零映射可隐藏主角色；别名可使多个 logical 身份失效。之后访问失效查找不是安全的缺失行动者无操作或普通实体群访问。

文本 1739、key06CB 的字面原文为 `The Cotton Balloon popped.{W1}`。
nextSingleText 操作数编码字 `$FFFF`：处理器绕过立绘和说话者精灵选择，设置 CURRENT_SPEECH_SFX=0。
这是哨兵值，不是 NPC255 说话或 Peter/Oddler 立绘/bleep 请求。
先前打开窗口不确立新选中的立绘。

普通情况下，处理器等待视野滚动、显示文本、递增游标、关闭立绘/文本并 sleeps10。
W1 等待方向/A/B/C 输入。调试模式下 P2 Start 可锁存 SKIP_CUTSCENE_TEXT，绕过表现、游标递增和 csWait sleep，
而行动/标志命令仍执行。这些是有边界源码事实，不是已观察 UX。
字面文本称发生爆裂；没有显式 playSound、爆炸或物理命令。
表现解释为**推断**；可见/音频演出和时长仍**未知**。

### 跟随是已安装，而非已观察完成

followEntity 先读字随从操作数之前的一个**字节**。Peter 的 `$0007` 和 B 的 `$001F` 高字节均为 0，
因此两个准入检查都测试 **Bowie HP**，不是 Peter 或 B HP。
准入后，完整字才解析实际随从/目标映射。Offset 2 选择有符号 `(-24,0)`。

AddFollower 将 eas_Follower1 的 42 字节复制至步行参数存储，修补目标/偏移字、安装指针，
并仅在缺失时追加 physical 随从。Peter 跟随 Bowie；随后 B 跟随 Peter。
执行复制体配置 speed 32/32 及随从碰撞/加速度设置；循环使用目标当前/目的地/朝向及布局请求行进、等待目的地并重复。
这不是传送、绝对阵形、治疗、招募、剧情/随从标志写入或已完成跟随观察。
它可替换未完成移动；映射改变可形成不同或自引用链。没有最终带等待 Init 重置三人。
Bowie 控制器在调用方返回后另行安装。

## 返回、重复及传入不同状态

**已确认：**csc_end 在有对话窗口时有条件地等待视野滚动，清 VIEW_SCROLLING_SPEED 并还原调用方寄存器。
它不添加全行动者等待。仅场景返回才允许 init 设置 F779，然后写 EGRESS_MAP=26，再独立检查 F780。
RunMapSetupInitFunction 还原保存寄存器，没有物品/区域（事件）包装器的额外主角色目的地等待或关窗口。

| 到达各检查时的标志 | 条件 init 结果 |
| --- | --- |
| F779 未设置，F780 未设置 | 场景返回 → F779 已设置 → EGRESS_MAP26 → 返回；129 保留。 |
| F779 未设置，F780 已设置 | 同一场景/标志/EGRESS 序列 → 停放 129 → 返回。 |
| F779 已设置，F780 未设置 | 跳过场景和两个写入；保留既有 EGRESS_MAP。 |
| F779 已设置，F780 已设置 | 跳过场景/EGRESS 写入；停放 129；返回。 |

F780 不抑制降落，也不由此尾部设置。
`$477EA` 的 MoveEntityOutOfMap 经 NPC129-minus96 查找解析 logical129，仅将当前/目的地 X/Y 写为原始 `$7000`。
它保留查找、随从列表、行动、精灵及动画。这是**停放**年轻男子，通常 physical 6；
不是 HideEntity 的查找/列表移除。别名改变可停放另一记录。这些辅助不能互换。

全新重复加载仍重建全部九行，包括 `(63,63)` 的对象。
单独 F779 跳过其场景放置/隐藏及最终跟随复制；全新随从声明仍适用。
F780 单独停放 129。传入 F779 既不证明更早完成降落，也不证明 EGRESS_MAP26；
被跳过分支不修复旧 EGRESS 值。标志到达时读取，不预留入口快照。

传入地图索引 255 则保留当前实体群/查找/布局。它更新主角色属性并调用 DeclareRaftEntity，
然后汇入精灵帧、LoadMap 和 init 工作，没有全新分配、瓦片集、临时标志清除或 F80 设置。
传入非负 X/Y 更新主角色当前/目的地及朝向/精灵。
Mode2 从此木筏辅助立即返回；mode 1 跳过随从重定位；其他模式重定位列表随从。
然后 F64 控制独立木筏声明：已保存地图匹配则声明 physical 31 和查找 `$3F`；
地图不同则清该查找并停放 31。这不同于全新加载木筏刷新。
场景的当前布局重载和场景返回均不调用该替代探索入场分支。

前缀中断可留下放置、隐藏或跟随，而 F779 仍未设置。
F779 设置后、EGRESS 写入前中断，可留下重复入场不会修复的旧 EGRESS 值。
EGRESS 后、F780 尾部前，129 可仍存在。没有通用回滚。
这些是静态改变状态区分，不是已观察中断、存档或持久性结果。

## 场景返回是条件控制交接

**已确认：**回调返回后，ExplorationLoop 比较长字 PALETTE_1_BASE_02 和 PALETTE_1_CURRENT_02。
仅不相等才调用 PlayMapMusic，再 FadeInFromBlack。
早先场景淡入淡出既不确立该比较结果，也不确立实际音乐/画面状态。随后是 SetMoveSfx 和 WaitForEvent。

WaitForEvent 入口 MAP_EVENT_TYPE 为零时，physical 0 接收当前模式控制器：
mode 0 为 eas_ControlledCharacter；mode 1 为源码名 eas_Raft（Caravan 控制）；其他非零模式为 eas_Caravan（木筏控制）。
源码名称不交换使用方含义，也不验证任意传入模式。步行行动执行时配置 speed 32 和控制循环；
仅场景返回尚未还原每个字段或准入可用输入。

非零待处理事件跳过控制器安装，并优先于 A/C 输入。
ProcessMapEvent 在分发前清采样事件类型；当前参数决定后续工作。
降落本身不写传送/战斗请求、PLAYER_TYPE 或已保存木筏状态。
因此服务、事件状态、模式、别名和实际控制器执行仍为可用探索前提。

本指南止于此。继续阅读 [Floor World 居民、巨环与对外退出](floor-world-interactions-and-exits.md)，
了解六个居民回调/默认、独立 F780 区域（事件）、限定范围的空列表及三个退出请求直至条件准入。
该指南使用已接受修正 #924，保留同次访问隐藏与全新加载停放的边界。
本抵达指南不扩展至那些回调、目的地初始化、隧道内容、Battle23 或后续招募。

## 证据审阅与保留限制

**已确认溯源：**已接受 main `6e8803feebaf95931246e6e435c305a8916a9aac` 包含 #920/PR922 和 #923/PR925。
研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
上文源码路径相对于 disasm，map26 配置文件位于 data/maps/entries/map26/mapsetups。
本综合指南不创建提取、载荷、运行时观察或重制决策。

对抗审阅使用完整已接受抵达所有者及前篇、
[探索控制合同](../../../contracts/world/exploration-control-flow.md)和下列选定既有边界。
[独立程序数据合同](../../../contracts/world/standalone-map-script-program-data.md)负责数据/溯源，不执行或渲染该 init 嵌入场景。

| 既有 fixture 与复现路线 | 选定边界和限制 |
| --- | --- |
| [sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`uv run sf2 h2 map-init` | routeJoins sourceOrder 63、pointerTableJoins sourceOrder 66（`$59D08` → `$59DDC`）、scriptCallSites siteOrder 44 及 directCallSites siteOrder 40 绑定配置/init/场景/停放调用。完整十操作 init 及嵌入 71 语句场景与已接受 41 命令分组匹配。源码顺序身份不是游戏时间顺序。 |
| [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`uv run sf2 h2 map-script-engine` | 选定生命周期/重载、行动、放置、动画/隐藏、文本及跟随处理器事实/守卫绑定局部接缝。不汇合每个被调用方完成，也不证明实际运动/表现结果。 |
| [sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`uv run sf2 h2 map-event-request-consumption` | explorationLoop 重置、waitForEvent 轮询及 processMapEvent 分发前清除；控制器执行和可用控制需要实际使用方。 |

既有 schema/验证器路线为
[init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)/[验证器](../../../../../../src/sf2tool/h2/map_init.py)、
[script schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)/[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)，
以及 [request schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)。
Init/script fixture 投影在 expected 下；请求使用方位于顶层 eventRequestConsumption.consumerContexts。
flag-route-selection fixture 没有 ms_map26_InitFunction 程序路由上下文：显式 init 关联负责此路线。
汇总守卫不独立验证每项正文声明；实体群/被调用方细节仍为已接受研究的直接关联。

保留 [#920 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6175992255)、
[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6176088792)和
[PR922 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/922#issuecomment-6176117381)：
原始 head `bd40b052b04ab087738f5fc170e406f97a244ad6`、tree `fc16905271421b3eda529e8bd30960516712cf2d`、
base `63fab597ee78136cd88f7cdefa2bc6c4512cf6b5`；
变基 head `7a83e84530bf01bd41c3e071886a24b086a5783e`、tree `cb3b2d356b73e72290bfc66fd0359c0974b6e42a`、
base `5e874fc507c0d740fb7a64536a69aa9a2c0d1d18`。
[CI38619448174](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38619448174)和
[CI38620203806](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38620203806)
通过 scope/research-public；engine/adapter/H4 按范围跳过。
未改变 18,285,979 字节对在 0.079s 内比较通过。
保留输出截断、字面通配符、混合路径 ValueError、猜测源码/宏路径及启动前写入被拒诊断，修正也在这些收据中。
这里不替换或重跑已完成结果。

[#923 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/923#issuecomment-6176246113)及
[PR925 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/925#issuecomment-6176272977)
保留修正的逐服务所有权及地址冲突：陈旧 `$4F4AA/$4F566` 对已接受 `$4F48A/$4F542`，
连同原始失败断言、已授权修正及
[CI38621272226](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38621272226)。
GetItemInventoryLocation `$81D0` → `$9146` 和 RemoveItemBySlot `$819C` → `$8E76` 属于 itemstats.asm；
ReceiveMandatoryItem `$4F48A` 和 RemoveItemFromInventory `$4F542` 属于 iteminventory.asm。
替代 items/itemfunctions_s7_0.asm 仍排除。前篇
[逐服务所有权边界](../../../../../research/world/map-event-item-transactions.md#ownership-and-unknown-boundary)及调用方/被调用方区别不变。
[#919 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/919#issuecomment-6175962879)/[PR921 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/921#issuecomment-6176035685)
及其 [#916 证据收据](cotton-balloon-through-floor-world-entry.md#证据审阅与剩余限制)
保留全部更早对象、比较、已完成失败及未知。

直接验收覆盖完整自有正文/diff、数量和命令覆盖、等待/HP/查找区别、溯源、相对链接/片段、表/代码围栏、英文/私有边界及 git diff --check。
检查已配置的干净已提交 `uv run sf2 verify plan --base origin/main --head HEAD` 和实际最终 head CI。
研究的[复现所有者](../../../../../research/chapters/southern-travel-creed/floor-world-first-arrival.md#reproduction-and-retained-limits)保留输入/比较命令；
本正文切片不选择局部一揽子 normal/full/engine/Godot/H3/H4 或翻译套件、获取或再生成。

**未知：**自然抵达；实际军团/HP/职业、标志、随从/查找、模式/木筏、布局及位置；
已完成行动、精灵、滚动、文本/输入、淡入淡出/音乐/控制服务；运动/时长、视听表现、可用探索、存档及持久性。
改变状态示例是源码反例，不是已游玩场景；依
[ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)及
[ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)，也不构成自动运行时队列。
本指南既未完成战役，也不扩展重制验收。
