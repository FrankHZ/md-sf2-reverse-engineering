from __future__ import annotations

import ast
import json
import re
from collections import Counter
from pathlib import Path

import pytest

from sf2tool.h3 import bizhawk, bootstrap
from sf2tool.harness import H3_STAGES

ROOT = Path(__file__).resolve().parents[2]
CLI_PATH = ROOT / "src" / "sf2tool" / "cli.py"
OBSERVER_ROOT = ROOT / "tools" / "bizhawk"
SCRIPTS_ROOT = ROOT / "scripts"
H3_SOURCE_ROOT = ROOT / "src" / "sf2tool" / "h3"
H3_FIXTURE_ROOT = ROOT / "tests" / "fixtures" / "h3"
OBSERVER_REFERENCE = re.compile(r"tools/bizhawk/[A-Za-z0-9_]+_observer\.lua")


def _registered_h3_commands() -> set[str]:
    tree = ast.parse(CLI_PATH.read_text(encoding="utf-8"))
    commands = set()
    for node in ast.walk(tree):
        if not isinstance(node, ast.Call) or not isinstance(node.func, ast.Attribute):
            continue
        if node.func.attr != "add_parser" or not isinstance(node.func.value, ast.Name):
            continue
        if node.func.value.id != "h3_commands" or not node.args:
            continue
        command = node.args[0]
        if isinstance(command, ast.Constant) and isinstance(command.value, str):
            commands.add(command.value)
    return commands


def _repo_path_literal(node: ast.AST) -> str | None:
    if (
        isinstance(node, ast.Call)
        and isinstance(node.func, ast.Name)
        and node.func.id == "repo_path"
        and node.args
        and isinstance(node.args[0], ast.Constant)
        and isinstance(node.args[0].value, str)
    ):
        return node.args[0].value
    return None


def _h3_dispatches() -> dict[str, tuple[str, str]]:
    tree = ast.parse(CLI_PATH.read_text(encoding="utf-8"))
    imports = {
        alias.asname or alias.name: node.module
        for node in tree.body
        if isinstance(node, ast.ImportFrom)
        and node.module is not None
        and node.module.startswith("sf2tool.h3.")
        for alias in node.names
    }
    dispatches: dict[str, tuple[str, str]] = {}
    for node in ast.walk(tree):
        if not isinstance(node, ast.If) or not isinstance(node.test, ast.BoolOp):
            continue
        comparisons = [value for value in node.test.values if isinstance(value, ast.Compare)]
        command = next(
            (
                comparison.comparators[0].value
                for comparison in comparisons
                if isinstance(comparison.left, ast.Attribute)
                and comparison.left.attr == "h3_command"
                and comparison.comparators
                and isinstance(comparison.comparators[0], ast.Constant)
                and isinstance(comparison.comparators[0].value, str)
            ),
            None,
        )
        is_h3 = any(
            isinstance(comparison.left, ast.Attribute)
            and comparison.left.attr == "command"
            and comparison.comparators
            and isinstance(comparison.comparators[0], ast.Constant)
            and comparison.comparators[0].value == "h3"
            for comparison in comparisons
        )
        if command is None or not is_h3:
            continue
        handlers = [
            call.func.id
            for statement in node.body
            for call in ast.walk(statement)
            if isinstance(call, ast.Call)
            and isinstance(call.func, ast.Name)
            and call.func.id in imports
        ]
        assert len(handlers) == 1, command
        dispatches[command] = (imports[handlers[0]], handlers[0])
    return dispatches


def _observer_argument(
    value: ast.AST,
    *,
    paths: dict[str, str],
    fixture_bindings: dict[str, str],
) -> str | None:
    if isinstance(value, ast.Name):
        return paths.get(value.id)
    if not (
        isinstance(value, ast.Call)
        and isinstance(value.func, ast.Name)
        and value.func.id == "repo_path"
        and value.args
    ):
        return None
    literal = _repo_path_literal(value)
    if literal is not None:
        return literal
    argument = value.args[0]
    if not (
        isinstance(argument, ast.Subscript)
        and isinstance(argument.value, ast.Name)
        and isinstance(argument.slice, ast.Constant)
        and isinstance(argument.slice.value, str)
    ):
        return None
    fixture_path = fixture_bindings.get(argument.value.id)
    if fixture_path is None:
        return None
    fixture = json.loads((ROOT / fixture_path).read_text(encoding="utf-8"))
    observer = fixture.get(argument.slice.value)
    return observer if isinstance(observer, str) else None


