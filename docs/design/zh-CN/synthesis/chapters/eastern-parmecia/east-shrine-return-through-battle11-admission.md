# East Shrine 返回至 Battle11 洞窟准入

> 本文是[规范英文原文](../../../../synthesis/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

洞窟遭遇可接续 Battle10 直接大地图返回，也可接续可选 East Shrine 访问。选定连接从 Map70 向西、从 Map69 向北，再从 Map72 进入 Dark Caves。若洞窟请求使 Battle11 准入且其介绍尚未播放，Peter 会描述通道、回应声音，并遇见欢迎又威胁访客的 hobgoblin。战斗准备随后以实际战斗员替换此场景角色阵容。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md)支持下文连接、准入与开场。**推断：**它们构成从 Battle10/可选神殿前沿走向洞窟遭遇的候选内容序列。**未知：**自然穿行、提供的行动者/故事/出战队伍状态、已完成服务和可见黑暗。这既不是强制神殿路线，也不是观察到的可玩入场或重制就绪主张。

本文承接 [Battle10 结果与可选神殿访问](battle10-outcomes-through-east-shrine-visit.md)，止于新战斗 start 分发器返回、紧邻 `ActivateEnemies` 之前。交战、结果、穿越洞窟和后续目标在本文之外。继续阅读 [Battle11 结果至 Polca 首次 Oddler 抵达](battle11-outcomes-through-polca-arrival.md)，了解独立战后返回、选定洞窟出口与首次城镇事件。Fnnn 表示标志 nnn；地图与战斗 ID 保留源码编号。

## 返回大地图并选择洞窟连接

**已确认条件连接：**跳过神殿时，Battle10 即时胜利返回仍在 Map70，位置和朝向由战斗导出。访问神殿提供不同起点：其南出口 X 通配、Y19，请求 Map70 `(43,41)`、Down。此出口既不测试祖父邀请，也不测试 F77/F728/F729。两种情况均仍需要完整请求与已完成使用方；单有源码连接不确立合法步行。

选定接续如下：

| 连接 | 后果 |
| --- | --- |
| Map70 西边缘，X0 / Y 通配 | 向 Left 滚动至 Map69，将 main X 设为 62 并保留 Y。此分支不把行内 Left 朝向赋给 main。 |
| Map69 北部固定点 `(41,15)` | 普通请求至 Map72 `(40,61)`、Up。 |
| Map72 洞窟点 `(58,49)` | 普通请求至 Map48 `(15,26)`、Up。 |

西向滚动继续既有探索循环，携带实体群体与临时状态，不调用普通实体/地图初始化、治疗或 `CheckBattle`。北部固定连接确实返回经过 main 的地图/战斗选择器。Map69 独立北向滚动有不同目标，并绕过该普通入场边界，不能替代选定固定点。Map70 北向滚动则通往完全不同的大地图。

神殿出口也经过战斗选择，不同于 Battle10 即时胜利返回。提供的 F410 可在那里选择 Battle10；F410 未设时，F414 可选择 Battle14。匹配的完成标志会清除解锁，但仍在同一次调用中选择该战斗。仅在没有战斗符合时才普通进入 Map70。它重建实体/随从、清除待处理与临时标志 256–383、治疗存活/不死己方角色，并运行空固定列表/仅返回 init。这些重置不清除持久故事或出战队伍标志；恢复输入仍依赖服务以待处理为零返回。

选定连接沿途还可能有其他中断。Map70 与 Map69 保留各自的特定/默认 zone 遭遇请求；特定匹配不会仅因辅助拒绝便落入默认。固定 Map69→72 请求可在普通 Map72 入场前，从 Battle8、Battle24、Battle26 中选择第一个提供的解锁，而不受其战斗区域输出限制。若普通入场发生，Map72 的 F716 已设/F10 未设 init 会调用后续场景，而非立即返回。其 zone 也可请求遭遇或后续内容。研究的[中断审阅](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request)拥有完整谓词与请求谱系。没有任何提供元组或完成的神殿对话能证明路线不受中断。传送与 zone 产生也各自要求布局属性；可变请求可能在消费前改变。

