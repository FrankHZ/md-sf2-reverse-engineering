"""Run the continuous and independent pre-voyage showcase segments through v2."""

import argparse
import json
import time
from collections import deque
from contextlib import suppress
from pathlib import Path

from debug_client import connect
from pre_voyage_showcase import map16, movement, observe, require, wait
from pre_voyage_showcase_field import (
    castle_quake,
    confirm_battle_reading,
    distance,
    field_tile,
    mark,
    royal_followers,
    settle_field,
    slade_join,
    walk,
)

BATTLE_DIRECTIONS = ((0, -1, "up"), (1, 0, "right"), (0, 1, "down"), (-1, 0, "left"))


def select(client, owner, key, value, direction="down"):
    for _ in range(12):
        sample = observe(client, owner)
        if sample.get(key) == value:
            return sample
        client.request("menu " + direction)
    raise ValueError(f"could not select {owner} {value}")


def show_entry(client, hold):
    wait(client, "host", lambda h: h["opening"]["active"], "opening")
    client.request("opening skip")
    wait(client, "host", lambda h: h["entry"] and h["entry"]["stage"] == "title", "title")
    mark(client, "title", hold)
    client.request("menu confirm")
    wait(
        client, "host", lambda h: h["entry"] and h["entry"]["stage"] == "dialogue", "Witch greeting"
    )
    mark(client, "Witch greeting", hold)
    client.request("witch skip")
    wait(client, "host", lambda h: h["entry"] and h["entry"]["stage"] == "menu", "Witch choices")


def choose_entry_action(client, action):
    for _ in range(6):
        if observe(client, "host")["entry"]["selectedAction"] == action:
            client.request("menu confirm")
            return
        client.request("menu down")
    raise ValueError(f"Witch choice unavailable: {action}")


def change_display(client, setting, expected, hold):
    require(
        observe(client, "settings")["readOnly"] is False,
        "settings require an isolated editable journey",
    )
    select(client, "settings", "focus", "display")
    client.request("menu confirm")
    select(client, "settings", "focus", "language" if setting == "Language" else "graphics")
    client.request("menu right")
    require(observe(client, "settings")["draft"][setting] == expected, f"wrong {setting} draft")
    mark(client, f"Set {setting} to {expected}", hold)
    client.request("menu back")
    select(client, "settings", "focus", "apply")
    client.request("menu confirm")
    wait(client, "settings", lambda s: not s, "applied settings")


def new_game(client, hold):
    show_entry(client, hold)
    choose_entry_action(client, "Settings")
    change_display(client, "Language", "zh-CN", hold)
    entry = observe(client, "host")["entry"]
    require(entry["displayLocale"] == "zh-CN", "Witch locale after Apply")
    mark(client, "Witch in Simplified Chinese", hold)
    choose_entry_action(client, "NewGame")
    wait(client, "slots", lambda s: s.get("state") == "new-game-slots", "new game slots")
    client.request("menu confirm")
    require(client.request("observe name")["data"]["valid"], "valid name prompt")
    client.request("name draft BOWIE")
    require(client.request("observe name")["data"]["draft"] == "BOWIE", "protagonist draft")
    mark(client, "protagonist name", hold)
    client.request("menu confirm")
    wait(client, "slots", lambda s: s.get("state") == "confirm-new-game", "new game confirmation")
    select(client, "slots", "selected", 1)
    client.request("menu confirm")
    state = settle_field(client, "New Game")
    require(state["map"] == "map-3" and field_tile(state) == (56, 3), "New Game landing")
    return state


def preparation(client, stage):
    return wait(
        client,
        "state",
        lambda s: (
            (p := s["partyStatus"]["preparation"])
            and p["stage"] == stage
            and not p["requiresRelease"]
        ),
        stage,
    )["partyStatus"]["preparation"]


