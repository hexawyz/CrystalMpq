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
	/// <summary>
	/// Thrown when the seed for a file is unknown.
	/// </summary>
	/// <remarks>
	/// The seed is needed for reading encrypted files.
	/// </remarks>
	public sealed class SeedNotFoundException : MpqException
	{
		internal SeedNotFoundException(int block) : base("Seed not found for file 0x" + block.ToString("X"))
		{
		}
	}
}
