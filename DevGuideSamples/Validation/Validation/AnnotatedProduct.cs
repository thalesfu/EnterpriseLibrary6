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
    [HasSelfValidation]
    public class AnnotatedProduct : IProduct
    {
        public AnnotatedProduct()
        { }

        public AnnotatedProduct(string id, string name, string description, string prodType,
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

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "You must specify a value for the product ID.")]
        [System.ComponentModel.DataAnnotations.StringLength(6, MinimumLength=6, ErrorMessage = "Product ID must be 6 characters.")]
        [System.ComponentModel.DataAnnotations.RegularExpression("[A-Z]{2}[0-9]{4}", ErrorMessage = "Product ID must be 2 capital letters and 4 numbers.")]
        public string ID { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3,  ErrorMessage = "Name must be between 3 and 50 characters.")]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 100 characters.")]
        public string Description { get; set; }

        public string ProductType { get; set; }

        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be less than 0.")]
        public int InStock { get; set; }

        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Quantity on order cannot be less than 0.")]
        public int OnOrder { get; set; }

        public DateTime? DateDue { get; set; }

        [SelfValidation]
        public void Validate(ValidationResults results)
        {
            string msg = string.Empty;

            // Rules that cannot be validated using Data Annotations.

						var enumConverterValidator = new EnumConversionValidator(typeof(ProductType), "Product type must be a value from the '{3}' enumeration.");
            enumConverterValidator.DoValidate(ProductType, this, "ProductType", results);


						if (DateDue.HasValue)
            {
                if (DateDue.Value < DateTime.Today ||
                    DateDue.Value > DateTime.Today.AddMonths(6))
                {
                    msg = "Date due must be between today and six months time.";
                    results.AddResult(new ValidationResult(msg, this, "DateDue", "", null));
                }
            }

            if (!DateDue.HasValue)
            {
                if (OnOrder > 0)
                {
                    msg = "Must provide a delivery due date for stock on back order.";
                    results.AddResult(new ValidationResult(msg, this, "ProductSelfValidation", "", null));
                }
            }
            else
            {
                if (OnOrder == 0)
                {
                    msg = "Can specify delivery due date only when stock is on back order.";
                    results.AddResult(new ValidationResult(msg, this, "ProductSelfValidation", "", null));
                }
            }

            if (InStock + OnOrder > 100)
            {
                msg = "Total inventory (in stock and on order) cannot exceed 100 items.";
                results.AddResult(new ValidationResult(msg, this, "ProductSelfValidation", "", null));
            }
        }
    }
}
