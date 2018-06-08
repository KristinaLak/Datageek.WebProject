<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Three-Month Planner" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="3MonthPlanner.aspx.cs" Inherits="ThreeMonthPlanner" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        input[type=text], textarea
        {
            border:dotted 1px gray;
            background-color:#f3f3f3;
            border-radius:5px;
            color:#3e3e3e;
            font-family:Verdana;
            font-size:small;
        }
        input[type=text]:hover, textarea:hover
        {
            border:dotted 1px black;
            background-color:#ffcb8c;
            color:black;
            border-radius:5px;
            font-family:Verdana; 
            font-size:small;
        }
        .threemp
        {
        	border:dotted 1px DarkOrange;
        }
        .threemp tr td
        {
        	border:none;
        	border-bottom:dotted 1px black;
        	border-left:dotted 1px black;
        	color:#3a3a3a;
        }
        .threemp tr td table tr td
        {
        	border:none;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<telerik:RadWindowManager ID="rwm" VisibleStatusBar="false" 
    UseClassicWindows="true" ReloadOnShow="false" runat="server"> 
    <Windows>
        <telerik:RadWindow runat="server" ID="win_email" Title="&nbsp;E-Mail Planner" OnClientClose="OnClientCloseHandler" Skin="Black"
             Behaviors="Move, Close, Pin" NavigateUrl="EmailPlanner.aspx" AutoSize="true"/>
    </Windows>
</telerik:RadWindowManager>

    <div id="div_page" runat="server" class="wide_page">
        <hr />
            <table width="99%" style="position:relative; left:2px; top:-2px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Three-Month" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Planner" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    </td>
                    <td align="right">
                        <asp:HyperLink ID="hl_show_summary" runat="server" Text="View Summaries" NavigateUrl="~/Dashboard/3MonthPlannerSummary/3MPSummary.aspx" 
                        ForeColor="DarkOrange" Visible="false" style="position:relative; top:-3px; left:4px;"/>
                    </td>
                </tr>
            </table>

            <%--MAIN TABLE--%>
            <table ID="tbl_main" runat="server" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
                <tr>
                    <td>
                        <%--HEADER--%>
                        <table border="0" runat="server" id="headerTable" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td valign="top" align="left">
                                    <table border="1" cellpadding="2" cellspacing="0" width="340px" bgcolor="White">
                                        <tr><td colspan="2" style="border-right:0;">
                                            <img src="/Images/Misc/titleBarLong.png" style="position:relative; top:-2px; left:-2px;"/> 
                                            <asp:Label Text="Office/CCA" runat="server" ForeColor="White" style="position:relative; top:-8px; left:-210px;"/>
                                        </td></tr>
                                        <tr><td>
                                            <%--Area--%> 
                                            <asp:DropDownList ID="dd_office" runat="server" Width="110px" AutoPostBack="true" OnSelectedIndexChanged="GetCCAsInOffice"/>
                                            <asp:DropDownList ID="dd_cca" runat="server" Enabled="false" Width="140px" AutoPostBack="true" OnSelectedIndexChanged="LoadSelectedPlanner"/> 
                                        </td></tr>
                                    </table>
                                </td>
                                <td valign="top" rowspan="2" align="right"> 
                                    <div id="gridViewDiv" runat="server" style="height:217px; overflow:auto; position:relative; top:-3px; left:-1px;">
                                        <table>  
                                            <tr>
                                                <td align="right" colspan="3" valign="top">
                                                    <asp:GridView ID="userUpdatesGridview" runat="server" border="2" AllowSorting="false" AllowPaging="false"
                                                        AllowAdding="True" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                                        RowStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" AutoGenerateColumns="False">
                                                        <Columns>
                                                            <asp:BoundField HeaderText="Office" DataField="office" ItemStyle-Width="100px" />
                                                            <asp:BoundField HeaderText="Planner Of" DataField="friendlyname" ItemStyle-Width="100px"/>
                                                            <asp:BoundField HeaderText="Updated By" DataField="updatedBy" ItemStyle-Width="110px" />
                                                            <asp:BoundField HeaderText="Last Updated" DataField="lastUpdated" ItemStyle-Width="160px"/>
                                                            <asp:BoundField HeaderText="Graded By" DataField="gradedBy" ItemStyle-Width="110px" />
                                                            <asp:BoundField HeaderText="Last Graded" DataField="lastGraded" ItemStyle-Width="150px"/>
                                                            <asp:BoundField HeaderText="Grade" DataField="grade" ItemStyle-Width="100px"/>
                                                        </Columns>
                                                    </asp:GridView>  
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <div style="position:relative; top:-6px;">
                                        <table width="70%">
                                            <tr>
                                                <td align="right">
                                                    <asp:Label ID="onlyThisOfficeLabel" runat="server" Text="Only this Office:" ForeColor="White" style="position:relative; left:147px;"/>
                                                    <asp:CheckBox ID="onlyThisOfficeCheckBox" runat="server" AutoPostBack="true" Checked="true" OnCheckedChanged="GetUserLastUpdates" style="position:relative; left:143px;"/>
                                                </td>
                                                <td align="right" width="52%">
                                                    <asp:RadioButtonList ID="updateOrderRadioList" runat="server" AutoPostBack="true" ForeColor="White" RepeatDirection="Horizontal" OnSelectedIndexChanged="GetUserLastUpdates">
                                                        <asp:ListItem>Oldest First</asp:ListItem>
                                                        <asp:ListItem Selected="true">Latest First</asp:ListItem>
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top" align="left">
                                    <table border="1" cellpadding="2" cellspacing="0" width="340px" bgcolor="White" style="margin-top:3px;">
                                        <tr><td colspan="2" style="border-right:0;">
                                            <asp:Image runat="server" ID="img_summarybar" ImageUrl="/Images/Misc/titleBarLong.png" alt="Summary" style="position:relative; top:-2px; left:-2px;"/> 
                                            <asp:Label runat="server" Text="Summary" ForeColor="White" style="position:relative; top:-8px; left:-210px;"/>
                                        </td></tr>
                                        <tr><td align="left">
                                                <asp:Label runat="server" ID="lbl_SummaryLastUpdated" ForeColor="Black"/><hr />
                                                <asp:Label runat="server" ID="lbl_SummaryMostRecentlyUpdated" ForeColor="Black"/><hr />
                                                <asp:Label runat="server" ID="lbl_SummaryTotalPlanners" ForeColor="Black"/><hr />
                                                <asp:Label runat="server" ID="lbl_SummaryTotalToday" ForeColor="Black"/>
                                        </td></tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                        
                        <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:3px; margin-left:auto; margin-right:auto;">
                            <tr>
                                <td align="left">
                                    <asp:Button ID="btn_email" runat="server" Visible="false" Text="E-Mail Planner" OnClientClick="try{ radopen(null, 'win_email'); }catch(E){ IE9Err(); } return false;" style="position:relative; left:-1px; top:3px;"/>
                                    <asp:Button ID="btn_confirm_grades" runat="server" Visible="false" Text="Confirm and E-mail Grades to CCA" OnClientClick="return confirm('Are you sure?');" OnClick="ConfirmGrades" style="position:relative; left:-1px; top:3px;"/>
                                </td>
                                <td align="right">
                                    <div style="width:680px; border:dotted 1px darkorange; padding:2px; background-color:ButtonText;">
                                        <asp:Label runat="server" Text="3-Month Planner" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_3mp_grade" runat="server" Width="60"/>   
                                        <asp:Label runat="server" Text="Lead Spreadsheets" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_leads_grade" runat="server" Width="60"/>   
                                        <asp:Label runat="server" Text="Google Alerts" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_google_grade" runat="server" Width="60"/>  
                                        <asp:Label runat="server" Text="Overall Quality Control" ForeColor="DarkOrange" /> 
                                        <asp:DropDownList ID="dd_qual_grade" runat="server" Width="60"/>   
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table id="tbl_PlannerTable" width="100%" runat="server" cellpadding="2" cellspacing="0" class="threemp"> 
                            <%--HEAD 1--%>
                            <tr>
                                <td bgcolor="#ffffcc"> 
                                    MONTH
                                    <asp:TextBox runat="server" ID="topMonth" Width="120px"/>
                                </td>
                                <td colspan="2" align="center" bgcolor="#ffffcc">
                                    <asp:Label runat="server" ID="whosePlannerLabel" Font-Size="Medium" style="position:relative; left:10px;"/>
                                    &nbsp;
                                </td>
                                <td align="right" colspan="2" bgcolor="#ffffcc">
                                    <asp:Label runat="server" ID="lastUpdatedLabel"/>
                                    <asp:Button runat="server"  Enabled="false" ID="savePlannerButton" Text="Save Planner" OnClick="SaveThisPlanner"
                                    OnClientClick="if(!Page_ClientValidate()){ alert('Some e-mail addresses entered in the planner are an incorrect format. Please ensure all e-mail addresses entered are valid!\n\nYour planner has not been saved.'); }"/>
                                </td>
                            </tr>
                            <%--TOP 1--%> 
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#ffffcc">
                                    ASSOCIATION/FEDERATION ETC. 1<br />
                                    <asp:TextBox runat="server" ID="topTopAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="topTopAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="topTopAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="topTopAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="topTopAssocFedwww" Width="350px"/>
                                </td>
                                <td colspan="2" bgcolor="#ffffcc">
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="topTopContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#ffffcc"> 
                                    LEADS GENERATED = <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="topTopLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="numValidator2" ControlToValidate="topTopLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table class="innerborder">
                                        <tr>
                                            <td>
                                                SELF GEN/<br/>
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="topTopSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="topTopQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#ffffcc" >
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="topTopAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td bgcolor="#ffffcc">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#ffffcc">
                                    DUE DATE
                                </td>
                            </tr>   
                            <tr valign="top"> 
                                <td bgcolor="#ffffcc">
                                    <asp:RadioButtonList ID="topTopForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#ffffcc">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="topTopDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="topTopDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>    
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#ffffcc" >
                                    ASSOCIATION/FEDERATION ETC. 2<br />
                                    <asp:TextBox runat="server" ID="topBottomAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="topBottomAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="topBottomAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="topBottomAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="topBottomAssocFedwww" Width="350px"/>
                                </td>
                                <td colspan="2" bgcolor="#ffffcc">
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="topBottomContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#ffffcc" >
                                    LEADS GENERATED =  <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="topBottomLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="RegularExpressionValidator5" ControlToValidate="topBottomLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table>
                                        <tr>
                                            <td>
                                                SELF GEN/<br />
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="topBottomSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="topBottomQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#ffffcc" >
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="topBottomAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>  
                            <tr valign="top">
                                <td bgcolor="#ffffcc">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#ffffcc">
                                    DUE DATE
                                </td>
                            </tr>    
                            <tr valign="top">
                                <td bgcolor="#ffffcc">
                                    <asp:RadioButtonList ID="topBottomForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#ffffcc">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="topBottomDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="topBottomDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>                    
                            
                            <%--HEAD 2--%>
                            <tr>  
                                <td colspan="1" bgcolor="#FFDEAD">
                                    MONTH
                                    <asp:TextBox runat="server" ID="middleMonth" Width="120px"/>
                                </td>
                                <td colspan="4" bgcolor="#FFDEAD">
                                    &nbsp;
                                </td>
                            </tr>
                            <%--TOP 2--%> 
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#FFDEAD" >
                                    ASSOCIATION/FEDERATION ETC. 1
                                    <br />
                                    <asp:TextBox runat="server" ID="middleTopAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="middleTopAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="middleTopAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="middleTopAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="middleTopAssocFedwww" Width="350px"/>
                                </td>
                                <td colspan="2" bgcolor="#FFDEAD">
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="middleTopContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#FFDEAD" > 
                                    LEADS GENERATED = <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="middleTopLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="RegularExpressionValidator1" ControlToValidate="middleTopLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table>
                                        <tr>
                                            <td>
                                                SELF GEN/<br />
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="middleTopSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="middleTopQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#FFDEAD" >
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="middleTopAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td bgcolor="#FFDEAD">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#FFDEAD">
                                    DUE DATE
                                </td>
                            </tr>   
                            <tr valign="top">
                                <td bgcolor="#FFDEAD">
                                    <asp:RadioButtonList ID="middleTopForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#FFDEAD">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="middleTopDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="middleTopDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>  
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#FFDEAD" >
                                    ASSOCIATION/FEDERATION ETC. 2<br />
                                    <asp:TextBox runat="server" ID="middleBottomAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="middleBottomAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="middleBottomAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="middleBottomAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="middleBottomAssocFedwww" Width="350px"/> 
                                </td>
                                <td colspan="2" bgcolor="#FFDEAD" >
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="middleBottomContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#FFDEAD" >
                                    LEADS GENERATED =  <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="middleBottomLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="RegularExpressionValidator2" ControlToValidate="middleBottomLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table>
                                        <tr>
                                            <td>
                                                SELF GEN/<br />
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="middleBottomSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="middleBottomQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#FFDEAD" >
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="middleBottomAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>  
                            <tr valign="top">
                                <td bgcolor="#FFDEAD">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#FFDEAD">
                                    DUE DATE
                                </td>
                            </tr>    
                            <tr valign="top">
                                <td bgcolor="#FFDEAD">
                                    <asp:RadioButtonList ID="middleBottomForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#FFDEAD">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="middleBottomDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="middleBottomDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>  
                            
                            <%--HEAD 3--%>
                            <tr> 
                                <td colspan="1" bgcolor="#DEB887">
                                    MONTH
                                    <asp:TextBox runat="server" ID="bottomMonth" Width="120px"/>
                                </td>
                                <td colspan="4" bgcolor="#DEB887">
                                &nbsp;
                                </td>
                            </tr>
                            <%--TOP 3--%> 
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#DEB887" >
                                    ASSOCIATION/FEDERATION ETC. 1<br />
                                    <asp:TextBox runat="server" ID="bottomTopAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="bottomTopAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="bottomTopAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="bottomTopAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="bottomTopAssocFedwww" Width="350px"/>  
                                </td>
                                <td colspan="2" bgcolor="#DEB887">
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="bottomTopContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#DEB887" > 
                                    LEADS GENERATED = <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="bottomTopLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="RegularExpressionValidator3" ControlToValidate="bottomTopLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table>
                                        <tr>
                                            <td>
                                                SELF GEN/<br />
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="bottomTopSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="bottomTopQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#DEB887" >
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="bottomTopAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td bgcolor="#DEB887">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#DEB887">
                                    DUE DATE
                                </td>
                            </tr>   
                            <tr valign="top">
                                <td bgcolor="#DEB887">
                                    <asp:RadioButtonList ID="bottomTopForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#DEB887">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="bottomTopDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="bottomTopDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>
                                
                            <tr valign="top">
                                <td rowspan="3" bgcolor="#DEB887" >
                                    ASSOCIATION/FEDERATION ETC. 2<br />
                                    <asp:TextBox runat="server" ID="bottomBottomAssocFed" TextMode="MultiLine" Height="50px" Width="350px"/>
                                    <br />TEL<br />
                                    <asp:TextBox runat="server" ID="bottomBottomAssocFedTel" Width="210px"/>
                                    <br />E-MAIL<br />
                                    <asp:TextBox runat="server" ID="bottomBottomAssocFedEmail" Width="210px"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="bottomBottomAssocFedEmail" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                                    <br />WWW<br />
                                    <asp:TextBox runat="server" ID="bottomBottomAssocFedwww" Width="350px"/>
                                </td>
                                <td colspan="2" bgcolor="#DEB887">
                                    CONTACT/INTERVIEWEE
                                    <br />
                                    <asp:TextBox runat="server" ID="bottomBottomContact" TextMode="MultiLine" Height="50px" Width="250px"/>
                                </td>
                                <td rowspan="3" bgcolor="#DEB887" >
                                    LEADS GENERATED =  <asp:TextBox style="border-top-style:solid; border-bottom-style:solid;" runat="server" Text="0" ID="bottomBottomLeadsGen" Width="50px"/>
                                    <br />
                                    <asp:RegularExpressionValidator Runat="server" ID="RegularExpressionValidator4" ControlToValidate="bottomBottomLeadsGen" Display="Dynamic"
                                        ForeColor="Brown" ErrorMessage="Please enter a valid number." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                    </asp:RegularExpressionValidator> 
                                    <br />
                                    <br />
                                    <table>
                                        <tr>
                                            <td>
                                                SELF GEN/<br />
                                                MEMBER LIST
                                            </td>
                                            <td>
                                                QUALIFIED 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButtonList ID="bottomBottomSelfGen" runat="server">
                                                    <asp:ListItem Selected="true">SELF GEN</asp:ListItem>
                                                    <asp:ListItem>MEMBER LIST</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td>
                                                <asp:RadioButtonList ID="bottomBottomQualified" runat="server">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="true">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="3" bgcolor="#DEB887">
                                    ANGLE & HOOK
                                    <br />
                                    <asp:TextBox runat="server" ID="bottomBottomAngleHook" TextMode="MultiLine" Height="140px" Width="350px"/>
                                </td>
                            </tr>  
                            <tr valign="top">
                                <td bgcolor="#DEB887">
                                    FOREWORD IN
                                </td>
                                <td bgcolor="#DEB887">
                                    DUE DATE
                                </td>
                            </tr>    
                            <tr valign="top">
                                <td bgcolor="#DEB887">
                                    <asp:RadioButtonList ID="bottomBottomForwardYesNo" runat="server">
                                        <asp:ListItem>Yes</asp:ListItem>
                                        <asp:ListItem Selected="true">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                                <td valign="middle" bgcolor="#DEB887">
                                    Select Date<br />
                                    <telerik:RadDatePicker ID="bottomBottomDateDue" runat="server" AutoPostBack="false" Width="140px" BackColor="Transparent">
                                        <Calendar ID="bottomBottomDateDueCalendar" runat="server">
                                            <SpecialDays>
                                                <telerik:RadCalendarDay Repeatable="Today"/>
                                            </SpecialDays>
                                        </Calendar>
                                    </telerik:RadDatePicker>
                                </td>
                            </tr>  
                            <tr>
                                <td colspan="3" bgcolor="#DEB887">
                                    <b>LHA NOTES</b> <asp:Label runat="server" Text="Use '+' with a following whitespace to add a time-stamped note, e.g. '+ hello'" Font-Size="Smaller" />
                                    <asp:TextBox runat="server" ID="tb_lhanotes" TextMode="MultiLine" Height="200" Width="664"/>
                                </td>
                                <td colspan="2" bgcolor="#DEB887">
                                    <b>Google Alerts</b><br />
                                    <asp:TextBox runat="server" ID="tb_googleAlerts" TextMode="MultiLine" Height="200" Width="550"/>
                                </td>
                            </tr>          
                        </table>
                    </td>
                </tr>
            </table>
        <hr />
    </div>
    
    <asp:Panel runat="server" ID="pnl_mail" style="display:none;">
        <asp:TextBox runat="server" ID="tb_mailto"/>
        <asp:TextBox runat="server" ID="tb_message"/>
        <asp:Button runat="server" ID="btn_send" OnClick="SendPlanner" />
    </asp:Panel>
    
    <script type="text/javascript">
        function OnClientCloseHandler(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%: tb_mailto.ClientID %>").value = data.to;
                grab("<%: tb_message.ClientID %>").value = data.message;
                grab("<%: btn_send.ClientID %>").click();
            }
        }
    </script>
</asp:Content>
