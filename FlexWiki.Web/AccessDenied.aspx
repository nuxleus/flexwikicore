<%@ Page Language="c#" Codebehind="AccessDenied.aspx.cs" AutoEventWireup="false"
    Inherits="FlexWiki.Web.AccessDenied" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Access Denied</title>
    <%= InsertStylesheetReferences() %>
</head>
<body>
    <form id="Form1" runat="server" method="post" action="">
        <table>
            <tbody>
                <tr>
                    <td>
                        <asp:Label ID="Msg" ForeColor="red" Font-Names="Verdana" Font-Size="10" runat="server">Access Denied Message</asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:HyperLink ID="LoginLink" runat="server">Try Logging In</asp:HyperLink>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:HyperLink ID="ReturnLink" runat="server">Return to FlexWiki</asp:HyperLink>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
</body>
</html>
