"""Render the five-page GitHub Wiki pilot against its audited snapshot."""

from __future__ import annotations

import argparse
import difflib
import posixpath
import re
import subprocess
import tempfile
from pathlib import Path, PurePosixPath
from urllib.parse import quote, unquote, urlsplit

from sf2tool.paths import REPO_ROOT
from sf2tool.zh_translation import _heading_anchors

REPOSITORY = "https://github.com/FrankHZ/md-sf2-reverse-engineering"
WIKI_BASE = "c027ddb07884c481cf06d20238065051d18ed093"
INDEX = "docs/README.zh-CN.md"
ROADMAP = "docs/design/documentation-roadmap.md"
EXPLORATION = "docs/design/contracts/world/map-exploration.md"
READINESS = "docs/design/synthesis/milestones/map3-battle01-readiness.md"
MIRRORS = {EXPLORATION: "Map-Exploration-Contract", READINESS: "Map3-Battle01-Readiness"}
MANAGED = ("Home", "_Sidebar", "Design-Documentation", *MIRRORS.values())


def git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "--no-optional-locks", "-C", str(repo), *args], capture_output=True, check=False
    )
    if result.returncode:
        raise ValueError(
            result.stderr.decode("utf-8", errors="replace").strip()
            or f"git {args[0]} failed ({result.returncode})"
        )
    return result.stdout.decode("utf-8")


def check_wiki(wiki: Path) -> None:
    if Path(git(wiki, "rev-parse", "--show-toplevel").strip()).resolve() != wiki:
        raise ValueError("--wiki must be the root of the Wiki checkout")
    git(wiki, "merge-base", "--is-ancestor", WIKI_BASE, "HEAD")
    paths = [title + ".md" for title in MANAGED]
    changed = git(wiki, "diff", "--name-only", WIKI_BASE, "HEAD", "--", *paths)
    dirty = git(wiki, "status", "--porcelain", "--", *paths)
    if changed or dirty:
        raise ValueError(f"Review Wiki edits before previewing:\n{changed}{dirty}")
    # An unmanaged title can shadow a managed page even in a subdirectory.
    names = git(wiki, "ls-files", "-z", "--cached", "--others", "--exclude-standard")
    titles = {title.casefold() for title in MANAGED}
    page_suffixes = [
        ".md",
        ".markdown",
        ".mdown",
        ".mkd",
        ".mkdn",
        ".mdwn",
        ".mdtxt",
        ".mdtext",
        ".adoc",
        ".asciidoc",
        ".asc",
        ".org",
        ".rst",
        ".rest",
        ".textile",
        ".mediawiki",
        ".wiki",
        ".creole",
        ".rdoc",
        ".pod",
    ]
    for name in names.rstrip("\0").split("\0"):
        path = PurePosixPath(name)
        if name in paths or path.suffix.lower() not in page_suffixes:
            continue
        if path.stem.replace(" ", "-").casefold() in titles:
            raise ValueError(f"Wiki title collision: {name}")


class Source:
    """Resolve every document and local destination from the same accepted Git tree."""

    def __init__(self, commit: str):
        if not re.fullmatch(r"[0-9a-f]{40}", commit):
            raise ValueError("--source requires a full lowercase commit ID")
        if commit not in git(REPO_ROOT, "rev-list", "--first-parent", "origin/main").splitlines():
            raise ValueError("source is outside the fetched origin/main first-parent history")
        self.commit = commit
        self.files = set(git(REPO_ROOT, "ls-tree", "-rz", "--name-only", commit).split("\0"))
        self.anchors: dict[str, set[str]] = {}

    def read(self, path: str) -> str:
        return git(REPO_ROOT, "show", f"{self.commit}:{path}")

    def url(self, path: str, fragment: str = "", *, image: bool = False) -> str:
        if not path or path not in self.files:
            raise ValueError(f"missing source target: {path}")
        if fragment:
            if path not in self.anchors:
                self.anchors[path] = _heading_anchors(self.read(path))
            if unquote(fragment) not in self.anchors[path]:
                raise ValueError(f"missing source anchor: {path}#{fragment}")
        route = "raw" if image else "blob"
        return f"{REPOSITORY}/{route}/{self.commit}/{quote(path)}" + (
            "#" + fragment if fragment else ""
        )

    def directory_link(self, path: str, label: str) -> str:
        if git(REPO_ROOT, "cat-file", "-t", f"{self.commit}:{path}").strip() != "tree":
            raise ValueError(f"missing source directory: {path}")
        return f"[{label}]({REPOSITORY}/tree/{self.commit}/{path})"

    def chinese_link(self, path: str, label: str) -> str:
        mirror = path.replace("docs/design/", "docs/design/zh-CN/", 1)
        if mirror in self.files:
            return f"[{label}]({self.url(mirror)})"
        return f"[{label}（主仓英文）]({self.url(path)})"

    def render_body(self, path: str) -> str:
        # The two pilot bodies use inline Markdown destinations. Leave their prose intact.
        def replace(match: re.Match[str]) -> str:
            prefix, target = match.groups()
            url = urlsplit(target)
            if url.scheme or url.netloc:
                return match[0]
            if url.query:
                raise ValueError(f"unsupported relative query in {path}: {target}")
            dest = posixpath.normpath(posixpath.join(posixpath.dirname(path), unquote(url.path)))
            dest = dest if url.path else path
            resolved = self.url(dest, url.fragment, image=prefix.startswith("!"))
            if not url.path:
                return match[0]  # same heading in the unchanged mirror body
            if dest in MIRRORS and not prefix.startswith("!"):
                resolved = f"{REPOSITORY}/wiki/{MIRRORS[dest]}"
                if url.fragment:
                    resolved += "#" + url.fragment
            return f"{prefix}{resolved})"

        return re.sub(r"(!?\[[^\]\n]*\]\()([^()\s]+)\)", replace, self.read(path))


