# 文档索引

> 本文件是 [`README.md`](./README.md) 的中文对照阅读版。英文版是规范索引与审阅基线；本文件为
> 派生文档，遵循 [`design/glossary.md`](./design/glossary.md) 的术语规则（R1–R7）。

文档按“证据 → 合同 → 实现选择”分层，避免把原版事实、推断和重制偏好混在一起。

## 无外部记忆的连续性

仓库是持久项目记录。新贡献者或 agent 应能在没有旧聊天记录或外部记忆库的情况下恢复工作：

1. 读根 [`README.md`](../README.md) 了解范围、基线、当前阶段、聚合证据与活跃前沿；
2. 读 [`research/source-coverage.md`](./research/source-coverage.md) 了解精确的覆盖分母、验证节奏与
   当前子系统方向；
3. 用本索引打开最近的 research、design 或 decision 所有者；
4. 在假设活动切片已完成或 worktree 干净之前，检查 `git status` 与最近提交；
5. 从受追踪的 manifest 与命令复现计数器，而不是复制过期的进度说明。

主题文档拥有详细发现与未知项，decision record 拥有持久的工具/架构选择，Git 历史拥有已完成切片的
时间线。外部 agent 记忆既非必需也不具权威性，本仓库的常规工作会禁用外部记忆。不要读或更新个人/
全局记忆库来恢复项目。如果用户显式要求一次性迁移审计，只把仍然有效的、项目特有的事实移入适当的受
追踪所有者，然后停止与该外部存储同步。当会话发现持久事实或改变前沿时，在同一切片内更新所属仓库
文档。

2026-07-19 的一次性迁移审计在已禁用的外部记忆索引中未找到 `md-sf2` 项目条目。聊天时代的持久决定
已在本仓库拥有：`AGENTS.md` 中的自主 Phase 2 切片与提交节奏、ADR 0002 中的 Python/uv 与聚焦提交
验证、ADR 0003 中的静态优先批量模拟，以及 `research/source-coverage.md` 中可复现的覆盖计数/前沿。
唯一剩余的仅实现层 Lua 语法预检规则在该次审计中被移入 ADR 0001。无需继续同步外部记忆。

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
  100% data-ASM H2 inventory、60.18% domain-aware data-file reach 的不同边界，以及静态优先、集中运行
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
  Kiwi 火焰、EGRESS/Angel Wing、battle load、移动音效，以及 6 个 player-control/cursor/menu
  入口和装备、give/drop、宝箱结果的静态状态机合同。
- [`battle-scene-engine.md`](./research/battle-scene-engine.md)：battle scene 根引擎 12 文件和动画
  实现 55 文件、21 条 scene-script 命令、初始化/选择器、32×2 setup/update 配对，以及完整
  87 ally/121 enemy battle-sprite sequence 和 421 条 frame entry 合同。
- [`battle-cutscenes.md`](./research/battle-cutscenes.md)：战前/开战/战后/败敌/区域 cutscene 的
  10 文件路由、flag 准入、leader-death position 准备和 map-script 调度。
- [`common-scripting.md`](./research/common-scripting.md)：entity/map/text/credits 的 29 文件
  inventory、90/80 槽解释器、完整 255-entry/86-tree/1,536-leaf context-Huffman corpus、
  17-bank/4,267-string/152,679-symbol 静态解码、80-glyph variable-width font/256-entry ASCII
  map 数据流、六个 map-script dialogue command 的 2,883 条 ordered program reference/handler/
  text-line/sprite-dialogue consumer 合同、五个 map-script transition command 的 146 条 ordered
  program-site/handler/caller 合同、六个 map-script roster/death command 的 43 条 ordered
  program-site/handler/caller 合同、两个 map-script block-copy command 的 208 条 ordered
  program-site/handler/cursor/helper/caller 合同、四个 map-script entity population/reload command 的
  96 条 ordered program-site/handler/caller 合同、单个 source-named `cloneEntity` command 的 9 条
  ordered program-site/handler/caller 合同、七个 map-script control/audio form 的 2,336 条 ordered
  source-site/static-control 合同与单启动六 case wait/skip/no-op/sound/subroutine/jump/end H3 边界、
  三个 map-script camera-control command 的 415 条
  ordered program-site/handler/caller/service 合同、四个 map-script map lifecycle command 的 108 条
  ordered program-site/handler/caller 合同，以及单启动五 case 的 handler return、direct-H1-JSR-site
  order、map/camera word 和双 marker H3 合同、三个 map-script camera-control command 的单启动七 case
  target branch/destination word-transfer/speed/wait H3 合同、四个 map-script entity-placement command 的单启动七 case
  alive/dead cursor、record-word/facing、31 次 flash loop/shared-tail 与 destination wait/bypass H3 合同、两个 source-named map-script trigger command 的 8 条
  ordered program-site/handler/caller/table-boundary 合同、四个 source-named map-script entity-placement
  command 的 2,288 条 ordered program-site/handler/caller 合同、六个 map-script 到 entity-action bridge
  command 的 3,256 条 ordered program-site/payload/handler/caller 合同、八个 source-named entity lifecycle/presentation
  command 的 464 条 ordered program-site/handler/caller 合同、七个 source-named entity gesture/relationship/motion
  command 的 545 条 ordered program-site/handler/caller 合同、十二个 source-named screen/map-presentation
  command 的 459 条 ordered program-site/handler/caller 合同、完整 entity-action 静态链和单启动 13-case/20-tick entity
  movement H3。
