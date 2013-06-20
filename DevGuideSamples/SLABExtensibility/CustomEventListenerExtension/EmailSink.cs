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

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CustomSinkExtension
{
    public sealed class EmailSink : IObserver<EventEntry>
    {
        private const string DefaultSubject = "Email Sink Extension";
        private IEventTextFormatter formatter;
        private MailAddress sender;
        private MailAddressCollection recipients = new MailAddressCollection();
        private string subject;
        private string host;
        private int port;
        private NetworkCredential credentials;

        public EmailSink(string host, int port, 
            string recipients, string subject, string credentials, IEventTextFormatter formatter)
        {
            this.formatter = formatter ?? new EventTextFormatter();
            this.host = host;
            this.port = port;
            this.credentials = CredentialManager.GetCredentials(credentials);
            this.sender = new MailAddress(this.credentials.UserName);
            this.recipients.Add(recipients);
            this.subject = subject ?? DefaultSubject;
        }

        public void OnNext(EventEntry entry)
        {
          if (entry != null)
          {
            using (var writer = new StringWriter())
            {
              this.formatter.WriteEvent(entry, writer);
              Post(writer.ToString());
            }
          }
        }

        private async void Post(string body)
        {
            using (var client = new SmtpClient(this.host, this.port) { Credentials = this.credentials, EnableSsl = true })
            using (var message = new MailMessage(this.sender, this.recipients[0]) { Body = body, Subject = this.subject })
            {
                for (int i = 1; i < this.recipients.Count; i++) message.CC.Add(this.recipients[i]);
                client.SendCompleted += (o, e) => Trace.WriteIf(e.Error != null, e.Error);
                await client.SendMailAsync(message).ConfigureAwait(false);
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }
}
