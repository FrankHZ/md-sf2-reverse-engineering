# South Parmecia、Kraken 现身与 Battle16 进入

> 本文是[规范英文原文](../../synthesis/south-parmecia-through-kraken-battle16-entry.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

选定 South Parmecia 出口通往 Kraken 战场，已解锁战斗的重新入场可安排木筏停下、河中怪物浮现的场景。Peter 和条件性第二随从作出反应，随后游戏为 Battle16 重建出战队伍与敌人。抵达不保证准入，场景精灵不确立交战名册，start 包装器返回也不确立可玩的首个回合（单次行动）。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md)支持下文路线、完整遭遇和初始化。**推断：**场面描绘 Kraken 拦停木筏并从河中浮现。**未知：**自然通行、实际传入状态、服务完成、原版表现、输入就绪和持久性。Fnnn 表示标志 nnn；除另指外，坐标为地图瓦片，行号从零开始。

前段条件 Map69 控制边界见[木筏登乘与 South Parmecia 入场](polca-raft-boarding-through-south-parmecia-entry.md)。本指南选择 Map69 row0，止于 Battle16 start 包装器返回后、敌人激活、区域/生成工作、行动生成、交战或结果之前。不施加 Battle14/15 前提或编号战斗的自然顺序，不确立重制能力或连续可玩性。

接续 [Battle16 结果、下筏与条件 Hassan 入场](battle16-outcomes-through-hassan-entry.md)，了解首领/结果选择、首次/重复返回、独立下筏事件与 Hassan 按标志选择的实体群/init。该指南保留直接战斗元组与普通传送消费者的区别，止于城镇交互之前；不补全本段自然交战或旅行缺口。

## 选择战场

