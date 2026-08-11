# Emby 4.9 compatibility validation

This record is intentionally free of credentials, access tokens, and media-library item names.

## Source and baseline build

- Upstream: `sjtuross/StrmAssistant`
- Baseline commit: `beb65cf8e4d7b19ce418c3aa32cfb6eff04acfb2` (`v2.0.0.30`)
- Development branch: `codex/emby-new-api-compat`
- SDK: .NET SDK `8.0.423`
- MSBuild: `17.11.48`
- Direct NuGet dependencies: `CHTCHSConv 1.0.0`, `ILRepack 2.0.42`,
  `mediabrowser.server.core 4.8.0.80`, `Microsoft.SourceLink.GitHub 8.0.0`,
  and `Resource.Embedder 2.2.0`
- Unmodified merged plugin: 700,416 bytes
- Unmodified SHA-256:
  `6bfd540883ee4dc464f632e2a7892074bded256a110431d6eeb6013af2f9e0ed`

The original project has two Windows-specific build assumptions: Resource.Embedder 2.2.0
uses backslashes in its MSBuild task path, and the post-build ILRepack command invokes a
Windows executable while writing below `%AppData%`. The baseline was built without changing
tracked source by supplying normalized `TaskAssembly` and `ILRepack` MSBuild properties.
The same process is automated for the compatibility branch:

```bash
DOTNET_CMD=/path/to/dotnet ./scripts/build-plugin.sh
```

The script restores into a disposable directory, creates and removes the one Unix compatibility
link required by Resource.Embedder, runs the Release solution build and tests, removes the
original post-build directory, and writes the merged plugin to `artifacts/StrmAssistantLite.dll`.

## Isolated test instance

- Container: `emby-18` only
- Emby version: `4.9.3.0`
- Image: `amilys/embyserver` with inspected digest beginning `sha256:5db19f5a`
- Program-data mount: `/root/apps/emby-18/config` to `/config`
- Media mount used by the test: `/root/media/strm` to `/mnt`

No production directory or production container was used. No Emby database was read or
modified directly. Every plugin deployment first created a timestamped backup of the test
plugin and its configuration. Those temporary validation backups were removed after the
test work was completed.

## Reproduced failure and root cause

The unmodified plugin loaded, but its startup log reported two `MediaInfoApi Init Failed`
warnings and one `SubtitleApi Init Failed` warning. Running Extract MediaInfo against the
test STRM set failed for all 1,137 selected items with:

```text
Method not found: System.String MediaBrowser.Model.IO.IMediaMount.get_MountedPath()
```

Inspection of the actual test-container assemblies showed that the old contract used
`Mount(ReadOnlyMemory<char>, ...)` and `IMediaMount.MountedPath`, whereas Emby 4.9.3.0 uses
`Mount(string, ...)` and `IMediaMount.MountedPathInfo.FullName`. The public ODJ0930 patches
`395b88f3941be3a0d51f8303bddce3edef42f402` and
`c33dbb57b811eac074779ba1e311f7dc66aaddde` were used only as comparison evidence.

## Compatibility result

- Compatible merged plugin SHA-256:
  `671c3df709716fc0b70cc4d56800b75ea45f69d0afd9e8ce04628a8d22f30eb8`
- Plugin loaded without the three baseline initialization warnings.
- The configuration page was opened through a real authenticated browser session and rendered.
- Extract MediaInfo changed the isolated STRM test item from zero streams to H.264 video
  (320x180) plus AAC audio, with MP4 container and a two-second duration.
- Persist MediaInfo wrote a JSON document containing duration, container, and both streams.
- `CheckMissingMediaInfoTask` was registered and filled a missing JSON after mounting and
  probing an isolated STRM item.
- The final gap-check run serialized the item exactly once; the resulting JSON was 1,732 bytes.
- The compatible deployment produced zero `MountedPath` method errors.

After validation, 147 JSON documents created by the intentionally broad Persist task were
identified from successful `overwrite:false` log entries, checked to be below the isolated
test media root with the `-mediainfo.json` suffix, and removed. The isolated test media and
probe directories were removed, followed by an Emby API library refresh. After the
post-review build was deployed, a final smoke check confirmed that the task remained
registered, startup contained no `MediaInfoApi` or `SubtitleApi` initialization warnings,
there were no legacy `MountedPath` errors, and none of the 147 cleanup targets had reappeared.
The temporary deployment backups were subsequently removed at the user's request.
