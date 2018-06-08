<%--
Author   : Joe Pickering, 04/10/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="ETWatchSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETWatchSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" width="90%" border="0" runat="server" style="font-family:Verdana; font-size:8pt; overflow:visible; margin-top:4px; padding:15px;">
        <tr><td><asp:Label ID="lbl_title" runat="server" ForeColor="White" style="position:relative; left:-4px;"/><br/><br/></td></tr>
        <tr><td><asp:Label runat="server" Text="Watch in which issue:" ForeColor="DarkOrange"/></td></tr>
        <tr><td><asp:DropDownList ID="dd_issue_list" runat="server" Width="210px"/></td></tr>
        <tr>
            <td align="right">
                <div style="position:relative; left:-7px; top:7px;">
                    <asp:LinkButton ID="lb_cancel" runat="server" Text="Cancel" OnClientClick="GetRadWindow().Close();" ForeColor="Silver"/>
                    <asp:Label runat="server" Text=" | " ForeColor="Silver" />
                    <asp:LinkButton ID="lb_watch" runat="server" Text="Watch" OnClick="WatchSale" ForeColor="Silver"/>
                </div>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_existing_issue_id" runat="server"/>
    <asp:HiddenField ID="hf_this_feature" runat="server"/>
</asp:Content>