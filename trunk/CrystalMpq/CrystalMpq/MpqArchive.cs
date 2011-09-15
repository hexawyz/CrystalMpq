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
using System.Diagnostics;
using System.Globalization;

namespace CrystalMpq
{
	/// <summary>
	/// This class is used to read MPQ archives.
	/// It gives you access to all files contained in the archive.
	/// </summary>
	public sealed class MpqArchive
	{
		#region MpqFileCollection Class

		/// <summary>Represents a collection of <see cref="MpqFile"/> in an <see cref="MpqArchive"/>.</summary>
		public class MpqFileCollection : IEnumerable<MpqFile>
		{
			private MpqArchive owner;

			internal MpqFileCollection(MpqArchive owner)
			{
				if (owner == null)
					throw new ArgumentNullException("owner");
				this.owner = owner;
			}

			/// <summary>Gets a file from the collection.</summary>
			/// <param name="index">Index of the desired <see cref="MpqFile"/> item.</param>
			/// <returns>Returns the <see cref="MpqFile"/> at the specified index.</returns>
			public MpqFile this[int index]
			{
				get
				{
					if (index < 0 || index > owner.files.Length)
						throw new ArgumentOutOfRangeException("index");
					return owner.files[index];
				}
			}

			/// <summary>Gets the number of <see cref="MpqFile"/> items in the collection.</summary>
			public long Count { get { return owner.files.Length; } }

			#region IEnumerable Implementation

			IEnumerator IEnumerable.GetEnumerator() { return owner.files.GetEnumerator(); }

			/// <summary>Gets an enumerator for the collection.</summary>
			/// <returns>Returns an enumerator for the current collection.</returns>
			public IEnumerator<MpqFile> GetEnumerator()
			{
				for (int i = 0; i < owner.files.Length; i++)
					yield return owner.files[i];
			}

			#endregion
		}

		#endregion

		#region Static Fields

		internal static readonly uint HashTableHash = Encryption.Hash("(hash table)", 0x300);
		internal static readonly uint BlockTableHash = Encryption.Hash("(block table)", 0x300);
		internal const uint MpqSignature = 0x1A51504D;
		internal const uint MpqUserDataSignature = 0x1B51504D;

		#endregion

		private string filename;
		private BinaryReader reader;
		private long archiveOffset;
		private long archiveSize;
		private long headerSize;
		private int blockSize;
		private MpqFormat archiveFormat;
		private MpqHashTable hashTable;
		private MpqFile[] files;
		private MpqFileCollection fileCollection;
		private MpqFile listFile;
		private bool listFileParsed;
		private readonly ResolveStreamEventArgs resolveStreamEventArgs;
		private readonly object syncRoot;

		/// <summary>Occurs when the base file for a given <see cref="MpqFile"/> should be resolved.</summary>
		/// <remarks>
		/// This event will be raised when opening an <see cref="MpqFile"/> which is a patch file.
		/// Because patch files should be applied to a base file, it is needed to access to this file.
		/// The application is responsible for providing a stream containing valid data for this to work.
		/// </remarks>
		public event EventHandler<ResolveStreamEventArgs> ResolveBaseFile;

		#region Instance Constructors

		private MpqArchive()
		{
			syncRoot = new object();
			resolveStreamEventArgs = new ResolveStreamEventArgs();
			fileCollection = new MpqFileCollection(this);
			listFileParsed = false;
		}

		/// <summary>Initializes a new instance of the <see cref="MpqArchive"/> class.</summary>
		/// <remarks>The listfile will be parsed if present.</remarks>
		/// <param name="filename">The MPQ archive's filename.</param>
		public MpqArchive(string filename)
			: this(filename, true) { }

		/// <summary>Initializes a new instance of the <see cref="MpqArchive"/> class.</summary>
		/// <param name="filename">The MPQ archive's filename.</param>
		/// <param name="parseListFile">Determines if the listfile will be parsed.</param>
		public MpqArchive(string filename, bool parseListFile)
			: this()
		{
			OpenInternal(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read), parseListFile);
			this.filename = filename;
		}

