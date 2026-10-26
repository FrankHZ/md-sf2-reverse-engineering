# Bedoe 雏鸟飞行与救援

> 本文是[规范英文原文](../../synthesis/bedoe-hatchling-flight-and-rescue.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Bedoe 的一只雏鸟提出演示飞行，两次尝试都不顺利，最后声称取得有限成功。另一独立场景编排坠落、birdwoman 惊呼及她向 Bowie 道谢。这些是调用方独立的可选互动。救援不要求演示或国王接受，birdwoman 报告安全也不确立之后招募。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/bedoe-hatchling-flight-and-rescue.md)支持下文选定对话、行动与状态边界。**推断：**编排描绘尝试飞行与救援。**未知：**自然互动/zone 可达、传入行动者、服务完成、可见飞行、坠落/接触和可玩连续性。本指南不增加重制能力或产品决定。Fnnn 表示标志 nnn；坐标与基本方向移动计数为地图瓦片，另注者除外。

[Volcanon 返回与国王选择指南](volcanon-return-through-bedoe-king-choice.md)解释周围 Bedoe 配置。其选择独立于这些事件。本文止于两个选定调用方返回及其条件控制交接；Bedoe 出口、Polca、Battle13 和未来 Skreech 招募仍在范围外。

## 寻找两个可选互动

**已确认选择：**仅 F722 选择 Map10 替代实体、实体事件、zone 事件与 init。配置查找重新解析当前标志；设置 F722 自身不重建行动者或调用任一互动。默认 F722 未设配置既没有查找 153 固定行，也没有此救援 zone 调用方。F723/F724/F799 不能替代 F722 选择。

| 选定调用方 | 准入与回应 |
| --- | --- |
| 与查找 153 交谈，F723 未设 | 运行飞行程序 `cs_5766C`；仅脚本返回后设置 F723。 |
| 与查找 153 交谈，F723 已设 | 再次请求结尾文本 1685；没有飞行、重新放置或新标志写入。 |
| Zone `(6,23)`，F13 已设 | 返回，不运行救援程序或写入 F724。 |
| Zone `(6,23)`，F13 未设且 F724 已设 | 返回，不运行救援程序或新写入标志。 |
| Zone `(6,23)`，F13 与 F724 均未设 | 运行救援程序 `cs_5779C`；仅脚本返回后设置 F724。 |

F13 是 Elric 已加入标志，此处仅作抑制区分条件。救援调用方既不读 F723，也不读 F799。反之，F724 不抑制飞行。这在谓词边界允许任一顺序，不证明每个存档都自然可达两者，或其共享行动者状态相同。传入完成标志不是先前场景回执。

普通 C 激活将 main 置为待机并等待 main 与滚动，再使用视野目标和朝向寻址瓦片，具有跨柜台瓦片延伸与随从排除。被激活物理行动者必须实际反向映射为 153。事件行 `DOWN=3` 是包装器位字段：请求转向玩家，并在之后恢复所保存物理行动者互动前朝向，不是面向 Down 的前提。包装器禁用实体更新；场景脚本 trap 再启用它们。

Zone 则要求已产生的待处理 type 6 请求，坐标为 `(6,23)`。其移动产生方需要布局属性 `$1400`；仅该行不证明可访问楼层或自然穿越。待处理消费在分发前清除 type。Zone 包装器为 physical 0 安装 `eas_Init` 并匹配 X/Y，没有实体包装器的朝向协议。被抑制分支仍经过其窗口/VInt/main 停止服务。

## 在场者取决于传入地图状态

**已确认替代声明：**普通 NPC 编号独立于己方角色行递增。最后两个普通行提供查找 152，BIRDWOMAN 位于 `(6,10)`、Right，以及查找 153，CHICK 位于 `(16,6)`、Left，二者使用 `eas_Init`。这些是查找身份，不是物理槽位 152/153。Bowie/main 单独为 physical 0。文本 1688/1689 使用 `{NAME;20}`，其己方常量是 Skreech；该归属不将 NPC 153 映射为己方角色 20，也不将其加入军团。

Peter 查找 7 在此没有固定行。新重建时，F67 在固定实体之前将其提供为随从。F67 未设使已清除映射为零，未防护请求可将它解析为 physical 0。F75 同样可在 Luke 固定行之前提供他，改变分配，但普通 NPC 编号不变。其他传入随从标志也可改变物理槽位。

