# Harpy Pond 离开至 Fairy Cave 入场

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

离开 Harpy Pond 请求返回 Devil's Tail。附近独立区域可调度 Elric 打开通道：他解释 仙女 告诉他如何移动岩石，庆祝成功并呼唤领队向前。场景改变当前地图，隐藏新创建的 Elric 行动者并请求 Fairy Cave。军团成员资格、通道标志、当前地图改变及洞穴入场完成是独立状态；达到其中一个不能确立其余全部状态。

**已确认：**已接受静态程序及其使用方确立下述条件顺序。**推断：**Elric 台词与图块改变表现打开通道。**未知：**自然旅行、传入行动者／出战队伍／运输状态、服务完成、渲染表现、响应输入及存档持久性。对话为归属台词，不证明此前与 仙女 相遇。

本指南接续 [Battle20 结果与 Elric 招募](battle20-outcomes-and-elric-recruitment.md)，使用已接受的[离开与洞穴入场研究](../../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md)。止于条件 Map32 入场控制、无关待处理继续，或开场前 Battle21 准入。独立范围的局部内容与返回见 [Fairy Cave 探索与 Devil's Tail 返回（中文）](fairy-cave-exploration-and-devils-tail-return.md)。洞穴探索、宝箱、搜索、岩石检查对话、后续移动、Creed 内容及战斗开场／结果均不在本指南范围内。Fnnn 表示标志 nnn；记录及行索引从零开始。坐标、等待及声音 ID 是源码操作数，不是观察位置、实测时长或可听结果。

## 离开池塘独立于警告

**已确认：**Map51 唯一外部传送行匹配 Y25 处任意 X，请求无滚动返回 Map67（Devil's Tail）`(57,26)`、Down。该表不检查 F470 或 F520。独立区域事件 `(9,24)` 和 `(10,24)` 在 F470 未设置时运行已接受出口警告，已设置时绕过。警告不是 Y25 出口行的谓词。警告有效时移动是否自然到达该行仍为**未知**。

普通移动在外部表查找前选择工作布局分支：`$1000` 到达 `WarpIfSetAtPoint`，`$1400` 则以字坐标提交 zone event 6。外部查找按有序行扫描，将负字节坐标视为通配；表匹配不确立地形可达或玩家输入。载具生产方具有独立条件。

外部写入方发布 event 1、parameters 1–6 及 WARP_SFX。`ProcessMapEvent` 在分发前清除待处理状态。无滚动使用方提交并清除 WARP_SFX，使 physical 0 空闲，携 `(67,57,26,Down,d4=0)` 返回 MainLoop。它将玩家当前坐标与非负请求 X/Y 组合；不规范化 PLAYER_TYPE。MainLoop 在全新探索**之前**运行 SwitchMap 和 CheckBattle。

### Battle21 可在 Map67 探索前被选择

**已确认：**完整标志切换地图表无 Map67 或 Map32 替换。完整 45 行战斗表有一个 Map67 行：slot 21、区域 `(21,0)`、大小 32x42、触发 X/Y 255。传入 F421 无论区域包含均接纳 Battle21；抵达 X 57 在矩形外不会拒绝它。F521 已设置在选择后清除 F421，但不取消此次准入。池塘出口及通道场景均不创建 F421/F521。若传入状态选择该分支，则止于开场前。F421 未设置允许下述全新探索分支。

### 全新返回重建角色配置

**已确认：**全新 ExplorationLoop 清除待处理状态，从有符号字踩踏计数器减去 20000，对负结果钳位到 0，治疗符合条件的存活／不死己方角色，并执行淡入淡出／资源工作。它设置 CURRENT_MAP=67 和 CURRENT_BATTLE=255，重建实体及精灵，清除临时标志 256–383，设置 F80，加载地图／视野、安装基础 VInts 并调用地图 init。F13、F470、F520、F751 和随从选择器不在临时重置范围。

Map67 有一张配置指针表、空固定实体列表及 `0x4FD4A` 处仅返回的 init。共享实体生成仍清除旧实体／查找，重建按标志选定的随从，在抵达元组声明 Bowie physical 0，终止随从列表并运行木筏尾部。空固定列表不表示空实体群。

Map67 是大地图。随从选择器中仅有 F65/F67/F68 时，physical 1/2/3 为 Caravan/A、Peter 和 Oddler/B。F66 添加 Sarah 和 Chester；其他所选行保留表顺序。F74 在 F68 的 Oddler 行之前声明 Astral 为 B；F70 可再次覆盖 B。F74/F77/F83 可覆盖 A。完整随从表无 Elric 行。出战或后备成员资格不声明探索随从，前一池塘场景的临时行动者／跟随被全新实体生成丢弃。

PLAYER_TYPE 与已保存木筏状态仍为传入值。Mode 2 给 physical 0 木筏精灵，将当前随从停放在 `(64,64)` 并安装站立动作，在那里声明独立 raft physical 31。其他模式仅在 F64 且已保存 RAFT_MAP=67 时以掩码后的已保存坐标声明该木筏。Mode 1 不重复登乘。实体生成尾部既不修复任意模式，也不改变已保存木筏位置。之后音乐／淡入淡出及待处理为零的 WaitForEvent 可安装当前模式控制器。抵达 `(57,26)` 本身不提交其区域回调。

## Elric 通道有独立准入检查

**已确认：**有序 Map67 区域表选择三个独立分支：

| 提交区域 | 选择 |
| --- | --- |
| `(57,25)`、row 0 | F751 已设置则立即返回。否则 F13 未设置或 Elric HP 0 则返回。已加入且存活的 Elric 运行 `cs_4FC32`；仅脚本返回后设置 F751。 |
| `(56,26)`、`(57,26)`、`(58,24)`、`(58,25)`、rows 1–4 | F520 已设置则返回。未设置则运行完整已接受求救场景，使用当前 A/B/Peter 查找、text 2543–2549、移动／重定位及最终 Map51 请求。 |
| 其他坐标 | 默认调用 Battle21 的 CheckRandomBattle；保留已接受踩踏／随机／完成／请求限制，止于选定继续。 |

通道回调检查 ally 13 当前 HP 字是否为零。它不检查出战 F45、F520、F470、职业、运输、地形、随从或 仙女 对话。已加入后备 Elric 的 HP 非零即可符合条件。缺失成员资格和 HP 0 不获修复。F751 已设置绕过后续检查，不重放场景，也不立即重新应用当前图块复制。

通道谓词不排除 F520 未设置／F13 已设置／HP 非零组合。F520 控制不同回调，Elric 感谢 仙女 不创建额外历史前提。这些传入状态组合约束解释；其自然可达为**未知**。

步行区域生产方及 type 6 使用方独立于外部传送路径。Type 6 在 physical 0 安装 Init，并将两个字坐标传给 `RunMapSetupZoneEvent`。包装器选择有序坐标行或默认项、调用它、关闭立绘／文本、等待一次 VInt 并等待 physical 0 当前位置等于目的地。提交区域坐标不确立场景中 Bowie 的精确起始位置。

## 完整通道场景请求什么

**已确认：**`cs_4FC32`（`0x4FC32`） 有 33 个顶层记录及四个完整内联 Up 1/endActions 程序体。下列顺序包括每个选定记录。Bowie=0、Peter=7、Elric=13、logical FOLLOWER_B=31；B 实际角色取决于传入查找。

| 记录 | 有序台词与动作 |
| --- | --- |
| 0–3 | 设置文本游标 2593；在 `(57,25)`、Up 创建 Elric，精灵操作数 255；安装／等待其 Init；请求 Up 1/endActions 并等待空闲。 |
| 4–7 | 依次使 Bowie、Peter 和 B 面朝 Up。Elric 说 仙女 告诉他如何移动岩石（2593）。 |
| 8–14 | 等待 30；提交 SECRET_PATH_ROCK (96)；等待 30；使 Elric 面朝 Down；分两次安装／等待 Jump。Elric 宣告成功并感谢 仙女（2594）。 |
| 15–21 | 设置 quake 1；提交 BIG_DOOR_RUMBLE (93)；等待 20；使 Elric 面朝 Up；等待 20；提交 DESOUL_HOVERING (110)；从 `(0,0)` 向 `(56,22)` 复制 3x2 图块矩形。 |
| 22–26 | 等待 30；设置 quake 0；等待 30；使 Elric 面朝 Down。他呼唤领队向前（2595）。 |
| 27–32 | Elric Up 1/endActions，带空闲等待；hide 13；Bowie Up 1/endActions，带空闲等待；Bowie Up 1/endActions，不带空闲等待；请求 Map32 `(29,3)`、Left；结束。 |

字面文本连接为 2593/`0A21`、2594/`0A22`、2595/`0A23`。每条 single-text 命令使用 modifier 0 及 Elric 当前映射立绘／台词解析器。Text 2593 含一个 `{N}` 后接 `{W1}`；2594 含 `{W1}`；2595 含 `{LEADER}` 和 `{W1}`。领队替换使用当前军团出战领队，不是无条件 Bowie 字面值。Modifier 0 既不设置右侧位，也不设置镜像位。程序中无选择／拒绝、加入、治疗、职业／状态重置或持久随从选择。

### 新行动者不是新军团成员

**已确认：**`newEntity` 提供 Init，并为 ally 13 调用 InitializeNewEntity。精灵操作数 255 被 GetAllyMapsprite 用当前 HP／职业和 row 13=ELRIC_SPECIAL (32) 替换：ACHR (5) 选择 30，SNIP (21) 选择 31，BRGN (22) 选择 32。探索中 HP 0 会选择蓝色火焰，但回调更早 HP 检查通常排除它。分配既不写 HP，也不写职业。

分配器从零开始用有符号字节比较扫描 lookup bytes 0–62，选择 maximum+1 并覆盖 lookup 13。它既不检查已有 Elric，也不复用旧行动者；不检查成员资格或容量。Lookup 63 木筏不在扫描中，负字节不增加最大值。传入重复者可保留旧物理行动者，而后续命令指向新映射。这不同于固定实体生成的重复检查。

声明按 tile×384 写入当前／目的地坐标、朝向、精灵、层／动作、初始标志和零运动／加速度；随后更新属性。它不将 Elric 添加至 EXPLORATION_ENTITIES，也不设置 F13/F45。全新 Map67 仅 F65/F67/F68 时，新 Elric 为 physical 4，Bowie/Peter/B 解析为 0/2/3。无 F65、仅 F67/F68 时，他为 physical 3，Peter/B 为 1/2。额外标志、保留的滚动实体群及无效映射可改变示例。清除的 lookup 0 可别名到 physical 0；255 未被验证为安全缺失。称作 B 不独立证明 Oddler 存在。

Hide 解析新映射 Elric，将当前／目的地停放在原始 `$70007000`，将所有指向该物理行动者的查找别名改为 255，若在随从列表中则移除。不清除 F13、不移除成员资格、不治疗 Elric，也不删除另一个旧重复者。新行动者从未被自动添加至随从列表。

### 动作与表现保留各自完成限制

**已确认：**Init 设置速度 32/32、零加速因子、禁用加速／减速、启用阻挡／碰撞／自动朝向、转向 Up、禁用浸没、设置大小 24、刷新并空闲。`setActscriptWait` 轮询精确 Idle 动作指针。Elric 显式 Init 等待先于首次移动。

Facing 跳过 HP 0 的 allies 0–29，B (31) 则绕过检查；因此 Bowie 和 Peter 转向取决于各自 HP 及映射。Init、内联动作、文本及 hide 不继承此 HP 门槛。内联动作清除 flags 5/6，将 Up 1 编译为当前行动者上方一个瓦片（384 内部单位）的相对目的地、目的地等待及 Idle 跳转。Flag 5 未设置时绕过相同目的地扫描。不调用步行传送或区域生产方。在行动者一致且移动完成时，Elric 首次 Up 1 到达 `(57,24)`。后续 Up 1 到达 `(57,23)` 的条件是中间动作使他仍处于 `(57,24)`，且该移动在 hide 前完成。Bowie 移动相对于传入位置；场景从未显式放置他。

Jump 禁用自动朝向，请求相对 `(0,-1)`、等待 5、请求 `(0,1)`、等待 5、请求 `(0,0)`，恢复自动朝向并跳至 Idle。这些定时请求不是三个目的地屏障，也不证明精确返回瓦片。场景等待每个 Jump 的 Idle 指针，不等待观察轨迹。

Single text 等待视野滚动、解析立绘／台词、显示、递增游标、关闭窗口并睡眠 10。ExecuteMapScript 在进入时清除 debug 跳过；debug 配合 player 2 Start 可设置它，跳过这些显示／游标递增及 csWait 睡眠。动作等待保留。声音无完成屏障。Quake 操作数 1/0 直接写 QUAKE_AMPLITUDE；滚动使用方以非零幅度产生抖动。这些调用均不证明渲染震动、可听声音、已确认台词或每个启用服务完成。

## 当前图块、F751 与洞穴请求相互独立

**已确认：**`setBlocks` 调用 CopyMapBlocks，将包括标志的六个布局字从 3x2 源矩形复制到当前宽度 64 的 RAM 布局 `(56..58,22..23)`（行跨度 128 字节），再请求两个视野平面更新。它不写 F751，单独也不证明可通行或表现。

Map67 唯一标志图块行含相同矩形，由 F751 控制。后续 LoadMapBlocksAndLayout 加载基础地图、扫描标志行，并为已设置标志重新应用复制。此加载器独立于配置选择及 Map67 仅返回 init。在地图已经加载后设置 F751 本身不执行加载器；跳过区域不修复当前图块。

最终 `warp` 写 event 1 和 parameters 1–5 为 `(no-scroll 0,map 32,X 29,Y 3,Left)`。它既不分发地图改变，也不设置 WARP_SFX 或改写 parameter 6。脚本结束在对话窗口索引非零时有条件等待视野滚动，清除摄像机滚动速度并恢复寄存器。它不是所有行动者屏障。脚本返回后，区域回调设置 F751；包装器随后关闭窗口、等待一次 VInt，并检查 physical 0 的位置／目的地相等。

最后 Bowie 动作为异步，洞穴请求在包装器完成前发布。Physical 0 检查既不是最终动作指针等待，也不保证传入映射或未完成服务下两次向上移动均完成。包装器停住可使 F751 已设置、请求待处理，却未到达洞穴消费。

若请求保留至探索恢复，WaitForEvent 返回它，不重新安装控制器。ProcessMapEvent 清除待处理状态；无滚动消费提交／清除传入 WARP_SFX，使 0 空闲并返回 `(32,29,3,Left,d4=0)`。显式洞穴 X/Y 不依赖 Bowie 最后移动了多远。之后生产方可替换单一待处理槽位；消费跟随实际请求。无队列或事务回滚。

| 传入或部分状态 | 支持的后果 |
| --- | --- |
| F751 已设置 | 无场景／生成／当前复制／洞穴请求；后续地图加载独立重新应用标志图块。 |
| F751 未设置，F13 未设置或 Elric HP 0 | 无场景且不修复；仅 Battle20 完成不足。 |
| F751 未设置、已加入且存活 Elric，包括后备与 F520 未设置组合 | 无 仙女 历史检查地接纳通道；后续服务仍有条件。 |
| 图块复制前服务停止 | 可能已有先前行动者／文本／动作改变；不确立后续图块／请求／F751。 |
| 复制完成，但脚本未返回 | 当前图块可已改变，而 F751 仍未设置；quake 重置、hide 和请求取决于停止点。 |
| 脚本已返回，但包装器／消费未完成 | F751 可先于洞穴加载／控制设置；运动、请求保留及消费仍独立。 |
| 重复 Elric、缺失 Peter/B 或冲突随从标志 | 通道准入不规范化角色配置，也不验证每个查找。 |

## 独立洞穴入口与条件抵达

**已确认：**Map67 完整外部表依次为 Creed、洞穴、洞穴、池塘及东边界滚动。选定洞穴行为 row 1 `(51,22)` → Map32 `(2,3)`、Right，及 row 2 `(57,23)` → Map32 `(29,3)`、Left，均无滚动。这些精确坐标行无 F751/F13/HP/F520 谓词；移动／布局生产方条件仍适用。Row 2 共享场景目的地，但为独立请求生产方。对侧入口不证明向西路线。在 `(57,25)`，外部表返回池塘，区域表则选择通道；工作布局分支区分两者，因此共享坐标不调用两者。

对于保留的脚本或选定外部洞穴请求，SwitchMap 不改变 32；即使传入 F421，CheckBattle 在完整战斗表任何位置也找不到 Map32 行。此次洞穴入场不接纳 Battle21。

Map32 实际配置回退为 `ms_Void: dc.w $FFFF`。完整 MapSetups 表从 31 跳至33，后续也无 32 行；GetCurrentMapSetup 到达终止符并返回该哨兵值。GetMapSetupEntityList 原样返回，共享实体生成跳过固定行，init 分发器不调用回调而返回。这是表／使用方证据，不是从猜测文件名缺失或替代空剧情脚本推断。

全新探索重复待处理清除、踩踏调整、符合条件治疗、淡入淡出／资源、实体／精灵重建、临时标志重置、F80 及地图加载顺序，写入 CURRENT_MAP=32 和 CURRENT_BATTLE=255。临时通道 Elric 及此前任何仅地图跟随被丢弃。F13/F45/F751 不在重置范围；Map32 标志图块列表为空。

Map32 不是大地图，因此 F65 不前置 Caravan。其他所选随从行仍适用，包括 Peter/F67 与 B/Oddler/F68 及可能的查找覆盖。仅这两个标志时，Peter/B 为 physical 1/2；Elric 仍无随从行。木筏尾部保留传入模式／F64／已保存 RAFT_MAP=32 规则。通道及洞穴配置均不登乘、下船或修复运输模式。场景／右侧入口在 `(29,3)`、Left 声明 Bowie physical 0；另一入口为 `(2,3)`、Right。

仅返回回退后，探索有条件提交音乐／淡入淡出。Pending 0 使 WaitForEvent 跟随 physical 0 并安装控制器：mode 0 步行、mode 1 源码名 `eas_Raft`（实际 Caravan 控制）、其他模式 `eas_Caravan`（实际木筏控制）。无关待处理请求优先。控制器安装是条件入场边界，不是可用输入观察，也不支持任意传入模式。

Map32 外部出口恰位于这些抵达坐标：`(2,3)` 请求 Map67 `(51,23)`、Down，`(29,3)` 请求 Map67 `(57,24)`、Down。全新实体生成／init 与 WaitForEvent 不调用外部传送生产方。步行首先需要获准方向移动候选及相应布局分支；载具生产方保持独立。仅坐标重叠不证明立即弹回。本指南止于任何新洞穴移动、出口选择或局部内容之前。

## 证据、审阅与剩余问题

**已确认溯源：**原版源码为 [SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，由研究所有者连接到 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[研究复现／审阅边界](../../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#reproduction-and-remaining-limits)负责源码路径、命令、全部 33 个行／宏／参数连接、四个内联体、字面文本及紧接使用方。本综合使用已接受结果，不重新调查原始源码、不重新生成载荷，也不开展新运行时采集。

| 审阅表面 | 支持内容及保留问题 |
| --- | --- |
| 已接受离开研究、完整场景说明及[请求消费](../../../../../research/world/map-event-request-consumption.md)／[写入](../../../../../research/world/map-event-request-state.md) | 条件回调、当前／标志／加载顺序、待处理保留／替换、元组返回及实际 Map32 回退。自然生产方选择／完成仍未知。 |
| `sf2-map-script-engine-static-v1`（[公开测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)）、保留的 `local/derived/map-script-engine-static.json` 中选定 `programCorpus.programs` id=`cs_4FC32` | 全部 33 个有序保留记录均已对照已接受研究检查。仅公开摘要／类别事实不暴露完整场景，也不证明自然执行；仍需研究所有者的内联／文本／使用方连接。 |
| `sf2-map-init-static-v1`（[公开测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)）及保留的 map-init 载荷 | 配置／分发器溯源及保留源码形态剖面；已接受完整 MapSetups／随从／大地图／战斗及选定表审阅提供 Map32 缺失／回退与角色配置结论。仅聚合相等不证明它们。 |
| [地图探索](../../../contracts/world/map-exploration.md)、[对话](../../../contracts/interface/dialogue-system.md)、[出战队伍／名册](../../../contracts/party/party-roster-state.md)、[地图脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[测试夹具 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)及[输出 schema](../../../../../../schemas/h2/map-script-engine-output.schema.json) | 共享有序状态／命令／溯源边界、封闭的公开与完整程序结构，以及行动者与成员资格之别。不将受控边界扩展成战役或已完成表现。 |

综合前审阅未发现需要新原版结论的所有者冲突。全文保留严格静态边界。现有配置命令及保留比较归[研究复现路线](../../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#reproduction-and-remaining-limits)；其已接受 PASS 不确立自然旅行，也不独立确立每条文字结论。保留[原始回执](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876#issuecomment-6171704588)、[变基后回执](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876#issuecomment-6171750883)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/878#issuecomment-6171765502)，包括检查／引号／路径失败、修正及不同的原始／最终 CI 结果。它们是审阅记录；跟踪的研究与精确已接受 Git 对象负责结论。

**未知：**自然路线／地形／生产方选择、实际标志／成员资格／HP／职业／查找／模式／木筏、动作／文本／声音／淡入淡出／滚动服务完成、可见通道、可用输入及持久性。后备／死亡／缺失／重复行动者、传入标志、debug 跳过、部分返回及待处理替换是静态区分用例，不是已执行缺陷。扩展需要另行接受证据；这些未知项不自动排队运行时工作。本指南既不完成战役，也不改变重制行为或验收。
