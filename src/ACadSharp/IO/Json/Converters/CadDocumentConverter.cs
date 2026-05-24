using ACadSharp.Header;
using System;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json.Converters;

public class CadDocumentConverter : JsonConverter<CadDocument>
{
#if NET

	public override CadDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, CadDocument value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(nameof(CadDocument.AppIds));
		JsonSerializer.Serialize(writer, value.AppIds, options);

		writer.WritePropertyName(nameof(CadDocument.BlockRecords));
		JsonSerializer.Serialize(writer, value.BlockRecords, options);

		writer.WritePropertyName(nameof(CadDocument.DimensionStyles));
		JsonSerializer.Serialize(writer, value.DimensionStyles, options);

		writer.WritePropertyName(nameof(CadDocument.Layers));
		JsonSerializer.Serialize(writer, value.Layers, options);

		writer.WritePropertyName(nameof(CadDocument.LineTypes));
		JsonSerializer.Serialize(writer, value.LineTypes, options);

		writer.WritePropertyName(nameof(CadDocument.TextStyles));
		JsonSerializer.Serialize(writer, value.TextStyles, options);

		writer.WritePropertyName(nameof(CadDocument.RootDictionary));
		JsonSerializer.Serialize(writer, value.RootDictionary, options);

		writer.WritePropertyName(nameof(CadDocument.Header));
		JsonSerializer.Serialize(writer, value.Header, options);

		writer.WriteEndObject();
	}

#else
	public override CadDocument ReadJson(JsonReader reader, Type objectType, CadDocument existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, CadDocument value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
#endif
}
