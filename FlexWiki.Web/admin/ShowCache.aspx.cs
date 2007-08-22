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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for ShowCache.
	/// </summary>
	public class ShowCache : AdminPage
	{
		
		// If you update these you MUST update the corresponding
        // constants in WikiAdminConstants.js
		private const string keyDataPrefix = "keyData";
		private const string keyPrefix = "keyRow";

		protected override void ShowMain()
		{
            string clear = Request.QueryString["clear"];
            if (clear == "1")
            {
                FlexWikiWebApplication.Cache.Clear();
                Response.Redirect("ShowCache.aspx");
                return;
            }

            ShowKeys();
		}
		
		protected override void ShowMenu()
		{
			UIResponse.WriteStartMenu("Cache");
			UIResponse.WriteMenuItem("ShowCache.aspx?clear=1", "Clear", "Clear the cache");
			UIResponse.WriteEndMenu();
			UIResponse.WritePara("&nbsp;");

			base.ShowMenu();
		}
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}
		
		private void ShowKeys()
        {
            // Get the number of pages to the value specified in the query string.
            // Default to 15 if it's null.
            // Get the start and end index of the keys to show.
            int numKeys = 15;
            try { numKeys = Int32.Parse(Request["keys"]); }
            catch { }
            int firstKey = 0;
            try { firstKey = Int32.Parse(Request["start"]); } catch {}
            int lastKey = firstKey + numKeys - 1;
            try { lastKey = Int32.Parse(Request["end"]); }
            catch { }

            Response.Write("<p>Search for key in current results: <input id=\"Filter\" onkeyup=\"javascript:FilterChanged()\" length=\"60\"/><br />");
            Response.Write(@"Number of results per page: <select id=""pages"" onchange=""javascript:PagesChanged()"">
    <option value=""5""" + ((5 == numKeys) ? "selected=\"selected\"" : string.Empty) + @">5</option>
    <option value=""10""" + ((10 == numKeys) ? "selected=\"selected\"" : string.Empty) + @">10</option>
    <option value=""15""" + ((15 == numKeys) ? "selected=\"selected\"" : string.Empty) + @">15</option>
    <option value=""20""" + ((20 == numKeys) ? "selected=\"selected\"" : string.Empty) + @">20</option>
    <option value=""25""" + ((25 == numKeys) ? "selected=\"selected\"" : string.Empty) + @">25</option>
</select></p>");
            
            int id = 0;

            // get and sort the cahce keys.
            ArrayList keys = new ArrayList();
            keys.AddRange(FlexWikiWebApplication.Cache.Keys);
            keys.Sort();
            // Make sure that the first and last key indices are valid.
            if (firstKey >= keys.Count)
            {
                firstKey = 0;
            }
            if (lastKey >= keys.Count)
            {
                lastKey = keys.Count - 1;
            }

            // Write out the cache results.
            Response.Write("<table cellpadding=\"2\" cellspacing=\"2\" class=\"TableClass\">");
            Response.Write("<tr>\r\n<td><strong>Cache Key</strong></td>");
            Response.Write("<td><strong>Cache Item Type</strong></td>\r\n<tr>");
            for (int i = firstKey; i <= lastKey; i++)
            {
                string key = keys[i] as string;
                Response.Write(string.Format("<tr id=\"{0}{1}\">",
                    keyPrefix, id));
                Response.Write(string.Format(
                    "<td class=\"{0}\"><span id=\"{1}{2}\">{3}</span></td>",
                    (((id & 1) == 0) ? "SearchOddRow" : "SearchEvenRow"), keyDataPrefix, id, key));
                Response.Write(string.Format(
                    "<td class=\"{0}\"><span>{1}</span></td>",
                    (((id & 1) == 0) ? "SearchOddRow" : "SearchEvenRow"),
                    FlexWikiWebApplication.Cache[key].GetType().ToString()));
                Response.Write("</tr>");
                id++;
            }
            // Write out the navigation elements.
            Response.Write("<tr>\r\n<td><strong>");
            if (firstKey > 0)
            {
                Response.Write(string.Format("<a href=\"ShowCache.aspx?keys={0}\" title=\"First page\">&lt;&lt;</a>&nbsp;|&nbsp;",
                    numKeys));
                Response.Write(string.Format("<a href=\"ShowCache.aspx?keys={0}&start={1}\" title=\"Previous page\">&lt;</a>",
                    numKeys, ((firstKey - numKeys >= 0) ? firstKey - numKeys : 0)));
            }
            Response.Write("</strong></td>\r\n<td><strong>");
            if (firstKey <= (keys.Count - numKeys))
            {
                Response.Write(string.Format("<a href=\"ShowCache.aspx?keys={0}&start={1}\" title=\"Next page\">&gt;</a>&nbsp;|&nbsp;",
                    numKeys, lastKey + 1));
                Response.Write(string.Format("<a href=\"ShowCache.aspx?keys={0}&start={1}\" title=\"Last page\">&gt;&gt;</a></strong></td>",
                    numKeys, (keys.Count / numKeys) * numKeys));
            }
            Response.Write("</tr>");
            Response.Write("</table>");
            Response.Write(string.Format("<p>Showing {0} of {1} cache keys", 
                lastKey - firstKey + 1, keys.Count));
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
