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
using Microsoft.Win32;
using IOPath = System.IO.Path;

namespace CrystalMpq.Utility
{
	/// <summary>
	/// Represents a WoW installation on the machine.
	/// </summary>
	public sealed class WoWInstallation
	{
		#region LanguagePackCollection

		/// <summary>
		/// Represents a collection of <see cref="LanguagePack"/> associated with a <see cref="WoWInstalaltion"/>.
		/// </summary>
		public sealed class LanguagePackCollection : IList<LanguagePack>
		{
			WoWInstallation wowInstallation;

			internal LanguagePackCollection(WoWInstallation wowInstallation) { this.wowInstallation = wowInstallation; }

			/// <summary>
			/// Gets or sets the <see cref="CrystalMpq.Utility.LanguagePack"/> at the specified index.
			/// </summary>
			/// <value></value>
			public LanguagePack this[int index]
			{
				get { return wowInstallation.languagePackArray[index]; }
				set { throw new NotSupportedException(); }
			}

			/// <summary>
			/// Gets the number of elements contained in the <see cref="LanguagePackCollection"/>.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The number of elements contained in the <see cref="LanguagePackCollection"/>.
			/// </returns>
			public int Count { get { return wowInstallation.languagePackArray.Length; } }
			/// <summary>
			/// Gets a value indicating whether the <see cref="LanguagePackCollection"/> is read-only.
			/// </summary>
			/// <value></value>
			/// <returns>true if the <see cref="LanguagePackCollection"/> is read-only; otherwise, false.
			/// </returns>
			/// <remarks>The <see cref="LanguagePackCollection"/> will always be read-only.</remarks>
			public bool IsReadOnly { get { return true; } }

			/// <summary>
			/// Determines the index of a specific item in the <see cref="LanguagePackCollection"/>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="LanguagePackCollection"/>.</param>
			/// <returns>
			/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(LanguagePack item) { return ((IList<LanguagePack>)wowInstallation.languagePackArray).IndexOf(item); }
			/// <summary>
			/// Determines whether the <see cref="LanguagePackCollection"/> contains a specific value.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="LanguagePackCollection"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> is found in the <see cref="LanguagePackCollection"/>; otherwise, false.
			/// </returns>
			public bool Contains(LanguagePack item) { return ((IList<LanguagePack>)wowInstallation.languagePackArray).Contains(item); }
			/// <summary>
			/// Copies the elements of the <see cref="LanguagePackCollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
			/// </summary>
			/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="LanguagePackCollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
			/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
			/// <exception cref="T:System.ArgumentNullException">
			/// 	<paramref name="array"/> is null.
			/// </exception>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="arrayIndex"/> is less than 0.
			/// </exception>
			/// <exception cref="T:System.ArgumentException">
			/// 	<paramref name="array"/> is multidimensional.
			/// -or-
			/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
			/// -or-
			/// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
			/// -or-
			/// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
			/// </exception>
			public void CopyTo(LanguagePack[] array, int arrayIndex) { wowInstallation.languagePackArray.CopyTo(array, arrayIndex); }

			void IList<LanguagePack>.Insert(int index, LanguagePack item) { throw new NotSupportedException(); }
			void IList<LanguagePack>.RemoveAt(int index) { throw new NotSupportedException(); }
			void ICollection<LanguagePack>.Add(LanguagePack item) { throw new NotSupportedException(); }
			bool ICollection<LanguagePack>.Remove(LanguagePack item) { throw new NotSupportedException(); }
			void ICollection<LanguagePack>.Clear() { throw new NotSupportedException(); }

			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<LanguagePack> GetEnumerator() { return ((IList<LanguagePack>)wowInstallation.languagePackArray).GetEnumerator(); }
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((System.Collections.IEnumerable)wowInstallation.languagePackArray).GetEnumerator(); }
		}

		#endregion

		/// <summary>Array of expected archive names.</summary>
		/// <remarks>
		/// Those names are highly related with the version of WoW supported.
		/// Archives listed here are for the old WoW instalaltion type. (Pre-Cataclysm)
		/// </remarks>
		static readonly string[] expectedArchiveNamesOld = new string[]
		{
			"common",
			"expansion",
			"lichking",
			"patch"
		};
		/// <summary>Array of expected archive names.</summary>
		/// <remarks>
		/// Those names are highly related with the version of WoW supported.
		/// Archives listes here are the new cataclysm archives.
		/// 
		/// </remarks>
		static readonly string[] expectedArchiveNamesCataclysm = new string[]
		{
			"sound",
			"art",
			"world",
		};

