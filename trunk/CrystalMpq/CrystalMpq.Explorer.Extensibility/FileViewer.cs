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
using System.Text;
using System.Windows.Forms;
using CrystalMpq;

namespace CrystalMpq.Explorer.Extensibility
{
	/// <summary>
	/// Base class for implementig a FileViewer plugin
	/// </summary>
	public /*abstract */class FileViewer : UserControl // Using abstract breaks the conceptor :(
	{
		MpqFile file;
		IHost host;
		IPluginSettings pluginSettings;

		// For the conceptor...
		private FileViewer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the FileViewer class
		/// </summary>
		/// <param name="host">The host which will be bound to this instance</param>
		public FileViewer(IHost host) { this.host = host; }

		/// <summary>
		/// Gets the MenuStrip control associated with this FileViewer, if there is one
		/// </summary>
		public virtual MenuStrip Menu { get { return null; } }

		/// <summary>
		/// Gets the ToolStrip control associated with this FileViewer, if there is one
		/// </summary>
		public virtual ToolStrip MainToolStrip { get { return null; } }

		/// <summary>
		/// Gets the StatusStrip control associated with this FileViewer, if there is one
		/// </summary>
		public virtual StatusStrip StatusStrip { get { return null; } }

		/// <summary>
		/// Gets the object that can be used to change the settings of this plugin, or null if there is none
		/// </summary>
		public virtual IPluginSettings Settings
		{
			get
			{
				if (pluginSettings != null)
					return pluginSettings;
				else
					return pluginSettings = CreatePluginSettings();
			}
		}

		/// <summary>
		/// Called when creation of the PluginSettings object is requested
		/// </summary>
		/// <returns>The PluginSettings object to use, or null if there is none</returns>
		protected virtual IPluginSettings CreatePluginSettings() { return null; }

		/// <summary>
		/// Gets or sets the MPQFile object to be viewed in this FileViewer
		/// </summary>
		public MpqFile File
		{
			get { return file; }
			set
			{
				if (value != file)
				{
					file = value;
					OnFileChanged();
				}
			}
		}

		/// <summary>
		/// Gets the associated host
		/// </summary>
		protected IHost Host { get { return host; } }

		/// <summary>
		/// Called when the viewed file has changed
		/// </summary>
		protected virtual void OnFileChanged() { }

		/// <summary>
		/// Called when the settings need to be updated
		/// </summary>
		public virtual void ApplySettings() { }
	}
}
