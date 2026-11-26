"""Operate one explicitly enabled SF2 debug instance over loopback TCP."""

import argparse
import json
import socket
from contextlib import contextmanager


class DebugClient:
    def __init__(self, connection, reader):
        self.connection = connection
        self.reader = reader

    def request(self, command):
        if not command.isascii() or not command.isprintable() or len(command) > 64:
            raise ValueError("expected one printable ASCII command of at most 64 bytes")
        self.connection.sendall(command.encode("ascii") + b"\n")
        line = self.reader.readline(1024 * 1024 + 1)
        if not line.endswith(b"\n") or len(line) > 1024 * 1024:
            raise ConnectionError("incomplete or oversized debug reply")
        reply = json.loads(line)
        if not reply["ok"]:
            raise ValueError(reply["error"])
        return reply

    def tap(self, key):
        self.request(f"down {key}")
        return self.request(f"up {key}")


@contextmanager
def connect(port):
    with (
        socket.create_connection(("127.0.0.1", port), timeout=10) as connection,
        connection.makefile("rb") as reader,
    ):
        yield DebugClient(connection, reader)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("port", type=int)
    parser.add_argument("commands", nargs="+", help='e.g. hello "tap Enter" "observe host"')
    args = parser.parse_args()
    try:
        with connect(args.port) as client:
            for command in args.commands:
                reply = (
                    client.tap(command[4:])
                    if command.startswith("tap ")
                    else client.request(command)
                )
                print(json.dumps(reply))
    except (OSError, ValueError) as error:
        parser.exit(1, f"Debug request failed: {error}\n")


if __name__ == "__main__":
    main()
