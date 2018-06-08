<%--
Author   : Joe Pickering, 16/05/13
For      : White Digital Media, ExecDigital - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Approve Red Line" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="FNApproveRedLine.aspx.cs" Inherits="FNApproveRedLine" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   

    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table border="1" width="99%" cellpadding="1" cellspacing="0" style="margin-left:auto; margin-right:auto; padding:15px;"> 
            <tr id="tr_approve" runat="server" visible="true">
                <td align="center">
                    <asp:Label ID="lbl_requested_for" runat="server" ForeColor="Bisque" />
                    <p/>
                    <asp:Label ID="lbl_requested_by" runat="server" ForeColor="DarkOrange" />
                
                    <table border="1" width="55%" style="color:White;">
                        <tr><td width="30%">Advertiser:</td><td><asp:Label ID="lbl_advertiser" runat="server" /></td></tr>
                        <tr><td>Feature:</td><td><asp:Label ID="lbl_feature" runat="server" /></td></tr>
                        <tr><td>Region:</td><td><asp:Label ID="lbl_region" runat="server"/></td></tr>
                        <tr><td>Date:</td><td><asp:Label ID="lbl_date" runat="server" /></td></tr>
                        <tr><td>Project Director:</td><td><asp:Label ID="lbl_rep" runat="server" /></td></tr>
                        <tr><td>Research Director:</td><td><asp:Label ID="lbl_list_gen" runat="server" /></td></tr>
                        <tr><td>Publication Month:</td><td><asp:Label ID="lbl_publication_month" runat="server" /></td></tr>
                        <tr><td>Invoice Number:</td><td><asp:Label ID="lbl_invoice" runat="server" /></td></tr>
                        <tr><td>Invoice Amount:</td><td><asp:Label ID="lbl_outstanding" runat="server" /></td></tr>
                        <tr><td>Date/Time Requested:</td><td><asp:Label ID="lbl_time_requested" runat="server" /></td></tr>
                        <tr><td>Destination Book:</td><td><asp:Label ID="lbl_destination_book" runat="server" /></td></tr>
                        <tr><td>Requested Red-Line Value:</td><td><asp:Label ID="lbl_red_line_value" runat="server" /></td></tr>
                        <tr><td>Reason for Request:</td><td><asp:Label ID="lbl_reason" runat="server" /></td></tr>
                    </table>
                
                    <p/>
                    <asp:Label ID="lbl_request_info" runat="server" ForeColor="DarkOrange" />
                    
                    <p/>
                    <asp:CheckBox ID="cb_accept" runat="server" Text="I confirm" ForeColor="White" />
                    <asp:Button ID="btn_appove" runat="server" Text="Approve" OnClick="ApproveRedLine" />
                    <p/>
                </td>
            </tr>
            <tr id="tr_return" runat="server" visible="false">
                <td align="center">
                    <p/>
                        <asp:Label runat="server" Text="Click" ForeColor="White" Font-Size="12" />
                        <asp:HyperLink runat="server" Text="here" ForeColor="Chocolate" NavigateUrl="~/Default.aspx" Font-Size="12"/>
                        <asp:Label runat="server" Text="to go back to the main page." ForeColor="White" Font-Size="12"/>
                    <p/>
                </td>
            </tr>
        </table>

        <hr />
    </div>
    
    <asp:HiddenField ID="hf_rl_rq_id" runat="server" />
    <asp:HiddenField ID="hf_rl_dest_book_id" runat="server" />
    <asp:HiddenField ID="hf_sale_id" runat="server" />
    <asp:HiddenField ID="hf_red_line_value" runat="server" />
    <asp:HiddenField ID="hf_requester" runat="server" />
    <asp:HiddenField ID="hf_approval_type" runat="server" />
    <asp:HiddenField ID="hf_md_email" runat="server" />
    <asp:HiddenField ID="hf_reason" runat="server" />
</asp:Content>