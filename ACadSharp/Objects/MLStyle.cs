using ACadSharp.Attributes;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	public partial class MLStyle : CadObject
	{
		public override ObjectType ObjectType => ObjectType.MLINESTYLE;

		public override string ObjectName => DxfFileToken.TableMLStyle;

		//Subclass marker(AcDbMlineStyle)

		/// <summary>
		/// Mline style name
		/// </summary>
		[DxfCodeValue(2)]
		public string Name { get; set; }

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
