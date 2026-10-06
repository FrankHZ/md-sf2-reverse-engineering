"""Selected report facts for the controlled C# obligation/integrity process."""

from __future__ import annotations

import sys

from sf2tool import h4_dotnet
from sf2tool.h4_dotnet import ResourceProcessError, _selected
from sf2tool.h4_inventory import _SelectedSequence
from sf2tool.h4_visual_source import VisualSources
from sf2tool.h4_visual_source import validate_reply as validate_source_reply


def validate_reply(operation, result):
    if isinstance(result, dict) and set(result) == {"sourceRead"}:
        row = result["sourceRead"]
        if isinstance(row, dict) and row.get("action") == "release":
            if row.get("index") != 0:
                raise ResourceProcessError("Malformed report source release")
            validate_source_reply("visual-read", {"sourceRead": dict(row, action="next")})
        else:
            validate_source_reply("visual-read", result)
        return
    if operation == "integrity-cancel" and result is None:
        return
    if (
        not isinstance(result, dict)
        or set(result) != {"mode", "rows", "offset", "total", "done"}
        or result["mode"] not in ("required", "check")
        or type(result["offset"]) is not int
        or result["offset"] < 0
        or type(result["total"]) is not int
        or result["total"] < 0
        or type(result["done"]) is not bool
        or not isinstance(result["rows"], list)
        or len(result["rows"]) > 256
        or not result["rows"]
        and not result["done"]
    ):
        raise ResourceProcessError("Malformed H4 report-integrity reply")
    for row in result["rows"]:
        valid = (
            isinstance(row, str)
            if result["mode"] == "check"
            else (
                isinstance(row, dict)
                and (
                    set(row) == {"parent"}
                    and (row["parent"] is None or isinstance(row["parent"], str))
                    or set(row) == {"child"}
                    and isinstance(row["child"], str)
                )
            )
        )
        if not valid:
            raise ResourceProcessError("Malformed H4 report-integrity result row")


def _counts(value):
    # Preserve native Counter-key types (including None/bool), unlike JSON object keys.
    return dict(
        dictionary=isinstance(value, dict),
        entries=[list(item) for item in value.items()] if isinstance(value, dict) else [],
    )


