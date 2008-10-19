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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public class MessagePost : BasePage
	{
        protected Button CancelBtn;
        protected Button SaveBtn;

        protected CheckBox NewForumCheck;

        protected Label ErrorMessageCaptcha;
        protected Label ForumKeyLbl;
        protected Label ForumNamespaceLbl;
        protected Label MessageTitleLbl;
        protected Label UserLbl;

        protected Panel Panel1;    // holds controls for a new forum
        protected Panel Panel2;    // holds a message for when messaging is disabled
        protected Panel CaptchaPanel;  // holds the Captcha stuff

        protected HiddenField CaptchaContext;
        protected HiddenField CookieId;
        protected HiddenField Nonce;
        protected Image CaptchaCode;

        protected TextBox CaptchaEnteredText;
        protected TextBox ForumKeyText;
        protected TextBox ForumNamespaceText;
        protected TextBox ForumNameText;
        protected TextBox MessageText;
        protected TextBox MessageTitleText;
        protected TextBox UserText;

        protected CustomValidator CaptchaEnteredValCntl;
        protected CustomValidator ForumKeyValCntl;
        protected CustomValidator ForumNamespaceValCntl;
        protected CustomValidator ForumNameValCntl;
        protected CustomValidator MsgBodyValCntl;
        protected CustomValidator MsgTitleValCntl;

        private string _captchaContext;
        private string _captchaEnteredText;
        private string _depth;
        private string _errorMessage;
        private string _forumKey;
        private string _forumName;
        private string _forumNamespace;
        private string _messageFileName;
        private string _messageTitle;
        private string _messageText;
        private string _parentThread;
        private string _userText;

        private bool _isCaptchaRequired;
        private bool _isNewDiscuss;
        private bool _isNewTrackBack;
        private bool _newForum;

        private QualifiedTopicRevision _theTopic;

        private bool endProcess = false;
        private bool templatedPage = false;
        private string template;
        private int lineCnt;



        private string Depth
        {
            get
            {
                if (_depth != null)
                {
                    return _depth;
                }
                if (!String.IsNullOrEmpty(Request.QueryString["parentThread"]))
                {
                    if (Request.QueryString["parentThread"].Contains(";"))
                    {
                        int test = (int)(Request.QueryString["parentThread"].Split(new char[] { ';' }).Length - 1);
                        if (test > 7)
                        {
                            test = 7; //maximum nesting practical
                        }
                        _depth = test.ToString();
                    }
                    else
                    {
                        _depth = "0";
                    }
                }
                else
                {
                    _depth = "0";
                }
                return _depth;
            }
        }

        private string Forum
        {
            get
            {
                _forumName = ForumNameText.Text;
                return _forumName;
            }
        }
        private string ForumKey
        {
            get
            {
                if (_forumKey != null)
                {
                    return _forumKey;
                }
                if (IsNewTrackBack)
                {
                    if (!String.IsNullOrEmpty(Request.QueryString["topic"]))
                    {
                        _forumKey = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(1).ToString() + "Tback";
                    }
                }
                else if (IsNewDiscuss)
                {
                    if (!String.IsNullOrEmpty(Request.QueryString["topic"]))
                    {
                        _forumKey = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(1).ToString() + "Discuss";
                    }
                }
                else if (!String.IsNullOrEmpty(ForumKeyText.Text))
                {
                    _forumKey = ForumKeyText.Text;
                }
                else
                {
                    if (Request.QueryString["forumKey"] != null)
                    {
                        _forumKey = Request.QueryString["forumKey"];
                    }
                }
                if (_forumKey == null)
                {
                    NewForumCheck.Checked = true;
                    Panel1.Visible = true;
                }
                return _forumKey;
            }
        }
        private string ForumNamespace
        {
            get
            {
                if (_forumNamespace != null)
                {
                    return _forumNamespace;
                }

                if (IsNewForum)
                {
                    _forumNamespace = ForumNamespaceText.Text;
                }
                else
                {
                    if (Request.QueryString["topic"] != null)
                    {
                        _forumNamespace = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(0).ToString();
                    }
                }
                return _forumNamespace;
            }
        }

        private bool IsCaptchaRequired
        {
            get
            {
                CaptchaRequired requirement = FlexWikiWebApplication.ApplicationConfiguration.RequireCaptchaOnEdit;

                if (requirement == CaptchaRequired.Always)
                {
                    _isCaptchaRequired = true;
                    return _isCaptchaRequired;
                }

                if (requirement == CaptchaRequired.IfAnonymous)
                {
                    _isCaptchaRequired = !(User.Identity.IsAuthenticated);
                    return _isCaptchaRequired;
                }

                if (requirement == CaptchaRequired.WhenOverLinkThreshold)
                {
                    if (!IsPost)
                    {
                        _isCaptchaRequired = false;
                        return _isCaptchaRequired;
                    }

                    int submittedLinkcount = CountLinks(_messageText);
                    if (submittedLinkcount >= FlexWikiWebApplication.ApplicationConfiguration.CaptchaLinkThreshold)
                    {
                        _isCaptchaRequired = true;
                        return _isCaptchaRequired;
                    }
                }
                _isCaptchaRequired = false;
                return _isCaptchaRequired;
            }
        }
        private bool IsNewDiscuss
        {
            get
            {
                if (!String.IsNullOrEmpty(Request.QueryString["discuss"]))
                {
                    _isNewDiscuss = true;
                }
                else
                {
                    _isNewDiscuss = false;
                }
                return _isNewDiscuss;
            }
        }
        private bool IsNewForum
        {
            get
            {
                if (!String.IsNullOrEmpty(Request.QueryString["newForum"]))
                {
                    NewForumCheck.Checked = true;
                }
                _newForum = NewForumCheck.Checked;
                return _newForum;
            }
        }
        private bool IsNewTrackBack
        {
            get
            {
                if (!String.IsNullOrEmpty(Request.QueryString["blog"]))
                {
                    _isNewTrackBack = true;
                }
                else
                {
                    _isNewTrackBack = false;
                }
                return _isNewTrackBack;
            }
        }
        private string Message
        {
            get
            {
                _messageText = MessageText.Text;
                return _messageText;
            }
        }
        private string MessageFileName
        {
            get
            {
                if (_messageFileName != null)
                {
                    return _messageFileName;
                }
                if (IsNewTrackBack)
                {
                    if (!String.IsNullOrEmpty(Request.QueryString["topic"]))
                    {
                        _messageFileName = Request.QueryString["topic"].Split( new char [] { '.' }).GetValue(1).ToString() + "Tback";
                    }
                }
                else if (IsNewDiscuss)
                {
                    if (!String.IsNullOrEmpty(Request.QueryString["topic"]))
                    {
                        _messageFileName = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(1).ToString() + "Discuss";
                    }
                }
                else if (IsNewForum)
                {
                    _messageFileName = ForumNameText.Text;
                }
                else
                {
                    System.DateTime dtStart = new DateTime(2007, 9, 1, 0, 0, 1);
                    System.DateTime dtNow = DateTime.Now;
                    TimeSpan duration = dtNow - dtStart;
                    _messageFileName = ForumKey + "_T" + ((long)duration.TotalMilliseconds).ToString();

                }
                return _messageFileName;
            }

        }
        private string MessageTitle
        {
            get
            {
                _messageTitle = MessageTitleText.Text;
                return _messageTitle;
            }
        }
        private string ParentThread
        {
            get
            {
                if (_parentThread != null)
                {
                    return _parentThread;
                }

                if (!String.IsNullOrEmpty(Request.QueryString["parentThread"]))
                {
                    _parentThread = Request.QueryString["parentThread"] + MessageFileName + ";";
                }
                else
                {
                    _parentThread = MessageFileName + ";";
                }
                return _parentThread;
            }
        }
        public QualifiedTopicRevision TheTopic
        {
            get
            {
                string topic;
                if (IsNewForum)
                {
                    topic = ForumNamespace + "." + Forum;
                }
                else
                {
                    topic = Request.QueryString["topic"];
                }
                if (IsNewTrackBack)
                {
                    topic = topic + "Tback";
                }
                if (IsNewDiscuss)
                {
                    topic = topic + "Discuss";
                }
                _theTopic = new QualifiedTopicRevision(topic);
                return _theTopic;
            }
        }
        private string UserName
        {
            get
            {
                _userText = UserText.Text.TrimStart(new char[] { ' ' });
                return _userText;
            }
        }
        private bool IsXsrfProtected
        {
            get
            {
                string cookieId = Request.Form["CookieId"];
                if (!String.IsNullOrEmpty(cookieId))
                {
                    if (Request.Cookies[cookieId] != null)
                    {
                        string cookieNonce = Request.Cookies[cookieId].Value;
                        if (!String.IsNullOrEmpty(cookieNonce))
                        {
                            string formNonce = Request.Form["Nonce"];
                            string nonceKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionNoncePassphrase;
                            string cookieKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionCookiePassphrase;
                            string decryptNonce = FlexWiki.Web.Security.Decrypt(formNonce, nonceKey);
                            string decryptCookie = FlexWiki.Web.Security.Decrypt(cookieNonce, cookieKey);
                            return (String.Equals(decryptNonce, decryptCookie)); //return true if matches, otherwise fail
                        }
                        else
                        {
                            return false; //did not get a usable value from the cookie to validate, so fail
                        }
                    }
                    else
                    {
                        return false; //the cookie may have expired, so fail
                    }
                }
                else
                {
                    return false;  //did not find any reference to an xsrf cookie, so fail
                }
            }
        }
        private int CountLinks(string text)
        {
            return TopicParser.CountExternalLinks(text);
        }
        public void ForumKeyValidator(object source, ServerValidateEventArgs args)
        {
            if (IsNewForum && ForumNamespaceValCntl.IsValid)
            {
                try
                {

                    string key = args.Value;
                    ExecutionContext ctx = new ExecutionContext();
                    args.IsValid = (Federation.NamespaceManagerForNamespace(ForumNamespace).
                        TopicsWith(ctx,"Keywords",key).Count == 0);

                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        private string GenerateNewCaptchaCode()
        {
            string code = CaptchaImage.GenerateRandomCode();
            string encryptedCode = Security.Encrypt(code, FlexWikiWebApplication.ApplicationConfiguration.CaptchaKey);
            return encryptedCode;
        }
        public void CaptchaValidator(object source, ServerValidateEventArgs args)
        {
            if (IsCaptchaRequired)
            {
                try
                {
                    _captchaEnteredText = args.Value;
                    string captchaContext = CaptchaContext.Value;
                    string expectedValue = Security.Decrypt(captchaContext,
                        FlexWikiWebApplication.ApplicationConfiguration.CaptchaKey);
                    bool captchaVerified = (_captchaEnteredText.Equals(expectedValue, StringComparison.InvariantCultureIgnoreCase));
                    if (!captchaVerified)
                    {
                        _captchaContext = GenerateNewCaptchaCode();
                        CaptchaContext.Value = _captchaContext;
                        CaptchaCode.ImageUrl = "~/CaptchaImage.ashx/" + _captchaContext;
                    }
                    args.IsValid = captchaVerified;
                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        public void MsgBodyValidator(object source, ServerValidateEventArgs args)
        {
            if (!IsNewForum)
            {
                try
                {
                    string msgBody = args.Value;
                    args.IsValid = (!String.IsNullOrEmpty(msgBody));

                    if (!IsXsrfProtected)
                    {
                        args.IsValid = false;

                        //set up protection from Cross Site Request Forgery (XSRF)
                        Random rand = new Random();
                        string nonce = rand.Next().ToString();
                        string cookieId = Guid.NewGuid().ToString();
                        string nonceKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionNoncePassphrase;
                        string cookieKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionCookiePassphrase;
                        string encryptedNonce = FlexWiki.Web.Security.Encrypt(nonce, nonceKey);
                        string encryptedCookie = FlexWiki.Web.Security.Encrypt(nonce, cookieKey);
                        CookieId.Value = cookieId;
                        Nonce.Value = encryptedNonce;
                        HttpCookie xsrf = new HttpCookie(cookieId);
                        xsrf.HttpOnly = true;
                        xsrf.Value = encryptedCookie;
                        xsrf.Expires = DateTime.Now.AddMinutes(FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionWikiEditTimeout);
                        Response.Cookies.Add(xsrf);
                    }
                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        public void MsgTitleValidator(object source, ServerValidateEventArgs args)
        {
            if (!IsNewForum)
            {
                try
                {
                    string msgTitle = args.Value;
                    args.IsValid = (!String.IsNullOrEmpty(msgTitle));
                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        public void NamespaceValidator(object source, ServerValidateEventArgs args)
        {
            if (IsNewForum)
            {
                try
                {
                    string nmspc = args.Value;
                    args.IsValid = (Federation.NamespaceManagerForNamespace(nmspc).Exists);
                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        public void ForumNameValidator(object source, ServerValidateEventArgs args)
        {
            if (IsNewForum && ForumNamespaceValCntl.IsValid)
            {
                try
                {
                    string name = args.Value;
                    if (String.IsNullOrEmpty(name))
                    {
                        args.IsValid = false;
                    }
                    else
                    {
                        args.IsValid = (!Federation.NamespaceManagerForNamespace(ForumNamespace).GetTopicInfo(name).Exists);
                    }
                }
                catch (Exception ex)
                {
                    args.IsValid = false;
                    _errorMessage = ex.Message;
                }
            }
            else
            {
                args.IsValid = true;
            }
        }
        public void NonceValidator(object source, ServerValidateEventArgs args)
        {
            if (!IsXsrfProtected)
            {
                args.IsValid = false;

                //set up protection from Cross Site Request Forgery (XSRF)
                Random rand = new Random();
                string nonce = rand.Next().ToString();
                string cookieId = Guid.NewGuid().ToString();
                string nonceKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionNoncePassphrase;
                string cookieKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionCookiePassphrase;
                string encryptedNonce = FlexWiki.Web.Security.Encrypt(nonce, nonceKey);
                string encryptedCookie = FlexWiki.Web.Security.Encrypt(nonce, cookieKey);
                CookieId.Value = cookieId;
                Nonce.Value = encryptedNonce;
                HttpCookie xsrf = new HttpCookie(cookieId);
                xsrf.HttpOnly = true;
                xsrf.Value = encryptedCookie;
                xsrf.Expires = DateTime.Now.AddMinutes(FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionWikiEditTimeout);
                Response.Cookies.Add(xsrf);

            }
            else
            {
                args.IsValid = true;
            }
        }
        protected void CancelBtn_Click(Object sender, System.EventArgs e)
        {
            Response.Redirect(TheLinkMaker.LinkToTopic(TheTopic.ToString()));
        }
        protected void SaveBtn_Click(Object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(ForumKey))
            {
                Panel1.Visible = true;
                NewForumCheck.Checked = true;
                ForumKeyValCntl.Validate();
            }
            else if (Page.IsValid)
            {
                DoSave();
                Response.Redirect(TheLinkMaker.LinkToTopic(TheTopic.ToString()));
            }
        }

        private void DoSave()
        {
            string _topicText;
            string userInfo;
            Services.EditServiceImplementation webSvc = new FlexWiki.Web.Services.EditServiceImplementation();
            Services.WireTypes.AbsoluteTopicName absTopic = new FlexWiki.Web.Services.WireTypes.AbsoluteTopicName();
            absTopic.Namespace = ForumNamespace;
            absTopic.Name = MessageFileName;
            if (IsNewForum || IsNewTrackBack || IsNewDiscuss)
            {
                _topicText = NewForumTopicText();
            }
            else
            {
                _topicText = NewMessageTopicText();
            }
            if (User.Identity.IsAuthenticated)
            {
                userInfo = VisitorIdentityString;
            }
            else
            {
                if (String.IsNullOrEmpty(UserName))
                {
                    userInfo = "Anonymous - " + Request.UserHostAddress;
                }
                else
                {
                    userInfo = UserName + " - " + Request.UserHostAddress;
                }
            }

            webSvc.SetTextForTopic(absTopic, _topicText, userInfo);
        }
        private string NewForumTopicText()
        {
            StringBuilder strbldr = new StringBuilder();

            strbldr.AppendLine(@"@@DoInfoBoxForum()@@");
            strbldr.AppendLine(":Keywords: " + ForumKey);
            strbldr.AppendLine(":Title: " + MessagePostFix(MessageTitle));
            if (_isNewDiscuss)
            {
                strbldr.AppendLine(":DiscussionForum: Active");
            }
            else if (_isNewTrackBack)
            {
                strbldr.AppendLine(":TbackDiscuss: Active");
            }
            if (!String.IsNullOrEmpty(FlexWikiWebApplication.ApplicationConfiguration.ThreadedMessagingEditPermissions))
            {
                strbldr.AppendLine(":DenyEdit: all");
                strbldr.AppendFormat(":AllowEdit: {0}", FlexWikiWebApplication.ApplicationConfiguration.ThreadedMessagingEditPermissions);
                strbldr.AppendLine("");
            }
            strbldr.Append(MessagePostFix(Message));
            strbldr.AppendLine("");
            strbldr.AppendLine("@@[Presentations.Link(\"javascript:Collapse()\", \"Collapse Sub Threads\"), \"&nbsp;&nbsp;&nbsp;\", Presentations.Link(\"javascript:OpenAll()\", \"Open All Threads\")]@@");

            strbldr.AppendLine("&nbsp;");
            strbldr.AppendLine(":With: WikiTalkLibrary.ForumLibrary");
            strbldr.AppendLine(@"@@Presentations.Link(federation.LinkMaker.SimpleLinkTo(""MessagePost.aspx?topic=" + TheTopic + @"&forumKey=" + ForumKey + @"&parentThread=""),""Start New Thread"")@@");
            strbldr.AppendLine("----");
            strbldr.AppendLine("@@GetNodes(topic.Keywords).Collect { each |ShowEntry(each)}@@");
            //strbldr.AppendLine(":GetNodes:{namespace.Topics.Select{ each |");
            //strbldr.AppendLine(@"        each.Keywords.Contains(""" + ForumKey + @""")}.Select{each | each.HasProperty(""ParentThread"")}.SortBy{each | each.GetProperty(""ParentThread"")}");
            //strbldr.AppendLine("}");
            //strbldr.AppendLine("&nbsp;");
            //strbldr.AppendLine(":ShowEntry:{entry |");
            //strbldr.AppendLine(@"[[""@@Presentations.ContainerStart(\""div\"","", ""\"""", entry.Name, ""\"","", ""\"""",");
            //strbldr.AppendLine(@"    [""Depth0"",""Depth1"",""Depth2"",""Depth3"",""Depth4""].Item(entry.GetProperty(""Depth"").AsInteger), ""\"")@@""].ToOneString, Newline,");
            //strbldr.AppendLine(@"""!!!!"", entry.GetProperty(""Title""), Newline,");
            //strbldr.AppendLine(@"    entry.LastModifiedBy, "" "", entry.LastModified.ToLongDateString(), entry.LastModified.ToLongTimeString(), Newline, entry.GetProperty(""_Body""), Newline,");
            //strbldr.AppendLine(@"[""@@Presentations.Link(federation.LinkMaker.SimpleLinkTo(\""MessagePost.aspx?topic=" + TheTopic + @"&forumKey=" + ForumKey + @"&parentThread=""");
            //strbldr.AppendLine(@",entry.GetProperty(""ParentThread"")," + @"""&title=""" + @",entry.GetProperty(""Title"")," + @"""\""),\""Reply To This\"")@@""].ToOneString, Newline,");
            //strbldr.AppendLine(@"[""@@Presentations.ContainerEnd(\""div\"")@@""], Newline,");
            //strbldr.AppendLine(@"""----"", Newline]");
            //strbldr.AppendLine("}");

            return strbldr.ToString();

        }

        private string NewMessageTopicText()
        {
            StringBuilder strbldr = new StringBuilder();

            strbldr.AppendLine(":Keywords: " + ForumKey);
            strbldr.AppendLine(":Title: " + MessagePostFix(MessageTitle));
            strbldr.AppendLine(":Depth: " + Depth);
            strbldr.AppendLine(":ParentThread: " + ParentThread);
            if (!String.IsNullOrEmpty(FlexWikiWebApplication.ApplicationConfiguration.ThreadedMessagingEditPermissions))
            {
                strbldr.AppendLine(":DenyEdit: all");
                strbldr.AppendFormat(":AllowEdit: {0}", FlexWikiWebApplication.ApplicationConfiguration.ThreadedMessagingEditPermissions);
                strbldr.AppendLine("");
            }
            strbldr.Append(MessagePostFix(Message));

            return strbldr.ToString();
        }
        private void Page_Load(object sender, System.EventArgs e)
		{
            CaptchaEnteredValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.CaptchaValidator);
            ForumNamespaceValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.NamespaceValidator);
            ForumKeyValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.ForumKeyValidator);
            ForumNameValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.ForumNameValidator);
            MsgBodyValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.MsgBodyValidator);
            MsgTitleValCntl.ServerValidate +=
                new ServerValidateEventHandler(this.MsgTitleValidator);
            

            CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            if (FlexWikiWebApplication.ApplicationConfiguration.DisableThreadedMessaging)
            {
                Panel1.Visible = false;
                NewForumCheck.Visible = false;
                MessageTitleText.Visible = false;
                MessageText.Visible = false;
                UserText.Visible = false;
                SaveBtn.Visible = false;
                MessageTitleLbl.Visible = false;
                UserLbl.Visible = false;

                Panel2.Visible = true;
                CaptchaPanel.Visible = false;
            }
            else
            {
                Panel1.Visible = false;
                Panel2.Visible = false;
                if (IsNewTrackBack || IsNewDiscuss)
                {
                    NewForumCheck.Visible = false;
                }
                if (!IsPostBack)
                {
                    if (IsCaptchaRequired)
                    {
                        _captchaContext = GenerateNewCaptchaCode();
                        CaptchaContext.Value = _captchaContext;
                        CaptchaCode.ImageUrl = "~/CaptchaImage.ashx/" + _captchaContext;
                        ErrorMessageCaptcha.Visible = false;
                        CaptchaPanel.Visible = true;
                    }
                    else
                    {
                        CaptchaPanel.Visible = false;
                    }
                    //set up protection from Cross Site Request Forgery (XSRF)
                    Random rand = new Random();
                    string nonce = rand.Next().ToString();
                    string cookieId = Guid.NewGuid().ToString();
                    string nonceKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionNoncePassphrase;
                    string cookieKey = FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionCookiePassphrase;
                    string encryptedNonce = FlexWiki.Web.Security.Encrypt(nonce, nonceKey);
                    string encryptedCookie = FlexWiki.Web.Security.Encrypt(nonce, cookieKey);
                    CookieId.Value = cookieId;
                    Nonce.Value = encryptedNonce;
                    HttpCookie xsrf = new HttpCookie(cookieId);
                    xsrf.HttpOnly = true;
                    xsrf.Value = encryptedCookie;
                    xsrf.Expires = DateTime.Now.AddMinutes(FlexWikiWebApplication.ApplicationConfiguration.XsrfProtectionWikiEditTimeout);
                    Response.Cookies.Add(xsrf);

                    NewForumCheck.Checked = false;
                    if (!String.IsNullOrEmpty(Request.QueryString["title"]))
                    {
                        MessageTitleText.Text = Request.QueryString["title"];
                    }
                    else
                    {
                        if ((!String.IsNullOrEmpty(Request.QueryString["newForum"])) && (!String.IsNullOrEmpty(Request.QueryString["topic"])))
                        {
                            if (IsNewForum)
                            {
                                Panel1.Visible = true;
                                ShowNewForumDetails();
                            }
                        }
                    }
                }
                if (IsPostBack)
                {
                   if (IsNewForum)
                    {
                        Panel1.Visible = true;
                    }
                }
            }

		}
        private void ShowNewForumDetails()
        {
            ForumNameText.Text = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(1).ToString() + "Discuss";
            ForumKeyText.Text = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(1).ToString() + "Discuss";
            ForumNamespaceText.Text = Request.QueryString["topic"].Split(new char[] { '.' }).GetValue(0).ToString();
        }
        protected string BuildPageOne()
        {

            StringBuilder strOutput = new StringBuilder();
            strOutput.AppendLine("<script type=\"text/javascript\" language=\"javascript\" src=\"" + RootUrl + "MessagePost.js\"></script>");

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

            strOutput.AppendLine(BuildPreview());
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

        protected string BuildPreview()
        {
            StringBuilder strbldr = new StringBuilder();

            string ns = ForumNamespace;
            if (String.IsNullOrEmpty(ns))
            {
                ns = DefaultNamespace;
            }
            strbldr.AppendLine("<div style=\"display: none\">");
            strbldr.AppendLine("<form id=\"Form2\" method=\"post\" target=\"previewWindow\" action=\"Preview.aspx\">");
            strbldr.AppendLine("<textarea id=\"body\" name=\"body\" rows=\"20\" cols=\"60\"></textarea>");
            strbldr.AppendFormat("<input  type=\"text\" id=\"Text1\" name=\"defaultNamespace\" value =\"{0}\" />", ns);
            strbldr.AppendFormat("<input  type=\"text\" id=\"Text2\" name=\"topic\" value =\"{0}\" />", "PreviewPost");

            strbldr.AppendLine("</form>");
            strbldr.AppendLine("</div></div>");
            //strbldr.AppendFormat("<div id=\"previewBtn\" style=\"display: inline\">");
            //strbldr.AppendLine("<button onclick=\"javascript:previewPost()\" id=\"button1\">Preview Post</button>");
            //strbldr.AppendLine("</div></div>");

            return strbldr.ToString();

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

            strOutput.AppendLine(InsertLeftTopBorders());

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
