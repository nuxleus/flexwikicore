<%@ Page Language="c#" Codebehind="Search.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Search" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title id="title">Search</title>
    <%= InsertStylesheetReferences() %>
</head>
<body class='Dialog'>
    <% DoSearch(); %>
</body>
</html>
