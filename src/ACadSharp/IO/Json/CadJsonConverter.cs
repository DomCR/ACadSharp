using ACadSharp.IO.Json.Converters;
using System;
using System.Linq;
using System.Text.Json;

namespace ACadSharp.IO.Json;

/// <summary>
/// Provides methods for serializing and deserializing CadObject instances to and from JSON format.
/// </summary>
/// <remarks>This class supports custom serialization for CadObject types using the appropriate JSON serialization
/// options. It ensures that the necessary converters are applied for correct handling of CadObject instances. All
/// methods are static and can be used without creating an instance of JsonConverter.</remarks>
public class CadJsonConverter
{
	/// <summary>
	/// Deserializes a JSON string into an instance of the specified <see cref="CadObject"/> type.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="CadObject"/> to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>An instance of the specified <see cref="CadObject"/> type.</returns>
	public static T Deserialize<T>(string json)
			where T : CadObject
	{
		return (T)deserialize(json, typeof(T));
	}

	/// <summary>
	/// Deserializes a JSON string into an instance of the specified <see cref="CadObject"/> type.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="type">The type of the <see cref="CadObject"/> to deserialize.</param>
	/// <returns>An instance of the specified <see cref="CadObject"/> type.</returns>
	/// <exception cref="ArgumentException">Thrown when the provided type is not a subclass of <see cref="CadObject"/>.</exception>
	public static CadObject Deserialize(string json, Type type)
	{
		if (!type.IsSubclassOf(typeof(CadObject)))
		{
			throw new ArgumentException($"The provided type must be a subclass of {typeof(CadObject).FullName}.", nameof(type));
		}

		return (CadObject)deserialize(json, type);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="CadDocument"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>An instance of <see cref="CadDocument"/>.</returns>
	public static CadDocument DeserializeDocument(string json)
	{
		return (CadDocument)deserialize(json, typeof(CadDocument));
	}

	/// <summary>
	/// Serializes a <see cref="CadObject"/> to a JSON string.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="CadObject"/> to serialize.</typeparam>
	/// <param name="obj">The <see cref="CadObject"/> instance to serialize.</param>
	/// <param name="options">The JSON serialization options.</param>
	/// <returns>A JSON string representation of the <see cref="CadObject"/>.</returns>
	public static string Serialize<T>(T obj, JsonSerializerOptions options = null)
		where T : CadObject
	{
		return serialize(obj, options);
	}

	/// <summary>
	/// Serializes a <see cref="CadDocument"/> to a JSON string.
	/// </summary>
	/// <param name="doc">The <see cref="CadDocument"/> instance to serialize.</param>
	/// <param name="options">The JSON serialization options.</param>
	/// <returns>A JSON string representation of the <see cref="CadDocument"/>.</returns>
	public static string Serialize(CadDocument doc, JsonSerializerOptions options = null)
	{
		return serialize(doc, options);
	}

	private static object deserialize(string json, Type type, JsonSerializerOptions options = null)
	{
		if (options == null)
		{
			options = new JsonSerializerOptions();
		}

		if (!options.Converters.OfType<CadConverterFactory>().Any())
		{
			options.Converters.Add(new CadConverterFactory());
		}

		return JsonSerializer.Deserialize(json, type, options);
	}

	private static string serialize(object doc, JsonSerializerOptions options = null)
	{
		if (options == null)
		{
			options = new JsonSerializerOptions();
		}

		if (!options.Converters.OfType<CadConverterFactory>().Any())
		{
			options.Converters.Add(new CadConverterFactory());
		}

		return JsonSerializer.Serialize(doc, doc.GetType(), options);
	}
}