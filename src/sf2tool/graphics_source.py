"""Pinned source identity and canonical-ROM parity for private graphics readers."""

import hashlib
from dataclasses import dataclass
from pathlib import Path

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses


@dataclass(frozen=True)
class ProvenanceBoundSource:
    """Read ignored upstream payloads only after exact canonical-ROM parity.

    The pinned checkout identity covers tracked source text, but SF2DISASM ignores the
    split ``*.bin`` payloads.  Every renderer therefore names the H1 symbol that owns its
    payload and this reader compares the bytes at that exact H1-resolved ROM range before
    returning them.  A clean checkout alone is intentionally insufficient.
    """

    disasm: Path
    rom: bytes
    addresses: dict[str, int]

    def read(
        self,
        relative_path: str | Path,
        *,
        symbol: str,
        expected_sha256: str | None = None,
    ) -> bytes:
        path = (self.disasm / relative_path).resolve(strict=True)
        try:
            path.relative_to(self.disasm)
        except ValueError as exc:
            raise ValueError(
                f"graphic source escapes the pinned disassembly: {relative_path}"
            ) from exc
        if symbol not in self.addresses:
            raise ValueError(f"graphic source H1 symbol is missing: {symbol}")
        payload = path.read_bytes()
        if not payload:
            raise ValueError(f"graphic source payload is empty: {symbol}")
        address = self.addresses[symbol]
        end = address + len(payload)
        if address < 0 or end > len(self.rom) or self.rom[address:end] != payload:
            raise ValueError(
                f"graphic source/ROM provenance drift: {symbol} ({Path(relative_path).as_posix()})"
            )
        digest = hashlib.sha256(payload).hexdigest().upper()
        if expected_sha256 is not None and digest != expected_sha256:
            raise ValueError(f"graphic source accepted-owner hash drift: {symbol}")
        return payload


def _verified_source(
    rom_path: Path, upstream_path: Path, *, owner: str, listing_path: Path | None = None
) -> tuple[ProvenanceBoundSource, str, dict[str, object]]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if (
        len(rom) != expected_rom["sizeBytes"]
        or hashlib.sha256(rom).hexdigest().upper() != expected_rom["hashes"]["sha256"]
    ):
        raise ValueError(f"{owner} ROM identity drift")
    listing_path = listing_path or upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"{owner} H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    return ProvenanceBoundSource(disasm, rom, addresses), commit, expected_rom
