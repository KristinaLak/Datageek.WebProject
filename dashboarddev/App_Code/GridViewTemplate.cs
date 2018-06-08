using System;
using System.Net;
using System.IO;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public class GridViewTemplate : ITemplate
{
    private String field_name;
    public GridViewTemplate(String colname)
    {
        field_name = colname;
    }

    void ITemplate.InstantiateIn(Control container)
    {
        // For Finance
        if (field_name == "imbtns")
        {
            ImageButton imbtn = new ImageButton();
            imbtn.ImageUrl = "~\\images\\Icons\\gridView_ChangeIssue.png";
            imbtn.Width = imbtn.Height = 16;
            imbtn.ToolTip = "Move to different tab";
            imbtn.ID = container.ID + "1";
            container.Controls.Add(imbtn);
            imbtn.EnableViewState = false;

            Label br = new Label();
            br.Text = "&nbsp;";
            br.ID = container.ID + "2";
            container.Controls.Add(br);

            imbtn = new ImageButton();
            imbtn.ID = container.ID + "3";
            imbtn.EnableViewState = false;
            imbtn.Width = imbtn.Height = 16;
            imbtn.ImageUrl = "~\\images\\Icons\\dashboard_Colours.png";
            imbtn.ToolTip = "Update Sale Status";
            container.Controls.Add(imbtn);
        }
        else // For list dist
        {
            TextBox tbc = new TextBox();
            tbc.Width = 100;
            tbc.Text = "0";
            tbc.DataBinding += new EventHandler(tb_DataBinding);
            container.Controls.Add(tbc);
            Label lblc = new Label();
            lblc.Width = 80;
            lblc.DataBinding += new EventHandler(lbl_DataBinding);
            container.Controls.Add(lblc);
        }
    }

    private void tb_DataBinding(object sender, EventArgs e)
    {
        TextBox txt = (TextBox)sender; 
        GridViewRow container = (GridViewRow)txt.NamingContainer;
        Object dataValue = DataBinder.Eval(container.DataItem, field_name); 
        if (dataValue != DBNull.Value)
            txt.Text = HttpContext.Current.Server.HtmlDecode(dataValue.ToString()); 
    }
    private void lbl_DataBinding(object sender, EventArgs e)
    {
        Label lbl = (Label)sender; 
        GridViewRow container = (GridViewRow)lbl.NamingContainer; 
        Object dataValue = DataBinder.Eval(container.DataItem, field_name);
        if (dataValue != DBNull.Value)
            lbl.Text = dataValue.ToString();
    }
}
