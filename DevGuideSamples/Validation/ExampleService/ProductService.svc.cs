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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

// references to configuration namespaces (required in all examples)
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

// references to application block namespace(s) for these examples
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Integration.WCF;

namespace ExampleService
{
  [ServiceContract]
  [ValidationBehavior]
  public interface IProductService
  {
    [OperationContract]
    [FaultContract(typeof(ValidationFault))]
    bool AddNewProduct(
			[NotNullValidator(MessageTemplate = "You must specify a value for the product ID.")]
      [StringLengthValidator(6, RangeBoundaryType.Inclusive, 6, RangeBoundaryType.Inclusive,
        MessageTemplate = "Product ID must be {3} characters.")]
      [RegexValidator("[A-Z]{2}[0-9]{4}",
      MessageTemplate = "Product ID must be two capital letters and four numbers.")]
      string id,
      [StringLengthValidator(3, RangeBoundaryType.Inclusive, 50, RangeBoundaryType.Inclusive,
         MessageTemplate = "Name must be between {3} and {5} characters.")]
      string name,
      [IgnoreNulls(MessageTemplate = "Description can be NULL or a string value.")]
      [StringLengthValidator(5, RangeBoundaryType.Inclusive, 100, RangeBoundaryType.Inclusive,
          MessageTemplate = "Description must be between {3} and {5} characters.")]
      string description,
      [EnumConversionValidator(typeof(ProductType),
        MessageTemplate = "Product type must be a value from the '{3}' enumeration.")]
      string prodType,
      [RangeValidator(0, RangeBoundaryType.Inclusive, 0, RangeBoundaryType.Ignore,
        MessageTemplate = "Quantity in stock cannot be less than {3}.")]
      int inStock,
      [RangeValidator(0, RangeBoundaryType.Inclusive, 0, RangeBoundaryType.Ignore,
        MessageTemplate = "Quantity on order cannot be less than {3}.")]
      int onOrder,
      [ValidatorComposition(CompositionType.Or,
        MessageTemplate = "Date due must be between today and six months time.")]
      [NotNullValidator(Negated = true,
        MessageTemplate = "Value can be NULL or a date.")]
      [RelativeDateTimeValidator(0, DateTimeUnit.Day, 6, DateTimeUnit.Month,
        MessageTemplate = "Value can be NULL or a date.")]
      DateTime? dateDue);
  }

  public class ProductService : IProductService
  {
    public bool AddNewProduct(
      string id,
      string name,
      string description,
      string prodType,
      int inStock,
      int onOrder,
      DateTime? dateDue)
    {
      try
      {
        List<Product> productList = new List<Product>();
        productList.Add(new Product(id, name, description, prodType, inStock, onOrder, dateDue));
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
