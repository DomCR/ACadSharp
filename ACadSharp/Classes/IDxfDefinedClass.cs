namespace ACadSharp.Classes
{
	/// <summary>
	/// Object holds information for application-defined dxf class
	/// </summary>
	public interface IDxfDefinedClass
	{
		/// <summary>
		/// Gets the dxf class definition for this object
		/// </summary>
		/// <returns></returns>
		DxfClass GetDxfClass();
	}
}
