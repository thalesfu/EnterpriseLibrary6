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
using System.ComponentModel;
using System.Linq;

namespace DevGuideExample.MenuSystem
{
    internal class DelegateMenuOption : MenuOption
    {
        private readonly Action optionCode;
        private readonly string optionText;

        public DelegateMenuOption(Action optionCode)
        {
            this.optionCode = optionCode;
            optionText = GetDescriptionFromOptionCodeDelegate();
        }

        public override string Text
        {
            get { return optionText; }
        }

        protected override void DoExecute()
        {
            optionCode();
        }

        /// <summary>
        /// Pull the description off attributes on the delegate passed in.
        /// This only works if you pass in actual methods, but that's ok
        /// for our purposes.
        /// </summary>
        /// <returns>Description text to display.</returns>
        private string GetDescriptionFromOptionCodeDelegate()
        {
            DescriptionAttribute description =
                optionCode.Method.GetCustomAttributes(typeof (DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>().FirstOrDefault();
            if (description == null)
            {
                return "No description present";
            }
            return description.Description;
        }
    }
}
