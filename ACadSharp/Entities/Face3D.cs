using ACadSharp.IO.Templates;
using CSMath;

namespace ACadSharp.Entities
{
	public class Face3D : Entity
	{
		public override ObjectType ObjectType => ObjectType.FACE3D;
		public override string ObjectName => DxfFileToken.Entity3DFace;

		//100	Subclass marker(AcDbFace)

		//10

		//First corner(in WCS)

		//DXF: X value; APP: 3D point

		//20, 30

		//DXF: Y and Z values of first corner(in WCS)

		//11

		//Second corner(in WCS)

		//DXF: X value; APP: 3D point

		//21, 31

		//DXF: Y and Z values of second corner(in WCS)

		//12

		//Third corner(in WCS)

		//DXF: X value; APP: 3D point

		//22, 32

		//DXF: Y and Z values of third corner(in WCS)

		//13

		//Fourth corner(in WCS). If only three corners are entered, this is the same as the third corner

		//DXF: X value; APP: 3D point

		//23, 33	DXF: Y and Z values of fourth corner(in WCS)


		public XYZ FirstCorner { get; set; }
		public XYZ SecondCorner { get; set; }
		public XYZ ThirdCorner { get; set; }
		public XYZ FourthCorner { get; set; }
		//70	Invisible edge flags(optional; default = 0):

		public InvisibleEdgeFlags Flags { get; set; }

		public Face3D() : base() { }

		internal Face3D(DxfEntityTemplate template) : base(template) { }
	}
}
