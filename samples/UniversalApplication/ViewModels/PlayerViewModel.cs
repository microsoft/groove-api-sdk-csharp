// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Client;
    using DataContract;
    using Helpers;

    public class PlayerViewModel : INotifyPropertyChanged
    {
        private string _streamUrl;

        public string StreamUrl
        {
            get {return _streamUrl;}
            set
            {
                _streamUrl = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private const string StreamRight = "stream";

        private readonly IGrooveClient _grooveClient;
        private readonly GrooveApiErrorViewModel _errorViewModel;

        public PlayerViewModel(IGrooveClient grooveClient, GrooveApiErrorViewModel errorViewModel)
        {
            _grooveClient = grooveClient;
            _errorViewModel = errorViewModel;
        }

        public async Task PlayTrackAsync(Track track, bool userIsSignedIn, bool userHasSubscription)
        {
            bool trackCanBeStreamed = track.Rights != null 
                && track.Rights.Any(right => right.Equals(StreamRight, StringComparison.OrdinalIgnoreCase));

            StreamResponse streamResponse = trackCanBeStreamed && userIsSignedIn && userHasSubscription
                ? await _grooveClient.StreamAsync(track.Id, StreamClientInstanceId.GetStableClientInstanceId())
                : await _grooveClient.PreviewAsync(track.Id, StreamClientInstanceId.GetStableClientInstanceId());

            await _errorViewModel.HandleGrooveApiErrorAsync(streamResponse.Error);

            if (!string.IsNullOrEmpty(streamResponse.Url))
            {
                StreamUrl = streamResponse.Url;
            }
        }
    }
}
