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
	internal sealed class Encryption
	{
		internal static uint[] precalc;
		private static byte[] unpackBuffer;

		static Encryption()
		{
			int q, r = 0x100001;
			uint seed;

			precalc = new uint[0x500];
			unpackBuffer = new byte[0x2000];
			for (int i = 0; i < 0x100; i++)
				for (int j = 0; j < 5; j++)
				{
					unchecked
					{
						q = Math.DivRem(r * 125 + 3, 0x2AAAAB, out r);
						seed = (uint)(r & 0xFFFF) << 16;
						q = Math.DivRem(r * 125 + 3, 0x2AAAAB, out r);
						seed |= (uint)(r & 0xFFFF);
						precalc[0x100 * j + i] = seed;
					}
				}
		}

		public static uint Hash(string text, uint hashOffset)
		{
			uint hash = 0x7FED7FED, seed = 0xEEEEEEEE;
			byte[] buffer = new byte[text.Length];
			char c;
			byte b;

			for (int i = 0; i < text.Length; i++)
				unchecked
				{
					c = text[i]; // The 128 first Unicode characters are the 128 ASCII characters, so it's fine like this
					if (c >= 128)
						c = '?'; // Replace invalid ascii characters with this...
					b = (byte)c;
					if (b > 0x60 && b < 0x7B)
						b -= 0x20;
					hash = precalc[hashOffset + b] ^ (hash + seed);
					seed += hash + (seed << 5) + b + 3;
				}
			return hash;
		}

		// Old version
		//public static uint Hash(string text, uint hashOffset)
		//{
		//    uint hash = 0x7FED7FED, seed = 0xEEEEEEEE;
		//    byte[] buffer = new byte[text.Length];
		//    byte b;

		//    System.Text.Encoding.ASCII.GetBytes(text, 0, text.Length, buffer, 0);
		//    foreach (byte c in buffer)
		//        unchecked
		//        {
		//            b = c;
		//            if (b > 0x60 && b < 0x7B)
		//                b -= 0x20;
		//            hash = precalc[hashOffset + b] ^ (hash + seed);
		//            seed += hash + (seed << 5) + b + 3;
		//        }
		//    return hash;
		//}

		public static void Encrypt(uint[] data, uint hash)
		{
			uint buffer, seed = 0xEEEEEEEE;

			for (int i = 0; i < data.Length; i++)
				unchecked
				{
					seed += precalc[0x400 + hash & 0xFF];
					buffer = data[i];
					seed += buffer + (seed << 5) + 3;
					data[i] = buffer ^ (seed + hash);
					hash = (hash >> 11) | (0x11111111 + ((hash ^ 0x7FF) << 21));
				}
		}

		public static unsafe void Decrypt(uint[] data, uint hash)
		{
			fixed (uint* dataPointer = data)
				Decrypt(dataPointer, hash, data.Length);
		}

		public static unsafe void Decrypt(uint[] data, uint hash, int length)
		{
			fixed (uint* dataPointer = data)
				Decrypt(dataPointer, hash, length);
		}

		public static unsafe void Decrypt(byte[] data, uint hash)
		{
			fixed (byte* dataPointer = data)
				Decrypt(dataPointer, hash, data.Length >> 2);
		}

		public static unsafe void Decrypt(byte[] data, uint hash, int length)
		{
			fixed (byte* dataPointer = data)
				Decrypt(dataPointer, hash, length >> 2);
		}

		public static unsafe void Decrypt(void *data, uint hash, int length)
		{
			uint buffer, temp = 0xEEEEEEEE;
			uint* dataPointer = (uint*)data;

			for (int i = 0; i < length; i++)
				unchecked
				{
					temp += precalc[0x400 + (hash & 0xFF)];
					buffer = *dataPointer ^ (temp + hash);
					temp += buffer + (temp << 5) + 3;
					*dataPointer++ = buffer;
					hash = (hash >> 11) | (0x11111111 + ((hash ^ 0x7FF) << 21));
				}
		}
	}
}
