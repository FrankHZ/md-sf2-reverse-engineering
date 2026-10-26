# Cotton Balloon 获取与前往 Floor World

> 本文是[规范英文原文](../../synthesis/cotton-balloon-through-floor-world-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**Desktop 宝箱在物品栏交付之前打开，Guard136 检查该宝箱标志，而非当前物品持有。
在玩家自身 `(35,24)` 瓦片使用选定 Cotton Balloon，会分发独立的物品栏搜索、条件移除及完整离开场景。
存续传送有条件地准入 Map26 探索，位置 `(26,8)`、Down。
**推断：**六个行动者描绘 Cotton Balloon 下降。
**未知：**自然到达、实际选择/持有/状态、移动/服务完成、表现、可用控制及持久性。

本指南接续[棋盘结果与 Desktop 返回](battle22-outcomes-and-desktop-return.md)，
使用[已接受 Cotton Balloon 研究](../../../research/cotton-balloon-through-floor-world-entry.md)，依据
[Issue #919](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/919) 和
[当前 Standard 约定](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
止于条件目的地探索加载器准入，在 Floor World 实体群/init、内部内容或 Battle23 开场之前。
Fnnn 表示标志 nnn；命令索引从零开始。坐标、速度和等待为源码操作数，
不是测得移动、帧时长或已证明可行走的行程。

## 找到礼物不等于收到礼物

**已确认：**F522 选择战后 Desktop 实体群及两项布局复制：`(25,10)` 的 chest-for-chessboard 变化，
以及 `(25,40)` 的 blank-for-blank。完整地图加载在宝箱覆盖前应用这些变化。
宝箱记录为 `(25,10)`、F212、COTTON_BALLOON125（`$7D`）；非宝箱物品表为空。
Ponpei 公告不授予物品或设置 F212。这些行本身既不证明宝箱可见，也不证明无碰撞路线。

普通搜索检查被观察玩家**前方**瓦片。A 的 FieldMenu SEARCH 调用 CheckArea；
C 仅在其调试、Caravan 和已激活实体替代路径未优先时才调用。
玩家先空闲，并等待目的地/滚动服务。负视野目标拒绝搜索；其他传入目标需要按实际索引处理。
宝箱分支需要宝箱布局字和匹配坐标记录。普通探索使用 CURRENT_BATTLE=255，不作战斗原点调整。

对此匹配宝箱，OpenChest 检查 F212，然后在发现物品文本或 AddItem 调用**之前**设置它并写打开图块 `$D802`。
F212 已设置则返回 NOTHING，随后搜索称宝箱为空。没有匹配宝箱记录则不作这些写入。

交付分支先尝试 Bowie 的四个槽位。若 Bowie 已满，它按 ID 0–29 升序构建已加入成员，
且在至少两个已加入成员时，从第二个已加入 ID 起扫描首个持有物品少于四个的成员。
这是 OTHER_FORCE_MEMBERS_LIST，不是仅后备列表；HP 和战斗出战成员资格均不门控扫描。
通常 Bowie 是被跳过的首 ID。传入状态若无已加入 Bowie，仍先尝试他，再跳过实际首个已加入 ID。
少于两个已加入成员时，跳过其他成员循环。一致物品栏紧凑排列；持有计数覆盖全部四槽。

AddItem 写首个经掩码后的 NOTHING 槽位并返回成功 0；无空间则不写、返回失败 1。
调用方依赖先前槽位计数，不测试该返回值。因此成功交付物品栏指实际物品写入，
不只是宝箱已打开、接收者已选或出现收取台词。选定分支请求物品音乐、收取/转交文本及输入等待。
[字面文本/控制符表](../../../research/cotton-balloon-through-floor-world-entry.md#chest-appearance-search-and-inventory-delivery)
保留发现/收到/转交/空/满消息，包括原版拼写。

若未找到接收者，双手已满文本先于 CloseChest；后者清 F212 并还原 `$D801`。
RefillNonChestItem 没有 Map22 记录可清。正常完成允许重试；关闭前中断可能留下宝箱标志已设置、却没有物品。
反之，物品写入后中断可保留持有，而表现尚未完成。重复搜索已打开宝箱不复制物品。
这些服务之间没有通用回滚。

## 出口守卫读取宝箱状态

**已确认：**两种 Map22 实体群的 Guard136 都使用共享实体事件回调。
其行动位使其转向玩家，并在返回时还原保存朝向；事件行的 DOWN 不是接近朝向限制。
普通 Peter/Oddler 随从示例将其映射为 physical 13、GUARD206，位置 `(33,24)`、Left，
无立绘、台词声音 bleep 5。其他分配需要实际 logical-to-physical 查找，不能套用该 physical 示例。

| 传入标志 | 服务返回时的回调后果 |
| --- | --- |
| F212 未设置 | King's-permission 拒绝 1802；无移动或 F258 写入。 |
| F212 已设置，F258 未设置 | 警告 1803；运行 cs_599A4；仅场景返回后设置 F258。 |
| F212 已设置，F258 已设置 | 再次警告 1803；不再移动。 |

cs_599A4 有三个命令：等待 Guard136 的 `moveUp 1; endActions`，将其朝向设 Down，然后结束。
相对行动从实际当前位置请求向上一瓦片；未改变且一致的 `(33,24)` 请求 `(33,23)`。
等待仅轮询该行动者 idle 指针。Trap6 在脚本期间激活实体更新，但静态激活不确立移动完成。
回调后，包装器还原保存朝向，通常为全新 Left，取代场景 Down；只有其尾部返回时才完成关窗口及重新激活更新。

F258 为临时标志。显式全新探索加载清标志 256–383 并重建守卫；F212 仍已设置时可再次移动。
工作布局脚本重载不重置 F258。移动中断可留下位置改变而 F258 未设置；
外部清 F258 而不重建可允许再次相对向上请求。F212 已设置而当前无 balloon 仍可使守卫让路；
持有 balloon 但 F212 未设置则不能。该回调从不测试 F777、F522、F778 或物品栏。
其许可文本不确立先前觐见或 King's-permission 标志。

## 使用 Balloon 匹配玩家自身瓦片

**已确认：**FieldMenu ITEM > USE 从已加入成员选择持有物品。
其物品摘要模式为 GIVE，允许在普通场地使用白名单检查之前选择。无物品成员被拒绝。
取消成员画面返回物品子菜单；取消图标选择返回成员选择。两种取消均不分发此事件。
实际成功选择仍为调用方前提，不是事件处理器可导出的结论。

Cotton Balloon 既不是 Angel Wing，也不是普通白名单场地使用物品。
它经地图物品分发，不询问目标成员。调用方读取 physical 0 当前 X/Y 除以 384，并将朝向掩码至 0–3。
选定持有者的位置、NPC 瓦片及前方搜索瓦片均不提供该匹配。完整表有一个显式行和一个默认行：

| 匹配 | 结果 |
| --- | --- |
| 玩家 X35/Y24，任意朝向，掩码后物品 125 | Map22_ItemEvent0。 |
| 默认 `$FD` 行 | 仅返回，d6=0。 |

地图配置包装器清 d6，并在依次匹配 X、Y、朝向和物品前以 `$7F` 掩码物品索引。
NO_DIRECTION=`$FF` 接受全部四种朝向；仍要求精确坐标。
EVENT_RELATIVE_POSITION 接收 Y 字节，不是另一个朝向或目标谓词。没有选定配置也返回 0。

### 处理器在场景前再次搜索

**已确认：**Map22_ItemEvent0 为 125 调用 GetItemInventoryLocation，将返回 d0 与 -1 比较，
存在时移除找到的槽位，然后无条件运行 cs_599B2。仅场景返回后设置 d6=-1。
它不按身份移除菜单所选持有者/槽位，也不测试移除的 d2 结果。

精确已接受别名为 `$81D0` → GetItemInventoryLocation `$9146` 和 `$819C` → RemoveItemBySlot `$8E76`，
其选定程序体在 `code/common/stats/itemstats.asm`。
GetItemInventoryLocation 重建升序已加入 0–29 ID，包括死亡和后备成员。
对每个持有 n 件物品的成员，从槽位 n-1 向下检查至 0，比较掩码后索引。
一致紧凑物品栏下，**首个已加入持有者中最高的匹配占用槽位**胜出，即使菜单选择另一持有者/槽位。
其他副本保留。排除 Caravan 仓储；物品栏已满不阻止移除。

无匹配返回 d0=d1=-1。传入物品栏若有空洞，n-1 以上的物品可能被隐藏；
零成员循环在首次迭代前缺少提前守卫。这些限制不是声称畸形物品栏或空军团自然可达。
RemoveItemBySlot 将更高槽位左移，以 NOTHING 填末槽，返回 0；寻址到 NOTHING 返回 3。
两个尾部均更新属性值。普通匹配搜索消费一件物品，不使用丢弃行动的诅咒物品门控。

直接调用处理器即使未找到物品仍启动场景；普通菜单使用则先要求选中持有物品。
单独选定 d4 或最终已处理状态均不证明持有。处理器不检查 212/258/522/776/777/778 中任一标志，
移除时也不清它们。移除先于场景，因此后续场景/包装器失败不自动还原物品。

FieldMenu 在 d6 非零时退出。默认 0 产生已使用物品文本 73，再显示“But nothing happened”（422）并返回物品子菜单。
匹配场景返回的 -1 跳过这些台词。其含义是已处理，不证明消费、下降完成或目的地准入。
离开脚本本身没有对话。

## 六个独立行动者完成离开编排

**已确认：**两个配置表共享此物品事件及脚本。全新加载重建实体/查找、分配标志选定随从、消费实体群行并声明 Bowie0。
选定场景指向 Bowie0、Peter7、B31 和 NPC137/138/139。Guard136 独立。
仅 F67/Peter 和 F68/Oddler、没有跳过己方声明或木筏覆盖时，physical 示例为 0/1/2 和 14/15/16；
这些数字不是可移植的行动者身份。

| 场景 logical 行动者 | F522 未设置实体群 | F522 已设置实体群 |
| --- | --- | --- |
| Bowie0 | 主己方角色 | 主己方角色 |
| Peter7 | F67 随从 | F67 随从 |
| B31 | F68 Oddler 示例 | F68 Oddler 示例 |
| NPC137 | KING153，`(24,5)`，Down | OBJECT1=173，`(63,63)`，Up |
| NPC138 | QUEEN135，`(26,5)`，Down | OBJECT1=173，`(63,63)`，Up |
| NPC139 | BISHOP144，`(22,5)`，Down | OBJECT1=173，`(63,63)`，Up |

替代对象是已分配的固定精灵行动者，不是新生成或附着到己方角色的物品。
场景从不替换其地图精灵。基线加载则移动这些 ID 上的 King/Queen/Bishop，不是旧 ORB153–155。
分配后改变 F522 影响后续配置选择，不改变活动精灵。
[精确实体群/随从所有者](../../../research/cotton-balloon-through-floor-world-entry.md#all-actors-and-the-complete-departure-program)
保留三十/十四行及分配前提。

Map22 不是大地图，因此 F65 不前置 Caravan/A。Peter 需要 F67；
B 别名依 F74/Astral、F68/Oddler、F70/Astral 准入顺序形成，后续查找写入可替换更早 B 映射，同时旧行动者仍存在。
其他随从会移动 NPC physical 槽位。缺失零查找可解析至 physical 0；多个命令随后可能作用于同一行动者。
HP/职业决定己方精灵，包括死亡探索己方的 BLUE_FLAME，但不补充随从标志或复活。
保留模式/raft31 分配可覆盖 physical 槽位而不修复别名。普通替代双随从分配止于 16，独立于 31。
场景命令不招募、不复活、不登木筏。

### 阅读全部二十九个命令

**已确认：**`0x599B2` 的 cs_599B2 有二十九个顶层命令。
此分组覆盖每个索引；[精确有序命令及嵌套体表](../../../research/cotton-balloon-through-floor-world-entry.md#all-actors-and-the-complete-departure-program)
为操作数权威。

| 索引 | 有序工作与后果 |
| --- | --- |
| 0–2 | 分别在 Bowie、Peter、B 安装/等待 eas_Init。 |
| 3–5 | 依次 setDest 至 Bowie `(35,24)`、Peter `(34,24)`、B `(33,24)`，分别等待其当前目的地。 |
| 6 | Bowie：jumpRight 1；faceDown 1；moveDown 2；endActions。 |
| 7 | Peter：moveRight 1；jumpRight 1；moveDownRight 1；faceDown 1；endActions。 |
| 8 | B：moveRight 1；jumpRight 1；faceDown 1；moveDown 1；endActions；仅等待 B。 |
| 9–11 | setPos 137 `(36,25)`、138 `(37,24)`、139 `(35,24)`，均 Up。 |
| 12–14 | stopEntity Bowie、Peter、B：停止动画计数，不停止行进或行动。 |
| 15–20 | 按 Bowie/Peter/B/137/138/139 顺序，六个独立 customActscriptWait 程序体：speed 10/10；jump eas_Idle；end。 |
| 21–26 | 按相同顺序为六者安装 moveDown 6/endActions；仅显式等待 139。 |
| 27 | 请求 Map26 传送至 `(26,8)`、Down。 |
| 28 | csc_end 及共用脚本返回尾部。 |

eas_Init 确立 speed 32/32、零加速度因子、无加速/减速、自动朝向、Up orientation、共享尺寸 24、精灵更新及 idle；
设置 obstructable 开、uncollidable 关、immersion 关。不设置位置、HP 或动画开启。
替代 InitFixedSprite 另禁用自动朝向/动画。尺寸/orientation 不是永久世界比例或几何。

setDest 是绝对瓦片×384 移动，不是传送。它写行进和幅度 32 的有符号速度，
再等当前位置等于行动者的**当前**目的地。没有 HP 守卫、复活或独立寻路器。
改变行动、目的地或别名，会改变等待可完成的内容。

九个内联序列使用 csc2D，清 FLAGS_A bits 5/6。
其普通移动从实际当前位置写相对瓦片×384 目的地，使用当前速度字节并等待到达。
moveDownRight 有两个正增量。未修改的 bit 5 未设置目的地扫描分支返回 Z 已设置，绕过 esc04 冲突分支；
这不确立通用碰撞/物理规则。faceDown 1 更新朝向/精灵并等待 1，不发生位移。

jumpRight 1 则复制 eas_JumpRight 前 34 字节：自动朝向关、相对 `(1,-1)`、wait 6、相对 `(1,1)`、
wait 6、相对 `(0,0)`、自动朝向开。操作数 1 既不改变距离，也不改变等待。
这些是**计时器**等待，不是目的地到达等待；每个相对写入都从当时当前位置开始。
最终零位移将目的地重写为该位置。相加增量不能证明两瓦片落点或三人的精确位置。
复制字节不含源码 idle 跳转；后续内联命令在 csc2D 追加的 idle 跳转之前继续。

索引 8 等待 B，不汇合 Bowie/Peter。后续行动安装可替换其未完成行动。
每个 setActscriptWait/customActscriptWait 仅轮询其解析行动者的 idle 指针，没有超时、HP 守卫或全行动者汇合。
行动等待计时器以 physical 索引作种子。Trap6 启用实体更新；实际 VInt 进展仍为前提。
X 至少为 `$7000` 的行动者被更新循环跳过；`(63,63)` 低于该界限。

setPos 写当前/目的地坐标和朝向，不重置行动、速度或速度分量。
其真正己方 HP 守卫不适用于 NPC137–139。stopEntity 仅写动画计数器 -1；跳过 HP0 Bowie/Peter，B31 则绕过该守卫。
死亡行动者并非统一被跳过。自定义程序体将 X/Y 速度改为 10/10 并跳至 idle；不还原动画或 Init/位置。

最终六个向下行动依次安装，允许并发进展但非同时开始。
各自从实际位置请求 `(0,+6)` 并等待自身目的地。
对象起点一致且未改变时，请求端点为 137 `(36,31)`、138 `(37,30)`、139 `(35,30)`；三人端点依赖早先移动。
139 完成不证明其他五者完成。程序没有附着/携带关系、地图精灵赋值、hide、对话、声音、淡入淡出、
摄像机目标、剧情标志写入或实体群替换。将其描述为下降是**推断**，不是已观察动画或通用物理合同。

## 传送必须存续并被消费

**已确认：**命令 27 写 MAP_EVENT_TYPE=1、PARAM1=0、PARAM2–5=`26,26,8,3`。
此待处理请求不立即改变 CURRENT_MAP 或加载目的地。它覆盖旧请求；后续写入方可替换或清除它。
它既非队列，也非预留转换。

csc_end 在仍有对话窗口时有条件地等待滚动，清滚动速度并经 trap6 返回。
它不添加六行动者完成等待。处理器随后设置 d6=-1。
物品包装器关闭立绘/文本，等待一次 VInt，并在返回前等待 physical 0 的当前目的地。
该额外主行动者等待不汇合 Peter/B/137/138，也不确立全部行动脚本 idle。
FieldMenu 退出；ProcessPlayerAction 还原视野/实体 VInt 并返回。
未返回前缀留下已完成效果，包括移除/请求，不作撤销。

ExplorationLoop 到达 WaitForEvent。待处理非零类型优先于为 physical 0 安装受控行动。
ProcessMapEvent 分发前清采样事件类型；type 1/PARAM1=0 提交/清 WARP_SFX，让 physical 0 空闲、
从探索展开返回并读取元组。场景不写 WARP_SFX，因此单凭传送不选择特定声音。
其非负 X26/Y8 替换回退低字节，朝向 3 直接读取；这些写入不会将任意传入负值/高字节状态规范成保证坐标。

存续返回进入 MainLoop 的普通 SwitchMap **然后 CheckBattle** 路径，与 Battle22 即时结果返回不同。
完整三行标志切换地图表没有 Map26 替代。全部四十五战斗行均要求地图匹配；没有一行指向 26。
CheckBattle 返回 d7=-1，为 `(Map26,26,8,Down)` 选择探索。
Battle22 使用 Map22；Battle23 使用 Map27。其解锁、介绍/完成或 F88 挂起标志不能凭空产生 Map26 战斗行。
选定场景也不改变 EGRESS_MAP。被替换请求遵循自身使用方，不能据此声称该 Floor World 准入。

这是条件目的地**加载器准入**，在实体群/init 之前。不证明 Floor World 内部、即时遭遇、可用控制或 Battle23 开场。
[Floor World 首次抵达与条件探索](floor-world-first-arrival.md)从此接缝继续，使用已接受 #920 研究：
全新实体群、完整降落/爆裂场景、F779/EGRESS 和独立 F780 停放，再到条件控制。
该指南保留传入状态、服务和表现限制，止于居民、巨环、搜索/退出及 Battle23 之前。
这些后续使用方仍不属本离开指南；导航链接不扩展声明。

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `63fab597ee78136cd88f7cdefa2bc6c4512cf6b5` 包含
[#916/PR918 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/918#issuecomment-6175709354)及
[#915/PR917 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/917#issuecomment-6175632640)。
研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
和 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
本综合指南不增加重新获取源码、提取或运行时观察。

对抗审阅检查完整已接受研究、返回/交互/实体群所有者、
[编写程序合同](../contracts/standalone-map-script-program-data.md)及选定公共 fixture/schema/验证器边界：

| 既有 fixture / 复现命令 | 其绑定内容及本指南额外使用内容 |
| --- | --- |
| [sf2-map-event-item-transactions-static-v1](../../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)，`uv run sf2 h2 map-event-item-transactions` | 表/处理器、精确别名、位置哨兵值、移除调用和 d6 调用方接缝；被调用方算法和实际持有需要已接受局部关联。 |
| [sf2-map-event-request-consumption-static-v1](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`uv run sf2 h2 map-event-request-consumption` | 轮询/重置/分发前清除拓扑；选定类型/元组及后续准入是直接研究关联，不是自然到达。 |
| [sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json)，`uv run sf2 h2 map-init` | 选定配置/init 结构；已分配随从、活动实体群和宝箱交付是独立前提/使用方。 |
| [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`uv run sf2 h2 map-script-engine` | 处理器局部语义及程序结构；精确 3/29 命令、九内联和六自定义体以及调用方/行动返回属于完整已接受选择。 |

既有 schema/验证器为
[transactions schema](../../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json)/[验证器](../../../../src/sf2tool/h2/map_event_item_transactions.py)、
[consumption schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[验证器](../../../../src/sf2tool/h2/map_event_request_consumption.py)、
[init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)/[验证器](../../../../src/sf2tool/h2/map_init.py)，
以及 [script schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)/[验证器](../../../../src/sf2tool/h2/map_script_engine.py)。
交易 fixture 在顶层存储 sourceContext/eventItemTransactions；其分组未知及汇总不验证服务程序体或自然选择。
编写程序合同负责数据，不负责移动执行或渲染。

[交易概览逐服务所有权](../../../research/map-event-item-transactions.md#ownership-and-unknown-boundary)
及其 fixture 与 #916 精确解析一致：GetItemInventoryLocation 和 RemoveItemBySlot 属于 `code/common/stats/itemstats.asm`；
ReceiveMandatoryItem 和 RemoveItemFromInventory 属于 `code/common/stats/iteminventory.asm`。
替代 `code/common/stats/items/itemfunctions_s7_0.asm` 仍排除。概览保留仅调用方边界；
已接受 #916 研究负责这里使用的被调用方算法。运行时问题队列依 ADR0014/0016 仍暂缓。

保留 [#916 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/916#issuecomment-6175681254)：
原始 head `9aaa845048a3be6d3c9011849e1f832c220ecde4`、变基 head `48d5175f0027a2a01088d171a2aaca02260da8a8`、
[原始 CI38616821310](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38616821310)和
[变基 CI38617104204](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38617104204)。
两次 scope/research-public 均成功；engine/adapter/H4 跳过。
未改变 18,285,979 字节对在 0.081s 内比较通过。
过大读取、猜测源码/配置/语料路径、字面通配符修复、fixture `expected` KeyError 及匹配 URL 的私有正则 AssertionError 均随修正保留。
链接的前篇对象、结果及检查/传输失败仍在这些收据中；本指南不重跑或替换。
[#915 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/915#issuecomment-6175613847)
保留独立的仅 scope [CI38616608492](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38616608492)
及其保留检查/导航/校验失败和链接前篇。

直接验收审阅完整正文/diff、每组命令和数量、坐标/身份/等待范围、溯源、链接/片段、英文/私有边界及空白。
检查已配置的干净已提交依赖规划器及实际最终 head CI。
不单因正文选择局部 normal/full/engine/Godot/H3/H4 或翻译套件；研究的
[复现边界](../../../research/cotton-balloon-through-floor-world-entry.md#reproduction-checks-and-retained-limits)继续负责比较和输入。

**未知：**自然宝箱/守卫/物品使用可达性及持有/选择；实际标志、军团/物品栏/HP/职业、随从/行动者/木筏别名、
布局/当前与已保存位置；已完成输入/文本/行动/滚动/精灵/转换服务；实际运动、视觉/音频表现、可用控制及持久性。
此处反例是传入源码状态，不是已执行场景；依
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)及
[ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)，也不构成自动运行时队列。
这既未完成战役，也未完成 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