随从声明不测试军团/出战队伍成员资格、字 HP 或 AI。两个雏鸟场景都不加入任何人、不改变成员资格或 AI、不治疗/复活或设置 F67。存活或不死己方角色入场治疗不确立之后 HP、映射或随从状态。[出战队伍/名册合同](../contracts/party-roster-state.md)将这些操作区分。

查找解析将 selector 掩码为字节，NPC 128+ 减 `$60`，再读取物理索引并乘以 32。零映射到 physical 0；`$FF` 无通用存在守卫就解析为索引 255。场景不创建或修复映射。下文行动者后果要求命名映射有效互异，且相关服务完成。

台词属性来自当前映射精灵。正常 CHICK 选择立绘 NONE/bleep 1；BIRDWOMAN 选择 NONE/bleep 6。改变传入精灵可改变这些输入。有归属对话与服务选择有支持；渲染外观与声音仍为**未知**。

## 演示：两次尝试与重复说法

**已确认有序内容：**游标 1680 提供六条文本，全部由查找 153 发出。前两条使用连续 `nextText`；后四条使用 `nextSingleText`。研究保留完整 48 条记录程序与内联体，因此本说明按体验分组，不重复命令台账。

雏鸟描述飞行梦想（1680），随后宣布演示（1681）。摄像机跟踪选择 153。面向 Right、sleep 40、点头和带等待向右跳跃后，精灵变为 FLYING_CHICK。禁用自动朝向；Up 1 请求使用速度对 `(4,4)`，然后 Down 6 使用 `(48,48)` 并有坠落声。精灵变回 CHICK，雏鸟表达困惑（1682）。随后带等待 Init 及 Left 2/faceUp 1 序列；请求 Bowie 和 Peter 面向 Down。雏鸟承认失败（1683），请他们留下看第二次尝试（1684）。

第二次再次面向 Right、sleep 40 并点头。摄像机速度变为 `$40`；Right 1 和向右跳跃后是 FLYING_CHICK、自动朝向 OFF 和 Down 1。速度 4 伴随 Up 1，速度 64 伴随 Down 8 和坠落声。安装不等待逆时针行动；随后 `startEntity 153` 和 sleep 12。摄像机速度恢复 `$20`，带等待 InitSlow 替换该行动，再带等待 Up 2/Right 1/Up 11 序列。请求雏鸟 Left 和 CHICK、Bowie/Peter Right，雏鸟声称取得有限成功（1685）。之后 F723 已设互动仅请求同一说法。六条不同飞行文本加重复共七个请求位置。

**已确认状态限制：**逆时针行动循环经过相对目的地 `(-1,+1)`、`(-1,-1)`、`(+1,-1)` 和 `(+1,+1)`，每个都等待到达。之后 InitSlow 中断其指针，而非等待循环完成，也不重置当前/目的地坐标。飞行没有 `setPos`，因此不能将名义移动加至新声明 `(16,6)` 就确立无条件最终坐标。

带等待行动完成后，飞行留下待机行动、CHICK 和 InitSlow 速度 `(16,16)`；结束时从不恢复 Init 的 `(32,32)` 速度。脚本 Left 请求随后被互动包装器保存的物理雏鸟朝向恢复所替换。新建 Left 恰好一致；传入互动前朝向不同时则恢复该值。查找 152 与优先级条目不变。摄像机跟踪保持 153，直到之后条件探索交接。

## 独立的坠落与救援

**已确认有序内容：**游标 1686 提供四条单文本请求：153 两条、152 两条。完整 78 条记录程序由研究保留。它不改变地图精灵、健康、成员资格或 AI，不重建实体，也不含内部完成标志写入。

### 弹跳与惊呼

场景将 153 放在 `(6,10)`、Down，使 152 面向 Up，并选择摄像机目的地 `(1,5)`。它等待 Peter 的 Init，请求 Bowie `(5,22)`、Down 和 Peter `(11,22)`、Up，再安装 Peter 不等待的 wait 60/Left 4 行动，使 Bowie 面向 Up。此处仅改变查找 152 朝向：她的新声明 `(6,10)` 不强制应用于传入当前行动者。

雏鸟 `eas_46198` 启用加速/减速因子，不设置速度。因此新 Init 状态可从速度 32 开始；已完成飞行则提供速度 16/InitSlow。前两次 Up 2/faceUp 5/Down 2 序列，各有跳跃声，以笑声 1686 和 1687 及 sleep 40 结束。第三次使用 Down 1，随后 Init 和带等待向左跳跃。该 Init 将速度规范至 32。雏鸟面向 Up，birdwoman 面向 Left；摄像机以速度 `$30` 跟踪 153。Birdwoman 使用 `{NAME;20}` 喊雏鸟姓名（1688）。这是有归属的惊呼，不是招募事件。

