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
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;

    public class WnsAccessTokenSendResult : NotificationSendResult
    {
        internal WnsAccessTokenSendResult(Uri channelUri, HttpWebResponse response)
            : base(channelUri, response)
        {
        }

        internal WnsAccessTokenSendResult(Uri channelUri, WebException exception, AccessTokenError error)
            : base(channelUri, exception)
        {
            this.AccessTokenError = error;
            this.ErrorDescription = string.Format("{0}: {1}.", this.AccessTokenError.Error, this.AccessTokenError.ErrorDescription);
        }

        internal WnsAccessTokenSendResult(Uri channelUri, Exception exception)
            : base(channelUri, exception)
        {
        }

        public AccessTokenError AccessTokenError { get; protected set; }

        protected override void InitializeStatusCodes(HttpWebResponse response)
        {
            this.StatusCode = (response == null) ? HttpStatusCode.InternalServerError : response.StatusCode;
            this.ErrorDescription = null;
            this.NotificationStatus = NotificationStatus.NotApplicable;
            this.DeviceConnectionStatus = DeviceConnectionStatus.NotApplicable;
            this.DebugTrace = null;
            this.MessageId = null;
        }
    }
}
