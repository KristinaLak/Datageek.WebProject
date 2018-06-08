<%--
// Author   : Joe Pickering, 20/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MultiNote.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MultiNote" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="height:200px; margin:20px;">
        <table ID="tbl_move_leads" runat="server" class="WindowTableContainer">
            <tr><td><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/></td></tr>
            <tr>
                <td>
                    <telerik:RadTextBox ID="tb_note" runat="server" Height="150" Width="100%" TextMode="MultiLine" AutoCompleteType="Disabled" 
                    EmptyMessage="Type a note here for each selected Lead.." style="border-radius:8px; padding:8px; outline:none;"/>
                </td>
            </tr>
            <tr>
                <td>
                    <div ID="div_common_notes" runat="server" style="margin:8px 0px 2px 2px;">
                        <asp:Label runat="server" Text='Common Notes:' CssClass="SmallTitle" style="float:left; padding-right:7px;"/>
                        <telerik:RadDropDownList ID="dd_common_notes" runat="server" Width="310" ExpandDirection="Up" ZIndex="99999999" OnClientItemSelected="SetCommonNote" DropDownWidth="350" AutoPostBack="false"/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="NewNoteContainer">
                        <div class="NextActionContainer">
                            <asp:Label runat="server" Text='Next Action:' CssClass="SmallTitle" style="float:left; padding-right:7px;"/>
                            <telerik:RadDropDownList ID="dd_next_action_type" runat="server" ExpandDirection="Up" ZIndex="99999999"/>
                            <telerik:RadDateTimePicker ID="rdp_next_action" runat="server" ZIndex="99999999" PopupDirection="TopRight"/>
                        </div>
                    </div>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <br />
                    <telerik:RadButton ID="btn_add_note" runat="server" Text="Add Note/Next Action to Selected Leads" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_add_note_serv', false); }"/>
                    <asp:Button ID="btn_add_note_serv" runat="server" OnClick="AddNoteToSelectedLeads" style="display:none;"/>
                </td>
            </tr>
        </table>
        <asp:HiddenField ID="hf_lead_ids" runat="server"/>
    </div>
    <script type="text/javascript">
        function SetCommonNote(sender, eventArgs) {
            var item = eventArgs.get_item();
            var tb = $find("<%= tb_note.ClientID %>");
            var text = item.get_text();;
            if (tb.get_value() == "Type a note here for each selected Lead..")
                tb.set_value("");
            if (tb.get_value() != "")
                text = "\n\n" + text;
            tb.set_value(tb.get_value() + text);
        }
    </script>
</asp:Content>