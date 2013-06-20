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

using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

using CustomSinkExtension;

namespace InProcConsoleApp
{
  class Program
  {
    static void Main(string[] args)
    {
      ObservableEventListener listener = new ObservableEventListener();
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      listener.LogToConsole();

      // Modify these settings to match your SMTP service requirements.
      listener.LogToEmail("smtp.live.com", 587, "bill@adatum.com", "In Proc Sample", "etw");

      MyCompanyEventSource.Log.Failure("No response from servers, general network failure!!");

      listener.DisableEvents(MyCompanyEventSource.Log);
      listener.Dispose();
    }
  }
}
