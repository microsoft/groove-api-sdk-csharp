// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.UI.Popups;
    using DataContract;

    public class GrooveApiErrorViewModel
    {
        public async Task HandleGrooveApiErrorAsync(Error error)
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
