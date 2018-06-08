<%--
Author   : Joe Pickering, 24/09/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: MWD Group Stats" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="MWDGroupStats.aspx.cs" Inherits="MWDGroupStats" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">   
        <hr />

        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr><td colspan="2"><asp:Label ID="lbl_stats" runat="server" Font-Size="Large" ForeColor="White" /></td></tr>
            <tr>
                <td valign="top" width="45%">
                    <asp:GridView ID="gv_group_stats" runat="server" AutoGenerateColumns="true"
                        border="2" Width="450" Font-Name="Verdana" 
                        Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Left" 
                        CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_stats_RowDataBound">
                    </asp:GridView>
                </td>
                <td valign="top" align="left">
                    <asp:GridView ID="gv_office_status" runat="server" AutoGenerateColumns="true"
                        border="2" Width="330" Font-Name="Verdana" 
                        Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Left" 
                        CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_off_RowDataBound">
                    </asp:GridView>
                </td>
            </tr>
            <tr id="tr_return" runat="server" visible="false">
                <td align="center" colspan="2">
                    <p/>
                        <asp:Label runat="server" Text="Click" ForeColor="White" Font-Size="12" />
                        <asp:HyperLink runat="server" Text="here" ForeColor="Chocolate" NavigateUrl="~/Default.aspx" Font-Size="12"/>
                        <asp:Label runat="server" Text="to go back to the main page." ForeColor="White" Font-Size="12"/>
                    <p/>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>


