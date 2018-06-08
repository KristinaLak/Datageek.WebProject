<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactNotesManager.ascx.cs" Inherits="ContactNotesManager"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="Lead Notes and Actions" style="font-weight:500; margin-top:0px; position:relative; top:-3px;"/>
<div ID="div_notes" runat="server" class="NotesContainer">
    <asp:Repeater ID="rep_notes" runat="server" OnItemDataBound="rep_notes_ItemDataBound">
        <ItemTemplate>
            <asp:ImageButton ID="imbtn_del_note" runat="server" ImageUrl="~/images/leads/ico_trash.png" Height="14" Width="14" OnClick="DeleteNote" ToolTip="Remove Note/Next Action" style="float:right; margin:6px 4px 0px 0px;"/> 
            <asp:Label ID="lbl_is_next_action" runat="server" Text='<%#: Bind("isnextaction") %>' Visible="false"/>
            <asp:Label ID="lbl_note_id" runat="server" Text='<%#: Bind("NoteID") %>' Visible="false"/>
            <li ID="li_note" runat="server" class="NoteBullet">
                <asp:Label ID="lbl_note" runat="server" Text='<%# Bind("Note") %>' CssClass="SmallTitle SmallTitleNoPad"/><br/>
                <asp:Label ID="lbl_note_added" runat="server" Text='<%#: Bind("date") %>' CssClass="SmallSubTitle"/>
                <asp:Label ID="lbl_added_by" runat="server" Text='<%#: Bind("fullname") %>' CssClass="SmallSubTitle"/>
            </li>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Label ID="lbl_no_notes" runat="server" Visible="false" CssClass="SmallTitle" Text="No notes added yet.." style="margin:6px;"/>
</div>
<div ID="div_new" runat="server" class="NewNoteContainer">
    <telerik:RadTextBox ID="tb_add_note" runat="server" Height="70" Width="100%" TextMode="MultiLine" AutoCompleteType="Disabled" 
        EmptyMessage="Add a note for this contact..." style="border-radius:4px; padding:8px; outline:none;"/>

    <div ID="div_common_notes" runat="server" Visible="false" style="margin:8px 0px 16px 3px;">
        <telerik:RadDropDownList ID="dd_common_notes" runat="server" Width="250" ExpandDirection="Up" ZIndex="99999999" DropDownWidth="350" 
            AutoPostBack="false" style="float:left; margin-right:5px;"/>
        <asp:Button ID="btn_add_common_note" runat="server" Text="Add Common Note" OnClick="SaveChanges" CssClass="LButton LButtonGreen" style="padding:2px 16px; position:relative; top:-1px;"/>
    </div>

    <div class="NextActionContainer" runat="server">
        <asp:Label runat="server" Text='Next Action:' CssClass="SmallTitle" style="float:left; padding-right:5px;"/>
        <telerik:RadDropDownList ID="dd_next_action_type" runat="server" Width="80" ExpandDirection="Up" ZIndex="99999999"/>
        <telerik:RadDateTimePicker ID="rdp_next_action" runat="server" ZIndex="99999999" PopupDirection="TopLeft" Skin="Metro"/>
        <asp:Button ID="btn_save_changes" runat="server" Text="Save" OnClick="SaveChanges" CssClass="LButton" style="float:right; margin-right:1px; padding:2px 18px;"/>
    </div>
</div>
<asp:HiddenField ID="hf_nat" runat="server"/>
<asp:HiddenField ID="hf_nad" runat="server"/>
<asp:HiddenField ID="hf_ctc_id" runat="server"/>
<asp:HiddenField ID="hf_lead_id" runat="server"/>    