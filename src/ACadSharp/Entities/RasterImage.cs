using ACadSharp.Attributes;
using ACadSharp.Objects;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="RasterImage"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityImage"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterImage"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityImage)]
	[DxfSubClass(DxfSubclassMarker.RasterImage)]
	public class RasterImage : CadWipeoutBase
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityImage;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImage;

		/// <inheritdoc/>
		public override ImageDefinition Definition
		{
			get
			{
				return base.Definition;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				base.Definition = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RasterImage" /> class.
		/// </summary>
		/// <param name="definition"></param>
		public RasterImage(ImageDefinition definition)
		{
			this.Definition = definition;
		}

		internal RasterImage() : base()
		{
		}
	}
}