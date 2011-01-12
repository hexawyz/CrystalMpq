#region Copyright Notice
// This file is part of CrystalMPQ.
// 
// Copyright (C) 2007-2011 Fabien BARBIER
// 
// CrystalMPQ is licenced under the Microsoft Reciprocal License.
// You should find the licence included with the source of the program,
// or at this URL: http://www.microsoft.com/opensource/licenses.mspx#Ms-RL
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using CrystalMpq;
using CrystalMpq.Utility;
using CrystalMpq.WoWFile;
using CrystalMpq.WoWDatabases;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;
#endregion

namespace WoWMapExplorer
{
	public partial class MainForm : Form
	{
		#region Map Information Structures

		class Continent
		{
			int id;
			int mapId;
			string name;
			string dataName;
			RectangleF bounds;
			List<Zone> zones;
			ZoneMap zoneMap;

			public Continent(int id, int mapId, RectangleF bounds, string name, string dataName)
			{
				this.id = id;
				this.mapId = mapId;
				this.name = name;
				this.bounds = bounds;
				this.dataName = dataName;
				this.zones = new List<Zone>();

			}

			public int Id { get { return id; } set { id = value; } }
			public int MapId { get { return mapId; } set { mapId = value; } }
			public string Name { get { return name; } set { name = value; } }
			public string DataName { get { return dataName; } set { dataName = value; } }
			public RectangleF Bounds { get { return bounds; } set { bounds = value; } }
			public List<Zone> Zones { get { return zones; } }
			public ZoneMap ZoneMap { get { return zoneMap; } set { zoneMap = value; } }

			public override string ToString() { return name; }
		}

		class Zone
		{
			int id;
			int areaId;
			int mapId;
			string name;
			string dataName;
			RectangleF bounds;
			List<Overlay> overlays;

			public Zone(int id, int areaId, int mapId, RectangleF bounds, string name, string dataName)
			{
				this.id = id;
				this.areaId = areaId;
				this.mapId = mapId;
				this.bounds = bounds;
				this.name = name;
				this.dataName = dataName;
				this.overlays = new List<Overlay>();
			}

			public int Id { get { return id; } set { id = value; } }
			public int AreaId { get { return areaId; } set { areaId = value; } }
			public int MapId { get { return mapId; } set { mapId = value; } }
			public string Name { get { return name; } set { name = value; } }
			public string DataName { get { return dataName; } set { dataName = value; } }
			public RectangleF Bounds { get { return bounds; } set { bounds = value; } }
			public List<Overlay> Overlays { get { return overlays; } }

			public override string ToString() { return name; }
		}

		class Overlay
		{
			int id;
			int zoneId;
			int areaId;
			Rectangle bounds;
			Rectangle boundingRectangle;
			string name;
			string dataName;

			public Overlay(int id, int zoneId, int areaId, Rectangle bounds, Rectangle boundingRectangle, string name, string dataName)
			{
				this.id = id;
				this.zoneId = zoneId;
				this.areaId = areaId;
				this.bounds = bounds;
				this.boundingRectangle = boundingRectangle;
				this.name = name;
				this.dataName = dataName;
			}

			public int Id { get { return id; } set { id = value; } }
			public int ZoneId { get { return zoneId; } set { zoneId = value; } }
			public int AreaId { get { return areaId; } set { areaId = value; } }
			public Rectangle Bounds { get { return bounds; } set { bounds = value; } }
			public Rectangle BoundingRectangle { get { return boundingRectangle; } set { boundingRectangle = value; } }
			public string Name { get { return name; } set { name = value; } }
			public string DataName { get { return dataName; } set { dataName = value; } }

			public override string ToString() { return name; }
		}

		#endregion

		#region Fields

