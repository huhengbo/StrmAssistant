#!/usr/bin/env python3
"""Cross-platform build entry point for Strm Assistant Enhanced."""

from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
import subprocess
import sys


def run(command: list[str], *, cwd: Path, timeout: int | None = None) -> None:
    print("+", " ".join(command), flush=True)
    subprocess.run(command, cwd=cwd, check=True, timeout=timeout)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


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
