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
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;

namespace ExampleService
{
  public class Global : System.Web.HttpApplication
  {

    protected void Application_Start(object sender, EventArgs e)
    {
      ConfigureExceptionHandlingBlock();
    }

    private void ConfigureExceptionHandlingBlock()
    {
      var policies = new List<ExceptionPolicyDefinition>();

      var mappings = new NameValueCollection();
      mappings.Add("FaultID", "{Guid}");
      mappings.Add("FaultMessage", "{Message}");

      var salaryServicePolicy = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof(Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                    {
                        new FaultContractExceptionHandler(typeof(SalaryCalculationFault), "Service Error. Please contact your administrator", mappings)
                    })
            };
      policies.Add(new ExceptionPolicyDefinition("SalaryServicePolicy", salaryServicePolicy));
      ExceptionManager exManager = new ExceptionManager(policies);
      ExceptionPolicy.SetExceptionManager(exManager);
    }
  }
}
