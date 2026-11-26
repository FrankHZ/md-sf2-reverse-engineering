"""Check one named showcase checkpoint on a freshly launched v2 game instance."""

import argparse
import time

from debug_client import connect


def require(ok, message):
    if not ok:
        raise ValueError(message)


def observe(client, owner):
    sample = client.request("observe " + owner)["data"] or {}
    for part in (sample, sample.get("entry"), sample.get("opening"), sample.get("scene"),
                 sample.get("presentation"), sample.get("audio")):
        if isinstance(part, dict):
            for key in ("failure", "error", "assetError"):
                require(not part.get(key), f"{owner} {key}: {part.get(key)}")
    return sample


def wait(client, owner, ready, label):
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline:
        sample = observe(client, owner)
        if ready(sample):
            return sample
        time.sleep(0.08)
    raise ValueError(f"deadline waiting for {label}")


def resume(client, slot, hold):
    def entry(predicate, label):
        host = wait(client, "host", lambda h: predicate(h.get("entry") or {}), label)
        require(not host["releaseBarrier"], f"held input at {label}")
        print(label, flush=True)
        time.sleep(hold)

    wait(client, "host", lambda h: h["opening"]["active"], "opening")
    client.request("opening skip")
    entry(lambda e: e.get("stage") == "title" and e["audio"]["musicCue"] == "MUSIC_TITLE", "title")
    client.request("menu confirm")
    entry(lambda e: e.get("stage") == "dialogue", "Witch")
    client.request("witch skip")
    entry(lambda e: e.get("selectedAction") == "Continue", "Continue")
    client.request("menu confirm")
    entry(lambda e: e.get("continueText") == 225, "question 225")
    client.request("menu confirm")
    slots = wait(client, "slots", lambda s: s.get("state") == "continue-slots", "slots")
    require(slots["selected"] == 0, "unexpected initial slot selection")
    if slot == 2:
        client.request("menu down")
    slots = observe(client, "slots")
    require(slots["selected"] == slot - 1 and "BOWIE" in slots["choices"][slot - 1],
            f"expected English BOWIE record in slot {slot}")
    client.request("menu confirm")
    entry(lambda e: e.get("continueText") == 226, "recognition 226")
    client.request("menu confirm")
    entry(lambda e: e.get("continueText") == 224, "farewell 224")
    client.request("menu confirm")


def movement(s):
    return s.get("stage") == "Movement" and s.get("stopReason") == "PlayerInput" and not s["movement"]


def battle01(client, hold):
    first = wait(client, "state", movement, "Chester movement")
    require((first["map"], first["round"], first["actor"], first["queueCursor"]) ==
            ("map57", 1, "ally-2", 0), "wrong Battle01 checkpoint")

    def mark(label, s):
        audio = observe(client, "audio")
        print(label, s["actor"], s["queueCursor"], s["previewX"], s["previewY"], audio["musicCue"], flush=True)
        time.sleep(hold)

    mark("Movement", first)
    client.request("battle back")
    mark("inspection", wait(client, "state", lambda s: s["inspection"]["page"] == "Cursor", "inspection"))
    client.request("battle back")
    back = wait(client, "state", lambda s: movement(s) and not s["inspection"]["active"], "unspent turn")
    require((back["actor"], back["queueCursor"]) == (first["actor"], first["queueCursor"]), "inspection spent turn")

    def preview(before):
        client.request("battle up")
        moved = wait(client, "state", lambda s: movement(s) and s["previewX"] == before["previewX"]
                     and s["previewY"] == before["previewY"] - 1, "legal preview")
        mark("preview", moved)
        return moved

    moved = preview(back)
    client.request("battle confirm")
    mark("actions", wait(client, "state", lambda s: s["stage"] == "ActionChoice"
                         and s["menu"]["visible"] and s["menu"]["page"] == "Main", "actions"))
    client.request("battle stay")
    nxt = wait(client, "state", lambda s: movement(s) and s["actor"] != first["actor"], "next turn")
    prior = next(a for a in nxt["actors"] if a["id"] == first["actor"])
    require(nxt["sessionId"] == first["sessionId"] and nxt["round"] == first["round"]
            and nxt["queueCursor"] > first["queueCursor"]
            and (prior["x"], prior["y"]) == (moved["previewX"], moved["previewY"]), "Stay did not commit")
    preview(nxt)


def map16(client):
    def tile(s, map_id, x, y):
        if s.get("map") != map_id or not s.get("canWaitAtInput"):
            return False
        return any(e["id"] == "entity-0" and not e["moving"] and
                   (e["x"], e["y"]) == (384 * x, 384 * y) for e in s["entities"])

    before = wait(client, "state", lambda s: tile(s, "map-16", 26, 30), "Map16 south approach")
    flags = set(before["flags"])
    require(before["partyLists"]["Active"] == list(range(6)) and before["gold"] == 1072
            and {5, 37, 384, 406, 505, 663} <= flags and flags.isdisjoint({73, 405, 456, 506}),
            "wrong Galam predecessor")
    print("Map16 (26,30), six active, gold 1072", flush=True)
    client.request("field down")
    for y in range(10, 29):
        wait(client, "state", lambda s: tile(s, "map-18", 9, y), f"Map18 (9,{y})")
        client.request("field down")

    deadline = time.monotonic() + 90
    texts = []
    while time.monotonic() < deadline:
        s = observe(client, "state")
        if movement(s):
            break
        if s.get("canWaitForText") and s.get("map") == "map-66":
            if not texts or texts[-1] != s["textId"]:
                texts.append(s["textId"])
            client.request("story confirm")
        elif (s.get("scene") or {}).get("phase") in ("ActionMessage", "ResultMessage", "DeathMessage",
                                                       "RewardMessage", "GrowthMessage", "GoldMessage"):
            client.request("battle confirm")
        time.sleep(0.10)
    else:
        raise ValueError("Battle06 first control deadline")
    active = {a["id"] for a in s["actors"] if a["id"].startswith("ally-") and a["hp"] > 0 and a["x"] is not None}
    require(texts == list(range(2342, 2357)) and s["sessionId"] == before["sessionId"]
            and s["gold"] == before["gold"] and {407, 456} <= set(s["storyFlags"]) and 506 not in s["storyFlags"]
            and active == {f"ally-{i}" for i in range(6)}, "Battle06 entry or custody mismatch")
    client.request("battle confirm")
    wait(client, "state", lambda b: b["stage"] == "ActionChoice", "usable Battle06 control")
    client.request("battle back")
    wait(client, "state", movement, "Battle06 movement restored")
    print("Map18 south gate -> Battle06, texts 2342–2356, six living allies,", observe(client, "audio")["musicCue"])


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("port", type=int)
    parser.add_argument("start", choices=("battle01", "map16"))
    parser.add_argument("--reading-hold", type=float, default=0, help="extra seconds per settled pilot shot")
    args = parser.parse_args()
    try:
        with connect(args.port) as game:
            print(game.request("hello")["data"], flush=True)
            resume(game, 1 if args.start == "battle01" else 2, args.reading_hold)
            if args.start == "battle01":
                battle01(game, args.reading_hold)
            else:
                map16(game)
    except (OSError, ValueError) as error:
        parser.exit(1, f"Showcase failed: {error}\n")
