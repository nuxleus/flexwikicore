<%@ Page Language="c#" Codebehind="RecoverPassword.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.RecoverPassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
    <title>Recover Forgotten Password</title>
	<%= BuildPageOne() %>
    <div class="Dialog">
    <form runat="server" action="">
        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="#E3EAEB" BorderColor="#E6E2D8"
            BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
            Font-Size="14pt">
            <SuccessTemplate>
                Your password has been sent to you. <br />
                <asp:HyperLink runat="server" ID="LoginLink">Return to login</asp:HyperLink><br />
                <asp:HyperLink runat="server" ID="ReturnLink">Return to the wiki</asp:HyperLink>
            </SuccessTemplate>
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <SuccessTextStyle Font-Bold="True" ForeColor="#1C5E55" />
            <TextBoxStyle Font-Size="0.8em" />
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
            <SubmitButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55" />
        </asp:PasswordRecovery>
    </form>
    </div>
	<%= BuildPageTwo() %>
