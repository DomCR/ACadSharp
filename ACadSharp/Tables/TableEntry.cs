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
		/// Standard flags
		/// </summary>
		[DxfCodeValue(70)]
		public StandardFlags Flags { get; set; }

		internal TableEntry()
		{
		}

		public TableEntry(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return $"{ObjectName}:{Name}";
		}
	}
}
