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
	/// Base class for CrystalMpq specific exceptions
	/// </summary>
	public class MpqException : Exception
	{
		/// <summary>
		/// Creates a new instance of the MPQException class.
		/// </summary>
		/// <param name="message"></param>
		protected internal MpqException(string message) : base(message)
		{
		}
	}
}
