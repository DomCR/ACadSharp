using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Material channel-enable flags. Each bit toggles whether the renderer
	/// considers the corresponding channel data on the <see cref="Material"/>.
	/// Mirrors the ODA-side <c>OdGiMaterialTraits::ChannelFlags</c> enum.
	/// </summary>
	[Flags]
	public enum MaterialChannelFlags
	{
		None = 0,
		UseDiffuse = 0x01,
		UseSpecular = 0x02,
		UseReflection = 0x04,
		UseOpacity = 0x08,
		UseBump = 0x10,
		UseRefraction = 0x20,
		UseNormalMap = 0x40,
		UseAll = UseDiffuse | UseSpecular | UseReflection | UseOpacity | UseBump | UseRefraction | UseNormalMap,
	}
}
