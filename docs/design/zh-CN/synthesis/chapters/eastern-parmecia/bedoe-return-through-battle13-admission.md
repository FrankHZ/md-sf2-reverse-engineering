# Bedoe 返回至 Battle13 准入

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/bedoe-return-through-battle13-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

返回 Polca 的途中可选中一场对峙：恶魔攻击两名鸟人，Luke 讲述被扣押的经过，Gerhalt 请求加入 Bowie 的军团。场景让 Gerhalt 加入并请求战斗交接。旅行、场景选择、成员资格与战斗准入各有独立条件；其中任何一项都不单独证明连续可玩的返回。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/bedoe-return-through-battle13-admission.md)支持下文选定出口、实体群、文本与服务顺序。**推断：**这些调度描绘对 Bedoe 士兵的攻击，以及 Gerhalt 为保护 Oddler 而介入。**未知：**自然旅行、传入状态、服务完成、渲染动作及可用战斗输入。本指南不增加重制能力或产品决策。Fnnn 表示标志 nnn；坐标单位为地图瓦片。

前序[国王选择指南](volcanon-return-through-bedoe-king-choice.md)说明可提供 F799 的可选接受分支。[雏鸟飞行与救援](bedoe-hatchling-flight-and-rescue.md)仍是独立可选内容：F723 与 F724 都不控制这些出口或对峙。这条阅读路线不要求雏鸟前提或强制顺序。止于新战斗 start 分发器返回、紧接 `ActivateEnemies` 之前；交战、结果、市长、木筏履行和 Oddler 离开仍不在范围内。

接续[Battle13 结果至 Polca 木筏授予](battle13-outcomes-through-polca-raft-grant.md)，了解首次/重复结果、Luke 后续、新城镇入场及可选市长交易。该指南区分跟随与招募/健康，区分有序木筏配置与当前地图存在、河流通行或登乘，止于条件调用方/控制返回。

## 返回 Polca 的方向

**已确认选定请求：**Bedoe 南出口与中间的西出口具有下列有序表匹配。它们描述请求，不描述两者之间的步行路径。

| 选定出口 | 无滚动请求 |
| --- | --- |
| Bedoe/Map10，任意 X、Y31；传送行 34 | Map52 `(28,7)`，Down。更早的行不匹配 Y31；之后的神殿行独立。 |
| Map52，X0、任意 Y；传送行 1 | Polca/Map13 `(30,6)`，Left。更早的行 0 在 `(28,6)` 返回 Bedoe。 |

两个出口都不测试 F799 或雏鸟标志。普通移动需布局属性 `$1000` 才产生传送。生产方写入待处理 type 1、目的地和传送音效；无滚动消费者使用声音服务、使 main 空闲、展开退出探索并将坐标交给 `MainLoop`。`SwitchMap` 此处没有 Map52/Map13 替换，随后运行 `CheckBattle`。服务期间待处理状态可变化；表匹配既不证明自然可达，也不证明请求不受中断。

战斗选择先于普通城镇入场。两张地图各有一条战斗行、通配触发 `(255,255)` 与输出区域 `(0,0,32,32)`。输出矩形不是点准入检查：选择器测试地图、解锁与触发，随后写入该区域。

| 传入目的地状态 | 本次调用的选择 |
| --- | --- |
| Map52，F412 未设 | 普通探索，与完成 F512 独立。 |
| Map52，F412 已设 | Battle12；F512 已设会清除 F412，但本次仍选择 Battle12。 |
| Map13，F413 未设 | 普通探索，与完成 F513 独立。 |
| Map13，F413 已设 | Battle13；F513 已设会清除 F413，但本次仍选择 Battle13。 |

国王选择、F715、Gerhalt 成员资格和雏鸟标志不是战斗准入测试。传入 F413 可在没有此次对峙时选择 Battle13。之前完成 Battle12 不确立传入的 F413/F513 值。

