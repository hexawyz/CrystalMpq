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
	/// Thrown when the archive appears to be corrupt.
	/// </summary>
	/// <remarks>
	/// It might happen that this error is thrown on a perfectly valid archive.
	/// If that's the case, please inform the author of this.
	/// </remarks>
	public sealed class ArchiveCorruptException : MpqException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArchiveCorruptException"/> class.
		/// </summary>
		internal ArchiveCorruptException() : base("Archive is corrupt")
		{
		}
	}
}
