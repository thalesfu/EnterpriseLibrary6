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
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ValidationExample
{
	public class Product : IProduct
  {
    public Product()
    { }

    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProductType { get; set; }
		public int InStock { get; set; }
    public int OnOrder { get; set; }
    public DateTime? DateDue { get; set; }

    public Product(string id, string name, string description, string prodType,
                   int inStock, int onOrder, DateTime? dateDue)
    {
      ID = id;
      Name = name;
      Description = description;
      ProductType = prodType;
      InStock = inStock;
      OnOrder = onOrder;
      DateDue = dateDue;
    }
	}
}
