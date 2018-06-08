<%--
Author   : Joe Pickering, 29/05/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FNViewSummaries.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNViewSummaries" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>

    <table ID="tbl" runat="server" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;" width="500">
        <tr>
            <td><asp:Label ID="lbl_total" runat="server" ForeColor="White" Font-Bold="true"/></td>
        </tr>
        <tr>
            <td><asp:GridView id="gv_reports" runat="server" AutoGenerateColumns="true"/></td>
        </tr>
    </table>
</asp:Content>