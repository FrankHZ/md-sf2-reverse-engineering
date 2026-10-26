# Creed 离开、矮人恢复与 Cannon 赠礼

> 本文是[规范英文原文](../../synthesis/creed-departure-dwarf-recovery-and-cannon-gift.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

Fairy 请求见生病的矮人，为他检查，随后布置明亮的恢复序列。矮人恢复动画，说自己感觉好了。Fairy 拒绝给自己的奖励，请矮人改为帮助出战队伍首领。他们出于感谢提出赠送 Cannon，随后返回挖掘通往 Grans 的道路。

**推断：**这是 Fairy 帮助矮人、矮人回报帮助的故事。**已确认：**恢复布置、物品栏处理、挖掘、调用方标志及后来地图重建是独立操作。**未知：**自然穿行与交互、实际传入状态、呈现完成、可用控制及存档持久性。恢复不改变战斗员 HP；赠礼既不保证发生，也不是通用的一次性奖励。

本指南接续 [Oddler 留下与 Fairy 交接](oddler-stays-and-fairy-handoff.md)，使用 main `c430f26f69a01a0f6e917660e34503c6bcccbec4` 上[已接受的 #961 研究](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md)及其已接受前篇。它止于紧接的实体/区域（事件）返回及相关重新入场后果。Cannon 使用、后续旅程和 New Granseal/Astral 内容在本指南之外。此 Layer B 解释不选择重制规则或新观察工作。

[返回 New Granseal 与 Astral 跟随（中文）](new-granseal-return-and-astral-following.md)独立接续选定的矿洞至城堡返回及回顾式顾问觐见；不把持有 Cannon 设为觐见前提。

## 离开 Creed，进入矿洞

**已确认：**选定路线是 Mansion33 → Devil's Tail67 → Southwest Parmecia68 → 矿洞内部30。[路线所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#selected-departure-and-mine-entry-route)保留精确表行、请求消费者、坐标、战斗前提和模式差异。这些是条件连接，不是已证明的强制地形行走路线。

宅邸任意 X/Y31 的行请求无滚动进入 Map67 `(26,23)` Up。Map67 的 X63/任意 Y 边界则请求 Right scroll16 进入 Map68，X1 并保留 Y。该滚动路径在探索内改变当前地图和工作布局，保留行动者及随从查找/列表。它跳过无滚动 MainLoop 战斗检查、新行动者构建、临时标志重置和选定 init。请求的 Right 朝向字节不在那里应用；采样的是主行动者当前朝向。普通 X1 推导还要求返回坐标高字节为零。

Map68 的 `(57,24)` 行请求无滚动进入 Map30 `(8,20)` Up。Map30 是直接从68 到达的矿洞内部；不同的村庄外部29 不是隐含中途站。其唯一出口行从任意 X/Y21 请求 Map68 `(57,25)` Down，仅建立这个局部返回边界。其他表格替代路径仍是可选替代，不是必经绕路。

无滚动入场退出探索，可在普通地图准入前选择战斗。Map67 的 Battle21、Map68 的 Battle19 标志以及独立步行/随机战斗区域（事件）保留各自已接受消费者；边界滚动本身不调用 CheckBattle。遇到获准战斗便停止，不假设旅行不中断。Map30 没有战斗表行。选定地图均不由全局替换表按标志切换。

显式重新、无战斗的 Map30 入场中，探索清除待处理状态，执行步数计数及存活/不死己方角色治疗，加载地图，重建行动者，清除临时标志256–383，设置80，再调用选定 init。这种入场治疗独立于 Fairy 恢复场景。F759–762、F785 和随从标志保留，不受临时重置影响。CURRENT255 复用保留当前人口；后续访问器可从变化标志选择表，而不重建这些行动者。音乐/淡入淡出/init 与待处理事件优先级仍使地图入场区别于可用控制。

实际工作布局传送/区域（事件）类型、传入坐标高字节、移动/碰撞、待处理消费及玩家模式仍是前提。Caravan mode1 没有步行的区域（事件）产生器。[已接受旅行指南](caravan-boarding-and-southwest-parmecia-travel.md)保留不同的 mode0/1/2、木筏与随从规则；此路线不建立登乘、登陆或自然步行观察。

## 矮人与 Fairy 是不同的行动者

**已确认：**重新构建 Map30 的选择采用最后设置标志胜出：基线、F785，然后 F761。即使 F785 已设置，F761 仍胜出。三个配置共用回调及 init，但居民列表不同；F69/F760/F762 不选择它们。

| 选定人口 | 居民与初始安排 |
| --- | --- |
| 基线 | Dwarf128 在 `(6,10)` 附近行走；129 在 `(7,4)` Up；130 在 `(15,12)` Down；131 在 `(15,11)` Down。没有132。 |
| F785 | Dwarf128 `(11,11)` Down；129 `(16,12)` Left；130 `(15,12)` Down；131 `(15,11)` Down；132 `(15,13)` Up。全部以 Init 开始。 |
| F761 | 没有居民。回调地址仍存在，但重新构建不提供矮人。 |

[人口/init 所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#complete-map30-population-and-init)保留声明行动、查找分配及变化状态用例。Resident130 是病矮人，不是 Fairy。Fairy 是由 F69 独立选择的 FOLLOWER_C158、lookup62；居民128–132 使用 lookups32–36。他们是 NPC 精灵，不是军团成员或具有交战 HP 的病患。

通常 Creed 之后的选择器 F67/F69 已设置、F68 未设置，提供 Peter 与 Fairy，省略普通 B31/Oddler。没有额外随从时，F785 居民占 physical3–7，位于 Bowie0、Peter1、Fairy2 之后。**缺失 B 的零查找解析为 physical0/Bowie**，所以不能简单把场景 B 命令叙述成 Oddler 仍在队伍。其 B 跟随请求可让 Bowie 跟随 Peter；B 布置/朝向也能影响 Bowie。稍后显式 Bowie 布置可覆盖那次布置。若 F68 也已设置，B 则在所述普通分配下是不同的行动者。F74/F70 及重复 ID 可提供其他 B 映射；隐藏查找变为 -1。局部没有修复来规范化缺失或别名行动者。

Map30 init 先在761 已设置时运行 F761 布局程序，再独立于此前分支，在760 未设置时运行患病配置。两者可在空 F761 人口上运行，缺失130 时患病操作可转到 physical0。患病配置禁用130 动画，转 Right，更新精灵，等待其行动 Idle 指针，再次停止动画。它不全局暂停移动或治疗 HP。760 已设置时，重新 init 跳过患病配置，不重放恢复或重布居民。实时改变标志不替换当前角色阵容。

## 对话与恢复邀请

**已确认：**[六个实体回调与区域（事件）所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#six-entity-rows-and-the-two-row-zone-table)把交谈与恢复准入分开。Dwarf128 起初说为 Mithril 从 Grans 而来。760 设置后，他的回调改为调用赠礼辅助程序，没有761 守卫。Dwarf129 报告地震堵住隧道，需要生病的朋友；130 在抱怨冷之前和之后发抖。这两段对话均设置759。761 未设置时，Dwarf131 说有矮人去找 Fairy，并设置759；761 已设置时则邀请大家去 Grans 的 Dwarven Village。Dwarf132 唱着挖向 Grans 的歌。默认仅返回。

这些回调并非全部受恢复门槛控制：直接调用130 回调在760 后仍发抖，129 仍报告病情，131 只检查761。基线没有132，重新761 没有任何居民。因此，选定回调地址不证明普通玩家能与该行动者交谈。目标范围、布局、列表排除、查找顺序及实际精灵仍是交互的一部分。包装器使128 转向玩家但不恢复，使129/131/132 转向并恢复，对130/默认则两者都不做。

`(7,13)` 区域（事件）恰在 **F69 已设置且 F760 未设置**时准入恢复。它不要求759、762、785、761、HP、可用物品栏或预期人口。更早对话不是局部前提。F785 通常提供五矮人布置，但它不是准入标志。默认、69 未设置或760 已设置路径返回，不执行场景/辅助程序/调用方写入。步行必须先产生适当区域（事件）请求；传入回调坐标不证明自然抵达。

## Fairy 检查病人

**已确认：**[完整恢复场景](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#complete-recovery-scene)按以下阶段排序。行动者标签描述预期的不同映射；此前别名、HP、人口和模式前提仍适用。

Fairy 收到等待式较慢 Init。随后场景请求方向2 的 B→Peter 跟随；编码高字节在解码 B 前检查 Bowie HP。Bowie HP0 跳过该关系，而非整个恢复。之后 Fairy 收到等待式自定义零速行动、目的地 `(8,11)`，并询问生病矮人在哪。Init 和自定义行动替换此前行动指针，不移除已有探索列表条目或重置位置。后续目的地有独立移动/坐标等待。

Fairy 左右转头，不等待行动完成，同时摄像机改变视野。Dwarf128 向 Left 走一步，欢迎她，跳两次且各有等待，再走 Right3，面向她，请求检查。摄像机跟随 Fairy 经 Right4、Down1、Right2 走向病人，再转移视野。又一次暂停后，Fairy 希望自己没来晚，开始检查。她跨几次暂停发抖两次，请求白色闪光、白色淡出、另一次闪光序列及白色淡入，然后再次暂停。这些是呈现指令和服务等待，不是已观察医疗效果或实测恢复时间。

Dwarf129 左右转头，走 Down1，询问病人情况。Dwarf132 摇头，说还不知道。随后病人130 收到等待式 Init 和 Jump 行动，恢复动画并喊出声。他自身不等待的左右转头行动和暂停后，朝 Up 提问。Dwarf129 往回走 Up，问他是否好了，130 说感觉很好。

动画恢复与自述在故事中表达恢复；**推断：**Fairy 帮到了他。**已确认：**这些命令不改变战斗员 HP、不复活己方角色，也不改变军团成员资格。布置/朝向中的具名己方角色 HP 守卫不同于无守卫的 Init/行动/手势/闪光/隐藏路径；NPC130 动画命令不检查 Bowie HP。变化行动者、死亡己方角色或别名仍保留这些差异。

## 感谢与 Fairy 离开

**已确认：**dwarf128 向 Right 走一步，转向 Fairy，其余矮人朝 Left，Fairy 朝 Up。他问如何感谢她；130 点头。Fairy 拒绝个人奖励，请他们改为帮助 LEADER。场景将 B 放在 `(7,11)` Right、Peter 在 `(7,10)` Right、Bowie 在 `(8,11)` Right，受各自不同布置守卫和实际查找约束。

摄像机跟随 Fairy 经 Left2、Up1、Left3，然后她道别，祝 LEADER 旅途平安。随后释放摄像机跟随。她走 Up2、Left3、Down3；Bowie、Peter 和 B 朝 Down；她走 Right1、Down5 后被隐藏。摄像机请求 `(4,7)`。Bowie 收到不等待的 Right2 行动，同时128 走 Left3 并有等待，然后场景返回。结尾不等待 Bowie 行动指针；辅助程序稍后的 Init 可替换它。

Hide 把 Fairy 当前/目的地坐标移到 `$7000`，使匹配查找失效，并从探索列表移除她的物理行动者。记录/行动在普通实体服务下保持休眠。这隐藏当前 C 行动者，却不清除 F69；区域（事件）调用方更晚才清除69。它不是军团成员移除，也不是已保存离开的证明。

[文本/服务所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#exact-text-and-immediate-services)保留全部 28 个精确文本关联 1705–1732、说话者操作数和控制。LEADER 是 UpdateForce 后首名出战军团成员的名字，可不同于 Bowie。普通映射下，Fairy/Dwarf 精灵选择不同提示音且无立绘；变化精灵或别名会改变此上下文。恢复、感谢或挖掘对话中没有 Yes/No 菜单。

N 插入换行；W1/W2 等待输入，W2 有继续/声音行为。单条文本关闭窗口并 sleeps 10；继续文本不如此。Fairy 单条告别1725 内部的 W2 与单条1731 末尾的 W2 是不同等待位置。入口清除跳过状态后，Debug/P2 Start 可锁存过场文本跳过，跳过游标推进/显示和编码等待，而其他命令仍执行。辅助程序直接 txt 调用及其 Sleep100 是独立消费者。仅源码序列不能证明渲染时序、输入完成或已观察文本/音频。

## 接受或推迟 Cannon

**已确认：**恢复返回后，区域（事件）调用 [Cannon 辅助程序](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#cannon-helper-and-complete-digging-scene)。它把恢复归功于对话对象并提出赠礼，然后计数 **Bowie** 全部四个非 NOTHING 物品槽。Slot0 不必空闲。固定接收者是 combatant0，独立于 LEADER 替换名字；没有全出战队伍容量后备方案。

若字计数等于 4，辅助程序说对话对象双手已满，推迟赠礼。它在 AddItem、物品音乐、761 或挖掘之前返回。获准序列中的恢复已发生，区域（事件）仍继续设置760 并清除69。若当前 dwarf128 保留，之后他的760 已设置回调可在腾出空间后重试邀请。

否则，辅助程序以 Cannon `$72` 为 Bowie 调用 AddItem。AddItem 使用按掩码识别的首个 NOTHING 槽，返回成功/失败，但调用方不检查结果。它请求物品音乐，叙述 LEADER 收到 Cannon，sleeps 100，关闭文本，设置761，请求先前音乐并运行挖掘。在未变、有效的物品栏下，计数小于 4 提供空槽；收到物品的叙述仍不是独立检查过的 AddItem 结果。

辅助程序不以761、已有 Cannon、HP、职业或成员资格作守卫。直接重复辅助程序或与保留128 交互时，只要 Bowie 有空间就可再次授予，包括761 已设置时。满则再次推迟。重新761 入场移除居民，通过人口重建限制普通访问；这不实现通用一次性赠礼事务。直接辅助程序自身不设置760 或清除69。

辅助程序继承台词上下文，而不显式选择128。实体交互提供其包装器解析的上下文；普通未跳过的区域（事件）链中，最初直接 txt 沿用 Fairy 最后告别的上下文。挖掘脚本更晚才显式选择128。不能用感谢叙述和固定接收者来虚构辅助程序说话者选择器。

## 矮人返回挖掘

**已确认：**[完整挖掘场景](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#cannon-helper-and-complete-digging-scene)将129–132 转 Left，请求视野 `(6,7)`。Dwarf128 解释 Cannon 可摧毁巨石，走 Right1，召集矮人返回 Grans 并命令挖掘。这是建议和意图，不是 Cannon 使用或旅行。

四名矮人129–132 分两组跳跃。每组给四者都安装行动，但只等待132。Bowie 收到等待式 Init 和 Left1，Peter 收到目的地 `(8,10)`。矮人向 Left 移动：128/132 为 2，129/130/131 为 1，仅等待132。随后 Bowie、Peter 和 B 朝 Right。

五名矮人收到不同北行路径。Dwarf128 走 Up2、Left1、Up3；131 走 Left3、Up2、Left1、Up3；132 走 Up2、Left3、Up2、Left1、Up3；129 走 Up1、Left5、Up2、Left1、Up3。这四次安装不等待。Dwarf130 则先在自身行动中 wait 40，走 Up1、Left4、Up1 并等待，暂停，走 Down1、朝 Left、点头，再走等待式 Up2、Left1、Up3 路径。

最后，128/129/130/131/132 被放在 `(7,3)/(8,3)/(7,4)/(8,4)/(7,5)` Up。脚本从 `(21,6)` 向 `(7,5)` 复制 2×2 布局矩形并返回。这些布置设置当前/目的地坐标，不清除其他行动指针。等待132 或130 不证明所有矮人都完成；在变化时序/映射下，更早异步行动仍可能有关。这里没有矮人隐藏或传送。

当前场景只执行该 2×2 复制。后来重新761 init 重复它，并额外从 `(21,0)` 向 `(6,0)` 复制 4×4，同时不构建居民。当前复制及额外重新加载复制，均不证明渲染出的挖掘、碰撞开口或自然可用后续路线。[行动/布局所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#action-layout-and-return-semantics)保留各行动者等待、布置、摄像机、闪光/淡入淡出和复制语义。

## 返回、重复与中断

**已确认：**区域（事件）顺序为恢复 → 辅助程序 → 设置760 → 清除69 → 返回。辅助程序有空间分支中，AddItem 先于叙述、761 和挖掘。因此，中断可留下物品先于761、761 先于挖掘完成，或恢复/辅助程序效果已完成而760/69 尚未写入。设置760 后、清除69 前，两标志可均已设置；重复区域（事件）因760 跳过，不补完该中断前缀。没有回滚、原子物品/场景事务或局部存档操作。

已有761 不拒绝传入的69 已设置/760 未设置区域（事件）。重新761 可提供空角色阵容，让缺失居民别名到主角，而非修复场景。基线/F785 人口、当前与重建状态、变化 HP/职业/成员资格/模式、缺失 C/B 及别名保留实际分支。区域（事件）/辅助程序不写759/762/785；独立患病回调写759。直接调用恢复脚本省略门槛、辅助程序和调用方标志；直接调用辅助程序省略恢复及这些调用方标志。

区域（事件）包装器在回调返回后关闭立绘/文本，等待一个 VInt 和 physical0 坐标相等。这既不是全部行动者/行动屏障，也不是可用控制确认。实体调用的辅助程序则通过实体包装器的朝向/窗口/更新服务返回，没有区域（事件）的 physical0 等待。脚本结束可等待视野滚动，然后返回调用方；不直接授予控制。全文始终区分跟随/列表改变、隐藏与持久随从选择器。

**未知：**自然路线/区域（事件）/交互可达；实际标志、布局、模式、HP、职业、物品栏、成员资格、人口、查找/列表及 AddItem 结果；碰撞/行动/文本/立绘/视野/淡入淡出/音频服务完成、可见恢复、可用控制及存档/读档持久性。这些问题不创建采集或模拟器队列，也不授权扩大后续战役范围。

## 证据审阅与保留结果

对抗审阅检查已接受 Layer A 文字与选定公开载荷/schema/消费者表面。未发现阻塞矛盾。公开脚本 fixture 保留处理器事实和引用，不是私有完整场景语料；这个差异限制本次审阅。

| 审阅表面 | 处置 |
| --- | --- |
| [#961 研究](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md)及已接受旅行/告别前篇 | 路线、实际行动者、完整恢复/辅助程序/挖掘顺序、文本及变化状态限制在已接受静态边界上一致。 |
| `sf2-map-events-static-v1`：[routing](../../../../tests/fixtures/h2/map-events/routing-setup.json)、[实体程序](../../../../tests/fixtures/h2/map-events/entity-programs.json)、[区域（事件）程序](../../../../tests/fixtures/h2/map-events/zone-programs.json) | 选定 Map30 行、六个实体上下文、两个区域（事件）上下文和共享返回重叠保留分支极性与调用方顺序。不从回调目标虚增辅助程序覆盖。 |
| `sf2-map-init-static-v1`：[fixture](../../../../tests/fixtures/h2/map-init-static-v1.json) | 三条路由共用 init；独立761/760 决策及完整嵌入布局/患病体一致。 |
| `sf2-map-script-engine-static-v1`：[fixture](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)及 [schema](../../../../schemas/h2/map-script-engine-fixture.schema.json) | 选定布置/行动/hide/follow/布局/对话事实保留守卫与服务边界，不证明自然执行。 |
| [事件加载器](../../../../src/sf2tool/h2/map_events_fixture.py)、[事件验证器](../../../../src/sf2tool/h2/map_events.py)、[脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py) | Schema/身份/顺序检查与源码输出溯源/相等是独立职责；合法 fixture 形状不是新源码证据。 |
| [名册合同](../contracts/party-roster-state.md#处理器边界)、[对话合同](../contracts/dialogue-system.md#已确认的静态合同)、[物品事务所有者](../../../research/map-event-item-transactions.md) | 成员资格、台词、服务和事务接缝不提供此场景的自然可达、已完成赠礼或持久性。Cannon 辅助程序细节仍归 #961。 |

[复现所有者](../../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#reproduction-and-review-boundary)固定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。保留比较未变，18,285,979 bytes、0.081s，含 144 个记录、25 个完整内联体、两个自定义体及 28 个文本关联。所有者命令仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events` 和 `uv run sf2 h2 map-script-engine`。原英文综合切片的检出缺固定 upstream/保留对：不获取、不重新生成，也不声称新比较。

[#961 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179316611)、[变基回执](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179363538)及 [PR964 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/964#issuecomment-6179370463)保留原始 608987b9/tree6c4bf429/base936748b0 和最终 eea8829e/tree2b329756/base3b5df736、CI38643445482/38643827378，以及已完成的 ac_setSpeed 编码大小检查失败/修正。所链接原始诊断及 #957/#953/#949/#945/#941/#937 回执、失败和未知项，包括 [#954 回调修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)，均保留，不重启。

[#962 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179258820)、[PR963 审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/963#issuecomment-6179272262)、[修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179318374)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/963#issuecomment-6179327445)保留原始 155899b8/CI38643054890 及修正后 7793bf19/CI38643473924。Fairy 的宅邸声明正确为 `(17,10)` Up，带 eas_InitSlow；该声明独立于矿洞随从重建。没有已完成失败被改称中断或重新运行。矛盾交回研究所有者，不由 Layer B 修改研究。直接文字、链接/锚/导航、范围/私有边界和 diff 检查、已配置的提交后 planner 及实际精确 head CI，属于原英文切片；这些文字改变不引入整套运行时或翻译验证。
