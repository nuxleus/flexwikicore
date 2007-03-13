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
//using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Mail;

using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;
using FlexWiki.Web;

namespace FlexWiki.Web.Newsletters
{
    /// <summary>
    /// Summary description for NewsletterManager.
    /// </summary>
    public class NewsletterManager
    {
        private IDeliveryBoy _deliveryBoy;
        private Federation _federation;
        private string _headInsert;
        private LinkMaker _linkMaker;
        private string _newslettersFrom;

        public NewsletterManager(Federation aFed, LinkMaker lm)
        {
            _linkMaker = lm;
            _federation = aFed;
            _headInsert = "";
        }
        public NewsletterManager(Federation aFed, LinkMaker lm, IDeliveryBoy boy, string newslettersFrom, string headInsert)
        {
            _linkMaker = lm;
            _newslettersFrom = newslettersFrom;
            _federation = aFed;
            _deliveryBoy = boy;
            _headInsert = headInsert;
        }

        private IDeliveryBoy DeliveryBoy
        {
            get { return _deliveryBoy; }
        }
        private Federation Federation
        {
            get
            {
                return _federation;
            }
        }
        private LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }

        public IEnumerable<TopicChange> AllChangesForNewsletterSince(QualifiedTopicName newsletter, DateTime since)
        {
            Log(LogLevel.Debug, "AllChangesForNewsletterSince since(" + since.ToString() + ")");
            return AllChangesForTopicsSince(AllTopicsForNewsletter(newsletter), since);
        }
        public IEnumerable<TopicChange> AllChangesForTopicsSince(IEnumerable<QualifiedTopicName> topics, DateTime since)
        {
            TopicChangeCollection answer = new TopicChangeCollection();
            foreach (QualifiedTopicName each in topics)
            {
                NamespaceManager namespaceManager = Federation.NamespaceManagerForTopic(each);
                TopicChangeCollection changes = namespaceManager.AllChangesForTopicSince(each.LocalName, since);
                int changeCount = 0;
                foreach (TopicChange change in changes)
                {
                    changeCount++;
                    answer.Add(change);
                }
                Log(LogLevel.Debug, "AllChangesForTopicsSince topic(" + each.DottedName + ") since(" + since.ToString() + ") - " + changeCount + " change(s)");
            }
            return answer;
        }
        public IEnumerable<QualifiedTopicName> AllTopicsForNewsletter(QualifiedTopicName newsletter)
        {
            Dictionary<string, QualifiedTopicName> answer = new Dictionary<string, QualifiedTopicName>();

            NamespaceManager namespaceManager = NamespaceManagerForNewsletter(newsletter);

            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Topics"))
            {
                // If the wildcard appears, ignore all the other topics listed - include every topic
                if (s == "*")
                {
                    answer.Clear();
                    foreach (QualifiedTopicName topic in namespaceManager.AllTopics(ImportPolicy.DoNotIncludeImports))
                    {
                        answer.Add(topic.DottedName, topic);
                    }
                    // No need to continue iterating after we find the wildcard 
                    break;
                }
                else
                {
                    foreach (QualifiedTopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                    {
                        answer.Add(topic.DottedName, topic);
                    }
                }
            }

            // Now we need to remove any topics that appear in the Exclude propertyName
            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Exclude"))
            {
                foreach (QualifiedTopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                {
                    answer.Remove(topic.DottedName);
                }
            }

            // Do the same for "Excludes", since it's hard to remember which one to use
            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Excludes"))
            {
                foreach (QualifiedTopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                {
                    answer.Remove(topic.DottedName);
                }
            }

            // We don't want the newsletter history page to be included, or we'll generate newsletters in an 
            // endless loop. 
            answer.Remove(NewsletterHistoryTopicFor(newsletter).DottedName); 

            // Now remove any topics for which we don't have read permission, as trying to include them 
            // would just result in an exception. 
            // We need to make a copy so we're not modifying a collection that we're iterating over
            QualifiedTopicName[] topics = new QualifiedTopicName[answer.Values.Count];
            answer.Values.CopyTo(topics, 0);
            foreach (QualifiedTopicName topic in topics)
            {
                if (!Federation.HasPermission(topic.AsQualifiedTopicRevision(), TopicPermission.Read))
                {
                    answer.Remove(topic.DottedName);
                }
            }

            return answer.Values;
        }
        public string BuildArbitraryNewsletter(string newsletterTitle, string newsletterLink, IEnumerable<QualifiedTopicName> topics,
            DateTime since, string headInsert, string description, string homeNamespace)
        {
            Log(LogLevel.Debug, "Build newsletter: " + newsletterTitle);
            StringBuilder builder = new StringBuilder();
            builder.Append(@"<html>
<head>" + headInsert + @"
        <meta name='Robots' content='NOINDEX, NOFOLLOW'>
</head>
<body class='NewsletterBody'>");

            LinkMaker lm = LinkMaker;

            if (newsletterTitle != null)
            {
                builder.Append("<div class='NewsletterName'>");
                if (newsletterLink != null)
                    builder.Append("<a href='" + newsletterLink + "'>");
                builder.Append(newsletterTitle);
                builder.Append("</a>");
                builder.Append("</div>\n");
            }

            builder.Append("<div class='NewsletterInterior'>\n");
            if (description != null)
                builder.Append("<div class='NewsletterDescription'>" + Formatter.EscapeHTML(description) + "</div>\n");

            // Organize all changes based on topic
            Dictionary<QualifiedTopicName, List<TopicChange>> changeMap = new Dictionary<QualifiedTopicName, List<TopicChange>>();
            foreach (TopicChange each in AllChangesForTopicsSince(topics, DateTime.MinValue))
            {
                List<TopicChange> list;
                QualifiedTopicName nameWithoutVersion = each.TopicRevision.AsQualifiedTopicName();
                if (!changeMap.ContainsKey(nameWithoutVersion))
                {
                    changeMap[nameWithoutVersion] = new List<TopicChange>();
                }
                list = changeMap[nameWithoutVersion];
                list.Add(each);
            }

            Dictionary<QualifiedTopicName, TopicChange> immediatelyPreviousVersions = new Dictionary<QualifiedTopicName, TopicChange>();

            builder.Append("<div class='NewsletterTOCHeader'>Table of Contents</div>");

            builder.Append("<table class='NewsletterTOCTable' border=0 cellpadding=2 cellspacing=0>");
            builder.Append("<tr>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Topic" + "</td>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Changes" + "</td>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Most Recent" + "</td>");
            builder.Append("</tr>");


            foreach (QualifiedTopicName each in topics)
            {
                if (!changeMap.ContainsKey(each))
                {
                    continue;
                }

                List<TopicChange> changesForThisTopic = changeMap[each];

                // Go through the changes and (1) find the version immediately prior 
                // to the cutoff time for each topic [so we can diff against it] and (2) remove 
                // all versions before the cutoff date
                for (int i = 0; i < changesForThisTopic.Count; )
                {
                    TopicChange c = changesForThisTopic[i];
                    if (c.Created > since)
                    {
                        i++;
                        continue;
                    }
                    if (!immediatelyPreviousVersions.ContainsKey(each))
                    {
                        immediatelyPreviousVersions[each] = c;
                    }
                    changesForThisTopic.RemoveAt(i);
                }

                if (changesForThisTopic.Count == 0)
                {
                    // no changes in range -- remove it
                    changeMap.Remove(each);
                    continue;
                }
                string appearAs = (homeNamespace == each.Namespace) ? each.LocalName : each.DottedName;
                TopicChange newestChange = (TopicChange)(changesForThisTopic[0]);

                builder.Append("<tr>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + "<div class='NewsletterTableOfContentsChangedTopicName'><a href='#" + each.DottedName + "'>" + appearAs + "</a></div>" + "</td>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + changesForThisTopic.Count + "</td>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + newestChange.Created.ToString() + "</td>");
                builder.Append("</tr>");

            }
            builder.Append("</table>");

            builder.Append("<br /><div class='NewsletterTOCFinsher'>&nbsp;</div>");

            foreach (QualifiedTopicName each in topics)
            {
                if (!changeMap.ContainsKey(each))
                {
                    continue;
                }

                List<TopicChange> changesForThisTopic = changeMap[each];
                string appearAs = (homeNamespace == each.Namespace) ? each.LocalName : each.DottedName;

                TopicChange newestChange = changesForThisTopic[0];
                TopicChange oldestChange = null;
                if (!immediatelyPreviousVersions.ContainsKey(each))
                {
                    oldestChange = newestChange;	// nothing prior
                }
                else
                {
                    oldestChange = immediatelyPreviousVersions[each];
                }

                string changedBy = null;
                List<string> changers = new List<string>();
                foreach (TopicChange c in changesForThisTopic)
                {
                    if (changers.Contains(c.Author))
                    {
                        continue;
                    }
                    changers.Add(c.Author);
                    if (changedBy == null)
                    {
                        changedBy = "changed by: " + c.Author;
                    }
                    else
                    {
                        changedBy += ", " + c.Author;
                    }
                }


                builder.AppendFormat("<div class='NewsletterTopicName'>");
                builder.AppendFormat("<a name='#" + each.DottedName + "'>{0}</a>", "<a href='" + lm.LinkToTopic(each) + "'>" + appearAs + "</a>");
                builder.AppendFormat(" (");
                builder.AppendFormat("<span class='NewsletterTopicChangers'>{0}</span>", Formatter.EscapeHTML(changedBy));
                builder.AppendFormat(" )");
                builder.AppendFormat("</div>");

                builder.Append("<div class='NewsletterTopicBody'>");
                try
                {
                    builder.Append(Formatter.FormattedTopicWithSpecificDiffs(newestChange.TopicRevision, OutputFormat.HTML, oldestChange.TopicRevision, Federation, lm));
                }
                catch (Exception ex)
                {
                    builder.Append(@"<p><b>An exception occurred while formatting this topic:</b> " + HtmlWriter.Escape(ex.ToString(), true) + "</p>");
                }
                builder.Append("</div>");
            }


            //			builder.Append("<div class='NewsletterInformationHeader'>Newsletter Information</div>\n");
            QualifiedTopicRevision homeTopic = new QualifiedTopicRevision(Federation.NamespaceManagerForNamespace(homeNamespace).HomePage, homeNamespace);
            builder.AppendFormat("<div class='NewsletterDeliveredBy'>Newsletter generated by <a href='{0}'>FlexWiki</a></div>", LinkMaker.LinkToTopic(homeTopic));

            builder.Append("</div>\n");

            builder.Append(@"</body>
</html>");

            return builder.ToString();
        }
        public QualifiedTopicNameCollection GetAllNewsletterNames()
        {
            return GetAllNewsletterNames(null);
        }
        public QualifiedTopicNameCollection GetAllNewsletterNames(string namespaceFilter)
        {
            QualifiedTopicNameCollection answer = new QualifiedTopicNameCollection();
            foreach (NamespaceManager namespaceManager in Federation.NamespaceManagers)
            {
                if (namespaceFilter != null && namespaceManager.Namespace != namespaceFilter)
                {
                    continue;
                }
                foreach (string s in Federation.GetTopicListPropertyValue(namespaceManager.QualifiedTopicNameFor("WikiNewsletterIndex"), "Newsletters"))
                {
                    answer.AddRange(namespaceManager.AllQualifiedTopicNamesThatExist(s));
                }
            }
            return answer;
        }
        public string GetDescriptionForNewsletter(QualifiedTopicName newsletter)
        {
            return Federation.GetTopicPropertyValue(newsletter, "Description");
        }
        public DateTime GetLastUpdateForNewsletter(QualifiedTopicName newsletter)
        {
            string f = Federation.GetTopicPropertyValue(NewsletterHistoryTopicFor(newsletter), newsletter.LocalName + "_LastUpdate");

            DateTime result;
            if (DateTime.TryParse(f, out result))
            {
                return result;
            }
            else
            {
                return DateTime.MinValue; // The beginning of time(ish)
            }
        }
        public void Notify()
        {
            // Troll through all the newsletters and see if any of them need an update
            foreach (QualifiedTopicName each in GetAllNewsletterNames())
            {
                Log(LogLevel.Debug, "Checking newsletter: " + each);
                DateTime nextUpdate = DateTime.MaxValue;
                DateTime lastUpdate = this.GetLastUpdateForNewsletter(each);
                Log(LogLevel.Debug, "Last newsletter update - " + lastUpdate);
                if (!IsNewsletterDueForUpdate(each, out nextUpdate))
                {
                    Log(LogLevel.Debug, "not due for update - " + nextUpdate);
                    continue;
                }
                Log(LogLevel.Debug, "due for update");
                Log(LogLevel.Debug, "collecting changes");
                IEnumerable<TopicChange> changes = AllChangesForNewsletterSince(each, lastUpdate);
                IEnumerator<TopicChange> e = changes.GetEnumerator();
                if (!e.MoveNext())
                {
                    Log(LogLevel.Debug, "no changes; skipping");
                    SetLastUpdateForNewsletter(each, DateTime.Now);
                    continue;	// no changes
                }
                Log(LogLevel.Debug, "changes found; sending newsletter");
                SendNewsletterUpdate(each);
                SetLastUpdateForNewsletter(each, DateTime.Now);
            }
        }

        private void DeliverNewsletterToAllSubscribers(QualifiedTopicName newsletter, string body, string subscribers)
        {
            foreach (string each in Federation.ParseListPropertyValue(subscribers))
            {
                DeliverNewsletterToSubscriber(newsletter, body, each);
            }
            Federation.Application.LogInfo(this.GetType().ToString(),
                string.Format("Newsletter {0} successfully delivered to all subscribers.", newsletter.DottedName));
        }
        private void DeliverNewsletterToSubscriber(QualifiedTopicName newsletter, string body, string address)
        {
            string from = _newslettersFrom;
            string to = address;
            string subject = newsletter.DottedName + " update";
            if (DeliveryBoy.Deliver(to, from, subject, body))
            {
                Federation.Application.LogDebug(this.GetType().ToString(),
                    string.Format("Newsletter {0} successfully delivered to {1}.", newsletter.DottedName, address));
            }
            else
            {
                Federation.Application.LogDebug(this.GetType().ToString(),
                    string.Format("Newsletter {0} delivery to {1} failed.", newsletter.DottedName, address));
            }
        }
        private void GenerateAndDeliverNewsletter(QualifiedTopicName newsletter)
        {
            string description = GetDescriptionForNewsletter(newsletter);
            string news = BuildArbitraryNewsletter(newsletter.LocalName,
                LinkMaker.LinkToTopic(newsletter),
                AllTopicsForNewsletter(newsletter),
                GetLastUpdateForNewsletter(newsletter),
                _headInsert,
                description,
                newsletter.Namespace);
            DeliverNewsletterToAllSubscribers(newsletter, news, Federation.GetTopicPropertyValue(newsletter, "Subscribers"));
        }
        private int GetUpdateFrequencyForNewsletter(QualifiedTopicName newsletter)
        {
            string f = Federation.GetTopicPropertyValue(newsletter, "UpdateFrequency");

            int result;
            if (Int32.TryParse(f, out result))
            {
                return result;
            }
            else
            {
                return 60 * 3;	// Default to once per three hours (60 * 3 minutes)
            }
        }
        private bool IsNewsletterDueForUpdate(QualifiedTopicName newsletter, out DateTime nextUpdate)
        {
            DateTime lastUpdate = GetLastUpdateForNewsletter(newsletter);
            int updateFrequency = GetUpdateFrequencyForNewsletter(newsletter);
            DateTime updateDue = lastUpdate.AddMinutes(updateFrequency);
            nextUpdate = updateDue;
            return updateDue <= DateTime.Now;
        }
        private bool IsNewsletterDueForUpdate(QualifiedTopicName newsletter)
        {
            DateTime throwAway;
            return IsNewsletterDueForUpdate(newsletter, out throwAway);
        }
        private void Log(LogLevel level, string message)
        {
            _federation.Application.Log(this.GetType().ToString(), level, message);
        }
        // History for a topic is in the same namespace, called _NewsletterHistory
        private QualifiedTopicName NewsletterHistoryTopicFor(QualifiedTopicName newsletter)
        {
            return new QualifiedTopicName("_NewsletterHistory", newsletter.Namespace);
        }
        private NamespaceManager NamespaceManagerForNewsletter(QualifiedTopicName newsletter)
        {
            return Federation.NamespaceManagerForNamespace(newsletter.Namespace);
        }
        private void SendNewsletterUpdate(QualifiedTopicName newsletter)
        {
            GenerateAndDeliverNewsletter(newsletter);
        }
        private void SetLastUpdateForNewsletter(QualifiedTopicName newsletter, DateTime dt)
        {
            Federation.SetTopicProperty(NewsletterHistoryTopicFor(newsletter).AsQualifiedTopicRevision(),
                newsletter.LocalName + "_LastUpdate", dt.ToString(), false, "NewsletterManager");
        }

    }
}
