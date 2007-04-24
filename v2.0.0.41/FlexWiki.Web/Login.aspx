<%@ Page Language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Login" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html>
<head>
    <title>Wiki Logon</title>
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
    <%= InsertStylesheetReferences() %>
</head>
<body class="Dialog">
    <form runat="server">
        <asp:Login ID="Login1" runat="server" BackColor="#E3EAEB" BorderColor="#E6E2D8" BorderPadding="4"
            BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em"
            ForeColor="#333333" TextLayout="TextOnTop" OnLoginError="Login1_LoginError" 
            OnLoggedIn="Login1_LoggedIn" 
            PasswordRecoveryText="Forgot your password?" PasswordRecoveryUrl="~/RecoverPassword.aspx" 
            CreateUserText="Register as a new user" CreateUserUrl="~/RegisterUser.aspx">
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <TextBoxStyle Font-Size="0.8em" />
            <LoginButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55" />
        </asp:Login>
        <asp:HyperLink runat="server" ID="ReturnLink" Visible="false" />
    </form>
</body>
</html>
