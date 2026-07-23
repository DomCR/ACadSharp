using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACadSharp.IO.Json.Converters;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="Color"/>.
/// </summary>
public class ColorConverter : JsonConverter<Color>
{
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
}
