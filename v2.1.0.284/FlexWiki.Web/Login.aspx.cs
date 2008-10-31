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
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration; 
using System.Web.SessionState;
using System.Web.Security;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public partial class Login : BasePage
	{
        protected System.Web.UI.WebControls.Login Login1;
        protected System.Web.UI.WebControls.HyperLink ReturnLink;

        private bool endProcess = false;
        private bool templatedPage = false;
        private string template;
        private int lineCnt;

        protected void Login1_LoggedIn(object sender, EventArgs a)
        {
            FlexWikiWebApplication.LogInfo(typeof(Login).ToString(), "Forms user logged in: " +
                Login1.UserName); 
        }
        protected void Login1_LoginError(object sender, EventArgs a)
        {
            // There doesn't seem to be an easy way to figure out what exactly went wrong
            FlexWikiWebApplication.LogWarning(typeof(Login).ToString(), "Forms user was unable to log in: " +
                Login1.UserName); 
        }
		private void Page_Load(object sender, System.EventArgs e)
		{
            AuthenticationSection section = 
                WebConfigurationManager.GetWebApplicationSection("system.web/authentication") as AuthenticationSection;

            //clear the existing session id cookie when the user logs in to eliminate XSS vulnerability
            if (!Page.User.Identity.IsAuthenticated)
            {
                if (Page.Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddYears(-30);
                }
                Session.Abandon();
            }

            if (section != null)
            {
                // If we're set up for Windows authentication, let the web server and the 
                // browser handle it. Otherwise we'll let page execution continue, which 
                // uses Forms authentication. 
                if (section.Mode == AuthenticationMode.Windows)
                {
                    string user = Request.ServerVariables["LOGON_USER"];
                    if (string.IsNullOrEmpty(user))
                    {
                        // The browser won't automatically resubmit the authentication on 
                        // subsequent requests, so we need to force it by manually returning
                        // a 401 on every request that isn't authenticated. We do so by 
                        // setting this cookie, which is detected in Application_BeginRequest
                        Response.Clear();
                        HttpCookie cookie = new HttpCookie(FlexWikiWebApplication.ForceWindowsAuthenticationCookieName, "true");
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie); 
                        Response.StatusCode = 401;
                        Response.StatusDescription = "Unauthorized";
                        Response.End();
                    }
                    else
                    {
                        FlexWikiWebApplication.LogInfo(typeof(Login).ToString(), "Windows user logged in: " +
                            user);
                        Login1.Visible = false;
                        ReturnLink.Visible = true;
                        ReturnLink.NavigateUrl = Request.QueryString["ReturnURL"];
                        ReturnLink.Text = string.Format("You have been logged in as {0}. Click here to return to the wiki.",
                            user); 

                        //Response.Redirect(Request.QueryString["ReturnURL"]); 
                    }
                }
            }
		}

        protected string BuildPageOne()
        {

            StringBuilder strOutput = new StringBuilder();

            string overrideBordersScope = "None";
            template = "";

            if (!String.IsNullOrEmpty(WikiApplication.ApplicationConfiguration.OverrideBordersScope))
            {
                overrideBordersScope = WikiApplication.ApplicationConfiguration.OverrideBordersScope;
            }
            if (!String.IsNullOrEmpty(overrideBordersScope))
            {
                template = PageUtilities.GetOverrideBordersContent(manager, overrideBordersScope);
            }
            if (!String.IsNullOrEmpty(template))  // page built using template
            {

                    SetBorderFlags(template);
                    templatedPage = true;

                    bool startProcess = false;
                    foreach (string s in template.Split(new char[] { '\n' }))
                    {
                        if (!startProcess)
                        {
                            lineCnt++;
                            if (s.Contains("</title>")) //ignore input until after tag </title>
                            {
                                startProcess = true;
                            }
                        }
                        else
                        {
                            if (!endProcess)
                            {
                                strOutput.Append(DoTemplatedPageOne(s.Trim()));
                            }
                        }
                    }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePageOne());
            }
            strOutput.AppendLine("<div id=\"TopicBody\">");
            return strOutput.ToString();
        }
        protected string BuildPageTwo()
        {

            StringBuilder strOutput = new StringBuilder();

            if (templatedPage)  // page built using template
            {
                if (!String.IsNullOrEmpty(template))
                {
                    int count = 0;

                    foreach (string s in template.Split(new char[] { '\n' }))
                    {
                        count++;
                        if (count >= lineCnt)
                        {
                            strOutput.Append(DoTemplatedPageTwo(s.Trim()));
                        }
                    }
                }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePageTwo());
            }
            return strOutput.ToString();
        }
        protected string DoNonTemplatePageOne()
        {
            StringBuilder strOutput = new StringBuilder();
            _javaScript = true;
            _metaTags = true;

            InitBorders();
            strOutput.AppendLine(InsertStylesheetReferences());
            strOutput.AppendLine(InsertFavicon());
            strOutput.AppendLine("</head>");
            strOutput.AppendLine("<body class=\"UserInfo\">");

            strOutput.Append(InsertLeftTopBorders());

            return strOutput.ToString();

        }
        protected string DoNonTemplatePageTwo()
        {
            StringBuilder strOutput = new StringBuilder();

            strOutput.AppendLine(InsertRightBottomBorders());

            strOutput.AppendLine("</body>");
            strOutput.AppendLine("</html>");
            return strOutput.ToString();

        }
        protected string DoTemplatedPageOne(string s)
        {
            StringBuilder strOutput = new StringBuilder();

            MatchCollection lineMatches = dirInclude.Matches(s);
            string temp = s;
            lineCnt++;
            if (lineMatches.Count > 0)
            {
                int position;
                position = temp.IndexOf("{{");
                if (position > 0)
                {
                    strOutput.AppendLine(temp.Substring(0, position));
                }
                foreach (Match submatch in lineMatches)
                {
                    switch (submatch.ToString())
                    {
                        case "{{FlexWikiTopicBody}}":
                            //strOutput.AppendLine(DoPageImplementationOne());
                            endProcess = true;
                            return strOutput.ToString();

                        case "{{FlexWikiHeaderInfo}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            strOutput.AppendLine(InsertFavicon());
                            break;

                        case "{{FlexWikiMetaTags}}":
                            break;

                        case "{{FlexWikiJavaScript}}":
                            break;

                        case "{{FlexWikiCss}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            break;

                        case "{{FlexWikiFavIcon}}":
                            strOutput.AppendLine(InsertFavicon());
                            break;

                        case "{{FlexWikiTopBorder}}":
                            if (!String.IsNullOrEmpty(temptop))
                            {
                                strOutput.AppendLine(temptop.ToString());
                            }
                            break;

                        case "{{FlexWikiLeftBorder}}":
                            if (!String.IsNullOrEmpty(templeft))
                            {
                                strOutput.AppendLine(templeft.ToString());
                            }
                            break;

                        default:
                            break;
                    }
                    temp = temp.Substring(s.IndexOf("}}") + 2);
                }
                if (!String.IsNullOrEmpty(temp))
                {
                    if (!endProcess)
                    {
                        strOutput.AppendLine(temp);
                    }
                }
            }
            else
            {
                strOutput.AppendLine(s);
            }
            return strOutput.ToString();
        }
        protected string DoTemplatedPageTwo(string s)
        {
            StringBuilder strOutput = new StringBuilder();

            MatchCollection lineMatches = dirInclude.Matches(s);
            string temp = s;
            if (lineMatches.Count > 0)
            {
                int position;
                position = temp.IndexOf("{{");
                if (position > 0)
                {
                    strOutput.AppendLine(temp.Substring(0, position));
                }
                foreach (Match submatch in lineMatches)
                {
                    switch (submatch.ToString())
                    {
                        case "{{FlexWikiTopicBody}}":
                            //strOutput.AppendLine(DoPageImplementationTwo());
                            break;

                        case "{{FlexWikiRightBorder}}":
                            if (!String.IsNullOrEmpty(tempright))
                            {
                                strOutput.AppendLine(tempright.ToString());
                            }
                            break;

                        case "{{FlexWikiBottomBorder}}":
                            if (!String.IsNullOrEmpty(tempbottom))
                            {
                                strOutput.AppendLine(tempbottom.ToString());
                            }
                            break;


                        default:
                            break;
                    }
                    temp = temp.Substring(s.IndexOf("}}") + 2);
                }
                if (!String.IsNullOrEmpty(temp))
                {
                    strOutput.AppendLine(temp);
                }
            }
            else
            {
                strOutput.AppendLine(s);
            }
            return strOutput.ToString();
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

	}
}
