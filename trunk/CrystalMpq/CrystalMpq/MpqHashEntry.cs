using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalMpq
{
	internal struct MpqHashEntry
	{
		public static readonly MpqHashEntry Invalid = new MpqHashEntry();

		private uint hashA;
		private uint hashB;
		private int locale;
		private int block;

		public MpqHashEntry(uint hashA, uint hashB, int locale, int block)
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
}
