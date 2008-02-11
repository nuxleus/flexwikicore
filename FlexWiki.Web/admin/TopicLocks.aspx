<%@ Page language="c#" Codebehind="TopicLocks.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.TopicLocks" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
		<title>Topic Locks</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW" />
		<%= InsertStylesheetReferences() %>
		<%= InsertFavicon() %>
<script  type="text/javascript" language="javascript">
function ChangeNamespace_Click()
{
    document.getElementById("topic").value = null;
    document.getElementById("fileaction").value = null; 
	document.forms["Form1"].submit();
}
function filter() {
	var author = AuthorFilter.options[AuthorFilter.selectedIndex].text;
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
function FileAction_Click(topic, activity)
{
    document.getElementById("topic").value = topic;
    document.getElementById("fileaction").value = activity; 
	document.forms["Form1"].submit();
}

function changeNamespace()
{
	var ns = NamespaceFilter.options[NamespaceFilter.selectedIndex].text;
	var newURL = 'TopicLocks.aspx?namespace=' + ns;
	window.location = newURL;	
}
</script>
</head>
	<body class='Dialog'>
    <form method="post" action="" id="Form1">	
    <fieldset>
	<legend class='DialogTitle'>Recent Changes and Topic Locks</legend>
	<p>Namespace: <%=NamespaceFilter()%></p>
		<% DoSearch(); %>
	</fieldset>
        <input id="topic" name="topic" type="hidden" />
        <input id="fileaction" name="fileaction" type="hidden" />
	</form>
	</body>
</html>
