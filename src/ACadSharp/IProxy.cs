using ACadSharp.Classes;

namespace ACadSharp
{
	public interface IProxy
	{
		/// <summary>
		/// Application object's class ID.
		/// </summary>
		int ClassId { get; }

		/// <summary>
		/// Application object's class.
		/// </summary>
		DxfClass DxfClass { get; set; }

		/// <summary>
		/// Proxy object class ID.
		/// </summary>
		int ProxyClassId { get; }

		/// <summary>
		/// Original custom object data format.
		/// </summary>
		bool OriginalDataFormatDxf { get; set; }
	}
}