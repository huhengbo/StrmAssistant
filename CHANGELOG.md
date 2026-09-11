# 更新日志

这里记录 `Strm Assistant Enhanced` 各正式版本的用户可见变更。

## 发布说明模板

后续发布新版本时，复制下面的结构并将标题改为实际版本号，例如 `## 2026.9.12.0`。GitHub Release 会自动截取对应版本章节作为中文发布说明。

### ✨ 主要更新

- **功能名称**
  - 简要说明用户能获得什么。

### 🐛 问题修复

- 修复具体问题。

### ⚙️ 优化调整

- 优化性能、构建、日志或兼容性。

### 🔒 安全与稳定性

- 有安全相关改动时填写；没有可删除本章节。

### 🌐 多语言

- 有本地化改动时填写；没有可删除本章节。

### 📦 升级说明

- 说明是否可以直接覆盖升级、是否需要重启、是否涉及配置迁移。

### ✅ 兼容性

- 说明重点兼容和已验证的 Emby Server 版本。

### 🔐 文件校验

- Release 同时提供 `StrmAssistantLite.dll.sha256`，用于校验下载文件完整性。

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0

## 版本规则

`Strm Assistant Enhanced` 使用 `YYYY.M.D.REVISION` 格式的日历版本号。

- `YYYY.M.D`：发布日期。
- `REVISION`：同一天的第几次正式发布，从 `0` 开始递增。
- Git Tag 使用 `v` 前缀，例如 `v2026.9.12.0`。
- 版本号保持与插件内部 `System.Version` 比较逻辑兼容。

## 2026.9.12.0

> 本版本是稳定性与项目链接清理修正版，不新增复杂功能。

### 🐛 问题修复

- 移除插件 About 页面中的 `Upstream / Credits` 旧项目入口，项目、文档与更新入口统一指向当前仓库。
- 修复运行中调整并发数时 `SemaphoreSlim` 被替换导致的竞态问题。
- 修复批量删除版本时清理错误内部元数据目录的问题。
- 修复前端请求失败仍显示“成功”的提示问题。
- 通知标题和管理员弹窗统一使用 `Strm Assistant Enhanced` 品牌名称。

### 📦 升级说明

已有用户可以直接覆盖升级 `StrmAssistantLite.dll`，现有插件 GUID 与配置保持不变。替换 DLL 后重启 Emby Server 即可。

### ✅ 兼容性

当前重点维护 Emby Server 4.9.x。

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0

## 2026.9.11.0

> `Strm Assistant Enhanced` 独立维护后的首个正式版本。

### ✨ 主要更新

- **完成二开品牌收口**
  - 插件显示名称统一为 `Strm Assistant Enhanced`。
  - 更换独立 Logo。
  - 项目、文档、免责声明及更新地址统一切换到当前维护仓库。
  - 项目发布继续遵循 GPL-3.0。

- **增强 Emby 4.9 兼容性**
  - 优化 STRM 媒体挂载与路径解析。
  - 适配新版媒体源读取接口。
  - 改进外挂字幕扫描兼容性。
  - Emby 私有 API 反射调用改为按完整参数类型与返回类型精确匹配，降低版本升级后的误调用风险。

- **新增 STRM MediaInfo 缺失补漏任务**
  - 自动检查已有 STRM 项目是否缺失 MediaInfo JSON。
  - 支持自动提取并补齐缺失信息。
  - 大媒体库改为分页扫描，每批 200 条。
  - 优化任务日志和取消响应。

### 🐛 问题修复

- 修复新版 Emby 中部分 STRM 媒体信息提取失败的问题。
- 修复媒体信息 JSON 写入或删除时可能触发重复媒体库监听的问题。
- 修复新版 Emby 媒体源与外挂字幕相关 API 变化带来的兼容问题。
- 修复旧 Logo、仓库链接、Wiki、免责声明等品牌残留。

### ⚙️ 优化调整

- Windows、macOS、Linux 统一使用：

  `python scripts/build_plugin.py`

- GitHub Actions 增加 Windows / macOS / Linux 三平台自动构建与测试。
- 建立独立版本体系 `YYYY.M.D.REVISION`。
- 建立自动 Release 发布流程。
- Release 自动生成：
  - `StrmAssistantLite.dll`
  - `StrmAssistantLite.dll.sha256`

### 🔒 安全与稳定性

- GitHub Token 不再发送给第三方 GitHub Proxy。
- 插件下载后会检查 PE/DLL 基本格式。
- 支持校验 GitHub Release 提供的 SHA-256 digest。
- 自动更新前备份现有 DLL。
- 更新失败时恢复原插件文件，降低自动更新导致插件损坏的风险。

### 🌐 多语言

- 更新任务名称和描述改为资源化管理。
- STRM 媒体信息补漏任务支持：
  - English
  - 简体中文
  - 繁體中文
- 插件品牌名称统一使用 `Strm Assistant Enhanced`。

### 📦 升级说明

已有 StrmAssistant 用户可以直接覆盖升级 `StrmAssistantLite.dll`。

插件 GUID 保持不变：

`63c322b7-a371-41a3-b11f-04f8418b37d8`

因此原有插件配置可以继续保留。替换 DLL 后重启 Emby Server 即可。

### ✅ 兼容性

当前重点维护 Emby Server 4.9.x。

已重点验证：

- 插件加载与配置页。
- MediaInfo 提取。
- MediaInfo JSON 持久化。
- STRM 媒体路径与挂载。
- MediaInfo 缺失补漏。
- 外挂字幕扫描。
- 新增 STRM 自动追更。

其他 Emby 版本建议升级前备份原插件与配置。

### 🔐 文件校验

本版本同时提供：

- `StrmAssistantLite.dll`
- `StrmAssistantLite.dll.sha256`

DLL SHA-256：

`29574e6f07074811e392391a16818f7917d4a1426a49a4cd49788a7da2221a68`

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0
