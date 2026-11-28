# Design Localization Glossary (EN → 中文)

- Status: **accepted repository governance** for L0–L3 design prose and Chinese design documents
  under `docs/design/zh-CN/`; this document adds no original-game fact and selects no remake
  product direction.
- Initial acceptance: 2026-08-04 · Governance revision: 2026-11-27
- Owner: design-synthesis lane
- Scope: canonical English design prose under `docs/design/` and L0–L3 Chinese design documents and
  excerpts under `docs/design/zh-CN/`
- Authority: the [Authoring Language Policy](./documentation-roadmap.md#authoring-language-policy)
  in [`documentation-roadmap.md`](./documentation-roadmap.md).

## Purpose and Authority

This glossary keeps Chinese design documents readable for game designers and players while
preserving exact gameplay distinctions and Layer A technical traceability.

It governs:

- L0–L3 Chinese design prose, system themes, chapter overviews, and reviewed design excerpts under
  `docs/design/zh-CN/` (and the subset mapped to the Design Wiki);
- bilingual annotations inside English design documents where helpful.

It does **not** govern:

- `docs/research/`, `schemas/`, `manifests/`, fixtures, or source-adjacent engineering references,
  which remain English-first and source-faithful;
- code symbols, memory labels, macros, handlers, and fixture IDs, which stay verbatim under R2;
- in-game UI/script localization (`#1468`), which has its own runtime owner in
  [`remake/docs/application/localization-design.md`](../../remake/docs/application/localization-design.md).

## Terminology Rules

The rules below are normative. Term tables at the bottom distinguish **L0–L3 design prose** from
**technical/traceability contexts**. A conflict between a rule and a table resolves in favor of the
rule; a conflict between two tables resolves in favor of the more specific category.

### R1 — Evidence labels, verification layers, and evidence placement

Evidence labels must remain recognizable in every document because they gate the repository's
evidence model. Keep these fixed Chinese translations wherever an evidence label appears:

| English | 中文 | Usage |
| --- | --- | --- |
| Confirmed | 已确认 | Reproduced or directly source/runtime-backed claim |
| Inferred | 推断 | Supported interpretation not independently reproduced at the stated boundary |
| Unknown | 未知 | Explicit open question; never fill with an assumption |
| Contract | 规则 / 规格（L0–L3 正文）；合同（Layer A 合同标题与溯源语境） | Avoid awkward prose like “结果合同”; keep “合同” when naming Layer A evidence-bound contracts |
| Fixture | 测试夹具（或 fixture） | Fixture ID itself is never translated |
| Evidence owner | 证据所有者 | Owning research or contract document |
| Evidence date | 证据日期 | Actual observation date |
| Source baseline | 源码基线 | Pinned upstream commit identity |
| Traceability | 可追溯性 | Contract-to-fixture link |
| Provenance | 溯源 | Reproducible source/command/ROM trail |

Verification layers `H0`–`H5`, `Phase 0`–`Phase 5`, `Layer A/B/C`, and `ADR` numbers are identifiers
and are never translated or renumbered.

**Placement in L0–L3 design prose:**

- Gather repetitive general disclaimers (overall source baseline, static-vs-runtime scope notes,
  or chapter-wide caveats) into a compact top/bottom **依据与边界** block or a dedicated table
  column instead of repeating boilerplate disclaimers in every paragraph.
- Keep any **Unknown（未知）**, **Inferred（推断）**, or rule condition that changes what the
  player or designer can conclude **directly adjacent to the affected statement**; never bury a
  decision-changing unknown only in a footer.

### R2 — Source-faithful identifiers stay verbatim

Never invent Chinese substitutes for source symbols, routine/macro/handler labels, fixture IDs,
file paths, opcodes, registers, or contract-significant numeric literals:

- source symbols and memory labels, e.g. `CURRENT_BATTLEACTION`, `RANDOM_SEED`,
  `CUTSCENE_DIALOG_INDEX`, `LEVELUP_ARGUMENTS`, `UpdatePlayerInputs`, `CheckSram`, `LevelUp`;
- source macro and handler labels, e.g. `nextSingleText`, `setPos`, `join`, `addNewFollower`,
  `warp`, `csc00_displaySingleTextbox`;
- fixture IDs, paths, and exact flag/encounter IDs when cited as identifiers, e.g.
  `sf2-map-script-engine-static-v1`, `tests/fixtures/h3/map-script-dialogue-v1.json`, `F401`;
- numeric/byte literals and hardware names (`$00`, `0xFF`, `6-byte`, `d1`, `a6`, VDP, DMA, VInt,
  SRAM, Z80, 68000).

L0–L3 prose should avoid dragging raw symbols into ordinary gameplay sentences unless a rule
depends on that identifier; put technical references in links or trace columns. When an identifier
is cited in prose, a first-occurrence Chinese gloss is allowed (e.g. `UpdateCombatantStats（刷新战斗员派生属性）`),
without altering the identifier itself.

### R3 — Natural L0–L3 gameplay wording with exact semantic distinctions

L0–L3 design prose should read as natural Chinese game design—describing the player's situation,
choices, costs, and consequences—rather than literal translations of disassembly or audit prose.

- Prefer natural gameplay phrasing over stiff literals in L0–L3 prose (for example, use `进入条件`
  / `触发条件` instead of `准入`, `规则` / `机制规格` instead of `合同`, `条件限制` / `触发条件`
  instead of `守卫`, `实测案例` instead of `证人`).
- **Preserve gameplay-critical semantic distinctions.** Never blur distinct mechanics into one
  vague word:
  - `round` = **整轮回合 / 战斗回合**（一轮完整的敌我行动周期）
  - `turn` (individual actor turn) = **轮到某单位行动 / 个人行动回合**（队列中单个单位的一次出手窗口；正文避免生硬的“首次单次行动交接”）
  - `action` = **具体行动 / 操作**（攻击、施法、使用物品、待机、搜索等单次命令提交）
  - `joined` / `active party` / `follower` = **已加入军团** / **出战队伍** / **临时随行（随从）**
  - `held (custody)` / `equipped` = **携带（持有）** / **已装备**
- When an English term has multiple repository senses (`cursor` as UI 光标 vs script 游标;
  `inventory` as 物品栏 vs source 清单), keep the senses distinct.
- Do **not** perform blind repository-wide search/replace when adopting natural wording; apply
  revisions deliberately when each L0–L3 page is reviewed.

### R4 — Proper nouns follow accepted `#1468` game localization

Chinese L0–L3 design prose is no longer required to leave all character, spell, item, and class
names in English.

- **Use independently accepted Chinese names when available.** The accepted game name corpus under
  `#1468` (`#1504` / `#1506`, owned by
  [`remake/docs/application/localization-design.md`](../../remake/docs/application/localization-design.md#current-full-text-chinese-corpus))
  is the authority for adopted Chinese proper nouns. In Chinese L0–L3 prose, you may use an
  accepted Chinese name directly and optionally annotate the English name at first occurrence when
  helpful for cross-referencing English specs or in-game English mode (e.g., `鲍伊（Bowie）`、
  `治疗（HEAL）`).
- **Do not invent ad-hoc Chinese proper nouns.** If a proper noun has no accepted Chinese name in
  the `#1468` corpus or an explicitly reviewed glossary row, keep its English name in the body
  until localization or glossary review settles it.
- Keep class codes (e.g., `SDMN`, `MAGE`, `MMNK`, `SORC`) and source command tokens (e.g.,
  `EGRESS`) recognizable when a rule compares exact class tables or source commands; pair them with
  their accepted Chinese name where it helps readability.

### R5 — Revising glossary entries without forced whole-corpus churn

1. Propose new or revised entries in the owning design/governance slice with the English form,
   L0–L3 Chinese wording, technical sense (if different), and affected context.
2. Update an existing row rather than creating near-duplicates; record the revision date.
3. Updating `glossary.md` does **not** require a mechanical search/replace across every existing
   `docs/design/zh-CN/` file in the same PR. Existing mirrors intentionally keep their prior
   `glossarySha256` anchor in `manifests/zh-translation-index.json` until each L0–L3 page is
   reviewed and updated in its own scoped slice (such as `#2259`, `#2366`, or `#2358`).
4. Never re-anchor an unreviewed mirror just to silence `zh-meta` glossary drift.

### R6 — Chinese design document and excerpt conventions (L0–L3 scope)

- Chinese design files live under `docs/design/zh-CN/` preserving the canonical English relative
  path (e.g., `docs/design/synthesis/themes/tactical-battle-loop.md` →
  `docs/design/zh-CN/synthesis/themes/tactical-battle-loop.md`). Canonical English files remain
  the review baseline for original evidence and technical authority.
- **L0–L3 focus only:** consistent with
  [`documentation-roadmap.md#design-and-support-scope`](./documentation-roadmap.md#design-and-support-scope),
  Chinese documents and Wiki pages cover L0–L3 player-facing design, rules, choices, and
  consequences. Do **not** restore full Chinese translations of research ledgers, ROM/byte tables,
  script opcodes, fixture inventories, or verification logs—link directly to their canonical English
  owners and label them `（英文源）`.
- Both full translations and reviewed `design-excerpt` pages are supported in
  `manifests/zh-translation-index.json`. Excerpts may omit technical audit sections while retaining
  the rule's conditions, formulas, exceptions, and evidence boundaries.
- Rewrite relative links for the extra `zh-CN/` directory depth and verify link/anchor integrity.
  Prefer existing Chinese L0–L3 links for design navigation and English links for technical support.
- Preserve R2 identifiers whenever cited by a rule, keep R1 evidence labels recognizable, and
  translate reader-visible Mermaid labels (keeping embedded source symbols verbatim).
- Chinese pages are not registered in `src/sf2tool/design_contracts.py`; the canonical English file
  owns contract traceability.

### R7 — Docs-index counterpart

`docs/README.md` stays English-only and does not list zh-CN mirrors. Its Chinese reading entry is
the sibling file `docs/README.zh-CN.md`, which is outside the `zh-meta` mirror set. Each file links
to the other at the top; the English index remains the repository-wide baseline.

## Calibration Examples (L0–L3 Chinese Design Prose)

Use these three short examples to calibrate tone and precision when revising L0–L3 Chinese pages:

### 1. 行动与回合（区分 `round` / `individual turn` / `action`）

- **避免（反汇编直译腔）：**
  > 每回合建立队列后，首次单次行动交接进入该行动者的动作选择；提交动作或取消目标选择分别走不同分支。
- **推荐（自然策划语言，保留 `round` / `individual turn` / `action` 区别）：**
  > 每一整轮战斗回合开始时，系统排定敌我出手顺序。轮到某名队员行动时，可以先移动并查看可选目标；在最终确认攻击、施法、使用物品或待机这类具体行动前，取消选择不会消耗资源。

### 2. 专名与源码标识（遵循 `#1468` 已定译名，不自造、不改写标识）

- **避免（生硬中英夹杂或擅自臆造译名）：**
  > Bowie 受伤后，同伴施放 HEAL 给他回血；若某件尚未核对定名的道具叫 `Wooden Panel`，顺手自造译名写成“神秘木板”。
- **推荐（采用 `#1468` 已定译名、未核对专名保留英文、标识原样保留）：**
  > 鲍伊（Bowie）受伤后，同伴可以施放治疗（HEAL）为他回血；尚未核对定名的专名（如 `Wooden Panel`）保持英文原名，不自行创造译名。剧情标记和英文规则链接（如 `F401`、英文契约锚点）留在技术追踪列或文末链接中，原样保留不翻译。

### 3. 证据说明与未知边界（通用说明集中，关键未知紧邻结论）

- **避免（每段穿插免责咒语，或把关键未知藏到文末）：**
  > **已确认（静态）：** 玩家施放 BOOST 1 可提高防御。以上规则以所引静态条件为限，系统联系为**推断**，完整表现仍为**未知**。重施失败时仍会刷新属性。（把“未刷新状态读档行为未知”藏在文末脚注）
- **推荐（通用来源放边界栏，关键条件与未知紧跟对应规则）：**
  > 施放强化类法术可提升目标属性；但若重施失败，计数变化与属性刷新分开结算，可能暂时沿用旧的有效属性。原版携带未刷新状态读档后的行为仍为**未知**，重制版在战斗 Continue 中保留该差异直至该单位下次刷新。
  > *(文末“依据与边界”集中说明：本节原版规则基于已确认的静态与受限运行证据，详见[英文法术结算规则](contracts/battle/spell-resolution.md)。)*

## Term Tables

Where a row lists both **L0–L3 prose** and **technical/traceability** usage, use the natural form
in L0–L3 design text and reserve the technical form for Layer A references or exact-source notes.

### A. Evidence, contract, and verification vocabulary

| English | 中文（L0–L3 策划 / 技术溯源） | Note |
| --- | --- | --- |
| Confirmed / Inferred / Unknown | 已确认 / 推断 / 未知 | R1 fixed labels |
| evidence | 证据 / 依据 | |
| evidence owner | 证据所有者（技术）/ 规则来源（策划） | |
| evidence label | 证据标签 | |
| evidence-bound contract | 证据绑定合同（技术）/ 有据规则（策划） | Layer A/B boundary |
| provenance | 溯源 | source/command/ROM reproduction trail |
| contract | 规则 / 规格（L0–L3 正文）；合同（Layer A 标题与溯源） | avoid “结果合同” in prose |
| admission | 进入条件 / 触发条件（L0–L3 正文）；准入（技术/验证） | avoid “Battle 02 准入” in story prose |
| guard | 触发条件 / 限制条件（L0–L3 正文）；守卫（技术） | avoid “各自守卫” in prose |
| witness | 实测案例 / 观察记录（L0–L3 正文）；证人（技术） | |
| disposition | 处理结果 / 范围结论（L0–L3 正文）；处置（技术） | |
| fixture | 测试夹具 / fixture | ID untranslated |
| golden fixture | 黄金夹具 | exact expected-output fixture |
| schema | schema（模式） | keep "schema" |
| gate | 门禁 / 门槛（触发门槛） | verification gate vs story gate |
| verification rail | 验证轨道 | maintained H2/H3 verification surface |
| verification layer | 验证层 | H0–H5 |
| baseline | 基线 | pinned source/ROM baseline |
| canonical | 规范 / 权威 | authoritative, canonical output |
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
| round | 整轮回合 / 战斗回合 | one full battle round (all eligible combatants); distinct from individual turn |
| turn | 行动回合 / 轮到某单位行动 | one individual combatant's turn; avoid stiff “单次行动” in prose |
| action | 行动 / 操作 | committed command (Attack, Magic, Item, Stay, Search); distinct from turn/round |
| actor | 行动单位 / 行动者 | acting combatant |
| attack | 攻击 | |
| physical attack | 物理攻击 | |
| dodge | 闪避 | |
| critical | 会心一击（暴击） | |
| double attack | 二连击 | |
| counterattack | 反击 | |
| damage | 伤害 | |
| damage stage | 伤害阶段 | per-stage integer damage |
| land effect | 地形效果 / 地形防御加成 | decoded terrain land-effect value |
| terrain | 地形 | |
| movement type | 移动类型 | combatant movement classification |
| prowess | 特殊攻击概率（会心/二连/反击） | SF2 打包字节：低半字节会心率、高半字节二连/反击率 |
| spell | 法术 | |
| spell level | 法术等级 | |
| element | 元素 / 属性 | spell element, e.g. FIRE 火属性 |
| elemental resistance | 属性抗性 | two-bit packed resistance setting |
| weakness | 弱点 | resistance setting 3 for damage |
| immunity | 免疫 | resistance setting 3 for status |
| status effect | 状态效果 / 异常状态 | |
| instant death | 即死 | DESOUL-style outcome |
| MP absorption | MP 吸收 | |
| target | 目标 | |
| target selection | 目标选择 | |
| targeting list | 候选目标列表 | ordered candidate targets |
| move range | 移动范围 | |
| pathfinding | 寻路 | |
| movement cost | 移动消耗 | |
| terrain obstruction | 地形阻挡 | |
| attack position | 攻击站位 / 攻击位置 | selected attack-adjacent tile |
| turn order | 行动顺序 / 出手顺序 | |
| region activation | 区域激活 / 敌军触发区 | enemy region activation |
| battle scene | 战斗演出 | generated battle-scene command replay |
| reaction | 受击反应（演出） | HP reaction replay |
| stay / search | 待机 / 搜索 | battle menu actions |
| victory / defeat | 胜利 / 败北 | |
| battle exit | 撤退 / 退出战斗 | EGRESS/Angel Wing exit state |
| AI | AI | keep "AI" |
| commandset | AI 指令集 | AI commandset |

### C. Growth, stats, and economy

| English | 中文 | Note |
| --- | --- | --- |
| level-up | 升级 | |
| level | 等级 | |
| level cap | 等级上限 | class level cap |
| stat | 属性值 | HP/MP/ATT/DEF/AGI |
| stat gain | 属性成长值 / 属性增益 | randomized gain per level |
| stat growth | 属性成长 | growth-curve behavior |
| growth curve | 成长曲线 | |
| projection | 成长投影 | per-level projected gain |
| minimum-stat pity | 最低成长保底（补偿） | expected-minimum floor |
| refresh | 属性刷新 | derived-stat refresh |
| derived stat | 派生属性 / 当前有效属性 | current/base-derived combatant stats |
| class | 职业 | |
| class block | 职业成长块 | class growth block |
| promotion | 转职 | |
| promoted | 转职后 | promoted effective level |
| ally | 己方队员 / 伙伴 | playable force member; vs enemy |
| enemy | 敌人 | opposing combatant |
| combatant | 战斗单位 / 战斗员 | any battlefield unit with combat stats, ally or enemy |
| party | 出战队伍 | active/battle party subset of the force |
| force | 军团（我方部队） | the whole player Force; aggregate of all allies |
| roster | 名册 / 队伍名册 | persistent member/defeated list |
| follower | 临时随行 / 随从 | map-following unit, `addNewFollower`; distinct from joined/active |
| member | 成员 / 队员 | generic group member |
| join | 正式入队 / 加入 | |
| revive | 复活 | |
| defeated allies | 倒下（败北）同伴 | |
| inventory | 物品栏 / 背包 | item container (design sense); distinct from 清单 |
| item | 物品 / 道具 | |
| equipment | 装备 | |
| equip / unequip | 装备 / 卸下 | |
| custody | 持有（携带保管） | carried by a member vs equipped |
| curse | 诅咒 | cursed equipment |
| gold | 金币 | |
| price | 价格 | |
| drop | 掉落 | enemy item drop |
| rare drop | 珍稀掉落 | |
| chest | 宝箱 | |
| reward | 奖励 | |
| EXP | EXP（经验值） | may keep "EXP" |
| EXP command | EXP 指令 | battle EXP award command |
| EXP threshold | 升级经验阈值 | 100-point level-up threshold |
| kill EXP | 击杀经验 | kill-level-difference EXP |
| clamp | 数值钳位（上下限截断） | clamp/underflow/saturation bounds |
| saturation | 饱和 | byte/word carry saturation |
| underflow | 下溢 | byte underflow |
| threshold | 阈值 | |
| capacity | 容量 | inventory/roster capacity |
| deals | 特卖（稀有品回购） | rare-item deals route |
| mithril | 秘银 | mithril weapon rows |

### D. Map, exploration, and events

| English | 中文 | Note |
| --- | --- | --- |
| map | 地图 | |
| map definition | 地图定义 | `MapDefinition` import record |
| map entry | 地图条目 | 79-map header entry |
| blockset | 图块集（blockset） | |
| layout | 地图布局 | 64x64 map layout |
| tileset | 瓦片集 | |
| tile | 瓦片 / 格子 | map tile |
| block | 图块 | 3x3-tile block |
| palette | 调色板 | |
| area | 区域 | area description records |
| zone | 触发区 / 区域（事件） | zone events; keep "zone" where ambiguous |
| event | 事件 | |
| entity | 地图角色 / 实体 | map entity |
| player entity | 玩家角色 / 玩家实体 | controlled entity |
| trigger | 触发 | |
| step event | 踩踏触发事件 | |
| roof event | 屋顶显隐事件 | |
| warp | 切换地图 / 传送 | warp-style transition |
| chest event | 宝箱事件 | |
| item event | 物品交互事件 | |
| setup | 地图配置 / 场景布置 | map setup definition; six-pointer setup |
| setup route | 配置路由 | ordered default/flag route |
| dispatch | 触发分发 | first-match dispatch |
| first-match | 首条匹配优先 | first matching record wins |
| last-set-flag-wins | 后置已满足配置优先 | setup selection rule |
| reachability | 可达性 | normal-story reachability |
| collision | 碰撞 / 阻挡 | |
| pathfinding consumer | 寻路使用方 | collision/pathfinding effects |
| camera | 镜头 / 摄像机 | camera control |
| view target | 镜头目标 | camera destination |
| scroll | 边界滚屏过图 / 滚动 | |
| fade | 淡入淡出 | fade in/out |
| exploration | 探索 | map exploration mode |
| map switch | 地图切换 | |
| savepoint | 存档点 / 撤退返回点 | |
| working layout | 当前工作布局 | mutable layout copy |
| layout copy | 局部地形改写（布局复制） | block-copy command |
| area description | 调查描述（区域描述） | `d6`-conditioned description path |

### E. Scripts, programs, and command forms (technical reference contexts)

| English | 中文 | Note |
| --- | --- | --- |
| script | 脚本 | |
| map-script | 地图脚本 | the map-script engine |
| command | 命令 | |
| command form | 命令形式 | source-named macro form |
| macro | 宏 | source macro |
| handler | 处理器（handler） | named handler section |
| dispatcher | 分发器 | slot dispatcher |
| opcode | 操作码 | |
| operand | 操作数 | |
| operand width | 操作数宽度 | byte/word/long |
| program | 脚本程序 | map-script program |
| program row | 程序行 | zero-inclusive 304-row corpus row |
| label | 标签 | |
| stream | 字节流 | source byte stream |
| stream offset | 流偏移 | |
| site | 调用位置 | program site / use-site |
| use-site | 使用位置 | parsed use-site record |
| invocation | 调用（次数） | command invocation count |
| corpus | 语料 | complete command corpus |
| cursor | 游标 | script stream cursor (A6); distinct from UI 光标 |
| inline | 内联 | inline payload/program |
| jump | 跳转 | |
| branch | 分支 | conditional branch |
| subroutine call | 子程序调用 | 68000 subroutine call |
| direct call | 直接调用 | |
| caller | 调用入口 / 调用方 | prefer “触发入口” in L0–L2 prose |
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
| witch | 女巫（存档/开局界面） | witch save/load/copy/delete screen |
| shop | 商店 | |
| church | 教堂 | |
| caravan | 车队（Caravan） | traveling caravan vehicle and service |
| blacksmith | 铁匠 | |
| depot | 车队仓库 / 仓库 | caravan depot |
| repair | 修理 | |
| buy / sell | 购买 / 出售 | |
| raise | 复活（教堂） | church raise action |
| cure | 治疗异常 / 解除诅咒（教堂） | church cure action |
| promote | 转职（教堂） | church promote action |
| save | 存档 | |
| load | 读档 | |
| copy | 复制 | slot copy |
| delete | 删除 | slot delete |
| slot | 存档槽位 / 格子 | two-slot SRAM or inventory slot (disambiguate by context) |
| occupied flag | 占用标志 | save-flag bit |
| checksum | 校验和 | additive low-byte checksum |
| signature | 签名 | SRAM signature |
| SRAM | SRAM | keep "SRAM" |
| window | 窗口 | eight-slot window system |
| textbox | 对话框 / 文本框 | |
| dialogue | 对话 | |
| portrait | 立绘 / 头像 | |
| speech | 说话音效 / 台词 | speech SFX/property |
| text line | 文本行 | declared text-line ID |
| prompt | 确认提示 / 询问 | yes/no prompt |
| suspend | 战斗中断存档（挂起） | suspend save/state |
| cursor | 光标 | UI cursor; distinct from script 游标 |
| highlight | 高亮 | highlight mask/copy |
| icon | 图标 | menu icon payload |
| layout allocation | 布局分配 | window layout allocation |
| packed coordinate | 压缩坐标 | packed X/Y addressing |

### G. System services: input, window, randomness, interrupts

| English | 中文 | Note |
| --- | --- | --- |
| input | 输入 / 操作 | |
| controller | 控制器 / 手柄 | |
| button state | 按键状态 | |
| raw sampling | 原始采样 | two-port sampling |
| repeat | 连发（输入重复） | repeat filtering cadence |
| initial delay | 初始延迟 | 24-frame delay |
| wait helper | 等待辅助 | input wait helper |
| VInt | VInt | vertical interrupt; keep "VInt" |
| DMA | DMA | keep "DMA" |
| interrupt | 中断 | |
| trap | trap（系统调用） | keep "trap" |
| handshake | 握手 | wait/sleep handshake |
| RNG | 随机数（RNG） | may keep "RNG" |
| seed | 随机种子 | |
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
| slice | 切片 | one bounded task/evidence slice |
| lane | 车道 / 专题线 | research/design lane |
| worktree | worktree（工作树） | isolated Git worktree |
| topic branch | 主题分支 | short-lived topic branch |
| milestone | 里程碑 | |
| matrix | 矩阵 | grouped runtime case table |
| case | 用例 | one runtime scenario |
| scenario | 场景 / 剧本起点 | scripted scenario |
| harness | 测试台（harness） | BizHawk runtime harness |
| observer | 观察器 | Lua observer script |
| replay | 回放 | command/state replay |
| static extraction | 静态提取 | |
| runtime observation | 运行时观察 | H3 evidence |
| inventory | 清单 | source inventory (research sense); distinct from 物品栏 |
| audit | 审计 | cross-owner adversarial audit |
| consumer | 使用方（消费者） | |
| owner | 所有者 / 负责人文档 | evidence or doc owner |
| index | 索引 | research index |
| manifest | 清单文件 | tracked manifest |
| report | 报告 | generated report |
| derived | 派生 | derived field/value |
| mutation | 状态变更 | mutation order/guard |
| mutation guard | 变更守卫 | smallest-scope mutation test |
| boundary | 边界 | contract boundary |
| domain | 域 | declared target domain |
| comment | 注释 | source comment |
| exclude | 排除 | explicit exclusion |

### I. Proper nouns (historical suggestions — reconcile against `#1468` before use)

Under revised **R4**, the accepted 337-entry game name corpus under `#1468` (`#1504`/`#1506`,
see [`remake/docs/application/localization-design.md`](../../remake/docs/application/localization-design.md#current-full-text-chinese-corpus))
takes precedence for Chinese proper nouns in L0–L3 prose. The table below retains the 2026-08-04
initial suggestions for reference only; verify each entry against the accepted `#1468` corpus
before adopting it in prose, and keep unreviewed entries in English rather than inventing names.

| English | 早期建议（2026-08-04，待与 `#1468` 核对） | Note |
| --- | --- | --- |
| Shining Force II | 光明力量 II | game title |
| Bowie | 波伊 | protagonist |
| Kazin | 卡辛 | |
| Slade | 史雷德 | |
| Karna | 卡娜 | |
| Kiwi | 奇威 | |
| Taros | 塔洛斯 | battle special enemy |
| SDMN | SDMN（剑士） | base class code |
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
| Attack | Attack（攻击强化术） | buff spell; distinguish from verb 攻击 |
| Dao | 达欧 | summon index |
| Apollo | 阿波罗 | summon index |
| Neptun | 尼普顿 | summon index |
| Atlas | 阿特拉斯 | summon index |
| EGRESS | EGRESS（逃脱） | exit spell; keep source token where rule cites it |
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
| Decision record | `ADR 0001`–`ADR 0019` | unchanged |
