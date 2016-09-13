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
    using System.Threading;
    using System.Threading.Tasks;
    using DataContract;
    using DataContract.CollectionEdit;

    internal class GrooveClient : SimpleServiceClient, IGrooveClient
    {
        private static readonly Uri Hostname = new Uri("https://api.media.microsoft.com");

        private readonly AzureDataMarketAuthenticationCache _azureDataMarketAuthenticationCache;

        internal GrooveClient(string azureDataMarketClientId, string azureDataMarketClientSecret)
        {
            _azureDataMarketAuthenticationCache = new AzureDataMarketAuthenticationCache(azureDataMarketClientId, azureDataMarketClientSecret);
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
            string continuationToken = null,
            string userToken = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeadersAsync(userToken);
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country, source);

            if (!string.IsNullOrEmpty(query))
                requestParameters.Add("q", Uri.EscapeDataString(query));

            if (filter != SearchFilter.Default)
                requestParameters.Add("filters", filter.ToString().Replace(", ", "+"));

            if (maxItems.HasValue)
                requestParameters.Add("maxItems", maxItems.ToString());

            return await GetAsync<ContentResponse>(
                Hostname, 
                "/1/content/" + mediaNamespace + "/search",
                new CancellationToken(false), 
                requestParameters,
                requestHeaders);
        }

        private async Task<ContentResponse> LookupApiAsync(
            IEnumerable<string> itemIds, 
            ContentSource? source = null,
            string language = null, 
            string country = null, 
            ExtraDetails extras = ExtraDetails.None,
            string continuationToken = null,
            string userToken = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeadersAsync(userToken);
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country, source);
            if (extras != ExtraDetails.None)
            {
                string extra = extras.ToString().Replace(", ", "+");
                requestParameters.Add("extras", extra);
            }

            string ids = itemIds.Aggregate("",
                (current, id) =>
                    current + (!string.IsNullOrEmpty(current) ? "+" : "") + id);

            return await GetAsync<ContentResponse>(
                Hostname, 
                "/1/content/" + ids + "/lookup", 
                new CancellationToken(false), 
                requestParameters,
                requestHeaders);
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
            string continuationToken = null,
            string userToken = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeadersAsync(userToken);
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(continuationToken, language, country);

            if (orderBy.HasValue)
                requestParameters.Add("orderby", orderBy.ToString());

            if (maxItems.HasValue)
                requestParameters.Add("maxitems", maxItems.ToString());

            if (page.HasValue)
                requestParameters.Add("page", page.ToString());

            return await GetAsync<ContentResponse>(
                Hostname,
                "/1/content/" + mediaNamespace + "/" + source + "/" + type + "/browse",
                new CancellationToken(false), 
                requestParameters,
                requestHeaders);
        }

        private async Task<ContentResponse> DiscoverAsync(
            MediaNamespace mediaNamespace, 
            string type,
            string country = null, 
            string language = null, 
            string genre = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeadersAsync(null);
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
            string country = null, 
            string userToken = null)
        {
            Dictionary<string, string> requestHeaders = FormatRequestHeadersAsync(userToken);
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync(country: country);

            if (!string.IsNullOrEmpty(clientInstanceId))
                requestParameters.Add("clientInstanceId", clientInstanceId);

            return await GetAsync<StreamResponse>(
                Hostname, 
                "/1/content/" + id + "/" + type, 
                new CancellationToken(false),
                requestParameters, 
                requestHeaders);
        }

        public Task<ContentResponse> SearchAsync(
            MediaNamespace mediaNamespace, 
            string query, 
            ContentSource? source = null, 
            SearchFilter filter = SearchFilter.Default,
            string language = null, 
            string country = null, 
            int? maxItems = null,
            string userToken = null)
        {
            return SearchApiAsync(mediaNamespace, query, source, filter, language, country, maxItems, userToken: userToken);
        }

        public Task<ContentResponse> SearchContinuationAsync(
            MediaNamespace mediaNamespace, 
            string continuationToken,
            string userToken = null)
        {
            return SearchApiAsync(mediaNamespace, continuationToken: continuationToken, userToken: userToken);
        }

        public Task<ContentResponse> LookupAsync(
            List<string> itemIds, 
            ContentSource? source = null, 
            string language = null,
            string country = null, 
            ExtraDetails extras = ExtraDetails.None,
            string userToken = null)
        {
            return LookupApiAsync(itemIds, source, language, country, extras, userToken: userToken);
        }

        public Task<ContentResponse> LookupAsync(
            string itemId,
            ContentSource? source = null,
            string language = null,
            string country = null,
            ExtraDetails extras = ExtraDetails.None,
            string userToken = null)
        {
            return LookupApiAsync(new List<string> {itemId}, source, language, country, extras, userToken: userToken);
        }

        public Task<ContentResponse> LookupContinuationAsync(
            List<string> itemIds, 
            string continuationToken,
            string userToken = null)
        {
            return LookupApiAsync(itemIds, continuationToken: continuationToken, userToken: userToken);
        }

        public Task<ContentResponse> LookupContinuationAsync(
            string itemId,
            string continuationToken,
            string userToken = null)
        {
            return LookupApiAsync(new List<string> {itemId}, continuationToken: continuationToken, userToken: userToken);
        }

        public Task<ContentResponse> BrowseAsync(
            MediaNamespace mediaNamespace, 
            ContentSource source, 
            ItemType type,
            OrderBy? orderBy = null, 
            int? maxItems = null, 
            int? page = null, 
            string country = null,
            string language = null,
            string userToken = null)
        {
            return BrowseApiAsync(mediaNamespace, source, type, orderBy, maxItems, page, country, language, userToken: userToken);
        }

        public Task<ContentResponse> BrowseContinuationAsync(
            MediaNamespace mediaNamespace, 
            ContentSource source, 
            ItemType type,
            string continuationToken,
            string userToken = null)
        {
            return BrowseApiAsync(mediaNamespace, source, type, continuationToken: continuationToken, userToken: userToken);
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
            TrackActionRequest trackActionRequest,
            string userToken = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync();
            return await PostAsync<TrackActionResponse, TrackActionRequest>(
                Hostname, 
                "/1/content/" + mediaNamespace + "/collection/" + operation,
                trackActionRequest, 
                new CancellationToken(false), 
                requestParameters);
        }

        public async Task<PlaylistActionResponse> PlaylistOperationAsync(
            MediaNamespace mediaNamespace, 
            PlaylistActionType operation,
            PlaylistAction playlistAction,
            string userToken = null)
        {
            Dictionary<string, string> requestParameters = await FormatRequestParametersAsync();
            return await PostAsync<PlaylistActionResponse, PlaylistAction>(
                Hostname, 
                "/1/content/" + mediaNamespace + "/collection/playlists/" + operation,
                playlistAction, 
                new CancellationToken(false), 
                requestParameters);
        }

        public Task<StreamResponse> StreamAsync(
            string id, 
            string clientInstanceId, 
            string userToken)
        {
            return LocationAsync(id, clientInstanceId, "stream", userToken: userToken);
        }

        public Task<StreamResponse> PreviewAsync(
            string id, 
            string clientInstanceId, 
            string country = null)
        {
            return LocationAsync(id, clientInstanceId, "preview", country);
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

        private static Dictionary<string, string> FormatRequestHeadersAsync(string userToken)
        {
            Dictionary<string, string> headersToSend = new Dictionary<string, string>
            {
                { "X-Client-Name", "GrooveClientSDK" },
                { "X-Client-Version", "1" },
            };

            if (userToken != null)
            {
                headersToSend.Add("Authorization", $"Bearer {userToken}");
            }

            return headersToSend;
        }
    }
}
