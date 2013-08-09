// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

namespace NotificationsExtensions
{
    using System;
    using System.Net;

    public static class Extensions
    {
        /// <summary>
        /// Gets the Notification Status code as <see cref="NotificationStatus"/> enumeration.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate enumeration value.</returns>
        public static NotificationStatus GetNotificationStatus(this HttpWebResponse response)
        {
            return response.GetStatus(NotificationStatus.NotApplicable, WnsHeaders.NotificationStatus);
        }

        /// <summary>
        /// Gets the Device Connection Status code as <see cref="DeviceConnectionStatus"/> enumeration.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate enumeration value.</returns>
        public static DeviceConnectionStatus GetDeviceConnectionStatus(this HttpWebResponse response)
        {
            return response.GetStatus(DeviceConnectionStatus.NotApplicable, WnsHeaders.DeviceConnectionStatus);
        }

        /// <summary>
        /// Gets the Debug Trace code as string.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate debug trace value.</returns>
        public static string GetDebugTrace(this HttpWebResponse response)
        {
            return response.GetHeaderValue(string.Empty, WnsHeaders.DebugTrace);
        }

        /// <summary>
        /// Gets the error description as string.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate error description.</returns>
        public static string GetErrorDescription(this HttpWebResponse response)
        {
            return response.GetHeaderValue(string.Empty, WnsHeaders.ErrorDescription);
        }

        /// <summary>
        /// Gets the Message ID from the response.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate Message ID.</returns>
        public static string GetMessageID(this HttpWebResponse response)
        {
            return response.GetHeaderValue(string.Empty, WnsHeaders.MsgID);
        }

        private static string GetHeaderValue(this HttpWebResponse response, string def, string header)
        {
            return response.Headers[header] ?? def;
        }

        private static T GetStatus<T>(this HttpWebResponse response, T def, string header) where T : struct
        {
            T status;
            string statusString = response.Headers[header];

            return !Enum.TryParse(statusString, true, out status) ? def : status;
        }
    }
}