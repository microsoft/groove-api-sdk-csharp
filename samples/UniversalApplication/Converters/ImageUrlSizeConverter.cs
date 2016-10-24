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
    using Windows.UI.Xaml.Media.Imaging;

    public class ImageUrlSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string imageUrl = value as string;
            int size = int.Parse((string) parameter);
            if (imageUrl != null)
            {
                return new BitmapImage(new Uri($"{imageUrl}&w={size}&h={size}"));
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
