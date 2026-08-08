from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from collections.abc import Collection
from datetime import date
from pathlib import Path
from typing import Any
from urllib.parse import unquote

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import REPO_ROOT

MANIFEST_PATH = REPO_ROOT / "manifests" / "zh-translation-index.json"
SCHEMA_PATH = REPO_ROOT / "schemas" / "core" / "zh-translation-index.schema.json"
_DESIGN_ROOT = REPO_ROOT / "docs" / "design"
_MIRROR_ROOT = REPO_ROOT / "docs" / "design" / "zh-CN"
_GLOSSARY = _DESIGN_ROOT / "glossary.md"
_DESIGN_PREFIX = Path("docs/design")

# The glossary is the translation key itself, never a mirror target.
NON_TRANSLATABLE = {"docs/design/glossary.md"}

# R1 fixed evidence-label translations. A mirror must retain every label class
# used by its English source, but it may add labels where Chinese prose needs an
# explicit boundary.
_EVIDENCE_LABELS = {
    "Confirmed": "已确认",
    "Inferred": "推断",
    "Unknown": "未知",
}
# R2 preserved fixture identity classes that a mirror must not drop or reorder.
_FIXTURE_ID_RE = re.compile(r"\bsf2-[a-z0-9-]+-v[0-9]+\b")
_FIXTURE_PATH_RE = re.compile(r"tests/fixtures/[\w./-]+\.json")
_LINK_RE = re.compile(r"(?<!!)\[[^\]]*\]\(([^)]+)\)")
_SCHEME_RE = re.compile(r"^[a-z][a-z0-9+.-]*:")
_HEADING_RE = re.compile(r"^(#{1,6})\s+(.+?)\s*#*\s*$")
_FENCE_RE = re.compile(r"^\s*(```|~~~)(.*)$")
_MERMAID_EDGE_RE = re.compile(r"(?:-->|-\.->|-\.[^\r\n]*\.->|==>)")
_HAN_RE = re.compile(r"[\u3400-\u9fff]")
_SOURCE_LABEL_RE = re.compile(r"^[A-Za-z][A-Za-z0-9_]*$")
_MERMAID_LABEL_RE = re.compile(r'"([^"]+)"')
_MERMAID_SOURCE_LABEL_ALLOWLIST = {"EGRESS / Angel Wing"}


def _sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _relative_source_path(source: str) -> Path:
    path = Path(source)
    try:
        relative = path.relative_to(_DESIGN_PREFIX)
    except ValueError as exc:
        raise ValueError(f"zh source is outside docs/design: {source}") from exc
    if (
        not relative.parts
        or relative.parts[0] == "zh-CN"
        or any(part in {"", ".", ".."} for part in relative.parts)
    ):
        raise ValueError(f"zh source is not a canonical English design document: {source}")
    return relative


def _source_path(source: str) -> Path:
    return _DESIGN_ROOT / _relative_source_path(source)


def _mirror_path(source: str) -> Path:
    return _MIRROR_ROOT / _relative_source_path(source)


def _translatable_sources() -> list[str]:
    if not _DESIGN_ROOT.is_dir():
        raise ValueError(f"missing design root: {_DESIGN_ROOT}")
    sources: list[str] = []
    for path in _DESIGN_ROOT.rglob("*.md"):
        if _MIRROR_ROOT in path.parents:
            continue
        source = (_DESIGN_PREFIX / path.relative_to(_DESIGN_ROOT)).as_posix()
        if source not in NON_TRANSLATABLE:
            sources.append(source)
    return sorted(sources)


def _markdown_links(text: str) -> list[str]:
    targets: list[str] = []
    for raw_target in _LINK_RE.findall(text):
        target = raw_target.strip()
        if target.startswith("<") and ">" in target:
            target = target[1 : target.index(">")]
        else:
            target = target.split(maxsplit=1)[0]
        if not _SCHEME_RE.match(target):
            targets.append(target)
    return targets


def _heading_slug(value: str) -> str:
    value = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", value)
    value = re.sub(r"[`*_~]", "", value).strip().lower()
    value = re.sub(r"[^\w\u3400-\u9fff -]", "", value)
    value = re.sub(r"\s+", "-", value)
    return re.sub(r"-+", "-", value).strip("-")


def _heading_anchors(text: str) -> set[str]:
    anchors: set[str] = set()
    counts: Counter[str] = Counter()
    in_fence = False
    fence = ""
    for line in text.splitlines():
        fence_match = _FENCE_RE.match(line)
        if fence_match:
            marker = fence_match.group(1)
            if not in_fence:
                in_fence = True
                fence = marker
            elif marker == fence:
                in_fence = False
                fence = ""
            continue
        if in_fence:
            continue
        match = _HEADING_RE.match(line)
        if not match:
            continue
        base = _heading_slug(match.group(2))
        if not base:
            continue
        ordinal = counts[base]
        counts[base] += 1
        anchors.add(base if ordinal == 0 else f"{base}-{ordinal}")
    return anchors


