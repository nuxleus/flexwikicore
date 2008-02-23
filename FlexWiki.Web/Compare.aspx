<%@ Page Language="c#" Codebehind="Compare.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Compare" %>

<%StartPage();%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>
        <%= GetTitle() %>
    </title>
    <%= InsertStylesheetReferences() %>
    <%= InsertFavicon() %>
	<%InsertScripts(); %>
    <style type="text/css">
			body { background:#FFF3E1; margin:4px; }
    </style>

    <script type="text/javascript" src="wikidefault.js"></script>
    <script type="text/javascript" src="wikitopicbar.js"></script>
    <script type="text/javascript" language="javascript">
    function BodyClick()
    {
        SetEditing(false);
    }
    </script>

</head>
<body onload="focus();">
	<%InsertLeftTopBorders(); %>
    <div id="content">
        <a name="top" id="contentTop"></a>
        <h1 class="firstHeading">
            Compare two versions</h1>
        <div id="bodyContent">
            <% DoPage(); %>
        </div>
    </div>
	<%InsertRightBottomBorders(); %>
</body>
</html>
<%
    EndPage();
%>
