<%--
Author   : Joe Pickering, 27/10/2010 - re-written 28/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: GSD" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="GSD.aspx.cs" Inherits="GSD" %>
 
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="position:relative; left:4px; top:-2px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="GSD" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        
        <table id="tbl_main" runat="server" width="99%" style="margin-left:auto; margin-right:auto; color:White;">
            <tr>
                <td valign="bottom">
                    <asp:Label runat="server" Text="Pick Year:" Font-Size="8"/>
                    <asp:DropDownList ID="dd_year" runat="server" AutoPostBack="true" OnSelectedIndexChanged="SetProgressReports"/>
                    <asp:Label runat="server" Text="&nbsp;Pick Progress Report:" Font-Size="8"/>
                    <asp:DropDownList ID="dd_pr" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GenerateGSD"/>
                </td>
                <td valign="bottom" align="left">
                    <asp:Label ID="lbl_comments" runat="server" ForeColor="White" Text="Comments&nbsp;"/>
                    <asp:LinkButton ID="lb_bullets" runat="server" ForeColor="Gray" Font-Size="Smaller" Text="(Add Bullets)" 
                        OnClientClick="return addBullets('Body_tb_comments')"/> 
                </td>
                <td align="right" valign="bottom">
                    <asp:Label runat="server" ID="lbl_calltimes" Text="Call Time"/>
                    <asp:TextBox runat="server" ID="tb_calltimes" Width="66"/>
                    <asp:Label runat="server" ID="lbl_callsmade" Text="Calls Made"/>
                    <asp:TextBox runat="server" ID="tb_callsmade" Width="65"/>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:GridView border="2" ID="gv" runat="server" ForeColor="Black" 
                        HeaderStyle-ForeColor="White" 
                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2"
                        RowStyle-HorizontalAlign="Center" Width="480px" 
                        HeaderStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="false"
                        OnRowDataBound="gv_RowDataBound"
                        RowStyle-BackColor="#f0f0f0" 
                        AlternatingRowStyle-BackColor="#b0c4de" 
                        HeaderStyle-BackColor="#444444" style="position:relative; top:2px;">
                        <Columns>
                            <asp:BoundField HeaderText="Territory" DataField="Territory" />
                            <asp:BoundField HeaderText="Week Start" DataField="StartDate" DataFormatString="{0:dd/MM/yyyy}"/>
                            <asp:BoundField HeaderText="CCAs" DataField="CCAs" />
                            <asp:BoundField HeaderText="S" DataField="S" />
                            <asp:BoundField HeaderText="P" DataField="P" />
                            <asp:BoundField HeaderText="A" DataField="A" />
                            <asp:BoundField HeaderText="Total Revenue" DataField="Total Revenue" />
                            <asp:BoundField HeaderText="Call Time" DataField="ct" />
                            <asp:BoundField HeaderText="Total Calls" DataField="tc" />
                        </Columns>
                    </asp:GridView>  
                </td>
                <td align="left" valign="top" colspan="2">
                    <asp:TextBox ID="tb_comments" runat="server" TextMode="MultiLine" Width="490" />
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Panel runat="server" ID="pnl_callstats"/>
                </td>
            </tr>
        </table>
        
        <table width="99%" style="margin-left:auto; margin-right:auto; position:relative; top:-6px;"><tr>
            <td align="left" valign="top">
                <asp:Label runat="server" Text="This report will be sent to Admins only." 
                Font-Size="7" Font-Names="Verdana" ForeColor="White" />
            </td>
            <td align="right" valign="top">
                <table cellpadding="0" cellspacing="0" style="position:relative; left:2px;">
                    <tr>
                        <td>
                            <telerik:RadSpell ID="RadSpell" runat="server" Skin="Black" ControlToCheck="tb_comments" 
                            SupportedLanguages="en-US,English" DictionaryPath="~/App_Data/RadSpell/"/>
                        </td>
                        <td>
                            <asp:Button runat="server" ID="btn_send" Text="Send" OnClick="SendReport"/>
                        </td>
                    </tr>
                </table> 
            </td>
        </tr></table>

        <hr />
    </div>
    
    <script type="text/javascript">
        function addBullets(id) {
            obj = document.getElementById(id);
            obj.value += '•\n\n•\n\n•';
            return false;
        }
    </script>
</asp:Content>

