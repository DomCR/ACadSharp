﻿using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="PdfUnderlay"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPdfUnderlay"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Underlay"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPdfUnderlay)]
	[DxfSubClass(DxfSubclassMarker.Underlay)]
	public class PdfUnderlay : UnderlayEntity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPdfUnderlay;
	}
}
