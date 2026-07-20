# Shining Force II Reverse Engineering & Remake

这是一个从原版 Mega Drive/Genesis ROM 出发，系统拆解 **Shining Force II**，形成可验证的
逆向研究、游戏设计文档和结构化数据，并最终在现代引擎中重建其玩法的长期工程。

仓库本身是项目的耐久记录；本项目已停用外部 agent memory，也不依赖旧聊天才能继续。接续工作先读本页、
[`docs/research/source-coverage.md`](./docs/research/source-coverage.md)、
[`docs/README.md`](./docs/README.md) 和当前主题文档，再检查 `git status` 与近期提交；详细结论、
未知项、复现命令和当前 frontier 必须回写到各自的仓库所有者。

项目已完成 **Phase 1：可复现原版基线**，正在推进 **Phase 2：发现与数据合同**。本地环境
已经固定 ROM 身份、社区反汇编提交和工具 hash，能非交互地重建出逐字节一致的原版 ROM，
并已完成角色槽位、职业、物品、法术、转职、敌人定义与 Battle 01 scene 的 source/ROM 双路径
H2、成长曲线与法术学习合同、battle AI、battlefield、battle-loop、顶层 battle control、battle actions、shared battle functions、battle scene 根引擎/动画实现、battle cutscene、startup/main-loop/exploration、special screens、ROM header/window/debug、common scripting、common maps、common stats、common menus、technical interrupts/graphics/interfaces/services 全目录 inventory 和静态决策/生命周期合同，以及基础/调试覆盖 RNG、成长计算/完整升级（含投影后成长、
职业等级上限、继承法术升级与战斗 EXP 自然升级入口）、完整击杀 EXP 等级差矩阵、行动顺序、区域激活、物理伤害计算链和
BLAZE 2 四档 FIRE 抗性矩阵、DAO/APOLLO/NEPTUN/ATLAS 四索引 target-count division、攻击法术 EXP、HEAL 1、SLEEP/SLOW 1 四档 STATUS 抗性、DESOUL 四档成败/多目标 kill reward、SPOIT 空/截断/不截断及施法者满 MP 边界矩阵、BOOST 1 首次/重施回放、DISPEL 1、SILENCE 施法门、敌人物品稀有/必掉/重复 flag、四种临时状态的回合后过期/继续/属性刷新、MUDDLE confusion 谓词与双边行动保护矩阵，以及单次启动 14 case 的 AI 最终行动/目标选择矩阵、5 case 的战场移动边界矩阵和 13 case/20 tick 的地图实体移动矩阵的整机运行时 H3；尚未下载外部补丁、选择现代重制引擎或开始
重制实现。实现无关的物理战斗、法术伤害与升级成长合同已经落地，并直接绑定现有 H3 fixture，供未来
H4 复用。

截至 2026-07-19，研究索引有 1,535 条 confirmed finding、74 个 H2 fixture、59 个 H3 fixture
和 2,103 个地址绑定；其中 1,498 条由 68000 H1 listing 校验，37 条由独立 Z80 music-bank
地址域校验。music rail 还静态闭合 29 个 byte-emitting macro、39 个 song entry、321 个 channel
label 和 39,290 次 macro invocation，并将 `F8`–`FF`/`F0`/`70` 接到五个 YM/PSG parser 与共享
loop state machine；播放时序仍留在集中运行矩阵。
按固定上游的 387 个 `disasm/code` ASM 文件作严格分母，已有可执行证据触达 381 个文件，即
**98.45% code-file reach**；其余 6 个均为已由 H2 盘点的 alternate、unlabeled 或独立 Z80 build
例外。这不是行/函数覆盖率，也不表示这些文件已全部理解。数据侧已开始按完整目录推进：
`data/battles/global` 的 18 个文件已全部盘点，其中 17 个原版布局文件拥有 H1 地址绑定，唯一
例外是未编入构建的旧版零表；`data/stats/allies` 的 42 个直接/传递 include 文件也已全部绑定。
items/spells/enemies 的 19 个布局文件也已完整盘点；其中 8 个物品辅助表进一步形成 9 段
source/H1/ROM parity 与完整 canonical catalog。battle cutscene 数据
则完成 61/61 H2 inventory 和 59 个真实 H1 绑定；45 个 battle spriteset 与其指针容器也已
达到 46/46 真实 H1 绑定；battle cutscene 路由与 terrain 容器再新增 7 个真实绑定和 1 个明确
alternate。完整 `data/maps` 树的 1,390 个 ASM 已全部进入 H2 构建图盘点，其中 727 个文件拥有
文件内全局符号和真实 H1 地址；graphics/scripting/tech/sprite-dialogue 的 65 文件边界也已完成
H2，其中 63 个拥有真实 H1 绑定。最后 41 个 Z80 music ASM 也完成 include 图与两组 32 KiB
bank/ROM 字节一致性验证；地图侧还进一步确认 64 个 setup map row、66 个 flag variant、
last-set-flag-wins 选择规则，以及 126 张六指针 setup table/756 个 slot 的 ROM parity；
地图 entity 层又闭合 125 个 source、980 条物理记录和 9 个跨文件 fallthrough（其中 map 17
复用七条 variant suffix）；这些记录使用 113 个 map-sprite ID，且全部排除 237-250 的
sentinel/unbacked 区间。后续 81 个脚本 sprite 赋值、5 个实际写入点、20 个
`UpdateEntityProperties` caller 以及 ally/enemy 派生表也已全部分类，原版 built domain 同样不会
写入 237-250；完整 entity-action surface 也已闭合三个共享 source 和 75 个分散式 source。
共享部分包含 118 labels、732 commands、38 条 relative branch 和 2,864 bytes；分散部分把
361 个内嵌程序和 11 个连续 standalone ROM 区段中的 1,472 条命令全部唯一归属，17 个 `eas_*`
入口全部有源码引用，5,684 action bytes、14 条 branch 和 364 条 jump 的目标全部解析。共享部分
对全部 code/data ASM 的引用图覆盖 230 个 source，61 个入口中只有 `eas_ShrinkDisappear` 没有
外部引用。80-slot dispatcher 也已闭合为 37 个 filler slot 和 43 个真实 handler；44 个宏中
`ac_end` 是不进入 dispatcher 的 `$8080` 内嵌终止词，其余 43 个运行时宏映射到 40 个 opcode，
另有三个无命名宏的条件/随机 branch handler。entity/zone/item event 层继续闭合 263 个 source、1,134 条物理记录和
1,451 次 setup-level record reference，并保留两个 direct-`rts` stub 与 map 44 错误指针例外。
description 层也闭合 75 个 callable target、37 个 wrapper、38 个空 stub 和 227 条物理 entry，并从
唯一正常调用链确认三个 `d6` 条件函数会被跳过；init 层闭合 84 个 source、90 个 callable entry、
597 条物理 statement 和 80 次 script 调用；最后 47 个 standalone setup script 也闭合为 178 个
全局标签、8,058 条语句和 146 次跨文件引用。
与 entity-action 相邻的 map-script engine 也已从入口形状提升为完整静态合同：90 个 dispatcher
slot 包含 82 个有效 opcode 和 8 个 filler，归并为 83 个唯一 handler；宏层闭合 82 个主命令、
8 个 alias 以及 sleep/source-nop/terminator 三种特殊编码。完整 code/data 源码的 169 个文件共
使用这些宏 13,515 次；93 个受跟踪宏中 82 个出现、11 个未出现，handler 的 955 条语句、16 个
实体字段、25 个全局状态和 62 个 direct-call target 也已结构化。90-word jump table 与输入 ROM
逐字节一致。82 个主命令进一步闭合 133 个逻辑参数/operand field 和 234 个 operand bytes；
2/4/6/8-byte 命令分别有 17/27/24/14 个，且 `defineShorthand.w` 也计入物理宽度。脚本指针流
归并为 77 个顺序 handler、1 个绝对跳转、4 个条件绝对跳转和 1 个内嵌 action-program；剧情
分支可达性、跨 command 等待时序和 palette/VDP presentation 仍进入集中模拟队列。
完整调用面又进一步唯一归入 304 个 program/348 个 label：303 个由 `csc_end` 终止，唯一的
`cs_5DE22` 以绝对 jump 终止。62 条脚本跳转全部解析为 42 条同 program 和 20 条跨 program，
122 条 `executeSubroutine` 全部解析到 68000 symbol；61,020 bytes 是宏自身发出的 map-command
编码，不混入内嵌 entity-action payload。296 个 program 有 H1 地址，另 8 个 source-only
边界被显式保留，其中 7 个没有入口标签。动态 story flag 组合仍不从静态 target 图猜测。
全 2,077 个 code/data ASM 的 token 引用图进一步证明 297/304 program 有源码引用：187 个存在
跨文件引用，110 个仅有同文件引用，7 个零引用；347/348 label 被引用，唯一零引用 label 是
未装配的 `rbcs_battle01`。这只是静态可达上界，不等同正常存档下的剧情可达性。
剧情状态面也已结构化：51 条条件读取只涉及 flag 6/8/29/71/76/89；53 条直接写入、22 条
yes/no 结果写入和 20 条 battle-unlock 写入覆盖 56 个 flag、89 个 program，读写域仅在
71/76/89 相交。`yesNo` 的返回 0=set/非零=clear 与 `setStoryFlag n` 写 `400+n` 都由 handler
源码绑定；实际存档路径和持久化顺序仍不从静态图推断。
因此 **1,690/1,690 data ASM 已全部进入 H2 inventory**。domain-aware indexed
data-file reach 为 1,017/1,690（60.18%）：980 个文件由 68000 H1 符号绑定，另 37 个 song source
由 Z80 pointer table、bank-relative 地址和 ROM 物理偏移双重绑定。H2 的 25 个 ROM
table range 另覆盖核心角色/职业/物品/法术/敌人/成长、物品辅助表、sprite-dialogue 与 Battle 01 数据。完整口径、空白子系统和
复现命令见 [`docs/research/source-coverage.md`](./docs/research/source-coverage.md)。

