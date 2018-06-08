// Author   : Joe Pickering, 20/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class MultiNote : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["lead_ids"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_ids"]))
            {
                hf_lead_ids.Value = Request.QueryString["lead_ids"];
                hf_lead_ids.Value = hf_lead_ids.Value.Replace(",", " ").Trim();

                String[] lead_ids = hf_lead_ids.Value.Split(' ');
                lbl_title.Text = "Add a note/next action to each of your " + lead_ids.Length + " selected <b>Leads</b>..";

                BindNextActionOptions();
                BindCommonNotesOptions();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void AddNoteToSelectedLeads(object sender, EventArgs e)
    {
        String[] lead_ids = hf_lead_ids.Value.Split(' ');
        ArrayList ctc_ids = new ArrayList();
        String user_id = Util.GetUserId();
        String iqry = "INSERT INTO db_contact_note (ContactID, Note, AddedBy, IsNextAction) VALUES (@ContactID, @Note, @AddedBy, @IsNextAction);";

        // Determine whether we should set a next action, if so, build notes
        DateTime nad = new DateTime();
        String new_note = tb_note.Text.Trim();
        int IsNextAction = 0;
        bool set_next_action = dd_next_action_type.Items.Count > 0 && dd_next_action_type.SelectedItem.Text != String.Empty &&
            rdp_next_action.SelectedDate != null && DateTime.TryParse(rdp_next_action.SelectedDate.ToString(), out nad);
        if (set_next_action)
        {
            IsNextAction = 1;
            if (dd_next_action_type.SelectedItem.Text == "Other" && new_note != String.Empty)
                new_note += " (" + nad.ToString("d MMMM yy, h tt") + ")";
            else if (new_note != String.Empty) // removed 06/01/16
                new_note = new_note + " -- " + dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + "."; //Environment.NewLine + Environment.NewLine + 

            if (new_note == String.Empty)
                new_note = dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + ".";
        }

        String uqry;
        foreach (String lead_id in lead_ids)
        {
            String qry = "SELECT ContactID FROM dbl_lead WHERE LeadID=@lead_id";
            String ctc_id = SQL.SelectString(qry, "ContactID", "@lead_id", lead_id);

            // Only add once for each same contact
            if (!ctc_ids.Contains(ctc_id))
            {
                ctc_ids.Add(ctc_id);

                long note_id = SQL.Insert(iqry,
                    new String[] { "@ContactID", "@Note", "@AddedBy", "@IsNextAction" },
                    new Object[] { ctc_id, new_note, user_id, IsNextAction });

                // Set latest note id for leads
                uqry = "UPDATE dbl_lead SET LatestNoteID=@lnid WHERE ContactID=@ContactID";
                SQL.Update(uqry,
                    new String[] { "@lnid", "@ContactID" },
                    new Object[] { note_id, ctc_id });
            }

            // Set next action for Lead (not contact)
            if (set_next_action)
            {
                uqry = "UPDATE dbl_lead SET NextActionTypeID=@nat, NextActionDate=@nad WHERE LeadID=@LeadID";
                SQL.Update(uqry,
                    new String[] { "@nat", "@nad", "@LeadID" },
                    new Object[] { dd_next_action_type.SelectedItem.Value, nad, lead_id });
            }

            uqry = "UPDATE dbl_lead SET DateUpdated=CURRENT_TIMESTAMP WHERE LeadID=@LeadID";
            SQL.Update(uqry, "@LeadID", lead_id);
        }

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }

    private void BindNextActionOptions()
    {
        String qry = "SELECT * FROM dbl_action_type ORDER BY ActionType";
        dd_next_action_type.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_next_action_type.DataTextField = "ActionType";
        dd_next_action_type.DataValueField = "ActionTypeID";
        dd_next_action_type.DataBind();
    }
    private void BindCommonNotesOptions()
    {
        String qry = "SELECT * FROM dbl_common_notes ORDER BY BindOrder";
        dd_common_notes.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_common_notes.DataTextField = "Note";
        dd_common_notes.DataValueField = "CommonNoteID";
        dd_common_notes.DataBind();
    }
}