## 洞窟抵达与战斗准入相互独立

**已确认准入：**F411 来自已接受的 [New Granseal 城堡初始化](new-granseal-castle-year-later-control.md)，在首次讲话/遣散脚本返回之后。F707 未设时，调用方写入 egress 地图、运行该脚本，再设置 F707 和 F411。重复 init 不修复提供的 F707 已设/F411 未设状态。神殿完成、May 招募和 Peter 成员身份/跟随均独立于此产生方。选定洞窟传送与 Battle11 准入均不增加这些谓词。

在 Map48，战斗选择器同时测试精确触发坐标与解锁。Battle11 的 `(0,0,27,32)` 区域是选定战斗的输出，不是准入矩形：

| 完整请求 / 提供标志 | 本次调用的选择 |
| --- | --- |
| `(15,26)`，F411 已设 / F511 未设 | Battle11。 |
| `(15,26)`，F411 与 F511 均已设 | 清除 F411，仍选择 Battle11。 |
| `(15,26)`，F411 未设 | 无战斗，即使 Battle25 的 F425 已设。 |
| `(55,29)`，F425 已设 | 独立 Battle25 行可符合；Battle11 触发失败。 |
| 其他点，包括 `(52,12)` 与 `(3,11)` | 不会仅因点位于区域内就使任一洞窟行符合。 |

请求 `(15,26)` 不同于开场 main 位置 `(15,24)`，也不同于视野原点 `(8,19)`。F511 完成与 F461 介绍状态相互独立；在任意提供状态中，两者互不确立。Battle25 内容及替代洞窟路线在本文之外。

**已确认无战斗替代：**Map48 没有正常地图配置。缺失配置哨兵值跳过 init 与固定 NPC 群体，而非选择臆造的空 init。普通探索仍执行共享加载/治疗/临时重置，清除并以 main 及适用持久随从重建实体。zone 分发器跳过不存在的处理器，但保留 VInt 和物理 main 停止等待。服务后待处理为零才允许控制；加载洞窟本身不运行 Battle11 开场。碰撞、可见性及可用洞窟穿行仍为**未知**。

## 开场使用场景阵容而非断言出战队伍

**已确认新战斗选择：**F88 未设进入新战斗初始化。F461 未设时，before 包装器分发 `bbcs_11` 而不设置 F461；之后 start 包装器先设置它，再分发空 `ms_Empty` 行。F461 已设跳过两个脚本，但仍准备新战斗。F88 已设则恢复已保存战斗状态、清除 F88/AI 内存，并绕过包装器与新放置。该恢复分支到达单独回合（单次行动），在本指南终点之外。

before 场景以视野原点 `(8,19)` 加载 Map48，再显式加载 `ce_4ACA0`。尽管洞窟没有正常配置，这个实体群体请求仍提供场景阵容：

| 编写行动者 | 场景角色与边界 |
| --- | --- |
| Bowie/main `(15,24)`、Up | 物理 main 槽位 0；脚本安装并等待 `eas_Init`。 |
| Peter `(15,25)`、Up | Character 7，带 `eas_Init`；仅在尚未映射时才由此行声明。 |
| Hobgoblin `(15,3)`、Down | Event/character 128，说话的迎接者。 |
| Vampire bat `(16,4)`、Down；witch `(14,4)`、Down | Event/characters 129 和 130，是编写场景实体群体的一部分。 |

实体加载清除旧群体，并在读取这些行之前重建持久随从。F67 可能已映射 Peter，于是显式 Peter 行（包括位置和行动指针）会被跳过。后续移动改为作用于重建随从。因此编写的 Peter 放置不是通用场景原点，相对行动也不证明绝对终点。

