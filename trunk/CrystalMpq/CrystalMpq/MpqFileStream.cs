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
#if DEBUG
using System.Diagnostics;
#endif

namespace CrystalMpq
{
	public sealed class MpqFileStream : Stream
	{
		MpqFile file;
		int index, last;
		int position, currentBlock, readBufferOffset;
		byte[] blockBuffer, compressedBuffer;
		uint[] /*decodeBuffer, */fileHeader;
		uint seed;
		bool closed, singleUnit;

		internal MpqFileStream(MpqFile file)
		{
			int blockCount;
			uint length;

			if (file.IsPatch)
				throw new NotSupportedException("Patch files will be supported in a later revision");
			this.file = file;
			this.index = file.Index;
			length = (uint)file.Size;
			this.last = (int)(length % file.Archive.BlockSize);
			this.position = 0;
			this.currentBlock = -1;
			this.readBufferOffset = 0;

			if (this.singleUnit = (file.Flags & MpqFileFlags.SingleBlock) != 0)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("Single block files are not fully supported yet...");
#endif
				this.blockBuffer = new byte[file.Size];
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
				else seed = file.Seed;

				if ((file.Flags & MpqFileFlags.PositionEncrypted) != 0)
					seed = (seed + (uint)file.Offset) ^ (uint)length;
			}
			
			if (this.singleUnit)
			{
				fileHeader = new uint[] { 0, (uint)file.CompressedSize };
				last = (int)file.Size;
			}
			else if (file.IsCompressed)
				file.Archive.GetPackedFileHeader(index, seed, out fileHeader);
			else // Uncompressed files
			{
				blockCount = (int)((length + file.Archive.BlockSize - 1) / file.Archive.BlockSize) + 1;
				fileHeader = new uint[blockCount];
				fileHeader[0] = 0;
				for (int i = 1; i < blockCount; i++)
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
			foreach (uint block in fileHeader)
				Debug.WriteLine(" 0x" + block.ToString("X8"));
#endif
#endif
			UpdateBuffer();
			closed = false;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanWrite { get { return false; } }
		public override bool CanSeek { get { return true; } }

		public override long Position
		{
			get { return position; }
			set
			{
				position = (int)value;
				UpdateBuffer();
			}
		}

		public override long Length { get { return file.Size; } }

		public MpqFile File { get { return file; } }

		public unsafe override int Read(byte[] buffer, int offset, int count)
		{
			if (offset + count > buffer.Length)
				throw new IndexOutOfRangeException();

			fixed (byte* bufferPointer = buffer)
				return Read(bufferPointer, offset, count);
		}

		[CLSCompliant(false)]
		public unsafe int Read(byte* buffer, int offset, int count)
		{
			int i, blockSize;

			if (position < 0) return 0;
			blockSize = file.Archive.BlockSize;
			fixed (byte* pReadBuffer = blockBuffer)
			{
				byte* pOutput = buffer + offset,
					pInput = pReadBuffer + readBufferOffset;

				for (i = 0; i < count; i++, readBufferOffset++, position++)
				{
					if (readBufferOffset >= blockSize)
					{
						UpdateBuffer();
						pInput = pReadBuffer + readBufferOffset;
					}
					if (position >= Length)
						break;
					*pOutput++ = *pInput++;
				}
			}
			return i;
		}

		public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }

		public override long Seek(long offset, SeekOrigin origin)
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

		public override void SetLength(long value) { throw new NotSupportedException(); }

		public override void Flush() { }

		public override void Close()
		{
			base.Close();

			if (!closed)
			{
				file.Close();
				closed = true;
			}
		}

		private void UpdateBuffer()
		{
			if (position < 0 || position >= Length)
				return;
			// Handle single-block files
			if (this.singleUnit)
			{
				if (currentBlock == -1)
				{
					ReadBlock(0);
					currentBlock = 0;
				}
				readBufferOffset = position;
			}
			else // Normal files
			{
				int newBlock = position / file.Archive.BlockSize;

				if (currentBlock != newBlock)
				{
					ReadBlock(newBlock);
					currentBlock = newBlock;
				}
				readBufferOffset = position % file.Archive.BlockSize;
			}
		}

		private unsafe void ReadBlock(int block)
		{
			int length;
			byte[] buffer = null;
			bool compressed;

			if (block >= fileHeader.Length - 1)
				throw new Exception("Invalid block access");
			length = (int)(fileHeader[block + 1] - fileHeader[block]);
			compressed = !(length == file.Archive.BlockSize || (length == last && block == fileHeader.Length - 2));
#if DEBUG && VERBOSE
			Debug.WriteLine("Reading block 0x" + block.ToString("X4") + " (Compressed length: 0x" + length.ToString("X4") + ") of file \"" + file.FileName + "\".");
#endif
			if (!compressed) // If block is not compressed we use the readBuffer directly
				buffer = blockBuffer;
			else
				buffer = compressedBuffer;
			file.Archive.ReadBlock(buffer, 0, file.Offset + fileHeader[block], length);
			if (file.IsEncrypted)
			{
#if DEBUG && VERBOSE
				Debug.WriteLine("Block is encrypted");
#endif
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
			if (!compressed) // If block is not compressed
			{
#if DEBUG && VERBOSE
				Debug.WriteLine("Block is not compressed");
#endif
				return;
			}
			else if ((file.Flags & MpqFileFlags.DclCompressed) != 0)
				Compression.DecompressBlock(compressedBuffer, length, blockBuffer, false);
			else if ((file.Flags & MpqFileFlags.MultiCompressed) != 0)
				Compression.DecompressBlock(compressedBuffer, length, blockBuffer, true);
		}
	}
}
