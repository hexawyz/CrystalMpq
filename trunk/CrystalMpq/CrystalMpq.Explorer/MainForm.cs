#region Copyright Notice
// This file is part of CrystalMPQ.
// 
// Copyright (C) 2007-2011 Fabien BARBIER
// 
// CrystalMPQ is licenced under the Microsoft Reciprocal License.
// You should find the licence included with the source of the program,
// or at this URL: http://www.microsoft.com/opensource/licenses.mspx#Ms-RL
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using CrystalMpq;
using System.IO;
using CrystalMpq.WoWFile;
using CrystalMpq.Explorer.Extensibility;
using CrystalMpq.Explorer.Properties;
using CrystalMpq.Explorer.Viewers;
using CrystalMpq.Utility;
#endregion

namespace CrystalMpq.Explorer
{
	sealed partial class MainForm : Form
	{
		#region PluginHost Class

		private sealed class PluginHost : IHost
		{
			MainForm mainForm;

			public PluginHost(MainForm mainForm) { this.mainForm = mainForm; }

			public string SelectedFileName
			{
				get
				{
					if (mainForm.treeView.SelectedNode != null)
						return mainForm.treeView.SelectedNode.FullPath;
					else
						return null;
				}
			}

			public Color ViewerBackColor { get { return Properties.Settings.Default.ViewerBackColor; } }

			public void StatusMessage(string text)
			{
				mainForm.statusStrip.Text = text;
			}

			public IntPtr Handle { get { return mainForm.Handle; } }
		}

		#endregion

		PluginHost pluginsHost;
		MpqFileSystem fileSystem;
		Dictionary<string, TreeNode> nodeDictionnary;
		List<TreeNode> temporaryNodeList;
		Dictionary<string, FileViewer> fileViewerAssociations, fileViewers;
		FileViewer currentViewer;
		DirectoryViewer directoryViewer;
		NodePropertiesForm nodePropertiesForm;
		LanguagePackDialog languagePackDialog;
		OptionsForm optionsForm;

		public MainForm()
		{
			pluginsHost = new PluginHost(this);
			InitializeComponent();
			Text = Properties.Resources.AppTitle;
			fileSystem = new MpqFileSystem();
			languagePackDialog = new LanguagePackDialog();
			nodePropertiesForm = new NodePropertiesForm(this);
			nodeDictionnary = new Dictionary<string, TreeNode>();
			temporaryNodeList = new List<TreeNode>();
			fileViewers = new Dictionary<string, FileViewer>();
			fileViewerAssociations = new Dictionary<string, FileViewer>();
			LoadEmbeddedViewers();
			LoadPlugins();
			ResolveAssociations();
			LoadIcons();
			ApplySettings();
		}

		#region Settings Management

		List<IPluginSettings> GetSettingsList()
		{
			List<IPluginSettings> settingsList = new List<IPluginSettings>();

			settingsList.Add(new GeneralSettings());

			foreach (FileViewer viewer in fileViewers.Values)
				if (viewer.Settings != null)
					settingsList.Add(viewer.Settings);

			return settingsList;
		}

		void ResetSettings(IList<IPluginSettings> settingsList)
		{
			foreach (IPluginSettings item in settingsList)
				item.Reset();
		}

		void SaveSettings(IList<IPluginSettings> settingsList)
		{
			foreach (IPluginSettings item in settingsList)
				item.Save();
		}

		void ApplySettings()
		{
			foreach (FileViewer viewer in fileViewers.Values)
				viewer.ApplySettings();
		}

		void ShowOptionsForm()
		{
			List<IPluginSettings> settingsList = GetSettingsList();

			if (optionsForm == null)
				optionsForm = new OptionsForm();

			optionsForm.Settings = settingsList;

			ResetSettings(settingsList);

			if (optionsForm.ShowDialog(this) == DialogResult.OK)
			{
				SaveSettings(settingsList);
				ApplySettings();
			}
		}

		#endregion

		#region Plugin Management

		private void LoadEmbeddedViewers()
		{
			AddViewer(directoryViewer = new DirectoryViewer(this, pluginsHost));
			AddViewer(new BitmapViewer(pluginsHost));
			AddViewer(new TextViewer(pluginsHost));
		}

