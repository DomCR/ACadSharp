using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Extensions;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="LineType"/> entry.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableLinetype"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Linetype"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableLinetype)]
	[DxfSubClass(DxfSubclassMarker.Linetype)]
	public partial class LineType : TableEntry
	{
		public static LineType ByBlock { get { return new LineType("ByBlock"); } }

		public static LineType ByLayer { get { return new LineType("ByLayer"); } }

		public static LineType Continuous { get { return new LineType("Continuous"); } }

		/// <summary>
		/// Alignment code.
		/// </summary>
		/// <value>
		/// value is always 65, the ASCII code for A.
		/// </value>
		[DxfCodeValue(72)]
		public char Alignment { get; internal set; } = 'A';

		/// <summary>
		/// Descriptive text for line type.
		/// </summary>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		public bool IsComplex { get { return this._segments.Count > 0; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLinetype;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LTYPE;

		/// <summary>
		/// Total pattern length.
		/// </summary>
		[DxfCodeValue(40)]
		public double PatternLen
		{
			get
			{
				return this.Segments.Sum(s => Math.Abs(s.Length));
			}
		}

		/// <summary>
		/// LineType Segments
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		public IEnumerable<Segment> Segments { get { return this._segments; } }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Linetype;

		public const string ByBlockName = "ByBlock";

		public const string ByLayerName = "ByLayer";

		public const string ContinuousName = "Continuous";

		private List<Segment> _segments = new List<Segment>();

		/// <inheritdoc/>
		public LineType(string name) : base(name) { }

		internal LineType() : base()
		{
		}

		/// <summary>
		/// Add a segment to this line type.
		/// </summary>
		/// <param name="segment"></param>
		/// <exception cref="ArgumentException"></exception>
		public void AddSegment(Segment segment)
		{
			if (segment.Owner != null)
				throw new ArgumentException($"Segment already assigned to a LineType: {segment.Owner.Name}");

			segment.Style = CadObject.updateCollection(segment.Style, this.Document?.TextStyles);
			segment.Owner = this;
			this._segments.Add(segment);
		}

		/// <summary>
		/// Converts a collection of <see cref="IVector"/> to a series of <see cref="Polyline3D"/> in the line type shape.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public IEnumerable<Polyline3D> CreateLineTypeShape(params IEnumerable<IVector> points)
		{
			return this.CreateLineTypeShape(null, points);
		}

		/// <summary>
		/// Converts a collection of <see cref="IVector"/> to a series of <see cref="Polyline3D"/> in the line type shape.
		/// </summary>
		/// <param name="pointSize"></param>
		/// <param name="points"></param>
		/// <returns></returns>
		public IEnumerable<Polyline3D> CreateLineTypeShape(double? pointSize, params IEnumerable<IVector> points)
		{
			if (!points.Any() || points.Count() < 2)
			{
				throw new ArgumentException("The list must contain at least 2 points to create the shape.");
			}

			return this.CreateLineTypeShape(new Polyline3D(points.Select(v => v.Convert<XYZ>())), pointSize);
		}

		/// <summary>
		/// Converts a <see cref="IPolyline"/> to a series of <see cref="Polyline3D"/> in line type shape.
		/// </summary>
		/// <param name="polyline"></param>
		/// <param name="pointSize"></param>
		/// <returns></returns>
		public IEnumerable<Polyline3D> CreateLineTypeShape(IPolyline polyline, double? pointSize = null)
		{
			if (!pointSize.HasValue)
			{
				pointSize = polyline.GetActiveLineWeightType().GetLineWeightValue();
			}

			var lst = new List<Polyline3D>();
			if (!this.IsComplex)
			{
				lst.Add(new Polyline3D(polyline.GetPoints<XYZ>(), polyline.IsClosed));
				return lst;
			}

			var pts = polyline.GetPoints<XYZ>().ToArray();
			XYZ current = pts[0];
			for (int i = 1; i < pts.Length; i++)
			{
				XYZ next = pts[i];
				lst.AddRange(this.createSegmentShape(current, next, pointSize.Value));
				current = next;
			}

			if (polyline.IsClosed)
			{
				lst.AddRange(this.createSegmentShape(current, pts[0], pointSize.Value));
			}

			return lst;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			LineType clone = (LineType)base.Clone();

			clone._segments = new List<Segment>();
			foreach (var segment in this._segments)
			{
				clone.AddSegment(segment.Clone());
			}

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			foreach (var item in this._segments.Where(s => s.Style != null))
			{
				item.AssignDocument(doc);
			}

			doc.TextStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.TextStyles.OnRemove -= this.tableOnRemove;

			foreach (var item in this._segments.Where(s => s.Style != null))
			{
				item.UnassignDocument();
			}

			base.UnassignDocument();
		}

		protected void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is TextStyle style)
			{
				foreach (var item in this._segments.Where(s => s.Style != null))
				{
					if (item.Style == style)
					{
						item.Style = null;
					}
				}
			}
		}

		private List<Polyline3D> createSegmentShape(XYZ start, XYZ end, double pointSize)
		{
			List<Polyline3D> lst = new List<Polyline3D>();
			Polyline3D current = new(start);

			double dist = start.DistanceFrom(end);
			XYZ next = start;
			int nSegments = (int)Math.Floor(dist / this.PatternLen);
			XYZ v = (end - start).Normalize();

			while ((double)dist > 0)
			{
				foreach (var item in this.Segments)
				{
					if (item.Length < (double)dist)
					{
						next += v * Math.Abs(item.Length);
						dist -= Math.Abs(item.Length);
					}
					else
					{
						next += v * Math.Abs((double)dist);
						dist -= Math.Abs((double)dist);
					}

					if (item.IsPoint)
					{
						Polyline3D pl = new Polyline3D(start, next + v * pointSize);
						lst.Add(pl);

						if (current.Vertices.Any())
						{
							lst.Add(current);
							current = new Polyline3D();
						}
					}
					else if (item.IsLine)
					{
						current.Vertices.Add(new Vertex3D(next));
					}
					else if (item.IsSpace)
					{
						if (current.Vertices.Any())
						{
							lst.Add(current);
						}

						current = new Polyline3D(next);
					}

					start = next;

					if ((double)dist <= 0)
					{
						if (current.Vertices.Any())
						{
							lst.Add(current);
						}

						break;
					}
				}
			}

			return lst;
		}
	}
}