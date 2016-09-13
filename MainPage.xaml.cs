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
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Client;
    using DataContract;

    public sealed partial class MainPage : Page
    {
        // Provide your own values here
        // See https://github.com/Microsoft/Groove-API-documentation/blob/master/Main/Using%20the%20Groove%20RESTful%20Services/Obtaining%20a%20Developer%20Access%20Token.md
        private const string AzureDataMarketClientId = "";
        private const string AzureDataMarketClientSecret = "";

        private readonly UserAccountManagerWithNotifications _userAccountManager;
        private readonly IGrooveClient _grooveClient;

        public MainPage()
        {
            InitializeComponent();

            _userAccountManager = new UserAccountManagerWithNotifications();
            _grooveClient = GrooveClientFactory.CreateGrooveClient(AzureDataMarketClientId, AzureDataMarketClientSecret);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += _userAccountManager.BuildAccountPaneAsync;
            await _userAccountManager.LoginUserAccountSilentlyAsync();
            if (_userAccountManager.UserIsLoggedIn)
            {
                Debug.WriteLine("Successful silent login");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= _userAccountManager.BuildAccountPaneAsync;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsLoggedIn)
            {
                await _userAccountManager.LogOutAccountAsync();
            }
            else
            {
                AccountsSettingsPane.Show();
            }
            
            ((Button)sender).IsEnabled = true;
        }

        private async void LookupButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            ContentResponse lookupResponse = await _grooveClient.LookupAsync(
                "music.B13EB907-0100-11DB-89CA-0019B92A3933", 
                ContentSource.Catalog);

            await HandleGrooveApiErrorAsync(lookupResponse.Error);

            ((Button)sender).IsEnabled = true;
        }

        private async void StreamButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            if (_userAccountManager.UserIsLoggedIn)
            {
                string userToken = await _userAccountManager.GetUserTokenAsync(
                    UserAccountManagerWithNotifications.GrooveApiScope, 
                    false);
                StreamResponse streamResponse = await _grooveClient.StreamAsync(
                    "music.AA3EB907-0100-11DB-89CA-0019B92A3933", 
                    Guid.NewGuid().ToString(), 
                    userToken);

                await HandleGrooveApiErrorAsync(streamResponse.Error);
            }
            else
            {
                AccountsSettingsPane.Show();
            }

            ((Button)sender).IsEnabled = true;
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
                if (error.ErrorCode == ErrorCode.INVALID_AUTHORIZATION_HEADER.ToString("G"))
                {
                    Debug.WriteLine("Purging token cache");
                    await _userAccountManager.PurgeTokenCache();
                }
            }
        }
    }
}