- [`common-maps.md`](./research/common-maps.md)：共享 map engine 7 文件、map switch、battle
  trigger、egress/savepoint、8 KiB layout 解压边界和 VInt gates。
- [`common-stats.md`](./research/common-stats.md)：共享 stats 20 文件、flags/party/inventory、
  field-item dispatch、spell learning、新游戏顺序、getter/mutation/clamp 静态合同和
  未 include 的 alternate source 边界。
- [`common-menus.md`](./research/common-menus.md)：共享菜单 42 文件、提示框/文本控制、field-item
  dispatch、商店/教堂/车队/铁匠的完整静态服务状态机、diamond/yes-no 压缩图形、完整 icon storage/copy/highlight
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
  独立 Z80 driver 构建链、RNG range-low-byte retry 与 controlled source-shaped copy 单启动矩阵、没有 symbolic consumer 的 cloud/base payload 边界，以及 20/20
  technical incbin 到 8 个深层 H2 owner 的可执行归属审计。
- [`gameflow-core.md`](./research/gameflow-core.md)：冷启动、系统初始化、主循环、战斗/探索分流、
  map event、交互和物品 handoff。
- [`special-screens.md`](./research/special-screens.md)：logo/title、witch save（四行 New/Load/Delete/Copy
  dispatcher、page selector、118 条 source-use provenance、SRAM action routing 与单启动 9 service/2 Load-branch
  及 4-case New/core-replay runtime matrix）、suspend/reset、ending 等 19 文件特殊画面边界，以及
  全部 9 条压缩 tile stream、DMA transfer/tail、choice palette 与 4×3 bubble-animation、7 套 palette/5 个
  layout 合同。
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
- [`map3-battle01-audit.md`](./research/map3-battle01-audit.md)：ADR 0009 Map 3 至 Battle 01 完成里程碑的研究所有缺口登记册，含已接受证据清单、精确索引分母、ADR 0010 画像约束、RA-01..RA-12 缺口与有序研究闭合计划；6A 持久性推迟而私有本地 7C/8C 证据为强制项；它仍是开放审计，不是就绪报告。
- [`map-data-inventory.md`](./research/map-data-inventory.md)：完整 1,390-file map ASM build graph、
  727 个内部 H1 binding、662 个 include-site-only body、64+66 setup selection rows、126 张六指针
  setup table 的 ROM parity、125 个 entity-list source/980 个物理记录与 suffix fallthrough、完整
  263 个 entity/zone/item event source/1,134 个物理记录、915 个 source/H1 target profile、
  684 个 entity-event、150 个 zone-event 与 80 个 item-event target program（另有 1 个 raw-expression
  exclusion）、3,579 个非注释操作（54 个 mnemonic、9 个 source-faithful family、34 个 macro/engine
  definition join 与 4 个 Map 21 action-payload context）、493 个 direct numeric flag source use
  （316 read / 169 set / 8 clear，151 个 operand、316 个 immediate conditional consumer）/各 category
  合计 469 个 instruction 与 effective target identity、147 个 direct `script` source reference
  （138 个 instruction label、135 个 effective map-script owner；348/304 个零计数完整 target domain）、
  以及 1,006 个 direct `TEXTBOX` source/H1 reference（981 个 numeric line-reference、25 个 `$FFFF`
  sentinel；914 个 caller 与 4,267 个 declared text-line ID 的零计数完整表，未解码 text）、
  378 个 pointer-table 与 390 个 selector-route category join、map44 raw-target boundary、9 个
  first-match 选择案例，以及 75 个 description target/227 个物理
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
  1 absolute/4 conditional/1 inline 的 cursor-flow 分类；完整源码进一步归属为 304 个 program/
  348 个 label，303 个 `csc_end` 加一个 jump 终止，并解析 42 条同 program、20 条跨 program
  script jump 和 122 条 68000 subroutine call；全 2,077 个 code/data ASM 的引用图再区分 187 个
  跨文件可引用、110 个仅同文件可引用和 7 个零引用 program，347/348 label 有引用。
  进一步闭合 89 个 program 的 story-state surface：51 条条件读取覆盖 6 个 flag，53 条直接写入、
  22 条 prompt 写入和 20 条 battle-unlock 写入覆盖 56 个 flag，读写域仅交于 71/76/89。
  同一 fixture 还以 304-row 零计数 program corpus 固定七个 story-state 分支/prompt form（146
  sites）、primary `csc10` 与 `setF`/`clearF` alias 的物理布局、五个 handler guard 和唯一 H3
  `story-state/branch-prompt-persistence-matrix`。
  相同 304-row 零计数 corpus 还固定四个 entity population/reload form（96 sites）、四个
  cursor/VInt/call/constant handler guard，以及保留 direct `j_InitializeMapEntities` 与 resolved
  `InitializeMapEntities` identity 的 zero-inclusive caller map；H3 的一启动 12 case fixture 现在固定
  handler-local callback/cursor/list/record 结果（含三个 selected `newEntity` index seed、direct table、
  identity-list-selected reload，以及七个 map-setup input）；剩余 queue 为 observed-high-water 之外的
  capacity、normal-story/save/map-reload persistence、player-visible rendering/animation/VDP timing、及
  collision/pathfinding consumer effects。
  同一 304-row 零计数 corpus 还固定四个 map lifecycle form（108 sites）、四个 named handler 的
  cursor/probe、VInt、branch、call/fall-through guards，以及 five-target zero-inclusive caller map；
  H3 以一个五 case/单启动 fixture 固定 handler return、direct-H1-JSR-site order、post-handler
  map/camera word 与双 nonasset marker，而剩余 queue 为 layout/collision/pathfinding、entity reload/
  player placement、presentation/fade/hardware timing 与 story reachability/persistence。
  同一 304-row 零计数 corpus 还固定两个 source-named trigger form（8 sites）、两段 named handler 的
  A6 word-read、`MAP_TILE_SIZE` use-site、call/return guards、two-target zero-inclusive caller map，
  以及独立解析的 94-step/114-roof table boundary；H3 以六 case/单启动 fixture 固定 Map 02
  record-0 hit、terminator miss、busy/battle gate、direct-H1-JSR-site、D0/D1 word、hash/table
  boundary、post-handler word 与双 marker。剩余 queue 为 full layout/collision/pathfinding、
  presentation/audio/timing/hardware 与 persistence/story reachability。
  同一 304-row 零计数 corpus 还固定八个 source-named entity lifecycle/presentation form（464
  sites）、八个 named handler 的 cursor/branch/callback/return guards，以及 nine-target
  zero-inclusive caller map。H3 以 11 case/单启动 fixture 固定所有八个 handler entry、live/zero-HP
  start/stop boundary、受控第二次 `waitIdle` compare、两侧 sprite selector、priority byte、完整
  remove-shadow callback chain，以及 source-backed temporary/restored sprite-size word 与 flags-B
  record；剩余 queue 为 normal-story reachability、full entity-state/callback effects 与
  player-visible presentation/timing/collision/persistence。
  同一 304-row 零计数 corpus 还固定七个 source-named entity gesture/relationship/motion form（545
  sites）、七个 named handler 的 A6 cursor、source operand/literal、branch/loop/call/return guards，
  以及 ten-target zero-inclusive caller map；其单启动 17-case H3 已覆盖七个 handler 的受控
  callback/state seam（包括 shiver 三轮 temporary/restore、follow high-byte zero-HP boundary、
  face/move word boundary、fly 两侧和 above register record）；剩余 queue 为 normal-story
  reachability、full entity-state/callback effects 与 player-visible presentation/timing/collision/persistence。
  同一 304-row 零计数 corpus 还固定十二个 source-named screen/map-presentation form（459 sites）、
  十二个 named handler 的 A6 cursor、immediate/operand、branch/loop/call/return guards，以及保留
  seven PC-relative `LaunchFading` target 的 five-target zero-inclusive caller map；其一启动 22-case
  H3 已固定 handler-local entry/return、cursor、direct-call/target/return、quake write、slow-counter
  和 flash-loop seam；visible/palette/VDP/timing/service body/persistence/reachability 仍保留四组
  Unknown queue。
  同一 304-row 零计数 corpus 还固定三个 source-named entity-presentation-FX form（177 sites）、
  三个 named handler 的 A6 cursor、immediate/operand、branch/function-chunk/loop/call/return guards，
  以及 nine-target zero-inclusive caller map；单启动 10-case H3 已固定 handler-local 的
  entry/operand/branch/loop/callback/return 记录及两个直接 entity-byte-write seam；剩余四组
  Unknown queue 为 normal-story reachability、player-visible output/timing/completion/repeat、
  bypassed-service/`WaitForVInt` effects 与 persistence/map-entity interactions。
  同一 304-row 零计数 corpus 还固定三个 source-named UI primary form（5 sites，含零使用 `menu`）、
  三个 named handler 的 A6 cursor、immediate/operand、branch/stack/call/return guards，及
  provenance join 的 portrait-helper 与 seven-target alias-aware caller map；单启动 11-case H3
  已验证 source-row input、busy/sentinel handler return、hide chronology 和 menu selector/A6/stack
  边界；正常剧情、完整 window/VDP timing、真实 choice/service side effect 与 persistence 仍为四组
  Unknown queue。
  同一 304-row 零计数 corpus 还固定单个 source-named `cloneEntity` `$25` form（9 sites）、完整
  `csc25_cloneEntity` 的两次 A6 word read/lookup 及唯一一字节 `ENTITYDEF_OFFSET_ENTNUM` transfer，
  而不把它提升为 whole-record copy/span；单启动 9-case H3 已固定 handler entry/RTS、A6 4/8
  cursor boundary、两次 word/lookup PC chronology、offset-18 byte before/after 和相邻字节 sentinel；
  其余 Unknown 为 neutral state、external-consumer 与 context 三组矩阵。
