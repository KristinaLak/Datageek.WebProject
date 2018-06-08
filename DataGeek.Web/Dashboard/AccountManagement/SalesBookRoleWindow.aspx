<%--
Author   : Joe Pickering, 24/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk 
--%>

<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <table style="font-family:Verdana; font-size:8pt;">
        <tr>
            <td>
                <b>Standard</b><br />
                &nbsp;&nbsp;&nbsp;&nbsp;Standard Sales Book view.<br /><br />
                <b>Office Admin</b><br />
                &nbsp;&nbsp;&nbsp;&nbsp;A role designed for viewing/updating advertiser list information. The design status of sales cannot be set.<br /><br />
                <b>Design</b><br />
                &nbsp;&nbsp;&nbsp;&nbsp;A role designed for viewing/updating advertiser list information and setting the design status of sales.<br />
                &nbsp;&nbsp;&nbsp;&nbsp;This role will override the Sales Book Delete privelidge - a user cannot delete/cancel sales if in this role unless these privileges are explicitly set by an admin.
            </td>
        </tr>
    </table>
</asp:Content>

