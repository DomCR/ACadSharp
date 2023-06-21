namespace ACadSharp.Objects
{
	public class VisualStyle : CadObject
	{
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string ObjectName => DxfFileToken.ObjectVisualStyle;

		public override string SubclassMarker => DxfSubclassMarker.VisualStyle;
	}
}
