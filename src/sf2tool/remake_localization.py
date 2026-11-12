"""Derive the selected private campaign display catalogs; never edit corpus inputs.

Paths are relative to this checkout, independent of the caller's working directory.
The ignored output directory must be fresh. Selection authority is the tracked
remake/docs/application/pre-voyage-chinese-admission.md, not an ignored inventory.
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path

from sf2tool.jsonio import load_json
from sf2tool.paths import REPO_ROOT
from sf2tool.source_text import read_upstream_text

_PRE_VOYAGE_RANGES = (
    (363, 363),
    (403, 405),
    (408, 410),
    (412, 412),
    (423, 423),
    (425, 430),
    (446, 447),
    (480, 505),
    (507, 573),
    (575, 581),
    (583, 586),
    (588, 596),
    (598, 617),
    (620, 651),
    (657, 789),
    (810, 842),
    (844, 878),
    (880, 1020),
    (2176, 2254),
    (2292, 2460),
    (4035, 4059),
    (4065, 4073),
)
_RANGES = (*_PRE_VOYAGE_RANGES, (2261, 2263))
NAME_IDS = {
    "name.ally": (0, 1, 2, 3, 4, 5, 6, 25, 28),
    "name.class": (0, 1, 2, 3, 4, 10, 11),
    "name.item": (0, 1, 2, 3, 4, 33, 45, 56, 57, 71, 72, 73, 85, 86, 103),
    "name.spell": (0, 2, 4, 6, 7, 8, 10, 11, 13, 14),
    "name.enemy": (0, 1, 2, 3, 39, 40, 56, 67, 76, 86),
}
_CALIBRATED = (
    447,
    480,
    483,
    487,
    491,
    495,
    497,
    504,
    520,
    538,
    544,
    545,
    551,
    553,
    560,
    568,
    569,
    579,
    590,
    591,
    2178,
    2187,
    2192,
    2193,
    2197,
    2203,
    2206,
    2211,
    2212,
    2299,
    2304,
    4044,
    4048,
    4052,
)
_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_ROM = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_CONTROL = re.compile(r"\{(?:N|W1|W2|D1|CLEAR)\}")
_TOKEN = re.compile(r"\{[^{}]+\}")
_SUPPORTED = re.compile(r"\{(?:N|W1|W2|D1|CLEAR|LEADER|NAME(?:;[0-9]+)?|CLASS)\}")


def _require(ok: bool, message: str) -> None:
    if not ok:
        raise ValueError(message)


def _path(value: str) -> Path:
    return (REPO_ROOT / value).resolve()


def _jsonl(path: Path, key) -> dict:
    rows = [json.loads(line) for line in path.read_text(encoding="utf-8").splitlines()]
    result = {key(row): row for row in rows}
    _require(len(result) == len(rows), "duplicate corpus identity")
    return result


def _topology(source: str, target: str) -> bool:
    return (
        _CONTROL.findall(source) == _CONTROL.findall(target)
        and [Counter(_TOKEN.findall(s)) for s in _CONTROL.split(source)]
        == [Counter(_TOKEN.findall(s)) for s in _CONTROL.split(target)]
        and not re.search(r"[{}]", _TOKEN.sub("", target))
    )


def _entry(value: str) -> str:
    result = value.removeprefix("{CLEAR}").removesuffix("{W2}").replace("{N}", "\n")
    _require(not re.search(r"[{}]", result), "entry boundary changed")
    return result


def derive(args: argparse.Namespace) -> dict:
    output = args.output
    _require(
        output.is_relative_to(REPO_ROOT / "local") and not output.exists(),
        "output must be a fresh directory beneath this checkout's local/",
    )
    ignored = subprocess.run(
        ["git", "-C", str(REPO_ROOT), "check-ignore", "--quiet", str(output)], check=False
    )
    _require(ignored.returncode == 0, "output must be ignored")
    inputs = [
        args.world,
        args.corpus,
        args.names,
        args.entry,
        args.previous_world,
        args.previous_entry,
    ]
    before = [(p.stat().st_size, p.stat().st_mtime_ns) for p in inputs]
    head = subprocess.run(
        ["git", "-C", str(args.upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()
    _require(head == _COMMIT, "upstream commit mismatch")
    world = load_json(args.world)  # One existing world parse; no media copy or regeneration.
    pin = {k: world["provenance"][k] for k in ("repository", "commit", "romSha256")}
    _require(
        pin
        == {
            "repository": "https://github.com/ShiningForceCentral/SF2DISASM.git",
            "commit": _COMMIT,
            "romSha256": _ROM,
        },
        "world source mismatch",
    )
    rows = _jsonl(args.corpus, lambda r: r["id"])
    names = _jsonl(args.names, lambda r: (r["namespace"], r["index"]))
    _require(set(rows) == set(range(4267)) and len(names) == 337, "corpus identity set changed")
    originals = {r["id"]: r["text"] for r in world["world"]["texts"]}
    pinned = {
        int(m[1], 16): m[2]
        for line in read_upstream_text(
            args.upstream / "disasm/data/scripting/text/gamescript.txt"
        ).splitlines()
        if (m := re.fullmatch(r"([0-9A-Fa-f]{4})=(.*)", line))
    }
    _require(
        all(rows[i]["source"] == originals[i] == pinned[i] for i in rows), "source text mismatch"
    )
    package = world["package"]
    del world
    selected = []
    ranges = _PRE_VOYAGE_RANGES if args.scope == "pre-voyage" else _RANGES
    world_ids = sorted(i for start, end in ranges for i in range(start, end + 1))
    for i in world_ids:
        row = rows[i]
        source, target = row["source"], row["translation"]
        _require(row["namespace"] == "world.text" and isinstance(target, str), f"invalid row {i}")
        _require(_topology(source, target), f"control/parameter mismatch {i}")
        _require(
            all(_SUPPORTED.fullmatch(t) for t in _TOKEN.findall(source)), f"unsupported token {i}"
        )
        _require(len(target.encode("utf-16-le")) // 2 <= 8192, f"message too long {i}")
        selected.append({"namespace": "world.text", "id": i, "text": target})
    selected_names = []
    for space, indices in sorted(NAME_IDS.items()):
        for i in indices:
            row = names[space, i]
            target = row["translation"]
            lines = read_upstream_text(args.upstream / row["source_file"]).splitlines()
            _require(
                row["source_expression"] in lines[row["source_line"] - 1],
                f"name source {space}/{i}",
            )
            _require(
                isinstance(target, str)
                and target.strip() != ""
                and len(target.encode("utf-16-le")) // 2 <= 128
                and not re.search(r"[{}\x00-\x1f\x7f-\x9f\u2028\u2029]", target),
                f"invalid name {space}/{i}",
            )
            selected_names.append({"namespace": space, "index": i, "text": target})
    previous = load_json(args.previous_world)
    _require(
        previous["package"] == package
        and previous["source"] == pin
        and len(previous["texts"]) == 209,
        "historical world identity mismatch",
    )
    old = {r["id"]: r["text"] for r in previous["texts"]}
    _require(len(old) == 209 and set(old) <= set(world_ids), "historical subset mismatch")
    changed = sorted(i for i in old if old[i] != rows[i]["translation"])
    _require(tuple(changed) == _CALIBRATED, "unexpected historical translation change")
    review = [
        {
            "id": i,
            "source": rows[i]["source"],
            "old": old[i],
            "new": rows[i]["translation"],
            "reason": (
                "Accepted full-corpus name/title calibration; preserved source control topology."
            ),
        }
        for i in changed
    ]
    entry_source = load_json(args.entry)
    old_entry = load_json(args.previous_entry)
    _require(
        old_entry["package"] == entry_source["profile"]
        and old_entry["source"]["commit"] == entry_source["upstreamCommit"]
        and old_entry["source"]["romSha256"] == entry_source["romSha256"],
        "entry source mismatch",
    )
    entry_rows = [
        {"namespace": "entry.text", "id": i, "text": _entry(rows[i]["translation"])}
        for i in range(216, 222)
    ]
    _require(
        entry_rows == old_entry["texts"]
        and all(_topology(rows[i]["source"], rows[i]["translation"]) for i in range(216, 222))
        and all(
            _entry(rows[i]["source"]) == entry_source["dialogue"][str(i)] for i in range(216, 222)
        ),
        "approved entry aliases changed",
    )
    catalog = {
        "package": package,
        "locale": "zh-CN",
        "source": pin,
        "texts": selected,
        "names": selected_names,
    }
    entry_catalog = {**old_entry, "texts": entry_rows}
    texts = defaultdict(list)
    for row in selected:
        texts[row["text"]].append(row["id"])
    heuristic = {
        "visiblePlaceholderIds": [
            r["id"] for r in selected if re.search(r"TODO|空白占位|\(blank\)", r["text"], re.I)
        ],
        "literalControlIds": [
            r["id"] for r in selected if re.search(r"[\x00-\x08\x0b-\x1f\x7f-\x9f]", r["text"])
        ],
        "rawSymbolIds": [
            r["id"]
            for r in selected
            if re.search(r"\b(?:cs_|byte_|word_|loc_)[0-9A-Fa-f]+\b", r["text"])
        ],
        "repeatedLongTargets": [
            ids
            for target, ids in texts.items()
            if len(target) > 16 and len({rows[i]["source"] for i in ids}) > 2
        ],
    }
    _require(not any(heuristic.values()), "selected heuristic QA needs review")
    files = {
        "world.zh-CN.json": catalog,
        "entry.zh-CN.json": entry_catalog,
        "historical-review.json": review,
        "heuristic.json": heuristic,
    }
    encoded = {
        name: (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
        for name, value in files.items()
    }
    _require(
        len(encoded["world.zh-CN.json"]) <= 256 * 1024
        and len(selected) == (806 if args.scope == "pre-voyage" else 809)
        and len(selected_names) == 51,
        "candidate budget/selection mismatch",
    )
    _require(
        before == [(p.stat().st_size, p.stat().st_mtime_ns) for p in inputs],
        "input changed during selection",
    )
    output.mkdir(parents=True)
    for name, value in encoded.items():
        (output / name).write_bytes(value)
    return {
        "messages": len(selected),
        "names": len(selected_names),
        "entryAliases": 6,
        "historicalChanges": changed,
        "bytes": {k: len(v) for k, v in encoded.items()},
        "strict": "passed",
        "heuristic": heuristic,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for key in (
        "world",
        "corpus",
        "names",
        "entry",
        "upstream",
        "previous-world",
        "previous-entry",
        "output",
    ):
        parser.add_argument("--" + key, required=True, type=_path)
    parser.add_argument("--scope", choices=("current", "pre-voyage"), default="current")
    args = parser.parse_args()
    try:
        result = derive(args)
    except (OSError, ValueError, KeyError, TypeError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Catalog selection failed: {error}\n")
    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
