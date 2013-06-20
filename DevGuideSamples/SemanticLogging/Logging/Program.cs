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

using System;
using System.IO;
using System.Diagnostics.Tracing;
using System.ComponentModel;
using System.Data.Linq;

using DevGuideExample.MenuSystem;

// references to application block namespace(s) for these examples
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;


namespace SemanticLoggingExample
{
  class Program
  {
    static string connectionString = @"Data Source=(localdb)\v11.0; AttachDBFilename='|DataDirectory|\Database\SemanticLogging.mdf';Integrated Security=True";

    static void Main(string[] args)
    {
      #region Setup workspace
      Console.WriteLine(AppDomain.CurrentDomain.GetData("DataDirectory"));
      
      // Create a folder named Temp on drive C: for disk log files if it does not exist
      try
      {
        if (!Directory.Exists(@"C:\Temp"))
        {
          Directory.CreateDirectory(@"C:\Temp");
        }
      }
      catch
      {
        Console.WriteLine(@"WARNING: Folder C:\Temp cannot be created for disk log files");
        Console.WriteLine();
      }

      #endregion

      new MenuDrivenApplication("Semantic Logging Block Developer's Guide Examples",
          SimpleEventSource,
          SimpleEventSourceAlternativeApproach,
          SimpleEventSourceWithXMLFormatter,
          SimpleEventSourceWithColorMapper,
          MultipleEventListenersLevel,
          MultipleEventListenersKeywords,
          VerifyEventSource,
          UsingRxFiltering).Run();
    }

    [Description("Simple logging with an EventSource")]
    static void SimpleEventSource()
    {
      // Set up and enable the event listener - typically done when the application starts
      var listener = new ObservableEventListener();
      listener.LogToConsole();
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      // Log some messages
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.Failure("Couldn't connect to server.");
      Console.WriteLine("Written two log messages.\nUsing a basic console listener to capture them.");
      Console.WriteLine("The color is determined by the severity level.\n");
      
      // Disable the event listener - typically done when the application terminates
      listener.DisableEvents(MyCompanyEventSource.Log);
      listener.Dispose();
     }

    [Description("Simple logging with an EventSource #2")]
    static void SimpleEventSourceAlternativeApproach()
    {
      // Set up and enable the event listener - typically done when the application starts
      EventListener listener = ConsoleLog.CreateListener();
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      // Log some messages
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.Failure("Couldn't connect to server.");
      Console.WriteLine("Written two log messages.\nUsing a basic console listener to capture them.");
      Console.WriteLine("The color is determined by the severity level.\n");

      // Disable and dispose the event listener - typically done when the application terminates
      listener.DisableEvents(MyCompanyEventSource.Log);
      listener.Dispose();
    }

    [Description("Simple logging with an EventSource and an XML formatter")]
    static void SimpleEventSourceWithXMLFormatter()
    {
      // Set up and enable the event listener - typically done when the application starts
      var listener = new ObservableEventListener();
      listener.LogToConsole(new XmlEventTextFormatter(EventTextFormatting.Indented));
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      // Log some messages
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.Failure("Couldn't connect to server.");
      Console.WriteLine("Written two log messages.\nUsing a console listener with an XML formatter to capture them.");

      // Disable the event listener - typically done when the application terminates
      listener.DisableEvents(MyCompanyEventSource.Log);
      listener.Dispose();
    }

    [Description("Simple logging with an EventSource and a custom color mapper")]
    static void SimpleEventSourceWithColorMapper()
    {
      // Set up and enable the event listener - typically done when the application starts
      var listener = new ObservableEventListener();
      listener.LogToConsole(new JsonEventTextFormatter(EventTextFormatting.Indented), new MyCustomColorMapper());
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      // Log some messages
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.PageStop(99);
      Console.WriteLine("Written two log messages.\nUsing a console listener with a JSON formatter to capture them.");

      // Disable the event listener - typically done when the application terminates
      listener.DisableEvents(MyCompanyEventSource.Log);
      listener.Dispose();
    }

    [Description("Using multiple event listeners and filtering based on the level")]
    static void MultipleEventListenersLevel()
    {
      // Set up and enable the event listeners - typically done when the application starts
      var listener1 = new ObservableEventListener();
      var listener2 = new ObservableEventListener();
      listener1.EnableEvents(MyCompanyEventSource.Log, EventLevel.Error, Keywords.All);
      listener2.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);
      listener1.LogToConsole();

      var subscription = listener2.LogToFlatFile(@"C:\Temp\DemoSemanticLogging.log", new EventTextFormatter("===================", "==================="));

