namespace ACadSharp.Entities
{
	public class Seqend : Entity
	{
		public override ObjectType ObjectType => ObjectType.SEQEND;
		public override string ObjectName => DxfFileToken.EndSequence;
	}
}
