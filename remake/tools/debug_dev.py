"""Launch a silent development instance at an explicit save or scenario."""

import argparse
import json
import os
import subprocess
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    target = parser.add_mutually_exclusive_group(required=True)
    target.add_argument(
        "--save", type=Path, help="read-only seed file, outside the run's save root"
    )
    target.add_argument("--scenario", type=Path, help="authored scenario package")
    target.add_argument("--field", type=Path, help="controlled exploration start")
    target.add_argument("--battle", type=Path, help="controlled battle start")
    target.add_argument(
        "--entry", action="store_true", help="explicit opening/title/Witch E2E check"
    )
    parser.add_argument(
        "--run-dir",
        type=Path,
        required=True,
        help="fresh disposable directory outside Git for saves and logs",
    )
    parser.add_argument("--port", type=int, required=True)
    parser.add_argument(
        "--audio", action="store_true", help="enable the real audio driver for a listening check"
    )
    parser.add_argument("--settings", type=Path, help="existing input-settings override")
    parser.add_argument(
        "--foreground", action="store_true", help="show the game for visual/input review"
    )
    args = parser.parse_args()
    if not 1 <= args.port <= 65535:
        parser.error("port must be in 1..65535")
    if os.name != "nt" and not args.foreground:
        parser.error("background native desktop is supported on Windows; use --foreground here")
    executable = os.environ.get("GODOT_BIN", "")
    if not Path(executable).is_absolute() or not Path(executable).is_file():
        parser.error("load the checkout's private-input configuration with GODOT_BIN first")
    options = []
    for value, option in (
        (args.save, "--load-save"),
        (args.scenario, "--authored-package"),
        (args.field, "--private-exploration-start"),
        (args.battle, "--private-battle-start"),
    ):
        if value is not None:
            if not value.is_file():
                parser.error(f"missing input: {value}")
            options.extend((option, str(value.resolve())))
    root = Path(__file__).resolve().parents[1]
    run = args.run_dir.resolve()
    if any((parent / ".git").exists() for parent in (run, *run.parents)):
        parser.error("run-dir must be outside Git checkouts (the ordinary save/settings boundary)")
    # Each launch owns its evidence and writable namespace; never replace an earlier run.
    run.mkdir(parents=True, exist_ok=False)
    settings = args.settings.resolve() if args.settings else None
    if not args.foreground:
        # A launch-only copy leaves player preferences and supplied input files untouched.
        values = (
            json.loads(settings.read_text(encoding="utf-8-sig"))
            if settings
            else {"formatVersion": 1}
        )
        values["runInBackground"] = True
        settings = run / "input-settings.json"
        settings.write_text(json.dumps(values), encoding="utf-8")
    if settings:
        options.extend(("--input-settings", str(settings)))
    environment = dict(
        os.environ,
        SF2_DEBUG_PORT=str(args.port),
        SF2_SAVE_ROOT=str(run / "saves"),
        SF2_SAVE_NAMESPACE="debug-dev",
    )
    command = [executable, "--path", str(root / "game"), "--resolution", "1920x1080"]
    if not args.audio:
        command.extend(("--audio-driver", "Dummy"))
    command.extend(("--", *options))
    print(
        f"Debug port {args.port}; audio {'enabled' if args.audio else 'silent'}; logs in {run}",
        flush=True,
    )
    with (
        (run / "stdout.log").open("w", encoding="utf-8") as stdout,
        (run / "stderr.log").open("w", encoding="utf-8") as stderr,
    ):
        if not args.foreground:
            from debug_desktop import run_background

            return run_background(command, root / "game", environment, stdout, stderr)
        return subprocess.call(
            command,
            cwd=root / "game",
            env=environment,
            stdout=stdout,
            stderr=stderr,
        )


if __name__ == "__main__":
    raise SystemExit(main())
