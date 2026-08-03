using ACadSharp.Header;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACadSharp.IO.Json.Converters;

public class CadHeaderConverter : JsonConverter<CadHeader>
{
	/// <inheritdoc/>
	public override CadHeader Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, CadHeader value, JsonSerializerOptions options)
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
			JsonSerializer.Serialize(writer, pValue, pValue.GetType(), options);
		}

		writer.WriteEndObject();
	}
}