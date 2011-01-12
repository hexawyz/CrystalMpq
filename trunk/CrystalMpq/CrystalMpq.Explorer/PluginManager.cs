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
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using CrystalMpq.Explorer.Properties;

namespace CrystalMpq.Explorer
{
	sealed class PluginManager
	{
		static List<Assembly> assemblyList;

		static PluginManager()
		{
			assemblyList = new List<Assembly>();
		}

		public static void LoadPluginAssemblies()
		{
			string assemblyDirectory, pluginsDirectory;

			assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			pluginsDirectory = Path.Combine(assemblyDirectory, Settings.Default.PluginsDirectory);

			LoadDirectoryAssemblies(pluginsDirectory);
		}

		public static T[] LoadPlugins<T>(Type[] parameterTypes, object[] parameters)
		{
			Type[] assemblyTypes;
			List<T> loadedPlugins;

			loadedPlugins = new List<T>();

			for (int i = 0; i < assemblyList.Count; i++)
			{
				assemblyTypes = assemblyList[i].GetExportedTypes();

				for (int j = 0; j < assemblyTypes.Length; j++)
				{
					Type type = assemblyTypes[j];

					if (typeof(T).IsAssignableFrom(type))
					{
						ConstructorInfo constructor = type.GetConstructor(parameterTypes);

						if (constructor != null)
							try { loadedPlugins.Add((T)constructor.Invoke(parameters)); }
							catch { }
					}
				}
			}

			return loadedPlugins.ToArray();
		}

		static bool LoadDirectoryAssemblies(string directory)
		{
			string[] assemblyFiles;

			try
			{
				assemblyFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);

				for (int i = 0; i < assemblyFiles.Length; i++)
					assemblyList.Add(LoadPluginAssembly(assemblyFiles[i]));
				return true;
			}
			catch
			{
				return false;
			}
		}

		static Assembly LoadPluginAssembly(string filename)
		{
			return Assembly.LoadFrom(filename);
		}
	}
}