## 我们要交付什么

最终目标不是“能把 ROM 拆开看看”，而是形成一条可持续生产新 deliverable 的流水线：

1. 可复现地识别、拆分并重建原版 ROM；
2. 逐系统解释代码、数据格式、状态机和运行时行为；
3. 将角色、职业、物品、法术、地图、战斗、剧情脚本等导出为有 schema 的规范数据；
4. 以证据为基础编写实现无关的游戏设计文档；
5. 用同一组行为 fixture 验证现代引擎实现；
6. 明确区分原版事实、推断、未知项和有意的现代化改动。

这里不追求开发一台新的 Mega Drive 模拟器，也不以制作 ROM hack 为最终目标。ROM hack、
社区补丁和编辑器可以作为对照实验，但重制版应拥有独立可维护的代码、数据合同和可分发
资源边界。

## 当前输入基线

2026-07-17 对本地私有输入 `local/roms/sf2-us.bin` 的只读检查结果：

| 字段 | 值 |
| --- | --- |
| 大小 | 2,097,152 bytes（2 MiB） |
| SHA-256 | `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` |
| SHA-1 | `22DEFC2E8E6C1DBB20421B906796538725B3D893` |
| MD5 | `6473B1505334EF5620D13191C18251FE` |
| 主机标识 | `SEGA GENESIS` |
| 标题 | `SHINING FORCE 2` |
| 产品代码 | `GM MK-1315 -00` |
| 区域 | `U` |
| ROM 内置/重算校验和 | `0x8921` / `0x8921` |

