# Changelog

All maintained-fork releases are documented here.

## Versioning

`Strm Assistant Enhanced` uses calendar versions in the form `YYYY.M.D.REVISION`.

- `YYYY.M.D` identifies the release date.
- `REVISION` starts at `0` and increments for additional releases on the same day.
- Git tags use the same value prefixed with `v`, for example `v2026.9.11.0`.
- This deliberately separates the maintained fork from the upstream `2.0.0.x` version sequence while remaining compatible with `System.Version` used by the in-plugin updater.

## 2026.9.11.0

Upstream baseline: `sjtuross/StrmAssistant` `v2.0.0.30`.

### Added

- Emby 4.9 STRM mount/path compatibility.
- STRM media-information JSON gap-check task.
- Compatibility tests for legacy/current media mount contracts.
- Fork-specific branding and plugin logo.
- Updater payload validation against GitHub Release SHA-256 digests.
- Updater backup/rollback protection and GitHub proxy token isolation.

### Changed

- Plugin display identity is now `Strm Assistant Enhanced`.
- Project, documentation, disclaimer, and update links point to `huhengbo/StrmAssistant`.
- Upstream attribution remains available explicitly through README and the plugin About page.

### Compatibility

- Plugin GUID remains `63c322b7-a371-41a3-b11f-04f8418b37d8` to preserve upgrade/configuration compatibility.
- Output assembly remains `StrmAssistantLite.dll`.
