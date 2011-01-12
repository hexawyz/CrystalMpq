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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace CrystalMpq
{
	/// <summary>
	/// This class is used to read MPQ archives.
	/// It gives you access to all files contained in the archive.
	/// </summary>
	public sealed class MpqArchive
	{
		#region MPQFileCollection Class

		/// <summary>
		/// Represents a collection of MPQFile in a MPQArchive
		/// </summary>
		public class MPQFileCollection : IEnumerable<MpqFile>
		{
			private MpqArchive owner;

			internal MPQFileCollection(MpqArchive owner)
			{
				if (owner == null)
					throw new ArgumentNullException("owner");
				this.owner = owner;
			}

			/// <summary>
			/// Gets a file from the collection
			/// </summary>
			/// <param name="index">Index of the desired MPQFile item</param>
			/// <returns>Returns the MPQFile at the specified index</returns>
			public MpqFile this[int index]
			{
				get
				{
					if (index < 0 || index > owner.files.Length)
						throw new ArgumentOutOfRangeException("index");
					return owner.files[index];
				}
			}

			/// <summary>
			/// Gets the number of MPQFile items in the collection
			/// </summary>
			public long Count
			{
				get
				{
					return owner.files.Length;
				}
			}

			#region IEnumerable Implementation

			IEnumerator IEnumerable.GetEnumerator()
			{
				return owner.files.GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns>Returns an enumerator for the current collection</returns>
			public IEnumerator<MpqFile> GetEnumerator()
			{
				for (int i = 0; i < owner.files.Length; i++)
					yield return owner.files[i];
			}

			#endregion
		}

		#endregion

		#region Static fields
		private static uint hashTableHash;
		private static uint blockTableHash;
		private const uint mpqSignature = 0x1A51504D;
		
		static MpqArchive()
		{
			hashTableHash = Encryption.Hash("(hash table)", 0x300);
			blockTableHash = Encryption.Hash("(block table)", 0x300);
		}
		#endregion

		private string filename;
		private BinaryReader reader;
		private long archiveOffset;
		private long archiveSize;
		private int headerSize;
		private int blockSize;
		private MpqFormat archiveFormat;
		private MpqHashTable hashTable;
		private MpqFile[] files;
		private MPQFileCollection fileCollection;
		private MpqFile listFile;
		private bool listFileParsed;
		private object syncRoot;

		#region Instance Constructors

		private MpqArchive()
		{
			syncRoot = new object();
			fileCollection = new MPQFileCollection(this);
			listFileParsed = false;
		}

		/// <summary>
		/// Initialize a new instance of the class MPQArchive
		/// </summary>
		/// <remarks>
		/// This constructor will by default parse the list file
		/// </remarks>
		/// <param name="filename">MPQ Archive filename</param>
		public MpqArchive(string filename)
			: this(filename, true)
		{
		}

		/// <summary>
		/// Initialize a new instance of the class MPQArchive
		/// </summary>
		/// <param name="filename">MPQ Archive filename</param>
		/// <param name="parseListFile">Determines if listfile will be parsed automatically when found</param>
		public MpqArchive(string filename, bool parseListFile)
			: this()
		{
			OpenInternal(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read), parseListFile);
			this.filename = filename;
		}

		/// <summary>
		/// Initialize a new instance of the class MPQArchive
		/// </summary>
		/// <remarks>
		/// This constructor will by default parse the list file
		/// </remarks>
		/// <param name="stream">Stream containing MPQ Archive</param>
		public MpqArchive(Stream stream)
			: this(stream, true)
		{
		}

		/// <summary>
		/// Initialize a new instance of the class MPQArchive
		/// </summary>
		/// <param name="stream">Stream containing MPQ Archive</param>
		/// <param name="parseListFile">Determines if listfile will be parsed automatically when found</param>
		public MpqArchive(Stream stream, bool parseListFile)
			: this()
		{
			OpenInternal(stream, parseListFile);
			this.filename = "";
		}

		#endregion

		private void OpenInternal(Stream stream, bool parseListFile)
		{
			// MPQ offsets actually use no more than 48 bits, then we don't need to use UInt64
			long hashTableOffset, blockTableOffset, extendedBlockTableOffset;
			int hashTableSize, blockTableSize;
			uint[] buffer;

			archiveOffset = stream.Position;
			reader = new BinaryReader(stream);
			if (reader.ReadUInt32() != mpqSignature)
				throw new ArchiveInvalidException();
			headerSize = reader.ReadInt32();
			archiveSize = reader.ReadUInt32();
			// MPQ format detection
			switch (reader.ReadUInt16()) // Read MPQ format
			{
				case 0: // Original MPQ format
					archiveFormat = MpqFormat.Original;
					if (headerSize != 0x20)
						throw new Exception();
					break;
				case 1: // Extended MPQ format
					archiveFormat = MpqFormat.Extended;
					if (headerSize != 0x2C)
						throw new Exception();
					break;
				default:
					throw new Exception();
			}
			blockSize = 0x200 << reader.ReadUInt16(); // Calculate block size
			hashTableOffset = reader.ReadUInt32(); // Get Hash Table Offset
			blockTableOffset = reader.ReadUInt32(); // Get Block Table Offset
			hashTableSize = reader.ReadInt32(); // Get Hash Table Size
			blockTableSize = reader.ReadInt32(); // Get Block Table Size
			// Process additional values for "Burning Crusade" MPQ format
			if (archiveFormat == MpqFormat.Extended)
			{
				ushort hashTableOffsetHigh, blockTableOffsetHigh;

				// Read extended information
				extendedBlockTableOffset = reader.ReadInt64();
				hashTableOffsetHigh = reader.ReadUInt16();
				blockTableOffsetHigh = reader.ReadUInt16();
				// Modify offsets accordingly
				hashTableOffset |= (long)hashTableOffsetHigh << 32;
				blockTableOffset |= (long)blockTableOffsetHigh << 32;
			}
			if (!CheckOffset((uint)headerSize)
				|| !CheckOffset(hashTableOffset) || !CheckOffset(blockTableOffset)
				|| hashTableSize < 0 || blockTableSize < 0 || hashTableSize < blockTableSize)
				throw new ArchiveCorruptException();
			// Read Tables
			buffer = new uint[4 * Math.Max(hashTableSize, blockTableSize)]; // Initialize read buffer
			hashTable = new MpqHashTable(hashTableSize); // Initialize Hash Table
			reader.BaseStream.Seek(archiveOffset + hashTableOffset, SeekOrigin.Begin);
			Buffer.BlockCopy(reader.ReadBytes(16 * hashTableSize), 0, buffer, 0, 16 * hashTableSize);
			Encryption.Decrypt(buffer, hashTableHash); // Decode Hash Table
			for (int i = 0; i < hashTableSize; i++) // Fill MPQHashTable object
				hashTable.SetEntry(i, buffer[4 * i], buffer[4 * i + 1], (int)buffer[4 * i + 2], (int)buffer[4 * i + 3]);
			if (!hashTable.CheckIntegrity(blockTableSize)) // Check HashTable Integrity (Could be too restrictive, correct if needed)
				throw new ArchiveCorruptException();
			reader.BaseStream.Seek(archiveOffset + blockTableOffset, SeekOrigin.Begin);
			Buffer.BlockCopy(reader.ReadBytes(16 * blockTableSize), 0, buffer, 0, 16 * blockTableSize); 
			Encryption.Decrypt(buffer, blockTableHash); // Decrypt Block Table
			files = new MpqFile[blockTableSize]; // Initialize array
			for (int i = 0; i < blockTableSize; i++) // Fill array
				files[i] = new MpqFile(this, i, buffer[4 * i], buffer[4 * i + 1], buffer[4 * i + 2], buffer[4 * i + 3]);
			foreach (MpqHashTable.HashEntry entry in hashTable) // Bind hash table entries to block table entries
				if (entry.Valid && entry.Block >= 0 && entry.Block < blockTableSize)
					files[entry.Block].BindHashTableEntry(entry);
			TryFilename("(listfile)");
			listFile = FindFile("(listfile)", 0);
			if (listFile == null)
				return;
			if (parseListFile)
				ParseListFile();
		}

		private bool CheckOffset(long offset)
		{
			if (offset < archiveSize)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Gets a value that indicate wether the current archive possesses a listfile.
		/// </summary>
		/// <remarks>
		/// Having a listfile is not required for an archive to be readable.
		/// But you have to know the filenames if you want to read the files inside.
		/// </remarks>
		public bool HasListFile
		{
			get
			{
				return listFile != null;
			}
		}

		/// <summary>
		/// Instructs the MPQArchive object to parse the list file associated with the archive if it had not already been done.
		/// </summary>
		/// <remarks>
		/// Once the list file has been parsed, calls this function will just do nothing.
		/// By default the list file will always be parsed, but you can override this behavior using some of the constructors.
		/// Please note that parsing the list file can take some time, and is not required if you already know the filenames.
		/// </remarks>
		public void ParseListFile()
		{
			Stream listFileStream;
			StreamReader listFileReader;
			string line;

			if (!listFileParsed)
			{
				listFileStream = listFile.Open();
				listFileReader = new StreamReader(listFileStream);
				while ((line = listFileReader.ReadLine()) != null)
				{
					//System.Diagnostics.Debug.WriteLine(line);
					TryFilenameInternal(line);
				}
				listFileReader.Close();
				listFileParsed = true;
			}
		}

		private void TryFilenameInternal(string filename)
		{
			int[] blocks = hashTable.FindMulti(filename);
			
			for (int i = 0; i < blocks.Length; i++)
				files[blocks[i]].AssignFileName(filename, true);
#if DEBUG
			if (blocks.Length == 0)
				System.Diagnostics.Debug.WriteLine("File \"" + filename + "\" not found in archive.");
#endif
		}

		/// <summary>
		/// Instructs the MPQArchive object to associate the given filename with files in the archive.
		/// If there are files who respond to this filename, it will be associated to them.
		/// Otherwise, nothing will happen.
		/// </summary>
		/// <remarks>
		/// This function may be useful when you don't have a listfile for a given MPQ archive or when you just want to find some hidden file.
		/// </remarks>
		/// <param name="filename">The filename you want to try</param>
		public void TryFilename(string filename)
		{
			int[] blocks = hashTable.FindMulti(filename);
			
			for (int i = 0; i < blocks.Length; i++)
				files[blocks[i]].AssignFileName(filename, false);
		}

		/// <summary>
		/// Find files in archive
		/// </summary>
		/// <remarks>
		/// This function will return all MPQFile matching the given filename
		/// There might be more than one MPQFile because of localization
		/// </remarks>
		/// <param name="filename">Filename of the files</param>
		/// <returns>Returns an array of MPQFile, containing 0 or more MPQFile</returns>
		public MpqFile[] FindFiles(string filename)
		{
			int[] blocks = hashTable.FindMulti(filename);
			MpqFile[] files = new MpqFile[blocks.Length];
			
			for (int i = 0; i < blocks.Length; i++)
				files[i] = this.files[blocks[i]];
			return files;
		}

		/// <summary>
		/// Find one file in archive
		/// </summary>
		/// <remarks>
		/// This function will only return the first result found
		/// </remarks>
		/// <param name="filename">Filename of the file to find</param>
		/// <returns>Returns an MPQFile object if file is found, or null otherwise</returns>
		public MpqFile FindFile(string filename)
		{
			int block = hashTable.Find(filename);

			if (block == -1)
				return null;
			else
				return files[block];
		}

		/// <summary>
		/// Find one file in archive, based on lcid
		/// </summary>
		/// <param name="filename">Filename of the file to find</param>
		/// <param name="lcid">LCID of file to find</param>
		/// <returns>Returns an MPQFile object if file is found, or null otherwise</returns>
		public MpqFile FindFile(string filename, int lcid)
		{
			int block;

			block = hashTable.Find(filename, lcid);

			if (block == -1)
				return null;
			else
				return files[block];
		}

		/// <summary>
		/// Sets the preferred culture to use when searching files in the archive.
		/// </summary>
		/// <remarks>
		/// It might happen that a given file exists for different culture in the same MPQ archive, but it is more likely that your MPQ archive is already localized itself...
		/// </remarks>
		/// <param name="lcid">The LCID for the desired culture</param>
		public void SetPreferredCulture(int lcid)
		{
			hashTable.SetPreferredCulture(lcid);
		}

		// To recode and move...
		internal uint FindFileHash(int index)
		{
			MpqFileFlags flags;
			uint[] header, tmp;
			uint hash;

			if (index >= files.Length)
				throw new ArgumentOutOfRangeException("index");
			flags = files[index].Flags;
			if (files[index].Seed != 0)
				return files[index].Seed;
			else if ((flags & MpqFileFlags.Encrypted) != 0) // If file is coded we try to find a valid Hash for the file
			{
				if ((flags & MpqFileFlags.Compressed) != 0) // If file is packed we will use some brute force method
				{
					GetPackedFileHeader(index, 0, out header);
					for (uint i = 0; i < 256; i++) // Check every possible hash
					{
						unchecked { hash = (((uint)header.Length * 4) ^ header[0]) - 0xEEEEEEEE - Encryption.precalc[0x400 + i]; }
						if ((hash & 0xFFU) == i)
						{
							tmp = (uint[])header.Clone();
							Encryption.Decrypt(tmp, hash);
							if (tmp[0] == tmp.Length * 4)
							{
								for (int j = 0; j < tmp.Length - 1; j++)
									if (tmp[j + 1] - tmp[j] > blockSize)
									{
										hash = 0;
										break;
									}
								if (hash != 0)
									return hash;
							}
						}
					}
					return 0;
				}
				else // If file is not packed we could assume it's WAVE or MPQ, but for now we'll just do nothing
				{
					return 0;
				}
			}
			else // If file is not coded, we don't need a valid hash and we can't find one...
				return 0;
		}

		internal void ReadBlock(byte[] buffer, int index, long offset, int length)
		{
			lock (syncRoot) // Allow multithreaded read access
			{
				reader.BaseStream.Seek(archiveOffset + offset, SeekOrigin.Begin);
				reader.Read(buffer, index, length);
			}
		}

		// To remove
		internal void GetPackedFileHeader(int index, uint hash, out uint[] header)
		{
			uint length;
			int blockCount;
			byte[] buffer;

			if (index >= files.Length)
				throw new ArgumentOutOfRangeException("index");
			if ((files[index].Flags & MpqFileFlags.Compressed) == 0)
				throw new Exception("Trying to read a compressed header from an uncompressed file");
			length = (uint)files[index].Size;
			blockCount = (int)((length - 1) / blockSize + 2);
			header = new uint[blockCount];
			lock (syncRoot)
			{
				buffer = new byte[Buffer.ByteLength(header)];
				ReadBlock(buffer, 0, (uint)files[index].Offset, Buffer.ByteLength(header));
				Buffer.BlockCopy(buffer, 0, header, 0, Buffer.ByteLength(header));
			}
			if (hash != 0) // If hash is valid, then we decode the header
				unchecked { Encryption.Decrypt(header, hash - 1); }
		}

		/// <summary>
		/// Gets the size of blocks in the archive.
		/// </summary>
		public int BlockSize
		{
			get
			{
				return blockSize;
			}
		}

		/// <summary>
		/// Gets a collection containing reference to all the files in the archive.
		/// </summary>
		public MPQFileCollection Files
		{
			get
			{
				return fileCollection;
			}
		}

		/// <summary>
		/// Gets the size of the MPQ archive
		/// </summary>
		public long FileSize
		{
			get
			{
				return archiveSize;
			}
		}

		/// <summary>
		/// Gets a flag indicating the format of the archive
		/// </summary>
		public MpqFormat Format
		{
			get
			{
				return archiveFormat;
			}
		}
	}
}