def prepare_bowie(client, hold):
    before = observe(client, "state")["party"]
    client.request("field back")
    wait(
        client,
        "state",
        lambda s: s["fieldMenu"]["visible"] and not s["fieldMenu"]["requiresRelease"],
        "field menu",
    )
    client.request("menu confirm")
    member = wait(client, "state", lambda s: s["partyStatus"]["selectorOpen"], "Member")
    require(
        len(member["partyStatus"]["projection"][0]["Inventory"]) == 4, "four inventory positions"
    )
    mark(client, "Member inventory", hold)
    client.request("menu confirm")
    for desired in (4, 0):
        preparation(client, "actions")
        client.request("menu confirm")
        for _ in range(6):
            options = preparation(client, "equipment")
            if options["index"] == desired:
                break
            client.request("menu down")
        require(options["index"] == desired, "equipment option")
        if desired == 0:
            description = options["page"]["description"]
            require(
                description["visible"] and bool(description["text"].strip()),
                "Wooden Sword description",
            )
            mark(client, "Wooden Sword description", hold)
        else:
            mark(client, "Unarmed option", hold)
        client.request("menu confirm")
        preparation(client, "confirm")
        client.request("menu confirm")
        preparation(client, "result")
        party = observe(client, "state")["party"]
        require((party != before) == (desired == 4), "equipment result")
        client.request("menu confirm")
    for _ in range(6):
        state = observe(client, "state")
        if not state["partyStatus"]["visible"] and not state["fieldMenu"]["visible"]:
            return
        client.request("menu back")
        time.sleep(0.07)
    raise ValueError("Member did not return to field")


def search_pickup(client, hold):
    client.request("field back")
    for _ in range(5):
        state = wait(
            client,
            "state",
            lambda s: s["fieldMenu"]["visible"] and not s["fieldMenu"]["requiresRelease"],
            "Search menu",
        )
        if state["fieldMenu"]["index"] == 3:
            break
        client.request("menu down")
    require(state["fieldMenu"]["index"] == 3, "Search selection unavailable")
    client.request("menu confirm")
    witness = wait(client, "state", lambda s: s.get("wait") == "FieldSearchWait", "Search response")
    mark(client, "Search response", hold, result=witness.get("search"))
    return settle_field(client, "Search returned control")


def opening_walk(client, hold):
    path = (
        Path(__file__).resolve().parents[2]
        / "tests/fixtures/h3/map3-battle01-natural-route-v1.json"
    )
    source = json.loads(path.read_text(encoding="utf-8"))
    trace = source["expectedObservation"]["records"][0]["logicalInputTrace"]
    for index, edge in enumerate(trace):
        state = settle_field(client, edge["waypoint"], hold=hold)
        require(
            (state["map"], field_tile(state)) == (f"map-{edge['map']}", (edge["x"], edge["y"])),
            f"opening trace diverged at {index}",
        )
        client.request("field " + ("interact" if edge["input"] == "C" else edge["input"].lower()))
        if index == 1:
            mark(client, "house threshold", hold)
    state = settle_field(client, "messenger")
    require({1, 2, 603} <= set(state["flags"]), "opening joins and messenger")
    mark(client, "Sarah and Chester after messenger", hold)
    walk(client, (46, 7), "map-3")
    walk(client, (59, 16))
    client.request("field down")
    settle_field(client, "face school pickup")
    state = search_pickup(client, hold)
    require(128 in state["flags"], "school pickup receipt")
    repeated = search_pickup(client, hold)
    require(
        state["party"] == repeated["party"] and 128 in repeated["flags"],
        "repeat Search preserved the party",
    )
    wait(client, "state", lambda s: s.get("settledFieldEntry"), "settled graphics comparison")
    open_menu(client)
    select(client, "host", "menuSelection", 2)
    client.request("menu confirm")
    change_display(client, "GraphicsMode", "hd", hold)
    if observe(client, "host")["systemMenu"]:
        client.request("menu back")
    toggled = settle_field(client, "graphics comparison")
    require(
        field_tile(toggled) == field_tile(repeated)
        and toggled["party"] == repeated["party"]
        and 128 in toggled["flags"],
        "graphics Apply preserved journey",
    )
    mark(client, "HD in the same school visit", hold)
    walk(client, (59, 12), "map-3")


def open_menu(client):
    client.request("menu open")
    wait(client, "host", lambda h: h["systemMenu"], "system menu")


