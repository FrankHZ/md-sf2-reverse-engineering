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
DESIGN_ROOT = "docs/design/zh-CN/"
DESIGN_MAP = DESIGN_ROOT + "design-map.md"


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


def repo_link(source: Source, path: str, label: str) -> str:
    return f"[{label}]({source.url(path)})"


def wiki_link(title: str, label: str) -> str:
    return f"[{label}]({REPOSITORY}/wiki/{title})"


def render(source: Source) -> dict[str, str]:
    def credit(path: str) -> str:
        return (
            f"> {repo_link(source, path, '主仓中文源页')} · "
            f"[{source.commit[:8]}]({REPOSITORY}/commit/{source.commit}) · "
            f"[反馈或纠错]({REPOSITORY}/issues/new)\n"
        )

    def design_page(path: str) -> str:
        english = path.replace(DESIGN_ROOT, "docs/design/", 1)
        return (
            credit(path)
            + "\n"
            + repo_link(source, english, "英文规范源 · English source")
            + "\n\n"
            + source.render_body(path)
        )

    entries = "".join(f"- {wiki_link(title, label)}\n" for title, label in source.mirrors.values())
    design_map = wiki_link(DESIGN_MAP_TITLE, "L0–L3 设计地图")
    index = repo_link(source, INDEX, "主仓中文设计索引")
    pages = {
        "Home": (
            "# Shining Force II 设计 Wiki\n\n"
            + credit(INDEX)
            + f"\n{design_map}连接愿景、玩法、各系统和具体规则。\n\n"
            + entries
            + f"\n{index} · "
            + repo_link(source, "docs/design/design-map.md", "English design map")
            + "\n"
        ),
        "_Sidebar": (
            "## SF2 设计\n\n"
            + f"- {wiki_link('Home', '首页')}\n- {design_map}\n"
            + entries
            + f"\n{index}\n\n"
            + credit(INDEX)
        ),
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
