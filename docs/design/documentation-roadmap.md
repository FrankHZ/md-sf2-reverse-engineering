# 文档路线图与治理边界

- 状态：**Confirmed 的仓库治理说明**；它不是原版游戏证据，也不选择重制引擎、产品、平台或商业方向。
- 记录日期：2026-08-01
- 范围：把有来源的合同组织成简洁的玩家向说明，不改变其证据标签；未来重制选择仍须进入明确
  的 decision 与 H4 验收边界。

## 三层边界

| 层 | 归属与允许内容 | 禁止的捷径 |
| --- | --- | --- |
| A. 原版行为/数据合同 | `docs/research/`、`schemas/`、manifests、H2/H3 fixtures 与有来源的设计合同记录 **Confirmed**、**Inferred**、**Unknown** 事实。 | 源宏、字段或符号名不自动等于玩家可见含义；保留原标签，另行说明有证据支持的解释。 |
| B. 重建设计说明 | `docs/design/` 可解释 A 层支持的玩家向后果，链接本地 research/fixture owner，并把缺口列为 **Unknown**。 | 不得把静态 call、地址或看似合理的读法升级为原版行为、战役结论或玩家体验事实。 |
| C. 未来重制决定 | 现代化、实现意图或产品选择属于明确的 decision 与独立的 expected-deviation/H4 验收边界。 | 不得把现代化重写为原版行为，或从综合文档中选择引擎/产品方向。 |

**Confirmed 仓库规则：**B 层是已接受 A 层证据的可追溯解释，不是第二套证据系统。**Inferred**
解释保留标签，**Unknown** 行为保留为问题而非补写成叙事。

## 当前基线与近期综合

