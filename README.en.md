<p align="center">
  <img src="StrmAssistant/Properties/thumb.png" alt="Strm Assistant Enhanced" width="160" />
</p>

<h1 align="center">Strm Assistant Enhanced</h1>

<p align="center">
  An STRM-focused enhancement plugin for Emby Server, providing media-info extraction and persistence, gap recovery, subtitle scanning, intro/credits support, Emby Web external playback, and compatibility improvements.
</p>

<p align="center">
  <a href="README.md">简体中文</a> ·
  <a href="https://github.com/huhengbo/StrmAssistant/releases">Releases</a> ·
  <a href="CHANGELOG.md">Changelog</a> ·
  <a href="https://github.com/huhengbo/StrmAssistant/issues">Issues</a>
</p>

<p align="center">
  <a href="https://github.com/huhengbo/StrmAssistant/actions/workflows/build.yml"><img src="https://github.com/huhengbo/StrmAssistant/actions/workflows/build.yml/badge.svg" alt="Build" /></a>
  <a href="https://github.com/huhengbo/StrmAssistant/releases/latest"><img src="https://img.shields.io/github/v/release/huhengbo/StrmAssistant?display_name=tag" alt="Release" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/huhengbo/StrmAssistant" alt="License" /></a>
  <img src="https://img.shields.io/badge/Emby-4.9.x-52B54B" alt="Emby 4.9.x" />
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-blue" alt="Platforms" />
</p>

> [!IMPORTANT]
> This repository is independently maintained. The existing plugin GUID is intentionally preserved so current users can upgrade without losing their configuration.

## Features

- **STRM MediaInfo extraction and persistence**
  - Extract media information and persist it as JSON.
  - Restore media information from persisted data.
  - Automatically process newly added STRM items through catch-up mode.

- **Missing MediaInfo recovery**
  - Detect STRM items that are missing MediaInfo JSON.
  - Extract and recreate missing information automatically.
  - Page through large libraries to avoid loading the entire candidate set at once.

- **Emby 4.9 compatibility improvements**
  - Adapt STRM mounting, path resolution, and media-source retrieval.
  - Support external subtitle scanning against newer APIs.
  - Match private API reflection calls by complete parameter and return-type contracts.

- **Subtitle and playback enhancements**
  - Scan and update external subtitle media information.
  - Preserve intro / credits detection capabilities.
  - Retain existing multi-version media enhancements.

- **Emby Web external playback**
  - Adds an External Player action to item details and overflow/context menus while reusing Emby's native button and action-sheet patterns where available.
  - Supports PotPlayer, VLC, MPV, IINA, Infuse, and copying the stream URL, with obvious unsupported players filtered by operating system.
  - Uses the currently selected MediaSource and passes external subtitles plus the current Emby resume position where the target player supports it.
  - Series uses Next Up and Season uses the first playable Episode.
  - The feature can be disabled globally from Experience Enhance settings. STRM Direct is also controlled from plugin settings and remains disabled by default.

- **Safer self-update flow**
  - Updates are sourced only from this repository's GitHub Releases.
  - Custom GitHub proxies never receive the configured GitHub token.
  - SHA-256 verification, DLL backup, and rollback protection are supported.

- **Cross-platform build and release automation**
  - One build entry point for Windows, macOS, and Linux.
  - GitHub Actions builds and tests on all three platforms.
  - Releases automatically publish the DLL, checksum file, and Chinese release notes.

## Compatibility

The actively maintained target is **Emby Server 4.9.x**.

| Area | Status |
| --- | --- |
| Emby Server `4.9.3.0` | ✅ Verified |
| Plugin loading and configuration | ✅ |
| MediaInfo extraction / persistence | ✅ |
| STRM mounting and media probing | ✅ |
| Missing MediaInfo recovery | ✅ |
| External subtitle scanning | ✅ |
| Automatic catch-up for new STRM items | ✅ |
| Emby Web external playback | ✅ Emby 4.9 Web |

Other Emby versions have not received the same level of end-to-end validation. Back up the existing plugin DLL and configuration before upgrading.

See [Emby 4.9 compatibility validation](docs/emby-4.9-compatibility-validation.md) for details.

## Installation

### Install from a Release

