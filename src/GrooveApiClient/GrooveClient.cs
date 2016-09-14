// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContract;
    using DataContract.CollectionEdit;

    internal class GrooveClient : SimpleServiceClient, IGrooveClient
    {
        private static readonly Uri Hostname;
        private static readonly string ClientVersion;

        static GrooveClient()
        {
            Hostname = new Uri("https://api.media.microsoft.com");
            Assembly assembly = typeof(GrooveClient).GetTypeInfo().Assembly;
            var assemblyName = new AssemblyName(assembly.FullName);
            ClientVersion = assemblyName.Version.Major + "." + assemblyName.Version.Minor;
        }

        private readonly AzureDataMarketAuthenticationCache _azureDataMarketAuthenticationCache;
        private readonly IUserTokenManager _userTokenManager;

        internal GrooveClient(AzureDataMarketAuthenticationCache azureDataMarketAuthenticationCache, IUserTokenManager userTokenManager)
        {
            _azureDataMarketAuthenticationCache = azureDataMarketAuthenticationCache;
            _userTokenManager = userTokenManager;
        }

        private async Task<ContentResponse> SearchApiAsync(
            MediaNamespace mediaNamespace, 
            string query = null, 
            ContentSource? source = null,
            SearchFilter filter = 
            SearchFilter.Default, 
            string language = null, 
            string country = null,
            int? maxItems = null, 
            string continuationToken = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country, source);

            if (!string.IsNullOrEmpty(query))
                requestParameters.Add("q", Uri.EscapeDataString(query));

            if (filter != SearchFilter.Default)
                requestParameters.Add("filters", filter.ToString().Replace(", ", "+"));

            if (maxItems.HasValue)
                requestParameters.Add("maxItems", maxItems.ToString());

            if (_userTokenManager?.UserIsSignedIn == true)
            {
                return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                    headers => GetAsync<ContentResponse>(
                        Hostname,
                        "/1/content/" + mediaNamespace + "/search",
                        new CancellationToken(false),
                        requestParameters,
                        headers));
            }
            else
            {
                Dictionary<string, string> requestHeaders = FormatRequestHeaders(null);

                return await GetAsync<ContentResponse>(
                    Hostname,
                    "/1/content/" + mediaNamespace + "/search",
                    new CancellationToken(false),
                    requestParameters,
                    requestHeaders);
            }
        }

        private async Task<ContentResponse> LookupApiAsync(
            IEnumerable<string> itemIds, 
            ContentSource? source = null,
            string language = null, 
            string country = null, 
            ExtraDetails extras = ExtraDetails.None,
            string continuationToken = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country, source);

            if (extras != ExtraDetails.None)
            {
                string extra = extras.ToString().Replace(", ", "+");
                requestParameters.Add("extras", extra);
            }

            string ids = itemIds.Aggregate("",
                (current, id) =>
                    current + (!string.IsNullOrEmpty(current) ? "+" : "") + id);

            if (_userTokenManager?.UserIsSignedIn == true)
            {
                return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                    headers => GetAsync<ContentResponse>(
                        Hostname,
                        "/1/content/" + ids + "/lookup",
                        new CancellationToken(false),
                        requestParameters,
                        headers));
            }
            else
            {
                Dictionary<string, string> requestHeaders = FormatRequestHeaders(null);
                return await GetAsync<ContentResponse>(
                    Hostname,
                    "/1/content/" + ids + "/lookup",
                    new CancellationToken(false),
                    requestParameters,
                    requestHeaders);
            }
        }

        private async Task<ContentResponse> BrowseApiAsync(
            MediaNamespace mediaNamespace, 
            ContentSource source, 
            ItemType type,
            OrderBy? orderBy = null, 
            int? maxItems = null, 
            int? page = null,
            string country = null, 
            string language = null, 
            string continuationToken = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country);

            if (orderBy.HasValue)
                requestParameters.Add("orderby", orderBy.ToString());

            if (maxItems.HasValue)
                requestParameters.Add("maxitems", maxItems.ToString());

            if (page.HasValue)
                requestParameters.Add("page", page.ToString());

            if (_userTokenManager?.UserIsSignedIn == true)
            {
                return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                    headers => GetAsync<ContentResponse>(
                        Hostname,
                        "/1/content/" + mediaNamespace + "/" + source + "/" + type + "/browse",
                        new CancellationToken(false),
                        requestParameters,
                        headers));
            }
            else
            {
                Dictionary<string, string> requestHeaders = FormatRequestHeaders(null);
                return await GetAsync<ContentResponse>(
                    Hostname,
                    "/1/content/" + mediaNamespace + "/" + source + "/" + type + "/browse",
                    new CancellationToken(false),
                    requestParameters,
                    requestHeaders);
            }
        }

        private async Task<ContentResponse> DiscoverAsync(
            MediaNamespace mediaNamespace, 
            string type,
            string country = null, 
            string language = null, 
            string genre = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeaders(null);
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(language: language, country: country);

            if (!string.IsNullOrEmpty(genre))
                requestParameters.Add("genre", genre);

            return await GetAsync<ContentResponse>(
                Hostname, 
                "/1/content/" + mediaNamespace + "/" + type,
                new CancellationToken(false), 
                requestParameters,
                requestHeaders);
        }

        private async Task<StreamResponse> LocationAsync(
            string id, 
            string clientInstanceId,
            string type, 
            string country = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(country: country);

            if (!string.IsNullOrEmpty(clientInstanceId))
                requestParameters.Add("clientInstanceId", clientInstanceId);

            if (_userTokenManager?.UserIsSignedIn == true)
            {
                return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                    headers => GetAsync<StreamResponse>(
                        Hostname,
                        "/1/content/" + id + "/" + type,
                        new CancellationToken(false),
                        requestParameters,
                        headers));
            }
            else
            {
                Dictionary<string, string> requestHeaders = FormatRequestHeaders(null);
                return await GetAsync<StreamResponse>(
                    Hostname,
                    "/1/content/" + id + "/" + type,
                    new CancellationToken(false),
                    requestParameters,
                    requestHeaders);
            }
        }

        public Task<ContentResponse> SearchAsync(
            MediaNamespace mediaNamespace, 
            string query, 
            ContentSource? source = null, 
            SearchFilter filter = SearchFilter.Default,
            string language = null, 
            string country = null, 
            int? maxItems = null)
        {
            return SearchApiAsync(mediaNamespace, query, source, filter, language, country, maxItems);
        }

        public Task<ContentResponse> SearchContinuationAsync(
            MediaNamespace mediaNamespace, 
            string continuationToken)
        {
            return SearchApiAsync(mediaNamespace, continuationToken: continuationToken);
        }

        public Task<ContentResponse> LookupAsync(
            List<string> itemIds, 
            ContentSource? source = null, 
            string language = null,
            string country = null, 
            ExtraDetails extras = ExtraDetails.None)
        {
            return LookupApiAsync(itemIds, source, language, country, extras);
        }

        public Task<ContentResponse> LookupAsync(
            string itemId,
            ContentSource? source = null,
            string language = null,
            string country = null,
            ExtraDetails extras = ExtraDetails.None)
        {
            return LookupApiAsync(new List<string> {itemId}, source, language, country, extras);
        }

        public Task<ContentResponse> LookupContinuationAsync(
            List<string> itemIds, 
            string continuationToken)
        {
            return LookupApiAsync(itemIds, continuationToken: continuationToken);
        }

        public Task<ContentResponse> LookupContinuationAsync(
            string itemId,
            string continuationToken)
        {
            return LookupApiAsync(new List<string> {itemId}, continuationToken: continuationToken);
        }

        public Task<ContentResponse> BrowseAsync(
            MediaNamespace mediaNamespace, 
            ContentSource source, 
            ItemType type,
            OrderBy? orderBy = null, 
            int? maxItems = null, 
            int? page = null, 
            string country = null,
            string language = null)
        {
            return BrowseApiAsync(mediaNamespace, source, type, orderBy, maxItems, page, country, language);
        }

        public Task<ContentResponse> BrowseContinuationAsync(
            MediaNamespace mediaNamespace, 
            ContentSource source, 
            ItemType type,
            string continuationToken)
        {
            return BrowseApiAsync(mediaNamespace, source, type, continuationToken: continuationToken);
        }

        public Task<ContentResponse> SpotlightApiAsync(
            MediaNamespace mediaNamespace, 
            string language = null,
            string country = null)
        {
            return DiscoverAsync(mediaNamespace, "spotlight", country, language);
        }

        public Task<ContentResponse> NewReleasesApiAsync(
            MediaNamespace mediaNamespace, 
            string genre = null,
            string language = null, 
            string country = null)
        {
            return DiscoverAsync(mediaNamespace, "newreleases", country, language, genre);
        }

        public async Task<ContentResponse> BrowseGenresAsync(
            MediaNamespace mediaNamespace,
            string country = null, 
            string language = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(language: language, country: country);
            return await GetAsync<ContentResponse>(
                Hostname, 
                "/1/content/" + mediaNamespace + "/catalog/genres",
                new CancellationToken(false), 
                requestParameters);
        }

        public async Task<TrackActionResponse> CollectionOperationAsync(
            MediaNamespace mediaNamespace, 
            TrackActionType operation, 
            TrackActionRequest trackActionRequest)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync();

            return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                headers => PostAsync<TrackActionResponse, TrackActionRequest>(
                    Hostname,
                    "/1/content/" + mediaNamespace + "/collection/" + operation,
                    trackActionRequest,
                    new CancellationToken(false),
                    requestParameters,
                    headers));
        }

        public async Task<PlaylistActionResponse> PlaylistOperationAsync(
            MediaNamespace mediaNamespace, 
            PlaylistActionType operation,
            PlaylistAction playlistAction)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync();

            return await ApiCallWithUserAuthorizationHeaderRefreshAsync(
                headers => PostAsync<PlaylistActionResponse, PlaylistAction>(
                    Hostname,
                    "/1/content/" + mediaNamespace + "/collection/playlists/" + operation,
                    playlistAction,
                    new CancellationToken(false),
                    requestParameters,
                    headers));
        }

        public Task<StreamResponse> StreamAsync(
            string id, 
            string clientInstanceId)
        {
            return LocationAsync(id, clientInstanceId, "stream");
        }

        public Task<StreamResponse> PreviewAsync(
            string id, 
            string clientInstanceId, 
            string country = null)
        {
            return LocationAsync(id, clientInstanceId, "preview", country);
        }

        private async Task<TResponse> ApiCallWithUserAuthorizationHeaderRefreshAsync<TResponse>(
            Func<Dictionary<string, string>, Task<TResponse>> apiCall)
            where TResponse : BaseResponse
        {
            string userAuthorizationHeader = await _userTokenManager.GetUserAuthorizationHeaderAsync();
            Dictionary<string, string> requestHeaders = FormatRequestHeaders(userAuthorizationHeader);

            TResponse response = await apiCall(requestHeaders);

            if (response.Error?.ErrorCode == ErrorCode.INVALID_AUTHORIZATION_HEADER.ToString("G"))
            {
                userAuthorizationHeader = await _userTokenManager.GetUserAuthorizationHeaderAsync(true);
                requestHeaders = FormatRequestHeaders(userAuthorizationHeader);

                response = await apiCall(requestHeaders);
            }

            return response;
        }

        private async Task<Dictionary<string, string>> FormatRequestParametersAsync(
            string continuationToken = null,
            string language = null, 
            string country = null, 
            ContentSource? source = null)
        {
            AzureDataMarketAuthenticationCache.AccessToken token = await _azureDataMarketAuthenticationCache.CheckAndRenewTokenAsync(new CancellationToken(false));

            Dictionary<string, string> requestParameters = new Dictionary<string, string>
            {
                {"accessToken", Uri.EscapeDataString("Bearer " + token.Token)}
            };

            if (!string.IsNullOrEmpty(continuationToken))
                requestParameters.Add("continuationToken", continuationToken);

            if (!string.IsNullOrEmpty(language))
                requestParameters.Add("language", language);

            if (!string.IsNullOrEmpty(country))
                requestParameters.Add("country", country);

            if (source.HasValue)
            {
                string sources = source.ToString().Replace(", ", "+");
                requestParameters.Add("source", sources);
            }

            return requestParameters;
        }

        private static Dictionary<string, string> FormatRequestHeaders(string userAuthorizationHeader)
        {
            Dictionary<string, string> headersToSend = new Dictionary<string, string>
            {
                { "X-Client-Name", "GrooveClientSDK" },
                { "X-Client-Version", ClientVersion }
            };

            if (userAuthorizationHeader != null)
            {
                headersToSend.Add("Authorization", userAuthorizationHeader);
            }

            return headersToSend;
        }
    }
}