		// Constants
		const int databaseLocaleFieldCount = 16;
		// MPQ Archives
		WoWInstallation wowInstallation;
		LanguagePack languagePack;
		MpqFileSystem mpqFileSystem;
		// Font for displaying zone information
		PrivateFontCollection wowFontCollection;
		Brush zoneInformationBrush;
		Pen zoneInformationPen;
		Font zoneInformationFont;
		const int zoneInformationFontHeight = 40;
		List<IntPtr> fontPointerList;
		// World Map Size: 1002x668
		Bitmap mapBitmap;
		Bitmap outlandHighlightBitmap,
			azerothHighlightBitmap;
		Rectangle outlandButtonBounds,
			azerothButtonBounds;
		bool outlandHighlighted,
			azerothHighlighted;
		// Databases
		KeyedClientDatabase<int, MapRecord> mapDatabase;
		KeyedClientDatabase<int, WorldMapContinentRecord> worldMapContinentDatabase;
		KeyedClientDatabase<int, WorldMapAreaRecord> worldMapAreaDatabase;
		KeyedClientDatabase<int, AreaTableRecord> areaTableDatabase;
		KeyedClientDatabase<int, WorldMapOverlayRecord> worldMapOverlayDatabase;
		KeyedClientDatabase<int, DungeonMapRecord> dungeonMapDatabase;
		// Status
		int currentContinent, currentZone;
		List<Continent> continents;
		List<Zone> zones;
		List<Overlay> overlays;
		string zoneInformationText;

		#endregion

		#region Constructor & Destructor

		public MainForm(WoWInstallation wowInstallation, LanguagePack languagePack)
		{
			InitializeComponent();
			//
			this.wowInstallation = wowInstallation;
			this.languagePack = languagePack;
			// Create resources for drawing text
			zoneInformationBrush = Brushes.White;
			zoneInformationPen = new Pen(Color.Black, 5);
			// Creates the bitmap
			mapBitmap = new Bitmap(1002, 668, PixelFormat.Format32bppRgb);
			Size = new Size(this.Width - renderPanel.ClientSize.Width + 1002, this.Height - renderPanel.ClientSize.Height + 668);
			renderPanel.BackgroundImage = mapBitmap;
			// Initialization
			InitializeFileSystem();
			LoadDatabases();
			LoadFonts();
			currentContinent = -1;
			InitializeMapData();
			FillContinents();
			FillZones();
			LoadCosmicHighlights();
			UpdateMap();
		}

