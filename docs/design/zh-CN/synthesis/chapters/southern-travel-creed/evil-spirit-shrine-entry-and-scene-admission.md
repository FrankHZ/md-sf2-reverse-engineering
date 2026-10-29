# Evil Spirit Shrine 入口与咨询准入

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境、验证及中文同步推迟记录保留原英文综合切片的时间范围。

前篇跟随 Creed 和 Fairy 从宅邸主房间离开的请求，前往咨询 Evil Spirit。**推断：**神殿准备的演员属于该预期咨询。**已确认：**宅邸有独立出口进入 Evil Spirit Shrine，两个局部转移连接其声明区域，后续区域（事件）请求可准入场景。进入房间本身不启动咨询。

本指南从 [Creed 恢复与主房间离开](creed-restoration-and-basement-departure.md)继续，仅消费 #945/PR948 已接受[入口/准入研究](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md)。它止于 cs_5B6C0 调用。咨询对话与效果、招募、Oddler 离开和场景后控制仍为更晚边界。**未知：**自然旅行、实际传入状态、服务完成、表现及持久性。这是实现中立的 Layer B 解释，不是重制需求或完整战役路线。

后续 [Evil Spirit 咨询与条件区域（事件）返回](evil-spirit-consultation-and-zone-return.md)解释完整获准场景及紧接的调用方/包装器边界。

## 从宅邸进入神殿

**已确认：**Map33 的 `(9,5)` 出口请求 Evil Spirit Shrine34 `(26,28)`、朝 Left。宅邸另一出口为通配 X、Y31，请求 Devils Tail67 `(26,23)`、Up。记录和直接匹配器均不检查 F783、F784、F523 或恢复完成。前篇恢复标志与邀请不成为额外神殿入场前提。

这些是条件目的地，不是已演示步行。普通步行先从当前位置和移动输入派生候选位置。边界、斜坡和实体阻挡影响路径；实际工作布局必须将候选分类为传送图块（掩码 `$3C00` 下 `$1000`），才调用传送匹配器。匹配器按顺序扫描，将负触发字节作为通配符，否则比较瓦片坐标低字节。仅表中坐标既不证明可通行几何，也不证明事件已产生。

匹配写待处理传送请求。WaitForEvent 优先处理待处理事件；ProcessMapEvent 在分发前清类型。不滚动消费者将元组返回 MainLoop，其中 SwitchMap 无 Map34 替换，CheckBattle 无 Map34 战斗行，因此选定路线准入探索。介入写入者或未消费请求仍为独立问题。

