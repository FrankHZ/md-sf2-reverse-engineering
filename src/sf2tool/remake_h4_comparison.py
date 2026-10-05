"""Compare the accepted original route with actual ordinary-host H4 observations.

The default legacy profile diagnoses first control and the first STAY/next actor.
The modern profile evaluates the continuous winning route and named settings matrix;
missing required original or host bindings keep full H4 acceptance incomplete.
Use the accepted read-only reference projector for those profiles. The audio,
turn-order, w2 and heal modes evaluate selected dependencies without loading the whole H4 report.
All outputs remain private.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import itertools
import json
import os
import re
import shutil
import sqlite3
import subprocess
import sys
import time
import uuid
import wave
from bisect import bisect_right
from collections import Counter
from collections.abc import Sequence
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import (
    ACCEPTED_UPSTREAM_REPOSITORY,
    _composite_generator_fingerprint,
)
from sf2tool.remake_assets import AssetPreflightError, inspect_asset_checkout
from sf2tool.remake_h4.admission_seed import admission_seed_binding
from sf2tool.remake_h4.ai_binding import ai_consumer_binding
from sf2tool.remake_h4.ai_source import (
    source_decision as _ai_source_decision,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.ai_source import (
    source_rules as _ai_source_rules,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.audio_consumer import audio_consumer_binding as _audio_consumer_binding
from sf2tool.remake_h4.audio_context import _audio_context
from sf2tool.remake_h4.field_binding import field_service_binding as _field_binding
from sf2tool.remake_h4.field_case import (
    service_case as _field_service_case,  # noqa: F401 - retained direct field observation import
)
from sf2tool.remake_h4.field_evidence import case_input as _field_input
from sf2tool.remake_h4.field_source import (
    _FIELD_COUNTERS,  # noqa: F401 - retained direct field observation import
    _FIELD_MOTION,  # noqa: F401 - retained direct field observation import
    _FIELD_SERVICES,  # noqa: F401 - retained direct field observation import
    _field_action_program,  # noqa: F401 - retained direct field observation import
    _field_npc_tick,  # noqa: F401 - retained direct field observation import
    _field_portrait,  # noqa: F401 - retained direct field observation import
    _field_rng,  # noqa: F401 - retained direct field observation import
)
from sf2tool.remake_h4.field_values import (
    _field_equal,  # noqa: F401 - retained direct field observation import
    _field_join,  # noqa: F401 - retained direct field observation import
)
from sf2tool.remake_h4.heal_binding import heal_consumer_binding
from sf2tool.remake_h4.heal_source import (
    fairy_source_step as _heal_fairy_source_step,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.map_binding import map_consumer_binding
from sf2tool.remake_h4.map_draw import (
    draw_cells as _map_draw_cells,  # noqa: F401 - retained direct map observation import
)
from sf2tool.remake_h4.map_source import (
    source_regions as _map_source_regions,  # noqa: F401 - retained direct map observation import
)
from sf2tool.remake_h4.motion_binding import field_motion_binding as _field_motion_binding
from sf2tool.remake_h4.opening_binding import admission_opening_binding
from sf2tool.remake_h4.operation_flow_binding import (
    operation_flow_binding as _operation_flow_binding,
)
from sf2tool.remake_h4.physical_binding import physical_consumer_binding
from sf2tool.remake_h4.physical_source import (
    source_action as _physical_source_action,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.physical_source import (
    source_operands as _physical_source_operands,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.reward_binding import reward_consumer_binding
from sf2tool.remake_h4.scene_binding import battle_scene_consumer_binding as _scene_consumer_binding
from sf2tool.remake_h4.scene_evidence import read_selection as _battle_scene_selection
from sf2tool.remake_h4.scene_source import (
    source_sequences as _battle_scene_source,  # noqa: F401 - retained direct source observation import
)
from sf2tool.remake_h4.turn_consumer import turn_order_consumer_binding
from sf2tool.remake_h4.turn_generation import turn_order_binding
from sf2tool.remake_h4.turn_source import (
    _source_turn_order,  # noqa: F401 - retained source observation alias
)
from sf2tool.remake_h4.w1_binding import w1_consumer_binding
from sf2tool.remake_h4.w1_source import (
    _W1_COHORT,  # noqa: F401 - retained W1 source cohort observation import
    _W1_SOURCE_FILES,  # noqa: F401 - retained W1 source cohort observation import
    _W1_SOURCE_SYMBOLS,  # noqa: F401 - retained W1 source cohort observation import
)
from sf2tool.remake_h4.w2_binding import w2_consumer_binding
from sf2tool.remake_h4.w2_source import _W2_COHORT  # noqa: F401 - retained public alias
from sf2tool.remake_h4_reference import (
    EXTENSION_SOURCE,
    OBSERVER,
    ROM,
    RUNNER,
    SOURCE,
    UPSTREAM,
    location,
    require,
    rows,
)

OWNER = "docs/design/contracts/map3-battle01-continuous-scenario.md"
DIRECTIONS = {1: "Up", 2: "Down", 4: "Left", 8: "Right"}


# Current captures and derived joins stay on disk; the historical JSON path is unchanged.
# Explicit SQLite ordinal/native-key joins retain the comparison predicates and
# detached publication semantics without per-group files or a full-run memory index.
_STREAM_CONTEXT = None
_STREAM_SCRATCH_ROOT = None
_STREAM_RECORD_LIMIT = 1024 * 1024
_STREAM_CACHE_LIMIT = 8 * 1024 * 1024
_STREAM_ROW_MEMORY_LIMIT = 16 * 1024 * 1024


def _row_memory(value):
    seen, total, pending = set(), 0, [value]
    while pending:
        row = pending.pop()
        identity = id(row)
        if identity in seen:
            continue
        seen.add(identity)
        total += sys.getsizeof(row)
        require(total <= _STREAM_ROW_MEMORY_LIMIT, "stream row decoded-memory limit")
        if isinstance(row, dict):
            pending.extend(row.keys())
            pending.extend(row.values())
        elif isinstance(row, (list, tuple)) and not isinstance(row, _RecordSpool):
            pending.extend(row)
    return total


class _ReaderContext:
    """One private SQLite store; bounded transactions, detached rows, explicit close."""

    def __init__(self, scratch_root=None, *, database=None, readonly=False):
        self.readonly = readonly
        self.connection = None
        self.pending_rows = self.pending_bytes = 0
        if database is None:
            root = Path(scratch_root).resolve()
            require(
                root.is_relative_to(repo_path("local").resolve()), "reader scratch outside local"
            )
            root = root / ("reader-" + uuid.uuid4().hex)
            root.mkdir(parents=True, exist_ok=False)
            database = root / "rows.sqlite"
        self.database = Path(database).resolve()
        self.root = self.database.parent
        require(
            self.database.is_relative_to(repo_path("local").resolve()),
            "reader database outside local",
        )
        if readonly:
            self.connection = sqlite3.connect(self.database.as_uri() + "?mode=ro", uri=True)
            self.connection.execute("PRAGMA query_only=ON")
        else:
            self.database.touch(exist_ok=False)
            self.connection = sqlite3.connect(self.database)
            self.connection.execute("PRAGMA journal_mode=DELETE")
            self.connection.execute("PRAGMA synchronous=FULL")
            self.connection.executescript("""
                CREATE TABLE stores(id INTEGER PRIMARY KEY, length INTEGER NOT NULL DEFAULT 0);
                CREATE TABLE records(store INTEGER NOT NULL, ordinal INTEGER NOT NULL,
                    payload BLOB NOT NULL, PRIMARY KEY(store,ordinal));
                CREATE TABLE entries(store INTEGER NOT NULL, key BLOB NOT NULL,
                    position INTEGER NOT NULL, original BLOB NOT NULL, payload BLOB NOT NULL,
                    PRIMARY KEY(store,key));
                CREATE INDEX entry_order ON entries(store,position);
                CREATE TABLE imports(source TEXT NOT NULL, store INTEGER NOT NULL,
                    span TEXT NOT NULL, target INTEGER NOT NULL,
                    PRIMARY KEY(source,store,span)) WITHOUT ROWID;
            """)
        self.connection.execute("PRAGMA cache_size=-8192")
        self.connection.execute("PRAGMA temp_store=FILE")
        self.connection.execute("PRAGMA mmap_size=0")

    def flush(self):
        budget = getattr(self, "resource_budget", None)
        if budget is not None:
            budget.observe()
        self.connection.commit()
        self.pending_rows = self.pending_bytes = 0

    def reserve(self, size):
        # Bound encoded values per transaction; each canonical/original key is also
        # independently record-bounded. SQLite's page-cache target is not a heap limit.
        require(0 <= size <= _STREAM_RECORD_LIMIT, "transaction value too large")
        if self.pending_rows >= 256 or self.pending_bytes + size > _STREAM_RECORD_LIMIT:
            self.flush()
        self.pending_rows += 1
        self.pending_bytes += size

    def close(self):
        if self.connection is not None:
            # Only an explicit successful flush commits the final partial transaction.
            self.connection.close()
            self.connection = None

    def __del__(self):
        self.close()

    def store(self):
        require(not self.readonly, "cannot append to a published report")
        self.reserve(32)
        result = self.connection.execute("INSERT INTO stores DEFAULT VALUES").lastrowid
        return result

    def length(self, store):
        row = self.connection.execute("SELECT length FROM stores WHERE id=?", (store,)).fetchone()
        require(row is not None, "missing derived row store")
        return row[0]

    def sequence(self, values=()):
        result = _RecordSpool(self, self.store())
        result.extend(values)
        return result

    def encode(self, value):
        _row_memory(value)
        raw = json.dumps(
            _pack_stream(value, self), ensure_ascii=False, separators=(",", ":")
        ).encode("utf-8")
        require(0 < len(raw) <= _STREAM_RECORD_LIMIT, "stream join record too large")
        return raw

    def decode(self, raw):
        require(0 < len(raw) <= _STREAM_RECORD_LIMIT, "stream row length limit")
        value = json.loads(raw)
        _row_memory(value)
        return _unpack_stream(value, self)

    def import_rows(self, rows):
        """Snapshot a published foreign store/slice once; memo lives on disk, not in RAM."""
        if rows.context is self and not isinstance(rows, _RecordSlice):
            return rows.store
        span = (
            str((rows.positions.start, rows.positions.stop, rows.positions.step))
            if isinstance(rows, _RecordSlice)
            else ""
        )
        source = (str(rows.context.database), rows.store, span)
        found = self.connection.execute(
            "SELECT target FROM imports WHERE source=? AND store=? AND span=?", source
        ).fetchone()
        if found is not None:
            return found[0]
        target = self.sequence()
        self.reserve(len(source[0]) + len(span) + 32)
        self.connection.execute("INSERT INTO imports VALUES(?,?,?,?)", (*source, target.store))
        target.extend(rows)
        return target.store


def _working_context():
    global _STREAM_CONTEXT
    if _STREAM_CONTEXT.readonly:
        _STREAM_CONTEXT = _ReaderContext(_STREAM_SCRATCH_ROOT or _STREAM_CONTEXT.root)
    return _STREAM_CONTEXT


class _RecordSpool(Sequence):
    """Append-only published records, not a mutable list or an object-identity cache.

    Indexed and iterated reads are detached. Build mutable records before publishing.
    Grouped child stores in this context remain appendable by their explicit reference.
    Foreign published stores are copied when incorporated into a new report/work store.
    """

    def __init__(self, context, store):
        self.context, self.store = context, store

    def __len__(self):
        return self.context.length(self.store)

    def __iter__(self):
        cursor = self.context.connection.execute(
            "SELECT payload FROM records WHERE store=? ORDER BY ordinal", (self.store,)
        )
        try:
            for (raw,) in cursor:
                yield self.context.decode(raw)
        finally:
            cursor.close()

    def __getitem__(self, index):
        if isinstance(index, slice):
            return _RecordSlice(self, range(*index.indices(len(self))))
        if index < 0:
            index += len(self)
        row = self.context.connection.execute(
            "SELECT payload FROM records WHERE store=? AND ordinal=?", (self.store, index)
        ).fetchone()
        if row is None:
            raise IndexError(index)
        return self.context.decode(row[0])

    def append(self, value):
        raw = self.context.encode(value)
        count = len(self)
        self.context.reserve(len(raw))
        self.context.connection.execute(
            "INSERT INTO records VALUES(?,?,?)", (self.store, count, raw)
        )
        self.context.connection.execute(
            "UPDATE stores SET length=length+1 WHERE id=?", (self.store,)
        )

    def extend(self, values):
        for value in values:
            self.append(value)

    def __reversed__(self):
        return (self[index] for index in range(len(self) - 1, -1, -1))

    def __add__(self, other):
        return _working_context().sequence(itertools.chain(self, other))

    def __radd__(self, other):
        return _working_context().sequence(itertools.chain(other, self))

    def __contains__(self, value):
        return any(row == value for row in self)

    def __eq__(self, other):
        return (
            isinstance(other, (list, _RecordSpool))
            and len(self) == len(other)
            and all(a == b for a, b in zip(self, other, strict=True))
        )

    def __ne__(self, other):
        return not self == other

    def index(self, value, start=0, stop=None):
        for i, row in enumerate(self):
            if i >= start and (stop is None or i < stop) and row == value:
                return i
        raise ValueError(value)

    def copy(self):
        return _working_context().sequence(self)

    def sort(self, *, key=None, reverse=False):
        # Merge two bounded rows at a time. At most 64 carry slots describe runs;
        # scratch rows are retained for failure reproduction, never cleaned implicitly.
        key = key or (lambda row: row)
        slots, batch, charge = [], [], 0

        def merge(left, right):
            def rows():
                a, b = iter(left), iter(right)
                missing = object()
                x, y = next(a, missing), next(b, missing)
                while x is not missing or y is not missing:
                    if (
                        y is missing
                        or x is not missing
                        and ((key(x) >= key(y)) if reverse else (key(x) <= key(y)))
                    ):
                        yield x
                        x = next(a, missing)
                    else:
                        yield y
                        y = next(b, missing)

            return _working_context().sequence(rows())

        def carry(run):
            level = 0
            while level < len(slots) and slots[level] is not None:
                run = merge(slots[level], run)
                slots[level] = None
                level += 1
            require(level < 64, "stream sort run limit")
            if level == len(slots):
                slots.append(run)
            else:
                slots[level] = run

        for row in self:
            size = _row_memory(row)
            if batch and charge + size > _STREAM_CACHE_LIMIT:
                batch.sort(key=key, reverse=reverse)
                carry(_working_context().sequence(batch))
                batch, charge = [], 0
            batch.append(row)
            charge += size
        if batch:
            batch.sort(key=key, reverse=reverse)
            carry(_working_context().sequence(batch))
        result = None
        for run in reversed(slots):
            if run is not None:
                result = run if result is None else merge(result, run)
        if result is not None:
            self.context, self.store = result.context, result.store


class _RecordSlice(_RecordSpool):
    def __init__(self, parent, positions):
        self.parent, self.positions = parent, positions
        self.context, self.store = parent.context, parent.store

    def __len__(self):
        return len(self.positions)

    def __iter__(self):
        return (self.parent[index] for index in self.positions)

    def __getitem__(self, index):
        if isinstance(index, slice):
            return _RecordSlice(self.parent, self.positions[index])
        return self.parent[self.positions[index]]


def _inventory_key(value):
    if isinstance(value, dict):
        return {key: _inventory_key(item) for key, item in value.items()}
    if isinstance(value, (tuple, list)):
        return [_inventory_key(item) for item in value]
    if isinstance(value, bool) or isinstance(value, float) and value.is_integer():
        return int(value)
    return value


def _join_key(value):
    # Preserve Python equality explicitly, rather than SQLite type affinity/collation.
    raw = json.dumps(
        _inventory_key(value), ensure_ascii=False, sort_keys=True, separators=(",", ":")
    ).encode("utf-8")
    require(len(raw) <= _STREAM_RECORD_LIMIT, "join key record too large")
    return raw


def _resource_identity(row):
    i = row["identity"]
    return tuple(i[k] for k in ("sessionId", "visit", "map", "phase")) + (
        row["kind"],
        row.get("subject"),
        row.get("slot"),
        row.get("layer"),
        row.get("highPriority"),
    )


def _resource_key(row, required=False):
    # Preserve the old candidate key, including its explicit Python numeric equality.
    want = row["expected"] if required else row.get("expected")
    if row["kind"] == "map":
        data = want if required else row["used"]
        match = (data.get("block"), data.get("tile"))
    else:
        match = (json.dumps(_inventory_key(want), sort_keys=True, separators=(",", ":")),)
    return _resource_identity(row) + match


class _ResourceRelation:
    """Count exact operand variants and ordered runs; never expand candidate pairs.

    Validation groups retain every operand their predicates read. Identical validation
    rows carry multiplicity; original locators and candidate ordinals survive reduction.
    Runs allow exact prefix counts when the legacy evaluated block stops at an exception.
    These tables are disposable scratch, not the report's publication representation.
    """

    def __init__(self, context=None, budget=None):
        global _STREAM_CONTEXT
        if context is None and _STREAM_CONTEXT is None:
            _STREAM_CONTEXT = _ReaderContext(_STREAM_SCRATCH_ROOT or repo_path("local"))
        self.context = context or _working_context()
        self.budget = budget
        self.id = self.context.store()
        self.count = 0
        self.batch, self.validation_batch, self.last, self.runs = {}, {}, {}, {}
        self.batch_bytes = 0
        self.context.connection.executescript("""
            CREATE TABLE IF NOT EXISTS resource_variants(
                relation INTEGER,key BLOB,signature BLOB,count INTEGER,first INTEGER,payload BLOB,
                PRIMARY KEY(relation,key,signature)) WITHOUT ROWID;
            CREATE TABLE IF NOT EXISTS resource_validation(
                relation INTEGER,category TEXT,signature BLOB,count INTEGER,
                first INTEGER,payload BLOB,
                PRIMARY KEY(relation,category,signature)) WITHOUT ROWID;
            CREATE TABLE IF NOT EXISTS resource_runs(
                relation INTEGER,key BLOB,first INTEGER,last INTEGER,signature BLOB,count INTEGER,
                PRIMARY KEY(relation,key,first)) WITHOUT ROWID;
        """)

    def __bool__(self):
        return bool(self.count)

    def append(self, row):
        ordinal = self.count
        self.count += 1
        locator = row.get("_captureLocator", dict(channel="resourceUses", index=ordinal))
        payload = dict(row, _captureLocator=locator, _resourceOrdinal=ordinal)
        # Exact JSON is a representation key, not the selector equality predicate.
        signature = json.dumps(row.get("used"), sort_keys=True, separators=(",", ":")).encode()

        def validation(category, operands):
            operands = dict(operands, _validationCategory=category)
            encoded = json.dumps(operands, sort_keys=True, separators=(",", ":")).encode()
            batch_key = (category, encoded)
            if batch_key not in self.validation_batch:
                self.validation_batch[batch_key] = [
                    0,
                    ordinal,
                    self.context.encode(dict(operands, _captureLocator=locator)),
                ]
                self.batch_bytes += len(encoded)
                self.batch_bytes += len(self.validation_batch[batch_key][2])
            self.validation_batch[batch_key][0] += 1

        try:
            for name in ("sessionId", "visit", "map", "phase", "observationSequence"):
                row["identity"][name]
            row["kind"]
            row["used"]
        except (KeyError, IndexError, ValueError, TypeError):
            key = None
            validation("occurrence", row)
        else:
            i = row["identity"]
            validation(
                "identity",
                dict(
                    kind="map" if row["kind"] == "map" else "entity",
                    identity={
                        k: i[k] for k in ("sessionId", "visit", "map", "observationSequence")
                    },
                    used=None,
                ),
            )
            try:
                key = _join_key(_resource_key(row))
            except (KeyError, IndexError, ValueError, TypeError) as error:
                key = None
                # Availability and candidate-key failures are different old boundaries.
                validation("texture", dict(kind=row["kind"], _keyError=type(error).__name__))
            else:
                operands = dict(kind=row["kind"], identity=dict(visit=i["visit"], phase=i["phase"]))
                if row["kind"] == "map":
                    operands.update(
                        {k: row[k] for k in ("layer", "highPriority", "pass") if k in row}
                    )
                    operands["used"] = (
                        {"word": row["used"]["word"]} if "word" in row["used"] else {}
                    )
                validation("texture", operands)
        if key is not None:
            entry = (key, signature)
            if entry not in self.batch:
                self.batch[entry] = [0, ordinal, self.context.encode(payload)]
                self.batch_bytes += len(key) + len(signature) + len(self.batch[entry][2])
            self.batch[entry][0] += 1
            if key not in self.last:
                found = self.context.connection.execute(
                    "SELECT first,signature FROM resource_runs WHERE relation=? AND key=? "
                    "ORDER BY first DESC LIMIT 1",
                    (self.id, key),
                ).fetchone()
                self.last[key] = found
            previous = self.last[key]
            if previous is None or previous[1] != signature:
                self.last[key] = (ordinal, signature)
                self.runs[key, ordinal] = [ordinal, signature, 1]
            else:
                run = self.runs.setdefault((key, previous[0]), [ordinal, signature, 0])
                run[0] = ordinal
                run[2] += 1
        if (
            len(self.batch) + len(self.validation_batch) + len(self.runs) >= 4096
            or self.batch_bytes >= 8 * 1024 * 1024
        ):
            self.flush()

    def flush(self):
        connection = self.context.connection
        connection.executemany(
            "INSERT INTO resource_variants VALUES(?,?,?,?,?,?) "
            "ON CONFLICT(relation,key,signature) DO UPDATE SET count=count+excluded.count",
            ((self.id, key, signature, *value) for (key, signature), value in self.batch.items()),
        )
        connection.executemany(
            "INSERT INTO resource_validation VALUES(?,?,?,?,?,?) "
            "ON CONFLICT(relation,category,signature) DO UPDATE SET count=count+excluded.count",
            (
                (self.id, category, signature, *value)
                for (category, signature), value in self.validation_batch.items()
            ),
        )
        connection.executemany(
            "INSERT INTO resource_runs VALUES(?,?,?,?,?,?) "
            "ON CONFLICT(relation,key,first) DO UPDATE SET "
            "last=excluded.last,count=count+excluded.count",
            ((self.id, key, first, *value) for (key, first), value in self.runs.items()),
        )
        self.batch.clear()
        self.validation_batch.clear()
        self.last.clear()
        self.runs.clear()
        self.batch_bytes = 0
        self.context.flush()
        if self.budget is not None:
            self.budget.checkpoint("resource count spill")

    def validations(self, category):
        self.flush()
        cursor = self.context.connection.execute(
            "SELECT count,payload FROM resource_validation "
            "WHERE relation=? AND category=? ORDER BY first",
            (self.id, category),
        )
        try:
            for count, raw in cursor:
                row = self.context.decode(raw)
                if row["_validationCategory"] == category:
                    yield row, count
        finally:
            cursor.close()

    def variants(self, key):
        cursor = self.context.connection.execute(
            "SELECT signature,count,first,payload FROM resource_variants "
            "WHERE relation=? AND key=? ORDER BY first",
            (self.id, _join_key(key)),
        )
        try:
            for signature, count, first, raw in cursor:
                yield signature, count, first, self.context.decode(raw)
        finally:
            cursor.close()

    def prefix_count(self, key, signature, stop):
        # A run contains only one signature; global ordinals need not be consecutive.
        # The stop is another variant's first occurrence, so it cannot bisect this run.
        row = self.context.connection.execute(
            "SELECT coalesce(sum(count),0) FROM resource_runs "
            "WHERE relation=? AND key=? AND signature=? AND last<?",
            (self.id, _join_key(key), signature, stop),
        ).fetchone()
        return row[0]

    def published_variants(self):
        self.flush()
        cursor = self.context.connection.execute(
            "SELECT key,count,first,payload FROM resource_variants "
            "WHERE relation=? ORDER BY key,first",
            (self.id,),
        )
        try:
            for key, count, first, raw in cursor:
                row = self.context.decode(raw)
                yield dict(
                    candidateKey=json.loads(key),
                    count=count,
                    firstOrdinal=first,
                    used=row["used"],
                    locator=row["_captureLocator"],
                )
        finally:
            cursor.close()


class _OccurrenceMap:
    """Explicit native-key join, detached reads, assignment updates, first-seen order."""

    def __init__(self):
        self.context = _working_context()
        self.store = self.context.store()

    def _key(self, key):
        require(
            key is None or isinstance(key, (int, float)) and int(key) == key and key >= 0,
            "invalid native occurrence key",
        )
        return _join_key(key)

    def __len__(self):
        return self.context.length(self.store)

    def __iter__(self):
        cursor = self.context.connection.execute(
            "SELECT original FROM entries WHERE store=? ORDER BY position", (self.store,)
        )
        try:
            for (raw,) in cursor:
                yield self.context.decode(raw)
        finally:
            cursor.close()

    def __getitem__(self, key):
        row = self.context.connection.execute(
            "SELECT payload FROM entries WHERE store=? AND key=?", (self.store, self._key(key))
        ).fetchone()
        if row is None:
            raise KeyError(key)
        return self.context.decode(row[0])

    def __setitem__(self, key, value):
        encoded = self._key(key)
        original, raw = self.context.encode(key), self.context.encode(value)
        self.context.reserve(len(raw))
        updated = self.context.connection.execute(
            "UPDATE entries SET payload=? WHERE store=? AND key=?", (raw, self.store, encoded)
        ).rowcount
        if not updated:
            self.context.connection.execute(
                "INSERT INTO entries VALUES(?,?,?,?,?)",
                (self.store, encoded, len(self), original, raw),
            )
            self.context.connection.execute(
                "UPDATE stores SET length=length+1 WHERE id=?", (self.store,)
            )

    def __contains__(self, key):
        return (
            self.context.connection.execute(
                "SELECT 1 FROM entries WHERE store=? AND key=?", (self.store, self._key(key))
            ).fetchone()
            is not None
        )

    def get(self, key, default=None):
        try:
            return self[key]
        except KeyError:
            return default

    def setdefault(self, key, default):
        if key not in self:
            if isinstance(default, list) and not default:
                default = self.context.sequence()
            self[key] = default
        return self[key]

    def values(self):
        return _working_context().sequence(self[key] for key in self)

    def items(self):
        return ((key, self[key]) for key in self)


class _ResourceUseGroups(_OccurrenceMap):
    """Composite resource keys map directly to explicit child record stores."""

    def _key(self, key):
        return _join_key(key)

    def setdefault(self, key, default):
        if key not in self:
            self[key] = self.context.sequence(default)
        return self[key]


def _resource_groups():
    return _ResourceUseGroups() if _STREAM_CONTEXT is not None else {}


class _InventoryKeys:
    """Equality-only composite inventory with indexed canonical keys."""

    def __init__(self, values=()):
        self.values = _ResourceUseGroups()
        for value in values:
            self.add(value)

    def __len__(self):
        return len(self.values)

    def __contains__(self, key):
        return key in self.values

    def add(self, key):
        if key not in self.values:
            self.values[key] = True


def _value_set(values=()):
    return _InventoryKeys(values) if _STREAM_CONTEXT is not None else set(values)


class _OccurrenceSet:
    def __init__(self):
        self.values = _OccurrenceMap()

    def add(self, key):
        if key not in self.values:
            self.values[key] = True

    def __iter__(self):
        return iter(self.values)

    def __len__(self):
        return len(self.values)

    def __contains__(self, key):
        return key in self.values


def _occurrence_set(values=()):
    result = _OccurrenceSet() if _STREAM_CONTEXT is not None else set()
    for value in values:
        result.add(value)
    return result


def _group_rows(groups, key):
    if key not in groups:
        groups[key] = _bounded_list()
    return groups[key]


def _occurrence_dict(values):
    result = _occurrence_map()
    for key, value in values:
        result[key] = value
    return result


_NO_STREAM_VALUES = object()


def _bounded_list(values=_NO_STREAM_VALUES):
    if _STREAM_CONTEXT is None:
        return [] if values is _NO_STREAM_VALUES else list(values)
    if values is _NO_STREAM_VALUES:
        return _working_context().sequence()
    iterator, small, charge = iter(values), [], 0
    for row in iterator:
        size = _row_memory(row)
        if len(small) >= 256 or charge + size > 1024 * 1024:
            return _working_context().sequence(itertools.chain(small, (row,), iterator))
        small.append(row)
        charge += size
    return small


def _bounded_sorted(values, *, key=None, reverse=False):
    if _STREAM_CONTEXT is None:
        return sorted(values, key=key, reverse=reverse)
    result = _bounded_list(values)
    result.sort(key=key, reverse=reverse)
    return result


def _occurrence_map():
    return _OccurrenceMap() if _STREAM_CONTEXT is not None else {}


def _pack_stream(value, context):
    if isinstance(value, _RecordSpool):
        return {"_sf2Rows": context.import_rows(value)}
    if isinstance(value, dict):
        return {k: _pack_stream(v, context) for k, v in value.items()}
    if isinstance(value, (list, tuple)):
        return [_pack_stream(v, context) for v in value]
    return value


def _unpack_stream(value, context):
    if isinstance(value, dict) and set(value) == {"_sf2Rows"}:
        store = value["_sf2Rows"]
        require(type(store) is int and store > 0, "invalid derived row reference")
        context.length(store)  # Missing storage must fail at readback, not look empty.
        return _RecordSpool(context, store)
    if isinstance(value, dict):
        return {k: _unpack_stream(v, context) for k, v in value.items()}
    if isinstance(value, list):
        return [_unpack_stream(v, context) for v in value]
    return value


class ResourceBudgetExceeded(RuntimeError):
    pass


def _private_bytes():
    if os.name != "nt":
        return None
    import ctypes
    from ctypes import wintypes

    class Counters(ctypes.Structure):
        _fields_ = [("cb", wintypes.DWORD), ("PageFaultCount", wintypes.DWORD)] + [
            (name, ctypes.c_size_t)
            for name in (
                "PeakWorkingSetSize",
                "WorkingSetSize",
                "QuotaPeakPagedPoolUsage",
                "QuotaPagedPoolUsage",
                "QuotaPeakNonPagedPoolUsage",
                "QuotaNonPagedPoolUsage",
                "PagefileUsage",
                "PeakPagefileUsage",
                "PrivateUsage",
            )
        ]

    counters = Counters()
    counters.cb = ctypes.sizeof(counters)
    require(
        ctypes.windll.psapi.GetProcessMemoryInfo(
            ctypes.c_void_p(-1),
            ctypes.byref(counters),
            counters.cb,
        ),
        "cannot measure process private bytes",
    )
    return counters.PrivateUsage


class _ResourceBudget:
    def __init__(self, root, input_bytes, baseline=None, *, selected=False):
        self.root, self.input_bytes, self.baseline = root, input_bytes, baseline
        self.selected, self.selected_bytes = selected, 0
        self.started, self.last = time.monotonic(), 0
        self.peak_logical, self.peak_private = 0, 0
        self.headroom = shutil.disk_usage(root).free
        self.limit = 64 * 1024 * 1024 if selected else input_bytes
        if self.headroom < input_bytes + 6 * 1024**3:
            raise ResourceBudgetExceeded(
                "insufficient physical reserve for conservative publication"
            )

    def observe(self):
        logical = sum(p.stat().st_size for p in self.root.rglob("*") if p.is_file())
        self.peak_logical = max(self.peak_logical, logical)
        self.limit = self.selected_bytes + 64 * 1024**2 if self.selected else self.input_bytes
        if logical > self.limit:
            raise ResourceBudgetExceeded("resource comparison exceeded logical byte budget")
        return logical

    def checkpoint(self, stage, *, force=False):
        now = time.monotonic()
        if not force and now - self.last < 5:
            return
        self.last = now
        logical = self.observe()
        private = _private_bytes()
        if private is not None:
            self.peak_private = max(self.peak_private, private)
        self.limit = self.selected_bytes + 64 * 1024**2 if self.selected else self.input_bytes
        print(
            json.dumps(
                dict(
                    stage=stage,
                    elapsedSeconds=round(now - self.started, 2),
                    selectedDependencyBytes=self.selected_bytes,
                    logicalBytes=logical,
                    privateBytes=private,
                )
            ),
            flush=True,
        )
        if now - self.started > 20 * 60:
            raise ResourceBudgetExceeded("resource comparison exceeded 20 minutes")
        if (
            private is not None
            and self.baseline is not None
            and private - self.baseline > 256 * 1024**2
        ):
            raise ResourceBudgetExceeded("resource comparison exceeded incremental private memory")
        if shutil.disk_usage(self.root).free < 6 * 1024**3:
            raise ResourceBudgetExceeded("resource comparison breached physical reserve")

    def receipt(self):
        return dict(
            elapsedSeconds=time.monotonic() - self.started,
            peakNewLogicalBytes=self.peak_logical,
            logicalByteLimit=self.limit,
            selectedDependencyBytes=self.selected_bytes,
            sourceOnlyPrivateBytes=self.baseline,
            sampledPeakPrivateBytes=self.peak_private,
            incrementalPrivateByteLimit=256 * 1024**2,
            initialPhysicalFreeBytes=self.headroom,
            finalPhysicalFreeBytes=shutil.disk_usage(self.root).free,
        )

    def release_scratch(self):
        context = getattr(self, "scratch_context", None)
        if context is None:
            return
        root = context.root.resolve()
        require(
            root.parent == self.root.resolve() and root.name.startswith("reader-"),
            "resource scratch cleanup escapes owned run",
        )
        context.close()
        shutil.rmtree(root)


def _resource_selected(identity, scope):
    return all(
        scope.get(key) is None or identity.get(key) == scope[key]
        for key in ("sessionId", "visit", "observationSequence")
    )


def resource_pair_details(capture, requirement_result, *, limit):
    """Replay at most limit original pairs from a previously verified raw capture.

    This preview has no acceptance verdict and makes no new whole-capture integrity
    claim. It stops on the requested limit and builds no derived stores. Source recipe
    checks and exact logical counts remain in the independently readable report.
    """
    require(type(limit) is int and 0 < limit <= 1000, "pair detail limit must be 1..1000")
    key = _join_key(requirement_result["candidateKey"])
    capture = Path(capture) if Path(capture).is_absolute() else repo_path(capture)
    descriptors, descriptor_bytes, emitted = {}, 0, 0
    with capture.open("rb") as source:
        while raw := source.readline(_STREAM_RECORD_LIMIT + 1):
            require(
                len(raw) <= _STREAM_RECORD_LIMIT and raw.endswith(b"\n"),
                "oversized/truncated capture detail",
            )
            row = json.loads(raw)
            channel, payload = row["channel"], row["payload"]
            if channel == "descriptorReset":
                descriptors.clear()
                descriptor_bytes = 0
            elif channel == "resourceDescriptors":
                require(payload["id"] not in descriptors, "duplicate resource descriptor")
                descriptor_bytes += _row_memory(payload["selector"])
                require(
                    len(descriptors) < 4096 and descriptor_bytes <= _STREAM_CACHE_LIMIT,
                    "resource descriptor lifetime bound",
                )
                descriptors[payload["id"]] = payload["selector"]
            elif channel in ("resourceUses", "drawUses"):
                uses = payload["uses"] if channel == "drawUses" else (payload,)
                for use_index, use in enumerate(uses):
                    used = dict(use, identity=payload["identity"])
                    if _join_key(_resource_key(used)) != key:
                        continue
                    bound = used["used"]
                    selector = bound.get("selector", bound)
                    if "captureDescriptor" in selector:
                        require(
                            selector["captureDescriptor"] in descriptors,
                            "missing/detached resource descriptor",
                        )
                        resolved = descriptors[selector["captureDescriptor"]]
                        used["used"] = (
                            dict(bound, selector=resolved) if "selector" in bound else resolved
                        )
                    yield dict(
                        expected=requirement_result["expected"],
                        actualUse=used,
                        locator=dict(
                            captureSequence=row["captureSequence"],
                            channel=channel,
                            index=row["index"],
                            useIndex=use_index if channel == "drawUses" else None,
                        ),
                    )
                    emitted += 1
                    if emitted == limit:
                        return


def _resource_state(state, family):
    fields = {"sessionId", "map", "observationSequence", "revision", "presentation"}
    if family in ("all", "map", "entity"):
        fields.add("cameraProjection")
    if family in ("all", "entity"):
        fields.update(
            ("entities", "nod", "portraitProjection", "portraitWork", "portraitFlags", "portraitId")
        )
    result = {k: v for k, v in state.items() if k in fields}
    if isinstance(result.get("presentation"), dict):
        result["presentation"] = {
            k: v
            for k, v in result["presentation"].items()
            if k in ("activeCue", "cameraX", "cameraY")
        }
    projection = result.get("cameraProjection")
    if isinstance(projection, dict):
        selected = {
            "sessionId",
            "revision",
            "map",
            "observationSequence",
            "simulationTick",
            "token",
            "drawSequence",
        }
        if family in ("all", "map"):
            selected.update(
                ("background", "foreground", "backgroundHigh", "foregroundHigh", "occlusionDraws")
            )
        if family in ("all", "entity"):
            selected.add("actors")
        result["cameraProjection"] = {k: v for k, v in projection.items() if k in selected}
    return result


def _read_capture(path, resource_scope=None, budget=None):
    global _STREAM_CONTEXT
    context = _ReaderContext(_STREAM_SCRATCH_ROOT or path.resolve().parent)
    if budget is not None:
        budget.scratch_context = context
        context.resource_budget = budget
    _STREAM_CONTEXT = context
    channels, counts, descriptors, terminal = {}, {}, {}, None
    metadata = {}
    metadata_keys = {
        "admissionSnapshot",
        "rawTextBoundary",
        "musicLogicalEnd",
        "musicPlainInput",
        "joinReturn",
    }
    previous, descriptor_bytes = 0, 0
    relation = _ResourceRelation(context, budget) if resource_scope is not None else None
    dependency_bytes, descriptor_sizes, counted_descriptors = 0, {}, set()
    current_visit, selected_maps, context_sessions = 0, set(), set()
    predecessor, selected_context = {}, set()

    def selected_resource(row):
        kind = row.get("kind")
        family = "map" if kind == "map" else "entity"
        return resource_scope["family"] in ("all", family) and _resource_selected(
            row.get("identity") or {}, resource_scope
        )

    def retain_descriptor(selector):
        nonlocal dependency_bytes
        if "captureDescriptor" in selector:
            ident = selector["captureDescriptor"]
            if ident not in counted_descriptors:
                dependency_bytes += descriptor_sizes.get(ident, 0)
                counted_descriptors.add(ident)

    with path.open("rb") as source:
        while raw := source.readline(_STREAM_RECORD_LIMIT + 1):
            require(
                len(raw) <= _STREAM_RECORD_LIMIT and raw.endswith(b"\n"),
                "oversized/truncated capture record",
            )
            row = json.loads(raw)
            require(terminal is None, "capture record follows terminal")
            require(
                type(row["captureSequence"]) is int and row["captureSequence"] == previous + 1,
                "missing/duplicate capture sequence",
            )
            previous = row["captureSequence"]
            channel, payload = row["channel"], row["payload"]
            require(
                isinstance(channel, str) and 0 < len(channel) <= 64 and isinstance(payload, dict),
                "invalid capture record shape",
            )
            _row_memory(payload)
            require(previous != 1 or channel == "header", "capture header must be first")
            require(channel in counts or len(counts) < 64, "capture channel bound")
            require(
                type(row["index"]) is int and row["index"] == counts.get(channel, 0),
                "missing/duplicate channel ordinal",
            )
            counts[channel] = row["index"] + 1
            if channel == "header":
                require(
                    previous == 1 and payload["format"] == "sf2-observation-jsonl-v1",
                    "invalid capture header",
                )
            elif channel == "descriptorReset":
                descriptors.clear()
                descriptor_bytes = 0
                descriptor_sizes.clear()
                counted_descriptors.clear()
            elif channel == "resourceDescriptors":
                require(payload["id"] not in descriptors, "duplicate resource descriptor")
                descriptor_bytes += _row_memory(payload["selector"])
                require(
                    len(descriptors) < 4096 and descriptor_bytes <= _STREAM_CACHE_LIMIT,
                    "resource descriptor lifetime bound",
                )
                descriptors[payload["id"]] = payload["selector"]
                descriptor_sizes[payload["id"]] = len(raw)
            elif channel == "resourceUses":
                used = payload["used"]
                selector = used.get("selector", used)
                if "captureDescriptor" in selector:
                    require(
                        selector["captureDescriptor"] in descriptors,
                        "missing/detached resource descriptor",
                    )
                    if resource_scope is not None and selected_resource(payload):
                        retain_descriptor(selector)
                    if "selector" in used:
                        used["selector"] = descriptors[selector["captureDescriptor"]]
                    else:
                        payload["used"] = descriptors[selector["captureDescriptor"]]
            if channel == "drawUses":
                if resource_scope is None and "resourceUses" not in channels:
                    channels["resourceUses"] = context.sequence()
                retained_draw, selected_draw_bytes = False, 0
                for use_index, use in enumerate(payload["uses"]):
                    used = use["used"]
                    selector = used.get("selector", used)
                    require(
                        "captureDescriptor" in selector
                        and selector["captureDescriptor"] in descriptors,
                        "missing/detached draw resource descriptor",
                    )
                    use_row = dict(use, identity=payload["identity"])
                    selected = resource_scope is None or selected_resource(use_row)
                    if not selected:
                        continue
                    retained_draw = True
                    if resource_scope is not None:
                        retain_descriptor(selector)
                        selected_draw_bytes += len(json.dumps(use, separators=(",", ":")).encode())
                    if "selector" in used:
                        used["selector"] = descriptors[selector["captureDescriptor"]]
                    else:
                        use["used"] = descriptors[selector["captureDescriptor"]]
                    use_row = dict(use, identity=payload["identity"])
                    use_row["_captureLocator"] = dict(
                        captureSequence=previous,
                        channel=channel,
                        index=row["index"],
                        useIndex=use_index,
                    )
                    if resource_scope is None:
                        channels["resourceUses"].append(use_row)
                    else:
                        relation.append(use_row)
                        selected_maps.add(payload["identity"].get("map"))
                if retained_draw and resource_scope is not None:
                    dependency_bytes += selected_draw_bytes + len(
                        json.dumps(
                            dict(row, payload=dict(identity=payload["identity"], uses=[])),
                            separators=(",", ":"),
                        ).encode()
                    )
                if budget is not None:
                    budget.selected_bytes = dependency_bytes
                    budget.checkpoint("scan and resource reduction")
                continue
            if channel == "captureMetadata":
                require(
                    set(payload) == {"key", "value"}
                    and isinstance(payload["key"], str)
                    and payload["key"] in metadata_keys
                    and payload["key"] not in metadata,
                    "invalid/duplicate capture metadata key",
                )
                metadata[payload["key"]] = payload["value"]
                continue
            if resource_scope is None and channel in ("resourceUses", "resourceRequirements"):
                payload["_captureLocator"] = dict(
                    captureSequence=previous,
                    channel=channel,
                    index=row["index"],
                )
            if resource_scope is not None:
                keep = False
                if channel in ("header", "terminal"):
                    keep = True
                elif channel in ("resourceRequirements", "resourceUses"):
                    keep = selected_resource(payload)
                    if keep:
                        payload["_captureLocator"] = dict(
                            captureSequence=previous, channel=channel, index=row["index"]
                        )
                        selected_maps.add(payload["identity"].get("map"))
                        if channel == "resourceUses":
                            relation.append(payload)
                            dependency_bytes += len(raw)
                            continue
                elif channel in ("samples", "consumerBoundaries", "warpRecords"):
                    state = payload.get("state") or {}
                    session = state.get("sessionId")
                    if resource_scope.get("sessionId") in (None, session):
                        if session:
                            context_sessions.add(session)
                        transitions = [
                            e
                            for e in payload.get("result", {}).get("observations", [])
                            if e.get("Kind") == "map-transferred"
                            or e.get("Detail") == "LoadSceneMap"
                        ]
                        if transitions:
                            current_visit = transitions[-1]["Sequence"]
                        identity = dict(state, visit=current_visit)
                        keep = _resource_selected(identity, resource_scope)
                        if resource_scope["family"] == "scene":
                            keep = False
                        original_selected = keep
                        if not keep and channel == "warpRecords":
                            # Required predecessor visit transitions, without unrelated projections.
                            payload = dict(
                                state={
                                    k: state[k]
                                    for k in ("sessionId", "observationSequence", "map")
                                    if k in state
                                },
                                result=dict(observations=transitions),
                            )
                            keep = bool(transitions)
                        if (
                            keep
                            and _resource_selected(identity, resource_scope)
                            and state.get("map")
                        ):
                            selected_maps.add(state["map"])
                        if original_selected:
                            selected_context.add(channel)
                            payload = dict(
                                state=_resource_state(state, resource_scope["family"]),
                                result=dict(observations=transitions),
                            )
                        elif channel not in selected_context:
                            predecessor[channel] = dict(
                                captureSequence=previous,
                                index=row["index"],
                                state={
                                    k: state[k]
                                    for k in ("sessionId", "observationSequence", "map")
                                    if k in state
                                },
                            )
                elif channel == "sceneObservations":
                    keep = resource_scope["family"] in ("all", "scene") and _resource_selected(
                        dict(payload, visit=current_visit),
                        resource_scope,
                    )
                if not keep:
                    continue
                if channel in ("samples", "consumerBoundaries", "warpRecords") and isinstance(
                    payload.get("state"), dict
                ):
                    payload["state"]["_captureLocator"] = dict(
                        captureSequence=previous,
                        channel=channel,
                        index=row["index"],
                    )
                dependency_bytes += min(
                    len(raw),
                    len(
                        json.dumps(
                            dict(row, payload=payload),
                            separators=(",", ":"),
                        ).encode("utf-8")
                    ),
                )
                if budget is not None:
                    budget.selected_bytes = dependency_bytes
                    budget.checkpoint("scan and resource reduction")
            if channel == "terminal":
                terminal = payload
            else:
                if channel not in channels:
                    require(len(channels) < 64, "capture channel bound")
                    channels[channel] = context.sequence()
                channels[channel].append(payload)
    require(terminal is not None and counts.get("header") == 1, "capture missing terminal/header")
    result = dict(terminal)
    if metadata or "captureMetadataKeys" in result:
        declared = result.pop("captureMetadataKeys", None)
        require(
            isinstance(declared, list)
            and len(declared) == len(metadata_keys)
            and all(isinstance(key, str) for key in declared)
            and set(declared) == metadata_keys
            and set(metadata) == metadata_keys,
            "missing/invalid capture metadata declaration",
        )
        require(
            not metadata_keys.intersection(result) and not metadata_keys.intersection(channels),
            "capture metadata conflicts with terminal/channel",
        )
        for key in metadata_keys:
            result[key] = metadata[key]
    result.update(channels)
    if resource_scope is not None:
        relation.flush()
        result["resourceUses"] = relation
        result["resourceScope"] = dict(
            resource_scope,
            maps=sorted(m for m in selected_maps if m),
            contextSessions=sorted(context_sessions),
            predecessors=predecessor,
            dependencyBytes=dependency_bytes,
        )
    result["captureIntegrity"] = dict(
        records=previous,
        channelCounts=counts,
        terminal=True,
        pageCacheTargetBytes=_STREAM_CACHE_LIMIT,
        recordLimit=_STREAM_RECORD_LIMIT,
        decodedRowLimit=_STREAM_ROW_MEMORY_LIMIT,
        scratch=context.root.relative_to(repo_path(".")).as_posix(),
    )
    context.flush()
    return result


def read(path):
    path = (Path(path) if Path(path).is_absolute() else repo_path(path)).resolve()
    with path.open("rb") as source:
        first = source.readline(_STREAM_RECORD_LIMIT + 1)
    try:
        header = json.loads(first)
    except (ValueError, UnicodeDecodeError):
        header = None
    if isinstance(header, dict) and header.get("channel") == "header":
        return _read_capture(path)
    value = json.loads(path.read_bytes())
    if isinstance(value, dict):
        format_name = value.get("streamReportFormat")
        require(
            format_name != "sf2-h4-stream-report-v1",
            "old private spool report requires its retained reader version",
        )
        if format_name == "sf2-h4-sqlite-report-v1":
            global _STREAM_CONTEXT
            name = value["streamDatabase"]
            require(
                isinstance(name, str) and Path(name).name == name, "invalid report companion name"
            )
            database = (path.parent / name).resolve()
            require(
                database.parent == path.parent and database.is_file(), "missing report companion"
            )
            context = _ReaderContext(database=database, readonly=True)
            _STREAM_CONTEXT = context
            return _unpack_stream(value, context)
    return value


def write(path, value, *, resource_budget=None, defer_publication=False):
    path = (Path(path) if Path(path).is_absolute() else repo_path(path)).resolve()
    require(
        path.is_relative_to(repo_path("local").resolve()) and not path.exists(),
        "output must be fresh beneath this worktree's local/",
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    if _STREAM_CONTEXT is not None:
        database = path.with_name(path.name + ".sqlite")
        context = _ReaderContext(database=database)
        if resource_budget is not None:
            context.resource_budget = resource_budget
        try:
            value = _pack_stream(
                dict(
                    value,
                    streamReportFormat="sf2-h4-sqlite-report-v1",
                    streamDatabase=database.name,
                ),
                context,
            )
            # Imports are only a bounded copy memo. A published bundle has no source DB paths.
            context.connection.execute("DELETE FROM imports")
            context.flush()
        finally:
            context.close()
    # Publish the entry point only after its complete companion has committed and closed.
    # Failed companion/JSON writes remain private discovery artifacts; never reuse that name.
    partial = path.with_name(path.name + ".partial")
    with partial.open("x", encoding="utf-8") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
        stream.flush()
        os.fsync(stream.fileno())
    if defer_publication:
        return partial
    partial.rename(path)


def reference(path):
    result = read(path)
    require(
        (result["sourceCommit"], result["romSha256"], result["upstream"])
        == (SOURCE, ROM, UPSTREAM),
        "not the accepted original reference",
    )
    require(
        [row["segment"] for row in result["lineage"]] == list(range(68, 87)),
        "not the selected winning lineage",
    )
    extension = result.get("postVictoryInput")
    if extension is not None:
        require(
            extension["sourceCommit"] == EXTENSION_SOURCE
            and [row["segment"] for row in extension["lineage"]] == list(range(2, 21))
            and extension["before"]["position"]
            == [result["endpoint"]["state"]["x"], result["endpoint"]["state"]["y"]]
            and extension["after"]["source"]["record"].startswith("prepared-20/runtime/"),
            "not the accepted original extension binding",
        )
    return result


def make_plan(ref, evidence):
    """Decode semantic field inputs, including no-displacement facing changes.

    A movement callback is a consumed displacement, not one request or held frame.
    An idle field C starts interaction; C during pending script is not a new one.
    No wall-clock/held-frame duration is used as a remake input duration.
    """
    evidence = evidence.resolve(strict=True)
    require(
        evidence.name == "issue496" and evidence.parent.name == "local",
        "select the accepted local/issue496 evidence root",
    )
    cutoff = ref["encounter"][0]["source"]["order"]
    moves = [m for m in ref["movements"] if m["source"]["order"] < cutoff]
    requests = {q["ordinal"]: q for q in ref["requests"]}
    occupied = {m["source"]["inputOrdinal"] for m in moves}
    events = []
    for move in moves:
        request = requests[move["source"]["inputOrdinal"]]
        require(
            move["direction"] in DIRECTIONS.values()
            and request["physicalFields"][-1] == move["direction"],
            "movement has no matching cardinal controller request",
        )
        events.append(
            dict(
                action=move["direction"],
                source=move["source"],
                before=move["state"],
                boundary="field",
                kind="movement",
            )
        )
    for previous, request in zip(ref["requests"], ref["requests"][1:], strict=False):
        if request["source"]["order"] >= cutoff:
            break
        if request["role"] != "controller-schedule":
            continue
        before, after = previous["resultState"], request["resultState"]
        action = request["physicalFields"][-1]
        kind = None
        if (
            action == "C"
            and before["pendingReturns"] == 0
            and before["lastConsumerPoll"]["kind"] == "WaitForEvent-action"
        ):
            require(
                not any(before["activeConsumers"].values())
                if isinstance(before["activeConsumers"], dict)
                else not before["activeConsumers"],
                "interaction consumer still active",
            )
            action, kind = "Confirm", "interaction"
        elif (
            action in DIRECTIONS.values()
            and request["ordinal"] not in occupied
            and all(before[k] == after[k] for k in ("map", "rawX", "rawY"))
            and before["facing"] != after["facing"]
        ):
            kind = "facing"
        if kind:
            events.append(
                dict(
                    action=action,
                    kind=kind,
                    boundary="field",
                    before=before,
                    source={**request["source"], "inputOrdinal": request["ordinal"]},
                    beforeSource=previous["resultSource"],
                    afterSource=request["resultSource"],
                )
            )
    events.sort(key=lambda e: e["source"]["order"])
    for ordinal, event in enumerate(events, 1):
        event["ordinal"] = ordinal

    # The reference retains the committed decision but not the movement polls.
    # Read only that decision's accepted segment; match actual one-frame direction
    # requests to original nonneutral consumer reads, not an invented shortest path.
    decision = ref["decisions"][0]
    segment = int(decision["source"]["record"].split("/")[0].removeprefix("prepared-"))
    require(68 <= segment <= 86, "decision outside accepted lineage")
    path = evidence / f"prepared-{segment}/runtime/checkpoints.jsonl"
    inputs = []
    for line, row in rows(path):
        if row["order"] >= decision["source"]["order"]:
            break
        if (
            row["kind"] != "battle:movement-input"
            or row["facts"]["poll"]["turn"] != decision["turn"]
        ):
            continue
        value = row["facts"]["input"]
        if value not in DIRECTIONS:
            continue
        request = next(
            q
            for q in ref["requests"]
            if q["source"]["order"] < row["order"] < q["resultSource"]["order"]
        )
        require(
            request["appliedFrames"] == 1 and request["physicalFields"][-1] == DIRECTIONS[value],
            "first battle movement requires additional held-input normalization",
        )
        inputs.append(
            dict(
                action=DIRECTIONS[value],
                source={**location(segment, line, row), "inputOrdinal": request["ordinal"]},
            )
        )
    require(inputs and decision["action"] == "Stay", "bounded first-STAY binding unavailable")
    # csc textbox callbacks omit direct DisplayText callers. Use the actual shared
    # DisplayText entry for both forms, retaining its shimmed-completion limitation.
    text_limit = ref["turns"][0]["source"]["order"]
    last_segment = int(ref["turns"][0]["source"]["record"].split("/")[0].removeprefix("prepared-"))
    request_orders = [q["source"]["order"] for q in ref["requests"]]
    texts = []
    for number in range(68, last_segment + 1):
        for line, row in rows(evidence / f"prepared-{number}/runtime/checkpoints.jsonl"):
            if row["order"] >= text_limit:
                break
            if row["kind"] == "DisplayText:entry":
                index = bisect_right(request_orders, row["order"]) - 1
                texts.append(
                    dict(
                        textId=row["facts"]["target"],
                        source={
                            **location(number, line, row),
                            "inputOrdinal": ref["requests"][index]["ordinal"]
                            if index >= 0
                            else None,
                        },
                    )
                )
    return dict(
        sourceCommit=SOURCE,
        scope="natural-route-and-first-STAY-diagnostic",
        acknowledgeDialogue=True,
        steps=events,
        choices=ref["choices"],
        texts=texts,
        firstDecision=decision,
        firstDecisionInputs=inputs,
        nextDecision=ref["decisions"][1],
        acknowledgementBoundary="Original DisplayText shim: natural acknowledgements Unavailable",
    )


def source_actor(value):
    if value is None:
        return None
    family, index = value.split("-")
    return int(index) + (128 if family == "enemy" else 0)


def compare(ref, plan, actual_path, host_log, host_exit):
    require(plan["sourceCommit"] == SOURCE, "plan/reference source mismatch")
    samples, signals, terminal = [], [], None
    record_sequences = []
    for _, row in rows(actual_path):
        if not row.get("terminal"):
            record_sequences.append(row["sequence"])
        if "result" in row:
            signals.append(row)
        elif row.get("terminal"):
            terminal = row
        else:
            samples.append(row)
    require(samples and terminal, "actual run lacks admission/terminal record")
    labels = {s["label"]: s for s in samples if s["label"]}
    admission = labels["admission"]
    assertions = []

    def check(
        layer,
        name,
        expected,
        actual,
        sample,
        original,
        reason="",
        unavailable=False,
        scope="baseline",
    ):
        unavailable = unavailable or (actual is None and expected is not None)
        result = "Unavailable" if unavailable else "PASS" if expected == actual else "FAIL"
        assertions.append(
            dict(
                layer=layer,
                assertion=name,
                checkpoint=sample.get("label"),
                original=dict(
                    owner=original.get("owner", OWNER),
                    commit=original.get("commit", SOURCE),
                    record=original,
                ),
                actualSequence=sample.get("sequence"),
                inputOrdinal=sample.get("inputOrdinal"),
                logicalStep=sample.get("logicalStep"),
                expected=expected,
                actual=actual,
                result=result,
                reason=reason or ("semantic equality" if result == "PASS" else "observed mismatch"),
                scope=scope,
            )
        )

    a = admission["state"]
    original = ref["admission"]
    o = original["state"]
    player = next((e for e in a.get("entities", []) if e["id"] == "entity-0"), {})
    for key, expected, value in (
        ("map", f"map-{o['map']}", a.get("map")),
        ("player.x", o["x"] * 384, player.get("x")),
        ("player.y", o["y"] * 384, player.get("y")),
        ("player.facing", o["facing"], player.get("facing")),
        ("gold", original["accounting"]["gold"], a.get("gold")),
        ("mainSeed", int.from_bytes(bytes(o["rngBytes"]), "big"), a.get("mainSeed")),
        ("joined", original["accounting"]["joined"], (a.get("partyLists") or {}).get("Joined")),
        ("active", original["accounting"]["active"], (a.get("partyLists") or {}).get("Active")),
    ):
        check(1, key, expected, value, admission, original["source"])
    for ally in original["accounting"]["allies"][:3]:
        actual = next(
            (p for p in a.get("party", []) if p["Actor"]["Value"] == f"ally-{ally['id']}"), {}
        )
        for expected_key, actual_key in (
            ("hpCurrent", "Hp"),
            ("mpCurrent", "Mp"),
            ("statusEffects", "Status"),
        ):
            check(
                1,
                f"ally-{ally['id']}.{expected_key}",
                ally[expected_key],
                actual.get(actual_key),
                admission,
                original["source"],
                unavailable=actual_key not in actual,
            )
        loadout = actual.get("SourceLoadout")
        items = loadout.get("Items") if isinstance(loadout, dict) else None
        if loadout is None:
            item_reason = "Admission SourceLoadout is absent/null; later inventories cannot fill it"
        elif not isinstance(items, list):
            item_reason = "Admission SourceLoadout has no supported typed Items array"
        else:
            item_reason = "Compare the admission typed SourceLoadout.Items slot array"
        check(
            1,
            f"ally-{ally['id']}.items",
            [x["raw"] for x in ally["items"]],
            items if isinstance(items, list) else loadout,
            admission,
            original["source"],
            item_reason,
            not isinstance(items, list),
        )
    npc_differences = []
    for entity in a.get("entities", []):
        expected = next(
            (e for e in ref["inherited"]["entities"] if e["physical"] == entity["slot"]), None
        )
        if expected:
            for key in ("x", "y", "facing"):
                if expected[key] != entity[key]:
                    npc_differences.append(
                        dict(
                            slot=entity["slot"],
                            entity=entity["id"],
                            field=key,
                            expected=expected[key],
                            actual=entity[key],
                        )
                    )
    check(
        1,
        "relevant NPC continuation phase",
        "justified phase mapping",
        npc_differences,
        admission,
        ref["inherited"]["source"],
        "Raw admission deltas retained; relevance and phase/timing normalization remain Unknown",
        True,
    )

    for step in plan["steps"]:
        sample = labels.get(f"before:{step['ordinal']}")
        if sample is None:
            break
        state, expected = sample["state"], step["before"]
        p = next((e for e in state.get("entities", []) if e["id"] == "entity-0"), {})
        for name, e, value in (
            ("map", f"map-{expected['map']}", state.get("map")),
            ("x", expected["x"] * 384, p.get("x")),
            ("y", expected["y"] * 384, p.get("y")),
        ):
            check(2, name, e, value, sample, step.get("beforeSource", step["source"]))
        for flag, value in expected["flags"].items():
            check(
                3,
                f"flag.{flag}",
                value,
                int(flag) in state["flags"] if state.get("flags") is not None else None,
                sample,
                step.get("beforeSource", step["source"]),
                "Actual flags field absent/null" if state.get("flags") is None else "",
            )

    first = labels.get("first-control")
    if first:
        state = first["state"]
        turn = ref["turns"][0]
        for name, expected, value in (
            ("first-actor", turn["actor"], source_actor(state.get("actor"))),
            ("round", turn["round"], state.get("round")),
            (
                "turn-order",
                turn["accounting"]["turnOrder"],
                [
                    dict(actor=source_actor(x["actor"]), score=x["score"])
                    for x in state["turnOrder"]
                    if x["actor"] is not None
                ],
            ),
        ):
            check(4, name, expected, value, first, turn["source"])
        check(
            4,
            "mainSeed readback",
            int.from_bytes(bytes(turn["accounting"]["rngBytes"]), "big"),
            state["mainSeed"],
            first,
            turn["source"],
            "Different readback retained; timing-to-RNG mapping is Unknown. "
            "This does not diagnose the RNG algorithm",
            True,
        )
        for ally in turn["accounting"]["allies"][:3]:
            actor = next(x for x in state["actors"] if x["id"] == f"ally-{ally['id']}")
            for ekey, akey in (
                ("hpCurrent", "hp"),
                ("hpMax", "maxHp"),
                ("mpCurrent", "mp"),
                ("mpMax", "maxMp"),
                ("level", "level"),
                ("attack", "attack"),
                ("defense", "defense"),
                ("move", "move"),
                ("statusEffects", "status"),
                ("x", "x"),
                ("y", "y"),
            ):
                check(
                    4, f"{actor['id']}.{ekey}", ally[ekey], actor.get(akey), first, turn["source"]
                )
            for key in ("items", "spells"):
                check(
                    4,
                    f"{actor['id']}.{key}",
                    [v["raw"] for v in ally[key]],
                    actor.get(key),
                    first,
                    turn["source"],
                    "Read live natural-entry actor; no retrospective admission claim",
                )

    texts = [s for s in samples if s["label"] == "dialogue"]
    expected_texts = plan["texts"]
    for i, expected in enumerate(expected_texts):
        sample = texts[i] if i < len(texts) else samples[-1]
        check(
            3,
            f"dialogue.{i}.textId",
            expected["textId"],
            texts[i]["state"].get("textId") if i < len(texts) else None,
            sample,
            expected["source"],
            "Ordered reached text identity; not original reveal/ack equality",
            i >= len(texts),
        )
    check(
        3,
        "dialogue-count",
        len(expected_texts),
        len(texts),
        samples[-1],
        {"records": "DisplayText:entry before first dispatch"},
        "No extra or missing reached dialogue identities in the executed route",
        first is None,
    )

    for label, name, expected, keys, original_record in (
        (
            "first-destination",
            "first-decision.destination",
            plan["firstDecision"]["destination"],
            ("previewX", "previewY"),
            plan["firstDecision"]["source"],
        ),
    ):
        if label in labels:
            sample = labels[label]
            check(
                5,
                name,
                expected,
                dict(zip(("x", "y"), (sample["state"].get(k) for k in keys), strict=True)),
                sample,
                original_record,
                "Diagnostic continuation after divergent initial order",
                scope="diagnostic",
            )
    if "after-first-decision" in labels:
        sample = labels["after-first-decision"]
        check(
            5,
            "next-decision.actor",
            plan["nextDecision"]["actor"],
            source_actor(sample["state"].get("actor")),
            sample,
            plan["nextDecision"]["source"],
            "Stop on divergence; no actor chasing, reseed or substituted decision",
            scope="diagnostic",
        )
    last = samples[-1]
    check(
        2,
        "single-session",
        [a.get("sessionId")],
        sorted({s["state"].get("sessionId") for s in samples}, key=str),
        last,
        original["source"],
        "Observed session identity across the executed prefix only",
    )
    check(
        2,
        "monotonic-actual-records",
        True,
        record_sequences == list(range(1, len(record_sequences) + 1)),
        last,
        original["source"],
    )
    observed = {}
    signal_problem = None
    prior_revision = -1
    prior_sequence = 0
    for entry in signals:
        result = entry["result"]
        if result["sessionId"] != a["sessionId"] or result["revision"] < prior_revision:
            signal_problem = "session/revision discontinuity"
        prior_revision = result["revision"]
        for observation in result["observations"]:
            number = observation["Sequence"]
            if number in observed:
                if result["boundary"] != "attach" or observed[number] != observation:
                    signal_problem = "duplicate or conflicting non-attach observation"
            elif number != prior_sequence + 1:
                signal_problem = "observation sequence gap"
            else:
                prior_sequence = number
                observed[number] = observation
        if result["observationSequence"] != prior_sequence:
            signal_problem = "result watermark gap"
        if result["failure"] is not None:
            check(
                2,
                "session-result-error",
                None,
                result["failure"],
                {**entry, "label": "signal:" + result["boundary"]},
                {},
                "Actual submitted failure retained",
            )
    check(
        2,
        "every-session-result",
        None,
        signal_problem,
        last,
        original["source"],
        "Subscribed before Begin/Attach; contiguous watermarks; duplicate attaches checked"
        if signals
        else "Latest-result snapshots cannot prove absent overwritten submissions/errors",
        not signals,
    )
    if "after-first-decision" in labels:
        actor = f"ally-{plan['firstDecision']['actor']}"
        kinds = {
            o["Kind"] for o in observed.values() if (o.get("Actor") or {}).get("Value") == actor
        }
        check(
            5,
            "first-STAY-consumed",
            ["action-committed", "after-turn", "stay-selected"],
            sorted(kinds & {"action-committed", "after-turn", "stay-selected"}),
            labels["after-first-decision"],
            plan["firstDecision"]["source"],
            "Actual selection/commit/after-turn in diagnostic extension",
            not signals,
            "diagnostic",
        )
    for sample in samples:
        if sample["state"].get("failure"):
            check(
                2,
                "host-error",
                None,
                sample["state"]["failure"],
                sample,
                {},
                "Actual runtime failure",
            )
    errors = [
        line
        for line in host_log.read_text(encoding="utf-8-sig").splitlines()
        if "ERROR:" in line or "Unhandled exception" in line
    ]
    check(
        2,
        "host-process-errors",
        [],
        errors,
        last,
        {},
        "Inspect actual Godot process errors",
        scope="observation",
    )
    terminal_failure = terminal.get("failure")
    controlled_divergence = terminal_failure == "next-decision-actor-mismatch" and any(
        row["assertion"] == "next-decision.actor" and row["result"] == "FAIL" for row in assertions
    )
    check(
        2,
        "probe-terminal",
        terminal_failure if controlled_divergence else "",
        terminal_failure,
        last,
        {},
        "Controlled stop corroborated by the actual next-actor comparison"
        if controlled_divergence
        else "Explicit probe failures/timeouts are observation failures",
        scope="diagnostic" if controlled_divergence else "observation",
    )
    # A probe-reported failure intentionally exits 2. Its assertion above still fails
    # unless the controlled divergence is corroborated. Any other nonzero exit,
    # including a crash after a complete terminal record, is a process failure.
    expected_exit = 2 if terminal_failure else 0
    check(
        2,
        "host-process-exit",
        expected_exit,
        host_exit,
        last,
        {},
        "Actual completed process exit; terminal/log success cannot hide shutdown failure",
        scope="observation",
    )
    for group in ref["coverage"]:
        if "RA-12" in group["fields"]:
            extension = ref.get("postVictoryInput")
            after = extension["after"] if extension else None
            expected = None
            if after:
                state = after["state"]
                accounts = after["accounting"]
                expected = {
                    "input": extension["input"]["direction"],
                    "map": state["map"],
                    "from": extension["before"]["position"],
                    "to": [state["x"], state["y"]],
                    "facing": state["facing"],
                    "flags": state["flags"],
                    "party": accounts["party"],
                    "gold": accounts["gold"],
                    "allies": accounts["allies"],
                    "rngBytes": state["rngBytes"],
                    "rngCopyByte": state["rngCopyByte"],
                    "readiness": after["readiness"],
                }
            check(
                7,
                group["fields"],
                expected,
                None,
                last,
                {
                    "owner": (
                        "docs/research/map3-messenger-acceptance.md"
                        "#native-post-victory-ordinary-input-result-issue-515"
                    ),
                    "commit": EXTENSION_SOURCE,
                    "record": after["source"] if after else None,
                },
                "Accepted original extension projected; actual run did not reach this boundary"
                if after
                else (
                    "Accepted original extension was not supplied to this projection; "
                    "actual run did not reach this boundary"
                ),
                True,
            )
            continue
        check(
            group["layer"],
            "remaining:" + group["fields"],
            group["binding"],
            None,
            last,
            {"binding": group["binding"]},
            f"Partial group; reference status: {group['referenceStatus']}",
            True,
        )
    for deviation in (
        "1A controlled construction",
        "2A mandatory route",
        "4A manual agency/no reseeding",
        "6A absent save/restart",
        "9A keyboard/gamepad/remap/swap/flash/text variants",
        "10A out-of-domain safety",
    ):
        check(
            10,
            deviation,
            "complete named acceptance",
            None,
            last,
            {},
            "Executed prefix cannot certify the whole deviation or unexecuted variants",
            True,
        )
    counts = Counter(x["result"] for x in assertions)
    failures = [x for x in assertions if x["result"] == "FAIL"]
    return dict(
        sourceCommit=SOURCE,
        scope=plan["scope"],
        assertions=assertions,
        counts=dict(counts),
        earliestDivergence=min(failures, key=lambda x: x["actualSequence"]) if failures else None,
        result="FAIL" if failures else "Unavailable",
        milestonePass=False,
        stop=terminal,
        hostExit=host_exit,
        remainder="Battle/return/endpoint and 9A variants not established; "
        "diagnostic extension stops at actor divergence",
    )


def semantic_value(value):
    """Compare common gameplay; raw predicate-time release evidence stays separate.

    EntityWaitRelease was added by PR600 to an existing observation. Older
    captures lack it; its absence is unavailable consumer evidence, not drift
    in the unchanged gameplay observation. Keep every other logical operand.
    """
    if isinstance(value, dict):
        return {
            k: v for k, v in value.items() if k not in ("Revision", "Sequence", "EntityWaitRelease")
        }
    return value


def endpoint_state(state):
    player = next((e for e in state.get("entities", []) if e["id"] == "entity-0"), {})
    camera = state.get("cameraProjection") or {}
    return {
        "player": {k: player.get(k) for k in ("x", "y", "facing", "moving", "busy")},
        "camera": {k: camera.get(k) for k in ("x", "y", "targetSlot", "bound")},
        **{
            k: state.get(k)
            for k in (
                "map",
                "canWaitAtInput",
                "wait",
                "cursor",
                "callers",
                "callerReturning",
                "warp",
                "battleMounted",
                "choice",
                "tickDebt",
            )
        },
    }


def paired_baseline_value(expected, actual):
    """An old paired entity cannot prove the subsequently exposed idle operand.

    Only this additive field is projected when absent historically. Identity,
    order, all older operands and idle values present on both sides stay exact.
    Raw actual states continue to own idle/release consumer evidence.
    """
    old, new = expected.get("entities"), actual.get("entities")
    if not isinstance(old, list) or not isinstance(new, list) or len(old) != len(new):
        return actual
    if not all(
        e.get("id") is not None and e["id"] == a.get("id") for e, a in zip(old, new, strict=True)
    ):
        return actual
    return dict(
        actual,
        entities=[
            {k: v for k, v in a.items() if k != "isScriptIdle" or "isScriptIdle" in e}
            for e, a in zip(old, new, strict=True)
        ],
    )


# This accepted candidate predates field-death additions. Fingerprint-v1 hashes
# checkout bytes, so the recorded Windows CRLF identity differs from Git LF blobs.
SCENE_SOURCE_SHA256 = "A2C7E64C23A858DC7E8EDC890C0279DB4B04F285A2940CBA14C9571055D30DCB"
SCENE_MANIFEST_SHA256 = "166425DC10924FCF47A761CC6D0FC92E315F1C7FAC404DD092A25CD4A559FCE8"
SCENE_GENERATOR_COMMIT = "bfcb819fe61cf6b7f2a3a45822f6de51e411e559"
SCENE_GENERATOR_COMPONENTS = (
    "src/sf2tool/remake_battle_scene_content.py",
    "src/sf2tool/remake_asset_build.py",
    "src/sf2tool/texture_extract.py",
    "src/sf2tool/compression.py",
)


def text_material_binding(actual, outcome, selection, source_root):
    """Full reached text material/modern Label join, independent of rendered selectors."""
    result = dict(
        value=None,
        checks=_bounded_list(),
        field=_bounded_list(),
        battle=_bounded_list(),
        boundary="modern configured font",
    )
    if not selection or source_root is None:
        return result

    def check(name, value):
        result["checks"].append(dict(name=name, value=value))

    def font(value, size):
        if not value or not value.get("faces"):
            return None
        return (
            value.get("size") == size
            and value.get("resourceClass") == "FontFile"
            and all(
                f.get("family") == "Open Sans SemiBold"
                and f.get("style") == "SemiBold"
                and f.get("faceIndex") == 0
                and f.get("allowSystemFallback") is True
                for f in value["faces"]
            )
        )

    w, texts, names, enemy_names = {}, {}, [], []
    ascii_map = advances = None
    try:
        world_path, scene_path, process_path = selection[:3]
        world_path, scene_path, process_path = (
            p.resolve() if p.is_absolute() else repo_path(p)
            for p in (world_path, scene_path, process_path)
        )
        world, scene, process = read(world_path), read(scene_path), read(process_path)
        w = world["world"]
        check(
            "world original identity",
            world["provenance"]["commit"] == UPSTREAM and world["provenance"]["romSha256"] == ROM,
        )
        selected = process.get("selectedInputs", {})
        check(
            "same-run material selection",
            all(
                repo_path(selected[k]).resolve() == p.resolve()
                for k, p in (
                    ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
                    ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
                )
            ),
        )
        source_root = source_root.resolve() if source_root.is_absolute() else repo_path(source_root)
        pin = subprocess.check_output(
            ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
        ).strip()
        check("original source pin", pin == UPSTREAM)

        def source(path):
            return subprocess.check_output(
                ["git", "-C", str(source_root), "show", f"{UPSTREAM}:disasm/{path}"]
            )

        from sf2tool.h2.variable_width_font import _glyph_metadata, _parse_ascii_map

        texts = {
            int(line[:4], 16): line[5:]
            for line in source("data/scripting/text/gamescript.txt").decode("utf-8").splitlines()
            if re.match(r"^[0-9A-Fa-f]{4}=.+", line)
        }
        check("full source text import", {t["id"]: t["text"] for t in w["texts"]} == texts)
        ascii_map = _parse_ascii_map(
            source("data/scripting/text/asciitotextsymbolmap.asm").decode("utf-8")
        )
        # Extracted private font bytes are ignored upstream. Validate against the
        # accepted source/ROM parity fixture before deriving their advances.
        try:
            font_fixture = read(repo_path("tests/fixtures/h2/variable-width-font-static-v1.json"))
            font_bytes = (
                source_root / "disasm/data/graphics/tech/fonts/variablewidthfont.bin"
            ).read_bytes()
            check(
                "original font bytes",
                font_fixture["upstreamCommit"] == UPSTREAM
                and font_fixture["romSha256"] == ROM
                and hashlib.sha256(font_bytes).hexdigest().upper()
                == font_fixture["fontHashes"]["fontSha256"],
            )
            advances = [g["advancePixels"] for g in _glyph_metadata(font_bytes, 0)]
        except (KeyError, ValueError, OSError):
            check("missing original font operand", None)
        names = re.findall(r'"([^"]*)"', source("data/stats/allies/allynames.asm").decode("utf-8"))
        enemies = re.findall(
            r'"([^"]*)"', source("data/stats/enemies/enemynames.asm").decode("utf-8")
        )
        check(
            "source symbol map and advances",
            None
            if ascii_map is None or advances is None
            else w["textFont"]["asciiToSymbol"] == ascii_map
            and w["textFont"]["advances"] == advances,
        )
        check("member names", w["memberNames"] == scene["memberNames"] == names)
        check(
            "scene text import",
            all(text == texts[int(tid)] for tid, text in scene["texts"].items()),
        )
        check("admitted enemy name", "GIZMO" in enemies)
        battle = read(repo_path(selected["SF2_PRIVATE_BATTLE01_DATA"]))
        battle_source = source("data/battles/spritesets/spriteset01.asm")
        enemy_names = re.findall(
            r"^\s*enemyCombatant\s+(\w+),", battle_source.decode("utf-8"), re.MULTILINE
        )
        selected_enemies = [e for e in battle["entities"] if e["kind"] == "enemy"]
        check(
            "original Battle01 enemy selectors",
            battle["provenance"]["commit"] == UPSTREAM
            and battle["provenance"]["sourcePath"] == "data/battles/spritesets/spriteset01.asm"
            # This admitted extractor recorded its Windows CRLF checkout bytes;
            # reproduce that representation from the pinned Git LF object.
            and battle["provenance"]["sourceSha256"]
            == hashlib.sha256(battle_source.replace(b"\n", b"\r\n")).hexdigest().upper()
            and [e["identityExpression"] for e in selected_enemies] == enemy_names
            and all(n == "GIZMO" for n in enemy_names),
        )
    except (KeyError, IndexError, ValueError, OSError, subprocess.CalledProcessError):
        check("missing source/material operand", None)
    session = actual["samples"][0]["state"]["sessionId"]
    records = actual.get("warpRecords", [])
    events, seen = _bounded_list(), _occurrence_set()
    for ri, r in enumerate(records):
        for e in r["result"].get("observations", []):
            if e["Sequence"] not in seen:
                events.append(dict(record=ri, **e))
                seen.add(e["Sequence"])
    events.sort(key=lambda e: e["Sequence"])
    event_by_sequence = _occurrence_dict((e["Sequence"], e) for e in events)

    def units(text, leader):
        out = []
        for part in re.split(r"(\{[^}]+\})", text):
            if part == "{N}":
                out.append(dict(Kind=1, Text="\n", Symbol=0, Advance=0))
                continue
            if part in ("{W1}", "{W2}", "{D1}"):
                out.append(
                    dict(
                        Kind={"{W1}": 3, "{W2}": 4, "{D1}": 6}[part],
                        Text="",
                        Symbol=0,
                        Advance=0,
                    )
                )
                continue
            if part.startswith("{NAME;"):
                part = names[int(part[6:-1])]
            elif part == "{LEADER}":
                part = names[int(leader)]
            elif part.startswith("{"):
                raise ValueError("unbound source control")
            for c in part:
                symbol = ascii_map[ord(c)]
                out.append(dict(Kind=0, Text=c, Symbol=symbol, Advance=advances[symbol - 1]))
        return out

    programs = {p["id"]: p for p in w.get("programs", [])}
    text_cursor, producer_text = None, None
    required_field, controls = _occurrence_map(), []
    active_producer, span = None, 0
    for e in events:
        try:
            loc = e.get("Program")
            if e["Kind"] == "program-instruction" and loc is not None:
                ins = programs[loc["Program"]]["instructions"][int(loc["Instruction"])]
                if ins.get("op") == "text-cursor":
                    text_cursor = ins["text"]
                elif ins.get("op") == "show-text":
                    active_producer, span = e["Sequence"], 0
                    producer_text = text_cursor
                    # Source controls, not consumer projections, define the spans.
                    text = texts[text_cursor]
                    controls = list(re.finditer(r"\{W[12]\}", text))
                    required_field[active_producer] = dict(
                        producer=active_producer, text=text_cursor, span=0
                    )
                    if text_cursor is not None:
                        text_cursor += 1
            elif (
                e["Kind"] in ("text-w1-accepted", "text-w2-accepted")
                and active_producer is not None
            ):
                if span >= len(controls):
                    check(f"source text acceptance {e['Sequence']}", False)
                    continue
                control = controls[span]
                check(
                    f"source text acceptance {e['Sequence']}",
                    e["Kind"] == "text-w" + control.group()[2] + "-accepted",
                )
                if control.end() < len(texts[producer_text]):
                    span += 1
                    required_field[e["Sequence"]] = dict(
                        producer=active_producer, text=producer_text, span=span
                    )
                else:
                    active_producer = None
        except (KeyError, IndexError, ValueError):
            check(f"missing source text producer {e['Sequence']}", None)
            active_producer = None
    field_rows = itertools.chain(
        actual["samples"],
        (r for r in outcome.get("records", []) if r.get("label") == "outcome-text-input"),
    )
    field_tokens = _occurrence_set()
    for i, row in enumerate(field_rows):
        s = row["state"]
        f = s.get("fieldText")
        if not f:
            continue
        label = s.get("fieldLabel")
        check(f"field font {i}", font(label.get("font") if label else None, 16))
        try:
            token = f["Token"]["Value"]
            required = required_field.get(token)
            if required is None or ascii_map is None or advances is None:
                check(f"missing field lineage/units {i}", None)
                continue
            tid, producer_sequence = required["text"], required["producer"]
            check(f"field producer {i}", None if f.get("Text") is None else f["Text"] == tid)
            active = s["partyLists"]["Active"]
            expected = units(texts[tid], active[0] if active else 0)
            check(f"field units {i}", None if f.get("Units") is None else expected == f["Units"])
            ends = [j for j, u in enumerate(expected) if u["Kind"] in (3, 4)]
            ends.append(len(expected))
            end = ends[required["span"]]
            check(f"field span {i}", None if f.get("End") is None else f["End"] == end)
            projection = "".join(u["Text"] for u in expected[:end])
            check(
                f"field Label {i}",
                None
                if label is None or f.get("Projection") is None
                else s["sessionId"] == session
                and projection == f["Projection"] == s["dialogue"] == label["text"],
            )
            if (
                label
                and label["visible"]
                and (
                    label["visibleCharacters"] < 0
                    or label["visibleCharacters"] >= label["totalCharacters"]
                )
            ):
                field_tokens.add(token)
            result["field"].append(
                dict(
                    sample=i,
                    token=token,
                    text=tid,
                    revision=s["revision"],
                    producerSequence=producer_sequence,
                    span=required["span"],
                    program=event_by_sequence[producer_sequence].get("Program"),
                )
            )
        except (KeyError, IndexError, ValueError):
            check(f"missing field occurrence operand {i}", None)
    check(
        "every logical/source field span mounted",
        True if required_field and all(token in field_tokens for token in required_field) else None,
    )
    result["requiredField"] = _bounded_list(
        dict(token=token, **binding) for token, binding in required_field.items()
    )
    preps = _bounded_list(e for e in events if e["Kind"] == "scene-prepared")
    prepseq = _bounded_list(e["Sequence"] for e in preps)
    event_sequences = _bounded_list(e["Sequence"] for e in events)
    messages = _occurrence_map()
    for i, row in enumerate(actual.get("sceneObservations", [])):
        if row["scene"].get("message"):
            messages.setdefault(row["scene"]["waitToken"], []).append((i, row))

    def actor(e, key="Actor"):
        return (e.get(key) or {}).get("Value")

    def name(who):
        if who.startswith("ally-"):
            return names[int(who.split("-")[1])]
        if who.startswith("enemy-"):
            return enemy_names[int(who.split("-")[1])]
        raise ValueError("unsupported actor")

    for token, rows_for_token in messages.items():
        step = event_by_sequence.get(token)
        phase = step.get("Detail") if step and step["Kind"] == "scene-step-started" else None
        for i, row in rows_for_token:
            check(f"battle font {i}", font(row["scene"].get("messageFont"), 9))
            check(
                f"battle phase {i}", None if phase is None else row["scene"].get("phase") == phase
            )
        try:
            si = rows_for_token[0][1]["scene"]
            if si.get("reactionAmount") is None:
                si = next(
                    (
                        row["scene"]
                        for _, row in rows_for_token
                        if row["scene"].get("reactionAmount") is not None
                    ),
                    si,
                )
            gi = bisect_right(prepseq, token) - 1
            if gi < 0:
                check(f"battle prepare {token}", None)
                continue
            p = preps[gi]
            end = prepseq[gi + 1] if gi + 1 < len(preps) else float("inf")
            body = events[
                bisect_right(event_sequences, p["Sequence"] - 1) : bisect_right(
                    event_sequences, end - 1
                )
            ]
            if gi + 1 < len(preps):
                body = [
                    e
                    for e in body
                    if not (
                        e["record"] == preps[gi + 1]["record"]
                        and e["Kind"]
                        in (
                            "gold",
                            "rng-dodge",
                            "rng-critical",
                            "rng-spread-1",
                            "rng-spread-2",
                            "rng-double",
                            "rng-counter",
                        )
                    )
                ]
            starts = [
                e["Sequence"]
                for e in body
                if e["Kind"] == "scene-step-started" and e["Detail"] == "ActionMessage"
            ]
            rx = bisect_right(starts, token) - 1
            rxend = starts[rx + 1] if rx + 1 < len(starts) else float("inf")
            reaction = [e for e in body if rx >= 0 and starts[rx] <= e["Sequence"] < rxend]
            actions = [
                e
                for e in reaction
                if e["Kind"]
                in ("physical-first", "physical-second", "physical-counter", "heal", "item-use")
            ]
            pair = actions[0] if len(actions) == 1 else None
            hp = [
                e
                for e in reaction
                if e["Kind"] == "hp" and pair and actor(e) == actor(pair, "Target")
            ]
            critical = any(e["Kind"] == "critical" for e in reaction)
            step = event_by_sequence.get(token)
            phase = step.get("Detail") if step and step["Kind"] == "scene-step-started" else None
            tid = value = who = None
            if phase in (
                "ActionMessage",
                "ResultMessage",
                "DeathMessage",
                "SpellCost",
                "MakeIdle",
                "SpellStop",
            ):
                check(
                    f"battle typed action {token}",
                    None if pair is None else si["actionKind"] == pair["Kind"],
                )
            healing = bool(si.get("healing"))
            if pair and phase in ("ActionMessage", "SpellCost"):
                who = actor(pair)
                value = int(si["spell"]["Level"]) if healing else 0
                tid = (
                    274
                    if healing
                    else {
                        "physical-first": 273,
                        "physical-second": 293,
                        "physical-counter": 292,
                    }.get(pair["Kind"])
                )
            elif pair and phase in ("ResultMessage", "MakeIdle", "SpellStop"):
                who = actor(pair, "Target")
                tid = (
                    298
                    if healing
                    else 286
                    if si["reactionKind"] == "Dodge"
                    else (287 if actor(pair).startswith("ally-") else 288)
                    if critical
                    else (284 if actor(pair).startswith("ally-") else 285)
                )
                value = 0 if si["reactionKind"] == "Dodge" else si.get("reactionAmount")
                if value is not None and si["reactionKind"] != "Dodge":
                    check(
                        f"reaction clipping {token}",
                        None
                        if len(hp) != 1
                        else hp[0]["After"] - hp[0]["Before"] == value
                        if healing
                        else hp[0]["After"] == max(0, hp[0]["Before"] - value),
                    )
            elif pair and phase == "DeathMessage":
                who = actor(pair, "Target")
                tid = 291 if who.startswith("ally-") else 290
                value = 0
            elif phase in ("RewardMessage", "GrowthMessage"):
                exp = [e for e in body if e["Kind"] == "exp"]
                if len(exp) == 1:
                    who = actor(exp[0])
                    if phase == "RewardMessage":
                        tid = 263
                        value = (
                            exp[0]["After"] - exp[0]["Before"] if exp[0]["After"] < 200 else None
                        )
                    else:
                        notices = []
                        for kind, template in (
                            ("level", 244),
                            ("level-max-hp", 266),
                            ("level-max-mp", 267),
                            ("level-base-attack", 268),
                            ("level-defense", 269),
                            ("level-agility", 270),
                        ):
                            for e in body:
                                if e["Kind"] == kind and (
                                    kind == "level" or e["After"] > e["Before"]
                                ):
                                    notices.append(
                                        (
                                            template,
                                            e["After"]
                                            if kind == "level"
                                            else e["After"] - e["Before"],
                                        )
                                    )
                        growthstarts = [
                            e["Sequence"]
                            for e in body
                            if e["Kind"] == "scene-step-started" and e["Detail"] == "GrowthMessage"
                        ]
                        ni = bisect_right(growthstarts, token) - 1
                        if 0 <= ni < len(notices):
                            tid, value = notices[ni]
            elif phase == "GoldMessage":
                gold = _bounded_list(
                    e
                    for e in events
                    if e["record"] == p["record"]
                    and e["Sequence"] <= p["Sequence"]
                    and e["Kind"] == "gold"
                    and actor(e) == actor(p)
                )
                if gold:
                    tid = 393
                    who = actor(p)
                    value = sum(e["After"] - e["Before"] for e in gold)
            expected = None
            if tid is not None and value is not None and who is not None:
                expected = (
                    texts[tid]
                    .replace("{NAME}", name(who))
                    .replace("{#}", str(int(value)))
                    .replace("{N}", "\n")
                )
                if healing:
                    expected = expected.replace("{SPELL}", si["spell"]["Value"].upper())
                expected = re.sub(r"\{D[0-9]+\}", "", expected)
            for i, row in rows_for_token:
                check(
                    f"battle text {i}",
                    None
                    if expected is None
                    else row["sessionId"] == session and row["scene"]["message"] == expected,
                )
                check(
                    f"battle operand {i}",
                    None
                    if expected is None or row["scene"].get("reactionAmount") is None
                    else row["scene"].get("reactionAmount") == si.get("reactionAmount"),
                )
            mounted = _bounded_list(
                r["state"]["scene"]
                for r in records
                if r.get("state", {}).get("scene", {}).get("waitToken") == token
            )
            check(
                f"battle mounted {token}",
                True
                if any(
                    s.get("messageFont", {}).get("visible")
                    and (
                        s.get("visibleCharacters", 0) < 0
                        or s.get("visibleCharacters", 0) >= len(s.get("message", ""))
                    )
                    for s in mounted
                )
                else None,
            )
            result["battle"].append(
                dict(
                    token=token,
                    phase=phase,
                    sourceTemplate=tid,
                    prepare=p["Sequence"],
                    action=pair["Sequence"] if pair else None,
                    reactionAmount=si.get("reactionAmount"),
                    projections=len(rows_for_token),
                )
            )
        except (KeyError, IndexError, ValueError):
            check(f"missing battle occurrence operand {token}", None)
    message_phases = {
        "ActionMessage",
        "ResultMessage",
        "DeathMessage",
        "SpellCost",
        "MakeIdle",
        "SpellStop",
        "RewardMessage",
        "GoldMessage",
        "GrowthMessage",
    }
    required_battle = _occurrence_set(
        e["Sequence"]
        for e in events
        if e["Kind"] == "scene-step-started" and e.get("Detail") in message_phases
    )
    check(
        "every logical battle message paired",
        True if required_battle and all(token in messages for token in required_battle) else None,
    )
    result["requiredBattle"] = _bounded_sorted(required_battle)
    values = _bounded_list(c["value"] for c in result["checks"])
    result["value"] = False if False in values else None if None in values or not values else True
    return result


def _resource_source_events(kind, want, sprites, source_sprites, portraits, source_portraits):
    events = []
    error = None
    try:
        if kind == "entity":
            events.append(
                (
                    "reached sprite original pointer/palette/decode",
                    sprites.get(want["sprite"]) == source_sprites.get(want["sprite"])
                    and want["sprite"] in source_sprites,
                )
            )
        elif kind != "map":
            events.append(
                (
                    "reached portrait original decode/tile composition",
                    portraits.get(want["portrait"]) == source_portraits.get(want["portrait"])
                    and want["portrait"] in source_portraits,
                )
            )
            original = source_portraits.get(want["portrait"])
            # Retain the legacy container's exception boundary as well as its operands.
            tiles = _bounded_list(range(64))
            if original:
                for changes in (
                    original["eyes"] if want["eyes"] else [],
                    original["mouth"] if want["mouth"] else [],
                ):
                    for x, y, alternate_x, alternate_y in changes:
                        tiles[y * 8 + x] = alternate_y * 8 + alternate_x
            events.append(
                (
                    "portrait source alternate tile selection",
                    None if original is None else want["tiles"] == tiles,
                )
            )
    except (KeyError, IndexError, ValueError, TypeError) as caught:
        error = type(caught).__name__
    return events, error


def _resource_pair_events(required, used, source_recipe):
    events = []
    error = None
    valid = None
    try:
        kind, want, bound = required["kind"], required["expected"], used["used"]
        if kind == "map":
            bound = bound.get("selector")
            valid = (
                None
                if bound is None
                else bound
                == dict(
                    kind="map-block",
                    map=required["identity"]["map"],
                    block=want["block"],
                )
                and used["used"].get("word") == want["word"]
            )
        elif kind == "entity":
            valid = None if bound is None else bound == dict(kind="entity", **want)
        else:
            valid = (
                None
                if bound is None
                else bound.get("texturePresent")
                and (bound.get("selector") == dict(kind="portrait", **want))
            )
        source_events, error = source_recipe(kind, want) if kind != "map" else ([], None)
        events.extend(source_events)
        if error is None:
            events.append(("bound texture selector matches logical source requirement", valid))
    except (KeyError, IndexError, ValueError, TypeError) as caught:
        error = type(caught).__name__
    # AttributeError was outside both old exception boundaries and still propagates.
    return events, valid, error


def _reduce_resource_requirement(required, relation, source_recipe):
    key = _resource_key(required, required=True)
    stop, failure = None, None
    for _, _, first, used in relation.variants(key):
        events, _, error = _resource_pair_events(required, used, source_recipe)
        if error is not None:
            stop, failure = first, (events, error, used["_captureLocator"])
            break
    counts, checks, executed = Counter(), [], 0
    for signature, count, first, used in relation.variants(key):
        if stop is not None and first >= stop:
            break
        if stop is not None:
            count = relation.prefix_count(key, signature, stop)
        if not count:
            continue
        events, value, error = _resource_pair_events(required, used, source_recipe)
        require(error is None, "resource exception prefix changed")
        for name, outcome in events:
            checks.append((name, outcome, count, used["_captureLocator"]))
        counts[False if value == False else None if value is None else True] += count  # noqa: E712
        executed += count
    if failure is not None:
        events, error, witness = failure
        for name, value in events:
            checks.append((name, value, 1, witness))
        checks.append(
            (
                "required texture join operand absent"
                if error == "KeyError"
                else "required texture join malformed " + error,
                None if error == "KeyError" else False,
                1,
                witness,
            )
        )
    total = sum(count for _, count, _, _ in relation.variants(key))
    return dict(
        candidatePairCount=total,
        executedPairCount=executed,
        counts=dict(PASS=counts[True], FAIL=counts[False], Unavailable=counts[None]),
        legacyStop=None
        if failure is None
        else dict(firstOrdinal=stop, error=failure[1], locator=failure[2]),
        checks=checks,
    )


def reached_visual_materials(
    actual,
    selection,
    source_root,
    canonical_content=None,
    tileset_metadata=None,
    palette_metadata=None,
    *,
    source_only=False,
    budget=None,
    map_binding=None,
):
    """Join reached texture selectors to existing source decoders and private exports."""
    result = dict(
        map=None,
        entity=None,
        scene=None,
        format="sf2-resource-binding-counts-v1",
        checks=[],
        joins=_bounded_list(),
        witnesses=_bounded_list(),
    )
    if map_binding is not None:
        result["mutableMapDelivery"] = map_binding
        result["historicalMutableLayout"] = dict(
            value=None,
            reason="Historical per-Submit working layouts and coordinate draws were not recorded",
        )
    scope = actual.get("resourceScope")
    enabled = (
        {"map", "entity", "scene"} if not scope or scope["family"] == "all" else {scope["family"]}
    )
    counted = Counter()
    witness_counts = Counter()
    weight, locator = 1, None

    def check(family, name, value, identity=None):
        if family not in enabled:
            return
        value = False if value == False else None if value is None else True  # noqa: E712
        counted[family, name, value] += weight
        if value is not True and witness_counts[family, name, value] < 8:
            row = dict(family=family, name=name, value=value, count=weight)
            if identity is not None:
                row["identity"] = identity
            if locator is not None:
                row["locator"] = locator
            result["witnesses"].append(row)
            witness_counts[family, name, value] += 1

    if map_binding is not None:
        check("map", "complete mutable-map composed boundary", map_binding["value"])

    from contextlib import contextmanager

    @contextmanager
    def evaluated(family, name):
        try:
            yield
        except KeyError:
            for dependent in family if isinstance(family, tuple) else (family,):
                check(dependent, name + " operand absent", None)
        except (IndexError, ValueError, TypeError) as error:
            for dependent in family if isinstance(family, tuple) else (family,):
                check(dependent, name + " malformed " + type(error).__name__, False)

    def finish():
        result["checks"] = [
            dict(family=f, name=n, value=v, count=c) for (f, n, v), c in counted.items()
        ]
        result["witnessPolicy"] = dict(
            limitPerCheckOutcome=8,
            retained=len(result["witnesses"]),
            candidateVariants="all distinct operands retained",
        )
        result["familyCounts"] = {}
        for family in ("map", "entity", "scene"):
            values = Counter()
            for (f, _, value), count in counted.items():
                if f == family:
                    values[value] += count
            result["familyCounts"][family] = dict(
                PASS=values[True], FAIL=values[False], Unavailable=values[None]
            )
            result[family] = (
                False if values[False] else None if values[None] or not values else True
            )
        return result

    def scene_uses(scene):
        scene_rows = actual.get("sceneObservations", [])
        check("scene", "reached scene observation channel", True if scene_rows else None)
        for row in scene_rows:
            with evaluated("scene", "scene occurrence"):
                scene_state = row["scene"]
                fairy = (scene_state.get("healing") or {}).get("Fairy")
                if scene_state.get("visible") and fairy and fairy.get("Control"):
                    needed = {}
                    for i, instance in enumerate(fairy["Fairies"]):
                        with evaluated("scene", "fairy instance"):
                            if instance["Active"]:
                                needed["FairyBody" + str(i)] = scene["healing"]["bodies"][
                                    int(instance["BodyFrame"])
                                ]
                                needed["FairyWings" + str(i)] = scene["healing"]["wings"][
                                    int(instance["WingFrame"])
                                ]
                    for i, dust in enumerate(fairy["Dust"]):
                        with evaluated("scene", "fairy dust"):
                            if dust["Age"]:
                                needed["FairyDust" + str(i)] = scene["healing"]["dust"][
                                    int(dust["Frame"])
                                ]
                    mounted = {s["name"]: s for s in scene_state.get("fairySprites", [])}
                    for name, resource in needed.items():
                        node = mounted.get(name, {}).get("binding")
                        check(
                            "scene",
                            "required fairy mounted texture " + name,
                            None
                            if node is None
                            else node.get("resource") == resource
                            and node.get("texturePresent")
                            and node.get("visible"),
                        )
                if scene_state.get("fieldDeath"):
                    actors = row.get("fieldActors")
                    check(
                        "scene",
                        "actual field-death consumer channel",
                        True if actors is not None else None,
                    )
                    if scene_state["phase"] in ("FieldSpin", "FieldExit"):
                        mounted = {a["id"]: a.get("sprite") for a in actors or []}
                        for dead in scene_state["fieldDeath"]["actors"]:
                            node = mounted.get(dead)
                            check(
                                "scene",
                                "required dead actor remains projected during its source effect",
                                None
                                if node is None
                                else node.get("visible") and node.get("texturePresent"),
                            )
                    for actor in actors or []:
                        with evaluated("scene", "field actor"):
                            sprite = actor.get("sprite")
                            if not sprite or not sprite.get("visible"):
                                continue
                            selector = sprite.get("resourceSelector")
                            facing = sprite["facing"]
                            direction = 0 if facing == 1 else 2 if facing == 3 else 1
                            ally = next(
                                (
                                    a["sprite"]
                                    for a in scene["fieldDeath"]["allies"]
                                    if actor["id"] == "ally-" + str(a["character"])
                                ),
                                None,
                            )
                            original_sprite = (
                                ally
                                if ally is not None
                                else scene["fieldDeath"]["enemies"][0]["sprite"]
                            )
                            expected_sprite = (
                                63
                                if actor["id"] in scene_state["fieldDeath"]["actors"]
                                and scene_state["phase"] == "FieldExit"
                                else original_sprite
                            )
                            check(
                                "scene",
                                "actual field-death assigned texture",
                                None
                                if selector is None
                                else selector
                                == dict(
                                    sprite=expected_sprite,
                                    direction=direction,
                                    frame=sprite["walkingFrame"],
                                    raster=scene["fieldDeath"]["exitFrames"][direction]
                                    if expected_sprite == 63
                                    else None,
                                )
                                and sprite.get("texturePresent")
                                and sprite.get("visibleInTree"),
                            )

    if not selection:
        if map_binding is not None:
            check("map", "independent map material selection absent", None)
            return finish()
        return result
    try:
        selected_scene = selection[1]
        selected_scene = (
            selected_scene.resolve() if selected_scene.is_absolute() else repo_path(selected_scene)
        )
        if "scene" in enabled:
            scene_uses(read(selected_scene))
    except FileNotFoundError:
        check("scene", "selected scene definition absent", None)
    except (KeyError, IndexError, ValueError, TypeError):
        check("scene", "selected scene definition malformed", False)
    if source_root is None or not all((canonical_content, tileset_metadata, palette_metadata)):
        for family in ("map", "entity", "scene"):
            check(family, "source decoding prerequisite absent", None)
        return finish()
    try:
        import io
        from types import SimpleNamespace

        from sf2tool.compression import decode_basic_compressed
        from sf2tool.h2.map_import import MANIFEST, _canonical_bytes
        from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
        from sf2tool.remake_asset_build import (
            _MAP3_ATLAS,
            _MAP19_20_ATLAS,
            _MAP21_ATLAS,
            _MAP40_ATLAS,
            _MAP57_ATLAS,
            ACCEPTED_PALETTE_METADATA_SHA256,
            ACCEPTED_TILESET_METADATA_SHA256,
            PLAYER_PALETTE_ADDRESS,
            PLAYER_POINTER_TABLE_ADDRESS,
            _build_world_atlas_source,
            _combine_player_halves,
            _render_player_frame,
            _scale_rgba_nearest,
        )
        from sf2tool.remake_exploration_content import OriginalPrograms, prepare_visuals
        from sf2tool.texture_extract import md_palette_color, write_png_rgba

        paths = [p.resolve() if p.is_absolute() else repo_path(p) for p in selection[:5]]
        world_path, scene_path, process_path, scene_root, asset_root = paths
        source_root = source_root.resolve() if source_root.is_absolute() else repo_path(source_root)
        document, scene, process = read(world_path), read(scene_path), read(process_path)
        world, presentation = document["world"], document["world"]["presentation"]
        selected_maps = set(scope["maps"]) if scope is not None and not source_only else None
        world_maps = [m for m in world["maps"] if selected_maps is None or m["id"] in selected_maps]
        binding = all(
            repo_path(process["selectedInputs"][key]).resolve() == path
            for key, path in (
                ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
                ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
            )
        )
        pin = (
            document["provenance"]["commit"] == UPSTREAM
            and document["provenance"]["romSha256"] == ROM
        )
        source_pin = (
            subprocess.check_output(
                ["git", "-C", str(source_root), "rev-parse", "HEAD"], text=True
            ).strip()
            == UPSTREAM
        )
        source_pin &= (
            subprocess.run(
                ["git", "-C", str(source_root), "diff", "--quiet", UPSTREAM, "--", "disasm"],
                check=False,
            ).returncode
            == 0
        )
        rom_path = private_input_path(ROM_INPUT_IDENTITY)
        rom = rom_path.read_bytes()
        for family in ("map", "entity", "scene"):
            check(
                family,
                "same-run source and selection pins",
                binding and pin and source_pin and hashlib.sha256(rom).hexdigest().upper() == ROM,
            )
        canonical_content = (
            canonical_content.resolve()
            if canonical_content.is_absolute()
            else repo_path(canonical_content)
        )
        canonical = read(canonical_content)
        canonical_valid = (
            hashlib.sha256(_canonical_bytes(canonical)).hexdigest().upper()
            == read(MANIFEST)["outputSha256"]
        )
        check("map", "accepted canonical source layout/blocksets", canonical_valid)
        manifest = read(asset_root / "manifests/presentation-assets-v1.json")
        assets = {a["assetId"]: a for a in manifest["assets"]}
        families = {
            m: family
            for family in (_MAP3_ATLAS, _MAP19_20_ATLAS, _MAP21_ATLAS, _MAP40_ATLAS, _MAP57_ATLAS)
            for m in family.map_indices
        }
        atlas_bindings = {
            row["id"]: families[int(row["id"].split("-")[-1])].asset_id for row in world_maps
        }
        compiler = OriginalPrograms(canonical, source_root)
        compiler.programs = {p["id"]: p for p in world["programs"]}
        expected = prepare_visuals(
            compiler, canonical, world_maps, rom_path, asset_root, selection[7], atlas_bindings
        )
        maps = {
            m["map"]: m
            for m in presentation["maps"]
            if selected_maps is None or m["map"] in selected_maps
        }
        source_maps = {m["map"]: m for m in expected["maps"]}
        sprites = {m["sprite"]: m for m in presentation["sprites"]}
        source_sprites = {m["sprite"]: m for m in expected["sprites"]}
        portraits = {m["portrait"]: m for m in presentation["portraits"]}
        source_portraits = {m["portrait"]: m for m in expected["portraits"]}
        canonical_maps = {m["id"]: m for m in canonical["maps"]}
        canonical_layouts = {m["id"]: m for m in canonical["resources"]["layouts"]}
        for row in world_maps if "map" in enabled else ():
            original = canonical_maps[int(row["id"].split("-")[-1])]
            layout = canonical_layouts[original["references"]["layout"]]
            check(
                "map",
                "selected layout original words " + row["id"],
                [word for line in row["layout"] for word in line] == layout["words"],
            )
        tileset_metadata = (
            tileset_metadata.resolve()
            if tileset_metadata.is_absolute()
            else repo_path(tileset_metadata)
        )
        palette_metadata = (
            palette_metadata.resolve()
            if palette_metadata.is_absolute()
            else repo_path(palette_metadata)
        )
        tilesets, palettes = read(tileset_metadata), read(palette_metadata)
        check(
            "map",
            "accepted private atlas metadata identities",
            hashlib.sha256(tileset_metadata.read_bytes()).hexdigest().upper()
            == ACCEPTED_TILESET_METADATA_SHA256
            and hashlib.sha256(palette_metadata.read_bytes()).hexdigest().upper()
            == ACCEPTED_PALETTE_METADATA_SHA256,
        )
        for name, visual in maps.items() if "map" in enabled else ():
            family = families[int(name.split("-")[-1])]
            decoded = _build_world_atlas_source(rom, tilesets, palettes, family)
            source_bytes = (asset_root / family.source_file).read_bytes()
            png = base64.b64decode(visual["atlas"]["data"], validate=True)
            encoded = io.BytesIO()
            # The existing deterministic writer needs only its write_bytes sink.
            write_png_rgba(
                SimpleNamespace(write_bytes=encoded.write),
                128 * visual["scale"],
                320 * visual["scale"],
                _scale_rgba_nearest(decoded.rgba_pixels, 128, 320, visual["scale"]),
            )
            source_valid = (
                source_bytes == decoded.source_bundle
                and hashlib.sha256(source_bytes).hexdigest().upper()
                == assets[family.asset_id]["source"]["sha256"]
            )
            source_valid &= png == encoded.getvalue()
            check(
                "map",
                "atlas source recipe and canonical selectors " + name,
                source_valid and visual == source_maps.get(name),
            )

        if budget is not None:
            budget.checkpoint("source loaded", force=True)
        if source_only:
            return dict(sourceOnly=True, prepared=True, sourceOnlyPrivateBytes=_private_bytes())
        requirements = actual.get("resourceRequirements", [])
        uses = actual.get("resourceUses", [])
        if not isinstance(uses, _ResourceRelation):
            relation = _ResourceRelation()
            for used in uses:
                relation.append(used)
            uses = relation
        uses.flush()
        result["actualUseCount"] = uses.count

        def available_rows(rows, requirement):
            for row in rows:
                family = "map" if row.get("kind") == "map" else "entity"
                with evaluated(family, "resource occurrence"):
                    i = row["identity"]
                    for key in ("sessionId", "visit", "map", "phase", "observationSequence"):
                        i[key]
                    row["kind"]
                    row["expected" if requirement else "used"]
                    yield row

        requirements = _bounded_list(available_rows(requirements, True))

        def counted_uses(category):
            nonlocal weight, locator
            for row, count in uses.validations(category):
                weight, locator = count, row["_captureLocator"]
                if category == "occurrence":
                    yield from available_rows((row,), False)
                else:
                    yield row
            weight, locator = 1, None

        # Availability is distinct from identity and texture validation. The latter
        # predicates read different operands; grouping them together amplifies state
        # sequence × tile cardinality even without the requirement/use Cartesian join.
        for _ in counted_uses("occurrence"):
            pass

        programs = {p["id"]: p for p in world["programs"]}
        visits = {0: read(repo_path(process["selectedStart"]))["start"]["map"]}
        for delivery in actual.get("warpRecords", []):
            for event in delivery.get("result", {}).get("observations", []):
                if event.get("Kind") == "map-transferred":
                    visits[event["Sequence"]] = event["Detail"]
                elif event.get("Detail") == "LoadSceneMap" and event.get("Program"):
                    loc = event["Program"]
                    visits[event["Sequence"]] = programs[loc["Program"]]["instructions"][
                        int(loc["Instruction"])
                    ]["map"]
        visit_sequences = _bounded_sorted(visits)
        sessions = {
            row["state"]["sessionId"]
            for channel in ("samples", "consumerBoundaries", "warpRecords")
            for row in actual.get(channel, [])
            if row.get("state", {}).get("sessionId")
        }
        if scope is not None:
            sessions.update(scope["contextSessions"])
            for family in enabled:
                check(
                    family,
                    "selected scope retains independent session context",
                    True if sessions else None,
                )
                if family in ("map", "entity"):
                    check(
                        family,
                        "selected scope retains independent current projection context",
                        True
                        if any(
                            row.get("state", {}).get("cameraProjection")
                            for channel in ("samples", "consumerBoundaries", "warpRecords")
                            for row in actual.get(channel, [])
                        )
                        else None,
                    )
        for row in itertools.chain(requirements, counted_uses("identity")):
            with evaluated("map" if row.get("kind") == "map" else "entity", "resource identity"):
                i = row["identity"]
                family = "map" if row["kind"] == "map" else "entity"
                check(
                    family,
                    "same-session resource delivery identity",
                    None if not sessions else len(sessions) == 1 and i["sessionId"] in sessions,
                )
                position = bisect_right(visit_sequences, i["observationSequence"]) - 1
                visit = visit_sequences[position] if position >= 0 else None
                check(
                    family,
                    "actual use belongs to its latest logical map visit",
                    i["visit"] == visit and i["map"] == visits.get(visit),
                )
        for family in ("map", "entity"):
            check(family, "independent reached requirement channel", True if requirements else None)
        field_maps = {
            row["state"]["map"]
            for row in actual.get("samples", [])
            if row.get("state", {}).get("map") in maps
        }
        observed_maps = {r["identity"].get("map") for r in requirements if r.get("kind") == "map"}
        check(
            "map",
            "every reached field map has delivered layer inventory",
            field_maps <= observed_maps if requirements else None,
        )

        requirement_phases = set()
        phase_groups = set()
        for required in requirements:
            i = required["identity"]
            requirement_phases.add(_join_key((i["visit"], required["kind"], i["phase"])))
            phase_groups.add(_join_key((i["visit"], required["kind"])))
        # Independently require visible logical subjects in retained current
        # projections. Surviving draw/use rows cannot define their own inventory.
        logical_states = (
            row.get("state", {})
            for channel in ("samples", "consumerBoundaries", "warpRecords")
            for row in actual.get(channel, [])
        )
        required_entities = _value_set(
            (
                r["identity"]["visit"],
                r["identity"]["phase"],
                r.get("subject"),
                r.get("slot"),
                json.dumps(r["expected"], sort_keys=True),
            )
            for r in requirements
            if r["kind"] == "entity"
        )

        def portrait_pose(visit, phase, want):
            return (
                visit,
                phase,
                want["portrait"],
                want["mirror"],
                want["eyes"],
                want["mouth"],
                tuple(want["tiles"]),
            )

        required_portraits = _value_set()
        for row in requirements:
            if row["kind"] == "portrait":
                with evaluated("entity", "portrait inventory"):
                    required_portraits.add(
                        portrait_pose(
                            row["identity"]["visit"], row["identity"]["phase"], row["expected"]
                        )
                    )
        required_tiles = _value_set()
        for row in requirements:
            if row["kind"] == "map":
                with evaluated("map", "tile inventory"):
                    i, want = row["identity"], row["expected"]
                    required_tiles.add(
                        (
                            i["visit"],
                            i["phase"],
                            row.get("layer"),
                            row.get("highPriority"),
                            row.get("subject"),
                            row.get("pass"),
                            want["block"],
                            want["tile"],
                            want["word"],
                        )
                    )
        map_definitions = {row["id"]: row for row in world["maps"]}
        logical_tiles = _value_set()

        def layer_tiles(map_id, layer, name):
            # Original draw geometry independently retains the first covered tile and
            # occlusion ink regions. Neither inventory comes from requirement/use rows.
            recorded = _bounded_list(layer.get("overlaps", []))
            if layer.get("first"):
                recorded.append(layer["first"])
            if name == "occlusion":
                if not recorded:
                    check("map", "independent occlusion tile operands", None)
                return recorded
            definition = map_definitions[map_id]
            events = definition.get("layoutEvents") or {}
            mutable_regions = _bounded_list(row["copy"] for rows in events.values() for row in rows)
            unknown_layout = False
            origin_x, origin_y = layer["x"], layer["y"]
            for y in range(int(origin_y // 24), int(origin_y // 24) + 9):
                for x in range(int(origin_x // 24), int(origin_x // 24) + 15):
                    sx, sy = int(x + layer["offsetX"]), int(y + layer["offsetY"])
                    if not (0 <= sx < 64 and 0 <= sy < 64):
                        continue
                    if any(
                        copy["destination"]["x"] <= sx < copy["destination"]["x"] + copy["width"]
                        and copy["destination"]["y"]
                        <= sy
                        < copy["destination"]["y"] + copy["height"]
                        for copy in mutable_regions
                    ):
                        unknown_layout = True
                        continue
                    block = definition["layout"][sy][sx] & 0x3FF
                    if name.startswith("foreground") and block == 0:
                        continue
                    high = layer.get("highPriority")
                    for tile in range(1 if high is None else 9):
                        word = maps[map_id]["blocks"][block][tile]
                        if high is not None and bool(word & 0x8000) != high:
                            continue
                        px = x * 24 - origin_x + (0 if high is None else tile % 3 * 8)
                        py = y * 24 - origin_y + (0 if high is None else tile // 3 * 8)
                        width = 24 if high is None else 8
                        if px + width > 0 and py + width > 0 and px < 320 and py < 192:
                            recorded.append(dict(block=block, tile=tile, word=word))
            if unknown_layout:
                check(
                    "map",
                    "reached mutable region composed delivery",
                    map_binding["value"]
                    if map_binding is not None and map_id in ("map-3", "map-19")
                    else None,
                )
            return recorded

        logical_inventory = _value_set()
        logical_layers = _value_set()
        required_layers = _value_set(
            (
                r["identity"]["visit"],
                r["identity"]["phase"],
                r.get("layer"),
                r.get("highPriority"),
                r.get("subject"),
            )
            for r in requirements
            if r["kind"] == "map"
        )
        for state_index, state in enumerate(
            logical_states if enabled.intersection(("map", "entity")) else ()
        ):
            if budget is not None and state_index % 256 == 0:
                budget.checkpoint("independent logical inventory")
            with evaluated(("map", "entity"), "logical draw occurrence"):
                projection = state.get("cameraProjection") or {}
                presentation = state.get("presentation") or {}
                if (
                    not projection
                    or projection.get("revision") != state.get("revision")
                    or projection.get("map") != state.get("map")
                ):
                    continue
                position = bisect_right(visit_sequences, state["observationSequence"]) - 1
                visit = visit_sequences[position] if position >= 0 else None
                phase = str(presentation.get("activeCue") or "<null>")
                # Godot str(null) is <null>; other cue names are retained verbatim.
                layers = [
                    (name, projection.get(name))
                    for name in ("background", "foreground", "backgroundHigh", "foregroundHigh")
                ]
                layers += [("occlusion", layer) for layer in projection.get("occlusionDraws", [])]
                for name, layer in layers if "map" in enabled else ():
                    if not layer or not layer.get("draws"):
                        continue
                    key = (visit, phase, name, layer.get("highPriority"), layer.get("subject"))
                    if key not in logical_layers:
                        logical_layers.add(key)
                        check(
                            "map",
                            "independent reached layer/pass/subject requirement",
                            True if key in required_layers else None,
                        )
                    with evaluated("map", "independent reached tile"):
                        for tile in layer_tiles(state["map"], layer, name):
                            tile_key = (
                                *key,
                                layer.get("pass"),
                                tile["block"],
                                tile["tile"],
                                tile["word"],
                            )
                            if tile_key not in logical_tiles:
                                logical_tiles.add(tile_key)
                                check(
                                    "map",
                                    "independent reached block/tile requirement",
                                    True if tile_key in required_tiles else None,
                                    tile_key,
                                )
                for logical in (state.get("entities") or []) if "entity" in enabled else ():
                    with evaluated("entity", "logical entity occurrence"):
                        x = logical["x"] / 16 - presentation["cameraX"]
                        y = logical["y"] / 16 - presentation["cameraY"]
                        if not (
                            logical["Visible"] and x + 24 > 0 and y + 24 > 0 and x < 320 and y < 192
                        ):
                            continue
                        nod = state.get("nod") or {}
                        subject = nod.get("Entity")
                        if isinstance(subject, dict):
                            subject = subject.get("Value")
                        want = dict(
                            sprite=logical["sprite"],
                            direction=0
                            if logical["facing"] == 1
                            else 2
                            if logical["facing"] == 3
                            else 1,
                            half=int(15 < logical["animationCounter"] < 128),
                            nod=subject == logical["id"] and bool(nod.get("Lowered")),
                        )
                        key = (
                            visit,
                            phase,
                            logical["id"],
                            logical["slot"],
                            json.dumps(want, sort_keys=True),
                        )
                        if key not in logical_inventory:
                            logical_inventory.add(key)
                            # The startup draw can precede callback installation. Its
                            # current snapshot retains the actual drawn texture selector;
                            # reuse that operand rather than fabricating a later draw.
                            if key not in required_entities:
                                actor = next(
                                    (
                                        a
                                        for a in projection.get("actors", [])
                                        if a.get("entity") == logical["id"]
                                        and a.get("slot") == logical["slot"]
                                        and a.get("visible")
                                    ),
                                    None,
                                )
                                if actor and actor.get("resourceSelector") is not None:
                                    i = {
                                        name: projection.get(name)
                                        for name in (
                                            "sessionId",
                                            "revision",
                                            "observationSequence",
                                            "simulationTick",
                                            "token",
                                            "drawSequence",
                                        )
                                    }
                                    i.update(visit=visit, map=state["map"], phase=phase)
                                    requirement = dict(
                                        identity=i,
                                        kind="entity",
                                        subject=logical["id"],
                                        slot=logical["slot"],
                                        expected=want,
                                        _captureLocator=dict(
                                            source="retained camera projection",
                                            state=state.get("_captureLocator"),
                                            subject=logical["id"],
                                            slot=logical["slot"],
                                        ),
                                    )
                                    requirements.append(requirement)
                                    uses.append(dict(requirement, used=actor["resourceSelector"]))
                                    required_entities.add(key)
                                    requirement_phases.add(_join_key((visit, "entity", phase)))
                                    phase_groups.add(_join_key((visit, "entity")))
                            check(
                                "entity",
                                "independent visible logical subject/pose requirement",
                                True if key in required_entities else None,
                                key,
                            )
                portrait = state.get("portraitProjection") or {}
                if "entity" in enabled and portrait.get("id", -1) >= 0:
                    with evaluated("entity", "independent portrait pose"):
                        work = state.get("portraitWork") or {}
                        flags = state["portraitFlags"]
                        if flags is None or state.get("portraitId") is None:
                            raise KeyError("logical portrait identity")
                        original = source_portraits[state["portraitId"]]
                        tiles = _bounded_list(range(64))
                        for changes in (
                            (original["eyes"] if work.get("EyesClosed") else []),
                            (original["mouth"] if work.get("MouthOpen") else []),
                        ):
                            for x, y, alternate_x, alternate_y in changes:
                                tiles[y * 8 + x] = alternate_y * 8 + alternate_x
                        want = dict(
                            portrait=state["portraitId"],
                            mirror=bool(int(flags) & 0x40),
                            eyes=bool(work.get("EyesClosed")),
                            mouth=bool(work.get("MouthOpen")),
                            tiles=tiles,
                        )
                        key = portrait_pose(visit, phase, want)
                        check(
                            "entity",
                            "independent reached portrait pose requirement",
                            True if key in required_portraits else None,
                            key,
                        )
                    check(
                        "entity",
                        "drawn portrait has logical source identity",
                        None
                        if "portraitId" not in state
                        else portrait["id"] == state["portraitId"],
                    )
        check(
            "entity",
            "independent reached visible logical inventory",
            True if logical_inventory else None,
        )
        result["projectionUseCount"] = uses.count - result["actualUseCount"]

        uses.flush()
        for used in counted_uses("texture"):
            with evaluated("map" if used.get("kind") == "map" else "entity", "actual texture use"):
                if "_keyError" in used:
                    error = used["_keyError"]
                    check(
                        "map" if used["kind"] == "map" else "entity",
                        "actual texture use operand absent"
                        if error == "KeyError"
                        else "actual texture use malformed " + error,
                        None if error == "KeyError" else False,
                    )
                    continue
                i = used["identity"]
                family = "map" if used["kind"] == "map" else "entity"
                if used["kind"] == "map":
                    high = used.get("highPriority")
                    word = used["used"]["word"]
                    name, draw_pass = used.get("layer"), used.get("pass")
                    valid_pass = (
                        {
                            "background": 0,
                            "foreground": 1,
                            "backgroundHigh": 2,
                            "foregroundHigh": 3,
                        }.get(name)
                        == draw_pass
                        if name != "occlusion"
                        else draw_pass >= 5
                    )
                    check(
                        "map",
                        "actual source tile priority and named layer pass",
                        valid_pass
                        and word == int(word)
                        and (high is None or bool(int(word) & 0x8000) == high),
                    )
                check(
                    family,
                    "actual use phase has independent logical requirements",
                    None
                    if _join_key((i["visit"], used["kind"])) not in phase_groups
                    else _join_key((i["visit"], used["kind"], i["phase"])) in requirement_phases,
                )
        result["requirementCount"] = len(requirements)
        result["candidatePairCount"] = 0
        result["executedPairCount"] = 0
        source_recipes = {}

        def source_recipe(kind, want):
            key = _join_key((kind, want))
            if key not in source_recipes:
                source_recipes[key] = _resource_source_events(
                    kind,
                    want,
                    sprites,
                    source_sprites,
                    portraits,
                    source_portraits,
                )
            return source_recipes[key]

        for requirement_index, required in enumerate(requirements):
            if uses.budget is not None and requirement_index % 256 == 0:
                uses.budget.checkpoint("indexed requirement reduction")
            locator = required.get(
                "_captureLocator", dict(channel="resourceRequirements", index=requirement_index)
            )
            with evaluated(
                "map" if required.get("kind") == "map" else "entity", "required texture join"
            ):
                kind, want = required["kind"], required["expected"]
                family = "map" if kind == "map" else "entity"
                key = _resource_key(required, required=True)
                candidate_count = sum(count for _, count, _, _ in uses.variants(key))
                result["candidatePairCount"] += candidate_count
                if kind == "map":
                    visual = maps[required["identity"]["map"]]
                    check(
                        family,
                        "required logical block/tile source word",
                        0 <= want["block"] < len(visual["blocks"])
                        and visual["blocks"][int(want["block"])][int(want["tile"])] == want["word"],
                    )
                check(family, "required actual texture use", True if candidate_count else None)
                compact = _reduce_resource_requirement(required, uses, source_recipe)
                for name, value, count, witness in compact.pop("checks"):
                    weight = count
                    check(family, name, value, witness)
                weight = 1
                compact.update(
                    kind=kind,
                    identity=required["identity"],
                    expected=want,
                    candidateKey=key,
                    locator=locator,
                )
                result["executedPairCount"] += compact["executedPairCount"]
                result["joins"].append(compact)
            weight, locator = 1, None
        result["candidateVariants"] = _bounded_list(uses.published_variants())

        # Source recipe for the accepted three-raster extension, separate from base42.
        with evaluated("scene", "field-death source"):
            if "scene" not in enabled:
                return finish()
            field = read(scene_path.parent / "field-death-provenance.json")
            field_valid = (
                field["upstreamCommit"] == UPSTREAM
                and field["romSha256"] == ROM
                and field["effectSprite"] == 63
            )
            field_valid &= field["allyAssignments"] == [
                dict(character=r["character"], sprite=r["sprite"])
                for r in compiler.initial_ally_sprites()[:3]
            ]
            field_valid &= (
                rom[field["enemyTableAddress"] + field["enemyId"]] == field["enemySprite"] == 103
            )
            palette = [
                md_palette_color(int.from_bytes(rom[i : i + 2], "big"))
                for i in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
            ]
            for direction, span in enumerate(field["spans"]):
                entry = PLAYER_POINTER_TABLE_ADDRESS + (63 * 3 + direction) * 4
                address = int.from_bytes(rom[entry : entry + 4], "big")
                decoded = decode_basic_compressed(rom[address:], expected_output_bytes=576)
                pixels = bytes(
                    _combine_player_halves(
                        _render_player_frame(decoded.output[:288], palette),
                        _render_player_frame(decoded.output[288:], palette),
                    )
                )
                raster = scene["rasters"][scene["fieldDeath"]["exitFrames"][direction]]
                field_valid &= span == dict(
                    pointerAddress=entry, address=address, byteLength=decoded.input_bytes_consumed
                )
                field_valid &= (
                    base64.b64decode(raster["data"], validate=True) == pixels
                    and hashlib.sha256(pixels).hexdigest().upper() == raster["sha256"]
                )
            check("scene", "field death original ROM spans and assignments", field_valid)
        return finish()
    except (FileNotFoundError, KeyError, subprocess.CalledProcessError):
        for family in ("map", "entity", "scene"):
            check(family, "source/use prerequisite absent", None)
    except (ValueError, OSError, IndexError, TypeError) as error:
        result["error"] = str(error)
        for family in ("map", "entity", "scene"):
            check(family, "source/use evidence contradiction " + type(error).__name__, False)
    return finish()


def reached_materials(
    actual,
    selection,
    source_root=None,
    canonical_content=None,
    tileset_metadata=None,
    palette_metadata=None,
    map_binding=None,
):
    """Offline material origin only; natural dispatch/consumer joins stay separate."""
    result = dict(
        scene=None,
        audio=None,
        actorWeapon=None,
        checks=_bounded_list(),
        joins=_bounded_list(),
        visuals=reached_visual_materials(
            actual,
            selection,
            source_root,
            canonical_content,
            tileset_metadata,
            palette_metadata,
            map_binding=map_binding,
        ),
    )
    if not selection:
        return result

    def check(name, value, source):
        result["checks"].append(dict(name=name, value=value, source=source))
        return value

    def verdict(values):
        return False if False in values else None if None in values else True

    def relative_file(root, name):
        path = (root / name).resolve()
        require(path.is_relative_to(root), "material file escapes explicit selected root")
        return path

    try:
        world_path, scene_path, process_path, scene_root, asset_root, commit, tree, manifest_pin = (
            selection
        )
        world_path, scene_path, process_path, scene_root, asset_root = (
            p.resolve() if p.is_absolute() else repo_path(p)
            for p in (world_path, scene_path, process_path, scene_root, asset_root)
        )
        process = read(process_path)
        selected = process.get("selectedInputs", {})
        binding = (
            all(
                repo_path(selected[key]).resolve() == path
                for key, path in (
                    ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
                    ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
                )
            )
            if all(
                selected.get(key)
                for key in ("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_BATTLE_SCENE_CONTENT")
            )
            else None
        )
        check("same-run explicit world/scene selection", binding, "process.selectedInputs")
        world, scene = read(world_path), read(scene_path)
        world_identity = check(
            "selected world original identity",
            world["provenance"]["commit"] == UPSTREAM
            and world["provenance"]["romSha256"] == ROM
            and world["provenance"]["repository"] == ACCEPTED_UPSTREAM_REPOSITORY,
            "world.provenance",
        )
        scene_manifest = read(scene_root / "manifests/presentation-assets-v1.json")
        source_path = scene_root / "source/battle-scenes/selection.json"
        source, report = read(source_path), read(scene_root / "candidate-report.json")
        base = read(scene_root / "battle-scenes.json")

        def digest(data):
            return hashlib.sha256(data).hexdigest().upper()

        historical = {
            name: subprocess.check_output(
                ["git", "show", f"{SCENE_GENERATOR_COMMIT}:{name}"], cwd=repo_path("")
            )
            for name in SCENE_GENERATOR_COMPONENTS
        }
        fingerprints = dict(
            gitLf=_composite_generator_fingerprint(historical),
            historicalCrlf=_composite_generator_fingerprint(
                {name: data.replace(b"\n", b"\r\n") for name, data in historical.items()}
            ),
            current=_composite_generator_fingerprint(
                {name: repo_path(name).read_bytes() for name in SCENE_GENERATOR_COMPONENTS}
            ),
        )
        scene_checks = [binding, world_identity]
        scene_checks.append(
            check(
                "scene bundle original pins and recorded file identities",
                source["upstreamCommit"] == report["upstreamCommit"] == UPSTREAM
                and source["romSha256"] == report["romSha256"] == ROM
                and source["upstreamRepository"] == ACCEPTED_UPSTREAM_REPOSITORY
                and digest(source_path.read_bytes())
                == report["sourceSha256"]
                == SCENE_SOURCE_SHA256
                and digest((scene_root / "manifests/presentation-assets-v1.json").read_bytes())
                == report["manifestSha256"]
                == SCENE_MANIFEST_SHA256
                and (scene_root / "battle-scenes.json").stat().st_size
                == report["sceneContentBytes"]
                and len(scene_manifest["assets"]) == report["assetCount"] == 42,
                "scene-source candidate-report/manifest/source selection",
            )
        )
        scene_checks.append(
            check(
                "recorded historical scene extractor fingerprint",
                fingerprints["historicalCrlf"] == report["generatorArtifactSha256"],
                dict(
                    commit=SCENE_GENERATOR_COMMIT,
                    components=SCENE_GENERATOR_COMPONENTS,
                    representation="historical CRLF checkout",
                    fingerprints=fingerprints,
                ),
            )
        )
        scene_checks.append(
            check(
                "selected scene base content equality",
                all(scene.get(k) == v for k, v in base.items() if k != "rasters")
                and all(scene["rasters"].get(k) == v for k, v in base["rasters"].items())
                and all(
                    len(base64.b64decode(span["data"], validate=True)) == span["byteLength"]
                    for span in source["spans"]
                ),
                "selected scene -> frozen base42 and original ROM spans",
            )
        )
        assets = {a["assetId"]: a for a in scene_manifest["assets"]}
        raster_valid = {}
        for name, raster in base["rasters"].items():
            asset = assets.get("battle.scene." + name.replace("/", "."))
            bucket = [b for b in asset["buckets"] if b["scale"] == 2] if asset else []
            payload = base64.b64decode(raster["data"], validate=True)
            raster_valid[name] = bool(
                asset
                and len(bucket) == 1
                and asset["source"]
                == dict(assetId="source.battle.scene.selection", sha256=report["sourceSha256"])
                and asset["derivation"]["generatorArtifactSha256"]
                == report["generatorArtifactSha256"]
                and digest(payload) == raster["sha256"] == bucket[0]["sha256"]
                and len(payload) == bucket[0]["byteLength"]
                and (raster["width"], raster["height"]) == (bucket[0]["width"], bucket[0]["height"])
            )
        scene_checks.append(
            check(
                "base42 embedded PNG/manifest identities",
                len(raster_valid) == 42 and all(raster_valid.values()),
                "scene rasters -> scale2 buckets/source/derivation",
            )
        )
        mounted = _bounded_list(
            (
                (i, r["scene"])
                for i, r in enumerate(actual.get("sceneObservations", []))
                if r["scene"].get("visible") and not r["scene"].get("fieldDeath")
            )
        )
        backgrounds, actors = [], []
        for i, row in mounted:
            for key in ("background", "backgroundWrap", "ground"):
                node = row.get(key, {})
                if node.get("visible"):
                    name = node.get("resource")
                    backgrounds.append(bool(node.get("texturePresent") and raster_valid.get(name)))
                    result["joins"].append(
                        dict(
                            record=f"sceneObservations[{i}].scene.{key}",
                            resource=name,
                            source="scene-source/base42",
                        )
                    )
            for key in ("allyResource", "enemyResource", "weaponResource"):
                if key == "enemyResource" and not row.get("enemyVisible"):
                    continue
                if key == "weaponResource" and not row.get("weaponVisible"):
                    continue
                if name := row.get(key):
                    actors.append(bool(raster_valid.get(name)))
                    result["joins"].append(
                        dict(
                            record=f"sceneObservations[{i}].scene.{key}",
                            resource=name,
                            source="scene-source/base42",
                        )
                    )
        result["scene"] = verdict(scene_checks + backgrounds) if backgrounds else None
        result["actorWeapon"] = verdict(scene_checks + actors) if actors else None

        inspection = inspect_asset_checkout(
            str(asset_root),
            expected_commit=commit,
            expected_tree=tree,
            expected_manifest_sha256=manifest_pin,
        )
        catalog = json.loads(inspection.manifest_bytes)
        audio_checks = [binding, world_identity]
        audio_checks.append(
            check(
                "explicit pinned clean asset checkout",
                True,
                dict(commit=commit, tree=tree, manifestSha256=manifest_pin),
            )
        )
        provenance = []
        for name, key in (
            ("audio-reached-inventory-provenance.json", "records"),
            ("audio-town-join-provenance.json", "assets"),
        ):
            owner = read(asset_root / "manifests" / name)
            audio_checks.append(
                check(
                    "audio source pins " + name,
                    owner["romSha256"] == ROM
                    and owner["sf2disasmCommit"] == UPSTREAM
                    and owner["emulator"]
                    == dict(
                        name="BizHawk",
                        version="2.11.1",
                        core="Genplus-gx",
                        commit="bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5",
                    ),
                    "manifests/" + name,
                )
            )
            provenance.extend((name, i, r) for i, r in enumerate(owner[key]))
        audio = world["world"]["presentation"]["audio"]
        starts = _bounded_list(
            (
                (i, r["receipt"])
                for i, r in enumerate(actual.get("audioReceipts", []))
                if r["receipt"]["Operation"] == "started"
            )
        )
        for cue in _bounded_sorted({r["Cue"] for _, r in starts}):
            selected_audio = [a for a in audio if a["cue"] == cue]
            library = [a for a in catalog["assets"] if a["kind"] == "audio" and a["cue"] == cue]
            records = [(n, i, r) for n, i, r in provenance if r["asset"]["cue"] == cue]
            unique = len(selected_audio) == len(library) == len(records) == 1
            available = bool(selected_audio and library and records)
            audio_checks.append(
                check(
                    "unique reached audio origin " + cue,
                    unique if available else None,
                    "world audio -> library catalog -> provenance record",
                )
            )
            if not unique:
                continue
            selected_audio, asset = selected_audio[0], library[0]
            owner_name, owner_index, record = records[0]
            runtime = asset["runtime"]
            with wave.open(str(relative_file(asset_root, runtime["runtimePath"])), "rb") as wav:
                pcm = wav.readframes(wav.getnframes())
                format_equal = (
                    wav.getsampwidth(),
                    wav.getnchannels(),
                    wav.getframerate(),
                    wav.getnframes(),
                ) == (2, runtime["channels"], runtime["sampleRate"], runtime["sampleFrames"])
            capture_path = relative_file(asset_root, record["sourcePath"])
            with wave.open(str(capture_path), "rb") as capture:
                begin, end = record["captureStartSample"], record["captureEndSample"]
                capture.setpos(begin)
                cut_equal = capture.readframes(end - begin) == pcm and (
                    capture.getsampwidth(),
                    capture.getnchannels(),
                    capture.getframerate(),
                ) == (2, runtime["channels"], runtime["sampleRate"])
            valid = (
                record["asset"] == asset
                and format_equal
                and cut_equal
                and digest(capture_path.read_bytes()) == asset["source"]["sha256"]
                and end - begin == runtime["sampleFrames"]
                and base64.b64decode(selected_audio["pcm16"], validate=True) == pcm
                and digest(pcm) == selected_audio["sha256"]
                and all(
                    selected_audio[k] == runtime[k]
                    for k in ("sampleRate", "channels", "sampleFrames", "loopBegin", "loopEnd")
                )
                and (selected_audio["command"], selected_audio["timerB"])
                == (asset["command"], asset["timerB"])
            )
            audio_checks.append(
                check(
                    "reached original capture cut/runtime PCM " + cue,
                    valid,
                    dict(
                        file="manifests/" + owner_name,
                        record=owner_index,
                        assetId=asset["assetId"],
                        captureStartSample=begin,
                        captureEndSample=end,
                        loopBegin=runtime["loopBegin"],
                        loopEnd=runtime["loopEnd"],
                    ),
                )
            )
            for i, receipt in ((i, r) for i, r in starts if r["Cue"] == cue):
                requested = receipt.get("RequestedTimerB")
                choices = [a for a in audio if a["command"] == receipt["Command"]]
                exact = [a for a in choices if a["timerB"] == requested]
                finite = [a for a in choices if a["loopBegin"] is None]
                policy = requested is None or (
                    len(exact) == 1
                    and exact[0] == selected_audio
                    or not exact
                    and len(finite) == 1
                    and finite[0] == selected_audio
                )
                valid = policy and all(
                    receipt[a] == selected_audio[b]
                    for a, b in (
                        ("Command", "command"),
                        ("TimerB", "timerB"),
                        ("PcmSha256", "sha256"),
                        ("SampleRate", "sampleRate"),
                        ("Channels", "channels"),
                        ("SampleFrames", "sampleFrames"),
                        ("LoopBegin", "loopBegin"),
                        ("LoopEnd", "loopEnd"),
                    )
                )
                audio_checks.append(
                    check(
                        f"audio start material selection receipt{i}",
                        valid,
                        "SessionAudio.Select exact timer/unique finite policy",
                    )
                )
                result["joins"].append(
                    dict(
                        record=f"audioReceipts[{i}].receipt",
                        cue=cue,
                        assetId=asset["assetId"],
                        requestedTimerB=requested,
                        assetTimerB=asset["timerB"],
                        provenance=owner_name,
                        sourceRecord=owner_index,
                        selection="exact"
                        if requested == asset["timerB"]
                        else "named cue"
                        if requested is None
                        else "unique finite fallback",
                    )
                )
        result["audio"] = verdict(audio_checks) if starts else None
    except (FileNotFoundError, subprocess.CalledProcessError):
        check("selected material input availability", None, "explicit selected inputs")
    except AssetPreflightError as error:
        # The existing validator distinguishes absence from an observed drift.
        unavailable = error.code in (
            "RepositoryUnavailable",
            "PayloadUnavailable",
            "SchemaUnavailable",
        )
        check("asset checkout " + error.code, None if unavailable else False, error.field)
        result["audio"] = None if unavailable else False
    except KeyError:
        check("selected material field availability", None, "explicit selected inputs")
    except (ValueError, wave.Error) as error:
        check("selected material evidence shape", False, type(error).__name__)
    return result


def plain_join_binding(ref, actual, evidence_root, world_path):
    """Bind only the accepted JOIN occurrence; indices locate evidence, not legality.

    The four selected files reuse the accepted segment seals. This is an offline
    consumer readback, not resumability validation or original music completion.
    Missing named evidence is unavailable; a present contradiction is false.
    """
    result = dict(original=None, plain=None, audio=None, caller=None, anchors={})
    plain_value = None

    def finalize():
        # Every exit preserves observed contradictions without closing partial evidence.
        values = [plain_value, result["audio"], result["caller"]]
        result["plain"] = False if False in values else None if None in values else True
        return result

    if evidence_root is None or world_path is None:
        return finalize()
    evidence_root, world_path = (
        p.resolve() if p.is_absolute() else repo_path(p) for p in (evidence_root, world_path)
    )
    require(
        evidence_root.is_relative_to(repo_path("local")),
        "JOIN evidence must be selected beneath this worktree's local/",
    )
    paths = [
        evidence_root / name
        for name in (
            "candidate.json",
            "runtime/segment-pair.json",
            "runtime/checkpoints.jsonl",
            "runtime/actual-inputs.jsonl",
        )
    ]
    if not all(p.is_file() for p in paths):
        return finalize()

    def digest(path):
        return hashlib.sha256(path.read_bytes()).hexdigest().upper()

    try:
        candidate, pair = read(paths[0]), read(paths[1])
        sealed = (
            digest(paths[1]) == ref["lineage"][0]["pairSha256"]
            and digest(paths[0]) == pair["material"]
            and all(digest(p) == pair["files"][p.name] for p in paths[2:])
            and [
                candidate[k]
                for k in ("RomSha256", "SourceCommit", "ObserverSha256", "RunnerSha256")
            ]
            == [ROM, UPSTREAM, OBSERVER, RUNNER]
        )
        result["original"] = sealed
        if not sealed:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        result["original"] = None
        checkpoints = [row for _, row in rows(paths[2])]
        inputs = [row for _, row in rows(paths[3])]
        # One-based locations from the sealed prepared68 witness, selected by the
        # accepted reference operation pair, not a new reference inferred from host output.
        op = ref["operationPairs"][83]
        entry, accepted, close, returned, script, ready = (
            checkpoints[i - 1] for i in (2608, 2609, 2610, 2612, 2621, 2624)
        )
        applying, frame = inputs[17233], inputs[17234]
        original_plain = (
            checkpoints[2572]["kind"] == "audio:request"
            and checkpoints[2572]["facts"]["command"] == 19
            and checkpoints[2573]["kind"] == "DisplayText:entry"
            and checkpoints[2573]["facts"]["target"] == 447
            and checkpoints[2602]["kind"] == "DisplayText:return"
            and checkpoints[2602]["facts"]["target"] == 447
            and [checkpoints[i]["facts"]["command"] for i in (2603, 2604)] == [240, 251]
            and checkpoints[2605]["kind"] == "audio:consumer-dispatch"
            and checkpoints[2606]["kind"] == "audio:mailbox-written"
            and checkpoints[2605]["facts"]["command"] == checkpoints[2606]["facts"]["command"] == 8
            and checkpoints[2606]["order"] < entry["order"]
            and entry["kind"] == "WaitForPlayerInput:entry"
            and accepted["kind"] == "WaitForPlayerInput:return"
            and entry["facts"]["target"] == accepted["facts"]["target"] == 0x1576
            and entry["state"]["input"] == 0
            and accepted["state"]["input"] == 32
            and accepted["facts"]["d0"] == 3
            and applying["kind"] == "applying"
            and frame["kind"] == "frame"
            and applying["button"] == frame["button"] == "C"
            and applying["id"] == frame["id"]
            and applying["order"] < accepted["order"] < frame["order"]
            and close["kind"] == "CloseDialogueWindow:entry"
            and returned["facts"]["operation"] == dict(opcode=8, pc=0x51630)
            and returned["order"] == op["return"]["order"]
            and script["kind"] == "script:return"
            and script["facts"]["target"] == 0x5149A
            and entry["order"]
            < accepted["order"]
            < close["order"]
            < returned["order"]
            < script["order"]
            < ready["order"]
            and not any(c["state"]["flags"]["603"] for c in (entry, accepted, returned, script))
            and ready["state"]["flags"]["603"]
            and ready["state"]["pendingReturns"] == 0
        )
        result["original"] = original_plain
        result["anchors"]["original"] = dict(
            segment=68,
            checkpoints=[2608, 2609, 2610, 2612, 2621, 2624],
            actualInputs=[17234, 17235],
            upstream=UPSTREAM,
            helperReturnOrder="Inferred",
            originalMusicCompletion="Unknown",
        )
        if not original_plain:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
    except json.JSONDecodeError:
        result.update(original=False, plain=False, audio=False, caller=False)
        return finalize()
    except (KeyError, IndexError):
        return finalize()

    try:
        labels = (
            "music-plain-input",
            "music-plain-poll",
            "music-plain-accepted",
            "join-field-return",
        )
        selected = [
            [(i, s["state"]) for i, s in enumerate(actual["samples"]) if s["label"] == label]
            for label in labels
        ]
        if any(not group for group in selected):
            return finalize()
        if any(len(group) != 1 for group in selected):
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        (pi, plain), (wi, polled), (ai, acked), (ri, ready) = (group[0] for group in selected)
        records = actual["warpRecords"]
        observations = _bounded_list(
            ((i, o) for i, r in enumerate(records) for o in r["result"]["observations"])
        )
        late = _bounded_list(
            (
                (i, row["state"])
                for i, row in enumerate(actual["samples"])
                if row["label"] == "music-logical-end"
            )
        )
        if len(late) > 1:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        held = _bounded_list(
            (
                (i, row["state"])
                for i, row in enumerate(records)
                if row.get("state", {}).get("wait") == "MusicWait"
                and row["state"]["revision"] < plain["revision"]
            )
        )
        if not held:
            return finalize()
        li, logical = late[0] if late else (None, held[0][1])
        initial_starts = _bounded_list(
            row["receipt"]
            for row in actual["audioReceipts"]
            if row["receipt"]["Cue"] == "MUSIC_JOIN"
            and row["receipt"]["Operation"] == "started"
            and row["receipt"]["Revision"] < logical["revision"]
        )
        if late:
            generation = logical["music"]["Generation"]
        else:
            if logical.get("sessionId") != plain.get("sessionId"):
                result["audio"] = False
            if not world_path.is_file():
                return finalize()
            early_world = read(world_path)["world"]
            early_programs = {p["id"]: p for p in early_world["programs"]}

            def source_operation(event):
                location = event.get("Program")
                if location is None:
                    return None
                return early_programs[location["Program"]]["instructions"][
                    int(location["Instruction"])
                ]

            helper_sources = [
                o
                for _, o in observations
                if o["Kind"] == "program-instruction"
                and o["Sequence"] <= logical["revision"]
                and (source_operation(o) or {}).get("kind") == "SoundWait"
            ]
            if not helper_sources:
                return finalize()
            install = helper_sources[-1]
            request_sources = [
                o
                for _, o in observations
                if o["Kind"] == "program-instruction"
                and o["Sequence"] < install["Sequence"]
                and (source_operation(o) or {}).get("kind") == "Sound"
                and (source_operation(o) or {}).get("resource") == "MUSIC_JOIN"
            ]
            if not request_sources:
                return finalize()
            request = request_sources[-1]
            generation = request["Sequence"]
            if (
                logical.get("token") != install["Sequence"]
                or logical.get("cursor") != install["Program"]
                or any(row["Revision"] != generation for row in initial_starts)
            ):
                result["audio"] = False

        def event(kind, detail=None):
            lower = (
                acked["revision"]
                if kind in ("simulation-tick", "zone-finished")
                else generation - 1
                if kind == "music-actual-completed"
                else logical["revision"]
            )
            found = [
                (i, o)
                for i, o in observations
                if o["Kind"] == kind
                and (detail is None or o["Detail"] == detail)
                and lower < o["Sequence"] <= ready["revision"]
            ]
            if not found:
                raise KeyError(kind)
            return found

        released = event("music-wait-returned", "MUSIC_JOIN")
        if not late and any(state["revision"] >= released[0][1]["Sequence"] for _, state in held):
            result["audio"] = False
        completed = event("music-actual-completed", "MUSIC_JOIN")
        previous = event("presentation-completed", "PreviousMusic")
        acknowledged = event("presentation-acknowledged")
        pressed = _bounded_list(r for r in actual["inputRecords"] if r["pressed"])
        early = [
            r
            for r in pressed
            if r["before"]["revision"] == logical["revision"]
            and r["before"]["token"] == logical["token"]
        ]
        wait = [
            r
            for r in pressed
            if r["before"]["revision"] == plain["revision"] and r["action"] == "wait"
        ]
        confirm = [
            r
            for r in pressed
            if r["before"]["revision"] == polled["revision"] and r["action"] == "confirm"
        ]
        if (late and not early) or not wait or not confirm:
            return finalize()
        wait, confirm = wait[0], confirm[0]

        def delivered(record, kind):
            return any(
                o["Kind"] == kind
                for r in records[record["resultStart"] : record["resultEnd"]]
                for o in r["result"]["observations"]
            )

        plain_value = (
            (
                li < pi < wi < ai < ri
                if late
                else logical["revision"] < plain["revision"] and pi < wi < ai < ri
            )
            and len(completed) == len(released) == len(previous) == len(acknowledged) == 1
            and completed[0][1]["Sequence"]
            < released[0][1]["Sequence"]
            < previous[0][1]["Sequence"]
            < plain["revision"]
            < acknowledged[0][1]["Sequence"]
            < acked["revision"]
            and plain["wait"] == polled["wait"] == "DialogueWait"
            and plain["token"] == polled["token"] == confirm["before"]["token"]
            and plain["cursor"] == polled["cursor"] == confirm["before"]["cursor"]
            and acked["wait"] == confirm["after"]["wait"] == "TextCloseWait"
            and all(
                side[key] == state[key]
                for side, state in (
                    (wait["before"], plain),
                    (wait["after"], polled),
                    (confirm["before"], polled),
                    (confirm["after"], acked),
                )
                for key in ("revision", "simulationTick", "mainSeed", "token", "cursor")
            )
            and (not late or {r["action"] for r in early} == {"wait", "confirm"})
            and all(r["resultStart"] == r["resultEnd"] and r["before"] == r["after"] for r in early)
            and polled["simulationTick"] == plain["simulationTick"] + 1
            and acked["simulationTick"] == polled["simulationTick"]
            and plain["mainSeed"] == polled["mainSeed"] == acked["mainSeed"]
            and delivered(wait, "gameplay-wait")
            and delivered(confirm, "presentation-acknowledged")
            and all(
                s["w1"] is None and s["fieldText"] is None and 603 not in s["flags"]
                for s in (plain, polled, acked)
            )
        )
        music = logical["music"] if late else None
        helper = logical["musicWait"] if late else None
        helper_token = helper["Token"]["Value"] if late else install["Sequence"]
        receipts = _bounded_list(r["receipt"] for r in actual["audioReceipts"])
        previous_starts = [
            r
            for r in receipts
            if r["Operation"] == "started"
            and r["Cue"].startswith("MUSIC_")
            and r["Revision"] < generation
        ]
        if not late and not previous_starts:
            return finalize()
        previous_cue = music["Previous"][0] if late else previous_starts[-1]["Cue"]
        starts = [
            r
            for r in receipts
            if r["Cue"] == "MUSIC_JOIN"
            and r["Operation"] == "started"
            and r["Revision"] == generation
        ]
        finishes = [
            r
            for r in receipts
            if r["Cue"] == "MUSIC_JOIN"
            and r["Operation"] == "finished"
            and (
                r["Revision"] == logical["revision"]
                if late
                else generation <= r["Revision"] < plain["revision"]
            )
        ]
        if not starts or not finishes:
            return finalize()
        start, finish = starts[0], finishes[0]
        finish_context = next(
            (
                row["state"]
                for row in reversed(records)
                if row.get("state", {}).get("revision") == finish["Revision"]
            ),
            None,
        )
        if not late and (finish_context is None or "token" not in finish_context):
            return finalize()
        restarts = [
            r
            for r in receipts
            if r["Cue"] == previous_cue
            and r["Operation"] == "started"
            and finish["Sequence"] < r["Sequence"]
            and r["Revision"] < plain["revision"]
        ]
        transitional = _bounded_list(
            s["state"]
            for s in (actual["samples"][li + 1 : pi] if late else [])
            if s["state"]["audio"]["musicGeneration"] == generation
        )
        if not restarts or (late and not transitional):
            return finalize()
        restart = restarts[0]
        interval = [
            r
            for r in receipts
            if start["Sequence"] <= r["Sequence"] <= restart["Sequence"]
            and r["Cue"].startswith("MUSIC_")
        ]
        if not late:
            if not world_path.is_file():
                return finalize()
            request_op, install_op = source_operation(request), source_operation(install)
            profile = next(
                (row for row in early_world["presentation"]["audio"] if row["cue"] == "MUSIC_JOIN"),
                None,
            )
            if profile is None or profile.get("modernEndStep") is None:
                return finalize()
            end = profile["modernEndStep"]
            before_steps = [
                o
                for _, o in observations
                if generation < o["Sequence"] < helper_token
                and o["Kind"] == "music-step"
                and o.get("Detail") == "MUSIC_JOIN"
            ]
            initial_step = min(end, len(before_steps))
            progress = [
                o
                for _, o in observations
                if helper_token < o["Sequence"] < released[0][1]["Sequence"]
                and o["Kind"] in ("music-step", "music-wait-armed", "music-previous-eligible")
                and o.get("Detail") == "MUSIC_JOIN"
            ]
            services = [
                o
                for _, o in observations
                if helper_token < o["Sequence"] < released[0][1]["Sequence"]
                and o["Kind"] == "music-helper-service"
            ]
            armed = [o for o in progress if o["Kind"] == "music-wait-armed"]
            eligible = [o for o in progress if o["Kind"] == "music-previous-eligible"]
            if not progress or not services or not armed or not eligible:
                return finalize()
            if completed[0][1]["Sequence"] > services[-1]["Sequence"]:
                # A real late-held interval requires its own retained state and
                # attempted-input/no-debt operands; its missing sample is not early.
                return finalize()
            needed = max(2, end - initial_step)
            groups = ((needed + 2) // 3) * 3
            attempts = [
                r
                for r in pressed
                if helper_token <= r["before"]["revision"] < released[0][1]["Sequence"]
            ]
            plain_value = plain_value and all(
                r["resultStart"] == r["resultEnd"] and r["before"] == r["after"] for r in attempts
            )
            result["audio"] = (
                result["audio"] is not False
                and len(starts) == len(finishes) == len(restarts) == 1
                and interval == [start, finish, restart]
                and request["Kind"] == install["Kind"] == "program-instruction"
                and request_op is not None
                and request_op.get("op") == "present"
                and request_op.get("kind") == "Sound"
                and request_op.get("resource") == "MUSIC_JOIN"
                and install_op is not None
                and install_op.get("op") == "present"
                and install_op.get("kind") == "SoundWait"
                and install["Program"] == logical["cursor"]
                and len(progress) == len(services) == groups
                and len(armed) == len(eligible) == 1
                and progress[0] == armed[0]
                and eligible[0]["Sequence"] == progress[int(needed) - 1]["Sequence"]
                and all(
                    p["Sequence"] < v["Sequence"]
                    and (
                        index + 1 == len(progress)
                        or v["Sequence"] < progress[index + 1]["Sequence"]
                    )
                    for index, (p, v) in enumerate(zip(progress, services, strict=False))
                )
                and all(
                    state["token"] == helper_token
                    and state["cursor"] == logical["cursor"]
                    and state["sessionId"] == plain["sessionId"]
                    and state["simulationTick"]
                    == logical["simulationTick"]
                    - sum(v["Sequence"] <= logical["revision"] for v in services)
                    + sum(v["Sequence"] <= state["revision"] for v in services)
                    for _, state in held
                )
                and finish_context["sessionId"] == plain["sessionId"]
                and finish["WaitToken"] == finish_context["token"]
                and start["PcmSha256"] == finish["PcmSha256"]
                and not finish["Playing"]
                and restart["Playing"]
                and finish["Revision"] < completed[0][1]["Sequence"]
                and restart["Revision"] < previous[0][1]["Sequence"]
                and plain["audio"]["musicCue"] == restart["Cue"]
                and plain["audio"]["musicPlaying"]
                and plain["audio"]["error"] is None
            )
            result["anchors"]["earlyLogicalWork"] = dict(
                request=generation,
                helper=helper_token,
                endStep=end,
                initialStep=initial_step,
                progressSequences=[p["Sequence"] for p in progress],
                serviceSequences=[v["Sequence"] for v in services],
                eligible=eligible[0]["Sequence"],
                previousCue=previous_cue,
            )
        else:
            result["audio"] = (
                len(starts) == len(finishes) == len(restarts) == 1
                and interval == [start, finish, restart]
                and music["Cue"] == "MUSIC_JOIN"
                and music["Step"] == music["EndStep"]
                and music["PreviousEligible"]
                and not music["ActualDone"]
                and helper["Generation"] == generation
                and helper["LogicalDone"]
                and helper["Armed"]
                and helper["Cleared"]
                and logical["audio"]["musicGeneration"] == generation
                and logical["audio"]["musicPlaying"]
                and not logical["audio"]["musicFinished"]
                and finish["WaitToken"] == helper["Token"]["Value"]
                and start["PcmSha256"] == finish["PcmSha256"]
                and not finish["Playing"]
                and restart["Playing"]
                and all(
                    s["audio"]["musicFinished"]
                    and not s["audio"]["musicPlaying"]
                    and s["audio"]["error"] is None
                    for s in transitional
                )
                and plain["audio"]["musicCue"] == restart["Cue"]
                and plain["audio"]["musicPlaying"]
                and plain["audio"]["musicPosition"] > 0
                and plain["audio"]["error"] is None
                and completed[0][1]["Sequence"] > finish["Revision"]
                and restart["Revision"] < previous[0][1]["Sequence"]
            )
        if not world_path.is_file():
            return finalize()
        world = read(world_path)["world"]
        program = next(p for p in world["programs"] if p["id"] == "cs-51614")
        instructions = program["instructions"]
        begin = plain["cursor"]["Instruction"]
        tail = instructions[int(begin) :]
        actual_tail = [
            (i, o)
            for i, o in observations
            if o["Kind"] == "program-instruction"
            and o["Program"]["Program"] == program["id"]
            and acked["revision"] <= o["Sequence"] <= ready["revision"]
        ]
        ticks = event("simulation-tick")
        flag = [
            (i, o)
            for i, o in observations
            if o["Kind"] == "program-instruction"
            and o["Detail"] == "WriteFlag"
            and acked["revision"] < o["Sequence"] <= ready["revision"]
        ]
        zone_finished = event("zone-finished")
        arrivals = _bounded_list(
            s["state"]
            for s in actual["samples"][ai + 1 : ri]
            if s["state"]["wait"] == "ZoneArrivalWait"
        )
        if not arrivals:
            return finalize()
        zone = next(p for p in world["programs"] if p["id"] == "map3-zoneevent8")
        entities = {e["id"]: e for e in ready["entities"]}
        effects = all(
            entities[i["entity"]]["follower"]
            == dict(
                LeaderSlot=int(i["leader"].removeprefix("entity-")),
                OffsetX=i["x"],
                OffsetY=i["y"],
            )
            for i in tail[3:5]
        ) and all(
            entities[i["entity"]]["x"] == i["position"]["x"] * 384
            and entities[i["entity"]]["y"] == i["position"]["y"] * 384
            and entities[i["entity"]]["facing"] == i["facing"]
            for i in tail[5:7]
        )
        result["caller"] = (
            effects
            and program["source"]
            == "disasm/data/maps/entries/map03/mapsetups/scripts_1.asm:cs_51614"
            and instructions[int(begin) - 4]["text"] == 447
            and instructions[int(begin) - 3]["waitForAcknowledgement"] is False
            and instructions[int(begin) - 2]["kind"] == "SoundWait"
            and instructions[int(begin) - 1]["kind"] == "PreviousMusic"
            and [i["op"] for i in tail]
            == [
                "wait-text-input",
                "close-text",
                "wait-ticks",
                "follow",
                "follow",
                "position",
                "position",
                "jump",
            ]
            and tail[2]["ticks"] == 10
            and [o["Program"]["Instruction"] for _, o in actual_tail]
            == _bounded_list(range(int(begin) + 1, len(instructions)))
            and [o["Detail"] for _, o in actual_tail]
            == [
                "CloseText",
                "WaitProgramTicks",
                "FollowEntity",
                "FollowEntity",
                "SetEntityPosition",
                "SetEntityPosition",
                "JumpProgram",
            ]
            and len(ticks) == tail[2]["ticks"]
            and actual_tail[1][1]["Sequence"]
            < ticks[0][1]["Sequence"]
            <= ticks[-1][1]["Sequence"]
            < actual_tail[2][1]["Sequence"]
            and len(flag) == len(zone_finished) == 1
            and flag[0][1]["Program"]["Program"] == zone["id"]
            and zone["instructions"][int(flag[0][1]["Program"]["Instruction"])]
            == dict(op="set-flag", flag=603, value=True)
            and actual_tail[-1][1]["Sequence"]
            < flag[0][1]["Sequence"]
            < zone_finished[0][1]["Sequence"]
            == ready["revision"]
            and 603 in ready["flags"]
            and ready["wait"] is None
            and ready["cursor"] is None
            and ready["canWaitAtInput"]
            and all(603 in s["flags"] and not s["canWaitAtInput"] for s in arrivals)
            and {1, 2}.issubset(ready["partyLists"]["Joined"])
        )
        result["anchors"]["actual"] = dict(
            samples=[li, pi, wi, ai, ri],
            completionOrder="late" if late else "early",
            heldHelperRecords=[i for i, _ in held],
            generation=generation,
            helperToken=helper_token,
            receiptSequences=[r["Sequence"] for r in interval],
            inputOrdinals=[r["ordinal"] for r in early] + [wait["ordinal"], confirm["ordinal"]],
            completionRecords=[completed[0][0], released[0][0], previous[0][0]],
            callerRecords=[i for i, _ in actual_tail] + [flag[0][0], zone_finished[0][0]],
        )
    except (KeyError, IndexError, StopIteration):
        pass
    return finalize()


MATRIX_OBLIGATION = "complete named continuous settings matrix"


# The admitted cohort inventory is evidence scope, never a gameplay admission rule.
def _field_case_input(case):
    """Pass the existing document reader to the bounded field case loader."""
    return _field_input(case, read)


def battle_scene_consumer_binding(actual, context, source_root):
    """Pass the existing document reader to the extracted scene comparison."""
    return _scene_consumer_binding(actual, context, source_root, read)


def field_service_binding(actual, context, source_root):
    """Pass the existing document reader to the extracted field comparison."""
    return _field_binding(actual, context, source_root, read)


def audio_consumer_binding(actual, context, source_root):
    """Pass the existing bounded report factory to the extracted audio comparison."""
    return _audio_consumer_binding(actual, context, source_root, _bounded_list)


def field_motion_binding(actual, selection, source_root):
    """Join source producers to occurrence-local field waits and actual consumers."""
    return _field_motion_binding(
        actual,
        selection,
        source_root,
        read=read,
        _bounded_list=_bounded_list,
        _occurrence_map=_occurrence_map,
        _bounded_sorted=_bounded_sorted,
        _group_rows=_group_rows,
    )


def operation_flow_binding(actual, selection, source_root, motion, text):
    """Bind complete reached source bodies to dynamic control and ordered effects."""
    return _operation_flow_binding(
        actual,
        selection,
        source_root,
        motion,
        text,
        read=read,
        _bounded_list=_bounded_list,
        _occurrence_map=_occurrence_map,
        _bounded_sorted=_bounded_sorted,
        _occurrence_dict=_occurrence_dict,
    )


def modern_required_children(variant, ref):
    """Frozen winning-profile children; observed subsets do not enlarge this set."""
    ally_ids = [a["id"] for a in ref["admission"]["accounting"]["allies"][:3]]
    families = {
        None: (
            "map",
            "x",
            "y",
            "facing",
            "gold",
            "mainSeed",
            *(f"ally-{ally}.{field}" for ally in ally_ids for field in ("Hp", "Mp", "Status")),
            "admission roster",
            "one continuous session",
            "monotonic observed sequence",
            "full result stream",
            "ordinary route and probe assertions",
            "mandatory reached checkpoints",
            "natural battle first control",
            "adaptive actions and consumed outcome",
            "whole after/return order",
            "victory flags/program/return",
            "accepted original Down extension selection",
            "returned actual field input",
            *(
                label + " two settled host updates"
                for label in ("first-return", "before-down", "after-down")
            ),
            "ordinary Down displacement",
            "clean actual process",
            "actual physical/consumer input records",
            "configured device and release observations",
            MATRIX_OBLIGATION,
            "modern finite-music deviation declaration",
            "observed input dispatch intervals",
            "host delivery adds no gameplay or RNG service",
            *(("actual reveal-only Confirm",) if variant == "C" else ()),
            *(("natural reveal before Confirm",) if variant == "D" else ()),
        ),
        "complete mandatory operation-to-consumption mapping": (
            "after-program join/flag/return causal order",
            "taken route/setup/caller branch operands and occurrence",
            "awaited entity motion/gesture/fade before caller return",
            "dialogue speaker/control-token occurrence and choice effect",
            "route roster/flag writes at their source branch",
            "warp destination/setup initialization before field release",
            "before/after operation effects and shared-tail return pairing",
        ),
        "matched-state rule/RNG/draw-to-effect comparisons": (
            "repeated logical sequence identifies the same observation",
            "recorded main draw operands are available",
            "turn candidate score draws and tie/order result",
            "physical range/dodge/critical/spread/double/counter effects",
            "HEAL recovery/cost/fairy opportunity and seed effects",
            "EXP/gold/growth/spell learning and after-turn/outcome effects",
            "AI thinking draw/choice/memory and movement decision",
            "field text/portrait/NPC service draw-to-effect gates",
        ),
        "complete reached 7C resource/provenance inventory": (
            "reached map3/19/20/21/40/57 atlas and layer identities",
            "reached entity sprites/portraits/gesture resource identities",
            "displayed text tokens/font/glyph private binding",
            "scene actor/weapon/healing/death resources",
            "scene background/ground actual resource identity",
            "reached audio command/timer/PCM provenance and playback lifecycle",
        ),
        "required unshimmed ack and scene consumer binding": (
            "W1 displayed token occurrence/accepting read/service gates",
            "W2 accepting read/validation indicator and token return",
            "plain JOIN input after matching finite completion",
            "entity motion/gesture/fade consumer start/completion before resume",
            "battle scene command/resources/wait/effect/end consumer edges",
            "audio replacement/fade/stop/resume dependent consumer edges",
        ),
        "complete relevant admission phase/field mapping": (
            "admission joined",
            "admission active",
            "admission logical consumer readiness",
            "admission seed-copy byte",
            "opening mouth/view controls before first source write",
            "admission occupied physical slots",
            "effective admission class/level/maxima/stats/spells definition identity",
            "walking motion gate/velocity/travel/flags correspondence",
            *(
                f"admission walking slot {slot} {field}"
                for slot in (5, 6, 8)
                for field in ("wait timer", "cursor/moving source binding")
            ),
            *(f"ally-{ally} candidate class/stats/spell words" for ally in ally_ids),
        ),
    }
    for ally in ally_ids:
        parent = f"ally-{ally}.items"
        families[parent] = tuple(
            parent + "." + field
            for field in (
                "effective four-slot words",
                "candidate definition slots",
                "admission loadout identity",
            )
        )
    for parent, prefix, state in (
        ("complete relevant admission phase/field mapping", "admission", ref["admission"]),
        (
            "complete mandatory operation-to-consumption mapping",
            "returned story",
            ref["endpoint"],
        ),
    ):
        families[parent] += tuple(f"{prefix} flag {flag}" for flag in state["state"]["flags"])
    families["complete relevant admission phase/field mapping"] += tuple(
        f"admission slot {e['physical']} position/destination/facing/layer"
        for e in ref["inherited"]["entities"]
        if not (e["actionScript"] == 0 and e["x"] == e["y"] == 0x7000)
    )
    return families


def verdict(counts):
    return "FAIL" if counts.get("FAIL") else "Unavailable" if counts.get("Unavailable") else "PASS"


def modern_report_integrity(report, ref):
    """Reject omitted obligations and contradictory serialization, not absent evidence."""
    errors = []
    if report.get("comparisonScope", "full") != "full":
        errors.append("selected-scope report cannot satisfy full H4 obligations")
    assertions = report.get("assertions", [])
    parents = report.get("coverageObligations", [])
    required = [a for a in assertions if a.get("applicability") != "historical-diagnostic"]
    historical = [a for a in assertions if a.get("applicability") == "historical-diagnostic"]
    names = Counter(a.get("assertion") for a in assertions)
    parent_names = Counter(p.get("assertion") for p in parents)
    for name, count in names.items():
        if count != 1:
            errors.append("duplicate assertion: " + str(name))
    for name, count in parent_names.items():
        if count != 1:
            errors.append("duplicate coverage parent: " + str(name))
    for parent, children in modern_required_children(report.get("variant"), ref).items():
        if parent is not None and parent_names[parent] != 1:
            errors.append("missing required family: " + parent)
        for name in children:
            matches = [
                a for a in required if a.get("assertion") == name and a.get("parent") == parent
            ]
            if len(matches) != 1:
                errors.append("missing required child: " + name)
    for row in assertions:
        if row.get("applicability") not in (
            "applicable",
            "required-unobserved",
            "historical-diagnostic",
        ):
            errors.append("invalid applicability: " + str(row.get("assertion")))
        expected_result = (
            "Unavailable"
            if row.get("actualValue") is None or row.get("applicability") == "required-unobserved"
            else "PASS"
            if row.get("expected") == row.get("actualValue")
            else "FAIL"
        )
        if row.get("result") != expected_result:
            errors.append("inconsistent assertion result: " + str(row.get("assertion")))
        if row.get("parent") and parent_names[row["parent"]] != 1:
            errors.append("missing assertion parent: " + row["parent"])
    for parent in parents:
        children = [a for a in required if a.get("parent") == parent.get("assertion")]
        old = [a for a in historical if a.get("parent") == parent.get("assertion")]
        counts = dict(Counter(a.get("result") for a in children))
        if (
            parent.get("children") != [a.get("assertion") for a in children]
            or parent.get("historicalChildren") != [a.get("assertion") for a in old]
            or parent.get("actualValue") != counts
            or parent.get("result") != verdict(counts)
            or parent.get("applicability")
            != ("required-unobserved" if verdict(counts) == "Unavailable" else "applicable")
        ):
            errors.append("inconsistent coverage parent: " + str(parent.get("assertion")))
    counts = dict(Counter(a.get("result") for a in required))
    if (
        report.get("counts") != counts
        or report.get("result") != verdict(counts)
        or report.get("milestonePass") is not (verdict(counts) == "PASS")
        or report.get("historicalCounts") != dict(Counter(a.get("result") for a in historical))
    ):
        errors.append("inconsistent report summary")
    return errors


def walking_admission_binding(ref, actual, evidence_root, world_path, original_binding):
    """Translate the pinned R1 walking continuation, then observe its consumption.

    The 50-byte eas_Walking layout includes the branch's external displacement.
    ClearEntities/SetWalkingActscript own these buffers; addresses locate this
    witness only. UpdateEntityData/esc01 define movement and destination waiting.
    No original velocity magnitude or frame duration is a modern expectation.
    """
    parts = {slot: [original_binding] for slot in (5, 6, 8)}
    motion_parts = [original_binding]
    anchors = dict(
        source=ref["inherited"]["source"],
        upstream=UPSTREAM,
        template="eas_Walking:50bytes; wait30@0/randomWalk@32/waitDest@40/wait20@42/branch@46",
        hiddenMotionGate="Inferred",
        slots={},
    )

    def combined(values):
        return False if False in values else None if None in values else True

    def finish():
        return dict(
            slots={slot: combined(values) for slot, values in parts.items()},
            motion=combined(motion_parts),
            anchors=anchors,
        )

    if original_binding is not True:
        return finish()
    if world_path is None or evidence_root is None:
        for values in parts.values():
            values.append(None)
        motion_parts.append(None)
        return finish()
    evidence_root, world_path = (
        p.resolve() if p.is_absolute() else repo_path(p) for p in (evidence_root, world_path)
    )
    # Plain JOIN has already verified the accepted pair/material/raw-file seals.
    checkpoints = [row for _, row in rows(evidence_root / "runtime/checkpoints.jsonl")]
    raw_record = checkpoints[1]
    source_ok = raw_record["kind"] == "r1:inherited-status-and-live-entities" and (
        raw_record["order"] == ref["inherited"]["source"]["order"]
        and ref["inherited"]["source"]["record"] == "prepared-68/runtime/checkpoints.jsonl:2"
    )
    for values in parts.values():
        values.extend((source_ok, None))
    motion_parts.extend((source_ok, None))
    if not world_path.is_file():
        return finish()
    try:
        selected = read(world_path)
        identity = selected["provenance"]
        source_ok = combined(
            [source_ok]
            + [
                None if identity.get(key) is None else identity[key] == expected
                for key, expected in (
                    ("commit", UPSTREAM),
                    ("romSha256", ROM),
                    ("repository", ACCEPTED_UPSTREAM_REPOSITORY),
                )
            ]
        )
        for values in parts.values():
            values.append(source_ok)
        motion_parts.append(source_ok)
        map3 = next(
            (m for m in selected.get("world", {}).get("maps", []) if m["id"] == "map-3"), {}
        )
        states = _bounded_list(s["state"] for s in actual["samples"])
        initial = states[0]
        admitted = actual.get("admissionSnapshot", {}).get("state", {})
        expected_actions = [
            dict(op="wait", ticks=30),
            dict(op="speed", x=0, y=0),
            dict(op="acceleration", x=1, y=1),
        ] + [dict(op="flags", field="a", mask=mask, value=mask) for mask in (3, 12, 128, 64, 32)]
        next_wait = 20  # Source wait20 follows waitDest, not a measured host duration.
        candidates = _bounded_list(
            (
                (i, s)
                for i, s in enumerate(states)
                if initial.get("simulationTick") is not None
                and s.get("simulationTick") is not None
                and 0 < s["simulationTick"] - initial["simulationTick"] < next_wait
            )
        )
        later = candidates[0] if candidates else None
        if later:
            sample_index, consumed = later
            reset_parts = [True if "warpRecords" in actual else None]
            for record in actual.get("warpRecords", []):
                try:
                    if record["result"]["revision"] > consumed["revision"]:
                        break
                    reset_parts.extend(
                        None if o.get("Kind") is None else o["Kind"] != "program-instruction"
                        for o in record["result"]["observations"]
                    )
                except KeyError:
                    reset_parts.append(None)
            # Missing reset evidence cannot hide an observed reinstall contradiction.
            no_reset = combined(reset_parts)
            delta = consumed["simulationTick"] - initial["simulationTick"]
            anchors["consumption"] = dict(
                sample=sample_index, logicalServices=delta, noProgramInstallation=no_reset
            )

        def movement(x, y, dx, dy, vx, vy, tx, ty, ax, ay, sx, sy, flags_a, flags_b):
            def sign(value):
                return None if value is None else (value > 0) - (value < 0)

            def difference(a, b):
                return None if a is None or b is None else a - b

            def tiles(value):
                return None if value is None else value / 384

            def bit(value, mask):
                return None if value is None else bool(value & mask)

            # Missing operands affect only their own normalized contribution.
            remaining = [difference(dx, x), difference(dy, y)]
            active = [None if d is None else d != 0 for d in remaining]
            return dict(
                activeAxes=active,
                direction=[sign(d) for d in remaining],
                velocityDirection=[
                    None if moving is None else sign(v) if moving else 0
                    for moving, v in zip(active, (vx, vy), strict=True)
                ],
                travelTiles=[tiles(tx), tiles(ty)],
                remainingTiles=[tiles(None if d is None else abs(d)) for d in remaining],
                accelerationSteps=[tiles(ax), tiles(ay)],
                configuredSpeed=[tiles(sx), tiles(sy)],
                acceleration=[bit(flags_a, 1), bit(flags_a, 2)],
                deceleration=[bit(flags_a, 4), bit(flags_a, 8)],
                obstructable=bit(flags_a, 128),
                mapCollision=bit(flags_a, 64),
                entityCollision=bit(flags_a, 32),
                autoFacing=bit(flags_b, 64),
            )

        def matches(expected, value):
            return None if value is None else expected == value

        for ordinal, (slot, character, center) in enumerate(
            ((5, 130, (20, 13, 3)), (6, 131, (18, 10, 1)), (8, 133, (12, 9, 1)))
        ):
            values = parts[slot]
            try:
                entity = next(e for e in raw_record["facts"]["entities"] if e["physical"] == slot)
                raw = bytes(entity["bytes"])

                def word(offset, signed=False, data=raw):
                    return int.from_bytes(data[offset : offset + 2], "big", signed=signed)

                pointer = int.from_bytes(raw[20:24], "big")
                base = 0xFF5600 + ordinal * 50
                offset = pointer - base
                projected = next(
                    (e for e in ref["inherited"].get("entities", []) if e["physical"] == slot), {}
                )
                bound_parts = [source_ok, len(raw) == 32, offset in (0, 40)]
                index_binding = None
                try:
                    index_binding = raw_record["facts"]["entityIndexBytes"][character - 96] == slot
                except (KeyError, IndexError):
                    index_binding = None
                bound_parts.append(index_binding)
                for key, value in (
                    ("actionScript", pointer),
                    ("waitTimer", raw[31]),
                    ("x", word(0)),
                    ("y", word(2)),
                    ("destinationX", word(12)),
                    ("destinationY", word(14)),
                ):
                    bound_parts.append(matches(value, projected.get(key)))
                values.extend(bound_parts)
                motion_parts.extend(bound_parts)
                content_ok = None
                try:
                    template = next(e for e in map3["entities"] if e["id"] == f"entity-{character}")
                    content_ok = template["actions"] == expected_actions + [
                        dict(op="random-walk", x=center[0], y=center[1], radius=center[2]),
                        dict(op="wait", ticks=next_wait),
                        dict(op="jump", instruction=8),
                    ]
                except (KeyError, StopIteration):
                    pass
                values[2] = content_ok
                motion_parts.append(content_ok)
                observed = next((e for e in initial.get("entities", []) if e["slot"] == slot), {})
                admission = next((e for e in admitted.get("entities", []) if e["slot"] == slot), {})
                cursor = 0 if offset == 0 else 9
                moving = (word(0), word(2)) != (word(12), word(14))
                for state in (observed, admission):
                    for key, value in (
                        ("id", f"entity-{character}"),
                        ("actionCursor", cursor),
                        ("moving", moving),
                    ):
                        values.append(matches(value, state.get(key)))
                expected = movement(
                    word(0),
                    word(2),
                    word(12),
                    word(14),
                    word(4, True),
                    word(6, True),
                    word(8),
                    word(10),
                    raw[24],
                    raw[25],
                    raw[26],
                    raw[27],
                    raw[28],
                    raw[29],
                )

                def actual_movement(e):
                    return movement(
                        *(
                            e.get(k)
                            for k in (
                                "x",
                                "y",
                                "targetX",
                                "targetY",
                                "velocityX",
                                "velocityY",
                                "travelX",
                                "travelY",
                                "accelerationX",
                                "accelerationY",
                                "speedX",
                                "speedY",
                            )
                        ),
                        int(e["flagsA"]) if e.get("flagsA") is not None else None,
                        int(e["flagsB"]) if e.get("flagsB") is not None else None,
                    )

                actual_motion = actual_movement(observed)
                for state_motion in (actual_motion, actual_movement(admission)):
                    for key, value in expected.items():
                        actual_value = state_motion[key]
                        if isinstance(value, list):
                            motion_parts.extend(
                                matches(v, a) for v, a in zip(value, actual_value, strict=True)
                            )
                        else:
                            motion_parts.append(matches(value, actual_value))
                anchors["slots"][slot] = dict(
                    base=base,
                    offset=offset,
                    character=character,
                    expectedCursor=cursor,
                    expectedMoving=moving,
                    expectedMotion=expected,
                    actualMotion=actual_motion,
                )
                gate_parts = [None]
                if later:
                    after = next((e for e in consumed.get("entities", []) if e["slot"] == slot), {})
                    gate_parts = [no_reset, matches(f"entity-{character}", after.get("id"))]
                    if offset == 0:
                        gate_parts.extend(
                            (
                                raw[31] >= 30,
                                None
                                if after.get("actionCursor") is None
                                else after["actionCursor"] in (8, 9),
                            )
                        )
                    else:
                        gate_parts.append(matches(cursor, after.get("actionCursor")))
                        gate_parts.append(matches(moving, after.get("moving")))
                        gate_parts.append(
                            matches(0 if moving else raw[31] + delta, after.get("waitTimer"))
                        )
                        if moving:
                            gate_parts.extend(
                                (
                                    matches(word(12), after.get("targetX")),
                                    matches(word(14), after.get("targetY")),
                                )
                            )
                            progress = None
                            try:
                                progress = abs(after["x"] - after["targetX"]) + abs(
                                    after["y"] - after["targetY"]
                                ) < abs(word(0) - word(12)) + abs(word(2) - word(14))
                            except KeyError:
                                progress = None
                            gate_parts.append(progress)
                gate = combined(gate_parts)
                if later:
                    anchors["slots"][slot]["consumedGate"] = gate
                values.append(gate)
                motion_parts.append(gate)
            except (KeyError, IndexError, StopIteration):
                values.append(None)
                motion_parts.append(None)
        motion_parts[2] = source_ok
    except (KeyError, IndexError, StopIteration):
        for values in parts.values():
            values.append(None)
        motion_parts.append(None)
    return finish()


def gameplay(s):
    actor_render = {"nodeX", "nodeY", "visible", "text", "sprite", "globalRect", "insideMap"}
    return {
        **{
            k: s.get(k)
            for k in (
                "sessionId",
                "revision",
                "mainSeed",
                "thinkingSeed",
                "gold",
                "queueCursor",
                "round",
                "actor",
                "target",
                "previewX",
                "previewY",
                "stage",
                "spell",
                "itemSlot",
                "inventories",
                "turnOrder",
                "storyFlags",
                "regionFlags",
                "aiMemory",
            )
        },
        "actors": [
            {k: v for k, v in a.items() if k not in actor_render} for a in s.get("actors", [])
        ],
    }


def _capture_outcome(actual, outcome_path):
    if "captureIntegrity" not in actual:
        return read(outcome_path)
    summaries = actual.get("outcomeSummary", [])
    require(len(summaries) == 1, "current capture missing/duplicate outcome summary")
    return dict(
        summaries[0],
        records=actual.get("outcomeRecords", []),
        endpoints=actual.get("outcomeEndpoints", []),
    )


def compare_modern(
    ref,
    actual_path,
    outcome_path,
    settings_path,
    host_log,
    host_exit,
    baseline_path=None,
    baseline_outcome=None,
    controlled_start_path=None,
    material_selection=None,
    original_join_evidence_root=None,
    text_source_root=None,
    canonical_content=None,
    tileset_metadata=None,
    palette_metadata=None,
    w2_context=None,
    heal_context=None,
    w1_context=None,
    map_context=None,
    admission_context=None,
    physical_context=None,
    reward_context=None,
    ai_context=None,
    field_context=None,
    scene_context=None,
    turn_context=None,
):
    actual = read(actual_path)
    outcome, settings = _capture_outcome(actual, outcome_path), read(settings_path)
    samples = actual.get("samples", [])
    require(samples, "modern actual lacks samples")
    records = actual.get("warpRecords", [])
    join = plain_join_binding(
        ref,
        actual,
        original_join_evidence_root,
        material_selection[0] if material_selection else None,
    )
    walking = walking_admission_binding(
        ref,
        actual,
        original_join_evidence_root,
        material_selection[0] if material_selection else None,
        join["original"],
    )
    field_motion = field_motion_binding(actual, material_selection, text_source_root)
    text_material = text_material_binding(actual, outcome, material_selection, text_source_root)
    operation_flow = operation_flow_binding(
        actual, material_selection, text_source_root, field_motion, text_material
    )
    audio_context = None
    if material_selection:
        try:
            world_path = material_selection[0]
            world_path = world_path if world_path.is_absolute() else repo_path(world_path)
            audio_context = _audio_context(read(world_path), actual)
        except (OSError, ValueError, KeyError):
            audio_context = None
    audio_consumers = audio_consumer_binding(actual, audio_context, text_source_root)
    w1_consumers = w1_consumer_binding(actual, w1_context, text_source_root)
    w2_consumers = w2_consumer_binding(actual, w2_context, text_source_root)
    heal_consumers = heal_consumer_binding(actual, heal_context, text_source_root)
    physical_consumers = (
        physical_consumer_binding(actual, physical_context, text_source_root)
        if physical_context is not None
        else None
    )
    ai_consumers = (
        ai_consumer_binding(actual, ai_context, text_source_root)
        if ai_context is not None
        else None
    )
    scene_consumers = (
        battle_scene_consumer_binding(actual, scene_context, text_source_root)
        if scene_context is not None
        else None
    )
    turn_consumers = (
        turn_order_consumer_binding(actual, turn_context, text_source_root)
        if turn_context is not None
        else None
    )
    field_consumers = (
        field_service_binding(actual, field_context, text_source_root)
        if field_context is not None
        else None
    )
    reward_consumers = (
        reward_consumer_binding(actual, reward_context, text_source_root)
        if reward_context is not None
        else None
    )
    map_consumers = (
        map_consumer_binding(actual, map_context, text_source_root)
        if map_context is not None
        else None
    )
    admission_seed = (
        admission_seed_binding(actual, admission_context, text_source_root)
        if admission_context is not None
        else None
    )
    admission_opening = (
        admission_opening_binding(actual, admission_context, text_source_root)
        if admission_context is not None and "opening" in admission_context
        else None
    )
    assertions = _bounded_list()
    obligations = {}

    def check(
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
        # Applicability precedes evaluation. Original diagnostics keep their raw mismatch.
        if value is None and applicability == "applicable":
            applicability = "required-unobserved"
            if reason == "semantic assertion":
                reason = "Actual field absent at " + location
        result = (
            "Unavailable"
            if value is None or applicability == "required-unobserved"
            else "PASS"
            if value == expected
            else "FAIL"
        )
        assertions.append(
            dict(
                layer=layer,
                assertion=name,
                applicability=applicability,
                original=original
                or dict(owner=OWNER, commit="87528953c6ee6d2631e24663a6d2e846be4c22ce"),
                actual=dict(
                    file=actual_file
                    or (
                        outcome_path
                        if location.startswith("outcome.")
                        else host_log
                        if location.startswith("host-log")
                        else actual_path
                    ).as_posix(),
                    record=location,
                ),
                expected=expected,
                actualValue=value,
                result=result,
                reason=reason,
                **({"parent": parent} if parent else {}),
                **({"missingSide": missing_side or "actual"} if result == "Unavailable" else {}),
            )
        )
        if parent:
            _group_rows(obligations, parent).append(assertions[-1])

    first = samples[0]["state"]
    admission = ref["admission"]
    player = endpoint_state(first)["player"]
    for name, expected, value in (
        ("map", f"map-{admission['state']['map']}", first.get("map")),
        ("x", admission["state"]["x"] * 384, player["x"]),
        ("y", admission["state"]["y"] * 384, player["y"]),
        ("facing", admission["state"]["facing"], player["facing"]),
        ("gold", admission["accounting"]["gold"], first.get("gold")),
        (
            "mainSeed",
            int.from_bytes(bytes(admission["state"]["rngBytes"]), "big"),
            first.get("mainSeed"),
        ),
    ):
        check(1, name, expected, value, "samples[0]", admission["source"])
    candidate = read(controlled_start_path) if controlled_start_path else None
    if candidate:
        require(
            candidate.get("formatVersion") == 1
            and candidate.get("profile") == "private-local-controlled-start",
            "not a controlled party definition input",
        )
        ids = [a["id"] for a in candidate["allies"]]
        require(len(ids) == len(set(ids)), "duplicate controlled party actor")
    candidate_allies = {a["id"]: a for a in candidate["allies"]} if candidate else {}
    startup = actual.get("admissionSnapshot", {})
    startup_state = startup.get("state", {})
    admitted = startup_state.get("admittedParty") or {}
    selected_encounters = [
        row
        for row in admitted.get("encounters", [])
        if row.get("encounter") == admitted.get("encounter")
    ]
    deployments = (
        selected_encounters[0].get("deployments", []) if len(selected_encounters) == 1 else []
    )
    admission_identities = []
    admitted_stats = []
    candidate_definitions = []
    for ally in admission["accounting"]["allies"][:3]:
        actor_id = f"ally-{ally['id']}"
        observed_inputs = [
            p for p in startup_state.get("party", []) if p["Actor"]["Value"] == actor_id
        ]
        bound = [d for d in deployments if d.get("actor") == actor_id]
        observed_input = observed_inputs[0] if len(observed_inputs) == 1 else None
        deployment = bound[0] if len(bound) == 1 else None
        definition = deployment.get("definition", {}) if deployment else {}
        carried = next(
            (p for p in first.get("party", []) if p["Actor"]["Value"] == f"ally-{ally['id']}"), {}
        )
        for source, field in (
            ("hpCurrent", "Hp"),
            ("mpCurrent", "Mp"),
            ("statusEffects", "Status"),
        ):
            check(
                1,
                f"ally-{ally['id']}.{field}",
                ally[source],
                carried.get(field),
                "samples[0].party",
                admission["source"],
            )
        parent = f"ally-{ally['id']}.items"
        explicit = carried.get("SourceLoadout")
        progress = carried.get("Progress")
        inherited = progress.get("SourceLoadout") if isinstance(progress, dict) else None
        loadout = explicit if explicit is not None else inherited
        selected = candidate_allies.get(ally["id"])
        identity = None
        if admitted:
            # Select the named encounter/deployment; ambiguity is not a first-match fallback.
            selected_definition = (
                dict(
                    classRule={
                        0: "UnpromotedSwordsman",
                        1: "UnpromotedKnight",
                        4: "UnpromotedPriest",
                    }.get(selected["classId"]),
                    level=selected["level"],
                    maxHp=selected["maxHp"],
                    maxMp=selected["maxMp"],
                    attack=selected["attack"],
                    defense=selected["defense"],
                    agility=selected["agility"] & 127,
                    extraRoundAction=bool(selected["agility"] & 128),
                    move=selected["move"],
                )
                if selected
                else None
            )
            input_fields = {
                "Hp": "hp",
                "Mp": "mp",
                "Exp": "exp",
                "Kills": "kills",
                "Defeats": "defeats",
                "Status": "status",
            }
            identity = bool(
                candidate
                and selected
                and observed_input
                and deployment
                and startup.get("inputOrdinal") == 0
                and startup_state.get("revision") is not None
                and 0 <= startup_state["revision"] <= first["revision"]
                and startup_state.get("sessionId") == first.get("sessionId")
                and admitted.get("package")
                and admitted.get("provenance")
                and admitted.get("origin") == candidate.get("profile")
                and admitted.get("encounter") == f"battle-{candidate['battle']}"
                and deployment.get("member") == ally["id"]
                and {k: definition.get(k) for k in selected_definition} == selected_definition
                and (definition.get("sourceLoadout") or {}).get("Items") == selected.get("items")
                and (definition.get("sourceLoadout") or {}).get("Spells") == selected.get("spells")
                and {k: observed_input.get(k) for k in input_fields}
                == {k: selected.get(v) for k, v in input_fields.items()}
                and observed_input == carried
            )
            if observed_input and deployment:
                explicit = observed_input.get("SourceLoadout")
                progress = observed_input.get("Progress")
                inherited = progress.get("SourceLoadout") if isinstance(progress, dict) else None
                loadout = (
                    explicit
                    if explicit is not None
                    else (inherited if inherited is not None else definition.get("sourceLoadout"))
                )
        admission_identities.append(identity)
        admitted_stats.append(definition if identity and progress is None else None)
        expected_items = [i["raw"] for i in ally["items"]]
        candidate_definitions.append(
            dict(
                actor=f"ally-{ally['id']}",
                file=controlled_start_path.as_posix() if controlled_start_path else None,
                record=f"allies[id={ally['id']}]",
                items=selected.get("items") if selected else None,
                actualOverride=explicit,
                actualProgress=progress,
                actual=dict(
                    file=actual_path.as_posix(),
                    record=f"admissionSnapshot.state.party[Actor={actor_id}]"
                    if admitted
                    else f"samples[0].party[Actor={actor_id}]",
                ),
                candidateEvidenceOwner=candidate.get("evidenceOwner") if candidate else None,
                effectiveAdmissionObserved=isinstance(loadout, dict) and "Items" in loadout,
                admittedDeployment=deployment,
                admittedDeploymentRecord=f"admissionSnapshot.state.admittedParty.encounters"
                f"[encounter={admitted.get('encounter')}].deployments[actor={actor_id}]"
                if admitted
                else None,
                sameSessionInputConsistency=identity,
                reason="Read-through admitted definition/operands at input ordinal0"
                if admitted
                else "Selected definition is a candidate input, not a same-run deployment snapshot",
            )
        )
        check(
            1,
            parent + ".effective four-slot words",
            expected_items,
            loadout.get("Items") if isinstance(loadout, dict) else None,
            "admissionSnapshot.state.party/admittedParty.encounters"
            if admitted
            else "samples[0].party.SourceLoadout",
            admission["source"],
            parent=parent,
            missing_side="actual deployment definition at admission",
            reason="Resolve explicit override, then Progress.SourceLoadout, then same-run "
            "deployment definition; absent definition cannot be replaced by later inventory",
        )
        check(
            1,
            parent + ".candidate definition slots",
            expected_items,
            selected.get("items") if selected else None,
            f"allies[id={ally['id']}].items",
            admission["source"],
            parent=parent,
            actual_file=controlled_start_path.as_posix() if controlled_start_path else None,
            missing_side="explicit controlled-start selection",
            reason="Candidate input comparison only; equality does not observe loaded "
            "admission slots",
        )
        check(
            1,
            parent + ".admission loadout identity",
            True,
            identity
            if admitted
            else (
                isinstance(loadout, dict) and "Items" in loadout if loadout is not None else None
            ),
            "admissionSnapshot.state.party/admittedParty.encounters"
            if admitted
            else "samples[0].party.SourceLoadout/Progress.SourceLoadout",
            dict(
                owner=OWNER,
                binding="R1 full item words; modern override/progress/definition precedence",
                modernOwner="remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs:83",
            ),
            parent=parent,
            missing_side="same-run loaded deployment definition identity",
            reason="Same-session pre-command admitted definition and party operands compared "
            "to the explicitly selected input; no instantiated battle is claimed at startup"
            if admitted
            else "Current launcher and selected input do not supply an omitted same-run definition",
        )
    check(
        1,
        "admission roster",
        admission["accounting"]["active"],
        (first.get("partyLists") or {}).get("Active"),
        "samples[0].partyLists",
        admission["source"],
    )

    phase_parent = "complete relevant admission phase/field mapping"
    for flag, expected_flag in admission["state"]["flags"].items():
        check(
            1,
            f"admission flag {flag}",
            expected_flag,
            int(flag) in first["flags"] if "flags" in first else None,
            "samples[0].flags",
            admission["source"],
            parent=phase_parent,
        )
    for source_key, actual_key in (("joined", "Joined"), ("active", "Active")):
        check(
            1,
            "admission " + source_key,
            admission["accounting"][source_key],
            (first.get("partyLists") or {}).get(actual_key),
            "samples[0].partyLists",
            admission["source"],
            parent=phase_parent,
        )
    readiness_fields = (
        "continuation",
        "cursor",
        "wait",
        "callers",
        "callerReturning",
        "warp",
        "choice",
        "battleMounted",
        "canWaitAtInput",
    )
    check(
        1,
        "admission logical consumer readiness",
        dict(
            continuation="FieldInput",
            cursor=None,
            wait=None,
            callers=[],
            callerReturning=None,
            warp=None,
            choice=None,
            battleMounted=False,
            canWaitAtInput=True,
        ),
        {k: first[k] for k in readiness_fields}
        if all(k in first for k in readiness_fields)
        else None,
        "samples[0].continuation/cursor/wait/callers/warp/choice",
        dict(
            **admission["source"],
            owner="docs/design/contracts/map3-controlled-admission.md",
            binding="zero pending returns/active consumers at the selected logical R1 seam",
        ),
        parent=phase_parent,
    )
    check(
        1,
        "admission seed-copy byte",
        True if admission_seed is not None else admission["state"]["rngCopyByte"],
        admission_seed["value"] if admission_seed is not None else first.get("randomSeedCopy"),
        "samples[0].randomSeedCopy",
        admission["source"],
        parent=phase_parent,
        reason="Selected non-resume write before readers plus accepted active-byte mechanism"
        if admission_seed is not None
        else "An absent latch is not zero; first-read/write relevance is not yet bound",
    )
    if admission_seed is not None:
        check(
            1,
            "historical A disconnected seed-copy latch",
            True,
            False,
            "historical text latch versus active ThinkingSeed",
            applicability="historical-diagnostic",
            parent=phase_parent,
            reason="Old null/latch/active12340000 remains a defect after the PR621 correction",
        )
    check(
        1,
        "opening mouth/view controls before first source write",
        True if admission_opening is not None else "original opening readback",
        admission_opening["value"]
        if admission_opening is not None
        else (first.get("textSettings") or {}),
        "samples[0].textSettings",
        dict(owner="docs/design/contracts/dialogue-system.md", binding="opening service gates"),
        applicability="applicable" if admission_opening is not None else "required-unobserved",
        parent=phase_parent,
        missing_side="selected original/actual admission evidence"
        if admission_opening is not None
        else "original",
        reason="Controlled original R1/readers bound to historical A's own initial settings"
        if admission_opening is not None
        else "Explicit controlled opening witness not selected",
    )
    # In this selected R1, zero script plus the 0x7000 sentinel denotes an unused physical slot.
    occupied = [
        e
        for e in ref["inherited"]["entities"]
        if not (e["actionScript"] == 0 and e["x"] == e["y"] == 0x7000)
    ]
    check(
        1,
        "admission occupied physical slots",
        [e["physical"] for e in occupied],
        _bounded_sorted(int(e["slot"]) for e in first["entities"]) if "entities" in first else None,
        "samples[0].entities[*].slot",
        ref["inherited"]["source"],
        parent=phase_parent,
    )
    entity_fields = {
        "x": "x",
        "y": "y",
        "targetX": "destinationX",
        "targetY": "destinationY",
        "facing": "facing",
        "layer": "layer",
    }
    for entity in occupied:
        modern = next((e for e in first.get("entities", []) if e["slot"] == entity["physical"]), {})
        check(
            1,
            f"admission slot {entity['physical']} position/destination/facing/layer",
            {k: entity[v] for k, v in entity_fields.items()},
            {k: modern[k] for k in entity_fields}
            if all(k in modern for k in entity_fields)
            else None,
            f"samples[0].entities[slot={entity['physical']}]",
            ref["inherited"]["source"],
            parent=phase_parent,
        )
    for slot in (5, 6, 8):
        modern = next((e for e in first.get("entities", []) if e["slot"] == slot), {})
        original_entity = next(e for e in ref["inherited"]["entities"] if e["physical"] == slot)
        check(
            1,
            f"admission walking slot {slot} wait timer",
            original_entity["waitTimer"],
            modern.get("waitTimer"),
            f"samples[0].entities[slot={slot}].waitTimer",
            dict(
                **ref["inherited"]["source"],
                binding=f"inherited.entities[physical={slot}].waitTimer",
            ),
            parent=phase_parent,
            reason="Direct original readback; no remake start supplies the expected timer",
        )
        check(
            1,
            f"admission walking slot {slot} cursor/moving source binding",
            True,
            walking["slots"][slot],
            f"samples[0].entities[slot={slot}].actionCursor/moving",
            dict(
                **ref["inherited"]["source"],
                binding=f"inherited.entities[physical={slot}].actionScript",
                actionScript=original_entity["actionScript"],
                admissionBinding=walking["anchors"],
            ),
            parent=phase_parent,
            missing_side="selected source/content/actual phase or consumed gate",
            reason="Pinned allocator/template and raw R1 identity/pointer translate the phase; "
            "actual admission plus blocked/unblocked continuation consume it. "
            "Hidden gate is Inferred",
        )
    for ally in admission["accounting"]["allies"][:3]:
        selected = candidate_allies.get(ally["id"])
        fields = {
            "classId": "class",
            "level": "level",
            "maxHp": "hpMax",
            "maxMp": "mpMax",
            "attack": "attack",
            "defense": "defense",
            "agility": "agility",
            "move": "move",
        }
        expected_definition = {k: ally[v] for k, v in fields.items()}
        expected_definition["spells"] = [s["raw"] for s in ally["spells"]]
        check(
            1,
            f"ally-{ally['id']} candidate class/stats/spell words",
            expected_definition,
            {k: selected.get(k) for k in expected_definition} if selected else None,
            f"allies[id={ally['id']}]",
            admission["source"],
            parent=phase_parent,
            actual_file=controlled_start_path.as_posix() if controlled_start_path else None,
            missing_side="explicit controlled-start selection",
            reason="Candidate definition only; this does not observe effective admission "
            "stats/spells",
        )
    check(
        1,
        "effective admission class/level/maxima/stats/spells definition identity",
        True,
        (False if not all(admission_identities) else True if all(admitted_stats) else None)
        if admitted
        else None,
        "admissionSnapshot.state.admittedParty.encounters/party",
        admission["source"],
        parent=phase_parent,
        missing_side="actual",
        reason="Same-session admitted class/stats/spell words match the selected input, whose "
        "original comparison is independent; non-null stat Progress needs effective stat binding",
    )
    check(
        1,
        "walking motion gate/velocity/travel/flags correspondence",
        True,
        walking["motion"],
        "samples[0].entities[slot=5/6/8]",
        dict(
            **ref["inherited"]["source"],
            owner="docs/research/map3-messenger-acceptance.md",
            admissionBinding=walking["anchors"],
        ),
        parent=phase_parent,
        missing_side="selected source/content/actual movement or consumed gate",
        reason="Source consumer normalization preserves direction, carried travel, acceleration/"
        "deceleration and collision/auto-facing gates; "
        "no hardware velocity magnitude/frame equality",
    )

    events = _bounded_list(
        e
        for r in records
        if r["result"]["boundary"] == "submit"
        for e in r["result"]["observations"]
    )
    session = first.get("sessionId")
    check(
        2,
        "one continuous session",
        True,
        bool(session)
        and all(r["result"]["sessionId"] == session for r in records)
        and outcome["final"]["sessionId"] == session,
        "warpRecords[*].result/sessionId",
    )
    sequences = _bounded_list(r["result"]["observationSequence"] for r in records)
    check(
        2,
        "monotonic observed sequence",
        True,
        all(x <= y for x, y in zip(sequences, sequences[1:], strict=False)),
        "warpRecords[*].result.observationSequence",
    )
    check(
        2,
        "full result stream",
        True,
        bool(records) and any(r.get("projection") == "field-view-pending" for r in records),
        "warpRecords[*]",
    )
    check(
        3,
        "ordinary route and probe assertions",
        True,
        actual.get("passed") and not actual.get("failures") and not actual.get("unavailable"),
        "passed/failures/unavailable",
    )
    required = [
        "parallax-field-return",
        "before-battle-text2293-input",
        "tracking-text2294-input",
        "tracking-text2295-input",
        "tracking-text2296-input",
        "white-chester2297-input",
        "bound-first-battle-input",
        "bound-first-action-choice",
        "bound-first-movement-cancel",
    ]
    labels = {s["label"]: s["state"] for s in samples if s["label"] in required}
    check(
        3,
        "mandatory reached checkpoints",
        True,
        all(k in labels for k in required),
        "samples[*].label",
    )
    if baseline_path is not None:
        baseline = {
            s["label"]: s["state"] for s in read(baseline_path)["samples"] if s["label"] in required
        }
        fields = (
            "simulationTick",
            "mainSeed",
            "randomSeedCopy",
            "continuation",
            "wait",
            "token",
            "cursor",
            "callers",
            "enteringBattle",
            "entities",
            "party",
            "flags",
            "gold",
            "logicalText",
            "logicalView",
            "textSettings",
            "fieldText",
            "portraitWork",
            "display",
            "round",
            "actor",
            "stage",
            "storyFlags",
        )
        for label in required:
            expected = {k: baseline.get(label, {}).get(k) for k in fields}
            observed = {k: labels.get(label, {}).get(k) for k in fields}
            check(
                3,
                "accepted modern baseline:" + label,
                expected,
                paired_baseline_value(expected, observed),
                "samples:" + label,
                dict(
                    owner="PR588 ordinary-normal-05",
                    commit="87528953c6ee6d2631e24663a6d2e846be4c22ce",
                    file=baseline_path.as_posix(),
                    record="samples:" + label,
                ),
            )
    battle = labels.get("bound-first-battle-input", {})
    check(
        4,
        "natural battle first control",
        ["Movement", "PlayerInput", 1],
        [battle.get("stage"), battle.get("stopReason"), battle.get("round")],
        "samples:bound-first-battle-input",
    )
    turn = ref["turns"][0]
    check(
        4,
        "original first order",
        turn["accounting"]["turnOrder"],
        [
            dict(actor=source_actor(x["actor"]), score=x["score"])
            for x in battle.get("turnOrder", [])
            if x["actor"]
        ],
        "samples:bound-first-battle-input",
        turn["source"],
        "historical-diagnostic",
        "Selected original history under declared modern finite-music policy; "
        "mismatch cause is not attributed",
    )
    check(
        4,
        "original first actor",
        turn["actor"],
        source_actor(battle.get("actor")),
        "samples:bound-first-battle-input",
        turn["source"],
        "historical-diagnostic",
        "Original trajectory, not a modern legality constant",
    )

    # Keep all browsing failures; only the accepted candidate rejection has a bounded allowance.
    previous = None
    rejections = _bounded_list()
    for index, row in enumerate(records):
        state, result = row.get("state", {}), row["result"]
        failure = result.get("failure")
        if failure:
            atomic = previous is not None and gameplay(state) == gameplay(previous)
            living = [
                a["id"]
                for a in state.get("actors", [])
                if a["id"].startswith("enemy-") and a["hp"] > 0
            ]
            candidate = previous.get("candidate") if previous else None
            next_candidate = (
                living[(living.index(candidate) + 1) % len(living)]
                if candidate in living
                else living[0]
                if living
                else None
            )
            permitted = (
                # SessionFailureKind.IllegalCommand is enum0 in the actual result payload.
                failure.get("Kind") == 0
                and failure.get("Code") == "target-range"
                and failure.get("Field") == "target"
                and state.get("candidate") == next_candidate
            )
            later = next(
                (
                    r
                    for r in records[index + 1 :]
                    if r.get("state", {}).get("stage") != "TargetChoice"
                    or r.get("state", {}).get("target") is not None
                ),
                None,
            )
            legal = (
                later is not None
                and later["result"].get("failure") is None
                and later.get("state", {}).get("target") is not None
            )
            confirmed = next(
                (
                    r
                    for r in records[index + 1 :]
                    if any(
                        e["Kind"] == "scene-prepared"
                        or e["Kind"] == "battle-movement-segment-started"
                        and e["Detail"] == "Action"
                        for e in r["result"]["observations"]
                    )
                ),
                None,
            )
            legal = legal and confirmed is not None and confirmed["result"].get("failure") is None
            check(
                5,
                f"rejection[{index}] atomic/accepted later target",
                True,
                permitted and atomic and not result["observations"] and legal,
                f"warpRecords[{index}]",
            )
            rejections.append(
                dict(index=index, code=failure.get("Code"), atomic=atomic, legalLaterTarget=legal)
            )
        if "stage" in state:
            previous = state
    check(
        5,
        "adaptive actions and consumed outcome",
        True,
        bool(
            _bounded_list(
                r for r in outcome.get("records", []) if r.get("label") == "action-selected"
            )
        )
        and outcome.get("issue") == "",
        "outcome.records/issue",
    )
    ordered = _bounded_list(
        e["Kind"]
        for e in events
        if e["Kind"]
        in ("after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned")
    )
    check(
        6,
        "whole after/return order",
        ["after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned"],
        ordered,
        "warpRecords[*].result.observations",
    )
    final = outcome["final"]
    check(
        6,
        "victory flags/program/return",
        True,
        501 in final.get("flags", [])
        and 401 not in final.get("flags", [])
        and "abcs-battle01" in outcome.get("programs", [])
        and outcome.get("heldBattle")
        and final.get("map") == "map-57",
        "outcome.final/programs",
    )
    check(
        7,
        "accepted original Down extension selection",
        True,
        True if ref.get("postVictoryInput") else None,
        "reference.postVictoryInput",
        reason="Separate original extension required; absent selection is not absent host data",
    )
    check(
        7,
        "returned actual field input",
        True,
        final.get("canWaitAtInput")
        and final.get("wait") is None
        and final.get("cursor") is None
        and not final.get("battleMounted")
        and final.get("failure") is None,
        "outcome.final",
    )
    endpoints = outcome.get("endpoints", [])
    paired = {}
    for label in ("first-return", "before-down", "after-down"):
        pair = [e for e in endpoints if e["label"] == label]
        if len(pair) == 2:
            states = [endpoint_state(e["state"]) for e in pair]
            ready = all(
                s["canWaitAtInput"]
                and s["wait"] is None
                and s["cursor"] is None
                and not s["callers"]
                and s["callerReturning"] is None
                and s["warp"] is None
                and not s["battleMounted"]
                and s["choice"] is None
                and s["tickDebt"] == 0
                and not s["player"]["moving"]
                and not s["player"]["busy"]
                for s in states
            )
            check(
                7,
                label + " two settled host updates",
                True,
                pair[0]["hostUpdate"] < pair[1]["hostUpdate"] and states[0] == states[1] and ready,
                f"outcome.endpoints:{label}",
            )
            paired[label] = states[1]
        else:
            check(
                7,
                label + " two settled host updates",
                True,
                None,
                f"outcome.endpoints:{label}",
                applicability="required-unobserved",
                reason="Actual independent host update records absent",
            )
    if "before-down" in paired and "after-down" in paired:
        before, after = paired["before-down"]["player"], paired["after-down"]["player"]
        check(
            7,
            "ordinary Down displacement",
            [before["x"], before["y"] + 384, 3],
            [after["x"], after["y"], after["facing"]],
            "outcome.endpoints:after-down",
            (ref.get("postVictoryInput") or {}).get("input"),
        )
        if baseline_outcome is not None:
            check(
                7,
                "accepted modern Left endpoint",
                endpoint_state(read(baseline_outcome)["final"]),
                paired["before-down"],
                "outcome.endpoints:before-down",
                dict(
                    owner="PR588 ordinary-normal-05",
                    commit="87528953c6ee6d2631e24663a6d2e846be4c22ce",
                    file=baseline_outcome.as_posix(),
                    record="final",
                ),
            )
    else:
        check(
            7,
            "ordinary Down displacement",
            "accepted local Down rule",
            None,
            "outcome.endpoints",
            applicability="required-unobserved",
            reason="Left evidence does not supply the named Down boundary",
        )
    expected = ref["endpoint"]
    returned = next((e["state"] for e in endpoints if e["label"] == "first-return"), None)
    returned_player = endpoint_state(returned)["player"] if returned else None
    for field, original, value in (
        ("gold", expected["accounting"]["gold"], returned.get("gold") if returned else None),
        (
            "position",
            [expected["state"]["x"] * 384, expected["state"]["y"] * 384],
            [returned_player["x"], returned_player["y"]] if returned_player else None,
        ),
    ):
        check(
            7,
            "original endpoint " + field,
            original,
            value,
            "outcome.endpoints:first-return",
            expected["source"],
            "historical-diagnostic",
            "Original terminal trajectory remains evidence, not a modern endpoint constant",
        )
    check(
        9,
        "clean actual process",
        True,
        host_exit == 0
        and not any(
            k in host_log.read_text(encoding="utf-8")
            for k in ("ERROR:", "SCRIPT ERROR", "Exception")
        ),
        "host-log/recorded-exit",
    )
    input_rows = actual.get("inputRecords", [])
    check(
        10,
        "actual physical/consumer input records",
        True,
        bool(input_rows) or None,
        "inputRecords",
        applicability="applicable" if input_rows else "required-unobserved",
        reason="Input ordinals are observed, not inferred from snapshots",
    )
    variant = actual.get("h4Variant")
    kinds = {r["delivery"]["kind"] for r in input_rows if r["pressed"]}
    if variant in ("A", "B", "C", "D"):
        check(
            10,
            "configured device and release observations",
            True,
            ({"axis", "button"}.issubset(kinds) if variant in "BD" else kinds == {"key"})
            and all(
                any(
                    not release["pressed"]
                    and release["action"] == press["action"]
                    and release["delivery"]["kind"] == press["delivery"]["kind"]
                    and release["delivery"]["code"] == press["delivery"]["code"]
                    for release in input_rows[index + 1 :]
                )
                for index, press in enumerate(input_rows)
                if press["pressed"]
            ),
            "inputRecords[*].delivery/ordinal",
        )

    def missing(layer, parent, name, binding, actual_location, side, reason, observed=None):
        check(
            layer,
            name,
            binding,
            observed,
            actual_location,
            dict(owner=OWNER, sourceCommit=SOURCE, upstream=UPSTREAM, binding=binding),
            applicability="required-unobserved",
            parent=parent,
            missing_side=side,
            reason=reason,
        )

    operation_parent = "complete mandatory operation-to-consumption mapping"
    for flag, expected_flag in ref["endpoint"]["state"]["flags"].items():
        check(
            3,
            f"returned story flag {flag}",
            expected_flag,
            int(flag) in final["flags"] if "flags" in final else None,
            "outcome.final.flags",
            ref["endpoint"]["source"],
            parent=operation_parent,
        )
    check(
        3,
        "after-program join/flag/return causal order",
        ["after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned"],
        ordered,
        "warpRecords[*].result.observations",
        dict(
            owner="docs/research/map3-battle01-victory-return.md",
            binding="accepted after-program shared tail before flag writes and enclosing return",
        ),
        parent=operation_parent,
    )
    for name, binding, actual_location in (
        (
            "taken route/setup/caller branch operands and occurrence",
            "operationPairs.entry/return + program/operation.pc/opcode + route/story branch "
            "operands",
            "warpRecords[*].result.observations.Program/Detail",
        ),
        (
            "awaited entity motion/gesture/fade before caller return",
            "story operation subject/destination/wait + operationPairs return",
            "warpRecords[*].state.entities/callers/presentation",
        ),
        (
            "dialogue speaker/control-token occurrence and choice effect",
            "story displayed text/speaker + choices + ordered W1/W2 tokens",
            "samples[*].textId/speaker/fieldText + inputRecords",
        ),
        (
            "route roster/flag writes at their source branch",
            "story joined/follower and F600..608/F66/F89 changes before operation return",
            "warpRecords[*].state.partyLists/flags",
        ),
        (
            "warp destination/setup initialization before field release",
            "route warp/setup operands and pending consumer return",
            "warpRecords[*].state.warp/loadServices/callers",
        ),
        (
            "before/after operation effects and shared-tail return pairing",
            "operationPairs for reached bbcs_01/abcs_battle01, including awaited effects and "
            "shared tail",
            "warpRecords[*].result.observations.Program + outcome.records/programs",
        ),
    ):
        if name == "awaited entity motion/gesture/fade before caller return":
            check(
                3,
                name,
                True,
                field_motion["operation"],
                actual_location,
                dict(
                    owner="docs/design/contracts/map-exploration.md",
                    upstreamCommit=UPSTREAM,
                    binding="complete ordered source producers, wait predicates "
                    "and signed command operands",
                ),
                parent=operation_parent,
                reason="Occurrence-local source/wait/caller join; false dominates missing operands",
            )
            continue
        if name in operation_flow["values"]:
            check(
                3,
                name,
                True,
                operation_flow["values"][name],
                actual_location,
                dict(
                    owner="docs/design/contracts/map-exploration.md",
                    upstreamCommit=UPSTREAM,
                    binding=binding,
                ),
                parent=operation_parent,
                reason=(
                    "Complete source body, evaluated operands and occurrence-local effects; "
                    "false dominates missing"
                ),
            )
            continue
        missing(
            3,
            operation_parent,
            name,
            binding,
            actual_location,
            "original/actual semantic join",
            "Source operation PC/operands and typed instruction/consumer occurrence are not "
            "joined; instruction presence or total count cannot establish this edge",
        )

    rule_parent = "matched-state rule/RNG/draw-to-effect comparisons"
    unique_events = _occurrence_map()
    sequence_consistent = True
    for event in events:
        sequence = event["Sequence"]
        if sequence in unique_events and unique_events[sequence] != event:
            sequence_consistent = False
        unique_events[sequence] = event
    check(
        5,
        "repeated logical sequence identifies the same observation",
        True,
        sequence_consistent,
        "warpRecords[*].result.observations.Sequence",
        parent=rule_parent,
        reason="Repeated snapshots cannot duplicate or alter a consumed draw/effect",
    )
    # Reuse the original H3 model, not the remake RNG. Its fixed-range model is sufficient
    # for these recorded ranges; preserve the full image's unmodified low word separately.
    draws = _bounded_list(e for e in unique_events.values() if e["Kind"].startswith("rng-"))
    for event in draws:
        operands = [event.get(k) for k in ("Before", "After", "RandomRange", "RandomValue")]
        if all(v is not None for v in operands):
            before, after, range_, value = map(int, operands)
            word, result = _rng_step(before >> 16, ((range_ * 2) & 65535))
            expected_draw = dict(after=(word << 16) | (before & 65535), value=result >> 1)
            actual_draw = dict(after=after, value=value)
        else:
            expected_draw, actual_draw = "word update, preserved low word, range/result", None
        check(
            5,
            f"main draw {int(event['Sequence'])} {event['Kind']}",
            expected_draw,
            actual_draw,
            f"warpRecords.observations[Sequence={int(event['Sequence'])}]",
            dict(
                owner="docs/design/contracts/randomness.md",
                fixture="tests/fixtures/h3/rng-v1.json",
                upstream=UPSTREAM,
                binding="GenerateRandomNumber word update and doubled-range upper product",
            ),
            parent=rule_parent,
            reason="Independent accepted original generator model at the actual seed/range; "
            "this does not yet associate the draw with a particular source effect",
        )
    check(
        5,
        "recorded main draw operands are available",
        True,
        bool(draws) or None,
        "warpRecords.observations: rng-*",
        parent=rule_parent,
    )
    for name, binding, actual_location, side in (
        (
            "turn candidate score draws and tie/order result",
            "source turn-order eligibility/score/stable signed ordering at matched live operands",
            "round-rng.TurnGeneration + independent live candidate roster/queue",
            "actual generation/state operands; controlled proof cannot backfill historical A",
        ),
        (
            "physical range/dodge/critical/spread/double/counter effects",
            "scenes.effects.attackType + source action rules at matched "
            "stats/status/equipment/seed",
            "rng-dodge/critical/spread/double/counter + physical-first/second/hp",
            "matched original operands and draw-to-effect join",
        ),
        (
            "HEAL recovery/cost/fairy opportunity and seed effects",
            "scenes.effects + source HEAL MP/recovery and fairy caller/gate rules",
            "spell-selected/mp/heal/rng-fairy-* + scene.healing",
            "original caller opportunities and matched recovery/draw effects",
        ),
        (
            "healing item slot words/removal and resource effects",
            "decisions UseItem zero-based slot/live word + scenes consumed item/HP boundary",
            "itemSlot/inventories + action-committed/scene-ended",
            "matched source item action/resource occurrence",
        ),
        (
            "EXP/gold/growth/spell learning and after-turn/outcome effects",
            "scenes.after accounting + source reward/growth/status and victory rules",
            "exp/gold/level-*/after-turn/battle-outcome + party.Progress",
            "matched source preconditions and draw/effect association",
        ),
        (
            "AI thinking draw/choice/memory and movement decision",
            "StartAiControl/ExecuteAiControl serialized actor memory + source thinking-byte "
            "contract",
            "thinking-rng/ai-memory/ai-target/ai-move + thinkingSeed",
            "original thinking draw stream and decoded matched AI memory",
        ),
        (
            "field text/portrait/NPC service draw-to-effect gates",
            "story W1/W2/portrait/entity caller state plus RNG consumerScopes",
            "rng-text-*/rng-portrait-* + fieldText/portraitWork/entities",
            "source live service gates and individual caller/effect mapping",
        ),
    ):
        if name == "turn candidate score draws and tie/order result" and turn_consumers is not None:
            check(
                5,
                name,
                True,
                turn_consumers["value"],
                actual_location,
                original=turn_consumers["sourceRules"],
                parent=rule_parent,
                reason="Accepted actual generation/source rules plus independently complete "
                "retained queue consumers at supplied seams; historical generation and corrected "
                "whole-A trajectory remain unavailable",
            )
            continue
        if (
            name == "physical range/dodge/critical/spread/double/counter effects"
            and physical_consumers is not None
        ):
            check(
                5,
                name,
                True,
                physical_consumers["value"],
                actual_location,
                original=physical_consumers["sourceRules"],
                parent=rule_parent,
                reason="Source physical rules, matched operands, complete selected census and "
                "persistent scene effects; historical trajectory remains separate",
            )
            continue
        if name == "HEAL recovery/cost/fairy opportunity and seed effects":
            check(
                5,
                name,
                True,
                heal_consumers["value"],
                actual_location,
                original=heal_consumers["sourceRules"],
                parent=rule_parent,
                reason="Source matched HEAL1 effects and actual logical opportunities; "
                "prepared04 CPU-timing/cursor failures remain historical, not a third-tick quota",
            )
            continue
        if (
            name == "EXP/gold/growth/spell learning and after-turn/outcome effects"
            and reward_consumers is not None
        ):
            check(
                5,
                name,
                True,
                reward_consumers["value"],
                actual_location,
                original=reward_consumers["sourceRules"],
                parent=rule_parent,
                reason="Source reward/growth rules at matched action operands, ordered lifecycle "
                "and composed first Party.Progress; no immediate live agility/base-attack claim",
            )
            continue
        if (
            name == "AI thinking draw/choice/memory and movement decision"
            and ai_consumers is not None
        ):
            check(
                5,
                name,
                True,
                ai_consumers["value"],
                actual_location,
                original=dict(owner="docs/design/contracts/battle-ai-decision.md", source=UPSTREAM),
                parent=rule_parent,
                reason="Original rules at matched historical callers and logical consumers; "
                "accepted current seed mechanism is a separate composition dependency; "
                "historical latch remains FAIL",
            )
            continue
        if (
            name == "field text/portrait/NPC service draw-to-effect gates"
            and field_consumers is not None
        ):
            check(
                5,
                name,
                True,
                field_consumers["value"],
                actual_location,
                original=field_consumers["sourceRules"],
                parent=rule_parent,
                reason=(
                    "Original rules, accepted executed dependencies and bounded current "
                    "input/state/Draw effects; historical stripped reads and intermediate "
                    "NPC attempts remain separate"
                ),
            )
            continue
        item_reached = any(e["Kind"] == "item-consumed" for e in unique_events.values()) or any(
            row["state"].get("itemSlot") is not None
            for row in outcome.get("records", [])
            if row.get("label") == "action-selected"
        )
        if name == "healing item slot words/removal and resource effects" and not item_reached:
            check(
                5,
                name,
                binding,
                None,
                actual_location,
                dict(owner=OWNER, binding="original UseItem history in decisions/scenes"),
                applicability="historical-diagnostic",
                parent=rule_parent,
                reason="No item action is reached in this actual winning profile; retain original "
                "UseItem history without imposing an unvisited consuming branch",
            )
            continue
        missing(
            5,
            rule_parent,
            name,
            binding,
            actual_location,
            side,
            "Compare the reached rule at matched operands; whole historical actor/round "
            "sequence and settings equality cannot supply this binding",
        )

    resource_parent = "complete reached 7C resource/provenance inventory"
    scene_rows = actual.get("sceneObservations", [])
    mounted_scenes = _bounded_list(
        r for r in scene_rows if r["scene"].get("visible") and not r["scene"].get("fieldDeath")
    )
    mounted_resources = _bounded_sorted(
        {
            r["scene"][name]["resource"]
            for r in mounted_scenes
            for name in ("background", "backgroundWrap", "ground")
            if r["scene"].get(name, {}).get("visible") and r["scene"][name].get("resource")
        }
    )
    if scene_rows:
        check(
            8,
            "actual mounted scene background/ground identities",
            True,
            bool(mounted_scenes)
            and all(
                r["scene"].get(name, {}).get("visible")
                for r in mounted_scenes
                for name in ("background", "backgroundWrap")
            )
            and all(
                node.get("resource") and node.get("texturePresent")
                for r in mounted_scenes
                for name in ("background", "backgroundWrap", "ground")
                if (node := r["scene"].get(name, {})).get("visible")
            ),
            "sceneObservations[*].scene.background/backgroundWrap/ground",
            parent=resource_parent,
            reason="IDs read from successfully bound visible nodes in this session; "
            "hidden metadata is not consumption",
        )
    receipt_rows = actual.get("audioReceipts", [])
    audio_terminal = actual.get("audioTerminal", {})
    audio_receipts = _bounded_list(r["receipt"] for r in receipt_rows)
    audio_contiguous = None
    audio_balances = Counter()
    audio_lifecycle = {}
    if audio_terminal:
        audio_contiguous = bool(
            not actual.get("audioReceiptGaps")
            and _bounded_list(r["Sequence"] for r in audio_receipts)
            == _bounded_list(range(1, int(audio_terminal["sequence"]) + 1))
            and actual.get("audioSequenceSeen") == audio_terminal["sequence"]
            and audio_terminal.get("error") is None
            and all(r["poll"]["sessionId"] == first.get("sessionId") for r in receipt_rows)
        )
        operations = Counter(r["Operation"] for r in audio_receipts)
        audio_ordered = True
        for receipt in audio_receipts:
            if receipt["Operation"] == "started":
                audio_balances[receipt["Cue"]] += 1
            elif receipt["Operation"] in ("finished", "stopped"):
                audio_balances[receipt["Cue"]] -= 1
                audio_ordered = audio_ordered and audio_balances[receipt["Cue"]] >= 0
        terminal_voices = Counter(s["cue"] for s in audio_terminal.get("sounds", []))
        if audio_terminal.get("musicPlaying"):
            terminal_voices[audio_terminal["musicCue"]] += 1
        audio_lifecycle = dict(
            operations=dict(operations),
            outstandingByCue=dict(+audio_balances),
            terminalVoices=dict(terminal_voices),
            terminalMusicPlaying=audio_terminal.get("musicPlaying"),
            reason="Cue totals classify starts/stops/finishes; wait tokens are not playback IDs. "
            "Same-cue overlapping voices are not assigned invented generations",
        )
        check(
            8,
            "actual audio receipt stream continuity",
            True,
            audio_contiguous,
            "audioReceipts/audioReceiptGaps/audioSequenceSeen/audioTerminal",
            parent=resource_parent,
            applicability="required-unobserved" if actual.get("audioReceiptGaps") else "applicable",
            missing_side="actual missing receipt ranges",
            reason="First sequence1 through terminal sequence; "
            "missing ranges remain observation failures",
        )
        check(
            8,
            "actual audio lifecycle balance",
            True,
            audio_ordered
            and +audio_balances == terminal_voices
            and not (set(operations) - {"started", "stopped", "finished", "fade-command"}),
            "audioReceipts[*].receipt.Operation/Cue + audioTerminal",
            parent=resource_parent,
            applicability="required-unobserved" if actual.get("audioReceiptGaps") else "applicable",
            missing_side="actual lifecycle across missed receipt ranges",
            reason="Actual cue totals reconcile with terminal voices; "
            "ongoing field music requires no fabricated end",
        )
    materials = reached_materials(
        actual,
        material_selection,
        text_source_root,
        canonical_content,
        tileset_metadata,
        palette_metadata,
        map_consumers,
    )
    for row in materials["checks"]:
        check(
            8,
            row["name"],
            True,
            row["value"],
            "explicit selected material inputs",
            original=dict(owner=OWNER, binding=row["source"]),
            parent=resource_parent,
            reason="Offline original material identity; "
            "natural dispatch and consumer remain separate",
        )
    if materials["actorWeapon"] is not None:
        check(
            8,
            "reached actor/weapon material subset",
            True,
            materials["actorWeapon"],
            "sceneObservations[*].scene.allyResource/enemyResource/weaponResource",
            parent=resource_parent,
            reason="Base42 original extraction; missing healing/death use keeps full family open",
        )
    for name, binding, actual_location, side in (
        (
            "reached map3/19/20/21/40/57 atlas and layer identities",
            "route/setup map identity -> admitted original map source/extractor -> actual "
            "atlas use",
            "samples[*].map/cameraProjection",
            "selected original asset/provenance to actual mount join",
        ),
        (
            "reached entity sprites/portraits/gesture resource identities",
            "story subject/sprite/portrait/gesture -> ROM/source selection -> actual node resource",
            "samples[*].entities/cameraProjection.actors/portraitProjection/presentation",
            "per-occurrence source asset and actual resource correspondence",
        ),
        (
            "displayed text tokens/font/glyph private binding",
            "story displayed text ID and control tokens -> admitted original text/font -> "
            "Label use",
            "samples[*].textId/fieldText/dialogue + scene.message",
            "private text/font provenance to actual displayed occurrence",
        ),
        (
            "scene actor/weapon/healing/death resources",
            "scenes actor/action/target selectors -> accepted scene source extraction -> "
            "mounted resource",
            "warpRecords[*].state.scene.allyResource/enemyResource/weaponResource/healing/fieldDeath",
            "healing/death bound raster identities and complete per-resource use/provenance join",
        ),
        (
            "scene background/ground actual resource identity",
            "original scene extraction -> selected raster -> mounted resource identity",
            "sceneObservations[*].scene.background/backgroundWrap/ground"
            if scene_rows
            else "warpRecords[*].state.scene.background/ground resource identity",
            "original selector/provenance to actual mounted ID join"
            if scene_rows
            else "actual resource ID (projection records positions but omits these IDs)",
        ),
        (
            "reached audio command/timer/PCM provenance and playback lifecycle",
            "pinned capture/cut/loop -> selected original PCM -> actual start/stop/finish",
            "audioReceipts/audioTerminal"
            if audio_terminal
            else "samples[*].audio.receipts + speechReceipts",
            "explicit selected world/library/capture-cut provenance join"
            if audio_contiguous
            else "complete reached audio/provenance join and exhaustive required playback receipts",
        ),
    ):
        material = None
        if name == "reached map3/19/20/21/40/57 atlas and layer identities":
            material = materials["visuals"]["map"]
        elif name == "reached entity sprites/portraits/gesture resource identities":
            material = materials["visuals"]["entity"]
        elif name == "scene actor/weapon/healing/death resources":
            values = (materials["actorWeapon"], materials["visuals"]["scene"])
            material = False if False in values else None if None in values else True
        elif name == "displayed text tokens/font/glyph private binding":
            material = text_material["value"]
        elif name == "scene background/ground actual resource identity":
            material = materials["scene"]
        elif name == "reached audio command/timer/PCM provenance and playback lifecycle":
            material = (
                materials["audio"]
                and audio_contiguous
                and audio_ordered
                and +audio_balances == terminal_voices
            )
        if material is not None:
            check(
                8,
                name,
                True,
                material,
                actual_location,
                parent=resource_parent,
                original=dict(owner=OWNER, binding=binding),
                reason="Reached original material joins with actual use; natural original "
                "dispatch/mailbox/wait remains operation/consumer work",
            )
            continue
        missing(
            8,
            resource_parent,
            name,
            binding,
            actual_location,
            side,
            "Only reached resources are required; no all-corpus/frame inventory or hardware "
            "equality. Current selected content is not by itself an actual consumption record",
            observed=mounted_resources
            if scene_rows and name == "scene background/ground actual resource identity"
            else dict(contiguous=audio_contiguous, lifecycle=audio_lifecycle)
            if audio_terminal
            and name == "reached audio command/timer/PCM provenance and playback lifecycle"
            else text_material
            if name == "displayed text tokens/font/glyph private binding"
            else None,
        )

    consumer_parent = "required unshimmed ack and scene consumer binding"
    for name, value, parent, layer in (
        ("bounded JOIN original witness binding", join["original"], consumer_parent, 9),
        ("bounded JOIN finite playback and previous restart", join["audio"], consumer_parent, 9),
        ("bounded JOIN dependent caller return", join["caller"], operation_parent, 7),
    ):
        check(
            layer,
            name,
            True,
            value,
            "samples/inputRecords/warpRecords/audioReceipts",
            original=dict(owner=OWNER, upstream=UPSTREAM, binding=join["anchors"]),
            parent=parent,
            reason="Only the sealed JOIN occurrence; missing selected evidence "
            "is unavailable. Original music completion remains Unknown",
        )
    for name, binding, actual_location, side, reason in (
        (
            "W1 displayed token occurrence/accepting read/service gates",
            "named text483 DisplayText -> symbol_wait1 -> loc_65B4 -> text return",
            "inputRecords Confirm/Wait + fieldText + text-w1-*",
            "remaining original occurrence/service-gate to actual consumer joins",
            "The accepted text483 witness proves that bounded seam; RTS/cursor alone cannot "
            "identify every required natural occurrence",
        ),
        (
            "W2 accepting read/validation indicator and token return",
            "source loc_6472 draw/copy/wait/read then sub_64A8 validation/clear -> token resume",
            "inputRecords + text-w2-* + audio validation receipt",
            "selected source/caller/actual occurrence and validation/indicator binding",
            "PR618 composed acceptance; original internal read/time remains Unknown",
        ),
        (
            "plain JOIN input after matching finite completion",
            "natural WaitForPlayerInput input-first seam after declared modern music completion",
            "music-actual-completed/music-wait-returned + inputRecords + presentation-acknowledged",
            "named original plain-input occurrence to actual completion/ack join",
            "Plain input is separate from W1/W2; modern finite music is an accepted deviation",
        ),
        (
            "entity motion/gesture/fade consumer start/completion before resume",
            "reached story subject/wait -> real node use/completion -> dependent caller/input "
            "release",
            "samples[*].presentation/entities + warpRecords.result/state",
            "per-occurrence original wait dependency and actual completion identity",
            "A culled subject or request/return counter alone does not prove required delivery",
        ),
        (
            "battle scene command/resources/wait/effect/end consumer edges",
            "Initialize/Execute/End plus source command rules -> mounted action/target "
            "resources -> "
            "each required completion/ack -> committed effect -> field input",
            "scene phase/waitToken/resources/completed + scene-step-*/scene-ended + actors",
            "source dynamic command/operand mapping and actual per-occurrence completion joins",
            "Existing phase snapshots and prepare/end pairs do not alone bind every required "
            "animation/message/resource edge; no original frame/pixel equality is required",
        ),
        (
            "audio replacement/fade/stop/resume dependent consumer edges",
            "source reached audio command/timer and wait/replacement rule -> real player "
            "generation "
            "completion/stop -> matching release",
            "audio receipts/musicWait/token + inputRecords/warpRecords",
            "source wait dependencies and complete actual playback/generation correspondence",
            "Original mailbox dispatch is not playback; persistent music needs no invented end "
            "event",
        ),
    ):
        if (
            name == "battle scene command/resources/wait/effect/end consumer edges"
            and scene_consumers is not None
        ):
            check(
                9,
                name,
                True,
                scene_consumers["value"],
                actual_location,
                original=scene_consumers["sourceRules"],
                parent=consumer_parent,
                reason="Selected source command/resources and actual phase completion/input/effect "
                "joins; terminal no-visual-effect composition retains Inferred completed and "
                "Unknown delay. Historical A failures remain.",
            )
            continue
        if name == "W1 displayed token occurrence/accepting read/service gates":
            check(
                9,
                name,
                True,
                w1_consumers["value"],
                actual_location,
                original=w1_consumers["sourceRules"],
                parent=consumer_parent,
                reason="Pinned source plus selected actual poll/copy/service/continuation binding; "
                "original live service timing and intermediate removed clocks remain Unknown",
            )
            continue
        if name == "W2 accepting read/validation indicator and token return":
            check(
                9,
                name,
                True,
                w2_consumers["value"],
                actual_location,
                original=w2_consumers["sourceRules"],
                parent=consumer_parent,
                reason="Accepted source/caller/actual composed proof; original internal read and "
                "exact intermediate host timing remain Unknown",
            )
            continue
        if (
            name == "plain JOIN input after matching finite completion"
            and join["plain"] is not None
        ):
            check(
                9,
                name,
                True,
                join["plain"],
                actual_location,
                original=dict(owner=OWNER, upstream=UPSTREAM, binding=join["anchors"]),
                parent=consumer_parent,
                reason=reason,
            )
            continue
        if name == "entity motion/gesture/fade consumer start/completion before resume":
            check(
                9,
                name,
                True,
                field_motion["consumer"],
                actual_location,
                dict(
                    owner="remake/docs/presentation-and-assets.md",
                    upstreamCommit=UPSTREAM,
                    binding="source occurrence -> real phase/draw/modulation -> completion "
                    "handoff -> logical restore/resume",
                ),
                parent=consumer_parent,
                reason="Complete reached inventory; geometric culling and actual finite handoffs, "
                "without per-tick/terminal draw quotas",
            )
            continue
        if name == "audio replacement/fade/stop/resume dependent consumer edges":
            check(
                9,
                name,
                True,
                audio_consumers["value"],
                actual_location,
                original=dict(
                    owner=OWNER, upstream=UPSTREAM, binding=audio_consumers["sourceRules"]
                ),
                parent=consumer_parent,
                reason="Independent accepted rules plus complete selected actual playback and "
                "matching release inventory; original completion/hardware time remain Unknown",
            )
            continue
        missing(9, consumer_parent, name, binding, actual_location, side, reason)

    check(
        10,
        MATRIX_OBLIGATION,
        "actual declared-scope matrix report",
        None,
        "coverage",
        missing_side="separate actual matrix report",
        reason="The matrix command closes only the declared current keyboard scope; "
        "historical device reports and supplemental settings do not expand it",
    )
    check(
        10,
        "modern finite-music deviation declaration",
        "accepted-modern-finite-music-clock",
        "accepted-modern-finite-music-clock",
        "profile",
        reason="ADR0010; downstream historical mismatches retain unresolved attribution",
    )

    causal_records = all("resultStart" in r and "resultEnd" in r for r in input_rows) and all(
        "inputDelivery" in r for r in records
    )
    check(
        10,
        "observed input dispatch intervals",
        True,
        causal_records or None,
        "inputRecords[*]/warpRecords[*].inputDelivery",
        reason="Only synchronous dispatch results establish physical input consumption",
    )
    inputs = (
        _bounded_list(
            dict(
                action=r["action"],
                wait=r["before"].get("wait"),
                cursor=r["before"].get("cursor"),
                actor=r["before"].get("actor"),
            )
            for r in input_rows
            if r["pressed"]
            and any(
                x.get("inputDelivery")
                and x.get("inputOrdinal") == r["ordinal"]
                and x["result"]["boundary"] == "submit"
                for x in records[r.get("resultStart", 0) : r.get("resultEnd", 0)]
            )
        )
        if causal_records
        else None
    )
    reveal_inputs = _bounded_list(
        r
        for r in input_rows
        if r["pressed"]
        and r["action"] == "confirm"
        and any(
            r["before"].get(visible, -1) >= 0 and r["before"][visible] < r["before"].get(total, 0)
            for visible, total in (
                ("visibleCharacters", "totalCharacters"),
                ("sceneVisibleCharacters", "sceneTotalCharacters"),
            )
        )
    )
    if variant == "C":
        check(
            10,
            "actual reveal-only Confirm",
            True,
            bool(reveal_inputs)
            and all(
                r["resultStart"] == r["resultEnd"]
                and all(
                    r["before"].get(k) == r["after"].get(k)
                    for k in (
                        "simulationTick",
                        "mainSeed",
                        "thinkingSeed",
                        "actor",
                        "stage",
                        "wait",
                        "cursor",
                    )
                )
                and all(
                    r["after"].get(visible, -1) < 0
                    or r["after"][visible] >= r["after"].get(total, 0)
                    for visible, total in (
                        ("visibleCharacters", "totalCharacters"),
                        ("sceneVisibleCharacters", "sceneTotalCharacters"),
                    )
                )
                for r in reveal_inputs
            )
            if causal_records
            else None,
            "inputRecords:incomplete-text-confirm",
            reason="Actual incomplete Label becomes ready without submit or logical service",
        )
    elif variant == "D":
        check(
            10,
            "natural reveal before Confirm",
            True,
            not reveal_inputs if causal_records else None,
            "inputRecords:confirm.before",
            reason="Actual field/scene Label ready before every Confirm; no reveal-only press",
        )
    # Delivery notifications may interleave with mandatory work as reveal speed changes.
    # Keep logical operands ordered; retain and independently check host-only deliveries.
    semantic, deliveries, delivery_checks = _bounded_list(), _bounded_list(), _bounded_list()
    previous = None
    for index, row in enumerate(records):
        result, state = row["result"], row.get("state", {})
        observed = result.get("observations", [])
        if result["boundary"] == "submit":
            pure_scene = (
                len(observed) == 1
                and observed[0]["Kind"] == "scene-delivery"
                and row.get("inputDelivery") is False
            )
            for event in observed:
                if event["Kind"] == "text-revealed" or pure_scene:
                    unchanged = previous is not None and all(
                        previous.get(k) == state.get(k)
                        for k in ("simulationTick", "mainSeed", "thinkingSeed", "gold")
                    )
                    if pure_scene:
                        unchanged = unchanged and all(
                            v == gameplay(previous).get(k)
                            for k, v in gameplay(state).items()
                            if k not in ("revision", "sessionId")
                        )
                    delivery_checks.append(unchanged)
                    deliveries.append(dict(record=index, observation=event, unchanged=unchanged))
                else:
                    semantic.append(semantic_value(event))
        if state:
            previous = state
    check(
        10,
        "host delivery adds no gameplay or RNG service",
        True,
        bool(delivery_checks) and all(delivery_checks),
        "warpRecords:delivery-notifications",
        reason="Only text-revealed and single automatic scene-delivery notifications; "
        "acknowledgements and scene continuation remain ordered logical observations",
    )
    battle_states = _bounded_list(
        {k: v for k, v in gameplay(row["state"]).items() if k not in ("sessionId", "revision")}
        for row in outcome.get("records", [])
        if row.get("label") == "action-selected"
    )
    coverage = _bounded_list()
    for name, children in obligations.items():
        required_children = _bounded_list(
            c for c in children if c["applicability"] != "historical-diagnostic"
        )
        child_counts = dict(Counter(child["result"] for child in required_children))
        parent_result = verdict(child_counts)
        coverage.append(
            dict(
                layer=children[0]["layer"],
                assertion=name,
                applicability="required-unobserved"
                if parent_result == "Unavailable"
                else "applicable",
                original=dict(
                    owner=OWNER, binding="required reached winning-profile child obligations"
                ),
                actual=dict(file=actual_path.as_posix(), record="assertions[parent=" + name + "]"),
                expected="all applicable required children PASS",
                actualValue=child_counts,
                result=parent_result,
                reason="Closed required child set is owned by the continuous contract; "
                "candidate input equality cannot substitute for missing actual/source bindings",
                children=[child["assertion"] for child in required_children],
                historicalChildren=[
                    child["assertion"]
                    for child in children
                    if child["applicability"] == "historical-diagnostic"
                ],
            )
        )
    required_rows = _bounded_list(
        a for a in assertions if a["applicability"] != "historical-diagnostic"
    )
    counts = dict(Counter(a["result"] for a in required_rows))
    result = verdict(counts)
    report = dict(
        profile="modern-continuous",
        variant=actual.get("h4Variant"),
        sourceCommit=SOURCE,
        originalIdentity=dict(
            rom=ref["romSha256"],
            upstream=ref["upstream"],
            extension=(ref.get("postVictoryInput") or {}).get("sourceCommit"),
        ),
        evidence=dict(
            canonicalContent=canonical_content.as_posix() if canonical_content else None,
            tilesetMetadata=tileset_metadata.as_posix() if tileset_metadata else None,
            paletteMetadata=palette_metadata.as_posix() if palette_metadata else None,
            originalJoinEvidenceRoot=(
                original_join_evidence_root.as_posix() if original_join_evidence_root else None
            ),
            materialSelection=(
                dict(
                    zip(
                        (
                            "world",
                            "scene",
                            "processReceipt",
                            "sceneEvidenceRoot",
                            "assetRoot",
                            "assetCommit",
                            "assetTree",
                            "assetManifestSha256",
                        ),
                        (v.as_posix() if isinstance(v, Path) else v for v in material_selection),
                        strict=True,
                    )
                )
                if material_selection
                else None
            ),
            actual=actual_path.as_posix(),
            outcome=outcome_path.as_posix(),
            settings=settings,
            hostExit=host_exit,
        ),
        assertions=assertions,
        coverageObligations=coverage,
        candidateDefinitions=candidate_definitions,
        actualObservations=dict(
            admissionSeed=admission_seed,
            admissionOpening=admission_opening,
            admissionInputConsistency=admission_identities,
            sceneRecords=len(scene_rows),
            visibleMountedResources=mounted_resources,
            completedSceneTokens=_bounded_sorted(
                _occurrence_set(
                    r["scene"]["waitToken"]
                    for r in scene_rows
                    if r["scene"].get("completed") and r["scene"].get("waitToken") is not None
                )
            ),
            audioReceiptCount=len(audio_receipts),
            audioContiguous=audio_contiguous,
            audioLifecycle=audio_lifecycle,
            audioConsumerBinding=audio_consumers,
            w1ConsumerBinding=w1_consumers,
            w2ConsumerBinding=w2_consumers,
            healConsumerBinding=heal_consumers,
            physicalConsumerBinding=physical_consumers,
            rewardConsumerBinding=reward_consumers,
            turnOrderConsumerBinding=turn_consumers,
            fieldServiceBinding=field_consumers,
            battleSceneConsumerBinding=scene_consumers,
            reachedMaterialJoins=materials["joins"],
            reachedVisualMaterialBinding=materials["visuals"],
            textMaterialBinding=text_material,
            plainJoinBinding=join,
            walkingAdmissionBinding=walking,
            fieldMotionBinding=field_motion,
            operationFlowBinding=operation_flow,
        ),
        counts=counts,
        historicalCounts=dict(
            Counter(
                a["result"] for a in assertions if a["applicability"] == "historical-diagnostic"
            )
        ),
        result=result,
        milestonePass=result == "PASS",
        rejections=rejections,
        deliveryNotifications=deliveries,
        equivalence=dict(
            admission={
                k: first.get(k) for k in ("map", "party", "partyLists", "flags", "gold", "mainSeed")
            },
            inputs=inputs,
            observations=semantic,
            battleStates=battle_states,
            endpoints=paired,
            party=final.get("party"),
            gold=final.get("gold"),
            mainSeed=final.get("mainSeed"),
        ),
    )

    errors = modern_report_integrity(report, ref)
    require(not errors, "; ".join(errors))
    return report


def _matrix_join_occurrence(report, ref, completion_index, crossed_indices):
    """Reproduce occurrence evidence from actual files; report PASS flags are not proof."""
    proof = dict(value=None)
    if modern_report_integrity(report, ref):
        return dict(value=False, reason="report integrity")
    identity = report.get("originalIdentity", {})
    if not isinstance(identity, dict):
        return dict(value=False, reason="malformed source identity")
    expected_identity = dict(
        rom=ref["romSha256"],
        upstream=ref["upstream"],
        extension=(ref.get("postVictoryInput") or {}).get("sourceCommit"),
    )
    identity_missing = "sourceCommit" not in report or any(
        k not in identity for k in expected_identity
    )
    if (
        "sourceCommit" in report
        and report["sourceCommit"] != SOURCE
        or any(k in identity and identity[k] != v for k, v in expected_identity.items())
    ):
        return dict(value=False, reason="wrong source identity")
    try:
        evidence = report["evidence"]
        selection = evidence.get("materialSelection")
        if not selection or not evidence.get("actual") or not evidence.get("outcome"):
            return proof
        actual_path, outcome_path = (
            Path(evidence[k]).resolve()
            if Path(evidence[k]).is_absolute()
            else repo_path(evidence[k]).resolve()
            for k in ("actual", "outcome")
        )
        if not all(
            p.is_relative_to(repo_path("local").resolve()) for p in (actual_path, outcome_path)
        ):
            return dict(value=False, reason="actual outside owned evidence")
        actual = read(actual_path)
        outcome = _capture_outcome(actual, outcome_path)
        if actual.get("h4Variant") != report["variant"] or actual.get("passed") is not True:
            return dict(value=False, reason="wrong or failed actual capture")
        records = actual["warpRecords"]
        events = _bounded_list(
            o
            for row in records
            if row["result"]["boundary"] == "submit"
            for o in row["result"]["observations"]
            if o["Kind"] != "text-revealed"
            and not (
                len(row["result"]["observations"]) == 1
                and o["Kind"] == "scene-delivery"
                and row.get("inputDelivery") is False
            )
        )
        inputs = _bounded_list(
            dict(
                action=r["action"],
                wait=r["before"].get("wait"),
                cursor=r["before"].get("cursor"),
                actor=r["before"].get("actor"),
            )
            for r in actual["inputRecords"]
            if r["pressed"]
            and any(
                x.get("inputDelivery")
                and x.get("inputOrdinal") == r["ordinal"]
                and x["result"]["boundary"] == "submit"
                for x in records[r["resultStart"] : r["resultEnd"]]
            )
        )
        first, final = actual["samples"][0]["state"], outcome["final"]
        paired = {}
        for label in ("first-return", "before-down", "after-down"):
            pair = _bounded_list(e for e in outcome.get("endpoints", []) if e["label"] == label)
            if len(pair) == 2:
                paired[label] = endpoint_state(pair[1]["state"])
        derived = dict(
            admission={
                k: first.get(k) for k in ("map", "party", "partyLists", "flags", "gold", "mainSeed")
            },
            inputs=inputs,
            observations=_bounded_list(semantic_value(o) for o in events),
            battleStates=_bounded_list(
                {
                    k: v
                    for k, v in gameplay(row["state"]).items()
                    if k not in ("sessionId", "revision")
                }
                for row in outcome.get("records", [])
                if row.get("label") == "action-selected"
            ),
            endpoints=paired,
            party=final.get("party"),
            gold=final.get("gold"),
            mainSeed=final.get("mainSeed"),
        )
        if derived != report["equivalence"]:
            return dict(value=False, reason="report does not reproduce actual equivalence")
        selected_world = Path(selection["world"])
        world = (
            selected_world.resolve() if selected_world.is_absolute() else repo_path(selected_world)
        )
        binding = plain_join_binding(
            ref,
            actual,
            Path(evidence["originalJoinEvidenceRoot"])
            if evidence.get("originalJoinEvidenceRoot")
            else None,
            world,
        )
        material_selection = tuple(
            Path(selection[k])
            for k in ("world", "scene", "processReceipt", "sceneEvidenceRoot", "assetRoot")
        ) + tuple(selection[k] for k in ("assetCommit", "assetTree", "assetManifestSha256"))
        source_audio = reached_materials(actual, material_selection)["audio"]
        if source_audio is False:
            return dict(value=False, reason="independent source audio binding")
        values = [binding[k] for k in ("original", "plain", "audio", "caller")] + [source_audio]
        if False in values:
            return dict(value=False, reason="contradictory actual JOIN gates or caller")
        if None in values:
            return proof
        anchor = binding["anchors"]["actual"]
        generation, token = anchor["generation"], anchor["helperToken"]
        completed = events[completion_index]
        release = next(
            o for o in events if o["Kind"] == "music-wait-returned" and o["Detail"] == "MUSIC_JOIN"
        )
        eligible = next(
            o
            for o in events
            if o["Kind"] == "music-previous-eligible" and o["Detail"] == "MUSIC_JOIN"
        )
        if not (
            generation < completed["Sequence"] < release["Sequence"]
            and all(
                generation < events[i]["Sequence"] < eligible["Sequence"] for i in crossed_indices
            )
        ):
            return dict(value=False, reason="completion crosses a dependent gate")
        held = _bounded_list(
            row["state"]
            for row in records
            if row.get("state", {}).get("wait") == "MusicWait"
            and row["state"].get("token") == token
        )
        if not held or any(
            not any(
                events[i] in row["result"]["observations"]
                and row.get("state", {}).get("wait") == "MusicWait"
                and row["state"].get("token") == token
                and row["state"]["revision"] >= events[i]["Revision"]
                and row["state"]["sessionId"] == held[0]["sessionId"]
                for row in records
            )
            for i in crossed_indices
        ):
            return dict(value=False, reason="crossed service is outside the actual helper")
        receipts = _bounded_list(
            r["receipt"]
            for r in actual["audioReceipts"]
            if r["receipt"]["Cue"] == "MUSIC_JOIN"
            and r["receipt"]["Operation"] in ("started", "finished")
        )
        return dict(
            value=None if identity_missing else True,
            sessionId=held[0]["sessionId"],
            generation=generation,
            helperToken=token,
            receiptSequences=[r["Sequence"] for r in receipts],
            completionSequence=completed["Sequence"],
            eligibleSequence=eligible["Sequence"],
            releaseSequence=release["Sequence"],
            crossedSequences=[events[i]["Sequence"] for i in crossed_indices],
        )
    except FileNotFoundError:
        return proof
    except (KeyError, IndexError, StopIteration):
        return proof
    except (ValueError, TypeError, AttributeError):
        return dict(value=False, reason="malformed occurrence evidence")


def bounded_join_correspondence(base, other, ref):
    """Pair one independently proved completion; retain both original ordered streams."""
    result = dict(value=False)
    a, b = base["equivalence"], other["equivalence"]
    if any(a[k] != b.get(k) for k in a if k != "observations"):
        return dict(value=False, reason="other unequal equivalence component")
    x, y = a["observations"], b["observations"]
    found = [
        [
            i
            for i, o in enumerate(stream)
            if o.get("Kind") == "music-actual-completed" and o.get("Detail") == "MUSIC_JOIN"
        ]
        for stream in (x, y)
    ]
    if len(x) != len(y) or any(len(v) != 1 for v in found):
        return dict(value=False, reason="missing, duplicate or unequal completion inventory")
    ai, bi = found[0][0], found[1][0]
    lo, hi = min(ai, bi), max(ai, bi)
    if ai == bi or x[ai] != y[bi] or x[:lo] != y[:lo] or x[hi + 1 :] != y[hi + 1 :]:
        return dict(value=False, reason="unequal payload or another moved observation")
    ax, bx = ([i for i in range(lo, hi + 1) if i != complete] for complete in (ai, bi))
    if any(x[i] != y[j] for i, j in zip(ax, bx, strict=True)) or any(
        x[i].get("Kind") not in ("music-step", "music-helper-service")
        or x[i].get("Detail") != ("MUSIC_JOIN" if x[i]["Kind"] == "music-step" else None)
        for i in ax
    ):
        return dict(value=False, reason="completion crosses an unallocated operation")
    proofs = [
        _matrix_join_occurrence(base, ref, ai, ax),
        _matrix_join_occurrence(other, ref, bi, bx),
    ]
    values = [p["value"] for p in proofs]
    result.update(
        value=False if False in values else None if None in values else True,
        completionIndices=dict(base=ai, other=bi),
        crossedCorrespondence=_bounded_list(zip(ax, bx, strict=True)),
        occurrences=proofs,
        rule="same JOIN occurrence music-step/music-helper-service only",
    )
    return result


def compare_matrix(paths, ref, *, scope="current-keyboard"):
    require(scope == "current-keyboard", "unsupported matrix scope")
    reports = [read(p) for p in paths]
    variants = [r.get("variant") for r in reports]
    require(len(variants) == len(set(variants)), "duplicate matrix variant")
    required = ("A",)
    supplemental = ("C",) if "C" in variants else ()
    excluded = ("B", "D")
    require(
        all(
            r.get("profile") == "modern-continuous"
            and r.get("variant") in ("A", "B", "C", "D")
            and r.get("comparisonScope", "full") == "full"
            for r in reports
        ),
        "matrix requires named modern reports",
    )
    checks = _bounded_list()
    base = next((r for r in reports if r.get("variant") == "A"), None)
    for name in required + supplemental:
        report = next((r for r in reports if r.get("variant") == name), None)
        settings = report["evidence"]["settings"] if report else {}
        expected = dict(
            confirmCancel="swapped" if name in "CD" else "standard",
            textMode="adjustable" if name in "CD" else "instant",
            reducedFlash=name in "CD",
            charactersPerSecond=20 if name in "CD" else 40,
        )
        observed = {k: settings.get(k) for k in expected}
        differences = []
        if report and base:
            for field, value in base["equivalence"].items():
                other = report["equivalence"].get(field)
                if value == other:
                    continue
                difference = dict(field=field)
                if isinstance(value, (list, _RecordSpool)) and isinstance(
                    other, (list, _RecordSpool)
                ):
                    index = next(
                        (
                            i
                            for i, pair in enumerate(zip(value, other, strict=False))
                            if pair[0] != pair[1]
                        ),
                        min(len(value), len(other)),
                    )
                    difference.update(
                        index=index,
                        expected=value[index] if index < len(value) else None,
                        actual=other[index] if index < len(other) else None,
                        expectedCount=len(value),
                        actualCount=len(other),
                    )
                else:
                    difference.update(expected=value, actual=other)
                differences.append(difference)
        causal = None
        if report and base and differences:
            causal = bounded_join_correspondence(base, report, ref)
        raw_equal = (
            report is not None and base is not None and report["equivalence"] == base["equivalence"]
        )
        contradiction = (
            report is not None
            and base is not None
            and (
                observed != expected
                or causal is not None
                and causal["value"] is False
                or bool(modern_report_integrity(report, ref))
                or bool(modern_report_integrity(base, ref))
            )
        )
        result = (
            "FAIL"
            if contradiction
            else "Unavailable"
            if report is None
            or base is None
            or report["equivalence"].get("inputs") is None
            or base["equivalence"].get("inputs") is None
            else "PASS"
            if raw_equal or causal and causal["value"] is True
            else "Unavailable"
            if causal and causal["value"] is None
            else "FAIL"
        )
        checks.append(
            dict(
                variant=name,
                report=next(
                    (
                        p.as_posix()
                        for p, r in zip(paths, reports, strict=True)
                        if r.get("variant") == name
                    ),
                    None,
                ),
                result=result,
                expectedSettings=expected,
                actualSettings=observed,
                differences=differences,
                rawOrderEqual=raw_equal,
                causalCorrespondence=causal,
                reason="Declared keyboard scope requires A; C is supplemental. "
                "Settings/occurrence equality does not close remaining H4 obligations",
            )
        )
    required_checks = [c for c in checks if c["variant"] in required]
    required_reports = [r for r in reports if r["variant"] in required]
    supplemental_results = []
    for check in checks:
        if check["variant"] not in supplemental:
            continue
        report = next(r for r in reports if r["variant"] == check["variant"])
        errors = modern_report_integrity(report, ref)
        supplemental_results.append(
            dict(
                check,
                comparisonResult=check["result"],
                result=verdict(Counter((check["result"], report["result"]))),
                reportResult=report["result"],
                integrityErrors=errors,
                coverageObligations=report.get("coverageObligations", []),
                remainingAssertions=[
                    a
                    for a in report["assertions"]
                    if a["applicability"] != "historical-diagnostic" and a["result"] != "PASS"
                ],
                milestoneApplicable=False,
            )
        )
    counts = dict(Counter(c["result"] for c in required_checks))
    integrity_errors = [modern_report_integrity(r, ref) for r in required_reports]
    inherited_fail = any(r.get("result") == "FAIL" for r in required_reports) or any(
        integrity_errors
    )
    matrix_complete = not counts.get("FAIL") and not counts.get("Unavailable")
    remaining = [
        dict(
            variant=r["variant"],
            coverageObligations=r.get("coverageObligations", []),
            assertions=[
                a
                for a in r["assertions"]
                if a["applicability"] != "historical-diagnostic"
                and a["result"] != "PASS"
                and not (matrix_complete and a["assertion"] == MATRIX_OBLIGATION)
            ],
            **({"integrityErrors": errors} if errors else {}),
        )
        for r, errors in zip(required_reports, integrity_errors, strict=True)
    ]
    incomplete = any(r["assertions"] for r in remaining) or any(
        p["result"] != "PASS" for r in required_reports for p in r.get("coverageObligations", [])
    )
    result = (
        "FAIL"
        if counts.get("FAIL") or inherited_fail
        else "Unavailable"
        if counts.get("Unavailable") or incomplete
        else "PASS"
    )
    return dict(
        profile="modern-continuous-matrix",
        scope=scope,
        requiredVariants=list(required),
        excludedVariants=list(excluded),
        supplementalVariants=list(supplemental),
        variants=required_checks,
        supplemental=supplemental_results,
        excludedReports=[
            dict(
                variant=r["variant"],
                report=p.as_posix(),
                result=r.get("result"),
                counts=r.get("counts"),
                reason="Historical device report outside current scope",
            )
            for p, r in zip(paths, reports, strict=True)
            if r["variant"] in excluded
        ],
        counts=counts,
        result=result,
        milestonePass=result == "PASS",
        remaining=remaining,
        requiredReports=[
            dict(variant=r["variant"], result=r["result"], counts=r["counts"])
            for r in required_reports
        ],
    )


def compare_resources(
    actual_path,
    selection,
    source_root,
    canonical_content,
    tileset_metadata,
    palette_metadata,
    scope,
    budget,
    *,
    source_only=False,
    map_context=None,
):
    actual_path = (
        Path(actual_path).resolve() if Path(actual_path).is_absolute() else repo_path(actual_path)
    )
    budget.checkpoint(
        "source-only preparation" if source_only else "capture integrity scan", force=True
    )
    if source_only:
        binding = reached_visual_materials(
            {},
            selection,
            source_root,
            canonical_content,
            tileset_metadata,
            palette_metadata,
            source_only=True,
            budget=budget,
        )
        require(binding.get("prepared"), "source-only preparation failed")
        return dict(
            profile="modern-resource-source-only",
            comparisonScope=scope,
            sourceOnlyPrivateBytes=binding["sourceOnlyPrivateBytes"],
            sourceOnly=True,
            milestonePass=False,
        )
    with actual_path.open("rb") as stream:
        header = json.loads(stream.readline(_STREAM_RECORD_LIMIT + 1))
    require(header.get("channel") == "header", "scoped comparison requires sealed JSONL capture")
    actual = _read_capture(actual_path, scope, budget)
    map_binding = (
        map_consumer_binding(actual, map_context, source_root) if map_context is not None else None
    )
    budget.checkpoint("source preparation and independent inventory", force=True)
    binding = reached_visual_materials(
        actual,
        selection,
        source_root,
        canonical_content,
        tileset_metadata,
        palette_metadata,
        budget=budget,
        map_binding=map_binding,
    )
    selected = ("map", "entity", "scene") if scope["family"] == "all" else (scope["family"],)
    values = [binding[family] for family in selected]
    outcome = "FAIL" if False in values else "Unavailable" if None in values else "PASS"
    budget.checkpoint("compact publication preparation", force=True)
    return dict(
        profile="modern-resource-scope",
        comparisonScope=actual["resourceScope"],
        result=outcome,
        milestonePass=False,
        fullH4Evidence=False,
        actualObservations=dict(reachedVisualMaterialBinding=binding),
        captureIntegrity=actual["captureIntegrity"],
        provenance=dict(
            capture=str(actual_path.relative_to(repo_path(".")))
            if actual_path.is_relative_to(repo_path("."))
            else str(actual_path),
            rawCaptureEmbedded=False,
        ),
        resources=budget.receipt(),
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "mode",
        choices=(
            "plan",
            "compare",
            "matrix",
            "resources",
            "audio",
            "turn-order",
            "w1",
            "w2",
            "heal",
            "map",
            "admission-seed",
            "physical",
            "reward",
            "ai",
            "field-service",
            "battle-scene",
        ),
    )
    parser.add_argument(
        "--scene-context",
        type=Path,
        help="Retained modern scene selection and accepted compact dependencies",
    )
    parser.add_argument(
        "--turn-context",
        type=Path,
        help="Explicit accepted generation and independently selected queue-consumer composition",
    )
    parser.add_argument(
        "--physical-context", type=Path, help="Selected complete physical census and source indices"
    )
    parser.add_argument(
        "--reward-context",
        type=Path,
        help="Selected reward/lifecycle census and first party boundary",
    )
    parser.add_argument(
        "--ai-context",
        type=Path,
        help="Selected AI census, caller inputs and accepted seed mechanism evidence",
    )
    parser.add_argument(
        "--field-context",
        type=Path,
        help="Declared source/current field-service composition inputs",
    )
    parser.add_argument("--profile", choices=("legacy", "modern-continuous"), default="legacy")
    parser.add_argument("--reference", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--evidence-root", type=Path)
    parser.add_argument("--plan", type=Path)
    parser.add_argument("--actual", type=Path)
    parser.add_argument(
        "--admission-context",
        type=Path,
        help="Selected original non-resume path and accepted seed correction evidence",
    )
    parser.add_argument(
        "--audio-context",
        type=Path,
        help="Selected audio metadata/operands/events, without PCM; audio mode only",
    )
    parser.add_argument(
        "--w2-context", type=Path, help="Accepted selected W2 occurrence inventory and session"
    )
    parser.add_argument(
        "--w1-context", type=Path, help="Independent selected W1 occurrence and NPC history indices"
    )
    parser.add_argument(
        "--map-context",
        type=Path,
        help="Bounded mutable-map historical applicability and separate controlled witnesses",
    )
    parser.add_argument("--host-log", type=Path)
    parser.add_argument("--host-exit", type=int)
    parser.add_argument("--outcome", type=Path)
    parser.add_argument("--settings", type=Path)
    parser.add_argument("--variant-report", type=Path, action="append", default=[])
    parser.add_argument(
        "--heal-context", type=Path, help="Accepted selected HEAL identities and source indices"
    )
    parser.add_argument("--matrix-scope", choices=("current-keyboard",), default="current-keyboard")
    parser.add_argument("--baseline-actual", type=Path)
    parser.add_argument("--baseline-outcome", type=Path)
    parser.add_argument(
        "--controlled-start",
        type=Path,
        help="Explicit candidate party definition; not a same-run admission snapshot",
    )
    parser.add_argument("--selected-world", type=Path)
    parser.add_argument(
        "--canonical-content",
        type=Path,
        help="Read-only existing canonical map export, verified against its accepted manifest",
    )
    parser.add_argument(
        "--tileset-metadata", type=Path, help="Read-only accepted private map tileset extraction"
    )
    parser.add_argument(
        "--palette-metadata", type=Path, help="Read-only accepted private map palette extraction"
    )
    parser.add_argument(
        "--text-source-root",
        type=Path,
        help="Read-only pinned SF2DISASM checkout for reached source bindings",
    )
    parser.add_argument("--original-join-evidence-root", type=Path)
    parser.add_argument("--selected-scene", type=Path)
    parser.add_argument("--process-receipt", type=Path)
    parser.add_argument("--scene-evidence-root", type=Path)
    parser.add_argument("--asset-root", type=Path)
    parser.add_argument("--expected-asset-commit")
    parser.add_argument("--expected-asset-tree")
    parser.add_argument("--expected-asset-manifest-sha256")
    parser.add_argument("--resource-family", choices=("all", "map", "entity", "scene"))
    parser.add_argument("--session-id")
    parser.add_argument("--visit", type=int)
    parser.add_argument("--occurrence", type=int, help="Exact logical observationSequence")
    parser.add_argument("--source-only", action="store_true")
    parser.add_argument("--source-only-private-bytes", type=int)
    args = parser.parse_args()
    require(
        args.turn_context is None
        or args.mode == "turn-order"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "turn context requires scoped turn-order or modern comparison",
    )
    if args.turn_context is not None:
        args.turn_context = (
            args.turn_context if args.turn_context.is_absolute() else repo_path(args.turn_context)
        )
        require(args.turn_context.stat().st_size <= 10 * 1024 * 1024, "Turn context exceeds 10 MiB")
    global _STREAM_SCRATCH_ROOT
    args.output = (args.output if args.output.is_absolute() else repo_path(args.output)).resolve()
    _STREAM_SCRATCH_ROOT = args.output.parent
    require(
        args.scene_context is None
        or args.mode == "battle-scene"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "Scene context applies only to battle-scene or modern compare",
    )
    if args.scene_context is not None:
        args.scene_context = (
            args.scene_context
            if args.scene_context.is_absolute()
            else repo_path(args.scene_context)
        ).resolve()
        require(args.scene_context.stat().st_size <= 512 * 1024, "Scene context exceeds 512KiB")
    if args.mode == "battle-scene":
        require(
            args.actual is not None and args.scene_context is not None,
            "battle-scene requires selected JSONL actual and independent context",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Scene output must be fresh beneath worktree local/",
        )
        binding = battle_scene_consumer_binding(
            _battle_scene_selection(args.actual), read(args.scene_context), args.text_source_root
        )
        verdict = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-battle-scene-composed",
            result=verdict,
            milestonePass=False,
            binding=binding,
        )
        require(len(json.dumps(report).encode("utf-8")) <= 1024 * 1024, "Scene report exceeds 1MiB")
        write(args.output, report)
        print(json.dumps(dict(result=verdict, coverage=binding["coverage"], milestonePass=False)))
        raise SystemExit(0 if binding["value"] is True else 1 if binding["value"] is False else 2)
    require(
        args.field_context is None
        or args.mode == "field-service"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "Field context applies only to field-service or modern compare",
    )
    if args.field_context is not None:
        args.field_context = (
            args.field_context
            if args.field_context.is_absolute()
            else repo_path(args.field_context)
        ).resolve()
        require(args.field_context.stat().st_size <= 1024 * 1024, "Field context exceeds 1MiB")
    if args.mode == "field-service":
        require(
            args.actual is not None and args.field_context is not None,
            "field-service requires actual and independent context",
        )
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.field_context.stat().st_size <= 10 * 1024 * 1024,
            "Field compact bundle exceeds 10MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Field output must be fresh beneath worktree local/",
        )
        binding = field_service_binding(
            read(actual_path), read(args.field_context), args.text_source_root
        )
        verdict = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-field-service-composed",
            result=verdict,
            milestonePass=False,
            binding=binding,
        )
        require(len(json.dumps(report).encode("utf-8")) <= 1024 * 1024, "Field report exceeds 1MiB")
        write(args.output, report)
        print(json.dumps(dict(result=verdict, cases=len(binding["cases"]), milestonePass=False)))
        raise SystemExit(0 if binding["value"] is True else 1 if binding["value"] is False else 2)
    require(
        args.ai_context is None
        or args.mode == "ai"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "AI context applies only to ai or modern compare",
    )
    if args.ai_context is not None:
        args.ai_context = (
            args.ai_context if args.ai_context.is_absolute() else repo_path(args.ai_context)
        ).resolve()
        require(args.ai_context.stat().st_size <= 1024 * 1024, "AI context exceeds 1MiB")
    if args.mode == "ai":
        require(
            args.actual is not None and args.ai_context is not None,
            "ai requires selected actual and independent context",
        )
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.ai_context.stat().st_size <= 10 * 1024 * 1024,
            "AI compact bundle exceeds 10MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "AI output must be fresh beneath this worktree local/",
        )
        binding = ai_consumer_binding(
            read(actual_path), read(args.ai_context), args.text_source_root
        )
        verdict = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-ai-consumer-composed",
            result=verdict,
            milestonePass=False,
            binding=binding,
        )
        require(len(json.dumps(report).encode("utf-8")) <= 1024 * 1024, "AI report exceeds 1MiB")
        write(args.output, report)
        print(
            json.dumps(
                dict(result=verdict, occurrences=len(binding["occurrences"]), milestonePass=False)
            )
        )
        raise SystemExit(0 if binding["value"] is True else 1 if binding["value"] is False else 2)
    require(
        args.reward_context is None
        or args.mode == "reward"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "Reward context applies only to reward or modern compare",
    )
    if args.reward_context is not None:
        args.reward_context = (
            args.reward_context
            if args.reward_context.is_absolute()
            else repo_path(args.reward_context)
        ).resolve()
        require(args.reward_context.stat().st_size <= 1024 * 1024, "Reward context exceeds 1MiB")
    if args.mode == "reward":
        require(
            args.actual is not None and args.reward_context is not None,
            "reward requires selected actual and independent census context",
        )
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.reward_context.stat().st_size <= 10 * 1024 * 1024,
            "Reward compact selection exceeds 10MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Reward output must be fresh beneath this worktree local/",
        )
        binding = reward_consumer_binding(
            read(actual_path), read(args.reward_context), args.text_source_root
        )
        verdict = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-reward-consumer", result=verdict, milestonePass=False, binding=binding
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024,
            "Reward report exceeds 10MiB",
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(result=verdict, occurrences=len(binding["occurrences"]), milestonePass=False)
            )
        )
        raise SystemExit(0 if binding["value"] is True else 1 if binding["value"] is False else 2)
    require(
        args.physical_context is None
        or args.mode == "physical"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "Physical context applies only to physical or modern compare",
    )
    if args.physical_context is not None:
        args.physical_context = (
            args.physical_context
            if args.physical_context.is_absolute()
            else repo_path(args.physical_context)
        ).resolve()
        require(
            args.physical_context.stat().st_size <= 1024 * 1024, "Physical context exceeds 1MiB"
        )
    if args.mode == "physical":
        require(
            args.actual is not None and args.physical_context is not None,
            "physical requires selected actual and independent census context",
        )
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.physical_context.stat().st_size <= 10 * 1024 * 1024,
            "Physical compact selection exceeds 10MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Physical output must be fresh beneath this worktree local/",
        )
        binding = physical_consumer_binding(
            read(actual_path), read(args.physical_context), args.text_source_root
        )
        verdict = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-physical-consumer", result=verdict, milestonePass=False, binding=binding
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024,
            "Physical report exceeds 10MiB",
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(result=verdict, occurrences=len(binding["occurrences"]), milestonePass=False)
            )
        )
        raise SystemExit(0 if binding["value"] is True else 1 if binding["value"] is False else 2)
    require(args.admission_context is None or args.mode == "admission-seed"
            or args.mode == "compare" and args.profile == "modern-continuous",
            "Admission context applies only to admission-seed or modern compare")
    if args.admission_context is not None:
        args.admission_context = (args.admission_context if args.admission_context.is_absolute()
                                  else repo_path(args.admission_context)).resolve()
        require(
            args.admission_context.stat().st_size <= 1024 * 1024, "Admission context exceeds 1MiB"
        )
    if args.mode == "admission-seed":
        require(args.actual is not None and args.admission_context is not None,
                "admission-seed requires selected actual and independent context")
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.admission_context.stat().st_size <= 10 * 1024 * 1024,
            "Admission selection exceeds 10MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Admission output must be fresh beneath this worktree local/",
        )
        selected_actual, selected_context = read(actual_path), read(args.admission_context)
        binding = admission_seed_binding(selected_actual, selected_context, args.text_source_root)
        opening = admission_opening_binding(
            selected_actual, selected_context, args.text_source_root
        )
        values = [binding["value"]]
        if "opening" in selected_context:
            values.append(opening["value"])
        decision = False if False in values else None if None in values else True
        verdict = (
            "Unavailable" if decision is None else "PASS" if decision else "FAIL"
        )
        write(
            args.output,
            dict(
                profile="modern-admission-seed-composition",
                result=verdict,
                milestonePass=False,
                binding=binding,
                opening=opening,
            ),
        )
        opening_verdict = (
            "Unavailable" if opening["value"] is None else "PASS" if opening["value"] else "FAIL"
        )
        print(
            json.dumps(dict(result=verdict, openingControls=opening_verdict, milestonePass=False))
        )
        raise SystemExit(2 if decision is None else 0 if decision else 1)
    require(
        args.map_context is None
        or args.mode in ("map", "resources")
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "Map context applies only to map, resources or modern compare",
    )
    if args.map_context is not None:
        args.map_context = (
            args.map_context.resolve()
            if args.map_context.is_absolute()
            else repo_path(args.map_context)
        )
        require(args.map_context.stat().st_size <= 10 * 1024 * 1024, "Map context exceeds 10 MiB")
    if args.mode == "map":
        require(
            args.actual is not None and args.map_context is not None,
            "map requires selected historical actual and controlled witness context",
        )
        actual_path = args.actual.resolve() if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.map_context.stat().st_size <= 10 * 1024 * 1024,
            "Map selection exceeds 10 MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "Map output must be fresh beneath this worktree's local/",
        )
        binding = map_consumer_binding(
            read(actual_path), read(args.map_context), args.text_source_root
        )
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-map-composed-consumer",
            comparisonScope="retained-keyboard-A-mutable-map",
            result=verdict_value,
            milestonePass=False,
            binding=binding,
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024, "Map report exceeds 10 MiB"
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(result=verdict_value, milestonePass=False, witnesses=len(binding["witnesses"]))
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    require(
        args.w1_context is None
        or args.mode == "w1"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "W1 context applies only to w1 or modern compare mode",
    )
    if args.w1_context is not None:
        args.w1_context = (
            args.w1_context.resolve()
            if args.w1_context.is_absolute()
            else repo_path(args.w1_context)
        )
        require(args.w1_context.stat().st_size <= 1024 * 1024, "W1 context exceeds 1 MiB")
    if args.mode == "w1":
        require(
            args.actual is not None and args.w1_context is not None,
            "w1 requires selected actual and independent occurrence context",
        )
        actual_path = args.actual.resolve() if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.w1_context.stat().st_size <= 10 * 1024 * 1024,
            "W1 selection exceeds 10 MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "W1 output must be fresh beneath this worktree's local/",
        )
        binding = w1_consumer_binding(
            read(actual_path), read(args.w1_context), args.text_source_root
        )
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-w1-consumer",
            comparisonScope="retained-keyboard-A-w1",
            result=verdict_value,
            milestonePass=False,
            binding=binding,
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024, "W1 report exceeds 10 MiB"
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(result=verdict_value, milestonePass=False, polls=len(binding["occurrences"]))
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    require(
        args.w2_context is None
        or args.mode == "w2"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "W2 context applies only to w2 or modern compare mode",
    )
    if args.w2_context is not None:
        args.w2_context = (
            args.w2_context.resolve()
            if args.w2_context.is_absolute()
            else repo_path(args.w2_context)
        )
        require(args.w2_context.stat().st_size <= 10 * 1024 * 1024, "W2 context exceeds 10 MiB")
    require(
        args.heal_context is None
        or args.mode == "heal"
        or args.mode == "compare"
        and args.profile == "modern-continuous",
        "HEAL context applies only to heal or modern compare mode",
    )
    if args.heal_context is not None:
        args.heal_context = (
            args.heal_context.resolve()
            if args.heal_context.is_absolute()
            else repo_path(args.heal_context)
        )
        require(args.heal_context.stat().st_size <= 1024 * 1024, "HEAL context exceeds 1 MiB")
    if args.mode == "heal":
        require(
            args.actual is not None and args.heal_context is not None,
            "heal requires selected actual and independent occurrence context",
        )
        actual_path = args.actual.resolve() if args.actual.is_absolute() else repo_path(args.actual)
        require(
            actual_path.stat().st_size + args.heal_context.stat().st_size <= 20 * 1024 * 1024,
            "HEAL compact selection exceeds 20 MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "HEAL output must be fresh beneath this worktree's local/",
        )
        binding = heal_consumer_binding(
            read(actual_path), read(args.heal_context), args.text_source_root
        )
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-heal-consumer",
            comparisonScope="retained-keyboard-A-heal",
            result=verdict_value,
            milestonePass=False,
            binding=binding,
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024,
            "HEAL report exceeds 10 MiB",
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(
                    result=verdict_value,
                    milestonePass=False,
                    occurrences=len(binding["occurrences"]),
                )
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    if args.mode == "w2":
        require(
            args.actual is not None and args.w2_context is not None,
            "w2 requires selected actual and independent occurrence context",
        )
        actual_path, context_path = (
            p.resolve() if p.is_absolute() else repo_path(p) for p in (args.actual, args.w2_context)
        )
        require(
            actual_path.stat().st_size + context_path.stat().st_size <= 10 * 1024 * 1024,
            "W2 selection exceeds 10 MiB",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "W2 output must be fresh beneath this worktree's local/",
        )
        binding = w2_consumer_binding(read(actual_path), read(context_path), args.text_source_root)
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-w2-composed-consumer",
            comparisonScope="retained-keyboard-A-w2",
            result=verdict_value,
            milestonePass=False,
            actual=actual_path.as_posix(),
            context=context_path.as_posix(),
            binding=binding,
        )
        require(
            len(json.dumps(report).encode("utf-8")) <= 10 * 1024 * 1024, "W2 report exceeds 10 MiB"
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(
                    result=verdict_value,
                    milestonePass=False,
                    occurrences=len(binding["occurrences"]),
                )
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    if args.mode == "turn-order":
        require(
            args.actual is not None, "turn-order requires selected actual generation/state records"
        )
        actual_path = args.actual.resolve() if args.actual.is_absolute() else repo_path(args.actual)
        context_path = (
            (
                args.turn_context.resolve()
                if args.turn_context.is_absolute()
                else repo_path(args.turn_context)
            )
            if args.turn_context is not None
            else None
        )
        require(
            actual_path.stat().st_size + (context_path.stat().st_size if context_path else 0)
            <= (10 if context_path else 1) * 1024 * 1024,
            "turn-order input exceeds its controlled/composed bundle limit",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "turn-order output must be fresh beneath this worktree's local/",
        )
        actual = read(actual_path)
        require(
            len(actual.get("rounds") or []) <= 3,
            "turn-order scope exceeds three selected generations",
        )
        binding = (
            turn_order_consumer_binding(actual, read(context_path), args.text_source_root)
            if context_path
            else turn_order_binding(actual, args.text_source_root)
        )
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-turn-order-rule",
            comparisonScope="composed-current-turn" if context_path else "controlled-application",
            result=verdict_value,
            milestonePass=False,
            actual=actual_path.as_posix(),
            binding=binding,
        )
        require(
            len(json.dumps(report, indent=2).encode("utf-8")) + 1 <= 1024 * 1024,
            "turn-order report exceeds 1 MiB",
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(
                    result=verdict_value,
                    milestonePass=False,
                    rounds=len(binding.get("generation", binding).get("rounds", [])),
                )
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    if args.mode == "audio":
        require(
            args.actual is not None and args.audio_context is not None,
            "audio requires selected actual dependency channels and audio context",
        )
        actual_path, context_path = (
            p.resolve() if p.is_absolute() else repo_path(p)
            for p in (args.actual, args.audio_context)
        )
        require(
            actual_path.stat().st_size + context_path.stat().st_size <= 10 * 1024 * 1024,
            "audio selection exceeds 10 MiB; select required fields before comparison",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "audio output must be fresh beneath this worktree's local/",
        )
        binding = audio_consumer_binding(
            read(actual_path), read(context_path), args.text_source_root
        )
        verdict_value = (
            "Unavailable" if binding["value"] is None else "PASS" if binding["value"] else "FAIL"
        )
        report = dict(
            profile="modern-audio-consumers",
            result=verdict_value,
            milestonePass=False,
            actual=actual_path.as_posix(),
            context=context_path.as_posix(),
            binding=binding,
        )
        write(args.output, report)
        print(
            json.dumps(
                dict(
                    result=verdict_value,
                    milestonePass=False,
                    playbacks=len(binding["playbacks"]),
                    releases=len(binding["releases"]),
                )
            )
        )
        raise SystemExit(2 if binding["value"] is None else 0 if binding["value"] else 1)
    require(args.audio_context is None, "audio context applies only to audio mode")
    material_selection = (
        args.selected_world,
        args.selected_scene,
        args.process_receipt,
        args.scene_evidence_root,
        args.asset_root,
        args.expected_asset_commit,
        args.expected_asset_tree,
        args.expected_asset_manifest_sha256,
    )
    require(
        not any(material_selection) or all(material_selection),
        "material comparison requires all explicit selections and asset pins",
    )
    material_selection = material_selection if all(material_selection) else None
    require(
        args.mode == "resources"
        or not any(
            (
                args.resource_family is not None,
                args.session_id is not None,
                args.visit is not None,
                args.occurrence is not None,
                args.source_only,
                args.source_only_private_bytes is not None,
            )
        ),
        "resource selections apply only to resources mode",
    )
    if args.mode == "resources":
        require(
            args.source_only
            or args.source_only_private_bytes is not None
            and args.source_only_private_bytes > 0,
            "resources requires separately measured source-only private bytes",
        )
        require(
            args.actual is not None and material_selection is not None,
            "resources requires actual capture and explicit material selection",
        )
        require(
            args.output.is_relative_to(repo_path("local").resolve()) and not args.output.exists(),
            "resource output must be fresh beneath this worktree's local/",
        )
        args.output.parent.mkdir(parents=True, exist_ok=True)
        temporary = args.output.parent / "temp"
        temporary.mkdir(exist_ok=True)
        os.environ["TEMP"] = os.environ["TMP"] = str(temporary)
        actual_path = args.actual if args.actual.is_absolute() else repo_path(args.actual)
        scope = dict(
            family=args.resource_family or "all",
            sessionId=args.session_id,
            visit=args.visit,
            observationSequence=args.occurrence,
        )
        budget = _ResourceBudget(
            args.output.parent,
            actual_path.stat().st_size,
            args.source_only_private_bytes,
            selected=scope["family"] != "all"
            or any(scope[key] is not None for key in ("sessionId", "visit", "observationSequence")),
        )
        try:
            result = compare_resources(
                actual_path,
                material_selection,
                args.text_source_root,
                args.canonical_content,
                args.tileset_metadata,
                args.palette_metadata,
                scope,
                budget,
                source_only=args.source_only,
                map_context=read(args.map_context) if args.map_context else None,
            )
            provisional = write(args.output, result, resource_budget=budget, defer_publication=True)
            # Reopen the detached bundle while its entry point is still provisional.
            read(provisional)
            budget.checkpoint("report prepared", force=True)
            receipt_path = args.output.with_name(args.output.name + ".resources.json")
            receipt_provisional = write(
                receipt_path,
                dict(
                    resources=budget.receipt(),
                    accountingStage="before receipt publication",
                    report=args.output.name,
                    complete=not result.get("incomplete", False),
                ),
                resource_budget=budget,
                defer_publication=True,
            )
            # Include both JSON entries, both companions and scratch before promotion.
            budget.checkpoint("publication prepared", force=True)
            receipt_provisional.rename(receipt_path)
            provisional.rename(args.output)
            budget.checkpoint("publication complete", force=True)
            budget.release_scratch()
        except ResourceBudgetExceeded as error:
            # A final miss must withdraw any promoted entry; keep its companion and
            # provisional payload as incomplete discovery artifacts, along with scratch.
            for path in (args.output, args.output.with_name(args.output.name + ".resources.json")):
                if path.exists():
                    path.rename(path.with_name(path.name + ".partial"))
            result = dict(
                profile="modern-resource-scope",
                comparisonScope=scope,
                result="Unavailable",
                incomplete=True,
                milestonePass=False,
                error=str(error),
                resources=budget.receipt(),
            )
            failure_path = args.output.with_name(args.output.name + ".incomplete.json")
            write(failure_path, result)
        print(json.dumps(dict(result=result.get("result"), resources=budget.receipt())), flush=True)
        raise SystemExit(
            1
            if result.get("result") == "FAIL"
            else 2
            if result.get("result") == "Unavailable"
            else 0
        )
    require(args.reference is not None, "plan/compare/matrix requires accepted reference")
    ref = reference(args.reference)
    if args.mode == "matrix":
        require(args.variant_report, "matrix requires actual variant reports")
        result = compare_matrix(args.variant_report, ref, scope=args.matrix_scope)
        write(args.output, result)
        print(
            json.dumps(
                dict(
                    **{
                        k: result[k]
                        for k in (
                            "scope",
                            "requiredVariants",
                            "excludedVariants",
                            "result",
                            "counts",
                            "milestonePass",
                        )
                    },
                    supplemental=[
                        {
                            k: row[k]
                            for k in ("variant", "result", "comparisonResult", "reportResult")
                        }
                        for row in result["supplemental"]
                    ],
                )
            )
        )
        raise SystemExit(
            1 if result["result"] == "FAIL" else 2 if result["result"] == "Unavailable" else 0
        )
    if args.mode == "plan":
        require(args.evidence_root is not None, "plan requires accepted evidence root")
        result = make_plan(ref, args.evidence_root)
        write(args.output, result)
        print(
            json.dumps(
                {
                    "logicalFieldInputs": len(result["steps"]),
                    "firstDecisionInputs": len(result["firstDecisionInputs"]),
                }
            )
        )
    else:
        if args.profile == "modern-continuous":
            require(
                all(
                    x is not None
                    for x in (
                        args.actual,
                        args.outcome,
                        args.settings,
                        args.host_log,
                        args.host_exit,
                    )
                ),
                "modern comparison requires actual/outcome/settings/host-log/recorded exit",
            )
            result = compare_modern(
                ref,
                args.actual,
                args.outcome,
                args.settings,
                args.host_log,
                args.host_exit,
                args.baseline_actual,
                args.baseline_outcome,
                args.controlled_start,
                material_selection,
                args.original_join_evidence_root,
                args.text_source_root,
                args.canonical_content,
                args.tileset_metadata,
                args.palette_metadata,
                read(args.w2_context) if args.w2_context else None,
                read(args.heal_context) if args.heal_context else None,
                read(args.w1_context) if args.w1_context else None,
                read(args.map_context) if args.map_context else None,
                read(args.admission_context) if args.admission_context else None,
                read(args.physical_context) if args.physical_context else None,
                read(args.reward_context) if args.reward_context else None,
                read(args.ai_context) if args.ai_context else None,
                read(args.field_context) if args.field_context else None,
                read(args.scene_context) if args.scene_context else None,
                read(args.turn_context) if args.turn_context else None,
            )
            write(args.output, result)
            print(json.dumps({k: result[k] for k in ("result", "counts", "milestonePass")}))
            raise SystemExit(
                1 if result["result"] == "FAIL" else 2 if result["result"] == "Unavailable" else 0
            )
        require(
            args.plan is not None
            and args.actual is not None
            and args.host_log is not None
            and args.host_exit is not None,
            "compare requires plan, actual, host-log and the recorded host-exit",
        )
        result = compare(ref, read(args.plan), args.actual, args.host_log, args.host_exit)
        write(args.output, result)
        print(
            json.dumps(
                {"result": result["result"], "counts": result["counts"], "milestonePass": False}
            )
        )
        raise SystemExit(1 if result["result"] == "FAIL" else 2)


if __name__ == "__main__":
    main()