		~MainForm()
		{
			foreach (IntPtr pointer in fontPointerList)
				Marshal.FreeHGlobal(pointer);
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		private void InitializeFileSystem()
		{
			// Create a new instance
			mpqFileSystem = wowInstallation.CreateFileSystem(languagePack, false, false);
		}

		private void LoadDatabases()
		{
			mapDatabase = LoadDatabase<MapRecord>(@"DBFilesClient\Map.dbc");
			worldMapContinentDatabase = LoadDatabase<WorldMapContinentRecord>(@"DBFilesClient\WorldMapContinent.dbc");
			worldMapAreaDatabase = LoadDatabase<WorldMapAreaRecord>(@"DBFilesClient\WorldMapArea.dbc");
			areaTableDatabase = LoadDatabase<AreaTableRecord>(@"DBFilesClient\AreaTable.dbc");
			worldMapOverlayDatabase = LoadDatabase<WorldMapOverlayRecord>(@"DBFilesClient\WorldMapOverlay.dbc");
			dungeonMapDatabase = LoadDatabase<DungeonMapRecord>(@"DBFilesClient\DungeonMap.dbc");
		}

		private void LoadFonts()
		{
			FontFamily family = LoadFont(@"Fonts\FRIZQT__.TTF");

			if (family != null)
				try { zoneInformationFont = new Font(family, zoneInformationFontHeight, FontStyle.Regular, GraphicsUnit.Pixel, 0); }
				catch { family.Dispose(); family = null; }
			if (family == null)
				zoneInformationFont = new Font("Arial Narrow", zoneInformationFontHeight, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, 0);
		}

		private void LoadCosmicHighlights()
		{
			using (BLPTexture texture = LoadTexture(@"Interface\WorldMap\Cosmic\Cosmic-Outland-Highlight.blp"))
				outlandHighlightBitmap = new Bitmap(texture.FirstMipMap, 856, 605);
			outlandButtonBounds = new Rectangle(115, 90, 320, 320);
			using (BLPTexture texture = LoadTexture(@"Interface\WorldMap\Cosmic\Cosmic-Azeroth-Highlight.blp"))
				azerothHighlightBitmap = new Bitmap(texture.FirstMipMap, 898, 647);
			azerothButtonBounds = new Rectangle(593, 255, 366, 366);
		}

		#region Generic Data Loading Functions

		private KeyedClientDatabase<int, T> LoadDatabase<T>(string filename) where T : struct
		{
			return LoadDatabase<int, T>(filename);
		}

		private KeyedClientDatabase<TKey, TValue> LoadDatabase<TKey, TValue>(string filename) where TValue : struct
		{
			MpqFile file;
			Stream fileStream = null;

			if ((file = mpqFileSystem.FindFile(filename)) != null)
				using (fileStream = file.Open())
					return new KeyedClientDatabase<TKey, TValue>(fileStream, languagePack.DatabaseFieldIndex);
			else
				return null;
		}

		private FontFamily LoadFont(string filename)
		{
			MpqFile fontFile;
			Stream stream;
			byte[] buffer;

			fontPointerList = new List<IntPtr>();
			wowFontCollection = new PrivateFontCollection();
			try
			{
				// Open the file
				fontFile = mpqFileSystem.FindFile(filename);
				// Read the contents of the file
				stream = fontFile.Open();
				buffer = new byte[fontFile.Size]; // Allocate the read buffer
				stream.Read(buffer, 0, (int)fontFile.Size);
				stream.Close();
				IntPtr fontPointer = Marshal.AllocHGlobal((int)fontFile.Size);
				Marshal.Copy(buffer, 0, fontPointer, (int)fontFile.Size);
				fontPointerList.Add(fontPointer); // Add the pointer to the list
				// Finally add the font
				wowFontCollection.AddMemoryFont(fontPointer, (int)fontFile.Size);
				// Return the result
				return wowFontCollection.Families[wowFontCollection.Families.Length - 1];
			}
			catch { return null; }
		}

		private BLPTexture LoadTexture(string filename)
		{
			MpqFile file = mpqFileSystem.FindFile(filename);
			Stream stream = null;
			BLPTexture texture;

			if (file == null)
				return null;
			try
			{
				stream = file.Open();
				texture = new BLPTexture(stream, false);
				stream.Close();
				return texture;
			}
			catch
			{
				if (stream != null)
					stream.Close();
				return null;
			}
		}

		private ZoneMap LoadZoneMap(string filename)
		{
			MpqFile file = mpqFileSystem.FindFile(filename);
			Stream stream = null;
			ZoneMap zoneMap;

			if (file == null)
				return null;
			try
			{
				stream = file.Open();
				zoneMap = new ZoneMap(stream);
				stream.Close();
#if DEBUG
				if (zoneMap != null)
				{
					Bitmap b = new Bitmap(128, 128);

					for (int i = 0; i < 128; i++)
						for (int j = 0; j < 128; j++)
							b.SetPixel(i, j, Color.FromArgb(zoneMap[i, j] | ~0xFFFFFF));

					b.Save(Path.GetFileNameWithoutExtension(filename) + ".bmp");
				}
#endif
				return zoneMap;
			}
			catch
			{
				if (stream != null)
					stream.Close();
				return null;
			}
		}
		#endregion

		#region Data Processing Functions

		private void InitializeMapData()
		{
			continents = new List<Continent>();

			LoadContinents();
		}

		private void LoadContinents()
		{
			foreach (var worldMapContinentRecord in worldMapContinentDatabase.Records)
			{
				MapRecord mapRecord;

				if (mapDatabase.TryGetValue(worldMapContinentRecord.Map, out mapRecord))
				{
					Continent continent = new Continent(worldMapContinentRecord.Id, worldMapContinentRecord.Map, RectangleF.Empty, mapRecord.Name, mapRecord.DataName);

					continent.ZoneMap = LoadZoneMap(@"Interface\WorldMap\" + continent.DataName + ".zmp");

					LoadZones(continent);

					continents.Add(continent);
				}
			}
		}

		private void LoadZones(Continent continent)
		{
			// Look for zones matching this map ID in WorldMapArea database
			foreach (var worldMapAreaRecord in worldMapAreaDatabase.Records)
			{
				// Zone information stored as following
				//  In field 1: ID of the game map containing the zone
				//  In field 8: ID of the map virtually containing the zone (-1 if it is the same as field 1)

				if ((worldMapAreaRecord.VirtualMap == -1 && worldMapAreaRecord.Map == continent.MapId) // Either virtual ID is -1 and we have ID in field 1
					|| worldMapAreaRecord.VirtualMap == continent.MapId) // Or we have ID in field 8
				{
					Zone zone = new Zone(worldMapAreaRecord.Id, worldMapAreaRecord.Area, continent.MapId,
									new RectangleF(worldMapAreaRecord.BoxLeft, worldMapAreaRecord.BoxTop, worldMapAreaRecord.BoxRight - worldMapAreaRecord.BoxLeft, worldMapAreaRecord.BoxBottom - worldMapAreaRecord.BoxTop),
									null, worldMapAreaRecord.DataName);

					if (worldMapAreaRecord.Area == 0) // For continents
						continent.Bounds = zone.Bounds;
					else // Now look into AreaTable database
					{
						AreaTableRecord areaTableRecord;

						if (areaTableDatabase.TryGetValue(worldMapAreaRecord.Area, out areaTableRecord))
						{
							// Get the localized area name and set it as zone name
							zone.Name = areaTableRecord.Name;
#if DEBUG
							System.Diagnostics.Debug.WriteLine("Bounds of \"" + zone.Name + "\": " + zone.Bounds.ToString());
#endif
							// Load overlays for this zone
							LoadOverlays(zone);
							// Add zone to the list
							continent.Zones.Add(zone);
						}
					}
				}
			}
		}

		private void LoadOverlays(Zone zone)
		{
			foreach (var overlayRecord in worldMapOverlayDatabase.Records)
				if (overlayRecord.WorldMapArea == zone.Id)
				{
					Rectangle bounds = new Rectangle(overlayRecord.Left, overlayRecord.Top, overlayRecord.Width, overlayRecord.Height),
						boundingRectangle = new Rectangle(overlayRecord.BoxLeft, overlayRecord.BoxTop, overlayRecord.BoxRight - overlayRecord.BoxLeft, overlayRecord.BoxBottom - overlayRecord.BoxTop);

					Overlay overlay = new Overlay(overlayRecord.Id, overlayRecord.WorldMapArea, overlayRecord.Area1,
						bounds, boundingRectangle,
						null, overlayRecord.DataName);

					AreaTableRecord areaTableRecord;

					if (areaTableDatabase.TryGetValue(overlayRecord.Area1, out areaTableRecord)) // Find the overlay name
						overlay.Name = areaTableRecord.Name;

					zone.Overlays.Add(overlay);
				}
		}
		#endregion

		private void FillContinents()
		{
			foreach (Continent continent in continents)
				continentToolStripComboBox.Items.Add(continent);
		}

		private void FillZones()
		{
			zoneToolStripComboBox.Items.Clear();

			currentZone = -1;

			if (currentContinent < 1)
				return;

			foreach (Zone zone in continents[currentContinent - 1].Zones)
				zoneToolStripComboBox.Items.Add(zone);
		}

		private void UpdateMap()
		{
			Graphics g = Graphics.FromImage(mapBitmap);
			string path = @"Interface\WorldMap\";
			string map = "";
			BLPTexture texture;

			zoneInformationText = "";
			overlays = null;
			if (currentContinent == -1)
				map = @"Cosmic";
			else if (currentContinent == 0)
				map = @"World";
			else if (currentZone == -1)
				map = ((Continent)continentToolStripComboBox.SelectedItem).DataName;
			else
				map = zones[currentZone].DataName;
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 4; j++)
				{
					texture = LoadTexture(String.Format(CultureInfo.InvariantCulture, @"{0}{1}\{1}{2}.blp", path, map, 4 * i + j + 1));
					g.DrawImageUnscaled(texture.FirstMipMap, 256 * j, 256 * i, 256, 256);
					texture.Dispose();
				}
			if (currentContinent > 0 && currentZone >= 0)
			{
				overlays = zones[currentZone].Overlays;

				foreach (Overlay overlay in overlays)
				{
					int x = overlay.Bounds.X,
						y = overlay.Bounds.Y,
						width = overlay.Bounds.Width,
						height = overlay.Bounds.Height,
						rowCount, colCount,
						textureCount;

					if (height > 256)
						rowCount = (height + 255) / 256;
					else
						rowCount = 1;
					if (width > 256)
						colCount = (width + 255) / 256;
					else
						colCount = 1;

					textureCount = rowCount * colCount;

					for (int i = 0; i < textureCount; i++)
					{
						texture = LoadTexture(String.Format(CultureInfo.InvariantCulture, @"{0}{1}\{2}{3}.blp", path, map, overlay.DataName, i + 1));
						if (texture != null)
						{
							g.DrawImageUnscaled(texture.FirstMipMap, x + 256 * (i % colCount), y + 256 * (i / colCount));
							texture.Dispose();
						}
					}
				}
			}
			//else if (currentContinent == -1)
			//{
			//    if (outlandHighlighted)
			//        g.DrawImageUnscaled(outlandHighlightBitmap, 23, 35);
			//    else if (azerothHighlighted)
			//        g.DrawImageUnscaled(azerothHighlightBitmap, 103, 11);
			//}
			g.Dispose();
		}

		private void renderPanel_Paint(object sender, PaintEventArgs e)
		{
			SizeF infoTextSize;
			GraphicsPath graphicsPath;

			//e.Graphics.DrawImageUnscaled(mapBitmap, new Rectangle(0, 0, 1002, 668));
			if (zoneInformationText != null && zoneInformationText.Length > 0)
			{
				graphicsPath = new GraphicsPath();
				infoTextSize = e.Graphics.MeasureString(zoneInformationText, zoneInformationFont);
				graphicsPath.AddString(zoneInformationText,
					zoneInformationFont.FontFamily, (int)zoneInformationFont.Style, zoneInformationFontHeight,
					new Point((int)((1002 - infoTextSize.Width) / 2), 10),
					new StringFormat());
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.DrawPath(zoneInformationPen, graphicsPath);
				graphicsPath.Transform(new Matrix(1, 0, 0, 1, -3, -3));
				e.Graphics.DrawPath(zoneInformationPen, graphicsPath);
				e.Graphics.FillPath(zoneInformationBrush, graphicsPath);
				graphicsPath.Dispose();
			}
			if (currentContinent == -1)
			{
				if (outlandHighlighted)
					e.Graphics.DrawImageUnscaled(outlandHighlightBitmap, 23, 35);
				if (azerothHighlighted)
					e.Graphics.DrawImageUnscaled(azerothHighlightBitmap, 103, 11);
#if DEBUG
				e.Graphics.DrawRectangle(Pens.Red, 115, 90, 320, 320);
				e.Graphics.DrawRectangle(Pens.Red, 593, 255, 366, 366);
#endif
			}
		}

		private void continentToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Continent continent = continentToolStripComboBox.SelectedItem as Continent;
			int newContinent = continents.IndexOf(continent) + 1;

			if (newContinent > 0 && newContinent != currentContinent)
				SetZoomLevel(newContinent, -1);
		}

