# 随机性服务

- 证据日期：2026-08-02
- 源码基线：`ShiningForceCentral/SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`randomness.md`](../../contracts/randomness.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同

**已确认：** 主生成器把 16 位 `RANDOM_SEED` 推进为 `(seed * 13 + 7) & 0xFFFF`，保留调用方的 `d6` 范围寄存器，并在把该范围加倍后从乘积的高字缩放。观察到的基行为由 `tests/fixtures/h3/rng-v1.json`（`sf2-rng-generate-random-number-v1`）记录，而完整静态服务形状由 `tests/fixtures/h2/tech-services-static-v1.json`（`sf2-tech-services-static-v1`）记录。

**已确认：** 调试模式按 Right、Up、Left、Down 的优先级检查方向并返回 0、1、2 或 3，而不推进基础种子；禁用调试模式或无方向则回落到基础生成器。观察到的覆盖/回退与寄存器边界是 `tests/fixtures/h3/debug-rng-v1.json`（`sf2-rng-debug-override-v1`）。

**已确认：** 思考 AI 字节路径使用 `RANDOM_SEED_COPY`；其 H2 源码形状在该基础地址读取一个字节，在无符号乘以 541 再加 12345 之前符号扩展，把结果掩码为一个字节，并在同一基础地址写回一个字节。有界 `GenerateRandomNumberUnderD6` 服务对低字节范围 0、1 与 128–255 立即返回零；对 2–127 它重试直到无符号字节位于 0..range-1。上游注释说接受的较低界是 2，这与静态比较不符。范围二分支的现有行动选择观察是 `tests/fixtures/h3/battle-ai-action-choice-v1.json`（`sf2-battle-ai-action-choice-runtime-v1`）。`tests/fixtures/h3/random-services-v1.json`（`sf2-random-services-matrix-runtime-v1`）中的独立十用例运行时矩阵确认那些范围低字节提前退出、无符号范围二三步重试，以及思考精确种子 57 步重试。它还解决字节通道：基础地址字节是大端种子副本字的高字节。原始有界辅助函数返回 `d7=0`，同时保留其辅助函数返回的种子副本状态（早前行中的 `$53C2` 与 `$985D`）。只有每个辅助函数之后受控的按源码形状探测副本把该返回字节写入高字节，产生 `$00C2` 与 `$005D`；源码上下文文本与菱形行同样保留其低字节。两个辅助函数都不改变 `RANDOM_SEED`。自然的 Battle Test 路线对该探测仅做配置；它不确立战斗、UI、文本/菜单、时序或剧情行为。

**未知：** 调用方可见的时序、精确矩阵种子之外的重试分布，以及实际的文本/菜单/AI 调用方执行与共享种子副本生命周期。它们仍是单组 H3 矩阵，而不是新的一次一用例测试夹具。

## 实现边界

保持主种子与种子副本状态分离，把调试覆盖设为显式的测试控制，并把范围零行为与有界采样分开暴露。不要把上游注释的较低界 2 编码为返回值规则。
