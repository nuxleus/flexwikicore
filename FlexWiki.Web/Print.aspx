<%@ Page Language="c#" Codebehind="Print.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Print" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <meta name="Robots" content="NOINDEX, NOFOLLOW"/>
    <title>
        <%= GetTitle() %>
    </title>
    <%= InsertStylesheetReferences() %>
    <%= InsertFavicon() %>
    <style type="text/css">
			body { background:#FFF; margin:4px; }
    </style>
</head>
<body onload="focus();">
    <% DoPage(); %>
</body>
</html>
