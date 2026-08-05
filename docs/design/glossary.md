# Design Localization Glossary (EN → 中文)

- Status: **accepted repository governance** for zh-CN localization of `docs/design/` synthesis
  documents; this document adds no original-game fact and selects no remake product direction.
- Record date: 2026-08-04
- Owner: design-synthesis lane
- Scope: `docs/design/*.md` and their zh-CN mirrors under `docs/design/zh-CN/`
- Authority: the Authoring Language Policy in
  [`documentation-roadmap.md`](./documentation-roadmap.md) accepts this glossary as the project
  glossary required before non-English localization begins.

## Purpose and Authority

This glossary is the single binding source for English-to-Chinese terminology in design-synthesis
documents. It exists so a localization batch produces terminology-consistent output instead of ad hoc
bilingual invention, which the Authoring Language Policy explicitly prohibits while the glossary is
unsettled.

It governs:

- future zh-CN translations of `docs/design/*.md` under `docs/design/zh-CN/`;
- bilingual annotations already allowed inside English design documents (for example a parenthetical
  Chinese term next to a proper noun at first occurrence).

It does not govern:

- `docs/research/`, `schemas/`, `manifests/`, fixtures, or source-adjacent vocabulary, whose content
  stays source-faithful and English-first; this glossary may later be extended by an explicit
  governance decision if those lanes adopt localization;
- code vocabulary, symbol names, or fixture IDs, which are preserved verbatim under rule R2 below;
  evidence labels are the explicit exception and use the fixed R1 translations.

## Terminology Rules

The rules below are normative. Term tables at the bottom are the accepted translations. A conflict
between a rule and a table resolves in favor of the rule; a conflict between two tables resolves in
favor of the more specific category.

### R1 — Evidence labels and verification layers use fixed translations

Evidence labels must remain recognizable in every document, translated or not, because they gate the
evidence model. Use the fixed Chinese renderings below everywhere, including inside zh-CN mirrors:

| English | 中文 | Note |
| --- | --- | --- |
| Confirmed | 已确认 | Evidence label; a reproduced or source-and-runtime-backed claim |
| Inferred | 推断 | Evidence label; strong but not yet independently reproduced |
| Unknown | 未知 | Evidence label; explicit open question, never filled by assumption |
| Contract | 合同 | Implementation-neutral contract; matches existing usage in `docs/README.md` |
| Fixture | 测试夹具 | May remain "fixture"; the fixture ID itself is never translated |
| Evidence owner | 证据所有者 | The research document that owns the claim |
| Evidence date | 证据日期 | Header field; must be the actual observation date |
| Source baseline | 源码基线 | Pinned upstream commit identity |
| Traceability | 可追溯性 | Contract-to-fixture relationship |
| Provenance | 溯源 | Reproducible source/command/ROM evidence trail |

Verification layers `H0` through `H5`, phases `Phase 0` through `Phase 5`, `Layer A/B/C`, and `ADR`
numbers are identifiers and are never translated or renumbered.

### R2 — Source-faithful identifiers are never translated

The Authoring Language Policy requires preserving source-faithful identifiers, fixture IDs, and code
vocabulary rather than inventing translated equivalents. Evidence labels are the explicit exception:
they must use the fixed Chinese forms in R1. A zh-CN translation must copy the following classes
verbatim:

- source symbols and memory labels, e.g. `CURRENT_BATTLEACTION`, `RANDOM_SEED`,
  `CUTSCENE_DIALOG_INDEX`, `LEVELUP_ARGUMENTS`, `UpdatePlayerInputs`, `CheckSram`, `LevelUp`;
- source macro names, e.g. `nextSingleText`, `textCursor`, `setPos`, `setFacing`, `join`,
  `reviveAlly`, `addNewFollower`, `warp`, `loadMapFadeIn`;
- source handler labels, e.g. `csc00_displaySingleTextbox`, `csc25_cloneEntity`,
  `csc0E_jumpIfForceMemberInList`;
- fixture IDs and file paths, e.g. `sf2-map-script-engine-static-v1`,
  `tests/fixtures/h3/map-script-dialogue-v1.json`;
- source- or contract-significant numeric literals, packed bytes, opcodes, operand widths, addresses,
  and register names (`$00`, `0xFF`, `$FFFF`, 6-byte, `d1`, `a6`, VDP, DMA, VInt, SRAM, Z80, 68000);