- [`sound-data-inventory.md`](./research/sound-data-inventory.md)：41-file Z80 music include graph、
  两个 32 KiB bank 的 canonical ROM parity、37 个 song range/address binding、29-macro/
  39,290-invocation 静态命令语料，以及单次启动 4 command/12 checkpoint/120 channel snapshot
  的 Z80 live-state H3 矩阵；同一 rail 还闭合驱动内嵌 56-entry SFX command/header 域及全部
  66 个 active stream 的 786-token/7-counted-loop 静态控制流。

## Design

`design/contracts/` 存放受证据约束、实现无关的子系统合同；`design/synthesis/` 存放消费已接受
证据的跨子系统或面向玩家的综合说明。共享治理文档保留在 `design/` 根目录。不能用设计文档
反向“证明”逆向结论。

中文本地化从英文 canonical 源以专用批次进行：术语表
[`glossary.md`](./design/glossary.md) 是英文→中文术语的单一绑定来源，镜像存于
`design/zh-CN/`，并保留 canonical 英文源的相对层级（英文文件始终是审阅基线）。翻译索引由
`manifests/zh-translation-index.json` 追踪，用 `uv run sf2 zh-meta test`（严格校验）与
`uv run sf2 zh-meta update`（保留已接受锚点的重生成）维护。只有在对照英文源码和当前术语表
审阅完变更镜像后，才能为每个允许更新锚点的文档重复传入
`--reanchor-source docs/design/<category>/<file>.md`。下列 design 文档条目均链接到 `design/zh-CN/`
下的中文镜像。

