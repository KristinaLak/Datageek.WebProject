<%--
Author   : Joe Pickering, 10/05/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Commission Forms" Language="C#" EnableEventValidation="false" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="CommissionForms.aspx.cs" Inherits="CommissionForms" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadToolTipManager ID="rttm" runat="server" Animation="None" ShowDelay="40" Title="<i><font color='Black' size='2'>Commission Forms:</font></i>" 
     ManualClose="true" RelativeTo="Element" Sticky="true" OffsetY="-5" Skin="Vista" ShowEvent="OnRightClick" OnClientShow="ResizeRadToolTip" AutoTooltipify="true"/>
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="True" ShowContentDuringLoad="false">
        <Windows>
            <telerik:RadWindow runat="server" ID="win_commdefaults" Title="&nbsp;Manage Commission Defaults" OnClientClose="OnClientCloseHandler" Behaviors="Move, Close, Pin" NavigateUrl="cfcommissiondefaults.aspx"/>
            <telerik:RadWindow runat="server" ID="win_userrules" Title="&nbsp;Manage User Commission Rules" OnClientClose="OnClientCloseHandler" Behaviors="Move, Close, Pin" NavigateUrl="cfuserrules.aspx"/>
            <telerik:RadWindow runat="server" ID="win_eligibility" Title="&nbsp;Manage User Commission Eligibility" OnClientClose="OnClientCloseHandler" Behaviors="Move, Close, Pin" NavigateUrl="cfeligibility.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
    <telerik:RadFormDecorator runat="server" DecoratedControls="Buttons" Skin="Bootstrap"/>
   
    <div id="div_page" runat="server" class="wide_page">   
    <hr />
      
        <table border="0" width="99%" cellpadding="1" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Commission" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Forms" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
                <td align="right">
                    <asp:LinkButton ID="lb_download_help" runat="server" Text="Download Help File" ForeColor="LightGray" OnClick="DownloadHelpFile" style="position:relative; top:-3px;"
                    OnClientClick="return confirm('This will download a Word document help file.\n\nAre you sure?')"/>
                    <asp:LinkButton ID="lb_manage_system_rules" runat="server" Text="Commission Defaults Manager" ForeColor="LightGray" 
                    OnClientClick="try{ radopen(null, 'win_commdefaults'); }catch(E){ IE9Err(); } return false;" style="position:relative; top:-3px; padding-left:4px; border-left:solid 1px gray;"/>
                </td>
            </tr>
            <tr>
                <td align="left" valign="top" colspan="2">
                    <%--Navigation--%>
                    <table border="1" cellpadding="0" cellspacing="0" width="300px" bgcolor="White">
                        <tr>
                            <td valign="top" align="left" style="border-right:0">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <asp:Label Text="Office/Year" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-170px;"/>
                            </td>
                            <td align="center" style="border-left:0">
                                <asp:ImageButton id="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png" OnClick="BindCommissionGrid"/>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:DropDownList id="dd_office" runat="server" Width="110px" AutoPostBack="true" OnSelectedIndexChanged="BindCommissionGrid"/>
                                <asp:DropDownList id="dd_year" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindCommissionGrid"/> 
                            </td>
                        </tr>
                    </table>
                    <asp:CheckBox ID="cb_show_terminated" runat="server" ForeColor="DarkOrange" Text="Include Terminated from&nbsp;" Checked="false" AutoPostBack="true" OnCheckedChanged="BindCommissionGrid"/>
                    <asp:DropDownList ID="dd_terminated_span" runat="server" Height="20" Width="129" AutoPostBack="false" OnSelectedIndexChanged="BindCommissionGrid">
                        <asp:ListItem Value="1" Text="within a month"/>
                        <asp:ListItem Value="3" Text="within 3 months"/>
                        <asp:ListItem Selected="true" Value="12" Text="within this year"/>
                        <asp:ListItem Value="24" Text="within two years"/>
                    </asp:DropDownList>
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td align="center" colspan="2">
                    <asp:GridView ID="gv_commissions" runat="server" AutoGenerateColumns="False" Width="1240px"
                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                        AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                        OnRowDataBound="gv_commissions_RowDataBound" OnRowCommand="gv_commissions_RowCommand" 
                        RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="LightSteelBlue" RowStyle-BackColor="#f0f0f0"
                        RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead">
                        <Columns>
                            <asp:HyperLinkField ItemStyle-BackColor="Moccasin" ItemStyle-Width="100" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" 
                                HeaderText="CCA" DataTextField="friendlyname" DataNavigateUrlFields="userid" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}"/>
                            <asp:BoundField DataField="friendlyname"/>
                            <asp:BoundField DataField="cca_type"/>
                            <asp:ButtonField ButtonType="Image" ImageUrl="~/Images/Icons/mng_comm.png" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="January" DataTextField="January" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="February" DataTextField="February" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="March" DataTextField="March" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="April" DataTextField="April" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="May" DataTextField="May" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="June" DataTextField="June" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="July" DataTextField="July" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="August" DataTextField="August" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="September" DataTextField="September" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="October" DataTextField="October" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="November" DataTextField="November" />
                            <asp:ButtonField ButtonType="Link" ItemStyle-Width="80" HeaderText="December" DataTextField="December" />
                            <asp:BoundField ItemStyle-Width="80" ItemStyle-BackColor="Azure" ItemStyle-Font-Bold="true" HeaderText="Total" DataField="total"/>
                            <asp:BoundField DataField="userid"/>
                            <asp:BoundField DataField="employed"/>
                        </Columns>
                    </asp:GridView>  
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <%--Form Div--%>
                    <div id="div_form" runat="server" visible="false">
                        <hr ID="hr_form_rule" runat="server"/>
                        <asp:HiddenField ID="hf_form_id" runat="server"/>
                        <asp:HiddenField ID="hf_form_user_id" runat="server"/>
                        <asp:HiddenField ID="hf_form_month" runat="server"/>
                        <asp:HiddenField ID="hf_form_type" runat="server"/>
                        <asp:HiddenField ID="hf_form_friendlyname" runat="server"/>
                        <asp:HiddenField ID="hf_form_hit_commission" runat="server"/>
                        
                        <%--Form Table--%>
                        <table border="0" cellpadding="0" cellspacing="0" width="100%" style="font-family:Verdana; font-size:8pt;">
                            <tr id="tr_test" runat="server">
                                <%--Form Titles--%>
                                <td width="72%" valign="top" style="white-space:nowrap;">
                                    <asp:Label ID="lbl_form_office" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium"/><img src="/Images/Icons/small_arrow.png" style="margin-left:5px;"/>
                                    <asp:Label ID="lbl_form_year" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium"/><img src="/Images/Icons/small_arrow.png" style="margin-left:5px;"/>
                                    <asp:Label ID="lbl_form_month" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium"/><img src="/Images/Icons/small_arrow.png" style="margin-left:5px;"/>
                                    <asp:Label ID="lbl_form_name" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium"/><img src="/Images/Icons/small_arrow.png" style="margin-left:5px;"/>
                                    <asp:Label ID="lbl_form_cca_type" runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium"/>
                                    <asp:ImageButton ID="imbtn_manage_user_rules" runat="server" ImageUrl="~/Images/Icons/mng_comm.png" style="margin-left:3px; position:relative; top:2px;"/>
                                    <asp:ImageButton ID="imbtn_manage_eligibility" runat="server" ImageUrl="~/Images/Icons/eligibility_comm.png" style="margin-left:1px; position:relative; top:3px;"/>
                                </td>
                                <td width="28%" valign="top">
                                    <asp:Label runat="server" ForeColor="DarkOrange" Font-Bold="true" Font-Size="Medium" Text="Commission Stats"/>
                                </td>
                            </tr>
                            <tr><td colspan="2" align="center">
                                <asp:Label ID="lbl_form_finalised" runat="server" ForeColor="BurlyWood" Font-Bold="true" Font-Size="13pt"/>
                                <asp:Label ID="lbl_ninety_day_no_threshold" runat="server" Visible="false" ForeColor="BurlyWood" Font-Bold="true" Font-Size="13pt"
                                    Text="<br/>No commission threshold applied to sales added within employee's first 90 days of employment." style="position:relative; left:-100px;"/>
                            </td></tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lbl_form_invoiced_sales" runat="server" ForeColor="AntiqueWhite" Font-Size="Small"/>
                                </td>
                                <td align="right">
                                    <div style="margin:5px 1px 3px 0px;">
                                        <asp:Button ID="btn_email_form" runat="server" Text="E-mail Form" OnClientClick="return confirm('Are you sure you wish to e-mail this form?')" OnClick="EmailForm" style="cursor:pointer; outline:none;"/>
                                        <asp:Button ID="btn_print_form" runat="server" Text="Print Preview" OnClientClick="aspnetForm.target ='_blank';" OnClick="PrintPreviewForm" style="cursor:pointer; outline:none;"/>
                                        <asp:Button ID="btn_finalise_form" runat="server" Text="Finalise Form" style="cursor:pointer; outline:none;" ForeColor="#006200"
                                        OnClick="FinaliseForm" OnClientClick="return confirm('This will permanently finalise this form which will stop it from tracking new sales.\n\nThis means no new commission other than \'Other\' commission can be added to this form.\n\nAre you sure?')"/>
                                    </div>
                                </td>
                            </tr>
                            <tr><td colspan="2" align="center">
                                <asp:Label ID="lbl_form_empty" runat="server" ForeColor="BurlyWood" Font-Size="13pt" Font-Bold="true" Text="<br/>This form contains no sales." style="position:relative; left:-100px;"/>
                            </td></tr>
                            <tr>
                                <td valign="top">
                                    <%--Invoiced Sales--%>
                                    <asp:GridView ID="gv_form_invoiced_sales" runat="server" AutoGenerateColumns="False"
                                    Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                                    AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" Width="884px"
                                    RowStyle-HorizontalAlign="Center" RowStyle-BackColor="LightSteelBlue"
                                    RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                                    OnRowDataBound="gv_form_all_sales_RowDataBound">
                                        <Columns>
                                        <%--0--%><asp:BoundField DataField="ent_id"/>
                                        <%--1--%><asp:BoundField HeaderText="Added" DataField="ent_date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>  
                                        <%--2--%><asp:BoundField HeaderText="Advertiser" DataField="advertiser" ItemStyle-Width="220px"/>
                                        <%--3--%><asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="220px"/>
                                        <%--4--%><asp:BoundField HeaderText="Size" DataField="size" ItemStyle-BackColor="Yellow" ItemStyle-Width="28px"/>
                                        <%--5--%><asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="65px"/>
                                        <%--6--%><asp:BoundField HeaderText="Invoice" DataField="invoice" ItemStyle-Width="45px"/>
                                        <%--7--%><asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/> 
                                        <%--8--%><asp:BoundField HeaderText="Commission" DataField="comm" ItemStyle-Width="60px" />
                                        <%--9--%><asp:BoundField HeaderText="%" DataField="percent" ItemStyle-Width="45px"/>
                                        <%--10--%><asp:BoundField HeaderText="T" DataField="cca_type" ItemStyle-Width="20px"/>
                                        <%--11--%><asp:BoundField HeaderText="DN" DataField="al_rag" ItemStyle-Width="15px"/>
                                        <%--12--%><asp:BoundField DataField="al_notes"/>
                                        <%--13--%><asp:BoundField HeaderText="FN" DataField="fnotes" ItemStyle-Width="15px"/>
                                        <%--14--%><asp:BoundField DataField="rep"/>
                                        <%--15--%><asp:BoundField DataField="list_gen"/>
                                        <%--16--%><asp:BoundField DataField="eligible"/>
                                        <%--17--%><asp:BoundField DataField="is_outstanding"/>
                                        </Columns>
                                    </asp:GridView> 
                                    
                                    <%--Non-Invoiced Sales--%>
                                    <asp:Label ID="lbl_form_noninvoiced_sales" runat="server" ForeColor="AntiqueWhite" Font-Size="Small"/>
                                    <asp:GridView ID="gv_form_noninvoiced_sales" runat="server" AutoGenerateColumns="False"
                                    Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                                    AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" Width="884px"
                                    RowStyle-HorizontalAlign="Center" RowStyle-BackColor="White"
                                    RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                                    OnRowDataBound="gv_form_all_sales_RowDataBound">
                                        <Columns>
                                        <%--0--%><asp:BoundField DataField="ent_id"/>
                                        <%--1--%><asp:BoundField HeaderText="Added" DataField="ent_date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>  
                                        <%--2--%><asp:BoundField HeaderText="Advertiser" DataField="advertiser" ItemStyle-Width="220px"/>
                                        <%--3--%><asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="220px"/>
                                        <%--4--%><asp:BoundField HeaderText="Size" DataField="size" ItemStyle-BackColor="Yellow" ItemStyle-Width="28px"/>
                                        <%--5--%><asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="65px"/>
                                        <%--6--%><asp:BoundField HeaderText="Invoice" DataField="invoice" ItemStyle-Width="45px"/>
                                        <%--7--%><asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/> 
                                        <%--8--%><asp:BoundField HeaderText="Commission" DataField="comm" ItemStyle-Width="60px" />
                                        <%--9--%><asp:BoundField HeaderText="%" DataField="percent" ItemStyle-Width="45px"/>
                                        <%--10--%><asp:BoundField HeaderText="T" DataField="cca_type" ItemStyle-Width="20px"/>
                                        <%--11--%><asp:BoundField HeaderText="DN" DataField="al_rag" ItemStyle-Width="15px"/>
                                        <%--12--%><asp:BoundField DataField="al_notes"/>
                                        <%--13--%><asp:BoundField HeaderText="FN" DataField="fnotes" ItemStyle-Width="15px"/>
                                        <%--14--%><asp:BoundField DataField="rep"/>
                                        <%--15--%><asp:BoundField DataField="list_gen"/>
                                        <%--16--%><asp:BoundField DataField="eligible"/>
                                        </Columns>
                                    </asp:GridView>
                                    
                                    <%--Outstanding Sales--%>
                                    <asp:Label ID="lbl_form_outstanding_sales" runat="server" ForeColor="AntiqueWhite" Font-Size="Small"/>
                                    <asp:GridView ID="gv_form_outstanding_sales" runat="server" AutoGenerateColumns="False"
                                    Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                                    AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" Width="884px"
                                    RowStyle-HorizontalAlign="Center" RowStyle-BackColor="White"
                                    RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                                    OnRowDataBound="gv_form_all_sales_RowDataBound">
                                        <Columns>
                                        <%--0--%><asp:BoundField DataField="ent_id"/>
                                        <%--1--%><asp:BoundField HeaderText="Added" DataField="ent_date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>  
                                        <%--2--%><asp:BoundField HeaderText="Advertiser" DataField="advertiser" ItemStyle-Width="220px"/>
                                        <%--3--%><asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="220px"/>
                                        <%--4--%><asp:BoundField HeaderText="Size" DataField="size" ItemStyle-BackColor="Yellow" ItemStyle-Width="28px"/>
                                        <%--5--%><asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="65px"/>
                                        <%--6--%><asp:BoundField HeaderText="Invoice" DataField="invoice" ItemStyle-Width="45px"/>
                                        <%--7--%><asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/> 
                                        <%--8--%><asp:BoundField HeaderText="Commission" DataField="OutstandingValue" ItemStyle-Width="60px" />
                                        <%--9--%><asp:BoundField HeaderText="%" DataField="Percent" ItemStyle-Width="45px"/>
                                        <%--10--%><asp:BoundField HeaderText="T" DataField="cca_type" ItemStyle-Width="20px"/>
                                        <%--11--%><asp:BoundField HeaderText="DN" DataField="al_rag" ItemStyle-Width="15px"/>
                                        <%--12--%><asp:BoundField DataField="al_notes"/>
                                        <%--13--%><asp:BoundField HeaderText="FN" DataField="fnotes" ItemStyle-Width="15px"/>
                                        <%--14--%><asp:BoundField DataField="rep"/>
                                        <%--15--%><asp:BoundField DataField="list_gen"/>
                                        </Columns>
                                    </asp:GridView> 
                                    
                                    <%--T&D Sales--%>
                                    <asp:Label ID="lbl_form_tnd_sales" runat="server" ForeColor="AntiqueWhite" Font-Size="Small"/>
                                    <asp:GridView ID="gv_form_tnd_sales" runat="server" AutoGenerateColumns="False"
                                    Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                                    AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" Width="884px"
                                    RowStyle-HorizontalAlign="Center" RowStyle-BackColor="White"
                                    RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                                    OnRowDataBound="gv_form_all_sales_RowDataBound">
                                        <Columns>
                                        <%--0--%><asp:BoundField DataField="ent_id"/>
                                        <%--1--%><asp:BoundField HeaderText="Added" DataField="ent_date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>  
                                        <%--2--%><asp:BoundField HeaderText="Advertiser" DataField="advertiser" ItemStyle-Width="220px"/>
                                        <%--3--%><asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="220px"/>
                                        <%--4--%><asp:BoundField HeaderText="Size" DataField="size" ItemStyle-BackColor="Yellow" ItemStyle-Width="28px"/>
                                        <%--5--%><asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="65px"/>
                                        <%--6--%><asp:BoundField HeaderText="Invoice" DataField="invoice" ItemStyle-Width="45px"/>
                                        <%--7--%><asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/> 
                                        <%--8--%><asp:BoundField HeaderText="Commission" DataField="comm" ItemStyle-Width="60px" />
                                        <%--9--%><asp:BoundField HeaderText="%" DataField="percent" ItemStyle-Width="45px"/>
                                        <%--10--%><asp:BoundField HeaderText="T" DataField="cca_type" ItemStyle-Width="20px"/>
                                        <%--11--%><asp:BoundField HeaderText="DN" DataField="al_rag" ItemStyle-Width="15px"/>
                                        <%--12--%><asp:BoundField DataField="al_notes"/>
                                        <%--13--%><asp:BoundField HeaderText="FN" DataField="fnotes" ItemStyle-Width="15px"/>
                                        <%--14--%><asp:BoundField DataField="rep"/>
                                        <%--15--%><asp:BoundField HeaderText="List Gen" DataField="list_gen"/>
                                        </Columns>
                                    </asp:GridView> 
                                    
                                    <asp:UpdatePanel ID="udp_form_notes" runat="server" EnableViewState="false" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">                        
                                                <tr><td><asp:Label ID="lbl_form_notes_title" runat="server" ForeColor="AntiqueWhite" Text="<br/>Form Notes" Font-Size="Small"/></td></tr>
                                                <tr>
                                                    <td>
                                                        <asp:TextBox ID="tb_form_notes" runat="server" TextMode="MultiLine" Height="100px" Width="880px" style="border:solid 1px #be151a;"/>
                                                        <asp:Label ID="lbl_form_notes" runat="server" Visible="false" />
                                                    </td>
                                                </tr>
                                                <tr><td align="right"><telerik:RadButton ID="btn_save_form_notes" runat="server" Skin="Bootstrap" Text="Save Form Notes" OnClick="SaveFormNotes" style="margin:3px 1px 0px 0px;"/></td></tr>
                                            </table>
                                        </ContentTemplate>
                                    </asp:UpdatePanel> 
                                </td>
                                <td valign="top">
                                    <%--Commission Tables--%>
                                    
                                    <%--PAID--%>
                                    <table border="1" cellpadding="0" cellspacing="0" width="100%" bgcolor="white" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                                <asp:Label ID="lbl_form_paid_stats_title" runat="server" Text="Paid" ForeColor="White" style="position:relative; top:-6px; left:-170px;"/>
                                            </td>
                                        </tr> 
                                        <tr ID="tr_form_p_lg_sales_total" runat="server">
                                            <td>Sales</td>
                                            <td><asp:Label ID="lbl_form_p_lg_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_p_s_own_list_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="Own List Sales" ForeColor="Blue"/></td>
                                            <td><asp:Label ID="lbl_form_p_s_own_list_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_p_lg_commission" runat="server">
                                            <td bgcolor="LightSteelBlue">Sales Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_form_p_lg_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_p_s_own_list_commission" runat="server">
                                            <td bgcolor="LightSteelBlue">Sales Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_form_p_s_own_list_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_p_s_list_gen_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="List Gen Sales" ForeColor="Green"/></td>
                                            <td><asp:Label ID="lbl_form_p_s_list_gen_sales_total" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_p_s_list_gen_commission" runat="server">
                                            <td bgcolor="LightSteelBlue">Sales Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_form_p_s_list_gen_commission" runat="server"/></td>
                                        </tr>  
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Outstanding Payments</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_form_p_outstanding" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue"><asp:Label runat="server" Text="T&D Commission"/></td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_form_p_tnd" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Other</td>
                                            <td bgcolor="LightSteelBlue" style="white-space:nowrap;">
                                                <asp:TextBox ID="tb_form_p_other" runat="server" Width="50"/>
                                                <asp:Label ID="lbl_form_p_other" runat="server" Visible="false"/>
                                                <telerik:RadButton ID="btn_form_save_other" runat="server" Text="Save Other" OnClick="SaveFormOther" Skin="Bootstrap" CssClass="ShortBootstrapRadButton"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_form_p_other" 
                                                    Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="<br/>Must be a valid number!"/> 
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="50%"><asp:Label ID="lbl_form_p_hit_commission_value" runat="server"/></td>
                                            <td><asp:Label ID="lbl_form_p_hit_commission" runat="server" Font-Bold="true"/></td>
                                        </tr>
                                        <tr>
                                            <td width="50%" bgcolor="DarkOrange">Total Payable</td>
                                            <td bgcolor="DarkOrange"><asp:Label ID="lbl_form_p_total_commission" runat="server" Font-Bold="true"/></td>
                                        </tr>
                                    </table>
                                    
                                     <%--TO BE PAID--%>
                                    <table border="1" cellpadding="0" cellspacing="0" width="100%" bgcolor="white" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                                <asp:Label ID="lbl_form_tbp_stats_title" runat="server" Text="To Be Paid" ForeColor="White" style="position:relative; top:-6px; left:-170px;"/>
                                            </td>
                                        </tr> 
                                        <tr ID="tr_form_tbp_lg_sales_total" runat="server">
                                            <td>Sales</td>
                                            <td><asp:Label ID="lbl_form_tbp_lg_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_tbp_s_own_list_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="Own List Sales" ForeColor="Blue"/></td>
                                            <td><asp:Label ID="lbl_form_tbp_s_own_list_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_tbp_lg_commission" runat="server">
                                            <td>Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_tbp_lg_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_tbp_s_own_list_commission" runat="server">
                                            <td>Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_tbp_s_own_list_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_tbp_s_list_gen_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="List Gen Sales" ForeColor="Green"/></td>
                                            <td><asp:Label ID="lbl_form_tbp_s_list_gen_sales_total" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_tbp_s_list_gen_commission" runat="server">
                                            <td>Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_tbp_s_list_gen_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_tbp_os_commission" runat="server">
                                            <td>Outstanding</td>
                                            <td><asp:Label ID="lbl_form_tbp_outstanding" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_tbp_tnd_commission" runat="server">
                                            <td><asp:Label runat="server" Text="T&D Commission"/></td>
                                            <td><asp:Label ID="lbl_form_tbp_tnd" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td width="50%"><asp:Label ID="lbl_form_tbp_total_commission_title" runat="server" Text="Total Comm. Remaining"/></td>
                                            <td><asp:Label ID="lbl_form_tbp_total_commission" runat="server"/></td>
                                        </tr>
                                    </table>
                                   
                                    <%--TOTAl--%>
                                    <table border="1" cellpadding="0" cellspacing="0" width="100%" bgcolor="white" style="font-family:Verdana; font-size:8pt;"> 
                                        <tr>
                                            <td align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                                <asp:Label ID="lbl_form_total_stats_title" runat="server" Text="Total" ForeColor="White" style="position:relative; top:-6px; left:-170px;"/>
                                            </td>
                                        </tr> 
                                        <tr ID="tr_form_t_lg_sales_total" runat="server">
                                            <td>Total Sales</td>
                                            <td><asp:Label ID="lbl_form_t_lg_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_t_s_own_list_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="Total Own List Sales" ForeColor="Blue"/></td>
                                            <td><asp:Label ID="lbl_form_t_s_own_list_sales_total" runat="server"/></td>
                                        </tr>
                                        <tr ID="tr_form_t_lg_commission" runat="server">
                                            <td>Total Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_t_lg_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_t_s_own_list_commission" runat="server">
                                            <td>Total Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_t_s_own_list_commission" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_t_s_list_gen_sales_total" runat="server">
                                            <td><asp:Label runat="server" Text="Total List Gen Sales" ForeColor="Green"/></td>
                                            <td><asp:Label ID="lbl_form_t_s_list_gen_sales_total" runat="server"/></td>
                                        </tr>  
                                        <tr ID="tr_form_t_s_list_gen_commission" runat="server">
                                            <td>Total Sales Commission</td>
                                            <td><asp:Label ID="lbl_form_t_s_list_gen_commission" runat="server"/></td>
                                        </tr>  
                                        <tr>
                                            <td>Total Outstanding</td>
                                            <td><asp:Label ID="lbl_form_t_outstanding" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label runat="server" Text="Total T&D Commission"/></td>
                                            <td><asp:Label ID="lbl_form_t_tnd" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td width="50%">Total Commission</td>
                                            <td><asp:Label ID="lbl_form_t_total_commission" runat="server"/></td>
                                        </tr>
                                    </table>
                                    
                                    <br />
                                    <%--OFFICE PARTICIPATION--%>
                                    <table id="tbl_form_office_participation" runat="server" border="1" cellpadding="0" cellspacing="0" width="100%" bgcolor="white" 
                                        style="font-family:Verdana; font-size:8pt;"> 
                                        <tr>
                                            <td align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                                <asp:Label ID="lbl_form_offices" runat="server" Text="Contributing Offices" ForeColor="White" 
                                                style="position:relative; top:-6px; left:-170px;"/>
                                            </td>
                                        </tr> 
                                        <tr>
                                            <td width="50%"><b>Office Name:</b></td>
                                            <td width="50%"><b>Viewable in Office:</b></td>
                                        </tr>
                                    </table>
                                   
                                </td>
                            </tr>
                        </table>
                        <%--End Form Table--%>
                    </div>
                    <%--End Form Div--%>
                    
                </td>
            </tr>
        </table>
        
    <hr />
    </div>
   
    <script type="text/javascript">
        function OnClientCloseHandler(sender, args) {
            Refresh();
        }
        function Refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.click();
            return true;
        }
    </script> 
</asp:Content>