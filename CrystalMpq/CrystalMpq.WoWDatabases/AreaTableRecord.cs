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
using System.Runtime.InteropServices;
using CrystalMpq.WoWFile;

namespace CrystalMpq.WoWDatabases
{
	[StructLayout(LayoutKind.Sequential)]
	public struct AreaTableRecord
	{
		[Id] public int Id;
		public int Map;
		public int Parent;
		public int UnknownId;
		public int Flags;
		public int SoundPreferences;
		public int Unknown1;
		public int SoundAmbience;
		public int ZoneMusic;
		public int ZoneIntroMusic;
		public int AreaLevel;
		public string Name;
		public int FactionGroup;
		public int Unknown2;
		public int Unknown3;
		public int Unknown4;
		public int Unknown5;
		public float Unknown6;
		public float Unknown7;
		public int Unknown8; // 0
		public int Flags2;
		public int Unknown9; // 0
		public int Unknown10;
		public int Unknown11;
	}
}
