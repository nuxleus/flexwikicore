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
using System.ComponentModel;
using System.Data;
using System.Configuration;
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
    /// Summary description for Admin.
    /// </summary>
    public class Default : AdminPage
    {

        protected override void EnsurePluginsLoaded()
        {
            // We ignore errors as the config checker will get them in the context of this specific page
            try
            {
                base.EnsurePluginsLoaded();
            }
            catch (Exception)
            {
            }
        }

        protected override void PageLoad()
        {
        }

        protected override void ShowMain()
        {
            if (CheckForConfigurationFormatUpgrade())
                return;

            ConfigurationChecker checker = new ConfigurationChecker();

            checker.Check();
            checker.WriteStoplightTo(UIResponse);

            UIResponse.WriteDivider();

            Federation aFederation = null;
            try
            {
                FlexWikiWebApplication application = new FlexWikiWebApplication(new LinkMaker(""));
                aFederation = new Federation(application);
            }
            catch (Exception)
            {
            }

            if (aFederation != null)
            {
                ShowFederationInfo(aFederation);
            }
        }
        private void ShowFederationInfo(Federation aFederation)
        {
            LinkMaker lm = new LinkMaker(RootUrl);

            UIResponse.WritePara(UIResponse.Bold("General Federation Information"));
            UIResponse.WriteStartKVTable();
            UIResponse.WriteKVRow("Created", HtmlWriter.Escape(aFederation.Created.ToString()));
            UIResponse.WriteKVRow("Default Namespace", HtmlWriter.Escape(aFederation.DefaultNamespace.ToString()));
            UIResponse.WriteEndKVTable();

            UIResponse.WriteDivider();

            UIResponse.WritePara(UIResponse.Bold("Namespace Information"));

            UITable namespacesTable = new UITable();
            namespacesTable.AddColumn(new UIColumn("Namespace"));
            namespacesTable.AddColumn(new UIColumn("Title"));
            namespacesTable.AddColumn(new UIColumn("Imports"));

            UIResponse.WriteStartTable(namespacesTable);
            foreach (NamespaceManager each in aFederation.NamespaceManagers)
            {
                UIResponse.WriteStartRow();

                string ns = each.Namespace;
                UIResponse.WriteCell(
                    UIResponse.Bold(
                        UIResponse.Link(lm.LinkToTopic(new QualifiedTopicRevision(each.HomePage, each.Namespace), false),
                            UIResponse.Escape(each.Namespace))));

                UIResponse.WriteCell(HtmlWriter.Escape(each.Title));

                string imports = "";
                foreach (string e in each.ImportedNamespaces)
                {
                    if (imports != "")
                        imports += ", ";
                    imports += e;
                }

                UIResponse.WriteCell(HtmlWriter.Escape(imports));

                UIResponse.WriteEndRow();

            }
            UIResponse.WriteEndTable();
        }


    }
}
