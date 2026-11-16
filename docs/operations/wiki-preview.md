# Wiki 的本地预览与单向发布

主仓的 accepted main 提交提供页面内容和导航。维护者先检查候选 Markdown 和补丁，再把
同一补丁提交到指定的 Wiki 工作副本。远端写入使用普通 Git push，独立于主仓合并。
此流程只需要现有的本地 Git 和 `uv` 环境。

## 选择目标与两个提交

在主仓的工具工作树中运行 PowerShell 7。独立的 Wiki 发布副本由一名维护者使用，
使用克隆时检出的默认分支；从该副本的 `origin` 读取并推送到同一个目标。首次操作可先
使用下节的临时 bare 目标演练。实际发布使用以下入口，填写新副本的绝对路径：

```powershell
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. ./local/private-inputs.ps1
$target = 'https://github.com/FrankHZ/md-sf2-reverse-engineering.wiki.git'
$wiki = Read-Host 'Wiki 发布副本的绝对路径（独立于主仓工作树）'
git clone -- $target $wiki   # 已有专用副本时跳过
```

**来源提交** `$source` 是本次选定的、已获取的主仓 `origin/main` 第一父链上的完整 SHA。
**Wiki 基线** `$base` 是上次已核对、成功发布的 Wiki 提交 SHA。首次接管五页时才使用下方
试点基线。后续发布从上版提交读取当时的完整受管页清单；其后独立提交的非管理页可以保留。
在新副本中可用 `git -C $wiki log --first-parent --grep='^Publish SF2 Wiki from '` 找到候选，
用 `git -C $wiki show <SHA>` 核对发布内容、来源与 `SF2-Wiki-Page` 记录后选作基线。

```powershell
git fetch origin main
$source = git rev-parse origin/main
$base = 'c027ddb07884c481cf06d20238065051d18ed093'  # 后续改为上次发布的 Wiki SHA
$branch = git -C $wiki symbolic-ref --short HEAD
$pushUrls = @(git -C $wiki remote get-url --push --all origin)
if ((git -C $wiki remote get-url origin) -ne $target -or
    $pushUrls.Count -ne 1 -or $pushUrls[0] -ne $target) { throw 'Wiki 远端与预期目标不符' }
git -C $wiki fetch origin "refs/heads/${branch}:refs/remotes/origin/$branch"
git -C $wiki merge --ff-only "origin/$branch"
if ((git -C $wiki rev-parse HEAD) -ne (git -C $wiki rev-parse "origin/$branch")) {
    throw '当前副本含未发布的本地提交，请先复核'
}
$wikiArgs = @('run', '--locked', 'python', '-m', 'sf2tool.wiki_preview',
              '--wiki', $wiki, '--source', $source, '--base', $base)
$candidate = & uv @wikiArgs
$diff = Join-Path $candidate 'preview.diff'
```

## 复核、提交与读回

预览只读 Wiki，在工具工作树忽略的 `local/wiki-preview-*/` 下写入候选页与 `preview.diff`。
打开 `Home.md`、`_Sidebar.md` 和正文，检查中文入口、锚点、源提交及纠错回链；
逐项检查补丁的增加、删除及公共内容边界。相同输入可重复预览。补丁长度为零时已同步，
`--apply` 返回 `No managed page changes.`，无需提交或推送。

```powershell
if ((Get-Item -LiteralPath $diff).Length) {
    git -C $wiki apply --stat $diff
    git -C $wiki apply --check --cached $diff
    Get-Content -Raw -Encoding utf8 -LiteralPath $diff
}
```

核对有变化的补丁后，执行以下操作。`--apply` 使用相同来源与基线重新计算补丁并逐字节
比较，通过后才暂存并创建普通 Wiki 提交。受管路径的本地/已提交编辑、标题冲突和无关的
已暂存修改都应先处理。发出的对象固定为刚复核的提交。成功后保存 `$publication` 作下一版基线。

