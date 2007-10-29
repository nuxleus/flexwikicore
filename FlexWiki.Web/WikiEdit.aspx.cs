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
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki.Collections; 
using FlexWiki.Formatting;


namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for WikiEdit.
    /// </summary>
    public class WikiEdit : BasePage
    {
        protected System.Web.UI.HtmlControls.HtmlImage _imgFile;
        protected System.Web.UI.HtmlControls.HtmlInputText _saveButtonPressed;
        protected System.Web.UI.HtmlControls.HtmlInputText _processAttachment;
        protected System.Web.UI.HtmlControls.HtmlInputFile _uploadFilePath;

        private string _errorMessage = "";
        private string _fileName = "";
        private string _fileSize = "";
        private string _fileUrl = "";
        private string _fileIconUrl = "";
        private QualifiedTopicRevision _theTopic;

        private void Page_Load(object sender, System.EventArgs e)
        {
            //Page_Load code here
            if (IsPostBack)
            {
                if (IsAttachment)
                {
                    FileUploadSendClick(sender, e);
                }
            }
        }
        public QualifiedTopicRevision TheTopic
        {
            get
            {
                if (_theTopic != null)
                {
                    return _theTopic;
                }
                string topic;
                if (IsPost)
                {
                    topic = Request.Form["Topic"];
                }
                else
                {
                    topic = Request.QueryString["topic"];
                }
                _theTopic = new QualifiedTopicRevision(topic);
                return _theTopic;
            }
        }
        protected string ReturnTopic
        {
            get
            {
                string answer = null;
                if (IsPost)
                {
                    answer = Request.Form["ReturnTopic"];
                }
                else
                {
                    answer = Request.QueryString["return"];
                }
                if (answer == "")
                {
                    answer = null;
                }
                return answer;
            }
        }

        private bool IsAttachment
        {
            get
            {
                if (_processAttachment.Value == "IsAttachment")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private bool IsBack
        {
            get
            {
                string saveButton = Request.Form["SaveButtonPressed"];

                if (string.IsNullOrEmpty(saveButton))
                {
                    return false;
                }
                else if (saveButton == "Back")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        protected bool IsBanned
        {
            get
            {
                string proposed = PostedTopicText;
                foreach (string each in Federation.BlacklistedExternalLinkPrefixes)
                {
                    if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        private bool IsCaptchaVerified
        {
            get
            {
                if (!IsCaptchaRequired())
                {
                    return true; 
                }

                string captchaContext = Request.Form["CaptchaContextSubmitted"];
                string captchaEntered = Request.Form["CaptchaEnteredSubmitted"];

                if (string.IsNullOrEmpty(captchaContext))
                {
                    FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                        "CAPTCHA context was empty or missing."); 
                    return false; 
                }

                if (string.IsNullOrEmpty(captchaEntered))
                {
                    FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                        "CAPTCHA entered by user was empty or missing.");
                    return false; 
                }

                string expectedValue = Security.Decrypt(captchaContext,
                    FlexWikiWebApplication.ApplicationConfiguration.CaptchaKey);

                if (captchaEntered.Equals(expectedValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                        "CAPTCHA value entered by user was verified.");
                    return true;
                }
                else
                {
                    FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                        "User entered incorrect CAPTCHA value. Expected value was " + expectedValue + 
                        " but user entered " + captchaEntered);
                    return false;
                }
            }
        }
        protected bool IsConflictingChange
        {
            get
            {
                if (!IsPost)
                {
                    return false;
                }
                string lastEdit = Request.Form["TopicLastWrite"];
                if (lastEdit == "" || lastEdit == null)
                {
                    return false;	// it's probably new
                }
                DateTime currentStamp;

                return Federation.TopicExists(TheTopic) &&
                    !(currentStamp = Federation.GetTopicModificationTime(TheTopic)).ToString("s").Equals(lastEdit);
            }
        }
        private bool IsUploadable
        {
            get
            {
                string uploadDirectory = FlexWikiWebApplication.ApplicationConfiguration.ContentUploadPath;
                if (uploadDirectory != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsWritable
        {
            get
            {
                return Federation.IsExistingTopicWritable(TheTopic);
            }
        }
        private string OriginalTopicText
        {
            get
            {
                string text = string.Empty;
                if (Federation.TopicExists(TheTopic))
                {
                    text = Federation.Read(TheTopic);
                }

                return text; 
            }
        }
        private string PostedTopicText
        {
            get
            {
                string topicbody = "";
                topicbody = Request.Form["EditBox"];

                if (String.IsNullOrEmpty(topicbody))
                {
                    topicbody = Request.Form["PostBox"];
                }
                return topicbody ?? "";
            }
        }


        protected void DoPage()
        {
            using (RequestContext.Create())
            {
                if (IsPost && !IsConflictingChange && !IsBanned)
                {
                    if (IsCaptchaVerified)
                    {
                        ProcessPost();
                    }
                    else
                    {
                        ShowEditPage(true);
                    }
                }
                else
                {
                    ShowEditPage();
                }
            }
        }

        protected void ProcessSave(bool back)
        {
            QualifiedTopicRevision returnTo = null;

            //Check Null edits
            string oldContent = OriginalTopicText;

			// if the posted text is different than the orig, or if there was no orig, proceed
            if (string.Compare(oldContent, PostedTopicText) != 0 || !Federation.TopicExists(TheTopic))
            {
                bool isDelete = Regex.IsMatch(PostedTopicText, "^delete$", RegexOptions.IgnoreCase);
                LogEvent ev;
                LogEventType evType = isDelete ? LogEventType.DeleteTopic : LogEventType.WriteTopic;
                ev = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, TheTopic.ToString(), evType);

                try
                {
                    QualifiedTopicRevision newVersionName = new QualifiedTopicRevision(TheTopic.LocalName, TheTopic.Namespace);
                    newVersionName.Version = TopicRevision.NewVersionStringForUser(VisitorIdentityString, Federation.TimeProvider);
                    NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(TheTopic.Namespace);
                    storeManager.WriteTopicAndNewVersion(newVersionName.LocalName, PostedTopicText, VisitorIdentityString);
                    returnTo = TheTopic;

                    if (isDelete)
                    {
                        returnTo = null;	// we won't be able to go back here because we're deleting it!
                        Federation.DeleteTopic(TheTopic);
                    }

                    if (back && ReturnTopic != null)
                    {
                        returnTo = new QualifiedTopicRevision(ReturnTopic);
                    }
                }
                finally
                {
                    if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicWrite) != null)
                    {
                        Federation.GetPerformanceCounter(PerformanceCounterNames.TopicWrite).Increment();
                    }
                    ev.Record();
                }
            }
            else
            {
                string topic = "<<null>>";
                if (TheTopic != null && TheTopic.DottedName != null)
                {
                    topic = TheTopic.DottedName; 
                }
                FlexWikiWebApplication.LogDebug(this.GetType().ToString(), "A null edit was submitted for topic " + topic); 
            }

            if (returnTo == null)
            {
               
                if (TheTopic != null && TheTopic.DottedName != null)
                {
                    Response.Redirect(TheLinkMaker.LinkToTopic(TheTopic.ToString()));
                }
                else
                {
                    FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                        "Redirecting to wiki root.");
                    Response.Redirect(RootUrl);
                }
                
            }
            else
            {
                FlexWikiWebApplication.LogDebug(this.GetType().ToString(),
                    "Redirecting to: " + returnTo.DottedNameWithVersion);
                Response.Redirect(TheLinkMaker.LinkToTopic(returnTo));
            }

        }
        protected void ShowCss()
        {
            Response.Write("<script type=\"text/javascript\" language=\"javascript\" src=\""+ RootUrl + "WikiEdit.js\"></script>");

        }
        
        protected void ShowEditPageFinal()
        {
            Response.Write("</td></tr></table></div></td></tr></table>");

            Response.Write("</div></td></tr></table>");

            Response.Write("</td></tr></table></body>");
        }
        protected void ShowEditPageMiddle()
        {

            StringBuilder strbldr = new StringBuilder();
            if (_errorMessage.Length > 0)
            {
                strbldr.AppendFormat("<p><span style=\"color: red\">{0}</span></p>", _errorMessage);
            }
            strbldr.AppendLine("<table class=\"SidebarTileInsert\">");
            strbldr.AppendLine("<tr>");
            strbldr.AppendLine("<td valign=\"middle\" class=\"SidebarTileTitle\" style=\"font-size: 1.1em;color: white;\">");
            strbldr.AppendLine("File Attachment");
            strbldr.AppendLine("</td>");
            strbldr.AppendLine("</tr>");
            strbldr.AppendLine("<tr class=\"SidebarTileBody\" style=\"font-size: 1.1em;\"><td>");

            strbldr.AppendLine("<p><input id=\"Radio1\" type=\"radio\" name=\"attachFormat\" value=\"Normal\"  onfocus=\"showNormal_OnFocus()\" />Normal Attachment</p>");
            strbldr.AppendLine("<p><input id=\"Radio2\" type=\"radio\" name=\"attachFormat\" value=\"Folder\"  onfocus=\"showFolder_OnFocus()\" />File Folder Attachment</p>");
            strbldr.AppendLine("<p><input id=\"Radio3\" type=\"radio\" name=\"attachFormat\"  value=\"DocMan\" onfocus=\"showDocMan_OnFocus()\" />Document Management Attachment</p>");
            strbldr.AppendLine("</td></tr><tr><td><div id=\"CommonFields\" style=\"display:none\" ><p>");

            strbldr.AppendFormat("<input type=\"text\" id=\"_fileName\" value=\"{0}\" />&nbsp;Filename</p>", _fileName);
            strbldr.AppendFormat("<p><input type=\"text\" id=\"_fileSize\" value=\"{0}\" />&nbsp;Size</p>", _fileSize);
            strbldr.AppendFormat("<p><input type=\"text\" id=\"_fileUrl\" value=\"{0}\" />&nbsp;URL</p>", Formatter.EscapeHTML(_fileUrl));
            strbldr.AppendFormat("<p><input type=\"text\" id=\"_fileIconUrl\" value=\"{0}\" />&nbsp;Icon URL</p>", Formatter.EscapeHTML(_fileIconUrl));
            strbldr.AppendLine("<p><input id=\"Form3DocTitle\" type=\"text\" size=\"20\" />&nbsp;Document Title</p></div>");
            strbldr.AppendLine("<div id=\"VersionFields\" style=\"display:none\" ><p><input id=\"Form3Version\" type=\"text\" /> Document Version</p></div>");
            strbldr.AppendLine("<div id=\"FileNoFields\" style=\"display:none\" ><p><input id=\"Form3FileNo\" type=\"text\" /> Orig File No</p></div>");
            strbldr.AppendLine("<div id=\"AuthorFields\" style=\"display:none\" ><p><input id=\"Form3Author\" type=\"text\" size=\"20\" /> Originator</p></div>");
            strbldr.AppendLine("<div id=\"PublishedFields\" style=\"display:none\" ><p><input id=\"Form3Published\" type=\"text\" /> Published</p></div>");
            strbldr.AppendLine("<div id=\"FiledFields\" style=\"display:none\" ><p><input id=\"Form3Filed\" type=\"text\" /> Filed</p></div>");
            strbldr.AppendLine("<div id=\"StatusFields\" style=\"display:none\" ><p>");
            strbldr.AppendLine("<select id=\"Form3Status\">");
            strbldr.AppendLine("<option value=\"Draft\">Draft</option>");
            strbldr.AppendLine("<option value=\"Review\">Review</option>");
            strbldr.AppendLine("<option value=\"Acceptance\">Acceptance</option>");
            strbldr.AppendLine("<option value=\"Published\">Published</option>");
            strbldr.AppendLine("<option value=\"Cancelled\">Cancelled</option>");
            strbldr.AppendLine("</select> Document Status");
            strbldr.AppendLine("</p></div>");
            strbldr.AppendLine("<div id=\"StatusDateFields\" style=\"display:none\" ><p>");
            strbldr.AppendLine("<input id=\"Form3StatusDate\" type=\"text\" /> Status Date</p></div>");
            strbldr.AppendLine("<div id=\"CommentFields\" style=\"display:none\" ><p>Comment: <br /><textarea id=\"Form3Comment\" cols=\"20\" rows=\"2\"></textarea></p></div>");
            strbldr.AppendLine("<div id=\"attachFileBtn\" ><p><button id=\"attachFile\"  name=\"attachFile\" onclick=\"attachFile_OnClick()\" >Link Attachment to Topic</button></p></div>");
            strbldr.AppendLine("</td></tr>");
            strbldr.AppendLine("</table>");

            Response.Write(strbldr.ToString());
        }

        private int CountLinks(string text)
        {
            return TopicParser.CountExternalLinks(text); 
        }
        private void FileUploadSendClick(object sender, System.EventArgs e)
        {

            try
            {
                // Check to        see        if file        was        uploaded
                if (_uploadFilePath != null)
                {
                    _errorMessage = "";
                    // Get a reference to PostedFile object
                    HttpPostedFile fileToUpload = _uploadFilePath.PostedFile;
                    int fileLength = fileToUpload.ContentLength;
                    if (fileLength > 0)
                    {
                        // Allocate a buffer for reading of the file
                        byte[] fileData = new byte[fileLength];
                        string directoryToWriteTo;
                        string writeToFile;
                        string fileName;
                        // Read        uploaded file from the Stream
                        fileToUpload.InputStream.Read(fileData, 0, fileLength);
                        
                        directoryToWriteTo = ReturnDirectoryToWriteTo(fileToUpload.FileName);
                        // Create a        name for the file to store
                        fileName = Path.GetFileName(fileToUpload.FileName); //
                        // Write data into a file
                        writeToFile = Server.MapPath(fileName);
                        // Get the current virtual directory, mapped to        a path
                        writeToFile = Server.MapPath("") + "\\" + directoryToWriteTo + "\\" + fileName;
                        // fileNumber var is the one digit number that is added        to a file name if it exists
                        int fileNumber = 0;
                        string fileNameAdder = "";
                        string newFileName = "";


                        // append a        three digit        int        to the file        name to        make it        unique
                        while (File.Exists(writeToFile))
                        {
                            if (fileNumber > 99)
                            {
                                throw new System.IO.IOException("Too many copies of file exist on server. Please rename it and upload again.");
                            }
                            fileNumber++;
                            fileNameAdder = ("-" + fileNumber.ToString("00") + ".");
                            newFileName = Server.MapPath("") + "\\" + directoryToWriteTo + "\\" + fileName.Replace(".", fileNameAdder);


                            if (File.Exists(newFileName))
                            {
                                // if the new file name is taken start over
                                continue;
                            }
                            else
                            {
                                // if the new file name is not taken, terminate the loop
                                writeToFile = newFileName;
                                break;
                            }
                        }


                        // write the file out
                        WriteToFile(writeToFile, ref fileData);
                        // get the name of the file
                        fileName = Path.GetFileName(writeToFile);


                        // Set label's text
                        _fileName = fileName;
                        // show        the        location
                        _fileSize = "File size: " + fileLength.ToString("0,0") + " bytes";
                        // show        the        full path
                        _fileUrl = RootUrl + directoryToWriteTo.Replace(@"\", @"/") + "/" + Path.GetFileName(writeToFile);
                        _fileIconUrl =  "images/attach/" + ReturnIconForAttach(fileName);


                        // Set URL of the the object to        point to the file we've        just saved
                        if (IsImageFile(writeToFile))
                        {

                            _fileUrl = RootUrl + directoryToWriteTo.Replace(@"\", @"/") + "/" + Path.GetFileName(writeToFile);

                            // show the images and text
                            // fix for height and width to ensure edit text remains visible for large files.
                            //_imgFile.Visible = true;
                        }
                    }
                }
                
            }
            catch (System.IO.IOException err)
            {
                _errorMessage = err.Message;
            }
            //no changes ever made to the topic before the attachment was made
            if (Request.Form["PostBox"] == null)
            {
                Request.Form["PostBox"] = OriginalTopicText;
            }

        }

        private string GenerateNewCaptchaCode()
        {
            string code = CaptchaImage.GenerateRandomCode();
            string encryptedCode = Security.Encrypt(code, FlexWikiWebApplication.ApplicationConfiguration.CaptchaKey);
            return encryptedCode;
        }
        private bool IsCaptchaRequired()
        {
            CaptchaRequired requirement = FlexWikiWebApplication.ApplicationConfiguration.RequireCaptchaOnEdit;

            if (requirement == CaptchaRequired.Always)
            {
                return true;
            }

            if (requirement == CaptchaRequired.IfAnonymous)
            {
                return !(User.Identity.IsAuthenticated);
            }

            if (requirement == CaptchaRequired.WhenOverLinkThreshold)
            {
                if (!IsPost)
                {
                    return false; 
                }

                string submitted = PostedTopicText;
                string original = OriginalTopicText;

                int submittedLinkCount = CountLinks(submitted);
                int originalLinkCount = CountLinks(original);

                int delta = submittedLinkCount - originalLinkCount;

                if (delta >= FlexWikiWebApplication.ApplicationConfiguration.CaptchaLinkThreshold)
                {
                    return true; 
                }
            }


            return false;
        }
        private bool IsImageFile(string fullPath)
        {
            bool blnIsImage = false;
            fullPath = fullPath.ToUpper();
            string extensionName = Path.GetExtension(fullPath);
            if ((extensionName == ".JPG") || (extensionName == ".GIF") || (extensionName == ".PNG"))
            {
                blnIsImage = true;
            }
            return blnIsImage;
        }
        protected void LogBannedAttempt()
        {
            string to = FlexWikiWebApplication.ApplicationConfiguration.SendBanNotificationsToMailAddress;
            if (to == null || to == "")
            {
                return;
            }

            HtmlStringWriter w = new HtmlStringWriter();
            w.WritePara(String.Format("{0} attempted to post a change with banned content to the topic {1} on the FlexWiki site at {2}.",
                HtmlStringWriter.Escape(VisitorIdentityString), HtmlStringWriter.Escape(TheTopic.DottedName), HtmlStringWriter.Escape((Request.Url.Host))));
            w.WritePara("Banned content includes:");
            w.WriteStartUnorderedList();
            string proposed = PostedTopicText;
            foreach (string each in Federation.BlacklistedExternalLinkPrefixes)
            {
                if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
                {
                    w.WriteListItem(HtmlStringWriter.Escape(each));
                }
            }
            w.WriteEndUnorderedList();

            string from = "noreply_spam_report@" + Request.Url.Host;
            MailMessage msg = new MailMessage(from, to);
            msg.Subject = "Banned content post attempt from " + VisitorIdentityString;
            msg.IsBodyHtml = true;
            msg.Body = w.ToString();
            SendMail(msg);
        }
        private void ProcessPost()
        {
            if (!IsAttachment)
            {

                ProcessSave(IsBack);
            }
            else
            {
                ShowEditPage(true);
            }
        }
        protected string ReturnIconForAttach(string fileName)
        {
            string iconHref = "";
            fileName = fileName.ToLower();
            string extensionName = Path.GetExtension(fileName);

            foreach (AttachmentIconConfiguration attIcon in FlexWikiWebApplication.ApplicationConfiguration.AttachmentIcons)
            {
                if (extensionName == attIcon.IconKey)
                {
                    iconHref = attIcon.Href;
                    break;
                }
            }
            if (String.IsNullOrEmpty(iconHref))
            {
                iconHref = "page_white_text.png";
            }
            return iconHref;
        }
        private string ReturnDirectoryToWriteTo(string fileName)
        {
            // get the root        directory for uploaded files
            
            string rootUploadDir = FlexWikiWebApplication.ApplicationConfiguration.ContentUploadPath;
            string returnDirectory = "";
            fileName = fileName.ToLower();
            string extensionName = Path.GetExtension(fileName);
            switch(extensionName)
            {
                case ".jpg":
                case ".gif":
                case ".png":
                    returnDirectory = Path.Combine(rootUploadDir, "images");
                    break;
                case ".htm":
                case ".html":
                    returnDirectory = Path.Combine(rootUploadDir, "html");
                    break;
                case ".doc":
                    returnDirectory = Path.Combine(rootUploadDir, "doc");
                    break;
                default:
                    returnDirectory = Path.Combine(rootUploadDir, "doc");
                    break;
            }
            return returnDirectory;
        }
        private void ShowEditPage()
        {
            ShowEditPage(false); 
        }
        private void ShowEditPage(bool preserveContent)
        {
            StringBuilder strbldr = new StringBuilder();

            //Response.Write("<body onload=\"javascript:ResizeEditBox()\" onresize=\"javascript:ResizeEditBox()\" class=\"EditBody\" width=\"100%\" height=\"100%\" scroll=\"no\">");
            strbldr.AppendLine("<body onload=\"javascript:ResizeEditBox()\" >");

            strbldr.AppendLine("<table width=\"100%\" id=\"MasterTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td valign=\"top\">");
            strbldr.AppendLine("<div id=\"MainRegion\" class=\"EditMain\">");

		    strbldr.AppendLine("<div style=\"display: none\">");
			strbldr.AppendLine("<form id=\"Form2\" method=\"post\" target=\"previewWindow\" action=\"Preview.aspx\">");
			strbldr.AppendLine("<textarea id=\"body\" name=\"body\" rows=\"20\" cols=\"60\"></textarea>");
			strbldr.AppendFormat("<input  type=\"text\" id=\"Text1\" name=\"defaultNamespace\" value =\"{0}\" />", TheTopic.Namespace);
			strbldr.AppendFormat("<input  type=\"text\" id=\"Text2\" name=\"topic\" value =\"{0}\" />", TheTopic.LocalName);

			strbldr.AppendLine("</form>");
		    strbldr.AppendLine("</div>");
		    strbldr.AppendLine("<div class=\"EditZone\" id=\"EditZone\" >");
			strbldr.AppendLine("<form id=\"Form1\" method=\"post\" action=\"\">");
			strbldr.AppendLine("<textarea class=\"EditBox\" onkeydown=\"javascript:textarea_OnKeyPress(event)\" rows=\"20\" cols=\"50\" id=\"EditBox\" name=\"EditBox\" onfocus=\"javascript:textArea_OnFocus(event)\" onblur=\"javascript:textArea_OnBlur(event)\">");
            Response.Write(strbldr.ToString());

            string defaultContent = @"
Check out the formatting tips on the right for help formatting and making links.

Use the template below:

----

Summary: add a one or paragraph summary or description of what's discussed here;  put yours after 'Summary:'

Add your wiki text here.

";
            string content = null;
			bool isValidName = Formatter.extractWikiNames.IsMatch("["+TheTopic.Name.LocalName+"]");
			if (!isValidName)
			{
				content = String.Format("Topic Name not valid for this wiki: {0}\nPlease go back and choose a different name.",TheTopic.Name);
			}
            if (Federation.TopicExists(TheTopic))
            {
                content = Federation.Read(TheTopic);
            }
            if (IsPost && IsBanned)
            {
                content = PostedTopicText;		// preserve what they asked for, even though we won't let them save
            }

            if (preserveContent)
            {
                content = PostedTopicText;
            }

            #region Build up the list of templates and set the content accordingly
            string templateSelect = string.Empty;
            NamespaceManager currentContentBase = Federation.NamespaceManagerForNamespace(TheTopic.Namespace);

            // Process the topics looking for topics beginning with '_Template'.
            QualifiedTopicNameCollection templates = new QualifiedTopicNameCollection();
            foreach (QualifiedTopicName topic in currentContentBase.AllTopics(ImportPolicy.DoNotIncludeImports))
            {
                if (topic.LocalName.StartsWith("_Template"))
                {
                    templates.Add(topic);

                    if ("_templatedefault" == topic.LocalName.ToLower())
                    {
                        defaultContent = Federation.Read(topic);
                    }
                }
            }

            if (0 != templates.Count)
            {
                // Build up a combo box for selecting the template.
                StringBuilder builder = new StringBuilder("<select name=\"templateSelect\" id=\"templateSelect\">\n");
                foreach (QualifiedTopicName topic in templates)
                {
                    builder.AppendLine("\t<option value=\"");
                    builder.AppendLine(Formatter.EscapeHTML(Federation.Read(topic)));
                    builder.AppendLine("\">");
                    string topicStart = "_ Template";
                    string name = topic.FormattedName;
                    builder.AppendLine(name.Substring(topicStart.Length));
                    builder.AppendLine("</option>\n");
                }
                builder.AppendLine("</select>\n");

                templateSelect = builder.ToString();

                // Check to see if a template was specified in the request.
                if (null != this.Request.QueryString["template"])
                {
                    string templateName = this.Request["template"];
                    QualifiedTopicRevision topicVersionKey = new QualifiedTopicRevision(templateName, currentContentBase.Namespace);
                    if ((content == null) && (true == currentContentBase.TopicExists(topicVersionKey.LocalName,
                        ImportPolicy.DoNotIncludeImports)))
                    {
                        content = Federation.Read(topicVersionKey);
                    }
                }
            }
            #endregion

            if (content == null)
            {
                content = defaultContent;
            }

            Response.Write(Formatter.EscapeHTML(content));

            Response.Write(@"</textarea>");
			bool isWritable = this.IsWritable && isValidName;
            if (isWritable)
            {
                Response.Write("<input type=\"text\" style=\"display:none\" name=\"CaptchaEnteredSubmitted\" value =\"\" />");
                Response.Write("<input type=\"text\" style=\"display:none\" name=\"CaptchaContextSubmitted\" value =\"\" />");
                Response.Write("<input type=\"text\" style=\"display:none\" id=\"SaveButtonPressed\" name=\"SaveButtonPressed\" value=\"\" />");
                Response.Write("<input type=\"text\" style=\"display:none\" name=\"UserSuppliedName\" value =\"" + Formatter.EscapeHTML(UserPrefix == null ? "" : UserPrefix) + "\" />");
                if (Federation.TopicExists(TheTopic))
                {
                    Response.Write("<input type=\"text\" style=\"display:none\" name=\"TopicLastWrite\" value =\"" + Formatter.EscapeHTML(Federation.GetTopicModificationTime(TheTopic).ToString("s")) + "\" />");
                }
                Response.Write("<input type=\"text\" style=\"display:none\" name=\"Topic\" value =\"" + Formatter.EscapeHTML(TheTopic.ToString()) + "\" />");
                if (ReturnTopic != null)
                {
                    Response.Write("<input type=\"text\" style=\"display:none\" name=\"ReturnTopic\" value =\"" + Formatter.EscapeHTML(ReturnTopic) + "\" />");
                }
            }

            Response.Write(@"</form></div>");

            Response.Write("</div></td>");
            Response.Write("<td valign=\"top\" id=\"Sidebar\" class=\"Sidebar\">");

            Response.Write("<table style=\"height: 100%\"><tr><td height=\"100%\" valign=\"top\">");

            /////////////////////////////
            OpenPane(Response.Output, "Edit&nbsp;" + BELString.MaxLengthString2(Formatter.EscapeHTML(TheTopic.ToString()), 20, "..."));
            if (isWritable)
            {
                NamespaceManager storeManager = Federation.NamespaceManagerForTopic(TheTopic);
                if (storeManager.TopicExists(TheTopic.LocalName, ImportPolicy.DoNotIncludeImports))
                {
                    Response.Write("Make your changes to the text on the left and then select Save.");
                }
                else
                {
                    Response.Write(@"
			<div class=""CreateTopicWarning"">
				You are about to create a new topic called <b>" + TheTopic.LocalName + @"</b> in the <b>" +
                        storeManager.FriendlyTitle + @"</b> namespace.");
                    Response.Write("<P>Please be sure you are creating this topic in the desired namespace.</p>");
                    Response.Write(@"</div>");
                }
            }
            else
            {
                Response.Write("<span class=\"ReadOnlyStripe\">You do not have permission to change this topic.</span>");
            }

            ClosePane(Response.Output);


            if (IsConflictingChange)
            {
                OpenPane(Response.Output, "Conflicting Change");
                Response.Write("<div class=\"ConflictingChange\">Your change can not be saved.</div>");
                Response.Write("The topic has been changed since you started to edit it and if you saved your changes, the other changes would be lost.");
                Response.Write(" Please save your changes somewhere and edit again (no merge functionality yet).  You are now being shown the new version on the left.  To recover your previous edits, use the Back button.");
                ClosePane(Response.Output);
            }

            if (IsPost && IsBanned)
            {
                OpenPane(Response.Output, "Banned URLs");
                Response.Write("<div class=\"BannedChange\">Your change can not be saved.</div>");
                Response.Write("The changes you are trying to save include banned URLs.");
                ClosePane(Response.Output);
                LogBannedAttempt();
            }

            if (Federation.NoFollowExternalHyperlinks)
            {
                OpenPane(Response.Output, "External Hyperlinks");
                Response.Write("<img src=\"" + TheLinkMaker.LinkToImage("images/NoFollowNoSpam.gif") + "\" align=\"right\" />External hyperlinks will not be indexed by search engines.");
                ClosePane(Response.Output);
            }


            ///////////////////////////////
            if (isWritable)
            {
                OpenPane(Response.Output, "Attribution");
                if (User.Identity.IsAuthenticated)
                {
                    Response.Write("Your edit will be attributed to: <b>" + Formatter.EscapeHTML(VisitorIdentityString) + "</b>.");
                }
                else
                {
                    Response.Write("Your edit will be attributed to: <b>" + Formatter.EscapeHTML(VisitorIdentityString) + "</b>.<br />");

                    Response.Write("<div id=\"ShowAttribution\" style=\"display: block\"><a onclick=\"javascript:Swap('ShowAttribution', 'HideAttribution')\">Change this...</a></div>");
                    Response.Write("<div id=\"HideAttribution\" style=\"display: none\">");
                    Response.Write("<a onclick=\"javascript:Swap('ShowAttribution', 'HideAttribution')\">Hide this...</a><br />");

                    Response.Write("You can change part of this by entering your preferred user identity here (e.g., an email address):<br />");
                    Response.Write(@"<input style=""font-size: x-small"" type=""text"" id=""UserNameEntryField"" value =""" +
                        (UserPrefix == null ? "" : Formatter.EscapeHTML(UserPrefix)) + "\" />");
                    Response.Write("</div>");

                }
                ClosePane(Response.Output);
            }

            ///////////////////////////////

            if (isWritable)
            {
                OpenPane(Response.Output, "Formatting Tips");
                Response.Write("<div id=\"ShowTips\" style=\"display: block\"><a onclick=\"javascript:Swap('ShowTips', 'HideTips')\">Show tips...</a></div>");
                Response.Write("<div id=\"HideTips\" style=\"display: none\">");
                Response.Write("<a onclick=\"javascript:Swap('ShowTips', 'HideTips')\">Hide tips...</a><br />");

                Response.Write("Click on a subject for more information about formatting rules: ");
                WriteTip("tip_boldtip", "Bold");
                WriteTip("tip_italicstip", "Italics");
                WriteTip("tip_headingtip", "Headings");
                WriteTip("tip_hypertip", "Hyperlinks");
                WriteTip("tip_linestip", "Lines");
                WriteTip("tip_liststip", "Lists");
                WriteTip("tip_tablestip", "Tables");
                WriteTip("tip_emoticonstip", "Emoticons");
                WriteTip("tip_pretip", "Preformatted");
                WriteTip("tip_imagetip", "Images");
                WriteTip("tip_proptip", "Properties");

                strbldr = new StringBuilder();
                strbldr.AppendLine("<br /><div class=\"TipArea\" id=\"TipArea\"></div>");

                strbldr.AppendLine("<div style=\"display: none\">");
		        strbldr.AppendLine("<div id=\"tip_proptip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("A line that starts with a wiki word and a colon identifies a property.");
				strbldr.AppendLine("The value of the property is everything on the line after the colon.");
				strbldr.AppendLine("Multiline imports use PropertyName:[ and then multiple lines and then ] on a");
				strbldr.AppendLine("blank line to mark the end.");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_imagetip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Any URL that ends with .gif, .jpeg, .jpg or .png will be turned into an image");
				strbldr.AppendLine("tag to display the actual image.");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_pretip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Any line that starts with at least one space will be fixed-width formatted.");
				strbldr.AppendLine("Good for code and simple tables.");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_liststip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Start a line with a tab (or 8 spaces) followed by a star \"*\".");
				strbldr.AppendLine("Two tabs (or 16 spaces) indents to the next level, etc.");
				strbldr.AppendLine("For ordered lists, use \"1.\" instead of \"*\"");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_linestip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Four hyphens makes a horizontal rule.");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_boldtip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Surround the text with three ticks (''').");
				strbldr.AppendLine("For example, '''<b>this text will be bold</b>'''");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_italicstip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Surround the text with two ticks ('').");
				strbldr.AppendLine("For example, ''<i>this text will be italic</i>''");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_headingtip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Bang (!) at the start of a line for H1.");
				strbldr.AppendLine("Bang Bang (!!) at the start of a line for H2.");
				strbldr.AppendLine("And so on...");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_hypertip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("Any PascalCased word becomes a link.");
				strbldr.AppendLine("Surrounding a word with square brackets [ word ] will make non-pascalcased");
				strbldr.AppendLine("words into links; generally this is considered \"odd\".");
				strbldr.AppendLine("Any URL becomes a link (http://www.msn.com)");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_emoticonstip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("All the common emoticons like :-) and :-( are turned into the apprpriate");
				strbldr.AppendLine("graphical images (like in messenger).");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
		        strbldr.AppendLine("<div id=\"tip_tablestip\">");
			    strbldr.AppendLine("<div class=\"tipBody\">");
				strbldr.AppendLine("A line that starts and ends with || is a table row.  Cells are divided by ||.");
				strbldr.AppendLine("For example: <br />");
				strbldr.AppendLine("||Region || Sales||<br />");
				strbldr.AppendLine("||East || $100||<br />");
				strbldr.AppendLine("||West || $100||<br />");
			    strbldr.AppendLine("</div>");
		        strbldr.AppendLine("</div>");
                strbldr.AppendLine("</div>");


                strbldr.AppendLine("</div>");
                Response.Write(strbldr.ToString());
                ClosePane(Response.Output);
            }

            //////////////////////////////
            if (isWritable && templateSelect.Length > 0)
            {
                // Render the template selection dropdown.
                OpenPane(Response.Output, "Topic Templates");
                Response.Write("Select a new template:<br/>\n");
                Response.Write(templateSelect);
                Response.Write("&nbsp;<input type=\"image\" src=\"" + TheLinkMaker.LinkToImage("images/go-dark.gif") + "\" title=\"Change Template\" onclick=\"javascript:ChangeTemplate('templateSelect');\" />");
                ClosePane(Response.Output);
            }

            //////////////////////////////

            strbldr = new StringBuilder();
            if (isWritable)
            {
                if (IsCaptchaRequired())
                {
                    strbldr.AppendLine("<table class=\"SidebarTile\" cellspacing=\"0\" cellpadding=\"2\" border=\"0\">");
                    strbldr.AppendLine("<tbody>");
                    strbldr.AppendLine("<tr>");
                    strbldr.AppendLine("<td class=\"SidebarTileTitle\" valign=\"middle\">Spam Prevention</td>"); 
                    strbldr.AppendLine("</tr>");
                    strbldr.AppendLine("<tr class=\"SidebarTileBody\" valign=\"middle\">");
                    strbldr.AppendLine("<td>");
                    strbldr.AppendLine("<div>"); 
                    string captchaCode = GenerateNewCaptchaCode();
                    string aboutHref = TheLinkMaker.SimpleLinkTo("AboutCaptcha.html");
                    if (IsPost && !IsCaptchaVerified)
                    {
                        strbldr.AppendLine("<span class=\"ErrorMessageBody\">To prevent automated spam attacks, you must properly enter the code shown below. Please enter the number you see in the box and then click Save. </span>"); 
                    }
                    else
                    {
                        strbldr.AppendLine("<span>Before saving, please enter the code you see below. </span>"); 
                    }
                    strbldr.AppendLine("<br />"); 
                    strbldr.AppendFormat("<a href=\"{0}\" target=\"_blank\">What's this?</a>", aboutHref);
                    strbldr.AppendLine("<br />");
                    string captchaHref = TheLinkMaker.SimpleLinkTo("CaptchaImage.ashx/" + captchaCode);
                    strbldr.AppendFormat("<img src=\"{0}\" alt=\"Enter this code in the box to the right.\" />", captchaHref);
                    strbldr.AppendLine("<br />");
                    strbldr.AppendFormat("<input type=\"hidden\" name=\"CaptchaContext\" id=\"CaptchaContext\" value=\"{0}\" />", captchaCode);
                    strbldr.AppendLine("<input type=\"text\" name=\"CaptchaEntered\" id=\"CaptchaEntered\" value=\"\" />");
                    strbldr.AppendLine("</div>"); 
                    strbldr.AppendLine("</td>"); 
                    strbldr.AppendLine("</tr>"); 
                    strbldr.AppendLine("</tbody>");
                    strbldr.AppendLine("</table>");
                }

                // generate cancel, save, search, preview, and Save&Return buttons
                strbldr.AppendLine("<div style=\"margin-top: 12px; text-align: center\"><table>");
                strbldr.AppendLine("<tr><td><button onclick=\"javascript:Cancel()\" name=\"CancelButton\">Cancel</button></td>");
                strbldr.AppendLine("<td><button onclick=\"javascript:Save()\" name=\"SaveButton\">Save</button></td></tr>");
                strbldr.AppendLine("<tr><td><button onclick=\"javascript:search()\" id=\"button3\">Search</button></td>");
                strbldr.AppendLine("<td><button onclick=\"javascript:preview()\" id=\"button1\">Preview</button></td></tr>");

                if (ReturnTopic != null)
                {
                    strbldr.AppendLine("<tr><td colspan=\"2\"><button onclick=\"javascript:SaveAndReturn()\"  name=\"SaveButton\">Save and Back</button></td></tr>");
                }

            }

            strbldr.AppendLine("</table></div>");

            if (IsUploadable && isWritable)
            {
                strbldr.AppendLine("<div id=\"AttachFile\" style=\"display: block\">");
            }
            else
            {
                strbldr.AppendLine("<div id=\"AttachFile\" style=\"display: none\">");
            }
            strbldr.AppendLine("<table cellspacing=\"0\" cellpadding=\"2\" border=\"0\" class=\"SidebarTile\">");
            strbldr.AppendLine("<tr>");
            strbldr.AppendLine("<td valign=\"middle\" class=\"SidebarTileTitle\">");
            strbldr.AppendLine("File Upload &amp; Attachment");
            strbldr.AppendLine("</td>");
            strbldr.AppendLine("</tr>");
            strbldr.AppendLine("<tr class=\"SidebarTileBody\"><td>");

            if (IsAttachment)
            {
                strbldr.AppendLine("<div id=\"ShowAttachmentControls\" style=\"display: none\" >");
                strbldr.AppendLine("<a onclick=\"javascript:Swap('ShowAttachmentControls', 'HideAttachmentControls')\">Show Attachment Controls...</a> </div>");
                strbldr.AppendLine("<div id=\"HideAttachmentControls\" style=\"display: block\" >");
                strbldr.AppendLine("<a onclick=\"javascript:Swap('HideAttachmentControls', 'ShowAttachmentControls')\" >Hide Attachment Controls...</a>");
            }
            else
            {
                strbldr.AppendLine("<div id=\"ShowAttachmentControls\" style=\"display: block\" >");
                strbldr.AppendLine("<a onclick=\"javascript:Swap('ShowAttachmentControls', 'HideAttachmentControls')\">Show Attachment Controls...</a> </div>");
                strbldr.AppendLine("<div id=\"HideAttachmentControls\" style=\"display: none\" >");
                strbldr.AppendLine("<a onclick=\"javascript:Swap('HideAttachmentControls', 'ShowAttachmentControls')\" >Hide Attachment Controls...</a>");
            }
            strbldr.AppendLine("<table><tr><td>");
            Response.Write(strbldr.ToString());
            

        }

        // Writes file to specified path
        private void WriteToFile(string filePath, ref byte[] Buffer)
        {
            LogEvent ev;
            LogEventType evType = LogEventType.UploadFile;
            ev = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, filePath, evType);

            try
            {
                FileStream newFile = new FileStream(filePath, FileMode.Create);
                newFile.Write(Buffer, 0, Buffer.Length);
                newFile.Close();
            }
            finally
            {
                ev.Record();
            }
        }
        private void WriteTip(string id, string text)
        {
            Response.Write("<span onclick=\"javascript:ShowTip('" + id + "')\"><b>" + text + "</b></span> ");
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
