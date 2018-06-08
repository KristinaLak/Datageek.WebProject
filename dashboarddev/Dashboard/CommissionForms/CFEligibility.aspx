<%--
Author   : Joe Pickering, 29/05/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="true" CodeFile="CFEligibility.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="CFEligibility" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; overflow:visible; padding:18px;">
        <tr>
            <td>
                <asp:GridView ID="gv_sales" runat="server" AutoGenerateColumns="false"
                Width="830" OnRowDataBound="gv_RowDataBound" RowStyle-HorizontalAlign="Center"
                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                AlternatingRowStyle-BackColor="LightSteelBlue" RowStyle-BackColor="#f0f0f0"
                RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead">
                    <Columns>
                        <asp:BoundField DataField="ent_id"/>
                        <asp:BoundField HeaderText="Advertiser" DataField="advertiser"/>
                        <asp:BoundField HeaderText="Feature" DataField="feature"/>
                        <asp:BoundField HeaderText="Size" DataField="size"/>
                        <asp:BoundField HeaderText="Price" DataField="price"/>
                        <asp:BoundField HeaderText="Invoice" DataField="invoice"/>
                        <asp:CheckBoxField HeaderText="Eligible" DataField="eligible"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
        <tr><td align="right"><telerik:RadButton ID="btn_save" runat="server" Skin="Bootstrap" Text="Save Sale Eligibility" OnClick="SaveEligibility"/></td></tr>
    </table>
    
    <asp:HiddenField ID="hf_form_id" runat="server" />

</asp:Content>