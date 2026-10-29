# Polca 木筏登乘与 South Parmecia 入场

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

登乘是 Luke 招募场景之后独立的玩家行动。符合条件的移动尝试可以请求登乘，但消费者仍必须在 Bowie 目的地正前方找到物理木筏。获准登乘会分阶段改变行动者与表现，最后提交木筏模式。在该模式下，Map72 选定的两个南出口都请求 South Parmecia，即 **Map69**，但通往探索或战斗准入的路径不同。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md)支持下文请求、行动者、控制器和选定入场后果。**推断：**这些操作实现登乘和水上旅行。**未知：**自然接触与通行、实际传入状态、服务完成、可见登乘/抵达、响应输入与持久性。Fnnn 表示标志 nnn；除明确注明内部单位外，坐标均为地图瓦片。

前段条件 Map72 控制边界见 [Polca 出发与 Luke 招募](../eastern-parmecia/polca-departure-through-luke-recruitment.md)，配置的 Map72 `(43,48)` 元组见[木筏授予](../eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md)。本指南止于数值 type5 下筏请求、初始化前获准的战斗，或条件 Map69 探索控制。不增加重制能力、连续可玩性声明或后续剧情/交战说明。

接续 [Kraken 现身与 Battle16 进入](south-parmecia-through-kraken-battle16-entry.md)，了解选定 Map69 row0→Map49 路线、条件准入、完整遭遇，以及重新构建战斗员直至空 start 包装器的过程。该独立指南保留本段自然旅行、传入状态与完成限制；不增加 Battle14/15 前提。

## 请求登乘尚不等于登乘

**已确认生产者：**普通角色移动根据当前边界考虑方向输入，在读取拟移动点的工作布局前检查行动者阻挡。当掩码后布局为 `$3C00`、当前无战斗（`$FF`）且 F64 已设时，写入待处理 type3 登乘请求。该分支可以改变朝向，但不推进目的地/行程，也不递减步数计数器。阻挡行动者可能使执行无法到达该处。扫描使用受控行动者的 bit5 和潜在阻挡者的 bit7，同时考虑其位置与目的地。木筏的站立行动在运行时清除自身 obstructable 位。

该生产者既不检查已保存木筏元组或物理木筏坐标，也不检查 Luke 成员资格、HP、F716、F718、F10、F75 或 F416。因此，授予时的 F64 与叙事位置不证明接触。Luke 被等待的 Down1 后隐藏属于 Luke 自身行动；它不让 Bowie 登乘，也不发出 type3。门、传送与 zone 标记各自保留独立生产分支。

**已确认消费者门控：**`ProcessMapEvent` 在把 type3 分发至 `MapEventType3` 前清除待处理状态。其对齐辅助读取物理 main0 的目的地与朝向，在该方向加一瓦片（384 内部单位），再与**物理31 的目的地**比较。只有精确匹配朝向相邻目的地才准入登乘主体。当前位置重合、看似木筏的精灵或逻辑随从映射都不足够。辅助不复查 F64、当前战斗或 `RAFT_MAP`；传入的对齐状态可以在缺少这些生产者门控时通过。遭拒请求仍已被消费，玩家模式不变。

