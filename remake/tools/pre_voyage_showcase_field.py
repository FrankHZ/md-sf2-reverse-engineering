"""Drive field inputs and observe the two short pre-voyage scenes."""

import json
import time
from collections import deque

from pre_voyage_showcase import movement, observe, require, wait

FIELD_DIRECTIONS = ((0, 1, "down"), (1, 0, "right"), (0, -1, "up"), (-1, 0, "left"))


def mark(client, label, hold=0, **details):
    frame = client.request("observe host")["frame"]
    print(json.dumps({"shot": label, "frame": frame, **details}, ensure_ascii=True), flush=True)
    if hold:
        time.sleep(hold)


def player(state):
    return next(entity for entity in state["entities"] if entity["id"] == "entity-0")


def field_tile(state):
    unit = player(state)
    return unit["x"] // 384, unit["y"] // 384


def settle_field(client, label, *, choice="yes", hold=0, timeout=90):
    deadline = time.monotonic() + timeout
    marked_texts = set()
    while time.monotonic() < deadline:
        state = observe(client, "state")
        if movement(state) or (state.get("canWaitAtInput") and not player(state)["moving"]):
            return state
        if state.get("canWaitForChoice"):
            if choice == "stop":
                return state
            client.request("story " + ("left" if choice == "yes" else "right"))
            client.request("story confirm")
        elif state.get("canWaitForText") or state.get("wait") in {
            "W1TextWait",
            "FieldTextWait",
            "FieldSearchWait",
        }:
            text = state.get("textId")
            if (
                text in (510, 1009, 1013, 1015, 1018, 1019, 2213, 2239, 2292)
                and text not in marked_texts
            ):
                marked_texts.add(text)
                mark(
                    client,
                    f"Story text {text}",
                    hold,
                    speaker=state.get("speaker"),
                    portrait=(state.get("portraitProjection") or {}).get("id"),
                )
            client.request("story confirm")
        elif (state.get("pacing") or {}).get("reading"):
            confirm_battle_reading(client)
        time.sleep(0.05)
    raise ValueError(
        f"{label}: {state.get('map')} {state.get('wait')} {(state.get('scene') or {}).get('phase')}"
    )


def next_field_step(layout, state, goal, rejected):
    start = field_tile(state)
    blocked = {
        (round(entity["x"] / 384), round(entity["y"] / 384))
        for entity in state["entities"]
        if entity["id"] != "entity-0" and entity["Visible"] and entity["flagsA"] & 128
    }
    pending = deque([start])
    first = {start: None}
    while pending:
        x, y = pending.popleft()
        for dx, dy, direction in FIELD_DIRECTIONS:
            nx, ny = x + dx, y + dy
            if not (0 <= nx < 64 and 0 <= ny < 64) or ((x, y), direction) in rejected:
                continue
            stair = layout[y * 64 + x] & 0xC000
            if dx and stair in (0x4000, 0x8000):
                slope_y = y + (dx if stair == 0x4000 else -dx)
                if 0 <= slope_y < 64 and layout[slope_y * 64 + nx] & 0xC000 == stair:
                    ny = slope_y
            cell = (nx, ny)
            if cell in first or cell in blocked:
                continue
            word = layout[ny * 64 + nx]
            warp = word & 0x3C00 == 0x1000
            target = all(
                want < 0 or want == have for want, have in zip(goal, cell, strict=True)
            ) and (min(goal) >= 0 or warp)
            if not target and (warp or word & 0xC000 == 0xC000 and word & 0x3C00 != 0x400):
                continue
            first[cell] = first[(x, y)] or direction
            if target:
                return first[cell]
            pending.append(cell)
    raise ValueError(f"no route from {state['map']} {start} to {goal}")


def walk(client, goal, destination=None):
    state = settle_field(client, str(goal))
    origin = state["map"]
    layout = [
        word
        for row in range(0, 64, 4)
        for word in client.request(f"observe layout {row}")["data"]["words"]
    ]
    rejected = set()
    for _ in range(256):
        start = field_tile(state)
        if start == goal:
            return state
        direction = next_field_step(layout, state, goal, rejected)
        client.request("field " + direction)
        state = settle_field(client, direction)
        if state["map"] != origin:
            require(
                state["map"] == destination, f"unexpected arrival {state['map']} towards {goal}"
            )
            return state
        new = field_tile(state)
        if new == start:
            rejected.add((start, direction))
        elif destination == origin and distance(start, new) > 2:
            return state
    raise ValueError(f"step budget crossed on {origin} towards {goal}")


def confirm_battle_reading(client):
    try:
        client.request("battle confirm")
    except ValueError as error:
        if str(error) != "action-unavailable":
            raise


