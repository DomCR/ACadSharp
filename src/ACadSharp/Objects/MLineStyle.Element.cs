using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

public partial class MLineStyle
{
	public class Element
	{
		/// <summary>
		/// Element color.
		/// </summary>
		[DxfCodeValue(62)]
		public Color Color { get; set; } = Color.ByLayer;

		/// <summary>
		/// Element linetype.
		/// </summary>
		[DxfCodeValue(6)]
		public LineType LineType
		{
			get => _lineType;
			set
			{
				if (this.Owner == null)
				{
					this._lineType = value;
				}
				else
				{
					this._lineType = this.Owner.updateTableEntry(value, l => this._lineType = l, this.Owner.Document?.LineTypes);
				}
			}
		}

		/// <summary>
		/// Element offset.
		/// </summary>
		[DxfCodeValue(49)]
		public double Offset { get; set; }

		/// <summary>
		/// Line type where this segment belongs.
		/// </summary>
		public MLineStyle Owner { get; internal set; }

		private LineType _lineType = LineType.ByLayer;

		/// <summary>
		/// Clone the current segment.
		/// </summary>
		/// <returns></returns>
		public MLineStyle.Element Clone()
		{
			Element clone = MemberwiseClone() as Element;
			clone.Owner = null;
			clone._lineType = (LineType)(this.LineType?.Clone());
			return clone;
		}

		internal void AssignDocument(CadDocument doc)
		{
			this._lineType = this.Owner.updateTableEntry(this._lineType, l => this._lineType = l, doc.LineTypes);
		}

		internal void UnassignDocument()
		{
			this.Owner?.Document.LineTypes.RemoveReference(this._lineType?.Name, this.Owner);

			this._lineType = this._lineType.CloneTyped();
		}
	}
}