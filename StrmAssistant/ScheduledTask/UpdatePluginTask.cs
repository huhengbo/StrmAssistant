using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using StrmAssistant.Common;
using StrmAssistant.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StrmAssistant.ScheduledTask
{
    public class UpdatePluginTask : IScheduledTask
    {
        private readonly ILogger _logger;
        private readonly IApplicationHost _applicationHost;
        private readonly IApplicationPaths _applicationPaths;
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IActivityManager _activityManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IServerApplicationHost _serverApplicationHost;

        public UpdatePluginTask(IApplicationHost applicationHost, IApplicationPaths applicationPaths,
            IHttpClient httpClient, IJsonSerializer jsonSerializer, IActivityManager activityManager,
            ILocalizationManager localizationManager, IServerApplicationHost serverApplicationHost)
        {
            _logger = Plugin.Instance.Logger;
            _applicationHost = applicationHost;
            _applicationPaths = applicationPaths;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _activityManager = activityManager;
            _localizationManager = localizationManager;
            _serverApplicationHost = serverApplicationHost;
        }

        private static string PluginAssemblyFilename => Assembly.GetExecutingAssembly().GetName().Name + ".dll";
        private static string RepoReleaseUrl => "https://api.github.com/repos/huhengbo/StrmAssistant/releases/latest";

        public string Key => "UpdatePluginTask";

        public string Name => "Update Plugin";

        public string Description => Resources.ResourceManager.GetString(
            "UpdatePluginTask_Description_Updates_plugin_to_the_latest_version", Plugin.Instance.DefaultUICulture);

        public string Category => Resources.ResourceManager.GetString("PluginOptions_EditorTitle_Strm_Assistant",
            Plugin.Instance.DefaultUICulture);

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerWeekly,
                DayOfWeek = (DayOfWeek)new Random().Next(7),
                TimeOfDayTicks = TimeSpan.FromMinutes(new Random().Next(24 * 4) * 15).Ticks
            };
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            await Task.Yield();
            progress.Report(0);

            try
            {
                var githubToken = Plugin.Instance.GetPluginOptions().AboutOptions.GitHubToken;
                var releaseRequest = new HttpRequestOptions
                {
                    Url = RepoReleaseUrl,
                    CancellationToken = cancellationToken,
                    AcceptHeader = "application/json",
                    UserAgent = Plugin.Instance.UserAgent,
                    EnableDefaultUserAgent = false
                };

                if (!string.IsNullOrWhiteSpace(githubToken) &&
                    PluginUpdateSecurity.ShouldAttachGitHubToken(RepoReleaseUrl))
                {
                    releaseRequest.RequestHeaders["Authorization"] = $"token {githubToken}";
                }

                using var response = await _httpClient.SendAsync(releaseRequest, "GET").ConfigureAwait(false);
                await using var contentStream = response.Content;
                var apiResult = _jsonSerializer.DeserializeFromStream<ApiResponseInfo>(contentStream);

                var currentVersion = ParseVersion(Plugin.Instance.CurrentVersion);
                var remoteVersion = ParseVersion(apiResult?.tag_name);

                if (currentVersion.CompareTo(remoteVersion) < 0)
                {
                    _logger.Info("Found new plugin version: {0}", remoteVersion);

                    var asset = (apiResult?.assets ?? new List<ApiAssetInfo>())
                        .FirstOrDefault(item => item.name == PluginAssemblyFilename);
                    var url = asset?.browser_download_url;
                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                        throw new InvalidOperationException("Invalid plugin download url");

                    var githubProxy = Plugin.Instance.GetPluginOptions().AboutOptions.GitHubProxy;
                    var downloadUrl = PluginUpdateSecurity.BuildDownloadUrl(url, githubProxy);

                    // Public release assets do not require authentication. In particular, never
                    // attach the GitHub token when a third-party proxy is configured.
                    await using var responseStream = await _httpClient.Get(new HttpRequestOptions
                    {
                        Url = downloadUrl,
                        CancellationToken = cancellationToken,
                        UserAgent = Plugin.Instance.UserAgent,
                        EnableDefaultUserAgent = false,
                        Progress = progress
                    }).ConfigureAwait(false);

                    using var memoryStream = new MemoryStream();
                    await responseStream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);
                    var actualDigest = PluginUpdateSecurity.ValidatePluginPayload(memoryStream, asset?.digest);
                    _logger.Info("Downloaded plugin SHA-256: {0}", actualDigest);

                    var dllFilePath = Path.Combine(_applicationPaths.PluginsPath, PluginAssemblyFilename);
                    InstallPlugin(memoryStream, dllFilePath, cancellationToken);

                    _logger.Info("Plugin update complete. Backup: {0}.bak", dllFilePath);

                    _activityManager.Create(new ActivityLogEntry
                    {
                        Name = string.Format(_localizationManager.GetLocalizedString("XUpdatedOnTo"), Category,
                            remoteVersion, _serverApplicationHost.FriendlyName),
                        Type = "PluginUpdateInstalled",
                        Severity = LogSeverity.Info
                    });

                    _applicationHost.NotifyPendingRestart();
                }
                else
                {
                    _ = Plugin.NotificationApi.SendMessageToAdmins(
                        $"[{Resources.PluginOptions_EditorTitle_Strm_Assistant}] {Resources.No_Update_Message}", 1000);
                    _logger.Info("No need to update");
                }
            }
            catch (Exception e)
            {
                _activityManager.Create(new ActivityLogEntry
                {
                    Name = string.Format(_localizationManager.GetLocalizedString("NameInstallFailedOn"), Category,
                        _serverApplicationHost.FriendlyName),
                    Type = "PluginUpdateFailed",
                    Overview = e.Message,
                    Severity = LogSeverity.Error
                });

                _ = Plugin.NotificationApi.SendMessageToAdmins(
                    $"[{Resources.PluginOptions_EditorTitle_Strm_Assistant}] {Resources.Update_Failed_Message}", 1000);
                _logger.Error("Update failed: {0}", e.Message);
                _logger.Debug(e.StackTrace);
            }

            progress.Report(100);
        }

        private static void InstallPlugin(MemoryStream stream, string dllFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(dllFilePath))
                throw new FileNotFoundException("Current plugin DLL was not found", dllFilePath);

            var tempFilePath = dllFilePath + ".download";
            var backupFilePath = dllFilePath + ".bak";

            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);

            try
            {
                stream.Position = 0;
                using (var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                }

                cancellationToken.ThrowIfCancellationRequested();
                File.Copy(dllFilePath, backupFilePath, true);

                try
                {
                    File.Copy(tempFilePath, dllFilePath, true);
                }
                catch
                {
                    if (File.Exists(backupFilePath)) File.Copy(backupFilePath, dllFilePath, true);
                    throw;
                }
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        private static Version ParseVersion(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) throw new InvalidOperationException("Plugin version is missing");
            return new Version(v.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? v.Substring(1) : v);
        }

        internal class ApiResponseInfo
        {
            public string tag_name { get; set; }

            public List<ApiAssetInfo> assets { get; set; }
        }

        internal class ApiAssetInfo
        {
            public string name { get; set; }

            public string browser_download_url { get; set; }

            public string digest { get; set; }
        }
    }
}
