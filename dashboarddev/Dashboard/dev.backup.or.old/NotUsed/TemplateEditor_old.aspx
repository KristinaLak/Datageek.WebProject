<%--
// Author   : Joe Pickering, 25/08/15
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="TemplateEditor_old.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="TemplateEditor" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="width:100%;">
        <table class="WindowTableContainer">
            <tr>
                <td><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/></td>
                <td align="right">
                    <asp:Image runat="server" CssClass="HandCursor" ImageUrl="~/images/leads/ico_info.png" Height="25" Width="25" 
                    ToolTip="The following changes will take place when this template is sent as an e-mail to a Lead/Suspect:
                    
:%lead_name%: will be replaced with the Lead's name  (formal)
:%lead_first_name%: will be replaced with the Lead's first name (informal)
:%interviewee_name%: will be replaced with the interviewee's name (formal)
:%interviewee_first_name%: will be replaced with the interviewee's first name (informal)
:%interviewee_job_title%: will be replaced with the interviewee's job title (if avail)
:%feature_company%: will be replaced with the feature company name
:%magazine_name%: will be replaced with the name of the relevant magazine for the sector/territory
:%magazine_cover_imgs%: will be replaced with the cover hyperlink/image to the advert (if avail)
:%advert_links%: will be replaced with the link(s) to the advert(s) (if avail)
:%signature%: will be replaced with a custom signature of the user who sends the mail"/>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <telerik:RadEditor ID="re_template" runat="server" AutoResizeHeight="false" EditModes="Design" EnableResize="true" Height="600">
                        <CssFiles>
                            
                        </CssFiles>
                    </telerik:RadEditor>
                    <%--<telerik:EditorCssFile Value="CSS/EditorContentArea.css"/>--%>
                </td>
            </tr> 
            <tr>
                <td align="left"><telerik:RadButton ID="btn_go_to_template_manager" runat="server" Text="Back to My Templates" CssClass="TemplateButton" OnClientClicking="BackToManager" Height="30"/></td>
                <td align="right"><telerik:RadButton ID="btn_save_templates" runat="server" Text="Save Template" CssClass="SaveButton" OnClick="SaveTemplate" Height="30"/></td></tr>
        </table>
    </div>
    <asp:HiddenField ID="hf_filename" runat="server"/>

<script type="text/javascript">
function BackToManager(sender, args) {
    if (confirm('Are you sure?\n\nYou will lose any unsaved changes to your template.')) {
        var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()
        { rwm.open('templatemanager.aspx', 'rw_template_manager'); rw.Close(); }, 0);
    } return false;
}
</script>
</asp:Content>