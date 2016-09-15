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
    public class ImagesShould : TestBase
    {
        [TestMethod, TestCategory("Unauthenticated")]
        public async Task ExposeResizableArtistImages()
        {
            const string katyPerryId = "music.97e60200-0200-11db-89ca-0019b92a3933";

            // Lookup Katy Perry's information
            ContentResponse lookupResponse = await Client.LookupAsync(katyPerryId, country: "US").Log();
            Artist artist = lookupResponse.Artists.Items.First();

            // Get a 1920x1080 image URL
            string squareImageUrl = artist.GetImageUrl(1920, 1080);
            Console.WriteLine($"1920x1080 image URL: {squareImageUrl}");

            // Get the default image URL
            string defaultImageUrl = artist.ImageUrl;
            Console.WriteLine($"Default image URL: {defaultImageUrl}");
        }
    }
}
