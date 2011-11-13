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
	internal struct MpqHashTable : IEnumerable<MpqHashEntry>
	{
		#region HashEntryEnumerator Structure

		public struct HashEntryEnumerator : IEnumerator<MpqHashEntry>
		{
			private MpqHashEntry[] entries;
			private int index;

			public HashEntryEnumerator(MpqHashEntry[] entries)
			{
				this.entries = entries;
				this.index = -1;
			}

			public void Dispose() { this.entries = null; }

			public MpqHashEntry Current { get { return entries[index]; } }
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
			var entries = new MpqHashEntry[tableLength];

			fixed (byte* bufferPointer = buffer)
			{
				uint* hashTableDataPointer = (uint*)bufferPointer;
				int uintCount = tableLength << 2; // One table entry is 4 [u]int…

				if (!BitConverter.IsLittleEndian) Utility.SwapBytes(hashTableDataPointer, uintCount);
				Encryption.Decrypt(hashTableDataPointer, MpqArchive.HashTableHash, uintCount);

				for (int i = 0; i < entries.Length; i++) // Fill MpqHashTable object
					entries[i] = new MpqHashEntry(*hashTableDataPointer++, *hashTableDataPointer++, (int)*hashTableDataPointer++, (int)*hashTableDataPointer++);
			}

			return new MpqHashTable(entries);
		}

		private MpqHashEntry[] entries;
		private int preferredCulture;

		private MpqHashTable(int capacity)
			: this(new MpqHashEntry[capacity]) { }

		private MpqHashTable(MpqHashEntry[] entries)
		{
			this.entries = entries;
			this.preferredCulture = 0;
		}

		public long Capacity { get { return entries.LongLength; } }

		internal void SetEntry(int index, uint hashA, uint hashB, int locale, int block) { entries[index] = new MpqHashEntry(hashA, hashB, locale, block); }

		public int[] FindMulti(string filename)
		{
			var matches = new List<int>();
			uint hash = Encryption.Hash(filename, 0);
			uint hashA = Encryption.Hash(filename, 0x100);
			uint hashB = Encryption.Hash(filename, 0x200);
			uint capacity = checked((uint)entries.LongLength);
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
			uint? neutralEntryIndex = null;
			uint? firstEntryIndex = null;

			uint hash = Encryption.Hash(filename, 0);
			uint hashA = Encryption.Hash(filename, 0x100);
			uint hashB = Encryption.Hash(filename, 0x200);
			uint capacity = checked((uint)entries.LongLength);
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
					else if (entries[index].Locale == 0)
						neutralEntryIndex = index;
					else if (firstEntryIndex == null)
						firstEntryIndex = index;
				}

				if (++index >= capacity) index = 0;
			}
			while (index != start);

			return neutralEntryIndex != null ?
				entries[neutralEntryIndex.Value].Block :
				firstEntryIndex != null && lcid == 0 ?
					entries[firstEntryIndex.Value].Block :
					-1;
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
		IEnumerator<MpqHashEntry> IEnumerable<MpqHashEntry>.GetEnumerator() { return new HashEntryEnumerator(entries); }
		IEnumerator IEnumerable.GetEnumerator() { return new HashEntryEnumerator(entries); }
	}
}
