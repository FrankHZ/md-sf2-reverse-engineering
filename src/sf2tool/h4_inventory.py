"""Fixed inventory storage receipts; calculation and check ordering live in C#."""

from __future__ import annotations

import json
import math
import sys
from collections.abc import Sequence

from sf2tool.h4_dotnet import ResourceProcessError, _batches, _selected

_ERRORS = {
    "KeyError": KeyError,
    "IndexError": IndexError,
    "TypeError": TypeError,
    "ValueError": ValueError,
    "AttributeError": AttributeError,
    "OverflowError": OverflowError,
    "OSError": OSError,
    "FileNotFoundError": FileNotFoundError,
}
_KEY_LENGTHS = {
    "phase": 3,
    "group": 2,
    "required-entity": 5,
    "required-portrait": 7,
    "required-tile": 9,
    "required-layer": 5,
    "layer": 5,
    "tile": 9,
    "entity": 5,
    "portrait": 7,
}


def validate_reply(operation, result):
    def malformed():
        raise ResourceProcessError(f"Malformed H4 inventory reply for {operation}")

    if operation in (
        "inventory-start",
        "inventory-sources",
        "inventory-coverage",
        "inventory-finish",
    ):
        if result is not None:
            malformed()
        return
    if operation not in ("inventory-rows", "inventory-ack"):
        malformed()
    if (
        not isinstance(result, dict)
        or set(result) != {"batch", "rows", "done", "aborted"}
        or type(result["batch"]) is not int
        or result["batch"] < 1
        or type(result["done"]) is not bool
        or type(result["aborted"]) is not bool
        or not isinstance(result["rows"], list)
        or len(result["rows"]) > 256
        or bool(result["rows"]) == result["done"]
        or result["aborted"]
        and not result["done"]
    ):
        malformed()
    for row in result["rows"]:
        if not isinstance(row, dict) or not isinstance(row.get("kind"), str):
            malformed()
        kind = row["kind"]
        if kind == "check":
            if set(row) != {"kind"}:
                malformed()
        elif kind == "operand-error":
            if (
                set(row) != {"kind", "error", "message"}
                or not isinstance(row["error"], str)
                or row["error"] not in _ERRORS
                or not (row["message"] is None or isinstance(row["message"], str))
            ):
                malformed()
        elif kind in ("field-map", "observed-map"):
            if set(row) not in ({"kind", "key"}, {"kind", "key", "nanIdentity"}):
                malformed()
            if "nanIdentity" in row and not (
                isinstance(row["key"], float)
                and math.isnan(row["key"])
                and isinstance(row["nanIdentity"], str)
                and row["nanIdentity"].startswith("nan:")
                and row["nanIdentity"][4:].isascii()
                and row["nanIdentity"][4:].isdigit()
            ):
                malformed()
            if (
                isinstance(row["key"], float)
                and math.isnan(row["key"])
                and "nanIdentity" not in row
            ):
                malformed()
        elif kind == "definition":
            if (
                set(row) != {"kind", "key", "index"}
                or type(row["index"]) is not int
                or row["index"] < 0
            ):
                malformed()
        elif kind in _KEY_LENGTHS:
            if set(row) != ({"kind", "key", "generated"} if kind == "entity" else {"kind", "key"}):
                malformed()
            if not isinstance(row["key"], list) or len(row["key"]) != _KEY_LENGTHS[kind]:
                malformed()
            if kind == "entity" and row["generated"] is not None:
                generated = row["generated"]
                if not isinstance(generated, dict):
                    malformed()
                if set(generated) == {"error", "message"}:
                    if (
                        not isinstance(generated["error"], str)
                        or generated["error"] not in _ERRORS
                        or not (
                            generated["message"] is None or isinstance(generated["message"], str)
                        )
                    ):
                        malformed()
                elif set(generated) != {"requirement", "used"} or not isinstance(
                    generated["requirement"], dict
                ):
                    malformed()
        else:
            malformed()


