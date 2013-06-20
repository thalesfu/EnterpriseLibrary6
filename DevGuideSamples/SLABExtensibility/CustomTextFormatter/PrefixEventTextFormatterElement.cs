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

using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Etw.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;

namespace CustomTextFormatter
{
  public class PrefixEventTextFormatterElement : IFormatterElement
  {
    private readonly XName formatterName = XName.Get("prefixEventTextFormatter", "urn:sample.etw.customformatter");

    public bool CanCreateFormatter(System.Xml.Linq.XElement element)
    {
      return this.GetFormatterElement(element) != null;
    }

    public IEventTextFormatter CreateFormatter(System.Xml.Linq.XElement element)
    {
      var formatter = this.GetFormatterElement(element);

      var header = (string)formatter.Attribute("header");
      var footer = (string)formatter.Attribute("footer");
      var prefix = (string)formatter.Attribute("prefix");
      var datetimeFormat = (string)formatter.Attribute("dateTimeFormat");

      return new PrefixEventTextFormatter(
        header,footer, prefix, datetimeFormat);
    }

    private XElement GetFormatterElement(XElement element)
    {
      return element.Element(this.formatterName);
    }
  }
}
