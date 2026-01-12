using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="ModelerGeometry"/> entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.ModelerGeometry)]
	public abstract partial class ModelerGeometry : Entity
	{
		public XYZ Point { get; set; }

		public List<Silhouette> Silhouettes { get; } = new();

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ModelerGeometry;

		public List<Wire> Wires { get; } = new();

		[DxfCodeValue(2)]
		internal Guid Guid { get; set; }

		/// <summary>
		/// Gets or sets the modeler format version used in the drawing.
		/// </summary>
		[DxfCodeValue(70)]
		public short ModelerFormatVersion { get; set; }

		[DxfCodeValue(1)]
		//[DxfCodeValue(3)]
		public StringBuilder ProprietaryData { get; } = new();

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}