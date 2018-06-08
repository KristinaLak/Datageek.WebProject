<%--
Author   : Joe Pickering, 18/10/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm_win.master" CodeFile="SurveyFeedbackViewer.aspx.cs" Inherits="SurveyFeedbackViewer" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<body background="/images/backgrounds/background.png"/>

    <table border="0" width="99%" style="font-family:Verdana; font-size:8pt; padding:15px;">
        <tr>
            <td colspan="2">
                <asp:Label ID="lbl_view_stats" runat="server" ForeColor="DarkOrange"/>
                <asp:GridView ID="gv_feedback" runat="server" AutoGenerateColumns="false" ForeColor="White"
                Border="2" Width="1000" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" 
                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound" style="margin-top:5px;">
                    <Columns>
                        <asp:BoundField HeaderText="Company Name" DataField="CompanyName" ItemStyle-Width="320"/>
                        <asp:BoundField HeaderText="Customer Name" DataField="ContactName"/>
                        <asp:BoundField HeaderText="View" DataField="SurveyFeedbackID"/>
                        <asp:BoundField HeaderText="Date/Time Submitted" DataField="DateAdded"/>
                        <asp:BoundField HeaderText="Language" DataField="Language"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
        
</asp:Content>


