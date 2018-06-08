<%--
// Author   :Joe Pickering, 23/05/17
// For      :Bizclik Media, Leads Project.
// Contact  :joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailingTemplateEditor.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailingTemplateEditor" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:99%; margin:0px; margin-top:10px; margin-left:auto; margin-right:auto; height:98vh;">

        <telerik:RadWindowManager ID="rwm_thumbnail" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" VisibleTitlebar="true" AutoSize="true">
            <Windows>
                <telerik:RadWindow ID="rw_import_thumbnail" runat="server" Title="Magazine/Brochure Thumbnail Selector" OnClientClose="ImportSelectedThumbnail"/>
            </Windows>
        </telerik:RadWindowManager>

        <%--This is a telerik fix to register css for the radedit, due to issues with repainting the RadEditor Ribbonbar onpostback--%>
        <%--<telerik:RadStyleSheetManager EnableStyleSheetCombine="true" runat="server">
            <StyleSheets>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Skins.EditorLite.css"/>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI.Skins" Name="Telerik.Web.UI.Skins.MaterialLite.Editor.Material.css"/>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Skins.RibbonBarLite.css"/>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI.Skins" Name="Telerik.Web.UI.Skins.MaterialLite.RibbonBar.Material.css"/>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Skins.WindowLite.css"/>
                <telerik:StyleSheetReference Assembly="Telerik.Web.UI.Skins" Name="Telerik.Web.UI.Skins.MaterialLite.Window.Material.css"/>
            </StyleSheets>
        </telerik:RadStyleSheetManager>--%>

        <telerik:RadAjaxManager ID="ram" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btn_load_selected_template_file">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="lbl_title"/>
                        <telerik:AjaxUpdatedControl ControlID="re_template" LoadingPanelID="ralp"/>
                        <telerik:AjaxUpdatedControl ControlID="hf_filename"/>
                        <telerik:AjaxUpdatedControl ControlID="hf_selection_changed"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_save_template_dummy"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_save_template_ok"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_import_thumbnail"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_export_template"/>
                        <telerik:AjaxUpdatedControl ControlID="rcm_save"/>
                    </UpdatedControls>
                </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btn_new_document">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="lbl_title"/>
                        <telerik:AjaxUpdatedControl ControlID="re_template" LoadingPanelID="ralp"/>
                        <telerik:AjaxUpdatedControl ControlID="hf_filename"/>
                        <telerik:AjaxUpdatedControl ControlID="hf_selection_changed"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_save_template_dummy"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_save_template_ok"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_import_thumbnail"/>
                        <telerik:AjaxUpdatedControl ControlID="btn_export_template"/>
                        <telerik:AjaxUpdatedControl ControlID="rcm_save"/>
                    </UpdatedControls>
                </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btn_save_template_ok">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="dd_template" LoadingPanelID="ralp"/>
                        <telerik:AjaxUpdatedControl ControlID="hf_selection_changed"/>
                    </UpdatedControls>
                </telerik:AjaxSetting>
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="100" InitialDelayTime="0"/>

        <div style="margin-bottom:6px;">
            <asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="No document loaded. Use the <b>Load Selected Template File</b> button to preview/edit a document."/>
            <asp:LinkButton ID="btn_view_tags" runat="server" Text="Insert DataGeek Mailmerge Tags" AutoPostBack="false" Style="position:absolute; right:27px; top:18px;"/>
            <asp:Image runat="server" ImageUrl="~/images/leads/ico_tag.png" Style="position:absolute; right:7px; top:18px;"/>
        </div>

        <div style="border:solid 1px #e5e5e5; background-color:#f6f6f6; height:34px; padding:6px; border-radius:4px;">
            <asp:Label runat="server" Text="Document:" CssClass="MediumTitle" Style="float:left; margin-left:6px; margin-right:6px; position:relative; top:2px; font-size:14px;"/>
            <telerik:RadDropDownList ID="dd_template" runat="server" Skin="Bootstrap" Width="200" DropDownWidth="300" EnableDirectionDetection="true" Style="float:left;"/>
            <telerik:RadButton ID="btn_load_selected_template_file" Text="Open Document" runat="server" OnClientClicking="function(a,b) { ConfirmLoadTemplate(a,b, false); }" OnClick="LoadSelectedTemplateFile" UseSubmitBehavior="false" Skin="Bootstrap" Style="float:left; margin-left:4px;"/>
            <telerik:RadButton ID="btn_upload_template" runat="server" Text="Upload a Document" AutoPostBack="false" Skin="Bootstrap" Style="float:left; margin-left:4px;"/>
            <telerik:RadButton ID="btn_new_document" runat="server" Text="Blank Document" OnClientClicking="function(a,b) { OnClientSelectionChange(a,b); ConfirmLoadTemplate(a,b, true); }" OnClick="NewTemplate" Skin="Bootstrap" Style="float:left; margin-left:4px;"/>

            <telerik:RadButton ID="btn_save_template_dummy" runat="server" Text="Save Template" OnClientClicking="function(x,y){ShowSaveMenu(); return false;}" AutoPostBack="false" Skin="Bootstrap" Style="float:right;"/>
            <telerik:RadContextMenu ID="rcm_save" runat="server" EnableRoundedCorners="true" EnableShadows="true" CausesValidation="false"
                CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap" OnItemClick="SaveTemplateFileProxy">
                <Targets>
                    <telerik:ContextMenuControlTarget ControlID="btn_save_template_dummy"/>
                </Targets>
            </telerik:RadContextMenu>
            <telerik:RadButton ID="btn_save_template_ok" runat="server" OnClick="SaveTemplateFile" Style="display:none;"/>
            <asp:HiddenField ID="hf_new_filename" runat="server"/>
            <asp:HiddenField ID="hf_new_file_type" runat="server"/>
            <telerik:RadButton ID="btn_export_template" runat="server" Text="Save Copy to Computer" AutoPostBack="false" OnClientClicking="ExportNonAjax" Skin="Bootstrap" Visible="false" Style="float:right; margin-right:4px;"/>
            <telerik:RadButton ID="btn_import_thumbnail" runat="server" Text="Insert Magazine/Brochure" AutoPostBack="false" Width="176" Skin="Bootstrap" Style="float:right; margin-right:4px;"/>
            <telerik:RadButton ID="btn_export_non_ajax" runat="server" OnClick="ExportTemplate" Style="display:none;"/>
        </div>

        <div id="div_editor" runat="server" style="height:90%;">
            <telerik:RadEditor ID="re_template" runat="server" EmptyMessage="&nbsp;No template file loaded, either create a new file from scratch or load one using the Open Document button." Language="En-Gb"
                NewLineMode="Br" ToolbarMode="Default" Width="99%" EnableResize="false" AutoResizeHeight="true" OnClientSelectionChange="OnClientSelectionChange" OnClientLoad="OnClientLoad"
                StripFormattingOptions="MSWordNoFonts, Font" style="margin-left:auto; margin-right:auto;">
                <ExportSettings>
                    <Docx DefaultFontName="Arial" DefaultFontSizeInPoints="12" HeaderFontSizeInPoints="18"/>
                </ExportSettings>
                <Tools>
                    <telerik:EditorToolGroup Tag="CustomToolbar">
                        <telerik:EditorSplitButton Name="Undo" Width="80px"/>
                        <telerik:EditorSplitButton Name="Redo" Width="80px"/>
                        <telerik:EditorTool Name="Cut"/>
                        <telerik:EditorTool Name="Copy"/>
                        <telerik:EditorTool Name="Paste" ShortCut="CTRL+V"/>
                        <telerik:EditorTool Name="PasteStrip" Text="Paste Special"/>
                        <telerik:EditorTool Name="Bold"/>
                        <telerik:EditorTool Name="Italic"/>
                        <telerik:EditorTool Name="Underline"/>
                        <telerik:EditorTool Name="ForeColor"/>
                        <telerik:EditorTool Name="BackColor"/>
                        <telerik:EditorTool Name="InsertOrderedList"/>
                        <telerik:EditorTool Name="InsertUnorderedList"/>
                        <telerik:EditorTool Name="StrikeThrough"/>
                        <telerik:EditorTool Name="ConvertToLower"/>
                        <telerik:EditorTool Name="ConvertToUpper"/>
                        <telerik:EditorTool Name="SelectAll"/>
                        <telerik:EditorTool Name="FontName"/>
                        <telerik:EditorTool Name="RealFontSize" Text="Font Size"/>
                        <telerik:EditorTool Name="FormatBlock"/>
                        <telerik:EditorTool Name="JustifyLeft"/>
                        <telerik:EditorTool Name="JustifyCenter"/>
                        <telerik:EditorTool Name="JustifyRight"/>
                        <telerik:EditorTool Name="JustifyNone"/>
                        <telerik:EditorTool Name="Indent"/>
                        <telerik:EditorTool Name="Outdent"/>
                        <telerik:EditorTool Name="FormatStripper"/>
                        <telerik:EditorTool Name="FindAndReplace"/>
                        <telerik:EditorTool Name="AjaxSpellCheck"/>
                        <telerik:EditorTool Name="ImageManager"/>
                        <%--<telerik:EditorTool Name="LinkManager"/>--%>
                        <telerik:EditorTool Name="InsertLink"/>
                        <telerik:EditorTool Name="Unlink"/>
                        <telerik:EditorTool Name="InsertTable"/>
                        <telerik:EditorTool Name="InsertSymbol"/>
                        <telerik:EditorTool Name="InsertHorizontalRule"/>
                        <telerik:EditorTool Name="SubScript"/>
                        <telerik:EditorTool Name="SuperScript"/>
                        <telerik:EditorTool Name="Zoom"/>
                        <telerik:EditorTool Name="ToggleScreenMode"/>
                    </telerik:EditorToolGroup>
                </Tools>
            </telerik:RadEditor>
        </div>

        <asp:HiddenField ID="hf_filename" runat="server"/>
        <asp:HiddenField ID="hf_selection_changed" runat="server" Value="0"/>

        <telerik:RadScriptBlock ID="rsb" runat="server">
            <script type="text/javascript">
                function ExportNonAjax(a, b) {
                    $get("<%= btn_export_non_ajax.ClientID %>").click();
            }
            function ShowSaveMenu() {
                var ctrl = $get("<%= btn_save_template_dummy.ClientID %>");
                var x = getOffset(ctrl).left;
                var y = getOffset(ctrl).top;
                setTimeout(function () {
                    $find("<%= rcm_save.ClientID %>").showAt(x + 110, y + 20);
                }, 100);
            }
            function OnClientSelectionChange(editor, args) {
                $get("<%= hf_selection_changed.ClientID %>").value = "1";
            }
            function ConfirmLoadTemplate(sender, args, isnew) {
                var ed_content = $find("<%=re_template.ClientID%>").get_text();
                var SelectionChanged = $get("<%= hf_selection_changed.ClientID %>").value == "1";
                if (ed_content && ed_content != "" && SelectionChanged) {
                    var m = 'load this';
                    if (isnew)
                        m = 'create a new';
                    args.set_cancel(!confirm('Are you sure you want to ' + m + ' document?\n\nAll work in the word processor will be lost!'));
                }
            }
            Sys.Application.add_load(
                function () {
                    var div_height = $get("<%=div_editor.ClientID%>").clientHeight - 4 + "px";
                    $get("<%=re_template.ClientID%>").style.height = div_height;
                    $get("<%=re_template.ClientID%>Wrapper").style.height = div_height;
                }
            );
            function OnClientLoad(editor, args) {
                var ed_content = $find("<%=re_template.ClientID%>").get_text();
                if (ed_content == '') {
                    setTimeout(function () {
                        var style = editor.get_contentArea().style;
                        style.fontFamily = 'Arial';
                        style.fontSize = 14 + 'px';
                    }, 100);
                }
            }
            function InsertContentAtCursor(c) {
                var editor = $find("<%= re_template.ClientID %>");
                var selection = editor.getSelection();
                selection.selectRange(selection.getRange());
                editor.pasteHtml(c);
            }
            function ImportSelectedThumbnail(sender, args) {
                var data = args.get_argument();
                if (data)
                    InsertContentAtCursor(HtmlDecode(data));
            }
            </script>
        </telerik:RadScriptBlock>
    </div>
</asp:Content>
