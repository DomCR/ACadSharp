using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMesh;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Mesh;

		/// <summary>
		/// Version number
		/// </summary>
		[DxfCodeValue(71)]
		public short Version { get; internal set; }

		/// <summary>
		/// Blend Crease flag
		/// </summary>
		[DxfCodeValue(72)]
		public bool BlendCrease { get; set; }

		/// <summary>
		/// Number of subdivision level
		/// </summary>
		[DxfCodeValue(91)]
		public int SubdivisionLevel { get; set; }

		/// <summary>
		/// Vertex count of level 0
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 92)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> Vertices { get; set; } = new();

		/// <summary>
		/// Face list of level 0
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 93)]
		[DxfCollectionCodeValue(90)]
		public List<int[]> Faces { get; set; } = new();

		/// <summary>
		/// Edges of level 0
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 94)]
		[DxfCollectionCodeValue(90)]
		public List<Edge> Edges { get; set; } = new();

		//90	Count of sub-entity which property has been overridden

		//91	Sub-entity marker

		//92	Count of property was overridden

		//90	Property type
		//0 = Color
		//1 = Material
		//2 = Transparency
		//3 = Material mapper

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.FromPoints(this.Vertices);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}
	}
}
