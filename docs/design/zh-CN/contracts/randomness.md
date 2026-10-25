# 随机性服务

- 证据日期：2026-08-09
- 源码基线：`ShiningForceCentral/SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`randomness.md`](../../contracts/randomness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同

**已确认：** 主生成器把 16 位 `RANDOM_SEED` 推进为 `(seed * 13 + 7) & 0xFFFF`，保留调用方的 `d6` 范围寄存器，并在把该范围加倍后从乘积的高字缩放。观察到的基行为由 `tests/fixtures/h3/rng-v1.json`（`sf2-rng-generate-random-number-v1`）记录，而完整静态服务形状由 `tests/fixtures/h2/tech-services-static-v1.json`（`sf2-tech-services-static-v1`）记录。

**已确认：** 调试模式按 Right、Up、Left、Down 的优先级检查方向并返回 0、1、2 或 3，而不推进基础种子；禁用调试模式或无方向则回落到基础生成器。观察到的覆盖/回退与寄存器边界是 `tests/fixtures/h3/debug-rng-v1.json`（`sf2-rng-debug-override-v1`）。

**已确认：** 思考 AI 字节路径使用 `RANDOM_SEED_COPY`；其 H2 源码形状在该基础地址读取一个字节，在无符号乘以 541 再加 12345 之前符号扩展，把结果掩码为一个字节，并在同一基础地址写回一个字节。有界 `GenerateRandomNumberUnderD6` 服务对低字节范围 0、1 与 128--255 立即返回零；对 2--127 它重试直到无符号字节位于 0..range-1。上游注释说接受的较低界是 2，这与静态比较不符。范围二分支的现有行动选择观察是 `tests/fixtures/h3/battle-ai-action-choice-v1.json`（`sf2-battle-ai-action-choice-runtime-v1`）。`tests/fixtures/h3/random-services-v1.json`（`sf2-random-services-matrix-runtime-v1`）中的独立十用例运行时矩阵确认那些范围低字节提前退出、无符号范围二三步重试，以及思考精确种子 57 步重试。它还解决字节通道：基础地址字节是大端种子副本字的高字节。原始有界辅助函数返回 `d7=0`，同时保留其辅助函数返回的种子副本状态（早前行中的 `$53C2` 与 `$985D`）。只有每个辅助函数之后受控的按源码形状探测副本把该返回字节写入高字节，产生 `$00C2` 与 `$005D`；源码上下文文本与菱形行同样保留其低字节。两个辅助函数都不改变 `RANDOM_SEED`。同一已接受矩阵进入精确的 `symbol_wait1` 与菱形菜单前导，观察各自的源码 RNG 调用、副本、寄存器恢复和一次 `WaitForVInt` 返回，然后转入受控 continuation；它确认这些有界接缝，而非周围原始调用方循环。自然的 Battle Test 路线对该探测仅做配置；它不确立战斗、UI、文本/菜单、时序或剧情行为。

**未知：** 调用方可见的时序、精确矩阵种子之外的重试分布、两个已观察接缝之外的正常完整文本/菜单/AI 调用方流程，以及跨调用方族的种子副本生命周期或覆写行为。它们仍是单组 `random-services-unobserved-caller-context-and-seed-copy-lifetime` 队列，而不是新的一次一用例测试夹具。

## 实现边界

保持主种子与种子副本状态分离，把调试覆盖设为显式的测试控制，并把范围零行为与有界采样分开暴露。不要把上游注释的较低界 2 编码为返回值规则。

### 初始字节与实时调用方状态

**已确认（静态源码）：** `code/common/scripting/text/textfunctions_1.asm` 的 `ParseSpecialTextSymbol:@wait2/loc_6472` 与 `symbol_wait1` 在等待前，把主 `GenerateRandomNumber(range=256)` 的结果写入 `RANDOM_SEED_COPY` 基础地址处的字节。`code/common/menus/diamondmenu.asm:ExecuteDiamondMenu/@loc_16` 执行同样写入。这些主生成器抽样依赖 `RANDOM_SEED`，不依赖之前的种子副本字节。因此，一次已执行轮询消除了对旧高字节的依赖，同时保留低字节；后续思考抽样仍依赖新写入值。这是先覆写后读取，不是把 null 归一化成零的规则。

[已准入的 W2 调用方证据](dialogue-system.md#w2-组合语义验收)把 Map3 开场文本 510/511 与战前文本 2292/2299/2303 放在选定路线中战斗 AI 之前。原版思考服务的直接调用方是战斗 AI 的待机、行动选择、目标优先级与特殊攻击者路径。对普通未激活敌人，`ExecuteIndividualTurn:@Call_StartAiControl` 进入 `StartAiControl:@NonSwarmAi`，然后进入 `DetermineAiStandbyMovement`，后者首次抽样范围为 8。这些入口例程不重新播种副本。文本等待与该 AI 之间的玩家菱形菜单轮询可能再次替换文本写入值。因此，开场字节失效并不证明哪个后续写入方提供了首次 AI 抽样值，也不证明两次运行的完整 AI 历史相同。

对初始字节给予 H4 诊断豁免，必须在原版与实际消费者路径**两侧**证明首次有效读取前的覆写。辅助函数算术正确、只有文本锁存观察，或存在 AI 测试，都不能提供这种绑定。

**已确认（现代引擎行为）：** `SourceEnemyAiTests.FieldTextCopyReplacesInitialAiByteBeforeEntryAndAiKeepsItsUpdatedByte` 从声明的探索场景开始，执行战前 W1/W2、正常战斗入口、玩家 Stay 与自动 SourceOrders AI。在其余输入相同的情况下，不同初始高字节 `$03/$12` 均在文本复制提交时变成 `$4E`。AI 随后消费范围 8/2/2 的值 0/1/0，移动到 (4,6)，记录记忆 `$13`，并把高字节留在 `$00`；各变体其余 24 位均保留。这些是受控引擎输入/结果，不是原版自然路线首次 AI 的黄金基准。复制绑定之前，同样合法的 W2 流程把 `$03/$12` 保留到 AI，分别产生在 (5,5) Stay/记忆 `$00` 与移动到 (6,5)/记忆 `$34`。该反例排除了仅因文本与 RNG 辅助检查通过就接受旧独立锁存行为的做法。

[现代文本所有者](../../../../remake/docs/application/session-and-programs.md)把 `ThinkingSeed` 定义为实时状态映像，把可空剧情副本定义为最后一次文本写入诊断。即使独立战斗没有先前文本，也会导出实际当前字节；null 诊断证据不得隐藏显式配置/执行的现代值，也不得被强制解释为原版零值。修正前保留的完整路线捕获仍是历史消费者证据；其种子/AI 轨迹不描述已修正的可执行程序。对 H4 应用诊断豁免，仍须把选定实际证据绑定到修正后的路径。

**未知：** 选定原版路线首次 AI 抽样前最后一个有效副本生产者。现代 ActionChoice 已准入的声音边界并不实现原版菱形菜单轮询或其 RNG 机会。本次文本复制修正不合成菜单轮询/CPU 时钟。上文分组生命周期未知项对这些调用方仍开放；这项有界覆写证明不建立完整菜单/AI 时间线，也不允许拿抽样机会不同的整段历史行动序列作比较。
