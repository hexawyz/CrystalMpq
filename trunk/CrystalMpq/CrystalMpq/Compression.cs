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
using System.IO.Compression;
#if DEBUG
using System.Diagnostics;
#endif
#if USE_SHARPZIPLIB
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.BZip2;
#endif

namespace CrystalMpq
{
	internal sealed class Compression
	{
#if USE_SHARPZIPLIB
		[ThreadStatic]
		static Inflater inflater; // A global inflater object, made unique for each thread
#endif

		public static int CompressBlock(byte[] inBuffer, byte[] outBuffer, bool multi)
		{
			return 0;
		}

		public static void DecompressBlock(byte[] inBuffer, int inLength, byte[] outBuffer, bool multi)
		{
			byte b;

			if (!multi) // If no multi compression then it's DCL
			{
#if DEBUG && VERBOSE
				Debug.WriteLine("Block is DCL compressed");
#endif
				DclCompression.DecompressBlock(inBuffer, 0, inLength, outBuffer);
			}
			else // Examinate first byte for finding compression methods used
			{
#if DEBUG && VERBOSE
				Debug.WriteLine("Block is multi-compressed");
#endif
				b = inBuffer[0];
				if ((b & 0x24) != 0) // If unknown compresison format used, throw an error
					throw new CompressionNotSupportedException((byte)(b & 0x24));
				if ((b & 0xE5) != 0) // If unimplemented compression algorithm, throw an error
					throw new CompressionNotSupportedException((byte)(b & 0xE5));
				if ((b & 0x10) != 0) // BZip2 Compression
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("BZip2 Compression");
#endif
#if USE_SHARPZIPLIB
					// Use SharpZipLib for decompression
					using (MemoryStream inStream = new MemoryStream(inBuffer, 1, inLength - 1, false, false))
					{
						using (BZip2InputStream outputStream = new BZip2InputStream(inStream))
							outputStream.Read(outBuffer, 0, outBuffer.Length);
					}
#else
					throw new UnsupportedCompressionException((byte)(b & 0x10));
#endif
				}
				if ((b & 0x8) != 0) // DCL Compression (Implode/Explode)
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("DCL Compression (Explode)");
#endif
					DclCompression.DecompressBlock(inBuffer, 1, inLength - 1, outBuffer);
				}
				if ((b & 0x2) != 0) // Zlib Compression (Deflate/Inflate)
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("Zlib Compression (Inflate)");
#endif
#if USE_SHARPZIPLIB
					// We handle the compression with SharpZipLib's Deflate implementation
					if (inflater == null)
						inflater = new Inflater(); // Create a new inflater if it had not been done before
					inflater.Reset();
					inflater.SetInput(inBuffer, 1, inLength - 1);
					inflater.Inflate(outBuffer); // Should theorically be safe
#else
					// We handle the decompression using .NET 2.0 built-in inflate algorithm
					using (MemoryStream inStream = new MemoryStream(inBuffer, 3, inLength - 7, false, false))
					{
						using (DeflateStream deflate = new DeflateStream(inStream, CompressionMode.Decompress))
							deflate.Read(outBuffer, 0, outBuffer.Length);
					}
#endif
				}
				if ((b & 0x1) != 0) // Huffman Compression
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("Huffman Compression");
#endif
				}
				if ((b & 0x80) != 0) // Stereo Wave Compression
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("Stereo Wave Compression - IMA ADPCM");
#endif
				}
				if ((b & 0x40) != 0) // Mono Wave Compression
				{
#if DEBUG && VERBOSE
					Debug.WriteLine("Mono Wave Compression - IMA ADPCM");
#endif
				}
			}
		}

		private Compression()
		{
		}
	}
}
