<%--
// Author   : Joe Pickering, 02/02/2011 - re-written 03/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Performance Report" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="PerformanceReport.aspx.cs" Inherits="PerformanceReport" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
<telerik:RadWindowManager ID="rwm" VisibleStatusBar="false" 
    UseClassicWindows="true" ReloadOnShow="false" runat="server" > 
    <Windows>
        <telerik:RadWindow runat="server" ID="win_newscheme" Title="&nbsp;New Scheme"
            Width="660" Height="370" ClientCallBackFunction="NewSchemeWCB"
            Behaviors="Move, Close, Pin" NavigateUrl="PerfRNewScheme.aspx"/>
    </Windows>
</telerik:RadWindowManager>
   
    <div id="div_page" runat="server" class="normal_page">
        <hr />
            
            <table width="200" cellpadding="0" cellspacing="0" style="background-color: #9C084A; border-radius:15px; margin-left:auto; margin-right:auto;">
                <tr>
                    <td align="center">L</td>
                    <td align="center">CONT</td>
                    <td align="center">R</td>
                </tr>
            </table>
            
            <table width="99%" cellpadding="1" cellspacing="0" style="font-family:Verdana; margin-left:auto; margin-right:auto;">
                <tr>
                    <td align="left" valign="top" colspan="2">
                        <asp:Label runat="server" ID="lbl_p" Text="Performance" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium"/>
                        <p> 
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                            <asp:DropDownList runat="server" ID="dd_office" AutoPostBack="true" OnSelectedIndexChanged="AddDropDownSchemes"/>
                            <asp:DropDownList runat="server" ID="dd_schemes" AutoPostBack="true" Width="150" OnSelectedIndexChanged="BindData"/>
                            <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png" OnClick="BindAll" style="position:relative; top:4px;"/> 
                            <asp:LinkButton ForeColor="Silver" runat="server" Text="New Scheme" OnClientClick="radopen(null, 'win_newscheme'); return false;"/>
                        <hr />
                    </td>
                </tr>
                <tr>
                    <td>
                        <table runat="server" id="tbl_schemes" border="0" cellpadding="2">
                            <tr>
                                <td valign="bottom">
                                    <asp:Label runat="server" ID="lbl_currencscheme" Text="" ForeColor="White" Font-Bold="false" Font-Size="Small"/>
                                </td>
                                <td valign="bottom" align="left">
                                    <asp:Label runat="server" Text="Scheme Stages" ForeColor="White" Font-Bold="false" Font-Size="Small"/>
                                    <asp:CheckBox runat="server" ID="cb_showcompletesteps" Text="Show Completed Stages" Checked="true" AutoPostBack="true" OnCheckedChanged="BindData"/>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top" width="50%" bgcolor="gray" style="border: solid 2px red; border-radius:5px;">   
                                    <asp:GridView ID="gv_schemes" runat="server" EnableViewState="true"  Width="485"
                                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2"
                                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" RowStyle-ForeColor="Black" 
                                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                                        AutoGenerateColumns="false"
                                        OnRowDataBound="gv_schemes_RowDataBound"
                                        OnRowEditing="gv_RowEditing"
                                        OnRowCancelingEdit="gv_RowCancelingEdit"
                                        OnRowUpdating="gv_schemes_RowUpdating">
                                        <Columns>
                                            <asp:CommandField ItemStyle-BackColor="White" 
                                                ShowEditButton="true"
                                                ShowDeleteButton="false"
                                                ButtonType="Image"
                                                EditImageUrl="~\Images\Icons\gridView_Edit.png"
                                                CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                                                UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>
                                            <asp:BoundField DataField="scheme_id"/>
                                            <asp:BoundField HeaderText="Scheme Name" DataField="scheme_name" ControlStyle-Width="80"/>
                                            <asp:BoundField HeaderText="Start Date" DataField="start_date" DataFormatString="{0:dd/MM/yyyy}" ControlStyle-Width="80" />
                                            <asp:BoundField HeaderText="CCA Type" DataField="scheme_type" ControlStyle-Width="50"/>
                                            <asp:BoundField HeaderText="S" DataField="step_s" ControlStyle-Width="20"/>
                                            <asp:BoundField HeaderText="P" DataField="step_p" ControlStyle-Width="20"/>
                                            <asp:BoundField HeaderText="A" DataField="step_a" ControlStyle-Width="20"/>
                                            <asp:BoundField HeaderText="Revenue" DataField="step_rev"/>
                                            <asp:TemplateField HeaderText="Active"> 
                                              <ItemTemplate>
                                                <asp:CheckBox runat="server" Checked='<%# Server.HtmlEncode(Eval("active").ToString()).Equals("True") %>' OnCheckedChanged="gv_schemes_updateActive" AutoPostBack="true"/>
                                              </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </td>
                                <td valign="top" width="50%" bgcolor="gray" style="border-radius:5px;">
                                    <asp:GridView ID="gv_schemesteps" runat="server" EnableViewState="true" Width="490"
                                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2"
                                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" RowStyle-ForeColor="Black" 
                                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                                        AutoGenerateColumns="false"
                                        OnRowDataBound="gv_schemesteps_RowDataBound"
                                        OnRowEditing="gv_RowEditing"
                                        OnRowCancelingEdit="gv_RowCancelingEdit"
                                        OnRowUpdating="gv_schemesteps_RowUpdating">
                                        <Columns>
                                            <asp:CommandField ItemStyle-BackColor="White" 
                                                ShowEditButton="true"
                                                ShowDeleteButton="false"
                                                ButtonType="Image"
                                                EditImageUrl="~\Images\Icons\gridView_Edit.png"
                                                CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                                                UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>
                                            <asp:BoundField DataField="step_id"/>
                                            <asp:BoundField Visible="false" DataField="scheme_id"/>
                                            <asp:BoundField Visible="false" HeaderText="Scheme Name" DataField="scheme_name"/>
                                            <asp:BoundField HeaderText="Stage No." DataField="step_no" ControlStyle-Width="40px"/>
                                            <asp:HyperLinkField ItemStyle-Width="70px" ControlStyle-ForeColor="Blue" HeaderText="Start Date" DataTextField="start_date" 
                                            DataTextFormatString="{0:dd/MM/yyyy}" DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PRInput/PRInput.aspx?r_id={0}" DataNavigateUrlFields="r_id"/>
                                            <asp:CheckBoxField HeaderText="Reccuring" DataField="recurring"/>
                                            <asp:BoundField HeaderText="Recur" DataField="recur"/>
                                            <asp:BoundField HeaderText="Stage Duration" DataField="num_weeks"/>
                                            <asp:BoundField HeaderText="Overall Duration" DataField="duration_weeks"/>
                                            <asp:CheckBoxField HeaderText="Complete" DataField="complete"/>
                                        </Columns>
                                    </asp:GridView>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">    
                                    <br /><br />
                                    <hr /> 
                                    <asp:Label runat="server" Text="Completed Stages - Underperformers" ForeColor="White" Font-Bold="false" Font-Size="Small"/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">    
                                    <asp:CheckBox runat="server" ID="cb_showoverperformers" Text="Include Overperformers" Checked="false" AutoPostBack="true" OnCheckedChanged="BindData" />
                                    <asp:Button runat="server" ID="btn_email" OnClick="SendEmail" Text="Send Email" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" valign="top">
                                    <div runat="server" ID="div_stats"/>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        <hr />
    </div>
    
    <script type="text/javascript">
        function NewSchemeWCB(radWindow, returnValue) {
            grab("<%= imbtn_refresh.ClientID %>").click();
            alert(returnValue);
        }
    </script>
</asp:Content>
