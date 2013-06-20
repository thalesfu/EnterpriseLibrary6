Notes for using this sample:

Setup
=====
You will need access to an SMTP server to be able to send email messages from the EmailSink.
One option is to to use smtp.live.com. The sample uses credentials from the Windows
Credential Store to access the SMTP service. By default the name of the credential
is "etw."

To build the solution and run the out-of-proc scenario, you will need to download
and install the Semantic Logging Application Block out-of-process Windows Service from
http://go.microsoft.com/fwlink/p/?LinkID=290903
The download includes installation instructions for this service.

Build
=====
The solution uses NuGet to add the Semantic Logging Application Block package to
the projects.
In the CustomSinkExtension and CustomTextFormatter projects, add a reference to the
Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Etw assembly that was included
in the Semantic Logging Application Block out-of-process Windows Service download.
This additional reference is only needed when you create a class that implements
ISinkElement to provide support for a custom schema for the sink when configuring 
the out-of-process Windows Service. You would not need this reference if you don't
require that kind of schema support.


In-Proc Scenario
================
To run the in-proc scenario, run the InProcConsoleApp. You will need to modify
the configuration settings in Program.cs.


Out-of-Proc Scenario
====================
To run the out-of-proc scenario, copy the contents of the OutOfProcHost folder
(CustomSinkExtension.dll, CustomTextFormatter.dll, EmailSinkElement.xsd,
PrefixEventTextFormatterElement.xsd, and SemanticLogging-svc.xml)
in the solution folder to the folder where you installed the Semantic Logging
Application Block out-of-process Windows Service replacing the existing
SemanticLogging-svc.xml file. You will need to edit the configuration settings in the
SemanticLogging-svc.xml file to select your formatter and to configure your
SMTP service settings. See the comments in the file for more information.

Run the Semantic Logging Application Block out-of-process Windows Service in 
console mode to observe the events in the console.

Then run the EventProducerConsole application. 