F67 不测试已加入/出战/HP 状态。若没有较早映射，显式 Peter 行同样可声明未加入、未出战或死亡的行动者。由于当前战斗已经是 11，非战斗时零 HP 蓝色火焰精灵替代被绕过。看见 Peter 行动者或选择该精灵，不能确立 Peter 存活、F7/F39、出战队伍成员身份或玩家/AI 控制。治疗及实际放置发生在场景之后。此开场没有加入、军团 AI 或 `followEntity` 命令。

F77 可在 `FOLLOWER_A=30` 下重建祖父，不保留神殿临时 129→`FOLLOWER_B=31` 关系。其他提供随从标志可改变顺序或别名。这些身份不是固定物理槽位；跟随不使祖父成为战斗员。May 没有持久随从行；仅 F8/F40 标志不将她放入场景。

## Peter 与 Hobgoblin 的交流

**已确认编写序列：**实体群体加载后，脚本将摄像机目标设为 Bowie、通过 `csub_458E` 刷新平面 B、请求 `fadeInB` 和声音淡出。加载与淡入命令名描述请求及处理器服务，不是观察到的已完成呈现。

Peter 先向 Left 移动一瓦片、再向 Up 一瓦片，均带行动等待，并描述穿过洞窟的路线。提到 hobgoblin 时，Peter 面向 Right，Bowie 面向 Left。请求 sleep 20 帧后，说话者 128 回答，脚本请求 `MUSIC_ENEMY_ATTACK`。

Peter 再向 Up 移动一次。sleep 五帧后，Bowie 启动 `eas_2xRightLeft`；再过五帧，Peter 启动 `eas_2xUpDown`。这两个往复命令自身不等待完成。随后 sleep 120 帧。Bowie 面向 Up，Peter 再带等待向 Up 移动并询问谁在说话；128 表明身份。摄像机随后收到速度 `$20` 并以 128 为目标。Hobgoblin 欢迎访客、带等待向 Down 移动一瓦片、发出威胁，脚本在终止前再请求声音淡出。

