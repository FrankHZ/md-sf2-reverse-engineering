"""Compose the separate original and historical opening admission proof domains."""

from sf2tool.remake_h4.opening_actual import compare_actual
from sf2tool.remake_h4.opening_checks import OpeningChecks
from sf2tool.remake_h4.opening_identity import compare_identity
from sf2tool.remake_h4.opening_readback import select_readback
from sf2tool.remake_h4.opening_records import compare_records
from sf2tool.remake_h4.opening_source import compare_source
from sf2tool.remake_h4.opening_terminal import compare_terminal


def admission_opening_binding(actual, context, source_root):
    """Controlled original R1/readers versus historical A's own admission.

    This decides the admission controls, not pixel cadence or every text service.
    Original and remake clocks/sessions are deliberately separate proof domains.
    """
    context = (context or {}).get("opening") or {}
    checks = OpeningChecks()
    candidate, observed, diagnostic = compare_identity(context, checks)
    rows, rows_available, epochs = compare_records(context, candidate, diagnostic, checks)
    selected = select_readback(rows, epochs, checks)
    compare_terminal(candidate, observed, diagnostic, rows, rows_available, selected[-1], checks)
    compare_source(selected, source_root, checks)
    compare_actual(actual, selected[0], checks)
    return dict(
        value=checks.merge([c["value"] for c in checks.rows]),
        checks=checks.rows,
        boundary="controlled original R1/readers versus historical A admission controls",
        unknown=[
            "unreached delay reader",
            "natural title/reset ancestry",
            "per-glyph hardware timing",
        ],
        owner="docs/research/map3-messenger-acceptance.md#controlled-opening-scalar-readback",
    )
