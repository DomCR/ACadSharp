namespace ACadSharp.Tests.TestModels
{
	public class TableEntryNode : Node
	{
		public string Name { get; set; }

		public override string ToString()
		{
			return $"{this.ACadName}:{this.Name}";
		}
	}
}
