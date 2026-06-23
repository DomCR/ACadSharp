using ACadSharp.Entities;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

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

#if NET

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

#else
	/// <inheritdoc/>
	public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
#endif

	/// <inheritdoc/>
#if NET

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
#else
	public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
#endif
	{
		writer.WriteStartObject();

		writer.WriteStringValue(nameof(CadObject.ObjectName), value.ObjectName);
		writer.WriteStringValue(nameof(CadObject.SubclassMarker), value.SubclassMarker);

		foreach (var prop in value.GetType().GetProperties().DistinctBy(p => p.Name))
		{
#if NET
			this.writeProperty(prop, writer, value, options);
#else
			this.writeProperty(prop, writer, value, serializer);
#endif
		}

		if (value is IEnumerable arr)
		{
#if NET
			this.writeEnumerable("Entries", arr, writer, value, options);
#else
			this.writeEnumerable("Entries", arr, writer, value, serializer);
#endif
		}

		writer.WriteEndObject();
	}

	protected void writeEnumerable(string name, IEnumerable arr,
#if NET
	Utf8JsonWriter writer, T value, JsonSerializerOptions options
#else
		JsonWriter writer, T value, JsonSerializer serializer
#endif
	)
	{
		writer.WritePropertyName(name);

		writer.WriteStartArray();

		foreach (var item in arr)
		{
#if NET
			JsonSerializer.Serialize(writer, item, item.GetType(), options);
#else
				serializer.Serialize(writer, item);
#endif
		}

		writer.WriteEndArray();
	}

	protected virtual void writeProperty(PropertyInfo prop,
#if NET
		Utf8JsonWriter writer, T value, JsonSerializerOptions options
#else
		JsonWriter writer, T value, JsonSerializer serializer
#endif
		)
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
#if NET
			if (!options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
			{
				writer.WriteNull(prop.Name);
			}
#else
				if (serializer.NullValueHandling == NullValueHandling.Include)
				{
					writer.WritePropertyName(prop.Name);
					writer.WriteNull();
				}
#endif
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
#if NET
		JsonSerializer.Serialize(writer, pValue, pValue.GetType(), options);
#else
		serializer.Serialize(writer, pValue);
#endif
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
