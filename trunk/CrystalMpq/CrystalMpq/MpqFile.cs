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
using System.IO;

namespace CrystalMpq
{
	/// <summary>
	/// This class represents a file in a MPQArchive
	/// </summary>
	public sealed class MpqFile
	{
		private MpqArchive owner;
		private MpqHashTable.HashEntry hashEntry;
		private string fileName;
		private uint offset, compressedSize, uncompressedSize, flags, seed;
		private int index;
		private bool listed, open;

		internal MpqFile(MpqArchive owner, int index, uint offset, uint compressedSize, uint uncompressedSize, uint flags)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			this.owner = owner;
			this.index = index;
			this.offset = offset;
			this.compressedSize = compressedSize;
			this.uncompressedSize = uncompressedSize;
			this.flags = flags;
			this.fileName = "";
			this.seed = 0;
			this.listed = false;
			this.open = false;
		}

		internal void BindHashTableEntry(MpqHashTable.HashEntry hashEntry)
		{
			this.hashEntry = hashEntry;
		}

		internal void AssignFileName(string fileName, bool listed)
		{
			this.fileName = fileName;
			// Calculate the seed based on the file name and not the full path
			// I really don't know why but it worked with the full path for a lot of files...
			// But now it's fixed at least
			int index = fileName.LastIndexOf('\\');
			if (index != -1)
				fileName = fileName.Substring(index + 1);
			this.seed = Encryption.Hash(fileName, 0x300);
			this.listed = listed;
		}

		/// <summary>
		/// Gets the archive to whom this file belongs
		/// </summary>
		public MpqArchive Archive
		{
			get
			{
				return owner;
			}
		}

		/// <summary>
		/// Gets the filename for this file, or null if the filename is unknown.
		/// </summary>
		public string FileName
		{
			get
			{
				return fileName;
			}
		}

		/// <summary>
		/// Gets the offset of this file in the archive
		/// </summary>
		public long Offset
		{
			get
			{
				return offset;
			}
		}

		/// <summary>
		/// Gets the size of this file
		/// </summary>
		public long Size
		{
			get
			{
				return uncompressedSize;
			}
		}

		/// <summary>
		/// Gets the compressed size of this file
		/// </summary>
		/// <remarks>If the file is not compressed, CompressedSize will return the same value than Size</remarks>
		public long CompressedSize
		{
			get
			{
				return compressedSize;
			}
		}

		/// <summary>
		/// Gets the flags that apply to this file
		/// </summary>
		public MpqFileFlags Flags
		{
			get
			{
				return (MpqFileFlags)flags;
			}
		}

		/// <summary>
		/// Gets the LCID associated with this file
		/// </summary>
		public int LCID
		{
			get
			{
				return hashEntry.Locale;
			}
		}

		/// <summary>
		/// Gets the index of the file in the collection
		/// </summary>
		/// <remarks>
		/// In the current impelmentation, this index is also the index of the file in the archive's block table
		/// </remarks>
		public int Index
		{
			get
			{
				return index;
			}
		}

		/// <summary>
		/// Gets a value indicating wether this file is compressed
		/// </summary>
		public bool Compressed
		{
			get
			{
				if (((MpqFileFlags)flags & MpqFileFlags.Compressed) != 0)
					return true;
				else
					return false;
			}
		}

		internal uint Seed
		{
			get
			{
				return seed;
			}
		}

		/// <summary>
		/// Gets a value indicating wether the file was found in the list file of the archive
		/// </summary>
		/// <remarks>
		/// This can only be true if the list file was parsed
		/// </remarks>
		public bool Listed
		{
			get
			{
				return listed;
			}
		}

		/// <summary>
		/// Opens the file for reading
		/// </summary>
		/// <returns>Returns a Stream object which can be used to read data in the file</returns>
		/// <remarks>Files can only be opened once, so don't forget to close the stream after you've used it.</remarks>
		public MpqFileStream Open()
		{
			MpqFileStream stream;
			
			// TODO: make thread-safe ?

			if (open)
				throw new IOException("File is already open");

			stream = new MpqFileStream(this);
			open = true;

			return stream;
		}

		internal void Close()
		{
			if (!open)
				throw new IOException("Trying to close an unopened file");
			open = false;
		}
	}
}
