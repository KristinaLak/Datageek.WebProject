<%--
Author   : Joe Pickering, 23/10/2009 - re-written 08/04/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Daily Sales Report" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="DSR.aspx.cs" Inherits="DSR" %>
 
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    
    <div id="div_page" runat="server" class="normal_page">   
    <hr />
 
        <table width="99%" cellpadding="1" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td valign="top" colspan="2">
                    <asp:Label runat="server" Text="Daily Sales" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium"/><br/><br/>
                </td>
            </tr>
            <tr>
                <td valign="top" width="55%">
                    <table ID="tbl_report" runat="server" border="1" cellpadding="0" cellspacing="0" 
                        style="background:white; font-family:Verdana; font-size:8pt; width:100%;">
                        <tr>
                            <td colspan="4" align="left" style="border-right:0;">
                                <img ID="img_title1" runat="server" src="/images/misc/titlebarextraextralongalpha.png" alt="Daily Sales Report"/>
                                <img ID="img_title2" runat="server" src="/images/icons/dashboard_log.png" alt="Daily Sales Report" height="20" width="20" style="position:relative; left:4px;"/>
                                <asp:Label ID="lbl_report_title" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-294px;"/>
                            </td>
                        </tr>
                        <tr style="background:yellow;">
                            <td>Daily Revenue</td>
                            <td><asp:Label ID="lbl_daily_revenue" runat="server" Font-Bold="true"/></td>
                            <td>Weekly Revenue</td>
                            <td><asp:Label ID="lbl_weekly_revenue" runat="server" Font-Bold="true"/></td>
                        </tr>
                        <tr>
                            <td>CCAs Employed</td>
                            <td><asp:Label ID="lbl_ccas_employed" runat="server"/></td>
                            <td>Input Employed</td>
                            <td><asp:Label ID="lbl_input_employed" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>CCAs Sick</td>
                            <td><asp:Label ID="lbl_ccas_sick" runat="server"/></td>
                            <td>Input Sick</td>
                            <td><asp:Label ID="lbl_input_sick" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>CCAs Holiday</td>
                            <td><asp:Label ID="lbl_ccas_holiday" runat="server"/></td>
                            <td>Input Holiday</td>
                            <td><asp:Label ID="lbl_input_holiday" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>CCAs in Action</td>
                            <td><asp:Label ID="lbl_ccas_in_action" runat="server"/></td>
                            <td>Input in Action</td>
                            <td><asp:Label ID="lbl_input_in_action" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Space in Box</td>
                            <td colspan="3">
                                <asp:TextBox ID="tb_space_in_box" runat="server" Width="50px" Height="12px"/>
                                <asp:CompareValidator runat="server" ControlToValidate="tb_space_in_box" ValidationGroup="DSR"
                                Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Must be number!"/> 
                                <asp:Label ID="lbl_space_in_box" runat="server" Visible="false"/>
                            </td>
                        </tr>
                        <tr>
                            <td>Average Calls</td>
                            <td>
                                <asp:TextBox ID="tb_avg_calls" runat="server" Width="50px" Height="12px"/>
                                <%--<asp:RequiredFieldValidator ID="rfv_avg_calls" runat="server" Enabled="false" ForeColor="Red" ControlToValidate="tb_avg_calls" Display="Dynamic" Text="<br/>Calls required!" Font-Size="8pt" ValidationGroup="DSR"/>--%>
<%--                                <asp:CompareValidator runat="server" ControlToValidate="avgCallTextBox" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>Must be number!"/> --%>
                                <asp:Label ID="lbl_avg_calls" runat="server" Visible="false"/>
                            </td>
                            <td>Average Dials</td>
                            <td>
                                <asp:TextBox ID="tb_avg_dials" runat="server" Width="50px" Height="12px"/>
                                <%--<asp:RequiredFieldValidator ID="rfv_avg_dials" runat="server" Enabled="false" ForeColor="Red" ControlToValidate="tb_avg_dials" Display="Dynamic" Text="<br/>Dials required!" Font-Size="8pt" ValidationGroup="DSR"/>--%>
