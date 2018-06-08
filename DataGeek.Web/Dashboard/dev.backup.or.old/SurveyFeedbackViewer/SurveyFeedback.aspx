<%--
Author   : Joe Pickering, 19/06/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Survey Feedback" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SurveyFeedback.aspx.cs" Inherits="SurveyFeedback" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="wide_page">   
        <hr />

        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="Survey" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Feedback" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td align="right"><asp:Button ID="btn_export" runat="server" Text="Export to Excel" OnClick="Export" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:GridView ID="gv_feedback" runat="server" AutoGenerateColumns="false"
                    Border="2" Width="1240" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2"
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="fb_id" />
                            <asp:BoundField HeaderText="Company" DataField="company" ItemStyle-Width="175"/>
                            <asp:BoundField HeaderText="Name" DataField="name"/>
                            <asp:BoundField HeaderText="Title" DataField="title"/>
                            <asp:BoundField HeaderText="E-mail" DataField="email"/>
                            <asp:BoundField HeaderText="Sector" DataField="sector"/>
                            <asp:BoundField HeaderText="Channel Mag" DataField="channel_mag"/>
                            <asp:BoundField HeaderText="Publication" DataField="publication"/>
                            <asp:BoundField HeaderText="Rating" DataField="experience_rating"/>
                            <asp:BoundField HeaderText="Recommend" DataField="recommend"/>
                            <asp:BoundField HeaderText="Subscriptions" DataField="magazines"/>
                            <asp:BoundField HeaderText="Testimonial" DataField="testimonial" ItemStyle-Width="20"/>
                            <asp:BoundField HeaderText="Comments" DataField="comments" ItemStyle-Width="20"/>
                            <asp:BoundField HeaderText="Submitted" DataField="datetime_submitted"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>


