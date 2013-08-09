﻿// ----------------------------------------------------------------------------------
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
    internal static class DiagnosticsConstants
    {
        public const int SendingWNSNotificationID = 1000;

        public const int SendingWNSAccessTokenRequestID = 2000;
        
        public const int SentWNSNotificationID = 100;

        public const int AccessTokenRecivedID = 200;

        public const int WNSNotificationFailureID = 1;

        public const int GeneralNotificationFailureID = 2;

        public const int WnsAccessTokenSendResultErrorID = 5;

        public const int ServiceUnavailableErrorID = 6;
    }
}
