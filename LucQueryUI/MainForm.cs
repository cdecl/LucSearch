using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Xml;

namespace LucQueryUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtDir.Text = @"";
            txtQuery.Text = "";
            txtTop.Text = "10";
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                string ROOT = txtDir.Text;
                string query = txtQuery.Text;
                int nTop = int.Parse(txtTop.Text);
                XmlDocument doc = LucSearchLib.CJKSearch.SearchQuery(ROOT, query, 0, nTop);

                trResult.Nodes.Clear();
                trResult.Nodes.Add(new TreeNode(doc.DocumentElement.Name));
                TreeNode tNode = new TreeNode();
                tNode = (TreeNode)trResult.Nodes[0];
                addTreeNode(doc.DocumentElement, tNode);
                trResult.ExpandAll();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void addTreeNode(XmlNode xmlNode, TreeNode treeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList xNodeList;
            if (xmlNode.HasChildNodes) //The current node has children
            {
                xNodeList = xmlNode.ChildNodes;
                for (int x = 0; x <= xNodeList.Count - 1; x++)
                //Loop through the child nodes
                {
                    xNode = xmlNode.ChildNodes[x];
                    treeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = treeNode.Nodes[x];
                    addTreeNode(xNode, tNode);
                }
            }
            else {
                treeNode.Text = xmlNode.OuterXml.Trim();
            }
        }
    }
}
