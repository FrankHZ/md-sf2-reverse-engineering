from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.legacy import run_powershell
from sf2tool.output import print_record
from sf2tool.research_index import verify_index
from sf2tool.rom import verify_rom
from sf2tool.toolchain import verify_toolchain


@dataclass(frozen=True)
class LegacyStage:
    title: str
    script: str
    needs_rom: bool = False
    needs_upstream: bool = False


H2_STAGES = (
    LegacyStage(
        "H2: deterministic static-data extraction", "Test-StaticExtraction.ps1", needs_upstream=True
    ),
    LegacyStage("H2: ROM byte decode and source parity", "Test-RomStaticParity.ps1", True, True),
    LegacyStage(
        "H2: ally growth and spell-learning extraction",
        "Test-GrowthExtraction.ps1",
        needs_upstream=True,
    ),
    LegacyStage(
        "H2: promotions and enemy definitions", "Test-EnemyPromotionExtraction.ps1", True, True
    ),
    LegacyStage(
        "H2: battle 01 placement and AI regions", "Test-Battle01Extraction.ps1", True, True
    ),
    LegacyStage(
        "H2: battle 01 terrain and scene metadata", "Test-Battle01SceneExtraction.ps1", True, True
    ),
)

H3_STAGES = (
    LegacyStage("H3: original RNG runtime behavior", "Test-H3RngFixture.ps1", needs_rom=True),
    LegacyStage(
        "H3: original stat-gain runtime behavior", "Test-H3StatGainFixture.ps1", True, True
    ),
    LegacyStage(
        "H3: battle 01 initialization and turn order",
        "Test-H3Battle01TurnOrderFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: battle 01 activation regions and enemy AI state",
        "Test-H3Battle01RegionActivationFixture.ps1",
        True,
        True,
    ),
    LegacyStage(
        "H3: battle 01 secondary-region activation state",
        "Test-H3Battle01SecondaryActivationFixture.ps1",
        True,
        True,
    ),
    LegacyStage(
        "H3: turn-order boundary behavior", "Test-H3TurnOrderBoundariesFixture.ps1", needs_rom=True
    ),
    LegacyStage(
        "H3: physical damage construction and persistent battle-scene replay",
        "Test-H3PhysicalDamageFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: dodge, double attack, counter, and counter half-damage",
        "Test-H3AttackChainFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: successful airborne dodge and no-damage path",
        "Test-H3DodgeFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: lethal target rejects double and counter follow-ups",
        "Test-H3LethalFollowupFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: out-of-range target rejects counter follow-up",
        "Test-H3CounterRangeFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: sleeping target rejects counter follow-up",
        "Test-H3CounterSleepFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: stunned target rejects counter follow-up",
        "Test-H3CounterStunFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: same-side target rejects counter follow-up",
        "Test-H3CounterSameSideFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: Burst Rock rejects counter follow-up",
        "Test-H3CounterBurstRockFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: remaining special-enemy counter exclusions",
        "Test-H3CounterSpecialEnemiesFixture.ps1",
        needs_rom=True,
    ),
    LegacyStage(
        "H3: muddle and same-side reject double follow-up",
        "Test-H3DoubleValidationFixture.ps1",
        needs_rom=True,
    ),
)


def _heading(title: str) -> None:
    print(f"=== {title} ===", flush=True)


def _run_stage(stage: LegacyStage, rom_path: Path, upstream_path: Path) -> None:
    _heading(stage.title)
    arguments: list[str | Path] = []
    if stage.needs_rom:
        arguments.extend(("-RomPath", rom_path))
    if stage.needs_upstream:
        arguments.extend(("-UpstreamPath", upstream_path))
    run_powershell(stage.script, arguments)


def verify(
    *,
    rom_path: Path,
    upstream_path: Path,
    skip_rebuild: bool = False,
    skip_extraction: bool = False,
    skip_runtime: bool = False,
) -> None:
    _heading("Documentation: design-contract traceability")
    print_record(verify_design_contracts())
    _heading("Research: symbol, address, fixture, and document index")
    print_record(verify_index(upstream_path))
    _heading("H0: ROM baseline")
    print_record(verify_rom(rom_path))
    _heading("Toolchain provenance")
    print_record(verify_toolchain(upstream_path))

    if not skip_rebuild:
        _heading("H1: bit-perfect original rebuild")
        run_powershell(
            "Invoke-Sf2Rebuild.ps1",
            ("-RomPath", rom_path, "-UpstreamPath", upstream_path),
        )
    if not skip_extraction:
        for stage in H2_STAGES:
            _run_stage(stage, rom_path, upstream_path)
    if not skip_runtime:
        for stage in H3_STAGES:
            _run_stage(stage, rom_path, upstream_path)
    _heading("Repository verification: PASS")
