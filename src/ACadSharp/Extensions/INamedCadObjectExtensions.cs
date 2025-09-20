namespace ACadSharp.Extensions
{
	public static class INamedCadObjectExtensions
	{
		public static readonly char[] InvalidCharacters = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', ';', ',', '=', '`' };

		/// <summary>
		/// Check if the name of the object is valid for dxf format.
		/// </summary>
		/// <param name="namedCadObject"></param>
		/// <returns></returns>
		public static bool IsValidDxfName(this INamedCadObject namedCadObject)
		{
			if (string.IsNullOrEmpty(namedCadObject.Name))
			{
				return false;
			}

			return namedCadObject.Name.IndexOfAny(InvalidCharacters) == -1;
		}
	}
}
