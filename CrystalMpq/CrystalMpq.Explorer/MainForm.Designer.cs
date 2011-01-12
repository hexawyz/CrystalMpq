#region Copyright Notice
// This file is part of CrystalMPQ.
// 
// Copyright (C) 2007-2011 Fabien BARBIER
// 
// CrystalMPQ is licenced under the Microsoft Reciprocal License.
// You should find the licence included with the source of the program,
// or at this URL: http://www.microsoft.com/opensource/licenses.mspx#Ms-RL
#endregion

namespace CrystalMpq.Explorer
{
	partial class MainForm
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.fileNameToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.treeView = new DoubleBufferedTreeView();
			this.file16ImageList = new System.Windows.Forms.ImageList(this.components);
			this.mainToolStrip = new System.Windows.Forms.ToolStrip();
			this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.saveAsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.propertiesToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.wowFileSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectFileSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.file32ImageList = new System.Windows.Forms.ImageList(this.components);
			this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer.ContentPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.mainToolStrip.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.fileContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer
			// 
			this.toolStripContainer.AccessibleDescription = null;
			this.toolStripContainer.AccessibleName = null;
			resources.ApplyResources(this.toolStripContainer, "toolStripContainer");
			// 
			// toolStripContainer.BottomToolStripPanel
			// 
			this.toolStripContainer.BottomToolStripPanel.AccessibleDescription = null;
			this.toolStripContainer.BottomToolStripPanel.AccessibleName = null;
			this.toolStripContainer.BottomToolStripPanel.BackgroundImage = null;
			resources.ApplyResources(this.toolStripContainer.BottomToolStripPanel, "toolStripContainer.BottomToolStripPanel");
			this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip);
			this.toolStripContainer.BottomToolStripPanel.Font = null;
			// 
			// toolStripContainer.ContentPanel
			// 
			this.toolStripContainer.ContentPanel.AccessibleDescription = null;
			this.toolStripContainer.ContentPanel.AccessibleName = null;
			resources.ApplyResources(this.toolStripContainer.ContentPanel, "toolStripContainer.ContentPanel");
			this.toolStripContainer.ContentPanel.BackgroundImage = null;
			this.toolStripContainer.ContentPanel.Controls.Add(this.splitContainer);
			this.toolStripContainer.ContentPanel.Font = null;
			this.toolStripContainer.Font = null;
			// 
			// toolStripContainer.LeftToolStripPanel
			// 
			this.toolStripContainer.LeftToolStripPanel.AccessibleDescription = null;
			this.toolStripContainer.LeftToolStripPanel.AccessibleName = null;
			this.toolStripContainer.LeftToolStripPanel.BackgroundImage = null;
			resources.ApplyResources(this.toolStripContainer.LeftToolStripPanel, "toolStripContainer.LeftToolStripPanel");
			this.toolStripContainer.LeftToolStripPanel.Font = null;
			this.toolStripContainer.Name = "toolStripContainer";
			// 
			// toolStripContainer.RightToolStripPanel
			// 
			this.toolStripContainer.RightToolStripPanel.AccessibleDescription = null;
			this.toolStripContainer.RightToolStripPanel.AccessibleName = null;
			this.toolStripContainer.RightToolStripPanel.BackgroundImage = null;
			resources.ApplyResources(this.toolStripContainer.RightToolStripPanel, "toolStripContainer.RightToolStripPanel");
			this.toolStripContainer.RightToolStripPanel.Font = null;
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.AccessibleDescription = null;
			this.toolStripContainer.TopToolStripPanel.AccessibleName = null;
			this.toolStripContainer.TopToolStripPanel.BackgroundImage = null;
			resources.ApplyResources(this.toolStripContainer.TopToolStripPanel, "toolStripContainer.TopToolStripPanel");
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.mainToolStrip);
			this.toolStripContainer.TopToolStripPanel.Font = null;
			// 
			// statusStrip
			// 
			this.statusStrip.AccessibleDescription = null;
			this.statusStrip.AccessibleName = null;
			resources.ApplyResources(this.statusStrip, "statusStrip");
			this.statusStrip.BackgroundImage = null;
			this.statusStrip.Font = null;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNameToolStripStatusLabel});
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			// 
			// fileNameToolStripStatusLabel
			// 
			this.fileNameToolStripStatusLabel.AccessibleDescription = null;
			this.fileNameToolStripStatusLabel.AccessibleName = null;
			resources.ApplyResources(this.fileNameToolStripStatusLabel, "fileNameToolStripStatusLabel");
			this.fileNameToolStripStatusLabel.BackgroundImage = null;
			this.fileNameToolStripStatusLabel.Name = "fileNameToolStripStatusLabel";
			this.fileNameToolStripStatusLabel.Spring = true;
			// 
			// splitContainer
			// 
			this.splitContainer.AccessibleDescription = null;
			this.splitContainer.AccessibleName = null;
			resources.ApplyResources(this.splitContainer, "splitContainer");
			this.splitContainer.BackgroundImage = null;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Font = null;
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.AccessibleDescription = null;
			this.splitContainer.Panel1.AccessibleName = null;
			resources.ApplyResources(this.splitContainer.Panel1, "splitContainer.Panel1");
			this.splitContainer.Panel1.BackgroundImage = null;
			this.splitContainer.Panel1.Controls.Add(this.treeView);
			this.splitContainer.Panel1.Font = null;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.AccessibleDescription = null;
			this.splitContainer.Panel2.AccessibleName = null;
			resources.ApplyResources(this.splitContainer.Panel2, "splitContainer.Panel2");
			this.splitContainer.Panel2.BackgroundImage = null;
			this.splitContainer.Panel2.Font = null;
			// 
			// treeView
			// 
			this.treeView.AccessibleDescription = null;
			this.treeView.AccessibleName = null;
			resources.ApplyResources(this.treeView, "treeView");
			this.treeView.BackgroundImage = null;
			this.treeView.Font = null;
			this.treeView.ImageList = this.file16ImageList;
			this.treeView.Name = "treeView";
			this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseClick);
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			// 
			// file16ImageList
			// 
			this.file16ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			resources.ApplyResources(this.file16ImageList, "file16ImageList");
			this.file16ImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// mainToolStrip
			// 
			this.mainToolStrip.AccessibleDescription = null;
			this.mainToolStrip.AccessibleName = null;
			resources.ApplyResources(this.mainToolStrip, "mainToolStrip");
			this.mainToolStrip.BackgroundImage = null;
			this.mainToolStrip.Font = null;
			this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripButton,
            this.saveAsToolStripButton,
            this.propertiesToolStripButton});
			this.mainToolStrip.Name = "mainToolStrip";
			// 
			// openToolStripButton
			// 
			this.openToolStripButton.AccessibleDescription = null;
			this.openToolStripButton.AccessibleName = null;
			resources.ApplyResources(this.openToolStripButton, "openToolStripButton");
			this.openToolStripButton.BackgroundImage = null;
			this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.openToolStripButton.Image = global::CrystalMpq.Explorer.Properties.Resources.OpenToolbarIcon;
			this.openToolStripButton.Name = "openToolStripButton";
			this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveAsToolStripButton
			// 
			this.saveAsToolStripButton.AccessibleDescription = null;
			this.saveAsToolStripButton.AccessibleName = null;
			resources.ApplyResources(this.saveAsToolStripButton, "saveAsToolStripButton");
			this.saveAsToolStripButton.BackgroundImage = null;
			this.saveAsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveAsToolStripButton.Image = global::CrystalMpq.Explorer.Properties.Resources.SaveToolbarIcon;
			this.saveAsToolStripButton.Name = "saveAsToolStripButton";
			this.saveAsToolStripButton.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// propertiesToolStripButton
			// 
			this.propertiesToolStripButton.AccessibleDescription = null;
			this.propertiesToolStripButton.AccessibleName = null;
			resources.ApplyResources(this.propertiesToolStripButton, "propertiesToolStripButton");
			this.propertiesToolStripButton.BackgroundImage = null;
			this.propertiesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.propertiesToolStripButton.Image = global::CrystalMpq.Explorer.Properties.Resources.PropertiesToolbarIcon;
			this.propertiesToolStripButton.Name = "propertiesToolStripButton";
			this.propertiesToolStripButton.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// menuStrip
			// 
			this.menuStrip.AccessibleDescription = null;
			this.menuStrip.AccessibleName = null;
			resources.ApplyResources(this.menuStrip, "menuStrip");
			this.menuStrip.BackgroundImage = null;
			this.menuStrip.Font = null;
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem});
			this.menuStrip.Name = "menuStrip";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.AccessibleDescription = null;
			this.fileToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
			this.fileToolStripMenuItem.BackgroundImage = null;
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openCollectionToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.propertiesToolStripMenuItem1,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.ShortcutKeyDisplayString = null;
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.AccessibleDescription = null;
			this.openToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
			this.openToolStripMenuItem.BackgroundImage = null;
			this.openToolStripMenuItem.Image = global::CrystalMpq.Explorer.Properties.Resources.OpenToolbarIcon;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// openCollectionToolStripMenuItem
			// 
			this.openCollectionToolStripMenuItem.AccessibleDescription = null;
			this.openCollectionToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.openCollectionToolStripMenuItem, "openCollectionToolStripMenuItem");
			this.openCollectionToolStripMenuItem.BackgroundImage = null;
			this.openCollectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wowFileSystemToolStripMenuItem,
            this.selectFileSystemToolStripMenuItem});
			this.openCollectionToolStripMenuItem.Name = "openCollectionToolStripMenuItem";
			this.openCollectionToolStripMenuItem.ShortcutKeyDisplayString = null;
			// 
			// wowFileSystemToolStripMenuItem
			// 
			this.wowFileSystemToolStripMenuItem.AccessibleDescription = null;
			this.wowFileSystemToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.wowFileSystemToolStripMenuItem, "wowFileSystemToolStripMenuItem");
			this.wowFileSystemToolStripMenuItem.BackgroundImage = null;
			this.wowFileSystemToolStripMenuItem.Name = "wowFileSystemToolStripMenuItem";
			this.wowFileSystemToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.wowFileSystemToolStripMenuItem.Click += new System.EventHandler(this.wowMpqFileSystemToolStripMenuItem_Click);
			// 
			// selectFileSystemToolStripMenuItem
			// 
			this.selectFileSystemToolStripMenuItem.AccessibleDescription = null;
			this.selectFileSystemToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.selectFileSystemToolStripMenuItem, "selectFileSystemToolStripMenuItem");
			this.selectFileSystemToolStripMenuItem.BackgroundImage = null;
			this.selectFileSystemToolStripMenuItem.Name = "selectFileSystemToolStripMenuItem";
			this.selectFileSystemToolStripMenuItem.ShortcutKeyDisplayString = null;
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.AccessibleDescription = null;
			this.saveAsToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
			this.saveAsToolStripMenuItem.BackgroundImage = null;
			this.saveAsToolStripMenuItem.Image = global::CrystalMpq.Explorer.Properties.Resources.SaveToolbarIcon;
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem1
			// 
			this.propertiesToolStripMenuItem1.AccessibleDescription = null;
			this.propertiesToolStripMenuItem1.AccessibleName = null;
			resources.ApplyResources(this.propertiesToolStripMenuItem1, "propertiesToolStripMenuItem1");
			this.propertiesToolStripMenuItem1.BackgroundImage = null;
			this.propertiesToolStripMenuItem1.Image = global::CrystalMpq.Explorer.Properties.Resources.PropertiesToolbarIcon;
			this.propertiesToolStripMenuItem1.Name = "propertiesToolStripMenuItem1";
			this.propertiesToolStripMenuItem1.ShortcutKeyDisplayString = null;
			this.propertiesToolStripMenuItem1.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.AccessibleDescription = null;
			this.toolStripMenuItem1.AccessibleName = null;
			resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.AccessibleDescription = null;
			this.exitToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.BackgroundImage = null;
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.AccessibleDescription = null;
			this.toolsToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
			this.toolsToolStripMenuItem.BackgroundImage = null;
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.ShortcutKeyDisplayString = null;
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.AccessibleDescription = null;
			this.optionsToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
			this.optionsToolStripMenuItem.BackgroundImage = null;
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
			// 
			// fileContextMenuStrip
			// 
			this.fileContextMenuStrip.AccessibleDescription = null;
			this.fileContextMenuStrip.AccessibleName = null;
			resources.ApplyResources(this.fileContextMenuStrip, "fileContextMenuStrip");
			this.fileContextMenuStrip.BackgroundImage = null;
			this.fileContextMenuStrip.Font = null;
			this.fileContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.propertiesToolStripMenuItem});
			this.fileContextMenuStrip.Name = "fileContextMenuStrip";
			this.fileContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.fileContextMenuStrip_Opening);
			// 
			// extractToolStripMenuItem
			// 
			this.extractToolStripMenuItem.AccessibleDescription = null;
			this.extractToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.extractToolStripMenuItem, "extractToolStripMenuItem");
			this.extractToolStripMenuItem.BackgroundImage = null;
			this.extractToolStripMenuItem.Image = global::CrystalMpq.Explorer.Properties.Resources.ExportToolbarIcon;
			this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
			this.extractToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.extractToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.AccessibleDescription = null;
			this.propertiesToolStripMenuItem.AccessibleName = null;
			resources.ApplyResources(this.propertiesToolStripMenuItem, "propertiesToolStripMenuItem");
			this.propertiesToolStripMenuItem.BackgroundImage = null;
			this.propertiesToolStripMenuItem.Image = global::CrystalMpq.Explorer.Properties.Resources.PropertiesToolbarIcon;
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.ShortcutKeyDisplayString = null;
			this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// openFileDialog
			// 
			resources.ApplyResources(this.openFileDialog, "openFileDialog");
			this.openFileDialog.RestoreDirectory = true;
			// 
			// saveFileDialog
			// 
			resources.ApplyResources(this.saveFileDialog, "saveFileDialog");
			this.saveFileDialog.RestoreDirectory = true;
			// 
			// file32ImageList
			// 
			this.file32ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			resources.ApplyResources(this.file32ImageList, "file32ImageList");
			this.file32ImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MainForm
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.toolStripContainer);
			this.Font = null;
			this.Icon = null;
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MainForm";
			this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer.ContentPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.ResumeLayout(false);
			this.mainToolStrip.ResumeLayout(false);
			this.mainToolStrip.PerformLayout();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.fileContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStrip mainToolStrip;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SplitContainer splitContainer;
		private DoubleBufferedTreeView treeView;
		private System.Windows.Forms.ContextMenuStrip fileContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton openToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton saveAsToolStripButton;
		private System.Windows.Forms.ToolStripStatusLabel fileNameToolStripStatusLabel;
		internal System.Windows.Forms.ImageList file32ImageList;
		internal System.Windows.Forms.ImageList file16ImageList;
		private System.Windows.Forms.ToolStripButton propertiesToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem openCollectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem wowFileSystemToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectFileSystemToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
	}
}