<%@ Page Language="c#" AutoEventWireup="false" Inherits="FlexWiki.Web.WikiEdit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>
        <%= TheTopic.ToString() %>
        (edit)</title>
<meta name="Robots" content="NOINDEX, NOFOLLOW" />
<%= InsertStylesheetReferences() %>
<%= InsertFavicon() %>
<% ShowCss(); %>
</head>
<% DoPage(); %>

<form id="Form3" method="post" enctype="multipart/form-data" runat="server" >
<input id="_uploadFilePath" type="file" size="30" name="_uploadFilePath" runat="server" />   
<input type="button" id="FileUploadSend" name="FileUploadSend" onclick="javascript:FileUploadSend_OnClick()" value="Upload File" />
<input type="text" style="display:none" name="Topic" value="<%= TheTopic.ToString() %>" />
<input type="text" style="display: none" id="_processAttachment" name="_processAttachment" value="IsNotAttachment" runat="server" />
<textarea name="PostBox" style="display:none" cols="20" rows="2"></textarea>
<% ShowEditPageMiddle(); %>
</form>
<% ShowEditPageFinal(); %>
</html>
