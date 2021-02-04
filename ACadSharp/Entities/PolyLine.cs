using ACadSharp.Attributes;
using ACadSharp.Entities.Collections;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class PolyLine : Entity
	{
		//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-ABF6B778-BE20-4B49-9B58-A94E64CEFFF3

		public override ObjectType ObjectType => ObjectType.POLYLINE_2D;    //Shit there is a 3d too...
		public override string ObjectName => DxfFileToken.EntityPolyline;

		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.ZCoordinate)]
		public double Elevation { get; set; } = 0.0;
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.Int16)]
		public PolylineFlags Flags { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.StartWith)]
		public double StartWith { get; set; } = 0.0;
		/// <summary>
		/// 
		/// </summary>
		[DxfCodeValue(DxfCode.EndWith)]
		public double EndWith { get; set; } = 0.0;

		//71 Polyface mesh vertex index(optional; present only if nonzero)
		//72 Polyface mesh vertex index(optional; present only if nonzero)
		//73 Polyface mesh vertex index(optional; present only if nonzero)
		//74 Polyface mesh vertex index(optional; present only if nonzero)

		[DxfCodeValue(DxfCode.SmoothType)]
		public SmoothSurfaceType SmoothSurface { get; set; }

		/// <summary>
		/// Vertices that form this polyline.
		/// </summary>
		/// <remarks>
		/// Each <see cref="Vertex"/> has it's own unique handle.
		/// </remarks>
		[DxfSubClassEntity(DxfFileToken.EntityVertex)]
		public VertexCollection Vertices { get; set; } = new VertexCollection();
		//public List<Entity> Vertices { get; set; } = new List<Entity>();

		public PolyLine() : base() { }

		internal PolyLine(DxfEntityTemplate template) : base(template) { }
	}
}
