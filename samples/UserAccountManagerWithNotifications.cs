// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Windows.Security.Authentication.Web.Core;
    using Windows.Security.Credentials;
    using Windows.Storage;
    using Windows.UI.ApplicationSettings;
    using Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class UserAccountManagerWithNotifications : INotifyPropertyChanged, IUserTokenManager
    {
        // To obtain Microsoft account tokens, you must register your application online 
        // (see https://github.com/Microsoft/Groove-API-documentation/blob/master/Main/Using%20the%20Groove%20RESTful%20Services/User%20Authentication.md).
        // Then, you must associate the app with the store.
        // Simply right-click on the project in Visual Studio, choose Store -> Associate App with the Store... and follow the steps.
        private const string MicrosoftAccountProviderId = "https://login.microsoft.com";

        /// <summary>
        /// Consumers means we're interested in MSA user accounts (and not enterprise Azure AD accounts).
        /// </summary>
        private const string ConsumerAuthority = "consumers";

        /// <summary>
        /// Scope to use to obtain a token for the Groove API.
        /// </summary>
        public const string GrooveApiScope = "MicrosoftMediaServices.GrooveApiAccess";

        /// <summary>
        /// Scope to use to obtain a token for the profile API.
        /// </summary>
        public const string ProfileScope = "wl.basic";

        private const string CurrentUserKey = "CurrentUserId";
        private const string CurrentUserProviderKey = "CurrentUserProviderId";

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

        // TODO: rename to UserIsSignedIn
        private bool _userIsLoggedIn;

        public bool UserIsLoggedIn
        {
            get { return _userIsLoggedIn; }
            set
            {
                _userIsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// This method customizes the accounts settings pane to setup user authentication with Microsoft accounts.
        /// You need to register this method to the AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested event.
        /// Don't forget to also remove the event handler (for example when user navigates away from your page).
        /// </summary>
        public async void BuildAccountPaneAsync(
            AccountsSettingsPane s,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            
            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                MicrosoftAccountProviderId,
                ConsumerAuthority);

            var command = new WebAccountProviderCommand(msaProvider, SignInUserAccountAsync);
            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
        }

        /// <summary>
        /// You shouldn't call this method directly. It's registered to the Account Pane, so it will be used as 
        /// a callback when you call AccountsSettingsPane.Show();
        /// </summary>
        private async void SignInUserAccountAsync(WebAccountProviderCommand command)
        {
            Stopwatch timer = Stopwatch.StartNew();

            try
            {
                // We want to require all scopes right from the beginning
                // This way the user gives consent only once
                WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, $"{ProfileScope} {GrooveApiScope}");
                WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);

                if (result.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    Debug.WriteLine("Successful sign-in");

                    WebAccount account = result.ResponseData[0].WebAccount;

                    ApplicationData.Current.LocalSettings.Values[CurrentUserProviderKey] = account.WebAccountProvider.Id;
                    ApplicationData.Current.LocalSettings.Values[CurrentUserKey] = account.Id;

                    UserIsLoggedIn = true;

                    timer.Stop();

                    string userName = await GetUserNameAsync(result.ResponseData[0].Token);
                    UserName = userName;
                    return;
                }

                HandleTokenError(result);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Sign-in failure:");
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                timer.Stop();
                Debug.WriteLine($"Sign-in took {timer.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>
        /// Tries to sign-in the user silently. It will fail if user action is required.
        /// In that case you'll need to fallback to using AccountsSettingsPane.Show();
        /// </summary>
        /// <returns>True if sign-in is successful.</returns>
        public async Task<bool> SignInUserAccountSilentlyAsync()
        {
            UserIsLoggedIn = false;
            UserName = string.Empty;

            WebAccount savedAccount = await GetCurrentAccountAsync();

            if (savedAccount?.WebAccountProvider == null)
            {
                return false;
            }

            string userToken = await GetUserTokenAsync(savedAccount, ProfileScope, true);
            string userName = await GetUserNameAsync(userToken);

            if (userName != null)
            {
                UserIsLoggedIn = true;
                UserName = userName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Signs the current user out and clears his account information.
        /// </summary>
        public async Task SignOutAccountAsync()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(CurrentUserKey))
            {
                WebAccountProvider providertoDelete = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                    MicrosoftAccountProviderId,
                    ConsumerAuthority);
                WebAccount accountToDelete = await WebAuthenticationCoreManager.FindAccountAsync(
                    providertoDelete,
                    (string)ApplicationData.Current.LocalSettings.Values[CurrentUserKey]);

                if (accountToDelete != null)
                {
                    await accountToDelete.SignOutAsync();
                }

                ApplicationData.Current.LocalSettings.Values.Remove(CurrentUserKey);
                ApplicationData.Current.LocalSettings.Values.Remove(CurrentUserProviderKey);

                UserIsLoggedIn = false;
                UserName = string.Empty;

                Debug.WriteLine("Successfully logged out");
            }
        }

        /// <summary>
        /// Gets the currently saved user account.
        /// </summary>
        private async Task<WebAccount> GetCurrentAccountAsync()
        {
            string providerId = ApplicationData.Current.LocalSettings.Values[CurrentUserProviderKey]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values[CurrentUserKey]?.ToString();

            if (providerId == null || accountId == null)
            {
                return null;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);
            return account;
        }

        /// <summary>
        /// Get a user token for the currently logged-in user.
        /// </summary>
        /// <param name="scope">Scope for the requested token.</param>
        /// <param name="silentlyOnly">If true, will only try to acquire a token silently. If false, will prompt the user if action is necessary.</param>
        /// <returns>Null if token couldn't be acquired.</returns>
        public async Task<string> GetUserTokenAsync(string scope, bool silentlyOnly)
        {
            WebAccount currentAccount = await GetCurrentAccountAsync();
            if (currentAccount?.WebAccountProvider == null)
            {
                return null;
            }

            return await GetUserTokenAsync(currentAccount, scope, silentlyOnly);
        }

        /// <summary>
        /// Gets a user token for a given account.
        /// </summary>
        private async Task<string> GetUserTokenAsync(WebAccount account, string scope, bool silentlyOnly)
        {
            Stopwatch timer = Stopwatch.StartNew();

            try
            {
                WebTokenRequest request = new WebTokenRequest(account.WebAccountProvider, scope);
                WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);

                if (result.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    Debug.WriteLine("Successfully got user token silently");
                    return result.ResponseData[0].Token;
                }

                HandleTokenError(result);

                if (!silentlyOnly)
                {
                    // If we couldn't get the token silently, RequestTokenAsync will prompt the user for necessary action
                    result = await WebAuthenticationCoreManager.RequestTokenAsync(request, account);

                    if (result.ResponseStatus == WebTokenRequestStatus.Success)
                    {
                        Debug.WriteLine("Successfully got user token with action");
                        return result.ResponseData[0].Token;
                    }

                    HandleTokenError(result);
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return null;
            }
            finally
            {
                timer.Stop();
                Debug.WriteLine($"Getting user token took {timer.ElapsedMilliseconds}ms");
            }
        }

        public async Task<string> GetUserAuthorizationHeaderAsync(bool forceRefresh)
        {
            if (forceRefresh)
            {
                await PurgeTokenCache();
            }

            string userToken = await GetUserTokenAsync(GrooveApiScope, !forceRefresh);
            return $"Bearer {userToken}";
        }

        private void HandleTokenError(WebTokenRequestResult result)
        {
            if (result.ResponseStatus == WebTokenRequestStatus.ProviderError)
            {
                Debug.WriteLine("You most likely haven't configured the application association with the store. " +
                                "Right-click on the project in Visual Studio, then follow the steps in " +
                                "Store -> Associate App with the store...");
            }

            Debug.WriteLine("Error from the provider: " +
                            $"ResponseStatus={result.ResponseStatus.ToString("G")}, " +
                            $"ErrorCode={result.ResponseError?.ErrorCode}, " +
                            $"ErrorMessage={result.ResponseError?.ErrorMessage}");
        }

        /// <summary>
        /// Purges token cache. Can be useful if user revoked consent (on https://account.live.com/consent/Manage) 
        /// and cached tickets get rejected by the Groove API.
        /// </summary>
        public async Task PurgeTokenCache()
        {
            WebAccount currentAccount = await GetCurrentAccountAsync();
            if (currentAccount != null)
            {
                foreach (string scope in new[] {ProfileScope, GrooveApiScope, $"{ProfileScope} {GrooveApiScope}"})
                {
                    WebTokenRequest request = new WebTokenRequest(currentAccount.WebAccountProvider, scope);
                    WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, currentAccount);

                    await result.InvalidateCacheAsync();
                }
            }
        }

        /// <summary>
        /// Gets the name of the currently logged-in user.
        /// </summary>
        /// <returns>Null if no user is logged in or user profile couldn't be fetched.</returns>
        public async Task<string> GetUserNameAsync()
        {
            string userToken = await GetUserTokenAsync(ProfileScope, false);
            return await GetUserNameAsync(userToken);
        }

        /// <summary>
        /// Gets the name of the currently logged-in user.
        /// </summary>
        /// <param name="userToken">User token with the correct scopes.</param>
        /// <returns>Null if user profile couldn't be fetched.</returns>
        public async Task<string> GetUserNameAsync(string userToken)
        {
            if (userToken == null)
            {
                return null;
            }

            Uri restApi = new Uri(@"https://apis.live.net/v5.0/me?access_token=" + userToken);

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(restApi))
            {
                string content = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(content);
                string name = jsonObject["name"].Value<string>();
                return name;
            }
        }
    }
}
