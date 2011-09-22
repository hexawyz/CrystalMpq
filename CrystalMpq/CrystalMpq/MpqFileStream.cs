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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
#if DEBUG
using System.Diagnostics;
#endif

namespace CrystalMpq
{
	/// <summary>Exposes <see cref="Stream"/> with the data contained in an <see cref="MpqFile"/>.</summary>
	public sealed class MpqFileStream : Stream
	{
		#region Patch Headers

		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct PatchInfoHeader
		{
			public uint HeaderLength;
			public uint Flags;
			public uint PatchLength;
			public fixed byte PatchMD5[16];
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PatchHeader
		{
			public uint Signature;
			public uint PatchLength;
			public uint OriginalFileSize;
			public uint PatchedFileSize;
		}

		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct PatchMD5ChunkData
		{
			public fixed byte OrginialFileMD5[16];
			public fixed byte PatchedFileMD5[16];
		}

		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct PatchBsd0Header
		{
			public ulong Signature;
			public ulong ControlBlockLength;
			public ulong DataBlockLength;
			public ulong PatchedFileSize;
		}

		#endregion

		private MpqFile file;
		private int index, last;
		private long position;
		int currentBlock;
		int readBufferOffset;
		private byte[] blockBuffer;
		private byte[] compressedBuffer;
		private uint[] fileHeader;
		private long offset;
		private uint length;
		private uint seed;

		internal MpqFileStream(MpqFile file, Stream baseStream = null)
		{
			try
			{
				offset = file.Offset;

				// Process the patch information header first, if needed

				PatchInfoHeader? patchInfoHeader;

				if (file.IsPatch)
				{
					if ((baseStream = baseStream ?? file.Archive.ResolveBaseFileInternal(file)) == null) throw new FileNotFoundException("The base file of the patch could not be resolved.");

					patchInfoHeader = ReadPatchInfoHeader(file.Archive, file.Offset);

					offset += patchInfoHeader.Value.HeaderLength;
					length = patchInfoHeader.Value.PatchLength;
				}
				else
				{
					patchInfoHeader = null;
					length = checked((uint)file.Size);
				}

				// Set up the stream the same way for both patches and regular files…

				this.file = file;
				this.index = file.Index;
				this.last = (int)(length % file.Archive.BlockSize);
				this.position = 0;
				this.currentBlock = -1;
				this.readBufferOffset = 0;

				bool singleUnit = (file.Flags & MpqFileFlags.SingleBlock) != 0;

				if (singleUnit)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("Single block files are not fully supported yet...");
#endif
					this.blockBuffer = new byte[length];
					this.compressedBuffer = new byte[file.CompressedSize];
				}
				else
				{
					this.blockBuffer = new byte[file.Archive.BlockSize];
					this.compressedBuffer = new byte[file.Archive.BlockSize];
				}

				if (file.IsEncrypted)
				{
					if (file.Seed == 0) throw new SeedNotFoundException(index);
					else this.seed = file.Seed;

					if ((file.Flags & MpqFileFlags.PositionEncrypted) != 0)
						seed = (seed + (uint)file.Offset) ^ (uint)length;
				}

				if (singleUnit)
				{
					this.fileHeader = new uint[] { 0, (uint)file.CompressedSize };
					this.last = (int)file.Size;
				}
				else if (file.IsCompressed)
					fileHeader = ReadBlockOffsets(file.Archive, seed, offset, (int)((length - 1) / file.Archive.BlockSize + 2));
				else
				{
					fileHeader = new uint[(int)((length + file.Archive.BlockSize - 1) / file.Archive.BlockSize) + 1];
					fileHeader[0] = 0;
					for (int i = 1; i < fileHeader.Length; i++)
					{
						fileHeader[i] = fileHeader[i - 1] + (uint)file.Archive.BlockSize;
						if (fileHeader[i] > length)
							fileHeader[i] = (uint)length;
					}
				}
#if DEBUG
				Debug.Write("Opening MPQ file #" + file.Index);
				Debug.WriteLine((file.FileName != null && file.FileName.Length > 0) ? " \"" + file.FileName + "\"" : string.Empty);
				Debug.Indent();
				Debug.WriteLine("File Size: " + file.Size);
				Debug.WriteLine("Last Block Size: " + last);
				Debug.Unindent();
#if VERBOSE
				Debug.WriteLine("Blocks:");
				foreach (uint block in fileHeader) Debug.WriteLine(" 0x" + block.ToString("X8"));
#endif
#endif
				UpdateBuffer();

				if (patchInfoHeader != null)
				{
					blockBuffer = ApplyPatch(patchInfoHeader.Value, baseStream);
					compressedBuffer = null;
					fileHeader = new uint[] { 0, (uint)blockBuffer.Length };
					position = 0;
					currentBlock = 0;
					readBufferOffset = 0;
					length = (uint)blockBuffer.Length;

					throw new NotSupportedException("Patch files will be supported in a later revision");
				}
			}
			finally { if (baseStream != null) baseStream.Dispose(); }
		}