- [`glossary.md`](./design/glossary.md)：已接受的英文→中文术语表及 zh-CN 本地化规则；固定证据标签
  译法、保留源码标识符、一词一译、专有名词保留英文，以及 `design/zh-CN/` 镜像约定。
- [`documentation-roadmap.md`](./design/zh-CN/documentation-roadmap.md)：三层 证据/解释/现代化 边界、英文
  编写基线、近期综合顺序、长期方向、可复用编写结构与协作治理；它不是原版行为的证据，也不是重制产品决定。
- [`gameplay-overview.md`](./design/zh-CN/synthesis/gameplay-overview.md)：综合玩家动作、顶层状态流、本地循环与子系统
  交接，基于已接受的 gameflow、地图、输入、对话、队伍/名册、服务、战斗、成长与存档合同，同时保留
  战役、体验、平衡与上层设计 未知/决策 边界。
- [`tactical-battle-loop.md`](./design/zh-CN/synthesis/tactical-battle-loop.md)：综合一个受限的战术战斗循环，基于
  已接受的战斗控制、玩家/AI 控制、移动/目标、行动构建、战斗/法术解决、状态回放与结果证据，同时
  保留战术、平衡、呈现与一般模拟 未知/决策 边界。
- [`progression-and-economy.md`](./design/zh-CN/synthesis/progression-and-economy.md)：在对抗性 owner/fixture 审计后
  连接动作局部 EXP、持久 EXP 与升级、属性刷新、金币、敌人掉落、物品去向、源码静态服务交换与存档
  边界，同时保留平衡、战役可达性、服务运行时与端到端持久性 未知 边界。
- [`story-progression.md`](./design/zh-CN/synthesis/story-progression.md)：在对抗性 owner/fixture 审计后映射受限的
  顶层路线、有序配置/事件选择、脚本图、故事状态、对话、名册、过渡与存档交接，同时把剧情时间线、
  选择后果、常规存档可达性、完整持久性与呈现保留为 未知 边界。
- [`map-design-principles.md`](./design/zh-CN/synthesis/map-design-principles.md)：在对抗性 owner/fixture 审计后，把
  地图定义、几何/资源同一性、有序配置变体、交互选择与可变工作布局综合为证据受限的结构原则，同时把
  路线质量、节奏、碰撞/寻路、可达性、可见呈现与作者意图保留为 未知 边界。