def _dispatch_observer_calls(module: str, function: str) -> list[str]:
    module_path = H3_SOURCE_ROOT / f"{module.rsplit('.', 1)[1]}.py"
    tree = ast.parse(module_path.read_text(encoding="utf-8"))
    functions = {
        node.name: node for node in tree.body if isinstance(node, ast.FunctionDef)
    }
    paths = {
        target.id: literal
        for node in tree.body
        if isinstance(node, ast.Assign)
        for target in node.targets
        if isinstance(target, ast.Name)
        if (literal := _repo_path_literal(node.value)) is not None
    }
    fixture_bindings: dict[str, str] = {}
    pending = [function]
    seen: set[str] = set()
    observers: list[str] = []
    while pending:
        current = pending.pop()
        if current in seen:
            continue
        seen.add(current)
        definition = functions[current]
        for node in ast.walk(definition):
            if not isinstance(node, ast.Assign) or len(node.targets) != 1:
                continue
            target = node.targets[0]
            if (
                isinstance(target, ast.Name)
                and isinstance(node.value, ast.Call)
                and isinstance(node.value.func, ast.Name)
                and node.value.func.id == "load_json"
                and node.value.args
            ):
                fixture_path = _repo_path_literal(node.value.args[0])
                if fixture_path is None and isinstance(node.value.args[0], ast.Name):
                    fixture_path = paths.get(node.value.args[0].id)
                if fixture_path is not None:
                    fixture_bindings[target.id] = fixture_path
        for node in ast.walk(definition):
            if not isinstance(node, ast.Call) or not isinstance(node.func, ast.Name):
                continue
            if node.func.id in functions:
                pending.append(node.func.id)
            if node.func.id != "run_observer":
                continue
            observer_path = next(
                (
                    _observer_argument(
                        keyword.value,
                        paths=paths,
                        fixture_bindings=fixture_bindings,
                    )
                    for keyword in node.keywords
                    if keyword.arg == "observer_path"
                ),
                None,
            )
            assert observer_path is not None, f"{module}:{function}:{node.lineno}"
            observers.append(observer_path)
    return observers


def test_h3_bootstrap_registry_closes_every_registered_owner() -> None:
    observers = {
        path.relative_to(ROOT).as_posix() for path in OBSERVER_ROOT.glob("*_observer.lua")
    }
    legacy_launchers = {
        path.relative_to(ROOT).as_posix() for path in SCRIPTS_ROOT.glob("Test-H3*.ps1")
    }

    dispatches = _h3_dispatches()

    assert set(bootstrap.OBSERVER_PROFILES) == observers
    assert set(dispatches) == _registered_h3_commands()
    assert set(bootstrap.COMMAND_LAUNCHES) == set(dispatches)
    assert set(bootstrap.H3_COMMAND_PROFILES) == set(dispatches)
    assert set(bootstrap.LEGACY_LAUNCHER_PROFILES) == legacy_launchers
    assert Counter(bootstrap.OBSERVER_PROFILES.values()) == {
        "battle01-intro-skip": 28,
        "map-debug-host": 25,
        "direct-function-seam": 13,
        "witch-menu": 6,
        "sound-driver": 1,
        "original-reference": 2,
    }
    assert list(dispatches).count("service-menu-lifecycle") == 1
    assert Counter(bootstrap.H3_COMMAND_PROFILES.values()) == {
        "battle01-intro-skip": 31,
        "map-debug-host": 25,
        "direct-function-seam": 11,
        "witch-menu": 6,
        "sound-driver": 1,
        "original-reference": 2,
    }
    assert Counter(launch.expected_launches for launch in bootstrap.COMMAND_LAUNCHES.values()) == {
        0: 1,
        1: 70,
        2: 1,
        8: 1,
        16: 1,
        14: 1,
        27: 1,
    }
    assert sum(launch.expected_launches for launch in bootstrap.COMMAND_LAUNCHES.values()) == 137
    assert Counter(bootstrap.LEGACY_LAUNCHER_PROFILES.values()) == {
        "battle01-intro-skip": 15,
    }
    assert {stage.script for stage in H3_STAGES} == {
        Path(path).name for path in legacy_launchers
    }

    assert set(bootstrap.PROFILES) == set(bootstrap.OBSERVER_PROFILES.values())
    for profile in bootstrap.PROFILES.values():
        assert profile.isolation_reason

    for command, launch in bootstrap.COMMAND_LAUNCHES.items():
        assert (launch.dispatch_module, launch.dispatch_function) == dispatches[command]
        assert bootstrap.H3_COMMAND_PROFILES[command] == launch.profile
        actual = _dispatch_observer_calls(
            launch.dispatch_module, launch.dispatch_function
        )
        if launch.profile == "original-reference":
            assert actual == []
            continue
        assert set(actual) == set(launch.observers)
        actual_counts = Counter(actual)
        for observer_launch in launch.launches:
            assert bootstrap.OBSERVER_PROFILES[observer_launch.observer] == launch.profile
            if observer_launch.cases_fixture is None:
                assert actual_counts[observer_launch.observer] == observer_launch.expected_launches
            else:
                fixture = json.loads(
                    (ROOT / observer_launch.cases_fixture).read_text(encoding="utf-8")
                )
                assert actual_counts[observer_launch.observer] == 1
                assert len(fixture["cases"]) == observer_launch.expected_launches


