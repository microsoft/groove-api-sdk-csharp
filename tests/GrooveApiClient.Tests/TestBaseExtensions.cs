// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Client.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataContract;

    /// <summary>
    /// Extension methods used to add output when executing tests
    /// </summary>
    public static class TestBaseExtensions
    {
        public static IEnumerable<Content> GetAllTopLevelContent(this ContentResponse response)
        {
            return response
                .GetAllContentLists()
                .Where(c => c?.ReadOnlyItems != null)
                .SelectMany(x => x.ReadOnlyItems);
        }

        public static IEnumerable<IPaginatedList<Content>> GetAllContentLists(this ContentResponse response)
        {
            yield return response.Artists;
            yield return response.Albums;
            yield return response.Tracks;
            yield return response.Playlists;
        }

        private static async Task<TResponse> LogResponse<TResponse>(
            this Task<TResponse> responseTask,
            Action<TResponse> log)
            where TResponse : BaseResponse
        {
            TResponse response = await responseTask;

            if (response == null)
            {
                Console.WriteLine("Response is null");
            }
            else
            {
                log(response);

                if (response.Error != null)
                {
                    Console.WriteLine($"  Error: {response.Error.ErrorCode} {response.Error.Description}");
                    Console.WriteLine($"         {response.Error.Message}");
                }
            }

            return response;
        }

        /// <summary>
        /// Log top level content items and sub tracks for debug purposes
        /// </summary>
        public static Task<ContentResponse> Log(this Task<ContentResponse> responseTask)
        {
            return LogResponse(responseTask, response =>
            {
                Console.WriteLine("Response top level content:");

                foreach (Content content in response.GetAllTopLevelContent())
                {
                    Playlist playlist = content as Playlist;
                    Artist artist = content as Artist;
                    Album album = content as Album;
                    Track track = content as Track;

                    if (album != null)
                    {
                        Console.WriteLine($"  {content.GetType().Name} {content.Id}: {content.Name}, " +
                                          $"{string.Join(" and ", album.Artists.Select(contributor => contributor.Artist.Name))}");
                    }
                    else if (track != null)
                    {
                        Console.WriteLine($"  {content.GetType().Name} {content.Id}: {content.Name}, {track.Album.Name}, " +
                                          $"{string.Join(" and ", track.Artists.Select(contributor => contributor.Artist.Name))}");
                    }
                    else
                    {
                        Console.WriteLine($"  {content.GetType().Name} {content.Id}: {content.Name}");
                    }

                    IPaginatedList<Track> tracks = playlist != null
                        ? playlist.Tracks
                        : album != null
                            ? album.Tracks
                            : artist?.TopTracks;

                    if (tracks?.ReadOnlyItems != null)
                    {
                        Console.WriteLine("  Contained tracks:");

                        foreach (Track subTrack in tracks.ReadOnlyItems)
                        {
                            Console.WriteLine($"    {subTrack.GetType().Name} {subTrack.Id}: " +
                                              $"{subTrack.Name}, {subTrack.Album?.Name}, " +
                                              $"{string.Join(" and ", subTrack.Artists.Select(contributor => contributor.Artist.Name))}");
                        }
                    }
                }

                if (response.CatalogGenres != null)
                {
                    foreach (Genre genre in response.CatalogGenres)
                    {
                        Console.WriteLine($" Genre: {genre.Name}");
                    }
                }
            });
        }

        public static Task<StreamResponse> Log(this Task<StreamResponse> responseTask)
        {
            return LogResponse(responseTask, response =>
            {
                Console.WriteLine("Stream response:");
                Console.WriteLine($"  URL: {response.Url}");
                Console.WriteLine($"  Content type: {response.ContentType}");
                Console.WriteLine($"  Expiration date: {response.ExpiresOn}");
            });
        }

        public static Task<UserProfileResponse> Log(this Task<UserProfileResponse> responseTask)
        {
            return LogResponse(responseTask, response =>
            {
                Console.WriteLine("User profile response:");
                Console.WriteLine($"  Has subscription: {response.HasSubscription}");
                Console.WriteLine($"  Culture: {response.Culture}");
                Console.WriteLine($"  Is subscription available for purchase: {response.IsSubscriptionAvailableForPurchase}");
            });
        }
    }
}
