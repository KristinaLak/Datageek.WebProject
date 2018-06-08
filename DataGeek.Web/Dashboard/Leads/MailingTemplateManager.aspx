<%--
// Author   : Joe Pickering, 12.05.17
// For      : Bizclick Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailingTemplateManager.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailingTemplateManager" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="margin:20px; width:800px; height:200px;">
        <asp:UpdatePanel ID="udp_files" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>

                <telerik:RadTabStrip ID="rts" runat="server" SelectedIndex="0" MultiPageID="rmp" Skin="Bootstrap" CausesValidation="false" OnClientTabSelected="ResizeRadWindowQuick" style="position:relative; left:-4px;">
                    <Tabs>
                        <telerik:RadTab runat="server" Text="My E-mail Templates" PageViewID="pv_files" Value="files" ToolTip="List of my uploaded documents"/>
                        <telerik:RadTab runat="server" Text="Upload Files" PageViewID="pv_upload" Value="upload" ToolTip="Upload new documents"/>
                    </Tabs>
                </telerik:RadTabStrip>
                <telerik:RadMultiPage ID="rmp" runat="server" SelectedIndex="0" Width="100%">
                    <telerik:RadPageView ID="pv_files" runat="server">
                        <asp:Label runat="server" CssClass="MediumTitle" Text="My <b>E-Mail Templates</b>" style="margin-bottom:10px; margin-top:10px;"/>
                        <asp:Label ID="lbl_files_info" runat="server" CssClass="SmallTitle"/>

                        <div ID="div_template_list" runat="server" style="padding:15px; max-height:350px; overflow-y:auto; margin-bottom:50px;">
                            <table ID="tbl_template_files" runat="server" width="100%"/>
                            <asp:Label ID="lbl_no_templates" runat="server" Visible="false" CssClass="SmallTitle" Text="You don't have any e-mail templates yet, upload some below or create some with the Template Editor." style="position:relative; top:-8px; left:4px;"/>
                        </div>

                        <telerik:RadButton ID="btn_refresh" runat="server" Text="Refresh List" OnClick="Refresh" Skin="Bootstrap" style="position:absolute; right:0; bottom:0; margin:10px 5px 0px 0px;"/>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="pv_upload" runat="server">
                        <asp:Label runat="server" CssClass="MediumTitle" Text="Upload <b>E-Mail Templates</b> or <b>Signatures</b>.." style="margin-bottom:10px; margin-top:10px;"/>
                        <asp:Label runat="server" Text="The files must be Word Documents (.docx)" CssClass="SmallTitle"/>
                        <div ID="div_upload" runat="server" style="height:200px;">
                            <div style="margin-top:20px;">
                                <asp:Label runat="server" Text="I want to upload an.." CssClass="TinyTitle"/>
                                <telerik:RadDropDownList ID="dd_upload_type" runat="server" Skin="Bootstrap" Width="220" ExpandDirection="Up" style="margin-top:2px;">
                                    <Items>
                                        <telerik:DropDownListItem Text="E-mail Template" Value="0"/>
                                        <telerik:DropDownListItem Text="E-mail Signature" Value="1"/>
                                    </Items>
                                </telerik:RadDropDownList>
                            </div>
                            <div>
                                <asp:Label runat="server" Text="&nbsp;" CssClass="MediumTitle"/>
                                <asp:Label runat="server" Text="Drag and drop files here, or select files using Browse & Upload.." CssClass="TinyTitle" style="float:left; position:relative; left:-1px; top:-1px; clear:both;"/>
                                <telerik:RadAsyncUpload ID="rau" runat="server" AllowedFileExtensions=".docx" OnClientValidationFailed="OnClientValidationFailed"
                                    OnClientFilesUploaded="Refresh" OnClientFileSelected="ResizeRadWindow" OnClientFileDropped="ResizeRadWindow" OnClientFileUploadRemoved="ResizeRadWindow"
                                    Width="320px" UploadedFilesRendering="BelowFileInput" MaxFileSize="3097152" MaxFileInputsCount="5" Skin="Bootstrap" OnFileUploaded="rau_FileUploaded"
                                    OverwriteExistingFiles="true" MultipleFileSelection="Automatic">
                                    <Localization Select="Browse & Upload"/>
                                </telerik:RadAsyncUpload>
                            </div>

                            <telerik:RadEditor ID="re_converter" runat="server" Visible="false"> 
                                <ImportSettings>
                                    <Docx DocumentLevel="Document" ImagesMode="Embedded"/>
                                </ImportSettings>
                            </telerik:RadEditor>
                        </div>
                    </telerik:RadPageView>
                </telerik:RadMultiPage>
                
                <asp:Button ID="btn_refresh_silent" runat="server" OnClick="SilentRefresh" style="display:none;"/>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <telerik:RadScriptBlock runat="server">
    <script type="text/javascript">
        function Refresh(a,b) {
            $get("<%= btn_refresh_silent.ClientID %>").click();
        }
        function OnClientValidationFailed(sender, args) {
            var fileExtention = args.get_fileName().substring(args.get_fileName().lastIndexOf('.') + 1, args.get_fileName().length);
            if (args.get_fileName().lastIndexOf('.') != -1) { // this checks if the extension is correct
                if (fileExtention != "docx") { 
                    Alertify("Wrong file extension!<br/><br/>Please only upload modern Microsoft Word Documents (.docx only, old .doc files will not be uploaded)", "Try again");
                }
                else {
                    Alertify("File size is too large!", "Oops");
                }
            }
            else {
                Alertify("Not a correct file extension!", "Oops");
            }
        }
    </script>
    </telerik:RadScriptBlock>
</asp:Content>