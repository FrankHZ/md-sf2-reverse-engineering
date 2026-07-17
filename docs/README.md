# Documentation Index

文档按“证据 → 合同 → 实现选择”分层，避免把原版事实、推断和重制偏好混在一起。

## Research

`research/` 保存可复现的逆向结论。每篇必须标明 Confirmed、Inferred 和 Unknown，并提供 ROM
hash、上游 commit、地址/符号或运行时观察以及复现命令。

- [`reproducible-original.md`](./research/reproducible-original.md)：ROM H0、固定工具链与
  bit-perfect H1 基线。
- [`static-core-data.md`](./research/static-core-data.md)：角色槽位、职业、物品、法术的 ROM
  ranges、ROM byte packing、双路径 parity 和待验证语义。
- [`ally-growth.md`](./research/ally-growth.md)：成长曲线、职业成长投影、法术学习与继承控制码。
- [`runtime-rng-and-battle-math.md`](./research/runtime-rng-and-battle-math.md)：RNG、成长计算与
  Battle 01 行动顺序、AGI 127/128 边界、区域激活，以及物理伤害从地形/弓手加成到
  dodge、critical、spread、double/counter、死亡/距离/状态/阵营/特殊敌人后续校验、HP/EXP 构造及持久回放的 H3 动态 fixture。
- [`enemy-promotions.md`](./research/enemy-promotions.md)：五段转职映射、103 个敌人名称和
  56-byte 敌人定义的 source/ROM 双路径合同，以及教堂与敌人初始化的静态消费者。
- [`battle01-placement.md`](./research/battle01-placement.md)：第一场剧情战斗的 map link、Stack
  压缩 terrain、背景/经验/胜负全局规则、9 个实体记录和三个 region polygon。

## Design

`design/` 将已确认行为整理成实现无关的游戏设计规格。Phase 2 开始按 subsystem 创建；不能用
设计文档反向“证明”逆向结论。

- [`combat-resolution.md`](./design/combat-resolution.md)：物理攻击从 dodge、地形/克制、critical、
  spread、double/counter 到临时 HP、reaction 回放和 EXP 的实现无关合同，以及未来 H4 的共享
  fixture 边界。

## Decisions

`decisions/` 记录引擎、模拟器、数据格式和工具链等耐久选择。只有出现真实分歧且选择会约束
后续实现时才创建 decision record。

- [`0001-bizhawk-for-h3-runtime-observation.md`](./decisions/0001-bizhawk-for-h3-runtime-observation.md)：
  固定 BizHawk 2.11.1，并记录 Genesis Plus GX 寄存器写入的实测边界。

## Evidence Vocabulary

- **Confirmed**：脚本/test 可复现，或由具体反汇编位置与运行时观察共同支持。
- **Inferred**：证据充分但尚未独立复现。
- **Unknown**：仍需实验的问题，不允许用便利假设填空。

根 [`README.md`](../README.md) 是范围与路线的 source of truth；根 [`AGENTS.md`](../AGENTS.md)
是工作约束；本目录只拥有研究与设计内容。
