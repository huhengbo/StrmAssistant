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
- Provides one reproducible build entry point for Windows, macOS, and Linux plus compatibility tests.

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

Python 3.10+ and .NET SDK 8 are required. Windows, macOS, and Linux all use the same entry point:

```bash
python scripts/build_plugin.py
```

If `dotnet` is not available through `PATH`:

```bash
python scripts/build_plugin.py --dotnet /path/to/dotnet
```

Unix environments may continue to use `./scripts/build-plugin.sh` as a compatibility wrapper. The build runs from a disposable copy under the system temporary directory so the legacy `Resource.Embedder` path workaround never writes backslash-named files or fake `%AppData%` folders into the working tree. Only the successful result is copied to `artifacts/StrmAssistantLite.dll`.

GitHub Actions executes this same build and test flow on Windows, macOS, and Linux.

## Upstream, Original Work And License

The original Strm Assistant work belongs to the upstream author and contributors. This repository is a derivative compatibility and maintenance build; it does not claim ownership of the upstream project's original work and does not represent or replace an official upstream release.

- Maintained repository: `huhengbo/StrmAssistant`
- Upstream: [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant)
- Earlier origin: `faush01/StrmExtract`
- License: [GNU General Public License v3.0](LICENSE)

The new logo in this fork was created specifically to distinguish this maintained derivative. Upstream copyright attribution and GPL-3.0 obligations remain unchanged.

## Disclaimer

This project is not affiliated with, authorized by, or endorsed by Emby LLC. It contains no proprietary Emby components and is not intended to bypass Emby licensing, DRM, or paid features. Users are responsible for ensuring that their Emby Server installation and use comply with applicable licenses and laws.
