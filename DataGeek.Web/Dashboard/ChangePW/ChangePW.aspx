<%--
// Author   : Joe Pickering, 07/11/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: New Password" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="ChangePW.aspx.cs" Inherits="ChangePW" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <ajax:AnimationExtender runat="server" TargetControlID="cp">
      <Animations>
        <OnLoad>
            <Sequence>
              <Color AnimationTarget="cp" Duration="0.9" StartValue="#ffa500" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
            </Sequence>
        </OnLoad>
      </Animations>
    </ajax:AnimationExtender>

    <div id="div_page" width="900" runat="server" class="normal_page" style="background-image:url('/Images/Backgrounds/Default.png');">
        <center>
        <div class="WarningContainer" style="width:640px; padding-top:40px; margin-top:50px; margin-bottom:50px;">
            <table width="100%">
                <tr>
                    <td align="center">
                        <div ID="div_not_done" runat="server" visible="true">
                            <asp:Label runat="server" Text="Before you continue, please specify a new password for your account." CssClass="WarningPageTitle"/>
                        </div>
                        <div ID="div_done" runat="server" visible="false" class="WarningPageDiv">
                            <asp:Label runat="server" Text="Password successfully changed, click"/>
                            <asp:HyperLink runat="server" NavigateUrl="~/default.aspx" Text="here" ForeColor="LightBlue"/>
                            <asp:Label runat="server" Text="to continue."/>
                        </div>
                        <br/><br/>
                    </td>
                </tr>
                <tr>
                    <td align="center">
                        <div ID="div_cp" runat="server" style="margin:20px;">
                            <asp:ChangePassword ID="cp" runat="server" TitleTextStyle-Font-Bold="true" BorderPadding="20"
                            TitleTextStyle-HorizontalAlign="Left" PasswordLabelText="Current Password:"
                            InstructionTextStyle-HorizontalAlign="Left" OnChangingPassword="ChangingPassword"
                            OnChangedPassword="ChangedPassword" ChangePasswordButtonStyle-CssClass="changepassword" 
                            CancelButtonStyle-CssClass="invisible" ContinueButtonStyle-CssClass="invisible"
                            InstructionText="Your new password must be at least 7 characters<br/> in length and contain at least one non-alphanumeric<br/> character (such as @!#).<br/><br/>" 
                            DisplayUserName="true" style="border:dotted 1px gray;"/>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>  
                        <div ID="div_request" runat="server" class="WarningPageDiv" style="width:550px; margin-top:10px;">      
                            <asp:Label runat="server" Text="If you can't remember your current password, click"/>
                            <asp:LinkButton ID="lb_rqpw" runat="server" Text="here" ForeColor="LightBlue" CausesValidation="false" OnClientClick="return requestPW();"/>
                            <asp:Label runat="server" Text="to request it via e-mail."/>
                        </div>              
                    </td>
                </tr>
            </table>
        </div>
    </div>
    </center>
    <div style="display:none;">
        <asp:HiddenField ID="hf_username" runat="server" />
        <asp:PasswordRecovery ID="pwr" runat="server" OnSendingMail="RequestedPassword"
        MailDefinition-BodyFileName="~/MailTemplates/RetrievePasswordMsg.txt" OnUserLookupError="OnUserLookupError" 
        MailDefinition-From="no-reply@wdmgroup.com" MailDefinition-IsBodyHtml="true"
        MailDefinition-Subject="DataGeek Login Details"
        MailDefinition-Priority="High" OnVerifyingUser="VerifyEmailAddress">
            <UserNameTemplate>
                <asp:TextBox ID="UserName" runat="server"/>
                <asp:Button ID="SubmitButton" runat="server" CommandName="Submit"/>
            </UserNameTemplate>
        </asp:PasswordRecovery>
    </div>
    <script type="text/javascript">
        function requestPW() {
            var username = grab("<%= hf_username.ClientID %>").value;
            var rqp_username = grab('Body_pwr_UserNameContainerID_UserName');
            var rqp_rqbutton = grab('Body_pwr_UserNameContainerID_SubmitButton');
            username = username.trim();
            
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