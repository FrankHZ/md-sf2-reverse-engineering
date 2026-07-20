# Documentation Index

文档按“证据 → 合同 → 实现选择”分层，避免把原版事实、推断和重制偏好混在一起。

## Continuity Without External Memory

The repository is the durable project record. A fresh contributor or agent should be able to resume
without a previous chat transcript or external memory store:

1. read the root [`README.md`](../README.md) for scope, baseline, current phase, aggregate evidence,
   and the active frontier;
2. read [`research/source-coverage.md`](./research/source-coverage.md) for the exact coverage
   denominators, verification cadence, and current subsystem direction;
3. use this index to open the closest research, design, or decision owner;
4. inspect `git status` and recent commits before assuming the active slice is complete or the
   worktree is clean;
5. reproduce counters from tracked manifests and commands instead of copying a stale progress note.

Topic documents own detailed findings and unknowns, decision records own durable tool/architecture
choices, and Git history owns completed-slice chronology. External agent memory is neither required
nor authoritative, and it is disabled for routine work on this repository. Do not read or update a
personal/global memory store to resume the project. If the user explicitly requests a one-time
migration audit, move only still-valid, project-specific facts into the appropriate tracked owner and
then stop synchronizing against the external store. When a session discovers a durable fact or
changes the frontier, update the owning repository document in the same slice.

The one-time migration audit on 2026-07-19 found no `md-sf2` project entry in the disabled external
memory index. The durable chat-era decisions were already owned here: autonomous Phase 2 slice and
commit cadence in `AGENTS.md`, Python/uv and focused commit verification in ADR 0002, static-first
batched simulation in ADR 0003, and reproducible coverage counters/frontiers in
`research/source-coverage.md`. The remaining implementation-only Lua syntax-preflight rule was
migrated into ADR 0001 during this audit. No continuing memory synchronization is required.

## Research

`research/` 保存可复现的逆向结论。每篇必须标明 Confirmed、Inferred 和 Unknown，并提供 ROM
hash、上游 commit、地址/符号或运行时观察以及复现命令。

- [`reproducible-original.md`](./research/reproducible-original.md)：ROM H0、固定工具链与
  bit-perfect H1 基线。
- [`static-core-data.md`](./research/static-core-data.md)：角色槽位、职业、物品、法术的 ROM
  ranges、ROM byte packing、双路径 parity 和待验证语义。
- [`ally-growth.md`](./research/ally-growth.md)：成长曲线、职业成长投影、跨角色职业块扫描、法术学习与继承控制码，以及升级后的当前/派生属性刷新。
- [`runtime-rng-and-battle-math.md`](./research/runtime-rng-and-battle-math.md)：基础/调试覆盖 RNG、成长计算/完整升级、投影/等级上限/法术继承边界与
  Battle 01 行动顺序、AGI 127/128 边界、区域激活，以及物理伤害从地形/弓手加成到
  dodge、critical、spread、double/counter、死亡/距离/状态/阵营/特殊敌人后续校验及完整 double validator、HP/EXP 构造、击杀等级差、最终 EXP 减半/随机化/最低值、EXP 200 饱和/单阈值命令、gold 9,999,999/carry 饱和、敌人物品稀有/必掉/重复 flag、持久回放、99 EXP 自然升级、BLAZE 2 四档 FIRE 抗性矩阵、DAO/APOLLO/NEPTUN/ATLAS 四索引 target-count division、攻击法术 EXP、HEAL 1、SLEEP/SLOW 1 四档 STATUS 抗性、DESOUL 四档成败与多目标 kill reward、SPOIT 目标/施法者 MP 边界矩阵、BOOST 1 首次/重施、DISPEL/SILENCE 消费链，以及回合后状态过期/继续的 H3 动态 fixture。
