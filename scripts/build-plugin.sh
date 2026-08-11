#!/usr/bin/env bash

set -euo pipefail

repo_root="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
solution="$repo_root/StrmAssistant.sln"
project_dir="$repo_root/StrmAssistant"
artifact_dir="$repo_root/artifacts"
generated_appdata="$project_dir/%AppData%"
build_temp="$(mktemp -d "$repo_root/.strmassistant-build.XXXXXX")"
dotnet_cmd="${DOTNET_CMD:-dotnet}"
resource_core_link=""
generated_appdata_owned=false

cleanup() {
  if [[ "$generated_appdata_owned" == true && -d "$generated_appdata" ]]; then
    find "$generated_appdata" -depth -delete
  fi
  if [[ -n "$resource_core_link" && -L "$resource_core_link" ]]; then
    unlink "$resource_core_link"
  fi
  if [[ -d "$build_temp" ]]; then
    find "$build_temp" -depth -delete
  fi
}
trap cleanup EXIT

if [[ -e "$generated_appdata" ]]; then
  printf 'Refusing to remove pre-existing build output: %s\n' "$generated_appdata" >&2
  exit 1
fi
mkdir "$generated_appdata"
generated_appdata_owned=true

nuget_packages="$build_temp/nuget-packages"
dotnet_cli_home="$build_temp/dotnet-home"

NUGET_PACKAGES="$nuget_packages" DOTNET_CLI_HOME="$dotnet_cli_home" \
  "$dotnet_cmd" restore "$solution"

resource_embedder_task="$nuget_packages/resource.embedder/2.2.0/tasks/netstandard2.0/ResourceEmbedder.MsBuild.dll"
resource_embedder_core="$nuget_packages/resource.embedder/2.2.0/tasks/netstandard2.0/ResourceEmbedder.Core.dll"
ilrepack="$nuget_packages/ilrepack/2.0.42/tools/ILRepack.exe"

test -f "$resource_embedder_task"
test -f "$resource_embedder_core"
test -f "$ilrepack"

# Resource.Embedder 2.2.0 also resolves its companion assembly with Windows
# separators. On Unix that becomes one literal filename relative to ProjectDir.
resource_core_windows_path="${resource_embedder_core#/}"
resource_core_windows_path="${resource_core_windows_path//\//\\}"
resource_core_link="$project_dir/$resource_core_windows_path"
ln -s "$resource_embedder_core" "$resource_core_link"
test -f "$resource_core_link"

NUGET_PACKAGES="$nuget_packages" DOTNET_CLI_HOME="$dotnet_cli_home" \
  "$dotnet_cmd" build "$solution" --configuration Release --no-restore \
  -p:TaskAssembly="$resource_embedder_task" \
  -p:ILRepack="$dotnet_cmd $ilrepack"

NUGET_PACKAGES="$nuget_packages" DOTNET_CLI_HOME="$dotnet_cli_home" \
  perl -e 'alarm shift; exec @ARGV or die "exec failed: $!\n"' 60 \
  "$dotnet_cmd" test "$solution" --configuration Release --no-restore --no-build

merged_plugin="$generated_appdata/Emby-Server/programdata/plugins/StrmAssistantLite.dll"
test -f "$merged_plugin"
mkdir -p "$artifact_dir"
cp "$merged_plugin" "$artifact_dir/StrmAssistantLite.dll"

shasum -a 256 "$artifact_dir/StrmAssistantLite.dll"
