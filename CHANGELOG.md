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

- 说明从哪些版本可以直接升级。
- 说明是否需要迁移配置、替换文件、重启服务或刷新客户端缓存。
- 说明新功能是否会改变现有默认行为，以及是否存在需要用户主动开启的选项。

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

## 2026.9.13.0

> 本版本收口 Emby Web 外部播放体验：界面更贴近 Emby 原生风格，并将外部播放与 STRM Direct 纳入插件统一配置，同时补全 README 与发布说明。

### ✨ 主要更新

- **外部播放加入插件设置**
  - “体验增强 / Experience Enhance”新增 `Web 外部播放 / External Player` 总开关。
  - 总开关默认开启，保持升级后现有外部播放能力可用。
  - 关闭后详情页按钮和更多菜单中的“外部播放”入口都不会显示。
  - `STRM 直通 / STRM Direct` 从浏览器 `localStorage` 调整为插件服务端配置，默认关闭。

- **外部播放 UI 更贴近 Emby**
  - 详情页入口改用 Emby 原生详情按钮样式。
  - 播放器选择优先使用 Emby 原生 Action Sheet，并保留标准 Dialog 回退。
  - 播放器操作与“复制播放链接”继续按当前操作系统过滤和展示。

### ⚙️ 优化调整

- Web 客户端启动时读取插件当前外部播放配置，设置修改后刷新 Emby Web 页面即可生效。
- 外部播放菜单和详情页入口使用同一总开关，避免显示状态不一致。
- STRM Direct 不再在播放器选择弹窗中临时切换，配置语义统一到插件设置。
- README / README.en 新增完整的 Emby Web 外部播放功能与使用说明。

### 📦 升级说明

- **可直接升级**：`v2026.9.12.2` 及更早版本均可直接覆盖升级。
- **外部播放总开关默认开启**：升级后仍会显示原有外部播放入口；如不需要，可在插件“体验增强”中关闭。
- **STRM Direct 默认关闭**：该设置已从浏览器 `localStorage` 迁移到插件配置。旧浏览器中曾经开启的本地 STRM Direct 状态不会自动迁移，需要在插件设置中重新开启。
- **生效方式**：替换 DLL 并重启 Emby Server 后，刷新 Emby Web 页面以重新加载外部播放配置和前端脚本。
- **播放器依赖不变**：PotPlayer / VLC / MPV / IINA / Infuse 仍需在客户端安装并注册 URL Scheme / Protocol Handler。
- **进度行为不变**：只单向传递 Emby 当前续播位置，不回写外部播放器进度。

### ✅ 兼容性

- 当前重点维护 Emby Server 4.9.x。
- 外部播放继续支持 Movie / Episode / Series / Season、多版本 MediaSource、外挂字幕、续播位置和复制播放链接。
- Windows / macOS / Linux 继续由 GitHub Actions 执行构建测试，并检查嵌入式 Web JavaScript 语法。

### 🔐 文件校验

Release 将继续提供：

- `StrmAssistantLite.dll`
- `StrmAssistantLite.dll.sha256`

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0

## 2026.9.12.2

> 本版本更新 Strm Assistant Enhanced 的项目与插件 Logo，不改变任何现有功能行为。

### ✨ 主要更新

- **启用新的卡通水獭 Logo**
  - 使用新的水獭形象作为项目与插件视觉标识。
  - Logo 保留播放、流媒体与服务端元素，继续对应 STRM / Emby 增强定位。
  - 插件内嵌 `thumb.png` 已替换，README 使用同一图标资源。

### ⚙️ 优化调整

- 插件图标按 Emby 插件展示场景处理为轻量方形 PNG，避免直接嵌入大尺寸原图增加 DLL 体积。
- 本版本不新增功能、不修改配置结构、不改变外部播放器或 STRM 处理逻辑。

### 📦 升级说明

- **可直接升级**：`v2026.9.12.1` 及更早的 Strm Assistant Enhanced 版本均可直接覆盖升级。
- **无需配置迁移**：插件 GUID 保持不变，现有插件配置与浏览器端外部播放器设置会继续保留。
- **升级步骤**：使用本版本 `StrmAssistantLite.dll` 覆盖原 DLL，然后重启 Emby Server。
- **Logo 缓存**：如果 Emby 插件页面仍显示旧 Logo，可刷新页面；浏览器缓存较强时可强制刷新或清理 Emby Web 缓存后重新打开插件页面。
- **默认行为不变**：本版本只更新视觉资源，不改变 MediaInfo、STRM、片头片尾、外部播放器和自动更新等现有功能行为。

