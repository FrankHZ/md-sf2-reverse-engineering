from __future__ import annotations

import hashlib
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

DEFAULT_MANIFEST = repo_path("manifests/roms/sf2-us.json")
ROM_SCHEMA = repo_path("schemas/rom-manifest.schema.json")


def _ascii_field(data: bytes, offset: int, length: int) -> str:
    return data[offset : offset + length].decode("ascii").strip("\0 ")


def mega_drive_checksum(data: bytes) -> str:
    checksum = 0
    for offset in range(0x200, len(data), 2):
        word = data[offset] << 8
        if offset + 1 < len(data):
            word |= data[offset + 1]
        checksum = (checksum + word) & 0xFFFF
    return f"{checksum:04X}"


def inspect_rom(path: Path) -> dict[str, Any]:
    data = path.read_bytes()
    if len(data) < 0x200:
        raise ValueError(f"file is too small to contain a Mega Drive ROM header: {path}")
    return {
        "sizeBytes": len(data),
        "sha256": hashlib.sha256(data).hexdigest().upper(),
        "sha1": hashlib.sha1(data).hexdigest().upper(),
        "md5": hashlib.md5(data, usedforsecurity=False).hexdigest().upper(),
        "console": _ascii_field(data, 0x100, 16),
        "domesticTitle": _ascii_field(data, 0x120, 48),
        "overseasTitle": _ascii_field(data, 0x150, 48),
        "serial": _ascii_field(data, 0x180, 14),
        "storedChecksum": f"{int.from_bytes(data[0x18E:0x190], 'big'):04X}",
        "computedChecksum": mega_drive_checksum(data),
        "regions": _ascii_field(data, 0x1F0, 16),
    }


def verify_rom(path: Path, manifest_path: Path = DEFAULT_MANIFEST) -> dict[str, Any]:
    path = path.resolve(strict=True)
    manifest_path = manifest_path.resolve(strict=True)
    manifest = load_json(manifest_path)
    validate_json(manifest, ROM_SCHEMA, owner=str(manifest_path))
    actual = inspect_rom(path)
    expected = {
        "sizeBytes": manifest["sizeBytes"],
        "sha256": manifest["hashes"]["sha256"],
        "sha1": manifest["hashes"]["sha1"],
        "md5": manifest["hashes"]["md5"],
        "console": manifest["header"]["console"],
        "domesticTitle": manifest["header"]["domesticTitle"],
        "overseasTitle": manifest["header"]["overseasTitle"],
        "serial": manifest["header"]["serial"],
        "storedChecksum": manifest["header"]["checksum"],
        "computedChecksum": manifest["header"]["checksum"],
        "regions": manifest["header"]["regions"],
    }
    failures = [
        f"{field}: expected {expected_value!r}, got {actual[field]!r}"
        for field, expected_value in expected.items()
        if actual[field] != expected_value
    ]
    if failures:
        raise ValueError("ROM baseline verification failed:\n - " + "\n - ".join(failures))
    return {
        "ManifestId": manifest["id"],
        "RomPath": str(path),
        "SizeBytes": actual["sizeBytes"],
        "SHA256": actual["sha256"],
        "HeaderChecksum": actual["storedChecksum"],
        "ComputedChecksum": actual["computedChecksum"],
        "Status": "PASS",
    }
