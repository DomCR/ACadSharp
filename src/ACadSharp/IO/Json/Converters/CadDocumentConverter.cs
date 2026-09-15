using ACadSharp.Header;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACadSharp.IO.Json.Converters;

public class CadDocumentConverter : JsonConverter<CadDocument>
{
	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, CadDocument value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(nameof(CadDocument.Header));
		JsonSerializer.Serialize(writer, value.Header, options);

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

		writer.WriteEndObject();
	}

	public override CadDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		CadDocument document = new CadDocument(false);
		JsonDocumentBuilder builder = new JsonDocumentBuilder(document);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string propertyName = reader.GetString();
				var prop = typeToConvert.GetProperty(propertyName);
				this.readPropertyValue(document, prop, ref reader, options);
			}
			else if (reader.TokenType == JsonTokenType.EndObject)
			{
				return document;
			}
		}

		throw new JsonException("Expected EndObject token.");
	}

	protected void readPropertyValue(CadDocument doc, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		switch (prop.Name)
		{
			case nameof(CadDocument.Header):
				doc.Header = this.readHeader(ref reader, options);
				break;
			default:
				throw new InvalidOperationException();
		}
	}

	private CadHeader readHeader(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		CadHeader header = new CadHeader();

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string variableName = reader.GetString();
				header.SetValue(variableName, JsonSerializer.Deserialize(ref reader, header.GetValue(variableName).GetType(), options));
			}
			else if (reader.TokenType == JsonTokenType.EndObject)
			{
				return header;
			}
		}

		throw new JsonException("Expected EndObject token.");
	}
}
