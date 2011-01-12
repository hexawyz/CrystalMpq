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
	public struct MapRecord
	{
		/* 000 */ [Id] public int Id;
		/* 001 */ public string DataName;
		/* 002 */ public int AreaType;
		/* 003 */ public int ExtraInformation;
		/* 004 */ public bool IsBattleground;
		/* 005-021 */ [Localized] public string Name;
		/* 022 */ public int AreaId;
		/* 023-039 */ [Localized] public string HordeDescription;
		/* 040-056 */ [Localized] public string AllianceDescription;
		/* 057 */ public int LoadingScreen;
		/* 058 */ public float MapIconScaling;
		/* 059 */ public int ParentMapId;
		/* 060 */ public float EntryCoordX;
		/* 061 */ public float EntryCoordY;
		/* 062 */ public int TimeOfDayOverride;
		/* 063 */ public int ExpansionNumber;
		/* 064 */ public int Unknown1;
		/* 065 */ public int MaximumPlayerCount;
	}
}