def distance(a, b):
    return abs(a[0] - b[0]) + abs(a[1] - b[1])


def royal_followers(state):
    wanted = {"entity-31": 209, "entity-30": 203, "entity-159": 205}
    actual = {
        unit["id"]: [unit["slot"], unit["sprite"]]
        for unit in state["entities"]
        if unit["id"] in wanted and unit["Visible"] and unit["follower"]
    }
    require(
        {ref: sprite for ref, (_, sprite) in actual.items()} == wanted
        and len({slot for slot, _ in actual.values()}) == 3
        and all(slot != 0 for slot, _ in actual.values()),
        "royal follower bindings",
    )
    return actual


def draw_actors(state):
    return {
        actor["entity"]: actor for actor in (state.get("cameraProjection") or {}).get("actors", [])
    }


def story_states(client, hold, reading_ids, timeout):
    deadline = time.monotonic() + timeout
    page = None
    while time.monotonic() < deadline:
        state = observe(client, "state")
        yield state
        if (
            state.get("canWaitForText")
            or (state.get("w1") or {}).get("AtInput")
            or state.get("wait") == "DialogueWait"
        ):
            text = state["textId"]
            panel = state["dialoguePanel"]
            current_page = (text, state["token"], panel["scroll"])
            if text in reading_ids and current_page != page:
                page = current_page
                mark(
                    client,
                    "Scene reading",
                    text=text,
                    speaker=state["speaker"],
                    portrait=(state.get("portraitProjection") or {}).get("id"),
                    locale=state["displayLocale"],
                )
                if hold:
                    end = client.request("hello")["frame"] + round(60 * hold)
                    while client.request("hello")["frame"] < end:
                        if time.monotonic() >= deadline:
                            raise ValueError(f"clock stopped during text {text}")
                        time.sleep(0.05)
            try:
                client.request("story confirm")
            except ValueError as error:
                if str(error) != "action-unavailable":
                    raise
        elif (state.get("pacing") or {}).get("reading"):
            confirm_battle_reading(client)
        time.sleep(0.03)
    raise ValueError(
        f"scene deadline at {state.get('map')} / {state.get('cursor')} / {state.get('wait')}"
    )


def describe_reading(state, readings):
    text = state.get("textId")
    if (
        text is not None
        and state.get("totalCharacters", 0) > 0
        and state["dialoguePanel"]["visible"]
    ):
        if not readings or readings[-1] != text:
            readings.append(text)
        return text
    return None


def party_items(state):
    return {
        actor["Actor"]["Value"]: (actor["SourceLoadout"]["Items"], actor["Exp"], actor["Kills"])
        for actor in state["party"]
        if actor["Initialized"] and actor["SourceLoadout"] is not None
    }


def slade_join(client, hold):
    start = settle_field(client, "Galam guard predecessor")
    flags = set(start["flags"])
    active = list(range(5))
    require(
        start["map"] == "map-16"
        and field_tile(start) == (4, 18)
        and start["partyLists"]["Joined"] == start["partyLists"]["Active"] == active
        and {73, 504, 660, 661, 662, 663} <= flags
        and flags.isdisjoint({5, 37, 74, 384, 405, 406, 505, 664}),
        "expected the ready Galam guard predecessor",
    )
    require(
        any(
            a["id"] == "entity-5" and a["follower"] and a["Visible"] and a["sprite"] == 14
            for a in start["entities"]
        ),
        "temporary Slade",
    )
    inventory = party_items(start)
    mark(client, "Galam guard predecessor", active=active, follower="entity-5")
    client.request("field left")

    expected = [*range(2323, 2340), 446, 2340, 2341]
    readings = []
    opened = False
    slade_draw = False
    join_music = False
    for state in story_states(client, hold, set(expected), 240):
        continuation = state.get("continuation")
        if continuation == "DefeatedProgramFinished" and not opened:
            require(
                {73, 405, 664} <= set(state["flags"])
                and {5, 37, 384, 406, 505}.isdisjoint(state["flags"]),
                "Battle05 defeated preconditions",
            )
            opened = True
            mark(client, "Clip start: Battle05 defeated program")
        text = describe_reading(state, readings) if opened else None
        if text in range(2332, 2340) and state["speaker"] == "entity-5":
            actor = draw_actors(state).get("entity-5", {})
            slade_draw |= (
                actor.get("visible", False)
                and actor.get("sprite") == 14
                and (state.get("portraitProjection") or {}).get("id") == 5
            )
        if text == 446 and state["musicWait"]:
            require({5, 37, 73} <= set(state["flags"]), "join announcement membership")
            if not join_music:
                join_music = True
                mark(
                    client,
                    "Slade join music wait",
                    text=text,
                    generation=state["musicWait"]["Generation"],
                )
        if opened and state.get("canWaitAtInput") and state["map"] == "map-16":
            break

    flags = set(state["flags"])
    require(
        readings == expected and slade_draw and join_music,
        f"Battle05 readings or projection: {readings}",
    )
    join_audio = [
        row["Operation"] for row in state["audio"]["receipts"] if row["Cue"] == "MUSIC_JOIN"
    ]
    require(
        join_audio == ["started", "finished"] and state["audio"]["musicCue"] != "MUSIC_JOIN",
        "finite join music and return",
    )
    require(
        {5, 37, 384, 406, 505} <= flags
        and flags.isdisjoint({73, 405})
        and state["partyLists"]["Joined"] == state["partyLists"]["Active"] == list(range(6))
        and field_tile(state) == (16, 18)
        and player(state)["facing"] == 3,
        "ordinary Battle05 field return",
    )
    require(
        party_items(state) == inventory and state["gold"] == start["gold"],
        "controlled victory custody",
    )
    mark(
        client,
        "Battle05 field control",
        position=field_tile(state),
        joined=state["partyLists"]["Joined"],
        join_audio=join_audio,
        slade_portrait=5,
        items_preserved=True,
    )
    client.request("field down")
    moved = wait(
        client,
        "state",
        lambda s: s.get("canWaitAtInput") and field_tile(s) != (16, 18),
        "first post-join movement",
    )
    require(moved["map"] == "map-16" and field_tile(moved) == (16, 19), "usable Map16 step")
    mark(client, "Clip end: Map16 under player control", position=field_tile(moved))


