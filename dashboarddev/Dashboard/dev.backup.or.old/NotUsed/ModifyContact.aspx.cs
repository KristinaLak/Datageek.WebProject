// Author   : Joe Pickering, 03/06/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ModifyContact : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ctc_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ctc_id"]))
            {
                hf_ctc_id.Value = Request.QueryString["ctc_id"];
                BindContact();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void UpdateContact(object sender, EventArgs e)
    {
        ContactManager.UpdateContacts(hf_cpy_id.Value);

        Util.CloseRadWindow(this, String.Empty, false);
    }

    private void BindContact()
    {
        String qry = "SELECT * FROM db_contact LEFT JOIN db_company ON db_contact.new_cpy_id = db_company.cpy_id " +
        "LEFT JOIN db_userpreferences ON db_contact.email_verification_user_id = db_userpreferences.userid WHERE ctc_id=@ctc_id";
        DataTable dt_contact = SQL.SelectDataTable(qry, "@ctc_id", hf_ctc_id.Value);
        if(dt_contact.Rows.Count > 0)
        {
            String ctc_id = dt_contact.Rows[0]["ctc_id"].ToString();
            String cpy_id = dt_contact.Rows[0]["new_cpy_id"].ToString();

            // Bind contacts
            ContactManager.BindContact(ctc_id);
            hf_cpy_id.Value = cpy_id;

            String name = (dt_contact.Rows[0]["title"] + " " + dt_contact.Rows[0]["first_name"] + " " + dt_contact.Rows[0]["last_name"]).Trim();
            lbl_title.Text = "Editing <b>" + Server.HtmlEncode(name) + "</b>..";
        }
        else
            Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
    }
}