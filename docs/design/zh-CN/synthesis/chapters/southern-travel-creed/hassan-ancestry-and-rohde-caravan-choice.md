# Hassan 祖先对话与 Rohde 的 Caravan 选择

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/hassan-ancestry-and-rohde-caravan-choice.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Hassan 提供两段独立对话。祭司询问一行人的来处，向 Peter 讲述祖先追随 Max 的故事。另一边，Rohde 认出 Achilles Sword，提议随行获取 Ancient Caravan。拒绝后可再次交谈选择；接受会请求跟随并设置未来剧情状态。不招募军团成员、消耗剑或使 Caravan 可运行。

**已确认：**已接受的[研究所有者](../../../../../research/chapters/southern-travel-creed/hassan-ancestry-and-rohde-caravan-choice.md)提供此处说明的静态准入、完整对话/行动图及直接调用方/消费者关联。**推断：**交流呈现祖先故事与未来交通工具向导。**未知：**自然准入、实际传入状态/输入、服务与表现完成、可用控制、说话者主张的历史真实性与持久保存。Fnnn 表示标志 nnn；坐标为地图瓦片，记录索引从零开始，下文对话为转述。

城镇实体群与交通限制见 [Battle16 结果至 Hassan 入场](battle16-outcomes-through-hassan-entry.md)。本指南不要求该结果或任何一段对话先发生。止于对话的条件调用方/控制返回，早于剑的获取、Rick、城镇离开、Taros 交战或 Caravan 使用。不增加重制能力或自然游玩流程。

独立物品持有问题见 [Ribble 树洞开启与 Achilles Sword](ribble-tree-opening-and-achilles-sword.md)。它解释选定 Panel/Sword 宝箱容量、Panel 使用与条件神殿/Ribble 返回，不要求每次 Rohde 对话前都发生这些事件，也不将本指南扩展为获取流程。宝箱/开启标志本身不证明持有 Sword。

独立离开与遭遇边界见 [Hassan 出发、Battle17 进入与条件返回](hassan-departure-and-battle17-outcomes.md)。它解释有序 Battle9/17 准入、步行/木筏请求与条件结果返回。Rohde 的 F418 独立于 Battle17 的 F417/F467/F517；此链接不增加对话前提。

## 三种对话入口

**已确认实体群：**Map9 两种配置使用相同 zone、实体事件与物品事件表。F518 选择不同实体群，不选择不同对话处理器。第三条非己方行是 `(7,58)` 的祭司130，不同于教堂服务祭司133。默认祭司朝 Up，使用 `eas_InitSlow`；替代朝 Left，使用 `eas_Init`。两者均提供 `(16,59)`、Down 的 Rohde11，除非当前随从映射已提供他。Peter 没有固定城镇行：由 F67 随从声明提供。F67 清除时重建实体群可能留下 Peter 查找映射为零，别名至物理0，而非安全省略其行动。

F84 可提供 Rohde 随从。Hassan 的 F65 init 可将映射 Rohde 移出地图，却不使映射失效。普通激活排除 `EXPLORATION_ENTITIES` 中的物理行动者。因此城镇入场、表中存在 Rohde 或接受标志都不证明当前玩家能与他交谈。下列谓词消费传入状态，不修复实体群或强制一致的先前历史。

### 穿过祭司 zone

三个精确 zone 行 `(4,57)`、`(4,58)` 与 `(4,59)` 共用 `Map9_ZoneEvent0`（位于 `0x56866`）。F726 已设则返回；F726 清除则运行祭司场景，在**脚本返回后**设置 F726。该写入同时抑制三行。不测试剑、HP、成员资格、F725 或 F727；Rohde 事件不是前提。

步行生产者必须实际遇到工作布局 zone 标记，以目的地加移动偏移坐标提交 type 6 请求。消费者清除待处理状态，令物理0 空闲并分发传入坐标。仅站在表列瓦片不是生产者已运行的回执。对话/行动中断可留下 F726 清除与部分行动者效果，因此另一次符合条件调用可再次请求场景。

### 与 Rohde 交谈

普通 C 交互先令物理0 空闲并等待其空闲与视野滚动。激活使用当前视野目标、朝向、布局与附近物理行动者，包括柜台延伸规则。选定行动者必须反向映射为逻辑11 才选择 Rohde 行。别名、随从列表排除、行动者缺失或地图外放置均可阻止选择。

