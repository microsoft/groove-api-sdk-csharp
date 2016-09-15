// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;

    internal class TestUserTokenManagerFromConfigurationValue : IUserTokenManager
    {
        public bool UserIsSignedIn => true;

        public async Task<string> GetUserAuthorizationHeaderAsync(bool forceRefresh)
        {
            await Task.Yield();

            string userAuthorizationHeader = ConfigurationManager.AppSettings["userAuthorizationHeader"];
            if (string.IsNullOrEmpty(userAuthorizationHeader) 
                || userAuthorizationHeader.Length < 20 
                || !userAuthorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Inconclusive("The Authorization header value should be set in App.config for user authenticated tests");
            }

            return userAuthorizationHeader;
        }
    }
}
