<%--
// Author   : Joe Pickering, 13.05.16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="LeadsStatsMailer.aspx.cs" Inherits="LeadsStatsMailer" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<div ID="div_main" runat="server" class="LeadsBody" style="height:100%; position:relative;">
    <div ID="div_container" runat="server" style="margin:18px;">
        <asp:GridView ID="gv_analytics" runat="server"
            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="100%"
            HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
            RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
            AutoGenerateColumns="true" RowStyle-ForeColor="Black" OnRowDataBound="gv_analytics_RowDataBound">
        </asp:GridView>   
    </div>
</div>
</asp:Content>