未准入战斗时，普通入场清除待处理、调整步数、治疗存活/不死同伴、保存地图/非战斗状态、重建实体/地图精灵、清除临时 F256–383、设置 F80 并运行地图加载/init。Map52 的 F512 覆盖配置没有固定 NPC，而默认配置有四名；共享随从独立。其 init 测试 F714，随后顺序落入的辅助函数测试 F712：F714 已设/F712 未设时调用已接受 Peter 回应 `cs_5C622`，随后设置 F712。抵达 `(28,7)` 不排除这段中间内容。本指南不重新调查它，也不把抵达当作保证立即向西离开。

## Polca 实体群先于 zone 选择

**已确认配置优先级：**最后设置标志胜出：**F513 > F799 > F710 > default**。四种配置共享 zone/init 处理器；F513 还改变实体事件。源码文件名后缀 709 使用标志 710 符号。写入标志既不重建行动者，也不调用 zone。

F799 已设且 F513 未设时，固定实体群声明 Luke、Gerhalt，随后五名普通 NPC。随从先构建。查找身份不同于物理槽位：

| 查找 | 声明角色及固定起点 |
| --- | --- |
| Bowie / 0 | 独立 main 行动者，位于传入抵达位置 `(30,6)`，Left。 |
| Peter / 7 | 由随从 F67 提供；没有固定 Peter 行。 |
| Luke / 10 | 固定 `(18,15)`、Right、`eas_Init`，除非随从 F75 已映射他。 |
| Gerhalt / 9 | 固定 `(11,9)`、Down、`eas_Init`。 |
| 128 | Dark Soldier，`(22,15)`、Left、`eas_Init`。 |
| 129 / 130 | Minotaurs，`(22,14)` / `(22,16)`、Left、`eas_Init`。 |
| 131 / 132 | 鸟人，`(18,14)` / `(18,16)`、Right、`eas_Init`。 |

随从标志中仅 F67 已设时，物理槽位为 Bowie 0、Peter 1、Luke 2、Gerhalt 3，NPC128–132 占槽位 4–8。没有随从标志时，Luke/Gerhalt 占槽位 1/2，NPC 占槽位 3–7。其他随从组合改变分配。Polca 允许随从但不是大地图；F65 在此不前置车队。

F75 在固定列表之前提供 Luke，因此跳过他的固定行，保留随从位置/行动。**F74 提供 Astral / 31、国王 / 30 与大臣 / 159；不提供 Gerhalt。**这些非同伴行影响分配，并可共享传入查找身份。没有随从行提供同伴 9。F67 未设会让 Peter 没有固定声明。跟随测试标志，而非军团成员资格、出战、HP 或 AI；场景中的名字不能确立上述状态。

即使 F799 已设，F513 仍覆盖对峙实体群：普通查找 128/129 是半人马，130 是河童，131/132 是商人，没有固定 Luke/Gerhalt。默认/F710 实体群也不同。zone 不测试 F513 或行动者可用性，因此传入状态可让场景配上不匹配的实体群。

查找解析对字节掩码、转换 NPC128+ 并读取物理索引映射。清零映射解析为 physical 0；`$FF` 不会被验证为行动者。`setPos`/`setFacing` 仅在**字 HP 为零**时跳过 30 以下同伴，因此 HP 256 非零；NPC 绕过此保护。行动、摄像机和文本请求不会因此获得存在/HP 测试。死亡同伴可能使用的蓝焰精灵既不是复活，也不是成员资格。

共享 init 依次独立测试 F711、F716、F718。F711 未设时运行 `cs_58310`，使 130 面向 Down，等待其动画关闭/Right 方向/精灵更新/空闲行动体，随后重置动画。在 F799 实体群中，130 是 Minotaur。F716 已设使 137 面向 Down；F718 已设将 140 移到内部 `$7000,$7000` 而不删除它。两者都没有固定 F799 行，因此零映射可能影响 main。先前神殿 F711 是可能历史，不能由 F799 推出其谓词。这些 init 效果与服务先于之后的 zone。

## 选择对峙

**已确认有序 zone 选择：**X24/通配 Y 排第一，先于通配 X/Y11、通配 X/Y30 和默认。即使在 `(24,11)` 或 `(24,30)`，X24 仍胜出；处理器返回不会顺序落入下一行。普通移动需 `$1400` 才产生 zone 请求。抵达 `(30,6)` 不会自动执行它。

