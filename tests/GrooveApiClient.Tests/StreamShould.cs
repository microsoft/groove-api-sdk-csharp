// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamShould : TestBase
    {
        [TestMethod, TestCategory("Unauthenticated")]
        public async Task TestPreview()
        {
            // Get popular tracks in Great Britain
            ContentResponse browseResults = await Client.BrowseAsync(
                MediaNamespace.music, 
                ContentSource.Catalog, 
                ItemType.Tracks, 
                country: "GB").Log();

            Assert.IsNotNull(browseResults, "The browse response should not be null");
            AssertPaginatedListIsValid(browseResults.Tracks, 25, 100);

            // Request a preview stream URL for the first one
            Track track = browseResults.Tracks.Items.First();
            StreamResponse streamResponse = await Client.PreviewAsync(
                track.Id, 
                ClientInstanceId, 
                country: "GB").Log();

            Assert.IsNotNull(streamResponse, "The preview stream URL response should not be null");
            Assert.IsNotNull(streamResponse.Url, "The preview stream URL should not be null");
            Assert.IsNotNull(streamResponse.ContentType, "The preview stream content type should not be null");
        }

        [TestMethod, TestCategory("Authenticated")]
        public async Task TestStream()
        {
            // Check that the user has a Groove Music Pass subscription
            UserProfileResponse userProfileResponse = await UserAuthenticatedClient.GetUserProfileAsync(MediaNamespace.music).Log();
            // Beware: HasSubscription is bool?. You want != true instead of == false
            if (userProfileResponse.HasSubscription != true)
            {
                Assert.Inconclusive("The user doesn't have a Groove Music Pass subscription. Cannot stream from catalog.");
            }

            // Get popular tracks in the user's country
            ContentResponse browseResults = await UserAuthenticatedClient.BrowseAsync(
                MediaNamespace.music, 
                ContentSource.Catalog, 
                ItemType.Tracks).Log();

            Assert.IsNotNull(browseResults, "The browse response should not be null");
            AssertPaginatedListIsValid(browseResults.Tracks, 25, 100);

            // Stream the first streamable track
            Track track = browseResults.Tracks.Items.First(t => t.Rights.Contains("Stream"));
            StreamResponse streamResponse = await UserAuthenticatedClient.StreamAsync(track.Id, ClientInstanceId).Log();

            Assert.IsNotNull(streamResponse, "The stream URL response should not be null");
            Assert.IsNotNull(streamResponse.Url, "The stream URL should not be null");
            Assert.IsNotNull(streamResponse.ContentType, "The stream content type should not be null");
            Assert.IsNotNull(streamResponse.ExpiresOn, "The stream expiry date should not be null");
        }
    }
}
