﻿using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="LineType"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableLinetype"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Linetype"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableLinetype)]
	[DxfSubClass(DxfSubclassMarker.Linetype)]
	public partial class LineType : TableEntry
	{
		public const string ByLayerName = "ByLayer";

		public const string ByBlockName = "ByBlock";

		public const string ContinuousName = "Continuous";

		public static LineType ByLayer { get { return new LineType("ByLayer"); } }

		public static LineType ByBlock { get { return new LineType("ByBlock"); } }

		public static LineType Continuous { get { return new LineType("Continuous"); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LTYPE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLinetype;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Linetype;

		/// <summary>
		/// Descriptive text for linetype
		/// </summary>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Total pattern length
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
		/// Alignment code
		/// </summary>
		/// <value>
		/// value is always 65, the ASCII code for A
		/// </value>
		[DxfCodeValue(72)]
		public char Alignment { get; internal set; } = 'A';

		/// <summary>
		/// Linetype Segments
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		public IEnumerable<Segment> Segments { get { return this._segments; } }

		///// <summary>
		///// Pointer to STYLE object (one per element if code 74 > 0)
		///// </summary>
		//[DxfCodeValue(DxfReferenceType.Handle, 340)]
		//public TextStyle Style { get; set; }

		private List<Segment> _segments = new List<Segment>();

		public LineType() : base() { }

		public LineType(string name) : base(name) { }

		public void AddSegment(Segment segment)
		{
			if (segment.LineType != null)
				throw new ArgumentException($"Segment has already a Linetype: {segment.LineType.Name}");

			segment.LineType = this;
			this._segments.Add(segment);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			LineType clone = new LineType(this.Name);

			clone._segments.Clear();
			foreach (var segment in this._segments)
			{
				clone.AddSegment(segment.Clone());
			}

			return clone;
		}
	}
}