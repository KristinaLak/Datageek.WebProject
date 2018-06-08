using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class OfficeToggler : System.Web.UI.UserControl
{
    private DropDownList dd_office = null;
    public event EventHandler TogglingOffice;
    public bool IncludeRegionalGroup = false;
    public int Top = 0;
    public int Left = 0;
    private bool _Enabled = true;

    public bool Enabled
    {
        get { return _Enabled; }
        set
        { 
            _Enabled = value;
            BindOffices();
        } 
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            div_toggle_office.Attributes.Add("style", "display:relative; top:15px;");

        // Set region
        dd_office = (DropDownList)Util.FindControlIterative(this.Parent.Page, "dd_office");
        if (dd_office != null)
        {
            String qry = "SELECT Region FROM db_dashboardoffices WHERE Office=@office";
            String this_region = SQL.SelectString(qry, "region", "@office", dd_office.SelectedItem.Text);
            if(this_region != String.Empty)
                hf_region.Value = this_region;
        }

        BindOffices();
    }

    protected void BindOffices()
    {
        div_toggle_office.Controls.Clear();
        String qry = "SELECT * FROM db_dashboardoffices WHERE Closed=0 AND Region=@region";
        DataTable dt_offices = SQL.SelectDataTable(qry, "@region", hf_region.Value);
        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            if (dt_offices.Rows[i]["Region"].ToString() == hf_region.Value)
            {
                LinkButton l = new LinkButton();
                l.Enabled = _Enabled;
                l.Click += new EventHandler(ToggleOffice);
                l.ForeColor = System.Drawing.Color.Silver;
                l.Text = dt_offices.Rows[i]["Office"].ToString();
                l.ID = dt_offices.Rows[i]["Office"].ToString().Replace(" ", String.Empty);
                if (i < dt_offices.Rows.Count - 1)
                    l.Attributes.Add("style", "border-right:solid 1px gray; margin-right:3px; padding-right:4px; position:relative; left:" + Left + "px; top:" + Top + "px;");
                else
                    l.Attributes.Add("style", "position:relative; left:" + Left + "px; top:" + Top + "px;");
                div_toggle_office.Controls.Add(l);
            }
        }

        if (IncludeRegionalGroup && dt_offices.Rows.Count > 0 && (hf_region.Value == "US" || hf_region.Value == "UK") 
        && (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS") || RoleAdapter.IsUserInRole("db_Finance")))
        {
            LinkButton l = new LinkButton();
            l.Enabled = _Enabled;
            l.Click += new EventHandler(ToggleOffice);
            l.ForeColor = System.Drawing.Color.Silver;
            l.Text = "Americas";
            if (hf_region.Value == "UK")
                l.Text = "EMEA";
            l.Attributes.Add("style", "border-left:solid 1px gray; margin-left:3px; padding-left:4px; position:relative; left:" + Left + "px; top:" + Top + "px;");
            div_toggle_office.Controls.Add(l);
        }
    }
    protected void ToggleOffice(object sender, EventArgs e)
    {
        LinkButton l = (LinkButton)sender;
        if (dd_office != null)
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(l.Text));

        OnTogglingOffice();
    }
    protected void OnTogglingOffice()
    {
        if (TogglingOffice != null)
            TogglingOffice(this, EventArgs.Empty);
    }
}