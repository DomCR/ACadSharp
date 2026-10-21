using System;
using System.Collections.Generic;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] XData string interning table.
/// GIS attribute exports contain the same XData string values (keys, coded values, layer names)
/// millions of times; sharing the immutable string instances by reference is semantically
/// invisible and collapses ~100M string objects into a few million unique ones, saving several
/// GB of memory. Enabled via <see cref="DxfReaderConfiguration.InternXDataStrings"/>.
/// </summary>
public static class DxfXDataInterning
{
	private static Dictionary<string, string> _table;

	/// <summary>
	/// Returns the shared instance for the given string (or the argument itself when no
	/// equivalent was seen before).
	/// </summary>
	public static string Intern(string s)
	{
		if (_table is null)
		{
			_table = new Dictionary<string, string>(StringComparer.Ordinal);
		}

		return _table.TryGetValue(s, out string existing) ? existing : (_table[s] = s);
	}

	/// <summary>
	/// Releases the intern table (call after reading, before writing, to free the table itself;
	/// the shared string instances stay alive via the XData records).
	/// </summary>
	public static void Clear()
	{
		_table = null;
	}
}
