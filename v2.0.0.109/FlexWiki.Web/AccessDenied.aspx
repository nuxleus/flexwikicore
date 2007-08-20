<%@ Page Language="c#" Codebehind="AccessDenied.aspx.cs" AutoEventWireup="false"
    Inherits="FlexWiki.Web.AccessDenied" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>Access Denied</title>
    <%= InsertStylesheetReferences() %>
    <style type="text/css">
			body { background:#FFF3E1; margin:4px; }
    </style>
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
                        <asp:HyperLink ID="ReturnLink" runat="server">Return to the wiki.</asp:HyperLink>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
</body>
</html>
