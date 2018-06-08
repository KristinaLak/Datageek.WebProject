<%--
// Author   : Joe Pickering, 26/04/2011 -- re-written 27/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Status Summary" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="StatusSummary.aspx.cs" Inherits="StatusSummary" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />
       
            <table width="100%">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Status" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px; left:4px;"/> 
                        <asp:Label runat="server" Text="Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px; left:4px;"/> 
                    </td>
                    <td valign="top" align="right">
                        <div style="position:relative; top:-4px;">
                            <asp:Label runat="server" Text="Calendar Year:" ForeColor="DarkOrange" Font-Size="Smaller"/>
                            <asp:DropDownList runat="server" ID="dd_year" Width="100" AutoPostBack="true" OnSelectedIndexChanged="BindData"/>
                        </div>
                    </td>
                </tr>
            </table>
            
            <table border="0" width="99%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
                <tr>
                    <td colspan="2" valign="bottom">
                        <telerik:RadTabStrip ID="tabstrip" AutoPostBack="true" MaxDataBindDepth="1" runat="server" SelectedIndex="0"
                        BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" OnTabClick="BindData" style="position:relative; top:6px;"/>
                    </td>
                </tr>
                <tr>
                    <td width="55%" valign="top">
                        <asp:GridView runat="server" ID="gv" AutoGenerateColumns="false" Width="550" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                        Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2" Border="0" AllowSorting="true"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" RowStyle-HorizontalAlign="Center"
                        OnRowDataBound="gv_RowDataBound" OnSorting="gv_Sorting">
                            <Columns> 
                                <asp:BoundField HeaderText="Book Name" DataField="Book Name" SortExpression="StartDate, #"/>
                                <asp:BoundField HeaderText="Status" ItemStyle-ForeColor="DarkSlateGray" DataField="Status" SortExpression="Status"/>
                                <asp:BoundField HeaderText="#" DataField="#" SortExpression="#"/>
                                <asp:BoundField DataField="StartDate" DataFormatString="{0:dd/MM/yyyy}"/>
                                <asp:BoundField DataField="EndDate" DataFormatString="{0:dd/MM/yyyy}"/>
                                <asp:BoundField DataField="total_sales"/>
                                <asp:BoundField DataField="total_del"/>
                                <asp:BoundField DataField="total_rl"/>
                            </Columns>
                        </asp:GridView>
                    </td>
                    <td valign="top">
                        <table width="100%" bgcolor="gray" style="font-family:Verdana; color:White; border:solid 2px darkgray; border-radius:5px;">
                            <tr>
                                <td width="50%">Book Name</td>
                                <td><asp:Label runat="server" ID="lbl_bookname" Font-Bold="true"/></td>
                            </tr>
                            <tr>
                                <td>Start Date</td>
                                <td><asp:Label runat="server" ID="lbl_startdate"/></td>
                            </tr>
                            <tr>
                                <td>End Date</td>
                                <td><asp:Label runat="server" ID="lbl_enddate"/></td>
                            </tr>
                            <tr>
                                <td>Total Sales</td>
                                <td><asp:Label runat="server" ID="lbl_totalsales"/></td>
                            </tr>
                            <tr>
                                <td>Total Deleted Sales</td>
                                <td><asp:Label runat="server" ID="lbl_totaldelsales"/></td>
                            </tr>
                            <tr>
                                <td>Total Red Lines In Issue</td>
                                <td><asp:Label runat="server" ID="lbl_totalredlines"/></td>
                            </tr>
                        </table>
                        <br />
                    </td>
                </tr>
            </table>
        <hr />
    </div>
    
    <script type="text/javascript">
        var lbl_bn = grab("<%= lbl_bookname.ClientID %>");
        var lbl_sd = grab("<%= lbl_startdate.ClientID %>");
        var lbl_ed = grab("<%= lbl_enddate.ClientID %>");
        var lbl_total = grab("<%= lbl_totalsales.ClientID %>");
        var lbl_totaldel = grab("<%= lbl_totaldelsales.ClientID %>");
        var lbl_totalrl = grab("<%= lbl_totalredlines.ClientID %>");
        function showstats(bn, sd, ed, total, totaldel, totalrl) {
            if (bn != '' && bn != '&nbsp;') {
                lbl_bn.innerHTML = bn;
                lbl_sd.innerHTML = sd;
                lbl_ed.innerHTML = ed;
                lbl_total.innerHTML = total;
                lbl_totaldel.innerHTML = totaldel;
                lbl_totalrl.innerHTML = totalrl;
            }
            else if(bn == ''){
                clearstats();
            }
            return false;
        }
        function clearstats() {
            lbl_bn.innerHTML = '';
            lbl_sd.innerHTML = '';
            lbl_ed.innerHTML = '';
            lbl_total.innerHTML = '';
            lbl_totaldel.innerHTML = '';
            lbl_totalrl.innerHTML = '';
            return false;
        }
    </script>   
</asp:Content>
