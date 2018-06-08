using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class tree : System.Web.UI.Page
{
    private List<List<Node>> _tree = new List<List<Node>>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindTreeSelection();
            DrawTree(null, null);
        }
    }

    protected void DrawTree(object sender, EventArgs e)
    {
        if(dd_tree.Items.Count > 0 && dd_tree.SelectedItem.Value != null)
        {
            // Build tree, 0 is always root for all trees
            BuildTree(0);

            // Draw tree
            foreach (List<Node> treeLevel in _tree)
            {
                HtmlTableRow r = new HtmlTableRow();
                foreach (Node n in treeLevel)
                {
                    HtmlTableCell c = new HtmlTableCell();
                    r.Cells.Add(c);

                    c.Align = "Center";
                    int ColSpan = n.ChildrenCount;
                    //if (ColSpan == 0)
                    //    ColSpan = 1;

                    c.ColSpan = ColSpan;
                    c.Controls.Add(n.NodeControl);

                    Util.Debug(n.Text + " has " + n.ChildrenCount + " children, this colspan is now " + c.ColSpan);
                }
                tbl_tree.Rows.Add(r);
            }
        }
        else
            Util.PageMessage(this, "There are no trees to draw!");
    }
    protected void BuildTree(int ParentNodeId)
    {
        // Get the node(s) for this level
        String qry = "SELECT node_id FROM node WHERE parent_node_id=@parent_node_id AND tree_id=@tree_id";
        DataTable dt_nodes = SQL.SelectDataTable(qry,
            new String[] { "@parent_node_id", "@tree_id" },
            new Object[] { ParentNodeId, dd_tree.SelectedItem.Value });

        if(dt_nodes.Rows.Count > 0)
        {
            // Create a tree level
            List<Node> thisTreeLevel = new List<Node>();
            // Add all nodes on this level to the tree level
            foreach (DataRow dr in dt_nodes.Rows)
            {
                Node n = new Node(dr["node_id"].ToString());
                thisTreeLevel.Add(n);
                BuildTree(n.NodeId);
            }

            // Add this tree level to the tree
            _tree.Add(thisTreeLevel);
        }
    }























































    protected void RefreshTree(object sender, EventArgs e)
    {
        DrawTree(null, null);
    }
    protected void BindTreeSelection()
    {
        String qry = "SELECT tree_id, tree_name FROM tree";
        dd_tree.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_tree.DataTextField = "tree_name";
        dd_tree.DataValueField = "tree_id";
        dd_tree.DataBind();
    }
}
