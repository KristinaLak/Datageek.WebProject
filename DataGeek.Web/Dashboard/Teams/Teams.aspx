<%--
// Author   : Joe Pickering, 22/04/2010 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Title="DataGeek :: Teams" ValidateRequest="false" Language="C#" AutoEventWireup="true" CodeFile="Teams.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="Teams" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" Runat="Server" >
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <div id="div_page" runat="server" class="normal_page">
        <hr/>
            
            <table width="99%" style="position:relative; left:8px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Mag" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Teams" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    </td>
                </tr>
            </table>
        
            <table>
                <tr>
                    <td>
                        <div style="height:350px; width:550px; overflow-y:scroll;">
                            <table border="0" width="92%" cellpadding="0" style="font-family:Verdana; font-size:8pt; margin-left:14px;">
                                <tr>
                                    <td valign="top" align="left">      
                                        <asp:Label runat="server" ID="lbl_region" ForeColor="White" Text="Regional"/>
                                        <br />
                                        <asp:GridView ID="gv_region" runat="server"
                                            border="0" AutoGenerateColumns="false" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            AllowSorting="false" EnableViewState="false" Cellpadding="2" RowStyle-ForeColor="Black">  
                                            <Columns>
                                                <asp:BoundField HeaderText="Name" DataField="name"/>
                                                <asp:BoundField HeaderText="Office" DataField="office" ItemStyle-ForeColor="Black" ItemStyle-CssClass="BlackGridEx"/>
                                                <asp:BoundField HeaderText="Region" DataField="region"/>
                                            </Columns>
                                        </asp:GridView>                      
                                    </td>
                                    <td valign="top" align="left">     
                                        <asp:Label runat="server" ID="lbl_channel" ForeColor="White" Text="Channels" />
                                        <br />
                                        <asp:GridView ID="gv_channel"  runat="server"
                                            border="0" AutoGenerateColumns="false" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            AllowSorting="false" EnableViewState="false" Cellpadding="2">  
                                            <Columns>
                                                <asp:BoundField HeaderText="Name" DataField="name"/>
                                                <asp:BoundField HeaderText="Office" DataField="office" ItemStyle-ForeColor="Black" ItemStyle-CssClass="BlackGridEx"/>
                                                <asp:BoundField HeaderText="Channel" DataField="Channel"/>
                                            </Columns>
                                        </asp:GridView>  
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                    <td valign="top">
                        <table border="0" width="280" bgcolor="gray" style="color:white; font-family:Verdana; font-size:8pt; position:relative; top:-4px; border:solid 2px darkgray; border-radius:5px;">
                            <tr>
                                <td style="border-bottom:solid 1px grey;" valign="bottom">
                                    <asp:Label runat="server" ID="lbl_showonly" Text="Show Only:"/>
                                </td>
                                <td style="border-bottom:solid 1px grey;">
                                    <asp:DropDownList id="dd_office" runat="server" AutoPostBack="true"/>
                                </td>
                                <td width="200px" valign="top" align="right"></td>
                            </tr>
                            <tr>
                                <td style="border-bottom:solid 1px grey;" valign="bottom">
                                    <br />
                                    <asp:Label runat="server" ID="lbl_oldemps" Text="Show Old Employees" />
                                </td>
                                <td align="left" colspan="2">
                                    <br />
                                    <asp:CheckBox runat="server" ID="cb_showoldemps" AutoPostBack="true"/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3">
                                    <br />
                                    <asp:Label runat="server" ID="lbl_search" Text="Search by Name: (e.g. 'Joe Bloggs' or 'Joe')" />
                                </td>             
                            </tr>
                            <tr>
                                <td style="border-bottom:solid 1px grey;"><asp:TextBox runat="server" ID="tb_search"/></td>
                                <td><asp:Button runat="server" ID="btn_search" Text="Search"/></td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            
        <hr/>
    </div>
</asp:Content>