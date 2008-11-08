<%@ Page Language="c#" Codebehind="RegisterUser.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.RegisterUser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
	<head>
    <title>Wiki User Registration</title>
	<%= BuildPageOne() %>
	<div class="Dialog">
    <form runat="server" action="">
        <asp:CreateUserWizard ID="CreateUserWizard1" runat="server" ActiveStepIndex="0" BackColor="#E3EAEB"
            BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
            Font-Size="14pt" DisableCreatedUser="false" LoginCreatedUser="true">
            <WizardSteps>
                <asp:CreateUserWizardStep runat="server">
                </asp:CreateUserWizardStep>
                <asp:CompleteWizardStep runat="server">
                    <ContentTemplate>
                        <table border="0" style="font-size: 100%; font-family: Verdana">
                            <tr>
                                <td align="center" colspan="2" style="font-weight: bold; color: white; background-color: #1c5e55">
                                    Complete</td>
                            </tr>
                            <tr>
                                <td>
                                    Your account creation request has been submitted to an administrator, who will review
                                    your account and complete its creation.</td>
                            </tr>
                            <tr>
                                <td align="right" colspan="2">
                                    <asp:HyperLink runat="server" ID="ReturnLink" BackColor="White" BorderColor="#C5BBAF"
                                        BorderStyle="Solid" BorderWidth="1px"  Font-Names="Verdana" ForeColor="#1C5E55" 
                                        Text="Continue" NavigateUrl="~/default.aspx" />
                                </td>
                            </tr>
                        </table>
                    </ContentTemplate>
                </asp:CompleteWizardStep>
            </WizardSteps>
            <SideBarStyle BackColor="#1C5E55" Font-Size="0.9em" VerticalAlign="Top" />
            <TitleTextStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <SideBarButtonStyle ForeColor="White" />
            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid"
                BorderWidth="1px" Font-Names="Verdana" ForeColor="#1C5E55" />
            <HeaderStyle BackColor="#666666" BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="2px"
                Font-Bold="True" Font-Size="0.9em" ForeColor="White" HorizontalAlign="Center" />
            <CreateUserButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid"
                BorderWidth="1px" Font-Names="Verdana" ForeColor="#1C5E55" />
            <ContinueButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid"
                BorderWidth="1px" Font-Names="Verdana" ForeColor="#1C5E55" />
            <StepStyle BorderWidth="0px" />
        </asp:CreateUserWizard>
    </form>
    </div>
	<%= BuildPageTwo() %>
