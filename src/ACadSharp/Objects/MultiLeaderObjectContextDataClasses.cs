using System;
using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Objects
{

	/// <summary>
	/// Nested classes in MultiLeaderAnnotContext
	/// </summary>
	public partial class MultiLeaderObjectContextData : AnnotScaleObjectContextData
	{
		/// <summary>
		/// Represents a leader root
		/// </summary>
		/// <remarks>
		/// Appears in DXF as 302 DXF: “LEADER“
		/// </remarks>
		public class LeaderRoot : ICloneable
		{
			public LeaderRoot() { }

			/// <summary>
			/// Is content valid (ODA writes true)
			/// </summary>
			[DxfCodeValue(290)]
			public bool ContentValid { get; set; }

			/// <summary>
			/// Unknown (ODA writes true)
			/// </summary>
			[DxfCodeValue(291)]
			public bool Unknown { get; set; }

			/// <summary>
			/// Connection point
			/// </summary>
			[DxfCodeValue(10, 20, 30)]
			public XYZ ConnectionPoint { get; set; }

			/// <summary>
			/// Direction
			/// </summary>
			[DxfCodeValue(11, 21, 31)]
			public XYZ Direction { get; set; }

			/// <summary>
			/// Gets a list of <see cref="StartEndPointPair" />.
			/// </summary>
			public IList<StartEndPointPair> BreakStartEndPointsPairs { get; private set; } = new List<StartEndPointPair>();

			/// <summary>
			/// Leader index
			/// </summary>
			[DxfCodeValue(90)]
			public int LeaderIndex { get; set; }

			/// <summary>
			/// Landing distance
			/// </summary>
			[DxfCodeValue(40)]
			public double LandingDistance { get; set; }

			/// <summary>
			/// Gets a list of <see cref="LeaderLine"/> objects representing
			/// leader lines starting from the landing point
			/// of the multi leader.
			/// </summary>
			public IList<LeaderLine> Lines { get; private set; } = new List<LeaderLine>();

			//R2010
			/// <summary>
			/// Attachment direction
			/// </summary>
			[DxfCodeValue(271)]
			public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

			public object Clone()
			{
				LeaderRoot clone = (LeaderRoot)base.MemberwiseClone();

				clone.BreakStartEndPointsPairs = new List<StartEndPointPair>();
				foreach (var breakStartEndPoint in this.BreakStartEndPointsPairs)
				{
					clone.BreakStartEndPointsPairs.Add((StartEndPointPair)breakStartEndPoint.Clone());
				}

				clone.Lines = new List<LeaderLine>();
				foreach (var line in this.Lines)
				{
					clone.Lines.Add((LeaderLine)line.Clone());
				}

				return clone;
			}
		}

		/// <summary>
		/// Start/end point pairs
		/// 3BD	11	Start Point
		/// 3BD	12	End point
		/// </summary>
		public struct StartEndPointPair : ICloneable
		{
			public StartEndPointPair(XYZ startPoint, XYZ endPoint) {
				StartPoint = startPoint;
				EndPoint = endPoint;
			}

			/// <summary>
			/// Break start point
			/// </summary>
			[DxfCodeValue(12, 22, 32)]
			public XYZ StartPoint { get; private set; }

			/// <summary>
			/// Break end point
			/// </summary>
			[DxfCodeValue(13, 23, 33)]
			public XYZ EndPoint { get; private set; }

			public object Clone()
			{
				return this.MemberwiseClone();
			}
		}


		/// <summary>
		///	Represents a leader line
		/// </summary>
		/// <remarks>
		/// Appears as 304	DXF: “LEADER_LINE“
		/// </remarks>
		public class LeaderLine : ICloneable
		{
			internal CadDocument Document { get; set; }
			private LineType _lineType;

			public LeaderLine() { }

			/// <summary>
			/// Get the list of points of this <see cref="LeaderLine"/>.
			/// </summary>
			public IList<XYZ> Points { get; private set; } = new List<XYZ>();

			/// <summary>
			/// Break info count
			/// </summary>
			public int BreakInfoCount { get; set; }

			/// <summary>
			/// Segment index
			/// </summary>
			[DxfCodeValue(90)]
			public int SegmentIndex { get; set; }

			/// <summary>
			/// Start/end point pairs
			/// </summary>
			public IList<StartEndPointPair> StartEndPoints { get; private set; } = new List<StartEndPointPair>();

			/// <summary>
			/// Leader line index.
			/// </summary>
			[DxfCodeValue(91)]
			public int Index { get; set; }

			//R2010
			/// <summary>
			/// Leader type
			/// </summary>
			[DxfCodeValue(170)]
			public MultiLeaderPathType PathType { get; set; }

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
			/// Arrowhead size
			/// </summary>
			[DxfCodeValue(40)]
			public double ArrowheadSize { get; set; }

			/// <summary>
			/// Gets or sets a <see cref="BlockRecord"/> containig elements
			/// to be dawn as arrow symbol.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Handle, 341)]
			public BlockRecord Arrowhead { get; set; }

			/// <summary>
			/// Override flags
			/// </summary>
			[DxfCodeValue(93)]
			public LeaderLinePropertOverrideFlags OverrideFlags { get; set; }

			public object Clone()
			{
				LeaderLine clone = (LeaderLine)this.MemberwiseClone();

				clone.LineType = (LineType)this.LineType?.Clone();
				clone.Arrowhead = (BlockRecord)this.Arrowhead?.Clone();

				clone.Points = new List<XYZ>();
				foreach (var point in this.Points)
				{
					clone.Points.Add(point);
				}

				clone.StartEndPoints = new List<StartEndPointPair>();
				foreach (var startEndPoint in this.StartEndPoints)
				{
					clone.StartEndPoints.Add((StartEndPointPair)startEndPoint.Clone());
				}

				return clone;
			}

			public void AssignDocument(CadDocument doc)
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

			public void UassignDocument()
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
}