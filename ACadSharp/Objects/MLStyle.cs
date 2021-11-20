using ACadSharp.Attributes;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Flags (bit-coded).
	/// </summary>
	[Flags]
	public enum MLineStyleFlags
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Fill on
		/// </summary>
		FillOn = 1,

		/// <summary>
		/// Display miters at the joints (inner vertexes)
		/// </summary>
		DisplayJoints = 2,

		/// <summary>
		/// Start square (line) cap
		/// </summary>
		StartSquareCap = 16,

		/// <summary>
		/// Start inner arcs cap
		/// </summary>
		StartInnerArcsCap = 32,

		/// <summary>
		/// Start round (outer arcs) cap
		/// </summary>
		StartRoundCap = 64,

		/// <summary>
		/// End square (line) cap
		/// </summary>
		EndSquareCap = 256,

		/// <summary>
		/// End inner arcs cap
		/// </summary>
		EndInnerArcsCap = 512,

		/// <summary>
		/// End round (outer arcs) cap
		/// </summary>
		EndRoundCap = 1024
	}

	public partial class MLStyle : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.MLINESTYLE;

		public override string ObjectName => DxfFileToken.TableMLStyle;

		//Subclass marker(AcDbMlineStyle)

		/// <summary>
		/// MLStyle flags
		/// </summary>
		[DxfCodeValue(70)]
		public MLineStyleFlags Flags { get; set; }

		/// <summary>
		/// Style description
		/// </summary>
		/// <value>
		/// 255 characters maximum
		/// </value>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		//62	Fill color(integer, default = 256)
		[DxfCodeValue(62)]
		public Color FillColor { get; set; } = Color.ByLayer;

		/// <summary>
		/// Start angle
		/// </summary>
		[DxfCodeValue(51)]
		public double StartAngle { get; set; } = 90;

		/// <summary>
		/// End angle
		/// </summary>
		[DxfCodeValue(52)]
		public double EndAngle { get; set; } = 90;

		//71	Number of elements

		public List<MLStyle.Element> Elements { get; } = new List<Element>();
	}
}
