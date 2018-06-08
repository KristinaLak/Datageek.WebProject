<%--
// Author   : Joe Pickering, 25/08/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ImportTemplates.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ImportLeads" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer">
        <table ID="tbl_upload" runat="server" class="WindowTableContainer">
            <tr>
                <td colspan="2">   
                    <asp:Label runat="server" Text="Click <b>Choose File</b> and browse for your Word <b>template</b> file (must be a <b>.doc</b> or <b>.docx</b> file).<br/><br/>" CssClass="MediumTitle"/>
                    <ajax:AsyncFileUpload ID="afu" runat="server" 
                        OnClientUploadError="UploadError" OnClientUploadComplete="UploadComplete" OnUploadedComplete="OnUploadComplete" CssClass="imageUploaderField"
                        UploaderStyle="Traditional" CompleteBackColor="#e7eff1" UploadingBackColor="White" ErrorBackColor="#ff8c6a" ThrobberID="lbl_throbber"/> <%--Width="250px"--%>
                    <asp:Label runat="server" ID="lbl_throbber" CssClass="MediumTitle" style="display:none;">
                         Uploading... &nbsp;
                         <img alt="Uploading, please wait." src="/Images/Misc/uploading.gif"/>
                    </asp:Label>
                </td>
            </tr>
        </table>
        
        <table ID="tbl_file_info" runat="server" width="100%" style="display:none;">
            <tr>
                <td><asp:Label runat="server" Text="Uploaded File Name:" CssClass="SmallTitle"/></td>
                <td><asp:Label ID="lbl_file_name" runat="server" CssClass="SmallTitle" Font-Bold="true"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Uploaded File Size:" CssClass="SmallTitle"/></td>
                <td><asp:Label ID="lbl_file_size" runat="server" CssClass="SmallTitle"/></td>
            </tr>
            <tr><td colspan="2" align="right"><asp:Button ID="btn_go_to_template_manager" runat="server" Text="Go to My Templates" CssClass="TemplateButton" style="display:none;"/></td></tr>
        </table>
    </div>
    
<script type="text/javascript">  
    function UploadError(sender, args) {
        alert("Error: " + args.get_errorMessage());
        addToClientTable(args.get_fileName(), "<span style='color:black;'>Error: File size must be a maximum of 10 MB and file must be non-empty</span>"); 
    }
    function UploadComplete(sender, args) {
        var contentType = args.get_contentType();
        var file_name = args.get_fileName();
        var file_size = args.get_length() / 1000 + " KB";

        grab('<%= tbl_file_info.ClientID %>').style.display = 'block';
        grab('<%= btn_go_to_template_manager.ClientID %>').style.display = 'block';
        grab('<%= lbl_file_name.ClientID %>').innerHTML = file_name;
        grab('<%= lbl_file_size.ClientID %>').innerHTML = file_size;
        Alertify('Upload of your Word template is complete!<br/><br/>Go back to your template manager to view/edit this template.', 'Upload Complete');
    }
</script>
</asp:Content>