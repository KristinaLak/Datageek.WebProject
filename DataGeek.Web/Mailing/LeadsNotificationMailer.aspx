<%--
// Author   : Joe Pickering, 02.09.16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="LeadsNotificationMailer.aspx.cs" Inherits="LeadsNotificationMailer" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<div ID="div_main" runat="server" class="LeadsBody" style="height:100%; position:relative;">
    <div ID="div_container" runat="server">
        <div ID="div_action_today" runat="server">
            <asp:Label ID="lbl_leads_action_today_title" runat="server" Text="Here's a summary of your Leads that need actioning today:" Font-Bold="true"/>
            <asp:GridView ID="gv_leads_action_today" runat="server"
                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="100%"
                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
                RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                AutoGenerateColumns="true" RowStyle-ForeColor="Black" OnRowDataBound="gv_leads_RowDataBound">
            </asp:GridView>
        </div>
        <br/>
        <div ID="div_action_outstanding" runat="server">
            <asp:Label ID="lbl_leads_action_outstanding_title" runat="server" Text="Here's a summary of your outstanding Leads that need actioning:" Font-Bold="true"/>
            <asp:GridView ID="gv_leads_action_outstanding" runat="server"
                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="100%"
                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
                RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                AutoGenerateColumns="true" RowStyle-ForeColor="Black" OnRowDataBound="gv_leads_RowDataBound">
            </asp:GridView>
        </div>
    </div>
    <asp:Label ID="lbl_log" runat="server" Text="Users who've received summaries:" CssClass="TinyTitle"/>
</div>
</asp:Content>

