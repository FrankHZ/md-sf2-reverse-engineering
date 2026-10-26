# Polca 仙女互动至 Battle12 准入

> 本文是[规范英文原文](../../synthesis/polca-fairy-through-battle12-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Polca 提供可选遭遇：一只乌龟请求帮助翻身。Bowie 和 Peter 收到编排的移动和推撞请求；脚本请求仙女精灵与马赛克效果时，乌龟最后的台词描述考验和离去。向东离开城镇独立于该互动。在普通 Mount Volcano 入场分支中，之后 zone 可编排 Peter 观察四名搜寻的 devils、他们对战斗痕迹和岩石的叙述，以及与访客的对峙。场景随后请求同地图返回，可能使 Battle12 准入。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/polca-fairy-through-battle12-admission.md)支持下文互动、连接及有序交接。**推断：**它们构成 [Polca 首次 Oddler 抵达](battle11-outcomes-through-polca-arrival.md)之后的候选内容接续。**未知：**自然旅行、提供的标志和行动者、完成的服务、已渲染变身和可玩战斗入场。帮助乌龟与见证 Oddler 抵达均不是选定东出口的必需谓词。本指南不增加重制能力或产品决定。

止于新战斗 start 分发器返回、紧邻 `ActivateEnemies` 之前。交战、结果、`cs_5C622`、Bedoe/Volcanon 场景和后继发现均在本文之外。Fnnn 表示标志 nnn；地图和战斗编号保留源码含义。

继续阅读 [Battle12 结果至 Bedoe 初始卫兵遭遇（英文）](../../synthesis/battle12-outcomes-through-bedoe-entry.md)，了解首领识别、首次/重复山地返回、Peter 独立回应及选定 Bedoe 入场。该指南保留结果、配置/行动者、调用方/服务和自然路径限制，止于国王讲话/觐见之前的初始条件性控制。

## 帮助乌龟是可选互动

