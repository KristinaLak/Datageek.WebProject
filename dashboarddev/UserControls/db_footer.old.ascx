<%@ Control Language="C#" AutoEventWireup="true" CodeFile="db_footer - Copy.ascx.cs" Inherits="Footer" %>

<div id="div_footer" runat="server" class="footer">
    <table style="margin-left:auto; margin-right:auto; width:540px; position:relative; top:13px;">
        <tr>
            <td> © <asp:Label ID="lbl_year" runat="server"/> BizClik Media</td>
            <td align="center"><asp:Image runat="server" ImageUrl="~/Images/Icons/separator.png" Height="17" Width="3" style="position:relative; top:1px; left:3px;"/></td>
            <td><a href="http://www.bizclikmedia.com" target="_blank">www.bizclikmedia.com</a></td>
            <td align="center"><asp:Image runat="server" ImageUrl="~/Images/Icons/separator.png" Height="17" Width="3" style="position:relative; top:1px; left:3px;"/></td>
            <td><a href="http://www.smartsocialteam.com" target="_blank">www.smartsocialteam.com</a></td>
        </tr>
    </table>
    <table width="100%" style="font-size:6.4pt; font-family:Verdana; color:gray; position:relative; top:4px;">
        <tr><td valign="bottom" align="right">
            <a style="color:gray;" href="mailto:joe.pickering@bizclikmedia.com; sam.pickering@bizclikmedia.com;?subject=[Dashboard Issue]&body=Detail any technical problems you experience with the site here. Please specify the page(s) in which the problem is occuring.">report a problem</a>
            •
            <a style="color:gray;" href="mailto:joe.pickering@bizclikmedia.com;?subject=[Dashboard Feedback]&body=Give your feedback">give feedback</a>
        </td></tr>
    </table>
</div>