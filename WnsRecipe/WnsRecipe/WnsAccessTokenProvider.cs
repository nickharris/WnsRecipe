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
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Web;

    [DataContract]
    public class WnsAccessTokenProvider : IAccessTokenProvider, IDisposable
    {
        private const string PayloadFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}";
        private const string UrlEncoded = "application/x-www-form-urlencoded";
        private const string AccessTokenUrl = "https://login.live.com/accesstoken.srf";
        private const string AccessScope = "notify.windows.com";

        [DataMember]
        private string cachedToken;

        [DataMember]
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        public WnsAccessTokenProvider(string clientId, string clientSecret)
        {
            this.ClientID = clientId;
            this.ClientSecret = clientSecret;
        }

        ~WnsAccessTokenProvider()
        {
            Dispose(false);
        }

        [DataMember]
        public string ClientID { get; set; }

        [DataMember]
        public string ClientSecret { get; set; }

        public string GetAccessToken(bool cache)
        {
            string result = null;

            if (cache)
            {
                this.readerWriterLock.EnterReadLock();
                try
                {
                    if (!string.IsNullOrEmpty(this.cachedToken))
                    {
                        result = this.cachedToken;
                    }
                }
                finally
                {
                    this.readerWriterLock.ExitReadLock();
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = GetAccessToken();
                if (cache)
                {
                    this.readerWriterLock.EnterWriteLock();
                    try
                    {
                        this.cachedToken = result;
                    }
                    finally
                    {
                        this.readerWriterLock.ExitWriteLock();
                    }
                }
            }

            return result;
        }

        public void ResetCachedToken()
        {
            this.readerWriterLock.EnterWriteLock();
            try
            {
                this.cachedToken = null;
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.readerWriterLock.Dispose();
            }
        }

        private static OAuthToken GetOAuthToken(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return null;
                }

                var ser = new DataContractJsonSerializer(typeof(OAuthToken));
                var authToken = (OAuthToken)ser.ReadObject(stream);
                return authToken;
            }
        }

        private static string AccessTokenRequestStatus(string accessTokenUrl, string payload, string urlEncoded, int payloadLength, string clientID, string clientSecret)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Access Token Url:              {0}\r\n", accessTokenUrl);
            sb.AppendFormat("Payload:                       {0}\r\n", payload);
            sb.AppendFormat("Payload Length:                {0}\r\n", payloadLength);
            sb.AppendFormat("UrlEncoded:                    {0}\r\n", urlEncoded);
            sb.AppendFormat("Client ID:                     {0}\r\n", clientID);
            sb.AppendFormat("Client Secret:                 {0}\r\n", clientSecret);
            sb.AppendLine();
            sb.AppendLine(payload);

            return sb.ToString();
        }

        private static string AccessTokenLogMessage(string message, OAuthToken authToken, string payload, HttpWebRequest request, HttpWebResponse response)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine();

            if (request != null)
            {
                sb.AppendLine("- REQUEST ------------------------------------------------------------------");
                HttpHeaders(sb, request.Headers);
                sb.AppendLine();
            }

            sb.AppendLine(payload);

            if (response != null)
            {
                sb.AppendLine();
                sb.AppendLine("- RESPONSE -----------------------------------------------------------------");
                HttpHeaders(sb, response.Headers);

                if (response.ContentLength > 0)
                {
                    sb.AppendLine("- CONTENT -----------------------------------------------------------------");
                    sb.AppendLine();
                    sb.AppendFormat("Access Token: {0}", authToken.AccessToken);
                    sb.AppendFormat("Token Type: {0}", authToken.TokenType);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static void HttpHeaders(StringBuilder sb, WebHeaderCollection headers)
        {
            foreach (var header in headers.AllKeys)
            {
                sb.AppendFormat("{0,-30}: {1}", header, headers[header]);
                sb.AppendLine();
            }
        }

        private string GetAccessToken()
        {
            string payload = string.Format(PayloadFormat, HttpUtility.UrlEncode(this.ClientID), HttpUtility.UrlEncode(this.ClientSecret), AccessScope);

            var request = (HttpWebRequest)WebRequest.Create(AccessTokenUrl);
            request.Method = Constants.Post;
            request.ContentType = UrlEncoded;

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            request.ContentLength = payload.Length;

            // trace TraceVerbose
            WnsDiagnostics.TraceVerbose(
                       DiagnosticsConstants.SendingWNSAccessTokenRequestID,
                       "Sending Access Token request.\r\n\r\n{0}",
                       AccessTokenRequestStatus(AccessTokenUrl.ToString(), payload, UrlEncoded, payload.Length, this.ClientID, this.ClientSecret));

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(payloadBytes, 0, payloadBytes.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            OAuthToken authToken = GetOAuthToken(response);

            WnsDiagnostics.TraceInformation(DiagnosticsConstants.AccessTokenRecivedID, AccessTokenLogMessage("Access Token recived.", authToken, payload, request, response));
            
            return authToken.AccessToken;
        }
    }
}