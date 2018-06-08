<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CpyContactManager.ascx.cs" Inherits="CpyContactManager" %>
<asp:UpdatePanel runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:HiddenField ID="hf_num_contacts" runat="server" Value="1"/>
        <div ID="div_contacts" runat="server" style="position:relative; left:-3px;"/>
        <asp:LinkButton ID="lb_new_ctc" runat="server" Text="Add another contact" OnClientClick="AddTemplate()" CausesValidation="false"/>
        <asp:HiddenField ID="hf_cpy_id" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    function AddTemplate() {
        var num_templates = parseInt(document.getElementById('<%= hf_num_contacts.ClientID %>').value, 10) + 1;
        document.getElementById('<%= hf_num_contacts.ClientID %>').value = num_templates;
        return true;
    }
</script> 