// Author   : Joe Pickering, 25.02.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.Collections.Generic;

public partial class Share : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["MagID"] != null && !String.IsNullOrEmpty(Request.QueryString["MagID"]))
            {
                hf_mag_id.Value = Request.QueryString["MagID"];
                SetShareInformation();
            }
        }
    }

    private void SetShareInformation()
    {
        String qry = "SELECT * FROM db_smartsocialmagazine WHERE SmartSocialMagazineID=@mag_id";
        DataTable dt_mag_info = SQL.SelectDataTable(qry, "@mag_id", hf_mag_id.Value);
        if(dt_mag_info.Rows.Count > 0)
        {
            String[] removables = new String[] { "1","2","3","4","5","6","7","8","9","0" };
            String magazine_type = dt_mag_info.Rows[0]["Type"].ToString();
            if (removables.Any(magazine_type.EndsWith))
                magazine_type = magazine_type.Remove(magazine_type.Length - 1).Trim();

            String image = dt_mag_info.Rows[0]["CoverImageURL"].ToString();
            String url = dt_mag_info.Rows[0]["NavigationURL"].ToString();
            String title = "Our " + magazine_type;
            String summary = "Check out our " + magazine_type + " here " + url;

            List<Control> spans = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(div_share_icons, ref spans, typeof(HtmlGenericControl));
            foreach (HtmlGenericControl span in spans)
            {
                span.Attributes.Add("st_title", title);
                span.Attributes.Add("st_image", image);
                span.Attributes.Add("st_url", url);
                span.Attributes.Add("st_summary", summary);
            }
        }
    }
}