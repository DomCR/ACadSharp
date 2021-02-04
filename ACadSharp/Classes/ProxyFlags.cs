using System;

namespace ACadSharp.Classes
{
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-DBD5351C-E408-4CED-9336-3BD489179EF5

	[Flags]
	public enum ProxyFlags : ushort
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// Erase allowed.
		/// </summary>
		EraseAllowed = 1,
		/// <summary>
		/// Transform allowed.
		/// </summary>
		TransformAllowed = 2,
		/// <summary>
		/// Clor change allowed.
		/// </summary>
		ColorChangeAllowed = 4,
		/// <summary>
		/// Layer change allowed.
		/// </summary>
		LayerChangeAllowed = 8,
		/// <summary>
		/// Line type change allowed.
		/// </summary>
		LinetypeChangeAllowed = 16,
		/// <summary>
		/// Line type scale change allowed.
		/// </summary>
		LinetypeScaleChangeAllowed = 32,
		/// <summary>
		/// Visibility change allowed.
		/// </summary>
		VisibilityChangeAllowed = 64,
		/// <summary>
		/// Cloning allowed.
		/// </summary>
		CloningAllowed = 128,
		/// <summary>
		/// Line weight change allowed.
		/// </summary>
		LineweightChangeAllowed = 256,
		/// <summary>
		/// Plot Style Name change allowed.
		/// </summary>
		PlotStyleNameChangeAllowed = 512,
		/// <summary>
		/// All operations except cloning allowed.
		/// </summary>
		AllOperationsExceptCloningAllowed = 895,
		/// <summary>
		/// All operations allowed.
		/// </summary>
		AllOperationsAllowed = 1023,
		/// <summary>
		/// Disables proxy warning dialog.
		/// </summary>
		DisablesProxyWarningDialog = 1024,
		/// <summary>
		/// R13 format proxy.
		/// </summary>
		R13FormatProxy = 32768,
	}
}
