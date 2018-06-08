<%--
// Author   : Joe Pickering, 23/10/2009 -- re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek - Login" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <ajax:AnimationExtender runat="server" TargetControlID="lgin_login">
      <Animations>
        <OnLoad>
            <Sequence>
              <Color AnimationTarget="lgin_login" Duration="0.9" StartValue="#ffa500" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
            </Sequence>
        </OnLoad>
      </Animations>
    </ajax:AnimationExtender>

    <div id="div_page" width="900" runat="server" class="normal_page" style="height:480px; background-image:none;">&nbsp;
        <table border="0" width="100%" style="height:90%">
            <tr>
                <td align="center" valign="middle">
               
                    <asp:Login ID="lgin_login" runat="server" OnLoggedIn="OnLoggedIn" OnLoginError="LoginError" style="font-family:Verdana; font-size:8pt;">
                        <LayoutTemplate>
                            <table width="350" cellspacing="0" cellpadding="0" style="border:solid 1px #000000; height:175px">
                                <tr>
                                    <td align="left" valign="top">
                                        <img src="Images/Misc/titleBarAlpha.png"><br />
                                        <asp:Label Text="Log in" runat="server" ForeColor="White" style="position:relative; top:-17px; left:4px;"/> 
                                    </td>
                                </tr>
                                <tr>
                                    <td align="center" valign="middle">
                                        <table cellpadding="0" cellspacing="0" style="margin-top:20px;">
                                            <tr>
                                                <td align="right"><asp:Label runat="server" AssociatedControlID="UserName" Text="User Name:&nbsp;"/></td>
                                                <td align="left">
                                                    <asp:TextBox ID="UserName" runat="server" Width="120px" style="border-radius:4px;"/>
                                                    <asp:RequiredFieldValidator runat="server" Display="Dynamic" ControlToValidate="UserName" 
                                                    ErrorMessage="User Name is required." ToolTip="User Name is required." Text="*"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right"><asp:Label runat="server" AssociatedControlID="Password" Text="Password:&nbsp;"/></td>
                                                <td align="left"><asp:TextBox ID="Password" TextMode="Password" runat="server" Width="120px" style="border-radius:4px;"/></td>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;</td>
                                                <td align="left">
                                                    <asp:LinkButton runat="server" Text="Request Password" ForeColor="Gray" CausesValidation="false" OnClientClick="return requestPW();"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr><td align="center"><asp:Label ID="FailureText" runat="server"/></td></tr>
                                <tr>
                                    <td align="right" valign="bottom">
                                        <telerik:RadButton runat="server" CommandName="Login" Text="Log In" OnClientClicking="function (button,args){lowertrim();}"
                                            style="margin:6px;" Skin="Bootstrap"/>
                                    </td>
                                </tr>
                            </table>
                        </LayoutTemplate>
                    </asp:Login>
                </td>
            </tr>
        </table>
    </div>
    <div style="display:none;">
        <asp:PasswordRecovery ID="pwr" runat="server" 
        OnSendingMail="OnSendingMail" OnSendMailError="OnSendMailError" OnVerifyingUser="OnVerifyingUser"
        MailDefinition-BodyFileName="~/MailTemplates/RetrievePasswordMsg.txt" 
        MailDefinition-From="no-reply@bizclikmedia.com" MailDefinition-IsBodyHtml="true"
        MailDefinition-Subject="DataGeek Login Details" MailDefinition-Priority="High">
            <UserNameTemplate>
                <asp:TextBox ID="UserName" runat="server"/>
                <asp:Button ID="SubmitButton" runat="server" CommandName="Submit"/>
            </UserNameTemplate>
        </asp:PasswordRecovery>
    </div>
    
    <script type="text/javascript">
        function lowertrim() {
            grab('Body_lgin_login_UserName').value = grab('Body_lgin_login_UserName').value.trim().toLowerCase();
            grab('Body_lgin_login_Password').value = grab('Body_lgin_login_Password').value.trim();
            return false;
        }
        function requestPW() {
            var username = grab('Body_lgin_login_UserName').value.trim();
            var rqp_username = grab('Body_pwr_UserNameContainerID_UserName');
            var rqp_rqbutton = grab('Body_pwr_UserNameContainerID_SubmitButton');

            if (username != "" && username != null) {
                rqp_username.value = username;
                rqp_rqbutton.click();
            }
            else {
                alert("Please enter your username in the User Name box and press Request Password to receive your login credentials via e-mail.");
            }
            return false;
        }
    </script>
</asp:Content>