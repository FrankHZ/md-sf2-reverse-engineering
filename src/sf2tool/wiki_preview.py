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
    DESIGN_MAP_TITLE,
    ENTRY_TITLES,
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
ENGLISH_INDEX = "docs/README.md"
DESIGN_ROOT = "docs/design/zh-CN/"
DESIGN_MAP = DESIGN_ROOT + "design-map.md"
ENGLISH_DESIGN_MAP = "docs/design/design-map.md"
REMAKE_README = "remake/README.md"
CAPABILITY_STATUS = "remake/docs/capability-status.md"
ARCHITECTURE = "remake/docs/architecture.md"


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
        reader = csv.reader(self.read(MAPPING).splitlines(), delimiter="\t")
        if next(reader, []) != ["source", "title", "中文入口"]:
            raise ValueError(f"invalid header in {MAPPING}")
        result = {}
        titles = {title.casefold() for title in ENTRY_TITLES}
        for path, title, label in reader:
            if not (
                path.startswith(DESIGN_ROOT)
                and path.endswith(".md")
                and path in self.files
                and path != DESIGN_MAP
            ):
                raise ValueError(f"expected a tracked Chinese design excerpt in {MAPPING}: {path}")
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

    def render_body(self, path: str) -> str:
        titles = {path: title for path, (title, _) in self.mirrors.items()}
        titles[DESIGN_MAP] = DESIGN_MAP_TITLE

        # Preserve the reviewed design prose and resolve its inline Markdown destinations.
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
            if not prefix.startswith("!") and dest in titles:
                resolved = f"{REPOSITORY}/wiki/{titles[dest]}"
                if url.fragment:
                    resolved += "#" + url.fragment
            return f"{prefix}{resolved})"

        return re.sub(r"(!?\[[^\]\n]*\]\()([^()\s]+)\)", replace, self.read(path))


def repo_link(source: Source, path: str, label: str, fragment: str = "") -> str:
    return f"[{label}]({source.url(path, fragment)})"


def wiki_link(title: str, label: str) -> str:
    return f"[{label}]({REPOSITORY}/wiki/{title})"


def render(source: Source) -> dict[str, str]:
    commit_link = f"[{source.commit}]({REPOSITORY}/commit/{source.commit})"
    short_commit_link = f"[{source.commit[:8]}]({REPOSITORY}/commit/{source.commit})"
    issue_link = f"[反馈或纠错]({REPOSITORY}/issues/new)"

    def footer(path: str, english_path: str | None = None) -> str:
        parts = [repo_link(source, path, "主仓中文源页")]
        if english_path is not None:
            parts.append(repo_link(source, english_path, "英文规范源 · English source"))
        parts.append(f"源码提交 {commit_link}")
        parts.append(issue_link)
        return "\n\n---\n\n> " + " · ".join(parts) + "\n"

    def design_page(path: str) -> str:
        english = path.replace(DESIGN_ROOT, "docs/design/", 1)
        body = source.render_body(path).rstrip("\n")
        return body + footer(path, english)

    def page_link(path: str, fallback_label: str) -> str:
        if path in source.mirrors:
            title, label = source.mirrors[path]
            return wiki_link(title, label)
        return repo_link(source, path, fallback_label)

    entries = "".join(f"- {wiki_link(title, label)}\n" for title, label in source.mirrors.values())
    design_map = wiki_link(DESIGN_MAP_TITLE, "L0–L3 设计地图")
    vision = page_link(DESIGN_ROOT + "game-vision.md", "L0 · 重制愿景")
    overview = page_link(DESIGN_ROOT + "synthesis/gameplay-overview.md", "L1 · 玩法总览")
    index = repo_link(source, INDEX, "主仓中文设计索引")
    english_map = repo_link(source, ENGLISH_DESIGN_MAP, "English design map")
    english_index = repo_link(source, ENGLISH_INDEX, "英文文档总索引")
    capability = repo_link(
        source, CAPABILITY_STATUS, "当前重制可玩范围与能力状态（英文源）", "campaign-feature-trace"
    )
    sidebar_capability = repo_link(
        source, CAPABILITY_STATUS, "当前可玩能力（英文源）", "campaign-feature-trace"
    )
    remake_start = repo_link(source, REMAKE_README, "本地运行与游玩指南（英文源）")
    architecture = repo_link(source, ARCHITECTURE, "重制引擎架构（英文源）")

    home_body = (
        "# Shining Force II 重制设计 Wiki\n\n"
        "本 Wiki 收录美版 Mega Drive / Genesis《光明力量 II》（Shining Force II）逆向研究与"
        "独立重制的 **L0–L3 中文设计文档**，按玩家处境、可选决策、代价与后果组织阅读。\n\n"
        "## 按你的目标开始阅读\n\n"
        f"1. **了解这款重制与核心玩法（玩家 / 新读者）**：先读 {vision} 与 {overview}；"
        f"查看最新可玩进度与本地运行方式，请前往主仓唯一入口：{capability}、{remake_start}。\n"
        f"2. **查阅系统设计与机制规则（游戏策划）**：从 {design_map} 按系统进入下方已同步的 "
        "L2 系统专题与 L3 规则页；未入 Wiki 的专题会明确标出缺口和主仓位置。\n"
        "3. **查原版反汇编证据、数据表或工程实现（研发 / 考据）**：完整技术资料、原版证据"
        f"与配置表均保留在主仓，请查阅 {index}、{english_index} 与 {architecture}。\n\n"
        "## 已同步的 Wiki 页面\n\n"
        f"- {design_map}（全系统总览与规则链路）\n"
        f"{entries}\n"
        "## 尚未同步到 Wiki 的设计专题\n\n"
        "剧情事件与章节总表（`#2366`）、现代引擎与 Debug 设计入口（`#2358`）、"
        "UI/现代化总览（`#2386`）仍在主仓整理中。"
        f"你可以先通过 {design_map} 或 {index} 跳转主仓阅读，待审阅完成后再纳入本 Wiki，"
        "不把纯工程文档直接冒充为中文策划正文。"
    )

    sidebar_body = (
        "## SF2 设计 Wiki\n\n"
        f"- {wiki_link('Home', '首页')}\n"
        f"- {design_map}\n\n"
        "### L0–L3 已同步正文\n\n"
        f"{entries}\n"
        "### 主仓入口\n\n"
        f"- {index}\n"
        f"- {english_map}\n"
        f"- {sidebar_capability}\n\n"
        "---\n\n"
        f"> 源码 {short_commit_link} · {issue_link}\n"
    )

    pages = {
        "Home": home_body + footer(INDEX, ENGLISH_DESIGN_MAP),
        "_Sidebar": sidebar_body,
        DESIGN_MAP_TITLE: design_page(DESIGN_MAP),
    }
    pages.update((title, design_page(path)) for path, (title, _) in source.mirrors.items())
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
