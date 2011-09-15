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
using System.Threading;

namespace CrystalMpq
{
	/// <summary>
	/// This class represents a file in a MPQArchive
	/// </summary>
	public sealed class MpqFile
	{
		private MpqArchive owner;
		private MpqHashTable.HashEntry hashEntry;
		private string name;
		private long offset;
		uint compressedSize;
		uint uncompressedSize;
		MpqFileFlags flags;
		uint seed;
		private int index;
		private bool listed, open;

		internal MpqFile(MpqArchive owner, int index, long offset, uint compressedSize, uint uncompressedSize, uint flags)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			this.owner = owner;
			this.index = index;
			this.offset = offset;
			this.compressedSize = compressedSize;
			this.uncompressedSize = uncompressedSize;
			this.flags = unchecked((MpqFileFlags)flags);
			this.name = "";
			this.seed = 0;
			this.listed = false;
			this.open = false;
		}

		internal void BindHashTableEntry(MpqHashTable.HashEntry hashEntry) { this.hashEntry = hashEntry; }

		/// <summary>Called internally when the name has been detected.</summary>
		/// <param name="name">Detected filename.</param>
		/// <param name="cache">If set to <c>true</c>, remember the filename.</param>
		/// <param name="listed">If set to <c>true</c>, the name was detected from the listfile.</param>
		/// <remarks>Right now, the method will only update the seed when needed.</remarks>
		internal void OnNameDetected(string name, bool cache = false, bool listed = false)
		{
			if (!string.IsNullOrEmpty(this.name)) return;

			// TODO: Improve the name caching mechanism (Global hash table for MPQ archives ?)
			if (cache || (flags & MpqFileFlags.Encrypted) != 0)
				this.seed = ComputeSeed(name);
			if (cache)
			{
				this.name = name;
				this.listed = listed;
			}
		}

		private static uint ComputeSeed(string filename)
		{
			// Calculate the seed based on the file name and not the full path.
			// I really don't know why but it worked with the full path for a lot of files...
			// But now it's fixed at least
			int index = filename.LastIndexOf('\\');
			return Encryption.Hash(index >= 0 ? filename.Substring(index + 1) : filename, 0x300);
		}

		/// <summary>Gets the archive to whom this file belongs.</summary>
		public MpqArchive Archive { get { return owner; } }

		/// <summary>Gets the filename for this file, or null if the filename is unknown.</summary>
		public string FileName { get { return name; } }

		/// <summary>Gets the offset of this file in the archive.</summary>
		public long Offset { get { return offset; } }

		/// <summary>Gets the size of this file.</summary>
		public long Size { get { return uncompressedSize; } }

		/// <summary>Gets the compressed size of this file.</summary>
		/// <remarks>If the file is not compressed, CompressedSize will return the same value than Size.</remarks>
		public long CompressedSize {get { return compressedSize; } }

		/// <summary>Gets the flags that apply to this file.</summary>
		public MpqFileFlags Flags { get { return flags; } }

		/// <summary>Gets a value indicating whether this file is encrypted.</summary>
		/// <value><c>true</c> if this file is encrypted; otherwise, <c>false</c>.</value>
		public bool IsEncrypted { get { return (flags & MpqFileFlags.Encrypted) != 0; } }

		/// <summary>Gets a value indicating whether this file is compressed.</summary>
		/// <value><c>true</c> if this file is compressed; otherwise, <c>false</c>.</value>
		public bool IsCompressed { get { return (flags & MpqFileFlags.Compressed) != 0; } }

		/// <summary>Gets a value indicating whether this file is a patch.</summary>
		/// <value><c>true</c> if this file is a patch; otherwise, <c>false</c>.</value>
		public bool IsPatch { get { return (flags & MpqFileFlags.Patch) != 0; } }

		/// <summary>Gets the LCID associated with this file.</summary>
		public int LCID { get { return hashEntry.Locale; } }

		/// <summary>Gets the index of the file in the collection.</summary>
		/// <remarks>In the current impelmentation, this index is also the index of the file in the archive's block table.</remarks>
		public int Index { get { return index; } }

		/// <summary>Gets a value indicating wether this file is compressed.</summary>
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

		internal uint Seed { get { return seed; } }

		/// <summary>Gets a value indicating wether the file was found in the list file of the archive.</summary>
		/// <remarks>This can only be true if the list file was parsed.</remarks>
		public bool Listed { get { return listed; } }

		/// <summary>Opens the file for reading.</summary>
		/// <returns>Returns a Stream object which can be used to read data in the file.</returns>
		/// <remarks>Files can only be opened once, so don't forget to close the stream after you've used it.</remarks>
		public MpqFileStream Open()
		{
			// TODO: make thread-safe ?

			if (open) throw new IOException("File is already open.");

			open = true;
			try { return new MpqFileStream(this); }
			catch { open = false; throw; }
		}

		internal void Close()
		{
			if (!open) throw new IOException("Trying to close an unopened file.");
			open = false;
		}
	}
}
