# Preview and Publish the Design Wiki

Publish L0–L3 design from an accepted first-parent `origin/main` commit. The Wiki's
[design map](../design/zh-CN/design-map.md) connects the vision, gameplay loop, system design,
and behavior rules. Publish reviewed design excerpts with their conditions and consequences.
The [scope agreement](../design/documentation-roadmap.md#design-and-support-scope) places
research, ROM and source mechanics, configuration, implementation, and acceptance at their
English repository owners. Design pages may link to those owners, including the
[map and exploration contract](../design/contracts/world/map-exploration.md) and
[Map3–Battle01 readiness ledger](../design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone).

## Select the source and Wiki base

Run from the main repository's tooling checkout with Git and the locked `uv` environment.
Use a dedicated Wiki clone under one publisher's control. Fetch and push using the same
Wiki remote and default branch. Use a disposable bare target when rehearsing.

```powershell
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$target = 'https://github.com/FrankHZ/md-sf2-reverse-engineering.wiki.git'
$wiki = Read-Host 'Absolute path to the dedicated Wiki clone'
git clone -- $target $wiki  # Skip for an existing dedicated clone
```

`$source` is the full accepted main SHA. `$base` is the last reviewed, published Wiki SHA.
Use the audited pilot base shown below for the first managed publication. For later runs,
inspect the source SHA, page changes, and `SF2-Wiki-Page` trailers with
`git -C $wiki log --first-parent --grep="^Publish SF2 Wiki from "` and `git -C $wiki show <SHA>`.

```powershell
git fetch origin main
$source = git rev-parse origin/main
$base = 'c027ddb07884c481cf06d20238065051d18ed093'  # Replace after the first publication
$branch = git -C $wiki symbolic-ref --short HEAD
$pushUrls = @(git -C $wiki remote get-url --push --all origin)
if ((git -C $wiki remote get-url origin) -ne $target -or
    $pushUrls.Count -ne 1 -or $pushUrls[0] -ne $target) { throw 'Unexpected Wiki remote' }
git -C $wiki fetch origin "refs/heads/${branch}:refs/remotes/origin/$branch"
git -C $wiki merge --ff-only "origin/$branch"
if ((git -C $wiki rev-parse HEAD) -ne (git -C $wiki rev-parse "origin/$branch")) {
    throw 'Review the unpublished commits in this clone before proceeding'
}
$wikiArgs = @('run', '--locked', 'python', '-m', 'sf2tool.wiki_preview',
              '--wiki', $wiki, '--source', $source, '--base', $base)
$candidate = & uv @wikiArgs
$diff = Join-Path $candidate 'preview.diff'
```

## Review, apply, and read back

The preview writes candidate Markdown and `preview.diff` under ignored `local/wiki-preview-*/`.
Read `Home.md`, `_Sidebar.md`, and `Design-Documentation.md` for the L0–L3 routes. Check the
body changes, deletion list, Chinese design links, English source links, anchors, full source SHA
and correction links. An empty diff means the selected source is already published at the
selected base.

```powershell
if ((Get-Item -LiteralPath $diff).Length) {
    git -C $wiki apply --stat $diff
    git -C $wiki apply --check --cached $diff
    Get-Content -Raw -Encoding utf8 -LiteralPath $diff
}
```

After review, `--apply` recomputes and checks the exact patch before creating a local Wiki
commit. Push that commit with a normal fast-forward push, then inspect an independent readback.
Record `$publication` as the next base once readback passes.

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

Check the readback page contents with `git -C $reader show "${publication}:Home.md"` and
the corresponding body paths. Repeat the preview with `$publication` as `--base` and
the same source. The diff should be empty and `--apply` should report `No managed page changes.`.

## Managed pages and recovery

`Home`, `_Sidebar`, and `Design-Documentation` are fixed entries. The body
[mapping](wiki-mirrors.tsv) lists a tracked Chinese design excerpt, Wiki title, and Chinese
navigation label per row in reading order. The publisher links to the corresponding English
source and rewrites inline relative links to a selected Wiki page or the same source commit.
Review the actual preview when adding a page or introducing new Markdown syntax.

Add a mapping row to publish a page and remove a row to retire it. Renaming a title creates
a reviewed delete/add pair. Ownership comes from the last published Wiki commit, with the
original five pages identified by the audited pilot base. `Map-Exploration-Contract` keeps its
title and uses the [design excerpt](../design/zh-CN/contracts/world/map-exploration.md).
`Map3-Battle01-Readiness` leaves the managed set through a normal deletion.

- If a managed path changed on the Wiki, compare `git -C $wiki diff $base HEAD` and local
  changes. Bring edits worth keeping into the main repository. Use a normal corrective commit
  to restore the managed paths to the reviewed Wiki base before retrying. Keep independent
  unmanaged pages and investigate title collisions before applying.
- If the local commit fails, retain that clone and inspect `git status` and `git diff --cached`.
  Fix the Git identity or hook in a fresh clone, then reuse the reviewed patch if it still matches.
- If a push is rejected or interrupted, read back the same remote first to see whether the
  publication arrived. If absent, use a fresh clone and preview against the last published base
  again. A concurrent unmanaged edit can remain in the parent of the new publication.
- To restore an earlier design release, keep the latest Wiki publication as `--base`, select
  the earlier compatible main SHA as `--source`, and review and publish the new diff. To undo
  a single publication across publisher versions, review a normal `git revert <publication>`
  and push it. Use the restored publication as the next base.

## Rehearse with a bare target

Use an existing read-only Wiki snapshot and a new `$lab` path under this checkout's ignored
`local/`. Seed the local bare target with the snapshot's reviewed tip, then use the same
preview, apply, push, and readback commands against the local target.

```powershell
$target = Join-Path $lab 'target.git'
$wiki = Join-Path $lab 'publisher'
$branch = git -C $snapshot symbolic-ref --short HEAD
git init --bare -b $branch $target
git -C $target fetch --no-tags $snapshot "refs/heads/${branch}:refs/heads/$branch"
git clone --no-local -- $target $wiki
```
