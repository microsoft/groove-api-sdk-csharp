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
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Windows.Media.Core;
    using Windows.Media.Streaming.Adaptive;

    using Client;
    using DataContract;
    using Helpers;

    public class PlayerViewModel : INotifyPropertyChanged
    {
        private MediaSource _mediaSource;

        public MediaSource MediaSource
        {
            get { return _mediaSource; }
            set
            {
                _mediaSource = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                MediaSource = await GetMediaSource(new Uri(streamResponse.Url), streamResponse.ContentType);
            }
        }

        private async Task<MediaSource> GetMediaSource(Uri streamUri, string contentType)
        {
            // Using AdaptiveMediaSource for HLS since MediaSource can fails to stream HLS on some versions of Windows.
            // Root cause: User-Agent header mismatch between HLS manifest & fragment request
            if (contentType == "application/vnd.apple.mpegurl")
            {
                AdaptiveMediaSourceCreationResult adaptiveMediaSourceCreation =
                    await AdaptiveMediaSource.CreateFromUriAsync(streamUri);

                if (adaptiveMediaSourceCreation.Status != AdaptiveMediaSourceCreationStatus.Success)
                {
                    Debug.WriteLine($"Error creating the AdaptiveMediaSource: {adaptiveMediaSourceCreation.Status}");

                    return null;
                }

                return MediaSource.CreateFromAdaptiveMediaSource(adaptiveMediaSourceCreation.MediaSource);
            }

            return MediaSource.CreateFromUri(streamUri);
        }
    }
}
