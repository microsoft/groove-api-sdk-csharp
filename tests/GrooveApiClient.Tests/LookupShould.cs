// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LookupShould
    {
        [TestMethod]
        public async Task SuccessfullyLookupValidContentWithoutUserAuthentication()
        {
            IGrooveClient client = GrooveClientFactory.CreateGrooveClient(
                TestSecrets.ClientId, 
                TestSecrets.ClientSecret);

            ContentResponse response = await client.LookupAsync(
                new List<string> { "music.D6670000-0200-11DB-89CA-0019B92A3933" }, 
                ContentSource.Catalog);

            Assert.AreEqual(response.Artists.Items[0].Id, "music.D6670000-0200-11DB-89CA-0019B92A3933", true,
                "artists.Items[0].Id should be the id we looked up on.");
            Assert.AreEqual(response.Artists.Items[0].Name, "Pink Floyd", true,
                "The artist should be 'Pink Floyd'.");
        }
    }
}
