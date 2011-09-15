using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalMpq.Utility
{
	/// <summary>Represents the kind of installation.</summary>
	public enum InstallationKind
	{
		/// <summary>The installation is a classic World of Warcraft installation.</summary>
		Classic = 0,
		/// <summary>The installation is a World of Warcraft installation from Cataclysm or newer.</summary>
		Cataclysmic = 1
	}
}
