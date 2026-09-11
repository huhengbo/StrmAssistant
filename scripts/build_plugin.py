#!/usr/bin/env python3
"""Cross-platform build entry point for Strm Assistant Enhanced."""

from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
import subprocess
import sys

RESOURCE_EMBEDDER_VERSION = "2.2.0"


def run(
    command: list[str],
    *,
    cwd: Path,
    timeout: int | None = None,
    capture_output: bool = False,
) -> subprocess.CompletedProcess[str]:
    print("+", " ".join(command), flush=True)
    return subprocess.run(
        command,
        cwd=cwd,
        check=True,
        timeout=timeout,
        capture_output=capture_output,
        text=True,
    )


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def nuget_global_packages(dotnet: str, repo_root: Path) -> Path:
    result = run(
        [dotnet, "nuget", "locals", "global-packages", "--list"],
        cwd=repo_root,
        capture_output=True,
    )
    line = next((item for item in result.stdout.splitlines() if ":" in item), "")
    if not line:
        raise RuntimeError("Unable to determine the NuGet global-packages directory")
    return Path(line.split(":", 1)[1].strip()).expanduser().resolve()


def prepare_resource_embedder_compat(dotnet: str, repo_root: Path) -> Path | None:
    """Work around Resource.Embedder 2.2.0's Unix backslash companion-path lookup.

    Resource.Embedder itself is kept because it preserves the current single-DLL satellite
    resource behavior. The temporary compatibility link is required only on Unix-like hosts
    and is removed before the build command exits.
    """

    if os.name == "nt":
        return None

    package_root = nuget_global_packages(dotnet, repo_root)
    core = (
        package_root
        / "resource.embedder"
        / RESOURCE_EMBEDDER_VERSION
        / "tasks"
        / "netstandard2.0"
        / "ResourceEmbedder.Core.dll"
    )
    if not core.is_file():
        raise RuntimeError(f"Resource.Embedder companion assembly not found: {core}")

    project_dir = repo_root / "StrmAssistant"
    literal_windows_name = str(core).lstrip("/").replace("/", "\\")
    compat_link = project_dir / literal_windows_name

    if compat_link.exists() or compat_link.is_symlink():
        raise RuntimeError(f"Refusing to replace pre-existing Resource.Embedder compatibility path: {compat_link}")

    compat_link.symlink_to(core)
    return compat_link


def main() -> int:
    parser = argparse.ArgumentParser(description="Build and test the Emby plugin on Windows, macOS, or Linux.")
    parser.add_argument("--dotnet", default=os.environ.get("DOTNET_CMD", "dotnet"), help="dotnet executable")
    parser.add_argument("--configuration", default="Release", help="MSBuild configuration")
    parser.add_argument("--skip-tests", action="store_true", help="build without running compatibility tests")
    parser.add_argument("--test-timeout", type=int, default=120, help="test command timeout in seconds")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[1]
    solution = repo_root / "StrmAssistant.sln"
    artifact = repo_root / "artifacts" / "StrmAssistantLite.dll"

    artifact.parent.mkdir(parents=True, exist_ok=True)
    if artifact.exists():
        artifact.unlink()

    run([args.dotnet, "restore", str(solution)], cwd=repo_root)
    compat_link = prepare_resource_embedder_compat(args.dotnet, repo_root)

    try:
        run(
            [args.dotnet, "build", str(solution), "--configuration", args.configuration, "--no-restore"],
            cwd=repo_root,
        )

        if not args.skip_tests:
            run(
                [
                    args.dotnet,
                    "test",
                    str(solution),
                    "--configuration",
                    args.configuration,
                    "--no-restore",
                    "--no-build",
                ],
                cwd=repo_root,
                timeout=args.test_timeout,
            )
    finally:
        if compat_link is not None and compat_link.is_symlink():
            compat_link.unlink()

    if not artifact.is_file() or artifact.stat().st_size == 0:
        raise RuntimeError(f"Plugin artifact was not produced: {artifact}")

    print(f"artifact: {artifact}")
    print(f"sha256:   {sha256(artifact)}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except subprocess.TimeoutExpired as exc:
        print(f"command timed out after {exc.timeout}s: {exc.cmd}", file=sys.stderr)
        raise SystemExit(124)
