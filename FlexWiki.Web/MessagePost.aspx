<%@ Page Language="c#" CodeBehind="MessagePost.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.MessagePost" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
    <script runat="server">
    protected void TextBox1_TextChanged(object sender, EventArgs e)
    {

    }
    </script>
    <head>
    <title>Create Message</title>
<%= BuildPageOne() %>
    <form id="Form1" runat="server">
        <div id="MessagePost" class="MessagePost" style="width: 849px; height: 382px">
            <asp:Panel ID="Panel2" runat="server" Height="36px" Width="840px">
                <asp:Literal ID="MessagingDisabled" runat="server" Text="The use of Forums, Threaded Messages or Talkback capabilities has been prohibited by an Administrator policy"></asp:Literal></asp:Panel>
            <div id="ForumInfo" class="ForumInfo" style="width: 882px; height: 134px">
                <asp:CheckBox ID="NewForumCheck" runat="server" AutoPostBack="True" Text="Create New Forum"
                    Width="151px" />
                <asp:Panel ID="Panel1" runat="server" Height="101px" Width="840px">
                <div id="ForumName" class="ForumName" style="width: 707px; height: 30px">
                <asp:Label ID="ForumNameLbl" runat="server" Text="Forum Name:" Width="118px"></asp:Label>
                <asp:TextBox ID="ForumNameText" runat="server" Width="337px"></asp:TextBox>
                    <asp:CustomValidator ID="ForumNameValCntl" runat="server" ControlToValidate="ForumNameText"
                        EnableClientScript="False" ErrorMessage="Forum Name provided is already in use"
                        ValidateEmptyText="True" Width="224px"></asp:CustomValidator></div>
                <div id="ForumNamespace" class="ForumNamespace" style="width: 709px; height: 33px">
                    <asp:Label ID="FormumNamespaceLbl" runat="server" Text="Forum Namespace:" Width="121px"></asp:Label>
                    <asp:TextBox ID="ForumNamespaceText" runat="server" Width="336px"></asp:TextBox>
                <asp:CustomValidator ID="ForumNamespaceValCntl" runat="server" ErrorMessage="Namespace provided does not exist." ControlToValidate="ForumNamespaceText" EnableClientScript="False" ValidateEmptyText="True"></asp:CustomValidator></div>
                <div id="ForumKey" class="ForumKey" style="width: 809px; height: 32px">
                    <asp:Label ID="ForumKeyLbl" runat="server" Text="ForumKey:" Width="120px"></asp:Label>
                    <asp:TextBox ID="ForumKeyText" runat="server" Width="334px"></asp:TextBox>
                    <asp:CustomValidator ID="ForumKeyValCntl" runat="server" ControlToValidate="ForumKeyText"
                        EnableClientScript="False" ErrorMessage="ForumKey provided is already in use, or is empty"
                        ValidateEmptyText="True"></asp:CustomValidator></div>
                </asp:Panel>
            </div>
            <div class="MessageTitle" style="width: 918px; height: 33px">
                <asp:Label ID="MessageTitleLbl" runat="server" Text="Message Title:" Width="110px"></asp:Label>
                <asp:TextBox ID="MessageTitleText" runat="server" Width="593px"></asp:TextBox>
                <asp:CustomValidator ID="MsgTitleValCntl" runat="server" ControlToValidate="MessageTitleText"
                    EnableClientScript="False" ErrorMessage="Message Title is Empty" ValidateEmptyText="True"></asp:CustomValidator></div>
            <div class="MessageBody" style="width: 95%; height: 226px">
                <asp:TextBox ID="MessageText" runat="server" Height="191px" Rows="15" TextMode="MultiLine"
                    Width="711px"></asp:TextBox>
                <asp:CustomValidator ID="MsgBodyValCntl" runat="server" ControlToValidate="MessageText"
                    EnableClientScript="False" ErrorMessage="The message has no text" ValidateEmptyText="True"></asp:CustomValidator></div>
            <div style="width: 759px; height: 26px; position: static;" class="UserInfo">
                <asp:Label ID="UserLbl" runat="server" Text="User:" Width="66px"></asp:Label>
                <asp:TextBox ID="UserText" runat="server" OnTextChanged="TextBox1_TextChanged" Width="337px"></asp:TextBox></div>
            <div id="Div1" class="MessageButtons" style="width: 241px; height: 28px; position: static;">
                <asp:Button ID="CancelBtn" runat="server" Text="Cancel" />
                <asp:Button ID="SaveBtn" Text="Save" CommandName="Save" runat="server" /></div>
            <div class="MessageValidation" style="width: 100px; height: 100px">
            <asp:Panel ID="Panel3" runat="server" Height="84px" Width="494px">
                &nbsp;<asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Errors:" />  
            </asp:Panel></div>
            &nbsp;
        </div>
    </form>
    </div>
	<%= BuildPageTwo() %>