- [`map3-battle01-readiness.md`](./design/zh-CN/synthesis/map3-battle01-readiness.md)：记录 ADR 0009 Map 3 至 Battle 01 里程碑的证据、设计合同与产品决定闭合台账，保留显式 **未就绪** 状态，以及 Phase 4 实现开始前所需的独立用户动作。
- [`ally-definition-data.md`](./design/zh-CN/contracts/ally-definition-data.md)：30 个盟友身份、两个独立 32 槽域、32 个职业定义、转职表、五条成长曲线、59 条成长记录、法术列表继承与呈现引用的实现无关合同。
- [`enemy-definition-data.md`](./design/zh-CN/contracts/enemy-definition-data.md)：103 个敌人身份、固定生成基线、有序战斗/地图呈现引用、独立 63 行 NPC 地图精灵尾段与未检查查找边界的实现无关合同。
- [`item-definition-data.md`](./design/zh-CN/contracts/item-definition-data.md)：128 个物品身份与固定定义、商店/宝箱/损坏/秘银/Caravan/野外使用目录、武器图形引用与受限查找规则的实现无关合同。
- [`spell-definition-data.md`](./design/zh-CN/contracts/spell-definition-data.md)：44 个法术身份与元素行、89 个固定八字节定义、打包身份/等级和动画字段，以及半径 3 数据例外的实现无关合同。
- [`spellbook-state.md`](./design/zh-CN/contracts/spellbook-state.md)：原始已学法术条目、定义未命中回退、调用方槽选择与独立计数，以及 `LearnSpell` 变更/结果的实现无关合同。
- [`name-table-lookup-service.md`](./design/zh-CN/contracts/name-table-lookup-service.md)：`GetClassName` 前端、长度前缀 `FindName` 遍历、受限有效表/索引结果、字栈保存与三个直接调用接缝的服务合同。
- [`stats-null-return-service.md`](./design/zh-CN/contracts/stats-null-return-service.md)：受限 `nullsub_9482` 源码身份及其唯一立即返回指令的溯源与兼容合同。
- [`audio-system.md`](./design/zh-CN/contracts/audio-system.md)：音乐/SFX 命令身份、双 bank 槽与目标别名、header/通道角色、静态宏/音符/采样/乐器/SFX 域及四命令播放状态矩阵的实现无关合同。
- [`combatant-state-access.md`](./design/zh-CN/contracts/combatant-state-access.md)：源码形态战斗员选择器、56 字节 entry ABI、类型化 getter、变更 wrapper、夹断 helper、九操作运行时矩阵、距离 helper 与未使用类型编码器的合同。
- [`global-flag-state.md`](./design/zh-CN/contracts/global-flag-state.md)：掩码标志索引、每字节八标志、bit-7 优先选择、共享 Check/Set/Clear 寻址与四 wrapper trap 清单的合同。
- [`graphics-service-state.md`](./design/zh-CN/contracts/graphics-service-state.md)：受限解压入口 ABI、显示初始化、精灵链接/调色板过渡状态、特殊精灵路由、固定闪烁字与图形 helper 清单的合同。
- [`special-sprite-graphics-data.md`](./design/zh-CN/contracts/special-sprite-graphics-data.md)：十个有序特殊精灵指针、五个初始载荷所有者、五个别名、六个源码资源定义与聚合解码/一致性元数据的私有导入合同。
- [`special-screen-asset-data.md`](./design/zh-CN/contracts/special-screen-asset-data.md)：九个 Stack 压缩特殊画面资源、十二个未压缩调色板/布局资源与女巫选择/气泡表的私有导入合同。
- [`special-screen-control-flow.md`](./design/zh-CN/contracts/special-screen-control-flow.md)：受限 Sega logo/title 控制、女巫入口/动作页/派发/菜单接缝、挂起计数器/重置交接与结局操作所有者身份的合同。
- [`portrait-graphics-data.md`](./design/zh-CN/contracts/portrait-graphics-data.md)：56 个有序立绘槽、52 个源码载荷所有者、四个别名、计数眼/嘴记录、调色板/stream 分区与聚合解码/一致性元数据的私有导入合同。
- [`portrait-window-state.md`](./design/zh-CN/contracts/portrait-window-state.md)：战斗员/盟友立绘选择、规范立绘数据消费、立绘/姓名窗口状态、callback 生命周期、精确加载/DMA 顺序与受限眼/嘴更新的合同。
- [`caravan-and-deals-state.md`](./design/zh-CN/contracts/caravan-and-deals-state.md)：Caravan 状态位正规化、满载忽略、有序移除压缩/尾部清零与 Deals 打包计数饱和/零移除边界的合同。
- [`new-game-state-initialization.md`](./design/zh-CN/contracts/new-game-state-initialization.md)：两个顶层初始化顺序边、完整原版盟友 entry 覆盖、空法术状态、职业→初始→派生属性顺序、状态清零与消息速度默认值的合同。
- [`party-membership-state.md`](./design/zh-CN/contracts/party-membership-state.md)：独立 joined/active 成员标志、三个 `UpdateForce` 计数前缀、`JoinForce` 先设 joined 再重建的时间顺序与离队操作身份的合同。
- [`combat-resolution.md`](./design/zh-CN/contracts/combat-resolution.md)：物理攻击从 dodge、地形/克制、critical、
  spread、double/counter 到临时 HP、reaction 回放、EXP 入账与升级连接的实现无关合同，以及未来 H4 的共享
  fixture 边界。
- [`exploration-control-flow.md`](./design/zh-CN/contracts/exploration-control-flow.md)：`MainLoop` 探索交接、迭代内地图事件优先级、玩家动作优先级、交互准入及受限 area/refill/flag/update 清单的实现无关合同。
- [`startup-control-flow.md`](./design/zh-CN/contracts/startup-control-flow.md)：源码形态条件初始配置、受限写入/循环范围、有序系统与游戏交接、logo/intro/title 返回路由及区域准入分支的合同。
- [`standalone-map-script-program-data.md`](./design/zh-CN/contracts/standalone-map-script-program-data.md)：47 个独立地图脚本源码文件、178 个非空程序、8,058 个有序操作、目标/词法引用拓扑与受限 init 目标所有权连接的私有导入合同。
- [`debug-control-flow.md`](./design/zh-CN/contracts/debug-control-flow.md)：战斗测试配置与服务交接、配置门/写入、七条调试战斗动作路线与四个 helper 局部栈写入的实现无关合同。
- [`rom-header-data.md`](./design/zh-CN/contracts/rom-header-data.md)：独立 64-entry vector-table 摘要与所选 console-header 元数据的实现无关溯源/导入合同。
- [`map-area-description-routing.md`](./design/zh-CN/contracts/map-area-description-routing.md)：126 个有序 setup 引用、75 个可调用目标、37 个 wrapper、38 个直接返回 stub、227 个物理 entry、first-match 选择与受限 `d6=1` 规则的私有导入/路由合同。
- [`map-camera-update-control-flow.md`](./design/zh-CN/contracts/map-camera-update-control-flow.md)：受限 `VInt_UpdateViewData` 目标分支、目的地请求接缝、计数路线、滚动速度优先级与四条精确字宽视差更新路径的合同。
- [`map-entity-data.md`](./design/zh-CN/contracts/map-entity-data.md)：126 个 setup 指针引用、125 个 entity-list 根、980 条物理记录形成的 987 个有序列表引用、后缀共享/终止拓扑与初始地图精灵域的私有导入合同。
- [`map-entry-routing-state.md`](./design/zh-CN/contracts/map-entry-routing-state.md)：有序标志切换地图选择、战斗候选准入及受限状态写入，以及存档点/木筏重置选择的合同。
- [`unused-mapload-control-flow.md`](./design/zh-CN/contracts/unused-mapload-control-flow.md)：源码命名未使用 mapload 入口、四个有序 RNG 请求操作数/暂存流、后 helper 测试的 VDP/VInt 请求循环与四字 signed-store helper 的归档合同。
- [`map-exploration.md`](./design/zh-CN/contracts/map-exploration.md)：79-map import boundary、共享 block/layout
  ownership、64x64 geometry、可执行 canonical import、area/event/item/animation 顺序、
  working-layout mutation、两个 source-faithful map-script block-copy form、四个 source-shaped
  entity population/reload form、单个 source-faithful `cloneEntity` form、三个 source-faithful map-script camera-control form、四个 source-faithful map lifecycle form、两个 source-named
  trigger form、四个 source-named entity-placement form、六个 source-named entity-action bridge form、八个 source-named
  entity lifecycle/presentation form、七个 source-named entity gesture/relationship/motion form、十二个 source-named
  screen/map-presentation form 与现代 renderer 的
  原版事实/未知/可现代化边界。
