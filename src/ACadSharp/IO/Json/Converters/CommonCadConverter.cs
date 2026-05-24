using System;
using System.Linq;

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
/// Provides custom JSON serialization and deserialization for CadObject instances using System.Text.Json.
/// </summary>
/// <remarks>This converter enables reading and writing of CadObject types to and from JSON, handling property
/// mapping and special cases for INamedCadObject and IHandledCadObject implementations. It is intended for use with
/// System.Text.Json serialization scenarios where CadObject or its derived types require custom handling.</remarks>
public class CommonCadConverter : JsonConverter<CadObject>
{
	private readonly string[] _ignore = new[]
	{
		nameof(CadObject.ObjectName),
		nameof(CadObject.SubclassMarker),
		nameof(CadObject.Document),
	};

#if NET

	/// <inheritdoc/>
	public override CadObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		CadObject obj = (CadObject)Activator.CreateInstance(typeToConvert);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string propertyName = reader.GetString();
				var prop = typeToConvert.GetProperty(propertyName);

				reader.Read();

				var value = JsonSerializer.Deserialize(ref reader, prop.PropertyType, options);
				prop.SetValue(obj, value);
			}
		}

		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, CadObject value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString(nameof(CadObject.ObjectName), value.ObjectName);
		writer.WriteString(nameof(CadObject.SubclassMarker), value.SubclassMarker);

		foreach (var prop in value.GetType().GetProperties().DistinctBy(p => p.Name))
		{
			if (!prop.CanRead || this._ignore.Contains(prop.Name))
				continue;

			if (!prop.CanWrite)
				continue;

			var pValue = prop.GetValue(value);
			if (pValue == null)
			{
				if (!options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
				{
					writer.WriteNull(prop.Name);
				}
				continue;
			}

			if (pValue is INamedCadObject named)
			{
				writer.WriteString(prop.Name, named.Name);
				continue;
			}

			if (pValue is IHandledCadObject handled)
			{
				writer.WriteNumber(prop.Name, handled.Handle);
				continue;
			}

			writer.WritePropertyName(prop.Name);
			JsonSerializer.Serialize(writer, pValue, pValue.GetType(), options);
		}

		writer.WriteEndObject();
	}

#else
	public override CadObject ReadJson(JsonReader reader, Type objectType, CadObject existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, CadObject value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
#endif
}