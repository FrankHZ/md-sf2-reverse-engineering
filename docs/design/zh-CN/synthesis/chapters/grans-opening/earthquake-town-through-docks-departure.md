# 地震城镇出口至码头出航

> 本文是[规范英文原文](../../../../synthesis/chapters/grans-opening/earthquake-town-through-docks-departure.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

离开地震城镇后，前方港口的实体群由后续标志选择。**推断的叙事连接：**王室一行准备撤离，选择出发，并在船离岸时目睹 Granseal 毁灭。**已确认静态内容：**选定城镇出口请求 Map44；一名特定老人实体提供出航选择；Yes 运行有序场景，清除随从/地震标志并产生 Map73 请求。**未知：**合法行走、自然交互、场景服务完成及出站使用方实际读取的请求。

本文承接[塔后返回至地震城镇入场](post-tower-return-through-earthquake-entry.md)。已接受的[研究所有者](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md)提供溯源、选定记录、对话审阅和使用方顺序。本指南支持内容/设计规划，止于向主循环的条件 Map73 请求交接，不增加重制能力。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#727](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/727) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。原版源码引用使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。下方命令索引是从零开始的脚本记录，不是帧数或实测时长。对话依据已接受研究概述。

## 1. 东出口请求改变后的港口

**已确认选定连接：**Map4 东边缘 `(50,23)`、`(50,24)`、`(50,25)` 记录均请求 Map44，即 `MAP_GRANSEAL_DOCKS_AND_SHIP`，目的地为 `(1,25)`、Right、无滚动。这些记录延续前篇条件城镇控制边界，不证明跨越改变后城镇布局的路线。普通首次胜利沿袭提供 F507/F74/F640，并在战前清除 F83。重复胜利、跳过 intro 和外部提供的标志保留各自调用方限制。

场地请求退出探索，随后 `MainLoop/@Start` 先调用 `SwitchMap`，再调用 `CheckBattle`。Map44 没有替换行，也没有第一列地图字段为 44 的遭遇行，因此将此选定请求交给探索。遭遇表其他位置的 44 是尺寸，不是另一个遭遇谓词。这一局部结果不建立自然旅程。

显式 Map44 入场时，探索清除待处理事件、治疗存活/不死己方角色、加载选定内容，并在配置 init 前初始化新的实体群。Map44 标志/踩踏事件表只有终止符，不提供创作中的登船路径布局复制证据。[入场审阅](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#town-exit-and-map44-admission)拥有生产方、无滚动使用方及准入顺序。

**已确认配置优先级：**Map44 配置行依次是 default、F609、F506、F507。选择器扫描所有标志行并保留最后一个已设的行，因此即使 F609/F506 已设，F507 仍胜出。没有 F507 时，F506 优先于 F609；否则应用 F609 或 default。F507 是内容选择器，不证明撤离或其前提已完成。

其 init 调用 `cs_54570`：请求首领攻击音乐，`fadeInB`，结束。它顺序落入也名为 `ms_map44_flag609_InitFunction` 的返回；default/F609/F506 直接进入该返回。源码所有者是 `s6_initfunction_507.asm`。这个短 init 没有首次入场锁存、行动者放置或传送。F507 重入会重复它。探索自身的音乐/淡入淡出路径仍取决于调色板；这些调用不建立最终听到或看到的结果。

实体重建期间，F74 启用 Astral (`FOLLOWER_B`)、King Granseal (`FOLLOWER_A`) 和 Minister (`FOLLOWER_D`)。Map44 允许这些标志选择的随从。F74 缺失则不声明这一组；F68/F70 也可使用 B，F77 使用 A，F83 在 F74 之后使用 A/D。额外声明可能覆盖所寻址随从映射。首次胜利的 F83 未设状态有意义，但不证明其他标志均未设；该场景不修复缺失/冲突映射。随从是当前行动者，不是新军团招募；仅 F507 不建立场景期待的说话者。

init 后，只有待处理字读取为零，`WaitForEvent` 才安装实体 0 控制。普通 `PLAYER_TYPE=0` 选择角色移动；任意木筏/Caravan 输入不同。待处理事件会绕过控制安装。早先清除以及 init 没有显式生产方，并不证明较后读取为零。这些[配置/随从/控制边界](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#setup-followers-and-conditional-control)仍是内容解释的条件。

## 2. 登船保留行动者；离开再入场会重建

**已确认创作登船记录：**三个无滚动传送门把码头标记连接到船上标记，另有三个返回传送门：

| 触发位置 | 请求位置与朝向 |
| --- | --- |
| `(12,11)`、`(13,11)`、`(14,11)` | 分别为 `MAP_CURRENT` 的 `(52,8)`、`(53,8)`、`(54,8)`、Up |
| `(52,9)`、`(53,9)`、`(54,9)` | 分别为 `MAP_CURRENT` 的 `(12,11)`、`(13,11)`、`(14,11)`、Down |
| 西边缘 `(0,255)` | Map3 `(49,24)`、Left；Y=255 是通配符 |

`MAP_CURRENT=255` 选择同地图路径。遭遇检查使用 `CURRENT_MAP`，因此 Map44 缺少遭遇行这一点仍适用。探索更新 Bowie 位置/朝向，并调用 `DeclareRaftEntity`，后者可以重新放置随从并按条件声明木筏。它跳过新配置实体群构建和临时标志清除，但仍加载当前地图属性并运行配置 init。因此登船保留实体群，并重复 F507 的音乐/淡入淡出请求；不保证行动者坐标完全相同。

从西侧离开，再经显式 Map44 请求返回，会重建选定实体群。F640 已设时，西侧 Map3 请求变成 Map4；F640 未设则保留 Map3。那些返回 init 细节仍属前篇。同地图登船不是显式 Map44 重入。

**未知通行与占位：**F507 实体群在三个码头触发坐标上均有固定行动者。这种重叠是不能仅从传送记录推断畅通的具体原因。碰撞、随从状态、行走及服务完成仍未证明。[登船/交互审阅](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#boarding-records-and-actual-entity-interaction)拥有这些记录和各自使用方。

## 3. 出航选择由老人提供

**已确认交谈对象：**F507 实体群中第十一条固定 NPC 记录为老人精灵，位于 `(10,14)`、Right、`eas_Init`，映射到事件索引 **138**。前一条记录是位于 `(20,12)` 的工人 137，后面是守卫 139/140。`Map44_1FB_EntityEvent7` 是索引 138 的处理器；其后缀既非 NPC 槽位 7，也非事件索引 7，且不绑定工人。

事件表的 `UP` 字节值为 1，被解释为行为位：处理器前转向玩家，之后不执行 bit-1 朝向恢复。它不要求玩家站在南面或朝 Up。普通 C 按钮路径仍需 `GetActivatedEntity` 根据朝向/位置、柜台图块和随从排除输入选择行动者，再由 `GetEntityEventIndex` 提供表索引。使用方在处理器期间挂起实体更新 VInt 处理，之后关闭窗口/重新激活更新。记录映射既不证明自然激活，也不证明等待完成。

处理器向王室一行问候 (1008)，选择随从 B 的立绘/对白提出问题 (1009)，然后调用 `j_YesNoPrompt`，有效目标为 `YesNoPrompt`。附近撤离文本报告损坏、紧迫与拥挤；它不是正式容量规则。处理器不增加 F74、F640、物品栏或出战队伍就绪谓词。

| 选择 | 已确认输入/结果及局部后果 |
| --- | --- |
| Yes | 初始选择的结果为 0；Left 选中，A/C 确认。零运行 `cs_54578`，随后到达公共处理器返回。 |
| No 或取消 | Right 选择 No；A/C 确认，或 B 产生相同的 -1 结果。非零显示催促尽快回来的 1010，然后到达公共返回，不运行出航脚本或其 F74/F640 清除。 |

在该返回处，No 与取消无法区分。正常清理经 `ProcessPlayerAction` 返回；只有下次待处理事件读取为零才恢复控制。拒绝不保证立即输入就绪，同意也不提供随从的自然就绪检查。[选择审阅](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#departure-choice-and-ordered-script)保留提示和两个分支。

## 4. 同意后分阶段安排出航

**已确认有序内容：**`cs_54578` 按下列创作阶段推进。**推断的叙事意义：**众人登船，讨论前往大陆，并在王国毁灭中出发。有序命令不建立已完成的运行时演出。

| 阶段 | 内容与状态后果 |
| --- | --- |
| 下令并登船 (0–8) | Astral 同意；行动者 138 点头/朝 Down 并下令。Bowie 被送往 `(11,13)` 并等待，守卫 139/140 启动不等待完成的北行动作列表，Bowie 的内联移动列表被等待。嵌入的 `Map21_DefaultZoneEvent` 标签是动作数据，不是转场至 Map21。 |
| 改变码头场景 (9–30) | 让 Bowie 和 B/A/D 朝 Down；摄像机目标 `(10,11)`。八条 `3x1` 图块带从 x15 复制到 x12，y22 递减至 y15，复制间及之后有等待。它们变更工作地图图块，并标记两个视野平面待更新；不是推进标志。 |
| 讨论目的地 (31–46) | 摄像机 `(13,12)`；大臣询问去处，Astral 提议大陆/Parmecia，国王因 Elis 反对。Astral 催促逃离。D 的两次跳跃被等待；B 较后移动不等待完成。最后一句之前摄像机为 `(11,12)`。对话提供提议，不证明抵达。 |
| 离开王室跟随状态 (47–50) | 清除 F74，摄像机 `(11,13)`，然后调用 `csub_54714` 并等待 120。子程序使 layer-1 自动滚动 X 递减 16 次，使 layer-2 X 递增 24 次，每次都有 `Sleep(4)`。这些迭代次数不建立实测位移或呈现时长。 |
| 替换地图与行动者 (51–57) | 在脚本内以视野操作数 `(28,26)` 加载 Map66；从 `ce_54736` 重建行动者；等待 Bowie 初始化，将其精灵替换为船，淡入，等待速度/idle 动作，然后启动不等待完成的船路径。 |
| 描写毁灭并产生请求 (58–76) | 等待伴随震动值 3、5、3、1、0、闪电 SFX、两次白色闪光，以及从 `(0,49)` 到 `(28,23)` 的 `10x15` 图块复制。最后请求 Map73 `(26,13)`、Right，再清除 F640 并结束。请求前没有显式等待船路径完成；不再有对话或显式音乐命令。 |

**已确认标志/行动者区别：**命令 47 清除 F74 改变后续随从声明，不在那里移除已经分配的王室行动者。命令 52 使用 `ce_54736`，通过 `InitializeMapEntities` 显式重建实体：主实体 `(34,33)`、Right，没有固定 NPC。此时随从处理中的 F74 已清除。其他启用标志仍可声明随从，因此固定列表为空不证明 Bowie 是唯一实体。将他的精灵替换为船既不改变军团成员身份，也不自行选择载具 `PLAYER_TYPE`。

**已确认地图加载区别：**命令 51 的 `loadMapFadeIn` 启动淡出、加载瓦片集、等待淡入淡出、加载 Map66 并恢复中断/显示处理。`LoadMap` 写 `CURRENT_MAP=66`；`(28,26)` 是视野操作数，实体表另行提供 Bowie 的 `(34,33)`。这个脚本内加载绕过主循环的替换/遭遇检查，也不运行 Map66 配置 init。较后的 `fadeInB` 显式请求淡入。在该 Map66 序列中，F640 保持已设，直至命令 75。船的最终位置、并发移动、震动/闪光/图块外观、音频与等待完成仍为**未知**。

## 5. 最后传送在使用方运行前保持待处理

**已确认生产方：**命令 74 的 `csc07_warp` 写事件类型 1、参数 1 为零以及字节 `(73,26,13,RIGHT)`。它返回解释器，不加载 Map73，也不退出处理器。与普通瓦片传送生产方不同，它不赋予 `WARP_SFX`。解释器随后在命令 75 清除 F640，再结束。脚本结束清理在仍有对话窗口时按条件等待视野滚动，清除视野滚动速度并恢复寄存器。处理器与共享事件清理经 `ProcessPlayerAction` 返回，此时有脚本内 `CURRENT_MAP=66` 及产生的 Map73 请求。

**已确认条件使用方：**若请求保留至 `WaitForEvent`，非零字绕过控制安装。`ProcessMapEvent` 清除请求，其无滚动传送使用方消费/清除已有 `WARP_SFX`，让当前实体 0 进入 idle，退出探索，并向主循环交付 `d0=73, d1=26, d2=13, d3=Right, d4=0`。

在此静态交接处，F74/F640 已被显式清除，出航中没有清除 F507，脚本当前行动者是 Map66 上的船精灵。该路径既不建立也不要求在场景与出站消费之间存在普通 Map66 玩家控制帧。实际待处理值、中间生产方及完成的服务仍为**未知**。[返回/使用方所有者](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#return-pending-request-and-stopping-boundary)保留该限制。

这就是停止边界。`MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` 命名出站 Map73；目的地配置/init、自然抵达、航行/定居及 Battle08 不在本指南范围内。假设重入 Map44，仍可能选择 F507，而此时 F74 已清除，除非其他所有者改变这些标志。这里没有使重复交互保持一致的“已完成出航”锁存。这是不能把配置选择当作完整场景状态的反例，不是自然可达返回声明。

接着读[航行登陆至损坏船只控制](voyage-through-damaged-ship-control.md)，了解目的地首次/重复选择、航行、登陆及条件 Map14 控制/事件读取。该独立分段保留本指南的请求存续和自然路线限制。

## 独立区域表异常限制路线声明

**已确认静态异常：**Map44 四种配置共用 `ms_map44_ZoneEvents`。其 `$FD00` 默认值和相对表达式 `byte_54868+4-ms_map44_ZoneEvents` 选择 **`0x5486C`**，不是相邻的 `Map44_DefaultZoneEvent` 返回 `0x5445C`。目标位于 Map6 固定实体记录内的动作脚本指针字段。区域（事件）使用方加上位移并调用该地址；表/使用方数据独立于源码 bug 注释建立此不匹配。

现有区域（事件）测试夹具将 `raw-map44-zone-default-expression-boundary` 保留为排除项，而非已准入区域程序主体。**未知：**自然区域（事件）生产方是否沿该路线到达此处，以及在那里执行会发生什么。不要推断正常空操作或崩溃，也不要将无关实体列表重新解释为场景。该区域（事件）使用方不同于与出航老人进行的普通实体事件交互。[异常所有者](../../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#selected-zone-table-anomaly)保留限定调查。

## 证据审阅与剩余限制

综合前审阅读取了已接受的 #724、其前篇、前篇指南、相关合同，以及选定已跟踪测试夹具载荷和 schema/验证器边界。研究拥有固定源码/对话审阅和保留比较结果。本指南不增加源码调查、提取、测试夹具实现、H2/H3 重跑或运行时观察。下方命令标识现有复现所有者，不是声称新执行的检查。

| 关系 | 已审阅可执行所有者及复现路线 | 审阅处置 |
| --- | --- | --- |
| 请求、替换与遭遇准入 | [地图内容测试夹具](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[通用地图](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[战斗全局](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)，`sf2-battle-global-data-static-v1`。命令 `uv run sf2 h2 map-content`、`common-maps`、`battle-global-data`。 | 通配符、当前地图重载及独立地图/遭遇字段与限定研究审阅一致。选定 Map4/Map44 记录使用该已接受所有者；聚合测试夹具不证明行走或占位。 |
| 配置、随从与 init | [配置测试夹具](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`；[init](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`，主主体 61；[init 验证器](../../../../../../src/sf2tool/h2/map_init.py)与 [schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)。命令为 `uv run sf2 h2` 下的 `map-setup`、`map-entities`、`map-init`。 | 最后设置标志胜出的选择器及 Map44 init 调用/返回一致。选定随从行以及显式/同地图使用方使用研究；通用实体记录事实不建立自然实体群或淡入淡出完成。 |
| 实体 138、行为位与选择 | `sf2-map-events-static-v1` 下的[路由](../../../../../../tests/fixtures/h2/map-events/routing-setup.json)与[实体程序](../../../../../../tests/fixtures/h2/map-events/entity-programs.json)，程序 529；[分片/schema 所有者](../../../../../../src/sf2tool/h2/map_events_fixture.py)；[菜单](../../../../../../tests/fixtures/h2/common-menus-static-v1.json)，`sf2-common-menus-static-v1`，`yesNoPrompt`；[菜单验证器](../../../../../../src/sf2tool/h2/menus.py)。命令 `uv run sf2 h2 map-events`、`common-menus`。 | 表标志是行为位；处理器别名、分支与共享返回一致。Yes=0，No/B=-1；不提供自然激活、随从就绪或下次控制读取。 |
| 地图/行动者/标志/呈现顺序 | [脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`；[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[schema 所有者](../../../../../../src/sf2tool/h2/map_script_schema.py)。命令 `uv run sf2 h2 map-script-engine`。 | 选定 `cs_54578` 位置保留 47/75 的 clearF、51 的地图加载、52 的行动者加载、74 的传送及图块复制。放置/动作/呈现位置键保留局部命令身份；完整阶段/内联动作意义使用已接受研究，不是新的场景观察。 |
| 事件清除、轮询和分发 | [请求测试夹具](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`；[验证器](../../../../../../src/sf2tool/h2/map_event_request_consumption.py)、[schema](../../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)。命令 `uv run sf2 h2 map-event-request-consumption`。 | 入场重置、WaitForEvent 读取与 ProcessMapEvent 清除/分发仍独立。实际使用方入口/读取及正常剧情可达性明确为未知；不增加组合请求存续声明。 |
| 区域表异常 | [区域（事件）程序](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)，`sf2-map-events-static-v1`，排除项 `raw-map44-zone-default-expression-boundary`；路由分片见上。命令 `uv run sf2 h2 map-events`。 | 目标 346220 (`0x5486C`) 和基础 H1 地址 346216 保留在已准入程序之外。自然可达及运行时结果仍为未知。 |

[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)拥有选择与加载顺序；[配置数据合同](../../../contracts/world/map-setup-data.md#合同边界)拥有静态指针身份，不作更广实体群声明。[出战队伍/脚本合同](../../../contracts/party/party-roster-state.md#处理器边界)区分行动者/随从操作与招募、交战死亡及持久性。这些合同不成为另一份出航证据台账。

**未知（保留）：**自然结果/调用方状态；合法城镇/港口行走和改变布局后的重入；实际交互时的占位及随从冲突；出战队伍/HP/行动者状态；等待、移动、音频/淡入淡出/闪光/震动/渲染的完成与时序；实际待处理请求存续/覆盖；以及保存/加载持久性。未来组合观察必须指明入口、调用方标志/玩家类型、选定配置/实体、选择、脚本后地图/标志和首个使用方。拒绝、F507 缺失、随从冲突及待处理覆盖是有用对照，不是自动运行时队列。原版证据、推断的剧情意义和重制选择仍独立。本段既不完成全流程，也不完成设计 epic。