### 坠落、追赶与安全报告

场景写入 Bowie 优先级 0、153 优先级 1，禁用 153 自动朝向、设置速度 48，并在安装不等待 Down 13 之前请求坠落声。Peter 面向 Left，再次写入优先级，随后 sleep 35。查找 152 收到速度 64 和不等待 Left 1/Down 11 追赶。显式 `waitIdle 153` 仅等待雏鸟，不等待她到达或接触。之后震动 3、Bowie Up、带等待自定义动画/定向行动、sleep 40 和震动 0。

雏鸟接着收到 Init、Down 和四次带等待跳跃，每次之后 sleep 20，包括第四次。雏鸟面向 Up，摄像机跟踪变中性。152 两次带等待跳跃后，向领队道谢，并说具名雏鸟没事（1689）。她点头，随后 sleep 30，再为 152 和 153 都安排带等待 Init。**推断：**此编排描绘救援。**未知：**实际飞行/坠落/接触、可见性和时序。她报告安全不是 HP 写入。

Peter 在 Left 朝向请求前没有初始待机屏障；其 wait 60/Left 4 可能仍待处理。之后 Left 1 行动替换已安装指针，不在其后排队。第一次带等待 152 跳跃同样替换她的追赶，没有先等待追赶。Sleep 和其他行动者等待不证明这些不等待移动已到达。

### 离开与之后局部状态

查找 152 收到不等待 Right 8，同时 153 被等待经过 Up 1/Right 8。场景隐藏 152，再显式将 153 放到 `(16,6)`、Left。她的离开在隐藏前没有专用完成屏障；等待雏鸟退出不提供这种屏障。Peter 收到带等待 Left 1、sleep 30 和带等待 BumpLeft。Bowie 收到 Init、Down 和摇头，再设优先级 1。最后命令尝试 selector 2 的 Peter 跟随 Bowie。脚本返回先于调用方 F724 写入。

映射有效且服务完成时，153 最终当前/目的地位置为 `(16,6)`、Left，行动待机/Init 速度 32。其地图精灵为传入值，新声明或完成飞行后通常为 CHICK，优先级条目保持 1。救援没有精灵规范化命令。仅 Bowie 的 HP 守卫位置请求运行且无其他移动介入时，他才保留 `(5,22)`；最终 Down 朝向请求也有 HP 条件。Peter 最终跟随请求不固定当前坐标，也不等待跟随稳定。

## 正确理解等待、动画与跟随