def castle_quake(client, hold):
    start = settle_field(client, "royal castle predecessor")
    flags = set(start["flags"])
    require(
        start["map"] == "map-19"
        and field_tile(start) == (26, 27)
        and start["partyLists"]["Active"] == list(range(7))
        and {74, 506, 507, 622, 623, 640} <= flags
        and flags.isdisjoint({73, 83, 407, 624, 700}),
        "expected the castle before first quake",
    )
    mark(
        client, "Clip start: castle exit", position=field_tile(start), royal=royal_followers(start)
    )
    royal_followers(walk(client, (26, 30)))
    client.request("field down")

    guards = {"entity-128", "entity-129"}
    painted = set()
    cast = guards | {"entity-31", "entity-30", "entity-159"}
    falling = {guard: set() for guard in sorted(guards)}
    readings = []
    parked = False
    for state in story_states(client, hold, set(range(715, 720)), 120):
        if state.get("map") != "map-4":
            continue
        text = describe_reading(state, readings)
        actors = draw_actors(state)
        if text == 715:
            painted.update(actor for actor in cast if actors.get(actor, {}).get("visible"))
        for guard, facings in falling.items():
            draw = actors.get(guard, {})
            sprite = draw.get("resourceSelector") or {}
            if draw.get("visible") and 0 < sprite.get("size", 24) < 24:
                if not facings:
                    mark(
                        client,
                        "Guard fall drawn",
                        guard=guard,
                        size=sprite["size"],
                        facing=draw["facing"],
                        draw_frame=state["cameraProjection"]["processFrame"],
                    )
                facings.add(draw["facing"])
        entities = {actor["id"]: actor for actor in state["entities"]}
        if (
            not parked
            and all(falling.values())
            and all(
                entities[guard]["x"] == entities[guard]["y"] == 0
                and guard in actors
                and not actors[guard]["visible"]
                for guard in guards
            )
        ):
            require(
                624 not in state["flags"] and readings == list(range(715, 720)),
                "first quake fall before its after-scene latch",
            )
            parked = True
            mark(
                client,
                "Clip end: both guards offscreen",
                latch=False,
                facings={guard: sorted(values) for guard, values in falling.items()},
                draw_frame=state["cameraProjection"]["processFrame"],
            )
        if state.get("canWaitAtInput"):
            break

    require(
        painted == cast
        and all(len(values) >= 2 for values in falling.values())
        and parked
        and readings == list(range(715, 721)),
        f"quake dialogue and both falling draws: {readings}, {falling}",
    )
    after = set(state["flags"])
    require(
        624 in after
        and 700 not in after
        and field_tile(state) == (29, 4)
        and state["partyLists"] == start["partyLists"],
        "first-visit control after quake",
    )
    falls = [
        row
        for row in state["audio"]["receipts"]
        if row["Operation"] == "started" and row["Command"] == 88
    ]
    require(len(falls) == 2, "two falling sound starts")
    mark(client, "Map4 control after quake", position=field_tile(state), falling_sounds=len(falls))
