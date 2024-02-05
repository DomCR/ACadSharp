using ACadSharp.Attributes;
using System.Drawing;
using System;

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
	public class Mesh : Entity
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

		//72 "Blend Crease" property
		//0 = Turn off
		//1 = Turn on

		//91	Number of subdivision level

		//92	Vertex count of level 0

		//10	Vertex position

		//93	Size of face list of level 0

		//90	Face list item

		//94	Edge count of level 0

		//90	Vertex index of each edge

		//95	Edge crease count of level 0

		//140	Edge create value

		//90	Count of sub-entity which property has been overridden

		//91	Sub-entity marker

		//92	Count of property was overridden

		//90	Property type
		//0 = Color
		//1 = Material
		//2 = Transparency
		//3 = Material mapper
	}
}
