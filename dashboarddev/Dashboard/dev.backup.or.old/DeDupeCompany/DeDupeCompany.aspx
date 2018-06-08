<%--
// Author   : Joe Pickering, 15/05/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Company DeDupah" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="DeDupeCompany.aspx.cs" Inherits="DeDupeCompany" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div id="div_page" runat="server" class="normal_page">
    <hr />
    
    <table width="99%" border="0" cellpadding="0" cellspacing="0" style="font-family:Verdana; margin-left:auto; margin-right:auto;">
        <tr>
            <td>
                <asp:GridView ID="gv_dupe" runat="server" AutoGenerateColumns="true" ForeColor="Silver" Width="984" OnRowDataBound="gv_RowDataBound" RowStyle-HorizontalAlign="Center"/>
                <asp:HiddenField ID="hf_dupe_id" runat="server" />
                <asp:Label ID="lbl_dupes_left" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Large" />
            </td>
        </tr>
        <tr>
            <td align="center">
                <br /><br />
                <asp:Button ID="btn_merge" runat="server" Text="Yes, merge!" Height="100" Width="200" BackColor="Lime" OnClick="MergeDupe"/>
                <asp:Button ID="btn_skip" runat="server" Text="No, skip!" Height="100" Width="200" BackColor="Red" OnClick="SkipDupe"/>
            </td>
        </tr>
    </table>
    
    <hr />
</div> 
</asp:Content>