		/// <summary>Initializes a new instance of the <see cref="MpqArchive"/> class.</summary>
		/// <remarks>The listfile will be parsed if present.</remarks>
		/// <param name="stream">A <see cref="Stream"/> containing the MPQ archive.</param>
		public MpqArchive(Stream stream)
			: this(stream, true) { }

		/// <summary>Initializes a new instance of the <see cref="MpqArchive"/> class.</summary>
		/// <param name="stream">A <see cref="Stream"/> containing the MPQ archive.</param>
		/// <param name="parseListFile">Determines if the listfile will be parsed.</param>
		public MpqArchive(Stream stream, bool parseListFile)
			: this()
		{
			OpenInternal(stream, parseListFile);
			var fileStream = stream as FileStream;
			this.filename = fileStream != null ? fileStream.Name : "";
		}

		#endregion

		private void OpenInternal(Stream stream, bool parseListFile)
		{
			// MPQ offsets can be 32 bits, 48 bits or 64 bits depending on the MPQ version used…
			long hashTableOffset, hashTableCompressedSize;
			long blockTableOffset, blockTableCompressedSize;
			long extendedBlockTableOffset, extendedBlockTableCompressedSize;
			long enhancedHashTableOffset, enhancedHashTableCompressedSize;
			long enhancedBlockTableOffset, enhancedBlockTableCompressedSize;
			int hashTableSize, blockTableSize;
			uint rawChunkSize;

			// We use a lot of "long" and "int" variables here, but data is likely stored as ulong and uint…
			// So better test for overflow… Who knows what might happen in the future… ;)
			// The "safe" pattern is to read as unsigned and cast to signed where oferflow is possible.
			checked
			{
				archiveOffset = stream.Position;
				reader = new BinaryReader(stream);
				if (reader.ReadUInt32() != MpqSignature)
					throw new ArchiveInvalidException();
				headerSize = reader.ReadUInt32();
				archiveSize = reader.ReadUInt32();
				// MPQ format detection
				// Unknown MPQ version will raise an error… This seems like a safe idea.
				switch (reader.ReadUInt16()) // Read MPQ format
				{
					case 0: // Original MPQ format
						archiveFormat = MpqFormat.Original;
						if (headerSize < 0x20) throw new ArchiveCorruptException();
						break;
					case 1: // Extended MPQ format (WoW Burning Crusade)
						archiveFormat = MpqFormat.BurningCrusade;
						if (headerSize < 0x2C) throw new ArchiveCorruptException();
						break;
					case 2: // Enhanced MPQ format (Take 1)
						archiveFormat = MpqFormat.CataclysmFirst;
						// Header may not contain any additional field than BC extended MPQ format.
						// However, if additional fields are present, the header should be at least 68 bytes long.
						if (headerSize < 0x2C || (headerSize > 0x2C && headerSize < 0x44))
							throw new ArchiveCorruptException();
						break;
					case 3: // Enhanced MPQ format (Take 2)
						archiveFormat = MpqFormat.CataclysmSecond;
						if (headerSize < 0xD0) throw new ArchiveCorruptException();
						break;
					default:
						throw new InvalidMpqVersionException();
				}
				blockSize = 0x200 << reader.ReadUInt16(); // Calculate block size
				hashTableOffset = reader.ReadUInt32(); // Get Hash Table Offset
				blockTableOffset = reader.ReadUInt32(); // Get Block Table Offset
				hashTableSize = (int)reader.ReadUInt32(); // Get Hash Table Size
				blockTableSize = (int)reader.ReadUInt32(); // Get Block Table Size

				// Assign the compressed size for the various tables.
				// Since compression was non-existant with V1 & V2, we know the compressed size is the uncompressed size.
				// If the compressed size is different as specified in V4, this will be overwritten later.
				hashTableCompressedSize = 4 * sizeof(uint) * hashTableSize;
				blockTableCompressedSize = 4 * sizeof(uint) * blockTableSize;

				// Process additional values for "Burning Crusade" MPQ format
				if (archiveFormat >= MpqFormat.BurningCrusade)
				{
					ushort hashTableOffsetHigh, blockTableOffsetHigh;

					// Read extended information
					extendedBlockTableOffset = (long)reader.ReadUInt64();
					hashTableOffsetHigh = reader.ReadUInt16();
					blockTableOffsetHigh = reader.ReadUInt16();
					// Modify offsets accordingly
					hashTableOffset |= (long)hashTableOffsetHigh << 32;
					blockTableOffset |= (long)blockTableOffsetHigh << 32;

					// Handle MPQ version 3 (Cataclysm First)
					if (archiveFormat >= MpqFormat.CataclysmFirst && headerSize >= 0x44)
					{
						archiveSize = (long)reader.ReadUInt64();
						enhancedBlockTableOffset = (long)reader.ReadUInt64();
						enhancedHashTableOffset = (long)reader.ReadUInt64();

						if (archiveFormat >= MpqFormat.CataclysmSecond)
						{
							hashTableCompressedSize = (long)reader.ReadUInt64();
							blockTableCompressedSize = (long)reader.ReadUInt64();
							extendedBlockTableCompressedSize = (long)reader.ReadUInt64();
							enhancedHashTableCompressedSize = (long)reader.ReadUInt64();
							enhancedBlockTableCompressedSize = (long)reader.ReadUInt64();

							rawChunkSize = reader.ReadUInt32();
						}
						else
						{
							// TODO: Compute the uncompresed size for the new enhanced tables of version 3… (Will have to check how to do that…)
							extendedBlockTableCompressedSize = extendedBlockTableOffset > 0 ? sizeof(ushort) * blockTableSize : 0;
						}
					}
					else
					{
#if DEBUG
						long oldArchiveSize = archiveSize;
#endif
						// Compute 64 bit archive size (Not sure whether this is actually needed, but just in case)
						if (extendedBlockTableOffset > hashTableOffset && extendedBlockTableOffset > blockTableOffset)
							archiveSize = extendedBlockTableOffset + sizeof(ushort) * blockTableSize;
						else if (blockTableOffset > hashTableOffset)
							archiveSize = blockTableOffset + 4 * sizeof(uint) * blockTableSize;
						else
							archiveSize = hashTableOffset + 4 * sizeof(uint) * hashTableSize;
#if DEBUG
						Debug.Assert(oldArchiveSize >= archiveSize);
#endif
					}
				}
				else
				{
					extendedBlockTableOffset = 0;
					extendedBlockTableCompressedSize = 0;
				}

				if (!CheckOffset((uint)headerSize)
					|| !CheckOffset(hashTableOffset) || !CheckOffset(blockTableOffset)
					|| hashTableSize < 0 || blockTableSize < 0 || hashTableSize < blockTableSize)
					throw new ArchiveCorruptException();
			}

			// Read Tables
			var buffer = new byte[4 * sizeof(uint) * Math.Max(hashTableSize, blockTableSize)]; // Shared read buffer

			// Read Hash Table
			hashTable = ReadHashTable(buffer, hashTableSize, hashTableOffset, hashTableCompressedSize);
#if ENFORCE_SAFETY
			if (!hashTable.CheckIntegrity(blockTableSize)) // Check HashTable Integrity (Could be too restrictive, correct if needed)
				throw new ArchiveCorruptException();
#endif

			// Read Block Table
			files = ReadBlockTable(buffer, blockTableSize, blockTableOffset, blockTableCompressedSize);
			foreach (MpqHashTable.HashEntry entry in hashTable) // Bind hash table entries to block table entries
				if (entry.IsValid && entry.Block >= 0 && entry.Block < blockTableSize)
					files[entry.Block].BindHashTableEntry(entry);

			// When possible, find and parse the listfile…
			TryFilename("(listfile)");
			listFile = FindFile("(listfile)", 0);
			if (listFile == null) return;
			if (parseListFile) ParseListFile();
		}

