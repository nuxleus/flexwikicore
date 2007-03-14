#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.IO;
using System.Net; 
using System.Net.Mail;
using System.Text;

namespace FlexWiki.Web.Newsletters
{
    internal class DaemonBasedDeliveryBoy : IDeliveryBoy
    {
        private IWikiApplication _application;
        //private StringWriter _log;
        private string _smtpHost;
        private string _smtpUser;
        private string _smtpPassword;
        private bool _sendAsAttachments;

        internal DaemonBasedDeliveryBoy(string smtpHost, string smtpUser, string smtpPassword, IWikiApplication application)
            : this(smtpHost, smtpUser, smtpPassword, false, application)
        {
        }

        internal DaemonBasedDeliveryBoy(string smtpHost, string smtpUser, string smtpPassword, bool sendAsAttachments, 
            IWikiApplication application)
        {
            _smtpHost = smtpHost;
            _smtpUser = smtpUser;
            _smtpPassword = smtpPassword;
            _sendAsAttachments = sendAsAttachments;
            _application = application; 
        }



        //public StringWriter Log
        //{
        //  get { return _log; }
        //  set { _log = value; }
        //}

        public bool Deliver(string to, string from, string subject, string body)
        {
            //SmtpMail mailer = new SmtpMail();
            //StringWriter smtpLog = new StringWriter(new StringBuilder());
            //mailer.sw = smtpLog;
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            if (_sendAsAttachments)
            {
                message.Body = "The wiki has been updated. See the attached files for changes.";
                MemoryStream memoryStream = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
                streamWriter.Write(body);
                streamWriter.Flush();
                streamWriter.Close();
                memoryStream.Position = 0;
                Attachment attachment = new Attachment(memoryStream, "Changes.htm");
                message.Attachments.Add(attachment);
            }
            else
            {
                message.Body = body;
                message.IsBodyHtml = true;
            }

            try
            {
                SmtpClient client = new SmtpClient(_smtpHost);
                if (_smtpPassword != null)
                {
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPassword); 
                }
                client.Send(message);
                return true; 
            }
            catch (Exception e)
            {
                _application.LogError(this.GetType().ToString(), 
                    "Exception while sending to host '" + _smtpHost + "': " + e.ToString());
                return false; 
            }
        }
    }
}
