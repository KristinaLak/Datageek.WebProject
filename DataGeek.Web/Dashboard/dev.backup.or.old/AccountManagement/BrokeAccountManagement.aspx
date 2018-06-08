<%--
// Author   : Joe Pickering, 02/11/2009 - partially re-written 15/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Account Management" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="BrokeAccountManagement.aspx.cs" Inherits="AccountManagement" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager Skin="Black" VisibleStatusbar="false" runat="server">
        <Windows>
            <telerik:RadWindow runat="server" ID="RolesBreakdownWindow" Title="Account Type Information" 
            Width="1100" Height="780" Top="150" Left="300" Behaviors="Move, Close" NavigateUrl="RolesBreakdownWindow.aspx"/>
            <telerik:RadWindow runat="server" ID="SalesBookRoleWindow" Title="Sales Book User Type" 
            Width="400" Height="300" Behaviors="Move, Close" NavigateUrl="SalesBookRoleWindow.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
    
    <div ID="div_page" runat="server" class="normal_page">
        <hr />
        
        <table width="99%" style="position:relative; top:-2px; left:8px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Account" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Management" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        
        <asp:Panel ID="pnl_usermanagement" runat="server">
            <%--User Preferences--%>
            <table cellpadding="0" cellspacing="0">
                <tr>
                    <td valign="top">  
                        <table border="1" cellpadding="1" cellspacing="0" width="300px" bgcolor="White" style="margin-left:11px; font-family:Verdana; font-size:8pt">
                            <tr>
                                <td colspan="2">
                                    <img src="/Images/Misc/titleBarAlphaShort.png" alt="Set User Preferences" style="position:relative; top:-1px; left:-1px;"/>
                                    <img src="/Images/Icons/admin_UserPreferences.png" alt="Users" height="20px" width="20px"/>
                                    <asp:Label runat="server" Text="Users" ForeColor="White" style="position:relative; top:-7px; left:-149px;"/>
                                    <asp:HyperLink runat="server" ForeColor="Blue" Text="View User List" NavigateUrl="~/Dashboard/UserList/UserList.aspx" style="position:relative; left:22px;"/>
                                </td>
                            </tr>
                            <tr>
                                <td style="border-right:0;">
                                    <asp:Label runat="server" Font-Bold="true" Text="Add | Disable | Edit"/>
                                    <asp:Label runat="server" Font-Bold="true" ID="editRolesTextLabel" Text=" | Roles"/>
                                </td>
                                <td align="right" style="border-left:0;">
                                    <asp:Label runat="server" Text="Inc. Terminated"/>
                                    <asp:CheckBox ID="cb_showDisabled" runat="server" AutoPostBack="true" Checked="false" OnCheckedChanged="changeOffice"/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:ImageButton ID="addUserButton" OnClick="addNewUserClick" ToolTip="Add New User" height="30px" width="30px" runat="server" Text="Add" ImageUrl="~/Images/Icons/admin_AddUser.png"/>
                                    <asp:ImageButton ID="deleteUserButton" OnClick="deleteExistingUserClick" ToolTip="Delete/Disable Existing User" height="30px" width="30px" runat="server" ImageUrl="~/Images/Icons/admin_DeleteUser.png" style="position:relative; left:11px;"/>
                                    <asp:ImageButton ID="editUserButton" OnClick="editExistingUserClick" ToolTip="Edit Existing User" height="30px" width="30px" runat="server" Text="Edit" ImageUrl="~/Images/Icons/admin_EditUser.png" style="position:relative; left:22px;"/>
                                    <asp:ImageButton ID="imbtn_privs" runat="server" ToolTip="Customise Privelidges" Height="30" Width="30" ImageUrl="~\Images\Icons\admin_CustomisePrivs.png" OnClick="editExistingUserClick" style="position:relative; left:33px;"/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Panel runat="server" ID="selectUserPanel" Visible="false">
                                        <img src="/Images/Misc/titleBarAlphaShort.png" alt="Select User" style="position:relative; top:-1px; left:-1px;"/>
                                        <img src="/Images/Icons/admin_User.png" alt="Users" height="20px" width="20px" style="position:relative"/>
                                        <asp:Label Text="Select User" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-148px;"/>
                                        <asp:ImageButton ID="closeLoadselectUserPanelButton" runat="server" Height="17" Width="17" ImageUrl="~\Images\Icons\dashboard_Close.png" OnClick="closeLoadUserPanels" style="position:relative; left:53px;"/>
                                    </asp:Panel>
                                    <asp:Panel runat="server" ID="deleteUserPanel" Visible="false">
                                        <table width="350" style="position:relative; top:-2px; left:-2px;">
                                            <tr>
                                                <td>
                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Pick User" style="position:relative; top:-2px; left:-2px;"/>
                                                    <img src="/Images/Icons/admin_User.png" alt="Users" height="20px" width="20px" style="position:relative"/>
                                                    <asp:Label Text="Delete/Disable User" runat="server" ForeColor="White" style="position:relative; top:-8px; left:-194px;"/>
                                                    <asp:ImageButton ID="closeLoaddeleteUserPanelButton" runat="server" Height="17" Width="17" ImageUrl="~\Images\Icons\dashboard_Close.png" OnClick="closeLoadUserPanels" style="position:relative; left:15px;"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    
                                    <%--Edit: Area Search Dropdown--%> 
                                    <asp:DropDownList id="areaBox" visible="false" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="changeOffice"/>
                                    <%--Edit: UserName Dropdownlist--%> 
                                    <asp:DropDownList id="userNameBox" visible="false" Enabled="false" runat="server" Width="110px" AutoPostBack="false" OnSelectedIndexChanged="loadPreferences"> 
                                        <asp:ListItem></asp:ListItem>
                                    </asp:DropDownList>
                                    <%--Edit: Load Roles Customisation Button--%>
                                    <asp:Button ID="loadRolePreferencesButton" OnClick="goToRoles" visible="false" Text="Edit Roles" Enabled="false" runat="server" Width="75px"/>
                                    <asp:Panel runat="server" ID="deleteLockOutPanel" Visible="false">
                                        <%--Delete: Delete Button--%>
                                        <asp:Button ID="deleteSelectedUserButton" OnClick="deleteUser" Text="Delete" runat="server" Enabled="false" Width="60px" 
                                        OnClientClick="if(confirm('This will permanently delete this user and will remove all of their historic data from reports such as the Progress Report and the 8-Week Report. \n\nIt is recommended that you disable the account instead which will remove this user from future reports and mark them as terminated. Are you sure you wish to permanently delete this user?')){return true;}else{return false;}"/>
                                        <%--Lock Out: Lock Out Button--%>
                                        <asp:Button ID="lockOutSelectedUserButton" OnClick="lockOutUser" Text="Disable Account" Enabled="false" runat="server" Width="110px"/>
                                    </asp:Panel>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td valign="top" align="left">
                        <asp:Button alt="Customise Privelidges" ID="imbtn_editprivs" runat="server" Text="Customise" style="position:relative; left:2px;" OnClick="goToRoles"/>
                                                                            <asp:TextBox ID="EditEmail" width="200" runat="server"/>
                                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                                    ControlToValidate="EditEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                        <%--NEW USER--%>
                        <asp:Panel ID="addNewUserPanel" runat="server" Visible="false" style="margin-left:62px; font-family:Verdana; font-size:8pt">
                            <table border="1" cellpadding="1" cellspacing="0" width="608px" bgcolor="White" style="margin-left:2px; font-family:Verdana; font-size:8pt">
                                <tr>
                                    <td align="left" style="border-right:0px;">
                                        <img src="/Images/Misc/titleBarAlpha.png" alt="New User" style="position:relative; top:-1px; left:-1px;"/>
                                        <asp:Label runat="server" Text="New User" ForeColor="White" style="position:relative; top:-7px; left:-170px;"/>
                                    </td>
                                    <td align="right" style="border-left:0px;">
                                        <asp:ImageButton id="closeNewUserPanelButton" alt="Close" runat="server" Height="17" Width="17" ImageUrl="~\Images\Icons\dashboard_Close.png" onclick="closeNewUserPanel" style="position:relative; left:-2px;"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" colspan="2">
                                        <table border="0" cellpadding="1" cellspacing="5">
                                            <tr>
                                                <td align="left" width="200">Full Name:</td>
                                                <td><asp:TextBox ID="newUserFullNameBox" width="145" runat="server"/></td>
                                                <td align="left" valign="top" width="100" rowspan="3" style="border-left:1px solid gray;">&nbsp;CCA Group:</td>
                                                <td rowspan="3" width="140">
                                                    <asp:RadioButtonList ID="newUserCCAGroupRadioList" runat="server">
                                                        <asp:ListItem Selected="True">N/A</asp:ListItem>
                                                        <asp:ListItem>Comm</asp:ListItem>
                                                        <asp:ListItem>List Gen</asp:ListItem>
                                                        <asp:ListItem>Sales</asp:ListItem>
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left">Friendly Name (short):</td>
                                                <td><asp:TextBox ID="newUserFriendlyNameBox" runat="server" Width="145"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Regional Mag:</td>
                                                <td><asp:DropDownList ID="dd_new_region" width="150" runat="server"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Sector:</td>
                                                <td><asp:DropDownList ID="dd_new_sector" Width="150" runat="server"/></td>
                                                <td align="left" valign="top" rowspan="8" style="border-left:1px solid gray;">
                                                    <asp:Label ID="newUserCCATeamLabel" runat="server" Text="&nbsp;CCA Team:"/>
                                                </td>
                                                <td rowspan="8" valign="top"><asp:RadioButtonList ID="newUserCCATeamRadioList" runat="server"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Sub-Sector:</td>
                                                <td><asp:TextBox ID="tb_new_sub_sector" runat="server" Width="145"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Office:</td>
                                                <td><asp:DropDownList runat="server" ID="newUserOfficeDropDown" AutoPostBack="true" OnTextChanged="getNewUserTeamsForSelectedOffice" width="150px"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Phone Number:</td>
                                                <td><asp:TextBox ID="newUserPhoneNumberBox" runat="server" Width="145"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Account Type:</td>
                                                <td>
                                                    <table><tr>
                                                        <td><asp:DropDownList runat="server" Width="110" ID="dd_NewUserPermission" style="position:relative; left:-3px;">
                                                            <asp:ListItem Value="db_Admin">Admin</asp:ListItem>
                                                            <asp:ListItem Value="db_HoS">HoS</asp:ListItem>
                                                            <asp:ListItem Value="db_TeamLeader">Team Leader</asp:ListItem>
                                                            <asp:ListItem Value="db_Finance">Finance</asp:ListItem>
                                                            <asp:ListItem Value="db_GroupUser">Group User</asp:ListItem>
                                                            <asp:ListItem Value="db_User">User</asp:ListItem>
                                                            <asp:ListItem Value="db_CCA">CCA</asp:ListItem>
                                                        </asp:DropDownList></td>
                                                    <td><asp:ImageButton runat="server" Height="20" Width="20" ImageUrl="~\Images\Icons\dashboard_Info.png" style="position:relative; left:2px;" OnClientClick="openHelp(); return false;"/></td>
                                                    </tr></table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left">Current Employee:</td>
                                                <td><asp:CheckBox ID="newUserCurrentlyEmployedCheckbox" runat="server" Checked="true"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left">Starter:</td>
                                                <td><asp:CheckBox ID="newUserStarterCheckbox" runat="server"/></td>
                                            </tr>
                                            <tr>
                                                <td align="left"><asp:Label runat="server" Text="T&D Commission:"/></td>
                                                <td>
                                                    <asp:CheckBox ID="cb_t_and_d_commission" runat="server" Enabled="false" AutoPostBack="true" OnCheckedChanged="ToggleTandDCommission"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <table ID="tbl_t_and_d_commission" runat="server" visible="false">
                                                        <tr><td colspan="2">A trainer will receive X commission on all of this new user's deals for the first 90 days of their employ.</td></tr>
                                                        <tr>
                                                            <td>Percentage:</td>
                                                            <td>Trainer:</td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <asp:TextBox ID="tb_t_and_d_percentage" runat="server" Text="2.5" Width="80"/>
                                                                <asp:CompareValidator runat="server" ControlToValidate="tb_t_and_d_percentage" Operator="GreaterThan" ForeColor="Red" Type="Double" Display="Dynamic" ValueToCompare="0" Text="<br/>Must be a valid number"/> 
                                                                <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_t_and_d_percentage" Display="Dynamic" Text="Required!" Enabled="false"/>
                                                            </td>
                                                            <td width="75%"><asp:DropDownList ID="dd_t_and_d_recipient" runat="server"/></td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                               <td colspan="2">Sales Book Role:</td>
                                               <td colspan="2">
                                                    <table cellpadding="0" cellspacing="0" style="position:relative; left:-184px;">
                                                        <tr>
                                                            <td>
                                                                <asp:RadioButtonList runat="server" CellPadding="0" CellSpacing="0"  Width="260" ID="rbl_officeadmindesign" RepeatDirection="Horizontal">
                                                                    <asp:ListItem Selected="True">Standard&nbsp;&nbsp;</asp:ListItem>
                                                                    <asp:ListItem Value="db_SalesBookOfficeAdmin">Office Admin</asp:ListItem>
                                                                    <asp:ListItem Value="db_SalesBookDesign">Design</asp:ListItem>
                                                                </asp:RadioButtonList>
                                                            </td>
                                                            <td>
                                                                <asp:ImageButton runat="server" Height="20" Width="20" ImageUrl="~\Images\Icons\dashboard_Info.png" OnClientClick="openSBRole(); return false;" style="position:relative; top:2px;"/>
                                                            </td>
                                                        </tr>
                                                    </table>
                                               </td>
                                            </tr>
                                            <tr>
                                                <td valign="top" colspan="2">
                                                    <asp:Label runat="server" Text="User Colour:"/>
                                                    <asp:TextBox ID="newUserColourBox" runat="server" Text="#FF0000"/>
                                                    <telerik:RadColorPicker
                                                        AutoPostBack="false"
                                                        OnClientColorChange="colorChange"
                                                        ID="userColourPicker"
                                                        runat="server"
                                                        Preset="Default"
                                                        Width="200px"
                                                        PreviewColor="True"
                                                        ShowEmptyColor="False"
                                                        Skin="Vista" SelectedColor="White" style="position:relative; top:8px;">
                                                    </telerik:RadColorPicker> 
                                                    <br />
                                                </td>
                                                <td colspan="2" align="left">
                                                    <asp:CreateUserWizard ID="CreateUserWizard" AutoGeneratePassword="true" ContinueButtonStyle-CssClass="invisible" runat="server" LoginCreatedUser="false" OnCreatingUser="OnCreatingUser" OnCreatedUser="addPreferencesToNewUser" style="position:relative; top:4px;">
                                                        <WizardSteps>
                                                            <asp:CreateUserWizardStep ID="CreateUserWizardStep" Title="Create this new user.<br/>(Ensure at least <b>Fullname</b>, <b>Friendlyname</b> and <b>Office</b> are provided)" runat="server">
                                                            </asp:CreateUserWizardStep>
                                                        </WizardSteps>
                                                    </asp:CreateUserWizard>
                                                    <br />
                                                    <asp:CheckBox ID="cb_new_user_force_new_pw" runat="server" Checked="true" Text="Force change password on first login" 
                                                    ToolTip="Force this user to change their password when first logging in." style="position:relative; top:-22px;"/>
                                                    <asp:Label runat="server" Text="<br/>Example username: jsmith"/><br />
                                                    <asp:Label runat="server" Text="Example e-mail: john.smith@bizclikmedia.com"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                    <asp:Label ID="teamLeaderLabel" runat="server" Text="Team Leader:"/>
                    <asp:Panel visible="false" runat="server" ID="teamLeaderOfPanel">
                        <asp:Label runat="server" Text="of&nbsp;"/>
                        <asp:DropDownList runat="server" ID="teamLeaderOfDropDown" Width="90"/>
                    </asp:Panel>
                    <asp:CheckBox ID="teamLeaderCheckBox" AutoPostBack="true" OnCheckedChanged="getTeamLeaderTeams" runat="server" style="position:relative; top:-3px;"/>

                    <asp:Button ID="btn_force_new_pw" runat="server" ToolTip="Force this user to change their password on their next request." OnClick="ForcePasswordReset" Text="Force Password Change"/>
                    <asp:Button ID="btn_unlock_user" runat="server" ToolTip="Unlock this user's account if locked." OnClick="UnlockAccount" Text="Unlock Account"/>
                    <asp:Button ID="saveChanges" runat="server" ToolTip="Save Changes" OnClick="saveUserChanges" Text="Save Changes"/>
