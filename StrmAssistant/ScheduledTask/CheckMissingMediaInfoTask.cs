using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;
using StrmAssistant.Common;
using StrmAssistant.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StrmAssistant.ScheduledTask
{
    public class CheckMissingMediaInfoTask : IScheduledTask
    {
        private const int PageSize = 200;

        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ILibraryManager _libraryManager;

        public CheckMissingMediaInfoTask(IFileSystem fileSystem, ILibraryManager libraryManager)
        {
            _logger = Plugin.Instance.Logger;
            _fileSystem = fileSystem;
            _libraryManager = libraryManager;
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("MediaInfoJsonGapCheck - Scheduled Task Execute");

            await Task.Yield();
            progress.Report(0);
            cancellationToken.ThrowIfCancellationRequested();

            var directoryService = new DirectoryService(_logger, _fileSystem);
            var options = Plugin.Instance.GetPluginOptions().MediaInfoExtractOptions;
            var libraryScope = options.LibraryScope ?? string.Empty;
            var allLibraries = string.IsNullOrWhiteSpace(libraryScope);
            var libraryIds = libraryScope.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var libraries = _libraryManager.GetVirtualFolders()
                .Where(folder => allLibraries || libraryIds.Contains(folder.Id))
                .ToList();
            var includeFavorites = libraryIds.Contains("-1");
            var includeExtra = options.IncludeExtra;

            _logger.Info("MediaInfoJsonGapCheck - Scope: {0}; IncludeExtra: {1}; PageSize: {2}",
                allLibraries ? "ALL" : string.Join(",", libraryIds), includeExtra, PageSize);

            var seenIds = new HashSet<long>();
            var scanned = 0;
            var strmCandidates = 0;
            var missing = 0;
            var success = 0;
            var skip = 0;
            var failures = new List<Exception>();
            var pageCount = 0;

            async Task ProcessCandidate(BaseItem item)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item == null || !seenIds.Add(item.InternalId)) return;

                scanned++;
                if (!IsStrmCandidate(item)) return;
                strmCandidates++;

                if (Plugin.MediaInfoApi.MediaInfoJsonExists(item, directoryService))
                {
                    skip++;
                    return;
                }

                missing++;
                try
                {
                    var result = await Plugin.LibraryApi
                        .EnsureMediaInfoJsonAsync(item, directoryService, Name, cancellationToken)
                        .ConfigureAwait(false);

                    if (result)
                    {
                        success++;
                        _logger.Debug("MediaInfoJsonGapCheck - Item processed: " + item.Name + " - " + item.Path);
                    }
                    else
                    {
                        skip++;
                        _logger.Debug("MediaInfoJsonGapCheck - Item skipped: " + item.Name + " - " + item.Path);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("MediaInfoJsonGapCheck - Cancelled after scanning {0} unique items", scanned);
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
            }

            async Task ScanPagedQuery(InternalItemsQuery query, string scopeName)
            {
                var startIndex = 0;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    query.StartIndex = startIndex;
                    query.Limit = PageSize;

                    var page = _libraryManager.GetItemList(query);
                    if (page == null || page.Length == 0) break;

                    pageCount++;
                    foreach (var item in page)
                    {
                        await ProcessCandidate(item).ConfigureAwait(false);
                    }

                    startIndex += page.Length;
                    _logger.Info(
                        "MediaInfoJsonGapCheck - Batch {0} ({1}) complete: rows={2}, scanned={3}, strm={4}, missing={5}, success={6}, failed={7}",
                        pageCount, scopeName, page.Length, scanned, strmCandidates, missing, success, failures.Count);

                    if (page.Length < PageSize) break;
                }
            }

            if (includeFavorites)
            {
                foreach (var user in LibraryApi.AllUsers.Keys)
                {
                    var startIndex = 0;
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var favoritesPage = _libraryManager.GetItemList(new InternalItemsQuery
                        {
                            User = user,
                            IsFavorite = true,
                            StartIndex = startIndex,
                            Limit = PageSize
                        });
                        if (favoritesPage == null || favoritesPage.Length == 0) break;

                        var expanded = Plugin.LibraryApi.ExpandFavorites(favoritesPage.ToList(), false, false, false);
                        if (includeExtra)
                        {
                            expanded = expanded.Concat(expanded.SelectMany(item =>
                                    item.GetExtras(LibraryApi.IncludeExtraTypes)))
                                .GroupBy(item => item.InternalId)
                                .Select(group => group.First())
                                .ToList();
                        }

                        foreach (var item in expanded)
                        {
                            await ProcessCandidate(item).ConfigureAwait(false);
                        }

                        startIndex += favoritesPage.Length;
                        pageCount++;
                        _logger.Info(
                            "MediaInfoJsonGapCheck - Batch {0} (favorites:{1}) complete: rows={2}, scanned={3}, strm={4}, missing={5}, success={6}, failed={7}",
                            pageCount, user.Name, favoritesPage.Length, scanned, strmCandidates, missing, success,
                            failures.Count);

                        if (favoritesPage.Length < PageSize) break;
                    }
                }
            }

            if (allLibraries || libraries.Any())
            {
                var baseQuery = new InternalItemsQuery
                {
                    HasPath = true,
                    MediaTypes = new[] { MediaType.Video }
                };

                if (!allLibraries && libraries.Any())
                {
                    baseQuery.PathStartsWithAny = libraries.SelectMany(folder => folder.Locations)
                        .Select(path => path.EndsWith(Path.DirectorySeparatorChar.ToString())
                            ? path
                            : path + Path.DirectorySeparatorChar)
                        .ToArray();
                }

                await ScanPagedQuery(baseQuery, "media").ConfigureAwait(false);

                if (includeExtra)
                {
                    var extraQuery = new InternalItemsQuery
                    {
                        HasPath = true,
                        MediaTypes = new[] { MediaType.Video },
                        ExtraTypes = LibraryApi.IncludeExtraTypes,
                        PathStartsWithAny = baseQuery.PathStartsWithAny
                    };
                    await ScanPagedQuery(extraQuery, "extras").ConfigureAwait(false);
                }
            }

            progress.Report(100.0);
            _logger.Info(
                "MediaInfoJsonGapCheck - Complete: pages={0}, uniqueScanned={1}, strmCandidates={2}, missing={3}, processed={4}, skipped={5}, failed={6}",
                pageCount, scanned, strmCandidates, missing, success, skip, failures.Count);

            if (failures.Count > 0)
            {
                throw new AggregateException("MediaInfo JSON gap check failed for " + failures.Count + " item(s).",
                    failures);
            }
        }

        private static bool IsStrmCandidate(BaseItem item)
        {
            return item.IsShortcut ||
                   string.Equals(Path.GetExtension(item.Path), ".strm", StringComparison.OrdinalIgnoreCase);
        }

        public string Category => Plugin.DisplayName;

        public string Key => "CheckMissingMediaInfoTask";

        public string Description => "检查已有 STRM 文件是否缺少 MediaInfo JSON，并逐一补齐。";

        public string Name => "检查补漏缺失媒体信息";

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }
    }
}
