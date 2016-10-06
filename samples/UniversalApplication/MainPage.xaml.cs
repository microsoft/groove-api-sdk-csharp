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
    using System.Threading.Tasks;
    using Windows.System;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Popups;
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

        /// <summary>
        /// The ClientInstanceId is used for streaming. You'll need to send it with every stream request.
        /// The purpose of this ID is to identify a specific instance of your application. 
        /// That means it should be generated once when the application is installed and persisted afterwards.
        /// It needs to has at least 32 characters and at most 128.
        /// It's used service-side to prevent users from streaming concurrently on multiple devices.
        /// </summary>
        private const string ClientInstanceId = "GrooveApiUniversalSamples42000000";

        private readonly UserAccountManagerWithNotifications _userAccountManager;
        private readonly IGrooveClient _grooveClient;

        public string SearchQuery { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public MusicContentPaneViewModel MusicContentPaneViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _userAccountManager = new UserAccountManagerWithNotifications();
            _grooveClient = GrooveClientFactory.CreateGrooveClient(AzureDataMarketClientId, AzureDataMarketClientSecret, _userAccountManager);

            MusicContentPaneViewModel = new MusicContentPaneViewModel();
            PlayerViewModel = new PlayerViewModel();
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
                ContentResponse playlists = await _grooveClient.BrowseAsync(
                    MediaNamespace.music,
                    ContentSource.Collection,
                    ItemType.Playlists);

                await HandleGrooveApiErrorAsync(playlists.Error);
                MusicContentPaneViewModel.DisplayMusicContent(playlists);
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

            ContentResponse searchResponse = await _grooveClient.SearchAsync(
                MediaNamespace.music,
                SearchQuery,
                ContentSource.Catalog,
                maxItems: 10);

            await HandleGrooveApiErrorAsync(searchResponse.Error);
            MusicContentPaneViewModel.DisplayMusicContent(searchResponse);

            ((Button)sender).IsEnabled = true;
        }

        private async void PlayButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SymbolIcon expectedSender = sender as SymbolIcon;
            Track selectedTrack = expectedSender?.DataContext as Track;
            if (selectedTrack != null)
            {
                if (_userAccountManager.UserIsSignedIn)
                {
                    StreamResponse streamResponse = await _grooveClient.StreamAsync(
                        selectedTrack.Id,
                        ClientInstanceId);

                    await HandleGrooveApiErrorAsync(streamResponse.Error);

                    if (!string.IsNullOrEmpty(streamResponse.Url))
                    {
                        PlayerViewModel.StreamUrl = streamResponse.Url;
                    }
                }
                else
                {
                    StreamResponse previewResponse = await _grooveClient.PreviewAsync(
                        selectedTrack.Id,
                        ClientInstanceId);

                    await HandleGrooveApiErrorAsync(previewResponse.Error);

                    if (!string.IsNullOrEmpty(previewResponse.Url))
                    {
                        PlayerViewModel.StreamUrl = previewResponse.Url;
                    }
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

        private async Task HandleGrooveApiErrorAsync(Error error)
        {
            if (error == null)
            {
                Debug.WriteLine("Successful Groove API call");
            }
            else
            {
                Debug.WriteLine($"Groove API error: {error.ErrorCode}");
                Debug.WriteLine($"Groove API error message: {error.Message}");
                Debug.WriteLine($"Groove API error description: {error.Description}");

                MessageDialog errorPopup = new MessageDialog(
                    $"{error.ErrorCode} : {error.Message}. {error.Description}", 
                    "Groove API error");
                await errorPopup.ShowAsync();
            }
        }
    }
}
