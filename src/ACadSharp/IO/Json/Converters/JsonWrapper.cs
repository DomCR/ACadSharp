#if NET

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json.Converters;

internal static class JsonWrapper
{
	public static void WriteStringValue(
#if NET
		this Utf8JsonWriter writer,
#else
		this JsonWriter writer,
#endif
		string name,
		string value
	)
	{
#if NET
		writer.WriteString(name, value);
#else
		writer.WritePropertyName(name);
		writer.WriteValue(value);
#endif
	}

	public static void WriteNumberValue(
#if NET
	this Utf8JsonWriter writer,
#else
		this JsonWriter writer,
#endif
	string name,
	ulong value
)
	{
#if NET
		writer.WriteNumber(name, value);
#else
		writer.WritePropertyName(name);
		writer.WriteValue(value);
#endif
	}
}

