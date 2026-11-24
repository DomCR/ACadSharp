using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	/// <summary>
	/// Represents the border settings for a cell, including appearance, visibility, and style options.
	/// </summary>
	/// <remarks>Use this class to configure the visual and printing characteristics of cell borders in a drawing or
	/// table. The properties allow customization of color, line weight, border type, and visibility. Changes to these
	/// settings affect how borders are displayed in the editor and when plotted or printed.</remarks>
	public class CellBorder
	{
		/// <summary>
		/// Gets or sets a value indicating whether a border is applied.
		/// </summary>
		public bool ApplyBorder { get; set; } = false;

		/// <summary>
		/// Gets or sets the color associated with the entity.
		/// </summary>
		[DxfCodeValue(64)]
		public Color Color { get; set; } = Color.ByBlock;

		/// <summary>
		/// Gets or sets the spacing distance between parallel lines in a double-line entity.
		/// </summary>
		/// <remarks>The value determines the distance between the two lines when rendering double-line geometry.
		/// Adjust this property to control the visual separation of the lines. The unit of measurement is typically
		/// consistent with the drawing's coordinate system.</remarks>
		[DxfCodeValue(40)]
		public double DoubleLineSpacing { get; set; }

		/// <summary>
		/// Gets the set of flags that describe the characteristics of the cell's edges.
		/// </summary>
		/// <remarks>Use these flags to determine which edges of the cell have specific properties, such as visibility
		/// or formatting. The meaning of each flag is defined by the CellEdgeFlags enumeration.</remarks>
		[DxfCodeValue(95)]
		public CellEdgeFlags EdgeFlags { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the border is invisible. Invisible borders are not plotted and do not print, but they are still visible in the drawing editor.
		/// </summary>
		[DxfCodeValue(284)]
		public bool IsInvisible { get; set; } = false;

		/// <summary>
		/// Gets or sets the type of line.
		/// </summary>
		public LineType LineType
		{
			get
			{
				if (this.Owner?.Document != null
					&& this._lineType != null
					&& this.Owner.Document.LineTypes.TryGetValue(this._lineType.Name, out var lineType))
				{
					this._lineType = lineType;
				}
				else if (this.Owner?.Document != this._lineType?.Document)
				{
					this._lineType = this._lineType?.CloneTyped();
				}

				return this._lineType;
			}
			set
			{
				if (this.Owner?.Document != null)
				{
					this._lineType = this.Owner.Document.LineTypes.TryAdd(value);
				}
				else
				{
					this._lineType = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the line weight to use when rendering the entity.
		/// </summary>
		[DxfCodeValue(274)]
		public LineWeightType LineWeight { get; set; } = LineWeightType.ByBlock;

		public CadObject Owner
		{
			get { return this._owner; }
			internal set
			{
				this._owner = value;

				if (this._owner == null)
				{
					this._lineType = this._lineType?.CloneTyped();
				}
			}
		}

		/// <summary>
		/// Gets or sets the set of flags that specify which border properties are overridden.
		/// </summary>
		[DxfCodeValue(90)]
		public BorderPropertyFlags PropertyOverrideFlags { get; set; }

		/// <summary>
		/// Gets or sets the border style type to be applied.
		/// </summary>
		[DxfCodeValue(91)]
		public BorderType Type { get; set; } = BorderType.Single;

		private LineType _lineType;

		private CadObject _owner;

		/// <summary>
		/// Initializes a new instance of the CellBorder class with the specified edge flags.
		/// </summary>
		/// <param name="edgeFlags">The set of edge flags that define which borders are applied to the cell.</param>
		public CellBorder(CellEdgeFlags edgeFlags)
		{
			this.EdgeFlags = edgeFlags;
		}

		/// <summary>
		/// Creates a copy of the current <see cref="CellBorder"/> instance.
		/// </summary>
		/// <remarks>Reference-type members are copied by reference.</remarks>
		/// <returns>A new <see cref="CellBorder"/> that is a shallow copy of the current instance, with <c>Owner</c> set to <see
		/// langword="null"/>.</returns>
		public CellBorder Clone()
		{
			CellBorder clone = this.MemberwiseClone() as CellBorder;
			clone.Owner = null;
			return clone;
		}
	}
}