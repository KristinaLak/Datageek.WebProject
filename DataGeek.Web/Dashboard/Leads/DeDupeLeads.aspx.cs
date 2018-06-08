// Author   : Joe Pickering, 06/11/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using Telerik.Web.UI;

public partial class DeDupeLeads : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LeadsUtil.BindProjects(dd_project, dd_buckets, LeadsUtil.GetProjectParentIDFromID(LeadsUtil.GetLastViewedProjectID()), null, false);
        }
    }

    protected void DeDuplicateLeads(object sender, EventArgs e)
    {
        if (dd_buckets.Items.Count > 0 && dd_buckets.SelectedItem != null)
        {
            String qry ="SELECT LeadID, ContactID, Active " +
            "FROM dbl_lead " +
            "WHERE ProjectID = @ClientListID " +
            "AND Active=1 AND ContactID IN ( " +
            "SELECT ContactID " +
            "FROM dbl_lead " +
            "WHERE ProjectID = @ClientListID " +
            "AND Active=1 " +
            "GROUP BY ContactID " +
            "HAVING COUNT(*) > 1 " +
            "ORDER BY COUNT(*) DESC) " +
            "ORDER BY ContactID, DateUpdated DESC";
            DataTable dt_dupes = SQL.SelectDataTable(qry, "@ClientListID", dd_buckets.SelectedItem.Value);
            div_preview.Visible = dt_dupes.Rows.Count > 0;

            ArrayList ContactIDs = new ArrayList();
            String uqry = "UPDATE dbl_lead SET Active=0 WHERE LeadID=@LeadID";
            for (int i = 0; i < dt_dupes.Rows.Count; i++)
            {
                String LeadID = dt_dupes.Rows[i]["LeadID"].ToString();
                String ContactID = dt_dupes.Rows[i]["ContactID"].ToString();

                if (!ContactIDs.Contains(ContactID))
                {
                    ContactIDs.Add(ContactID);
                    continue;
                }
                else
                    SQL.Update(uqry, "@LeadID", LeadID);
            }

            Util.PageMessageAlertify(this, "De-Duplication Done!", "Done");
            Util.ResizeRadWindow(this);
        }
        else
            Util.PageMessageAlertify(this, "You have no Projects!", "No Projects");
    }
    protected void PreviewDuplicateLeads(object sender, EventArgs e)
    {
        if (dd_buckets.Items.Count > 0 && dd_buckets.SelectedItem != null)
        {
            String qry = "SELECT FirstName, LastName, CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END as Email, Occurences FROM " +
            "(SELECT ContactID, COUNT(*) as 'Occurences' " +
            "FROM dbl_lead " +
            "WHERE ProjectID = @ClientListID " +
            "AND Active=1 " +
            "GROUP BY ContactID " +
            "HAVING COUNT(*) > 1 " +
            "ORDER BY COUNT(*) DESC) as t " +
            "LEFT JOIN db_contact c ON t.ContactID = c.ContactID";
            DataTable dt_dupes = SQL.SelectDataTable(qry, "@ClientListID", dd_buckets.SelectedItem.Value);
            div_preview.Visible = dt_dupes.Rows.Count > 0;
            if (dt_dupes.Rows.Count > 0)
            {
                rg_preview.DataSource = dt_dupes;
                rg_preview.DataBind();

                Util.PageMessageAlertify(this, dt_dupes.Rows.Count + " contacts were found to have at least one duplicate in this Client List.<br/><br/>Remove these duplicated by hitting the De-Duplicate Now button.", "Duplicates Found");
            }
            else
                Util.PageMessageAlertify(this, "Could not find any duplicates in the selected Client List!", "No Duplicates");

            btn_dedupe.Visible = dt_dupes.Rows.Count > 0;

            Util.ResizeRadWindow(this);
        }
        else
            Util.PageMessageAlertify(this, "You have no Projects!", "No Projects");
    }

    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_project, dd_buckets, null, true);
    }

    protected void rg_preview_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            //CheckBox cb_selected = (CheckBox)item["Selected"].FindControl("cb_selected");

            //String Email = item["Email"].Text;
            //if (!Util.IsValidEmail(Email))
            //{
            //    cb_selected.Checked = false;
            //    cb_selected.Enabled = false;
            //    item.ForeColor = System.Drawing.Color.Red;
            //}

            //// Truncate job title
            //if (item["JobTitle"].Text.Length > 45)
            //{
            //    item["JobTitle"].ToolTip = item["JobTitle"].Text;
            //    item["JobTitle"].Text = Util.TruncateText(item["JobTitle"].Text, 45);
            //}

            //if (item["EmailSent"].Text == "1")
            //{
            //    item.ForeColor = System.Drawing.Color.Green;
            //    cb_selected.Checked = false;
            //    cb_selected.Enabled = false;
            //}
        }
    }
    protected void rg_preview_PreRender(object sender, EventArgs e)
    {
        // Set width of expand column
        foreach (GridColumn column in rg_preview.MasterTableView.RenderColumns) 
        {
            if (column.ColumnGroupName.Contains("Thin"))
                column.HeaderStyle.Width = Unit.Pixel(30);
        }
    }
}