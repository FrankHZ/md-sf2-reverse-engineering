# Battle20 结果与 Elric 招募

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/battle20-outcomes-and-elric-recruitment.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

首次 Battle20 胜利后，Elric 自称 弓箭手，感谢一行救了他并提出帮忙。Peter 将功劳归于 Oddler，欢迎新朋友。场景明确将 Elric 加入军团，随后短暂使其跟随一行。返回普通 Harpy Pond 探索会重建行动者：Elric 的成员资格保留，临时随从却不保留。重复胜利跳过他的整个场景，不修复缺失成员资格。

**已确认：**已接受静态程序及其使用方确立下述条件顺序。**推断：**对话呈现救援与自愿帮忙提议。**未知：**自然交战／结果／调用方状态、服务完成、渲染表现、响应控制及存档持久性。角色陈述仍为归属台词，不是独立证明的历史。

本指南接续 [Harpy Pond 接近至 Battle20 入场](harpy-pond-approach-through-battle20-entry.md)，使用已接受的 [Battle20 结果／招募研究](../../../../../research/chapters/southern-travel-creed/battle20-outcomes-and-elric-recruitment.md)。止于紧接的条件 Map51 控制或选定无关存档点继续。后续池塘离开、F751 通道、Fairy Cave/Creed 旅行及 Battle21 均不在范围内。独立有边界的离开、通道及条件洞穴入场说明见 [Harpy Pond 离开至 Fairy Cave 入场](harpy-pond-departure-through-fairy-cave-entry.md)。Fnnn 表示标志 nnn；记录与槽位从零开始。坐标及等待是源码操作数，不是观察位置、实测时间或自然到达场景的证明。

## 战斗如何结束

**已确认：**Battle20 的 defeated 钩子选择仅终止符的 `ms_Empty`，enemy-leader 表行为 0，完整区域表无 Battle20 场景。Defeated 包装器仍在分发空程序前检查 Bowie 存活、enemy slot128 死亡及 F520 未设置。共享尾部不将其他敌人加入死亡队列。因此仅击杀 slot128 不是胜利捷径。无区域场景不描述整个 AI 激活算法。

各自回合（单次行动）后，BattleLoop 运行该 defeated 钩子，处理击杀列表并计数剩余战斗员。己方为零选择败北，**先于**敌方为零选择胜利。若两者均未结束战斗，则处理回合（单次行动）后效果，按同样顺序重复击杀列表／计数检查，不第二次调用 defeated 钩子。计数扫描 30 个己方和 32 个敌人槽位，仅接纳 X 非负且 HP 非零者；Bowie HP 为零则强制己方计数为零。同时为零意味着败北。独立中立 Elric159 为 HP0，不会仅因有行动者就计入存活敌人数。

击杀列表清理执行记账／表现，写入 X/Y=-1，清除状态，刷新属性值并将映射行动者移出地图。Battle20 不提供首领追加受害者或特殊首领死亡后位置规则；独立位置表仅选择 Battle05。

| 结果 | 有序后果 |
| --- | --- |
| 首次胜利，F520 未设置 | 治疗符合条件的己方角色；保存返回元组；运行 Elric 完整场景；经共享 after 尾部加入 Bowie；清除 F420；设置 F520；以 d4=1 返回已保存元组。 |
| 重复胜利，F520 已设置 | 执行相同治疗、种子、共享 Bowie 加入及完成写入，但跳过 Elric 场景、join13 及其全部 follow 调用。 |
| 败北 | 更新解锁标志；疲惫音乐／文本；恢复 Bowie 的 HP；金币减半；选择存档点；返回 d4=-1。Battle04 特殊可败分支不适用。 |
| 获准 Egress / Angel Wing | 消耗所选法术 MP／物品槽位；文本／闪光／关闭；更新解锁；展开动作栈帧；选择存档点；返回 d4=0。无胜利招募／完成或败北金币惩罚。 |