该行的 `RIGHT=0` 操作数是**行动标志，不是要求玩家朝向**。调用前转向位与已保存朝向恢复位均未设置。包装器保存传入玩家朝向，从映射行动者选择说话者属性，在处理器前后禁用实体更新，随后关窗并重新激活更新。脚本宏执行场景前激活更新；此服务顺序不证明实际完成。

| 传入交谈状态 | 所调用服务返回时的后果 |
| --- | --- |
| F727 清除，未找到剑 | Rohde 请求忙碌回绝 text1371，设置 F798，不识别或选择便返回。传入 F725 不绕过此路径。 |
| F727 清除，找到剑，F798 清除 | 请求 text1371，关闭立绘/文本，Sleep 40，再运行识别与初次选择；脚本返回后设置 F727。此前缀不设置 F798。 |
| F727 清除，找到剑，F798 已设 | 立即进入识别与初次选择；脚本返回后设置 F727。 |
| F727 已设，F725 清除 | 不搜索物品栏。让映射 Rohde 相对传入玩家朝向转向，再运行重复选择。不新写 F727。 |
| F727 与 F725 均已设 | 静默返回，不搜索物品栏、不运行朝向辅助或提示。 |

初次搜索调用 `GetItemInventoryLocation`，更新军团并扫描**已加入成员，包括后备成员**，不要求 HP。从末槽至首槽搜索各成员持有物品，以 `$7F` 掩码物品条目，再比较 Achilles Sword `$3D`。处理器仅测试未找到结果。Bowie 不必亲自携带或装备剑；这不是搜索所有可能存储位置。不移除或装备物品。搜索假定普通非空军团/列表不变量，不修复畸形空列表。

重复辅助虽名为 `GetRhodeFacing`，实际包含写入：解析 Rohde，把其朝向设为 `(suppliedPlayerFacing+2)&3`。行标志为零不会撤销此转向或拒绝分支显式 Down 朝向。F727 已设的重复交谈可在不再持剑后继续。

### 在 Item → Use 中选择剑

另一入口从 FieldMenu 当前军团物品界面开始：选择成员、槽位与物品，再选 Use。取消不会到达回调。Achilles Sword 不在普通野外可用物品列表内，因此此选择走地图物品事件分支。

第一条物品行要求**选定 Achilles Sword `$3D` 且物理0 当前位于 `(16,58)`**。使用当前位置除以每瓦片 384 内部单位，不是前方瓦片或选定携带者位置。`NO_DIRECTION=$FF` 接受任意传入朝向。此路线不重新搜索物品栏、不测试 F798、不按接近激活 Rohde，也不独立要求其在场。某处持有不足够；菜单选择与坐标谓词必须匹配。Rohde 固定行 `(16,59)` 是不同坐标。

| 选定物品处理器状态 | 服务返回时的调用方/菜单后果 |
| --- | --- |
| F727 清除 | 运行无 text1371 的初次识别/选择；任一回答完成后设置 F727，再返回已处理结果 d6=-1。FieldMenu 退出。 |
| F727 已设，F725 任意值 | 抑制场景，保留包装器 d6=0。FieldMenu 请求通用已用物品 text73 和无效果 text422，再返回物品子菜单。 |

错误坐标/物品不选择此行；共享未匹配返回具有同样零结果回退。被抑制物品使用不是重复 Rohde 选择，也不直接恢复探索。玩家仍需随后退出菜单。选定处理器均不移除、接收、给予或装备剑。通用已用物品措辞不是消耗证据。

## 祭司的故事

**已确认完整场景：**`cs_568F6`（位于 `0x568F6`） 含 44 条保留记录和六个内联主体。序列请求十条文本1372–1381，说话者为祭司130 与 Peter。移动与停顿属于场景，不是新旅行或名册事实。