class _SelectedSequence(Sequence):
    def __init__(self, rows, select):
        self.rows, self.select = rows, select

    def __len__(self):
        return len(self.rows)

    def __getitem__(self, index):
        return self.select(self.rows[index])

    def __iter__(self):
        for row in self.rows:
            yield self.select(row)


def _pack(value):
    """Retain a lazy sequence's prefix and error at its original iteration seam."""
    tails = []

    def visit(value, path):
        if isinstance(value, dict):
            return {key: visit(item, [*path, key]) for key, item in value.items()}
        if isinstance(value, Sequence) and not isinstance(value, (str, bytes)):
            rows = []
            try:
                for item in value:
                    rows.append(visit(item, [*path, len(rows)]))
            except tuple(_ERRORS.values()) as error:
                tails.append(
                    dict(
                        path=path, error=type(error).__name__, message=str(error), length=len(value)
                    )
                )
            return rows
        return value

    return dict(value=visit(value, []), tails=tails)


def _shape(value, names):
    selected = _selected(value, names)
    # A nonempty unconsumed field still makes the original container truthy.
    return {"unconsumed": None} if isinstance(value, dict) and value and not selected else selected


class Inventory:
    def __init__(self, resources, requirements, uses, value_set, join_key):
        self.resources, self.requirements, self.uses = resources, requirements, uses
        self.value_set, self.join_key = value_set, join_key
        self.sets = {}
        self.phases, self.groups = set(), set()
        self.field_maps, self.observed_maps = set(), set()
        self.definitions, self.definition_rows, self.sent = {}, [], set()
        self.sequence = 0

    def exchange(self, op, **values):
        return self.resources.exchange(dict(op="inventory-" + op, **values))

    @staticmethod
    def operand_error(value):
        error = _ERRORS[value["error"]]
        raise error(
            value["message"]
            if value["message"] is not None
            else "H4 inventory operand: " + value["error"]
        )

    def apply(self, row):
        kind = row["kind"]
        if kind == "check":
            return None
        if kind == "operand-error":
            self.operand_error(row)
        key = row["key"]
        if kind in ("field-map", "observed-map"):
            if "nanIdentity" in row:
                key = ("nan", row["nanIdentity"])
            (self.field_maps if kind == "field-map" else self.observed_maps).add(key)
        elif kind == "definition":
            if row["index"] >= len(self.definition_rows):
                raise ResourceProcessError("Inventory definition index outside current input batch")
            self.definitions[key] = self.definition_rows[row["index"]]
        elif kind in ("phase", "group"):
            (self.phases if kind == "phase" else self.groups).add(self.join_key(key))
        else:
            if kind in ("entity", "required-entity"):
                key = [*key[:4], json.dumps(key[4], sort_keys=True)]
            if kind.startswith("required-"):
                self.sets[kind].add(key)
            elif kind == "portrait":
                return dict(fresh=True, present=key in self.sets["required-portrait"], key=key)
            else:
                logical, required = self.sets[kind], self.sets["required-" + kind]
                fresh = key not in logical
                present = False
                if fresh:
                    logical.add(key)
                    if kind == "entity" and key not in required:
                        generated = row["generated"]
                        if generated is not None:
                            if "error" in generated:
                                self.operand_error(generated)
                            requirement = generated["requirement"]
                            self.requirements.append(requirement)
                            self.uses.append(dict(requirement, used=generated["used"]))
                            required.add(key)
                            self.phases.add(self.join_key((key[0], "entity", key[1])))
                            self.groups.add(self.join_key((key[0], "entity")))
                    present = key in required
                return dict(fresh=fresh, present=present, key=key)
        return None

    def drain(self, reply):
        while True:
            if reply["batch"] != self.sequence + 1 or reply["aborted"]:
                raise ResourceProcessError("Out-of-order inventory batch")
            self.sequence = reply["batch"]
            if reply["done"]:
                return
            results, failed = [], None
            for row in reply["rows"]:
                try:
                    results.append(self.apply(row))
                except ResourceProcessError:
                    raise
                except Exception as error:
                    failed = error
                    break
            failure = None
            if failed is not None:
                category = next(
                    (
                        kind
                        for kind in ("KeyError", "IndexError", "TypeError", "ValueError")
                        if isinstance(failed, _ERRORS[kind])
                    ),
                    "fatal",
                )
                failure = dict(category=category, message=str(failed))
            reply = self.exchange("ack", batch=self.sequence, results=results, failure=failure)
            if reply["aborted"]:
                if failed is None or reply["batch"] != self.sequence + 1:
                    raise ResourceProcessError("Inventory abort without a failed operation")
                self.sequence = reply["batch"]
                raise failed

    def rows(self, channel, rows, select=lambda row: row):
        def selected():
            for row in rows:
                yield _pack(select(row))

        for batch in _batches(selected(), flush_on_error=True):
            self.drain(self.exchange("rows", channel=channel, rows=batch))

    def sources(self, rows, maps, portraits):
        entries = []
        for record in rows:
            row = record["value"]
            state = row.get("state") if isinstance(row, dict) else None
            if not isinstance(state, dict):
                continue
            for kind, key, table, names in (
                ("definition", state.get("map"), self.definitions, ("layout", "layoutEvents")),
                ("visual", state.get("map"), maps, ("blocks",)),
                ("portrait", state.get("portraitId"), portraits, ("eyes", "mouth")),
            ):
                try:
                    identity = kind, key
                    if identity in self.sent or key not in table:
                        continue
                except TypeError:
                    continue  # The original C# lookup preserves this operand failure.
                entries.append(dict(kind=kind, key=key, value=_pack(_selected(table[key], names))))
                self.sent.add(identity)
        for batch in _batches(entries):
            self.exchange("sources", rows=batch)


