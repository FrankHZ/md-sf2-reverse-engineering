"""One scoped C# resource process; Python retains SQLite selection and publication."""

from __future__ import annotations

import json
import math
import os
import subprocess
import sys
import tempfile
from contextlib import contextmanager

from sf2tool.dotnet_environment import shared_dotnet_environment
from sf2tool.paths import repo_path


class ResourceProcessError(RuntimeError):
    """A failed comparison process is not unavailable original evidence."""


def _resource_reply(operation, reply):
    """Validate protocol structure before caller evidence handlers see any data."""

    def malformed():
        raise ResourceProcessError(f"Malformed H4 resource reply for {operation}")

    def count(value):
        return type(value) is int and value >= 0

    def totals(value):
        return (
            isinstance(value, dict)
            and {"PASS", "FAIL", "Unavailable"} <= value.keys()
            and all(count(value[key]) for key in ("PASS", "FAIL", "Unavailable"))
        )

    def counted_row(row):
        return (
            isinstance(row, dict)
            and {"family", "name", "value", "count"} <= row.keys()
            and isinstance(row["family"], str)
            and isinstance(row["name"], str)
            and (row["value"] is None or isinstance(row["value"], bool))
            and count(row["count"])
        )

    if not isinstance(reply, dict):
        malformed()
    if set(reply) == {"operandError"}:
        name = reply["operandError"]
        error = (
            {
                "AttributeError": AttributeError,
                "KeyError": KeyError,
                "TypeError": TypeError,
                "ValueError": ValueError,
                "IndexError": IndexError,
                "OverflowError": OverflowError,
            }.get(name)
            if isinstance(name, str)
            else None
        )
        if error is None:
            malformed()
        raise error("H4 resource operand: " + name)
    if set(reply) != {"result"}:
        malformed()
    result = reply["result"]
    if operation in (
        "sources",
        "source",
        "begin",
        "accumulate",
        "report-start",
        "report-records",
        "scene-start",
        "scene-rows",
    ):
        if result is not None:
            malformed()
    elif operation == "scan":
        if result is not None and not count(result):
            malformed()
    elif operation == "report-finish":
        if (
            not isinstance(result, dict)
            or not {"summary", "checkCount", "witnessCount"} <= result.keys()
            or not count(result["checkCount"])
            or not count(result["witnessCount"])
        ):
            malformed()
        summary = result["summary"]
        if (
            not isinstance(summary, dict)
            or not {"map", "entity", "scene", "familyCounts", "witnessPolicy"} <= summary.keys()
        ):
            malformed()
        families, policy = summary["familyCounts"], summary["witnessPolicy"]
        if (
            not isinstance(families, dict)
            or not {"map", "entity", "scene"} <= families.keys()
            or any(not totals(families[f]) for f in ("map", "entity", "scene"))
            or any(
                summary[f] is not None and not isinstance(summary[f], bool)
                for f in ("map", "entity", "scene")
            )
            or not isinstance(policy, dict)
            or set(policy) != {"limitPerCheckOutcome", "retained", "candidateVariants"}
            or type(policy["limitPerCheckOutcome"]) is not int
            or policy["limitPerCheckOutcome"] != 8
            or not count(policy["retained"])
            or policy["retained"] != result["witnessCount"]
            or policy["candidateVariants"] != "all distinct operands retained"
        ):
            malformed()
    elif operation in ("report-checks", "report-witnesses"):
        if (
            not isinstance(result, dict)
            or set(result) != {"rows", "done"}
            or not isinstance(result["rows"], list)
            or not isinstance(result["done"], bool)
            or not all(counted_row(row) for row in result["rows"])
            or not result["rows"]
            and not result["done"]
        ):
            malformed()
    elif operation == "finish":
        fields = {"candidatePairCount", "executedPairCount", "counts", "legacyStop", "checks"}
        if not isinstance(result, dict) or not fields <= result.keys():
            malformed()
        counts, checks, stop = result["counts"], result["checks"], result["legacyStop"]
        if (
            not count(result["candidatePairCount"])
            or not count(result["executedPairCount"])
            or not isinstance(counts, dict)
            or not {"PASS", "FAIL", "Unavailable"} <= counts.keys()
            or not all(count(counts[name]) for name in ("PASS", "FAIL", "Unavailable"))
            or not isinstance(checks, list)
        ):
            malformed()
        for check in checks:
            # Outcome and locator are original operands, not protocol type tags.
            if (
                not isinstance(check, list)
                or len(check) != 4
                or not isinstance(check[0], str)
                or not count(check[2])
                or check[2] == 0
            ):
                malformed()
        if stop is not None and (
            not isinstance(stop, dict)
            or not {"firstOrdinal", "error", "locator"} <= stop.keys()
            or not count(stop["firstOrdinal"])
            or not isinstance(stop["error"], str)
            or stop["error"] not in ("KeyError", "IndexError", "ValueError", "TypeError")
        ):
            malformed()
    else:
        malformed()
    return result


