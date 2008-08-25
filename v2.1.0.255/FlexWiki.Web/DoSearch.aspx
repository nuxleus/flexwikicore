<%@ Page language="c#" Codebehind="DoSearch.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.DoSearch" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title runat="server" id="title">Search</title>
		<%= InsertStylesheetReferences() %>
		<%= InsertFavicon() %>
		<%InsertScripts(); %>
	</head>
	<body>
	<%InsertLeftTopBorders(); %>
		<form id="Form1" method="post" runat="server">
			<p>Search for:
				<asp:textbox runat="server" id="searchString" text="type regex search" />
				<asp:button runat="server" onclick="Search" text="Search" ID="Button1" />
				<script runat="server">
			void Search(object sender, EventArgs e)
			{
				Response.Redirect("Search.aspx?search=" + searchString.Text);
			}
				</script>
		</form>
		</P>
	<%InsertRightBottomBorders(); %>
	</body>
</html>
