# 棋盘结果与返回 Desktop Kingdom

> 本文是[规范英文原文](../../synthesis/battle22-outcomes-and-desktop-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**击败 King 可通过首领清理路径结束 Battle22；普通敌人计数是独立结果检查。
胜利完成战斗并返回重建后的 Desktop 实体群。独立门控场景宣布 Cotton Balloon，但不授予或使用它。
**推断：**Ponpei 将其作为赢得棋盘挑战的奖励。
**未知：**自然胜利、传入状态、服务完成、可见场景及可用控制。

本指南接续[棋盘初始化与初始名册](battle22-initialization-and-roster.md)，
使用[已接受 Battle22 结果研究](../../../research/battle22-outcomes-and-desktop-return.md)及
[战斗生命周期合同](../contracts/battle-control-lifecycle.md)，依据
[Issue #915](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/915) 和
[当前 Standard 约定](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。
止于条件局部探索/控制或待处理返回，并包括改变后的 King/Maid 对话。
[Cotton Balloon 获取与 Floor World 离开路线](cotton-balloon-through-floor-world-entry.md)
继续说明独立宝箱/守卫/物品使用关联，直至条件 Map26 加载器准入、目的地实体群/init 之前。
Fnnn 表示标志 nnn；行及命令索引从零开始。坐标是源码操作数，不是观察到的移动。

## 遭遇如何结束

**已确认：**defeated 钩子首先要求 Bowie 存活、敌人 128 的 HP 为 0。
全新名册中，敌人 128 是棋子 King。Battle22 的首领存在条目为 -1。
F522 未设置时钩子执行 `ms_Empty`；F522 已设置时跳过脚本。
两个合格路径都到达首领尾部，将 128–159 中每个非零 HP 敌人追加至 killed 列表。
它既不检查放置 X，也不检查新伤害结果，本身不清这些敌人的 HP。

ProcessKilledCombatants 随后进行表现/记账，并写列表成员 X/Y=-1、清状态、更新属性值、将映射行动者移至地图外。
剩余计数要求非负 X 和非零 HP，因此该清理可使存活棋子退出计数，而无需逐个将其减至 HP 0。
全新 Ponpei/Maid 中立槽位 159/158 已为 HP 0，不追加。传入状态可能不同。

普通计数扫描三十个己方和三十二个敌人。Bowie HP 0 强制己方计数为零。
两个结果检查都先测零己方：同时全灭为败北。Bowie 死亡也绕过 defeated 钩子。
状态一致且 Bowie 存活时，经该钩子的 King 死亡可结束遭遇；普通计数边界上敌人计数为零也足够。
目标并非只能是逐个消灭全部敌人。

顺序很关键：个体回合（单次行动）→ defeated 钩子 → killed 处理 → 计数。
若两方仍有成员，随后是回合（单次行动）后效果 → killed 处理 → 计数。
第二次检查没有 defeated 钩子：King HP 在那里变零，本身不调用首领清理。
调试 kill-all 调用方是独立证据。Battle22 没有区域过场行或首领后位置覆盖；
不要引入 Battle21 场景或 Battle05 位置变化。

## 胜利在 Desktop Init 前完成战斗

**已确认：**首次和重复胜利共享此顺序：

1. 将存活己方，加上即使已死亡的 Peter 7 和 Lemon 28，治疗至最大 HP/MP。
   仅保留已有 STUN/POISON/CURSE 位并更新属性值；其他死亡己方仍死亡。
2. 按己方 ID 升序重建已加入/出战军团列表。首个出战成员提供返回位置，其映射战斗行动者提供朝向。
3. 运行 after 包装器。F522 未设置分发 `ms_Empty`；F522 已设置则跳过。
   两条路径均到达为 Bowie 执行的共用 JoinForce(0) 尾部。
4. 清 F422，设置 F522，读取返回参数并返回 d4=1。

选定加入表是布局包含的 **cutscenes** 表，不是其名称相似的全局对应表。
Bowie 加入不是新招募 Eric/Karna。JoinForce 设置他的已加入标志并重建列表；
仅在出战计数小于十二时将他设为出战。它不为满员队伍腾位，且该出战激活不立即再次重建列表/计数。
返回种子先前已经选定，因此此加入不能修复种子。

空脚本仍走 ExecuteMapScript 终止符尾部，包括条件对话滚动等待、清滚动速度、还原寄存器及返回。
空不保证立即完成。每项后续写入依赖更早服务返回；未完成前缀没有自动回滚。

### 返回位置不是场景放置

**已确认：**胜利种子由 CURRENT_MAP、首个出战成员的战斗员 X/Y 以字节运算加 BATTLE_AREA_X/Y，
以及该成员映射行动者的朝向组成。已接受 Map22 区域 `(9,0)` 下，局部 `(x,y)` 按该运算模数成为完整地图 `(x+9,y)`。
它不是初始放置行，也不是更早的 `(0,0,Right)` 准入传送。
首成员辅助不寻找存活成员或修复空列表。成员资格、位置、查找或已保存区域状态改变可改变元组。

Battle22 空 after 脚本不替换该种子。BattleLoop 直接读取 PARAM2–5，不等待新传送事件，也不在此写新 MAP_EVENT_TYPE。
MainLoop 从替代入口恢复，执行 SwitchMap 并进入探索，**绕过 CheckBattle**。
标志切换地图表没有 Map22 行。

F422 准入、F472 介绍、F522 完成及 F88 挂起仍彼此独立。
胜利清 422/设置 522，不写 472/88。F88 已设置入场使用当前/已保存状态，不进行全新名册/钩子；
这些结果声明仍要求实际 CURRENT_BATTLE=22。F472 不是结果或 Desktop-init 门控。
挂起不是胜利或完成写入。

## Desktop 以不同身份重建

**已确认：**普通显式 Map22 探索返回清 MAP_EVENT_TYPE，再治疗合格己方，淡出、禁用视野跟随，
设置 CURRENT_MAP=22、CURRENT_BATTLE=255。它替换实体/查找、加载地图精灵、清临时标志 256–383、
设置 F80、加载完整地图、安装 VInt 并运行 init。战斗行动者及其行动/查找消失；棋盘不再裁剪。
传入地图 255 则使用既有实体群分支，不作上述全新选择/重置/赋值。

F522 选择替代实体群，独立于 F776/F778。两个配置共享事件、区域（事件）、描述、Section5 和 init。
替代实体群有**十四行**：Eric/Karna、Caravan、年轻男子/女子、老人、牧师、Maid/Ponpei、两个守卫，
以及三个固定在 `(63,63)` 的 OBJECT1 占位角色。
[完整实体群表](../../../research/battle22-outcomes-and-desktop-return.md#f522-population-and-layout)
负责全部位置、朝向、精灵和行动。Eric 和老人使用步行动作；其他可见行使用 init 动作。
列出的位置不预测后续移动。

己方行不测试已加入/出战成员资格，因此看到 Eric/Karna 不招募他们。
既有非零己方查找可跳过整行而不消耗 physical 槽位；当前随从表不含二人，因此普通全新选择不提供该跳过。
普通 NPC 查找单独追加。仅 F67/Peter 和 F68/B 随从时，physical 1/2 为这些随从；
行 0–13 占据 physical 3–16。该示例中 Maid 133/Ponpei 134 为 physical 10/11。
NPC 128 又是 Caravan，而非棋子战斗员 King 128。
战斗中立角色状态 159/158 与 B31/A30 脚本键也分别是不同身份。

十六个棋子行和旧 ORB153–155 行不保留。共享事件条目 137–152 仍存在；占位角色 137–139 复用其中部分 logical ID。
地图外放置不证明自然交互，也不移除其事件回调。

### 随从单独提供

**已确认：**Map22 不是大地图，因此 F65 不在此添加 Caravan/A。
F522 不创建 Peter 或 Oddler；战斗成员资格不是随从准入。
普通 F67/F68 示例在返回种子处声明 Peter/eas_Follower1 和 B/eas_Follower2。
没有固定 Peter 回退。其他随从标志可改变分配、覆盖 B/A/D 查找别名，同时更早 physical 行动者仍存在。

全新 ClearEntities 将查找清零。缺失的存活 Peter 或缺失 B 因而可解析至 physical 0，而非安全无操作。
当前 HP/职业选择己方精灵，包括战斗外 HP 0 的 BLUE_FLAME。
既有模式/木筏处理也仍有效：F64 配合 RAFT_MAP=22 在保存坐标声明 physical 31；
mode 2 则将主角色改为 RAFT、停放随从，并在 `(64,64)` 声明 31。
普通双随从实体群止于 16，但额外随从分配可到达 31 并被覆盖，而别名仍保留。
选定结果或场景均不登木筏。

### 宝箱布局变化不授予物品

**已确认：**完整加载在 64 宽工作布局上应用两个 F522 复制：
1×1 从 `(39,0)` 至 `(25,10)`，再 1×1 从 `(40,0)` 至 `(25,40)`。
源码注释分别标为 chest-for-chessboard 与 blank-for-blank。
随后应用宝箱覆盖；宝箱标志已设置时可随后应用打开宝箱字 `$D802`。
宝箱行指向 `(25,10)`、F212 和 COTTON_BALLOON。F522 不设置 F212。
这些记录既不确立物品栏持有，也不确立已解码碰撞/渲染路线。
[获取指南](cotton-balloon-through-floor-world-entry.md#找到礼物不等于收到礼物)
解释后续搜索、交付前 F212 写入及全员物品栏满/重复/中断路径。

## 返回时运行哪个场景

**已确认：**Map22 init 先测试 F776。未设置则运行完整抵达场景，仅返回后才设置 776。
它独立测试 F522 和 F778：战斗完成且 F778 未设置时运行 `cs_5996E`，仅返回后才设置 778。

| 传入门控 | 条件结果 |
| --- | --- |
| F776 已设置，F522 未设置 | 无场景，不受 F778 影响。 |
| F776 未设置，F522 未设置 | 抵达，然后 776；无胜利场景。 |
| F776 已设置，F522 已设置，F778 未设置 | 胜利场景，然后 778。 |
| F776 已设置，F522 已设置，F778 已设置 | 无场景；仍选择替代实体群/布局。 |
| F776 未设置，F522 已设置 | 先抵达，然后 776；仅 F778 未设置且抵达完成时才接着运行胜利场景。 |

F522 已设置/F776 未设置的抵达在**替代**实体群上运行。它引用旧 ORB153–155，缺失查找可别名至主角色。
不能保证该状态实现基线抵达编排/完成示例。抵达未完成会同时阻止 776 写入及后续胜利场景分发。
外部传入标志不证明场景曾被游玩。

### 完整奖励公告

**已确认：**`cs_5996E` 含十二个命令，包括终止符：

| 索引 | 命令 | 若到达则产生的后果 |
| --- | --- | --- |
| 0 | textCursor 1794 | 选择下文首句。 |
| 1 | setCameraEntity CAMERA_NEUTRAL | 直接写摄像机目标 -1，不查找行动者。 |
| 2 | reloadMap 10,0 | 复用工作布局并请求摄像机/显示状态。 |
| 3 | setPos Bowie,25,12,Up | 放置已准入 Bowie 行动者。 |
| 4 | setPos Peter,24,13,Up | 放置已准入 Peter 行动者。 |
| 5 | setPos B,25,13,Up | 放置实际 B31 查找。 |
| 6 | fadeInB | 请求淡入并等待服务。 |
| 7 | nextSingleText $0,134 | 显示 1794，关窗口，sleep 10。 |
| 8 | setCamDest 20,7 | 禁用跟随，请求摄像机目的地，等待滚动。 |
| 9 | nextText $0,134 | 显示 1795，保留续接打开。 |
| 10 | nextSingleText $0,134 | 显示 1796，关窗口，sleep 10。 |
| 11 | csc_end | 条件滚动等待，清速度，还原寄存器，返回。 |

1794–1796 使用 Ponpei 134：“Checkmate”和礼物介绍；说明里面有 Cotton Balloon；
以及它能让队伍安全离开 Desktop Kingdom 的建议。
[字面文本/控制符表](../../../research/battle22-outcomes-and-desktop-return.md#camera-actor-and-text-services)
保留键 0702–0704 和 W1/W2 控制符。普通 NPC 134 使用 KING_PONPEI188、无立绘及台词声音 bleep 6；
他既不是 KING_GRANSEAL，也不是棋子 King。实际映射精灵和已打开立绘会影响服务。
两个文本处理器均等待滚动；仅 nextSingleText 执行最终窗口关闭/sleep。
调试 P2-Start 可跳过文本而不跳过其他命令，因此 F778 不证明看过每句。

reloadMap 使用负 d1 复用 CURRENT_MAP=22 的工作图块/布局；
它不重新复制标志、不替换实体、不重跑 init，也不再次裁剪棋盘。
全新探索加载已选择完整 F522 布局。其压缩摄像机操作数 `(10,0)` 成为 `$1E00`，
在区域/视差处理前解释为 `(3840,0)`。setCamDest 将 `(20,7)` 乘以 384。
这些是摄像机边界请求，不是玩家或返回坐标。

映射一致时，三个 setPos 请求是完整地图 Bowie `(25,12)`、Peter `(24,13)` 和 B `(25,13)`，
均为 Up，**不加战斗原点 9**。处理器跳过 HP 0 的真正己方角色 0–29；B31 绕过该测试。
缺失的存活 Peter 可写主角色；缺失 B 随后可在 `(25,13)` 覆盖主角色。
其他 B 别名指向另一行动者。胜利治疗不补充缺失随从标志。
写入改变当前/目的地坐标和朝向，却保留既有行动脚本：随从随后可能移动。
它们不创建新返回元组、不冻结行动者，也不保证不变的最终位置。

程序中没有传送、物品授予、选择、标志命令、移动序列或随从行动替换。
Init 仅在终止符返回后写 778。淡入淡出和滚动等待依赖 VInt，此处没有已过时间或可见完成证明；
后续失败保留早先放置/摄像机/文本效果。探索随后有条件地提交音乐/淡入淡出并到达 WaitForEvent。
待处理 type 0 以 physical 0 为目标并安装当前模式行动；新待处理事件优先。
BattleLoop 提供 PLAYER_TYPE=0，因此一致路径选择 eas_ControlledCharacter。
仅 init 完成不证明后续输入/服务返回或曾有可用控制。

## 再与 Ponpei 和 Maid 交谈

**已确认：**共享事件表仍存在。实际激活和包装器返回由
[交互所有者](../../../research/desktop-kingdom-interactions-through-battle22-entry.md#actual-activation-event-selection-and-return)负责；
复用 physical 示例前先应用新实体群。这些回调使用 F522，独立于 F776/F778：

| 交互/状态 | 内容与写入 |
| --- | --- |
| Ponpei，F522 已设置 | 1797 赞扬队伍，并提出若留下便送军队；不加入军团，不写 F777。 |
| Ponpei，F522 未设置 | 前篇已有 F777 许可分支。 |
| Maid，F522 已设置/F256 未设置 | 1798–1799 解释 King 的玩笑和特殊宝物；两个文本调用均返回后设置 256。 |
| Maid，F522 已设置/F256 已设置 | 1800–1801 提及 Floor World 东北的冒险者并请队伍拜访；不再写标志。 |
| Maid，F522 未设置 | 1777；不检查/写 F256。 |

F256 为临时标志：每次显式全新探索加载均清 256–383，即使同一地图。场景的 reloadMap 不重置它。
该次加载内重复与 Maid 交互进入第二分支；以后全新重入可恢复第一分支，即使 F778 已设置。
SetFlag 前中断使 256 仍未设置；之后则无回滚。F522/776/777/778 在重置范围外。
这些均不确立已观察对话。

事件行动位域将 Maid 转向玩家但不还原，将 Ponpei 转向后在返回时还原保存的朝向；它们不是接近限制。
Maid 选择无立绘/bleep 3，Ponpei 无立绘/bleep 6。
包装器暂时禁用实体更新，回调后有条件地还原朝向、关窗口并重新激活更新。
这些服务须返回才有局部完成；没有军队招募、Balloon 获取或新地图请求。
F522 独立抑制棋盘启动区域（事件）。
[守卫与离开后续](cotton-balloon-through-floor-world-entry.md#出口守卫读取宝箱状态)
解释 F212/F258 放行、包装器还原朝向及独立的自身瓦片物品匹配。

## 败北、撤离与重复返回

**已确认：**败北更新解锁标志，发送耗尽音乐/文本，还原 Bowie HP、将无符号金币减半，并以 d4=-1 返回 Egress 坐标。
获准的 Egress/Angel Wing 扣法术 MP 或移除选定物品，执行文本/闪光/关闭，更新解锁并以 d4=0 返回 Egress 坐标。
准入不证明菜单可用、MP 或物品持有。两条路径均不运行胜利 after/加入，不设置 522/472。
解锁更新在 522 未设置时保留 422，522 已设置时清 422。

Battle22 没有特殊 Egress 目的地。获准区域（事件）写 EGRESS_MAP=22；若保留该值且 BattleLoop 的 F399 已设置，
存档点为 Desktop `(5,8)`、Up。EGRESS_MAP 改变则选择自身行或默认 `(1,1)`、Up；
直接 F399 未设置调用使用 Map3 `(56,3)`、Down。
F64 独立重置已保存木筏坐标；Map22 走默认 Map69 `(42,15)` 重置，F64 未设置则保留。
这些不是胜利种子或场景后位置。

败北/逃脱的即时返回也绕过 CheckBattle。未完成首次遭遇后保留的 F422 不立即重新准入 Battle22；
后续普通准入可以使用它。显式 Map22 探索按实际 F522 加载实体群/init 门控，不按结果符号选择 init。
传入 522 已设置/778 未设置，即使非胜利后也可运行公告。
522/778 已设置的重复胜利仍生成种子/加入/写标志，但跳过场景，因此没有自动 `(25,12)` 放置。
挂起仍为独立 F88 保存/继续路径，不证明跨进程持久性。

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `546ccbb21ab1e9da42f5899fe7eae86ead18d255` 包含
[#912/PR914 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/914#issuecomment-6175364090)及
[#911/PR913 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/913#issuecomment-6175260041)。
研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
和 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
本指南不增加提取、重新获取源码或运行时观察。

综合前对抗审阅检查完整结果所有者、初始名册指南、链接的抵达/交互与请求使用方、生命周期合同、
fixture 载荷、schema 及既有验证器选择：

| Fixture / 窄复现所有者 | 已审阅边界 |
| --- | --- |
| [sf2-battle-loop-static-v1](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，`uv run sf2 h2 battle-loop` | 计数资格、治疗和 killed 清理；kill-all HP 写入不同于选定首领尾部。 |
| [sf2-battle-control-static-v1](../../../../tests/fixtures/h2/battle-control-static-v1.json)，`uv run sf2 h2 battle-control` | 两个有序结果检查、胜利/败北返回和挂起；选定种子/门控操作数来自已接受直接关联。 |
| [sf2-battle-cutscenes-static-v1](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，`uv run sf2 h2 battle-cutscenes` | Bowie 存活/首领死亡检查和共用加入尾部；精确 slot22 目标/表需要研究的完整选择。 |
| [sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json)，`uv run sf2 h2 map-init` | 完整十一操作 Map22 init、抵达优先分支及返回后 776/778 写入。 |
| [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`uv run sf2 h2 map-script-engine` | 选定 cs_5996E 重载/摄像机位置及处理器局部放置/文本/服务边界；汇总本身不证明组合自然执行。 |

其 schema 和验证器仍为既有
[loop](../../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../../src/sf2tool/h2/battle_loop.py)、
[control](../../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../../src/sf2tool/h2/battle_control.py)、
[cutscenes](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../../src/sf2tool/h2/battle_cutscenes.py)、
[init](../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../src/sf2tool/h2/map_init.py) 及
[script](../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)。
精确十四行、两个复制、十二命令、字面文本及直接完整加载/随从/回调使用方仍由研究
[复现边界](../../../research/battle22-outcomes-and-desktop-return.md#reproduction-and-retained-limits)负责。
routeTargetsParsed=false 的路由汇总不证明选定槽位目标。未发现已接受语义冲突。
更早运行时问题队列依 ADR0014/0016 仍是暂缓歧义，不是获取指令。

保留 [#912 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/912#issuecomment-6175333338)：
head `9cd21b5766caa4e939913aef2c818e9097681f98`、tree `6c76d0b0cd5d30d36f4201d57f815d01aa98aa41`、
base `f8cd67cd3818a1790b76354922015c13d8af083a`、
[CI38614495457](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38614495457)。
未改变的 18,285,979 字节对在 0.084s 内比较通过；scope/research-public 成功，engine/adapter/H4 按范围跳过。
过大读取、猜测配置/源码路径/语料键、Windows 206 拒绝写入及路由 output.romSha256 的 `KeyError` 均随修正保留；
没有游戏玩法套件失败。本综合指南不重跑或替换该收据。
[#911 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/911#issuecomment-6175244286)
保留仅 scope 的 [CI38613823392](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38613823392)
及其已修正 URL/驱动器标记诊断。
前篇[证据段](battle22-initialization-and-roster.md#证据审阅与剩余限制)保留不同的 #908 原始/变基对象及 CI、
#907 仅 scope CI 和链接的 #904/#903/#900/#899 失败/修正，包括 csc2D FLAGS_A5/6 绕过、工作布局重载及 RightLeft 朝向。
这些结果仍在其所有者收据中。

直接验收检查完整正文/diff、数量和坐标/身份区别、溯源、链接/片段、英文/私有边界及空白。
检查已配置的干净已提交依赖规划器和实际最终 head CI。
不单因正文选择局部 normal/full/engine/Godot/H3/H4 或翻译套件。

**未知：**自然结果/返回/交互；传入标志、出战队伍、HP/职业/物品、查找/当前与已保存几何、模式/木筏；
已完成输入/行动/文本/滚动/淡入淡出/服务；可见/可听演出、可用控制与持久性。
上文反例是源码状态，不是已执行场景；依
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和
[ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md)，也不构成自动运行时队列。
已接受 #916 后续属于 [Cotton Balloon 指南](cotton-balloon-through-floor-world-entry.md)。
这既未完成战役，也未完成 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
