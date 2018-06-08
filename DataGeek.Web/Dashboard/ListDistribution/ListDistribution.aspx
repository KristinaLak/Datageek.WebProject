<%--
Author   : Joe Pickering, 23/10/2009 - re-written 18/08/2011 for MySQL
For      : BizClik Media - DataGeek Project
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: List Distribution" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="ListDistribution.aspx.cs" Inherits="ListDistribution" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="/UserControls/OfficeToggler.ascx" TagName="OfficeToggler" TagPrefix="cc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_pros" runat="server" ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="imbtn_export"/>
    </Triggers>
    <ContentTemplate>
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="True" ShowContentDuringLoad="false">
        <Windows>
            <telerik:RadWindow ID="win_assign" runat="server" Title="&nbsp;Assign to Rep" Behaviors="Close, Move, Pin" OnClientClose="ListAssignedOnClientClose" NavigateUrl="LDAssign.aspx"/>
            <telerik:RadWindow ID="win_movetoissue" runat="server" Title="&nbsp;Move to Issue" Behaviors="Close, Move, Pin" OnClientClose="ListTransferredOnClientClose" NavigateUrl="LDMoveToIssue.aspx"/>
            <telerik:RadWindow ID="win_movealltoissue" runat="server" Title="&nbsp;Move All Lists to Issue" Behaviors="Close, Move, Pin" OnClientClose="ListTransferredOnClientClose" NavigateUrl="LDMoveAllListsToIssue.aspx"/>
            <telerik:RadWindow ID="win_newlist" runat="server" Title="&nbsp;New List" OnClientClose="NewListOnClientClose" Behaviors="Move, Close, Pin"/>
            <telerik:RadWindow ID="win_editlist" runat="server" Title="&nbsp;Edit List" OnClientClose="EditListOnClientClose" Behaviors="Move, Close, Pin"/>
            <telerik:RadWindow ID="win_newissue" runat="server" Title="&nbsp;New Issue" Behaviors="Move, Close, Resize, Pin" OnClientClose="NewIssueOnClientClose" NavigateUrl="LDNewIssue.aspx"/> 
            <telerik:RadWindow ID="win_setcolour" runat="server" Title="&nbsp;Set List Colour" VisibleTitlebar="true" VisibleStatusbar="false" Behaviors="Move, Close, Pin" OnClientClose="GenericOnClientClose" NavigateUrl="LDSetColour.aspx"/>
            <telerik:RadWindow ID="win_featoverview" runat="server" Title="&nbsp;Feature Overview" VisibleTitlebar="true" VisibleStatusbar="false" Behaviors="Move, Close, Pin" OnClientClose="GenericOnClientClose" NavigateUrl="FeatureOverview.aspx"/>
        </Windows>
    </telerik:RadWindowManager> 
   
    <div id="div_page" runat="server" class="wider_page">   
    <hr />
    
        <table width="99%" style="position:relative; left:5px; top:-2px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="List" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Distribution" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        <table border="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt;">
            <tr>
                <td valign="top">
                    <%--Add New Issue Button--%>
                    <asp:ImageButton ID="imbtn_new_issue" Visible="true" alt="New Issue" runat="server" Height="26" Width="26" ImageUrl="~\images\icons\listdist_addnewissue.png" 
                    OnClientClick="try{ radopen(null, 'win_newissue'); }catch(E){ IE9Err(); } return false;" style="position:relative; left:-2px; float:left;"/> 
                    <cc:OfficeToggler ID="ot" runat="server" Top="8" Left="4"/>
                    <table border="0" width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td valign="top" align="left">                 
                                <table border="1" cellpadding="0" cellspacing="0" width="392px" bgcolor="White">
                                    <tr>
                                        <td colspan="2" style="border-right:0;">
                                            <img src="/Images/Misc/titleBarLong.png" alt="Issue"/> 
                                            <img src="/Images/Icons/button_ListDistributionInput.png" alt="Issue" height="20px" width="20px" style="position:relative; top:-1px; left:-2px;"/>
                                            <asp:Label runat="server" Text="Office/Issue" ForeColor="White" style="position:relative; top:-0px; left:-232px; top:-6px;"/>
                                        </td>
                                        <td align="center" style="border-left:0;"><asp:ImageButton id="RefreshIssueButton" runat="server" alt="Refresh Issue" OnClick="BindData" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png"/></td>
                                    </tr>
                                    <tr>
                                        <td align="center">
                                            <asp:ImageButton ID="leftButton" ToolTip="Previous Issue" height="22" OnClick="PrevIssue" ImageUrl="~\images\Icons\dashboard_LeftGreenArrow.png" runat="server" Text="Previous Issue" />  
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="dd_office" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="ChangeOffice"/>
                                            <asp:DropDownList ID="dd_issue" runat="server" Enabled="false" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindData"/> 
                                            <asp:ImageButton ID="toggleOrderImageButton" ToolTip="Toggle New to Old" height="18" width="18" ImageUrl="~\images\Icons\dashboard_NewtoOld.png" OnClick="ToggleDateOrder" runat="server" style="position:relative; left:2px;"/> 
                                            <asp:LinkButton ID="lb_edit_issue" runat="server" Text="Edit Issue" ForeColor="Gray" OnClientClick="return showHide('Body_editIssuePanel');"/>
                                        </td>
                                        <td align="center">
                                            <asp:ImageButton ID="rightButton" ToolTip="Next Issue" height="22" OnClick="NextIssue" ImageUrl="~\images\Icons\dashboard_RightGreenArrow.png" runat="server" Text="Next Issue" />  
                                        </td>
                                    </tr>
                                </table>

                                <%--Added Today Table--%>
                                <table runat="server" ID="tbl_addedToday" visible="false" border="1" width="392" cellpadding="1" cellspacing="0" bgcolor="White" style="margin-top:10px;"> 
                                    <tr>
                                        <td colspan="3" valign="top" style="border-right:0"> 
                                            <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-1px; left:-1px;"/>
                                            <img src="/Images/Icons/listDist_Issue.png" height="18px" width="18px" style="position:relative;"/>
                                            <asp:Label Text="Added Today" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-192px;"/>
                                        </td>
                                    </tr>
                                    <tr><td>Lists Added Today</td><td><asp:Label ID="listsAddedTodayLabel" Text="-" runat="server" /></td></tr>
                                    <tr><td>Made Working Today</td><td><asp:Label ID="listsMadeWorkingTodayLabel" Text="-" runat="server" /></td></tr>
                                </table>

                                <%--Colour Key--%>
                                <table ID="tbl_key" runat="server" visible="false" border="1" width="392" cellpadding="1" cellspacing="0" bgcolor="White" style="margin-top:11px;"> 
                                    <tr>
                                        <td colspan="3" valign="top" style="border-right:0"> 
                                            <img src="/images/misc/titlebaralpha.png" style="position:relative; top:-1px; left:-1px;"/>
                                            <img src="/images/icons/listdist_issue.png" height="18px" width="18px" style="position:relative;"/>
                                            <asp:Label Text="Key" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-192px;"/>
                                        </td>
                                    </tr>
                                    <tr><td><b>Closed</b></td><td bgcolor="#00FF00"/></tr>
                                    <tr><td><b>Blown</b></td><td bgcolor="Red"/></tr>
                                    <tr><td><b>Underwrite</b></td><td bgcolor="Yellow"/></tr>
                                </table>
                            </td>
                            <td valign="top" width="400" align="center">
                                <%--Summary Area--%>             
                                <asp:Panel ID="summaryPanel" runat="server" Visible="false">
                                    <table border="1" width="360px" cellpadding="0" cellspacing="0" bgcolor="White" style="text-align:left;">   
                                        <tr>
                                            <td align="left" colspan="3" style="border-right:0;">
                                                <img src="/Images/Misc/titleBarAlpha.png"/>
                                                <img src="/Images/Icons/dashboard_PencilAndPaper.png" height="20px" width="20px" style="position:relative;"/>
                                                <asp:Label ID="SummaryLabel" Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Pink">Lists In Waiting Above 15 Names</td>
                                            <td bgcolor="Pink" align="center"><asp:Label ID="listsWaiting15PlusLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                            <td bgcolor="Pink" align="center"><asp:Label ID="listsWaiting15PlusValueLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Azure">Lists Being Worked Above 15 Names</td>
                                            <td bgcolor="Azure" align="center"><asp:Label ID="listsWorking15PlusLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                            <td bgcolor="Azure" align="center"><asp:Label ID="listsWorking15PlusValueLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Pink">Lists In Waiting Below 15 Names</td>
                                            <td bgcolor="Pink" align="center"><asp:Label ID="listsWaiting15MinusLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                            <td bgcolor="Pink" align="center"><asp:Label ID="listsWaiting15MinusValueLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Azure">Lists Being Worked Below 15 Names</td>
                                            <td bgcolor="Azure" align="center"><asp:Label ID="listsWorking15MinusLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                            <td bgcolor="Azure" align="center"><asp:Label ID="listsWorking15MinusValueLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                         <tr>
                                            <td bgcolor="#ffff99"> Total Worked</td>
                                            <td colspan="2" bgcolor="#ffff99" align="center"><asp:Label ID="totalWorkedLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99">Total Waiting</td>
                                            <td colspan="2" bgcolor="#ffff99" align="center"><asp:Label ID="totalWaitingLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99">Total List Value Overall</td>
                                            <td colspan="2" bgcolor="#ffff99" align="center"><asp:Label ID="totalListsValue" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99">Total Lists in Room</td>
                                            <td colspan="2" bgcolor="#ffff99" align="center"><asp:Label ID="totalListsInRoomLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99"><asp:Label runat="server" Text="Total Suppliers + M&O Names"/></td>
                                            <td colspan="2" bgcolor="#FAEBD7" align="center"><asp:Label ID="totalSuppliersAndMaOLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99"><asp:Label runat="server" Text="Suppliers/M&O Names Total"/></td>
                                            <td colspan="2" bgcolor="#FAEBD7" align="center"><asp:Label ID="individualTotalLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="#ffff99"><asp:Label runat="server" Text="Suppliers/M&O Names Ratio"/></td>
                                            <td colspan="2" bgcolor="#FAEBD7" align="center"><asp:Label ID="supMaoPercentageLabel" Text="0" runat="server" ForeColor="Black" Font-Size="9pt"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                            <td valign="top" align="right"> <%--Console/Log Area--%>
                                <table id="tbl_log" runat="server" border="1" cellpadding="1" cellspacing="0" bgcolor="White">
                                    <tr><td align="left">
                                        <img alt="Title" src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-1px; left:-1px;"/> 
                                        <asp:Label ID="tb_consoleTitle" Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-170px;"/>
                                    </td></tr>
                                    <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="MultiLine" Height="175px" Width="486px"/></td></tr>
                                </table>
                            </td>           
                        </tr>
                        <tr>
                            <td colspan="3" height="15">
                                 <%--Edit Issue Area--%>
                                <asp:Panel ID="editIssuePanel" runat="server" ForeColor="White" style="display:none; font-family:Verdana; font-size:8pt; padding-left:30px; position:relative; top:-20px;">
                                    <asp:ImageButton id="cancelEditIssueLinkButton" alt="Cancel" runat="server" Height="19" Width="19" ImageUrl="~\Images\Icons\dashboard_Close.png" OnClientClick="return showHide('Body_editIssuePanel');"/>
                                    <table cellpadding="4"><tr><td>
                                        <asp:Table runat="server" width="270px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="left">
                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Edit Issue" style="position:relative; top:-0px; left:-0px;"/>
                                                    <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Edit Issue" height="20px" width="20px" style="position:relative;"/>
                                                    <asp:Label ID="PREditIssueLabel" Text="Edit Issue" runat="server" style="position:relative; top:-6px; left:-193px; color:#ffffff;"/>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                            <asp:TableRow>
                                                <asp:TableCell>
                                                    <%--Edit Issue Input Boxes  --%>  
                                                    <asp:Table HorizontalAlign="Left" ID="editIssueInputTable" runat="server" Border="0" CellPadding="1" 
                                                        GridLines="horizontal" Font-Name="Verdana" Font-Size="8pt" Height="16px">
                                                        <asp:TableRow>
                                                            <asp:TableCell BackColor="#444444">
                                                                Change Issue Name
                                                                <br/><asp:Label ID="lbl_issue_name" runat="server" ForeColor="DarkOrange"/>
                                                            </asp:TableCell>
                                                            <asp:TableCell BackColor="#444444">
                                                                Change Start Date
                                                                <br/><asp:Label ID="lbl_start_date" runat="server" ForeColor="DarkOrange"/>
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow>
                                                            <asp:TableCell BackColor="#444444" VerticalAlign="Top">
                                                                <asp:TextBox ID="changeNameTextBox" runat="server" Width="140px" ReadOnly="true" BackColor="LightGray"/>
                                                            </asp:TableCell>
                                                            <asp:TableCell BackColor="#444444" VerticalAlign="Top">
                                                                <telerik:RadDatePicker ID="changeIssueStartDateBox" runat="server" Width="140px" AutoPostBack="false">
                                                                    <Calendar runat="server">
                                                                        <SpecialDays>
                                                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                                                        </SpecialDays>
                                                                    </Calendar>
                                                                </telerik:RadDatePicker>
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow>
                                                            <asp:TableCell BackColor="#444444" VerticalAlign="Middle" HorizontalAlign="Left">
                                                                <asp:Button ID="changeNameButton"  runat="server" Text="Update Name" alt="Change Issue Name" OnClick="UpdateIssueName" Enabled="false"/>        
                                                            </asp:TableCell>
                                                            <asp:TableCell BackColor="#444444" VerticalAlign="Middle" HorizontalAlign="Left">
                                                                <asp:Button ID="updateStartDateButton" Text="Update Start Date" alt="Update the issue start date" OnClick="UpdateIssueStartDate" runat="server"/>        
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                    </asp:Table>
                                                </asp:TableCell>                   
                                            </asp:TableRow>  
                                        </asp:Table></td></tr>
                                    </table>
                                </asp:Panel>  
                            </td>
                        </tr>
                    </table>
                    <div ID="div_no_lists" runat="server" visible="false" style="text-align:center; height:40px;"><asp:Label runat="server" Text="There are no Lists in this Issue yet." CssClass="MediumTitle" style="color:gray;"/></div>
                    <table ID="tbl_hdr" runat="server" width="100%" border="0" cellpadding="0" cellspacing="0" style="margin-top:15px;">
                        <tr>
                            <td align="left" valign="bottom">
                                <table cellpadding="0" cellspacing="0" style="margin-left:2px;">
                                    <tr>
                                        <td>                                
                                            <asp:ImageButton ID="imbtn_new_list" alt="New List" runat="server" Visible="false" Height="26" Width="26" ImageUrl="~\Images\Icons\listDist_AddNewList.png" OnClientClick="try{ radopen(null, 'win_newlist'); }catch(E){ IE9Err(); } return false;" style="position:relative; top:-2px;"/> 
                                            <asp:ImageButton ID="imbtn_print" alt="View Printer-Friendly Version" runat="server" Height="26" Width="22" ImageUrl="~\Images\Icons\salesBook_PrinterFriendlyVersion.png" OnClick="PrintGridView" style="position:relative; left:-1px; top:-2px;"/>
                                            <asp:ImageButton ID="imbtn_export" alt="Export to Excel" runat="server" Height="23" Width="23" ImageUrl="~\Images\Icons\salesBook_ExportToExcel.png" OnClick="ExportToExcel" style="position:relative; left:-3px; top:-2px;"/>
                                        </td>
                                        <td>
                                            <asp:Panel ID="pnl_company_search" runat="server" DefaultButton="btn_search" style="position:relative; top:-1px;">
                                                <asp:TextBox ID="tb_search_company" runat="server"/>
                                                <asp:DropDownList ID="dd_search_territory" runat="server" />
                                                <asp:DropDownList ID="dd_search_sector" runat="server" />
                                                <asp:DropDownList ID="dd_search_timescale" runat="server">
                                                    <asp:ListItem Text="All Time"/>
                                                    <asp:ListItem Text="2 years" Value="-24"/>
                                                    <asp:ListItem Text="1 year" Value="-12"/>
                                                    <asp:ListItem Text="9 months" Value="-9"/>
                                                    <asp:ListItem Text="6 months" Value="-6"/>
                                                    <asp:ListItem Text="3 months" Value="-3"/>
                                                </asp:DropDownList>
                                                <asp:Button ID="btn_search" runat="server" OnClick="CompanySearch" Text="Search"/> 
                                                <asp:Button ID="btn_end_search" runat="server" OnClick="ResetSearch" Text="End Search" Visible="false"/>
                                                <asp:Label ID="lbl_search_results" runat="server" ForeColor="DarkOrange" />
                                                <ajax:TextBoxWatermarkExtender runat="server" TargetControlID="tb_search_company" WatermarkText="Enter company name" WatermarkCssClass="watermark"/>
                                            </asp:Panel>
                                        </td>
                                    </tr>
                                </table>                
                            </td>
                            <td align="right">
                                <asp:Button ID="editAllButton" Text="Edit All" runat="server" OnClick="ToggleEditAll" Visible="false"/>
                                <asp:Button ID="cancelEditAllButton" Visible="false" Text="Cancel" runat="server" OnClick="CancelEditAll"/>
                            </td>
                        </tr>
                    </table>
                    
                    <%--Dynamic Grid Area--%>
                    <div id="div_dynamic_gv" runat="server" style="width:1276px; overflow-x:auto; overflow-y:hidden; margin-left:auto; margin-right:auto;"/>
                    <asp:Panel runat="server" Visible="false" ID="staticGridViewPanel">
                        <asp:GridView ID="gv_lists" runat="server" 
                            border="2" Width="1277" RowStyle-CssClass="gv_hover"
                            AllowSorting="True" CssClass="BlackGridHead"
                            OnRowDeleting="staticGridView_RowDeleting"
                            OnSorting="staticGridView_Sorting"
                            OnRowDataBound="staticGridView_RowDataBound"
                            AllowAdding="True" Font-Name="Verdana"
                            Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2"
                            HeaderStyle-HorizontalAlign="Center"
                            RowStyle-HorizontalAlign="Center"
                            AutoGenerateColumns="False">
                            <Columns>
                                <asp:CommandField CausesValidation="true" ItemStyle-HorizontalAlign="Center" ItemStyle-BackColor="White" 
                                    ShowEditButton="true" ShowDeleteButton="false" ButtonType="Image"
                                    EditImageUrl="~\images\icons\gridview_edit.png"
                                    CancelImageUrl="~\images\icons\gridview_canceledit.png"
                                    UpdateImageUrl="~\images\icons\gridview_update.png"
                                    ItemStyle-Width="15px"
                                    HeaderText="">
                                </asp:CommandField> 
                                <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-BackColor="White" ItemStyle-Width="15">
                                    <ItemTemplate>
                                    <asp:ImageButton ID="DeleteButton" runat="server"
                                         ImageUrl="~\images\icons\gridview_delete.png"
                                         CommandName="Delete" ToolTip="Cancel/Restore"
                                         OnClientClick="return confirm('Are you sure you wish to cancel/restore this list?')"/>                      
                                    </ItemTemplate>
                                </asp:TemplateField>  
                                <asp:BoundField DataField="ListID"/>
                                <asp:BoundField DataField="ListIssueID"/>
                                <asp:TemplateField HeaderText="With Admin" SortExpression="WithAdmin" ItemStyle-Width="33px">
                                  <ItemTemplate>
                                    <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="UpdateWithAdmin" Checked='<%# Server.HtmlEncode(Eval("WithAdmin").ToString()).Equals("1") %>'/>
                                  </ItemTemplate>
                                </asp:TemplateField> 
                                <asp:TemplateField HeaderText="Quick Ready" ItemStyle-Width="33px">
                                  <ItemTemplate>
                                    <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="UpdateReady" Checked='<%# Server.HtmlEncode(Eval("IsReady").ToString()).Equals("1") %>'/> 
                                  </ItemTemplate>
                                </asp:TemplateField> 
                                <asp:TemplateField HeaderText="Status" SortExpression="ListStatus" ItemStyle-Width="180px" ControlStyle-Width="180px">
                                    <ItemTemplate>
                                        <asp:Label runat="server" Text='<%# Server.HtmlEncode(Eval("ListStatus").ToString()) %>'/>
                                    </ItemTemplate>
                                </asp:TemplateField>              
                                <asp:TemplateField HeaderText="Company Name" SortExpression="CompanyName" ItemStyle-Width="225px" ControlStyle-Width="225px">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lb_company_name" runat="server" Text='<%# Server.HtmlEncode(Eval("CompanyName").ToString()) %>' ToolTip='<%# Server.HtmlEncode(Eval("ListNotes").ToString()) %>' ForeColor="Black"/>
                                    </ItemTemplate>
                                </asp:TemplateField>          
                                <asp:TemplateField HeaderText="List Gen" SortExpression="ListGeneratorFriendlyname" ItemStyle-Width="109px" ControlStyle-Width="109px">
                                    <ItemTemplate>
                                        <asp:Label runat="server" Text='<%# Server.HtmlEncode(Eval("ListGeneratorFriendlyname").ToString()) %>'/>
                                    </ItemTemplate>
                                </asp:TemplateField>                                 
                                <asp:BoundField HeaderText="List Out" SortExpression="DateAdded" DataField="DateAdded" DataFormatString="{0:dd/MM/yyyy}" HtmlEncode="false" ItemStyle-Width="80px" ControlStyle-Width="80px"/>
                                <asp:TemplateField HeaderText="REP WORKING" SortExpression="ListWorkedByFriendlyname" HeaderStyle-Font-Size="11pt" ItemStyle-Width="142px" ControlStyle-Width="142px">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_worked_by" runat="server" Text='<%# Server.HtmlEncode(Eval("ListWorkedByFriendlyname").ToString()) %>'/>
                                    </ItemTemplate>
                                </asp:TemplateField>    
                                <asp:BoundField HeaderText="Suppliers" SortExpression="Suppliers" DataField="Suppliers" ItemStyle-Width="66px" ControlStyle-Width="54px"/>
                                <asp:BoundField HeaderText="M&O Names" SortExpression="MaONames" DataField="MaONames" ItemStyle-Width="66px" ControlStyle-Width="54px"/>
                                <asp:BoundField HeaderText="Annual Sales" SortExpression="Turnover" DataField="Turnover" ItemStyle-Width="105px" ControlStyle-Width="95px"/>
                                <asp:BoundField HeaderText="No. Emps" SortExpression="Employees" DataField="Employees" ItemStyle-Width="60px" ControlStyle-Width="60px"/>  
                                <asp:TemplateField HeaderText="Channel" Visible="false" SortExpression="Industry" ItemStyle-Width="88px" ControlStyle-Width="90px"> <%--not used but don't delete--%>
                                    <ItemTemplate>
                                        <asp:Label runat="server" Text='<%# Server.HtmlEncode(Eval("Industry").ToString()) %>'/>
                                    </ItemTemplate>
                                </asp:TemplateField>                 
                     <%--16--%> <asp:TemplateField HeaderText="Assign" Visible="True" ItemStyle-BackColor="White" ItemStyle-Width="45">
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server" ImageUrl="~\images\icons\gridview_tocca.png" Width="16" Height="16" Text="Move" ToolTip="Move to Rep"/>        
                                        <asp:ImageButton ID="imbtn_move_to_issue" runat="server" ImageUrl="~\images\icons\gridview_changeissue.png" Width="16" Height="16" ToolTip="Move to Issue"/>  
                                    </ItemTemplate>
                                </asp:TemplateField>  
                     <%--17--%> <asp:BoundField DataField="IsCancelled"/>
                     <%--18--%> <asp:BoundField DataField="IsCancelled"/> <%--dummy--%>
                     <%--19--%> <asp:TemplateField HeaderText="PC" ItemStyle-Width="10px">
                                  <ItemTemplate>
                                    <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="UpdateParachute" Checked='<%# Server.HtmlEncode(Eval("Parachute").ToString()).Equals("1") %>' /> 
                                  </ItemTemplate>
                                </asp:TemplateField> 
                     <%--20--%> <asp:TemplateField HeaderText="Sy" ItemStyle-Width="10px">
                                  <ItemTemplate>
                                    <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="UpdateSynopsis" Checked='<%# Server.HtmlEncode(Eval("Synopsis").ToString()).Equals("1") %>' /> 
                                  </ItemTemplate>
                                </asp:TemplateField> 
                     <%--21--%> <asp:TemplateField Visible="false" ItemStyle-Width="10px" ItemStyle-BackColor="White"> <%-- no longer used --%> 
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server" Visible="false" ImageUrl="~\images\icons\gridview_delete.png" Text="Delete" ToolTip="Permanently Delete" CommandName="PermDelete"
                                         OnClientClick="return confirm('Are you sure you wish to delete this entry?\nThe entry will be removed from the database and cannot be recovered.')"/>                
                                    </ItemTemplate>
                                </asp:TemplateField>
                     <%--22--%> <asp:BoundField HeaderText="G" DataField="Grade" SortExpression="Grade"/>
                     <%--23--%> <asp:BoundField DataField="CompanyID"/>
                            </Columns>
                            <EmptyDataTemplate>&nbsp;</EmptyDataTemplate>
                            <HeaderStyle BackColor="#444444" ForeColor="White"></HeaderStyle>
                            <RowStyle BackColor="White"></RowStyle>
                        </asp:GridView>
                    </asp:Panel>                                  
                </td>
            </tr>
        </table>
        <asp:Button ID="btn_move_all_waiting_lists" runat="server" Visible="false" Text="Move All Waiting Lists" style="position:relative; left:7px;"/>
        <hr/>
    </div>
    
    <asp:DropDownList ID="dd_rep_colours" runat="server" Visible="false"/>
    <asp:HiddenField ID="hf_assigned_list" runat="server"/>
    <asp:HiddenField ID="hf_transferred_list" runat="server"/>
    <asp:HiddenField ID="hf_new_list" runat="server"/>
    <asp:HiddenField ID="hf_edit_list" runat="server"/>
    <asp:HiddenField ID="hf_new_issue" runat="server"/>
    
    <script type="text/javascript">
        function showHide(id) {
            obj = grab(id);
            if (obj.style.display == "none") {
                obj.style.display = "block";
            }
            else {
                obj.style.display = "none";
            }
            return false;
        }
        var row = null;
        function NavigateCell(key, gridViewID) {
            var grid = grab(gridViewID);
            // When in full edit mode
            if (key.keyCode == 38 || key.keyCode == 40)
            {
                if (key.keyCode == 38) // Up
                {
                    if (row > 0) { row--; }
                }
                else if (key.keyCode == 40) // Down
                {
                    if (row < grid.rows.length - 3)
                    row++;
                }
                try {
                  var gridRow = grid.rows[(row+1)];
                  var gridCell = gridRow.cells[0];
                  gridCell.children[0].focus();
                }
                catch (E) { alert(E); }
            }
        }
        function SelectCell(thisCell, RowIndex) {
            row = RowIndex;
        }
        function refresh() {
            var button = grab("<%= RefreshIssueButton.ClientID %>");
            button.click();
            return true;
        }
        function ListAssignedOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_assigned_list.ClientID %>").value = data;
                alert(data);
                refresh();
            }
        }
        function ListTransferredOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_transferred_list.ClientID %>").value = data;
                alert(data);
                refresh();
            }
        }
        function NewListOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_list.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function EditListOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_edit_list.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function NewIssueOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_issue.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function GenericOnClientClose(sender, args) {
            refresh();
            return true;
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>