<%@ Control Language="C#" AutoEventWireup="true" CodeFile="db_header - Copy.ascx.cs" Inherits="Header" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<ajax:AnimationExtender runat="server" TargetControlID="lbl_welcome">
  <Animations>
    <OnHoverOver>
      <FadeIn AnimationTarget="lbl_quote" Duration="6" Fps="20"/>
    </OnHoverOver>
    <OnHoverOut>
        <ScriptAction Script="grab('Header_lbl_quote').style.opacity='0'"/>
    </OnHoverOut>
  </Animations>
</ajax:AnimationExtender>

<div ID="div_header" runat="server" class="header">
    <asp:HyperLink runat="server" NavigateUrl="~" style="position:absolute; top:2px; left:4px;">
        <img src="/Images/Misc/header_banner.png" alt="BizClik Media Dashboard" onmouseout="src='/Images/Misc/header_banner.png';" onmouseover="src='/Images/Misc/header_banner_invert.png';"/>
    </asp:HyperLink>

    <div ID="div_welcome" runat="server">
        <asp:Label ID="lbl_quote" runat="server" ForeColor="White" Font-Names="Verdana" Font-Size="7pt" style="position:absolute; top:0; right:0; margin:2px 3px 0px 0px; opacity:0;"/>
        <asp:Label ID="lbl_welcome" runat="server" ForeColor="White" Font-Names="Verdana" Font-Size="7pt" 
         style="position:absolute; bottom:0; right:0; margin:0px 2px 2px 0px; cursor:pointer; cursor:hand;"/>
    </div>
     
    <div ID="div_christmas" runat="server" visible="false" style="position:absolute; bottom:0; right:0; margin:0px 2px 2px 0px;">
        <asp:ImageButton ID="imbtn_christmas_lights" runat="server" Visible="false" Height="19" Width="19" ImageUrl="~/Images/Icons/christmas_lights.png" OnClick="ToggleChristmasLights" 
        style="position:relative; top:5px; left:2px; opacity:0.2; filter:alpha(opacity=20);"/>
        <asp:DropDownList ID="dd_snow_setting" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangeSnow" Font-Size="7" Width="70" BackColor="#cef7de">
            <asp:ListItem Text="Snow On"/>
            <asp:ListItem Text="Snow Off" Selected="True"/>
        </asp:DropDownList>
        <asp:DropDownList ID="dd_snow_count" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangeSnow" Font-Size="7" Width="50" BackColor="#fbddde">
            <asp:ListItem Text="10" Value="10"/>
            <asp:ListItem Text="20" Value="20"/>
            <asp:ListItem Text="30" Value="30"/>
            <asp:ListItem Text="50" Value="50"/>
            <asp:ListItem Text="100" Value="100" Selected="True"/>
            <asp:ListItem Text="200" Value="200"/>
            <asp:ListItem Text="300" Value="300"/>
            <asp:ListItem Text="400" Value="400"/>
            <asp:ListItem Text="500" Value="500"/>
            <asp:ListItem Text="750" Value="750"/>
            <asp:ListItem Text="1000" Value="1000"/>
        </asp:DropDownList>
    </div>
</div>

