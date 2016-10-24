// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.Converters
{
    using System;
    using Windows.Media.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class StreamUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string streamUrl = value as string;
            if (streamUrl != null)
            {
                return MediaSource.CreateFromUri(new Uri(streamUrl));
            }

            return null;
        }

        // No need to implement converting back on a one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
