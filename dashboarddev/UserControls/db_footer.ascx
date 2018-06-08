<%@ Control Language="C#" AutoEventWireup="true" CodeFile="db_footer.ascx.cs" Inherits="Footer" %>

<div ID="div_footer" runat="server" class="Footer">
    <table class="FooterContainer">
        <tr>
            <td><asp:Label ID="lbl_company" runat="server"/></td>
            <td align="center" class="FooterBreak"></td>
            <td><asp:HyperLink ID="hl_company_url" runat="server" Target="_blank"/></td>
            <td align="center" class="FooterBreak"></td>
            <td><asp:HyperLink ID="hl_smartsocial_url" runat="server" Target="_blank"/></td>
        </tr>
    </table>
</div>