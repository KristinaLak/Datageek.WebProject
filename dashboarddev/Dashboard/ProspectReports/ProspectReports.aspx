<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 05/05/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Prospect Reports" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="ProspectReports.aspx.cs" Inherits="ProspectReports" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="/UserControls/OfficeToggler.ascx" TagName="OfficeToggler" TagPrefix="cc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<asp:UpdateProgress runat="server">
    <ProgressTemplate>
        <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
    </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_pros" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
      <Triggers>
        <asp:PostBackTrigger ControlID="imbtn_exportToExcel"/>
    </Triggers>
    <ContentTemplate>
    <telerik:RadToolTipManager ID="rttm" runat="server" ShowDelay="450" OffsetY="-18" EnableViewState="false" Overlay="false"
    ManualClose="true" RelativeTo="Mouse" Sticky="true" Skin="Silk" ShowCallout="false"
    Animation="None" ShowEvent="OnMouseOver" AutoTooltipify="false" RenderInPageRoot="true" OnAjaxUpdate="ShowContactList"/>

    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="true" ShowContentDuringLoad="false">
        <Windows>
            <telerik:RadWindow ID="win_movetodue" runat="server" Title="&nbsp;Move to Due" Behaviors="Close, Move, Pin" OnClientClose="ProsDueOnClientClose"/>
            <telerik:RadWindow ID="win_movetold" runat="server" Title="&nbsp;Approve Prospect" Behaviors="Close, Move, Pin" OnClientClose="MoveToLDOnClientClose"/>
            <telerik:RadWindow ID="win_moveprospect" runat="server" Title="&nbsp;Move Prospect" Behaviors="Move, Close, Pin" OnClientClose="MoveToTeamOnClientClose"/>
            <telerik:RadWindow ID="win_editpros" runat="server"  Title="&nbsp;Edit Prospect" MaxHeight="900" OnClientClose="EditProsOnClientClose" Behaviors="Move, Close, Pin"/> 
        </Windows>
    </telerik:RadWindowManager> 
    <div id="div_page" runat="server" class="wider_page">
        <hr />
            
            <table width="99%" style="position:relative; left:7px; top:-2px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Prospect" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Reports" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    </td>
                </tr>
            </table>
            
             <%--MAIN TABLE--%>          
            <table width="99%" style="margin-left:auto; margin-right:auto;"> 
                <tr>
                    <td valign="top" width="32%">
                        <%-- Navigation Panel--%> 
                        <cc:OfficeToggler ID="ot" runat="server" Top="-2" Left="1"/>
                        <asp:Panel ID="pnl_NavPanel" runat="server" HorizontalAlign="Left" Width="400px">
                            <table border="1" cellpadding="0" cellspacing="0" width="392px" bgcolor="White">
                                <tr>
                                    <td valign="top" colspan="2" style="border-right:0">
                                        <img src="/images/misc/titlebarlong.png"/> 
                                        <img src="/images/icons/admin_admins.png" height="20px" width="20px"/>
                                        <asp:Label Text="Office/Team" ForeColor="White" runat="server" style="position:relative; left:-232px; top:-6px;"/>
                                    </td>
                                    <td align="center" valign="middle" style="border-left:0">
                                        <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\images\icons\dashboard_refresh.png" OnClick="BindProspects"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="center">
                                        <%--Left Button--%> 
                                        <asp:ImageButton ID="imbtn_leftNavButton" height="22" ImageUrl="~\Images\Icons\dashboard_leftgreenarrow.png"  runat="server" Text="Previous Team" OnClick="PrevTeam"/> 
                                    </td>
                                    <td>
                                        <%--Area--%> 
                                        <asp:DropDownList ID="dd_office" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="ChangeOffice"/>
                                        <%--Team Dropdownlist--%> 
                                        <asp:DropDownList ID="dd_team" Enabled="false" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindProspects"> 
                                            <asp:ListItem></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td align="center">
                                        <%--Right Button--%> 
                                        <asp:ImageButton ID="imbtn_rightNavButton" height="22" ImageUrl="~\Images\Icons\dashboard_RightGreenArrow.png"  runat="server" Text="Next Team" OnClick="NextTeam"/> 
                                    </td>  
                                </tr>
                            </table>
                        </asp:Panel>
                        <br />
                        <%--Summary--%>
                        <table ID="tbl_summary" runat="server" border="1" cellpadding="1" cellspacing="0" width="392px" bgcolor="White" style="color:Black">
                            <tr>
                                <td align="left" colspan="4">
                                    <asp:Image ID="img_summarybar" runat="server" ImageUrl="/images/misc/titleBarLong.png" style="position:relative; top:-1px; left:-1px;"/> 
                                    <asp:Label ID="lbl_infoText" Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-210px;"/>
                                </td>
                            </tr>
                            <tr>
                                <td width="25%">Prospects</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoProspects" ForeColor="Black"></asp:Label></td>
                                <td width="25%">Reps</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoReps" ForeColor="Black"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>P1</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoP1"/></td>
                                <td>P2</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoP2"/></td>
                            </tr>  
                            <tr>
                                <td>Due this Week</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoDueThisWeek"/></td>
                                <td>Due Today</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoDueToday" ForeColor="Orange"/></td>
                            </tr>
                            <tr>
                                <td>LH Due this Wk</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoLHDueThisWeek"/></td>
                                <td>LH Due Today</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoLHDueToday" ForeColor="Orange"/></td>
                            </tr>
                            <tr>
                                <td>Overdue</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoOverdue" ForeColor="Red"/></td>
                                <td>Without Emails</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoNoEmails" ForeColor="Orange"/></td>
                            </tr> 
                            <tr>
                                <td>Waiting</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoWaiting" ForeColor="Orange"></asp:Label></td>
                                <td>Due</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoDue" ForeColor="LightGreen"></asp:Label></td>
                            </tr> 
                            <tr>
                                <td>Blown/P3</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoBlown" ForeColor="Red"></asp:Label></td>
                                <td>In</td>
                                <td><asp:Label runat="server" ID="lbl_SummaryNoIn" ForeColor="LimeGreen"></asp:Label></td>
                            </tr> 
                        </table>
                        <%--End Summary--%>
                    </td>
                    <td align="right">
                        <%--Console Table--%>    
                        <asp:Label ID="lbl_norwich_toggle_buffer" Text="&nbsp;" runat="server" Visible="false" />   
                        <table ID="tbl_log" runat="server" border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                            <tr>
                                <td align="left">
                                    <img src="/images/misc/titleBarAlpha.png"/> 
                                    <img src="/images/icons/dashboard_Log.png" height="20px" width="20px"/>
                                    <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                                </td>
                            </tr>
                            <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="177" Width="861px"/></td></tr>
                        </table>
                       <%-- End Console Table--%>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <%--Headers--%>
                        <table style="position:relative; top:11px; left:-4px;">
                            <tr>
                                <td>
                                     <div ID="div_commonheader" runat="server" visible="true">                                                                         
                                        <asp:ImageButton ID="imbtn_printGridView" runat="server" Height="26" Width="22" ImageUrl="~\Images\Icons\salesBook_PrinterFriendlyVersion.png" OnClick="PrintGridView"/> 
                                        <asp:ImageButton ID="imbtn_exportToExcel" runat="server" Height="24" Width="23" ImageUrl="~\Images\Icons\salesBook_ExportToExcel.png" OnClick="ExportToExcel"/> 
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <%--End Headers--%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <telerik:RadTabStrip ID="tabstrip" AutoPostBack="true" MaxDataBindDepth="1" runat="server" MultiPageID="multiPage" SelectedIndex="0"
                                     BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:4px;">
                                        <Tabs>
                                            <telerik:RadTab ID="tab_due" runat="server" Text="Prospects"/>
                                            <telerik:RadTab ID="tab_p3" runat="server" Text="P3s"/>
                                            <telerik:RadTab ID="tab_blown" runat="server" Text="Blown"/>
                                            <telerik:RadTab ID="tab_listin" runat="server" Text="Approvals"/>
                                        </Tabs>
                                    </telerik:RadTabStrip>
                                </td>
                                <td valign="bottom">             
                                    <table cellpadding="0" cellspacing="0">
                                        <tr>
                                            <td><asp:Label ID="lbl_within" runat="server" Text="&nbsp;Within:" ForeColor="DarkOrange" style="position:relative; top:3px;"/></td>
                                            <td>
                                                <asp:DropDownList  ID="dd_appcriteria" runat="server" Width="110" 
                                                AutoPostBack="true" OnSelectedIndexChanged="WithinRangeChanging" Visible="false" Height="22" style="position:relative; top:2px; left:6px;">
                                                    <asp:ListItem Text="last 3 months" Value="-3"/>
                                                    <asp:ListItem Text="last month" Value="-1"/>
                                                    <asp:ListItem Text="last 2 months" Value="-2"/>
                                                    <asp:ListItem Text="last 6 months" Value="-6"/>
                                                    <asp:ListItem Text="a year" Value="-12"/>
                                                    <asp:ListItem Text="all time" Value="-999"/>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td align="right">
                        <asp:Panel runat="server" DefaultButton="btn_search_company" style="position:relative; left:-1px; top:3px;">
                            <asp:Label ID="lbl_search" runat="server" ForeColor="DarkOrange" Text="Search for a company:" EnableViewState="false"/>
                            <asp:TextBox ID="tb_search_company" runat="server"/>
                            <asp:Button ID="btn_search_company" runat="server" OnClick="LoadCompanySearch" Text="Search" EnableViewState="false"/> 
                            <asp:Button ID="btn_reset_search" runat="server" OnClick="EndCompanySearch" Text="End Search" Visible="false"/>
                            <ajax:TextBoxWatermarkExtender runat="server" TargetControlID="tb_search_company" WatermarkText="Enter a company name" WatermarkCssClass="watermark"/>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">  
                        <%--Tab Pages--%>
                        <telerik:RadMultiPage ID="tabstrip_multipage" runat="server" SelectedIndex="0" Width="376px" RenderSelectedPageOnly="true">
                            <%--Prospects--%> 
                            <telerik:RadPageView runat="server"> 
                                <div ID="div_duegridarea" runat="server"/>
                            </telerik:RadPageView>
                            <%--P3s--%> 
                            <telerik:RadPageView runat="server"> 
                                <div ID="div_p3gridarea" runat="server"/>
                            </telerik:RadPageView>
                            <%--Blown--%> 
                            <telerik:RadPageView runat="server">
                                <div ID="div_blowngridarea" runat="server"/>
                            </telerik:RadPageView>
                            <%--Approvals--%> 
                            <telerik:RadPageView runat="server">
                                <div ID="div_ingridarea" runat="server"/>
                            </telerik:RadPageView>
                        </telerik:RadMultiPage>
                        <%--End Tab Pages--%>
                    </td>
                </tr>
            </table>
        <hr/>
    </div>
    
    <div style="display:none;"> 
        <asp:Button ID="btn_confirmWorking" runat="server" OnClick="UpdateWaiting"/>
        <asp:Button ID="btn_moveToListDist" runat="server" OnClick="UpdateIn"/>
        <asp:TextBox ID="tb_dateDueTempBox" runat="server" Text=""/>
        <asp:TextBox ID="tb_idOfClicked" runat="server" Text=""/>
        <asp:TextBox ID="tb_suppliers" runat="server" Text=""/>
        <asp:TextBox ID="tb_status" runat="server" Text=""/>
        <asp:HiddenField ID="hf_new_pros" runat="server" />
        <asp:HiddenField ID="hf_edit_pros" runat="server" />
    </div> 
    
    <script type="text/javascript">
        function showHide(id) {
            obj = grab(id);
            if (obj.style.display == "none") { obj.style.display = "block"; }
            else { obj.style.display = "none"; }
            return false;
        }
        function opendue(chkbx) {
            var idOfClicked = grab("<%= tb_idOfClicked.ClientID %>");
            idOfClicked.value = chkbx.name;
            var oWnd = radopen("ProspectMoveToDue.aspx", "win_movetodue");
        }
        function openld(chkbx) {
            var idOfClicked = grab("<%= tb_idOfClicked.ClientID %>");
            idOfClicked.value = chkbx.name;
            var oWnd = radopen("ProspectMoveToListDist.aspx", "win_movetold");
        }
        function ProsDueOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                var dateDue = grab("<%= tb_dateDueTempBox.ClientID %>");
                dateDue.value = data;
                var button = grab("<%= btn_confirmWorking.ClientID %>");
                button.click();
            }
        }
        function MoveToLDOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                var suppliers = grab("<%= tb_suppliers.ClientID %>");
                var status = grab("<%= tb_status.ClientID %>");

                suppliers.value = data.suppliers;
                status.value = data.status;

                var button = grab("<%= btn_moveToListDist.ClientID %>");
                button.click();
            }
        }
        function MoveToTeamOnClientClose(sender, args) {
            grab("<%= imbtn_refresh.ClientID %>").click();
        }
        function NewProsOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_pros.ClientID %>").value = data;
                grab("<%= imbtn_refresh.ClientID %>").click();
                return;
            }
        }
        function EditProsOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_edit_pros.ClientID %>").value = data;
            }
            if (sender.rebind == true) {
                grab("<%= imbtn_refresh.ClientID %>").click();
            }
            return;
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>