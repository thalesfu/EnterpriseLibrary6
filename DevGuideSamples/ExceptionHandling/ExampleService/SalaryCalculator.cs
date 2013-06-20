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
using System.Configuration;

namespace ExampleService
{
  class SalaryCalculator
  {
    public decimal GetWeeklySalary(string employeeId, int weeks)
    {
      string connString = string.Empty;
      string employeeName = String.Empty;
      decimal salary = 0;
      try
      {
        // Access the database to get the salary for this employee.
        connString = ConfigurationManager.ConnectionStrings
                                  ["EmployeeDatabase"].ConnectionString;
        // ... etc.
        // In this example, just assume it's some large number.
        employeeName = "John Smith";
        salary = 1000000;
        return salary / weeks;
      }
      catch (Exception ex)
      {
        // Provide error information for debugging.
        string template = "Error calculating salary for {0}. " +
                          "Salary: {1}. Weeks: {2}\n" +
                          "Connection: {3}\n" +
                          "{4}";
        // Create a new exception to return.
        Exception informationException = new Exception(
          string.Format(template, employeeName, salary, weeks, connString, ex.Message));
        throw informationException;
      }
    }
  }
}
