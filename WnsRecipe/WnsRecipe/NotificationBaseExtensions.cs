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
    using NotificationsExtensions.BadgeContent;
    using NotificationsExtensions.RawContent;
    using NotificationsExtensions.TileContent;
    using NotificationsExtensions.ToastContent;

    public static class NotificationBaseExtensions
    {
        public static NotificationSendResult Send(this INotificationContent notification, Uri channelUri, IAccessTokenProvider accessTokenProvider)
        {
            var payload = notification.GetContent();
            var type = notification.GetNotificationType();

            return NotificationSendUtils.Send(channelUri, accessTokenProvider, payload, type);
        }

        public static NotificationSendResult Send(this INotificationContent notification, Uri channelUri, IAccessTokenProvider accessTokenProvider, NotificationSendOptions options)
        {
            var payload = notification.GetContent();
            var type = notification.GetNotificationType();

            return NotificationSendUtils.Send(channelUri, accessTokenProvider, payload, type, options);
        }

        public static void SendAsynchronously(this INotificationContent notification, Uri channelUri, IAccessTokenProvider accessTokenProvider, Action<NotificationSendResult> sent, Action<NotificationSendResult> error)
        {
            var payload = notification.GetContent();
            var type = notification.GetNotificationType();

            NotificationSendUtils.SendAsynchronously(channelUri, accessTokenProvider, payload, sent, error, type);
        }

        public static void SendAsynchronously(this INotificationContent notification, Uri channelUri, IAccessTokenProvider accessTokenProvider, Action<NotificationSendResult> sent, Action<NotificationSendResult> error, NotificationSendOptions options)
        {
            var payload = notification.GetContent();
            var type = notification.GetNotificationType();

            NotificationSendUtils.SendAsynchronously(channelUri, accessTokenProvider, payload, sent, error, type, options);
        }

        private static NotificationType GetNotificationType(this INotificationContent notification)
        {
            if (notification is IToastNotificationContent)
            {
                return NotificationType.Toast;
            }
            else if (notification is ITileNotificationContent)
            {
                return NotificationType.Tile;
            }
            else if (notification is IBadgeNotificationContent)
            {
                return NotificationType.Badge;
            }
            else if (notification is IRawNotificationContent)
            {
                return NotificationType.Raw;
            }

            throw new ArgumentException("Notification type is not valid", "notification");
        }
    }
}
