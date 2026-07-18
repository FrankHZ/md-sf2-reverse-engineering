# Shining Force II Reverse Engineering & Remake

这是一个从原版 Mega Drive/Genesis ROM 出发，系统拆解 **Shining Force II**，形成可验证的
逆向研究、游戏设计文档和结构化数据，并最终在现代引擎中重建其玩法的长期工程。

项目已完成 **Phase 1：可复现原版基线**，正在推进 **Phase 2：发现与数据合同**。本地环境
已经固定 ROM 身份、社区反汇编提交和工具 hash，能非交互地重建出逐字节一致的原版 ROM，
并已完成角色槽位、职业、物品、法术、转职、敌人定义与 Battle 01 scene 的 source/ROM 双路径
H2、成长曲线与法术学习合同，以及基础/调试覆盖 RNG、成长计算/完整升级（含投影后成长、
职业等级上限、继承法术升级与战斗 EXP 自然升级入口）、行动顺序、区域激活、物理伤害计算链和
BLAZE 2 四档 FIRE 抗性矩阵、DAO 四目标 power division、攻击法术 EXP、HEAL 1、SLEEP 1 四档 STATUS 抗性、DESOUL 即死/kill reward 与 SPOIT MP 吸收回放的整机运行时 H3；尚未下载外部补丁、选择现代重制引擎或开始
重制实现。实现无关的物理战斗、法术伤害与升级成长合同已经落地，并直接绑定现有 H3 fixture，供未来
H4 复用。

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
uv run sf2 verify --quick
uv run sf2 verify
```

日常提交默认只跑 `--quick`（Ruff、pytest、设计合同、研究索引、ROM 身份和工具链来源）以及本次
改动直接拥有的窄 rail，例如 `uv run sf2 h3 battle-exp`。十分钟以上的完整 `verify` 只在阶段
里程碑、准备合并/发布、共享 harness 或兼容层发生变化，以及明确要求全量 parity 时运行。

统一入口已经覆盖逆向关系索引、H0、toolchain provenance、H1、静态表双路径 parity 与成长合同 H2，以及
固定 BizHawk/Genesis Plus GX 的基础/调试覆盖 RNG、成长计算/完整升级及投影/上限/法术继承边界、战斗 EXP 自然升级、行动顺序、区域激活、物理伤害计算链、攻击法术伤害/EXP、HEAL 1、SLEEP 1、DESOUL 及 SPOIT H3；后续在同一入口继续补齐：

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
uv run sf2 verify
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
- 先打通角色/职业/物品/法术与一张战斗地图的端到端提取；
- 定义 schema、canonical serializer 和确定性测试；
- 建立模拟器选择决策与首批行为场景。

当前 H2 已覆盖 12 个 ROM table range：角色/职业/物品/法术的 281 条固定记录字段级零差异，
5 条成长曲线、59 个职业成长记录和 122 个学法术条目，以及 5 段转职表、103 个敌人名称和
103 个 56-byte 敌人定义。新增敌人/转职双路径比较 2,722 个字段、零差异；所有 canonical JSON
均有 schema、固定 hash 与重复导出验证，内容只写入 ignored 的 `local/derived/`，仓库不保存
原版名称清单。H3 以 7 组受控 seed 验证 `GenerateRandomNumber` 的原版 ROM 指令、RAM seed
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
counter 1/8 错清为 1/32。另一个完全不改状态的启动观察确认 Karna 在 HEAL 3 预扫描时会把 PRST
基础 prowess 从 `0x03` 改为 `0x13`，随后再由普通升级回放学会法术。
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
以及不回复当前 HP/MP、刷新派生属性后的最终持久状态。

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

下一块继续扩展 **Phase 2 的运行时证据**：补齐非 critical 的其他 spread seed、状态/抗性分支、
后续回合 region 状态与自然 muddle/same-side/special-enemy action reachability。
同时保留升级前一等级、缺失职业块、HEAL 3 的合成 counter 边界和当前/最大属性刷新，以及
`LASER radius = 3` 的显式行为验证队列。

参与工作前请阅读 [`AGENTS.md`](./AGENTS.md)。
