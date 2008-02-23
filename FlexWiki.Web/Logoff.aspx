<%@ Page language="c#" CodeBehind="Logoff.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Logoff" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title>Wiki Logoff</title>
		<%= InsertStylesheetReferences() %>
		<%= InsertFavicon() %>
		<%InsertScripts(); %>
	</head>
	<body>
	<%InsertLeftTopBorders(); %>
		<fieldset class="Dialog">
			<legend class='DialogTitle'>
				Logoff</legend>
			<form id="LogoffForm" runat="server" method="post">
				<table width="100%">
					<tr>
						<td align="center">
							<asp:Label id="LogOffMessage" runat="server" />
						</td>
					</tr>
	                <tr>
						<td align="center">
							<asp:HyperLink id="ReturnLink" runat="server">Return to the wiki.</asp:HyperLink>
						</td>
					</tr>
				</table>
			</form>
		</fieldset>
	<%InsertRightBottomBorders(); %>
	</body>
</html>
