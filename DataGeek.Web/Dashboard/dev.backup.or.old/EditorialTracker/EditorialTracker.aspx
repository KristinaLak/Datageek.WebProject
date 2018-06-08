<%--
Author   : Joe Pickering, 08/08/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Editorial Tracker" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="EditorialTracker.aspx.cs" Inherits="EditorialTracker" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif" Height="40" Width="40" style="position:relative; top:7px;"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_et" runat="server" ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="imbtn_export"/>
    </Triggers>
    <ContentTemplate>
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="True">
        <Windows>
            <telerik:RadWindow ID="win_newissue" runat="server" Title="&nbsp;New Issue" Behaviors="Move, Close, Pin" OnClientClose="NewIssueOnClientClose" NavigateUrl="ETNewIssue.aspx"/> 
            <telerik:RadWindow ID="win_managesale" runat="server" Behaviors="Move, Close, Pin" OnClientClose="ManageSaleOnClientClose"/> 
            <telerik:RadWindow ID="win_managefp" runat="server" Title="&nbsp;Digital Footprint" Behaviors="Move, Close, Pin" OnClientClose="refresh"/> 
            <telerik:RadWindow ID="win_copysale" runat="server" Title="&nbsp;Move Feature" Behaviors="Move, Close, Pin" OnClientClose="CopySaleOnClientClose"/> 
            <telerik:RadWindow ID="win_editlinks" runat="server" Title="&nbsp;Edit Magazine Links" Behaviors="Move, Close, Pin"/>
            <telerik:RadWindow ID="win_feedback" runat="server" Title="&nbsp;Survey Feedback Viewer" Behaviors="Move, Close, Pin"/>
        </Windows> 
    </telerik:RadWindowManager>
    <div id="div_page" runat="server" class="wider_page">
        <hr />

        <%--Main Table--%>
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Editorial" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Tracker" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/>
                </td>
                <td align="right" valign="top">
                    <asp:LinkButton ID="lb_edit_links" runat="server" ForeColor="Silver" Text="Edit Magazine Links" style="position:relative; top:-3px;"/>
                    <asp:LinkButton ID="lb_send_link_mails" runat="server" ForeColor="Silver" Text="E-mail Magazine Links" OnClick="SendMagLinkOrFootprintEmails" style="position:relative; top:-3px; padding-left:4px; border-left:solid 1px gray;"
                    OnClientClick="return confirm('Are you sure you wish to send all magazine link e-mails?\n\nLink mails will only be sent for non-cancelled sales with valid: contact name, contact e-mail address, page number(s), corresponding magazine link and magazine cover image.\n\nPlease be patient, this may take a minute or two.')"/>
                    <asp:LinkButton ID="lb_send_footprint_mails" runat="server" ForeColor="Silver" Text="E-mail Digital Footprints" OnClick="SendMagLinkOrFootprintEmails" style="position:relative; top:-3px; padding-left:4px; border-left:solid 1px gray;"
                    OnClientClick="return confirm('Are you sure you wish to send all ready Digital Footprint e-mails?\n\nDigital Footprint mails will only be sent for non-cancelled sales with valid: contact name, contact e-mail address, page number(s), corresponding magazine link and magazine cover image.\n\nPlease be patient, this may take a minute or two.')"/>
                    <asp:LinkButton ID="lb_survey_feedback" runat="server" ForeColor="Silver" Text="View Survey Feedback" style="position:relative; top:-3px; padding-left:4px; border-left:solid 1px gray;"/>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <%-- Navigation Panel--%> 
                    <asp:Panel runat="server" HorizontalAlign="Left" Width="415px">
                        <table border="1" cellpadding="0" cellspacing="0" width="415px" bgcolor="White">
                            <tr>
                                <td valign="top" align="left" colspan="2">
                                    <img src="/Images/Misc/titleBarAlpha.png"/> 
                                    <asp:Label Text="Region/Issue" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-168px;"/>
                                </td>
                                <td align="right" valign="top">
                                    <asp:ImageButton id="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png"/>
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <%--Left Button--%> 
                                    <asp:ImageButton ID="imbtn_p_issue" height="22" Text="Previous Quarter" ImageUrl="~\Images\Icons\dashboard_LeftGreenArrow.png" runat="server" OnClick="PrevIssue" />  
                                </td> 
                                <td>
                                    <asp:DropDownList id="dd_region" runat="server" Width="110px" AutoPostBack="true">
                                        <asp:ListItem Text="Norwich" Value="Norwich/Africa/Europe/Middle East/Asia"/>
                                        <asp:ListItem Text="Australia" Value="Australia"/>
                                        <asp:ListItem Text="North America" Value="Canada/Boston/East Coast/West Coast/USA"/>
                                        <asp:ListItem Text="India" Value="India"/>
                                        <asp:ListItem Text="Brazil" Value="Brazil"/>
                                        <asp:ListItem Text="Latin America" Value="Latin America"/>
                                    </asp:DropDownList>
                                    <asp:DropDownList id="dd_issue" runat="server" Width="120px" AutoPostBack="true"/>
                                    <asp:LinkButton ID="lb_delete_issue" ForeColor="Gray" runat="server" Text="Delete" alt="Delete Issue" OnClick="DeleteIssue" style="position:relative; top:2px; left:11px;"
                                    OnClientClick="if(confirm('This will permanently delete this issue but its companies will remain in the database. Are you sure?')){if(confirm('Are you positive you wish to permanently delete this issue?')){return true;}else{return false;}}else{return false;}"/>
                                    <asp:LinkButton ID="lb_new_issue" runat="server" Text="New Issue" ForeColor="Gray" style="position:relative; top:2px; left:12px; padding-left:5px; border-left:solid 1px gray;"
                                    OnClientClick="try{ radopen('ETNewIssue.aspx', 'win_newissue'); }catch(E){ IE9Err(); } return false;"/>
                                </td>
                                <td align="center">
                                    <%--Right Button--%> 
                                    <asp:ImageButton ID="imbtn_n_issue" height="22" Text="Next Quarter" ImageUrl="~\Images\Icons\dashboard_RightGreenArrow.png" runat="server" OnClick="NextIssue"/> 
                                </td>  
                            </tr>
                        </table>
                        <br />
                    </asp:Panel>
                    <%--Summary--%>
                    <table border="1" runat="server" id="tbl_summary" cellpadding="1" cellspacing="0" width="415px" bgcolor="White" style="color:Black">
                        <tr>
                            <td align="left" colspan="5">
                                <asp:Image runat="server" ImageUrl="/Images/Misc/titleBarLong.png" style="position:relative; top:-1px; left:-1px;"/> 
                                <asp:Label Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-208px;"/>
                            </td>
                        </tr>
                        <tr>
                            <td width="38%">Total Features</td>
                            <td width="11%"><asp:Label runat="server" ID="lbl_s_total_companies"/></td>
                            <td width="35%" colspan="2">Total Re-Runs</td>
                            <td width="16%"><asp:Label runat="server" ID="lbl_s_reruns" ForeColor="MediumSlateBlue" Font-Bold="true"/></td>
                        </tr>
                        <tr>
                            <td>Parachutes</td>
                            <td><asp:Label runat="server" ID="lbl_s_total_p_companies"/></td>
                            <td colspan="2">Associations</td>
                            <td><asp:Label runat="server" ID="lbl_s_total_a_companies"/></td>
                        </tr>  
                        <tr>
                            <td>Interviews Scheduled</td>
                            <td><asp:Label runat="server" ID="lbl_s_interviews_scheduled"/></td>
                            <td colspan="2">Interviews Conducted</td>
                            <td><asp:Label runat="server" ID="lbl_s_interviews_conducted"/></td>
                        </tr>  
                        <tr>
                            <td>Para. Interviews Sched.</td>
                            <td><asp:Label runat="server" ID="lbl_s_p_interviews_scheduled"/></td>
                            <td colspan="2">Para. Interviews Cond.</td>
                            <td><asp:Label runat="server" ID="lbl_s_p_interviews_conducted"/></td>
                        </tr>  
                        <tr>
                            <td>Interviews Not Scheduled</td>
                            <td><asp:Label runat="server" ID="lbl_s_interviews_not_conducted"/></td>
                            <td colspan="2">Added Today/Yesterday</td>
                            <td><asp:Label runat="server" ID="lbl_s_added_today"/>/<asp:Label runat="server" ID="lbl_s_added_yesterday"/></td>
                        </tr>   
                        <tr>
                            <td colspan="2">Total Waiting for Draft</td>
                            <td width="55"><asp:Label runat="server" ID="lbl_s_drafts" ForeColor="Red" Font-Bold="true"/></td>
                            <td>Cold Edits</td>
                            <td><asp:Label runat="server" ID="lbl_s_cold_edits" ForeColor="Red" Font-Bold="true"/></td>
                        </tr>  
                        <tr>
                            <td colspan="2">Total Drafts out for Approval</td>
                            <td><asp:Label runat="server" ID="lbl_s_draft_out" ForeColor="Orange" Font-Bold="true"/></td>
                            <td>Watched</td>
                            <td><asp:Label runat="server" ID="lbl_s_watched" ForeColor="DarkBlue" Font-Bold="true"/></td>
                        </tr>
                        <tr>
                            <td colspan="2">Total Copy Approved</td>
                            <td colspan="3"><asp:Label runat="server" ID="lbl_s_approved" ForeColor="Lime" Font-Bold="true"/></td>
                        </tr> 
                        <tr>
                            <td colspan="5">
                                <table cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td width="44">Totals:</td>
                                        <td><asp:Label runat="server" ID="lbl_s_region_totals" Font-Size="X-Small"/></td>
                                    </tr>
                                </table>
                            </td>
                        </tr> 
                    </table>
                    <%--End Summary--%>
                </td>
                <td align="right" valign="top">
                    <%--Log --%>       
                    <table border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                        <tr>
                            <td align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/dashboard_Log.png" height="20px" width="20px"/>
                                <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                            </td>
                        </tr>
                        <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="212" Width="845px"/></td></tr>
                    </table>
                   <%-- End Log--%>
                </td>
            </tr>
            <tr>
                <td>
                    <div ID="div_bookcontrols" runat="server">
                        <br />
                        <asp:ImageButton ID="imbtn_new_sale" alt="New Sale" runat="server" Height="26" Width="26" ImageUrl="~\Images\Icons\salesBook_AddNewSale.png"/> 
                        <asp:ImageButton ID="imbtn_printpreview" alt="View Printer-Friendly Version" runat="server" Height="26" Width="22" ImageUrl="~\Images\Icons\salesBook_PrinterFriendlyVersion.png" OnClick="PrintPreview"/>
                        <asp:ImageButton ID="imbtn_export" alt="Export to Excel" runat="server" Height="25" Width="23" ImageUrl="~\Images\Icons\salesBook_ExportToExcel.png" OnClick="ExportToExcel"/>
                        <asp:Label ID="lbl_issue_empty" runat="server" Visible="true" Text="This issue is empty." ForeColor="DarkOrange" Font-Size="Medium" style="position:relative; left:500px;"/>     
                    </div>
                </td>
                <td align="right" valign="bottom"><telerik:RadTabStrip ID="rts_region" runat="server" AutoPostBack="true" Align="Right" SelectedIndex="0" BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:6px;"/></td>
            </tr>
             <%-- GridView Area --%>
            <tr><td colspan="2"><div ID="div_gv" runat="server"/></td></tr>
        </table>
        <hr />
    </div>

    <asp:HiddenField ID="hf_new_sale" runat="server"/>
    <asp:HiddenField ID="hf_edit_sale" runat="server"/>
    <asp:HiddenField ID="hf_copy_sale" runat="server"/>
    <asp:HiddenField ID="hf_new_issue" runat="server"/>
    <asp:HiddenField ID="hf_watched_sale" runat="server"/>

    <script type="text/javascript">
        function ManageSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                if (data.indexOf("added") != -1) {
                    grab("<%= hf_new_sale.ClientID %>").value = data;
                }
                else {
                    grab("<%= hf_edit_sale.ClientID %>").value = data;
                }
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
        function CopySaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_copy_sale.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.disabled = false;
            button.click();
            return true;
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>