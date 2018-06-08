<%--
// Author   : Joe Pickering, 08.05.15
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="ProspectSummaryMailer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="ProspectSummaryMailer" %>
<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <asp:GridView ID="gv_prospects" runat="server" Border="2" OnRowDataBound="gv_RowDataBound" Width="750" RowStyle-HorizontalAlign="Center"
            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" AutoGenerateColumns="true" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
    </div>
</asp:Content>

