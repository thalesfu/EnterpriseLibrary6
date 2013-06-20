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

// references to configuration namespaces (required in all examples)
using System.ComponentModel;
using DevGuideExample.MenuSystem;

// references to application block namespace(s) for these examples
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using ExceptionHandlingExample.SalaryService;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging;

namespace ExceptionHandlingExample
{
  class Program
  {
    static ExceptionManager exManager;

    static void Main(string[] args)
    {
      #region Create the required objects

      LoggingConfiguration loggingConfiguration = BuildLoggingConfig();
      LogWriter logWriter = new LogWriter(loggingConfiguration);
      //Logger.SetLogWriter(logWriter, false);

      // Create the default ExceptionManager object from the configuration settings.
      //ExceptionPolicyFactory policyFactory = new ExceptionPolicyFactory();
      //exManager = policyFactory.CreateManager();

      // Create the default ExceptionManager object programatically
      exManager = BuildExceptionManagerConfig(logWriter);

      // Create an ExceptionPolicy to illustrate the static HandleException method
      ExceptionPolicy.SetExceptionManager(exManager);


      #endregion

      #region Main menu routines

      var app = new MenuDrivenApplication("Exception Handling Block Developer's Guide Examples",
          DefaultNoExceptionShielding,
          WithWrapExceptionShielding,
          WithWrapExceptionShieldingStatic,
          WithReplaceExceptionShielding,
          LoggingTheException,
          ShieldingExceptionsInWCF,
          ExecutingCodeAroundException,
          ProvidingAdminAssistance);

      app.Run();

      #endregion
    }

    // You can run the examples in one of two ways inside Visual Studio:
    // -----------------------------------------------------------------
    // 1: By starting them with F5 (debugging mode) and then pressing F5 again
    //    when the debugger halts at the exception in the SalaryCalculator class,
    //    or:
    // 2: By starting them Ctrl-F5 (non-debugging mode).
    [Description("Typical Default Behavior without Exception Shielding")]
    static void DefaultNoExceptionShielding()
    {
      Console.WriteLine("Getting salary for 'jsmith'...");
      Console.WriteLine();
      SalaryCalculator calc = new SalaryCalculator();
      Console.WriteLine("Result is: {0}", calc.GetWeeklySalary("jsmith", 0));
    }

    [Description("Behavior After Applying Exception Shielding with a Wrap Handler")]
    static void WithWrapExceptionShielding()
    {
      Console.WriteLine("Getting salary for 'jsmith'...");
      Console.WriteLine();
      // NOTE: Any exception raised when creating the SalaryCalculator
      // class instance will not be handled using this approach.
      SalaryCalculator calc = new SalaryCalculator();
      var result = exManager.Process(() => calc.GetWeeklySalary("jsmith", 0), "ExceptionShielding");
      Console.WriteLine("Result is: {0}", result);
      // NOTE: If you do not need to return the value from the function, you can
      // simply consume it within the lambda expression. This is a simple example:
      // ------------------------------
      //exManager.Process(() =>
      //  {
      //    SalaryCalculator calc = new SalaryCalculator();
      //    Console.WriteLine("Result is: {0}", calc.GetWeeklySalary("jsmith", 0));
      //  },
      //  "ExceptionShielding");
      // ------------------------------
      // This approach also allows you to handle any exception raised by creating the 
      // instance of the SalaryCalculator class.
    }

    // This example behaves in the same way as the previous example but uses
    // the static HandleException method instead of the Process method.
    [Description("Using the static ExceptionPolicy class")]
    static void WithWrapExceptionShieldingStatic()
    {
      Console.WriteLine("Getting salary for 'jsmith'...");
      Console.WriteLine();

      // NOTE: Any exception raised when creating the SalaryCalculator
      // class instance will not be handled using this approach.
      SalaryCalculator calc = new SalaryCalculator();
      decimal result = 0;
      try
      {
        result = calc.GetWeeklySalary("jsmith", 0);
      }
      catch (Exception ex)
      {
        Exception exceptionToThrow;
        if (ExceptionPolicy.HandleException(ex, "ExceptionShielding", out exceptionToThrow))
        {
          if (exceptionToThrow == null)
            throw;
          else
            throw exceptionToThrow;
        }
      }
      Console.WriteLine("Result is: {0}", result);
      // NOTE: If you do not need to return the value from the function, you can
      // simply consume it within the lambda expression. This is a simple example:
      // ------------------------------
      //exManager.Process(() =>
      //  {
      //    SalaryCalculator calc = new SalaryCalculator();
      //    Console.WriteLine("Result is: {0}", calc.GetWeeklySalary("jsmith", 0));
      //  },
      //  "ExceptionShielding");
      // ------------------------------
      // This approach also allows you to handle any exception raised by creating the 
      // instance of the SalaryCalculator class.
    }

