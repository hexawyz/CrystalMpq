﻿#region Copyright Notice
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
	internal static class DclCompression
	{
		#region Node Structure

		private sealed class Node
		{
			public int Value = -1;
			public Node Child0 = null;
			public Node Child1 = null;

			public Node this[int childIndex]
			{
				get { return childIndex != 0 ? Child1 : Child0; }
				set
				{
					if (childIndex != 0) Child1 = value;
					else Child0 = value;
				}
			}
		}

		#endregion

		#region Initial Data

		private static readonly byte[] offsetCodeLength = new byte[]
		{
			0x02, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06,
			0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
			0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
			0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08
		};
		private static readonly byte[] offsetCode = new byte[]
		{
			0x03, 0x0D, 0x05, 0x19, 0x09, 0x11, 0x01, 0x3E, 0x1E, 0x2E, 0x0E, 0x36, 0x16, 0x26, 0x06, 0x3A,
			0x1A, 0x2A, 0x0A, 0x32, 0x12, 0x22, 0x42, 0x02, 0x7C, 0x3C, 0x5C, 0x1C, 0x6C, 0x2C, 0x4C, 0x0C,
			0x74, 0x34, 0x54, 0x14, 0x64, 0x24, 0x44, 0x04, 0x78, 0x38, 0x58, 0x18, 0x68, 0x28, 0x48, 0x08,
			0xF0, 0x70, 0xB0, 0x30, 0xD0, 0x50, 0x90, 0x10, 0xE0, 0x60, 0xA0, 0x20, 0xC0, 0x40, 0x80, 0x00
		};
		private static readonly short[] lengthBase = new short[]
		{
			0x0000, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0007,
			0x0008, 0x000A, 0x000E, 0x0016, 0x0026, 0x0046, 0x0086, 0x0106
		};
		private static readonly byte[] lengthUpperCodeLength = new byte[]
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
		};
		private static readonly byte[] lengthLowerCodeLength = new byte[]
		{
			0x03, 0x02, 0x03, 0x03, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x07, 0x07
		};
		private static readonly byte[] lengthLowerCode = new byte[]
		{
			0x05, 0x03, 0x01, 0x06, 0x0A, 0x02, 0x0C, 0x14, 0x04, 0x18, 0x08, 0x30, 0x10, 0x20, 0x40, 0x00
		};
		private static readonly byte[] asciiCodeLength = new byte[]
		{
			0x0B, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x08, 0x07, 0x0C, 0x0C, 0x07, 0x0C, 0x0C,
			0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0D, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
			0x04, 0x0A, 0x08, 0x0C, 0x0A, 0x0C, 0x0A, 0x08, 0x07, 0x07, 0x08, 0x09, 0x07, 0x06, 0x07, 0x08,
			0x07, 0x06, 0x07, 0x07, 0x07, 0x07, 0x08, 0x07, 0x07, 0x08, 0x08, 0x0C, 0x0B, 0x07, 0x09, 0x0B,
			0x0C, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x08, 0x08, 0x06, 0x0B, 0x09, 0x06, 0x07, 0x06, 0x06,
			0x07, 0x0B, 0x06, 0x06, 0x06, 0x07, 0x09, 0x08, 0x09, 0x09, 0x0B, 0x08, 0x0B, 0x09, 0x0C, 0x08,
			0x0C, 0x05, 0x06, 0x06, 0x06, 0x05, 0x06, 0x06, 0x06, 0x05, 0x0B, 0x07, 0x05, 0x06, 0x05, 0x05,
			0x06, 0x0A, 0x05, 0x05, 0x05, 0x05, 0x08, 0x07, 0x08, 0x08, 0x0A, 0x0B, 0x0B, 0x0C, 0x0C, 0x0C,
			0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D,
			0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D,
			0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D,
			0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
			0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
			0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C,
			0x0D, 0x0C, 0x0D, 0x0D, 0x0D, 0x0C, 0x0D, 0x0D, 0x0D, 0x0C, 0x0D, 0x0D, 0x0D, 0x0D, 0x0C, 0x0D,
			0x0D, 0x0D, 0x0C, 0x0C, 0x0C, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D
		};
		private static readonly short[] asciiCode = new short[]
		{
			0x0490, 0x0FE0, 0x07E0, 0x0BE0, 0x03E0, 0x0DE0, 0x05E0, 0x09E0,
			0x01E0, 0x00B8, 0x0062, 0x0EE0, 0x06E0, 0x0022, 0x0AE0, 0x02E0,
			0x0CE0, 0x04E0, 0x08E0, 0x00E0, 0x0F60, 0x0760, 0x0B60, 0x0360,
			0x0D60, 0x0560, 0x1240, 0x0960, 0x0160, 0x0E60, 0x0660, 0x0A60,
			0x000F, 0x0250, 0x0038, 0x0260, 0x0050, 0x0C60, 0x0390, 0x00D8,
			0x0042, 0x0002, 0x0058, 0x01B0, 0x007C, 0x0029, 0x003C, 0x0098,
			0x005C, 0x0009, 0x001C, 0x006C, 0x002C, 0x004C, 0x0018, 0x000C,
			0x0074, 0x00E8, 0x0068, 0x0460, 0x0090, 0x0034, 0x00B0, 0x0710,
			0x0860, 0x0031, 0x0054, 0x0011, 0x0021, 0x0017, 0x0014, 0x00A8,
			0x0028, 0x0001, 0x0310, 0x0130, 0x003E, 0x0064, 0x001E, 0x002E,
			0x0024, 0x0510, 0x000E, 0x0036, 0x0016, 0x0044, 0x0030, 0x00C8,
			0x01D0, 0x00D0, 0x0110, 0x0048, 0x0610, 0x0150, 0x0060, 0x0088,
			0x0FA0, 0x0007, 0x0026, 0x0006, 0x003A, 0x001B, 0x001A, 0x002A,
			0x000A, 0x000B, 0x0210, 0x0004, 0x0013, 0x0032, 0x0003, 0x001D,
			0x0012, 0x0190, 0x000D, 0x0015, 0x0005, 0x0019, 0x0008, 0x0078,
			0x00F0, 0x0070, 0x0290, 0x0410, 0x0010, 0x07A0, 0x0BA0, 0x03A0,
			0x0240, 0x1C40, 0x0C40, 0x1440, 0x0440, 0x1840, 0x0840, 0x1040,
			0x0040, 0x1F80, 0x0F80, 0x1780, 0x0780, 0x1B80, 0x0B80, 0x1380,
			0x0380, 0x1D80, 0x0D80, 0x1580, 0x0580, 0x1980, 0x0980, 0x1180,
			0x0180, 0x1E80, 0x0E80, 0x1680, 0x0680, 0x1A80, 0x0A80, 0x1280,
			0x0280, 0x1C80, 0x0C80, 0x1480, 0x0480, 0x1880, 0x0880, 0x1080,
			0x0080, 0x1F00, 0x0F00, 0x1700, 0x0700, 0x1B00, 0x0B00, 0x1300,
			0x0DA0, 0x05A0, 0x09A0, 0x01A0, 0x0EA0, 0x06A0, 0x0AA0, 0x02A0,
			0x0CA0, 0x04A0, 0x08A0, 0x00A0, 0x0F20, 0x0720, 0x0B20, 0x0320,
			0x0D20, 0x0520, 0x0920, 0x0120, 0x0E20, 0x0620, 0x0A20, 0x0220,
			0x0C20, 0x0420, 0x0820, 0x0020, 0x0FC0, 0x07C0, 0x0BC0, 0x03C0,
			0x0DC0, 0x05C0, 0x09C0, 0x01C0, 0x0EC0, 0x06C0, 0x0AC0, 0x02C0,
			0x0CC0, 0x04C0, 0x08C0, 0x00C0, 0x0F40, 0x0740, 0x0B40, 0x0340,
			0x0300, 0x0D40, 0x1D00, 0x0D00, 0x1500, 0x0540, 0x0500, 0x1900,
			0x0900, 0x0940, 0x1100, 0x0100, 0x1E00, 0x0E00, 0x0140, 0x1600,
			0x0600, 0x1A00, 0x0E40, 0x0640, 0x0A40, 0x0A00, 0x1200, 0x0200,
			0x1C00, 0x0C00, 0x1400, 0x0400, 0x1800, 0x0800, 0x1000, 0x0000  
		};

		#endregion

		private static readonly Node asciiTree = BuildAsciiTree();
		private static readonly Node lengthTree = BuildLengthTree();
		private static readonly Node offsetTree = BuildOffsetTree();

		#region Data Initialization

		private static Node BuildAsciiTree()
		{
			// Creates the ascii tree
			var asciiTree = new Node();

			for (int i = 0; i < 256; i++)
			{
				var node = asciiTree;

				int length = asciiCodeLength[i];
				int value = asciiCode[i];

				while (length-- > 0)
				{
					int bit = value & 0x1;

					value >>= 1;

					if (bit != 0)
					{
						if (node.Child1 == null) node.Child1 = new Node();
						node = node.Child1;
					}
					else
					{
						if (node.Child0 == null) node.Child0 = new Node();
						node = node.Child0;
					}
					if (length == 0) node.Value = i;
				}
			}

			return asciiTree;
		}

		private static Node BuildLengthTree()
		{
			// Create the length tree
			var lengthTree = new Node();

			for (int i = 0; i < 518; i++)
			{
				var node = lengthTree;

				int j = 15;
				while (i < lengthBase[j]) j--; // Should not cause an infinite loop

				int length = lengthLowerCodeLength[j];
				int value = ((i - lengthBase[j]) << length) | lengthLowerCode[j];
				length += lengthUpperCodeLength[j];

				while (length-- > 0)
				{
					int bit = value & 0x1;

					value >>= 1;

					if (bit != 0)
					{
						if (node.Child1 == null) node.Child1 = new Node();
						node = node.Child1;
					}
					else
					{
						if (node.Child0 == null) node.Child0 = new Node();
						node = node.Child0;
					}
					if (length == 0) node.Value = i + 2;
				}
			}

			return lengthTree;
		}

		private static Node BuildOffsetTree()
		{
			// Create the offset tree
			var offsetTree = new Node();

			for (int i = 0; i < 64; i++)
			{
				var node = offsetTree;

				int length = offsetCodeLength[i];
				int value = offsetCode[i];

				while (length-- > 0)
				{
					int bit = value & 0x1;

					value >>= 1;

					if (bit != 0)
					{
						if (node.Child1 == null) node.Child1 = new Node();
						node = node.Child1;
					}
					else
					{
						if (node.Child0 == null) node.Child0 = new Node();
						node = node.Child0;
					}
					if (length == 0) node.Value = i;
				}
			}

			return offsetTree;
		}

		#endregion

		public static int CompressBlock(byte[] inBuffer, byte[] outBuffer)
		{
			return 0;
		}

		// 'explode' decompression
		public static int DecompressBlock(byte[] inBuffer, int index, int count, byte[] outBuffer)
		{
			bool ascii;
			byte b;

			b = inBuffer[index++];

			// Check the ASCII encoding flag
			if (b == 0) ascii = false; // Don't use ASCII encoding
			else if (b == 1) ascii = true; // Use ASCII encoding
			else throw new InvalidDataException();

			b = inBuffer[index++];
			if (b < 4 || b > 6) throw new InvalidDataException();

			int dictSize = 0x40 << b; // Calculate dictionnary size
			int lowOffsetSize = b;

			var bitBuffer = new BitBuffer(inBuffer, index, count - 2);

			try
			{
				int i = 0;

				while (i < outBuffer.Length && !bitBuffer.Eof)
				{
					int t = bitBuffer.GetBit();

					if (t == 0) // Litteral
					{
						// Depending on the compression mode, this can either be a raw byte or a coded ASCII character
						t = ascii ? DecodeValue(ref bitBuffer, asciiTree) : bitBuffer.GetByte();

						outBuffer[i++] = (byte)t;
					}
					else // Length/Offset Pair
					{
						// Get the length
						int length = DecodeValue(ref bitBuffer, lengthTree);
						if (length == 519) break; // Length 519 means end of stream

						// Get the offset
						int offsetHigh = DecodeValue(ref bitBuffer, offsetTree);
						int offset = length == 2 ?
							i - ((offsetHigh << 2) | bitBuffer.GetBits(2)) - 1 :
							i - ((offsetHigh << lowOffsetSize) | bitBuffer.GetBits(lowOffsetSize)) - 1;

						if (offset < 0) throw new InvalidDataException();

						// Copy
						while (length-- != 0) outBuffer[i++] = outBuffer[offset++];
					}
				}

				return i;
			}
			finally { bitBuffer.Dispose(); }
		}

		private static int DecodeValue(ref BitBuffer bitBuffer, Node node)
		{
			// This cannot cause an infinite loop if the tables are correct.
			while (node.Value == -1) node = node[bitBuffer.GetBit()];

			return node.Value;
		}
	}
}
