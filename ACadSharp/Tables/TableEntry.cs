using ACadSharp.Attributes;
using System;

namespace ACadSharp.Tables
{
	[DxfSubClass(DxfSubclassMarker.TableRecord, true)]
	public abstract class TableEntry : CadObject, INamedCadObject
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

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public abstract TableEntry Clone();

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.Name}";
		}

		protected override void mapClone(CadObject clone)
		{
			base.mapClone(clone);

			TableEntry te = clone as TableEntry;

			te.Name = this.Name;
			te.Flags = this.Flags;
		}
	}
}
