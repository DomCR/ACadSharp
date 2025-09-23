using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="ModelerGeometry"/> entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.ModelerGeometry)]
	public abstract class ModelerGeometry : Entity
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ModelerGeometry;

		public XYZ Point { get; set; }

		public List<Wire> Wires { get; } = new();

		public List<Silhouette> Silhouettes { get; } = new();

		public class Silhouette
		{
			public List<Wire> Wires { get; } = new();
			public int ViewportId { get; internal set; }
			public XYZ ViewportTarget { get; internal set; }
			public XYZ ViewportDirectionFromTarget { get; internal set; }
			public XYZ ViewportUpDirection { get; internal set; }
			public bool ViewportPerspective { get; internal set; }
		}

		public class Wire
		{
			internal bool HasReflection;

			public byte Type { get; set; }
			public int SelectionMarker { get; set; }
			public Color Color { get; set; }
			public int AcisIndex { get; set; }
			public List<XYZ> Points { get; } = new List<XYZ>();
			public bool ApplyTransformPresent { get; internal set; }
			public XYZ XAxis { get; internal set; }
			public XYZ YAxis { get; internal set; }
			public XYZ ZAxis { get; internal set; }
			public XYZ Translation { get; internal set; }
			public double Scale { get; internal set; }
			public bool HasRotation { get; internal set; }
			public bool HasShear { get; internal set; }
		}
	}
}
