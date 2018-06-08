<%--
Author   : Joe Pickering, 25/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FNSetColour.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNSetColour" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"/>
    
    <table runat="server" ID="tbl" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="6">
                <asp:Label runat="server" ID="lbl_title" ForeColor="White" Font-Bold="false" style="position:relative; left:-10px; top:-10px;"/> 
            </td>
        </tr>
        <tr>
            <td>    
                <asp:LinkButton runat="server" ID="lb_remove" Text="Clear Current Colour" ForeColor="Silver" OnClick="SetColour" style="position:relative; top:-2px;"/>
                <telerik:RadColorPicker ID="cp" runat="server"
                    AutoPostBack="true" OnColorChanged="SetColour" Preset="None" Width="200px"
                    PreviewColor="True" ShowEmptyColor="False" Skin="Vista">
                        <telerik:ColorPickerItem Title="Yellow" Value="#fafad2"/>
                        <telerik:ColorPickerItem Title="Green" Value="#3cb371"/>
                        <telerik:ColorPickerItem Title="Light Blue" Value="#add8e6"/>
                        <telerik:ColorPickerItem Title="Red" Value="#cd5c5c"/>
                        <telerik:ColorPickerItem Title="Teal" Value="#008080"/>
                        <telerik:ColorPickerItem Title="Thistle" Value="#D8BFD8"/>
                        <telerik:ColorPickerItem Title="Gray - 50%" Value="#808080"/>
                </telerik:RadColorPicker> 
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_ptp" runat="server"/>
    <asp:HiddenField ID="hf_lia" runat="server"/>
</asp:Content>