def render(source: Source) -> dict[str, str]:
    def credit(path: str) -> str:
        return (
            f"> 主仓源页 [{path}]({source.url(path)}) · "
            f"[{source.commit[:8]}]({REPOSITORY}/commit/{source.commit}) · "
            f"[反馈或纠错]({REPOSITORY}/issues/new)\n"
        )

    index = f"[主仓中文文档索引]({source.url(INDEX)})"
    roadmap = source.chinese_link(ROADMAP, "设计路线图")
    exploration = source.chinese_link(EXPLORATION, "地图与探索合同")
    readiness = source.chinese_link(READINESS, "Map 3 至 Battle 01 就绪台账")
    directories = " · ".join(
        (
            source.directory_link("docs/design/zh-CN", "中文设计目录"),
            source.directory_link("docs/design/zh-CN/contracts", "合同"),
            source.directory_link("docs/design/zh-CN/synthesis", "设计综合"),
            source.directory_link("docs/decisions", "ADR（主仓英文）"),
        )
    )
    pages = {
        "Home": (
            "# Shining Force II Reverse Engineering Wiki（试点）\n\n"
            + credit(INDEX)
            + "\n## 从这里开始\n\n"
            f"- {roadmap} · [[Design Documentation]]\n"
            f"- {exploration} · [[Map Exploration Contract]]（英文镜像）\n"
            f"- {readiness} · [[Map3 Battle01 Readiness]]（英文镜像）\n"
            f"- {index}\n\n"
            "## 阅读边界\n\n"
            "- **Confirmed**：由项目命令、测试或具名证据直接支持。\n"
            "- **Inferred**：有较强支持，但尚未在所需边界独立复现。\n"
            "- **Unknown**：明确保留的问题。\n\n"
            "证据、合同注册、翻译和验证由主仓维护。请在主仓通过 PR 修订。\n"
        ),
        "_Sidebar": (
            "## SF2 Wiki 试点\n\n- [[Home]]\n- [[Design Documentation]]\n"
            "  - [[Map Exploration Contract]]\n  - [[Map3 Battle01 Readiness]]\n\n"
            f"{index} · {roadmap}\n\n" + credit(INDEX)
        ),
        "Design-Documentation": (
            "# Design Documentation\n\n" + credit(ROADMAP) + "\n## 设计文档入口\n\n"
            f"- {roadmap} · {source.chinese_link('docs/design/glossary.md', '术语表')}\n"
            f"- Layer C 子系统合同：{exploration} · [[Map Exploration Contract]]（英文镜像）\n"
            f"- Layer B 跨系统综合：{readiness} · [[Map3 Battle01 Readiness]]（英文镜像）\n"
            f"- {index} · {directories}\n\n"
            "主仓保存设计路径、证据关联、翻译索引和门禁。通过主仓 PR 修改后可重新生成镜像。\n"
        ),
    }
    for path, title in MIRRORS.items():
        pages[title] = credit(path) + source.chinese_link(path, "阅读中文源页") + "\n\n"
        pages[title] += source.render_body(path)
    return pages


def preview(commit: str, wiki: Path) -> Path:
    check_wiki(wiki)
    pages = render(Source(commit))
    patch = []
    for title, content in pages.items():
        name = title + ".md"
        before = git(wiki, "show", f"{WIKI_BASE}:{name}")
        patch.extend(
            difflib.unified_diff(
                before.splitlines(keepends=True),
                content.splitlines(keepends=True),
                fromfile=f"a/{name}",
                tofile=f"b/{name}",
            )
        )
    scratch = REPO_ROOT / "local"
    scratch.mkdir(exist_ok=True)
    out = Path(tempfile.mkdtemp(prefix="wiki-preview-", dir=scratch))
    for title, content in pages.items():
        (out / (title + ".md")).write_text(content, encoding="utf-8", newline="\n")
    (out / "preview.diff").write_text("".join(patch), encoding="utf-8", newline="\n")
    return out


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", required=True, help="full accepted main commit")
    parser.add_argument("--wiki", required=True, type=Path, help="existing Wiki checkout")
    args = parser.parse_args()
    try:
        print(preview(args.source, args.wiki.resolve()))
    except (OSError, ValueError) as exc:
        parser.exit(1, f"wiki preview: {exc}\n")


if __name__ == "__main__":
    main()
