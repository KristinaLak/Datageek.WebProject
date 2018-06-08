<%--
Author   : Joe Pickering, 24/10/2011
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Finance" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Finance.aspx.cs" Inherits="Finance" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_fin" runat="server" ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="lb_export"/>
        <asp:PostBackTrigger ControlID="btn_export_due"/>
    </Triggers>
    <ContentTemplate>
    <telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusBar="false" Skin="Black" Behaviors="Move, Pin, Close" OnClientAutoSizeEnd="CenterRadWindow" AutoSize="True" ShowContentDuringLoad="false">
        <Windows>
            <telerik:RadWindow runat="server" ID="win_export" Title="&nbsp;Accounts Data Export" NavigateUrl="FNExport.aspx"/>
            <telerik:RadWindow runat="server" ID="win_newtab" Title="&nbsp;New Tab" OnClientClose="NewTabOnClientClose" NavigateUrl="FNNewTab.aspx"/>  
            <telerik:RadWindow runat="server" ID="win_newliab" Title="&nbsp;New Liability" OnClientClose="NewLiabOnClientClose" NavigateUrl="FNNewLiab.aspx"/>
            <telerik:RadWindow runat="server" ID="win_editliab" Title="&nbsp;Edit Liability" OnClientClose="EditLiabOnClientClose"/>
            <telerik:RadWindow runat="server" ID="win_newreminder" Title="&nbsp;New Reminder" NavigateUrl="FNNewReminder.aspx"/> 
            <telerik:RadWindow runat="server" ID="win_reopenaccount" Title="&nbsp;Re-open Account" OnClientClose="ReOpenAccOnClientClose" NavigateUrl="FNReOpenAccount.aspx"/> 
            <telerik:RadWindow runat="server" ID="win_edittab" Title="&nbsp;Edit Tab" OnClientClose="EditTabOnClientClose" NavigateUrl="FNEditTab.aspx"/> 
            <telerik:RadWindow runat="server" ID="win_editsale" Title="&nbsp;Account Details" OnClientClose="EditSaleOnClientClose" NavigateUrl="FNEditSale.aspx"/> 
            <telerik:RadWindow runat="server" ID="win_movetotab" Title="&nbsp;Move to Tab" OnClientClose="MoveToTabOnClientClose" NavigateUrl="FNMoveToTab.aspx"/> 
            <telerik:RadWindow runat="server" ID="win_setcolour" Title="&nbsp;Set Colour" VisibleTitlebar="true" VisibleStatusbar="false" OnClientClose="GenericOnClientClose" NavigateUrl="FNSetColour.aspx"/>
            <telerik:RadWindow runat="server" ID="win_setrecur" Title="&nbsp;Set Recurring" VisibleTitlebar="true" VisibleStatusbar="false" OnClientClose="GenericOnClientClose" KeepInScreenBounds="true" NavigateUrl="FNSetRecur.aspx"/>
            <telerik:RadWindow runat="server" ID="win_help" Title="Finance Sales Information" NavigateUrl="HelpWindow.aspx"/>
            <telerik:RadWindow runat="server" ID="win_fnds" Title="Duplicate Daily Summary" OnClientClose="DailySummaryOnClientClose" NavigateUrl="FNSaveDailySummary.aspx"/>
            <telerik:RadWindow runat="server" ID="win_viewsummaries" Title="Daily Summary List" NavigateUrl="FNViewSummaries.aspx"/>
            <telerik:RadWindow runat="server" ID="win_editmediasale" Title="&nbsp;Edit Media Sale Part" OnClientClose="EditMediaSaleOnClientClose"/> 
            <telerik:RadWindow runat="server" ID="win_editlinks" Title="&nbsp;Edit Magazine Links"/>
        </Windows>
    </telerik:RadWindowManager>

    <div id="div_page" runat="server" class="wider_page">   
        <hr />
        <%--Main Table--%>
        <table border="0" width="99%" cellpadding="1" cellspacing="0" style="margin-left:auto; margin-right:auto;"> 
            <tr>
                <%--Page Header--%>
                <td align="left" valign="top" height="25">
                    <asp:Label runat="server" Text="Finance" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
                <td align="right">&nbsp;</td>
                <td align="right">
                    <asp:LinkButton ID="lb_edit_links" runat="server" ForeColor="Silver" Text="Edit Magazine Links" style="position:relative; top:-8px; padding-right:4px; border-right:solid 1px gray;"/>
                    <asp:LinkButton runat="server" ForeColor="Silver" Text="Download Help File" OnClick="DownloadHelpFile" style="position:relative; top:-8px; padding-right:4px; border-right:solid 1px gray;"
                    OnClientClick="return confirm('This will download a PowerPoint help file.\n\nAre you sure?')"/>
                    <asp:LinkButton runat="server" ForeColor="Silver" Text="Data Export" style="position:relative; top:-8px; padding-right:4px; border-right:solid 1px gray;" OnClientClick="try{ radopen(null, 'win_export'); }catch(E){ IE9Err(); } return false;"/>
                    <asp:LinkButton runat="server" ForeColor="Silver" Text="Add Reminder" Visible="true" OnClientClick="try{ radopen(null, 'win_newreminder'); }catch(E){ IE9Err(); } return false;" style="position:relative; top:-8px;"/>
                    <asp:ImageButton ID="btn_info" Visible="true" alt="Info" runat="server" Height="20" Width="20" ImageUrl="~\Images\Icons\dashboard_Info.png" OnClientClick="try{ radopen(null, 'win_help'); }catch(E){ IE9Err(); } return false;" style="position:relative; top:-3px;"/> 
                </td>
            </tr>
            <tr>
                <td valign="top" align="left" height="10">
                    <%--Navigation--%>
                    <table border="1" cellpadding="0" cellspacing="0" width="350px" bgcolor="White">
                        <tr>
                            <td valign="top" align="left" colspan="2" style="border-right:0">
                                <img src="/images/misc/titlebarlong.png"/> 
                                <asp:Label Text="Office/Year" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-208px;"/>
                            </td>
                            <td align="center" style="border-left:0">
                                <asp:ImageButton id="imbtn_refresh" OnClick="CloseSearch" runat="server" Height="21" Width="21" ImageUrl="~\images\icons\dashboard_refresh.png"/>
                            </td>
                        </tr>
                        <tr>
                            <td align="center">
                                <%--Left Button--%> 
                                <asp:ImageButton ID="imbtn_leftNavButton" height="22" Text="Previous Book" ImageUrl="~\images\icons\dashboard_leftgreenarrow.png" runat="server" OnClick="PrevYear"/>  
                            </td> 
                            <td>
                                <%--Office Year--%> 
                                <asp:DropDownList ID="dd_office" runat="server" Width="110px" AutoPostBack="true"/>
                                <asp:DropDownList ID="dd_year" runat="server" Width="100px" AutoPostBack="true"/> 
                            </td>
                            <td align="center">
                                <%--Right Button--%> 
                                <asp:ImageButton ID="imbtn_rightNavButton" height="22" Text="Next Book" ImageUrl="~\images\icons\dashboard_rightgreenarrow.png" runat="server" OnClick="NextYear"/> 
                            </td>  
                        </tr>
                    </table>
                </td>
                <td align="center" valign="top" rowspan="2">
                    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                        <Triggers> 
                            <asp:AsyncPostBackTrigger ControlID="lb_savekey" EventName="Click" /> 
                        </Triggers>
                        <ContentTemplate>
                            <table ID="tbl_key" border="1" runat="server" cellpadding="0" cellspacing="0" bgcolor="white" width="270px" style="position:relative; left:1px;"> 
                                <tr>
                                    <td height="16"> 
                                        <b>&nbsp;Colour&nbsp;</b>
                                    </td>
                                    <td align="left" style="border-right:0;">
                                        <b>&nbsp;Description</b> 
                                    </td>
                                    <td align="right" style="border-left:0;"><asp:LinkButton ID="lb_savekey" runat="server" Text="Save" ForeColor="DarkBlue" Font-Bold="true" OnClick="UpdateColourScheme" style="position:relative; left:-2px;"/></td>
                                </tr> 
                                <tr>
                                    <td bgcolor="#F0E68C"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c1" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#3CB371"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c2" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#ADD8E6"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c3" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#CD5C5C"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c4" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#008080"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c5" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#D8BFD8"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c6" width="200px"/></td>
                                </tr>
                                <tr>
                                    <td bgcolor="#808080"></td>
                                    <td colspan="2" align="center"><asp:TextBox runat="server" ID="tb_c7" width="200px"/></td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td align="right" valign="top" rowspan="2"> 
                    <%--Console--%>
                    <table border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                        <tr>
                            <td align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/dashboard_Log.png" height="20px" width="20px"/>
                                <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                            </td>
                        </tr>
                        <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="166" Width="636px"/></td></tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="top" height="130">
                    <%--Summary--%>
                    <table runat="server" id="tbl_summary" border="1" cellpadding="1" cellspacing="0" width="350" bgcolor="White" height="162">
                        <tr>
                            <td align="left" colspan="4">
                                <asp:Image runat="server" ImageUrl="/Images/Misc/titleBarLong.png" style="position:relative; top:-1px; left:-1px;"/> 
                                <asp:Label Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-208px;"/>
                            </td>
                        </tr>
                        <tr>
                            <td width="35%">Total in Tab</td>
                            <td width="25%"><asp:Label runat="server" ID="lbl_SummaryTabTotal"/></td>
                            <td width="23%">Tab Value</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTabTotalValue"/></td>
                        </tr>
                        <tr>
                            <td>Total Outstanding</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryYearTotalValue"/></td>
                            <td><asp:Label runat="server" Text="Liabilities"/></td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalLiabilitiesValue"/></td>
                        </tr>
                        <tr>
                            <td colspan="3">Waiting for Copy</td>
                            <td bgcolor="red"><asp:Label runat="server" ID="lbl_al_wfc"/></td>
                        </tr>
                        <tr>
                            <td colspan="3">Copy In</td>
                            <td bgcolor="DodgerBlue"><asp:Label runat="server" ID="lbl_al_copyin" ForeColor="White"/></td>
                        </tr>
                        <tr>
                            <td colspan="3">Proof Out</td>
                            <td bgcolor="Orange"><asp:Label runat="server" ID="lbl_al_proofout"/></td>
                        </tr>
                        <tr>
                            <td colspan="3">Own Advert</td>
                            <td bgcolor="Purple"><asp:Label runat="server" ID="lbl_al_ownadvert"/></td>
                        </tr>
                        <tr>
                            <td colspan="3">Approved</td>
                            <td bgcolor="Lime"><asp:Label runat="server" ID="lbl_al_approved"/></td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <asp:LinkButton runat="server" ID="lb_export" ForeColor="DarkBlue" Text="Export Tab to Excel" OnClick="ExportToExcel" />
                                <asp:Label runat="server" ForeColor="DarkOrange" Text=" | " />
                                <asp:LinkButton runat="server" ID="lb_print" ForeColor="DarkBlue" Text="View Printable Version"  OnClick="ViewPrintableVersion" />
                            </td>
                        </tr>
                    </table>

                    <%--End Summary--%>
                </td>
            </tr>
            <tr>
                <td colspan="3">    
                    <%--Tabs--%>
                    <table border="0" width="1280" cellpadding="0" cellspacing="0" style="position:relative; top:4px;">
                        <tr>
                            <td valign="bottom">
                                <table cellpadding="0" cellspacing="0" style="position:relative; top:-4px;">
                                    <tr>
                                        <td valign="bottom">
                                            <br />
                                            <asp:LinkButton ID="lb_newtab" Text="New Tab" runat="server" ForeColor="Silver" OnClientClick="try{ radopen(null, 'win_newtab'); }catch(E){ IE9Err(); } return false;" style="padding-right:4px; border-right:solid 1px gray;"/> 
                                            <asp:LinkButton ID="lb_edittab" ForeColor="Silver" runat="server" Text="Edit Tab" style="padding-right:4px; border-right:solid 1px gray;"/>
                                            <asp:LinkButton ID="lb_deletetab" ForeColor="Silver" runat="server" Text="Delete Tab" OnClick="DeleteSelectedTab" style="padding-right:4px; border-right:solid 1px gray;"
                                            OnClientClick="if(confirm('Are you sure? This will reassign all sales in this tab to the \'In Progress\' tab.')){if(confirm('Are you positive you wish to permanently delete this tab?')){return true;}else{return false;}}else{return false;}"/>
                                            <asp:LinkButton ID="lb_reopen_account" ForeColor="Silver" runat="server" Text="Re-open Account"/>
                                            <asp:LinkButton ID="lb_new_liab" ForeColor="Silver" runat="server" Text="New Liability" style="padding-left:4px; border-left:solid 1px gray;" OnClientClick="try{ radopen(null, 'win_newliab'); }catch(E){ IE9Err(); } return false;"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td align="right">
                                <asp:Panel ID="pnl_search" runat="server" DefaultButton="btn_search_company" style="position:relative; left:1px; top:-8px;">
                                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Search for an advertiser or an invoice number:"/>
                                    <asp:TextBox ID="tb_search_company" runat="server"/>
                                    <asp:Button ID="btn_search_company" runat="server" OnClick="SearchCompanyOrInvoiceNumber" Text="Search" EnableViewState="false"/> 
                                    <asp:Button runat="server" OnClick="CloseSearch" Text="Clear Search"/>
                                    <ajax:TextBoxWatermarkExtender runat="server" TargetControlID="tb_search_company" WatermarkText="Enter company name" WatermarkCssClass="watermark"/>
                                </asp:Panel>
                                <table ID="tbl_due_span" runat="server" visible="false" cellpadding="0" cellspacing="1" style="position:relative; top:-3px;">
                                    <tr>
                                        <td><asp:Label runat="server" Text="Search for unpaid sales added between&nbsp;" ForeColor="DarkOrange" /></td>
                                        <td><telerik:RadDatePicker ID="dp_due_sd" runat="server" width="129px"/></td>
                                        <td><asp:Label runat="server" Text="and&nbsp;" ForeColor="DarkOrange" /></td>
                                        <td><telerik:RadDatePicker ID="dp_due_ed" runat="server" width="129px"/></td>
                                        <td><asp:DropDownList ID="dd_due_offices" runat="server" Width="150"/></td>
                                        <td><asp:DropDownList ID="dd_due_tabs" runat="server" Width="150"/></td>
                                        <td><asp:Button runat="server" Text="Search" OnClick="SearchDue"/></td>
                                        <td><asp:Label runat="server" Text="&nbsp;or&nbsp;" ForeColor="DarkOrange"/></td>
                                        <td><asp:Button ID="btn_export_due" runat="server" Text="Export" OnClick="ExportToExcel"
                                        OnClientClick="return confirm('Are you sure?\n\nPlease be patient, this may take a minute.');"/></td>
                                    </tr>
                                    <tr><td colspan="9"><br/></td></tr>
                                </table>
                                <table ID="tbl_liab_search" runat="server" visible="false" cellpadding="0" cellspacing="1" style="position:relative; top:-3px;">
                                    <tr>
                                        <td><asp:Label runat="server" Text="Search for liabilities between&nbsp;" ForeColor="DarkOrange" /></td>
                                        <td><telerik:RadDatePicker ID="dp_liab_search_from" runat="server" width="129px"/></td>
                                        <td><asp:Label runat="server" Text="and&nbsp;" ForeColor="DarkOrange" /></td>
                                        <td><telerik:RadDatePicker ID="dp_liab_search_to" runat="server" width="129px"/></td>
                                        <td>
                                            <asp:DropDownList ID="dd_liab_search_type" runat="server">
                                                <asp:ListItem>Liabilities</asp:ListItem>
                                                <asp:ListItem>Direct Debits</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td><asp:Button runat="server" Text="Search" OnClick="SearchLiabilities"/></td>
                                        <td>
                                            <asp:Button runat="server" Text="Cancel Search" OnClick="CancelSearchLiabilities"/>
                                            <asp:HiddenField ID="hf_liab_in_search" runat="server" Value="0" />
                                        </td>
                                    </tr>
                                    <tr><td colspan="9"><br/></td></tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <telerik:RadTabStrip ID="tabstrip" runat="server" AutoPostBack="true" MaxDataBindDepth="1" SelectedIndex="0"
                                 BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" Font-Size="Smaller" style="position:relative; top:-2px;"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <%--Div Area--%>
                    <div runat="server" id="div_container">
                        <div runat="server" ID="div_gv"/>
                        <div runat="server" ID="div_ptp"/>
                        <div runat="server" ID="div_liab">
                            <asp:Label runat="server" Text="Select Month of Year" ForeColor="DarkOrange" style="float:left; margin-top:4px; margin-right:4px;"/>
                            <asp:DropDownList ID="dd_liab_graph_month" runat="server" AutoPostBack="true" style="float:left; margin-top:4px; margin-right:4px;"/>
                            <asp:CheckBox ID="cb_detailed_liab_include_weekends" runat="server" Checked="false" Text="Include Weekends<br/><br/>" ForeColor="DarkOrange" AutoPostBack="true"/>
                            <asp:GridView ID="gv_detailed_liab_graph" runat="server" Width="800" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_dlg_RowDataBound"/>
                            <telerik:RadChart ID="rc_detailed_liab_graph" runat="server" EnableHandlerDetection="false" Height="500px" Width="1280px"
                            IntelligentLabelsEnabled="false" Autolayout="True" PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="270"
                            PlotArea-XAxis-AutoScale="false" Skin="Metal"/>
                            <br/>
                            <asp:DropDownList ID="dd_ddgraph_type" runat="server" AutoPostBack="true" style="position:relative; left:4px; top:8px;">
                                <asp:ListItem Text="Direct Debits/BAC Only"/>
                                <asp:ListItem Text="All Liabilities"/>
                            </asp:DropDownList>
                        </div>
                        <div runat="server" ID="div_mediasales"/>
                        <div runat="server" ID="div_groupsummary">
                            <table border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top:solid 1px gray;">
                                <tr>
                                    <td valign="top">
                                        <table runat="server" id="tbl_groupsummary" bgcolor="white" border="1" cellpadding="0" cellspacing="0" width="550" style="font-family:Verdana; font-size:8pt;">
                                            <tr>
                                                <td><asp:CheckBox runat="server" ID="cb_gs_invoice" Text="Only invoiced sales" Checked="true" AutoPostBack="true"/></td>
                                                <td colspan="3"><i>&nbsp;All values are converted to USD</i></td>
                                            </tr> 
                                            <tr><td colspan="4"><br/><b>Value:</b></td></tr>
                                            <tr>
                                                <td width="28%">Group Total Sales</td>
                                                <td width="22%"><asp:Label runat="server" ID="lbl_gs_group_total_sales"/></td>
                                                <td width="28%">Group Total Value</td>
                                                <td width="22%"><asp:Label runat="server" ID="lbl_gs_group_total_sales_value"/></td>
                                            </tr>
                                            <tr><td colspan="4"><br/><b>Status Totals (All Years):</b></td></tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="In Progress" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_inprogress"/></td>
                                                <td><asp:Label runat="server" Text="In Progress Value" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_inprogress_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Promise to Pay" ForeColor="Blue"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_ptp"/></td>
                                                <td><asp:Label runat="server" Text="Promise to Pay Value" ForeColor="Blue"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_ptp_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Proof of Payment" ForeColor="DarkGreen"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_pop"/></td>
                                                <td><asp:Label runat="server" Text="Proof of Payment Value" ForeColor="DarkGreen"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_pop_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Litigation" ForeColor="Crimson"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_litigation"/></td>
                                                <td><asp:Label runat="server" Text="Litigation Value" ForeColor="Crimson"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_litigation_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Written Off" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_writtenoff"/></td>
                                                <td><asp:Label runat="server" Text="Written Off Value" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_writtenoff_value"/></td>
                                            </tr> 
                                            <tr>
                                                <td><asp:Label runat="server" Text="Other Tabs" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_othertab"/></td>
                                                <td><asp:Label runat="server" Text="Other Tabs Value" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_othertab_value"/></td>
                                            </tr>
                                            <tr><td colspan="4"><br /><b>Liabilities:</b></td></tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Creditors" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_total_standard_liabilities"/></td>
                                                <td><asp:Label runat="server" Text="Direct Debits/BAC" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_total_dd_liabilities"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Cheques" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_total_cheque_liabilities"/></td>
                                                <td><asp:Label runat="server" Text="All Liabilities"/></td>
                                                <td><asp:Label runat="server" ID="lbl_gs_total_liabilities_value"/></td>
                                            </tr>
                                            <tr>
                                                <td colspan="4">
                                                    <br/><b>Cash:&nbsp;<asp:Label runat="server" ID="lbl_gs_cash_total_reports" Font-Bold="true"/></b>  
                                                    <asp:LinkButton ID="lb_gs_view_summaries" runat="server" ForeColor="Blue" Text="(show reports)"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>Cash Collected</td>
                                                <td><asp:Label ID="lbl_gs_cashcollected" runat="server" Text="0"/></td>
                                                <td>Cash Available</td>
                                                <td><asp:Label ID="lbl_gs_cashavail" runat="server" Text="0"/></td>
                                            </tr>
                                            <tr><td colspan="4"><br /><b>Call Stats:&nbsp;<asp:Label runat="server" ID="lbl_gs_calls_total_reports" Font-Bold="true"/></b></td></tr>
                                            <tr>
                                                <td>Employee</td>
                                                <td>Total Calls</td>
                                                <td>Region</td>
                                                <td>Total Calls</td>
                                            </tr>
                                            <tr>
                                                <td valign="top"><div runat="server" id="div_gs_call_stats_names"/></td>
                                                <td valign="top"><div runat="server" id="div_gs_call_stats_calls"/></td>
                                                <td valign="top">
                                                    <table>
                                                        <tr><td>USA</td></tr>
                                                        <%--<tr><td>India</td></tr>--%>
                                                        <tr><td>UK</td></tr>
                                                        <tr><td><b>Total:</b></td></tr>
                                                    </table>
                                                </td>
                                                <td valign="top">                                                
                                                    <table>
                                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_usa" Text="0"/></td></tr>
                                                        <%--<tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_india" Text="0"/></td></tr>--%>
                                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_uk" Text="0"/></td></tr>
                                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_total" Text="0" Font-Bold="true"/></td></tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td valign="top" align="right">
                                        <table border="0" cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td align="left">
                                                    <asp:Label runat="server" Text="Report Messages/Notes:" ForeColor="White"/><br />
                                                    <asp:TextBox ID="tb_gs_message" runat="server" ReadOnly="true" TextMode="MultiLine" Width="716" Height="363"/>
                                                </td>
                                            </tr>
                                        </table>  
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div runat="server" ID="div_detailedgroupsummary">
                            <table border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top:solid 1px gray;">
                                <tr><td colspan="2"><asp:Label runat="server" Text="NOTE: This summary shows totals for invoiced <i>and</i> non-invoiced sales. Tab total values may differ from other summaries which only sum invoiced sales."
                                Font-Size="Medium" ForeColor="#3366ff" BackColor="#f4a83d" Width="1277"/></td></tr>
                                <tr>
                                    <td valign="top">
                                        <asp:Label ID="lbl_gv_cur" runat="server" ForeColor="White" Font-Bold="true" BackColor="#3366ff" Width="637"/>
                                        <asp:GridView ID="gv_dgs_cur" runat="server" 
                                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="637px" HeaderStyle-Font-Size="8"
                                            HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            AutoGenerateColumns="true" AllowSorting="false" RowStyle-ForeColor="Black" OnRowDataBound="gv_dgs_RowDataBound">
                                        </asp:GridView> 
                                    </td>
                                    <td valign="top">
                                        <asp:Label ID="lbl_gv_past" runat="server" ForeColor="White" Font-Bold="true" BackColor="#3366ff" Width="637" style="margin-left:2px;"/>
                                        <asp:GridView ID="gv_dgs_past" runat="server" style="margin-left:2px;" 
                                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="637px" HeaderStyle-Font-Size="8"
                                            HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            AutoGenerateColumns="true" AllowSorting="false" RowStyle-ForeColor="Black" OnRowDataBound="gv_dgs_RowDataBound">
                                        </asp:GridView>  
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <asp:Label runat="server" ForeColor="White" Font-Bold="true" BackColor="#3366ff" Text="<br/>Value by Tab"/>
                                        <asp:GridView ID="gv_value_by_tab" runat="server" 
                                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" HeaderStyle-Font-Size="8"
                                            HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            AutoGenerateColumns="true" AllowSorting="false" RowStyle-ForeColor="Black" OnRowDataBound="gv_vbt_RowDataBound">
                                        </asp:GridView> 
                                        <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="View between dates:&nbsp;" style="float:left; position:relative; top:4px;"/>
                                        <telerik:RadDatePicker ID="dp_value_by_tab_start" runat="server" width="100px" SelectedDate="01/01/2009" style="float:left;"/>
                                        <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="&nbsp;and&nbsp;" style="float:left; position:relative; top:4px;"/>
                                        <telerik:RadDatePicker ID="dp_value_by_tab_end" runat="server" width="100px" SelectedDate="01/01/2050" style="float:left;"/>                           
                                        <asp:Button ID="btn_value_by_tab_daterange_submit" runat="server" Text="Go" style="float:left;"/>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div runat="server" ID="div_dailysummary">
                            <table border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top:solid 1px gray;">
                                <tr>
                                    <td valign="top">
                                        <table runat="server" id="tbl_dailysummary" bgcolor="white" border="1" cellpadding="0" cellspacing="0" width="550" style="font-family:Verdana; font-size:8pt;">
                                            <tr>
                                                <td colspan="4"><asp:CheckBox runat="server" ID="cb_ds_invoice" Text="Only invoiced sales" Checked="true" AutoPostBack="true" /></td>
                                            </tr> 
                                            <tr><td colspan="4"><br/><b>Status Totals (All Years):</b></td></tr>
                                            <tr>
                                                <td width="30%"><asp:Label runat="server" Text="In Progress" ForeColor="Black"/></td>
                                                <td width="20%"><asp:Label runat="server" ID="lbl_ds_inprogress_value"/></td>
                                                <td width="30%"><asp:Label runat="server" Text="Promise to Pay" ForeColor="Blue"/></td>
                                                <td width="20%"><asp:Label runat="server" ID="lbl_ds_ptp_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" ID="lbl_ds_ptp_week" ForeColor="Blue"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_ptp_week_value"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_ptp_nextweek" ForeColor="Blue"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_ptp_nextweek_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Proof of Payment" ForeColor="DarkGreen"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_pop_value"/></td>
                                                <td><asp:Label runat="server" Text="Litigation" ForeColor="Crimson"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_litigation_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Written Off" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_writtenoff_value"/></td>
                                                <td><asp:Label runat="server" Text="Other Tabs" ForeColor="Black"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_othertab_value"/></td>
                                            </tr>
                                            <tr><td colspan="4"><br/><b>Liabilities:</b></td></tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Creditors" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_total_standard_liabilities"/></td>
                                                <td><asp:Label runat="server" Text="Direct Debits/BAC" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_total_dd_liabilities_value"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" Text="Cheques" ForeColor="DimGray"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_total_cheque_liabilities_value"/></td>
                                                <td><asp:Label runat="server" Text="All Liabilities"/></td>
                                                <td><asp:Label runat="server" ID="lbl_ds_total_liabilities_value"/></td>
                                            </tr>
                                            <tr><td colspan="4"><br/><b>Cash:</b></td></tr>
                                            <tr>
                                                <td>Cash Collected<asp:Label runat="server" ID="lbl_ds_cash_c_currency"/></td>
                                                <td>
                                                    <asp:TextBox ID="tb_ds_cashcollected" runat="server" Width="100px" BackColor="LemonChiffon"/>
                                                    <asp:Label ID="lbl_ds_cashcollected" runat="server" Visible="false" />
                                                </td>
                                                <td>Cash Available<asp:Label runat="server" ID="lbl_ds_cash_a_currency"/></td>
                                                <td>
                                                    <asp:TextBox ID="tb_ds_cashavail" runat="server" Width="100px" BackColor="LemonChiffon"/>
                                                    <asp:Label ID="lbl_ds_cashavail" runat="server" Visible="false"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>Collected this Week</td>
                                                <td colspan="3"><asp:Label runat="server" ID="lbl_ds_cashcollected_so_far"/></td>
                                            </tr>
                                            <tr>
                                                <td style="border-right:0px;"><br/><b>Call Stats:</b></td>
                                                <td style="border-left:0px;" colspan="3">
                                                    <asp:CompareValidator runat="server" ControlToValidate="tb_ds_cashcollected" 
                                                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Cash Collected must be a valid number!"> 
                                                    </asp:CompareValidator>
                                                    <asp:CompareValidator runat="server" ControlToValidate="tb_ds_cashavail" 
                                                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Cash Available must be a valid number!"> 
                                                    </asp:CompareValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4">Total Calls</td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <b>User 1:</b>
                                                    <asp:DropDownList runat="server" ID="dd_ds_cs_user1" Width="150" onchange="return CheckFinanceNames();"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_user1" Visible="false"/>
                                                </td>
                                                <td colspan="2">
                                                    <i>Total Calls:</i>
                                                    <asp:TextBox ID="tb_ds_cs_tc1" runat="server" Width="50px" BackColor="LemonChiffon"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_tc1" Visible="false"/>
                                                    <asp:CompareValidator runat="server" ControlToValidate="tb_ds_cs_tc1" 
                                                        Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Total Calls must be a valid number!"> 
                                                    </asp:CompareValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <b>User 2:</b>
                                                    <asp:DropDownList runat="server" ID="dd_ds_cs_user2" Width="150" onchange="return CheckFinanceNames();"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_user2" Visible="false"/>
                                                </td>
                                                <td colspan="2">
                                                    <i>Total Calls:</i>
                                                    <asp:TextBox ID="tb_ds_cs_tc2" runat="server" Width="50px" BackColor="LemonChiffon"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_tc2" Visible="false"/>
                                                    <asp:CompareValidator runat="server" ControlToValidate="tb_ds_cs_tc2" 
                                                        Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Total Calls must be a valid number!"> 
                                                    </asp:CompareValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <b>User 3:</b>
                                                    <asp:DropDownList runat="server" ID="dd_ds_cs_user3" Width="150" onchange="return CheckFinanceNames();"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_user3" Visible="false"/>
                                                </td>
                                                <td colspan="2">
                                                    <i>Total Calls:</i>
                                                    <asp:TextBox ID="tb_ds_cs_tc3" runat="server" Width="50px" BackColor="LemonChiffon"/>
                                                    <asp:Label runat="server" ID="lbl_ds_cs_tc3" Visible="false" />
                                                    <asp:CompareValidator runat="server" ControlToValidate="tb_ds_cs_tc3" 
                                                        Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Total Calls must be a valid number!"> 
                                                    </asp:CompareValidator>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td valign="top" align="right">
                                        <table cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td align="left">
                                                    <asp:Label ID="lbl_ds_mail_subject" runat="server" Text="Subject" ForeColor="White"/><br />
                                                    <asp:TextBox ID="tb_ds_mail_subject" runat="server" TextMode="SingleLine" Width="610"/>
                                                </td>
                                                <td align="right" valign="bottom">
                                                    <asp:Button runat="server" ID="btn_ds_send" Text="Send E-mail" OnClick="CheckExistingSummaries" 
                                                    OnClientClick="if(!Page_ClientValidate()){ alert('One or more recipient addresses are invalid!'); }else{ return CheckDSValues(); }"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left" colspan="2"> 
                                                    <asp:Label ID="lbl_ds_mail_mailto" runat="server" Text="Recipients (must be separated by semicolons) - " ForeColor="White"/>
                                                    <asp:LinkButton ID="lb_ds_save_to" runat="server" Text="Save Recipient List" OnClick="SaveDSMailingRecipients" ForeColor="DarkOrange" Font-Size="7"/><br />
                                                    <asp:TextBox ID="tb_ds_mail_mailto" runat="server" TextMode="MultiLine" Height="50px" Width="718" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                                    ControlToValidate="tb_ds_mail_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" Display="Dynamic"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left" colspan="2">
                                                    <asp:Label ID="lbl_ds_mail_message" runat="server" Text="Message/Notes" ForeColor="White"/><br />
                                                    <asp:TextBox ID="tb_ds_mail_message" runat="server" TextMode="MultiLine" Height="225" Width="718" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                                                </td>
                                            </tr>
                                        </table> 
                                    </td>
                                </tr>
                            </table>    
                            <asp:HiddenField ID="hf_ds_action" runat="server" />
                        </div>
                    </div>
                    <br runat="server" ID="gv_br" visible="false"/>
                </td>
            </tr>
        </table>
        <asp:HiddenField ID="hf_moveToTab" runat="server"/>
        <asp:HiddenField ID="hf_newTab" runat="server"/>
        <asp:HiddenField ID="hf_editSale" runat="server"/>
        <asp:HiddenField ID="hf_edit_media_sale" runat="server" />
        <asp:HiddenField ID="hf_edit_liab" runat="server" />
        <asp:HiddenField ID="hf_editTab" runat="server"/>
        <asp:HiddenField ID="hf_newLiab" runat="server"/>
        <asp:HiddenField ID="hf_newLiabList" runat="server"/>
        <asp:HiddenField ID="hf_reopen_acc" runat="server"/>
        <hr />
    </div>
    
    <script type="text/javascript">
        function NewTabOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_newTab.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function NewLiabOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_newLiab.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function EditLiabOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_edit_liab.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function EditTabOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_editTab.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function EditSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data || sender.rebind == true) {
                if (data) {
                    grab("<%= hf_editSale.ClientID %>").value = data;
                }
                refresh();
                return true;
            }
        }
        function EditMediaSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_edit_media_sale.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function ReOpenAccOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_reopen_acc.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function MoveToTabOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_moveToTab.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function DailySummaryOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_ds_action.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function GenericOnClientClose(sender, args) {
            refresh();
            return true;
        }
        function AddToNewLiabList(name) {
            grab("<%= hf_newLiabList.ClientID %>").value += name + ', ';
            return true;
        }
        function refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.disabled = false;
            button.click();
            return true;
        }
        function CheckFinanceNames() {
            var dd1 = grab("<%= dd_ds_cs_user1.ClientID %>");
            var dd2 = grab("<%= dd_ds_cs_user2.ClientID %>");
            var dd3 = grab("<%= dd_ds_cs_user3.ClientID %>");

            if (dd2.value != '' && dd2.value == dd1.value) { dd2.value = ''; }
            if (dd3.value != '' && dd3.value == dd1.value) { dd3.value = ''; }
            if (dd3.value != '' && dd3.value == dd2.value) { dd3.value = ''; }
            return false;
        }
        function CheckDSValues() {
            var cc = grab("<%= tb_ds_cashcollected.ClientID %>");
            var ca = grab("<%= tb_ds_cashavail.ClientID %>");

            if(parseFloat(cc.value) >= 1000000 || parseFloat(ca.value) >= 1000000){         
                return confirm('Cash Collected/Cash Available is higher than 1,000,000, are you sure this is correct?');}
            else{
                return true;}
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>