class ReportIntegrity:
    def __init__(self, sequence_type, *, budget=None):
        self.sequence_type = sequence_type
        self.child = h4_dotnet.ResourceComparison({}, {}, {}, {}, budget=budget, defer_start=True)
        # Reuse the accepted reference codec only; no visual qualification is called.
        self.codec = VisualSources(self.child, None, {}, (sequence_type, _SelectedSequence))

    def __enter__(self):
        self.child.__enter__()
        return self

    def __exit__(self, *args):
        return self.child.__exit__(*args)

    def rows(self, value, select):
        return (
            _SelectedSequence(value, select)
            if isinstance(value, (list, tuple, self.sequence_type))
            else value
        )

    def reference(self, value):
        def fields(value, selected):
            if not isinstance(value, dict):
                return value
            return {
                key: transform(value[key]) for key, transform in selected.items() if key in value
            }

        def state(value):
            return fields(
                value,
                {
                    "state": lambda v: fields(
                        v,
                        {"flags": lambda flags: list(flags) if isinstance(flags, dict) else flags},
                    )
                },
            )

        def admission(value):
            selected = state(value)
            if isinstance(value, dict) and "accounting" in value:
                selected["accounting"] = fields(
                    value["accounting"],
                    {
                        "allies": lambda allies: self.rows(
                            allies, lambda row: _selected(row, ("id",))
                        )
                    },
                )
            return selected

        return fields(
            value,
            {
                "admission": admission,
                "endpoint": state,
                "inherited": lambda v: fields(
                    v,
                    {
                        "entities": lambda rows: self.rows(
                            rows, lambda row: _selected(row, ("actionScript", "x", "y", "physical"))
                        )
                    },
                ),
            },
        )

    def report(self, value):
        value = _selected(
            value,
            (
                "comparisonScope",
                "variant",
                "assertions",
                "coverageObligations",
                "counts",
                "result",
                "milestonePass",
                "historicalCounts",
            ),
        )
        if not isinstance(value, dict):
            return value
        for key in ("counts", "historicalCounts"):
            if key in value:
                value[key] = _counts(value[key])
        if "assertions" in value:
            value["assertions"] = self.rows(
                value["assertions"],
                lambda row: _selected(
                    row,
                    (
                        "applicability",
                        "assertion",
                        "parent",
                        "expected",
                        "actualValue",
                        "result",
                    ),
                ),
            )

        def parent(row):
            row = _selected(
                row,
                (
                    "assertion",
                    "children",
                    "historicalChildren",
                    "actualValue",
                    "result",
                    "applicability",
                ),
            )
            if isinstance(row, dict) and "actualValue" in row:
                row["actualValue"] = _counts(row["actualValue"])
            return row

        if "coverageObligations" in value:
            value["coverageObligations"] = self.rows(value["coverageObligations"], parent)
        return value

    def operands(self, value):
        # JSON erases tuple/list and native mapping-key types. Retain factual
        # container tags; bounded sequences keep the accepted reference codec.
        containers = []

        def visit(value, path):
            if isinstance(value, dict):
                if all(isinstance(key, str) for key in value):
                    return {key: visit(item, [*path, key]) for key, item in value.items()}
                items = [
                    [visit(key, [*path, i, 0]), visit(item, [*path, i, 1])]
                    for i, (key, item) in enumerate(value.items())
                ]
                containers.append(dict(path=path, kind="mapping"))
                return items
            if isinstance(value, (list, tuple)):
                items = [visit(item, [*path, i]) for i, item in enumerate(value)]
                if isinstance(value, tuple):
                    containers.append(dict(path=path, kind="tuple"))
                return items
            return value

        return dict(source=self.codec.operands(visit(value, [])), containers=containers)

    def exchange(self, mode, **values):
        iterators, positions = {}, {}
        sequence = last_cursor = 0
        try:
            result = self.child.exchange(
                dict(op="integrity-" + mode, operands=self.operands(values))
            )
            while isinstance(result, dict) and "sourceRead" in result:
                row = result["sourceRead"]
                sequence += 1
                source_id, cursor = row["source"], row["cursor"]
                if row["sequence"] != sequence or source_id >= len(self.codec.sequences):
                    raise ResourceProcessError("Report source read sequence/reference changed")
                source = self.codec.sequences[source_id]
                try:
                    if row["action"] == "release":
                        if (
                            cursor not in iterators
                            or positions[cursor][0] != source_id
                            or cursor != next(reversed(iterators))
                        ):
                            raise ResourceProcessError("Report source release cursor changed")
                        iterator = iterators.pop(cursor)
                        del positions[cursor]
                        close = getattr(iterator, "close", None)
                        if close is not None:
                            close()
                        value = None
                    elif row["action"] == "length":
                        value = len(source)
                    elif row["action"] == "index":
                        value = source[row["index"]]
                    else:
                        if positions.get(cursor, (source_id, 0)) != (source_id, row["index"]):
                            raise ResourceProcessError("Report source cursor changed")
                        if cursor not in iterators:
                            if cursor <= last_cursor:
                                raise ResourceProcessError(
                                    "Report source cursor reused after release"
                                )
                            last_cursor = cursor
                            iterators[cursor] = iter(source)
                        try:
                            value = dict(done=False, value=next(iterators[cursor]))
                        except StopIteration:
                            value = dict(done=True, value=None)
                        positions[cursor] = (source_id, None if value["done"] else row["index"] + 1)
                except (
                    KeyError,
                    IndexError,
                    TypeError,
                    ValueError,
                    AttributeError,
                    OverflowError,
                    OSError,
                ) as error:
                    try:
                        self.child.exchange(dict(op="integrity-cancel"))
                    except Exception as cancellation:
                        error.add_note(f"H4 integrity cancellation failed: {cancellation}")
                    raise
                result = self.child.exchange(
                    dict(op="integrity-read", sequence=sequence, value=self.operands(value))
                )
            offset, total, output = 0, result["total"], []
            while True:
                if result["mode"] != mode or result["offset"] != offset or result["total"] != total:
                    raise ResourceProcessError("Report integrity drain changed")
                output.extend(result["rows"])
                offset += len(result["rows"])
                if offset > total or result["done"] and offset != total:
                    raise ResourceProcessError("Report integrity result count changed")
                if result["done"]:
                    return output
                result = self.child.exchange(dict(op="integrity-drain"))
        finally:
            active_error, close_error = sys.exception(), None
            for iterator in iterators.values():
                close = getattr(iterator, "close", None)
                if close is not None:
                    try:
                        close()
                    except Exception as error:
                        if close_error is None:
                            close_error = error
            if active_error is None and close_error is not None:
                raise close_error

    def required(self, variant, ref):
        rows = self.exchange("required", variant=variant, reference=self.reference(ref))
        families, parent = {}, None
        for row in rows:
            if "parent" in row:
                parent = row["parent"]
                if parent in families:
                    raise ResourceProcessError("Duplicate report family page")
                families[parent] = []
            else:
                if parent not in families:
                    raise ResourceProcessError("Report child precedes its family")
                families[parent].append(row["child"])
        return {parent: tuple(children) for parent, children in families.items()}

    def check(self, report, ref):
        return self.exchange("check", report=self.report(report), reference=self.reference(ref))


def required_children(variant, ref, *, sequence_type):
    with ReportIntegrity(sequence_type) as process:
        return process.required(variant, ref)


def report_integrity(report, ref, *, sequence_type):
    with ReportIntegrity(sequence_type) as process:
        return process.check(report, ref)
