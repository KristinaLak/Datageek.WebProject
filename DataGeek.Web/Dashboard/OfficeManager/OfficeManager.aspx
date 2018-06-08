<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 06/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Office Manager" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="OfficeManager.aspx.cs" Inherits="OfficeManager" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox"/>
    <div id="div_page" runat="server" class="normal_page">
        <hr />
       
            <table width="99%" style="position:relative; top:-2px; left:4px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Office" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Manager" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    </td>
                </tr>
            </table>
       
            <table width="99%" style="margin-left:auto; margin-right:auto; color:White;">
                <tr>
                    <td colspan="2"><asp:Label runat="server" Text="Manage Existing Offices" ForeColor="DarkOrange" Font-Size="Small"/></td>
                </tr>
                <tr>
                    <td colspan="2" align="left" valign="top">
                        <asp:GridView ID="gv_t" border="2" runat="server" ForeColor="Black" 
                            CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2"
                            RowStyle-HorizontalAlign="Center" Width="980px" 
                            HeaderStyle-HorizontalAlign="Center"
                            AutoGenerateColumns="false"
                            OnRowCancelingEdit="gv_RowCancelingEdit" 
                            OnRowUpdating="gv_RowUpdating"
                            OnRowEditing="gv_RowEditing" 
                            OnRowDataBound="gv_RowDataBound">
                            <Columns>
                                <%--0--%><asp:CommandField ItemStyle-BackColor="White" ItemStyle-Width="18"
                                    ShowEditButton="true"
                                    ShowDeleteButton="false"
                                    ButtonType="Image"
                                    EditImageUrl="~\Images\Icons\gridView_Edit.png"
                                    CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                                    UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>
                                <%--1--%><asp:BoundField HeaderText="Office Name" DataField="Office" />
                                <%--2--%><asp:BoundField HeaderText="Short Name" DataField="ShortName" />
                                <%--3--%><asp:BoundField HeaderText="Team Name" DataField="TeamName" />
                                <%--4--%><asp:BoundField HeaderText="Conversion to USD" DataField="ConversionToUSD" />
                                <%--5--%><asp:BoundField HeaderText="Time Offset to GMT" DataField="TimeOffset" />
                                <%--6--%><asp:BoundField HeaderText="Day Offset" DataField="DayOffset" />
                                <%--7--%><asp:BoundField HeaderText="Colour" DataField="Colour" ItemStyle-ForeColor="Black" ItemStyle-CssClass="BlackGridEx"/>
                                <%--8--%><asp:BoundField HeaderText="Region" DataField="Region"/>
                                <%--9--%><asp:BoundField HeaderText="Closed" DataField="Closed"/>
                            </Columns>
                        </asp:GridView>  
                        <hr />
                    </td>
                </tr>
                <tr>
                    <td width="66%">
                        <asp:Label runat="server" ID="lbl_newoffice" Text="Add a New Office" ForeColor="DarkOrange" Font-Size="Small"/>
                        <asp:Label runat="server" ID="lbl_editoffice" Text="Change Colour" Visible="false"/>
                    </td>
                    <td><asp:Label runat="server" ID="lbl_close" Text="Close an Existing Office" ForeColor="DarkOrange" Font-Size="Small"/></td>
                </tr>
                <tr>
                    <td align="left" valign="top">
                        <table>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_name" Text="Office Name"/></td>
                                <td><asp:TextBox Width="200px" id="tb_newoffice" runat="server"/></td>
                                <td rowspan="7" valign="top">
                                    <telerik:RadColorPicker ID="rcp" runat="server" AutoPostBack="false" Preset="Default"
                                     Width="200px" PreviewColor="True" ShowEmptyColor="False" Skin="Vista" SelectedColor="White" style="margin-left:10px;"/>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_shortname" Text="Short Name"/></td>
                                <td><asp:TextBox Width="200px" id="tb_newofficesn" runat="server"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_teamname" Text="Team Name (For League Tables)"/></td>
                                <td><asp:TextBox Width="200px" id="tb_newofficeteamname" runat="server"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_time_offset" Text="Time Offset (From GMT in Hrs)"/></td>
                                <td><asp:TextBox Width="200px" id="tb_newofficetimeoffset" runat="server"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_day_offset" Text="Day Offset"/></td>
                                <td><asp:TextBox Width="200px" id="tb_newofficedayoffset" runat="server"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_conv" Text="Conversion to USD"/></td>
                                <td><asp:TextBox Width="200px" id="tb_conversion" runat="server" Text="1"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" ID="lbl_region" Text="Region (UK, US or CA)"/></td>
                                <td>
                                    <asp:DropDownList runat="server" ID="dd_region">
                                        <asp:ListItem Text="CA"/>
                                        <asp:ListItem Text="UK"/>
                                        <asp:ListItem Text="US"/>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button runat="server" ID="btn_addoffice" OnClick="AddOffice" 
                                    OnClientClick="return confirm('Are you sure you wish to add this office?');" Text="Add Office" />
                                </td>
                            </tr>
                        </table>
                        <asp:CompareValidator runat="server" ErrorMessage="Time offset must be a number value." 
                        ControlToValidate="tb_newofficetimeoffset" Type="Integer" Operator="DataTypeCheck" Display="Dynamic"/>
                        <asp:CompareValidator runat="server" ErrorMessage="Day offset must be a number value." 
                        ControlToValidate="tb_newofficedayoffset" Type="Integer" Operator="DataTypeCheck" Display="Dynamic"/>
                        <asp:CompareValidator runat="server" ErrorMessage="Conversion must be number." 
                        ControlToValidate="tb_conversion" Type="Double" Operator="DataTypeCheck" Display="Dynamic"/>
                    </td>
                    <td align="left" valign="top" width="40%">
                        <div id="div_close" runat="server">
                            Select an office to close:<br />
                            <asp:DropDownList Width="116px" runat="server" ID="dd_close_offices"/>
                            <asp:Button runat="server" ID="btn_close_office" OnClick="CloseOffice"
                            OnClientClick="return confirm('Are you sure you wish to close this office?');" 
                            Text="Close Office" />            
                        </div><br />
                        <div id="div_reopen" runat="server">
                            <asp:Label runat="server" ID="lbl_openter" Text="Re-Open an Existing Office" ForeColor="DarkOrange" Font-Size="Small"/>
                            <br />Select an office to re-open:<br />
                            <asp:DropDownList Width="116px" runat="server" ID="dd_open_offices"/>
                            <asp:Button runat="server" ID="btn_open_office" OnClick="OpenOffice"
                            OnClientClick="return confirm('Are you sure you wish to re-open this office?');" 
                            Text="Re-Open Office" />         
                        </div><br />
                        <div id="div_rename" runat="server">
                            <asp:Label runat="server" ID="lbl_rename" Text="Rename an Existing Office" ForeColor="DarkOrange" Font-Size="Small"/>
                            <br />Select an office to rename:<br />
                            <asp:DropDownList Width="116px" runat="server" ID="dd_rename_offices"/>
                            <asp:TextBox ID="tb_rename_office" runat="server" Text="New Office" Width="100px"/>
                            <asp:Button runat="server" ID="btn_renameoffice" OnClick="RenameOffice"
                            OnClientClick="return confirm('This will rename the office and will have no effect on old data. The office name is simply a container label. If you wish to keep historic data for this office under its current label then create a new office with a new name rather than renaming this one.\n\nDo you want to continue?');" 
                            Text="Rename Office" />            
                        </div><br />
                    </td>
                </tr>
            </table>     
        <hr />
    </div>
</asp:Content>
