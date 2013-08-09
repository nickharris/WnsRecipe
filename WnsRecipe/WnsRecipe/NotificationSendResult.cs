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
    using System.Text;

    /// <summary>
    /// Push notification message send operation result.
    /// </summary>
    public class NotificationSendResult
    {
        private const string StatusFormat = "Status Code {0} - {1}: {2}";

        internal NotificationSendResult(Uri channelUri, string errorDescription, HttpStatusCode statusCode)
        {
            this.Timestamp = DateTimeOffset.Now;
            this.ChannelUri = channelUri;

            this.InitializeStatusCodes(null);

            this.StatusCode = statusCode;
            this.ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Initializes a new instance of the NotificationSendResult class.
        /// </summary>
        internal NotificationSendResult(Uri channelUri, HttpWebResponse response)
        {
            this.Timestamp = DateTimeOffset.Now;
            this.ChannelUri = channelUri;

            this.InitializeStatusCodes(response);
        }

        /// <summary>
        /// Initializes a new instance of the NotificationSendResult class.
        /// </summary>
        internal NotificationSendResult(Uri channelUri, WebException exception)
            : this(channelUri, exception.Response as HttpWebResponse)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the NotificationSendResult class.
        /// </summary>
        internal NotificationSendResult(Uri channelUri, Exception exception)
            : this(channelUri, response: null)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the original exception or null.
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// Gets the response time offset.
        /// </summary>
        public DateTimeOffset Timestamp { get; protected set; }

        /// <summary>
        /// Gets the channel URI.
        /// </summary>
        public Uri ChannelUri { get; protected set; }

        /// <summary>
        /// Gets the web request status.
        /// </summary>
        public HttpStatusCode StatusCode { get; protected set; }

        /// <summary>
        /// Gets the push notification status.
        /// </summary>
        public NotificationStatus NotificationStatus { get; protected set; }

        /// <summary>
        /// Gets the device connection status.
        /// </summary>
        public DeviceConnectionStatus DeviceConnectionStatus { get; protected set; }

        /// <summary>
        /// Gets the error description.
        /// </summary>
        public string ErrorDescription { get; protected set; }

        /// <summary>
        /// Gets the debug trace.
        /// </summary>
        public string DebugTrace { get; protected set; }

        /// <summary>
        /// Gets the message id.
        /// </summary>
        public string MessageId { get; protected set; }

        public string LookupHttpStatusCode()
        {
            string result;

            if (Exception == null)
            {
                switch (this.StatusCode)
                {
                    case HttpStatusCode.OK:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Notification was accepted by WNS");
                        break;
                    case HttpStatusCode.BadRequest:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, string.Empty);
                        break;
                    case HttpStatusCode.Unauthorized:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Appropriate authentication token not supplied to WNS");
                        break;
                    case HttpStatusCode.Forbidden:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "The WNS is not authorized to send a notification to this URI even though they are authenticated.");
                        break;
                    case HttpStatusCode.NotFound:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Channel URL is not valid or recognized by WNS");
                        break;
                    case HttpStatusCode.MethodNotAllowed:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Invalid method (GET, DELETE, CREATE) only POST is allowed");
                        break;
                    case HttpStatusCode.NotAcceptable:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Throttle limit exceeded");
                        break;
                    case HttpStatusCode.Gone:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Channel URL has expired");
                        break;
                    case HttpStatusCode.RequestEntityTooLarge:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "Notification payload exceeds 5k limit");
                        break;
                    case HttpStatusCode.InternalServerError:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "An internal failure caused notification delivery to fail");
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, "The server is currently unavailable");
                        break;
                    default:
                        result = string.Format(StatusFormat, this.StatusCode, (int)this.StatusCode, string.Empty);
                        break;
                }
            }
            else
            {
                result = Exception.Message;
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Channel URI:              {0}\r\n", this.ChannelUri.AbsoluteUri);
            sb.AppendFormat("Notification status:      {0}\r\n", this.NotificationStatus);
            sb.AppendFormat("Status code:              {0}\r\n", this.StatusCode);
            sb.AppendFormat("Device connection status: {0}\r\n", this.DeviceConnectionStatus);
            sb.AppendFormat("Error description:        {0}\r\n", this.ErrorDescription);
            sb.AppendFormat("Debug Trace:              {0}\r\n", this.DebugTrace);
            sb.AppendFormat("MessageId:                {0}\r\n", this.MessageId);
            sb.AppendFormat("Timestamp:                {0}\r\n", this.Timestamp);

            return sb.ToString();
        }

        protected virtual void InitializeStatusCodes(HttpWebResponse response)
        {
            if (response == null)
            {
                this.StatusCode = HttpStatusCode.InternalServerError;
                this.NotificationStatus = NotificationStatus.NotApplicable;
                this.DeviceConnectionStatus = DeviceConnectionStatus.NotApplicable;
                this.ErrorDescription = null;
                this.DebugTrace = null;
                this.MessageId = null;
            }
            else
            {
                this.StatusCode = response.StatusCode;
                this.NotificationStatus = response.GetNotificationStatus();
                this.DeviceConnectionStatus = response.GetDeviceConnectionStatus();
                this.ErrorDescription = response.GetErrorDescription();
                this.DebugTrace = response.GetDebugTrace();
                this.MessageId = response.GetMessageID();
            }
        }
    }
}