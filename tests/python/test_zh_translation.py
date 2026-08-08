from __future__ import annotations

import hashlib
import json
from pathlib import Path

import pytest

from sf2tool import zh_translation
from sf2tool.zh_translation import generate_zh_translation, verify_zh_translation


def _sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _build_mini_repo(tmp_path: Path) -> tuple[Path, Path, Path]:
    design = tmp_path / "docs" / "design"
    mirror = design / "zh-CN"
    mirror.mkdir(parents=True)
    glossary = design / "glossary.md"
    glossary.write_text("# Glossary\n已确认 未知\n", encoding="utf-8")
    source_a = design / "a.md"
    source_a.write_text(
        "# A\n**Confirmed** x **Inferred** y **Unknown**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    (mirror / "a.md").write_text(
        "# A\n**已确认** x **推断** y **未知**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    (design / "b.md").write_text("# B\n", encoding="utf-8")
    return design, mirror, glossary


def _install_repo(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    design, mirror, glossary = _build_mini_repo(tmp_path)
    monkeypatch.setattr(zh_translation, "_DESIGN_ROOT", design)
    monkeypatch.setattr(zh_translation, "_MIRROR_ROOT", mirror)
    monkeypatch.setattr(zh_translation, "_GLOSSARY", glossary)
    return design, mirror, glossary


def test_generate_and_verify_roundtrip(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    result = generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    assert result["Status"] == "PASS"
    assert result["DesignDocuments"] == 2
    assert result["Translated"] == 1
    assert result["Pending"] == 1
    assert result["Stale"] == 0
    assert result["GlossaryDrift"] == 0
    assert result["MirrorDirty"] == 0
    assert result["LinksChecked"] == 0
    assert result["FixtureIdentitySequencesPreserved"] == 1
    assert result["StructuralDocuments"] == 1


def test_generate_is_deterministic(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    _install_repo(monkeypatch, tmp_path)
    first = tmp_path / "first.json"
    second = tmp_path / "second.json"
    generate_zh_translation(
        output_path=first,
        translated_date="2026-08-04",
    )
    generate_zh_translation(
        output_path=second,
        translated_date="2026-08-04",
    )
    assert first.read_bytes() == second.read_bytes()


def test_generate_preserves_nested_source_hierarchy(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, mirror, _ = _install_repo(monkeypatch, tmp_path)
    source_directory = design / "contracts"
    mirror_directory = mirror / "contracts"
    source_directory.mkdir()
    mirror_directory.mkdir()
    design.joinpath("a.md").replace(source_directory / "a.md")
    mirror.joinpath("a.md").replace(mirror_directory / "a.md")
    synthesis_source = design / "synthesis"
    synthesis_mirror = mirror / "synthesis"
    synthesis_source.mkdir()
    synthesis_mirror.mkdir()
    (synthesis_source / "a.md").write_text(
        source_directory.joinpath("a.md").read_text(encoding="utf-8"), encoding="utf-8"
    )
    (synthesis_mirror / "a.md").write_text(
        mirror_directory.joinpath("a.md").read_text(encoding="utf-8"), encoding="utf-8"
    )

    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(output_path=manifest, translated_date="2026-08-04")
    records = json.loads(manifest.read_text(encoding="utf-8"))["documents"]
    paths = {doc["source"]: doc["mirror"] for doc in records if doc["status"] == "translated"}
    assert paths["docs/design/contracts/a.md"] == "docs/design/zh-CN/contracts/a.md"
    assert paths["docs/design/synthesis/a.md"] == "docs/design/zh-CN/synthesis/a.md"


def test_verify_rejects_mirror_outside_source_hierarchy(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(output_path=manifest, translated_date="2026-08-04")
    records = json.loads(manifest.read_text(encoding="utf-8"))
    translated = next(doc for doc in records["documents"] if doc["status"] == "translated")
    translated["mirror"] = "docs/design/zh-CN/synthesis/a.md"
    manifest.write_bytes((json.dumps(records, indent=2) + "\n").encode("utf-8"))

    with pytest.raises(ValueError, match="mirror path does not match source hierarchy"):
        verify_zh_translation(manifest_path=manifest)


def test_generate_reports_stale_after_source_edit(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, _, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    design.joinpath("a.md").write_text(
        "# A revised\n**Confirmed** x **Inferred** y **Unknown**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="anchors are not current"):
        verify_zh_translation(manifest_path=manifest)
    result = verify_zh_translation(manifest_path=manifest, require_current=False)
    assert result["Status"] == "FAIL"
    assert result["Stale"] == 1
    assert result["GlossaryDrift"] == 0
    assert result["MirrorDirty"] == 0


def test_generate_requires_explicit_reanchor_after_mirror_edit(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, mirror, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    design.joinpath("a.md").write_text(
        "# A revised\n**Confirmed** x **Inferred** y **Unknown**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    mirror.joinpath("a.md").write_text(
        "# A 修订\n**已确认** x **推断** y **未知**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    original = manifest.read_bytes()
    with pytest.raises(ValueError, match="anchors are not current"):
        generate_zh_translation(output_path=manifest, translated_date="2026-08-05")
    assert manifest.read_bytes() == original
    result = verify_zh_translation(manifest_path=manifest, require_current=False)
    assert result["Status"] == "FAIL"
    assert result["Stale"] == 1
    assert result["MirrorDirty"] == 1

    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-05",
        reanchor_sources={"docs/design/a.md"},
    )
    result = verify_zh_translation(manifest_path=manifest)
    assert result["Status"] == "PASS"
    assert result["Stale"] == 0
    assert result["MirrorDirty"] == 0
    records = json.loads(manifest.read_text(encoding="utf-8"))["documents"]
    translated = next(doc for doc in records if doc["status"] == "translated")
    assert translated["translatedDate"] == "2026-08-05"
    assert translated["sourceSha256"] == _sha256(design / "a.md")
    assert translated["mirrorSha256"] == _sha256(mirror / "a.md")


def test_verify_rejects_translated_missing_mirror(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    zh_translation._MIRROR_ROOT.joinpath("a.md").unlink()
    with pytest.raises(ValueError, match="missing its mirror"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_pending_with_mirror(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _, mirror, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    mirror.joinpath("b.md").write_text("# B mirror\n", encoding="utf-8")
    with pytest.raises(ValueError, match="unexpected mirror"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_document_set_mismatch(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    records = json.loads(manifest.read_text(encoding="utf-8"))
    records["documents"] = [doc for doc in records["documents"] if doc["source"].endswith("a.md")]
    manifest.write_bytes((json.dumps(records, indent=2) + "\n").encode("utf-8"))
    with pytest.raises(ValueError, match="document set mismatch"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_new_source_missing_from_index(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, _, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    design.joinpath("c.md").write_text("# C\n", encoding="utf-8")

    with pytest.raises(
        ValueError,
        match=r"missing from index: docs/design/c\.md",
    ):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_missing_evidence_labels(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _, mirror, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    mirror.joinpath("a.md").write_text(
        "# A\nfixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n", encoding="utf-8"
    )
    with pytest.raises(ValueError, match="evidence-label"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_broken_relative_link(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _, mirror, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    mirror.joinpath("a.md").write_text(
        "# A\n**已确认** x **推断** y **未知**\n[missing](../nope.md)\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="broken relative link or anchor"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_dropped_fixture_path(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, mirror, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    design.joinpath("a.md").write_text(
        "# A\n**Confirmed** x **Inferred** y **Unknown**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n"
        "also sf2-extra-v1 tests/fixtures/h2/extra-v1.json\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="preserved fixture identity"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_rejects_glossary_drift_by_default(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, _, _ = _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    design.joinpath("glossary.md").write_text(
        "# Glossary revised\n已确认 推断 未知\n", encoding="utf-8"
    )
    with pytest.raises(ValueError, match="anchors are not current"):
        verify_zh_translation(manifest_path=manifest)
    result = verify_zh_translation(manifest_path=manifest, require_current=False)
    assert result["Status"] == "FAIL"
    assert result["GlossaryDrift"] == 1
    assert result["GlossaryIndexStale"] is True
    assert result["Stale"] == 0


def test_verify_checks_relative_heading_fragments(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, mirror, _ = _install_repo(monkeypatch, tmp_path)
    design.joinpath("target.md").write_text("# Target\n## Evidence Matrix\n", encoding="utf-8")
    mirror.joinpath("a.md").write_text(
        "# A\n**已确认** x **推断** y **未知**\n"
        "[matrix](../target.md#missing)\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n",
        encoding="utf-8",
    )
    manifest = tmp_path / "zh-index.json"
    with pytest.raises(ValueError, match="broken relative link or anchor"):
        generate_zh_translation(output_path=manifest, translated_date="2026-08-04")


def test_verify_rejects_structure_or_untranslated_mermaid_labels(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    design, mirror, _ = _install_repo(monkeypatch, tmp_path)
    design.joinpath("a.md").write_text(
        "# A\n## Flow\n**Confirmed** x **Inferred** y **Unknown**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n"
        "```mermaid\nflowchart TD\nA[\"Source state\"] --> B[\"BattleLoop\"]\n```\n",
        encoding="utf-8",
    )
    mirror.joinpath("a.md").write_text(
        "# A\n## 流程\n**已确认** x **推断** y **未知**\n"
        "fixture sf2-a-v1 tests/fixtures/h2/a-v1.json\n"
        "```mermaid\nflowchart TD\nA[\"Source state\"] --> B[\"BattleLoop\"]\n```\n",
        encoding="utf-8",
    )
    manifest = tmp_path / "zh-index.json"
    with pytest.raises(ValueError, match="untranslated Mermaid label"):
        generate_zh_translation(output_path=manifest, translated_date="2026-08-04")


def test_schema_rejects_translated_without_anchor(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    _install_repo(monkeypatch, tmp_path)
    manifest = tmp_path / "zh-index.json"
    generate_zh_translation(
        output_path=manifest,
        translated_date="2026-08-04",
    )
    records = json.loads(manifest.read_text(encoding="utf-8"))
    for doc in records["documents"]:
        if doc["status"] == "translated":
            doc.pop("sourceSha256")
    manifest.write_bytes((json.dumps(records, indent=2) + "\n").encode("utf-8"))
    with pytest.raises(ValueError, match="schema validation"):
        verify_zh_translation(manifest_path=manifest)


def test_verify_passes_on_current_repository() -> None:
    manifest = json.loads(zh_translation.MANIFEST_PATH.read_text(encoding="utf-8"))
    canonical_sources = zh_translation._translatable_sources()
    indexed_sources = [document["source"] for document in manifest["documents"]]
    translated = sum(
        document["status"] == "translated" for document in manifest["documents"]
    )
    pending = sum(document["status"] == "pending" for document in manifest["documents"])

    assert indexed_sources == canonical_sources
    result = verify_zh_translation()
    assert result["Status"] == "PASS"
    assert result["DesignDocuments"] == len(canonical_sources)
    assert result["Translated"] == translated
    assert result["Pending"] == pending
    assert result["Pending"] + result["Translated"] == len(canonical_sources)
