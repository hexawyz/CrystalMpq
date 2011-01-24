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
		/* 005 */ public string Name;
		/* 006 */ public int AreaId;
		/* 007 */ public string HordeDescription;
		/* 008 */ public string AllianceDescription;
		/* 009 */ public int LoadingScreen;
		/* 010 */ public float MapIconScaling;
		/* 011 */ public int ParentMapId;
		/* 012 */ public float EntryCoordX;
		/* 013 */ public float EntryCoordY;
		/* 014 */ public int TimeOfDayOverride;
		/* 015 */ public int ExpansionNumber;
		/* 016 */ public int Unknown1;
		/* 017 */ public int MaximumPlayerCount;
		/* 018 */ public int PhaseMapId;
	}
}