def publish(client, slot_index, *, quit_after=False, hold=0):
    slots = wait(
        client, "slots", lambda s: s.get("state") in ("save-slots", "error"), "Save destinations"
    )
    require(slots["state"] == "save-slots", slots["message"])
    select(client, "slots", "selected", slot_index)
    mark(client, "Save destination", hold, destination=slots["message"])
    client.request("menu confirm")
    wait(client, "slots", lambda s: s.get("state") == "confirm-save", "Save confirmation")
    select(client, "slots", "selected", 2 if quit_after else 1)
    mark(client, "Save and Quit" if quit_after else "Save and Return", hold)
    if quit_after:
        submit_exit(client)
        return
    client.request("menu confirm")
    saved = wait(client, "slots", lambda s: s.get("state") == "saved", "published Save")
    mark(client, "Save published", hold, destination=saved["message"])
    client.request("menu confirm")


def church(client, hold):
    walk(client, (32, 13))
    client.request("field up")
    settle_field(client, "face priest")
    client.request("field interact")
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline:
        state = observe(client, "state")
        if state["church"]["visible"] and not state["church"]["requiresRelease"]:
            break
        if state.get("canWaitForText"):
            client.request("story confirm")
        time.sleep(0.05)
    else:
        raise ValueError("priest services unavailable")
    mark(client, "Church services", hold)
    client.request("menu confirm")
    wait(
        client,
        "state",
        lambda s: s["church"]["page"] != "services" and not s["church"]["requiresRelease"],
        "Church result",
    )
    mark(client, "Church result", hold)
    client.request("menu back")
    for _ in range(5):
        state = wait(
            client,
            "state",
            lambda s: s["church"]["page"] == "services" and not s["church"]["requiresRelease"],
            "Church Record selection",
        )
        if state["church"]["index"] == 3:
            break
        client.request("menu down")
    require(
        state["church"]["commands"][3]["id"] == "Save" and state["church"]["index"] == 3,
        "Record row",
    )
    client.request("menu confirm")
    publish(client, 1, hold=hold)
    settle_field(client, "Church return")


def battle01_route(client):
    walk(client, (-1, 1), "map-19")
    walk(client, (23, 3), "map-20")
    walk(client, (23, 37), "map-19")
    walk(client, (16, 6))
    client.request("field up")
    settle_field(client, "face Astral")
    client.request("field interact")
    require(608 in settle_field(client, "Astral agreement")["flags"], "Astral agreement")
    walk(client, (6, 2), "map-20")
    walk(client, (3, 36), "map-21")
    walk(client, (5, 17))
    client.request("field up")
    settle_field(client, "face guard")
    client.request("field interact")
    require(401 in settle_field(client, "guard opens")["flags"], "guard unlock")
    walk(client, (9, 1), "map-40")
    return walk(client, (-1, 12), "map57")


def next_player(client):
    deadline = time.monotonic() + 90
    phases = set()
    while time.monotonic() < deadline:
        state = observe(client, "state")
        if movement(state):
            return state, phases
        phase = (state.get("scene") or {}).get("phase")
        if phase:
            phases.add(phase)
        if state.get("canWaitAtInput"):
            raise ValueError(
                f"battle returned to field at {state['map']} before the requested action"
            )
        if (state.get("pacing") or {}).get("reading"):
            confirm_battle_reading(client)
        time.sleep(0.08)
    raise ValueError(f"battle control deadline at {state.get('stage')} / {phase}")


def point(actor):
    return actor["x"], actor["y"]


def by_id(state, actor):
    return next(unit for unit in state["actors"] if unit["id"] == actor)


def wounded(actor):
    return actor["x"] is not None and 0 < actor["hp"] < actor["maxHp"]


def targets(state, action, name):
    ids = [target["Actor"]["Value"] for target in action["Targets"] if target["Enabled"]]
    return [actor for actor in ids if wounded(by_id(state, actor))] if name == "heal" else ids


def offer(state, name):
    for action in state["battleChoices"]["Actions"]:
        spell = action["Action"]["Spell"]
        if name == "attack":
            matches = action["Action"]["Kind"] == 2
        else:
            matches = spell and spell["Value"] == name
        if matches and action["Enabled"] and targets(state, action, name):
            return action
    return None


