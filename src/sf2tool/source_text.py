from __future__ import annotations

from pathlib import Path


def decode_upstream_text(raw: bytes) -> str:
    try:
        return raw.decode("utf-8")
    except UnicodeDecodeError:
        # The pinned IDA-exported Z80 source carries legacy C1 bytes in
        # comments, including values undefined by CP1252. Latin-1 preserves
        # every byte while leaving the ASCII assembly syntax unchanged.
        return raw.decode("latin-1")


def read_upstream_text(path: Path) -> str:
    return decode_upstream_text(path.read_bytes())
