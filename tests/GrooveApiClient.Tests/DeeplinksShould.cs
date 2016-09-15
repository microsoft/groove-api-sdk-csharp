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
    public class DeeplinksShould : TestBase
    {
        [TestMethod, TestCategory("Unauthenticated")]
        public async Task CorrectlyBeReturned()
        {
            const string katyPerryId = "music.97e60200-0200-11db-89ca-0019b92a3933";

            // Lookup Katy Perry's information, including latest album releases
            ContentResponse lookupResponse = await Client.LookupAsync(katyPerryId, extras: ExtraDetails.Albums, country: "US").Log();
            Artist artist = lookupResponse.Artists.Items.First();

            // Create a link to Katy Perry's artist page in a Groove Music client
            string artistPageDeepLink = artist.Link;
            Console.WriteLine($"Artist page deep link: {artistPageDeepLink}");
            Assert.IsNotNull(artistPageDeepLink, "The artist page deep link should not be null");

            // Create a link which starts playback of Katy Perry's latest album in the US (exclude singles and EPs)
            Album album = artist.Albums.Items.First(a => a.AlbumType == "Album");
            string albumPlayDeepLink = album.GetLink(ContentExtensions.LinkAction.Play);
            Console.WriteLine($"Album play deep link: {albumPlayDeepLink}");
            Assert.IsNotNull(albumPlayDeepLink, "The album play deep link should not be null");
        }
    }
}