def toward_target(state, origin, goal, occupied):
    # The viewport terrain observation retains the 48-column storage stride.
    pending = deque([origin])
    first = {origin: None}
    closest = origin
    while pending:
        x, y = pending.popleft()
        if distance((x, y), goal) < distance(closest, goal):
            closest = (x, y)
        if distance(closest, goal) <= 1:
            break
        for dx, dy, direction in BATTLE_DIRECTIONS:
            cell = (x + dx, y + dy)
            nx, ny = cell
            if (
                cell in first
                or cell in occupied
                or not (0 <= nx < state["mapWidth"] and 0 <= ny < state["mapHeight"])
            ):
                continue
            terrain = state["terrain"][ny * 48 + nx]
            if terrain in ("Barrier", "Impassable"):
                continue
            first[cell] = first[x, y] or (cell, direction, terrain)
            pending.append(cell)
    return first[closest]


def advance(client, state, actor, goal, name):
    budget = actor["move"] * 2
    occupied = {
        point(unit)
        for unit in state["actors"]
        if unit["hp"] > 0 and unit["x"] is not None and unit["id"] != actor["id"]
    }
    for _ in range(actor["move"]):
        if name and offer(state, name):
            break
        step = toward_target(state, (state["previewX"], state["previewY"]), goal, occupied)
        if step is None:
            break
        cell, direction, terrain = step
        centaur = actor["mover"] == "Centaur"
        cost = {"Brush": 3, "Rough": 5 if centaur else 3, "Deep": 5 if centaur else 4}.get(
            terrain, 2
        )
        if budget < cost:
            break
        client.request("battle " + direction)
        state = wait(
            client,
            "state",
            lambda s, cell=cell: movement(s) and (s["previewX"], s["previewY"]) == cell,
            "preview step",
        )
        budget -= cost
    return state


def select_target(client, state, action, name, hold):
    client.request("battle confirm")
    choice = wait(client, "state", lambda s: s.get("stage") == "ActionChoice", "command list")
    mark(client, "Command list", hold)
    if name == "attack":
        client.request("battle attack")
    else:
        for _ in range(4):
            if choice["menu"]["command"] == 1:
                break
            client.request("battle down")
            choice = observe(client, "state")
        client.request("battle confirm")
        choice = wait(client, "state", lambda s: s["menu"]["page"] == "Magic", "Magic list")
        spells = [row for row in choice["battleChoices"]["Actions"] if row["Action"]["Spell"]]
        rank = next(i for i, row in enumerate(spells) if row["Action"] == action["Action"])
        for _ in range(rank):
            client.request("battle down")
        mark(client, action["Label"], hold, rank=action["Action"]["Spell"]["Level"])
        client.request("battle confirm")
    choice = wait(client, "state", lambda s: s["menu"]["page"] == "Target", "target cursor")
    legal = targets(state, action, name)
    for _ in range(len(action["Targets"]) + 1):
        target = choice.get("candidate")
        if target in legal:
            mark(client, f"{name} target", hold, target=target)
            return target
        client.request("battle right")
        choice = observe(client, "state")
    raise ValueError(f"no legal projected {name} target")


