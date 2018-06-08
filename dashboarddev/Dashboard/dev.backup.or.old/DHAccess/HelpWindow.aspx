<%--
Author   : Joe Pickering, 23/10/2009
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk 
--%>

<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <table style="font-family:Verdana; font-size:8pt;" width="90%">
        <%--KEY FEATURES--%>
        <tr>
            <td align="left">
                <h5>DataHub Access Key Features</h5>
                <hr />
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                This section outlines the key features found on the DataHub Access page.       
                <p></p>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Searching</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                Searching can be performed for any company in the DataHub database using the 'Search Company' text box, the 'Territory' and 'Demographic'
                dropdowns and the 'Search' button. 
                Searching will return a list of companies within the selected territory and demographic criteria range whose name contains the search 
                term you entered. E.g., the search term 'farming' would return results such as 'Farming Solutions' and 'Farmington Foods Inc.'.
                <br /><br />
                Once the search has been carried out, a dropdown list of company names will appear below the search area listing company names
                and their bracketed zip (or postal) code. Companies in this list will have a text colour that ranges between bright red and black;
                the more black the item, the more recently the data for that company was updated.
                <br /><br />
                Clicking on a company in the list will load and display the company's data along with the first contact for that company.
                It will also populate the contacts dropdown (to the right of the companies dropdown) with all of the associated contacts for that company. 
                Similarly, selecting a contact from the contacts dropdown will load the data for that contact.
                <br /><br />      
                NOTE: The company name and zip together form a unique identifier for a company - the combination of name and zip must always 
                be unique for each company. 
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Adding</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                Adding a company can be performed using the 'new' hyperlink at the top of the page in the 'create a new company' dialogue below
                the search pane. Clicking 'new' will load a New Company form which can be closed at any time using the 'Close' button at the top-right
                of the page whilst the New Company form is opened.
                <br /><br />
                The input fields in this form are mostly self explanatory, but some fields are <i>required</i>. Adding the company to the database 
                can then be achieved by clicking the 'Add Company' button at the bottom-right of the page. 
                The 'Cancel' button next to this will cancel all of your changes and you will be navigated back to the search dialogue.
                Upon your add attempt, you may receive a 'Required!' message next to incomplete fields - these must be completed to meet the required criteria to add a new company. 
                <br /><br />
                Adding a contact to a company can be performed by first searching and loading up the company that you wish to add a contact to, then scrolling
                to the Contact section and clicking the 'New' button. The Contact section is at the bottom of the page once a company is loaded. Click Show/Hide 
                on the Contact section header to restore it if it's currently collapsed. Fill in the new contact data and click 'Add' to confirm the addition or 'Cancel' to go back to
                the company/contact view.
                <br /><br />
                NOTE: You must always add at least one contact to a company. Also, be sure to use the 'Refresh' button at the top-right of the page after you have
                added a new contact to a company to keep the list up to date.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Editing</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                You can perform company and contact edits using the respective 'Edit' button next to the loaded company/contact. 
                Clicking 'Edit' will enable the information text boxes for editing and will also show 'Cancel' and 'Update' options.
                <br /><br />  
                Clicking on the cancel button will take you back to the normal company/contact view, and clicking update will make 
                the modifications to the selected company/contact.
                <br /><br />      
                NOTE: While changes made to a company during an edit will be reflected instantly, edits to contacts may not be. 
                After a contact edit, be sure to use the 'Refresh' button next to the contacts list - this will get the up-to-date list of contacts
                for the selected company.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Deleting</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                You can perform company and contact deletes using the respective 'Delete' button next to the loaded company/contact. 
                Clicking 'Delete' will bring up a confirmation prompt, simply click 'Ok' to confirm the delete, or 'Cancel' to abort the deletion process.
                <br /><br />      
                NOTE: Though it is valid to completely delete a company and all of its associated contacts, it is not possible to delete a contact from a company when the
                contact you are trying to delete is the only contact in that company.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Company Merging</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                Merging companies allows you to remove duplicate company data in the database. To begin a merge, click the 'Merge' button on a loaded company.
                Notes will appear providing you with a guideline of how to merge the currently opened company with a new selected company - it is important to read 
                these notes carefully.
                <br /><br />
                To perform the merge, find a company in the dropdown at the bottom of the company section. The company you select from the dropdown will be <b>deleted</b> 
                from the database and all of the associated contacts will be moved into the company that you currently have opened. The dropdown of companies to merge with
                is populated with related companies by name. Once you have selected a company, hit the 'Merge' button next to the merging company dropdown list. Cancel
                the entire process by hitting the 'Cancel' button next to the merging company dropdown list.
                <br /><br />
                NOTE: Merging can take a minute or two depending on current database load.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Logs</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                At the far top-right of the page there is a 'Logs' hyperlink. Clicking this will navigate you to the DataHub Access logs page.
                This page allows you to see various breakdowns of your DataHub Access activities and performance. You can optionally view your 
                performance over a selected period using the two date boxes. If you select two identical dates, (e.g. 20/05/10) or select 'Today', it will
                show your activities for just that day. View your logs by selecting your criteria and hitting 'Get Logs'.
                <br /><br />
                To navigate back to the search page, click the 'Back to Search' hyperlink at the far top-right of the logs page
                <br /><br />
                NOTE: DataHub Access Admins can view the logs for all users, either individually or simultaneously grouped.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td>
                <h5>Other</h5>
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                <h6>Access Keys</h6>
                You can additionally use access key combinations to perform actions on this page for ease of use. 
                The key combinations are as follows:
                <br /><br />
                ALT+s = Search
                <br />
                ALT+x = Close All / Close New Company
                <br />
                ALT+r = Refresh Contact List
                <br />
                ALT+u = Update Company / Update Contact / Merge Company
                <br />
                ALT+c = Cancel Company Edit, Cancel Company Merge, Cancel New Company (or Done),  Cancel Contact Edit, Cancel New Contact
                <br />
                ALT+a = Add New Company / Add New Contact
                <br /><br />
                NOTE: In FireFox the key combination is Shift+Alt+[key].
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                <h6>Show/Hide</h6>
                The company or contact sections of the DataHub Access page can be shown or hidden (collapsed or restored) for tidiness. Simply click the respective
                'Show/Hide' hyperlink to toggle showing or hiding of that section.
                <br /><br />
            </td>
        </tr>
        <tr style="position:relative; left:16px;">
            <td style="position:relative; left:16px;">
                <h6>Close All</h6>
                At any time you can click the 'Close All' button at the top of the page near the search pane. This will close/cancel all searches/actions and redirect 
                you to the main search dialogue.
                <p></p>
            </td>
        </tr>
        <tr>
            <td align="left">
                <h5>DataHub Access Misc Information</h5>
                <hr />
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                <h5>Access</h5>
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                The DataHub Access page can be accessed from <a href="http://dashboard.wdmgroup.com/">http://dashboard.wdmgroup.com/</a> under Tools > DataHub Access.
                A valid Dashboard login is required to visit this page.
                <br /><br />
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                <h5>Enquiries</h5>
            </td>
        </tr>
        <tr>
            <td style="position:relative; left:16px;">
                Please send any queries/error reports to j.pickering@whitedm.com.
                <br /><br />
            </td>
        </tr>
        <tr><td><br/><hr/><br />Last Updated: 11/08/10</td></tr>
    </table>
</asp:Content>
