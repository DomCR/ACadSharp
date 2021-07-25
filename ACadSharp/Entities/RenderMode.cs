#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
namespace ACadSharp.Entities
{
	/// <summary>
	/// Viewport render mode.
	/// </summary>
	public enum RenderMode : byte
	{
		/// <summary>
		/// Classic 2D
		/// </summary>
		Optimized2D,
		/// <summary>
		/// Wire frame.
		/// </summary>
		Wireframe,
		/// <summary>
		/// Hidden line.
		/// </summary>
		HiddenLine,
		/// <summary>
		/// Flat shaded.
		/// </summary>
		FlatShaded,
		/// <summary>
		/// Gouraud shaded.
		/// </summary>
		GouraudShaded,
		/// <summary>
		/// Flat shaded with wire frame.
		/// </summary>
		FlatShadedWithWireframe,
		/// <summary>
		/// Gouraud shaded with wireframe.
		/// </summary>
		GouraudShadedWithWireframe,
	}
}
