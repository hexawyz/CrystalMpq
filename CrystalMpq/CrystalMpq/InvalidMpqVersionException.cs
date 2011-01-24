using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalMpq
{
	/// <summary>
	/// Exception thrown when the MPQ version is not recognized by the library.
	/// </summary>
	sealed class InvalidMpqVersionException : MpqException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidMpqVersionException"/> class.
		/// </summary>
		public InvalidMpqVersionException() : base("Invalid MPQ version.") { }
	}
}
