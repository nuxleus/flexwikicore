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
using System.Security.Principal;
using System.Threading; 
using System.Web;

namespace FlexWiki
{
    // This class is pretty busted - it is linked tightly with the ASP.NET infrastructure, 
    // when instead it should be getting that stuff from IFlexWikiApplication. But I'm not
    // going to fix it right now. 
	[ExposedClass("Request", "Provides information about the current user's request for this topic")]
	public class Request : BELObject
	{
		public Request()
		{
		}

		private HttpRequest HTTPRequest
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return null;
				return System.Web.HttpContext.Current.Request;
			}
		}



		private IPrincipal User
		{
			get
			{
                return Thread.CurrentPrincipal;
			}
		}
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the contents of the named cookie, null if absent")]
        public string GetCookie(string key)
        {
            if (HTTPRequest == null)
            {
                return null;
            }
            if (!String.IsNullOrEmpty(HTTPRequest.Cookies[key].Value))
            {
                return HTTPRequest.Cookies[key].Value;
            }
            else
            {
                return null;
            }
        }
		[ExposedMethod(ExposedMethodFlags.Default, "Answer the named parameter from the query string and form of the request; null if absent")]
		public string GetParameterNamed(string parm)
		{
			if (HTTPRequest == null)
				return null;
            if (!String.IsNullOrEmpty(HTTPRequest.QueryString[parm]))
            {
                return HTTPRequest.QueryString[parm];
            }
            else if (!String.IsNullOrEmpty(HTTPRequest.Form[parm]))
            {
                return HTTPRequest.Form[parm];
            }
            else
            {
                return null;
            }
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer an Array the names of all of the parameters in the query string and form")]
		public ArrayList ParameterNames
		{
			get
			{
				ArrayList answer = new ArrayList();
				if (HTTPRequest == null)
					return answer;
                foreach (string s in HTTPRequest.QueryString.Keys)
                {
                    answer.Add(s);
                }
                foreach (string s in HTTPRequest.Form.Keys)
                {
                    if (!answer.Contains(s))
                    {
                        answer.Add(s);
                    }
                }
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer an Array of VisitorEvents describing the current user's visits during the session")]
		public ArrayList VisitorEvents   
		{
			get
			{
				ArrayList answer = new ArrayList();
				IEnumerable events = (IEnumerable)(System.Web.HttpContext.Current.Session["VisitorEvents"]);
                if (events == null)
                {
                    return answer;
                }
                foreach (VisitorEvent each in events)
                {
                    answer.Add(each);
                }
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer an Array of Unique VisitorEvents describing the current user's visits during the session")]
		public ArrayList UniqueVisitorEvents
		{
			get
			{
				ArrayList answer = new ArrayList();
				System.Web.HttpContext ctx = System.Web.HttpContext.Current;
                if (ctx == null)
                {
                    return answer;
                }
				IEnumerable events = (IEnumerable)(ctx.Session["VisitorEvents"]);
                if (events == null)
                {
                    return answer;
                }
                Set<QualifiedTopicRevision> seen = new Set<QualifiedTopicRevision>();
				foreach (VisitorEvent currentEvent in events)
				{
                    if (seen.Contains(currentEvent.Topic))
                    {
                        continue;
                    }
					seen.Add(currentEvent.Topic);
					answer.Add(currentEvent);
				}
				return answer;
			}
		}
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the URL of the HttpRequest")]
        public string Url
        {
            get
            {
                if (HTTPRequest == null)
                {
                    return "";
                }
                return (string) (HTTPRequest.Url.ToString());
            }
        }
		[ExposedMethod(ExposedMethodFlags.Default, "Answer a string identifying the visitor (not authenticated)")]
		public string VisitorIdentityString
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return "";
				return (string)(System.Web.HttpContext.Current.Items["VisitorIdentityString"]);
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a string identifying the authenticated identity of the user (or null if none)")]
		public string AuthenticatedUserName
		{
			get
			{
				if (User == null)
					return null;
				if (!User.Identity.IsAuthenticated)
					return null;
				return User.Identity.Name;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true if the current user is authenticated")]
		public bool IsAuthenticated
		{
			get
			{
				if (User == null)
					return false;
				return User.Identity.IsAuthenticated;
			}
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer true if the user can log in and out; else false")]
		public bool CanLogInAndOut
		{
			get
			{
				// The next line is designed to hide the loggoff command if the user is logged in via NTLM; 
				// I'm not 100% sure the test is right
				return ("Negotiate" != User.Identity.AuthenticationType);
			}
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer true if differences are being shown; else false")]
		public bool AreDifferencesShown
		{
			get
			{
				if (HTTPRequest == null)
					return false;
				return HTTPRequest.QueryString["diff"] == "y";
			}
		}

	}
}
