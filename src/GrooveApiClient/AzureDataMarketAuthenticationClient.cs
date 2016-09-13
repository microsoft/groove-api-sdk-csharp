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
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContract.Authentication;

    /// <summary>
    /// Basic Azure Data Market (http://datamarket.azure.com/) authentication client
    /// </summary>
    public class AzureDataMarketAuthenticationClient : SimpleServiceClient
    {
        private readonly Uri _hostname = new Uri("https://datamarket.accesscontrol.windows.net");

        /// <summary>
        /// Authenticate an application on Azure Data Market
        /// </summary>
        /// <param name="clientId">The application's client ID</param>
        /// <param name="clientSecret">The application's secret</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<AzureDataMarketAuthenticationResponse> AuthenticateAsync(string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            Dictionary<string, string> request = new Dictionary<string, string>()
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"scope", "http://music.xboxlive.com/"},
                {"grant_type", "client_credentials"}
            };
            return PostAsync<AzureDataMarketAuthenticationResponse, Dictionary<string, string>>(_hostname, "/v2/OAuth2-13", request, cancellationToken);
        }

        protected override HttpContent CreateHttpContent<TRequest>(TRequest requestPayload, StreamWriter writer, MemoryStream stream)
        {
            // We need the url-encoded data for Azure authentication
            return new FormUrlEncodedContent(requestPayload as Dictionary<string, string>);
        }
    }
}