**已确认路线：**Map69 row0 匹配 `(28,51)`，将源码符号 `MAP_KRAKEN_BATTLEFIELD` 解析为 **Map49**，请求无滚动传送至 `(15,16)`、Up。其他七行不在该点竞争；row0 还具有首匹配优先级。[选定请求所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#selected-request-and-conditional-battle-admission)保留符号到编号关联与直接消费者。

已提交的木筏 mode2 安装源码 `eas_Caravan` / opcode8，尽管符号名如此。经过输入、边界与行动者碰撞检查，拟移动点工作布局的 `$1000` 标记调用传送匹配器；之后仍执行移动尾部。普通步行也可调用该生产者。匹配行写入数值 type1、其元组与传送声音请求，不检查木筏所有权、成员资格或战斗标志。表与控制器均不证明存在通往 `(28,51)` 的自然水路。

普通无滚动消费者清除待处理状态，请求/清除传送声音，令物理0 空闲并退出探索调用层至 `MainLoop/@Start(49,15,16,Up)`。它保留 `PLAYER_TYPE` 与 `RAFT_MAP/X/Y`；请求既不放置 Bowie，也不更新已保存木筏元组。待处理状态与参数是可变槽位，因此写入保留和服务完成仍未知。传入非零传送 parameter1 会改走滚动探索，而非本准入路径。

前段滚动 Map72 出口到达 Map69 时不进行新战斗扫描；其无滚动出口可先准入 Map69 battles9/17。本指南从条件 Map69 控制开始，不假定已避开任一前段战斗。Map49 没有按标志选择的替换。其唯一战斗地图行以 F416 和通配触发选择 **Battle16**，即 `BATTLE_VERSUS_KRAKEN`。`(0,0,30,30)` 矩形是区域输出，不是入场坐标包含测试。

| 传入准入状态 | 本次调用的后果 |
| --- | --- |
| F416 清除，F516 任意值 | 不准入 Battle16；仅有完成标志不能准入。 |
| F416 已设，F516 清除 | 准入 Battle16。 |
| F416 与 F516 均已设 | 清除 F416，但本次调用仍准入 Battle16。后续调用可以不同。 |

Luke 已接受招募场景提供 F416 解锁。模式、F64、跟随标志和成员资格不是附加准入谓词。**F516 是完成标志；F466 是后续登场/start 标志。**战斗返回的替代 MainLoop 入口跳过扫描，因此不能用返回或已保存入场代替原始无滚动调用。

## 未准入战斗时

**已确认回退：**普通 Map49 探索选择 **`ms_Void`**，即 `$FFFF` 哨兵值，因为 Map49 没有地图配置条目或专用配置目录。init 包装器跳过其回调。不存在具名的空 Map49 init 可执行。

通用探索生命周期仍清除待处理状态、治疗存活/不死己方角色、加载瓦片与地图、重建主角与按标志选择的随从、清除临时 flags256–383 并设置 F80。哨兵不提供固定配置行。Map49 使用普通随从表而非大地图表；其调色板/音乐/淡入淡出与事件等待路径可有条件地安装控制。

保留的 mode2 给主角木筏精灵，将列表随从移至 `(64,64)` 并安装站立行动，在该处声明物理木筏31。其他模式仅在 F64 与传入已保存木筏地图匹配49 时声明独立木筏。这些分支解释传入状态，不证明此战斗专用地图上有可用探索或出口。[回退所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#no-admission-fallback-and-transport-boundary)保留实体群与交通细节；进一步回退旅行不在本指南内。

## 重新入场与遭遇实体群

**已确认调用方顺序：**BattleLoop **先**清除 `PLAYER_TYPE=0` 并设置 F399，**再**测试 F88。这不经下筏行动便清除交通模式，也不更新已保存木筏元组。F88 清除时重置经过时间、保存 Map49/Battle16 并调用 before 包装器。F466 已设会跳过登场场景，继续重新初始化；F466 清除则选择 `bbcs_16`；before 包装器不设置 F466。

登场以摄像机 `(9,10)` 加载 Map49，随后只调用 `csub_4B1CA`，清除 PLAYER_TYPE 后返回。其位于战后源码并不意味着执行该结果脚本。场景实体群随后清除物理状态与查找表，重建获准随从并加载固定行。其头部把 Bowie 放在 `(15,14)`、Right。此处不同于传送 `(15,16)`、Up 及摄像机 `(9,10)`；后续交战放置又是另一边界。

十一条固定行从 `(63,63)`、Down 开始，带 init 行动：一个头、两条臂和八条腿。它们提供逻辑场景行动者128–138。头使用特殊物理47；普通肢体使用实际已声明随从之后的槽位。这是地图精灵与行动者行，不是十一名已初始化敌人。重建丢弃登乘实体群的隐藏位置与映射。独立木筏要求 F64 且 RAFT_MAP=49；早前授予的 Map72 元组不满足该检查。

Peter 是逻辑己方角色7。此地图中 F67 准入其随从，F68 以 Oddler 精灵提供 FOLLOWER_B31，但后续启用声明可覆盖同一查找表项。因此 Oddler 是**条件性**第二说话者，不是无条件脚本身份。FOLLOWER_B31 不同于物理木筏31，也不同于逻辑 FOLLOWER_D159/lookup63。重建时缺失查找映射可解析到物理 main0；命令不验证有效行动者。

位置/朝向命令跳过零 HP 己方角色0–29，包括 Peter。FOLLOWER_B31 与场景行动者绕过该己方守卫；行动、等待和复制没有同等 HP 检查。死亡或缺失 Peter 可以跳过放置，却经解析映射接收行动。后续战斗重建治疗不能为其初始场景 HP 或在场提供依据。登场命令均不加入、移除或复活出战队伍成员，也不改变跟随标志。[实体群所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#fresh-caller-scene-population-and-identities)保留物理槽位与映射细节。

## 完整现身场景

**已确认编码顺序：**`bbcs_16` 含 78 条顶层记录和四个单独检查的内联主体。遭遇有七个普通文本位置，IDs2513–2519，没有选择或剧情分支。下表转述内容并标出说话者操作数；Bowie 没有文本位置。[完整程序与对话所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#complete-pre-battle-program-and-dialogue)保留精确记录范围、原始文本键与源码溯源。

第一行前，Bowie 接收一个被等待的 init 行动。脚本把 `(13,4)` 的 3×3 工作布局区域保存至 `(0,0)`，再以 `(31,4)` 的区域替换，让两条臂共享一个精灵槽位、八条腿共享另一个。随后是被等待的 Peter 与 FOLLOWER_B init 行动。淡入前 Peter 被放在 `(15,15)`、Left，FOLLOWER_B 在 `(13,14)`、Down。等待 30 与 5 后安装 Bowie 的 Right/Left/Right/Left 行动，等待 5，安装 Peter 的，等待 5，安装 FOLLOWER_B 的，再等待 120。这三次安装没有逐个等待。随后 Peter 被等待地 Right1 移动，等待20，三者均朝 Down。

| 文本 ID | 说话者 | 内容与周围推进 |
| --- | --- | --- |
| 2513 | Peter | 注意到木筏停下。随后 Bowie/Peter 朝 Left，FOLLOWER_B 朝 Right。 |
| 2514 | FOLLOWER_B，条件性 Oddler | 询问水的动静。等待30，Peter 朝 Up，等待30，然后两次分别等待的 Peter 跳跃。 |
| 2515 | Peter | 对前方事物作出反应。Bowie 与 FOLLOWER_B 朝 Up；随后等待20，Peter 被等待地 Up2。 |
| 2516 | Peter | 在摄像机移向现身位置前提醒领队注意。 |
| 2517 | Peter | 头与肢体布置后辨认为河中怪物。 |
| 2518 | FOLLOWER_B，条件性 Oddler | 将其与 Polca 的传闻联系；此普通 `nextText` 将关窗留给下一个单文本位置。 |
| 2519 | FOLLOWER_B，条件性 Oddler | 警告出战队伍。随后等待60 并终止脚本。 |

texts2516 与2517 之间，摄像机请求 `(9,0)` 并等待滚动。隆隆声音、第一段震动幅度变化、另一次隆隆声与等待30，先于头部被等待的关闭自动朝向和 speed16/idle 主体。逻辑128 放在 `(14,5)`、Down，随后请求被等待的 Up2。只有移动完成才会到 `(14,3)`。保存的 3×3 图块区域恢复；arms129/130 放在 `(12,4)` 与 `(16,4)`、Down。随后第三次隆隆声和129 上的 FX。Legs131–138 全朝 Down，依次放在 `(13,2),(15,2),(17,3),(11,4),(12,5),(13,5),(16,5),(14,6)`；131 上的 FX 与第二段震动幅度变化先于最后三行。

**已确认服务限制：**`cloneEntity` 仅复制 **ENTNUM**，即精灵资源槽位选择器。不复制位置、行动指针、地图精灵 ID、查找表、HP 或成员资格。两臂共享129 槽位，八腿共享131 槽位；两个 FX 命令作用于共享资源。可见肢体同步属于**推断**，不是观察到行动者被克隆。

图块操作复制九个工作布局字并请求视野平面更新；不改变 ROM，也不证明画面。`$8002` 震动请求幅度1,2，每次后 Sleep40；`$4002` 同样请求1,0。每次隆隆声提交 sound93，不等待音频完成。`SLIDE_IN=5` 请求 23 次地图精灵/裁切/DMA 迭代，每次等待两次 VInt；它改变精灵素材，不改变行动者位置。淡入淡出、摄像机、文本、声音与 FX 仍为所需服务，其原版渲染/音频完成未知。

行动等待轮询具名解析行动者的指针是否空闲。固定睡眠不汇合三个并发摇晃行动。位置命令设置位置与目的地，但不清除行程/速度；init 不治疗或声明缺失行动者。文本从映射行动者解析立绘/台词属性，等待滚动并推进游标；单文本还关窗并睡眠10。调试文本跳过可抑制显示、游标增量与编码等待，却不跳过所有行动/效果。七个 ID 描述普通未跳过路径。终止符的条件滚动等待和速度清除不是全部行动者/服务的完成回执。[直接消费者所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#immediate-action-and-presentation-consumers)提供详细行动与表现边界。

## 重建实际战斗

**已确认重新初始化：**登场返回或被跳过后，BattleLoop 恢复 Map49/Battle16，清除区域 flags90–105，治疗存活/不死己方角色，初始化己方与敌人位置，清除 AI 内存并调用 `LoadBattle(0)`。治疗恢复存活己方与不死 Peter/Lemon；不复活全部死亡己方。场景 HP 与映射限制适用于该治疗之前。

BattleSpriteset16 提供 **12 条己方放置行与 11 条敌人定义**，AI 区域和 AI 点均为零。敌人为一个头、两条臂与八条腿，均为 STARTING。其 AI/物品/行为定义仍存在：区域/点表为零不代表没有 AI。此处不覆盖 AI 或交战执行。

所有己方 X 先变为 -1，`UpdateForce` 重建成员列表。放置使用 `BATTLE_PARTY_MEMBERS` 中的身份，不使用场景随从或放置行序号。零 HP 成员消耗列表/循环槽位，但不消耗放置行；存活成员取下一行。容量溢出可移除剩余出战成员。因此十二行既不证明十二名存活己方，也不证明 Peter/FOLLOWER_B 参与。不能从精灵推断缓存名册状态、当前 HP 与出战成员资格。

敌人初始化器用选定十一条定义初始化属性与位置；剩余槽位走隐藏/清除分支。不复制场景位置或克隆链接。`LoadBattle` 从已放置战斗员清除/重建行动者与查找表，选择请求战斗员的视野目标，重载地图/地形/地图精灵并请求音乐/淡入。第一条己方放置行是 `(15,15)`，取决于出战/存活列表，独立于 Bowie 场景 `(15,14)` 和传送 `(15,16)` 输入。

F466 已设时 start 包装器跳过；否则**先设置 F466**，再执行 Battle16 共享的 `ms_Empty` 终止符。没有专用 Battle16 start 场景。本指南止于该包装器返回 `@Start`，早于 `ActivateEnemies`、区域、生成、行动、交战与结果。空脚本不证明首个回合（单次行动）就绪。[初始化所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#battle-initialization-and-stopping-boundary)保留精确指针/头部、放置与调用方顺序。

## 重复、中断与已保存入场

**已确认区别：**F466 已设会在重新入场时抑制登场，仍执行重新治疗与名册/敌人重建。start 包装器前中断可在部分场景效果后仍留下 F466 清除，使登场可再次选中。F466 一旦设置，不代表全部先前行动、文本或表现服务的回执。F516 完成处理与 F416 解锁处理仍独立于登场决定。

F88 已设则选择已保存入场。BattleLoop 已清除交通模式，但该分支恢复已保存秒数、清除 F88/AI 内存、调用 LoadBattle 并转至单个行动循环。绕过 before/start 包装器、重新治疗/放置与新行动顺序生成。不用传入元组替换已保存当前地图/战斗，也不提供重建分支的显式 combatant0 参数。已保存地图/战斗/战斗员/行动状态与调用方操作数必须一致；传送元组不修复它们。恢复交战、存档一致性与木筏状态持久性仍未知，不在停止边界内。

## 证据追踪与未解问题

**已确认溯源：**本指南使用 main 已接受的 #820/PR822 研究，绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与已注册 USA 零售 ROM。[复现与覆盖所有者](../../../research/south-parmecia-through-kraken-battle16-entry.md#reproduction-coverage-and-preserved-limits)列出精确源码主体、保留比较配方、夹具与直接关联。这不是新源码调查或运行时采集。

| 边界 | 合同与现有夹具追踪 | 覆盖限制 |
| --- | --- | --- |
| MainLoop/请求/准入/实体群 | [探索流程](../contracts/exploration-control-flow.md)、[入场路由](../contracts/map-entry-routing-state.md)；`map-event-request-consumption-static-v1.json`、`battle-routing-data-static-v1.json`、`map-setup-static-v1.json`、`map-entities-static-v1.json`、`map-init-static-v1.json`（位于 `tests/fixtures/h2/` 下） | 共享结构事实；Map69→49→16 关联与 Map49 哨兵缺失仍为研究所有者的直接审阅，不是组合路线夹具。 |
| 登场/start 与完整场景 | [过场路由](../contracts/battle-cutscene-routing.md)、[对话](../contracts/dialogue-system.md)；[地图脚本夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、[过场夹具](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) | 夹具保留命令事实，包括八个 bbcs_16 复制位置，而非完整 programCorpus。研究检查保留载荷的 78 条记录，另检四个内联主体与行动者行。汇总比较不是自然入场/服务回执。 |
| 战斗员重建与成员资格 | [战斗生命周期](../contracts/battle-control-lifecycle.md)、[名册状态](../contracts/party-roster-state.md)；[spriteset 夹具](../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)、`battle-control-static-v1.json`、`battle-loop-static-v1.json`、`common-stats-static-v1.json` | spriteset 夹具明确为 rowContentTracked=false；选定12/11/0/0 输入是研究直接审阅。不能由数量相同推断场景/名册相等或出战/存活部署。 |

现有所有者 H2 命令包括 `uv run sf2 h2 map-script-engine`、`battle-cutscenes`、`battle-spriteset-data`、`battle-loop` 与 `common-stats`；链接研究配方复用保留 map-init/map-script 载荷与过场事实，不重写输入。本说明切片检查有界跟踪选项与可追溯性，不重跑载荷生成或全套测试。在任何受控复现前，于同一进程加载所属工作树忽略的私有输入配置。

[PR822 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072)保留已完成比较/CI、已解决的有界检查/路径错误、已修正说明标志 F466，以及被策略拒绝的组合交接命令。[PR818 修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)保留最初目的地假设失败与修正关联。此处均不例行重放或提升为更广覆盖。

**未知：**从 Map69 控制自然旅行；实际标志、模式、HP、成员资格、映射、列表与待处理参数；全部行动者/行动/服务完成；原版渲染/音频时序；可用输入；中断/重复历史、已保存入场一致性与持久性。这些按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)继续延期，不授权激活/行动/交战/结果调查、采集、战役完成或新产品决策。
