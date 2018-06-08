<%--
Author   : Joe Pickering, 19/07/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Group Performance Report" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="GPRExcel.aspx.cs" Inherits="GPRExcel" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="normal_page">   
        <hr />

        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="Group Performance" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px; left:1px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px; left:1px;"/> 
                </td>
            </tr>
            <tr>
                <td>
                    <%-- File info table--%>
                    <table width="300" border="1" bgcolor="#444444" style="color:DarkOrange;">
                        <tr>
                            <td><asp:Image runat="server" ImageUrl="~/Images/Icons/excel.png"/></td>
                            <td><asp:Label ID="lbl_filename" runat="server" Font-Size="Large"/></td>
                        </tr>
                        <tr><td colspan="2">
                            <div runat="server" id="div_in_use">
                                <asp:Label runat="server" Text="Excel file currently in use!<br/>Please click " ForeColor="Red" Font-Size="Large"/>
                                <asp:HyperLink runat="server" Text="here" NavigateUrl="~/Dashboard/GPR/GPR.aspx" Font-Bold="true" Font-Size="Large" />
                                <asp:Label runat="server" Text=" to retry.<br/>" ForeColor="Red" Font-Size="Large"/>
                            </div>
                            <asp:Label ID="lbl_file_info" runat="server"/>
                        </td></tr>
                        <tr>
                            <td>Sheet List:&nbsp;</td>
                            <td>
                                <asp:DropDownList ID="dd_sheets" runat="server"/>
                                <asp:Label ID="lbl_num_sheets" runat="server"/>
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Button ID="btn_preview_sheet" runat="server" Text="Preview Excel Sheet" OnClick="PreviewExcelSheet"/></td>
                            <td><asp:Button ID="btn_download" runat="server" Text="Download Excel File" OnClick="DownloadFile"/></td>
                        </tr>
                    </table>
                </td>
                <td valign="top" align="right">
                    <table border="1" bgcolor="#444444" style="color:DarkOrange;">
                        <tr><td align="left"><asp:Label runat="server" Text="Upload GroupPerformance.xlsx file." Font-Size="Large" ForeColor="DarkOrange"/></td></tr>
                        <tr>
                            <td align="center">
                                <ajax:AsyncFileUpload ID="afu" runat="server" Width="325px"
                                OnClientUploadError="UploadError" OnClientUploadComplete="UploadComplete" OnUploadedComplete="OnUploadComplete"
                                UploaderStyle="Modern" UploadingBackColor="#CCFFFF" ThrobberID="lbl_throbber"/>
                            </td>
                        </tr>
                        <tr>
                            <td align="center">
                                <asp:Label runat="server" ID="lbl_throbber" style="display:none;">
                                     Uploading... &nbsp;<img alt="" src="/Images/Misc/uploading.gif"/>
                                </asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td align="left">
                                <br />Click "Select File" and browse for your updated <b>GroupPerformance.xlsx</b> document.<br />
                                Re-uploading this file will overwrite the existing file stored on the server. 
                                <br /><br />The file must be named exactly <b>GroupPerformance.xlsx</b>.
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
            <tr><td colspan="2" style="border-top:dotted 1px gray;"></td></tr>
            <tr>
                <td>
                    <asp:Label runat="server" Text="Yesterday's Stats:" Font-Size="Large" ForeColor="DarkOrange"/>
                    <asp:Label ID="lbl_day" runat="server" Font-Size="Large" ForeColor="Firebrick"/>         
                    <asp:DropDownList ID="dd_day_override" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindGPR"/> 
                    <asp:Label runat="server" Font-Size="Large" ForeColor="Firebrick" Text="Override"/>                   
                </td>
                <td align="right">
                    <asp:Button runat="server" Text="New Sheet" Visible="false" />
                    <asp:Label runat="server" Text="Month Length:" ForeColor="DarkOrange" Visible="false" />
                    <asp:DropDownList ID="dd_month_length" runat="server" Visible="false"> <%--AutoPostBack="true" OnSelectedIndexChanged="CreateNewSheet"--%>
                        <asp:ListItem Text="20" Selected="True" />
                        <asp:ListItem Text="25" />
                    </asp:DropDownList>
                    <asp:Label runat="server" Text="Select Issue:" ForeColor="DarkOrange" />
                    <asp:DropDownList ID="dd_issue" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindGPR" />
                </td>
            </tr>
            <tr>
                <td colspan="2" align="center">     
                    <table ID="tbl_performance" runat="server" cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                        <tr>
                            <td valign="top">                 
                                <asp:Label runat="server" Text="Daily Performance:" ForeColor="Firebrick"/>
                                <asp:GridView ID="gv_day_stats" runat="server" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" AutoGenerateColumns="false"
                                Cellpadding="2" Border="2" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" OnRowDataBound="gv_stats_RowDataBound"
                                HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" Width="375">    
                                <Columns>
                                    <asp:BoundField HeaderText="Region" DataField="Office" ItemStyle-Width="70"/>
                                    <asp:BoundField HeaderText="Total (USD)" DataField="Total" ItemStyle-Width="60"/>
                                    <asp:TemplateField HeaderText="Override (USD)">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tb" runat="server" Width="90" Height="18" BackColor="LightGray" ReadOnly="true" Text='<%# Eval("Override") %>'/>
                                        </ItemTemplate>    
                                    </asp:TemplateField>
                                    <asp:BoundField HeaderText="Week (USD)" DataField="TotalWeek" ItemStyle-Width="60"/>
                                    <asp:BoundField HeaderText="S" DataField="S" ItemStyle-Width="20"/>
                                    <asp:BoundField HeaderText="P" DataField="P" ItemStyle-Width="20"/>
                                    <asp:BoundField HeaderText="A" DataField="A" ItemStyle-Width="20"/>
                                </Columns>
                                </asp:GridView>
                            </td>
                            <td valign="top" width="0">                                
                                <asp:GridView ID="gv_book_stats" runat="server" Visible="false" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" AutoGenerateColumns="false"
                                Cellpadding="2" Border="2" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" OnRowDataBound="gv_stats_RowDataBound"
                                HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0"> 
                                <Columns>
                                    <asp:BoundField HeaderText="Region" DataField="Office" ItemStyle-Width="90"/>
                                    <asp:BoundField HeaderText="Total (USD)" DataField="Total"/>
                                    <asp:TemplateField HeaderText="Override (USD)">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tb" runat="server" Width="90" Height="18" BackColor="LightGray" ReadOnly="true" Text='<%# Eval("Override") %>'/>
                                        </ItemTemplate>    
                                    </asp:TemplateField>
                                </Columns>
                                </asp:GridView>
                            </td>
                            <td valign="top">
                                <asp:Label runat="server" Text="Book to Date Stats:" ForeColor="Firebrick"/>
                                <asp:GridView ID="gv_overall_stats" runat="server" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" AutoGenerateColumns="false" 
                                Cellpadding="2" Border="2" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" OnRowDataBound="gv_overall_stats_RowDataBound"
                                HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" Width="610"> 
                                <Columns>
                                    <asp:BoundField HeaderText="Region" DataField="Office" ItemStyle-Width="90"/>
                                    <asp:BoundField HeaderText="Budget (USD)" DataField="Budget"/>
                                    <asp:BoundField HeaderText="RR Pred. (USD)" DataField="RR Prediction"/>
                                    <asp:BoundField HeaderText="Actual (USD)" DataField="Total"/>
                                    <asp:BoundField HeaderText="Index v Budget" DataField="Index v Budget"/>
                                    <asp:BoundField HeaderText="Index v RR" DataField="Index v RR" ItemStyle-BackColor="#d8d8d8"/>
                                </Columns>
                                </asp:GridView>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="center">
                    <telerik:RadChart runat="server" ID="rc_performance" PlotArea-XAxis-AutoScale="false" PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="-40"
                    IntelligentLabelsEnabled="true" Autolayout="True" PlotArea-YAxis-Appearance-CustomFormat="$###,###" 
                    SkinsOverrideStyles="False" Skin="Mac" Height="400px" Width="986px"/>
                </td>
            </tr>       
            <tr>
                <td colspan="2" style="border-top:dotted 1px gray;">   
                    <%--E-mailing & Saving --%>
                    <table>
                        <tr><td><asp:Label runat="server" Text="Save and E-mail:" Font-Size="Large" ForeColor="DarkOrange"/></td></tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Mail To:" ForeColor="DarkOrange"/>
                                <asp:TextBox ID="tb_email_to" runat="server" Height="90" Width="978" TextMode="MultiLine" 
                                Text="glen@bizclikmedia.com;"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Mail Message:" ForeColor="DarkOrange"/>
                                <asp:TextBox ID="tb_email_message" runat="server" Height="140" Width="978" TextMode="MultiLine"/>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Button ID="btn_save_to_excel" runat="server" Text="Save Stats to Sheet" OnClick="SaveStatsToExcelSheet" OnClientClick="return confirm('Are you sure?');"/>
                                <asp:Button ID="btn_send_email" runat="server" Text="Send E-mail" OnClick="SendEmail" OnClientClick="return confirm('Are you sure?');"/>
                                <asp:Button ID="btn_save_and_email" runat="server" Text="Save Stats and Send in E-mail" OnClick="SaveStatsAndEmail" OnClientClick="return confirm('Are you sure? This may take a minute.');"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2" style="border-top:dotted 1px gray;">
                    <%--Preview--%>
                    <div ID="div_preview" runat="server" visible="false">
                        <table>
                            <tr>
                                <td align="left" valign="bottom">
                                    <asp:Label runat="server" Text="Preview:" Font-Size="Large" ForeColor="DarkOrange"/>
                                    <asp:Label ID="lbl_preview" runat="server" ForeColor="DarkOrange"/>
                                </td>
                                <td align="right"><asp:Button ID="btn_close_preview" runat="server" Text="Close Sheet Preview" OnClick="ClosePreview" style="position:relative; left:3px;"/></td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <div style="overflow:auto; width:984px;">
                                        <asp:GridView runat="server" ID="gv_excel" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                        Cellpadding="2" Border="2" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" RowStyle-BackColor="#f0f0f0"/>    
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <a name="a_preview"></a>
                    </div>
                </td>
            </tr>
        </table>
        <hr />
    </div>
    
    <asp:Button ID="btn_refresh" runat="server" OnClick="BindGPR" style="display:none;"/>     
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
            grab("<%= btn_refresh.ClientID %>").click();
        }
    </script>
</asp:Content>


