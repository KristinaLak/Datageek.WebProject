<%--
Author   : Joe Pickering, 08/08/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Editorial Tracker" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="EditorialTracker.aspx.cs" Inherits="EditorialTracker" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_et" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:PostBackTrigger ControlID="imbtn_export"/>
        </Triggers>
        <ContentTemplate>
        <telerik:RadToolTipManager ID="rttm" runat="server" ShowDelay="450" OffsetY="-18" EnableViewState="false" Overlay="false"
        ManualClose="true" RelativeTo="Mouse" Sticky="true" Skin="Silk" ShowCallout="false"
        Animation="None" ShowEvent="OnMouseOver" AutoTooltipify="false" RenderInPageRoot="true" OnAjaxUpdate="ShowContactList"/>

        <telerik:RadWindowManager runat="server" VisibleStatusbar="false" Skin="Black" OnClientShow="CenterRadWindow" Behaviors="Move, Close, Pin" AutoSize="True" RenderInPageRoot="true" ShowContentDuringLoad="false">
            <Windows>
                <telerik:RadWindow ID="win_newissue" runat="server" Title="&nbsp;New Issue" OnClientClose="NewIssueOnClientClose" NavigateUrl="etnewissue.aspx"/>
                <telerik:RadWindow ID="win_managesale" runat="server"  OnClientClose="ManageSaleOnClientClose"/>
                <telerik:RadWindow ID="win_managefp" runat="server" Title="&nbsp;Digital Footprint" OnClientClose="refresh"/>
                <telerik:RadWindow ID="win_movesale" runat="server" Title="&nbsp;Move Feature" OnClientClose="MoveSaleOnClientClose"/>
                <telerik:RadWindow ID="win_editlinks" runat="server" Title="&nbsp;Edit Magazine Links"/>
                <telerik:RadWindow ID="win_feedback" runat="server" Title="&nbsp;Survey Feedback Viewer"/>
            </Windows>
        </telerik:RadWindowManager>

        <div ID="div_page" runat="server" class="wider_page">
            <hr />

            <%--Main Table--%>
            <table width="99%" style="margin-left:auto; margin-right:auto;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Editorial" ForeColor="White" Font-Bold="true" Font-Size="Medium" Style="position:relative; top:-2px;"/>
                        <asp:Label runat="server" Text="Tracker" ForeColor="White" Font-Bold="false" Font-Size="Medium" Style="position:relative; top:-2px;"/>
                    </td>
                    <td align="right" valign="top">
                        <asp:LinkButton ID="lb_edit_links" runat="server" ForeColor="Silver" Text="Edit Magazine Links" OnClientClick="alert('bye');" Style="position:relative; top:-3px;"/>
                        <asp:LinkButton ID="lb_send_link_mails" runat="server" ForeColor="Silver" Text="E-mail Magazine Links" OnClick="SendMagLinkOrFootprintEmails" Style="position: relative; top:-3px; padding-left:4px; border-left:solid 1px gray;"
                            OnClientClick="return confirm('Are you sure you wish to send all magazine link e-mails?\n\nLink mails will only be sent for non-cancelled sales with valid: contact name, contact e-mail address, page number(s), corresponding magazine link and magazine cover image.\n\nPlease be patient, this may take a minute or two.')"/>
                        <asp:LinkButton ID="lb_send_footprint_mails" runat="server" ForeColor="Silver" Text="E-mail Digital Footprints" OnClick="SendMagLinkOrFootprintEmails" Style="position: relative; top: -3px; padding-left: 4px; border-left: solid 1px gray;"
                            OnClientClick="return confirm('Are you sure you wish to send all ready Digital Footprint e-mails?\n\nDigital Footprint mails will only be sent for non-cancelled sales with valid: contact name, contact e-mail address, page number(s), corresponding magazine link and magazine cover image.\n\nPlease be patient, this may take a minute or two.')"/>
                        <asp:LinkButton ID="lb_survey_feedback" runat="server" ForeColor="Silver" Text="View Survey Feedback" Style="position: relative; top: -3px; padding-left: 4px; border-left: solid 1px gray;"/>
                    </td>
                </tr>
                <tr>
                    <td valign="top">
                        <%-- Navigation Panel--%>
                        <asp:Panel runat="server" HorizontalAlign="Left" Width="415px">
                            <table border="1" cellpadding="0" cellspacing="0" width="415px" bgcolor="White">
                                <tr>
                                    <td valign="top" align="left" colspan="2">
                                        <img src="/images/misc/titlebaralpha.png"/>
                                        <asp:Label Text="Region/Issue" ForeColor="White" runat="server" Style="position:relative; top:-6px; left:-168px;"/>
                                    </td>
                                    <td align="right" valign="top">
                                        <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\images\icons\dashboard_refresh.png" OnClick="ChangeIssue"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="center">
                                        <%--Left Button--%>
                                        <asp:ImageButton ID="imbtn_p_issue" Height="22" Text="Previous Quarter" ImageUrl="~\images\icons\dashboard_leftgreenarrow.png" runat="server" OnClick="PrevIssue"/>
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="dd_region" runat="server" Width="110px" AutoPostBack="true" OnSelectedIndexChanged="ChangeRegion">
                                            <asp:ListItem Text="Group" Value="Norwich/Africa/Europe/Middle East/Asia/Canada/Boston/East Coast/West Coast/USA"/>
                                            <asp:ListItem Text="ANZ" Value="ANZ"/>
                                            <asp:ListItem Text="North America" Value="Canada/Boston/East Coast/West Coast/USA"/>
                                            <asp:ListItem Text="India" Value="India"/>
                                            <asp:ListItem Text="Brazil" Value="Brazil"/>
                                            <asp:ListItem Text="Latin America" Value="Latin America"/>
                                        </asp:DropDownList>
                                        <asp:DropDownList ID="dd_issue" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="ChangeIssue"/>
                                        <asp:LinkButton ID="lb_new_issue" runat="server" Text="Create New Issue" ForeColor="Gray" Style="float:right; position:relative; top:3px; padding-right:5px;"
                                            OnClientClick="radopen('etnewissue.aspx', 'win_newissue'); return false;"/>
                                    </td>
                                    <td align="center">
                                        <%--Right Button--%>
                                        <asp:ImageButton ID="imbtn_n_issue" Height="22" Text="Next Quarter" ImageUrl="~\images\icons\dashboard_rightgreenarrow.png" runat="server" OnClick="NextIssue"/>
                                    </td>
                                </tr>
                            </table>
                            <br />
                        </asp:Panel>
                        <%--Summary--%>
                        <table border="1" runat="server" id="tbl_summary" cellpadding="1" cellspacing="0" width="415px" bgcolor="White" style="color: Black">
                            <tr>
                                <td align="left" colspan="5">
                                    <asp:Image runat="server" ImageUrl="/images/misc/titlebarlong.png" Style="position:relative; top:-1px; left:-1px;"/>
                                    <asp:Label Text="Summary" runat="server" ForeColor="White" Style="position:relative; top:-7px; left:-208px;"/>
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
                                    <img src="/images/misc/titlebaralpha.png"/>
                                    <img src="/images/icons/dashboard_log.png" height="20px" width="20px"/>
                                    <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                                </td>
                            </tr>
                            <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="MultiLine" Height="210" Width="845px"/></td></tr>
                        </table>
                        <%-- End Log--%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div ID="div_bookcontrols" runat="server">
                            <br />
                            <asp:ImageButton ID="imbtn_new_sale" alt="New Sale" runat="server" Height="26" Width="26" ImageUrl="~\images\icons\salesbook_addnewsale.png"/>
                            <asp:ImageButton ID="imbtn_printpreview" alt="View Printer-Friendly Version" runat="server" Height="26" Width="22" ImageUrl="~\images\icons\salesbook_printerfriendlyversion.png" OnClick="PrintPreview"/>
                            <asp:ImageButton ID="imbtn_export" alt="Export to Excel" runat="server" Height="25" Width="23" ImageUrl="~\images\icons\salesbook_exporttoexcel.png" OnClick="ExportToExcel"/>
                            <asp:Label ID="lbl_issue_empty" runat="server" Visible="false" Text="This issue is empty." ForeColor="DarkOrange" Font-Size="Medium" style="position:relative; left:450px;"/>
                            <asp:Label ID="lbl_old_norwich_book" runat="server" Visible="false" Text="This issue is an old 'Norwich' issue." ForeColor="DarkOrange" Font-Size="Small"/>
                        </div>
                    </td>
                    <td align="right" valign="bottom">
                        <telerik:RadTabStrip ID="rts_region" runat="server" AutoPostBack="true" OnTabClick="ChangeIssue" Align="Right" SelectedIndex="0" Skin="Vista" style="position:relative; top:6px;"/>
                    </td>
                </tr>
                <%-- GridView Area --%>
                <tr><td colspan="2"><div ID="div_gv" runat="server"/></td></tr>
            </table>
            <hr />
        </div>

        <asp:HiddenField ID="hf_new_sale" runat="server"/>
        <asp:HiddenField ID="hf_edit_sale" runat="server"/>
        <asp:HiddenField ID="hf_move_sale" runat="server"/>
        <asp:HiddenField ID="hf_new_issue" runat="server"/>
        <asp:HiddenField ID="hf_set_refresh" runat="server"/>

        <script type="text/javascript">
        function ManageSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data || sender.rebind == true) {
                if (data && data.indexOf("added") != -1) {
                    grab("<%= hf_new_sale.ClientID %>").value = data;
                }
                else if(data) {
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
        function MoveSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_move_sale.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function SetRefresh() {
            grab("<%= hf_set_refresh.ClientID %>").value = "1";
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
