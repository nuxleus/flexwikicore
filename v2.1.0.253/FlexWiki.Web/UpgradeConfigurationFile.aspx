<%@ Page language="c#" Codebehind="UpgradeConfigurationFile.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.UpgradeConfigurationFile" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title>Upgrade Configuration File</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW" />
		<%= InsertStylesheetReferences() %>
		<%= InsertFavicon() %>
		<%InsertScripts(); %>
	</head>
	<body>
	<%InsertLeftTopBorders(); %>
	<div class="Dialog">
		<P>The configuration file for this installation of FlexWiki is using an old format 
			and needs to be upgraded.</P>
		<P>If you are the system administrator, you may <A href="admin/UpgradeConfigFile.aspx">learn 
				how to perform this upgrade automatically</A>.&nbsp; If not, you should 
			contact the system administrator.</P>
		<P>&nbsp;</P>
		</div>
	<%InsertRightBottomBorders(); %>
	</body>
</html>
