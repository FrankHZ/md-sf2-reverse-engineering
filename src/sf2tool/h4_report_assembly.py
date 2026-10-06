"""Modern assembly decisions in C#; existing Python storage and factual IO."""

from __future__ import annotations

from functools import wraps

from sf2tool.h4_dotnet import ResourceProcessError, _selected
from sf2tool.h4_inventory import _SelectedSequence
from sf2tool.h4_report_integrity import ReportIntegrity
from sf2tool.h4_report_integrity import validate_reply as validate_integrity_reply
from sf2tool.h4_visual_source import VisualSources

_FIELDS = {
    "layer",
    "assertion",
    "applicability",
    "original",
    "actual",
    "expected",
    "actualValue",
    "result",
    "reason",
}
_COPY = {
    "layer",
    "assertion",
    "applicability",
    "original",
    "actual.file",
    "actual.record",
    "expected",
    "actualValue",
    "reason",
    "parent",
    "missingSide",
}
_BASE_COPY = {"layer", "assertion", "expected", "actualValue", "actual.file", "actual.record"}
_STEPS = {
    "ready",
    "reason",
    "reason-location",
    "reason-storage",
    "compare",
    "original",
    "file",
    "location",
    "path",
    "parent-truth",
    "missing",
    "append",
}


def _key_valid(value):
    return isinstance(value, dict) and (
        set(value) == {"value"}
        and (value["value"] is None or isinstance(value["value"], (str, int, float)))
        or set(value) == {"tuple"}
        and isinstance(value["tuple"], list)
        and all(_key_valid(item) for item in value["tuple"])
    )


def validate_reply(operation, result):
    def bad():
        raise ResourceProcessError("Malformed H4 report-assembly reply for " + operation)

    if operation.removeprefix("assembly-") not in (_STEPS - {"ready"}) | {
        "start",
        "check",
        "admit",
        "parent",
        "counts",
        "counts-drain",
        "read",
        "cancel",
    } or not operation.startswith("assembly-"):
        bad()
    if isinstance(result, dict) and set(result) == {"sourceRead"}:
        if operation in ("assembly-cancel", "assembly-counts-drain"):
            bad()
        validate_integrity_reply("integrity-read", result)
        return
    if operation == "assembly-cancel":
        if result is not None:
            bad()
        return
    if operation in ("assembly-admit", "assembly-append"):
        if type(result) is not bool:
            bad()
        return
    if not isinstance(result, dict):
        bad()
    if operation in ("assembly-counts", "assembly-counts-drain") or "rows" in result:
        if (
            set(result) != {"rows", "offset", "total", "done", "result", "milestonePass"}
            or not isinstance(result["rows"], list)
            or len(result["rows"]) > 256
            or type(result["offset"]) is not int
            or result["offset"] < 0
            or type(result["total"]) is not int
            or result["total"] < 0
            or type(result["done"]) is not bool
            or not result["rows"]
            and not result["done"]
            or result["result"] not in ("PASS", "FAIL", "Unavailable")
            or type(result["milestonePass"]) is not bool
        ):
            bad()
        for row in result["rows"]:
            if (
                not isinstance(row, dict)
                or set(row) != {"key", "count"}
                or not _key_valid(row["key"])
                or type(row["count"]) is not int
                or row["count"] < 1
            ):
                bad()
        return
    if operation == "assembly-parent":
        if (
            set(result) != _FIELDS | {"children", "historicalChildren"}
            or result["result"] not in ("PASS", "FAIL", "Unavailable")
            or result["applicability"] not in ("applicable", "required-unobserved")
            or not isinstance(result["actual"], dict)
            or set(result["actual"]) != {"file", "record"}
            or not isinstance(result["assertion"], str)
            or not isinstance(result["actual"]["record"], str)
            or any(
                result[key] is not None
                for key in ("layer", "actualValue", "children", "historicalChildren")
            )
        ):
            bad()
        return
    step = result.get("step")
    if not isinstance(step, str) or step not in _STEPS:
        bad()
    if step == "path":
        if set(result) != {"step", "path"} or result["path"] not in (
            "actual",
            "outcome",
            "host-log",
        ):
            bad()
    elif step == "append":
        if (
            set(result) != {"step", "row", "copy"}
            or not isinstance(result["row"], dict)
            or not isinstance(result["copy"], list)
            or not all(isinstance(k, str) for k in result["copy"])
        ):
            bad()
        row, fields = result["row"], result["copy"]
        if (
            not _FIELDS <= set(row) <= _FIELDS | {"parent", "missingSide"}
            or len(fields) != len(set(fields))
            or not _BASE_COPY <= set(fields) <= _COPY
            or row["result"] not in ("PASS", "FAIL", "Unavailable")
            or not isinstance(row["actual"], dict)
            or set(row["actual"]) != {"file", "record"}
            or ("parent" in row) != ("parent" in fields)
            or "missingSide" in fields
            and "missingSide" not in row
        ):
            bad()
    elif set(result) != {"step"}:
        bad()


def _truth_fact(value, sequence_type):
    if value is None or isinstance(value, (bool, int, float)):
        return dict(scalar=value)
    if isinstance(value, (str, dict, list, tuple, sequence_type)):
        return dict(length=len(value))
    # Opaque objects have Python's default true value; publication still owns encoding.
    return dict(object=True)


def _key(fact):
    return tuple(_key(item) for item in fact["tuple"]) if "tuple" in fact else fact["value"]


