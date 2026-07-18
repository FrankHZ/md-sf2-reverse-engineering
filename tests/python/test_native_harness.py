from __future__ import annotations

import re
from pathlib import Path

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.research_index import verify_index
from sf2tool.rom import mega_drive_checksum


def test_design_contracts_are_traceable() -> None:
    assert verify_design_contracts()["Status"] == "PASS"


def test_research_index_validates_without_private_inputs() -> None:
    result = verify_index()
    assert result["Status"] == "PASS"
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 21
    assert result["AddressBindings"] == 106


def test_mega_drive_checksum_handles_an_odd_trailing_byte() -> None:
    data = bytearray(0x203)
    data[0x200:] = b"\x12\x34\x56"
    assert mega_drive_checksum(bytes(data)) == "6834"


def test_legacy_powershell_surface_does_not_expand() -> None:
    root = Path(__file__).resolve().parents[2]
    scripts = sorted((root / "scripts").rglob("*.ps1"))
    lines = sum(len(path.read_text(encoding="utf-8").splitlines()) for path in scripts)
    assert len(scripts) <= 36
    assert lines <= 4813


def test_tracked_lua_does_not_use_reserved_words_as_dot_fields() -> None:
    root = Path(__file__).resolve().parents[2]
    keywords = (
        "and|break|do|else|elseif|end|false|for|function|goto|if|in|local|nil|not|or|"
        "repeat|return|then|true|until|while"
    )
    pattern = re.compile(rf"\.\s*(?:{keywords})\b")
    failures = []
    for path in sorted((root / "tools" / "bizhawk").glob("*.lua")):
        for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            if pattern.search(line):
                failures.append(f"{path.name}:{line_number}: {line.strip()}")
    assert not failures, "Lua reserved word used after '.':\n" + "\n".join(failures)