显式目的地字节仅替换从 physical0 先前位置派生坐标的低字节。将结果读作精确 `(26,28)` 假设继承高字节为零；目的地不修复任意传入高字节。完整地图探索加载也意味着坐标不相对于 Battle23 裁切战场。已接受研究负责[完整路线及直接消费者](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#mansion-exit-and-fresh-map34-admission)。

## 在神殿区域之间移动

**已确认：**完整 Map34 表含三个不滚动传送：

| 候选触发 | 请求目的地 | 实体群结果 |
| --- | --- | --- |
| `(26,28)` | Creed Mansion33、`(9,5)`、Right | 显式 Map33 准入重建按标志选择的实体群，并运行选定 init。 |
| `(22,29)` | CURRENT255、`(24,2)`、Left | 保留 Map34 及既有居民。 |
| `(24,2)` | CURRENT255、`(22,29)`、Right | 保留 Map34 及既有居民。 |

两个声明主图层区域为 `(0,0)..(26,25)` 和 `(19,25)..(29,34)`。**推断：**相互转移连接神殿两个声明区域。它们不是已走阶梯、自动反向传送或已演示地下室抵达的证据。三个记录均保留上述实际候选/布局/请求和坐标高字节前提；均不检查 F783、F784 或 F802。

CURRENT255 走保留地图路径。更新主角当前/目的地位置、朝向和己方精灵，可在步行模式重置既有随从位置。不重新声明居民，不清新地图临时标志，也不经新入场分支设置 F80。公共加载/init 仍运行，使用 Map34 与当前选定配置。Caravan/木筏模式有不同随从与木筏处理；局部转移不将它们规范为步行。

同样，请求宅邸不证明返回完成、特定场景后实体群或玩家控制恢复。目的地配置和 init 仍属 [Creed 指南](creed-restoration-and-basement-departure.md)。源码表和保留路径消费者见研究的[三个传送审阅](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#return-only-local-surfaces-and-the-three-warps)。

## 新房间准备了谁

**已确认：**Map34 有一个默认配置和一个 F784 覆盖。F784 未设置选择21 条居民行，已设置选择空居民列表。两者选择相同实体事件、区域（事件）、描述、物品事件与仅返回 init。F783、F523、F802 不选择神殿配置。新显式 Map34 入场重建行动者并清临时标志256–383；F783/F784/F802 不受该重置影响。

基线准备 Creed128 于 `(7,10)`、Fairy129 于 `(6,10)`，均 Up。还将 Evil Spirit130、Beastman131、Boy132 和十六个独立 EFFECT1 居民133–148 分配于 `(63,63)`、Up。这是21 条独立居民记录，不是一个效果的十六帧。基线文件自身无终止符：紧接包含的 F784 替代文件唯一 `$FFFF` 字结束基线扫描，也使替代列表从首字节即为空。

**已确认分配细节：**Evil Spirit 精灵252 走特殊路径：logical130 映射 physical47，存储 ENTNUM32。它消费查找条目但不推进普通物理计数器。Creed/Fairy、Beastman/Boy 及全部十六个 EFFECT1 精灵63 行使用普通分配。选定 N 个普通随从时，Creed/Fairy 占 N+1/N+2，Beastman/Boy 占 N+3/N+4，效果占 N+5 至 N+20。研究仅 F67/F68 的示例中，Peter 与 B/Oddler 占 physical1/2、Creed/Fairy3/4、Beastman/Boy5/6、效果7–22；Evil Spirit 仍为47。逻辑 ID、查找偏移、物理槽和 ENTNUM 在此是不同身份。

五个角色行安装 eas_Init；效果行安装 eas_InitFixedSprite。两者在设置移动/碰撞/精灵属性后均止于 eas_Idle；固定精灵版本还禁用自动朝向与 motion。其精灵大小写入指向共享状态，不是独立证明的持久身体比例。**未知：**这些异步行动是否在自然到达的房间执行、稳定或渲染。`(63,63)` 仍是分配，不是 HideEntity；EFFECT1 名称不证明召唤、爆炸、动画或可见布置。研究保留[全部行身份与完整行动主体](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#setup-pointers-and-all-resident-identities)。

主角/Bowie 以传入抵达元组和己方精灵独立声明于 physical0。随从依自身标志、职业/HP 和映射状态；出战队伍成员资格不能替代跟随。Map34 没有大地图 Caravan 前缀。F69 下 Fairy 随从为 logical158，独立于 resident129。重复 B 随从行可分别分配并覆盖同一查找；缺席 Peter/B 或居民留下可别名 physical0 的零查找。咨询调用前没有局部实体群检查修复别名。

因此空居民覆盖不等于空地图：主角、选定随从及可能的木筏独立存在。PLAYER_TYPE 不重置；步行0 是一致前篇前提，不是局部强制。F64 与保存 RAFT_MAP34 可声明 physical31，木筏 mode2 则有自身主角精灵、随从及停放木筏行为。过多传入普通实体群可与 physical31 冲突；分配公式不认证任意标志混合。己方精灵使用实际职业/HP，新探索治疗不是通用复活。固定居民精灵声明仍独立于己方 HP。

## 房间等待独立请求

**已确认：**两个配置的 init 均仅为 rts，不赋场景、位置、标志、待处理请求或 EGRESS_MAP。局部实体事件和物品事件表仅含返回默认；描述回调返回而无已处理结果。标志、踩踏、屋顶、宝箱及其他物品表仅含结束记录。这些是局部缺席声明：共享交互/搜索包装器仍可运行、关闭窗口并等待，不意味着全部通用交互缺席。

新加载后，若待处理状态和服务允许，探索可到达摄像机/控制器尾部。这是条件源码流，不是观察到的可用抵达。`(26,28)` 和两个局部传送目的地均不因出现在表中就产生场景请求，且均无 Y13。

**已确认：**实际候选布局图块为类型 `$1400` 时，普通移动可写区域（事件）请求。该候选用当前位置加移动偏移；请求 X/Y 字则来自目的地位置加这些偏移，使用有符号字算术并除384。只有位置稳定前提下它们才描述同一瓦片。后续碰撞检查可在请求已写后抑制移动。产生 type6 既不证明移动完成，也不证明任何配置行将匹配。

Map34 区域（事件）表精确匹配通配 X255 和请求 Y **低字节13**；其他 Y 选择返回默认。X 通配精确为 `$FF`，不同于传送匹配器的任意负字节规则。非零 Y 高字节不阻止低字节匹配。表不确立每个 X 上的区域范围自然触发：符合条件请求仍需实际产生方，除非在受控接缝直接传入。

## 准入独立于恢复标志

**已确认：**消费 type6 清待处理类型，并在选择当前区域（事件）表前为 physical0 安装 eas_Init。这是行动指针替换，无 HP 守卫、坐标吸附或执行等待；不证明回调开始时主角已停止。

Y13 匹配时，Map34_ZoneEvent0 检查 F784。已设置则返回，无场景调用及两个调用方标志写入。未设置则经 script 宏和 MAPSCRIPT trap 的 ExecuteMapScript 接口调用 cs_5B6C0。F783、F523、F802、恢复完成、居民、跟随、HP、EGRESS_MAP 均非额外回调谓词。**推断：**它准入预期咨询；**未知：**此边界上的咨询内容和结果。

只有该尚未在本指南分析的场景返回，调用方才先设置 F784、再 F802。这些是有条件的有序效果，不是完成事实或原子对。返回前中断可使两个调用方写入均不发生；两个写入之间中断可使 F784 已设置而 F802 未改变。场景内部可能有其他效果；本指南不声称只有这些写入，也不证明持久性。外部传入 F784 已设置可抑制场景，不证明曾运行。

回调返回时，区域（事件）包装器关闭立绘/文本、等待一个 VInt，再等待 physical0 当前坐标等于目的地。该等待不等待所有行动者/行动/服务。寄存器恢复同样依赖相关调用返回。这些边界不确立场景后玩家控制、招募或离开。精确回调及 trap/包装器关联属于[已接受准入所有者](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#actual-zone-request-and-scene-call-boundary)。

## 改变状态会改变可作出的声明

以下是**已确认静态判别示例**，不是已执行存档状态场景：

- 新显式 Map34 入场、F784 未设置时重建全部21 居民。只要 F784 保持未设置，重复入场仍重建，包括中断尝试后；两种入场均不自动调用场景。
- 新 F784 已设置入场不创建居民，并抑制 Y13 场景调用。基线创建后设置 F784 改变选择/准入，但不在 CURRENT255 转移时追溯移除居民。
- 空加载后清 F784 可准入调用，而居民查找仍为零。回调不创建缺失演员。
- 仅改变 F783/F523/F802 不改变神殿配置和回调极性；Map33 更早 init/历史可独立不同。
- 不同随从标志、HP/职业、模式或木筏状态可改变分配、别名、精灵及控制，不改变局部 F784 门槛。
- 传入 type6/Y13 请求可匹配而不演示自然布局可达性。不同 Y 低字节经默认返回。当前/目的地不一致和介入请求写入者保留独立限制。
- 场景未返回或包装器服务未完成，不证明调用方标志完成、包装器等待完成或可用控制。

## 证据审阅与剩余限制

**已确认溯源：**已接受 main `c6bbc1c11ea52d9a0828abf2280e9ff125bfaf91` 包含 #945/PR948 和已接受前篇。研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#changed-premise-checks-and-reproduction)保留源码路径、地址、操作数、直接消费者与命令。原英文切片不以并行咨询研究为本指南证据。

原英文切片的对抗审阅覆盖完整已接受正文、两个完整传送表、全部居民/分配/行动区别、仅返回局部表面及候选/布局/目的地/请求关联。选定可执行所有者为：

| 公共 fixture / schema 与消费者 | 检查边界 |
| --- | --- |
| sf2-map-events-static-v1:routing-setup，tests/fixtures/h2/map-events/routing-setup.json | ms_map34_ZoneEvents：两个精确记录、选择器、偏移、目标与源码行；map-events-routing-setup.schema.json 和 map_events_fixture.py 保留分片身份/字段组合。 |
| sf2-map-events-static-v1:zone-programs，tests/fixtures/h2/map-events/zone-programs.json | Map34_ZoneEvent0：`$5B69E..$5B6B4` 全部六操作、精确源码行/操作数、分支极性、cs_5B6C0 目标、先 F784 后 F802 及共享默认 rts；map-events-zone-programs.schema.json 和 target-program schema 保留结构。 |
| sf2-map-init-static-v1，tests/fixtures/h2/map-init-static-v1.json | expected.routeJoins79/80、pointerTableJoins80/81 和 primarySourceBodies50 将两个 Map34 配置关联至 `$5B6BE` 同一 rts；h2-map-init-static-fixture.schema.json 和 map_init.py 比较完整语义合同和溯源。 |
| sf2-map-script-engine-static-v1，tests/fixtures/h2/map-script-engine-static-v1.json | entityActionBridgeCommandFacts、entityPlacementCommandFacts、entityLifecyclePresentationCommandFacts 区分行动安装、定位与显式隐藏；组合 map-script schemas 和 map_script_engine.py 保留处理器/操作数边界。不证明当时未审阅的场景主体或执行。 |

schema 在 schemas/（map-event/map-script 分片在 schemas/h2/），消费者在 src/sf2tool/h2/。原英文切片直接对两个选定事件分片及预期段、init/script fixtures 做 schema 验证；按已接受正文检查精确区域（事件）行、回调操作和仅返回 init。map_events.py 验证器比较完整语义输出/溯源，不仅形状。选定关联未发现矛盾。更底层分配/布局声明保留已接受研究的源码审阅，不在此新执行或独立再提取。

原英文切片检出缺少固定 upstream 和保留私有对。窄复现所有者仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events` 和 `uv run sf2 h2 map-script-engine`，配合已接受保留比较配方。该正文切片不需要再生成、获取、运行时或新环境。[#945 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849)、[变基收据](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178108485)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/948#issuecomment-6178118055)保留0.084s 未改变18,285,979 字节对比较、源码关联、CI38634122032 和 CI38634788342，以及检查/换行/索引诊断失败。其链接的 #941/#937 修正事实和失败继续保留，包括1892 文本控制及编码 followEntity 高字节/Bowie HP 推理。这些是已接受结果，不是新重跑；未重启任何已完成失败。

原英文切片执行直接完整正文/diff、证据/链接/锚点、英文/私有边界检查和 git diff --check，再检查已配置干净已提交 planner 及实际精确 head CI。该仅文档改动不要求局部一揽子套件或翻译检查。**在原英文切片当时，中文同步仍被推迟**；这保留该切片的历史范围，不是本译文当前阻碍。

**未知：**自然旅行及活动布局/碰撞、传入标志/实体群、职业/HP/映射/模式/木筏、请求时机、服务/行动完成、渲染行动者/效果/摄像机/文本/音频、可用控制及持久性。咨询主体与全部后续招募/离开结果仍在此入口/准入指南外。[ADR0014](../../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)和 [ADR0016](../../../../../decisions/0016-remake-start-evidence-deferral.md)不创建自动运行时队列或扩大重制验收。