| `Map13_ZoneEvent0` 传入状态 | 调用方结果 |
| --- | --- |
| F710 未设 | 选择更早 Oddler 抵达分支，执行依赖 F711 的后处理，设置 F710 后返回。本次调用不对峙。 |
| F710 已设、F799 未设 | 返回。 |
| F710/F799 已设、F715 已设 | 返回。 |
| F710/F799 已设、F715 未设 | 执行 `cs_585FE`，脚本返回后设置 F715。 |

该分支不测试 F723/F724、F413/F513、F9/F41、随从标志、HP、AI 或行动者存在。F715 不在场景进入时写入。传入 F715 已设/F413 未设会抑制场景；调用方没有重放或修复合同。

## 对峙及其归属台词

**已确认请求序列：**`cs_585FE` 有 100 条记录：87 个命令、12 次 sleep 及终止符。起始为声音淡出，依次等待 129–132 的速度 48/空闲行动体，等待 131/132 的自动朝向关闭，设置游标 1468 并等待 Peter `eas_Init`。摄像机移动和敌人攻击音乐伴随首轮对话。游标请求全部 19 条文本 1468–1486：16 次 `nextSingleText`、三次 `nextText`（1472,1479,1485）。下表改述归属台词；既不重现逐字台词，也不验证说话者映射身份。

| 文本 | 说话者查找 / 归属 | 内容改述 |
| --- | --- | --- |
| 1468 | 128 / Dark Soldier | 侮辱 Bedoe 士兵。 |
| 1469 | Luke / 10 | 回应侮辱。 |
| 1470 | Luke / 10 | 哀叹同袍遭遇。 |
| 1471 | 128 / Dark Soldier | 称 Luke 为俘虏并威胁他。 |
| 1472 | 128 / Dark Soldier | 警告不要抵抗。 |
| 1473 | 128 / Dark Soldier | 索要受伤男孩。 |
| 1474 | Gerhalt / 9 | 反对。 |
| 1475 | Gerhalt / 9 | 表示愿意战斗并保护 Oddler。 |
| 1476 | Gerhalt / 9 | 呼喊。 |
| 1477 | Luke / 10 | 认出领袖和 Peter。 |
| 1478 | Peter / 7 | 问 Luke 为什么在这里。 |
| 1479 | Luke / 10 | 说自己来取木筏。 |
| 1480 | Luke / 10 | 声称恶魔控制 Polca 并欺骗了他。 |
| 1481 | Gerhalt / 9 | 向 Luke 道歉。 |
| 1482 | Gerhalt / 9 | 说 Polca 人不愿战斗，并请求加入。 |
| 1483 | Luke / 10 | 赞同 Gerhalt 的决定。 |
| 1484 | Luke / 10 | 请求不要忘记自己。 |
| 1485 | 128 / Dark Soldier | 挑衅继续抵抗的人。 |
| 1486 | 128 / Dark Soldier | 威胁杀死他们。 |

Peter 请求使用立绘操作数 `$C0`；其他人用 `$0`。128 的六次、Luke 的七次、Gerhalt 的五次与 Peter 的一次请求，都独立于加入文本 446（`01BE`）；后者使用传入职业/名字索引宣布 Gerhalt 加入。台词提到 Oddler；没有 Oddler 查找/放置/加入命令。Luke 的木筏说法不授予木筏，也不确立先前交付。他的请求不是招募：没有 `join Luke`、F10 或 F42 写入。

Luke 首次回应后，等待其 Right1。Minotaurs 接收 Left3，随后 Right3 行动；129 首次移动不等待，130 则等待。鸟人接收相同成对启动/等待模式的 Left2，随后脚本化 `eas_Die`（启动 131，等待 132）。Minotaurs 和 Luke 面向 Left；Luke 哀叹鸟人，随后 128 以 Left1 接近并威胁俘虏。威胁与索要男孩穿插 sleep、128 的 Up/Down 往复及 Minotaurs 接近。

