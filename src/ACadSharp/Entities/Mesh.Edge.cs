namespace ACadSharp.Entities
{
	public partial class Mesh
	{
		public struct Edge
		{
			/// <summary>
			/// Index from the vertices where this edge starts
			/// </summary>
			public int Start { get; set; }

			/// <summary>
			/// Index from the vertices where this edge ends
			/// </summary>
			public int End { get; set; }

			/// <summary>
			/// Edge crease value, null if not set
			/// </summary>
			public double? Crease { get; set; }

			public override string ToString()
			{
				string str = $"{this.Start}|{this.End}";
				return this.Crease.HasValue ? $"{this.Start}|{this.End}|{this.Crease}" : str;
			}
		}
	}
}
