using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	internal class CadUtils
	{
		/// <summary>
		/// Get the version of the autocad drawing by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static ACadVersion GetVersionFromName(string name)
		{
			//Modify the format of the name
			string vname = name.Replace('.', '_').ToUpper();

			if (Enum.TryParse(vname, out ACadVersion version))
				return version;
			else
				return ACadVersion.Unknown;
		}
		public static Color CreateColorFromIndex(short index)
		{
			//TODO: get the color index form autocad 
			return new Color();
		}
	}
}
