using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Face3D : Entity
	{
		public override ObjectType ObjectType => ObjectType.FACE3D;
		public override string ObjectName => DxfFileToken.Entity3DFace;

		public Face3D() : base() { }

		internal Face3D(DxfEntityTemplate template) : base(template) { }
	}
}
