// Author   : Joe Pickering, 13/06/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class MMMagManager : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            BindMags();
    }

    protected void BindMags()
    {
        String qry = "SELECT MagazineID, ShortMagazineName FROM db_magazine WHERE MagazineType='BR' AND IsLive=1 ORDER BY ShortMagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_businessreview.DataSource = dt_mags;
        dd_businessreview.DataTextField = "ShortMagazineName";
        dd_businessreview.DataValueField = "MagazineID";
        dd_businessreview.DataBind();

        qry = qry.Replace("'BR'", "'CH'");
        dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_channel.DataSource = dt_mags;
        dd_channel.DataTextField = "ShortMagazineName";
        dd_channel.DataValueField = "MagazineID";
        dd_channel.DataBind();
    }

    protected void AddMag(object sender, EventArgs e)
    {
        // Configure
        Button b = (Button)sender;
        String type = String.Empty;
        String type_name = String.Empty;
        String new_mag_shortname = String.Empty;
        String new_mag_fullname = String.Empty;
        if (b.ID.Contains("br"))
        {
            type = "BR";
            type_name = "Business Chief";
            new_mag_fullname = tb_new_br_fullname.Text.Trim();
            new_mag_shortname = tb_new_br_shortname.Text.Trim();
        }
        else if (b.ID.Contains("ch"))
        {
            type = "CH";
            type_name = "Channel";
            new_mag_fullname = tb_new_ch_fullname.Text.Trim();
            new_mag_shortname = tb_new_ch_shortname.Text.Trim();
        }

        if (new_mag_fullname.Length > 100)
            Util.PageMessageAlertify(this, "Magazine name must be 100 characters max!", "Name Too Long");
        else if (new_mag_shortname.Length > 50)
            Util.PageMessageAlertify(this, "Short Magazine name must be 50 characters max!", "Name Too Long");
        else if (!String.IsNullOrEmpty(new_mag_fullname) && !String.IsNullOrEmpty(new_mag_shortname))
        {
            // Add new
            String iqry = "INSERT INTO db_magazine (MagazineName, ShortMagazineName, MagazineType) VALUES(@full_mag_name, @short_mag_name, @mag_type)";
            String[] pn = new String[] { "@full_mag_name", "@short_mag_name", "@mag_type" };
            Object[] pv = new Object[] { new_mag_fullname, new_mag_shortname, type };
            String msg = type_name + " Magazine '" + new_mag_fullname + "' sucessfully added.";
            try
            {
                SQL.Insert(iqry, pn, pv);
                Util.PageMessage(this, msg);
                Util.WriteLogWithDetails(msg, "magmanager_log");
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else if (r.Message.Contains("Duplicate"))
                {
                    // Check to see if this mag has been set non-live, if so, make live
                    String qry = "SELECT IsLive FROM db_magazine WHERE MagazineName=@full_mag_name AND ShortMagazineName=@short_mag_name AND MagazineType=@mag_type";
                    if (SQL.SelectString(qry, "IsLive", pn, pv) == "0")
                    {
                        String uqry = "UPDATE db_magazine SET IsLive=1 WHERE MagazineName=@full_mag_name AND ShortMagazineName=@short_mag_name AND MagazineType=@mag_type";
                        SQL.Update(uqry, pn, pv);
                        Util.PageMessage(this, msg);
                        Util.WriteLogWithDetails(msg, "magmanager_log");
                    }
                    else
                        Util.PageMessage(this, "Error adding magazine, a magazine by that full name/short name already exists!");
                }
                else
                {
                    Util.PageMessage(this, "Unknown error while adding magazine.");
                    Util.WriteLogWithDetails(r.Message + " " + r.StackTrace + " " + r.InnerException, "magmanager_log");
                }
            }
            finally
            {
                BindMags();
            }
        }
        else
            Util.PageMessage(this, "You must specify full name/short name for the new magazine!"); //distinct
    }
    protected void RemoveMag(object sender, EventArgs e)
    {
        Button b = (Button)sender;
        String type = String.Empty;
        DropDownList d = new DropDownList();
        if(b.ID.Contains("br"))
        {
            type = "Business Chief";
            d = dd_businessreview;
        }
        else if (b.ID.Contains("ch"))
        {
            type = "Channel";
            d = dd_channel;
        }

        // Set existing mag not live
        String uqry = "UPDATE db_magazine SET IsLive=0 WHERE MagazineID=@mag_id"; 
        SQL.Update(uqry, "@mag_id", d.SelectedItem.Value);

        String msg = type + " Magazine '" + d.SelectedItem.Text + "' sucessfully removed.";
        Util.WriteLogWithDetails(msg, "magmanager_log");
        Util.PageMessage(this, msg);
        BindMags();
    }
}