    [Description("Behavior After Applying Exception Shielding with a Replace Handler")]
    static void WithReplaceExceptionShielding()
    {
      Console.WriteLine("Getting salary for 'jsmith'...");
      Console.WriteLine();
      // NOTE: Any exception raised when creating the SalaryCalculator
      // class instance will not be handled using this approach.
      SalaryCalculator calc = new SalaryCalculator();
      decimal result = exManager.Process(() => calc.GetWeeklySalary("jsmith", 0), "ReplacingException");
      Console.WriteLine("Result is: {0}", result);
    }

    [Description("Logging an Exception to Preserve the Information it Contains")]
    static void LoggingTheException()
    {
      try
      {
        Console.WriteLine("Getting salary for 'jsmith'...");
        Console.WriteLine();
        // NOTE: Any exception raised when creating the SalaryCalculator
        // class instance will not be handled using this approach.
        var calc = new SalaryCalculator();
        decimal result = exManager.Process(() => calc.GetWeeklySalary("jsmith", 0), "LoggingAndReplacingException");
        Console.WriteLine("Result is: {0}", result);
      }
      catch (Exception ex)
      {
        MenuOption.ShowExceptionDetails(ex);
        Console.WriteLine("Open the Windows Application Event Log to see the logged exception details.");
      }
    }

    [Description("Applying Exception Shielding at WCF Application Boundaries")]
    static void ShieldingExceptionsInWCF()
    {
      // You can run this example in one of three ways:
      // - Inside VS by starting it with F5 (debugging mode) and then pressing F5 again
      //   when the debugger halts at the exception in the SalaryCalculator class.
      // - Inside VS by right-clicking SalaryService.svc in Solution Explorer and selecting
      //   View in Browser to start the service, then pressing Ctrl-F5 (non-debugging mode) 
      //   to run the application.
      // - By starting the SalaryService in VS (as in previous option) and then running the
      //   executable file ExceptionHandlingExample.exe in the bin\debug folder directly. 

      try
      {
        Console.WriteLine("Getting salary for 'jsmith' from WCF Salary Service...");
        // Create an instance of the client for the WCF Salary Service.
        ISalaryService svc = new SalaryServiceClient();
        // Call the method of the service to get the result.
        Console.WriteLine("Result is: {0}", svc.GetWeeklySalary("jsmith", 0));
      }
      catch (Exception ex)
      {
        // Show details of the exception returned to the calling code.  
        MenuOption.ShowExceptionDetails(ex);
        // Show details of the fault contract returned from the WCF service.  
        ShowFaultContract(ex);
      }
    }

    [Description("Executing Custom Code Before and After Handling an Exception")]
    static void ExecutingCodeAroundException()
    {
      //------------------------------------------------------
      // Note that this is a somewhat contrived exeample designed to
      // show how you can use the HandleException method, detect different
      // exception types, and ignore specific types of exceptions.
      try
      {
        // Execute code that raises a DivideByZeroException.
        Console.WriteLine("Getting salary for 'jsmith' ... this will raise a DivideByZero exception.");
        SalaryCalculator calc = new SalaryCalculator();
        Console.WriteLine("Result is: {0}", calc.RaiseDivideByZeroException("jsmith", 0));
      }
      catch (Exception ex)
      {
        Exception newException;
        bool rethrow = exManager.HandleException(ex, "LogAndWrap", out newException);
        if (rethrow)
        {
          // Exception policy setting is "ThrowNewException".
          // Code here to perform any clean up tasks required.
          // Then throw the exception returned by the exception handling policy.
          throw newException;
        }
        else
        {
          // Exception policy setting is "None" so exception is not thrown.
          // Code here to perform any other processing required.
          // In this example, just ignore the exception and do nothing.
          Console.WriteLine("Detected and ignored Divide By Zero Error - no value returned.");
        }
      }
      try
      {
        // Now execute code that raises an ArgumentOutOfRangeException.
        // Use the same exception handling policy and catch section code.
        Console.WriteLine();
        Console.WriteLine("Getting salary for 'jsmith' ... this will raise an ArgumentOutOfRange exception.");
        SalaryCalculator calc = new SalaryCalculator();
        Console.WriteLine("Result is: {0}", calc.RaiseArgumentOutOfRangeException("jsmith", 0));
      }
      catch (Exception ex)
      {
        Exception newException;
        bool rethrow = exManager.HandleException(ex, "LogAndWrap", out newException);
        if (rethrow)
        {
          // Exception policy could specify to throw the existing exception
          // or the new exception.
          // Code here to perform any clean up tasks required.
          if (newException == null)
            throw;
          else
           throw newException;
        }
        else
        {
          // Exception policy setting is "None".
          // Code here to perform any other processing required.
          // In this example, just ignore the exception and do nothing.
          Console.WriteLine("Detected and ignored Divide By Zero Error - no value returned.");
        }
      }
    }

