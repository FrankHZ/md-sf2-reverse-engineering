"""Exercise the ordinary opening/title/Witch/Back route through semantic commands."""

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


def wait_witch_menu(client):
    # Slot discovery is asynchronous. Service only its menu while focus pauses
    # publication. Space the small frame grants so disk/content work can finish.
    budget = 120
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline:
        host = client.request("observe host")["data"]
        stage = (host["entry"] or {}).get("stage")
        if stage == "menu" and host["runFramesRemaining"] == 0:
            return
        paused = not host["focused"] and not host["runInBackground"]
        if stage == "checking-saves" and paused and host["runFramesRemaining"] == 0 and budget:
            client.request("clock run 2")
            budget -= 2
        time.sleep(0.25)
    raise ValueError("Witch menu did not settle within the frame and time budgets")


def round_trip(client):
    host = wait_host(client, lambda sample: sample["opening"]["active"] or sample["entry"])
    if host["opening"]["active"]:
        client.request("opening skip")
    title = wait_host(client, lambda sample: sample["entry"]["stage"] == "title")
    if title["entry"]["audio"]["musicCue"] != "MUSIC_TITLE":
        raise ValueError("title music missing")
    client.request("menu confirm")
    client.request("witch skip")
    wait_witch_menu(client)
    client.request("menu back")
    back = wait_host(client, lambda sample: sample["entry"]["stage"] == "title")
    if back["opening"]["active"] or back["entry"]["audio"]["musicCue"] != "MUSIC_TITLE":
        raise ValueError("Back did not restore the playable title and its music")
    print("Title -> Witch menu -> playable title")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("port", type=int)
    args = parser.parse_args()
    try:
        with connect(args.port) as game:
            round_trip(game)
    except (OSError, ValueError) as error:
        parser.exit(1, f"Title round trip failed: {error}\n")
