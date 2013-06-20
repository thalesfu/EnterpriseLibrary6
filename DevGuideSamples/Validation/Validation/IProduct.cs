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
using System.Linq;
using System.Text;

namespace ValidationExample
{
  public interface IProduct
  {
    string ID {get; set;}
    string Name { get; set; }
    string Description { get; set; }
    string ProductType { get; set; }
    int InStock { get; set; }
    int OnOrder { get; set; }
    DateTime? DateDue { get; set; }
  }
}