- verification layer names, command names, and any identifier spelled in `code` or `UPPER_SNAKE`.

A translation may add a Chinese gloss after the identifier the first time it appears, e.g.
`UpdateCombatantStats（刷新战斗员派生属性）`, but it must not replace the identifier or change its
spelling, case, or separators. Ordinary prose ordinals and quantities may use natural Chinese
wording only when their value and unit remain unchanged; values that identify a source record,
fixture case, field, byte, offset, address, or executable boundary stay verbatim.

### R3 — Mechanics and process terms use one term, one translation

Binding terms are translated consistently across all documents. When an English term has multiple
Chinese senses in this repository, the table records both senses and their context (for example
`cursor` as UI cursor vs script stream cursor, `inventory` as item container vs source inventory).
Use the recorded rendering; do not introduce synonyms for an already-listed term.

### R4 — Proper nouns are bilingual in the glossary; the body keeps English

Character, spell, item, and class names keep their English form in English and zh-CN bodies. The
glossary records suggested Chinese renderings for first-occurrence annotations. These renderings are
**suggestions to be finalized per entry during the localization batch review**; they are not
original-game evidence and do not affect source identifiers, fixture IDs, or evidence labels. Where
no community-standard rendering exists, the entry stays `待定` until the batch review decides it.

### R5 — Adding or revising entries

1. Propose the term in the owning localization branch with the English form, proposed Chinese form,
   category, and one or two documents that use it.
2. Review against existing entries; a near-duplicate updates the existing row instead of adding a new
   one.
3. Accept in the same design lane review that accepts the translation batch; record the date.
4. Update every affected zh-CN mirror in the same batch so the glossary and the mirrors do not drift.

A binding term (R3) may only be changed together with a batch-wide search/replace of all affected
documents; a proper-noun suggestion (R4) may be changed per entry.

### R6 — zh-CN mirror conventions

- A mirror lives at `docs/design/zh-CN/<same-filename>.md` and translates the English file of the
  same name. It is a derivative; the English file remains the review baseline.
- Preserve verbatim under R2: identifiers, fixture IDs and paths, source/code labels,
  contract-significant numeric literals, header field values, and evidence-layer/phase/ADR names.
- Rewrite relative links by adding one `../` level because the mirror sits one directory deeper
  (`../research/x.md` → `../../research/x.md`, `../../tests/...` → `../../../tests/...`). Link
  integrity is an acceptance criterion of every batch.
- Keep the evidence-label translations from R1 so a Chinese reader can still recognize label
  boundaries.
- Translate player- or reader-visible prose inside diagrams, including Mermaid node and edge labels;
  preserve only the source identifiers embedded in those labels.
- A mirror is not registered in `src/sf2tool/design_contracts.py`; the English file owns contract
  traceability.

### R7 — Docs-index counterpart

The docs index `docs/README.md` stays English-only and does not list zh-CN mirrors. Its Chinese
reading copy lives as the sibling file `docs/README.zh-CN.md`, which is not a mirror of a design
synthesis document and is outside the zh-meta translatable set. Each file cross-links the other at
the top; the English file remains the review baseline.

## Term Tables

### A. Evidence and verification vocabulary

| English | 中文 | Note |
| --- | --- | --- |
| Confirmed / Inferred / Unknown | 已确认 / 推断 / 未知 | R1 fixed labels |
| evidence | 证据 | |
| evidence owner | 证据所有者 | |
| evidence label | 证据标签 | |
| evidence-bound contract | 证据绑定合同 | Layer B contract tied to Layer A evidence |
| provenance | 溯源 | source/command/ROM reproduction trail |
| contract | 合同 | implementation-neutral contract |
| fixture | 测试夹具 | may keep "fixture"; ID untranslated |
| golden fixture | 黄金夹具 | exact expected-output fixture |
| schema | schema（模式） | keep "schema"; schemas are canonical data contracts |
| gate | 门禁 | verification gate; matches `docs/README.md` |
| verification rail | 验证轨道 | maintained H2/H3 verification surface |
| verification layer | 验证层 | H0–H5 |
| baseline | 基线 | pinned source/ROM baseline |
| canonical | 规范 | authoritative, canonical output |
| parity | 一致性 | source/ROM/fixture agreement |
| bit-perfect | 逐位一致 | byte-for-byte rebuild |
| rebuild | 重建 | original ROM reconstruction |
| provenance record | 溯源记录 | |
| traceability | 可追溯性 | |
| coverage denominator | 覆盖分母 | evidence-coverage denominator |

