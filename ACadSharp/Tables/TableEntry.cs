using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	[DxfSubClass(DxfSubclassMarker.TableRecord, true)]
	public abstract class TableEntry : CadObject, INamedCadObject, ICloneable
	{
		/// <summary>
		/// Specifies the name of the object
		/// </summary>
		[DxfCodeValue(2)]
		public string Name { get; set; }

		/// <summary>
		/// Standard flags
		/// </summary>
		[DxfCodeValue(70)]
		public StandardFlags Flags { get; set; }

		internal TableEntry() { }

		public TableEntry(string name)
		{
			this.Name = name;
		}

		/// <inheritdoc/>
		public object Clone()
		{
			var clone = Activator.CreateInstance(this.GetType(), this.Name);

			this.createCopy(clone as CadObject);

			return clone;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.Name}";
		}

		protected override void createCopy(CadObject copy)
		{
			base.createCopy(copy);

			TableEntry te = copy as TableEntry;

			te.Name = this.Name;
			te.Flags = this.Flags;
		}
	}
}