		private void LoadPlugins()
		{
			FileViewer[] viewers = PluginManager.LoadPlugins<FileViewer>(new Type[] { typeof(IHost) }, new object[] { pluginsHost });

			for (int i = 0; i < viewers.Length; i++)
				AddViewer(viewers[i]);
		}

		private void AddViewer(FileViewer fileViewer)
		{
			if (fileViewer != null)
			{
				fileViewer.Dock = DockStyle.Fill;
				fileViewer.Visible = false;
				splitContainer.Panel2.Controls.Add(fileViewer);
				fileViewers.Add(fileViewer.GetType().AssemblyQualifiedName, fileViewer);
			}
		}

		public void ResolveAssociations()
		{
			for (int i = 0; i < Settings.Default.ViewerAssociations.Count; i++)
			{
				string formatLoader = Settings.Default.ViewerAssociations[i];
				string[] parts = formatLoader.Split('|');

				if (parts.Length != 2)
				{
					Settings.Default.ViewerAssociations.RemoveAt(i);
					i--;
				}
				else
				{
					FileViewer fileViewer;

					if (fileViewers.TryGetValue(parts[1], out fileViewer))
					    fileViewerAssociations.Add(parts[0], fileViewer);
				}
			}
		}

		#endregion

		private Icon GetSmallIcon(Icon icon)
		{
			if (icon.Size == new Size(16, 16))
				return icon;
			else
				return new Icon(icon, new Size(16, 16));
		}

		private Icon GetLargeIcon(Icon icon)
		{
			if (icon.Size == new Size(32, 32))
				return icon;
			else
				return new Icon(icon, new Size(32, 32));
		}

		private void AddIcons(Icon baseIcon)
		{
			file16ImageList.Images.Add(GetSmallIcon(baseIcon));
			file32ImageList.Images.Add(GetLargeIcon(baseIcon));
		}

		private void LoadIcons()
		{
			AddIcons(Properties.Resources.UnknownFileIcon);
			AddIcons(Properties.Resources.ClosedFolderIcon);
			AddIcons(Properties.Resources.OpenFolderIcon);
		}

		private void Merge(ToolStrip source, ToolStrip target)
		{
			if (source != null)
				ToolStripManager.Merge(source, target);
		}

		private void RevertMerge(ToolStrip target, ToolStrip source)
		{
			if (source != null)
				ToolStripManager.RevertMerge(target, source);
		}

		private void SetViewer(FileViewer viewer)
		{
			if (viewer == currentViewer)
				return;

			if (currentViewer != null)
			{
				RevertMerge(menuStrip, currentViewer.Menu);
				RevertMerge(mainToolStrip, currentViewer.MainToolStrip);
				RevertMerge(statusStrip, currentViewer.StatusStrip);
				currentViewer.Visible = false;
			}
			currentViewer = viewer;
			if (viewer != null)
			{
				Merge(currentViewer.Menu, menuStrip);
				Merge(currentViewer.MainToolStrip, mainToolStrip);
				Merge(currentViewer.StatusStrip, statusStrip);
				currentViewer.Visible = true;
			}
		}

		private void SetTitle(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				Text = Properties.Resources.AppTitle;
			else
				Text = Properties.Resources.AppTitle + " - " + fileName;
		}

