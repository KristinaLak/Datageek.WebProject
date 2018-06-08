<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 13/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Account Management" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="RoleManagement.aspx.cs" Inherits="RolesManagement" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .contents {
        	background:darkgray;
        }
        .contents:hover {
        	background:#bbbbbb;
        }
        .description {
        	border-radius:3px;
        	background:lightgray;
        	padding-left:5px;
        	padding-right:5px;
        	padding-top:2px;
        	color:Maroon;
        	font-size:10pt;
        }
        .title {
        	background:gray;
        	font-size:8pt;
        	color:DarkOrange;
        	padding:3px;
        	border-radius:2px;
        	border-top:solid 1px darkgray;
        	border-bottom:solid 1px lightgray;
        	font-family:Verdana;
        	font-style:italic;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        <table width="99%" style="font-family:Verdana; color:White; margin-left:auto; margin-right:auto;" cellpadding="0" cellspacing="0">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:CheckBox ID="cb_employed_only" runat="server" Checked="true" Text="Show Employed Users Only" AutoPostBack="true" OnCheckedChanged="BindOfficeUsers" style="float:right;"/>
                    <asp:Label runat="server" Text="Permission" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px; left:2px;"/> 
                    <asp:Label runat="server" Text="Management" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px; left:2px;"/> 
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td align="left" valign="top" bgcolor="DarkGray" width="200">
                    <table>
                        <tr>
                            <td><asp:Label runat="server" Text="Office:" ForeColor="White" Font-Size="8pt"/></td>
                            <td><asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" Width="100" OnSelectedIndexChanged="BindOfficeUsers"/></td>
                            <td><asp:Label runat="server" Text="User:" ForeColor="White" Font-Size="8pt"/></td>
                            <td><asp:DropDownList ID="dd_user" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindUserRoles" Width="120"/></td>
                            <td><asp:Label ID="lbl_template" runat="server" Text="Template:" ForeColor="White" Font-Size="8pt" Visible="false"/></td>
                            <td>
                                <asp:DropDownList ID="dd_templates" runat="server" Width="100" AutoPostBack="true" OnSelectedIndexChanged="LoadTemplate" Visible="false">
                                    <asp:ListItem Value="db_Custom">Custom</asp:ListItem>
                                    <asp:ListItem Value="db_Admin">Admin</asp:ListItem>
                                    <asp:ListItem Value="db_HoS">HoS</asp:ListItem>
                                    <asp:ListItem Value="db_TeamLeader">Team Leader</asp:ListItem>
                                    <asp:ListItem Value="db_Finance">Finance</asp:ListItem>
                                    <asp:ListItem Value="db_GroupUser">Group User</asp:ListItem>
                                    <asp:ListItem Value="db_User">User</asp:ListItem>
                                    <asp:ListItem Value="db_CCA">CCA</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="right">
                    <asp:Button runat="server" Text="Undo Unsaved Changes" OnClick="UndoChanges" style="position:relative; left:8px;"/>
                    <asp:Button ID="btn_back" runat="server" Text="Back to Account Management" Width="200" OnClick="BackToAccountManagement" style="position:relative; left:5px;"/>
                    <asp:Button ID="btn_save" runat="server" Text="Save Permissions" OnClientClick="return confirm('Are you sure you wish to save this user\'s permissions?\n\nThese changes will be applied instantly, the user simply needs to refresh their browser.');" 
                    OnClick="SaveRoles" Enabled="false" style="position:relative; left:1px;"/>
                </td>
            </tr>
        </table>
   
        <table id="tbl_main" width="99%" border="0" runat="server" cellpadding="0" cellspacing="0"  
            style="font-family:Verdana; color:White; font-size:8pt; margin-left:auto; margin-right:auto;">
            
            <tr>
                <td bgcolor="DarkGray" valign="top" style="border-left:solid 2px gray;">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr>
                            <td width="25%">
                                <table>
                                    <tr>
                                        <td width="110" class="title">Management Lvl.</td>
                                        <td style="border:solid 1px gray;" valign="middle">
                                            <asp:RadioButtonList runat="server" Enabled="false" CellPadding="0" CellSpacing="0" Width="250" Height="30" ID="rbl_adminhos" RepeatDirection="Horizontal">
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="">None</asp:ListItem>
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_HoS">HoS</asp:ListItem>
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_Admin">Admin</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="110" class="title">Misc.</td>
                                        <td style="border:solid 1px gray;" valign="middle">
                                             <asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Designer" ID="cb_designer" runat="server" Text="Designer"/>   
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="left" valign="top" width="62%" height="65" bgcolor="DarkGray" style="border-left:solid 2px gray;" class="description">
                     <asp:Label ID="lbl_roledescription" runat="server" Text="Hover over a checkbox to see a description of the permission here."/>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Sales Book</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBook" ID="cb_sb" runat="server" Text="Sales Book"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" Tooltip="db_SalesBookTL" ID="cb_sbtl" runat="server" Text="Sales Book TL"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookAdd" ID="cb_sbadd" runat="server" Text="Sales Book Add"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookEdit" ID="cb_sbedit" runat="server" Text="Sales Book Edit"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookDelete" ID="cb_sbdelete" runat="server" Text="Sales Book Delete"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookEmail" ID="cb_sbemail" runat="server" Text="Sales Book Approval E-mail"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookMove" ID="cb_sbmove" runat="server" Text="Sales Book Move"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SalesBookOutput" ID="cb_sboutput" runat="server" Text="Sales Book Output"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_BudgetSheet" ID="cb_budgetsheet" runat="server" Text="Budget Sheet"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)"  Tooltip="db_StatusSummary" ID="cb_statussummary" runat="server" Text="Status Summary"/></td>
                            <td colspan="2"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)"  Tooltip="db_SalesBookNoBookLock" ID="cb_salesbooknobooklock" runat="server" Text="Sales Book No Book Lock"/></td></tr>
                        <tr>    
                            <td colspan="4" style="border-left:solid 2px gray;" id="4">
                                <table>
                                    <tr>
                                        <td class="title">Sales Book Role&nbsp;</td>
                                        <td>
                                            <asp:RadioButtonList runat="server" CellPadding="0" CellSpacing="0"  Width="260" ID="rbl_officeadmindesign" RepeatDirection="Horizontal">
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="">Standard</asp:ListItem>
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_SalesBookOfficeAdmin">Office Admin</asp:ListItem>
                                                <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_SalesBookDesign">Design</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr runat="server" visible="false"><td colspan="2" class="title">&nbsp;Collections</td></tr>
            <tr runat="server" visible="false">
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Collections" ID="cb_collections" runat="server" Text="Collections"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_CollectionsTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_collectionstl" runat="server" Text="Collections TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Editorial Tracker</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_EditorialTracker" ID="cb_editorialtracker" runat="server" Text="Editorial Tracker"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_EditorialTrackerTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_editorialtrackertl" runat="server" Text="Editorial Tracker TL"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_EditorialTrackerEdit" ID="cb_editorialtrackeredit" runat="server" Text="Editorial Tracker Edit"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SmartSocialEdit" ID="cb_smartsocialedit" runat="server" Text="SMARTsocial Edit"/></td>
                        </tr>
                        <tr><td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SmartSocialAnalytics" ID="cb_smartsocialanalytics" runat="server" Text="SMARTsocial Analytics"/></td>
                            <td width="25%" colspan="3"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_EditorialTrackerProjectManager" ID="cb_editorialtrackerprojectmanager" runat="server" Text="Project Manager"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>

                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Finance</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSales" ID="cb_financesales" runat="server" Text="Finance Sales"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_financesalestl" runat="server" Text="Finance Sales TL"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesEdit" ID="cb_financesalesedit" runat="server" Text="Finance Sales Edit"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesExport" ID="cb_financesalesexport" runat="server" Text="Finance Sales Export"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesDS" ID="cb_financesalesds" runat="server" Text="Finance Daily Summary" Visible="false"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesGS" ID="cb_financesalesgs" runat="server" Text="Finance Group Summary" Visible="false"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesLiab" ID="cb_financesalesliab" runat="server" Text="Finance Liabilities" Visible="false"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_FinanceSalesTab" ID="cb_financesalestab" runat="server" Text="Finance Tab" Visible="false"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Media Sales</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_MediaSales" ID="cb_mediasales" runat="server" Text="Media Sales"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_MediaSalesTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_mediasalestl" runat="server" Text="Media Sales TL"/></td>
                            <td colspan="2"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_MediaSalesEdit" ID="cb_mediasalesedit" runat="server" Text="Media Sales Edit"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Progress Report</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReport" ID="cb_pr" runat="server" Text="Progress Report"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReportTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_prtl" runat="server" Text="Progress Report TL"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReportEdit" ID="cb_predit" runat="server" Text="Progress Report Edit"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReportSummary" ID="cb_prsummary" runat="server" Text="Progress Report Group Summary"/></td>
                        </tr>
                        <tr><td width="25%" colspan="4"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_8-WeekSummary" ID="cb_8weeksummary" runat="server" Text="8-Week Summary"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Progress Report Output</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReportOutput" ID="cb_proutput" runat="server" Text="Progress Report Output"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProgressReportOutputTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_proutputtl" runat="server" Text="Progress Report Output TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;List Distribution</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistribution" ID="cb_ld" runat="server" Text="List Distribution"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistributionTL" OnCheckedChanged="ToggleTLShow" AutoPostBack="true" ID="cb_ldtl" runat="server" Text="List Distribution TL"/></td>
                            <td width="25%"><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" AutoPostBack="true" OnCheckedChanged="ToggleTEL" Tooltip="db_ListDistributionTEL" ID="cb_ldtel" runat="server" Text="List Distribution TEL"/></td>
                            <td colspan="2"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistributionEdit" ID="cb_ldedit" runat="server" Text="List Distribution Edit"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistributionMove" ID="cb_ldmove" runat="server" Text="List Distribution Move"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistributionDelete" ID="cb_lddelete" runat="server" Text="List Distribution Delete"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ListDistributionAdd" ID="cb_ldadd" runat="server" Text="List Distribution Add"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Prospect Reports</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReports" ID="cb_pros" runat="server" Text="Prospect Reports"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_prostl" runat="server" Text="Prospect Reports TL"/></td>
                            <td width="25%"><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" AutoPostBack="true" OnCheckedChanged="ToggleTEL" Tooltip="db_ProspectReportsTEL" ID="cb_prostel" runat="server" Text="Prospect Reports TEL"/></td>
                            <td colspan="2"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsEdit" ID="cb_prosedit" runat="server" Text="Prospect Reports Edit"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsEmail" ID="cb_prosemail" runat="server" Text="Prospect Reports Approval E-mail"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsDelete" ID="cb_prosdelete" runat="server" Text="Prospect Reports Delete"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsAdd" ID="cb_prosadd" runat="server" Text="Prospect Reports Add"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ProspectReportsHoS" ID="cb_proshos" runat="server" Text="Prospect Reports HoS"/></td>
                        </tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Three-Month Planner</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Three-MonthPlanner" ID="cb_3mp" runat="server" Text="Three-Month Planner"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Three-MonthPlannerTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_3mptl" runat="server" Text="Three-Month Planner TL"/></td>
                            <td width="25%"><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" OnCheckedChanged="ToggleTLShow" AutoPostBack="true" Tooltip="db_Three-MonthPlannerUL" ID="cb_3mpul" runat="server" Text="Three-Month Planner UL"/></td>
                            <td><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Three-MonthPlannerSummary" ID="cb_3mpsummary" runat="server" Text="Three-Month Planner Summary"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;LHA Report</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_LHAReport" ID="cb_lhareport" runat="server" Text="LHA Report"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_LHAReportTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_lhareporttl" runat="server" Text="LHA Report TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Leads</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Leads" ID="cb_leads" runat="server" Text="Leads"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_LeadsAnalytics" ID="cb_leadsanalytics" runat="server" Text="Leads Analytics"/></td>
                            <td>&nbsp;</td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Home Hub</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_HomeHub" ID="cb_homehub" runat="server" Text="Home Hub"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_HomeHubTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_homehubtl" runat="server" Text="Home Hub TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Training</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TrainingPres" ID="cb_trainingpres" runat="server" Text="Training Presentations"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TrainingDocs" ID="cb_trainingdocs" runat="server" Text="Training Documents"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TrainingUpload" ID="cb_trainingupload" runat="server" Text="Training Upload"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TrainingAdmin" ID="cb_trainingadmin" runat="server" Text="Training Admin"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;" colspan="3"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TrainingVideos" ID="cb_trainingvideos" runat="server" Text="Training Videos"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br/></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;8-Week Report</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_8-WeekReport" ID="cb_8weekreport" runat="server" Text="8-Week Report"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_8-WeekReportTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_8weekreporttl" runat="server" Text="8-Week Report TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;RSG</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" mouseout="res()" onmouseover="dscptn(this)" Tooltip="db_RSG" ID="cb_rsg" runat="server" Text="RSG"/></td>
                            <td><asp:CheckBox onclick="cg(this)" mouseout="res()" onmouseover="dscptn(this)" Tooltip="db_RSGEmail" ID="cb_rsgemail" runat="server" Text="RSG Weekly E-mail"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Report Generator</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ReportGenerator" ID="cb_reportgenerator" runat="server" Text="Report Generator"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_ReportGeneratorTL" AutoPostBack="true" OnCheckedChanged="ToggleTLShow" ID="cb_reportgeneratortl" runat="server" Text="Report Generator TL"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Commission Forms</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_CommissionForms" ID="cb_commissionforms" runat="server" Text="Commission Forms"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this);" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_CommissionFormsTL" OnCheckedChanged="ToggleTLShow" AutoPostBack="true" ID="cb_commissionformstl" runat="server" Text="Commission Forms TL"/></td>
                            <td colspan="2"><asp:CheckBox onmouseout="res()" onmouseover="dscptn(this)" AutoPostBack="true" OnCheckedChanged="ToggleTEL" Tooltip="db_CommissionFormsTEL" ID="cb_commissionformstel" runat="server" Text="Commission Forms TEL"/></td></tr>
                        <tr>
                            <td colspan="4">
                                <asp:Label ID="lbl_cf_view_level" runat="server" Text="&nbsp;View Level" BackColor="Gray"/>
                                <asp:RadioButtonList ID="rbl_cflevel" runat="server" CellPadding="0" CellSpacing="0" Width="290" RepeatDirection="Horizontal" style="border-left:solid 2px gray;">
                                    <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_CommissionFormsL0">CCA.</asp:ListItem>
                                    <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_CommissionFormsL1">Standard.</asp:ListItem>
                                    <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_CommissionFormsL2">HoS.</asp:ListItem>
                                    <asp:ListItem onmouseout="res()" onmouseover="dscptn(this)" Value="db_CommissionFormsL3">Admin/Finance.</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr><td style="border-left:solid 2px gray;"><br /></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Other Reports</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_DataHubReport" ID="cb_datahubreport" runat="server" Text="DataHub Report"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_DSR" ID="cb_dsr" runat="server" Text="DSR"/></td>
                            <td width="25%">
                                <asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_GPR" ID="cb_gpr" runat="server" Text="GPR"/>
                                <%--<asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_GSD" ID="cb_gsd" runat="server" Text="GSD"/>--%>
                            </td>
                            <td>
                                <asp:CheckBox onclick="cg(this)" mouseout="res()" onmouseover="dscptn(this)" Tooltip="db_QuarterlyReport" ID="cb_quarterlyreport" runat="server" Text="Quarterly Report"/>
                                <%--<asp:CheckBox onclick="cg(this)" mouseout="res()" onmouseover="dscptn(this)" Tooltip="db_PerformanceReport" ID="cb_performancereport" runat="server" Text="Performance Report"/>--%>
                                <%--<asp:CheckBox onclick="cg(this)" mouseout="res()" onmouseover="dscptn(this)" Tooltip="db_CCAPerformance" ID="cb_ccaperformance" runat="server" Text="CCA Performance"/>--%>
                            </td></tr>
                        <tr><td  width="25%" style="border-left:solid 2px gray;" colspan="4"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_MWD" ID="cb_mwd" runat="server" Text="MWD"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br/></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="title">&nbsp;Other Pages</td></tr>
            <tr>
                <td class="contents" colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr><td width="25%" style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Teams" ID="cb_teams" runat="server" Text="Teams"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Cam" ID="cb_cam" runat="server" Text="Cam"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Logs" ID="cb_logs" runat="server" Text="Logs"/></td>
                            <td width="25%"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_DataHubAccess" ID="cb_dha" runat="server" Text="DataHub Access"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_TerritoryManager" ID="cb_termgr" runat="server" Text="Territory Manager"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_CashReport" ID="cb_cashreport" runat="server" Text="Cash Report"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_SoA" ID="cb_soa" runat="server" Text="Statement of Achievement"/></td>
                            <td><asp:CheckBox onclick="cg(this)" onmouseout="res()" onmouseover="dscptn(this)" Tooltip="db_Search" ID="cb_search" runat="server" Text="Search"/></td></tr>
                        <tr><td style="border-left:solid 2px gray;"><br/></td></tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="top" colspan="4" height="30" bgcolor="DarkGray" style="border-top:solid 2px gray;" class="description">
                     <asp:Label ID="lbl_roledescription2" runat="server" Text="Hover over a checkbox to see a description of the permission here."/>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
            
    <script type="text/javascript" language="javascript">
        function dscptn(item) {
            var lbl = grab("<%= lbl_roledescription.ClientID%>");
            var lbl2 = grab("<%= lbl_roledescription2.ClientID%>");
            var txt;
            
            if (navigator.userAgent.indexOf("Firefox") != -1) {
                txt = item.children[1].innerHTML;
            }
            else {
                txt = item.innerText;
            }
            var d = '';
            var tl = 'If checked, this page will be territory locked. This user will not be able to view any data outside of their territory on this page unless other territory access is specified.';
            var tel = 'If checked, this page will be team locked. This user will not be able to view any data outside of their CCA team on this page.';
            var ul = 'If checked, this page will be user locked. This user will not be able to view any data other than their own.';
            switch (txt) 
            {
                case 'Designer':
                    d = 'Whether this user is flagged as a Designer, this role will provide certain useful functions for Designers.'; break;
                case 'None':
                    d = 'Standard user with no special Dashboard privileges.'; break;
                case 'HoS':
                    d = 'Various elevated Dashboard privileges. Allows user to manage teams and users within their territory via the Account Management page, but cannot tailor user permissions.' +
                    ' '; break;
                case 'Admin':
                    d = 'Highest Dashboard privileges. Can view most pages, data and has access to most functionality. Can modify all teams and users\' permissions.'; break;
                    
                case 'Sales Book':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Sales Book TL':
                    d = tl; break;
                case 'Sales Book Add':
                    d = 'Whether this user can add sales, red-lines and book issues to the Sales Book.'; break;
                case 'Sales Book Edit':
                    d = 'Whether this user can edit any sales, red-lines or book information.'; break;
                case 'Sales Book Delete':
                    d = 'Whether this user can delete or cancel sales and books.'; break;
                case 'Sales Book Approval E-mail':
                    d = 'Whether this user will receive artwork receipt notifications for their office via e-mail.'; break;
                case 'Sales Book Move':
                    d = 'Whether this user can move sales or features between books.'; break;
                case 'Sales Book Output':
                    d = 'Access to ' + txt + ' page.'; break;
                case 'Sales Book No Book Lock':
                    d = 'Whether this user is able to edit sales in locked books (edit/cancel/restore/delete/move etc).'; break;
                case 'Budget Sheet':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Status Summary':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Standard':
                    d = 'Standard Sales Book view, allows user to view both Standard and Design tabs.'; break;
                case 'Office Admin':
                    d = 'Design Sales Book view (Ad List). Allows user to view both Standard and Design tabs. Cannot delete/cancel sales. Design status for sales cannot be set. i.e. Waiting for Copy, Copy In, Proof Out etc.'; break;
                case 'Design':
                    d = 'Same as Office Admin, but user is permitted to set sale design status.'; break;
                    
                case 'Collections':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Collections TL':
                    d = tl; break;
                    
                case 'Editorial Tracker':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Editorial Tracker TL':
                    d = tl; break;
                case 'Editorial Tracker Edit':
                    d = 'Ability to Edit/Add/Delete/Cancel features on the Editorial Tracker page.'; break;
                case 'SMARTsocial Edit':
                    d = 'Ability to create and modify SMARTsocial pages for customers (through the Editorial Tracker page).'; break;
                case 'SMARTsocial Analytics':
                    d = 'Ability to see analytics for SMARTsocial pages and customer views.'; break;
                case 'Project Manager':
                    d = 'Allows user to be set as the Project Manager for Editorial Features and also allows the user to set the assigned Designer of Editorial Features.'; break;

                case 'Finance Sales':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Finance Sales Edit':
                    d = 'Whether this user can edit any sale information on the finance page.'; break;
                case 'Finance Sales Export':
                    d = 'Whether this user can export finance data to Excel from the finance page.'; break;
                case 'Finance Sales TL':
                    d = tl; break;
                case 'Finance Daily Summary':
                    d = 'Whether this user can generate/view daily finance summaries.'; break;
                case 'Finance Group Summary':
                    d = 'Whether this user can generate/receive weekly e-mails for the finance group summary.'; break;
                case 'Finance Liabilities':
                    d = 'Whether this user can view/manage liabilities on the finance page.'; break;
                case 'Finance Tab':
                    d = 'Whether this user can add custom tabs to the finance page.'; break;
                    
                case 'Media Sales':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Media Sales TL':
                    d = tl; break;
                case 'Media Sales Edit':
                    d = 'Whether this user can edit any Media Sales information or blow/approve sales.'; break;    
                    
                case 'Progress Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Progress Report TL':
                    d = tl; break;
                case 'Progress Report Output':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Progress Report Output TL':
                    d = tl; break;
                case 'Progress Report Edit':
                    d = 'Whether this user can update the Progress Report(s).'; break;
                case 'Progress Report Group Summary':
                    d = 'Access to the ' + txt + ' page.'; break;
                case '8-Week Summary':
                    d = 'Access to the ' + txt + ' page.'; break;
                    
                case 'List Distribution':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'List Distribution TL':
                    d = tl; break;
                case 'List Distribution Delete':
                    d = 'Whether this user can delete or cancel lists.'; break;
                case 'List Distribution Edit':
                    d = 'Whether this user can edit any list or issue information.'; break;
                case 'List Distribution Move':
                    d = 'Whether this user can move lists between issues or assign lists to users.'; break;
                case 'List Distribution TEL':
                    d = tel; break;
                case 'List Distribution Add':
                    d = 'Whether this user can add List Distribution issues and lists.'; break;
                    
                case 'Prospect Reports':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Prospect Reports TL':
                    d = tl; break;
                case 'Prospect Reports Delete':
                    d = 'Whether this user can delete prospects.'; break;
                case 'Prospect Reports Edit':
                    d = 'Whether this user can edit any prospect information.'; break;
                case 'Prospect Reports Approval E-mail':
                    d = 'Whether this user will receive prospect approval notifications for their territory via e-mail.'; break;
                case 'Prospect Reports TEL':
                    d = tel; break;
                case 'Prospect Reports Add':
                    d = 'Whether this user can add prospects.'; break;
                case 'Prospect Reports HoS':
                    d = 'Whether this user can act as a HoS (to see all Prospects) for the Prospect Report for their team(s).'; break;

                case 'Three-Month Planner':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Three-Month Planner TL':
                    d = tl; break;
                case 'Three-Month Planner UL':
                    d = ul; d += ' Checking this will also check Three-Month Planner TL.'; break;
                case 'Three-Month Planner Summary':
                    d = 'Access to the ' + txt + ' page.'; break;
                    
                case 'LHA Report':
                    d = 'Access to the ' + txt + ' page (Leads-Hooks-Angles).'; break;
                case 'LHA Report TL':
                    d = tl; break;

                case 'Leads':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Leads Analytics':
                    d = 'Access to the ' + txt + ' page.'; break;
                    
                case 'Home Hub':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Home Hub TL':
                    d = tl; break;

                case 'Training Presentations':
                    d = 'Access to the Training Presentation documents.'; break;
                case 'Training Videos':
                    d = 'Access to the Training Videos page, where videos can be played.'; break;
                case 'Training Documents':
                    d = 'Access to the BizClik ToolBox Training documents.'; break;
                case 'Training Upload':
                    d = 'Whether this user can upload files to the Presentations/BizClik Toolbox.'; break;
                case 'Training Admin':
                    d = 'Whether this user is a training admin. Admins can assign permissions to other users through the training Administration page.'; break;

                case '8-Week Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case '8-Week Report TL':
                    d = tl; break;

                case 'RSG':
                    d = 'Access to the ' + txt + ' page (Ready-Set-Go).'; break;
                case 'RSG Weekly E-mail':
                    d = 'Whether this user will receive weekly RSG group reports via e-mail.'; break;

                case 'Report Generator':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Report Generator TL':
                    d = tl; break;
                    
                case 'Commission Forms':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Commission Forms TL':
                    d = tl; break;
                case 'Commission Forms TEL':
                    d = tel; d += ' Checking this will also check Commission Forms TL.'; break;
                case 'CCA.':
                    d = 'Allows a CCA to see their commission form only.'; break;
                case 'Standard.':
                    d = 'Allows user to see commission for all call centre staff that are not HoS or Admins.'; break;
                case 'HoS.':
                    d = 'Allows user to see commission for all call centre staff that are not Admins.'; break;
                case 'Admin/Finance.':
                    d = 'Allows user to see commission for all call centre staff including any HoS and Admins.'; break;
                          
                case 'Logs':
                    d = 'Access to the Dashboard ' + txt + ' page.'; break;
                case 'DataHub Access':
                    d = 'Access to ' + txt + ' page.'; break;
                case 'Teams':
                    d = 'Access to the Teams page'; break;
                case 'Territory Manager':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Cam':
                    d = 'Access to the ' + txt + ' page.'; break;

                case 'DataHub Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'GSD':
                    d = 'Access to the ' + txt + ' page (Group Sales Director)'; break;
                case 'GPR':
                    d = 'Access to the ' + txt + ' page (Group Performance Report).'; break;
                case 'DSR':
                    d = 'Access to the ' + txt + ' page (Daily Sales Report).'; break;
                case 'MWD':
                    d = 'Access to the ' + txt + ' page (My Working Day Report).'; break;
                case 'Performance Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Quarterly Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'User List':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'Cash Report':
                    d = 'Access to the ' + txt + ' page.'; break;
                case 'CCA Performance':
                    d = 'Access to the ' + txt + ' pages.'; break;
                case 'Statement of Achievement':
                    d = 'Access to the ' + txt + ' pages.'; break;
                case 'Widget Generator':
                    d = 'Access to the ' + txt + ' page.'; break;
                      
                default: d = 'No description available.';
            }
            if (navigator.userAgent.indexOf("Firefox") != -1)
            {
                lbl.innerHTML = item.innerHTML + ': ' + d;
                lbl2.innerHTML = item.innerHTML + ': ' + d;
            }
            else 
            {
                lbl.innerText = item.innerText + ': ' + d;
                lbl2.innerText = item.innerText + ': ' + d;
            }
        }
        function res() {
            grab("<%= lbl_roledescription.ClientID%>").innerHTML =
            grab("<%= lbl_roledescription2.ClientID%>").innerHTML =
            'Hover over a checkbox to see a description of the permission here.';
            return;
        }
        function cg(cb) {
            var c;
            if (cb.checked) {
                c = 'Green';
            }
            else { c = 'Red'; }
            cb.style.backgroundColor = c;
            cb.nextSibling.style.backgroundColor = c;
        }
    </script> 
</asp:Content>
