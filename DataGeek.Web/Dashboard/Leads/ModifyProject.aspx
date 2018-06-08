<%--
// Author   : Joe Pickering, 06/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ModifyProject.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ModifyProject" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer">
        <table class="WindowTableContainer">
            <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/></td></tr>
            <tr>
                <td width="33%"><asp:Label runat="server" Text="Project name:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_project_name" runat="server" AutoCompleteType="Disabled"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Target industry:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_target_industry" runat="server" Width="250"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Target territory:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_target_territory" runat="server" Width="250"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Description:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_project_description" runat="server" TextMode="MultiLine" Height="60" Width="98%" AutoCompleteType="Disabled"/></td>
            </tr>
            <tr ID="tr_sharing_management" runat="server" visible="false"> 
                <td valign="top"><asp:Label runat="server" Text="Sharing Management:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadTreeView ID="rtv_share_recipients" runat="server" CheckBoxes="true">
                        <Nodes>
                            <telerik:RadTreeNode Text="Shared With:" Checkable="false" Expanded="true"/>
                        </Nodes>
                    </telerik:RadTreeView>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <br />
                    <telerik:RadButton ID="btn_modify_project" runat="server" Text="Update" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_modify_proj_serv', false);}"/>
                    <asp:Button ID="btn_modify_proj_serv" runat="server" OnClick="ModifyThisProject" style="display:none;"/>
                </td>
            </tr>
        </table>
        
        <asp:UpdatePanel ID="udp_move" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table ID="tbl_move_leads" runat="server" visible="false" class="WindowTableContainer">
                    <tr><td colspan="2"><asp:Label runat="server" Text="Move <b>Leads</b>.." CssClass="MediumTitle"/></td></tr>
                    <tr><td colspan="2"><asp:Label ID="lbl_title_move" runat="server" CssClass="SmallTitle"/></td></tr>
                    <tr>
                        <td><telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" Width="100%"/></td>
                        <td><telerik:RadDropDownList ID="dd_buckets" runat="server" Width="100%"/></td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <br />
                            <telerik:RadButton ID="btn_move_leads" runat="server" Text="Move All Leads" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_move_leads_serv', false);}"/>
                            <asp:Button ID="btn_move_leads_serv" runat="server" OnClick="MoveAllLeads" style="display:none;"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <table ID="tbl_delete_project" runat="server" visible="false" class="WindowTableContainer">
            <tr><td colspan="2"><asp:Label ID="lbl_header_delete" runat="server" Text="Delete this <i>entire</i> <b>Project</b>.." CssClass="MediumTitle"/></td></tr>
            <tr><td colspan="2"><asp:Label ID="lbl_title_delete" runat="server" CssClass="SmallTitle"/></td></tr>
            <tr ID="tr_delete_select" runat="server"><td colspan="2"><asp:Label runat="server" Text="Move existing Leads to:" CssClass="SmallTitle"/></td></tr>
            <tr ID="tr_delete_select_2" runat="server">
                <td><telerik:RadDropDownList ID="dd_del_move_project" runat="server" Width="100%" ExpandDirection="Up" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets"/></td>
                <td><telerik:RadDropDownList ID="dd_del_move_bucket" runat="server" Width="100%" ExpandDirection="Up" Enabled="false"/></td>
            </tr>
            <tr>
                <td align="left">
                    <br />
                    <telerik:RadButton ID="btn_move_or_share" runat="server" Text="Move or Share Project" AutoPostBack="false" Skin="Bootstrap"/>
                </td>
                <td align="right">
                    <br />
                    <telerik:RadButton ID="btn_delete_project" runat="server" Text="Delete Entire Project" Skin="Bootstrap" AutoPostBack="false" 
                        OnClientClicking="function(button, args){ AlertifyConfirm('This will delete this <b>entire</b> Project, its Client Lists and all its Leads, not just the selected Client List.<br/><br/>If you want to delete just a custom Client List, first select it and then use the pencil icon near the Client List title.<br/><br/>Any deleted Projects or Client Lists can be restored at any time using the <b>Restore Project/Client List</b> tool in the toolbar to the left.<br/><br/>Are you sure you want to delete this <b>entire</b> Project?', 'Sure?', 'Body_btn_delete_project_serv', false);}"/>
                    <asp:Button ID="btn_delete_project_serv" runat="server" OnClick="DeactivateProject" style="display:none;"/>
                    <asp:Button ID="btn_remove_project_serv" runat="server" OnClick="RemoveSharedProject" style="display:none;"/>
                </td>
            </tr>
        </table>
    </div>

<asp:HiddenField ID="hf_mode" runat="server"/>
<asp:HiddenField ID="hf_project_id" runat="server"/>
<asp:HiddenField ID="hf_parent_project_id" runat="server"/>
<asp:HiddenField ID="hf_user_id" runat="server"/>
</asp:Content>