<%@ Page Language="c#" Codebehind="HttpsRequired.aspx.cs" AutoEventWireup="false"
    Inherits="FlexWiki.Web.HttpsRequired" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Request Requires HTTPS</title>
    <meta runat="server" id="MetaRefresh" http-equiv="refresh" content="" />
    <%= InsertStylesheetReferences() %>
    <%= InsertFavicon() %>
</head>
<body>
    <form id="Form1" runat="server" method="post" action="">
        <table>
            <tbody>
                <tr>
                    <td>
                        <asp:Label ID="Msg" ForeColor="red" Font-Names="Verdana" Font-Size="10" runat="server">
                        This request requires a secure connection (HTTPS). You will be redirected to the secure portion of the website momentarily, or you can use the link below to retry your request immediately.</asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:HyperLink ID="ReturnLink" runat="server">Try again using HTTPS</asp:HyperLink>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
</body>
</html>