      // Log some messages
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.Failure("Couldn't connect to server.");
      Console.WriteLine("Written two log messages.\nUsing a basic console listener and a Flat File listener to capture them.");
      Console.WriteLine("Only the critical message appears in the console, both appear in: \nC:\\Temp\\DemoSemanticLogging.log.\n");
      
      // Flush the sink
      subscription.Sink.FlushAsync().Wait();

      // Disable the event listener - typically done when the application terminates
      listener1.DisableEvents(MyCompanyEventSource.Log);
      listener2.DisableEvents(MyCompanyEventSource.Log);

      listener1.Dispose();
      listener2.Dispose();
    }

    [Description("Using multiple event listeners and filtering based on keywords")]
    static void MultipleEventListenersKeywords()
    {
      var listener1 = new ObservableEventListener();
      var listener2 = new ObservableEventListener();
      listener1.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, MyCompanyEventSource.Keywords.Perf | MyCompanyEventSource.Keywords.Diagnostic);
      listener2.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);
      // Set up and enable the event listeners -  typically done when the application starts
      listener1.LogToConsole();
      // The SinkSubscription is used later to flush the buffer
      var subscription = listener2.LogToSqlDatabase("Demo Semantic Logging Instance", connectionString);

      // Log some messages
      MyCompanyEventSource.Log.PageStart(23, "http://mysite/demopage");
      MyCompanyEventSource.Log.Startup();
      MyCompanyEventSource.Log.Failure("Couldn't connect to server.");
      Console.WriteLine("Written three log messages.\nUsing a basic console listener and a SQL listener to capture them.");
      Console.WriteLine("Only the messages with the Perf or Diagnostic keywords appears in the console, \nall three appear in the SQL Database:\n");

      // Disable the event listener - typically done when the application terminates
      listener1.DisableEvents(MyCompanyEventSource.Log);
      listener2.DisableEvents(MyCompanyEventSource.Log);

      // Manually flush the buffer so you can see what's in the database
      subscription.Sink.FlushAsync().Wait();
      ShowContentsOfSqlDatabaseTable();

      listener1.Dispose();
      listener2.Dispose();
    }

    [Description("Verify the event source class")]
    static void VerifyEventSource()
    {
      try
      {
        EventSourceAnalyzer.InspectAll(MyCompanyEventSource.Log);
        Console.WriteLine("No incompatibilities found in MyCompanyEventSource");
      }
      catch (EventSourceAnalyzerException e)
      {
        Console.WriteLine("Incompatibilities found in MyCompanyEventSource");
        Console.WriteLine(e.Message);
      }

    }

    [Description("Using Rx to add filtering to the event stream before it sends events to the underlying sink.")]
    static void UsingRxFiltering()
    {
      // Configure the observable listener.
      var listener = new ObservableEventListener();
      listener.EnableEvents(MyCompanyEventSource.Log, EventLevel.LogAlways, Keywords.All);

      // Use the custom filter extension method (see the ObservableHelper class).
      // If an message of level Error is received, send it and the two previous
      // information messages to the console.
      listener
        .FlushOnTrigger(entry => entry.Schema.Level <= EventLevel.Error, bufferSize: 2)
        .LogToConsole();

      Console.WriteLine("Sending 20 informational messages and one error message.");
      for (int i = 0; i < 20; i++)
      {
        MyCompanyEventSource.Log.DBQueryStart("select... (query #" + i);
      }
      MyCompanyEventSource.Log.DBQueryError(231);
      Console.WriteLine("Only the last two information messages and the error message get sent to the console sink.");

      listener.Dispose();
    }


    #region Auxiliary routines

    static void ShowContentsOfSqlDatabaseTable()
    {

      Console.WriteLine("ID Timestamp                   Message");
      Console.WriteLine("== =========================== ========================");
      LogMessagesDataContext db = new LogMessagesDataContext(connectionString);
      Table<Trace> traces = db.GetTable<Trace>();
      foreach (var trace  in traces)
      {
        Console.WriteLine("{0}  {1}  {2}", trace.EventId, trace.Timestamp, trace.FormattedMessage);
      }

    }

    #endregion
  }

  public class MyCustomColorMapper : IConsoleColorMapper
  {
    public ConsoleColor? Map(
      System.Diagnostics.Tracing.EventLevel eventLevel)
    {
      switch (eventLevel)
      {
        case EventLevel.Critical:
          return ConsoleColor.White;
        case EventLevel.Error:
          return ConsoleColor.DarkMagenta;
        case EventLevel.Warning:
          return ConsoleColor.DarkYellow;
        case EventLevel.Verbose:
          return ConsoleColor.Blue;
        default:
          return null;
      }
    }
  }

}
