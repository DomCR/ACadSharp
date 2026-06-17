using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Mesh"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityMesh"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Mesh"/>
/// </remarks>
[DxfName(DxfFileToken.EntityMesh)]
[DxfSubClass(DxfSubclassMarker.Mesh)]
public partial class Mesh : Entity
{
	/// <summary>
	/// Blend Crease flag
	/// </summary>
	[DxfCodeValue(72)]
	public bool BlendCrease { get; set; }

	/// <summary>
	/// Edges of level 0
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 94)]
	[DxfCollectionCodeValue(90)]
	public List<Edge> Edges { get; private set; } = new();

	/// <summary>
	/// Face list of level 0
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 93)]
	[DxfCollectionCodeValue(90)]
	public List<int[]> Faces { get; private set; } = new();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityMesh;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Mesh;

	/// <summary>
	/// Number of subdivision level
	/// </summary>
	[DxfCodeValue(91)]
	public int SubdivisionLevel { get; set; }

	/// <summary>
	/// Per-vertex texture coordinates (U, V, W). Empty if the mesh has no UV mapping.
	/// </summary>
	/// <remarks>
	/// Persisted as an <c>ADSK_XREC_SUBDVERTEXTEXCOORDS</c> XRecord on the mesh extension dictionary,
	/// matching the AutoCAD/AcDbSubDMesh convention. The list must have the same length as <see cref="Vertices"/>.
	/// </remarks>
	public IEnumerable<XYZ> TextureCoordinates
	{
		get
		{
			if (this.XDictionary == null
				|| !this.XDictionary.TryGetEntry(Mesh.TextureCoordsXRecordName, out XRecord xrec))
			{
				return Enumerable.Empty<XYZ>();
			}

			var coords = new List<XYZ>();

			double? u = null, v = null;
			foreach (var entry in xrec.Entries)
			{
				switch (entry.Code)
				{
					case 43:
						u = System.Convert.ToDouble(entry.Value, System.Globalization.CultureInfo.InvariantCulture);
						break;
					case 44:
						v = System.Convert.ToDouble(entry.Value, System.Globalization.CultureInfo.InvariantCulture);
						break;
					case 45:
						double w = System.Convert.ToDouble(entry.Value, System.Globalization.CultureInfo.InvariantCulture);
						if (u.HasValue && v.HasValue)
						{
							coords.Add(new XYZ(u.Value, v.Value, w));
						}
						u = null;
						v = null;
						break;
				}
			}

			return coords;
		}
		set
		{
			var xdict = this.CreateExtendedDictionary();
			if (!xdict.TryGetEntry(Entities.Mesh.TextureCoordsXRecordName, out XRecord xrec))
			{
				xrec = new XRecord(Entities.Mesh.TextureCoordsXRecordName);
				xdict.Add(xrec);
			}

			xrec.Clear();

			foreach (var uv in value)
			{
				xrec.CreateEntry(43, uv.X);
				xrec.CreateEntry(44, uv.Y);
				xrec.CreateEntry(45, uv.Z);
			}
		}
	}

	/// <summary>
	/// Version number.
	/// </summary>
	[DxfCodeValue(71)]
	public short Version { get; set; } = 2;

	/// <summary>
	/// Vertex count of level 0.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 92)]
	[DxfCollectionCodeValue(10, 20, 30)]
	public List<XYZ> Vertices { get; private set; } = new();

	internal const string TextureCoordsXRecordName = "ADSK_XREC_SUBDVERTEXTEXCOORDS";

	/// <summary>
	/// Adds a texture coordinate to the mesh. The coordinates are stored in an XRecord in the mesh's extension dictionary.
	/// </summary>
	/// <param name="uvw">The texture coordinate to add.</param>
	public void AddTextureCoordinate(XYZ uvw)
	{
		var xdict = this.CreateExtendedDictionary();
		if (!xdict.TryGetEntry(Entities.Mesh.TextureCoordsXRecordName, out XRecord xrec))
		{
			xrec = new XRecord(Entities.Mesh.TextureCoordsXRecordName);
			xdict.Add(xrec);
		}

		xrec.CreateEntry(43, uvw.X);
		xrec.CreateEntry(44, uvw.Y);
		xrec.CreateEntry(45, uvw.Z);
	}

	//90	Count of sub-entity which property has been overridden

	//91	Sub-entity marker

	//92	Count of property was overridden

	//90	Property type
	//0 = Color
	//1 = Material
	//2 = Transparency
	//3 = Material mapper

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		for (int i = 0; i < this.Vertices.Count; i++)
		{
			this.Vertices[i] = transform.ApplyTransform(this.Vertices[i]);
		}
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		Mesh clone = (Mesh)base.Clone();

		clone.Edges = new List<Edge>(this.Edges);
		clone.Vertices = new List<XYZ>(this.Vertices);
		clone.Faces = new List<int[]>(this.Faces);

		return clone;
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		return BoundingBox.FromPoints(this.Vertices);
	}
}