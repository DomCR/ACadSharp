using System;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json.Converters;

public class CadConverterFactory :
#if NET
	JsonConverterFactory
#else
	Newtonsoft.Json.JsonConverter
#endif
{
	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsSubclassOf(typeof(CadObject))
			|| typeToConvert == typeof(CadDocument);
	}

#if NET

	/// <inheritdoc/>
	public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert == typeof(CadDocument))
		{
			return new CadDocumentConverter();
		}

		return new CommonCadConverter();
	}

#else
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (objectType == typeof(CadDocument))
			return new CadDocumentConverter().ReadJson(reader, objectType, existingValue, serializer);

		return new CommonCadConverter().ReadJson(reader, objectType, existingValue, serializer);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is CadDocument)
			new CadDocumentConverter().WriteJson(writer, value, serializer);
		else
			new CommonCadConverter().WriteJson(writer, value, serializer);
	}
#endif
}