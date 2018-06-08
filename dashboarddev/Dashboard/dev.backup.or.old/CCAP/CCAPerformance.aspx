<%--
Author   : Joe Pickering, 06/03/2012
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="CCAPerformance.aspx.cs" Inherits="CCAPerformance" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
 
<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" Runat="Server">
    <title>DataGeek :: CCA Performance</title>
</asp:Content>

<%--Body--%>
<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadToolTipManager ID="rttm" runat="server" Animation="Resize" ShowDelay="40" AutoTooltipify="true" ShowEvent="OnRightClick"
     ManualClose="true" RelativeTo="Element" Sticky="true" OffsetY="-5" EnableEmbeddedSkins="True" Skin="Vista" />

    <div id="div_page" runat="server" class="normal_page">   
    <hr/>
    
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td>
                    <asp:Label runat="server" Text="CCA" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Performance" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCCAs"/>
                    <asp:DropDownList ID="dd_cca" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Bind"/>
                    <br /><br />
                </td>
            </tr>
            <tr>    
                <td style="border-bottom:solid 1px gray;">
                    <telerik:RadTabStrip ID="rts" runat="server" AutoPostBack="true" MaxDataBindDepth="1" SelectedIndex="0" OnTabClick="Bind"
                        BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:2px;"> <%--Width="300px"--%>
                        <Tabs>
                            <telerik:RadTab id="rt_sparev" runat="server" Text="SPA/Revenue"/>
                            <telerik:RadTab id="rt_rsg" runat="server" Text="RSG"/>
                            <telerik:RadTab id="rt_lts" runat="server" Text="LTS"/>
                            <telerik:RadTab id="rt_salesbook" runat="server" Text="Sales"/>
                            <telerik:RadTab id="rt_prospects" runat="server" Text="Prospects"/>
                            <telerik:RadTab id="rt_commission" runat="server" Text="Commission"/>
                            <telerik:RadTab id="rt_sick" runat="server" Text="Sick Tracker"/>
                        </Tabs>
                    </telerik:RadTabStrip>
                </td>
            </tr>
            <tr>
                <td>
                    <div id="div_pages" runat="server">
                        <div id="div_sparev" runat="server">    
                            <table>
                                <%--Top Quadrant Titles--%>
                                <tr>
                                    <td style="height:18px;"><asp:Label runat="server" ForeColor="DarkOrange" Text="SPA Table"/></td>
                                    <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Statistics"/></td>
                                </tr>
                                <tr>
                                    <td valign="top" width="50%" align="center">
                                        <asp:GridView id="gv_spa" runat="server" RowStyle-CssClass="gv_hover"
                                            AutoGenerateColumns="false" AllowSorting="true"
                                            Font-Name="Verdana" Font-Size="8pt" Cellpadding="1" Border="1" Width="488"
                                            HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" 
                                            RowStyle-HorizontalAlign="Center" RowStyle-BackColor="Moccasin"
                                            OnSorting="gv_spa_Sorting" OnRowDataBound="gv_spa_RowDataBound">
                                            <Columns>
                                                <asp:HyperLinkField HeaderText="Week Start" SortExpression="start_date" DataTextField="start_date" ItemStyle-Width="84px" ControlStyle-ForeColor="Black" ItemStyle-BackColor="Khaki"
                                                DataTextFormatString="{0:dd/MM/yyyy}" DataNavigateUrlFormatString="~/Dashboard/PRInput/PRInput.aspx?r_id={0}" DataNavigateUrlFields="r_id"/>
                                                <asp:BoundField HeaderText="S" ReadOnly="true" SortExpression="Suspects" DataField="Suspects" ItemStyle-Width="46px"/>          
                                                <asp:BoundField HeaderText="P" ReadOnly="true" SortExpression="Prospects"  DataField="Prospects" ItemStyle-Width="46px"/> 
                                                <asp:BoundField HeaderText="A" ReadOnly="true" SortExpression="Approvals" DataField="Approvals" ItemStyle-Width="46px"/>
                                                <asp:BoundField HeaderText="S:A" ReadOnly="true" SortExpression="StoA" DataField="StoA" ItemStyle-Width="40px"/>
                                                <asp:BoundField HeaderText="P:A" ReadOnly="true" SortExpression="PtoA" DataField="PtoA" ItemStyle-Width="40px"/> 
                                                <asp:BoundField HeaderText="TR" ReadOnly="true" SortExpression="TR"  DataField="TR" ItemStyle-Width="75px"/>
                                                <asp:BoundField HeaderText="PR" ReadOnly="true" SortExpression="PR"  DataField="PR" ItemStyle-Width="75px"/>
                                                <asp:BoundField HeaderText="%" ReadOnly="true" SortExpression="RAG" DataField="RAG" ItemStyle-Width="35px"/>
                                            </Columns>
                                        </asp:GridView>
                                    </td>
                                    <td valign="top" width="50%">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td><asp:Label runat="server" ForeColor="DarkOrange" Text="SPA Percentage of Goal" Font-Size="12pt"/></td>
                                                <td><asp:Label runat="server" ForeColor="PowderBlue" Text="47%" Font-Size="26pt"/></td>
                                            </tr>
                                            <tr>
                                                <td width="55%"><asp:Label runat="server" ForeColor="DarkOrange" Text="Revenue Percentage of Goal" Font-Size="12pt"/></td>
                                                <td><asp:Label runat="server" ForeColor="PowderBlue" Text="84%" Font-Size="26pt"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Overall Percentage of Goal" Font-Size="12pt"/></td>
                                                <td><asp:Label runat="server" ForeColor="PowderBlue" Text="65.5%" Font-Size="26pt"/></td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr><td colspan="2" style="height:5px; border-bottom: dotted 2px gray;"></td></tr>
                                <%-- Bottom Quadrant Titles --%>
                                <tr>
                                    <td style="height:18px;"><asp:Label runat="server" ForeColor="DarkOrange" Text="SPA Graph"/></td>
                                    <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Revenue Graph"/></td> <%----%>
                                </tr>
                                <tr>
                                    <td valign="top" align="center"> 
                                        <telerik:RadChart ID="rc_spa" runat="server" 
                                            Autolayout="True" 
                                            SkinsOverrideStyles="False" Skin="Web20" IntelligentLabelsEnabled="true"    
                                            Height="300px" Width="488px"> 
                                        </telerik:RadChart>   
                                    </td>
                                    <td valign="top" align="center">
                                        <telerik:RadChart ID="rc_rev" runat="server" 
                                            Autolayout="True" 
                                            SkinsOverrideStyles="False" Skin="Web20" IntelligentLabelsEnabled="true"  
                                            Height="300px" Width="488px"> 
                                        </telerik:RadChart>   
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div id="div_rsg" runat="server">
                            <asp:Label runat="server" Text="RSG" ForeColor="DarkOrange" />
                        </div>
                        <div id="div_lts" runat="server">
                            <asp:Label runat="server" Text="LTS" ForeColor="DarkOrange" />
                        </div>
                        <div id="div_sales" runat="server">    
                            <asp:Label runat="server" Text="Latest " ForeColor="DarkOrange" />
                            <asp:DropDownList ID="dd_sales_latest" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Bind">
                                <asp:ListItem Text="5"/>
                                <asp:ListItem Text="10"/>
                                <asp:ListItem Text="15"/>
                                <asp:ListItem Text="25" Selected="true"/>
                                <asp:ListItem Text="50"/>
                                <asp:ListItem Text="100"/>
                            </asp:DropDownList>
                            
                            <asp:GridView ID="gv_s" runat="server" 
                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="986px" RowStyle-CssClass="gv_hover"
                            HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" AutoGenerateColumns="false"
                            RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                            OnRowDataBound="gv_s_RowDataBound">
                            <Columns> 
                            <%--0--%><asp:BoundField HeaderText="SD" ItemStyle-BackColor="SandyBrown" DataField="sale_day" SortExpression="sale_day" ItemStyle-Width="30px" ControlStyle-Width="24px"/>
                            <%--1--%><asp:BoundField HeaderText="Date Added" DataField="ent_date" SortExpression="ent_date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                            <%--2--%><asp:BoundField HeaderText="Advertiser" HtmlEncode="false" ItemStyle-Font-Bold="true" DataField="Advertiser" SortExpression="Advertiser" ItemStyle-Width="200px" ControlStyle-Width="185px"/>
                            <%--3--%><asp:BoundField HeaderText="Feature" HtmlEncode="false" ItemStyle-Font-Bold="true" ItemStyle-BackColor="Plum" DataField="feature" SortExpression="feature" ItemStyle-Width="200px" ControlStyle-Width="185px"/>
                            <%--4--%><asp:BoundField HeaderText="Size" ItemStyle-BackColor="Yellow" DataField="size" SortExpression="size" ItemStyle-Width="30px" ControlStyle-Width="30px"/>
                            <%--5--%><asp:BoundField HeaderText="Price" DataField="price" SortExpression="price" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                            <%--6--%><asp:BoundField HeaderText="Rep" DataField="rep" SortExpression="rep" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                            <%--7--%><asp:BoundField HeaderText="Info" HtmlEncode="false" DataField="info" SortExpression="info" ItemStyle-Width="100px" ControlStyle-Width="100px"/>
                            <%--8--%><asp:BoundField HeaderText="List Gen" DataField="list_gen" SortExpression="list_gen" ItemStyle-Width="100px" ControlStyle-Width="80px"/>
                            <%--9--%><asp:BoundField HeaderText="Invoice" DataField="invoice" SortExpression="invoice" ItemStyle-Width="100px" ControlStyle-Width="80px"/>
                            <%--10--%><asp:BoundField HeaderText="Date Paid" DataField="date_paid" SortExpression="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                            <%--11--%><asp:BoundField HeaderText="DN" DataField="al_notes" />
                            <%--12--%><asp:BoundField HeaderText="FN" DataField="fnotes"/>
                            <%--13--%><asp:BoundField HeaderText="RAG" DataField="al_rag"/>
                            <%--14--%><asp:BoundField DataField="Deleted"/>
                            </Columns>
                        </asp:GridView>  
                        </div>
                        <div id="div_prospects" runat="server">
                            <asp:Label runat="server" Text="PROSPECTS" ForeColor="DarkOrange" />
                            <asp:DropDownList ID="dd_prospects_latest" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Bind">
                                <asp:ListItem Text="5"/>
                                <asp:ListItem Text="10"/>
                                <asp:ListItem Text="15"/>
                                <asp:ListItem Text="25" Selected="true"/>
                                <asp:ListItem Text="50"/>
                                <asp:ListItem Text="100"/>
                            </asp:DropDownList>
                        </div>
                        <div id="div_commission" runat="server">
                            <asp:Label runat="server" Text="COMMISSION" ForeColor="DarkOrange" />
                        </div>
                        <div id="div_sicktracker" runat="server">
                            <asp:Label runat="server" Text="SICK TRACKER" ForeColor="DarkOrange" />
                        </div>
                    </div>
                </td>
            </tr>

        </table>
        
        <hr />
    </div>
</asp:Content>