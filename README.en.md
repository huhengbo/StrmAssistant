# Strm Assistant Emby 4.9 Compatibility Build

![logo](StrmAssistant/Properties/thumb.png "logo")

[[中文]](README.md)

This is a personal compatibility build based on [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant) `v2.0.0.30`.

This repository preserves the upstream features, project structure, and GPL-3.0 license while adding compatibility for newer Emby Server releases and media-information gap recovery for STRM libraries. Refer to the upstream repository for the complete feature documentation and update history.

## Changes In This Build

- Adapted STRM mounting and path resolution for Emby 4.9, fixing media-information extraction on the newer API.
- Adapted media-source retrieval and external-subtitle scanning for the newer Emby API.
- Prevented media-information JSON writes and deletions from triggering duplicate library-monitor events.
- Added a scheduled gap-check task that creates missing media-information JSON files for existing STRM items.
- Retained catch-up processing so newly added STRM items can automatically enter the media-information extraction queue.
- Added a reproducible Linux/macOS build workflow and automated compatibility tests.

## Verified Environment

This compatibility build has been validated in an isolated Emby Server `4.9.3.0` container.

Verified areas include:

- Plugin loading and configuration page
- Extract MediaInfo
- Persist MediaInfo
- STRM mounting and media probing
- Missing media-information JSON gap check
- External-subtitle scanning compatibility
- Automatic media-information catch-up for newly added STRM items

> Other Emby versions have not received the same level of end-to-end validation. Back up the existing plugin and configuration before upgrading or replacing it.

See [Emby 4.9 compatibility validation](docs/emby-4.9-compatibility-validation.md) for the detailed build and validation record.

## Installation

1. Download `StrmAssistantLite.dll` from Releases.
2. Copy it into the Emby Server `plugins` directory.
3. Restart Emby Server.
4. Confirm that `Strm Assistant` is loaded on the Emby plugins page, then configure the required features.

## Build

.NET SDK 8 is required. Run the reproducible build script:

```bash
./scripts/build-plugin.sh
```

If `dotnet` is not available through `PATH`, specify it explicitly:

```bash
DOTNET_CMD=/path/to/dotnet ./scripts/build-plugin.sh
```

The script also runs the compatibility tests and writes the merged plugin to `artifacts/StrmAssistantLite.dll`.

## Original Work And License

The original work of Strm Assistant belongs to the upstream author and contributors. This repository is a personal derivative and compatibility build. It does not claim ownership of the upstream project's original work and does not represent or replace an official upstream release.

This project remains available under the upstream [GNU General Public License v3.0](LICENSE). For the complete feature set, licensing details, usage documentation, and upstream support, visit:

[https://github.com/sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant)

## Disclaimer

This project is not affiliated with, authorized by, or endorsed by Emby LLC. It contains no proprietary Emby components and is not intended to bypass Emby licensing, DRM, or paid features. Users are responsible for ensuring that their Emby Server installation and use comply with applicable licenses and laws.
