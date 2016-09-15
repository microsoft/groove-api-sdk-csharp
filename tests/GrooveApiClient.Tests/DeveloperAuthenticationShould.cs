// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeveloperAuthenticationShould : TestBase
    {
        [TestMethod]
        public async Task CorrectlyAuthenticateWithAzureDataMarket()
        {
            AzureDataMarketAuthenticationCache client = new AzureDataMarketAuthenticationCache(AzureDataMarketClientId, AzureDataMarketClientSecret);
            AzureDataMarketAuthenticationCache.AccessToken accessToken = await client.CheckAndRenewTokenAsync(CancellationToken.None);

            Assert.IsFalse(string.IsNullOrEmpty(accessToken.Token));
            Assert.IsFalse(accessToken.Expiration < DateTimeOffset.UtcNow, "Validity delay should not be null");
        }
    }
}