def test_battle01_compatible_observers_use_the_shared_two_prompt_helper() -> None:
    helper = (ROOT / "tools" / "bizhawk" / "bootstrap.lua").read_text(
        encoding="utf-8"
    )
    expected_call = re.compile(
        r"bootstrap\.battle01_intro_skip\(\s*"
        r"config\.bootstrap\.profile\s*,\s*"
        r"prompt_count\s*,\s*pulse\s*\)"
    )
    bootstrap_load = re.compile(
        r"local\s+bootstrap\s*=\s*assert\(dofile\(config\.bootstrapLibraryPath\)\)"
    )
    bare_prompt_two = 'prompt_count == 2 then pulse("C")'
    compatible = {
        path
        for path, profile in bootstrap.OBSERVER_PROFILES.items()
        if bootstrap.PROFILES[profile].battle01_compatible
    }

    assert compatible == set(bootstrap.BATTLE01_OBSERVERS)
    assert "if profile ~= \"battle01-intro-skip\" then" in helper
    assert "if prompt_count == 1 or prompt_count == 2 then" in helper
    assert helper.index('pulse("Right")') < helper.index('pulse("C")')
    for relative in compatible:
        source = (ROOT / relative).read_text(encoding="utf-8")
        assert bootstrap_load.search(source)
        assert expected_call.search(source)
        assert bare_prompt_two not in source


def test_non_battle01_observers_cannot_load_or_call_the_bootstrap_helper() -> None:
    for relative, profile_name in bootstrap.OBSERVER_PROFILES.items():
        if bootstrap.PROFILES[profile_name].battle01_compatible:
            continue
        source = (ROOT / relative).read_text(encoding="utf-8")
        assert "config.bootstrapLibraryPath" not in source
        assert "bootstrap.battle01_intro_skip" not in source


def test_legacy_battle01_launchers_reuse_the_accepted_observer() -> None:
    for relative, profile_name in bootstrap.LEGACY_LAUNCHER_PROFILES.items():
        assert profile_name == "battle01-intro-skip"
        source = (ROOT / relative).read_text(encoding="utf-8")
        assert "Observe-H3Battle01TurnOrder.ps1" in source


def test_shared_runner_injects_the_profile_and_waits_for_one_process() -> None:
    source = (ROOT / "src" / "sf2tool" / "h3" / "bizhawk.py").read_text(encoding="utf-8")
    existing_runner = source[source.index("def run_observer(") :]

    assert source.count("subprocess.Popen(") == 2
    assert existing_runner.count("subprocess.Popen(") == 1
    assert "runtime_bootstrap(observer_path)" in existing_runner
    assert "bootstrapLibraryPath" in existing_runner
    assert "validate_lua_syntax(BOOTSTRAP_LIBRARY, executable)" in existing_runner
    assert "process.communicate(timeout=timeout_seconds)" in existing_runner
    assert '"failure:observer-callback:" in status_tail' in existing_runner


