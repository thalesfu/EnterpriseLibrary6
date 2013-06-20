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

namespace DevGuideExample.MenuSystem
{
    /// <summary>
    /// A class that encapsulates the behavior of a single menu item,
    /// and is responsible for 
    /// </summary>
    abstract class MenuOption
    {
        /// <summary>
        /// Text to display in the menu.
        /// </summary>
        public abstract string Text { get; }

        /// <summary>
        /// Execute the menu option
        /// </summary>
        public void Execute()
        {
            WriteExampleHeader();
            try
            {
                DoExecute();
            }
            catch(Exception ex)
            {
                ShowExceptionDetails(ex);
            }
        }

        /// <summary>
        /// Execute the actual operation.
        /// </summary>
        protected abstract void DoExecute();

        private void WriteExampleHeader()
        {
            Console.WriteLine(MenuDrivenApplication.Underline);
            Console.WriteLine(Text);
            Console.WriteLine(MenuDrivenApplication.Underline);
        }

        public static void ShowExceptionDetails(Exception ex)
        {
            Console.WriteLine("Exception type {0} was thrown.", ex.GetType().ToString());
            Console.WriteLine("Message: '{0}'", ex.Message);
            Console.WriteLine("Source: '{0}'", ex.Source);
            if (null == ex.InnerException)
            {
                Console.WriteLine("No Inner Exception");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Inner Exception: {0}", ex.InnerException.ToString());
            }
            Console.WriteLine();
        }
    }
}