SHA-1 与 MD5 和公开登记的 USA 版本一致，可对照
[TASVideos 的版本记录](https://tasvideos.org/615G)和
[RetroAchievements 的受支持哈希](https://retroachievements.org/game/75/hashes)。哈希匹配只说明
输入身份，不代表我们可以重新分发 ROM。

ROM 已移动到被 Git 忽略的 `local/` 工作区，移动前后 SHA-256 一致。Git 已初始化，
`.gitignore` 同时提供目录级隔离和 ROM/patch/save-state 扩展名的第二层防护。任何工具都不得
原地改写这份基准文件。

## 已发现的上游资料

最重要的发现是
[`ShiningForceCentral/SF2DISASM`](https://github.com/ShiningForceCentral/SF2DISASM)：社区已经将
游戏组织成 68000 汇编和拆分数据，并提供 split/build 工具。其 `master` 分支目标是在提供
原版美版 ROM 后重建出 bit-perfect ROM；`build/standard` 则组合了社区修复和功能。原版基线
已固定到 `master` 的 `c834c652b6862bc5679fd7f69a38a7093206efc6`，而不是浮动默认分支。

同一组织还维护
[`SF2JavaToolSuite`](https://github.com/ShiningForceCentral/SF2JavaToolSuite)及地图、文本、图标、
声音等专项工具。上游说明这些 Java 工具需要 Java 17 或更高版本；项目已按官方 checksum
固定本地 Eclipse Temurin `17.0.19+10`，不修改系统原有 Java 15。当前只下载了直接支持 Phase 1
的 SF2DISASM checkout 和本地 JDK，没有下载社区补丁、pre-patched ROM 或其他编辑器仓库。

其他资料入口：

- [SF2 Disassembly Super Thread](https://forum.shiningforcecentral.com/viewtopic.php?t=41303)：项目历史、
  分支用途、工具和专题研究索引；
- [MegaDrive Development Wiki](https://wiki.megadrive.org/index.php?title=Main_Page)：ROM header、
  68000、VDP、Z80/音频等硬件主题索引；
- [Genesis Software Manual](https://nemesis.hacking-cult.org/MegaDrive/Documentation/GenesisSoftwareManual.pdf)：
  Mega Drive 软件开发硬件参考；
- [FantasyAnime 的 SF2 下载页](https://fantasyanime.com/shiningforce/sf2down.htm)：可用于盘点社区补丁
  和改版，不作为 ROM 获取来源。

固定提交中没有找到 LICENSE、NOTICE 或 COPYING 文件，因此 checkout 只保留在 ignored 的
`local/upstream/` 中；在许可证核清之前不把其代码 vendoring 到本仓库，也不假定可以重新
授权。外部补丁同样只下载当前实验确实需要的 patch-only 文件；不下载 pre-patched ROM，
并记录来源、版本和预期 base hash。

## 工程分层

目标目录会随真实工作逐步创建，而不是一次性铺满空文件夹：

```text
AGENTS.md              agent 工作边界与完成标准
README.md              项目合同、现状和路线
docs/
  research/            有来源、地址、符号和复现实验的逆向结论
  design/              从证据提炼的实现无关设计文档
  decisions/           引擎、格式和工具链等架构决策
schemas/               规范化导出数据的 schema
manifests/              ROM、工具链、提取布局与逆向研究关系索引
src/sf2tool/            Python CLI、验证器、提取器与 harness 主实现
tools/                 盘点、提取、转换、差分和报告工具
scripts/               迁移期间冻结的 H1-H3 PowerShell 兼容层
tests/fixtures/        小型、可再分发的元数据与行为期望
tests/python/          Python 单元与合同测试
remake/                现代引擎工程（合同稳定后创建）
local/                 被 Git 忽略的 ROM、补丁、上游 checkout 和生成物
```

`local/`、ROM、存档、trace、录屏、拆出的原始文本/图像/音乐以及重建 ROM 均不得提交。可提交的
golden 数据应尽量是地址、数值、短结构、哈希和状态转换，而不是原始版权内容。

## Harness：项目的主干

依赖由 `uv` 和提交的 `uv.lock` 固定。首次同步、日常提交与完整里程碑验证入口为：

```powershell
uv sync --locked
uv run sf2 verify
uv run sf2 verify --full
```

日常提交默认只跑 `verify`（Ruff、pytest、设计合同、研究索引、ROM 身份和工具链来源）以及本次
改动直接拥有的窄 rail，例如 `uv run sf2 h3 battle-exp`。十分钟以上的 `verify --full` 只在阶段
里程碑、准备合并/发布、共享 harness 或兼容层发生变化，以及明确要求全量 parity 时运行。

统一入口已经覆盖逆向关系索引、H0、toolchain provenance、H1、静态表双路径 parity 与成长合同 H2，以及
固定 BizHawk/Genesis Plus GX 的基础/调试覆盖 RNG、成长计算/完整升级及投影/等级上限/法术继承边界、属性夹断、战斗 EXP 自然升级、击杀 EXP 等级差、行动顺序、区域激活、物理伤害计算链、攻击法术伤害/EXP、HEAL 1、SLEEP 1、DESOUL、SPOIT、BOOST 1、SLOW 1、DISPEL 1、SILENCE 施法门及回合后状态过期/继续 H3；后续在同一入口继续补齐：

| 层 | 验证内容 | 通过标准 |
| --- | --- | --- |
| H0 输入身份 | 文件大小、hash、ROM header、区域、内部 checksum | 与锁定 manifest 完全一致 |
| H1 原版重建 | 固定提交的 `SF2DISASM master` 执行 split/build | 输出与输入逐字节一致 |
| H2 提取确定性 | 同一输入重复导出并校验 schema | canonical 输出无差异 |
| H3 原版行为 | 固定模拟器、输入脚本和场景观察状态 | 小型状态事实/trace hash 一致 |
| H4 重制 parity | 同一行为 fixture 驱动现代实现 | 原版规则一致，偏差有显式决策 |
| H5 发布边界 | 扫描产物与 staged files | 不包含 ROM、抽取资源或不明许可证代码 |

当前重建证据、命令和限制见
[`docs/research/reproducible-original.md`](./docs/research/reproducible-original.md)，静态数据的范围、
合同和已发现漂移见 [`docs/research/static-core-data.md`](./docs/research/static-core-data.md)。首次配置
本地环境：

```powershell
uv run sf2 init --rom-path <合法持有的美版 ROM 路径>
uv run sf2 verify --full
```

每条重要设计结论都应能回到 H1-H3 的证据；每个重制系统都应能被 H4 独立测试。截图和人工
试玩可以辅助理解，但不能替代可重复的状态验证。

当前 H3 关系可通过 [`manifests/research-index.json`](./manifests/research-index.json) 查询。它把固定
上游 symbol、ROM/RAM 地址、fixture/verifier、研究文档与已接受的设计合同连成机器可读记录，并由
`uv run sf2 research-index test` 检查全部 H3 fixture 的地址绑定；使用方法和边界见
[`docs/research/indexing.md`](./docs/research/indexing.md)。

项目维护语言已经切换为 Python 3.12+，依赖和命令统一通过 `uv`。既有 H1-H3 PowerShell rails
在迁移期间只能由 `sf2tool.legacy` 调用，并被 36 文件/4,813 行的不可增长测试冻结；新增工具不得
再写 PowerShell。迁移原则见
[`docs/decisions/0002-python-and-uv-for-project-tooling.md`](./docs/decisions/0002-python-and-uv-for-project-tooling.md)。

## 研究地图

逆向按“先建立依赖，再解释上层”的顺序推进：

1. ROM 布局、启动流程、RAM map、中断、输入、存档和随机数；
2. 压缩、文本、调色板、tiles/sprites、地图和声音格式；
3. 全局 game flow、地图实体、触发器、对话和 cutscene 脚本；
4. 角色、职业、成长、物品、装备、商店、法术和状态效果；
5. 战斗地图、地形、回合/行动顺序、移动、AI、伤害、经验和胜负条件；
6. 章节内容与跨系统资源引用；
7. 可被现代引擎消费的规范数据和行为规格。

每个主题文档至少包含：范围、来源、已确认事实、推断、未知项、数据结构/流程、可复现命令、
fixture 覆盖和对现代重制的合同影响。

## 分阶段路线

### Phase 0 — Bootstrap（完成）

- 建立 `README.md` 与 `AGENTS.md`；
- 锁定输入身份、版权边界和上游候选；
- 定义 harness 层级与研究地图。

### Phase 1 — Reproducible Original（完成）

- 增加安全的 `.gitignore` 和 `local/` 工作约定后再初始化 Git；
- 生成机器可读 ROM manifest 和 H0 检查器；
- 安装/固定 Java 17+ 与必要工具版本；
- 固定 `SF2DISASM master` 提交，完成 split/build 的 bit-perfect H1；
- 记录上游许可证审计和第三方工具清单。

### Phase 2 — Discovery & Contracts（当前）

- 建立 ROM/RAM/symbol 索引和研究文档模板（当前 H3 关系索引已落地，后续随 H2/H3 扩展）；
- 按子系统先做完整静态 inventory/解析，再把静态无法判定的问题合并成 BizHawk 矩阵；
- 继续扩展角色/职业/物品/法术与战斗地图的端到端提取；
- 定义 schema、canonical serializer 和确定性测试；
- 建立模拟器选择决策与首批行为场景。

当前 H2 已覆盖 25 个 ROM table range：角色/职业/物品/法术的 281 条固定记录字段级零差异，
5 条成长曲线、59 个职业成长记录和 122 个学法术条目，以及 5 段转职表、103 个敌人名称和
103 个 56-byte 敌人定义、30 条跨 22 场战斗的敌人掉落记录，以及 103 条敌人 gold word。
drop rail 确认 flags 0-29、三个 `RNG(32)` 特例和 `0xFFFF` 终止；gold rail 还保留并拒绝误解释源码标记的
69-word unused 尾部；新增敌人/转职双路径比较 2,722 个字段、零差异；所有 canonical JSON
均有 schema、固定 hash 与重复导出验证，内容只写入 ignored 的 `local/derived/`，仓库不保存
原版名称清单。item-auxiliary rail 另把 30 家商店/235 个物品引用、128 项 debug shop、13 档宝箱
金币、25 条损坏文案映射、9 组 mithril 职业/8×4 武器候选、特殊 Caravan 描述、9 个战外可用
物品和 84 行武器图形展开成 canonical catalog；9 段共 768 bytes 全部通过 source/H1/ROM parity。
静态消费者还证明 BRN/RDBN 不直接使用第 9 个职业组，而是随机转到武器 row 0 或 2；商店剧情
准入、blacksmith 持久化/呈现与实测频率保留到集中运行时矩阵。enemy-map-sprites rail 再闭合
166-byte 表与唯一消费者：0-102 对应敌人定义，103-165 是 NPC sprite tail；627 个 spriteset
引用和全部 upgrade range 都不会进入尾部，只有无 bounds check 的 raw/debug/corrupt enemy index
仍保留为非标准边界。sprite-dialogue rail 进一步闭合 119 个四字节记录与 `0xFFFF` sentinel：
119 个 map sprite 全部唯一，80 行带 portrait、39 行返回 `PORTRAIT_NONE`，十种 speech SFX 的
完整分布和 first-match/default 返回规则均通过 source/H1/ROM parity；实际 portrait 抑制与发声
时序仍进入集中运行时队列。battle AI 静态 rail 另覆盖完整
26 文件/5,991 行源码面、82 个 global label、
388 个直接调用点、五类 action getter、四套 attack priority script、物理/法术 potential-damage
模型、4×32 class adjustment 表，healing eligibility/spell-level/item precedence 与
movetype/AOE scoring，support admission、DISPEL/MUDDLE 2 target scoring 和两个不可达
ATTACK/BOOST 2 路由、最终 action/target tie-break、move/move-order、dispatcher 与 standby rules，并将 44 个新 symbol
绑定到 H1 地址。battlefield/pathfinding 静态 rail 另覆盖该目录全部 17 个文件、2,299 行、126 个
global label 和 116 个直接调用点，并将每个文件的代表入口绑定到 H1 地址；48×48 RAM 数组、
初始化/占用规则、移动邻格准入、四层 Manhattan range、目标阵营/准入、攻击站位和 move-string
回溯、move-order、trapped-chest 和 32 桶 weighted propagation 已形成静态合同；边界行为进入同一批
集中 H3，以一次 BizHawk 启动确认 5 组传播、budget 128 桶回绕、平面跨行和边界预读结果，不拆成逐例启动。
相邻 battle-loop 静态 rail 覆盖全部 18 个文件、2,692 行、48 个 global label 和 216 个直接调用点，
并将 roster 扫描、AI memory reset、terrain 解压、spawn admission、死亡清理和战后恢复固化为合同；
顶层 battle-control rail 再覆盖 9 个文件、618 行、18 个 global label 和 60 个直接调用点，将主循环、
胜负出口、difficulty、spriteset、music/VInt 和 laser ray 固化为静态合同。两批均未增加模拟器启动；
battle-actions rail 接着覆盖 29 个文件、4,375 行、130 个 global label 和 274 个直接调用点，补齐动作
engine、物理分支顺序、item use/break、Taros 特例和目标排序。shared battle-functions rail 再覆盖
7 个文件、3,182 行、127 个 global label 和 394 个直接调用点，确认单回合控制、Kiwi 火焰、退出、
battle load 与 move SFX；随后收口 6 个 player-control/cursor/menu 入口的 9 段源码，结构化
1,039 条语句、231 个分支点、207 个直接调用点和 59 个全局状态，并固定 tile/target 确认、
battle/item menu、取消回滚与 suspend 分支；装备交换、give/drop 和 normal/trapped/gold/item
chest 的静态结果也由同一 fixture 固定。四批均未增加模拟器启动；尚未静态
闭合的问题继续积累到后续共享模拟矩阵。
新一批 battle scene 根引擎 rail 覆盖 12 个文件、6,261 行、387 个 global label 和 376 个直接调用点，
固定 21 条 scene-script 命令、初始化/资源选择顺序及 32×2 法术动画分发表；其中 11 个此前未触达
文件先将严格 reach 推到 32.30%。紧接着的动画 rail 又覆盖全部 55 个实现文件、7,919 行和 650 个
global label，将 29 个 setup 与 26 个 update 文件逐槽配对，最终把 reach 推到 46.51%。帧时序和 VDP
效果统一进入后续 presentation 模拟矩阵，两批均未启动模拟器。
最后补齐 10 个 battle cutscene 路由文件，确认 intro/completed flags、败敌/leader-death 准入和
region map-script 顺序，使 `code/gameflow/battle` 的 183 个文件全部获得逐文件 H2/H3 入口绑定，
严格全局 reach 升到 49.10%；表内容与脚本语义仍按各自后续批次计算，本批同样没有启动模拟器。
common scripting rail 随后覆盖 29 个 entity/map/text/credits 文件、11,153 行、888 个 global label，
固定 90 槽 map-script、80 槽 entity-script 和 Huffman 状态。28 个有标签文件绑定 H1；唯一 288-byte
无标签废弃 blob 由 range/byte-count 验证但不虚增 symbol reach，因此严格全局 reach 为 56.07%。
common maps rail 再覆盖全部 7 个文件、2,199 行，固定 flag-switched map、battle trigger、
egress/savepoint、8 KiB layout 输出边界和 VInt gate，使严格 reach 升到 57.88%；camera/VDP 时序
继续留在集中 presentation 队列，本批没有启动模拟器。
common stats rail 再 inventory 全部 20 个文件、5,149 行，固定 flags、party、caravan/deals、
spell learning 和 new-game 顺序。17 个文件具有独立证据；三个未被 layout include 的 alternate
item source 不借用 canonical 地址，因此严格 reach 只增 12 个至 60.98%。field-item dispatch 由
实际 layout include 的 common-menu 文件继续承接。
common menus rail 随后 inventory 42 个文件、14,827 行，其中 41 个 layout-owned 文件各自绑定
H1 地址；唯一 member-list alternate source 仅做 range/hash 对照。diamond、yes/no、number prompt、
文本控制、九组 field-item dispatch 以及五个 service menu 入口形成静态合同，使严格 reach 升到
71.58%。窗口/portrait/动画时序合并进入 UI/presentation 模拟矩阵，本批未启动模拟器。
technical interrupts rail 接着覆盖全部 21 个 layout-owned 文件、2,320 行，将 VInt 顺序、8 个
contextual slot、wait/sleep handshake、DMA queue、四种 fade、24/6 input repeat 与 trap 路由固化为
静态合同，使严格 reach 升到 77.00%。VDP/Z80 总线时序和 queue capacity 留给集中技术矩阵，本批
仍未启动模拟器。研究索引现在直接输出 code/data 唯一文件计数，防止手工覆盖率再次漂移。
technical graphics rail 再覆盖全部 11 个 layout-owned 文件、2,137 行，固定两种 decompressor
调用合同、display init、sprite links、32-frame palette transition、九槽 special-sprite routing、
parallax/autoscroll gate 和 flash script，使严格 reach 升到 79.84%。随后 Python-owned Stack decoder
闭合全部 43 个 battle-terrain payload：45 个指针槽、两个 alias、99,072 个解压字节及 source/ROM
payload parity；同一解码器随后闭合 56 槽/52 个唯一 portrait 容器、261 条 eye 与 218 条 mouth
动画元数据、palette 边界及 106,496 个解压图形字节；battle background rail 又闭合 30 槽/
27 个唯一容器、3 个 alias、27 份 32-byte palette 和 54 条 Stack stream，共 331,776 个解压字节。
battle sprite rail 随后闭合 32 个 ally 与 54 个 enemy 容器、167 份 palette 和 408 个 frame，
共验证 2,271,744 个解压字节；weapon/ground rail 再闭合 23 个 weapon stream、42 份 weapon
palette、27 个 ground header/10 个共享 ground stream，新增 203,776 个解压字节。其余
嵌套 compression corpus 和渲染帧继续归入静态批量 parity 与集中 presentation 矩阵，五个 Stack 解压
批次都没有模拟器启动。
battle-sprite animation rail 随后闭合 87 个 ally 与 121 个 enemy sequence：两张 pointer table、
208 个 payload 的 3,800 bytes 和 421 个 frame entry 全部通过 source/H1/ROM parity。ally payload
使用 8-byte header + 8-byte entry，首 entry 兼作第二 idle frame，攻击实际播放其余 147 条；enemy
使用 4-byte header + 4-byte entry并播放全部 187 条。43 条 frame 使用索引 15 保持上一画面，7 个
header 带默认 spell animation，全部 208 个 terminate flag 为零；normal/dodge/direct-special 与
KNTE/PLDN/PGNT spear remap 的 selector 规则已固化，60 Hz 呈现与 weapon placement 留给集中矩阵。
独立的 Basic decoder 随后闭合 map-sprite 表的 720 个指针槽、670 个唯一 payload 和 50 个 alias：
其中 669 个有效 payload 全部解出 576 bytes，共 385,344 bytes；最后一个 `0xFFFF` placeholder 被
9 个 free-spot 槽复用并保持为显式未解码边界。
special-sprite rail 又闭合 10 个指针槽、5 个初始 palette/container 和 1 个替代动画流，共解码
16,704 bytes。它同时证明 route threshold 虽接纳 240–255，资源表仅覆盖 index 0–9，load/update
dispatch 仅覆盖 index 0–8：原版符号引用只出现于 ID 251–255；ID 246 有 Kraken 指针但无 dispatch，
240–245 无指针，237–239 也没有符号引用。动态或编码值能否选到这些保留 ID 仍保持 Unknown。
special-screen graphics rail 随后闭合 `code/specialscreens` 消费的全部 9 条 Stack-compressed tile
stream：23,296 个压缩字节确定性解出 50,176 bytes，9 个资源和 6 个可直接寻址的 source pointer
均与 ROM 一致。8 个固定 transfer 中 3 个恰好匹配解压长度，另 5 个合计多传 27,648 bytes；这些
DMA tail 当时由何种 staging bytes 占据仍为 Unknown，已与渲染、palette、transition 和 pixel-fill
顺序一起进入集中 presentation 矩阵。
UI graphics rail 再闭合 base tiles、6 套 compressed diamond-menu tiles 和 yes/no prompt 共 8 条
Stack stream：7,848 个压缩字节解出 23,168 bytes；又补齐 4,032-byte uncompressed main-menu
payload 的 7 个 576-byte/18-tile icon。9 个资源、9 个 source pointer 与 9 槽 menu table 全部通过
source/H1/ROM parity。menu table 前 3 槽的 high-bit packed 组合分别选 `[5,1,2,4]`、`[0,1,2,3]`
和 `[0,1,2,4]`，后 6 槽才是压缩资源二级指针；icon 6 没有静态 table 引用。technical-services
ownership audit 同时证明 section 3/6/17 的 20 个 incbin 已全部归入 8 个深层 H2 owner，unowned 为 0。
icon-graphics rail 接着闭合连续的原版 icon storage：目录中有 167 个 192-byte payload，但构建只
装配 163 个（127 item、30 spell、6 other），共 31,296 bytes，并由 `p_Icons` 作为无指针表的
算术索引基址。未装配的 item 127 和 spell 16-18 明确保留为四个 source-only payload；物理槽
127/128 分别是 nothing/unarmed，槽 129 无 enum，槽 146-148 则由 Jewel of Light、Jewel of Evil
和 cracks overlay 占据，同时与 `ICON_SPELLS_START + 16..18` 碰撞。成员/商店 loader 的 192-byte
复制及四角强制色、highlight loader 的 192-byte mask/384-byte 双帧输出均形成静态合同；最终 UI
palette、DMA 与可达性进入集中 presentation matrix。
UI-layout rail 随后闭合 vanilla build 的 19 个 graphics/tech ASM owner：27 个 leaf layout、
2,394 个 VDP word、16 槽/10 个唯一目标的 spell-level pointer table、4 套 48-byte diamond border
和 4 个 direct tile payload，共 5,614 个唯一字节。全部 local label、macro 展开值、pointer 和
payload 通过 source/H1/ROM parity；window-border aggregate 与 fighter mini-status alternate
继续明确排除。运行时 tile overwrite、palette/DMA、window motion 和最终渲染进入共享
presentation matrix。
variable-width-font rail 接着闭合对话字体的静态数据流：80 个固定 32-byte glyph、256 项
ASCII-to-symbol table、唯一 longword pointer 和三个 loader/renderer 入口，共 2,820 bytes 通过
source/H1/ROM parity。glyph 是 15×12 bitplane，stored width 为 3-9，loader 对非零值再加一形成
advance；ASCII 路径只发出 78/80 个 glyph。后续 text-Huffman rail 又闭合 510-byte/255-entry
offset table 与 1,952-byte tree payload：86 棵定义树、1,536 个 leaf code 连续覆盖整个 payload，
从初始 context 254 可达全部定义树且不会落入 169 个 `$FFFF` 槽。上游说明写的 256 entries 与
实际 255 entries 的差异被显式保留；Huffman 与 ASCII 的正常输入并集仍不发出 glyph 70/71。
完整 control-code side-effect replay、非标准直接 symbol 注入，以及 overlap、palette、typewriter
timing 与 DMA 呈现，保留到共享 text-presentation matrix。随后 text-banks rail 已把语料静态部分
闭合：17 个 bank 的 79,013 source bytes/4,267 条 length-prefixed record 解出 152,679 个 symbol，
每条恰好一个 terminator 254，86 个已定义 context 全部在真实语料中出现，17-entry pointer table、
top-level pointer 和总计 79,086 parity bytes 均与 ROM 一致。control 253 在原版 corpus 中出现 0 次；
明文与逐条 symbol 只保留在 ignored 输出，仓库仅追踪聚合和 hash。
witch-menu-graphics rail 随后补齐同一 section 6 的非压缩 presentation 数据：32-byte/16-color
choice palette、960-byte bubble table、两个 longword pointer 和 `ExecuteWitchMainMenu`/
`DrawWitchMenuBubble` 消费路径，共 1,000 bytes 通过 source/H1/ROM parity。bubble 表严格拆成
4 个选项 × 3 帧 × 5×8 word；`-$5D00` 调整后 480 个 word 全为 palette 2 + priority，覆盖 60 个
tile index，选中项的 20-state phase 为 0→1→2→1。最终 CRAM/窗口运动与逐帧呈现进入共享矩阵。
special-screen-presentation rail 再闭合所有未压缩 screen palette/layout：7 套 palette 共 240 个
color word，5 个 layout 共 4,176 个 word，12 个资源合计 8,832 bytes 全部通过 H1/ROM parity。
其中 Title A/B 的 2,560-byte `vdpTile` ASM 展开还与两个编辑器 binary mirror 完全一致；压缩
tile stream 仍归既有九资源 rail，不重复计数。palette upload、layout mutation/scroll 与最终画面
继续集中进入 screen-presentation matrix。
unused-tech-assets rail 又闭合构建中保留但没有符号化消费者的最后两个技术资源：5,694-byte
cloud-named container 唯一解析为 4 条 Stack stream，每条解出 8,192 bytes/256 tiles，共 32,768
decoded bytes；64-byte base palette payload 是两套合法 16 色 palette，只在 color index 1/5
不同，其 longword pointer 同样通过 H1/ROM parity。源码 token 扫描只证明没有 symbolic consumer；
raw/computed/debug reach、四流顺序、palette/VDP 目的地和最终画面继续保持 Unknown。
battle-effect graphics rail 接着闭合 23 个 spell container、4 个 invocation container 的 15 frame/
30 stream、1 条 status animation 与 2 条 battle-transition stream，共 56 条 Stack stream、46,364
个压缩字节和 200,992 个解压字节。所有 30 个资源、4 个顶层 pointer 和 3 张 pointer table 与 ROM
一致；每条 invocation stream 解出 4,096 bytes，却固定传输 4,608 bytes，30 条流共留下 15,360-byte
staging tail，保持为集中运行时观察问题。
map-tileset rail 随后闭合 `pt_MapTilesets` 全部 115 条 Stack stream：198,514 个压缩字节统一
解出 471,040 bytes（每条固定 4,096），pointer table、payload、79 份 map header 和 32 份 animation
header 全部通过 source/H1/ROM parity。395 个普通槽含 326 个引用与 69 个 `255` 空槽；普通地图使用
100 个唯一 tileset，动画再覆盖 15 个，合并触达 114/115。只有 `MapTileset029` 没有静态引用，
动态不可达性仍保持 Unknown。
map-palette rail 紧接着闭合 `pt_MapPalettes` 全部 16 个 32-byte payload 和 79 个 map header 引用：
512 个源字节、256 个 Genesis color word、pointer table、顶层 pointer 与每张地图的 palette byte
全部通过 source/H1/ROM parity。所有 16 套 palette 都有静态地图引用；源数据中 15 套的 color 0
非零，但 `LoadMap` 在复制整套 32 bytes 后总会清零 `PALETTE_1_BASE` 首 word，因此 16 套有效
palette 的 color 0 均为零。fade、transition 与逐地图最终呈现仍合并到 presentation matrix。
全源码 compression-consumer rail 随后建立直接调用分母：23 个 code 文件共有 46 个命名调用点，
其中 35 个直接 Stack、4 个 Basic、7 个 compressed-DMA wrapper；全部映射到 12 个已有 corpus owner，
未归属调用为 0。该口径明确不声称覆盖动态间接跳转或自修改调用，后者只有出现证据时才进入运行时 trace。
technical interfaces rail 将 10 个 jump-interface 与 15 个 pointer 文件合并盘点，完整锁定 331
个 PC-relative jump stub 和 60 个 longword pointer 的 canonical mapping，使严格 reach 升到
86.30%。该结构完全由 source/H1 决定，不产生运行时问题，也没有模拟器启动。
remaining technical services rail 随后覆盖 12 个 resource/sound/SRAM/input/copy/RNG 文件、6,986
行和 550 个 global label，锁定 20 个资源入口、两槽交错 SRAM/8-bit checksum、重叠 byte copy、
双控制器扫描和独立 ASW Z80 sound-driver 构建链。11 个主 layout 文件把严格 reach 提到 88.89%；
Z80 源文件由 canonical H2 hash 覆盖，但不伪装成 68000 H1 symbol。四类硬件/时序问题继续合并排队，
本批仍未启动模拟器。
gameflow core rail 再覆盖 startup 7 文件、main loop 1 文件和 exploration 5 文件，共 3,126 行、
200 个 global label 和 176 个直接调用点。冷启动/VDP/Z80 顺序、region gate、battle/exploration
分流、六种 map event、48 实体交互扫描、区域物品与 inventory handoff 已形成静态合同，使严格
reach 提到 92.25%。reset/TMSS、intro input 和 exploration transition 继续按四组矩阵排队，本批
同样没有启动模拟器。
special-screens rail 一次覆盖 `code/specialscreens` 全部 19 个文件、3,225 行、119 个 global
label 和 18 个资源入口，固定 Sega logo/checksum/cheat、title 两段 scroll、witch 的
new/load/copy/delete、suspend reset，以及 ending pixel-fill/falling-jewels 所有权，使严格 reach
升到 97.16%。其中全部 9 条压缩 tile stream 已由独立 rail 完成 source/H1/ROM parity 和解码边界；
视觉/输入时序及 5 个 oversized transfer tail 合并成 3 个 presentation matrix，本批没有启动模拟器。
remaining-core rail 最后覆盖 ROM header、8-slot window engine 和 3 个 debug/special 文件，共
1,210 行、69 个 global label；64-vector/header、window VInt interpolation、30 人 battle test、
4 个 configuration toggle、7 路 debug action 和 4 个 hit override 均成为静态合同。严格 reach
到 381/387（98.45%），剩余 6 文件全部是明确例外，不再是未知漏项；本批仍未启动模拟器。
H3 以 7 组受控 seed 验证 `GenerateRandomNumber` 的原版 ROM 指令、RAM seed
更新和 D7 输出，并以 18 个自然启动调用验证 curve-none、两次 RNG 随机成长、返回 gain 和一次
最低成长补偿分支。完整升级 H3 进一步确认 Kazin 的普通基础职业路径，以及 Kiwi/TORT 在
`InitializeAllyStats` 和 `LevelUp` 两处被误加 20 effective levels、但本场景无学法术副作用的原版缺陷。
新增七个自然 `LevelUp` 调用确认 level 30 后改用固定 1.5 基础增益、基础职业 40/转职职业 99
上限及其前一级、WIZ 的 `level + 20` 阈值和 `$FE` 继承首职业法术表会把 Kazin 的 BLAZE 1
升级为 BLAZE 3；同时确认缺失本地职业块时会跨角色继续扫描（Peter/WIZ 借用 Tyrin/WIZ），
直到 Claude/SDMN 的末尾哨兵才真正退出。完整 Slade 39→40 fixture 进一步确认升级不回复当前
HP/MP，派生属性会从 base/class 重建并重新应用 Short Knife 的 +5 ATT；叠加 ATTACK/BOOST/SLOW/STUN
与 Thieve's Dagger 的第二案例又确认状态先于装备、各自按 base 属性取整。后续案例继续确认
部分状态计数、Black Ring 刷新 CURSE，以及 Ninja Katana 的 `INCREASE_DOUBLE` 会把 NINJ
counter 1/8 错清为 1/32。独立启动观察确认 Karna 在 HEAL 3 预扫描时会把 PRST 基础 prowess
从 `0x03` 改为 `0x13`，随后再由普通升级回放学会法术；同一分支的合成 `0x43→0x53` 用例确认
counter 1/16 保留而 double 从 1/32 提升到 1/16。完整 16 组高半字节矩阵进一步确认原版把
double/counter 合并当作一个数值加一：`0x73→0x73` 封顶，`0xF3→0x03` 则发生 byte wrap 并清空两者。
同一成长子系统的单启动 clamp fixture 又覆盖 8 个 wrapper 边界：ATT/DEF/AGI/MOV 的五个上限
饱和，以及 DEF/AGI/MOV 三个减法下溢归零；带 bit 7 的 base AGI `0xE3→0xE4` 同时确认标志位保留。
Battle 01 的自然 enemy refresh 另确认 Black Ring 的 ATT +10 对敌人仍生效，但 stale CURSE
`4→0` 且不会由 cursed item 重新插入。
第一场剧情战斗另有 map 57/16×20 area、Stack 解压 terrain、背景/经验/胜负
全局元数据、9 个 placement/AI 实体和 3 个 region polygon 的独立 ROM decode；固定 seed 的自然
初始化 H3 确认实际首回合列表恰有 3 名盟友与 6 个 Gizmo，边界 H3 另确认 AGI 127/128、第二行动、
dead/unplaced 过滤与 signed-byte 稳定排序。区域激活 H3 进一步确认初始三块区域均未触发；把 Bowie
移到 `(8,12)`（region 2 的边界）会同时触发三个 polygon，并只给对应六个 Gizmo 的原始 AI
bitfield 增加 primary-active 位；受控 secondary-region 用例进一步确认 secondary 命中会同时置
primary/secondary active 两位。AI 驱动的自然攻击另确认 30% 地形减伤和弓手对 hovering
目标的 25% 加成顺序及整数截断，并继续覆盖 dodge、受控 critical、两次向下 spread、double/counter、
死亡目标清除后续 double/counter，完整覆盖 double validator，并确认距离、睡眠/眩晕状态、同阵营标志及五种特殊敌人的方向性反击排除、HP 零下限、击杀判定、单行动 49 EXP 累加上限、
reaction 的持久 HP 回放，以及 Battle 01 减半/抖动后的 24 EXP 实际入账。连接夹具进一步从
99 EXP 出发确认 `99 -> 123 -> 23`、一次自然 Bowie/SDMN 升级、`[2,2,0,1,1,1,255]` 结果载荷，
以及不回复当前 HP/MP、刷新派生属性后的最终持久状态。独立矩阵还确认有效等级差 `<3/3/4/5/6/>=7`
对应击杀值 `50/40/30/20/10/0`，且晋职职业先把存储等级加 20；最终奖励矩阵则覆盖 Battle 01
减半后的两次 `RNG(16)`：不变、`+1`、`-1`、相互抵消，以及最低 1 EXP。
攻击法术 EXP 又把同一档位连接到自然 BLAZE 2：10/100 HP 比例得到 `5/4/3/2/1/0`，致死时
先加比例奖励再加完整 kill 奖励，且两次加算都独立饱和到 49；受控 award seam 还确认非表内
battle ID 不执行 Battle 01 减半。
治疗 EXP 矩阵确认 PRST/VICR/MMNK 白名单、非治疗职业/敌方/零最大 HP 跳过、普通晋职威力 5/4、
HEAL 4 的 power-255 满回复旁路，以及 10 点最低值和累计 25 点上限。
EXP 命令边界进一步确认加算先在 200 饱和、每条命令只处理一次 100 点阈值；`199+24`
因此结束为升级一次且仍保留 100 EXP，而不会在同一命令内连续升级。
gold 边界矩阵确认普通加算、恰好/超过 9,999,999 上限及 32-bit carry 都遵循原版饱和规则。

### Phase 3 — Game Design Reconstruction

- 按研究地图补齐系统文档；
- 形成战斗、成长、探索、剧情、UI 和内容管线设计规格；
- 建立“原版事实 vs 现代化选择”的差异登记。

### Phase 4 — Modern Engine Vertical Slice

- 根据数据导入、自动化测试、2D tile/sprite、平台和许可证要求选择引擎；
- 用占位或明确许可的资源完成一张地图、一场战斗、少量角色/敌人和存读档；
- 让 H4 复用前期 fixture，而不是另造一套规则测试。

### Phase 5 — Content & Productization

- 扩展全游戏数据与系统覆盖；
- 建立资源替换、编辑器、可访问性、现代 UI 和发布管线；
- 只发布项目自有或获得许可的代码与资源。

现代引擎暂不拍板。Godot、Unity 或自定义框架的选择必须由 Phase 2-3 得到的实际合同和自动化
要求驱动，并用 decision record 说明；否则过早开 engine project 只会把未知的原版规则固化
成偶然实现。

## 下一步

源码“找文件”阶段已经收口：code 是 381/387 strict reach，剩余 6 个都有明确 H2 所有权；data
是 1,690/1,690 H2 inventory，domain-aware reach 已为 1,017/1,690；其中 980 个 H1 文件与 37 个
Z80 song 文件分开报告，余下差额归因于 include-site-only、unlabeled/alternate 或无入口符号。
地图 setup 的 flag selection、六指针 layout、四类 event
dispatcher 结构、全部 entity stream、263 个 entity/zone/item event source 和 75 个 description
target、84 个 initialization source 与全部 47 个 standalone setup script 也已静态闭合。79 个
map entry、662 个 source-form map-content section 和 154 个私有 blocks/layout payload 现已全部完成
source/H1/ROM parity；77 组压缩 blocks/layout 也已由 Python 全量解码为 19,771 个 3x3 block 与
77 个 64x64 layout，并验证所有 layout 索引不越界。解码结果现已接入包含 79 个 map definition、
1,859 个共享资源和 15,805 个逻辑记录/操作的实现无关 canonical import；其中 64 条 setup route、
126 份六指针 definition 已连接 entity/event/description/init 资源，90 个 init callable 的 654 条
operation 与全部 130 条 branch 也已结构化。完整输出保持 ignored，仓库仅追踪 schema、digest 和
聚合 fixture。47 个 standalone script 的 178 个 label/8,058 条 operation、init source 的 201 个
内嵌 program/3,718 条 operation 均已结构化，75 个 init script target 全部可解析。首个十案例 setup-selector H3 已在单次
BizHawk 启动中确认 missing/default、last-set-flag-wins 与 alias route，
随后六案例 init-dispatch H3 又确认 missing map 跳过调用，以及 active/scripted/direct-return setup
各自只调用一个 H2 目标并返回；entity/zone/item rail 的 9 个 exact/wildcard/default H2
选择案例也已在一个 BizHawk 启动中全部通过原版 wrapper 分发验证，
同时把渲染、VDP animation timing 与 transition persistence 保持为集中运行矩阵。

分散式 entity-action 队列现已从候选扫描提升为 H2 合同：75 个文件的 1,472 条命令全部归属于
361 个内嵌程序或 11 个连续 standalone ROM 区段，17 个具名入口、14 条相对分支和 364 条绝对
jump 均已解析，11 段共 942 bytes 与 H1/ROM 地址和哈希一致。80-slot entity-action dispatcher
也已逐槽绑定：37 个 filler、43 个 handler、40 个宏可达 opcode 与三个 handler-only branch
opcode 全部有 H1 地址和结构化 handler catalog；五个运行时宏在完整源码 corpus 中从未使用。
catalog 已进一步按源码指令分类 18 个实体字段（11 read/17 write）和 15 个全局状态（10
read/5 write），并保留脚本参数的 byte/word/long 读取宽度。43 个 handler 现已完整分成八个
源码角色家族，同时记录 22 条实体 bit test/set/clear 和 46 条 fixed/relative/absolute 脚本指针
动作；`FLAGS_A` 的 entity-collision、map-collision、obstruction 位均已关联读写 handler。
宏参数 ABI 也已闭合 46 个声明参数/86 bytes：40 个运行时宏完整消费，`ac_pass` 跳过其 word，
`ac_setId`/`ac_setSprite` 只读声明 word 的低字节。`ac_branch` 的宏外 word-relative operand
和三个 handler-only 6-byte layout 也已闭合；后三者在
2,204 条源码命令中均未出现，因此完整 corpus 实际使用 35/43 handler。43 个 handler 的 flow
outcome 也已统一为 redispatch/yield：39 个可 redispatch、11 个可 yield、
七个具备双路径，四个连续控制 handler 只 yield。46 个声明参数现已全部获得 handler-side
角色：10 signed numeric、20 unsigned numeric、15
boolean、1 ignored；七个 dual handler 也全部绑定选择条件及源码语句。`ac_randomWalk` 是唯一
显式源码注释冲突：宏称前两个 word 为 X/Y speed，handler 实际把它们作为 unsigned 中心 tile
坐标，并把第三个 word 作为半径。`UpdateEntityData` 也已闭合为 560 bytes/190 instructions/九个
H1 阶段：15 个实体字段、FLAGS_A 0-3 的加减速、3/4 acceleration 与 1/4 deceleration、velocity
积分、±8 facing dominance、动画 `>>5`/`-1` disable/`>30` reset、目的地 crossover snap 和到达
tile 的 layer/immersed 更新均有源码与 ROM 合同。四个 helper 也已闭合 434 bytes/135
instructions/22 callers。目的地冲突实际扫描 49 slots，以
`abs(dx)+abs(dy)<384` 判定；冲突留下 Z=0、无冲突留下 Z=1，明确推翻了源码注释中的 “zero-bit
set if true”。sprite gate 的 auto-facing/facing-change/queue-limit fallthrough、special sprite 与
entity 32 bypass、effect/DMA 链，以及 `(hash(Y>>7)<<6)+hash(X>>7)` 的 word-byte offset 均已结构化。
单次启动的 entity-movement H3 已把 13 个 case 合并为 20 个原始 VInt tick：wait 阈值、相对/
绝对移动各自的阻挡与放行、连续加减速、facing dominance、动画推进/禁用/夹回、crossover snap
以及 layer 2/layer 0/immersed 到达状态均逐字段匹配 Python 模型。RAM 脚本用 dispatcher 的
yield-only filler 槽终止；`ac_pass` 会前进四字节并继续分发，不能当作停止命令。
caller-dependent story reachability 仍单独保留。
map-script engine 同样已按静态优先闭合 90-slot jump table、82 个非 filler opcode、83 个唯一
handler、93 个宏合同和全源码 13,515 次命令调用；其三项剩余运行时问题按 story branch、跨
entity/camera/text/transition 的 frame timing、palette/VDP presentation 分组。其 133 个主宏参数
field/234 operand bytes、四档命令宽度和 77 sequential/1 absolute/4 conditional/1 inline 的
cursor-flow 分类也已静态闭合。完整 corpus 的 304 program/348 label/184 transfer 同样已解析，
所有 62 条 script jump 与 122 条 subroutine call 均有目标；297 个 program 有静态引用、7 个
零引用。89 个 program 的 6-flag read/56-flag write 状态依赖也已闭合，不为单一 opcode 另起
模拟器 fixture。
只有 direct-`rts` stub reachability、
非标准 description caller、script side effects、transition persistence 或 presentation timing 在静态解析后仍有歧义
时，才启动同一 observation seam 的集中 BizHawk matrix；UI/presentation、SRAM hardware 与
VDP/Z80/audio timing 继续留在各自的共享矩阵队列，不拆成单案例模拟。

参与工作前请阅读 [`AGENTS.md`](./AGENTS.md)。
