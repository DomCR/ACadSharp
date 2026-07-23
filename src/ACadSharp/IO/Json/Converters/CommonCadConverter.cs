using ACadSharp.Entities;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACadSharp.IO.Json.Converters;

public class CommonCadConverter<T> : JsonConverter<T>
 where T : CadObject
{
	private readonly string[] _ignore = new[]
	{
		nameof(CadObject.ObjectName),
		nameof(CadObject.SubclassMarker),
		nameof(CadObject.Document),
	};

	/// <inheritdoc/>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		T obj = (T)Activator.CreateInstance(typeToConvert);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string propertyName = reader.GetString();
				var prop = typeToConvert.GetProperty(propertyName);
				this.readPropertyValue(obj, prop, ref reader, options);
			}
			else if (reader.TokenType == JsonTokenType.EndObject)
			{
				return obj;
			}
		}

		throw new JsonException("Expected EndObject token.");
	}

	protected virtual void readPropertyValue(T obj, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (!prop.CanWrite)
		{
			return;
		}

		object value = null;
		if (prop.PropertyType.HasInterface<INamedCadObject>())
		{
			value = Activator.CreateInstance(prop.PropertyType, reader.GetString());
		}
		else if (prop.PropertyType.IsAssignableFrom(typeof(IHandledCadObject)))
		{
			return;
		}
		else
		{
			value = JsonSerializer.Deserialize(ref reader, prop.PropertyType, options);
		}

		prop.SetValue(obj, value);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteStringValue(nameof(CadObject.ObjectName), value.ObjectName);
		writer.WriteStringValue(nameof(CadObject.SubclassMarker), value.SubclassMarker);

		foreach (var prop in value.GetType().GetProperties().DistinctBy(p => p.Name))
		{
			this.writeProperty(prop, writer, value, options);
		}

		if (value is IEnumerable arr)
		{
			this.writeEnumerable("Entries", arr, writer, value, options);
		}

		writer.WriteEndObject();
	}

	protected void writeEnumerable(string name, IEnumerable arr, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(name);

		writer.WriteStartArray();

		foreach (var item in arr)
		{
			JsonSerializer.Serialize(writer, item, item.GetType(), options);
		}

		writer.WriteEndArray();
	}

	protected virtual void writeProperty(PropertyInfo prop, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		if (!prop.CanRead || this._ignore.Contains(prop.Name))
		{
			return;
		}

		if (!prop.CanWrite)
		{
			return;
		}

		var pValue = prop.GetValue(value);
		if (pValue == null)
		{
			if (!options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
			{
				writer.WriteNull(prop.Name);
			}

			return;
		}

		if (pValue is INamedCadObject named && !prop.Name.Equals(nameof(CadObject.Owner), StringComparison.OrdinalIgnoreCase))
		{
			writer.WriteStringValue(prop.Name, named.Name);
			return;
		}

		if (pValue is IHandledCadObject handled)
		{
			writer.WriteNumberValue(prop.Name, handled.Handle);
			return;
		}

		writer.WritePropertyName(prop.Name);

		JsonSerializer.Serialize(writer, pValue, pValue.GetType(), options);
	}
}

/// <summary>
/// Provides custom JSON serialization and deserialization for CadObject instances using System.Text.Json.
/// </summary>
/// <remarks>This converter enables reading and writing of CadObject types to and from JSON, handling property
/// mapping and special cases for INamedCadObject and IHandledCadObject implementations. It is intended for use with
/// System.Text.Json serialization scenarios where CadObject or its derived types require custom handling.</remarks>
public class CommonCadConverter : CommonCadConverter<CadObject>
{
}
