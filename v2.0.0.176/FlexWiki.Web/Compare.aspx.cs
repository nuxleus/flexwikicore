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
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary for Compare.
    /// </summary>
    public class Compare : BasePage
    {

        private LogEvent _mainEvent = null;
        private QualifiedTopicRevision _requestedTopic = null;

        private string _topicString = string.Empty;
        private int _diff = 0;
        private int _oldid = 0;

        protected QualifiedTopicRevision RequestedTopic
        {
            get
            {
                return _requestedTopic;
            }
        }

        #region Vom Web Form-Designer generierter Code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: Dieser Aufruf ist f�r den ASP.NET Web Form-Designer erforderlich.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Erforderliche Methode f�r die Designerunterst�tzung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor ge�ndert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }
        #endregion

        private void Page_Load(object sender, System.EventArgs e)
        {
            _topicString = Request.QueryString["topic"];

            try
            {
                _diff = Convert.ToInt32(Request.QueryString["diff"]);
            }
            catch { }
            try
            {
                _oldid = Convert.ToInt32(Request.QueryString["oldid"]);
            }
            catch { }

            try
            {
                _requestedTopic = new QualifiedTopicRevision(_topicString);
            }
            catch { }

            if (_requestedTopic == null || _diff >= _oldid)
            {
                Response.Redirect("default.aspx");
            }
        }

        protected string GetTitle()
        {
            string title = Federation.GetTopicPropertyValue(RequestedTopic, "Title");
            if (title == null || title == "")
            {
                title = string.Format("{0} - {1}", GetTopicVersionKey().FormattedName, GetTopicVersionKey().Namespace);
            }
            return HtmlStringWriter.Escape(title);
        }

        protected void StartPage()
        {
            if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicsCompared) != null)
            {
                Federation.GetPerformanceCounter(PerformanceCounterNames.TopicsCompared).Increment();
            }

            _mainEvent = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, RequestedTopic.LocalName, LogEventType.CompareTopic);
            VisitorEvent e = new VisitorEvent(RequestedTopic, VisitorEvent.Compare, DateTime.Now);
            LogVisitorEvent(e);
        }

        protected void EndPage()
        {
            _mainEvent.Record();
        }

        protected void DoPage()
        {
            QualifiedTopicRevision newestTopicVersion = null;
            QualifiedTopicRevision oldTopicVersion = null;
            int counter = 0;
            IEnumerable changeList = Federation.GetTopicChanges(RequestedTopic.AsQualifiedTopicName());
            foreach (TopicChange change in changeList)
            {
                if (counter == _diff)
                {
                    newestTopicVersion = new QualifiedTopicRevision(change.DottedName);
                }
                else if (counter == _oldid)
                {
                    oldTopicVersion = new QualifiedTopicRevision(change.DottedName);
                    break;
                }
                counter++;
            }

            if (newestTopicVersion != null && oldTopicVersion != null)
            {
                Response.Write(@"<div id=""MainRegion"" class=""TopicBody"">
<form method=""post"" action=""" + TheLinkMaker.LinkToQuicklink() + @"?QuickLinkNamespace=" + RequestedTopic.Namespace + @""" name=""QuickLinkForm"">
<div id=""TopicBar"" title=""Click here to quickly jump to or create a topic"" class=""TopicBar"" onmouseover=""javascript:TopicBarMouseOver()""  onclick=""javascript:TopicBarClick(event)""  onmouseout=""javascript:TopicBarMouseOut()"">
<div  id=""StaticTopicBar"" class=""StaticTopicBar"" style=""display: block"">" + GetTitle() + @"</div>
<div id=""DynamicTopicBar"" class=""DynamicTopicBar"" style=""display: none"">
<!-- <input id=""TopicBarNamespace"" style=""display: none"" type=""text""  name=""QuickLinkNamespace""/> -->
<input id=""TopicBarInputBox"" title=""Enter a topic here to go to or create"" class=""QuickLinkInput"" type=""text""  name=""QuickLink""/>
<div class=""DynamicTopicBarHelp"">Enter a topic name to show or a new topic name to create; then press Enter</div>
</div>
</div>
</form>
");
                string formattedBody = Federation.GetTopicFormattedContent(newestTopicVersion, oldTopicVersion);
                Response.Write(formattedBody);
                Response.Write("</div>");

            }
        }
    }
}
