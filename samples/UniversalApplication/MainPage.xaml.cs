// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples
{
    using System;
    using System.Diagnostics;
    using Windows.System;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Navigation;
    using Client;
    using DataContract;
    using ViewModels;

    public sealed partial class MainPage : Page
    {
        // Provide your own values here
        // See https://review.docs.microsoft.com/en-us/groove/using-the-groove-restful-services/obtaining-a-developer-access-token
        // WARNING: this is used here as an example but authentication for Universal Windows App should be done only using User
        // auth (cf. WindowsUniversalUserAccountManager) since you should not to put your Application secret in a client app.
        private const string ApplicationClientId = "";
        private const string ApplicationClientSecret = "";

        private readonly WindowsUniversalUserAccountManager _userAccountManager;

        public string SearchQuery { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public MusicContentPaneViewModel MusicContentPaneViewModel { get; set; }
        public UserProfileViewModel UserProfileViewModel { get; set; }
        public GrooveApiErrorViewModel ErrorViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _userAccountManager = new WindowsUniversalUserAccountManager();
            IGrooveClient grooveClient = GrooveClientFactory.CreateGrooveClient(ApplicationClientId, ApplicationClientSecret, _userAccountManager);

            ErrorViewModel = new GrooveApiErrorViewModel();
            MusicContentPaneViewModel = new MusicContentPaneViewModel(grooveClient, ErrorViewModel);
            PlayerViewModel = new PlayerViewModel(grooveClient, ErrorViewModel);
            UserProfileViewModel = new UserProfileViewModel(_userAccountManager, grooveClient, ErrorViewModel);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += _userAccountManager.BuildAccountPaneAsync;
            await _userAccountManager.SignInUserAccountSilentlyAsync();
            if (_userAccountManager.UserIsSignedIn)
            {
                Debug.WriteLine("Successful silent sign-in");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= _userAccountManager.BuildAccountPaneAsync;
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsSignedIn)
            {
                await _userAccountManager.SignOutAccountAsync();
            }
            else
            {
                AccountsSettingsPane.Show();
            }

            ((Button)sender).IsEnabled = true;
        }

        private async void PlaylistsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsSignedIn)
            {
                await MusicContentPaneViewModel.GetPlaylistsAsync();
            }
            else
            {
                AccountsSettingsPane.Show();
            }

            ((Button)sender).IsEnabled = true;
        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            await MusicContentPaneViewModel.SearchCatalogAsync(
                SearchQuery,
                UserProfileViewModel.UserGrooveSubscriptionCountry);

            ((Button)sender).IsEnabled = true;
        }

        private async void PlayButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SymbolIcon expectedSender = sender as SymbolIcon;
            Track selectedTrack = expectedSender?.DataContext as Track;
            if (selectedTrack != null)
            {
                await PlayerViewModel.PlayTrackAsync(
                    selectedTrack,
                    _userAccountManager.UserIsSignedIn,
                    UserProfileViewModel.UserHasGrooveSubscription);
            }
        }

        private async void DeeplinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SymbolIcon expectedSender = sender as SymbolIcon;
            Track selectedTrack = expectedSender?.DataContext as Track;
            if (selectedTrack != null)
            {
                string deeplink = selectedTrack.Link;
                await Launcher.LaunchUriAsync(new Uri(deeplink));
            }
        }
    }
}
