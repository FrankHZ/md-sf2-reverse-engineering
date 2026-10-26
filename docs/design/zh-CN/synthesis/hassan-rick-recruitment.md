# Hassan Rick 招募与跟随

> 本文是[规范英文原文](../../synthesis/hassan-rick-recruitment.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Rick 可通过 Hassan 一段独立对话加入军团。他感谢领队帮助自己到达 Hassan，随后注意到 Caravan。Peter 说明其来处，Rick 请求与出战队伍同行；场景先让他加入，再请求其当前行动者跟随一行人。这是可选交互分支，不是必需的神殿返回或新旅行指令。

**已确认：**已接受的[研究所有者](../../../research/hassan-rick-recruitment.md)确立条件实体群、普通激活、完整招募场景及其直接成员资格/跟随/控制消费者。**推断：**Rick 对 Caravan 的好奇促使他请求随队离开。**未知：**自然可达、实际传入标志/HP/名册与行动者映射、服务/表现完成、可用输入与存档/恢复持久性。

各自边界见 [Hassan 入场](battle16-outcomes-through-hassan-entry.md)、[Rohde 提议](hassan-ancestry-and-rohde-caravan-choice.md)和 [Caravan 获取](battle18-outcomes-and-caravan-acquisition.md)。这些场景不确立到此对话的必需旅程。本指南止于条件普通控制/重新入场边界。其他 NPC 对话、Caravan 运行、后续旅行/Battle19 与 Rick 成长、装备或交战均在范围外。Fnnn 表示标志 nnn；索引从零开始。下文坐标、等待与行动请求是源码操作数，不是测量动画或输入回执。对话为转述。

## 选择事件不同部分的状态

**已确认：**下列状态具有不同消费者与生命周期。

| 状态 | 此处控制内容 | 不确立的内容 |
| --- | --- | --- |
| F518 | 选择含 Rick 固定行动者的 Hassan 替代实体群。 | Caravan 运行、旅程完成或交谈准入。 |
| F12 | Rick 已加入军团成员资格；已设时 Hassan init 把其映射行动者移至地图外。 | 出战队伍成员资格或永久跟随。 |
| F44 | Rick 出战队伍成员资格，独立于 F12。 | 同步名册列表快照、存活 HP 或随从行动。 |
| F256 | Rick 处理器检查的临时抑制，在场景返回后设置。 | 持久招募或原子完成回执。 |
| F65 | 独立 Caravan 上下文；Hassan init 据此把 Rohde 移至地图外。 | Rick 处理器检查的前提，或 Rick 交通/控制器模式。 |
| 当前物理随从列表/行动 | 选择跟随目标，将列表行动者排除普通交谈，并保存当前跟随请求。 | 在地图实体群重建时重建 Rick 的持久标志。 |

本招募处理器不测试 F65、持有 Sword、F516 或战斗顺序。下文 Kraken 与神殿提及均为归属台词。实体群可使 Rick 出现，而处理器未检查那些叙述事件。

## Rick 的出现位置与 init 效果

**已确认：**Map9 默认配置11 行，无 Rick。F518 选择12 行替代配置，在 Rohde 后立即加入 `(12,3)`、Right、使用 `eas_Init` 的 Rick。两列表共享实体事件表与地图 init。替代配置还改变其他三个行动者的放置/行动操作数；其对话不在本指南内。十条非己方行映射至逻辑128–137；己方行不消耗该序列。Rick 逻辑身份是 ally12，不是预设的 Rohde 旁物理槽位。

重新初始化实体会清除物理状态与全部64 查找字节，从物理1 起声明按标志选择的随从，再声明配置行动者，最后创建主角物理0。己方配置行复用既有非零查找映射，不重复声明。Rick 已加入标志不是实体群准入测试。其初始行动设 speed32、禁用加速/减速、启用阻挡及地图/实体碰撞、启用自动朝向、设 Up、禁用浸入、设 size24、刷新精灵并空闲。不治疗他。

战斗外，HP0 Rick 获得蓝焰地图精灵身份；存活 Rick 使用当前职业精灵。HP0 不移除配置行动者或其事件行。此实体群规则不推出固定职业或装备值。

即使 F65 已设，Hassan 仍使用普通随从表。该表没有 Rick 声明。Peter 取决于 F67，在两种 Hassan 列表中均无固定行。F84 可在固定行前声明 Rohde，使固定行复用其查找映射。仅选择替代配置不推出 Peter 在场/存活 HP 或特定随从顺序。

完整共享 init 有13 操作。依次在 F518 清除时将逻辑137 移出地图、F65 已设时将 Rohde11 移出地图、F12 已设时将 Rick12 移出地图，然后返回。既不招募，也不写交通或新战斗标志。`MoveEntityOutOfMap` 解析当前查找映射，把位置 X/Y 与两个目的地写为 `$7000`。不删除行动者/映射/列表、不停止行动或速度、不调用 `HideEntity`，也不改变 HP。

因此 F518 已设/F12 已设时重建实体群仍先声明 Rick，再移走。传入 F518 清除/F12 已设的重建状态没有 Rick 行填充 lookup12；其默认零别名至物理0，init 改为移动它。这些是传入状态的源码后果，不是观察到的自然缺陷或可用控制证明。辅助不修复缺失映射。

## 普通对话准入

**已确认：**普通玩家行动先令物理0 空闲，等待其移动与视野滚动。A 优先于 C。调试/Caravan 分支之后的普通 C 分支中，`GetActivatedEntity` 使用当前视野目标/朝向与布局，选取首个符合条件的附近行动者，曼哈顿距离小于一个内部地图瓦片（384）。跨 `$2800` 柜台图块延伸寻址点，排除视野目标及已在 `EXPLORATION_ENTITIES` 中的物理行动者。不只是选择固定 Rick 坐标，也不测试 F518/F12。

反向查找必须将该物理行动者解析为逻辑12，才选择 Rick 事件行。查找别名可在扫描中解析至更早身份。用人工状态直接调用 `Map9_EntityEvent1` 既不证明接近准入，也不证明存在 Rick 行动者。同样，默认零查找映射不足以证明可交谈 Rick。

行 `ALLY_RICK, DOWN, Map9_EntityEvent1` 中 DOWN=3 是**行动标志**，不是要求玩家朝向。实体包装器保存玩家朝向并提供立绘/台词属性。Bit0 在处理器前把映射行动者转向玩家对面，即 `(playerFacing+2)&3`。Bit1 随后以已保存物理索引恢复行动者原朝向。因此场景最终朝向不一定保留到普通包装器之后。

包装器在处理器周围禁用更新，但 Trap6 脚本桥在执行场景前启用实体更新。这为被等待行动提供更新路径，不证明完成。处理器后，包装器恢复朝向、关闭立绘/文本并重新激活更新。

处理器仅五操作：检查 F256、已设则返回、运行 `cs_56B02`、设置 F256、返回。不检查成员资格、容量或 HP。F256 已设抑制招募脚本，而获准包装器朝向/窗口工作仍发生。F256 清除进入完整场景；仅场景返回后才写 F256。

## 完整对话与布置

**已确认：**`cs_56B02`（位于 `0x56B02`） 含29 命令，无选择、内部分支或内联行动主体。保留物理语料把它归于 `cs_56AE6`：records6–34 是 Rick 场景；records0–5 是 Rohde 重复提示并分支跳离。Rick 处理器直接进入自身标签。Rohde 前缀不是 Rick 台词或招募选择。

| 物理记录 | 有序场景含义与请求 |
| --- | --- |
| 6–11 | 设置文本游标1411；设 Rick priority `$FFFF`、Bowie priority0。Rick 问领队是否杀死 Kraken（1411），感谢帮助他到达 Hassan（1412），随后说城镇无聊（1413）。 |
| 12–18 | 等待20；设 Rick Up；等待30；运行/等待 Rick `eas_Jump`；等待20；令 Rick 朝向 Bowie；Rick 询问所见事物（1414）。 |
| 19–23 | Peter 朝向 Rick，随后 Rick 朝向 Peter。Peter 说 Caravan 是在神殿找到的古代交通工具（1415）。等待20；Rick 朝向 Bowie。 |
| 24–28 | Rick 表现兴趣（1416）、问及发现（1417）、请求与出战队伍同行（1418）；加入 Rick；Rick 庆祝离开（1419）。 |
| 29–34 | 两次运行/等待 Rick 跳跃；等待20；设 Bowie priority `$FFFF`；把 Rick 加为当前随从；结束。 |

以上为全部九个场景文本位置，IDs1411–1419，源码文本键 `0583..058B`。Rick 说话者操作数为 `$0,ALLY_RICK`；Peter 为 `$C0,ALLY_PETER`。IDs1411,1412,1416,1417 使用 `nextText`，带源码 `{W2}`；1413,1414,1415,1418,1419 使用 `nextSingleText`，带 `{W1}`。通用加入宣告446（`01BE`）是用 Rick 姓名/当前职业的附加服务文本，不是第十个游标位置。

文本处理器取得立绘/台词属性、等待视野滚动、请求显示并推进游标。单文本还关闭立绘/文本并等待10。W1/W2 是文本服务控制，不是额外场景命令或确认回执。脚本入口清除文本跳过字节。调试模式 P2 Start 可设置它，抑制普通显示/游标增量与脚本睡眠；不绕过每个行动或加入服务，也不是普通剧情路径。

**推断：**短暂移开视线、跳跃与互相注视呈现无聊转为兴奋。Peter 解释提供对话上下文。**已确认：**这些台词均不检查实际交通工具行动者、F65、Sword 持有、Kraken 结果或自然神殿路线。

### 朝向、优先级与跳跃完成

**已确认：**初始两次 Rick/Bowie 优先级写入给 Rick 非零精灵链接优先级、Bowie 零；后续 Bowie 写入给其非零优先级。非零条目在零之前处理。它们是显式值，不是恢复旧值；既不是战斗行动排序，也不是直接 VDP 瓦片优先位写入。

显式 Rick-Up 命令将打包己方选择器传给角色存活辅助，在 Rick HP0 时跳过。四个 `faceEntity` 命令没有 HP 守卫。使用行动者**目的地**，不是可见位置：只有 X 距离绝对值大于 Y 距离才选水平，否则垂直。目标在左/上选 Left/Up，另一侧选 Right/Down；目的地重合选 Down。每次写朝向、刷新精灵并等待一次 VInt。Peter lookup7 缺失可将操作重定向至物理0；不移除 Peter 台词。

三次被等待跳跃均替换 Rick 行动指针，再轮询至该行动者指针为 `eas_Idle`。没有 HP 守卫或全队屏障。跳跃禁用自动朝向，请求相对 `(0,-1)`、等待5、请求 `(0,1)`、等待5、请求 `(0,0)`、重启自动朝向并返回空闲。相对移动从当前位置按384 缩放，检查其他实体目的地冲突。请求之间没有目的地相等等待。因此空闲行动指针不证明精确可见弧线或稳定位置。停滞更新/服务可阻止成员资格、跟随或调用方后续 F256 写入，同时保留更早变化。

## 加入军团先于跟随

**已确认：**`join ALLY_RICK` 是普通 csc08 word12 形式，没有 sad-join 或特殊128 Sarah/Chester 行为。清除当前台词 SFX、等待视野滚动、选择加入音乐、调用 `JoinForce`、读取当前职业、提供姓名/职业对话索引并请求446。

`JoinForce` 执行三个不同步骤：

1. 设置 F12，即便原已设。
2. 从独立标志重建已加入、出战与后备计数列表，不过滤 HP。
3. 重建出战计数低于12 时，设置 Rick 出战标志 F44。12 或更多则跳过该写入；不清除既有 F44 或替换另一成员。

重建先于可能的 F44 写入，该加入尾部没有第二次重建。新激活 Rick 可继续留在后备快照中，直到后续 `UpdateForce`。满队仅阻止新出战写入，不阻止 F12、宣告或后续随从请求。HP0 不阻止这些成员资格写入。场景与加入均不治疗/复活、重置交战属性、授予 AI 控制或重选 Rick 地图精灵身份。

宣告尾部通过 `PlayMusicAfterCurrentOne` 调度先前音乐，随后 `WaitForPlayerInput` 等待 VInt，直到 `CURRENT_PLAYER_INPUT` 含标准方向/按键位。不要求释放或新输入边沿；已按住的当前输入可合格。辅助名 `FadeOut_WaitForP1Input` 既不证明屏幕淡出，也不证明音频完成。加入处理器随后关文本并等待10。Rick 最后台词与两跳仍在该尾部之后。

## 跟随属于当前实体群

**已确认：**`addNewFollower ALLY_RICK` 使用 csc56，不同于显式 `followEntity` 命令。没有 Rick-HP 或 Bowie-HP 守卫。解析 Rick12，扫描当前物理列表直到 `$FF` 哨兵，选择**最后列出行动者**，空列表则物理0。传递随从偏移 `(-24,0)` 像素。不按名字选 Peter，也不创建缺失 Rick。

`AddFollower` 复制 42-byte 随从行动块，修补目标/偏移并推进行走参数分配。仅 Rick 物理行动者尚不存在时，才将其追加在哨兵前。既有条目不移动或移除，但即使重复也会安装/分配另一个块。若 Rick 已是尾部，目标成为自身；没有自身目标拒绝。默认缺失查找映射可改为给物理0 安装该行动。这些传入状态限制不是自然观察的缺陷或容量安全证据。

行动设 speed32、禁用加速/减速和阻挡、启用地图/实体不可碰撞，再循环跟随/目的地等待。其移动消费者在较大坐标距离超过 `$1E0` 内部单位时作用；由目标目的地/朝向与偏移派生目的地，带回退检查布局，并写行程/速度。安装不是立即放置或跟随完成。场景没有最终距离/全部行动者等待。

不写 Rick 随从标志。F12/F44 记录成员资格；F256 抑制对话；当前物理列表/行动保存跟随。普通表的实体群重建在持久随从中没有 Rick 行。`InitializeFollowerActscripts` 按当前随从标志向连续物理槽位写入，是另一例程，不重建/解析该列表。Rick 场景既不调用它，也不调用随从重建初始化。

## 完成、部分执行与重新入场

**已确认：**优先级/朝向/首跳变化先于成员资格；成员资格先于最后台词/两跳、Bowie 优先级写入、跟随与调用方 F256。这不是原子事务或回滚流程。后续服务未完成时，任何已完成前缀均可保留。F518/F65 不变，没有传送、Sword 事务、Caravan 运行或交通模式规范化。

场景结束后，对话窗口索引非零时脚本解释器条件等待滚动，清除滚动速度并恢复寄存器。Trap6 返回；处理器设置 F256；包装器恢复 Rick 已保存朝向、关窗并启用更新；普通行动返回。`WaitForEvent` 仅在待处理事件为零时重新以物理0 为视野目标并安装受控行动。PLAYER_TYPE0 选普通步行；其他传入模式保留共享选择。返回顺序已由静态确立；响应输入、随从稳定和下一旅程未知。

| 传入状态/边界 | 条件后果 |
| --- | --- |
| 重建时 F518 已设/F12 清除 | 声明固定 Rick 并留在原处；重建后 F256 清除。普通接近/反向查找仍须选中他。不要求 F65 已设。 |
| 重建时 F518 清除/F12 清除 | 配置不提供 Rick 行动者。仅有共享事件行不使其可交互。 |
| F256 已设/F12 清除 | 缺少成员资格仍由处理器抑制招募；获准包装器工作仍发生。 |
| F256 清除/F12 已设 | 若到达，处理器可重放完整场景。无已加入捷径；init/实体群与交谈排除是独立障碍。 |
| 同一实体群内场景完成 | F12 已设，F44 条件设置，当前跟随安装，再设置 F256。列表物理行动者被排除普通激活；直接处理器调用另由 F256 抑制。 |
| 加入后停止 | 成员资格可已改变，而跟随安装与 F256 尚未完成。重试不撤销该前缀。 |
| F518/F12 已设时重新入场 | 重建丢弃旧行动者/列表/行动实体群并清除临时标志。Rick 固定行先声明，再由 F12 移走；清除 F256 不恢复普通可见性或永久跟随。 |
| 保留实体群的探索入场（-1） | 列表/映射/F256 不重置。F12 已设使 init 仍把映射 Rick 移出地图，却不停止其跟随行动；后续效果未证明。 |
| F518 清除/F12 已设时重新入场 | 未填充 lookup12 别名至 main0，init 把它移出地图。不确立自然失败或安全可用控制。 |
| 满队、HP0、Peter 缺失或列表变化 | 分别应用容量、HP、查找和当前目标规则；无共享资格检查修复。 |

两种探索入场之前，共享治疗消费者均恢复存活己方与不死 Peter/Lemon，跳过普通死亡 Rick。这是探索服务，不是招募治疗。显式地图入场重建行动者、清除 F256–383 并设置 F80；map -1 入场改为更新主角/木筏状态，跳过实体群/临时重置。两者随后都运行当前地图 init。因此再次调用探索不一定重置 Rick 对话或当前跟随。交通仍独立传入。本指南止于此条件边界。

## 证据、可执行所有者与审阅边界

本 Layer B 说明使用已接受 [#848](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/848)，工作归于 [#851](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/851)。[研究协议/溯源](../../../research/hassan-rick-recruitment.md#agreement-and-provenance)和[复现/覆盖所有者](../../../research/hassan-rick-recruitment.md#reproduction-and-coverage-limits)保留选定源码关联：SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`，USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`，scene source134–162、全部29 命令/九条文本、完整直接消费者与通用446。本指南不重新调查源码或采集新运行时。

| 说明边界 | 合同、精确夹具与选定字段 | 限制 |
| --- | --- | --- |
| 配置/行动者/init | [配置](../contracts/map-setup-data.md)、[实体](../contracts/map-entity-data.md)、[探索](../contracts/map-exploration.md)；`sf2-map-setup-static-v1` [配置夹具](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-entities-static-v1` [实体夹具](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-init-static-v1` [init 夹具](../../../../tests/fixtures/h2/map-init-static-v1.json) | Init `expected.primarySourceBodies[sourceOwnerSymbol=ms_map9_InitFunction]` 含13 有序操作。选定实际配置列表、枚举/查找/init 消费者及重建/-1 调用方是研究所有者关联，不是自然访问采集。 |
| 普通交互与临时守卫 | [探索](../contracts/map-exploration.md)；`sf2-map-events-static-v1` [实体分片](../../../../tests/fixtures/h2/map-events/entity-programs.json) | `expected.entityTargetPrograms[canonicalSymbol=Map9_EntityEvent1]`，program672/address0x56842，含5 操作、一个物理行/两个配置引用。精确处理器调用不证明普通行动者准入。 |
| 场景台词/行动/朝向/优先级 | [对话](../contracts/dialogue-system.md)、[探索](../contracts/map-exploration.md)；`sf2-map-script-engine-static-v1` [脚本夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | `dialogueCommandFacts`、行动/动作/生命周期字段保留共享处理器/位置；保留 `programCorpus.programs[id=cs_56AE6]` records6–34 经研究提供完整场景顺序。夹具13 对话命令包含 Rohde 前缀与两个游标；不是 Rick 九个台词位置。 |
| 加入与出战列表快照 | [成员资格](../contracts/party-membership-state.md)、[名册](../contracts/party-roster-state.md)；`sf2-common-stats-static-v1` [属性夹具](../../../../tests/fixtures/h2/common-stats-static-v1.json)，脚本 `forceStateCommandFacts` | 脚本 join site27/source155 为 Rick12。共享成员资格合同保留符号范围/容量及先重建后出战写入；Rick F12/F44、threshold12 与 HP/输入后果使用已接受选定研究，不扩展合同主张。 |
| 当前跟随 | [名册](../contracts/party-roster-state.md)、[已接受跟随研究](../../../research/polca-oddler-choice-and-following.md)；脚本 `forceStateCommandFacts.activePartyCommandFacts` | Rick add site33/source161 独立于 `followEntity`。共享处理器事实与有界直接处理器观察不证明永久 Rick 跟随、自然目标顺序或场景完成。 |
| 条件控制/请求返回 | [探索流程](../contracts/exploration-control-flow.md)、[入场路由](../contracts/map-entry-routing-state.md)与[请求研究](../../../research/map-event-request-consumption.md)；`sf2-map-event-request-consumption-static-v1` [请求夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) | 共享请求访问事实不执行其消费者被调用体；选定控制顺序仍由研究拥有，无实际 Rick 确认、响应控制或持久性。 |

Schema/消费者配对为[配置 schema](../../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../../src/sf2tool/h2/map_setup.py)、[实体 schema](../../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../../src/sf2tool/h2/map_entities.py)、[init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../src/sf2tool/h2/map_init.py)、[实体章节 schema](../../../../schemas/h2/map-events-entity-programs.schema.json)及[分片 schema](../../../../schemas/h2/map-events-fixture-shard.schema.json)/[map_events_fixture.py](../../../../src/sf2tool/h2/map_events_fixture.py)与 [map_events.py](../../../../src/sf2tool/h2/map_events.py)、[脚本 schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)与[组件 schema 所有权](../../../../src/sf2tool/h2/map_script_schema.py)、[属性 schema](../../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../../src/sf2tool/h2/stats.py)，以及[请求 schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)。现有窄命令为 `uv run sf2 h2`，后接对应 `map-setup`、`map-entities`、`map-init`、`map-events`、`map-script-engine`、`common-stats` 或 `map-event-request-consumption`。这些标识可执行所有者，不是说明要求的额外证据运行。任何受控命令均于同一进程加载工作树忽略配置。

综合前审阅检查已接受研究、合同边界与有界夹具/schema/消费者选项，覆盖独立标志、物理与逻辑身份、命令与文本计数、HP 守卫、目的地朝向、当前输入确认、名册快照、重复/自身目标跟随及部分/重建/-1 执行。不需要 Layer A 修复。当汇总合同刻意将数值容量或随从生命周期保留未知时，以更严格的选定 Rick 关联为准。传入状态表是区分行为的静态审阅矩阵，不是已执行自然玩法。

[PR849 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161609727)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161628177)保留完整源码/夹具关联、保留比较、CI 与已修正/失败检查。[PR850 rebase 后交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/850#issuecomment-6161661260)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/850#issuecomment-6161671119)保留原始/rebase 后结果与前段历史。不仅为本综合重放这些已接受检查，不重标已完成失败或削弱夹具。

**未知：**自然访问/交谈可达、实际标志/HP/职业/出战名册/映射/列表顺序、输入/确认、行动/位置/优先级/精灵/音频及其他服务完成、可用控制与存档/恢复持久性。汇总静态事实与共享处理器观察不证明自然组合招募场景。这些缺口不按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 或 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)自动准入运行时工作。
