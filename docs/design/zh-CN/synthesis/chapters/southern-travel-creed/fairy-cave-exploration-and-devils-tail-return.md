# Fairy Cave 探索与 Devil's Tail 返回

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/fairy-cave-exploration-and-devils-tail-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Fairy Cave 包含四项选定宝箱奖励：Angel Wing、Brave Apple、Medical Herb 和 Mithril。
符合准入条件的搜索可以领取奖励、让已领取的宝箱保持空置，或在无人有物品栏空间时还原宝箱。
两个出口都请求 Devil's Tail。在洞外检查摇晃的岩石，可能得到 Peter 的提示；该检查与 Elric 打开通道的交互彼此独立。

**已确认：**已接受静态研究确立下述数据、条件分发及服务顺序。**推断：**岩石对话提示了由另一所有者负责的 Elric 通道交互。
**未知：**自然到达、实际布局和传入出战队伍/运输状态、服务完成、表现、响应输入及存档持久性。
这些是条件内容规则，不是连续游玩行程，也不保证四个宝箱都能自然到达。

本指南接续 [Harpy Pond 离开至 Fairy Cave 入场](harpy-pond-departure-through-fairy-cave-entry.md)，
使用[已接受探索/返回研究](../../../../../research/chapters/southern-travel-creed/fairy-cave-exploration-and-devils-tail-return.md)。
后续见 [Battle21 开场与初始名册](battle21-opening-and-initial-roster.md)。
止于完成的条件局部交互/搜索、Map67 控制或选定请求，或开场前 Battle21 准入。
Creed 穿行/内容、Battle21 场景/名册/交战/结果、地形重建、物品使用效果及经济不在本指南范围内。
Fnnn 表示标志 nnn；行索引从零开始。坐标、等待及声音 ID 是源码操作数，不是观察位置、实测时长或可听结果。

## 搜索没有局部配置程序的洞穴

**已确认：**完整 MapSetups 表中没有 Map32。GetCurrentMapSetup 到达终止符并返回 `ms_Void: dc.w $FFFF`。
其实际使用方赋予该哨兵值具体含义：共享实体生成跳过固定行，但重建 Bowie/随从并执行木筏尾部；init 不调用回调即返回。
已接受入场边界设置 CURRENT_MAP=32/CURRENT_BATTLE=255，并有条件地安装控制。
这不会禁用共享搜索，也不强制步行、成员资格或 HP。Map32 的标志图块、踩踏、屋顶及其他物品表均为空。

仅有 F67/F68 时，Peter 和 B/Oddler 占据 physical 1/2。Map32 不是大地图，因此仅 F65 不会添加 Caravan 随从。
Elric 没有随从行。其他传入选择器、查找替换及模式/木筏状态遵循共享所有者；全新实体生成不会保留通道场景的临时 Elric。
空局部配置不确立特定角色群。

普通 C 先使 physical 0 空闲，等待移动/视野滚动。在调试/Caravan 特殊分支之后，尝试 GetActivatedEntity。
即使洞穴实体分发器未调用回调便返回，被激活实体仍消耗此次交互，不会继续落入搜索。
该分发器写入 EVENT_RELATIVE_POSITION，之后哨兵值绕过立绘、朝向、实体 VInt 及回调工作。
只有没有激活实体的 C 才以 d6=1 调用 CheckArea。FieldMenu 的显式 Search 清除 d6，直接调用 CheckArea，随后离开菜单。
实体目标选择可以越过柜台图块；CheckArea 只检查面前的一块。

CheckArea 要求视野目标非负，清除名称索引和台词值，依据方向偏移及每块 384 内部单位计算面前图块，并读取宽度为 64 的工作布局。
其掩码类型（`& $3C00`）优先于描述坐标：