		private void ErrorDialog(string message)
		{
			MessageBox.Show(this, message, Properties.Resources.ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		private void OpenArchive(string fileName)
		{
			ClearView();

			try
			{
#if false
				// Code used to open wow single-file executable patches
				Stream strm = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				strm.Seek(0xF5400, SeekOrigin.Begin);
				archive = new MPQArchive(strm, true);
#else
				fileSystem.Archives.Clear();
				fileSystem.Archives.Add(new MpqArchive(fileName));
#endif
				SetTitle(fileName);
				FillTreeView();
			}
			catch (Exception e)
			{
				ErrorDialog(e.Message);
			}
		}

		private void OpenWoWFileSystem()
		{
			try
			{
				WoWInstallation wowInstallation = WoWInstallation.Find();

				languagePackDialog.WoWInstallation = wowInstallation;

				foreach (LanguagePack languagePack in wowInstallation.LanguagePacks)
					if (languagePack.Culture == System.Globalization.CultureInfo.CurrentCulture)
						languagePackDialog.LanguagePack = languagePack;

				if (wowInstallation.LanguagePacks.Count > 1)
					if (languagePackDialog.ShowDialog(this) != DialogResult.OK)
						return;

				ClearView();
				fileSystem = wowInstallation.CreateFileSystem(languagePackDialog.LanguagePack, false, true);
				SetTitle(wowInstallation.Path);
				FillTreeView();
			}
			catch (Exception e)
			{
				ErrorDialog(e.Message);
			}
		}

		#region Tree View Filling

		private void ClearView()
		{
			SetViewer(null);

			saveAsToolStripMenuItem.Enabled = false;
			saveAsToolStripButton.Enabled = false;
			propertiesToolStripMenuItem1.Enabled = false;
			propertiesToolStripButton.Enabled = false;

			treeView.Nodes.Clear();
			nodeDictionnary.Clear();

			fileNameToolStripStatusLabel.Text = "";

			SetTitle(null);
		}

		private void FillTreeView()
		{
			List<TreeNode> nodeList = new List<TreeNode>();

			foreach (MpqArchive archive in fileSystem.Archives)
			{
				foreach (MpqFile file in archive.Files)
				{
#if EXPERIMENTAL
					if (file.FileName != null && file.FileName.Length > 0 && (file.Flags & MpqFileFlags.Deleted) == 0)
#else
					if (file.FileName != null && file.FileName.Length > 0)
#endif
					{
						string[] parts = file.FileName.Split('\\');
						string assembledPath = "";
						TreeNode currentNode = null;

						for (int i = 0; i < parts.Length; i++)
						{
							string part = parts[i];
							TreeNode newNode;

							if (assembledPath.Length == 0)
								assembledPath = part.ToUpperInvariant();
							else
								// Since MPQ file names are not case-sensitive,
								// we can sort the files case-insensitively
								assembledPath += '\\' + part.ToUpperInvariant();
							if (nodeDictionnary.TryGetValue(assembledPath, out newNode))
							{
								string nodeText = newNode.Text;

								// This code is for detecting case differences between the two names
								if (nodeText[0] != part[0] || nodeText[1] != part[1])
								{
									// If we detect difference, we try to choose the best one, which probably is not the one ALL IN CAPS
									if (nodeText[1] == char.ToUpperInvariant(nodeText[1])) // If second character is capitalized, assume the name is capitalized
										newNode.Text = part;
								}
								currentNode = newNode;
							}
							else
							{
								newNode = new TreeNode(part);
								newNode.ContextMenuStrip = fileContextMenuStrip;

								if (i == parts.Length - 1)
								{
									newNode.Tag = file;
								}
								else
								{
									newNode.ImageIndex = 1;
									newNode.SelectedImageIndex = 2;
									//newNode.Tag = false;
									newNode.Tag = new List<TreeNode>();
								}

								if (currentNode == null)
								{
									nodeList.Add(newNode);
								}
								else
								{
									//currentNode.Nodes.Add(newNode);
									(currentNode.Tag as List<TreeNode>).Add(newNode);
								}

								nodeDictionnary.Add(assembledPath, newNode);
								currentNode = newNode;
							}
						}
					}
				}
			}
			// Sort top-level nodes alphabetically before adding them to the treeview
			SortNodeList(nodeList);
			// Add all the nodes in a single pass, evoiding any bottlenecks caused by repeated Win32 interop
			treeView.Nodes.AddRange(nodeList.ToArray());
			// Garbage collection after this memory-intensive operation
			GC.Collect();
		}

		private static int CompareNodes(TreeNode x, TreeNode y)
		{
			if (x.ImageIndex == 1 && y.ImageIndex != 1) // Case where x is a directory but y isn't
				return -1;
			else if (y.ImageIndex == 1 && x.ImageIndex != 1) // Case where y is a directory but x isn't
				return 1;
			else
				return string.Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		private void SortNodeList(List<TreeNode> nodeList)
		{
			// Sort the root list
			nodeList.Sort(CompareNodes);
			// Sort the sub-lists and build the sub-tree
			foreach (TreeNode treeNode in nodeList)
				if (treeNode.Tag is List<TreeNode>)
				{
					List<TreeNode> subNodeList = (List<TreeNode>)treeNode.Tag;

					treeNode.Tag = null;
					SortNodeList(subNodeList);
					treeNode.Nodes.AddRange(subNodeList.ToArray());
				}
		}

		#endregion

		private void ViewFile(MpqFile file)
		{
		}

		internal void InteractiveExtractFile(MpqFile file)
		{
			string fileName = Path.GetFileName(file.FileName),
					ext = Path.GetExtension(fileName).ToLowerInvariant();

			if (ext == null || ext.Length == 0)
				saveFileDialog.Filter = "";
			else
				saveFileDialog.Filter = ext.ToUpperInvariant() + " Files (*" + ext.ToLowerInvariant() + ")|*" + ext.ToLowerInvariant();
			saveFileDialog.FileName = fileName;
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
				ExtractFile(file, saveFileDialog.FileName);
		}

		internal void ExtractFile(MpqFile file, string fileName)
		{
			Stream inputStream = null, outputStream = null;
			byte[] buffer;

			try
			{
				inputStream = file.Open();
				outputStream = File.OpenWrite(fileName);

				buffer = new byte[4096];

				int length;
				do
				{
					length = inputStream.Read(buffer, 0, 4096);
					outputStream.Write(buffer, 0, length);
				}
				while (length == 4096);
			}
			catch
			{
			}
			finally
			{
				if (inputStream != null)
					inputStream.Close();
				if (outputStream != null)
					outputStream.Close();
			}
		}

		#region File Menu Actions

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				OpenArchive(openFileDialog.FileName);
			}
		}

		private void wowMpqFileSystemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenWoWFileSystem();
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode.Tag is MpqFile)
				InteractiveExtractFile((MpqFile)treeView.SelectedNode.Tag);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Tools Menu Actions

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowOptionsForm();
		}