def _child_memory(process):
    if os.name != "nt":
        return None
    import ctypes
    from ctypes import wintypes

    class Counters(ctypes.Structure):
        _fields_ = [("cb", wintypes.DWORD), ("faults", wintypes.DWORD)] + [
            (name, ctypes.c_size_t)
            for name in (
                "peakWorking",
                "working",
                "peakPaged",
                "paged",
                "peakNonPaged",
                "nonPaged",
                "pagefile",
                "peakPrivate",
                "private",
            )
        ]

    counters = Counters()
    counters.cb = ctypes.sizeof(counters)
    ok = ctypes.windll.psapi.GetProcessMemoryInfo(
        wintypes.HANDLE(int(process._handle)), ctypes.byref(counters), counters.cb
    )
    return (counters.private, counters.peakPrivate) if ok else None


def _wire_message(message):
    # Python's existing JSON reader admits nonfinite numbers. JSON's numeric
    # grammar does not; explicit paths preserve them without reserving user keys.
    nonfinite = []

    def visit(value, path):
        if isinstance(value, float) and not math.isfinite(value):
            nonfinite.append([path, repr(value), repr(value) + ":" + str(id(value))])
            return None
        if isinstance(value, dict):
            return {key: visit(item, [*path, key]) for key, item in value.items()}
        if isinstance(value, (list, tuple)):
            return [visit(item, [*path, index]) for index, item in enumerate(value)]
        return value

    result = visit(message, [])
    result["nonFinite"] = nonfinite
    return result


def _batches(rows):
    batch, size = [], 0
    for row in rows:
        charge = len(json.dumps(row, ensure_ascii=True).encode("utf-8"))
        if batch and (len(batch) >= 256 or size + charge > 1024 * 1024):
            yield batch
            batch, size = [], 0
        batch.append(row)
        size += charge
    if batch:
        yield batch


def build():
    environment = shared_dotnet_environment(os.environ, repo_path("."))
    project = repo_path("tools/h4-comparison")
    for arguments in (
        ["restore", "H4Comparison.csproj", "--locked-mode"],
        [
            "build",
            "H4Comparison.csproj",
            "--configuration",
            "Release",
            "--no-restore",
            "--disable-build-servers",
        ],
    ):
        subprocess.run(
            [environment["DOTNET_BIN"], *arguments], cwd=project, env=environment, check=True
        )


