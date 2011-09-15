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
using System.Collections.ObjectModel;
using System.Text;
using CrystalMpq;

namespace CrystalMpq.Utility
{
	/// <summary>Represents a file system composed of multiple MPQ archives.</summary>
	/// <remarks>When searching a file, the first archives are always searched first.</remarks>
	public sealed class MpqFileSystem
	{
		private List<MpqArchive> archiveList;

		/// <summary>Initializes a new instance of the <see cref="MpqFileSystem"/> class.</summary>
		public MpqFileSystem() { archiveList = new List<MpqArchive>(); }

		/// <summary>Gets the list of archives.</summary>
		/// <remarks>Archives should be added to this list for being searched.</remarks>
		/// <value>The archive list.</value>
		public List<MpqArchive> Archives { get { return archiveList; } }

		public MpqFile[] FindFiles(string filename)
		{
			foreach (MpqArchive archive in archiveList)
			{
				MpqFile[] files = archive.FindFiles(filename);

				if (files.Length > 0)
					return files;
			}
			return new MpqFile[0];
		}

		public MpqFile FindFile(string filename)
		{
			foreach (MpqArchive archive in archiveList)
			{
				MpqFile file = archive.FindFile(filename);

				if (file != null)
					return file;
			}
			return null;
		}

		public MpqFile FindFile(string filename, int lcid)
		{
			foreach (MpqArchive archive in archiveList)
			{
				MpqFile file = archive.FindFile(filename, lcid);

				if (file != null)
					return file;
			}
			return null;
		}
	}
}
