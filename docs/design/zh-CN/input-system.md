# 输入系统合同

- **已确认的原版行为：** 原始双端口采样序列、Player 1/2 状态字节存储、VInt 派生的当前/输入重复阶段，以及下面的输入等待辅助控制流。
- **未知的原版行为：** 控制器电气延迟、三键与六键协议行为、控制器型号兼容性，以及帧精确的玩家可见重复时序。
- 重制状态：实现无关的输入管线合同；硬件适配器与平台事件时序在分组运行时矩阵观察原版之前仍是实现选择。
- 证据日期：2026-07-20
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/tech-services-static-v1.json` 中的 `sf2-tech-services-static-v1`；`tests/fixtures/h2/tech-interrupts-static-v1.json` 中的 `sf2-tech-interrupts-static-v1`；`src/sf2tool/h2/services.py`；以及
  `src/sf2tool/h2/interrupts.py`。

> 本文件是 [`input-system.md`](../input-system.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的静态合同

`UpdatePlayerInputs` 先采样 `DATA1` 再采样 `DATA2`（两字节端口步进）。它写 TH 低（`0`）、读取、写 TH 高（`$40`）、把第一次读取左移两位并用 `$C0` 掩码、读取并用 `$3F` 掩码、组合各部件、反转字节并存储。它的本地采样器在连续的 Player 1 再 Player 2 状态存储中为每个端口产生两个状态字节。这是原始控制器采样；它本身不是“每个现代控制器都暴露相同电气协议”的陈述。

VInt 拥有的重复阶段是一个独立合同。它调用原始采样器，派生 `CURRENT_PLAYER_INPUT` 与 `LAST_PLAYER_INPUT`，通过初始 24 帧延迟抑制未变化的输入，然后减去六来创建静态重复节奏。源码确立计数器运算；它不确立外部观察到的输入到帧延迟。

`WaitForPlayerInput` 掩码 VInt 派生的当前输入，并且只在已识别的按钮非零时返回，否则等待另一个 VInt。`WaitForPlayer1NewInput` 首先等待已识别的 Player 1 输入被释放，然后等待一个新的已识别按下。有界的 Player 1 等待在每个 VInt 前轮询原始状态：一秒最多允许 60 次等待，三秒最多 180 次，识别到的按下会提前返回。`sub_15A4` 被单独建模为 scratch（暂存）掩码重叠与计数器控制流：重叠低于 10 清除 Player 1 输入，而零重叠或计数器至少为 10 清除其 scratch 状态。其调用方角色仍未证实。

## 运行时矩阵边界

一个未来的控制器/输入矩阵应覆盖从原始状态 A/B 到 `LAST`/`CURRENT`、新按下与释放/再按下、保持的 24 帧初始延迟与六帧节奏、一/三秒提前退出与超时，以及三键与六键/控制器延迟边界用例。这些共享相同的 VInt 与控制器配置；不需要单用例模拟器 fixtures。

## 重制边界

重制版可以分离原始设备采样、规范化按键状态、输入重复过滤与消费者等待。在需要保真的地方应保留已确认的阶段边界与辅助行为，同时把平台轮询、控制器能力协商、延迟政策与无障碍重复设置设为显式的现代决策。
