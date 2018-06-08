<%--
Author   : Joe Pickering, 25/11/2010 - re-written 06/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Collections" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Collections.aspx.cs" Inherits="Collections" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <telerik:RadToolTipManager ID="rttm" Visible="True" Animation="Resize" ShowDelay="40" Title="<i><font color='Black' size='2'>Collections Notes:</font></i>" 
     ManualClose="true" RelativeTo="Element" Sticky="true" OffsetY="-5" OffsetX="-100" EnableEmbeddedSkins="True" Skin="Vista" Width="400" ShowEvent="OnRightClick" runat="server">
    </telerik:RadToolTipManager>
    
    <div id="div_page" runat="server" class="wide_page">   
        <hr />
        <table width="100%">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Collections" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:6px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:6px;"/> 
                </td>
                <td valign="top" align="right">
                    <asp:Label runat="server" Text="Select Issue:" ForeColor="DarkOrange" style="position:relative; top:-4px;"/>
                    <asp:DropDownList ID="dd_book" runat="server" Width="150px" AutoPostBack="true" style="position:relative; top:-4px;"/>
                </td>
            </tr>
        </table>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td>
                    <table runat="server" ID="tbl_summary" bgcolor="gray" style="font-family:Verdana; color:White; border:solid 2px darkgray; border-radius:5px;">
                        <tr>
                            <td>Outstanding Sales:</td>
                            <td><asp:Label runat="server" ID="lbl_count"/></td>
                        </tr>
                        <tr>
                            <td valign="top">Avg:</td>
                            <td><asp:Label runat="server" ID="lbl_avg"/></td>
                        </tr>
                        <tr>
                            <td valign="top">Total:</td>
                            <td><asp:Label runat="server" ID="lbl_total"/></td>
                        </tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td>
                    <telerik:RadTabStrip ID="tabstrip" AutoPostBack="true" MaxDataBindDepth="1" runat="server" MultiPageID="multiPage" SelectedIndex="0"
                        BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:2px;">
                    </telerik:RadTabStrip>
                </td>
                <td valign="bottom" align="right">
                    <table cellpadding="0" cellspacing="0">
                        <tr>
                            <td><asp:CheckBox ID="cb_onlyredlines" runat="server" Text="Show Red-Lines" Visible="true" Checked="false" AutoPostBack="true" ForeColor="White" Font-Size="8pt"/></td>
                            <td><asp:CheckBox ID="cb_invoice" runat="server" Text="Hide with Invoice" Checked="false" AutoPostBack="true" ForeColor="White" Font-Size="8pt"/></td>
                            <td><asp:CheckBox ID="cb_notes" runat="server" Text="Hide with Notes" Checked="false" AutoPostBack="true" ForeColor="White" Font-Size="8pt"/></td>
                            <td><asp:CheckBox ID="cb_shownotes" runat="server" Text="Expand Notes&nbsp;" Checked="false" AutoPostBack="true" ForeColor="White" Font-Size="8pt"/></td>
                            <td>                    
                                <telerik:RadTabStrip runat="server" ID="rts_sbview" AutoPostBack="false" MaxDataBindDepth="1" SelectedIndex="0"
                                    BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" OnTabClick="BindGrid" style="position:relative; top:1px;">
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
            <tr>
                <td colspan="2">  <%--CssClass="BlackGridHead" RowStyle-CssClass="gv_hover"  --%>
                    <asp:GridView ID="gv_s" runat="server" EnableViewState="true" 
                        OnSorting="gv_Sorting" AllowPaging="false" 
                        OnRowEditing="gv_RowEditing"                   
                        OnRowCancelingEdit="gv_RowCancelingEdit" 
                        OnRowUpdating="gv_RowUpdating"
                        OnRowDataBound="gv_RowDataBound"
                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="1238px"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" AllowSorting="true"
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                        RowStyle-BackColor="#f0f0f0" AutoGenerateColumns="False" PagerStyle-BackColor="#f0f0f0" 
                        PagerSettings-Position="TopAndBottom" PagerStyle-ForeColor="Black" OnPageIndexChanging="gv_PageIndexChanging"
                        PageIndex="0" PageSize="40" style="font-family:Verdana;">    
                        <Columns> 
                            <%--0--%><asp:CommandField ItemStyle-BackColor="White" ItemStyle-Width="18"
                                ShowEditButton="true"
                                ShowDeleteButton="false"
                                ButtonType="Image"
                                EditImageUrl="~\Images\Icons\gridView_Edit.png"
                                CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                                UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>
                            <%--1--%><asp:BoundField DataField="ent_id"/>
                            <%--2--%><asp:BoundField DataField="sb_id"/>
                            <%--3--%><asp:BoundField HeaderText="SD" ItemStyle-BackColor="SandyBrown" DataField="sale_day" SortExpression="sale_day" ItemStyle-Width="30px"/>
                            <%--4--%><asp:BoundField HeaderText="Date Added" DataField="ent_date" SortExpression="ent_date" DataFormatString = "{0:dd/MM/yyyy}" ItemStyle-Width="68px" ControlStyle-Width="68px"/>
                            <%--5--%><asp:BoundField HeaderText="Advertiser" ItemStyle-Font-Bold="true" DataField="Advertiser" SortExpression="Advertiser" ItemStyle-Width="300px"/>
                            <%--6--%><asp:BoundField HeaderText="Feature" ItemStyle-Font-Bold="true" ItemStyle-BackColor="Plum" DataField="feature" SortExpression="feature" ItemStyle-Width="300px"/>
                            <%--7--%><asp:BoundField HeaderText="Size" ItemStyle-BackColor="Yellow" DataField="size" SortExpression="size" ItemStyle-Width="30px"/>
                            <%--8--%><asp:BoundField HeaderText="Price" DataField="price" SortExpression="price" ItemStyle-Width="60px"/>
                            <%--9--%><asp:BoundField HeaderText="Info" DataField="info" SortExpression="info" ItemStyle-Width="100px"/>
                            <%--10--%><asp:BoundField HeaderText="Channel" DataField="channel_magazine" SortExpression="channel_magazine" ItemStyle-Width="120px"/>
                            <%--11--%><asp:BoundField HeaderText="Invoice" SortExpression="Invoice" DataField="invoice" ItemStyle-Width="60px" ControlStyle-Width="60px"/>
                            <%--12--%><asp:BoundField HeaderText="Rep" SortExpression="Rep" DataField="Rep" ItemStyle-Width="50px"/>
                            <%--13--%><asp:BoundField HeaderText="List Gen" SortExpression="list_gen" DataField="list_gen" ItemStyle-Width="50px"/>
                            <%--14--%><asp:BoundField HeaderText="Date Paid" SortExpression="date_paid" DataField="date_paid" ItemStyle-Width="80px"/>
                            <%--15--%><asp:BoundField HeaderText="Office" SortExpression="centre" DataField="centre" ItemStyle-Width="80px"/>
                            <%--16--%><asp:BoundField HeaderText="Contact" SortExpression="al_contact" DataField="al_contact" ItemStyle-Width="110px" ControlStyle-Width="110px"/>
                            <%--17--%><asp:BoundField HeaderText="E-mail" SortExpression="al_email" DataField="al_email" ItemStyle-Width="190px" ControlStyle-Width="190px"/>
                            <%--18--%><asp:BoundField HeaderText="Tel" SortExpression="al_tel" DataField="al_tel" ItemStyle-Width="100px" ControlStyle-Width="160px"/>
                            <%--19--%><asp:BoundField HeaderText="Mobile" SortExpression="al_mobile" DataField="al_mobile" ItemStyle-Width="100px" ControlStyle-Width="160px"/>
                            <%--20--%><asp:BoundField HeaderText="Deadline" SortExpression="al_deadline" DataField="al_deadline"  DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="68px" ControlStyle-Width="68px"/>
                            <%--21--%><asp:BoundField HeaderText="Ad Notes" SortExpression="al_notes" DataField="al_notes"/>
                            <%--22--%><asp:TemplateField HeaderText="AM" SortExpression="al_admakeup" ItemStyle-Width="20px" ControlStyle-Width="20px"> 
                              <ItemTemplate>
                                <asp:CheckBox runat="server" Enabled="false" Checked='<%# Server.HtmlEncode(Eval("al_admakeup").ToString()).Equals("True") %>'/>
                              </ItemTemplate>
                            </asp:TemplateField>
                            <%--23--%><asp:TemplateField HeaderText="Links" SortExpression="links" ItemStyle-Width="30px" ControlStyle-Width="30px"> 
                              <ItemTemplate>
                                <asp:CheckBox runat="server" Height="16" AutoPostBack="true" OnCheckedChanged="gv_UpdateLinks" Checked='<%# Server.HtmlEncode(Eval("links").ToString()).Equals("1") %>' style="position:relative; top:-2px;"/>
                              </ItemTemplate>
                            </asp:TemplateField>
                            <%--24--%><asp:BoundField HeaderText="DN" SortExpression="al_rag" DataField="al_rag" ItemStyle-Width="14px"/>
                            <%--25--%><asp:BoundField HeaderText="FN" SortExpression="fnotes" DataField="fnotes" ItemStyle-Width="14px"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <table cellpadding="1" cellspacing="0" border="1" bgcolor="#f0f0f0" runat="server" id="tbl_ragcount" 
                    style="border:solid 1px gray; border-top:0; position:relative; top:-2px; font-size:8pt;">
                        <tr>
                            <td align="left">Waiting for Copy</td>
                            <td bgcolor="red" width="14" align="center"><asp:Label runat="server" ID="lbl_al_wfc"/></td>
                            <td align="left">&nbsp;Copy In</td>
                            <td bgcolor="blue" width="14" align="center"><asp:Label runat="server" ID="lbl_al_copyin" ForeColor="White"/></td>
                            <td align="left">&nbsp;Proof Out</td>
                            <td bgcolor="Orange" width="14" align="center"><asp:Label runat="server" ID="lbl_al_proofout"/></td>
                            <td align="left">&nbsp;Own Advert</td>
                            <td bgcolor="Purple" width="14" align="center"><asp:Label runat="server" ID="lbl_al_ownadvert"/></td>
                            <td align="left">&nbsp;Approved</td>
                            <td bgcolor="Lime" width="14" align="center"><asp:Label runat="server" ID="lbl_al_approved"/></td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>

