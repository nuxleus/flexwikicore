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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlexWiki.Web
{
    public class PasswordReset : BasePage
    {
        protected Wizard PasswordResetWizard;

        protected TextBox Step1UserIdTxt;
        protected TextBox Step1UserEmailTxt;
        protected TextBox Step2SecurityQuestionTxt;
        protected TextBox Step2SecurityAnswerTxt;
        protected TextBox Step3Pwd1Txt;
        protected TextBox Step3Pwd2Txt;

        protected Literal Step2UserIdLit;
        protected Literal Step2UserEmailLit;
        protected Literal Step3UserIdLit;
        protected Literal Step3UserEmailLit;
        protected Literal Step4MsgLit;

        protected HiddenField PasswordFormat;
        protected HiddenField Step1Status;
        protected HiddenField Step2Status;
        protected HiddenField Step3Status;
        protected HiddenField UserProviderKey;
        protected HiddenField Salt;

        protected string _salt;
        protected string _userName;
        protected string _userEmail;
        protected string _userKey;

        protected string _connect;

        protected bool _error = true;

        protected MembershipUser _user = null;

        private bool endProcess = false;
        private bool templatedPage = false;
        private string template;
        private int lineCnt;


        internal string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0) // MembershipPasswordFormat.Clear
                return pass;

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            byte[] bRet = null;

            System.Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            System.Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            if (passwordFormat == 1)
            { // MembershipPasswordFormat.Hashed
                HashAlgorithm s = HashAlgorithm.Create( Membership.HashAlgorithmType );
                bRet = s.ComputeHash(bAll);
            } else
            {
                //bRet = EncryptPassword( bAll );
                throw new NotImplementedException("EncryptPassword method not implemented");
            }

            return Convert.ToBase64String(bRet);
        }        

        private void Page_Load(object sender, System.EventArgs e)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["ReturnURL"]))
            {
                PasswordResetWizard.CancelDestinationPageUrl = Request.QueryString["ReturnURL"];
                PasswordResetWizard.FinishDestinationPageUrl = Request.QueryString["ReturnURL"];
            }
            else
            {
                PasswordResetWizard.CancelDestinationPageUrl = "~/default.aspx";
                PasswordResetWizard.FinishDestinationPageUrl = "~/default.aspx";
            }
            //_connect = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
            //SqlConnection conn = new SqlConnection(_connect);
            //SqlCommand cmd = new SqlCommand();
            if (String.IsNullOrEmpty(Step1Status.Value))
            {
                Step1Status.Value = "Init";
                Step2Status.Value = "Init";
                Step3Status.Value = "Init";
            }
            
        }

        protected void GetMembershipUser()
        {
            if (!String.IsNullOrEmpty(Step1UserEmailTxt.Text))
            {
                _userEmail = Step1UserEmailTxt.Text;
                _userName = Membership.GetUserNameByEmail(_userEmail);
               
            }
            if (!String.IsNullOrEmpty(Step1UserIdTxt.Text))
            {
                _userName = Step1UserIdTxt.Text;
            }
            if (!String.IsNullOrEmpty(_userName))
            {
                try
                {
                    _user = Membership.GetUser(_userName, false);
                }
                catch (ArgumentException)
                {
                    Step1Status.Value = "Error";
                    PasswordResetWizard.ActiveStepIndex = 3;
                }
            }
            if (_user != null)
            {
                Step1Status.Value = "Success";
                Step1UserEmailTxt.Text = _user.Email;
                Step1UserIdTxt.Text = _user.UserName;
                Step2UserEmailLit.Text = _user.Email;
                Step2UserIdLit.Text = _user.UserName;
                Step2SecurityQuestionTxt.Text = _user.PasswordQuestion;
                Step3UserEmailLit.Text = _user.Email;
                Step3UserIdLit.Text = _user.UserName;
                UserProviderKey.Value = _user.ProviderUserKey.ToString();
            }
            else
            {
                Step1Status.Value = "Error";
                PasswordResetWizard.ActiveStepIndex = 3;
            }
        }

        protected void ValidatePasswordAnswer()
        {
            if (!String.IsNullOrEmpty(Step2SecurityAnswerTxt.Text))
            {
                SqlDataReader reader = null;
                string encryptedPasswordAnswer = "";
                StringBuilder sql = new StringBuilder();
                sql.Append("Select PasswordAnswer, PasswordSalt from aspnet_Membership M ");
                sql.Append("INNER JOIN aspnet_Applications A ");
                sql.Append("ON A.ApplicationId = M.ApplicationId ");
                sql.Append("Where UserId = @UserId ");
                sql.Append("and LoweredApplicationName = @ApplicationName");

                SqlCommand cmd = new SqlCommand(sql.ToString());
                string conn = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
                SqlConnection sqlConn = new SqlConnection(conn);
                sqlConn.Open();
                cmd.Connection = sqlConn;
     
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.Add("@UserId", System.Data.SqlDbType.UniqueIdentifier);
                cmd.Parameters["@UserId"].Value = new Guid(UserProviderKey.Value);

                cmd.Parameters.Add("@ApplicationName", System.Data.SqlDbType.NVarChar, 256);
                cmd.Parameters["@ApplicationName"].Value = Membership.ApplicationName.ToLower();

                try
                {
                    reader = cmd.ExecuteReader();
                }
                catch (SqlException)
                {
                    Step2Status.Value = "Error";
                    PasswordResetWizard.ActiveStepIndex = 3;
                }
                if (reader.HasRows)
                {
                    reader.Read();
                    string passwordFormat = Membership.Provider.PasswordFormat.ToString();
                    int pwdFormat = 0;
                    switch (passwordFormat)
                    {
                        case "Clear":
                            pwdFormat = 0;
                            break;
                        case "Hashed":
                            pwdFormat = 1;
                            break;
                        case "Encrypted":
                            pwdFormat = 2;
                            break;
                    }
                    string salt = (String)(reader["PasswordSalt"]);
                    Salt.Value = salt;
                    PasswordFormat.Value = pwdFormat.ToString();
                    encryptedPasswordAnswer = (String)(reader["PasswordAnswer"]);
                    reader.Close();
                    string testPasswordAnswer = EncodePassword(Step2SecurityAnswerTxt.Text.ToLower(CultureInfo.InvariantCulture), pwdFormat, salt);


                    if (String.Equals(encryptedPasswordAnswer, testPasswordAnswer))
                    {
                        Step2Status.Value = "Success";
                    }
                    else
                    {
                        Step2Status.Value = "Error";
                        PasswordResetWizard.ActiveStepIndex = 3;
                    }
                }
                else
                {
                    Step2Status.Value = "Error";
                    PasswordResetWizard.ActiveStepIndex = 3;
                }
            }
            else
            {
                Step2Status.Value = "Error";
                PasswordResetWizard.ActiveStepIndex = 3;
            }

        }

        protected void UpdatePassword()
        {
            string firstPwd = Step3Pwd1Txt.Text;
            string secondPwd = Step3Pwd2Txt.Text;
            if (String.Equals(firstPwd, secondPwd))
            {
                int minLength = Membership.MinRequiredPasswordLength;
                int minNonAlpha = Membership.MinRequiredNonAlphanumericCharacters;

                if (secondPwd.Length >= minLength)
                {
                    if (Regex.Matches(secondPwd, @"\p{P}").Count >= minNonAlpha)
                    {
                        SqlCommand cmd = new SqlCommand();
                        string conn = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
                        SqlConnection sqlConn = new SqlConnection(conn);
                        sqlConn.Open();
                        cmd.Connection = sqlConn;
                        cmd.CommandText = "aspnet_Membership_SetPassword";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ApplicationName", System.Data.SqlDbType.NVarChar, 256);
                        cmd.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar, 256);
                        cmd.Parameters.Add("@NewPassword", System.Data.SqlDbType.NVarChar, 256);
                        cmd.Parameters.Add("@PasswordSalt", System.Data.SqlDbType.NVarChar, 256);
                        cmd.Parameters.Add("@CurrentTimeUtc", System.Data.SqlDbType.DateTime);
                        cmd.Parameters.Add("@PasswordFormat", SqlDbType.Int);

                        short pwdFormat = 0;
                        Int16.TryParse(PasswordFormat.Value, out pwdFormat);

                        cmd.Parameters["@ApplicationName"].Value = Membership.ApplicationName;
                        cmd.Parameters["@UserName"].Value = Step3UserIdLit.Text;
                        cmd.Parameters["@NewPassword"].Value = EncodePassword(secondPwd, (int)pwdFormat, Salt.Value);
                        cmd.Parameters["@PasswordSalt"].Value = Salt.Value;
                        cmd.Parameters["@CurrentTimeUtc"].Value = DateTime.Now.ToUniversalTime();

                        cmd.Parameters["@PasswordFormat"].Value = (int)pwdFormat;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 1)
                        {
                            Step3Status.Value = "Success";
                        }
                        else
                        {
                            Step3Status.Value = "Error";
                            PasswordResetWizard.ActiveStepIndex = 3;
                        }
                    }
                    else
                    {
                        Step3Status.Value = "MinNonAlpha";
                        PasswordResetWizard.ActiveStepIndex = 3;
                    }
                }
                else
                {
                    Step3Status.Value = "MinLength";
                    PasswordResetWizard.ActiveStepIndex = 3;
                }
            }
            else
            {
                Step3Status.Value = "Error";
                PasswordResetWizard.ActiveStepIndex = 3;
            }
            FinishWizard();
        }

        protected void FinishWizard()
        {
            StringBuilder msg = new StringBuilder();
            if (Step1Status.Value == "Init")
            {
                msg.AppendLine("User Identification step has not been entered<br />");
            }
            else if (Step1Status.Value == "Error")
            {
                msg.AppendLine("User Identification step has not been completed successfully<br />");
            }
            if (Step2Status.Value == "Init")
            {
                msg.AppendLine("User Verification step has not been entered<br />");
            }
            else if (Step2Status.Value == "Error")
            {
                msg.AppendLine("User Verification step has not been completed successfully<br />");
            }
            if (Step3Status.Value == "Init")
            {
                msg.AppendLine("User New Password step has not been entered<br />");
            }
            else if (Step3Status.Value == "Error")
            {
                msg.AppendLine("User New Password step has not been completed successfully<br />");
            }
            else if (Step3Status.Value == "MinLength")
            {
                msg.AppendLine("User New Password does not meet the minimum required length of - " + Membership.MinRequiredPasswordLength.ToString() + "<br />");
            }
            else if (Step3Status.Value == "MinNonAlpha")
            {
                msg.AppendLine("User New Password does not meet the minimum required number of punctuation characters of - " + Membership.MinRequiredNonAlphanumericCharacters.ToString() + "<br />");
            }
            Step4MsgLit.Text = msg.ToString();
        }

 
        protected void PasswordResetWizard_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (PasswordResetWizard.ActiveStepIndex)
            {
                case 0:
                    break;
                case 1:
                    GetMembershipUser();
                    break;
                case 2:
                    ValidatePasswordAnswer();
                    break;
                case 3:
                    _error = false;
                    if (Step1Status.Value == "Error" || Step2Status.Value == "Error" || Step3Status.Value == "Error")
                    {
                        _error = true;
                    }
                    if (Step1Status.Value == "Init" || Step2Status.Value == "Init")
                    {
                        _error = true;
                    }
                    if (_error)
                    {
                        FinishWizard();
                    }
                    else
                    {
                        UpdatePassword();
                    }
                break;
            }
        }
        protected string BuildPageOne()
        {

            StringBuilder strOutput = new StringBuilder();

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

            strOutput.Append(InsertLeftTopBorders());

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
