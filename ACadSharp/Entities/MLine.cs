using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MLine"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLine"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLine"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLine)]
	[DxfSubClass(DxfSubclassMarker.MLine)]
	public partial class MLine : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MLINE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLine;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLine;

		/// <summary>
		/// MLine Style
		/// </summary>
		/// <remarks>
		/// Name reference: <br/>
		/// String of up to 32 characters.The name of the style used for this mline. An entry for this style must exist in the MLINESTYLE dictionary.
		/// Do not modify this field without also updating the associated entry in the MLINESTYLE dictionary
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle | DxfReferenceType.Name, 340)]
		public MLStyle MLStyle
		{
			get { return _style; }
			set
			{
				if (value == null)
				{
					throw new System.ArgumentNullException(nameof(value), "Multi line style cannot be null");
				}
				this._style = value;
			}
		}

		/// <summary>
		/// Scale factor
		/// </summary>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Justification
		/// </summary>
		[DxfCodeValue(70)]
		public MLineJustification Justification { get; set; }

		/// <summary>
		/// Flags
		/// </summary>
		[DxfCodeValue(71)]
		public MLineFlags Flags { get; set; }

		/// <summary>
		/// Start point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; }

		/// <summary>
		/// Extrusion direction
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Vertices in the MLine
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		private MLStyle _style = MLStyle.Default;

		public MLine() : base() { }

		public override CadObject Clone()
		{
			MLine clone = (MLine)base.Clone();

			clone.MLStyle = (MLStyle)(this.MLStyle?.Clone());

			clone.Vertices.Clear();
			foreach (var item in this.Vertices)
			{
				clone.Vertices.Add(item.Clone());
			}

			return clone;
		}
	}
}
