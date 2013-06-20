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
    public class AttributedProduct : IProduct
    {
        public AttributedProduct()
        { }

        public AttributedProduct(string id, string name, string description, string prodType,
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

				[NotNullValidator(MessageTemplate = "[{0}]You must specify a value for the product ID.")]
        [StringLengthValidator(6, RangeBoundaryType.Inclusive, 6, RangeBoundaryType.Inclusive,
          MessageTemplate = "[{0}]Product ID must be {3} characters.")]
        [RegexValidator("[A-Z]{2}[0-9]{4}",
					MessageTemplate = "[{0}]Product ID must be 2 capital letters and 4 numbers.")]
        public string ID { get; set; }

        [StringLengthValidator(3, RangeBoundaryType.Inclusive, 50, RangeBoundaryType.Inclusive,
						MessageTemplate = "[{0}]Name must be between {3} and {5} characters.")]
        public string Name { get; set; }

				[IgnoreNulls(MessageTemplate = "[{0}]Description can be NULL or a string value.")]
        [StringLengthValidator(5, RangeBoundaryType.Inclusive, 100, RangeBoundaryType.Inclusive,
						MessageTemplate = "[{0}]Description must be between {3} and {5} characters.")]
        public string Description { get; set; }

        [EnumConversionValidator(typeof(ProductType),
					MessageTemplate = "[{0}]Product type must be a value from the '{3}' enumeration.")]
        public string ProductType { get; set; }

        [RangeValidator(0, RangeBoundaryType.Inclusive, 0, RangeBoundaryType.Ignore,
					MessageTemplate = "[{0}]Quantity in stock cannot be less than {3}.")]
        public int InStock { get; set; }

        [RangeValidator(0, RangeBoundaryType.Inclusive, 0, RangeBoundaryType.Ignore,
					MessageTemplate = "[{0}]Quantity on order cannot be less than {3}.")]
        public int OnOrder { get; set; }

        [ValidatorComposition(CompositionType.Or,
					MessageTemplate = "[{0}]Date due must be between today and six months time.")]
        [NotNullValidator(Negated = true,
					MessageTemplate = "[{0}]Value can be NULL or a date.")]
        [RelativeDateTimeValidator(0, DateTimeUnit.Day, 6, DateTimeUnit.Month,
					MessageTemplate = "[{0}]Value can be NULL or a date.")]
        public DateTime? DateDue { get; set; }

        [SelfValidation]
        public void Validate(ValidationResults results)
        {
            string msg = string.Empty;

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