<div ID="div_rm" runat="server" style="margin-left:auto; margin-right:auto;">
    <telerik:RadMenu ID="rm" runat="server" EnableShadows="true" CausesValidation="false" style="z-index:100;" ExpandDelay="50" OnItemClick="LogInOut" Skin="Glow" ShowToggleHandle="true" Width="100%" CssClass="DashboardMainMenu">
        <Items>
            <%--Home Hub--%>
            <telerik:RadMenuItem Tooltip="A group overview illustrating overall performance." Text="Home Hub" Value="HomeHub" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" Visible="false"/>
            <%--Break--%>
            <telerik:RadMenuItem Width="8" Enabled="False" Text="&nbsp;"/>
            <%--Sales Book Menu--%>
            <telerik:RadMenuItem Text="Sales Book" NavigateUrl="~/Dashboard/SBInput/SBInput.aspx" Value="SalesBook" Tooltip="View and manage profile sales.">
                <Items>
                    <telerik:RadMenuItem Tooltip="View and manage profile sales." Text="Sales Book" Value="SalesBook" NavigateUrl="~/Dashboard/SBInput/SBInput.aspx"/> 
                    <telerik:RadMenuItem Tooltip="View sale data and sale trends over selected time periods." Text="Sales Book Output" Value="SalesBookOutput" NavigateUrl="~/Dashboard/SBOutput/SBOutput.aspx"/>
                    <telerik:RadMenuItem Tooltip="See a breakdown of advertiser list status by book." Text="Status Summary" Value="StatusSummary" NavigateUrl="~/Dashboard/StatusSummary/StatusSummary.aspx"/>
                    <telerik:RadMenuItem Tooltip="Set up annual Sales Book targets and break-even values." Text="Budget Sheet" Value="BudgetSheet" NavigateUrl="~/Dashboard/BackBudget/BackBudget.aspx"/>
                    <telerik:RadMenuItem Tooltip="View annual Sales Book revenue and completion figures for the group." Text="Cash Report" Value="CashReport" NavigateUrl="~/Dashboard/CashReport/CashReport.aspx"/>
                </Items>
            </telerik:RadMenuItem>
            <%--Progress Report Menu--%>
            <telerik:RadMenuItem Text="Progress Report" Value="ProgressReport" NavigateUrl="~/Dashboard/PRInput/PRInput.aspx" Tooltip="Maintain weekly CCA performance reports based on SPAs and revenue earned.">
                <Items>
                    <telerik:RadMenuItem Tooltip="Maintain weekly CCA performance reports based on SPAs and revenue earned." Text="Progress Report" Value="ProgressReport" NavigateUrl="~/Dashboard/PRInput/PRInput.aspx"/>
                    <telerik:RadMenuItem Tooltip="View CCA and office performance data over selected time periods." Text="Progress Report Output" Value="ProgressReportOutput" NavigateUrl="~/Dashboard/PROutput/PRTerritoryOutput.aspx"/>
                    <telerik:RadMenuItem Tooltip="View weekly input/conversion summaries for the group." Text="Progress Report Group Summary" Value="ProgressReportSummary" NavigateUrl="~/Dashboard/PRGroupSummary/PRGroupSummary.aspx"/>
                    <telerik:RadMenuItem Tooltip="View CCA 8-week summaries for the group." Text="8-Week Summary" Value="8-WeekSummary" NavigateUrl="~/Dashboard/8WeekSummary/8WeekSummary.aspx"/>
                    <%--<telerik:RadMenuItem Tooltip="View individual CCA performance statistics." Text="CCA Performance" Value="CCAPerformance" NavigateUrl="~/Dashboard/CCAP/CCAPerformance.aspx"/>--%>
                </Items>
            </telerik:RadMenuItem>
            <%--List Dist --%>
            <telerik:RadMenuItem Tooltip="View and manage the distribution of lists." Text="List Distribution" Value="ListDistribution" NavigateUrl="~/Dashboard/ListDistribution/ListDistribution.aspx"/>
            <%--Prospect Reports--%>
            <telerik:RadMenuItem Tooltip="View and manage prospects." Text="Prospect Reports" Value="ProspectReports" NavigateUrl="~/Dashboard/ProspectReports/ProspectReports.aspx"/>
            <%--Editorial Tracker--%>
            <telerik:RadMenuItem Tooltip="View and manage editorial data." Text="Editorial Tracker" Value="EditorialTracker" NavigateUrl="~/Dashboard/EditorialTracker/EditorialTracker.aspx"/>
            <%--Commission Forms Menu--%>
            <telerik:RadMenuItem Tooltip="View, edit, print or e-mail CCA commission forms for a selected calendar month and view annual overviews for each territory." 
                Text="Commission" Value="CommissionForms" NavigateUrl="~/Dashboard/CommissionForms/CommissionForms.aspx"/>
            <%--Media Sales--%>
            <telerik:RadMenuItem Tooltip="View and manage media sales." Text="Media Sales" Value="MediaSales" NavigateUrl="~/Dashboard/MediaSales/MediaSales.aspx"/> 
            <%--Finance--%>
            <telerik:RadMenuItem Tooltip="View and manage financial data for sales." Text="Finance" Value="FinanceSales" NavigateUrl="~/Dashboard/Finance/Finance.aspx"/> 
            <%--Leads--%>
            <telerik:RadMenuItem Tooltip="Leads manager for CCAs." Text="Leads" Value="Leads" NavigateUrl="~/Dashboard/Leads/Leads.aspx"/> 
            <%--Tools Menu--%>
            <telerik:RadMenuItem Text="Tools" Tooltip="Dashboard tools -- Expand for more." NavigateUrl="~/Dashboard/AccountManagement/AccountManagement.aspx">
                <Items>
                    <telerik:RadMenuItem Tooltip="Change or retrieve your password. View recent user logins and manage users, their teams and their permissions." 
                    Text="Account Management" Value="" NavigateUrl="~/Dashboard/AccountManagement/AccountManagement.aspx"/>
                    <telerik:RadMenuItem Tooltip="Expand for more." Text="Administrative Tools" NavigateUrl="~/Dashboard/Cam/Cam.aspx">
                        <Items>
                            <telerik:RadMenuItem Tooltip="Links to real-time camera feeds." Text="Cam" Value="Cam" NavigateUrl="~/Dashboard/Cam/Cam.aspx"/>
                            <telerik:RadMenuItem Tooltip="View Dashboard activity logs." Text="Log Viewer" Value="Logs" NavigateUrl="~/Dashboard/LogViewer/LogViewer.aspx"/>
                            <%--<telerik:RadMenuItem Tooltip="View and modify mailing lists for various Dashboard mailers." Text="Mail Lists" Value="" NavigateUrl="~/Dashboard/MailLists/MailLists.aspx"/>--%>
                            <telerik:RadMenuItem Tooltip="View and modify mailing addresses for various Dashboard mailers." Text="Mail Addresses" Value="Admin" NavigateUrl="~/Dashboard/MailAddresses/MailAddresses.aspx"/>
                            <telerik:RadMenuItem Tooltip="Manage Dashboard territories." Text="Office Manager" Value="TerritoryManager" NavigateUrl="~/Dashboard/OfficeManager/OfficeManager.aspx"/>
                        </Items>
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem Tooltip="Search, modify and add data to the DataHub database." Text="DataHub Access" Value="DataHubAccess" NavigateUrl="~/Dashboard/DHAccess/DHAccess.aspx"/>
                    <telerik:RadMenuItem Tooltip="A feedback survey to send to customers." Text="Customer Feedback Survey" Value="" NavigateUrl="~/FeedbackSurvey.aspx" Target="_blank"/>
                    <telerik:RadMenuItem Tooltip="View territory and group league tables." Text="League Tables" Value="" NavigateUrl="~/Dashboard/League/League.aspx"/>
                    <telerik:RadMenuItem Tooltip="View and manage CCA LHAs." Text="LHA Report" Value="LHAReport" NavigateUrl="~/Dashboard/LHAReport/LHAReport.aspx"/>
                    <telerik:RadMenuItem Text="Reports" NavigateUrl="~/Dashboard/8WeekReport/8WeekReport.aspx" Tooltip="Expand for more.">
                        <Items>
                            <telerik:RadMenuItem Tooltip="Generate and distribute an 8-week report for selected territories." Text="8-Week Report" Value="8-WeekReport" NavigateUrl="~/Dashboard/8WeekReport/8WeekReport.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate and distribute an end-of-day Daily Sales Report for your territory." Text="Daily Sales Report" Value="DSR" NavigateUrl="~/Dashboard/DSR/DSR.aspx"/>
                            <telerik:RadMenuItem Tooltip="View history for the DataHub database including total companies, contacts and e-mails over time." Text="DataHub Report" Value="DataHubReport" NavigateUrl="~/Dashboard/DataHubReport/DataHubReport.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate group daily performance figures and append them to the GPR spreadsheet." Text="Group Performance Report" Value="GPR" NavigateUrl="~/Dashboard/GPR/GPR.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate and distribute a start-of-day Daily Sales Predictions report for your territory." Text="My Working Day" Value="MWD" NavigateUrl="~/Dashboard/MWD/MWD.aspx"/>
                            <telerik:RadMenuItem Tooltip="A group quarterly report summarising CCA performance via revenue leaderboards." Text="Quarterly Report" Value="QuarterlyReport" NavigateUrl="~/Dashboard/QuarterlyReport/QuarterlyReport.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate reports on selected sale data over selected time periods." Text="Report Generator" Value="ReportGenerator" NavigateUrl="~/Dashboard/ReportGenerator/ReportGenerator.aspx"/>
                            <telerik:RadMenuItem Tooltip="RSG for heads of sales." Text="RSG" Value="RSG" NavigateUrl="~/Dashboard/RSG/RSG.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate annual statements of achievement for CCAs" Text="Statement of Achievement" Value="SOA" NavigateUrl="~/Dashboard/SoA/SoA.aspx"/>
                            <telerik:RadMenuItem Tooltip="Generate and distribute a weekly group summary report." Text="Weekly Group Summary" Value="Admin" NavigateUrl="~/Dashboard/WeeklyGroupSummary/WeeklyGroupSummary.aspx"/>
                        </Items>
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem Tooltip="Search and export company lists from the Dashboard database." Text="Search" Value="Search" NavigateUrl="~/Dashboard/Search/Search.aspx"/>
                    <telerik:RadMenuItem Tooltip="Edit and test Dashboard mailer signature files." Text="Signature Test" Value="" NavigateUrl="~/Dashboard/SigTest/SignatureTest.aspx"/>
                    <telerik:RadMenuItem Tooltip="View Regional/Channel Mag teams." Text="Teams" Value="Teams" NavigateUrl="~/Dashboard/Teams/Teams.aspx"/>
                    <telerik:RadMenuItem Tooltip="Three-Month Planner for CCAs." Text="Three-Month Planner" Value="Three-MonthPlanner" NavigateUrl="~/Dashboard/3MonthPlanner/3MonthPlanner.aspx">
                        <Items>
                            <telerik:RadMenuItem Tooltip="Three-Month Planner for CCAs." Text="Three-Month Planner" Value="Three-MonthPlanner" NavigateUrl="~/Dashboard/3MonthPlanner/3MonthPlanner.aspx"/>
                            <telerik:RadMenuItem Tooltip="Grouped Three-Month Planner summaries." Text="Three-Month Planner Summary" Value="Three-MonthPlannerSummary" NavigateUrl="~/Dashboard/3MonthPlannerSummary/3MPSummary.aspx"/>
                        </Items>
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem Tooltip="Training resources." Text="Training" Value="" NavigateUrl="~/Dashboard/Training/ToolBox/WDMToolBox.aspx">
                        <Items>
                            <telerik:RadMenuItem ToolTip="Manage access to training pages and upload private files." Value="" Text="Administration" NavigateUrl="~/Dashboard/Training/Admin/TrainingAdmin.aspx"/>
                            <telerik:RadMenuItem ToolTip="View/download presentation files." Text="Presentations" Value="" NavigateUrl="~/Dashboard/Training/Presentations/TrainingPresentations.aspx"/>
                            <telerik:RadMenuItem ToolTip="View/download training files." Text="BizClik Toolbox" Value="" NavigateUrl="~/Dashboard/Training/ToolBox/WDMToolBox.aspx"/>
                        </Items>
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem Tooltip="View/edit the Dashboard user list." Text="User List" Value="" NavigateUrl="~/Dashboard/UserList/UserList.aspx"/>
                    <%--<telerik:RadMenuItem Tooltip="Generate/download/upload widget files." Text="Widget Generator" Value="WidgetGenerator" NavigateUrl="~/Dashboard/WidgetGenerator/WidgetGenerator.aspx"/>--%>
                </Items>
            </telerik:RadMenuItem>
            <%--Log In/Out--%>
            <telerik:RadMenuItem ID="rmi_login" runat="server" Text="Log In" Value="LogIn" ImageUrl="../images/icons/lock.png"/>
        </Items>
    </telerik:RadMenu>
</div>

<script type="text/javascript">
    function HideQuote() {
        grab('Header_lbl_quote').style.opacity = "0";
    }
</script>