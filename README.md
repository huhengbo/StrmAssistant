<p align="center">
  <img src="StrmAssistant/Properties/thumb.png" alt="Strm Assistant Enhanced" width="160" />
</p>

<h1 align="center">Strm Assistant Enhanced</h1>

<p align="center">
  面向 Emby Server 的 STRM 媒体库增强插件，提供媒体信息提取与持久化、缺失补漏、字幕扫描、片头片尾检测及兼容性增强。
</p>

<p align="center">
  <a href="README.en.md">English</a> ·
  <a href="https://github.com/huhengbo/StrmAssistant/releases">Releases</a> ·
  <a href="CHANGELOG.md">更新日志</a> ·
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
> 本项目由本仓库独立维护。当前保留原插件 GUID，以便已有用户直接升级并继续使用原配置。

## 功能特性

- **STRM MediaInfo 提取与持久化**
  - 提取媒体信息并持久化为 JSON。
  - 支持从已持久化数据恢复媒体信息。
  - 支持新增 STRM 的自动追更处理。

- **STRM 媒体信息缺失补漏**
  - 检查已有 STRM 项目是否缺少 MediaInfo JSON。
  - 自动提取并补齐缺失信息。
  - 大媒体库采用分页扫描，降低一次性内存占用。

- **Emby 4.9 兼容增强**
  - 适配 STRM 媒体挂载、路径解析与媒体源读取变化。
  - 外挂字幕扫描适配新版 API。
  - 私有 API 反射调用采用完整参数与返回类型匹配，降低版本变化后的误调用风险。

- **字幕与播放体验增强**
  - 扫描并更新外挂字幕媒体信息。
  - 保留片头 / 片尾检测相关能力。
  - 支持多版本媒体整理等现有增强功能。

- **安全的插件自更新**
  - 更新源固定为本仓库 GitHub Releases。
  - 自定义 GitHub Proxy 不会收到 GitHub Token。
  - 支持 SHA-256 完整性校验、旧 DLL 备份与失败回滚。

- **跨平台构建与自动发布**
  - Windows / macOS / Linux 使用统一构建入口。
  - GitHub Actions 三平台自动构建与测试。
  - Release 自动生成 DLL、SHA-256 校验文件和中文发布说明。

## 兼容性

当前主要维护 **Emby Server 4.9.x**。

已重点验证的环境：

| 项目 | 状态 |
| --- | --- |
| Emby Server `4.9.3.0` | ✅ 已验证 |
| 插件加载与配置页 | ✅ |
| MediaInfo 提取 / 持久化 | ✅ |
| STRM 挂载与媒体探测 | ✅ |
| MediaInfo 缺失补漏 | ✅ |
| 外挂字幕扫描 | ✅ |
| 新增 STRM 自动追更 | ✅ |

其他 Emby 版本没有进行同等强度的完整验证，升级或替换插件前建议备份现有 DLL 与配置。

详细记录见：[Emby 4.9 兼容性验证](docs/emby-4.9-compatibility-validation.md)。

## 安装

### 从 Release 安装

1. 打开 [Releases](https://github.com/huhengbo/StrmAssistant/releases/latest)。
2. 下载 `StrmAssistantLite.dll`。
3. 将 DLL 放入 Emby Server 的 `plugins` 目录。
4. 重启 Emby Server。
5. 在插件页确认 **Strm Assistant Enhanced** 已成功加载。

已有 StrmAssistant 用户可以直接覆盖升级。当前插件 GUID 保持为：

```text
63c322b7-a371-41a3-b11f-04f8418b37d8
```

因此原有插件配置可以继续保留。

### 文件校验

每个正式 Release 同时提供：

```text
StrmAssistantLite.dll
StrmAssistantLite.dll.sha256
```

可使用对应平台的 SHA-256 工具验证下载文件完整性。

## 使用说明

安装后进入 Emby Server 的插件配置页，根据需要启用相关功能。

建议初次使用时：

1. 先确认媒体库范围配置正确。
2. 对已有 STRM 媒体，可先运行 MediaInfo 缺失补漏任务。
3. 开启自动追更前，先在少量媒体上验证 MediaInfo 提取与持久化结果。
4. 执行大规模处理前，建议备份 Emby 数据与插件配置。

更完整的功能说明会逐步补充到 `docs/`。

## 本地构建

### 环境要求

- Python 3.10+
- .NET SDK 8
- Git

### 构建命令

Windows、macOS、Linux 使用相同入口：

```bash
python scripts/build_plugin.py
```

如果 `dotnet` 不在 `PATH` 中：

```bash
python scripts/build_plugin.py --dotnet /path/to/dotnet
```

Unix 环境仍可使用兼容包装器：

```bash
./scripts/build-plugin.sh
```

构建与测试成功后，最终产物位于：

```text
artifacts/StrmAssistantLite.dll
```

构建过程在系统临时目录中的一次性源码副本执行，避免旧版 `Resource.Embedder` 的路径兼容逻辑污染工作区。

## 开发与测试

GitHub Actions 会在以下环境执行统一构建和测试流程：

- `ubuntu-latest`
- `macos-latest`
- `windows-latest`

当前自动测试重点覆盖：

- Emby Media Mount 兼容契约。
- 插件自动更新安全逻辑。
- Emby 私有 API 反射方法契约匹配。

提交改动前建议至少执行：

```bash
python scripts/build_plugin.py
```

## 版本与发布

本项目使用日期版本号：

```text
YYYY.M.D.REVISION
```

示例：

```text
2026.9.11.0
```

发布信息只维护在 [CHANGELOG.md](CHANGELOG.md)。Release workflow 会自动提取对应版本章节，并生成中文 GitHub Release Notes。

正式发布提交使用：

```text
release: vYYYY.M.D.REVISION
```

自动发布流程包括：

1. 校验 `AssemblyVersion` / `FileVersion`。
2. 生成中文 Release Notes。
3. 构建并执行测试。
4. 生成 SHA-256 校验文件。
5. 创建 Git tag 和 GitHub Release。
6. 上传 `StrmAssistantLite.dll` 与 `.sha256`。

## 贡献

欢迎提交 Issue、改进建议和 Pull Request。

建议贡献流程：

1. Fork 本仓库。
2. 从最新 `main` 创建功能分支。
3. 保持单个 PR 聚焦于一个明确问题。
4. 新功能或兼容性修复尽量补充自动测试。
5. 提交前确保跨平台构建流程至少在本地通过。

报告问题时建议附带：

- Emby Server 版本。
- 插件版本。
- 操作系统与部署方式。
- 可复现步骤。
- 相关日志（请先移除 Token、账号、路径等敏感信息）。

## 项目信息

- 当前仓库：[huhengbo/StrmAssistant](https://github.com/huhengbo/StrmAssistant)
- 许可证：[GNU General Public License v3.0](LICENSE)

## License

本项目按照 [GNU General Public License v3.0](LICENSE) 发布。

## 免责声明

本项目与 Emby LLC 没有任何关联，也未获得 Emby LLC 的授权或认可。

本项目不包含 Emby 专有组件，不用于绕过 Emby 授权、DRM 或解锁付费功能。使用者需自行确保 Emby Server 的安装和使用符合相应许可协议及所在地法律法规。
