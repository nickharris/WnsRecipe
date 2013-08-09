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
    using System.Text;

    public static class NotificationSendUtils
    {
        private const int MaxSendRetries = 3;

        public static NotificationSendResult Send(Uri channelUri, IAccessTokenProvider accessTokenProvider, string payload, NotificationType type, NotificationSendOptions options)
        {
            return Send(channelUri, accessTokenProvider, payload, type, options.SecondsTTL, options.Cache, options.RequestForStatus, options.Tag, options.Priority);
        }

        public static NotificationSendResult Send(Uri channelUri, IAccessTokenProvider accessTokenProvider, string payload, NotificationType type, int? secondsTTL = null, bool? cache = null, bool? requestForStatus = null, string tag = null, NotificationPriority priority = NotificationPriority.Normal, int tokenRetry = 0)
        {
            NotificationSendResult result;
            byte[] payloadBytes;
            HttpWebRequest request = null;

            try
            {
                WnsDiagnostics.TraceVerbose(
                       DiagnosticsConstants.SendingWNSNotificationID,
                       "Sending WNS notification.\r\n\r\n{0}",
                       NotificationRequestStatus(channelUri.ToString(), payload, secondsTTL, cache, requestForStatus, tag, priority, tokenRetry));

                var accessToken = string.Empty;

                try
                {
                    accessToken = accessTokenProvider.GetAccessToken(true);
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {
                        var accessTokenError = GetAccessTokenError(e.Response);
                        WnsDiagnostics.TraceError(DiagnosticsConstants.WnsAccessTokenSendResultErrorID, AccessTokenLogMessage(e.Response, accessTokenError));
                        return new WnsAccessTokenSendResult(channelUri, e, accessTokenError);
                    }
                    else
                    {
                        WnsDiagnostics.TraceError(DiagnosticsConstants.ServiceUnavailableErrorID, e.Message);
                        return new NotificationSendResult(channelUri, e.Message, HttpStatusCode.ServiceUnavailable);
                    }
                }

                request = CreateWebRequest(channelUri, accessToken, payload, type, secondsTTL, cache, requestForStatus, tag, priority, out payloadBytes);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(payloadBytes, 0, payloadBytes.Length);
                }               

                var response = (HttpWebResponse)request.GetResponse();
                result = new NotificationSendResult(channelUri, response);

                WnsDiagnostics.TraceInformation(DiagnosticsConstants.SentWNSNotificationID, NotificationLogMessage("Sent WNS notification", result.NotificationStatus.ToString(), result, payload, request, response));
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string exceptionDetails = e.Response.Headers[WnsHeaders.WWWAuthenticate];
                    if (!string.IsNullOrWhiteSpace(exceptionDetails) && exceptionDetails.Contains("Token expired"))
                    {
                        accessTokenProvider.ResetCachedToken();

                        tokenRetry++;
                        if (tokenRetry <= MaxSendRetries)
                        {
                            result = Send(channelUri, accessTokenProvider, payload, type, secondsTTL, cache, requestForStatus, tag, priority, tokenRetry);
                        }
                        else
                        {
                            result = new NotificationSendResult(channelUri, e);
                        }
                    }
                    else
                    {
                        result = new NotificationSendResult(channelUri, e);
                    }

                    WnsDiagnostics.TraceError(DiagnosticsConstants.WNSNotificationFailureID, NotificationLogMessage("WNS notification failure", e.Message, result, payload, request, e.Response));
                }
                else
                {
                    WnsDiagnostics.TraceError(DiagnosticsConstants.ServiceUnavailableErrorID, e.Message);
                    return new NotificationSendResult(channelUri, e.Message, HttpStatusCode.ServiceUnavailable);
                }
            }
            catch (Exception e)
            {
                result = new NotificationSendResult(channelUri, e);

                WnsDiagnostics.TraceException(DiagnosticsConstants.WNSNotificationFailureID, e, "WNS notification failure: {0}\r\n\r\n{1}", e.Message, result);
            }

            return result;
        }

        public static void SendAsynchronously(Uri channelUri, IAccessTokenProvider accessTokenProvider, string payload, Action<NotificationSendResult> sent, Action<NotificationSendResult> error, NotificationType type, NotificationSendOptions options)
        {
            SendAsynchronously(channelUri, accessTokenProvider, payload, sent, error, type, options.SecondsTTL, options.Cache, options.RequestForStatus, options.Tag, options.Priority);
        }

        public static void SendAsynchronously(Uri channelUri, IAccessTokenProvider accessTokenProvider, string payload, Action<NotificationSendResult> sent, Action<NotificationSendResult> error, NotificationType type, int? secondsTTL = null, bool? cache = null, bool? requestForStatus = null, string tag = null, NotificationPriority priority = NotificationPriority.Normal, int tokenRetry = 0)
        {
            byte[] payloadBytes;

            try
            {
                WnsDiagnostics.TraceVerbose(
                    DiagnosticsConstants.SendingWNSNotificationID,
                    "Sending WNS notification.\r\n\r\n{0}",
                    NotificationRequestStatus(channelUri.ToString(), payload, secondsTTL, cache, requestForStatus, tag, priority, tokenRetry));

                var accessToken = string.Empty;

                try
                {
                    accessToken = accessTokenProvider.GetAccessToken(true);
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {
                        var accessTokenError = GetAccessTokenError(e.Response);
                        WnsDiagnostics.TraceError(DiagnosticsConstants.WnsAccessTokenSendResultErrorID, AccessTokenLogMessage(e.Response, accessTokenError));
                        error(new WnsAccessTokenSendResult(channelUri, e, accessTokenError));
                    }
                    else
                    {
                        WnsDiagnostics.TraceError(DiagnosticsConstants.ServiceUnavailableErrorID, e.Message);
                        error(new NotificationSendResult(channelUri, e.Message, HttpStatusCode.ServiceUnavailable));
                    }
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var request = CreateWebRequest(channelUri, accessToken, payload, type, secondsTTL, cache, requestForStatus, tag, priority, out payloadBytes);

                    // Get the request stream asynchronously.
                    request.BeginGetRequestStream(
                        requestAsyncResult =>
                        {
                            try
                            {
                                using (Stream requestStream = request.EndGetRequestStream(requestAsyncResult))
                                {
                                    // Start writing the payload to the stream.
                                    requestStream.Write(payloadBytes, 0, payloadBytes.Length);
                                }

                                // Switch to receiving the response from WNS asynchronously.
                                request.BeginGetResponse(
                                    responseAsyncResult =>
                                    {
                                        try
                                        {
                                            using (
                                                var response =
                                                    (HttpWebResponse)request.EndGetResponse(responseAsyncResult))
                                            {
                                                var result = new NotificationSendResult(channelUri, response);
                                                if (result.StatusCode == HttpStatusCode.OK)
                                                {
                                                    WnsDiagnostics.TraceInformation(DiagnosticsConstants.SentWNSNotificationID, NotificationLogMessage("Sent WNS notification", result.NotificationStatus.ToString(), result, payload, request, response));
                                                    sent(result);
                                                }
                                                else
                                                {
                                                    error(result);
                                                    WnsDiagnostics.TraceError(DiagnosticsConstants.WNSNotificationFailureID, "WNS notification failure:\r\n\r\n{0}", result);
                                                }
                                            }
                                        }
                                        catch (WebException e)
                                        {
                                            if (e.Response != null)
                                            {
                                                var result = new NotificationSendResult(channelUri, e);

                                                string exceptionDetails = e.Response.Headers[WnsHeaders.WWWAuthenticate];
                                                if (!string.IsNullOrWhiteSpace(exceptionDetails) &&
                                                    exceptionDetails.Contains("Token expired"))
                                                {
                                                    accessTokenProvider.ResetCachedToken();

                                                    tokenRetry++;
                                                    if (tokenRetry <= MaxSendRetries)
                                                    {
                                                        SendAsynchronously(channelUri, accessTokenProvider, payload, sent, error, type, secondsTTL, cache, requestForStatus, tag, priority, tokenRetry);
                                                    }
                                                    else
                                                    {
                                                        WnsDiagnostics.TraceError(DiagnosticsConstants.WNSNotificationFailureID, NotificationLogMessage("WNS notification failure", e.Message, result, payload, request, e.Response));
                                                        error(result);
                                                    }
                                                }
                                                else
                                                {
                                                    WnsDiagnostics.TraceError(DiagnosticsConstants.WNSNotificationFailureID, NotificationLogMessage("WNS notification failure", e.Message, result, payload, request, e.Response));
                                                    error(result);
                                                }
                                            }
                                            else
                                            {
                                                WnsDiagnostics.TraceError(DiagnosticsConstants.ServiceUnavailableErrorID, e.Message);
                                                error(new NotificationSendResult(channelUri, e.Message, HttpStatusCode.ServiceUnavailable));
                                            }
                                        }
                                        catch (Exception ex3)
                                        {
                                            var result = new NotificationSendResult(channelUri, ex3);
                                            WnsDiagnostics.TraceException(DiagnosticsConstants.GeneralNotificationFailureID, ex3, "WNS notification failure: {0}\r\n\r\n{1}", ex3.Message, result);
                                            error(result);
                                        }
                                    },
                                    null);
                            }
                            catch (Exception ex2)
                            {
                                var result = new NotificationSendResult(channelUri, ex2);
                                WnsDiagnostics.TraceException(DiagnosticsConstants.GeneralNotificationFailureID, ex2, "WNS notification failure: {0}\r\n\r\n{1}", ex2.Message, result);
                                error(result);
                            }
                        },
                        null);
                }
            }
            catch (Exception ex1)
            {
                var result = new NotificationSendResult(channelUri, ex1);
                WnsDiagnostics.TraceException(DiagnosticsConstants.GeneralNotificationFailureID, ex1, "WNS notification failure: {0}\r\n\r\n{1}", ex1.Message, result);
                error(result);
            }
        }

        private static void SetCustomHeaders(HttpWebRequest request, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Badge:
                    request.Headers.Add(WnsHeaders.Type, "wns/badge");
                    request.ContentType = "text/xml; charset=utf-8";
                    break;
                case NotificationType.Tile:
                    request.Headers.Add(WnsHeaders.Type, "wns/tile");
                    request.ContentType = "text/xml; charset=utf-8";
                    break;
                case NotificationType.Toast:
                    request.Headers.Add(WnsHeaders.Type, "wns/toast");
                    request.ContentType = "text/xml; charset=utf-8";
                    break;
                case NotificationType.Raw:
                    request.Headers.Add(WnsHeaders.Type, "wns/raw");
                    request.ContentType = "application/octet-stream";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void SetHeaders(string accessToken, NotificationType type, int? secondsTTL, bool? cache, bool? requestForStatus, string tag, NotificationPriority priority, HttpWebRequest request)
        {
            SetCustomHeaders(request, type);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", accessToken));           

            if (priority != NotificationPriority.Normal)
            {
                request.Headers.Add(WnsHeaders.Priority, ((int)priority).ToString());
            }

            // Cache policy
            if (cache.HasValue)
            {
                request.Headers.Add(WnsHeaders.CachePolicy, cache.Value ? Constants.Cache : Constants.NoCache);
            }

            // Request for Device Status and Notification Status to be returned in the response
            if (requestForStatus.HasValue)
            {
                request.Headers.Add(WnsHeaders.RequestForStatus, requestForStatus.Value.ToString().ToLower());
            }

            // Assign a tag label for a notification - used by device to detect dups - note 16chars in length
            if (!string.IsNullOrWhiteSpace(tag))
            {
                request.Headers.Add(WnsHeaders.Tag, tag);
            }

            // if null default behaviour is to not expire
            if (secondsTTL.HasValue)
            {
                request.Headers.Add(WnsHeaders.TTL, secondsTTL.Value.ToString());
            }
        }

        private static HttpWebRequest CreateWebRequest(Uri channelId, string accessToken, string payload, NotificationType type, int? secondsTTL, bool? cache, bool? requestForStatus, string tag, NotificationPriority priority, out byte[] payloadBytes)
        {
            
            var request = (HttpWebRequest) HttpWebRequest.Create(channelId);
            request.Method = Constants.Post;
            
            request.ServicePoint.Expect100Continue = false;

            SetHeaders(accessToken, type, secondsTTL, cache, requestForStatus, tag, priority, request);

            payloadBytes = Encoding.UTF8.GetBytes(payload);
            request.ContentLength = payloadBytes.Length;
            
            return request;
        }

        private static string NotificationRequestStatus(string channelId, string payload, int? secondsTTL, bool? cache, bool? requestForStatus, string tag, NotificationPriority priority, int tokenRetry)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Channel URI:              {0}\r\n", channelId);
            sb.AppendFormat("TTL:                      {0}\r\n", secondsTTL.HasValue ? secondsTTL.Value.ToString() : "-");
            sb.AppendFormat("Cache:                    {0}\r\n", cache.HasValue && cache.Value ? "yes" : "no");
            sb.AppendFormat("Request for status:       {0}\r\n", requestForStatus.HasValue && requestForStatus.Value ? "yes" : "no");
            sb.AppendFormat("Tag:                      {0}\r\n", tag);
            sb.AppendFormat("Priority:                 {0}\r\n", priority);
            sb.AppendFormat("Token retry count:        {0}\r\n", tokenRetry);
            sb.AppendLine();
            sb.AppendLine(payload);

            return sb.ToString();
        }

        private static string NotificationLogMessage(string message, string description, NotificationSendResult result, string payload, WebRequest request, WebResponse response)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}: {1}", message, description);
            sb.AppendLine();
            sb.AppendLine(result.ToString());
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
                    var responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        var responseBody = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            sb.AppendLine();
                            sb.AppendLine(responseBody);
                        }
                    }
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

        private static string AccessTokenLogMessage(WebResponse webResponse, AccessTokenError error)
        {
            var sb = new StringBuilder();

            if (error != null)
            {
                sb.AppendFormat("Error: {0}.", error.Error);
                sb.AppendFormat("Error Description: {0}.", error.ErrorDescription);
                sb.AppendLine();
            }

            sb.AppendLine("- Headers ------------------------------------------------------------------");
            HttpHeaders(sb, webResponse.Headers);
            sb.AppendLine();

            return sb.ToString();
        }

        private static AccessTokenError GetAccessTokenError(WebResponse response)
        {
            if (response.ContentLength > 0)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    var ser = new DataContractJsonSerializer(typeof(AccessTokenError));
                    var tokenError = (AccessTokenError)ser.ReadObject(stream);

                    return tokenError;
                }
            }

            return null;
        }
    }
}