Gerhalt 反对；更多敌人往复和 Luke 朝 Up 后，摄像机移到 `(6,6)`，Gerhalt 提议保护 Oddler。摄像机跟踪 Gerhalt，同时 Bowie 放置于 `(20,9)`、Peter 于 `(19,8)`，均 Down，受各自 HP 保护约束。Gerhalt 等待 Down3/Right6、`eas_BumpRight`，再 Right1。他面向 Up，摄像机移到 `(14,8)`，随后呼喊。敌人面向 Up；Gerhalt 面向 Right 后，Luke 等待 Up1 并认出 Bowie/Peter。Peter 等待 Down1 后发问。接着是 Luke 的木筏/受骗叙述。Gerhalt 等待 Right1、面向 Down、道歉，随后被跟踪并等待 Up1，再请求加入。

加入先于摄像机跟踪 Luke 及 Luke 的赞同。Gerhalt 面向 Down 并点头；Luke 等待 Up1 并请求别忘记自己。Minotaur 129 等待 Up1/Left1。随后关闭 Luke 自动朝向，并以等待空闲行动体把速度改为 48。他的 Left3 不等待启动；129 等待 Right1/Down1，随后 Luke 行动被等待动画关闭/Left 方向/精灵更新/空闲行动体替换。129 面向 Left 并 sleep 40 后，128 等待 Left1/Up1 并说出最后两项威胁。脚本仅在此后设置战斗解锁、排入传送并结束。

**已确认行动边界：**行动安装器替换指针，而不排队保留旧尾部。等待形式轮询指定实体行动指针是否为 `eas_Idle`。等待 130 不证明 129 完成；等待 132 不显式等待 131。sleep 共请求 260 帧，不含移动/文本/摄像机/加入等待及调试跳过；不是现实经过时长。速度 48 是内部移动参数，不是每帧瓦片数。相对瓦片操作数按 384 内部单位（24 像素）缩放；实际移动仍由布局、碰撞与更新控制。Peter 的 `eas_Init` 重置速度 32、加速/减速、阻挡/碰撞、自动朝向、方向和精灵更新状态，不加入、复活或声明缺失行动者。Gerhalt 的 bump 请求相对 `(1,0)`、等待 5、`(-1,0)`、等待 5、`(0,0)`，重启自动朝向后返回空闲；渲染效果未知。

`eas_Die` 关闭动画运动、循环朝向/精灵更新、请求战场死亡音效和 `MAPSPRITE_EFFECT1`，随后将位置/目的地写为 `(63,63)`、清除行进并空闲。它既不移除映射，也不调用交战 HP/死亡消费者。因此鸟人接收的是脚本化死亡表现与离图放置，不是已证明的交战伤害、奖励、击杀处理或胜利。渲染/音频及 131 并发进度仍**未知**。

Luke 的替换不等待其目的地，也不清除位置、目的地、速度或行进。Motion OFF 将动画计数器写为 `-1`；方向不同于朝向。实体位置更新可独立于动画继续朝旧目的地行进。因此**既不证明 Left3 完成，也不证明冻结在已知坐标**。自动朝向保持关闭、速度 48 保留到后续重置。这里之后没有 `startEntity Luke`。

摄像机目的地清除跟踪并等待滚动；跟踪行动者本身不等待。最后跟踪查找为 Luke，没有存在/存活验证。文本、立绘、台词、窗口、点头、精灵和声音服务保留各自等待与完成限制。终止符的条件滚动等待和速度重置不是全行动者完成屏障。

## Gerhalt 加入，出战独立

**已确认成员资格顺序：**操作码 `$08`、字 9 选择普通加入音乐及 Gerhalt 的 `JoinForce` 服务（经 `j_JoinForce`），不走成对加入 128 分支。服务设置 **F9**，以 `UpdateForce` 重建军团/出战/后备列表，随后比较出战数与 12。低于 12 时，`JoinBattleParty` 设置 **F41=32+9**，不重建这些缓存列表。独立文本 446 服务等待淡入淡出/输入、关闭文本并 sleep 10；它不推进游标 1468–1486，也不会因为 Gerhalt 已加入而消失。此处未编码命名提示。