		/// <summary>Format of the default archive filename.</summary>
		static readonly string firstArchive = "{0}.MPQ";
		/// <summary>Format of the filename for supplementary archives.</summary>
		static readonly string otherArchive = "{0}-{1}.MPQ";
		/// <summary>Format of the filename for expansion archives.</summary>
		static readonly string expansionArchive = "expansion{0}.MPQ";

		/// <summary>Path to the instalaltion.</summary>
		string wowPath;
		/// <summary>Path to the data associated with the instalaltion.</summary>
		string dataPath;
		/// <summary>Array of archives associated with the instalaltion.</summary>
		/// <remarks>The archives are detected based on their filename, during the instantiation of the class.</remarks>
		string[] archiveArray;
		/// <summary>Collection of archives associated with the instalaltion.</summary>
		/// <remarks>This is a wrapper around <seealso cref="F:archiveArray"/>.</remarks>
		ReadOnlyCollection<string> archiveCollection;
		/// <summary>Array of <see cref="LanguagePack"/> associated with the installation.</summary>
		LanguagePack[] languagePackArray;
		/// <summary>Collection of <see cref="LanguagePack"/> associated with the installation.</summary>
		/// <remarks>This is a wrapper around <seealso cref="F:languagePackArray"/>.</remarks>
		LanguagePackCollection languagePackCollection;
		/// <summary>Value representing the instllation kind.</summary>
		InstallationKind installationKind;

		/// <summary>Initializes a new instance of the <see cref="WoWInstallation"/> class.</summary>
		/// <param name="path">The path.</param>
		private WoWInstallation(string path)
		{
			if (!Directory.Exists(this.wowPath = path))
				throw new DirectoryNotFoundException();

			if (!Directory.Exists(this.dataPath = System.IO.Path.Combine(path, "Data")))
				throw new DirectoryNotFoundException();

			if ((archiveArray = FindArchives(this.dataPath)) != null)
				installationKind = InstallationKind.Cataclysmic;
			else if ((archiveArray = FindArchivesOld(this.dataPath)) != null)
				installationKind = InstallationKind.Classic;
			else throw new FileNotFoundException();

			archiveCollection = new ReadOnlyCollection<string>(archiveArray);

			FindLanguagePacks();
		}

		/// <summary>Tries to locate the standard WoW installation.</summary>
		/// <returns>A <see cref="WoWInstallation"/> instance representing the standard WoW installation, if found.</returns>
		public static WoWInstallation Find()
		{
			RegistryKey wowKey = null;
			string path = null;

			try
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					if ((wowKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Blizzard Entertainment\World of Warcraft") ??
							Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Blizzard Entertainment\World of Warcraft")) != null)
						path = (string)wowKey.GetValue("InstallPath");
					else
						throw new FileNotFoundException();
				}
				else if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
					path = @"/Applications/World of Warcraft";
				else
					throw new PlatformNotSupportedException("Automatic WoW Installation Discovery Unsupported for your platform.");
			}
			finally
			{
				if (wowKey != null)
					wowKey.Close();
			}
			return new WoWInstallation(path);
		}

		#region Archive Detection Functions

		/// <summary>Finds the archives associated with this <see cref="WoWInstallation"/>.</summary>
		/// <remarks>This implementation will find archives for the new Cataclysm instlaltions, not the old ones.</remarks>
		private static string[] FindArchives(string dataPath)
		{
			List<string> archiveList = new List<string>();

			foreach (string expectedArchiveName in expectedArchiveNamesCataclysm)
			{
				string archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName);
				if (File.Exists(IOPath.Combine(dataPath, archiveName))) archiveList.Add(archiveName);
				else return null;
			}

			for (int i = 1; ; i++)
			{
				string archiveName = string.Format(CultureInfo.InvariantCulture, expansionArchive, i);
				if (File.Exists(IOPath.Combine(dataPath, archiveName))) archiveList.Add(archiveName);
				else if (i <= 3) return null; // There are at least 3 expansion archives for cataclysm…
				else break;
			}