败北或逃脱时，解锁辅助程序在 F520 未设置时保留 F420，在 F520 已设置时清除它。两条路径均不设置 F520、不重置 F470，也不加入 Elric。获准逃脱使用方不证明菜单可用、MP 充足或物品持有。

## 返回位置在场景前保存

**已确认：**胜利先治疗所有符合条件的己方槽位：存活己方角色，加上即使 HP0 的 Peter7/Lemon28，恢复 HP/MP 至上限，仅保留 STUN/POISON/CURSE 后刷新属性值。不要求已加入成员资格。死亡 Elric 被跳过；这是生命周期服务，不是招募。

随后，胜利保存 CURRENT_MAP、当前首名出战成员的战斗员 X/Y 加战斗区域偏移（字节运算），及其映射朝向。辅助程序重建军团列表并取首个出战条目；不搜索存活成员，也不修复空／不一致条目。一致 Battle20 准入下，map51 区域偏移为 `(0,0)`。此前接近传送 `(0,0)`、战斗初始行及后续场景放置是不同输入，不可替代此已保存元组。

只有此后 after 包装器才检查 F520。场景加载 Map51，将主行动者放在 `(6,10)`、Up，却不写返回参数、传送、CURRENT_BATTLE 清除或模式改变。其加载不进入 ExplorationLoop。完成写入及元组返回等待完整脚本返回。

## Elric 的完整招募场景

