using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Text : TextEntity
	{
		public override ObjectType ObjectType => ObjectType.TEXT;
		public override string ObjectName => DxfFileToken.EntityText;

		public Text() : base() { }

		internal Text(DxfEntityTemplate template) : base(template) { }
	}
}
