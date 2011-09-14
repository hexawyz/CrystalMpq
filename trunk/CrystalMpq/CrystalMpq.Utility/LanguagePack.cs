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
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using _Path = System.IO.Path;

namespace CrystalMpq.Utility
{
	public sealed class LanguagePack
	{
		static readonly string[] expectedArchiveNamesOld = new string[]
		{
			"backup",
			"base",
			"locale",
			"speech",
			"expansion-locale",
			"expansion-speech",
			"lichking-locale",
			"lichking-speech",
			"patch"
		};

		static readonly string[] expectedArchiveNamesCataclysm = new string[]
		{
			"backup",
			"base",
			"locale",
			"speech"
		};

		static readonly string[] expectedExpansionArchiveNames = new string[]
		{
			"locale",
			"speech"
		};

		static readonly string firstArchive = "{0}-{1}.MPQ";
		static readonly string otherArchive = "{0}-{1}-{2}.MPQ";
		static readonly string expansionArchive = "expansion{0}-{1}-{2}.MPQ";

		static readonly Dictionary<string, int> localeFieldIndexDictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "enUS", 0 },
			{ "enGB", 0 },
			{ "koKR", 1 },
			{ "frFR", 2 },
			{ "deDE", 3 },
			{ "zhCN", 4 },
			{ "zhTW", 5 },
			{ "esES", 6 },
			{ "esMX", 6 },
			{ "ruRU", 7 },
		};

		WoWInstallation wowInstallation;
		CultureInfo culture;
		string wowCultureId;
		string dataPath;
		string[] archiveArray;
		int localeFieldIndex;

		ReadOnlyCollection<string> archiveCollection;

		internal LanguagePack(WoWInstallation wowInstallation, CultureInfo culture)
		{
			this.wowInstallation = wowInstallation;
			this.culture = culture;
			this.wowCultureId = string.Join(null, culture.Name.Split('-'));
			if (!localeFieldIndexDictionary.TryGetValue(this.wowCultureId, out this.localeFieldIndex))
				this.localeFieldIndex = -1;
			this.dataPath = _Path.Combine(wowInstallation.DataPath, wowCultureId);

			archiveArray = wowInstallation.InstallationKind == InstallationKind.Cataclysmic ?
				FindArchives(this.dataPath, this.wowCultureId) :
				FindArchivesOld(this.dataPath, this.wowCultureId);
			archiveCollection = new ReadOnlyCollection<string>(archiveArray);
		}

		#region Archive Detection Functions

		private static string[] FindArchives(string dataPath, string wowCultureId)
		{
			List<string> archiveList = new List<string>();

			foreach (string expectedArchiveName in expectedArchiveNamesCataclysm)
			{
				string archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName, wowCultureId);
				if (File.Exists(_Path.Combine(dataPath, archiveName))) archiveList.Add(archiveName);
			}

			for (int i = 1; ; i++)
			{
				foreach (string expectedArchiveName in expectedExpansionArchiveNames)
				{
					string archiveName = string.Format(CultureInfo.InvariantCulture, expansionArchive, i, expectedArchiveName, wowCultureId);
					if (File.Exists(_Path.Combine(dataPath, archiveName))) archiveList.Add(archiveName);
					else if (i <= 3) return null; // There are at least 3 expansion archives for cataclysm…
					else
					{
						archiveList.Reverse();
						return archiveList.ToArray();
					}
				}
			}
		}

		private static string[] FindArchivesOld(string dataPath, string wowCultureId)
		{
			List<string> archiveList = new List<string>();

			foreach (string expectedArchiveName in expectedArchiveNamesOld)
			{
				string archiveName = null;
				int i = 0;

				do
				{
					if (i++ != 0)
					{
						archiveList.Add(archiveName);
						archiveName = string.Format(CultureInfo.InvariantCulture, otherArchive, expectedArchiveName, wowCultureId, i);
					}
					else
						archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName, wowCultureId);
				} while (File.Exists(_Path.Combine(dataPath, archiveName)));
			}

			archiveList.Reverse();
			return archiveList.ToArray();
		}

		#endregion

		public WoWInstallation WoWInstallation { get { return wowInstallation; } }
		public CultureInfo Culture { get { return culture; } }
		public string Path { get { return dataPath; } }
		public ReadOnlyCollection<string> Archives { get { return archiveCollection; } }
		[Obsolete("Localized DBC fields now share the same index for every language. This field has no more use starting with Cataclysm.")]
		public int DatabaseFieldIndex { get { return localeFieldIndex; } }

		public override string ToString() { return culture.DisplayName; }
	}
}
