using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class ContactNotesManager : System.Web.UI.UserControl
{
    public String ContactID
    {
        get
        {
            return hf_ctc_id.Value;
        }
        set
        {
            hf_ctc_id.Value = value;
        }
    }
    public String LeadID
    {
        get
        {
            return hf_lead_id.Value;
        }
        set
        {
            hf_lead_id.Value = value;
            if (ContactID == String.Empty)
                ContactID = LeadsUtil.GetContactIDFromLeadID(hf_lead_id.Value);
        }
    }
    public event EventHandler UpdateParent;
    public bool AllowDelete = true;
    public bool ShowNotesAndNextActionTitle = true;
    public bool InWindow = false;
    public bool IncludeCommonNotes = false;
    public int NotesBoxHeight = 50;
    public int NotesListHeight = 300;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
        }
    }

    public void Bind(String TargetContactID)
    {
        ContactID = TargetContactID;
        Bind();
    }
    public void Bind()
    {
        ConfigureControl();

        BindNextActionOptions();
        if (IncludeCommonNotes)
            BindCommonNotesOptions();
        BindNotesAndNextAction();
    }

    private void BindNotesAndNextAction()
    {
        String qry = String.Empty;

        if (ContactID != String.Empty)
        {
            qry = "SELECT NoteID, CONCAT(DATE_FORMAT(db_contact_note.DateAdded,'%b %d %Y %h:%i %p'),',') as 'date', note, fullname, isnextaction " +
                "FROM db_contact_note, db_userpreferences WHERE db_contact_note.AddedBy = db_userpreferences.userid "+
                "AND ContactID=@ctc_id ORDER BY db_contact_note.DateAdded DESC"; // LIMIT 5
            DataTable dt_notes = SQL.SelectDataTable(qry, "@ctc_id", ContactID);
            for (int i = 0; i < dt_notes.Rows.Count; i++)
                dt_notes.Rows[i]["note"] = Server.HtmlEncode(dt_notes.Rows[i]["note"].ToString()).Replace(Environment.NewLine, "<br/>");

            rep_notes.DataSource = dt_notes;
            rep_notes.DataBind();

            lbl_no_notes.Visible = dt_notes.Rows.Count == 0;
        }

        if (LeadID != String.Empty)
        {
            qry = "SELECT NextActionTypeID, NextActionDate FROM dbl_lead WHERE LeadID=@lead_id;";
            DataTable dt_next_action = SQL.SelectDataTable(qry, "@lead_id", LeadID);
            if (dt_next_action.Rows.Count > 0)
            {
                String next_action_id = dt_next_action.Rows[0]["NextActionTypeID"].ToString();
                String next_action_date = dt_next_action.Rows[0]["NextActionDate"].ToString();
                hf_nat.Value = next_action_id;
                hf_nad.Value = next_action_date;

                if (dd_next_action_type.FindItemByValue(next_action_id) != null)
                    dd_next_action_type.SelectedIndex = dd_next_action_type.FindItemByValue(next_action_id).Index;

                DateTime d = new DateTime();
                if (DateTime.TryParse(next_action_date, out d))
                    rdp_next_action.SelectedDate = d;
            }
        }
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
    
    public String GetLatestNote()
    {
        return GetLatestNote(ContactID);
    }
    public String GetLatestNote(String ContactID)
    {
        String qry = "SELECT note FROM db_contact_note, db_userpreferences WHERE db_contact_note.AddedBy = db_userpreferences.userid " +
        "AND ContactID=@ctc_id ORDER BY db_contact_note.DateAdded DESC LIMIT 1";
        return SQL.SelectString(qry, "note", "@ctc_id", ContactID);
    }
    public String[] GetNextAction()
    {
        return GetNextAction(LeadID);
    }
    public String[] GetNextAction(String LeadID)
    {
        String qry = "SELECT DATE_FORMAT(NextActionDate, '%d %b %H:%i') as 'nad', ActionType FROM dbl_lead, dbl_action_type WHERE dbl_lead.NextActionTypeID=dbl_action_type.ActionTypeID AND LeadID=@lead_id;";
        DataTable dt_app_details = SQL.SelectDataTable(qry, "@lead_id", LeadID);
        if(dt_app_details.Rows.Count > 0)
            return new String[] { dt_app_details.Rows[0]["nad"].ToString(), dt_app_details.Rows[0]["ActionType"].ToString() };
        
        return new String[] { String.Empty, String.Empty };
    }

    protected void DeleteNote(object sender, EventArgs e)
    {
        String note_id = ((Label)((ImageButton)sender).Parent.FindControl("lbl_note_id")).Text;

        // Set next action date blank when we delete an action note which is the latest action for this lead
        String qry = "SELECT ContactID, IsNextAction FROM db_contact_note WHERE NoteID=@NoteID AND IsNextAction=1";
        DataTable dt_app = SQL.SelectDataTable(qry, "@NoteID", note_id);
        bool RemovingNextAction = false;
        if (dt_app.Rows.Count > 0)
        {
            String ContactID = dt_app.Rows[0]["ContactID"].ToString();

            qry = "SELECT NoteID FROM db_contact_note WHERE IsNextAction=1 AND ContactID=@ContactID ORDER BY DateAdded DESC";
            String LatestNoteID = SQL.SelectString(qry, "NoteID", "@ContactID", ContactID);

            if (LatestNoteID == note_id)
            {
                String uqry = "UPDATE dbl_lead SET NextActionTypeID=0, NextActionDate=NULL WHERE LeadID=@LeadID";
                SQL.Update(uqry, "@LeadID", hf_lead_id.Value);
                RemovingNextAction = true;
            }
        }

        String dqry = "DELETE FROM db_contact_note WHERE NoteID=@NoteID";
        SQL.Delete(dqry, "@NoteID", note_id);

        // Set previous note as latestnoteid
        String l_uqry = "UPDATE dbl_lead " +
        "LEFT JOIN " +
        "( " +
        "    SELECT ContactID, MAX(NoteID) as NoteID " +
        "    FROM db_contact_note " +
        "    GROUP BY ContactID " +
        ") t on dbl_lead.ContactID = t.ContactID " +
        "SET dbl_lead.LatestNoteID = t.NoteID WHERE dbl_lead.ContactID=@ContactID";
        SQL.Update(l_uqry, "@ContactID", hf_ctc_id.Value);

        if (InWindow) // resize
            Util.ResizeRadWindow(this);

        String msg = "Deleted!";
        if (RemovingNextAction)
            msg = "Next Action Removed!";
        Util.PageMessageSuccess(this, msg, "top-right");

        BindNotesAndNextAction();

        UpdateParentPage(RemovingNextAction);
    }
    public void SaveChanges(object sender, EventArgs e)
    {
        // insert new notes
        String username = Util.GetUserName();
        String new_note = tb_add_note.Text.Trim();
        tb_add_note.Text = String.Empty;
        int is_next_action = 0;

        bool IsCommonNote = ((Button)sender).ID == "btn_add_common_note" && dd_common_notes.Items.Count > 0 && dd_common_notes.SelectedItem != null;

        String msg = String.Empty;
        String uqry;
        if (LeadID != String.Empty && !IsCommonNote)
        {
            // Determine whether an action is changing or is new, then add special note if new/changed
            DateTime nad = new DateTime();
            bool next_action_changing = false;
            if (rdp_next_action.SelectedDate != null && DateTime.TryParse(rdp_next_action.SelectedDate.ToString(), out nad)
                && dd_next_action_type.Items.Count > 0 && dd_next_action_type.SelectedItem != null)
            {
                if (dd_next_action_type.SelectedItem.Text != String.Empty)
                {
                    next_action_changing = (hf_nat.Value != dd_next_action_type.SelectedItem.Value || hf_nad.Value != nad.ToString());
                    if (next_action_changing)
                    {
                        is_next_action = 1;

                        if (dd_next_action_type.SelectedItem.Text == "Other" && new_note != String.Empty)
                            new_note += " (" + nad.ToString("d MMMM yy, h tt") + ")";
                        else if (new_note != String.Empty) // removed 06/01/16
                            new_note = new_note + " -- " + dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + "."; //Environment.NewLine + Environment.NewLine + 

                        if (new_note == String.Empty)
                            new_note = dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + ".";

                        msg = "Next Action Set!";
                    }
                }

               uqry  = "UPDATE dbl_lead SET NextActionTypeID=@nat, NextActionDate=@nad WHERE LeadID=@LeadID";
                SQL.Update(uqry,
                    new String[] { "@nat", "@nad", "@LeadID" },
                    new Object[] { dd_next_action_type.SelectedItem.Value, nad, LeadID });
            }

            uqry = "UPDATE dbl_lead SET DateUpdated=CURRENT_TIMESTAMP WHERE LeadID=@LeadID";
            SQL.Update(uqry, "@LeadID", LeadID);
        }

        if (IsCommonNote)
        {
            String br = String.Empty;
            if (new_note != String.Empty) br = Environment.NewLine + Environment.NewLine;
            new_note = dd_common_notes.SelectedItem.Text + br + new_note;
        }

        if (ContactID != String.Empty && new_note != String.Empty)
        {
            // Ensure we have the latest version of this contact (it may have been merged)
            Contact c = new Contact(ContactID);
            ContactID = c.ContactID;

            new_note = Util.ConvertStringToUTF8(new_note);

            String iqry = "INSERT INTO db_contact_note (ContactID, Note, AddedBy, IsNextAction) VALUES (@ContactID, @Note, @AddedBy, @IsNextAction);";
            long note_id = SQL.Insert(iqry,
                new String[] { "@ContactID", "@Note", "@AddedBy", "@IsNextAction" },
                new Object[] { hf_ctc_id.Value, new_note, Util.GetUserId(), is_next_action });

            uqry = "UPDATE dbl_lead SET LatestNoteID=@lnid WHERE ContactID=@ContactID";
            SQL.Update(uqry,
                new String[] { "@lnid", "@ContactID" },
                new Object[] { note_id, hf_ctc_id.Value });

            // Log
            LeadsUtil.AddLeadHistoryEntry(LeadID, "Adding note/next action: " + new_note);

            if (msg == String.Empty)
                msg = "Saved!";

            Util.PageMessageSuccess(this, msg, "top-right");

            if (InWindow) // resize
                Util.ResizeRadWindow(this);
        }

        BindNotesAndNextAction();

        UpdateParentPage();
    }
    public void ClearNextAction(object sender, EventArgs e)
    {
        String uqry = "UPDATE dbl_lead SET NextActionTypeID=0, NextActionDate=NULL WHERE LeadID=@LeadID";
        SQL.Update(uqry, "@LeadID", hf_lead_id.Value);
        rdp_next_action.SelectedDate = null;

        // Log
        LeadsUtil.AddLeadHistoryEntry(LeadID, "Next action cleared.");

        Util.PageMessageSuccess(this, "Next Action Cleared!", "top-right");

        if (InWindow) // resize
            Util.ResizeRadWindow(this);

        BindNotesAndNextAction();

        UpdateParentPage(true);
    }
    private void UpdateParentPage(bool RemovingNextAction = false)
    {
        var args = EventArgs.Empty;
        if (RemovingNextAction)
            args = null;

        if (UpdateParent != null)
            UpdateParent(this, args);
    }

    protected void rep_notes_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            bool is_next_action = ((Label)e.Item.FindControl("lbl_is_next_action")).Text == "1";
            if (is_next_action)
            {
                ((System.Web.UI.HtmlControls.HtmlGenericControl)e.Item.FindControl("li_note")).Style.Add("color", "#da9726");
                ((Label)e.Item.FindControl("lbl_note")).CssClass = "NextActionEntry";
            }

            e.Item.FindControl("imbtn_del_note").Visible = AllowDelete;
        }
    }

    private void ConfigureControl()
    {
        tb_add_note.Height = NotesBoxHeight;
        lbl_title.Visible = ShowNotesAndNextActionTitle;
        if(NotesListHeight != 300)
            div_notes.Attributes.Add("style", "max-height:" + NotesListHeight + "px;");

        if (IncludeCommonNotes)
            div_common_notes.Visible = true;
    }
}