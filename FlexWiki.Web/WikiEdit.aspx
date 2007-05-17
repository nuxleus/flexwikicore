<%@ Page Language="c#" Codebehind="WikiEdit.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.WikiEdit" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>
        <%= TheTopic.ToString() %>
        (edit)</title>
<meta name="Robots" content="NOINDEX, NOFOLLOW">
<%= InsertStylesheetReferences() %>
<script type="text/javascript" language="javascript" src="WikiEdit.js"></script>
<style type="text/css"> @media all { tool\:tip { behavior: url(tooltip_js.htc) }}
	.EditZone { background: lemonchiffon; overflow: hidden; height: 100%; height: expression(CalcEditZoneHeight()); /* IE only, other browsers ignore expression */ width: 100%; }
	.tip { font-weight: bold; }
	.tipBody { font: 8pt Verdana; }
	.TipArea { margin: 3px; display: none; border: 1px solid #ffcc00; font: 8pt Verdana; padding: 4px; }
	.EditBox { font: 9pt Courier New; background: whitesmoke; height: 100%; height: expression(CalcEditBoxHeight()); width: 100%; }
	</style>
</head>
<% DoPage(); %>
</html>
