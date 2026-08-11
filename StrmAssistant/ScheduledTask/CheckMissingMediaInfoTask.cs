using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;
using StrmAssistant.Common;
using StrmAssistant.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StrmAssistant.ScheduledTask
{
    public class CheckMissingMediaInfoTask : IScheduledTask
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;

        public CheckMissingMediaInfoTask(IFileSystem fileSystem)
        {
            _logger = Plugin.Instance.Logger;
            _fileSystem = fileSystem;
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("MediaInfoJsonGapCheck - Scheduled Task Execute");

            await Task.Yield();
            progress.Report(0);
            cancellationToken.ThrowIfCancellationRequested();

            var directoryService = new DirectoryService(_logger, _fileSystem);
            var items = Plugin.LibraryApi.FetchMissingStrmMediaInfoJsonItems(directoryService);
            cancellationToken.ThrowIfCancellationRequested();

            double total = items.Count;
            var current = 0;
            var success = 0;
            var skip = 0;
            var failures = new List<Exception>();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var result = await Plugin.LibraryApi
                        .EnsureMediaInfoJsonAsync(item, directoryService, Name, cancellationToken)
                        .ConfigureAwait(false);

                    if (result)
                    {
                        success++;
                        _logger.Info("MediaInfoJsonGapCheck - Item processed: " + item.Name + " - " + item.Path);
                    }
                    else
                    {
                        skip++;
                        _logger.Info("MediaInfoJsonGapCheck - Item skipped: " + item.Name + " - " + item.Path);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("MediaInfoJsonGapCheck - Item cancelled: " + item.Name + " - " + item.Path);
                    throw;
                }
                catch (Exception e)
                {
                    failures.Add(new InvalidOperationException(
                        "Failed to create MediaInfo JSON for item " + item.InternalId + ": " + item.Path, e));
                    _logger.Error("MediaInfoJsonGapCheck - Item failed: " + item.Name + " - " + item.Path);
                    _logger.Error(e.Message);
                    _logger.Debug(e.StackTrace);
                }
                finally
                {
                    current++;
                    progress.Report(total > 0 ? current / total * 100 : 100);
                    _logger.Info("MediaInfoJsonGapCheck - Progress " + current + "/" + total + ": " + item.Path);
                }
            }

            progress.Report(100.0);
            _logger.Info("MediaInfoJsonGapCheck - Number of items processed: " + success);
            _logger.Info("MediaInfoJsonGapCheck - Number of items skipped: " + skip);
            _logger.Info("MediaInfoJsonGapCheck - Number of items failed: " + failures.Count);

            if (failures.Count > 0)
            {
                throw new AggregateException("MediaInfo JSON gap check failed for " + failures.Count + " item(s).",
                    failures);
            }

            _logger.Info("MediaInfoJsonGapCheck - Scheduled Task Complete");
        }

        public string Category => Resources.ResourceManager.GetString("PluginOptions_EditorTitle_Strm_Assistant",
            Plugin.Instance.DefaultUICulture);

        public string Key => "CheckMissingMediaInfoTask";

        public string Description => "检查已有 STRM 文件是否缺少 MediaInfo JSON，并逐一补齐。";

        public string Name => "检查补漏缺失媒体信息";

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }
    }
}
