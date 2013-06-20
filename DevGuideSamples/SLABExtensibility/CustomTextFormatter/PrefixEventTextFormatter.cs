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

using System.IO;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;

namespace CustomTextFormatter
{
  public class PrefixEventTextFormatter : IEventTextFormatter
  {
    public PrefixEventTextFormatter(string header, string footer,
      string prefix, string dateTimeFormat)
    {
      this.Header = header;
      this.Footer = footer;
      this.Prefix = prefix;
      this.DateTimeFormat = dateTimeFormat;
    }

    public string Header { get; set; }

    public string Footer { get; set; }

    public string Prefix { get; set; }

    public string DateTimeFormat { get; set; }

    public void WriteEvent(EventEntry eventEntry, TextWriter writer)
    {
      // Write header
      if (!string.IsNullOrWhiteSpace(this.Header))
        writer.WriteLine(this.Header);

      // Write properties
      writer.WriteLine("{0}SourceId : {1}",
        this.Prefix, eventEntry.ProviderId);
      writer.WriteLine("{0}EventId : {1}",
        this.Prefix, eventEntry.EventId);
      writer.WriteLine("{0}Keywords : {1}",
        this.Prefix, eventEntry.Schema.Keywords);
      writer.WriteLine("{0}Level : {1}",
        this.Prefix, eventEntry.Schema.Level);
      writer.WriteLine("{0}Message : {1}",
        this.Prefix, eventEntry.FormattedMessage);
      writer.WriteLine("{0}Opcode : {1}",
        this.Prefix, eventEntry.Schema.Opcode);
      writer.WriteLine("{0}Task : {1} {2}",
        this.Prefix, eventEntry.Schema.Task, eventEntry.Schema.TaskName);
      writer.WriteLine("{0}Version : {1}",
        this.Prefix, eventEntry.Schema.Version);
      writer.WriteLine("{0}Payload :{1}",
        this.Prefix, FormatPayload(eventEntry));
      writer.WriteLine("{0}Timestamp : {1}",
        this.Prefix, eventEntry.GetFormattedTimestamp(this.DateTimeFormat));


      // Write footer
      if (!string.IsNullOrWhiteSpace(this.Footer))
        writer.WriteLine(this.Footer);

      writer.WriteLine();
    }

    private static string FormatPayload(EventEntry entry)
    {
      var eventSchema = entry.Schema;
      var sb = new StringBuilder();
      for (int i = 0; i < entry.Payload.Count; i++)
      {
        // Any errors will be handled in the event sink.
        sb.AppendFormat(" [{0} : {1}]",
          eventSchema.Payload[i], entry.Payload[i]);
      }
      return sb.ToString();
    }
  }
}
