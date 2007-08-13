<%@ Page Language="c#" Codefile="default.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Default2" %>
<%@ Register TagPrefix="flexwiki" Namespace="FlexWiki.Web" Assembly="FlexWiki.Web" %>
<% StartPage(); %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>
        <%= GetTitle() %>
    </title>
    <%= InsertStylesheetReferences() %>
    <%= DoHead() %>
</head>
<body onclick="javascript:BodyClick()" ondblclick="javascript:BodyDblClick()">
    <flexwiki:TopicControl runat="server" id="TopBorder" Topic="_NormalBorders.TopBorder" IsProperty="true" CssClass="Border" />
    <flexwiki:TopicControl runat="server" id="LeftBorder" Topic="_NormalBorders.LeftBorder" IsProperty="true" CssClass="Border" />
    <flexwiki:TopicControl runat="server" id="TopicBody" Topic="HomePage" IsMain="true" />
    <flexwiki:TopicControl runat="server" id="RightBorder" Topic="_NormalBorders.RightBorder" IsProperty="true" CssClass="Border" />
    <flexwiki:TopicControl runat="server" id="BottomBorder" Topic="_NormalBorders.BottomBorder" IsProperty="true" CssClass="Border" />
</body>
</html>
<% EndPage(); %>
