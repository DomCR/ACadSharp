using ACadSharp.Header;
using System;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json.Converters;

public class CadHeaderConverter : JsonConverter<CadHeader>
{

	/// <inheritdoc/>
#if NET
	public override void Write(Utf8JsonWriter writer, CadHeader value, JsonSerializerOptions options)
#else
	public override void WriteJson(JsonWriter writer, CadHeader value, JsonSerializer serializer)
#endif
	{
		writer.WriteStartObject();

		foreach (var item in CadHeader.GetHeaderMap().Keys)
		{
			var pValue = value.GetValue(item);
			if (pValue == null)
			{
				continue;
			}

			writer.WritePropertyName(item);
#if NET
			JsonSerializer.Serialize(writer, pValue, pValue.GetType(), options);
#else
			serializer.Serialize(writer, pValue);
#endif
		}

		writer.WriteEndObject();
	}

	/// <inheritdoc/>
#if NET
	public override CadHeader Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#else
	public override CadHeader ReadJson(JsonReader reader, Type objectType, CadHeader existingValue, bool hasExistingValue, JsonSerializer serializer)
#endif
	{
		throw new NotImplementedException();
	}
}