```powershell
if ((Get-Item -LiteralPath $diff).Length) {
    $publication = & uv @wikiArgs --apply $diff
    git -C $wiki show --stat $publication
    git -C $wiki push origin "${publication}:refs/heads/$branch"
    $reader = Read-Host '新建独立读回克隆的绝对路径'
    git clone --no-local --branch $branch -- $target $reader
    git -C $reader merge-base --is-ancestor $publication HEAD
    git -C $reader show --format=full --name-status $publication
}
```

## 页面集合、失败重试与恢复

三个固定入口是 `Home`、`_Sidebar` 和 `Design-Documentation`。正文镜像由来源提交中的
[映射表](wiki-mirrors.tsv)选择。每行是公开英文设计文档的仓库路径、Wiki 标题与中文入口
标签，以制表符分隔。表中顺序决定导航顺序。早于映射表的来源提交沿用下方试点的两篇正文。

- 源文档移动时，在同一个主仓 PR 中更新路径和受影响的文档链接。改变 Wiki 标题会删除旧页、
  添加新页并更新三个入口与受管正文的跨页链接。增行加入页面，删行退出纳管并删除上版镜像；
  仅保留表头即可撤下全部正文镜像。新页与非管理页同名或标题冲突时停止。
- 发现 Wiki 端修改时，用 `git -C $wiki diff $base HEAD` 和 `git -C $wiki diff HEAD`
  检查差异。保留原提交，将应采纳的内容通过主仓 PR 合入。若要继续发布，由维护者在 Wiki
  用普通纠正提交将冲突的受管路径恢复到上次已核对状态，再用已选基线预览。非管理页可独立演进。
- 本地提交失败时，`git status` 和 `git diff --cached` 可查看暂存补丁。保留故障副本，
  在同一远端的新克隆中修复身份或钩子问题，重用来源、已发布基线及已复核补丁。
- 推送遇到并发非快进或连接中断时，先从同一目标新建独立克隆并检查提交是否已经到达。
  已到达则使用发布 SHA；否则保留旧副本，在新克隆中取远端最新内容，用原来源和上一次
  **已发布**基线重新执行预览和应用。非管理页的并发改动会留在新提交的父版本中；
  受管页变化会令操作停止。整个流程使用普通快进推送。
- 要恢复到上一版，保持最新 Wiki 发布为 `$base`，把 `$source` 改成上一版的主仓 SHA，
  再预览、复核、提交和推送。新纠正提交会恢复该来源的正文、导航与受管集合，并保留其间
  的非管理页修改。读回时可用 `git diff <上一版Wiki提交> <纠正提交> -- <各受管路径>` 比对。
  如需逐字撤回跨工具版本的单个发布，也可在已更新的专用副本上 `git revert <发布SHA>`，
  复核并普通推送；后续预览选用已恢复的上一版基线。

## 在临时 bare 目标上演练

`$snapshot` 为已有只读 Wiki 克隆，`$lab` 为本工作树 `local/` 下的新目录绝对路径。
以下操作将已审计 Git 对象读入私有演练目标；随后在同一 PowerShell 会话用上面的
“选择目标与两个提交”第二段及“复核、提交与读回”步骤，只使用这里的本地目标：

```powershell
$target = Join-Path $lab 'target.git'
$wiki = Join-Path $lab 'publisher'
git init --bare -b master $target
git -C $target fetch --no-tags $snapshot 'c027ddb07884c481cf06d20238065051d18ed093:refs/heads/master'
git clone --no-local -- $target $wiki
```

可先选 `6c417a52cecf9e8925762045637d0a917796fb24`，再以首次发布的 Wiki SHA 为基线重跑，
确认空补丁后换用 `401a48e3a4114800cff57885aa296c3513d67cfe`。在第二次推送前从读回克隆提交
并推送一个非管理页，再执行失败重试和恢复步骤。要演练清单变动，可在一次性主仓副本的
本地 bare `origin/main` 上提交下一版映射，同时移动一篇源文档并改其标题、移除另一行、
加入一篇含跨页锚点的公开设计文档。从该工具副本运行相同入口。核对独立读回的
`git ls-tree --name-only HEAD`、`git show HEAD:<页面>` 和纠正前后的受管路径差异。

### 从首页或侧栏查 ROM 证据

