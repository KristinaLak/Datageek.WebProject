<%--
Author   : Joe Pickering, 19/04/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNMailingOptions.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNMailingOptions" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <div style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin-left:auto; margin-right:auto; padding:15px;">
        <table id="tbl_cbs" runat="server" width="500">
            <tr>
                <td colspan="3"><asp:Label ID="lbl_sale_details" runat="server" ForeColor="DarkOrange"/><br /><br /></td>
                <td><asp:LinkButton ID="lb_view_templates" runat="server" Text="View E-mail Examples" ForeColor="Silver" OnClick="DownloadTemplates" style="position:relative; left:12px;"
                OnClientClick="return confirm('This will download a Word document with all mail template examples.\n\nAre you sure?')"/><br /><br /></td>
            </tr>
            <tr>
                <td colspan="4">
                    <asp:Label runat="server" Font-Bold="true" ForeColor="Chocolate" Text="Outstanding Value&nbsp;" />
                    <asp:TextBox ID="tb_outstanding_value" runat="server" Width="150"/>
                    <asp:Label runat="server" Font-Bold="true" ForeColor="Chocolate" Font-Size="7" Text="e.g. $123.45 USD" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Font-Bold="true" ForeColor="Red" Text="Mail Name" /></td>
                <td><asp:Label runat="server" Font-Bold="true" ForeColor="Red" Text="Sent" /></td>
                <td colspan="2"><asp:Label runat="server" Font-Bold="true" ForeColor="Red" Text="Last Sent" /></td>
            </tr>
            <tr>
                <td>Pre-Mag Mail&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_pre_mag_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_pre_mag_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_pre_mag_mail_sent" runat="server" Text="Send" OnClick="SendMail" Width="80"/></td>
            </tr>
            <tr>
                <td>Post-Mag Mail&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_post_mag_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_post_mag_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_post_mag_mail_sent" runat="server" Text="Send" OnClick="SendMail" Width="80"/></td>
            </tr>
            <tr>
                <td>CFO Mail <i>(requires value)</i>&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_cfo_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_cfo_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_cfo_mail_sent" runat="server" Text="Send" OnClientClick="return CheckOutstanding()" OnClick="SendMail" Width="80"/></td>
            </tr>
            <tr>
                <td>Feature Company Notification Mail&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_feature_co_notif_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_feature_co_notif_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_feature_co_notif_mail_sent" runat="server" Text="Send" OnClick="SendMail" Width="80"/></td>
            </tr>
            <tr>
                <td>Final Notice Mail <i>(requires value)</i>&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_final_notice_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_final_notice_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_final_notice_mail_sent" runat="server" Text="Send" OnClientClick="return CheckOutstanding()" OnClick="SendMail" Width="80"/></td>
            </tr>
            <tr>
                <td>Debt Recovery Mail <i>(requires value)</i>&nbsp;</td>
                <td align="center"><asp:CheckBox ID="cb_debt_recovery_mail_sent" runat="server" Enabled="false"/></td>
                <td><asp:Label ID="lbl_debt_recovery_mail_sent_when" runat="server" Text="Never"/></td>
                <td><asp:Button ID="btn_debt_recovery_mail_sent" runat="server" Text="Send" OnClientClick="return CheckOutstanding()" OnClick="SendMail" Width="80"/></td>
            </tr>
        </table>
    </div>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_issue_name" runat="server"/>
    <asp:HiddenField ID="hf_issue_date" runat="server"/>
    <asp:HiddenField ID="hf_advertiser" runat="server"/>
    <asp:HiddenField ID="hf_feature" runat="server"/>
    <asp:HiddenField ID="hf_magazine" runat="server"/>
    <asp:HiddenField ID="hf_invoice" runat="server"/>
    <asp:HiddenField ID="hf_invoice_date" runat="server"/>
    <asp:HiddenField ID="hf_cc_list" runat="server"/>
    <asp:HiddenField ID="hf_wdm_finance_ctc_email" runat="server"/>
    <asp:HiddenField ID="hf_wdm_finance_ctc_phone" runat="server"/>
    <asp:HiddenField ID="hf_finance_ctc_name" runat="server"/>
    <asp:HiddenField ID="hf_finance_ctc_email" runat="server"/>
    
    <script type="text/javascript">
        function CheckOutstanding() {
            var osv = grab("<%= tb_outstanding_value.ClientID %>").value.trim();
            if (osv == '') {
                alert('You must enter an Outstanding Value to send this mail.');
                return false;
            }
            else
                return true;
        }
    </script>
</asp:Content>