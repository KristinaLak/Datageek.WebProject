<%--
Author   : Joe Pickering, 10/05/12
For      : Black Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <table height="130" style="font-family:Verdana; font-size:8pt;">
        <tr>
            <td>
                <asp:Button runat="server" Text="E-mail All" OnClientClick="GetRadWindow().Close('email');"/>
            </td>
            <td>or</td>
            <td align="right">
                <asp:Button runat="server" Text="Print All" OnClientClick="GetRadWindow().Close('print');"/>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <br />
                Selecting <b>E-mail All</b> will send each CCA their commission form via e-mail for the selected month. Note, this may take a minute or two.
            </td>
        </tr>
        <tr>
            <td colspan="3">
                Selecting <b>Print All</b> will open a printer friendly version of all the commission forms for the selected month.
            </td>
        </tr>
    </table>
</asp:Content>