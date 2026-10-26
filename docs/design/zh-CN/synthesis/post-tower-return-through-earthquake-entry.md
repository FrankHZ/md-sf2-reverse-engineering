# 塔后返回至地震城镇入场

> 本文是[规范英文原文](../../synthesis/post-tower-return-through-earthquake-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

塔楼后续结束后，出战队伍面前是已发生变化的 Granseal。**推断的叙事连接：**众人穿过塔楼与城堡，回到正在因地震撤离的城镇。**已确认静态内容：**选定出口通向城镇请求，地震标志改变其有效地图，城镇有不同的首次与重复入场程序。**未知：**合法连续行走、自然完成的场景服务及初始化返回时实际的待处理事件值。

本文承接 [Battle07 结果与探索交接](battle07-outcomes-and-aftermath.md)。已接受的[研究所有者](../../../research/post-tower-return-through-earthquake-entry.md)提供溯源、精确记录和使用方顺序。本指南支持内容/设计规划，止于 Map4 初始化后的控制/事件检查，不增加重制能力。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有 [#723](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/723) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。原版源码引用使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../README.md#pinned-original-baseline)中的 USA 零售 ROM。引用中的命令索引是从零开始的脚本记录，不是经过帧数。下方对话依据已接受审阅概述。

## 1. 离开塔楼是一系列条件场地请求

**已确认起始边界：**完成首次胜利后返回 Map58 `(13,35)`、Down，after 场景设置 F74/F640，战斗调用方清除 F407、设置 F507。必须 F999 未设才能保留 Map58。普通 before 场景清除 F83；跳过 intro 的调用方可能保留它。重复胜利不重新建立固定位置或随从/地震标志。[前篇返回边界](battle07-outcomes-and-aftermath.md#4-返回请求先于脚本和调用方完成)继续拥有这些替代路径及自然交战/服务限制。

**已确认选定连接：**Map58 出站标记请求楼梯 Map60；选定 Map60 出口请求塔入口 Map57。后续记录依次请求外部 Map40、城堡 Map21 **3F**、Map20 **1F**、Map19 **2F**，最后是 Map3 `(29,2)`、Down。每个具名记录均使用无滚动传送。[选定请求审阅](../../../research/post-tower-return-through-earthquake-entry.md#starting-state-and-outgoing-requests)保留每个触发/目的地元组，包括通配坐标。这些记录描述各个独立使用方处可能发出的请求，不提供它们之间的行走路径。

普通场地传送经请求使用方退出探索。在主循环中，`SwitchMap` 先于 `CheckBattle` 运行，只有未选中战斗才初始化探索。这不同于前篇直接战斗结果返回；后者绕过该遭遇检查。

**已确认拦截：**F401 已设时，Map57 可以选择 Battle01，不论坐标。F501 已设仅在选择后才清除 F401，因此不阻止当前调用中的遭遇。必须 F401 未设，该请求才能直接进入探索；仅 F507 不提供这种许可。战斗坐标行没有列出该链中的其他选定地图，但这一局部缺席不证明整段旅程无遭遇。

**未知的行走：**Map60 还有其他出口和同地图传送门；抵达不证明能到达选定后续标记。Map57 的 F506 图块复制与抵达区域重叠；其他启用的复制、碰撞、占位和随从均可能影响通行。反向出口或图块复制记录不能替代可达性证据。绕行 Map58 有独立 F407/F999 条件。

## 2. 城堡反映后续变化，不重演早先场景

**已确认配置规则：**按源码顺序，最后设置标志胜出，决定配置。在前篇 F506/F507 状态下，且适用处没有较后 F543/F982 覆盖时，选定内容为：

| 地点 | 选定内容及含义 |
| --- | --- |
| Map60 楼梯与 Map57 入口 | 两者都没有配置行。请求未被遭遇拦截时，`ms_Void` 不提供固定行动者或配置 init；通用标志选择的随从仍会初始化。 |
| Map40 外部 | F507 在 F506 变体之后恢复默认配置。其固定实体表为空，因此不选择早先 Lemon/守卫实体群。 |
| Map21 城堡 3F 与 Map20 城堡 1F | F506 路由选择较后的 init 目标，绕过较早守卫位置/卧室程序。其选定实体/区域（事件）只有默认事件；共用实现文件名不意味着选择较早对话。 |
| Map19 城堡 2F | F507 配置声明一个 Astral 精灵，引用 128，初始位于 `(63,63)`、Down。该配置行动者不同于标志选择的 `FOLLOWER_B`。其默认实体事件直接返回；两个区域（事件）对话仍可按各自谓词调用。 |

四个外部/城堡 init 函数检查 F507，并调用同一个短呈现程序：请求首领攻击音乐，运行 `fadeInB`，结束。它不写推进标志、传送、加入或随从行。这是创作中的呈现请求，不确认服务已完成。Battle07 胜利不清除较后配置标志，因此该表不能描述任意重入状态。[城堡配置审阅](../../../research/post-tower-return-through-earthquake-entry.md#selected-castle-setups-and-events)拥有选定指针和覆盖。

### Astral 的两次警告有独立重复控制

**已确认创作对话与守卫：**两个 Map19 区域（事件）程序均寻址配置行动者 128。

| 标记 | 各自标志未设时的首次调用 | 重复行为 |
| --- | --- | --- |
| `(16,5)`，F622 | Astral 警告地震并催促逃离；处理器在脚本返回后设置 F622。 | F622 已设抑制该对话。 |
| `(25,13)` 或 `(25,14)`，F623 | Astral 宣布众人安全；处理器在脚本返回后设置 F623。它不测试 F622。 | F623 已设独立抑制该对话。 |

这些对白不改变随从、F74/F83/F640、交战 HP、物品栏或军团成员身份。南侧传送、地图替换与城镇 init 不读取 F622/F623，因此在这些使用方处，两段对话都不是显式推进前提。**未知：**合法行走是否必然穿过标记。没有标志前提，不等于物理上可以绕过内容。Astral 的安慰也不独立证明每个角色存活。

## 3. 地震标志在城镇行动者重建前选择地图

**已确认地图选择：**城堡南出口请求 Map3 `(29,2)`、Down。F640 已设时，`SwitchMap` 替换为 Map4，保留坐标和朝向。F640 未设则保留 Map3；F507/F622/F623/F624 不是该替换的谓词。直接请求 Map4 无需这种替换。Map4 始终选择 `ms_map4`；首次/重复的决定发生在初始化内部。

**已确认实体群顺序：**探索入场时清除待处理事件，治疗存活/不死己方角色，淡入淡出，赋予当前地图和非战斗状态，再加载瓦片集并初始化实体。初始化清除先前实体群，声明标志选择的随从，读取固定配置行动者，并在指定入口位置以 idle 脚本声明 Bowie。Map4 固定行动者为守卫 128/129，分别在 `(28,7)` / `(29,7)`、Up。这些是新地图声明，不是旧场景 ID 沿旅程保留下来。

F74 启用 Astral `FOLLOWER_B`、国王 `FOLLOWER_A`、大臣 `FOLLOWER_D`。选定内容不清除 F74。其他启用行仍是输入：跳过 intro 所保留的 F83 可以增加王室行，冲突的 `FOLLOWER_B` 声明可以改变脚本寻址的映射。初始化器不保证去重或随从总数固定为三；城镇 init 不修复缺失/冲突随从。跟随独立于军团招募或交战 HP。

随后探索清除临时标志 256–383，设置 F80，加载地图并安装基本 VInt 函数，再运行配置 init。F74/F507/F622/F623/F624/F640 在临时清除后保留。标志选择的布局复制在加载时、init 脚本之前运行。[入场/实体群审阅](../../../research/post-tower-return-through-earthquake-entry.md#effective-map4-entry-and-population)及[地图加载合同](../contracts/map-exploration.md#加载与选择顺序)拥有该区别。

## 4. 首次入场布置守卫与扩大的裂缝

**已确认首次入场顺序：**F624 未设选择完整城镇场景。首先将存活 Bowie 放在 `(29,4)`、Down，将随从 B/A/D 放在 `(28,3)` / `(29,3)` / `(30,3)`、Down。这些场景放置替代抵达位置，并非新的传送目的地。放置处理器设置位置、目的地和朝向；它跳过死亡军团成员，对非军团随从则依赖已初始化映射。

场景按四个可读阶段推进：

| 阶段 | 创作内容与状态边界 |
| --- | --- |
| 地震中抵达 | 震动幅度 1、淡入与等待布置众人在外的场景。呈现服务完成仍未被观察。 |
| 守卫解释危险 | 守卫向国王/Astral 问候。Astral 下移一格，问他们为何留下；他们报告裂谷及船上的人群。他催促离开，一名守卫呼救。不同守卫速度之后是非等待移动：128 向下，129 向右。他们的报告是对话，不是对码头的观察。 |
| 守卫周围裂缝扩大 | 轰鸣、震动幅度 3 与等待伴随连续三次 `11x7` 工作布局复制到 `(24,3)`。守卫 128 收到坠落动作，随后等待 5，再将该动作赋给守卫 129，并显式等待动作脚本。 |
| Astral 指向港口 | 轰鸣及三次 `5x4` 复制到 `(27,0)` 完成第二处变化。震动恢复为 1；Astral 朝 Right，让 Bowie 走东侧。`FOLLOWER_B` 收到 `eas_Follower1`，然后脚本结束。只有返回后，init 调用方才设置 F624。 |

**已确认行动者边界：**坠落动作移动守卫，请求坠落音频，旋转/缩小精灵，等待到达目的地，停放在 `(0,0)` 并返回 idle。显式等待守卫 129；没有等价的后继命令等待 128 完成。这些操作不写交战 HP/死亡或军团成员身份。**推断的叙事解读：**场景描写守卫落入裂缝。**未知：**并发移动、等待终止、精确最终行动者状态、观感上的死亡及视听交付完成。叙述或动画中的坠落不是战斗伤亡规则。

城镇六条文本命令消费普通未跳过的 ID 715–720；城堡对话使用 713/714。[首次/重复研究审阅](../../../research/post-tower-return-through-earthquake-entry.md#first-and-repeat-initialization)保留文本溯源、源码位置和命令顺序，不提供逐字转录。

## 5. 重复入场重建改变后的布局并跳过场景

**已确认重复分支：**F624 已设选择短程序，将两个守卫停放在 `(0,0)`、Right，然后结束。在该 init 运行前，地图加载已经把 F624 选择的最终 `5x4` 与 `11x7` 复制应用到前述目的地。入场时按持久标志重建，不同于首次场景的动画工作布局变化；两者均不证明在结果地图中的合法路线或跨进程存档持久性。

重复程序不重新放置 Bowie 或随从，不显示文本、复制图块、设置震动或请求其他事件。因此，对具名南出口请求，Bowie 在控制前保留 `(29,2)`、Down；另行提供的入口则保留自己的元组。这里没有震动命令不证明幅度为零：调用方震动状态与可见呈现仍独立。F624 已设可以选择该分支，但不证明本次执行此前完成过首次场景。

## 6. 只有初始化后事件读取允许，才恢复控制

**已确认条件交接：**两个城镇 init 程序都不显式产生另一次传送或区域（事件）。init 返回后，探索按条件处理音乐/淡入淡出，再进入 `WaitForEvent`。若 `MAP_EVENT_TYPE` 为零，它跟随实体 0 并安装其受控动作脚本。前篇普通 `PLAYER_TYPE=0` 选择 `eas_ControlledCharacter`；任意木筏/Caravan 调用方输入不会被默默归一化。若有待处理事件，跳过控制安装，将事件交给 `ProcessMapEvent`。

**未知：**该次读取的实际待处理值及自然输入就绪状态。入场清除事件不证明服务和脚本之后仍为零。各独立静态使用方没有以可执行验证闭合这整个调用方组合。这个条件读取是停止边界，不能声称连续可玩返回。

Map4 默认区域（事件）与实体处理器直接返回。通用区域（事件）使用方仍关闭文本/立绘窗口，等待一个 VInt，并等待主实体停止。下一出站所有者是 [Map4 传送表](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map04/6-warp-events.asm)，见[已接受交接](../../../research/post-tower-return-through-earthquake-entry.md#immediate-controlevent-boundary-and-next-owner)。其东边缘记录指向码头请求。接着读[地震城镇出口至码头出航](earthquake-town-through-docks-departure.md)，了解已接受的请求、登船选择和条件出站交接。这些使用方、Battle08 及较后目的地内容不在本指南范围内。

## 证据审阅与剩余限制

综合前审阅读取了已接受的 #720 及其前篇、前篇指南、相关合同、选定已跟踪测试夹具载荷和 schema/验证器边界。研究所有者保留固定源码/文本审阅与已完成比较回执。本指南不进行新的源码调查、提取、H2/H3 运行或运行时观察。一次本地读取旧派生脚本载荷的尝试发现其缺失，因而改为审阅已跟踪测试夹具。此处不报告重跑任何保留比较；缺失也不使已接受研究失效。

| 关系 | 已审阅可执行所有者及复现路线 | 审阅处置 |
| --- | --- | --- |
| 场地请求、替换与拦截 | [地图内容测试夹具](../../../../tests/fixtures/h2/map-content-static-v1.json)，`sf2-map-content-static-v1`；[通用地图测试夹具](../../../../tests/fixtures/h2/common-maps-static-v1.json)，`sf2-common-maps-static-v1`；[战斗全局测试夹具](../../../../tests/fixtures/h2/battle-global-data-static-v1.json)，`sf2-battle-global-data-static-v1`。命令 `uv run sf2 h2 map-content`、`common-maps`、`battle-global-data`。 | 独立表/使用方范围与已接受选定记录一致。精确路线组合使用研究；不建立行走或无遭遇旅程。 |
| 配置、实体群和 init 分支 | [配置测试夹具](../../../../tests/fixtures/h2/map-setup-static-v1.json)，`sf2-map-setup-static-v1`；[实体测试夹具](../../../../tests/fixtures/h2/map-entities-static-v1.json)，`sf2-map-entities-static-v1`；[init 测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)，`sf2-map-init-static-v1`。命令为 `uv run sf2 h2` 下的 `map-setup`、`map-entities`、`map-init`；[init 验证器](../../../../src/sf2tool/h2/map_init.py)与 [schema](../../../../schemas/h2-map-init-static-fixture.schema.json)。 | 选定 `primarySourceBodies` 保留 Map19 的 F507 检查，以及 Map4 的 F624 读取 → 首次脚本 → 调用方标志写入，与重复脚本的区别。选择器/行动者身份独立于自然可达性。 |
| 城堡对话和城镇默认事件 | `sf2-map-events-static-v1` 下的[区域（事件）程序](../../../../tests/fixtures/h2/map-events/zone-programs.json)，顺序 33/34/93，以及[实体程序](../../../../tests/fixtures/h2/map-events/entity-programs.json)，顺序 568；[验证器](../../../../src/sf2tool/h2/map_events.py)、[分片/schema 所有者](../../../../src/sf2tool/h2/map_events_fixture.py)。命令 `uv run sf2 h2 map-events`。 | F622/F623 各自在自身脚本返回后写入，F623 没有 F622 前提；Map4 处理器返回。不证明标记被调用。 |
| 场景命令、行动者等待与复制 | [脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`sf2-map-script-engine-static-v1`；[验证器](../../../../src/sf2tool/h2/map_script_engine.py)、[schema 所有者](../../../../src/sf2tool/h2/map_script_schema.py)。命令 `uv run sf2 h2 map-script-engine`。 | 放置/动作/呈现位置键与图块复制事实保留选定程序及首次场景六次复制。`cs_51750` 不含剧情标志命令；F624 属于 init。完整对话/场景顺序使用已接受研究，不是新观察。 |
| 事件重置/轮询/消费与控制读取 | [请求测试夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，`sf2-map-event-request-consumption-static-v1`；[验证器](../../../../src/sf2tool/h2/map_event_request_consumption.py)、[schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)。命令 `uv run sf2 h2 map-event-request-consumption`。 | 具名访问和局部使用方一致；测试夹具明确保留实际使用方入口/读取及正常剧情可达性为未知。不声称自然初始化后值。 |

[地图合同](../contracts/map-exploration.md)拥有配置选择及加载/复制顺序；[配置数据合同](../contracts/map-setup-data.md#合同边界)拥有静态指针/实体列表身份，不包含更广实体群声明。[出战队伍/脚本合同](../contracts/party-roster-state.md#处理器边界)将行动者/随从操作与招募、交战死亡和持久性分开。已接受研究提供选定随从行和场景使用方；这些合同不成为第二份路线台账。

**未知（从前篇保留）：**自然战斗结果及调用方状态；合法行走和改变布局后的重入；自然使用方处的出战队伍/HP/行动者/随从状态；服务、音频/淡入淡出/移动的完成与时序；实际初始化后待处理状态；以及保存/加载持久性。这些缺口既不抹去有支持的静态内容，也不授权模拟器队列。未来组合声明必须复用所属请求/init/脚本/随从面，并指出缺失关系。原版静态内容与推断的剧情意义仍独立于重制实现及未来产品决策；本段既不完成全流程，也不完成设计 epic。
