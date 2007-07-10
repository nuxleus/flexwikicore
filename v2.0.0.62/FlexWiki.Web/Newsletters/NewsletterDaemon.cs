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
using System.IO;
using System.Collections;
using System.Configuration;
using System.Web.Mail;
using System.Web;
using System.Threading;
using System.Text;
using System.Security.Principal;

using FlexWiki.Security;

namespace FlexWiki.Web.Newsletters
{
    /// <summary>
    /// 
    /// </summary>
    public class NewsletterDaemon
    {
        private const int c_checkinInterval = 5000;
        private const int c_maxResults = 30;
        private const int c_workInterval = 60000;

        private Federation _federation;
        private string _headInsert;
        private DateTime _lastCheckin = DateTime.MinValue;
        private Thread _monitorThread;
        private string _newslettersFrom;
        private DateTime _nextWorkDue = DateTime.MinValue;
        private GenericPrincipal _principal;
        private readonly ArrayList _results = new ArrayList();
        private string _rootUrl;
        private bool _sendAsAttachments;
        private DateTime _started = DateTime.MinValue;
        private DateTime _workLastCompleted = DateTime.MinValue;
        private DateTime _workLastStarted = DateTime.MinValue;
        private bool _workUnderway = false;


        public NewsletterDaemon(Federation fed, string rootURL, string newslettersFrom,
            string headInsert, string authenticateAs)
            : this(fed, rootURL, newslettersFrom, headInsert, false, authenticateAs)
        { }

        public NewsletterDaemon(Federation fed, string rootURL, string newslettersFrom,
            string headInsert, bool sendAsAttachments, string authenticateAs)
        {
            _federation = fed;
            _rootUrl = rootURL;
            _newslettersFrom = newslettersFrom;
            _headInsert = headInsert;
            _sendAsAttachments = sendAsAttachments;

            AuthorizationRuleWho who = AuthorizationRuleWho.Parse(authenticateAs);

            if (who.WhoType == AuthorizationRuleWhoType.GenericAnonymous)
            {
                _principal = new GenericPrincipal(new GenericIdentity(""), null);
            }
            else if (who.WhoType == AuthorizationRuleWhoType.User)
            {
                _principal = new GenericPrincipal(new GenericIdentity(who.Who), null);
            }
            else
            {
                throw new ArgumentException("Newsletters can only authenticate as 'anonymous' or as a particular user. Illegal value: " +
                    authenticateAs, "authenticateAs");
            }
        }

        public DateTime LastCheckin
        {
            get { return _lastCheckin; }
            set { _lastCheckin = value; }
        }
        public ILogEventFactory LogEventFactory
        {
            get
            {
                return Federation.LogEventFactory;
            }
        }
        public DateTime NextWorkDue
        {
            get { return _nextWorkDue; }
            set { _nextWorkDue = value; }
        }
        public IEnumerable Results
        {
            get
            {
                return _results;
            }
        }
        public DateTime Started
        {
            get { return _started; }
            set { _started = value; }
        }
        public DateTime WorkLastCompleted
        {
            get { return _workLastCompleted; }
            set { _workLastCompleted = value; }
        }
        public DateTime WorkLastStarted
        {
            get { return _workLastStarted; }
            set { _workLastStarted = value; }
        }
        public bool WorkUnderway
        {
            get { return _workUnderway; }
            set { _workUnderway = value; }
        }

        private Federation Federation
        {
            get
            {
                return _federation;
            }
        }

        public void EnsureRunning()
        {
            if (_monitorThread != null && _monitorThread.IsAlive)
            {
                return;
            }

            lock (this)
            {
                if (_monitorThread == null || !(_monitorThread.IsAlive))
                {
                    Start();
                }
                else
                {
                    MakeSureThreadHasRecentlyCheckedIn();
                }
            }
        }

        private void Checkin()
        {
            _lastCheckin = DateTime.Now;
        }
        private void DoWork()
        {
            try
            {
                if (WorkUnderway)
                    return;
                WorkUnderway = true;
                _workLastStarted = DateTime.Now;
                ReallyDoWork();
            }
            finally
            {
                _workLastCompleted = DateTime.Now;
                WorkUnderway = false;
            }
        }
        private void DoWorkIfItIsTime()
        {
            if (_nextWorkDue > DateTime.Now)
                return;
            DoWork();
            _nextWorkDue = DateTime.Now.AddMilliseconds(c_workInterval);
        }
        private void LogResult(StringBuilder b)
        {
            _results.Insert(0, b);
            while (_results.Count > c_maxResults)
            {
                _results.RemoveAt(_results.Count - 1);
            }
        }
        private void MakeSureThreadHasRecentlyCheckedIn()
        {
            // TODO
        }
        private void ReallyDoWork()
        {
            DaemonBasedDeliveryBoy boy = new DaemonBasedDeliveryBoy(_sendAsAttachments, Federation.Application);
            LinkMaker lm = new LinkMaker(_rootUrl);
            NewsletterManager manager = new NewsletterManager(Federation, lm, boy, _newslettersFrom, _headInsert);
            LogEvent ev = Federation.LogEventFactory.CreateAndStartEvent(null, null, null, LogEventType.NewsletterGeneration);
            try
            {
                manager.Notify();
            }
            finally
            {
                ev.Record();
            }
        }
        private void Start()
        {
            _monitorThread = new Thread(new ThreadStart(ThreadProc));
            _monitorThread.Name = "Newsletter " + DateTime.Now;
            _monitorThread.Priority = ThreadPriority.BelowNormal;	// be kind
            _monitorThread.Start();
            _started = DateTime.Now;
        }
        private void ThreadProc()
        {
            try
            {
                // We need the thread to run as *someone* so that it can have access 
                // to the topics. 
                Thread.CurrentPrincipal = _principal;
                while (true)
                {
                    lock (this)
                    {
                        Checkin();
                        DoWorkIfItIsTime();
                    }
                    Thread.Sleep(c_checkinInterval);
                }
            }
            catch (Exception e)
            {
                Federation.Application.LogError(this.GetType().ToString(), 
                    "Newsletter thread encountered an error and is shutting down." + e.ToString());
                throw; 
            }
        }


    }
}
