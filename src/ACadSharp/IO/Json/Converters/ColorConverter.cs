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
		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException("Expected StartObject token.");
		}

		byte r = 0, g = 0, b = 0;

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return new Color(r, g, b);
			}

			if (reader.TokenType != JsonTokenType.PropertyName)
			{
				throw new JsonException("Expected PropertyName token.");
			}

			string propertyName = reader.GetString();
			reader.Read();

			switch (propertyName)
			{
				case nameof(Color.R):
					r = reader.GetByte();
					break;
				case nameof(Color.G):
					g = reader.GetByte();
					break;
				case nameof(Color.B):
					b = reader.GetByte();
					break;
			}
		}

		throw new JsonException("Expected EndObject token.");
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
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw new JsonSerializationException("Expected StartObject token.");
		}

		byte r = 0, g = 0, b = 0;

		while (reader.Read())
		{
			if (reader.TokenType == JsonToken.EndObject)
			{
				return new Color(r, g, b);
			}

			if (reader.TokenType != JsonToken.PropertyName)
			{
				throw new JsonSerializationException("Expected PropertyName token.");
			}

			string propertyName = (string)reader.Value;
			reader.Read();

			switch (propertyName)
			{
				case nameof(Color.R):
					r = Convert.ToByte(reader.Value);
					break;
				case nameof(Color.G):
					g = Convert.ToByte(reader.Value);
					break;
				case nameof(Color.B):
					b = Convert.ToByte(reader.Value);
					break;
			}
		}

		throw new JsonSerializationException("Expected EndObject token.");
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
