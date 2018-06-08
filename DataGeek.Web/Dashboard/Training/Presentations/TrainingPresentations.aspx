<%--
// Author   : Joe Pickering, 02/11/2009 - re-written 12/09/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Training Presentations" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="TrainingPresentations.aspx.cs" Inherits="TrainingPresentations" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <head>
        <meta http-equiv="X-UA-Compatible">
        <script type="text/javascript">
            var message = "Right click is disabled";
            function click(e) {
                if (document.all) {
                    if (event.button == 2) {
                        alert(message);
                        return false;
                    }
                }
                if (document.layers) {
                    if (e.which == 3) {
                        alert(message);
                        return false;
                    }
                }
            }
            if (document.layers) {
                document.captureEvents(Event.MOUSEDOWN);
            }
            if (document.all) // for IE
            {
                document.onmousedown = click;
            }
            else // for FF
            {
                document.onclick = click;
            }
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
                location.reload();
            }
            function SetText(text) {
                grab("<%= lbl_description.ClientID%>").innerHTML = text;
                return;
            }
        </script>
    </head>
    <body oncontextmenu="return false;"></body>
</asp:Content> 

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
            <table width="99%" style="font-family:Verdana; position:relative; left:2px; top:-2px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Training" ForeColor="White" Font-Bold="true" Font-Size="Medium" /> 
                        <asp:Label runat="server" Text="Presentations" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                        <br />
                    </td>
                    <td align="right"><asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png"/></td>
                </tr>
            </table>
        
            <%--MAIN TABLE--%>
            <table border="0" width="99%" style="margin-left:auto; margin-right:auto; border-top:solid 1px darkgray;" bgcolor="gray">                               
                <tr>
                    <td valign="top" width="65%" rowspan="2" style="border-bottom:solid 1px OldLace; border-bottom:solid 1px OldLace;">
                        <asp:Label runat="server" ID="lbl_uploads" ForeColor="Orange"/>
                        <div runat="server" ID="div_uploads"/>
                    </td>
                    <td valign="top" align="right" width="35%" style="border-left:solid 1px OldLace;">
                        <table runat="server" id="tbl_upload" cellpadding="0" cellspacing="0">
                            <tr>
                                <td align="left">
                                    <asp:Label runat="server" Text="Upload .doc, .docx, .xls, .xlsx, .pdf, .ppt, or .pptx file (20MB max):" ForeColor="Orange"/>
                                </td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <ajax:AsyncFileUpload ID="afu" runat="server" Width="325px"
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
                                    <asp:Label runat="server" ID="lbl_throbber" style="display:none; position:relative; top:9px;">
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
                    <td width="325px" valign="top" style="border-left:solid 1px OldLace; border-top:solid 1px OldLace; border-bottom:solid 1px OldLace;">
                        <asp:Label runat="server" ID="lbl_description" Text="Hover over a file for more information about the file type." />
                    </td>
                </tr>             
                <tr>
                    <td align="center" valign="top" colspan="2"> 
                        <%--Info Label--%>
                        <asp:Label runat="server" ID="lbl_opendoc" ForeColor="Orange" Text="<br/>Select a presentation to view it here or download a copy using the save icon.<br/><br/>" />
                    </td>
                </tr> 
                <tr>
                    <td valign="top" colspan="2"> 
                        <%--Presentation Content--%>
                        <div runat="server" id="div_googledocs" visible="false">
                            <table border="0" cellpadding="0" cellspacing="0" style="position:relative; left:2px;">
                                <tr>
                                    <td width="80">Frame Height</td>
                                    <td width="80">Frame Width</td>
                                    <td width="80">Apply Size</td>
                                    <td width="60">Close</td>
                                    <td width="640">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td><asp:TextBox runat="server" ID="tb_iframe_height" Width="70px" Text="750"/></td>
                                    <td><asp:TextBox runat="server" ID="tb_iframe_width" Width="70px" Text="976"/></td>
                                    <td><asp:Button runat="server" ID="btn_resize" Text="Set Size" OnClick="SetGoogleDocViewerSize" style="position:relative; left:-2px;"/></td>
                                    <td><asp:Button runat="server" ID="btn_closegoogle" Text="Close Presentation" OnClick="CloseGoogleDoc" Width="130" style="position:relative; left:-2px;"/></td>
                                    <td><asp:Label runat="server" ForeColor="Orange" Text="Note: if you can't view the content below you may need to " />
                                    <a style="color:Blue;" href="http://docs.google.com">log in</a>
                                    <asp:Label runat="server" ForeColor="Orange" Text="to your Google account first."/></td>
                                </tr>
                                <tr>
                                    <td colspan="5">                            
                                        <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                                            ControlToValidate="tb_iframe_height" ForeColor="Red" ErrorMessage="Height must be a valid number!"> 
                                        </asp:CompareValidator>
                                        <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                                            ControlToValidate="tb_iframe_width" ForeColor="Red" ErrorMessage="Width must be a valid number!"> 
                                        </asp:CompareValidator>
                                    </td>
                                </tr>
                            </table>
                            <iframe runat="server" ID="iframe_google" frameborder="1" src="" style="width:976px; height:750px;"/>
                        </div>
                    </td>
                </tr> 
            </table>

        <hr />
    </div>
</asp:Content>