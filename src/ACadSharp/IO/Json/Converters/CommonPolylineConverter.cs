using ACadSharp.Entities;
using System.Reflection;

#if NET

using System.Text.Json;

#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace ACadSharp.IO.Json.Converters;

/// <summary>
/// Provides JSON conversion functionality for <see cref="Polyline{T}"/> entities with vertices of type T.
/// </summary>
/// <remarks>
/// This converter handles serialization and deserialization of <see cref="Polyline{T}"/> objects, ensuring that vertex
/// collections are properly processed. It is intended for use with CAD-related data models where polylines are composed
/// of custom vertex types.
/// </remarks>
/// <typeparam name="T">The type of vertex contained in the polyline. Must inherit from Entity and implement <see cref="IVertex"/>.</typeparam>
public class CommonPolylineConverter<T> : CommonCadConverter<Polyline<T>>
	where T : Entity, IVertex
{
	protected override void writeProperty(PropertyInfo prop,
#if NET
	Utf8JsonWriter writer, Polyline<T> value, JsonSerializerOptions options
#else
		JsonWriter writer, Polyline<T> value, JsonSerializer serializer
#endif
	)
	{
		if (prop.Name.Equals(nameof(Polyline<T>.Vertices)))
		{
#if NET
			this.writeEnumerable(nameof(Polyline<T>.Vertices), value.Vertices, writer, value, options);
#else
			this.writeEnumerable(nameof(Polyline<T>.Vertices), value.Vertices, writer, value, serializer);
#endif
		}

#if NET
		base.writeProperty(prop, writer, value, options);
#else
		base.writeProperty(prop, writer, value, serializer);
#endif
	}

#if NET
	protected override void readPropertyValue(Polyline<T> obj, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (!prop.Name.Equals(nameof(Polyline<T>.Vertices)))
		{
			base.readPropertyValue(obj, prop, ref reader, options);
			return;
		}

		reader.Read();

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			reader.Read();

			while (reader.TokenType != JsonTokenType.EndArray)
			{
				var v = JsonSerializer.Deserialize(ref reader, typeof(T), options);
				obj.Vertices.Add((T)v);

				reader.Read();
			}

			reader.Read();
		}
	}
#endif
}