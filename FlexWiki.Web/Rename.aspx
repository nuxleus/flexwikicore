<%@ Page Language="c#" CodeBehind="Rename.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Rename" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
    <title>Rename Topic</title>
    <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1" />
    <meta name="CODE_LANGUAGE" content="C#" />
    <meta name="Robots" content="NOINDEX, NOFOLLOW" />
    <%= InsertStylesheetReferences() %>
    <%= InsertFavicon() %>
</head>
<body>
    <table width="100%" height="100%" border="0" cellpadding="7" cellspacing="0">
        <tr>
            <%	DoLeftBorder(); %>
            <td valign="top" id="RenameContent">
                <fieldset>
                    <%
                        if (Request.HttpMethod == "POST")
                        {
                            Response.Write("<legend class=\"DialogTitle\">Rename <b>" + FlexWiki.Web.HtmlWriter.Escape(OldName) + "</b></legend>");
                            PerformRename();
                        }
                        else
                        {
                            // See if the topic is read only
                            if (IsTopicReadOnly)
                            {
                                Response.Write("<legend class=\"DialogTitle\"><b>Topic (" + AbsTopicName + ") can non be changed.</b></legend>");
                            }
                            else
                            {
                    %>
                    <legend class="DialogTitle">Rename <b>
                        <%= FlexWiki.Web.HtmlWriter.Escape(AbsTopicName.DottedName)  %>
                    </b></legend>
                    <p>
                        <b>&nbsp;Important guidelines</b> (rename is not always straightforward!):</p>
                        <ul>
                            <li>When you rename a topic, you may be able to ask (depending on how this site is configured)
                                to have references to the topic automatically updated.</li>
                            <ul>
                                <li>References from other namespaces are not updated</li>
                                <li>Plural references are not found</li>
                            </ul>
                            <li>Because not all references can be reliably found (e.g., because a topic has been
                                bookmarked by someone, or is referenced from another namespace), a page will be
                                automatically generated that tells people where the new page is.</li>
                            <li>Often when you rename a topic you are changing its meaning. As a result, you might
                                want to change the text surrounding the references to the topic; please consider
                                reviewing the current references when you are done renaming to be sure they still
                                make sense.</li>
                        </ul>
                    <hr noshade size="1"/>
                    <form id="RenameForm" method="post" action="Rename.aspx">
                    <input style="display: none;" type="text" name="oldName" value="<%= FlexWiki.Web.HtmlWriter.Escape(AbsTopicName.LocalName)  %>" />
                    <b>&nbsp;Old</b> name:
                    <%= FlexWiki.Web.HtmlWriter.Escape(AbsTopicName.LocalName)  %>
                    <br />
                    <b>&nbsp;New</b> name:
                    <input style="font-size:x-small;" type="text" name="newName" value="<%= FlexWiki.Web.HtmlWriter.Escape(AbsTopicName.LocalName)  %>" />
                    <%
                        {
                            bool fixupDisabled = false;
                            try
                            {
                                fixupDisabled = bool.Parse(System.Configuration.ConfigurationSettings.AppSettings["DisableRenameFixup"]);
                            }
                            catch
                            {
                            }
                            if (!fixupDisabled)
                            {					
                    %>
                    <input type="checkbox" id="fixup" name="fixup" /><label for="fixup">Automatically update
                        references</label>
                    <%
                        }
                            else
                            {
                    %>
                    <p>
                        <i>Automatically updating references from other topics has been disabled on this site</i></p>
                    <%
                        }
                        }
                    %>
                    <div style='display: none'>
                        <br />
                        <input type="checkbox" checked="checked" id="leaveRedirect" name="leaveRedirect" /><label for="leaveRedirect">Generate
                            a redirect page under the old name</label>
                    </div>
                    <p>
                    </p>
                    <p>
                        <input style="display: none;" type="text" name="namespace" value="<%= FlexWiki.Web.HtmlWriter.Escape(AbsTopicName.Namespace)  %>" />
                        <input type="submit" id="SaveButton" value="Rename" />
                    </p>
                    </form>
                    <%
                        }
                        }
                    %>
                </fieldset>
            </td>
            <%  DoRightBorder(); %>
        </tr>
    </table>
</body>
</html>
