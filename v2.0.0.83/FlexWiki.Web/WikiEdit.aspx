<%@ Page Language="c#" Codebehind="WikiEdit.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.WikiEdit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>
        <%= TheTopic.ToString() %>
        (edit)</title>
    <meta name="Robots" content="NOINDEX, NOFOLLOW" />
    <%= InsertStylesheetReferences() %>

    <script type="text/javascript" language="javascript" src="wikiedit.js"></script>

</head>
<% DoPage(); %>
</html>
