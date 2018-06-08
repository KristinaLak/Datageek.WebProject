// Author   : Joe Pickering, 23/10/2009
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;

public partial class ListDistAssign : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if(!IsPostBack)
            cb_parachute.Checked = Request.Url.ToString().Contains("True");
    }

    protected void AssignList(object sender, EventArgs e)
    {
        int test_int = -1;

        if (tb_value_predicted.Text.Trim() == String.Empty || !Int32.TryParse(tb_value_predicted.Text.Trim(), out test_int))
            Util.PageMessage(this, "Please enter a valid Value Predicted.");
        else if (tb_original_prediction.Text.Trim() == String.Empty || !Int32.TryParse(tb_original_prediction.Text.Trim(), out test_int))
            Util.PageMessage(this, "Please enter a valid Original Prediction.");
        else
        {
            if (Request.QueryString["rep"] != null && Request.QueryString["rep"].ToString() != String.Empty && 
                Request.QueryString["rep"].ToString() != "&nbsp;" && Request.QueryString["l_id"].ToString() != String.Empty)
            {
                String rep_working = Request.QueryString["rep"];
                String list_id = Request.QueryString["l_id"];
                String company = String.Empty;
                String office = String.Empty;
                String issue_name = String.Empty;
                String qry = "SELECT CompanyName, Office, IssueName " +
                    "FROM db_listdistributionlist ld, db_listdistributionhead ldh " +
                    "WHERE ld.ListIssueID = ldh.ListIssueID " +
                    "AND ListID=@list_id";
                DataTable dt_list_info = SQL.SelectDataTable(qry, "@list_id", list_id);
                if (dt_list_info.Rows.Count > 0)
                {
                    company = dt_list_info.Rows[0]["CompanyName"].ToString();
                    office = dt_list_info.Rows[0]["Office"].ToString();
                    issue_name = dt_list_info.Rows[0]["IssueName"].ToString();
                }
                String log_msg = String.Empty;
                String html_encode_log_msg = String.Empty;

                // Get list of CCA names if mulitple names.
                if (rep_working.Contains("/"))
                {
                    String all_reps = rep_working;
                    ArrayList reps = new ArrayList();
                    for (int i = 0; i < rep_working.Length; i++)
                    {
                        if (rep_working[i] == '/')
                        {
                            reps.Add(rep_working.Substring(0, i));
                            rep_working = rep_working.Replace(rep_working.Substring(0, i + 1), String.Empty);
                            i = 0;
                            if (!rep_working.Contains("/") && rep_working.Length > 0)
                            {
                                reps.Add(rep_working);
                                break;
                            }
                        }
                    }

                    int isUnique = 1;
                    for (int j = 0; j < reps.Count; j++)
                    {
                        if (j == 1) 
                            isUnique = 0;

                        String iqry = "INSERT INTO db_listdistributionlist " +
                        "(CompanyID, ListIssueID, OriginalValuePrediction, CurrentValuePrediction, SpaceSold, CompanyName, ListGeneratorFriendlyname, ListWorkedByFriendlyname, Suppliers, MaONames, "+
                        "Turnover, Employees, Industry, CribSheet, OptMail, ListAssignedToFriendlyname, WithAdmin, ListStatus, ListNotes, IsUnique, IsCancelled, Parachute, Synopsis) " +
                        "SELECT CompanyID, ListIssueID, @orig_prediction, @val_predicted, SpaceSold, CompanyName, ListGeneratorFriendlyname, ListWorkedByFriendlyname, Suppliers, MaONames, "+
                        "Turnover, Employees, Industry, @crib_sheet, @opt_mail, @rep, WithAdmin, ListStatus, ListNotes, @is_unique, IsCancelled, @parachute, Synopsis " +
                        "FROM db_listdistributionlist WHERE ListID=@list_id";
                        String[] pn = new String[] { "@orig_prediction", "@val_predicted", "@crib_sheet", "@opt_mail", "@rep", "@is_unique", "@parachute", "@list_id" };
                        Object[] pv = new Object[]{ tb_original_prediction.Text,
                                tb_value_predicted.Text,
                                cb_crib_sheet.Checked,
                                cb_opt_mail.Checked,
                                reps[j].ToString(),
                                isUnique,
                                cb_parachute.Checked,
                                list_id
                            };
                        SQL.Insert(iqry, pn, pv);

                        log_msg = "List '" + company + "' assigned to " + all_reps + " in " + office + " - " + issue_name + ".";
                    }

                    String et_uqry = "UPDATE db_editorialtracker SET ListID=NULL WHERE ListID=@list_id";
                    SQL.Update(et_uqry, "@list_id", list_id);

                    String dqry = "DELETE FROM db_listdistributionlist WHERE ListID=@list_id";
                    SQL.Delete(dqry, "@list_id", list_id);
                }
                else
                {
                    String uqry = "UPDATE db_listdistributionlist SET " +
                    "ListAssignedToFriendlyname=@ListCCA, OriginalValuePrediction=@orig_prediction, CurrentValuePrediction=@val_predicted, CribSheet=@crib_sheet, OptMail=@opt_mail, Parachute=@parachute, " +
                    "DateListAssigned=CURRENT_TIMESTAMP WHERE ListID=@list_id";
                    String[] pn = new String[] { "@ListCCA", "@orig_prediction", "@val_predicted", "@crib_sheet", "@opt_mail", "@parachute", "@list_id" };
                    Object[] pv = new Object[] { rep_working,
                            tb_original_prediction.Text,
                            tb_value_predicted.Text,
                            cb_crib_sheet.Checked,
                            cb_opt_mail.Checked,
                            cb_parachute.Checked,
                            list_id
                        };
                    SQL.Update(uqry, pn, pv);

                    log_msg = "List '" + company + "' assigned to " + rep_working + " in " + office + " - " + issue_name + ".";
                }

                Util.WriteLogWithDetails(log_msg, "listdistribution_log");
                Util.CloseRadWindow(this, log_msg, false);
            }
            else
                Util.PageMessage(this, "There was an error. Please close this window and try again.");
        }
    }
}