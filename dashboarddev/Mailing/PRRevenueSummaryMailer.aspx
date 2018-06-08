<%--
// Author   : Joe Pickering, 11/07/14
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="PRRevenueSummaryMailer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="PRRevenueSummaryMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
    
    <div id="div_page" runat="server" class="normal_page">

        <asp:GridView ID="gv" runat="server" AutoGenerateColumns="true" Width="300"
            Border="2" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Center" 
            CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_daysweeks_RowDataBound">
        </asp:GridView>
                              
    </div>

</asp:Content>