- [`map-layout-data.md`](./design/zh-CN/contracts/map-layout-data.md)：77 个唯一 block/layout 载荷所有者服务 79 个地图引用、两个共享所有者别名、完整解码 block/64×64 layout 形状、聚合 decoder-family 计数与 source/ROM 一致性的私有导入合同。
- [`map-palette-data.md`](./design/zh-CN/contracts/map-palette-data.md)：16 个有序源码/有效地图调色板身份、79 个有序私有地图引用、公开 usage/一致性元数据与源码形态首字清零规则的私有导入合同。
- [`map-sprite-assignment-surface.md`](./design/zh-CN/contracts/map-sprite-assignment-surface.md)：五个地图精灵 writer 来源、一个索引派生 helper、私有 81-assignment/20-caller 目录与已接受 built-input 中 ID 237..250 排除的合同。
- [`map-sprite-graphics-data.md`](./design/zh-CN/contracts/map-sprite-graphics-data.md)：720 个有序常规地图精灵源码槽、670 个载荷身份、完整别名关系、669 个定长 Basic 解码形式与共享 sentinel 身份的私有导入合同。
- [`map-setup-data.md`](./design/zh-CN/contracts/map-setup-data.md)：64 个有序地图行、66 个有序标志行、130 个 setup 引用、126 个六槽定义身份、四个别名与受限 entity-list 访问接缝的私有导入合同。
- [`map-tileset-data.md`](./design/zh-CN/contracts/map-tileset-data.md)：115 个有序压缩/解码地图 tileset 身份、私有 79-map/32-animation 引用关系与受限公开 usage/一致性元数据的私有导入合同。
- [`battle-ai-decision.md`](./design/zh-CN/contracts/battle-ai-decision.md)：AI 法术/物品过滤、优先级/治疗/支援评分、最终动作/目标、Move/Move Order、临时地形、commandset 与 14-case 运行时矩阵的实现无关合同。
- [`battle-action-construction.md`](./design/zh-CN/contracts/battle-action-construction.md)：动作族路由、目标顺序、物理提前退出、物品使用/损坏路由、Taros/Burst Rock 门、消息命令记录与 54-site 消息语料的合同。
- [`battle-cutscene-routing.md`](./design/zh-CN/contracts/battle-cutscene-routing.md)：四个独立 48 槽路线表、59 个 built cutscene 程序、intro/completion/leader/region 门、join/dead-list 尾段与修正的 leader-position X/HP 范围的合同。
- [`battle-encounter-definition.md`](./design/zh-CN/contracts/battle-encounter-definition.md)：45 槽 spriteset、地图/全局与地形选择骨架、放置/局部 AI 几何、支援战斗元数据、地形别名与独立 48 槽 cutscene 路由命名空间的合同。
- [`battle-functions-control-flow.md`](./design/zh-CN/contracts/battle-functions-control-flow.md)：fixture 约束的单回合路由、战斗加载/移动命令选择、光标/目标列表控制、战斗/战场菜单分支及有序装备/物品/宝箱请求的合同。
- [`battle-background-graphics-data.md`](./design/zh-CN/contracts/battle-background-graphics-data.md)：30 个有序背景槽、27 个载荷所有者、三个别名、精确 stream 前缀/调色板/双 stream 结构与聚合解码/一致性元数据的私有导入合同。
- [`battle-effect-graphics-data.md`](./design/zh-CN/contracts/battle-effect-graphics-data.md)：23 个法术、30 个 invocation、一个 status-animation 与两个 battle-transition stream 身份及其私有 container/palette/offset 关系的私有导入合同。
- [`battle-sprite-graphics-data.md`](./design/zh-CN/contracts/battle-sprite-graphics-data.md)：独立 32 槽盟友/54 槽敌人表、86 个源码载荷所有者、header/palette/frame-stream 结构与聚合解码/一致性元数据的私有导入合同。
- [`battle-weapon-ground-graphics-data.md`](./design/zh-CN/contracts/battle-weapon-ground-graphics-data.md)：23 槽武器表、42 条武器调色板、30 槽/27-header 地面表与别名、33 个 stream 所有者及异构调色板/header 核算的私有导入合同。
- [`battle-scene-presentation.md`](./design/zh-CN/contracts/battle-scene-presentation.md)：21-command scene interpreter、初始化/selector 顺序、各类战斗资源选择/加载/呈现接缝、208 条 actor-animation sequence 与法术 setup/update 派发的合同。
- [`battle-control-lifecycle.md`](./design/zh-CN/contracts/battle-control-lifecycle.md)：新/恢复战斗入口、回合激活/生成/调度、死亡 worklist/清理、Battle 01 region/turn-order 行为、after-turn 双方检查与静态胜负变更的合同。
- [`battlefield-navigation.md`](./design/zh-CN/contracts/battlefield-navigation.md)：48×48 战场网格、地形/占用状态、加权移动传播、Manhattan 范围/目标准入、攻击位置选择、移动串与五 case 原版运行时移动矩阵的合同。
- [`level-up.md`](./design/zh-CN/contracts/level-up.md)：成长曲线随机增益、最低成长补偿、战斗 EXP 阈值入口、完整升级顺序、
  投影后固定成长、职业等级上限、跨角色职业块扫描、当前/派生属性与装备刷新、属性上限/下溢夹断、敌人诅咒抑制、继承法术升级、Karna/HEAL 3 完整 prowess 高半字节矩阵、`LEVELUP_ARGUMENTS` 结果合同，以及 TORT
  effective-level 缺陷的原版事实和重制选择边界。
