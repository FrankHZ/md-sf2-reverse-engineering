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
    target.add_argument("--campaign-field", type=Path, help="controlled full campaign start")
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
        "--save-output",
        action="store_true",
        help="enable a separate staged-save.json for a read-only seed",
    )
    parser.add_argument(
        "--journey", action="store_true", help="seed editable preferences for a new entry run"
    )
    parser.add_argument(
        "--continue-journey", type=Path, help="reuse a previous entry run's saves and preferences"
    )
    parser.add_argument(
        "--movie", action="store_true", help="record movie.ogv at 60 fps in the run directory"
    )
    parser.add_argument(
        "--foreground", action="store_true", help="show the game for visual/input review"
    )
    args = parser.parse_args()
    if not 1 <= args.port <= 65535:
        parser.error("port must be in 1..65535")
    if (args.journey or args.continue_journey) and not args.entry:
        parser.error("journey options require --entry")
    if args.continue_journey and (args.journey or args.settings):
        parser.error("continuation uses existing journey preferences")
    if args.save_output and not args.save:
        parser.error("--save-output requires --save")
    if args.movie and not args.audio:
        parser.error("Movie Maker requires explicit --audio")
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
        (args.campaign_field, "--private-campaign-start"),
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
    save_root = run / "saves"
    if args.continue_journey:
        previous = args.continue_journey.resolve()
        if previous.parent != run.parent or not (previous / "entry-journey").is_file():
            parser.error("select a previously created entry journey beside the new run directory")
        save_root = (previous / "saves").resolve()
        if save_root.parent != previous or (save_root / "debug-dev").resolve().parent != save_root:
            parser.error("the journey save namespace must remain within its original run")
        if not (save_root / "debug-dev" / "sf2-opening").is_dir():
            parser.error("the entry journey has no published save directory")
    # Each process owns its logs. Cold Continue deliberately reuses the prior save root.
    run.mkdir(parents=True, exist_ok=False)
    if args.save_output:
        options.extend(("--save-output", str(run / "staged-save.json")))
    settings = args.settings.resolve() if args.settings else None
    if not args.continue_journey and (args.journey or not args.foreground):
        # A launch-only copy leaves player preferences and supplied input files untouched.
        values = (
            json.loads(settings.read_text(encoding="utf-8-sig"))
            if settings
            else {"formatVersion": 1}
        )
        if not args.foreground:
            values["runInBackground"] = True
        if args.journey:
            preferences = save_root / "debug-dev" / "sf2-preferences"
            preferences.mkdir(parents=True)
            (preferences / "input.json").write_text(json.dumps(values), encoding="utf-8")
            (run / "entry-journey").touch()
            settings = None
        else:
            settings = run / "input-settings.json"
            settings.write_text(json.dumps(values), encoding="utf-8")
    if settings:
        options.extend(("--input-settings", str(settings)))
    environment = dict(
        os.environ,
        SF2_DEBUG_PORT=str(args.port),
        SF2_DEBUG_BACKGROUND_CLOCK="1" if args.continue_journey and not args.foreground else "0",
        SF2_SAVE_ROOT=str(save_root),
        SF2_SAVE_NAMESPACE="debug-dev",
    )
    command = [executable, "--path", str(root / "game"), "--resolution", "1920x1080"]
    if not args.audio:
        command.extend(("--audio-driver", "Dummy"))
    if args.movie:
        command.extend(
            ("--write-movie", str(run / "movie.ogv"), "--fixed-fps", "60", "--quit-after", "54000")
        )
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