def demonstrate_battle(client, state, pending, hold, *, stage_spells=False):
    for _ in range(24):
        actor = by_id(state, state["actor"])
        name = (
            "attack"
            if "attack" in pending
            else {"ally-4": "blaze", "ally-1": "heal"}.get(actor["id"])
        )
        if name not in pending:
            name = None
        enemies = [
            unit for unit in state["actors"] if unit["id"].startswith("enemy-") and unit["hp"] > 0
        ]
        injured = [
            unit for unit in state["actors"] if unit["id"].startswith("ally-") and wounded(unit)
        ]
        guarded = stage_spells and "heal" in pending
        if name == "heal" and injured:
            goals = injured
        elif guarded and actor["id"] in ("ally-0", "ally-3", "ally-4"):
            goals = [actor]
        elif guarded and name == "heal" and by_id(state, "ally-2")["hp"] > 0:
            goals = [by_id(state, "ally-2")]
        elif "attack" not in pending and actor["id"] == "ally-1" and not injured:
            goals = [by_id(state, "ally-4")]
        else:
            goals = enemies
        goal = point(min(goals, key=lambda unit: distance(point(actor), point(unit))))
        state = advance(client, state, actor, goal, name)
        action = offer(state, name) if name else None
        if stage_spells and action:
            if (state["previewX"], state["previewY"]) != point(actor):
                client.request("battle back")
                wait(client, "state", lambda s: s["inspection"]["active"], "preview cancellation")
                client.request("battle back")
                state = wait(
                    client,
                    "state",
                    lambda s: movement(s) and not s["inspection"]["active"],
                    "unspent turn",
                )
            mark(client, f"{name} ready at unspent turn", actor=state["actor"])
            open_menu(client)
            select(client, "host", "menuSelection", 0)
            client.request("menu confirm")
            publish(client, 0, quit_after=True)
            return state
        if action is None:
            client.request("battle confirm")
            client.request("battle stay")
            state, _ = next_player(client)
            continue
        target = select_target(client, state, action, name, hold)
        hp = by_id(state, target)["hp"]
        client.request("battle confirm")
        after, phases = next_player(client)
        target_after = by_id(after, target)
        actor_after = by_id(after, actor["id"])
        require(
            "ActionAnimation" in phases and "ResultMessage" in phases,
            f"{name} action and result projection",
        )
        if name != "attack":
            require(
                actor["mp"] - actor_after["mp"] == {"blaze": 2, "heal": 3}[name],
                f"{name} MP payment",
            )
            require(
                target_after["hp"] > hp if name == "heal" else target_after["hp"] < hp,
                f"{name} target HP",
            )
        mark(
            client,
            f"{name} resolved",
            hold,
            actor=actor["id"],
            target=target,
            hp=[hp, target_after["hp"]],
            mp=[actor["mp"], actor_after["mp"]],
            exp=[actor["exp"], actor_after["exp"]],
            gold=[state["gold"], after["gold"]],
            phases=sorted(phases),
            next_actor=after["actor"],
        )
        state = after
        pending.remove(name)
        if not pending:
            return state
    raise ValueError(f"battle turn budget reached before {sorted(pending)}")


def battle_facts(state):
    keys = (
        "map",
        "round",
        "actor",
        "queueCursor",
        "gold",
        "mainSeed",
        "thinkingSeed",
        "storyFlags",
        "inventories",
        "itemCustody",
    )
    result = {key: state[key] for key in keys}
    actor_keys = (
        "id",
        "x",
        "y",
        "hp",
        "mp",
        "exp",
        "kills",
        "defeats",
        "status",
        "items",
        "spells",
    )
    result["actors"] = [{key: actor[key] for key in actor_keys} for actor in state["actors"]]
    return result


def submit_exit(client):
    # The owner may exit before replying. The caller verifies the process exit.
    with suppress(ConnectionError):
        client.request("menu confirm")


def quit_game(client, *, field=False):
    if field:
        wait(client, "state", lambda s: s.get("settledFieldEntry"), "field ready for Quit")
    open_menu(client)
    select(client, "host", "menuSelection", 4)
    client.request("menu confirm")
    wait(client, "slots", lambda s: s.get("state") == "confirm-quit", "Quit")
    select(client, "slots", "selected", 1)
    submit_exit(client)


def continuous(client, hold, witness):
    start = new_game(client, hold)
    mark(client, "New Game control", hold, position=field_tile(start))
    prepare_bowie(client, hold)
    opening_walk(client, hold)
    church(client, hold)
    state = battle01_route(client)
    client.request("battle back")
    wait(client, "state", lambda s: s["inspection"]["active"], "Battle01 inspection")
    mark(client, "Battle01 inspection", hold)
    client.request("battle back")
    restored = wait(
        client,
        "state",
        lambda s: movement(s) and not s["inspection"]["active"],
        "Battle01 inspection return",
    )
    require(
        restored["actor"] == state["actor"] and restored["queueCursor"] == state["queueCursor"],
        "inspection retained turn",
    )
    state = demonstrate_battle(client, restored, {"attack"}, hold)
    report = {
        "pid": client.request("hello")["data"]["pid"],
        "session": state["sessionId"],
        "state": battle_facts(state),
    }
    with witness.open("x", encoding="utf-8") as output:
        json.dump(report, output)
    open_menu(client)
    select(client, "host", "menuSelection", 0)
    client.request("menu confirm")
    publish(client, 2, quit_after=True, hold=hold)


