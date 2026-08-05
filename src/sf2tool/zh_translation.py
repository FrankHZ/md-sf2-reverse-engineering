from __future__ import annotations

import hashlib
import json
import re
from datetime import date
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import REPO_ROOT

MANIFEST_PATH = REPO_ROOT / "manifests" / "zh-translation-index.json"
SCHEMA_PATH = REPO_ROOT / "schemas" / "core" / "zh-translation-index.schema.json"
_DESIGN_ROOT = REPO_ROOT / "docs" / "design"
_MIRROR_ROOT = REPO_ROOT / "docs" / "design" / "zh-CN"
_GLOSSARY = _DESIGN_ROOT / "glossary.md"

# The glossary is the translation key itself, never a mirror target.
NON_TRANSLATABLE = {"glossary.md"}

# R1 fixed evidence-label translations required in every mirror.
REQUIRED_LABELS = ("已确认", "未知")
# R2 preserved fixture-path class that a mirror must not drop.
_FIXTURE_PATH_RE = re.compile(r"tests/fixtures/[\w./-]+\.json")
_LINK_RE = re.compile(r"\[[^\]]*\]\(([^)#]+)")
_SCHEME_RE = re.compile(r"^[a-z][a-z0-9+.-]*:")


def _sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _source_path(source: str) -> Path:
    return _DESIGN_ROOT / Path(source).name


def _mirror_path(source: str) -> Path:
    return _MIRROR_ROOT / Path(source).name


def _translatable_sources() -> list[str]:
    if not _DESIGN_ROOT.is_dir():
        raise ValueError(f"missing design root: {_DESIGN_ROOT}")
    sources = [
        f"docs/design/{path.name}"
        for path in sorted(_DESIGN_ROOT.glob("*.md"))
        if path.name not in NON_TRANSLATABLE
    ]
    return sources


def _markdown_links(text: str) -> list[str]:
    return [
        target
        for target in _LINK_RE.findall(text)
        if not _SCHEME_RE.match(target)
    ]


def _relative_links_resolve(source: str, mirror_text: str) -> tuple[bool, int]:
    """Resolve every relative link from the mirror's own directory."""
    mirror_dir = _mirror_path(source).parent
    targets = _markdown_links(mirror_text)
    for target in targets:
        if not (mirror_dir / target).resolve().exists():
            return False, len(targets)
    return True, len(targets)


def _fixture_paths_preserved(source_text: str, mirror_text: str) -> list[str]:
    missing = sorted(
        {path for path in _FIXTURE_PATH_RE.findall(source_text) if path not in mirror_text}
    )
    return missing


def _evidence_labels_present(mirror_text: str) -> bool:
    return all(label in mirror_text for label in REQUIRED_LABELS)


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
        "mirror": f"docs/design/zh-CN/{mirror.name}",
        "sourceSha256": _sha256(_source_path(source)),
        "mirrorSha256": _sha256(mirror),
    }


def generate_zh_translation(
    output_path: Path | None = None,
    *,
    glossary_sha256: str | None = None,
    translated_date: str | None = None,
) -> dict[str, Any]:
    """Regenerate the zh-CN translation index from the repository files.

    A translated mirror whose content is unchanged since its recorded anchor keeps
    that anchor, so English-source and glossary drift stay visible. A new or re-edited
    mirror is re-anchored to the current files and today's date.
    """
    output_path = output_path or MANIFEST_PATH
    previous = load_json(MANIFEST_PATH) if MANIFEST_PATH.is_file() else {"documents": []}
    glossary_hash = glossary_sha256 or _sha256(_GLOSSARY)
    today = translated_date or date.today().isoformat()

    documents: list[dict[str, Any]] = []
    for source in _translatable_sources():
        current = _compute_status(source)
        if current["status"] == "pending":
            documents.append({"source": source, "status": "pending"})
            continue
        prior = _record_by_source(previous, source)
        if prior and prior.get("mirrorSha256") == current["mirrorSha256"]:
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
    payload = (json.dumps(manifest, indent=2, ensure_ascii=False) + "\n").encode("utf-8")
    output_path.write_bytes(payload)
    return verify_zh_translation(manifest_path=output_path, manifest=manifest)


def verify_zh_translation(
    manifest_path: Path | None = None,
    *,
    manifest: dict[str, Any] | None = None,
) -> dict[str, Any]:
    """Validate the zh-CN translation index against the repository files.

    Structural and mirror-quality violations raise. English-source staleness,
    glossary drift, mirror edits, and a stale top-level glossary hash are reported
    as counts so a translation batch can see what needs re-translation.
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
    fixture_paths_preserved = 0

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
        if mirror.name != Path(source).name:
            raise ValueError(
                f"mirror filename does not match source: {source} vs {mirror.name}"
            )
        mirror_text = mirror.read_text(encoding="utf-8")
        if not _evidence_labels_present(mirror_text):
            raise ValueError(f"mirror lacks required evidence labels: {source}")
        links_ok, link_count = _relative_links_resolve(source, mirror_text)
        links_checked += link_count
        if not links_ok:
            raise ValueError(f"mirror contains a broken relative link: {source}")
        missing_fixtures = _fixture_paths_preserved(source_text, mirror_text)
        if missing_fixtures:
            raise ValueError(
                f"mirror dropped fixture path(s) from {source}: "
                + ", ".join(missing_fixtures)
            )
        fixture_paths_preserved += 1

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
    return {
        "Manifest": manifest_label,
        "DesignDocuments": len(actual_sources),
        "Translated": translated,
        "Pending": pending,
        "Stale": stale,
        "GlossaryDrift": glossary_drift,
        "MirrorDirty": mirror_dirty,
        "GlossaryIndexStale": glossary_index_stale,
        "LinksChecked": links_checked,
        "FixturePathsPreserved": fixture_paths_preserved,
        "Status": "PASS",
    }


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