### B. Battle and combat

| English | 中文 | Note |
| --- | --- | --- |
| battle | 战斗 | |
| combat | 交战 | physical combat resolution |
| round | 回合 | one full battle round |
| turn | 回合（单次行动） | an individual actor's turn |
| actor | 行动者 | acting combatant |
| attack | 攻击 | |
| physical attack | 物理攻击 | |
| dodge | 闪避 | |
| critical | 会心一击 | may gloss 暴击 |
| double attack | 二连击 | |
| counterattack | 反击 | |
| damage | 伤害 | |
| damage stage | 伤害阶段 | per-stage integer damage |
| land effect | 地形效果 | decoded terrain land-effect value |
| terrain | 地形 | |
| movement type | 移动类型 | combatant movement classification |
| prowess | 特殊攻击概率（会心/二连/反击） | SF2 特有的打包字节：低半字节为会心率、高半字节为二连/反击率；被动概率设定而非可主动施放的技能 |
| spell | 法术 | |
| spell level | 法术等级 | |
| element | 元素 | spell element, e.g. FIRE 火元素 |
| elemental resistance | 属性抗性 | two-bit packed resistance setting |
| weakness | 弱点 | resistance setting 3 for damage |
| immunity | 免疫 | resistance setting 3 for status |
| status effect | 状态效果 | |
| instant death | 即死 | DESOUL-style outcome |
| MP absorption | MP 吸收 | |
| target | 目标 | |
| target selection | 目标选择 | |
| targeting list | 目标列表 | ordered candidate targets |
| move range | 移动范围 | |
| pathfinding | 寻路 | |
| movement cost | 移动消耗 | |
| terrain obstruction | 地形阻挡 | |
| attack position | 攻击位置 | selected attack-adjacent tile |
| turn order | 行动顺序 | |
| region activation | 区域激活 | enemy region activation |
| battle scene | 战斗演出 | generated battle-scene command replay |
| reaction | 反应（演出） | HP reaction replay |
| action | 行动 | committed battle action |
| stay / search | 待机 / 搜索 | battle menu actions |
| victory / defeat | 胜利 / 败北 | |
| battle exit | 战斗退出 | EGRESS/Angel Wing exit state |
| AI | AI | keep "AI" |
| commandset | 指令集 | AI commandset |

### C. Growth, stats, and economy

| English | 中文 | Note |
| --- | --- | --- |
| level-up | 升级 | |
| level | 等级 | |
| level cap | 等级上限 | class level cap |
| stat | 属性值 | HP/MP/ATT/DEF/AGI |
| stat gain | 属性增益 | randomized gain per level |
| stat growth | 属性成长 | growth-curve behavior |
| growth curve | 成长曲线 | |
| projection | 成长投影 | per-level projected gain |
| minimum-stat pity | 最低成长补偿 | expected-minimum floor |
| refresh | 刷新 | derived-stat refresh |
| derived stat | 派生属性 | current/base-derived combatant stats |
| class | 职业 | |
| class block | 职业块 | class growth block |
| promotion | 转职 | |
| promoted | 转职后 | promoted effective level |
| ally | 己方角色（伙伴） | playable force member; vs enemy |
| enemy | 敌人 | opposing combatant |
| combatant | 战斗员 | any battlefield unit with combat stats, ally or enemy |
| party | 出战队伍 | active/battle party subset of the force |
| force | 军团（我方部队） | the whole player Force; aggregate of all allies |
| roster | 名册 | persistent member/defeated list |
| follower | 随从 | map-following unit, `addNewFollower` |
| member | 成员 | generic group member |
| join | 加入 | |
| revive | 复活 | |
| defeated allies | 败北同伴 | |
| inventory | 物品栏 | item container (design sense); see note |
| item | 物品 | |
| equipment | 装备 | |
| equip / unequip | 装备 / 卸下 | |
| curse | 诅咒 | cursed equipment |
| gold | 金币 | |
| price | 价格 | |
| drop | 掉落 | enemy item drop |
| rare drop | 珍稀掉落 | |
| chest | 宝箱 | |
| reward | 奖励 | |
| EXP | EXP | keep the abbreviation |
| EXP command | EXP 指令 | battle EXP award command |
| EXP threshold | EXP 阈值 | 100-point level-up threshold |
| kill EXP | 击杀 EXP | kill-level-difference EXP |
| clamp | 钳位 | clamp/underflow/saturation bounds |
| saturation | 饱和 | byte/word carry saturation |
| underflow | 下溢 | byte underflow |
| threshold | 阈值 | |
| capacity | 容量 | inventory/roster capacity |
| deals | 特卖 | rare-item deals route |
| mithril | 秘银 | mithril weapon rows |

