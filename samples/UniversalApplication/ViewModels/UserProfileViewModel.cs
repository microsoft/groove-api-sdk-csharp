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
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Client;
    using DataContract;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private bool _userIsSignedIn;
        public bool UserIsSignedIn
        {
            get { return _userIsSignedIn; }
            set
            {
                _userIsSignedIn = value;
                OnPropertyChanged();
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private bool _userHasGrooveSubscription;
        public bool UserHasGrooveSubscription
        {
            get { return _userHasGrooveSubscription; }
            set
            {
                _userHasGrooveSubscription = value;
                OnPropertyChanged();
            }
        }

        private string _userGrooveSubscriptionCountry;
        public string UserGrooveSubscriptionCountry
        {
            get { return _userGrooveSubscriptionCountry; }
            set
            {
                _userGrooveSubscriptionCountry = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly WindowsUniversalUserAccountManager _accountManager;
        private readonly IGrooveClient _grooveClient;
        private readonly GrooveApiErrorViewModel _errorViewModel;

        public UserProfileViewModel(
            WindowsUniversalUserAccountManager accountManager,
            IGrooveClient grooveClient,
            GrooveApiErrorViewModel errorViewModel)
        {
            _accountManager = accountManager;
            _grooveClient = grooveClient;
            _errorViewModel = errorViewModel;

            _accountManager.UserSignInChange += (manager, userIsSignedIn) => UserSignInUpdateAsync(userIsSignedIn);
        }

        private async void UserSignInUpdateAsync(bool userIsSignedIn)
        {
            UserIsSignedIn = userIsSignedIn;
            if (userIsSignedIn)
            {
                await UpdateUserNameAsync();
                await UpdateUserSubscriptionInformationAsync();
            }
            else
            {
                UserName = string.Empty;
                UserHasGrooveSubscription = false;
                UserGrooveSubscriptionCountry = string.Empty;
            }
        }

        /// <summary>
        /// Gets the name of the currently logged-in user and updates the <see cref="UserName"/> property.
        /// </summary>
        private async Task UpdateUserNameAsync()
        {
            string userToken = await _accountManager.GetUserTokenAsync(WindowsUniversalUserAccountManager.ProfileScope, true);
            Uri restApi = new Uri(@"https://apis.live.net/v5.0/me?access_token=" + userToken);

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(restApi))
            {
                string content = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(content);
                UserName = jsonObject["name"].Value<string>();
            }
        }

        /// <summary>
        /// Gets the Groove profile of the currently logged-in user and updates the <see cref="UserHasGrooveSubscription"/> 
        /// and <see cref="UserGrooveSubscriptionCountry"/> properties.
        /// </summary>
        private async Task UpdateUserSubscriptionInformationAsync()
        {
            UserProfileResponse profileResponse = await _grooveClient.GetUserProfileAsync(MediaNamespace.music);
            await _errorViewModel.HandleGrooveApiErrorAsync(profileResponse.Error);

            UserHasGrooveSubscription = profileResponse.HasSubscription ?? false;
            UserGrooveSubscriptionCountry = profileResponse.Subscription?.Region;
        }
    }
}
