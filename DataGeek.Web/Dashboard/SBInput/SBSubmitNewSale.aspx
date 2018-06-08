<%--
Author   : Joe Pickering, 12/09/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Submit New Sale" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SBSubmitNewSale.aspx.cs" Inherits="SubmitNewSale" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .tbl
        {
        	border:dashed 1px gray;
        	color:Black;
        	background-color:White;
        	width:80%;
        	margin-left:auto; 
        	margin-right:auto;
        	margin-top:40px;
        	margin-bottom:40px;
        	padding:5px 5px 5px 5px;
        }
        .ttl
        {
        	color:GrayText;
        	font-size:small;
        }
        input[type=text], textarea
        {
            border:dotted 1px gray;
            background-color:#f3f3f3;
            border-radius:5px;
            color:#3e3e3e;
            font-family:Verdana;
            font-size:small;
        }
        input[type=text]:hover, textarea:hover
        {
            border:dotted 1px black;
            background-color:#ffcb8c;
            color:black;
            border-radius:5px;
            font-family:Verdana; 
            font-size:small;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page">   
    
        <table ID="tbl_main" runat="server" class="tbl" style="padding:15px;">
            <tr><td colspan="4" style="border-bottom:solid 1px gray; padding-bottom:4px;"><asp:Image ID="bzcl_img" runat="server" ImageUrl="~/images/misc/bizclik_logo_dark.png" style="position:relative; left:3px;"/></td></tr>
            <tr>
                <td><asp:Label runat="server" Text="Advertiser:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox ID="tb_advertiser" runat="server" Width="200"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_advertiser" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td valign="top"><asp:Label runat="server" Text="Feature:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox ID="tb_feature" runat="server" style="width:178px; position:absolute;"/>
                    <asp:DropDownList id="dd_feature" runat="server" Width="200" OnChange="javascript:setDDText('dd_feature');"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_feature" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="List Gen:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_list_gen" runat="server" Width="200"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_list_gen" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td width="110"><asp:Label runat="server" Text="Rep:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox ID="tb_rep" runat="server" style="width:178px; position:absolute;"/>
                    <asp:DropDownList ID="dd_rep" runat="server" Width="200px" EnableViewState="true" OnChange="javascript:setDDText('dd_rep');"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_rep" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Country:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_country" runat="server" Width="200"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Address:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_address" runat="server" Width="614" TextMode="MultiLine" Height="40"/></td>
            </tr>
            <tr><td colspan="4"><br /><asp:Label runat="server" Text="Book" CssClass="ttl" Font-Italic="true"/></td></tr>
            <tr>
                <td><asp:Label runat="server" Text="Office:" CssClass="ttl"/></td>
                <td><asp:DropDownList ID="dd_office" runat="server" Width="200" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Issue:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_issue" runat="server" Width="200"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_issue" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Magazine:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_territory_mag" runat="server" Width="110px"/>
                    &nbsp;<asp:Label runat="server" Text="(Business Chief)" CssClass="ttl"/>
                </td>
                <td><asp:Label runat="server" Text="Magazine:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_channel_mag" runat="server" Width="110px"/>
                    &nbsp;<asp:Label runat="server" Text="(Channel Mag)" CssClass="ttl"/>
                </td>
            </tr>
            <tr ID="tr_magazine_note" runat="server" visible="false">
                <td><asp:Label runat="server" Text="Magazine:" CssClass="ttl"/></td>
                <td colspan="3">
                    <asp:TextBox ID="tb_magazine_note" runat="server" Width="150" />
                    <asp:Label runat="server" Text="Add a note indicating which mag this sale will appear in." ForeColor="Gray" />
                    <asp:RequiredFieldValidator ID="rfv_magazine_note" runat="server" Enabled="false" ControlToValidate="tb_magazine_note" Font-Size="Smaller" 
                    ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr><td colspan="4"><br /><asp:Label runat="server" Text="Confirmation Contact" CssClass="ttl" Font-Italic="true"/></td></tr>
            <tr>
                <td><asp:Label runat="server" Text="Contact:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_c_contact" Width="250"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_c_contact" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Tel:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_c_tel" Width="250px"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_c_tel" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_c_email" Width="250"/>
                    <asp:RegularExpressionValidator runat="server" 
                    ValidationExpression='<%# Util.regex_email %>'
                    ControlToValidate="tb_c_email" Font-Size="Smaller" ForeColor="Red" ErrorMessage="<br/>Invalid e-mail format!For multiple addresses use semicolons ;" Display="Dynamic"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_c_email" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_c_mob" Width="250px"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Fax:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_c_fax" Width="200"/></td>
            </tr>
            <tr>
                <td><br /><asp:Label runat="server" Text="Artwork" CssClass="ttl" Font-Italic="true"/></td>
                <td colspan="3">
                    <asp:CheckBox ID="cb_dctc_same" runat="server" Checked="false" Text="Same as confirmation contact" CausesValidation="false" 
                    ForeColor="Gray" Font-Size="Smaller" AutoPostBack="true" OnCheckedChanged="ToggleMirroredConfirmationContact" style="position:relative; top:9px; left:-3px;"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Contact:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_d_contact" Width="250"/>
                    <asp:RequiredFieldValidator ID="rfv_d_contact" runat="server" ControlToValidate="tb_d_contact" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Tel:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_d_tel" Width="250px"/>
                    <asp:RequiredFieldValidator ID="rfv_d_tel" runat="server" ControlToValidate="tb_d_tel" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_d_email" Width="250"/>
                    <asp:RegularExpressionValidator ID="rev_d_email" runat="server" 
                    ValidationExpression='<%# Util.regex_email %>'
                    ControlToValidate="tb_d_email" Font-Size="Smaller" ForeColor="Red" ErrorMessage="<br/>Invalid e-mail format!For multiple addresses use semicolons ;" Display="Dynamic"/>
                    <asp:RequiredFieldValidator ID="rfv_d_email" runat="server" ControlToValidate="tb_d_email" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_d_mob" Width="250px"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Edit Mention:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_edit_mention" Width="250"/></td>
                <td><asp:Label runat="server" Text="Ad Makeup:" CssClass="ttl"/></td>
                <td><asp:CheckBox ID="cb_admakeup" runat="server" Checked="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Size:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList id="dd_size" runat="server" Width="110px">
                        <asp:ListItem Text="None (special case)" Value="0"/>
                        <asp:ListItem Text="Quarter Page" Value="0.25" Selected="True"/>
                        <asp:ListItem Text="Half" Value="0.5"/>
                        <asp:ListItem Text="FPC" Value="1"/>
                        <asp:ListItem Text="DPS" Value="2"/>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_size" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Info:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_info" runat="server" Width="122px">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="Platinum"/>
                        <asp:ListItem Text="Gold"/>
                        <asp:ListItem Text="Silver"/>
                    </asp:DropDownList>            
                </td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Artwork Note:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_d_notes" runat="server" Width="614" TextMode="MultiLine" Height="50"/></td>
            </tr>
            <tr>
                <td><br /><asp:Label runat="server" Text="Accounts" CssClass="ttl" Font-Italic="true"/></td>
                <td colspan="3">
                    <asp:CheckBox ID="cb_fctc_same" runat="server" Checked="false" Text="Same as artwork contact" CausesValidation="false" ForeColor="Gray" 
                    Font-Size="Smaller" AutoPostBack="true" OnCheckedChanged="ToggleMirroredArtworkContact" style="position:relative; top:9px; left:-3px;"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Contact:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_f_contact" Width="250"/>
                    <asp:RequiredFieldValidator ID="rfv_f_contact" runat="server" ControlToValidate="tb_f_contact" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Tel:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_f_tel" Width="250px"/>
                    <asp:RequiredFieldValidator ID="rfv_f_tel" runat="server" ControlToValidate="tb_f_tel" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox runat="server" ID="tb_f_email" Width="250"/>
                    <asp:RegularExpressionValidator ID="rev_f_email" runat="server" 
                    ValidationExpression='<%# Util.regex_email %>'
                    ControlToValidate="tb_f_email" Font-Size="Smaller" ForeColor="Red" ErrorMessage="<br/>Invalid e-mail format!For multiple addresses use semicolons ;" Display="Dynamic"/>
                    <asp:RequiredFieldValidator ID="rfv_f_email" runat="server" ControlToValidate="tb_f_email" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_f_mob" Width="250px"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Price:" CssClass="ttl"/></td>
                <td>
                    <asp:TextBox ID="tb_price" runat="server" Width="116px"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_price" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                    <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                    ControlToValidate="tb_price" ForeColor="Red" Font-Size="Smaller" ErrorMessage="Price must be positive or zero"/> 
                </td>
                <td><asp:Label runat="server" Text="VAT NO:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_vat_no" runat="server" Width="200"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Accounts Note:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_f_notes" runat="server" Width="614" TextMode="MultiLine" Height="50"/></td>
            </tr>
            <tr>
                <td colspan="4"><br /><asp:Label runat="server" Text="Submit Info" CssClass="ttl" Font-Italic="true"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Submit To:" CssClass="ttl"/></td>
                <td colspan="3">
                    <asp:DropDownList ID="dd_s_to" runat="server" Width="250" TextMode="MultiLine"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_s_to" Font-Size="Smaller" ErrorMessage="*" Display="Dynamic" ForeColor="Red"/>
                </td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Submit Notes:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_s_notes" runat="server" Width="614" TextMode="MultiLine" Height="50"/></td>
            </tr>
            <tr>
                <td colspan="4" align="right">
                    <asp:Button ID="btn_submit" runat="server" Text="Submit for Approval" OnClick="Submit" 
                    OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to submit this sale?');}else{alert('Please fill in the required fields (*).');}"/>
                </td>
            </tr>
            <tr id="tr_return" runat="server" visible="false">
                <td align="center" colspan="4">
                    <p/>
                        <asp:Label runat="server" Text="Submission sent successfully!<br/>Click" ForeColor="#bc1f2c" Font-Size="12" />
                        <asp:HyperLink runat="server" Text="here" ForeColor="Blue" NavigateUrl="~/Default.aspx" Font-Size="12"/>
                        <asp:Label runat="server" Text="to return to the main page." ForeColor="#bc1f2c" Font-Size="12"/>
                    <p/>
                </td>
            </tr>            
            <tr id="tr_back" runat="server" visible="false">
                <td align="center" colspan="4">
                    <p/>
                        <asp:Label runat="server" Text="Submission failed to send!<br/>Click" ForeColor="#bc1f2c" Font-Size="12" />
                        <asp:LinkButton runat="server" Text="here" ForeColor="Blue" Font-Size="12" OnClientClick="window.history.back(); return false;"/>
                        <asp:Label runat="server" Text="to retry the submission." ForeColor="#bc1f2c" Font-Size="12"/>
                    <p/>
                </td>
            </tr>
        </table>
    </div>
    
    <script type="text/javascript">
        function setDDText(DropDownID) {
            var dropDown = null;
            var textBox = null;
            switch (DropDownID) {
                case 'dd_feature':
                    dropDown = grab("<%= dd_feature.ClientID %>");
                    textBox = grab("<%= tb_feature.ClientID %>");
                    var lg = grab("<%= dd_list_gen.ClientID %>"); 
                    for (var x = 0; x < lg.length; x++) {
                        if (lg.options[x].text == dropDown.options[dropDown.selectedIndex].value)
                        { lg.selectedIndex = x; }
                    }
                    break;
                case 'dd_rep':
                    dropDown = grab("<%= dd_rep.ClientID %>");
                    textBox = grab("<%= tb_rep.ClientID %>");
                    break;
                default:
                    break;
            }
            textBox.value = dropDown.options[dropDown.selectedIndex].text;
            return false;
        }
    </script> 
</asp:Content>


