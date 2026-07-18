from __future__ import annotations

from pathlib import Path

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.research_index import verify_index
from sf2tool.rom import mega_drive_checksum


def test_design_contracts_are_traceable() -> None:
    assert verify_design_contracts()["Status"] == "PASS"


def test_research_index_validates_without_private_inputs() -> None:
    result = verify_index()
    assert result["Status"] == "PASS"
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 20
    assert result["AddressBindings"] == 96


def test_mega_drive_checksum_handles_an_odd_trailing_byte() -> None:
    data = bytearray(0x203)
    data[0x200:] = b"\x12\x34\x56"
    assert mega_drive_checksum(bytes(data)) == "6834"


def test_legacy_powershell_surface_does_not_expand() -> None:
    root = Path(__file__).resolve().parents[2]
    scripts = sorted((root / "scripts").rglob("*.ps1"))
    lines = sum(len(path.read_text(encoding="utf-8").splitlines()) for path in scripts)
    assert len(scripts) <= 37
    assert lines <= 5045