- [`enemy-promotions.md`](./research/enemy-promotions.md)：五段转职映射、103 个敌人名称、
  56-byte 敌人定义、30 条敌人掉落与 103 条敌人 gold word 的 source/ROM 合同，以及 drop
  终止/flag/RNG 特例、gold 表后 69-word unused 尾部边界、教堂与敌人初始化的静态消费者。
- [`battle01-placement.md`](./research/battle01-placement.md)：第一场剧情战斗的 map link、Stack
  压缩 terrain、背景/经验/胜负全局规则、9 个实体记录、三个 region polygon 及 primary/secondary AI 激活语义。
- [`indexing.md`](./research/indexing.md)：机器可读的 symbol → ROM/RAM address → fixture → 文档/设计
  合同索引、校验规则和新增发现的落地流程。
- [`source-coverage.md`](./research/source-coverage.md)：当前覆盖分母、98.45% code-file reach、
  100% data-ASM H2 inventory、57.99% strict data-file reach 的不同边界，以及静态优先、集中运行
  H3 的子系统批处理节奏。
- [`battle-ai.md`](./research/battle-ai.md)：完整 battle AI 源码 inventory、action filter、攻击、
  治疗、支援、最终行动/目标选择、terrain/swarm/special-attacker 控制，以及单启动 14 case 的首组集中 H3 和后续问题矩阵。
- [`battlefield-pathfinding.md`](./research/battlefield-pathfinding.md)：战场/寻路 17 文件完整静态
  inventory、移动/范围/目标/move-string 合同，以及单次启动 5 case 的传播和边界 H3 矩阵。
- [`battle-loop.md`](./research/battle-loop.md)：战斗循环 18 文件与顶层控制 9 文件的完整静态
  inventory，以及主循环、胜负、roster、terrain、spawn、死亡清理和战后恢复合同。
- [`battle-actions.md`](./research/battle-actions.md)：战斗动作 29 文件完整静态 inventory、动作管线、
  物理分支顺序、物品损坏、Taros 特例和目标排序合同。
- [`battle-functions.md`](./research/battle-functions.md)：共享战斗函数 7 文件 inventory、单回合控制、
  Kiwi 火焰、EGRESS/Angel Wing、battle load 和移动音效合同。
- [`battle-scene-engine.md`](./research/battle-scene-engine.md)：battle scene 根引擎 12 文件和动画
  实现 55 文件、21 条 scene-script 命令、初始化/选择器、32×2 setup/update 配对，以及完整
  87 ally/121 enemy battle-sprite sequence 和 421 条 frame entry 合同。
- [`battle-cutscenes.md`](./research/battle-cutscenes.md)：战前/开战/战后/败敌/区域 cutscene 的
  10 文件路由、flag 准入、leader-death position 准备和 map-script 调度。
- [`common-scripting.md`](./research/common-scripting.md)：entity/map/text/credits 的 29 文件
  inventory、90/80 槽解释器、完整 255-entry/86-tree/1,536-leaf context-Huffman corpus、
  17-bank/4,267-string/152,679-symbol 静态解码、80-glyph variable-width font/256-entry ASCII
  map 数据流、完整 entity-action 静态链和单启动 13-case/20-tick entity movement H3。
- [`common-maps.md`](./research/common-maps.md)：共享 map engine 7 文件、map switch、battle
  trigger、egress/savepoint、8 KiB layout 解压边界和 VInt gates。
- [`common-stats.md`](./research/common-stats.md)：共享 stats 20 文件、flags/party/inventory、
  field-item dispatch、spell learning、新游戏顺序和未 include 的 alternate source 边界。
- [`common-menus.md`](./research/common-menus.md)：共享菜单 42 文件、提示框/文本控制、field-item
  dispatch、商店/教堂/车队等服务入口、diamond/yes-no 压缩图形、完整 icon storage/copy/highlight
  合同、27 个 leaf UI layout/2,394 个 VDP word、spell-level pointer 与 diamond-border 合同及
  alternate 边界。
- [`technical-interrupts.md`](./research/technical-interrupts.md)：VInt、DMA、fade、输入重复、wait/sleep
  handshake、trap 路由和待集中验证的硬件时序。
