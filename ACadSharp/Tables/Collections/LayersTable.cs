using ACadSharp.IO.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables.Collections
{
	public class LayersTable : Table<Layer>
	{
		public override ObjectType ObjectType => ObjectType.LAYER_CONTROL_OBJ;

		public LayersTable(CadDocument document) : base(document) { }
	}
}