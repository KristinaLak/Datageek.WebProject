<%--
// Author   : Joe Pickering, 21/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MultiColour.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MultiColour" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="height:390px; width:400px;">
        <table class="WindowTableContainer">
            <tr><td><asp:Label runat="server" Text="Apply a colour to each of your selected <b>Leads</b>.." CssClass="MediumTitle"/></td></tr>
            <tr>
                <td>
                    <telerik:RadColorPicker ID="rcp" runat="server" ShowRecentColors="true" EnableCustomColor="true" />
                </td>
            </tr>
            <tr>
                <td align="right">
                    <br />
                    <telerik:RadButton ID="btn_set_colour" runat="server" Text="Apply Colour to Selected Leads" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_set_colour_serv', false); }"/>
                    <asp:Button ID="btn_set_colour_serv" runat="server" OnClick="ApplyColour" style="display:none;"/>
                </td>
            </tr>
        </table>
        <asp:HiddenField ID="hf_lead_ids" runat="server"/>
    </div>

</asp:Content>