<%--
Author   : Joe Pickering, 18/10/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm_win.master" CodeFile="SurveyFeedbackViewer.aspx.cs" Inherits="SurveyFeedbackViewer" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<body background="/Images/Backgrounds/Background.png"/>

    <table border="0" width="99%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
        <tr>
            <td colspan="2">
                <asp:Label ID="lbl_view_stats" runat="server" ForeColor="DarkOrange"/>
                <asp:GridView ID="gv_feedback" runat="server" AutoGenerateColumns="false" ForeColor="White"
                Border="2" Width="565" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" 
                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound">
                    <Columns>
                        <asp:BoundField HeaderText="Company Name" DataField="company" ItemStyle-Width="175"/>
                        <asp:BoundField HeaderText="Customer Name" DataField="name"/>
                        <asp:BoundField HeaderText="View" DataField="fb_id"/>
                        <asp:BoundField HeaderText="Date/Time Submitted" DataField="datetime_submitted"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
        
</asp:Content>