			archiveList.Reverse();
			return archiveList.ToArray();
		}
		
		/// <summary>Finds the archives associated with this <see cref="WoWInstallation"/>.</summary>
		/// <remarks>This implementation will find archives for the old pre-Cataclysm WoW installations.</remarks>
		private static string[] FindArchivesOld(string dataPath)
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
						archiveName = string.Format(CultureInfo.InvariantCulture, otherArchive, expectedArchiveName, i);
					}
					else
						archiveName = string.Format(CultureInfo.InvariantCulture, firstArchive, expectedArchiveName);
				} while (File.Exists(IOPath.Combine(dataPath, archiveName)));
			}

			archiveList.Reverse();
			return archiveList.ToArray();
		}

		/// <summary>Finds the <see cref="LanguagePack"/>s associated with this <see cref="WoWInstallation"/>.</summary>
		/// <remarks>Each <see cref="LanguagePack"/> itself contains another list of archives.</remarks>
		private void FindLanguagePacks()
		{
			List<LanguagePack> languagePackList = new List<LanguagePack>();

			foreach (string directoryPath in Directory.GetDirectories(dataPath))
			{
				string directoryName = IOPath.GetFileName(directoryPath);

				if (directoryName != null && directoryName.Length == 4)
				{
					try
					{
						// Tries to create a CultureInfo object from th directory name
						// WoW language packs use standard culture identifiers, meaning this should only fail if an invalid directory is found here
						CultureInfo culture = new CultureInfo(directoryName.Substring(0, 2) + '-' + directoryName.Substring(2, 2));

						// Adds the newly found LanguagePack to the list
						languagePackList.Add(new LanguagePack(this, culture));
					}
					catch (ArgumentException) { } // Catches only ArgumentException, which should only happen when CultureInfo constructor fails
				}
			}

			languagePackArray = languagePackList.ToArray();
			languagePackCollection = new LanguagePackCollection(this);
		}

		#endregion

		/// <summary>Creates a MpqFileSystem using the specified language pack.</summary>
		/// <param name="languagePack">The language pack.</param>
		/// <param name="parseListFiles">if set to <c>true</c> the list files will be parsed.</param>
		/// <returns>The newly created MpqFileSystem.</returns>
		public MpqFileSystem CreateFileSystem(LanguagePack languagePack, bool parseListFiles)
		{
			return CreateFileSystem(languagePack, true, parseListFiles);
		}

		/// <summary>Creates a MpqFileSystem using the specified language pack.</summary>
		/// <param name="languagePack">The language pack.</param>
		/// <param name="enforceCultureCheck">if set to <c>true</c> the culture checks will be enforced.</param>
		/// <param name="parseListFiles">if set to <c>true</c> the list files will be parsed.</param>
		/// <returns>The newly created MpqFileSystem.</returns>
		public MpqFileSystem CreateFileSystem(LanguagePack languagePack, bool enforceCultureCheck, bool parseListFiles)
		{
			MpqFileSystem mpqFileSystem;

			if (languagePack == null)
				throw new ArgumentNullException("languagePack");
			if (languagePack.WoWInstallation != this)
				throw new ArgumentException();
#pragma warning disable 618
			if (enforceCultureCheck && installationKind == Utility.InstallationKind.Classic && languagePack.DatabaseFieldIndex < 0)
#pragma warning restore 618
				throw new CultureNotSupportedException(languagePack.Culture);

			mpqFileSystem = new MpqFileSystem();

			foreach (string archive in languagePack.Archives)
				mpqFileSystem.Archives.Add(new MpqArchive(IOPath.Combine(languagePack.Path, archive), parseListFiles));

			foreach (string archive in archiveArray)
				mpqFileSystem.Archives.Add(new MpqArchive(IOPath.Combine(dataPath, archive), parseListFiles));

			return mpqFileSystem;
		}

		/// <summary>Gets the path of this WoW installation.</summary>
		public string Path { get { return wowPath; } }
		/// <summary>Gets the path to the data associated with the installation.</summary>
		public string DataPath { get { return dataPath; } }
		/// <summary>Gets a collection of language packs associated with the installation.</summary>
		public LanguagePackCollection LanguagePacks { get { return languagePackCollection; } }
		/// <summary>Gets a collection of string containing the names of the archives detected as part of the installation.</summary>
		public ReadOnlyCollection<string> Archives { get { return archiveCollection; } }
		/// <summary>Gets a value representing the installation kind. </summary>
		/// <remarks>This value is useful to differenciate classic installations from newer installations (Cataclysm or newer).</remarks>
		/// <value>The kind of the installation.</value>
		public InstallationKind InstallationKind { get { return installationKind; } }
	}
}
