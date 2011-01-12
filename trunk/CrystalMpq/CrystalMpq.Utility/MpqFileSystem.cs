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
	public sealed class MpqFileSystem
	{
		List<MpqArchive> archiveList;

		public MpqFileSystem()
		{
			archiveList = new List<MpqArchive>();
		}

		public List<MpqArchive> Archives
		{
			get
			{
				return archiveList;
			}
		}

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