这些节点概述七个编写文本请求，ID 2483–2489（`09B3..09B9`），由 Peter 和 128 说话。其中没有场景选择、条件跳转、故事写入、传送或交战决定。四次显式 sleep 共请求 150 帧，不是场景经过时长。文本增加自身滚动/窗口/sleep 服务，调试跳过可省略文本、游标推进和 sleep。完整记录/文本组合仍归[开场研究](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md#opening-records-guards-and-services)所有。

命令的状态要求也不同。Peter/Bowie 朝向会跳过**字** HP 为零的己方角色；HP256 非零。行动安装/移动与摄像机目标没有同等 HP 守卫，且仅指定摄像机目标不等待滚动。缺失 character 映射解析为槽位 0，而非安全跳过。因此行动者名称和对话不能证明移动成功、映射正确、发声、摄像机到达或服务完成。脚本终止符条件性等待视野滚动并清除滚动速度；本文不主张组合运行时观察。

## 黑暗呈现与战斗交接

**已确认平面服务：**`csub_458E` 在第二次 Bowie 摄像机请求之后运行。它使用既有摄像机/图层辅助写入平面 B 坐标及目标字、构建环形计数器并设置平面 B 更新位。既有 VInt/平面使用方将这些字段用于布局和 VRAM 工作。输入依赖实际实体、摄像机和图层状态；固定开场坐标不能替代它们。它不设置可见半径、出战队伍/故事标志或实体群体，也不等待渲染输出。研究的[辅助审阅](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md#plane-b-helper-and-darkness-evidence-boundary)拥有运算、描述符及使用方链。

**推断：**刷新让洞窟次级平面相对于选定行动者对齐，用于黑暗相关呈现。**未知：**自然入场时可见遮罩、照亮区域、遮挡、调色板及硬件时序。后续淡入淡出、移动、摄像机变化及战斗加载增加服务并重载状态；辅助写入不证明其组合结果或持久性。

**已确认战斗交接：**before 返回后，循环恢复地图/战斗、清除区域标志、治疗存活/不死己方角色、先初始化己方再初始化敌方位置、清除 AI 内存，并为战斗员 0 加载战斗。己方放置使用实际出战列表和字 HP、跳过死亡成员；十二个编写己方位置不保证十二人出战。十条敌方行提供战斗放置，替换三个场景敌人。

`LoadBattle` 通过 `PositionBattleEntities` 清除脚本实体，准备地图/精灵/地形资源、选择战斗员映射的摄像机目标、播放地图音乐并请求淡入。它不保留场景阵容，也不保证辅助的精确平面字存续。start 包装器随后在 F461 未设时设置它，并分发 `ms_Empty`；其正常终止服务仍适用。本指南止于该分发器返回、紧邻 `ActivateEnemies` 之前。这确立源码交接，而非观察到的玩家就绪回合（单次行动）、激活/AI 规则或交战结果。

## 证据审阅与剩余限制

本指南使用 PR #770 接受的研究，前序英文指南在 PR #769 接受。研究拥有固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、USA retail 身份、源码组合和复现。直接有界审阅未发现需要修正的 Layer A。既有受跟踪夹具在内存中读取并对所属 schema 验证；未生成/复制可选私有载荷，也未重新调查源码或运行运行时观察。

| 审阅关系 | 证据与区分边界 |
| --- | --- |
| 连接、缺失配置与 init 门控 | [sf2-map-data-static-v1](../../../../../../tests/fixtures/h2/map-data-static-v1.json)、[sf2-map-setup-static-v1](../../../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-init-static-v1](../../../../../../tests/fixtures/h2/map-init-static-v1.json) 主体 11/76/77/78，以及[地图/探索合同](../../../contracts/world/map-exploration.md)。map-data 是清单；精确传送行与使用方组合来自已接受研究。不存在 Map48 init 路由关联。 |
| 准入与 before/start 选择 | [sf2-common-maps-static-v1](../../../../../../tests/fixtures/h2/common-maps-static-v1.json)、[sf2-battle-global-data-static-v1](../../../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[sf2-battle-cutscenes-static-v1](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)及[过场合同](../../../contracts/battle/battle-cutscene-routing.md)。战斗区域输出先于完成清除；`routeTargetsParsed=false` 阻止从汇总占用推导精确 row-11 目标。 |
| 场景命令、映射与等待 | [sf2-map-script-engine-static-v1](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，其 `bbcs_11` 切换/实体群体/摄像机选择及放置/行动/对话/控制处理器事实。它保留族位置与守卫，而非完整公开场景记录。已接受研究提供完整有序记录、实体群体、文本和随从组合。 |
| 平面刷新与最终实体群体 | 公共地图平面更新事实及 [sf2-battle-spriteset-data-static-v1](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)。`rowContentTracked=false`；精确 Battle11 放置与辅助/加载顺序来自已接受研究。两个汇总夹具均不验证可见黑暗或自然出战放置。 |

[地图 init 验证器](../../../../../../src/sf2tool/h2/map_init.py)与 [schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)保留主体/操作顺序。[脚本 schema 组合](../../../../../../src/sf2tool/h2/map_script_schema.py)与[验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)保留处理器、操作数及族位置边界。窄所有者是既有 `uv run sf2 h2 map-data`、`map-setup`、`map-init`、`map-script-engine`、`common-maps`、`battle-global-data`、`battle-cutscenes`、`battle-routing-data` 和 `battle-spriteset-data` 命令；列明它们是可追溯性，不主张新提取。研究的[复现边界](../../../../../research/chapters/eastern-parmecia/east-shrine-return-through-battle11-admission.md#existing-evidence-reproduction-and-unknowns)保留精确选定比较路径。不单为本篇文字选择广泛测试套件。

**未知：**无碰撞且不中断的旅行；实际行动者/映射/标志/HP/出战队伍/AI 状态；请求存续及完成的步行/文本/输入/摄像机/淡入淡出服务；组合黑暗与时序；连续可玩的 Battle11 入场。这些限制不授权采集或新机制。本指南不选择产品/引擎，也未完成战役。
