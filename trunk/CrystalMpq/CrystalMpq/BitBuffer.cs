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

namespace CrystalMpq
{
	internal class BitBuffer
	{
		private byte[] buffer;
		private int pos, count, length;
		private byte b;

		/// <summary>
		/// Initializes a new instance of the class BitBuffer
		/// </summary>
		/// <param name="buffer">Array of bit containing the data</param>
		/// <param name="index">Position of data in the array</param>
		/// <param name="count">Size of data in the array</param>
		public BitBuffer(byte[] buffer, int index, int count)
		{
			this.buffer = buffer;
			this.count = 8;
			this.pos = index;
			this.length = index + count;
			b = buffer[this.pos++];
		}

		public int GetBit()
		{
			int r;

			if (count-- == 0)
			{
				if (pos < length) b = buffer[pos++];
				else return 0;
				count = 7;
			}

			r = b & 0x1;
			b >>= 1;

			return r;
		}

		// This should return a sequence of 'count' bits when possible
		public int GetBits(int count)
		{
			int r = 0, n = 0, d;

			while (count > 0)
			{
				d = this.count - count;
				if (d >= 0)
					do
					{
						r = r | ((b & 0x1) << n++);
						b >>= 1;
						count--;
						this.count--;
					} while (count > 0);
				else
					while (this.count > 0)
					{
						r = r | ((b & 0x1) << n++);
						b >>= 1;
						count--;
						this.count--;
					}
				if (this.count == 0)
				{
					if (pos < length)
						b = buffer[pos++];
					else
						return r;
					this.count = 8;
				}
			}

			return r;
		}

		public byte GetByte() { return (byte)GetBits(8); }

		public bool Eof { get { return (pos == length - 1 && count == 0) || (pos >= length); } }
	}
}
