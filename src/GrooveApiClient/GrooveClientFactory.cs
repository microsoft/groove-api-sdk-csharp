// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System.Collections.Generic;

    public static class GrooveClientFactory
    {
        /// <summary>
        /// We will reuse Azure Data Market tokens as much as possible.
        /// </summary>
        private static readonly Dictionary<string, AzureDataMarketAuthenticationCache> AzureDataMarketAuthenticationCaches =
            new Dictionary<string, AzureDataMarketAuthenticationCache>();

        /// <summary>
        /// Create a Groove client with user authentication.
        /// </summary>
        /// <param name="azureDataMarketClientId">Azure Data Market application client id</param>
        /// <param name="azureDataMarketClientSecret">Azure Data Market application secret</param>
        /// <param name="userTokenManager"><see cref="IUserTokenManager"/> implementation</param>
        public static IGrooveClient CreateGrooveClient(
            string azureDataMarketClientId, 
            string azureDataMarketClientSecret, 
            IUserTokenManager userTokenManager)
        {
            return new GrooveClient(
                GetOrAddAzureDataMarketAuthenticationCache(azureDataMarketClientId, azureDataMarketClientSecret), 
                userTokenManager);
        }

        /// <summary>
        /// Create a Groove client without user authentication.
        /// </summary>
        /// <param name="azureDataMarketClientId">Azure Data Market application client id</param>
        /// <param name="azureDataMarketClientSecret">Azure Data Market application secret</param>
        public static IGrooveClient CreateGrooveClient(
            string azureDataMarketClientId,
            string azureDataMarketClientSecret)
        {
            return new GrooveClient(GetOrAddAzureDataMarketAuthenticationCache(azureDataMarketClientId, azureDataMarketClientSecret));
        }

        private static AzureDataMarketAuthenticationCache GetOrAddAzureDataMarketAuthenticationCache(
            string azureDataMarketClientId,
            string azureDataMarketClientSecret)
        {
            if (!AzureDataMarketAuthenticationCaches.ContainsKey(azureDataMarketClientId))
            {
                AzureDataMarketAuthenticationCache azureDataMarketAuthenticationCache = new AzureDataMarketAuthenticationCache(
                    azureDataMarketClientId,
                    azureDataMarketClientSecret);

                AzureDataMarketAuthenticationCaches[azureDataMarketClientId] = azureDataMarketAuthenticationCache;
            }

            return AzureDataMarketAuthenticationCaches[azureDataMarketClientId];
        }
    }
}
