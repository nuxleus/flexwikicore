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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using FlexWiki;

namespace FlexWiki.Web
{
    public class TopicControl: WebControl
    {
		
		private bool _isMain = false;
		private bool _isProperty = false;
		private LinkMaker _linkMaker;
		private int _propertyIndex = 0;
		private string _propertyName;
		private string _topic;
		private string _topicName;


		[Bindable(true),
        Category("Default"),
        DefaultValue(""),
        Description("Indicates whether this is the main topic on the page.")]
        public bool IsMain
        {
            get { return _isMain; }
            set { _isMain = value; }
        }
		
		[Bindable(true),
        Category("Default"),
        DefaultValue(""),
        Description("Indicates whether the Topic property refers to a page property.")]
        public bool IsProperty
        {
            get { return _isProperty; }
            set { _isProperty = value; }
        }
		
		[Bindable(true),
        Category("Default"),
        DefaultValue(""),
        Description("The index of the page property to use when IsProperty is true.")]
        public int PropertyIndex
        {
            get { return _propertyIndex; }
            set { _propertyIndex = value; }
        }
		
		[Bindable(true),
        Category("Default"),
        DefaultValue(""),
        Description("The name of the topic to display in this control.")]
        public string Topic
        {
            get { return _topic; }
            set { _topic = value; }
        }
		
		protected Federation Federation
        {
            get
            {
                return (Federation)(Context.Application[Constants.FederationCacheKey]);
            }
        }
		
		protected LinkMaker LinkMaker
        {
            get
            {
                return _linkMaker;
            }
        }
		
		protected string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }
		
		protected string TopicName
        {
            get { return _topicName; }
            set { _topicName = value; }
        }
		

		protected string GetTitle()
        {
            StringBuilder titlebldr = new StringBuilder();

            string title = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Title");
            if (String.IsNullOrEmpty(title))
            {
                title = GetTopicVersionKey().FormattedName;
            }

            return HtmlStringWriter.Escape(title);
        }
		
		protected QualifiedTopicRevision GetTopicVersionKey()
        {
            // Use the query string to get the current topic name
            // if this is the main topic control.
            if (true == IsMain)
            {
                return PageUtilities.GetTopicRevision(Federation);
            }
            return PageUtilities.GetTopicRevision(Federation, TopicName);
        }
		
		protected override void OnLoad(EventArgs e)
        {
            _linkMaker = new LinkMaker(PageUtilities.RootUrl);
            
            string[] parts = Topic.Split('.');
            if ((true == IsProperty) && (parts.Length > 0))
            {
                PropertyName = parts[parts.Length - 1];
                TopicName = Topic.Substring(0, Topic.LastIndexOf('.'));
            }
            else
            {
                TopicName = Topic;
            }
            // Ensure that we have a namespace prefix by defaulting
            // to the DefaultNamespace from the configuration.
            if (TopicName.IndexOf('.') < 0)
            {
                TopicName = string.Format("{0}.{1}",
                    Federation.Configuration.DefaultNamespace,
                    TopicName);
            }
        }
		
		protected override void Render(HtmlTextWriter writer)
        {
            QualifiedTopicRevision topic = GetTopicVersionKey();
            QualifiedTopicRevision diffVersion = null;

            // Get the core data (the formatted topic and the list of changes) from the cache.
            // If it's not there, generate it!
            string formattedBody = string.Empty;
            if (true == IsProperty)
            {
                try
                {
                    string borderPosition = PropertyName.Replace("Border", "");
                    Border border = (Border)Enum.Parse(typeof(Border), borderPosition, true);
                    formattedBody = Federation.GetTopicFormattedBorder(topic, border);
                }
                catch (ArgumentException)
                {
                    // PropertyName is not a Border.
                    ArrayList properties = Federation.GetTopicListPropertyValue(topic, PropertyName);
                    if (properties.Count > 0)
                    {
                        if (properties.Count > PropertyIndex)
                        {
                            formattedBody = properties[PropertyIndex] as string;
                        }
                        else
                        {
                            formattedBody = properties[0] as string;
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                formattedBody = Federation.GetTopicFormattedContent(topic, diffVersion);
            }

            // Note: This makes successive calls to the HtmlTextWriter object's Write methods 
            // instead of performing using a StringBuilder and then invoking the Write method.
            // This improves performance because the HtmlTextWriter object writes directly to 
            // the output stream. String concatenation requires time and memory to create the 
            // string, and then writes to the stream.
            if ((false == string.IsNullOrEmpty(formattedBody)) && (formattedBody.Length > 0))
            {
                // Opening <div> tag.
                writer.WriteBeginTag("div");
                writer.WriteAttribute("id", ID);
                if (false == string.IsNullOrEmpty(CssClass))
                {
                    writer.WriteAttribute("class", CssClass);
                }
                writer.Write(HtmlTextWriter.TagRightChar);

                if (true == IsMain)
                {
                    // Quicklink bar.
                    writer.WriteLine("<form method=\"post\" action=\"" + LinkMaker.LinkToQuicklink() + "?QuickLinkNamespace=" + topic.Namespace + "\" name=\"QuickLinkForm\">");
                    writer.WriteLine("<div id=\"TopicBar\" title=\"Click here to quickly jump to or create a topic\" class=\"TopicBar\" onmouseover=\"TopicBarMouseOver()\"  onclick=\"TopicBarClick(event)\"  onmouseout=\"TopicBarMouseOut()\">");
                    writer.WriteLine("<div  id=\"StaticTopicBar\"  class=\"StaticTopicBar\" style=\"display: block\">" + GetTitle() + "</div>");
                    writer.WriteLine("<div id=\"DynamicTopicBar\" class=\"DynamicTopicBar\" style=\"display: none\">");
                    writer.WriteLine("<input id=\"TopicBarInputBox\" title=\"Enter a topic here to go to or create\" class=\"QuickLinkInput\" type=\"text\"  name=\"QuickLink\" />");
                    writer.WriteLine("<div class=\"DynamicTopicBarHelp\">Enter a topic name to show or a new topic name to create; then press Enter</div>");
                    writer.WriteLine("</div></div></form>");
                }

                //if (isBlacklistedRestore)
                //{
                //    strbldr.AppendLine("<div class=\"BlacklistedRestore\"><font color=\"red\"><b>The version of the topic you are trying to restore contains content that has been banned by policy of this site.  Restore can not be completed.</b></font></div>");
                //}

                // Topic contents.            
                writer.WriteLine(formattedBody);

                // Closing </div> tag.
                writer.WriteEndTag("div");
            }
        }


    }
}
