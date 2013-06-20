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
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

// references to configuration namespaces (required in all examples)
using System.ComponentModel;
using DevGuideExample.MenuSystem;

// references to application block namespace(s) for these examples
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;


namespace LoggingExample
{
    class Program
    {

        static LogWriter defaultWriter;

        static void Main(string[] args)
        {

            #region Create the required objects


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


            new MenuDrivenApplication("Logging Block Developer's Guide Examples",
                SimpleLogWriterWrite
                ).Run();
            #endregion
        }

        [Description("Using programmatic configuration")]
        static void SimpleLogWriterWrite()
        {
          // Build the configuration programtically.
          LoggingConfiguration loggingConfiguration = BuildProgrammaticConfig();
          defaultWriter = new LogWriter(loggingConfiguration);


          // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // The default values if not specified in call to Write method are:
                // - Category: "General"
                // - Priority: -1
                // - Event ID: 1
                // - Severity: Information
                // - Title: [none]
                defaultWriter.Write("Log entry created using the simplest overload.");
                Console.WriteLine("Created a Log Entry using the simplest overload.");
                defaultWriter.Write("Log entry with a single category.", "General");
                Console.WriteLine("Created a Log Entry with a single category.");
                defaultWriter.Write("Log entry with a category, priority, and event ID.", "General", 6, 9001);
                Console.WriteLine("Created a Log Entry with a category, priority, and event ID.");
                defaultWriter.Write("Log entry with a category, priority, event ID, and severity.", "General", 5, 9002, TraceEventType.Warning);
                Console.WriteLine("Created a Log Entry with a category, priority, event ID, and severity.");
                defaultWriter.Write("Log entry with a category, priority, event ID, severity, and title.", "General", 8, 9003, TraceEventType.Warning, "Logging Block Examples");
                Console.WriteLine("Created a Log Entry with a category, priority, event ID, severity, and title.");
                Console.WriteLine();
                Console.WriteLine(@"Open 'C:\Temp\ConfigSampleFlatFile.log' to see the results.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }


        static LoggingConfiguration BuildProgrammaticConfig()
        {
          // Formatters
          TextFormatter formatter = new TextFormatter("Timestamp: {timestamp(local)}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}ActivityId: {property(ActivityId)}{newline}Severity: {severity}{newline}Title:{title}{newline}");

          // Category Filters
          ICollection<string> categories = new List<string>();
          categories.Add("BlockedByFilter");

          // Log Filters
          var priorityFilter = new PriorityFilter("Priority Filter", 2, 99);
          var logEnabledFilter = new LogEnabledFilter("LogEnabled Filter", true);
          var categoryFilter = new CategoryFilter("Category Filter", categories, CategoryFilterMode.AllowAllExceptDenied);

          // Trace Listeners
          var flatFileTraceListener = new FlatFileTraceListener(@"C:\Temp\ConfigSampleFlatFile.log", "----------------------------------------", "----------------------------------------", formatter);

          // Build Configuration
          var config = new LoggingConfiguration();
          config.Filters.Add(priorityFilter);
          config.Filters.Add(logEnabledFilter);
          config.Filters.Add(categoryFilter);

          config.AddLogSource("General", SourceLevels.All, true, flatFileTraceListener);

          return config;
        }
    }
}
