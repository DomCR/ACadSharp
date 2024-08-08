namespace ACadSharp.Tests.TestModels
{
	public class LayerNode : TableEntryNode
	{
		public string LinetypeName { get; set; }

		public LineweightType LineWeight { get; set; }

		public ColorData Color { get; set; } = new ColorData();
	}
}