def _relative_link_issues(source: str, mirror_text: str) -> tuple[list[str], int]:
    """Resolve every relative path and heading fragment from the mirror directory."""
    mirror_dir = _mirror_path(source).parent
    targets = _markdown_links(mirror_text)
    issues: list[str] = []
    for target in targets:
        path_text, separator, fragment = target.partition("#")
        target_path = _mirror_path(source) if not path_text else mirror_dir / path_text
        target_path = target_path.resolve()
        if not target_path.exists():
            issues.append(target)
            continue
        if separator:
            if not target_path.is_file():
                issues.append(target)
                continue
            anchors = _heading_anchors(target_path.read_text(encoding="utf-8"))
            if unquote(fragment).lower() not in anchors:
                issues.append(target)
    return issues, len(targets)


def _preserved_sequence_issues(source_text: str, mirror_text: str) -> list[str]:
    issues: list[str] = []
    source_ids = _FIXTURE_ID_RE.findall(source_text)
    mirror_ids = _FIXTURE_ID_RE.findall(mirror_text)
    if source_ids != mirror_ids:
        issues.append("fixture ID sequence")
    source_paths = _FIXTURE_PATH_RE.findall(source_text)
    mirror_paths = _FIXTURE_PATH_RE.findall(mirror_text)
    if source_paths != mirror_paths:
        issues.append("fixture path sequence")
    return issues


def _evidence_label_issues(source_text: str, mirror_text: str) -> list[str]:
    issues: list[str] = []
    for english, chinese in _EVIDENCE_LABELS.items():
        source_count = len(re.findall(rf"\*\*{re.escape(english)}", source_text))
        mirror_count = len(re.findall(rf"\*\*{re.escape(chinese)}", mirror_text))
        if mirror_count < source_count:
            issues.append(f"{english}={source_count}, {chinese}={mirror_count}")
    return issues


def _markdown_structure(text: str) -> dict[str, Any]:
    headings: list[int] = []
    table_rows = 0
    fence_count = 0
    mermaid_blocks = 0
    mermaid_edges = 0
    mermaid_labels: list[str] = []
    in_fence = False
    fence = ""
    in_mermaid = False
    for line in text.splitlines():
        fence_match = _FENCE_RE.match(line)
        if fence_match:
            marker = fence_match.group(1)
            if not in_fence:
                in_fence = True
                fence = marker
                in_mermaid = fence_match.group(2).strip().lower() == "mermaid"
                if in_mermaid:
                    mermaid_blocks += 1
            elif marker == fence:
                in_fence = False
                fence = ""
                in_mermaid = False
            fence_count += 1
            continue
        if in_mermaid:
            mermaid_edges += len(_MERMAID_EDGE_RE.findall(line))
            mermaid_labels.extend(_MERMAID_LABEL_RE.findall(line))
            continue
        if in_fence:
            continue
        heading = _HEADING_RE.match(line)
        if heading:
            headings.append(len(heading.group(1)))
        stripped = line.strip()
        if stripped.startswith("|") and stripped.endswith("|"):
            table_rows += 1
    return {
        "headingLevels": headings,
        "tableRows": table_rows,
        "fences": fence_count,
        "mermaidBlocks": mermaid_blocks,
        "mermaidEdges": mermaid_edges,
        "mermaidLabels": mermaid_labels,
    }


def _structure_issues(source_text: str, mirror_text: str) -> list[str]:
    source = _markdown_structure(source_text)
    mirror = _markdown_structure(mirror_text)
    issues = [
        key
        for key in ("headingLevels", "tableRows", "fences", "mermaidBlocks", "mermaidEdges")
        if source[key] != mirror[key]
    ]
    untranslated = [
        label
        for label in mirror["mermaidLabels"]
        if not _HAN_RE.search(label)
        and not _SOURCE_LABEL_RE.fullmatch(label)
        and label not in _MERMAID_SOURCE_LABEL_ALLOWLIST
    ]
    if untranslated:
        issues.append("untranslated Mermaid label(s): " + ", ".join(untranslated))
    return issues


def _record_by_source(manifest: dict[str, Any], source: str) -> dict[str, Any] | None:
    return next(
        (
            document
            for document in manifest.get("documents", [])
            if document.get("source") == source
        ),
        None,
    )


