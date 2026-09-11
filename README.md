# Strm Assistant Enhanced

![Strm Assistant Enhanced](StrmAssistant/Properties/thumb.png "Strm Assistant Enhanced")

[[English]](README.en.md)

`Strm Assistant Enhanced` 是由本仓库维护的 StrmAssistant 二次开发版本，当前基于 [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant) `v2.0.0.30` 演进，重点维护新版 Emby Server 兼容性、STRM 媒体信息处理与稳定性增强。

> 本项目不是上游官方发布。插件 GUID 暂时保持兼容，以便已有 StrmAssistant 用户直接升级并保留原有配置；项目 Logo、维护链接、更新源和发布信息由本仓库独立维护。

## 本版本主要改动

- 适配 Emby 4.9 的 STRM 媒体挂载与路径解析，修复新版 Emby 中媒体信息提取失败的问题。
- 适配新版 Emby 的媒体源读取和外挂字幕扫描能力。
- 修复媒体信息 JSON 写入或删除时触发重复媒体库监听的问题。
- 新增 STRM 媒体信息 JSON 缺失检查与补漏任务，可为已入库但缺少持久化媒体信息的 STRM 逐项补齐。
- 保留追更模式，可在新增 STRM 入库后自动进入媒体信息提取队列。
- 插件自更新固定从本仓库 Releases 获取，并对下载文件进行完整性校验与失败回滚保护。
- 补充 Linux/macOS 环境下的可复现构建流程和兼容性自动测试。

## 验证环境

当前版本已在隔离的 Emby Server `4.9.3.0` 容器中验证，覆盖：

- 插件加载和配置页显示
- Extract MediaInfo / Persist MediaInfo
- STRM 媒体信息挂载与探测
- 媒体信息 JSON 缺失检查与补漏
- 外挂字幕扫描兼容
- 新增 STRM 的媒体信息自动追更

> 其他 Emby 版本尚未进行同等强度的完整验证，请在升级或替换插件前备份现有插件和配置。

详细构建与验证记录见 [Emby 4.9 兼容性验证](docs/emby-4.9-compatibility-validation.md)。

## 安装

1. 从本仓库 Releases 下载 `StrmAssistantLite.dll`。
2. 将文件放入 Emby Server 的 `plugins` 目录。
3. 重启 Emby Server。
4. 在 Emby 插件页确认 `Strm Assistant Enhanced` 已加载，再按需配置功能。

## 更新与安全

插件的自动更新源为本仓库 `huhengbo/StrmAssistant` 的 GitHub Releases。自定义 GitHub 下载代理不会收到 GitHub Token；下载完成后会校验 Release asset 的 SHA-256（GitHub 提供 digest 时），并在覆盖插件前保留 `.bak` 备份。如果下载、校验或替换失败，当前插件不会被静默破坏。

## 构建

仓库当前提供可重复执行的构建脚本，需要 .NET SDK 8：

```bash
./scripts/build-plugin.sh
```

如果 `dotnet` 不在 `PATH` 中，可显式指定：

```bash
DOTNET_CMD=/path/to/dotnet ./scripts/build-plugin.sh
```

构建会同时运行兼容性测试，产物位于 `artifacts/StrmAssistantLite.dll`。

> 跨 Windows / macOS / Linux 的统一构建链路仍在继续收口，详见仓库 Issues。

## 上游、原创与授权

Strm Assistant 的原创工作归上游作者及项目贡献者所有。本仓库是二次开发和兼容维护版本，不声称拥有上游项目的原创成果，也不代表或替代上游官方发布。

- 当前维护仓库：`huhengbo/StrmAssistant`
- 上游项目：[sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant)
- 更早来源：`faush01/StrmExtract`
- 许可证：[GNU General Public License v3.0](LICENSE)

本 fork 的新 Logo 为本维护版本重新设计，仅用于区分当前二开发布；上游版权与 GPL-3.0 义务保持不变。

## 免责声明

本项目与 Emby LLC 没有任何关联，也未获得 Emby LLC 的授权或认可。本项目不包含 Emby 专有组件，不用于绕过 Emby 授权、DRM 或解锁付费功能。使用者需自行确保对 Emby Server 的安装和使用符合许可协议及所在地法律法规。