def test_shared_runner_promotes_callback_failure_after_process_exit(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    rom_path = tmp_path / "input.bin"
    rom_path.touch()
    observer_path = ROOT / bootstrap.BATTLE01_OBSERVERS[0]
    launched: dict[str, object] = {}

    class FinishedProcess:
        returncode = 1

        def communicate(self, *, timeout: int) -> tuple[str, str]:
            launched["timeout"] = timeout
            status_path = tmp_path / "callback-failure.status.txt"
            status_path.write_text(
                "milestone:number-prompt:2\n"
                'failure:observer-callback:{"caseId":"case-7",'
                '"expectedCallback":{"pc":4096},'
                '"actualCallback":{"pc":4112}}\n',
                encoding="utf-8",
            )
            return "observer stdout", "observer stderr"

    def fake_popen(*args: object, **kwargs: object) -> FinishedProcess:
        launched["command"] = args[0]
        environment = kwargs["env"]
        assert isinstance(environment, dict)
        config_path = Path(environment["SF2_H3_CONFIG"])
        config = config_path.read_text(encoding="utf-8")
        assert '"profile"] = "battle01-intro-skip"' in config
        assert "bootstrapLibraryPath" in config
        return FinishedProcess()

    monkeypatch.setattr(bizhawk, "DERIVED_ROOT", tmp_path)
    monkeypatch.setattr(bizhawk, "bizhawk_contract", lambda: ({}, tmp_path / "EmuHawk.exe"))
    monkeypatch.setattr(bizhawk, "validate_lua_syntax", lambda *_: None)
    monkeypatch.setattr(bizhawk.subprocess, "Popen", fake_popen)

    with pytest.raises(RuntimeError, match="callback failure") as error:
        bizhawk.run_observer(
            rom_path=rom_path,
            observer_path=observer_path,
            config={"case": {"id": "case-7"}},
            output_name="callback-failure",
            timeout_seconds=17,
        )

    assert "case-7" in str(error.value)
    assert "milestone:number-prompt:2" in str(error.value)
    assert "expectedCallback" in str(error.value)
    assert "actualCallback" in str(error.value)
    assert launched["timeout"] == 17


def test_shared_runner_tree_kills_and_reaps_a_timed_out_windows_process(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    if bizhawk.os.name != "nt":
        pytest.skip("Windows tree-kill branch")
    rom_path = tmp_path / "input.bin"
    rom_path.touch()
    observer_path = ROOT / bootstrap.BATTLE01_OBSERVERS[0]
    events: list[object] = []

    class TimedOutProcess:
        pid = 4242

        def communicate(self, timeout: int | None = None) -> tuple[str, str]:
            events.append(timeout)
            if timeout is not None:
                raise bizhawk.subprocess.TimeoutExpired("EmuHawk.exe", timeout)
            return "after kill stdout", "after kill stderr"

    def fake_popen(*_: object, **__: object) -> TimedOutProcess:
        return TimedOutProcess()

    def fake_taskkill(command: list[str], **kwargs: object) -> None:
        events.append((command, kwargs))

    monkeypatch.setattr(bizhawk, "DERIVED_ROOT", tmp_path)
    monkeypatch.setattr(bizhawk, "bizhawk_contract", lambda: ({}, tmp_path / "EmuHawk.exe"))
    monkeypatch.setattr(bizhawk, "validate_lua_syntax", lambda *_: None)
    monkeypatch.setattr(bizhawk.subprocess, "Popen", fake_popen)
    monkeypatch.setattr(bizhawk.subprocess, "run", fake_taskkill)

    with pytest.raises(RuntimeError, match="timed out after 9s"):
        bizhawk.run_observer(
            rom_path=rom_path,
            observer_path=observer_path,
            config={"case": {"id": "timeout-case"}},
            output_name="timeout",
            timeout_seconds=9,
        )

    assert events == [
        9,
        (
            ["taskkill.exe", "/PID", "4242", "/T", "/F"],
            {"check": False, "capture_output": True},
        ),
        None,
    ]
