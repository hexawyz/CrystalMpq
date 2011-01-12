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
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;
using Stream = System.IO.Stream;
using CrystalMpq.WoWFile;
using CrystalMpq.Explorer.Extensibility;

namespace CrystalMpq.Explorer.BaseViewers
{
	public sealed partial class BLPViewer : FileViewer
	{
		BLPTexture texture;

		public BLPViewer(IHost host)
			: base(host)
		{
			DoubleBuffered = true;
			InitializeComponent();
			UpdateStatusInformation();
			ApplySettings();
		}

		public override void ApplySettings()
		{
			BackColor = Host.ViewerBackColor;
		}

		public override MenuStrip Menu { get { return menuStrip; } }
		public override ToolStrip MainToolStrip { get { return mainToolStrip; } }
		public override StatusStrip StatusStrip { get { return statusStrip; } }

		public BLPTexture Texture
		{
			get
			{
				return texture;
			}
			set
			{
				if (value != texture)
				{
					texture = value;
					if (texture != null)
					{
						this.BackgroundImage = texture.FirstMipMap;
						exportToolStripMenuItem.Enabled = true;
					}
					else
					{
						this.BackgroundImage = null;
						exportToolStripMenuItem.Enabled = false;
					}
					UpdateStatusInformation();
				}
			}
		}

		protected override void OnFileChanged()
		{
			if (File == null)
			{
				Texture = null;
				return;
			}
			else
			{
				Stream stream;
				BLPTexture texture = null; // Avoid the stupid catch-throw with this mini hack

				stream = File.Open();
				try { texture = new BLPTexture(stream, false); }
				finally { stream.Close(); Texture = texture; }
			}
		}

		private void ShowStatusInformation(bool show)
		{
			sizeToolStripStatusLabel.Visible = show;
		}

		private void UpdateStatusInformation()
		{
			if (texture != null)
			{
				sizeToolStripStatusLabel.Text = string.Format(Properties.Resources.Culture,
					Properties.Resources.SizeFormat,
					texture.FirstMipMap.Width, texture.FirstMipMap.Height);
				ShowStatusInformation(true);
			}
			else
				ShowStatusInformation(false);
		}

		private void exportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				switch (saveFileDialog.FilterIndex)
				{
					case 1:
						texture.FirstMipMap.Save(saveFileDialog.FileName, ImageFormat.Png);
						break;
					case 2:
						texture.FirstMipMap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
						break;
				}
			}
		}
	}
}
