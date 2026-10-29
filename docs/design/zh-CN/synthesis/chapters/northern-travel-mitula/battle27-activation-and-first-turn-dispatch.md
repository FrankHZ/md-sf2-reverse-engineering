# Battle27：激活与首次单次行动交接

> 本文是[规范英文原文：Battle27: activation and the first turn handoff](../../../../synthesis/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

军团可在全部声明区域外，而每个已放置敌人仍符合单次行动资格。**已确认：** 新 Battle27 初始化与开场完成后，十二编队位置均不触发四区域中的任何一个。十六初始化敌人却全部符合单次行动记录。区域激活、生成与单次行动准入判断不同问题。

本指南在开场包装器返回后的 `BattleLoop:@Start`，接续[初始编队与限制对峙](battle27-initialization-and-opening.md)。止于首次 `ExecuteIndividualTurn` 调用，**早于函数体**。结论要求新状态不变且全部中间服务返回。**推断：** 编队在进入声明区域前布置接触。**未知：** 自然抵达、实际队伍/状态/RNG、首行动者、已完成表现、可用控制与持久性。观察或重制版前沿均不推进。

## 从开场返回到单次行动调用

**已确认：** 已接受[有序前缀](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#ordered-prefix-and-supplied-continuation)执行以下步骤：

1. 激活敌人，包括槽160 的额外顺落辅助调用。
2. 运行当前战斗区域过场包装器。
3. 调试字节非零时，为持有90–105 标志请求诊断：text463，区域索引放 DIALOGUE_NUMBER，再关文本。零跳这些请求。未观察显示结果。
4. 构建生成敌人列表。长度零跳生成；否则按存储字节 ID 顺序逐个调 SpawnEnemyWithCamera。
5. 生成行动顺序缓冲区，清当前单次行动字节。
6. `@IndividualTurns_Loop` 将该字节读为**偏移**，再从 BATTLE_TURN_ORDER 读战斗员字节。`$FF` 返回 `@Start`；其他值到结束本指南的调用。

该字节读取与调用间无 HP、分数、成员资格、激活或结果测试。读偏移/ID 前清 d0.w，因此 ID 在字内零扩展；不清 d0 高位。a0 指缓冲区。抵达调用不建立已执行单次行动或控制所有者。

前序 F477 写入在场景**之前**。持有标志或中断场景不是开场完成证据。仅相关包装器返回后才开始此解释；当前选择器与缺失物理映射限制仍由前序拥有。

## 区域外的位置与已记住的检查

**已确认：** 激活扫描敌槽128–159，接受有符号字节 X 非负、当前 HP 字非零且 ID 标为敌人。不测 Y、成员资格、中立状态或移动命令。全部新行128–143 通过；缺席144–159 因 X/HP 失败。高位已设 HP 字仍通过非零测试。

当前 Battle27 下，触发辅助扫描声明区域0–3。变化选择器若声明零区，在激活写入前返回。先测 NEWLY_TRIGGERED_BATTLE_REGIONS 的各位，即**已检查字**。已持有位跳几何；否则求区域，仅命中时设持久 F90+index，无论命中或未命中都设已检查位。前缀与辅助均不清该字或持久标志。新入口清 F90–105，**不清**已检查字。更晚 AI 侧清除在指南外，不提供其传入值前提。

[完整几何、十二位置结果与拒绝算术](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#integer-region-join)是四个 type4 区域的权威。先用三角形1/2/4，必要时3/2/4。实际边消费者用有符号坐标字节、较大绝对增量（相等时水平）、向零截断的有符号乘/除与**从端点 B**的投影。边顺序、立即接受 result-1、对顶点比较与 bit0 例外均有关；通用浮点多边形模型不足。Region1 拒绝包含负 X 投影与实际 CCR/SynthesizeFlag 结果2，相对 vertex1 的0。源码注释不能替代。

局部 X14–17/Y44–46 的每个编队序号均在**每个**区域外。完整所有者列出每序号全部四结果，不由包围矩形推断拒绝。几何考虑全部30 己方槽，要求**有符号正** HP 与有符号 X **及 Y** 非负。不查加入/出战成员资格；变化为存活已放置非成员也可触发。高位 HP、HP0 或负坐标失败。不考虑敌人/中立槽。不同于激活/单次行动准入的非零 HP 与无 Y 测试。区域索引守卫不是通用严格 index<count 验证；普通迭代提供0–3。Trigger15 是哨兵，不是第五多边形。

更少/死亡成员只留下初始化位置子集，其他己方 X=-1。不变新前提下 F90–93 因此保持未设，与传入已检查位无关；F94–105 也未设。首个通过敌人后，已检查字等于**传入值 OR `$000F`**，不一定恰15。位记录检查，不记录成功触发。之后改位置不会重扫已持有位的区域。

## 激活能对十六敌人改变什么

**已确认：** [完整十六行触发/激活表](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#all-sixteen-row-effects-and-the-extra-call)将每行连接存储主/次半字节与结果字。全部新字不变：Bishop 行2/12/13 保持 `$3000`，其他 `$2000`，主/次 active 位未设。无区域标志设置。

表中变化标志后果可按下列分组阅读，覆盖全部十六行（战斗员 ID = 128 + 行）：

| 相关标志 | 使用该主触发的行 | 持有该标志时字变化 |
| --- | --- | --- |
|F93 / region3|0,1,2|0/1：`$2000 -> $2001`；Bishop2：`$3000 -> $3001`|
|F90 / region0|4,10,11,12|4/10/11：`$2000 -> $2001`；Bishop12：`$3000 -> $3001`|
|F92 / region2|6,7,8,15|`$2000 -> $2001`|
|F91 / region1|13,14|Bishop13：`$3000 -> $3001`；行14：`$2000 -> $2001`|
|无触发（15/15）|3,5,9|无激活写入|

持有非15 主触发胜出，设 bit0。只有主触发缺失/未设，才允许持有非15 次触发设 bits0/1。未命中保留字。主选择保留其他位，包括之前持有 bit1；不重置所供次激活状态。

行0/1/2、7、10/11 与15 主/次触发相等，持有标志时选主。其余全部次触发15。因此**未变行均无仅次触发激活路径**。Bishop12/13 存主 FOLLOW_ENEMY10/14 与次 FOLLOW_ENEMY0，但它们是移动命令；次**触发仍是15**，不是 region0。变化触发字节可准入次路径。名称/位均不证明战术、目标、移动、行动或玩家/AI 所有权。

### 额外槽是有条件特卖别名

32 槽循环后 d0.w=160、d7.w=`$FFFF`。ActivateEnemies 与辅助间无返回、分支或数据，因此再运行一次160。已接受[地址与压缩特卖连接](../../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)表明地址辅助拒绝**大于**160 的 ID，准入160 本身：`(160-96)*56 + $FFE800 = $FFF600`，即 DEALS_ITEMS 旁 CURRENT_GOLD。

`$FFF62E` 伪 X 是数量84/85；`$FFF60E` 伪 HP 是戒指20–23 数量；`$FFF636` 触发是数量100/101；`$FFF634` 激活是数量96–99。伪 X 非负、伪 HP 非零且持有非15 触发标志90–104 时，可将 bit0，或次选择的 bits0/1，OR 入 Goddess Staff99 计数半字节，保留其他位。已有位不一定改计数。这不是无条件添加物品、饱和递增或已证明商店访问/持久性。

新 Battle27 标志未设，即便伪 X/HP 门槛通过也阻止此写入。变化己方/标志/特卖可改结论；先前逐敌写入对额外调用可见。本指南消费已接受别名，不扩展为商店审计。

## 无 Battle27 区域场景或新生成请求

**已确认：** [完整区域钩子表与实际包装器](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#region-hook-and-spawn-admission)含四记录：battle32 区域0/1 共享 played flag386；battle40 区域0 用387、区域1 用388。CURRENT_BATTLE 始终27 时，包装器跳全部记录，在检查 played/区域标志、设 played 标志或分发 MAPSCRIPT 前返回。结论用完整表，不靠缺失猜测 Battle27 文件。变化当前战斗可匹配记录；此时 played 标志在脚本**前**写。中断可使场景未完成而标志已设。包装器后接调试诊断。

生成准入扫描全部32 敌槽，以 `$0F00` 掩 activation。十六新行全 STARTING0；缺席槽模式也0。区域激活只改 bits0/1，不能将 mode0 变生成模式。零跳 HP/区域/位置/重置/物品调用。TARGETS_LIST_LENGTH 变零；陈旧列表字节不消费。跳 SpawnEnemyWithCamera，因此新准备不建立生成动画、摄像机、物品或文本效果。

变化模式使用已接受[生成/重置消费者](../../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#spawn-list-and-changed-state-conditions)，采用 Battle27 实际行数16：

| 掩码模式 | 行重置前准入 |
| --- | --- |
|`$0200` hidden|当前 HP0 且持有非15 触发；随后最大 HP 必须0。|
|`$0100` respawn|当前 HP0；无区域要求。|
|`$0300` both|当前 HP0 且持有非15 触发；无额外最大 HP 测试。|
|其他非零模式|无匹配追加分支。|

死亡/触发辅助可在最大 HP 或重置拒绝**之前**设激活位。重置用界 `16 OR $80 = 144`，拒绝144–159，即便它们参与扫描。真实行起始 X/Y 与全部30 己方及32 敌人比较，无 HP、中立或自身排除。死亡行动者在自身起点可阻挡自己。无效行或占用起点写 max/current HP0、X=-1，返回 carry，不追加 ID。

成功初始化真实行属性、位置与物品，再恢复保存的**完整**激活字，经 setter 路径返回 clear carry。Battle27 无升级；全部行物品 NOTHING，保留复制定义装备，不产生行特定 AddItem 奖励。每成功项按升序扫描顺序追加一字节 ID 到字长度列表。后续比较用返回完整 d1，不重掩；源码顺落不重复 hidden/respawn 追加。

非空列表只在构建后按存储顺序消费。每 SpawnEnemyWithCamera 保存外遍历寄存器，请求实体/摄像机/等待、十二次 orientation/Sleep3 迭代、音效、text397 与关闭。服务必须返回才生成行动顺序。顺序不证明已观察出现、时序、表现完成或 AI 行动。

## 十六敌人符合资格，首行动者仍未知

**已确认：** [完整当前属性表与构造器](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#turn-eligibility-records-and-call-boundary)将十六敌人连接至定义、装备与清初始状态后的当前 HP/AGI。Lizardman AGI38、Wyvern36、Bishop27、Soldier34、Sniper35、Wizard44。实际 HP 为正，各提供一条记录。难度影响 ATT，不影响此 AGI 连接；F527 不升级27。区域 active 位未设**不**排除这些单次行动。仅名称或 AGI 不能识别首行动者。

构造器先将 `$FFF71A` 的64 条两字节记录清 `$FFFF`。先考虑己方0–29，再敌128–157；**排除158/159**，即使变化为存活已放置行动者。准入测有符号 X 非负与 HP 非零，无 Y、成员、中立、region-active 或指令集测试。新144–157 因 X 失败。所供存活已放置非成员与高位 HP 字可通过。

已接受[分数、连续 RNG 与排序推导](../../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch)适用于这些调用方。对无符号当前 rawAGI，令 `A=rawAGI&127`，R(n) 为逐次直接 RNG 调用：

- 首分数：`A + R(A>>3) - R(A>>3) + R(3) - 1`。
- rawAGI>=128 时，立即为同一行动者再追加，`B=floor(A*5/6)`，分数 `B + R(B>>3) - R(B>>3)`。
- 字算术后存各低字节。单记录消费3 次抽取、双记录5 次、拒绝0 次，按己方再敌槽顺序。

每次调用推进 `(seed*13+7)&65535`，包括 range0；这些范围返回 `floor(seed*n/65536)`。单次行动调用方用直接辅助，不是调试方向覆盖。抽取依序耦合；此处不为 Battle27 选择、枚举或模拟种子。

十六新敌人在每种允许抽取下分数均正，使用**实际己方抽取后的48 次敌方抽取**。符合初始化己方 N 个、双记录己方 T 个时，已接受十二位置编队总记录 `16+N+T <= 40`。更少/死亡成员减少 N。变化成员/位置/属性未经审阅不能继承该界。

排序覆盖完整64 记录缓冲区：62 轮，每轮63 对相邻比较，有符号字节降序，仅右分数严格更大时交换。平分保留追加顺序，包括己方先敌方；双记录行动者第二条紧接首条追加。填充255 为有符号 -1，回绕135 为 -121。已接受 rawAGI128 分数0/255 与 rawAGI127 分数135 用例是测试夹具边界，不是 Battle27 观察。

无追加容量守卫。变化为大量/双记录候选可写超64，62 轮不证明任意64 条完整排序。新最大40 可容纳，其正敌分数在这些轮数内把非哨兵记录带到前端。128 字节缓冲区后 `$FFF79A` 的 CURRENT_BATTLE_TURN 字节清零。之前服务返回时，offset0 抵达首次调用。实际行动者仍依队伍、当前 AGI 与传入连续 RNG。

空准入集合保留首 `$FF` 并重复准备，不宣告胜利/败北。变化状态中，低于 -1 的分数可排在填充之后。

## 所供延续与中断准备

**已确认：** [F88 延续](battle27-initialization-and-opening.md#f88-延续使用所供战斗状态)恢复秒数、清88/AI 内存与视野目标，载战场，再直接分支 `@IndividualTurns_Loop`。后续去 `@Start` 分支不可达。起初绕过以上**全部**准备，读取所供单次行动/顺序字节，无对齐或范围验证。非 FF 字节即到调用，即使该 ID 不符合新准入；偏移可奇数或越缓冲区。哨兵则以实际选择器和所供标志/位置/HP/模式/AGI/RNG 进入 `@Start`。传入74/27 不修当前选择器。前序条件 d0=74 越列表视野查找与缺物理映射仍是限制；加载不是安全状态验证或逐字节保留承诺。

| 变化前提 | 仍适用边界 |
| --- | --- |
|F477 持有或开场中断|场景前写的标志不证明开场已返回。|
|F88/当前选择器/单次行动偏移不同|初始所供分发与哨兵重入不同；无自动新状态修复。|
|更少/死亡/变化/非成员己方|编队序号不是身份；几何与单次行动 HP/Y 谓词不同。|
|已检查位持有或之后坐标变化|记住的未命中跳几何；新持久标志重置不清检查。|
|区域标志/激活字/触发字节持有或变化|主优先、哨兵15 与保留 bit1 仍适用；此处无逐回合重置。|
|Bishop12/13 命令变化或重解释|移动命令 FOLLOW_ENEMY0 不把次触发15 变为 region0。|
|缺席/中立/高位 HP 槽变化|激活、几何、生成与单次行动边界/谓词不同；无通用中立排除。|
|特卖字段变化|槽160 的 X/HP/触发门槛限制压缩计数写入。|
|调试字节非零或当前战斗变化|诊断或匹配的先标志后脚本区域钩子可介入。|
|生成模式/占用变化|激活可早于拒绝；重置可在追加/服务前改 HP/X 或重建属性。|
|AGI/RNG/队伍变化|记录数、回绕有符号分数、缓冲区写入与首行动者需实际输入。|
|准备任意处中断|先前检查/标志/激活/特卖/列表/属性/种子/缓冲区写入不回滚。|

准备不是原子事务。**未知：** 自然抵达、实际队伍/职业/HP/物品/标志/特卖/已检查字/当前/保存状态/种子、首行动者、已完成服务、时序、呈现/音频、可用控制与持久性。不建立 ExecuteIndividualTurn 函数体、AI/行动选择、移动/伤害、完成单次行动、结果、Battle32 或后续剧情。可选压缩布局缺失仍是无解码路径限制；区域不证明碰撞路径，也不授权采集。

## 证据审阅与保留结果

完整已接受 [Battle27 激活研究](../../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md)是**已确认的静态证据所有者**，以 [#1037 / PR1039](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1039)在 `6334e76cf59e7f04ac871115d91a994f0c581e67` 接受。固定 SF2DISASM 修订为 `c834c652b6862bc5679fd7f69a38a7093206efc6`，绑定 USA retail ROM 身份。[已接受开场指南](battle27-initialization-and-opening.md#证据审阅与保留结果)与其 Layer A 所有者提供已完成新入口前提。地址、源码路径、完整表、整数算术与溯源仍由其拥有。

合成前审阅读完整已接受证据与变化状态行、前序、[生命周期合同](../../../contracts/battle/battle-control-lifecycle.md)、[过场路由合同](../../../contracts/battle/battle-cutscene-routing.md)、选定[放置/激活](../../../../../research/chapters/grans-opening/battle01-placement.md)与 [RNG/单次行动证据](../../../../../research/battle/runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-turn-order-score)。合同紧凑“存活/已放置”措辞与旧广泛未知队列不替代更深已接受 HP/X/Y/边界/重置谓词。先前 Battle01 运行时及其几何模型只证明具名边界，不是通用整数边模型或自然 Battle27 穿行。未发现阻塞冲突；未来矛盾返回研究，不在 Layer B 静默修补。

| 公共测试夹具 ID / 消费者 | 已审阅连接与限制 |
| --- | --- |
|`sf2-battle-spriteset-data-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_spriteset_data.py)|27 的390 字节范围；`rowContentTracked=false` 不能证明完整行或几何。|
|`sf2-battle-loop-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_loop.py)|名册/内存/初始化源码守卫；无完整激活、生成或单次行动模拟。|
|`sf2-battle-control-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-control-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_control.py)|实际新/续接/回合顺序、重置90–105 与哨兵255；不保证已检查字零或安全所供偏移。|
|`sf2-battle-cutscenes-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_cutscenes.py)|区域8 字节/终止符/顺序/先标志后脚本守卫，不同于已完成分发。|
|`sf2-battle-routing-data-static-v1`；[测试夹具](../../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[消费者](../../../../../../src/sf2tool/h2/battle_routing_data.py)|四区域记录与终止符、`routeTargetsParsed=false`；完整27 排除用已接受全表。|
|`sf2-battle01-region-activation-v1`；[测试夹具](../../../../../../tests/fixtures/h3/battle01-region-activation-v1.json)、[消费者](../../../../../../scripts/Test-H3Battle01RegionActivationFixture.ps1)|基线 checked7/标志未设与受控命中；仅 Battle01 声明点与边界。|
|`sf2-battle01-secondary-activation-v1`；[测试夹具](../../../../../../tests/fixtures/h3/battle01-secondary-activation-v1.json)、[消费者](../../../../../../scripts/Test-H3Battle01SecondaryActivationFixture.ps1)|受控触发 `$F2` 与 `$2060 -> $2063`；未变27 行无自然次路径。|
|`sf2-battle01-turn-order-v1`；[测试夹具](../../../../../../tests/fixtures/h3/battle01-turn-order-v1.json)、[消费者](../../../../../../scripts/Test-H3Battle01TurnOrderFixture.ps1)|种子 `$1234`、九记录与平分顺序；无固定 Battle27 首行动者。|
|`sf2-turn-order-boundaries-v1`；[测试夹具](../../../../../../tests/fixtures/h3/turn-order-boundaries-v1.json)、[消费者](../../../../../../scripts/Test-H3TurnOrderBoundariesFixture.ps1)|四受控变更、八记录、有符号135 与0/255 分数；无满容量安全声明。|
|`sf2-rng-generate-random-number-v1`；[测试夹具](../../../../../../tests/fixtures/h3/rng-v1.json)、[消费者](../../../../../../src/sf2tool/h3/rng.py)|七个 range128 用例、种子/返回/寄存器比较与直接 RNG 模型；无 Battle27 种子观察。|

所属 H2 命令为 `uv run sf2 h2 battle-spriteset-data`、`battle-loop`、`battle-control`、`battle-cutscenes` 与 `battle-routing-data`；运行时命令仍属所链消费者与 `uv run sf2 h3 rng`。原切片检查实际载荷、schema 与选定消费者分支，未启动这些观察。此设计环境中源码与保留18,285,979 字节配对不可用：仅已接受0.083s 溯源，无采集、复制、再生成或新私有比较。

[#1037 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1037#issuecomment-6183776676)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1039#issuecomment-6183789330)保留作者0.340s、初始0.329s、独立0.332s 与 CI38676678592，包括 Region1 负投影/CCR 文字修正。[#1038 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1038#issuecomment-6183815772)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1040#issuecomment-6183838094)保留最终作者0.389s/独立0.393s/CI38677018042 与不同早期提交头/检查/CI/诊断。所链 #1033 原/变基结果、schema-path/local-$ref 失败及更早前序未知项继续保留。不重置已完成套件或失败记录。

Standard 严谨级别与[当前协议](../../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)为 [#1042](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1042)区分这些状态权威。交接记录完整文字/diff/变化状态审阅、直接公共测试夹具/schema/消费者检查、五路径/导航/UTF-8/链接/私有检查、干净提交 planner 与精确提交头实际范围 CI。文档范围跳过笼统 normal/full/Python/.NET/Godot/H3/H4/翻译套件。原切片中结果研究 #1041 与 UI692 仍禁止触及且未消费。不增加运行时队列、引擎/平台变化、翻译或后续剧情范围。