### D. Map, exploration, and events

| English | 中文 | Note |
| --- | --- | --- |
| map | 地图 | |
| map definition | 地图定义 | `MapDefinition` import record |
| map entry | 地图条目 | 79-map header entry |
| blockset | 图块集 | keep "blockset" glossed |
| layout | 布局 | 64x64 map layout |
| tileset | 瓦片集 | |
| tile | 瓦片 | map tile |
| block | 图块 | 3x3-tile block |
| palette | 调色板 | |
| area | 区域 | area description records |
| zone | 区域（事件） | zone events; keep "zone" where ambiguous |
| event | 事件 | |
| entity | 实体 | map entity |
| player entity | 玩家实体 | controlled entity |
| trigger | 触发 | |
| step event | 踩踏事件 | |
| roof event | 屋顶事件 | |
| warp | 传送 | warp-style transition |
| chest event | 宝箱事件 | |
| item event | 物品事件 | |
| setup | 配置 | map setup definition; six-pointer setup |
| setup route | 配置路由 | ordered default/flag route |
| dispatch | 分发 | first-match dispatch |
| first-match | 首匹配 | first matching record wins |
| last-set-flag-wins | 最后设置标志胜出 | setup selection rule |
| reachability | 可达性 | normal-story reachability |
| collision | 碰撞 | |
| pathfinding consumer | 寻路消费者 | collision/pathfinding effects |
| camera | 摄像机 | camera control |
| view target | 视野目标 | camera destination |
| scroll | 滚动 | |
| fade | 淡入淡出 | fade in/out |
| exploration | 探索 | map exploration mode |
| map switch | 地图切换 | |
| savepoint | 存档点 | |
| working layout | 工作布局 | mutable layout copy |
| layout copy | 布局复制 | block-copy command |
| area description | 区域描述 | `d6`-conditioned description path |

### E. Scripts, programs, and command forms

| English | 中文 | Note |
| --- | --- | --- |
| script | 脚本 | |
| map-script | 地图脚本 | the map-script engine |
| command | 命令 | |
| command form | 命令形式 | source-named macro form |
| macro | 宏 | source macro |
| handler | 处理器 | named handler section; may keep "handler" |
| dispatcher | 分发器 | slot dispatcher |
| opcode | 操作码 | |
| operand | 操作数 | |
| operand width | 操作数宽度 | byte/word/long |
| program | 程序 | map-script program |
| program row | 程序行 | zero-inclusive 304-row corpus row |
| label | 标签 | |
| stream | 流 | source byte stream |
| stream offset | 流偏移 | |
| site | 位置 | program site / use-site |
| use-site | 使用位置 | parsed use-site record |
| invocation | 调用（次数） | command invocation count |
| corpus | 语料 | complete command corpus |
| cursor | 游标 | script stream cursor (A6); distinct from UI cursor |
| inline | 内联 | inline payload/program |
| jump | 跳转 | |
| branch | 分支 | conditional branch |
| subroutine call | 子程序调用 | 68000 subroutine call |
| direct call | 直接调用 | |
| caller | 调用方 | |
| effective target | 有效目标 | resolved alias target |
| instruction target | 指令目标 | direct jump-interface target |
| alias | 别名 | jump-interface alias |
| jump interface | 跳转接口 | `j_` stub |
| terminator | 终止符 | `csc_end` / `$8080` terminator |
| sentinel | 哨兵值 | `$FFFF` sentinel |
| payload | 载荷 | embedded payload |
| physical width | 物理宽度 | stored byte width |
| zero-inclusive | 含零计数 | complete totals including zero rows |
| shared tail | 共享尾部 | shared handler tail |
| handler-local | 处理器局部 | handler-local observation boundary |
| callback | 回调 | H3 observer callback |
| shim | 垫片（shim） | H3 service shim |

