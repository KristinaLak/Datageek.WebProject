// Author   : Joe Pickering, 08/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class ManageColumns : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            BindSelectableColumns();
            BindUserSelection();
        }
    }

    protected void BindSelectableColumns()
    {
        String user_id = Util.GetUserId();
        String qry = "SELECT COUNT(*) FROM dbl_user_columns WHERE UserID=@user_id";
        bool use_defaults = SQL.SelectDataTable(qry, "@user_id", user_id).Rows[0][0].ToString() == "0";

        // Bind Selectable Company Columns
        qry = "SELECT * FROM dbl_selectable_columns WHERE type='cpy'";
        DataTable dt_cpy = SQL.SelectDataTable(qry, null, null);
        for(int i=0; i<dt_cpy.Rows.Count; i++)
        {
            bool enabled = dt_cpy.Rows[i]["Compulsory"].ToString() == "0";
            RadTreeNode n = new RadTreeNode(Server.HtmlEncode(dt_cpy.Rows[i]["ColumnName"].ToString()), dt_cpy.Rows[i]["ColumnID"].ToString());
            if (!enabled)
            {
                n.Text += " (Compulsory)";
                n.Checkable = false;
                n.Checked = true;
            }
            if (use_defaults && dt_cpy.Rows[i]["DefaultColumn"].ToString() == "1")
                n.Checked = true;

            rtv_cpy.Nodes[0].Nodes.Add(n);
        }

        // Bind Selectable Contact Columns
        qry = qry.Replace("cpy", "ctc");
        DataTable dt_ctc = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_ctc.Rows.Count; i++)
        {
            bool enabled = dt_ctc.Rows[i]["Compulsory"].ToString() == "0";
            RadTreeNode n = new RadTreeNode(Server.HtmlEncode(dt_ctc.Rows[i]["ColumnName"].ToString()), dt_ctc.Rows[i]["ColumnID"].ToString());
            if (!enabled)
            {
                n.Text += " (Compulsory)";
                n.Checkable = false;
                n.Checked = true;
            }
            if (use_defaults && dt_ctc.Rows[i]["DefaultColumn"].ToString() == "1")
                n.Checked = true;
            rtv_ctc.Nodes[0].Nodes.Add(n);
        }

        // Bind Selectable Lead Columns
        qry = qry.Replace("ctc", "lead");
        DataTable dt_lead = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_lead.Rows.Count; i++)
        {
            bool enabled = dt_lead.Rows[i]["Compulsory"].ToString() == "0";
            RadTreeNode n = new RadTreeNode(Server.HtmlEncode(dt_lead.Rows[i]["ColumnName"].ToString()), dt_lead.Rows[i]["ColumnID"].ToString());
            if (!enabled)
            {
                n.Text += " (Compulsory)";
                n.Checkable = false;
                n.Checked = true;
            }
            if (use_defaults && dt_lead.Rows[i]["DefaultColumn"].ToString() == "1")
                n.Checked = true;
            rtv_lead.Nodes[0].Nodes.Add(n);
        }
    }
    protected void BindUserSelection()
    {
        String user_id = Util.GetUserId();
        String qry = "SELECT ColumnID FROM dbl_user_columns WHERE UserID=@user_id";
        DataTable dt_selected = SQL.SelectDataTable(qry, "@user_id", user_id);

        // Select user-picked company columns
        for (int i = 0; i < rtv_cpy.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_cpy.Nodes[0].Nodes[i];
            for (int j = 0; j < dt_selected.Rows.Count; j++)
            {
                if (n.Value == dt_selected.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }

        // Select user-picked contact columns
        for (int i = 0; i < rtv_ctc.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_ctc.Nodes[0].Nodes[i];
            for (int j = 0; j < dt_selected.Rows.Count; j++)
            {
                if (n.Value == dt_selected.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }

        // Select user-picked lead columns
        for (int i = 0; i < rtv_lead.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_lead.Nodes[0].Nodes[i];
            for (int j = 0; j < dt_selected.Rows.Count; j++)
            {
                if (n.Value == dt_selected.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }
    }

    protected void SaveColumnSelection(object sender, EventArgs e)
    {
        String user_id = Util.GetUserId();
        String dqry = "DELETE FROM dbl_user_columns WHERE UserID=@user_id";
        SQL.Delete(dqry, "@user_id", user_id);

        // Add selected company columns
        String iqry = "INSERT INTO dbl_user_columns (UserID, ColumnID) VALUES (@user_id, @column_id)";
        for (int i = 0; i < rtv_cpy.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_cpy.Nodes[0].Nodes[i];
            if (n.Checked)
            {
                String column_id = n.Value;
                SQL.Insert(iqry, new String[] { "@user_id", "@column_id" }, new Object[] { user_id, column_id });
            }
        }

        // Add selected contact columns
        for (int i = 0; i < rtv_ctc.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_ctc.Nodes[0].Nodes[i];
            if (n.Checked)
            {
                String column_id = n.Value;
                SQL.Insert(iqry, new String[] { "@user_id", "@column_id" }, new Object[] { user_id, column_id });
            }
        }

        // Add selected lead columns
        for (int i = 0; i < rtv_lead.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_lead.Nodes[0].Nodes[i];
            if (n.Checked)
            {
                String column_id = n.Value;
                SQL.Insert(iqry, new String[] { "@user_id", "@column_id" }, new Object[] { user_id, column_id });
            }
        }

        // Log
        LeadsUtil.AddLogEntry("Visible columns updated.");

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
    protected void SetDefault(object sender, EventArgs e)
    {
        String qry = "SELECT ColumnID FROM dbl_selectable_columns WHERE DefaultColumn=1";
        DataTable dt_defaults = SQL.SelectDataTable(qry, null, null);

        // Select default company columns
        for (int i = 0; i < rtv_cpy.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_cpy.Nodes[0].Nodes[i];
            n.Checked = false;
            for (int j = 0; j < dt_defaults.Rows.Count; j++)
            {
                if (n.Value == dt_defaults.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }

        // Select default contact columns
        for (int i = 0; i < rtv_ctc.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_ctc.Nodes[0].Nodes[i];
            n.Checked = false;
            for (int j = 0; j < dt_defaults.Rows.Count; j++)
            {
                if (n.Value == dt_defaults.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }

        // Select default lead columns
        for (int i = 0; i < rtv_lead.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode n = rtv_lead.Nodes[0].Nodes[i];
            n.Checked = false;
            for (int j = 0; j < dt_defaults.Rows.Count; j++)
            {
                if (n.Value == dt_defaults.Rows[j]["ColumnID"].ToString())
                {
                    n.Checked = true;
                    break;
                }
            }
        }
    }
}