- [`spell-resolution.md`](./design/zh-CN/contracts/spell-resolution.md)：攻击法术的元素抗性位域、整数伤害调整、
  promoted power、DAO target-count division、spell critical、共用 downward spread、攻击法术 EXP、
  HEAL 1 治疗与治疗 EXP、SLEEP/SLOW 1 状态抗性与免疫、DESOUL 成败/即死/多目标 kill EXP/gold、SPOIT MP 吸收与边界截断、BOOST 1 属性/重施时序、SILENCE 施法门，以及临时状态回合后生命周期/持久场景回放边界的实现无关合同。
- [`byte-copy-service.md`](./design/zh-CN/contracts/byte-copy-service.md)：受限正长度域上保留重叠的 `CopyBytes` 结果、原版 signed 方向选择、精确 `d7`/`a0`/`a1` 保存接缝与允许使用平台 `memmove` 的现代化合同。
- [`music-wait-service.md`](./design/zh-CN/contracts/music-wait-service.md)：两个有序源码命令请求身份、`Sleep(3)` 后谓词形状与 `k + 1` 等待请求兼容轨迹的实现无关合同。
- [`service-interactions.md`](./design/zh-CN/contracts/service-interactions.md)：shop、church、caravan/depot 与
  blacksmith 的动作顺序、取消边界与静态资源 mutation 合同，以及明确保留的持久化/时序未知项。
- [`save-system.md`](./design/zh-CN/contracts/save-system.md)：两槽 SRAM、交错字节布局、checksum、occupied flag 与
  save/load/copy/delete 静态合同、单启动 in-process service matrix，以及仍留给 H3 的跨进程持久化和断电边界。
- [`input-system.md`](./design/zh-CN/contracts/input-system.md)：双端口原始采样、VInt current/repeat 过滤、输入等待
  helper 与控制器/时序未知边界。
- [`text-and-font-system.md`](./design/zh-CN/contracts/text-and-font-system.md)：255-entry context-Huffman 表、86 棵可达树、17 个 bank/4,267 条记录、聚合符号回放、80 个变宽 glyph 与 256-entry ASCII map 的实现无关合同。
- [`ui-graphics-asset-data.md`](./design/zh-CN/contracts/ui-graphics-asset-data.md)：九个共享 UI 资源/指针身份、八个 Stack/一个未压缩载荷、异构菜单路线、167 个源码 icon path/163 槽 vanilla 组装及复制/高亮变换的私有导入合同。
- [`ui-layout-data.md`](./design/zh-CN/contracts/ui-layout-data.md)：27 个有序 UI 布局网格、一等 16-entry 法术等级指针表与别名、四个菱形边框、四个直接资源及精确不重叠 source/ROM 覆盖的静态数据合同。
- [`unused-menu-constant-write-control-flow.md`](./design/zh-CN/contracts/unused-menu-constant-write-control-flow.md)：受限 `sub_15268` 常量写入时间顺序、精确 14-longword 准入内存投影与静态零符号调用者清单的合同。
- [`unused-technical-asset-data.md`](./design/zh-CN/contracts/unused-technical-asset-data.md)：一个源码名义未使用的 5,694 字节 container、四个有序 Stack stream、两个有序调色板、独立 palette-pointer 身份与受限符号引用清单的私有导入合同。
- [`window-system.md`](./design/zh-CN/contracts/window-system.md)：八槽 window entry、layout 分配/回收、packed
  coordinate 寻址、VInt composition/DMA 调用顺序，以及呈现时序未知边界。
- [`dialogue-system.md`](./design/zh-CN/contracts/dialogue-system.md)：六个 map-script dialogue command 的物理
  layout、cursor/name-index/portrait consumer 静态顺序，以及 21-case 单启动 handler-local H3 合同和
  三个明确的 presentation/runtime Unknown 边界。
