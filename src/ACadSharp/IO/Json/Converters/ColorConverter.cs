using System;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
#endif
#if NET5_0

using CSUtilities.Extensions;

#endif

namespace ACadSharp.IO.Json.Converters;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="Color"/>.
/// </summary>
public class ColorConverter : JsonConverter<Color>
{
#if NET

	/// <inheritdoc/>
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteNumber(nameof(value.R), value.R);
		writer.WriteNumber(nameof(value.G), value.G);
		writer.WriteNumber(nameof(value.B), value.B);

		writer.WriteEndObject();
	}

#else
	/// <inheritdoc/>
	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(nameof(value.R));
		writer.WriteValue(value.R);
		writer.WritePropertyName(nameof(value.G));
		writer.WriteValue(value.G);
		writer.WritePropertyName(nameof(value.B));
		writer.WriteValue(value.B);

		writer.WriteEndObject();
	}
#endif
}
