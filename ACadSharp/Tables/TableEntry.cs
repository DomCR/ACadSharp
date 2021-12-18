using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public abstract class TableEntry : CadObject
	{
		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(2)]
		public virtual string Name { get; set; }

		/// <summary>
		/// Table entry is xref dependent.
		/// </summary>
		[Obsolete]
		public virtual bool XrefDependant { get; set; }

		/// <summary>
		/// Standard flags
		/// </summary>
		[DxfCodeValue(70)]
		public StandardFlags Flags { get; set; }

		public TableEntry()
		{
			this.Name = string.Empty;
		}

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
