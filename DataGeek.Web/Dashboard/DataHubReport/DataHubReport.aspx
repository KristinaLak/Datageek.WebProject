<%--
Author   : Joe Pickering, 04/10/2010 - re-written 28/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: DataHub Report" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="DataHubReport.aspx.cs" Inherits="DataHubReport" %>
 
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">   

    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="position:relative; left:6px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="DataHub" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="bottom">
                    <table cellpadding="0">
                        <tr>
                            <td><asp:Label runat="server" ForeColor="White" Text="From:"/></td>
                            <td><telerik:RadDatePicker ID="dp_start" width="118px" runat="server"/></td>
                            <td><asp:Label runat="server" ForeColor="White" Text="To:"/></td>
                            <td><telerik:RadDatePicker ID="dp_end" width="118px" runat="server"/></td>
                            <td valign="bottom">&nbsp;<asp:Button runat="server" Text="Show" OnClick="ChangeDates"/>&nbsp;</td>
                            <td valign="bottom">
                                <asp:Label runat="server" ForeColor="White" Font-Size="7pt" 
                                Text="Note: highlight a large rectangle over an area of a chart to zoom."
                                style="position:relative; top:-2px;"/>
                            </td>
                        </tr>
                    </table>
                </td>
                <td width="20%" align="right" valign="bottom">
                    <asp:Button runat="server" ID="btn_reset" Text="Reset Graphs" OnClick="ResetZoom"/>
                </td>
            </tr>
            <tr>
                <td align="center" colspan="2">
                    <%--Companies Chart--%> 
                    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                        <Triggers><asp:AsyncPostBackTrigger ControlID="rc_companies"/></Triggers>
                        <ContentTemplate>
                            <telerik:RadChart ID="rc_companies" runat="server" IntelligentLabelsEnabled="false"  
                            Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="980px"
                            PlotArea-YAxis-Appearance-CustomFormat="###,###">
                            <ClientSettings ScrollMode="Both" />
                            </telerik:RadChart>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>   
            <tr>
                <td align="center" colspan="2">
                    <%--Contacts Chart--%> 
                    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                        <Triggers><asp:AsyncPostBackTrigger ControlID="rc_contacts"/></Triggers>
                        <ContentTemplate>
                            <telerik:RadChart ID="rc_contacts" runat="server" IntelligentLabelsEnabled="false"  
                            Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="980px"
                            PlotArea-YAxis-Appearance-CustomFormat="###,###">
                            <ClientSettings ScrollMode="Both" />
                            </telerik:RadChart>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>   
            <tr>
                <td align="center" colspan="2">
                    <%--Emails Chart--%> 
                    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                        <Triggers><asp:AsyncPostBackTrigger ControlID="rc_emails"/></Triggers>
                        <ContentTemplate>
                            <telerik:RadChart ID="rc_emails" runat="server" IntelligentLabelsEnabled="false"  
                            Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="980px"
                            PlotArea-YAxis-Appearance-CustomFormat="###,###">
                            <ClientSettings ScrollMode="Both" />
                            </telerik:RadChart>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>   
            <tr>
                <td align="left" valign="top">
                    <div id="div_gv" runat="server" style="display:none;">
                        <asp:GridView border="2" ID="gv_notes" runat="server" 
                            AlternatingRowStyle-BackColor="#ff0000" 
                            RowStyle-HorizontalAlign="Left" 
                            HeaderStyle-HorizontalAlign="Center" 
                            AutoGenerateColumns="false" 
                            ForeColor="Black">
                            <Columns>
                                <asp:BoundField ItemStyle-Width="100px" HeaderText="Date" DataField="date" DataFormatString="{0:dd/MM/yyyy}"/>
                                <asp:BoundField ItemStyle-Width="300px" HeaderText="Notes" DataField="notes"/>
                            </Columns>
                            <AlternatingRowStyle BackColor="LightSteelBlue"></AlternatingRowStyle>
                            <HeaderStyle BackColor="#444444" ForeColor="White"></HeaderStyle>
                            <RowStyle BackColor="#f0f0f0"></RowStyle>
                        </asp:GridView>
                    </div>
                </td>
                <td align="right" valign="top">
                    <asp:LinkButton runat="server" ID="lb_showhidenotes" Text="Show Activity History" 
                    OnClientClick="return showHideNotes();" style="position:relative; left:-2px;"/>
                </td>
            </tr>
        </table>
        <hr />
    </div>

    <script type="text/javascript">
        function showHideNotes(){ 
            if(grab('<%= lb_showhidenotes.ClientID %>').innerText == "Show Activity History"){
                grab('<%= div_gv.ClientID %>').style.display = "block";
                grab('<%= lb_showhidenotes.ClientID %>').innerText = "Hide Activity History";
            }
            else{
                grab('<%= div_gv.ClientID %>').style.display = "none";
                grab('<%= lb_showhidenotes.ClientID %>').innerText = "Show Activity History";
            }
            return false;
        }
    </script>
</asp:Content>