### F. UI, menus, services, and save

| English | 中文 | Note |
| --- | --- | --- |
| menu | 菜单 | |
| diamond menu | 菱形菜单 | action/selection diamond menu |
| main menu | 主菜单 | seven-icon main-menu payload |
| page | 菜单页 | witch-menu page |
| witch | 女巫 | witch save/load/copy/delete screen |
| shop | 商店 | |
| church | 教堂 | |
| caravan | 车队 | matches `docs/README.md`; the traveling caravan service |
| blacksmith | 铁匠 | |
| depot | 仓库 | caravan depot |
| repair | 修理 | |
| buy / sell | 购买 / 出售 | |
| raise | 复活（教堂） | church raise action |
| cure | 治疗（教堂） | church cure action |
| promote | 转职（教堂） | church promote action |
| save | 存档 | |
| load | 读档 | |
| copy | 复制 | slot copy |
| delete | 删除 | slot delete |
| slot | 槽位 | two-slot SRAM |
| occupied flag | 占用标志 | save-flag bit |
| checksum | 校验和 | additive low-byte checksum |
| signature | 签名 | SRAM signature |
| SRAM | SRAM | keep "SRAM" |
| window | 窗口 | eight-slot window system |
| textbox | 文本框 | |
| dialogue | 对话 | |
| portrait | 立绘 | |
| speech | 台词 | speech SFX/property |
| text line | 文本行 | declared text-line ID |
| prompt | 提示 | yes/no prompt |
| suspend | 挂起 | suspend save/state |
| cursor | 光标 | UI cursor; distinct from script stream cursor |
| highlight | 高亮 | highlight mask/copy |
| icon | 图标 | menu icon payload |
| layout allocation | 布局分配 | window layout allocation |
| packed coordinate | 压缩坐标 | packed X/Y addressing |

### G. System services: input, window, randomness, interrupts

| English | 中文 | Note |
| --- | --- | --- |
| input | 输入 | |
| controller | 控制器 | |
| button state | 按键状态 | |
| raw sampling | 原始采样 | two-port sampling |
| repeat | 输入重复 | repeat filtering cadence |
| initial delay | 初始延迟 | 24-frame delay |
| wait helper | 等待辅助 | input wait helper |
| VInt | VInt | vertical interrupt; keep "VInt" |
| DMA | DMA | keep "DMA" |
| interrupt | 中断 | |
| trap | trap（系统调用） | keep "trap" |
| handshake | 握手 | wait/sleep handshake |
| RNG | RNG | keep "RNG"; random number generator |
| seed | 种子 | |
| random seed copy | 随机种子副本 | `RANDOM_SEED_COPY` |
| range | 范围 | bounded sampling range |
| debug override | 调试覆盖 | debug direction override |
| low-byte range | 低字节范围 | |
| retry | 重试 | range rejection retry |
| timer | 计时器 | |
| frame | 帧 | |
| timing | 时序 | hardware/presentation timing |

### H. Project and process vocabulary

| English | 中文 | Note |
| --- | --- | --- |
| slice | 切片 | one bounded evidence slice |
| lane | 车道 | research/design lane |
| worktree | worktree（工作树） | isolated Git worktree |
| topic branch | 主题分支 | short-lived topic branch |
| milestone | 里程碑 | |
| matrix | 矩阵 | grouped runtime case table |
| case | 用例 | one runtime scenario |
| scenario | 场景 | scripted scenario |
| harness | 测试台（harness） | BizHawk runtime harness |
| observer | 观察器 | Lua observer script |
| replay | 回放 | command/state replay |
| static extraction | 静态提取 | |
| runtime observation | 运行时观察 | H3 evidence |
| inventory | 清单 | source inventory (research sense); distinct from 物品栏 |
| audit | 审计 | cross-owner adversarial audit |
| consumer | 使用方（消费者） | |
| owner | 所有者 | evidence owner |
| index | 索引 | research index |
| manifest | 清单文件 | tracked manifest |
| report | 报告 | generated report |
| derived | 派生 | derived field/value |
| mutation | 变更 | mutation order/guard |
| mutation guard | 变更守卫 | smallest-scope mutation test |
| boundary | 边界 | contract boundary |
| domain | 域 | declared target domain |
| comment | 注释 | source comment |
| exclude | 排除 | explicit exclusion |

