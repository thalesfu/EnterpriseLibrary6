//===============================================================================
// Microsoft patterns & practices
// Enterprise Library 6 Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

namespace CustomSinkExtension
{
  public static class EmailSinkExtensions
  {
    public static SinkSubscription<EmailSink> LogToEmail(this IObservable<EventEntry> eventStream, string host, int port, string recipients, string subject, string credentials, IEventTextFormatter formatter = null)
    {
      var sink = new EmailSink(host, port, recipients, subject, credentials, formatter);

      var subscription = eventStream.Subscribe(sink);

      return new SinkSubscription<EmailSink>(subscription, sink);
    }
  }
}
