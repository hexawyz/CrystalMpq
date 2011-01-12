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
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace CrystalMpq.Utility
{
	public sealed class CultureNotSupportedException : NotSupportedException
	{
		CultureInfo cultureInfo;

		public CultureNotSupportedException(CultureInfo culture)
			: base(string.Format(Properties.Resources.Culture, Properties.Resources.UnsupportedCultureMessage, culture.DisplayName, culture.Name))
		{
			this.cultureInfo = culture;
		}

		public CultureNotSupportedException(CultureInfo culture, Exception innerException)
			: base(string.Format(Properties.Resources.Culture, Properties.Resources.UnsupportedCultureMessage, culture.DisplayName, culture.Name), innerException)
		{
			this.cultureInfo = culture;
		}

		public CultureInfo Culture
		{
			get
			{
				return cultureInfo;
			}
		}
	}
}
