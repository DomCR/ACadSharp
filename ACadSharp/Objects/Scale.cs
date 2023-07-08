namespace ACadSharp.Objects
{
	public class Scale : CadObject
	{
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		public short Unknown { get; set; }
		public string Name { get; set; }
		public double PaperUnits { get; set; }
		public double DrawingUnits { get; set; }
		public bool IsUnitScale { get; set; }
	}
}
