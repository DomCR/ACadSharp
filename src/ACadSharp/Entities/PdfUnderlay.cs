using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="PdfUnderlay"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityPdfUnderlay"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Underlay"/>
/// </remarks>
[DxfName(DxfFileToken.EntityPdfUnderlay)]
[DxfSubClass(DxfSubclassMarker.Underlay)]
public class PdfUnderlay : UnderlayEntity<PdfUnderlayDefinition>, IDxfClassDefined
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

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.PdfReference,
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityPdfUnderlay,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		};
	}

	protected override ObjectDictionaryCollection<PdfUnderlayDefinition> getDocumentCollection(CadDocument document)
	{
		return document?.PdfDefinitions;
	}
}