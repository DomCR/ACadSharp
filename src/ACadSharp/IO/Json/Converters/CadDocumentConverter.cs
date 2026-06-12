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
	/// <inheritdoc/>
#if NET
	public override void Write(Utf8JsonWriter writer, CadDocument value, JsonSerializerOptions options)
#else
	public override void WriteJson(JsonWriter writer, CadDocument value, JsonSerializer serializer)
#endif
	{
		writer.WriteStartObject();

		writer.WritePropertyName(nameof(CadDocument.AppIds));
#if NET
		JsonSerializer.Serialize(writer, value.AppIds, options);
#else
		serializer.Serialize(writer, value.AppIds);
#endif

		writer.WritePropertyName(nameof(CadDocument.BlockRecords));
#if NET
		JsonSerializer.Serialize(writer, value.BlockRecords, options);
#else
		serializer.Serialize(writer, value.BlockRecords);
#endif

		writer.WritePropertyName(nameof(CadDocument.DimensionStyles));
#if NET
		JsonSerializer.Serialize(writer, value.DimensionStyles, options);
#else
		serializer.Serialize(writer, value.DimensionStyles);
#endif

		writer.WritePropertyName(nameof(CadDocument.Layers));
#if NET
		JsonSerializer.Serialize(writer, value.Layers, options);
#else
		serializer.Serialize(writer, value.Layers);
#endif

		writer.WritePropertyName(nameof(CadDocument.LineTypes));
#if NET
		JsonSerializer.Serialize(writer, value.LineTypes, options);
#else
		serializer.Serialize(writer, value.LineTypes);
#endif

		writer.WritePropertyName(nameof(CadDocument.TextStyles));
#if NET
		JsonSerializer.Serialize(writer, value.TextStyles, options);
#else
		serializer.Serialize(writer, value.TextStyles);
#endif

		writer.WritePropertyName(nameof(CadDocument.RootDictionary));
#if NET
		JsonSerializer.Serialize(writer, value.RootDictionary, options);
#else
		serializer.Serialize(writer, value.RootDictionary);
#endif

		writer.WritePropertyName(nameof(CadDocument.Header));
#if NET
		JsonSerializer.Serialize(writer, value.Header, options);
#else
		serializer.Serialize(writer, value.Header);
#endif

		writer.WriteEndObject();
	}

#if NET

	public override CadDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

#else
	public override CadDocument ReadJson(JsonReader reader, Type objectType, CadDocument existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
#endif
}
