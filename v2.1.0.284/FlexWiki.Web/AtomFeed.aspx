<%@ Page language="c#" contenttype="text/xml" Codebehind="AtomFeed.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.AtomFeed" %>
<%@ OutputCache Duration="1" VaryByParam="namespace;inherited" %>
<% 
	Response.Clear(); 
	Response.ContentType="text/xml"; 
	BuildFeed();	
%>