def run(
    resources,
    actual,
    requirements,
    uses,
    world,
    maps,
    source_portraits,
    *,
    value_set,
    join_key,
    budget,
    map_binding,
):
    inventory = Inventory(resources, requirements, uses, value_set, join_key)
    resources.flush_checks()
    inventory.exchange(
        "start",
        enabled=list(resources.enabled),
        present=bool(requirements),
        maps=list(maps),
        mapBinding=_selected(map_binding, ("value",)),
        integerDigitLimit=sys.get_int_max_str_digits(),
    )

    def field(row):
        value = _selected(row, ("state",))
        if isinstance(value, dict) and "state" in value:
            value["state"] = _selected(value["state"], ("map",))
        return value

    inventory.rows("fields", actual.get("samples", []), field)
    inventory.rows("observed", requirements, lambda r: _selected(r, ("kind", "identity")))
    inventory.exchange("coverage", complete=inventory.field_maps <= inventory.observed_maps)
    inventory.rows("phases", requirements, lambda r: _selected(r, ("kind", "identity")))
    for channel, name in (("entities", "entity"), ("portraits", "portrait"), ("tiles", "tile")):
        inventory.sets["required-" + name] = value_set()
        inventory.rows(
            channel,
            requirements,
            lambda r: _selected(
                r,
                (
                    "kind",
                    "identity",
                    "subject",
                    "slot",
                    "expected",
                    "layer",
                    "highPriority",
                    "pass",
                ),
            ),
        )
    # Definitions retain the original Python dictionary/storage authority and only
    # expose ID operands. Selected geometry is sent on demand below.
    definition_refs = {}

    def definitions():
        for index, row in enumerate(world["maps"]):
            definition_refs[index] = row
            yield dict(index=index, row=_selected(row, ("id",)))

    for batch in _batches(definitions(), flush_on_error=True):
        inventory.definition_rows = [definition_refs.pop(row["index"]) for row in batch]
        inventory.drain(
            inventory.exchange(
                "rows", channel="definitions", rows=[_pack(row["row"]) for row in batch]
            )
        )
    inventory.definition_rows = []
    for name in ("tile", "entity", "layer", "required-layer"):
        inventory.sets[name] = value_set()
    inventory.rows(
        "layers",
        requirements,
        lambda r: _selected(r, ("kind", "identity", "layer", "highPriority", "subject")),
    )

    def states():
        index = 0
        for channel in ("samples", "consumerBoundaries", "warpRecords"):
            for row in actual.get(channel, []):
                if budget is not None and index % 256 == 0:
                    budget.checkpoint("independent logical inventory")
                index += 1
                selected = _selected(row, ("state",))
                if isinstance(selected, dict) and "state" in selected:
                    selected["state"] = _selected(
                        selected["state"],
                        (
                            "cameraProjection",
                            "presentation",
                            "revision",
                            "map",
                            "observationSequence",
                            "entities",
                            "nod",
                            "portraitProjection",
                            "portraitWork",
                            "portraitFlags",
                            "portraitId",
                            "_captureLocator",
                        ),
                    )
                    state = selected["state"]
                    if isinstance(state, dict):
                        for field_name, names in (
                            ("presentation", ("cameraX", "cameraY", "activeCue")),
                            (
                                "cameraProjection",
                                (
                                    "revision",
                                    "map",
                                    "background",
                                    "foreground",
                                    "backgroundHigh",
                                    "foregroundHigh",
                                    "occlusionDraws",
                                    "actors",
                                    "sessionId",
                                    "observationSequence",
                                    "simulationTick",
                                    "token",
                                    "drawSequence",
                                ),
                            ),
                            ("portraitProjection", ("id",)),
                            ("portraitWork", ("EyesClosed", "MouthOpen")),
                            ("nod", ("Entity", "Lowered")),
                        ):
                            if field_name in state:
                                state[field_name] = _shape(state[field_name], names)
                        if isinstance(state.get("entities"), Sequence) and not isinstance(
                            state["entities"], (str, bytes)
                        ):
                            state["entities"] = _SelectedSequence(
                                state["entities"],
                                lambda entity: _selected(
                                    entity,
                                    (
                                        "x",
                                        "y",
                                        "Visible",
                                        "sprite",
                                        "facing",
                                        "animationCounter",
                                        "id",
                                        "slot",
                                    ),
                                ),
                            )
                        projection = state.get("cameraProjection")
                        if isinstance(projection, dict):

                            def layer(value):
                                selected = _selected(
                                    value,
                                    (
                                        "draws",
                                        "x",
                                        "y",
                                        "offsetX",
                                        "offsetY",
                                        "highPriority",
                                        "subject",
                                        "pass",
                                        "first",
                                        "overlaps",
                                    ),
                                )
                                if isinstance(selected, dict):
                                    if "first" in selected:
                                        selected["first"] = _shape(
                                            selected["first"], ("block", "tile", "word")
                                        )
                                    if isinstance(
                                        selected.get("overlaps"), Sequence
                                    ) and not isinstance(selected["overlaps"], (str, bytes)):
                                        selected["overlaps"] = _SelectedSequence(
                                            selected["overlaps"],
                                            lambda tile: _selected(tile, ("block", "tile", "word")),
                                        )
                                return selected

                            for name in (
                                "background",
                                "foreground",
                                "backgroundHigh",
                                "foregroundHigh",
                            ):
                                if name in projection:
                                    projection[name] = layer(projection[name])
                            if isinstance(
                                projection.get("occlusionDraws"), Sequence
                            ) and not isinstance(projection["occlusionDraws"], (str, bytes)):
                                projection["occlusionDraws"] = _SelectedSequence(
                                    projection["occlusionDraws"], layer
                                )
                            if isinstance(projection.get("actors"), Sequence) and not isinstance(
                                projection["actors"], (str, bytes)
                            ):
                                projection["actors"] = _SelectedSequence(
                                    projection["actors"],
                                    lambda actor: _selected(
                                        actor, ("entity", "slot", "visible", "resourceSelector")
                                    ),
                                )
                yield _pack(selected)

    if resources.enabled.intersection(("map", "entity")):
        for batch in _batches(states(), flush_on_error=True):
            inventory.sources(batch, maps, source_portraits)
            inventory.drain(inventory.exchange("rows", channel="states", rows=batch))
    inventory.exchange("finish", any=bool(inventory.sets["entity"]))
    return inventory.phases, inventory.groups
