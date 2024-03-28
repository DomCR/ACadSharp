using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Tolerance"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityTolerance"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Tolerance"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityTolerance)]
	[DxfSubClass(DxfSubclassMarker.Tolerance)]
	public class Tolerance : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.TOLERANCE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityTolerance;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Tolerance;

		/// <summary>
		/// Dimension style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 3)]
		public DimensionStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = this.updateTable(value, this.Document.DimensionStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <summary>
		/// Insertion point (in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// X-axis direction vector (in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Visual representation of the tolerance
		/// </summary>
		[DxfCodeValue(1)]
		public string Text { get; set; }

		private DimensionStyle _style = DimensionStyle.Default;
	}
}
