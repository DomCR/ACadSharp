namespace ACadSharp.Classes;

/// <summary>
/// Defines a contract for classes that can provide a <see cref="DxfClass"/> instance.
/// </summary>
public interface IDxfClassDefined
{
	/// <summary>
	/// Gets the <see cref="DxfClass"/> instance associated with this class.
	/// </summary>
	/// <returns>The <see cref="DxfClass"/> instance associated with this class.</returns>
	public DxfClass GetDxfClass();
}