**已确认路由：**在 Polca 默认和 F710 实体群体中，普通 NPC 查找 130 是位于 `(1,11)`、Down、使用 `eas_InitSlow` 的 kappa/乌龟。Gerhalt 己方角色行不占用普通 NPC 索引。[前序实体群体指南](battle11-outcomes-through-polca-arrival.md#事件之前的城镇实体群体)保留最后设置标志胜出的配置优先级 F513 > F799 > F710 > default，以及 Peter 由随从选择决定的存在。这些条件决定互动可作用于何者；仅事件标签不确立可用乌龟。

普通实体激活通过 C 在调试/车队替代之后到达，A 则打开菜单。激活使用非负视野目标及沿其朝向偏移的点，跨越柜台图块继续，并扫描排除受控实体与随从的物理实体。候选者与该点的曼哈顿距离必须小于一瓦片，即 384 内部单位。选定物理槽位先经 `ENTITY_INDEX_LIST` 反向映射，之后配置分发器才匹配实体索引。本文未演示自然步行到可用激活点。

普通实体事件 row 3 为查找 130 选择 `Map13_EntityEvent3`。拼作 `RIGHT` 的操作数数值为 0，存储为**事件标志**，不是朝向准入谓词。分发器匹配实体索引；标志位 0 和 1 若设置，会请求暂时朝向玩家并在之后恢复。两位均未设，因此此行既不提供自动转向/恢复，也不要求玩家面向 Right。

调用方有两个分支：

| 提供的 F711 | 处理器行为 |
| --- | --- |
| 未设 | 运行 `cs_58512`；仅在返回后设置 F711，再返回。 |
| 已设 | 请求关于古代魔法隧道的文本 1425（`0591`），然后返回。 |

第二分支不确立已离去行动者仍可再次交谈。F711 已设 init 跳过乌龟调度，但自身不隐藏 130。独立的首次 Oddler 调用方在重建居民之后，F711 已设则隐藏 130，未设则再次编排其姿态。这些是同一标志的不同使用方。

提供的实体群体也改变查找 130 含义。F799 保留普通事件表，但使 130 成为 minotaur。F513 使用另一事件表，其 row 2 将 130 以 flags 0 路由至同一处理器；其 kappa 从 `(1,13)` 开始。这些是选择替代，不证明每种标志组合都有合理乌龟场景。缺失映射可能解析为物理 main 槽位 0，而非安全跳过操作。

F711 不是专属救助历史位。已接受研究记录另一产生方：`Map11_ZoneEvent0` 在 F722 未设时调用其独立场景，然后设置 F722 和 F711。本文仅使用该标志关系；另一场景内容排除。因此仅 F711 既不证明 Bowie 帮助过这只乌龟，也不证明行动者 130 存在。

## 考验与仙女离去

**已确认有序内容：**`cs_58512` 向行动者 130 请求四条文本，ID 1435–1438（`059B..059E`）。其概述内容与调度如下：

1. 乌龟请求帮助翻身（1435）。其阻挡被禁用，行动被等待；摄像机跟踪转为中性。Bowie 和 Peter 收到初始化行动，Peter 被请求到 `(2,12)` 并面向 Left，Bowie 收到第一次带等待推撞。
2. 乌龟请求更用力推（1436）。Bowie 收到点头和两次带等待推撞。乌龟随后鼓励（1437）。推撞行动使用小幅相对运动并等待；其单位不同于场景瓦片步进移动命令。
3. Bowie 被请求 Down1，收到带等待速度行动，再被请求 Up1。Quake 2 和 reset 0 前后夹着一次 sleep。Actor130 收到 `MOSAIC_OUT`、写入 Up 朝向和 `MAPSPRITE_FAIRY=219` 的不等待行动，再收到 `MOSAIC_IN`。又一次 sleep 和 Down 朝向请求后，其最后文本描述考验和离去（1438）。
4. Actor130 收到 `eas_InitSlow`，自动朝向被禁用，Up10 被等待。Bowie 被重置，Peter 被指示跟随他，之后脚本结束。场景内部没有加入、实体群体重载、故事标志或传送命令。

这是四个文本请求的概述，不是全文记录。仙女精灵写入属于内联实体行动，不是招募，也不是地图脚本 `setSprite` 命令。Up10 请求移动，不是删除，也不保证离图放置。调用方之后 F711 写入独立于所有这些请求。

**已确认服务限制：**七次 sleep 共请求 200 帧，不是场景时长。带等待行动轮询 `eas_Idle`，没有通用缺失行动者或 HP 跳过。`setDest` 将瓦片乘以 384，并在此等待运动；它不瞬间放置 Peter。朝向使用字 HP 守卫。Quake 写入幅度。马赛克选择器进入十六次迭代源码循环，调用精灵加载、处理、DMA 和 VInt 服务。该结构及其间不等待的精灵行动均不证明可见变身或时序。

[既有受控效果证据](../../../research/common-scripting.md#confirmed-map-script-entity-presentation-fx-command-family)仅在具名接缝确认选择器进入/循环/返回顺序。该矩阵中解析、加载、裁剪/DMA 等服务被垫片替代，原始主体不执行。它不保证此 Polca 场景渲染或完成。

末尾 `followEntity` 将 Peter 编码为字 `$0007`。处理器探测该字高字节，因此字 HP 检查属于 **Bowie0**，而非 Peter7。Bowie HP 为零跳过操作数。Bowie 存活且映射有效时，选择器 2 添加 Peter 以 `(-24,0)` 跟随 Bowie，独立于 Peter HP。缺失 Peter 可解析为 main。这不写入 F7、F39、F67 或军团 AI 成员状态，也不授予玩家对 Peter 的控制。

此关系与仙女精灵属于当前实体状态。脚本返回且调用方设置 F711 后，待处理为零才允许共享事件等待恢复摄像机目标 0 和受控行动；另一待处理请求会优先取得该次轮询。之后普通入场清除实体/随从，并从持久标志重建。F67 可独立重建 Peter，但这不是保留此临时跟随关系或精灵。场景没有较早 Oddler 事件所用的那种显式重载。

## 离开 Polca 前往 Mount Volcano

**已确认选定连接：**Map13 东传送 row 3，X31/Y 通配，请求 Map52 `(1,28)`、Right，不滚动。Map52 是 Mount Volcano。该行不测试 F710、F711、Battle11 完成或 Peter 成员身份。在 `(31,31)`，较早的 X 通配/Y31 南出口胜出；此重叠处不选择东行。

移动必须遇到布局 `$1000` 属性，传送产生方才扫描有序行。使用方在分发前清除待处理类型、执行声音和运动/返回服务，并将可变地图/X/Y/朝向元组提供给 `MainLoop`，用于 `SwitchMap` 和 `CheckBattle`。Map52 没有按标志切换的替代。匹配坐标或目标行既不证明无碰撞旅行，也不证明参数能在中间服务/写入方之后存续。[请求所有者](../../../research/map-event-request-consumption.md)保留该边界。

Map52 战斗 row 12 的区域为 `(0,0,32,32)`，触发通配 `(255,255)`，解锁 F412、完成 F512。选择器测试触发，不测试是否包含于输出区域。因此完整东出口请求有下列替代：

| 提供状态 | 即时选择 |
| --- | --- |
| F412 未设，F512 任意 | 普通 Map52 入场；本次调用无 Battle12 准入。 |
| F412 已设，F512 未设 | 立即 Battle12，先于普通地图实体群体/init 和搜寻场景。 |
| F412 已设，F512 已设 | 清除 F412，仍在同一次调用中选择 Battle12。 |

F713、F710 和 F711 不是该准入谓词。提供的解锁可绕过场景；完成不否决同次调用选择。下文候选内容描述普通入场分支，而非所有可能的 Battle12 入场。

普通入场清除待处理、调整步数计数器、治疗存活/不死己方角色、设置当前地图为非战斗、重建实体/地图精灵、清除临时标志 256–383、设置 F80，并运行地图加载/init。默认 Map52 提供四个探索行动者，均 Down 且使用 `eas_Init`：Death Archer128 在 `(21,7)`、Hobgoblin129 在 `(18,9)`、Witch130 在 `(23,8)`、Evil Cloud131 在 `(21,15)`。它们是场景阵容，不是战斗名册。

F512 选择**空**固定实体群体，同时保留相同事件/zone/init 指针。F512 已设且 F713 未设时，即使四个查找均缺失，场景调用方仍可被选择。Peter 没有固定行；F67 可独立于已加入/出战标志、HP 与军团 AI 提供他。其他随从改变物理顺序。Map52 接纳随从，但不是大地图，因此 F65 不提供车队前缀。死亡己方精灵替代既不修复映射，也不修复成员身份。这些提供状态风险不是额外准入检查。

init 是另一独立中断。F714 未设则返回；已设则顺序进入 `sub_5C4DC`，后者测试 F712，仅在未设时调用排除的后续场景，然后设置 F712。主地图 init 夹具主体只含最初两条指令；已接受研究的辅助组合提供余下部分。仅抵达坐标和 F412 未设不确立该中断不存在。

## 观察 Devils 并遭到质问

**已确认选择：**点 `(3,23)` 与 `(4,23)` 指向 `Map52_ZoneEvent0`；默认仅返回。zone 产生要求 `$1400`。抵达 `(1,28)` 不自动运行场景。F713 已设则返回；未设则运行 `cs_5C4EE`，脚本返回后设置 F713。调用方既不检查 F512、行动者可用性，也不检查乌龟/Oddler 历史。

脚本请求十三条文本，ID 1397–1409（`0575..0581`）：Peter 两条、Evil Cloud131 三条、Hobgoblin129 两条、Death Archer128 六条。其概述序列如下：

1. Bowie 和 Peter 收到初始化及目标位置请求，分别到 `(5,23)` 与 `(4,22)`；Peter 移动 Right1。他询问陌生人是谁（1397）。摄像机被请求到 `(16,4)`，Hobgoblin129 和 Witch130 则收到反复、不等待的往复行动。Peter 描述他们在搜寻（1398）。
2. Evil Cloud131 移动 Up6，并报告悬崖下未发现目标（1399）。Hobgoblin129 向上/右移动；Death Archer128 被设为面向 Left。Hobgoblin 描述战斗痕迹（1400），并询问 birdmen（1401）。
3. Death Archer128 收到点头，并回应 birdmen 和岩石（1402–1403）。之后更多往复和 sleep。接下来两条文本将岩石归因于 Volcanon，并继续该叙述（1404–1405）。这四个回应转述 devils 的解释，不独立证明较早事件成因。
4. Evil Cloud131 向下移动、收到带等待速度行动，并向下/左靠近。它询问谁在那里（1406）。摄像机转向 Bowie/Peter，两人朝向改变，再返回 devils。Evil Cloud131 重置并移动 Up1，再将访客认作 Polca 孩子（1407）。
5. Death Archer128 向下/左/下移动、贬低 Polca（1408），并与 131 一同面向 Up。其最后文本威胁访客（1409）。脚本随后写入战斗解锁、请求同地图传送，再终止。

这些是概述，不是全文记录，也不证明所有行动者到达请求位置。场景不改变加入、跟随、实体群体重载或军团 AI。Witch130 参与运动调度，没有文本请求。九次 sleep 共请求 215 帧；摄像机、移动和文本服务增加等待。摄像机目标位置等待滚动，文本/调试路径可跳过工作，终止符条件性等待滚动并重置速度。实际阵容、HP、运动、视野、对话、声音及服务完成仍为**未知**。

## 请求分阶段成为战斗准入

**已确认顺序：**最终 `setStoryFlag 12` 加上战斗解锁基数 400，设置 **F412**，而非字面 F12。下一命令写入待处理 type 1 和 Map52 `(0,0)`、Right，不滚动。它不立即切换地图或设置 `WARP_SFX`；消费使用携带的声音值。仅在脚本终止后，调用方才设置 **F713**。

zone 分发器随后关闭立绘/文本、等待一个 VInt 并等待 main 停止。type 1 完整时，探索事件轮询跳过恢复受控输入。`ProcessMapEvent` 清除类型、消费请求并返回 `MainLoop`，后者切换地图并再次检查战斗。因此顺序是 **F412 → 待处理传送 → 调用方 F713 → 请求消费 → 战斗选择**。这不是原子场景到战斗切换：早期服务停滞可阻止写入，后期停滞则可使部分或全部已写入而未消费。中间写入方可改变可变请求。F713 已设/F412 未设不会因再访 zone 自行修复，因为调用方跳过场景。

完整 `(0,0)` 请求通过通配触发，在 F412 已设时符合 Battle12。F512 保留同次调用清除解锁的行为。此分支进入 `BattleLoop`，而非再次运行普通 Map52 init/实体群体。探索行动者位置不是战斗放置。

## 空开场仍有战斗生命周期

**已确认新战斗交接：**`BattleLoop` 清除玩家类型、设置 F399 并测试 F88。提供的 F88 已设走恢复战斗分支：恢复已保存秒数、清除 F88/AI 内存、加载已保存战斗状态并跳到单独回合（单次行动），绕过新放置与两个介绍包装器。该分支在本指南新战斗终点之外。

F88 未设时，循环在 before-battle 包装器前将秒数归零、存储地图/战斗并安装基准 VInts。Battle12 before 和 start 行均选择 `ms_Empty`。共享介绍标志 F462 未设，允许 before 分发而不设置；start 先设置 F462 再分发。F462 已设跳过两个脚本，但不跳过其间初始化。空脚本仍执行终止符的条件性视野滚动等待及滚动速度重置。

before 返回与 start 分发之间，循环恢复地图/战斗、清除区域标志 90–105、治疗存活/不死己方角色、先初始化己方再初始化敌方位置、清除 AI 内存，并为战斗员 0 加载战斗。实际己方放置在 `UpdateForce` 后使用出战列表、测试字 HP 并跳过死亡成员；列表与放置指针推进不同，剩余尾部可将成员移出出战队伍。精灵集声明十二条己方放置行和十二名敌人。它既不保证十二名己方出战，也不保证复用四个场景行动者。Peter 的 F7/F39、HP 与军团 AI 独立于 F67 故事跟随和脚本参与。本内容未在任何地方招募他，也不授予玩家对他的控制。

战斗加载器淡出、加载瓦片集并等待、以战斗实体替换场景实体、选择映射视野目标、初始化精灵、加载地图/地图精灵、恢复基准 VInts、加载地形、播放地图音乐并请求淡入。start 包装器随后返回。**止于 `BattleLoop:@Start` 调用 `ActivateEnemies` 紧前。**敌人激活、回合（单次行动）、交战、结果、完成的呈现和可用战斗输入均未被本指南接受。

## 证据审阅与剩余限制

本指南使用 [PR #778](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/778) 接受的研究及 [PR #777](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/777) 的前序英文指南，没有堆叠依赖或使用后继内容。研究拥有固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、USA retail 身份、精确源码/文本/行组合与[复现边界](../../../research/polca-fairy-through-battle12-admission.md#reproduction-and-review-boundary)。有界审阅使用已接受文字、选定受跟踪夹具及其合同/schema/验证器；不需要生成/复制可选载荷、重新调查源码或运行时观察。

| 审阅关系 | 既有证据及其限制 |
| --- | --- |
| 互动与调用方 | [sf2-map-events-static-v1:routing-setup](../../../../tests/fixtures/h2/map-events/routing-setup.json)、[entity-programs](../../../../tests/fixtures/h2/map-events/entity-programs.json) program41 `Map13_EntityEvent3`，以及[地图探索合同](../contracts/map-exploration.md)。保留 event flags0、实际查找和脚本返回先于 F711。选择证据不证明自然可交谈。 |
| 场景命令与跟随 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、[出战队伍/名册合同](../contracts/party-roster-state.md)、[sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json) 和 [sf2-map-script-entity-presentation-fx-runtime-v1](../../../../tests/fixtures/h3/map-script-entity-presentation-fx-v1.json)。公开族位置/处理器事实不是完整场景记录；受控 HP/效果接缝不保证此组合场景或已渲染仙女。 |
| 出发与普通入场 | [sf2-map-data-static-v1](../../../../tests/fixtures/h2/map-data-static-v1.json)、[sf2-map-setup-static-v1](../../../../tests/fixtures/h2/map-setup-static-v1.json)及 [sf2-map-entities-static-v1](../../../../tests/fixtures/h2/map-entities-static-v1.json)。研究提供精确传送/实体群体关联；清单不证明自然旅行或当前阵容。 |
| init 与场景请求 | [sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json) body65 及 [sf2-map-events-static-v1:zone-programs](../../../../tests/fixtures/h2/map-events/zone-programs.json) program97 `Map52_ZoneEvent0`。两指令主 init 主体不含辅助顺序落入；研究拥有该组合。有序 zone 操作确立先脚本后调用方 F713，不确立请求消费。 |
| 战斗选择与 start | [sf2-battle-global-data-static-v1](../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[sf2-battle-cutscenes-static-v1](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[sf2-battle-spriteset-data-static-v1](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)及[战斗生命周期合同](../contracts/battle-control-lifecycle.md)。汇总 routing 保留 `routeTargetsParsed=false`；研究提供精确 row12/空目标和放置组合。不隐含可玩 Battle12 观察。 |

[事件夹具加载器](../../../../src/sf2tool/h2/map_events_fixture.py)和[分片 schema](../../../../schemas/h2/map-events-fixture-shard.schema.json)保留分片身份与有序程序操作数；[地图 init 验证器](../../../../src/sf2tool/h2/map_init.py)和 [schema](../../../../schemas/h2-map-init-static-fixture.schema.json)保留有界主体，而非完整事件运行时轨迹。[脚本 schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)与[验证器](../../../../src/sf2tool/h2/map_script_engine.py)保留处理器和族位置边界。窄所有者包括 `uv run sf2 h2 map-events`、`map-init`、`map-script-engine`、`map-data`、`map-setup`、`map-entities`、`battle-global-data`、`battle-routing-data`、`battle-cutscenes` 和 `battle-spriteset-data`。这些是复现路径，不主张本纯文字切片进行了新提取或执行。有界解释不需要修正 Layer A。

**未知：**自然互动及从 Polca 穿过山地 zone 的无碰撞旅行；实际标志、行动者、映射、HP、出战队伍和 AI；无中断入场与请求存续；行动/文本/摄像机/待处理/音频/淡入淡出完成；地震/马赛克/仙女外观及时序；连续可玩的 Battle12 入场。这些仍是限制，不是采集队列。本指南止于敌人激活之前，既未完成战役，也未完成更广设计 epic。
