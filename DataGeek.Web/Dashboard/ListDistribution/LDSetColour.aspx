<%--
Author   : Joe Pickering, 29/05/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="LDSetColour.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LDSetColour" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png" />
    
    <table ID="tbl" runat="server" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; top:10px; left:6px; padding:15px;">
        <tr><td colspan="6"><asp:Label runat="server" ID="lbl_title" ForeColor="White" Font-Bold="false" style="position:relative; left:-10px; top:-10px;"/></td></tr>
        <tr>
            <td>    
                <asp:LinkButton runat="server" ID="lb_remove" Text="Clear Current Colour" ForeColor="Silver" OnClick="SetColour" style="position:relative; top:-2px;"/>
                <telerik:RadColorPicker ID="cp" runat="server" AutoPostBack="true" OnColorChanged="SetColour" Preset="None" Width="200px" 
                PreviewColor="True" ShowEmptyColor="False" Skin="Vista">
                    <telerik:ColorPickerItem Title="Yellow" Value="#FFFF00"/>
                    <telerik:ColorPickerItem Title="Light Green" Value="#00FF00"/>
                    <telerik:ColorPickerItem Title="Turquoise" Value="#00FFFF"/>
                    <telerik:ColorPickerItem Title="Red" Value="#FF0000"/>
                    <telerik:ColorPickerItem Title="Teal" Value="#008080"/>
                    <telerik:ColorPickerItem Title="Thistle" Value="#D8BFD8"/>
                    <telerik:ColorPickerItem Title="Gray - 50%" Value="#808080"/>
                </telerik:RadColorPicker> 
            </td>
        </tr>
    </table>
    <asp:HiddenField ID="hf_list_id" runat="server"/>
</asp:Content>