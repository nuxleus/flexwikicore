<%@ Page Language="c#" Codebehind="PasswordReset.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.PasswordReset" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
    <title>Reset Forgotten Password</title>
    </head>
    <%= BuildPageOne() %>
    <div id="TopicBody">
    <div class="Dialog">
      <form id="PasswordReset" runat="server" method="post">
        <asp:Wizard ID="PasswordResetWizard" runat="server" DisplayCancelButton="true" ActiveStepIndex="0" 
                    OnActiveStepChanged="PasswordResetWizard_ActiveStepChanged" 
                    DisplaySideBar="false" 
                     BackColor="#E3EAEB" BorderColor="#E6E2D8" BorderStyle="Solid" Font-Names="Verdana" Font-Size="14pt" ForeColor="#333333"
                     HeaderStyle-BackColor="#1C5E55" HeaderStyle-Font-Bold="true" HeaderStyle-Font-Size="0.9em" HeaderStyle-ForeColor="White"
                     CancelButtonStyle-BackColor="White" CancelButtonStyle-BorderColor="#C5BBAF" CancelButtonStyle-BorderStyle="Solid"
                     CancelButtonStyle-BorderWidth="1px" CancelButtonStyle-Font-Names="Verdana" CancelButtonStyle-Font-Size="0.8em"
                     CancelButtonStyle-ForeColor="#1C5E55"   >
            <WizardSteps>
                <asp:WizardStep ID="GetUserStep1" runat="server" Title="User Identification">
                  <asp:Table ID="Step1Tbl" runat="server">
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step1UserIdLbl" runat="server" Text="Provide UserId: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step1UserIdTxt" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server"></asp:TableCell>
                  <asp:TableCell runat="server">
                   <asp:Label ID="Step1Or" runat="server" Text="or" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step1UserEmailLbl" runat="server" Text="Provide User Email: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step1UserEmailTxt" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  </asp:Table>
                </asp:WizardStep>
                <asp:WizardStep ID="VerifyUserStep2" runat="server" Title="User Verification">
                  <asp:Table ID="Step2Tbl" runat="server">
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step2UserIdLbl" runat="server" Text="UserId Provided: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:Literal ID="Step2UserIdLit" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step2UserEmailLbl" runat="server" Text="User Email Provided: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                   <asp:Literal ID="Step2UserEmailLit" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step2SecurityQuestionLbl" runat="server" Text="Security Question for User: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step2SecurityQuestionTxt" runat="server"  ReadOnly="True" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step2SecurityAnswerLbl" runat="server" Text="Security Answer for User: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step2SecurityAnswerTxt" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  </asp:Table>
                </asp:WizardStep>
                <asp:WizardStep ID="ResetPasswordStep3" runat="server" Title="Reset Password">
                  <asp:Table ID="Step3Tbl" runat="server">
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step3UserIdLbl" runat="server" Text="UserId Provided: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:Literal ID="Step3UserIdLit" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step3UserEmailLbl" runat="server" Text="User Email Provided: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                   <asp:Literal ID="Step3UserEmailLit" runat="server" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step3Pwd1Lbl" runat="server" Text="Enter New Password: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step3Pwd1Txt" runat="server" TextMode="Password" />
                  </asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow runat="server">
                  <asp:TableCell runat="server">
                    <asp:Label ID="Step3Pwd2Lbl" runat="server" Text="Verify Password: " />
                  </asp:TableCell>
                  <asp:TableCell runat="server">
                    <asp:TextBox ID="Step3Pwd2Txt" runat="server"  TextMode="Password" />
                  </asp:TableCell>
                  </asp:TableRow>
                  </asp:Table>
                </asp:WizardStep>
                <asp:WizardStep ID="CompleteStep4" runat="server" Title="Reset Completed">
                 <asp:Literal ID="Step4MsgLit" runat="server" />
                 <asp:HiddenField ID="Step1Status" runat="server" />
                 <asp:HiddenField ID="Step2Status" runat="server" />
                 <asp:HiddenField ID="Step3Status" runat="server" />
                 <asp:HiddenField ID="UserProviderKey" runat="server" />
                 <asp:HiddenField ID="Salt" runat="server" />
                 <asp:HiddenField ID="PasswordFormat" runat="server" />
                </asp:WizardStep>
            </WizardSteps>
            <HeaderTemplate>
                <b>Password Reset Wizard - <%= PasswordResetWizard.ActiveStep.Title %></b>
                <br />
            </HeaderTemplate>
        </asp:Wizard>
      </form>
      </div>
      </div>
      </div>
      <%=BuildPageTwo() %>