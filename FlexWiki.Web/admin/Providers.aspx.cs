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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki;
using System.Configuration;
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class Providers : AdminPage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
		}

		protected override void DefaultPageLoad()  
		{
			MinimalPageLoad();
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

		protected void ShowPage()
		{
			UIResponse.ShowPage("Namespace Providers", new UIResponse.MenuWriter(ShowMenu), new UIResponse.BodyWriter(ShowProviders));
		}

        private NamespaceProviderParameter GetParameter(ArrayList arrayList, string name)
        {
            foreach (NamespaceProviderParameter parameter in arrayList)
            {
                if (parameter.Name.Equals(name))
                {
                    return parameter;
                }
            }

            return null;
        }
		// Answer an array of arrays.  Each inner array has a collection of NamespaceProviderDefinitions with the same type
		private List<List<NamespaceProviderDefinition>> ProvidersByType
		{
			get
			{
				Dictionary<string, List<NamespaceProviderDefinition>> collector = 
                    new Dictionary<string, List<NamespaceProviderDefinition>>();	
				FederationConfiguration fc = Federation.Application.FederationConfiguration;
				if (fc != null)
				{
					foreach (NamespaceProviderDefinition each in fc.NamespaceMappings)
					{
						string key = each.Type;
                        List<NamespaceProviderDefinition> list; 
                        if (collector.ContainsKey(key))
                        {
                            list = collector[key]; 
                        }
						else 
						{
							list = new List<NamespaceProviderDefinition>();
							collector[key] = list;
						}
						list.Add(each);
					}
				}
				List<List<NamespaceProviderDefinition>> answer = new List<List<NamespaceProviderDefinition>>();
				List<string> keys = new List<string>();
				keys.AddRange(collector.Keys);
				keys.Sort();
				foreach (string k in keys)
                {
					answer.Add(collector[k]);
                }

				return answer;
			}
		}
        private void ShowMenu()
        {
            UIResponse.WriteStartMenu("Providers");
            UIResponse.WriteMenuItem("EditProvider.aspx", "Add provider", "Add a new provider instance");
            UIResponse.WriteEndMenu();
            UIResponse.WritePara("&nbsp;");

            ShowAdminMenu();
        }
		private void ShowProviders()
		{

			foreach (List<NamespaceProviderDefinition> each in ProvidersByType)
			{
				// Lay down the header
				NamespaceProviderDefinition first = each[0];
				string s = first.Type;
                if (first.AssemblyName != null)
                {
                    s += " (in " + first.AssemblyName + ")";
                }
				UIResponse.WriteHeading(UIResponse.Escape(s), 1);

				UITable table = new UITable();
				table.AddColumn(new UIColumn()); // edit link
                List<string> columns = new List<string>(); 
                foreach (NamespaceProviderParameter parm in first.Parameters)
                {
                    string column = parm.Name;
                    columns.Add(column); 
                    table.AddColumn(new UIColumn(column));
                }

				UIResponse.WriteStartTable(table);
				foreach (NamespaceProviderDefinition inner in each)
				{
					UIResponse.WriteStartRow();
					UIResponse.WriteCell(UIResponse.CommandLink("EditProvider.aspx?Provider=" + inner.Id, Command.Edit, "edit this provider's information"));
                    // Parameters might be listed out of order, so we have to look through
                    // them all to get the right one. 
                    foreach (string column in columns)
                    {
                        NamespaceProviderParameter parm = GetParameter(inner.Parameters, column); 
                        UIResponse.WriteCell(UIResponse.Escape(parm.Value));
                    }
					UIResponse.WriteEndRow();
				}
				UIResponse.WriteEndTable();

			}
		}

	}
}
