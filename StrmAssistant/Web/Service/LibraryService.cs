using MediaBrowser.Controller.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using StrmAssistant.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static StrmAssistant.Common.CommonUtility;
using static StrmAssistant.Common.LanguageUtility;

namespace StrmAssistant.Web.Service
{
    public class LibraryService : BaseApiService
    {
        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IFileSystem _fileSystem;
        private readonly IItemRepository _itemRepository;

        public LibraryService(ILibraryManager libraryManager, IItemRepository itemRepository, IFileSystem fileSystem)
        {
            _logger = Plugin.Instance.Logger;
            _libraryManager = libraryManager;
            _itemRepository = itemRepository;
            _fileSystem = fileSystem;
        }

        public void Any(DeleteVersion request)
        {
            var item = _libraryManager.GetItemById(request.Id);

            if (!(item is Video video) || !(video is Movie || video is Episode) || !video.IsFileProtocol ||
                video.GetAlternateVersionIds().Count == 0)
            {
                return;
            }
            
            var user = GetUserForRequest(null);
            var collectionFolders = _libraryManager.GetCollectionFolders(item);

            if (user is null)
            {
                if (!item.CanDelete())
                {
                    return;
                }
            }
            else if (!item.CanDelete(user, collectionFolders))
            {
                return;
            }

            var deleteItems = item is Episode episode && request.DeleteParent
                ? GetSeasonEpisodesSameVersion(episode)
                : new List<BaseItem> { item };

            foreach (var deleteItem in deleteItems)
            {
                var proceedToDelete = true;
                var deletePaths = Plugin.LibraryApi.GetDeletePaths(deleteItem);

                foreach (var path in deletePaths)
                {
                    try
                    {
                        if (!path.IsDirectory)
                        {
                            _logger.Info("DeleteVersion - Attempting to delete file: " + path.FullName);
                            _fileSystem.DeleteFile(path.FullName, true);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is IOException || e is UnauthorizedAccessException)
                        {
                            proceedToDelete = false;
                            _logger.Error("DeleteVersion - Failed to delete file: " + path.FullName);
                            _logger.Error(e.Message);
                            _logger.Debug(e.StackTrace);
                        }
                    }
                }

                if (proceedToDelete)
                {
                    _itemRepository.DeleteItems(new[] { deleteItem });

                    try
                    {
                        _fileSystem.DeleteDirectory(deleteItem.GetInternalMetadataPath(), true, true);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private List<BaseItem> GetSeasonEpisodesSameVersion(Episode episode)
        {
            var seasonFolderChildren = episode.Parent.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Episode) },
                    Recursive = false,
                    GroupByPresentationUniqueKey = false,
                })
                .ToList();

            var seasonEpisodesCount = episode.Season.GetEpisodeIds(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Episode) }, GroupByPresentationUniqueKey = true
                })
                .Length;

            if (seasonFolderChildren.Count == seasonEpisodesCount)
            {
                return seasonFolderChildren;
            }

            var targetCleaned = CleanEpisodeName(episode.FileNameWithoutExtension);

            var allEpisodes = episode.Season
                .GetEpisodes(new InternalItemsQuery
                {
                    GroupByPresentationUniqueKey = false, EnableTotalRecordCount = false
                })
                .Items.ToList();

            var similarEpisodes = new List<BaseItem>();

            foreach (var ep in allEpisodes)
            {
                var cleanedName = CleanEpisodeName(ep.FileNameWithoutExtension);
                var similarity = LevenshteinDistance(targetCleaned, cleanedName);

                if (similarity > 0.92)
                {
                    similarEpisodes.Add(ep);
                }
            }

            return similarEpisodes;
        }
    }
}