def cold_continue(client, hold, witness):
    before = json.loads(witness.read_text(encoding="utf-8"))
    show_entry(client, hold)
    choose_entry_action(client, "Continue")
    wait(
        client,
        "host",
        lambda h: h["entry"] and h["entry"]["continueText"] == 225,
        "Continue question",
    )
    client.request("menu confirm")
    wait(client, "slots", lambda s: s.get("state") == "continue-slots", "Continue slots")
    select(client, "slots", "selected", 2)
    mark(client, "Continue Battle01 slot 3", hold)
    client.request("menu confirm")
    for text in (226, 224):
        wait(
            client,
            "host",
            lambda h, text=text: h["entry"] and h["entry"]["continueText"] == text,
            f"Continue {text}",
        )
        mark(client, f"Continue reading {text}", hold)
        client.request("menu confirm")
    state = wait(client, "state", movement, "cold Battle01 Movement")
    require(
        client.request("hello")["data"]["pid"] != before["pid"]
        and state["sessionId"] != before["session"],
        "Continue launched a distinct process and session",
    )
    require(
        battle_facts(state) == before["state"], "cold Continue changed the saved battle projection"
    )
    mark(client, "Cold Battle01 control", hold, actor=state["actor"], turn=state["queueCursor"])
    origin = (state["previewX"], state["previewY"])
    occupied = {point(actor) for actor in state["actors"] if actor["hp"] > 0}
    for dx, dy, direction in BATTLE_DIRECTIONS:
        x, y = cell = (origin[0] + dx, origin[1] + dy)
        if cell in occupied or not (0 <= x < state["mapWidth"] and 0 <= y < state["mapHeight"]):
            continue
        if state["terrain"][y * 48 + x] in ("Barrier", "Impassable"):
            continue
        client.request("battle " + direction)
        wait(
            client,
            "state",
            lambda s, cell=cell: movement(s) and (s["previewX"], s["previewY"]) == cell,
            "cold Continue preview",
        )
        mark(client, "Cold Continue preview", hold, position=cell)
        break
    else:
        raise ValueError("no legal adjacent cold Continue preview")
    quit_game(client)


def stage_voyage(client, hold):
    state = settle_field(client, "tower after Battle07")
    flags = set(state["flags"])
    active = state["partyLists"]["Active"]
    require(
        state["map"] == "map-58"
        and field_tile(state) == (13, 35)
        and active == list(range(7))
        and {74, 506, 507, 640} <= flags
        and flags.isdisjoint({73, 622, 623, 624, 700}),
        "wrong tower record",
    )
    mark(client, "Post-Battle07 tower", hold, active=active, royal=royal_followers(state))
    for x, y, arrival in (
        (-1, 37, "map-60"),
        (21, 34, "map-60"),
        (38, 2, "map-60"),
        (1, 30, "map-60"),
        (13, 1, "map-60"),
        (53, 1, "map-57"),
        (-1, 19, "map-40"),
        (-1, 31, "map-21"),
        (3, 16, "map-20"),
        (6, 37, "map-19"),
    ):
        state = walk(client, (x, y), arrival)
    mark(
        client, "Post-tower castle", hold, position=field_tile(state), royal=royal_followers(state)
    )
    walk(client, (16, 5))
    walk(client, (25, 13))
    state = walk(client, (-1, 31), "map-4")
    require({622, 623, 624} <= set(state["flags"]), "ordinary quake receipts")
    mark(client, "Quake town control", hold, position=field_tile(state))
    docks = walk(client, (50, 24), "map-44")
    require(
        docks["partyLists"]["Active"] == active
        and 74 in docks["flags"]
        and 73 not in docks["flags"],
        "dock party and followers",
    )
    mark(client, "Docks control", hold, position=field_tile(docks), royal=royal_followers(docks))
    wait(client, "state", lambda s: s.get("settledFieldEntry"), "dock ready to Save")
    open_menu(client)
    select(client, "host", "menuSelection", 0)
    client.request("menu confirm")
    publish(client, 0, quit_after=True, hold=hold)


