// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CollectionShould : TestBase
    {
        [TestMethod, TestCategory("Unauthenticated")]
        public async Task LookupPublicPlaylist()
        {
            // Tip: You get your own playlistId by opening your playlist on http://music.microsoft.com
            //      If the page is http://music.microsoft.com/playlist/great-music/66cd8e9d-802a-00fe-364d-3ead4f82facf
            //      the id is music.playlist.66cd8e9d-802a-00fe-364d-3ead4f82facf .
            const string playlistId = "music.playlist.0016e20b-80c0-00fe-fac0-1a47365516d1";

            // Get playlist contents as viewed from the US
            ContentResponse playlistUsResponse = await Client.LookupAsync(
                playlistId, 
                ContentSource.Collection, 
                country: "US").Log();

            foreach (Track track in playlistUsResponse.Playlists.Items.First().Tracks.Items)
            {
                Console.WriteLine($"  Track {track.Id} can be {string.Join(" and ", track.Rights)} in the US");
            }

            // Get playlist contents as viewed from Brasil
            // Note that rights (such as Stream, FreeStream and Purchase) and collection item ids can be country specific
            ContentResponse playlistBrResponse = await Client.LookupAsync(
                playlistId, 
                ContentSource.Collection, 
                country: "BR").Log();

            foreach (Track track in playlistBrResponse.Playlists.Items.First().Tracks.Items)
            {
                Console.WriteLine($"  Track {track.Id} can be {string.Join(" and ", track.Rights)} in Brasil");
            }
        }

        [TestMethod, TestCategory("Authenticated")]
        public async Task BrowseUserPlaylists()
        {
            // Get all the user's playlists
            ContentResponse browseResults = await UserAuthenticatedClient.BrowseAsync(
                MediaNamespace.music, 
                ContentSource.Collection, 
                ItemType.Playlists).Log();

            Assert.IsNotNull(browseResults, "The browse response should not be null");
            AssertPaginatedListIsValid(browseResults.Playlists, 1);
        }
    }
}