| 传入出战队伍状态 | 加入结果 |
| --- | --- |
| F41 未设；设置 F9 后出战数低于 12 | F9/F41 已设，但缓存列表仍反映 F41 未设，直到另一次 `UpdateForce`。 |
| F41 未设；出战数至少 12 | F9 已设、F41 保持未设；Gerhalt 在重建快照中仍为后备，不选择替换者。 |
| F41 已设 | 保持已设，包括满员/超员状态；设置 F9 后重建包含 Gerhalt，不驱逐或规范化。 |

这些辅助函数既不测试 HP 或治疗/复活，也不创建地图行动者、改变跟随或写入 AI。场景没有 `joinForceAI`、`followEntity` 或 `addFollower`；F67/F74/F75 与传入军团 AI 不变。Gerhalt 的固定地图存在、F9 成员资格、F41 出战选择、缓存列表、健康与实际放置不同。Peter 与 Luke 保留各自独立状态。不承诺玩家控制的单次行动或特定 Gerhalt 战斗坐标。

## 请求并初始化 Battle13

**已确认交接顺序：**`setStoryFlag 13` 加 400 并设置 **F413**，不是 F13。传送写入待处理 type 1、parameter 1=0 和 Map13/X0/Y0/Right；不切换地图、启动战斗或写 `WARP_SFX`。消费者使用携带的声音值。脚本返回先于调用方 **F715**。zone 包装器随后关闭立绘/文本、等待一次 VInt 并等待 physical 0 停止。

条件链为 **F413 → 待处理传送 → 脚本返回 → F715 → 包装器返回 → 待处理消费 → 战斗选择**。待处理使 `WaitForEvent` 绕过普通 camera 0/受控行动恢复；消费清除待处理并展开返回 `MainLoop`。每项服务及请求的存续仍是条件。停滞可保留前序写入而没有后续完成。标志不是回执，该序列不是原子事务或修复规则。

完整 Map13 `(0,0)` 以 F413 选择行 13、`BATTLE_POLCA_VILLAGE`，输出区域 `(0,0,32,32)`。F513 已设时清除 F413，但本次仍选择。随后是 `BattleLoop`，而非普通 Map13 实体群/init。隐藏的鸟人、地图目的地和 Luke 中断行动不定义战斗实体群。

`BattleLoop` 清除 `PLAYER_TYPE`、设置 F399 并测试 F88。**F88 已设**时恢复保存的秒数、清除 F88/AI 内存、加载保存战斗并直接进入各次行动，绕过新放置和两个包装器；该替代路径不在本次新战斗终点范围。**F88 未设**时清零秒数、保存当前地图/战斗并在 before 包装器之前安装基础 VInt。实际行 13 的 before/start 目标均为 **`ms_Empty`**。

包装器共享 **F463=450+13**。before 只测试、不设置；start 先测试并在分发前设置。F463 已设跳过两个脚本，不跳过中间初始化。F463 未设执行空程序，其终止符仍有条件滚动等待和速度重置。这里没有 Battle13 开场脚本，但空目标不表示服务路径为空。

两个包装器之间，初始化恢复当前地图/战斗、清除区域标志 90–105、治疗存活/不死同伴、先初始化同伴再初始化敌人位置、清除 AI 内存并为战斗员 0 加载战斗。同伴放置先重置 X 并调用 `UpdateForce`，把新 F41 刷新到缓存列表。它消费实际出战列表并跳过零**字** HP。列表推进不同于放置指针推进；剩余尾部分支可调用 `LeaveBattleParty`。最终出战不由场景顺序固定。

已接受选定源码在 `spriteset13` 中声明 12 个同伴放置、16 名敌人、三个 AI 区域和一个 AI 点。12 行是容量，不是具名同伴或保证 12 名存活成员。敌人初始化使用实际战斗条目/类型，而非五名探索 NPC。`LoadBattle` 淡出/加载资源、等待、从已放置战斗员替换实体与映射、初始化视图/精灵/地形/音乐并请求淡入。无效坐标和敌人中立状态影响声明。地图行动/随从及 F799 实体群不保留为战斗状态；AI 声明/内存清除不证明已执行 AI 或玩家控制。start 包装器返回。**止于 `BattleLoop:@Start` 调用 `ActivateEnemies` 之前。**

