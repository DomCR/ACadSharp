using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="MLineStyle"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectMLineStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLineStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectMLineStyle)]
	[DxfSubClass(DxfSubclassMarker.MLineStyle)]
	public partial class MLineStyle : NonGraphicalObject
	{
		/// <summary>
		/// Default multiline style name
		/// </summary>
		public const string DefaultName = "Standard";

		/// <summary>
		/// Gets the default MLine style
		/// </summary>
		public static MLineStyle Default { get { return new MLineStyle(DefaultName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MLINESTYLE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectMLineStyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLineStyle;

		/// <summary>
		/// Mline style name
		/// </summary>
		[DxfCodeValue(2)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		/// <summary>
		/// Multi line style flags
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

		/// <summary>
		/// Fill color
		/// </summary>
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

		/// <summary>
		/// Elements in the style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 71)]
		public List<MLineStyle.Element> Elements { get; } = new List<Element>();

		internal MLineStyle() { }

		public MLineStyle(string name)
		{
			this.Name = name;
		}
	}
}
