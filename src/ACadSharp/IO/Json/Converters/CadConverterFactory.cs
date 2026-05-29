using ACadSharp.Entities;
using ACadSharp.Header;
using System;
using System.Collections.Generic;

#if NET

using System.Text.Json;
using System.Text.Json.Serialization;

#else
using Newtonsoft.Json;
#endif

namespace ACadSharp.IO.Json.Converters;

public class CadConverterFactory :
#if NET
	JsonConverterFactory
#else
	Newtonsoft.Json.JsonConverter
#endif
{
	private readonly Dictionary<Type, JsonConverter> _converters = new()
	{
		{ typeof(Color), new ColorConverter() },
		{ typeof(PolygonMesh), new CommonPolylineConverter<PolygonMesh, PolygonMeshVertex>() },
		{ typeof(PolyfaceMesh), new PolyfaceMeshConverter() },
		{ typeof(Polyline2D), new CommonPolylineConverter<Polyline2D, Vertex2D>() },
		{ typeof(Polyline3D), new CommonPolylineConverter<Polyline3D, Vertex3D>() },
		{ typeof(CadDocument), new CadDocumentConverter() },
		{ typeof(CadHeader), new CadHeaderConverter() },
	};

	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsSubclassOf(typeof(CadObject))
			|| _converters.ContainsKey(typeToConvert);
	}

#if NET
	/// <inheritdoc/>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (this._converters.TryGetValue(typeToConvert, out var converter))
		{
			return converter;
		}

		return new CommonCadConverter();
	}

#else
	/// <inheritdoc/>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (this._converters.TryGetValue(objectType, out var converter))
		{
			return converter.ReadJson(reader, objectType, existingValue, serializer);
		}

		return new CommonCadConverter().ReadJson(reader, objectType, existingValue, serializer);
	}

	/// <inheritdoc/>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (this._converters.TryGetValue(value.GetType(), out var converter))
		{
			converter.WriteJson(writer, value, serializer);
			return;
		}

		new CommonCadConverter().WriteJson(writer, value, serializer);
	}
#endif
}