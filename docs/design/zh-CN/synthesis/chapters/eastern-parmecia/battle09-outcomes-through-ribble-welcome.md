# Battle09 结果至 Ribble 欢迎场景

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/battle09-outcomes-through-ribble-welcome.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle09 胜利后，出战队伍返回大地图，位置由战斗状态导出。另一条独立连接通往 Ribble，一名多疑的年轻人与村长在那里质问新来者。Peter 解释灾难后抵达此地的经过，表明自己是 phoenix，村庄的反应随之改变。**已确认静态内容：**选定脚本替换居民、更改四处门图块，并恢复 Peter 当前的跟随关系。**推断：**这是名为 `BATTLE_TO_RIBBLE` 的遭遇的叙事接续。**未知：**自然连续旅程、成功的行动者调度，以及这些变化的可见完成。

本文承接[New Granseal 出发至 Battle09 准入](new-granseal-departure-through-battle09.md)。已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/battle09-outcomes-through-ribble-welcome.md)提供结果、入场、对话及调用方顺序。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#759](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/759) 的 Standard 严谨度协议。本文是在已接受证据之上的内容/设计指南，止于村长调用方之后及首个条件性控制恢复边界；May 招募与后续旅行需要后继证据。Fnnn 表示标志 nnn；战斗 ID 使用源码从零开始的表行。

## 1. 战斗结算返回探索

**已确认静态内容：**战斗循环在行动和回合（单次行动）后效果之后，统计已放置且存活的战斗员。Bowie 死亡会强制将计入的己方总数置零，败北判定先于胜利。Battle09 要求仍有计入的己方且没有计入的敌人。仅击杀敌方槽位 128 不足以确立胜利。其首领存在行是零，因此结束包装器不会将全部剩余敌人追加到死亡列表。

| 结果 | 状态变化与返回 |
| --- | --- |
| 胜利 | 治疗存活/不死己方角色，然后从当前地图、`UpdateForce` 后首名出战队伍成员、战斗区域原点及该成员映射的实体导出地图/X/Y/朝向。调用 after 包装器，清除 F409 并设置 F509；以 D4=1 返回。 |
| 败北 | 呈现败北消息，恢复 Bowie HP 至最大值，通过无符号右移将金币减半，并以 D4=-1 返回 egress 元组。不新设 F509。Battle4 特殊分支不适用。 |
| Egress / Angel Wing | 消耗法术 MP 或物品，调用逃脱服务，并以 D4=0 返回 egress 元组。此路径不施加败北的 HP/金币变化或胜利的完成写入。 |

Battle09 在 Map69 的区域原点为 `(0,0)`，因此胜利使用选中成员在该大地图上的战斗坐标与朝向；成员并未硬编码为 Bowie，返回点也不是固定城镇入口。胜利治疗不会复活死亡且非不死的己方角色。败北/逃脱则使用携带的 `EGRESS_MAP` 及其存档点行；若没有匹配行，则使用 `(1,1)`、Up。本分段不确立特定的最近存档点。首次战斗败北/逃脱时，F409 保持提供值；共享解锁辅助仅在 F509 原已设置时才清除它。

defeated 包装器仅在 Bowie 存活、槽位 128 死亡且 F509 未设时分发空的 row-9 脚本。after 包装器也有空的 row-9 目标，受 F509 门控。它们的空内容不写入 Ribble 目标。存在对话窗口时，解释器终止符仍可能等待视野滚动并清除滚动速度，因此空脚本不代表服务瞬间完成。共享 after 尾部即使跳过脚本也会执行：row 9 为 Bowie 调用 `JoinForce`，设置 F0、更新军团，并在容量允许时条件性设置 F32。它既不招募 May，也不改变 Peter 成员身份。

