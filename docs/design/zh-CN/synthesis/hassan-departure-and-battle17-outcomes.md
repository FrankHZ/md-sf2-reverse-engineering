# Hassan 出发、Battle17 进入与条件返回

> 本文是[规范英文原文](../../synthesis/hassan-departure-and-battle17-outcomes.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

离开 Hassan 可以直接进入战斗，或进入 South Parmecia 探索。在该处步行可请求名为 `BATTLE_TO_TAROS_SHRINE` 的 Battle17，但符合条件的 Battle9 优先。赢得 Battle17 后，一行人返回由当前出战队伍派生的位置；败北或 Egress 返回 Hassan。这些是条件源码路径，不是必需战役顺序或观察到的前往神殿旅程。

**已确认：**已接受的[研究所有者](../../../research/hassan-departure-and-battle17-outcomes.md)确立此处说明的离开、请求/准入关联、战斗入场与直接结果消费者。**推断：**它们支持从 Hassan 朝神殿旅行。**未知：**自然旅行、传入状态、交战结果、服务/表现完成、可用输入与持久性。Fnnn 表示标志 nnn；坐标为地图瓦片，表索引从零开始。源码等待、速度和计数器是参数，不是测量的时长或移动。

城镇实体群和下筏限制见 [Battle16 结果至 Hassan 入场](battle16-outcomes-through-hassan-entry.md)，Rohde 独立提议见 [Hassan 对话](hassan-ancestry-and-rohde-caravan-choice.md)。两段对话、Achilles Sword 与 Rohde 跟随均不是选定 Battle17 前提。本指南止于战斗准入/start 或直接条件探索/控制返回。西部 Map68 旅行、Taros/Battle18、Caravan 运行、Rick 交互与交战模拟均不在范围内。

接续 [Southwest Parmecia、Taros 警告与条件 Battle18 进入](southwest-parmecia-through-taros-battle18-entry.md)，了解独立西向滚动、神殿警告/撤退与 before/start 边界。该指南保留独立准入标志，区分场景 Caravan 行动者与可用交通工具授予。

## 离开城镇并选择战斗

**已确认离开：**Hassan Map9 第一条传送行匹配 Y0 的任意 X，请求无滚动过渡至 South Parmecia Map69 `(26,54)`、Down。其他四行为不同 Y 的内部楼梯。该行不测试战斗/剧情标志或成员资格。步行与实际木筏控制均可到达传送生产者，但仅凭表不证明布局标记可达或已成功移动至该处。普通无滚动消费者返回请求元组，d4=0；保留传入交通模式与已保存木筏元组。

MainLoop **先**切换地图并扫描战斗，**再**进入探索。Map9/69 没有选定的按标志切换地图覆盖。只有两条战斗地图行使用 Map69，顺序如下：

| 候选 | 必需解锁 | 触发与后果 |
| --- | --- | --- |
| Battle9 | F409 | 两个触发坐标均为通配255。只要符合条件便胜出，包括 Hassan 出口以及 F417 也已设时。 |
| Battle17 | F417 | 两个触发坐标均为通配255。F409 清除且 F417 已设时胜出，不论玩家 X/Y 或 Rohde F418。 |

区域矩形提供战斗坐标：Battle9 原点 `(0,0)`、大小 `48x32`；Battle17 原点 `(0,31)`、大小 `30x32`。它们**不**限制玩家位置准入。符合条件的已完成战斗会让 CheckBattle 清除解锁，但仍返回该战斗索引。因此 F509/F517 不抑制已解锁候选。两解锁均清除时，离开进入 Map69 探索。若选择 Battle9，本指南止于该准入。

Battle17 使用 F417 解锁、F467 登场和 F517 完成。Rohde 已接受提议写入 Battle18 的 F418，独立于三者。F418 本身不准入17；F467 不解锁；F517 不确立当前 Sword、出战队伍或随从状态。编号战斗在该选择器处不构成强制顺序。

## 步行请求、重复遭遇与实际木筏控制

**已确认步行路线：**Map69 zone 表匹配 X23、X24 或 X25、任意 Y，并以 battle17 调用 `CheckRandomBattle`。末尾默认处理器独立。首匹配分发意味着匹配 zone 返回而未请求战斗后，不再运行默认处理器。

步行控制器仅通过工作布局 `$1400` 属性分支产生数值 type6。在最终布局/调试/阻挡决定前，以目的地加尝试瓦片增量写 word X/Y。成功移动尾部仅在有符号字为正时递减 STEP_COUNTER。表列坐标本身不是回调回执，计数器也不是时钟或观察到已完成步数的计数。

Type6 消费清除待处理状态，令物理0 空闲并分发两个坐标字。zone 包装器关闭文本/立绘，等待一次 VInt 与物理0 当前位置/目的地对齐。如有其他待处理事件，WaitForEvent 返回该事件；仅待处理为零才重新安装当前交通控制器。这是一个行动者的对齐边界，不是全部行动者或渲染屏障。

辅助读取**回调运行时**传入状态：

| Battle17 状态 | 请求/计数器结果 |
| --- | --- |
| F517 清除 | 立即请求，不测试计数器或抽取 RNG。 |
| F517 已设，计数器非零 | 返回，不请求或抽取 RNG。负字也满足此测试。 |
| F517 已设，counter0，range8 draw0 | 请求。 |
| F517 已设，counter0，range8 draw1–7 | 不请求；抽取 range4，加2，保存 counter2–5。 |

直接 RNG 按 `(13*seed+7) mod65536` 推进 16-bit 种子，并缩放至传入范围。种子状态、中途调用方和控制器进度仍为传入。Range8 不是测量的独立八分之一遭遇概率；counter2–5 不是固定遭遇期限。

请求分支设置 F417、写部分重载请求、保存 counter30000、等待视野滚动、请求 boost SFX 并执行闪光脚本。Operand30 变为四次快速淡入淡出调用，随后复制调色板。脚本终止时条件等待滚动并清除滚动速度。标志/请求写入可先于停滞服务；闪光既不加载地图，也不补全其余返回参数。

**默认**改测 F509。清除则设置 F409 并写同样部分重载请求；已设则返回。不执行随机辅助、计数器写入或 boost/闪光序列。两个回调均不清除另一战斗解锁，因此请求17 不保证扫描选中17。

实际木筏 mode2 使用 opcode8（`eas_Caravan`/`esc08_controlCaravan`，尽管名称如此）。其门、传送、移动与数值 type5 登陆分支不产生步行 `$1400` zone 请求，也不递减 STEP_COUNTER。类似 zone 的原始瓦片可走登陆/不移动行为。与步行 zone 坐标相同不运行其回调。独立[下筏说明](battle16-outcomes-through-hassan-entry.md#离开木筏)保留自身传入状态与行动者等待边界。

## 重载实际携带的内容

**已确认字节边界：**long `$000100FF` 写入设置 type1、parameter1=0（无滚动）与 parameter2=255（当前地图）。保留 parameter3/4 坐标、parameter5 朝向与 WARP_SFX。具名重载常量就是该值，不是另一分发器或完整元组。

无滚动消费者采样物理0 当前位置，再替换每个请求字节非负的坐标，并始终采用 parameter5 朝向。在未改变的步行 type6 链中，parameter3/4 保留 **zone Y 字**的高/低字节。普通 Y0–63 下成为请求 X0 与该 Y，朝向为传入残留值。负坐标字节仅保留各自采样坐标。消费者仍请求并清除保留的 WARP_SFX。这不是保留位置/朝向的重载。

CheckBattle 将 map255 解析为 CURRENT_MAP69。通配行使残留坐标与候选选择无关；重新交战放置使用选定 spriteset。若消费时两行均不再符合条件，未提供地图的探索分支更新既有主角实体属性并声明木筏，而不重建传入地图的实体群。这是条件回退，不是普通 F417 请求丢失标志的证据。

探索入场从 word 计数器减去 20000，并清除负结果。未变化的30000 在最终探索入场时变为10000；战斗入场本身不作该减少。低重试计数器在探索重启时钳位。任意 word 溢出不是通用饱和规则。待处理状态可变，因此中途写入仍属传入状态边界。

## 空场景目标与实质战斗入场

**已确认重新入场：**获准 Battle17 把 PLAYER_TYPE 清为步行0、设置 F399 并测试已保存入场 F88。F88 清除时，清除经过秒数、安装 map69/battle17 与基础 VInt，调用 before 包装器，恢复地图/战斗，清除区域 flags90–105，治疗符合条件己方，重建己方/敌人战斗位置，清除 AI 内存，以视野目标 d0=0 加载战斗，再调用 start 包装器。

全部四个实际 slot17 目标均为 `ms_Empty`，即共享 `$FFFF` 终止符（位于 `0x47A4E`）：

| 钩子 | 空目标之外的工作 |
| --- | --- |
| Before | F467 已设则跳过；否则执行而不设置它。 |
| Start | F467 已设则跳过；否则在执行**之前**设置 F467。 |
| Defeated | 脚本分发要求 Bowie 存活、enemy128 死亡且 F517 清除；首领尾部行为另行选择。 |
| After | F517 已设跳过脚本，但两条路径均运行共享加入尾部。 |

没有选定 Battle17 自定义文本、传送、行动者表、内联行动或交通写入。执行仍初始化脚本读取器/文本跳过状态，并执行普通终止/滚动工作。空目标不取消包装器标志、服务依赖或周围生命周期。F467 仍独立，不因失败或重复而重置。

`BattleSpriteset17`（位于 `0x1B43F2`） 提供12 条己方放置、14 个 STARTING 敌人、两个 AI 区域与零 AI 点。首条己方放置 `(24,23)` 加区域原点后对应世界 `(24,54)`；既不是离开 `(26,54)`，也不是固定胜利返回位置。

己方初始化先将全部30 个 X 清为-1，再重建当前已加入/出战成员资格。死亡成员消耗出战队伍槽位但不消耗放置行；超出放置容量成员可离开出战队伍。十二个放置不证明十二名存活己方或 Rohde 参与。共享治疗把存活己方与不死 Peter/Lemon 恢复至 HP/MP 最大值，仅保留既有 STUN/POISON/CURSE 位；不复活其他死亡成员或创建跟随。战斗成员资格与探索跟随仍为独立状态权威。

14 条起始行填充 enemy128–141；其他槽位走隐藏/清除初始化。Row0 基础敌人是 `(1,10)` 的 SOULSOWER。Battle17 属于重复升级集合，F517 准入共享升级算法，因此基础行不固定升级后身份。此处不评估等级/随机结果、AI、行动、伤害与获胜轨迹。

LoadBattle 清除物理实体/查找表，从已放置战斗员声明行动者，带中立敌人排除。它替换城镇/随从/木筏映射，不保留该实体群。瓦片、精灵、地形、VInt、音乐和淡入淡出有普通服务依赖。Start 在激活、区域/生成工作或行动顺序生成前返回：未观察首个可玩回合（单次行动）。

**已确认已保存入场：**F88 已设在 mode0/F399 之后测试。恢复已保存秒数、清除 F88 与 AI 内存、清除视野目标、加载并直接进入单个行动循环。跳过重建 before/start、治疗/放置、传入地图/战斗赋值与新行动顺序生成，且加载前没有重建入场显式 d0=0。已保存地图、区域、名册与行动状态必须已经一致；离开/重载不修复它们。此分支不证明存档/恢复持久。

## 结束条件与三种不同返回

**已确认静态结束规则：**单个行动后，defeated 包装器先于击杀列表处理和存活计数。先检查零己方再检查零敌人；Bowie HP0 强制零己方，双方同时为零选败北。否则运行行动后效果，再次击杀列表/计数与败北优先于胜利检查，不再调用第二次 defeated 钩子。计数要求已放置 X 和非零 HP。Battle17 首领行值为0，因此 enemy128 死亡不将全部其他敌人排队移除。仅击杀初始 SOULSOWER 不是胜利条件。死亡服务/记账仍须返回；规则不预测特定交战结果。

| 结果 | 选定返回与不同变更 |
| --- | --- |
| 胜利 | 治疗符合条件己方、初始化返回种子、运行 after/join 尾部、清除 F417、设置 F517；d4=1。 |
| 败北 | 更新解锁、力竭文本/音乐、恢复 Bowie HP、以无符号移位将金币减半、选择 Hassan 存档点；d4=-1。Battle4 特殊失败不适用。 |
| Egress / Angel Wing | 消费选定法术 MP 或移除物品槽位，请求文本/闪光/关闭、更新解锁并选择同一存档点；d4=0。无败北金币惩罚或胜利脚本/加入/完成写入。 |

Egress 描述获准的掩码行动，不是菜单可用、当前持有物品/法术或服务完成的证据。首次败北/Egress 保持 F517 清除且保留 F417；F517 原已设则解锁更新清除 F417。两分支均不重置 F467 或写 Rohde F418。

### 首次与重复胜利

返回种子使用 CURRENT_MAP、重建军团标志返回的**首个当前出战成员**、其最终战斗员 X/Y 加区域原点与其映射朝向。不无条件等于 Bowie 或首名存活成员；不修复不一致/空列表和映射。在一致 Battle17 状态下，以字节算术返回 map69、`(localX,localY+31)`、传入映射朝向与 d4=1。最终交战位置为传入，不是初始放置。

首次胜利执行终止符；重复胜利跳过。均不改变种子。两者运行共享 after 尾部：join row17 为0，因此 `JoinForce(0)` 写 Bowie 已加入标志、更新军团，并在低于12 名出战成员时有条件地激活他。既非无操作，也非新 Rohde/Rick 招募。此操作发生在种子选择**之后**，不替换返回元组。胜利调用方随后清除 F417、设置 F517。

没有神殿传送、固定西出口或 Battle16 式木筏模式写入。模式通常保留入场0；已保存 RAFT_MAP/X/Y、F64、F418 与无关 F409/F509 不被该结果改变。MainLoop 切换地图后直接探索，不经 CheckBattle。因此传入 F409 不强制胜利返回后立即战斗。后续普通请求可以扫描。

### 败北、Egress 与已保存木筏

选定 battle17 逃脱辅助选择 Hassan9，覆盖携带 EGRESS_MAP。正常战斗入场 F399 下，存档点为 `(8,9)`、Up。F64 已设还将**已保存**木筏元组写为 map69 `(28,55)`；F64 清除则保留。不调用登乘/登陆场景。异常 F399 清除调用走更早 Bowie 房间回退，不在正常 Battle17 链中。

模式通常独立于木筏重置而保留0；失败处理器不另作模式规范化。F467 如原已设则保留，因此首次失败重试不必重放登场工作。后续 Hassan 出发再次扫描：F409 可胜出，否则保留的 F417 立即准入17。F417 已清除则需新请求。这既不确立自然重试，也不确立后续神殿旅行。

## 重建探索与条件控制

**已确认选定重建：**胜利提供 map69，败北/Egress 提供 map9，无战斗离开提供 map69 `(26,54)`、Down。这些传入地图重新构建探索实体群，与 map255 回退不同。入场清除待处理、减少/钳位计数器、治疗符合条件己方、加载瓦片/地图、写 CURRENT_BATTLE=255、选择配置、重建实体/查找表、清除临时 flags256–383、设置 F80、运行 init 并条件请求音乐/淡入。不复活全部其他死亡己方。普通离开保留传入模式；未运行战斗 mode0 写入。

Map69 无配置居民，init 直接返回。随从仍依赖当前标志，F65 选择大地图表。正常 mode0 下，仅当 F64 已设且已保存 RAFT_MAP69 时，在保存坐标声明独立木筏31，lookup63→31。胜利不移动/保存它。独立传入 mode2 使 main0 成为木筏并隐藏随从；Battle17 空 after 脚本不产生该模式。

Hassan 默认/F518 替代配置共用 init 与普通随从表。[已接受实体群/init 说明](battle16-outcomes-through-hassan-entry.md#hassan-实体群与条件控制)拥有其11/12 配置行，以及逻辑137、Rohde11 与 Rick12 的条件隐藏。Battle17 不写这些实体群标志。F64 已设的失败返回把木筏保存在 map69，因此不在 town9 放置独立木筏；F64 清除也不满足放置门控。当前战斗成员资格不保证当前或重新构建的探索跟随链。

仅 WaitForEvent 中待处理为零时，才跟随物理0 并安装当前模式控制器。服务停滞或新事件可阻止交接。此处止于条件控制，不声称响应输入、全部行动者/行动完成、渲染/音频结果或状态持久。

## 证据追踪与审阅边界

**已确认溯源：**已接受 #836/PR838 研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与已注册 USA 零售 ROM。[复现所有者](../../../research/hassan-departure-and-battle17-outcomes.md#reproduction-and-evidence-limits)保留精确枚举/表/指针/参数/调用方关联。本综合使用已接受关联，不重新调查源码、重生成载荷或采集运行时。

| 说明边界 | 合同与现有夹具追踪 | 限制 |
| --- | --- | --- |
| 有序准入/存档点 | [地图入场路由](../contracts/map-entry-routing-state.md)、[探索流程](../contracts/exploration-control-flow.md)；[地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)/[gameflow](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)中的 `sf2-common-maps-static-v1`、`sf2-gameflow-core-static-v1` | 辅助/调用方规则；选定 Map9/69 与 Battle9/17 行是已接受研究关联，不是自然路线夹具。 |
| 步行/默认/木筏与残留请求 | [地图探索](../contracts/map-exploration.md)；[init](../../../../tests/fixtures/h2/map-init-static-v1.json)/[请求](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-init-static-v1`、`sf2-map-event-request-consumption-static-v1`；[随机性](../contracts/randomness.md) | 共享配置/请求事实不执行该步行/木筏/计数器/种子链，也不测量概率。 |
| 空钩子与共享尾部 | [过场路由](../contracts/battle-cutscene-routing.md)；[钩子](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)/[路由](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)中的 `sf2-battle-cutscenes-static-v1`、`sf2-battle-routing-data-static-v1` | 共享包装器事实；实际四个空 slot17 指针、leader0 与 join0 是选定研究关联。未使用加入数据不是执行的加入表。 |
| 实体群与结果规则 | [战斗生命周期](../contracts/battle-control-lifecycle.md)、[遭遇定义](../contracts/battle-encounter-definition.md)、[名册](../contracts/party-roster-state.md)；[循环](../../../../tests/fixtures/h2/battle-loop-static-v1.json)/[控制](../../../../tests/fixtures/h2/battle-control-static-v1.json)/[spriteset](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)/[属性](../../../../tests/fixtures/h2/common-stats-static-v1.json)中的 `sf2-battle-loop-static-v1`、`sf2-battle-control-static-v1`、`sf2-battle-spriteset-data-static-v1`、`sf2-common-stats-static-v1` | 共享入场/治疗/成员资格/计数/返回事实。Spriteset `rowContentTracked=false`；全局头部范围不独立证明 Battle17 的12/14/2/0，也不模拟交战/升级。 |

现有[路由](../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)、[钩子](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)、[循环](../../../../schemas/h2-battle-loop-static-fixture.schema.json)、[控制](../../../../schemas/h2-battle-control-static-fixture.schema.json)、[spriteset](../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)、[init](../../../../schemas/h2-map-init-static-fixture.schema.json)与[请求](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json) schema 保留各自载荷边界。[地图](../../../../schemas/h2-common-maps-static-fixture.schema.json)、[gameflow](../../../../schemas/h2-gameflow-core-static-fixture.schema.json)与[属性](../../../../schemas/h2/common-stats-fixture.schema.json) schema 拥有相应共享选择器、外循环与成员资格事实。其消费者为 [battle_routing_data.py](../../../../src/sf2tool/h2/battle_routing_data.py)、[battle_cutscenes.py](../../../../src/sf2tool/h2/battle_cutscenes.py)、[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py)、[battle_control.py](../../../../src/sf2tool/h2/battle_control.py)、[battle_spriteset_data.py](../../../../src/sf2tool/h2/battle_spriteset_data.py)、[map_init.py](../../../../src/sf2tool/h2/map_init.py)、[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)、[maps.py](../../../../src/sf2tool/h2/maps.py)、[gameflow.py](../../../../src/sf2tool/h2/gameflow.py)与 [stats.py](../../../../src/sf2tool/h2/stats.py)。所有者命令为 `uv run sf2 h2 battle-routing-data`、`battle-cutscenes`、`battle-loop`、`battle-control`、`battle-spriteset-data`、`map-init`、`map-event-request-consumption`、`common-maps`、`gameflow-core` 与 `common-stats`。受控复现于同一进程加载工作树忽略的私有输入配置。

综合审阅读取已接受研究与共享合同边界、选定跟踪的 intro/after/leader、重建/已保存/结果、存活/治疗及 spriteset 限制事实，以及其 schema 与消费者选项。汇总事实支持共享规则，不独立组合此路线。说明更窄的已接受关联不需要 Layer A 修正。

[PR838 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/838#issuecomment-6160960756)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/838#issuecomment-6160982826)保留原始/rebase 后 CI、保留比较与已修正/失败检查，包括猜测场景路径失败和前段历史。不仅为说明而重放。[修正的 #831 计数](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/831#issuecomment-6160802747)仍为初次13 加重复2 个 Rohde 文本；Battle17 空场景不增加对话归属。

**未知：**自然离开/zone/准入/返回可达；实际标志、计数器/种子、HP、名册、映射、请求字节与输入；行动/全部行动者/服务完成、渲染/音频、可用控制、连续交战至探索旅行与存档/恢复持久性。这些按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)继续延期。既不阻止此静态内容说明，也不授权采集队列、后续剧情扩展或新重制能力。
