// Author   : Joe Pickering, 13/06/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using MySql.Data.MySqlClient;
using AjaxControlToolkit;

public partial class MMEditLinks : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_mags_live);
            BindAllIssues();

            // Select issue if passed in query string
            if (Request.QueryString["issue"] != null && !String.IsNullOrEmpty(Request.QueryString["issue"]))
            {
                int issue_idx = -1;
                issue_idx = dd_issues.Items.IndexOf(dd_issues.Items.FindByText(Request.QueryString["issue"]));
                if (issue_idx != -1)
                    dd_issues.SelectedIndex = issue_idx;
                else
                    Util.PageMessage(this, "The issue " + dd_issues.SelectedItem.Text + " doesn't exist. Selecting " + dd_issues.SelectedItem.Text + " instead.");
            }
        }
        BuildMagazinesTemplate();

        if(!IsPostBack)
            FillMagazineData(null, null);
    }

    // Binding
    protected void BuildMagazinesTemplate()
    {
        String qry = "SELECT MagazineID, ShortMagazineName, MagazineType " +
        "FROM db_magazine " +
        "WHERE (IsLive=1 OR MagazineID IN (SELECT MagazineID FROM db_magazinelinks WHERE IssueName=@book_name)) " +
        "ORDER BY MagazineType, ShortMagazineName";
        DataTable dt_mag_list = SQL.SelectDataTable(qry, "@book_name", dd_issues.SelectedItem.Text);

        if (dt_mag_list.Rows.Count > 0)
        {
            HtmlTable tbl = new HtmlTable();
            tbl.Border = 0;
            tbl.ID = "tbl";
            HtmlTableRow r = new HtmlTableRow();
            HtmlTableCell c1 = new HtmlTableCell();
            HtmlTableCell c2 = new HtmlTableCell();
            HtmlTableCell c3 = new HtmlTableCell();
            HtmlTableCell c4 = new HtmlTableCell();
            div_mags.Controls.Add(tbl);
            r.Cells.Add(c1);
            r.Cells.Add(c2);
            r.Cells.Add(c3);
            r.Cells.Add(c4);
            c1.Controls.Add(new Label() { Text = "Magazine Name", ForeColor = Color.White, Width=135 });
            c2.Controls.Add(new Label() { Text = "Magazine Link (full URL inc. http://)", ForeColor = Color.White });
            c3.Controls.Add(new Label() { Text = "Cover Image Thumbnail URL (full URL inc. http://)", ForeColor = Color.White });
            c4.Controls.Add(new Label() { Text = "Preview Cover Image", ForeColor = Color.White });
            tbl.Rows.Add(r);

            bool break_added = false;
            for (int i = 0; i < dt_mag_list.Rows.Count; i++)
            {
                if (dt_mag_list.Rows[i]["ShortMagazineName"] != DBNull.Value && dt_mag_list.Rows[i]["ShortMagazineName"].ToString().Trim() != String.Empty)
                {
                    r = new HtmlTableRow();
                    c1 = new HtmlTableCell() { VAlign = "Top" };
                    c2 = new HtmlTableCell() { VAlign = "Top" };
                    c3 = new HtmlTableCell() { VAlign = "Top" };
                    c4 = new HtmlTableCell() { VAlign = "Top" };
                    r.Cells.Add(c1);
                    r.Cells.Add(c2);
                    r.Cells.Add(c3);
                    r.Cells.Add(c4);
                    tbl.Rows.Add(r);

                    // Add break when first channel mag is encountered
                    if (!break_added && dt_mag_list.Rows[i]["MagazineType"].ToString() == "CH")
                    {
                        c1.Controls.Add(new Label() { Text = "<BR/>" });
                        c2.Controls.Add(new Label() { Text = "<BR/>" });
                        c3.Controls.Add(new Label() { Text = "<BR/>" });
                        c4.Controls.Add(new Label() { Text = "<BR/>" });
                        break_added = true;
                    }

                    String mag_id = dt_mag_list.Rows[i]["MagazineID"].ToString();
                    Label lbl = new Label();
                    lbl.ForeColor = Color.DarkOrange;
                    lbl.ID = "lbl_" + mag_id;
                    lbl.Text = Server.HtmlEncode(dt_mag_list.Rows[i]["ShortMagazineName"].ToString()) + " (" + Server.HtmlEncode(dt_mag_list.Rows[i]["MagazineType"].ToString()) + ")&nbsp;&nbsp;";
                    lbl.Attributes.Add("style", "position:relative; top:5px;");

                    TextBox tb_url = new TextBox();
                    tb_url.ID = "tb_url-" + mag_id;
                    tb_url.Width = 340;
                    tb_url.Height = 13;
                    tb_url.Font.Size = 8;

                    TextBoxWatermarkExtender wme_url = new TextBoxWatermarkExtender();
                    wme_url.ID = "wme_url_" + mag_id;
                    wme_url.TargetControlID = tb_url.ID;
                    wme_url.WatermarkText = "e.g. http://www.execdigital.com/magazines/10531";
                    wme_url.WatermarkCssClass = "watermark";

                    TextBox tb_img = new TextBox();
                    tb_img.ID = "tb_img-" + mag_id;
                    tb_img.Width = 340;
                    tb_img.Height = 13;
                    tb_img.Font.Size = 8;

                    TextBoxWatermarkExtender wme_img = new TextBoxWatermarkExtender();
                    wme_img.ID = "wme_img_" + mag_id;
                    wme_img.TargetControlID = tb_img.ID;
                    wme_img.WatermarkText = "e.g. http://www.businessreviewindia.in/magazines/api/get_thumbnail.php?magazineid=13737&websiteid=192";
                    wme_img.WatermarkCssClass = "watermark";

                    RegularExpressionValidator rev_link = new RegularExpressionValidator();
                    rev_link.ValidationExpression = Util.regex_url;
                    rev_link.ForeColor = Color.Red;
                    rev_link.ControlToValidate = tb_url.ID;
                    rev_link.Display = ValidatorDisplay.Dynamic;
                    rev_link.ErrorMessage = "<br/>Not a valid URL!";

                    RegularExpressionValidator rev_img = new RegularExpressionValidator();
                    rev_img.ValidationExpression = Util.regex_url;
                    rev_img.ForeColor = Color.Red;
                    rev_img.ControlToValidate = tb_img.ID;
                    rev_img.Display = ValidatorDisplay.Dynamic;
                    rev_img.ErrorMessage = "<br/>Not a valid URL!";

                    c1.Controls.Add(lbl);
                    c2.Controls.Add(tb_url);
                    c2.Controls.Add(wme_url);
                    c2.Controls.Add(rev_link);
                    c3.Controls.Add(tb_img);
                    c3.Controls.Add(wme_img);
                    c3.Controls.Add(rev_img);

                    Button btn = new Button() { ID = "btn_" + mag_id, Text = "Toggle Cover Preview" };
                    btn.Attributes.Add("disabled", "true");
                    btn.Attributes.Add("style", "position:relative; top:1px;");
                    c4.Controls.Add(btn);
                }
            }
        }
    }
    protected void FillMagazineData(object sender, EventArgs e)
    {
        GetSelectedIssuePublicationDate();

        lbl_edit_book_links.Text = "Edit magazine info for the " + Server.HtmlEncode(dd_issues.SelectedItem.Text) + " publication.";

        String qry = "SELECT * FROM db_magazinelinks WHERE IssueName=@book_name";
        DataTable dt_links = SQL.SelectDataTable(qry, "@book_name", dd_issues.SelectedItem.Text);

        String link = String.Empty;
        String img_url = String.Empty;
        TextBox tb_link = null;
        foreach (HtmlTableRow ctrl in div_mags.Controls[0].Controls)
        {
            foreach (HtmlTableCell tc in ctrl.Controls)
            {
                for(int j=0; j< tc.Controls.Count; j++)
                {
                    Control c = tc.Controls[j];
                    if (c is TextBox)
                    {
                        TextBox tb = (TextBox)c;
                        tb.Text = String.Empty;
                        
                        if (tb.ID.Contains("tb_url"))
                        {
                            link = tb.Text.Trim();
                            tb_link = tb;
                        }
                        else if (tb.ID.Contains("tb_img"))
                        {
                            String mag_id = tb.ID.Substring(tb.ID.IndexOf("-") + 1);
                            img_url = tb.Text.Trim();
                            for (int i = 0; i < dt_links.Rows.Count; i++)
                            {
                                if (dt_links.Rows[i]["MagazineID"].ToString() == mag_id)
                                {
                                    tb.Text = dt_links.Rows[i]["MagazineImageURL"].ToString();
                                    tb_link.Text = dt_links.Rows[i]["MagazineLink"].ToString();

                                    // Add a preview image to preview cell
                                    if (tb.Text != String.Empty)
                                    {
                                        HtmlTableCell cell = ((HtmlTableRow)tb.Parent.Parent).Cells[3];

                                        HyperLink hl = new HyperLink();
                                        hl.ID = "img_" + mag_id;
                                        hl.ImageUrl = tb.Text + "?v=" + DateTime.Now.Ticks;
                                        if (tb_link.Text.Trim() != String.Empty && (tb_link.Text.Contains("http") || tb_link.Text.Contains("www")))
                                        {
                                            hl.NavigateUrl = tb_link.Text;
                                            hl.Target = "_blank";
                                        }
                                        hl.Attributes.Add("style", "display:none;");

                                        String dead_img_alert = String.Empty;
                                        if (!Util.UrlExists(tb.Text))
                                            dead_img_alert = "alert('The URL to this cover thumbnail image is broken. Please update the Cover Image URL, save, and re-check OR make sure the image exists at the existing URL and click the Refresh Cover Images button.'); ";

                                        Button btn_preview = ((Button)cell.FindControl("btn_" + mag_id));
                                        btn_preview.Attributes.Clear();
                                        btn_preview.Attributes.Add("enabled", "true");
                                        btn_preview.OnClientClick =
                                            "var img = grab('Body_" + hl.ClientID + "'); " +
                                            "if(img.style.display=='block'){ "+
                                            "   img.style.display='none';} "+
                                            "else{ " +
                                            "   img.style.display='block'; "+dead_img_alert+" } " +
                                            "return false;";

                                        //cell.Controls.Add(hl);
                                        ((HtmlTableRow)tb.Parent.Parent).Cells[1].Controls.Add(hl);
                                    }

                                    break;
                                }
                            }
                            link = img_url = String.Empty;
                            tb_link = null;
                        }
                    }
                }
            }
        }
    }
    protected void GetSelectedIssuePublicationDate()
    {
        bool has_publication_date = false;
        DateTime dt_live = Util.GetIssuePublicationDate(dd_issues.SelectedItem.Text, out has_publication_date);
        if(has_publication_date)
            dp_mags_live.SelectedDate = dt_live;
        else
            dp_mags_live.SelectedDate = null;
    }
   
    // Save
    protected void SaveMagazineInfoAndPublicationDate(object sender, EventArgs e)
    {
        // Get copy of existing
        String qry = "SELECT * FROM db_magazinelinks WHERE IssueName=@sb_issue";
        DataTable dt_links = SQL.SelectDataTable(qry, "@sb_issue", dd_issues.SelectedItem.Text);
        ArrayList existing_links = new ArrayList();
        for (int i=0; i<dt_links.Rows.Count; i++)
            existing_links.Add(dt_links.Rows[i]["MagazineLink"].ToString());

        // Delete existing
        String dqry = "DELETE FROM db_magazinelinks WHERE IssueName=@sb_issue";
        SQL.Delete(dqry, "@sb_issue", dd_issues.SelectedItem.Text);

        String link = String.Empty;
        String img_url = String.Empty;
        bool error_inserting = false;
        try
        {
            foreach (HtmlTableRow tr in div_mags.Controls[0].Controls)
            {
                foreach (HtmlTableCell tc in tr.Controls)
                {
                    foreach (Control c in tc.Controls)
                    {
                        if (c is TextBox)
                        {
                            TextBox tb = (TextBox)c;
                            String mag_id = tb.ID.Substring(tb.ID.IndexOf("-") + 1);
                            String mag_name = ((Label)((HtmlTableRow)tb.Parent.Parent).Cells[0].Controls[0]).Text.Replace("&nbsp;",String.Empty);

                            if (tb.ID.Contains("tb_url"))
                                link = tb.Text.Trim();
                            else if (tb.ID.Contains("tb_img"))
                            {
                                img_url = tb.Text.Trim();
                                if (link != String.Empty || img_url != String.Empty)
                                {
                                    if (!existing_links.Contains(link))
                                        Util.WriteLogWithDetails("Magazine " + mag_name + " issue link (" + link + ", " + img_url + ") added for " + dd_issues.SelectedItem.Text, "magmanager_log");

                                    if (img_url == String.Empty)
                                        img_url = null;

                                    String iqry = "INSERT INTO db_magazinelinks (IssueName, MagazineID, MagazineLink, MagazineImageURL) VALUES(@sb_issue, @mag_id, @link, @img_url)";
                                    SQL.Insert(iqry,
                                        new String[] { "@sb_issue", "@mag_id", "@link", "@img_url" },
                                        new Object[] { dd_issues.SelectedItem.Text, mag_id, link, img_url });

                                    link = img_url = String.Empty;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception r)
        {
            error_inserting = true;
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails("Error updating links. " + r.Message + " " + r.StackTrace, "magmanager_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }

        // update mags live date
        dqry = "DELETE FROM db_publicationlivedates WHERE IssueName=@issue_name; INSERT INTO db_publicationlivedates (IssueName, DateLive) VALUES (@issue_name, @live_date);";
        SQL.Delete(dqry, new String[] { "@issue_name", "@live_date" }, new Object[] { dd_issues.SelectedItem.Text, dp_mags_live.SelectedDate });

        if (!error_inserting)
        {
            Util.WriteLogWithDetails("Magazine issue links saved for " + dd_issues.SelectedItem.Text, "magmanager_log");
            Util.PageMessage(this, "Links saved for " + dd_issues.SelectedItem.Text + ".");
        }

        FillMagazineData(null, null);
    }

    protected void BindAllIssues()
    {
        String qry = "SELECT DISTINCT(IssueName) as bn FROM db_salesbookhead ORDER BY StartDate DESC";
        dd_issues.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_issues.DataTextField = "bn";
        dd_issues.DataBind();
    }
}