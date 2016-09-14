// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContract.Authentication;

    /// <summary>
    /// Basic Azure Data Market (http://datamarket.azure.com/) authentication cache
    /// </summary>
    public class AzureDataMarketAuthenticationCache : IDisposable
    {
        public class AccessToken
        {
            public string Token { get; set; }
            public DateTimeOffset Expiration { get; set; }
        }

        private readonly string _clientId;
        private readonly string _clientSecret;
        private AccessToken _token;

        private readonly AzureDataMarketAuthenticationClient _client = new AzureDataMarketAuthenticationClient();

        /// <summary>
        /// Cache an application's authentication token on Azure Data Market
        /// </summary>
        /// <param name="clientId">The application's client ID</param>
        /// <param name="clientSecret">The application's secret</param>
        public AzureDataMarketAuthenticationCache(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// Get the application's token. Renew it if needed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AccessToken> CheckAndRenewTokenAsync(CancellationToken cancellationToken)
        {
            if (_token == null || _token.Expiration < DateTimeOffset.UtcNow)
            {
                // This is not thread safe. Unfortunately, portable class library requirements prevent use of
                // asynchronous locking mechanisms. The risk here is authenticating multiple times in parallel
                // which is bad from a performance standpoint but is transparent from a functional standpoint.
                AzureDataMarketAuthenticationResponse authenticationResponse = await _client.AuthenticateAsync(
                    _clientId, 
                    _clientSecret, 
                    cancellationToken);

                if (authenticationResponse != null)
                {
                    _token = new AccessToken
                    {
                        Token = authenticationResponse.AccessToken,
                        Expiration = DateTimeOffset.UtcNow.AddSeconds(authenticationResponse.ExpiresIn)
                    };
                }
            }

            return _token;
        }
    }
}
