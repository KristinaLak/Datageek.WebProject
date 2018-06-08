<%@ Control Language="C#" AutoEventWireup="true" CodeFile="db_header.ascx.cs" Inherits="Header" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<ajax:AnimationExtender runat="server" TargetControlID="lbl_welcome">
  <Animations>
    <OnHoverOver>
      <FadeIn AnimationTarget="lbl_quote" Duration="4" Fps="20"/>
    </OnHoverOver>
    <OnHoverOut>
        <ScriptAction Script="grab('Header_lbl_quote').style.opacity='0'"/>
    </OnHoverOut>
  </Animations>
</ajax:AnimationExtender>

<asp:HiddenField ID="hf_user_id" runat="server"/>

<div ID="div_header" runat="server" class="Header">
    <asp:HyperLink runat="server" NavigateUrl="~">
        <img src="/images/misc/datageek_logo.png" alt="DataGeek" class="DataGeekLogo"/>
    </asp:HyperLink>

    <div ID="div_search" runat="server" class="HeaderSearchContainer" visible="false">
        <telerik:RadComboBox ID="rcb_search" runat="server" Enabled="false" EnableLoadOnDemand="True" OnItemsRequested="rcb_search_ItemsRequested" AutoPostBack="false" 
            HighlightTemplatedItems="true" Width="300" DropDownWidth="500px" AutoCompleteType="Disabled" Skin="Bootstrap" ItemRequestTimeout="350"
            EmptyMessage="Search for Companies and Contacts by name or phone.." CausesValidation="false" DropDownAutoWidth="Enabled" ChangeTextOnKeyBoardNavigation="false" 
            ShowWhileLoading="true" OnClientDropDownOpening="rcb_Rebind">
        </telerik:RadComboBox>
        <asp:HiddenField ID="hf_lead_view_id" runat="server"/>
        <div style="display:none;"><asp:Button ID="btn_view_in_project" runat="server" OnClick="ViewLeadInProject"/></div>
    </div>

    <div ID="div_welcome" runat="server">
        <asp:Label ID="lbl_quote" runat="server" CssClass="HeaderQuote"/>
        <asp:Label ID="lbl_welcome" runat="server" CssClass="HeaderWelcome"/>
    </div>
     
    <div ID="div_christmas" runat="server" visible="false" class="ChristmasContainer">
        <asp:ImageButton ID="imbtn_christmas_lights" runat="server" Visible="false" Height="19" Width="19" ImageUrl="~/images/icons/christmas_lights.png" OnClick="ToggleChristmasLights" 
        style="position:relative; top:6px; left:2px; opacity:0.2; filter:alpha(opacity=20);"/>
        <asp:DropDownList ID="dd_snow_setting" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangeSnow" Font-Size="7" Width="70" BackColor="#cef7de" style="padding:3px; border-radius:3px;">
            <asp:ListItem Text="Snow On"/>
            <asp:ListItem Text="Snow Off" Selected="True"/>
        </asp:DropDownList>
        <asp:DropDownList ID="dd_snow_count" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangeSnow" Font-Size="7" Width="55" BackColor="#fbddde" style="padding:3px; border-radius:3px;">
            <asp:ListItem Text="x10" Value="10"/>
            <asp:ListItem Text="x20" Value="20"/>
            <asp:ListItem Text="x30" Value="30"/>
            <asp:ListItem Text="x50" Value="50"/>
            <asp:ListItem Text="x100" Value="100" Selected="True"/>
            <asp:ListItem Text="x200" Value="200"/>
            <asp:ListItem Text="x300" Value="300"/>
            <asp:ListItem Text="x400" Value="400"/>
            <asp:ListItem Text="x500" Value="500"/>
            <asp:ListItem Text="x750" Value="750"/>
            <asp:ListItem Text="x1000" Value="1000"/>
        </asp:DropDownList>
        <asp:Literal ID="js_snowstorm" runat="server"/>
    </div>

    <div ID="div_rm" runat="server" class="MainMenuContainer">
        <telerik:RadMenu ID="rm" runat="server" EnableShadows="true" CausesValidation="false" ExpandDelay="50" OnItemClick="LogInOut" 
                Skin="Glow" ShowToggleHandle="true" Width="100%" CssClass="DashboardMainMenu">
            <Items>
                <%--Home Hub--%>
                <telerik:RadMenuItem Tooltip="A group overview illustrating overall performance." Text="Home Hub" Value="HomeHub" NavigateUrl="~/dashboard/homehub/homehub.aspx" Visible="false"/>
                <%--Break--%>
                <telerik:RadMenuItem Width="8" Enabled="False" Text="&nbsp;"/>
                <%--Sales Book Menu--%>
                <telerik:RadMenuItem Text="Sales Book" NavigateUrl="~/dashboard/sbinput/sbinput.aspx" Value="SalesBook" Tooltip="View and manage profile sales.">
                    <Items>
                        <telerik:RadMenuItem Tooltip="View and manage profile sales." Text="Sales Book" Value="SalesBook" NavigateUrl="~/dashboard/sbinput/sbinput.aspx"/> 
                        <telerik:RadMenuItem Tooltip="View sale data and sale trends over selected time periods." Text="Sales Book Output" Value="SalesBookOutput" NavigateUrl="~/dashboard/sboutput/sboutput.aspx"/>
                        <telerik:RadMenuItem Tooltip="See a breakdown of advertiser list status by book." Text="Status Summary" Value="StatusSummary" NavigateUrl="~/dashboard/statussummary/statussummary.aspx"/>
                        <telerik:RadMenuItem Tooltip="Set up annual Sales Book targets and break-even values." Text="Budget Sheet" Value="BudgetSheet" NavigateUrl="~/dashboard/backbudget/backbudget.aspx"/>
                        <telerik:RadMenuItem Tooltip="View annual Sales Book revenue and completion figures for the group." Text="Cash Report" Value="CashReport" NavigateUrl="~/dashboard/cashreport/cashreport.aspx"/>
                    </Items>
                </telerik:RadMenuItem>
                <%--Progress Report Menu--%>
                <telerik:RadMenuItem Text="Progress Report" Value="ProgressReport" NavigateUrl="~/dashboard/prinput/prinput.aspx" Tooltip="Maintain weekly CCA performance reports based on SPAs and revenue earned.">
                    <Items>
                        <telerik:RadMenuItem Tooltip="Maintain weekly CCA performance reports based on SPAs and revenue earned." Text="Progress Report" Value="ProgressReport" NavigateUrl="~/dashboard/prinput/prinput.aspx"/>
                        <telerik:RadMenuItem Tooltip="View CCA and office performance data over selected time periods." Text="Progress Report Output" Value="ProgressReportOutput" NavigateUrl="~/dashboard/proutput/prterritoryoutput.aspx"/>
                        <telerik:RadMenuItem Tooltip="View weekly input/conversion summaries for the group." Text="Progress Report Group Summary" Value="ProgressReportSummary" NavigateUrl="~/dashboard/prgroupsummary/prgroupsummary.aspx"/>
                        <telerik:RadMenuItem Tooltip="View CCA 8-week summaries for the group." Text="8-Week Summary" Value="8-WeekSummary" NavigateUrl="~/dashboard/8weeksummary/8weeksummary.aspx"/>
                    </Items>
                </telerik:RadMenuItem>
                <%--List Dist --%>
                <telerik:RadMenuItem Tooltip="View and manage the distribution of lists." Text="List Distribution" Value="ListDistribution" NavigateUrl="~/dashboard/listdistribution/listdistribution.aspx"/>
                <%--Prospect Reports--%>
                <telerik:RadMenuItem Tooltip="View and manage prospects." Text="Prospect Reports" Value="ProspectReports" NavigateUrl="~/dashboard/prospectreports/prospectreports.aspx"/>
                <%--Editorial Tracker--%>
                <telerik:RadMenuItem Tooltip="View and manage editorial data." Text="Editorial" Value="EditorialTracker" NavigateUrl="~/dashboard/editorialtracker/editorialtracker.aspx">
                    <Items>
                        <telerik:RadMenuItem Tooltip="View and manage editorial data." Text="Editorial Tracker" Value="EditorialTracker" NavigateUrl="~/dashboard/editorialtracker/editorialtracker.aspx"/>
                        <telerik:RadMenuItem Tooltip="View and manage information relating to SMARTsocial profiles." Text="SMARTsocial Tracker" Value="EditorialTracker" NavigateUrl="~/dashboard/smartsocial/tracker.aspx"/>
                        <telerik:RadMenuItem Tooltip="View stats for SMARTsocial profiles." Text="SMARTsocial Analytics" Value="SmartSocialAnalytics" NavigateUrl="~/dashboard/smartsocial/analytics.aspx"/>
                    </Items>
                </telerik:RadMenuItem>
                <%--Commission Forms Menu--%>
                <telerik:RadMenuItem Tooltip="View, edit, print or e-mail CCA commission forms for a selected calendar month and view annual overviews for each territory." 
                    Text="Commission" Value="CommissionForms" NavigateUrl="~/dashboard/commissionforms/commissionforms.aspx"/>
                <%--Media Sales--%>
                <telerik:RadMenuItem Tooltip="View and manage media sales." Text="Media Sales" Value="MediaSales" NavigateUrl="~/dashboard/mediasales/mediasales.aspx"/> 
                <%--Finance--%>
                <telerik:RadMenuItem Tooltip="View and manage financial data for sales." Text="Finance" Value="FinanceSales" NavigateUrl="~/dashboard/finance/finance.aspx"/> 
                <%--Leads--%>
                <telerik:RadMenuItem Tooltip="Leads manager for CCAs." Text="Leads" Value="Leads" NavigateUrl="~/dashboard/leads/leads.aspx"/> 
                <%--Tools Menu--%>
                <telerik:RadMenuItem Text="Tools" Tooltip="Dashboard tools -- Expand for more." NavigateUrl="~/dashboard/accountmanagement/accountmanagement.aspx">
                    <Items>
                        <telerik:RadMenuItem Tooltip="Change or retrieve your password. View recent user logins and manage users, their teams and their permissions." 
                        Text="Account Management" Value="" NavigateUrl="~/dashboard/accountmanagement/accountmanagement.aspx"/>
                        <telerik:RadMenuItem Tooltip="Expand for more." Text="Administrative Tools" NavigateUrl="~/dashboard/logviewer/logviewer.aspx">
                            <Items>
                                <telerik:RadMenuItem Tooltip="View Dashboard activity logs." Text="Log Viewer" Value="Logs" NavigateUrl="~/dashboard/logviewer/logviewer.aspx"/>
                                <telerik:RadMenuItem Tooltip="View and modify mailing addresses for various Dashboard mailers." Text="Mail Addresses" Value="Admin" NavigateUrl="~/dashboard/mailaddresses/mailaddresses.aspx"/>
                                <telerik:RadMenuItem Tooltip="Manage Dashboard territories." Text="Office Manager" Value="TerritoryManager" NavigateUrl="~/dashboard/officemanager/officemanager.aspx"/>
                            </Items>
                        </telerik:RadMenuItem>
                        <telerik:RadMenuItem Tooltip="Search, modify and add data to the DataHub database." Text="DataHub Access" Value="DataHubAccess" NavigateUrl="~/dashboard/dhaccess/dhaccess.aspx" Visible="false"/>
                        <telerik:RadMenuItem Tooltip="A feedback survey to send to customers." Text="Customer Feedback Survey" Value="" NavigateUrl="~/feedbacksurvey.aspx" Target="_blank"/>
                        <telerik:RadMenuItem Tooltip="View territory and group league tables." Text="League Tables" Value="" NavigateUrl="~/dashboard/league/league.aspx"/>
                        <telerik:RadMenuItem Tooltip="View and manage CCA LHAs." Text="LHA Report" Value="LHAReport" NavigateUrl="~/dashboard/lhareport/lhareport.aspx"/>
                        <telerik:RadMenuItem Text="Reports" NavigateUrl="~/dashboard/8weekreport/8weekreport.aspx" Tooltip="Expand for more.">
                            <Items>
                                <telerik:RadMenuItem Tooltip="Generate and distribute an 8-week report for selected territories." Text="8-Week Report" Value="8-WeekReport" NavigateUrl="~/dashboard/8weekreport/8weekreport.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate and distribute an end-of-day Daily Sales Report for your territory." Text="Daily Sales Report" Value="DSR" NavigateUrl="~/dashboard/dsr/dsr.aspx"/>
                                <telerik:RadMenuItem Tooltip="View history for the DataHub database including total companies, contacts and e-mails over time." Text="DataHub Report" Value="DataHubReport" NavigateUrl="~/dashboard/datahubreport/datahubreport.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate group daily performance figures and append them to the GPR spreadsheet." Text="Group Performance Report" Value="GPR" NavigateUrl="~/dashboard/gpr/gpr.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate and distribute a start-of-day Daily Sales Predictions report for your territory." Text="My Working Day" Value="MWD" NavigateUrl="~/dashboard/mwd/mwd.aspx"/>
                                <telerik:RadMenuItem Tooltip="A group quarterly report summarising CCA performance via revenue leaderboards." Text="Quarterly Report" Value="QuarterlyReport" NavigateUrl="~/dashboard/quarterlyreport/quarterlyreport.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate reports on selected sale data over selected time periods." Text="Report Generator" Value="ReportGenerator" NavigateUrl="~/dashboard/reportgenerator/reportgenerator.aspx"/>
                                <telerik:RadMenuItem Tooltip="RSG for heads of sales." Text="RSG" Value="RSG" NavigateUrl="~/dashboard/rsg/rsg.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate annual statements of achievement for CCAs" Text="Statement of Achievement" Value="SOA" NavigateUrl="~/dashboard/soa/soa.aspx"/>
                                <telerik:RadMenuItem Tooltip="Generate and distribute a weekly group summary report." Text="Weekly Group Summary" Value="Admin" NavigateUrl="~/dashboard/weeklygroupsummary/weeklygroupsummary.aspx"/>
                            </Items>
                        </telerik:RadMenuItem>
                        <telerik:RadMenuItem Tooltip="Search and export company lists from the Dashboard database." Text="Search" Value="Search" NavigateUrl="~/dashboard/search/search.aspx"/>
                        <telerik:RadMenuItem Tooltip="Edit and test Dashboard mailer signature files." Text="Signature Test" Value="" NavigateUrl="~/dashboard/sigtest/signaturetest.aspx"/>
                        <telerik:RadMenuItem Tooltip="View Regional/Channel Mag teams." Text="Teams" Value="Teams" NavigateUrl="~/dashboard/teams/teams.aspx"/>
                        <telerik:RadMenuItem Tooltip="Three-Month Planner for CCAs." Text="Three-Month Planner" Value="Three-MonthPlanner" NavigateUrl="~/dashboard/3monthplanner/3monthplanner.aspx">
                            <Items>
                                <telerik:RadMenuItem Tooltip="Three-Month Planner for CCAs." Text="Three-Month Planner" Value="Three-MonthPlanner" NavigateUrl="~/dashboard/3monthplanner/3monthplanner.aspx"/>
                                <telerik:RadMenuItem Tooltip="Grouped Three-Month Planner summaries." Text="Three-Month Planner Summary" Value="Three-MonthPlannerSummary" NavigateUrl="~/dashboard/3monthplannersummary/3mpsummary.aspx"/>
                            </Items>
                        </telerik:RadMenuItem>
                        <telerik:RadMenuItem Tooltip="Training resources." Text="Training" Value="" NavigateUrl="~/dashboard/training/toolbox/wdmtoolbox.aspx">
                            <Items>
                                <telerik:RadMenuItem ToolTip="Manage access to training pages and upload private files." Value="" Text="Administration" NavigateUrl="~/dashboard/training/admin/trainingadmin.aspx"/>
                                <telerik:RadMenuItem ToolTip="View/download training files." Text="BizClik Toolbox" Value="" NavigateUrl="~/dashboard/training/toolbox/wdmtoolbox.aspx"/>
                                <telerik:RadMenuItem ToolTip="View/download presentation files." Text="Presentations" Value="" NavigateUrl="~/dashboard/training/presentations/trainingpresentations.aspx"/>
                                <telerik:RadMenuItem ToolTip="View training video files." Text="Videos" Value="" NavigateUrl="~/dashboard/training/videos/videoviewer.aspx"/>
                            </Items>
                        </telerik:RadMenuItem>
                        <telerik:RadMenuItem Tooltip="View/edit the Dashboard user list." Text="User List" Value="" NavigateUrl="~/dashboard/userlist/userlist.aspx"/>
                    </Items>
                </telerik:RadMenuItem>
                <%--Log In/Out--%>
                <telerik:RadMenuItem ID="rmi_login" runat="server" Text="Log In" Value="LogIn" ImageUrl="../images/icons/lock.png"/>
            </Items>
        </telerik:RadMenu>
    </div>
</div>

<script type="text/javascript">
    function HideQuote() {
        grab('Header_lbl_quote').style.opacity = "0";
    }
    function rcb_HideDropDown() {
        var combo = $find("<%= rcb_search.ClientID %>");
        combo.hideDropDown();
    }
    function rcb_Rebind(sender, args) {
        if (sender.get_items().get_count() == 0) {
            return;
        }
        else {
            sender.requestItems(sender.get_text(), false);
        }
    }
    function SetViewLeadID(LeadID) {
        grab("<%= hf_lead_view_id.ClientID %>").value = LeadID;
        grab("<%= btn_view_in_project.ClientID %>").click();
    }
</script>