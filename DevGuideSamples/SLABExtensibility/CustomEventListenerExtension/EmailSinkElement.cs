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

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Etw.Configuration;
using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System.Xml.Linq;

namespace CustomSinkExtension
{
  /// <summary>
  /// Event sink class for sending events through the configured email host to one or many recipients.
  /// </summary>
  public class EmailSinkElement : ISinkElement
  {
    private readonly XName sinkName = XName.Get("emailSink", "urn:sample.etw.emailsink");

    public bool CanCreateSink(XElement element)
    {
      return element.Name == this.sinkName;
    }

    public IObserver<EventEntry> CreateSink(XElement element)
    {
      var host = (string)element.Attribute("host");
      var port = (int)element.Attribute("port");
      var recipients = (string)element.Attribute("recipients");
      var subject = (string)element.Attribute("subject");
      var credentials = (string)element.Attribute("credentials");

      var formatter = FormatterElementFactory.Get(element);

      var sink = new EmailSink(host, port, recipients, subject, credentials, formatter);

      return sink;
    }
  }
}
