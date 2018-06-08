<%--
Author   : Joe Pickering, 08/08/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ETManageFootprint.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETManageFootprint" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <head runat="server"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
    <ContentTemplate>
    <table border="0" width="1150" style="font-family:Verdana; font-size:8pt; color:white; margin-left:auto; margin-right:auto; margin:15px;">
        <tr><td colspan="4" style="border-bottom:solid 1px gray; padding-bottom:5px;"><asp:Label ID="lbl_feature" runat="server" ForeColor="White" Font-Size="Larger" style="position:relative; left:-5px;"/></td></tr>
        <tr>
            <td colspan="3" style="padding-bottom:5px;"><asp:Label runat="server" Text="Basic Company Info" ForeColor="DarkOrange" style="position:relative; left:-5px; top:4px;"/></td>
            <td align="right"><div style="margin-right:4px;"><asp:Button runat="server" Text="Save" OnClick="VerifyAndSaveBasicInfo" CausesValidation="false"/></div></td>
        </tr>
        <tr>
            <td>E-mail Recipients:</td>
            <td colspan="3">
                <div style="width:890px; overflow-x:auto; overflow-y:hidden; padding:2px;">
                    <table cellpadding="0" cellspacing="0"><tr>
                        <td><asp:CheckBoxList ID="cbl_recipients" runat="server" RepeatDirection="Horizontal" ForeColor="White" Font-Size="8pt" style="position:relative; left:-6px;"/></td>
                    </tr></table>
                    <asp:Label ID="lbl_no_contacts" runat="server" Text="There are no contacts for this company, add some otherwise this information cannot be marked as verified." ForeColor="Red" Visible="false" />
                </div>
            </td>
        </tr>
        <tr>
            <td>E-mail Company Name:</td>
            <td><asp:TextBox ID="tb_company_name" runat="server" Width="200"/></td>
            <td width="15%">Issue Name:</td>
            <td><asp:TextBox ID="tb_issue_name" runat="server" ReadOnly="true" BackColor="LightGray" Width="200"/></td>
        </tr>
        <tr><td colspan="4" style="border-bottom:solid 1px gray;"><asp:Label Text="&nbsp;" runat="server" Font-Size="1pt"/></td></tr>
        <tr>
            <td colspan="3" style="padding-bottom:5px;">
                <asp:Label runat="server" Text="Territory Links E-mail Info" ForeColor="DarkOrange" style="position:relative; left:-5px;"/>
                <asp:CheckBox ID="cb_t_pub_req" runat="server" Text="Required?" ForeColor="White" AutoPostBack="true" OnCheckedChanged="VerifyAndSaveTerritory" style="position:relative; top:2px; left:-4px;"/>
            </td>
            <td align="right">
                <div style="margin-right:4px;">
                    <asp:LinkButton ID="lb_ter_force_sent" runat="server" Text="Force to <font color='lime'>Sent</font>" OnClick="ForceSetSent" CausesValidation="false" ForeColor="Silver" style="padding-right:5px; border-right:solid 1px gray;"/>
                    <asp:Label ID="lbl_ter_ver" runat="server"/>
                    <asp:Button ID="btn_ter_save" runat="server" Text="Verify and Save" OnClick="VerifyAndSaveTerritory" CausesValidation="false"/>
                    <br/><br/><br/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Territory Publication:</td>
            <td><asp:DropDownList ID="dd_territory_publication" runat="server" Width="230" AutoPostBack="true" OnSelectedIndexChanged="GetMagazineLinks"/></td>
            <td>Territory Mag Page No:</td>
            <td><asp:TextBox ID="tb_territory_page_no" runat="server" Width="100"/></td>
        </tr>
        <tr>
            <td>Territory Mag URL:</td>
            <td><asp:TextBox ID="tb_territory_mag" runat="server" ReadOnly="true" BackColor="LightGray" Width="330"/></td>
            <td>Territory Mag Cover Img:</td>
            <td><asp:TextBox ID="tb_territory_mag_img" runat="server" ReadOnly="true" BackColor="LightGray" Width="330"/></td>
        </tr>
        <tr><td colspan="4" style="border-bottom:solid 1px gray;"><asp:Label Text="&nbsp;" runat="server" Font-Size="1pt"/></td></tr>
        <tr>
            <td colspan="3" style="padding-bottom:5px;">
                <asp:Label runat="server" Text="Sector Links E-mail Info" ForeColor="DarkOrange" style="position:relative; left:-5px;"/>
                <asp:CheckBox ID="cb_s_pub_req" runat="server" Text="Required?" ForeColor="White" AutoPostBack="true" OnCheckedChanged="VerifyAndSaveSector" style="position:relative; top:2px; left:-4px;"/>
            </td>
            <td align="right">
                <div style="margin-right:4px;">
                    <asp:LinkButton ID="lb_sec_force_sent" runat="server" Text="Force to <font color='lime'>Sent</font>" OnClick="ForceSetSent" CausesValidation="false" ForeColor="Silver" style="padding-right:5px; border-right:solid 1px gray;" />
                    <asp:Label ID="lbl_sec_ver" runat="server"/>
                    <asp:Button ID="btn_sec_save" runat="server" Text="Verify and Save" OnClick="VerifyAndSaveSector" CausesValidation="false"/>
                    <br/><br/><br/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Sector Publication:</td>
            <td><asp:DropDownList ID="dd_sector_publication" runat="server" Width="230" AutoPostBack="true" OnSelectedIndexChanged="GetMagazineLinks"/></td>
            <td>Sector Mag Page No:</td>
            <td><asp:TextBox ID="tb_sector_page_no" runat="server" Width="100"/></td>
        </tr>
        <tr>
            <td>Sector Mag URL:</td>
            <td><asp:TextBox ID="tb_sector_mag" runat="server" ReadOnly="true" BackColor="LightGray" Width="330"/></td>
            <td>Sector Mag Cover Img:</td>
            <td><asp:TextBox ID="tb_sector_mag_img" runat="server" ReadOnly="true" BackColor="LightGray" Width="330"/></td>
        </tr>
        <tr><td colspan="4" style="border-bottom:solid 1px gray;"><asp:Label Text="&nbsp;" runat="server" Font-Size="1pt"/></td></tr>
        <tr>
            <td colspan="3" style="padding-bottom:5px;">
                <asp:Label runat="server" Text="Digital Footprint E-mail Info" ForeColor="DarkOrange" style="position:relative; left:-5px;"/>
                <asp:CheckBox ID="cb_footprint_req" runat="server" Text="Required?" ForeColor="White" AutoPostBack="true" OnCheckedChanged="VerifyAndSaveFootPrint" style="position:relative; top:2px; left:-4px;"/>
                <asp:Label runat="server" ForeColor="Bisque" Text="Blue numbers correspond to the elements of the widget shown in the example." Font-Size="7pt" Font-Bold="true" Visible="false"/>
            </td>
            <td align="right">
                <div ID="div_fp_verification" runat="server" style="margin-right:4px;">
                    <asp:LinkButton ID="lb_fp_force_sent" runat="server" Text="Force to <font color='lime'>Sent</font>" OnClick="ForceSetSent" CausesValidation="false" ForeColor="Silver" style="padding-right:5px; border-right:solid 1px gray;" />
                    <asp:Label ID="lbl_fp_ver" runat="server"/>
                    <asp:Button ID="btn_fp_save" runat="server" Text="Verify and Save" OnClick="VerifyAndSaveFootPrint" CausesValidation="false"/>
                </div>
                <div style="margin-right:4px;"><asp:Button ID="btn_brochure_save" runat="server" Text="Save Brochure Links" OnClick="SaveBrochureLinks" CausesValidation="false"/><br/><br/><br/></div>
            </td>
        </tr>  
        <tr>
            <td>Sector Brochure:</td>
            <td><asp:TextBox ID="tb_sector_brochure" runat="server" Width="330"/></td>
            <td>Sector Web Profile URL:</td>
            <td><asp:TextBox ID="tb_web_profile_url" runat="server" Width="330"/></td>
        </tr>
        <tr>
            <td width="190">
                <asp:Label runat="server" ForeColor="DarkOrange" Text="Brochure Thumbnail Image URL:"/>
                <%--<asp:Label runat="server" ForeColor="#81cffd" Text="(1)" Font-Bold="true"/>--%>
            </td>
            <td colspan="3">
                <asp:TextBox ID="tb_thumbnail_src" runat="server" Width="900"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_thumbnail_src" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_thumbnail_src" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label runat="server" ForeColor="DarkOrange" Text="Brochure URL:"/>
                <%--<asp:Label runat="server" ForeColor="#81cffd" Text="(3)" Font-Bold="true"/>--%>
            </td>
            <td colspan="3">
                <asp:TextBox ID="tb_digital_reader_href" runat="server" Width="900"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_digital_reader_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_digital_reader_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr ID="tr_widget_1" runat="server" visible="false">
            <td>
                <asp:Label runat="server" ForeColor="DarkOrange" Text="Web Profile URL:"/>
                <%--<asp:Label runat="server" ForeColor="#81cffd" Text="(4)" Font-Bold="true"/>--%>
            </td>
            <td colspan="3">
                <asp:TextBox ID="tb_website_href" runat="server" Width="900"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_website_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_website_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr ID="tr_widget_2" runat="server" visible="false">
            <td>
                <asp:Label runat="server" ForeColor="DarkOrange" Text="PDF URL:"/>
                <%--<asp:Label runat="server" ForeColor="#81cffd" Text="(5)" Font-Bold="true"/>--%>
            </td>
            <td colspan="3">
                <asp:TextBox ID="tb_pdf_href" runat="server" Width="900" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_pdf_href" ErrorMessage="<br/>Field Required!" ForeColor="Red" Display="Dynamic"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_pdf_href" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr ID="tr_widget_3" runat="server" visible="false">
            <td><asp:Label runat="server" ForeColor="#81cffd" Text="Widget URL:"/></td>
            <td colspan="3"><asp:TextBox ID="tb_url_source" runat="server" Width="900"/></td>                      
        </tr>
        <tr ID="tr_widget_4" runat="server" visible="false">
            <td valign="top">
                <asp:Label runat="server" ForeColor="#81cffd" Text="Widget Iframe (embed):<br/>"/>
                <asp:CheckBox ID="cb_download" runat="server" Text="Download on generate?" ForeColor="Bisque" Checked="false" style="position:relative; left:-2px; top:0px;"/>
                <br />
                <asp:Button ID="btn_generate" runat="server" Text="Generate Widget" OnClick="GenerateWidget" 
                OnClientClick="if(!Page_ClientValidate()){return confirm('Please fill in the required widget fields!');}"/>
                <br />
                <asp:Button ID="btn_preview_live" runat="server" Text="Preview Uploaded Widget" Visible="false" OnClick="PreviewLiveWidget" 
                OnClientClick="alert('If the preview does not appear, you will need to allow pop-ups on this page (Happens by default with AdBlock Plus)'); return true;" style="font-size:8pt;"/>
            </td>
            <td colspan="3"><asp:TextBox ID="tb_iframe_source" runat="server" Width="900" TextMode="MultiLine" Height="60" style="overflow:visible !important; font-size:8pt !important;"/></td>                      
        </tr>
        <tr><td colspan="4" style="border-bottom:solid 1px gray;"><asp:Label Text="&nbsp;" runat="server" Font-Size="1pt"/></td></tr>
        <tr ID="tr_widget" runat="server" visible="false">
            <td valign="top">
                <asp:Label runat="server" Text="Example Widget:" ForeColor="Bisque" Font-Size="Large"/>
                <asp:Image runat="server" ImageUrl="~/Images/Misc/widget_example.png"/>
                <asp:Button ID="btn_cancel_widget" runat="server" Text="Cancel Widget Generation" OnClick="CancelWidget" style="position:relative; top:4px;"/>
            </td>
            <td valign="top" colspan="3">
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td valign="top">
                            <div id="div_generated" runat="server" visible="false">
                                <asp:Label runat="server" Text="Generated Widget:" ForeColor="Bisque" Font-Size="Large"/>
                                <asp:Label ID="lbl_generated_widget" runat="server"/>
                                <asp:Button ID="btn_upload" runat="server" Text="Upload this Widget" OnClick="UploadWidget"
                                OnClientClick="return confirm('Are you sure you wish to upload the widget to the Amazon s3 server?\n\nUploading should only take a few seconds.');"/>
                            </div>
                        </td>
                        <td valign="top">
                            <div id="div_uploaded" runat="server" visible="false">
                                <asp:Label runat="server" Text="Live Widget (stored on Amazon s3 server):<br/>" ForeColor="Lime" Font-Size="Large"/>
                                <asp:Label ID="lbl_uploaded_widget" runat="server" style="position:relative; top:-7px;"/>
                                <asp:Button ID="btn_preview" runat="server" Text="Preview Live Widget" OnClick="PreviewLiveWidget" style="position:relative; top:-10px; left:7px;"/>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_cpy_id" runat="server" />
    <asp:HiddenField ID="hf_issue_id" runat="server"/>
    <asp:HiddenField ID="hf_issue_region" runat="server"/>
    <asp:HiddenField ID="hf_region" runat="server"/>
    <asp:HiddenField ID="hf_file_name" runat="server"/>
    <asp:HiddenField ID="hf_original_email_name" runat="server"/>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>