### ✅ 兼容性

- 当前重点维护 Emby Server 4.9.x。
- 本版本仅更新内嵌图片资源，功能兼容范围与 `v2026.9.12.1` 一致。

### 🔐 文件校验

Release 同时提供：

- `StrmAssistantLite.dll`
- `StrmAssistantLite.dll.sha256`

请以 Release 页面公布的 SHA-256 为准。

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0

## 2026.9.12.1

> 本版本新增 Emby Web 外部播放器快捷入口，保持现有播放与插件配置默认行为不变。

### ✨ 主要更新

- **新增 Emby Web 外部播放**
  - 详情页增加“外部播放”快捷入口。
  - 右键 / `...` 菜单同步增加“外部播放”。
  - 第一阶段支持 PotPlayer、VLC、MPV、IINA、Infuse 和复制播放链接。
  - 根据 Windows、macOS、Linux、Android、iOS 自动过滤明显不可用的播放器。

- **保留当前播放上下文**
  - 多版本媒体使用当前选中的 `MediaSource`，不会固定播放第一个版本。
  - 优先传递当前选中的外挂字幕；未选择时优先默认外挂字幕，其次中文字幕。
  - 将 Emby 当前续播位置传给支持 seek/position 的外部播放器。
  - Series 默认使用 Next Up；Season 使用首个可播放项目。

- **STRM Direct 高级选项**
  - 可选择将 HTTP/HTTPS 类型 STRM 原始地址直接交给外部播放器。
  - 默认关闭，默认仍通过 Emby 串流地址播放，因此升级后不会改变现有播放链路。
  - 该选项仅保存在当前浏览器 `localStorage`，不会写入服务端插件配置。

### ⚙️ 优化调整

- 外部播放器模块复用现有 StrmAssistant Web 注入机制，不修改 Emby `index.html`。
- 不加载第三方远程 JavaScript 或远程播放器图标。
- CI 增加嵌入式 Web JavaScript 语法检查，并继续执行 Windows / macOS / Ubuntu 三平台构建与测试。

### 🔒 安全与稳定性

- 带 Emby access token 的串流地址只在当前浏览器播放操作中生成，不写入日志、不持久化到插件配置。
- 外部播放不引入 nginx、AList、302 路由、路径映射或本地 companion service。

### 📦 升级说明

- **可直接升级**：`v2026.9.12.0` 及更早的 Strm Assistant Enhanced 版本均可直接覆盖升级。
- **无需配置迁移**：插件 GUID 保持不变，现有插件设置会继续保留。
- **升级步骤**：使用本版本 `StrmAssistantLite.dll` 覆盖原 DLL，然后重启 Emby Server。
- **Web 缓存**：重启后如果详情页仍未出现“外部播放”，请先强制刷新浏览器页面；仍显示旧页面时再清理 Emby Web 浏览器缓存后重新登录。
- **默认行为不变**：`STRM Direct` 默认关闭，升级不会自动把现有 STRM 改为原始 URL 直通。
- **外部播放器依赖**：PotPlayer / VLC / MPV / IINA / Infuse 需要客户端已安装，并且对应 URL Scheme / Protocol Handler 已正确注册；插件只负责生成并调用播放器链接。
- **进度说明**：本版本仅将 Emby 当前续播位置单向传给支持的播放器，不会将外部播放器的播放进度回写 Emby。

### ✅ 兼容性

- 当前重点维护 Emby Server 4.9.x。
- Windows / macOS / Ubuntu 构建、测试及嵌入式 JavaScript 语法检查均已通过。
- 外部播放器最终拉起行为仍取决于浏览器对自定义协议的支持，以及本机播放器协议注册状态。

### 🔐 文件校验

Release 同时提供：

- `StrmAssistantLite.dll`
- `StrmAssistantLite.dll.sha256`

请以 Release 页面公布的 SHA-256 为准。

### 📌 项目说明

- 当前维护：`huhengbo/StrmAssistant`
- License：GPL-3.0

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
