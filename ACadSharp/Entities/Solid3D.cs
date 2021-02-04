using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Solid3D : Entity
	{
		public override ObjectType ObjectType => ObjectType.SOLID3D;
		public override string ObjectName => DxfFileToken.Entity3DSolid;

		public Solid3D() : base() { }

		internal Solid3D(DxfEntityTemplate template) : base(template) { }
	}
}
