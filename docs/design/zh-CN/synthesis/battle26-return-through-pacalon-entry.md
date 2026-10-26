# Battle26 返回至有条件 Pacalon 入口

> 本文是[规范英文原文：Battle26 return through conditional Pacalon entry](../../synthesis/battle26-return-through-pacalon-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

Higins 请求去 Pacalon 帮忙，但招募不将队伍带到那里。**已确认：** 胜利先返回营救场景前准备的位置。新探索随后重建行动者，移除 Higins 临时跟随而保留其军团成员资格。选定东边界可滚动进入相邻大地图；进入 Pacalon 是另一请求，且可在城镇探索前准入 Battle31。这些是相连状态转换，不是一次未中断行走的证明。

本指南接续 [Battle26 结果与 Higins 招募](battle26-outcomes-and-higins-recruitment.md)，使用完整已接受[返回/入口研究](../../../research/battle26-return-through-pacalon-entry.md)及前序所有者。它是 [#1022](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1022)与 [Epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)下的英文 Layer B 合成。Standard 严谨级别适合独立返回、人口、滚动与准入生命周期。Pacalon 互动仍属独立已接受所有者。终点为有条件的首次 Map02 事件/控制读取，或 Battle31 执行前分流。不含城镇互动与城堡准入。续读 [Pacalon 对话、补给与城堡交接](pacalon-interactions-and-castle-handoff.md)，了解两种居民事件、服务、描述、可能拾取与离开传送消费；该指南止于目的地准入/初始化前。

**未知：** 自然抵达、实际所供状态、解码可行走路径、服务完成、呈现、可用控制与存档/读档持久性。不改变重制版验收或原版观察前沿。Fnnn 指游戏标志 nnn；地图与战斗编号属不同域。除指明内部字/字节操作外，下文坐标均为瓦片请求。源码路径与数值身份仍属研究所有者。

## 返回位置属于结果

**已确认：** 普通胜利将当前地图（名义72）准备到 PARAM2，将首个出战成员战斗员坐标加战斗裁剪准备到 PARAM3/4，将其映射实体朝向准备到 PARAM5。这发生在营救前。场景 Bowie `(19,16)`、摄像机 `(13,11)` 与 Higins 场景移动不替换种子。完成路径清 F426、设 F526；476 仍为独立战斗介绍标志。持有 F526 跳营救，但共享 JoinForce0 尾部仍运行。既不是再加入 Higins，也不修返回位置。

败北、获准 Egress 与 Angel Wing 改用所供存档点派生返回，各有金币、HP、MP、物品与筏写入。不一定返回72。F88 续接入口同样保留所供状态。[结果所有者](battle26-outcomes-and-higins-recruitment.md#完成撤退与返回交接)拥有这些替代路径；胜利场景不将它们规范化。

全部返回到 `alt_MainLoopEntry`，调用 SwitchMap 后调用 ExplorationLoop，不测试返回 d4 符号，也不在中间调 CheckBattle。因此仍存 Battle26 解锁不会在此返回边界立即重新准入该战斗。它不同于之后无滚动传送使用的普通 MainLoop 入口。

SwitchMap 扫描六字节记录中的字地图身份，返回首个匹配且标志已设的替换。完整表为 Map3/F640→4、Map71/F530→78、Map58/F999→59。无 Map72 或 Map02 替换，也无反向78→71 规则。它不先解析 MAP_CURRENT255。

## 新探索重建在场者

**已确认：** 对显式返回地图，ExplorationLoop 按此顺序运行：

1. 清待处理事件类型；字步数计数器减20000，有符号分支不成立时清零。名义30000 变10000；普通较小非负值变0。这是字/CCR 算术，不是无界整数钳位。清类型不清所有剩余参数字节。
2. 治疗存活/不死己方角色，请求全调色板淡出，设视野目标 byte-1，保存请求地图字。
3. 地图低字节不为255 时，设 CURRENT_MAP 与 CURRENT_BATTLE255，加载瓦片集，等淡入淡出并清 VInt。取得选定配置列表，初始化实体/地图精灵，清临时 F256–383 并设 F80。
4. 初始化探索精灵计数器，以所存地图字与 d0=-1 调 LoadMap，安装基础 VInt 并运行选定配置初始化。之后调色板状态有条件请求地图音乐与淡入。
5. 设置移动音效并进入 WaitForEvent。只有待处理事件为零才允许在 physical0 安装控制脚本；非零跳过安装。

治疗扫描全部30 个己方槽，包括未加入者。Peter7 与 Lemon28 始终符合；其他要求字 HP 非零。符合槽获得最大 HP/MP，只保留原有 STUN/POISON/CURSE 位，随后重算属性。存活 Higins19 无论 F19 如何都治疗；死亡19 保持死亡。因此之前逃脱 MP 消耗或败北恢复 Bowie HP 不一定描述之后探索值。成员资格本身不赋予不死性。

InitializeMapEntities 将所供主坐标**字**乘384，清行动者记录、映射与行走分配，构建标志随从，消费固定配置列表，再以所供朝向声明 physical0 Bowie。终止新随从列表。场景脚本与旧物理随从链被丢弃。F19/F51 与有关剧情/战斗标志通过临时标志清除而保留。

因此 Higins 可保持招募而不在 Bowie 身后行走。普通随从表与 Map72 空固定列表均无 Higins 行。Join19 持久成员资格与营救临时 AddFollower 是不同权威。缺失逻辑映射可解析为 physical0；这不是独立隐形 Higins 行动者。

Map72 是大地图，不在随从排除表中，有一个默认配置。F65 在普通随从前加 Caravan 前缀；固定列表仅 `msEntitiesEnd`。其 `$4FF2E` 初始化仅在 F716 已设且加入 F10 未设时运行已有所有者的 [Luke 招募](../../../research/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)。不测试 F526 或 Battle26 完成前提。因此变化历史可在考虑控制前改变状态/坐标；本指南不复述该场景。

BattleLoop 清 PLAYER_TYPE，普通 Battle26 结果不再改它，因此选定返回为 numeric mode0。所供其他模式仍独立。CURRENT255 探索入口仍清待处理、调整计数器、治疗/淡出、更新主角/筏、加载并运行初始化，但不重建行动者、不替换 CURRENT_BATTLE、不清临时标志。负坐标低字节跳过主坐标更新。直接脚本 LoadMap 与滚动又有不同生命周期。

## 东边界请求滚动

**已确认：** Map72 传送行8 位于 `$C1C3E`，匹配 `(63,255)`；负源码字节指任意 Y。记录 scroll RIGHT、目的地71 `(1,255)` 与 facing RIGHT0。八字节行编码 transition `$10`、map71、X1/Y$FF、facing0 与 padding0；宏用 `$10+direction` 表示滚动，0 表示无滚动。无426/476/526、Higins 成员资格、HP 或队伍人数守卫。更早匹配行仍优先。这不指引队伍向东，也不证明抵达边缘。

普通行走先读实时布局，要求掩码 kind `$1000` 才调用 WarpIfSetAtPoint。该生产者将内部坐标除384，按序扫描，先检查字 `$FFFF` 终止符，将负源码字节当作轴通配符。写 type1、PARAM1–4 长字、PARAM5/padding 字以及 WARP_SFX。通配符不作碰撞或边界检查。移动有自身阻挡/模式/标志门槛；kind `$1400` 改为请求 zone 事件。已提交请求与已完成行走仍不同。

ProcessMapEvent 在分发所存事件值前清 type。PARAM1 非零的传送分支让主角 Idle 并读目的地字节。71 时显式检查 F530，已设则替换78，在过渡服务前写 CURRENT_MAP。这是独立于 SwitchMap 通用表的代码分支；之后改 F530 不会追溯选择另一已加载地图。

ProcessMapTransition 加载选定地图瓦片/图块/布局，按 PARAM1 &3 分发。RIGHT 调整平面计数器与精灵投影，排队 DMA 并等 VInt。不经 MainLoop 返回或构建新人口。目的地字节有刻意不同宽度规则：

| 操作数 | 实际滚动写入与限制 |
| --- | --- |
| Y$FF | 清 d0.w 后读 Y。负字节保留实时 Y/YDEST。 |
| X1 | 过渡后非负字节仅替换 d0 低字节；`mulu.w` 消费 `((d0.w & $FF00) \| PARAM3)`。存储坐标是乘384 后乘积低字。记录 X1 不是无条件零扩展的世界坐标字。 |
| PARAM5 facing0 | 滚动分支从不复制到实体朝向。普通朝向来自移动主角；注入冲突朝向不会应用。 |

它通过有符号除384 采样实际主角 X/Y 与当前朝向，再调 DeclareRaftEntity。该调用保留脚本/映射，但可移动行动者：

| 数值模式 / 筏状态 | 已到达行为 |
| --- | --- |
|2, PLAYERTYPE_RAFT|立即返回；不重定位随从或修复筏。|
|1, PLAYERTYPE_CARAVAN|跳随从重定位，再用筏门槛。|
|其他模式|遍历既有 EXPLORATION_ENTITIES，将所列物理行动者位置/目的地设到采样主角瓦片，再用筏门槛。|
|64 未设|返回，不修筏。|
|64 已设，RAFT_MAP 匹配|由掩码筏 X/Y 字节声明 physical31，Left、筏精灵/站立脚本、lookup offset63。|
|64 已设，RAFT_MAP 不同|清该查找并将筏移出地图。|

物理筏31 不同于逻辑 FOLLOWER_B31/lookup31。重复或变化物理 ID 按所供值消费。不重建基于标志的随从列表；此处新设随从标志不分配行动者。普通随从位置/目的地刷新时，行走临时区、已安装脚本与映射身份保持活动。

滚动路径无治疗、计数器相减、临时清除、CURRENT_BATTLE 重置、配置/初始化或战斗扫描。也省略无滚动分支的 WARP_SFX 提交/清除。返回后既有探索循环设移动音效并重入 WaitForEvent；待处理为零重装模式所选控制脚本，新事件则绕过安装。中断 DMA、淡入淡出、行动或 VInt 服务不回滚已到达写入。

## 已加载大地图不等于已解码路径

**已确认：** Map71 与 Map78 都无地图配置条目。GetCurrentMapSetup 到终止符返回 `ms_Void/$FFFF`；实体列表消费者返回该哨兵值，初始化分发跳调用。这不是有人口默认配置或默认随机遭遇。假设新的显式71/78 入口仍构建共享随从/主角/筏，但无固定 NPC 列表。选定滚动不调这些配置消费者，保留 Map72 实时阵容。

两图分别选择 Map71s0_Blocks/Map71s1_Layout 与 Map78s0_Blocks/Map78s1_Layout。两者头部均用 palette0/tilesets101–105；区域指定 `(0,0)..(63,63)`、其他原点 `(0,0)`、两组视差256/256、零自动滚动、type254 与 MUSIC_MAIN_THEME。标志表与踩踏表均立即终止。即使这些区域属性和城镇传送一致，不同布局仍是不同碰撞输入。

过渡读取/缩放首区域矩形，但不调 LoadMapArea 刷新区域/音乐/动画元数据。Map72 选定区域具有相同所述矩形/层属性；变化后保留状态仍是前提。新布局不蕴含新区域/摄像机初始化。LoadMapBlocksAndLayout 也服从实时 CURRENT_BATTLE：普通255 避免战斗裁剪复制，所供非255 则可在滚动时应用其裁剪。

71 与78 均未出现在 BattleMapCoordinates。缺失配置使 zone 分发在通用 VInt/主角停止等待前跳过回调选择。不发明随机战斗默认，也不排除所供事件或之后转移别处。离开前 Map72 保留自身已接受 zone/计数器规则：F526 未设时 Battle26 辅助立即请求，持有 F526 时使用计数器/range8 重试；普通扫描在 F408/424/426 下按8/24/26 排序。边界滚动不作这些扫描。

实际行走继续消费布局位、行动者目的地、模式与待处理状态。未建立从战斗种子到边缘，或边缘到 Pacalon 的解码路径。已接受研究中可选源码侧压缩布局读取不可用；选定资源/消费者证据不闭合单元格可行走性、遭遇节奏或自然抵达。

## Pacalon 门口早于独立战斗决定

**已确认：** 两大地图具有相同的首两条无滚动行：

| 源地图 / 行地址 | 源瓦片 | 请求入口 |
| --- | --- | --- |
|71 / `$C0694`, `$C069C`|`(21,15)`, `(22,15)`|Map02 `(21,30)`，Up1|
|78 / `$C69E4`, `$C69EC`|`(21,15)`, `(22,15)`|Map02 `(21,30)`，Up1|

无 F530/526 或成员资格门槛，使用同一布局 kind 生产者与可变单事件槽。无滚动消费者提交/清 WARP_SFX，让主角 Idle 并退出探索。清 d0–d4 **字**，读 map2，采样主角坐标，将非负目的地**低字节**替换为21/30，读 facing1 并返回 d4=0。替换不抹去采样坐标高字节；`(21,30)` 是普通名义结果。变化哨兵值、高字节、服务或事件状态按实际操作处理。

此返回进入普通 MainLoop `@Start`：SwitchMap，然后 CheckBattle。Map2 不切换。有序45 行扫描仅有一个 Map02 候选，行31：crop `(0,0,48,32)`、trigger `(255,255)` 与 unlock431。其他地图行跳过；crop 是输出数据，不是包含性测试。通配触发不要求名义21/30 位置。

| 扫描时状态 | 选定后续 |
| --- | --- |
|431 未设，531 任意|无候选，d7=-1；进入 Map02 探索。|
|431 已设，531 未设|选31，写裁剪并调 BattleLoop。|
|431 已设，531 已设|选31，写裁剪，清431，且**本次调用仍准入31**。|

F530 选择内容，不是此战斗门槛。不测试 F526 与 Higins F19/F51；Battle26 胜利不新设 F431 或530。所供 CURRENT255 会为 CheckBattle 解析，但完整城镇行请求显式2。Battle31 分支止于初始化/执行前。即便持有 F531 也不足以承诺无条件城镇探索。

## 城镇人口是有条件内容

**已确认：** 只有无战斗分支开始新的显式 Map02 探索。重复治疗/重置/重建序列，设当前地图2、当前战斗255，再于 F530 未设时选 `$5E86C` 的 `ms_map2`，F530 已设时选 `$5E884` 的 `ms_map2_flag530`。两组六指针表选择实体、实体事件、zone、描述、物品事件与初始化。人口、实体事件和描述指针不同；zone/物品/初始化指针共享。选互动指针不是执行它。

Map02 不是大地图，也不排除随从，因此65 不加 Caravan 前缀。普通随从行仍是独立标志，完整顺序如下：

`66/Sarah, 67/Peter, 72/Kazin, 73/Slade, 74/B/Astral, 66/Chester,
68/B/Oddler, 75/Luke, 69/C/Fairy, 70/B/Astral, 74/A/King, 71/Lemon,
74/D/Minister, 76/Zynk, 77/A/Old Man, 82/Frayja, 83/A/King,
83/D/Minister, 84/Rohde`。

A/B/C/D 是逻辑30/31/158/159 与查找偏移30/31/62/63。每启用行从1 分配下一物理行动者，写查找，安装下一随从脚本并追加物理 ID。成员资格/HP 不门控这些行。重复标志/别名可分配多个行动者，之后覆盖查找却不删除早先行动者。无 Higins19 行。新清除移去陈旧映射；CURRENT 重载与之后变化状态不继承此保证。

实际职业与战外 HP 选择己方精灵：HP 零变蓝焰；存活 Rohde11 未加入 F11 时用 NPC 精灵；否则依职业/表规则。下列固定 NPC 行用普通精灵30–239，无己方 HP/职业或特殊精灵/去重分支。设实际普通随从数为 n，行 j 声明物理 `n+1+j`、逻辑 `128+j`、查找 `32+j`；主角保持0。这些成功声明公式不保证容量安全：随从/脚本存储有限，初始化不为任意大量标志启用作边界检查。

完整 F530 未设列表为 `$5E89C..$5E8EE`，十条八字节行加哨兵值：

| j / 逻辑 | X,Y | 朝向 | 精灵 | 脚本或行走原点/范围 |
| --- | --- | --- | --- | --- |
|0 /128|21,22|Down3|CENTAUR_MALE233|21,22/range1|
|1 /129|34,26|Down3|CENTAUR_MALE233|eas_Init|
|2 /130|8,21|Right0|CENTAUR_FEMALE234|eas_Init|
|3 /131|23,4|Up1|CENTAUR_MALE233|eas_Init|
|4 /132|16,23|Down3|CENTAUR_FEMALE234|eas_Init|
|5 /133|34,7|Down3|CENTAUR_FEMALE234|34,7/range1|
|6 /134|31,15|Down3|CENTAUR_MALE233|31,15/range1|
|7 /135|9,8|Down3|MERCHANT193|eas_Init|
|8 /136|15,5|Down3|MERCHANT193|eas_Init|
|9 /137|41,8|Down3|PRIEST192|eas_Init|

完整 F530 已设列表为 `$5E8EE..$5E938`，九条八字节行加哨兵值：

| j / 逻辑 | X,Y | 朝向 | 精灵 | 脚本或行走原点/范围 |
| --- | --- | --- | --- | --- |
|0 /128|22,25|Down3|CENTAUR_MALE233|22,25/range1|
|1 /129|41,26|Up1|CENTAUR_MALE233|eas_Init|
|2 /130|30,17|Right0|CENTAUR_MALE233|eas_Init|
|3 /131|8,21|Right0|CENTAUR_FEMALE234|eas_Init|
|4 /132|34,7|Down3|CENTAUR_FEMALE234|34,7/range1|
|5 /133|21,7|Down3|CENTAUR_MALE233|eas_Init|
|6 /134|9,8|Down3|MERCHANT193|eas_Init|
|7 /135|15,5|Down3|MERCHANT193|eas_Init|
|8 /136|41,8|Down3|PRIEST192|eas_Init|

精灵名省 MAPSPRITE_。仅67/70/84 启用时，Peter/Astral/Rohde 占1/2/3，NPC 接着占4–13 或4–12。无随从时 NPC 占1–10 或1–9。商人/祭司在两列表间逻辑索引改变，物理索引随随从改变；可见角色不是稳定物理身份。

声明写位置/目的地，清运动，存朝向、精灵与脚本。行走宏在行动长字编码 `$FF,originX,originY,range`；SetWalkingActscript 复制50 字节并修补原点/范围字。F530 未设三个行走者用150 字节；F530 已设两个用100。随机步消费者受布局、行动者目的地与斜坡约束，不是已证明路径或不变矩形钳位。eas_Init 在 Idle 前建立运动/碰撞与 Up orientation；orientation 不同于声明 facing。VInt 行动执行可在之后事件读取前推进行动者，因此表格不是观察到的冻结城镇画面。

主角为 Bowie physical0，使用职业/HP 派生精灵、eas_Idle 与**返回的**坐标字/朝向，名义 `(21,30)`、Up。不是战斗种子首个出战成员的城镇化身。新筏处理先测 numeric2，将主角设为筏，使随从站立/隐藏，并在 `(64,64)` 声明独立筏31；其他模式用64/RAFT_MAP。进城既不登车，也不证明教堂访问。

## 空初始化止于事件读取

**已确认：** 完整 LoadMap 选择 Map02 主区域 `(0,0)..(47,31)`、次前景原点 `(0,32)`、背景 `(0,0)`、视差256、零自动滚动、type0 与 MUSIC_TOWN。20 个有序 F530 控制的1×1 布局复制用源 `(49,2)` 与水坑目的地。它们变更布局，不产生 F530 或选择人口。共享 `ms_map2_InitFunction` 完整体位于 `$5EAA6`，为 `rts`。不请求玩家移动。列表/初始化分别选择；两次读取间改 F530 不重建选定列表。

初始化后，调色板比较有条件请求音乐/淡入淡出。`$2591C` 的 WaitForEvent 随后读待处理 MAP_EVENT_TYPE。零则设 view target0 并调 SetControlledEntityActScript0：实际 numeric0 装 eas_ControlledCharacter，numeric1 装 eas_Raft，其他非零装 eas_Caravan。这些是消费者选择，即使先前 mode1/2 枚举分别叫 Caravan/Raft。待处理非零跳安装。循环先读事件类型，否则将 CURRENT_PLAYER_INPUT 掩码为 A/C。

**这就是终点：** 有条件首次事件/控制读取，不是已观察可用城镇控制。不建立对话、商店/教堂结果、物品、zone 互动、城堡准入或 Battle31 执行。

## 变化状态与保留证据

| 变化前提或中断 | 必需解读 |
| --- | --- |
|结果、首出战成员、裁剪、朝向或返回地图不同|用实际场景前或 egress 种子；不自动通往 Pacalon。|
|426/476/526 或早期 Map72 解锁不同|区分完成、介绍与准入；战斗返回跳立即 CheckBattle。|
|716 已设/10 未设|既有 Luke 初始化可改初始化后状态/位置。|
|显式地图、CURRENT 或滚动|重建、临时重置、治疗/初始化与保留行动者生命周期不同。|
|530 在读取间改变|SwitchMap、显式滚动替换、城镇人口与布局写入是独立消费者。|
|负坐标字节、高字节或服务后寄存器不同|在各自边界应用通配、保留、低字节替换与字乘法。|
|布局 kind、碰撞、模式、待处理替换或服务进度不同|表行或已提交请求不是已完成旅行/控制；已到达写入保留。|
|随从、别名、职业、HP 或存储不同|由标志分配，保留实际映射与精灵规则；不假定容量修复或持久 Higins 跟随。|
|配置缺失或所供 CURRENT_BATTLE/区域不同|不发明默认人口/遭遇；滚动保留元数据，可用所供战斗裁剪。|
|431 已设，531 任意|准入31；持有531 只在选择后清解锁。|
|64/65、筏地图或数值模式不同|前缀、筏行动者、随从重定位与已装控制脚本是独立决定。|
|首次城镇读取时待处理非零|跳控制安装并返回事件，不承诺可用控制。|

路线图要求的对抗审阅消费完整已接受 #1017、先前 Battle26 结果/入口所有者、[Luke 初始化](../../../research/polca-departure-through-luke-recruitment.md)、[成员/随从生命周期](../../../research/ribble-may-through-battle10-admission.md)、[共享地图](../../../research/common-maps.md)、[共享脚本](../../../research/common-scripting.md)、[请求状态](../../../research/map-event-request-state.md)、[请求消费](../../../research/map-event-request-consumption.md)，以及[地图/探索](../contracts/map-exploration.md)与[队伍/名册](../contracts/party-roster-state.md)合同。在以下边界检查选定公共载荷、schema 与实际消费者：

| 测试夹具 / 实际消费者 | 使用证明与更严格限制 |
| --- | --- |
|`sf2-common-maps-static-v1`；[maps.py](../../../../src/sf2tool/h2/maps.py)|`_map_facts` 守卫切换/CheckBattle/加载顺序。通用已完成匹配清解锁不抑制同次准入；完整加载区域顺序不整体适用于滚动。选定行/资源仍是已接受 #1017 连接。|
|`sf2-map-init-static-v1`；[map_init.py](../../../../src/sf2tool/h2/map_init.py)|`_route_joins` 与完整测试夹具比较保留 Map02 路由117/118、指针连接2/3、共享单 rts 体及完整六操作 Map72 初始化谓词。这些公共连接不替代新私有源码/配对比较。|
|`sf2-battle-loop-static-v1`；[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py)|`lifecycleFacts.betweenBattleHealing` 保留 Peter/Lemon、跳死亡槽、最大 HP/MP、mask7 与属性更新。实际选定入口/顺序与非零字 HP 谓词仍由已接受研究拥有。|
|`sf2-map-event-request-state-static-v1`；[map_event_request_state.py](../../../../src/sf2tool/h2/map_event_request_state.py)|调用方局部写入/到达定义止于其交接。通用39 程序计数不证明选定自然路径或已完成消费者。|
|`sf2-map-event-request-consumption-static-v1`；[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)|实际 ExplorationLoop/WaitForEvent/ProcessMapEvent/筏上下文访问与 `_controls` 保留重置/读取/清除/分支宽度和顺序。仅有界形状不推出被调用方算法、实时状态或呈现结果。|
|`sf2-map-script-engine-static-v1`；[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)|生命周期与随从形式/处理器/源码连接区分直接加载与临时跟随。不提供私有 Map02/72 程序配对、新探索观察或随从容量安全。|
|`sf2-map-lifecycle-runtime-v1`；[map_lifecycle.py](../../../../src/sf2tool/h3/map_lifecycle.py)|五个受控处理器用例与首/末布局字标记是有界证据，不是全部布局单元格、边界滚动、Pacalon 抵达或淡入淡出完成。|
|`sf2-force-state-active-party-runtime-v1`；[force_state_active_party.py](../../../../src/sf2tool/h3/force_state_active_party.py)|九处理器用例保留列表滞后与空/已有/重复随从行为。不证明自然 Higins 跟随、新城镇人口或持久性。|

所属复现路由为 `uv run sf2 h2 common-maps`、`map-init`、`battle-loop`、`map-event-request-state`、`map-event-request-consumption`、`map-script-engine`，以及有界 `uv run sf2 h3 map-lifecycle` / `force-state-active-party`。这些已检查，未作为私有源码/运行时比较重跑。更广问题队列不抹去已接受选定源码事实；选定事实不闭合其自然路径/表现问题。未发现阻塞分歧。未来矛盾交研究所有者，不作 Layer B 修补。

[#1017 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1017#issuecomment-6182642561)与 [PR1020 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1020#issuecomment-6182664667)保留固定源码/测试夹具/请求/列表审阅、最终 head `bbcf476f`、tree `e84ff8c6` / base `89233108`、作者0.071s 与独立0.062s 检查、原0.065s/修正0.064s 回执及精确提交头 CI38668375749（scope15s/research25s）。原 CI38668208926 及变基前 head/tree/base 保持独立。未变18,285,979 字节配对的已接受0.083s 回执按溯源复用，不在此再生成或新比较。可选压缩布局数据缺失、已重命名 inspect.py 遮蔽、字面量640 解析修正、猜测文件名、字面量通配错误与截断检查，连同所链前序结果仍在交接。

[#1018 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1018#issuecomment-6182597910)与 [PR1019 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1019#issuecomment-6182612250)保留设计环境/源码/配对限制、九 schema/对话/表/链接检查、精确提交头 CI38668093688、修正 common-stats 字段查找与全部所链已完成诊断/失败。此处本地私有源码与保留配对仍不可用。不采集/复制，不声称新私有比较或运行时结果；不重置已完成套件。

此原切片 Issue 交接记录完整文字/变化状态/五路径/diff、NPC 表与公共测试夹具/schema 检查、导航/链接/锚点、UTF-8/私有边界、git diff --check、已配置干净提交头 planner 与实际精确提交头 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4 与翻译套件。**未知：** 全部实际结果/种子/标志/HP/映射/容量、数值模式、运动、过渡后寄存器、事件替换、解码路径、服务完成、呈现、输入/控制与持久性前提。不授权运行时队列，也不扩展重制版前沿。
