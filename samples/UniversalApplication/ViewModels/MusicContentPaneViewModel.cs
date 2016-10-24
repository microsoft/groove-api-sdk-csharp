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
        private ObservableCollection<Track> _tracks;
        public ObservableCollection<Track> Tracks
        {
            get {return _tracks;}
            set
            {
                _tracks = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Album> _albums;
        public ObservableCollection<Album> Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Artist> _artists;
        public ObservableCollection<Artist> Artists
        {
            get { return _artists; }
            set
            {
                _artists = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Playlist> _playlists;
        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set
            {
                _playlists = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
