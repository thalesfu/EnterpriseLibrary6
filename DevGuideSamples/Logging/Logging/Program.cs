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
using DevGuideExample.MenuSystem;
using System.ComponentModel;

// references to application block namespace(s) for these examples
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Logging.Database;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace LoggingExample
{
    class Program
    {

        static LogWriter defaultWriter;
        static TraceManager traceMgr;

        static void Main(string[] args)
        {
            #region Create the required objects

            DatabaseProviderFactory factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
            DatabaseFactory.SetDatabaseProviderFactory(factory, false);

            LoggingConfiguration loggingConfiguration = BuildProgrammaticConfig();
            defaultWriter = new LogWriter(loggingConfiguration);

            // Create a TraceManager object for use in activity tracing example.
            traceMgr = new TraceManager(defaultWriter);

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

            new MenuDrivenApplication("Logging Block Developer's Guide Examples",
                SimpleLogWriterWrite,
                SimpleLogWriterWriteDecalarative,
                MultipleCategoriesLogWriterWrite,
                SimpleLogEntryWrite,
                SpecialSources,
                LogToDatabase,
                DynamicReconfiguration,
                CheckStatusAndAddContextInfo,
                UseAsyncWrapper,
                TraceActivities).Run();

            defaultWriter.Dispose();
 

        }

        [Description("Simple logging with the Write method of a LogWriter")]
        static void SimpleLogWriterWrite()
        {
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
                Console.WriteLine("Open Windows Event Viewer 'Application' Log to see the results.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Simple logging with the LogWriter using declarative configuration")]
        static void SimpleLogWriterWriteDecalarative()
        {
          // Create the default LogWriter object from the configuration.
          // The actual concrete type is determined by the configuration settings.
          Console.WriteLine("This example reads the configuration data from the config file.");
          LogWriterFactory logWriterFactory = new LogWriterFactory();
          var logWriter = logWriterFactory.Create();

          // Check if logging is enabled before creating log entries.
          if (logWriter.IsLoggingEnabled())
          {
            // The default values if not specified in call to Write method are:
            // - Category: "General"
            // - Priority: -1
            // - Event ID: 1
            // - Severity: Information
            // - Title: [none]
            logWriter.Write("Log entry created using the simplest overload (Declarative configuration).");
            Console.WriteLine("Created a Log Entry using the simplest overload.");
            logWriter.Write("Log entry with a single category. (Declarative configuration)", "General");
            Console.WriteLine("Created a Log Entry with a single category.");
            logWriter.Write("Log entry with a category, priority, and event ID. (Declarative configuration)", "General", 6, 9001);
            Console.WriteLine("Created a Log Entry with a category, priority, and event ID.");
            logWriter.Write("Log entry with a category, priority, event ID, and severity. (Declarative configuration)", "General", 5, 9002, TraceEventType.Warning);
            Console.WriteLine("Created a Log Entry with a category, priority, event ID, and severity.");
            logWriter.Write("Log entry with a category, priority, event ID, severity, and title. (Declarative configuration)", "General", 8, 9003, TraceEventType.Warning, "Logging Block Examples");
            Console.WriteLine("Created a Log Entry with a category, priority, event ID, severity, and title.");
            Console.WriteLine();
            Console.WriteLine("Open Windows Event Viewer 'Application' Log to see the results.");
          }
          else
          {
            Console.WriteLine("Logging is disabled in the configuration.");
          }
        }

        [Description("Logging to multiple categories with the Write method of a LogWriter")]
        static void MultipleCategoriesLogWriterWrite()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // Create a string array (or List<>) containing the categories.
                string[] logCategories = new string[] { "DiskFiles", "Important" };
                // Write the log entries using these categories.
                defaultWriter.Write("Log entry with multiple categories.", logCategories);
                Console.WriteLine("Created a Log Entry with multiple categories.");
                defaultWriter.Write("Log entry with multiple categories, a priority, and an event ID.", logCategories, 7, 9004);
                Console.WriteLine("Created a Log Entry with multiple categories, a priority, and an event ID.");
                defaultWriter.Write("Log entry with multiple categories, a priority, event ID, severity, and title.", logCategories, 10, 9005, TraceEventType.Critical, "Logging Block Examples");
                Console.WriteLine("Created a Log Entry with multiple categories, a priority, event ID, severity, and title.");
                Console.WriteLine();
                Console.WriteLine(@"Open Event Viewer 'Application' Log and the folder C:\Temp to see the results.");
                Console.WriteLine(@"Note that the XML Trace Listener has the Filter property set to 'Error'.");
                Console.WriteLine("Therefore, it will log only events with a severity of Error or higher.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Creating and writing log entries with a LogEntry object")]
        static void SimpleLogEntryWrite()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // Create a Dictionary of extended properties
                Dictionary<string, object> exProperties = new Dictionary<string, object>();
                exProperties.Add("Extra Information", "Some Special Value");
                // Create a LogEntry using the constructor parameters. 
                LogEntry entry1 = new LogEntry("LogEntry with category, priority, event ID, severity, and title.", "General", 8, 9006, TraceEventType.Error, "Logging Block Examples", exProperties);
                defaultWriter.Write(entry1);
                Console.WriteLine("Created and written LogEntry with a category, priority, event ID, severity, and title.");
                Console.WriteLine();
                // Create a LogEntry and populate the individual properties.
                LogEntry entry2 = new LogEntry
                {
                  Categories = new string[] { "General" },
                  EventId = 9007,
                  Message = "LogEntry with individual properties specified.",
                  Priority = 9,
                  Severity = TraceEventType.Warning,
                  Title = "Logging Block Examples",
                  ExtendedProperties = exProperties
                };
                defaultWriter.Write(entry2);
                Console.WriteLine("Created and written LogEntry with individual properties specified.");
                Console.WriteLine();
                Console.WriteLine("Open Windows Event Viewer 'Application' Log to see the results.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Using Special Sources to capture unprocessed events or errors")]
        static void SpecialSources()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // Create a log entry that will be processed by the "Unprocessed" special source.
                defaultWriter.Write("Entry with category not defined in configuration.", "InvalidCategory");
                Console.WriteLine("Created a Log Entry with a category name not defined in the configuration.");
                Console.WriteLine(@"The Log Entry will appear in the Unprocessed.log file in the C:\Temp folder.");
                Console.WriteLine();
                // Create a log entry that will be processed by the "Errors & Warnings" special source.
                defaultWriter.Write("Entry that causes a logging error.", "CauseLoggingError");
                Console.WriteLine("Created a Log Entry that causes a logging error.");
                Console.WriteLine(@"The Log Entry will appear in the Windows Application Event Log.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Sending log entries to a database")]
        static void LogToDatabase()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // Create a Dictionary of extended properties
                Dictionary<string, object> exProperties = new Dictionary<string, object>();
                exProperties.Add("Extra Information", "Some Special Value");
                // Create a LogEntry using the constructor parameters. 
                defaultWriter.Write("Log entry with category, priority, event ID, severity, title, and extended properties.", "Database",
                                                        5, 9008, TraceEventType.Warning, "Logging Block Examples", exProperties);
                Console.WriteLine("Created a log entry with a category, priority, event ID, severity, title, and   extended properties.");
                Console.WriteLine();
                // Create a LogEntry using the constructor parameters. 
                LogEntry entry = new LogEntry("LogEntry with category, priority, event ID, severity, title, and extended properties.", "Database",
                                                8, 9009, TraceEventType.Error, "Logging Block Examples", exProperties);
                defaultWriter.Write(entry);
                Console.WriteLine("Created and written LogEntry with a category, priority, event ID, severity,     title, and extended properties.");
                Console.WriteLine();
                Console.WriteLine(@"Open the 'Logging.mdf' database in the Bin\Debug folder to see the results.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Dynamically changing logging settings")]
        static void DynamicReconfiguration()
        {
          // Check if logging is enabled before creating log entries.
          if (defaultWriter.IsLoggingEnabled())
          {
            defaultWriter.Write("Log entry with a category, priority, and event ID.", "General", 6, 9001);
            Console.WriteLine("Created a Log Entry with priority 6.");
            defaultWriter.Write("Log entry with a category, priority, and event ID.", "General", 106, 9001);
            Console.WriteLine("Created a Log Entry with priority 106.");
            Console.WriteLine("The Priority Filter filtered out the entry with priority 106.");

            ReplacePriorityFilter(199);

            Console.WriteLine("Modified the priority filter to allow both entries.");
            defaultWriter.Write("Log entry with a category, priority, and event ID.", "General", 6, 9001);
            Console.WriteLine("Created a Log Entry with priority 6.");
            defaultWriter.Write("Log entry with a category, priority, and event ID.", "General", 106, 9001);
            Console.WriteLine("Created a Log Entry with priority 106.");
            Console.WriteLine("The Priority Filter allowed the entry with priority 106.");
            Console.WriteLine("\nOpen Windows Event Viewer 'Application' Log to see the results.");

            ReplacePriorityFilter(99);
          }
          else
          {
            Console.WriteLine("Logging is disabled in the configuration.");
          }

        }

        [Description("Checking filter status and adding context information to the log entry")]
        static void CheckStatusAndAddContextInfo()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {
                // Create a new LogEntry with two categories and priority 3.
                string[] logCategories = new string[] { "General", "DiskFiles" };
                LogEntry entry1 = new LogEntry("LogEntry with categories 'General' and 'DiskFiles' and Priority 3.", logCategories, 3, 9009, TraceEventType.Error, "Logging Block Examples", null);
                Console.WriteLine("Created a LogEntry with categories 'General' and 'DiskFiles'.");
                ShowDetailsAndAddExtraInfo(entry1);
                // Create a new LogEntry with one category and priority 1.
                logCategories = new string[] { "BlockedByFilter" };
                LogEntry entry2 = new LogEntry("LogEntry with category 'BlockedByFilter' and Priority 1.", logCategories, 1, 9010, TraceEventType.Information, "Logging Block Examples", null);
                Console.WriteLine("Created a LogEntry with category 'BlockedByFilter', and Priority 1.");
                ShowDetailsAndAddExtraInfo(entry2);
                Console.WriteLine();
                Console.WriteLine("To view the LogEntries, look in the Windows Application Event Log.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }

        [Description("Write log entries asynchronously")]
        static void UseAsyncWrapper()
        {
          // The async listener was created using the following code (see the BuildProgrammaticConfig method below):
          // var asyncDatabaseTraceListener = 
          //   new AsynchronousTraceListenerWrapper(databaseTraceListener, false);
          // Check if logging is enabled before creating log entries.
          if (defaultWriter.IsLoggingEnabled())
          {
            // Create a Dictionary of extended properties
            Dictionary<string, object> exProperties = new Dictionary<string, object>();
            exProperties.Add("Extra Information", "Some Special Value");
            // Create a LogEntry using the constructor parameters. 
            defaultWriter.Write("Log entry with category, priority, event ID, severity, title, and extended properties. Written asynchronously.", "AsyncDatabase",
                                                    5, 9008, TraceEventType.Warning, "Logging Block Examples", exProperties);
            Console.WriteLine("Created a log entry with a category, priority, event ID, severity, title, and   extended properties.");
            Console.WriteLine();
            Console.WriteLine(@"Open the 'Logging.mdf' database in the Bin\Debug folder to see the results.");
          }
          else
          {
            Console.WriteLine("Logging is disabled in the configuration.");
          }
        }

        [Description("Tracing activities and publishing activity information to categories")]
        static void TraceActivities()
        {
            // Check if logging is enabled before creating log entries.
            if (defaultWriter.IsLoggingEnabled())
            {

                Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                Console.WriteLine();
                // Start tracing for the category 'General'. All log entries within the trace context
                // will be included in this category and will use any specified Activity ID (GUID).
                // If you do not specify an Activity ID, the TraceManager will create a new one.
                using (traceMgr.StartTrace("General"))
                {
                    // Create and write a log entry with another category - it will be assigned to both.
                    defaultWriter.Write("LogEntry with category 'DiskFiles' created within context of 'General' category tracer.", "DiskFiles");
                    Console.WriteLine("Written LogEntry with category 'DiskFiles' created within context of 'General'   category tracer.");
                    Console.WriteLine();
                    Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                    Console.WriteLine();
                    // Start tracing for the category 'Database' within the context of the 'General' TraceManager.
                    // Do not specify a GUID to use so that the existing one is used.
                    using (traceMgr.StartTrace("Database"))
                    {
                        // Create and write a log entry with another category - it will be assigned to all three.
                        defaultWriter.Write("LogEntry with category 'Important' created within context of first nested 'Database' category tracer.", "Important");
                        Console.WriteLine("Written LogEntry with category 'Important' created within context of first       'Database' category tracer nested within 'DiskFiles' category TraceManager.");
                        Console.WriteLine();
                        Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                        Console.WriteLine();
                    }
                    Console.WriteLine("Leaving the context of the first Database tracer");
                    Console.WriteLine();
                    Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                    Console.WriteLine();
                    // Start tracing for the category 'Database' within the context of the 'General' TraceManager.
                    // This time, specify a GUID to use.
                    using (traceMgr.StartTrace("Database", new Guid("{12345678-1234-1234-1234-123456789ABC}")))
                    {
                        // Create and write a log entry with another category - it will be assigned to all three.
                        defaultWriter.Write("LogEntry with category 'Important' created within context of second nested 'Database' category tracer.", "Important");
                        Console.WriteLine("Written LogEntry with category 'Important' created within context of second      'Database' category tracer nested within 'DiskFiles' category TraceManager.");
                        Console.WriteLine();
                        Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                        Console.WriteLine();
                    }
                    Console.WriteLine("Leaving the context of the second Database tracer");
                    Console.WriteLine();
                    Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                    Console.WriteLine();
                }
                Console.WriteLine("Leaving the context of the General tracer");
                Console.WriteLine();
                Console.WriteLine("- Current Activity ID is: {0}", Trace.CorrelationManager.ActivityId);
                Console.WriteLine();
                Console.WriteLine(@"Open the log files in the folder C:\Temp to see the results.");
            }
            else
            {
                Console.WriteLine("Logging is disabled in the configuration.");
            }
        }


        #region Auxiliary routines

        static LoggingConfiguration BuildProgrammaticConfig()
        {
          // Formatters
          TextFormatter briefFormatter = new TextFormatter("Timestamp: {timestamp(local)}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}ActivityId: {property(ActivityId)}{newline}Severity: {severity}{newline}Title:{title}{newline}");
          TextFormatter extendedFormatter = new TextFormatter("Timestamp: {timestamp}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}Severity: {severity}{newline}Title: {title}{newline}Activity ID: {property(ActivityId)}{newline}Machine: {localMachine}{newline}App Domain: {localAppDomain}{newline}ProcessId: {localProcessId}{newline}Process Name: {localProcessName}{newline}Thread Name: {threadName}{newline}Win32 ThreadId:{win32ThreadId}{newline}Extended Properties: {dictionary({key} - {value}{newline})}");

          // Category Filters
          ICollection<string> categories = new List<string>();
          categories.Add("BlockedByFilter");

          // Log Filters
          var priorityFilter = new PriorityFilter("Priority Filter", 2, 99);
          var logEnabledFilter = new LogEnabledFilter("LogEnabled Filter", true);
          var categoryFilter = new CategoryFilter("Category Filter", categories, CategoryFilterMode.AllowAllExceptDenied);

          // Trace Listeners
          var causeLoggingErrorTraceListener = new FormattedDatabaseTraceListener(DatabaseFactory.CreateDatabase("DoesNotExist"), "WriteLog", "AddCategory", null);
          var databaseTraceListener = new FormattedDatabaseTraceListener(DatabaseFactory.CreateDatabase("ExampleDatabase"),"WriteLog","AddCategory",extendedFormatter);
          var flatFileTraceListener = new FlatFileTraceListener(@"C:\Temp\FlatFile.log", "----------------------------------------", "----------------------------------------", briefFormatter);
          var eventLog = new EventLog("Application", ".", "Enterprise Library Logging");
          var eventLogTraceListener = new FormattedEventLogTraceListener(eventLog);
          var rollingFlatFileTraceListener = new RollingFlatFileTraceListener(@"C:\Temp\RollingFlatFile.log", "----------------------------------------", "----------------------------------------", extendedFormatter, 20, "yyyy-MM-dd", RollFileExistsBehavior.Increment,RollInterval.None, 3);
          var unprocessedFlatFileTraceListener = new FlatFileTraceListener(@"C:\Temp\Unprocessed.log", "----------------------------------------", "----------------------------------------", extendedFormatter);
          var xmlTraceListener = new XmlTraceListener(@"C:\Temp\XmlLogFile.xml");
          xmlTraceListener.Filter = new EventTypeFilter(SourceLevels.Error);

          // Build Configuration
          var config = new LoggingConfiguration();
          config.Filters.Add(priorityFilter);
          config.Filters.Add(logEnabledFilter);
          config.Filters.Add(categoryFilter);

          config.AddLogSource("BlockedByFilter", SourceLevels.All, true).AddTraceListener(eventLogTraceListener);
          config.AddLogSource("CauseLoggingError", SourceLevels.All, true).AddTraceListener(causeLoggingErrorTraceListener);
          config.AddLogSource("Database", SourceLevels.All, true).AddTraceListener(databaseTraceListener);
          // The defaults for the asynchronous wrapper are:
          //   bufferSize: 30000
          //   disposeTimeout: infinite
          config.AddLogSource("AsyncDatabase", SourceLevels.All, true).AddAsynchronousTraceListener(databaseTraceListener);
          config.AddLogSource("DiskFiles", SourceLevels.All, true).AddTraceListener(flatFileTraceListener);
          config.LogSources["DiskFiles"].AddTraceListener(xmlTraceListener);
          config.AddLogSource("General", SourceLevels.All, true).AddTraceListener(eventLogTraceListener);
          config.AddLogSource("Important", SourceLevels.All, true).AddTraceListener(eventLogTraceListener);
          config.LogSources["Important"].AddTraceListener(rollingFlatFileTraceListener);

          // Special Sources Configuration
          config.SpecialSources.Unprocessed.AddTraceListener(unprocessedFlatFileTraceListener);
          config.SpecialSources.LoggingErrorsAndWarnings.AddTraceListener(eventLogTraceListener);

          return config;
        }

        static void ReplacePriorityFilter(int maximumPriority)
        {
           defaultWriter.Configure(cfg => { 
              cfg.Filters.Clear();
              // Category Filters
              ICollection<string> categories = new List<string>();
              categories.Add("BlockedByFilter");

              // Log Filters
              var priorityFilter = new PriorityFilter("Priority Filter", 2, maximumPriority);
              var logEnabledFilter = new LogEnabledFilter("LogEnabled Filter", true);
              var categoryFilter = new CategoryFilter("Category Filter", categories, CategoryFilterMode.AllowAllExceptDenied);
              cfg.Filters.Add(priorityFilter);
              cfg.Filters.Add(logEnabledFilter);
              cfg.Filters.Add(categoryFilter);
          });
        }
      
        static void ShowDetailsAndAddExtraInfo(LogEntry entry)
        {
            // Display information about the Trace Sources and Listeners for this LogEntry. 
            IEnumerable<LogSource> sources = defaultWriter.GetMatchingTraceSources(entry);
            foreach (LogSource source in sources)
            {
                Console.WriteLine("Log Source name: '{0}'", source.Name);
                foreach (TraceListener listener in source.Listeners)
                {
                    Console.WriteLine(" - Listener name: '{0}'", listener.Name);
                }
            }
            // Check if any filters will block this LogEntry.
            // This approach allows you to check for specific types of filter.
            // If there are no filters of the specified type configured, the GetFilter 
            // method returns null, so check this before calling the ShouldLog method.
            CategoryFilter catFilter = defaultWriter.GetFilter<CategoryFilter>();
            if (null == catFilter || catFilter.ShouldLog(entry.Categories))
            {
                Console.WriteLine("Category Filter(s) will not block this LogEntry.");
            }
            else
            {
                Console.WriteLine("A Category Filter will block this LogEntry.");
            }
            PriorityFilter priFilter = defaultWriter.GetFilter<PriorityFilter>();
            if (null == priFilter || priFilter.ShouldLog(entry.Priority))
            {
                Console.WriteLine("Priority Filter(s) will not block this LogEntry.");
            }
            else
            {
                Console.WriteLine("A Priority Filter will block this LogEntry.");
            }
            // Alternatively, a simple approach can be used to check for any type of filter
            if (defaultWriter.ShouldLog(entry))
            {
                Console.WriteLine("This LogEntry will not be blocked due to configuration settings.");
                // Create the additional context information to add to the LogEntry. Checking that 
                // the LogEntry will not be blocked first minimizes the performance impact.
                Dictionary<string, object> dict = new Dictionary<string, object>();
                // Use the information helper classes to get information about the environment and add it to the dictionary.
                DebugInformationProvider debugHelper = new DebugInformationProvider();
                debugHelper.PopulateDictionary(dict);
                Console.WriteLine("Added the current stack trace to the Log Entry.");
                ManagedSecurityContextInformationProvider infoHelper = new ManagedSecurityContextInformationProvider();
                infoHelper.PopulateDictionary(dict);
                Console.WriteLine("Added current identity name, authentication type, and status to the Log Entry.");
                UnmanagedSecurityContextInformationProvider secHelper = new UnmanagedSecurityContextInformationProvider();
                secHelper.PopulateDictionary(dict);
                Console.WriteLine("Added the current user name and process account name to the Log Entry.");
                ComPlusInformationProvider comHelper = new ComPlusInformationProvider();
                comHelper.PopulateDictionary(dict);
                Console.WriteLine("Added COM+ IDs and caller account information to the Log Entry.");
                // Get any other information you require and add it to the dictionary.
                string configInfo = File.ReadAllText(@"..\..\App.config");
                dict.Add("Config information", configInfo);
                Console.WriteLine("Added information about the configuration of the application to the Log Entry.");
                // Set the dictionary in the LogEntry and write it using the default LogWriter.
                entry.ExtendedProperties = dict;
                defaultWriter.Write(entry);
                Console.WriteLine("LogEntry written to configured trace listeners.");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("This LogEntry will be blocked due to configuration settings.");
            }
        }

        #endregion
    }
}
