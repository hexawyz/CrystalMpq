using System;
using System.Resources;

namespace CrystalMpq
{
	public static class ErrorMessages
	{
		private static readonly ResourceManager resourceManager = new ResourceManager("ErrorMessages", typeof(ErrorMessages).Assembly);

		public static string GetString(string name) { return resourceManager.GetString(name); }
	}
}
