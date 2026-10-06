"""One scoped C# resource process; Python retains SQLite selection and publication."""

from __future__ import annotations

import json
import math
import os
import subprocess
import sys
import tempfile

from sf2tool.dotnet_environment import shared_dotnet_environment
from sf2tool.paths import repo_path


class ResourceProcessError(RuntimeError):
    """A failed comparison process is not unavailable original evidence."""


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

    def __init__(self, sprites, source_sprites, portraits, source_portraits, *, budget=None):
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

    def __enter__(self):
        environment = shared_dotnet_environment(os.environ, repo_path("."))
        assembly = repo_path("local/h4-comparison/bin/net10.0/H4Comparison.dll")
        if not assembly.is_file():
            raise ResourceProcessError(
                "Build the H4 resource tool: python -m sf2tool.h4_dotnet build"
            )
        scratch = repo_path("local/h4-comparison")
        self.errors = tempfile.TemporaryFile(dir=scratch)
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
        if "operandError" in reply:
            error = {
                "AttributeError": AttributeError,
                "KeyError": KeyError,
                "TypeError": TypeError,
                "ValueError": ValueError,
                "IndexError": IndexError,
                "OverflowError": OverflowError,
            }.get(reply["operandError"])
            if error is None:
                raise ResourceProcessError("Unknown H4 operand error")
            raise error("H4 resource operand: " + reply["operandError"])
        return reply["result"]

    def reduce(self, required, relation, key):
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