**已确认使用方区别：**[研究使用方章节](../../../research/bedoe-hatchling-flight-and-rescue.md#action-animation-priority-hp-and-hide-consumers)和[通用脚本所有者](../../../research/common-scripting.md)保留下列限制：

- 安装行动立即替换指针。带等待形式轮询该行动者指针是否为 `eas_Idle`；它们不是队列、全局屏障或无阻挡移动保证。基本方向差值按每瓦片 384 内部单位相乘，使用当前坐标。速度 4/16/32/48/64 是成对 X/Y 操作数，不是瓦片计数或时长；`faceUp 5` 是朝向/精灵/等待工作，不是 Up 5。Jump/Bump 行动发出定时相对目的地，单凭名称不保证位移。
- Init 设置速度 32、加速因子零、加速/减速 OFF、阻挡 ON、实体/地图不可碰撞 OFF、自动朝向 ON、定向 Up、浸没 OFF、大小 24，再待机。InitSlow 设置速度 16、因子 `(1,1)` 与源码 `(-2,ON)` 加速/减速对，之后阻挡/自动朝向/定向/大小配置相同。两者都不重置坐标。
- `startEntity 153` 写入动画计数器 0，不启动/恢复行动。FLYING_CHICK 和 CHICK 是地图精灵 selector，不是交战移动类型变化。Bowie 的 `motion OFF` 写入计数器 `$FF`，不是冻结移动；orientDown 改变定向，独立于其 Up 朝向。最终 Init 恢复定向 0，但没有动画开启命令；完成摇头结束于计数器 0。
- 优先级写入在物理行动者优先级数组中将零映射为 0、任意非零字映射为 1。普通精灵链接使用方将非零条目放在零条目前；这不同于精灵瓦片的 `VDPTILE_PRIORITY`。救援在该方案中使 153 先于 Bowie，之后将 Bowie 设为 1；它不保存/恢复旧条目，并使 153 保持 1。实际重叠与 VDP 可见性仍未知。
- `setPos`/`setFacing` 测试按字节选择的己方角色**字 HP**：零则跳过操作数字节。NPC 152/153 绕过该检查。行动、精灵、优先级、手势和隐藏不继承它；因此 Bowie/Peter HP 为零不抑制整场，也不意味着治疗。
- `followEntity Peter,Bowie,2` 编码 `$0007,$0000,$0002`，但准入辅助程序检查第一字**高字节**，因此是 Bowie 0 的 HP，不是 Peter 7 的。非零时解析两个映射，以 selector 2 偏移 `(-24,0)` 像素安装/追加跟随。即使随从已在列表，也替换其指针。跟随使用方将偏移左移 4 位为内部坐标，再应用自身距离/方向/布局规则。这既不写 F67，也不写成员资格，不证明即时位置、跟随稳定或未来持久性。

摄像机目标选择不等待滚动；摄像机目的地选择使用中性跟踪并等待视野滚动完成。地图脚本 sleep 使用低字节服务计数，可被调试文本跳过绕过；Peter 的 wait 60 是实体行动等待，属于独立使用方。没有一种是墙钟测量。文本/立绘/关闭、声音、精灵/DMA、VInt 和输入服务保留完成限制。终止符仅在对话窗口仍在时等待滚动，随后清除滚动速度，既不写入调用方标志，也不设置控制。[对话合同](../contracts/dialogue-system.md)区分游标/显示/关闭服务与观察到的自然文本及输入响应。

## 隐藏与重入不产生相同状态

**已确认生命周期边界：**`hide 152` 将当前/目的地坐标写为内部 `$7000,$7000`，使每个匹配映射失效为 `$FF`，并移除匹配随从列表条目。它保留行动指针，因此隐藏不证明不等待离开到达目标。之后替代 init 独立读取 F799 将 Luke 10 移出地图、F724 将 152 移出地图，不读取 F723 或 F13。其 `MoveEntityOutOfMap` 仅改变坐标，保留映射、跟随和行动指针；此处 F799 仅控制 Luke。

| 传入顺序或入场边界 | 后果与限制 |
| --- | --- |
| 先救援后飞行，F723 未设 | 仅 F13/F724 决定救援准入；F799 无关。完成救援设置 F724。之后有效 153 互动仍可从当前位置/精灵/优先级 1 运行飞行。飞行不重置该优先级。 |
| 先飞行后救援 | F723 不阻止救援。完成飞行提供 CHICK/InitSlow 速度 16；救援起初保留该速度，之后 Init 设为 32。不同状态不证明观察到不同时长。 |
| 替代 init 前传入 F724 | Init 通过坐标移除 152，不使有效映射失效。仅 F13 已设抑制不移除她。 |
| Init 后 152 仍在时 F724 改变 | 被抑制 zone 返回不重跑移除或修复行动者。部分场景反而可在 F724 未设时隐藏 152。 |
| 当前地图入场（`MAP_CURRENT=255`） | Main/木筏/随从放置与地图/init 服务运行，不重建固定行动者/映射，不将 153 重置为表内速度/精灵/优先级/位置。Init 不能修复已为 `$FF` 的 152 映射；它按传入值解析该无效索引，因而不保证有效清理。 |
| 新显式 Map10 入场 | 实体群/映射在选定 init 前重建。替代 153 从 CHICK `(16,6)`、Left/Init、普通优先级 0 开始；152 从 BIRDWOMAN `(6,10)`、Right/Init 开始，随后 F724 可通过坐标移除她，同时保留新的有效映射。F67 独立提供 Peter。 |

新清除重置优先级数组；随从初始化将 main 优先级设为 1，并设置各随从声明优先级，普通 152/153 保留 0。这不同于救援保留的 153 优先级 1。Init 时 F723 不重放/重新放置 153。默认 F722 未设 init 是独立卫兵状态所有者，不能导入此替代实体群。

## 从各调用方返回条件控制

**已确认独立包装器：**飞行脚本返回后，实体调用方写入 F723。其包装器恢复所保存物理雏鸟互动前朝向、关闭立绘/文本并启用实体更新，重复互动也如此。它不增加调用方后 main 移动等待。救援脚本返回后，zone 调用方写入 F724；其包装器关闭窗口、等待一个 VInt，并等待 physical 0 当前/目的地相等，包括场景被抑制时。

任一普通路线返回探索后，`WaitForEvent` 仅在待处理地图事件 type 为零时将摄像机重新指向 0，并安装 physical 0 的受控行动。`PLAYER_TYPE=0` 选择 `eas_ControlledCharacter`；其他类型选择各自行动。调用方/窗口/VInt/移动服务完成及待处理为零是该条件交接所需。完成标志或脚本终止符都不证明玩家输入响应。止于这两个边界。

## 证据审阅与剩余限制

本指南消费 [PR #798 已接受研究](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/798)及 [PR #797 前序英文指南](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/797)。原版溯源仍为固定 SF2DISASM `master` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6` 与[研究协议](../../../research/bedoe-hatchling-flight-and-rescue.md#agreement-provenance-and-resources)中的 USA 零售版身份。审阅使用限定 tracked 选择，没有复制/生成可选派生载荷、重复源码调查或采集运行时。选定调用方与 init 和已接受研究一致。处理器测试夹具确立源码结构，不是组合雏鸟观察。

| 审阅面 | 追踪与结论 |
| --- | --- |
| 十条不同文本、两个完整场景及内联体 | [飞行](../../../research/bedoe-hatchling-flight-and-rescue.md#complete-flight-demonstration)、[救援](../../../research/bedoe-hatchling-flight-and-rescue.md#complete-fall-and-rescue-scene)和[复现选择](../../../research/bedoe-hatchling-flight-and-rescue.md#reproduction-and-review-boundary)保留 48/78 条记录与键 `0690..0699`。本指南概述全部 1680–1689 及重复 1685，不提升有归属的安全说法或渲染运动。完整命令台账比较仍为保留研究证据，不是此处新执行。 |
| 独立调用方谓词 | 测试夹具 ID `sf2-map-events-static-v1`：[实体分片](../../../../tests/fixtures/h2/map-events/entity-programs.json) program 30，位于 `0x56F68`，及 [zone 分片](../../../../tests/fixtures/h2/map-events/zone-programs.json) program 5，位于 `0x56FC8`。各自全部七个操作、分支目标/极性和各一个表引用均已审阅。它们确认返回后 F723/F724 写入、重复 1685 及救援不测试 F723/F799。 |
| 替代 init | 测试夹具 ID `sf2-map-init-static-v1`、[测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)、`expected.primarySourceBodies[15]`，位于 `0x5708E`。全部九个操作保留独立 F799/Luke 和 F724/152 移除，不读取 F13/F723。仅坐标效果由研究使用方支持，不由调用名称支持。 |
| 单位、行动/放置、动画、优先级与跟随 | 测试夹具 ID `sf2-map-script-engine-static-v1`、[测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、选定 `entityPlacementCommandFacts`、`entityActionBridgeCommandFacts`、`entityLifecyclePresentationCommandFacts` 和 `entityGestureRelationshipMotionCommandFacts`；[研究使用方](../../../research/bedoe-hatchling-flight-and-rescue.md#action-animation-priority-hp-and-hide-consumers)提供解释。字/高字节 HP、瓦片/速度/像素单位、动画与运动、链接与瓦片优先级保持区别。 |
| 激活、zone 产生与条件控制 | [调用方准入](../../../research/bedoe-hatchling-flight-and-rescue.md#callers-and-admission-boundaries)、[调用方返回/重入](../../../research/bedoe-hatchling-flight-and-rescue.md#caller-return-re-entry-and-independent-ordering)和[地图/探索合同](../contracts/map-exploration.md)。不推断朝向前提、自然穿越、全体行动者屏障、已修复无效映射或无条件控制。 |

[地图事件 schema](../../../../schemas/h2/map-events-target-program.schema.json)与[加载器](../../../../src/sf2tool/h2/map_events_fixture.py)保留有序操作、目标和分片组合。[Init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)与[验证器](../../../../src/sf2tool/h2/map_init.py)保留体/溯源相等性。[地图脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)与 [schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)保持不同命令族及使用方。聚焦所有者为 `tests/python/test_map_events.py`、`test_map_init.py` 和 `test_map_script_engine.py`；窄复现路线为 `uv run sf2 h2 map-events`、`map-init` 和 `map-script-engine`。这些是证据导航，不是新执行门禁声明。

**未知：**自然互动/zone 访问与楼层/碰撞；实际标志、映射、行动者、位置、HP、出战队伍/AI/随从/玩家状态；中断/不等待移动、服务/输入/VInt 时序及完成；渲染后的飞行/坠落/接触、定向/重叠/隐藏、对话/摄像机/音频；待处理请求、输入响应、之后持久性和可玩连续性。两个场景都不确立未来 Skreech 加入。这些缺口不准入自动运行时工作，该可选指南既不完成战役，也不完成设计 epic。
