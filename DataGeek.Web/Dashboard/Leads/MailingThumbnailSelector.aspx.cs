// Author   : Joe Pickering, 25/05/17
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections;

public partial class MailingThumbnailSelector : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            BindIssues();
            BindPaddingDropDowns();
            BindMagazinesIssues(null, null);
        }
    }

    private void BindIssues()
    {
        String qry = "SELECT DISTINCT(IssueName) as bn FROM db_salesbookhead ORDER BY StartDate DESC";
        dd_issue.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_issue.DataTextField = "bn";
        dd_issue.DataBind();

        if (dd_issue.Items.Count > 2)
            dd_issue.SelectedIndex = 2;
    }
    private void BindPaddingDropDowns()
    {
        ArrayList dds = new ArrayList() { dd_padding_overall, dd_padding_bottom, dd_padding_left, dd_padding_right, dd_padding_top };
        foreach(RadDropDownList dd in dds)
        {
            dd.Items.Add(new DropDownListItem("None", "0"));
            for(int i=1; i<16; i++)
                dd.Items.Add(new DropDownListItem(i+" Pixels", i.ToString()));
        }

        // Set initial padding selection
        String qry = "SELECT * FROM dbl_preferences WHERE UserID=@UserID";
        DataTable dt_paddings = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());
        if (dt_paddings.Rows.Count > 0)
        {
            String Overall = dt_paddings.Rows[0]["LastUsedThumbnailOverallPadding"].ToString();
            String Top = dt_paddings.Rows[0]["LastUsedThumbnailTopPadding"].ToString();
            String Left = dt_paddings.Rows[0]["LastUsedThumbnailLeftPadding"].ToString();
            String Bottom = dt_paddings.Rows[0]["LastUsedThumbnailBottomPadding"].ToString();
            String Right = dt_paddings.Rows[0]["LastUsedThumbnailRightPadding"].ToString();
            bool UseMagazineDropShadow = dt_paddings.Rows[0]["UseMagazineDropShadow"].ToString() == "1";

            if (!String.IsNullOrEmpty(Overall) && dd_padding_overall.FindItemByValue(Overall) != null)
                dd_padding_overall.SelectedIndex = dd_padding_overall.FindItemByValue(Overall).Index;
            if (!String.IsNullOrEmpty(Top) && dd_padding_top.FindItemByValue(Top) != null)
                dd_padding_top.SelectedIndex = dd_padding_top.FindItemByValue(Top).Index;
            if (!String.IsNullOrEmpty(Left) && dd_padding_left.FindItemByValue(Left) != null)
                dd_padding_left.SelectedIndex = dd_padding_left.FindItemByValue(Left).Index;
            if (!String.IsNullOrEmpty(Bottom) && dd_padding_bottom.FindItemByValue(Bottom) != null)
                dd_padding_bottom.SelectedIndex = dd_padding_bottom.FindItemByValue(Bottom).Index;
            if (!String.IsNullOrEmpty(Right) && dd_padding_right.FindItemByValue(Right) != null)
                dd_padding_right.SelectedIndex = dd_padding_right.FindItemByValue(Right).Index;

            cb_drop_shadow.Checked = UseMagazineDropShadow;
        }
    }
    protected void BindMagazinesIssues(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null && dd_issue.SelectedItem.Text != String.Empty)
        {
            String qry = "SELECT MagazineName, CONCAT(MagazineLink,'|',MagazineImageURL) as 'mag_info' FROM db_magazinelinks ml, db_magazine m "+
            "WHERE ml.MagazineID = m.MagazineID AND IssueName=@book_name ORDER BY MagazineName";
            DataTable dt_magazines = SQL.SelectDataTable(qry, "@book_name", dd_issue.SelectedItem.Text);
            dd_magazine.DataSource = dt_magazines;
            dd_magazine.DataTextField = "MagazineName";
            dd_magazine.DataValueField = "mag_info";
            dd_magazine.DataBind();

            bool HasThumbs = dd_magazine.Items.Count > 0 && dd_magazine.SelectedItem != null;
            if (!HasThumbs && sender != null)
                Util.PageMessageAlertify(this, "There are no thumbnails/links for this issue..", "Oh");

            tr_preview.Visible = HasThumbs;
            btn_import.Visible = HasThumbs;

            if (HasThumbs)
                BindMagazineOrBrochurePreview(true);
        }
    }
    protected void BindMagazineOrBrochurePreview(bool IsMagazine)
    {
        tr_preview.Visible = true;

        String ThumbnailImgUrl = String.Empty;
        String ThumbnailLink = String.Empty;
        if (IsMagazine)
        {
            if (dd_magazine.Items.Count > 0 && dd_magazine.SelectedItem != null)
            {
                String MagazineName = dd_magazine.SelectedItem.Text;
                String MagazineData = dd_magazine.SelectedItem.Value;

                ThumbnailLink = MagazineData.Substring(0, MagazineData.IndexOf("|")).Replace("https", "http");
                ThumbnailImgUrl = MagazineData.Substring(MagazineData.IndexOf("|") + 1);
            }
            else
                Util.PageMessageAlertify(this, "Couldn't bind magazine thumbnail preview!", "Oops");
        }
        else
        {
            String EditorialFeatureID = rcb_feature_search.SelectedValue;
            String qry = "SELECT WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl FROM db_editorialtracker WHERE EditorialID=@ent_id";
            DataTable dt_feature = SQL.SelectDataTable(qry, "@ent_id", EditorialFeatureID);
            if(dt_feature.Rows.Count > 0)
            {
                ThumbnailImgUrl = dt_feature.Rows[0]["WidgetThumbnailImgURL"].ToString();
                ThumbnailLink = dt_feature.Rows[0]["WidgetTerritoryBrochureUrl"].ToString();
            }
            else
                Util.PageMessageAlertify(this, "Couldn't bind brochure thumbnail preview!", "Oops");
        }

        btn_import.Visible = true;

        img_preview.ImageUrl = ThumbnailImgUrl;
        hl_mag.OnClientClick = "window.open('" + ThumbnailLink + "', '_blank');";
        hl_link.Text = hl_link.NavigateUrl = ThumbnailLink;

        // Determine margins
        String Margin = "0px";
        if (dd_padding_overall.SelectedIndex > 0)
            Margin = dd_padding_overall.SelectedItem.Value + "px";
        else if (dd_padding_left.SelectedIndex > 0 || dd_padding_right.SelectedIndex > 0 || dd_padding_top.SelectedIndex > 0 || dd_padding_bottom.SelectedIndex > 0)
            Margin = dd_padding_top.SelectedItem.Value + "px " + dd_padding_right.SelectedItem.Value + "px " + dd_padding_bottom.SelectedItem.Value + "px " + dd_padding_left.SelectedItem.Value + "px";

        String Shadow = String.Empty;
        if (cb_drop_shadow.Checked)
        {
            img_preview.CssClass = "MagLink";
            Shadow = "; box-shadow:6px 6px 5px #888888";
        }

        String BlackBorderCss = String.Empty;
        String Size = "Width=\"144\" Height=\"204\"";
        if (cb_black_border.Checked)
        {
            BlackBorderCss = "; border:solid 1px black !important";
            img_preview.Style.Add("border", "solid 1px black !important;");
            Size = "Width=\"143\" Height=\"203\"";
            img_preview.Height = 254;
            img_preview.Width = 179;
        }
        else
        {
            img_preview.Style.Remove("border");
            img_preview.Height = 255;
            img_preview.Width = 180;
        }

        Session["import_mag_html"] = "<a href=\""
            + ThumbnailLink + "\" target=\"_blank\"><img style='margin:" + Margin + Shadow + BlackBorderCss + ";' src=\"" + ThumbnailImgUrl + "\" " + Size + " alt=\"Magazine\"></a>";
        //Width=\"180\" Height=\"255\"

        if (Margin != "0px")
            div_padding_preview.Attributes.Add("style", "padding:" + Margin + "; border:solid 1px orange; width:180px;");
        else
            div_padding_preview.Attributes.Clear();

        // Save padding
        String uqry = "UPDATE dbl_preferences SET LastUsedThumbnailOverallPadding=@Overall, LastUsedThumbnailTopPadding=@Top, LastUsedThumbnailLeftPadding=@Left, " +
        "LastUsedThumbnailRightPadding=@Right, LastUsedThumbnailBottomPadding=@Bottom WHERE UserID=@UserID";
        SQL.Update(uqry, new String[] { "@Overall", "@Top", "@Left", "@Right", "@Bottom", "@UserID" },
            new Object[] { 
            dd_padding_overall.SelectedItem.Value, 
            dd_padding_top.SelectedItem.Value, 
            dd_padding_left.SelectedItem.Value, 
            dd_padding_right.SelectedItem.Value, 
            dd_padding_bottom.SelectedItem.Value, 
            Util.GetUserId() });

        Util.ResizeRadWindow(this);
    }
    protected void BindMagazinePreview(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        BindMagazineOrBrochurePreview(true);
    }
    protected void BindFeaturePreview(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
    {
        rcb_feature_search.SelectedValue = e.Value;
        BindMagazineOrBrochurePreview(false);
    }

    protected void PerformFeatureSearch(object sender, RadComboBoxItemsRequestedEventArgs e)
    {
        rcb_feature_search.Items.Clear();

        String search_term = e.Text;
        String qry = "SELECT DISTINCT EditorialID, CONCAT(Feature,' (',IssueName,')') as 'Feature' "+
        "FROM db_editorialtracker et, db_editorialtrackerissues eti WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
        "AND IsCancelled=0 AND WidgetThumbnailImgURL IS NOT NULL AND WidgetTerritoryBrochureUrl IS NOT NULL " +
        "AND Feature LIKE @feature AND Feature IS NOT NULL AND Feature!='' ORDER BY Feature LIMIT 10";
        DataTable dt_features = new DataTable();
        if (search_term != String.Empty)
            dt_features = SQL.SelectDataTable(qry, "@feature", search_term + "%");

        for (int i = 0; i < dt_features.Rows.Count; i++)
        {
            RadComboBoxItem item = new RadComboBoxItem();
            item.Text = dt_features.Rows[i]["Feature"].ToString();
            item.Value = dt_features.Rows[i]["EditorialID"].ToString();
            rcb_feature_search.Items.Add(item);
        }

        rcb_feature_search.ClearSelection();
        rcb_feature_search.DataBind();
    }
    protected void ImportThumbnail(object sender, EventArgs e)
    {
        if (Session["import_mag_html"] != null)
            Util.CloseRadWindowFromUpdatePanel(this, Session["import_mag_html"].ToString(), false);
    }
    protected void ChangeInsertType(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        bool IsBrochure = dd_type.SelectedItem.Text.Contains("Brochure");
        int DivHeight = !IsBrochure ? 230 : 300;
        rcb_feature_search.Text = String.Empty;
        rcb_feature_search.SelectedValue = String.Empty;

        tr_bro_feature.Visible = IsBrochure;
        tr_issue.Visible = tr_mag_issue.Visible = !IsBrochure;

        btn_import.Visible = false;
        tr_preview.Visible = false;

        if (!IsBrochure && dd_magazine.Items.Count > 0 && dd_magazine.SelectedItem != null)
            BindMagazineOrBrochurePreview(!IsBrochure);

        div_main.Style.Add("height", DivHeight+"px");    

        Util.ResizeRadWindow(this);
    }
    protected void ToggleDropShadow(object sender, EventArgs e)
    {
        if (cb_drop_shadow.Checked)
            img_preview.CssClass = "MagLink";
        else
            img_preview.CssClass = String.Empty;

        String uqry = "UPDATE dbl_preferences SET UseMagazineDropShadow=CASE WHEN UseMagazineDropShadow=1 THEN 0 ELSE 1 END WHERE UserID=@UserID";
        SQL.Update(uqry, "@UserID", Util.GetUserId());
    }
    protected void RemoveAllPadding(object sender, EventArgs e)
    {
        dd_padding_overall.SelectedIndex = dd_padding_left.SelectedIndex = dd_padding_right.SelectedIndex = dd_padding_top.SelectedIndex = dd_padding_bottom.SelectedIndex = 0;
        BindMagazineOrBrochurePreview(tr_issue.Visible);
    }
    protected void PaddingChanged(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        RadDropDownList dd_margin = (RadDropDownList)sender;
        if (dd_margin.ToolTip == "Overall")
            dd_padding_bottom.SelectedIndex = dd_padding_left.SelectedIndex = dd_padding_right.SelectedIndex = dd_padding_top.SelectedIndex = 0;
        else if (dd_margin.ToolTip == "Top" || dd_margin.ToolTip == "Bottom" || dd_margin.ToolTip == "Left" || dd_margin.ToolTip == "Right")
            dd_padding_overall.SelectedIndex = 0;
        BindMagazineOrBrochurePreview(tr_issue.Visible);
    }
    protected void AddBlackBorder(object sender, EventArgs e)
    {
        BindMagazineOrBrochurePreview(tr_issue.Visible);
    }
}