using ACadSharp.Entities;
using System.Reflection;

#if NET

using System.Text.Json;

#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace ACadSharp.IO.Json.Converters;

public class PolyfaceMeshConverter : CommonPolylineConverter<PolyfaceMesh, VertexFaceMesh>
{
#if NET
	protected override void readPropertyValue(PolyfaceMesh obj, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (!prop.Name.Equals(nameof(PolyfaceMesh.Faces)))
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
				var v = JsonSerializer.Deserialize(ref reader, typeof(VertexFaceRecord), options);
				obj.Faces.Add((VertexFaceRecord)v);

				reader.Read();
			}

			reader.Read();
		}
	}
#endif
}