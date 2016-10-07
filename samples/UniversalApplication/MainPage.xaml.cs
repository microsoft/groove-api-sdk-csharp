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
        // See https://github.com/Microsoft/Groove-API-documentation/blob/master/Main/Using%20the%20Groove%20RESTful%20Services/Obtaining%20a%20Developer%20Access%20Token.md
        private const string AzureDataMarketClientId = "";
        private const string AzureDataMarketClientSecret = "";

        private readonly WindowsUniversalUserAccountManager _userAccountManager;
        private readonly IGrooveClient _grooveClient;

        public string SearchQuery { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public MusicContentPaneViewModel MusicContentPaneViewModel { get; set; }
        public UserProfileViewModel UserProfileViewModel { get; set; }
        public GrooveApiErrorViewModel ErrorViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _userAccountManager = new WindowsUniversalUserAccountManager();
            _grooveClient = GrooveClientFactory.CreateGrooveClient(AzureDataMarketClientId, AzureDataMarketClientSecret, _userAccountManager);

            ErrorViewModel = new GrooveApiErrorViewModel();
            MusicContentPaneViewModel = new MusicContentPaneViewModel(_grooveClient, ErrorViewModel);
            PlayerViewModel = new PlayerViewModel(_grooveClient, ErrorViewModel);
            UserProfileViewModel = new UserProfileViewModel(_userAccountManager, _grooveClient, ErrorViewModel);
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
                if (_userAccountManager.UserIsSignedIn && UserProfileViewModel.UserHasGrooveSubscription)
                {
                    await PlayerViewModel.StreamAsync(selectedTrack.Id);
                }
                else
                {
                    await PlayerViewModel.PreviewAsync(selectedTrack.Id);
                }
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
