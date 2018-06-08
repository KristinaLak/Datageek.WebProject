<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Log Viewer" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="LogViewer.aspx.cs" Inherits="LogViewer" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="wide_page">
        <hr />
       
            <table width="99%" style="margin-left:auto; margin-right:auto;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Log" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-3px; left:3px;"/> 
                        <asp:Label runat="server" Text="Viewer" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-3px; left:3px;"/>
                        <asp:Label ID="lbl_text_logs_off" runat="server" Text="<br/>NOTE: Text logs are currently disabled! Events are now logged to the database only. Debug files still remain in text format." 
                        ForeColor="DarkOrange"/> 
                    </td>
                </tr>
                <tr>
                    <td align="left" valign="bottom">
                        <asp:Label runat="server" Text="Select Log:" ForeColor="DarkOrange" Font-Names="Verdana"/>
                        <asp:DropDownList id="dd_logname" runat="server" Width="180px" AutoPostBack="true" OnSelectedIndexChanged="LoadLog">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Value="homehub_log">Home Hub</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="salesbook_log">Sales Book</asp:ListItem>
                            <asp:ListItem Value="salesbookoutput_log">Sales Book Output</asp:ListItem>
                            <asp:ListItem Value="statussummary_log">Status Summary</asp:ListItem>
                            <asp:ListItem Value="budgetsheet_log">Budget Sheet</asp:ListItem>
                            <asp:ListItem Value="cashreport_log">Cash Report</asp:ListItem>
                            <asp:ListItem Value="mailer_cashreport_log">Cash Report Mailer</asp:ListItem>
                            <asp:ListItem Value="collections_log">Collections</asp:ListItem>
                            <asp:ListItem Value="magmanager_log">Magazine Links Manager</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="progressreport_log">Progress Report</asp:ListItem>
                            <asp:ListItem Value="progressreportoutput_log">Progress Report Output</asp:ListItem>
                            <asp:ListItem Value="progressreportgroupsummary_log">Progress Report Group Summary</asp:ListItem>
                            <asp:ListItem Value="mailer_progressreportgroupsummary_log">Progress Report Group Summary Mailer</asp:ListItem>
                            <asp:ListItem Value="8weeksummary_log">8-Week Summary</asp:ListItem>
                            <asp:ListItem Value="mailer_8weeksummary_log">8-Week Summary Mailer</asp:ListItem>
                            <asp:ListItem Value="mailer_prlivestats_log">PR Live Stats Mailer</asp:ListItem>
                            <asp:ListItem Value="mailer_prrevenuesummarymailer_log">Weekly Group Revenue Mailer</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="listdistribution_log">List Distribution</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="prospectreports_log">Prospect Reports</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="editorialtracker_log">Editorial Tracker</asp:ListItem> 
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="commissionforms2009_log">Commission Forms 2009</asp:ListItem>
                            <asp:ListItem Value="commissionforms2011A_log">Commission Forms 2011A</asp:ListItem>
                            <asp:ListItem Value="commissionforms2011B_log">Commission Forms 2011B</asp:ListItem>
                            <asp:ListItem Value="commissionforms2012_log">Commission Forms 2012</asp:ListItem>
                            <asp:ListItem Value="commissionforms2013_log">Commission Forms 2013</asp:ListItem>
                            <asp:ListItem Value="commissionforms_log">Commission Forms 2014</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="mediasales_log">Media Sales</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="finance_log">Finance</asp:ListItem>
                            <asp:ListItem Value="mailer_financealerts_log">Finance Reminders Mailer</asp:ListItem>
                            <asp:ListItem Value="mailer_financegroupsummary_log">Finance Group Summary Mailer</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="accountmanagement_log">Account Management</asp:ListItem>
                            <asp:ListItem Value="rolesmanagement_log">Roles Management</asp:ListItem>
                            <asp:ListItem Value="logs_log">Log Viewer</asp:ListItem>
                            <asp:ListItem Value="officemanager_log">Office Manager</asp:ListItem>
                            <asp:ListItem Value="datahubaccess_log">DataHub Access</asp:ListItem>
                            <asp:ListItem Value="datahubaccesslogs_log">DataHub Access Logs</asp:ListItem>
                            <asp:ListItem Value="leaguetables_log">League Tables</asp:ListItem>
                            <asp:ListItem Value="mailer_leaguetables_log">League Tables Mailer</asp:ListItem>
                            <asp:ListItem Value="search_log">Search</asp:ListItem>
                            <asp:ListItem Value="lha_report_log">LHA Report</asp:ListItem>
                            <asp:ListItem Value="signaturetester_log">Signature Test</asp:ListItem>
                            <asp:ListItem Value="8weekreport_log">8-Week Report</asp:ListItem>
                            <asp:ListItem Value="mailer_8weekreport_log">8-Week Report Mailer</asp:ListItem>
                            <asp:ListItem Value="dsr_log">DSR</asp:ListItem>
                            <asp:ListItem Value="mailer_group_dsr_log">Group DSR</asp:ListItem>
                            <asp:ListItem Value="mwd_log">My Working Day</asp:ListItem>
                            <asp:ListItem Value="datahubreport_log">DataHub Report</asp:ListItem>
                            <asp:ListItem Value="gpr_log">GPR</asp:ListItem>
                            <asp:ListItem Value="quarterlyreport_log">Quarterly Report</asp:ListItem> 
                            <asp:ListItem Value="reportgenerator_log">Report Generator</asp:ListItem>
                            <asp:ListItem Value="rsg_log">RSG</asp:ListItem>
                            <asp:ListItem Value="mailer_rsg_log">RSG Mailer</asp:ListItem>
                            <asp:ListItem Value="soa_log">SOA</asp:ListItem>
                            <asp:ListItem Value="weeklygroupsummary_log">Weekly Group Summary</asp:ListItem>
                            <asp:ListItem Value="teams_log">Teams</asp:ListItem> 
                            <asp:ListItem Value="3monthplanner_log">Three-Month Planner</asp:ListItem> 
                            <asp:ListItem Value="training_log">Training</asp:ListItem> 
                            <asp:ListItem Value="trainingrsg_log">Training RSG</asp:ListItem> 
                            <asp:ListItem Value="userlist_log">User List</asp:ListItem>
                            <asp:ListItem>--</asp:ListItem>
                            <asp:ListItem Value="logins_log">User Logins</asp:ListItem>
                            <asp:ListItem Value="logouts_log">User Logouts</asp:ListItem>
                            <asp:ListItem Value="requestpassword_log">Password Requests</asp:ListItem>
                            <asp:ListItem Value="feedbacksurvey_log">Feedback Survey</asp:ListItem>
                        </asp:DropDownList>
                        <asp:Label runat="server" Text="Line Limit:" ForeColor="DarkOrange" />
                        <asp:DropDownList ID="dd_line_limit" runat="server" AutoPostBack="true" OnSelectedIndexChanged="LoadLog">
                            <asp:ListItem Text="10"/>
                            <asp:ListItem Text="25"/>
                            <asp:ListItem Text="50"/>
                            <asp:ListItem Text="100"/>
                            <asp:ListItem Text="250" Selected="true"/>
                            <asp:ListItem Text="500"/>
                            <asp:ListItem Text="1000"/>
                            <asp:ListItem Text="5000"/>
                            <asp:ListItem Text="10000"/>
                        </asp:DropDownList>
                        <asp:Button ID="btn_refresh" runat="server" OnClick="LoadLog" Text="Refresh Log"/> 
                        <asp:Button ID="btn_save_debug" runat="server" OnClick="SaveDebugFile" Text="Save Debug File" Visible="false"/> 
                        <asp:Label ID="lbl_loaddetails" runat="server" ForeColor="White" style="font-family:Verdana; font-size:8pt;"/>
                    </td>
                </tr>
                <tr>
                    <td align="center">
                        <asp:TextBox ID="tb_logwindow" runat="server" TextMode="MultiLine" Height="550px" Width="1236px" style="border:solid 1px #be151a;"/>
                    </td>
                </tr>
            </table>
            
        <hr/>
    </div>
</asp:Content>
