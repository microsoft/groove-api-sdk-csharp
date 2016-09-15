// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataContract;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GettingStarted : TestBase
    {
        [TestMethod, TestCategory("Unauthenticated"), TestCategory("GettingStarted")]
        public async Task HelpYouGetStarted()
        {
            // Start by registering an Azure Data Market client ID and secret (see http://music.microsoft.com/developer)

            // Create a client
            IGrooveClient client = GrooveClientFactory.CreateGrooveClient(AzureDataMarketClientId, AzureDataMarketClientSecret);

            // Use null to get your current geography.
            // Specify a 2 letter country code (such as "US" or "DE") to force a specific country.
            string country = null;

            // Search for albums in your current geography
            ContentResponse searchResponse = await client.SearchAsync(
                MediaNamespace.music, 
                "Foo Fighters", 
                filter: SearchFilter.Albums, 
                maxItems: 5, 
                country: country);

            Console.WriteLine($"Found {searchResponse.Albums.TotalItemCount} albums");
            foreach (Album albumResult in searchResponse.Albums.Items)
            {
                Console.WriteLine(albumResult.Name);
            }

            // List tracks in the first album
            Album album = searchResponse.Albums.Items[0];
            ContentResponse lookupResponse = await client.LookupAsync(
                album.Id, 
                extras: ExtraDetails.Tracks, 
                country: country);

            // Display information about the album
            album = lookupResponse.Albums.Items[0];
            Console.WriteLine($"Album: {album.Name} (link: {album.GetLink(ContentExtensions.LinkAction.Play)}, " +
                              $"image: {album.GetImageUrl(800, 800)})");

            foreach (Contributor contributor in album.Artists)
            {
                Artist artist = contributor.Artist;
                Console.WriteLine($"Artist: {artist.Name} (link: {artist.GetLink()}, image: {artist.GetImageUrl(1920, 1080)})");
            }

            foreach (Track track in album.Tracks.Items)
            {
                Console.WriteLine($"Track: {track.TrackNumber} - {track.Name}");
            }
        }
    }
}
