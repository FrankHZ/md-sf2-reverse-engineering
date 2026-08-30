"""Shared, passive transport primitives for original-reference replay contracts.

The transport layer owns canonical public bytes and structural Lua containment.
It intentionally has no candidate ledger, private-output directory, emulator launch,
or scenario-specific address meaning.  Callers provide any filesystem paths only at
their outer composition boundary; returned identities contain hashes and sizes only.
"""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any


class TransportError(ValueError):
    """A deterministic transport or passive-observer contract failure."""


def canonical_json_bytes(value: Any) -> bytes:
    """Encode a stable UTF-8 JSON identity without host-specific formatting."""

    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")) + "\n"
    ).encode("utf-8")


def sha256(data: bytes) -> str:
    """Return the repository's canonical uppercase SHA-256 representation."""

    return hashlib.sha256(data).hexdigest().upper()


def canonical_utf8_lf_bytes(path: Path) -> bytes:
    """Accept only UTF-8 text and the deliberate CRLF checkout normalization."""

    raw = path.resolve(strict=True).read_bytes()
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as error:
        raise TransportError(f"passive observer is not UTF-8: {error}") from error
    canonical = text.replace("\r\n", "\n")
    if "\r" in canonical:
        raise TransportError("passive observer has a non-CRLF carriage return")
    return canonical.encode("utf-8")


def file_identity(path: Path) -> dict[str, Any]:
    """Return the identity-only public projection of a regular input file."""

    resolved = path.resolve(strict=True)
    data = resolved.read_bytes()
    return {"sha256": sha256(data), "sizeBytes": len(data)}


def validate_passive_lua_source(
    *,
    path: Path,
    expected_sha256: str,
    allowed_api_names: tuple[str, ...],
    allowed_bare_calls: tuple[str, ...],
    forbidden_patterns: tuple[str, ...],
) -> str:
    """Reject dynamic aliases and every Lua capability outside the declared read policy.

    This is a structural source gate, not a Lua gameplay implementation.  It rejects
    indirect namespace/member calls before the optional pinned-runtime syntax check
    that a private launch may later perform.
    """

    source = canonical_utf8_lf_bytes(path)
    digest = sha256(source)
    if digest != expected_sha256:
        raise TransportError(
            f"passive observer hash drift: expected {expected_sha256}, got {digest}"
        )
    text = source.decode("utf-8")
    lowered = text.lower()
    for pattern in forbidden_patterns:
        normalized_pattern = pattern.lower()
        present = (
            re.search(rf"\b{re.escape(normalized_pattern)}\b", lowered) is not None
            if re.fullmatch(r"[a-z_]+", normalized_pattern)
            else normalized_pattern in lowered
        )
        if present:
            raise TransportError(f"passive observer uses forbidden Lua surface: {pattern}")
    if re.search(r"\b(?:event|client|os|io|string|table)\s*\[[^\]]*\]", text):
        raise TransportError("passive observer uses dynamic member access")
    if re.search(r"\b[A-Za-z_][A-Za-z0-9_]*\s*\[[^\]]*\]\s*\(", text):
        raise TransportError("passive observer uses dynamic member access")
    allowed_by_namespace: dict[str, set[str]] = {}
    for name in allowed_api_names:
        namespace, separator, method = name.partition(".")
        if not separator or not namespace or not method:
            raise TransportError(f"invalid allowed API name: {name}")
        allowed_by_namespace.setdefault(namespace, set()).add(method)
    for namespace in allowed_by_namespace:
        if re.search(
            rf"(?m)^\s*(?:local\s+)?[A-Za-z_][A-Za-z0-9_]*\s*=\s*{re.escape(namespace)}\b"
            rf"(?!\s*\.)",
            text,
        ):
            raise TransportError(f"passive observer aliases API namespace: {namespace}")
        if re.search(rf"\b{re.escape(namespace)}\s*:", text):
            raise TransportError(f"passive observer uses unallowed API: {namespace}:<method>")

    for match in re.finditer(r"\b([a-z_][a-z0-9_]*)\.([A-Za-z_][A-Za-z0-9_]*)", text):
        namespace, method = match.groups()
        direct_call = text[match.end() :].lstrip().startswith("(")
        if namespace in allowed_by_namespace and not direct_call:
            raise TransportError("passive observer aliases API member")
        if not direct_call:
            continue
        if method not in allowed_by_namespace.get(namespace, set()):
            raise TransportError(f"passive observer uses unallowed API: {namespace}.{method}")
    if re.search(
        r"(?m)^\s*(?:local\s+)?[A-Za-z_][A-Za-z0-9_]*\s*=\s*"
        r"[a-z_][a-z0-9_]*\.[A-Za-z_][A-Za-z0-9_]*\b(?!\s*\()",
        text,
    ):
        raise TransportError("passive observer aliases API member")

    local_functions = set(re.findall(r"\blocal\s+function\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(", text))
    allowed_calls = set(allowed_bare_calls) | local_functions
    for match in re.finditer(r"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", text):
        name = match.group(1)
        if match.start() > 0 and text[match.start() - 1] in {".", ":"}:
            continue
        if name == "function" or name in allowed_calls:
            continue
        raise TransportError(f"passive observer uses unallowed Lua call: {name}")
    if re.search(r"\bload\s*\(", text):
        raise TransportError("passive observer uses forbidden Lua surface: load")
    return digest
