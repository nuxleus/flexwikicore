<%@ Page Language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
    <title>Wiki Logon</title>
    <%= InsertStylesheetReferences() %>
    <%= InsertFavicon() %>
    <%InsertScripts(); %>
</head>
<body>
	<%InsertLeftTopBorders(); %>
	<div class="Dialog">
    <form runat="server" action="">
        <asp:Login ID="Login1" 
                runat="server" 
                BackColor="#E3EAEB" 
                BorderColor="#E6E2D8" 
                BorderPadding="4"
                BorderStyle="Solid" 
                BorderWidth="1px" 
                Font-Names="Verdana" 
                Font-Size="14pt"
                ForeColor="#333333" 
                TextLayout="TextOnTop" 
                OnLoginError="Login1_LoginError" 
                OnLoggedIn="Login1_LoggedIn"
                PasswordRecoveryText="Forgot your password?" 
                PasswordRecoveryUrl="~/RecoverPassword.aspx"
                CreateUserText="Register as a new user" 
                CreateUserUrl="~/RegisterUser.aspx">
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <TextBoxStyle Font-Size="0.8em" />
            <LoginButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55" />
        </asp:Login>
        <asp:HyperLink runat="server" ID="ReturnLink" Visible="false" />
    </form>
    </div>
	<%InsertRightBottomBorders(); %>
</body>
</html>
