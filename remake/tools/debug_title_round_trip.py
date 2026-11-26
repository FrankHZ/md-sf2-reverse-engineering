"""Exercise the ordinary opening/title/Witch/Back route with default keyboard bindings."""

import argparse
import time

from debug_client import connect


def wait_host(client, ready):
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline:
        host = client.request("observe host")["data"]
        if ready(host):
            return host
        time.sleep(0.05)
    raise ValueError("expected host state did not arrive before deadline")


def released(stage):
    def ready(host):
        entry = host["entry"] or {}
        return (
            entry.get("stage") == stage
            and not entry["latched"]
            and not entry["activated"]
            and not host["gameplayHeld"]
            and not host["releaseBarrier"]
            and host["focused"]
        )

    return ready


def round_trip(client):
    print("Instance", client.request("hello")["data"])
    host = wait_host(client, lambda h: h["focused"] and (h["opening"]["active"] or h["entry"]))
    if host["opening"]["active"]:
        client.tap("E")
    title = wait_host(client, released("title"))
    if title["entry"]["audio"]["musicCue"] != "MUSIC_TITLE":
        raise ValueError("title music missing")
    client.tap("Enter")
    client.request("down E")
    menu = wait_host(client, lambda h: h["entry"]["stage"] == "menu" and h["entry"]["latched"])
    if not menu["gameplayHeld"]:
        raise ValueError("Witch menu lost the held Start input")
    client.request("up E")
    wait_host(client, released("menu"))
    client.tap("Escape")
    back = wait_host(client, released("title"))
    if back["opening"]["active"] or back["entry"]["audio"]["musicCue"] != "MUSIC_TITLE":
        raise ValueError("Back did not restore the playable title and its music")
    print("Title -> Witch menu (held, released) -> playable title")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("port", type=int)
    args = parser.parse_args()
    try:
        with connect(args.port) as game:
            round_trip(game)
    except (OSError, ValueError) as error:
        parser.exit(1, f"Title round trip failed: {error}\n")
