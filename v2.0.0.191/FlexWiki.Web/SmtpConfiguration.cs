using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Web
{
    public class SmtpConfiguration
    {
        private string _password;
        private string _server;
        private string _user; 

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }
        public string User
        {
            get { return _user; }
            set { _user = value; }
        }

    }
}
