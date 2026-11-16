"""Protect Wiki ownership and commit a reviewed unified diff in the Wiki checkout."""

from __future__ import annotations

import difflib
import io
import re
import subprocess
from pathlib import Path, PurePosixPath

WIKI_BASE = "c027ddb07884c481cf06d20238065051d18ed093"
ENTRY_TITLES = ("Home", "_Sidebar", "Design-Documentation")
PILOT_MIRRORS = (
    (
        "docs/design/contracts/world/map-exploration.md",
        "Map-Exploration-Contract",
        "地图与探索合同",
    ),
    (
        "docs/design/synthesis/milestones/map3-battle01-readiness.md",
        "Map3-Battle01-Readiness",
        "Map 3 至 Battle 01 就绪台账",
    ),
)
SUBJECT = "Publish SF2 Wiki from "
PAGE_SUFFIXES = {
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
}


def git(repo: Path, *args: str, data: bytes | None = None) -> str:
    result = subprocess.run(
        ["git", "--no-optional-locks", "-C", str(repo), *args], input=data, capture_output=True
    )
    if result.returncode:
        raise ValueError(
            result.stderr.decode("utf-8", errors="replace").strip()
            or f"git {args[0]} failed ({result.returncode})"
        )
    return result.stdout.decode("utf-8")


def previous_pages(wiki: Path, base: str) -> set[str]:
    if not re.fullmatch(r"[0-9a-f]{40}", base):
        raise ValueError("--base requires a full reviewed Wiki commit ID")
    git(wiki, "merge-base", "--is-ancestor", WIKI_BASE, base)
    git(wiki, "merge-base", "--is-ancestor", base, "HEAD")
    if base == WIKI_BASE:
        return {*ENTRY_TITLES, *(title for _, title, _ in PILOT_MIRRORS)}
    message = git(wiki, "show", "-s", "--format=%B", base)
    if not re.fullmatch(re.escape(SUBJECT) + "[0-9a-f]{40}", message.splitlines()[0]):
        raise ValueError("--base must identify the audited pilot or a reviewed publication")
    titles = set(re.findall(r"^SF2-Wiki-Page: ([-_a-zA-Z0-9]+)$", message, re.MULTILINE))
    if not set(ENTRY_TITLES).issubset(titles):
        raise ValueError("incomplete managed page list in the Wiki publication commit")
    return titles


def check_wiki(wiki: Path, base: str, current: set[str]) -> set[str]:
    if Path(git(wiki, "rev-parse", "--show-toplevel").strip()).resolve() != wiki:
        raise ValueError("--wiki must be the root of the Wiki checkout")
    previous = previous_pages(wiki, base)
    paths = [title + ".md" for title in sorted(previous | current)]
    changed = git(wiki, "diff", "--name-only", base, "HEAD", "--", *paths)
    dirty = git(wiki, "status", "--porcelain", "--untracked-files=all", "--ignored", "--", *paths)
    if changed or dirty:
        raise ValueError(
            f"Review Wiki edits or select the last published --base:\n{changed}{dirty}"
        )

    names = git(wiki, "ls-files", "-z", "--cached", "--others", "--exclude-standard")
    owned = {title + ".md" for title in previous}
    titles = {title.casefold() for title in current}
    for name in names.rstrip("\0").split("\0"):
        path = PurePosixPath(name)
        if name in owned or path.suffix.lower() not in PAGE_SUFFIXES:
            continue
        if path.stem.replace(" ", "-").casefold() in titles:
            raise ValueError(f"Wiki title collision: {name}")
    return previous


def page_diff(wiki: Path, base: str, previous: set[str], pages: dict[str, str]) -> bytes:
    lines = []
    for title in sorted(previous | pages.keys()):
        name = title + ".md"
        before = git(wiki, "show", f"{base}:{name}") if title in previous else ""
        after = pages.get(title, "")
        diff = difflib.unified_diff(
            io.StringIO(before).readlines(),
            io.StringIO(after).readlines(),
            fromfile=f"a/{name}" if title in previous else "/dev/null",
            tofile=f"b/{name}" if title in pages else "/dev/null",
        )
        for line in diff:
            lines.append(line if line.endswith("\n") else line + "\n\\ No newline at end of file\n")
    return "".join(lines).encode("utf-8")


def apply_review(wiki: Path, source: str, pages: dict[str, str], patch: bytes, review: Path) -> str:
    if review.read_bytes() != patch:
        raise ValueError("reviewed diff differs from the current preview; review again")
    if not patch:
        return "No managed page changes."
    git(wiki, "symbolic-ref", "--quiet", "HEAD")
    if git(wiki, "diff", "--cached", "--name-only"):
        raise ValueError("commit or unstage existing staged changes before applying the preview")
    trailers = "\n".join(f"SF2-Wiki-Page: {title}" for title in sorted(pages))
    git(wiki, "apply", "--index", "--whitespace=nowarn", "-", data=patch)
    git(wiki, "commit", "-m", SUBJECT + source, "-m", trailers)
    return git(wiki, "rev-parse", "HEAD").strip()
