// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.Converters
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class SignInButtonTextConverter : IValueConverter
    {
        private const string SignInText = "Sign in";
        private const string SignOutText = "Sign out";

        // This converts the boolean indicating if the user is signed-in to a message to display on the sign-in button
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                return (bool) value ? SignOutText : SignInText;
            }
            
            return SignInText;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