- [`sprite-dialogue-property-data.md`](./design/zh-CN/contracts/sprite-dialogue-property-data.md)：119 条有序地图精灵对话属性记录、独立 terminator、不同 key/portrait/speech-SFX 元数据与 signed/zero-extended first-match 查找结果的合同。
- [`party-roster-state.md`](./design/zh-CN/contracts/party-roster-state.md)：十个 map-script roster/death 与
  active-party/AI/follower source form 的 physical layout、named handler branch/mutation/call order、
  alias-aware caller identity，以及两个 grouped H3 runtime 边界。
- [`randomness.md`](./design/zh-CN/contracts/randomness.md)：主 RNG、debug 方向覆盖、AI byte RNG、有界采样、helper-return state 与 controlled source-shaped copy 的
  静态/运行时合同，以及 retry 与 seed-copy 隔离边界。
- [`interrupt-dma-and-trap-state.md`](./design/zh-CN/contracts/interrupt-dma-and-trap-state.md)：VInt 调度门/有序阶段、contextual slot、wait/sleep/DMA 控制状态、fade 谓词与受限 trap transport 的实现无关合同。

## Decisions

`decisions/` 记录引擎、模拟器、数据格式和工具链等耐久选择。只有出现真实分歧且选择会约束
后续实现时才创建 decision record。

- [`0001-bizhawk-for-h3-runtime-observation.md`](./decisions/0001-bizhawk-for-h3-runtime-observation.md)：
  固定 BizHawk 2.11.1，并记录 Genesis Plus GX 寄存器写入的实测边界。
- [`0002-python-and-uv-for-project-tooling.md`](./decisions/0002-python-and-uv-for-project-tooling.md)：
  Python/uv 工具链、稳定 CLI，以及现有 PowerShell rails 的冻结迁移边界。
- [`0003-static-first-batched-runtime-research.md`](./decisions/0003-static-first-batched-runtime-research.md)：
  Phase 2 先整批静态审计，再把不可静态判定的问题集中到单次 BizHawk 矩阵。
- [`0004-single-terra-worker-with-root-acceptance.md`](./decisions/0004-single-terra-worker-with-root-acceptance.md)：
  单一 Terra worker 完成 Phase 2 证据切片，root 线程独立复核、验证、扫描并提交的工作流边界。
- [`0005-remake-value-driven-driver-freeze.md`](./decisions/0005-remake-value-driven-driver-freeze.md)：
  保留既有证据与验证，同时冻结低重制价值的 driver/hardware 精确度，把 Phase 2 主线转向事件、地图、
  UI/存档与实现无关内容合同。
- [`0006-parallel-worktrees-and-topic-branch-integration.md`](./decisions/0006-parallel-worktrees-and-topic-branch-integration.md)：
  `main` 串行集成、research/design 双 worktree 车道、短生命周期 topic branch、共享文件所有权与
  tracked-only 远端检查的协作边界。
- [`0007-schema-contract-composition-and-migration.md`](./decisions/0007-schema-contract-composition-and-migration.md)：
  审计巨型 schema 的 golden/shape 重复，规定本地 `$ref` registry、结构合同与精确 fixture 分层，
  并按 common-stats、common-menus、map-events、map-script/H3 的顺序迁移且不削弱负向门禁。
- [`0008-godot-csharp-cli-first-remake-tooling.md`](./decisions/0008-godot-csharp-cli-first-remake-tooling.md)：审计既有 Godot/C# 与机器本地 MCP 工作流，接受 Godot 4.7.2 .NET/C# 为固定 CLI-first Phase 4 基线，并采用纯 C# 领域层、薄 Godot 适配器与可选可移除 MCP；接受并不启动重制实现。
- [`0009-first-phase4-playable-slice.md`](./decisions/0009-first-phase4-playable-slice.md)：接受从 Map 3 到 Battle 01 完成的一个连续场景作为首个可玩里程碑，并要求独立研究/设计缺口审计、已接受缺口闭合、主门禁就绪报告与单独明确的 Phase 4 启动动作。
- [`0010-map3-battle01-product-acceptance.md`](./decisions/0010-map3-battle01-product-acceptance.md)：接受仅限私有本地的 Map 3 至 Battle 01 产品画像，包括自然可玩连续性、禁止公开再分发的原版私有资源、帧/音频/硬件精确一致性、现代可访问控制，以及 Phase 4 前剩余研究/H4 门。
- [`0011-phase4-remake-runtime-architecture.md`](./decisions/0011-phase4-remake-runtime-architecture.md)：接受确定性模块化单体边界、纯 C# 权威状态、验证数据端口、薄 Godot 适配器与分层 H4 门，而不启动 Phase 4 实现。
- [`0012-dependency-aware-partitioned-verification.md`](./decisions/0012-dependency-aware-partitioned-verification.md)：接受始终运行的 public core 加保守受影响 Python/H1/H2/H3 分区、只读 committed-range planner 与供后续编排使用的显式资源锁边界。

## 证据词汇

- **Confirmed（已确认）**：脚本/test 可复现，或由具体反汇编位置与运行时观察共同支持。
- **Inferred（推断）**：证据充分但尚未独立复现。
- **Unknown（未知）**：仍需实验的问题，不允许用便利假设填空。

根 [`README.md`](../README.md) 是范围与路线的 source of truth；根 [`AGENTS.md`](../AGENTS.md)
是工作约束；本目录只拥有研究与设计内容。
