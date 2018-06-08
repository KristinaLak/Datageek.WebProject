<%--
Author   : Joe Pickering, 13/06/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="BizClik Media Feedback Survey" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="FeedbackSurvey.aspx.cs" Inherits="FeedbackSurvey" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .outer_t
        {
        	 width:80%;
        	 height:100%;
        	 margin-left:auto; 
        	 margin-right:auto;
        	 margin-top:45px;
        	 margin-bottom:40px;
        	 border:dotted 1px gray;
        	 background-color:#101010;
        }
        .outer_t h1
        {
            background-color:#bc202d;
            padding:10px 10px 10px 10px;
            font-size:24px;
            line-height:30px;
            margin-top:0px;
            padding:10px 20px;
        }
        .inner_t 
        {
        	margin-right:auto;
        	margin-left:auto;
            width:94%;
            font-size:10pt;
        }
        .td_title  
        {
        	padding-top:10px;
        	font-weight:bold; 
        }
        .td_content { padding:2px; }
        select { padding:3px; }
        select:hover { background-color:#ffcb8c; }
        input[type=text], textarea
        {
        	padding:3px;
        	width:200px;
        }
        input[type=text]:hover, textarea:hover { background-color:#ffcb8c; }
        .rating_t { color:#a4bfdd; }
        .rating_t td
        {
        	padding:2px;
        	padding-top:4px;
        	padding-bottom:4px;
        	border-bottom:solid 1px #a4bfdd;
        }
        .rating_t table td { border-bottom:0px; }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div style="background:url('/Images/Backgrounds/Graywood.png') no-repeat fixed;">
        <div id="div_page" runat="server" class="normal_page" style="background:transparent; color:white; font-family:Verdana;">
            <table class="outer_t" cellpadding="0" cellspacing="0">
                <tr><td align="center"><asp:Label ID="lbl_feedback_entry" runat="server" Visible="false" ForeColor="BurlyWood" Font-Size="12pt"/></td></tr>
                <tr><td bgcolor="white" style="border:solid 1px transparent;"><asp:Image runat="server" ImageUrl="~/images/misc/bizclik_logo_dark.png" style="padding:9px;"/></td></tr>
                <tr><td><h1><asp:Label ID="lbl_form_title" runat="server" Text="Testimonial Survey" /></h1></td></tr>
                <tr>
                    <td>
                        <table class="inner_t">
                            <tr>
                                <td>
                                    <asp:Label ID="lbl_introduction" runat="server" 
                                    Text="We hope you enjoyed working with us, as much as we did with you. 
                                    At BizClik Media we are always striving to improve our products and customer service and your feedback is extremely important to us. 
                                    It would be much appreciated if you could take a few moments to fill out this short feedback survey. 
                                    If you have any questions please don’t hesitate to contact us. Thank you."/><br /><br />
                                </td>
                            </tr>
                            <tr><td><asp:Label ID="lbl_required" runat="server" ForeColor="#f6a099" Text="* Required" /></td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_sector_publication" runat="server" Text="Industry Publication in which you were featured"/>: <asp:Label runat="server" ForeColor="#f6a099" Text="*"/></td></tr>
                            <tr><td class="td_content">
                                <asp:DropDownList ID="dd_channel_mag" runat="server" Width="250" />
                                <asp:RequiredFieldValidator ID="rfv_channel_mag" runat="server" ControlToValidate="dd_channel_mag" ForeColor="Red" Text="*" Display="Dynamic"/>
                            </td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_territory_publication" runat="server" Text="What Territory Publication did you work with"/>?</td></tr>
                            <tr><td class="td_content"><asp:DropDownList ID="dd_territory_mag" runat="server" Width="250" /></td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_company_name" runat="server" Text="Company Name" />: <asp:Label runat="server" ForeColor="#f6a099" Text="*"/></td></tr>
                            <tr><td class="td_content">
                                <asp:TextBox ID="tb_company" runat="server"/>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_company" ForeColor="Red" Text="*" Display="Dynamic"/>
                            </td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_name" runat="server" Text="Name" />: <asp:Label runat="server" ForeColor="#f6a099" Text="*"/></td></tr>
                            <tr><td class="td_content">
                                <asp:TextBox ID="tb_name" runat="server"/>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_name" ForeColor="Red" Text="*" Display="Dynamic"/>
                            </td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_title" runat="server" Text="Job Title" />: <asp:Label runat="server" ForeColor="#f6a099" Text="*"/></td></tr>
                            <tr><td class="td_content">
                                <asp:TextBox ID="tb_title" runat="server"/>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_title" ForeColor="Red" Text="*" Display="Dynamic"/>
                            </td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_experience_notes" runat="server" ForeColor="Gray" Text="Please describe your experience of working with us"/></td></tr>
                            <tr><td class="td_content"><asp:TextBox ID="tb_testimonial" runat="server" TextMode="MultiLine" Height="180" Width="600"/></td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_experience_rating" runat="server" Text="How would you rate your overall experience"/>?</td></tr>
                            <tr><td class="td_content">
                                <table class="rating_t" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td align="center">1</td>
                                        <td align="center">2</td>
                                        <td align="center">3</td>
                                        <td align="center">4</td>
                                        <td align="center">5</td>
                                        <td>&nbsp;</td>
                                    </tr>
                                    <tr>
                                        <td><asp:Label ID="lbl_poor" runat="server" Text="Poor" /></td>
                                        <td colspan="5">
                                            <asp:RadioButtonList ID="rbl_experience_rating" runat="server" RepeatDirection="Horizontal" EnableViewState="true" ForeColor="Black" Font-Size="0pt">
                                                <asp:ListItem Value="1"/>
                                                <asp:ListItem Value="2"/>
                                                <asp:ListItem Value="3" Selected="True"/>
                                                <asp:ListItem Value="4"/>
                                                <asp:ListItem Value="5"/>
                                            </asp:RadioButtonList>
                                        </td>
                                        <td><asp:Label ID="lbl_excellent" runat="server" Text="Excellent" /></td>
                                    </tr>
                                </table>
                            </td></tr>
                            <tr><td class="td_title"><asp:Label ID="lbl_recommend" runat="server" Text="Would you recommend our product to other companies" />?</td></tr>
                            <tr><td class="td_content">
                                <asp:RadioButtonList ID="rbl_recommendation" runat="server" BulletStyle="Disc" RepeatDirection="Vertical">
                                    <asp:ListItem Text="Yes"></asp:ListItem>
                                    <asp:ListItem Text="No"></asp:ListItem>
                                    <asp:ListItem Text="Maybe" Selected="True"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td></tr>
                            <tr><td class="td_title">
                                <asp:Label ID="lbl_subscriptions" runat="server" Text="Would you like to receive any of our digital magazines in the future for FREE"/>?<br/>
                                <asp:Label ID="lbl_subscription_selections" runat="server" ForeColor="Gray" Text="Please select the magazines that you would like to receive"/>
                            </td></tr>
                            <tr><td class="td_content">
                                <asp:CheckBoxList ID="cbl_subscribe" runat="server"/>
                            </td></tr>
                            <tr><td class="td_title">
                                <asp:Label ID="lbl_email" runat="server" Text="If you selected any of the above digital magazines, please enter your e-mail address to which you would like to receive the digital magazine(s)"/>:<br/>
                                <asp:Label ID="lbl_enter_email" runat="server" ForeColor="Gray" Text="Please enter a valid e-mail address (you can cancel your free subscription(s) at any time)"/>
                            </td></tr>
                            <tr><td class="td_content">
                                <asp:TextBox ID="tb_email" runat="server" Width="300"/>
                                <asp:RegularExpressionValidator runat="server" 
                                ValidationExpression='<%# Util.regex_email %>'
                                ControlToValidate="tb_email" Font-Size="Smaller" ForeColor="Red" ErrorMessage="<br/>Invalid e-mail format!" Display="Dynamic"/>
                            </td></tr>
                            <tr><td align="right"><asp:Button ID="btn_submit" runat="server" Text="Submit Survey" OnClick="SubmitSurvey" style="position:relative; left:25px; padding:5px; margin:5px;"
                            OnClientClick="if(!Page_ClientValidate()){return confirm('Please fill in the *s!');}" /></td></tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <asp:HiddenField ID="hf_company_id" runat="server" />
    <asp:HiddenField ID="hf_feedback_entry_id" runat="server" />
    <asp:HiddenField ID="hf_language" runat="server" Value="English" />
</asp:Content>


