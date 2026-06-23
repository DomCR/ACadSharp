namespace ACadSharp.Objects;

/// <summary>
/// Shader chosen to evaluate the illumination of a <see cref="Material"/>.
/// Mirrors the ODA-side <c>OdGiMaterialTraits::IlluminationModel</c> enum.
/// </summary>
public enum MaterialIlluminationModel
{
	BlinnShader = 0,
	MetalShader = 1,
}
