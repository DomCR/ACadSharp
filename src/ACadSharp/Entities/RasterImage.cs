using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.IO;
using ACadSharp.Objects;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="RasterImage"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityImage"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.RasterImage"/>
/// </remarks>
[DxfName(DxfFileToken.EntityImage)]
[DxfSubClass(DxfSubclassMarker.RasterImage)]
public class RasterImage : CadWipeoutBase, IDxfClassDefined
{
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

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityImage;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.RasterImage;

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

	/// <inheritdoc/>
	public override bool IsValid(CadFileFormat format, ACadVersion version, out IList<string> errors)
	{
		var result = base.IsValid(format, version, out errors);

		if (this.DefinitionReactor == null)
		{
			errors.Add($"The {nameof(this.DefinitionReactor)} property is null.");
			result = false;
		}

		if (this.Definition == null)
		{
			errors.Add($"The {nameof(this.Definition)} property is null.");
			result = false;
		}

		return result;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			ApplicationName = "ISM",
			CppClassName = DxfSubclassMarker.RasterImage,
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.EntityImage,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
			WasZombie = false,
		};
	}
}