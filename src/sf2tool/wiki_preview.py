"""Preview managed Wiki pages or commit their reviewed diff to a local Wiki branch."""

from __future__ import annotations

import argparse
import csv
import posixpath
import re
import tempfile
from pathlib import Path
from urllib.parse import quote, unquote, urlsplit

from sf2tool.paths import REPO_ROOT
from sf2tool.wiki_sync import (
    ENTRY_TITLES,
    PILOT_MIRRORS,
    WIKI_BASE,
    apply_review,
    check_wiki,
    git,
    page_diff,
)
from sf2tool.zh_translation import _heading_anchors

REPOSITORY = "https://github.com/FrankHZ/md-sf2-reverse-engineering"
MAPPING = "docs/operations/wiki-mirrors.tsv"
INDEX = "docs/README.zh-CN.md"
ROADMAP = "docs/design/documentation-roadmap.md"


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

        self.mirrors = self.mirror_mapping()

    def mirror_mapping(self) -> dict[str, tuple[str, str]]:
        rows = PILOT_MIRRORS
        if MAPPING in self.files:
            reader = csv.reader(self.read(MAPPING).splitlines(), delimiter="\t")
            if next(reader, []) != ["source", "title", "中文入口"]:
                raise ValueError(f"invalid header in {MAPPING}")
            rows = list(reader)
        result = {}
        titles = {title.casefold() for title in ENTRY_TITLES}
        for path, title, label in rows:
            if not (
                path.startswith("docs/design/") and path.endswith(".md") and path in self.files
            ):
                raise ValueError(f"expected a tracked design document in {MAPPING}: {path}")
            if (
                not re.fullmatch(r"[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*", title)
                or re.fullmatch(r"CON|AUX|NUL|PRN|(?:COM|LPT)[0-9]", title, re.IGNORECASE)
                or title.casefold() in titles
                or path in result
                or not label.strip()
            ):
                raise ValueError(f"invalid or duplicate Wiki mapping: {path} -> {title}")
            titles.add(title.casefold())
            result[path] = (title, label)
        return result

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
            if dest in self.mirrors and not prefix.startswith("!"):
                resolved = f"{REPOSITORY}/wiki/{self.mirrors[dest][0]}"
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
    entries = []
    design_entries = []
    sidebar = []
    for path, (title, label) in source.mirrors.items():
        wiki_link = "[[" + title.replace("-", " ") + "]]"
        entry = f"{source.chinese_link(path, label)} · {wiki_link}（英文镜像）\n"
        layer = {"contracts": "Layer C 子系统合同：", "synthesis": "Layer B 跨系统综合："}.get(
            path.split("/")[2], ""
        )
        entries.append("- " + entry)
        design_entries.append("- " + layer + entry)
        sidebar.append(f"  - {wiki_link}\n")
    navigation = "".join(entries)
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
            f"{navigation}"
            f"- {index}\n\n"
            "## 阅读边界\n\n"
            "- **Confirmed**：由项目命令、测试或具名证据直接支持。\n"
            "- **Inferred**：有较强支持，但尚未在所需边界独立复现。\n"
            "- **Unknown**：明确保留的问题。\n\n"
            "证据、合同注册、翻译和验证由主仓维护。请在主仓通过 PR 修订。\n"
        ),
        "_Sidebar": (
            "## SF2 Wiki 试点\n\n- [[Home]]\n- [[Design Documentation]]\n"
            f"{''.join(sidebar)}\n"
            f"{index} · {roadmap}\n\n" + credit(INDEX)
        ),
        "Design-Documentation": (
            "# Design Documentation\n\n" + credit(ROADMAP) + "\n## 设计文档入口\n\n"
            f"- {roadmap} · {source.chinese_link('docs/design/glossary.md', '术语表')}\n"
            f"{''.join(design_entries)}"
            f"- {index} · {directories}\n\n"
            "主仓保存设计路径、证据关联、翻译索引和门禁。通过主仓 PR 修改后可重新生成镜像。\n"
        ),
    }
    for path, (title, _) in source.mirrors.items():
        pages[title] = credit(path) + source.chinese_link(path, "阅读中文源页") + "\n\n"
        pages[title] += source.render_body(path)
    return pages


def preview(
    commit: str, wiki: Path, base: str = WIKI_BASE, review: Path | None = None
) -> Path | str:
    pages = render(Source(commit))
    previous = check_wiki(wiki, base, set(pages))
    patch = page_diff(wiki, base, previous, pages)
    if review is not None:
        return apply_review(wiki, commit, pages, patch, review)
    scratch = REPO_ROOT / "local"
    scratch.mkdir(exist_ok=True)
    out = Path(tempfile.mkdtemp(prefix="wiki-preview-", dir=scratch))
    for title, content in pages.items():
        (out / (title + ".md")).write_text(content, encoding="utf-8", newline="\n")
    (out / "preview.diff").write_bytes(patch)
    return out


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", required=True, help="full accepted main commit")
    parser.add_argument("--wiki", required=True, type=Path, help="existing Wiki checkout")
    parser.add_argument("--base", default=WIKI_BASE, help="last reviewed Wiki publication commit")
    parser.add_argument(
        "--apply",
        type=Path,
        metavar="PREVIEW.DIFF",
        help="commit this reviewed patch to the local Wiki branch",
    )
    args = parser.parse_args()
    try:
        print(preview(args.source, args.wiki.resolve(), args.base, args.apply))
    except (OSError, ValueError) as exc:
        parser.exit(1, f"wiki preview: {exc}\n")


if __name__ == "__main__":
    main()
