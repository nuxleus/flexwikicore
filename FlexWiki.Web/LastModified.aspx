<%@ Page language="c#" Codebehind="LastModified.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.LastModified" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title>Last Modified</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW"/>
		<%= InsertStylesheetReferences() %>
		<%= InsertFavicon() %>
		<%InsertScripts(); %>
<script  type="text/javascript" language="javascript">
function filter() {
    var selected = document.getElementById('AuthorFilter');
	var author = selected.options[selected.selectedIndex].text;
	var table = document.getElementById('MainTable');
	for (var i = 0; i < table.rows.length; i++)	{
		var row = table.rows.item(i);
		var authorcell = row.cells.item(2);
		var show = true;
		if (author != '[All]' && authorcell.innerHTML != author) {	show = false;	}
		if (show) {
			try  {
			 row.style.display = 'table-row';
			} catch (e) {
			  row.style.display = 'block';
			}
		}
		else
		{
			row.style.display = 'none';
		}
	}
}


function changeNamespace()
{
    var filter = document.getElementById('NamespaceFilter');
	var ns = filter.options[filter.selectedIndex].text;
	var newURL = 'LastModified.aspx?namespace=' + ns;
	window.location = newURL;	
}
</script>
</head>
	<body>
	<%InsertLeftTopBorders(); %>
	<fieldset class="Dialog">
	<legend class='DialogTitle'>Recent Changes</legend>
	<p>Namespace: <%=NamespaceFilter()%></p>
		<% DoSearch(); %>
	</fieldset>
	<%InsertRightBottomBorders(); %>
	</body>
</html>