**已确认：**`abcs_battle20`（`0x4B6CE`） 含 36 个顶层记录及一个完整内联移动。下文覆盖每个选定记录和对话键，转述自[研究所属程序及字面文本连接](../../../../../research/chapters/southern-travel-creed/battle20-outcomes-and-elric-recruitment.md#complete-selected-elric-program-and-replacement-population)。B 表示 logical FOLLOWER_B31。其身份取决于传入实体生成；脚本不独立检查它是否表现 Oddler。

| 记录 | 对话与动作顺序 |
| --- | --- |
| 0–2 | 设置 cursor2566；以摄像机 `(2,6)` 加载／淡入淡出 Map51；以 ce_4B780 替换实体群：main `(6,10)`、Up，固定 Elric13 `(6,8)`、Down、Init。 |
| 3–8 | 安装并等待 Bowie、Peter 和 B Init；放置 Peter `(5,10)`、Up 及 B `(7,10)`、Up；从黑屏淡入。 |
| 9–12 | Elric 自称 弓箭手，说此前不知道池塘危险（2566），感谢他们救命（2567）。Peter 将功劳归于 Oddler（2568），说他听见呼救并带他们来（2569）。 |
| 13–19 | Peter 面朝 Right；wait5；开始 B 的 Right/Left/Right/Left 动作；wait120；B 面朝 Up，谦逊地谢绝感谢（2570）；Peter 面朝 Up。 |
| 20–24 | Elric 说 精灵 会报恩（2571）并提出帮忙（2572）。Peter 面朝 Right、Bowie 面朝 Left，Peter 告诉领队他们多了一个朋友（2573）。 |
| 25–29 | Elric 移动 Down1，等待其动作空闲；他道谢（2574）。Bowie 面朝 Up 并点头；显式 join13 服务宣布 Elric 及其当前职业（通用 text446）。 |
| 30–35 | Peter 面朝 Up，提议拜访 Mr. Creed（2575）。Peter 跟随 Bowie，B 跟随 Peter，Elric 跟随 B，均为 relative2；结束。 |

地图加载禁用实体摄像机跟随，执行瓦片集／淡入淡出／VInt／地图工作；`(2,6)` 是摄像机操作数，不是 Bowie 瓦片。实体生成加载随后清除旧实体、查找及行走分配，声明按标志选定随从、固定行和 Bowie，并加载地图精灵。它使用场景表，不使用 Map51 默认／F520 行。仅普通 F67/F68 随从时，Peter→physical1、B/Oddler→2、Elric→3、Bowie→0。其他传入随从标志可改变索引或覆盖 B 绑定；固定己方行在逻辑查找已填入时被跳过。

这清除旧 neutral159 及战斗实体群。没有从 neutral159 转换或复制 HP／状态到 ally13。默认地图停放怪物同样不在场景表。声明场景 Elric 不要求已加入标志检查。CURRENT_BATTLE 保持20，因此己方精灵使用方不走探索 HP 为零的蓝色火焰分支；当前职业仍有影响。

### 对话与动作完成

**已确认：**Elric 台词用 modifier `$0`；Peter 与 B 用 `$C0`（镜像／右侧立绘位）。立绘／台词通过当前映射地图精灵解析。Text2566 用 `{NAME;13}` 及两次换行；2567/2569/2571/2573 含一次换行。Text2566/2568/2571 以 `{W2}` 结束；其他选定场景行用 `{W1}`。Text2573 用 `{LEADER}`，在重建列表后解析首名出战成员姓名，不是无条件 Bowie 字面值。Generic446 用姓名、职业及换行，无字面 W1/W2；之后是独立音乐／输入服务。

通用加入公告不消费场景游标：2574 后的下个场景文本仍是 2575。没有 YesNoPrompt、拒绝分支或选择结果。点头及之后输入确认是表现动作，不是决定接纳 Elric。Peter 归功于 Oddler 及提议拜访 Creed 只确立对话内容，不确立自然路线或未来旅行。

Init 替换动作／计时器，可等待精确空闲。刷新／空闲前设置 速度32/32、零加速度、碰撞／可阻挡性／自动朝向、Up 朝向、无浸没及 大小24。场景显式等待 Bowie/Peter/B Init，却无对 Elric 表 Init 的独立此类等待。B 交替动作在四个朝向分别刷新并 等待30；wait120 是 Sleep 请求，不是动作指针屏障。Bowie/Peter 位置／朝向操作跳过各自 HP 为零状态；B31 绕过该己方 HP 门槛。

Elric 内联 Down1 无 HP 门槛。它清除 动作标志5/6 并安装相对移动、目的地等待及最终空闲。在一致实体生成、Init／移动完成且无其他位移时，`(6,8)` 变为 `(6,9)`。清除的冲突标志绕过目的地冲突扫描；命令既不分发普通步行区域／传送，也不证明经过时长。等待仅汇合 Elric 的空闲动作。Bowie 点头停止动画，在精灵工作／刷新之间 睡眠10/20/10，再恢复动画，不检查 HP 或同意。

文本等待滚动并使用当前行动者；single-text 命令还关闭窗口并 睡眠10。脚本进入清除 debug 文本跳过，但 debug/player2 Start 可抑制场景文本、游标递增和 csWait Sleep。通用加入遵循自身无条件服务路径。清除后的缺失查找可别名到 physical0；255 不是经验证安全的缺失哨兵值。动作、对话和点头不继承位置／朝向 HP 检查。因此缺失 Peter/B、死亡行动者或未完成服务可能不同于普通角色配置，且在此不获修复。

## 加入军团与跟随不同

**已确认：**显式 join13 清除台词 SFX、等待滚动并选择普通加入音乐。JoinForce 设置 F13，按己方顺序0–29升序重建已加入／出战／后备列表，再检查出战数量是否小于12。若小于12，JoinBattleParty 设置 Elric 出战标志 F45；该调用不立即再次重建列表。之后 UpdateForce 使用方反映激活。

出战队伍已满且 F45 未设置时，Elric 加入后备，不挤走任何人。已加入的后备在有空位时可激活。即使达到／超过容量，传入 F45 仍保持设置；辅助程序不修复不一致名册。重复加入不追加重复成员。首次场景前传入 F13 不跳过其对话、音乐或加入服务。

JoinForce 及加入操作码均不治疗 HP/MP、状态、职业、装备或战斗员位置。死亡 Elric 可加入而不复活。变更后，操作码读取其当前职业、显示446、请求先前音乐并等待当前掩码输入，再关闭文本并 睡眠10。`FadeOut_WaitForP1Input` 是辅助程序名称：它执行音乐／输入工作，无屏幕淡出。音乐轮询在检查间 睡眠3。任意非零掩码当前方向/A/B/C/Start 均满足输入；不要求新按下／松开边沿或 Yes/No 回答。未选择特殊 Sarah/Chester case128 及 text447。若这些服务停住，成员资格已改变；无回滚保证之后文本、跟随、共享加入或完成标志。

全部三条 follow 命令检查其**字编码**随从 ID 的高字节。IDs7/31/13 的高字节为0，因此每条检查 **Bowie 的 HP**，而非具名随从 HP。HP 为零的 Bowie 跳过整个命令。否则依次映射当前行动者并安装 Peter→Bowie、B→Peter、Elric→B，使用 relative2 的 `(-24,0)` 随从操作数。

AddFollower 复制带物理目标／偏移的 42 字节随从动作，仅在当前探索列表中不存在时添加物理随从。普通角色配置中，Peter/B 已在列表，Elric 的 physical3 被追加。它不写成员资格、出战或持久随从选择标志。动作禁用可阻挡性、启用不可碰撞位并跟随／等待目的地；跟随使用方仍检查布局和间距。最大轴间距在 `$1E0` 内保留目的地，否则尝试目标朝向偏移、零横向回退，阻挡时再尝试目标目的地。Relative2 不承诺世界坐标终点。场景不等待这些新随从动作稳定。

场景结束在对话窗口索引非零时有条件等待滚动，清除摄像机滚动速度并返回。它不是所有行动者完成屏障，加载后也无摄像机跟随恢复。仅此前写入不证明完整程序返回。

## 全新池塘返回移除临时 Elric 行动者

**已确认：**完整场景返回或重复绕过后，共享 after 尾部使用实际过场加入表 row20=0 并加入 Bowie。它设置 F0 并重建列表，此时反映已设置的 Elric F45；仅当结果出战数量小于12时激活 Bowie。无通用加入公告／音乐，也不修订此前保存的返回种子。

胜利随后清除 F420、设置 F520、读取已保存 map/X/Y／朝向并返回 d4=1。F470 保持传入值。MainLoop 运行 SwitchMap 再运行 ExplorationLoop，**不经过 CheckBattle**。Map51 无标志切换替换，因此保留元组。普通已保存 map51 选择全新实体生成，即使场景刚加载相同数字地图；它不是 map255 保留路径。

全新探索清除待处理状态，从踩踏计数器减去20000并对负值钳位，再次治疗符合条件己方角色，淡出，设置 CURRENT_MAP51/CURRENT_BATTLE255，选择配置并从已保存元组构建实体。它加载精灵、清除临时标志256–383、设置 F80、加载地图／视野、安装基础 VInts 并调用 init。成员资格 F13/F45、F470/F520 及随从标志不在临时重置范围。

F520 选择 Map51 空固定实体列表。全新初始化清除场景查找／动作，按持久随从选择表重建跟随，其中**没有 Elric 行**。因此 Elric 临时行动者、动作和当前列表条目消失，而已加入／出战标志保留。军团出战成员资格不会使每个己方角色生成探索随从。Peter/F67 与 B/Oddler/F68 若被选择则重建；其他标志保留顺序及可能的 B 覆盖。Map51 不是大地图，因此 F65 不在此添加 Caravan。

| 传入状态 | 紧接的边界 |
| --- | --- |
| F470 已设置、首次胜利完成 | Init 无池塘场景地返回；已保存场景前位置／朝向经配置保留，受普通服务约束。Elric 保留军团成员资格，无持久探索随从行。 |
| F470 未设置、首次或重复胜利 | 即使 F520 固定实体群为空，共享 init 仍运行此前求救提示。F470 既不被修复，也不由 F520 控制。其放置可覆盖已保存元组。 |
| 胜利前 F520 已设置 | 跳过完整 Elric 程序。共享 Bowie 加入／全新配置仍运行；传入 F13 保留，缺失 F13 不恢复。空重复不是招募证明。 |
| 满队伍、已加入后备或传入出战标志 | 上述加入规则决定成员资格／激活；它们均独立于临时跟随及其移除。 |
| 死亡／缺失／错误映射行动者、空／不一致列表或部分服务 | HP 门槛、查找别名及未完成等待仍适用。此前写入不确立自然可达、完整返回或可用输入。 |

[已接受池塘 init 所有者](elven-village-departure-through-harpy-pond-entry.md#完整初始提示)提供完整 F470 未设置继续：初始化 B/Peter；将 Bowie 放在 `(11,22)`、B 放在 `(10,22)`、Peter 放在 `(9,22)`，均 Up；淡入；B 移动 Up2、等待／张望／等待、面朝 Up 并说2550；Peter 移动 Up2／说2551；摄像机 `(2,2)`/wait50；Elric2552 和 Peter2553–2554；摄像机跟随 Bowie；Peter 跟随 Bowie，B 跟随 Peter；结束。行动者一致／移动完成时，B/Peter 在跟随前到达 `(10,20)`/`(9,20)`。不产生新 Elric 行动者、成员资格或 F470 写入；缺失 Elric lookup0 可通过 Bowie 解析求救台词。此传入状态不保证救援后表现，也不证明其自然可达。

普通战斗入场设置 mode0 和 F399，本场景均不改变。胜利不改变 F64 及已保存木筏坐标；可另行声明选定匹配木筏，不登乘。独立传入 mode2 走既有木筏实体生成路径。后续控制选择 mode0 步行、mode1 eas_Raft（实际 Caravan），或其他模式 eas_Caravan（实际木筏），不验证任意模式。init／音乐／淡入淡出后，待处理为零才让 WaitForEvent 跟随 physical0 并安装控制器；待处理请求优先。这是**条件 Map51 控制**，不是观察到的可玩暂停。止于后续离开生产方之前。

## 败北与逃脱止于选定返回

**已确认：**Battle20 无特殊 Egress 位置用例。普通 F399 已设置时，辅助程序使用传入 EGRESS_MAP 及其匹配存档点行；无匹配则保留该地图并使用 `(1,1)`、Up。异常直接 F399 未设置调用则在存档点或木筏扫描前返回初始 Map3 `(56,3)`、Down。这些是传入状态例外，不是已证明败北行程。

F64 未设置保留已保存木筏位置。F64 已设置则按所选存档点重置：Hassan9 到 Map69 `(28,55)`，Polca13/Bedoe10 到 Map72 `(43,48)`，其他情况为默认 Map69 `(42,15)`。任何重置均不登乘或改变模式。传入 EGRESS_MAP23 时，返回 Elven Village `(61,4)`、Up，启用时使用默认木筏重置；不证明上次存档点历史。其无关内容不在本指南范围内。

SwitchMap 先于探索，无战斗准入。匹配替换为 Map3→4/F640、Map71→78/F530 和 Map58→59/F999，保留坐标／朝向。传入 map255 走既有实体群路径，更新主实体属性／木筏并加载／init，不执行全新配置或临时标志清除；它不是普通 Map51 胜利。目的地 init 或待处理请求可进一步改变继续。不由此推出通用响应控制结论。

## 证据追踪与审阅边界

**已确认溯源：**已接受研究绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。其[复现所有者](../../../../../research/chapters/southern-travel-creed/battle20-outcomes-and-elric-recruitment.md#reproduction-and-remaining-limits)保留精确源码路径、符号、源码行连接及使用方审阅。选定原文为 `disasm/data/battles/entries/battle20/cs_afterbattle.asm` lines4–44（程序／ce_4B780）、text2566–2575/446、实际过场表 slot20 及首领 row20。所 include 的过场加入表提供 Bowie0；名称相近全局表的未使用功能注释不能抹去此已执行加入。本指南使用已接受证据，不开展新源码调查、运行时或重新生成。

| 解释 | 合同／可执行所有者 | 结论限制 |
| --- | --- | --- |
| Defeated/region/after 选择及共享加入 | [过场路由](../../../contracts/battle/battle-cutscene-routing.md)；[过场测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、sf2-battle-cutscenes-static-v1/expected.cutsceneFacts | 共享包装器守卫公开。精确 slot20、无 region20、leader0 及实际 include 的 join0 是已接受研究连接，不是完整公开表证明。 |
| 计数检查、清理、治疗、种子及结果 | [战斗生命周期](../../../contracts/battle/battle-control-lifecycle.md)；[循环测试夹具](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)、sf2-battle-loop-static-v1/expected.lifecycleFacts | 测试夹具绑定计数／清理／治疗上下文。选定分支／种子／逃脱／存档点顺序归研究所有者；不声称新交战模拟或自然结果。 |
| 完整场景、对话、加入及跟随 | [对话](../../../contracts/interface/dialogue-system.md)、[出战队伍／名册](../../../contracts/party/party-roster-state.md)、[地图探索](../../../contracts/world/map-exploration.md)；[引擎测试夹具](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、sf2-map-script-engine-static-v1/expected command facts；[属性值测试夹具](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)、sf2-common-stats-static-v1/expected.statsFacts.party | 公开摘要／处理器事实不复现完整调度或字面对话。已接受保留的 programCorpus.programs abcs_battle20 及直接源码连接提供全部36记录、内联移动、角色配置、文本、Bowie-HP 跟随门槛及紧接服务。 |
| 替换实体生成、全新配置／init 及条件控制 | [配置](../../../contracts/world/map-setup-data.md)、[实体](../../../contracts/world/map-entity-data.md)、[探索流程](../../../contracts/world/exploration-control-flow.md)；[init 测试夹具](../../../../../../tests/fixtures/h2/map-init-static-v1.json)、sf2-map-init-static-v1/expected | 公开 init 有共享分发器、选定源码体及嵌入程序；已接受 map-init/map-script 比较及直接选定使用方提供 F520 空实体群／无 Elric 持久行。不推出通用行动者／控制规则。 |

选定引擎字段为 dialogueCommandFacts、forceStateCommandFacts、entityPopulationCommandFacts、mapLifecycleCommandFacts、entityPlacementCommandFacts、entityActionBridgeCommandFacts、entityGestureRelationshipMotionCommandFacts、mapCameraControlCommandFacts 和 scriptControlCommandFacts。
schema／验证器路线为 [过场 schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、
[循环 schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、
[引擎 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)、
[属性值 schema](../../../../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../../../../src/sf2tool/h2/stats.py)，以及
[init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../../../src/sf2tool/h2/map_init.py)。现有 `uv run sf2 h2` 路线 battle-cutscenes、battle-loop、map-script-engine、common-stats 及 map-init 定位这些所有者；不选择纯文字重新生成或一揽子套件。受控命令须在启动进程中加载被忽略的私有输入配置。

综合前审阅检查已接受研究、共享合同及上述有边界测试夹具／schema／验证器表面。公开数量与处理器边界同更严格说明一致。完整选定程序／表／文本及使用方语义依赖已接受研究及其保留／直接源码审阅，而非新的独立复现。超出这些选定静态连接的共享合同未知项仍未解决；map-exploration 中已接受重制音乐／控制策略不确立原版 Battle20 时序或自然连续性。

保留 [PR874 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/872#issuecomment-6171350805)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/874#issuecomment-6171383624)，包括未改变比较、最终 head CI、已解决检查失败及链接的前序回执／失败。新的文字检查不隐藏任何已完成失败。

**未知：**自然交战／结果／路线；实际标志、HP、职业、成员资格、查找、模式、木筏及存档点；文本／动作／摄像机／淡入淡出／音乐／输入服务完成；渲染场景、可用控制及存档持久性。独立 F420/F470/F520、满／超容量／空队伍、重复／传入成员资格、死亡／缺失行动者、debug 跳过及部分服务约束此静态说明；它们不是已执行场景。按 [ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)，它们不产生自动运行时队列。本指南既不确立组合运行时路线，也不完成战役／设计 epic。
