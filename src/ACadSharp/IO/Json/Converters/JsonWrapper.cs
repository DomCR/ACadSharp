using System;
using System.Text.Json;

namespace ACadSharp.IO.Json.Converters;

internal static class JsonWrapper
{
	[Obsolete]
	public static void WriteStringValue(this Utf8JsonWriter writer,
		string name,
		string value
	)
	{
		writer.WriteString(name, value);
	}

	[Obsolete]
	public static void WriteNumberValue(
	this Utf8JsonWriter writer,
	string name,
	ulong value
)
	{
		writer.WriteNumber(name, value);
	}
}

