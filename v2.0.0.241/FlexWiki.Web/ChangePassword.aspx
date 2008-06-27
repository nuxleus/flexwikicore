<%@ Page Language="c#" Codebehind="ChangePassword.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.ChangePassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
    <title>Wiki User Password Change</title>
	<%= BuildPageOne() %>
	<div class="Dialog">
    <form id="Form1" runat="server" action="">
        <asp:ChangePassword ID="ChangePassword1" runat="server" BackColor="#E3EAEB"
            BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
            Font-Size="14pt" CancelDestinationPageUrl="~/default.aspx" DisplayUserName="true">
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        </asp:ChangePassword>
    </form>
    </div>
	<%= BuildPageTwo() %>