所选主仓提交包含[ROM 分块总表](../research/rom-block-ledger.md)时，首页与侧栏各显示一个
“ROM 分块总表（主仓英文）”链接。两处都打开该提交的总表。早于总表的来源保留原有设计导航。
可用来源 `ff68b54341806563afc7c195a0a46225d63e3c0d` 在本地 bare 目标上演练，独立读回后
分别检查 `Home.md` 和 `_Sidebar.md` 的链接含有完整来源 SHA，再沿总表核对以下路径：

- 查地图时，在 **Physical sections** 的 S08 行打开 **Block and layout ranges by map**。
  页首的 **Map07's layout** 定位到 Map07 与 Map46 共用的布局行，同段的 **Decode and ROM parity**
  通向[解码与 ROM 比对证据](../research/world/map-content.md#private-binary-payloads)。
- 查具名资源时，从总表首段的 **family and topic** 打开家族表。例如 `portraits` 行的
  **Portrait graphics** 通向[肖像数据头与压缩流合同](../design/contracts/graphics/portrait-graphics-data.md#payload-header-and-stream-partition)。
- 查尚未解释的字节时，在 **Documented subranges** 找到 `S17-ally-tail`，沿 **Canonical entities**
  打开[队员记录证据](../research/party/static-core-data.md#canonical-entities)。总表给出该段的
  12 字节物理范围，其游戏用途仍为 **Unknown**。

## 页面与来源

审计基准是 Wiki 提交 `c027ddb07884c481cf06d20238065051d18ed093`。该版本的正文镜像来自
[主仓提交 65d9aee7](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/65d9aee70e3a6894fc9ac22e17b0fa97cc89ddc0)。
下表列出试点的五个页面。在 Wiki 中，带空格的页面标题对应连字符文件名。
`Home` 和 `_Sidebar` 沿用 GitHub Wiki 的入口名称。

| 受管文件 | 已有 Wiki 手写内容或旧路径 | 当前主仓 owner |
| --- | --- | --- |
| `Home.md` | 手写阅读入口、证据标签与纠错边界 | [中文文档索引](../README.zh-CN.md) |
| `_Sidebar.md` | 手写的四个 Wiki 导航目标与主仓导航 | [中文文档索引](../README.zh-CN.md)与下列设计入口 |
| `Design-Documentation.md` | 手写镜像表、目录和单向维护说明 | [中文路线图](../design/zh-CN/documentation-roadmap.md)及[英文 owner](../design/documentation-roadmap.md) |
| `Map-Exploration-Contract.md` | `docs/design/contracts/map-exploration.md` | [英文合同](../design/contracts/world/map-exploration.md)及[中文合同](../design/zh-CN/contracts/world/map-exploration.md) |
| `Map3-Battle01-Readiness.md` | `docs/design/synthesis/map3-battle01-readiness.md` | [英文台账](../design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone)及[中文台账](../design/zh-CN/synthesis/milestones/map3-battle01-readiness.md#当前已接受里程碑) |

两篇旧镜像剥离手写来源说明、按旧提交解析相对链接后，正文逐行等于各自的源文件。
两处旧路径在原提交仍有效。旧正文唯一的外部文档片段
`common-scripting.md#confirmed-map-script-controlaudio-macro-boundary` 在旧提交存在，
当前合同的链接已随研究 owner 迁往 `docs/research/world/`。审计未发现失效锚点、相对图片
或重复目标标题。Map3 台账的当前里程碑状态来自新提交的正文。

中文导航链接到已存在的中文 owner，英文 Wiki 正文保留其源语言与证据链接。
术语表与 ADR 目录显式标明主仓英文。每页都给出源文件、提交和纠错入口。
正文内的页内锚点保留，指向另一篇受管正文的链接转到保留标题的 Wiki 页；
其他相对文件与片段固定到同一提交，内联相对图片使用该提交的 raw 文件链接。
正文转换针对这两份源文件使用的内联 Markdown 链接语法。引入新的 Markdown
语法或调整页面集合时，应复核实际 diff 和相应链接。

**Unknown**：现有 Wiki Git 历史和可检索到的项目仓库未能确认 DeepSeek 独立实验仓的身份。
