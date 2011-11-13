using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalMpq
{
	internal struct MpqBlockEntry
	{
		public string Name;
		public long Offset;
		public uint CompressedSize;
		public uint UncompressedSize;
		public MpqFileFlags Flags;
		public uint Seed;
		public bool Listed;
	}
}
