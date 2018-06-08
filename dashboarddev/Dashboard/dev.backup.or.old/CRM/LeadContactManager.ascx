<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LeadContactManager.ascx.cs" Inherits="ContactManager" %>

<asp:UpdatePanel runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:Label runat="server" Text="Contacts:" style="position:relative; left:-2px;" CssClass="MediumTitle"/>
        <asp:HiddenField ID="hf_num_contacts" runat="server" Value="1"/>
        <div ID="div_contacts" runat="server" style="position:relative; left:-3px;"/>
        <asp:Button ID="btn_add_another_contact" runat="server" Text="Add Another Contact" CausesValidation="false" OnClientClick="AddTemplate()"/>
        <asp:HiddenField ID="hf_cpy_id" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    function AddTemplate() {
        var num_templates = parseInt(grab('<%= hf_num_contacts.ClientID %>').value, 10) + 1;
        grab('<%= hf_num_contacts.ClientID %>').value = num_templates;
        return true;
    }
</script> 