- [`technical-graphics.md`](./research/technical-graphics.md)：解压、显示初始化、sprite/palette、视差、
  battle terrain/background/sprite/weapon/ground/portrait/special-sprite 完整 Stack corpus、flash
  script 及渲染 parity 边界，以及 720 槽 regular map-sprite Basic-compression 与 9 条
  special-screen、8 条 base/menu UI Stack stream 与 7-icon uncompressed main-menu、56 条
  battle-effect、115 条 map-tileset
  Stack-compression corpus、208 张 battle-sprite animation table、163 槽 icon
  storage/menu-copy/highlight 合同、19 个 vanilla-built UI-layout owner 与 5,614-byte 静态 corpus、
  80-glyph variable-width font/ASCII map/loader 与完整 context-Huffman 合同、witch menu
  16-color palette/12-frame bubble
  table、12-resource/8,832-byte special-screen palette/layout corpus、4-stream/32,768-byte
  unused-cloud 与双 unused-base palette、16 套 map palette/79-map
  使用与有效 color-zero 合同，以及 46/46 direct named compression consumer 的 owner inventory。
- [`technical-interfaces.md`](./research/technical-interfaces.md)：331 个 jump stub 与 60 个 longword
  pointer 的完整静态路由表。
- [`technical-services.md`](./research/technical-services.md)：资源 incbin、byte copy、输入、SRAM、
  variable-width font、context-Huffman 与 witch-menu direct payload、68000 sound bridge、RNG 和
  独立 Z80 driver 构建链、没有 symbolic consumer 的 cloud/base payload 边界，以及 20/20
  technical incbin 到 8 个深层 H2 owner 的可执行归属审计。
- [`gameflow-core.md`](./research/gameflow-core.md)：冷启动、系统初始化、主循环、战斗/探索分流、
  map event、交互和物品 handoff。
- [`special-screens.md`](./research/special-screens.md)：logo/title、witch save、suspend/reset、ending
  等 19 文件特殊画面边界，以及全部 9 条压缩 tile stream、DMA transfer/tail、choice palette 与
  4×3 bubble-animation、7 套 palette/5 个 layout 合同。
- [`remaining-core.md`](./research/remaining-core.md)：ROM header/vector、window engine、battle test、
  configuration 与 debug action 的最后主代码边界。

数据侧目录 inventory 与 ROM parity：

- [`battle-global-data.md`](./research/battle-global-data.md)：全局战斗数据 18/18 H2 inventory 与
  17 个 H1-bound canonical tables。
- [`ally-data-inventory.md`](./research/ally-data-inventory.md)：ally/class 42 个直接或传递 include
  文件，以及对既有成长/法术学习 rails 的复用关系。
- [`core-stats-data-inventory.md`](./research/core-stats-data-inventory.md)：items/spells/enemies 的
  19 个 source 文件与表维度，以及 shops/debug shop/chest gold/break messages/mithril/Caravan/
  field items/weapon graphics 的 9-range 深层 source/H1/ROM 合同和 166-row enemy map-sprite
  normal-vs-tail reachability contract。
- [`battle-cutscene-data.md`](./research/battle-cutscene-data.md)：61 个 battle cutscene data 文件、
  59 个构建内脚本及两个显式例外。
- [`battle-spriteset-data.md`](./research/battle-spriteset-data.md)：46 文件 spriteset pointer/include
  图、header ranges 与 combatant macro 计数。
- [`battle-routing-data.md`](./research/battle-routing-data.md)：cutscene slots、region routes、terrain
  aliases、43 个 Stack-compressed terrain payload 的完整解码/ROM parity、unused joins 和旧
  aggregate 边界。