| 工作布局类型 | 选定内容 | 初始 / 空置文本 ID |
| --- | --- | --- |
| `$1800`，宝箱 | OpenChest，洞穴宝箱表 | 403 / 408 |
| `$2C00`，花瓶 | CheckNonChestItem，空洞穴其他物品表 | 404 / 409 |
| `$3000`，木桶 | 同一其他物品使用方 | 405 / 410 |
| `$3400`，书架 | 同一其他物品使用方 | 427 / 412 |
| `$1C00`，可搜索区域 | 同一其他物品使用方 | 423 / 412 |
| 其他掩码值 | 区域描述分发器 | 洞穴哨兵值不处理描述。 |

五个物品分支都不会在空结果、重复或奖励失败后重试描述。宝箱类型图块仍需匹配表行。
其他类型下，空描述返回未处理：d6=1 返回 d0=0，C 打开菜单；d6=0 发出调查 423，再发出空置 412，关闭文本并返回 -1。
已处理的物品/描述路径返回 -1。列出的坐标本身既不证明当前图块类型，也不证明可达。

空区域描述分发器清除 d7 并测试其零结果。空区域事件分发器跳过回调及关闭窗口，但仍等待一次 VInt 和 physical 0 位置/目的地相等；
它不会凭空产生洞穴随机战斗事件。物品使用分发器独立清除台词、掩码物品、写入相对位置、清除 d6，并跳过回调/关闭窗口，同时保留 VInt/physical 0 等待。
该物品使用边界既不授予可搜索奖励，也不确立每种通用物品使用结果。

## 四项奖励与物品栏边界

**已确认：**完整 `map32/7-chest-items.asm` 包含以下四行及终止符。每个四字节 mapItem 保存 X、Y、标志字节和物品字节；
标志直接使用，不加宝箱基值。枚举提供其身份：

| 行 | 坐标 | 领取标志 | 物品字节 / 奖励 |
| --- | --- | --- | --- |
| 0 | `(23,15)` | F160 | 4 / `$04`，Angel Wing |
| 1 | `(24,15)` | F161 | 15 / `$0F`，Brave Apple |
| 2 | `(25,15)` | F162 | 0 / `$00`，Medical Herb |
| 3 | `(19,26)` | F163 | 123 / `$7B`，Mithril |

Medical Herb 的物品 0 是真实奖励。空物品是 ITEM_NOTHING=127。四项均低于金币宝箱阈值 128；名称不确立使用效果。
GetChestItem/GetItem 选择当前地图宝箱表及其首个 X/Y 匹配。CURRENT_BATTLE=255 跳过战斗坐标调整。
无匹配返回 -1，掩码为物品 127；OpenChest 对该未匹配情形不改变标志或图块。

对匹配行，若领取标志已设置，OpenChest 替换为 127。随后设置标志、写入打开宝箱图块 `$D802`，并请求平面更新。
关闭的 `$D801` 和打开的 `$D802` 经掩码均为宝箱类型。标志已设置的重复搜索发出 403/408，不授予物品；持有另一份物品不是重复谓词。
反之，传入未设置标志配合外观已打开的宝箱，仍可选中奖励。