## 证据审阅与剩余限制

本指南仅从已接受 main 消费 [PR #802](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/802) 接受的研究，以及前序英文国王选择与可选雏鸟指南。研究拥有 USA 零售 ROM 身份、固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、场景 `0x585FE`、zone `0x5823A`、init `0x582E0` 及选定源码/服务组合。其[复现边界](../../../../../research/chapters/eastern-parmecia/bedoe-return-through-battle13-admission.md#reproduction-and-evidence-limits)保留源码/保留载荷比较。本切片审阅已接受正文和限定 tracked 测试夹具选择，不重新调查源码、不复制/生成派生载荷或新增观察。

| 审阅边界 | 既有测试夹具 / 合同 / 消费者 | 结论 |
| --- | --- | --- |
| 调用方 | [sf2-map-events-static-v1:zone-programs](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，program 9；[zone schema](../../../../../../schemas/h2/map-events-zone-programs.schema.json)、[测试夹具加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)。 | 全部 18 个操作保留首次抵达返回与 F710/F799/F715 顺序，场景先于 F715。没有可选场景前提。 |
| Init 与实体群 | [sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，body 18/65；[init 验证器](../../../../../../src/sf2tool/h2/map_init.py)；[地图探索](../../../contracts/world/map-exploration.md)。 | Body 18 的 13 个操作独立测试 F711/F716/F718。Body 65 的两操作边界止于 Map52 辅助函数之前；已接受选定研究提供该顺序落入路径。配置/实体清单不包含完整实体群组合。 |
| 对话、行动者与加入 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)；[对话](../../../contracts/interface/dialogue-system.md)、[出战队伍/名册](../../../contracts/party/party-roster-state.md)；[地图脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)。 | 命令族事实拥有处理器，不拥有完整 100 记录场景/文本语料。已接受研究拥有全部 19 条文本、加入 446、行动中断及 F9/F41/缓存组合；共享运行时边界不是自然场景证明。 |
| 战斗交接 | [sf2-battle-routing-data-static-v1](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[战斗路由合同](../../../contracts/battle/battle-cutscene-routing.md)；[过场验证器](../../../../../../src/sf2tool/h2/battle_cutscenes.py)。 | 路由测试夹具显式保留精确目标未解析；包装器事实拥有共享标志顺序。实际行 13 空目标来自已接受选定研究，与 Battle12 独立。 |
| 战斗实体群 | [sf2-battle-spriteset-data-static-v1](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)；[放置研究](../../../../../research/battle/battle-spriteset-data.md)。 | `rowContentTracked=false`：选定行 13 数量是已接受源码证据，不是逐行测试夹具覆盖或 Gerhalt 放置证明。 |

[Init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)和验证器保留完整体/溯源相等性；[地图脚本 schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)保持命令族独立。既有聚焦检查位于 `tests/python/test_map_events.py`、`test_map_init.py` 和 `test_map_script_engine.py`；本文档范围不重跑它们。

既有窄证据路线为 `uv run sf2 h2 map-events`、`map-init`、`map-script-engine`、`map-data`、`map-setup`、`map-entities`、`battle-routing-data`、`battle-cutscenes` 与 `battle-spriteset-data`；命名它们不是新执行声明。选定检查保留更严格的可执行边界，不把汇总清单扩大为路线观察。

**未知：**自然步行/碰撞及不间断抵达；实际标志、行动者、映射、HP、出战队伍/AI 与位置；并发行动进度；摄像机/文本/精灵/淡入淡出/音频/待处理完成；鸟人死亡或 Luke 姿势/运动的渲染；响应控制、连续 Battle13 可玩性及更广持久性。这些问题不产生采集队列。保留的前序比较/失败仍在研究交接中；本文切片不重跑它们或声称新运行时通过。它既不完成战役，也不完成设计 epic。
