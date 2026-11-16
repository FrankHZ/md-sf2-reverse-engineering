"""Find a retail ROM offset or a named split in the pinned source and research ledger."""

import argparse
import json
import re
import subprocess
from collections import Counter, defaultdict
from dataclasses import dataclass
from fnmatch import fnmatchcase
from pathlib import Path

from sf2tool.paths import display_path, repo_path

DOCS = repo_path("docs/research")
LEDGER = DOCS / "rom-block-ledger.md"
MAPS = DOCS / "world/map-payload-ranges.md"
FAMILIES = DOCS / "rom-split-families.md"
ANCHOR = re.compile(r'<a id="([\w-]+)"></a>')
SPLIT = re.compile(r"#split\s+(0x[\da-fA-F]+),(0x[\da-fA-F]+),(data/[\w./-]+\.bin)")


@dataclass(frozen=True)
class Span:
    start: int
    end: int

    def contains(self, offset: int) -> bool:
        return self.start <= offset < self.end

    def __str__(self) -> str:
        return f"[0x{self.start:06X}, 0x{self.end:06X})  {self.end - self.start:,} bytes"


@dataclass(frozen=True)
class Family:
    name: str
    patterns: tuple[str, ...]
    depth: str
    owner: str
    scope: str


def routed_rows(path):
    for line in path.read_text(encoding="utf-8").splitlines():
        if line.startswith('| <a id="'):
            identity, *cells = (cell.strip() for cell in line.strip("|").split("|"))
            yield ANCHOR.match(identity)[1], cells


def read_families():
    families = []
    for name, (paths, depth, link, scope) in routed_rows(FAMILIES):
        target = re.fullmatch(r"\[[^\]]+\]\(([^)]+)\)", link)[1]
        path, _, fragment = target.partition("#")
        owner = (DOCS / path).resolve(strict=True)
        address = display_path(owner) + (f"#{fragment}" if fragment else "")
        patterns = tuple(f"data/{p}" for p in re.findall(r"`([^`]+)`", paths))
        families.append(Family(name, patterns, depth, address, scope))
    return families


def read_splits(upstream):
    source = json.loads(repo_path("manifests/toolchain.json").read_text(encoding="utf-8"))[
        "sf2disasm"
    ]
    result = subprocess.run(
        [
            "git",
            "-C",
            upstream or repo_path(source["localPath"]),
            "show",
            source["commit"] + ":split/sf2splits.txt",
        ],
        capture_output=True,
        check=True,
        encoding="utf-8",
    )
    return parse_splits(result.stdout)


def parse_splits(text):
    splits = {}
    for line in re.sub(r"/\*.*?\*/", "", text, flags=re.S).splitlines():
        directive = line.strip()
        if directive.split(maxsplit=1)[:1] != ["#split"]:
            continue
        match = SPLIT.fullmatch(directive)
        if not match:
            raise ValueError(f"invalid split directive: {directive}")
        first, last, path = match.groups()
        span = Span(int(first, 16), int(last, 16))
        if path in splits or span.start >= span.end:
            raise ValueError(f"invalid or duplicate split: {path}")
        splits[path] = span
    return splits


def number(cell):
    return int(cell.strip("`").replace(",", ""), 0)


def read_ledger(splits):
    sections, children = {}, {}
    for anchor, (first, last, length, *rest) in routed_rows(LEDGER):
        span = Span(number(first), number(last))
        if span.end - span.start != number(length):
            raise ValueError(f"length mismatch: {anchor}")
        if re.fullmatch(r"s\d\d", anchor):
            sections[span] = (anchor, number(rest[0]), number(rest[1]))
        else:
            purpose = rest[0].split("[")[0].strip()
            children[span] = f"{display_path(LEDGER)}#{anchor}  {purpose}"

    map_paths = {path for path in splits if path.startswith("data/maps/entries/")}
    for line in MAPS.read_text(encoding="utf-8").splitlines():
        if not line.startswith("| Map"):
            continue
        selectors, blocks, block_length, layout, layout_length = line.strip("|").split("|")
        owner = selectors.strip().split(",")[0].lower()
        for cell, length, kind, filename in (
            (blocks, block_length, "blocks", "0-blocks.bin"),
            (layout, layout_length, "layout", "1-layout.bin"),
        ):
            anchor = f"s08-{owner}-{kind}"
            path = f"data/maps/entries/{owner}/{filename}"
            if path not in map_paths:
                raise ValueError(f"duplicate map row or missing split: {path}")
            map_paths.remove(path)
            span = splits[path]
            expected = f"[0x{span.start:06X}, 0x{span.end:06X})"
            if (
                ANCHOR.findall(cell) != [anchor]
                or expected not in cell
                or span.end - span.start != number(length.strip())
            ):
                raise ValueError(f"map row disagrees with split: {anchor}")
            children[span] = f"{display_path(MAPS)}#{anchor} ({selectors.strip()})"
    if map_paths:
        raise ValueError(f"missing map row: {min(map_paths)}")
    return sections, children


