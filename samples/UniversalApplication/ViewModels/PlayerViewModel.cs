// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.ViewModels
{
    using System.ComponentModel;
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

        private readonly IGrooveClient _grooveClient;
        private readonly GrooveApiErrorViewModel _errorViewModel;

        public PlayerViewModel(IGrooveClient grooveClient, GrooveApiErrorViewModel errorViewModel)
        {
            _grooveClient = grooveClient;
            _errorViewModel = errorViewModel;
        }

        public async Task StreamAsync(string trackId)
        {
            StreamResponse streamResponse = await _grooveClient.StreamAsync(
                trackId,
                StreamClientInstanceId.GetStableClientInstanceId());

            await _errorViewModel.HandleGrooveApiErrorAsync(streamResponse.Error);

            if (!string.IsNullOrEmpty(streamResponse.Url))
            {
                StreamUrl = streamResponse.Url;
            }
        }

        public async Task PreviewAsync(string trackId)
        {
            StreamResponse previewResponse = await _grooveClient.PreviewAsync(
                trackId,
                StreamClientInstanceId.GetStableClientInstanceId());

            await _errorViewModel.HandleGrooveApiErrorAsync(previewResponse.Error);

            if (!string.IsNullOrEmpty(previewResponse.Url))
            {
                StreamUrl = previewResponse.Url;
            }
        }
    }
}
