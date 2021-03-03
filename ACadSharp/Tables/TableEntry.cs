using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-8427DD38-7B1F-4B7F-BF66-21ADD1F41295
	public abstract class TableEntry : CadObject
	{
		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.SymbolTableName)]
		public string Name { get; set; }
		/// <summary>
		/// Table entry is xref dependent.
		/// </summary>
		public virtual bool XrefDependant { get; set; }

		public TableEntry() { }
		public TableEntry(string name)
		{
			Name = name;
		}
		internal TableEntry(DxfEntryTemplate template)
		{
			//TableName = template.TableName;
			Name = template.Name;
			Handle = template.Handle;
			OwnerHandle = template.OwnerHandle;
		}

		public override string ToString()
		{
			return $"{ObjectName}:{Name}";
		}
	}
}
