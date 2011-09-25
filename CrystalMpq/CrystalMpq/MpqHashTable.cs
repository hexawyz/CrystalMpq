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
using System.Runtime.InteropServices;

namespace CrystalMpq
{
	internal class MpqHashTable : IEnumerable<MpqHashTable.HashEntry>
	{
		#region HashEntry Class

		public struct HashEntry
		{
			public static readonly HashEntry Invalid = new HashEntry();

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

			public bool Test(uint hashA, uint hashB) { return hashA == this.hashA && hashB == this.hashB; }

			public int Locale { get { return locale; } }

			public int Block { get { return block; } }

			public bool IsValid { get { return block != -1 && hashA != 0xFFFFFFFF && hashA != 0xFFFFFFFF; } }
		}

		#endregion

		#region HashEntryEnumerator Structure

		public struct HashEntryEnumerator : IEnumerator<HashEntry>
		{
			private HashEntry[] entries;
			private int index;

			public HashEntryEnumerator(HashEntry[] entries)
			{
				this.entries = entries;
				this.index = -1;
			}

			public void Dispose() { this.entries = null; }

			public HashEntry Current { get { return entries[index]; } }
			object IEnumerator.Current { get { return entries[index]; } }

			public bool MoveNext()
			{
				if (index >= entries.Length) return false;

				return ++index < entries.Length;
			}

			public void Reset() { this.index = -1; }
		}

		#endregion

		public static unsafe MpqHashTable FromData(byte[] buffer, int dataLength, int tableLength)
		{
			var entries = new HashEntry[tableLength];

			fixed (byte* bufferPointer = buffer)
			{
				uint* hashTableDataPointer = (uint*)bufferPointer;

				// One table entry is 4 [u]int…
				Encryption.Decrypt(bufferPointer, MpqArchive.HashTableHash, 4 * tableLength);

				for (int i = 0; i < entries.Length; i++) // Fill MpqHashTable object
					entries[i] = new HashEntry(*hashTableDataPointer++, *hashTableDataPointer++, (int)*hashTableDataPointer++, (int)*hashTableDataPointer++);
			}

			return new MpqHashTable(entries);
		}

		private HashEntry[] entries;
		private uint capacity;
		private int preferredCulture;

		private MpqHashTable(int capacity)
			: this(new HashEntry[capacity]) { }

		private MpqHashTable(HashEntry[] entries)
		{
			this.capacity = (uint)entries.Length;
			this.entries = entries;
		}

		public HashEntry this[int index]
		{
			get
			{
				if (index < 0 || index > capacity) throw new ArgumentOutOfRangeException("index");

				return entries[index];
			}
		}

		public int this[string filename] { get { return Find(filename); } }

		public long Capacity { get { return capacity; } }

		internal void SetEntry(int index, uint hashA, uint hashB, int locale, int block)
		{
			if (index < 0 || index > entries.Length) throw new ArgumentOutOfRangeException("index");

			entries[index] = new HashEntry(hashA, hashB, locale, block);
		}

		public int[] FindMulti(string filename)
		{
			var matches = new List<int>();
			uint hash = Encryption.Hash(filename, 0);
			uint hashA = Encryption.Hash(filename, 0x100);
			uint hashB = Encryption.Hash(filename, 0x200);
			uint start = hash % capacity;
			uint index = start;

			do
			{
				// Stop on invalid entry
				if (!entries[index].IsValid) break;

				if (entries[index].Test(hashA, hashB))
					matches.Add(entries[index].Block);

				// If we find an invalid entry, then we end the research
				if (++index >= capacity) index = 0;
			}
			while (index != start);

			return matches.ToArray();
		}

		public int Find(string filename) { return Find(filename, preferredCulture); }

		public int Find(string filename, int lcid)
		{
			var matches = new List<HashEntry>();
			uint hash = Encryption.Hash(filename, 0);
			uint hashA = Encryption.Hash(filename, 0x100);
			uint hashB = Encryption.Hash(filename, 0x200);
			uint start = hash % capacity;
			uint index = start;

			do
			{
				// Stop on invalid entry
				if (!entries[index].IsValid) break;

				if (entries[index].Test(hashA, hashB))
				{
					if (entries[index].Locale == lcid)
						return entries[index].Block;
					else
						matches.Add(entries[index]);
				}

				if (++index >= capacity) index = 0;
			}
			while (index != start);

			if (matches.Count == 0) return -1;
			else if (matches.Count == 1) return matches[0].Block;
			else
				foreach (HashEntry entry in matches)
					if (entry.Locale == 0) return entry.Block;

			return -1;
		}

		private bool TryFindEntry(int block, out HashEntry result)
		{
			foreach (var entry in entries)
				if (entry.Block == block)
				{
					result = entry;
					return true;
				}

			result = HashEntry.Invalid;
			return false;
		}

		public int GetLocale(int block)
		{
			HashEntry entry;

			if (!TryFindEntry(block, out entry) || !entry.IsValid) throw new InvalidFileReferenceException();
			else return entry.Locale;
		}

		public void SetPreferredCulture(int lcid) { preferredCulture = lcid; }

		internal bool CheckIntegrity(int blockTableSize)
		{
			bool[] array;
			int counter;

			array = new bool[blockTableSize];
			counter = 0;
			foreach (var entry in entries)
			{
				if (!entry.IsValid) continue;
				if (entry.Block >= blockTableSize || array[entry.Block] != false)
					return false;
				array[entry.Block] = true;
				counter++;
			}

			return counter <= blockTableSize;
		}

		public HashEntryEnumerator GetEnumerator() { return new HashEntryEnumerator(entries); }
		IEnumerator<HashEntry> IEnumerable<HashEntry>.GetEnumerator() { return new HashEntryEnumerator(entries); }
		IEnumerator IEnumerable.GetEnumerator() { return new HashEntryEnumerator(entries); }
	}
}
