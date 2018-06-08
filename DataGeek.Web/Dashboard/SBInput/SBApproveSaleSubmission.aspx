<%--
Author   : Joe Pickering, 19/09/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Approve Sale Submission" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SBApproveSaleSubmission.aspx.cs" Inherits="FNApproveSaleSubmission" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   

    <div ID="div_page" runat="server" class="normal_page">   
        <hr/>
        
        <table border="1" width="99%" cellpadding="1" cellspacing="0" style="padding:15px;"> 
            <tr id="tr_approve" runat="server" visible="true">
                <td align="center">
                    <asp:Label ID="lbl_requested_by" runat="server" ForeColor="DarkOrange"/>
                
                    <%--Submission Form--%>
                    <table style="background:white;"><tr><td><asp:Label ID="lbl_request_form" runat="server"/></td></tr></table>
                
                    <p/>
                    <table>
                        <tr>
                            <td>
                                <%--Approve Table--%>
                                <table style="background:white;">
                                    <tr><td><asp:Button ID="btn_approve" runat="server" Text="Approve Submission" OnClick="ApproveSaleSubmission"/></td></tr>
                                </table>
                            </td>
                            <td>
                                <%--Decline Table--%>
                                <table style="background:white;">
                                    <tr>
                                        <td>
                                            <asp:TextBox ID="tb_decline_notes" runat="server" TextMode="MultiLine" Height="50" Width="200"/>
                                            <asp:Button ID="btn_decline" runat="server" Text="Decline Submission" OnClick="DeclineSaleSubmission"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <p/>
                </td>
            </tr>
            <tr id="tr_return" runat="server" visible="false">
                <td align="center">
                    <p/>
                        <asp:Label runat="server" Text="Click" ForeColor="White" Font-Size="12"/>
                        <asp:HyperLink runat="server" Text="here" ForeColor="Chocolate" NavigateUrl="~/Default.aspx" Font-Size="12"/>
                        <asp:Label runat="server" Text="to go back to the main page." ForeColor="White" Font-Size="12"/>
                    <p/>
                </td>
            </tr>
        </table>

        <hr/>
    </div>
    
    <asp:HiddenField ID="hf_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_sb_id" runat="server"/>
    <asp:HiddenField ID="hf_sb_end_date" runat="server"/>
    <asp:HiddenField ID="hf_total_revenue" runat="server"/>
    <asp:HiddenField ID="hf_total_adverts" runat="server"/>
</asp:Content>