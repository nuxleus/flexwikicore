<%@ Page Language="c#" Codebehind="Compare.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Compare" %>

<%StartPage();%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <script type="text/javascript" src="wikitopicbar.js"></script>
    <script type="text/javascript" language="javascript">
    function BodyClick()
    {
        SetEditing(false);
    }
    </script>
    <title>
        <%= GetTitle() %>
    </title>
    <%= BuildPage() %>
<%
    EndPage();
%>
