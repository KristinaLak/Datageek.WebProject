<%--
// Author   : Joe Pickering, 20/01/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Training Administration" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="TrainingAdmin.aspx.cs" Inherits="TrainingAdmin" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .hoverlabel
        {
            color:#3b3b3b;
            font-weight:bold;
            height:19px;
            width:350px;
            background-color:#e0e0e0;
            cursor:pointer; 
            cursor:hand; 
            border:dotted 1px gray;
            display:block;
        }
        .hoverlabel:hover
        {
        	color:White;
        	background-color:SteelBlue;
        }
        .panelstyle
        {
        	background-color:DarkGray;
        	border:dotted 1px darkorange;
        }
    </style>
</asp:Content> 

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
            <table width="99%" style="font-family:Verdana; position:relative; left:2px; top:-2px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Training" ForeColor="White" Font-Bold="true" Font-Size="Medium" /> 
                        <asp:Label runat="server" Text="Administration" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                        <br />
                    </td>
                    <td align="right">
                        <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png" OnClick="Refresh"/> 
                    </td>
                </tr>
            </table>
       
            <%--MAIN TABLE--%>
            <table border="0" width="99%" style="margin-left:auto; margin-right:auto; border-top:solid 1px darkgray;" bgcolor="#868686">                               
                <tr>
                    <td colspan="2">Use this page to upload private documents and view/set user access permissions.<br /><br /></td>
                </tr>    
                <tr>
                    <td style="border-right:solid 1px darkgray;"><asp:Label runat="server" Text="Permissions:" ForeColor="White" Font-Size="Medium"/></td>
                    <td><asp:Label runat="server" Text="Files:" ForeColor="White" Font-Size="Medium"/></td>
                </tr>
                <tr>
                    <td width="41%" valign="top" style="border-right:solid 1px darkgray;">     
                        <%--Admin Users--%>
                        <asp:Panel ID="pnl_admin_users_head" runat="server" style="height:20px;">
                            <asp:Label ID="lbl_admin_users_epxandcollapse" runat="server" CssClass="hoverlabel"/>
                        </asp:Panel>
                        <asp:Panel ID="pnl_admin_users_body" runat="server" CssClass="panelstyle"> 
                            <asp:GridView runat="server" ID="db_TrainingAdmin" Width="360"
                                Cellpadding="2" border="2" ToolTip="administrative"
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false"
                                OnRowDeleting="gv_RowDeleting" OnRowDataBound="gv_RowDataBound">
                                <Columns> 
                                    <asp:CommandField ItemStyle-BackColor="White" 
                                        ItemStyle-Width="18"
                                        ShowEditButton="false"
                                        ShowDeleteButton="true"
                                        ButtonType="Image" 
                                        DeleteImageUrl="~\Images\Icons\gridView_Delete.png"/>  
                                    <asp:BoundField HeaderText="Username" DataField="Username"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office"/>
                                    <asp:BoundField HeaderText="Last Login" DataField="Login"/>
                                </Columns>
                            </asp:GridView>
                            <table runat="server" cellpadding="0" cellspacing="0" style="border-top:solid 1px OldLace;">
                                <tr>
                                    <td><asp:DropDownList runat="server" ID="dd_admin_users"/>&nbsp;</td>
                                    <td><asp:LinkButton runat="server" ID="pnl_admin_users_body_btn_add" Text="Add Selected User" OnClick="AddUserToRole" ForeColor="White"/></td>
                                </tr>
                            </table>
                            &nbsp;
                        </asp:Panel>
                        
                        <ajax:CollapsiblePanelExtender runat="server" ID="cpe_admin" 
                            TargetControlID="pnl_admin_users_body" 
                            CollapseControlID="pnl_admin_users_head" 
                            ExpandControlID="pnl_admin_users_head"
                            TextLabelID="lbl_admin_users_epxandcollapse" 
                            Collapsed="true"
                            CollapsedText="&nbsp;Manage Administrative users" 
                            ExpandedText="&nbsp;Hide Administrative users" 
                            CollapsedSize="0"
                            ExpandedSize="300"
                            AutoCollapse="False"
                            AutoExpand="False"
                            ExpandDirection="Vertical"
                            ScrollContents="True"/>
                    </td>
                    <td rowspan="5" valign="top">
                        <table runat="server" id="tbl_upload" cellpadding="0" cellspacing="0">
                            <tr>
                                <td valign="top">    
                                    <asp:Label runat="server" ID="lbl_uploads" ForeColor="Orange"/>
                                    <div runat="server" ID="div_uploads"/>
                                </td>
                            </tr>
                            <tr>
                                <td align="left" style="border-top:solid 1px OldLace;">
                                    <table cellpadding="0" cellspacing="0" style="margin-top:5px;">
                                        <tr>
                                            <td>    
                                                <asp:Label runat="server" Text="Upload:" ForeColor="White" Font-Size="Medium"/>
                                                <asp:Label runat="server" Text="Upload any file type: (20MB max):" ForeColor="Orange"/>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <ajax:AsyncFileUpload ID="afu" runat="server" Width="450px"
                                        OnClientUploadError="UploadError"
                                        OnClientUploadComplete="UploadComplete" 
                                        OnUploadedComplete="OnUploadComplete"
                                        UploaderStyle="Modern" 
                                        UploadingBackColor="#CCFFFF" 
                                        ThrobberID="lbl_throbber"/>
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <asp:Label runat="server" ID="lbl_throbber" style="display:none;">
                                         Uploading... &nbsp;
                                         <img alt="" src="/Images/Misc/uploading.gif"/>
                                    </asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td align="left">
                                    <br />Click "Select File" and browse for your presentation file.<br /> Re-uploading a file with the same name as an existing file will update (overwrite) the existing file. 
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <asp:Label runat="server" Text="&nbsp;" ID="lbl_upload_result"/>
                                    <table runat="server" ID="tb_uploaded_files" cellpadding="3" style="border-collapse:collapse; border-left:solid 1px #aaaaff; border-top:solid 1px #aaaaff;"/>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>      
                <tr>
                    <td valign="top"  style="border-right:solid 1px darkgray;">    
                        <%--Presentation Users--%>
                        <asp:Panel ID="pnl_pres_users_head" runat="server" style="height:20px;">
                            <asp:Label ID="lbl_pres_users_epxandcollapse" runat="server" CssClass="hoverlabel"/>
                        </asp:Panel>
                        <asp:Panel ID="pnl_pres_users_body" runat="server" CssClass="panelstyle">
                            <asp:GridView runat="server" ID="db_TrainingPres" Width="360"
                                Cellpadding="2" border="2" ToolTip="presentation"
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false"
                                OnRowDeleting="gv_RowDeleting" OnRowDataBound="gv_RowDataBound">
                                <Columns> 
                                    <asp:CommandField ItemStyle-BackColor="White" 
                                        ItemStyle-Width="18"
                                        ShowEditButton="false"
                                        ShowDeleteButton="true"
                                        ButtonType="Image" 
                                        DeleteImageUrl="~\Images\Icons\gridView_Delete.png"/>  
                                    <asp:BoundField HeaderText="Username" DataField="Username"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office"/>
                                    <asp:BoundField HeaderText="Last Login" DataField="Login"/>
                                </Columns>
                            </asp:GridView>
                            <table runat="server" cellpadding="0" cellspacing="0" style="border-top:solid 1px OldLace;">
                                <tr>
                                    <td><asp:DropDownList runat="server" ID="dd_pres_users"/>&nbsp;</td>
                                    <td><asp:LinkButton runat="server" ID="pnl_pres_users_body_btn_add" Text="Add Selected User" OnClick="AddUserToRole" ForeColor="White"/></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <ajax:CollapsiblePanelExtender runat="server" ID="cpe_pres" 
                            TargetControlID="pnl_pres_users_body" 
                            CollapseControlID="pnl_pres_users_head" 
                            ExpandControlID="pnl_pres_users_head"
                            TextLabelID="lbl_pres_users_epxandcollapse" 
                            Collapsed="true"
                            CollapsedText="&nbsp;Manage Presentation users" 
                            ExpandedText="&nbsp;Hide Presentation users" 
                            CollapsedSize="0"
                            ExpandedSize="300"
                            AutoCollapse="False"
                            AutoExpand="False"
                            ExpandDirection="Vertical"
                            ScrollContents="True"/>
                    </td>
                </tr>
                <tr>
                    <td valign="top"  style="border-right:solid 1px darkgray;">    
                        <%--Documents Users--%>
                        <asp:Panel ID="pnl_docs_users_head" runat="server" style="height:20px;">
                            <asp:Label ID="lbl_docs_users_epxandcollapse" runat="server" CssClass="hoverlabel"/>
                        </asp:Panel>
                        <asp:Panel ID="pnl_docs_users_body" runat="server" CssClass="panelstyle">
                            <asp:GridView runat="server" ID="db_TrainingDocs" Width="360"
                                Cellpadding="2" border="2" ToolTip="BizClik ToolBox"
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false"
                                OnRowDeleting="gv_RowDeleting" OnRowDataBound="gv_RowDataBound">
                                <Columns> 
                                    <asp:CommandField ItemStyle-BackColor="White" 
                                        ItemStyle-Width="18"
                                        ShowEditButton="false"
                                        ShowDeleteButton="true"
                                        ButtonType="Image" 
                                        DeleteImageUrl="~\Images\Icons\gridView_Delete.png"/>  
                                    <asp:BoundField HeaderText="Username" DataField="Username"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office"/>
                                    <asp:BoundField HeaderText="Last Login" DataField="Login"/>
                                </Columns>
                            </asp:GridView>
                            <table runat="server" cellpadding="0" cellspacing="0" style="border-top:solid 1px OldLace;">
                                <tr>
                                    <td><asp:DropDownList runat="server" ID="dd_docs_users"/>&nbsp;</td>
                                    <td><asp:LinkButton runat="server" ID="pnl_docs_users_body_btn_add" Text="Add Selected User" OnClick="AddUserToRole" ForeColor="White"/></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <ajax:CollapsiblePanelExtender runat="server" ID="cpe_docs" 
                            TargetControlID="pnl_docs_users_body" 
                            CollapseControlID="pnl_docs_users_head" 
                            ExpandControlID="pnl_docs_users_head"
                            TextLabelID="lbl_docs_users_epxandcollapse" 
                            Collapsed="true"
                            CollapsedText="&nbsp;Manage BizClik ToolBox users" 
                            ExpandedText="&nbsp;Hide BizClik ToolBox users" 
                            CollapsedSize="0"
                            ExpandedSize="300"
                            AutoCollapse="False"
                            AutoExpand="False"
                            ExpandDirection="Vertical"
                            ScrollContents="True"/>
                    </td>
                </tr>
                <tr>
                    <td valign="top" style="border-right:solid 1px darkgray;">    
                        <%--Video Users--%>
                        <asp:Panel ID="pnl_video_users_head" runat="server" style="height:20px;">
                            <asp:Label ID="lbl_video_users_epxandcollapse" runat="server" CssClass="hoverlabel"/>
                        </asp:Panel>
                        <asp:Panel ID="pnl_video_users_body" runat="server" CssClass="panelstyle">
                            <asp:GridView runat="server" ID="db_TrainingVideos" Width="360"
                                Cellpadding="2" border="2" ToolTip="video"
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false"
                                OnRowDeleting="gv_RowDeleting" OnRowDataBound="gv_RowDataBound">
                                <Columns> 
                                    <asp:CommandField ItemStyle-BackColor="White" 
                                        ItemStyle-Width="18"
                                        ShowEditButton="false"
                                        ShowDeleteButton="true"
                                        ButtonType="Image" 
                                        DeleteImageUrl="~\Images\Icons\gridView_Delete.png"/>  
                                    <asp:BoundField HeaderText="Username" DataField="Username"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office"/>
                                    <asp:BoundField HeaderText="Last Login" DataField="Login"/>
                                </Columns>
                            </asp:GridView>
                            <table runat="server" cellpadding="0" cellspacing="0" style="border-top:solid 1px OldLace;">
                                <tr>
                                    <td><asp:DropDownList runat="server" ID="dd_video_users"/>&nbsp;</td>
                                    <td><asp:LinkButton runat="server" ID="pnl_video_users_body_btn_add" Text="Add Selected User" OnClick="AddUserToRole" ForeColor="White"/></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <ajax:CollapsiblePanelExtender runat="server" ID="cpe_video" 
                            TargetControlID="pnl_video_users_body" 
                            CollapseControlID="pnl_video_users_head" 
                            ExpandControlID="pnl_video_users_head"
                            TextLabelID="lbl_video_users_epxandcollapse" 
                            Collapsed="true"
                            CollapsedText="&nbsp;Manage Video users" 
                            ExpandedText="&nbsp;Hide Video users" 
                            CollapsedSize="0"
                            ExpandedSize="300"
                            AutoCollapse="False"
                            AutoExpand="False"
                            ExpandDirection="Vertical"
                            ScrollContents="True"/>
                    </td>
                </tr>
                <tr>
                    <td valign="top" style="border-right:solid 1px darkgray;">    
                        <%--Upload Users--%>
                        <asp:Panel ID="pnl_upload_users_head" runat="server" style="height:20px;">
                            <asp:Label ID="lbl_upload_users_epxandcollapse" runat="server" CssClass="hoverlabel"/>
                        </asp:Panel>
                        <asp:Panel ID="pnl_upload_users_body" runat="server" CssClass="panelstyle">
                            <asp:GridView runat="server" ID="db_TrainingUpload" Width="360"
                                Cellpadding="2" border="2" ToolTip="Upload"
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false"
                                OnRowDeleting="gv_RowDeleting" OnRowDataBound="gv_RowDataBound">
                                <Columns> 
                                    <asp:CommandField ItemStyle-BackColor="White" 
                                        ItemStyle-Width="18"
                                        ShowEditButton="false"
                                        ShowDeleteButton="true"
                                        ButtonType="Image" 
                                        DeleteImageUrl="~\Images\Icons\gridView_Delete.png"/>  
                                    <asp:BoundField HeaderText="Username" DataField="Username"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office"/>
                                    <asp:BoundField HeaderText="Last Login" DataField="Login"/>
                                </Columns>
                            </asp:GridView>
                            <table runat="server" cellpadding="0" cellspacing="0" style="border-top:solid 1px OldLace;">
                                <tr>
                                    <td><asp:DropDownList runat="server" ID="dd_upload_users"/>&nbsp;</td>
                                    <td><asp:LinkButton runat="server" ID="pnl_upload_users_body_btn_add" Text="Add Selected User" OnClick="AddUserToRole" ForeColor="White"/></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <ajax:CollapsiblePanelExtender runat="server" ID="cpe_upload" 
                            TargetControlID="pnl_upload_users_body" 
                            CollapseControlID="pnl_upload_users_head" 
                            ExpandControlID="pnl_upload_users_head"
                            TextLabelID="lbl_upload_users_epxandcollapse" 
                            Collapsed="true"
                            CollapsedText="&nbsp;Manage Upload users" 
                            ExpandedText="&nbsp;Hide Upload users" 
                            CollapsedSize="0"
                            ExpandedSize="300"
                            AutoCollapse="False"
                            AutoExpand="False"
                            ExpandDirection="Vertical"
                            ScrollContents="True"/>
                    </td>
                </tr>
            </table>

        <hr />
    </div>
    
    <script type="text/javascript">  
        function FillCell(row, cellNumber, text) {
            var cell = row.insertCell(cellNumber);
            cell.innerHTML = text;
            cell.style.borderBottom = cell.style.borderRight = "solid 1px #aaaaff";
        }
        function AddToClientTable(name, text) {
            var table = grab("<%= tb_uploaded_files.ClientID %>");
            var row = table.insertRow(0);
            FillCell(row, 0, name);
            FillCell(row, 1, text);
        }

        function UploadError(sender, args) {
            alert("Error: " + args.get_errorMessage());
            addToClientTable(args.get_fileName(), "<span style='color:orange;'>Error: File size must be a maximum of 10 MB and file must be non-empty</span>"); 
        }
        function UploadComplete(sender, args) {
            var contentType = args.get_contentType();
            var text = args.get_length()/1000 + " Kbytes";
            if (contentType.length > 0) {
                text += ", '" + contentType + "'";
            }
            AddToClientTable(args.get_fileName(), text);
        }
    </script>
</asp:Content>