		private bool CheckOffset(long offset) { return offset >= 0 && offset < archiveSize; }

		private MpqHashTable ReadHashTable(byte[] buffer, int tableLength, long offset, long dataLength)
		{
			// Stream.Read only takes an int length for now, and it is unlikely that the hash table will exceed 2GB.
			// But like always, who knows what might happen in the future… Better check for overflow and crash nicely ! ;)
			int dataLength2 = checked((int)dataLength);

			reader.BaseStream.Seek(archiveOffset + offset, SeekOrigin.Begin);
			if (reader.Read(buffer, 0, dataLength2) != dataLength)
				throw new EndOfStreamException();

			var hashTable = MpqHashTable.FromData(buffer, dataLength2, tableLength);

			return hashTable;
		}

		private unsafe MpqFile[] ReadBlockTable(byte[] buffer, int tableLength, long offset, long dataLength)
		{
			// Stream.Read only takes an int length for now, and it is unlikely that the block table will exceed 2GB.
			// But like always, who knows what might happen in the future… Better check for overflow and crash nicely ! ;)
			int dataLength2 = checked((int)dataLength);

			reader.BaseStream.Seek(archiveOffset + offset, SeekOrigin.Begin);
			if (reader.Read(buffer, 0, dataLength2) != dataLength)
				throw new EndOfStreamException();

			fixed (byte* bufferPointer = buffer)
			{
				uint* blockTableDataPointer = (uint*)bufferPointer;

				// One table entry is 4 [u]int…
				Encryption.Decrypt(bufferPointer, MpqArchive.BlockTableHash, 4 * tableLength);

				var files = new MpqFile[tableLength];
				for (int i = 0; i < tableLength; i++)
					files[i] = new MpqFile(this, i, *blockTableDataPointer++, *blockTableDataPointer++, *blockTableDataPointer++, *blockTableDataPointer++);

				return files;
			}
		}

