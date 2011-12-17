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
using IOPath = System.IO.Path;

namespace CrystalMpq.WoW
{
	/// <summary>Represents a language pack in a <see cref="WoWInstallation"/>.</summary>
	public sealed class WoWLanguagePack
	{
		private static readonly string[] expectedArchiveNamesOld = new string[]
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

		private static readonly string[] expectedArchiveNamesCataclysm = new string[]
		{
			"backup",
			"base",
			"locale",
			"speech"
		};

		private static readonly string[] expectedExpansionArchiveNames = new string[]
		{
			"locale",
			"speech"
		};

		private const string firstArchive = "{0}-{1}.MPQ";
		private const string otherArchive = "{0}-{1}-{2}.MPQ";
		private const string expansionArchive = "expansion{0}-{1}-{2}.MPQ";
		/// <summary>Format of the filename for cataclysm patch archives.</summary>
		private const string patchArchivePattern = "wow-update-{0}-?????.MPQ";
		/// <summary>Start index of the version number for patches.</summary>
		private const int patchArchiveNumberIndex = 11;
		/// <summary>Number of digits for patch MPQ version number.</summary>
		private const int patchArchiveNumberLength = 5;

		private static readonly Dictionary<string, int> localeFieldIndexDictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
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

		private WoWInstallation wowInstallation;
		private CultureInfo culture;
		private string wowCultureId;
		private string dataPath;
		private WoWArchiveInformation[] archiveArray;
		private ReadOnlyCollection<WoWArchiveInformation> archiveCollection;
		private int localeFieldIndex;

		internal WoWLanguagePack(WoWInstallation wowInstallation, CultureInfo culture)
		{
			this.wowInstallation = wowInstallation;
			this.culture = culture;
			this.wowCultureId = string.Join(null, culture.Name.Split('-'));
			if (!localeFieldIndexDictionary.TryGetValue(this.wowCultureId, out this.localeFieldIndex))
				this.localeFieldIndex = -1;
			this.dataPath = IOPath.Combine(wowInstallation.DataPath, wowCultureId);

			archiveArray = wowInstallation.InstallationKind == WoWInstallationKind.Cataclysmic ?
				FindArchives(this.dataPath, this.wowCultureId) :
				FindArchivesOld(this.dataPath, this.wowCultureId);
			archiveCollection = new ReadOnlyCollection<WoWArchiveInformation>(archiveArray);
		}

		#region Archive Detection Functions

		private static WoWArchiveInformation[] FindArchives(string dataPath, string wowCultureId)
		{
			var archiveList = new List<WoWArchiveInformation>();

			foreach (string expectedArchiveName in expectedArchiveNamesCataclysm)
			{
				string archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName, wowCultureId);
				if (File.Exists(IOPath.Combine(dataPath, archiveName))) archiveList.Add(new WoWArchiveInformation(archiveName, WoWArchiveKind.LanguagePack));
			}

			for (int i = 1; ; i++)
			{
				foreach (string expectedArchiveName in expectedExpansionArchiveNames)
				{
					string archiveName = string.Format(CultureInfo.InvariantCulture, expansionArchive, i, expectedArchiveName, wowCultureId);
					if (File.Exists(IOPath.Combine(dataPath, archiveName))) archiveList.Add(new WoWArchiveInformation(archiveName, WoWArchiveKind.LanguagePack));
					else if (i <= 3) return null; // There are at least 3 expansion archives for cataclysm…
					else goto FindPatchArchives;
				}
			}

		FindPatchArchives: ;
			var patchArchives = Directory.GetFiles(dataPath, string.Format(CultureInfo.InvariantCulture, patchArchivePattern, wowCultureId), SearchOption.TopDirectoryOnly);
			Array.Sort(patchArchives, StringComparer.InvariantCultureIgnoreCase);

			foreach (var archivePath in patchArchives)
			{
				string archiveName = IOPath.GetFileName(archivePath);
				archiveList.Add(new WoWArchiveInformation(archiveName, WoWArchiveKind.LanguagePack | WoWArchiveKind.Patch, WoWInstallation.DetectArchiveNumber(archiveName)));
			}

			return archiveList.ToArray();
		}

		private static WoWArchiveInformation[] FindArchivesOld(string dataPath, string wowCultureId)
		{
			var archiveList = new List<WoWArchiveInformation>();

			foreach (string expectedArchiveName in expectedArchiveNamesOld)
			{
				string archiveName = null;
				int i = 0;

				do
				{
					if (i++ != 0)
					{
						archiveList.Add(new WoWArchiveInformation(archiveName, WoWArchiveKind.LanguagePack));
						archiveName = string.Format(CultureInfo.InvariantCulture, otherArchive, expectedArchiveName, wowCultureId, i);
					}
					else
						archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName, wowCultureId);
				} while (File.Exists(IOPath.Combine(dataPath, archiveName)));
			}

			archiveList.Reverse();
			return archiveList.ToArray();
		}

		#endregion

		/// <summary>Gets the <see cref="WoWInstallation"/> containing this language pack.</summary>
		/// <value>The <see cref="WoWInstallation"/> containing this instance.</value>
		public WoWInstallation WoWInstallation { get { return wowInstallation; } }
		/// <summary>Gets the culture associated with this language pack.</summary>
		/// <value>The culture associated with this instance.</value>
		public CultureInfo Culture { get { return culture; } }
		/// <summary>Gets the path of this language pack.</summary>
		/// <value>The path of this language pack.</value>
		public string Path { get { return dataPath; } }
		/// <summary>Gets the collection of archives for this language pack.</summary>
		/// <value>The collection of archives for this language pack.</value>
		public ReadOnlyCollection<WoWArchiveInformation> Archives { get { return archiveCollection; } }
		/// <summary>Gets the index of the localized database field.</summary>
		/// <remarks>
		/// In first versions of World of Warcraft, the client databases contained special localized strings spanning multiple fields.
		/// One index was assigned to each of the supported cultures, allowing to fetch the localized string.
		/// Usually, all localization fields were blank excepted for the one corresponding to the language pack culture.
		/// </remarks>
		/// <value>The index of the localized database field.</value>
		[Obsolete("Localized DBC fields now share the same index for every language. This field has no more use starting with Cataclysm.")]
		public int DatabaseFieldIndex { get { return localeFieldIndex; } }

		/// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public override string ToString() { return culture.DisplayName; }
	}
}
