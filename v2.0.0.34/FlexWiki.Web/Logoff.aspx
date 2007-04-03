<%@ Page language="c#" CodeBehind="Logoff.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Logoff" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Wiki Logoff</title>
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<%= InsertStylesheetReferences() %>
	</HEAD>
	<body class='Dialog'>
		<fieldset>
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
							<asp:HyperLink id="ReturnLink" runat="server">Return to FlexWiki.</asp:HyperLink>
						</td>
					</tr>
				</table>
			</form>
		</fieldset>
	</body>
</HTML>
