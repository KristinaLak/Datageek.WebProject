<%--
Author   : Joe Pickering, 09/04/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: LHA Report" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="LHAReport.aspx.cs" Inherits="LHAReport" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <telerik:RadToolTipManager ID="rttm" runat="server" Animation="Resize" ShowDelay="40" Title="<i><font color='Black' size='2'>LHA Report:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></i>" 
     ManualClose="true" RelativeTo="Element" Sticky="true" OffsetY="0" Skin="Vista" ShowEvent="OnRightClick" OnClientShow="ResizeRadToolTip" AutoTooltipify="true"/>
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="True">
        <Windows>
            <telerik:RadWindow ID="win_newlha" runat="server" Title="&nbsp;New LHA" Behaviors="Move, Close, Pin" OnClientClose="NewLHAOnClientClose"/>  
            <telerik:RadWindow ID="win_editlha" runat="server" Title="&nbsp;Edit LHA" Behaviors="Move, Close, Pin" OnClientClose="EditLHAOnClientClose"/>  
        </Windows> 
    </telerik:RadWindowManager>
 
    <div id="div_page" runat="server" class="wider_page">   
        <hr />

        <%--Main Table--%>
        <table width="99%" border="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="LHA" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/>
                </td>
            </tr>
            <tr>
                <td align="left" valign="top">
                    <%-- Navigation Panel--%> 
                    <asp:Panel runat="server" HorizontalAlign="Left" Width="350px">
                        <table border="1" cellpadding="0" cellspacing="0" width="350px" bgcolor="White">
                            <tr>
                                <td valign="top" align="left" colspan="2" style="border-right:0">
                                    <img src="/Images/Misc/titleBarLong.png"/> 
                                    <asp:Label Text="Office/Team" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-208px;"/>
                                </td>
                               <td align="center" style="border-left:0">
                                    <asp:ImageButton id="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png"/>
                               </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <%--Left Button--%> 
                                    <asp:ImageButton runat="server" height="22" Text="Previous Quarter" ImageUrl="~\Images\Icons\dashboard_LeftGreenArrow.png" OnClick="PrevTeam"/>
                                </td> 
                                <td>
                                    <asp:DropDownList ID="dd_office" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindTeams"/>
                                    <asp:DropDownList ID="dd_team" runat="server" Width="130px" AutoPostBack="true"/>
                                </td>
                                <td align="center">
                                    <%--Right Button--%> 
                                    <asp:ImageButton runat="server" height="22" Text="Next Quarter" ImageUrl="~\Images\Icons\dashboard_RightGreenArrow.png" OnClick="NextTeam"/>
                                </td>  
                            </tr>
                        </table> 
                    </asp:Panel>
                </td>
                <td valign="top" align="right" rowspan="2">
                    <%--Log --%>       
                    <table border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                        <tr>
                            <td align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/dashboard_Log.png" height="20px" width="20px"/>
                                <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                            </td>
                        </tr>
                        <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="150" Width="910px"/></td></tr>
                    </table>
                   <%-- End Log--%>
                </td>
            </tr>
            <tr>
                <td align="left" valign="top">
                    <table ID="tbl_summary" runat="server" border="1" cellpadding="1" cellspacing="0" width="350px" bgcolor="White" style="color:Black;">
                        <tr>
                            <td align="left" colspan="4">
                                <asp:Image runat="server" ImageUrl="/Images/Misc/titleBarLong.png" style="position:relative; top:-1px; left:-1px;"/> 
                                <asp:Label Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-208px;"/>
                            </td>
                        </tr>
                        <tr>
                            <td width="30%">Total LHAs</td>
                            <td width="19%"><asp:Label runat="server" ID="lbl_s_total_lhas"/></td>
                            <td width="35%">With Due %</td>
                            <td width="16%"><asp:Label runat="server" ID="lbl_s_with_due_percent"/></td>
                        </tr>
                        <tr>
                            <td>With Due Date</td>
                            <td><asp:Label runat="server" ID="lbl_s_with_due"/></td>
                            <td>Without Due Date</td>
                            <td><asp:Label runat="server" ID="lbl_s_without_due"/></td>
                        </tr>  
                    </table>
                    <asp:Panel runat="server" BackColor="#f4a83d" style="border:1px solid #d6800c;" Width="222" Visible="false">
                        <br />
                        <table>
                            <tr><td valign="middle" align="center">
                                <asp:Label runat="server" ForeColor="#735005" Font-Bold="true" Text="Page in development" Font-Size="Large"/>
                            </td></tr>
                        </table>
                    </asp:Panel>
                    <asp:CheckBox ID="cb_include_terminated" runat="server" Text="Show Terminated Employees" ForeColor="DarkOrange" AutoPostBack="true" Checked="false"/>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:ImageButton id="imbtn_NewProspect" runat="server" Height="30" Width="27" ImageUrl="~\Images\Icons\prosReports_AddProspect.png" 
                    OnClientClick="try{ radopen(null, 'win_newlha'); }catch(E){ IE9Err(); } return false;"/>
                    <telerik:RadTabStrip ID="rts" runat="server" AutoPostBack="true" MaxDataBindDepth="1" SelectedIndex="0"
                     BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:1px;">
                        <Tabs>
                            <telerik:RadTab id="tab_lhas" runat="server" Text="Active"/>
                            <telerik:RadTab id="tab_blown" runat="server" Text="Blown"/>
                            <telerik:RadTab id="tab_approved" runat="server" Text="Approved"/>
                        </Tabs>
                    </telerik:RadTabStrip>
                    <asp:Label ID="lbl_empty" runat="server" ForeColor="DarkOrange" Visible="false" />
                    <div ID="div_gv" runat="server"/>
                </td>
            </tr>
        </table>
        <hr />
    </div>

    <asp:DropDownList ID="dd_rep_colours" runat="server" Visible="false"/>
    <asp:HiddenField ID="hf_edit_lha" runat="server"/>
    <asp:HiddenField ID="hf_new_lha" runat="server"/>

    <script type="text/javascript">
        function NewLHAOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_lha.ClientID %>").value = data;
                Refresh();
                return true;
            }
        }
        function EditLHAOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_edit_lha.ClientID %>").value = data;
                Refresh();
                return true;
            }
        }
        function Refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.disabled = false;
            button.click();
            return true;
        }
    </script>
</asp:Content>


