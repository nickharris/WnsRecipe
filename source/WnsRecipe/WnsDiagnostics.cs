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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class WnsDiagnostics
    {
        private const string DefaultTraceSourceName = "WNSRecipe";
        private const SourceLevels DefaultTraceLevel = SourceLevels.Error;

        private static TraceSource trace;

        public static TraceSource TraceSource
        {
            get
            {
                if (WnsDiagnostics.trace == null)
                {
                    WnsDiagnostics.trace = new TraceSource(DefaultTraceSourceName, SourceLevels.Off);
                }

                return WnsDiagnostics.trace;
            }
        }

        public static void Enable(string traceSourceName = DefaultTraceSourceName)
        {
            WnsDiagnostics.trace = new TraceSource(traceSourceName, DefaultTraceLevel);
        }

        public static void Disable()
        {
            WnsDiagnostics.trace.Switch.Level = SourceLevels.Off;
        }

        internal static void TraceVerbose(int id, string format, params object[] args)
        {
            if (WnsDiagnostics.trace != null)
            {
                WnsDiagnostics.trace.TraceEvent(TraceEventType.Verbose, id, format, args);
            }
        }

        internal static void TraceInformation(int id, string format, params object[] args)
        {
            if (WnsDiagnostics.trace != null)
            {
                WnsDiagnostics.trace.TraceEvent(TraceEventType.Information, id, format, args);
            }
        }

        internal static void TraceWarning(int id, string format, params object[] args)
        {
            if (WnsDiagnostics.trace != null)
            {
                WnsDiagnostics.trace.TraceEvent(TraceEventType.Warning, id, format, args);
            }
        }

        internal static void TraceError(int id, string format, params object[] args)
        {
            if (WnsDiagnostics.trace != null)
            {
                WnsDiagnostics.trace.TraceEvent(TraceEventType.Error, id, format, args);
            }
        }

        internal static void TraceException(int id, Exception exception, string format, params object[] args)
        {
            if (WnsDiagnostics.trace == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, args);
            sb.AppendLine();
            
            sb.AppendLine("Stack trace:");
            sb.Append(exception.StackTrace);
            sb.AppendLine();

            if (exception is AggregateException)
            {
                ((AggregateException)exception).Flatten().Handle((innerException) =>
                {
                    while (innerException != null)
                    {
                        sb.AppendLine();
                        sb.Append("* ");
                        sb.AppendLine(innerException.Message);
                        sb.AppendLine(innerException.StackTrace);
                        innerException = innerException.InnerException;
                    }

                    return true;
                });
            }

            trace.TraceEvent(TraceEventType.Error, id, sb.ToString());
        }
    }
}
