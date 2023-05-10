using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="MLStyle"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectMLStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLineStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectMLStyle)]
	[DxfSubClass(DxfSubclassMarker.MLineStyle)]
	public partial class MLStyle : CadObject, INamedCadObject
	{
		public override ObjectType ObjectType => ObjectType.MLINESTYLE;

		public override string ObjectName => DxfFileToken.ObjectMLStyle;

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
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double StartAngle { get; set; } = System.Math.PI / 2;

		/// <summary>
		/// End angle
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 52)]
		public double EndAngle { get; set; } = System.Math.PI / 2;

		//71	Number of elements
		public List<MLStyle.Element> Elements { get; } = new List<Element>();
	}
}
