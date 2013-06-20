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

namespace EventProducerConsole
{
    class Program
    {
        static void Main(string[] args)
        {            
            var logger = MyCompanyEventSource.Log;
            
            var rnd = new Random();

            Console.WriteLine("Press enter key to start logging events...");
            Console.ReadLine();
            ConsoleKey readKey = ConsoleKey.NoName;

            while (readKey != ConsoleKey.Spacebar)
            {
                Console.WriteLine();
                
                logger.Startup();
                Console.WriteLine("Event {0}", "Startup");

                int v = rnd.Next();
                logger.PageStart(v, "page start");
                Console.WriteLine("Event {0} values: {1}, {2}", "PageStart", v, "page start");

                v = rnd.Next();
                logger.PageStop(v);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Event {0} value: {1}", "PageStop", v);
                Console.ResetColor();

                logger.LogColor(MyColor.Red);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Event {0}, value: {1}", "LogColor", MyColor.Red);
                Console.ResetColor();

                logger.Failure("All servers are unreachable, general network failure!!");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Event {0}, value: {1}", "Failure", "All servers are unreachable, general network failure!!");
                Console.ResetColor();

                Console.WriteLine("Press space to stop logging events or any key for logging next event ...");

                readKey = Console.ReadKey().Key;
            }
        }
    }
}