		private void zoneToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Zone zone = zoneToolStripComboBox.SelectedItem as Zone;
			int zoneIndex = zones.IndexOf(zone);

			if (currentZone != zoneIndex)
				SetZoomLevel(currentContinent, zoneIndex);
		}

		private void zoomOutToolStripButton_Click(object sender, EventArgs e)
		{
			ZoomOut();
		}

		private void ZoomOut()
		{
			if (currentContinent <= 0)
			{
				currentContinent = -1;
				currentZone = -1;
			}
			else if (currentZone >= 0)
				currentZone = -1;
			else if (currentContinent == 3)
				currentContinent = -1;
			else
				currentContinent = 0;
			SetZoomLevel(currentContinent, currentZone);
		}

		private void SetZoomLevel(int continent, int zone)
		{
			if (continent < -1 || continent > continentToolStripComboBox.Items.Count)
				continent = -1;
			if (continent <= 0)
			{
				currentContinent = continent;
				currentZone = -1;
				continentToolStripComboBox.SelectedIndex = -1;
				FillZones();
			}
			else
			{
				if (currentContinent != continent)
				{
					currentContinent = continent;
					continentToolStripComboBox.SelectedItem = continents[currentContinent - 1];
					zones = continents[currentContinent - 1].Zones;
					FillZones();
				}
				if (zone <= -1 || zone >= zones.Count)
				{
					currentZone = -1;
					zoneToolStripComboBox.SelectedIndex = -1;
				}
				else
				{
					currentZone = zone;
					zoneToolStripComboBox.SelectedItem = zones[zone];
				}
			}
			UpdateMap();
			renderPanel.Invalidate();
		}