		/// <summary>Gets a value that indicate wether the current archive has a listfile.</summary>
		/// <remarks>
		/// Having a listfile is not required for an archive to be readable.
		/// However, you need to know the filenames if you want to read the files.
		/// </remarks>
		public bool HasListFile { get { return listFile != null; } }

		/// <summary>Parses the listfile if it has not already been done.</summary>
		/// <remarks>
		/// Once the list file has been parsed, calls this function will just do nothing.
		/// The list file will always be parsed by default, but you can override this behavior using an appropriate constructor.
		/// Please note that parsing the list file can take some time, and is not required if you already know the filenames.
		/// Also, this operation is irreversible. Once the filenames are present in memory, the only way to free the memory is to close the archive.
		/// </remarks>
		public void ParseListFile()
		{
			if (listFileParsed) return;

			using (var listFileReader = new StreamReader(listFile.Open()))
			{
				string line;

				while ((line = listFileReader.ReadLine()) != null)
					TryFilename(line, true);
				listFileParsed = true;
			}
		}

		/// <summary>Associate the specified filename with files in the archive.</summary>
		/// <remarks>
		/// The filename will only be associated to matching files. If no file corresponds to the specified filename, nothing will happen.
		/// This function may be useful when you don't have a listfile for a given MPQ archive or when you just want to find some hidden files.
		/// </remarks>
		/// <param name="filename">The filename to associate.</param>
		/// <param name="listed">If set to <c>true</c>, the name was found in the listfile.</param>
		private void TryFilename(string filename, bool listed)
		{
			int[] blocks = hashTable.FindMulti(filename);

#if DEBUG
			if (listed) Debug.Assert(blocks.Length > 0);
#endif

			for (int i = 0; i < blocks.Length; i++)
				files[blocks[i]].OnNameDetected(filename, true, listed);
#if DEBUG
			if (blocks.Length == 0) Debug.WriteLine("File \"" + filename + "\" not found in archive.");
#endif
		}

