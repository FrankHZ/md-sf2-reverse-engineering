# Creed 魔法门与 Desktop 转移

> 本文是[规范英文原文](../../synthesis/creed-magic-door-and-desktop-transfer.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**独立 Creed Mansion 区域事件编排警告、闪烁、缩小动作、旁白拾取和收藏，再请求 Desktop Kingdom。
**推断：**场景描绘出战队伍成为 Goliath 的收藏。对话解释魔法门，但选定代码不授予收藏成员资格或持久玩法尺寸。
**未知：**自然到达、实际传入行动者/状态、服务完成、渲染拾取及可用目的地控制。

本指南接续[首次 Goliath 阻拦](creed-mansion-entry-and-first-goliath-scene.md)，
使用[已接受魔法门/转移研究](../../../research/creed-magic-door-and-desktop-transfer.md)。
Standard 协议见 [Issue #899](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/899)
及[文档路线图](../documentation-roadmap.md#当前基线与近期综合)。
止于全新 Map22 探索入场的**实体生成/init 之前**，或选定 Battle22 交接的**开场之前**。
Desktop 抵达、恢复、后续宅邸内容及 Chessboard 玩法是独立工作。本指南不带来新游玩流程、源码重新调查或重制决策。

## 独立门事件与传入行动者

**已确认：**当前配置提供该表时，Map33 区域 `(12,14)` 选择 Zone1。Fnnn 表示标志 nnn。
Zone1 检查 F783：已设置则返回，未设置则运行完整 `cs_5AF36` 后返回。
它既不检查 F774，也不写 F783 或其他完成标志。F774 属于较早阻拦，其完成不是此场景的局部前提。
前一场景不落入缩小程序，F774 未设置/已设置未必提供相同位置或历史。

基线配置提供该区域表。当前配置选择仍按顺序扫描 F523、F783、F784、F786、F22 覆盖，最后设置标志优先。
坐标或基线指针不证明自然到达，也不证明另一选定配置提供此回调。
区域调用方首先清除旧事件类型，对 physical actor 0 应用 init 动作，再分发选定配置的匹配区域行。
这些调用方边界见[配置选择](../../../research/map-event-flag-route-selection.md)及[请求使用](../../../research/map-event-request-consumption.md)。

场景不创建行动者。它通过既有查找表操作 logical Bowie 0、Peter 7、B 31 和 NPC 128。
[前篇实体群审阅](creed-mansion-entry-and-first-goliath-scene.md)负责六个固定行及随从分配。
Map33 不是大地图，因此 F65 不提供 Caravan 前缀。F67 选择 Peter；独立 F74/F68/F70 行可分配重复 logical B 行动者，较后查找覆盖。
仅 F67/F68 时，Peter/B/128 解析为 physical 1/2/3。基线 logical 128 使用 WORKER 地图精灵，而文本称其为 Goliath；
仅军团成员资格不确立随从分配或传入映射。

缺失随从查找可别名到 physical 0。其他 B 行改变动作归属。
动作安装没有某些放置/朝向命令使用的己方存活跳过；HP/职业可改变映射精灵而不阻止安装动作，特殊精灵可跳过精灵改变工作。
相对移动从实际位置开始。F774 不修复任何这些输入。

## 警告、闪烁与缩小序列

**已确认：**已接受完整场景有 43 个命令，索引 0–42，包括终止符。
下表覆盖全部命令，包括三个内联动作体和一个自定义体。方向、尺寸、速度及等待数值是字面源码操作数，不是屏幕实测距离或时长。

| 场景阶段 / 命令 | 有序工作 |
| --- | --- |
| Goliath 反应，0–4 | 设置文本游标 1834。等待 128 的 `eas_Init`，再等待 `eas_Jump`。带等待自定义体设置速度 `48,48`，跳至 `eas_Idle`，再以 `ac_end` 结束。带等待内联体移动 Down 2、Left 2，再 `endActions`。 |
| 警告与首轮闪烁，5–12 | 摄像机指向 128；单文本 1834 警告勿入。白色闪烁 4、等待 40、白色闪烁 4、等待 30、白色闪烁 4、等待 5。 |
| 出战队伍反应，13–21 | 安装 Bowie 的 `eas_2xRightLeft`，等待 5；Peter 的 `eas_2xUpDown`，等待 5；B 的 `eas_2xRightLeft`，等待 20。白色闪烁 4、等待 10、白色闪烁 30。这些安装不等待对应行动者空闲。 |
| 缩小与责备，22–26 | 依次给 Bowie、Peter、B 安装 `eas_TwirlShrinkDisappear`，不等待完成；等待 60；单文本 1835 责备众人。 |
| 解释，27–31 | 等待 128 的 `eas_Init`；带等待内联 Left 3、`endActions`；128 朝 Up。继续文本 1836 解释门，再用单文本 1837 声称例外。 |
| 旁白收藏，32–36 | 停止 128 动画，等待 20，显示无立绘说话者单文本 1838；重启 128 动画；单文本 1839 宣布收藏。 |
| 离开，37–42 | `stepEvent 12,13`；带等待内联 Up 2、Left 5、`faceUp 20`、`endActions`；等待 128 的 `eas_Jump`；等待 20；`warp MAP_DESKTOP_KINGDOM,0,0,DOWN`；`csc_end`。 |

具名动作助手解释编排，不保证演出。`eas_Init` 将加速度因子重置为 0/0、速度为 32/32、关闭加速/减速、开启可阻挡、关闭 entity/map uncollidable、
开启自动朝向、orientation Up、关闭浸没、尺寸 24，更新精灵后进入空闲。它不重置位置。第二次 Init 替换先前速度 48。
`eas_Jump` 禁用自动朝向，移动相对 `(0,-1)`、等待 5、移动 `(0,1)`、等待 5、移动 `(0,0)`、还原自动朝向并跳至空闲。
RightLeft 和 UpDown 执行四次具名朝向变化，每次更新精灵并等待 30，随后空闲。
后续安装替换同一行动者动作指针；转向序列未先等待全部完成。

带等待动作形式只轮询选定 physical 行动者脚本指针是否为 `eas_Idle`。
内联编译器附加跳至空闲；`faceUp 20` 表示朝 Up、更新精灵、再动作等待 20，不是移动二十块。
相对移动将有符号瓦片差值乘以 MAP_TILE_SIZE=384，并使用目的地/冲突处理。
速度 `48,48` 是动作引擎操作数，不是实测像素速率。没有超时或缺失行动者恢复。
安装器以解析的 physical 索引播种动作等待计时器，动作等待使用方会用该值；即使首次等待也不是独立测量的时长。

从地图脚本角度，三个缩小安装是并发工作。其 等待 60 不轮询三者全部空闲。
后续等待针对 128 或 physical 0 坐标。因此，别名映射、被替换/停滞动作及跳过精灵工作，均可使“三个独立行动者在对话继续前可见地完成缩小”这一说法不成立。

## Goliath 的说法与命令实际变化

**已确认：**文本 1834–1839 按顺序关联源码键 072A–072F。此处保留完整字面文本和控制符；1836 接续 1837，不打开选择。

| 文本 | 说话者 / 形式 | 字面内容 |
| --- | --- | --- |
| 1834 | 128 / 单文本 | `No, don't go in there!{N}That door is...!{W1}` |
| 1835 | 128 / 单文本 | `What stupid kids!{W1}` |
| 1836 | 128 / 继续 | `Whoever passes through this{N}magic door becomes small.{W2}` |
| 1837 | 128 / 单文本 | `That is, anybody who's not{N}protected by Mr. Creed.{W1}` |
| 1838 | 无 / 单文本 | `Goliath picks up{N}{LEADER}.{W1}` |
| 1839 | 128 / 单文本 | `Now, you're part of the{N}collection!{W1}` |

五条命令使用 speaker 128/modifier 0。立绘和台词 SFX 从实际映射精灵解析；modifier 0 既不设置镜像位，也不设置右侧位，不保证显示立绘。
拾取文本使用 `$FF,ENTITY_NONE`，编码 `$FFFF`，绕过该查找并将台词 SFX 设为零。
`{LEADER}` 运行 UpdateForce，取首个 BATTLE_PARTY_MEMBERS 条目并解析战斗员名称。它未必显示不可变名称 Bowie。

`{N}` 推进文本行。W1/W2 轮询当前方向/A/B/C 输入掩码；W2 额外运行箭头及验证声音路径。
单文本命令在 DisplayText 后关闭立绘/文本并 sleep 10；继续形式把关闭留给下一命令。
调试模式加 P2 Start 可锁存 SKIP_CUTSCENE_TEXT，绕过选定文本命令和 csWait sleep。名义六条文本演出假设该跳过未锁存。

white-flash 操作数不是帧数。选定处理器将操作数右移三位并执行 DBF 循环：4 请求一次淡入淡出启动，30 请求四次。
每次启动选择全部调色板（`$F`）、counter maximum 1 和 FLASH_QUICKLY_2，等待 FADING_SETTING 清除及额外一次 VInt，再还原已保存 maximum；
处理器最后复制调色板。场景含四次单启动调用和一次四启动调用，既不证明四帧闪烁，也不证明三十帧闪烁。完成仍依赖这些服务。

摄像机命令将 logical 128 解析为 physical 行动者，不是固定摄像机坐标。
stop/start 将该行动者动画计数器写为 -1/0；它们不全局停止/启动移动、动作或输入。拾取文本位于这两次写入之间。
没有选定命令将 Bowie 附着到 128、授予收藏成员资格、招募/移除军团成员或提供选择。
**推断：**警告、缩小和对话共同描绘收藏。门的普遍效果及 Creed 保护仍是 Goliath 的解释，不是独立实现的通用门规则。

## 临时精灵工作、停放与共享尺寸

**已确认：**完整 `eas_TwirlShrinkDisappear` 有 42 个操作。
开始为 等待 5 和 resize ON，随后每行执行朝向变化、尺寸写入、精灵更新、等待 5（九组，每组四操作）：

| 顺序 | 朝向 | 尺寸操作数 |
| --- | --- | --- |
| 1 | Down | 22 |
| 2 | Right | 20 |
| 3 | Up | 18 |
| 4 | Left | 16 |
| 5 | Down | 14 |
| 6 | Right | 12 |
| 7 | Up | 10 |
| 8 | Left | 8 |
| 9 | Down | 6 |

最后四操作为 `ac_setPos 63,63`、`ac_resizable OFF`、`ac_setSize 18`、`ac_jump eas_Idle`。
最终尺寸写入后没有精灵更新。

尺寸命令写入**共享 SPRITE_SIZE 字**；resize bit 3、朝向、当前及目的地位置、动作指针和等待计时器属于 physical 实体。
精灵更新请求 ChangeEntityMapsprite；容量影响进展，特殊地图精灵/entity number 32 跳过此工作。
实体 resize bit 已设置时，ResizeSprite 使用当前共享尺寸，变换 RAM 中已解压地图精灵像素，包括两个步行帧。
并发动作共享该尺寸暂存字。这确立精灵工作，不确立每个角色的持久尺寸、玩法属性变化或地形尺寸规则。

SetPos 将 63 乘以 384，写入当前和目的地 X/Y 并清除 travel pair。
它既不释放槽位，也不清除查找、不改变军团成员资格或发出传送。其 X 仍低于调度器 `$7000` 空槽阈值。
精灵投影器将视野外精灵送出屏幕；停放 `(63,63)` 不等于在每种摄像机下都保证可见消失。
选定场景/动作命令没有移除或还原影子，动作名称也不证明独立影子结果。
即使尾部完成，也只证明临时行动者停放、resize off 及共享尺寸写入 18。后续目的地行动者在本指南范围外。

## 打开当前门与请求转移

**已确认：**`stepEvent 12,13` 将坐标乘以 384 并调用 OpenDoor。
CURRENT_BATTLE=255 时，Map33 完整踩踏表选择来源 `(25,0)`、大小 1x1、目的地 `(12,13)`。
非负 source Y 选择 FF0000 中行步长 64 字的当前布局单字复制。
它播放 SFX_DOOR_OPEN，并设置 MAP_AREA_LAYER_TYPE 选择的平面更新位。战斗内调用不执行此工作便返回。

这是当前布局变化，没有持久标志。Map33 完整标志事件表仅含 F22 大范围替换，没有按 F774/F783 重放此复制。
复制不保证渲染开口、碰撞/通行、穿行或存档/读档持久性。
最终带等待移动及 Jump 在复制后执行，必须返回才可到达传送命令。

字面传送编码 map 22、X 0、Y 0、Down=3。处理器首先写 MAP_EVENT_TYPE=1，parameter 1=0，parameters 2–5=`22,0,0,3`；
既不加载地图，也不等待抵达。后续失败不会回滚先前场景变更，其他写入方可替换共享待处理请求。
下述路线假设完整元组保留至使用，并非观察到的原子事务。

`csc_end` 在存在对话窗口时有条件地等待视野滚动，清除滚动速度并还原寄存器。Zone1 不写标志便返回。
区域包装器关闭立绘/文本、等待一次 VInt，再等待 **physical 0 当前 X/Y 等于目的地 X/Y**。
这既不是 Peter/B 完成汇合，也不证明普通源码放置：完成的缩小尾部已使两对坐标均为 `(63,63)`。

WaitForEvent 在重新安装普通摄像机目标 0 和当前模式控制器前检查待处理工作，因此保留的传送会立即返回。
ProcessMapEvent 在分发 type 1 前清除事件类型。无滚动分支使用/清除 WARP_SFX，使 physical 0 空闲并退出探索返回。
UpdatePlayerPosFromMapEvent 先读取来源瓦片位置/朝向，但显式非负 X/Y 字节覆盖两个位置，parameter 5 提供 Down。
因此来源停放坐标不改变请求的 `(22,0,0,Down)` 目的地。不保证场景返回与转移之间有可用来源地图控制区间。

## Desktop 准入取决于独立标志

**已确认：**MainLoop 从探索返回后先 SwitchMap，再 CheckBattle。
这不是跳过 CheckBattle 的替代战后入场。完整切换表没有 Map22 替换。
其唯一战斗坐标行为从零开始的 Battle22，元组 `22,9,0,30,30,255,255`：map 22、区域原点 `(9,0)`、尺寸 30x30 和两个通配触发坐标。

CheckBattle 先匹配地图，再 F422，再触发字段。不将请求与区域矩形比较。
请求 `(0,0)` 在矩形外，但 F422 已设置时通过两个通配检查。选择时，在检查 F522 **之前**赋值区域。
F522 已设置随后清除 F422，但不取消此次 Battle22 选择。Zone1、场景及缩小动作均不写这两个标志；旁白不提供目的地准入状态。

| 传入 F422 | 传入 F522 | 直接准入结果 |
| --- | --- | --- |
| 未设置 | 未设置或已设置 | 在区域赋值或完成检查前跳过 Map22 行；返回 d7=-1，以 map 22、`(0,0)`、Down 进入全新探索。 |
| 已设置 | 未设置 | 赋值区域 `9,0,30,30`，保留 F422，返回 d7=22 并交接 BattleLoop。 |
| 已设置 | 已设置 | 赋值相同区域、清除 F422，仍返回 d7=22 并交接 BattleLoop。后续检查需要自身传入状态。 |

全新探索接收显式 map 22，而不是 MAP_CURRENT=255，清除待处理类型，并在实体生成前赋值 CURRENT_MAP=22/CURRENT_BATTLE=255。
**止于 GetMapSetupEntities、InitializeMapEntities 和 init 之前。**战斗分支止于选定 BattleLoop 交接的**开场之前**。
两个分支均不确立已加载名册、抵达完成、最终目的地放置、加载成功或可用控制。

仅列后续标识符：基线 Map22 声明 30 个实体行，F522 选择替代实体配置。
两个指针表均引用 `ms_map22_InitFunction`；其分发器检查 F776，未设置时调用 `cs_59656`，返回后设置 F776。
后续 F522 已设置/F778 未设置选择 `cs_5996E`，再设置 F778。
实体群和这些场景体属于独立抵达/战后工作。本指南不使用并发抵达研究或 Battle22 开场/Chessboard 内容。

后续见 [Desktop Kingdom 首次抵达与条件控制](desktop-kingdom-first-arrival.md)，了解已接受全新实体群/抵达边界；
本指南保留实体生成前/开场前停止点。

## 证据审阅与改变状态限制

**已确认溯源：**研究已在 main `73366b7a557fe03399a675a4103d89b89c9f9806` 接受，固定于
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
[研究所有者](../../../research/creed-magic-door-and-desktop-transfer.md#agreement-and-provenance)负责精确路径、行范围及直接源码关联。
其完整选定审阅确立局部场景/动作/文本/使用方边界；公开汇总不证明整个场景或运行时成功。

| 已审阅证据表面 | 支持内容与停止点 |
| --- | --- |
| 完整已接受魔法门研究及前篇指南 | 全部 43 命令、三个内联/一个自定义体、六个文本/控制符关联、完整 42 操作动作及直接使用方、复制/请求/准入顺序。不声称自然或可见完成。 |
| `sf2-map-script-engine-static-v1`，[fixture](../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | programSummary 和选定过渡、踩踏、摄像机、动作、动画及闪烁事实。公开 fixture 含汇总/命令族事实；完整选定关联仍在研究中，不假设公开完整脚本/文本语料。 |
| `sf2-map-events-static-v1`，[zone shard](../../../../tests/fixtures/h2/map-events/zone-programs.json)，Map33_ZoneEvent1 | 源码行 53,54,55,58 的四个操作：F783 检查、已设置分支返回、cs_5AF36 调用、返回。无 F774 前提或局部标志写入。 |
| [Map-init fixture](../../../../tests/fixtures/h2/map-init-static-v1.json) 和 [setup fixture](../../../../tests/fixtures/h2/map-setup-static-v1.json) | 共享实体群/init/选择器上下文；两者汇总均不确立此场景完整内容、传入角色群或目的地抵达。 |

已审阅 schema 选项为 [script-engine fixture](../../../../schemas/h2/map-script-engine-fixture.schema.json)、
[zone programs](../../../../schemas/h2/map-events-zone-programs.schema.json) 及其引用事实形状。
[脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)比较溯源/地址和选定语义字段；
[事件验证器](../../../../src/sf2tool/h2/map_events.py)比较完整组装 fixture 语义。
[Map-init](../../../../src/sf2tool/h2/map_init.py) 和 [setup](../../../../src/sf2tool/h2/map_setup.py)负责各自独立 fixture/schema 检查。
这些是既有验证边界，不是每句话已独立执行的证据。
[研究复现路线](../../../research/creed-magic-door-and-desktop-transfer.md#counterexamples-and-reproduction)负责保留相等比较及选定直接源码检查。
本综合不重跑/重新生成它们。

保留 [#896 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173732542)、
[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173857962)
和 [PR897 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/897#issuecomment-6173871815)：
不同原始/变基后 head/tree/base 及 CI 收据、未改变的 18,285,979 字节对/0.079 秒比较、完整选定关联及检查失败。
保留前篇 [#895 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/895#issuecomment-6173738883)
和 [PR898 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/898#issuecomment-6173819074)，
包括各自负责的比较、CI 及链接的早期失败。交接定位检查；跟踪所有者及精确已接受 Git 对象确立事实。
本指南的直接检查、已配置提交 planner 及实际最终提交 CI 属于 Issue #899。

| 改变的传入状态 | 静态区分条件 |
| --- | --- |
| F783 已设置，选定 Zone1 | 回调不产生新场景/文本/复制/传送；包装器关闭/等待仍适用。 |
| F783 未设置，F774 独立未设置/已设置 | 相同局部选择；先前行程、配置及位置可能不同。 |
| 缺失/重复映射或改变 HP/职业 | 不同/别名行动者或跳过精灵工作；逻辑名称不保证三名可见参与者。 |
| 等待 60 后缩小未完成/被替换 | 场景可在三者未全部空闲时继续；128 动作等待和 physical 0 坐标等待测试不同事项。 |
| 服务停滞或待处理请求被替换/不完整 | 先前变化保留；最终传送可能从未到达或未按所写内容使用。 |
| F422/F522 改变 | 遵循独立准入表，包括区域外通配准入及先选择、后按完成状态清除。 |

这些示例不是已执行场景。**未知：**自然触发可达；实际配置、标志、出战队伍、HP/职业、查找、来源位置、模式及待处理状态；
服务/并发动作完成；渲染缩小、影子及拾取；实际目的地准入/控制及持久性。
依 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
和 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)，这些不创建自动运行时队列，
也不完成战役或[设计 epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