def _compute_status(source: str) -> dict[str, Any]:
    """Compute the current on-disk state for one translatable source."""
    mirror = _mirror_path(source)
    if not mirror.is_file():
        return {"source": source, "status": "pending"}
    return {
        "source": source,
        "status": "translated",
        "mirror": f"docs/design/zh-CN/{_relative_source_path(source).as_posix()}",
        "sourceSha256": _sha256(_source_path(source)),
        "mirrorSha256": _sha256(mirror),
    }


def generate_zh_translation(
    output_path: Path | None = None,
    *,
    glossary_sha256: str | None = None,
    translated_date: str | None = None,
    reanchor_sources: Collection[str] = (),
) -> dict[str, Any]:
    """Regenerate the zh-CN translation index from the repository files.

    Existing translated records retain their accepted anchors by default, even when
    their source, mirror, or glossary changed. A caller must explicitly name each
    reviewed source that may be re-anchored. Newly added mirrors receive their first
    anchor automatically.
    """
    output_path = output_path or MANIFEST_PATH
    previous = load_json(output_path) if output_path.is_file() else {"documents": []}
    glossary_hash = glossary_sha256 or _sha256(_GLOSSARY)
    today = translated_date or date.today().isoformat()
    requested = set(reanchor_sources)
    actual_sources = set(_translatable_sources())
    unknown = sorted(requested - actual_sources)
    if unknown:
        raise ValueError("cannot re-anchor unknown zh source(s): " + ", ".join(unknown))

    documents: list[dict[str, Any]] = []
    for source in _translatable_sources():
        current = _compute_status(source)
        if current["status"] == "pending":
            if source in requested:
                raise ValueError(f"cannot re-anchor a source without a mirror: {source}")
            documents.append({"source": source, "status": "pending"})
            continue
        prior = _record_by_source(previous, source)
        if prior and prior.get("status") == "translated" and source not in requested:
            documents.append(prior)
        else:
            documents.append(
                {
                    "source": source,
                    "status": "translated",
                    "mirror": current["mirror"],
                    "sourceSha256": current["sourceSha256"],
                    "mirrorSha256": current["mirrorSha256"],
                    "glossarySha256": glossary_hash,
                    "translatedDate": today,
                }
            )

    manifest = {
        "schemaVersion": 1,
        "glossary": {"path": "docs/design/glossary.md", "sha256": glossary_hash},
        "mirrorRoot": "docs/design/zh-CN",
        "documents": documents,
    }
    result = verify_zh_translation(
        manifest_path=output_path,
        manifest=manifest,
        require_current=True,
    )
    payload = (json.dumps(manifest, indent=2, ensure_ascii=False) + "\n").encode("utf-8")
    output_path.write_bytes(payload)
    return result