1. Open the [latest Release](https://github.com/huhengbo/StrmAssistant/releases/latest).
2. Download `StrmAssistantLite.dll`.
3. Copy it into the Emby Server `plugins` directory.
4. Restart Emby Server.
5. Confirm that **Strm Assistant Enhanced** is loaded in the plugin page.

Existing StrmAssistant users can replace the DLL directly. The plugin GUID remains:

```text
63c322b7-a371-41a3-b11f-04f8418b37d8
```

Existing plugin configuration is therefore preserved.

### Verify the download

Each stable Release includes:

```text
StrmAssistantLite.dll
StrmAssistantLite.dll.sha256
```

Use your platform's SHA-256 tooling to verify the downloaded DLL when needed.

## Usage

After installation, open the plugin configuration page in Emby Server and enable the features you need.

For first-time use:

1. Verify the configured library scope.
2. Run the missing MediaInfo recovery task for existing STRM libraries if needed.
3. Test MediaInfo extraction and persistence on a small subset before enabling automatic catch-up.
4. Back up Emby data and plugin configuration before large-scale processing.

### Emby Web external playback

1. In **Experience Enhance**, enable **Web 外部播放 / External Player**. It defaults to enabled to preserve the behavior of existing installations.
2. Open a Movie / Episode / Series / Season and use External Player from the detail page or the item's overflow/context menu.
3. The plugin presents supported candidates for the current OS: PotPlayer, VLC, MPV, IINA, Infuse, plus Copy Stream URL.
4. External players must already be installed on the client device and have their URL scheme / protocol handler registered. The plugin only generates and launches the playback URL.
5. **STRM Direct** is disabled by default. When enabled, HTTP/HTTPS STRM items pass their original URL directly to the external player; otherwise the Emby stream URL is used.
6. Resume position is passed one-way where supported. External-player playback progress is **not written back to Emby**.

Refresh the Emby Web page after changing external-player plugin settings so the Web client reloads the current configuration.

Additional documentation will continue to be added under `docs/`.

## Build from Source

### Requirements

- Python 3.10+
- .NET SDK 8
- Git

### Build

Windows, macOS, and Linux use the same entry point:

```bash
python scripts/build_plugin.py
```

If `dotnet` is not available through `PATH`:

```bash
python scripts/build_plugin.py --dotnet /path/to/dotnet
```

Unix users may also use the compatibility wrapper:

```bash
./scripts/build-plugin.sh
```

After a successful build and test run, the plugin is written to:

```text
artifacts/StrmAssistantLite.dll
```

The build runs from a disposable source copy under the system temporary directory so legacy `Resource.Embedder` path workarounds do not pollute the working tree.

## Development and Testing

GitHub Actions runs the same build and test flow on:

- `ubuntu-latest`
- `macos-latest`
- `windows-latest`

Current automated tests focus on:

- Emby media-mount compatibility contracts.
- Plugin updater security behavior.
- Reflection contract matching for Emby private APIs.
- Embedded Emby Web JavaScript syntax checks.

Before submitting changes, run at least:

```bash
python scripts/build_plugin.py
```

## Versioning and Releases

This project uses calendar versions:

```text
YYYY.M.D.REVISION
```

Example:

```text
2026.9.11.0
```

Release information is maintained only in [CHANGELOG.md](CHANGELOG.md). The release workflow extracts the matching version section and uses it as the Chinese GitHub Release notes.

Stable release commits use:

```text
release: vYYYY.M.D.REVISION
```

The release pipeline validates versions, generates release notes, builds and tests the plugin, creates a SHA-256 checksum, creates the Git tag and GitHub Release, and uploads the release assets.

## Contributing

Issues, suggestions, and pull requests are welcome.

Recommended contribution flow:

1. Fork the repository.
2. Create a focused branch from the latest `main`.
3. Keep each pull request scoped to one clear problem.
4. Add automated tests for new behavior or compatibility fixes where practical.
5. Make sure the build flow passes locally before submitting.

When reporting issues, include the Emby version, plugin version, operating system or deployment method, reproduction steps, and relevant logs. Remove tokens, account data, local paths, and other sensitive information first.

## Project Information

- Repository: [huhengbo/StrmAssistant](https://github.com/huhengbo/StrmAssistant)
- License: [GNU General Public License v3.0](LICENSE)

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).

## Disclaimer

This project is not affiliated with, authorized by, or endorsed by Emby LLC.

It contains no proprietary Emby components and is not intended to bypass Emby licensing, DRM, or paid features. Users are responsible for ensuring that their Emby Server installation and use comply with applicable licenses and laws.
