<%--
Author   : Joe Pickering, 27/02/14
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Group Performance Report" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="GPR.aspx.cs" Inherits="GPR" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
          .RadGrid_Bootstrap .rgAltRow, .b > td  { background:#B2BCE4 !important; }
          .RadGrid_Bootstrap .rgAltRow, .y > td  { background-color:#FAFF4F !important; }
          .RadGrid_Bootstrap .rgAltRow, .g > td  { background-color:#A4D058 !important; }
          .RadGrid_Bootstrap .rgAltRow, .o > td  { background-color:#E4A02A !important; }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
        <asp:UpdateProgress runat="server">
            <ProgressTemplate>
                <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif?v1"/></div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <asp:UpdatePanel ID="udp_sb" runat="server" ChildrenAsTriggers="true">
        <ContentTemplate>
        <div ID="div_page" runat="server" class="normal_page">   
        <hr />

        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Group Performance" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px; left:1px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px; left:1px;"/> 
                </td>
            </tr>
        </table>
        
        <telerik:RadTabStrip ID="rts" runat="server" SelectedIndex="0" MultiPageID="rmp" CausesValidation="false" Skin="Black" AutoPostBack="true" OnTabClick="TabChanging" style="margin:8px;" Width="190">
            <Tabs>
                <telerik:RadTab runat="server" Text="View by Issue" PageViewID="pv_monthly" Value="Monthly" ToolTip="See figures by monthly issues"/>
                <telerik:RadTab runat="server" Text="View Recent" PageViewID="pv_daily" Value="Daily" ToolTip="See daily figures"/>
            </Tabs>
        </telerik:RadTabStrip>
        <telerik:RadMultiPage ID="rmp" runat="server" SelectedIndex="0" Width="100%">
            <telerik:RadPageView ID="pv_monthly" runat="server">
                <div style="margin:8px">
                    <telerik:RadDropDownList ID="dd_issue" runat="server" Skin="Bootstrap" DropDownHeight="200" Width="200" AutoPostBack="true" OnSelectedIndexChanged="ChangingIssue"/>
                    <div ID="div_monthly" runat="server"/>
                </div>
            </telerik:RadPageView>
            <telerik:RadPageView ID="pv_daily" runat="server">
                <div ID="div_daily" runat="server">
                    <table width="50%" style="margin-left:auto; margin-right:auto;">
                        <tr>
                            <td>                                
                                <table style="color:White; margin-left:auto; margin-right:auto; position:relative; left:4px;">
                                    <tr>
                                        <td><br/><b>Recipients:</b></td>
                                        <td colspan="2" align="right"> 
                                            <asp:DropDownList ID="dd_office" runat="server" width="130" AutoPostBack="true" OnSelectedIndexChanged="BindGroupProgress" style="position:relative; top:3px;">
                                                <asp:ListItem>Group</asp:ListItem>
                                                <asp:ListItem>Americas</asp:ListItem>
                                                <asp:ListItem>UK</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:DropDownList ID="dd_day" runat="server" width="130" AutoPostBack="true" OnSelectedIndexChanged="BindGroupProgress" style="position:relative; top:3px;">
                                                <asp:ListItem>Today</asp:ListItem>
                                                <asp:ListItem>Monday</asp:ListItem>
                                                <asp:ListItem>Tuesday</asp:ListItem>
                                                <asp:ListItem>Wednesday</asp:ListItem>
                                                <asp:ListItem>Thursday</asp:ListItem>
                                                <asp:ListItem>Friday</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr><td colspan="3">
                                    <asp:TextBox ID="tb_mailto" TextMode="MultiLine" Height="80px" Width="572px" runat="server" 
                                    style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="tb_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" 
                                    Display="Dynamic"/>
                                    </td></tr>
                                    <tr><td colspan="3"><b>Message:</b></td></tr>
                                    <tr><td colspan="3"><asp:TextBox ID="tb_mail_message" runat="server" TextMode="MultiLine" Height="150" Width="572"/></td></tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table align="center" style="position:relative; left:4px;">
                                    <tr>
                                        <td>
                                            <asp:Table ID="tbl_group_summary" runat="server" width="580px" border="2" cellpadding="0" cellspacing="0" BackColor="White" style="font-family:Verdana; font-size:8pt; overflow-x:auto; overflow-y:hidden;">   
                                                <asp:TableRow BackColor="Orange">
                                                    <asp:TableCell Width="25%">Daily Revenue</asp:TableCell>
                                                    <asp:TableCell Width="20%"><asp:Label ID="dailyRevenueLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell Width="25%">Weekly Revenue</asp:TableCell>
                                                    <asp:TableCell Width="30%"><asp:Label ID="weeklyRevenueLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>CCAs Employed</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="ccaEmployedLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Input Employed</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="inputEmployedLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>CCAs Sick</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="ccaSickLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Input Sick</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="inputSickLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>CCAs Holiday</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="ccaHolidayLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Input Holiday</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="inputHolidayLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>CCAs in Action</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="ccaWorkingLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Input in Action</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="inputWorkingLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow BackColor="BurlyWood">
                                                    <asp:TableCell>Daily Suspects</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="dailySuspectsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                    <asp:TableCell>Weekly Suspects</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="weeklySuspectsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow BackColor="BurlyWood">
                                                    <asp:TableCell>Daily Prospects</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="dailyProspectsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                    <asp:TableCell>Weekly Prospects</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="weeklyProspectsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow BackColor="BurlyWood">
                                                    <asp:TableCell>Daily Approvals</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="dailyApprovalsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                    <asp:TableCell>Weekly Approvals</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="weeklyApprovalsLabel" ForeColor="Black" runat="server" Text="0"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>Sale Approvals</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="saleAppsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Weekly Sale Apps</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="weeklySalesAppsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>LG Approvals</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="lgApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                    <asp:TableCell>Weekly LG Apps</asp:TableCell>
                                                    <asp:TableCell><asp:Label ID="weeklyLgApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow BackColor="MintCream">
                                                    <asp:TableCell ColumnSpan="4">
                                                        <br />
                                                        <table ID="tbl_budgets" runat="server" width="100%" style="border:solid 1px black; font-family:Verdana; font-size:8pt;">
                                                            <tr>
                                                                <td><i><b>Region</b></i></td>
                                                                <td><i><b>Budget</b></i></td>
                                                                <td><i><b>RR Prediction</b></i></td>
                                                                <td><i><b>Actual USD</b></i></td>
                                                                <td><i><b>% of Budget</b></i></td>
                                                                <td><i><b>Prev. Day USD</b></i></td>
                                                                <td><i><b>Diff.</b></i></td>
                                                            </tr>
                                                        </table>
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </td>   
                                    </tr>           
                                </table>
                            </td>
                        </tr>
                        <tr><td align="right"><asp:Button ID="btn_send_mail" runat="server" OnClick="SendMail" Text="Send Report"/></td></tr>
                    </table>
                </div>
            </telerik:RadPageView>
        </telerik:RadMultiPage>
        <hr/>
    </div>
   
    <%--Hidden GV to grab vals from (just binds like the PR does)--%>
    <asp:GridView ID="gv_pr" runat="server" EnableViewState="true" AutoGenerateColumns="False" Visible="False"
    border="1" Cellpadding="1" AllowSorting="false" Width="100%"
    AllowAdding="False" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8"
    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" RowStyle-BackColor="Khaki"
    CssClass="BlackGridHead" RowStyle-Font-Bold="true"
    OnRowDataBound="gv_pr_RowDataBound">  
        <Columns>                 
            <%--0--%><asp:BoundField ReadOnly="false" DataField="ent_id"/>
            <%--1--%><asp:BoundField ReadOnly="false" DataField="ProgressReportID"/>
            <%--2--%><asp:HyperLinkField ItemStyle-Width="110px" ControlStyle-Width="110px" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" 
            DataTextField="UserID" DataNavigateUrlFields="id" DataNavigateUrlFormatString="~/dashboard/proutput/prccaoutput.aspx?uid={0}"/>     
            <%--Monday--%>        
            <%--3--%><asp:TemplateField InsertVisible="false" HeaderText="Monday" ItemStyle-Height="22px" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="mSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mS").ToString()) %>' />
                    <asp:TextBox ID="mSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mS") %>'/>
                </ItemTemplate>    
            </asp:TemplateField>
            <%--4--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="mPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mP").ToString()) %>' />
                    <asp:TextBox ID="mPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--5--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="mALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mA").ToString()) %>' />
                    <asp:TextBox ID="mATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--6--%><asp:BoundField InsertVisible="false" HeaderText="mAc" DataField="mAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--7--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow" 
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="mTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mTotalRev").ToString()) %>' />
                    <asp:TextBox ID="mTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--Tuesday--%>
            <%--8--%><asp:TemplateField InsertVisible="false" HeaderText="Tuesday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="tSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tS").ToString()) %>' />
                    <asp:TextBox ID="tSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tS") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--9--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="tPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tP").ToString()) %>' />
                    <asp:TextBox ID="tPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--10--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="tALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tA").ToString()) %>' />
                    <asp:TextBox ID="tATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--11--%><asp:BoundField InsertVisible="false" HeaderText="tAc" DataField="tAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--12--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="tTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tTotalRev").ToString()) %>' />
                    <asp:TextBox ID="tTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--Wednesday--%>
            <%--13--%><asp:TemplateField InsertVisible="false" HeaderText="Wednesday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="wSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wS").ToString()) %>' />
                    <asp:TextBox ID="wSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wS") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--14--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="wPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wP").ToString()) %>' />
                    <asp:TextBox ID="wPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--15--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="wALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wA").ToString()) %>' />
                    <asp:TextBox ID="wATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--16--%><asp:BoundField InsertVisible="false" HeaderText="wAc" DataField="wAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--17--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="wTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wTotalRev").ToString()) %>' />
                    <asp:TextBox ID="wTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--Thursday--%>
            <%--18--%><asp:TemplateField InsertVisible="false" HeaderText="Thursday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="thSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thS").ToString()) %>' />
                    <asp:TextBox ID="thSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thS") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--19--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="thPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thP").ToString()) %>' />
                    <asp:TextBox ID="thPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--20--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="thALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thA").ToString()) %>' />
                    <asp:TextBox ID="thATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--21--%><asp:BoundField InsertVisible="false" HeaderText="thAc" DataField="thAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--22--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="thTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thTotalRev").ToString()) %>' />
                    <asp:TextBox ID="thTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--Friday--%>
            <%--23--%><asp:TemplateField InsertVisible="false" HeaderText="Friday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="fSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fS").ToString()) %>' />
                    <asp:TextBox ID="fSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fS") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--24--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="fPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fP").ToString()) %>' />
                    <asp:TextBox ID="fPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--25--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="fALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fA").ToString()) %>' />
                    <asp:TextBox ID="fATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--26--%><asp:BoundField InsertVisible="false" HeaderText="fAc" DataField="fAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--27--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="fTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fTotalRev").ToString()) %>' />
                    <asp:TextBox ID="fTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                                               
            <%--X-Day--%>
            <%--28--%><asp:TemplateField InsertVisible="false" HeaderText="X-Day" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="xSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xS").ToString()) %>' />
                    <asp:TextBox ID="xSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xS") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--29--%><asp:TemplateField InsertVisible="false" HeaderText="X-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="xPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xP").ToString()) %>' />
                    <asp:TextBox ID="xPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xP") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--30--%><asp:TemplateField InsertVisible="false" HeaderText="X-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                <ItemTemplate>
                    <asp:Label ID="xALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xA").ToString()) %>' />
                    <asp:TextBox ID="xATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xA") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
            <%--31--%><asp:BoundField InsertVisible="false" HeaderText="xAc" DataField="xAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
            <%--32--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="xTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xTotalRev").ToString()) %>' />
                    <asp:TextBox ID="xTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xTotalRev") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--33--%><asp:BoundField ReadOnly="true" HeaderText="Weekly" DataField="weS" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>                          
            <%--34--%><asp:BoundField ReadOnly="true" HeaderText="weP" DataField="weP" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>
            <%--35--%><asp:BoundField ReadOnly="true" HeaderText="weA" DataField="weA" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>
                        
            <%--36--%><asp:TemplateField HeaderText="Rev" ItemStyle-Width="50px" ControlStyle-Width="50px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="weTRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("weTotalRev").ToString()) %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                                        
            <%--37--%><asp:TemplateField HeaderText="Pers" ItemStyle-Width="50px" ControlStyle-Width="50px" ItemStyle-BackColor="Yellow"
                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                <ItemTemplate>
                    <asp:Label ID="persRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("PersonalRevenue").ToString()) %>' />
                    <asp:TextBox ID="persRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("PersonalRevenue") %>' />
                </ItemTemplate>     
            </asp:TemplateField>
                
            <%--38--%><asp:BoundField ReadOnly="true" HeaderText="Conv." ItemStyle-BackColor="Moccasin" DataField="weConv" ItemStyle-Width="34px" ControlStyle-Width="0px"/>  
            <%--39--%><asp:BoundField ReadOnly="true" HeaderText="ConvP" ItemStyle-BackColor="Moccasin" DataField="weConv2" ItemStyle-Width="34px" ControlStyle-Width="0px"/> 
            <%--40--%><asp:BoundField ReadOnly="true" DataField="Perf" ItemStyle-Width="34px" ControlStyle-Width="0px" HeaderText="rag"/>    
            <%--41--%><asp:HyperLinkField ItemStyle-Width="150px" ControlStyle-ForeColor="Black" HeaderText="team" DataTextField="Team" ItemStyle-BackColor="#FFFC17" DataNavigateUrlFields="TeamNo" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRTeamOutput.aspx?id={0}"/>     
            <%--42--%><asp:BoundField ReadOnly="true" DataField="sector" ItemStyle-BackColor="#FFFC17" ItemStyle-Width="34px" ControlStyle-Width="0px"/>    
            <%--43--%><asp:BoundField ReadOnly="true" HeaderText="starter" DataField="starter"/>       
            <%--44--%><asp:BoundField ReadOnly="true" HeaderText="cca_level" DataField="cca_level"/>     
        </Columns>
    </asp:GridView>

    <asp:Button ID="btn_save_value" runat="server" OnClick="SaveValueFromGrid" style="display:none;"/>
    <asp:HiddenField ID="hf_save_value_tb_id" runat="server"/>
    <asp:HiddenField ID="hf_save_value_value" runat="server"/>

    <script type="text/javascript">
        function RTBOnBlur(sender, args) {
            $get("<%= hf_save_value_tb_id.ClientID %>").value = sender.get_id();
            $get("<%= hf_save_value_value.ClientID %>").value = sender.get_value();
            $get("<%= btn_save_value.ClientID %>").click(); 
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>