<asp:CheckBox ID="cb_edit_t_and_d_update" runat="server" Text="Apply T&D changes to any existing deals?" Checked="false" />
                                            
                    </td>
                </tr>
            </table>
            <asp:Button runat="server" ID="btn_chatenabled" Text="Enable/Disable Chat" OnClick="EnableDisableChat" Visible="false" style="margin-left:11px;"/>
            <asp:Button runat="server" ID="btn_reset_all_passwords" Text="Reset All Passwords" OnClick="ResetAllPasswords" Visible="false" style="margin-left:11px;"
            OnClientClick="return confirm('Are you sure?')"/>
             
            <br />
            <hr style="margin-right:8px; margin-left:8px;"/>     
            
            <%--Edit/Add CCA Team--%>
            <table cellpadding="0" cellspacing="0">
                <tr>
                    <td valign="top">
                        <%--Edit/Add CCA Team Head--%>
                        <table border="1" cellpadding="1" cellspacing="0" width="300px" bgcolor="White" style="margin-left:11px; font-family:Verdana; font-size:8pt">
                            <tr>
                                <td colspan="2">
                                    <img src="/Images/Misc/titleBarAlphaVeryShort.png" alt="Add/Edit CCA Teams" style="position:relative; top:-1px; left:-1px;"/>
                                    <img src="/Images/Icons/admin_Admins.png" alt="CCA Teams" height="20px" width="20px" style="position:relative"/>
                                    <asp:Label Text="CCA Teams" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-125px;"/>
                                </td>
                            </tr>
                            <tr><td><asp:Label runat="server" Font-Bold="true" Text="Add | Edit"/></td></tr>
                            <tr>
                                <td>
                                    <asp:ImageButton ID="AddCCATeamButton" OnClick="AddCCATeamClick" ToolTip="Add New CCA Team" height="30px" width="30px" runat="server" Text="Add" 
                                    ImageUrl="~/Images/Icons/admin_AddNewTeam.png"/>
                                    <asp:ImageButton ID="EditCCATeamButton" OnClick="EditCCATeamClick" ToolTip="Edit/Delete Existing CCA Team" height="30px" width="30px" runat="server" Text="Edit" 
                                    ImageUrl="~/Images/Icons/admin_EditUser.png" style="position:relative; left:3px;"/>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td valign="top" align="left">
                        <asp:Panel runat="server" ID="AddEditCCAPanel" visible="false" style="font-family:Verdana; font-size:8pt">
                            <table border="1" cellpadding="1" cellspacing="0" width="300px" bgcolor="White" style="margin-left:2px; font-family:Verdana; font-size:8pt">
                                <tr>
                                    <td align="left">
                                        <img src="/Images/Misc/titleBarAlpha.png" alt="Edit CCA Team" style="position:relative; top:-1px; left:-1px;"/>
                                        <asp:Label ID="lbl_teamselectadd" Text="Edit CCA Team:" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-171px;"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left">
                                        <table border="0" width="330px" cellpadding="1" cellspacing="5">
                                            <tr>                                       
                                                <td valign="bottom">
                                                    Team Territory
                                                    <asp:ImageButton id="CloseEditCCATeamLabelButton" alt="Close" runat="server" Height="17" Width="17" ImageUrl="~\Images\Icons\dashboard_Close.png" onclick="closeTeamPanel" style="position:relative; left:214px; top:-30px"/>
                                                </td>
                                                <td valign="bottom">
                                                    Team Name
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <asp:DropDownList runat="server" ID="AddEditCCAAreaDropdown" OnTextChanged="getTeamsForSelectedOffice" width="120px"/>
                                                </td>
                                                <td>
                                                    <asp:DropDownList runat="server" visible="false" ID="TeamListDropdown" width="180" OnSelectedIndexChanged="showTeamOptionsAndMembers" AutoPostBack="true"/>
                                                    <asp:TextBox ID="AddEditCCATeamName" width ="150" runat="server"/>
                                                </td>
                                                <td>
                                                    <asp:ImageButton ID="InsertCCATeamButton" ToolTip="Create Team" OnClick="addTeam" height="26px" width="26px" runat="server" Text="Add" ImageUrl="~/Images/Icons/admin_AddNewTeam.png" /> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="3" style="border-top:1">
                                                    <asp:Panel runat="server" ID="EditCCATeamPanel" Visible="false" style="position:relative; left:-5px;">
                                                        <img src="/Images/Misc/titleBarLong.png" alt="Select User" style="position:relative; top:-1px; left:-1px;"/>
                                                        <asp:Label runat="server" Text="Edit/Delete Team:" ForeColor="White" style="position:relative; top:-6px; left:-210px;"/>
                                                        <table>
                                                            <tr>
                                                                <td>Office</td>
                                                                <td>Team Name</td>
                                                            </tr>
                                                            <tr>
                                                                <td><asp:DropDownList ID="newAreaBox" runat="server" Visible="false" Width="120px"/></td>
                                                                <td><asp:TextBox ID="newNameBox" runat="server" Text="New Name" Visible="false" Width="130"/></td>
                                                            </tr>
                                                        </table>
                                                        <asp:Label ID="lbl_thisteamleader" runat="server" Text="Team Leader:" style="position:relative; left:6px;"/>
                                                        <br />
                                                        <br />
                                                        <%--Save Button--%>
                                                        <asp:Button ID="saveTeamChangesButton" runat="server" OnClick="saveTeamChanges" Visible="false" Text="Save" Enabled="True" Width="60px" style="position:relative; left:6px;"/>
                                                        <%--Delete Button--%>
                                                        <asp:Button ID="deleteTeamButton" OnClientClick="return confirm('Are you sure?\n\nThis will set the team of all associated users to N/A and will cause all associated Prospect Report data to be unassigned.\n\nPlease contact an admin if you want these prospects recovered to a new team after you delete this team or simply move each CCA to a new team manually (this will only move currently worked prospects).')" 
                                                        OnClick="deleteSelectedTeam" visible="true" Text="Delete" Enabled="True" runat="server" Width="60px" style="position:relative; left:6px;"/>
                                                    </asp:Panel>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                    <td align="left" valign="top">
                        <asp:Panel ID="pnl_teammembers" runat="server" Visible="false" style="position:relative; left:3px;">
                            <asp:Repeater ID="rptr_teammembers" runat="server">
                                <HeaderTemplate>
                                    <table border="1" cellpadding="1" cellspacing="0" width="198px" bgcolor="White" style="font-family:Verdana; font-size:8pt">
                                        <tr><td colspan="2"><b>Team Members</b></td></tr>
                                </HeaderTemplate>
                                <ItemTemplate>
                                        <tr><td bgcolor="white"><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"FullName").ToString()) %></td></tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>  
                        </asp:Panel>
                    </td>
                </tr>
            </table>
            <br />
            <hr style="margin-right:8px; margin-left:8px;"/> 
        </asp:Panel> 
        
        <asp:Panel ID="otherPanel" runat="server">
            <table>
                <tr>
                    <td valign="bottom">
                        <table style="margin-left:4px; font-family:Verdana; font-size:8pt">
                            <tr>
                                <td valign="top">  
                                    <table border="1" cellpadding="1" cellspacing="0" bgcolor="White" height="235" width="300px">
                                        <tr>
                                            <td>
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="User Logins" style="position:relative; top:-1px; left:-1px;"/>
                                                <img src="/Images/Icons/admin_UserLogins.png" alt="User Logins" height="20px" width="20px" style="position:relative"/>
                                                <asp:Label runat="server" ID="lbl_logins" Text="Logins" ForeColor="White" style="position:relative; top:-7px; left:-193px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <div style="height:202px; overflow:auto;">
                                                    <asp:GridView ID="userLoginsGridView" runat="server" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                                        border="2" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" AutoGenerateColumns="False">
                                                        <Columns>
                                                            <asp:BoundField ItemStyle-HorizontalAlign="Left" HeaderText="User ID" DataField="name" ItemStyle-Width="200px"/>
                                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderText="Date-Time" DataField="datetime" ItemStyle-Width="200px"/>
                                                        </Columns>
                                                    </asp:GridView> 
                                                </div> 
                                            </td>
                                        </tr>
                                    </table>    
                                </td>
                                <td valign="bottom">    
                                    <%--Change Password--%>
                                    <table ID="tbl_change_pw" runat="server" border="1" cellpadding="1" cellspacing="0" width="320px" height="235" bgcolor="White" style="margin-left:4px; font-family:Verdana; font-size:8pt">
                                        <tr>
                                            <td colspan="2" valign="top" style="height:23px">
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="" style="position:relative; top:-1px; left:-1px;"/>
                                                <img src="/Images/Icons/admin_Password.png" alt="" height="20px" width="20px" style="position:relative"/>
                                                <asp:Label runat="server" Text="Change Password" ForeColor="White" style="position:relative; top:-7px; left:-193px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td valign="top">
                                                <asp:ChangePassword ID="cp" runat="server" TitleTextStyle-Font-Bold="true"
                                                TitleTextStyle-HorizontalAlign="Left" PasswordLabelText="Current Password:"
                                                InstructionTextStyle-HorizontalAlign="Left" OnChangingPassword="changingPassword"
                                                OnChangedPassword="changedPassword" ChangePasswordButtonStyle-CssClass="changepassword" 
                                                CancelButtonStyle-CssClass="invisible" ContinueButtonStyle-CssClass="invisible" 
                                                InstructionText="Your new password must be at least 7 characters<br/> in length and contain at least one non-alphanumeric<br/> character (such as @!#)." 
                                                DisplayUserName="true"/>
                                            </td>
                                        </tr>
                                    </table>                         
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td valign="bottom">
                        <table ID="tbl_reset_pw" runat="server" visible="false" border="1" cellpadding="1" cellspacing="0" bgcolor="White" width="340" style="position:relative; top:-7px;">
                            <tr>
                                <td>
                                    <img src="/Images/Misc/titleBarAlpha.png" alt="" style="position:relative; top:-1px; left:-1px;"/>
                                    <img src="/Images/Icons/admin_RequestPassword.png" alt="" height="20px" width="20px" style="position:relative"/>
                                    <asp:Label Text="Reset Password" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-192px;"/>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lbl_reset_pw_username" runat="server" Text="Username:"/>
                                    <asp:TextBox ID="tb_reset_pw_username" runat="server" width="130"/>
                                    <asp:Button ID="btn_reset_pw" runat="server" Width="115" Text="Reset Password" OnClick="ResetPassword" OnClientClick="return confirm('Are you sure you wish to reset this user\'s password to a new random password?\n\nRemember to request the new password after resetting it.');"/>
                                </td>
                            </tr>
                        </table>
                    
                        <asp:Label ID="lbl_userip" runat="server" ForeColor="DarkOrange" Font-Names="Verdana" Font-Size="Small" style="position:relative; left:3px; top:-3px;"/>
                        <asp:UpdatePanel ID="udp_request_password" runat="server">
                            <ContentTemplate>
                                <table ID="retrievePasswordTable" runat="server" border="1" cellpadding="1" cellspacing="0" bgcolor="White" style="position:relative; width:340px; top:-3px; font-family:Verdana; font-size:8pt">
                                    <tr>
                                        <td colspan="2">
                                            <img src="/Images/Misc/titleBarAlpha.png" alt="" style="position:relative; top:-1px; left:-1px;"/>
                                            <img src="/Images/Icons/admin_RequestPassword.png" alt="" height="20px" width="20px" style="position:relative"/>
                                            <asp:Label Text="Request Password" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-192px;"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" style="border-bottom:0px;">
                                            <center><asp:Image ID="img_pwr_loading" runat="server" ImageUrl="~/Images/Icons/loading.gif" Height="50" Width="50" style="display:none;"/></center>
                                            <asp:PasswordRecovery ID="PasswordRecovery1" OnSendingMail="requestedPassword" SuccessText="Password has been successfully sent to your e-mail address" runat="server" MailDefinition-IsBodyHtml="true" MailDefinition-BodyFileName="~/MailTemplates/RetrievePasswordMsg.txt"
                                            SubmitButtonText="Get Password" SubmitButtonType="Link" MailDefinition-From="no-reply@wdmgroup.com" MailDefinition-Subject="DataGeek Login Details" MailDefinition-Priority="High" OnVerifyingUser="VerifyEmailAddress">
                                                <UserNameTemplate>
                                                    <asp:Panel runat="server" DefaultButton="SubmitLinkButton">
                                                        <table border="0" cellpadding="1" cellspacing="0" style="border-collapse:collapse; font-family:Verdana; font-size:8pt;">
                                                            <tr>
                                                                <td>
                                                                    <table border="0" cellpadding="0">
                                                                        <tr>
                                                                            <td align="left" colspan="2"><asp:Label ID="lbl_pwr_info" runat="server" Text="Enter your username and hit the <b>Retrieve Password</b> button to have your login credentials e-mailed to you.<br /><br />" style="display:block;"/></td>  
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="right" valign="top">
                                                                                <asp:Label ID="UserNameLabel" runat="server" Text="Username:" AssociatedControlID="UserName" style="position:relative; top:6px;"/>
                                                                            </td>
                                                                            <td>
                                                                                <asp:TextBox ID="UserName" runat="server"/>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="2" align="right" width="330">
                                                                                <asp:Button ID="SubmitLinkButton" runat="server" CommandName="Submit" OnClientClick="LowerTrimPWRQUsername();" 
                                                                                ValidationGroup="PasswordRecovery1" Text="Retrieve Password" CssClass="retrievepassword"/>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="2">
                                                                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" Display="Dynamic" 
                                                                                ControlToValidate="UserName" ErrorMessage="User Name is required." 
                                                                                ToolTip="User Name is required." ValidationGroup="PasswordRecovery1" Text="*"/>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="center" colspan="2" style="color:Red;">
                                                                                <asp:Literal ID="FailureText" runat="server" EnableViewState="False"/>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                </UserNameTemplate>
                                            </asp:PasswordRecovery>
                                            <asp:Button ID="btn_request_any_pw" runat="server" Visible="false" OnClientClick="LowerTrimPWRQUsername();" OnClick="DebugAnyPassword" Text="DebugPW"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" align="right" style="border-top:0px;">
                                            <asp:Button ID="btn_refresh" runat="server" Text="Request Another Password" Width="200" style="display:none;"
                                            OnClick="newPasswordRequest" ToolTip="Refresh this module so you can request another password."/> 
                                        </td>
                                    </tr>
                                    <tr><td colspan="2"><asp:label ID="emailLabel" runat="server"/></td></tr>
                                </table>   
                            </ContentTemplate>
                        </asp:UpdatePanel>  
                    </td>
                </tr>
            </table>    
        </asp:Panel> <%--end other panel--%>      
        <hr />
    </div> 
    
    <script type="text/javascript" language="javascript">
        function colorChange(sender, eventArgs) {
            var cb = grab("<%= newUserColourBox.ClientID%>");
            cb.value = sender.get_selectedColor();
        }
        function showHide(id) {
            obj = grab(id);
            if (obj.style.display == "none") { obj.style.display = "block"; }
            else { obj.style.display = "none"; }
            return false;
        }
        function toggleText(btn) {
            if (btn.innerText == 'Show Details') {
                btn.innerText = 'Hide Details';
            }
            else {
                btn.innerText = 'Show Details';
            }
            return false;
        }
        function openHelp() {
            try{ var oWnd = radopen("RolesBreakdownWindow.aspx", "RolesBreakdownWindow"); }
            catch(E){IE9Err();}
        }
        function openSBRole() {
            try{ var oWnd = radopen("SalesBookRoleWindow.aspx", "SalesBookRoleWindow"); }
            catch(E){IE9Err();}
        }
        function LowerTrimPWRQUsername() {
            grab("<%= img_pwr_loading.ClientID %>").style.display = 'block';
            grab('Body_PasswordRecovery1_UserNameContainerID_lbl_pwr_info').style.display = 'none';
            grab('Body_PasswordRecovery1_UserNameContainerID_UserName').value = 
            grab('Body_PasswordRecovery1_UserNameContainerID_UserName').value.trim().toLowerCase();
            return true;
        }
    </script> 
</asp:Content>
