using ACadSharp.Entities;
using System.Collections;
using System.Collections.Generic;
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
/// <typeparam name="TPolyline">The type of polyline. Must inherit from <see cref="Polyline{TVertex}"/>.</typeparam>
/// <typeparam name="TVertex">The type of vertex contained in the polyline. Must inherit from Entity and implement <see cref="IVertex"/>.</typeparam>
public class CommonPolylineConverter<TPolyline, TVertex> : CommonCadConverter<TPolyline>
	where TPolyline : Polyline<TVertex>
	where TVertex : Entity, IVertex
{
	protected override void writeProperty(PropertyInfo prop,
#if NET
	Utf8JsonWriter writer, TPolyline value, JsonSerializerOptions options
#else
	JsonWriter writer, TPolyline value, JsonSerializer serializer
#endif
	)
	{
		if (prop.Name.Equals(nameof(Polyline<>.Vertices)))
		{
#if NET
			this.writeEnumerable(nameof(Polyline<>.Vertices), value.Vertices, writer, value, options);
#else
			this.writeEnumerable(nameof(Polyline<>.Vertices), value.Vertices, writer, value, serializer);
#endif
		}

#if NET
		base.writeProperty(prop, writer, value, options);
#else
		base.writeProperty(prop, writer, value, serializer);
#endif
	}

#if NET
	protected override void readPropertyValue(TPolyline obj, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (!prop.Name.Equals(nameof(Polyline<>.Vertices)))
		{
			base.readPropertyValue(obj, prop, ref reader, options);
			return;
		}

		reader.Read();

		obj.Vertices.AddRange(this.readArray<TVertex>(ref reader, options));
	}

	protected IEnumerable<T> readArray<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		List<T> lst = new List<T>();

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			reader.Read();

			while (reader.TokenType != JsonTokenType.EndArray)
			{
				var v = JsonSerializer.Deserialize(ref reader, typeof(T), options);
				lst.Add((T)v);

				reader.Read();
			}

			reader.Read();
		}

		return lst;
	}
#endif
}