| 记录 | 有序行动与归属含义 |
| --- | --- |
| 0–9 | 设置游标1372；Bowie 朝 Right；Sleep 20；等待祭司 Left1/Up1，再令其朝 Left。祭司询问一行人来自哪里（1372）。Sleep 20；将 Peter 目的地设为 `(4,58)`、朝 Right；Peter 说领队来自 Grans（1373）。 |
| 10–18 | 等待祭司 speed48/48 后 idle，再 Left1。祭司提出可能有渊源（1374）。Sleep 20；Peter 朝 Up；Sleep 20；Peter 问他是什么意思（1375）。Sleep 40；等待祭司 `eas_InitSlow`。 |
| 19–29 | 祭司 Down；Sleep 30；Left；Sleep 40；启动其 Right1/Down1 行动但不等待。Sleep 20；等待 Bowie Right1；Sleep 10；Bowie Down、Peter Right、祭司 Left。 |
| 30–38 | 祭司说祖先追随士兵 Max 到 Parmecia（1376），在海岸失去其踪迹并分开（1377），其中一群经 Devil's Tail 返回 Grans（1378）。Sleep 20；Peter 问 Max 后来如何（1379）。Sleep 20；祭司猜测 Max 去了 Grans，并承认不确定（1380），随后宣布小睡（1381）。Sleep 30。 |
| 39–43 | 等待 Peter Up1；Peter Right；Bowie Left；以 selector2 请求 Peter→Bowie 跟随；结束。仅随后 zone 调用方才设置 F726。 |

故事属于祭司，包括他自己的不确定。Peter 的来处陈述也是归属台词。该交流既不确认 Bowie 血统、Max 历史，也不确认经独立验证的 Devil's Tail 路线。不写祖先或军团成员资格标志。请求的 Peter 跟随仍受下文映射/HP 边界限制。

祭司操作数为 `$C0,130`；Peter 使用 `$0,ALLY_PETER`。Text1376/1377/1380 用连续 `nextText`；其余七处用 `nextSingleText`。这些保留命令/服务区别，不是新观察的立绘布局、文本节奏、动作或可听结果。

## 识别与初次选择

**已确认完整初次图：**`cs_569BC`（位于 `0x569BC`） 在一个含 50 条记录的物理程序内，包含识别区域、拒绝 `cs_56A70`、接受 `cs_56A82` 与共同返回 `cs_56AD4`，另有六个内联主体。其 13 个文本位置为1382–1391 和1394–1396；1392–1393 两处属于下述独立重复前缀。Rohde 区域合计有 15 个跨替代分支文本位置；没有单一回答会按物理顺序播放全部文本。

| 记录 | 有序布置与归属含义 |
| --- | --- |
| 0–6 | 游标1382；Peter 目的地 `(15,57)`；Peter 与 Bowie Down。Rohde 评论年龄（1382），朝 Up 并认出 Achilles Sword（1383）。 |
| 7–14 | 等待 Rohde speed40/40→idle、Bowie speed40/40→idle 与 Bowie 关闭自动朝向。启动 Rohde Up1 但不等待；Sleep 3；等待 Bowie Up1。Rohde 询问冒险/交通工具（1384），声称剑可用于获取 Ancient Caravan（1385）。 |
| 15–23 | Peter 目的地 `(15,58)`、Right；斥责 Rohde 先前的回绝（1386）。Rohde Left，辩解（1387），再 Up；请求同行（1388），声称唯有自己会驾驶（1389），问一行人是否去获取 Caravan（1390）。 |
| 24–25 | 请求 Yes/No；F89 已设选择接受，F89 清除选择初次拒绝布置。 |
| 26–28，仅 No/B | 等待 Rohde speed10/10→idle，等待关闭自动朝向，再等待 Down1；落入拒绝。 |
| 29–32，拒绝 | 游标1391；Rohde 抗议错失机会（1391），朝 Down 并跳至共同返回，绕过接受。 |

Rohde 使用 `$C0,ALLY_ROHDE`；Peter 使用 `$0,ALLY_PETER`。Text1382/1384 与1387–1390 使用 `nextText`；1383/1385/1386 及拒绝 text1391 使用 `nextSingleText`。即使选定物品 Use 跳过 text1371，或未观察到更早回绝，Peter 仍会斥责。不证明该回绝自然发生。Rohde 关于剑和驾驶的陈述是对话中的主张，不是交通工具获取或运行证明。

### 选择 Yes 或 No

