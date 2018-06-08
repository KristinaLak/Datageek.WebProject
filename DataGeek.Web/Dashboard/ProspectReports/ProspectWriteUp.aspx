<%--
// Author   : Joe Pickering, 30/04/15
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="ProspectWriteUp.aspx.cs" Inherits="ProspectWriteUp" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

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
        	font-weight:bold;
        	color:Black;
        	font-size:small;
        }
        input[type=text], textarea
        {
            border:dotted 1px gray;
            background-color:white;
            border-radius:5px;
            color:#3e3e3e;
            font-family:Verdana;
            font-size:small;
            padding-left:4px;
        }
        input[type=text]:hover, textarea:hover
        {
            border:dotted 1px black;
            padding-left:4px;
            background-color:#ffcb8c;
            color:black;
            border-radius:5px;
            font-family:Verdana; 
            font-size:small;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="background:none;">   
    
        <table ID="tbl_main" runat="server" class="tbl">
            <tr><td colspan="4"><asp:Image ID="bzcl_img" runat="server" ImageUrl="~/images/misc/bizclik_logo_dark.png" style="position:relative; left:3px;"/></td></tr>
            <tr><td valign="bottom" align="center" colspan="4"><asp:Label ID="lbl_title" runat="server" CssClass="ttl"/></td></tr>
            <tr><td colspan="4" style="height:18px; border-top:solid 1px black; border-bottom:solid 1px black;" align="center">
                <asp:Label runat="server" Text="SECTION 1" CssClass="ttl"/>
            </td></tr>
            <tr>
                <td style="padding-top:5px;" width="130"><asp:Label runat="server" Text="Company Name:" CssClass="ttl"/></td>
                <td style="padding-top:5px;"><asp:TextBox ID="tb_company_name" runat="server" Width="250px" Enabled="false"/></td>
                <td style="padding-top:5px;"><asp:Label runat="server" Text="Rep Name:" CssClass="ttl"/></td>
                <td style="padding-top:5px;"><asp:TextBox ID="tb_rep" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Industry:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_industry" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="What They Do:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_description" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Suspect Date:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_suspect_date" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="LHA:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_lha" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Company Size:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_company_size" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Tel:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_tel" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Website:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_website" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Turnover:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_turnover" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Suspect Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_suspect_contact_name" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Position/Title:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_suspect_contact_job_title" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Direct Line:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_suspect_contact_phone" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_suspect_contact_email" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_suspect_contact_mob" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr><td colspan="4" style="height:18px; background:gray; border-top:solid 1px black;">&nbsp;</td></tr>
            <tr>
                <td colspan="4" style="height:18px; border-bottom:solid 1px black;" align="center">
                    <asp:Label runat="server" Text="'Top Floor to Shop Floor' Benchmark Interview Data" CssClass="ttl"/>
                </td>
            </tr>
            <tr><td colspan="4"><asp:TextBox ID="tb_benchmark_data" runat="server" Height="350" Width="99%" TextMode="MultiLine" Enabled="false"/></td></tr>
            <tr><td colspan="4" style="height:18px; background:gray; border-top:solid 1px black;">&nbsp;</td></tr>
            <tr>
                <td colspan="4" style="height:18px; border-bottom:solid 1px black;" align="center">
                    <asp:Label runat="server" Text="SECTION 2" CssClass="ttl"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Interviewee:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_interviewee_name" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Position/Title:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interviewee_job_title" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Direct Line:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interviewee_phone" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interviewee_email" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interviewee_mob" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="List Provider:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_list_provider_name" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Position/Title:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_list_provider_job_title" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Direct Line:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_list_provider_phone" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_list_provider_email" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_list_provider_mob" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="List Due Date:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_list_due" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Interview Point of Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_interview_poc_name" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Position/Title:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interview_poc_job_title" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Direct Line:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interview_poc_phone" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interview_poc_email" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_interview_poc_mob" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Pictures Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_pictures_contact_name" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Position/Title:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_pictures_contact_job_title" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Direct Line:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_pictures_contact_phone" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_pictures_contact_email" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Mobile:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_pictures_contact_mob" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr><td colspan="4" style="border-bottom:solid 1px gray; padding-bottom:2px;">&nbsp;</td></tr>
            <tr><td colspan="4"><asp:Label ID="lbl_footer" runat="server" Font-Size="Smaller" ForeColor="Gray" Text="<br/>%u_email%  |  www.bizclikmedia.com"/></td></tr>
 
        </table>
    </div>
    
</asp:Content>
