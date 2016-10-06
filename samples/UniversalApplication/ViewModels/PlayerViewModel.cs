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
    }
}
