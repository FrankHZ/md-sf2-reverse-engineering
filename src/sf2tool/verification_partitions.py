"""Shared partition record and standalone managed tool build entries."""

from dataclasses import dataclass


@dataclass(frozen=True)
class VerificationPartition:
    """One stable scheduling and maintenance boundary."""

    partition_id: str
    layer: str
    description: str
    commands: tuple[str, ...]
    parallel_safe: bool = True
    resource_lock: str | None = None
    external_gates: tuple[str, ...] = ()


TOOL_BUILD_PARTITIONS = tuple(
    VerificationPartition(
        partition,
        "remake",
        f"Compile {project} from remake/ after SDK setup; no attachment or private input in CI.",
        (
            f"& $env:DOTNET_BIN build tools/{project}/{project}.csproj "
            "--configuration Release -p:RestoreLockedMode=true",
        ),
        external_gates=(f"Public checks / {partition}",),
    )
    for partition, project in (
        ("private-admission-build", "Sf2.Remake.PrivateAdmission"),
        ("live-inspection-build", "Sf2.Remake.LiveInspection"),
    )
)