		/// <summary>Associate the specified filename with files in the archive.</summary>
		/// <remarks>
		/// The filename will only be associated to matching files. If no file corresponds to the specified filename, nothing will happen.
		/// This function may be useful when you don't have a listfile for a given MPQ archive or when you just want to find some hidden files.
		/// </remarks>
		/// <param name="filename">The filename you want to try</param>
		public void TryFilename(string filename) { TryFilename(filename, false); }

		/// <summary>Finds files with the specified filename.</summary>
		/// <remarks>
		/// This function will return all <see cref="MpqFile"/>s matching the given filename.
		/// There might be more than one <see cref="MpqFile"/> returned because of the localization.
		/// </remarks>
		/// <param name="filename">The filename of the files to find.</param>
		/// <returns>Returns an array of <see cref="MpqFile"/>, containing zero or more <see cref="MpqFile"/>.</returns>
		public MpqFile[] FindFiles(string filename)
		{
			int[] blocks = hashTable.FindMulti(filename);
			MpqFile[] files = new MpqFile[blocks.Length];

			for (int i = 0; i < blocks.Length; i++)
				(files[i] = this.files[blocks[i]]).OnNameDetected(filename);
			return files;
		}

		/// <summary>Finds one file the specified filename.</summary>
		/// <remarks>This function will only return the first result found.</remarks>
		/// <param name="filename">The filename of the file to find.</param>
		/// <returns>Returns an <see cref="MpqFile"/> object if file is found, or <c>null</c> otherwise.</returns>
		public MpqFile FindFile(string filename)
		{
			int block = hashTable.Find(filename);

			if (block >= 0)
			{
				var file = files[block];
				
				file.OnNameDetected(filename);
				return file;
			}
			else return null;
		}

		/// <summary>Finds one file the specified filename and LCID.</summary>
		/// <param name="filename">The filename of the file to find.</param>
		/// <param name="lcid">The LCID of file to find.</param>
		/// <returns>Returns an <see cref="MpqFile"/> object if file is found, or <c>null</c> otherwise.</returns>
		public MpqFile FindFile(string filename, int lcid)
		{
			int block = hashTable.Find(filename, lcid);

			if (block >= 0)
			{
				var file = files[block];

				file.OnNameDetected(filename);
				return file;
			}
			else return null;
		}

		/// <summary>Sets the preferred culture to use when searching files in the archive.</summary>
		/// <remarks>It might happen that a given file exists for different culture in the same MPQ archive, but it is more likely that your MPQ archive is already localized itself…</remarks>
		/// <param name="lcid">The LCID for the desired culture</param>
		public void SetPreferredCulture(int lcid) { hashTable.SetPreferredCulture(lcid); }

		/// <summary>Resolves the data corresponding to the base file of a given patch <see cref="MpqFile"/>.</summary>
		/// <param name="file">The patch file.</param>
		/// <returns>A <see cref="Stream"/> containing the data for the base file if it was found; otherwise <c>null</c>.</returns>
		internal Stream ResolveBaseFileInternal(MpqFile file)
		{
			if (ResolveBaseFile != null)
				lock (resolveStreamEventArgs)
				{
					ResolveBaseFile(file, resolveStreamEventArgs);
					return resolveStreamEventArgs.TransferStreamOwnership();
				}
			else return null;
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

		/// <summary>Gets the size of blocks in the archive.</summary>
		public int BlockSize { get { return blockSize; } }

		/// <summary>Gets a collection containing reference to all the files in the archive.</summary>
		public MpqFileCollection Files { get { return fileCollection; } }

		/// <summary>Gets the size of the MPQ archive.</summary>
		public long FileSize { get { return archiveSize; } }

		/// <summary>Gets a flag indicating the format of the archive.</summary>
		public MpqFormat Format { get { return archiveFormat; } }
	}
}

