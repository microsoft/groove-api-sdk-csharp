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
    using DataContract;

    public class MusicContentPaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Track> Tracks { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }

        public MusicContentPaneViewModel()
        {
            Tracks = new ObservableCollection<Track>();
            Albums = new ObservableCollection<Album>();
            Artists = new ObservableCollection<Artist>();
            Playlists = new ObservableCollection<Playlist>();
        }

        public void DisplayMusicContent(ContentResponse contentResponse)
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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
