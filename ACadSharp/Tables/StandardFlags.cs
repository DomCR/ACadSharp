using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Standard flags for tables
	/// </summary>
	[Flags]
	public enum StandardFlags : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// If set, table entry is externally dependent on an xRef
		/// </summary>
		XrefDependent = 16,

		/// <summary>
		/// If both this bit and bit 16 are set, the externally dependent xRef has been successfully resolved
		/// </summary>
		XrefResolved = 32,

		/// <summary>
		/// If set, the table entry was referenced by at least one entity in the drawing the last time the 
		/// drawing was edited. (This flag is for the benefit of AutoCAD commands. It can be ignored by 
		/// most programs that read DXF files and need not be set by programs that write DXF files)
		/// </summary>
		Referenced = 64,
	}
}
