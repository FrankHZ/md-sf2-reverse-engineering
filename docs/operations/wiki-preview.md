# Wiki 试点的本地预览

维护者可以在主仓工作树里预览五个既有 Wiki 页的变更。输入是一个完整的 accepted main
提交号和一个本地 Wiki Git 工作副本。命令读取提交里的文档，输出五个候选 Markdown 文件
和一份 `preview.diff`。每次运行都会在本工作树忽略的 `local/` 下创建新目录。
工具没有上传入口。主仓 PR 继续拥有事实、设计和翻译修订。

## 运行与复核

从主仓的工具工作树运行。先按[本地配置说明](local-private-inputs.md)准备该工作树的
`uv` 环境。已有 Wiki 副本时，将 `$wiki` 设为其根目录并跳过 clone。首次获取副本可用：

```powershell
$wiki = Read-Host '输入独立于主仓工作树的新 Wiki 副本绝对路径'
git clone https://github.com/FrankHZ/md-sf2-reverse-engineering.wiki.git $wiki
```

```powershell
. ./local/private-inputs.ps1
git fetch origin main
$source = '6c417a52cecf9e8925762045637d0a917796fb24'
$previewArgs = @('run', '--locked', 'python', '-m', 'sf2tool.wiki_preview',
                 '--wiki', $wiki, '--source', $source)
$first = & uv @previewArgs
if ($LASTEXITCODE) { throw 'Wiki preview failed' }
$second = & uv @previewArgs
if ($LASTEXITCODE) { throw 'Second Wiki preview failed' }
git -c core.autocrlf=false diff --no-index --exit-code -- $first $second
if ($LASTEXITCODE) { throw 'Different preview outputs' }
git -C $wiki apply --stat "$first/preview.diff"
git -C $wiki apply --check "$first/preview.diff"
```

最后两个命令列出改动并检查补丁是否适用于 Wiki 副本。打开 `$first/Home.md`、
`$first/_Sidebar.md` 和 `$first/preview.diff` 核对导航与正文。更新预览时可以用
`git rev-parse origin/main` 取得新提交号。若已接受的主仓文档移动，先在本工具中
更新对应路径；缺失的文件或标题锚点会令预览失败。

## 页面与来源

审计基准是 Wiki 提交 `c027ddb07884c481cf06d20238065051d18ed093`。该版本的正文镜像来自
[主仓提交 65d9aee7](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/65d9aee70e3a6894fc9ac22e17b0fa97cc89ddc0)。
下表映射到预览示例提交 `6c417a52`。在 Wiki 中，带空格的页面标题对应连字符文件名。
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

## 保留 Wiki 端的修改

Wiki 副本需包含审计基准。工具读取该基准的原文，检查受管路径在 Wiki HEAD、索引
和工作树中的变化，并检查受管标题是否与其他候选页面冲突。遇到修改、删除、
重命名或标题冲突时会报告路径并退出。可以用以下命令在原副本中查看相关差异：

```powershell
git -C $wiki status --short --untracked-files=all
git -C $wiki diff c027ddb07884c481cf06d20238065051d18ed093 HEAD
git -C $wiki diff HEAD
```

保留差异并在主仓 PR 中协调需要接纳的手写内容。非管理页保留在原处，预览目录仅含受管页和
补丁。远端发布、受管页更名或删除、权限与回退由[父票 #1820](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1820)承接。

**Unknown**：现有 Wiki Git 历史和可检索到的项目仓库未能确认 DeepSeek 独立实验仓的身份。
