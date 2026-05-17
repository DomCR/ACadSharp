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
		throw new NotImplementedException();
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