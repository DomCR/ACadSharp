namespace ACadSharp.Tables
{
	public class ViewportEntityHeader : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.VP_ENT_HDR;
		public override string ObjectName => DxfFileToken.EntityViewport;
	}
}
