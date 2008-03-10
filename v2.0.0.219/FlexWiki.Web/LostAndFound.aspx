<%@ Page language="c#" Codebehind="LostAndFound.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.LostAndFound" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en" >
	<head>
		<title>Lost And Found</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW" />
		<%= InsertStylesheetReferences() %>
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
