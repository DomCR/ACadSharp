using ACadSharp.Entities;
using System.Reflection;
using System.Text.Json;

namespace ACadSharp.IO.Json.Converters;

public class PolyfaceMeshConverter : CommonPolylineConverter<PolyfaceMesh, VertexFaceMesh>
{
	protected override void readPropertyValue(PolyfaceMesh obj, PropertyInfo prop, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (!prop.Name.Equals(nameof(PolyfaceMesh.Faces)))
		{
			base.readPropertyValue(obj, prop, ref reader, options);
			return;
		}

		reader.Read();

		obj.Faces.AddRange(this.readArray<VertexFaceRecord>(ref reader, options));
	}
}