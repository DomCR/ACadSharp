﻿namespace ACadSharp.Tests.TestModels
{
	public class EntityNode : Node
	{
		public ColorData Color { get; set; } = new ColorData();

		public string LayerName { get; set; }

		public bool IsInvisible { get; set; }

		public Transparency Transparency { get; set; }

		public string LinetypeName { get; set; }

		public double LinetypeScale { get; set; }

		public LineweightType LineWeight { get; set; }
	}
}
