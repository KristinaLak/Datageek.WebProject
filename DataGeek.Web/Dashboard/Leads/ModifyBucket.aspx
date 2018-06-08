<%--
// Author   : Joe Pickering, 20/11/15
// For      : WDM Group, Leads Bucket.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ModifyBucket.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ModifyBucket" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:500px;">
        <table class="WindowTableContainer" style="height:150px;">
            <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/></td></tr>
            <tr>
                <td width="20%"><asp:Label runat="server" Text="Client List Name:" CssClass="SmallTitle"/></td>
                <td><asp:TextBox ID="tb_bucket_name" runat="server" Width="300"/></td>
            </tr>
            <tr><td colspan="2" align="right"><asp:Label ID="lbl_cant_rename" runat="server" Text="Default Client Lists cannot be renamed." CssClass="TinyTitle" Visible="false" style="margin:0; position:relative; top:-5px;"/><telerik:RadButton ID="btn_modify_bucket" runat="server" OnClick="ModifyThisBucket" Skin="Bootstrap"/></td></tr>
        </table>

        <asp:UpdatePanel ID="udp_move" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table ID="tbl_move_leads" runat="server" class="WindowTableContainer" visible="false">
                    <tr><td colspan="2"><asp:Label runat="server" Text="Move all <b>Leads</b> from this Client List.." CssClass="MediumTitle"/></td></tr>
                    <tr><td colspan="2"><asp:Label runat="server" CssClass="SmallTitle" Text="Select a Project you'd like to move <b>all</b> this Client List's <b>Leads</b> to"/></td></tr>
                    <tr>
                        <td width="50%"><telerik:RadDropDownList ID="dd_move_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" Width="100%"/></td>
                        <td width="50%"><telerik:RadDropDownList ID="dd_move_buckets" runat="server" Width="100%"/></td>
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

        <asp:UpdatePanel ID="udp_del" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table ID="tbl_delete_bucket" runat="server" Visible="false" class="WindowTableContainer">
                    <tr><td colspan="2"><asp:Label runat="server" Text="Delete this <b>Client List</b>.." CssClass="MediumTitle"/></td></tr>
                    <tr><td colspan="2"><asp:Label ID="lbl_title_delete" runat="server" CssClass="SmallTitle"/></td></tr>
                    <tr>
                        <td><telerik:RadDropDownList ID="dd_del_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" Width="100%" ExpandDirection="Up"/></td>
                        <td><telerik:RadDropDownList ID="dd_del_buckets" runat="server" Width="100%" ExpandDirection="Up"/></td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <br />
                            <telerik:RadButton ID="btn_delete_bucket" runat="server" Text="Delete Client List" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?<br/><br/>Any deleted Projects or Client Lists can be restored at any time using the <b>Restore Project/Client List</b> tool in the toolbar to the left.', 'Sure?', 'Body_btn_delete_bucket_serv', false);}"/>
                            <asp:Button ID="btn_delete_bucket_serv" runat="server" OnClick="DeleteThisBucket" style="display:none;" CausesValidation="false"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:HiddenField ID="hf_mode" runat="server"/>
        <asp:HiddenField ID="hf_project_id" runat="server"/>
        <asp:HiddenField ID="hf_parent_project_id" runat="server"/>
    </div>
</asp:Content>