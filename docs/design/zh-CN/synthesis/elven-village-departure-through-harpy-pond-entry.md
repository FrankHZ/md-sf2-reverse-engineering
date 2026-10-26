# Elven Village 离开至 Harpy Pond 入场

> 本文是[规范英文原文](../../synthesis/elven-village-departure-through-harpy-pond-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

离开 Elven Village 可使队伍返回 Southwest Parmecia。另行选定的西边界滚动将当前队伍带入 Devil's Tail，Oddler 在那里听见有人求救，可打断移动。该场景请求 Harpy Pond；独立入口也请求池塘。若普通入场没有先接纳 Battle20，池塘初始场景便指出一名陷入困境的 精灵男孩，并提议帮助他。

**已确认：**这些是选定源码程序及条件请求／实体生成使用方。**推断：**两段场景引出救援。**未知：**自然村庄至池塘行程、实际调用方状态、表现完成和可用控制。对话提出帮助；两段场景均不救援或招募 Elric、不打开通道，也不解锁战斗。

本指南使用已接受的[离开与入场研究](../../../research/elven-village-departure-through-harpy-pond-entry.md)，接续可选[村庄对话与局部探索](elven-village-conversations-and-local-exploration.md)。选定出口／请求程序不施加村庄活动前提。范围止于条件池塘控制、另一待处理或获准继续，或**开场前** Battle20 准入。池塘接近／出口警告、战斗内容／结果、Elric 救援／招募、F751 通道、Fairy Cave/Creed 及 Battle21 内容均不在范围内。独立的 [Harpy Pond 接近至 Battle20 入场](harpy-pond-approach-through-battle20-entry.md)指南接续此停止边界。Fnnn 表示标志 nnn；行和命令记录从零开始。

## 选定村庄出口

**已确认：**Map23 有序传送表以三个出口开始。每个都请求无滚动返回 Southwest Parmecia 的 Map68 `(48,31)`、朝向 Down：

| 行 | 匹配村庄瓦片 | 选择后果 |
| --- | --- | --- |
| 0 | 任意 X、Y 1 | 北出口；在也匹配后续行的角落优先。 |
| 1 | X 0、任意 Y | row 0 未匹配时的西出口。 |
| 2 | 任意 X、Y 30 | 此前行均未匹配时的南出口。 |

内部楼梯保留局部所有者。排除前往 Map24 的东出口。WarpIfSetAtPoint 将传入内部坐标除以 384 后按源码顺序扫描；负坐标字节为通配。它写入 event 1、parameters 1–6 及 WARP_SFX。移动／布局准入保持独立：表不证明步行或载具可自然到达出口。

普通无滚动使用方在分发前清除待处理状态，提交并清除当前 WARP_SFX，使 physical actor 0 空闲并展开探索。它采样当前元组，替换非负请求坐标并返回 `(68,48,31,Down,d4=0)`，保留 PLAYER_TYPE。MainLoop 在全新探索前运行 SwitchMap 和 CheckBattle；Maps68、67 或 51 均不应用标志切换替换。

### 战斗准入先于全新实体生成

选定战斗行区分准入标志与完成标志：

| 地图／唯一战斗行 | 传入解锁 | 完成 | 触发 | 区域原点及大小 |
| --- | --- | --- | --- | --- |
| 68 / 19 | F419 | F519 | 任意 X/Y | `(5,19)`、48x32 |
| 67 / 21 | F421 | F521 | 任意 X/Y | `(21,0)`、32x42 |
| 51 / 20 | F420 | F520 | 任意 X/Y | `(0,0)`、18x26 |

CheckBattle 使用解锁与触发字节；区域矩形是输出，不是包含检查。选择已完成行后，它清除该行解锁，但不取消当前选择。因此村庄离开时传入 F419 即使 F519 已设置也会接纳 Battle19。止于该无关准入。选定出口均不产生这些标志。

F419 未设置时，全新 Map68 探索清除待处理状态，从 STEP_COUNTER 减去 20000，并以有符号字负结果钳位到零，治疗存活／不死己方角色，淡出，加载瓦片集并写入 CURRENT_MAP=68/CURRENT_BATTLE=255。它重建实体群，清除临时标志 256–383，设置 F80，加载地图并调用仅返回的 init。Map68 无固定行动者行。

实体生成从 physical 1 分配按标志选择的随从，另行分配 main actor 0；main 0 不进入全新 EXPLORATION_ENTITIES 随从列表。在大地图 Map68，F65 在普通表之前添加 Caravan 并绑定 FOLLOWER_A。仅选定 F65/F67/F68 时，Caravan、Peter 和 Oddler/FOLLOWER_B 占 physical 1/2/3。这检查标志，不检查 HP 或军团成员资格。后续 F74/F77/F83 行可覆盖 A 查找；F70 可覆盖 B。F65 未设置既不提供 Caravan，也不修复传入 mode 1。

全新 mode 2 给 main 0 木筏精灵，将当前随从列表停放在 `(64,64)` 并安装站立动作，在那里声明独立 raft 31。其他模式仅在 F64 且已保存 RAFT_MAP 匹配时声明 raft 31。模式、已保存木筏坐标及成员资格仍为传入值；mode 1 不重放登乘。待处理为零可能允许其控制器，但不证明响应输入或向西旅行。

## 独立向西滚动保留当前队伍

**已确认：**Map68 warp row 4 匹配 X 0／任意 Y，请求向 Left 滚动进入 Devil's Tail 的 Map67，目的地 X 62、Y 255，朝向操作数为 Left。此前 row 3 匹配任意 X/Y 0 并向北通往 Map73，故在 `(0,0)` 优先。Rows 0–2 是更早的内部入口。只有这些行均未匹配才选择西行。

此使用方留在既有 ExplorationLoop 内。它使 main 0 空闲，写入 CURRENT_MAP=67，并调用 ProcessMapTransition 加载瓦片集／图块／布局，再用低方向位执行平面／精灵滚动。此处不选择特殊 Map71/F530 替换。转换后，main 0 的当前和目的地 X 变为 `62*384`；负 Y 255 保留 Y。使用方不应用传送行的朝向字节，因此 Left 不是无条件的结果朝向。

滚动不展开至 MainLoop，也不运行 SwitchMap/CheckBattle。不治疗、不减少踩踏计数器、不替换 CURRENT_BATTLE、不重建实体群、不清除临时标志，也不运行 Map67 init。行动者、逻辑映射、随从列表及模式保留。它还绕过普通 WARP_SFX 提交／清除。

DeclareRaftEntity 随后使用采样的 main 元组。Mode 2 立即返回；mode 1 跳过当前随从重定位；其他模式将每个既有随从列表条目移至采样坐标，不重建列表。非2模式下，F64 未设置则返回。已保存木筏地图匹配 67 时，在掩码后的已保存坐标声明 raft 31；不匹配则清除 lookup 63，将 raft 31 停放在原始 `$70007000`。这些调用既不写已保存 RAFT_MAP/X/Y，也不写 PLAYER_TYPE。即使无全新配置行，仅当前随从也会保留；新传入随从标志不会在滚动期间创建行动者。

因此传入 F421 不会立即引起 Battle21 扫描。另行普通返回／重载到 67 会扫描 row 21，即使 F521 已设置也可能接纳它；止于那里。F419 属于 Map68，不能代替 F421。若独立选择全新 Map67，其固定列表为空、init 仅返回；这些事实不把此次滚动变成全新入场，也不确立后续战斗内容。

## Oddler 的求救场景

**已确认：**步行的 `$1400` 布局分支以目的地加候选移动、除以 384 得到的字 X/Y 写入 event 6。其 `$1000` 传送分支独立；Caravan 控制器无对应区域生产方。区域使用方初始化 physical 0，将这些字提交至 RunMapSetupZoneEvent。包装器解析当前配置，扫描带 255 通配及 `$FD` 默认的有序行。回调后关闭立绘／文本、等待一次 VInt，并等待 main 0 的当前位置等于目的地。自然布局／生产方选择仍未知。

| Map67 区域选择 | 回调与有边界结果 |
| --- | --- |
| Row 0、`(57,25)` | Map67_ZoneEvent0 属于独立 F751/F13/Elric-HP 通道路径；排除。 |
| Rows 1–4、`(56,26)`、`(57,26)`、`(58,24)`、`(58,25)` | Map67_ZoneEvent1 在 F520 已设置时返回；F520 未设置时执行 cs_4FB64。它既不检查 F470，也不检查 F420。 |
| 默认、其他坐标 | Map67_DefaultZoneEvent 向 CheckRandomBattle 提供 BATTLE_DEVIL_TAIL (21)。 |

默认处理器的共享踩踏／随机／完成机制可能写入重载请求。它不是每次滚动或回调都立即扫描战斗。其战斗内容及通道继续被排除；无关请求接管时停止。

### 完整场景顺序

`cs_4FB64`（`0x4FB64`）有 39 个顶层记录。下文 B 为 FOLLOWER_B (31)，A 为 FOLLOWER_A (30)。脚本使用当前查找映射，不搜索已加入成员，也不验证角色配置。在未改变的全新 Map68 F65/F67/F68 示例中，B/A/Peter/Bowie 解析为 physical 3/1/2/0。实体生成后改变标志不替换查找映射。Oddler/Peter 对话姓名不修复缺失或冲突行动者。

| 记录 | 有序动作与对话 |
| --- | --- |
| 0–5 | 设置游标 2543；等待 B Init、A Init、Peter Init 及 B Jump；B single text 2543 `$09EF`、modifier 0，说“Wait!”（“等等！”）`{W1}`。 |
| 6–10 | 移动 B 至 `(60,27)` 并等待；csWait 5；安装 B 2xRightLeft，不等待空闲；csWait 120；摄像机移至 `(53,24)` 并等待滚动。 |
| 11–18 | 移动 Peter 至 `(59,27)` 并等待，面朝 Right；Peter single 2544 `$09F0`、modifier `$C0`，问 Oddler 怎么了 `{W1}`。移动 Bowie 至 `(60,26)` 并等待，面朝 Down；B 面朝 Up，single 2545 `$09F1`、modifier 0，称呼 `{LEADER}...{NAME;7}`，问他们是否听见 `{N}`/`{W1}`。Peter single 2546 `$09F2`、modifier `$C0`，问听见什么 `{W1}`。 |
| 19–24 | csWait 5；安装 Peter 2xRightLeft；csWait 120；Peter 面朝 Down，single 2547 `$09F3`、modifier `$C0`，说什么也没听见 `{W1}`。B 内联 moveDown 1、endActions，等待空闲。 |
| 25–29 | csWait 5；安装 B 2xRightLeft；csWait 120；B nextText 2548 `$09F4`、modifier 0，说有人呼救 `{W2}`；B single 2549 `$09F5`、modifier 0，指出在“over here”（“这边”）`{W1}`。 |
| 30–38 | 移动 B 至 `(57,25)` 并等待，放在 `(63,63)` Down；csWait 40；移动 Peter 至 `(57,25)` 并等待，放在 `(63,63)` Down；移动 Bowie 至 `(57,25)` 并等待，放在 `(63,63)` Down；请求 Map51 `(11,22)` Up；结束。 |

对话说明转述已接受文本注释；引述片段、ID、替换项和控制保留字面身份。坐标及等待值是源码操作数，不是观察位置、实测时长或渲染场景。在 `(63,63)` 放置保留映射／列表条目；它不是 HideEntity。场景不设置／清除 F520/F470/F420/F751/F13／随从标志或 PLAYER_TYPE，不创建 Elric，也不加入任何人。它没有防止再次 F520 未设置调用的局部完成标志。

### 动作与等待实际消费什么

Init 安装速度 32/32、零加速因子、禁用加速／减速、启用阻挡／碰撞／自动朝向、Up 方向、禁用浸没及大小 24；刷新并跳至空闲。它不分配全新行动者。Jump 禁用自动朝向，相对移动 `(0,-1)`、等待 5、移动 `(0,1)`、等待 5、移动 `(0,0)`，恢复自动朝向并跳至空闲。2xRightLeft 刷新 Right/Left/Right/Left，每次等待 30，随后跳至空闲。独立 csWait 120 是睡眠请求，不证明动作完成。

`csc15` 通过映射行动者安装动作／计时器，可选等待精确 eas_Idle 指针，无 HP／存在门槛。`csc29` setDest 同样无 HP 门槛；它写瓦片缩放目的地、绝对行程及有符号速度 32。选定操作数无高位不等待修饰符，故等待通过 VInts 轮询当前位置／目的地相等。这绕过步行地形／碰撞生产方。

`csc19` setPos 和 `csc23` facing 使用 AdjustScriptPointerByCharacterAliveStatus：HP 0 的 ally IDs 0–29 跳过这些命令，A/B IDs 30/31 则绕过该 HP 检查。放置更新当前与目的地坐标／朝向并刷新；不删除动作、列表条目或成员资格。因此 Peter/Bowie HP 0 可跳过放置／朝向，但动作、目的地与文本记录仍运行。

`csc2D` 将选定内联移动编译为相对目的地 opcode 4、等待目的地 opcode 1 及最终空闲跳转。entityActionsWait 清除 FLAGS_A 位 5/6 并等待空闲，无 HP 门槛。Opcode 4 将有符号瓦片位移加至当前位置；清除位 5 禁用其目的地冲突检查。它绕过步行布局检查，使用当前动作速度。实际完成仍需实体处理；调用方不验证存在。

GetEntityAddressFromCharacter 掩码逻辑 ID 并索引当前查找表，不检查成员资格或存在。ClearEntities 将未分配条目留为 0，故缺失 A/B/Peter/Elric 可能别名到 main 0，而非安全跳过。Lookup 255 在此也不是安全的缺失检查。未完成等待／服务之前的写入可保留：不提供事务或自动角色配置修复。这些是条件源码限制，不是观察到的失败场景，也不证明死亡行动者到达此场景。

`csc32` 禁用实体摄像机跟随，设置摄像机瓦片目的地并等待滚动。`csc24` 恢复映射摄像机实体，不验证 HP，也无自身滚动等待。csWait 用低字节睡眠；debug P2 Start 可设置 SKIP_CUTSCENE_TEXT 并绕过这些等待／文本。脚本进入清除跳过状态；结束在对话状态要求时有条件等待视野滚动，并清除摄像机滚动速度。

游标选择下个文本 ID；nextText 与 single 形式消费／递增它。Single 还关闭立绘／文本并睡眠 10；nextText 留待后续关闭。`$C0` 组合镜像／右侧立绘修饰位。立绘／台词查找使用映射地图精灵。`{N}`、`{W1}`、`{W2}` 及替换项保留字面源码控制身份，无观察到的节奏或服务完成。两段选定场景均无 YesNoPrompt 或选择结果。

## 两种池塘请求，准入先于 init

**已确认：**场景的 `csc07_warp` 写入 pending 1、no-scroll 0、Map51、X 11、Y 22 和 Up。它不立即移动、不结束脚本，也不写 WARP_SFX。脚本与区域包装器返回后，普通消费使用请求及传入声音状态；另一待处理写入可替换它。

独立地，Map67 warp row 3 在 `(57,25)` 经 WarpIfSetAtPoint 请求 Map51 `(10,24)` Up，包括 WARP_SFX。此前 `(26,22)`、`(51,22)`、`(57,23)` 及后续 X63 边界均不在此竞争。直接传送与被排除的 ZoneEvent0 共享坐标，但布局／生产方选择不同；两者不互相蕴含，也不蕴含通道打开或求救前提。

两个普通返回均到达 MainLoop 准入。传入 F420 在任一坐标均会于池塘实体生成／init 前选择 Battle20。没有 F470/F13/HP／成员资格前提。F520 已设置会在选择后清除 F420，但仍接纳 20。止于 BattleLoop／开场前；探索提示尚未运行。这些场景／出口不设置 F420，本指南也不调用后续接近内容来获取它。

## F520 选择实体生成；F470 独立选择提示

**已确认：**无获准战斗时，全新 Map51 探索应用相同治疗、计数器、临时标志、F80 及地图加载顺序，写入 CURRENT_MAP=51/CURRENT_BATTLE=255。默认 ms_map51（`0x5C27C`）与唯一 F520 替代 ms_map51_flag520（`0x5C294`）仅实体指针不同。其实际六槽位表共享实体事件、区域、描述、物品和 ms_map51_InitFunction 指针。替代文件名不确立独立 init；指针绑定不执行区域。

| 默认固定行／身份 | 初始瓦片及朝向 | 地图精灵／动作 |
| --- | --- | --- |
| 0 / Elric、ally 13 | `(6,6)`、Down | GetAllyMapsprite(13)；eas_WhirlsInWater |
| 1 / NPC128 | `(63,63)`、Up | HARPY 108；eas_Init |
| 2 / NPC129 | `(63,63)`、Up | SOULSOWER 75；eas_Init |
| 3 / NPC130 | `(63,63)`、Up | SOULSOWER 75；eas_Init |

默认 s1_entities（`0x5C2AC`）含这四行；替代 s1_entities_520（`0x5C2CE`）立即终止。Elric 使用 ally lookup 13，不是 NPC128。实体生成跳过查找已非零的己方行，不重复它。普通随从表无 Elric 行；仅 F67/F68 时，Peter/B 占 physical 1/2、默认 Elric 占 3、NPC 占 4–6；替代配置无这四个固定行动者。Main 0 单独分配。固定行动者不追加到全新随从列表，放置 Elric 既不设置加入 F13，也不设置出战成员资格。

即使 F65 已设置，Map51 仍使用普通随从表；它既非大地图，也非排除随从地图，因此仅 F65 不添加 Caravan。GetAllyMapsprite 消费职业／HP；战斗外 HP 0 选择蓝色火焰，不以成员资格作为通用门槛。WhirlsInWater 启用浸没及双倍动画速度，循环 Right/Up/Left/Down，每次刷新／等待 10，再循环。这是初始动作，不是观察到的溺水／救援动画。

Mode 2 保留全新木筏／停放随从尾部；mode 1 不被规范化或重新登乘。清除临时 256–383 保留传入 F420/F470/F520、F13 和随从标志。下列四种组合仅在 F420 尚未先接纳战斗时到达 init：

| F520 | F470 | 实体生成及 init 后果 |
| --- | --- | --- |
| 未设置 | 未设置 | 默认固定行；请求共享初始提示。 |
| 未设置 | 已设置 | 默认固定行；跳过提示。 |
| 已设置 | 未设置 | 空固定列表；仍请求相同提示，不修复固定 Elric。 |
| 已设置 | 已设置 | 空固定列表；跳过提示。 |

### 完整初始提示

ms_map51_InitFunction（`0x5C3CA`）在 F470 已设置时返回；未设置则运行 cs_5C3D8（`0x5C3D8`）后返回。它不检查 F520／存在，也不设置 F470/F420。跳过时保留传入 `(10,24)` 或 `(11,22)` main 元组，受共享配置约束。选定场景有 24 个顶层记录：

| 记录 | 有序动作与对话 |
| --- | --- |
| 0–6 | 游标 2550；等待 B Init 和 Peter Init；放置 Bowie `(11,22)`、B `(10,22)`、Peter `(9,22)`，均为 Up；fadeInB 调用 FadeInFromBlack。 |
| 7–12 | B 内联 moveUp 2/endActions，等待空闲；csWait 5；安装 B 2xRightLeft；csWait 120；B 面朝 Up；B single 2550 `$09F6`、modifier 0，指出方向 `{N}`/`{W1}`。 |
| 13–17 | Peter 内联 moveUp 2/endActions，等待空闲；Peter single 2551 `$09F7`、modifier 0，指向“over there”（“那边”）`{W1}`；摄像机目的地 `(2,2)`，等待滚动；csWait 50；Elric single 2552 `$09F8`、modifier `$C0`，呼喊“Help me!”（“救救我！”）`{W1}`。 |
| 18–23 | Peter nextText 2553 `$09F9`、modifier 0，指出一名 精灵男孩 困在池塘中 `{N}`/`{W2}`；Peter single 2554 `$09FA`、modifier 0，说他累了并提议把他拉出来 `{N}`/`{W1}`；摄像机跟随 Bowie；Peter 跟随 Bowie、relative 2；B 跟随 Peter、relative 2；结束。 |

在一致存活映射／HP 且动作完成时，放置／内联操作数在新跟随运行前将 Bowie 放在 `(11,22)`、B 放在 `(10,20)`、Peter 放在 `(9,20)`。这是条件推导，不是固定控制快照：跟随、跳过放置及部分执行可改变它。F520 已设置／F470 未设置时仍调用同一脚本，却无固定 Elric；其文本查找既不被抑制，也不被修复。

没有提示命令救援／加入 Elric、将其固定行移出池塘、显露停放 NPC、解锁 Battle20、设置 F470/F520/F751、改变模式或写入已保存木筏位置。F470 仍未设置时，重新进入可再次请求提示。求救不等于完成救援。

### 跟随使用实际的首字节 HP 检查

`csc2C_followEntity` 经存活辅助程序检查字编码随从 ID 的首字节。选定 IDs 7 和 31 的高字节均为 0，因此是 **Bowie 的 HP** 控制两条完整跟随命令，而非 Peter 或 B 的 HP。Bowie HP 0 则逐条跳过。否则命令映射随从／目标，从 table_FollowerPositions 取得 relative 2 为 `(-24,0)`，并调用 AddFollower。

AddFollower 安装带物理目标／偏移的 42 字节 eas_Follower1 副本，仅在列表中尚不存在时追加随从至 EXPLORATION_ENTITIES。它不写成员资格或随从标志。模板使用速度 32/32、零加速度、禁用加速／减速／可阻挡性、地图／实体不可碰撞位及跟随／等待目的地循环。

实际 esc03_follow 仍检查布局。当前轴最大间距不超过 `$1E0` 时，保留既有目的地。否则从目标目的地推导朝向相对偏移，尝试零横向回退（对这些命令是相同偏移），若布局仍阻挡则尝试目标目的地。行程／速度使用当前随从速度。地图命令不等待此新动作稳定。Relative 2 不是保证的世界坐标终点；缺失映射可将目标重定向至 main 0，无验证或剧情状态修复。

## 条件控制是停止边界

init 后，共享循环有条件提交音乐／淡入淡出。待处理为零才让 WaitForEvent 跟随 physical 0 并安装传入模式控制器：

| PLAYER_TYPE | 源码动作／操作码 | 实际运输处理 |
| --- | --- | --- |
| 0 | eas_ControlledCharacter / 2 | 步行 |
| 1 | eas_Raft / 7 | Caravan |
| 2 | eas_Caravan / 8 | 木筏 |

其他非0／非1值走最后分支，无有效性修复。尽管源码标签如此，数字使用方决定解释。两段场景均不改变该策略。待处理请求先于控制器安装；未完成的动作／文本／淡入淡出／摄像机服务不确立可用控制。止于池塘接近／出口警告回调或任何战斗开场之前。

## 证据追踪与审阅边界

**已确认溯源：**已接受研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。其[复现所有者](../../../research/elven-village-departure-through-harpy-pond-entry.md#reproduction-and-remaining-limits)保留精确选定源码路径、地址、程序及紧接的使用方连接。地图表路径始于 `data/maps/entries/`，位于固定的 `disasm/` 中。本综合不开展新源码调查、完整程序比较、H1／ROM 采集或组合运行时观察。

| 解释 | 合同与可执行证据 | 有意保留的限制 |
| --- | --- | --- |
| 有序出口、滚动与普通返回、准入 | [探索流程](../contracts/exploration-control-flow.md)、[入场路由](../contracts/map-entry-routing-state.md)、[地图探索](../contracts/map-exploration.md)；[请求写入](../../../../tests/fixtures/h2/map-event-request-state-static-v1.json) eventRequestState 及[消费](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) eventRequestConsumption；[战斗路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json) expected.facts | 共享辅助事实／聚合不是选定传送／战斗行的完整守卫。已接受研究负责完整表及实际使用方连接。 |
| Map67 回调门槛／默认 | [区域分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)、expected.zoneTargetPrograms programOrder 122–124 | 123 绑定 F520/read/branch/script/return，124 绑定共享随机辅助程序。122 是被排除的通道上下文；不推断激活。 |
| 两个完整场景程序及动作／文本 | [地图脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、expected.dialogueCommandFacts、transitionCommandFacts、scriptControlCommandFacts、entityPlacementCommandFacts、entityActionBridgeCommandFacts、entityGestureRelationshipMotionCommandFacts 和 mapCameraControlCommandFacts；[对话](../contracts/dialogue-system.md) | 公开摘要不含完整选定程序证明。已接受保留的 programCorpus.programs IDs cs_4FB64/cs_5C3D8 绑定全部 39/24 记录及三个内联移动；研究负责字面文本注释及外部动作。 |
| F520 实体生成、共享 F470 init 及当前跟随 | [配置](../contracts/map-setup-data.md)、[实体](../contracts/map-entity-data.md)、[地图探索](../contracts/map-exploration.md)；[配置测试夹具](../../../../tests/fixtures/h2/map-setup-static-v1.json)、[实体测试夹具](../../../../tests/fixtures/h2/map-entities-static-v1.json)、[init 测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json) | Init primarySourceBodies 64/74、routeJoins 101/102/120 和 pointerTableJoins 101/102/116 绑定共享 init／Map67 返回。实际四行／空行、首字节 HP 及异步跟随来自已接受选定研究，不来自聚合行动者总数或剧情姓名。 |

schema／使用方所有者为 [请求写入 schema](../../../../schemas/h2/map-event-request-state-static-fixture.schema.json)/[map_event_request_state.py](../../../../src/sf2tool/h2/map_event_request_state.py)、
[请求消费 schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)、
[战斗路由 schema](../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../../src/sf2tool/h2/battle_routing_data.py)、
[区域 schema](../../../../schemas/h2/map-events-zone-programs.schema.json)/[map_events_fixture.py](../../../../src/sf2tool/h2/map_events_fixture.py) 和 [map_events.py](../../../../src/sf2tool/h2/map_events.py)、
[引擎 schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)、
[配置 schema](../../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../../src/sf2tool/h2/map_setup.py)、
[实体 schema](../../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../../src/sf2tool/h2/map_entities.py)，以及
[init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../src/sf2tool/h2/map_init.py)。现有复现使用 `uv run sf2 h2` 搭配 `map-event-request-state`、`map-event-request-consumption`、`battle-routing-data`、`map-events`、`map-script-engine`、`map-setup`、`map-entities` 和 `map-init`，在同一进程加载被忽略的 worktree 配置。这些路线定位证据，不选择纯文字重新生成、运行时或一揽子套件。

审阅反例包括角落优先级、出口的 F419、滚动与普通返回中的 F421、init 前 F420、全部四种 F520/F470 组合、缺失／冲突映射、HP 0、mode 0/1/2、已保存木筏不匹配、debug 文本跳过及被替换待处理请求。它们约束静态结论，不是已执行场景。较窄共享合同保留运行时／服务未知项；已接受内容专属研究提供选定连接，不扩大这些合同。

保留 [PR866 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170376311)、[更新交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170461179)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/866#issuecomment-6170476818)，包括未改变的保留配对、原始／变基后 CI、完整连接及已修正检查。保留 [PR865 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/863#issuecomment-6170364766)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/865#issuecomment-6170393336)，以及链接的前序证据／失败。不替换或重放已完成结果、黄金夹具、测试夹具或私有载荷来刷新此文字。

**未知：**自然访问／地形／路线、传入标志／HP／映射／成员资格／模式／木筏、生产方选择、动作／文本／淡入淡出／摄像机／服务完成、表现、响应控制及持久性。这些按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 和 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md) 继续延后，不是自动运行时队列。此段既不确立组合运行时路线，也不完成战役／设计 epic。
