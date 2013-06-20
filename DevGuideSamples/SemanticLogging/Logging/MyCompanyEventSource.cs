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
using System.Diagnostics.Tracing;

namespace SemanticLoggingExample
{
    public enum MyColor { Red, Yellow, Blue };

    [EventSource(Name = "MyCompany")]
    public class MyCompanyEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        public class Tasks
        {
            public const EventTask Page = (EventTask)1;
            public const EventTask DBQuery = (EventTask)2;
        }

        [Event(1, Message = "Application Failure: {0}", Level = EventLevel.Critical, Keywords = Keywords.Diagnostic)]
        internal void Failure(string message) { this.WriteEvent(1, message); }

        [Event(2, Message = "Starting up.", Keywords = Keywords.Perf, Level = EventLevel.Informational)]
        internal void Startup() { this.WriteEvent(2); }

        [Event(3, Message = "loading page {1} activityID={0}", Opcode = EventOpcode.Start,
            Task = Tasks.Page, Keywords = Keywords.Page, Level = EventLevel.Informational)]
        internal void PageStart(int ID, string url) { if (this.IsEnabled()) this.WriteEvent(3, ID, url); }

        [Event(4, Opcode = EventOpcode.Stop, Task = Tasks.Page, Keywords = Keywords.Page, Level = EventLevel.Warning)]
        internal void PageStop(int ID) { this.WriteEvent(4, ID); }

        [Event(5, Opcode = EventOpcode.Start, Task = Tasks.DBQuery, Keywords = Keywords.DataBase, Level = EventLevel.Informational)]
        internal void DBQueryStart(string sqlQuery) { this.WriteEvent(5, sqlQuery); }

        [Event(6, Opcode = EventOpcode.Stop, Task = Tasks.DBQuery, Keywords = Keywords.DataBase, Level = EventLevel.Informational)]
        internal void DBQueryStop() { this.WriteEvent(6); }

        [Event(7, Level = EventLevel.Verbose, Keywords = Keywords.DataBase)]
        internal void Mark(int ID) { this.WriteEvent(7, ID); }

        [Event(8, Level = EventLevel.Error)]
        internal void LogColor(MyColor color) { this.WriteEvent(8, (int)color); }

        [Event(9, Opcode = EventOpcode.Start)]
        internal void WithOpcodeAndNoTaskSpecfied(int arg1) { this.WriteEvent(9, arg1); }

        [Event(10, Level = EventLevel.Error, Message = "Error in DBQuery: {0}")]
        internal void DBQueryError(int arg1) { this.WriteEvent(10, arg1); }

        [Event(11, Level = EventLevel.Error, Message = "UI Error - Exception: {0}, Screen ID: {1}, User ID: {2}, OS: {3}")]
        internal void UIError(string message, int screenID, int userID, string OSName) { if (this.IsEnabled()) this.WriteEvent(11, message, screenID, userID, OSName); }


        private static readonly Lazy<MyCompanyEventSource> Instance = new Lazy<MyCompanyEventSource>(() => new MyCompanyEventSource());

        private MyCompanyEventSource()
        {
        }

        public static MyCompanyEventSource Log
        {
            get { return Instance.Value; }
        }

    }
}
