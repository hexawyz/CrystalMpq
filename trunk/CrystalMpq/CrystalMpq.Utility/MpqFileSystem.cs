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
	public class MpqFileSystem : IMpqFileSystem
	{
		public sealed class MpqArchiveCollection : Collection<MpqArchive>
		{
			private readonly MpqFileSystem fileSystem;
			private readonly EventHandler<ResolveStreamEventArgs> baseFileResolver;

			internal MpqArchiveCollection(MpqFileSystem fileSystem)
				: base(fileSystem.archiveList)
			{
				this.fileSystem = fileSystem;
				this.baseFileResolver = fileSystem.ResolveBaseFile;
			}

			protected sealed override void InsertItem(int index, MpqArchive item)
			{
				base.InsertItem(index, item);
				item.ResolveBaseFile += baseFileResolver;
			}

			protected sealed override void SetItem(int index, MpqArchive item)
			{
				fileSystem.archiveList[index].ResolveBaseFile -= baseFileResolver;
				base.SetItem(index, item);
				item.ResolveBaseFile += baseFileResolver;
			}

			protected sealed override void RemoveItem(int index)
			{
				fileSystem.archiveList[index].ResolveBaseFile -= baseFileResolver;
				base.RemoveItem(index);
			}

			protected sealed override void ClearItems()
			{
				foreach (var archive in fileSystem.archiveList)
					archive.ResolveBaseFile -= baseFileResolver;
				base.ClearItems();
			}
		}

		private readonly List<MpqArchive> archiveList;

		private readonly MpqArchiveCollection archiveCollection;

		/// <summary>Initializes a new instance of the <see cref="MpqFileSystem"/> class.</summary>
		public MpqFileSystem()
		{
			archiveList = new List<MpqArchive>();
			archiveCollection = new MpqArchiveCollection(this);
		}

		/// <summary>Gets the collection of <see cref="MpqArchive"/>.</summary>
		/// <remarks>Archives should be added to this list for being searched.</remarks>
		/// <value>The archive list.</value>
		public MpqArchiveCollection Archives { get { return archiveCollection; } }
		IList<MpqArchive> IMpqFileSystem.Archives { get { return archiveCollection; } }

		private void ResolveBaseFile(object sender, ResolveStreamEventArgs e)
		{
			var file = sender as MpqFile;

			if (file == null) throw new InvalidOperationException();

			bool archiveFound = false;

			foreach (var archive in archiveList)
			{
				if (!archiveFound)
				{
					if (archive == file.Archive) archiveFound = true;
					continue;
				}

				var foundFile = archive.FindFile(file.Name);

				if (foundFile != null)
				{
					e.Stream = foundFile.Open();
					return;
				}
			}
		}

		public MpqFile[] FindFiles(string filename)
		{
			foreach (var archive in archiveList)
			{
				var files = archive.FindFiles(filename);

				if (files.Length > 0) return files;
			}
			return new MpqFile[0];
		}

		public MpqFile FindFile(string filename)
		{
			foreach (var archive in archiveList)
			{
				var file = archive.FindFile(filename);

				if (file != null) return file;
			}
			return null;
		}

		public MpqFile FindFile(string filename, int lcid)
		{
			foreach (var archive in archiveList)
			{
				var file = archive.FindFile(filename, lcid);

				if (file != null) return file;
			}
			return null;
		}
	}
}
