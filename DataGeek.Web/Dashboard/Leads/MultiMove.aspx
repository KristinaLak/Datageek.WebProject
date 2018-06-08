<%--
// Author   : Joe Pickering, 06/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MultiMove.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MultiMove" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="height:200px;">
        <asp:UpdatePanel ID="udp_move" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table ID="tbl_move_leads" runat="server" class="WindowTableContainer">
                    <tr><td><asp:Label ID="lbl_title" runat="server" Text="Move your selected <b>Leads</b>.." CssClass="MediumTitle"/></td></tr>
                    <tr><td><asp:Label runat="server" CssClass="SmallTitle" Text="Select a Project/Client List you'd like to move your selected Leads to.."/></td></tr>
                    <tr><td><telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" Width="100%"/></td></tr>
                    <tr><td><telerik:RadDropDownList ID="dd_buckets" runat="server" Width="100%"/></td></tr>
                    <tr><td align="right">
                        <br /><br /><br />
                        <telerik:RadButton ID="btn_move_leads" runat="server" Text="Move Selected Leads" Skin="Bootstrap" AutoPostBack="false" 
                            OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_move_leads_serv', false);  }"/>
                        <asp:Button ID="btn_move_leads_serv" runat="server" OnClick="MoveSelectedLeads" OnClientClick="AlertifySuccess('Processing.. please wait.', 'bottom-right');" style="display:none;"/>
                    </td></tr>
                </table>
                <asp:HiddenField ID="hf_lead_ids" runat="server"/>
                <asp:HiddenField ID="hf_project_id" runat="server"/>
                <asp:HiddenField ID="hf_bucket_id" runat="server"/>
                <asp:HiddenField ID="hf_from_search" runat="server"/>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>