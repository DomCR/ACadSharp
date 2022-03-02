#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion

namespace ACadSharp
{
	/// <summary>
	/// Type of dxf reference
	/// </summary>
	public enum DxfReferenceType
	{
		/// <summary>
		/// No reference, the value is a primitive
		/// </summary>
		None,

		/// <summary>
		/// Handle reference, the value is a handle pointing to an object
		/// </summary>
		Handle,

		/// <summary>
		/// Name reference, the value is a name pointing to an object 
		/// </summary>
		Name,

		/// <summary>
		/// Counter reference, the value is a list with multiple elements referenced to it
		/// </summary>
		Count
	}
}