[共享接收者与回滚所有者](../../../../../research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md#four-item-rows-and-immediate-inventoryflag-consumers)
提供物品系统细节。此处的直接后果为：

1. 发现物品文本 413 先于测试 ally index 0 的四个槽位。有空间时，AddItem 写入掩码物品 ID 为 127 的首个槽位，保留索引/损坏位。
   调用方提交物品音乐，发出 415，并使用 FadeOut_WaitForP1Input。即使传入成员资格不含 Bowie，仍测试 index 0；它不是动态选择当前领队。
2. 若 index 0 已满，UpdateForce 按己方角色索引顺序重建已加入成员。
   OTHER_FORCE_MEMBERS_LIST=TARGETS_LIST+1 的列表约定与 length-2/DBF 扫描跳过首个已加入条目。
   普通 Bowie 已加入状态下，首个有空间的其他已加入成员接收奖励，包括后备和 HP 为零成员。
   调用方设置接收者名称、提交音乐、发出 416、等待并关闭文本。缺失 Bowie 成员资格不会修复该跳过约定。
3. 若没有符合条件的空槽，包括初始扫描计数器为负的情形，文本 417 先于还原搜索坐标并依次调用 CloseChest 和 RefillNonChestItem。
   宝箱标志清除，恢复 `$D801` 并请求平面更新。洞穴空其他物品表使第二次调用无效果。
   后续符合准入的搜索可重试。已领取宝箱走空置分支，不会到达此物品栏回滚。

调用方检查容量，但不按 AddItem 返回的 d2 结果分支。成功交接以一致的物品栏及服务返回为假设。
先前标志/布局变化不是在文本、声音、等待或其他服务停滞时自动回滚的通用事务。

文本 403 使用 `{NAME}{W2}{CLEAR}`；408 含 `{W1}`。发现物品 413 使用名称/换行/物品和 `{W2}{CLEAR}`。
领取行 415/416 使用名称/换行/物品，不含 W-token，依赖显式物品等待；417 使用换行和 `{W1}`。
初始名称是 ally 0 的当前名称。这些控制符标识服务及参数流，不代表显示、物品栏变更或声音已完成。

后续完整地图加载时，LoadMapBlocksAndLayout 加载基础图块/布局、应用标志图块行，再在每个已设置宝箱标志的对应坐标写入 `$D802`。
因此，即使 Map32 标志图块表为空，F160–F163 仍独立重新应用打开宝箱。它们在临时 256–383 重置范围之外。
仅改变标志不重绘当前地图，当前地图重载还可能保留布局。存档持久性及可见的打开宝箱仍为**未知**。

## 两个出口与返回 Devil's Tail

**已确认：**Map32 完整外部传送表有两行：

| 行 / 洞穴坐标 | 无滚动请求 |
| --- | --- |
| 0 / `(2,3)` | Map67 `(51,23)`，Down 1，no-scroll 0 |
| 1 / `(29,3)` | Map67 `(57,24)`，Down 1，no-scroll 0 |

WarpIfSetAtPoint 将传入内部坐标除以 384，按顺序扫描带负字节通配的行。步行仅从移动/布局 `$1000` 分支调用它；载具生产方有独立检查。
选定行写入 event 1、parameters 1–6 和 WARP_SFX。它不测试 F160–F163、F751、F13、F520、F421 或 F521。
在相同洞穴坐标入场并不是新的移动生产方，既不证明立即弹回，也不证明向西抵达 Creed 的路线。

请求若保留至 ProcessMapEvent，待处理状态在分发前清除。Type 1 无滚动提交并清除 WARP_SFX，使 physical 0 空闲，退出 ExplorationLoop，
并将非负表行坐标代入采样位置。MainLoop 收到 `(67,51,23,Down,d4=0)` 或 `(67,57,24,Down,d4=0)`，不规范化 PLAYER_TYPE。
后续生产方可替换唯一待处理槽位；发布请求不等于过渡已被使用。

MainLoop 在全新探索前调用 SwitchMap，再调用 CheckBattle。没有按标志切换的 Map32 或 Map67 替换，没有 Map32 战斗表行，
而 Map67 恰有一行：slot 21、区域 `(21,0)`、大小 32x42、通配触发 255/255。区域矩形不限制这些触发。
传入 F421 从任一返回接纳 Battle21，包括超过该区域 X 范围的 `(57,24)`。
F521 已设置在选择后清除 F421，不取消准入；F521 未设置则保留它。止于 BattleLoop 开场前。洞穴出口和岩石描述均不设置这些标志。

F421 未设置时，全新探索清除待处理，从 STEP_COUNTER 减去 20000，并按有符号字负结果钳位至 0，治疗符合条件的存活/不死己方角色，再执行淡入淡出。
它设置 CURRENT_MAP=67/CURRENT_BATTLE=255，加载瓦片集、重建实体/查找/地图精灵、清除标志 256–383、设置 F80、加载地图/视野、安装基础 VInts 并调用 init。
临时重置保留 F13、四个宝箱标志、F421/F521、F520/F751 及随从选择器。

Map67 有一套六指针配置、空固定实体列表及仅返回的 init。共享实体生成在请求元组处放置 Bowie physical 0。
Map67 是大地图：仅 F65/F67/F68 时，physical 1/2/3 为 Caravan/A、Peter 和 Oddler/B。额外选择器可添加 Sarah/Chester 或替换 A/B 映射。
这些标志不测试成员资格/HP，也不添加 Elric 行。全新重建丢弃洞穴专属行动者及地图局部跟随。

[共享模式/木筏所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md)保留传入状态。
Mode 2 下，physical 0 使用木筏精灵，随从以站立脚本停放在 `(64,64)`，并在那里声明独立 raft 31。
其他模式仅在 F64 且已保存 RAFT_MAP=67 时，以掩码后的已保存坐标声明 raft 31。Mode 1 不重复登乘。
两个出口/init 均不重写已保存木筏坐标/地图，不下船，也不修复任意运输状态。

F751 独立控制后续加载器从 `(0,0)` 到 `(56,22)` 的 3x2 图块复制。
[通道所有者](../../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#live-block-change-completion-flag-and-request-return)
区分当前复制、脚本后标志写入及后续重新应用。返回不会重放场景。仅有标志或文本不证明可通行、可见开口或当前布局修复。

在条件音乐/淡入淡出后，pending 0 让 WaitForEvent 跟随 physical 0 并安装控制：mode 0 步行；mode 1 源码名 eas_Raft，实际 Caravan；
其他值为 eas_Caravan，mode 2 实际为木筏。待处理请求优先。这是条件控制器安装，可用输入及服务完成仍为**未知**。

## 抵达与后续区域选择彼此独立

**已确认：**东返回 `(57,24)` 不是 F520 求救坐标。已接受修正保留完整 Map67 区域选择：
`(57,25)` 选择独立 F751/F13/HP 通道；`(56,26)`、`(57,26)`、`(58,24)`、`(58,25)` 选择 F520 求救；其他坐标选择默认项。
两个洞穴返回均不会仅因抵达而分发区域事件。单独提交任一返回坐标，都选择默认项。

符合准入的求救事件在 F520 已设置时返回，否则执行[完整已接受分支](../../../../../research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md#complete-call-for-help-program)，
包括当前 B/A/Peter 动作、文本 2543–2549 及最终 Map51 `(11,22)`、Up 请求。
其[请求/准入边界](../../../../../research/chapters/southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md#independent-pond-requests-and-admission-before-init)保留部分服务/替换限制。
`(57,25)` 通道属于[独立已接受谓词与场景](../../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#separate-map67-zone-selection-and-passage-admission)。
两个分支均不是在 `(57,23)` 检查岩石的后果。

后续默认区域事件中，步行 `$1400` 生产方在原始布局阻挡测试前提交 event 6 和字 X/Y。
只有获准的移动尾部才递减正 STEP_COUNTER；移动受阻可留下请求，而无位移/递减。Caravan 控制没有等效区域生产方。
Type 6 安装 physical 0 Init；包装器选择/调用区域事件、关闭窗口、等待一次 VInt 及 physical 0 位置/目的地相等。安装 Init 不证明完成。

Map67_DefaultZoneEvent 加载 BATTLE_DEVIL_TAIL=21 并调用 CheckRandomBattle：

| 传入状态 | 助手结果 |
| --- | --- |
| F521 未设置 | 无论计数器均走请求路径；无 RNG。 |
| F521 已设置，计数器非零（包括负数） | 无新请求/RNG；助手保持计数器不变。 |
| F521 已设置，计数器 0，range-8 RNG 返回 0 | 请求路径；无第二次 RNG。 |
| F521 已设置，计数器 0，首次 RNG 非零 | 无请求；range-4 RNG 结果+2 成为计数器 2–5。 |

这些是源码条件分支，不是测得遭遇概率。请求路径设置 F421，在 MAP_EVENT_TYPE 写入压缩值 `$000100FF`，将计数器设为 30000，等待视野滚动，
提交 SFX_BOOST 并执行闪烁脚本。它既不设置 F521，也不立即调用战斗。滚动/闪烁/包装器完成及后续请求保留是独立条件。

压缩写入提供 event 1、no-scroll parameter 1=0 和 parameter 2=255/MAP_CURRENT；parameters 3–5 及 WARP_SFX 保持不变。
未改变的步行区域链中，字 parameter 3 仍含区域 Y：使用时替换 X=0 和该 Y，以残留 parameter 5 为朝向，提交/清除残留 WARP_SFX。
这不一定是在原位置重载。[压缩重载所有者](../../../../../research/chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md#packed-reload-and-residual-parameters)
保留其他传入字节和有符号坐标情形。

包装器返回后，待处理优先于控制。Type 1 返回 map 255；SwitchMap 无 255 行，CheckBattle 解析当前 Map67，
若 F421 仍设置则接纳 Battle21，与残留坐标无关。F521 再次在选择后清除 F421。止于开场前。替换后的请求有其自身继续路径。
若 F421 在准入前清除，无战斗 map 255 沿既有实体群重载主属性/木筏、加载地图/init 并有条件地安装控制，不执行全新实体配置或临时重置。
这不确立无中断路线或自动遭遇。

## 检查岩石与条件提示

**已确认：**Map67 完整区域描述表在 `(57,23)` 有一个函数行，condition byte 0、kind 1，以及指向 Map67_DescFunc0 的表相对偏移。
包装器的 d3=434 不是额外无条件文本行。DisplayAreaDescription 匹配压缩 X/Y，对 condition 0 绕过 d6 筛选，保存/还原 d6 后调用函数。
只要 CheckArea 到达描述分支，C 的 d6=1 与显式 Search 的 d6=0 都可接纳它。

上述五种物品类型抢先于描述。Map67 无宝箱行；唯一其他物品行是 `(45,9)`、F214、Mithril。
因此在 `(57,23)`，物品类型布局产生对应空结果。搜索 `$1000` 传送类型可到达此描述，步行传送生产方则是独立动作。
普通 C 的激活实体也会拦截检查。自然岩石图块类型和可达仍为**未知**。

| 有序条件 | 对话与直接后果 |
| --- | --- |
| 每次获准调用 | 调查 424 的 `{NAME}` 指 ally 0，含 `{W2}{CLEAR}`，随后测试 F751。 |
| F751 已设置 | 434 说没有特别之处，`{W1}`；返回，不解析 Peter 或测试 F13/HP。 |
| F751 未设置 | 2590 描述摇晃岩石，`{W1}`；关闭文本，将当前台词复制到 SPEECH_SFX_COPY，解析 logical Peter 7 并写入当前立绘/台词，再测试 F13。 |
| F751 未设置、F13 未设置 | DisplayCurrentPortrait；2591 将怀疑归于 Peter，`{N}{W1}`；不查找 Elric HP。 |
| F751 未设置、F13 已设置、Elric HP 字 0 | 读取 combatant 13 HP；DisplayCurrentPortrait；2592 建议让 Elric 帮忙，`{NAME;13}{N}{W1}`。 |
| F751 未设置、F13 已设置、Elric HP 非零 | 已完成 Peter 解析后直接返回，无后续文本或 DisplayCurrentPortrait。 |

HP 极性按源码原样保留。F13 是已加入成员资格，独立于出战 F45，因此不排除后备成员。通道区域事件反而要求 Elric HP 非零。
描述既不加入、治疗或生成 Elric，不移动行动者、不复制图块、不设置 F751，也不请求传送；不测试 F520。
提示不确立 仙女 历史或物品解法。

GetEntityPortaitAndSpeechSfx（源码拼写）沿 Peter 当前 logical-7 查找到 physical 行动者，按其地图精灵选择属性。
Peter 基础/转职后映射提供立绘及提示音 4/5；不匹配精灵使用共享默认值。解析器不分配 Peter，不检查 F67、成员资格、出战状态或 HP。
全新未分配查找 0 可别名到 Bowie；陈旧/255 映射不会被安全拒绝。因此 Peter 归属取决于实际映射。
DisplayCurrentPortrait 跳过负立绘；否则 OpenPortraitWindow 可因窗口已打开而返回，或创建/加载/移动/等待/注册眨眼。
一次调用不证明新面孔已渲染。

这些直接 txt/clsTxt 操作使用 Trap5_TextBox 的寄存器保留显示/关闭服务，不是地图脚本单文本操作码，也不使用其调试跳过规则。
CheckArea 的名称索引 0 解析 424 的调查者姓名；`{NAME;13}` 显式命名 combatant 13。
`{N}` 为换行；W1/W2 使用不同输入等待路径，W2 带光标/验证处理；CLEAR 清除对话布局。
逐字显示读取 CURRENT_SPEECH_SFX，为选定非空格字形提交声音。显示、输入确认、音频及计时仍是服务限制，而非观察结果。

台词复制不等于还原。CheckArea 在 F751 未设置分支复制前，已将台词清为 0；已检查返回均不读回 SPEECH_SFX_COPY。
DisplayAreaDescription 关闭立绘/文本并返回 d7=-1；外层分发器测试该结果，CheckArea 再关闭文本并返回 d0=-1。
窗口关闭以及外层 C/FieldMenu/探索返回均不还原台词或 CURRENT_PORTRAIT。解析值因此保持至其他写入方改变，
包括已加入、HP 非零而没有 Peter 文本的分支。F751 已设置的检查保留清零台词，且不调用该解析器。
这不是全局永久性或可听结果声明。

Map67 默认实体/物品回调都仅返回。实体包装器仍可解析立绘/台词、暂停/恢复实体 VInts 并关闭窗口；modifier 0 不请求改变朝向。
物品包装器保留清除/掩码/相对位置/窗口/VInt/physical 0 等待服务。两个回调都不是岩石函数，也不绕过搜索优先级。
完成的局部返回依待处理状态恢复既有探索/实体群；检查前不强制全新加载/控制。
文本/窗口停滞可留下中间状态，不会回滚或自动执行通道动作。

## 证据与剩余问题

**已确认溯源：**本综合使用 main 提交 `05e2fd194be07c6581e4fd3fe1abf3b38854ae65` 上的已接受研究，原始源码为
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA 零售 ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
[研究复现边界](../../../../../research/chapters/southern-travel-creed/fairy-cave-exploration-and-devils-tail-return.md#reproduction-and-retained-limits)
负责源码路径、地址、字面文本/控制符关联和直接使用方。本说明不重新调查原始源码、不重新生成载荷，也不新增运行时采集。

| 已审阅表面 | 声明边界 |
| --- | --- |
| 已接受探索/返回研究及共享接收者、通道、求救和压缩重载所有者 | 完整选定宝箱/出口/岩石数据及实际使用方。原版坐标/文本/行动者效果仍由研究负责；仅公开汇总不能复现它们。 |
| `sf2-field-search-control-static-v1`，[fixture](../../../../../../tests/fixtures/h2/field-search-control-static-v1.json)，fieldSearchSpine.callers/blockDispatch/itemRecipientPath/fullInventoryRollback/returnContract | 共享 C/Search、类型优先级、四槽接收者及回滚主干；不是洞穴特定行或已完成领取。 |
| `sf2-map-events-static-v1` [索引](../../../../../../tests/fixtures/h2/map-events-static-v1.json)、[实体](../../../../../../tests/fixtures/h2/map-events/entity-programs.json) row 616、[物品](../../../../../../tests/fixtures/h2/map-events/item-programs.json) row 63、[区域](../../../../../../tests/fixtures/h2/map-events/zone-programs.json) row 124 | 公开有序 Map67 仅返回回调和默认 Battle21 调用，包括源码行身份。不序列化完整岩石内容。 |
| `sf2-map-descriptions-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-descriptions-static-v1.json)，expected.consumerFacts；[区域描述合同](../../../contracts/world/map-area-description-routing.md) | 共享坐标/条件/函数偏移路由。旧的仅 d6=1 调用讨论覆盖无实体路径；已接受搜索研究也确立显式 Search。公开条件行不包含此 condition-0 岩石程序体。 |
| `sf2-map-event-random-battle-state-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)，eventRandomBattleState.callerSites.map67-default-battle21/completionFlagGate/stepCounterGate/randomCadence/requestWriteSequence | 助手分支及请求服务，与抵达、后续使用及开场相区分。 |
| `sf2-map-init-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json)；`sf2-map-script-engine-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | 已接受保留配置/脚本比较支持其源码形状范围；完整缺失 Map32/战斗/随从及选定表关联来自研究。汇总相等不证明自然路线或控制完成。 |

已审阅 schema/验证器边界为 [field-search schema](../../../../../../schemas/h2/field-search-control-static-fixture.schema.json)
和 [field_search_control.py](../../../../../../src/sf2tool/h2/field_search_control.py)；
[事件索引 schema](../../../../../../schemas/h2/map-events-fixture-index.schema.json) 及
[实体](../../../../../../schemas/h2/map-events-entity-programs.schema.json)、[物品](../../../../../../schemas/h2/map-events-item-programs.schema.json)、
[区域](../../../../../../schemas/h2/map-events-zone-programs.schema.json)部分和 [map_events_fixture.py](../../../../../../src/sf2tool/h2/map_events_fixture.py)；
[描述 schema](../../../../../../schemas/h2-map-descriptions-static-fixture.schema.json) 和 [map_descriptions.py](../../../../../../src/sf2tool/h2/map_descriptions.py)。
[探索流程](../../../contracts/world/exploration-control-flow.md)、[入场路由](../../../contracts/world/map-entry-routing-state.md)、
[地图探索](../../../contracts/world/map-exploration.md)、[对话](../../../contracts/interface/dialogue-system.md)及[军团成员资格](../../../contracts/party/party-membership-state.md)保留共享边界。
这里使用更窄的已接受 C/Search 区别，不扩大旧合同的未知项。

保留 [PR882 原始/变基后交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880#issuecomment-6172113709)、
[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/882#issuecomment-6172145421)
和[东返回分发修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880#issuecomment-6171924743)，
包括保留比较、原始/最终 CI、已解决检查失败及链接的前序收据。这些审阅记录定位结果；原版声明由跟踪研究和精确已接受 Git 对象负责。
本指南的直接检查、已提交 planner 和最终提交 CI 属于其 Issue 交接；不创建新证据系统。

**未知：**自然洞穴/宝箱/岩石/出口地形及布局类型；实际标志、物品栏、接收者、HP、成员资格、行动者映射、模式和已保存木筏；
文本/窗口/移动/滚动/闪烁/加载服务完成、可见或可听结果、可用控制、存档持久性及自然 Battle21 准入。
物品 0 与空 127、重复/满物品栏、缺失 Bowie、实体拦截的 C 与 Search、优先图块类型、独立 F751/F13/HP、后备/死亡 Elric、
缺失 Peter、独立 F421/F521、负计数器和替换请求约束静态声明；它们不是已执行场景。
未知项不会自动排入运行时工作，也不表示战役/设计 epic 完成。
