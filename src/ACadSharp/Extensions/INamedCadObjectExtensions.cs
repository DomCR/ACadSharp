namespace ACadSharp.Extensions
{
	public static class INamedCadObjectExtensions
	{
		public static readonly char[] InvalidCharacters = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', ';', ',', '=', '`' };

		/// <summary>
		/// Check if the name of the object is valid for dxf format.
		/// </summary>
		/// <param name="namedCadObject"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public static bool IsValidDxfName(this INamedCadObject namedCadObject, ACadVersion version = ACadVersion.AC1032)
		{
			if (string.IsNullOrEmpty(namedCadObject.Name))
			{
				return false;
			}

			if (version <= ACadVersion.AC1015 && namedCadObject.Name.Length > 31)
			{
				return false;
			}
			else if (namedCadObject.Name.Length > 255)
			{
				return false;
			}

			return namedCadObject.Name.IndexOfAny(InvalidCharacters) == -1;
		}
	}
}
