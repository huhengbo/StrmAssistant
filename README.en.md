# Strm Assistant Enhanced

![Strm Assistant Enhanced](StrmAssistant/Properties/thumb.png "Strm Assistant Enhanced")

[[中文]](README.md)

`Strm Assistant Enhanced` is the independently maintained derivative in this repository. It currently evolves from [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant) `v2.0.0.30`, with a focus on newer Emby Server compatibility, STRM media-information handling, and stability improvements.

> This is not an official upstream release. The existing plugin GUID is intentionally preserved for upgrade compatibility so current StrmAssistant installations can keep their configuration, while the logo, maintenance links, update source, and release information are maintained by this repository.

## Changes In This Build

- Adapted STRM mounting and path resolution for Emby 4.9, fixing media-information extraction on the newer API.
- Adapted media-source retrieval and external-subtitle scanning for newer Emby APIs.
- Prevented media-information JSON writes and deletions from triggering duplicate library-monitor events.
- Added a scheduled gap-check task that creates missing media-information JSON files for existing STRM items.
- Retained catch-up processing so newly added STRM items can automatically enter the media-information extraction queue.
- Updates are sourced from this repository's Releases and protected by download integrity validation and rollback safeguards.
- The build entry point is now a cross-platform Python script validated on Windows, macOS, and Linux CI runners.

## Verified Environment

The current build has been validated in an isolated Emby Server `4.9.3.0` container, including:

- Plugin loading and configuration page
- Extract MediaInfo / Persist MediaInfo
- STRM mounting and media probing
- Missing media-information JSON gap check
- External-subtitle scanning compatibility
- Automatic media-information catch-up for newly added STRM items

> Other Emby versions have not received the same level of end-to-end validation. Back up the existing plugin and configuration before upgrading or replacing it.

See [Emby 4.9 compatibility validation](docs/emby-4.9-compatibility-validation.md) for the detailed build and validation record.

## Installation

1. Download `StrmAssistantLite.dll` from this repository's Releases.
2. Copy it into the Emby Server `plugins` directory.
3. Restart Emby Server.
4. Confirm that `Strm Assistant Enhanced` is loaded on the Emby plugins page, then configure the required features.

## Update Security

The plugin updater reads releases from `huhengbo/StrmAssistant`. A custom GitHub download proxy never receives the configured GitHub token. After download, the plugin validates the release asset SHA-256 when GitHub provides a digest and keeps a `.bak` copy before replacing the active DLL. A failed download, validation, or replacement should therefore leave a recoverable existing plugin.

## Build

.NET SDK 8 and Python 3.9+ are required. Windows, macOS, and Linux use the same entry point:

```bash
python scripts/build_plugin.py
```

On Windows, Python Launcher can also be used:

```powershell
py scripts/build_plugin.py
```

If `dotnet` is not available through `PATH`, provide it through the environment or argument:

```bash
DOTNET_CMD=/path/to/dotnet python scripts/build_plugin.py
python scripts/build_plugin.py --dotnet /path/to/dotnet
```

The script restores packages, performs the Release build, runs compatibility tests, and produces `artifacts/StrmAssistantLite.dll` with its SHA-256 printed to the console. A normal build no longer writes to the actual Emby plugin directory; installation/replacement is a separate action.

## Versioning And Releases

Maintained releases use `YYYY.M.D.REVISION`, for example `2026.9.11.0`, with a matching tag such as `v2026.9.11.0`. See [CHANGELOG.md](CHANGELOG.md). The tag release workflow validates the version, builds and tests, generates a checksum, and creates the GitHub Release.

## Upstream, Original Work And License

The original Strm Assistant work belongs to the upstream author and contributors. This repository is a derivative compatibility and maintenance build; it does not claim ownership of the upstream project's original work and does not represent or replace an official upstream release.

- Maintained repository: `huhengbo/StrmAssistant`
- Upstream: [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant)
- Earlier origin: `faush01/StrmExtract`
- License: [GNU General Public License v3.0](LICENSE)

The new logo in this fork was created specifically to distinguish this maintained derivative. Upstream copyright attribution and GPL-3.0 obligations remain unchanged.

## Disclaimer

This project is not affiliated with, authorized by, or endorsed by Emby LLC. It contains no proprietary Emby components and is not intended to bypass Emby licensing, DRM, or paid features. Users are responsible for ensuring that their Emby Server installation and use comply with applicable licenses and laws.