### I. Proper nouns (suggestions, to be finalized per entry under R4)

Chinese renderings here are **suggestions for first-occurrence annotations**. The body keeps the
English name. Entries marked 待定 await a community-standard rendering decided in the batch review.

| English | 中文（建议） | Note |
| --- | --- | --- |
| Shining Force II | 光明力量 II | game title |
| Bowie | 波伊 | protagonist |
| Kazin | 卡辛 | |
| Slade | 史雷德 | |
| Karna | 卡娜 | |
| Kiwi | 奇威 | |
| Taros | 塔洛斯 | battle special enemy |
| SDMN | SDMN（剑士） | base class label; keep the class code |
| MAGE | MAGE（法师） | class code |
| TORT | TORT（龟人） | class code; TORT effective-level quirk |
| Blaze | 烈焰 | fire spell |
| Heal | 治愈 | |
| Freeze | 冰冻 | |
| Bolt | 闪电 | |
| Boost | 强化 | |
| Muddle | 混乱 | confusion spell/status |
| Sleep | 沉睡 | |
| Slow | 迟缓 | |
| Desoul | 即死术 | instant-death spell |
| Spout | 汲取 | MP-absorb spell |
| Aura | 灵光 | area heal spell |
| Dispel | 驱散 | |
| Detox | 解毒 | |
| Silence | 沉默 | |
| Attack | 攻击 | buff spell; keep English to avoid clash with verb 攻击 |
| Dao | 达欧 | summon index |
| Apollo | 阿波罗 | summon index |
| Neptun | 尼普顿 | summon index |
| Atlas | 阿特拉斯 | summon index |
| EGRESS | EGRESS（逃脱） | exit spell; keep the source name |
| Angel Wing | 天使之翼 | exit item |
| Power Water | 力量之水 | 待定 |
| Church | 教堂 | covered in table F |

### J. Preserved identifier classes (rules and representative examples)

These classes are copied verbatim under R2; the examples are representative, not exhaustive.

| Class | Example | Handling |
| --- | --- | --- |
| Source symbol / memory label | `CURRENT_BATTLEACTION`, `RANDOM_SEED`, `CUTSCENE_DIALOG_INDEX`, `LEVELUP_ARGUMENTS` | unchanged |
| Source routine name | `UpdatePlayerInputs`, `WaitForPlayerInput`, `CheckSram`, `SaveGame`, `LoadGame`, `LevelUp`, `CalculateStatGain`, `UpdateCombatantStats`, `JoinForce`, `AddFollower` | unchanged |
| Source macro form | `nextSingleText`, `nextText`, `textCursor`, `hideText`, `setPos`, `setFacing`, `setDest`, `warp`, `resetMap`, `loadMapFadeIn`, `reloadMap`, `mapLoad`, `join`, `allyDefeated`, `reviveAlly`, `addNewFollower`, `setActscriptWait`, `customActscriptWait`, `entityActionsWait` | unchanged |
| Source handler label | `csc00_displaySingleTextbox`, `csc25_cloneEntity`, `csc0E_jumpIfForceMemberInList`, `csc1D_showPortrait` | unchanged |
| Fixture ID | `sf2-map-script-engine-static-v1`, `sf2-physical-damage-land-archer-v1` | unchanged |
| Fixture path | `tests/fixtures/h3/map-script-dialogue-v1.json` | unchanged, not translated |
| Numeric/byte literal | `$00`, `0xFF`, `$FFFF`, `-1`, 6-byte, 4,016 bytes | unchanged |
| Register/hardware name | `d1`, `a6`, `d7`, VDP, DMA, VInt, SRAM, Z80, 68000, Plane-A | unchanged |
| Opcode / operand width | `$19`, `$23`, `$2D`, 4/6/8-byte | unchanged |
| Verification layer / phase | `H0`–`H5`, `Phase 0`–`Phase 5`, `Layer A/B/C` | unchanged |
| Decision record | `ADR 0001`–`ADR 0007` | unchanged |
