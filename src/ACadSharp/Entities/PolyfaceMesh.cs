using ACadSharp.Attributes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="PolyfaceMesh"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.PolyfaceMesh"/>
/// </remarks>
[DxfName(DxfFileToken.EntityPolyline)]
[DxfSubClass(DxfSubclassMarker.PolyfaceMesh)]
public class PolyfaceMesh : Polyline<VertexFaceMesh>
{
	/// <summary>
	/// Face records with the triangle indexes.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 72)]
	public CadObjectCollection<VertexFaceRecord> Faces { get; private set; }

	/// <inheritdoc/>
	public override PolylineFlags Flags { get => base.Flags | PolylineFlags.PolyfaceMesh; set => base.Flags = value; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityPolyline;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.POLYLINE_PFACE; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.PolyfaceMesh;

	/// <inheritdoc/>
	public PolyfaceMesh() : base()
	{
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		PolyfaceMesh clone = (PolyfaceMesh)base.Clone();

		foreach (VertexFaceRecord v in this.Faces)
		{
			clone.Faces.Add((VertexFaceRecord)v.Clone());
		}

		return clone;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);
		doc.RegisterCollection(this.Faces);
	}

	internal override void UnassignDocument()
	{
		this.Document.UnregisterCollection(this.Faces);
		base.UnassignDocument();
	}

	protected override void initCollections()
	{
		base.initCollections();
		this.Faces = new CadObjectCollection<VertexFaceRecord>(this);
		this.Faces.OnAdd += this.onAddVertices;
	}
}