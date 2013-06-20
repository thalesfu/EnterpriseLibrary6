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
using System.Collections;
using System.Collections.Generic;

namespace DevGuideExample.MenuSystem
{
    /// <summary>
    /// A tiny "application framework" to run the menus for
    /// each of the dev guide examples.
    /// </summary>
    class MenuDrivenApplication : IEnumerable<MenuOption>
    {
        private readonly string introText;
        public static readonly string Underline = new string('-', 79);
        private readonly List<MenuOption> menuOptions = new List<MenuOption>();
        private bool shouldQuit;

        public MenuDrivenApplication(string introText)
        {
            this.introText = introText;
        }

        public MenuDrivenApplication(string introText, params Action[] options)
            : this(introText)
        {
            foreach(var option in options)
            {
                Add(option);
            }
        }

        public void Add(MenuOption option)
        {
            menuOptions.Add(option);
        }

        public void Add(Action optionAction)
        {
            menuOptions.Add(new DelegateMenuOption(optionAction));
        }

        public IEnumerator<MenuOption> GetEnumerator()
        {
            return menuOptions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Run()
        {
            while(!shouldQuit)
            {
                DisplayMenu();
                DoUserSelection();
                PromptToContinue();
            }
        }

        private void DisplayMenu()
        {
            Console.Clear();
            WriteMenuHeader();
            
            for(int i = 0; i < menuOptions.Count; ++i)
            {
                WriteMenuOption(i);
            }

            WriteMenuFooter();
        }

        private void WriteMenuHeader()
        {
            Console.WriteLine(Underline);
            Console.WriteLine(introText);
            Console.WriteLine(Underline);
        }

        private void WriteMenuOption(int index)
        {
            Console.WriteLine("[{0}] {1}",
                Convert.ToChar(index + 65),
                menuOptions[index].Text);
        }

        private static void WriteMenuFooter()
        {
            Console.WriteLine();
            Console.Write("> Select option or [ESC] to quit...");
        }

        private void DoUserSelection()
        {
            int selectedOption = ReadValidSelectedOptionFromUser();
            if(selectedOption != -1)
            {
                Console.Clear();
                menuOptions[selectedOption].Execute();
            }
        }

        private int ReadValidSelectedOptionFromUser()
        {
            int selectedOptionIndex = -1;
            while(!shouldQuit && !SelectedOptionInValidRange(selectedOptionIndex))
            {
                selectedOptionIndex = ReadSelectedOptionFromUser();
            }
            return selectedOptionIndex;
        }

        private int ReadSelectedOptionFromUser()
        {
            var key = Console.ReadKey(true);
            if(key.Key == ConsoleKey.Escape)
            {
                shouldQuit = true;
                return -1;
            }
            return (int) key.Key - (int) ConsoleKey.A;
        }

        private bool SelectedOptionInValidRange(int selectedOptionIndex)
        {
            return selectedOptionIndex >= 0 && selectedOptionIndex < menuOptions.Count;
        }

        private void PromptToContinue()
        {
            if(!shouldQuit)
            {
                Console.WriteLine();
                Console.Write("Press any key to continue or [ESC] to quit...");
                var key = Console.ReadKey(true);
                if(key.Key == ConsoleKey.Escape)
                {
                    shouldQuit = true;
                }
            }
        }
    }
}
