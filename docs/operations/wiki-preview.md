# Preview and Publish the Design Wiki

Publish L0–L3 Chinese design from an accepted first-parent `origin/main` commit. The Wiki's
[design map](../design/zh-CN/design-map.md) connects vision, overall gameplay, system design,
and mechanism rules. `Home` introduces the remake and reader routes first; source links, full
commit identity, and issue-feedback links live in a compact footer on each page.

The [scope agreement](../design/documentation-roadmap.md#design-and-support-scope) keeps research,
ROM/source mechanics, configuration tables, implementation, and acceptance ledgers at their
canonical English owners in the main repository. Wiki pages link directly to those owners rather
than duplicating raw technical prose or mutable campaign counters.

## Select the source and Wiki base

Run from the main repository's tooling checkout with Git and the locked `uv` environment. Use a
dedicated Wiki clone under one publisher's control. Fetch and push using the same Wiki remote and
default branch. Use a disposable bare target when rehearsing.

```powershell
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$target = 'https://github.com/FrankHZ/md-sf2-reverse-engineering.wiki.git'
$wiki = Read-Host 'Absolute path to the dedicated Wiki clone'
git clone -- $target $wiki  # Skip for an existing dedicated clone
```

`$source` is the full accepted `origin/main` SHA. `$base` is the last reviewed, published Wiki SHA.
Always re-fetch the Wiki remote and inspect the actual latest managed publication commit (for
example, `a46072b9ce37cdec037113f97ddd21ab341ad2c7` published source `91088c36...` prior to `#2259`
Phase B2) instead of assuming an earlier base:

```powershell
git fetch origin main
$source = git rev-parse origin/main
$branch = git -C $wiki symbolic-ref --short HEAD
$pushUrls = @(git -C $wiki remote get-url --push --all origin)
if ((git -C $wiki remote get-url origin) -ne $target -or
    $pushUrls.Count -ne 1 -or $pushUrls[0] -ne $target) { throw 'Unexpected Wiki remote' }
git -C $wiki fetch origin "refs/heads/${branch}:refs/remotes/origin/$branch"
git -C $wiki merge --ff-only "origin/$branch"
if ((git -C $wiki rev-parse HEAD) -ne (git -C $wiki rev-parse "origin/$branch")) {
    throw 'Review the unpublished commits in this clone before proceeding'
}
git -C $wiki log -3 --first-parent --grep="^Publish SF2 Wiki from " --oneline
$base = git -C $wiki log -1 --first-parent --grep="^Publish SF2 Wiki from " --format="%H"
$wikiArgs = @('run', '--locked', 'python', '-m', 'sf2tool.wiki_preview',
              '--wiki', $wiki, '--source', $source, '--base', $base)
$candidate = & uv @wikiArgs
$diff = Join-Path $candidate 'preview.diff'
```

## Review, apply, and read back

The preview writes candidate Markdown and `preview.diff` under ignored `local/wiki-preview-*/`.
Read `Home.md`, `_Sidebar.md`, `Design-Documentation.md`, and each body page for readability,
L0–L3 navigation, compact source footers, Chinese design links, English source links, anchors,
full source SHA, and unintended deletions. An empty diff means the selected source is already
published at the selected base.

```powershell
if ((Get-Item -LiteralPath $diff).Length) {
    git -C $wiki apply --stat $diff
    git -C $wiki apply --check --cached $diff
    Get-Content -Raw -Encoding utf8 -LiteralPath $diff
}
```

After review, `--apply` recomputes and verifies the exact reviewed patch before creating a local
Wiki commit. Push that commit with a normal fast-forward push, then inspect an independent fresh
clone readback.

```powershell
if ((Get-Item -LiteralPath $diff).Length) {
    $publication = & uv @wikiArgs --apply $diff
    git -C $wiki show --stat $publication
    git -C $wiki push origin "${publication}:refs/heads/$branch"
    $reader = Read-Host 'Absolute path to a new independent readback clone'
    git clone --no-local --branch $branch -- $target $reader
    git -C $reader merge-base --is-ancestor $publication HEAD
    git -C $reader show --format=full --name-status $publication
    git -C $reader ls-tree -r --name-only $publication
}
```

Inspect `Home.md`, `_Sidebar.md`, and body pages from `$reader`, then repeat the preview with
`$publication` as `--base` and the same `$source`. The diff must be empty and `--apply` must report
`No managed page changes.`.

## Managed pages and candidate rehearsal boundary

`Home`, `_Sidebar`, and `Design-Documentation` are fixed managed entries. The body
[mapping](wiki-mirrors.tsv) lists each tracked Chinese L0–L3 design page, its stable Wiki title,
and its Chinese navigation label in L0 → L1 → L2 → L3 order:

- `Remake-Vision` (`L0 · 重制愿景`)
- `Gameplay-Overview` (`L1 · 玩法总览`)
- `Tactical-Battle-Loop` (`L2 · 战术战斗循环`)
- `Progression-and-Economy` (`L2 · 成长与经济`)
- `Map-Design-Principles` (`L2 · 地图设计原则`)
- `Map-Exploration-Contract` (`L3 · 地图探索`)

Unreviewed or pending topics (such as `story-progression.md` under `#2366`) stay linked from
`Design-Documentation` to the main repository rather than being published prematurely.

**Candidate rehearsal boundary:** `Source` strictly requires `--source` to be on the fetched
`origin/main` first-parent history and reads `wiki-mirrors.tsv` and all Markdown bodies from that
same Git tree. Therefore:

- Before a mapping PR merges, previewing against accepted `origin/main` exercises the new renderer
  template against the currently accepted mapping.
- To rehearse an unmerged `wiki-mirrors.tsv` change end-to-end before merge, run only against an
  isolated local candidate fixture / bare target under ignored `local/` (never mutate real
  `origin/main`, weaken `Source` guards, or push candidate output to the live Wiki).
- Live publication always regenerates its reviewed preview from the post-merge accepted
  `origin/main` commit.

## Recovery

- If a managed path changed on the Wiki, compare `git -C $wiki diff $base HEAD` and local changes.
  Bring edits worth keeping into the main repository. Use a normal corrective commit to restore
  managed paths to the reviewed Wiki base before retrying. Keep independent unmanaged pages and
  investigate title collisions before applying.
- If a push is rejected or interrupted, read back the same remote first to see whether the
  publication arrived. If absent, use a fresh clone and preview against the last published base
  again.
- To restore an earlier design release, keep the latest Wiki publication as `--base`, select the
  earlier compatible `origin/main` SHA as `--source`, and review and publish the new diff. To undo
  a single publication, review a normal `git revert <publication>` and push it.

## Rehearse with a bare target

Use an existing read-only Wiki snapshot and a new `$lab` path under this checkout's ignored
`local/`. Seed the local bare target with the snapshot's reviewed tip, then run the same preview,
apply, push, readback, and no-op checks against the local target.

```powershell
$target = Join-Path $lab 'target.git'
$wiki = Join-Path $lab 'publisher'
$branch = git -C $snapshot symbolic-ref --short HEAD
git init --bare -b $branch $target
git -C $target fetch --no-tags $snapshot "refs/heads/${branch}:refs/heads/$branch"
git clone --no-local -- $target $wiki
```