待处理状态是可变槽位，不是队列或原子事务。插入服务后哪次写入保留仍未知。[请求与对齐所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#movement-request-and-the-separate-alignment-gate)保留精确生产者与目的地偏移消费者。

## 木筏模式提交前的变化

**已确认获准顺序：**令物理0 空闲，清除精灵链接排序字节 `byte_FFAFB0`，运行含四条记录的 `cs_453F2`，向当前探索列表安装跟随行动，经列表辅助等待，最后写入 `PLAYER_TYPE=2`。安装空闲不清除先前行程/速度字段。清除排序字节不是 HP、成员资格或模式变化。

| 阶段 | 请求行动与完成边界 |
| --- | --- |
| Bowie 靠近 | 第一个被等待的 Bowie 行动请求移动到物理31、相对距离0，等待6，请求传送 SFX，等待自身位置等于目的地，复制物理31 的位置/目的地/行程/朝向，随后空闲。脚本调用方另行等待映射后 Bowie 的行动指针变为 `eas_Idle`。 |
| 独立木筏移至地图外 | `setPos FOLLOWER_D,64,64,DOWN` 经 lookup63 解析逻辑159，把映射行动者的位置和目的地设为内部 `$6000,$6000`、朝 Down，并刷新精灵。这是位置命令，不是移除。 |
| Bowie 采用木筏表现 | 第二个被等待的 Bowie 行动设置木筏地图精灵、禁用缩放、请求精灵刷新并空闲。其调用方再次等待该映射行动者的空闲指针。 |
| 场景返回 | 第四条记录终止，包括条件视野滚动等待与滚动速度清除。在列表行动和模式写入之前返回登乘消费者。 |
| 列表行动者汇合并迁移 | `byte_45434` 请求朝物理0 移动，然后等待6/传送 SFX/自身目的地等待/复制物理31/空闲。在正常映射下物理31 此时位于地图外；行动者复制其状态，但保留各自精灵。 |
| 木筏模式提交 | 列表等待按实际谓词返回，随后写入 `PLAYERTYPE_RAFT=2`。此前精灵与位置效果已经请求。 |

两个被等待的 Bowie 命令均解析 lookup0。两次等待都没有逐行动者 HP 守卫。FOLLOWER_D 位置命令走存活状态辅助的非己方角色分支，因此 Bowie/Peter/Luke 的零 HP 不阻止该迁移。这些命令都不治疗、加入、清除随从标志、调用 `HideEntity` 或改变地图。独立行动脚本与[完整登乘所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#complete-boarding-and-actor-identity)保留所请求服务与完成限制。

三种身份必须区分：

| 身份 | 本边界的含义 |
| --- | --- |
| 物理31（`ENTITY_RAFT`） | 对齐、移向实体和复制位置行动的直接目标。这些行动使用物理索引，不经逻辑查找。 |
| 逻辑159（`FOLLOWER_D`）→ lookup63 | 地图外位置命令的目标；正常木筏声明将其映射至物理31。映射缺失或别名可能改为影响 main0 或其他行动者，而直接复制仍以物理31 为目标。 |
| 逻辑31（`FOLLOWER_B`）→ lookup31 | 前次实体群中的 Oddler 或其他标志选定随从。其逻辑编号与物理31 相同不代表它是木筏。 |

**已确认列表语义：**尽管辅助名为 `ApplyActscriptToHeroAndFollowers`，重新构建实体群时仍单独声明 main0，从物理1 起将获准随从追加至 `EXPLORATION_ENTITIES`，再写终止符。main0 不被追加。`FOLLOWERS_LIST` 从后一字节开始；不是另一个重建状态权威。行动安装器遍历当前列表中的实际物理字节，不按成员资格或标志重建行动者。因此正常重建状态下作用于随从；传入或保留的列表可能不同。

空列表使 `WaitForHeroAndFollowersStopped` 立即返回。否则扫描到终止符后，它在 VInt 后**仅轮询列表中最后一个物理行动者**的指针是否为 `eas_Idle`，不会汇合每个随从的完成状态。mode2 写入时，更早列出的行动者可能仍忙碌。两次 Bowie 等待与末行动者列表等待是独立谓词。此处不修复畸形/变化列表、缺失行动者或停滞服务。

**已确认部分状态后果：**中断可能使 Bowie 已有木筏表现或随从已迁移，而 `PLAYER_TYPE` 仍为0。登乘保留传入的 `RAFT_MAP/X/Y`，不更新已保存元组。这些有序效果不建立存档历史或交通持久性；到达最终模式写入不证明所有随从或表现均已完成。

## 木筏控制使用实际数值分发

**已确认选择器：**待处理值为零时，`WaitForEvent` 重新以物理0 为目标，安装由玩家类型选定的行动。固定源码名称与木筏模式含义不一致：

| 玩家类型 | 安装的源码脚本 | 实际控制器 |
| --- | --- | --- |
| 0 | `eas_ControlledCharacter` | `ac_controlCharacter`，opcode2 |
| 1（`PLAYERTYPE_CARAVAN`） | `eas_Raft` | `ac_controlRaft`，opcode7 |
| 2（`PLAYERTYPE_RAFT`），或其他非零且非1 的传入值 | `eas_Caravan` | `ac_controlCaravan`，**opcode8** |

登乘提交的 mode2 消费者为 `esc08_controlCaravan`。不能因标签写着“Raft”就改用 opcode7。同样，**数值 type5** 的符号名为 `MAP_EVENT_GETOUTOFCARAVAN`，分发到下筏消费者 `MapEventType5`。另一个名称 `MAP_EVENT_GETOUTOFRAFT` 对应 type4。本指南止于 type5 请求，不消费它。

**已确认输入与阻挡边界：**安装的 mode2 脚本设置 speed32 及其加速/减速参数，随后循环图块复制检查、opcode8 与目的地等待。Opcode8 按 Up、Down、Left、Right 顺序根据未收缩的 layer1 边界考虑输入。符合条件的轴可以组合；后续检查决定最终朝向。没有符合条件的方向时直接推进，不请求移动。尽管宏名含“Uncollidable”，脚本的 FLAGS_A bits7/6/5 不禁用 bit5 碰撞分支。其扫描仅考虑其他 bit7 行动者的**当前位置**，要求两轴差均小于 `$100`；普通移动还考虑其目的地。阻挡会跳过移动。

扫描后由工作布局决定下一行动：

| 按顺序测试 | 后果 |
| --- | --- |
| 掩码后 `$0400` | 请求开门，再读取/掩码布局字。 |
| 掩码后 `$1000` | 调用 `WarpIfSetAtPoint`，即使无行匹配也走移动尾部。 |
| 掩码后 `$3C00` | 走移动尾部。 |
| 否则，原始字按无符号解释小于 `$C000` | 写入数值 type5 并跳过移动：下筏请求这一停止分支。 |
| 否则，原始字至少为 `$C000` | 跳过移动，不发出该请求。 |

移动尾部写入选定速度，为每个移动轴设置 384 行程，并在精灵更新前按选定瓦片偏移推进目的地。它不增加 F64/当前战斗/解锁门控，不产生 type6 zone，也不递减步数计数器。不在淡入淡出时，图块复制标记 `$0800/$0C00` 可以影响工作布局。这些测试和对话均不提供已解码水路、无碰撞旅行或输入回执。[控制器所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#installed-raft-controller-and-stopping-alternatives)保留脚本与布局顺序，不推广为 Caravan 操作。

## 两个南出口与同一正确目的地

**已确认选定内容：**Map72 有序传送表包含以下南向与竞争边缘行。行号从零开始。选择目的地数值对应的配置、init 或战斗所有者之前，必须先解析每个源码符号。

| 行 | 匹配 `(X,Y)` | 源码目的地 → 编号 | 选定请求 |
| --- | --- | --- | --- |
| 6 | `(40,62)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → **69** | 无滚动至 `(41,16)`，Down。 |
| 7 | `(0,255)` | `MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` → 73 | 向 Left 滚动；在 `(0,63)` 先于 row9 胜出。 |
| 8 | `(63,255)` | `MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT` → 71 | 向 Right 滚动；在 `(63,63)` 先于 row9 胜出。 |
| 9 | `(255,63)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → **69** | 向 Down 滚动，元组 `(255,1)`，Down。 |

South Parmecia69 不同于 `MAP_OVERWORLD_SOUTHEAST_PARMECIA=70`。负坐标字节为通配符；首匹配胜出。Rows0–5 不匹配选定点，rows6/9 不重叠。在 Y63 的其他 X，只要 opcode8 到达 `$1000` 布局调用，row9 就可能匹配。传送匹配先将拟移动内部坐标按有符号除法除以384，再写入 type1、其元组和传送 SFX；不要求木筏模式或剧情标志。

Map72 与 Map69 声明 layer1 边界 `(0,0)..(63,63)`。Opcode8 使用这些未收缩边界，因此在输入、占用与布局条件满足时，选定点与南边缘兼容该生产者。普通角色控制在 `d7==0` 分支收缩一瓦片；`d7` 是剩余实体扫描计数器，不是固定物理0 身份。这种兼容性不证明可自然接近选定行。[出口所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#selected-map72-exits-and-producer-compatibility)保留符号到编号关联与优先级。

## 两个出口为何产生不同抵达后果

**已确认 type1 消费：**两个分支都先清除待处理状态，均保留 `PLAYER_TYPE` 与已保存木筏元组；服务完成和中途参数写入仍未知。[入场生命周期所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#no-scroll-admission-versus-scrolling-continuation)区分下列调用。

**Row6，无滚动：**消费者请求/清除传送 SFX，令主角空闲，退出探索调用层，并将 Map69 `(41,16)`、Down 返回原始 `MainLoop/@Start`。`SwitchMap` 没有 Map69 替换。随后 `CheckBattle` 按顺序扫描这些实际 Map69 行：

| 战斗行 | 区域输出 `(X,Y,width,height)` | 触发 | 解锁 / 完成 |
| --- | --- | --- | --- |
| 9 | `(0,0,48,32)` | `(255,255)` | F409 / F509 |
| 17 | `(0,31,30,32)` | `(255,255)` | F417 / F517 |

地图、解锁与触发决定准入。通配触发允许任一位置；矩形是输出，不是包含测试，因此 `(41,16)` 不排除任何一行。两解锁均设时 F409 胜出。完成标志已设会清除选中解锁，**但本次调用仍准入该选定战斗**，不会继续下一行。两解锁均清除则进入普通探索。Luke 的 F416 解锁 Map49 的 Battle16；单凭它不证明立即准入 Map69 战斗。不能根据假定剧情历史重建传入的完成/解锁标志。止于初始化或交战前的准入；后续战斗返回另有入场路径。

**Row9，滚动：**消费者令主角空闲，设置 `CURRENT_MAP=69` 并调用 Down 地图过渡。Map71/F530 特殊替换不适用。服务加载瓦片、图块与工作布局并作条件复制，再通过 DMA/VInt 等待滚动/重绘。返回时负 X 保留主角当前 X，而 Y 位置/目的地变为384（tile1）。元组中的 Down 朝向**不被消费**；保留行动者当前朝向。处理器采样主角当前位置/朝向，调用 `DeclareRaftEntity`，返回同一探索循环内部。

本次选定调用不执行 `SwitchMap/CheckBattle`、实体群重建、临时标志清除、入场治疗、Map69 init 或新的地图音乐/淡入生命周期。F409/F417 不打断此滚动调用。在 mode2 下，`DeclareRaftEntity` 立即返回，保留当前映射、列表和隐藏行动者。将 row6 的战斗扫描或实体群重建套到 row9 会虚构调用。后续事件不在本指南范围内。

## 重新构建 Map69 实体群与条件控制

**已确认重新入场边界：**此处只有未准入战斗的 row6 才到达重新入场的 `ExplorationLoop(69,41,16,Down)`。它清除待处理状态、调整步数、治疗存活/不死己方角色、淡出、设置 Map69/不在战斗中、加载瓦片并初始化行动者/地图精灵。配置清除临时 F256–383，并在地图加载、基础 VInt 与 init 前设置 F80。Map69 仅有默认配置、**空固定实体列表**，以及 `ms_map69_InitFunction`（位于 `0x4FDE2`），其唯一指令为 `rts`。该主体不带来新剧情场景或依赖 F416 的 init。

固定实体群为空不代表没有行动者。Map69 是不在随从排除范围内的大地图。main0 与获准标志随从被重建：F65 可在前部加入 Caravan，F67 准入 Peter，F68 在冲突行限制下以 FOLLOWER_B31 提供 Oddler，F75 可重建 Luke，尽管前段当前实例已被隐藏。单凭成员资格不准入这些随从。[前段实体群边界](../../../../../research/chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)保留依赖 HP 的己方精灵、顺序与映射限制。没有固定 NPC 补救缺失随从。

**已确认保留 mode2：**重建时的木筏消费者给 main0 木筏精灵，把各列表随从移至 `$6000,$6000` 并安装站立行动。main0 不在该重建列表内，因此留在 `(41,16)`。另在 `(64,64)`、Left 声明独立物理31，使用木筏精灵与站立行动，并将 lookup63 设为31。此分支既不需要 F64，也不需要 `RAFT_MAP` 匹配，且不更新已保存元组。

传入非木筏模式走另一分支：普通独立木筏声明要求 F64 且 `CURRENT_MAP==RAFT_MAP`。仍保存 Map72 的元组不会在 Map69 声明木筏。滚动时不重建的 `DeclareRaftEntity` 又不同：在 mode2 之外可迁移列表随从；F64 已设且地图不匹配时清除 lookup63，并把物理31 移至 `$7000,$7000`。这些分支解释传入状态，不声称登乘自然导致下筏。

重新入场的无操作 init 后，仅当基础/当前调色板不同时才执行 `PlayMapMusic` 与 `FadeInFromBlack`。不存在无条件第二次音乐请求。下一次 `WaitForEvent` 中，待处理值为零会重新以物理0 为目标并安装所保留 mode2 的 opcode8 控制器；非零则延后控制配置，先处理事件。滚动以保留的实体群到达该条件配置，不经历重新入场生命周期。**止于条件控制。**两条路径本身都不证明抵达已渲染、输入响应或后续 zone/剧情行为。[实体群与控制所有者](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#ordinary-map69-population-and-conditional-control)保留重建与保留行动者的区别。

## 证据追踪与未解问题

**已确认溯源：**本综合使用 main 已接受的 PR818 研究，不是新源码调查。其所有者绑定固定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与已注册 USA 零售 ROM，列出精确生产者/行动/控制器/过渡源码主体，并提供[复现与覆盖限制](../../../../../research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#reproduction-and-coverage-limits)。[探索控制流合同](../../../contracts/world/exploration-control-flow.md)拥有顶层交接顺序；[地图入场路由合同](../../../contracts/world/map-entry-routing-state.md)拥有辅助局部选择，不拥有本内容特定路线或后续完成。

| 跟踪的测试夹具与精确 ID | 选定关系与限制 |
| --- | --- |
| [map-script-engine-static-v1.json](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1` | 已建立的放置/行动/复制处理器与语料身份。已接受研究比较了保留语料中全部四条 `cs_453F2` 记录。直接物理行动/列表/控制器主体是具名源码证据，不是组合夹具覆盖。 |
| [map-event-request-state-static-v1.json](../../../../../../tests/fixtures/h2/map-event-request-state-static-v1.json)，`sf2-map-event-request-state-static-v1` | 待处理与木筏状态写入；不保证特定请求保留或已保存状态持久。 |
| [map-event-request-consumption-static-v1.json](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1` | 待处理清除/分发与木筏消费者拓扑；不组合登乘、旅行与抵达。 |
| [map-setup-static-v1.json](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[map-entities-static-v1.json](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1` | 汇总配置/实体群边界。实际 Map69 默认/空固定列表由修正研究解析后的目的地选定；动态随从仍独立。 |
| [map-init-static-v1.json](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1` | `expected.primarySourceBodies` sourceOrder76、Map69 init 路径、所有者/地址与唯一 `rts`；没有登乘或输入完成断言。 |
| [battle-routing-data-static-v1.json](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)，`sf2-battle-routing-data-static-v1` | 汇总路由证据；实际 Map69 row9/17 操作数与调用方准入是具名直接研究证据，不是逐用例夹具覆盖。 |

综合前审阅检查了已接受研究、前段边界、辅助合同、这些有界跟踪夹具选项及其 schema/验证器。地图脚本夹具保留处理器/语料事实；map-init 夹具保留精确选定主体；请求夹具保留结构关系。其验证器比较各自拥有的投影，不是自然旅程。不得把汇总 PASS 描述为逐用例登乘/抵达覆盖。现有所有者命令为 `uv run sf2 h2 map-script-engine`、`map-event-request-state`、`map-event-request-consumption`、`map-setup`、`map-entities`、`map-init` 和 `battle-routing-data`，均使用同一 H2 前缀。本说明切片不重跑它们或生成载荷。研究配方在任何复现前于启动进程加载当前忽略的输入配置。

[修正后的已接受 PR818 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)保留最初 South Parmecia=70 假设失败，以及未验证目的地关联的早前成功检查/CI。修正检查先解析传送符号，再选择该数值地图的配置/实体/区域/init 与战斗所有者，包括角落竞争行。最终修正检查/CI 与早前检查、引用/路径/传播和 rebase 失败仍保留在该交接及链接记录中。例行重放或独立假定的地图所有者不能替代修正关联。

**未知：**从 Luke 出口自然到达登乘标记与任一南出口；实际布局、标志、HP、行动者映射与列表；待处理写入保留；行动、VInt/DMA、声音、精灵与过渡完成；所有随从完成；可见登乘/抵达、输入响应及已保存交通状态的持久性。即便 F64 已设仍不对齐、lookup63 别名、空/重排列表、更早随从仍忙碌、模式提交前表现被中断、角落行优先级、已完成但仍解锁的准入，以及同一 F417 状态走不同 row6/row9 生命周期，仍是用于区分行为的传入用例，不是观察到的自然历史。

这些限制保留登乘/旅行含义的**推断**标签，并按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)继续延期。它们不授权采集、完整 type5 消费、后续 zone/Creed/Kraken 剧情、交战或战役/epic 完成。
