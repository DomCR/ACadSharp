using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Objects;

public partial class MultiLeaderObjectContextData
{
	/// <summary>
	/// Represents a leader line of a <see cref="MultiLeaderObjectContextData"/> object.
	/// </summary>
	public class LeaderLine
	{
		public List<BreakInfo> BreakInfoEntries { get; private set; } = new();

		/// <summary>
		/// Gets or sets a <see cref="BlockRecord"/> containig elements
		/// to be dawn as arrow symbol.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 341)]
		public BlockRecord Arrowhead { get; set; }

		/// <summary>
		/// Arrowhead size
		/// </summary>
		[DxfCodeValue(40)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Leader line index.
		/// </summary>
		[DxfCodeValue(91)]
		public int Index { get; set; }

		/// <summary>
		/// Line color
		/// </summary>
		[DxfCodeValue(92)]
		public Color LineColor { get; set; }

		/// <summary>
		/// Line type
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public LineType LineType
		{
			get { return this._lineType; }
			set
			{
				if (value == null)
				{
					_lineType = null;
					return;
				}

				if (this.Document != null)
				{
					if (this.Document.LineTypes.TryGetValue(((LineType)value).Name, out LineType lt))
					{
						this._lineType = lt;
					}
					else
					{
						this._lineType = value;
						this.Document.LineTypes.Add(this._lineType);
					}
				}
			}
		}

		/// <summary>
		/// Line weight
		/// </summary>
		[DxfCodeValue(171)]
		public LineWeightType LineWeight { get; set; }

		/// <summary>
		/// Override flags
		/// </summary>
		[DxfCodeValue(93)]
		public LeaderLinePropertOverrideFlags OverrideFlags { get; set; }

		//R2010
		/// <summary>
		/// Leader type
		/// </summary>
		[DxfCodeValue(170)]
		public MultiLeaderPathType PathType { get; set; }

		/// <summary>
		/// Get the list of points of this <see cref="LeaderLine"/>.
		/// </summary>
		public IList<XYZ> Points { get; private set; } = new List<XYZ>();

		internal CadDocument Document { get; set; }

		private LineType _lineType;

		public LeaderLine()
		{ }

		internal void AssignDocument(CadDocument doc)
		{
			this.Document = doc;

			if (_lineType != null)
			{
				if (doc.LineTypes.TryGetValue(_lineType.Name, out LineType existing))
				{
					this._lineType = existing;
				}
				else
				{
					doc.LineTypes.Add(_lineType);
				}
			}

			doc.LineTypes.OnRemove += this.tableOnRemove;
		}

		public LeaderLine Clone()
		{
			LeaderLine clone = (LeaderLine)this.MemberwiseClone();

			clone.LineType = (LineType)this.LineType?.Clone();
			clone.Arrowhead = (BlockRecord)this.Arrowhead?.Clone();

			clone.Points = new List<XYZ>();
			foreach (var point in this.Points)
			{
				clone.Points.Add(point);
			}

			return clone;
		}

		internal void UassignDocument()
		{
			this.Document.LineTypes.OnRemove -= this.tableOnRemove;

			this.Document = null;

			this._lineType = (LineType)this._lineType?.Clone();
		}

		private void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this._lineType))
			{
				this._lineType = null;
			}
		}
	}
}