- [`map-data-inventory.md`](./research/map-data-inventory.md)：完整 1,390-file map ASM build graph、
  727 个内部 H1 binding、662 个 include-site-only body、64+66 setup selection rows、126 张六指针
  setup table 的 ROM parity、125 个 entity-list source/980 个物理记录与 suffix fallthrough、完整
  263 个 entity/zone/item event source/1,134 个物理记录与 9 个 first-match 选择案例，以及 75 个 description target/227 个物理
  entry、正常调用链上的 `d6` 条件、84 个 init source/90 个 callable entry，以及 47 个 standalone
  script/8,058 条语句；十案例 selector 与六案例 init-dispatch 单启动 H3 另确认 missing/default、
  last-set-flag-wins、alias route 及 active/scripted/direct-return init 调用。
- [`map-content.md`](./research/map-content.md)：79 个 46-byte map entry、662 个 source-form content
  section、154 个私有 blocks/layout payload 的完整 source/H1/ROM parity，77 组 bitstream 的
  canonical Python 解码、1,859-resource/79-map engine-neutral import（含 64 route、126 setup、
  178 个 standalone 与 201 个 init-source program）、
  record/consumer 规则与
  `MAPDATA_OFFSET_LAYOUT` 上游常量缺陷。
- [`auxiliary-data-inventory.md`](./research/auxiliary-data-inventory.md)：graphics/scripting/technical/
  sprite-dialogue 的 65 文件边界、77 个 indexed symbol/63 个 source file、两个 alternate，以及
  56 槽/52 payload 的 portrait header、动画元数据、palette 与 Stack 图形解码合同和 30 槽/27
  payload 的 battle background 双 tileset 解码合同、86 个 battle-sprite 容器/408 个图形 frame、208 个 animation
  sequence/421 个 frame entry，以及 weapon/ground graphics/palette、670 个 regular map-sprite
  payload、6 个 special-sprite stream 与 9 个
  special-screen tile stream、8 个 base/menu UI stream，以及 56 条 spell/invocation/status/
  transition stream、115 条 map-tileset stream、163 个 assembled icon/4 个 source-only icon 例外、
  27 个 UI layout/16 槽 spell pointer/4 套 border/4 个 direct tile payload，以及 16 套 map
  palette/79 个 header reference 的完整 parity；并单独闭合 119 行 map-sprite/portrait/speech-SFX
  属性、`0xFFFF` sentinel、first-match 与 fallback 消费规则的完整 source/H1/ROM parity，以及
  5 个 sprite 写入点、81 个脚本赋值、20 个 property-update caller 和 ally/enemy 派生域的
  237-250 排除审计，以及三个 shared entity-action corpus 的 2,864 bytes、118 labels、
  732 commands、38 个 relative branch 与 61-entry external-reference graph，并继续闭合 75 个
  distributed source、361 个 inline program、11 个 standalone ROM range、1,472 commands 和
  17 个具名入口，以及 80-slot dispatcher 的 37 filler/43 handler、40 个宏可达 opcode、三个
  handler-only branch opcode 与 `$8080` inline terminator 边界；handler catalog 还分类了 18 个
  实体字段的 11 read/17 write、15 个全局状态的 10 read/5 write、参数读取宽度、八个 handler
  家族、22 条实体 bit access、46 条脚本指针动作，以及 46 个宏参数/86 bytes 到 handler 读取的
  完整/低字节/跳过分类；另闭合 `ac_branch` 的宏外相对位移、三个 handler-only 6-byte layout 与
  35/43 handler 的完整源码使用边界，以及 39 redispatch/11 yield/7 dual 的全 handler flow
  outcome 分类；46 个参数的 10 signed/20 unsigned/15 boolean/1 ignored 解释与七个 dual
  predicate 也已绑定源码证据；后续 `UpdateEntityData` 另闭合 560 bytes、190 instructions、九个
  movement phase、15 个字段、5 个 bit access 和 16-byte facing table ROM parity；四个 update
  helper 再闭合 434 bytes、135 instructions、22 callers、目的地冲突 CCR、sprite fallthrough 与
  map-offset hash 公式；map-script engine 则闭合 90-slot dispatcher 的 82 个有效 opcode/8 个
  filler、83 个唯一 handler、82 主宏/8 alias/3 special，以及 169 个源码文件中的 13,515 次调用、
  955 条 handler 语句、16 个实体字段、25 个全局状态与 62 个 direct-call target；ABI 再闭合
  133 个主宏参数/operand field、234 个 operand bytes、2/4/6/8-byte 宽度分布，以及 77 sequential/
  1 absolute/4 conditional/1 inline 的 cursor-flow 分类。
