<%--
Author   : Joe Pickering, 15/09/2010 - re-written 07/04/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: 8-Week Report" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="8WeekReport.aspx.cs" Inherits="EightWeekReport" %>
 
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="8-Week" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:3px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:3px;"/> 
                </td>
                <td valign="top" align="right">
                    <asp:Label runat="server" ForeColor="White" Font-Size="8pt" Text="Week Commencing:" style="position:relative; top:-2px; left:3px;"/>
                    <asp:DropDownList runat="server" ID="dd_week" AutoPostBack="true" OnSelectedIndexChanged="ChangeWeek"/>
                </td>
            </tr>
        </table>
        
        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td colspan="2">
                    <asp:Panel runat="server" ID="pnl_mailto" style="display:none;">
                        <asp:Label runat="server" ID="lbl_mailto" Font-Size="Smaller" ForeColor="White"/>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <%--Tabs--%>
                    <telerik:RadTabStrip ID="tabstrip" AutoPostBack="true" MaxDataBindDepth="1" runat="server" MultiPageID="multiPage" SelectedIndex="0"
                        BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; left:4px; top:7px;">
                    </telerik:RadTabStrip>
                    <%--End Tabs--%>
                </td>
                <td align="right">
                    <asp:LinkButton ForeColor="Gray" ID="lb_recipients" Font-Size="Smaller" runat="server" 
                    Text="Show Recipients" OnClientClick="return showHide('Body_pnl_mailto', 'Body_lb_recipients')" />
                    <asp:Button runat="server" Text="Go to GSD" OnClick="GoToGSD"/> 
                    <asp:Button runat="server" ID="btn_sendreport" Text="Send" OnClick="SendReport"/> 
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table runat="server" width="982" id="tbl_gvs" style="font-family:Verdana; font-size:8pt; position:relative; top:-2px; left:1px;">
                        <tr>
                            <td valign="top" width="25%">
                                <asp:GridView ID="gv_unp_approvals" runat="server"
                                    border="1" Cellpadding="1" Width="242" CssClass="BlackGridHead"
                                    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                                    AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                                    AutoGenerateColumns="false" style="font-family:Verdana; font-size:8pt;"> 
                                    <Columns>
                                        <asp:BoundField HeaderText="CCA" ItemStyle-Width="130px" DataField="UserName"/>                          
                                        <asp:BoundField HeaderText="S" DataField="S"/>
                                        <asp:BoundField HeaderText="P" DataField="P"/>
                                        <asp:BoundField HeaderText="A" DataField="A"/>
                                        <asp:BoundField HeaderText="Apps" DataField="Approvals"/>
                                    </Columns>        
                                </asp:GridView>
                            </td>
                            <td valign="top" width="75%"><asp:TextBox ID="tb_unp_approvals" runat="server" Text="SPA Underperformance Comments" TextMode="MultiLine" Width="728" style="position:relative; top:-1px;"/></td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:GridView ID="gv_unp_tr" runat="server"
                                    border="1" Cellpadding="1" Width="242" CssClass="BlackGridHead"
                                    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                                    AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                                    AutoGenerateColumns="false" style="font-family:Verdana; font-size:8pt;"> 
                                    <Columns>
                                        <asp:BoundField HeaderText="CCA" ItemStyle-Width="130px" DataField="UserName"/>                          
                                        <asp:BoundField HeaderText="Avg. Revenue" DataField="TR"/>
                                    </Columns>             
                                </asp:GridView>
                            </td>
                            <td valign="top" align="left"><asp:TextBox ID="tb_unp_tr" runat="server" Text="Total Revenue Underperformance Comments" TextMode="MultiLine" Width="728" style="position:relative; top:-1px;"/></td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:GridView ID="gv_unp_pr_lg" runat="server"
                                    border="1" Cellpadding="1" Width="242" CssClass="BlackGridHead"
                                    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                                    AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                                    AutoGenerateColumns="false" style="font-family:Verdana; font-size:8pt;"> 
                                    <Columns>
                                        <asp:BoundField HeaderText="CCA (LG)" ItemStyle-Width="130px" DataField="UserName"/>                          
                                        <asp:BoundField HeaderText="Avg. Revenue" DataField="PR"/>
                                    </Columns>             
                                </asp:GridView>
                            </td>
                            <td valign="top" align="left"><asp:TextBox ID="tb_unp_pr_lg" runat="server" Text="List Gen Personal Revenue Underperformance Comments" TextMode="MultiLine" Width="728" style="position:relative; top:-1px;"/></td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:GridView ID="gv_unp_pr_s" runat="server"
                                    border="1" Cellpadding="1" Width="242" CssClass="BlackGridHead"
                                    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                                    AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                                    AutoGenerateColumns="false" style="font-family:Verdana; font-size:8pt;"> 
                                    <Columns>
                                        <asp:BoundField HeaderText="CCA (Sales)" ItemStyle-Width="130px" DataField="UserName"/>                          
                                        <asp:BoundField HeaderText="Avg. Revenue" DataField="PR"/>
                                    </Columns>             
                                </asp:GridView>
                            </td>
                            <td valign="top" align="left"><asp:TextBox ID="tb_unp_pr_s" runat="server" Text="Sales Personal Revenue Underperformance Comments" TextMode="MultiLine" Width="728" style="position:relative; top:-1px;"/></td>
                        </tr>
                        <tr>
                            <td valign="top" width="25%" colspan="2">Call Stats:<br />
                                <asp:TextBox runat="server" ID="tb_callstats2"/>   
                                <asp:Label runat="server" ID="lbl_tb_callstats2" ForeColor="Black" Visible="false"/>
                                <asp:Label runat="server" ID="lbl_callstats2" Text="Calls Made" ForeColor="White"/>
                                <br />
                                <asp:DropDownList runat="server" Width="64px" ID="dd_hours"/>
                                <asp:Label runat="server" ForeColor="Black" Visible="false" ID="lbl_hours"/>
                                <asp:Label runat="server" ForeColor="White" ID="lbl_hourstext" Text="Hrs"/>
                                <asp:DropDownList runat="server" Width="64px" ID="dd_minutes"/>
                                <asp:Label runat="server" ForeColor="Black" Visible="false" ID="lbl_mins"/>
                                <asp:Label runat="server" ForeColor="White" ID="lbl_minstext" Text="Mins"/>
                                <asp:Label runat="server" ID="lbl_callstats1" Text="Call Time " ForeColor="White"/>
                                <br />
                                <asp:Label runat="server" ID="lbl_callstatsinfo" Text="Note: call stats values will be saved and can be seen in the GSD." Font-Size="7" ForeColor="White"/><br />
                                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ForeColor="Red" ControlToValidate="tb_callstats2" Display="Dynamic"
                                ErrorMessage="Calls made must be a number" ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"/> 
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox runat="server" ID="tb_dectime" Width="50"/>
                                <asp:Button runat="server" OnClientClick="return makeTime()" ID="btn_convert" Text="Convert to HH/MM"/>
                                <asp:Label runat="server" ForeColor="White" Text="Convert a decimal call time to hours and minutes e.g. 3.23"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <hr />    
                </td>
            </tr>
            <tr>
                <td>
                    <%--Bar Chart--%> 
                    <telerik:RadChart ID="rc_bar" runat="server" IntelligentLabelsEnabled="false"  
                    Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="569px"/>
                    <asp:RadioButtonList ID="rbl_bartype" RepeatDirection="Horizontal" ForeColor="White" 
                    runat="server" AutoPostBack="true">
                        <asp:ListItem Selected="true">Bar</asp:ListItem>
                        <asp:ListItem>Area</asp:ListItem>
                        <asp:ListItem>Spline Area</asp:ListItem>
                        <asp:ListItem>Bubble</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
                <td>
                    <%--Line Chart--%> 
                    <telerik:RadChart ID="rc_line" runat="server" IntelligentLabelsEnabled="true"  
                    Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="408px"/>
                    <asp:RadioButtonList ID="rbl_linetype" RepeatDirection="Horizontal" ForeColor="white" runat="server" AutoPostBack="true"> 
                        <asp:ListItem Selected="true">Normal Line</asp:ListItem>
                        <asp:ListItem>Spline Line</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>   
            <tr>
                <td colspan="2" align="center">   
                    <asp:GridView ID="gv" runat="server"
                        border="1" Width="982" Cellpadding="1" CssClass="BlackGridHead"
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                        AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                        OnRowDataBound="reportgv_RowDataBound" AutoGenerateColumns="false" EnableViewState="false" style="font-family:Verdana; font-size:8pt; position:relative; top:2px;"> 
                        <Columns>
                            <asp:HyperlinkField ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" ItemStyle-BackColor="Moccasin"
                                HeaderText="Week Start" DataTextField="WeekStart" DataTextFormatString="{0:dd/MM/yyyy}" 
                                DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PRInput/PRInput.aspx?r_id={0}" 
                                DataNavigateUrlFields="r_id" ItemStyle-Width="124px"/>
                            <asp:BoundField HeaderText="List Gen Apps" DataField="ListGensApps"/>                          
                            <asp:BoundField Visible="false" HeaderText="Comms Apps" DataField="CommsApps"/>
                            <asp:BoundField HeaderText="Sales Apps" DataField="SalesApps"/>
                            <asp:BoundField HeaderText="Suspects" DataField="Suspects"/>
                            <asp:BoundField HeaderText="Prospects" DataField="Prospects"/>  
                            <asp:BoundField HeaderText="Approvals" DataField="Approvals"/>  
                            <asp:BoundField HeaderText="S:A" DataField="StoA"/>                       
                            <asp:BoundField HeaderText="P:A" DataField="PtoA"/> 
                            <asp:BoundField HeaderText="Total Revenue" DataField="TR"/>
                        </Columns>             
                    </asp:GridView>
                    <br />
                </td>
            </tr>   
            <tr>
                <td colspan="2" align="center">   
                    <asp:GridView ID="gv_sac" runat="server"
                        border="1" Width="982" CssClass="BlackGridHead" Cellpadding="1" Font-Size="7pt"
                        HeaderStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="Khaki" 
                        HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" OnRowDataBound="ccagv_RowDataBound" style="font-family:Verdana; font-size:8pt; position:relative; top:-1px;">  
                        <Columns>
                            <asp:HyperlinkField ControlStyle-ForeColor="Black" 
                            HeaderText="Sales/Comm Rep" DataTextField="CCA" ItemStyle-BackColor="Moccasin"
                            DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" 
                            datanavigateurlfields="uid" ItemStyle-Width="100px"/>
                        </Columns>
                    </asp:GridView>
                    <br />
                    <asp:GridView ID="gv_lg" runat="server" 
                        border="1" Width="982" Cellpadding="1" Font-Size="7pt" CssClass="BlackGridHead"
                        HeaderStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="Khaki" 
                        HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" OnRowDataBound="ccagv_RowDataBound" style="font-family:Verdana; font-size:8pt;">             
                        <Columns>
                            <asp:HyperlinkField ControlStyle-ForeColor="Black"
                            HeaderText="List Gen" DataTextField="CCA" ItemStyle-BackColor="Moccasin"
                            DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" 
                            datanavigateurlfields="uid" ItemStyle-Width="100px"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr> 
            <tr>
                <td colspan="2">
                    <asp:Label runat="server" ForeColor="White" Text="Comments&nbsp;"/>
                    <asp:LinkButton runat="server" ForeColor="Gray" Font-Size="Smaller" Text="(Add Bullets)" OnClientClick="return addBullets('Body_tb_comments')"/> 
                    <asp:TextBox ID="tb_comments" runat="server" TextMode="MultiLine" Height="130" Width="976" />
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
    
    <script type="text/javascript">
        function showHide(id, lb) {
            obj = grab(id);
            lb = grab(lb);
            if (obj.style.display == "none") { obj.style.display = "block"; lb.innerText = 'Hide Recipients'; }
            else { obj.style.display = "none"; lb.innerText = 'Show Recipients'; }
            return false;
        }
        function addBullets(id) {
            obj = grab(id);
            obj.value += '\n•\n\n•\n\n•';
            return false;
        }
        function checkDecimal(str) {
            if (!str) return 0;
            var ok = "";
            for (var i = 0; i < str.length; i++) 
            {
                var ch = str.substring(i, i+1);
                if ((ch < "0" || "9" < ch) && ch != '.') 
                {
                    alert("Only numeric input is allowed!\n\n" 
                    + parseFloat(ok) + " will be used because '" 
                    + str + "' is invalid.\nYou may correct "
                    + "this entry and try again.");
                    return parseFloat(ok);
                }
                else ok += ch;
            }
            return str;
        }
        function makeTime() {
            num = (checkDecimal(grab("<%= tb_dectime.ClientID %>").value)); // validates input
            if (num) {
                grab("<%= dd_hours.ClientID %>").value = parseInt(num);
                num -= parseInt(num); num *= 60;
                grab("<%= dd_minutes.ClientID %>").value = parseInt(num);
                num -= parseInt(num);
            }
            return false;
        }
    </script>
</asp:Content>

