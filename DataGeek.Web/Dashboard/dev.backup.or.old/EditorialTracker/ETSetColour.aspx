<%--
Author   : Joe Pickering, 08/08/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="ETSetColour.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETSetColour" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table runat="server" ID="tbl" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="6"><asp:Label runat="server" ID="lbl_title" ForeColor="White" Font-Bold="false" style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr>
            <td>    
                <telerik:RadColorPicker ID="cp" runat="server" AutoPostBack="true" OnColorChanged="SetColour" Preset="Aspect"
                Width="200px" PreviewColor="True" ShowEmptyColor="False" Skin="Vista"/>
            </td>
        </tr>
    </table>
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
</asp:Content>