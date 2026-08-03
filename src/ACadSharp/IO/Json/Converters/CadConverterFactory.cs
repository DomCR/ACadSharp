using ACadSharp.Entities;
using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACadSharp.IO.Json.Converters;

public class CadConverterFactory : JsonConverterFactory
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

	/// <inheritdoc/>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (this._converters.TryGetValue(typeToConvert, out var converter))
		{
			return converter;
		}

		return new CommonCadConverter();
	}
}