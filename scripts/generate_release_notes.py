#!/usr/bin/env python3
"""Generate GitHub Release notes from one version section in CHANGELOG.md."""

from __future__ import annotations

import argparse
import re
from pathlib import Path


def extract_version_section(changelog: str, version: str) -> str:
    lines = changelog.splitlines()
    heading = re.compile(rf"^##\s+v?{re.escape(version)}\s*$", re.IGNORECASE)
    next_heading = re.compile(r"^##\s+")

    start = None
    for index, line in enumerate(lines):
        if heading.match(line.strip()):
            start = index + 1
            break

    if start is None:
        raise ValueError(f"CHANGELOG.md 中未找到版本章节: {version}")

    end = len(lines)
    for index in range(start, len(lines)):
        if next_heading.match(lines[index]):
            end = index
            break

    body = "\n".join(lines[start:end]).strip()
    if not body:
        raise ValueError(f"CHANGELOG.md 的版本章节为空: {version}")

    return body


def main() -> None:
    parser = argparse.ArgumentParser(description="从 CHANGELOG.md 生成中文 GitHub Release Notes")
    parser.add_argument("--version", required=True, help="版本号，例如 2026.9.11.0")
    parser.add_argument("--changelog", default="CHANGELOG.md", help="CHANGELOG 文件路径")
    parser.add_argument("--output", required=True, help="输出 Markdown 文件路径")
    args = parser.parse_args()

    changelog_path = Path(args.changelog)
    output_path = Path(args.output)
    content = changelog_path.read_text(encoding="utf-8")
    section = extract_version_section(content, args.version)

    notes = (
        f"> 本发布说明由 `CHANGELOG.md` 的 `{args.version}` 版本章节自动生成。\n\n"
        f"{section}\n"
    )
    output_path.write_text(notes, encoding="utf-8")
    print(f"Release notes generated: {output_path}")


if __name__ == "__main__":
    main()
