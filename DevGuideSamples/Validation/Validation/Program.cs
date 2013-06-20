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
using System.ComponentModel;
using System.Linq;

using DevGuideExample.MenuSystem;

// references to application block namespace(s) for these examples
using System.ServiceModel;
using ValidationExample.ProductService;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ValidationExample
{
    class Program
    {

        static void Main(string[] args)
        {
            // Create a default ValidationFactory object from the configuration.
            ValidationFactory.SetDefaultConfigurationValidatorFactory(new SystemConfigurationSource(false));

            //AttributeValidatorFactory attrFactory = new AttributeValidatorFactory();

            new MenuDrivenApplication("Validation Block Developer's Guide Examples",
                UsingAValidationRuleSetToValidateAnObject,
                ValidatingACollectionOfObjects,
                UsingValidationAttributesAndSelfValidation,
                UsingDataAnnotationsValidationAttributesAndSelfValidation,
                CreatingAndUsingValidatorsDirectly,
                ValidatingParametersInAWCFService).Run();
        }

        [Description("Using a Validation Rule Set to Validate an Object")]
        static void UsingAValidationRuleSetToValidateAnObject()
        {
            // Create a Product instance that contains invalid values.
            Product myProduct = new Product();
            PopulateInvalidProduct(myProduct);
            Console.WriteLine("Created and populated an invalid instance of the Product class.");
            Console.WriteLine();
            // Create a type validator for this type, specifying the rule set to use.
            Validator<Product> productValidator = ValidationFactory.CreateValidator<Product>("MyRuleset");
            // Validate the Product instance to obtain a collection of validation errors.
            ValidationResults results = productValidator.Validate(myProduct);
            // Display the contents of the validation errors collection.
            ShowValidationResults(results);
        }

        [Description("Validating a Collection of Objects")]
        static void ValidatingACollectionOfObjects()
        {
            // Create a collection of two objects, one with valid values and one with invalid values.
            List<IProduct> productList = new List<IProduct>();
            Product validProduct = new Product();
            PopulateValidProduct(validProduct);
            productList.Add(validProduct);
            Product invalidProduct = new Product();
            PopulateInvalidProduct(invalidProduct);
            productList.Add(invalidProduct);
            Console.WriteLine("Created and populated a collection of the Product class.");
            // Create an Object Collection Validator for the collection type. 
            Validator collValidator = new ObjectCollectionValidator(typeof(Product));
            // Validate all of the objects in the collection.
            ValidationResults results = collValidator.Validate(productList);
            // Display the results. 
            ShowValidationResults(results);
        }

        [Description("Using Validation Attributes and Self Validation")]
        static void UsingValidationAttributesAndSelfValidation()
        {
            // Create and populate a product instance with invalid values.
            IProduct invalidProduct = new AttributedProduct();
            PopulateInvalidProduct(invalidProduct);
            Console.WriteLine("Created and populated an invalid instance of the AttributedProduct class.");
            // Create a validator for this type. Must use the actual product type.
            // This will use the default rule set unless one is specified as the parameter.
            Validator<AttributedProduct> productValidator = ValidationFactory.CreateValidator<AttributedProduct>();

            // Validate the instance to obtain a collection of validation errors.
            ValidationResults results = productValidator.Validate(invalidProduct);
            // Alternatively, you could create and execute an ObjectValidator directly using:
            //   ValidationResults results = new ObjectValidator().Validate(validProduct);
            // Now display the contents of the validation errors collection.
            ShowValidationResults(results);
        }

        [Description("Using Data Annotation Attributes and Self Validation")]
        static void UsingDataAnnotationsValidationAttributesAndSelfValidation()
        {
            // Create and populate a product instance with invalid values.
            IProduct invalidProduct = new AnnotatedProduct();
            PopulateInvalidProduct(invalidProduct);
            Console.WriteLine("Created and populated an invalid instance of the AnnotatedProduct class.");
            // Create a validator for this type. Can use the interface type or the actual product type.
            Validator<AnnotatedProduct> productValidator = ValidationFactory.CreateValidator<AnnotatedProduct>();
            // Validate the instance to obtain a collection of validation errors.
            ValidationResults results = productValidator.Validate(invalidProduct);
            // Display the contents of the validation errors collection.
            ShowValidationResults(results);
        }

        [Description("Creating and Using Validators Directly")]
        static void CreatingAndUsingValidatorsDirectly()
        {
            // Create a Contains Characters Validator and use it to validate a String value.
            Validator charsValidator = new ContainsCharactersValidator("cat", ContainsCharacters.All,
                                                                         "Value must contain {4} of the characters '{3}'.");
            Console.WriteLine("Validating a string value using a Contains Characters Validator...");
            charsValidator.Tag = "Validating the String value 'disconnected'";
            // This overload of the Validate method returns a new ValidationResults 
            // instance populated with any/all of the validation errors.
            ValidationResults valResults = charsValidator.Validate("disconnected");
            // Create a Domain Validator and use it to validate an Integer value.
            Validator integerValidator = new DomainValidator<int>("Value must be in the list 1, 3, 7, 11, 13.",
                                                                     new int[] { 1, 3, 7, 11, 13 });
            integerValidator.Tag = "Validating the Integer value '42'";
            Console.WriteLine("Validating an integer value using a Domain Validator...");
            // This overload of the Validate method takes an existing ValidationResults 
            // instance and adds any/all of the validation errors to it.
            integerValidator.Validate(42, valResults);
            // Create an Or Composite Validator containing two validators.
            // Note that the NotNullValidator is negated to allow NULL values.
            Validator[] valArray = new Validator[] { 
				      new NotNullValidator(true, "Value can be NULL."),
				      new StringLengthValidator(5, RangeBoundaryType.Inclusive, 5, RangeBoundaryType.Inclusive, 
						                        "Value must be between {3} ({4}) and {5} ({6}) chars.")
				    };
            Validator orValidator = new OrCompositeValidator("Value can be NULL or a string of 5 characters.", valArray);
            // Validate two strings using the Or Composite Validator.
            Console.WriteLine("Validating a NULL value using an Or Composite Validator...");
            orValidator.Validate(null, valResults);  // this will not cause a validation error
            Console.WriteLine("Validating a string value using an Or Composite Validator...");
            orValidator.Validate("MoreThan5Chars", valResults);
            // Validate a single property of an existing class instance.
            // First create a Product instance with an invalid ID value.
            IProduct productWithID = new Product();
            PopulateInvalidProduct(productWithID);
            // Create a Property Value Validator that will use a RegexValidator
            // to validate the property value.
            Validator propValidator = new PropertyValueValidator<Product>("ID",
                                new RegexValidator("[A-Z]{2}[0-9]{4}", "Product ID must be 2 capital letters and 4 numbers."));
            Console.WriteLine("Validating one property of an object using a Property Value Validator...");
            propValidator.Validate(productWithID, valResults);
            // Now display the results of all the previous validation operations.
            ShowValidationResults(valResults);
        }

        [Description("Validating Parameters in a WCF Service")]
        static void ValidatingParametersInAWCFService()
        {
            bool success;
            // Create a client to access the ProductService WCF service.
            ProductService.IProductService svc = new ProductService.ProductServiceClient();
            Console.WriteLine("Created a client to access the ProductService WCF service.");
            Console.WriteLine();
            // Call the method, which returns success or failure, to add a valid product.
            IProduct validProduct = new Product();
            PopulateValidProduct(validProduct);
            try
            {
                Console.WriteLine("Adding a valid product using the ProductService... ");
                success = svc.AddNewProduct(validProduct.ID, validProduct.Name, validProduct.Description,
                                              validProduct.ProductType, validProduct.InStock,
                                                                        validProduct.OnOrder, validProduct.DateDue);
                Console.WriteLine("Successful: {0}", success);
            }
            catch (FaultException<ValidationFault> ex)
            {
                // Validation of the Product instance failed within the service interface.
                Console.WriteLine("Validation within the ProductService failed.");
                // Convert the validation details in the exception to individual
                // ValidationResult instances and add them to the collection.
                ValidationResults results = ConvertToValidationResults(ex.Detail.Details, validProduct);
                // Display information about the validation errors
                ShowValidationResults(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while calling service method: {0}", ex.Message);
            }
            Console.WriteLine();
            // Now call the same method to add an invalid product.
            IProduct invalidProduct = new Product();
            PopulateInvalidProduct(invalidProduct);
            try
            {
                Console.WriteLine("Adding an invalid product using the ProductService... ");
                success = svc.AddNewProduct(invalidProduct.ID, invalidProduct.Name, invalidProduct.Description,
                                              invalidProduct.ProductType, invalidProduct.InStock,
                                                                        invalidProduct.OnOrder, invalidProduct.DateDue);
                Console.WriteLine("Added an invalid product using the ProductService. Successful: {0}", success);
            }
            catch (FaultException<ValidationFault> ex)
            {
                // Validation of the Product instance failed within the service interface.
                Console.WriteLine("Validation within the ProductService failed.");
                // Convert the validation details in the exception to individual
                // ValidationResult instances and add them to the collection.
                ValidationResults results = ConvertToValidationResults(ex.Detail.Details, invalidProduct);
                // Display information about the validation errors
                ShowValidationResults(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while calling service method: {0}", ex.Message);
            }
        }


        #region Auxiliary routines

        static void PopulateValidProduct(IProduct prod)
        {
            prod.ID = "AA1234";
            prod.Name = "A Valid Product";
            prod.Description = "Something to keep the grandchildren quiet";
            prod.ProductType = "FunThings";
            prod.InStock = 3;
            prod.OnOrder = 12;
            prod.DateDue = DateTime.Now.AddMonths(2);
        }

        static void PopulateInvalidProduct(IProduct prod)
        {
            prod.ID = "42";
            prod.Name = "An Invalid Product";
            prod.Description = "-";
            prod.ProductType = "FurryThings";
            prod.InStock = -3;
            prod.OnOrder = 9000;
            prod.DateDue = DateTime.Now.AddMonths(9);
        }

        static void ShowValidationResults(ValidationResults results)
        {
            // Check if the ValidationResults detected any validation errors.
            if (results.IsValid)
            {
                Console.WriteLine("There were no validation errors.");
            }
            else
            {
                Console.WriteLine("The following {0} validation errors were detected:", results.Count);
                // Iterate through the collection of validation results.
                foreach (ValidationResult item in results)
                {
                    // Show the target member name and current value.
                    Console.WriteLine("+ Target object: {0}, Member: {1}", GetTypeNameOnly(item.Target), item.Key);
                    // Display details of this validation error.
                    ShowValidatorDetails(item, "  ");
                }
            }
            Console.WriteLine();
        }

        static void ShowValidatorDetails(ValidationResult item, string indent)
        {
            // Display the values of the properties of this validator.
            Console.WriteLine("{0}- Detected by: {1}", indent, GetTypeNameOnly(item.Validator));
            if (null != item.Tag)
            {
                Console.WriteLine("{0}- Tag value: {1}", indent, item.Tag);
            }
            // Split the value of the Message property into the value and the message.
            // Only required here because we may include the [{0}] token at the start
            // of the Message string so that the validated value can be displayed here.
            string theMessage = item.Message;
            string theValue = String.Empty;
            int endPosition = theMessage.IndexOf(']');
            if (endPosition > 0)
            {
                int startPosition = theMessage.IndexOf('[') + 1;
                theValue = theMessage.Substring(startPosition, endPosition - startPosition);
                theMessage = theMessage.Substring(endPosition + 1);
                // Show the value - it's a good idea to encode it for display.
                Console.WriteLine("{0}- Validated value was: '{1}'", indent, System.Web.HttpUtility.HtmlEncode(theValue));
            }
            Console.WriteLine("{0}- Message: '{1}'", indent, theMessage);
            // See if this is a composite validator - if so, iterate the nested validators.
            // Note that there may be multiple levels of nesting, so use recursion.
            if (item.NestedValidationResults.Count() > 0)
            {
                Console.WriteLine("{0}+ Nested validators:", indent, item.Message);
                foreach (ValidationResult nestedItem in item.NestedValidationResults)
                {
                    ShowValidatorDetails(nestedItem, indent + "  ");
                }
            }
        }

        static ValidationResults ConvertToValidationResults(ValidationDetail[] results, object target)
        {
            // Convert the validation details in the exception to individual
            // ValidationResult instances and add them to the collection.
            ValidationResults adaptedResults = new ValidationResults();
            foreach (ValidationDetail result in results)
            {
                adaptedResults.AddResult(new ValidationResult(result.Message, target, result.Key, result.Tag, null));
            }
            return adaptedResults;
        }

        static string GetTypeNameOnly(object item)
        {
            if (null != item)
            {
                string typeString = item.ToString();
                if (typeString.Contains('['))
                {
                    // Type is a generic with a specified type enclosed in square brackets.
                    string matchString = typeString.Substring(0, typeString.LastIndexOf("["));
                    return typeString.Substring(matchString.LastIndexOf(".") + 1);
                }
                else
                {
                    return typeString.Substring(typeString.LastIndexOf(".") + 1);
                }
            }
            else
            {
                return "[none]";
            }
        }

        #endregion
    }
}
