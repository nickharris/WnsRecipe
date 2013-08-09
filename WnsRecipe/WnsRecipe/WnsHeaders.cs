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
    internal static class WnsHeaders
    {
        // Request
        public const string Type = "X-WNS-Type";
        public const string Authroization = "Authorization";
        public const string CachePolicy = "X-WNS-Cache-Policy";
        public const string Priority = "X-WNS-Priority";
        public const string RequestForStatus = "X-WNS-RequestForStatus";
        public const string Tag = "X-WNS-Tag";
        public const string TTL = "X-WNS-TTL";

        // Response
        public const string DebugTrace = "X-WNS-Debug-Trace";
        public const string DeviceConnectionStatus = "X-WNS-DeviceConnectionStatus";
        public const string ErrorDescription = "X-WNS-Error-Description";
        public const string MsgID = "X-WNS-Msg-ID";
        public const string NotificationStatus = "X-WNS-NotificationStatus";
        public const string WWWAuthenticate = "WWW-Authenticate";
    }
}