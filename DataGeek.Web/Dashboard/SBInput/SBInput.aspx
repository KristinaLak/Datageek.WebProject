<%--
// Author   : Joe Pickering, 23/10/2009 -- re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Title="DataGeek :: Sales Book" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true"  MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SBInput.aspx.cs" Inherits="SBInput" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="/usercontrols/officetoggler.ascx" TagName="OfficeToggler" TagPrefix="cc"%>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .pager span { color:#009900; font-weight:bold; font-size:10pt; }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif?v1"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_sb" runat="server" ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="btn_export"/>
    </Triggers>
    <ContentTemplate>
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" Behaviors="Move, Close, Pin" AutoSize="true" ShowContentDuringLoad="false"> 
        <Windows>
            <telerik:RadWindow ID="win_export" runat="server" Title="&nbsp;Sales Book Export" OnClientClose="ExpOnClientClose" NavigateUrl="sbinputexport.aspx"/>
            <telerik:RadWindow ID="win_newbook" runat="server" Title="&nbsp;New Book" Behaviors="Move, Close, Resize, Pin" OnClientClose="RadWindowClientClose" NavigateUrl="sbnewbook.aspx"/> 
            <telerik:RadWindow ID="win_newsale" runat="server" Title="&nbsp;New Sale" OnClientClose="RadWindowClientClose"/>
            <telerik:RadWindow ID="win_newredline" runat="server" Title="&nbsp;New Red Line" OnClientClose="RadWindowClientClose"/>
            <telerik:RadWindow ID="win_editlinks" runat="server" Title="&nbsp;Edit Magazine Links"/>
            <telerik:RadWindow ID="win_editsale" runat="server" Title="&nbsp;Edit Sale" OnClientClose="RadWindowClientClose"/> 
            <telerik:RadWindow ID="win_viewtac" runat="server" Title="&nbsp;Terms and Conditions Viewer" NavigateUrl="sbviewtac.aspx"/>
            <telerik:RadWindow ID="win_movesale" runat="server" Title="&nbsp;Move Sale" OnClientClose="RadWindowClientClose"/> 
            <telerik:RadWindow ID="win_editrl" runat="server" Title="&nbsp;Edit Red Line Value" OnClientClose="RadWindowClientClose"/> 
            <telerik:RadWindow ID="win_groupstats" runat="server" Title="&nbsp;Sales Book Group Stats" NavigateUrl="sbgroupstats.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
    <ajax:BalloonPopupExtender ID="bpe_columns" runat="server"
    TargetControlID="lb_resetview" BalloonPopupControlID="lbl_reset_col_info" Position="TopLeft" BalloonStyle="Rectangle" BalloonSize="Medium"
    UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false" Enabled="false"/>
    <ajax:BalloonPopupExtender ID="bpe_groupbymag" runat="server"
    TargetControlID="cb_grouped_by_mag" BalloonPopupControlID="lbl_group_by_mag_info" Position="TopLeft" BalloonStyle="Rectangle" BalloonSize="Small"
    UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false" Enabled="false"/>
    <ajax:BalloonPopupExtender ID="bpe_links" runat="server"
    TargetControlID="lb_sendlinks" BalloonPopupControlID="lbl_email_mag_links_info" Position="TopLeft" BalloonStyle="Rectangle" BalloonSize="Medium"
    UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false" Enabled="false"/>
    <ajax:BalloonPopupExtender ID="bpe_editall" runat="server"
    TargetControlID="btn_editAll" BalloonPopupControlID="lbl_edit_all_info" Position="TopLeft" BalloonStyle="Rectangle" BalloonSize="Small"
    UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false" Enabled="false"/>
    <ajax:BalloonPopupExtender ID="bpe_editlinks" runat="server"
    TargetControlID="lb_edit_links" BalloonPopupControlID="lbl_edit_links_info" Position="BottomLeft" BalloonStyle="Rectangle" BalloonSize="Medium"
    UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false" Enabled="false"/>
    
    <div id="div_page" runat="server" class="wide_page">      
        <hr />
            <%--Labels--%>
            <table width="99%" style="position:relative; font-family:Verdana; color:white; top:22px; left:12px;">
                <tr>
                    <td align="left" width="67%">
                        <asp:Label runat="server" Text="Sales" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-19px; left:-6px;"/> 
                        <asp:Label runat="server" Text="Book" Font-Size="Medium" style="position:relative; top:-19px; left:-6px;"/>
                        <asp:Label ID="lbl_summary" Text="Book Summary" runat="server" style="display:block;"/> 
                    </td>
                    <td align="left" width="20%"> 
                        <asp:Label ID="lbl_log" Text="&nbsp;Activity Log" runat="server" style="position:relative; top:9px; left:1px;"/>
                    </td>
                    <td align="right" width="13%" valign="top"> 
                        <div style="position:relative; top:-26px; left:-2px;">
                            <asp:LinkButton ID="lb_edit_links" runat="server" ForeColor="DarkGray" Text="Magazine Links" style="padding-right:4px; border-right:solid 1px gray;"/>
                            <asp:LinkButton runat="server" Text="Group Stats" ForeColor="DarkGray" OnClientClick="try{ radopen('SBGroupStats.aspx', 'win_groupstats'); }catch(E){ IE9Err(); } return false;"/>
                            <asp:LinkButton ID="lb_morelessinfo" runat="server" ForeColor="DarkGray" Text="Less Info" OnClick="ShowHideBookInfo" Visible="false"/>
                        </div>
                    </td>
                </tr>
            </table>

            <%--Main Table--%>
            <table border="0" cellspacing="0" cellpadding="0" width="99%" style="margin-left:auto; margin-right:auto;">
                <tr>
                    <td width="33%" valign="top"style="padding-left:2px;" > 
                        <%--Book Summary--%>
                            <table runat="server" id="tbl_summary" border="2" cellpadding="0" cellspacing="0" bgcolor="White" width="402px"> 
                                <tr>
                                    <td valign="top" style="border-right:0;">
                                        <img src="/Images/Misc/titleBarAlpha.png"/>
                                        <img src="/Images/Icons/dashboard_ListGenStats.png" height="20px" width="20px"/>
                                    </td>
                                    <td align="right" colspan="2" style="border-left:0;">
                                        <table cellpadding="0" style="position:relative; left:0px;">
                                            <tr>
                                                <td><asp:LinkButton ID="lb_editBookData" runat="server" OnClientClick="return editSummary();" ForeColor="Gray" Text="Edit Book Dates" style="display:block; padding-left:3px;"/></td>
                                                <td><asp:LinkButton ID="lb_updateBookData" runat="server" OnClick="UpdateBookData" ForeColor="Gray" Text="Update" style="display:none;"/></td>
                                                <td><asp:LinkButton ID="lb_cancelBookData" runat="server" OnClientClick="return cancelSummary();" ForeColor="Gray" Text="Cancel" style="display:none; border-left:solid 1px gray; margin-left:2px; padding-left:3px;"/></td>
                                                <td><asp:Image ID="img_country" runat="server" align="right" height="20px" width="20px" Visible="false"/></td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td width="49%">Book Name</td>
                                    <td colspan="2" width="51%"><asp:label ID="lbl_bookName" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Target</td>
                                    <td colspan="2"> <asp:label ID="lbl_bookTarget" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Total Revenue</td>
                                    <td colspan="2"><asp:label ID="lbl_bookTotalRevenue" Text="-" runat="server" Font-Bold="true"/></td>
                                </tr>
                                <tr>
                                    <td>Start Date</td>
                                    <td style="border-right:0">
                                        <asp:Label ID="lbl_bookStartDate" Text="-" runat="server"/>
                                    </td>
                                    <td align="left" style="border-left:0">
                                        <div runat="server" ID="div_dp_bookStartDate" style="display:none;">
                                            <telerik:RadDatePicker ID="dp_bookStartDate" runat="server" width="86px" >
                                                <Calendar runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </div>
                                        &nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td>End Date</td>
                                    <td style="border-right:0">
                                        <asp:Label ID="lbl_bookEndDate" Text="-" runat="server"/>
                                    </td>
                                    <td align="left" style="border-left:0">
                                        <div runat="server" ID="div_dp_bookEndDate" style="display:none;">
                                            <telerik:RadDatePicker ID="dp_bookEndDate" runat="server" width="86px">
                                                <Calendar runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </div>
                                        &nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td>Days Left to Hit Target</td>
                                    <td style="border-right:0">
                                        <asp:Label ID="lbl_bookDaysLeft" Text="-" runat="server"/>
                                    </td>
                                    <td align="left" style="border-left:0">
                                        <asp:TextBox ID="tb_bookDaysLeft" runat="server" Width="60px" style="display:none;"/>
                                        <asp:RegularExpressionValidator ID="val_tb_bookDaysLeft" runat="server" ControlToValidate="tb_bookDaysLeft" Display="Dynamic"
                                        ErrorMessage="*" ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"/> 
                                    </td>
                                </tr>
                                <tr>
                                    <td>Total Adverts</td>
                                    <td colspan="2"><asp:label ID="lbl_bookTotalAdverts" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Total Unique Features</td>
                                    <td colspan="2"><asp:label ID="lbl_bookUnqFeatures" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Total Re-Runs (From Prev. Issue)</td>
                                    <td colspan="2"><asp:label ID="lbl_bookTotalReruns" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Average Yield</td>
                                    <td colspan="2"><asp:label ID="lbl_bookAvgYield" Text="-" runat="server" /></td>
                                </tr>
                                <tr>
                                    <td>Daily Sales Required</td>
                                    <td colspan="2"><asp:label ID="lbl_bookDailySalesReq" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Page Rate</td>
                                    <td colspan="2"><asp:label ID="lbl_bookPageRate" Text="-" runat="server" /></td>
                                </tr>
                                <tr>
                                    <td>Completion</td>
                                    <td colspan="2"><asp:label ID="lbl_bookCompleteness" Text="-" runat="server" /></td>
                                </tr>
                            </table>
                            <%--Space Sold--%>
                            <table runat="server" id="tbl_soldsummary" border="2" cellpadding="0" cellspacing="0" bgcolor="White" width="402px"> 
                                <tr>
                                    <td align="left" style="border-right:0;">
                                        <img src="/Images/Misc/titleBarAlpha.png"/> 
                                        <img src="/Images/Icons/salesBook_SpaceSold.png" height="20px" width="20px"/>
                                    </td>
                                    <td valign="bottom" align="center" style="border-left:0;">
                                         <asp:Label Text="Space Sold" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-264px;"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td width="51%">Space Sold Today</td>
                                    <td><asp:Label ID="lbl_bookSpaceToday" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Space Sold Yesterday</td>
                                    <td><asp:Label ID="lbl_bookSpaceYday" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Space Left to Hit Target</td>
                                    <td><asp:Label ID="lbl_bookSpaceLeft" Text="-" runat="server"/></td>
                                </tr>
                            </table>
                            <%--End Space Sold--%>
                            <%--Red Lines--%>
                            <table runat="server" id="tbl_redlinesummary" border="2" cellpadding="0" cellspacing="0" bgcolor="White" width="402px"> 
                                <tr>
                                    <td align="left" style="border-right:0;">
                                        <img src="/Images/Misc/titleBarAlpha.png"/> 
                                        <img src="/Images/Icons/salesBook_RedLineOrders.png" height="20px" width="20px"/>
                                    </td>
                                    <td valign="bottom" align="center" style="border-left:0;">
                                         <asp:Label Text="Red Lines" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-267px;"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Red-Line Orders</td>
                                    <td><asp:Label ID="lbl_bookRedLines" Text="-" runat="server"/></td>
                                </tr>
                                <tr>
                                    <td>Outstanding</td>
                                    <td><asp:Label ID="lbl_bookOutstanding" Text="-" runat="server" Font-Bold="true"/></td>
                                </tr>
                                <tr>
                                    <td width="51%">Total Minus Red-Lines</td>
                                    <td><asp:Label ID="lbl_bookTotalMinusRedLines" Text="-" runat="server" Font-Bold="true"/></td>
                                </tr>
                            </table>
                            <%--End Red Lines--%>
                        <%--End Book Summary--%>
                    </td>
                    <td width="33%" align="center" valign="top" rowspan="3">
                        <%--Sales Repeaters--%>
                        <asp:Repeater ID="repeater_salesStats" runat="server" OnItemDataBound="rp_ItemDataBound">
                            <HeaderTemplate>
                                <table border="2" cellpadding="0" cellspacing="0" width="406px" bgcolor="White"><tr>
                                <td colspan="6" align="left">
                                    <img src="/Images/Misc/titleBarAlpha.png"/> 
                                    <img src="/Images/Icons/dashboard_cca.png" height="18px" width="18px"/>
                                    <asp:Label Text="Sales Stats" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-192px;"/>
                                </td></tr>
                                <tr bgcolor="#444444" style="color:White;"><td><b>CCA</td>
                                <td><b>Total</td>
                                <td><b>Adverts</td>
                                <td><b>Features </td>
                                <td><b>Avg. Yield </td>
                                <td><b>No. Pages</td></tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr><td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"Rep").ToString()) %></td>
                                <td><asp:Label ID="rp_sales_total" runat="server" Text='<%#DataBinder.Eval(Container.DataItem,"Total").ToString() %>'/></td>
                                <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"Features").ToString()) %></td>
                                <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"UniqueFeatures").ToString())  %></td>
                                <td><asp:Label ID="rp_sales_avg" runat="server" Text='<%#DataBinder.Eval(Container.DataItem,"Avge").ToString() %>'/></td>
                                <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "Pages").ToString()) %> (<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "Qrs").ToString()) %> Qrts)</td></tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Repeater ID="repeater_listGenStats" runat="server" OnItemDataBound="rp_ItemDataBound">
                            <HeaderTemplate>
                                <table border="2" cellpadding="0" cellspacing="0" width="406px" bgcolor="White"><tr>
                                <td colspan="4" align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/listDist_Issue.png" height="18px" width="18px"/>
                                <asp:Label Text="List Gen Stats" runat="server" ForeColor="White" style="position:relative; top:-5px; left:-192px;"/></td></tr>
                                <tr bgcolor="#444444" style="color:White;"><td><b>CCA</td>
                                <td><b>Total</td>
                                <td><b>Features</td>
                                <td><b>Avg. Yield</td></tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr><td><asp:Label ID="lg_name" runat="server" Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"list_gen").ToString()) %>'/></td>
                                <td><asp:Label ID="lg_sales_total" runat="server" Text='<%#DataBinder.Eval(Container.DataItem,"Total").ToString() %>'/></td>
                                <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"UniqueFeatures").ToString()) %></td>
                                <td><asp:Label ID="lg_sales_avg" runat="server" Text='<%#DataBinder.Eval(Container.DataItem,"Avge").ToString() %>'/></td>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <%--End Sales Repeaters--%>
                    </td>
                    <td width="33%" align="right" valign="top" rowspan="3" style="padding-right:2px;">
                        <%--Console Table--%>      
                        <table runat="server" id="tbl_console" border="2" cellpadding="0" cellspacing="0" bgcolor="White">
                            <tr>
                                <td align="left" style="border-right:0;">
                                    <img src="/Images/Misc/titleBarAlpha.png"/> 
                                    <img src="/Images/Icons/dashboard_Log.png" height="20px" width="20px"/>
                                </td>
                                <td valign="bottom" align="right" style="border-left:0;">
                                    <asp:label ID="lbl_numOnline" runat="server" style="margin-right: 2px;"/>
                                </td>
                            </tr>
                            <tr><td colspan="2"><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="333px" Width="390px"/></td></tr>
                        </table>
                       <%-- End Console Table--%>
                    </td>
                </tr>
                <tr><td><br/></td></tr>
                <tr>
                    <td valign="bottom" align="left" style="padding-left:2px;">
                        <%--Header Table--%> 
                        <asp:ImageButton ID="imbtn_newBook" alt="New Book" runat="server" Height="26" Width="26" ImageUrl="~\Images\Icons\salesBook_AddNewBook.png" 
                        OnClientClick="try{ radopen('SBNewBook.aspx', 'win_newbook'); }catch(E){ IE9Err(); } return false;" style="position:relative; left:-2px; float:left;"/>
                        <div ID="div_ot" runat="server"><cc:OfficeToggler ID="ot" runat="server" Top="8" Left="5"/></div>
                        <table border="2" cellpadding="0" cellspacing="0" width="402px" bgcolor="White" >
                            <tr>
                                <td colspan="2" style="border-right:0;" valign="top">
                                    <img src="/Images/Misc/titleBarLong.png"/> 
                                    <img src="/Images/Icons/button_SalesBookInput.png" height="20px" width="20px"/>
                                    <asp:Label Text="Office/Book" ID="lbl_curbook" runat="server" ForeColor="White" style="position:relative; left:-232px; top:-6px;"/>
                                </td>
                                <td align="center" valign="middle" style="border-left:0;">
                                    <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png" OnClick="Load"/> 
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <asp:ImageButton ID="imbtn_prevBook" ToolTip="Previous Book" height="22" ImageUrl="~\Images\Icons\dashboard_LeftGreenArrow.png" runat="server" OnClick="PrevBook"/> 
                                </td>
                                <td valign="bottom">
                                    <asp:DropDownList ID="dd_office" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="ChangeOffice" style="position:relative; top:-2px; margin-left:4px;"/> 
                                    <asp:DropDownList ID="dd_book" Enabled="false" runat="server" Width="150px" AutoPostBack="true" OnSelectedIndexChanged="Load" style="position:relative; top:-2px;"/>
                                    <asp:ImageButton ID="imbtn_toggleOrder" ToolTip="Toggle New to Old" Height="18" Width="18" ImageUrl="~\images\icons\dashboard_newtoold.png" runat="server" OnClick="ToggleDateOrder" style="position:relative; top:3px;"/>
                                </td>
                                <td align="center">
                                    <asp:ImageButton ID="imbtn_nextBook" ToolTip="Next Book" height="22" ImageUrl="~\Images\Icons\dashboard_RightGreenArrow.png" CommandArgument="DESC" runat="server" OnClick="NextBook"/>  
                                </td>
                            </tr>
                        </table>
                        <%--End Header Table--%>
                    </td>
                </tr>
                <tr><td colspan="3"><br /></td></tr>
                <tr>
                    <td colspan="3">
                        <%--GridView Header--%>
                        <asp:Panel runat="server" ID="panel_gvHeader">
                            <br/>
                            <table width="100%" style="position:relative; top:3px; left:-2px;">
                                <tr>
                                    <td>
                                        <asp:ImageButton ID="imbtn_newSale" alt="New Sale" runat="server" Height="26" Width="26" ImageUrl="~\images\icons\salesbook_addnewsale.png" OnClientClick="try{ radopen(null, 'win_newsale'); }catch(E){ IE9Err(); } return false;"/> 
                                        <asp:ImageButton ID="imbtn_print" alt="View Printer-Friendly Version" runat="server" Height="26" Width="22" ImageUrl="~\images\icons\salesbook_printerfriendlyversion.png" OnClick="PrintGridView"/>
                                        <asp:ImageButton ID="imbtn_export" alt="Export to Excel" runat="server" Height="25" Width="23" ImageUrl="~\images\icons\salesbook_exporttoexcel.png" OnClientClick="try{ radopen('SBInputExport.aspx', 'win_export'); }catch(E){ IE9Err(); } return false;"/>
                                        <asp:ImageButton ID="imbtn_search" alt="Search Book" runat="server" Height="22" Width="22" ImageUrl="~\images\icons\salesbook_opensearch.png" OnClick="OpenSearch"/>
                                    </td>
                                    <td align="right">
                                        <table cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td valign="bottom">
                                                    <div style="position:relative; left:-2px;">
                                                        <asp:CheckBox ID="cb_show_asia_only" runat="server" Visible="false" ForeColor="Silver" Text="Show Asia Deals Only" OnCheckedChanged="Load" AutoPostBack="true" Checked="false" style="padding-right:4px; border-right:solid 1px gray;"/>
                                                        <asp:CheckBox ID="cb_show_orig_price" runat="server" ForeColor="Silver" Text="Show Original Price" OnCheckedChanged="Load" AutoPostBack="true" Checked="false" style="padding-right:4px; border-right:solid 1px gray;"/>
                                                        <asp:LinkButton ID="lb_sendlinks" runat="server" ForeColor="Silver" Text="E-mail Mag Links"
                                                        OnClientClick="return confirm('Are you sure you wish to e-mail all links?\n\nLink mails will only be sent for non-cancelled sales with a valid: contact name, contact e-mail address, BR# and/or CH#, invoice# and corresponding magazine link.\n\nA sale\'s page number will appear green if its link mail has been sent -- automated mails for these sales cannot be sent again and any further correspondence must be made by hand.\n\nPlease be patient, this may take a minute or two.')" OnClick="SendLinkEmails"/>
                                                        <asp:LinkButton ID="lb_resetview" runat="server" ForeColor="Silver" Text="Show All Columns" onmouseout="BalloonPopupControlBehavior.hidePopup();" OnClientClick="return resetColumns();" style="display:none; padding-left:4px; border-left:solid 1px gray;"/>
                                                        <asp:CheckBox ID="cb_grouped_by_mag" runat="server" Checked="true" Text="Group by Mag" ForeColor="Silver" OnCheckedChanged="GroupByMagToggle" AutoPostBack="true" Visible="false" 
                                                        onmouseout="return BalloonPopupControlBehavior.hidePopup();" style="padding-left:2px; border-left:solid 1px gray;"/>
                                                        <asp:LinkButton ID="btn_saveAll" runat="server" ForeColor="Silver" Text="Save All" Visible="false" OnClick="SaveAll" style="padding-left:4px; border-left:solid 1px gray;"/>
                                                        <asp:LinkButton ID="btn_editAll" runat="server" ForeColor="Silver" Text="Edit All" OnClick="EditAll" onmouseout="BalloonPopupControlBehavior.hidePopup();" style="padding-left:4px; border-left:solid 1px gray;"/>
                                                    </div>
                                                </td>
                                                <td>    
                                                    <telerik:RadTabStrip ID="rts_sbview" AutoPostBack="false" MaxDataBindDepth="1" runat="server" SelectedIndex="0"
                                                        BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" OnTabClick="ChangeView" style="position:relative; left:3px; top:2px;">
                                                        <Tabs>
                                                            <telerik:RadTab Text="Standard View"/>
                                                            <telerik:RadTab Text="Ad List View"/>
                                                        </Tabs>
                                                    </telerik:RadTabStrip>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <%--End GridView Header--%>
                    </td>
                </tr>
                <tr>
                    <td colspan="3">
                        <%--Search Container Table--%>
                        <asp:Panel runat="server" DefaultButton="btn_search">
                        <asp:UpdatePanel ID="udp_search" runat="server" ChildrenAsTriggers="true">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="dd_search_territory" EventName="SelectedIndexChanged"/>
                            <asp:AsyncPostBackTrigger ControlID="dd_s_datetypepaid" EventName="SelectedIndexChanged"/>
                        </Triggers>
                        <ContentTemplate>
                        <table ID="tbl_search" visible="false" runat="server" cellpadding="0" style="font-family:Verdana; font-size:8pt; 
                         position:relative; left:3px; background-image:url('/Images/Backgrounds/SBSearchPanel.png');">
                            <tr>
                                <td valign="top">
                                    <asp:Button ID="btn_search" runat="server" Width="60px" Text="Search" OnClick="SearchBook"/><br /><br />
                                    <asp:Button ID="btn_clear" runat="server" Width="60px" Text="Clear" OnClientClick="return clearSearchBook();"/>
                                    <asp:Button runat="server" Text="Close" Width="60px" OnClick="CloseSearch"/>
                                </td> 
                                <td>
                                    <%--Search Table--%>
                                    <table style="margin-left:10px;">
                                        <tr>
                                            <td colspan="14"> 
                                                <asp:Label ID="lbl_search_usd" runat="server" Text="All values are converted into" ForeColor="Black" Font-Bold="true" Visible="false" EnableViewState="false"/>
                                                <asp:Label ID="lbl_searchresults" Visible="false" runat="server" ForeColor="Black"/><br />
                                                <asp:CheckBox ID="cb_search_onlyDeleted" runat="server"/>
                                                <asp:Label Text="Only Non-Cancelled" runat="server" ForeColor="DimGray"/>&nbsp;&nbsp;
                                                <asp:CheckBox ID="cb_search_onlyThisBook" runat="server" Checked="false"/>
                                                <asp:Label ForeColor="LightGray" Text="Only Selected Book" runat="server"/>&nbsp;&nbsp;
                                                <asp:CheckBox ID="cb_search_onlyThisYear" runat="server" Checked="true"/>
                                                <asp:Label ForeColor="LightGray" Text="Only Selected Year" runat="server"/>
                                                <asp:DropDownList ID="dd_search_year" runat="server" Width="60"/>
                                                <asp:Label ForeColor="LightGray" Text="Territory: " runat="server"/>
                                                <asp:DropDownList ID="dd_search_territory" runat="server" Width="100" AutoPostBack="true" OnSelectedIndexChanged="SetCCASearchTrees"/>
                                                <asp:CheckBox ID="cb_search_paginate" runat="server" Checked="true"/>
                                                <asp:Label ForeColor="LightGray" Text="Paginate Results" runat="server"/>
                                                <asp:DropDownList runat="server" ID="dd_search_force_currency" Width="150" Visible="false" style="position:relative; left:3px;">
                                                    <asp:ListItem Text="Respective Currency" Selected="True"/>
                                                    <asp:ListItem Text="Force to USD"/>
                                                    <asp:ListItem Text="Force to GBP"/>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr bgcolor="#444444" style="color:White;">
                                            <td>Q</td>
                                            <td>Date Added</td>
                                            <td>Advertiser</td>
                                            <td>Feature</td>
                                            <td>Size</td>
                                            <td>Price</td>
                                            <td>Rep</td>
                                            <td>Info</td>
                                            <td>Channel</td>
                                            <td>List Gen</td>
                                            <td>Invoice</td>
                                            <td>Date Paid</td>
                                            <td>P#</td>
                                            <td>BP</td>
                                        </tr>
                                        <tr valign="top">
                                            <td>
                                                <asp:DropDownList ID="dd_s_q" runat="server" Width="40px">
                                                    <asp:ListItem Text="" Value="0"></asp:ListItem>
                                                    <asp:ListItem Text="Q1" Value="1"/>
                                                    <asp:ListItem Text="Q2" Value="2"/>
                                                    <asp:ListItem Text="Q3" Value="3"/>
                                                    <asp:ListItem Text="Q4" Value="4"/>
                                                </asp:DropDownList>
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="dd_s_datetypeadded" runat="server" Width="72px" OnChange="return dateSearchType('added');">
                                                    <asp:ListItem></asp:ListItem>
                                                    <asp:ListItem>Cal. Month</asp:ListItem>
                                                    <asp:ListItem>Between</asp:ListItem>           
                                                    <asp:ListItem>Date</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td><asp:TextBox ID="tb_s_advertiser" runat="server" Width="184px"/></td>
                                            <td><asp:TextBox ID="tb_s_feature" runat="server" Width="188px"/></td>
                                            <td>
                                                <asp:DropDownList ID="dd_s_size" runat="server" Width="30px">
                                                    <asp:ListItem></asp:ListItem>
                                                    <asp:ListItem>0</asp:ListItem>
                                                    <asp:ListItem>0.25</asp:ListItem>
                                                    <asp:ListItem>0.5</asp:ListItem>
                                                    <asp:ListItem>1</asp:ListItem>
                                                    <asp:ListItem>2</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td><asp:TextBox ID="tb_s_price" runat="server" Width="42px"/></td>
                                            <td rowspan="3" valign="top">
                                                <div style="width:100px; overflow:auto; ">
                                                    <telerik:RadTreeView ID="tv_s_rep" runat="server" CheckBoxes="True"
                                                        TriStateCheckBoxes="true" CheckChildNodes="true" ForeColor="DarkGray">
                                                        <Nodes>
                                                            <telerik:RadTreeNode Text="" Expanded="true"/>
                                                        </Nodes>
                                                    </telerik:RadTreeView>
                                                </div>
                                            </td>
                                            <td><asp:TextBox ID="tb_s_info" runat="server" Width="66px"/></td>
                                            <td><asp:TextBox ID="tb_s_channel" runat="server" Width="90px"/></td>
                                            <td rowspan="3" valign="top">
                                                <div style="width:100px; overflow:auto;">
                                                    <telerik:RadTreeView ID="tv_s_listgen" runat="server" CheckBoxes="True"
                                                        TriStateCheckBoxes="true" CheckChildNodes="true" ForeColor="DarkGray">
                                                        <Nodes>
                                                            <telerik:RadTreeNode Text="" Expanded="true"/>
                                                        </Nodes>
                                                    </telerik:RadTreeView>
                                                </div>
                                            </td>
                                            <td><asp:TextBox ID="tb_s_invoice" runat="server" Text="" Width="54px"/></td>
                                            <td>
                                                <asp:DropDownList ID="dd_s_datetypepaid" runat="server" AutoPostBack="true" onChange="return dateSearchType('paid');" Width="71px">
                                                    <asp:ListItem></asp:ListItem>
                                                    <asp:ListItem>Not Paid</asp:ListItem>
                                                    <asp:ListItem>Cal. Month</asp:ListItem>
                                                    <asp:ListItem>Between</asp:ListItem>           
                                                    <asp:ListItem>Date</asp:ListItem>
                                                </asp:DropDownList>  
                                            </td>
                                            <td><asp:TextBox ID="tb_s_pageno" runat="server" Text="" Width="12px"/></td>
                                            <td><asp:TextBox ID="tb_s_bp" runat="server" Text="" Width="16px"/></td>  
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td valign="top" colspan="10">
                                                <telerik:RadDatePicker ID="dp_s_dateadded" runat="server" Width="96px" style="display:none;"/>
                                                <asp:DropDownList ID="dd_s_dateaddedmonths"  runat="server" Width="74px" style="display:none;">
                                                    <asp:ListItem>January</asp:ListItem>
                                                    <asp:ListItem>February</asp:ListItem>           
                                                    <asp:ListItem>March</asp:ListItem>
                                                    <asp:ListItem>April</asp:ListItem>
                                                    <asp:ListItem>May</asp:ListItem>
                                                    <asp:ListItem>June</asp:ListItem>
                                                    <asp:ListItem>July</asp:ListItem>
                                                    <asp:ListItem>August</asp:ListItem>
                                                    <asp:ListItem>September</asp:ListItem>
                                                    <asp:ListItem>October</asp:ListItem>
                                                    <asp:ListItem>November</asp:ListItem>
                                                    <asp:ListItem>December</asp:ListItem>
                                                </asp:DropDownList>
                                                <telerik:RadDatePicker ID="dp_s_dateaddedfrom" runat="server" Width="96px" style="display:none;"/>
                                                <telerik:RadDatePicker ID="dp_s_dateaddedto" runat="server" Width="96px" style="display:none;"/>
                                            </td>
                                            <td valign="top" colspan="5">
                                                <telerik:RadDatePicker ID="dp_s_datepaid" runat="server" Width="96px" style="display:none;"/>
                                                <asp:DropDownList id="dd_s_datepaidmonths"  runat="server" Width="74px" style="display:none;">
                                                    <asp:ListItem>January</asp:ListItem>
                                                    <asp:ListItem>February</asp:ListItem>           
                                                    <asp:ListItem>March</asp:ListItem>
                                                    <asp:ListItem>April</asp:ListItem>
                                                    <asp:ListItem>May</asp:ListItem>
                                                    <asp:ListItem>June</asp:ListItem>
                                                    <asp:ListItem>July</asp:ListItem>
                                                    <asp:ListItem>August</asp:ListItem>
                                                    <asp:ListItem>September</asp:ListItem>
                                                    <asp:ListItem>October</asp:ListItem>
                                                    <asp:ListItem>November</asp:ListItem>
                                                    <asp:ListItem>December</asp:ListItem>
                                                </asp:DropDownList>
                                                <telerik:RadDatePicker ID="dp_s_datepaidfrom" runat="server" style="display:none;" Width="96px" AutoPostBack="false"/>
                                                <telerik:RadDatePicker ID="dp_s_datepaidto" runat="server" style="display:none;" Width="96px" AutoPostBack="false"/>
                                            </td>
                                        </tr>
                                    </table>
                                    <%--End Search Table--%>
                                </td>
                            </tr>
                        </table>
                        </ContentTemplate>
                        </asp:UpdatePanel>
                        </asp:Panel>
                        <%--End Search Container Table--%>
                    </td>
                </tr>
                <tr>
                    <td colspan="3" align="center">
                        <%--Sales Gridview--%>
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <asp:GridView ID="gv_s" runat="server" EnableViewState="true" 
                                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="1234px" RowStyle-CssClass="gv_hover"
                                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                                        AutoGenerateColumns="false" AllowSorting="true" HeaderStyle-Font-Size="8" RowStyle-Height="27"
                                        PagerStyle-BackColor="#f0f0f0" PagerStyle-HorizontalAlign="Left" PagerStyle-CssClass="pager"
                                        PagerSettings-Position="TopAndBottom" PagerStyle-ForeColor="Black" OnPageIndexChanging="gv_s_PageIndexChanging" PageIndex="0" PageSize="40"
                                        OnSorting="gv_s_Sorting" OnRowDeleting="gv_s_RowDeleting" OnRowDataBound="gv_s_RowDataBound">
                                        <Columns> 
                                        <%--0--%><asp:CommandField ItemStyle-BackColor="White" ItemStyle-Width="18"
                                                ShowEditButton="true"
                                                ShowDeleteButton="false"
                                                ButtonType="Image"
                                                EditImageUrl="~\images\icons\gridview_edit.png"
                                                CancelImageUrl="~\images\icons\gridvew_canceledit.png"
                                                UpdateImageUrl="~\images\icons\gridview_udate.png"/>
                                        <%--1--%><asp:TemplateField ItemStyle-BackColor="White" ItemStyle-Width="66px"> 
                                                    <ItemTemplate>
                                                        <asp:ImageButton runat="server" CommandName="Delete"
                                                             ImageUrl="~\Images\Icons\gridView_Delete.png" ToolTip="Cancel/Restore"
                                                             OnClientClick="return confirm('Are you sure you wish to cancel/restore this entry?')"/>
                                                        <asp:ImageButton runat="server" ImageUrl="~\images\icons\gridview_changeissue.png" Width="16" Height="16"
                                                             ToolTip="Move Sale/Feature to Another Issue"/>
                                                    </ItemTemplate>
                                                </asp:TemplateField>    
                                        <%--2--%><asp:BoundField DataField="ent_id"/>
                                        <%--3--%><asp:BoundField DataField="sb_id"/>
                                        <%--4--%><asp:BoundField HeaderText="SD" ItemStyle-BackColor="SandyBrown" DataField="sale_day" SortExpression="sale_day" ItemStyle-Width="30px" ControlStyle-Width="24px"/>
                                        <%--5--%><asp:BoundField HeaderText="Added" DataField="ent_date" SortExpression="ent_date" DataFormatString="{0:dd/MM/yyyy}"  ApplyFormatInEditMode="false" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                                        <%--6--%><asp:BoundField HeaderText="Advertiser" ItemStyle-Font-Bold="true" DataField="Advertiser" SortExpression="Advertiser" ItemStyle-Width="200px" ControlStyle-Width="185px"/>
                                        <%--7--%><asp:BoundField HeaderText="Feature" ItemStyle-Font-Bold="true" ItemStyle-BackColor="Plum" DataField="feature" SortExpression="feature" ItemStyle-Width="200px"/>
                                        <%--8--%><asp:BoundField HeaderText="Size" ItemStyle-BackColor="Yellow" DataField="size" SortExpression="size" ItemStyle-Width="30px" ControlStyle-Width="30px"/>
                                        <%--9--%><asp:BoundField HeaderText="Price" DataField="price" SortExpression="price" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                                        <%--10--%><asp:BoundField HeaderText="Rep" DataField="rep" SortExpression="rep" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                                        <%--11--%><asp:BoundField HeaderText="Package" DataField="info" SortExpression="info" ItemStyle-Width="80px" ControlStyle-Width="80px"/>
                                        <%--12--%><asp:BoundField DataField="page_rate"/>
                                        <%--13--%><asp:BoundField HeaderText="Channel Mag" DataField="channel_magazine" SortExpression="channel_magazine" ItemStyle-Width="90px"/>
                                        <%--14--%><asp:BoundField HeaderText="List Gen" DataField="list_gen" SortExpression="list_gen" ItemStyle-Width="100px" ControlStyle-Width="80px"/>
                                        <%--15--%><asp:BoundField HeaderText="Invoice" DataField="invoice" SortExpression="Invoice" ItemStyle-Width="46px" ControlStyle-Width="46px"/>
                                        <%--16--%><asp:TemplateField InsertVisible="false" HeaderText="Date Paid" SortExpression="date_paid" ItemStyle-Width="70px" ControlStyle-Width="70px">
                                                <ItemTemplate>
                                                    <asp:Label Visible='<%# !(bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("date_paid", "{0:dd/MM/yyyy}").ToString()) %>' />
                                                    <asp:TextBox Visible='<%# (bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Eval("date_paid", "{0:dd/MM/yyyy}") %>' />
                                                </ItemTemplate>    
                                                <EditItemTemplate><asp:TextBox runat="server" Text='<%# Eval("date_paid", "{0:dd/MM/yyyy}") %>'/></EditItemTemplate> 
                                            </asp:TemplateField>
                                        <%--17--%><asp:TemplateField InsertVisible="false"  HeaderText="BR#" SortExpression="br_page_no" ItemStyle-Width="25px">
                                                <ItemTemplate>
                                                    <asp:Label Visible='<%# !(bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("br_page_no").ToString()) %>' />
                                                    <asp:TextBox Visible='<%# (bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Eval("br_page_no") %>' />
                                                </ItemTemplate>    
                                                <EditItemTemplate><asp:TextBox runat="server" Text='<%# Eval("br_page_no") %>'/></EditItemTemplate> 
                                            </asp:TemplateField>
                                        <%--18--%><asp:TemplateField InsertVisible="false"  HeaderText="CH#" SortExpression="ch_page_no" ItemStyle-Width="25px">
                                                <ItemTemplate>
                                                    <asp:Label Visible='<%# !(bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("ch_page_no").ToString()) %>' />
                                                    <asp:TextBox Visible='<%# (bool)ViewState["isEditMode"] %>' runat="server" Text='<%# Eval("ch_page_no") %>' />
                                                </ItemTemplate>    
                                                <EditItemTemplate><asp:TextBox runat="server" Text='<%# Eval("ch_page_no") %>'/></EditItemTemplate> 
                                            </asp:TemplateField>
                                        <%--19--%><asp:BoundField DataField="Deleted"/>
                                        <%--20--%><asp:TemplateField HeaderText="BP" SortExpression="bp" ItemStyle-Width="20px" ControlStyle-Width="20px"> 
                                              <ItemTemplate>
                                                <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="gv_s_UpdateBP" Checked='<%# Server.HtmlEncode(Eval("BP").ToString()).Equals("1")  %>'/>
                                              </ItemTemplate>
                                            </asp:TemplateField>
                                        <%--21--%><asp:TemplateField Visible="false" ItemStyle-Width="30px" ItemStyle-BackColor="White">
                                                <ItemTemplate></ItemTemplate>
                                            </asp:TemplateField>  
                                        <%--22--%><asp:BoundField DataField="ftotal"/>      
                                        <%--23--%><asp:TemplateField HeaderText="Contact(s)" SortExpression="al_contact" ItemStyle-Width="110px"> 
                                            <ItemTemplate> 
                                                <asp:HyperLink runat="server" Text='<%# Server.HtmlEncode(Eval("al_contact").ToString()) %>'/>  
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                        <%--24--%><asp:BoundField HeaderText="E-mail" Visible="false" DataField="al_email" ItemStyle-Width="220px" ControlStyle-Width="200px"/> 
                                        <%--25--%><asp:BoundField DataField="contact" HtmlEncode="false" ItemStyle-Width="100px" ControlStyle-Width="95px"/> <%--manually encoded--%>
                                        <%--26--%><asp:BoundField Visible="false" SortExpression="al_mobile" DataField="al_mobile" ItemStyle-Width="100px" ControlStyle-Width="95px" /> 
                                        <%--27--%><asp:BoundField HeaderText="Deadline" SortExpression="al_deadline" DataField="al_deadline" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="60px" ControlStyle-Width="70px"/>
                                        <%--28--%><asp:TemplateField HeaderText="AM" SortExpression="al_admakeup" ItemStyle-Width="20px" ControlStyle-Width="20px"> 
                                              <ItemTemplate>
                                                <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="gv_s_UpdateAM" Checked='<%# Server.HtmlEncode(Eval("al_admakeup").ToString()).Equals("1") %>'/>
                                              </ItemTemplate>
                                            </asp:TemplateField>
                                        <%--29--%><asp:TemplateField HeaderText="SP" SortExpression="al_sp" ItemStyle-Width="20px" ControlStyle-Width="20px"> 
                                              <ItemTemplate>
                                                <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="gv_s_UpdateSP" Checked='<%# Server.HtmlEncode(Eval("al_sp").ToString()).Equals("1") %>'/>
                                              </ItemTemplate>
                                            </asp:TemplateField>    
                                        <%--30--%><asp:BoundField HeaderText="Magazine" DataField="territory_magazine" SortExpression="territory_magazine" ItemStyle-Width="60px" ControlStyle-Width="60px"/>
                                        <%--31--%><asp:BoundField HeaderText="Notes" ItemStyle-Width="250px" SortExpression="al_notes" DataField="al_notes"/>
                                        <%--32--%><asp:TemplateField HeaderText="L" SortExpression="links" ItemStyle-Width="20px" ControlStyle-Width="20px" Visible="false"> 
                                          <ItemTemplate>
                                            <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="gv_s_UpdateLinks" Checked='<%# Server.HtmlEncode(Eval("links").ToString()).Equals("1") %>'/>
                                          </ItemTemplate>
                                        </asp:TemplateField>
                                        <%--33--%><asp:BoundField HeaderText="FN" ItemStyle-Width="18" SortExpression="fnotes" DataField="fnotes"/>
                                        <%--34--%><asp:TemplateField HeaderText="AN" ItemStyle-Width="18" SortExpression="al_rag" ItemStyle-VerticalAlign="Top">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem , "al_rag").ToString()) %>'/>
                                                    <asp:RadioButtonList Visible="false" Font-Size="0" CellPadding="0" CellSpacing="0" AutoPostBack="true" runat="server" OnSelectedIndexChanged="gv_s_StatusChanged" RepeatDirection="Horizontal">
                                                        <asp:ListItem Text="a" style="background-color:Red; border:solid 1px Red;"/>
                                                        <asp:ListItem Text="b" style="background-color:DodgerBlue; border:solid 1px DodgerBlue;"/>
                                                        <asp:ListItem Text="c" style="background-color:Orange; border:solid 1px Orange;"/>
                                                        <asp:ListItem Text="d" style="background-color:Purple; border:solid 1px Purple;"/>
                                                        <asp:ListItem Text="e" style="background-color:Lime; border:solid 1px Lime;"/>
                                                    </asp:RadioButtonList>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        <%--35--%><asp:BoundField DataField="br_links_sent"/>
                                        <%--36--%><asp:BoundField DataField="ch_links_sent"/>
                                        <%--37--%><asp:BoundField DataField="running_feature"/>   
                                        <%--38--%><asp:BoundField DataField="red_lined"/>   
                                        <%--39--%><asp:BoundField DataField="third_magazine"/> 
                                        <%--40--%><asp:BoundField DataField="country"/> 
                                        <%--41--%><asp:BoundField HeaderText="Orig. Price" DataField="orig_price"/> 
                                        <%--42--%><asp:BoundField DataField="override_mag_sb_id"/> 
                                        <%--43--%><asp:BoundField DataField="ad_cpy_id"/>
                                        <%--44--%><asp:BoundField DataField="feat_cpy_id"/>
                                        <%--45--%><asp:BoundField DataField="FourthMagazine"/>
                                        </Columns>
                                    </asp:GridView>  
                                </td>
                            </tr>
                            <tr>
                                <td align="right">
                                    <table ID="tbl_ragcount" runat="server" cellpadding="1" cellspacing="0" border="1" bgcolor="#f0f0f0" 
                                    style="border:solid 1px gray; border-top:0; position:relative; top:-2px; font-size:8pt;">
                                        <tr>
                                            <td align="left">Waiting for Copy</td>
                                            <td bgcolor="red" width="12" align="center"><asp:Label runat="server" ID="lbl_al_wfc"/></td>
                                            <td align="left">&nbsp;Copy In</td>
                                            <td bgcolor="blue" width="12" align="center"><asp:Label runat="server" ID="lbl_al_copyin" ForeColor="White"/></td>
                                            <td align="left">&nbsp;Proof Out</td>
                                            <td bgcolor="Orange" width="12" align="center"><asp:Label runat="server" ID="lbl_al_proofout"/></td>
                                            <td align="left">&nbsp;Own Advert</td>
                                            <td bgcolor="Purple" width="12" align="center"><asp:Label runat="server" ID="lbl_al_ownadvert"/></td>
                                            <td align="left">&nbsp;Approved</td>
                                            <td bgcolor="Lime" width="12" align="center"><asp:Label runat="server" ID="lbl_al_approved"/></td>
                                            <td align="left" colspan="3">&nbsp;Cancelled</td>
                                            <td bgcolor="yellow" width="12" align="center"><asp:Label runat="server" ID="lbl_al_cancelled"/></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                        <br />
                        <%--End Sales GridView--%>
                    </td>
                </tr>
                <tr>
                    <td colspan="3" style="padding-left:2px;">
                        <table width="70%"><tr>
                        <td valign="bottom" align="left" width="50%"><asp:ImageButton id="imbtn_newRedLine" alt="New Red Line" Visible="false" runat="server" Height="25" Width="25" ImageUrl="~\Images\Icons\salesBook_AddRedLine.png" 
                        style="position:relative; top:2px; left:-4px;" OnClientClick="alert('This feature is disabled. Please use the Request Red-Line feature on the Finance system to make an electronic request via e-mail.'); return false;"/></td>
                        <%--OnClientClick="try{ radopen(null, 'win_newredline'); }catch(E){ IE9Err(); } return false;"--%>
                        <td valign="bottom" width="50%"><asp:Label runat="server" ID="lbl_redlines" Text="Red-Line Orders" ForeColor="White" Font-Size="8pt" Font-Names="Verdana"/></td>
                        </tr></table>
                        
                        <%--Red-Lines Gridview--%>
                        <asp:GridView ID="gv_f" runat="server" EnableViewState="true" 
                            Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2" border="2"
                            HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                            RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                            AutoGenerateColumns="false" OnRowDataBound="gv_f_RowDataBound">
                            <Columns>
                                <%--0--%><asp:CommandField ShowEditButton="true"
                                    ShowDeleteButton="false" ItemStyle-BackColor="White" ButtonType="Image"
                                    EditImageUrl="~\images\icons\gridview_edit.png"
                                    CancelImageUrl="~\images\icons\gridview_canceledit.png"
                                    UpdateImageUrl="~\images\icons\gridview_update.png"
                                    ItemStyle-Width="19">
                                </asp:CommandField>
                                <%--1--%><asp:TemplateField ItemStyle-BackColor="White" ItemStyle-Width="20px">
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server"
                                         ImageUrl="~\images\icons\gridview_delete.png"
                                         ToolTip="Permanently Delete" OnClick="gv_f_PermenantlyDelete"
                                         OnClientClick="return confirm('Are you sure you wish to delete this red-line?')"/>
                                    </ItemTemplate>
                                </asp:TemplateField>    
                                <%--2--%><asp:BoundField DataField="ent_id"/>
                                <%--3--%><asp:BoundField DataField="sb_id"/>
                                <%--4--%><asp:BoundField HeaderText="From Book" DataField="IssueName" ItemStyle-Width="100px" ItemStyle-BackColor="SandyBrown"/>
                                <%--5--%><asp:BoundField HeaderText="Advertiser" DataField="Advertiser" ItemStyle-Width="180px"/>
                                <%--6--%><asp:BoundField HeaderText="Feature" ItemStyle-BackColor="Plum" DataField="feature" ItemStyle-Width="180px"/>
                                <%--7--%><asp:BoundField HeaderText="Size" ItemStyle-BackColor="Yellow" DataField="size" ItemStyle-Width="30px"/>
                                <%--8--%><asp:BoundField HeaderText="Orig. Price" DataField="price"/>
                                <%--9--%><asp:BoundField HeaderText="Price" DataField="rl_price" ItemStyle-Width="50px"/>
                                <%--10--%><asp:BoundField HeaderText="Rep" DataField="rep" ItemStyle-Width="70px"/>
                                <%--11--%><asp:BoundField HeaderText="List Gen" DataField="list_gen" ItemStyle-Width="70px"/>
                                <%--12--%><asp:BoundField HeaderText="Added By" DataField="rl_stat" ItemStyle-Width="180px"/>
                                <%--13--%><asp:BoundField HeaderText="Invoice" DataField="invoice"/>
                            </Columns>
                        </asp:GridView>  
                        <%--End Finance Gridview--%>
                    </td>
                </tr>
            </table>
            <%--End Main Table--%>  
        <hr />
    </div>
    
    <div style="display:none;">
        <asp:Label runat="server" ID="lbl_reset_col_info" ForeColor="Black"/>
        <asp:Label runat="server" ID="lbl_email_mag_links_info" ForeColor="Black"/>
        <asp:Label runat="server" ID="lbl_edit_all_info" ForeColor="Black"/> 
        <asp:Label runat="server" ID="lbl_edit_links_info" ForeColor="Black"/> 
        <asp:Label runat="server" ID="lbl_group_by_mag_info" ForeColor="Black"/> 
        <asp:Button id="btn_export" runat="server" OnClick="ExportGridView"/>
        <asp:DropDownList runat="server" ID="dd_friendlynames" Visible="false"/>
        <asp:HiddenField ID="hf_export_argument" runat="server"/>
        <asp:HiddenField ID="hf_close_win_msg" runat="server"/>
    </div>
    
    <script type="text/javascript">
        function clearSearchBook() {
            grab('<%= tb_s_bp.ClientID %>').value = "";
            grab('<%= tb_s_pageno.ClientID %>').value = "";
            grab('<%= tb_s_invoice.ClientID %>').value = "";
            grab('<%= tb_s_channel.ClientID %>').value = "";
            grab('<%= tb_s_info.ClientID %>').value = "";
            grab('<%= tb_s_price.ClientID %>').value = "";
            grab('<%= tb_s_feature.ClientID %>').value = "";
            grab('<%= tb_s_advertiser.ClientID %>').value = "";
            grab('<%= dd_s_q.ClientID %>').value = "";
            grab('<%= dd_s_datepaidmonths.ClientID %>').style.display = "none";
            grab('<%= dd_s_dateaddedmonths.ClientID %>').style.display = "none";
            grab('<%= dd_s_datetypepaid.ClientID %>').value = "";
            grab('<%= dd_s_datetypeadded.ClientID %>').value = "";
            grab('<%= dd_s_size.ClientID %>').value = "";
            $find("<%= dp_s_datepaidto.ClientID %>").clear();
            $find("<%= dp_s_dateaddedfrom.ClientID %>").clear();
            $find("<%= dp_s_dateaddedto.ClientID %>").clear();
            $find("<%= dp_s_datepaidfrom.ClientID %>").clear();
            $find("<%= dp_s_datepaid.ClientID %>").clear();
            $find("<%= dp_s_dateadded.ClientID %>").clear();
            grab('ctl00_Body_dp_s_datepaid_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_dateadded_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_datepaidto_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_datepaidfrom_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_dateaddedto_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_dateaddedfrom_wrapper').style.display = "none";
            grab('<%= cb_search_onlyDeleted.ClientID %>').checked = false;
            grab('<%= cb_search_onlyThisYear.ClientID %>').checked = true;
            return false;
        }
        function dateSearchType(datetype) {
            var dd = grab('Body_dd_s_datetype' + datetype);
            grab('ctl00_Body_dp_s_date' + datetype + 'to_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_date' + datetype + 'from_wrapper').style.display = "none";
            grab('ctl00_Body_dp_s_date' + datetype + '_wrapper').style.display = "none";
            grab('Body_dd_s_date' + datetype + 'months').style.display = "none";
            if (dd.value == 'Cal. Month') {
                grab('Body_dd_s_date' + datetype + 'months').style.display = "block";
            }
            else if (dd.value == 'Between') {
                grab('ctl00_Body_dp_s_date' + datetype + 'to_wrapper').style.display = "block";
                grab('ctl00_Body_dp_s_date' + datetype + 'from_wrapper').style.display = "block";
            }
            else if (dd.value == 'Date') {
                grab('ctl00_Body_dp_s_date' + datetype + '_wrapper').style.display = "block";
            }
        }
        function editSummary() {
            grab('<%= lb_cancelBookData.ClientID %>').style.display = "block";
            grab('<%= lb_updateBookData.ClientID %>').style.display = "block";
            grab('<%= lb_editBookData.ClientID %>').style.display = "none";
            grab('<%= tb_bookDaysLeft.ClientID %>').style.display = "block";
            grab('<%= div_dp_bookStartDate.ClientID %>').style.display = "block";
            grab('<%= div_dp_bookEndDate.ClientID %>').style.display = "block";
            $find("<%= dp_bookStartDate.ClientID %>").clear();
            $find("<%= dp_bookEndDate.ClientID %>").clear();
            grab('<%= tb_bookDaysLeft.ClientID %>').value = "";
            return false;
        }
        function cancelSummary() {
            grab('<%= lb_cancelBookData.ClientID %>').style.display = "none";
            grab('<%= lb_updateBookData.ClientID %>').style.display = "none";
            grab('<%= lb_editBookData.ClientID %>').style.display = "block";
            grab('<%= tb_bookDaysLeft.ClientID %>').style.display = "none";
            grab('<%= div_dp_bookStartDate.ClientID %>').style.display = "none";
            grab('<%= div_dp_bookEndDate.ClientID %>').style.display = "none";
            grab('<%= val_tb_bookDaysLeft.ClientID %>').style.display = "none";
            return false;
        }
        function ExpOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                var argBox = grab("<%= hf_export_argument.ClientID %>");
                argBox.innertext = data;
                argBox.value = data;
                if (argBox.innertext != "") {
                    var button = grab("<%= btn_export.ClientID %>");
                    button.click();
                }
                else { alert("Please select at least one column!"); }
            }
        }
        function RadWindowClientClose(sender, args) {
            var data = args.get_argument();
            if (data || sender.rebind == true) {
                if(data){
                    grab("<%= hf_close_win_msg.ClientID %>").value = data; }
                refresh();
                return true;
            }
        }
        function refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.click();
            return true;
        }
        var hidden = false;
        function toggleColumn(colname) {
            if (colname != "") {
                grab("<%= lb_resetview.ClientID %>").style.display = '';
                if (!hidden)
                {
                    alert('You are about to hide the ' + colname + ' column.\n\nTo show all columns again click \'Reset Columns\' above the book or refresh the page.');
                    hidden = true;
                }
                rows = grab("<%= gv_s.ClientID %>").rows;
                var colidx = 0;
                var text;

                for (i = 0; i < rows[0].cells.length; i++) {
                    if (navigator.userAgent.indexOf("Firefox") != -1) {
                        if (rows[0].cells[i].children[0] != null) {
                            text = rows[0].cells[i].children[0].innerHTML;
                        }
                    }
                    else {
                        text = rows[0].cells[i].innerText;
                    }
                    if (text == colname) {
                        colidx = i;
                        break;
                    }
                }
                var showhide = "";
                if (rows[0].cells[colidx].style.display == "") {
                    showhide = "none";
                }
                for (i = 0; i < rows.length; i++) {
                    rows[i].cells[colidx].style.display = showhide;
                }
            }
            return false;
        }
        function resetColumns() {
            grab("<%= lb_resetview.ClientID %>").style.display = 'none';
            rows = grab("<%= gv_s.ClientID %>").rows;

            for (i = 0; i < rows[0].cells.length; i++) {
                if (rows[0].cells[i].style.display == "none") {
                    for (j = 0; j < rows.length; j++) {
                        rows[j].cells[i].style.display = "";
                    }
                }
            }
            return false;
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>