		private Zone GetZone(Point position)
		{
			Zone foundZone = null;

			if (currentContinent > 0 && currentZone == -1)
			{
				Continent continent = continents[currentContinent - 1];
				float x = continent.Bounds.Left,
					y = continent.Bounds.Top,
					width = continent.Bounds.Width,
					height = continent.Bounds.Height;
				float xPos = (1002 - position.X) * width / 1002 + x,
					yPos = (668 - position.Y) * height / 668 + y;

				//for (int i = 0; i < zones.Count; i++)
				for (int i = zones.Count - 1; i >= 0; i--)
				{
					Zone zone = zones[i];

					if (xPos >= zone.Bounds.Left && xPos < zone.Bounds.Right
						&& yPos >= zone.Bounds.Top && yPos < zone.Bounds.Bottom
						&& (foundZone == null || zone.AreaId > foundZone.AreaId))
						foundZone = zone;
				}
//                int zoneId = continent.ZoneMap[127 * position.X / 1002, 127 * position.Y / 668];

//                foreach (Zone zone in zones)
//                    if (zone.AreaId == zoneId)
//                        return zone;
//#if DEBUG
//                System.Diagnostics.Debug.WriteLine(zoneId);
//#endif
			}
			return foundZone;
		}

		private void renderPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (currentContinent == -1)
			{
				bool needUpdate = false;

				if (e.X >= outlandButtonBounds.X && e.X < outlandButtonBounds.Right
					&& e.Y >= outlandButtonBounds.Y && e.Y < outlandButtonBounds.Bottom)
				{
					if (!outlandHighlighted)
					{
						outlandHighlighted = true;
						needUpdate = true;
					}
				}
				else
				{
					if (outlandHighlighted)
					{
						outlandHighlighted = false;
						needUpdate = true;
					}
				}

				if (e.X >= azerothButtonBounds.X && e.X < azerothButtonBounds.Right
					&& e.Y >= azerothButtonBounds.Y && e.Y < azerothButtonBounds.Bottom)
				{
					if (!azerothHighlighted)
					{
						azerothHighlighted = true;
						needUpdate = true;
					}
				}
				else
				{
					if (azerothHighlighted)
					{
						azerothHighlighted = false;
						needUpdate = true;
					}
				}

				if (needUpdate)
				{
					//UpdateMap();
					renderPanel.Invalidate();
				}
			}
			else if (currentContinent > 0)
			{
				if (currentZone == -1)
				{
					Zone zone = GetZone(e.Location);

					if (zone != null)
					{
						if (zoneInformationText != zone.Name)
						{
							zoneInformationText = zone.Name;
							renderPanel.Invalidate();
						}
						return;
					}
				}
				else if (overlays != null)
				{
					foreach (Overlay overlay in overlays)
					{
						if (e.X >= overlay.BoundingRectangle.Left && e.X < overlay.BoundingRectangle.Right
							&& e.Y >= overlay.BoundingRectangle.Top && e.Y < overlay.BoundingRectangle.Bottom)
						{
							if (zoneInformationText != overlay.Name)
							{
								zoneInformationText = overlay.Name;
								renderPanel.Invalidate();
							}
							return;
						}
					}
				}
			}
			//if (zoneInformationText != null && zoneInformationText.Length > 0)
			//    renderPanel.Invalidate();
			zoneInformationText = null;
		}

		private void renderPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (currentContinent == -1)
				{
					if (e.X >= outlandButtonBounds.X && e.X < outlandButtonBounds.Right
						&& e.Y >= outlandButtonBounds.Y && e.Y < outlandButtonBounds.Bottom)
						SetZoomLevel(3, -1);
					else if (e.X >= azerothButtonBounds.X && e.X < azerothButtonBounds.Right
					&& e.Y >= azerothButtonBounds.Y && e.Y < azerothButtonBounds.Bottom)
						SetZoomLevel(0, -1);
				}
				else
				{
					Zone zone = GetZone(e.Location);

					if (zone != null)
						SetZoomLevel(currentContinent, zones.IndexOf(zone));
				}
			}
			else if (e.Button == MouseButtons.Right)
				ZoomOut();
		}
	}
}