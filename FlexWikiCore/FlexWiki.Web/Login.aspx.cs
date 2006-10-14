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
using System.Web.SessionState;
using System.Web.Security;
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
	public class Login : BasePage
	{
		protected System.Web.UI.WebControls.TextBox userEmail;
		protected System.Web.UI.WebControls.TextBox userPassword;
		protected System.Web.UI.WebControls.CheckBox userPersist;
		protected System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;
		protected System.Web.UI.WebControls.RegularExpressionValidator RegexValidator;
		protected System.Web.UI.WebControls.TextBox userPass;
		protected System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator2;
		protected System.Web.UI.WebControls.Label Msg;
		protected System.Web.UI.WebControls.Button Button1;
		protected System.Web.UI.WebControls.Button RemindMeButton;
		protected System.Web.UI.HtmlControls.HtmlForm LogonForm;
		protected System.Web.UI.WebControls.Label userPasswordLabel;
		protected System.Web.UI.WebControls.Label userPersistLabel;
		protected System.Web.UI.WebControls.Label userEmailLabel;
		protected System.Web.UI.WebControls.Button logonButton;
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
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
			this.logonButton.Click += new System.EventHandler(this.logonButton_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void logonButton_Click(object sender, System.EventArgs e)
		{

		}

	}
}
