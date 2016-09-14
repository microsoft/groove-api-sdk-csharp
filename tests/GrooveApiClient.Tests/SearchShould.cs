// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System.Threading.Tasks;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SearchShould
    {
        [TestMethod]
        public async Task SuccessfullySearchArtistsInCatalogWithoutUserAuthentication()
        {
            IGrooveClient client = GrooveClientFactory.CreateGrooveClient(
                TestSecrets.ClientId, 
                TestSecrets.ClientSecret);

            ContentResponse response = await client.SearchAsync(MediaNamespace.music, "Pink Floyd", ContentSource.Catalog, SearchFilter.Artists);

            Assert.AreEqual(response.Artists.Items[0].Name, "Pink Floyd", true,
                "The first artist for query 'Pink Floyd' should be 'Pink Floyd'.");
            Assert.IsNull(response.Albums, "Albums should be null");
            Assert.IsNull(response.Tracks, "Tracks should be null");
        }
    }
}
