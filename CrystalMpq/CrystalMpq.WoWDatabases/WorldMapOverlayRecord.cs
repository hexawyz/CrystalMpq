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
using CrystalMpq.DataFormats;
using System.Diagnostics;

namespace CrystalMpq.WoWDatabases
{
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("WorldMapOverlayRecord: Id={Id}, DataName={DataName}")]
	public struct WorldMapOverlayRecord
	{
		[Id] public int Id;
		public int WorldMapArea;
		public int Area1;
		public int Area2;
		public int Area3;
		public int Area4;
		public int Unknown1;
		public int Unknown2;
		public string DataName;
		public int Width;
		public int Height;
		public int Left;
		public int Top;
		public int BoxTop;
		public int BoxLeft;
		public int BoxBottom;
		public int BoxRight;
	}
}
