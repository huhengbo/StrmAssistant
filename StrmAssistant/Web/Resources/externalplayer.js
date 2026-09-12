define(['connectionManager', 'globalize', 'loading', 'toast', 'dialog'], function (connectionManager, globalize, loading, toast, dialog) {
    'use strict';

    const detailButtonId = 'strmAssistantExternalPlayerButton';
    const supportedItemTypes = ['Movie', 'Episode', 'Series', 'Season'];
    let initialized = false;
    let config = { enabled: false, strmDirect: false };

    function localeText(en, zhCn, zhHant) {
        const locale = globalize.getCurrentLocale().toLowerCase();
        if (locale === 'zh-cn' || locale === 'zh-sg') return zhCn;
        if (locale === 'zh-hk' || locale === 'zh-tw' || locale === 'zh-mo') return zhHant;
        return en;
    }

    function getLabels() {
        return {
            externalPlay: localeText('External Player', '外部播放', '外部播放'),
            copy: localeText('Copy Stream URL', '复制播放链接', '複製播放連結'),
            copySuccess: localeText('Stream URL copied', '播放链接已复制', '播放連結已複製'),
            noMedia: localeText('No playable media source found', '未找到可播放媒体源', '未找到可播放媒體來源'),
            failed: localeText('Unable to open external player', '无法打开外部播放器', '無法開啟外部播放器'),
            cancel: globalize.translate('Cancel') || 'Cancel'
        };
    }

    const OS = {
        isAndroid: () => /Android/i.test(navigator.userAgent),
        isIOS: () => /iPad|iPhone|iPod/i.test(navigator.userAgent),
        isMacOS: () => /Macintosh|MacIntel/i.test(navigator.userAgent),
        isWindows: () => /Windows/i.test(navigator.userAgent),
        isLinux: () => /Linux/i.test(navigator.userAgent) && !/Android/i.test(navigator.userAgent)
    };

    function normalizeConfig(data) {
        const enabled = data && (data.Enabled !== undefined ? data.Enabled : data.enabled);
        const strmDirect = data && (data.StrmDirect !== undefined ? data.StrmDirect : data.strmDirect);
        return {
            enabled: enabled === true,
            strmDirect: strmDirect === true
        };
    }

    async function loadConfig() {
        try {
            const apiClient = connectionManager.currentApiClient();
            const response = await apiClient.ajax({
                type: 'GET',
                url: apiClient.getUrl('StrmAssistant/ExternalPlayer/Config')
            });
            const data = typeof response === 'string' ? JSON.parse(response) : response;
            config = normalizeConfig(data);
        } catch (_) {
            config = { enabled: false, strmDirect: false };
        }
        return config;
    }

    function getStrmDirect() {
        return config.strmDirect === true;
    }

    function getCurrentMediaSourceId() {
        const select = document.querySelector("div[is='emby-scroller']:not(.hide) select.selectSource:not([disabled])");
        return select && select.value ? select.value : null;
    }

    function getCurrentSubtitleIndex() {
        const select = document.querySelector("div[is='emby-scroller']:not(.hide) select.selectSubtitles");
        if (!select || select.value === '' || select.value === '-1') return null;
        return select.value;
    }

    async function resolvePlayableItem(apiClient, itemId) {
        const userId = apiClient.getCurrentUserId();
        let item = await apiClient.getItem(userId, itemId);

        if (item.Type === 'Series' && typeof apiClient.getNextUpEpisodes === 'function') {
            const nextUp = await apiClient.getNextUpEpisodes({ SeriesId: item.Id, UserId: userId, Limit: 1 });
            if (nextUp && nextUp.Items && nextUp.Items.length) {
                item = await apiClient.getItem(userId, nextUp.Items[0].Id);
            }
        } else if (item.Type === 'Season' && typeof apiClient.getItems === 'function') {
            const episodes = await apiClient.getItems(userId, {
                parentId: item.Id,
                Recursive: true,
                IsFolder: false,
                Limit: 1,
                SortBy: 'SortName'
            });
            if (episodes && episodes.Items && episodes.Items.length) {
                item = await apiClient.getItem(userId, episodes.Items[0].Id);
            }
        }

        if (item.MediaSources && item.MediaSources.length) return item;

        if (typeof apiClient.getItems === 'function') {
            const children = await apiClient.getItems(userId, {
                parentId: item.Id,
                Recursive: true,
                IsFolder: false,
                Limit: 1
            });
            if (children && children.Items && children.Items.length) {
                return apiClient.getItem(userId, children.Items[0].Id);
            }
        }

        return item;
    }

    function chooseMediaSource(item, preferredMediaSourceId) {
        const mediaSources = item.MediaSources || [];
        if (!mediaSources.length) return null;

        const selectedId = getCurrentMediaSourceId() || preferredMediaSourceId;
        return mediaSources.find(source => source.Id === selectedId) || mediaSources[0];
    }

    function chooseExternalSubtitle(mediaSource) {
        const streams = (mediaSource.MediaStreams || []).filter(stream => stream.Type === 'Subtitle' && stream.IsExternal);
        if (!streams.length) return null;

        const selectedIndex = getCurrentSubtitleIndex();
        if (selectedIndex !== null) {
            const selected = streams.find(stream => String(stream.Index) === String(selectedIndex));
            if (selected) return selected;
        }

        const defaultSubtitle = streams.find(stream => stream.IsDefault);
        if (defaultSubtitle) return defaultSubtitle;

        const chineseLanguages = ['chi', 'zho', 'zh', 'chs', 'cht'];
        return streams.find(stream => chineseLanguages.includes(String(stream.Language || '').toLowerCase())) || null;
    }

    function appendQuery(url, params) {
        const query = new URLSearchParams(params).toString();
        if (!query) return url;
        return url + (url.includes('?') ? '&' : '?') + query;
    }

    function buildStreamUrl(apiClient, item, mediaSource) {
        if (getStrmDirect() && /^https?:\/\//i.test(mediaSource.Path || '')) {
            return mediaSource.Path;
        }

        const accessToken = typeof apiClient.accessToken === 'function' ? apiClient.accessToken() : null;
        if (!accessToken) throw new Error('Missing Emby access token');

        const container = String(mediaSource.Container || 'mkv').replace(/[^a-z0-9]/gi, '') || 'mkv';
        const baseUrl = apiClient.getUrl(`Videos/${item.Id}/stream.${container}`);
        const params = {
            api_key: accessToken,
            Static: 'true',
            MediaSourceId: mediaSource.Id
        };
        if (typeof apiClient.deviceId === 'function') {
            const deviceId = apiClient.deviceId();
            if (deviceId) params.DeviceId = deviceId;
        }
        return appendQuery(baseUrl, params);
    }

    function buildSubtitleUrl(apiClient, item, mediaSource, subtitle) {
        if (!subtitle) return '';
        const accessToken = typeof apiClient.accessToken === 'function' ? apiClient.accessToken() : null;
        if (!accessToken) return '';

        const codec = String(subtitle.Codec || 'srt').replace(/[^a-z0-9]/gi, '') || 'srt';
        const url = apiClient.getUrl(`Videos/${item.Id}/${mediaSource.Id}/Subtitles/${subtitle.Index}/Stream.${codec}`);
        return appendQuery(url, { api_key: accessToken });
    }

    async function getMediaInfo(itemId, preferredMediaSourceId) {
        const apiClient = connectionManager.currentApiClient();
        const item = await resolvePlayableItem(apiClient, itemId);
        const mediaSource = chooseMediaSource(item, preferredMediaSourceId);
        if (!mediaSource || mediaSource.IsInfiniteStream) {
            throw new Error(getLabels().noMedia);
        }

        const subtitle = chooseExternalSubtitle(mediaSource);
        return {
            item: item,
            mediaSource: mediaSource,
            streamUrl: buildStreamUrl(apiClient, item, mediaSource),
            subtitleUrl: buildSubtitleUrl(apiClient, item, mediaSource, subtitle),
            positionMs: Math.max(0, Math.floor(((item.UserData && item.UserData.PlaybackPositionTicks) || 0) / 10000)),
            title: item.Name || mediaSource.Name || 'Emby',
            sourceName: mediaSource.Name || mediaSource.Container || ''
        };
    }

    function formatSeek(positionMs) {
        const totalSeconds = Math.max(0, Math.floor(positionMs / 1000));
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        return [hours, minutes, seconds].map(value => String(value).padStart(2, '0')).join(':');
    }

    function base64Url(text) {
        const bytes = new TextEncoder().encode(text);
        let binary = '';
        bytes.forEach(value => { binary += String.fromCharCode(value); });
        return btoa(binary).replace(/\//g, '_').replace(/\+/g, '-').replace(/=+$/g, '');
    }

    async function writeClipboard(text) {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch (_) {
                // Fall through to the legacy path.
            }
        }

        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();
        let copied = false;
        try {
            copied = document.execCommand('copy');
        } finally {
            textarea.remove();
        }
        return copied;
    }

    function safeTitle(title) {
        return String(title || 'Emby').replace(/["\r\n]/g, "'");
    }

    async function launchPlayer(playerId, mediaInfo) {
        const streamUrl = mediaInfo.streamUrl;
        const subtitleUrl = mediaInfo.subtitleUrl;
        const title = safeTitle(mediaInfo.title);
        const position = mediaInfo.positionMs;
        let targetUrl = '';

        switch (playerId) {
            case 'potplayer': {
                let command = `potplayer://${encodeURI(streamUrl)} /current`;
                if (subtitleUrl) command += ` /sub=${encodeURI(subtitleUrl)}`;
                if (position > 0) command += ` /seek=${formatSeek(position)}`;
                command += ` /title="${title}"`;
                const copied = await writeClipboard(command);
                targetUrl = copied ? 'potplayer:///current/clipboard' : command;
                break;
            }
            case 'vlc':
                if (OS.isAndroid()) {
                    targetUrl = `intent:${encodeURI(streamUrl)}#Intent;package=org.videolan.vlc;type=video/*;S.subtitles_location=${encodeURI(subtitleUrl)};S.title=${encodeURI(title)};i.position=${position};end`;
                } else if (OS.isIOS()) {
                    targetUrl = `vlc-x-callback://x-callback-url/stream?url=${encodeURIComponent(streamUrl)}&sub=${encodeURIComponent(subtitleUrl)}`;
                } else {
                    targetUrl = `vlc://${encodeURI(streamUrl)}`;
                }
                break;
            case 'mpv':
                if (OS.isMacOS()) {
                    targetUrl = `mpvplay://${encodeURI(streamUrl)}`;
                } else {
                    targetUrl = `mpv-handler://play/${base64Url(streamUrl)}`;
                    if (subtitleUrl) targetUrl += `/?subfile=${base64Url(subtitleUrl)}`;
                }
                break;
            case 'iina':
                targetUrl = `iina://weblink?url=${encodeURIComponent(streamUrl)}&new_window=1`;
                break;
            case 'infuse':
                targetUrl = `infuse://x-callback-url/play?url=${encodeURIComponent(streamUrl)}&sub=${encodeURIComponent(subtitleUrl)}`;
                break;
            case 'copy':
                if (await writeClipboard(streamUrl)) {
                    toast(getLabels().copySuccess);
                }
                return;
            default:
                return;
        }

        window.location.href = targetUrl;
    }

    function getAvailablePlayers() {
        const players = [];
        if (OS.isWindows()) players.push({ id: 'potplayer', name: 'PotPlayer', icon: 'play_arrow' });
        players.push({ id: 'vlc', name: 'VLC', icon: 'play_arrow' });
        if (OS.isWindows() || OS.isMacOS() || OS.isLinux()) players.push({ id: 'mpv', name: 'MPV', icon: 'play_arrow' });
        if (OS.isMacOS()) players.push({ id: 'iina', name: 'IINA', icon: 'play_arrow' });
        if (OS.isMacOS() || OS.isIOS()) players.push({ id: 'infuse', name: 'Infuse', icon: 'play_arrow' });
        players.push({ id: 'copy', name: getLabels().copy, icon: 'content_copy' });
        return players;
    }

    async function chooseAction(mediaInfo) {
        const labels = getLabels();
        const players = getAvailablePlayers();
        try {
            const modules = await require(['actionsheet']);
            const actionSheet = modules && modules[0];
            if (actionSheet && typeof actionSheet.show === 'function') {
                return actionSheet.show({
                    title: labels.externalPlay,
                    text: mediaInfo.sourceName ? `${mediaInfo.title}\n${mediaInfo.sourceName}` : mediaInfo.title,
                    items: players.map(player => ({
                        name: player.name,
                        id: player.id,
                        icon: player.icon
                    }))
                });
            }
        } catch (_) {
            // Fall back to Emby's standard dialog on clients without the action-sheet module.
        }

        const buttons = players.map(player => ({
            name: player.name,
            id: player.id,
            type: 'submit'
        }));
        buttons.push({ name: labels.cancel, id: 'cancel', type: 'cancel' });
        return dialog({
            title: labels.externalPlay,
            text: mediaInfo.sourceName ? `${mediaInfo.title}\n${mediaInfo.sourceName}` : mediaInfo.title,
            buttons: buttons,
            centerText: false
        });
    }

    async function show(itemId, preferredMediaSourceId) {
        if (!itemId || !config.enabled) return;

        loading.show();
        let mediaInfo;
        try {
            mediaInfo = await getMediaInfo(itemId, preferredMediaSourceId);
        } catch (error) {
            loading.hide();
            toast((error && error.message) || getLabels().failed);
            return;
        }
        loading.hide();

        const id = await chooseAction(mediaInfo);
        if (!id || id === 'cancel') return;

        try {
            await launchPlayer(id, mediaInfo);
        } catch (_) {
            toast(getLabels().failed);
        }
    }

    function getItemIdFromLocation() {
        const source = `${window.location.hash || ''}&${window.location.search || ''}`;
        const match = /[?&]id=([^&#]+)/i.exec(source);
        return match ? decodeURIComponent(match[1]) : null;
    }

    function removeDetailButton() {
        const existing = document.getElementById(detailButtonId);
        if (existing) existing.remove();
    }

    async function addDetailButton(itemId, attempt) {
        if (!config.enabled || !itemId) return;
        const container = document.querySelector("div[is='emby-scroller']:not(.hide) .mainDetailButtons");
        if (!container) {
            if ((attempt || 0) < 15) {
                setTimeout(() => addDetailButton(itemId, (attempt || 0) + 1), 200);
            }
            return;
        }

        if (document.getElementById(detailButtonId)) return;

        try {
            const apiClient = connectionManager.currentApiClient();
            const item = await apiClient.getItem(apiClient.getCurrentUserId(), itemId);
            if (!item || !supportedItemTypes.includes(item.Type)) return;
        } catch (_) {
            return;
        }

        const button = document.createElement('button');
        button.id = detailButtonId;
        button.type = 'button';
        button.className = 'detailButton emby-button emby-button-backdropfilter raised-backdropfilter detailButton-primary';
        button.title = getLabels().externalPlay;
        button.setAttribute('aria-label', getLabels().externalPlay);
        button.innerHTML = '<div class="detailButton-content"><i class="md-icon detailButton-icon button-icon button-icon-left material-icons">open_in_new</i><span class="button-text"></span></div>';
        const text = button.querySelector('.button-text');
        if (text) text.textContent = getLabels().externalPlay;
        button.addEventListener('click', () => show(itemId));
        container.appendChild(button);
    }

    function handleViewBeforeShow(event) {
        removeDetailButton();
        if (!config.enabled) return;
        const contextPath = event && event.detail && event.detail.contextPath;
        let itemId = null;
        if (contextPath && contextPath.includes('/item')) {
            const match = /[?&]id=([^&]+)/i.exec(contextPath);
            itemId = match ? decodeURIComponent(match[1]) : null;
        }
        itemId = itemId || getItemIdFromLocation();
        if (itemId) setTimeout(() => addDetailButton(itemId, 0), 0);
    }

    function init() {
        if (initialized || !config.enabled) return;
        initialized = true;
        document.addEventListener('viewbeforeshow', handleViewBeforeShow);
        const itemId = getItemIdFromLocation();
        if (itemId) setTimeout(() => addDetailButton(itemId, 0), 0);
    }

    return {
        init: init,
        loadConfig: loadConfig,
        show: show
    };
});
