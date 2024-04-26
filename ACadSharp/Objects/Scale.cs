using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Scale"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectScale"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Scale"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectScale)]
	[DxfSubClass(DxfSubclassMarker.Scale)]
	public class Scale : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectScale;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Scale;

		/// <summary>
		/// Name
		/// </summary>
		[DxfCodeValue(300)]
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
		/// Group description.
		/// </summary>
		[DxfCodeValue(140)]
		public double PaperUnits { get; set; }

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(141)]
		public double DrawingUnits { get; set; }

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(290)]
		public bool IsUnitScale { get; set; }
	}
}