<%--                                <asp:CompareValidator runat="server" ControlToValidate="avgDialsTextBox" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>Must be number!"/> --%>
                                <asp:Label ID="lbl_avg_dials" runat="server" Visible="false"/>
                            </td>
                        </tr>
                        <tr style="background:LightBlue">
                            <td>Daily Suspects</td>
                            <td><asp:Label ID="lbl_daily_suspects" runat="server"/></td>
                            <td>Weekly Suspects</td>
                            <td><asp:Label ID="lbl_weekly_suspects" runat="server"/></td>
                        </tr>
                        <tr style="background:LightBlue">
                            <td>Daily Prospects</td>
                            <td><asp:Label ID="lbl_daily_prospects" runat="server"/></td>
                            <td>Weekly Prospects</td>
                            <td><asp:Label ID="lbl_weekly_prospects" runat="server"/></td>
                        </tr>
                        <tr style="background:LightBlue">
                            <td>Daily Approvals</td>
                            <td><asp:Label ID="lbl_daily_approvals" runat="server"/></td>
                            <td>Weekly Approvals</td>
                            <td><asp:Label ID="lbl_weekly_approvals" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Daily Sale Approvals</td>
                            <td><asp:Label ID="lbl_daily_sale_approvals" runat="server"/></td>
                            <td>Weekly Sale Approvals</td>
                            <td><asp:Label ID="lbl_weekly_sale_approvals" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Daily LG Approvals</td>
                            <td><asp:Label ID="lbl_daily_lg_approvals" runat="server"/></td>
                            <td>Weekly LG Approvals</td>
                            <td><asp:Label ID="lbl_weekly_lg_approvals" runat="server"/></td>
                        </tr>
                        <tr style="background:LightGreen">
                            <td><asp:Label ID="lbl_book1_name" runat="server"/></td>
                            <td><asp:Label ID="lbl_book1_budget" runat="server"/></td>
                            <td>Days Left</td>
                            <td><asp:Label ID="lbl_book1_days_left" runat="server"/></td>
                        </tr>
                        <tr style="background:LightGreen">
                            <td>Actual (Inc. Red Lines)</td>
                            <td><asp:Label ID="lbl_book1_actual" runat="server"/></td>
                            <td>Daily Requirements</td>
                            <td><asp:Label ID="lbl_book1_daily_reqs" runat="server"/></td>
                        </tr>
                        <tr style="background:LightGreen">
                            <td><asp:Label ID="lbl_book2_name" runat="server"/></td>
                            <td><asp:Label ID="lbl_book2_budget" runat="server"/></td>
                            <td>Days Left</td>
                            <td><asp:Label ID="lbl_book2_days_left" runat="server"/></td>
                        </tr>
                        <tr style="background:LightGreen">
                            <td>Actual (Inc. Red Lines)</td>
                            <td><asp:Label ID="lbl_book2_actual" runat="server"/></td>
                            <td>Daily Requirements</td>
                            <td><asp:Label ID="lbl_book2_daily_reqs" runat="server"/></td>
                        </tr>
                        <tr style="background:MintCream">
                            <td>Prospects</td>
                            <td><asp:Label ID="lbl_no_prospects" runat="server"/></td>
                            <td>Reps</td>
                            <td><asp:Label ID="lbl_no_reps" runat="server"/></td>
                        </tr>
                        <tr style="background:MintCream">
                            <td>P1</td>
                            <td><asp:Label ID="lbl_no_p1" runat="server"/></td>
                            <td>P2</td>
                            <td><asp:Label ID="lbl_no_p2" runat="server"/></td>
                        </tr>  
                        <tr style="background:MintCream; display:none;">
                            <td>P3</td>
                            <td><asp:Label ID="lbl_no_p3" runat="server"/></td>
                            <td>Hot</td>
                            <td><asp:Label ID="lbl_no_hot" runat="server"/></td>
                        </tr>  
                        <tr style="background:MintCream"> 
                            <td>Due this Week</td>
                            <td><asp:Label ID="lbl_no_due_this_week" runat="server"/></td>
                            <td>Due Today</td>
                            <td><asp:Label ID="lbl_no_due_today" runat="server" ForeColor="Orange"/></td>
                        </tr>
                        <tr style="background:MintCream"> 
                            <td>LH Due this Week</td>
                            <td><asp:Label ID="lbl_no_lh_due_this_week" runat="server"/></td>
                            <td>LH Due Today</td>
                            <td><asp:Label ID="lbl_no_lh_due_today" runat="server" ForeColor="Orange"/></td>
                        </tr>
                        <tr style="background:MintCream">
                            <td>Overdue</td>
                            <td><asp:Label ID="lbl_no_overdue" runat="server" ForeColor="Red"/></td>
                            <td>Without Emails</td>
                            <td><asp:Label ID="lbl_no_without_emails" runat="server" ForeColor="Orange"/></td>
                        </tr> 
                        <tr style="background:LightGray">
                            <td>Waiting Lists >= 15</td>
                            <td><asp:Label ID="lbl_waiting_lists_above" runat="server" ForeColor="LimeGreen"/></td>
                            <td>Waiting Lists < 15</td>
                            <td><asp:Label ID="lbl_waiting_lists_below" runat="server" ForeColor="Red"/></td>
                        </tr> 
                        <tr style="background:LightGray">
                            <td>Working Lists >= 15</td>
                            <td><asp:Label ID="lbl_working_lists_above" runat="server" ForeColor="LimeGreen"/></td>
                            <td>Working Lists < 15</td>
                            <td><asp:Label ID="lbl_working_lists_below" runat="server" ForeColor="Red"/></td>
                        </tr> 
                    </table>
                </td>
                <td valign="top" width="45%" rowspan="2">           
                    <table width="100%" style="position:relative; top:3px;">
                        <tr>
                            <td>
                                <asp:Label ID="lbl_subject" Text="Subject" runat="server" ForeColor="White"/>
                                <asp:TextBox ID="tb_subject" TextMode="MultiLine" runat="server" Height="15px" Width="99%" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                            </td>
                        </tr>
                        <tr>
                            <td> 
                                <asp:Label ID="lbl_mailto" Text="Recipients - " runat="server" ForeColor="White"/>
                                <asp:LinkButton runat="server" Text="Save Recipient List" OnClick="SaveRecipients" ForeColor="DarkOrange" Font-Size="7"/><br/>
                                <asp:TextBox ID="tb_mailto" TextMode="MultiLine" Height="66px" Width="99%" runat="server" 
                                style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                                <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_mailto" Display="Dynamic" Text="<br/>E-mails required!" Font-Size="8pt" ValidationGroup="Emails"/>
                                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red" ValidationGroup="Emails"
                                ControlToValidate="tb_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" Display="Dynamic"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lbl_message" runat="server" Text="Message" ForeColor="White"/><br/>
                                <asp:TextBox ID="tb_message" runat="server" TextMode="MultiLine" Width="99%" Height="255px"
                                style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/> 
                            </td>
                        </tr>
                    </table>  
                </td>       
            </tr>
            <tr>
                <td>
                    <table border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tr>
                            <td><asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GenerateReport" Width="120"/></td>
                            <td>
                                <asp:DropDownList ID="dd_day" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GenerateReport" Width="130">
                                    <asp:ListItem>Today</asp:ListItem>
                                    <asp:ListItem>Monday</asp:ListItem>
                                    <asp:ListItem>Tuesday</asp:ListItem>
                                    <asp:ListItem>Wednesday</asp:ListItem>
                                    <asp:ListItem>Thursday</asp:ListItem>
                                    <asp:ListItem>Friday</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td width="20%">&nbsp;</td>
                            <td align="right">
                                <telerik:RadSpell ID="trs_radspell" runat="server" Skin="Black" ControlToCheck="tb_message"
                                SupportedLanguages="en-US,English" DictionaryPath="~/App_Data/RadSpell/"/>
                            </td>
                            <td align="right">
                                <asp:Button ID="btn_send" runat="server" OnClick="SendReport" Text="Send Report" CausesValidation="true"
                                OnClientClick="if(!Page_ClientValidate('Emails')){alert('Ensure recipients are valid first!');}else if(!Page_ClientValidate('DSR')){alert('Please fill in required fields!');}else{return confirm('Are you sure you wish to e-mail this report?');}"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>             
        </table>
        
        <hr />
    </div>
</asp:Content>