<%--
// Author   : Joe Pickering, 25/06/15
// For      : WDM Group - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Account Management" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="AccountManagement.aspx.cs" Inherits="AccountManagement" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>


<asp:Content ContentPlaceHolderID="Head" runat="server">
    <link rel="stylesheet" type="text/css" href="CSS/accountmanagement.css"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
   
    <div ID="div_page" runat="server" class="normal_page">
        <hr />
        
        <asp:UpdatePanel ID="udp_users" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <ajax:AnimationExtender ID="ae_select_user" runat="server" TargetControlID="dd_select_user" Enabled="false">
                  <Animations>
                    <OnLoad>
                        <Sequence>
                          <Color AnimationTarget="dd_select_user" Duration="0.9" StartValue="#90ee90" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
                        </Sequence>
                    </OnLoad>
                  </Animations>
                </ajax:AnimationExtender>  
            
                <asp:Button ID="btn_new_user" runat="server" Text="Add New User" OnClick="NewProfile"/>
                <table ID="tbl_select_user" runat="server" class="ContainerTable"> 
                    <tr>
                        <td><asp:Label runat="server" Text="Office:" CssClass="SmallTitle"/></td>
                        <td><asp:DropDownList ID="dd_select_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindUsersAndTeamsInSelectedOffice"/></td>
                        <td><asp:Label runat="server" Text="User:" CssClass="SmallTitle"/></td>
                        <td><asp:DropDownList ID="dd_select_user" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindSelectedUserProfile"/></td>
                        <td><asp:CheckBox ID="cb_select_include_unemployed" runat="server" Checked="false" Text="Show Terminated Employees" AutoPostBack="true" OnCheckedChanged="BindUsersAndTeamsInSelectedOffice"/></td>
                    </tr>
                </table>
        
                <table ID="tbl_user_profile" runat="server" class="ContainerTable" visible="false">
                    <tr>
                        <td colspan="2"><asp:Label ID="lbl_add_edit_user_title" runat="server" CssClass="MediumTitle"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Full Name" CssClass="SmallTitle"/></td>
                        <td><asp:TextBox ID="tb_user_full_name" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Friendly Name (short)" CssClass="SmallTitle"/></td>
                        <td>
                            <asp:TextBox ID="tb_user_friendly_name" runat="server"/>
                            <asp:HiddenField ID="hf_user_friendly_name" runat="server"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="E-mail" CssClass="SmallTitle"/></td>
                        <td>
                            <asp:TextBox ID="tb_user_email" runat="server"/>
                            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                            ControlToValidate="tb_user_email" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Phone" CssClass="SmallTitle"/></td>
                        <td><asp:TextBox ID="tb_user_phone" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Magazine" CssClass="SmallTitle"/></td>
                        <td><asp:DropDownList ID="dd_user_magazine" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Sector" CssClass="SmallTitle"/></td>
                        <td><asp:DropDownList ID="dd_user_sector" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Sub-Sector" CssClass="SmallTitle"/></td>
                        <td><asp:TextBox ID="tb_user_sub_sector" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Office" CssClass="SmallTitle"/></td>
                        <td>
                            <asp:DropDownList ID="dd_user_office" runat="server"/>
                            <asp:HiddenField ID="hf_user_office" runat="server"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Account Type" CssClass="SmallTitle"/></td>
                        <td>
                            <asp:DropDownList ID="dd_user_account_type" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CheckUserCCAType">
                                <asp:ListItem Value="db_Custom" Text="Custom"/>
                                <asp:ListItem Value="db_Admin" Text="Admin"/>
                                <asp:ListItem Value="db_HoS" Text="Head of Sales"/>
                                <asp:ListItem Value="db_Finance" Text="Finance"/>
                                <asp:ListItem Value="db_GroupUser" Text="Group User"/>
                                <asp:ListItem Value="db_User" Text="User"/>
                                <asp:ListItem Value="db_CCA" Text="CCA"/>
                            </asp:DropDownList>
                            <asp:Image runat="server" Height="20" Width="20" ImageUrl="~/images/leads/lead_info.png" ToolTip="Help" CssClass="HandCursor" style="position:relative; top:5px;"/>
                        </td>
                    </tr>
                    <tr ID="tr_cca_type" runat="server" visible="false">
                        <td><asp:Label runat="server" Text="CCA Type" CssClass="SmallTitle" Font-Bold="true"/></td>
                        <td>
                            <asp:DropDownList ID="dd_user_cca_type" runat="server">
                                <asp:ListItem Text="Sales (Sales Director)" Value="-1"/>
                                <asp:ListItem Text="List Generator (Research Director)" Value="2"/>
                                <asp:ListItem Text="Commission Only" Value="1"/>
                            </asp:DropDownList>
                            <asp:HiddenField ID="hf_user_cca_type" runat="server"/>
                        </td>
                    </tr>
                    <tr ID="tr_cca_team" runat="server" visible="false">
                        <td><asp:Label runat="server" Text="CCA Team" CssClass="SmallTitle" Font-Bold="true"/></td>
                        <td><asp:DropDownList ID="dd_user_cca_team" runat="server"/></td>
                    </tr>
                    <tr ID="tr_cca_t_and_d_commission_toggle" runat="server" visible="false">
                        <td align="left"><asp:Label runat="server" Text="CCA T&D Commission" CssClass="SmallTitle" Font-Bold="true"/></td>
                        <td><asp:CheckBox ID="cb_t_and_d_commission_toggle" runat="server" AutoPostBack="true" OnCheckedChanged="ToggleTandDCommission"/></td>
                    </tr>
                    <tr ID="tr_cca_t_and_d_commission" runat="server" visible="false">
                        <td colspan="2">
                            <table width="50%">
                                <tr><td colspan="2"><asp:Label runat="server" CssClass="SmallTitle" Text="A trainer will receive X commission on all of this user's deals for the first 90 days of their employ."/></td></tr>
                                <tr>
                                    <td><asp:Label runat="server" Text="Percentage" CssClass="SmallTitle"/></td>
                                    <td><asp:Label runat="server" Text="Trainer" CssClass="SmallTitle"/></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="tb_t_and_d_percentage" runat="server" Text="2.5" Width="80"/>
                                        <asp:CompareValidator runat="server" ControlToValidate="tb_t_and_d_percentage" Operator="GreaterThan" ForeColor="Red" Type="Double" Display="Dynamic" ValueToCompare="0" Text="<br/>Must be a valid number"/> 
                                        <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_t_and_d_percentage" Display="Dynamic" Text="Required!" Enabled="false"/>
                                    </td>
                                    <td><asp:DropDownList ID="dd_t_and_d_recipient" runat="server"/></td>
                                </tr>
                                <tr><td colspan="2"><asp:CheckBox ID="cb_t_and_d_update" runat="server" Text="Apply T&D changes to any existing deals?" Checked="false" CssClass="SmallTitle"/></td></tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Currently Employed" CssClass="SmallTitle"/></td>
                        <td><asp:CheckBox ID="cb_user_employed" runat="server" CssClass="SmallTitle" style="position:relative; left:-2px;" 
                        onclick="if(this.checked){Alertify('If you are trying to re-enable this account, make sure you also click the Unlock Account button below - this allows the user to log in.','Also Unlock');};"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Starter" CssClass="SmallTitle"/></td>
                        <td><asp:CheckBox ID="cb_user_starter" runat="server"/></td>
                    </tr>
                    <tr ID="tr_user_date_added" runat="server">
                        <td><asp:Label runat="server" Text="Date Added" CssClass="SmallTitle"/></td>
                        <td><asp:Label ID="lbl_user_date_added" runat="server" CssClass="SmallTitle"/></td>
                    </tr>
                    <tr ID="tr_user_date_last_updated" runat="server">
                        <td><asp:Label runat="server" Text="Date Last Updated" CssClass="SmallTitle"/></td>
                        <td><asp:Label ID="lbl_user_date_last_updated" runat="server" CssClass="SmallTitle"/></td>
                    </tr>
                    <tr ID="tr_user_date_last_login" runat="server">
                        <td><asp:Label runat="server" Text="Date Last Logged In" CssClass="SmallTitle"/></td>
                        <td><asp:Label ID="lbl_user_date_last_login" runat="server" CssClass="SmallTitle"/></td>
                    </tr>
                    <tr>
                        <td valign="top"><asp:Label runat="server" Text="Dashboard Colour" CssClass="SmallTitle"/></td>
                        <td><div ID="div_rcp_user_colour" runat="server" /><telerik:RadColorPicker ID="rcp_user_colour" runat="server" Preset="Default" Width="200px" PreviewColor="True" ShowEmptyColor="False" Skin="Vista" SelectedColor="White" EnableViewState="false"/></td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:CreateUserWizard ID="CreateUserWizard" AutoGeneratePassword="true" ContinueButtonStyle-CssClass="invisible" runat="server" LoginCreatedUser="false" OnCreatingUser="OnCreatingUser" OnCreatedUser="OnCreatedUser">
                                <WizardSteps>
                                    <asp:CreateUserWizardStep ID="CreateUserWizardStep" runat="server" Title="Create this new user.<br/>(Ensure at least <b>Full Name</b>, <b>Friendly Name</b> and <b>Office</b> are provided)"/>
                                </WizardSteps>
                            </asp:CreateUserWizard>
                            <br />
                            <asp:CheckBox ID="cb_new_user_force_new_pw" runat="server" Checked="true" Text="Force change password on first login" 
                            ToolTip="Force this user to change their password when first logging in." style="position:relative; top:-22px;"/>
                            <asp:Label runat="server" Text="<br/>Example username: jsmith"/><br />
                            <asp:Label runat="server" Text="Example e-mail: john.smith@bizclikmedia.com"/>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <br />
                            <asp:Button ID="btn_force_new_pw" runat="server" ToolTip="Force this user to change their password on their next request." OnClick="ForceUserPasswordReset" Text="Force Password Change" CausesValidation="false"/>
                            <asp:Button ID="btn_unlock_user" runat="server" ToolTip="Unlock this user's account." OnClick="UnlockUserAccount" Text="Unlock Account" Visible="false" CausesValidation="false"/>
                            <asp:Button ID="btn_update_user" runat="server" Text="Update User" OnClick="UpdateUser" visible="false"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <hr />
    </div> 
    
    <script type="text/javascript" language="javascript">
    </script> 
</asp:Content>
