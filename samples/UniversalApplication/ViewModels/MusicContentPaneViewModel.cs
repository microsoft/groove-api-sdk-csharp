// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Client;
    using DataContract;

    public class MusicContentPaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Track> Tracks { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly IGrooveClient _grooveClient;
        private readonly GrooveApiErrorViewModel _errorViewModel;

        public MusicContentPaneViewModel(IGrooveClient grooveClient, GrooveApiErrorViewModel errorViewModel)
        {
            _grooveClient = grooveClient;
            _errorViewModel = errorViewModel;

            Tracks = new ObservableCollection<Track>();
            Albums = new ObservableCollection<Album>();
            Artists = new ObservableCollection<Artist>();
            Playlists = new ObservableCollection<Playlist>();
        }

        public async Task GetPlaylistsAsync()
        {
            ContentResponse playlists = await _grooveClient.BrowseAsync(
                    MediaNamespace.music,
                    ContentSource.Collection,
                    ItemType.Playlists);

            await _errorViewModel.HandleGrooveApiErrorAsync(playlists.Error);
            DisplayMusicContent(playlists);
        }

        public async Task SearchCatalogAsync(string searchQuery, string country)
        {
            ContentResponse searchResponse = await _grooveClient.SearchAsync(
                MediaNamespace.music,
                searchQuery,
                ContentSource.Catalog,
                maxItems: 10,
                country: country);

            await _errorViewModel.HandleGrooveApiErrorAsync(searchResponse.Error);
            DisplayMusicContent(searchResponse);
        }

        private void DisplayMusicContent(ContentResponse contentResponse)
        {
            ResetMusicContent();

            AddMusicContent(contentResponse?.Tracks?.Items, Tracks);
            AddMusicContent(contentResponse?.Albums?.Items, Albums);
            AddMusicContent(contentResponse?.Artists?.Items, Artists);
            AddMusicContent(contentResponse?.Playlists?.Items, Playlists);
        }

        private void AddMusicContent<T>(IReadOnlyCollection<T> contentList, ObservableCollection<T> addToDisplayList)
            where T : Content
        {
            if (contentList != null)
            {
                foreach (T content in contentList)
                {
                    addToDisplayList.Add(content);
                }
            }
        }

        public void ResetMusicContent()
        {
            Tracks.Clear();
            Albums.Clear();
            Artists.Clear();
            Playlists.Clear();
        }
    }
}
