using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Classes
{
	public class DxfClass
	{
		/// <summary>
		/// C++ class name. Used to bind with software that defines object class behavior; always unique
		/// </summary>
		public string CppClassName { get; set; }
		/// <summary>
		/// Application name. Posted in Alert box when a class definition listed in this section is not currently loaded
		/// </summary>
		public string ApplicationName { get; set; }
		/// <summary>
		/// Proxy capabilities flag. Bit-coded value that indicates the capabilities of this object as a proxy
		/// </summary>
		public ProxyFlags ProxyFlags { get; set; }
		/// <summary>
		/// Class DXF record name; always unique
		/// </summary>
		public string DxfName { get; set; }
		/// <summary>
		/// Instance count for a custom class
		/// </summary>
		public int InstanceCount { get; set; }
		/// <summary>
		/// Was-a-proxy flag.Set to 1 if class was not loaded when this DXF file was created, and 0 otherwise
		/// </summary>
		public bool WasAProxy { get; set; }
		/// <summary>
		/// Is-an-entity flag.Set to 1 if class was derived from the AcDbEntity class and can reside in the BLOCKS or ENTITIES section.If 0, instances may appear only in the OBJECTS section
		/// </summary>
		public bool IsAnEntity { get; set; }

		public short ClassNumber { get; set; }
		public short ItemClassId { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{DxfName}:{ClassNumber}";
		}
	}
}