    [Description("Providing Assistance to Administrators for Locating Exception Details")]
    static void ProvidingAdminAssistance()
    {
      Console.WriteLine("Getting salary for 'jsmith'...");
      Console.WriteLine();
      // NOTE: Any exception raised when creating the SalaryCalculator
      // class instance will not be handled using this approach.
      SalaryCalculator calc = new SalaryCalculator();
      decimal result = exManager.Process(() => calc.GetWeeklySalary("jsmith", 0), "AssistingAdministrators");
      Console.WriteLine("Result is: {0}", result);
    }

    #region Auxiliary routines

    private static ExceptionManager BuildExceptionManagerConfig(LogWriter logWriter)
    {
      var policies = new List<ExceptionPolicyDefinition>();

      var assistingAdministrators = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                       new LoggingExceptionHandler("General", 9001, TraceEventType.Error,
                         "Salary Calculations Service", 5, typeof(TextExceptionFormatter), logWriter),
                       new ReplaceHandler("Application error.  Please advise your administrator and provide them with this error code: {handlingInstanceID}",
                         typeof(SalaryException))
                     })
            };

      var exceptionShielding = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                       new WrapHandler("Application Error. Please contact your administrator.",
                         typeof(SalaryException))
                     })
            };

      var loggingAndReplacing = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                       new LoggingExceptionHandler("General", 9000, TraceEventType.Error,
                         "Salary Calculations Service", 5, typeof(TextExceptionFormatter), logWriter),
                       new ReplaceHandler("An application error occurred and has been logged. Please contact your administrator.",
                         typeof(SalaryException))
                     })
            };

      var logAndWrap = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (DivideByZeroException),
                    PostHandlingAction.None,
                    new IExceptionHandler[]
                     {
                       new LoggingExceptionHandler("General", 9002, TraceEventType.Error,
                         "Salary Calculations Service", 5, typeof(TextExceptionFormatter), logWriter),
                       new ReplaceHandler("Application error will be ignored and processing will continue.",
                         typeof(SalaryException))
                     }),
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                       new WrapHandler("An application error has occurred.",
                         typeof(SalaryException))
                     })
            };

      var replacingException = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                       new ReplaceHandler("Application Error. Please contact your administrator.",
                         typeof(SalaryException))
                     })
            };

      policies.Add(new ExceptionPolicyDefinition("AssistingAdministrators", assistingAdministrators));
      policies.Add(new ExceptionPolicyDefinition("ExceptionShielding", exceptionShielding));
      policies.Add(new ExceptionPolicyDefinition("LoggingAndReplacingException", loggingAndReplacing));
      policies.Add(new ExceptionPolicyDefinition("LogAndWrap", logAndWrap));
      policies.Add(new ExceptionPolicyDefinition("ReplacingException", replacingException));
      return new ExceptionManager(policies);
    }

    private static LoggingConfiguration BuildLoggingConfig()
    {
      // Formatters
      TextFormatter formatter = new TextFormatter("Timestamp: {timestamp}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}Severity: {severity}{newline}Title:{title}{newline}Machine: {localMachine}{newline}App Domain: {localAppDomain}{newline}ProcessId: {localProcessId}{newline}Process Name: {localProcessName}{newline}Thread Name: {threadName}{newline}Win32 ThreadId:{win32ThreadId}{newline}Extended Properties: {dictionary({key} - {value}{newline})}");

      // Listeners
      var flatFileTraceListener = new FlatFileTraceListener(@"C:\Temp\SalaryCalculator.log", "----------------------------------------", "----------------------------------------", formatter);
      var eventLog = new EventLog("Application", ".", "Enterprise Library Logging");
      var eventLogTraceListener = new FormattedEventLogTraceListener(eventLog);
      // Build Configuration
      var config = new LoggingConfiguration();
      config.AddLogSource("General", SourceLevels.All, true).AddTraceListener(eventLogTraceListener);
      config.LogSources["General"].AddTraceListener(flatFileTraceListener);

      // Special Sources Configuration
      config.SpecialSources.LoggingErrorsAndWarnings.AddTraceListener(eventLogTraceListener);

      return config;
    }

    private static void ShowFaultContract(Exception ex)
    {
      var faultContract = ex as FaultException<SalaryCalculationFault>;
      if (faultContract != null)
      {
        SalaryCalculationFault salaryCalculationFault = faultContract.Detail;
        Console.WriteLine("Fault contract detail: ");
        Console.WriteLine("Fault ID: {0}", salaryCalculationFault.FaultID);
        Console.WriteLine("Message: {0}", salaryCalculationFault.FaultMessage);
      }
    }

    #endregion
  }

  [Serializable]
  public class SalaryException : Exception
  {
    public SalaryException() {}

    public SalaryException(string message): base(message) {}

    public SalaryException(string message, Exception inner): base(message, inner) {}

    public SalaryException(SerializationInfo info, StreamingContext context): base(info, context) {}

  }
}
