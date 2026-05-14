using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;

namespace ACadSharp.Objects
{
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
					this._lineType = CadObject.updateCollection(value, this.Owner?.Document?.LineTypes);
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
				this._lineType = CadObject.updateCollection(this._lineType, doc.LineTypes);
			}

			internal void UnassignDocument()
			{
				this._lineType = this._lineType.CloneTyped();
			}
		}
	}
}