任何战斗返回后，`alt_MainLoopEntry` 调用 `SwitchMap` 并进入探索，不会再次立即 `CheckBattle`。因此携带的 F417 不会截获这次即时 Map69 胜利返回。普通的提供地图探索清除待处理、重建实体/精灵、清除临时标志 256–383、治疗存活/不死己方角色，并在 init 前加载地图。Map69 没有替换，其默认固定列表为空，init 直接返回；提供的随从仍可能存在。服务返回后，待处理为零且普通玩家类型为 0，才允许下一次控制交接。[战斗生命周期](../../../../../research/battle/battle-loop.md)与[探索合同](../../../contracts/world/exploration-control-flow.md)保留更广边界。

## 2. Ribble 是独立目标

**已确认静态连接：**Map69 的精确传送行 `(44,7)` 请求 Map8，即 Ribble，位置 `(15,30)`、Up，不滚动。步行必须遇到布局属性 `$1000`、匹配该行，并将完整 type-1 请求保留到无滚动使用方返回 `MainLoop`。连接行不确立从可变胜利位置到此处的可达路径。

F509 已设时，Map69 默认 zone 不再产生 Battle09 重载，但特定 X23/24/25 zone 仍调用 Battle17 随机辅助。默认处理器不清除携带的 F409。在之后一次 Map69 准入中，解锁的 row 9 仍先于 row 17；即使 F409/F509 均已设，也是在同一次调用中清除解锁并返回 9。[前序指南](new-granseal-departure-through-battle09.md#5-有序战斗选择决定准入)和[随机战斗所有者](../../../../../research/world/map-event-random-battle-state.md)保留这些可能的中断。完成不证明可以不受打断地走到 Ribble。

Map8 既没有按标志切换的替换，也没有战斗地图行。因此，存续的选定请求在提供的城镇位置进入普通入场：重建行动者、清除包括 F258 的临时标志、加载布局/标志复制，并在读取待处理/控制状态前运行 init。init 既不请求欢迎场景，也不请求新传送。传送与两个欢迎处理器均不检查 F509，因此 Battle09 胜利不是任意提供的 Ribble 访问所编码的门控。实际可达性及存续的请求谱系仍为**未知**。

## 3. 入村选择居民与随从

**已确认静态内容：**每次配置查找时，F708 选择改变后的 Map8 配置。默认与 F708 配置共享 zone、描述、物品和 init 目标，但实体列表及实体事件目标不同。实体事件文件名中的 `_518` 不是 F518 配置条件。

| 配置 / 提供状态 | 实体群体或 init 效果 |
| --- | --- |
| 默认，F708 未设 | 两条固定 NPC 行：老人 128 在 `(14,7)`、Down，年轻人 129 在 `(13,26)`、Right。对话将他们识别为村长与质疑者。 |
| F708 已设 | 十四条源码行：十三条普通 NPC 行加 May。相关位置包括老人 `(37,8)`、在 `(14,23)` 附近游走的年轻人，以及在 `(18,27)` 附近游走的 May。这些是列表行，不保证可见行动者数或物理槽位数。 |
| 按标志选定的随从 | 实体初始化在固定列表之前添加提供的随从；F67 选择 Peter。其他随从标志仍是输入。Map8 不使用 F65 选择的大地图车队前缀。 |
| 普通 init 时 F8 已设 | 为 May 调用 `MoveEntityOutOfMap`，通过她当前映射写入 `(0x7000,0x7000)`。这不使她加入、不设置 F8，也不确立行动者存在；默认配置没有编写的 May 行。 |

Character 128/129 是查找身份。随从先占用物理槽位，普通 NPC 从 128 起取得事件身份，而既有己方角色映射可使己方角色行被跳过。Bowie 占用物理槽位 0。行动者名称和源码列表不证明脚本会得到有效角色阵容。

Peter 前序觐见提供 F7 成员身份、AI 控制位和持久随从标志 F67；出战队伍 F39 仍有条件。招募、战斗出战状态、AI 控制、持久地图重新实例化，以及当前安装的跟随行动必须保持区分。[觐见指南](peter-castle-audience-through-town-return.md)和[出战队伍/名册合同](../../../contracts/party/party-roster-state.md)拥有这些区别。

## 4. 怀疑与和解具有独立门控

**已确认静态内容：**普通移动需要 zone 属性 `$1400`，选定行才能产生 type-6 事件。提供的行坐标不是自然激活证据。

| 欢迎 zone | 脚本返回后的门控与调用方状态 |
| --- | --- |
| `(15,28)` 或 `(16,28)` | 若 F708 或 F258 已设则返回。否则运行 `cs_563B2`，然后设置 F703 和 F258。 |
| 村长 zone `(14,8)` | 若 F708 已设则返回。否则运行 `cs_56422`，然后设置 F708。没有 F703、F258 或 F709 前提。 |
| 默认 | 返回。 |

首个场景让年轻人靠近、怀疑新来者并呼叫村长。正常文本流是 1291–1293。脚本等待指定的右/左/上移动，改变朝向和速度，最后将他放在 `(15,7)`、Down。这一放置不证明较早路径可通行。F703 仅在解释器返回后写入，且已被 Map14 的木板/甲板 init 使用；此处理器从不测试它。既有 F703 既不抑制质疑，也不确立此前访问过 Ribble。

F258 在当前入场内抑制重复质疑。普通重新入场清除它，因此只要 F708 仍未设，场景就可再次运行。即使质疑从未运行，村长场景也可独立通过其局部门控。对话中合理的顺序是**推断的叙事流程**，并非强制前提链，也不证明替代路径上的行动者有效。

## 5. Peter 解释来由并改变反应

**已确认静态对话内容：**村长场景先将 Peter 放在 `(14,10)`、Up，禁用 Bowie/Peter 自动朝向，并调整年轻人的速度。五次各 30 的暂停分别先于五轮 Bowie 向 Down 后退、村长向 Down 碰撞/移动。Peter 在第 2–5 轮收到不等待的 Down 行动。村长移动会被等待；这些 Bowie/Peter 启动并未逐一等待汇合。Peter 最初的普通 `setDest` 确实等待移动。

村长质问出战队伍，年轻人报告士兵，文本 1294–1303 支持这一质疑。年轻人的靠近序列会被等待。Peter 随后反驳并质疑他们的假设（1304–1307），在村长回复（1308）前后移动，解释灾难导致的抵达（1309–1312），并表明自己是 phoenix（1314）。村长与质疑者在 1313 和 1315–1320 中改变反应。编写的交流没有明确选择或拒绝分支。**推断的玩家含义：**Peter 的解释化解误会，并向村庄介绍新来者；这不是已演示的分支说服机制。

脚本在阶段之间初始化行动者并明确改变朝向。村长的 Up 移动不等待，年轻人的则等待。两人的成对惊讶跳跃同样等待年轻人，而非村长。指定等待不证明每个行动者的行动均已完成。行动/序列等待依赖映射行动者达到 `eas_Idle`；查找不是通用的存在或死亡行动者检查。缺失的 Peter/NPC 映射、提供的行动者状态及死亡己方角色仍是重要调度限制。

对话使用编写的 `{W1}`/`{W2}` 控制符、换行和具名替换。这些控制符不是测得延迟。`nextText` 保持共享生命周期开放；`nextSingleText` 关闭它，并在显示后 sleep 10。正常显示推进游标；调试跳过文本则绕过显示、游标递增和 sleep 记录。本指南描述正常编写的流，不主张其自然时序，也不重现对话全文。

## 6. 改变后的村庄具有多种独立状态效果

**已确认静态顺序：**和解后，脚本淡出、加载 `ce_55FBE`、初始化 Bowie、复制四处图块、设置 F709、调用 `j_csub_40F2`、安装 Peter→Bowie 跟随、sleep 50、淡入并终止。此后村长调用方才设置 F708。

实体重载使用显式指针，而非 F708 配置选择器。`ce_55FBE` 含 main `(14,13)`、Up，存储上紧接 `s1_entities_708.asm`。`csc42_loadMapEntities` 读取三个 main 字并从后续地址重建，包括提供的按标志选定随从和十四行实体群体，此时正常调用方 F708 仍未设。它在实体/地图精灵重建前后施加显示/中断及 VInt 控制。它不进行新传送、`LoadMap`、临时标志重置或地图 init。尤其不会重跑依赖 F8 的 May 隐藏。因此提供 F8 已设状态，不保证本次重载后 May 立即隐藏。之后普通入场才使用 F708 选定配置，并再次运行该 init。

四次即时 `setBlocks` 复制先使用源 `(32,2)`，在 `(25,12)`、`(22,24)` 和 `(7,25)` 进行三处 1×1 编辑，再使用源 `(32,3)`，在 `(6,15)` 进行 2×1 编辑。它们改变工作地图图块，并设置两个视野平面更新位。F709 的四条标志事件行在后续地图加载时重建同样编辑。F708 选择居民/事件；F709 重建门变化。任一均可在没有另一标志时被提供。村长门 `(14,9)` 的踩踏表复制独立于这四处编辑。工作布局写入和排队刷新不证明门可见打开或碰撞可通行。

`j_csub_40F2` 仅在 busy 字非零时恢复保存的屋顶矩形。它清除此状态、读取保存的目标/尺寸，并将保存的字复制回工作布局，然后发送 `MUSIC_NOTHING` 并设置平面更新位 0。非活动缓存直接返回而不做这些工作。村长门 `(14,9)` 的屋顶行使用 `(13,33)` 处的 7×8 矩形，但此路径不证明它曾自然激活，或保存缓存属于它。另一个重叠的保存矩形可能影响前面的编辑。这是条件性屋顶恢复，不是门解锁机制、实体重载或保证的完整重绘/音乐改变。[地图图块生命周期所有者](../../../../../research/world/common-scripting.md#confirmed-map-block-copy-showhide-lifecycle)保留有界辅助证据。

末尾 `followEntity ALLY_PETER,ALLY_BOWIE,2` 编码三个字 `$0007,$0000,$0002`。其 HP 探测读取第一个字的高字节，即零，因此 Bowie HP=0 会跳过命令。Bowie 存活时，Peter HP=0 本身不会跳过：处理器解析当前 Peter/Bowie 映射，安装选择器 2、偏移 `(-24,0)` 的跟随行动。这些是不同的区分状态，不是自然死亡行动者或缺失映射行为的证据。命令不设置 F67、不让 Peter 加入、不设置 F39，也不改变 AI 控制；F67 管理后续重新实例化。此守卫不得推广到所有前面的行动者命令。[研究所有者](../../../../../research/chapters/eastern-parmecia/battle09-outcomes-through-ribble-welcome.md#mayor-scene-through-explicit-reload-and-control)绑定宏/处理器及既有受控高字节观察；本文不主张新运行时证据。

解释器返回后，调用方设置 F708。zone 分发器关闭立绘/文本、等待一个 VInt 和物理实体 0 的移动，然后返回探索。它既不等待汇合所有其他行动，也不无条件清除新待处理工作。两个选定场景均未明确请求新事件。下一次 `WaitForEvent` 时，待处理为零才跟随 main，并为携带的 `PLAYER_TYPE=0` 安装普通角色控制；非零待处理先分发。这是首个条件性控制恢复边界，依赖被调用服务返回，不是观察到的响应输入或完整淡入淡出/随从运动。之后普通 F708/F709 重新入场重建居民和门复制、清除 F258 并运行 F8 init，同时两个欢迎门控均保持抑制。

## 证据审阅与剩余限制

已接受研究绑定 USA retail 及固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`。场景专属行、文本和存储相邻关系提供组合依据；受跟踪夹具提供更窄的可执行接缝。直接审阅保留其 schema/使用方限制，未发现必须修正的研究或合同。尤其是，源码位置命令索引不是研究中从一开始的完整脚本记录索引，公开引擎夹具也不是完整对话/动作编排记录。

| 审阅关系 | 可执行/合同所有者 | 保留边界 |
| --- | --- | --- |
| 战斗结算与空包装器 | [sf2-battle-loop-static-v1](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`expected.lifecycleFacts`；[sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`expected.cutsceneFacts`；[过场合同](../../../contracts/battle/battle-cutscene-routing.md)。 | 共享计数/门控/加入接缝；row-9 组合与可变返回来自已接受研究，不含实际游玩的遭遇。 |
| 实体群体选择与 May init | [sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-entities-static-v1](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)、[sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`expected.primarySourceBodies[12]`。 | F708 选择、分配/映射及 F8→hide 调用；不保证行动者存在或招募。 |
| 独立欢迎处理器 | [sf2-map-events-static-v1](../../../../../../tests/fixtures/h2/map-events-static-v1.json) 与 [zone 程序分片](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，`expected.zoneTargetPrograms[145..147]`。 | 精确 F708/F258 检查及脚本返回后标志顺序；不证明布局可达或强制先质疑。 |
| 显式重载、图块与跟随 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`expected.entityPopulationCommandFacts`、`mapBlockMutationCommandFacts`、`entityGestureRelationshipMotionCommandFacts`；[地图/探索](../../../contracts/world/map-exploration.md)与[出战队伍/名册](../../../contracts/party/party-roster-state.md)。 | `cs_56422` 源码位置及不同处理器/操作数事实；已接受研究拥有先重载后 F708、存储相邻及无 init 组合。 |
| 保存屋顶恢复 | [sf2-map-block-copy-lifecycle-runtime-v1](../../../../../../tests/fixtures/h3/map-block-copy-lifecycle-v1.json) 及公共脚本所有者。 | 受控活动/非活动辅助接缝不证明村长的自然屋顶缓存或可见门。 |
| 请求与首个控制读取 | [sf2-map-event-request-consumption-static-v1](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`eventRequestConsumption.consumerContexts.waitForEvent` / `processMapEvent`；[请求所有者](../../../../../research/world/map-event-request-consumption.md)与探索合同。 | 待处理轮询/分发前清除及使用方生命周期，不含组合的行动者/服务完成。 |

[地图 init 验证器](../../../../../../src/sf2tool/h2/map_init.py)及 [schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)、[地图脚本引擎验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)及[夹具 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)、[请求消费验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)及 [schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)拥有有界结构。所属命令是 `uv run sf2 h2` 后接 `map-init`、`map-setup`、`map-entities`、`map-events`、`map-script-engine`、`battle-loop`、`battle-cutscenes` 或 `map-event-request-consumption`。**本设计切片未重跑这些提取/运行时轨道。**[研究复现边界](../../../../../research/chapters/eastern-parmecia/battle09-outcomes-through-ribble-welcome.md#coverage-discriminators-and-reproduction)保留精确源码/文本锚及先前比较；可选派生载荷在此既不需要，也不生成/复制。

**未知：**自然胜利位置→传送→欢迎可达性、碰撞与 zone 属性；可达的标志/随从/出战队伍历史；提供、缺失或死亡的行动者及未汇合行动；屋顶缓存身份和重叠；完整待处理状态及所有移动/加载/对话/淡入淡出/音频服务完成；可见村庄变化、响应控制及存档持久性。这些限制不创建新采集队列。May 招募、其他 NPC/服务/宝物、神殿/木筏及后续旅行仍在本指南之外。本文既未完成战役编年，也未完成 epic #514，不改变重制能力或引擎/产品选择，英文原文将中文同步留待路线图另行安排。

继续阅读 [Ribble 可选 May 招募至 Battle10 准入](ribble-may-through-battle10-admission.md)，了解可选互动、不同的成员身份/当前跟随、旅行建议及选定条件遭遇路径。该指南保留自身的请求、行动者和自然路径限制。