**Confirmed 仓库基线：**现有合同覆盖 combat、maps、level-up、spells、services、save/input/window、
dialogue、party/roster state 与 randomness，均已列入[设计索引](../README.md#design)，并追溯到
research 与 fixture owner。本路线图不合并或替换这些合同。

下列是 **Inferred 的规划优先级**，不是其中设计结论已经成立的声称。每个综合文档在所列 evidence
owner 稳定前必须保持增量性质。

| 顺序 | 候选文档 | 范围与前提 | 现有合同/证据链接 | 非目标 / 停止条件 |
| ---: | --- | --- | --- | --- |
| 1 | gameplay overview | 说明当前可支持的 player actions、state boundary 与主要 subsystem handoff；仅从已接受的 map、dialogue、roster、service、input 事实开始。 | [map exploration](./map-exploration.md)、[dialogue](./dialogue-system.md)、[party/roster](./party-roster-state.md)、[services](./service-interactions.md)、[map-script fixture](../../tests/fixtures/h2/map-script-engine-static-v1.json) | 不承诺完整战役流、界面手感或 narrative experience；它们仍为 **Unknown**。 |
| 2 | tactical battle loop | 说明从 player input/control 经 battle action、resolution、state replay 到已知 outcome 的有界顺序，并保留每条未解分支；需要已接受的 battle-loop/action/AI research 与 combat/spell 合同。 | [battle-loop research](../research/battle-loop.md)、[battle-actions research](../research/battle-actions.md)、[combat](./combat-resolution.md)、[spell resolution](./spell-resolution.md)、[physical-damage fixture](../../tests/fixtures/h3/physical-damage-v1.json) | 不从孤立案例发明 tactics、balance intent、target-selection meaning 或通用 simulation。 |
| 3 | progression and economy | 仅在 input、output、order 与 persistence 有证据时，连接 growth、EXP/gold/item/service 边界为资源流说明。 | [ally-growth research](../research/ally-growth.md)、[common-stats research](../research/common-stats.md)、[level-up](./level-up.md)、[services](./service-interactions.md)、[level-up fixture](../../tests/fixtures/h3/level-up-boundaries-v1.json) | 不声称预期难度曲线、价格、最优 build 或长期 economy。 |
| 4 | story progression | 把 Confirmed 的 state/route/dialogue/roster boundary 说明为可追溯的 progression map，并保留 normal-story reachability 与 presentation 标签；因后二者最不稳定，置于前述文档之后。 | [gameflow research](../research/gameflow-core.md)、[common-scripting research](../research/common-scripting.md)、[dialogue](./dialogue-system.md)、[party/roster](./party-roster-state.md)、[dialogue runtime fixture](../../tests/fixtures/h3/map-script-dialogue-v1.json) | 不从 source label 或孤立 program reference 重建 plot beat、player choice consequence 或完整 story route。 |

该顺序先建立读者导航，再处理最有界的 tactical loop、相连的资源流，最后处理依赖 reachability 的
story 说明。当 active slice 正在改写 owner contract，或答案大多仍为 **Unknown** 时，文档等待。

## 长期方向

下列是 **Unknown 的未来方向**，不是当前承诺。只有 entry criteria 能引用已接受的本地 evidence 时才可
启动；任一项均不授权新的 engine design。

| 方向 | 进入条件与证据依赖 | 非目标 |
| --- | --- | --- |
| map-design principles | 有文档化 map corpus、route/event/area evidence，且有足够的 reachability 与 interaction outcome 观察，可区分 layout 事实和 player-route 解释。 | 不从 64×64 layout data 单独推断作者意图或重设计关卡。 |
| player roster choice space | 已接受的 roster、class/promotion、growth、equipment、battle-party 与 persistence/capacity boundary；未解 lifecycle limit 仍须可见。 | 不发布 tier list、“best party”建议或假设的玩家偏好。 |
| player/enemy numerical curves | 完整的 source-backed numeric table，加上 runtime-confirmed application、cap 与 level/encounter context，足以命名 unit 和 boundary。 | 不设定重制 balance target，也不把数学曲线称为预期难度。 |
| battle simulation | 完整且彼此兼容的 battle-loop/action/AI/pathfinding/state contract，以及有范围的 H4 adapter acceptance surface。 | 不选择 simulation architecture、不声称通用预测准确性，亦不以模型补全未解分支。 |

## 可复用的作者结构

每篇未来 `docs/design/` 综合文档可选择性采用以下结构；这是文档形状，不是并行 workspace 或强制的
完整 GDD 模板。

1. **受众与判断边界。**说明读者（researcher、fidelity implementer 或 player-facing explainer）、
   支持的判断与不支持的判断；原版声称在 source-owner 层标为 **Confirmed**、**Inferred** 或 **Unknown**。
2. **玩家动词与 action–goal alignment。**从有证据的 input、state change、outcome 开始；原 source
   label 与中性 player-action phrase 分开。没有本地证据的 player goal/meaning 是 **Inferred** 或 **Unknown**。
3. **loops、state flow 与 system dynamics。**只画有 evidence owner 的有序 transition、resource 与
   feedback relationship；保留未观察分支，不能把 control-flow graph 画成 engine architecture。
4. **evidence matrix。**每条实质记录包含 label、bounded claim、source/research owner、contract、适用时的
   fixture ID/path 与 remaining question。诸如 [runtime RNG and battle math](../research/runtime-rng-and-battle-math.md)、
   [combat fixture](../../tests/fixtures/h3/physical-damage-v1.json)、[combat contract](./combat-resolution.md)
   的本地链接即为规范 trace，不另复制 evidence ledger。
5. **original-fidelity 与 modernization。**先写 original-fidelity rule，再把 deliberate deviation 标为
   future decision 与独立 expected-deviation fixture；没有 decision 即不暗示现代化。
6. **H4 acceptance、扩展与停止条件。**列出 adapter-visible parity fact、fixture consumer 与继续扩展所需
   evidence；当缺口属于 runtime、reachability、presentation 或 product question 时停止，不静默扩张合同。

## 外部参考的 provenance 与选择性采用

**Confirmed 外部参考 provenance：**[DY-2026/GameDesignOS README](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/README.md)、
[contract catalog](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/README.md) 与
[player-promise contract schema](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/player-promise-contract.schema.json)
于 2026-08-01 在固定 `main` commit `d01dfebc6eac7a619b9a18f3cbafa51270d1edba` 访问；其许可证为
[MIT](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/LICENSE)。复现命令
`git ls-remote https://github.com/DY-2026/GameDesignOS.git` 观察到该 commit 为 `refs/heads/main`；对所列
pinned raw document/template 的请求均返回 HTTP 200。

选择性采用下列结构提示：[player-verb inventory](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/player-verb-inventory.md)、
[system-dynamics map](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/system-dynamics-map.md)、
[game-dissection report](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-experience-analyzer/templates/game-dissection-report.md)、
[full design brief](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/full-design-brief.md) 与
[reference-game boundary](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/reference-game-boundary.md)：
reader/action scope、visible uncertainty、loop mapping、evidence link、scope gate 与 validation condition。
它们已适配本仓库的 evidence label 与 H4 boundary，未复制模板文本。

明确拒绝该外部项目的九目录 workspace、commercial pitch/market assumption 与第二套 evidence/decision
system。本仓库已分别拥有 `docs/research/`、`docs/design/`、`docs/decisions/`、`schemas/`、
manifests/research-index、H2/H3 fixtures 与 H4 acceptance boundary。外部参考只是选择性借用的作者视角，
不是项目依赖或新的 source of truth。

## 协作与后续 hygiene

**Confirmed 协作规则：**在 accepted evidence 基础上可在 reverse-engineering 持续期间增加综合文档；
不得与 active worker 并行重写某 subsystem contract。未来 finding 改变结论时，同时更新 owning research
note、fixture/contract 与 design explanation，使 trace 双向保持一致。

**Confirmed 仓库 hygiene closure：**[`party-roster-state.md`](./party-roster-state.md) 现已在
`src/sf2tool/design_contracts.py` 注册其 H2 map-script 与 H3 active-party fixture；公共 tracked-input
gate 会验证文档、fixture path 和 fixture ID 的双向 trace。这项 closure 不改变任何原版游戏 finding。
