using ACadSharp.Attributes;
using ACadSharp.Extensions;
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
				throw new ArgumentException($"Segment has already a LineType: {segment.Owner.Name}");

			segment.Style = CadObject.updateCollection(segment.Style, this.Document?.TextStyles);
			segment.Owner = this;
			this._segments.Add(segment);
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
	}
}