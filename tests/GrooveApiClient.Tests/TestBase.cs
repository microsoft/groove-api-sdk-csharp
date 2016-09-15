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

        protected static string AzureDataMarketClientId { get; }
        protected static string AzureDataMarketClientSecret { get; }

        static TestBase()
        {
            AzureDataMarketClientId = ConfigurationManager.AppSettings["clientid"];
            AzureDataMarketClientSecret = ConfigurationManager.AppSettings["clientsecret"];

            Assert.IsNotNull(AzureDataMarketClientId, "The client id should be set in App.config");
            Assert.IsNotNull(AzureDataMarketClientSecret, "The client secret should be set in App.config");
            Assert.IsFalse(AzureDataMarketClientSecret.Contains("%"), "The client secret should not be URL encoded");

            Client = GrooveClientFactory.CreateGrooveClient(
                AzureDataMarketClientId,
                AzureDataMarketClientSecret);

            UserAuthenticatedClient = GrooveClientFactory.CreateGrooveClient(
                AzureDataMarketClientId,
                AzureDataMarketClientSecret, 
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
