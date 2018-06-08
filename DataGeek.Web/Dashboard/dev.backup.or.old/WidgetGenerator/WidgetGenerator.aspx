<%--
Author   : Joe Pickering, 11/06/13
For      : DarkOrange Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Widget Generator" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="WidgetGenerator.aspx.cs" Inherits="WidgetGenerator" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="wide_page">   
        <hr />

        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="3">
                    <asp:Label runat="server" Text="Widget" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Generator" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td align="left" valign="top" colspan="3">
                    <asp:Label runat="server" Text="Select a feature from the Editorial Tracker and specify its widget resource locations.<br/>" ForeColor="AppWorkspace" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Click <b>Generate Widget</b> to build and preview the widget and then click <b>Upload this Widget</b> to confirm and upload the widget file to the Amazon S3 sever.<br/>" ForeColor="AppWorkspace" Font-Size="Small"/>
                    <asp:Label runat="server" Text="Once uploaded to the Amazon S3 sever you may send the customer the embed code for their widget (will appear on this page once uploaded).<br/><br/>" ForeColor="AppWorkspace" Font-Size="Small"/>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <table style="position:relative; left:-3px;">
                        <tr>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Office: "/></td>
                            <td><asp:DropDownList ID="dd_region" runat="server" Width="150" AutoPostBack="true" OnSelectedIndexChanged="BindIssues"/></td>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Issue: "/></td>
                            <td><asp:DropDownList ID="dd_issues" runat="server" Width="200" AutoPostBack="true" OnSelectedIndexChanged="BindFeatures"/></td>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Feature: "/></td>
                            <td>
                                <asp:DropDownList ID="dd_feature" runat="server" Width="300" AutoPostBack="true" OnSelectedIndexChanged="BindFeatureInfo"/>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_feature" ErrorMessage="<br/>Field required to build widget filename!" ForeColor="Red" Display="Dynamic"/>
                            </td>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Region: "/></td>
                            <td>
                                <asp:DropDownList ID="dd_feature_region" runat="server">
                                    <asp:ListItem Text=""/>
                                    <asp:ListItem Text="Africa"/>
                                    <asp:ListItem Text="Australia"/>
                                    <asp:ListItem Text="Brazil"/>
                                    <asp:ListItem Text="Canada"/>
                                    <asp:ListItem Text="Europe"/>
                                    <asp:ListItem Text="India"/>
                                    <asp:ListItem Text="Latino"/>
                                    <asp:ListItem Text="NA"/>
                                    <asp:ListItem Text="USA"/>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_feature_region" ErrorMessage="<br/>Field required to build widget filename!" ForeColor="Red" Display="Dynamic"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="3"><asp:Label runat="server" ForeColor="Bisque" Text="Blue numbers correspond to the elements of the widget shown in the example." Font-Bold="true"/></td></tr>
            <tr>
                <td width="190">
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Thumbnail Image URL"/>
                    <asp:Label runat="server" ForeColor="#81cffd" Text="(1)" Font-Bold="true"/>
                </td>
                <td colspan="2">
                    <asp:TextBox ID="tb_thumbnail_src" runat="server" Width="1005"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_thumbnail_src" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_thumbnail_src" Display="Dynamic" ErrorMessage="Invalid URL!"/>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Thumbnail URL"/>
                    <asp:Label runat="server" ForeColor="#81cffd" Text="(2)" Font-Bold="true"/>
                </td>
                <td colspan="2">
                    <asp:TextBox ID="tb_thumbnail_href" runat="server" Width="1005"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_thumbnail_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_thumbnail_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Digital Reader URL"/>
                    <asp:Label runat="server" ForeColor="#81cffd" Text="(3)" Font-Bold="true"/>
                </td>
                <td colspan="2">
                    <asp:TextBox ID="tb_digital_reader_href" runat="server" Width="1005"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_digital_reader_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_digital_reader_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Website URL"/>
                    <asp:Label runat="server" ForeColor="#81cffd" Text="(4)" Font-Bold="true"/>
                </td>
                <td colspan="2">
                    <asp:TextBox ID="tb_website_href" runat="server" Width="1005"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_website_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_website_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="PDF URL"/>
                    <asp:Label runat="server" ForeColor="#81cffd" Text="(5)" Font-Bold="true"/>
                </td>
                <td colspan="2">
                    <asp:TextBox ID="tb_pdf_href" runat="server" Width="1005" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_pdf_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_pdf_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td valign="top" colspan="2">
                    <asp:Button ID="btn_generate" runat="server" Text="Generate Widget" OnClick="GenerateWidget" 
                    OnClientClick="if(!Page_ClientValidate()){return confirm('Please fill in the required fields!');}"/>
                    <asp:CheckBox ID="cb_download" runat="server" Text="Download widget .html file on generate" ForeColor="DarkOrange" Checked="false" />
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:Label runat="server" Text="Example Widget:" ForeColor="Bisque" Font-Size="Large"/>
                    <asp:Image runat="server" ImageUrl="~/Images/Misc/widget_example.png"/>
                </td>
                <td valign="top">
                    <div id="div_generated" runat="server" visible="false">
                        <asp:Label runat="server" Text="Generated Widget:" ForeColor="Bisque" Font-Size="Large"/>
                        <asp:Literal ID="lit_generated_widget" runat="server"/>
                        <asp:Button ID="btn_upload" runat="server" Text="Upload this Widget" OnClick="UploadWidget"
                        OnClientClick="return confirm('Are you sure you wish to upload the widget to the Amazon s3 server?\n\nUploading should only take a few seconds.');"/>
                    </div>
                </td>
                <td valign="top">
                    <div id="div_uploaded" runat="server" visible="false">
                        <asp:Label runat="server" Text="Live Widget Preview (stored on Amazon s3 server):<br/>" ForeColor="Bisque" Font-Size="Large"/>
                        <asp:Literal ID="lit_uploaded_widget" runat="server"/>
                        
                        <asp:Label runat="server" Text="<br/>Embed link (iframe code, send this to the customer unless they ask for a direct link):<br/>" ForeColor="DarkOrange"/>
                        <asp:TextBox ID="tb_iframe_source" runat="server" Width="780" TextMode="MultiLine" Height="60" ReadOnly="true"/>
                        <asp:Label runat="server" Text="<br/>Direct link to uploaded HTML content:" ForeColor="DarkOrange"/>
                        <asp:TextBox ID="tb_url_source" runat="server" Width="780" ReadOnly="true"/>
                    </div>
                </td>
            </tr>
        </table>
        <hr />
        
    <asp:HiddenField ID="hf_file_name" runat="server" />
    <asp:Label runat="server" Text="<h3>Generated HTML Source:</h3>" ForeColor="DarkOrange" Visible="false"/>
    <asp:TextBox ID="tb_source" runat="server" TextMode="MultiLine" Height="500" Width="1040" Visible="false"/>
    
    </div>
</asp:Content>


