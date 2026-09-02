using System.Linq;

namespace ACadSharp.Extensions;

public static class INamedCadObjectExtensions
{
	public static readonly char[] InvalidCharacters = { '*', '\\', '/', ':', '?', '"', '<', '>', '|', ';', ',', '=', '`' };

	/// <summary>
	/// Check if the name of the object is valid for dxf format.
	/// </summary>
	/// <param name="namedCadObject"></param>
	/// <param name="version"></param>
	/// <returns></returns>
	public static bool HasValidDxfName(this INamedCadObject namedCadObject, ACadVersion version = ACadVersion.AC1032)
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

		if (namedCadObject.Name.IndexOf(InvalidCharacters[0]) > 0)
		{
			return false;
		}

		var invalidCharacters = InvalidCharacters.Skip(1);
		if (namedCadObject is Tables.TableEntry entry
			&& entry.Flags.HasFlag(Tables.StandardFlags.XrefDependent))
		{
			invalidCharacters = invalidCharacters.Where(c => c != '|');
		}

		return namedCadObject.Name.IndexOfAny(invalidCharacters.ToArray()) == -1;
	}
}
