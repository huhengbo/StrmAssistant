#!/usr/bin/env python3

from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile

RESOURCE_EMBEDDER_VERSION = "2.2.0"
ILREPACK_VERSION = "2.0.42"
ARTIFACT_NAME = "StrmAssistantLite.dll"


def run(command: list[str], *, cwd: Path, env: dict[str, str], timeout: int = 300) -> None:
    printable = " ".join(command)
    print(f"+ {printable}", flush=True)
    subprocess.run(command, cwd=cwd, env=env, check=True, timeout=timeout)


def copy_source(repo_root: Path, work_root: Path) -> None:
    ignored = shutil.ignore_patterns(
        ".git",
        ".idea",
        ".vs",
        "artifacts",
        "bin",
        "obj",
        "__pycache__",
        ".strmassistant-build.*",
    )
    shutil.copytree(repo_root, work_root, ignore=ignored)


def prepare_resource_embedder_compat(project_dir: Path, resource_core: Path) -> Path | None:
    if os.name == "nt":
        return None

    # Resource.Embedder 2.2.0 resolves its companion assembly using Windows
    # separators. Keep that compatibility artifact inside the disposable build
    # copy instead of writing a backslash-named symlink into the source tree.
    literal_windows_path = str(resource_core).lstrip(os.sep).replace("/", "\\")
    compatibility_link = project_dir / literal_windows_path
    compatibility_link.symlink_to(resource_core)
    return compatibility_link


def build(repo_root: Path, dotnet: str) -> Path:
    artifact_dir = repo_root / "artifacts"
    artifact_dir.mkdir(parents=True, exist_ok=True)
    final_artifact = artifact_dir / ARTIFACT_NAME

    with tempfile.TemporaryDirectory(prefix="strmassistant-build-") as temp_name:
        temp_root = Path(temp_name)
        work_root = temp_root / "repo"
        copy_source(repo_root, work_root)

        solution = work_root / "StrmAssistant.sln"
        project_dir = work_root / "StrmAssistant"
        build_output = temp_root / "output" / ARTIFACT_NAME
        build_output.parent.mkdir(parents=True, exist_ok=True)

        nuget_packages = temp_root / "nuget-packages"
        dotnet_cli_home = temp_root / "dotnet-home"
        env = os.environ.copy()
        env["NUGET_PACKAGES"] = str(nuget_packages)
        env["DOTNET_CLI_HOME"] = str(dotnet_cli_home)
        env.setdefault("DOTNET_NOLOGO", "1")
        env.setdefault("DOTNET_CLI_TELEMETRY_OPTOUT", "1")

        run([dotnet, "restore", str(solution)], cwd=work_root, env=env)

        resource_task = (
            nuget_packages
            / "resource.embedder"
            / RESOURCE_EMBEDDER_VERSION
            / "tasks"
            / "netstandard2.0"
            / "ResourceEmbedder.MsBuild.dll"
        )
        resource_core = (
            nuget_packages
            / "resource.embedder"
            / RESOURCE_EMBEDDER_VERSION
            / "tasks"
            / "netstandard2.0"
            / "ResourceEmbedder.Core.dll"
        )
        ilrepack = (
            nuget_packages
            / "ilrepack"
            / ILREPACK_VERSION
            / "tools"
            / "ILRepack.exe"
        )

        for required in (resource_task, resource_core, ilrepack):
            if not required.is_file():
                raise FileNotFoundError(f"Required build tool was not restored: {required}")

        prepare_resource_embedder_compat(project_dir, resource_core)

        ilrepack_command = f'{dotnet} "{ilrepack}"'
        run(
            [
                dotnet,
                "build",
                str(solution),
                "--configuration",
                "Release",
                "--no-restore",
                f"-p:TaskAssembly={resource_task}",
                f"-p:ILRepack={ilrepack_command}",
                f"-p:PluginOutputPath={build_output}",
            ],
            cwd=work_root,
            env=env,
        )

        run(
            [
                dotnet,
                "test",
                str(solution),
                "--configuration",
                "Release",
                "--no-restore",
                "--no-build",
            ],
            cwd=work_root,
            env=env,
            timeout=180,
        )

        if not build_output.is_file():
            raise FileNotFoundError(f"Plugin build did not produce {build_output}")

        shutil.copy2(build_output, final_artifact)

    digest = hashlib.sha256(final_artifact.read_bytes()).hexdigest()
    print(f"SHA256 {final_artifact.name}: {digest}")
    return final_artifact


def main() -> int:
    parser = argparse.ArgumentParser(description="Build and test Strm Assistant Enhanced")
    parser.add_argument("--dotnet", default=os.environ.get("DOTNET_CMD", "dotnet"))
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[1]
    build(repo_root, args.dotnet)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except subprocess.TimeoutExpired as exc:
        print(f"Build command timed out: {exc.cmd}", file=sys.stderr)
        raise SystemExit(124)
