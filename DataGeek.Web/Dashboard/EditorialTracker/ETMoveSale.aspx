<%--
Author   : Joe Pickering, 02/10/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="ETMoveSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETMoveSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" runat="server" border="0" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto; margin:16px; height:120px; width:300px;">
        <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" ForeColor="White" style="position:relative; left:-4px;"/><br/><br/></td></tr>
        <tr>   
            <td><asp:Label runat="server" Text="Move Feature to Issue:" ForeColor="DarkOrange" style="position:relative; top:-8px;"/></td>
            <td><telerik:RadDropDownList ID="dd_issue_list" runat="server" Width="140px" Skin="Vista" style="position:relative; top:-8px;"/></td>
        </tr>
        <tr>
            <td align="left"><asp:CheckBox ID="cb_rerun" runat="server" Checked="false" Text="Re-Run (Make a Copy)" ForeColor="Silver"/></td>
            <td align="right">
                <div style="position:relative; left:-7px; top:7px;">
                    <asp:LinkButton ID="lb_cancel" runat="server" Text="Cancel" OnClientClick="GetRadWindow().Close();" ForeColor="Silver"/>
                    <asp:LinkButton ID="lb_move" runat="server" Text="Move" OnClick="MoveFeature" ForeColor="Silver" style="padding-left:4px; border-left:solid 1px gray;"/>
                </div>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_region" runat="server"/>
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_old_issue_id" runat="server"/>
    <asp:HiddenField ID="hf_old_issue_name" runat="server"/>
    <asp:HiddenField ID="hf_this_feature" runat="server"/>
    <asp:HiddenField ID="hf_old_c_type" runat="server"/>
</asp:Content>