- [`sound-data-inventory.md`](./research/sound-data-inventory.md)：41-file Z80 music include graph、
  两个 32 KiB bank 及其 canonical ROM byte parity。

## Design

`design/` 将已确认行为整理成实现无关的游戏设计规格。Phase 2 开始按 subsystem 创建；不能用
设计文档反向“证明”逆向结论。

- [`combat-resolution.md`](./design/combat-resolution.md)：物理攻击从 dodge、地形/克制、critical、
  spread、double/counter 到临时 HP、reaction 回放、EXP 入账与升级连接的实现无关合同，以及未来 H4 的共享
  fixture 边界。
- [`map-exploration.md`](./design/map-exploration.md)：79-map import boundary、共享 block/layout
  ownership、64x64 geometry、可执行 canonical import、area/event/item/animation 顺序、
  working-layout mutation 与现代 renderer 的原版事实/未知/可现代化边界。
- [`level-up.md`](./design/level-up.md)：成长曲线随机增益、最低成长补偿、战斗 EXP 阈值入口、完整升级顺序、
  投影后固定成长、职业等级上限、跨角色职业块扫描、当前/派生属性与装备刷新、属性上限/下溢夹断、敌人诅咒抑制、继承法术升级、Karna/HEAL 3 完整 prowess 高半字节矩阵、`LEVELUP_ARGUMENTS` 结果合同，以及 TORT
  effective-level 缺陷的原版事实和重制选择边界。
- [`spell-resolution.md`](./design/spell-resolution.md)：攻击法术的元素抗性位域、整数伤害调整、
  promoted power、DAO target-count division、spell critical、共用 downward spread、攻击法术 EXP、
  HEAL 1 治疗与治疗 EXP、SLEEP/SLOW 1 状态抗性与免疫、DESOUL 成败/即死/多目标 kill EXP/gold、SPOIT MP 吸收与边界截断、BOOST 1 属性/重施时序、SILENCE 施法门，以及临时状态回合后生命周期/持久场景回放边界的实现无关合同。

## Decisions

`decisions/` 记录引擎、模拟器、数据格式和工具链等耐久选择。只有出现真实分歧且选择会约束
后续实现时才创建 decision record。

- [`0001-bizhawk-for-h3-runtime-observation.md`](./decisions/0001-bizhawk-for-h3-runtime-observation.md)：
  固定 BizHawk 2.11.1，并记录 Genesis Plus GX 寄存器写入的实测边界。
- [`0002-python-and-uv-for-project-tooling.md`](./decisions/0002-python-and-uv-for-project-tooling.md)：
  Python/uv 工具链、稳定 CLI，以及现有 PowerShell rails 的冻结迁移边界。
- [`0003-static-first-batched-runtime-research.md`](./decisions/0003-static-first-batched-runtime-research.md)：
  Phase 2 先整批静态审计，再把不可静态判定的问题集中到单次 BizHawk 矩阵。

## Evidence Vocabulary

- **Confirmed**：脚本/test 可复现，或由具体反汇编位置与运行时观察共同支持。
- **Inferred**：证据充分但尚未独立复现。
- **Unknown**：仍需实验的问题，不允许用便利假设填空。

根 [`README.md`](../README.md) 是范围与路线的 source of truth；根 [`AGENTS.md`](../AGENTS.md)
是工作约束；本目录只拥有研究与设计内容。