		private static unsafe PatchInfoHeader ReadPatchInfoHeader(MpqArchive archive, long offset)
		{
			// Always get a buffer big enough, even if the extra bytes are not present…
			// As of now (09/2011), the header should always be 28 bytes long, but this may change in the future…
			var sharedBuffer = Utility.GetSharedBuffer(sizeof(PatchInfoHeader));

			// No buffer should ever be smaller than 28 bytes… right ?
			if (archive.ReadBlock(sharedBuffer, 0, offset, 28) != 28) throw new EndOfStreamException(); // It's weird if we could not read the whole 28 bytes… (At worse, we should have read trash data)

			var patchInfoHeader = new PatchInfoHeader();

			patchInfoHeader.HeaderLength = (uint)sharedBuffer[0] | (uint)sharedBuffer[1] << 8 | (uint)sharedBuffer[2] << 16 | (uint)sharedBuffer[3] << 24;
			patchInfoHeader.Flags = (uint)sharedBuffer[4] | (uint)sharedBuffer[5] << 8 | (uint)sharedBuffer[6] << 16 | (uint)sharedBuffer[7] << 24;
			patchInfoHeader.PatchLength = (uint)sharedBuffer[8] | (uint)sharedBuffer[9] << 8 | (uint)sharedBuffer[10] << 16 | (uint)sharedBuffer[11] << 24;

			// Let's assume the MD5 is not amndatory…
			if (patchInfoHeader.HeaderLength >= 28)
				for (int i = 0; i < 16; i++) patchInfoHeader.PatchMD5[i] = sharedBuffer[12 + i];

			return patchInfoHeader;
		}

		private static unsafe uint[] ReadBlockOffsets(MpqArchive archive, uint hash, long offset, int count)
		{
			int length = count * sizeof(uint);
			var sharedBuffer = Utility.GetSharedBuffer(length);

			if (archive.ReadBlock(sharedBuffer, 0, offset, length) != length) throw new EndOfStreamException();

			var offsets = new uint[count];

			Buffer.BlockCopy(sharedBuffer, 0, offsets, 0, length);

			// If hash is valid, decode the header
			if (hash != 0) unchecked { Encryption.Decrypt(offsets, hash - 1); }

			return offsets;
		}

		private unsafe byte[] ApplyPatch(PatchInfoHeader patchInfoHeader, Stream baseStream)
		{
			PatchHeader patchHeader;

			Read((byte*)&patchHeader, sizeof(PatchHeader));

			if (patchHeader.Signature != 0x48435450 /* 'PTCH' */
				//|| patchHeader.PatchLength != patchInfoHeader.PatchLength
				|| patchHeader.PatchedFileSize != file.Size
				|| baseStream.Length != patchHeader.OriginalFileSize) throw new InvalidDataException();

			// Once the initial tests are passed, we can load the whole patch in memory.
			// This will take a big amount of memory, but will avoid having to unpack the file twice…

			var originalData = new byte[baseStream.Length];
			if (baseStream.Read(originalData, 0, originalData.Length) != originalData.Length) throw new EndOfStreamException();

			var md5 = Utility.SharedMD5;

			var originalHash = md5.ComputeHash(originalData);

			PatchMD5ChunkData md5ChunkData;
			bool hasMD5 = false;
			
			while (true)
			{
				long chunkPosition = Position;
				var chunkHeader = stackalloc uint[2];

				if (Read((byte*)chunkHeader, 8) != 8) throw new EndOfStreamException();

				if (chunkHeader[0] == 0x5F35444D /* 'MD5_' */)
				{
					if (Read((byte*)&md5ChunkData, sizeof(PatchMD5ChunkData)) != sizeof(PatchMD5ChunkData)) throw new EndOfStreamException();

					if (!Utility.CompareData(originalHash, md5ChunkData.OrginialFileMD5)) throw new InvalidDataException("Base file MD5 verification failed.");

					hasMD5 = true;
				}
				else if (chunkHeader[0] == 0x4D524658 /* 'XFRM' */)
				{
					// This may not be a real problem, however, let's not handle this case for now…
					if (chunkPosition + chunkHeader[1] != length) throw new InvalidDataException("The XFRM chunk is not the last one in the file.");

					uint patchType;

					Read((byte*)&patchType, 4);

					uint patchLength = chunkHeader[1] - 12;

					byte[] patchedData;

					if (patchType == 0x59504F43 /* 'COPY' */) patchedData = ApplyCopyPatch(ref patchInfoHeader, ref patchHeader, patchLength, originalData);
					if (patchType == 0x30445342 /* 'BSD0' */) patchedData = ApplyBsd0Patch(ref patchInfoHeader, ref patchHeader, patchLength, originalData);
					else throw new NotSupportedException("Unsupported patch type: '" + Utility.FourCCToString(chunkHeader[0]) + "'");

					if (hasMD5)
					{
						var patchedHash = md5.ComputeHash(patchedData);

						if (!Utility.CompareData(patchedHash, md5ChunkData.PatchedFileMD5)) throw new InvalidDataException("Patched file MD5 verification failed.");
					}

					return patchedData;
				}
				else throw new InvalidDataException("Unknown chunk encountered in patch file: '" + Utility.FourCCToString(chunkHeader[0]) + "'");

				Seek(chunkPosition + chunkHeader[1], SeekOrigin.Begin);
			}
		}

