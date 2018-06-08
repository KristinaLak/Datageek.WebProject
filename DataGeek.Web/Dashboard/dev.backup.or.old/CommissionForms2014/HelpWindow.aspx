<%--
Author   : Joe Pickering, 23/10/2009 - re-written 19/09/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div style="color:white; font-family:Verdana; font-size:8pt; background-image:url('/Images/Backgrounds/Background.png'); overflow:visible;">
        <table width="90%">
            <%--KEY FEATURES--%>
            <tr>
                <td align="left">
                    <h5>Commission Forms Key Features</h5>
                    <hr />
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:16px;">
                    This section outlines the functionality of the available options for an individual's commission form.   
                    <p></p>
                </td>
            </tr>
            <tr>
                <td align="left" style="position:relative; left:16px;">
                    <h6>E-mail</h6>
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:32px;">
                    Selecting E-mail will send this form via e-mail to the respective CCA. 
                    You can e-mail all forms at once by clicking on a month header on the main grid and selecting E-mail All.
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:16px;">
                    <br />
                    <h6>Print</h6>
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:32px;">
                    Selecting Print will open a printer friendly version of this form.
                    You can see a printer-friendly version of all forms by clicking on a month header on the main grid and selecting Print All.
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:16px;">
                    <br />
                    <h6>Save Notes</h6>
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:32px;">
                    Selecting Save Notes will save any comments you have written against this form.
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:16px;">
                    <br />
                    <h6>Update</h6>
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:32px;">
                    Selecting Update will save any custom values you have entered in this form. The form will then be updated and will show the re-calculated commission.
                </td>
            </tr>
            <tr>
                <td align="left">
                    <br /><br />
                    <h5>Commission Forms Misc Information</h5>
                    <hr />
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:16px;">
                    <h6>Enquiries</h6>
                </td>
            </tr>
            <tr>
                <td style="position:relative; left:32px;">
                    Please send any queries/error reports to joe.pickering@bizclikmedia.com
                    <br /><br />
                </td>
            </tr>
            <tr><td><br/><hr/><br />Last Updated: 30/07/10</td></tr>
        </table>
    </div>
</asp:Content>
