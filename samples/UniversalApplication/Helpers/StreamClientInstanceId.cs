// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  
//  All Rights Reserved.
//  Licensed under the MIT License.
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Groove.Api.Samples.Helpers
{
    using System;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.ApplicationModel;
    using Windows.System.Profile;

    public static class StreamClientInstanceId
    {
        /// <summary>
        /// Compute a stable application-specific client instance id string for use as "clientInstanceId" parameters in IGrooveClient
        /// </summary>
        /// <returns>A valid clientInstanceId string. This string is specific to the current machine, user and application.</returns>
        public static string GetStableClientInstanceId()
        {
            // Generate a somewhat stable application instance id
            HardwareToken ashwid = HardwareIdentification.GetPackageSpecificToken(null);

            byte[] id = ashwid.Id.ToArray();
            string idstring = Package.Current.Id.Name + ":";

            for (int i = 0; i < id.Length; i += 4)
            {
                short what = BitConverter.ToInt16(id, i);
                short value = BitConverter.ToInt16(id, i + 2);
                // Only include stable components in the id
                // http://msdn.microsoft.com/en-us/library/windows/apps/jj553431.aspx
                const int cpuId = 1;
                const int memorySize = 2;
                const int diskSerial = 3;
                const int bios = 9;
                if (what == cpuId || what == memorySize || what == diskSerial || what == bios)
                {
                    idstring += value.ToString("X4");
                }
            }

            string machineClientInstanceId = idstring.PadRight(32, 'X');
            return machineClientInstanceId;
        }
    }
}
