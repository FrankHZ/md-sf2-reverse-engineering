# Documentation Index

文档按“证据 → 合同 → 实现选择”分层，避免把原版事实、推断和重制偏好混在一起。

## Research

`research/` 保存可复现的逆向结论。每篇必须标明 Confirmed、Inferred 和 Unknown，并提供 ROM
hash、上游 commit、地址/符号或运行时观察以及复现命令。

- [`reproducible-original.md`](./research/reproducible-original.md)：ROM H0、固定工具链与
  bit-perfect H1 基线。
- [`static-core-data.md`](./research/static-core-data.md)：角色槽位、职业、物品、法术的 ROM
  ranges、canonical H2 合同和待验证语义。

## Design

`design/` 将已确认行为整理成实现无关的游戏设计规格。Phase 2 开始按 subsystem 创建；不能用
设计文档反向“证明”逆向结论。

## Decisions

`decisions/` 记录引擎、模拟器、数据格式和工具链等耐久选择。只有出现真实分歧且选择会约束
后续实现时才创建 decision record。

## Evidence Vocabulary

- **Confirmed**：脚本/test 可复现，或由具体反汇编位置与运行时观察共同支持。
- **Inferred**：证据充分但尚未独立复现。
- **Unknown**：仍需实验的问题，不允许用便利假设填空。

根 [`README.md`](../README.md) 是范围与路线的 source of truth；根 [`AGENTS.md`](../AGENTS.md)
是工作约束；本目录只拥有研究与设计内容。
