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
using CrystalMpq;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace CrystalMpq.Explorer
{
	class Program
	{
		internal static string FormatFileSize(long size)
		{
			CultureInfo currentCulture = Properties.Resources.Culture;
			double currentValue = size;
			string[] formatStrings = new string[] {
				Properties.Resources.UnitBytesFormat,
				Properties.Resources.UnitKiloByteFormat,
				Properties.Resources.UnitMegaByteFormat,
				Properties.Resources.UnitGigaByteFormat,
				Properties.Resources.UnitTeraByteFormat };
			int formatIndex = 0;

			// Special case
			if (size == 1)
				return Properties.Resources.UnitByteFormat;

			while (currentValue >= 1024 && formatIndex < formatStrings.Length)
			{
				formatIndex++;
				currentValue /= 1024;
			}

			return string.Format(currentCulture, formatStrings[formatIndex], currentValue);
		}

		[STAThread]
		static void Main(string[] args)
		{
			////MPQArchive archive = new MPQArchive("C:\\Program Files\\Warcraft III\\war3x.mpq");
			//MPQArchive archive = new MPQArchive("C:\\Program Files\\World of Warcraft\\Data\\patch.MPQ");
			////MPQArchive archive = new MPQArchive("C:\\Program Files\\Diablo II\\d2data.mpq");
			////MPQArchive archive = new MPQArchive("C:\\Program Files\\Diablo II\\Patch_D2.mpq");

			//StreamReader reader;
			//Console.WriteLine("The archive contains {0} files", archive.Files.Count);
			//foreach (MPQFile file in archive.Files)
			//    Console.WriteLine(file.Filename);
			////reader = new StreamReader(archive.FindFile("Styles.css").Open());
			////Console.WriteLine(reader.ReadToEnd());

			PluginManager.LoadPluginAssemblies();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