def assembly_session(sequence_type, bounded_list, group_rows, owner):
    def decorate(function):
        @wraps(function)
        def run(*args, **kwargs):
            with ReportAssembly(sequence_type, bounded_list, group_rows, owner) as assembly:
                return function(*args, **kwargs, _assembly=assembly)

        return run

    return decorate


class ReportAssembly:
    def __init__(self, sequence_type, bounded_list, group_rows, owner):
        self.sequence_type, self.bounded_list, self.group_rows = (
            sequence_type,
            bounded_list,
            group_rows,
        )
        self.owner, self.started = owner, False
        self.transport = ReportIntegrity(sequence_type)

    def __enter__(self):
        self.transport.__enter__()
        return self

    def __exit__(self, *args):
        return self.transport.__exit__(*args)

    def begin(self, actual_path, outcome_path, host_log):
        self.paths = {"actual": actual_path, "outcome": outcome_path, "host-log": host_log}
        self.assertions, self.obligations = self.bounded_list(), {}

    def _exchange(self, operation, **values):
        try:
            return self.transport._source_exchange(
                "assembly-" + operation, values, family="assembly"
            )
        finally:
            # The operation has completed/cancelled. C# retains no source references.
            self.transport.codec = VisualSources(
                self.transport.child, None, {}, (self.sequence_type, _SelectedSequence)
            )

    def _call(self, operation, **values):
        if not self.started:
            self._exchange("start", owner=self.owner)
            self.started = True
        return self._exchange(operation, **values)

    def check(
        self,
        layer,
        name,
        expected,
        value,
        location,
        original=None,
        applicability="applicable",
        reason="semantic assertion",
        parent=None,
        missing_side=None,
        actual_file=None,
    ):
        supplied = dict(
            layer=layer,
            assertion=name,
            expected=expected,
            actualValue=value,
            original=original,
            applicability=applicability,
            reason=reason,
            parent=parent,
            missingSide=missing_side,
            **{"actual.record": location, "actual.file": actual_file},
        )
        reply = self._call("check", valueNull=value is None, applicability=applicability)
        while reply["step"] != "append":
            step = reply["step"]
            if step == "compare":
                reply = self._call(step, value=value, expected=expected)
            elif step == "reason":
                reply = self._call(step, reason=reason)
            elif step in ("reason-location", "location"):
                reply = self._call(step, location=location)
            elif step == "reason-storage":
                supplied["reason"] = "Actual field absent at " + location
                reply = self._call(step)
            elif step == "path":
                supplied["actual.file"] = self.paths[reply["path"]].as_posix()
                reply = self._call(step)
            elif step in ("original", "file", "parent-truth", "missing"):
                operand = {
                    "original": original,
                    "file": actual_file,
                    "parent-truth": parent,
                    "missing": missing_side,
                }[step]
                reply = self._call(step, **_truth_fact(operand, self.sequence_type))
            else:
                raise ResourceProcessError("Unexpected assembly check continuation")
        row = reply["row"]
        for field in reply["copy"]:
            if field.startswith("actual."):
                row["actual"][field.split(".", 1)[1]] = supplied[field]
            else:
                row[field] = supplied[field]
        self.assertions.append(row)
        if self._call("append", **_truth_fact(parent, self.sequence_type)):
            self.group_rows(self.obligations, parent).append(self.assertions[-1])

    def _admitted(self, rows, *, historical=False):
        for row in rows:
            if self._call("admit", row=_selected(row, ("applicability",)), historical=historical):
                yield row

    def _counts(self, rows, *, historical=False):
        fields = ("applicability", "result") if historical else ("result",)
        selected = _SelectedSequence(rows, lambda row: _selected(row, fields))
        reply = self._call("counts", rows=selected, historical=historical)
        offset, total, counts = 0, reply["total"], {}
        verdict, milestone = reply["result"], reply["milestonePass"]
        while True:
            if (
                reply["offset"] != offset
                or reply["total"] != total
                or reply["result"] != verdict
                or reply["milestonePass"] is not milestone
            ):
                raise ResourceProcessError("Assembly count drain changed")
            for row in reply["rows"]:
                key = _key(row["key"])
                if key in counts:
                    raise ResourceProcessError("Duplicate assembly count key")
                counts[key] = row["count"]
            offset += len(reply["rows"])
            if offset > total or reply["done"] and offset != total:
                raise ResourceProcessError("Assembly count result count changed")
            if reply["done"]:
                return dict(counts=counts, result=verdict, milestonePass=milestone)
            reply = self.transport.child.exchange(dict(op="assembly-counts-drain"))

    def coverage(self):
        coverage = self.bounded_list()
        for name, children in self.obligations.items():
            required = self.bounded_list(self._admitted(children))
            summary = self._counts(required)
            layer = children[0]["layer"]
            path = self.paths["actual"].as_posix()
            row = self._call("parent", name=name)
            row["layer"], row["actual"]["file"], row["actualValue"] = layer, path, summary["counts"]
            row["children"] = [child["assertion"] for child in required]
            row["historicalChildren"] = [
                child["assertion"] for child in self._admitted(children, historical=True)
            ]
            coverage.append(row)
        return coverage

    def summary(self):
        required = self.bounded_list(self._admitted(self.assertions))
        return self._counts(required)

    def historical_counts(self):
        return self._counts(self.assertions, historical=True)["counts"]