class ResourceComparison:
    """The lifetime is one reached-resource phase, never a row or requirement."""

    def __init__(
        self,
        sprites,
        source_sprites,
        portraits,
        source_portraits,
        *,
        budget=None,
        defer_start=False,
    ):
        self.sources = dict(
            sprites=sprites,
            sourceSprites=source_sprites,
            portraits=portraits,
            sourcePortraits=source_portraits,
        )
        self.sent_sources = set()
        self.input_bytes = self.output_bytes = self.launches = 0
        self.process = self.errors = None
        self.budget = budget
        self.peak_private = 0
        self.defer_start = defer_start
        self.enabled = None
        self.pending, self.pending_bytes = [], 0

    def __enter__(self):
        return self if self.defer_start else self._start()

    def _start(self):
        environment = shared_dotnet_environment(os.environ, repo_path("."))
        assembly = repo_path("local/h4-comparison/bin/net10.0/H4Comparison.dll")
        if not assembly.is_file():
            raise ResourceProcessError(
                "Build the H4 resource tool: python -m sf2tool.h4_dotnet build"
            )
        scratch = repo_path("local/h4-comparison")
        self.errors = tempfile.TemporaryFile(dir=scratch)  # noqa: SIM115 - closed by __exit__
        try:
            self.process = subprocess.Popen(
                [environment["DOTNET_BIN"], str(assembly)],
                cwd=repo_path("tools/h4-comparison"),
                env=environment,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=self.errors,
            )
            self.launches += 1
            if self.budget is not None:
                self.budget.child_started()
            self.exchange(dict(op="sources", sources={name: [] for name in self.sources}))
            if self.enabled is not None:
                self.exchange(dict(op="report-start", enabled=list(self.enabled)))
        except BaseException as error:
            self.__exit__(*sys.exc_info())
            if isinstance(error, OSError):
                raise ResourceProcessError("Cannot start H4 resource process") from error
            raise
        return self

    def observe_memory(self, *, exited=False):
        memory = _child_memory(self.process)
        if memory is not None:
            self.peak_private = max(self.peak_private, memory[1])
        if self.budget is not None:
            self.budget.observe_child(memory, exited=exited)

    def exchange(self, message):
        if self.process is None:
            self._start()
        try:
            for part in json.JSONEncoder(ensure_ascii=True, separators=(",", ":")).iterencode(
                _wire_message(message)
            ):
                raw = part.encode("utf-8")
                self.process.stdin.write(raw)
                self.input_bytes += len(raw)
            self.process.stdin.write(b"\n")
            self.input_bytes += 1
            self.process.stdin.flush()
            raw = self.process.stdout.readline()
            self.output_bytes += len(raw)
            if not raw:
                raise ResourceProcessError("H4 resource process ended before its reply")
            reply = json.loads(raw)
            self.observe_memory()
        except (OSError, ValueError) as error:
            raise ResourceProcessError("H4 resource process transport failed") from error
        return _resource_reply(message["op"], reply)

    def reduce(self, required, relation, key):
        self.flush_checks()
        self.exchange(dict(op="begin", required=required))
        stop = None
        variants = relation.variants(key)
        try:
            for batch in _batches(dict(first=first, used=used) for _, _, first, used in variants):
                self.select_sources(required)
                stop = self.exchange(dict(op="scan", rows=batch))
                if stop is not None:
                    break
        finally:
            variants.close()

        def prefix():
            for signature, count, first, used in relation.variants(key):
                if stop is not None and first >= stop:
                    break
                if stop is not None:
                    count = relation.prefix_count(key, signature, stop)
                if count:
                    yield dict(count=count, used=used)

        rows = prefix()
        try:
            for batch in _batches(rows):
                self.exchange(dict(op="accumulate", rows=batch))
        finally:
            rows.close()
        total = sum(count for _, count, _, _ in relation.variants(key))
        return self.exchange(dict(op="finish", total=total))

    def _record(self, row):
        size = len(json.dumps(row, ensure_ascii=True).encode("utf-8"))
        if self.pending and (len(self.pending) >= 256 or self.pending_bytes + size > 1024 * 1024):
            self.flush_checks()
        self.pending.append(row)
        self.pending_bytes += size

    def check(self, family, name, value, identity=None, *, weight=1, locator=None):
        self._record(
            dict(
                kind="check",
                family=family,
                name=name,
                value=value,
                identity=identity,
                weight=weight,
                locator=locator,
            )
        )

    @contextmanager
    def evaluated(self, family, name, state):
        try:
            yield
        except (KeyError, IndexError, ValueError, TypeError) as error:
            weight, locator = state()
            self._record(
                dict(
                    kind="error",
                    families=list(family) if isinstance(family, tuple) else [family],
                    name=name,
                    error=type(error).__name__,
                    weight=weight,
                    locator=locator,
                )
            )

    def flush_checks(self):
        if self.pending:
            self.exchange(dict(op="report-records", rows=self.pending))
            self.pending, self.pending_bytes = [], 0

    def scene_uses(self, scene, rows):
        # Keep missing/null/malformed shapes; only remove fields never read by this phase.
        def select(value, names):
            return {k: value[k] for k in names if k in value} if isinstance(value, dict) else value

        selected = select(scene, ("healing", "fieldDeath"))
        if isinstance(selected, dict):
            for key, names in (
                ("healing", ("bodies", "wings", "dust")),
                ("fieldDeath", ("allies", "enemies", "exitFrames")),
            ):
                if key in selected:
                    selected[key] = select(selected[key], names)
        self.flush_checks()
        self.exchange(
            dict(
                op="scene-start",
                scene=selected,
                present=bool(rows),
                integerDigitLimit=sys.get_int_max_str_digits(),
            )
        )
        for batch in _batches(select(row, ("scene", "fieldActors")) for row in rows):
            self.exchange(dict(op="scene-rows", rows=batch))

    def finish_report(self, bounded_list):
        self.flush_checks()
        metadata = self.exchange(dict(op="report-finish"))

        def rows(operation, expected):
            total = 0
            while True:
                batch = self.exchange(dict(op=operation))
                total += len(batch["rows"])
                if total > expected or batch["done"] and total != expected:
                    raise ResourceProcessError("Resource report row count changed during drain")
                yield from batch["rows"]
                if batch["done"]:
                    return

        return dict(
            metadata["summary"],
            checks=bounded_list(rows("report-checks", metadata["checkCount"])),
            witnesses=bounded_list(rows("report-witnesses", metadata["witnessCount"])),
        )

    def select_sources(self, required):
        # Selection only; C# owns presence, equality and alternate-tile judgments.
        kind, want = required.get("kind"), required.get("expected")
        if kind == "map" or not isinstance(want, dict):
            return
        operand, names = (
            ("sprite", ("sprites", "sourceSprites"))
            if kind == "entity"
            else ("portrait", ("portraits", "sourcePortraits"))
        )
        if operand not in want:
            return
        key = want[operand]
        try:
            hash(key)
        except TypeError:
            return  # C# retains the original caught unhashable lookup.
        identity = operand, key
        if identity in self.sent_sources:
            return
        self.exchange(
            dict(
                op="source",
                entries={
                    name: [[key, self.sources[name][key]]] if key in self.sources[name] else []
                    for name in names
                },
            )
        )
        self.sent_sources.add(identity)

    def __exit__(self, exc_type, exc_value, traceback):
        failure = None
        try:
            if self.process is not None:
                try:
                    if exc_type is None:
                        self.observe_memory()
                except BaseException as error:
                    failure = error
                if exc_type is not None or failure is not None:
                    self.process.kill()
                try:
                    self.process.stdin.close()
                except OSError as error:
                    if exc_type is None and failure is None:
                        failure = ResourceProcessError(str(error))
                try:
                    code = self.process.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    self.process.kill()
                    code = self.process.wait()
                    failure = ResourceProcessError("H4 resource process failed to stop")
                try:
                    self.observe_memory(exited=True)
                except BaseException as error:
                    if failure is None and exc_type is None:
                        failure = error
                if code and exc_type is None and failure is None:
                    failure = ResourceProcessError(f"H4 resource process exited with status {code}")
        finally:
            if self.process is not None:
                self.process.stdout.close()
            if self.errors is not None:
                self.errors.close()
        if failure is not None:
            raise failure


if __name__ == "__main__":
    if sys.argv[1:] != ["build"]:
        raise SystemExit("usage: python -m sf2tool.h4_dotnet build")
    build()
