using ACadSharp.Attributes;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Attribute : TextEntity
	{
		public override ObjectType ObjectType => ObjectType.ATTRIB;
		public override string ObjectName => DxfFileToken.EntityAttribute;

		[DxfCodeValue(DxfCode.VerticalTextJustification1)]
		public override TextVerticalAlignment VerticalAlignment { get; set; } = TextVerticalAlignment.Baseline;

		public Attribute() : base() { }

		internal Attribute(DxfEntityTemplate template) : base(template) { }
	}
}
