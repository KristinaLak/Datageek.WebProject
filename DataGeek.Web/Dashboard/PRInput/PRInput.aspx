<%--
// Author   : Joe Pickering, 23/10/2009 - re-written 03/05/2011 for MySQL -- modified 07/02/12
// For      : BizClik Media - DataGeek Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Progress Report" ValidateRequest="false" Language="C#" AutoEventWireup="true" CodeFile="PRInput.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="ProgressReport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="/usercontrols/officetoggler.ascx" TagName="OfficeToggler" TagPrefix="cc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator runat="server" DecoratedControls="Buttons" Skin="Bootstrap"/>
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <div ID="div_page" runat="server" class="wider_page">
        <hr/>
        <asp:UpdatePanel ID="udp_pr" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusBar="false" Skin="Black" AutoSize="true" ShowContentDuringLoad="false"> 
                    <Windows>
                        <telerik:RadWindow ID="win_editreport" runat="server" Title="&nbsp;Modify Report" OnClientClose="ForceRefresh" Behaviors="Move, Close" NavigateUrl="preditreport.aspx"/> 
                    </Windows>
                </telerik:RadWindowManager>

                <%--Main Table--%>
                <table width="99%" style="margin-left:auto; margin-right:auto;">
                    <tr>
                        <td align="left" valign="top" colspan="2">
                            <asp:Label runat="server" Text="Progress" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                            <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:ImageButton ID="imbtn_new_report" runat="server" alt="New Report" Height="22" Width="22" ImageUrl="~\images\icons\progressreport_fastnewreport.png" OnClick="NewReport" style="position:relative; left:-1px; top:2px; float:left;"/>
                            <cc:OfficeToggler ID="ot" runat="server" Top="8" Left="8" IncludeRegionalGroup="True"/>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="left">    
                            <%-- Navigation --%> 
                            <table border="1" cellpadding="0" cellspacing="0" width="400px" bgcolor="White">
                                <tr>
                                    <td valign="top" colspan="2" style="border-right:0">
                                        <img src="/images/misc/titlebarlong.png"/> 
                                        <img src="/images/icons/button_progressreportinput.png" alt="Report" height="20px" width="20px"/>
                                        <asp:Label ID="lbl_current_report" runat="server" Text="Office/Year/Report" ForeColor="White" style="position:relative; top:-6px; left:-232px;"/>                
                                    </td>
                                    <td align="center" style="border-left:0">
                                        <asp:ImageButton ID="imbtn_refresh" runat="server" alt="Refresh Report" Height="21" Width="21" ImageUrl="~\images\icons\dashboard_refresh.png" OnClick="BindReport"/> 
                                    </td>
                                </tr>
                                <tr>
                                    <td width="30" align="center">
                                        <asp:ImageButton ID="imbtn_nav_left" runat="server" ToolTip="Previous Report" Height="22" ImageUrl="~\images\icons\dashboard_leftgreenarrow.png" OnClick="PrevReport"/>  
                                    </td>
                                    <td valign="middle">
                                        <div runat="server" id="div_naviagte" style="position:relative; left:3px;">
                                            <asp:DropDownList ID="dd_office" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="ChangeOffice" style="margin:2px 0px 2px 0px;"/>
                                            <asp:DropDownList ID="dd_year" runat="server" Enabled="false" Width="70px" AutoPostBack="true" OnSelectedIndexChanged="ChangeYear" style="margin:2px 0px 2px 0px;"/> 
                                            <asp:DropDownList ID="dd_report" runat="server" Enabled="false" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindReport" style="margin:2px 0px 2px 0px;"/> 
                                            <asp:ImageButton ID="imbtn_toggle_order" runat="server" ToolTip="Toggle New to Old" Height="22" 
                                                ImageUrl="~\images\icons\dashboard_newtoold.png" OnClick="ToggleDateOrder" style="float:right; margin-right:2px; margin-top:1px;"/> 
                                        </div>
                                    </td>
                                    <td width="30" align="center">
                                        <asp:ImageButton ID="imbtn_nav_right" runat="server" ToolTip="Next Report" Height="22" ImageUrl="~\images\icons\dashboard_rightgreenarrow.png" OnClick="NextReport"/> 
                                    </td>
                                </tr>
                            </table>    
                        </td>
                        <td valign="top" align="right">
                            <%--Change CCA Status (colours) --%>
                            <table ID="tbl_statuses" runat="server" border="1" cellpadding="0" cellspacing="0" width="440px" bgcolor="White">   
                                <tr>
                                    <td valign="top" align="left">
                                        <img src="/images/misc/titlebaralpha.png" alt="Set Status"/>
                                        <img src="/images/icons/dashboard_colours.png" alt="Set Status" height="18px" width="18px"/>
                                        <asp:Label Text="Set CCA Status" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-192px;"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left">
                                        <table cellpadding="0" cellspacing="0">
                                             <tr>
                                                <td rowspan="2" valign="top">                                                            
                                                    <telerik:RadTreeView ID="rtv_cca" runat="server" CheckBoxes="True"
                                                        TriStateCheckBoxes="true" CheckChildNodes="true" style="margin:4px;">
                                                        <Nodes>
                                                            <telerik:RadTreeNode Text="CCA" Expanded="true"/>
                                                        </Nodes>
                                                    </telerik:RadTreeView>
                                                </td>
                                                <td rowspan="2" valign="top">                                                            
                                                    <telerik:RadTreeView ID="rtv_days" runat="server" CheckBoxes="True"
                                                        TriStateCheckBoxes="true" CheckChildNodes="true" style="margin:4px;">
                                                        <Nodes>
                                                            <telerik:RadTreeNode Text="Days" Expanded="false">
                                                                <Nodes>
                                                                    <telerik:RadTreeNode Text="Mon" Value="mAc"/>
                                                                    <telerik:RadTreeNode Text="Tues" Value="tAc"/>
                                                                    <telerik:RadTreeNode Text="Weds" Value="wAc"/>
                                                                    <telerik:RadTreeNode Text="Thurs" Value="thAc"/>
                                                                    <telerik:RadTreeNode Text="Fri" Value="fAc"/>
                                                                </Nodes>
                                                            </telerik:RadTreeNode>
                                                        </Nodes>
                                                    </telerik:RadTreeView>
                                                </td>
                                                <td valign="top">
                                                    <%--Status Dropdown--%> 
                                                    <asp:DropDownList ID="dd_cca_status" runat="server" Width="120px" Height="22" style="margin:4px;">
                                                        <asp:ListItem Text="Normal" Value="w"/>
                                                        <asp:ListItem Text="Holiday" Value="g"/>
                                                        <asp:ListItem Text="Holiday Half-Day" Value="G"/>
                                                        <asp:ListItem Text="Sick" Value="r"/>
                                                        <asp:ListItem Text="Sick Half-Day" Value="R"/>
                                                        <asp:ListItem Text="Starter" Value="b"/>
                                                        <asp:ListItem Text="Business Trip" Value="p"/>
                                                        <asp:ListItem Text="B-Trip Half-Day" Value="P"/>
                                                        <asp:ListItem Text="Bank Holiday" Value="h"/>
                                                        <asp:ListItem Text="Terminated" Value="t"/>
                                                        <asp:ListItem Text="Training" Value="x"/>
                                                        <asp:ListItem Text="Training Half-Day" Value="X"/>
                                                    </asp:DropDownList>
                                                </td>
                                                <td valign="top" align="right">
                                                    <telerik:RadButton ID="btn_assign_status" runat="server" Width="55" Text="Apply" ToolTip="Change the colour scheme of a CCA" OnClick="AssignColours" Skin="Bootstrap" CssClass="ShortBootstrapRadButton" style="margin:4px;"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <%-- Header --%>
                        <td align="left" valign="bottom">
                            <br />
                            <table cellpadding="0" cellspacing="0" style="margin-left:2px; float:left;">
                                <tr>
                                    <td><div ID="div_edit_report" runat="server" style="margin-right:3px;"><asp:Button ID="btn_edit_report" runat="server" Text="Modify Report" style="cursor:pointer; outline:none;"/></div></td>
                                    <td><div style="margin-right:4px;"><asp:Button ID="btn_print" runat="server" Text="Printer Version" OnClick="ViewPrintableVersion" style="cursor:pointer; outline:none;"/></div></td>
                                    <td><div style="display:none;"><asp:Button ID="btn_toleague" runat="server" Text="View League" OnClick="ToLeague" style="cursor:pointer; outline:none;"/></div></td>
                                    <td><div ID="div_groupview_options" runat="server" style="white-space:nowrap; position:relative; top:4px;">
                                        <asp:CheckBox ID="cb_expand_group" runat="server" Text="Expand Group View" ForeColor="DarkOrange" Checked="false" AutoPostBack="true" OnCheckedChanged="BindReport"/>
                                        <asp:CheckBox ID="cb_refresh_groupview" runat="server" Text="Auto-Refresh every" ForeColor="DarkOrange" ToolTip="Refresh the Group view every 5 minutes" Checked="true" AutoPostBack="true" OnCheckedChanged="BindReport"/>
                                        <asp:DropDownList ID="dd_refresh_interval" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindReport">
                                            <asp:ListItem Text="10 seconds" Value="10000"/>
                                            <asp:ListItem Text="30 seconds" Value="30000"/>
                                            <asp:ListItem Text="1 minute" Value="60000"/>
                                            <asp:ListItem Text="2 minutes" Value="120000"/>
                                            <asp:ListItem Text="5 minutes" Selected="True" Value="300000"/>
                                            <asp:ListItem Text="10 minutes" Value="600000"/>
                                        </asp:DropDownList>
                                    </div></td>
                                </tr>
                            </table>
                            <div ID="div_la_exception" runat="server" Visible="false" style="float:left; padding-left:4px; position:relative; top:11px;">
                                <asp:Label ID="lbl_la_exception" runat="server" ForeColor="DarkOrange" Font-Size="8pt"/>
                                <asp:CheckBox ID="cb_la_exception" runat="server" AutoPostBack="true" OnCheckedChanged="BindReport" Text="Show data for all LATAM employees" ForeColor="White"/>
                            </div>
                        </td>
                        <td align="right" valign="bottom">
                            <%-- Modify Report/Print/Edit Report--%>
                            <table border="0" cellpadding="0" cellspacing="0" style="margin-right:2px;">
                                <tr>
                                    <td valign="bottom"><asp:Label ID="lbl_last_updated" runat="server" ForeColor="Silver" style="position:relative; top:-2px; left:-5px;"/></td>
                                    <td valign="bottom" align="right"><asp:Button ID="btn_save_all" runat="server" Text="Update" OnClick="SaveAll" Visible="false" style="cursor:pointer; outline:none;"/>&nbsp;</td>
                                    <td valign="bottom" align="right"><asp:Button ID="btn_edit_all" runat="server" Text="Edit Report" OnClick="EditAll" style="cursor:pointer; outline:none;"/></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" colspan="2">
                            <%-- Main GridView --%> 
                            <div id="div_gv" runat="server">
                                <asp:GridView ID="gv_pr" runat="server" EnableViewState="true" AutoGenerateColumns="False" 
                                    border="1" Cellpadding="1" AllowSorting="false" Width="100%"
                                    AllowAdding="False" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8"
                                    HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" RowStyle-BackColor="Khaki"
                                    CssClass="BlackGridHead" RowStyle-Font-Bold="true"
                                    OnRowDataBound="gv_pr_RowDataBound">  
                                    <Columns>                 
                                        <%--0--%><asp:BoundField ReadOnly="false" DataField="ProgressID"/>
                                        <%--1--%><asp:BoundField ReadOnly="false" DataField="ProgressReportID"/>
                                        <%--2--%><asp:HyperLinkField ItemStyle-Width="110px" ControlStyle-Width="110px" HeaderText="name" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" 
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

                                        <%--38--%><asp:TemplateField HeaderText="Cncs" ItemStyle-Width="10px" ControlStyle-Width="34px" ItemStyle-BackColor="Yellow"
                                            ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                                            <ItemTemplate>
                                                <asp:Label ID="lbl_connections" Visible='<%# !(bool)ViewState["FullEditMode"] %>' Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("Connections").ToString()) %>' />
                                                <asp:TextBox ID="tb_connections" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("Connections") %>' />
                                            </ItemTemplate>     
                                        </asp:TemplateField>

                                        <%--39--%><asp:BoundField ReadOnly="true" HeaderText="Conv." ItemStyle-BackColor="Moccasin" DataField="weConv" ItemStyle-Width="34px" ControlStyle-Width="0px"/>  
                                        <%--40--%><asp:BoundField ReadOnly="true" HeaderText="ConvP" ItemStyle-BackColor="Moccasin" DataField="weConv2" ItemStyle-Width="34px" ControlStyle-Width="0px"/> 
                                        <%--41--%><asp:BoundField ReadOnly="true" DataField="Perf" ItemStyle-Width="34px" ControlStyle-Width="0px" HeaderText="rag"/>    
                                        <%--42--%><asp:HyperLinkField ItemStyle-Width="150px" ControlStyle-ForeColor="Black" HeaderText="team" DataTextField="Team" ItemStyle-BackColor="#FFFC17" DataNavigateUrlFields="TeamNo" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRTeamOutput.aspx?id={0}"/>     
                                        <%--43--%><asp:BoundField ReadOnly="true" DataField="sector" ItemStyle-BackColor="#FFFC17" ItemStyle-Width="34px" ControlStyle-Width="0px"/>    
                                        <%--44--%><asp:BoundField ReadOnly="true" HeaderText="starter" DataField="starter"/>       
                                        <%--45--%><asp:BoundField ReadOnly="true" HeaderText="cca_level" DataField="cca_level"/>     
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="left" colspan="2">
                            <%--Footer--%> 
                            <table border="0" cellpadding="0" cellspacing="0" width="1286"> 
                                <tr>
                                    <td valign="top">
                                        <%-- Activity Log --%>
                                        <table border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                                            <tr>
                                                <td valign="top" align="left">
                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Log"/>
                                                    <img src="/Images/Icons/dashboard_Log.png" alt="Log" height="20px" width="20px"/>
                                                    <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-194px;"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><asp:TextBox ID="tb_log" runat="server" TextMode="MultiLine" Height="170px" Width="600px"/></td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td valign="top">
                                        <%--Value Generated Table--%>
                                        <table border="1" cellpadding="0" cellspacing="0" bgcolor="White" width="200px"> 
                                            <tr>
                                                <td colspan="2"><b>Value Generated this Week</b></td>
                                            </tr>
                                            <tr >
                                                <td><asp:Label ID="lbl_through_s_or_a" runat="server" Text="Through Suspects"/></td>
                                                <td><asp:Label ID="lbl_prs_s" runat="server" Font-Bold="true"/></td>
                                            </tr>
                                            <tr>
                                                <td>Through Prospects</td>
                                                <td><asp:Label ID="lbl_prs_p" runat="server" Font-Bold="true"/></td>
                                            </tr>
                                            <tr>
                                                <td>Through Approvals</td>
                                                <td><asp:Label ID="lbl_prs_a" runat="server" Font-Bold="true"/></td>
                                            </tr>
                                            <tr>
                                                <td><b>Total</b></td>
                                                <td><asp:Label ID="lbl_prs_total" runat="server" Font-Bold="true"/></td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td valign="top">
                                        <%-- Report Summary --%>
                                        <table border="1" cellpadding="0" cellspacing="0" bgcolor="White" width="170px"> 
                                            <tr>
                                                <td colspan="4" align="left"><b>CCA Summary</b></td>
                                            </tr>
                                            <tr>
                                                <td>No. CCAs&nbsp;</td>
                                                <td><asp:Label ID="lbl_prs_total_ccas" runat="server" Font-Bold="true"/>&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td>No. List Gens&nbsp;</td>
                                                <td><asp:Label ID="lbl_prs_total_lgs" runat="server" Font-Bold="true"/>&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td>No. Sales&nbsp;</td>
                                                <td><asp:Label ID="lbl_prs_total_sales" runat="server" Font-Bold="true"/>&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td>No. Comm Only&nbsp;</td>
                                                <td><asp:Label ID="lbl_prs_total_comm" runat="server" Font-Bold="true"/>&nbsp;</td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td valign="top">
                                        <%--Key--%>
                                        <table border="1" cellpadding="0" cellspacing="0" bgcolor="White" width="300"> 
                                            <tr>
                                                <td colspan="6" align="left"><b>Status Key</b></td>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;Sick</td>
                                                <td bgcolor="Red" width="14"/>
                                                <td colspan="3">&nbsp;Sick Half-Day</td>
                                                <td bgcolor="FireBrick" width="14"/>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;Holiday</td>
                                                <td bgcolor="Green"/>
                                                <td colspan="3">&nbsp;Holiday Half-Day</td>
                                                <td bgcolor="LimeGreen"/>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;Business Trip</td>
                                                <td bgcolor="Plum"/>
                                                <td colspan="3">&nbsp;Business Trip Half-Day</td>
                                                <td bgcolor="DarkOrchid"/>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;Training</td>
                                                <td bgcolor="Orange"/>
                                                <td colspan="3">&nbsp;Training Half-Day</td>
                                                <td bgcolor="Chocolate"/>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;Bank Holiday</td>
                                                <td bgcolor="Purple"/>
                                                <td>&nbsp;Terminated</td>
                                                <td bgcolor="Black" width="14"></td>
                                                <td>&nbsp;Starter</td>
                                                <td bgcolor="CornflowerBlue" width="14"/>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:HiddenField ID="hf_is_user_in_uk" runat="server" Value="0"/>
        <hr/>             
    </div>
    
    <script type="text/javascript">
        var row = null;
        var cell = null;

        function NavigateCell(key) {
            var dd = grab("<%= dd_office.ClientID %>");
            var office = dd.options[dd.selectedIndex].value;
            var is_six_day = false;
            var gridView = grab("<%= gv_pr.ClientID %>");
            // If using arrow keys
            if (key.keyCode == 37 || key.keyCode == 38 || key.keyCode == 39 || key.keyCode == 40) {
                if (key.keyCode == 37)  // Left
                {
                    if (cell >= 1) { cell--; }
                }
                else if (key.keyCode == 39) // Right
                {
                    if (is_six_day) {
                       if (cell < 25) { cell++; }
                    }
                    else if (cell < 21) { cell++; }
                }
                else if (key.keyCode == 38) // Up
                {
                    if (row >= 2) { row--; }
                }
                else if (key.keyCode == 40) // Down
                {
                    if (row <= (gridView.rows.length - 3)) { row++; }
                }
                
                var gridRow = gridView.rows[(row + 1)];
                var gridCell = gridRow.cells[(cell + 1)];

                if (gridCell != null && gridCell.children[0] != null) {
                    gridCell.children[0].focus();
                }
            }
            else {
                return;
            }
        }
        function SelectCell(r, c) {
            row = r;
            cell = c;
        }
        function ExtendedSelect(t) {
            setTimeout(function() { t.select(); }, 4);
        }
        function ForceRefresh(sender, args) {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.click();
            return true;
        }
        function ViewOffice(office) {
            var dd = grab("<%= dd_office.ClientID %>");
            for (var i = 0; i < dd.options.length; i++) {
                if (dd.options[i].text === office) {
                    dd.selectedIndex = i;
                    break;
                }
            }
            ForceRefresh();
            return false;
        }
    </script>
</asp:Content>

