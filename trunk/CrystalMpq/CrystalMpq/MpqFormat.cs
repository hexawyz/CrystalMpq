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
using System.Collections.Generic;
using System.Text;

namespace CrystalMpq
{
	/// <summary>
	/// This enumeraction gives information about the format of a given MPQ archive
	/// </summary>
	public enum MpqFormat
	{
		/// <summary>
		/// The archive follows the original MPQ format
		/// </summary>
		Original = 0,
		/// <summary>
		/// The archive is an extended MPQ archive
		/// </summary>
		/// <remarks>
		/// These archives can exceed the file size of 2 Gb, and possesses additionnal parameters for the files included
		/// </remarks>
		Extended = 1
	}
}
