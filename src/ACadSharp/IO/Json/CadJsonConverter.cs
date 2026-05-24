using ACadSharp.IO.Json.Converters;
using System.Linq;
using System;

#if NET

using System.Text.Json;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json;

/// <summary>
/// Provides methods for serializing and deserializing CadObject instances to and from JSON format.
/// </summary>
/// <remarks>This class supports custom serialization for CadObject types using the appropriate JSON serialization
/// options. It ensures that the necessary converters are applied for correct handling of CadObject instances. All
/// methods are static and can be used without creating an instance of JsonConverter.</remarks>
public class CadJsonConverter
{
	public static string Deserialize<T>(string json)
			where T : CadObject
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Serializes a <see cref="CadObject"/> to a JSON string.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="CadObject"/> to serialize.</typeparam>
	/// <param name="obj">The <see cref="CadObject"/> instance to serialize.</param>
	/// <param name="options">The JSON serialization options.</param>
	/// <returns>A JSON string representation of the <see cref="CadObject"/>.</returns>
	public static string Serialize<T>(T obj,
#if NET
		JsonSerializerOptions options = null
#else
		JsonSerializerSettings options = null
#endif
		) where T : CadObject
	{
		return serialize(obj, options);
	}

	public static string Serialize(CadDocument doc,
#if NET
		JsonSerializerOptions options = null
#else
		JsonSerializerSettings options = null
#endif
		)
	{
		return serialize(doc, options);
	}

	private static string serialize(object doc,
#if NET
		JsonSerializerOptions options = null
#else
		JsonSerializerSettings options = null
#endif
		)
	{
		if (options == null)
		{
#if NET
			options = new JsonSerializerOptions();
#else
			options = new JsonSerializerSettings();
#endif
		}

		if (!options.Converters.OfType<CadConverterFactory>().Any())
		{
			options.Converters.Add(new CadConverterFactory());
		}

#if NET
		return JsonSerializer.Serialize(doc, doc.GetType(), options);
#else
		return JsonConvert.SerializeObject(doc, doc.GetType(), options);
#endif
	}
}