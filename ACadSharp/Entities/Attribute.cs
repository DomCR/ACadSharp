using ACadSharp.Attributes;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Attribute : AttributeBase
	{
		public override ObjectType ObjectType => ObjectType.ATTRIB;
		public override string ObjectName => DxfFileToken.EntityAttribute;

		public Attribute() : base() { }

		internal Attribute(DxfEntityTemplate template) : base(template) { }
	}
}
