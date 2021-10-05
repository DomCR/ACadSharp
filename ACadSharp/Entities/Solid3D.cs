using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Solid3D : Entity
	{
		public override ObjectType ObjectType => ObjectType.SOLID3D;
		public override string ObjectName => DxfFileToken.Entity3DSolid;

		//100	Subclass marker(AcDbTrace)

		//10	First corner
		//DXF: X value; APP: 3D point
		//20, 30
		//DXF: Y and Z values of first corner

		//11	Second corner
		//DXF: X value; APP: 3D point
		//21, 31
		//DXF: Y and Z values of second corner

		//12	Third corner
		//XF: X value; APP: 3D point
		//22, 32
		//DXF: Y and Z values of third corner

		//13Fourth corner.If only three corners are entered to define the SOLID, then the fourth corner coordinate is the same as the third.
		//DXF: X value; APP: 3D point
		//23, 33
		//DXF: Y and Z values of fourth corner

		//39	Thickness(optional; default = 0)

		//210	Extrusion direction(optional; default = 0, 0, 1)
		//DXF: X value; APP: 3D vector
		//220, 230	DXF: Y and Z values of extrusion direction(optional)

		public Solid3D() : base() { }

		internal Solid3D(DxfEntityTemplate template) : base(template) { }
	}
}