def verify_zh_translation(
    manifest_path: Path | None = None,
    *,
    manifest: dict[str, Any] | None = None,
    require_current: bool = True,
) -> dict[str, Any]:
    """Validate the zh-CN translation index against the repository files.

    Structural and mirror-quality violations always raise. By default, stale source
    anchors, glossary drift, mirror edits, and a stale top-level glossary hash also
    fail the acceptance gate. Callers that only need a diagnostic report may set
    ``require_current=False`` and inspect the returned FAIL status and counts.
    """
    manifest_path = manifest_path or MANIFEST_PATH
    index = load_json(manifest_path) if manifest is None else manifest
    validate_json(index, SCHEMA_PATH, owner="zh translation index")

    current_glossary_hash = _sha256(_GLOSSARY)
    indexed_sources = [document["source"] for document in index["documents"]]
    actual_sources = _translatable_sources()
    if indexed_sources != actual_sources:
        missing = sorted(set(actual_sources) - set(indexed_sources))
        extra = sorted(set(indexed_sources) - set(actual_sources))
        detail: list[str] = []
        if missing:
            detail.append("missing from index: " + ", ".join(missing))
        if extra:
            detail.append("not translatable: " + ", ".join(extra))
        raise ValueError("zh translation document set mismatch: " + "; ".join(detail))

    translated = 0
    pending = 0
    stale = 0
    glossary_drift = 0
    mirror_dirty = 0
    links_checked = 0
    fixture_pairs_preserved = 0
    fixture_ids_checked = 0
    fixture_paths_checked = 0
    structural_documents = 0
    headings_checked = 0
    table_rows_checked = 0
    fences_checked = 0
    mermaid_edges_checked = 0

    for document in index["documents"]:
        source = document["source"]
        source_text = _source_path(source).read_text(encoding="utf-8")
        if document["status"] == "pending":
            pending += 1
            if _mirror_path(source).is_file():
                raise ValueError(f"pending document has an unexpected mirror: {source}")
            continue

        translated += 1
        mirror = _mirror_path(source)
        if not mirror.is_file():
            raise ValueError(f"translated document is missing its mirror: {source}")
        expected_mirror = f"docs/design/zh-CN/{_relative_source_path(source).as_posix()}"
        if document["mirror"] != expected_mirror:
            raise ValueError(
                f"indexed mirror path does not match source hierarchy: "
                f"{document['mirror']} vs {expected_mirror}"
            )
        if mirror.name != Path(source).name:
            raise ValueError(
                f"mirror filename does not match source: {source} vs {mirror.name}"
            )
        mirror_text = mirror.read_text(encoding="utf-8")
        evidence_issues = _evidence_label_issues(source_text, mirror_text)
        if evidence_issues:
            raise ValueError(
                f"mirror lacks required evidence-label occurrences for {source}: "
                + "; ".join(evidence_issues)
            )
        link_issues, link_count = _relative_link_issues(source, mirror_text)
        links_checked += link_count
        if link_issues:
            raise ValueError(
                f"mirror contains broken relative link or anchor in {source}: "
                + ", ".join(link_issues)
            )
        preserved_issues = _preserved_sequence_issues(source_text, mirror_text)
        if preserved_issues:
            raise ValueError(
                f"mirror changed preserved fixture identity in {source}: "
                + ", ".join(preserved_issues)
            )
        fixture_pairs_preserved += 1
        fixture_ids_checked += len(_FIXTURE_ID_RE.findall(source_text))
        fixture_paths_checked += len(_FIXTURE_PATH_RE.findall(source_text))
        structure_issues = _structure_issues(source_text, mirror_text)
        if structure_issues:
            raise ValueError(
                f"mirror structure differs from {source}: " + "; ".join(structure_issues)
            )
        structural_documents += 1
        mirror_structure = _markdown_structure(mirror_text)
        headings_checked += len(mirror_structure["headingLevels"])
        table_rows_checked += mirror_structure["tableRows"]
        fences_checked += mirror_structure["fences"]
        mermaid_edges_checked += mirror_structure["mermaidEdges"]

        current_source_hash = _sha256(_source_path(source))
        current_mirror_hash = _sha256(mirror)
        if current_source_hash != document["sourceSha256"]:
            stale += 1
        if current_glossary_hash != document["glossarySha256"]:
            glossary_drift += 1
        if current_mirror_hash != document["mirrorSha256"]:
            mirror_dirty += 1

    glossary_index_stale = index["glossary"]["sha256"] != current_glossary_hash
    manifest_label = (
        manifest_path.relative_to(REPO_ROOT).as_posix()
        if manifest_path.is_relative_to(REPO_ROOT)
        else str(manifest_path)
    )
    current = not (stale or glossary_drift or mirror_dirty or glossary_index_stale)
    result = {
        "Manifest": manifest_label,
        "DesignDocuments": len(actual_sources),
        "Translated": translated,
        "Pending": pending,
        "Stale": stale,
        "GlossaryDrift": glossary_drift,
        "MirrorDirty": mirror_dirty,
        "GlossaryIndexStale": glossary_index_stale,
        "LinksChecked": links_checked,
        "FixtureIdentitySequencesPreserved": fixture_pairs_preserved,
        "FixtureIdsChecked": fixture_ids_checked,
        "FixturePathsChecked": fixture_paths_checked,
        "StructuralDocuments": structural_documents,
        "HeadingsChecked": headings_checked,
        "TableRowsChecked": table_rows_checked,
        "FencesChecked": fences_checked,
        "MermaidEdgesChecked": mermaid_edges_checked,
        "Status": "PASS" if current else "FAIL",
    }
    if require_current and not current:
        raise ValueError(
            "zh translation anchors are not current: "
            f"stale={stale}, glossaryDrift={glossary_drift}, "
            f"mirrorDirty={mirror_dirty}, glossaryIndexStale={glossary_index_stale}; "
            "review the affected mirrors and re-anchor them explicitly"
        )
    return result


def translation_rows() -> list[dict[str, Any]]:
    """Return one row per indexed document for the zh-meta list command."""
    index = load_json(MANIFEST_PATH)
    current_glossary_hash = _sha256(_GLOSSARY)
    rows: list[dict[str, Any]] = []
    for document in index["documents"]:
        row: dict[str, Any] = {
            "Source": document["source"],
            "Status": document["status"],
        }
        if document["status"] == "pending":
            rows.append(row)
            continue
        mirror = _mirror_path(document["source"])
        source = _source_path(document["source"])
        row["Translated"] = document["translatedDate"]
        row["Stale"] = _sha256(source) != document["sourceSha256"]
        row["GlossaryDrift"] = current_glossary_hash != document["glossarySha256"]
        row["MirrorDirty"] = _sha256(mirror) != document["mirrorSha256"]
        rows.append(row)
    return rows
