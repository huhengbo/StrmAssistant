# Strm Assistant Emby 4.9 兼容构建

![logo](StrmAssistant/Properties/thumb.png "logo")

[[English]](README.en.md)

这是基于原项目 [sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant) `v2.0.0.30` 进行二次开发的个人兼容构建。

本仓库保留原项目的功能、项目结构和 GPL-3.0 许可证，重点补充新版 Emby Server 兼容性与 STRM 媒体信息补漏能力。原项目的完整功能介绍、使用文档和历史更新请以上游仓库为准。

## 本构建的主要改动

- 适配 Emby 4.9 的 STRM 媒体挂载与路径解析，修复新版 Emby 中媒体信息提取失败的问题。
- 适配新版 Emby 的媒体源读取和外挂字幕扫描能力。
- 修复媒体信息 JSON 写入或删除时触发重复媒体库监听的问题。
- 新增 STRM 媒体信息 JSON 缺失检查与补漏任务，可为已入库但缺少持久化媒体信息的 STRM 逐项补齐。
- 保留追更模式，可在新增 STRM 入库后自动进入媒体信息提取队列。
- 补充 Linux/macOS 环境下的可复现构建流程和兼容性自动测试。

## 验证环境

当前兼容构建已在隔离的 Emby Server `4.9.3.0` 容器中验证。

已验证内容包括：

- 插件加载和配置页显示
- Extract MediaInfo
- Persist MediaInfo
- STRM 媒体信息挂载与探测
- 媒体信息 JSON 缺失检查与补漏
- 外挂字幕扫描兼容
- 新增 STRM 的媒体信息自动追更

> 其他 Emby 版本尚未进行同等强度的完整验证，请在升级或替换插件前备份现有插件和配置。

详细构建与验证记录见 [Emby 4.9 兼容性验证](docs/emby-4.9-compatibility-validation.md)。

## 安装

1. 从 Releases 下载 `StrmAssistantLite.dll`。
2. 将文件放入 Emby Server 的 `plugins` 目录。
3. 重启 Emby Server。
4. 在 Emby 插件页确认 `Strm Assistant` 已加载，再按需配置功能。

## 构建

仓库提供可重复执行的构建脚本，需要 .NET SDK 8：

```bash
./scripts/build-plugin.sh
```

如果 `dotnet` 不在 `PATH` 中，可显式指定：

```bash
DOTNET_CMD=/path/to/dotnet ./scripts/build-plugin.sh
```

构建会同时运行兼容性测试，产物位于 `artifacts/StrmAssistantLite.dll`。

## 原创与授权声明

Strm Assistant 的原创工作归上游作者及项目贡献者所有。本仓库是针对个人使用场景的二次开发和兼容构建，不声称拥有原项目的原创成果，也不代表或替代上游官方发布。

本项目依照上游采用 [GNU General Public License v3.0](LICENSE) 发布。如需了解原项目的完整功能、授权、使用说明或后续支持，请访问：

[https://github.com/sjtuross/StrmAssistant](https://github.com/sjtuross/StrmAssistant)

## 免责声明

本项目与 Emby LLC 没有任何关联，也未获得 Emby LLC 的授权或认可。本项目不包含 Emby 专有组件，不用于绕过 Emby 授权、DRM 或解锁付费功能。使用者需自行确保对 Emby Server 的安装和使用符合许可协议及所在地法律法规。
