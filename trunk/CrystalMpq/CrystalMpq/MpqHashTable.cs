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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace CrystalMpq
{
	internal class MpqHashTable : IEnumerable<MpqHashTable.HashEntry>
	{
		#region HashEntry Class

		public class HashEntry
		{
			public static HashEntry Invalid;

			static HashEntry()
			{
				Invalid = new HashEntry(0, 0, 0, 0);
			}

			private uint hashA;
			private uint hashB;
			private int locale;
			private int block;

			public HashEntry(uint hashA, uint hashB, int locale, int block)
			{
				this.hashA = hashA;
				this.hashB = hashB;
				this.locale = locale;
				this.block = block;
			}

			public bool Test(uint hashA, uint hashB)
			{
				if (hashA == this.hashA && hashB == this.hashB)
					return true;
				else
					return false;
			}

			public int Locale
			{
				get
				{
					return locale;
				}
			}

			public int Block
			{
				get
				{
					return block;
				}
			}

			public bool Valid
			{
				get
				{
					if (block == -1 || hashA == 0xFFFFFFFF || hashA == 0xFFFFFFFF)
						return false;
					else
						return true;
				}
			}
		}

		#endregion

		private HashEntry[] table;
		private uint capacity;
		private int preferredCulture;

		public MpqHashTable(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			table = new HashEntry[capacity];
			for (int i = 0; i < table.Length; i++)
				table[i] = HashEntry.Invalid;

			this.capacity = (uint)capacity;
		}

		public HashEntry this[int index]
		{
			get
			{
				if (index < 0 || index > capacity)
					throw new ArgumentOutOfRangeException("index");
				return table[index];
			}
		}

		public int this[string filename]
		{
			get
			{
				return Find(filename);
			}
		}

		public long Capacity
		{
			get
			{
				return capacity;
			}
		}

		internal void SetEntry(int index, uint hashA, uint hashB, int locale, int block)
		{
			if (index < 0 || index > table.Length)
				throw new ArgumentOutOfRangeException("index");
			table[index] = new HashEntry(hashA, hashB, locale, block);
		}

		public int[] FindMulti(string filename)
		{
			uint hash, hashA, hashB, start, index;
			List<int> matches;

			matches = new List<int>();
			hash = Encryption.Hash(filename, 0);
			hashA = Encryption.Hash(filename, 0x100);
			hashB = Encryption.Hash(filename, 0x200);
			start = hash % capacity;
			index = start;
			do
			{
				// Stop on invalid entry
				if (!table[index].Valid)
					break;
				if (table[index].Test(hashA, hashB))
					matches.Add(table[index].Block);
				// If we find an invalid entry, then we end the research
				if (++index >= capacity)
					index = 0;
			}
			while (index != start);
			return matches.ToArray();
		}

		public int Find(string filename)
		{
			return Find(filename, preferredCulture);
		}

		public int Find(string filename, int lcid)
		{
			uint hash, hashA, hashB, start, index;
			List<HashEntry> matches;

			matches = new List<HashEntry>();
			hash = Encryption.Hash(filename, 0);
			hashA = Encryption.Hash(filename, 0x100);
			hashB = Encryption.Hash(filename, 0x200);
			start = hash % capacity;
			index = start;
			do
			{
				// Stop on invalid entry
				if (!table[index].Valid)
					break;
				if (table[index].Test(hashA, hashB))
				{
					if (table[index].Locale == lcid)
						return table[index].Block;
					else
						matches.Add(table[index]);
				}
				if (++index >= capacity)
					index = 0;
			}
			while (index != start);
			if (matches.Count == 0)
				return -1;
			else if (matches.Count == 1)
				return matches[0].Block;
			else
				foreach (HashEntry entry in matches)
					if (entry.Locale == 0)
						return entry.Block;
			return -1;
		}

		private HashEntry FindEntry(int block)
		{
			foreach (HashEntry entry in table)
				if (entry.Block == block)
					return entry;
			return null;
		}

		public int GetLocale(int block)
		{
			HashEntry entry = FindEntry(block);

			if (entry == null || !entry.Valid)
				throw new InvalidFileReference();
			else
				return entry.Locale;
		}

		public void SetPreferredCulture(int lcid)
		{
			preferredCulture = lcid;
		}

		internal bool CheckIntegrity(int blockTableSize)
		{
			bool[] array;
			int counter;

			array = new bool[blockTableSize];
			counter = 0;
			foreach (HashEntry entry in table)
			{
				if (!entry.Valid)
					continue;
				if (entry.Block >= blockTableSize || array[entry.Block] != false)
					return false;
				array[entry.Block] = true;
				counter++;
			}
			if (counter <= blockTableSize)
				return true;
			else
				return false;
		}

		public IEnumerator<HashEntry> GetEnumerator()
		{
			for (int i = 0; i < table.Length; i++)
				yield return table[i];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return table.GetEnumerator();
		}
	}
}
