// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.DataContract
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.Xmlns)]
    public class Genre : ContentCategory
    {
        [DataMember(EmitDefaultValue = false)]
        public string ParentName { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public bool HasEditorialPlaylists { get; set; }
    }
}
