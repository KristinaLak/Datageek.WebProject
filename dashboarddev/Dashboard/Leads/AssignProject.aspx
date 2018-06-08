<%--
// Author   : Joe Pickering, 18/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="AssignProject.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="AssignProject" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="height:190px; width:400px;">
        <table class="WindowTableContainer">
            <tr><td><asp:Label ID="lbl_title" runat="server" Text="Select a user to move this <b>Project</b> to, or select a user to share this <b>Project</b> with.." CssClass="MediumTitle"/></td></tr>
            <tr>
                <td>
                    <telerik:RadDropDownList ID="dd_move_or_share" runat="server" Width="300" style="margin-bottom:4px;">
                        <Items>
                            <telerik:DropDownListItem Text="Move Project to.."/>
                            <telerik:DropDownListItem Text="Share Project with.."/>
                        </Items>
                    </telerik:RadDropDownList><br />
                    <telerik:RadDropDownList ID="dd_recipient_user" runat="server" Width="300"/>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <br />
                    <telerik:RadButton ID="btn_assign" runat="server" Text="Assign Project" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_assign_serv', false); }"/>
                    <asp:Button ID="btn_assign_serv" runat="server" OnClick="MoveOrShareProject" style="display:none;"/>
                </td>
            </tr>
        </table>
        <asp:HiddenField ID="hf_project_id" runat="server"/>
        <asp:HiddenField ID="hf_cca_action" runat="server"/>
    </div>
</asp:Content>