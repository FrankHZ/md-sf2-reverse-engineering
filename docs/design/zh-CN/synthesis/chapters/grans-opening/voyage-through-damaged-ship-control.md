# 航行登陆至损坏船只控制

> 本文是[规范英文原文](../../../../synthesis/chapters/grans-opening/voyage-through-damaged-ship-control.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

出航场景将船送向大陆。**推断的叙事连接：**航行抵达 Parmecia，猛烈登陆促使难民回收木材，而 Jaha 尝试帮忙后，众人要求 Bowie 将木板带到甲板上。**已确认静态内容：**Map73 首次抵达 init 包含整个序列，在脚本内将当前地图改为 Map14，返回后写入 F700。**未知：**自然抵达、服务完成、实际行动者状态，以及随后事件读取是否允许玩家控制。

本文承接[地震城镇出口至码头出航](earthquake-town-through-docks-departure.md)。已接受的[研究所有者](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md)提供固定源码、对话、内联动作及调用方审阅。本指南支持内容/设计规划，止于 Map14 首次初始化后的控制/事件读取。下一条件分段见[损坏船只木板选择至 Battle08 准入（中文）](../eastern-parmecia/damaged-ship-plank-through-battle08.md)；城镇建设仍独立。本指南不增加重制能力。[路线图](../../../documentation-roadmap.md#当前基线与近期综合)拥有 [#731](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/731) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。源码引用使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。下方命令索引是从零开始的脚本记录，不是帧数或实测时长。对话依据已接受研究概述；措辞不产生玩法谓词。

## 1. 抵达先选择首次场景，再考虑交还控制

**已确认条件连接：**若前篇请求存续且其无滚动使用方运行，则在 F74/F640 已清除后，向主循环交付 Map73 `(26,13)`、Right。Map73 没有替换行，也没有第一列地图字段为 73 的遭遇行。Battle08 行列出的是 Map72。因此无论遭遇解锁标志如何，该请求都选择探索而非 Battle08；这不证明自然完成航行。

显式入场清除待处理事件、治疗存活/不死己方角色，并在加载地图和调用配置 init 前构建选定实体群。它还清除配置临时标志 256–383，设置 F80。Map73 只有默认配置、空固定实体列表，以及只有终止符的标志/踩踏复制表。固定内容为空仍允许随从：重建先初始化它们。F74 未设会去除该标志的王室随从声明，而其他随从标志与 `PLAYER_TYPE` 仍是输入。该 init 前没有玩家控制交接。

init 选择器区分首次抵达和后续访问：

| 输入 | 已确认选定内容与返回 |
| --- | --- |
| F700 未设，F508/F705 任意 | 运行 `cs_503A6`，然后设置 F700 并返回。首次场景不顺序落入较后选择器。 |
| F700 已设，F508 未设 | 不运行任何一个场景，直接返回；本次入场保留 Map73。 |
| F700 已设，F508 已设，F705 已设 | 不运行任何一个场景，直接返回；本次入场保留 Map73。 |
| F700 已设，F508 已设，F705 未设 | 选择较后的 `cs_50806`，然后设置 F705 并返回。其城镇建设内容不在本指南范围内。 |

F700 选择场景，不证明控制已安装或所有服务已完成。F74、F640、出战队伍就绪和 Battle08 完成都不是 F700 未设分支的额外谓词。[抵达/选择器审阅](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#map73-admission-and-init-selection)拥有这些区别。

## 2. 航行重置属性值，并保留 Kiwi 内容的条件性

**已确认有序内容：**`cs_503A6` 先重置己方角色战斗属性值，以视野操作数 `(0,0)` 重新加载当前 Map73，从 `ce_506DE` 重建航行行动者，等待 Bowie 初始化，复制一个 `22x10` 图块区域，并请求 Town 音乐/淡入 (0–8)。此处重载地图既不改变主行动者创作位置，也不分发另一次配置 init。

`resetForceBattleStats` 处理己方索引 0–29，不按已加入、出战或存活过滤。它赋予最大 HP/MP，在中间状态掩码内仅保留已有 STUN/POISON/CURSE 位，然后更新战斗员属性值。它不添加这些位；更新从已装备物品派生诅咒，因此仅凭掩码不能建立最终诅咒状态。它可以恢复零 HP 己方角色。这不同于入场时对存活/不死角色的治疗。它不招募任何人，也不改变出战队伍成员身份。处理器局部重置证据不证明自然运行过这段航行。

Astral、Bowie、Sarah、大臣和 Jaha 讨论逃离与大陆；叙述让船东行，Sarah 晕船 (9–20)。随后 F6 选择可选 Kiwi 对话：

| 分支 | 内容后果 |
| --- | --- |
| F6 已设 | Kiwi 引起 Sarah 注意并发现陆地；Sarah 告诉 Astral，后者向国王说话 (21–28)。 |
| F6 未设 | 跳到 29 处的 `cs_50426`，跳过全部八句，包括与国王的交流。 |

初始游标 2239 在首条文本前被替换为 2213。两条路径均在 29 将它重置为 2229；较后在抵达叙述前重置为 2239。省略 Kiwi 不改变后续对话身份。两条分支均不让 Kiwi 加入或写 F6。他的可选招募属于[较早 Granseal 指南](battle06-outcomes-through-battle07.md)。

Astral 与大臣要求船长不顾损坏登陆，并讨论木材再利用 (29–43)。滚动赋值、等待和震动请求安排进岸过程。先等待船的速度/idle 动作，再不等待完成地启动下一次 Right-1 移动，随后 Slade、Sarah 和船长对撞击作出反应。`waitIdle 132` 等待该移动后才执行 `stopEntity 132`、更多等待及抵达叙述 (44–55)。速度、滚动或等待操作数不衡量观察到的位移或时长；被等待的步骤仍依赖实体/服务更新完成。

## 3. 登陆替换行动者，并向人群下达指令

**已确认场景转换：**在 Map73 仍进行中的 init 内，命令 56 以视野操作数 `(18,25)` 加载 `MAP_SHIP_DAMAGED` (Map14)。命令 57 从 `ce_5072E` 重建行动者；等待 Bowie 初始化，随后显式淡入。大臣要求众人注意，提议建立新的 Granseal，并命令将船上木材搬到岸上 (56–120)。**推断的叙事意义：**这是登陆与回收说明，不是已完成的建设篇章。

十二名城镇居民（引用 129–140）收到错开启动、不等待完成的 Right/Left 动作列表。大臣转向/等待后，脚本赋予人群朝向，安装重复随机行走动作，不等待完成，再等待 80。这些动作与等待不证明木材交付、到达目的地或新城镇完成。

三次列表加载建立不同引用上下文，不是一组永久角色：

| 上下文 | 创作的主实体放置与相关逻辑引用 |
| --- | --- |
| 航行：`ce_506DE`，命令 4，Map73 | 主实体 `(63,63)`、Up。NPC 128–131 是 Astral、大臣、国王和商人/船长；132 是位于 `(5,5)`、Right 的船。Sarah、Jaha、Kiwi、Slade 使用己方角色引用。 |
| 人群：`ce_5072E`，命令 57，Map14 | 主实体 `(0,0)`、Up。128 是位于 `(23,30)`、Down 的大臣；129–140 是城镇居民。引用 132 此时标识一名居民。 |
| 船内：`ce_5079E`，命令 122，Map14 | 主实体 `(40,8)`、Left。固定己方角色记录列出 Sarah、Chester、Jaha、Kazin、Slade、Kiwi。NPC 128/129 是工人，130 是年轻男子；131–133 是三条 `MAPSPRITE_OBJECT2` 记录。引用 132 是位于 `(45,6)`、Down 的中间物体。 |

这些是声明，不是观察到的最终位置或军团成员身份。`loadMapEntities` 清除实体、初始化随从、声明固定记录，并在精灵重载前声明主实体 0。逻辑引用通过实体索引映射解析。已映射己方角色会被跳过，因此 F66/F72/F73 可使 Sarah/Chester、Kazin 或 Slade 使用随从放置，而非其固定记录。F65 在 Map73 添加大地图随从前缀；Map14 使用普通随从列表。清除 F74 不会归一化这些输入，脚本也不归一化 `PLAYER_TYPE`。声明 Kiwi 精灵不招募他或设置 F6。

[实体/重置审阅](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#force-reset-and-entity-list-semantics)拥有列表身份和随从反例。在该调用方处，所需行动者映射、自然移动及实际军团/出战队伍状态仍为**未知**。

## 4. Jaha 木板场景改变布局与呈现

**已确认有序内容：**脚本再次以视野操作数 `(45,2)` 加载 Map14，重建船内列表，并等待 Bowie 初始化 (121–123)。该显式加载在后续临时复制前重新加载布局；摄像机/视野操作数独立于主实体放置。

| 阶段 | 脚本内容与限定状态效果 |
| --- | --- |
| 准备场景 (124–131) | 将图块 `(51,7)` 保存到 `(63,63)`，并将 `(51,6)` 复制到 `(51,7)`，均为 `1x1`；停止物体 131–133 动画。F6 已设则跳到 131 处 `cs_50624`；F6 未设则先在 130 隐藏 Kiwi。两条路径均淡入。 |
| 要求 Jaha 帮忙 (132–147) | 等待 Jaha Down 1；他反对拆船，Sarah 让他帮忙处理木板。等待 Down 2，朝 Down，使用姿势精灵并等待。用力对白后，等待禁用自动朝向及自定义速度 `(64,64)`/idle 动作。 |
| 安排意外 (148–157) | 将保存的图块恢复到 `(51,7)`，请求坠落 SFX，等待 Jaha Up 4，请求震动 5/开门 SFX，等待并隐藏 Jaha。把 `(56,0)` 的 `1x2` 复制到 `(51,3)`，停止震动并等待。 |
| 指引 Bowie 继续 (158–168) | Slade 评论并 Right 2，带等待；Sarah 评论。摄像机目的地变为 `(34,4)`；Kazin 让 Bowie 将木板带到甲板上。将 Bowie 优先级操作数设为 0，引用 132 的设为 `$FFFF`，然后结束。 |

**推断的场景意义：**对话、物体放置及图块/精灵变化描写木板搬运和 Jaha 的意外。操作本身的含义更窄：

- `stopEntity` 写动画计数器 -1；不是通用移动完成屏障。
- `hide` 将实体/目的地移到地图外，使匹配的实体索引映射失效，并移除匹配的探索随从条目。隐藏 Jaha 不改变交战 HP、军团成员身份或出战队伍成员身份；不证明死亡或离开军团。
- 图块复制变更工作地图并标记视野平面待更新。`(63,63)` 是临时布局单元，不是物品栏槽位。这些复制不提供物品获取或转移。
- `setPriority` 解析当前实体，对零操作数写 0，对任意非零操作数写 1。因此最后一对操作针对 Bowie 和船内中间物体，而非此前使用 132 的航行船只或人群行动者。源码层名称不证明渲染覆盖关系。

该脚本后面没有玩家控制的搬运、传送请求、剧情标志写入或第二次 init。Bowie 在创作放置 `(40,8)`、Left 之后没有移动。实际行动者位置、服务/等待终止、物体解读和视听结果仍受[完整脚本审阅](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#complete-first-arrival-script)限定。

## 5. Map73 调用方返回时，当前地图已是 Map14

**已确认加载边界：**两次 Map14 加载都发生在原 Map73 init 调用内。`loadMapFadeIn` 加载地图资源/属性并恢复显示/中断处理；后续命令另行构建实体并请求淡入。这些加载不执行 MainLoop 替换/遭遇扫描、配置实体群选择、配置临时标志清除或 `RunMapSetupInitFunction` 调用。Map14 标志/踩踏复制表只有终止符。

Map14 默认 init 有 F703 控制的 `cs_58FA4` 调用及较后 EGRESS_MAP 写入。通过本脚本抵达**不**运行该 init 或这些效果，返回时也没有普通 Map14 实体群替换 `ce_5079E`。位于 Map14 不证明普通 Map14 入场。

脚本结束时，清理在仍有对话窗口时按条件等待视野滚动，清除滚动速度并恢复寄存器。随后 Map73 init 设置 F700 并返回。寄存器恢复不恢复原先当前地图或行动者：探索在 Map14 继续。之后执行其取决于调色板的音乐/淡入淡出路径、`SetMoveSfx` 和 `WaitForEvent`。Town 音乐与两个具名 SFX 是请求，不证明最终可听状态或服务完成。

首次 `WaitForEvent` 读取 `MAP_EVENT_TYPE` 是停止边界：

| 该次读取的待处理字 | 已确认条件使用方 |
| --- | --- |
| 零 | 让摄像机以主实体 0 为目标，并安装其受控动作。`PLAYER_TYPE=0` 选择角色控制；其他类型选择载具分支。次级循环在 A/C 输入之前再次读取待处理事件。 |
| 非零 | 跳过摄像机/控制安装，进入次级读取。保留的事件返回给 `ProcessMapEvent`，后者清除该字并分发。不建立可移动就绪帧。 |

早先入场清除以及 `cs_503A6` 没有显式传送，不证明该次读取为零：VInt 驱动的行动者/服务可在等待期间运行。F700 写入发生在**脚本之后、该条件控制使用方之前**。其源码注释不能证明玩家控制已恢复。

后续事件查找使用当前 Map14 数据。`GetCurrentMapSetup` 选择默认配置，除非 F458 选择 `ms_map14_flag458`；这些配置有不同的实体/区域（事件）指针，并共用区域描述/物品指针。这标识未来事件所有者，不调用新 init，也不消费其玩家触发内容。[调用方/控制审阅](../../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#in-script-loading-caller-return-and-first-control-read)保留该边界。后续行走、木板选择和 Battle08 准入不在本指南范围内。

## 证据审阅与剩余限制

综合前审阅读取了已接受的 #728、出航前篇/指南、相关合同、选定已跟踪测试夹具及其 schema/验证器边界。研究拥有固定源码/文本、完整内联脚本/列表审阅及保留比较结果。本指南不增加源码调查、提取、比较重跑或运行时观察。下方命令标识现有复现所有者，不是新执行的检查。

| 关系 | 已审阅可执行所有者及复现路线 | 处置 |
| --- | --- | --- |
| 抵达准入与待处理/控制 | [通用地图测试夹具](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[战斗全局](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)，`sf2-battle-global-data-static-v1`；[请求消费](../../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`。命令 `uv run sf2 h2 common-maps`、`battle-global-data`、`map-event-request-consumption`。 | 独立表/使用方事实与已接受 Map73 准入及条件事件读取一致。不证明请求存续、抵达或待处理字为零。 |
| init、配置与行动者 | [init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`，主主体 79；[验证器](../../../../../../src/sf2tool/h2/map_init.py)、[schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)；[配置](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体](../../../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`。命令为 `uv run sf2 h2` 下的 `map-init`、`map-setup`、`map-entities`。 | init 的两次调用和 F700/F508/F705 分支一致。通用配置/实体事实不建立本调用方的随从映射、出战队伍状态或等待完成。内联列表使用已接受研究。 |
| 脚本操作及其顺序 | [脚本测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`；[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[schema 所有者](../../../../../../src/sf2tool/h2/map_script_schema.py)。命令 `uv run sf2 h2 map-script-engine`。 | 选定 `cs_503A6` 位置保留重置 1、重载 2、列表加载 4/57/122、Map14 加载 56/121、图块复制 6/124/125/148/155 及生命周期/优先级位置。公开命令族事实比保留的完整程序更窄：F6 目标、游标重置和内联动作顺序使用已接受研究的完整审阅。不增加调用方组合观察。 |
| 当前地图事件路由 | [地图内容](../../../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[路由分片](../../../../../../tests/fixtures/h2/map-events/routing-setup.json)，`sf2-map-events-static-v1`；[分片/schema 所有者](../../../../../../src/sf2tool/h2/map_events_fixture.py)。命令 `uv run sf2 h2 map-content`、`map-events`。 | Map14 数据/配置查找标识未来所有者，不是自然激活、新 init 或已执行的木板选择。 |

[地图合同](../../../contracts/world/map-exploration.md#加载与选择顺序)描述加载/选择顺序；其概念上的 init 步骤适用于外围普通探索路径，不能读成每次 `LoadMap` 服务都会调用 init。已接受研究中的显式脚本内使用方才是此处更窄边界。[配置数据合同](../../../contracts/world/map-setup-data.md#合同边界)保留静态指针身份，[出战队伍/脚本合同](../../../contracts/party/party-roster-state.md#处理器边界)区分重置、招募与行动者操作。本综合不因此要求合同重写，也不产生需扩大声明的证据冲突。

**未知（保留）：**自然出航/抵达沿袭、合法行走、所需随从/行动者映射及出战队伍状态；实际位置和等待/移动完成；淡入淡出、震动、滚动、精灵覆盖和音频结果/时序；交接处待处理事件值/覆盖；以及保存/加载持久性。未来组合观察需要具名调用方、F700/F6/随从/玩家类型输入、选定脚本分支、当前地图/实体映射、F700 写入和首次待处理/控制使用方。F700 已设、F6 未设、随从冲突、非零待处理及其他玩家类型是有用对照，不是自动运行时队列。原版事实、推断的叙事意义与重制选择仍独立；本段既不完成全流程，也不完成设计 epic。