		#endregion

		#region TreeView Context Menu Actions

		private void fileContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			if (treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
				extractToolStripMenuItem.Enabled = false;
			else
				extractToolStripMenuItem.Enabled = true;
		}

		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode != null)
			{
				nodePropertiesForm.Node = treeView.SelectedNode;
				nodePropertiesForm.ShowDialog(this);
			}
		}

		#endregion

		#region TreeView Actions

		private void treeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeNode node = treeView.HitTest(e.X, e.Y).Node;
				if (node != null)
					treeView.SelectedNode = node;
			}
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			MpqFile file = e.Node.Tag as MpqFile;

			if (file == null)
			{
				directoryViewer.RootNode = e.Node;

				SetViewer(directoryViewer);

				saveAsToolStripMenuItem.Enabled = false;
				saveAsToolStripButton.Enabled = false;

				fileNameToolStripStatusLabel.Text = "";
			}
			else
			{
				string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
				FileViewer fileViewer;

				saveAsToolStripMenuItem.Enabled = true;
				saveAsToolStripButton.Enabled = true;
				if (fileViewerAssociations.TryGetValue(ext, out fileViewer))
				{
					try
					{
						SetViewer(fileViewer);
						fileViewer.File = file;
					}
					catch (Exception ex) { ErrorDialog(ex.Message); }
				}
				else
					SetViewer(null);
				fileNameToolStripStatusLabel.Text = file.FileName;
			}
			propertiesToolStripMenuItem1.Enabled = true;
			propertiesToolStripButton.Enabled = true;
		}

		//private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		//{
		//    TreeNode treeNode = e.Node;

		//    if (treeNode.Tag is bool && (bool)treeNode.Tag == false)
		//    {
		//        if (treeNode.Nodes.Count > 1)
		//        {
		//            temporaryNodeList.Clear();
		//            temporaryNodeList.Capacity = treeNode.Nodes.Count;

		//            foreach (TreeNode node in treeNode.Nodes)
		//                temporaryNodeList.Add(node);

		//            SortNodeList(temporaryNodeList);

		//            //treeView.BeginUpdate();
		//            e.Node.Nodes.Clear();
		//            e.Node.Nodes.AddRange(temporaryNodeList.ToArray());
		//            //treeView.EndUpdate();
		//            //treeView.BeginUpdate();
		//            //for (int i = 0; i < temporaryNodeList.Count; i++)
		//            //    treeNode.Nodes[i] = temporaryNodeList[i];
		//            //treeView.EndUpdate();
		//        }
		//        e.Node.Tag = true;
		//    }
		//}

		#endregion
	}
}