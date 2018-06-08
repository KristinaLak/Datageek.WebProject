<%--
// Author   : Joe Pickering, 28/02/18
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ManageWorld.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ManageWorld" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>

    <div class="WindowDivContainer" style="height:300px;">
        <table class="WindowTableContainer" style="padding:18px; width:550px; margin-right:20px;">
            <tr style="height:30px;"><td colspan="2"><asp:Label ID="lbl_title" runat="server" Text="Manage World" CssClass="MediumTitle"/></td></tr>
            <tr style="height:30px;"><td colspan="2"><asp:Label ID="lbl_sub_title" runat="server" CssClass="SmallTitle"/></td></tr>
            <tr>
                <td colspan="2">
                    <div ID="div_industries" runat="server">

                    </div>
                </td>
            </tr>
            <tr>
                <td align="right" colspan="2">
                    <telerik:RadButton ID="btn_add_sub_industry" runat="server" Text="Add New Sub Industry" Skin="Bootstrap" Visible="false" AutoPostBack="false"
                    OnClientClicking="function(b,args){alertify.prompt('Add a Sub Industry', 'Enter a new Sub Industry Name', '', function(evt, value){ grab('Body_hf_sub_industry').value = value; grab('Body_btn_add_sub_industry_serv').click(); }, null);}"/>
                    <asp:HiddenField ID="hf_sub_industry" runat="server"/>
                    <asp:HiddenField ID="hf_sub_industry_id" runat="server"/>
                    <asp:Button ID="btn_add_sub_industry_serv" runat="server" OnClick="AddSubIndustry" style="display:none;"/>
                    <asp:Button ID="btn_rename_sub_industry_serv" runat="server" OnClick="RenameOrDeleteSubIndustry" style="display:none;"/>
                    <asp:Button ID="btn_del_sub_industry_serv" runat="server" OnClick="RenameOrDeleteSubIndustry" style="display:none;"/>
                </td>
            </tr>
        </table>
    </div>

    <asp:HiddenField ID="hf_world_id" runat="server"/>
    <asp:HiddenField ID="hf_world_industry_id" runat="server"/>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>