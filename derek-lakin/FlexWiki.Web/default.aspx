<%@ Page Language="c#" Codebehind="default.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Default2" %>

<% StartPage(); %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>
        <%= GetTitle() %>
    </title>
    <%= InsertStylesheetReferences() %>
    <%= DoHead() %>
</head>
<body onclick="javascript:BodyClick()" ondblclick="javascript:BodyDblClick()">
    <%= DoPage() %>
</body>
</html>
<% EndPage(); %>
