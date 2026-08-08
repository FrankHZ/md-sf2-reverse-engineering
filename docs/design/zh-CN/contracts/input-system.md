# 输入系统合同

- **已确认的原版行为：** 下文受限的一次启动矩阵中的五个原始双端口采样用例，以及三个直接 VInt 输入重复用例。
- **未知的原版行为：** 输入等待辅助函数的运行时行为、`sub_15A4`、控制器电气延迟、三键与六键协商、控制器型号兼容性，以及面向用户的 UI 时序。
- 重制状态：实现无关的输入管线合同；硬件适配器与平台事件时序在分组运行时矩阵观察原版之前仍是实现选择。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/tech-services-static-v1.json` 中的 `sf2-tech-services-static-v1`；`tests/fixtures/h2/tech-interrupts-static-v1.json` 中的 `sf2-tech-interrupts-static-v1`；`tests/fixtures/h3/controller-input-v1.json` 中的 `sf2-controller-input-runtime-v1`；`src/sf2tool/h2/services.py`；`src/sf2tool/h2/interrupts.py`；以及 `src/sf2tool/h3/controller_input.py`。

> 本文件是 [`input-system.md`](../../contracts/input-system.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的静态合同

`UpdatePlayerInputs` 先采样 `DATA1` 再采样 `DATA2`（两字节端口步进）。它写 TH 低（`0`）、读取、写 TH 高（`$40`）、把第一次读取左移两位并用 `$C0` 掩码、读取并用 `$3F` 掩码、组合各部件、反转字节并存储。它的本地采样器在连续的 Player 1 再 Player 2 状态存储中为每个端口产生两个状态字节。这是原始控制器采样；它本身不是“每个现代控制器都暴露相同电气协议”的陈述。

VInt 拥有的重复阶段是一个独立合同。它调用原始采样器，派生 `CURRENT_PLAYER_INPUT` 与 `LAST_PLAYER_INPUT`，通过初始 24 帧延迟抑制未变化的输入，然后减去六来创建静态重复节奏。源码确立计数器运算；它不确立外部观察到的输入到帧延迟。

静态源码清单还分别描述 `WaitForPlayerInput`、`WaitForPlayer1NewInput`、一秒与三秒等待，以及 `sub_15A4`。它们的运行时行为有意不属于此 H3 合同；它们仍在队列中，不从其静态控制流推断。

## 运行时矩阵边界

`sf2-controller-input-runtime-v1` 恰为一次直接函数接缝启动。对原始 `UpdatePlayerInputs` 的五次调用观察中性、Player 1 Up+B、Player 2 C+Start、同时组合基本按键，以及释放。所有调用记录每个端口的两个原始字节。对原始 `ApplyZ80BusUpdates` 的三次直接调用观察新按下、释放/再按下，以及在源码派生的 24 帧阈值和六帧节奏下保持 C 输入。重复执行是直接 VInt 输入阶段观察；它不是正常的 `WaitForVInt` 调用方推进。

观察器还检查直接 call/target/return 三元组，以及原始嵌套源码 call/target/return 路径 `ApplyZ80BusUpdates` → `UpdatePlayerInputs`。它的 `CheckSram` 返回重定向只进入临时 work-RAM probe；其 gate 在每个主机帧前 arm 一个直接调用，并在返回后 pause。运行时 `WaitForPlayerInput`、`WaitForPlayer1NewInput`、一秒与三秒等待、`sub_15A4`、三键/六键协商、硬件延迟，以及 UI/菜单行为仍为成组的未知问题。

## 重制边界

重制版可以分离原始设备采样、规范化按键状态、输入重复过滤与消费者等待。在需要保真的地方应保留已确认的阶段边界与辅助行为，同时把平台轮询、控制器能力协商、延迟政策与无障碍重复设置设为显式的现代决策。