已接受的[提示所有者](../../../../../research/chapters/eastern-parmecia/volcanon-return-through-bedoe-king-choice.md#caller-prompt-result-and-response-texts)提供实际输入规则。初始选中 Yes。非零入口输入快照必须先释放至零。循环按 Left、Right、B、C、A 顺序测试：Left 选 Yes，Right 选 No；均不确认。B 选择 No 并退出；C/A 确认选项。无输入则等待，没有超时或自动接受。B 是拒绝，不是第三种剧情结果。

提示窗口关闭后，d0=0 表示 Yes，d0=`$FFFF` 表示 No/B。脚本在零时设置 F89、非零时清除，再睡眠 10 后按标志分支。旧传入 F89 不能替代本次提示完成。实际采样输入、释放时序和可见选择仍未知。

### 接受与共同结尾

接受 records33–46 设置游标1394。Rohde 赞同（1394）；旁白（1395）宣布决定带上他。等待 Rohde `eas_Init`；设 Rohde priority `$FFFF`、Bowie priority0；等待 Rohde `eas_Jump`，随后启动第二次跳跃而不等待。请求其出发催促（1396），设 Bowie priority `$FFFF`。这三条文本使用 `nextSingleText`；旁白为 `$FF,ENTITY_NONE`，不是新说话行动者。

随后依次**设置 F725、设置 F84、setStoryFlag 18→F418**，以 selector2 请求 Rohde11 跟随**当前逻辑 FOLLOWER_B31**。接受落入共同 records47–49：等待 Bowie `eas_Init`，以 selector2 请求 Peter7→Bowie0 跟随，结束。拒绝直接跳至同一共同区域。因此两个回答均初始化 Bowie 并请求 Peter 跟随；仅接受写725/84/418 并请求 Rohde 跟随。

没有 `JoinForce`、出战队伍或 AI 授予、治疗、F65 写入、交通模式变化、Caravan 创建、传送或离开请求。F418 是未来战斗解锁写入，不是立即 Battle18 准入或 F518 完成。F725 接受、F84 随从实体群与军团成员资格仍是不同状态通道。旁白陈述不是成员资格服务。

**仅在初次脚本返回后**，交谈/物品调用方才设置 F727；物品调用方随后设置 d6=-1。F727 记录任一回答后的初次完成，不记录同意提议。拒绝不清除传入725/84/418；部分接受可在跟随、共同完成或 F727 之前已写这些标志。处理器不修复该不一致。

## 重返提议

**已确认重复图：**`cs_56AE6` 前六条记录将游标重置为1392，以 `nextText` 请求 Rohde 认出/询问是否改变主意（1392–1393），提示，再按 F89 已设跳接受，否则拒绝。仅从 F727 已设/F725 清除的**交谈**进入，不再搜索剑。物理程序也含 Rick 场景，但两个选定重复分支在此前跳离；Rick 不在本指南范围。

| 选择路径 | 执行区域与直接调用方尾部 |
| --- | --- |
| 初次 Yes | 识别0–25 → 接受33–46 → 共同47–49 → 调用方 F727；选定物品调用方另设 d6=-1。 |
| 初次 No/B | 识别0–28 → 拒绝29–32 → 共同47–49 → 调用方 F727；选定物品调用方另设 d6=-1。 |
| 重复 Yes | 六记录重复前缀 → 接受 → 共同 → 实体返回；不新写 F727。 |
| 重复 No/B | 重复前缀 → 拒绝 → 共同 → 实体返回；没有初次 speed10/关闭自动朝向/Down1 布置或新 F727 写入。 |

重复 Yes 跳过全部初次识别/布置，使用当前行动者。重复 No 仍请求1391 并令 Rohde 朝 Down，但不重新初始化其速度或自动朝向状态。初次 No 留下更早 speed10/自动朝向关闭状态；共同部分仅初始化 Bowie。F727/F725 均已设的交谈即使跟随不一致也静默。反之，传入 F725 已设/F727 清除仍走初次准入。祭司 F726 从不参与这些决定。

## 跟随与行动完成的实际含义

**已确认消费者限制：**普通 `setFacing` 使用打包己方字节进行己方 HP 检查，因此零 HP Bowie/Peter/Rohde 的朝向命令可跳过；NPC130 绕过。行动安装、目的地写入和优先级写入没有同样守卫。行动等待解析当前映射，轮询映射行动者空闲指针，不检查所有行动者目的地。缺失/默认零映射不同于显式无效映射，均不修复。`setDest` 将坐标乘以384，写目的地/行程与每轴有符号速度32，并用这些非负实体字等待选定行动者位置等于目的地。既不传送，也不重置行动指针。Peter 目的地等待与另一行动者空闲等待不构成组屏障；祭司未等待移动及 Rohde Up1/第二跳可能尚未结束。睡眠值与速度是源码参数，不是测量的表现时序。

`eas_Init` 将移动/行动表现字段重置为源码默认值；`eas_InitSlow` 使用较慢初始化。均不治疗。`eas_DeactivateAutoFacing` 关闭自动朝向并跳至 idle；相邻源码指令不延长该行动。`eas_Jump` 请求相对 `(0,-1)`、等待5、`(0,1)`、等待5，再 `(0,0)`、开启自动朝向并空闲。每个请求相对当前位置，其间没有显式目的地相等等待。请求跳跃不证明两瓦片往返完成；跟随可替换尚未结束的第二跳。

优先级命令写映射物理行动者的精灵链接排序字节：零→0，非零→1。精灵链接构建器把非零条目放在零条目之前。接受设 Rohde1、Bowie0、再 Bowie1；不恢复已保存旧值。这不是战斗行动优先级或 VDP 瓦片优先位。实际重叠/渲染仍未知。

三个跟随位置编码的随从字为7 或11，**高字节均为零**。处理器把该高字节作为 HP 操作数，因此 **Bowie HP 控制全部选定跟随请求**。Bowie 零 HP 跳过六操作数字节；先前标志/行动仍可运行。Bowie 存活则准入映射解析，不检查 Peter/Rohde/目标的健康或在场。

`AddFollower` 以打补丁的 42-byte 跟随行动替换映射物理随从行动，记录映射目标及 selector2 的 `(-24,0)` 像素偏移，推进分配，并仅在不存在时将物理随从追加至 `EXPLORATION_ENTITIES`。既有条目不重排或移除。移动消费者从目标状态与布局派生后续目的地；安装不是立即放置或跟随已稳定的回执。

FOLLOWER_B31 是逻辑查找映射，**不是物理31，也不自动等于 Peter/Oddler**。可以在 F68 下指向 Oddler、在 F74/F70 下指向 Astral、其他传入映射，或缺失时默认零物理0；后续声明可覆盖它。场景不创建31 或设置 F68。预期 Rohde→Oddler 连接取决于当前查找映射确实指向 Oddler。Peter→Bowie 是独立请求；F84 本身不确立完整链。

后续实体群重建消费 F84→Rohde，作为普通随从表最后声明，并可抑制固定 Rohde 重复。它重建槽位、映射和列表。按标志恢复随从行动则分配连续槽位，不重建列表或解析身份。脚本不调用这两种例程。因此未来实体群可不同于当前安装链，Hassan F65 init 还可再次将映射 Rohde 移至地图外。

## 条件控制返回

**已确认有序返回：**脚本结束时条件等待视野滚动、清除滚动速度并返回。zone 包装器关窗，等待一次 VInt 与物理0 当前位置/目的地相等。实体包装器关窗并重新激活更新，此零标志行不恢复已保存朝向。物品包装器关窗，等待一次 VInt/物理0 对齐，之后 FieldMenu 解释 d6。已处理结果退出菜单；零结果返回物品子菜单。FieldMenu 退出后，`ProcessPlayerAction` 重新激活视野/实体更新。

探索返回 `WaitForEvent`。仅待处理为零时重新以 camera0 为目标并安装受控实体行动；普通玩家类型0 选择步行。场景不规范化传入模式或待处理状态。服务/等待停滞可阻止返回；Bowie 死亡可跳过跟随，但其余服务返回后仍留下已完成接受标志/F727。这些是条件源码后果，不是自然游玩传入此状态或可用输入恢复的证据。

## 证据追踪与审阅边界

**已确认溯源：**已接受 #828/PR830 研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与已注册 USA 零售 ROM。[复现与覆盖所有者](../../../../../research/chapters/southern-travel-creed/hassan-ancestry-and-rohde-caravan-choice.md#reproduction-and-coverage-limits)涵盖100 条选定记录、十二个内联主体、25 个脚本文本位置1372–1396 和两个调用方位置1371。物理语料范围不是执行路线。本指南使用这些已接受关联，不调查原始源码、重生成载荷或采集运行时。

| 说明边界 | 现有所有者与可执行追踪 | 追踪不确立的内容 |
| --- | --- | --- |
| 祭司/交谈/物品调用方 | [地图探索](../../../contracts/world/map-exploration.md)；`sf2-map-events-static-v1` 分片[zone 程序](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)、[实体程序](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)、[物品程序](../../../../../../tests/fixtures/h2/map-events/item-programs.json)：programs148/671/78，全部5/25/6 操作 | 选定行、激活、物品栏服务主体与包装器是已接受研究关联；操作计数不是自然准入。 |
| 完整场景、提示与标志 | [对话](../../../contracts/interface/dialogue-system.md)、[输入](../../../contracts/interface/input-system.md)；[地图脚本夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)中的 `sf2-map-script-engine-static-v1`，[菜单夹具](../../../../../../tests/fixtures/h2/common-menus-static-v1.json)中的 `sf2-common-menus-static-v1` | 跟踪命令事实投影保留位置/操作数；完整 programCorpus/内联主体与文本归属是保留研究选项。菜单/处理器矩阵不是这些选择的传入输入或启用服务观察。 |
| 物品栏搜索与物品结果 | [物品事务研究](../../../../../research/world/map-event-item-transactions.md)；对应[物品](../../../../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)/[属性](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)夹具中的 `sf2-map-event-item-transactions-static-v1`、`sf2-common-stats-static-v1` | 汇总调用方事实保留 Map9 交谈的定位调用与 FieldMenu/d6 接缝；不独立执行 Map9 物品使用、消耗或存储/持久性。 |
| 跟随、实体群与返回 | [名册](../../../contracts/party/party-roster-state.md)、[探索流程](../../../contracts/world/exploration-control-flow.md)、[已接受跟随](../../../../../research/chapters/eastern-parmecia/polca-oddler-choice-and-following.md#current-chain-versus-future-follower-population)；[init](../../../../../../tests/fixtures/h2/map-init-static-v1.json)/[请求](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)夹具中的 `sf2-map-init-static-v1`、`sf2-map-event-request-consumption-static-v1` | 共享命令/服务、实体群与请求事实不证明当前链、全部行动者完成、响应输入或存档/恢复。 |

所有者复现命令包括 `uv run sf2 h2 map-events`、`map-script-engine`、`common-menus`、`common-stats`、`map-init`、`map-event-item-transactions` 与 `map-event-request-consumption`。受控复现于同一进程加载所属工作树忽略的私有输入配置。仅审阅说明不要求复现。

[地图事件夹具加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)组合并验证[索引](../../../../../../schemas/h2/map-events-fixture-index.schema.json)、章节分片与[目标操作 schema](../../../../../../schemas/h2/map-events-target-program.schema.json)；[地图事件验证器](../../../../../../src/sf2tool/h2/map_events.py)拥有其源码/ROM 比较。[地图脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)及 [schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)比较命令事实投影，不是公开完整场景游玩流程。[物品事务验证器](../../../../../../src/sf2tool/h2/map_event_item_transactions.py)与 [schema](../../../../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json)拥有静态调用方编排。本综合审阅选择三个完整调用方程序、选定对话/剧情/行动位置与共享菜单/物品/init/请求事实，并检查其 schema 和消费者。未发现需 Layer A 修正的不一致；汇总相等不闭合自然状态或表现缺口。

[#828 最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160569868)与[独立 PR830 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/830#issuecomment-6160579144)保留通过比较、实际最终 CI、已解决 PowerShell/路径/截断读取检查失败与[原始完整交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160524404)。链接的 PR826/822/818 比较、已修正宏/目的地/检查和被拒命令历史经该研究所有者保留其已完成状态；本切片不重放或替换。

**未知：**自然 zone/交谈/选定物品可达；实际物品栏、标志、HP、映射、列表、布局、待处理/模式及输入/释放/回答；行动/窗口/更新/服务完成、文本/动作、渲染/重叠/音频、可用控制、传说与存档/恢复持久性。这些按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)继续延期。传入标志不是历史回执；未知不自动授权采集运行时证据或扩展至下个剧情节点。
