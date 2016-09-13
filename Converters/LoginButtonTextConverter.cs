// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.Converters
{
    using System;
    using Windows.UI.Xaml.Data;

    public class LoginButtonTextConverter : IValueConverter
    {
        private const string LogInText = "Log in";
        private const string LogOutText = "Log out";

        // This converts the boolean indicating if the user is logged-in to a message to display on the login button
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                return (bool) value ? LogOutText : LogInText;
            }
            
            return LogInText;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