def voyage_branch(client, hold):
    state = settle_field(client, "staged docks")
    flags = set(state["flags"])
    require(
        state["map"] == "map-44"
        and field_tile(state) == (1, 25)
        and state["partyLists"]["Active"] == list(range(7))
        and {74, 506, 507, 622, 623, 624, 640} <= flags
        and flags.isdisjoint({73, 700}),
        "expected the staged post-tower docks",
    )
    mark(client, "Docks control", hold, position=field_tile(state), royal=royal_followers(state))
    walk(client, (11, 14))
    client.request("field left")
    state = settle_field(client, "face captain")
    require(field_tile(state) == (11, 14), "captain counter approach")
    for choice in ("no", "yes"):
        client.request("field interact")
        prompt = settle_field(client, f"captain {choice} prompt", choice="stop")
        require(prompt.get("canWaitForChoice"), "captain departure choice")
        mark(
            client,
            f"Captain choice before {choice}",
            hold,
            speaker=prompt.get("speaker"),
            portrait=(prompt.get("portraitProjection") or {}).get("id"),
        )
        client.request("story " + ("right" if choice == "no" else "left"))
        client.request("story confirm")
        after = settle_field(client, f"captain {choice} result", hold=hold, timeout=240)
        if choice == "no":
            require(
                after["map"] == "map-44" and set(after["flags"]) == set(state["flags"]),
                "No preserved docks control",
            )
            mark(client, "No returned to docks", hold)
    flags = set(after["flags"])
    require(
        after["map"] == "map-14"
        and field_tile(after) == (40, 8)
        and 700 in flags
        and flags.isdisjoint({74, 640}),
        "damaged ship landing",
    )
    mark(client, "Damaged ship control", hold, position=field_tile(after))
    client.request("field down")
    moved = settle_field(client, "first ship step")
    require(moved["map"] == "map-14" and field_tile(moved) == (40, 9), "usable ship step")
    mark(client, "Damaged ship step", hold, position=field_tile(moved))
    quit_game(client, field=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("port", type=int)
    parser.add_argument(
        "start",
        choices=(
            "continuous",
            "cold",
            "blaze-stage",
            "heal-stage",
            "blaze",
            "heal",
            "voyage-stage",
            "voyage",
            "slade-join",
            "castle-quake",
            "equipment",
        ),
    )
    parser.add_argument(
        "--reading-hold", type=float, default=0, help="additional seconds per marked recording shot"
    )
    parser.add_argument(
        "--battle-witness",
        type=Path,
        help="fresh pre-save receipt for continuous, existing receipt for cold",
    )
    args = parser.parse_args()
    if not 0 <= args.reading_hold <= 10:
        parser.error("choose a reading hold between 0 and 10 seconds")
    if (args.start in ("continuous", "cold")) != (args.battle_witness is not None):
        parser.error("continuous and cold require a battle witness path")
    try:
        with connect(args.port) as game:
            print(json.dumps(game.request("hello")["data"]), flush=True)
            if args.start == "continuous":
                continuous(game, args.reading_hold, args.battle_witness)
            elif args.start == "cold":
                cold_continue(game, args.reading_hold, args.battle_witness)
            elif args.start in {"blaze-stage", "heal-stage"}:
                map16(game)
                state = wait(game, "state", movement, "Battle06 player turn")
                demonstrate_battle(
                    game, state, {args.start.removesuffix("-stage")}, 0, stage_spells=True
                )
            elif args.start in {"blaze", "heal"}:
                state = wait(game, "state", movement, "restored spell turn")
                require(
                    state["map"] == "map66"
                    and state["actor"] == {"heal": "ally-1", "blaze": "ally-4"}[args.start],
                    "expected a staged spell turn",
                )
                demonstrate_battle(game, state, {args.start}, args.reading_hold)
                quit_game(game)
            elif args.start == "voyage-stage":
                stage_voyage(game, args.reading_hold)
            elif args.start == "voyage":
                voyage_branch(game, args.reading_hold)
            elif args.start == "slade-join":
                slade_join(game, args.reading_hold)
                quit_game(game, field=True)
            elif args.start == "castle-quake":
                castle_quake(game, args.reading_hold)
                quit_game(game, field=True)
            else:
                state = settle_field(game, "Church field record")
                require(
                    state["map"] == "map-3"
                    and field_tile(state) == (32, 13)
                    and 128 in state["flags"],
                    "expected the journey's Church field record",
                )
                prepare_bowie(game, args.reading_hold)
                quit_game(game, field=True)
    except (OSError, ValueError) as error:
        parser.exit(1, f"Showcase failed: {error}\n")


if __name__ == "__main__":
    main()
