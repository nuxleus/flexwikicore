<%@ Page Language="c#" Codebehind="RecoverPassword.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.RecoverPassword" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Recover Forgotten Password</title>
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
    <%= InsertStylesheetReferences() %>
</head>
<body class="Dialog">
    <form runat="server">
        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="#E3EAEB" BorderColor="#E6E2D8"
            BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
            Font-Size="0.8em">
            <SuccessTemplate>
                Your password has been sent to you. <br />
                <asp:HyperLink runat="server" ID="LoginLink">Return to login</asp:HyperLink><br />
                <asp:HyperLink runat="server" ID="ReturnLink">Return to FlexWiki</asp:HyperLink>
            </SuccessTemplate>
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <SuccessTextStyle Font-Bold="True" ForeColor="#1C5E55" />
            <TextBoxStyle Font-Size="0.8em" />
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
            <SubmitButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55" />
        </asp:PasswordRecovery>
    </form>
</body>
</html>
