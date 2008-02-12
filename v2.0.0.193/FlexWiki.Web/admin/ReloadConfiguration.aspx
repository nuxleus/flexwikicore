<%@ Page language="c#" Codebehind="ReloadConfiguration.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Admin.ReloadConfiguration" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>FlexWiki Administration: Reload Configuration File</title>
    <% ShowHead(); %>
</head>
<body>
    <div class="Border" id="TopBorder">
    <% ShowTitle("Reload Configuration"); %>
    </div>
    <div class="Border" id="LeftBorder">
    <% ShowMenu(); %>
    </div>
    <div class="Admin" id="TopicBody">
    <% ShowMain(); %>
    </div>
</body>
</html>