		private byte[] ApplyCopyPatch(ref PatchInfoHeader patchInfoHeader, ref PatchHeader patchHeader, uint patchLength, byte[] originalData)
		{
			if (patchLength != patchHeader.PatchedFileSize) throw new InvalidDataException("Inconsistent file size");

			var patchedData = patchLength == originalData.Length ? originalData : new byte[patchLength];

			if (Read(patchedData, 0, patchedData.Length) != patchedData.Length) throw new EndOfStreamException();

			return patchedData;
		}

		private unsafe byte[] ApplyBsd0Patch(ref PatchInfoHeader patchInfoHeader, ref PatchHeader patchHeader, uint patchLength, byte[] originalData)
		{
			PatchBsd0Header header;

			if (Read((byte*)&header, sizeof(PatchBsd0Header)) != sizeof(PatchBsd0Header)) throw new EndOfStreamException();

			throw new NotImplementedException();
		}

		public sealed override bool CanRead { get { return true; } }
		public sealed override bool CanWrite { get { return false; } }
		public sealed override bool CanSeek { get { return true; } }

		public override long Position
		{
			get { return position; }
			set
			{
				position = (int)value;
				UpdateBuffer();
			}
		}

		public sealed override long Length { get { return length; } }

		public MpqFile File { get { return file; } }

		public sealed override int ReadByte()
		{
			if (position >= length) return -1;

			if (readBufferOffset >= blockBuffer.Length) UpdateBuffer();

			position++;

			return blockBuffer[readBufferOffset++];
		}

		public unsafe sealed override int Read(byte[] buffer, int offset, int count)
		{
			if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");

			fixed (byte* bufferPointer = buffer)
				return Read(bufferPointer + offset, count);
		}

		[CLSCompliant(false)]
		public unsafe int Read(byte* buffer, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException("count");

			if (position < 0) return 0;
			if (position + count > length) count = (int)(length - (uint)position);

			fixed (byte* readBufferPointer = blockBuffer)
			{
				var destinationPointer = buffer;
				var sourcePointer = readBufferPointer + readBufferOffset;

				for (int i = count; i-- != 0; readBufferOffset++, position++)
				{
					if (readBufferOffset >= blockBuffer.Length)
					{
						UpdateBuffer();
						sourcePointer = readBufferPointer + readBufferOffset;
					}
					*destinationPointer++ = *sourcePointer++;
				}
			}

			return count;
		}

		public sealed override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }

		public sealed override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					position = (int)offset;
					break;
				case SeekOrigin.Current:
					position += (int)offset;
					break;
				case SeekOrigin.End:
					position = (int)(Length + offset);
					break;
			}
			UpdateBuffer();
			return position;
		}

		public sealed override void SetLength(long value) { throw new NotSupportedException(); }

		public sealed override void Flush() { }

		public sealed override void Close()
		{
			base.Close();

			blockBuffer = null;
			compressedBuffer = null;
		}

		private void UpdateBuffer()
		{
			if (position < 0 || position >= length) return;

			int newBlock = (int)(position / file.Archive.BlockSize);

			if (currentBlock != newBlock)
			{
				ReadBlock(newBlock);
				currentBlock = newBlock;
			}

			readBufferOffset = (int)(position % file.Archive.BlockSize);
		}

		private unsafe void ReadBlock(int block)
		{
			int length = (int)(fileHeader[block + 1] - fileHeader[block]);
			bool compressed = !(length == file.Archive.BlockSize || (length == last && block == fileHeader.Length - 2));
			var buffer = compressed ? compressedBuffer : blockBuffer;

			file.Archive.ReadBlock(buffer, 0, offset + fileHeader[block], length);

			if (file.IsEncrypted)
			{
				// If last bytes don't fit in an uint, then they won't be encrypted/decrypted
				// Therefore we just leave "length" here as a parameter and bits 0..1 will be cut
				unchecked { Encryption.Decrypt(buffer, seed + (uint)block, length); }
			}
#if DEBUG
			// This is useful for debugging decompression algorithms
			// We clear buffer with 0 to see what happens
			if (compressed)
			{
				for (int i = length; i < compressedBuffer.Length; i++)
					compressedBuffer[i] = 0;
				for (int i = 0; i < blockBuffer.Length; i++)
					blockBuffer[i] = 0;
			}
#endif
			if (compressed)
			{
				// Check the advanced compression scheme first, as it is the only used in modern games.
				if ((file.Flags & MpqFileFlags.MultiCompressed) != 0)
					Compression.DecompressBlock(compressedBuffer, length, blockBuffer, true);
				else if ((file.Flags & MpqFileFlags.DclCompressed) != 0)
					Compression.DecompressBlock(compressedBuffer, length, blockBuffer, false);
			}
		}
	}
}
