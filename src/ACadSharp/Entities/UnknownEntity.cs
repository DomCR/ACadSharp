using ACadSharp.Classes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Class that holds the basic information for an unknown <see cref="Entity"/>.
	/// </summary>
	/// <remarks>
	/// Unknown entities may appear in the <see cref="CadDocument"/> if the cad file contains proxies or entities not yet supported by ACadSharp.
	/// </remarks>
	public class UnknownEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName
		{
			get
			{
				if (this.DxfClass == null)
				{
					return "UNKNOWN";
				}
				else
				{
					return this.DxfClass.DxfName;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker
		{
			get
			{
				if (this.DxfClass == null)
				{
					return DxfSubclassMarker.Entity;
				}
				else
				{
					return this.DxfClass.CppClassName;
				}
			}
		}

		/// <summary>
		/// Remote text (Express Tools RTEXT) data, when this unknown entity is one and could be read.
		/// </summary>
		public RemoteText RText { get; set; }

		/// <summary>
		/// Data of an Express Tools RTEXT object: text from a file or from a DIESEL expression.
		/// </summary>
		public class RemoteText
		{
			/// <summary>Insertion point.</summary>
			public XYZ InsertPoint { get; set; }
			/// <summary>Extrusion direction.</summary>
			public XYZ Normal { get; set; } = XYZ.AxisZ;
			/// <summary>Rotation, radians.</summary>
			public double Rotation { get; set; }
			/// <summary>Text height.</summary>
			public double Height { get; set; }
			/// <summary>Flags: 1 = contents is a DIESEL expression, otherwise a file name.</summary>
			public short Flags { get; set; }
			/// <summary>DIESEL expression or file name.</summary>
			public string Contents { get; set; }
			/// <summary>Handle of the text style.</summary>
			public ulong StyleHandle { get; set; }
		}

		/// <summary>
		/// Dxf class linked to this entity.
		/// </summary>
		public DxfClass DxfClass { get; }

		internal UnknownEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// An Unknown Entity does not have any geometric shape, therfore it's bounding box will be always 0
		/// </remarks>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}
