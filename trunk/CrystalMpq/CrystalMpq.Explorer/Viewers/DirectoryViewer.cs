#region Copyright Notice
// This file is part of CrystalMPQ.
// 
// Copyright (C) 2007-2011 Fabien BARBIER
// 
// CrystalMPQ is licenced under the Microsoft Reciprocal License.
// You should find the licence included with the source of the program,
// or at this URL: http://www.microsoft.com/opensource/licenses.mspx#Ms-RL
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CrystalMpq.Explorer.Extensibility;

namespace CrystalMpq.Explorer.Viewers
{
	sealed partial class DirectoryViewer : FileViewer
	{
		MainForm mainForm;
		TreeNode rootNode;

		public DirectoryViewer(MainForm mainForm, IHost host)
			: base(host)
		{
			this.mainForm = mainForm;
			InitializeComponent();
			listView.SmallImageList = mainForm.file16ImageList;
			listView.LargeImageList = mainForm.file32ImageList;
		}

		public override MenuStrip Menu { get { return menuStrip; } }
		public override ToolStrip MainToolStrip { get { return mainToolStrip; } }
		public override StatusStrip StatusStrip { get { return statusStrip; } }

		public TreeNode RootNode
		{
			get
			{
				return rootNode;
			}
			set
			{
				if (value != rootNode)
				{
					rootNode = value;
					UpdateView();
				}
			}
		}

		private void UpdateView()
		{
			List<ListViewItem> items;

			listView.SuspendLayout();
			listView.Items.Clear();
			if (rootNode == null)
				return;
			items = new List<ListViewItem>(rootNode.Nodes.Count);
			foreach (TreeNode childNode in rootNode.Nodes)
			{
				ListViewItem lvi = new ListViewItem();

				lvi.Text = childNode.Text;
				lvi.ImageIndex = Math.Max(0, childNode.ImageIndex);
				lvi.Tag = childNode;

				items.Add(lvi);
			}
			listView.Items.AddRange(items.ToArray());
			listView.ResumeLayout(false);
		}

		private void listView_ItemActivate(object sender, EventArgs e)
		{
			TreeNode node = listView.SelectedItems[0].Tag as TreeNode;

			node.TreeView.SelectedNode = node;
		}
	}
}
