<%@ Page language="c#" Codebehind="RequestNamespace.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.RequestNamespace" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title>Request Namesapce</title>
		<%= MainStylesheetReference() %>
		<%= InsertFavicon() %>
		<%InsertScripts(); %>
	</head>
	<body>
	<%InsertLeftTopBorders(); %>
	    <div class="Dialog">
		<% ShowPage(); %>
		</div>
	<%InsertRightBottomBorders(); %>
	</body>
</html>
