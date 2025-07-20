using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;

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
	public class PdfUnderlay : UnderlayEntity<PdfUnderlayDefinition>
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPdfUnderlay;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public PdfUnderlay(PdfUnderlayDefinition definition) : base(definition) { }

		internal PdfUnderlay()
		{
		}

		protected override ObjectDictionaryCollection<PdfUnderlayDefinition> getDocumentCollection(CadDocument document)
		{
			return document.PdfDefinitions;
		}
	}
}