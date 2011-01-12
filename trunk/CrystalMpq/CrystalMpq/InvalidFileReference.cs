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
	/// 
	/// </summary>
	public sealed class InvalidFileReference : MpqException
	{
		/// <summary>
		/// 
		/// </summary>
		internal InvalidFileReference() : base("Referrence to a file (block) that doesn't exist")
		{
		}
	}
}
