// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System.Configuration;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    public abstract class TestBase
    {
        protected static IGrooveClient Client { get; }
        protected static IGrooveClient UserAuthenticatedClient { get; }

        // When implementing real calls, use a unique stable client instance id per client id.
        protected const string ClientInstanceId = "GrooveClientTests12345678901234567890";

        protected static string MicrosoftAppClientId { get; }
        protected static string MicrosoftAppClientSecret { get; }

        static TestBase()
        {
            MicrosoftAppClientId = ConfigurationManager.AppSettings["clientid"];
            MicrosoftAppClientSecret = ConfigurationManager.AppSettings["clientsecret"];

            Assert.IsNotNull(MicrosoftAppClientId, "The client id should be set in App.config");
            Assert.IsNotNull(MicrosoftAppClientSecret, "The client secret should be set in App.config");
            Assert.IsFalse(MicrosoftAppClientSecret.Contains("%"), "The client secret should not be URL encoded");

            Client = GrooveClientFactory.CreateGrooveClient(
                MicrosoftAppClientId,
                MicrosoftAppClientSecret);

            UserAuthenticatedClient = GrooveClientFactory.CreateGrooveClient(
                MicrosoftAppClientId,
                MicrosoftAppClientSecret, 
                new TestUserTokenManagerFromConfigurationValue());
        }

        protected void AssertPaginatedListIsValid<TContent>(
            PaginatedList<TContent> list, 
            int minItems,
            int? minTotalItems = null)
        {
            Assert.IsNotNull(list, $"Results should contain {typeof(TContent)}");
            Assert.IsNotNull(list.Items, $"Results should contain {typeof(TContent)} items");
            Assert.IsTrue(minItems <= list.Items.Count,
                $"Results should contain more than {minItems}{typeof(TContent)} items");

            if (minTotalItems != null)
            {
                Assert.IsTrue(minTotalItems <= list.TotalItemCount,
                    $"The total number of {typeof(TContent)} should be greater than {minTotalItems}");
            } 
        }
    }
}