def index_members(splits, families):
    by_span = defaultdict(list)
    by_path = {}
    for path, span in splits.items():
        matches = [f for f in families if any(fnmatchcase(path, p) for p in f.patterns)]
        if len(matches) != 1:
            raise ValueError(f"{path}: expected one family, found {[f.name for f in matches]}")
        by_path[path] = matches[0]
        by_span[span].append(path)
    if missing := set(families) - set(by_path.values()):
        raise ValueError(f"empty families: {sorted(f.name for f in missing)}")
    return by_span, by_path


def reconcile(sections, by_span):
    rom_size = json.loads(repo_path("manifests/roms/sf2-us.json").read_text())["sizeBytes"]
    previous = 0
    total_paths = 0
    for section, (name, count, size) in sections.items():
        if section.start != previous:
            raise ValueError(f"noncontiguous ledger at {name}")
        previous = section.end
        member_count = union = 0
        end = section.start
        within_section = (span for span in by_span if section.contains(span.start))
        for span in sorted(within_section, key=lambda r: r.start):
            if span.start < end or span.end > section.end:
                raise ValueError(f"unexpected partial overlap or crossing at {span}")
            end = span.end
            union += end - span.start
            member_count += len(by_span[span])
        if (member_count, union) != (count, size):
            raise ValueError(f"{name}: got {member_count} paths and {union} distinct bytes")
        total_paths += member_count
    if previous != rom_size or total_paths != sum(map(len, by_span.values())):
        raise ValueError("ROM boundary or split outside sections")


def summary(families, by_span, by_path, children, sections):
    print("Family / paths / distinct intervals / stored bytes / depth")
    counts_by_family = defaultdict(Counter)
    for span, paths in by_span.items():
        owners = {by_path[path] for path in paths}
        if len(owners) != 1:
            raise ValueError(f"aliases span different families at {span}")
        counts_by_family[owners.pop()].update(
            paths=len(paths), intervals=1, bytes=span.end - span.start
        )
    totals = defaultdict(Counter)
    for family in families:
        counts = counts_by_family[family]
        totals[family.depth].update(counts)
        print(
            f"{family.name:18} {counts['paths']:4}  {counts['intervals']:4}  "
            f"{counts['bytes']:7,}  {family.depth}"
        )
    print(
        f"{len(by_path)} paths, {len(by_span)} distinct intervals, "
        f"{sum(t['bytes'] for t in totals.values()):,} split bytes"
    )
    for depth, counts in totals.items():
        print(
            f"{depth}: {counts['paths']} paths, {counts['intervals']} intervals, "
            f"{counts['bytes']:,} stored bytes"
        )
    print(f"{len(sections)} ledger sections and {len(children)} documented subranges")
    covered = end = 0
    for span in sorted(by_span.keys() | children.keys(), key=lambda r: r.start):
        covered += max(0, span.end - max(span.start, end))
        end = max(end, span.end)
    split_bytes = sum(t["bytes"] for t in totals.values())
    rom_bytes = sum(r.end - r.start for r in sections)
    print(
        f"Outside splits: {rom_bytes - split_bytes:,} bytes, "
        f"{covered - split_bytes:,} in documented subranges, "
        f"{rom_bytes - covered:,} at section granularity"
    )
    for span, paths in by_span.items():
        if len(paths) > 1:
            print(f"aliases {span}: {', '.join(paths)}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--upstream", type=Path, help="local checkout containing the pinned object")
    query = parser.add_mutually_exclusive_group(required=True)
    query.add_argument("query", nargs="?", help="decimal/0x file offset or exact data/... path")
    query.add_argument("--family", help="list members of a family ID")
    query.add_argument("--summary", action="store_true", help="reconcile and count all families")
    args = parser.parse_args()
    try:
        splits = read_splits(args.upstream)
        families = read_families()
        sections, children = read_ledger(splits)
        by_span, by_path = index_members(splits, families)
        reconcile(sections, by_span)
        if args.summary:
            summary(families, by_span, by_path, children, sections)
            return
        if args.family is not None:
            if args.family not in {f.name for f in families}:
                parser.error(f"unknown family: {args.family}")
            for path, span in sorted(splits.items(), key=lambda item: item[1].start):
                if by_path[path].name == args.family:
                    print(span, path)
            return
        if args.query in splits:
            offset = splits[args.query].start
        else:
            try:
                offset = int(args.query, 0)
            except ValueError:
                parser.error(f"unknown path or invalid file offset: {args.query}")
        section = next((r for r in sections if r.contains(offset)), None)
        if section is None:
            parser.error("offset outside the retail ROM")
        print(f"Section {section}  {display_path(LEDGER)}#{sections[section][0]}")
        for child, link in children.items():
            if child.contains(offset):
                print(f"Documented subrange {child}  {link}")
        for span, paths in by_span.items():
            if span.contains(offset):
                print(f"Confirmed split {span}")
                for path in paths:
                    family = by_path[path]
                    print(f"  {path}")
                    print(f"  {family.depth}: {display_path(FAMILIES)}#{family.name}")
                    print(f"  {family.owner}  {family.scope}")
                return
        print("No active split at this offset. Follow the documented subrange or section layout.")
    except (OSError, ValueError, subprocess.CalledProcessError) as exc:
        parser.exit(1, f"{parser.prog}: {exc}\n")


if __name__ == "__main__":
    main()
