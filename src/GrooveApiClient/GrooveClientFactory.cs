// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    public static class GrooveClientFactory
    {
        /// <summary>
        /// Create a Groove client
        /// </summary>
        /// <param name="azureDataMarketClientId">Azure Data Market application client id</param>
        /// <param name="azureDataMarketClientSecret">Azure Data Market application secret</param>
        /// <param name="userTokenManager"><see cref="IUserTokenManager"/> implementation</param>
        public static IGrooveClient CreateGrooveClient(
            string azureDataMarketClientId, 
            string azureDataMarketClientSecret, 
            IUserTokenManager userTokenManager)
        {
            return new GrooveClient(azureDataMarketClientId, azureDataMarketClientSecret, userTokenManager);
        }
    }
}
