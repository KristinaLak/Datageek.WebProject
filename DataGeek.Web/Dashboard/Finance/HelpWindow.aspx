<%--
Author   : Joe Pickering, 07/12/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" style="background-image:url('/Images/Backgrounds/SpiderMapWhiteWeb.png');">
    <table style="font-family:Verdana; font-size:8pt;" width="300">
        <%--KEY FEATURES--%>
        <tr>
            <td align="left">
                <h5>Finance Sales Information</h5>
                <hr/>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td><h5>Key</h5></td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                <hr />
                <table style="font-size:7pt;">
                    <tr><td colspan="2"><b>Sales</b></td></tr>
                    <tr><td><img src="/Images/Icons/gridView_Edit.png" /></td><td>&nbsp;Edit sale</td></tr>
                    <tr><td><img src="/Images/Icons/gridView_ChangeIssue.png" height="16" width="16"/></td><td>&nbsp;Move sale to a different tab</td></tr>
                    <tr><td><img src="/Images/Icons/dashboard_Colours.png" height="16" width="16"/></td><td>&nbsp;Set sale colour</td></tr>
                    <tr><td><asp:CheckBox runat="server" Text="BP" style="position:relative; left:-3px;"/></td><td>&nbsp;Before Publication</td></tr>
                    <tr><td>DN</td><td>&nbsp;Artwork Notes</td></tr>
                    <tr><td colspan="2">
                        <table border="0" style="margin-left:15px; font-size:7pt;">
                            <tr><td width="15" bgcolor="red">&nbsp;</td><td>Waiting for Copy</td></tr>
                            <tr><td width="15" bgcolor="DodgerBlue">&nbsp;</td><td>Copy In</td></tr>
                            <tr><td width="15" bgcolor="orange">&nbsp;</td><td>Proof Out</td></tr>
                            <tr><td width="15" bgcolor="lime">&nbsp;</td><td>Approved</td></tr>
                        </table>
                    </td></tr>
                    <tr><td>FN</td><td>&nbsp;Finance Notes</td></tr>
                    <tr><td colspan="2">
                        <table border="0" style="margin-left:15px; font-size:7pt;">
                            <tr><td width="15" bgcolor="SandyBrown">&nbsp;</td><td>Has Notes</td></tr>
                        </table>
                    </td></tr>
                    <tr><td width="15" bgcolor="SlateBlue"></td><td>&nbsp;Bank 1 (Proof of Payment)</td></tr>
                    <tr><td width="15" bgcolor="YellowGreen"></td><td>&nbsp;Bank 2 (Proof of Payment)</td></tr>
                </table>
                <br />
                <table style="font-size:7pt;">
                    <tr><td colspan="2"><b>Liabilities</b></td></tr>
                    <tr><td><img src="/Images/Icons/gridView_Edit.png" /></td><td>&nbsp;Edit liability</td></tr>
                    <tr><td><img src="/Images/Icons/dashboard_Colours.png" height="16" width="16"/></td><td>&nbsp;Set liability colour</td></tr>
                    <tr><td><img src="/Images/Icons/gridView_Delete.png" /></td><td>&nbsp;Permanently delete liability</td></tr>
                    <tr><td><asp:CheckBox runat="server" Text="RA" style="position:relative; left:-3px;"/></td><td>&nbsp;Request Authorisation - sends an authorisation request to relevant overseer.</td></tr>
                    <tr><td><asp:CheckBox runat="server" Text="CQ" style="position:relative; left:-3px;"/></td><td>&nbsp;Convert to Cheque</td></tr>
                    <tr><td><asp:CheckBox runat="server" Text="DD" style="position:relative; left:-3px;"/></td><td>&nbsp;Convert to Direct Debit</td></tr>
                    <tr><td><img src="/Images/Icons/finance_Recurring.png" /></td><td>&nbsp;Set liability recurring</td></tr>
                    <tr><td colspan="2">
                        <table border="0" style="margin-left:15px; font-size:7pt;">
                            <tr><td width="15" bgcolor="Chartreuse">&nbsp;</td><td>Monthly</td></tr>
                            <tr><td width="15" bgcolor="DarkCyan">&nbsp;</td><td>Quarterly</td></tr>
                            <tr><td width="15" bgcolor="Purple">&nbsp;</td><td>Six Monthly</td></tr>
                            <tr><td width="15" bgcolor="Coral">&nbsp;</td><td>Yearly</td></tr>
                        </table>
                    </td></tr>
                </table>
                <hr />
                <br/><br/>
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                <h5>Enquiries</h5>
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                Please send any queries/error reports to joe.pickering@bizclikmedia.com;
                <br /><br />
            </td>
        </tr>
        <tr><td><br/><hr/><br />Last Updated: 07/12/11</td></tr>
    </table>
</asp:Content>